namespace Enterprise.Customs.GUI
{
	partial class CusRefPreferenceForm
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
			this.components = new System.ComponentModel.Container();
			this.mainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.TabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.LogsTabPage = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			this.postingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
			this.mainSplitContainer.Panel1.SuspendLayout();
			this.mainSplitContainer.Panel2.SuspendLayout();
			this.mainSplitContainer.SuspendLayout();
			this.TabControl.SuspendLayout();
			this.postingButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 425, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Universal.CusRefPreference);
			// 
			// mainSplitContainer
			// 
			this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.mainSplitContainer.IsSplitterFixed = true;
			this.mainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainSplitContainer.Name = "mainSplitContainer";
			this.mainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// mainSplitContainer.Panel1
			// 
			this.mainSplitContainer.Panel1.Controls.Add(this.TabControl);
			// 
			// mainSplitContainer.Panel2
			// 
			this.mainSplitContainer.Panel2.Controls.Add(this.postingButtonsUserControl);
			this.mainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 449, true);
			this.mainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(420);
			this.mainSplitContainer.TabIndex = 1;
			this.mainSplitContainer.TabStop = false;
			// 
			// TabControl
			// 
			this.TabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.TabControl.Controls.Add(this.DetailsTabPage);
			this.TabControl.Controls.Add(this.LogsTabPage);
			this.TabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 420, true);
			this.TabControl.TabIndex = 0;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("CusRefPreferenceForm|658116E8-576E-46C9-A394-49ABB96C3594", "Details");
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 393, true);
			this.DetailsTabPage.TabIndex = 0;
			this.DetailsTabPage.UseVisualStyleBackColor = true;
			this.DetailsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.DetailsTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.CusRefPreference)(null)).CR8_Description)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.CusRefPreference)(null)).CR8_RN_NKCountryCode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.CusRefPreference)(null)).CR8_Preference)));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.ExcludeFromBindingOnSave = true;
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LogsTabPage.Name = "LogsTabPage";
			this.LogsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.LogsTabPage.ShouldBeReadOnlyInViewMode = false;
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 415, true);
			this.LogsTabPage.TabIndex = 1;
			this.LogsTabPage.UseVisualStyleBackColor = true;
			// 
			// postingButtonsUserControl
			// 
			this.postingButtonsUserControl.AllowDrop = true;
			this.postingButtonsUserControl.Dock = System.Windows.Forms.DockStyle.Right;
			this.postingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(340, 0, true);
			this.postingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.postingButtonsUserControl.Name = "postingButtonsUserControl";
			this.postingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 25, true);
			this.postingButtonsUserControl.TabIndex = 0;
			// 
			// CusRefPreferenceForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("CusRefPreferenceForm|E7A5A241-0BDD-486C-9DDF-6EB2D0F8C39F", "Preference");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 449, true);
			this.Controls.Add(this.mainSplitContainer);
			this.DataSourceType = typeof(Enterprise.Customs.Universal.CusRefPreference);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(658, 509, true);
			this.Name = "CusRefPreferenceForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Text = "CusRefPreferenceForm";
			this.Controls.SetChildIndex(this.mainSplitContainer, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.mainSplitContainer.Panel1.ResumeLayout(false);
			this.mainSplitContainer.Panel2.ResumeLayout(false);
			this.mainSplitContainer.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
			this.mainSplitContainer.ResumeLayout(false);
			this.mainSplitContainer.PerformLayout();
			this.TabControl.ResumeLayout(false);
			this.TabControl.PerformLayout();
			this.postingButtonsUserControl.ResumeLayout(true);
			this.postingButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		void DetailsTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.CR8_DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CR8_RN_NKCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CR8_PreferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DetailsTabPage.SuspendLayout();
			this.CR8_RN_NKCountryCodeFindBox.SuspendLayout();
			this.DetailsTabPage.Controls.Add(this.CR8_RN_NKCountryCodeFindBox);
			this.DetailsTabPage.Controls.Add(this.CR8_PreferenceTextBox);
			this.DetailsTabPage.Controls.Add(this.CR8_DescriptionTextBox);
			// 
			// CR8_DescriptionTextBox
			// 
			this.CR8_DescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CR8_DescriptionTextBox, "CR8_Description");
			this.CR8_DescriptionTextBox.CaptionResourceString = null;
			this.CR8_DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 67, true);
			this.CR8_DescriptionTextBox.Multiline = true;
			this.CR8_DescriptionTextBox.Name = "CR8_DescriptionTextBox";
			this.CR8_DescriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.CR8_DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(532, 299, true);
			this.CR8_DescriptionTextBox.TabIndex = 2;
			// 
			// CR8_RN_NKCountryCodeFindBox
			// 
			this.CR8_RN_NKCountryCodeFindBox.AllowDrop = true;
			this.CR8_RN_NKCountryCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CR8_RN_NKCountryCodeFindBox, "CR8_RN_NKCountryCode");
			this.CR8_RN_NKCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 15, true);
			this.CR8_RN_NKCountryCodeFindBox.Name = "CR8_RN_NKCountryCodeFindBox";
			this.CR8_RN_NKCountryCodeFindBox.PreBoundMaxLength = 2;
			this.CR8_RN_NKCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(532, 20, true);
			this.CR8_RN_NKCountryCodeFindBox.TabIndex = 0;
			// 
			// CR8_PreferenceTextBox
			// 
			this.CR8_PreferenceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CR8_PreferenceTextBox, "CR8_Preference");
			this.CR8_PreferenceTextBox.CaptionResourceString = null;
			this.CR8_PreferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 41, true);
			this.CR8_PreferenceTextBox.Name = "CR8_PreferenceTextBox";
			this.CR8_PreferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 20, true);
			this.CR8_PreferenceTextBox.TabIndex = 1;
			this.DetailsTabPage.PerformLayout();
			this.CR8_RN_NKCountryCodeFindBox.ResumeLayout(true);
			this.CR8_RN_NKCountryCodeFindBox.PerformLayout();
			this.DetailsTabPage.ResumeLayout(true);

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer mainSplitContainer;
		private Enterprise.Core.Forms.ZPostingButtonsUserControl postingButtonsUserControl;
		private Enterprise.ZArchitecture.ZTextBox CR8_DescriptionTextBox;
		private Enterprise.ZArchitecture.GUI.ZTabControl TabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage DetailsTabPage;
		private Enterprise.ZArchitecture.GUI.ZLogsTabPage LogsTabPage;
		private ZArchitecture.GUI.ZCodeFindBox CR8_RN_NKCountryCodeFindBox;
		private ZArchitecture.ZTextBox CR8_PreferenceTextBox;
	}
}
