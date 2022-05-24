namespace ChessGame
{
    partial class GameWindow
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
            this.boardPanel = new System.Windows.Forms.Panel();
            this.statusScrollLeftBtn = new System.Windows.Forms.Button();
            this.statusScrollRightBtn = new System.Windows.Forms.Button();
            this.scrollPosLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // boardPanel
            // 
            this.boardPanel.Location = new System.Drawing.Point(12, 38);
            this.boardPanel.Name = "boardPanel";
            this.boardPanel.Size = new System.Drawing.Size(199, 109);
            this.boardPanel.TabIndex = 0;
            // 
            // statusScrollLeftBtn
            // 
            this.statusScrollLeftBtn.Location = new System.Drawing.Point(12, 9);
            this.statusScrollLeftBtn.Name = "statusScrollLeftBtn";
            this.statusScrollLeftBtn.Size = new System.Drawing.Size(75, 23);
            this.statusScrollLeftBtn.TabIndex = 1;
            this.statusScrollLeftBtn.Text = "<";
            this.statusScrollLeftBtn.UseVisualStyleBackColor = true;
            this.statusScrollLeftBtn.Click += new System.EventHandler(this.statusScrollLeftBtn_Click);
            // 
            // statusScrollRightBtn
            // 
            this.statusScrollRightBtn.Location = new System.Drawing.Point(93, 9);
            this.statusScrollRightBtn.Name = "statusScrollRightBtn";
            this.statusScrollRightBtn.Size = new System.Drawing.Size(75, 23);
            this.statusScrollRightBtn.TabIndex = 2;
            this.statusScrollRightBtn.Text = ">";
            this.statusScrollRightBtn.UseVisualStyleBackColor = true;
            this.statusScrollRightBtn.Click += new System.EventHandler(this.statusScrollRightBtn_Click);
            // 
            // scrollPosLabel
            // 
            this.scrollPosLabel.AutoSize = true;
            this.scrollPosLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.scrollPosLabel.Location = new System.Drawing.Point(174, 9);
            this.scrollPosLabel.Name = "scrollPosLabel";
            this.scrollPosLabel.Size = new System.Drawing.Size(166, 24);
            this.scrollPosLabel.TabIndex = 3;
            this.scrollPosLabel.Text = "PLACE HOLDER";
            // 
            // GameWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(526, 546);
            this.Controls.Add(this.scrollPosLabel);
            this.Controls.Add(this.statusScrollRightBtn);
            this.Controls.Add(this.statusScrollLeftBtn);
            this.Controls.Add(this.boardPanel);
            this.Name = "GameWindow";
            this.Text = "GameWindow";
            this.Shown += new System.EventHandler(this.shownEvent);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.Panel boardPanel;
        private System.Windows.Forms.Button statusScrollLeftBtn;
        private System.Windows.Forms.Button statusScrollRightBtn;
        public System.Windows.Forms.Label scrollPosLabel;
    }
}