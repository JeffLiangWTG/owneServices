using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Web;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(TrackingCFSShipmentColumnProvider))]
	[HttpContextEnabledTest]
	sealed class CFSShipmentColumnProviderTest : GridColumnProviderTest
	{
		public void TestViewAccountsOffColumnKeys()
		{
			WebDataRegistry.Instance.UseWebAccountsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			TestColumnKeys();
			TestUniqueColumns();
			TestDefaultColumns();
			TestRequiredColumns();
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
				TestProvider[WebTracker.Grids.CFSShipments.MainVoyage],
				TestProvider[WebTracker.Grids.CFSShipments.ShipmentNumber],
				TestProvider[WebTracker.Grids.CFSShipments.ShipperFullAddress],
				TestProvider[WebTracker.Grids.CFSShipments.Type]
			};
		}

		protected override DataGridColumn[] GetColumnsForLayoutFixAllDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.CFSShipments.Type],
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDate],
				TestProvider[WebTracker.Grids.CFSShipments.ShipmentNumber],
				TestProvider[WebTracker.Grids.Milestones.NextMilestoneDate],
				TestProvider[WebTracker.Grids.CFSShipments.Origin],
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDescription],
				TestProvider[WebTracker.Grids.CFSShipments.ShipperFullAddress],
				TestProvider[WebTracker.Grids.Milestones.NextMilestoneDescription]
			};
		}

		protected override DataGridColumn[] GetColumnsForLayoutFixFewDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.CFSShipments.Type],
				TestProvider[WebTracker.Grids.CFSShipments.ShipmentNumber],
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDate],
				TestProvider[WebTracker.Grids.CFSShipments.ShipperFullAddress],
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDescription],
				TestProvider[WebTracker.Grids.CFSShipments.MainVoyage]
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddRequiredColumn(new ZHyperLinkColumn("Shipment#", TrackingCFSShipment.Schema.JS_UniqueConsignRef)
			{
				ColumnKey = WebTracker.Grids.CFSShipments.ShipmentNumber,
				DataNavigateUrlFormatString = TrackingConstants.RelativePath.ShipmentPage + "?Ref={0}&Table={1}", // Partial URL
				DataNavigateUrlFields = new string[2] { "PersistentBizOPK", "TableName" }
			});

			AddDefaultsColumn(new ZTextEditColumn("Bill", TrackingCFSShipment.Schema.JS_HouseBill) { ColumnKey = WebTracker.Grids.CFSShipments.HouseBill });
			AddDefaultsColumn(new ZTextEditColumn("Shipper", TrackingCFSShipment.Schema.ConsignorName) { ColumnKey = WebTracker.Grids.CFSShipments.Shipper });
			AddDefaultsColumn(new ZTextEditColumn("Consignee", TrackingCFSShipment.Schema.ConsigneeName) { ColumnKey = WebTracker.Grids.CFSShipments.Consignee });
			AddDefaultsColumn(new ZCodeFindBoxColumn("Origin", TrackingCFSShipment.Schema.JS_RL_NKOrigin, "Ports")
			{
				ColumnKey = WebTracker.Grids.CFSShipments.Origin,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});
			AddDefaultsColumn(new ZDateTimeColumn("ETD", TrackingCFSShipment.Schema.ETDWithSuppression, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.CFSShipments.ETD });
			AddDefaultsColumn(new ZCodeFindBoxColumn("Destination", TrackingCFSShipment.Schema.JS_RL_NKDestination, "Ports")
			{
				ColumnKey = WebTracker.Grids.CFSShipments.Destination,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});
			AddDefaultsColumn(new ZDateTimeColumn("ETA", TrackingCFSShipment.Schema.ETAWithSuppression, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.CFSShipments.ETA });
			AddColumn(new ZCodeFindBoxColumn("Current Load Port", TrackingCFSShipment.Schema.CurrentLoadPort, "Ports") { DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly, ColumnKey = WebTracker.Grids.CFSShipments.CurrentLoadPort });
			AddColumn(new ZCodeFindBoxColumn("Current Discharge Port", TrackingCFSShipment.Schema.CurrentDischargePort, "Ports") { DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly, ColumnKey = WebTracker.Grids.CFSShipments.CurrentDischargePort });
			AddColumn(new ZTextEditColumn("Current Vessel", TrackingCFSShipment.Schema.CurrentVessel) { ColumnKey = WebTracker.Grids.CFSShipments.CurrentVessel });
			AddColumn(new ZTextEditColumn("Current Voy./Flight", TrackingCFSShipment.Schema.CurrentVoyageWithSuppression) { ColumnKey = WebTracker.Grids.CFSShipments.CurrentVoyage });
			AddColumn(new ZTextEditColumn("Shipper's Ref#", TrackingCFSShipment.Schema.JS_BookingReference) { ColumnKey = WebTracker.Grids.CFSShipments.BookingReference });
			AddColumn(new ZTextEditColumn("Mode", TrackingCFSShipment.Schema.TransportMode) { ColumnKey = WebTracker.Grids.CFSShipments.Mode });
			AddColumn(new ZTextEditColumn("Packs", TrackingCFSShipment.Schema.PacksWithUnits) { ColumnKey = WebTracker.Grids.CFSShipments.Packs });
			AddColumn(new ZTextEditColumn("Weight", TrackingCFSShipment.Schema.WeightWithUnits) { ColumnKey = WebTracker.Grids.CFSShipments.Weight });
			AddColumn(new ZTextEditColumn("Volume", TrackingCFSShipment.Schema.VolumeWithUnits) { ColumnKey = WebTracker.Grids.CFSShipments.Volume });
			AddColumn(new ZTextEditColumn("Goods Description", TrackingCFSShipment.Schema.JS_GoodsDescription) { ColumnKey = WebTracker.Grids.CFSShipments.GoodsDescription });
			AddColumn(new ZDateTimeColumn("Estimated Pickup", TrackingCFSShipment.Schema.EstimatedPickupDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.CFSShipments.EstimatedPickup });
			AddColumn(new ZDateTimeColumn("Pickup Required By", TrackingCFSShipment.Schema.PickupDateRequiredBy, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.CFSShipments.PickupRequiredBy });
			AddColumn(new ZDateTimeColumn("Estimated Delivery", TrackingCFSShipment.Schema.EstimatedDeliveryDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.CFSShipments.EstimatedDelivery });
			AddColumn(new ZDateTimeColumn("Delivery Required By", TrackingCFSShipment.Schema.DeliveryDateRequiredBy, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.CFSShipments.DeliveryRequiredBy });
			AddColumn(new ZDateTimeColumn("Delivery Date", TrackingCFSShipment.Schema.DeliveryDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.CFSShipments.DeliveryDate });
			AddColumn(new ZCodeFindBoxColumn("Service Level", TrackingCFSShipment.Schema.JS_RS_NKServiceLevel, "ServiceLevels")
			{
				ColumnKey = WebTracker.Grids.CFSShipments.ServiceLevel,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});

			if (WebEnv.AppInstance != null)
			{
				TrackingSiteUser siteUser = WebEnv.AppInstance.SiteUser as TrackingSiteUser;
				if (siteUser != null && siteUser.CanViewAccounts)
				{
					AddColumn(new ZTextEditColumn("Charges", TrackingCFSShipment.Schema.Charges) { ColumnKey = WebTracker.Grids.CFSShipments.Charges });
				}
			}

			AddColumn(new ZTextEditColumn("Shipper Full Address", TrackingCFSShipment.Schema.ConsignorFullAddress) { ColumnKey = WebTracker.Grids.CFSShipments.ShipperFullAddress });
			AddColumn(new ZTextEditColumn("Shipper Address", TrackingCFSShipment.Schema.ConsignorAddress) { ColumnKey = WebTracker.Grids.CFSShipments.ShipperAddress });
			AddColumn(new ZTextEditColumn("Shipper City", TrackingCFSShipment.Schema.ConsignorCity) { ColumnKey = WebTracker.Grids.CFSShipments.ShipperCity });
			AddColumn(new ZTextEditColumn("Shipper State", TrackingCFSShipment.Schema.ConsignorState) { ColumnKey = WebTracker.Grids.CFSShipments.ShipperState });
			AddColumn(new ZTextEditColumn("Shipper Post Code", TrackingCFSShipment.Schema.ConsignorPostCode) { ColumnKey = WebTracker.Grids.CFSShipments.ShipperPostCode });
			AddColumn(new ZTextEditColumn("Consignee Full Address", TrackingCFSShipment.Schema.ConsigneeFullAddress) { ColumnKey = WebTracker.Grids.CFSShipments.ConsigneeFullAddress });
			AddColumn(new ZTextEditColumn("Consignee Address", TrackingCFSShipment.Schema.ConsigneeAddress) { ColumnKey = WebTracker.Grids.CFSShipments.ConsigneeAddress });
			AddColumn(new ZTextEditColumn("Consignee City", TrackingCFSShipment.Schema.ConsigneeCity) { ColumnKey = WebTracker.Grids.CFSShipments.ConsigneeCity });
			AddColumn(new ZTextEditColumn("Consignee State", TrackingCFSShipment.Schema.ConsigneeState) { ColumnKey = WebTracker.Grids.CFSShipments.ConsigneeState });
			AddColumn(new ZTextEditColumn("Consignee Post Code", TrackingCFSShipment.Schema.ConsigneePostCode) { ColumnKey = WebTracker.Grids.CFSShipments.ConsigneePostCode });
			AddColumn(new ZDateTimeColumn("Received Date", TrackingCFSShipment.Schema.ReceivedDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.CFSShipments.ReceivedDate });
			AddColumn(new ZTextEditColumn("Received By", TrackingCFSShipment.Schema.ReceivedBy) { ColumnKey = WebTracker.Grids.CFSShipments.ReceivedBy });
			AddColumn(new ZCalcEditColumn("Pieces Received", TrackingCFSShipment.Schema.PiecesReceived) { ColumnKey = WebTracker.Grids.CFSShipments.PiecesReceived });
			AddColumn(new ZCheckBoxColumn("Booked Online", TrackingCFSShipment.Schema.BookedOnline) { ColumnKey = WebTracker.Grids.CFSShipments.BookedOnline });
			AddColumn(new ZDateTimeColumn("Actual Pickup", TrackingCFSShipment.Schema.ActualPickupDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.CFSShipments.ActualPickup });
			if (WebDataRegistry.Instance.MilestoneVisibility.Value != MilestoneVisibilityList.Codes.None)
			{
				foreach (ZTemplateColumn column in ConfigurationHelper.GetMilestonesColumns("Milestones")) // Data column name
				{
					if (column.HeaderText != "Last Milestone Desc.")
					{
						AddColumn(column);
					}
					else
					{
						AddDefaultsColumn(column);
					}
				}
			}
			AddColumn(new ZCodeFindBoxColumn("Main Load Port", TrackingCFSShipment.Schema.MainLoadPort, "Ports") { DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly, ColumnKey = WebTracker.Grids.CFSShipments.MainLoadPort });
			AddColumn(new ZCodeFindBoxColumn("Main Discharge Port", TrackingCFSShipment.Schema.MainDischargePort, "Ports") { DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly, ColumnKey = WebTracker.Grids.CFSShipments.MainDischargePort });
			AddColumn(new ZTextEditColumn("Main Vessel", TrackingCFSShipment.Schema.MainVessel) { ColumnKey = WebTracker.Grids.CFSShipments.MainVessel });
			AddColumn(new ZTextEditColumn("Main Voy./Flight", TrackingCFSShipment.Schema.MainVoyageWithSuppression) { ColumnKey = WebTracker.Grids.CFSShipments.MainVoyage });
			AddColumn(new ZTextEditColumn("Type", TrackingCFSShipment.Schema.ShipmentType) { ColumnKey = WebTracker.Grids.CFSShipments.Type });
			AddColumn(new ZTextEditColumn("Additional Terms", TrackingCFSShipment.Schema.JS_AdditionalTerms) { ColumnKey = WebTracker.Grids.CFSShipments.AdditionalTerms });
			AddColumn(new ZDropEditColumn("Payment Term", TrackingCFSShipment.Schema.JS_INCO)
			{
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly,
				ColumnKey = WebTracker.Grids.CFSShipments.INCO,
				BindToList = "PaymentTerm_List"
			});

			AddColumn(new ZDropEditColumn("Charges Apply", TrackingCFSShipment.Schema.JS_HBLAWBChargesDisplay)
			{
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly,
				ColumnKey = WebTracker.Grids.CFSShipments.ChargesApply,
				BindToList = "ChargesApply_List"
			});
			AddColumn(new ZDropEditColumn("Release Type", TrackingCFSShipment.Schema.JS_ReleaseType)
			{
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly,
				ColumnKey = WebTracker.Grids.CFSShipments.ReleaseType,
				BindToList = "ReleaseType_List"
			});
			AddColumn(new ZDropEditColumn("On Board", TrackingCFSShipment.Schema.JS_ShippedOnBoard)
			{
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly,
				ColumnKey = WebTracker.Grids.CFSShipments.OnBoard,
				BindToList = "OnBoard_List"
			});

			AddColumn(new ZTextEditColumn("Pickup Agent", TrackingCFSShipment.Schema.PickupAgentFullName) { ColumnKey = WebTracker.Grids.CFSShipments.PickupAgent });
			AddColumn(new ZTextEditColumn("Delivery Agent", TrackingCFSShipment.Schema.DeliveryAgentFullName) { ColumnKey = WebTracker.Grids.CFSShipments.DeliveryAgent });
			AddColumn(new ZTextEditColumn("Client Ref", TrackingCFSShipment.Schema.JS_ConsolReference) { ColumnKey = WebTracker.Grids.CFSShipments.ClientRef });
			AddColumn(new ZTextEditColumn("Interim Receipt", TrackingCFSShipment.Schema.JS_InterimReceipt) { ColumnKey = WebTracker.Grids.CFSShipments.InterimReceipt });
			AddColumn(new ZDateTimeColumn("Whs. Receipt", TrackingCFSShipment.Schema.JS_A_RCV, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.CFSShipments.WhsReceipt });
			AddColumn(new ZTextEditColumn("Entry No", TrackingCFSShipment.Schema.CustomsEntryNumber) { ColumnKey = WebTracker.Grids.CFSShipments.EntryNo });
			AddColumn(new ZTextEditColumn("Warehouse Location", TrackingCFSShipment.Schema.JS_WarehouseLocation) { ColumnKey = WebTracker.Grids.CFSShipments.WhsLocation });
			AddColumn(new ZTextEditColumn("Ocean Bill", TrackingCFSShipment.Schema.MasterBill) { ColumnKey = WebTracker.Grids.CFSShipments.MasterBill });
		}

		protected override List<object> GetUnsortableColumnKeys() => new List<object>
		{
			WebTracker.Grids.CFSShipments.Origin,
			WebTracker.Grids.CFSShipments.Destination,
			WebTracker.Grids.CFSShipments.CurrentLoadPort,
			WebTracker.Grids.CFSShipments.CurrentDischargePort,
			WebTracker.Grids.CFSShipments.ServiceLevel,
			WebTracker.Grids.CFSShipments.MainLoadPort,
			WebTracker.Grids.CFSShipments.MainDischargePort
		};

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new TrackingCFSShipmentColumnProvider();
		}

		protected override void SetUp()
		{
			TestHelper testHelper = new TestHelper(Factory);
			testHelper.TestSiteUser.Login(testHelper.TestOrg.OH_Code, testHelper.TestContact.OC_Email, testHelper.TestContact.PasswordForTesting);
			base.SetUp();
			WebDataRegistry.Instance.UseWebAccountsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}
	}
}
