namespace итер4
{
    partial class GameForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GameForm));
            this.lblTurnInfo = new System.Windows.Forms.Label();
            this.lblPlayerInfo = new System.Windows.Forms.Label();
            this.lblMyFieldTitle = new System.Windows.Forms.Label();
            this.lblEnemyFieldTitle = new System.Windows.Forms.Label();
            this.btnExit = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblTurnInfo
            // 
            this.lblTurnInfo.AutoSize = true;
            this.lblTurnInfo.Location = new System.Drawing.Point(31, 36);
            this.lblTurnInfo.Name = "lblTurnInfo";
            this.lblTurnInfo.Size = new System.Drawing.Size(0, 16);
            this.lblTurnInfo.TabIndex = 0;
            // 
            // lblPlayerInfo
            // 
            this.lblPlayerInfo.AutoSize = true;
            this.lblPlayerInfo.Location = new System.Drawing.Point(34, 85);
            this.lblPlayerInfo.Name = "lblPlayerInfo";
            this.lblPlayerInfo.Size = new System.Drawing.Size(0, 16);
            this.lblPlayerInfo.TabIndex = 1;
            // 
            // lblMyFieldTitle
            // 
            this.lblMyFieldTitle.AutoSize = true;
            this.lblMyFieldTitle.Location = new System.Drawing.Point(70, 122);
            this.lblMyFieldTitle.Name = "lblMyFieldTitle";
            this.lblMyFieldTitle.Size = new System.Drawing.Size(0, 16);
            this.lblMyFieldTitle.TabIndex = 2;
            // 
            // lblEnemyFieldTitle
            // 
            this.lblEnemyFieldTitle.AutoSize = true;
            this.lblEnemyFieldTitle.Location = new System.Drawing.Point(591, 121);
            this.lblEnemyFieldTitle.Name = "lblEnemyFieldTitle";
            this.lblEnemyFieldTitle.Size = new System.Drawing.Size(0, 16);
            this.lblEnemyFieldTitle.TabIndex = 3;
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(957, 201);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(61, 58);
            this.btnExit.TabIndex = 4;
            this.btnExit.Text = "Выйти";
            this.btnExit.UseVisualStyleBackColor = true;
            // 
            // GameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(1019, 574);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.lblEnemyFieldTitle);
            this.Controls.Add(this.lblMyFieldTitle);
            this.Controls.Add(this.lblPlayerInfo);
            this.Controls.Add(this.lblTurnInfo);
            this.Name = "GameForm";
            this.Text = "GameForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTurnInfo;
        private System.Windows.Forms.Label lblPlayerInfo;
        private System.Windows.Forms.Label lblMyFieldTitle;
        private System.Windows.Forms.Label lblEnemyFieldTitle;
        private System.Windows.Forms.Button btnExit;
    }
}