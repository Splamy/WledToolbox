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
            checkDebugOutputImage = new System.Windows.Forms.CheckBox();
            label2 = new System.Windows.Forms.Label();
            trackBar1 = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            groupBox1.SuspendLayout();
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
            pictureBox2.Size = new System.Drawing.Size(527, 326);
            pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
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
            groupBox1.Controls.Add(checkDebugOutputImage);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(trackBar1);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(selectMonitorDropDown);
            groupBox1.Controls.Add(sendWledDataCheckbox);
            groupBox1.Location = new System.Drawing.Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(337, 326);
            groupBox1.TabIndex = 5;
            groupBox1.TabStop = false;
            groupBox1.Text = "groupBox1";
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
            trackBar1.Location = new System.Drawing.Point(104, 96);
            trackBar1.Maximum = 20;
            trackBar1.Name = "trackBar1";
            trackBar1.Size = new System.Drawing.Size(214, 45);
            trackBar1.TabIndex = 4;
            trackBar1.Value = 3;
            trackBar1.Scroll += trackBar1_Scroll;
            // 
            // FormDemo
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            ClientSize = new System.Drawing.Size(894, 350);
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
    }
}

