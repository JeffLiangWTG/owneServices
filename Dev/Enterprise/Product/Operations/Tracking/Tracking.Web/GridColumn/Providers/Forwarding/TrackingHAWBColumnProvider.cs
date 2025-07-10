using System;
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
	public class TrackingHAWBHeaderColumnProvider : GridColumnProvider
	{
		public TrackingHAWBHeaderColumnProvider()
			: base()
		{
		}

		public TrackingHAWBHeaderColumnProvider(bool showInPopup)
			: base()
		{
			ShowInPopup = showInPopup;
		}

		readonly bool ShowInPopup;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();

			var aWBNumberColumn = new ZHyperLinkColumn(Res.GetString("5a2c873d-a6c7-438e-b1bf-7728b74b3df2", "HAWB"), ExportAWBHeaderSchema.Constants.EH_WayBillNumber)
			{
				DataNavigateUrlFormatString = this.UrlFormatWithAppRoot(TrackingConstants.RelativePath.HAWBDetailsPage) + (NoResString)"?Ref={0}", // Partial URL
				DataNavigateUrlFields = new string[1] { "PK" },
				ColumnKey = WebTracker.Grids.HAWB.HAWB
			};

			if (ShowInPopup)
			{
				aWBNumberColumn.DataNavigateUrlFormatString += String.Format("&{0}={1}", TrackingConstants.QueryStringKeys.PopupKey, "Y"); // Partial URL
				aWBNumberColumn.Target = (NoResString)"_blank"; // JavaScript Code
				aWBNumberColumn.WindowStyle = Global.HAWBDetailsPopupWindowStyle;
			}

			AddToDictionaryAsRequired(aWBNumberColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_AWBOriginCode);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("187c91d7-a7d7-47d0-9a34-5928e073d629", "Origin"), ExportAWBHeaderSchema.Constants.EH_AWBOriginCode) { ColumnKey = WebTracker.Grids.HAWB.Origin });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_AirportOfDestinationCode);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("f2ba23b8-f974-465c-8eb3-2193fa83b5be", "Destination"), ExportAWBHeaderSchema.Constants.EH_AirportOfDestinationCode) { ColumnKey = WebTracker.Grids.HAWB.Destination });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).NumberOfPieces);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("d14281dc-4afd-4a97-87cf-67753695be03", "Pieces"), TrackingMAWBHeader.Schema.NumberOfPieces) { ColumnKey = WebTracker.Grids.HAWB.Pieces });

			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingMAWBHeader)null).ActualWeight);
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("e0259127-27dc-4459-baec-19aa1e203054", "Act. Weight"), TrackingMAWBHeader.Schema.ActualWeight) { ColumnKey = WebTracker.Grids.HAWB.ActWeight });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).ActualWeightUnit);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("b3c07262-1780-4569-80e2-d473f2948789", "Weight UQ"), TrackingMAWBHeader.Schema.ActualWeightUnit) { ColumnKey = WebTracker.Grids.HAWB.WeightUQ });

			ZBindToChecker.CheckBindTo((ZInt)((TrackingMAWBHeader)null).EH_ShippingLoadAndCount);
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("0f0444e7-f967-4aad-b9cd-a8465b64f02f", "SLAC"), ExportAWBHeaderSchema.Constants.EH_ShippingLoadAndCount) { ColumnKey = WebTracker.Grids.HAWB.SLAC });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ShipperName);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("73ac7c83-5cbc-4fcc-87a0-a9b8dddb65a9", "Shipper Name"), ExportAWBHeaderSchema.Constants.EH_ShipperName) { ColumnKey = WebTracker.Grids.HAWB.ShipperName });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ShipperAddress);
			AddToDictionary(new ZTextEditColumn(Res.GetString("bc77e47c-a83f-44c0-930c-435650da740e", "Shipper Address"), ExportAWBHeaderSchema.Constants.EH_ShipperAddress) { ColumnKey = WebTracker.Grids.HAWB.ShipperAddress });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ShipperAddress2);
			AddToDictionary(new ZTextEditColumn(Res.GetString("14b9ce8d-c40e-4e4c-a64b-0e08591eb452", "Shipper Address 2"), ExportAWBHeaderSchema.Constants.EH_ShipperAddress2) { ColumnKey = WebTracker.Grids.HAWB.ShipperAddress2 });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ShipperPlace);
			AddToDictionary(new ZTextEditColumn(Res.GetString("4517a377-a4a4-42df-98af-6e3717ae222d", "Shipper Place"), ExportAWBHeaderSchema.Constants.EH_ShipperPlace) { ColumnKey = WebTracker.Grids.HAWB.ShipperPlace });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ShipperState);
			AddToDictionary(new ZTextEditColumn(Res.GetString("d424cd31-d82d-4da7-8a96-77041f2187b5", "Shipper State"), ExportAWBHeaderSchema.Constants.EH_ShipperState) { ColumnKey = WebTracker.Grids.HAWB.ShipperState });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ShipperPostCode);
			AddToDictionary(new ZTextEditColumn(Res.GetString("f6f75ecd-6393-4ee4-945f-8c56c7f308a1", "Shipper Postal Code"), ExportAWBHeaderSchema.Constants.EH_ShipperPostCode) { ColumnKey = WebTracker.Grids.HAWB.ShipperPostCode });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ShipperCountryCode);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("b66cdb64-d48f-4411-8396-d5772fc2877a", "Shipper Country/Region"), ExportAWBHeaderSchema.Constants.EH_ShipperCountryCode) { ColumnKey = WebTracker.Grids.HAWB.ShipperCountryCode });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ShipperContactDetail);
			AddToDictionary(new ZTextEditColumn(Res.GetString("a59f4aa2-099c-4fb9-9098-8b6dc5debda4", "Shipper Phone"), ExportAWBHeaderSchema.Constants.EH_ShipperContactDetail) { ColumnKey = WebTracker.Grids.HAWB.ShipperContactDetail });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ConsigneeName);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("8c554368-016e-4373-b273-a2b664b25583", "Consignee Name"), ExportAWBHeaderSchema.Constants.EH_ConsigneeName) { ColumnKey = WebTracker.Grids.HAWB.ConsigneeName });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ConsigneeAddress);
			AddToDictionary(new ZTextEditColumn(Res.GetString("979f3e80-279d-4c74-8b6c-fadc4f30b5aa", "Consignee Address"), ExportAWBHeaderSchema.Constants.EH_ConsigneeAddress) { ColumnKey = WebTracker.Grids.HAWB.ConsigneeAddress });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ConsigneeAddress2);
			AddToDictionary(new ZTextEditColumn(Res.GetString("45898a19-5c76-4ce6-bb6c-637165a215a3", "Consignee Address 2"), ExportAWBHeaderSchema.Constants.EH_ConsigneeAddress2) { ColumnKey = WebTracker.Grids.HAWB.ConsigneeAddress2 });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ConsigneePlace);
			AddToDictionary(new ZTextEditColumn(Res.GetString("45ba348b-8d92-4ede-8997-082e681909af", "Consignee Place"), ExportAWBHeaderSchema.Constants.EH_ConsigneePlace) { ColumnKey = WebTracker.Grids.HAWB.ConsigneePlace });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ConsigneeState);
			AddToDictionary(new ZTextEditColumn(Res.GetString("e69df319-4a54-4e4d-89fc-d192dfcebcab", "Consignee State"), ExportAWBHeaderSchema.Constants.EH_ConsigneeState) { ColumnKey = WebTracker.Grids.HAWB.ConsigneeState });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ConsigneePostCode);
			AddToDictionary(new ZTextEditColumn(Res.GetString("efb3e4ff-dab8-4f48-b218-9f989c9f909a", "Consignee Postal Code"), ExportAWBHeaderSchema.Constants.EH_ConsigneePostCode) { ColumnKey = WebTracker.Grids.HAWB.ConsigneePostCode });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ConsigneeCountryCode);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("525305fc-22ac-4f76-a19b-233559ee2a11", "Consignee Country/Region"), ExportAWBHeaderSchema.Constants.EH_ConsigneeCountryCode) { ColumnKey = WebTracker.Grids.HAWB.ConsigneeCountryCode });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).EH_ConsigneeContactDetail);
			AddToDictionary(new ZTextEditColumn(Res.GetString("4078e234-88ce-4032-9cb5-62e314db84d9", "Consignee Phone"), ExportAWBHeaderSchema.Constants.EH_ConsigneeContactDetail) { ColumnKey = WebTracker.Grids.HAWB.ConsigneeContactDetail });

			ZBindToChecker.CheckBindTo((ZString)((TrackingMAWBHeader)null).AWBMessagingStatusDescription);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("04bd0543-290d-4fe0-bbfb-0f686b559841", "Status"), TrackingMAWBHeader.Schema.AWBMessagingStatusDescription) { ColumnKey = WebTracker.Grids.HAWB.AWBStatusDescription });
		}
	}
}
