namespace Enterprise.MasterFiles.GUI
{
	public partial class ExternalValidationResultForm
	{

		#region Windows Form Designer generated code

		Enterprise.ZArchitecture.GUI.ZButton buttonClose;
		internal Enterprise.ZArchitecture.ZGrid resultGrid;
		Enterprise.ZArchitecture.ZLabel zLabel;

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.buttonClose = new Enterprise.ZArchitecture.GUI.ZButton();
			this.resultGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.resultGrid)).BeginInit();
			this.resultGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 401, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 0, true);
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(294);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(294);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ExternalValidationResultWrapper);
			// 
			// buttonClose
			// 
			this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonClose.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ExternalValidationResultForm|0c2fc185-6693-4ccf-ba71-9038c15cbf70", "Close");
			this.buttonClose.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(466, 366, true);
			this.buttonClose.Name = "buttonClose";
			this.buttonClose.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 23, true);
			this.buttonClose.TabIndex = 2;
			this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
			// 
			// resultGrid
			// 
			this.resultGrid.AllowNavigation = false;
			this.resultGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.resultGrid, "Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ExternalValidationResultWrapper)(null)).Messages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ExternalValidationResultMessage)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ExternalValidationResultWrapper)(null)).Messages)).SyncRoot)).MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ExternalValidationResultMessage)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ExternalValidationResultWrapper)(null)).Messages)).SyncRoot)).MessageContent)));
			this.resultGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b748adaf-cbb2-47ef-9249-a27974c793c6", "Type");
			zTextBoxColumnStyleInfo1.ColumnName = "MessageType";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9b917cc4-3db4-49c6-9cf0-d2e153627efc", "Message");
			zTextBoxColumnStyleInfo2.ColumnName = "MessageContent";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.resultGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.resultGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.resultGrid.CopySelectedRowsAllowed = true;
			this.resultGrid.GridId = "92b6db75-1767-4b43-8730-19dc00f05d38";
			this.resultGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.resultGrid.LayoutKey = "ResultGrid";
			this.resultGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 66, true);
			this.resultGrid.Name = "resultGrid";
			this.resultGrid.ReadOnly = true;
			this.resultGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(548, 294, true);
			this.resultGrid.TabIndex = 1;
			// 
			// zLabel
			// 
			this.zLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ExternalValidationResultForm|71921235-fa4f-4a25-95c7-289c773446ff", "", "This organization failed validation against the external web service.\r\nPlease review the validation results below and update the organization accordingly.");
			this.zLabel.IsFontBold = true;
			this.zLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 9, true);
			this.zLabel.Name = "zLabel";
			this.zLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 54, true);
			this.zLabel.TabIndex = 0;
			// 
			// ExternalValidationResultForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ExternalValidationResultForm|5e5e8e1f-c40c-43a2-8e69-a20577cbe9d2", "Organization Validation");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 401, true);
			this.Controls.Add(this.zLabel);
			this.Controls.Add(this.buttonClose);
			this.Controls.Add(this.resultGrid);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.ExternalValidationResultWrapper);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(580, 440, true);
			this.Name = "ExternalValidationResultForm";
			this.Load += new System.EventHandler(this.ExternalValidationResultForm_Load);
			this.Controls.SetChildIndex(this.resultGrid, 0);
			this.Controls.SetChildIndex(this.buttonClose, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.resultGrid)).EndInit();
			this.resultGrid.ResumeLayout(false);
			this.resultGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
