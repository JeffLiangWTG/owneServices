using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Module;
using Enterprise.Customs.SG.Registry;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs.ASYCUDA.SGAccess;
using CusEntryHeader = Enterprise.Customs.SG.V4.Business.CusEntryHeader;

namespace Enterprise.Customs.SG.V4.Module.Testing
{
	[TestedType(typeof(JobDeclarationFilterBusinessObject))]
	sealed class JobDeclarationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestSetExternalDefaults()
		{
			IFilterBusinessObjectDefaultsProvider collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var provider = new DeclarationDefaultFilterProvider();
			provider.TransportMode = "1";
			provider.ContainerMode = "1";
			provider.OriginPort = "1";
			provider.DestinationPort = "1";
			provider.LoadPort = "1";
			provider.DischargePort = "1";
			provider.SetDefaultFilters(collection);
			var filterStripBizO = new JobDeclarationFilterBusinessObjectForTest();
			Assert(!filterStripBizO.ContainsDefaults);
			var defaults = collection.FilterBusinessObjectDefaults;
			filterStripBizO.SetExternalDefaults(defaults);
			Assert(filterStripBizO.ContainsDefaults);
			AssertNull(filterStripBizO[DeclarationDefaultFilterProvider.Constants.TransportMode]);
			AssertNull(filterStripBizO[DeclarationDefaultFilterProvider.Constants.ContainerMode]);
			AssertNotNull(filterStripBizO[JobDeclarationFilterBusinessObject.Descriptions.InwardTransportMode]);
			AssertNotNull(filterStripBizO[JobDeclarationFilterBusinessObject.Descriptions.PackingType]);
			AssertNotNull(filterStripBizO[DeclarationDefaultFilterProvider.Constants.LoadDischarge]);
			AssertNotNull(filterStripBizO[DeclarationDefaultFilterProvider.Constants.OriginDestination]);
		}

		public void TestApplicationCodeFilterCaption()
		{
			AssertEquals("Message Type", new JobDeclarationFilterBusinessObject().ApplicationCodeFilterCaption);
		}

		public void TestServiceLevelFilter()
		{
			var filter = new JobDeclarationFilterBusinessObject();
			AssertNotNull(filter[DeclarationFilterConstants.ServiceLevel]);

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_RS_NKServiceLevel = "STD";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_RS_NKServiceLevel = "DEF";

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_RS_NKServiceLevel = "STD";
			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_JS = shipment1.PK;
			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_RS_NKServiceLevel = "DEF";
			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_JS = shipment2.PK;
			Factory.Save();

			var serviceLevelModeFilter = (ModuleNkFilter)filter[DeclarationFilterConstants.ServiceLevel];
			serviceLevelModeFilter.Property = "DEF";
			serviceLevelModeFilter.IsActive = true;
			serviceLevelModeFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			var declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();

			AssertEquals(2, declarations.Count);
			AssertEquals("declaration1 is not in Collection", false, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is in Collection", true, declarations.Contains(declaration2.PK));
			AssertEquals("declaration3 is not in Collection", false, declarations.Contains(declaration3.PK));
			AssertEquals("declaration4 is in Collection", true, declarations.Contains(declaration4.PK));

			serviceLevelModeFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals(2, declarations.Count);
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));
			AssertEquals("declaration3 is in Collection", true, declarations.Contains(declaration3.PK));
			AssertEquals("declaration4 is not in Collection", false, declarations.Contains(declaration4.PK));

			serviceLevelModeFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals(0, declarations.Count);

			serviceLevelModeFilter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals(4, declarations.Count);
		}

		public void TestFindByMessageURN()
		{
			var sgCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, Core.Constants.CountryCodes.Singapore));
			using (Environment.DisposableEnvironment.ForBranch(sgCompany.FirstActiveBranch.PK.ToGuid()))
			{
				CreateDeclarationWithMessage("123");
				CreateDeclarationWithMessage("123456");
				CreateDeclarationWithMessage("123456798");
				CreateDeclarationWithMessage("abc");
				var filter = new JobDeclarationFilterBusinessObject();
				filter.CountryCode = sgCompany.GC_RN_NKCountryCode;
				var uRN = (ModuleNumberFilter)filter["Message URN#"];
				uRN.IsActive = true;
				uRN.Property = "123";
				uRN.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
				declarations.Load(filter.Filter);
				AssertEquals(3, declarations.Count);
				uRN.SqlComparisonOperator = SQLComparisonOperator.Equal;
				declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
				declarations.Load(filter.Filter);
				AssertEquals(1, declarations.Count);
				uRN.Property = "345";
				uRN.SqlComparisonOperator = SQLComparisonOperator.Contains;
				declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
				declarations.Load(filter.Filter);
				AssertEquals(2, declarations.Count);
				uRN.Property = "abc";
				uRN.SqlComparisonOperator = SQLComparisonOperator.Contains;
				declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
				declarations.Load(filter.Filter);
				AssertEquals(1, declarations.Count);
			}
		}

		public void TestFlightVoyageVesselFilter()
		{
			var filter = new JobDeclarationFilterBusinessObject();
			AssertNotNull(filter["Inward Flight/Voyage # and Vessel"]);
			var declaration1 = Factory.New<BaseJobDeclaration>();
			declaration1.JE_VoyageFlightNo = "12345";
			declaration1.JE_VesselName = "HEEYAAAA";
			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_VoyageFlightNo = "6789";
			declaration2.JE_VesselName = "Flying Dutchman";
			var declaration3 = Factory.New<BaseJobDeclaration>();
			declaration3.JE_VoyageFlightNo = "25874";
			declaration3.JE_VesselName = "Boaty McBoatface";
			Factory.Save();
			var voyageAndVesselFilter = (ModuleTextAndNkFilter)filter["Inward Flight/Voyage # and Vessel"];
			voyageAndVesselFilter.NkProperty = "HEEYAAAA";
			voyageAndVesselFilter.IsActive = true;
			var declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));
			AssertEquals("declaration3 is not in Collection", false, declarations.Contains(declaration3.PK));
			voyageAndVesselFilter.Property = "12345";
			voyageAndVesselFilter.NkProperty = "HEEYAAAA";
			voyageAndVesselFilter.IsActive = true;
			declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));
			AssertEquals("declaration3 is not in Collection", false, declarations.Contains(declaration3.PK));
			voyageAndVesselFilter.Property = "12345";
			voyageAndVesselFilter.NkProperty = "";
			voyageAndVesselFilter.IsActive = true;
			declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));
			AssertEquals("declaration3 is not in Collection", false, declarations.Contains(declaration3.PK));
			voyageAndVesselFilter.Property = "";
			voyageAndVesselFilter.NkProperty = "Flying Dutchman";
			voyageAndVesselFilter.IsActive = true;
			declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is not in Collection", false, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is in Collection", true, declarations.Contains(declaration2.PK));
			AssertEquals("declaration3 is not in Collection", false, declarations.Contains(declaration3.PK));
			voyageAndVesselFilter.Property = "2";
			voyageAndVesselFilter.NkProperty = "";
			voyageAndVesselFilter.IsActive = true;
			voyageAndVesselFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));
			AssertEquals("declaration3 is in Collection", true, declarations.Contains(declaration3.PK));
			voyageAndVesselFilter.Property = "";
			voyageAndVesselFilter.NkProperty = "";
			voyageAndVesselFilter.IsActive = true;
			voyageAndVesselFilter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is in Collection", true, declarations.Contains(declaration2.PK));
			AssertEquals("declaration3 is in Collection", true, declarations.Contains(declaration3.PK));
			voyageAndVesselFilter.Property = "";
			voyageAndVesselFilter.NkProperty = "";
			voyageAndVesselFilter.IsActive = true;
			voyageAndVesselFilter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is not in Collection", false, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));
			AssertEquals("declaration3 is not in Collection", false, declarations.Contains(declaration3.PK));
		}

		public void TestLookups()
		{
			JobDeclarationFilterBusinessObject filterBizObj = new JobDeclarationFilterBusinessObject();
			AssertEquals("Lookups of correct type", typeof(JobDeclarationFilterLookups), filterBizObj.Lookups.GetType());
		}

		public void TestRelatedManifestCustomsStatusEXP()
		{
			using (SGCustomsDataRegistry.Instance.ACCESSEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var manifestIMP = SetupGlobalManifestAndBill(Core.Constants.CountryCodes.Singapore, "CR", "MB001", "HB001");
				Factory.LoadTop1<IAsycudaBill>(new ZQuery(AsycudaBillSchema.ABL_BillNumber, "HB001")).ABL_ShipmentType = "IMP";
				var manifestEXP = SetupGlobalManifestAndBill(Core.Constants.CountryCodes.Singapore, "CR", "MB002", "HB002");
				manifestEXP.AMA_JobReference = "SG2";
				Factory.LoadTop1<IAsycudaBill>(new ZQuery(AsycudaBillSchema.ABL_BillNumber, "HB002")).ABL_ShipmentType = "EXP";
				Factory.Save();
				var jobIMP = Factory.New<JobDeclaration>();
				jobIMP.JE_DeclarationReference = "SG00012349";
				jobIMP.JE_MasterBill = "MB001";
				jobIMP.JE_HouseBill = "HB001";
				var jobEXP = Factory.New<JobDeclaration>();
				jobEXP.JE_DeclarationReference = "SG00012350";
				jobEXP.SG_OutwardMAWB = "MB002";
				jobEXP.SG_OutwardHAWB = "HB002";
				Factory.Save();
				var filter = new JobDeclarationFilterBusinessObject();
				var manifestFilter = (ModuleTextFilter)filter["Related Manifest Bill Customs Status"];
				manifestFilter.IsActive = true;
				manifestFilter.Property = "CR";
				Assert("IMP bills should be matched.", jobIMP.MatchesFilter(filter.Filter));
				Assert("EXP bills should be matched.", jobEXP.MatchesFilter(filter.Filter));
			}
		}

		public void TestRelatedManifestCustomsStatusNotSent()
		{
			using (SGCustomsDataRegistry.Instance.ACCESSEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var manifestHeader = SetupGlobalManifestAndBill(Core.Constants.CountryCodes.Singapore, ZString.Empty, "08109191442", "AA1235579412");
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_DeclarationReference = "SG00012349";
				declaration.JE_MasterBill = "08109191442";
				declaration.JE_HouseBill = "AA1235579412";
				Factory.Save();
				var jobDecfilter = new JobDeclarationFilterBusinessObject();
				var manifestFilter = (ModuleTextFilter)jobDecfilter["Related Manifest Bill Customs Status"];
				manifestFilter.IsActive = true;
				manifestFilter.Property = Common.SG.GlobalManifestStatusList.Codes.NoStatus;
				var declarations = new BaseJobDeclarationCollection(Factory, jobDecfilter.Filter);
				declarations.Load();
				AssertEquals(1, declarations.Count);
				Assert(object.ReferenceEquals(declarations[0], declaration));
			}
		}

		public void TestRelatedManifestCustomsStatusOnlyOneMatchedManifest()
		{
			using (SGCustomsDataRegistry.Instance.ACCESSEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var manifestHeader = SetupGlobalManifestAndBill(Core.Constants.CountryCodes.Singapore, Common.SG.GlobalManifestStatusList.Codes.Clear, "08109191442", "AA1235579412");
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_DeclarationReference = "SG00012349";
				declaration.JE_MasterBill = "08109191442";
				declaration.JE_HouseBill = "AA1235579412";
				Factory.Save();
				var jobDecfilter = new JobDeclarationFilterBusinessObject();
				var manifestFilter = (ModuleTextFilter)jobDecfilter["Related Manifest Bill Customs Status"];
				manifestFilter.IsActive = true;
				manifestFilter.Property = Common.SG.GlobalManifestStatusList.Codes.Cancelled;
				var declarations = new BaseJobDeclarationCollection(Factory, jobDecfilter.Filter);
				declarations.Load();
				AssertEquals(0, declarations.Count);
				manifestFilter.Property = Common.SG.GlobalManifestStatusList.Codes.Clear;
				declarations = new BaseJobDeclarationCollection(Factory, jobDecfilter.Filter);
				declarations.Load();
				AssertEquals(1, declarations.Count);
				var manifestBillCountry2 = AddBillToHeader(manifestHeader, Common.SG.GlobalManifestStatusList.Codes.Cancelled);
				var declaration2 = Factory.New<JobDeclaration>();
				declaration2.JE_DeclarationReference = "SG00012348";
				Factory.Save();
				manifestFilter.Property = ZString.Empty;
				declarations = new BaseJobDeclarationCollection(Factory, jobDecfilter.Filter);
				declarations.Load();
				AssertEquals(2, declarations.Count);
			}
		}

		public void TestRelatedManifestCustomsStatusIsReturnedForMultipleMatchedManifest()
		{
			using (SGCustomsDataRegistry.Instance.ACCESSEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				SetupGlobalManifestAndBill(Core.Constants.CountryCodes.Singapore, Common.SG.GlobalManifestStatusList.Codes.InspectionRequired, "08109191442", "AA1235579412");
				SetupGlobalManifestAndBill(Core.Constants.CountryCodes.Singapore, Common.SG.GlobalManifestStatusList.Codes.Cancelled, "08109191442", "AA1235579412").AMA_JobReference = "SG2";
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_DeclarationReference = "SG00012349";
				declaration.JE_MasterBill = "08109191442";
				declaration.JE_HouseBill = "AA1235579412";
				Factory.Save();
				var jobDecfilter = new JobDeclarationFilterBusinessObject();
				var manifestFilter = (ModuleTextFilter)jobDecfilter["Related Manifest Bill Customs Status"];
				manifestFilter.IsActive = true;
				manifestFilter.Property = Common.SG.GlobalManifestStatusList.Codes.Clear;
				var declarations = new BaseJobDeclarationCollection(Factory, jobDecfilter.Filter);
				declarations.Load();
				AssertEquals(0, declarations.Count);
				manifestFilter.Property = Common.SG.GlobalManifestStatusList.Codes.InspectionRequired;
				declarations = new BaseJobDeclarationCollection(Factory, jobDecfilter.Filter);
				declarations.Load();
				AssertEquals(1, declarations.Count);
				Assert(object.ReferenceEquals(declarations[0], declaration));
				manifestFilter.Property = Common.SG.GlobalManifestStatusList.Codes.Cancelled;
				declarations = new BaseJobDeclarationCollection(Factory, jobDecfilter.Filter);
				declarations.Load();
				AssertEquals(1, declarations.Count);
				Assert(object.ReferenceEquals(declarations[0], declaration));
			}
		}

		public void TestOnlySGManifestsAreReturned()
		{
			using (SGCustomsDataRegistry.Instance.ACCESSEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				SetupGlobalManifestAndBill(Core.Constants.CountryCodes.Singapore, Common.SG.GlobalManifestStatusList.Codes.Clear, "08109191442", "AA1235579412");
				var manifestHeaderZA = SetupGlobalManifestAndBill(Core.Constants.CountryCodes.SouthAfrica, Common.SG.GlobalManifestStatusList.Codes.Clear, "08109191442", "AA1235579413");
				manifestHeaderZA.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
				var declaration1 = Factory.New<JobDeclaration>();
				declaration1.JE_DeclarationReference = "SG00099997";
				declaration1.JE_MasterBill = "08109191442";
				declaration1.JE_HouseBill = "AA1235579412";
				var declaration2 = Factory.New<JobDeclaration>();
				declaration2.JE_DeclarationReference = "SG00099998";
				declaration2.JE_MasterBill = "08109191442";
				declaration2.JE_HouseBill = "AA1235579413";
				Factory.Save();
				var jobDecfilter = new JobDeclarationFilterBusinessObject();
				var manifestFilter = (ModuleTextFilter)jobDecfilter["Related Manifest Bill Customs Status"];
				manifestFilter.IsActive = true;
				manifestFilter.Property = Common.SG.GlobalManifestStatusList.Codes.Cancelled;
				var newFactory = new BusinessObjectFactory();
				var declarations = new BaseJobDeclarationCollection(newFactory, jobDecfilter.Filter);
				declarations.Load();
				AssertEquals(0, declarations.Count);
				manifestFilter.Property = Common.SG.GlobalManifestStatusList.Codes.Clear;
				declarations = new BaseJobDeclarationCollection(newFactory, jobDecfilter.Filter);
				declarations.Load();
				AssertEquals(1, declarations.Count);
				AssertEquals(declaration1.PK, declarations[0].PK);
			}
		}

		public void TestManifestCustomsStatusOnlyVisibleWhenRegistryEnabled()
		{
			using (SGCustomsDataRegistry.Instance.ACCESSEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var jobDecfilter = new JobDeclarationFilterBusinessObject();
				AssertNull(jobDecfilter["Related Manifest Bill Customs Status"]);
			}

			using (SGCustomsDataRegistry.Instance.ACCESSEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var jobDecfilter = new JobDeclarationFilterBusinessObject();
				AssertNotNull(jobDecfilter["Related Manifest Bill Customs Status"]);
			}
		}

		public void TestMAWBRecyclePeriodCorrectlyFiltersJobDecs()
		{
			using (SGCustomsDataRegistry.Instance.ACCESSEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				SetupGlobalManifestAndBill(Core.Constants.CountryCodes.Singapore, Common.SG.GlobalManifestStatusList.Codes.InspectionRequired, "08109191442", "AA1235579412");
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_DeclarationReference = "SG00012349";
				declaration.JE_MasterBill = "08109191442";
				declaration.JE_HouseBill = "AA1235579412";
				declaration.JE_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMonths(-11);
				Factory.Save();
				using (FreightDataRegistry.Instance.MAWBRecyclePeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 10))
				{
					var jobDecfilter = new JobDeclarationFilterBusinessObject();
					var manifestFilter = (ModuleTextFilter)jobDecfilter["Related Manifest Bill Customs Status"];
					manifestFilter.IsActive = true;
					manifestFilter.Property = Common.SG.GlobalManifestStatusList.Codes.InspectionRequired;
					var declarations = new BaseJobDeclarationCollection(Factory, jobDecfilter.Filter);
					declarations.Load();
					AssertEquals(0, declarations.Count);
				}
			}
		}

		public void TestOutwardHAWBAndMAWBFilter()
		{
			using (SGCustomsDataRegistry.Instance.ACCESSEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_DeclarationReference = "SG00012349";
				declaration.SG_OutwardHAWB = "AA1235579412";
				declaration.SG_OutwardMAWB = "08109191442";
				Factory.Save();
				var jobDecfilter = new JobDeclarationFilterBusinessObject();
				var hawbFilter = (ModuleTextFilter)jobDecfilter["Outward House Bill"];
				var mawbFilter = (ModuleTextFilter)jobDecfilter["Outward Master Bill"];
				hawbFilter.IsActive = true;
				mawbFilter.IsActive = false;
				hawbFilter.Property = "AA002688";
				var declarations = new BaseJobDeclarationCollection(Factory, jobDecfilter.Filter);
				declarations.Load();
				AssertEquals(0, declarations.Count);
				hawbFilter.Property = "AA1235579412";
				declarations = new BaseJobDeclarationCollection(Factory, jobDecfilter.Filter);
				declarations.Load();
				AssertEquals(1, declarations.Count);
				mawbFilter.IsActive = true;
				mawbFilter.Property = "AA002688";
				hawbFilter.IsActive = false;
				declarations = new BaseJobDeclarationCollection(Factory, jobDecfilter.Filter);
				declarations.Load();
				AssertEquals(0, declarations.Count);
				mawbFilter.Property = "08109191442";
				declarations = new BaseJobDeclarationCollection(Factory, jobDecfilter.Filter);
				declarations.Load();
				AssertEquals(1, declarations.Count);
			}
		}

		public void TestOutwardTransportModeFilter()
		{
			using (SGCustomsDataRegistry.Instance.ACCESSEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_DeclarationReference = "SG00012349";
				declaration.SG_OutwardTransportMode = "SEA";
				Factory.Save();
				var jobDecfilter = new JobDeclarationFilterBusinessObject();
				var filter = (ModuleTextFilter)jobDecfilter["Outward Transport Mode"];
				filter.IsActive = true;
				filter.Property = "AIR";
				var declarations = new BaseJobDeclarationCollection(Factory, jobDecfilter.Filter);
				declarations.Load();
				AssertEquals(0, declarations.Count);
				filter.Property = "SEA";
				declarations = new BaseJobDeclarationCollection(Factory, jobDecfilter.Filter);
				declarations.Load();
				AssertEquals(1, declarations.Count);
			}
		}

		public void TestOutwardShippingLineForwarderFilter()
		{
			using (SGCustomsDataRegistry.Instance.ACCESSEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var carrierAgent = Factory.NewWithValidTestData<OrgHeader>();
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_DeclarationReference = "SG00012349";
				declaration.OutwardShippingLineForwarderDocAddress.E2_OA_Address = carrierAgent.MainAddress.PK;
				Factory.Save();
				var jobDecfilter = new JobDeclarationFilterBusinessObject();
				var filter = (ModuleGuidFilter)jobDecfilter["Outward Shipping Line/Forwarder"];
				filter.IsActive = true;
				filter.Property = ZGuid.BrettsGuid;
				var declarations = new BaseJobDeclarationCollection(Factory, jobDecfilter.Filter);
				declarations.Load();
				AssertEquals(0, declarations.Count);
				filter.Property = carrierAgent.PK;
				declarations = new BaseJobDeclarationCollection(Factory, jobDecfilter.Filter);
				declarations.Load();
				AssertEquals(1, declarations.Count);
			}
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new JobDeclarationFilterBusinessObject();

		Integration.Customs.ASYCUDA.IAsycudaManifestHeader SetupGlobalManifestAndBill(ZString countryCode, ZString billStatus, ZString masterBill, ZString houseBill)
		{
			Integration.Customs.ASYCUDA.IAsycudaManifestHeader manifestHeader;
			if (countryCode == Core.Constants.CountryCodes.Singapore)
			{
				manifestHeader = Factory.New<IAsycudaManifestHeader>();
				((BusinessObject)manifestHeader).FillWithValidTestData();
				manifestHeader.AMA_JobReference = "SG1";
			}
			else if (countryCode == Core.Constants.CountryCodes.SouthAfrica)
			{
				manifestHeader = Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
				((BusinessObject)manifestHeader).FillWithValidTestData();
				manifestHeader.AMA_JobReference = "ZA1";
			}
			else
			{
				throw new NotImplementedException("The other country codes are not implemented here yet.");
			}

			if (!masterBill.IsEmpty)
			{
				manifestHeader.AMA_MasterBill = masterBill;
				var masterBillBO = manifestHeader.MasterBill;
				masterBillBO.ABL_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-20);
			}

			Integration.Customs.ASYCUDA.IAsycudaBill manifestBill;
			if (countryCode == Core.Constants.CountryCodes.Singapore)
			{
				manifestBill = Factory.New<IAsycudaBill>();
			}
			else if (countryCode == Core.Constants.CountryCodes.SouthAfrica)
			{
				manifestBill = Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaBill>();
			}
			else
			{
				throw new NotImplementedException("The other country codes are not implemented here yet.");
			}

			manifestBill.ABL_AMA = manifestHeader.PK;
			manifestBill.ABL_BillNumber = houseBill;
			manifestBill.ABL_BillStatus = billStatus;
			return manifestHeader;
		}

		IAsycudaBill AddBillToHeader(Integration.Customs.ASYCUDA.IAsycudaManifestHeader manifestHeader, ZString billMessageStatus)
		{
			var manifestBill = Factory.New<IAsycudaBill>();
			manifestBill.ABL_AMA = manifestHeader.PK;
			var manifestBillCountry = manifestBill;
			manifestBillCountry.ABL_MessageStatus = billMessageStatus;
			return manifestBillCountry;
		}

		void CreateDeclarationWithMessage(string uRN)
		{
			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_GB = Enterprise.MasterFiles.Business.GlbBranch.CurrentBranch.PK;
			CusEntryHeader entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			CUSDECEDIMessage message = Factory.New<CUSDECEDIMessage>();
			entryHeader.Messages.Add(message);
			message.EM_MessageText = "TEST+++<<MSGNO PLACEHOLDER>>+++TEST";
			Factory.Save();
			message.EM_ApplicationReference = uRN;
			Factory.Save();
		}
	}

	sealed class JobDeclarationFilterBusinessObjectForTest : JobDeclarationFilterBusinessObject
	{
		public void AddModuleFilterForTest(ModuleFilter filter)
		{
			userAddedModuleFilters.Add(filter);
			// so that next access will get module filters again
			typeof(FilterStripBusinessObject).GetField("fModuleFilters", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, null);
		}

		readonly List<ModuleFilter> userAddedModuleFilters = new List<ModuleFilter>();
	}
}
