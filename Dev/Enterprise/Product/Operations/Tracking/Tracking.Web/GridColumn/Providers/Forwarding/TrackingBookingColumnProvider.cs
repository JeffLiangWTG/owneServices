using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Web;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class TrackingBookingColumnProvider : GridColumnProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			ZHyperLinkColumn orderNumberColumn = new ZHyperLinkColumn(Res.GetString("142f4fbf-4a93-4c85-83f8-ea6a9403929d", "Booking#"), "TrackingBooking+" + TrackingBooking.Schema.UniqueConsignRef) { ColumnKey = WebTracker.Grids.TrackingBookings.BookingNumber };
			orderNumberColumn.DataNavigateUrlFormatString = UrlFormatWithAppRoot(TrackingConstants.RelativePath.BookingDetailsPage) + (NoResString)"?Ref={0}"; // Partial URL
			orderNumberColumn.DataNavigateUrlFields = new string[] { "TrackingBooking+" + TrackingBooking.Schema.BookingPK };
			AddToDictionaryAsRequired(orderNumberColumn);

			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("0c95abcb-498a-4129-92da-3ec198e9ab77", "Description"), "TrackingBooking+" + TrackingBooking.Schema.GoodsDescription) { ColumnKey = WebTracker.Grids.TrackingBookings.Description });

			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("794a16f9-9947-45a1-8390-08d79e2de682", "Shipper's Ref#"), "TrackingBooking+" + TrackingBooking.Schema.BookingReference) { ColumnKey = WebTracker.Grids.TrackingBookings.ShipperReference });

			ZBindToChecker.CheckBindTo((ZString)((ViewTrackingBooking)null).TrackingBooking.OriginUNLOCO.Description);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("ed4ef525-5334-439f-af22-3a1082a4ab03", "Origin"), "TrackingBooking+OriginUNLOCO+Description") { ColumnKey = WebTracker.Grids.TrackingBookings.Origin });

			ZBindToChecker.CheckBindTo((ZString)((ViewTrackingBooking)null).TrackingBooking.DestinationUNLOCO.Description);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("dc2c6cbf-db77-4bab-81f6-cdd63200df9c", "Destination"), "TrackingBooking+DestinationUNLOCO+Description") { ColumnKey = WebTracker.Grids.TrackingBookings.Destination });

			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("f32a7d9d-e0d0-4a52-9710-c5d2d6def861", "Packs"), "TrackingBooking+" + TrackingBooking.Schema.OuterPacks) { ColumnKey = WebTracker.Grids.TrackingBookings.Packs });

			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("96635033-323d-4670-9e6e-1d91a9575f97", "Weight"), "TrackingBooking+" + TrackingBooking.Schema.WeightWithUnits) { ColumnKey = WebTracker.Grids.TrackingBookings.Weight });

			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("c8e784a9-d062-463d-a713-9b8c8d97fa33", "Volume"), "TrackingBooking+" + TrackingBooking.Schema.VolumeWithUnits) { ColumnKey = WebTracker.Grids.TrackingBookings.Volume });

			ZBindToChecker.CheckBindTo((ZDecimal)((ViewTrackingBooking)null).TrackingBooking.GoodsValue);
			ZBindToChecker.CheckBindTo((ZString)((ViewTrackingBooking)null).TrackingBooking.GoodsValueCurr);

			AddGroupColumn(
				Res.GetString("0a03b111-d972-46e8-b0a5-1a45f30685cf", "Goods Value"),
				WebTracker.Grids.TrackingBookings.GoodsValue,
				true,
				new ZCalcEditColumn(Res.GetString("0a03b111-d972-46e8-b0a5-1a45f30685cf", "Goods Value"), "TrackingBooking+" + TrackingBooking.Schema.GoodsValue),
				new ZTextEditColumn(Res.GetString("2585f8f9-3d5f-4ab7-bd41-993e11bf1043", "Currency"), "TrackingBooking+" + TrackingBooking.Schema.GoodsValueCurr)
				);

			AddToDictionary(new ZTextEditColumn(Res.GetString("2d4891ee-28b9-49a8-baab-d620c4ef49b4", "Canceled?"), "TrackingBooking+" + TrackingBooking.Schema.IsCancelled) { ColumnKey = WebTracker.Grids.TrackingBookings.Canceled });
			AddToDictionary(new ZDateTimeColumn(Res.GetString("0ce60246-51be-4a53-a554-638d0e3e971b", "Estimated Pickup"), "TrackingBooking+" + TrackingBooking.Schema.EstimatedPickup, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingBookings.EstimatedPickup });
			AddToDictionary(new ZDateTimeColumn(Res.GetString("2a788cab-6f8b-4ef8-9a79-cf34e7ec21c2", "Pickup Required By"), "TrackingBooking+" + TrackingBooking.Schema.PickupRequiredBy, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingBookings.PickupRequiredBy });
			AddToDictionary(new ZDateTimeColumn(Res.GetString("cce3d753-862c-4136-b24c-bae42f96f31e", "Estimated Delivery"), "TrackingBooking+" + TrackingBooking.Schema.EstimatedDelivery, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingBookings.EstimatedDelivery });
			AddToDictionary(new ZDateTimeColumn(Res.GetString("02bdd981-9dc5-4d12-be53-806ea4c29bc5", "Delivery Required By"), "TrackingBooking+" + TrackingBooking.Schema.DeliveryRequiredBy, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingBookings.DeliveryRequiredBy });
			AddToDictionary(new ZDateTimeColumn(Res.GetString("5a5c5452-3d3c-44c3-ac0a-5c13c8377d51", "Delivery Date"), "TrackingBooking+" + TrackingBooking.Schema.DeliveryCartageCompleted, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingBookings.DeliveryDate });

			ZBindToChecker.CheckBindTo((ZString)((ViewTrackingBooking)null).TrackingBooking.ServiceLevel);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((ViewTrackingBooking)null).TrackingBooking.ServiceLevels);
			AddToDictionary(new ZCodeFindBoxColumn(Res.GetString("acfef403-6662-4fd7-81c8-3e8b7a3c12a6", "Service Level"), "TrackingBooking+" + TrackingBooking.Schema.ServiceLevel, "TrackingBooking.ServiceLevels", typeof(ViewTrackingBooking))
			{
				ColumnKey = WebTracker.Grids.TrackingBookings.ServiceLevel,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});

			AddToDictionary(new ZTextEditColumn(Res.GetString("989CCE86-1F64-43C7-9DC7-2814F012CFF6", "Order Ref#"), "TrackingBooking+" + TrackingBooking.Schema.OrderItemsAsString) { ColumnKey = WebTracker.Grids.TrackingBookings.OrderReferences });

			if (WebDataRegistry.Instance.MilestoneVisibility.Value != MilestoneVisibilityList.Codes.None)
			{
				ZBindToChecker.CheckBindTo((TrackingMilestoneCollection)((ViewTrackingBooking)null).TrackingBooking.Milestones);
				foreach (ZTemplateColumn column in ConfigurationHelper.GetMilestonesColumns("TrackingBooking+Milestones")) // Data column name
				{
					if (column.HeaderText == Res.GetString("f3ff4281-e665-4549-8321-e80e987ec09e", "Last Milestone Desc."))
					{
						AddToDictionaryAsDefault(column);
					}
					else
					{
						AddToDictionary(column);
					}
				}
			}
			ZBindToChecker.CheckBindTo((ZString)((ViewTrackingBooking)null).TrackingBooking.Vessel);
			AddToDictionary(new ZTextEditColumn(Res.GetString("4af241a3-497d-45db-bda1-8dd61c70e548", "Vessel"), "TrackingBooking+" + TrackingBooking.Schema.Vessel) { ColumnKey = WebTracker.Grids.TrackingBookings.Vessel });

			ZBindToChecker.CheckBindTo((ZString)((ViewTrackingBooking)null).TrackingBooking.VoyageFlightWithSuppression);
			AddToDictionary(new ZTextEditColumn(Res.GetString("83250f21-c4a6-43d2-aef8-14a7bc323ffb", "Voyage/Flight"), "TrackingBooking+" + TrackingBooking.Schema.VoyageFlightWithSuppression) { ColumnKey = WebTracker.Grids.TrackingBookings.Voyage });
			AddToDictionary(new ZTextEditColumn(Res.GetString("04dd750d-a5aa-45c8-b756-03d5ed8e57f5", "MAWB"), "TrackingBooking+" + TrackingBooking.Schema.MAWBNumber) { ColumnKey = WebTracker.Grids.TrackingBookings.MAWB });
			AddToDictionary(new ZTextEditColumn(Res.GetString("351f52c2-90fa-49fd-ae7e-b21b32ee29de", "Consignee"), "TrackingBooking+" + TrackingBooking.Schema.ConsigneeFullName) { ColumnKey = WebTracker.Grids.TrackingBookings.Consignee });
			AddToDictionary(new ZTextEditColumn(FreightDataRegistry.Instance.ConsignorShipperTerminology.Value, "TrackingBooking+" + TrackingBooking.Schema.ConsignorFullName) { ColumnKey = WebTracker.Grids.TrackingBookings.Consignor });
			AddToDictionary(new ZDateTimeColumn(Res.GetString("34abd81f-61dd-4ae4-a3fa-2bd1938d0ce2", "CFS Cut Off"), "TrackingBooking+" + TrackingBooking.Schema.DepotCutOff) { ColumnKey = WebTracker.Grids.TrackingBookings.DepotCutOff });
			AddToDictionary(new ZTextEditColumn(Res.GetString("975b9259-b510-4ac8-95eb-3d46fa8c2a19", "CFS Ref#"), "TrackingBooking+" + TrackingBooking.Schema.CFSReference) { ColumnKey = WebTracker.Grids.TrackingBookings.CFSReference });

			ZBindToChecker.CheckBindTo((ZString)((ViewTrackingBooking)null).TrackingBooking.AdditionalTerms);
			AddToDictionary(new ZTextEditColumn(Res.GetString("feb47a2d-38e9-4c72-a49e-bdb46fc74f12", "Additional Terms"), "TrackingBooking+" + TrackingBooking.Schema.AdditionalTerms) { ColumnKey = WebTracker.Grids.TrackingBookings.AdditionalTerms });
			ZBindToChecker.CheckBindTo((ZString)((ViewTrackingBooking)null).TrackingBooking.INCO);
			AddToDictionary(new ZDropEditColumn(Res.GetString("3124af76-1a5e-4612-98b2-840348a03fea", "Payment Term"), "TrackingBooking+" + TrackingBooking.Schema.INCO) { DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly, ColumnKey = WebTracker.Grids.TrackingBookings.INCO });

			ZBindToChecker.CheckBindTo((ZString)((ViewTrackingBooking)null).TrackingBooking.ChargesApply);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((ViewTrackingBooking)null).TrackingBooking.ChargesApply_List);
			AddToDictionary(new ZDropEditColumn(Res.GetString("4429a8d0-c621-4195-9f87-512c549d311d", "Charges Apply"), "TrackingBooking+" + TrackingBooking.Schema.ChargesApply)
			{
				ColumnKey = WebTracker.Grids.TrackingBookings.ChargesApply,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly,
				BindToList = "TrackingBooking.ChargesApply_List"
			});

			ZBindToChecker.CheckBindTo((ZString)((ViewTrackingBooking)null).TrackingBooking.ReleaseType);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((ViewTrackingBooking)null).TrackingBooking.ReleaseType_List);
			AddToDictionary(new ZDropEditColumn(Res.GetString("575d65f6-f305-4709-bbce-5c12019c1213", "Release Type"), "TrackingBooking+" + TrackingBooking.Schema.ReleaseType)
			{
				ColumnKey = WebTracker.Grids.TrackingBookings.ReleaseType,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly,
				BindToList = "TrackingBooking.ReleaseType_List"
			});

			ZBindToChecker.CheckBindTo((ZString)((ViewTrackingBooking)null).TrackingBooking.OnBoard);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((ViewTrackingBooking)null).TrackingBooking.OnBoard_List);
			AddToDictionary(new ZDropEditColumn(Res.GetString("c5503c1a-744d-4140-971f-6896e6b2cf9b", "On Board"), "TrackingBooking+" + TrackingBooking.Schema.OnBoard)
			{
				ColumnKey = WebTracker.Grids.TrackingBookings.OnBoard,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly,
				BindToList = "TrackingBooking.OnBoard_List"
			});

			ZBindToChecker.CheckBindTo((ZString)((ViewTrackingBooking)null).TrackingBooking.PickupAgentFullName);
			AddToDictionary(new ZTextEditColumn(Res.GetString("c1ff9d02-327c-4e0d-9efc-c02a7509bbaa", "Pickup Agent"), "TrackingBooking+" + TrackingBooking.Schema.PickupAgentFullName) { ColumnKey = WebTracker.Grids.TrackingBookings.PickupAgent });

			ZBindToChecker.CheckBindTo((ZString)((ViewTrackingBooking)null).TrackingBooking.DeliveryAgentFullName);
			AddToDictionary(new ZTextEditColumn(Res.GetString("db8bd4ec-12f2-464f-95f6-78dc611f9298", "Delivery Agent"), "TrackingBooking+" + TrackingBooking.Schema.DeliveryAgentFullName) { ColumnKey = WebTracker.Grids.TrackingBookings.DeliveryAgent });
		}

		protected override List<int> GetOldColumnsOrder()
		{
			List<int> result = new List<int>();
			result.Add((int)WebTracker.Grids.TrackingBookings.BookingNumber);
			result.Add((int)WebTracker.Grids.TrackingBookings.Description);
			result.Add((int)WebTracker.Grids.TrackingBookings.ShipperReference);
			result.Add((int)WebTracker.Grids.TrackingBookings.Origin);
			result.Add((int)WebTracker.Grids.TrackingBookings.Destination);
			result.Add((int)WebTracker.Grids.TrackingBookings.Packs);
			result.Add((int)WebTracker.Grids.TrackingBookings.Weight);
			result.Add((int)WebTracker.Grids.TrackingBookings.WeightUnit);
			result.Add((int)WebTracker.Grids.TrackingBookings.Volume);
			result.Add((int)WebTracker.Grids.TrackingBookings.VolumeUnit);
			result.Add((int)WebTracker.Grids.TrackingBookings.GoodsValue);
			result.Add((int)WebTracker.Grids.TrackingBookings.Currency);
			result.Add((int)WebTracker.Grids.TrackingBookings.Canceled);
			result.Add((int)WebTracker.Grids.TrackingBookings.EstimatedPickup);
			result.Add((int)WebTracker.Grids.TrackingBookings.PickupRequiredBy);
			result.Add((int)WebTracker.Grids.TrackingBookings.EstimatedDelivery);
			result.Add((int)WebTracker.Grids.TrackingBookings.DeliveryRequiredBy);
			result.Add((int)WebTracker.Grids.TrackingBookings.DeliveryDate);
			result.Add((int)WebTracker.Grids.TrackingBookings.ServiceLevel);
			result.Add((int)WebTracker.Grids.TrackingBookings.OrderReferences);

			foreach (ZTemplateColumn column in ConfigurationHelper.GetMilestonesColumns("TrackingBooking+Milestones")) // Data column name
			{
				result.Add(((IUniqueKeyColumn)column).UniqueKey);
			}

			result.Add((int)WebTracker.Grids.TrackingBookings.Vessel);
			result.Add((int)WebTracker.Grids.TrackingBookings.Voyage);
			result.Add((int)WebTracker.Grids.TrackingBookings.MAWB);
			result.Add((int)WebTracker.Grids.TrackingBookings.Consignee);
			result.Add((int)WebTracker.Grids.TrackingBookings.Consignor);
			result.Add((int)WebTracker.Grids.TrackingBookings.DepotCutOff);
			result.Add((int)WebTracker.Grids.TrackingBookings.CFSReference);
			return result;
		}
	}
}
