using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.ZA.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.Manifest.GUI
{
	public class ZAManifestLayouts : IPanelLayoutProvider
	{
		public PanelLayout ManifestDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => ManifestDetails;

		public ZAManifestLayouts()
		{
			ManifestDetails = CreateManifestDetailsLayout();
		}

		PanelLayout CreateManifestDetailsLayout()
		{
			var builder = new ManifestLayoutBuilder<AsycudaManifestHeader>();
			var common = builder.CommonBag;
			var za = ZAManifestControlBag.Instance;
			builder.AddControlBag(za);

			builder.AddColumn();

			builder.Add(common.CountryTextBox, ControlWidthClass.Medium);
			builder.Add(common.RegistrationDateEdit, ControlWidthClass.Medium);
			builder.Add(common.RegistrationNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.TransportModeDropEdit, ControlWidthClass.Long);
			builder.Add(common.ManifestTypeDropEdit, ControlWidthClass.Long);
			builder.Add(common.NatureDropEdit, ControlWidthClass.Long);
			builder.Add(common.AgentTypeDropEdit, ControlWidthClass.Long);
			builder.Add(common.ContainerModeDropEdit, ControlWidthClass.Long);
			builder.Add(common.BuyersConsolidationCheckBox, ControlWidthClass.Long);
			builder.Add(za.VesselCodeFindBox, ControlWidthClass.Long);
			builder.Add(za.RadioCallSignCodeFindBox, ControlWidthClass.Medium);
			builder.Add(common.VoyageFlightTextBox, ControlWidthClass.Medium);
			builder.Add(common.ConveyanceCountryCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.VehicleRegistrationTextBox, ControlWidthClass.Medium);
			builder.Add(common.Trailer1RegNoTextBox, ControlWidthClass.Medium);
			builder.Add(common.Trailer2RegNoTextBox, ControlWidthClass.Medium);
			builder.Add(common.PortOfLoadingCodeFindBox, ControlWidthClass.Long);
			builder.Add(za.EstLoadDateEdit, ControlWidthClass.Auto);
			builder.Add(common.EstDepartureDateEdit, ControlWidthClass.Auto);
			builder.Add(common.PortOfFirstArrivalCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.PortOfDischargeCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.EstArrivalDateEdit, ControlWidthClass.Auto);
			builder.Add(common.CustomsOfficeDropEdit, ControlWidthClass.Long);
			builder.Add(za.CallPurposeCodeDropEdit, ControlWidthClass.Long);
			builder.Add(za.DateAtCustomsOfficeDateEdit, ControlWidthClass.Auto);
			builder.Add(za.PlaceOfEntryDropEdit, ControlWidthClass.Long);
			builder.Add(za.PlaceOfExitDropEdit, ControlWidthClass.Long);
			builder.Add(za.SeparatorTextUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(za.VoyageFlightTextBox, ControlWidthClass.Medium);
			builder.Add(za.TssVesselCodeFindBox, ControlWidthClass.Long);
			builder.Add(za.TssRadioCallSignCodeFindBox, ControlWidthClass.Medium);
			builder.Add(za.TssCargoCarrierCodeCodeFindBox, ControlWidthClass.Long);
			builder.Add(za.DateOfDepartureDateEdit, ControlWidthClass.Medium);

			builder.AddColumn();

			builder.Add(common.JobReferenceTextBox, ControlWidthClass.Medium);
			builder.Add(common.MessageStatusTextBox, ControlWidthClass.Medium);
			builder.Add(common.MessageStatusDropEdit, ControlWidthClass.Long);
			builder.Add(common.CustomsStatusDropEdit, ControlWidthClass.Long);
			builder.Add(common.ManifestNumberFromMasterBillTextBox, ControlWidthClass.Long);
			builder.Add(common.MasterBOLTextBox, ControlWidthClass.Medium);
			builder.Add(common.IssueDateDateEdit, ControlWidthClass.Auto);
			builder.Add(common.CarrierAddressControl, ControlWidthClass.Long);
			builder.Add(common.CarrierCodeTextBox, ControlWidthClass.Medium);
			builder.Add(za.MasterCarrierCodeTextBox, ControlWidthClass.Medium);
			builder.Add(common.ShippingAgentAddressControl, ControlWidthClass.Long);
			builder.Add(common.DeconsolidateAddressControl, ControlWidthClass.Long);
			builder.Add(common.DischargeTerminalAddressControl, ControlWidthClass.Long);
			builder.Add(za.CaseNumberManifestHeaderGroupBox, ControlWidthClass.LongNoCaption);

			builder.SetVisibility(common.DischargeTerminalAddressControl, h => h.IsDischargeTerminalEnabled, h => h.AMA_ManifestTypeInfo);
			builder.SetVisibility(common.DeconsolidateAddressControl, h => h.IsDeconsolidatorEnabled, h => h.AMA_ManifestTypeInfo);

			builder.SetVisibility(za.PlaceOfEntryDropEdit, h => h.PlaceOfEntryVisible, h => h.AMA_TransportModeInfo, h => h.AMA_NatureInfo);
			builder.SetVisibility(za.PlaceOfExitDropEdit, h => h.PlaceOfExitVisible, h => h.AMA_TransportModeInfo, h => h.AMA_NatureInfo);
			builder.SetVisibility(za.DateAtCustomsOfficeDateEdit, h => h.AMA_DateAtCustomsOfficeVisible, h => h.AMA_TransportModeInfo);

			builder.SetVisibility(za.VesselCodeFindBox, h => h.IsSea, h => h.AMA_TransportModeInfo);
			builder.SetVisibility(za.RadioCallSignCodeFindBox, h => h.IsSea, h => h.AMA_TransportModeInfo);

			builder.SetVisibility(za.EstLoadDateEdit, h => h.EstimatedTimeOfLoadingVisible, h => h.AMA_ManifestTypeInfo, h => h.AMA_TransportModeInfo, h => h.AMA_NatureInfo);
			builder.SetVisibility(za.CaseNumberManifestHeaderGroupBox, h => h.CaseNumbersVisible);

			builder.SetVisibility(za.SeparatorTextUserControl, h => h.IsTSS, h => h.AMA_NatureInfo);
			builder.SetVisibility(za.VoyageFlightTextBox, h => h.IsTSS, h => h.AMA_NatureInfo);
			builder.SetVisibility(za.TssVesselCodeFindBox, h => h.IsTSS && h.IsTSSSea, h => h.AMA_NatureInfo, h => h.AMA_TransportModeInfo);
			builder.SetVisibility(za.TssRadioCallSignCodeFindBox, h => h.IsTSS, h => h.AMA_NatureInfo);
			builder.SetVisibility(za.TssCargoCarrierCodeCodeFindBox, h => h.IsTSS, h => h.AMA_NatureInfo);
			builder.SetVisibility(za.DateOfDepartureDateEdit, h => h.IsTSS, h => h.AMA_NatureInfo);
			builder.SetCaption(za.MasterCarrierCodeTextBox, GetCOHCaption, h => h.AMA_ManifestTypeInfo, h => h.AMA_AgentTypeInfo);

			return builder.Build();
		}
		ResourceStringData GetCOHCaption(AsycudaManifestHeader header)
		{
			return (header.AMA_ManifestType.Equals(nameof(Universal.Messaging.CUSCAR.ManifestDocumentType.COH)) && header.AMA_AgentType.Equals(Core.Constants.AgentType.CoLoad))
				? Res.GetData("97EAE193-2548-4B83-B335-45B4E1D28AF4", "Sub-Master Carrier Code")
				: Res.GetData("E5AFBBA6-20D8-44ED-A7B1-BF37788AE90B", "Master Carrier Code");
		}
	}
}
