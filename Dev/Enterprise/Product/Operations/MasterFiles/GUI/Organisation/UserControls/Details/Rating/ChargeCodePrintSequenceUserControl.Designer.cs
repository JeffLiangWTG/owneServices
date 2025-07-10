using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.GUI.Organisation.UserControls
{
	public partial class ChargeCodePrintSequenceUserControl
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
			this.components = new System.ComponentModel.Container();
			this.SequenceGrid = new Enterprise.ZArchitecture.ZGrid();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// SequenceGrid
			// 
			this.SequenceGrid.AllowNavigation = false;
			this.SequenceGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
				| System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SequenceGrid, "RatingDocumentsChargeOrders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).RatingDocumentsChargeOrders)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.Rating.RatingDocumentsChargeOrder)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).RatingDocumentsChargeOrders)).SyncRoot)).RCO_AC_ChargeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.Rating.RatingDocumentsChargeOrder)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).RatingDocumentsChargeOrders)).SyncRoot)).AC_Desc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.Rating.RatingDocumentsChargeOrder)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).RatingDocumentsChargeOrders)).SyncRoot)).RCO_DocumentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.Rating.RatingDocumentsChargeOrder)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).RatingDocumentsChargeOrders)).SyncRoot)).RCO_PrintOrder)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.Rating.RatingDocumentsChargeOrder)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).RatingDocumentsChargeOrders)).SyncRoot)).AC_PrintSequence)));
			this.SequenceGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "RCO_AC_ChargeCode";
			zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TariffsAndRatesDetailsUserControl|62A3DC94-4141-4D5A-A2DA-DDB2B597399D", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "AC_Desc";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo7.ColumnName = "RCO_DocumentType";
			zDropEditColumnStyleInfo7.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "RCO_PrintOrder";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TariffsAndRatesDetailsUserControl|B2491FF3-F5E7-4FFD-85D1-0D235064D54F", "Base Sequence");
			zCalcEditColumnStyleInfo2.ColumnName = "AC_PrintSequence";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.SequenceGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.SequenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SequenceGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.SequenceGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.SequenceGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.SequenceGrid.GridId = "1AF6C6D1-9B9B-47B6-A51B-A407CCBD23EC";
			this.SequenceGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SequenceGrid.LayoutKey = "zGrid1";
			this.SequenceGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SequenceGrid.Name = "SequenceGrid";
			this.SequenceGrid.TabIndex = 0;
			//
			// ChargeCodePrintSequenceUserControl
			//
			this.Controls.Add(this.SequenceGrid);
			this.Name = "ChargeCodePrintSequenceUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 419, true);
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SequenceGrid)).EndInit();
			this.SequenceGrid.ResumeLayout(false);
			this.SequenceGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		ZGrid SequenceGrid;
	}
}
