using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.ETrade.GUI
{
	public class ETradeControlBag : ControlBag
	{
		public static ETradeControlBag Instance => eTradeControlBag.Value;

		public ETradeControlBag()
		{
			RegistrationDateLongDateEdit = RegisterControl(nameof(ETradeMainUserControl.RegistrationDateLongDateEdit));
			MessageModeDropEdit = RegisterControl(nameof(ETradeMainUserControl.MessageModeDropEdit));
			TransshipmentCountryCodeFindBox = RegisterControl(nameof(ETradeMainUserControl.TransshipmentCountryCodeFindBox));
			DepartureCountryCodeFindBox = RegisterControl(nameof(ETradeMainUserControl.DepartureCountryCodeFindBox));
			TransshipmentConveyanceCountryCodeFindBox = RegisterControl(nameof(ETradeMainUserControl.TransshipmentConveyanceCountryCodeFindBox));
			DepartureFlightTextBox = RegisterControl(nameof(ETradeMainUserControl.DepartureFlightTextBox));
			TransshipmentLocationTextBox = RegisterControl(nameof(ETradeMainUserControl.TransshipmentLocationTextBox));
			TransshipmentReferenceTextBox = RegisterControl(nameof(ETradeMainUserControl.TransshipmentReferenceTextBox));
			PreviousContainerNoTextBox = RegisterControl(nameof(ETradeMainUserControl.PreviousContainerNoTextBox));
			NewContainerNoTextBox = RegisterControl(nameof(ETradeMainUserControl.NewContainerNoTextBox));
			TempRegNoTextBox = RegisterControl(nameof(ETradeMainUserControl.TempRegNoTextBox));
			TempRegNoDateEdit = RegisterControl(nameof(ETradeMainUserControl.TempRegNoDateEdit));
			DischargeRecordNoTextBox = RegisterControl(nameof(ETradeMainUserControl.DischargeRecordNoTextBox));
			DischargeRecordNoDateEdit = RegisterControl(nameof(ETradeMainUserControl.DischargeRecordNoDateEdit));
			ClosureNoTextBox = RegisterControl(nameof(ETradeMainUserControl.ClosureNoTextBox));
			ClosureNoDateEdit = RegisterControl(nameof(ETradeMainUserControl.ClosureNoDateEdit));
			InspectionClerkTextBox = RegisterControl(nameof(ETradeMainUserControl.InspectionClerkTextBox));
			NumberOfBillsTextBox = RegisterControl(nameof(ETradeMainUserControl.NumberOfBillsTextBox));
			TotalBoxQtyTextBox = RegisterControl(nameof(ETradeMainUserControl.TotalBoxQtyTextBox));
			GoodsLocationCodeCodeFindBox = RegisterControl(nameof(ETradeMainUserControl.GoodsLocationCodeCodeFindBox));
			LocationInformationTextBox = RegisterControl(nameof(ETradeMainUserControl.LocationInformationTextBox));
			ProcedureCodeFindBox = RegisterControl(nameof(ETradeMainUserControl.ProcedureCodeFindBox));
			PresentationCustomsOfficeDropEdit = RegisterControl(nameof(ETradeMainUserControl.PresentationCustomsOfficeDropEdit));
			ImportExportCustomsOfficeDropEdit = RegisterControl(nameof(ETradeMainUserControl.ImportExportCustomsOfficeDropEdit));
			DischargeLoadingCustomsOfficeDropEdit = RegisterControl(nameof(ETradeMainUserControl.DischargeLoadingCustomsOfficeDropEdit));
			GoodsDescriptionTextBox = RegisterControl(nameof(ETradeMainUserControl.GoodsDescriptionTextBox));
			CustomsValueCalcFindBox = RegisterControl(nameof(ETradeMainUserControl.CustomsValueCalcFindBox));
			ExchangeRateCalcEdit = RegisterControl(nameof(ETradeMainUserControl.ExchangeRateCalcEdit));
			OtherValueCalcFindBox = RegisterControl(nameof(ETradeMainUserControl.OtherValueCalcFindBox));
			FreightValueCalcFindBox = RegisterControl(nameof(ETradeMainUserControl.FreightValueCalcFindBox));
			InsuranceValueCalcFindBox = RegisterControl(nameof(ETradeMainUserControl.InsuranceValueCalcFindBox));
			GuaranteeTypeDropEdit = RegisterControl(nameof(ETradeMainUserControl.GuaranteeTypeDropEdit));
			GuaranteeRefNoTextBox = RegisterControl(nameof(ETradeMainUserControl.GuaranteeRefNoTextBox));
			GuaranteeAmountCalcEdit = RegisterControl(nameof(ETradeMainUserControl.GuaranteeAmountCalcEdit));
			DateAtCustomsOfficeDateEdit = RegisterControl(nameof(ETradeMainUserControl.DateAtCustomsOfficeDateEdit));
			StampTaxValueTextBox = RegisterControl(nameof(ETradeMainUserControl.StampTaxValueTextBox));
		}
		public ControlReference RegistrationDateLongDateEdit { get; }
		public ControlReference TransshipmentCountryCodeFindBox { get; }
		public ControlReference DepartureCountryCodeFindBox { get; }
		public ControlReference TransshipmentConveyanceCountryCodeFindBox { get; }
		public ControlReference DepartureFlightTextBox { get; }
		public ControlReference TransshipmentLocationTextBox { get; }
		public ControlReference TransshipmentReferenceTextBox { get; }
		public ControlReference PreviousContainerNoTextBox { get; }
		public ControlReference NewContainerNoTextBox { get; }
		public ControlReference TempRegNoTextBox { get; }
		public ControlReference TempRegNoDateEdit { get; }
		public ControlReference DischargeRecordNoTextBox { get; }
		public ControlReference DischargeRecordNoDateEdit { get; }
		public ControlReference ClosureNoTextBox { get; }
		public ControlReference ClosureNoDateEdit { get; }
		public ControlReference InspectionClerkTextBox { get; }
		public ControlReference NumberOfBillsTextBox { get; }
		public ControlReference TotalBoxQtyTextBox { get; }
		public ControlReference GoodsLocationCodeCodeFindBox { get; }
		public ControlReference LocationInformationTextBox { get; }
		public ControlReference ProcedureCodeFindBox { get; }
		public ControlReference PresentationCustomsOfficeDropEdit { get; }
		public ControlReference ImportExportCustomsOfficeDropEdit { get; }
		public ControlReference DischargeLoadingCustomsOfficeDropEdit { get; }
		public ControlReference GoodsDescriptionTextBox { get; }
		public ControlReference CustomsValueCalcFindBox { get; }
		public ControlReference ExchangeRateCalcEdit { get; }
		public ControlReference OtherValueCalcFindBox { get; }
		public ControlReference FreightValueCalcFindBox { get; }
		public ControlReference InsuranceValueCalcFindBox { get; }
		public ControlReference GuaranteeTypeDropEdit { get; }
		public ControlReference GuaranteeRefNoTextBox { get; }
		public ControlReference GuaranteeAmountCalcEdit { get; }
		public ControlReference DateAtCustomsOfficeDateEdit { get; }
		public ControlReference StampTaxValueTextBox { get; }
		public ControlReference MessageModeDropEdit { get; }

		protected override Control CreateTemplate() => new ETradeMainUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<ETradeControlBag> eTradeControlBag = new Lazy<ETradeControlBag>(() => new ETradeControlBag());
	}
}
