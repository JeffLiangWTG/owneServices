using System;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefDataForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			loadButton = new ZButton();
			zLabel1 = new ZLabel();
			zLabel2 = new ZLabel();
			dataSetUpdaterComboBox = new CargoWise.Windows.UI.KComboBox();
			dataSetVersionComboBox = new CargoWise.Windows.UI.KComboBox();
			outputTextBox = new ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();

			SuspendLayout();

			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 376, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 24, true);

			// 
			// loadButton
			// 
			this.loadButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.loadButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(430, 24, true);
			this.loadButton.Name = "loadButton";
			this.loadButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.loadButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 30, true);
			this.loadButton.TabIndex = 6;
			this.loadButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0c5bbf76-7fcd-43b5-80ef-30cb7ad0855e", "Load");
			this.loadButton.ToolTipCaption = null;
			this.loadButton.UseVisualStyleBackColor = true;
			this.loadButton.Click += new System.EventHandler(this.loadButton_Click);

			// 
			// zLabel1
			// 
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 13, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.zLabel1.TabIndex = 2;
			this.zLabel1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e9e51a77-2be0-4258-8428-c43207c64037", "Data Set Updater:");
			this.zLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

			// 
			// zLabel2
			// 
			this.zLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 43, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.zLabel2.TabIndex = 4;
			this.zLabel2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4795976f-f137-48cf-94cc-c1980ee7398f", "Data Set Version:");
			this.zLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

			// 
			// dataSetUpdaterComboBox
			// 
			this.dataSetUpdaterComboBox.AllowDrop = true;
			this.dataSetUpdaterComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.dataSetUpdaterComboBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 13, true);
			this.dataSetUpdaterComboBox.Name = "dataSetUpdaterComboBox";
			this.dataSetUpdaterComboBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 21, true);
			this.dataSetUpdaterComboBox.TabIndex = 3;
			this.dataSetUpdaterComboBox.SelectedIndexChanged += new System.EventHandler(this.dataSetUpdaterComboBox_Changed);

			// 
			// dataSetVersionComboBox
			// 
			this.dataSetVersionComboBox.AllowDrop = true;
			this.dataSetVersionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.dataSetVersionComboBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 43, true);
			this.dataSetVersionComboBox.Name = "dataSetVersionComboBox";
			this.dataSetVersionComboBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 21, true);
			this.dataSetVersionComboBox.TabIndex = 5;

			// 
			// outputTextBox
			// 
			this.outputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.outputTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("09cb8f5c-75cd-44d5-9d10-4647bd82a85a", "Output");
			this.outputTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.outputTextBox.Font = new System.Drawing.Font("Courier New", 8.25F);
			this.outputTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 105, true);
			this.outputTextBox.Multiline = true;
			this.outputTextBox.Name = "outputTextBox";
			this.outputTextBox.ReadOnly = true;
			this.outputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.outputTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 255, true);
			this.outputTextBox.TabIndex = 8;

			// 
			// RefDataForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 400, true);
			this.Controls.Add(this.outputTextBox);
			this.Controls.Add(this.dataSetVersionComboBox);
			this.Controls.Add(this.dataSetUpdaterComboBox);
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.loadButton);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 400, true);
			this.Name = "RefDataForm";
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f75ac325-0b76-4ff1-b082-f7e16c45df10", "Ref Data");
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.loadButton, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.zLabel2, 0);
			this.Controls.SetChildIndex(this.dataSetUpdaterComboBox, 0);
			this.Controls.SetChildIndex(this.dataSetVersionComboBox, 0);
			this.Controls.SetChildIndex(this.outputTextBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		#region Components
		private ZButton loadButton;
		private ZLabel zLabel1;
		private ZLabel zLabel2;
		private CargoWise.Windows.UI.KComboBox dataSetUpdaterComboBox;
		private CargoWise.Windows.UI.KComboBox dataSetVersionComboBox;
		private ZTextBox outputTextBox;
		#endregion
	}
}
