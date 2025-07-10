using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class LinerAndAgencyBOLColumnProvider : GridColumnProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			ZBindToChecker.CheckBindTo((ZString)((TrackingBillOfLading)null).JS_UniqueConsignRef);
			ZHyperLinkColumn shipmentNumberColumn = new ZHyperLinkColumn(Res.GetString("5af325c6-38aa-4c6c-b75d-a9671fbde1a7", "Shipment #"), TrackingBillOfLading.Schema.JS_UniqueConsignRef) { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.ShipmentNumber };
			shipmentNumberColumn.DataNavigateUrlFormatString = this.UrlFormatWithAppRoot(TrackingConstants.RelativePath.LinerAndAgencyBillOfLadingDetailsPage) + (NoResString)"?Ref={0}"; // Partial URL
			shipmentNumberColumn.DataNavigateUrlFields = new string[] { "PK" };
			AddToDictionaryAsRequired(shipmentNumberColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingBillOfLading)null).JS_HouseBill);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("a77c29c3-6e17-4f47-a69d-748bcbc3625d", "Ocean Bill"), TrackingBillOfLading.Schema.JS_HouseBill) { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.OceanBill });

			ZBindToChecker.CheckBindTo((ZString)((TrackingBillOfLading)null).BookingPartyDocumentaryAddress.E2_CompanyName);
			AddToDictionary(new ZTextEditColumn(Res.GetString("e7750632-555b-4a3c-a1dc-728af308e501", "Booking Party"), "BookingPartyDocumentaryAddress+E2_CompanyName") { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.BookingParty });

			ZBindToChecker.CheckBindTo((ZString)((TrackingBillOfLading)null).ConsignorDocumentaryAddress.E2_CompanyName);
			AddToDictionaryAsDefault(new ZTextEditColumn(FreightDataRegistry.Instance.ConsignorShipperTerminology.Value, "ConsignorDocumentaryAddress+E2_CompanyName") { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.Shipper });

			ZBindToChecker.CheckBindTo((ZString)((TrackingBillOfLading)null).ConsigneeDocumentaryAddress.E2_CompanyName);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("26bd1ebd-0629-4451-a654-11904ade6158", "Consignee"), "ConsigneeDocumentaryAddress+E2_CompanyName") { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.Consignee });

			ZBindToChecker.CheckBindTo((ZString)((TrackingBillOfLading)null).JS_ShipmentStatus);
			AddToDictionaryAsDefault(new ZDropDownListColumn(Res.GetString("81b9ea4d-a8e1-4b1a-9369-6a489a16e0e5", "Status"), TrackingBillOfLading.Schema.JS_ShipmentStatus) { DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly, ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.Status });

			ZBindToChecker.CheckBindTo((ZString)((TrackingBillOfLading)null).Sailing.JX_JV_NKVessel);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("78c33903-20f4-4f92-a4e9-689befc464f6", "Vessel"), "Sailing+JX_JV_NKVessel") { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.Vessel });

			ZBindToChecker.CheckBindTo((ZString)((TrackingBillOfLading)null).Sailing.JX_JV_VoyageFlight);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("d377a733-5330-4f09-bc65-54cb3e42943a", "Voyage No."), "Sailing+JX_JV_VoyageFlight") { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.Voyage });

			ZBindToChecker.CheckBindTo((ZString)((TrackingBillOfLading)null).JS_RL_NKOrigin);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingBillOfLading)null).Lookups.Origins);
			ZCodeFindBoxColumn originColumn = new ZCodeFindBoxColumn(Res.GetString("81c990fc-956f-4473-984b-28db7cf4d76e", "Origin"), TrackingBillOfLading.Schema.JS_RL_NKOrigin, "Lookups.Origins", typeof(TrackingBillOfLading)) { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.Origin };
			originColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AddToDictionary(originColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingBillOfLading)null).JS_NKLoadPort);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingBillOfLading)null).Lookups.Origins);
			ZCodeFindBoxColumn loadPortColumn = new ZCodeFindBoxColumn(Res.GetString("873be9b8-a8ac-4d13-9d1f-f325c949de67", "Load"), TrackingBillOfLading.Schema.JS_NKLoadPort, "Lookups.Origins", typeof(TrackingBillOfLading)) { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.Load };
			loadPortColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AddToDictionaryAsDefault(loadPortColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingBillOfLading)null).JS_NKDischargePort);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingBillOfLading)null).Lookups.Destinations);
			ZCodeFindBoxColumn dischargePortColumn = new ZCodeFindBoxColumn(Res.GetString("6cd9471e-9ad8-43f8-9778-d715a5b4d661", "Disch."), TrackingBillOfLading.Schema.JS_NKDischargePort, "Lookups.Destinations", typeof(TrackingBillOfLading)) { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.DischargePort };
			dischargePortColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AddToDictionaryAsDefault(dischargePortColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingBillOfLading)null).JS_RL_NKDestination);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingBillOfLading)null).Lookups.Destinations);
			ZCodeFindBoxColumn destinationColumn = new ZCodeFindBoxColumn(Res.GetString("eebdcf31-db46-4b4e-bb3a-7b2116a67142", "Dest."), TrackingBillOfLading.Schema.JS_RL_NKDestination, "Lookups.Destinations", typeof(TrackingBillOfLading)) { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.Destination };
			destinationColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AddToDictionary(destinationColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingBillOfLading)null).JS_GoodsDescription);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("ee10c195-fde8-47ad-b0a1-666c2bcbf7dd", "Cargo Desc."), TrackingBillOfLading.Schema.JS_GoodsDescription) { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.CargoDescription });

			ZBindToChecker.CheckBindTo((ZInt)((TrackingBillOfLading)null).JS_OuterPacks);
			ZCalcEditColumn packsColumn = new ZCalcEditColumn(Res.GetString("1c72a264-eb41-440f-800d-a7ea8ca858b0", "Packs"), TrackingBillOfLading.Schema.JS_OuterPacks);

			ZBindToChecker.CheckBindTo((ZString)((TrackingBillOfLading)null).JS_F3_NKPackType);
			ZTextEditColumn packTypeColumn = new ZTextEditColumn(Res.GetString("c87715ca-f50d-4da7-8057-85868085429c", "Type"), TrackingBillOfLading.Schema.JS_F3_NKPackType);
			AddToDictionary(new ZGroupColumn(Res.GetString("1c72a264-eb41-440f-800d-a7ea8ca858b0", "Packs"), packsColumn, packTypeColumn) { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.Packs });

			AddToDictionary(new ZTextEditColumn(Res.GetString("26a1ae9a-d0b4-43f2-a13d-b9c5b4e28872", "Weight"), TrackingBillOfLading.Schema.WeightWithUnits) { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.Weight });

			AddToDictionary(new ZTextEditColumn(Res.GetString("f74fe1d6-38f5-4a39-ab40-16b74f65b8f6", "Volume"), TrackingBillOfLading.Schema.VolumeWithUnits) { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.Volume });

			ZBindToChecker.CheckBindTo((ZString)((TrackingBillOfLading)null).JS_PackingMode);
			AddToDictionary(new ZTextEditColumn(Res.GetString("d360c827-6b13-4025-8f0b-b4d985c08fc5", "Cargo Type"), TrackingBillOfLading.Schema.JS_PackingMode) { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.CargoType });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingBillOfLading)null).JS_Calc_CurrentETD);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("a5fdc527-d455-4ad4-a0e2-a8d5fb7b1c45", "ETD"), TrackingBillOfLading.Schema.JS_Calc_CurrentETD) { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.ETD });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingBillOfLading)null).JS_Calc_CurrentETA);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("df0003f1-b024-4746-835b-63a5a3101bb4", "ETA"), TrackingBillOfLading.Schema.JS_Calc_CurrentETA) { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.ETA });

			ZBindToChecker.CheckBindTo((ZString)((TrackingBillOfLading)null).ConsignorContact);
			AddToDictionary(new ZTextEditColumn(Res.GetString("3a592206-3559-47e8-9b8e-92ca69590939", "Cnr. Contact"), TrackingBillOfLading.Schema.ConsignorContact) { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.ConsignorContact });

			ZBindToChecker.CheckBindTo((ZString)((TrackingBillOfLading)null).JS_BookingReference);
			AddToDictionary(new ZTextEditColumn(Res.GetString("52A1FE66-7C4C-4D13-856D-CC1FAD7DBBA3", "Shipper's Ref#"), TrackingBillOfLading.Schema.JS_BookingReference) { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.ShippersRef });

			ZBindToChecker.CheckBindTo((ZString)((TrackingBillOfLading)null).JS_InterimReceipt);
			AddToDictionary(new ZTextEditColumn(Res.GetString("d607f388-0733-41dc-b171-262bc00dd48d", "Interim #"), TrackingBillOfLading.Schema.JS_InterimReceipt) { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.InterimNumber });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingBillOfLading)null).JS_A_BKD);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("9237f269-a6f5-4c10-9340-230c2247feca", "Booked"), TrackingBillOfLading.Schema.JS_A_BKD) { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.Booked });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingBillOfLading)null).Sailing.JX_DepotReceivalCommences);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("d848ced7-6ea3-40dd-b313-eb8814dc597f", "CFS Recv. Start"), "Sailing+JX_DepotReceivalCommences") { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.LCLReceivalCommences });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingBillOfLading)null).Sailing.JX_DepotCutOff);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("ac6aa694-448b-4d7b-91dc-9e9fbf68317c", "CFS Cut Off"), "Sailing+JX_DepotCutOff") { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.LCLCutOff });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingBillOfLading)null).Sailing.JX_JA_CTOReceivalCommences);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("ec182dd2-e009-4d03-9d4d-d3875735d488", "CTO Recv. Start"), "Sailing+JX_JA_CTOReceivalCommences") { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.FCLReceivalCommences });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingBillOfLading)null).Sailing.JX_JA_CTOCutOff);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("4b9571d3-69e7-449d-9992-16710a37f8da", "CTO Cut Off"), "Sailing+JX_JA_CTOCutOff") { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.FCLCutOff });

			ZBindToChecker.CheckBindTo((ZString)((TrackingBillOfLading)null).JS_INCO);
			AddToDictionary(new ZDropDownListColumn(Res.GetString("714d4439-dfe8-489e-8443-b0e76fa9fe23", "Payment Term"), TrackingBillOfLading.Schema.JS_INCO) { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.PaymentTerms });
		}

		protected override List<int> GetOldColumnsOrder()
		{
			List<int> result = new List<int>();
			result.Add((int)WebTracker.Grids.LinerAndAgencyBillOfLadings.ShipmentNumber);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBillOfLadings.OceanBill);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBillOfLadings.BookingParty);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBillOfLadings.Shipper);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBillOfLadings.Consignee);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBillOfLadings.Status);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBillOfLadings.Vessel);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBillOfLadings.Voyage);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBillOfLadings.Origin);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBillOfLadings.Load);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBillOfLadings.DischargePort);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBillOfLadings.Destination);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBillOfLadings.CargoDescription);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBillOfLadings.Packs);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBillOfLadings.Type);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBillOfLadings.Weight);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBillOfLadings.Volume);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBillOfLadings.CargoType);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBillOfLadings.ETD);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBillOfLadings.ETA);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBillOfLadings.ConsignorContact);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBillOfLadings.ShippersRef);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBillOfLadings.InterimNumber);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBillOfLadings.Booked);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBillOfLadings.LCLReceivalCommences);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBillOfLadings.LCLCutOff);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBillOfLadings.FCLReceivalCommences);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBillOfLadings.FCLCutOff);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBillOfLadings.PaymentTerms);
			return result;
		}
	}
}
