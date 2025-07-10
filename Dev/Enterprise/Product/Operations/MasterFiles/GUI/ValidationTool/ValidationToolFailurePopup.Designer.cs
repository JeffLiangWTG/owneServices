namespace Enterprise.MasterFiles.GUI;

public partial class ValidationToolFailurePopup
{
	new void InitializeComponent()
	{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.FailedRuleResultsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ButtonClose = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonProceed = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LabelStatus = new Enterprise.ZArchitecture.ZLabel();
			this.ButtonDeliver = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PanelBottom = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PanelTop = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FailedRuleResultsGrid)).BeginInit();
			this.FailedRuleResultsGrid.SuspendLayout();
			this.PanelBottom.SuspendLayout();
			this.PanelTop.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 547, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(857, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.NonPersistentValidationFailure);
			// 
			// FailedRuleResultsGrid
			// 
			this.FailedRuleResultsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.FailedRuleResultsGrid, "FailedRuleResults");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.NonPersistentValidationFailure)(null)).FailedRuleResults)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.NonPersistentRuleValidationResult)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.NonPersistentValidationFailure)(null)).FailedRuleResults)).SyncRoot)).Rule)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.NonPersistentRuleValidationResult)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.NonPersistentValidationFailure)(null)).FailedRuleResults)).SyncRoot)).Severity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.NonPersistentRuleValidationResult)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.NonPersistentValidationFailure)(null)).FailedRuleResults)).SyncRoot)).Message)));
			this.FailedRuleResultsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "Rule";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.ColumnName = "Severity";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.ColumnName = "Message";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
			this.FailedRuleResultsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FailedRuleResultsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FailedRuleResultsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FailedRuleResultsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FailedRuleResultsGrid.GridId = "954e4c91-ae9c-45f1-82ea-68764bb7c9f4";
			this.FailedRuleResultsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FailedRuleResultsGrid.LayoutKey = "FailedRuleResultsGrid";
			this.FailedRuleResultsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 38, true);
			this.FailedRuleResultsGrid.Name = "FailedRuleResultsGrid";
			this.FailedRuleResultsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(857, 476, true);
			this.FailedRuleResultsGrid.TabIndex = 2;
			this.FailedRuleResultsGrid.ColourDeciding += new System.EventHandler<Enterprise.ZArchitecture.ColourDecidingEventArgs>(this.FailedRuleResultsGrid_ColourDeciding);
			// 
			// ButtonClose
			// 
			this.ButtonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonClose.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("851ddcb6-3112-4d32-af25-36537034eca4", "Close");
			this.ButtonClose.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(766, 5, true);
			this.ButtonClose.Name = "ButtonClose";
			this.ButtonClose.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 22, true);
			this.ButtonClose.TabIndex = 5;
			this.ButtonClose.ToolTipCaption = null;
			this.ButtonClose.UseVisualStyleBackColor = false;
			this.ButtonClose.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// ButtonProceed
			// 
			this.ButtonProceed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonProceed.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("86b99f82-bc9f-43e9-bd9f-dd085f7db8d3", "Proceed");
			this.ButtonProceed.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(679, 5, true);
			this.ButtonProceed.Name = "ButtonProceed";
			this.ButtonProceed.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 22, true);
			this.ButtonProceed.TabIndex = 4;
			this.ButtonProceed.ToolTipCaption = null;
			this.ButtonProceed.UseVisualStyleBackColor = false;
			this.ButtonProceed.Click += new System.EventHandler(this.ButtonProceed_Click);
			// 
			// LabelStatus
			// 
			this.BindingSource.SetBindingMember(this.LabelStatus, "Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.NonPersistentValidationFailure)(null)).Status)));
			this.LabelStatus.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.LabelStatus, false);
			this.LabelStatus.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 8, true);
			this.LabelStatus.Name = "LabelStatus";
			this.LabelStatus.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 22, true);
			this.LabelStatus.TabIndex = 1;
			this.LabelStatus.UseMnemonic = false;
			// 
			// ButtonDeliver
			// 
			this.ButtonDeliver.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonDeliver.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8beae969-67c4-4bc0-810f-aa65eeb5a839", "Deliver");
			this.ButtonDeliver.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(592, 5, true);
			this.ButtonDeliver.Name = "ButtonDeliver";
			this.ButtonDeliver.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 22, true);
			this.ButtonDeliver.TabIndex = 3;
			this.ButtonDeliver.ToolTipCaption = null;
			this.ButtonDeliver.UseVisualStyleBackColor = false;
			this.ButtonDeliver.Click += new System.EventHandler(this.ButtonDeliver_Click);
			// 
			// PanelBottom
			// 
			this.PanelBottom.Controls.Add(this.ButtonDeliver);
			this.PanelBottom.Controls.Add(this.ButtonProceed);
			this.PanelBottom.Controls.Add(this.ButtonClose);
			this.PanelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PanelBottom.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 514, true);
			this.PanelBottom.Name = "PanelBottom";
			this.PanelBottom.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(857, 33, true);
			this.PanelBottom.TabIndex = 3;
			// 
			// PanelTop
			// 
			this.PanelTop.Controls.Add(this.LabelStatus);
			this.PanelTop.Dock = System.Windows.Forms.DockStyle.Top;
			this.PanelTop.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PanelTop.Name = "PanelTop";
			this.PanelTop.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(857, 38, true);
			this.PanelTop.TabIndex = 7;
			// 
			// ValidationToolFailurePopup
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(857, 571, true);
			this.Controls.Add(this.FailedRuleResultsGrid);
			this.Controls.Add(this.PanelTop);
			this.Controls.Add(this.PanelBottom);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.NonPersistentValidationFailure);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 233, true);
			this.Name = "ValidationToolFailurePopup";
			this.ShouldSerializeTabPageMethods = true;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PanelBottom, 0);
			this.Controls.SetChildIndex(this.PanelTop, 0);
			this.Controls.SetChildIndex(this.FailedRuleResultsGrid, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.FailedRuleResultsGrid)).EndInit();
			this.FailedRuleResultsGrid.ResumeLayout(false);
			this.FailedRuleResultsGrid.PerformLayout();
			this.PanelBottom.ResumeLayout(false);
			this.PanelBottom.PerformLayout();
			this.PanelTop.ResumeLayout(false);
			this.PanelTop.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	private ZArchitecture.GUI.ZPanel PanelBottom;
	internal ZArchitecture.GUI.ZButton ButtonClose;
	internal ZArchitecture.GUI.ZButton ButtonProceed;
	internal ZArchitecture.GUI.ZButton ButtonDeliver;
	internal ZArchitecture.ZGrid FailedRuleResultsGrid;
	private ZArchitecture.GUI.ZPanel PanelTop;
	internal ZArchitecture.ZLabel LabelStatus;
}
