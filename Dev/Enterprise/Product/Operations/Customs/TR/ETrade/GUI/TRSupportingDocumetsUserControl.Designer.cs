namespace Enterprise.Customs.TR.ETrade.GUI
{
	partial class TRSupportingDocumetsUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.groupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SupportingDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.groupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).BeginInit();
			this.SupportingDocumentsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.ETrade.Business.AsycudaBill);
			// 
			// subgroup
			//
			this.groupBox.CaptionResourceString = Enterprise.Customs.TR.ETrade.GUI.Res.GetData("74868F34-2855-45D1-980B-387352FAF9C0", "subgroup");
			this.groupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox.Controls.Add(this.SupportingDocumentsGrid);
			this.groupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.groupBox.Name = "subgroup";
			this.groupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(943, 373, true);
			this.groupBox.TabIndex = 2;
			this.groupBox.TabStop = false;
			// 
			// SupportingDocumentsGrid
			// 
			this.SupportingDocumentsGrid.AllowNavigation = false;
			this.SupportingDocumentsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SupportingDocumentsGrid, "SupportingDocumentsForBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TR.ETrade.Business.AsycudaBill)(null)).SupportingDocumentsForBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.SupportingDocuments)(((System.Collections.IList)(((Enterprise.Customs.TR.ETrade.Business.AsycudaBill)(null)).SupportingDocumentsForBill)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.SupportingDocuments)(((System.Collections.IList)(((Enterprise.Customs.TR.ETrade.Business.AsycudaBill)(null)).SupportingDocumentsForBill)).SyncRoot)).DocumentDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.TR.ETrade.Business.SupportingDocuments)(((System.Collections.IList)(((Enterprise.Customs.TR.ETrade.Business.AsycudaBill)(null)).SupportingDocumentsForBill)).SyncRoot)).CSI_DateOfIssue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.SupportingDocuments)(((System.Collections.IList)(((Enterprise.Customs.TR.ETrade.Business.AsycudaBill)(null)).SupportingDocumentsForBill)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.SupportingDocuments)(((System.Collections.IList)(((Enterprise.Customs.TR.ETrade.Business.AsycudaBill)(null)).SupportingDocumentsForBill)).SyncRoot)).CSI_Status)));
			this.SupportingDocumentsGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CSI_Code";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.TR.ETrade.GUI.Res.GetData("DADC70BA-D226-467A-B7B4-05BC5B936FEF", "Code");
			zTextBoxColumnStyleInfo2.ColumnName = "DocumentDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.GroupName = Enterprise.Customs.TR.ETrade.GUI.Res.GetData("DADC70BA-D226-467A-B7B4-05BC5B936FEF", "Code");
			zDateEditColumnStyleInfo1.ColumnName = "CSI_DateOfIssue";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo3.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "CSI_Status";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.SupportingDocumentsGrid.GridId = "dc8cdc3e-5c49-41e0-bd14-be354f34f741";
			this.SupportingDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SupportingDocumentsGrid.LayoutKey = "SupportingDocumentsGrid";
			this.SupportingDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupportingDocumentsGrid.Name = "SupportingDocumentsGrid";
			this.SupportingDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(940, 370, true);
			this.SupportingDocumentsGrid.TabIndex = 0;
			// 
			// TRSupportingDocumentsUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.groupBox);
			this.Name = "TRSupportingDocumentsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(943, 373, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.groupBox.ResumeLayout(false);
			this.groupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).EndInit();
			this.SupportingDocumentsGrid.ResumeLayout(false);
			this.SupportingDocumentsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox groupBox;
		private ZArchitecture.ZGrid SupportingDocumentsGrid;
	}
}
