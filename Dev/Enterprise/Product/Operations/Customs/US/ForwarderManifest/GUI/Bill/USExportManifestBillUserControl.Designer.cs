using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	partial class USExportManifestBillUserControl
	{
		void InitializeComponent()
		{
			this.BoardedQuantityCalcEdit = new ZArchitecture.ZCalcEdit();
			this.BoardedWeightCalcDropEdit = new ZCalcDropEdit();
			this.PriorTransportationModeDropEdit = new ZDropEdit();
			this.FinalDestinationPortUserControl = new FinalDestinationPortUserControl();
			this.ArrivalPortUserControl = new ArrivalPortUserControl();
			this.DeparturePortUserControl = new DeparturePortUserControl();
			this.LadingPortUserControl = new LadingPortUserControl();
			this.UnladingPortUserControl = new UnladingPortUserControl();
			this.OriginPortUserControl = new OriginPortUserControl();
			this.SpecialCargoCodesDropEdit = new ZDropEdit();
			this.PlaceOfReceiptTextBox = new ZArchitecture.ZTextBox();
			this.AESITNNumbersUserControl = new ITNUserControl();
			this.InBondNumbersUserControl = new InBondUserControl();
			this.AESExemptionCodeTextBox = new ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PriorTransportationModeDropEdit.SuspendLayout();
			this.FinalDestinationPortUserControl.SuspendLayout();
			this.AESITNNumbersUserControl.SuspendLayout();
			this.InBondNumbersUserControl.SuspendLayout();
			this.ArrivalPortUserControl.SuspendLayout();
			this.DeparturePortUserControl.SuspendLayout();
			this.LadingPortUserControl.SuspendLayout();
			this.UnladingPortUserControl.SuspendLayout();
			this.OriginPortUserControl.SuspendLayout();
			this.SpecialCargoCodesDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.USExportAsycudaBill);
			// 
			// BoardedQuantityCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.BoardedQuantityCalcEdit, "ATL_Quantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.USExportAsycudaBill)(null)).ATL_Quantity)));
			this.BoardedQuantityCalcEdit.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("842561EE-CC67-463F-B2AB-03CC101BF413", "Boarded Quantity");
			this.BoardedQuantityCalcEdit.DecimalPlaces = 2;
			this.BoardedQuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 377, true);
			this.BoardedQuantityCalcEdit.Name = "BoardedQuantityCalcEdit";
			this.BoardedQuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.BoardedQuantityCalcEdit.TabIndex = 1;
			this.BoardedQuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// BoardedWeightCalcDropEdit
			//
			this.BoardedWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BoardedWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.USExportAsycudaBill)(null)).ATL_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.USExportAsycudaBill)(null)).ATL_WeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.USExportAsycudaBill)(null)).Lookups.WeightUQList)));
			this.BoardedWeightCalcDropEdit.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("842561EE-CC67-462F-1234-03CC101BF413", "Boarded Weight");
			this.BoardedWeightCalcDropEdit.BindToAmount = "ATL_Weight";
			this.BoardedWeightCalcDropEdit.BindToList = "Lookups+WeightUQList";
			this.BoardedWeightCalcDropEdit.BindToUnit = "ATL_WeightUQ";
			this.BoardedWeightCalcDropEdit.Decimals = 3;
			this.BoardedWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 339, true);
			this.BoardedWeightCalcDropEdit.Name = "BoardedWeightCalcDropEdit";
			this.BoardedWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.BoardedWeightCalcDropEdit.TabIndex = 2;
			this.BoardedWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// PriorTransportationModeDropEdit
			// 
			this.PriorTransportationModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PriorTransportationModeDropEdit, "ABL_InlandTransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.USExportAsycudaBill)(null)).ABL_InlandTransportMode)));
			this.PriorTransportationModeDropEdit.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("842561EE-CC67-462F-B2AB-03CC101BF425", "Prior Mode of Transportation");
			this.PriorTransportationModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 351, true);
			this.PriorTransportationModeDropEdit.Name = "PriorTransportationModeDropEdit";
			this.PriorTransportationModeDropEdit.PreBoundMaxLength = 3;
			this.PriorTransportationModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 17, true);
			this.PriorTransportationModeDropEdit.TabIndex = 3;
			// 
			// FinalDestinationPortUserControl
			// 
			this.FinalDestinationPortUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FinalDestinationPortUserControl, ".");
			this.FinalDestinationPortUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(197, 305, true);
			this.FinalDestinationPortUserControl.Name = "FinalDestinationPortUserControl";
			this.FinalDestinationPortUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 23, true);
			this.FinalDestinationPortUserControl.TabIndex = 4;
			// 
			// ArrivalPortUserControl
			// 
			this.ArrivalPortUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ArrivalPortUserControl, ".");
			this.ArrivalPortUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(197, 262, true);
			this.ArrivalPortUserControl.Name = "ArrivalPortUserControl";
			this.ArrivalPortUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 23, true);
			this.ArrivalPortUserControl.TabIndex = 5;
			// 
			// DeparturePortUserControl
			// 
			this.DeparturePortUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeparturePortUserControl, ".");
			this.DeparturePortUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(197, 220, true);
			this.DeparturePortUserControl.Name = "DeparturePortUserControl";
			this.DeparturePortUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 23, true);
			this.DeparturePortUserControl.TabIndex = 6;
			// 
			// LadingPortUserControl
			// 
			this.LadingPortUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LadingPortUserControl, ".");
			this.LadingPortUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(197, 173, true);
			this.LadingPortUserControl.Name = "LadingPortUserControl";
			this.LadingPortUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 23, true);
			this.LadingPortUserControl.TabIndex = 7;
			// 
			// UnladingPortUserControl
			// 
			this.UnladingPortUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnladingPortUserControl, ".");
			this.UnladingPortUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(197, 128, true);
			this.UnladingPortUserControl.Name = "UnladingPortUserControl";
			this.UnladingPortUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 23, true);
			this.UnladingPortUserControl.TabIndex = 8;
			// 
			// OriginPortUserControl
			// 
			this.OriginPortUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OriginPortUserControl, ".");
			this.OriginPortUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(197, 86, true);
			this.OriginPortUserControl.Name = "OriginPortUserControl";
			this.OriginPortUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 23, true);
			this.OriginPortUserControl.TabIndex = 11;
			// 
			// SpecialCargoCodesDropEdit
			// 
			this.SpecialCargoCodesDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SpecialCargoCodesDropEdit, "ABL_SpecialCargoCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.USExportAsycudaBill)(null)).ABL_SpecialCargoCode)));
			this.SpecialCargoCodesDropEdit.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("842566EE-CC67-462F-B2AB-03CC101BF426", "Bill of Lading Type");
			this.SpecialCargoCodesDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(197, 45, true);
			this.SpecialCargoCodesDropEdit.Name = "SpecialCargoCodesDropEdit";
			this.SpecialCargoCodesDropEdit.PreBoundMaxLength = 1;
			this.SpecialCargoCodesDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 17, true);
			this.SpecialCargoCodesDropEdit.TabIndex = 10;
			// 
			// PlaceOfReceiptTextBox
			// 
			this.BindingSource.SetBindingMember(this.PlaceOfReceiptTextBox, "ABL_LocationInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.USExportAsycudaBill)(null)).ABL_LocationInformation)));
			this.PlaceOfReceiptTextBox.CaptionResourceString = null;
			this.PlaceOfReceiptTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(621, 427, true);
			this.PlaceOfReceiptTextBox.Name = "PlaceOfReceiptTextBox";
			this.PlaceOfReceiptTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 17, true);
			this.PlaceOfReceiptTextBox.TabIndex = 12;
			// 
			// AESExemptionCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.AESExemptionCodeTextBox, "ABL_UCRNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.USExportAsycudaBill)(null)).ABL_UCRNumber)));
			this.AESExemptionCodeTextBox.CaptionResourceString = null;
			this.AESExemptionCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(621, 460, true);
			this.AESExemptionCodeTextBox.Name = "AESExemptionCodeTextBox";
			this.AESExemptionCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 17, true);
			this.AESExemptionCodeTextBox.TabIndex = 13;
			// 
			// AESITNNumbersUserControl
			//
			this.BindingSource.SetBindingMember(this.AESITNNumbersUserControl, ".");
			this.AESITNNumbersUserControl.Name = "AESITNNumbersUserControl";
			this.AESITNNumbersUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 23, true);
			// 
			// InBondNumbersUserControl
			//
			this.BindingSource.SetBindingMember(this.InBondNumbersUserControl, ".");
			this.InBondNumbersUserControl.Name = "InBondNumbersUserControl";
			this.InBondNumbersUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 23, true);
			// 
			// USExportManifestBillUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PriorTransportationModeDropEdit);
			this.Controls.Add(this.BoardedQuantityCalcEdit);
			this.Controls.Add(this.BoardedWeightCalcDropEdit);
			this.Controls.Add(this.FinalDestinationPortUserControl);
			this.Controls.Add(this.ArrivalPortUserControl);
			this.Controls.Add(this.DeparturePortUserControl);
			this.Controls.Add(this.LadingPortUserControl);
			this.Controls.Add(this.UnladingPortUserControl);
			this.Controls.Add(this.OriginPortUserControl);
			this.Controls.Add(this.SpecialCargoCodesDropEdit);
			this.Controls.Add(this.PlaceOfReceiptTextBox);
			this.Controls.Add(this.AESExemptionCodeTextBox);
			this.Controls.Add(this.AESITNNumbersUserControl);
			this.Controls.Add(this.InBondNumbersUserControl);
			this.Name = "USExportManifestBillUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1013, 650, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PriorTransportationModeDropEdit.ResumeLayout(true);
			this.PriorTransportationModeDropEdit.PerformLayout();
			this.FinalDestinationPortUserControl.ResumeLayout(true);
			this.FinalDestinationPortUserControl.PerformLayout();
			this.AESITNNumbersUserControl.ResumeLayout(true);
			this.AESITNNumbersUserControl.PerformLayout();
			this.InBondNumbersUserControl.ResumeLayout(true);
			this.InBondNumbersUserControl.PerformLayout();
			this.ArrivalPortUserControl.ResumeLayout(true);
			this.ArrivalPortUserControl.PerformLayout();
			this.DeparturePortUserControl.ResumeLayout(true);
			this.DeparturePortUserControl.PerformLayout();
			this.LadingPortUserControl.ResumeLayout(true);
			this.LadingPortUserControl.PerformLayout();
			this.UnladingPortUserControl.ResumeLayout(true);
			this.UnladingPortUserControl.PerformLayout();
			this.OriginPortUserControl.ResumeLayout(true);
			this.OriginPortUserControl.PerformLayout();
			this.SpecialCargoCodesDropEdit.ResumeLayout(true);
			this.SpecialCargoCodesDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal ZDropEdit PriorTransportationModeDropEdit;
		internal ZArchitecture.ZCalcEdit BoardedQuantityCalcEdit;
		internal ZCalcDropEdit BoardedWeightCalcDropEdit;
		internal FinalDestinationPortUserControl FinalDestinationPortUserControl;
		internal ArrivalPortUserControl ArrivalPortUserControl;
		internal DeparturePortUserControl DeparturePortUserControl;
		internal LadingPortUserControl LadingPortUserControl;
		internal UnladingPortUserControl UnladingPortUserControl;
		internal OriginPortUserControl OriginPortUserControl;
		internal ZDropEdit SpecialCargoCodesDropEdit;
		internal ZArchitecture.ZTextBox PlaceOfReceiptTextBox;
		internal ZArchitecture.ZTextBox AESExemptionCodeTextBox;
		internal CusEntryNumbersUserControl AESITNNumbersUserControl;
		internal CusEntryNumbersUserControl InBondNumbersUserControl;
	}
}
