namespace Enterprise.Freight.Agency.GUI
{
	partial class VoyageAccountingForm
	{
		new void InitializeComponent()
		{
			this.postingButtons = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.selectVoyageButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.mainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			topPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			vesselTextBox = new Enterprise.ZArchitecture.ZTextBox();
			voyageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			principalFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			noteTabPage = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			logsTabPage = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			bottomPanel.SuspendLayout();
			topPanel.SuspendLayout();
			this.mainTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 666, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1014, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.VoyageAccount);
			// 
			// bottomPanel
			// 
			bottomPanel.Controls.Add(this.postingButtons);
			bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 640, true);
			bottomPanel.Name = "bottomPanel";
			bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1014, 26, true);
			bottomPanel.TabIndex = 2;
			// 
			// postingButtons
			// 
			this.postingButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.postingButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(768, 0, true);
			this.postingButtons.Name = "postingButtons";
			this.postingButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 25, true);
			this.postingButtons.TabIndex = 0;
			// 
			// topPanel
			// 
			topPanel.Controls.Add(this.selectVoyageButton);
			topPanel.Controls.Add(vesselTextBox);
			topPanel.Controls.Add(voyageTextBox);
			topPanel.Controls.Add(principalFindBox);
			topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			topPanel.Name = "topPanel";
			topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1014, 32, true);
			topPanel.TabIndex = 0;
			// 
			// selectVoyageButton
			// 
			this.selectVoyageButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("VoyageAccountingForm|9ae5f2c4-84c6-4526-b42f-c62822d006ae", "Select Voyage");
			this.selectVoyageButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 7, true);
			this.selectVoyageButton.Name = "selectVoyageButton";
			this.selectVoyageButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 23, true);
			this.selectVoyageButton.TabIndex = 2;
			this.selectVoyageButton.Click += new System.EventHandler(this.selectVoyageButton_Click);
			// 
			// vesselTextBox
			// 
			this.BindingSource.SetBindingMember(vesselTextBox, "NA_Calc_Voyage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.VoyageAccount)(null)).NA_Calc_Voyage)));
			vesselTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 8, true);
			vesselTextBox.Name = "vesselTextBox";
			vesselTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			vesselTextBox.TabIndex = 1;
			// 
			// voyageTextBox
			// 
			this.BindingSource.SetBindingMember(voyageTextBox, "NA_Calc_Vessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.VoyageAccount)(null)).NA_Calc_Vessel)));
			voyageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 8, true);
			voyageTextBox.Name = "voyageTextBox";
			voyageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			voyageTextBox.TabIndex = 0;
			// 
			// principalFindBox
			// 
			this.BindingSource.SetBindingMember(principalFindBox, "NA_OH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.VoyageAccount)(null)).NA_OH)));
			principalFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(568, 8, true);
			principalFindBox.Name = "principalFindBox";
			principalFindBox.PopupCaption = null;
			principalFindBox.PreBoundMaxLength = 12;
			principalFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			principalFindBox.TabIndex = 3;
			// 
			// noteTabPage
			// 
			noteTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			noteTabPage.Name = "noteTabPage";
			noteTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1006, 581, true);
			noteTabPage.TabIndex = 1;
			// 
			// logsTabPage
			// 
			logsTabPage.ExcludeFromBindingOnSave = true;
			logsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			logsTabPage.Name = "logsTabPage";
			logsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1006, 581, true);
			logsTabPage.TabIndex = 0;
			// 
			// mainTabControl
			// 
			this.mainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.mainTabControl.Controls.Add(noteTabPage);
			this.mainTabControl.Controls.Add(logsTabPage);
			this.mainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 32, true);
			this.mainTabControl.Name = "mainTabControl";
			this.mainTabControl.SelectedIndex = 0;
			this.mainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1014, 608, true);
			this.mainTabControl.TabIndex = 1;
			// 
			// VoyageAccountingForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1014, 690, true);
			this.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("VoyageAccountingForm|4e18fc30-e792-491a-a8b3-260e8bd86bca", "Voyage Accounting");
			this.Controls.Add(this.mainTabControl);
			this.Controls.Add(topPanel);
			this.Controls.Add(bottomPanel);
			this.DataSourceType = typeof(Enterprise.Freight.Agency.Business.VoyageAccount);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1022, 717, true);
			this.Name = "VoyageAccountingForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(bottomPanel, 0);
			this.Controls.SetChildIndex(topPanel, 0);
			this.Controls.SetChildIndex(this.mainTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			bottomPanel.ResumeLayout(false);
			topPanel.ResumeLayout(false);
			topPanel.PerformLayout();
			this.mainTabControl.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		Enterprise.Core.Forms.ZPostingButtonsUserControl postingButtons;
		Enterprise.ZArchitecture.GUI.ZButton selectVoyageButton;
		Enterprise.ZArchitecture.GUI.ZTemplateTabControl mainTabControl;
		Enterprise.ZArchitecture.GUI.ZPanel bottomPanel;
		Enterprise.ZArchitecture.GUI.ZPanel topPanel;
		Enterprise.ZArchitecture.ZTextBox vesselTextBox;
		Enterprise.ZArchitecture.ZTextBox voyageTextBox;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox principalFindBox;
		Enterprise.ZArchitecture.GUI.ZStmNoteTabPage noteTabPage;
		Enterprise.ZArchitecture.GUI.ZLogsTabPage logsTabPage;
	}
}
