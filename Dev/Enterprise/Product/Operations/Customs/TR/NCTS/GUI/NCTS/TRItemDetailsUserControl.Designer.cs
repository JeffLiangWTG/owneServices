
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.NCTS.GUI
{
	partial class TRItemDetailsUserControl
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
			this.exportDeclarationNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.exportDeclarationTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.isDeclarationPartialCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ItemDetailsGroupBox.SuspendLayout();
			this.DeclarationTypeDropEdit.SuspendLayout();
			this.CountryOfDestinationDropEdit.SuspendLayout();
			this.OriginCountryDropEdit.SuspendLayout();
			this.CountryOfDispatchDropEdit.SuspendLayout();
			this.ItemConsigneeDocAddressControl.SuspendLayout();
			this.ItemConsignorDocAddressControl.SuspendLayout();
			this.NetWeightCalcDropEdit.SuspendLayout();
			this.GrossWeightCalcDropEdit.SuspendLayout();
			this.CommodityCodeTariffFindBox.SuspendLayout();
			this.FiscalUnitsDropEdit.SuspendLayout();
			this.CustomsValueDropEdit.SuspendLayout();
			this.TaxOrFeeDropEdit.SuspendLayout();
			this.CustomsFirstQtyDropEdit.SuspendLayout();
			this.CustomsThirdQtyDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.exportDeclarationTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// ItemDetailsGroupBox
			// 
			this.ItemDetailsGroupBox.Controls.Add(this.isDeclarationPartialCheckBox);
			this.ItemDetailsGroupBox.Controls.Add(this.exportDeclarationTypeDropEdit);
			this.ItemDetailsGroupBox.Controls.Add(this.exportDeclarationNumberTextBox);
			this.ItemDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1138, 463, true);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.OriginCountryDropEdit, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.CustomsThirdQtyDropEdit, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.CustomsFirstQtyDropEdit, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.ItemNumberTextBox, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.CountryOfDispatchDropEdit, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.DeclarationTypeDropEdit, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.DescriptionOfGoodsTextBox, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.GrossWeightCalcDropEdit, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.NetWeightCalcDropEdit, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.AdditionalSupplementaryCodesEditButton, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.AdditionalSupplementaryCodesTextBox, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.TaxOrFeeDropEdit, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.CustomsValueDropEdit, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.FiscalUnitsDropEdit, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.CommodityCodeTariffFindBox, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.CountryOfDestinationDropEdit, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.exportDeclarationNumberTextBox, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.ItemConsignorDocAddressControl, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.exportDeclarationTypeDropEdit, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.ItemConsigneeDocAddressControl, 0);
			this.ItemDetailsGroupBox.Controls.SetChildIndex(this.isDeclarationPartialCheckBox, 0);
			// 
			// ItemConsigneeDocAddressControl
			// 
			this.ItemConsigneeDocAddressControl.TabIndex = 16;
			// 
			// ItemConsignorDocAddressControl
			// 
			this.ItemConsignorDocAddressControl.TabIndex = 15;
			// 
			// FeesGroupBox
			// 
			this.FeesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(540, 217, true);
			this.FeesGroupBox.TabIndex = 17;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc);
			// 
			// exportDeclarationNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.exportDeclarationNumberTextBox, "ExportDeclarationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).ExportDeclarationNumber)));
			this.exportDeclarationNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 377, true);
			this.exportDeclarationNumberTextBox.Name = "exportDeclarationNumberTextBox";
			this.exportDeclarationNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 17, true);
			this.exportDeclarationNumberTextBox.TabIndex = 14;
			// 
			// exportDeclarationTypeDropEdit
			// 
			this.exportDeclarationTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.exportDeclarationTypeDropEdit, "ExportDeclarationType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).ExportDeclarationType)));
			this.exportDeclarationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 401, true);
			this.exportDeclarationTypeDropEdit.Name = "exportDeclarationTypeDropEdit";
			this.exportDeclarationTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 17, true);
			this.exportDeclarationTypeDropEdit.TabIndex = 15;
			// 
			// isDeclarationPartialCheckBox
			// 
			this.BindingSource.SetBindingMember(this.isDeclarationPartialCheckBox, "IsDeclarationPartial");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).IsDeclarationPartial)));
			this.isDeclarationPartialCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.isDeclarationPartialCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 424, true);
			this.isDeclarationPartialCheckBox.Name = "isDeclarationPartialCheckBox";
			this.isDeclarationPartialCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 15, true);
			this.isDeclarationPartialCheckBox.TabIndex = 16;
			this.isDeclarationPartialCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.isDeclarationPartialCheckBox.UseVisualStyleBackColor = true;
			// 
			// TRItemDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Name = "TRItemDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1138, 463, true);
			this.ItemDetailsGroupBox.ResumeLayout(false);
			this.ItemDetailsGroupBox.PerformLayout();
			this.DeclarationTypeDropEdit.ResumeLayout(true);
			this.DeclarationTypeDropEdit.PerformLayout();
			this.CountryOfDestinationDropEdit.ResumeLayout(true);
			this.CountryOfDestinationDropEdit.PerformLayout();
			this.OriginCountryDropEdit.ResumeLayout(true);
			this.OriginCountryDropEdit.PerformLayout();
			this.CountryOfDispatchDropEdit.ResumeLayout(true);
			this.CountryOfDispatchDropEdit.PerformLayout();
			this.ItemConsigneeDocAddressControl.ResumeLayout(true);
			this.ItemConsigneeDocAddressControl.PerformLayout();
			this.ItemConsignorDocAddressControl.ResumeLayout(true);
			this.ItemConsignorDocAddressControl.PerformLayout();
			this.NetWeightCalcDropEdit.ResumeLayout(true);
			this.NetWeightCalcDropEdit.PerformLayout();
			this.GrossWeightCalcDropEdit.ResumeLayout(true);
			this.GrossWeightCalcDropEdit.PerformLayout();
			this.CommodityCodeTariffFindBox.ResumeLayout(true);
			this.CommodityCodeTariffFindBox.PerformLayout();
			this.FiscalUnitsDropEdit.ResumeLayout(true);
			this.FiscalUnitsDropEdit.PerformLayout();
			this.CustomsValueDropEdit.ResumeLayout(true);
			this.CustomsValueDropEdit.PerformLayout();
			this.TaxOrFeeDropEdit.ResumeLayout(true);
			this.TaxOrFeeDropEdit.PerformLayout();
			this.CustomsFirstQtyDropEdit.ResumeLayout(true);
			this.CustomsFirstQtyDropEdit.PerformLayout();
			this.CustomsThirdQtyDropEdit.ResumeLayout(true);
			this.CustomsThirdQtyDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.exportDeclarationTypeDropEdit.ResumeLayout(true);
			this.exportDeclarationTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZCheckBox isDeclarationPartialCheckBox;
		private ZDropEdit exportDeclarationTypeDropEdit;
		private ZTextBox exportDeclarationNumberTextBox;

	}
}
