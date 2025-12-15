from flask import Flask, request, jsonify
from datetime import datetime

app = Flask(__name__)

active_games = {
    "1": {  
        "players": [],
        "player_status": {},
        "ships_data": {},
        "game_history": [],
        "boards": {},
        "hits": {},
        "shots": {},
        "current_turn": None,
        "created_at": datetime.now().isoformat()
    }
}

next_game_id = 2  

def save_game_event(game_id, event_type, player=None, details=None):
    """Сохранить событие в истории игры (в памяти)"""
    if game_id in active_games:
        active_games[game_id]["game_history"].append({
            "timestamp": datetime.now().isoformat(),
            "type": event_type,
            "player": player,
            "details": details
        })

def get_or_create_game(game_id=None):
    """Получить игру по ID или создать новую"""
    if game_id is None:
        game_id = "1"  
    
    if game_id not in active_games:
        active_games[game_id] = {
            "players": [],
            "player_status": {},
            "ships_data": {},
            "game_history": [],
            "boards": {},
            "hits": {},
            "shots": {},
            "current_turn": None,
            "created_at": datetime.now().isoformat()
        }
    
    return game_id, active_games[game_id]

def create_new_game():
    """Создать новую игру"""
    global next_game_id
    game_id = str(next_game_id)
    next_game_id += 1
    
    active_games[game_id] = {
        "players": [],
        "player_status": {},
        "ships_data": {},
        "game_history": [],
        "boards": {},
        "hits": {},
        "shots": {},
        "current_turn": None,
        "created_at": datetime.now().isoformat()
    }
    
    return game_id

@app.route("/connect", methods=["POST"])
def connect():
    name = request.form.get("name")
    game_id = request.form.get("game_id", "1")  

    if not name:
        return jsonify({"success": False, "message": "Имя не может быть пустым"}), 400

    game_id, game = get_or_create_game(game_id)

    if name in game["players"]:
        return jsonify({"success": False, "message": "Игрок с таким именем уже подключен к этой игре"}), 400

    if len(game["players"]) >= 2:
        new_game_id = create_new_game()
        
        new_game = active_games[new_game_id]
        new_game["players"].append(name)
        new_game["player_status"][name] = "waiting"
        
        save_game_event(new_game_id, "player_connect", name)
        
        return jsonify({
            "success": True, 
            "game_ready": False,
            "game_id": new_game_id,
            "message": f"Создана новая игра (ID: {new_game_id}). Ожидаем второго игрока..."
        })

    game["players"].append(name)
    game["player_status"][name] = "waiting"
    save_game_event(game_id, "player_connect", name)

    if len(game["players"]) == 2:
        for p in game["players"]:
            game["player_status"][p] = "placing_ships"
        save_game_event(game_id, "game_start", None)

    return jsonify({
        "success": True, 
        "game_ready": len(game["players"]) == 2,
        "game_id": game_id
    })

@app.route("/state")
def state():
    game_id = request.args.get("game_id", "1")
    
    if game_id not in active_games:
        return jsonify({"success": False, "message": "Игра не найдена"}), 404
    
    game = active_games[game_id]
    
    shots_for_json = {}
    for player in game.get("shots", {}):
        shots_for_json[player] = game["shots"][player]
    
    turn_info = game.get("current_turn")
    players_list = game.get("players", [])
    
    if not players_list or len(players_list) < 2:
        turn_info = None
    ready_players = [
        p for p in game["players"]
        if game["player_status"].get(p) == "ready"
    ]

    if (
        len(game["players"]) == 2 and
        len(ready_players) == 2 and
        game["current_turn"] is None
    ):
        print(f"[SERVER] Автостарт боя в игре {game_id}")

        for player in game["players"]:
            cells = []
            for ship in game["ships_data"].get(player, []):
                for coord in ship:
                    cells.append([coord[0], coord[1]])

            game["boards"][player] = cells
            game["hits"][player] = []
            game["shots"][player] = []

        game["current_turn"] = game["players"][0]

        for p in game["players"]:
            game["player_status"][p] = "battle"

    
    return jsonify({
        "success": True,
        "game_id": game_id,
        "players": players_list,
        "connected": len(players_list),
        "game_ready": len(players_list) == 2,
         "game_started": game.get("current_turn") is not None,  
        "player_status": game.get("player_status", {}),
        "current_turn": turn_info,
        "shots": shots_for_json
    })

@app.route("/games")
def list_games():
    """Получить список всех активных игр"""
    games_list = []
    
    for gid, game in active_games.items():
        games_list.append({
            "game_id": gid,
            "players": game.get("players", []),
            "connected": len(game.get("players", [])),
            "status": "waiting" if len(game.get("players", [])) < 2 else "ready",
            "created_at": game.get("created_at", "")
        })
    
    return jsonify({
        "success": True,
        "games": games_list,
        "total_games": len(games_list)
    })

@app.route("/ships", methods=["POST"])
def ships():
    try:
        game_id = request.args.get("game_id", "1")
        
        if not request.is_json:
            return jsonify({"success": False, "message": "Неверный формат данных"}), 400
            
        data = request.get_json()
        
        name = data.get("name")
        ships_list = data.get("ships")

        if not name:
            return jsonify({"success": False, "message": "Не указано имя"}), 400
            
        if ships_list is None:
            return jsonify({"success": False, "message": "Не указаны корабли"}), 400

        if game_id not in active_games:
            return jsonify({"success": False, "message": "Игра не найдена"}), 404

        game = active_games[game_id]

        if name not in game["players"]:
            return jsonify({"success": False, "message": "Игрок не найден в этой игре"}), 404

        print(f"[SERVER] Игрок {name} в игре {game_id} отправил корабли: {len(ships_list)} кораблей")
        
        game["ships_data"][name] = ships_list
        game["player_status"][name] = "ready"

        ready_players = [p for p in game["players"] if game["player_status"].get(p) == "ready"]
        
        if len(game["players"]) == 2 and len(ready_players) == 2:
            print(f"[SERVER] Оба игрока готовы в игре {game_id}! Начинаем игру.")

            for player in game["players"]:
                cells = []
                ship_list = game["ships_data"].get(player, [])

                for ship in ship_list:
                    for coord in ship:
                        if len(coord) >= 2:
                            cells.append([coord[0], coord[1]])  
                
                game["boards"][player] = cells  
                game["hits"][player] = [] 
                game["shots"][player] = []  

            game["current_turn"] = game["players"][0]

            for p in game["players"]:
                game["player_status"][p] = "battle"
            
            print(f"[SERVER] Игра {game_id} начата! Первый ход у: {game['current_turn']}")

        return jsonify({"success": True, "game_id": game_id})
        
    except Exception as e:
        print(f"[SERVER ERROR] Ошибка в /ships: {str(e)}")
        return jsonify({"success": False, "message": f"Внутренняя ошибка сервера: {str(e)}"}), 500

@app.route("/shoot", methods=["POST"])
def shoot():
    try:

        game_id = request.args.get("game_id", "1")

        if not request.is_json:
            return jsonify({"success": False, "message": "Неверный формат данных"}), 400
            
        data = request.get_json()
        
        name = data.get("name")
        x = data.get("x")
        y = data.get("y")

        if not name:
            return jsonify({"success": False, "message": "Не указано имя"}), 400
            
        if x is None or y is None:
            return jsonify({"success": False, "message": "Не указаны координаты"}), 400

        if game_id not in active_games:
            return jsonify({"success": False, "message": "Игра не найдена"}), 404

        game = active_games[game_id]

        if name not in game["players"]:
            return jsonify({"success": False, "message": "Игрок не найден"}), 404

        if game["current_turn"] is None:
            return jsonify({
                "success": False,
                "message": "Игра еще не началась"
            }), 400

        if name != game["current_turn"]:
            return jsonify({
                "success": False,
                "result": "not_your_turn",
                "message": "Сейчас ход соперника"
            }), 400

        enemy = None
        for p in game["players"]:
            if p != name:
                enemy = p
                break
        
        if enemy is None:
            return jsonify({
                "success": False,
                "message": "Противник не найден"
            }), 400

        shot = [x, y]

        if shot in game["hits"].get(enemy, []):
            return jsonify({
                "success": False,
                "result": "repeat",
                "message": "Вы уже стреляли в эту клетку"
            }), 400

        if enemy not in game["hits"]:
            game["hits"][enemy] = []
        game["hits"][enemy].append(shot)

        if enemy not in game["shots"]:
            game["shots"][enemy] = []
        game["shots"][enemy].append([x, y])

        if shot in game["boards"].get(enemy, []):
            if enemy in game["boards"]:
                try:
                    game["boards"][enemy].remove(shot)
                except ValueError:
                    pass

            if not game["boards"].get(enemy, []):
                game["player_status"][name] = "win"
                game["player_status"][enemy] = "lose"
                save_game_event(game_id, "game_over", name)
                
                return jsonify({
                    "success": True,
                    "result": "win",
                    "message": "Вы победили!"
                })

            return jsonify({
                "success": True,
                "result": "hit",
                "message": "Попадание! Ваш ход продолжается"
            })

        game["current_turn"] = enemy

        return jsonify({
            "success": True,
            "result": "miss",
            "message": "Мимо! Ход переходит сопернику"
        })
        
    except Exception as e:
        print(f"[SERVER ERROR] Ошибка в /shoot: {str(e)}")
        return jsonify({"success": False, "message": f"Внутренняя ошибка сервера: {str(e)}"}), 500

@app.route("/reset", methods=["POST"])
def reset():
    game_id = request.args.get("game_id", "1")
    
    if game_id in active_games:
        players_list = active_games[game_id].get("players", [])
        
        active_games[game_id] = {
            "players": players_list, 
            "player_status": {p: "waiting" for p in players_list},
            "ships_data": {},
            "game_history": [],
            "boards": {},
            "hits": {},
            "shots": {},
            "current_turn": None,
            "created_at": datetime.now().isoformat()
        }
        
        save_game_event(game_id, "game_reset", None)
        
        return jsonify({"success": True, "message": f"Игра {game_id} сброшена"})
    
    return jsonify({"success": False, "message": "Игра не найдена"}), 404

@app.route("/clear_all", methods=["POST"])
def clear_all():
    """Полностью очистить все игры и начать заново"""
    global active_games, next_game_id
    
    active_games = {
        "1": {
            "players": [],
            "player_status": {},
            "ships_data": {},
            "game_history": [],
            "boards": {},
            "hits": {},
            "shots": {},
            "current_turn": None,
            "created_at": datetime.now().isoformat()
        }
    }
    next_game_id = 2
    
    return jsonify({"success": True, "message": "Все игры очищены"})

@app.route("/debug")
def debug():
    """Отладочная информация о всех играх"""
    debug_info = {
        "active_games_count": len(active_games),
        "games": {}
    }
    
    for game_id, game in active_games.items():
        debug_info["games"][game_id] = {
            "players": game.get("players", []),
            "player_status": game.get("player_status", {}),
            "players_count": len(game.get("players", [])),
            "has_ships_data": len(game.get("ships_data", {})) > 0,
            "current_turn": game.get("current_turn"),
            "boards_count": len(game.get("boards", {})),
            "hits_count": len(game.get("hits", {})),
            "shots_count": len(game.get("shots", {})),
            "created_at": game.get("created_at")
        }
    
    return jsonify(debug_info)

if __name__ == "__main__":
    print("=" * 80)
    print("БАТЛШИП СЕРВЕР ЗАПУЩЕН!")
    print("=" * 80)
    print("Примечание: Все данные хранятся только в памяти.")
    print("При перезапуске сервера все игры будут очищены.")
    print("=" * 80)
    print("\nДоступные эндпоинты:")
    print("  POST /connect      - подключиться к игре")
    print("  GET  /state        - получить состояние игры")
    print("  GET  /games        - список всех активных игр")
    print("  POST /ships        - отправить корабли")
    print("  POST /shoot        - сделать выстрел")
    print("  POST /reset        - сбросить игру")
    print("  POST /clear_all    - полностью очистить все игры")
    print("  GET  /debug        - отладочная информация")
    print(f"\nТекущие активные игры: {list(active_games.keys())}")
    print("=" * 80)
    app.run("0.0.0.0", 5000, debug=True)