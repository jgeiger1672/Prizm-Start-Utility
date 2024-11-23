namespace Prizm_Start_Utility
{
    partial class PrizmStartUtility
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
            startPrizmButton = new Button();
            existingVersionComboBox = new ComboBox();
            pasCheckbox = new CheckBox();
            useExistingVersionButton = new RadioButton();
            pullNewVersionButton = new RadioButton();
            newVersionTextBox = new TextBox();
            latestVersionButton = new RadioButton();
            adminPageButton = new Button();
            outputConsole = new RichTextBox();
            progressBar = new ProgressBar();
            configGroupBox = new GroupBox();
            licenseComboBox = new ComboBox();
            licenseLabel = new Label();
            libreButton = new RadioButton();
            msoButton = new RadioButton();
            versionGroupBox = new GroupBox();
            configGroupBox.SuspendLayout();
            versionGroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // startPrizmButton
            // 
            startPrizmButton.Enabled = false;
            startPrizmButton.Location = new Point(12, 358);
            startPrizmButton.Name = "startPrizmButton";
            startPrizmButton.Size = new Size(241, 46);
            startPrizmButton.TabIndex = 0;
            startPrizmButton.Text = "Start Prizm";
            startPrizmButton.UseVisualStyleBackColor = true;
            startPrizmButton.Click += Start_Prizm_Click;
            // 
            // existingVersionComboBox
            // 
            existingVersionComboBox.DropDownWidth = 146;
            existingVersionComboBox.FormattingEnabled = true;
            existingVersionComboBox.Location = new Point(269, 53);
            existingVersionComboBox.Name = "existingVersionComboBox";
            existingVersionComboBox.Size = new Size(146, 40);
            existingVersionComboBox.TabIndex = 1;
            existingVersionComboBox.SelectedIndexChanged += ExistingVersionComboBox_SelectedIndexChanged;
            // 
            // pasCheckbox
            // 
            pasCheckbox.AutoSize = true;
            pasCheckbox.Checked = true;
            pasCheckbox.CheckState = CheckState.Checked;
            pasCheckbox.Location = new Point(6, 237);
            pasCheckbox.Name = "pasCheckbox";
            pasCheckbox.Size = new Size(316, 36);
            pasCheckbox.TabIndex = 4;
            pasCheckbox.Text = "Start same version of PAS";
            pasCheckbox.UseVisualStyleBackColor = true;
            // 
            // useExistingVersionButton
            // 
            useExistingVersionButton.AutoSize = true;
            useExistingVersionButton.Checked = true;
            useExistingVersionButton.Location = new Point(6, 57);
            useExistingVersionButton.Name = "useExistingVersionButton";
            useExistingVersionButton.Size = new Size(257, 36);
            useExistingVersionButton.TabIndex = 5;
            useExistingVersionButton.TabStop = true;
            useExistingVersionButton.Text = "Use Existing Version";
            useExistingVersionButton.UseVisualStyleBackColor = true;
            useExistingVersionButton.CheckedChanged += UseExistingVersionButton_CheckedChanged;
            // 
            // pullNewVersionButton
            // 
            pullNewVersionButton.AutoSize = true;
            pullNewVersionButton.Location = new Point(6, 114);
            pullNewVersionButton.Name = "pullNewVersionButton";
            pullNewVersionButton.Size = new Size(224, 36);
            pullNewVersionButton.TabIndex = 6;
            pullNewVersionButton.Text = "Pull New Version";
            pullNewVersionButton.UseVisualStyleBackColor = true;
            pullNewVersionButton.CheckedChanged += PullNewVersionButton_CheckedChanged;
            // 
            // newVersionTextBox
            // 
            newVersionTextBox.Enabled = false;
            newVersionTextBox.Location = new Point(269, 113);
            newVersionTextBox.Name = "newVersionTextBox";
            newVersionTextBox.Size = new Size(146, 39);
            newVersionTextBox.TabIndex = 7;
            newVersionTextBox.Leave += NewVersionTextBox_Leave;
            // 
            // latestVersionButton
            // 
            latestVersionButton.AutoSize = true;
            latestVersionButton.Location = new Point(6, 173);
            latestVersionButton.Name = "latestVersionButton";
            latestVersionButton.Size = new Size(192, 36);
            latestVersionButton.TabIndex = 8;
            latestVersionButton.TabStop = true;
            latestVersionButton.Text = "Latest Version";
            latestVersionButton.UseVisualStyleBackColor = true;
            latestVersionButton.CheckedChanged += LatestVersionButton_CheckedChanged;
            // 
            // adminPageButton
            // 
            adminPageButton.Location = new Point(275, 358);
            adminPageButton.Name = "adminPageButton";
            adminPageButton.Size = new Size(239, 46);
            adminPageButton.TabIndex = 9;
            adminPageButton.Text = "Open Admin Page";
            adminPageButton.UseVisualStyleBackColor = true;
            adminPageButton.Click += AdminPageButton_Click;
            // 
            // outputConsole
            // 
            outputConsole.Location = new Point(12, 436);
            outputConsole.Name = "outputConsole";
            outputConsole.ReadOnly = true;
            outputConsole.Size = new Size(877, 192);
            outputConsole.TabIndex = 10;
            outputConsole.Text = "";
            // 
            // progressBar
            // 
            progressBar.Location = new Point(12, 649);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(877, 46);
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.TabIndex = 11;
            progressBar.Visible = false;
            // 
            // configGroupBox
            // 
            configGroupBox.Controls.Add(licenseComboBox);
            configGroupBox.Controls.Add(licenseLabel);
            configGroupBox.Controls.Add(libreButton);
            configGroupBox.Controls.Add(msoButton);
            configGroupBox.Location = new Point(460, 8);
            configGroupBox.Name = "configGroupBox";
            configGroupBox.Size = new Size(429, 317);
            configGroupBox.TabIndex = 12;
            configGroupBox.TabStop = false;
            configGroupBox.Text = "Configuration";
            // 
            // licenseComboBox
            // 
            licenseComboBox.DropDownWidth = 319;
            licenseComboBox.FormattingEnabled = true;
            licenseComboBox.Location = new Point(104, 53);
            licenseComboBox.Name = "licenseComboBox";
            licenseComboBox.Size = new Size(319, 40);
            licenseComboBox.TabIndex = 3;
            // 
            // licenseLabel
            // 
            licenseLabel.AutoSize = true;
            licenseLabel.Location = new Point(6, 59);
            licenseLabel.Name = "licenseLabel";
            licenseLabel.Size = new Size(92, 32);
            licenseLabel.TabIndex = 2;
            licenseLabel.Text = "License";
            // 
            // libreButton
            // 
            libreButton.AutoSize = true;
            libreButton.Location = new Point(114, 114);
            libreButton.Name = "libreButton";
            libreButton.Size = new Size(161, 36);
            libreButton.TabIndex = 1;
            libreButton.Text = "LibreOffice";
            libreButton.UseVisualStyleBackColor = true;
            // 
            // msoButton
            // 
            msoButton.AutoSize = true;
            msoButton.Checked = true;
            msoButton.Location = new Point(10, 114);
            msoButton.Name = "msoButton";
            msoButton.Size = new Size(98, 36);
            msoButton.TabIndex = 0;
            msoButton.TabStop = true;
            msoButton.Text = "MSO";
            msoButton.UseVisualStyleBackColor = true;
            // 
            // versionGroupBox
            // 
            versionGroupBox.Controls.Add(useExistingVersionButton);
            versionGroupBox.Controls.Add(existingVersionComboBox);
            versionGroupBox.Controls.Add(pasCheckbox);
            versionGroupBox.Controls.Add(pullNewVersionButton);
            versionGroupBox.Controls.Add(newVersionTextBox);
            versionGroupBox.Controls.Add(latestVersionButton);
            versionGroupBox.Location = new Point(12, 8);
            versionGroupBox.Name = "versionGroupBox";
            versionGroupBox.Size = new Size(432, 317);
            versionGroupBox.TabIndex = 13;
            versionGroupBox.TabStop = false;
            versionGroupBox.Text = "Version";
            // 
            // PrizmStartUtility
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(901, 708);
            Controls.Add(versionGroupBox);
            Controls.Add(configGroupBox);
            Controls.Add(progressBar);
            Controls.Add(outputConsole);
            Controls.Add(adminPageButton);
            Controls.Add(startPrizmButton);
            Name = "PrizmStartUtility";
            Text = "Prizm Start Utility";
            Click += PrizmStartUtility_Click;
            configGroupBox.ResumeLayout(false);
            configGroupBox.PerformLayout();
            versionGroupBox.ResumeLayout(false);
            versionGroupBox.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button startPrizmButton;
        private ComboBox existingVersionComboBox;
        private CheckBox pasCheckbox;
        private RadioButton useExistingVersionButton;
        private RadioButton pullNewVersionButton;
        private TextBox newVersionTextBox;
        private RadioButton latestVersionButton;
        private Button adminPageButton;
        private RichTextBox outputConsole;
        private ProgressBar progressBar;
        private GroupBox configGroupBox;
        private RadioButton libreButton;
        private RadioButton msoButton;
        private GroupBox versionGroupBox;
        private ComboBox licenseComboBox;
        private Label licenseLabel;
    }
}
