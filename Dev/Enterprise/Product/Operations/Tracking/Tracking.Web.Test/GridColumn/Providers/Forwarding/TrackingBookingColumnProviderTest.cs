using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Web;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(TrackingBookingColumnProvider))]
	sealed class TrackingBookingColumnProviderTest : GridColumnProviderTest
	{
		public override void TestFixOldLayout()
		{
			string cachedRegistryValue = WebDataRegistry.Instance.MilestoneVisibility.Value;
			base.TestFixOldLayout();
			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedRegistryValue);
		}

		protected override void BeforeLayoutsWithFewDynamicColumns()
		{
			base.BeforeLayoutsWithFewDynamicColumns();
			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneVisibilityList.Codes.LastCompletedMilestoneOnly);
		}

		protected override void BeforeLayoutsWithAllDynamicColumns()
		{
			base.BeforeLayoutsWithAllDynamicColumns();
			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneVisibilityList.Codes.All);
		}

		protected override DataGridColumn[] GetColumnsForLayoutFixNoDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.TrackingBookings.Consignee],
				TestProvider[WebTracker.Grids.TrackingBookings.BookingNumber],
				TestProvider[WebTracker.Grids.TrackingBookings.OrderReferences],
				TestProvider[WebTracker.Grids.TrackingBookings.CFSReference]
			};
		}

		protected override DataGridColumn[] GetColumnsForLayoutFixAllDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.TrackingBookings.Consignee],
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDate],
				TestProvider[WebTracker.Grids.TrackingBookings.BookingNumber],
				TestProvider[WebTracker.Grids.Milestones.NextMilestoneDate],
				TestProvider[WebTracker.Grids.TrackingBookings.OrderReferences],
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDescription],
				TestProvider[WebTracker.Grids.TrackingBookings.CFSReference],
				TestProvider[WebTracker.Grids.Milestones.NextMilestoneDescription]
			};
		}

		protected override DataGridColumn[] GetColumnsForLayoutFixFewDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.TrackingBookings.Consignee],
				TestProvider[WebTracker.Grids.TrackingBookings.BookingNumber],
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDate],
				TestProvider[WebTracker.Grids.TrackingBookings.OrderReferences],
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDescription],
				TestProvider[WebTracker.Grids.TrackingBookings.CFSReference]
			};
		}

		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddRequiredColumn(new ZHyperLinkColumn("Booking#", "TrackingBooking+" + TrackingBooking.Schema.UniqueConsignRef)
			{
				ColumnKey = WebTracker.Grids.TrackingBookings.BookingNumber,
				DataNavigateUrlFormatString = TrackingConstants.RelativePath.BookingDetailsPage + "?Ref={0}", // Partial URL
				DataNavigateUrlFields = new string[] { "TrackingBooking+" + TrackingBooking.Schema.BookingPK }
			});
			AddDefaultsColumn(new ZTextEditColumn("Description", "TrackingBooking+" + TrackingBooking.Schema.GoodsDescription) { ColumnKey = WebTracker.Grids.TrackingBookings.Description });
			AddDefaultsColumn(new ZTextEditColumn("Shipper's Ref#", "TrackingBooking+" + TrackingBooking.Schema.BookingReference) { ColumnKey = WebTracker.Grids.TrackingBookings.ShipperReference });
			AddDefaultsColumn(new ZTextEditColumn("Origin", "TrackingBooking+OriginUNLOCO+Description") { ColumnKey = WebTracker.Grids.TrackingBookings.Origin });
			AddDefaultsColumn(new ZTextEditColumn("Destination", "TrackingBooking+DestinationUNLOCO+Description") { ColumnKey = WebTracker.Grids.TrackingBookings.Destination });
			AddDefaultsColumn(new ZCalcEditColumn("Packs", "TrackingBooking+" + TrackingBooking.Schema.OuterPacks) { ColumnKey = WebTracker.Grids.TrackingBookings.Packs });

			AddDefaultsColumn(new ZTextEditColumn("Weight", "TrackingBooking+" + TrackingBooking.Schema.WeightWithUnits) { ColumnKey = WebTracker.Grids.TrackingBookings.Weight });
			AddDefaultsColumn(new ZTextEditColumn("Volume", "TrackingBooking+" + TrackingBooking.Schema.VolumeWithUnits) { ColumnKey = WebTracker.Grids.TrackingBookings.Volume });

			AddDefaultsColumn(new ZGroupColumn("Goods Value", new DataGridColumn[]
			{
		new ZCalcEditColumn("Goods Value", "TrackingBooking+" + TrackingBooking.Schema.GoodsValue),
		new ZTextEditColumn("Currency", "TrackingBooking+" + TrackingBooking.Schema.GoodsValueCurr)
			})
			{ ColumnKey = WebTracker.Grids.TrackingBookings.GoodsValue });

			AddColumn(new ZTextEditColumn("Canceled?", "TrackingBooking+" + TrackingBooking.Schema.IsCancelled) { ColumnKey = WebTracker.Grids.TrackingBookings.Canceled });
			AddColumn(new ZDateTimeColumn("Estimated Pickup", "TrackingBooking+" + TrackingBooking.Schema.EstimatedPickup, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingBookings.EstimatedPickup });
			AddColumn(new ZDateTimeColumn("Pickup Required By", "TrackingBooking+" + TrackingBooking.Schema.PickupRequiredBy, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingBookings.PickupRequiredBy });
			AddColumn(new ZDateTimeColumn("Estimated Delivery", "TrackingBooking+" + TrackingBooking.Schema.EstimatedDelivery, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingBookings.EstimatedDelivery });
			AddColumn(new ZDateTimeColumn("Delivery Required By", "TrackingBooking+" + TrackingBooking.Schema.DeliveryRequiredBy, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingBookings.DeliveryRequiredBy });
			AddColumn(new ZDateTimeColumn("Delivery Date", "TrackingBooking+" + TrackingBooking.Schema.DeliveryCartageCompleted, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingBookings.DeliveryDate });

			AddColumn(new ZCodeFindBoxColumn("Service Level", "TrackingBooking+" + TrackingBooking.Schema.ServiceLevel, "TrackingBooking.ServiceLevels", typeof(ViewTrackingBooking))
			{
				ColumnKey = WebTracker.Grids.TrackingBookings.ServiceLevel,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});
			AddColumn(new ZTextEditColumn("Order Ref#", "TrackingBooking+" + TrackingBooking.Schema.OrderItemsAsString) { ColumnKey = WebTracker.Grids.TrackingBookings.OrderReferences });

			if (WebDataRegistry.Instance.MilestoneVisibility.Value != MilestoneVisibilityList.Codes.None)
			{
				string bindToLast = "TrackingBooking+Milestones+" + TrackingMilestoneCollection.Schema.LastMilestone + '+';

				AddDefaultsColumn(new ZTextEditColumn(ConfigurationHelper.ColumnHeaders.LastMilestoneDesc, bindToLast + TrackingMilestone.Schema.Description) { ColumnKey = WebTracker.Grids.Milestones.LastMilestoneDescription });
				if (WebDataRegistry.Instance.MilestoneDatesVisibility.Value != MilestoneDatesVisibilityList.Codes.None)
				{
					AddColumn(new ZDateTimeColumn(ConfigurationHelper.ColumnHeaders.LastMilestoneDate, bindToLast + TrackingMilestone.Schema.DisplayDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.Milestones.LastMilestoneDate });
				}
				if (WebDataRegistry.Instance.MilestoneVisibility.Value == MilestoneVisibilityList.Codes.All)
				{
					string bindToNext = "TrackingBooking+Milestones+" + TrackingMilestoneCollection.Schema.NextMilestone + '+';

					AddColumn(new ZTextEditColumn(ConfigurationHelper.ColumnHeaders.NextMilestoneDesc, bindToNext + TrackingMilestone.Schema.Description) { ColumnKey = WebTracker.Grids.Milestones.NextMilestoneDescription });
					if (WebDataRegistry.Instance.MilestoneDatesVisibility.Value != MilestoneDatesVisibilityList.Codes.None)
					{
						AddColumn(new ZDateTimeColumn(ConfigurationHelper.ColumnHeaders.NextMilestoneDate, bindToNext + TrackingMilestone.Schema.EstimatedDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.Milestones.NextMilestoneDate });
					}
				}
			}
			AddColumn(new ZTextEditColumn("Vessel", "TrackingBooking+" + TrackingBooking.Schema.Vessel) { ColumnKey = WebTracker.Grids.TrackingBookings.Vessel });
			AddColumn(new ZTextEditColumn("Voyage/Flight", "TrackingBooking+" + TrackingBooking.Schema.VoyageFlightWithSuppression) { ColumnKey = WebTracker.Grids.TrackingBookings.Voyage });
			AddColumn(new ZTextEditColumn("MAWB", "TrackingBooking+" + TrackingBooking.Schema.MAWBNumber) { ColumnKey = WebTracker.Grids.TrackingBookings.MAWB });
			AddColumn(new ZTextEditColumn("Consignee", "TrackingBooking+" + TrackingBooking.Schema.ConsigneeFullName) { ColumnKey = WebTracker.Grids.TrackingBookings.Consignee });
			AddColumn(new ZTextEditColumn(FreightDataRegistry.Instance.ConsignorShipperTerminology.Value, "TrackingBooking+" + TrackingBooking.Schema.ConsignorFullName) { ColumnKey = WebTracker.Grids.TrackingBookings.Consignor });
			AddColumn(new ZDateTimeColumn("CFS Cut Off", "TrackingBooking+" + TrackingBooking.Schema.DepotCutOff) { ColumnKey = WebTracker.Grids.TrackingBookings.DepotCutOff });
			AddColumn(new ZTextEditColumn("CFS Ref#", "TrackingBooking+" + TrackingBooking.Schema.CFSReference) { ColumnKey = WebTracker.Grids.TrackingBookings.CFSReference });
			AddColumn(new ZTextEditColumn("Additional Terms", "TrackingBooking+" + TrackingBooking.Schema.AdditionalTerms) { ColumnKey = WebTracker.Grids.TrackingBookings.AdditionalTerms });
			AddColumn(new ZDropEditColumn("Payment Term", "TrackingBooking+" + TrackingBooking.Schema.INCO) { DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly, ColumnKey = WebTracker.Grids.TrackingBookings.INCO });

			AddColumn(new ZDropEditColumn("Charges Apply", "TrackingBooking+" + TrackingBooking.Schema.ChargesApply)
			{
				ColumnKey = WebTracker.Grids.TrackingBookings.ChargesApply,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly,
				BindToList = "TrackingBooking.ChargesApply_List"
			});

			AddColumn(new ZDropEditColumn("Release Type", "TrackingBooking+" + TrackingBooking.Schema.ReleaseType)
			{
				ColumnKey = WebTracker.Grids.TrackingBookings.ReleaseType,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly,
				BindToList = "TrackingBooking.ReleaseType_List"
			});

			AddColumn(new ZDropEditColumn("On Board", "TrackingBooking+" + TrackingBooking.Schema.OnBoard)
			{
				ColumnKey = WebTracker.Grids.TrackingBookings.OnBoard,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly,
				BindToList = "TrackingBooking.OnBoard_List"
			});

			AddColumn(new ZTextEditColumn("Pickup Agent", "TrackingBooking+" + TrackingBooking.Schema.PickupAgentFullName) { ColumnKey = WebTracker.Grids.TrackingBookings.PickupAgent });
			AddColumn(new ZTextEditColumn("Delivery Agent", "TrackingBooking+" + TrackingBooking.Schema.DeliveryAgentFullName) { ColumnKey = WebTracker.Grids.TrackingBookings.DeliveryAgent });
		}

		protected override List<object> GetUnsortableColumnKeys() => new List<object>
		{
			WebTracker.Grids.TrackingBookings.GoodsValue,
			WebTracker.Grids.TrackingBookings.ServiceLevel
		};

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new TrackingBookingColumnProvider();
		}
	}
}
