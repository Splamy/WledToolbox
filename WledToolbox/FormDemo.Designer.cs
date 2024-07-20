namespace DesktopDuplication.Demo
{
    partial class FormDemo
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
            pictureBox2 = new System.Windows.Forms.CustomPictureBox();
            selectMonitorDropDown = new System.Windows.Forms.ComboBox();
            label1 = new System.Windows.Forms.Label();
            sendWledDataCheckbox = new System.Windows.Forms.CheckBox();
            groupBox1 = new System.Windows.Forms.GroupBox();
            label4 = new System.Windows.Forms.Label();
            whiteLow = new System.Windows.Forms.TrackBar();
            whiteHigh = new System.Windows.Forms.TrackBar();
            blackLow = new System.Windows.Forms.TrackBar();
            blackHigh = new System.Windows.Forms.TrackBar();
            gammaLock = new System.Windows.Forms.CheckBox();
            gammaB = new System.Windows.Forms.TrackBar();
            gammeG = new System.Windows.Forms.TrackBar();
            label3 = new System.Windows.Forms.Label();
            gammaR = new System.Windows.Forms.TrackBar();
            checkDebugOutputImage = new System.Windows.Forms.CheckBox();
            label2 = new System.Windows.Forms.Label();
            trackBar1 = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)whiteLow).BeginInit();
            ((System.ComponentModel.ISupportInitialize)whiteHigh).BeginInit();
            ((System.ComponentModel.ISupportInitialize)blackLow).BeginInit();
            ((System.ComponentModel.ISupportInitialize)blackHigh).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gammaB).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gammeG).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gammaR).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBar1).BeginInit();
            SuspendLayout();
            // 
            // pictureBox2
            // 
            pictureBox2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pictureBox2.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            pictureBox2.Location = new System.Drawing.Point(355, 12);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new System.Drawing.Size(527, 374);
            pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox2.TabIndex = 0;
            pictureBox2.TabStop = false;
            // 
            // selectMonitorDropDown
            // 
            selectMonitorDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            selectMonitorDropDown.FormattingEnabled = true;
            selectMonitorDropDown.Location = new System.Drawing.Point(104, 17);
            selectMonitorDropDown.Name = "selectMonitorDropDown";
            selectMonitorDropDown.Size = new System.Drawing.Size(214, 23);
            selectMonitorDropDown.TabIndex = 1;
            selectMonitorDropDown.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(48, 20);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(50, 15);
            label1.TabIndex = 2;
            label1.Text = "Monitor";
            // 
            // sendWledDataCheckbox
            // 
            sendWledDataCheckbox.AutoSize = true;
            sendWledDataCheckbox.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            sendWledDataCheckbox.Location = new System.Drawing.Point(28, 71);
            sendWledDataCheckbox.Name = "sendWledDataCheckbox";
            sendWledDataCheckbox.Size = new System.Drawing.Size(91, 19);
            sendWledDataCheckbox.TabIndex = 3;
            sendWledDataCheckbox.Text = "Enable Wled";
            sendWledDataCheckbox.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(whiteLow);
            groupBox1.Controls.Add(whiteHigh);
            groupBox1.Controls.Add(blackLow);
            groupBox1.Controls.Add(blackHigh);
            groupBox1.Controls.Add(gammaLock);
            groupBox1.Controls.Add(gammaB);
            groupBox1.Controls.Add(gammeG);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(gammaR);
            groupBox1.Controls.Add(checkDebugOutputImage);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(trackBar1);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(selectMonitorDropDown);
            groupBox1.Controls.Add(sendWledDataCheckbox);
            groupBox1.Location = new System.Drawing.Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(337, 374);
            groupBox1.TabIndex = 5;
            groupBox1.TabStop = false;
            groupBox1.Text = "groupBox1";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(48, 300);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(62, 15);
            label4.TabIndex = 16;
            label4.Text = "Brightness";
            // 
            // whiteLow
            // 
            whiteLow.AutoSize = false;
            whiteLow.Location = new System.Drawing.Point(124, 300);
            whiteLow.Maximum = 100;
            whiteLow.Name = "whiteLow";
            whiteLow.Size = new System.Drawing.Size(94, 25);
            whiteLow.TabIndex = 15;
            whiteLow.TickFrequency = 10;
            whiteLow.Value = 80;
            whiteLow.Scroll += whiteLow_Scroll;
            // 
            // whiteHigh
            // 
            whiteHigh.AutoSize = false;
            whiteHigh.Location = new System.Drawing.Point(224, 300);
            whiteHigh.Maximum = 100;
            whiteHigh.Name = "whiteHigh";
            whiteHigh.Size = new System.Drawing.Size(94, 25);
            whiteHigh.TabIndex = 14;
            whiteHigh.TickFrequency = 10;
            whiteHigh.Value = 80;
            whiteHigh.Scroll += whiteHigh_Scroll;
            // 
            // blackLow
            // 
            blackLow.AutoSize = false;
            blackLow.Location = new System.Drawing.Point(124, 269);
            blackLow.Maximum = 100;
            blackLow.Name = "blackLow";
            blackLow.Size = new System.Drawing.Size(94, 25);
            blackLow.TabIndex = 13;
            blackLow.TickFrequency = 10;
            blackLow.Value = 20;
            blackLow.Scroll += blackLow_Scroll;
            // 
            // blackHigh
            // 
            blackHigh.AutoSize = false;
            blackHigh.Location = new System.Drawing.Point(224, 269);
            blackHigh.Maximum = 100;
            blackHigh.Name = "blackHigh";
            blackHigh.Size = new System.Drawing.Size(94, 25);
            blackHigh.TabIndex = 12;
            blackHigh.TickFrequency = 10;
            blackHigh.Value = 20;
            blackHigh.Scroll += blackHigh_Scroll;
            // 
            // gammaLock
            // 
            gammaLock.AutoSize = true;
            gammaLock.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            gammaLock.Checked = true;
            gammaLock.CheckState = System.Windows.Forms.CheckState.Checked;
            gammaLock.Location = new System.Drawing.Point(48, 173);
            gammaLock.Name = "gammaLock";
            gammaLock.Size = new System.Drawing.Size(51, 19);
            gammaLock.TabIndex = 11;
            gammaLock.Text = "Lock";
            gammaLock.UseVisualStyleBackColor = true;
            gammaLock.CheckedChanged += gammaLock_CheckedChanged;
            // 
            // gammaB
            // 
            gammaB.AutoSize = false;
            gammaB.Location = new System.Drawing.Point(104, 217);
            gammaB.Maximum = 90;
            gammaB.Name = "gammaB";
            gammaB.Size = new System.Drawing.Size(214, 25);
            gammaB.TabIndex = 10;
            gammaB.TickFrequency = 30;
            gammaB.Value = 30;
            gammaB.Scroll += gammaB_Scroll;
            // 
            // gammeG
            // 
            gammeG.AutoSize = false;
            gammeG.Location = new System.Drawing.Point(104, 186);
            gammeG.Maximum = 90;
            gammeG.Name = "gammeG";
            gammeG.Size = new System.Drawing.Size(214, 25);
            gammeG.TabIndex = 9;
            gammeG.TickFrequency = 30;
            gammeG.Value = 30;
            gammeG.Scroll += gammeG_Scroll;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(48, 155);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(49, 15);
            label3.TabIndex = 8;
            label3.Text = "Gamma";
            // 
            // gammaR
            // 
            gammaR.AutoSize = false;
            gammaR.Location = new System.Drawing.Point(104, 155);
            gammaR.Maximum = 90;
            gammaR.Name = "gammaR";
            gammaR.Size = new System.Drawing.Size(214, 25);
            gammaR.TabIndex = 7;
            gammaR.TickFrequency = 30;
            gammaR.Value = 30;
            gammaR.Scroll += gammaR_Scroll;
            // 
            // checkDebugOutputImage
            // 
            checkDebugOutputImage.AutoSize = true;
            checkDebugOutputImage.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            checkDebugOutputImage.Location = new System.Drawing.Point(28, 46);
            checkDebugOutputImage.Name = "checkDebugOutputImage";
            checkDebugOutputImage.Size = new System.Drawing.Size(136, 19);
            checkDebugOutputImage.TabIndex = 6;
            checkDebugOutputImage.Text = "Debug output image";
            checkDebugOutputImage.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(28, 106);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(73, 15);
            label2.TabIndex = 5;
            label2.Text = "BlurStrength";
            // 
            // trackBar1
            // 
            trackBar1.AutoSize = false;
            trackBar1.Location = new System.Drawing.Point(107, 102);
            trackBar1.Maximum = 50;
            trackBar1.Name = "trackBar1";
            trackBar1.Size = new System.Drawing.Size(214, 25);
            trackBar1.TabIndex = 4;
            trackBar1.TickFrequency = 5;
            trackBar1.Value = 15;
            trackBar1.Scroll += trackBar1_Scroll;
            // 
            // FormDemo
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            ClientSize = new System.Drawing.Size(894, 398);
            Controls.Add(groupBox1);
            Controls.Add(pictureBox2);
            DoubleBuffered = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "FormDemo";
            Text = "Desktop Duplication API Demo";
            Shown += FormDemo_Shown;
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)whiteLow).EndInit();
            ((System.ComponentModel.ISupportInitialize)whiteHigh).EndInit();
            ((System.ComponentModel.ISupportInitialize)blackLow).EndInit();
            ((System.ComponentModel.ISupportInitialize)blackHigh).EndInit();
            ((System.ComponentModel.ISupportInitialize)gammaB).EndInit();
            ((System.ComponentModel.ISupportInitialize)gammeG).EndInit();
            ((System.ComponentModel.ISupportInitialize)gammaR).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBar1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.CustomPictureBox pictureBox2;
        private System.Windows.Forms.ComboBox selectMonitorDropDown;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox sendWledDataCheckbox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.CheckBox checkDebugOutputImage;
        private System.Windows.Forms.CheckBox gammaLock;
        private System.Windows.Forms.TrackBar gammaB;
        private System.Windows.Forms.TrackBar gammeG;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TrackBar gammaR;
        private System.Windows.Forms.TrackBar whiteLow;
        private System.Windows.Forms.TrackBar whiteHigh;
        private System.Windows.Forms.TrackBar blackLow;
        private System.Windows.Forms.TrackBar blackHigh;
        private System.Windows.Forms.Label label4;
    }
}

