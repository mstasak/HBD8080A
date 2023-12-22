namespace GraphicsAdapter;

partial class frmGraphicsAdapter {
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
        if (disposing && (components != null)) {
            components.Dispose();
        }
        ShutdownThreads();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
        components = new System.ComponentModel.Container();
        toolStripContainer1 = new ToolStripContainer();
        picDisplayPanel = new PictureBox();
        toolStrip1 = new ToolStrip();
        btnAddRandomElement = new ToolStripButton();
        menuStrip1 = new MenuStrip();
        displayToolStripMenuItem = new ToolStripMenuItem();
        modeToolStripMenuItem = new ToolStripMenuItem();
        text25x80ToolStripMenuItem = new ToolStripMenuItem();
        text43x80ToolStripMenuItem = new ToolStripMenuItem();
        text50x80ToolStripMenuItem = new ToolStripMenuItem();
        text50x132ToolStripMenuItem = new ToolStripMenuItem();
        x200ToolStripMenuItem = new ToolStripMenuItem();
        x200ToolStripMenuItem1 = new ToolStripMenuItem();
        x400ToolStripMenuItem = new ToolStripMenuItem();
        x480ToolStripMenuItem = new ToolStripMenuItem();
        x600ToolStripMenuItem = new ToolStripMenuItem();
        x768ToolStripMenuItem = new ToolStripMenuItem();
        x1024ToolStripMenuItem = new ToolStripMenuItem();
        x1200ToolStripMenuItem = new ToolStripMenuItem();
        x1600ToolStripMenuItem = new ToolStripMenuItem();
        x768ToolStripMenuItem1 = new ToolStripMenuItem();
        x1080ToolStripMenuItem = new ToolStripMenuItem();
        x2160ToolStripMenuItem = new ToolStripMenuItem();
        colorsToolStripMenuItem = new ToolStripMenuItem();
        toolStripMenuItem2 = new ToolStripMenuItem();
        toolStripMenuItem3 = new ToolStripMenuItem();
        toolStripMenuItem4 = new ToolStripMenuItem();
        toolStripMenuItem5 = new ToolStripMenuItem();
        mToolStripMenuItem = new ToolStripMenuItem();
        bitRGBAToolStripMenuItem = new ToolStripMenuItem();
        paletteToolStripMenuItem = new ToolStripMenuItem();
        monochromeWhiteToolStripMenuItem = new ToolStripMenuItem();
        monochromeGreenToolStripMenuItem = new ToolStripMenuItem();
        monochromeAmberToolStripMenuItem = new ToolStripMenuItem();
        cGA1ToolStripMenuItem = new ToolStripMenuItem();
        cGA2ToolStripMenuItem = new ToolStripMenuItem();
        vGA256ToolStripMenuItem = new ToolStripMenuItem();
        highcolorToolStripMenuItem = new ToolStripMenuItem();
        trueColor24BitToolStripMenuItem = new ToolStripMenuItem();
        rGBA32BitToolStripMenuItem = new ToolStripMenuItem();
        toolStripSeparator2 = new ToolStripSeparator();
        resetToolStripMenuItem = new ToolStripMenuItem();
        toolStripSeparator1 = new ToolStripSeparator();
        exitToolStripMenuItem = new ToolStripMenuItem();
        timer1 = new System.Windows.Forms.Timer(components);
        toolStripContainer1.ContentPanel.SuspendLayout();
        toolStripContainer1.TopToolStripPanel.SuspendLayout();
        toolStripContainer1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picDisplayPanel).BeginInit();
        toolStrip1.SuspendLayout();
        menuStrip1.SuspendLayout();
        SuspendLayout();
        // 
        // toolStripContainer1
        // 
        toolStripContainer1.BottomToolStripPanelVisible = false;
        // 
        // toolStripContainer1.ContentPanel
        // 
        toolStripContainer1.ContentPanel.Controls.Add(picDisplayPanel);
        toolStripContainer1.ContentPanel.Size = new Size(1024, 768);
        toolStripContainer1.Dock = DockStyle.Fill;
        toolStripContainer1.LeftToolStripPanelVisible = false;
        toolStripContainer1.Location = new Point(0, 0);
        toolStripContainer1.Name = "toolStripContainer1";
        toolStripContainer1.RightToolStripPanelVisible = false;
        toolStripContainer1.Size = new Size(1024, 823);
        toolStripContainer1.TabIndex = 1;
        toolStripContainer1.Text = "toolStripContainer1";
        // 
        // toolStripContainer1.TopToolStripPanel
        // 
        toolStripContainer1.TopToolStripPanel.Controls.Add(toolStrip1);
        toolStripContainer1.TopToolStripPanel.Controls.Add(menuStrip1);
        // 
        // picDisplayPanel
        // 
        picDisplayPanel.Dock = DockStyle.Fill;
        picDisplayPanel.Location = new Point(0, 0);
        picDisplayPanel.Name = "picDisplayPanel";
        picDisplayPanel.Size = new Size(1024, 768);
        picDisplayPanel.TabIndex = 0;
        picDisplayPanel.TabStop = false;
        picDisplayPanel.Click += BtnAddRandomElement_Click;
        // 
        // toolStrip1
        // 
        toolStrip1.AllowMerge = false;
        toolStrip1.Dock = DockStyle.None;
        toolStrip1.ImageScalingSize = new Size(20, 20);
        toolStrip1.Items.AddRange(new ToolStripItem[] { btnAddRandomElement });
        toolStrip1.Location = new Point(4, 0);
        toolStrip1.Name = "toolStrip1";
        toolStrip1.Size = new Size(172, 27);
        toolStrip1.TabIndex = 1;
        // 
        // btnAddRandomElement
        // 
        btnAddRandomElement.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnAddRandomElement.ImageTransparentColor = Color.Magenta;
        btnAddRandomElement.Name = "btnAddRandomElement";
        btnAddRandomElement.Size = new Size(159, 24);
        btnAddRandomElement.Text = "Add Random Element";
        btnAddRandomElement.TextImageRelation = TextImageRelation.TextBeforeImage;
        btnAddRandomElement.Click += BtnAddRandomElement_Click;
        // 
        // menuStrip1
        // 
        menuStrip1.Dock = DockStyle.None;
        menuStrip1.ImageScalingSize = new Size(20, 20);
        menuStrip1.Items.AddRange(new ToolStripItem[] { displayToolStripMenuItem });
        menuStrip1.Location = new Point(4, 27);
        menuStrip1.Name = "menuStrip1";
        menuStrip1.Size = new Size(80, 28);
        menuStrip1.Stretch = false;
        menuStrip1.TabIndex = 0;
        menuStrip1.Text = "menuStrip1";
        // 
        // displayToolStripMenuItem
        // 
        displayToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { modeToolStripMenuItem, colorsToolStripMenuItem, paletteToolStripMenuItem, toolStripSeparator2, resetToolStripMenuItem, toolStripSeparator1, exitToolStripMenuItem });
        displayToolStripMenuItem.Name = "displayToolStripMenuItem";
        displayToolStripMenuItem.Size = new Size(72, 24);
        displayToolStripMenuItem.Text = "Display";
        // 
        // modeToolStripMenuItem
        // 
        modeToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { text25x80ToolStripMenuItem, text43x80ToolStripMenuItem, text50x80ToolStripMenuItem, text50x132ToolStripMenuItem, x200ToolStripMenuItem, x200ToolStripMenuItem1, x400ToolStripMenuItem, x480ToolStripMenuItem, x600ToolStripMenuItem, x768ToolStripMenuItem, x1024ToolStripMenuItem, x1200ToolStripMenuItem, x1600ToolStripMenuItem, x768ToolStripMenuItem1, x1080ToolStripMenuItem, x2160ToolStripMenuItem });
        modeToolStripMenuItem.Name = "modeToolStripMenuItem";
        modeToolStripMenuItem.Size = new Size(137, 26);
        modeToolStripMenuItem.Text = "Mode";
        // 
        // text25x80ToolStripMenuItem
        // 
        text25x80ToolStripMenuItem.Name = "text25x80ToolStripMenuItem";
        text25x80ToolStripMenuItem.Size = new Size(170, 26);
        text25x80ToolStripMenuItem.Text = "Text 25x80";
        // 
        // text43x80ToolStripMenuItem
        // 
        text43x80ToolStripMenuItem.Name = "text43x80ToolStripMenuItem";
        text43x80ToolStripMenuItem.Size = new Size(170, 26);
        text43x80ToolStripMenuItem.Text = "Text 43x80";
        // 
        // text50x80ToolStripMenuItem
        // 
        text50x80ToolStripMenuItem.Name = "text50x80ToolStripMenuItem";
        text50x80ToolStripMenuItem.Size = new Size(170, 26);
        text50x80ToolStripMenuItem.Text = "Text 50x80";
        // 
        // text50x132ToolStripMenuItem
        // 
        text50x132ToolStripMenuItem.Name = "text50x132ToolStripMenuItem";
        text50x132ToolStripMenuItem.Size = new Size(170, 26);
        text50x132ToolStripMenuItem.Text = "Text 50x132";
        // 
        // x200ToolStripMenuItem
        // 
        x200ToolStripMenuItem.Name = "x200ToolStripMenuItem";
        x200ToolStripMenuItem.Size = new Size(170, 26);
        x200ToolStripMenuItem.Text = "320x200";
        // 
        // x200ToolStripMenuItem1
        // 
        x200ToolStripMenuItem1.Name = "x200ToolStripMenuItem1";
        x200ToolStripMenuItem1.Size = new Size(170, 26);
        x200ToolStripMenuItem1.Text = "640x200";
        // 
        // x400ToolStripMenuItem
        // 
        x400ToolStripMenuItem.Name = "x400ToolStripMenuItem";
        x400ToolStripMenuItem.Size = new Size(170, 26);
        x400ToolStripMenuItem.Text = "640x400";
        // 
        // x480ToolStripMenuItem
        // 
        x480ToolStripMenuItem.Name = "x480ToolStripMenuItem";
        x480ToolStripMenuItem.Size = new Size(170, 26);
        x480ToolStripMenuItem.Text = "640x480";
        // 
        // x600ToolStripMenuItem
        // 
        x600ToolStripMenuItem.Name = "x600ToolStripMenuItem";
        x600ToolStripMenuItem.Size = new Size(170, 26);
        x600ToolStripMenuItem.Text = "800x600";
        // 
        // x768ToolStripMenuItem
        // 
        x768ToolStripMenuItem.Name = "x768ToolStripMenuItem";
        x768ToolStripMenuItem.Size = new Size(170, 26);
        x768ToolStripMenuItem.Text = "1024x768";
        // 
        // x1024ToolStripMenuItem
        // 
        x1024ToolStripMenuItem.Name = "x1024ToolStripMenuItem";
        x1024ToolStripMenuItem.Size = new Size(170, 26);
        x1024ToolStripMenuItem.Text = "1280x1024";
        // 
        // x1200ToolStripMenuItem
        // 
        x1200ToolStripMenuItem.Name = "x1200ToolStripMenuItem";
        x1200ToolStripMenuItem.Size = new Size(170, 26);
        x1200ToolStripMenuItem.Text = "1600x1200";
        // 
        // x1600ToolStripMenuItem
        // 
        x1600ToolStripMenuItem.Name = "x1600ToolStripMenuItem";
        x1600ToolStripMenuItem.Size = new Size(170, 26);
        x1600ToolStripMenuItem.Text = "2560x1600";
        // 
        // x768ToolStripMenuItem1
        // 
        x768ToolStripMenuItem1.Name = "x768ToolStripMenuItem1";
        x768ToolStripMenuItem1.Size = new Size(170, 26);
        x768ToolStripMenuItem1.Text = "1368x768";
        // 
        // x1080ToolStripMenuItem
        // 
        x1080ToolStripMenuItem.Name = "x1080ToolStripMenuItem";
        x1080ToolStripMenuItem.Size = new Size(170, 26);
        x1080ToolStripMenuItem.Text = "1920x1080";
        // 
        // x2160ToolStripMenuItem
        // 
        x2160ToolStripMenuItem.Name = "x2160ToolStripMenuItem";
        x2160ToolStripMenuItem.Size = new Size(170, 26);
        x2160ToolStripMenuItem.Text = "3840x2160";
        // 
        // colorsToolStripMenuItem
        // 
        colorsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItem2, toolStripMenuItem3, toolStripMenuItem4, toolStripMenuItem5, mToolStripMenuItem, bitRGBAToolStripMenuItem });
        colorsToolStripMenuItem.Name = "colorsToolStripMenuItem";
        colorsToolStripMenuItem.Size = new Size(137, 26);
        colorsToolStripMenuItem.Text = "Colors";
        // 
        // toolStripMenuItem2
        // 
        toolStripMenuItem2.Name = "toolStripMenuItem2";
        toolStripMenuItem2.Size = new Size(168, 26);
        toolStripMenuItem2.Text = "2";
        // 
        // toolStripMenuItem3
        // 
        toolStripMenuItem3.Name = "toolStripMenuItem3";
        toolStripMenuItem3.Size = new Size(168, 26);
        toolStripMenuItem3.Text = "16";
        // 
        // toolStripMenuItem4
        // 
        toolStripMenuItem4.Name = "toolStripMenuItem4";
        toolStripMenuItem4.Size = new Size(168, 26);
        toolStripMenuItem4.Text = "256";
        // 
        // toolStripMenuItem5
        // 
        toolStripMenuItem5.Name = "toolStripMenuItem5";
        toolStripMenuItem5.Size = new Size(168, 26);
        toolStripMenuItem5.Text = "65536";
        // 
        // mToolStripMenuItem
        // 
        mToolStripMenuItem.Name = "mToolStripMenuItem";
        mToolStripMenuItem.Size = new Size(168, 26);
        mToolStripMenuItem.Text = "24M";
        // 
        // bitRGBAToolStripMenuItem
        // 
        bitRGBAToolStripMenuItem.Name = "bitRGBAToolStripMenuItem";
        bitRGBAToolStripMenuItem.Size = new Size(168, 26);
        bitRGBAToolStripMenuItem.Text = "32bit RGBA";
        // 
        // paletteToolStripMenuItem
        // 
        paletteToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { monochromeWhiteToolStripMenuItem, monochromeGreenToolStripMenuItem, monochromeAmberToolStripMenuItem, cGA1ToolStripMenuItem, cGA2ToolStripMenuItem, vGA256ToolStripMenuItem, highcolorToolStripMenuItem, trueColor24BitToolStripMenuItem, rGBA32BitToolStripMenuItem });
        paletteToolStripMenuItem.Name = "paletteToolStripMenuItem";
        paletteToolStripMenuItem.Size = new Size(137, 26);
        paletteToolStripMenuItem.Text = "Palette";
        // 
        // monochromeWhiteToolStripMenuItem
        // 
        monochromeWhiteToolStripMenuItem.Name = "monochromeWhiteToolStripMenuItem";
        monochromeWhiteToolStripMenuItem.Size = new Size(230, 26);
        monochromeWhiteToolStripMenuItem.Text = "Monochrome White";
        // 
        // monochromeGreenToolStripMenuItem
        // 
        monochromeGreenToolStripMenuItem.Name = "monochromeGreenToolStripMenuItem";
        monochromeGreenToolStripMenuItem.Size = new Size(230, 26);
        monochromeGreenToolStripMenuItem.Text = "Monochrome Green";
        // 
        // monochromeAmberToolStripMenuItem
        // 
        monochromeAmberToolStripMenuItem.Name = "monochromeAmberToolStripMenuItem";
        monochromeAmberToolStripMenuItem.Size = new Size(230, 26);
        monochromeAmberToolStripMenuItem.Text = "Monochrome Amber";
        // 
        // cGA1ToolStripMenuItem
        // 
        cGA1ToolStripMenuItem.Name = "cGA1ToolStripMenuItem";
        cGA1ToolStripMenuItem.Size = new Size(230, 26);
        cGA1ToolStripMenuItem.Text = "CGA1";
        // 
        // cGA2ToolStripMenuItem
        // 
        cGA2ToolStripMenuItem.Name = "cGA2ToolStripMenuItem";
        cGA2ToolStripMenuItem.Size = new Size(230, 26);
        cGA2ToolStripMenuItem.Text = "CGA2";
        // 
        // vGA256ToolStripMenuItem
        // 
        vGA256ToolStripMenuItem.Name = "vGA256ToolStripMenuItem";
        vGA256ToolStripMenuItem.Size = new Size(230, 26);
        vGA256ToolStripMenuItem.Text = "VGA 256";
        // 
        // highcolorToolStripMenuItem
        // 
        highcolorToolStripMenuItem.Name = "highcolorToolStripMenuItem";
        highcolorToolStripMenuItem.Size = new Size(230, 26);
        highcolorToolStripMenuItem.Text = "Highcolor 16 bit";
        // 
        // trueColor24BitToolStripMenuItem
        // 
        trueColor24BitToolStripMenuItem.Name = "trueColor24BitToolStripMenuItem";
        trueColor24BitToolStripMenuItem.Size = new Size(230, 26);
        trueColor24BitToolStripMenuItem.Text = "TrueColor 24 bit";
        // 
        // rGBA32BitToolStripMenuItem
        // 
        rGBA32BitToolStripMenuItem.Name = "rGBA32BitToolStripMenuItem";
        rGBA32BitToolStripMenuItem.Size = new Size(230, 26);
        rGBA32BitToolStripMenuItem.Text = "RGBA 32 bit";
        // 
        // toolStripSeparator2
        // 
        toolStripSeparator2.Name = "toolStripSeparator2";
        toolStripSeparator2.Size = new Size(134, 6);
        // 
        // resetToolStripMenuItem
        // 
        resetToolStripMenuItem.Name = "resetToolStripMenuItem";
        resetToolStripMenuItem.Size = new Size(137, 26);
        resetToolStripMenuItem.Text = "Reset";
        // 
        // toolStripSeparator1
        // 
        toolStripSeparator1.Name = "toolStripSeparator1";
        toolStripSeparator1.Size = new Size(134, 6);
        // 
        // exitToolStripMenuItem
        // 
        exitToolStripMenuItem.Name = "exitToolStripMenuItem";
        exitToolStripMenuItem.Size = new Size(137, 26);
        exitToolStripMenuItem.Text = "Exit";
        // 
        // timer1
        // 
        timer1.Enabled = true;
        timer1.Interval = 50;
        timer1.Tick += timer1_Tick;
        // 
        // frmGraphicsAdapter
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1024, 823);
        Controls.Add(toolStripContainer1);
        KeyPreview = true;
        Name = "frmGraphicsAdapter";
        Text = "Form1";
        Load += frmGraphicsAdapter_Load;
        KeyPress += frmGraphicsAdapter_KeyPress;
        toolStripContainer1.ContentPanel.ResumeLayout(false);
        toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
        toolStripContainer1.TopToolStripPanel.PerformLayout();
        toolStripContainer1.ResumeLayout(false);
        toolStripContainer1.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)picDisplayPanel).EndInit();
        toolStrip1.ResumeLayout(false);
        toolStrip1.PerformLayout();
        menuStrip1.ResumeLayout(false);
        menuStrip1.PerformLayout();
        ResumeLayout(false);
    }

    #endregion
    private ToolStripContainer toolStripContainer1;
    private MenuStrip menuStrip1;
    private ToolStrip toolStrip1;
    private ToolStripButton btnAddRandomElement;
    private PictureBox picDisplayPanel;
    private ToolStripMenuItem displayToolStripMenuItem;
    private ToolStripMenuItem modeToolStripMenuItem;
    private ToolStripMenuItem text25x80ToolStripMenuItem;
    private ToolStripMenuItem text43x80ToolStripMenuItem;
    private ToolStripMenuItem text50x80ToolStripMenuItem;
    private ToolStripMenuItem text50x132ToolStripMenuItem;
    private ToolStripMenuItem x200ToolStripMenuItem;
    private ToolStripMenuItem x200ToolStripMenuItem1;
    private ToolStripMenuItem x400ToolStripMenuItem;
    private ToolStripMenuItem x480ToolStripMenuItem;
    private ToolStripMenuItem x600ToolStripMenuItem;
    private ToolStripMenuItem x768ToolStripMenuItem;
    private ToolStripMenuItem x1024ToolStripMenuItem;
    private ToolStripMenuItem x1200ToolStripMenuItem;
    private ToolStripMenuItem x1600ToolStripMenuItem;
    private ToolStripMenuItem x768ToolStripMenuItem1;
    private ToolStripMenuItem x1080ToolStripMenuItem;
    private ToolStripMenuItem x2160ToolStripMenuItem;
    private ToolStripMenuItem colorsToolStripMenuItem;
    private ToolStripMenuItem toolStripMenuItem2;
    private ToolStripMenuItem toolStripMenuItem3;
    private ToolStripMenuItem toolStripMenuItem4;
    private ToolStripMenuItem toolStripMenuItem5;
    private ToolStripMenuItem mToolStripMenuItem;
    private ToolStripMenuItem bitRGBAToolStripMenuItem;
    private ToolStripMenuItem paletteToolStripMenuItem;
    private ToolStripMenuItem monochromeWhiteToolStripMenuItem;
    private ToolStripMenuItem monochromeGreenToolStripMenuItem;
    private ToolStripMenuItem monochromeAmberToolStripMenuItem;
    private ToolStripMenuItem cGA1ToolStripMenuItem;
    private ToolStripMenuItem cGA2ToolStripMenuItem;
    private ToolStripMenuItem vGA256ToolStripMenuItem;
    private ToolStripMenuItem highcolorToolStripMenuItem;
    private ToolStripMenuItem trueColor24BitToolStripMenuItem;
    private ToolStripMenuItem rGBA32BitToolStripMenuItem;
    private ToolStripSeparator toolStripSeparator2;
    private ToolStripMenuItem resetToolStripMenuItem;
    private ToolStripSeparator toolStripSeparator1;
    private ToolStripMenuItem exitToolStripMenuItem;
    private System.Windows.Forms.Timer timer1;
}
