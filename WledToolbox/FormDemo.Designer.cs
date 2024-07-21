namespace WledToolbox
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
            outputDebugPicture = new System.Windows.Forms.CustomPictureBox();
            selectMonitorDropDown = new System.Windows.Forms.ComboBox();
            label1 = new System.Windows.Forms.Label();
            sendWledDataCheckbox = new System.Windows.Forms.CheckBox();
            groupBox = new System.Windows.Forms.GroupBox();
            cropLeft = new System.Windows.Forms.NumericUpDown();
            cropBottom = new System.Windows.Forms.NumericUpDown();
            cropRight = new System.Windows.Forms.NumericUpDown();
            cropTop = new System.Windows.Forms.NumericUpDown();
            label5 = new System.Windows.Forms.Label();
            brightnessPointPicker = new PointPicker2D();
            label4 = new System.Windows.Forms.Label();
            gammaLock = new System.Windows.Forms.CheckBox();
            gammaB = new System.Windows.Forms.TrackBar();
            gammeG = new System.Windows.Forms.TrackBar();
            label3 = new System.Windows.Forms.Label();
            gammaR = new System.Windows.Forms.TrackBar();
            label2 = new System.Windows.Forms.Label();
            trackBar1 = new System.Windows.Forms.TrackBar();
            checkDebugOutputImage = new System.Windows.Forms.CheckBox();
            inputDebugPicture = new System.Windows.Forms.CustomPictureBox();
            checkDebugInputImage = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)outputDebugPicture).BeginInit();
            groupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)cropLeft).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cropBottom).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cropRight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cropTop).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gammaB).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gammeG).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gammaR).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBar1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)inputDebugPicture).BeginInit();
            SuspendLayout();
            // 
            // outputDebugPicture
            // 
            outputDebugPicture.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            outputDebugPicture.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            outputDebugPicture.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            outputDebugPicture.Location = new System.Drawing.Point(355, 32);
            outputDebugPicture.Name = "outputDebugPicture";
            outputDebugPicture.Size = new System.Drawing.Size(598, 144);
            outputDebugPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            outputDebugPicture.TabIndex = 0;
            outputDebugPicture.TabStop = false;
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
            sendWledDataCheckbox.Location = new System.Drawing.Point(28, 46);
            sendWledDataCheckbox.Name = "sendWledDataCheckbox";
            sendWledDataCheckbox.Size = new System.Drawing.Size(91, 19);
            sendWledDataCheckbox.TabIndex = 3;
            sendWledDataCheckbox.Text = "Enable Wled";
            sendWledDataCheckbox.UseVisualStyleBackColor = true;
            // 
            // groupBox
            // 
            groupBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            groupBox.Controls.Add(cropLeft);
            groupBox.Controls.Add(cropBottom);
            groupBox.Controls.Add(cropRight);
            groupBox.Controls.Add(cropTop);
            groupBox.Controls.Add(label5);
            groupBox.Controls.Add(brightnessPointPicker);
            groupBox.Controls.Add(label4);
            groupBox.Controls.Add(gammaLock);
            groupBox.Controls.Add(gammaB);
            groupBox.Controls.Add(gammeG);
            groupBox.Controls.Add(label3);
            groupBox.Controls.Add(gammaR);
            groupBox.Controls.Add(label2);
            groupBox.Controls.Add(trackBar1);
            groupBox.Controls.Add(label1);
            groupBox.Controls.Add(selectMonitorDropDown);
            groupBox.Controls.Add(sendWledDataCheckbox);
            groupBox.Location = new System.Drawing.Point(12, 12);
            groupBox.Name = "groupBox";
            groupBox.Size = new System.Drawing.Size(337, 698);
            groupBox.TabIndex = 5;
            groupBox.TabStop = false;
            groupBox.Text = "groupBox1";
            // 
            // cropLeft
            // 
            cropLeft.Location = new System.Drawing.Point(143, 81);
            cropLeft.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            cropLeft.Name = "cropLeft";
            cropLeft.Size = new System.Drawing.Size(61, 23);
            cropLeft.TabIndex = 22;
            cropLeft.ValueChanged += cropLeft_ValueChanged;
            // 
            // cropBottom
            // 
            cropBottom.Location = new System.Drawing.Point(206, 110);
            cropBottom.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            cropBottom.Name = "cropBottom";
            cropBottom.Size = new System.Drawing.Size(61, 23);
            cropBottom.TabIndex = 21;
            cropBottom.ValueChanged += cropBottom_ValueChanged;
            // 
            // cropRight
            // 
            cropRight.Location = new System.Drawing.Point(269, 81);
            cropRight.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            cropRight.Name = "cropRight";
            cropRight.Size = new System.Drawing.Size(61, 23);
            cropRight.TabIndex = 20;
            cropRight.ValueChanged += cropRight_ValueChanged;
            // 
            // cropTop
            // 
            cropTop.Location = new System.Drawing.Point(206, 53);
            cropTop.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            cropTop.Name = "cropTop";
            cropTop.Size = new System.Drawing.Size(61, 23);
            cropTop.TabIndex = 19;
            cropTop.ValueChanged += cropTop_ValueChanged;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(220, 83);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(33, 15);
            label5.TabIndex = 18;
            label5.Text = "Crop";
            // 
            // brightnessPointPicker
            // 
            brightnessPointPicker.BackColor = System.Drawing.Color.Gray;
            brightnessPointPicker.Location = new System.Drawing.Point(116, 317);
            brightnessPointPicker.Name = "brightnessPointPicker";
            brightnessPointPicker.Size = new System.Drawing.Size(100, 100);
            brightnessPointPicker.TabIndex = 17;
            brightnessPointPicker.Text = "pointPicker2d1";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(48, 317);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(62, 15);
            label4.TabIndex = 16;
            label4.Text = "Brightness";
            // 
            // gammaLock
            // 
            gammaLock.AutoSize = true;
            gammaLock.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            gammaLock.Checked = true;
            gammaLock.CheckState = System.Windows.Forms.CheckState.Checked;
            gammaLock.Location = new System.Drawing.Point(48, 233);
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
            gammaB.Location = new System.Drawing.Point(104, 277);
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
            gammeG.Location = new System.Drawing.Point(104, 246);
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
            label3.Location = new System.Drawing.Point(48, 215);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(49, 15);
            label3.TabIndex = 8;
            label3.Text = "Gamma";
            // 
            // gammaR
            // 
            gammaR.AutoSize = false;
            gammaR.Location = new System.Drawing.Point(104, 215);
            gammaR.Maximum = 90;
            gammaR.Name = "gammaR";
            gammaR.Size = new System.Drawing.Size(214, 25);
            gammaR.TabIndex = 7;
            gammaR.TickFrequency = 30;
            gammaR.Value = 30;
            gammaR.Scroll += gammaR_Scroll;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(28, 166);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(73, 15);
            label2.TabIndex = 5;
            label2.Text = "BlurStrength";
            // 
            // trackBar1
            // 
            trackBar1.AutoSize = false;
            trackBar1.Location = new System.Drawing.Point(107, 162);
            trackBar1.Maximum = 50;
            trackBar1.Name = "trackBar1";
            trackBar1.Size = new System.Drawing.Size(214, 25);
            trackBar1.TabIndex = 4;
            trackBar1.TickFrequency = 5;
            trackBar1.Value = 15;
            trackBar1.Scroll += trackBar1_Scroll;
            // 
            // checkDebugOutputImage
            // 
            checkDebugOutputImage.AutoSize = true;
            checkDebugOutputImage.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            checkDebugOutputImage.Location = new System.Drawing.Point(355, 12);
            checkDebugOutputImage.Name = "checkDebugOutputImage";
            checkDebugOutputImage.Size = new System.Drawing.Size(136, 19);
            checkDebugOutputImage.TabIndex = 6;
            checkDebugOutputImage.Text = "Debug output image";
            checkDebugOutputImage.UseVisualStyleBackColor = true;
            // 
            // inputDebugPicture
            // 
            inputDebugPicture.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            inputDebugPicture.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            inputDebugPicture.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            inputDebugPicture.Location = new System.Drawing.Point(355, 207);
            inputDebugPicture.Name = "inputDebugPicture";
            inputDebugPicture.Size = new System.Drawing.Size(598, 503);
            inputDebugPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            inputDebugPicture.TabIndex = 7;
            inputDebugPicture.TabStop = false;
            // 
            // checkDebugInputImage
            // 
            checkDebugInputImage.AutoSize = true;
            checkDebugInputImage.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            checkDebugInputImage.Location = new System.Drawing.Point(355, 182);
            checkDebugInputImage.Name = "checkDebugInputImage";
            checkDebugInputImage.Size = new System.Drawing.Size(131, 19);
            checkDebugInputImage.TabIndex = 8;
            checkDebugInputImage.Text = "Debug input  image";
            checkDebugInputImage.UseVisualStyleBackColor = true;
            checkDebugInputImage.CheckedChanged += checkDebugInputImage_CheckedChanged;
            // 
            // FormDemo
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            ClientSize = new System.Drawing.Size(965, 722);
            Controls.Add(checkDebugInputImage);
            Controls.Add(inputDebugPicture);
            Controls.Add(groupBox);
            Controls.Add(outputDebugPicture);
            Controls.Add(checkDebugOutputImage);
            DoubleBuffered = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "FormDemo";
            Text = "Desktop Duplication API Demo";
            Shown += FormDemo_Shown;
            ((System.ComponentModel.ISupportInitialize)outputDebugPicture).EndInit();
            groupBox.ResumeLayout(false);
            groupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)cropLeft).EndInit();
            ((System.ComponentModel.ISupportInitialize)cropBottom).EndInit();
            ((System.ComponentModel.ISupportInitialize)cropRight).EndInit();
            ((System.ComponentModel.ISupportInitialize)cropTop).EndInit();
            ((System.ComponentModel.ISupportInitialize)gammaB).EndInit();
            ((System.ComponentModel.ISupportInitialize)gammeG).EndInit();
            ((System.ComponentModel.ISupportInitialize)gammaR).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBar1).EndInit();
            ((System.ComponentModel.ISupportInitialize)inputDebugPicture).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.CustomPictureBox outputDebugPicture;
        private System.Windows.Forms.ComboBox selectMonitorDropDown;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox sendWledDataCheckbox;
        private System.Windows.Forms.GroupBox groupBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.CheckBox checkDebugOutputImage;
        private System.Windows.Forms.CheckBox gammaLock;
        private System.Windows.Forms.TrackBar gammaB;
        private System.Windows.Forms.TrackBar gammeG;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TrackBar gammaR;
        private System.Windows.Forms.Label label4;
        private WledToolbox.PointPicker2D brightnessPointPicker;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CustomPictureBox inputDebugPicture;
        private System.Windows.Forms.CheckBox checkDebugInputImage;
        private System.Windows.Forms.NumericUpDown cropLeft;
        private System.Windows.Forms.NumericUpDown cropBottom;
        private System.Windows.Forms.NumericUpDown cropRight;
        private System.Windows.Forms.NumericUpDown cropTop;
    }
}

