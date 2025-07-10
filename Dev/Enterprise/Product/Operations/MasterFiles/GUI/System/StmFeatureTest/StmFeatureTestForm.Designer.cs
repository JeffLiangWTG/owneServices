using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	partial class StmFeatureTestForm
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
		new void InitializeComponent()
		{
			this.globalGroupsFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.postingButtons = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.categoryLabel = new Enterprise.ZArchitecture.ZLabel();
			this.featureLabel = new Enterprise.ZArchitecture.ZLabel();
			this.globalGroupLabel = new Enterprise.ZArchitecture.ZLabel();
			this.activeCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.categoryValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.featureNameDropEditWithFixedWidth = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.globalGroupsFindBox.SuspendLayout();
			this.postingButtons.SuspendLayout();
			this.featureNameDropEditWithFixedWidth.SuspendLayout();
			this.SuspendLayout();
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 495, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 24, true);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.StmFeatureTest);
			//
			// globalGroupsFindBox
			//
			this.globalGroupsFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.globalGroupsFindBox, "SFT_GG_Group");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.StmFeatureTest)(null)).SFT_GG_Group)));
			this.globalGroupsFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 90, true);
			this.globalGroupsFindBox.Name = "globalGroupsFindBox";
			this.globalGroupsFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.globalGroupsFindBox.ParentType = null;
			this.globalGroupsFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 20, true);
			this.globalGroupsFindBox.TabIndex = 1;
			//
			// postingButtons
			//
			this.postingButtons.AllowDrop = true;
			this.postingButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.postingButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(559, 465, true);
			this.postingButtons.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.postingButtons.Name = "postingButtons";
			this.postingButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.postingButtons.TabIndex = 2;
			//
			// categoryLabel
			//
			this.categoryLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.categoryLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 10, true);
			this.categoryLabel.Name = "categoryLabel";
			this.categoryLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.categoryLabel.TabIndex = 5;
			this.categoryLabel.CaptionResourceString = Res.GetData("StmFeatureTestForm|CategoryLabel", "Category");
			//
			// featureLabel
			//
			this.featureLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.featureLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 50, true);
			this.featureLabel.Name = "featureLabel";
			this.featureLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.featureLabel.TabIndex = 6;
			this.featureLabel.CaptionResourceString = Res.GetData("StmFeatureTestForm|FeatureLabel", "Feature");
			//
			// globalGroupLabel
			//
			this.globalGroupLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.globalGroupLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 90, true);
			this.globalGroupLabel.Name = "globalGroupLabel";
			this.globalGroupLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.globalGroupLabel.TabIndex = 7;
			this.globalGroupLabel.CaptionResourceString = Res.GetData("StmFeatureTestForm|GroupLabel", "Target Group");
			//
			// activeCheckbox
			//
			this.BindingSource.SetBindingMember(this.activeCheckbox, "SFT_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.StmFeatureTest)(null)).SFT_IsActive)));
			this.activeCheckbox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.activeCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 129, true);
			this.activeCheckbox.Name = "activeCheckbox";
			this.activeCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 24, true);
			this.activeCheckbox.TabIndex = 8;
			this.activeCheckbox.CaptionResourceString = Res.GetData("StmFeatureTestForm|IsActiveCheckbox", "Is Active");
			this.activeCheckbox.UseVisualStyleBackColor = true;
			//
			// categoryValueLabel
			//
			this.categoryValueLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.categoryValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 10, true);
			this.categoryValueLabel.Name = "categoryValueLabel";
			this.categoryValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.categoryValueLabel.TabIndex = 9;
			this.categoryValueLabel.CaptionResourceString = Res.GetData("StmFeatureTestForm|WinzorCategoryName", StmFeatureTest.WinzorFeatureCode);
			//
			// featureNameDropEditWithFixedWidth
			//
			this.featureNameDropEditWithFixedWidth.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.featureNameDropEditWithFixedWidth, "SFT_FeatureName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.StmFeatureTest)(null)).SFT_FeatureName)));
			this.featureNameDropEditWithFixedWidth.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 52, true);
			this.featureNameDropEditWithFixedWidth.Name = "featureNameDropEditWithFixedWidth";
			this.featureNameDropEditWithFixedWidth.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.featureNameDropEditWithFixedWidth.TabIndex = 10;
			this.featureNameDropEditWithFixedWidth.CaptionResourceString = Res.GetData("StmFeatureTestForm|FeatureNameDropEdit", "Feature Name");
			//
			// StmFeatureTestForm
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 519, true);
			this.Controls.Add(this.featureNameDropEditWithFixedWidth);
			this.Controls.Add(this.categoryValueLabel);
			this.Controls.Add(this.activeCheckbox);
			this.Controls.Add(this.globalGroupLabel);
			this.Controls.Add(this.featureLabel);
			this.Controls.Add(this.categoryLabel);
			this.Controls.Add(this.postingButtons);
			this.Controls.Add(this.globalGroupsFindBox);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.StmFeatureTest);
			this.Name = "StmFeatureTestForm";
			this.CaptionResourceString = Res.GetData("StmFeatureTestForm|FormName", "Feature Test Form");
			this.Controls.SetChildIndex(this.globalGroupsFindBox, 0);
			this.Controls.SetChildIndex(this.postingButtons, 0);
			this.Controls.SetChildIndex(this.categoryLabel, 0);
			this.Controls.SetChildIndex(this.featureLabel, 0);
			this.Controls.SetChildIndex(this.globalGroupLabel, 0);
			this.Controls.SetChildIndex(this.activeCheckbox, 0);
			this.Controls.SetChildIndex(this.categoryValueLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.featureNameDropEditWithFixedWidth, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.globalGroupsFindBox.ResumeLayout(true);
			this.globalGroupsFindBox.PerformLayout();
			this.postingButtons.ResumeLayout(true);
			this.postingButtons.PerformLayout();
			this.featureNameDropEditWithFixedWidth.ResumeLayout(true);
			this.featureNameDropEditWithFixedWidth.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGuidFindBox globalGroupsFindBox;
		private Core.Forms.ZPostingButtonsUserControl postingButtons;
		private ZArchitecture.ZLabel categoryLabel;
		private ZArchitecture.ZLabel featureLabel;
		private ZArchitecture.ZLabel globalGroupLabel;
		private ZArchitecture.GUI.ZCheckBox activeCheckbox;
		private ZArchitecture.ZLabel categoryValueLabel;
		private ZArchitecture.GUI.ZDropEditWithFixedWidth featureNameDropEditWithFixedWidth;
	}
}
