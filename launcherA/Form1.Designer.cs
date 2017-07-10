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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SGDCLauncher));
            this.pbLogo = new System.Windows.Forms.PictureBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblDesc = new System.Windows.Forms.Label();
            this.lblList = new System.Windows.Forms.Label();
            this.mpVideo = new AxWMPLib.AxWindowsMediaPlayer();
            this.lblPressStart = new System.Windows.Forms.Label();
            this.lblDevs = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.lblMessage = new System.Windows.Forms.Label();
            this.tmrBlink = new System.Windows.Forms.Timer(this.components);
            this.lblPlays = new System.Windows.Forms.Label();
            this.tmrController = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mpVideo)).BeginInit();
            this.SuspendLayout();
            // 
            // pbLogo
            // 
            this.pbLogo.BackColor = System.Drawing.Color.Transparent;
            this.pbLogo.BackgroundImage = global::launcherA.Properties.Resources.SGDC_logo;
            this.pbLogo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pbLogo.Location = new System.Drawing.Point(12, 40);
            this.pbLogo.Name = "pbLogo";
            this.pbLogo.Size = new System.Drawing.Size(522, 166);
            this.pbLogo.TabIndex = 2;
            this.pbLogo.TabStop = false;
            // 
            // lblTitle
            // 
            this.lblTitle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.Font = new System.Drawing.Font("NK57 Monospace Cd Rg", 42F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(574, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(959, 117);
            this.lblTitle.TabIndex = 3;
            this.lblTitle.Text = "Game Name";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDesc
            // 
            this.lblDesc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDesc.BackColor = System.Drawing.Color.Transparent;
            this.lblDesc.Font = new System.Drawing.Font("NK57 Monospace Cd Rg", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDesc.Location = new System.Drawing.Point(565, 754);
            this.lblDesc.Name = "lblDesc";
            this.lblDesc.Size = new System.Drawing.Size(968, 172);
            this.lblDesc.TabIndex = 4;
            this.lblDesc.Text = "This is a placeholder description.";
            this.lblDesc.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblList
            // 
            this.lblList.BackColor = System.Drawing.Color.Transparent;
            this.lblList.Font = new System.Drawing.Font("NK57 Monospace Cd Rg", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblList.Location = new System.Drawing.Point(12, 321);
            this.lblList.Name = "lblList";
            this.lblList.Size = new System.Drawing.Size(522, 739);
            this.lblList.TabIndex = 5;
            this.lblList.Text = "Game A\r\nGame B\r\nGame C\r\nGame D\r\n> Game E <\r\nGame F\r\nGame G";
            this.lblList.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // mpVideo
            // 
            this.mpVideo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.mpVideo.Enabled = true;
            this.mpVideo.Location = new System.Drawing.Point(565, 129);
            this.mpVideo.Name = "mpVideo";
            this.mpVideo.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("mpVideo.OcxState")));
            this.mpVideo.Size = new System.Drawing.Size(968, 546);
            this.mpVideo.TabIndex = 8;
            // 
            // lblPressStart
            // 
            this.lblPressStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPressStart.BackColor = System.Drawing.Color.Transparent;
            this.lblPressStart.Font = new System.Drawing.Font("NK57 Monospace Cd Rg", 71.99999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPressStart.Location = new System.Drawing.Point(565, 879);
            this.lblPressStart.Name = "lblPressStart";
            this.lblPressStart.Size = new System.Drawing.Size(968, 137);
            this.lblPressStart.TabIndex = 9;
            this.lblPressStart.Text = "Press Start";
            this.lblPressStart.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDevs
            // 
            this.lblDevs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDevs.BackColor = System.Drawing.Color.Transparent;
            this.lblDevs.Font = new System.Drawing.Font("NK57 Monospace Cd Rg", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDevs.Location = new System.Drawing.Point(560, 701);
            this.lblDevs.Name = "lblDevs";
            this.lblDevs.Size = new System.Drawing.Size(973, 35);
            this.lblDevs.TabIndex = 10;
            this.lblDevs.Text = "By: Adam Gincel";
            this.lblDevs.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.BackColor = System.Drawing.Color.Transparent;
            this.lblSubtitle.Font = new System.Drawing.Font("NK57 Monospace Sc Bk", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSubtitle.Location = new System.Drawing.Point(12, 206);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(522, 101);
            this.lblSubtitle.TabIndex = 11;
            this.lblSubtitle.Text = "Jacobus Arcade Machine\r\n\r\nGames:";
            this.lblSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblMessage
            // 
            this.lblMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMessage.BackColor = System.Drawing.Color.Transparent;
            this.lblMessage.Font = new System.Drawing.Font("NK57 Monospace Sc Bk", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMessage.Location = new System.Drawing.Point(1564, 40);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(300, 976);
            this.lblMessage.TabIndex = 12;
            this.lblMessage.Text = resources.GetString("lblMessage.Text");
            this.lblMessage.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // tmrBlink
            // 
            this.tmrBlink.Enabled = true;
            this.tmrBlink.Interval = 850;
            this.tmrBlink.Tick += new System.EventHandler(this.tmrBlink_Tick);
            // 
            // lblPlays
            // 
            this.lblPlays.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPlays.BackColor = System.Drawing.Color.Transparent;
            this.lblPlays.Font = new System.Drawing.Font("NK57 Monospace Cd Rg", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPlays.Location = new System.Drawing.Point(562, 733);
            this.lblPlays.Name = "lblPlays";
            this.lblPlays.Size = new System.Drawing.Size(971, 35);
            this.lblPlays.TabIndex = 13;
            this.lblPlays.Text = "Plays: 0      Time Played: 00:00";
            this.lblPlays.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tmrController
            // 
            this.tmrController.Enabled = true;
            this.tmrController.Interval = 1;
            this.tmrController.Tick += new System.EventHandler(this.tmrController_Tick);
            // 
            // SGDCLauncher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::launcherA.Properties.Resources.hip_square;
            this.ClientSize = new System.Drawing.Size(1904, 1041);
            this.Controls.Add(this.lblPlays);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.lblSubtitle);
            this.Controls.Add(this.lblDevs);
            this.Controls.Add(this.lblPressStart);
            this.Controls.Add(this.mpVideo);
            this.Controls.Add(this.lblList);
            this.Controls.Add(this.lblDesc);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.pbLogo);
            this.Name = "SGDCLauncher";
            this.Text = "SGDC Launcher";
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mpVideo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.PictureBox pbLogo;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblDesc;
        private System.Windows.Forms.Label lblList;
        private AxWMPLib.AxWindowsMediaPlayer mpVideo;
        private System.Windows.Forms.Label lblPressStart;
        private System.Windows.Forms.Label lblDevs;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Timer tmrBlink;
        private System.Windows.Forms.Label lblPlays;
        private System.Windows.Forms.Timer tmrController;
    }
}

