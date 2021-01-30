namespace AutoLockHelper
{
    partial class Settings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Settings));
            this.logoutTimeBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // logoutTimeBox
            // 
            this.logoutTimeBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.logoutTimeBox.FormattingEnabled = true;
            this.logoutTimeBox.Items.AddRange(new object[] {
            "00:20:00",
            "00:30:00",
            "00:40:00",
            "00:45:00",
            "00:50:00",
            "00:55:00",
            "01:00:00",
            "01:05:00",
            "01:10:00",
            "01:15:00",
            "01:20:00",
            "01:25:00",
            "01:30:00",
            "01:35:00",
            "01:40:00",
            "01:45:00",
            "01:50:00",
            "01:55:00",
            "02:00:00"});
            this.logoutTimeBox.Location = new System.Drawing.Point(121, 30);
            this.logoutTimeBox.Name = "logoutTimeBox";
            this.logoutTimeBox.Size = new System.Drawing.Size(151, 26);
            this.logoutTimeBox.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.25F);
            this.label1.Location = new System.Drawing.Point(8, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(112, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "Logout Time: ";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(278, 30);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(127, 26);
            this.button1.TabIndex = 2;
            this.button1.Text = "Update Settings";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(424, 86);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.logoutTimeBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Settings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox logoutTimeBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
    }
}