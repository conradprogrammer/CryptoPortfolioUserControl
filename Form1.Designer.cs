namespace CryptoPortfolioManagerTest
{
    partial class Form1
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
            this.portfolioView1 = new CryptoPortfolioManager.PortfolioView();
            this.SuspendLayout();
            // 
            // portfolioView1
            // 
            this.portfolioView1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.portfolioView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.portfolioView1.Location = new System.Drawing.Point(0, 0);
            this.portfolioView1.MinimumSize = new System.Drawing.Size(500, 300);
            this.portfolioView1.Name = "portfolioView1";
            this.portfolioView1.Padding = new System.Windows.Forms.Padding(10);
            this.portfolioView1.Size = new System.Drawing.Size(884, 510);
            this.portfolioView1.TabIndex = 0;
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(884, 510);
            this.Controls.Add(this.portfolioView1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private CryptoPortfolioManager.PortfolioView portfolioView1;
    }
}

