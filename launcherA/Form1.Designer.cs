namespace launcherA
{
    partial class SGDCLauncher
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
            this.components = new System.ComponentModel.Container();
            this.tmrController = new System.Windows.Forms.Timer(this.components);
            this.tmrAttract = new System.Windows.Forms.Timer(this.components);
            this.webBrowser = new CefSharp.WinForms.ChromiumWebBrowser();
            this.tmrAttractWait = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // tmrController
            // 
            this.tmrController.Enabled = true;
            this.tmrController.Interval = 16;
            this.tmrController.Tick += new System.EventHandler(this.TmrController_Tick);
            // 
            // tmrAttract
            // 
            this.tmrAttract.Enabled = true;
            this.tmrAttract.Interval = 10000;
            this.tmrAttract.Tick += new System.EventHandler(this.TmrAttract_Tick);
            // 
            // webBrowser
            // 
            this.webBrowser.ActivateBrowserOnCreation = false;
// TODO: Code generation for '' failed because of Exception 'Invalid Primitive Type: System.IntPtr. Consider using CodeObjectCreateExpression.'.
            this.webBrowser.Location = new System.Drawing.Point(0, 0);
            this.webBrowser.Margin = new System.Windows.Forms.Padding(0);
            this.webBrowser.Name = "webBrowser";
            this.webBrowser.Size = new System.Drawing.Size(1905, 1040);
            this.webBrowser.TabIndex = 14;
            this.webBrowser.TabStop = false;
            // 
            // tmrAttractWait
            // 
            this.tmrAttractWait.Enabled = true;
            this.tmrAttractWait.Interval = 45000;
            this.tmrAttractWait.Tick += new System.EventHandler(this.TmrAttractWait_Tick);
            // 
            // SGDCLauncher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1904, 1041);
            this.Controls.Add(this.webBrowser);
            this.Name = "SGDCLauncher";
            this.Text = "SGDC Launcher";
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Timer tmrController;
        private System.Windows.Forms.Timer tmrAttract;
        private CefSharp.WinForms.ChromiumWebBrowser webBrowser;
        private System.Windows.Forms.Timer tmrAttractWait;
    }
}

