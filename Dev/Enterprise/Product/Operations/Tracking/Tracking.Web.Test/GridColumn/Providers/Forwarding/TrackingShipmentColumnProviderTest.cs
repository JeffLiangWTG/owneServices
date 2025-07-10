using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.UI.WebControls;
using CargoWise.ComponentModel;
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
	[TestedType(typeof(TrackingShipmentColumnProvider))]
	[HttpContextEnabledTest]
	class TrackingShipmentColumnProviderTest : GridColumnProviderTest
	{
		public void TestDeclarationHasColumns()
		{
			var columnProperties = TestProvider.GridColumnFields.Select(c => ((IBindTo)c).BindTo).Where(p => !p.StartsWith("Milestones+"));
			var declarationProperties = typeof(TrackingDeclaration).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanRead).Select(p => p.Name);
			Assert(columnProperties.Any());

			foreach (var columnProperty in columnProperties)
			{
				AssertCollectionContains("Missing column " + columnProperty, columnProperty, declarationProperties);
			}
		}

		public virtual void TestViewAccountsOffColumnKeys()
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
				TestProvider[WebTracker.Grids.TrackingShipments.MainVoyage],
				TestProvider[WebTracker.Grids.TrackingShipments.ShipmentNumber],
				TestProvider[WebTracker.Grids.TrackingShipments.ShipperFullAddress],
				TestProvider[WebTracker.Grids.TrackingShipments.Type]
			};
		}

		protected override DataGridColumn[] GetColumnsForLayoutFixAllDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.TrackingShipments.Type],
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDate],
				TestProvider[WebTracker.Grids.TrackingShipments.ShipmentNumber],
				TestProvider[WebTracker.Grids.Milestones.NextMilestoneDate],
				TestProvider[WebTracker.Grids.TrackingShipments.Origin],
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDescription],
				TestProvider[WebTracker.Grids.TrackingShipments.ShipperFullAddress],
				TestProvider[WebTracker.Grids.Milestones.NextMilestoneDescription]
			};
		}

		protected override DataGridColumn[] GetColumnsForLayoutFixFewDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.TrackingShipments.Type],
				TestProvider[WebTracker.Grids.TrackingShipments.ShipmentNumber],
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDate],
				TestProvider[WebTracker.Grids.TrackingShipments.ShipperFullAddress],
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDescription],
				TestProvider[WebTracker.Grids.TrackingShipments.MainVoyage]
			};
		}

		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddRequiredColumn(new ZHyperLinkColumn("Shipment#", ShipmentDeclarationSchema.Constants.Number)
			{
				ColumnKey = WebTracker.Grids.TrackingShipments.ShipmentNumber,
				DataNavigateUrlFormatString = TrackingConstants.RelativePath.ShipmentPage + "?Ref={0}&Table={1}", // Partial URL
				DataNavigateUrlFields = new string[2] { "PersistentBizOPK", "TableName" }
			});

			AddDefaultsColumn(new ZTextEditColumn("Bill", ShipmentDeclarationSchema.Constants.HouseBill) { ColumnKey = WebTracker.Grids.TrackingShipments.HouseBill });
			AddDefaultsColumn(new ZTextEditColumn("Shipper", ShipmentDeclarationSchema.Constants.ConsignorName) { ColumnKey = WebTracker.Grids.TrackingShipments.Shipper });
			AddDefaultsColumn(new ZTextEditColumn("Consignee", ShipmentDeclarationSchema.Constants.ConsigneeName) { ColumnKey = WebTracker.Grids.TrackingShipments.Consignee });
			AddDefaultsColumn(new ZCodeFindBoxColumn("Origin", ShipmentDeclarationSchema.Constants.OriginPortCode, "ShipmentDeclarationLookups.Ports")
			{
				ColumnKey = WebTracker.Grids.TrackingShipments.Origin,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});
			AddDefaultsColumn(new ZDateTimeColumn("ETD", ShipmentDeclarationSchema.Constants.ETDWithSuppression, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingShipments.ETD });
			AddDefaultsColumn(new ZCodeFindBoxColumn("Destination", ShipmentDeclarationSchema.Constants.DestinationPortCode, "ShipmentDeclarationLookups.Ports")
			{
				ColumnKey = WebTracker.Grids.TrackingShipments.Destination,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});
			AddDefaultsColumn(new ZDateTimeColumn("ETA", ShipmentDeclarationSchema.Constants.ETAWithSuppression, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingShipments.ETA });
			AddColumn(new ZCodeFindBoxColumn("Current Load Port", ShipmentDeclarationSchema.Constants.CurrentLoadPort, "ShipmentDeclarationLookups.Ports") { DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly, ColumnKey = WebTracker.Grids.TrackingShipments.CurrentLoadPort });
			AddColumn(new ZCodeFindBoxColumn("Current Discharge Port", ShipmentDeclarationSchema.Constants.CurrentDischargePort, "ShipmentDeclarationLookups.Ports") { DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly, ColumnKey = WebTracker.Grids.TrackingShipments.CurrentDischargePort });
			AddColumn(new ZTextEditColumn("Current Vessel", ShipmentDeclarationSchema.Constants.CurrentVessel) { ColumnKey = WebTracker.Grids.TrackingShipments.CurrentVessel });
			AddColumn(new ZTextEditColumn("Current Voy./Flight", ShipmentDeclarationSchema.Constants.CurrentVoyageWithSuppression) { ColumnKey = WebTracker.Grids.TrackingShipments.CurrentVoyage });
			AddColumn(new ZTextEditColumn("Shipper's Ref#", ShipmentDeclarationSchema.Constants.BookingReference) { ColumnKey = WebTracker.Grids.TrackingShipments.BookingReference });
			AddColumn(new ZTextEditColumn("Owner's Ref#", ShipmentDeclarationSchema.Constants.OwnerReference) { ColumnKey = WebTracker.Grids.TrackingShipments.OwnerReference });
			AddColumn(new ZTextEditColumn("Mode", ShipmentDeclarationSchema.Constants.TransportMode) { ColumnKey = WebTracker.Grids.TrackingShipments.Mode });
			AddColumn(new ZTextEditColumn("Packs", ShipmentDeclarationSchema.Constants.PacksWithUnits) { ColumnKey = WebTracker.Grids.TrackingShipments.Packs });
			AddColumn(new ZTextEditColumn("Weight", ShipmentDeclarationSchema.Constants.WeightWithUnits) { ColumnKey = WebTracker.Grids.TrackingShipments.Weight });
			AddColumn(new ZTextEditColumn("Volume", ShipmentDeclarationSchema.Constants.VolumeWithUnits) { ColumnKey = WebTracker.Grids.TrackingShipments.Volume });
			AddColumn(new ZCalcEditColumn("Goods Value", ShipmentDeclarationSchema.Constants.GoodsValue) { ColumnKey = WebTracker.Grids.TrackingShipments.GoodsValue });
			AddColumn(new ZCodeFindBoxColumn("Currency", ShipmentDeclarationSchema.Constants.GoodsValueCurrency, "ShipmentDeclarationLookups.Currencies") { ColumnKey = WebTracker.Grids.TrackingShipments.Currency });
			AddColumn(new ZTextEditColumn("Goods Description", ShipmentDeclarationSchema.Constants.GoodsDescription) { ColumnKey = WebTracker.Grids.TrackingShipments.GoodsDescription });
			AddColumn(new ZDateTimeColumn("Estimated Pickup", ShipmentDeclarationSchema.Constants.EstimatedPickupDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingShipments.EstimatedPickup });
			AddColumn(new ZDateTimeColumn("Pickup Required By", ShipmentDeclarationSchema.Constants.PickupDateRequiredBy, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingShipments.PickupRequiredBy });
			AddColumn(new ZDateTimeColumn("Estimated Delivery", ShipmentDeclarationSchema.Constants.EstimatedDeliveryDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingShipments.EstimatedDelivery });
			AddColumn(new ZDateTimeColumn("Delivery Required By", ShipmentDeclarationSchema.Constants.DeliveryDateRequiredBy, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingShipments.DeliveryRequiredBy });
			AddColumn(new ZDateTimeColumn("Delivery Date", ShipmentDeclarationSchema.Constants.DeliveryDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingShipments.DeliveryDate });
			AddColumn(new ZCodeFindBoxColumn("Service Level", ShipmentDeclarationSchema.Constants.ServiceLevelCode, "ShipmentDeclarationLookups.ServiceLevels")
			{
				ColumnKey = WebTracker.Grids.TrackingShipments.ServiceLevel,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});

			if (WebEnv.AppInstance != null)
			{
				TrackingSiteUser siteUser = WebEnv.AppInstance.SiteUser as TrackingSiteUser;
				if (siteUser != null && siteUser.CanViewAccounts)
				{
					AddColumn(new ZTextEditColumn("Charges", ShipmentDeclarationSchema.Constants.Charges) { ColumnKey = WebTracker.Grids.TrackingShipments.Charges });
				}
			}

			AddColumn(new ZTextEditColumn("Shipper Full Address", ShipmentDeclarationSchema.Constants.ConsignorFullAddress) { ColumnKey = WebTracker.Grids.TrackingShipments.ShipperFullAddress });
			AddColumn(new ZTextEditColumn("Shipper Address", ShipmentDeclarationSchema.Constants.ConsignorAddress) { ColumnKey = WebTracker.Grids.TrackingShipments.ShipperAddress });
			AddColumn(new ZTextEditColumn("Shipper City", ShipmentDeclarationSchema.Constants.ConsignorCity) { ColumnKey = WebTracker.Grids.TrackingShipments.ShipperCity });
			AddColumn(new ZTextEditColumn("Shipper State", ShipmentDeclarationSchema.Constants.ConsignorState) { ColumnKey = WebTracker.Grids.TrackingShipments.ShipperState });
			AddColumn(new ZTextEditColumn("Shipper Post Code", ShipmentDeclarationSchema.Constants.ConsignorPostCode) { ColumnKey = WebTracker.Grids.TrackingShipments.ShipperPostCode });
			AddColumn(new ZTextEditColumn("Consignee Full Address", ShipmentDeclarationSchema.Constants.ConsigneeFullAddress) { ColumnKey = WebTracker.Grids.TrackingShipments.ConsigneeFullAddress });
			AddColumn(new ZTextEditColumn("Consignee Address", ShipmentDeclarationSchema.Constants.ConsigneeAddress) { ColumnKey = WebTracker.Grids.TrackingShipments.ConsigneeAddress });
			AddColumn(new ZTextEditColumn("Consignee City", ShipmentDeclarationSchema.Constants.ConsigneeCity) { ColumnKey = WebTracker.Grids.TrackingShipments.ConsigneeCity });
			AddColumn(new ZTextEditColumn("Consignee State", ShipmentDeclarationSchema.Constants.ConsigneeState) { ColumnKey = WebTracker.Grids.TrackingShipments.ConsigneeState });
			AddColumn(new ZTextEditColumn("Consignee Post Code", ShipmentDeclarationSchema.Constants.ConsigneePostCode) { ColumnKey = WebTracker.Grids.TrackingShipments.ConsigneePostCode });
			AddColumn(new ZDateTimeColumn("Received Date", ShipmentDeclarationSchema.Constants.ReceivedDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingShipments.ReceivedDate });
			AddColumn(new ZTextEditColumn("Received By", ShipmentDeclarationSchema.Constants.ReceivedBy) { ColumnKey = WebTracker.Grids.TrackingShipments.ReceivedBy });
			AddColumn(new ZCalcEditColumn("Pieces Received", ShipmentDeclarationSchema.Constants.PiecesReceived) { ColumnKey = WebTracker.Grids.TrackingShipments.PiecesReceived });
			AddColumn(new ZCheckBoxColumn("Booked Online", ShipmentDeclarationSchema.Constants.BookedOnline) { ColumnKey = WebTracker.Grids.TrackingShipments.BookedOnline });
			AddColumn(new ZDateTimeColumn("Actual Pickup", ShipmentDeclarationSchema.Constants.ActualPickupDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingShipments.ActualPickup });
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
			AddDefaultsColumn(new ZTextEditColumn("Declaration Country/Region", "DeclarationCountry") { ColumnKey = WebTracker.Grids.TrackingShipments.DeclarationCountry });
			AddColumn(new ZTextEditColumn("Containers", "Top3Containers") { ColumnKey = WebTracker.Grids.TrackingShipments.Containers });
			AddColumn(new ZTextEditColumn("Order Ref#", ShipmentDeclarationSchema.Constants.OrderReference) { ColumnKey = WebTracker.Grids.TrackingShipments.OrderReferences });
			AddColumn(new ZCodeFindBoxColumn("Main Load Port", ShipmentDeclarationSchema.Constants.MainLoadPort, "ShipmentDeclarationLookups.Ports") { DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly, ColumnKey = WebTracker.Grids.TrackingShipments.MainLoadPort });
			AddColumn(new ZCodeFindBoxColumn("Main Discharge Port", ShipmentDeclarationSchema.Constants.MainDischargePort, "ShipmentDeclarationLookups.Ports") { DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly, ColumnKey = WebTracker.Grids.TrackingShipments.MainDischargePort });
			AddColumn(new ZTextEditColumn("Main Vessel", ShipmentDeclarationSchema.Constants.MainVessel) { ColumnKey = WebTracker.Grids.TrackingShipments.MainVessel });
			AddColumn(new ZTextEditColumn("Main Voy./Flight", ShipmentDeclarationSchema.Constants.MainVoyageWithSuppression) { ColumnKey = WebTracker.Grids.TrackingShipments.MainVoyage });
			AddColumn(new ZTextEditColumn("Type", ShipmentDeclarationSchema.Constants.ShipmentType) { ColumnKey = WebTracker.Grids.TrackingShipments.Type });
			AddColumn(new ZTextEditColumn("Inspection", ShipmentDeclarationSchema.Constants.InspectionTypeCode) { ColumnKey = WebTracker.Grids.TrackingShipments.Inspection });
			AddColumn(new ZTextEditColumn("Additional Terms", ShipmentDeclarationSchema.Constants.AdditionalTerms) { ColumnKey = WebTracker.Grids.TrackingShipments.AdditionalTerms });
			AddColumn(new ZDropEditColumn("Payment Term", ShipmentDeclarationSchema.Constants.PaymentTerm)
			{
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly,
				ColumnKey = WebTracker.Grids.TrackingShipments.INCO,
				BindToList = "PaymentTerm_List"
			});

			AddColumn(new ZCalcEditColumn("Loading Meters", ShipmentDeclarationSchema.Constants.LoadingMeters) { ColumnKey = WebTracker.Grids.TrackingShipments.LoadingMeters });
			AddColumn(new ZTextEditColumn("Container Mode", ShipmentDeclarationSchema.Constants.ContainerMode) { ColumnKey = WebTracker.Grids.TrackingShipments.ContainerMode });

			AddColumn(new ZDropEditColumn("Charges Apply", ShipmentDeclarationSchema.Constants.ChargesApply)
			{
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly,
				ColumnKey = WebTracker.Grids.TrackingShipments.ChargesApply,
				BindToList = "ChargesApply_List"
			});
			AddColumn(new ZDropEditColumn("Release Type", ShipmentDeclarationSchema.Constants.ReleaseType)
			{
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly,
				ColumnKey = WebTracker.Grids.TrackingShipments.ReleaseType,
				BindToList = "ReleaseType_List"
			});
			AddColumn(new ZDropEditColumn("On Board", ShipmentDeclarationSchema.Constants.OnBoard)
			{
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly,
				ColumnKey = WebTracker.Grids.TrackingShipments.OnBoard,
				BindToList = "OnBoard_List"
			});

			AddColumn(new ZTextEditColumn("Pickup Agent", ShipmentDeclarationSchema.Constants.PickupAgentFullName) { ColumnKey = WebTracker.Grids.TrackingShipments.PickupAgent });
			AddColumn(new ZTextEditColumn("Delivery Agent", ShipmentDeclarationSchema.Constants.DeliveryAgentFullName) { ColumnKey = WebTracker.Grids.TrackingShipments.DeliveryAgent });
			AddColumn(new ZDateTimeColumn("Storage Commences", ShipmentDeclarationSchema.Constants.StorageDate) { ColumnKey = WebTracker.Grids.TrackingShipments.StorageDate });
			AddColumn(new ZCalcEditColumn("TEU", ShipmentDeclarationSchema.Constants.TEUCount) { ColumnKey = WebTracker.Grids.TrackingShipments.TEUCount });
			AddColumn(new ZTextEditColumn("Job Notes", ShipmentDeclarationSchema.Constants.Top3JobNotes) { ColumnKey = WebTracker.Grids.TrackingShipments.JobNotes });
			AddColumn(new ZDateTimeColumn("First Leg Load ETD", ShipmentDeclarationSchema.Constants.FirstLegLoadETD) { ColumnKey = WebTracker.Grids.TrackingShipments.FirstLegLoadETD });
			AddColumn(new ZDateTimeColumn("First Leg Load ATD", ShipmentDeclarationSchema.Constants.FirstLegLoadATD) { ColumnKey = WebTracker.Grids.TrackingShipments.FirstLegLoadATD });
			AddColumn(new ZDateTimeColumn("Last Leg Discharge ETA", ShipmentDeclarationSchema.Constants.LastLegDischargeETA) { ColumnKey = WebTracker.Grids.TrackingShipments.LastLegDischargeETA });
			AddColumn(new ZDateTimeColumn("Last Leg Discharge ATA", ShipmentDeclarationSchema.Constants.LastLegDischargeATA) { ColumnKey = WebTracker.Grids.TrackingShipments.LastLegDischargeATA });
		}

		protected override List<object> GetUnsortableColumnKeys() => new List<object>
		{
			WebTracker.Grids.TrackingShipments.Origin,
			WebTracker.Grids.TrackingShipments.Destination,
			WebTracker.Grids.TrackingShipments.CurrentLoadPort,
			WebTracker.Grids.TrackingShipments.CurrentDischargePort,
			WebTracker.Grids.TrackingShipments.Currency,
			WebTracker.Grids.TrackingShipments.ServiceLevel,
			WebTracker.Grids.TrackingShipments.MainLoadPort,
			WebTracker.Grids.TrackingShipments.MainDischargePort
		};

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new TrackingShipmentColumnProvider();
		}

		protected override void SetUp()
		{
			TestHelper testHelper = new TestHelper(Factory);
			testHelper.TestSiteUser.Login(testHelper.TestOrg.OH_Code, testHelper.TestContact.OC_Email, testHelper.TestContact.PasswordForTesting);
			base.SetUp();
			cachedRegistryUseWebAccountsValue = WebDataRegistry.Instance.UseWebAccountsModule.Value;
			WebDataRegistry.Instance.UseWebAccountsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			WebDataRegistry.Instance.UseWebAccountsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedRegistryUseWebAccountsValue);
			base.TearDown();
		}

		bool cachedRegistryUseWebAccountsValue;
	}
}
