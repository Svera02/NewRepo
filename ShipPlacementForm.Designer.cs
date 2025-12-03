namespace итер4
{
    partial class ShipPlacementForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShipPlacementForm));
            this.lblInfo = new System.Windows.Forms.Label();
            this.btnReady = new System.Windows.Forms.Button();
            this.panelField = new System.Windows.Forms.Panel();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnRotate = new System.Windows.Forms.Button();
            this.btnAuto = new System.Windows.Forms.Button();
            this.btnrules = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.Location = new System.Drawing.Point(105, 31);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(44, 16);
            this.lblInfo.TabIndex = 0;
            this.lblInfo.Text = "label1";
            // 
            // btnReady
            // 
            this.btnReady.Location = new System.Drawing.Point(692, 205);
            this.btnReady.Name = "btnReady";
            this.btnReady.Size = new System.Drawing.Size(75, 23);
            this.btnReady.TabIndex = 1;
            this.btnReady.Text = "Готов!";
            this.btnReady.UseVisualStyleBackColor = true;
            // 
            // panelField
            // 
            this.panelField.BackColor = System.Drawing.Color.Transparent;
            this.panelField.Location = new System.Drawing.Point(172, 174);
            this.panelField.Name = "panelField";
            this.panelField.Size = new System.Drawing.Size(462, 428);
            this.panelField.TabIndex = 2;
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(12, 385);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(108, 52);
            this.btnClear.TabIndex = 3;
            this.btnClear.Text = "Отчистить";
            this.btnClear.UseVisualStyleBackColor = true;
            // 
            // btnRotate
            // 
            this.btnRotate.Location = new System.Drawing.Point(12, 266);
            this.btnRotate.Name = "btnRotate";
            this.btnRotate.Size = new System.Drawing.Size(108, 48);
            this.btnRotate.TabIndex = 4;
            this.btnRotate.Text = "Развернуть";
            this.btnRotate.UseVisualStyleBackColor = true;
            // 
            // btnAuto
            // 
            this.btnAuto.Location = new System.Drawing.Point(12, 332);
            this.btnAuto.Name = "btnAuto";
            this.btnAuto.Size = new System.Drawing.Size(108, 47);
            this.btnAuto.TabIndex = 5;
            this.btnAuto.Text = "Случайно";
            this.btnAuto.UseVisualStyleBackColor = true;
            // 
            // btnrules
            // 
            this.btnrules.Location = new System.Drawing.Point(12, 199);
            this.btnrules.Name = "btnrules";
            this.btnrules.Size = new System.Drawing.Size(108, 52);
            this.btnrules.TabIndex = 6;
            this.btnrules.Text = "Правила";
            this.btnrules.UseVisualStyleBackColor = true;
            // 
            // ShipPlacementForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(800, 629);
            this.Controls.Add(this.btnrules);
            this.Controls.Add(this.btnAuto);
            this.Controls.Add(this.btnRotate);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.panelField);
            this.Controls.Add(this.btnReady);
            this.Controls.Add(this.lblInfo);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ShipPlacementForm";
            this.Text = "ShipPlacementForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Button btnReady;
        private System.Windows.Forms.Panel panelField;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnRotate;
        private System.Windows.Forms.Button btnAuto;
        private System.Windows.Forms.Button btnrules;
    }
}