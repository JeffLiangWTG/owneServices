using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.ETrade.GUI
{
	public sealed class ETradeLayouts : IPanelLayoutProvider
	{
		PanelLayout ETradeDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => ETradeDetails;

		public ETradeLayouts()
		{
			ETradeDetails = CreateETradeDetailsLayout();
		}

		PanelLayout CreateETradeDetailsLayout()
		{
			var builder = new ETradeLayoutBuilder();
			var common = builder.CommonBag;
			var etrade = ETradeControlBag.Instance;
			builder.AddControlBag(etrade);

			builder.AddColumn();
			builder.Add(common.CountryTextBox, ControlWidthClass.Medium);
			builder.Add(etrade.RegistrationDateLongDateEdit, ControlWidthClass.Long);
			builder.Add(common.RegistrationNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.TransportModeDropEdit, ControlWidthClass.Long);
			builder.Add(common.NatureDropEdit, ControlWidthClass.Long);
			builder.Add(common.AgentTypeDropEdit, ControlWidthClass.Long);
			builder.Add(common.ContainerModeDropEdit, ControlWidthClass.Long);
			builder.Add(etrade.DepartureFlightTextBox, ControlWidthClass.Long);
			builder.Add(common.ConveyanceCountryCodeFindBox, ControlWidthClass.Long);
			builder.Add(etrade.DepartureCountryCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.VoyageFlightTextBox, ControlWidthClass.Medium);
			builder.Add(common.PortOfFirstArrivalCodeFindBox, ControlWidthClass.Long);
			builder.Add(etrade.DateAtCustomsOfficeDateEdit, ControlWidthClass.Auto);
			builder.Add(etrade.TransshipmentCountryCodeFindBox, ControlWidthClass.Long);
			builder.Add(etrade.TransshipmentLocationTextBox, ControlWidthClass.Long);
			builder.Add(etrade.TransshipmentReferenceTextBox, ControlWidthClass.Long);
			builder.Add(etrade.TransshipmentConveyanceCountryCodeFindBox, ControlWidthClass.Long);
			builder.Add(etrade.PreviousContainerNoTextBox, ControlWidthClass.Long);
			builder.Add(etrade.NewContainerNoTextBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(etrade.MessageModeDropEdit, ControlWidthClass.Long);
			builder.Add(common.JobReferenceTextBox, ControlWidthClass.Medium);
			builder.Add(common.MessageStatusTextBox, ControlWidthClass.Medium);
			builder.Add(common.MessageStatusDropEdit, ControlWidthClass.Long);
			builder.Add(common.CustomsStatusDropEdit, ControlWidthClass.Long);
			builder.Add(etrade.ProcedureCodeFindBox, ControlWidthClass.Long);
			builder.Add(etrade.PresentationCustomsOfficeDropEdit, ControlWidthClass.Long);
			builder.Add(etrade.ImportExportCustomsOfficeDropEdit, ControlWidthClass.Long);
			builder.Add(etrade.DischargeLoadingCustomsOfficeDropEdit, ControlWidthClass.Long);
			builder.Add(common.CustomsOfficeDropEdit, ControlWidthClass.Long);
			builder.Add(common.CarrierAddressControl, ControlWidthClass.Long);
			builder.Add(etrade.GoodsLocationCodeCodeFindBox, ControlWidthClass.Long);
			builder.Add(etrade.LocationInformationTextBox, ControlWidthClass.Long);
			builder.Add(etrade.GoodsDescriptionTextBox, ControlWidthClass.Long);
			builder.Add(etrade.CustomsValueCalcFindBox, ControlWidthClass.Long);
			builder.Add(etrade.ExchangeRateCalcEdit, ControlWidthClass.Long);
			builder.Add(etrade.FreightValueCalcFindBox, ControlWidthClass.Long);
			builder.Add(etrade.InsuranceValueCalcFindBox, ControlWidthClass.Long);
			builder.Add(etrade.OtherValueCalcFindBox, ControlWidthClass.Long);
			builder.Add(etrade.GuaranteeTypeDropEdit, ControlWidthClass.Long);
			builder.Add(etrade.GuaranteeRefNoTextBox, ControlWidthClass.Long);
			builder.Add(etrade.GuaranteeAmountCalcEdit, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(etrade.TempRegNoTextBox, ControlWidthClass.Long);
			builder.Add(etrade.TempRegNoDateEdit, ControlWidthClass.Long);
			builder.Add(etrade.DischargeRecordNoTextBox, ControlWidthClass.Long);
			builder.Add(etrade.DischargeRecordNoDateEdit, ControlWidthClass.Long);
			builder.Add(etrade.ClosureNoTextBox, ControlWidthClass.Long);
			builder.Add(etrade.ClosureNoDateEdit, ControlWidthClass.Long);
			builder.Add(etrade.InspectionClerkTextBox, ControlWidthClass.Long);
			builder.Add(etrade.NumberOfBillsTextBox, ControlWidthClass.Long);
			builder.Add(etrade.TotalBoxQtyTextBox, ControlWidthClass.Long);
			builder.Add(etrade.StampTaxValueTextBox, ControlWidthClass.Auto);

			builder.SetCaption(etrade.ImportExportCustomsOfficeDropEdit, h => h.ImportExportCustomsOfficeLabel, h => h.AMA_NatureInfo);
			builder.SetCaption(etrade.DischargeLoadingCustomsOfficeDropEdit, h => h.DischargeLoadingCustomsOfficeLabel, h => h.AMA_NatureInfo);
			builder.SetCaption(etrade.DepartureFlightTextBox, h => h.DepartureFlightLabel, h => h.DepartureFlightInfo);
			builder.SetVisibility(common.ContainerModeDropEdit, h => h.IsAir || h.IsSea || h.IsRoad, h => h.AMA_TransportModeInfo);
			builder.SetVisibility(common.ConveyanceCountryCodeFindBox, h => h.IsSea || h.IsAir, h => h.AMA_TransportModeInfo);

			return builder.Build();
		}
	}
}
