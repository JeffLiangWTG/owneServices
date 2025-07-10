using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class LinerAndAgencyBookingsColumnProvider : GridColumnProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			ZBindToChecker.CheckBindTo((ZString)((TrackingLinerAndAgencyBooking)null).JS_UniqueConsignRef);
			ZHyperLinkColumn shipmentNumberColumn = new ZHyperLinkColumn(Res.GetString("b534fda3-186d-4166-8385-a614e9b1834e", "Shipment #"), TrackingLinerAndAgencyBooking.Schema.JS_UniqueConsignRef) { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.ShipmentNumber };
			shipmentNumberColumn.DataNavigateUrlFormatString = this.UrlFormatWithAppRoot(TrackingConstants.RelativePath.LinerAndAgencyBookingDetailsPage) + (NoResString)"?Ref={0}"; // Partial URL
			shipmentNumberColumn.DataNavigateUrlFields = new string[] { "PK" };
			AddToDictionaryAsRequired(shipmentNumberColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingLinerAndAgencyBooking)null).JS_HouseBill);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("7d813443-fc34-482b-a77c-dbb3326639d2", "Ocean Bill"), TrackingLinerAndAgencyBooking.Schema.JS_HouseBill) { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.OceanBill });

			ZBindToChecker.CheckBindTo((ZString)((TrackingLinerAndAgencyBooking)null).ConsignorDocumentaryAddress.E2_CompanyName);
			AddToDictionary(new ZTextEditColumn(Res.GetString("12db0079-ec05-4965-b9e4-e43c4bbfbb1c", "Consignor"), "ConsignorDocumentaryAddress+E2_CompanyName") { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.Consignor });

			ZBindToChecker.CheckBindTo((ZString)((TrackingLinerAndAgencyBooking)null).ConsigneeDocumentaryAddress.E2_CompanyName);
			AddToDictionary(new ZTextEditColumn(Res.GetString("7477bf95-3d19-45ee-a52d-47504b0b6c99", "Consignee"), "ConsigneeDocumentaryAddress+E2_CompanyName") { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.Consignee });

			ZBindToChecker.CheckBindTo((ZString)((TrackingLinerAndAgencyBooking)null).JS_ShipmentStatus);
			AddToDictionaryAsDefault(new ZDropDownListColumn(Res.GetString("421ef32e-5778-4e52-9875-4b4bf578b4d3", "Status"), TrackingLinerAndAgencyBooking.Schema.JS_ShipmentStatus) { DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly, ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.Status });

			ZBindToChecker.CheckBindTo((ZString)((TrackingLinerAndAgencyBooking)null).Sailing.JX_JV_NKVessel);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("1292db90-5d34-483b-9efc-547c2e1ad209", "Vessel"), "Sailing+JX_JV_NKVessel") { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.Vessel });

			ZBindToChecker.CheckBindTo((ZString)((TrackingLinerAndAgencyBooking)null).Sailing.JX_JV_VoyageFlight);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("30b843d4-a4cd-4134-83e8-d2b3581a6f5e", "Voyage No."), "Sailing+JX_JV_VoyageFlight") { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.Voyage });

			ZBindToChecker.CheckBindTo((ZString)((TrackingLinerAndAgencyBooking)null).JS_RL_NKOrigin);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingLinerAndAgencyBooking)null).Lookups.Origins);
			ZCodeFindBoxColumn originColumn = new ZCodeFindBoxColumn(Res.GetString("88708f4f-0ca8-41b8-98d7-635bc96be0fc", "Origin"), TrackingLinerAndAgencyBooking.Schema.JS_RL_NKOrigin, "Lookups.Origins", typeof(TrackingLinerAndAgencyBooking)) { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.Origin };
			originColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AddToDictionary(originColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingLinerAndAgencyBooking)null).JS_NKLoadPort);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingLinerAndAgencyBooking)null).Lookups.Origins);
			ZCodeFindBoxColumn loadPortColumn = new ZCodeFindBoxColumn(Res.GetString("fd5de398-cd73-482f-b530-ff0ffb561597", "Load"), TrackingLinerAndAgencyBooking.Schema.JS_NKLoadPort, "Lookups.Origins", typeof(TrackingLinerAndAgencyBooking)) { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.Load };
			loadPortColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AddToDictionaryAsDefault(loadPortColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingLinerAndAgencyBooking)null).JS_NKDischargePort);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingLinerAndAgencyBooking)null).Lookups.Destinations);
			ZCodeFindBoxColumn dischargePortColumn = new ZCodeFindBoxColumn(Res.GetString("1f9e92c3-fbbf-4ab5-ab9b-81c8262f2841", "Disch."), TrackingLinerAndAgencyBooking.Schema.JS_NKDischargePort, "Lookups.Destinations", typeof(TrackingLinerAndAgencyBooking)) { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.DischargePort };
			dischargePortColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AddToDictionaryAsDefault(dischargePortColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingLinerAndAgencyBooking)null).JS_RL_NKDestination);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingLinerAndAgencyBooking)null).Lookups.Destinations);
			ZCodeFindBoxColumn destinationColumn = new ZCodeFindBoxColumn(Res.GetString("072e3526-64ec-4a1c-b70d-5ccacba7137d", "Dest."), TrackingLinerAndAgencyBooking.Schema.JS_RL_NKDestination, "Lookups.Destinations", typeof(TrackingLinerAndAgencyBooking)) { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.Destination };
			destinationColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AddToDictionaryAsDefault(destinationColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingLinerAndAgencyBooking)null).JS_GoodsDescription);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("779c2daf-3373-4638-9fa9-0850316e8146", "Cargo Desc."), TrackingLinerAndAgencyBooking.Schema.JS_GoodsDescription) { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.CargoDescription });

			ZBindToChecker.CheckBindTo((ZInt)((TrackingLinerAndAgencyBooking)null).JS_OuterPacks);
			ZCalcEditColumn packsColumn = new ZCalcEditColumn(Res.GetString("3b527973-3312-405f-8a36-c58b260bb603", "Packs"), TrackingLinerAndAgencyBooking.Schema.JS_OuterPacks) { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.Packs };

			ZBindToChecker.CheckBindTo((ZString)((TrackingLinerAndAgencyBooking)null).JS_F3_NKPackType);
			ZTextEditColumn packTypeColumn = new ZTextEditColumn(Res.GetString("45cc0ff4-e830-4772-88df-c3c9f40f0437", "Type"), TrackingLinerAndAgencyBooking.Schema.JS_F3_NKPackType) { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.Type };
			AddToDictionary(new ZGroupColumn(Res.GetString("3b527973-3312-405f-8a36-c58b260bb603", "Packs"), packsColumn, packTypeColumn) { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.Packs });

			AddToDictionary(new ZTextEditColumn(Res.GetString("4019762f-0624-4c3f-9957-2826b2090182", "Weight"), TrackingLinerAndAgencyBooking.Schema.WeightWithUnits) { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.Weight });

			AddToDictionary(new ZTextEditColumn(Res.GetString("23d7c38c-1672-461b-ba21-07e0137b8967", "Volume"), TrackingLinerAndAgencyBooking.Schema.VolumeWithUnits) { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.Volume });

			ZBindToChecker.CheckBindTo((ZString)((TrackingLinerAndAgencyBooking)null).JS_PackingMode);
			AddToDictionary(new ZTextEditColumn(Res.GetString("4d709a98-64a9-40e2-9b4e-4c4469d9b6ca", "Cargo Type"), TrackingLinerAndAgencyBooking.Schema.JS_PackingMode) { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.CargoType });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingLinerAndAgencyBooking)null).Sailing.JX_JA_E_DEP);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("3f04ff30-692a-4c7d-b8a9-67f80da05530", "ETD"), "Sailing+JX_JA_E_DEP") { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.ETD });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingLinerAndAgencyBooking)null).Sailing.JX_JB_E_ARV);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("7c4ea1f9-97ea-4d7f-861b-4b75a37237bc", "ETA"), "Sailing+JX_JB_E_ARV") { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.ETA });

			ZBindToChecker.CheckBindTo((ZString)((TrackingLinerAndAgencyBooking)null).ConsignorContact);
			AddToDictionary(new ZTextEditColumn(Res.GetString("b1f1d180-7eed-4ad5-b9fd-165eefd763ee", "Cnr. Contact"), TrackingLinerAndAgencyBooking.Schema.ConsignorContact) { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.ConsignorContact });

			ZBindToChecker.CheckBindTo((ZString)((TrackingLinerAndAgencyBooking)null).JS_BookingReference);
			AddToDictionary(new ZTextEditColumn(Res.GetString("BEE38064-E19A-4E14-9466-9880E271DB17", "Shipper's Ref#"), TrackingLinerAndAgencyBooking.Schema.JS_BookingReference) { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.ShippersRef });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingLinerAndAgencyBooking)null).JS_A_BKD);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("c368e460-5448-4a4c-be35-14d136f39823", "Booked"), TrackingLinerAndAgencyBooking.Schema.JS_A_BKD) { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.Booked });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingLinerAndAgencyBooking)null).Sailing.JX_DepotReceivalCommences);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("c9178589-ab42-419e-b660-927b2d067477", "CFS Recv."), "Sailing+JX_DepotReceivalCommences") { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.LCLReceivalCommences });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingLinerAndAgencyBooking)null).Sailing.JX_DepotCutOff);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("984be7c6-5144-4384-9bd0-6a6fdc66a198", "CFS Cut"), "Sailing+JX_DepotCutOff") { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.LCLCutOff });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingLinerAndAgencyBooking)null).Sailing.JX_JA_CTOReceivalCommences);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("beeff1d3-a5eb-49e1-86b8-551b36808c2c", "CTO Recv."), "Sailing+JX_JA_CTOReceivalCommences") { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.FCLReceivalCommences });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingLinerAndAgencyBooking)null).Sailing.JX_JA_CTOCutOff);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("6b9b4fdc-da27-40ef-8483-efe3df6fa813", "CTO Cut"), "Sailing+JX_JA_CTOCutOff") { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.FCLCutOff });

			ZBindToChecker.CheckBindTo((ZString)((TrackingLinerAndAgencyBooking)null).Job.JH_Status);
			AddToDictionary(new ZTextEditColumn(Res.GetString("a3b64e1a-fdd7-4603-abbc-63b85578acfa", "Job Status"), "Job+JH_Status") { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.JobStatus });
		}

		protected override List<int> GetOldColumnsOrder()
		{
			List<int> result = new List<int>();
			result.Add((int)WebTracker.Grids.LinerAndAgencyBookings.ShipmentNumber);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBookings.OceanBill);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBookings.Consignor);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBookings.Consignee);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBookings.Status);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBookings.Vessel);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBookings.Voyage);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBookings.Origin);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBookings.Load);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBookings.DischargePort);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBookings.Destination);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBookings.CargoDescription);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBookings.Packs);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBookings.Weight);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBookings.Volume);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBookings.CargoType);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBookings.ConsignorContact);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBookings.ShippersRef);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBookings.Booked);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBookings.LCLReceivalCommences);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBookings.LCLCutOff);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBookings.FCLReceivalCommences);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBookings.FCLCutOff);
			result.Add((int)WebTracker.Grids.LinerAndAgencyBookings.JobStatus);
			return result;
		}
	}
}
