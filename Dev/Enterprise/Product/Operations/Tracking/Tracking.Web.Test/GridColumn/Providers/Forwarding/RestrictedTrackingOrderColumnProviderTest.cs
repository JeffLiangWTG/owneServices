using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(TrackingOrderColumnProvider))]
	[HttpContextEnabledTest]
	sealed class RestrictedTrackingOrderColumnProviderTest : TrackingOrderColumnProviderTest
	{
		protected override void SetUp()
		{
			LoginAsQuickShipmentUser();
			SetupRestrictedColumns();
			base.SetUp();
		}

		void SetupRestrictedColumns()
		{
			ExpectedRestrictedColumns.AddRange(new[]
			{
				(int)WebTracker.Grids.TrackingOrders.SplitNumber,
				(int)WebTracker.Grids.TrackingOrders.Supplier,
				(int)WebTracker.Grids.TrackingOrders.Buyer,
				(int)WebTracker.Grids.TrackingOrders.ControllingCustomer,
				(int)WebTracker.Grids.TrackingOrders.Origin,
				(int)WebTracker.Grids.TrackingOrders.Destination,
				(int)WebTracker.Grids.TrackingOrders.Packs,
				(int)WebTracker.Grids.TrackingOrders.Volume,
				(int)WebTracker.Grids.TrackingOrders.Weight
			});
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			AddDefaultsColumn(new ZTextEditColumn("Order #", TrackingOrder.Schema.JD_OrderNumberAndSplit) { ColumnKey = WebTracker.Grids.TrackingOrders.OrderNumber });

			AddDefaultsColumn(new ZTextEditColumn("Transport Mode", TrackingOrder.Schema.JD_TransportMode) { ColumnKey = WebTracker.Grids.TrackingOrders.TransportMode });
			AddDefaultsColumn(new ZTextEditColumn("Status", TrackingOrder.Schema.JD_OrderStatusDesc) { ColumnKey = WebTracker.Grids.TrackingOrders.Status });

			AddDefaultsColumn(new ZDateTimeColumn("Order Date", TrackingOrder.Schema.JD_OrderDate)
			{
				ColumnKey = WebTracker.Grids.TrackingOrders.OrderDate,
				DateTimeFormat = ZDateTimePickerFormat.Short
			});

			AddColumn(new ZTextEditColumn("Current Vessel", TrackingOrder.Schema.CurrentVessel) { ColumnKey = WebTracker.Grids.TrackingOrders.CurrentVessel });
			AddColumn(new ZTextEditColumn("Current Voyage/Flight", TrackingOrder.Schema.CurrentVoyageWithSuppression) { ColumnKey = WebTracker.Grids.TrackingOrders.CurrentVoyage });

			foreach (ZTemplateColumn column in ConfigurationHelper.GetMilestonesColumns("Milestones")) // Data column name
			{
				AddColumn(column);
			}

			AddColumn(new ZDateTimeColumn("Req. Ex Works", TrackingOrder.Schema.JD_ExWorksRequiredBy)
			{
				ColumnKey = WebTracker.Grids.TrackingOrders.RequiredExWorks,
				DateTimeFormat = ZDateTimePickerFormat.Short
			});

			AddColumn(new ZDateTimeColumn("Req. In Store", TrackingOrder.Schema.JD_DeliveryRequiredBy)
			{
				ColumnKey = WebTracker.Grids.TrackingOrders.RequiredInStore,
				DateTimeFormat = ZDateTimePickerFormat.Short
			});

			AddColumn(new ZTimelineColumn("Ex-Factory", TrackingOrder.Schema.JD_Milestone_A_EXW, TrackingOrder.Schema.JD_Milestone_E_EXW, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingOrders.ExFactory });
			AddColumn(new ZTimelineColumn("Origin Receival", TrackingOrder.Schema.JD_Milestone_A_GIW, TrackingOrder.Schema.JD_Milestone_E_GIW, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingOrders.OriginReceival });
			AddColumn(new ZTimelineColumn("Departure", TrackingOrder.Schema.ATDWithSuppression, TrackingOrder.Schema.ETDWithSuppression, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingOrders.Departure });
			AddColumn(new ZTimelineColumn("Arrival", TrackingOrder.Schema.ATAWithSuppression, TrackingOrder.Schema.ETAWithSuppression, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingOrders.Arrival });
			AddColumn(new ZTimelineColumn("Clearance Commenced", TrackingOrder.Schema.JD_Milestone_A_CCC, TrackingOrder.Schema.JD_Milestone_E_CCC, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingOrders.ClearanceCommenced });
			AddColumn(new ZTimelineColumn("Clearance Finalized", TrackingOrder.Schema.JD_Milestone_A_CLR, TrackingOrder.Schema.JD_Milestone_E_CLR, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingOrders.ClearanceFinalized });
			AddColumn(new ZTimelineColumn("Unpacked", TrackingOrder.Schema.JD_Milestone_A_CAV, TrackingOrder.Schema.JD_Milestone_E_CAV, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingOrders.Unpacked });
			AddColumn(new ZTimelineColumn("Port Transport Advised", TrackingOrder.Schema.JD_Milestone_A_DCA, TrackingOrder.Schema.JD_Milestone_E_DCA, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingOrders.LocalTransportAdvised });
			AddColumn(new ZTimelineColumn("Delivered", TrackingOrder.Schema.JD_Milestone_A_DCF, TrackingOrder.Schema.JD_Milestone_E_DCF, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingOrders.Delivered });

			AddColumn(new ZTextEditColumn("House Bill", TrackingOrder.Schema.JD_Waybill) { ColumnKey = WebTracker.Grids.TrackingOrders.HouseBill });
			AddColumn(new ZTextEditColumn("Master Bill", TrackingOrder.Schema.JD_MasterWaybill) { ColumnKey = WebTracker.Grids.TrackingOrders.MasterBill });

			AddColumn(new ZCodeFindBoxColumn("Load", TrackingOrder.Schema.JD_RL_NKPortOfLoading, "Lookups+PortOfLoadings", typeof(TrackingOrder))
			{
				ColumnKey = WebTracker.Grids.TrackingOrders.Load,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});

			AddColumn(new ZCodeFindBoxColumn("Discharge", TrackingOrder.Schema.JD_RL_NKPortOfDischarge, "Lookups+PortOfDischarges", typeof(TrackingOrder))
			{
				ColumnKey = WebTracker.Grids.TrackingOrders.Discharge,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});

			AddColumn(new ZTextEditColumn("Pickup Address", TrackingOrder.Schema.PickupAddressAsString) { ColumnKey = WebTracker.Grids.TrackingOrders.PickupAddress });
			AddColumn(new ZTextEditColumn("Delivery Address", TrackingOrder.Schema.DeliveryAddressAsString) { ColumnKey = WebTracker.Grids.TrackingOrders.DeliveryAddress });
			AddColumn(new ZTextEditColumn("Consol #", TrackingOrder.Schema.ConsolsAsString) { ColumnKey = WebTracker.Grids.TrackingOrders.ConsolNumber });
			AddColumn(new ZTextEditColumn("Booking Conf. Ref. #", JobOrderHeaderSchema.JD_BookingConfRef.Name) { ColumnKey = WebTracker.Grids.TrackingOrders.BookingConfRef });

			AddColumn(new ZHyperLinksColumn("Container #", "Containers", PKDescription.Schema.Description)
			{
				ColumnKey = WebTracker.Grids.TrackingOrders.ContainerNumber,
				DataNavigateUrlFormatString = TrackingConstants.RelativePath.ContainerDetailsPage + "?Ref={0}", // Redirection path
				DataNavigateUrlFields = new string[] { "PK" }
			});

			AddColumn(new ZTextEditColumn("Invoice #", JobOrderHeaderSchema.JD_InvoiceNumber.Name) { ColumnKey = WebTracker.Grids.TrackingOrders.InvoiceNumber });

			AddColumn(new ZHyperLinksColumn("Product #", "Products", OrgSupplierPartSchema.OP_PartNum.Name)
			{
				ColumnKey = WebTracker.Grids.TrackingOrders.ProductNumber,
				DataNavigateUrlFormatString = TrackingConstants.RelativePath.ProductProfileDetailsPage + "?Ref={0}", // Redirection path
				DataNavigateUrlFields = new string[] { "PK" }
			});

			AddColumn(new ZHyperLinkColumn("Shipment #", TrackingOrder.Schema.ShipOrDecNumber)
			{
				ColumnKey = WebTracker.Grids.TrackingOrders.ShipmentNumber,
				DataNavigateUrlFormatString = TrackingConstants.RelativePath.ShipmentPage + "?Ref={0}&Table={1}", // Redirection path
				DataNavigateUrlFields = new string[] { TrackingOrder.Schema.ShipOrDecPK, TrackingOrder.Schema.ShipOrDecTableName }
			});

			AddColumn(new ZDateTimeColumn("Confirmed Date", JobOrderHeaderSchema.JD_BookingConfDate.Name) { ColumnKey = WebTracker.Grids.TrackingOrders.ConfirmedDate });
			AddColumn(new ZDateTimeColumn("Follow Up Date", JobOrderHeaderSchema.JD_FollowUpDate.Name) { ColumnKey = WebTracker.Grids.TrackingOrders.FollowUpDate });

			AddColumn(new ZFindBoxColumn("Sending Agent", TrackingOrder.Schema.JD_OH_SendingAgent, "Lookups+SendingAgents", typeof(TrackingOrder))
			{
				ColumnKey = WebTracker.Grids.TrackingOrders.SendingAgent,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});

			AddColumn(new ZFindBoxColumn("Receiving Agent", TrackingOrder.Schema.JD_OH_ReceivingAgent, "Lookups+ReceivingAgents", typeof(TrackingOrder))
			{
				ColumnKey = WebTracker.Grids.TrackingOrders.ReceivingAgent,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});

			AddColumn(new ZTextEditColumn("Service Level", TrackingOrder.Schema.ServiceLevel) { ColumnKey = WebTracker.Grids.TrackingOrders.ServiceLevel });
			AddColumn(new ZTextEditColumn("Container Mode", JobOrderHeaderSchema.JD_ContainerMode.Name) { ColumnKey = WebTracker.Grids.TrackingOrders.ContainerMode });
			AddColumn(new ZTextEditColumn("Created On", TrackingOrder.Schema.CreatedOn) { ColumnKey = WebTracker.Grids.TrackingOrders.CreatedOn });
			AddColumn(new ZDateTimeColumn("Created Time", JobOrderHeaderSchema.JD_SystemCreateTimeUtc.Name) { ColumnKey = WebTracker.Grids.TrackingOrders.CreatedTime });
			AddColumn(new ZDateTimeColumn("Last Edit Time", JobOrderHeaderSchema.JD_SystemLastEditTimeUtc.Name) { ColumnKey = WebTracker.Grids.TrackingOrders.LastEditTime });

			AddColumn(new ZTextEditColumn("Main Vessel", TrackingOrder.Schema.MainVessel) { ColumnKey = WebTracker.Grids.TrackingOrders.MainVessel });
			AddColumn(new ZTextEditColumn("Main Voyage/Flight", TrackingOrder.Schema.MainVoyageWithSuppression) { ColumnKey = WebTracker.Grids.TrackingOrders.MainVoyage });

			AddColumn(new ZHyperLinksColumn("Planned Container #", "PlannedContainerNumbers", PKDescription.Schema.Description)
			{
				ColumnKey = WebTracker.Grids.TrackingOrders.PlannedContainers,
				DataNavigateUrlFormatString = TrackingConstants.RelativePath.ContainerDetailsPage + "?Ref={0}",
				DataNavigateUrlFields = new string[] { "PK" }
			});

			AddColumn(new ZDropEditColumn("Incoterm", TrackingOrder.Schema.JD_IncoTerm) { ColumnKey = WebTracker.Grids.TrackingOrders.IncoTerm });
			AddColumn(new ZTextEditColumn("Additional Terms", TrackingOrder.Schema.JD_AdditionalTerms) { ColumnKey = WebTracker.Grids.TrackingOrders.AdditionalTerms });
		}

		void LoginAsQuickShipmentUser()
		{
			var instance = (DummyHttpApplication)WebEnv.AppInstance;
			var helper = new TestHelper(Factory);

			instance.SetSiteUser(helper.TestSiteUser);
			instance.SiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new TrackingOrderColumnProvider(false);
		}
	}
}
