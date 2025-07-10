using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class TrackingMAWBHeaderColumnProvider : GridColumnProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();

			var aWBNumberColumn = new ZHyperLinkColumn(Res.GetString("a57ef3d6-6b99-424e-a467-f8399d7f78b5", "MAWB"), ExportAWBHeaderSchema.Constants.EH_WayBillNumber)
			{
				DataNavigateUrlFormatString = this.UrlFormatWithAppRoot(TrackingConstants.RelativePath.MAWBDetailsPage) + (NoResString)"?Ref={0}", // Partial URL
				DataNavigateUrlFields = new string[1] { "PK" },
				ColumnKey = WebTracker.Grids.MAWB.MAWB
			};
			AddToDictionaryAsRequired(aWBNumberColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_AWBOriginCode);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("b4a9f558-e55e-4a23-9b6e-fd0df42cb33d", "Origin"), ExportAWBHeaderSchema.Constants.EH_AWBOriginCode) { ColumnKey = WebTracker.Grids.MAWB.Origin });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_AirportOfDepartureAndRequestRouteText);
			AddToDictionary(new ZTextEditColumn(Res.GetString("cff9a193-43b9-4355-8c6a-e9d24890cc6c", "Departure and Routing"), ExportAWBHeaderSchema.Constants.EH_AirportOfDepartureAndRequestRouteText) { ColumnKey = WebTracker.Grids.MAWB.DepartureAndRouting });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_AirportOfDestinationCode);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("c1d12665-e51e-478d-9553-970e5b86d9a6", "Destination"), ExportAWBHeaderSchema.Constants.EH_AirportOfDestinationCode) { ColumnKey = WebTracker.Grids.MAWB.Destination });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_AirportOfDestinationText);
			AddToDictionary(new ZTextEditColumn(Res.GetString("dbbd0ab6-9ea1-4163-a500-c005cb6ed34f", "Destination Name"), ExportAWBHeaderSchema.Constants.EH_AirportOfDestinationText) { ColumnKey = WebTracker.Grids.MAWB.DestinationName });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingMAWBHeader)null).EH_AWBIssueDate);
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("8aeb8e87-5aac-444f-9431-06b02b9b530c", "Issue Date"), ExportAWBHeaderSchema.Constants.EH_AWBIssueDate) { ColumnKey = WebTracker.Grids.MAWB.IssueDate });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_AWBIssuePlace);
			AddToDictionary(new ZTextEditColumn(Res.GetString("87ee98b4-8404-42f9-a239-a4124210f75e", "Issue Place"), ExportAWBHeaderSchema.Constants.EH_AWBIssuePlace) { ColumnKey = WebTracker.Grids.MAWB.IssuePlace });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_To1st);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("d2ed70d3-330f-4147-82dd-d11369a8bd27", "To 1st"), ExportAWBHeaderSchema.Constants.EH_To1st) { ColumnKey = WebTracker.Grids.MAWB.To1st });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_By1st);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("b9ca2e49-8986-4035-82e8-d737cde449d3", "By 1st"), ExportAWBHeaderSchema.Constants.EH_By1st) { ColumnKey = WebTracker.Grids.MAWB.By1st });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_To2nd);
			AddToDictionary(new ZTextEditColumn(Res.GetString("357976bb-3690-464c-9467-9d88e228ff9d", "To 2nd"), ExportAWBHeaderSchema.Constants.EH_To2nd) { ColumnKey = WebTracker.Grids.MAWB.To2nd });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_By2nd);
			AddToDictionary(new ZTextEditColumn(Res.GetString("8b62e07e-845b-4739-b69b-ddc6d927d02b", "By 2nd"), ExportAWBHeaderSchema.Constants.EH_By2nd) { ColumnKey = WebTracker.Grids.MAWB.By2nd });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_To3rd);
			AddToDictionary(new ZTextEditColumn(Res.GetString("ce9ec7e5-41db-4585-bc63-a2169f9b9e52", "To 3rd"), ExportAWBHeaderSchema.Constants.EH_To3rd) { ColumnKey = WebTracker.Grids.MAWB.To3rd });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_By3rd);
			AddToDictionary(new ZTextEditColumn(Res.GetString("3da3ef79-a011-4f94-b8bc-ae4a47c72456", "By 3rd"), ExportAWBHeaderSchema.Constants.EH_By3rd) { ColumnKey = WebTracker.Grids.MAWB.By3rd });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_Booking1stCarrier);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("93b38450-93b0-4684-bf19-1bee389a1c8c", "1st Carrier"), ExportAWBHeaderSchema.Constants.EH_Booking1stCarrier) { ColumnKey = WebTracker.Grids.MAWB.FirstCarrier });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_Booking1stFlight);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("6b0ffb78-6784-4259-81ba-4eabbe5a0e50", "1st Flight"), ExportAWBHeaderSchema.Constants.EH_Booking1stFlight) { ColumnKey = WebTracker.Grids.MAWB.FirstFlight });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_Booking1stFlightDate);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("6136413d-d414-4e4f-be7c-35f0e8120bb8", "1st Flight Date"), ExportAWBHeaderSchema.Constants.EH_Booking1stFlightDate) { ColumnKey = WebTracker.Grids.MAWB.FirstFlightDate });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_Booking2ndCarrier);
			AddToDictionary(new ZTextEditColumn(Res.GetString("9ba6599d-93ca-4fec-bc5e-60e2fc832bb0", "2nd Carrier"), ExportAWBHeaderSchema.Constants.EH_Booking2ndCarrier) { ColumnKey = WebTracker.Grids.MAWB.SecondCarrier });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_Booking2ndFlight);
			AddToDictionary(new ZTextEditColumn(Res.GetString("0bf1e2ab-fab7-484c-b56e-430b440099f5", "2nd Flight"), ExportAWBHeaderSchema.Constants.EH_Booking2ndFlight) { ColumnKey = WebTracker.Grids.MAWB.SecondFlight });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_Booking2ndFlightDate);
			AddToDictionary(new ZTextEditColumn(Res.GetString("ef887f24-73b4-4af8-8460-8d9ce2f7338f", "2nd Flight Date"), ExportAWBHeaderSchema.Constants.EH_Booking2ndFlightDate) { ColumnKey = WebTracker.Grids.MAWB.SecondFlightDate });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_Currency);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("9198b06b-a0f7-4e39-a40b-5904d538b4ec", "Currency"), ExportAWBHeaderSchema.Constants.EH_Currency) { ColumnKey = WebTracker.Grids.MAWB.Currency });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ChargesCode);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("6e5b5f27-383c-4dd6-aea0-d76d2845ba75", "Charge Code"), ExportAWBHeaderSchema.Constants.EH_ChargesCode) { ColumnKey = WebTracker.Grids.MAWB.ChargeCode });

			//ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_WeightPrepaidCollect);
			//AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("179ebba8-6ccf-472e-9a87-bb947b64df6e", "WT/VAL"), TrackingMAWBHeader.Schema.EH_WeightPrepaidCollect));

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_OtherPrepaidCollect);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("179ebba8-6ccf-472e-9a87-bb947b64df6e", "WT/VAL"), ExportAWBHeaderSchema.Constants.EH_OtherPPDCOL) { ColumnKey = WebTracker.Grids.MAWB.WtVal });

			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingMAWBHeader)null).EH_DeclaredValue);
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("441c6833-1f4f-40b4-aaac-ba45d6ffcbff", "Declared Value"), ExportAWBHeaderSchema.Constants.EH_DeclaredValue) { ColumnKey = WebTracker.Grids.MAWB.DeclaredValue });

			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingMAWBHeader)null).EH_CustomsValue);
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("7984323e-ee32-4bee-a40f-369b086f988f", "Customs Value"), ExportAWBHeaderSchema.Constants.EH_CustomsValue) { ColumnKey = WebTracker.Grids.MAWB.CustomsValue });

			//ZBindToChecker.CheckBindTo((ZDecimal)((TrackingMAWBHeader)null).EH_TotalPPD);
			//AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("96d76a1e-2f9b-4433-813f-fc55152390b2", "Total PPD"), TrackingMAWBHeader.Schema.EH_TotalPPD));

			//ZBindToChecker.CheckBindTo((ZDecimal)((TrackingMAWBHeader)null).EH_TotalCOL);
			//AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("1bc41a1e-6861-4ea4-bf97-1d95406fe97f", "Total COL"), TrackingMAWBHeader.Schema.EH_TotalCOL));

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).AWBMessagingStatusDescription);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("5bdefc58-2edf-4248-913c-cb70c62b9bc9", "Status"), TrackingMAWBHeader.Schema.AWBMessagingStatusDescription) { ColumnKey = WebTracker.Grids.MAWB.AWBStatusDescription });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ShipperName);
			AddToDictionary(new ZTextEditColumn(Res.GetString("f9c7869d-c1a9-4e9d-981f-1895c46dd2be", "Shipper Name"), ExportAWBHeaderSchema.Constants.EH_ShipperName) { ColumnKey = WebTracker.Grids.MAWB.ShipperName });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ShipperAddress);
			AddToDictionary(new ZTextEditColumn(Res.GetString("4f7ba3b0-810e-4bd4-9008-35e9e5de9d93", "Shipper Address"), ExportAWBHeaderSchema.Constants.EH_ShipperAddress) { ColumnKey = WebTracker.Grids.MAWB.ShipperAddress });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ShipperAddress2);
			AddToDictionary(new ZTextEditColumn(Res.GetString("400e1943-5c29-4c11-ae53-b8b9378575b7", "Shipper Address 2"), ExportAWBHeaderSchema.Constants.EH_ShipperAddress2) { ColumnKey = WebTracker.Grids.MAWB.ShipperAddress2 });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ShipperPlace);
			AddToDictionary(new ZTextEditColumn(Res.GetString("98cfa881-a2e5-4b27-97cb-d7758d429e95", "Shipper Place"), ExportAWBHeaderSchema.Constants.EH_ShipperPlace) { ColumnKey = WebTracker.Grids.MAWB.ShipperPlace });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ShipperState);
			AddToDictionary(new ZTextEditColumn(Res.GetString("76df787e-7ad3-432e-b5a3-36294931b178", "Shipper State"), ExportAWBHeaderSchema.Constants.EH_ShipperState) { ColumnKey = WebTracker.Grids.MAWB.ShipperState });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ShipperPostCode);
			AddToDictionary(new ZTextEditColumn(Res.GetString("2bd56280-b956-4795-9146-ef702434524e", "Shipper Postal Code"), ExportAWBHeaderSchema.Constants.EH_ShipperPostCode) { ColumnKey = WebTracker.Grids.MAWB.ShipperPostCode });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ShipperCountryCode);
			AddToDictionary(new ZTextEditColumn(Res.GetString("71a15c4a-e673-4bea-a934-7e05696c25f0", "Shipper Country/Region"), ExportAWBHeaderSchema.Constants.EH_ShipperCountryCode) { ColumnKey = WebTracker.Grids.MAWB.ShipperCountryCode });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ShipperContactDetail);
			AddToDictionary(new ZTextEditColumn(Res.GetString("9ee76ed4-584d-441f-a958-c806addec384", "Shipper Phone"), ExportAWBHeaderSchema.Constants.EH_ShipperContactDetail) { ColumnKey = WebTracker.Grids.MAWB.ShipperContactDetail });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ConsigneeName);
			AddToDictionary(new ZTextEditColumn(Res.GetString("60642980-6b17-43b9-b647-9cdd91525192", "Consignee Name"), ExportAWBHeaderSchema.Constants.EH_ConsigneeName) { ColumnKey = WebTracker.Grids.MAWB.ConsigneeName });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ConsigneeAddress);
			AddToDictionary(new ZTextEditColumn(Res.GetString("3a7d3437-b8ed-4e35-9cba-32fba8c64d58", "Consignee Address"), ExportAWBHeaderSchema.Constants.EH_ConsigneeAddress) { ColumnKey = WebTracker.Grids.MAWB.ConsigneeAddress });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ConsigneeAddress2);
			AddToDictionary(new ZTextEditColumn(Res.GetString("422680d6-f30e-4488-bcc9-6229db95e6e6", "Consignee Address 2"), ExportAWBHeaderSchema.Constants.EH_ConsigneeAddress2) { ColumnKey = WebTracker.Grids.MAWB.ConsigneeAddress2 });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ConsigneePlace);
			AddToDictionary(new ZTextEditColumn(Res.GetString("ca01b723-8038-43e5-ba96-62a734bb39ee", "Consignee Place"), ExportAWBHeaderSchema.Constants.EH_ConsigneePlace) { ColumnKey = WebTracker.Grids.MAWB.ConsigneePlace });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ConsigneeState);
			AddToDictionary(new ZTextEditColumn(Res.GetString("d1ecbd73-1c47-4be4-a139-157b52f33ec5", "Consignee State"), ExportAWBHeaderSchema.Constants.EH_ConsigneeState) { ColumnKey = WebTracker.Grids.MAWB.ConsigneeState });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ConsigneePostCode);
			AddToDictionary(new ZTextEditColumn(Res.GetString("cd9fdc04-e0df-45c3-adcd-f725eb6999ef", "Consignee Postal Code"), ExportAWBHeaderSchema.Constants.EH_ConsigneePostCode) { ColumnKey = WebTracker.Grids.MAWB.ConsigneePostCode });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ConsigneeCountryCode);
			AddToDictionary(new ZTextEditColumn(Res.GetString("92a995b0-f2e1-4155-ba09-80d76b1e585b", "Consignee Country/Region"), ExportAWBHeaderSchema.Constants.EH_ConsigneeCountryCode) { ColumnKey = WebTracker.Grids.MAWB.ConsigneeCountryCode });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ConsigneeContactDetail);
			AddToDictionary(new ZTextEditColumn(Res.GetString("1ff1ccc6-90a0-4021-a8ee-9c5ad2dacb4e", "Consignee Phone"), ExportAWBHeaderSchema.Constants.EH_ConsigneeContactDetail) { ColumnKey = WebTracker.Grids.MAWB.ConsigneeContactDetail });
		}
	}
}
