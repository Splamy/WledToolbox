using ShaderTests.DdList;

namespace ShaderTests;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        customPictureBox1 = new CustomPictureBox();
        listBox1 = new ListBox();
        dragDropListBox1 = new DragDropListBox();
        selectMonitorDropDown = new ComboBox();
        panel1 = new Panel();
        button1 = new Button();
        button2 = new Button();
        ((System.ComponentModel.ISupportInitialize)customPictureBox1).BeginInit();
        SuspendLayout();
        // 
        // customPictureBox1
        // 
        customPictureBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        customPictureBox1.BorderStyle = BorderStyle.FixedSingle;
        customPictureBox1.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
        customPictureBox1.Location = new Point(627, 12);
        customPictureBox1.Name = "customPictureBox1";
        customPictureBox1.Size = new Size(716, 633);
        customPictureBox1.TabIndex = 0;
        customPictureBox1.TabStop = false;
        // 
        // listBox1
        // 
        listBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
        listBox1.FormattingEnabled = true;
        listBox1.ItemHeight = 15;
        listBox1.Location = new Point(12, 42);
        listBox1.Name = "listBox1";
        listBox1.Size = new Size(179, 604);
        listBox1.TabIndex = 1;
        listBox1.DoubleClick += listBox1_DoubleClick;
        // 
        // dragDropListBox1
        // 
        dragDropListBox1.AllowDrop = true;
        dragDropListBox1.FormattingEnabled = true;
        dragDropListBox1.ItemHeight = 15;
        dragDropListBox1.Location = new Point(197, 42);
        dragDropListBox1.Name = "dragDropListBox1";
        dragDropListBox1.Size = new Size(424, 184);
        dragDropListBox1.TabIndex = 2;
        dragDropListBox1.SelectedValueChanged += dragDropListBox1_SelectedValueChanged;
        // 
        // selectMonitorDropDown
        // 
        selectMonitorDropDown.DropDownStyle = ComboBoxStyle.DropDownList;
        selectMonitorDropDown.FormattingEnabled = true;
        selectMonitorDropDown.Location = new Point(12, 12);
        selectMonitorDropDown.Name = "selectMonitorDropDown";
        selectMonitorDropDown.Size = new Size(179, 23);
        selectMonitorDropDown.TabIndex = 3;
        selectMonitorDropDown.SelectedIndexChanged += selectMonitorDropDown_SelectedIndexChanged;
        // 
        // panel1
        // 
        panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
        panel1.BorderStyle = BorderStyle.FixedSingle;
        panel1.Location = new Point(197, 232);
        panel1.Name = "panel1";
        panel1.Size = new Size(424, 412);
        panel1.TabIndex = 4;
        // 
        // button1
        // 
        button1.Location = new Point(197, 11);
        button1.Name = "button1";
        button1.Size = new Size(85, 23);
        button1.TabIndex = 5;
        button1.Text = "Save";
        button1.UseVisualStyleBackColor = true;
        button1.Click += button1_Click;
        // 
        // button2
        // 
        button2.Location = new Point(288, 11);
        button2.Name = "button2";
        button2.Size = new Size(85, 23);
        button2.TabIndex = 6;
        button2.Text = "Load";
        button2.UseVisualStyleBackColor = true;
        button2.Click += button2_Click;
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1355, 657);
        Controls.Add(button2);
        Controls.Add(button1);
        Controls.Add(panel1);
        Controls.Add(selectMonitorDropDown);
        Controls.Add(dragDropListBox1);
        Controls.Add(listBox1);
        Controls.Add(customPictureBox1);
        Name = "Form1";
        Text = "Form1";
        Shown += Form1_Show;
        ((System.ComponentModel.ISupportInitialize)customPictureBox1).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private CustomPictureBox customPictureBox1;
    private ListBox listBox1;
    private DragDropListBox dragDropListBox1;
    private ComboBox selectMonitorDropDown;
    private Panel panel1;
    private Button button1;
    private Button button2;
}
