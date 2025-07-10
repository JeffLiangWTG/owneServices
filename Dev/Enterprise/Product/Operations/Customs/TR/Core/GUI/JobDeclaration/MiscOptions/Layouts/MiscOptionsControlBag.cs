using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI
{
	public sealed class MiscOptionsControlBag : ControlBag
	{
		public static MiscOptionsControlBag Instance => instance ?? (instance = new MiscOptionsControlBag());

		[ThreadStatic]
		static MiscOptionsControlBag instance;

		MiscOptionsControlBag()
		{
			ManifestToOpenUserControl = RegisterControl(nameof(MiscOptionsUserControl.ManifestToOpenUserControl));
			ExportersUnionInfoUserControl = RegisterControl(nameof(MiscOptionsUserControl.ExportersUnionInfoUserControl));
			GuaranteeInfoOptionsSeparatorUserControl = RegisterControl(nameof(MiscOptionsUserControl.GuaranteeInfoOptionsSeparatorUserControl));
			GuaranteeGuidFindBox = RegisterControl(nameof(MiscOptionsUserControl.GuaranteeGuidFindBox));
			BondTypeDropEdit = RegisterControl(nameof(MiscOptionsUserControl.BondTypeDropEdit));
			ReferenceNumberTextBox = RegisterControl(nameof(MiscOptionsUserControl.ReferenceNumberTextBox));
			GuaranteeDescriptionTextBox = RegisterControl(nameof(MiscOptionsUserControl.GuaranteeDescriptionTextBox));
			DedicatedAmountCalcEdit = RegisterControl(nameof(MiscOptionsUserControl.DedicatedAmountCalcEdit));
			RatioCalcEdit = RegisterControl(nameof(MiscOptionsUserControl.RatioCalcEdit));
			AmountCalcEdit = RegisterControl(nameof(MiscOptionsUserControl.AmountCalcEdit));
			SupportingInformationUserControl = RegisterControl(nameof(MiscOptionsUserControl.SupportingInformationUserControl));
			TotalAmountsSeparatorUserControl = RegisterControl(nameof(MiscOptionsUserControl.TotalAmountsSeparatorUserControl));
			InvoiceCountCalcEdit = RegisterControl(nameof(MiscOptionsUserControl.InvoiceCountCalcEdit));
			TotalInvoiceAmountLocalCurrencyControl = RegisterControl(nameof(MiscOptionsUserControl.TotalInvoiceAmountLocalCurrencyControl));
			TotalFreeOnBoardLocalCurrencyControl = RegisterControl(nameof(MiscOptionsUserControl.TotalFreeOnBoardLocalCurrencyControl));
			TotalFreightLocalCurrencyControl = RegisterControl(nameof(MiscOptionsUserControl.TotalFreightLocalCurrencyControl));
			TotalInsuranceLocalCurrencyControl = RegisterControl(nameof(MiscOptionsUserControl.TotalInsuranceLocalCurrencyControl));
			TotalOverseasLocalCurrencyControl = RegisterControl(nameof(MiscOptionsUserControl.TotalOverseasLocalCurrencyControl));
			LocalTotalChargesLocalCurrencyControl = RegisterControl(nameof(MiscOptionsUserControl.LocalTotalChargesLocalCurrencyControl));
		}

		protected override Control CreateTemplate() => new MiscOptionsUserControl();

		public ControlReference ManifestToOpenUserControl { get; }
		public ControlReference ExportersUnionInfoUserControl { get; }
		public ControlReference BondTypeDropEdit { get; }
		public ControlReference AmountCalcEdit { get; }
		public ControlReference GuaranteeGuidFindBox { get; }
		public ControlReference GuaranteeInfoOptionsSeparatorUserControl { get; }
		public ControlReference RatioCalcEdit { get; }
		public ControlReference ReferenceNumberTextBox { get; }
		public ControlReference DedicatedAmountCalcEdit { get; }
		public ControlReference GuaranteeDescriptionTextBox { get; }
		public ControlReference SupportingInformationUserControl { get; }
		public ControlReference TotalAmountsSeparatorUserControl { get; }
		public ControlReference InvoiceCountCalcEdit { get; }
		public ControlReference TotalInvoiceAmountLocalCurrencyControl { get; }
		public ControlReference TotalFreeOnBoardLocalCurrencyControl { get; }
		public ControlReference TotalFreightLocalCurrencyControl { get; }
		public ControlReference TotalInsuranceLocalCurrencyControl { get; }
		public ControlReference TotalOverseasLocalCurrencyControl { get; }
		public ControlReference LocalTotalChargesLocalCurrencyControl { get; }
	}
}
