using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
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
	[TestedType(typeof(TrackingShipmentsModule))]
	[HttpContextEnabledTest]
	class TrackingShipmentsModuleTest : ZFilterStripGridModuleTestCase
	{
		public void TestDefaultColumnsWhenMilestonesDisabled()
		{
			var oldValue = WebDataRegistry.Instance.MilestoneVisibility.Value;
			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneVisibilityList.Codes.All);
			var testModule = new TrackingShipmentsModule(Factory, null);
			Assert(testModule.AllColumns.ContainsKey("Last Milestone Desc."));
			var hasMilestoneColumn = false;
			var defaultCols = testModule.DefaultGridColumnFields;
			foreach (var col in defaultCols)
			{
				if (col.HeaderText == "Last Milestone Desc.")
				{
					hasMilestoneColumn = true;
				}
			}
			Assert(hasMilestoneColumn);

			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneVisibilityList.Codes.None);
			testModule.Dispose();
			testModule = new TrackingShipmentsModule(Factory, null);
			Assert(!testModule.AllColumns.ContainsKey("Last Milestone Desc."));
			hasMilestoneColumn = false;
			defaultCols = testModule.DefaultGridColumnFields;
			foreach (var col in defaultCols)
			{
				if (col.HeaderText == "Last Milestone Desc.")
				{
					hasMilestoneColumn = true;
				}
			}
			Assert(!hasMilestoneColumn);

			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oldValue);
			testModule.Dispose();
		}

		public virtual void TestGetEDocsBulkDownloadRelevantPK()
		{
			var testHelper = new TestHelper(Factory);
			testHelper.TestSiteUser.Login(testHelper.TestOrg.OH_Code, testHelper.TestContact.OC_Email, testHelper.TestContact.PasswordForTesting);

			var filter = new TrackingShipmentFilterBusinessObject();

			var shipment1 = Factory.NewWithValidTestData<TrackingShipment>();
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment1.JS_OH_DeliveryAgent = testHelper.TestOrg.PK;
			shipment1.JS_UniqueConsignRef = "S00001001";

			var shipment2 = Factory.NewWithValidTestData<TrackingShipment>();
			shipment2.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment2.JS_OH_DeliveryAgent = testHelper.TestOrg.PK;
			shipment2.JS_UniqueConsignRef = "S00001002";

			var shipment3 = Factory.NewWithValidTestData<TrackingShipment>();
			shipment3.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment3.JS_OH_DeliveryAgent = testHelper.TestOrg.PK;
			shipment3.JS_UniqueConsignRef = "S00001003";

			var baseDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			baseDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			baseDeclaration.JE_OH_Supplier = testHelper.TestOrg.PK;
			baseDeclaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;
			baseDeclaration.JE_DeclarationReference = "B00001001";

			Factory.Save();

			FilterGridModule.LoadCollection(filter);
			AssertEquals(4, FilterGridModule.GridCollection.Count);

			using (var module = new TrackingShipmentsModule(Factory, null))
			{
				var grid = new TestHelper.ZTestDataGrid();
				grid.AllowPaging = true;
				grid.PageSize = 2;
				grid.BindTo = "GridCollection";
				grid.Bind(FilterGridModule);

				AssertEquals(2, grid.PageCount);

				var dataKeys = grid.ViewState_Exposed["DataKeys"] as System.Collections.ArrayList;
				dataKeys.Add(shipment1.PK);
				dataKeys.Add(shipment2.PK);

				var iSupportEDocsBulkDownload = module as ISupportEDocsBulkDownload;
				AssertEquals(shipment1.PK, iSupportEDocsBulkDownload.GetEDocsBulkDownloadRelevantPK(grid, 0));
				AssertEquals(shipment2.PK, iSupportEDocsBulkDownload.GetEDocsBulkDownloadRelevantPK(grid, 1));

				var trackingDeclaration = FilterGridModule.GridCollection.OfType<TrackingDeclaration>().First(d => d.Number == baseDeclaration.JE_DeclarationReference);
				grid.SetCurrentPageIndex(1);
				dataKeys.Clear();
				dataKeys.Add(shipment3.PK);
				dataKeys.Add(trackingDeclaration.PK);

				AssertEquals(shipment3.PK, iSupportEDocsBulkDownload.GetEDocsBulkDownloadRelevantPK(grid, 0));
				AssertEquals(baseDeclaration.PK, iSupportEDocsBulkDownload.GetEDocsBulkDownloadRelevantPK(grid, 1));
			}
		}

		public virtual void TestGetEDocsBulkDownloadRelevantAndRelatedPKs()
		{
			var testHelper = new TestHelper(Factory);
			testHelper.TestSiteUser.Login(testHelper.TestOrg.OH_Code, testHelper.TestContact.OC_Email, testHelper.TestContact.PasswordForTesting);

			var filter = new TrackingShipmentFilterBusinessObject();

			var shipment = Factory.NewWithValidTestData<TrackingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_OH_DeliveryAgent = testHelper.TestOrg.PK;
			shipment.JS_UniqueConsignRef = "S00001001";
			//Add related consol
			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;

			var baseDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			baseDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			baseDeclaration.JE_OH_Supplier = testHelper.TestOrg.PK;
			baseDeclaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;
			baseDeclaration.JE_DeclarationReference = "B00001001";
			//Add related
			baseDeclaration.Invoices.AddNew();

			Factory.Save();

			FilterGridModule.LoadCollection(filter);
			AssertEquals(2, FilterGridModule.GridCollection.Count);

			using (var module = new TrackingShipmentsModule(Factory, null))
			{
				var trackingDeclaration = FilterGridModule.GridCollection.OfType<TrackingDeclaration>().First(d => d.Number == baseDeclaration.JE_DeclarationReference);

				var grid = new TestHelper.ZTestDataGrid();
				grid.AllowPaging = true;
				grid.BindTo = "GridCollection";
				grid.Bind(FilterGridModule);

				var dataKeys = grid.ViewState_Exposed["DataKeys"] as System.Collections.ArrayList;
				dataKeys.Add(shipment.PK);
				dataKeys.Add(trackingDeclaration.PK);

				var iSupportEDocsBulkDownload = module as ISupportEDocsBulkDownload;

				//Assert Shipment
				var actual = iSupportEDocsBulkDownload.GetEDocsBulkDownloadRelevantAndRelatedPKs(grid, 0);
				AssertEquals("Count", shipment.DocRelatedPKs.Count + 1, actual.Count);
				AssertEquals("Parent PK", shipment.PK, actual[0]);
				foreach (var relatedPK in shipment.DocRelatedPKs)
				{
					Assert("Contain relatedPK: " + relatedPK, actual.Contains(relatedPK));
				}

				//Assert Declaration
				actual = iSupportEDocsBulkDownload.GetEDocsBulkDownloadRelevantAndRelatedPKs(grid, 1);
				AssertEquals("Count", trackingDeclaration.DocRelatedPKs.Count + 1, actual.Count);
				AssertEquals("Parent PK", trackingDeclaration.Declaration.PK, actual[0]);
				foreach (var relatedPK in trackingDeclaration.DocRelatedPKs)
				{
					Assert("Contain relatedPK: " + relatedPK, actual.Contains(relatedPK));
				}
			}
		}

		#region Overrides

		protected override void SetUp()
		{
			base.SetUp();
			cachedRegistryUseWebAccountsValue = WebDataRegistry.Instance.UseWebAccountsModule.Value;
			WebDataRegistry.Instance.UseWebAccountsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override ZWebTestHelper GetNewHelper() => new TestHelper(Factory);

		protected override void TearDown()
		{
			WebDataRegistry.Instance.UseWebAccountsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedRegistryUseWebAccountsValue);
			base.TearDown();
		}

		bool cachedRegistryUseWebAccountsValue;

		protected override BusinessObject GetNewElement(Type elementType, bool isCancelled)
		{
			var result = base.GetNewElement(elementType, isCancelled);
			var shipment = result as TrackingShipment;
			if (shipment != null)
			{
				shipment.JS_BookingReference = DateTime.Now.Ticks.ToString();
				shipment.JS_OH_DeliveryAgent = SiteUser.LoggedInOrganisation.PK;
			}

			return result;
		}

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override string GetTableName() => Freight.Common.Business.AutoJobShipment.Schema.TableName;

		protected override Type GetCollectionElementType() => typeof(TrackingShipment);

		protected override List<BusinessObject> GetNewBusinessObjectsExpectedFromFilter()
		{
			var result = new List<BusinessObject>();
			for (var i = 1; i < 10; i++)
			{
				var testObject = Factory.NewWithValidTestData<TrackingShipment>();
				testObject.JS_BookingReference = "Include" + i.ToString();
				testObject.JS_OH_DeliveryAgent = SiteUser.LoggedInOrganisation.PK;
				result.Add(testObject);
			}

			return result;
		}

		protected override List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter()
		{
			var result = new List<BusinessObject>();
			for (var i = 1; i < 10; i++)
			{
				var testObject = Factory.NewWithValidTestData<TrackingShipment>();
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

		protected override WebModuleID TestID => WebModuleIDs.TrackingShipments;

		protected override FilterLayoutCodePairRegistryItem ExpectedDefaultLayoutRegistryItem => WebDataRegistry.Instance.DefaultFilterLayoutForwardingShipments;

		protected override bool ExpectCachingOfCollectionKeys => false;

		protected override void FillCollectionWithAtLeastOneElement()
		{
			FilterGridModule.GridCollection.Add(Factory.New<TrackingShipment>());
		}

		#endregion

		#region TestGetBusinessObjectPKColumn

		public override void TestGetBusinessObjectPKColumn()
		{
			base.TestGetBusinessObjectPKColumn();

			AssertEquals(ShipmentDeclarationSchema.PersistentBizOPK, FilterStripGridModule.GetBusinessObjectPKColumn(null));
		}

		#endregion

		#region TestLoadCollectionCore

		public virtual void TestLoadCollectionCore()
		{
			var testHelper = new TestHelper(Factory);
			testHelper.TestSiteUser.Login(testHelper.TestOrg.OH_Code, testHelper.TestContact.OC_Email, testHelper.TestContact.PasswordForTesting);

			var filter = new TrackingShipmentFilterBusinessObject();

			var shipment1 = Factory.NewWithValidTestData<TrackingShipment>();
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment1.JS_OH_DeliveryAgent = testHelper.TestOrg.PK;
			shipment1.JS_UniqueConsignRef = "S00001000";

			var shipment2 = Factory.NewWithValidTestData<TrackingShipment>();
			shipment2.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment2.JS_OH_DeliveryAgent = testHelper.TestOrg.PK;
			shipment2.JS_UniqueConsignRef = "S00001001";

			var shipment3 = Factory.NewWithValidTestData<TrackingShipment>();
			shipment3.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment3.JS_OH_DeliveryAgent = testHelper.TestOrg.PK;
			shipment3.JS_IsCancelled = true;
			shipment3.JS_UniqueConsignRef = "S00001002";

			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration1.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration1.JE_OH_Forwarder = testHelper.TestOrg.PK;
			declaration1.JE_DeclarationReference = "B00001000";

			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration2.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration2.JE_OH_Supplier = testHelper.TestOrg.PK;
			declaration2.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration2.JE_DeclarationReference = "B00001001";

			var declaration3 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration3.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration3.JE_OH_Supplier = testHelper.TestOrg.PK;
			declaration3.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration3.JE_IsCancelled = true;
			declaration3.JE_DeclarationReference = "B00001002";

			var declaration4 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration4.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration4.JE_OH_Supplier = testHelper.TestOrg.PK;
			declaration4.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration4.JE_DeclarationReference = "B00001003";

			var declaration5 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration5.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration5.JE_OH_Importer = testHelper.TestOrg.PK;
			declaration5.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration5.JE_DeclarationReference = "B00001004";

			var declaration6 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration6.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration6.JE_OH_Importer = testHelper.TestOrg.PK;
			declaration6.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration6.JE_DeclarationReference = "B00001005";

			var declaration7 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration7.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration7.JE_DeclarationReference = "B00001006";
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_JobNum = "hello";
			job.JH_ParentID = declaration7.PK;
			job.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			declaration7.Job.JH_OA_LocalChargesAddr = testHelper.TestOrg.MainAddress.PK;

			Factory.Save();

			FilterGridModule.LoadCollection(filter);

			AssertEquals("Expected all shipments and declarations which are active and that can be displayed based on security rights", 5, FilterGridModule.GridCollection.Count);

			var collection = FilterGridModule.GridCollection.Cast<IShipmentDeclaration>().OrderBy(shipmentDeclaration => shipmentDeclaration.Number).ToArray();

			AssertEquals(declaration2.JE_DeclarationReference, collection[0].Number);
			AssertEquals(declaration5.JE_DeclarationReference, collection[1].Number);
			AssertEquals(declaration7.JE_DeclarationReference, collection[2].Number);

			AssertEquals(shipment1.JS_UniqueConsignRef, collection[3].Number);
			AssertEquals(shipment2.JS_UniqueConsignRef, collection[4].Number);

			((ModuleTextFilter)filter["Transport Mode"]).Property = Core.Constants.TransportModes.Air;
			((ModuleTextFilter)filter["Transport Mode"]).IsActive = true;

			FilterGridModule.LoadCollection(filter);

			collection = FilterGridModule.GridCollection.Cast<IShipmentDeclaration>().OrderBy(shipmentDeclaration => shipmentDeclaration.Number).ToArray();
			AssertEquals("Expected shipments and declarations that fit the filter", 2, FilterGridModule.GridCollection.Count);
			AssertEquals(declaration7.JE_DeclarationReference, collection[0].Number);
			AssertEquals(shipment1.JS_UniqueConsignRef, collection[1].Number);
		}

		public virtual void TestLoadCollectionCoreWithCountryFilter()
		{
			var testHelper = new TestHelper(Factory);
			testHelper.TestSiteUser.Login(testHelper.TestOrg.OH_Code, testHelper.TestContact.OC_Email, testHelper.TestContact.PasswordForTesting);

			//companies & branches
			var companyAU = Factory.NewWithValidTestData<GlbCompany>();
			companyAU.GC_OH_OrgProxy = testHelper.TestOrg.PK;
			companyAU.GC_RN_NKCountryCode = "AU";

			var branchAU = Factory.NewWithValidTestData<GlbBranch>();
			companyAU.Branches.Add(branchAU);
			branchAU.GB_OH_OrgProxy = testHelper.TestOrg.PK;
			branchAU.GB_RL_NKHomePort = companyAU.GC_RN_NKCountryCode + "XXX";

			var companyUS = Factory.NewWithValidTestData<GlbCompany>();
			companyUS.GC_OH_OrgProxy = testHelper.TestOrg.PK;
			companyUS.GC_RN_NKCountryCode = "US";

			var branchUS = Factory.NewWithValidTestData<GlbBranch>();
			companyUS.Branches.Add(branchUS);
			branchUS.GB_OH_OrgProxy = testHelper.TestOrg.PK;
			branchUS.GB_RL_NKHomePort = companyUS.GC_RN_NKCountryCode + "XXX";

			var shipment1 = Factory.NewWithValidTestData<TrackingShipment>();
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment1.JS_OH_DeliveryAgent = testHelper.TestOrg.PK;

			var shpDecAU = Factory.NewWithValidTestData<BaseJobDeclaration>();
			shpDecAU.JE_OH_Importer = testHelper.TestOrg.PK;
			shpDecAU.JE_GB = branchAU.PK;
			shpDecAU.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
			shpDecAU.JE_JS = shipment1.PK;

			var shpDecUS = Factory.NewWithValidTestData<BaseJobDeclaration>();
			shpDecUS.JE_OH_Supplier = testHelper.TestOrg.PK;
			shpDecUS.JE_GB = branchUS.PK;
			shpDecUS.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;
			shpDecUS.JE_JS = shipment1.PK;

			//declarations
			var decAU_Imp = Factory.NewWithValidTestData<BaseJobDeclaration>();
			decAU_Imp.JE_OH_Importer = testHelper.TestOrg.PK;
			decAU_Imp.JE_GB = branchAU.PK;
			decAU_Imp.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;

			var decAU_Exp = Factory.NewWithValidTestData<BaseJobDeclaration>();
			decAU_Exp.JE_OH_Supplier = testHelper.TestOrg.PK;
			decAU_Exp.JE_GB = branchAU.PK;
			decAU_Exp.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;

			var decUS_Exp = Factory.NewWithValidTestData<BaseJobDeclaration>();
			decUS_Exp.JE_OH_Supplier = testHelper.TestOrg.PK;
			decUS_Exp.JE_GB = branchUS.PK;
			decUS_Exp.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;

			Factory.Save();

			var filterBO = new TrackingShipmentFilterBusinessObject();

			var countryFilter = (ModuleNkFilter)filterBO[TrackingDeclarationFilterConstants.DeclarationCountry];
			AssertNotNull(countryFilter);
			countryFilter.IsActive = false;

			FilterGridModule.LoadCollection(filterBO);
			AssertEquals("All declarations should be shown", 4, FilterGridModule.GridCollection.Count);
			var pks = FilterGridModule.GridCollection.Cast<IShipmentDeclaration>().Select(s => s.PersistentBizOPK);
			AssertCollectionContains(shipment1.PK, pks);
			AssertCollectionContains(decAU_Imp.PK, pks);
			AssertCollectionContains(decAU_Exp.PK, pks);
			AssertCollectionContains(decUS_Exp.PK, pks);

			countryFilter.IsActive = true;
			countryFilter.Property = "AU";
			FilterGridModule.LoadCollection(filterBO);

			AssertEquals("Only AU jobs are shown", 3, FilterGridModule.GridCollection.Count);
			pks = FilterGridModule.GridCollection.Cast<IShipmentDeclaration>().Select(s => s.PersistentBizOPK);
			AssertCollectionContains(shipment1.PK, pks);
			AssertCollectionContains(decAU_Imp.PK, pks);
			AssertCollectionContains(decAU_Exp.PK, pks);

			countryFilter.IsActive = true;
			countryFilter.Property = "US";
			FilterGridModule.LoadCollection(filterBO);
			AssertEquals("Only US job is shown", 2, FilterGridModule.GridCollection.Count);
			pks = FilterGridModule.GridCollection.Cast<IShipmentDeclaration>().Select(s => s.PersistentBizOPK);
			AssertCollectionContains(shipment1.PK, pks);
			AssertCollectionContains(decUS_Exp.PK, pks);

			countryFilter.IsActive = true;
			countryFilter.Property = "CA";
			FilterGridModule.LoadCollection(filterBO);
			AssertEquals("Only US job is shown", 1, FilterGridModule.GridCollection.Count);
			pks = FilterGridModule.GridCollection.Cast<IShipmentDeclaration>().Select(s => s.PersistentBizOPK);
			AssertCollectionContains(shipment1.PK, pks);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public virtual void TestLoadDeclarationOfOrgAsLocalClient()
		{
			var testHelper = new TestHelper(Factory);
			testHelper.TestSiteUser.Login(testHelper.TestOrg.OH_Code, testHelper.TestContact.OC_Email, testHelper.TestContact.PasswordForTesting);

			var otherOrg = Factory.NewWithValidTestData<OrgHeader>();

			//other companies & branches
			var otherCompAU = Factory.NewWithValidTestData<GlbCompany>();
			otherCompAU.GC_OH_OrgProxy = otherOrg.PK;
			otherCompAU.GC_RN_NKCountryCode = "AU";

			var otherCompBranchAU = Factory.NewWithValidTestData<GlbBranch>();
			otherCompAU.Branches.Add(otherCompBranchAU);
			otherCompBranchAU.GB_OH_OrgProxy = otherOrg.PK;
			otherCompBranchAU.GB_RL_NKHomePort = otherCompAU.GC_RN_NKCountryCode + "XXX";

			//declarations
			var dec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			dec.JE_GB = otherCompBranchAU.PK;
			dec.DontReAssignReferenceNoForUnitTest = true;

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_GB = otherCompBranchAU.PK;
			job.JH_GC = otherCompAU.PK;
			job.JH_JobNum = "hello";
			job.JH_ParentID = dec.PK;
			job.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			job.JH_OA_LocalChargesAddr = testHelper.TestOrg.MainAddress.PK;

			Factory.Save();

			var filterBO = new TrackingShipmentFilterBusinessObject();

			FilterGridModule.LoadCollection(filterBO);
			AssertEquals("All declarations should be shown", 1, FilterGridModule.GridCollection.Count);
			AssertEquals(dec.PK, ((IShipmentDeclaration)FilterGridModule.GridCollection[0]).PersistentBizOPK);
		}

		#endregion

		#region Columns and Sorting

		protected override ColumnDetailsForTest[] ExpectedColumnDetails
		{
			get
			{
				var i = 0;
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
					new ColumnDetailsForTest("Owner's Ref#", i++, typeof(ZTextEditColumn)),
					new ColumnDetailsForTest("Mode", i++, typeof(ZTextEditColumn)),
					new ColumnDetailsForTest("Packs", i++, typeof(ZTextEditColumn)),
					new ColumnDetailsForTest("Weight", i++, typeof(ZTextEditColumn)),
					new ColumnDetailsForTest("Volume", i++, typeof(ZTextEditColumn)),
					new ColumnDetailsForTest("Goods Value", i++, typeof(ZCalcEditColumn)),
					new ColumnDetailsForTest("Currency", i++, typeof(ZCodeFindBoxColumn)),
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

					new ColumnDetailsForTest("Declaration Country/Region", i++, typeof(ZTextEditColumn)),

					new ColumnDetailsForTest("Containers", i++, typeof(ZTextEditColumn)),
					new ColumnDetailsForTest("Order Ref#", i++, typeof(ZTextEditColumn)),

					new ColumnDetailsForTest("Main Load Port", i++, typeof(ZCodeFindBoxColumn)),
					new ColumnDetailsForTest("Main Discharge Port", i++, typeof(ZCodeFindBoxColumn)),
					new ColumnDetailsForTest("Main Vessel", i++, typeof(ZTextEditColumn)),
					new ColumnDetailsForTest("Main Voy./Flight", i++, typeof(ZTextEditColumn)),
					new ColumnDetailsForTest("Type", i++, typeof(ZTextEditColumn)),
					new ColumnDetailsForTest("Inspection", i++, typeof(ZTextEditColumn)),
					new ColumnDetailsForTest("Additional Terms", i++, typeof(ZTextEditColumn)),
					new ColumnDetailsForTest("Payment Term", i++, typeof(ZDropEditColumn)),

					new ColumnDetailsForTest("Loading Meters", i++, typeof(ZCalcEditColumn)),

					new ColumnDetailsForTest("Container Mode", i++, typeof(ZTextEditColumn)),

					new ColumnDetailsForTest("Charges Apply", i++, typeof(ZDropEditColumn)),
					new ColumnDetailsForTest("Release Type", i++, typeof(ZDropEditColumn)),
					new ColumnDetailsForTest("On Board", i++, typeof(ZDropEditColumn)),

					new ColumnDetailsForTest("Pickup Agent", i++, typeof(ZTextEditColumn)),
					new ColumnDetailsForTest("Delivery Agent", i++, typeof(ZTextEditColumn)),
					new ColumnDetailsForTest("Storage Commences", i++, typeof(ZDateTimeColumn)),
					new ColumnDetailsForTest("TEU", i++, typeof(ZCalcEditColumn)),
					new ColumnDetailsForTest("Job Notes", i++, typeof(ZTextEditColumn)),
					new ColumnDetailsForTest("First Leg Load ETD", i++, typeof(ZDateTimeColumn)),
					new ColumnDetailsForTest("First Leg Load ATD", i++, typeof(ZDateTimeColumn)),
					new ColumnDetailsForTest("Last Leg Discharge ETA", i++, typeof(ZDateTimeColumn)),
					new ColumnDetailsForTest("Last Leg Discharge ATA", i++, typeof(ZDateTimeColumn)),
				};
			}
		}

		protected override DataGridColumn[] ExpectedDefaultGridColumns
		{
			get
			{
				var result = new List<DataGridColumn>();

				using (var module = FilterGridModule as TrackingShipmentsModule)
				{
					result.Add(module.AllColumns["Bill"]);
					result.Add(module.AllColumns["Shipper"]);
					result.Add(module.AllColumns["Consignee"]);
					result.Add(module.AllColumns["Origin"]);
					result.Add(module.AllColumns["ETD"]);
					result.Add(module.AllColumns["Destination"]);
					result.Add(module.AllColumns["ETA"]);
					result.Add(module.AllColumns["Last Milestone Desc."]);
					result.Add(module.AllColumns["Declaration Country/Region"]);
				}

				return result.ToArray();
			}
		}

		protected override DataGridColumn[] ExpectedRequiredGridColumns
		{
			get
			{
				var module = FilterGridModule as TrackingShipmentsModule;
				return new[]
				{
					module.AllColumns["Shipment#"]
				};
			}
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(ShipmentDeclarationSchema.ETA.Name, ListSortDirection.Descending) };

		protected override ListSortDirection ExpectedDefaultSortOrder => ListSortDirection.Descending;

		#endregion

		protected override Dictionary<string, string> GetExpectedAuditFilters()
		{
			var result = base.GetExpectedAuditFilters();
			result.Add("Created Time", "Created Time");

			return result;
		}
	}
}
