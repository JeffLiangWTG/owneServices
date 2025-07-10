using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using Enterprise.ZArchitecture.Web.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Module.Testing
{
	[TestedType(typeof(TrackingDeclarationsModule))]
	class TrackingDeclarationsModuleTest : TrackingShipmentsModuleTest
	{
		protected override FilterLayoutCodePairRegistryItem ExpectedDefaultLayoutRegistryItem
		{
			get { return WebDataRegistry.Instance.DefaultFilterLayoutDeclaration; }
		}

		#region TestLoadCollectionCore

		public override void TestLoadCollectionCore()
		{
			var testHelper = new TestHelper(Factory);
			testHelper.TestSiteUser.Login(testHelper.TestOrg.OH_Code, testHelper.TestContact.OC_Email, testHelper.TestContact.PasswordForTesting);

			var filter = new JobDeclarationFilterBusinessObject();

			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration1.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration1.JE_OH_Forwarder = testHelper.TestOrg.PK;

			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration2.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration2.JE_OH_Forwarder = testHelper.TestOrg.PK;

			var declaration3 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration3.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration3.JE_OH_Supplier = testHelper.TestOrg.PK;
			declaration3.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;

			var declaration4 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration4.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration4.JE_OH_Supplier = testHelper.TestOrg.PK;
			declaration4.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;

			var declaration5 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration5.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration5.JE_OH_Importer = testHelper.TestOrg.PK;
			declaration5.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;

			var declaration6 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration6.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration6.JE_OH_Importer = testHelper.TestOrg.PK;
			declaration6.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;

			var declaration7 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration7.JE_TransportMode = Core.Constants.TransportModes.Air;
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

			AssertEquals("Expected all shipments and declarations which can be displayed", 3, FilterGridModule.GridCollection.Count);
			var pks = FilterGridModule.GridCollection.Cast<TrackingDeclaration>().Select(trackDec => trackDec.Declaration.PK);
			AssertCollectionContains(declaration4.PK, pks);
			AssertCollectionContains(declaration5.PK, pks);
			AssertCollectionContains(declaration7.PK, pks);

			((ModuleTextFilter)filter["Transport Mode"]).Property = Core.Constants.TransportModes.Air;
			((ModuleTextFilter)filter["Transport Mode"]).IsActive = true;

			FilterGridModule.LoadCollection(filter);
			AssertEquals("Expected shipments and declarations that fit the filter", 2, FilterGridModule.GridCollection.Count);
			pks = FilterGridModule.GridCollection.Cast<TrackingDeclaration>().Select(trackDec => trackDec.Declaration.PK);
			AssertCollectionContains(declaration4.PK, pks);
			AssertCollectionContains(declaration7.PK, pks);
		}

		public override void TestLoadCollectionCoreWithCountryFilter()
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

			Factory.Save();

			var filterBO = new JobDeclarationFilterBusinessObject();

			var countryFilter = (ModuleNkFilter)filterBO[DeclarationFilterConstants.Country];
			countryFilter.IsActive = false;

			FilterGridModule.LoadCollection(filterBO);
			AssertEquals("All declarations should be shown", 5, FilterGridModule.GridCollection.Count);

			countryFilter.IsActive = true;
			countryFilter.Property = "AU";
			FilterGridModule.LoadCollection(filterBO);

			AssertEquals("Only AU jobs are shown", 3, FilterGridModule.GridCollection.Count);
			var pks = FilterGridModule.GridCollection.Cast<TrackingDeclaration>().Select(trackDec => trackDec.Declaration.PK);
			AssertCollectionContains(decAU_Imp.PK, pks);
			AssertCollectionContains(decAU_Exp.PK, pks);
			AssertCollectionContains(shpDecAU.PK, pks);

			countryFilter.IsActive = true;
			countryFilter.Property = "US";
			FilterGridModule.LoadCollection(filterBO);
			AssertEquals("Only US job is shown", 2, FilterGridModule.GridCollection.Count);
			pks = FilterGridModule.GridCollection.Cast<TrackingDeclaration>().Select(trackDec => trackDec.Declaration.PK);
			AssertCollectionContains(decUS_Exp.PK, pks);
			AssertCollectionContains(shpDecUS.PK, pks);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public override void TestLoadDeclarationOfOrgAsLocalClient()
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

			var filterBO = new JobDeclarationFilterBusinessObject();

			FilterGridModule.LoadCollection(filterBO);
			AssertEquals("All declarations should be shown", 1, FilterGridModule.GridCollection.Count);
			AssertEquals(dec.PK, ((TrackingDeclaration)FilterGridModule.GridCollection[0]).Declaration.PK);
		}

		public override void TestGetEDocsBulkDownloadRelevantPK()
		{
			var testHelper = new TestHelper(Factory);
			testHelper.TestSiteUser.Login(testHelper.TestOrg.OH_Code, testHelper.TestContact.OC_Email, testHelper.TestContact.PasswordForTesting);

			var filter = GetDeclarationFilterBusinessObject();
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

			var baseDeclaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			baseDeclaration1.JE_TransportMode = Core.Constants.TransportModes.Sea;
			baseDeclaration1.JE_OH_Supplier = testHelper.TestOrg.PK;
			baseDeclaration1.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;
			baseDeclaration1.JE_DeclarationReference = "B00001001";
			baseDeclaration1.JE_JS = shipment1.PK;

			var baseDeclaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			baseDeclaration2.JE_TransportMode = Core.Constants.TransportModes.Sea;
			baseDeclaration2.JE_OH_Supplier = testHelper.TestOrg.PK;
			baseDeclaration2.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;
			baseDeclaration2.JE_DeclarationReference = "B00001002";
			baseDeclaration2.JE_JS = shipment2.PK;

			var baseDeclaration3 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			baseDeclaration3.JE_TransportMode = Core.Constants.TransportModes.Sea;
			baseDeclaration3.JE_OH_Supplier = testHelper.TestOrg.PK;
			baseDeclaration3.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;
			baseDeclaration3.JE_DeclarationReference = "B00001003";
			baseDeclaration3.JE_JS = shipment3.PK;

			var baseDeclaration4 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			baseDeclaration4.JE_TransportMode = Core.Constants.TransportModes.Sea;
			baseDeclaration4.JE_OH_Supplier = testHelper.TestOrg.PK;
			baseDeclaration4.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;
			baseDeclaration4.JE_DeclarationReference = "B00001004";

			Factory.Save();

			FilterGridModule.LoadCollection(filter);
			AssertEquals(4, FilterGridModule.GridCollection.Count);

			using (var module = new TrackingShipmentsModule(Factory, null))
			{
				var trackingDeclaration1 = FilterGridModule.GridCollection.OfType<TrackingDeclaration>().First(d => d.Number == baseDeclaration1.JE_DeclarationReference);
				var trackingDeclaration2 = FilterGridModule.GridCollection.OfType<TrackingDeclaration>().First(d => d.Number == baseDeclaration2.JE_DeclarationReference);
				var trackingDeclaration3 = FilterGridModule.GridCollection.OfType<TrackingDeclaration>().First(d => d.Number == baseDeclaration3.JE_DeclarationReference);
				var trackingDeclaration4 = FilterGridModule.GridCollection.OfType<TrackingDeclaration>().First(d => d.Number == baseDeclaration4.JE_DeclarationReference);

				var grid = new TestHelper.ZTestDataGrid();
				grid.AllowPaging = true;
				grid.PageSize = 2;
				grid.BindTo = "GridCollection";
				grid.Bind(FilterGridModule);

				AssertEquals(2, grid.PageCount);

				var dataKeys = grid.ViewState_Exposed["DataKeys"] as System.Collections.ArrayList;
				dataKeys.Add(trackingDeclaration1.PK);
				dataKeys.Add(trackingDeclaration2.PK);

				var iSupportEDocsBulkDownload = module as ISupportEDocsBulkDownload;
				AssertEquals(baseDeclaration1.PK, iSupportEDocsBulkDownload.GetEDocsBulkDownloadRelevantPK(grid, 0));
				AssertEquals(baseDeclaration2.PK, iSupportEDocsBulkDownload.GetEDocsBulkDownloadRelevantPK(grid, 1));

				grid.SetCurrentPageIndex(1);
				dataKeys.Clear();
				dataKeys.Add(trackingDeclaration3.PK);
				dataKeys.Add(trackingDeclaration4.PK);

				AssertEquals(baseDeclaration3.PK, iSupportEDocsBulkDownload.GetEDocsBulkDownloadRelevantPK(grid, 0));
				AssertEquals(baseDeclaration4.PK, iSupportEDocsBulkDownload.GetEDocsBulkDownloadRelevantPK(grid, 1));
			}
		}

		public override void TestGetEDocsBulkDownloadRelevantAndRelatedPKs()
		{
			var testHelper = new TestHelper(Factory);
			testHelper.TestSiteUser.Login(testHelper.TestOrg.OH_Code, testHelper.TestContact.OC_Email, testHelper.TestContact.PasswordForTesting);

			var filter = GetDeclarationFilterBusinessObject();
			var shipment = Factory.NewWithValidTestData<TrackingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_OH_DeliveryAgent = testHelper.TestOrg.PK;
			shipment.JS_UniqueConsignRef = "S00001001";

			var baseDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			baseDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			baseDeclaration.JE_OH_Supplier = testHelper.TestOrg.PK;
			baseDeclaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;
			baseDeclaration.JE_DeclarationReference = "B00001001";

			//add related 
			baseDeclaration.JE_JS = shipment.PK;
			baseDeclaration.Invoices.AddNew();
			baseDeclaration.CustomsEntryHeaders.AddNew();
			var landedCostHeader = (BusinessObject)Factory.New<Integration.LandedCosting.ILandedCostHeader>();
			landedCostHeader[LandedCostHeaderSchema.LT_ParentID.Name] = baseDeclaration.PK;
			landedCostHeader[LandedCostHeaderSchema.LT_ParentTableCode.Name] = JobDeclarationSchema.Constants.Prefix;

			Factory.Save();

			FilterGridModule.LoadCollection(filter);
			AssertEquals(1, FilterGridModule.GridCollection.Count);

			using (var module = new TrackingShipmentsModule(Factory, null))
			{
				var trackingDeclaration1 = FilterGridModule.GridCollection.OfType<TrackingDeclaration>().First(d => d.Number == baseDeclaration.JE_DeclarationReference);

				var grid = new TestHelper.ZTestDataGrid();
				grid.BindTo = "GridCollection";
				grid.Bind(FilterGridModule);

				var dataKeys = grid.ViewState_Exposed["DataKeys"] as System.Collections.ArrayList;
				dataKeys.Add(trackingDeclaration1.PK);

				var iSupportEDocsBulkDownload = module as ISupportEDocsBulkDownload;

				//Assert
				var actual = iSupportEDocsBulkDownload.GetEDocsBulkDownloadRelevantAndRelatedPKs(grid, 0);
				AssertEquals("Count", trackingDeclaration1.DocRelatedPKs.Count + 1, actual.Count);
				AssertEquals("Parent PK", trackingDeclaration1.Declaration.PK, actual[0]);
				foreach (var relatedPK in trackingDeclaration1.DocRelatedPKs)
				{
					Assert("Contain relatedPK: " + relatedPK, actual.Contains(relatedPK));
				}
			}
		}

		JobDeclarationFilterBusinessObject GetDeclarationFilterBusinessObject()
		{
			var declarationFilterBusinessObjectFactory = new TrackingJobDeclarationFilterBusinessObjectFactory();
			return declarationFilterBusinessObjectFactory.GetJobDeclarationFilterBusinessObject("AU");
		}
		#endregion

		#region Overrides

		protected override BusinessObject GetNewElement(Type elementType, bool isCancelled)
		{
			var result = base.GetNewElement(elementType, isCancelled);
			var declaration = result as BaseJobDeclaration;
			if (declaration != null)
			{
				declaration.JE_AgentsReference = DateTime.Now.Ticks.ToString();
				declaration.JE_OH_Importer = SiteUser.LoggedInOrganisation.PK;
			}
			return result;
		}

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override List<BusinessObject> GetNewBusinessObjectsExpectedFromFilter()
		{
			var result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				var testObject = Factory.NewWithValidTestData<BaseJobDeclaration>();
				testObject.JE_AgentsReference = "I" + i.ToString();
				testObject.JE_OH_Importer = SiteUser.LoggedInOrganisation.PK;
				result.Add(testObject);
			}
			return result;
		}

		protected override List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter()
		{
			var result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				var testObject = Factory.NewWithValidTestData<BaseJobDeclaration>();
				testObject.JE_AgentsReference = "O" + i.ToString();
				testObject.JE_OH_Importer = SiteUser.LoggedInOrganisation.PK;
				result.Add(testObject);
			}
			return result;
		}

		protected override string GetTableName()
		{
			return "JobDeclaration";
		}

		protected override void SetupDBOnlyQuery(ZDBOnlyQuery filter)
		{
			filter.AddToFilter(JobDeclarationSchema.JE_AgentsReference, SQLComparisonOperator.StartsWith, "I");
		}

		#endregion

		protected override BusinessObject GetNewBizObjOfType(Type type)
		{
			if (type == typeof(TrackingDeclaration))
			{
				return new TrackingDeclaration(Factory.New<BaseJobDeclaration>());
			}
			else if (type == typeof(Freight.Business.JobDocsAndCartage))
			{
				return Factory.New<BaseJobDeclaration>().DocsAndCartage;
			}
			return base.GetNewBizObjOfType(type);
		}

		protected override WebModuleID TestID
		{
			get { return WebModuleIDs.TrackingDeclarations; }
		}

		protected override bool ExpectCachingOfCollectionKeys
		{
			get { return false; }
		}

		protected override void FillCollectionWithAtLeastOneElement()
		{
			FilterGridModule.GridCollection.Add(new TrackingDeclaration(Factory.New<BaseJobDeclaration>()));
		}

		#endregion

		#region TestGetBusinessObjectPKColumn

		public override void TestGetBusinessObjectPKColumn()
		{
			base.TestGetBusinessObjectPKColumn();

			AssertEquals(ShipmentDeclarationSchema.PersistentBizOPK, FilterStripGridModule.GetBusinessObjectPKColumn(null));
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
						new ColumnDetailsForTest("Job#", i++, typeof(ZHyperLinkColumn)),
						new ColumnDetailsForTest("Branch", i++, typeof(ZFindBoxColumn)),
						new ColumnDetailsForTest("Type", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Transport", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Job Number", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Vessel", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Voyage/Flight", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Date Of Arrival", i++, typeof(ZDateTimeColumn)),
						new ColumnDetailsForTest("Origin", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Final Dest.", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("House Bill", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Supplier", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Importer", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Country/Region", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Importer Code", i++, typeof(ZFindBoxColumn)),
						new ColumnDetailsForTest("Supplier Code", i++, typeof(ZFindBoxColumn)),
						new ColumnDetailsForTest("Agents Ref", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Container Mode", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Containers Count", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Date of First Arrival", i++, typeof(ZDateTimeColumn)),
						new ColumnDetailsForTest("EFT Mode", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Entry Auth. Date", i++, typeof(ZDateTimeColumn)),
						new ColumnDetailsForTest("Export Date", i++, typeof(ZDateTimeColumn)),
						new ColumnDetailsForTest("Export Goods Type", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Goods Description", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Master Bill", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Sub Type", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Owner's Ref#", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Arrival", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("First Arrival", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Loading", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Total Packs", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Pack Type", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Entry Number", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Earliest Customs Entry Issue Date", i++, typeof(ZDateTimeColumn)),
						new ColumnDetailsForTest("Order Ref#", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Volume", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Volume UQ", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Weight", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Weight UQ", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Message Status", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Date Created", i++, typeof(ZDateTimeColumn)),
						new ColumnDetailsForTest("Broker", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Containers", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("TEU", i++, typeof(ZCalcEditColumn)),
						new ColumnDetailsForTest("Entry Submitted", i++, typeof(ZDateTimeColumn)),
						new ColumnDetailsForTest("Entry Status", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Entry Status Desc.", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Cargo Status", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Cargo Status Description", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Last Milestone Desc.", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Last Milestone Date", i++, typeof(ZDateTimeColumn)),
						new ColumnDetailsForTest("Next Milestone Desc.", i++, typeof(ZTextEditColumn)),
						new ColumnDetailsForTest("Next Milestone Date", i++, typeof(ZDateTimeColumn))
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
					result.Add(module.AllColumns["Branch"]);
					result.Add(module.AllColumns["Type"]);
					result.Add(module.AllColumns["Transport"]);
					result.Add(module.AllColumns["Job Number"]);
					result.Add(module.AllColumns["Vessel"]);
					result.Add(module.AllColumns["Voyage/Flight"]);
					result.Add(module.AllColumns["Date Of Arrival"]);
					result.Add(module.AllColumns["Origin"]);
					result.Add(module.AllColumns["Final Dest."]);
					result.Add(module.AllColumns["House Bill"]);
					result.Add(module.AllColumns["Supplier"]);
					result.Add(module.AllColumns["Importer"]);
					result.Add(module.AllColumns["Country/Region"]);
				}

				return result.ToArray();
			}
		}

		protected override DataGridColumn[] ExpectedRequiredGridColumns
		{
			get
			{
				var result = new List<DataGridColumn>();

				using (var module = FilterGridModule as TrackingShipmentsModule)
				{
					result.Add(module.AllColumns["Job#"]);
				}
				return result.ToArray();
			}
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(ShipmentDeclarationSchema.ETA.Name, ListSortDirection.Descending) };

		protected override ListSortDirection ExpectedDefaultSortOrder
		{
			get { return ListSortDirection.Descending; }
		}

		#endregion
	}
}
