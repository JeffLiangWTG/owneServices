using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Web;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using Enterprise.ZArchitecture.Web.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Module.Testing
{
	[TestedType(typeof(TrackingCFSShipmentsModule))]
	[HttpContextEnabledTest]
	sealed class TrackingCFSShipmentsModuleTest : ZFilterStripGridModuleTestCase
	{
		public void TestDefaultColumnsWhenMilestonesDisabled()
		{
			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneVisibilityList.Codes.All);
			var testModule = new TrackingCFSShipmentsModule(Factory, null);
			Assert(testModule.AllColumns.ContainsKey("Last Milestone Desc."));
			DataGridColumn[] defaultCols = testModule.DefaultGridColumnFields;
			bool hasMilestoneColumn = defaultCols.ToList().Any(col => col.HeaderText == "Last Milestone Desc.");
			Assert(hasMilestoneColumn);

			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneVisibilityList.Codes.None);
			testModule.Dispose();
			testModule = new TrackingCFSShipmentsModule(Factory, null);
			Assert(!testModule.AllColumns.ContainsKey("Last Milestone Desc."));
			defaultCols = testModule.DefaultGridColumnFields;
			hasMilestoneColumn = defaultCols.ToList().Any(col => col.HeaderText == "Last Milestone Desc.");
			Assert(!hasMilestoneColumn);

			testModule.Dispose();
		}

		#region Overrides

		protected override void SetUp()
		{
			base.SetUp();
			TestHelper testHelper = new TestHelper(Factory);
			testHelper.TestSiteUser.Login(testHelper.TestOrg.OH_Code, testHelper.TestContact.OC_Email, testHelper.TestContact.PasswordForTesting);
			WebDataRegistry.Instance.UseWebAccountsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override BusinessObject GetNewElement(Type elementType, bool isCancelled)
		{
			var result = base.GetNewElement(elementType, isCancelled);
			var shipment = result as TrackingCFSShipment;
			if (shipment != null)
			{
				shipment.JS_BookingReference = DateTime.Now.Ticks.ToString();
				shipment.JS_OH_DeliveryAgent = SiteUser.LoggedInOrganisation.PK;
			}
			return result;
		}

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override string GetTableName()
		{
			return Freight.Common.Business.AutoJobShipment.Schema.TableName;
		}

		protected override Type GetCollectionElementType()
		{
			return typeof(TrackingCFSShipment);
		}

		protected override List<BusinessObject> GetNewBusinessObjectsExpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				TrackingCFSShipment testObject = Factory.NewWithValidTestData<TrackingCFSShipment>();
				testObject.JS_BookingReference = "Include" + i.ToString();
				testObject.JS_OH_DeliveryAgent = SiteUser.LoggedInOrganisation.PK;
				result.Add(testObject);
			}
			return result;
		}

		protected override List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				TrackingCFSShipment testObject = Factory.NewWithValidTestData<TrackingCFSShipment>();
				testObject.JS_BookingReference = "Other" + i.ToString();
				testObject.JS_OH_DeliveryAgent = SiteUser.LoggedInOrganisation.PK;
				result.Add(testObject);
			}
			return result;
		}

		protected override void SetupDBOnlyQuery(ZDBOnlyQuery filter)
		{
			filter.AddToFilter(JobShipmentSchema.JS_BookingReference, SQLComparisonOperator.StartsWith, "Include");
		}

		#endregion

		protected override BusinessObject GetNewBizObjOfType(Type type)
		{
			if (type == typeof(TrackingMilestone))
			{
				return new TrackingMilestone(string.Empty, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, ZInt.Zero);
			}
			return base.GetNewBizObjOfType(type);
		}

		protected override WebModuleID TestID
		{
			get { return WebModuleIDs.TrackingCFSShipments; }
		}

		protected override FilterLayoutCodePairRegistryItem ExpectedDefaultLayoutRegistryItem
		{
			get { return WebDataRegistry.Instance.DefaultFilterLayoutCFSShipments; }
		}

		protected override void FillCollectionWithAtLeastOneElement()
		{
			FilterGridModule.GridCollection.Add(Factory.New<TrackingCFSShipment>());
		}

		#endregion

		#region TestLoadCollectionCore

		public void TestLoadCollectionCore()
		{
			var testHelper = new TestHelper(Factory);
			testHelper.TestSiteUser.Login(testHelper.TestOrg.OH_Code, testHelper.TestContact.OC_Email, testHelper.TestContact.PasswordForTesting);
			AssertEquals("Precondition: Should log in the correct contact", testHelper.TestContact.PK, testHelper.TestSiteUser.LoggedInUserPK);
			var filter = new TrackingCFSShipmentFilterStripBusinessObject();

			var shipment1 = Factory.NewWithValidTestData<TrackingCFSShipment>();
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment1.JS_OH_DeliveryAgent = testHelper.TestOrg.PK;
			shipment1.JS_UniqueConsignRef = "H00001000";

			var shipment2 = Factory.NewWithValidTestData<TrackingCFSShipment>();
			shipment2.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment2.JS_OH_DeliveryAgent = testHelper.TestOrg.PK;
			shipment2.JS_UniqueConsignRef = "H00001001";

			var shipment3 = Factory.NewWithValidTestData<TrackingCFSShipment>();
			shipment3.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment3.JS_OH_DeliveryAgent = testHelper.TestOrg.PK;
			shipment3.JS_IsCancelled = true;
			shipment3.JS_UniqueConsignRef = "H00001002";

			Factory.Save();

			FilterGridModule.LoadCollection(filter);
			AssertEquals("Expected all shipments which are active and that can be displayed based on security rights", 2, FilterGridModule.GridCollection.Count);

			((ModuleTextFilter)filter["Transport Mode"]).Property = Core.Constants.TransportModes.Air;
			((ModuleTextFilter)filter["Transport Mode"]).IsActive = true;

			FilterGridModule.LoadCollection(filter);

			AssertEquals("Expected CFS shipments that fit the filter", 1, FilterGridModule.GridCollection.Count);
		}

		#endregion

		#region Columns and Sorting

		protected override ColumnDetailsForTest[] ExpectedColumnDetails
		{
			get
			{
				int i = 0;
				return new[]
					   {
						new ColumnDetailsForTest("Shipment#", i++, typeof(ZHyperLinkColumn)),
						new ColumnDetailsForTest("Bill", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Shipper", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Consignee", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Origin", i++, typeof(ZCodeFindBoxColumn)),
						new ColumnDetailsForTest("ETD", i++, typeof(ZDateTimeColumn)),
						new ColumnDetailsForTest("Destination", i++, typeof(ZCodeFindBoxColumn)),
						new ColumnDetailsForTest("ETA", i++, typeof(ZDateTimeColumn)),
						new ColumnDetailsForTest("Current Load Port", i++, typeof(ZCodeFindBoxColumn)),
						new ColumnDetailsForTest("Current Discharge Port", i++, typeof(ZCodeFindBoxColumn)),
						new ColumnDetailsForTest("Current Vessel", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Current Voy./Flight", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Shipper's Ref#", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Mode", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Packs", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Weight", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Volume", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Goods Description", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Estimated Pickup", i++, typeof(ZDateTimeColumn)),
						new ColumnDetailsForTest("Pickup Required By", i++, typeof(ZDateTimeColumn)),
						new ColumnDetailsForTest("Estimated Delivery", i++, typeof(ZDateTimeColumn)),
						new ColumnDetailsForTest("Delivery Required By", i++, typeof(ZDateTimeColumn)),
						new ColumnDetailsForTest("Delivery Date", i++, typeof(ZDateTimeColumn)),
						new ColumnDetailsForTest("Service Level", i++, typeof(ZCodeFindBoxColumn)),
						new ColumnDetailsForTest("Charges", i++, typeof(ZTextEditColumn)),

						new ColumnDetailsForTest("Shipper Full Address", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Shipper Address", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Shipper City", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Shipper State", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Shipper Post Code", i++, typeof(ZTextEditColumn)),

						new ColumnDetailsForTest("Consignee Full Address", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Consignee Address", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Consignee City", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Consignee State", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Consignee Post Code", i++, typeof(ZTextEditColumn)),

						new ColumnDetailsForTest("Received Date", i++, typeof(ZDateTimeColumn)),
						new ColumnDetailsForTest("Received By", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Pieces Received", i++, typeof(ZCalcEditColumn)),

						new ColumnDetailsForTest("Booked Online", i++, typeof(ZCheckBoxColumn)),
						new ColumnDetailsForTest("Actual Pickup", i++, typeof(ZDateTimeColumn)),

						new ColumnDetailsForTest("Last Milestone Desc.", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Last Milestone Date", i++, typeof(ZDateTimeColumn)),
						new ColumnDetailsForTest("Next Milestone Desc.", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Next Milestone Date", i++, typeof(ZDateTimeColumn)),

						new ColumnDetailsForTest("Main Load Port", i++, typeof(ZCodeFindBoxColumn)),
						new ColumnDetailsForTest("Main Discharge Port", i++, typeof(ZCodeFindBoxColumn)),
						new ColumnDetailsForTest("Main Vessel", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Main Voy./Flight", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Type", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Additional Terms", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Payment Term", i++, typeof(ZDropEditColumn)),

						new ColumnDetailsForTest("Charges Apply", i++, typeof(ZDropEditColumn)),
						new ColumnDetailsForTest("Release Type", i++, typeof(ZDropEditColumn)),
						new ColumnDetailsForTest("On Board", i++, typeof(ZDropEditColumn)),

						new ColumnDetailsForTest("Pickup Agent", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Delivery Agent", i++, typeof(ZTextEditColumn)),

						new ColumnDetailsForTest("Client Ref", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Interim Receipt", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Whs. Receipt", i++, typeof(ZDateTimeColumn)),
						new ColumnDetailsForTest("Entry No", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Warehouse Location", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Ocean Bill", i++, typeof(ZTextEditColumn))
					   };
			}
		}

		protected override DataGridColumn[] ExpectedDefaultGridColumns
		{
			get
			{
				List<DataGridColumn> result = new List<DataGridColumn>();

				using (TrackingCFSShipmentsModule module = FilterGridModule as TrackingCFSShipmentsModule)
				{
					result.Add(module.AllColumns["Bill"]);
					result.Add(module.AllColumns["Shipper"]);
					result.Add(module.AllColumns["Consignee"]);
					result.Add(module.AllColumns["Origin"]);
					result.Add(module.AllColumns["ETD"]);
					result.Add(module.AllColumns["Destination"]);
					result.Add(module.AllColumns["ETA"]);
					result.Add(module.AllColumns["Last Milestone Desc."]);
				}

				return result.ToArray();
			}
		}

		protected override DataGridColumn[] ExpectedRequiredGridColumns
		{
			get
			{
				var module = FilterGridModule as TrackingCFSShipmentsModule;
				return new[] {
					module.AllColumns["Shipment#"]
				};
			}
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(JobShipmentSchema.JS_E_ARV.Name, ListSortDirection.Descending) };

		protected override ListSortDirection ExpectedDefaultSortOrder
		{
			get
			{
				return ListSortDirection.Descending;
			}
		}
		#endregion

		protected override Dictionary<string, string> GetExpectedAuditFilters()
		{
			Dictionary<string, string> result = base.GetExpectedAuditFilters();
			result.Add("Created Time", "Created Time");
			result.Add("Created On Web/Internal", "Created On Web/Internal");
			result.Add("Last Edit Time", "Last Edit Time");
			return result;
		}
	}
}
