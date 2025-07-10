using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(EntryHeaderFilterBusinessObject))]
	sealed class EntryHeaderFilterBusinessObjectTestForBaseOnly : FilterStripBusinessObjectTestCase
	{
		public void TestAdditionalReferenceFilter()
		{
			var (declaration1, _, entry1, _) = CreateDeclarationsAndEntries();
			var cusEntryNum1 = declaration1.AdditionalReferenceNumbers.AddNew();
			cusEntryNum1.CE_EntryNum = "963258";
			cusEntryNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			cusEntryNum1.CE_EntryType = "ABC";
			Factory.Save();
			var filterBusinessObject = new EntryHeaderFilterBusinessObject();
			var filter = (ReferenceNumberFilter)filterBusinessObject[DeclarationFilterConstants.NumberFilterTypes.AdditionalReferenceNumber];
			filter.IsActive = true;
			CombineAssertions(() =>
			{
				AssertEquals("find for empty filter", true, entry1.MatchesFilter(filterBusinessObject.Filter));
				filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
				filter.Property = "258";
				filter.Country = Core.Constants.CountryCodes.SouthAfrica;
				filter.Type = "ABC";
				AssertEquals("find when matching", true, entry1.MatchesFilter(filterBusinessObject.Filter));
				filter.Property = "123";
				AssertEquals("No result for number:123", false, entry1.MatchesFilter(filterBusinessObject.Filter));
				filter.Property = "258";
				filter.Type = "CCC";
				AssertEquals("No result for type:CCC", false, entry1.MatchesFilter(filterBusinessObject.Filter));
				filter.Type = "ABC";
				filter.Country = Core.Constants.CountryCodes.Afghanistan;
				AssertEquals("No result for country:AF", false, entry1.MatchesFilter(filterBusinessObject.Filter));
			});
		}

		public void TestBranchFilter()
		{
			var companyPK = GlbCompany.CurrentCompany.PK;
			var branch = GlbBranch.CurrentBranch;
			branch.GB_GC = companyPK;
			var company2 = Factory.New<GlbCompany>();
			var branch2 = company2.Branches.AddNew();
			(var declaration1, var declaration2, var entry1, var entry2) = CreateDeclarationsAndEntries();
			declaration1.JE_GB = branch.PK;
			declaration2.JE_GB = branch2.PK;
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleGuidFilter)filterObj[EntryHeaderFilterBusinessObject.Constants.Branch];
			filter.IsActive = true;
			filter.Property = branch.PK;
			AssertEquals(true, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entry2.MatchesFilter(filterObj.Filter));
			filter.Property = branch2.PK;
			AssertEquals(false, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entry2.MatchesFilter(filterObj.Filter));
		}

		public void TestMessageTypeFilter()
		{
			(var _, var _, var entry1, var entry2) = CreateDeclarationsAndEntries();
			entry1.CH_MessageType = JobMessageTypeList.Codes.Import;
			entry2.CH_MessageType = JobMessageTypeList.Codes.Export;
			Factory.Save();
			AssertModuleTextFilterMatchResult(EntryHeaderFilterBusinessObject.Constants.MessageType, entry1, entry2, JobMessageTypeList.Codes.Import, JobMessageTypeList.Codes.Export);
		}

		public void TestEntryStatusFilter()
		{
			(var _, var _, var entry1, var entry2) = CreateDeclarationsAndEntries();
			entry1.CH_EntryStatus = "ACK";
			entry2.CH_EntryStatus = "ERR";
			Factory.Save();
			AssertModuleTextFilterMatchResult(EntryHeaderFilterBusinessObject.Constants.EntryStatus, entry1, entry2, "ACK", "ERR");

			var filterBusinessObject = new EntryHeaderFilterBusinessObject();
			var entryStatusFilter = (ModuleTextFilter)filterBusinessObject[EntryHeaderFilterBusinessObject.Constants.EntryStatus];
			var comparisonOperatorList = entryStatusFilter.ComparisonOperator_List;
			AssertEquals(4, comparisonOperatorList.Count);
			AssertEquals(true, comparisonOperatorList.ContainsCode(ModuleTextFilter.ComparisonConstants.Exact));
			AssertEquals(true, comparisonOperatorList.ContainsCode(ModuleTextFilter.ComparisonConstants.NotEqual));
			AssertEquals(true, comparisonOperatorList.ContainsCode(ModuleTextFilter.ComparisonConstants.IsBlank));
			AssertEquals(true, comparisonOperatorList.ContainsCode(ModuleTextFilter.ComparisonConstants.IsNotBlank));
			AssertEquals(false, comparisonOperatorList.ContainsCode(ModuleTextFilter.ComparisonConstants.StartsWith));
			AssertEquals(false, comparisonOperatorList.ContainsCode(ModuleTextFilter.ComparisonConstants.NotStartsWith));
			AssertEquals(false, comparisonOperatorList.ContainsCode(ModuleTextFilter.ComparisonConstants.Contains));
			AssertEquals(false, comparisonOperatorList.ContainsCode(ModuleTextFilter.ComparisonConstants.NotContain));
		}

		public void TestImporterSupplierFilter()
		{
			(var orgHeader1, var orgHeader2) = CreateOrgheaders("AAA", "BBB");
			(var declaration1, var declaration2, var entry1, var entry2) = CreateDeclarationsAndEntries();
			declaration1.JE_OH_Importer = orgHeader1.PK;
			declaration2.JE_OH_Supplier = orgHeader2.PK;
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleGuidsFilter)filterObj[DeclarationFilterConstants.OrgFilterTypes.ImporterSupplier];
			filter.IsActive = true;
			filter.Property1 = orgHeader1.PK;
			filter.Property2 = ZGuid.Empty;
			AssertEquals(true, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entry2.MatchesFilter(filterObj.Filter));
			filter.Property1 = ZGuid.Empty;
			filter.Property2 = orgHeader2.PK;
			AssertEquals(false, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entry2.MatchesFilter(filterObj.Filter));
			filter.Property1 = orgHeader1.PK;
			filter.Property2 = orgHeader2.PK;
			AssertEquals(false, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entry2.MatchesFilter(filterObj.Filter));
		}

		public void TestControllingAgentFilter()
		{
			(var orgHeader1, var orgHeader2) = CreateOrgheaders("AAA", "BBB");
			(var declaration1, var declaration2, var entry1, var entry2) = CreateDeclarationsAndEntries();
			declaration1.JE_OH_ControllingAgent = orgHeader1.PK;
			declaration2.JE_OH_ControllingAgent = orgHeader2.PK;
			Factory.Save();
			AssertModuleGuidFilterMatchResult(DeclarationFilterConstants.OrgFilterTypes.ControllingAgent, entry1, entry2, orgHeader1.PK, orgHeader2.PK);
		}

		public void TestControllingCustomerFilter()
		{
			(var orgHeader1, var orgHeader2) = CreateOrgheaders("AAA", "BBB");
			(var declaration1, var declaration2, var entry1, var entry2) = CreateDeclarationsAndEntries();
			declaration1.JE_OH_ControllingCustomer = orgHeader1.PK;
			declaration2.JE_OH_ControllingCustomer = orgHeader2.PK;
			Factory.Save();
			AssertModuleGuidFilterMatchResult(DeclarationFilterConstants.OrgFilterTypes.ControllingCustomer, entry1, entry2, orgHeader1.PK, orgHeader2.PK);
		}

		public void TestDeclarantfilter()
		{
			(var orgHeader1, var orgHeader2) = CreateOrgheaders("AAA", "BBB");
			(var declaration1, var declaration2, var entry1, var entry2) = CreateDeclarationsAndEntries();
			declaration1.JE_OA_DeclarantAddress = orgHeader1.MainAddress.PK;
			declaration2.JE_OA_DeclarantAddress = orgHeader2.MainAddress.PK;
			Factory.Save();
			AssertModuleGuidFilterMatchResult(EntryHeaderFilterBusinessObject.Constants.Declarant, entry1, entry2, orgHeader1.MainAddress.PK, orgHeader2.MainAddress.PK);
		}

		public void TestAgentReferenceFilters()
		{
			(var declaration1, var declaration2, var entry1, var entry2) = CreateDeclarationsAndEntries();
			declaration1.JE_AgentsReference = "Test 1";
			declaration2.JE_AgentsReference = "Test 2";
			Factory.Save();
			AssertModuleTextFilterMatchResult(DeclarationFilterConstants.NumberFilterTypes.AgentsReference, entry1, entry2, "Test 1", "Test 2");
		}

		public void TestShipmentTypeFilter()
		{
			(var declaration1, var declaration2, var entry1, var entry2) = CreateDeclarationsAndEntries();
			declaration1.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration2.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			Factory.Save();
			AssertModuleTextFilterMatchResult(DeclarationFilterConstants.ShipmentType, entry1, entry2, Common.Shared.SharedJobMessageTypeList.Codes.Import, Common.Shared.SharedJobMessageTypeList.Codes.Export);
		}

		public void TestTransportModeFilter()
		{
			(var declaration1, var declaration2, var entry1, var entry2) = CreateDeclarationsAndEntries();
			declaration1.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration2.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Factory.Save();
			AssertModuleTextFilterMatchResult(DeclarationFilterConstants.TransportMode, entry1, entry2, Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Sea);
		}

		public void TestFlightVoyageVesselFilter()
		{
			(var declaration1, var declaration2, var entry1, var entry2) = CreateDeclarationsAndEntries();
			declaration1.JE_TransportMode = "SEA";
			declaration1.JE_VoyageFlightNo = "12345";
			declaration1.JE_VesselName = "The Spitfire";
			declaration2.JE_TransportMode = "SEA";
			declaration2.JE_VoyageFlightNo = "6789";
			declaration2.JE_VesselName = "Flying Dutchman";
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject();
			var voyageAndVesselFilter = (ModuleTextAndNkFilter)filterObj[DeclarationFilterConstants.FlightVoyageVessel];
			voyageAndVesselFilter.NkProperty = "The Spitfire";
			voyageAndVesselFilter.IsActive = true;
			AssertEquals(true, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entry2.MatchesFilter(filterObj.Filter));
			voyageAndVesselFilter.Property = "XXX";
			AssertEquals(false, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entry2.MatchesFilter(filterObj.Filter));
			voyageAndVesselFilter.Property = "12345";
			AssertEquals(true, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entry2.MatchesFilter(filterObj.Filter));
			voyageAndVesselFilter.NkProperty = "";
			AssertEquals(true, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entry2.MatchesFilter(filterObj.Filter));
			voyageAndVesselFilter.Property = "";
			AssertEquals(true, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entry2.MatchesFilter(filterObj.Filter));
			voyageAndVesselFilter.Property = "2";
			voyageAndVesselFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			AssertEquals(true, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entry2.MatchesFilter(filterObj.Filter));
			voyageAndVesselFilter.Property = "";
			voyageAndVesselFilter.NkProperty = "ch";
			AssertEquals(false, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entry2.MatchesFilter(filterObj.Filter));
			voyageAndVesselFilter.Property = "";
			voyageAndVesselFilter.NkProperty = "";
			voyageAndVesselFilter.IsActive = true;
			voyageAndVesselFilter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			AssertEquals(true, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entry2.MatchesFilter(filterObj.Filter));
			voyageAndVesselFilter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			AssertEquals(false, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entry2.MatchesFilter(filterObj.Filter));
			AssertEquals(JobDeclarationSchema.JE_VoyageFlightNo.MaxLength, voyageAndVesselFilter.MaxLength);
			AssertEquals(JobDeclarationSchema.JE_VesselName.MaxLength, voyageAndVesselFilter.NkMaxLength);
		}

		public void TestCustomsAgentfilter()
		{
			(var declaration1, var declaration2, var entry1, var entry2) = CreateDeclarationsAndEntries();
			declaration1.JE_GS_NKCusAgent = "SF1";
			declaration2.JE_GS_NKCusAgent = "SF2";
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleNkFilter)filterObj[EntryHeaderFilterBusinessObject.Constants.CustomsAgent];
			filter.IsActive = true;
			filter.Property = "SF1";
			AssertEquals(true, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entry2.MatchesFilter(filterObj.Filter));
			filter.Property = "SF2";
			AssertEquals(false, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entry2.MatchesFilter(filterObj.Filter));
			filter.Property = "XXX";
			AssertEquals(false, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entry2.MatchesFilter(filterObj.Filter));
		}

		public void TestOwnerReferencefilter()
		{
			(var declaration1, var declaration2, var entry1, var entry2) = CreateDeclarationsAndEntries();
			declaration1.JE_OwnerRef = "Ref1";
			declaration2.JE_OwnerRef = "Ref2";
			Factory.Save();
			AssertModuleTextFilterMatchResult(EntryHeaderFilterBusinessObject.Constants.OwnerReference, entry1, entry2, "Ref1", "Ref2");
		}

		public void TestMasterBillfilter()
		{
			(var declaration1, var declaration2, var entry1, var entry2) = CreateDeclarationsAndEntries();
			declaration1.JE_MasterBill = "Mster1";
			declaration2.JE_MasterBill = "Mster2";
			Factory.Save();
			AssertModuleTextFilterMatchResult(DeclarationFilterConstants.NumberFilterTypes.MasterBill, entry1, entry2, "Mster1", "Mster2");
		}

		public void TestHouseBillfilter()
		{
			(var declaration1, var declaration2, var entry1, var entry2) = CreateDeclarationsAndEntries();
			declaration1.JE_HouseBill = "House1";
			declaration2.JE_HouseBill = "House2";
			Factory.Save();
			AssertModuleTextFilterMatchResult(DeclarationFilterConstants.NumberFilterTypes.HouseBill, entry1, entry2, "House1", "House2");
		}

		public void TestOriginETDfilter()
		{
			(var declaration1, var declaration2, var entry1, var entry2) = CreateDeclarationsAndEntries();
			declaration1.JE_DateAtOrigin = ZDateTime.Today;
			declaration2.JE_DateAtOrigin = ZDateTime.Today.AddDays(1);
			Factory.Save();
			AssertModuleDateFilterMatchResult(EntryHeaderFilterBusinessObject.Constants.OriginETD, entry1, entry2, ZDateTime.Today, ZDateTime.Today.AddDays(1));
		}

		public void TestFinalDestinationETAfilter()
		{
			(var declaration1, var declaration2, var entry1, var entry2) = CreateDeclarationsAndEntries();
			declaration1.JE_DateAtFinalDestination = ZDateTime.Today;
			declaration2.JE_DateAtFinalDestination = ZDateTime.Today.AddDays(1);
			Factory.Save();
			AssertModuleDateFilterMatchResult(EntryHeaderFilterBusinessObject.Constants.FinalDestinationETA, entry1, entry2, ZDateTime.Today, ZDateTime.Today.AddDays(1));
		}

		public void TestArrivalDatefilter()
		{
			(var declaration1, var declaration2, var entry1, var entry2) = CreateDeclarationsAndEntries();
			declaration1.JE_DateOfArrival = ZDateTime.Today;
			declaration2.JE_DateOfArrival = ZDateTime.Today.AddDays(1);
			Factory.Save();
			AssertModuleDateFilterMatchResult(EntryHeaderFilterBusinessObject.Constants.ArrivalDate, entry1, entry2, ZDateTime.Today, ZDateTime.Today.AddDays(1));
		}

		public void TestExportDatefilter()
		{
			(var declaration1, var declaration2, var entry1, var entry2) = CreateDeclarationsAndEntries();
			declaration1.JE_ExportDate = ZDateTime.Today;
			declaration2.JE_ExportDate = ZDateTime.Today.AddDays(1);
			Factory.Save();
			AssertModuleDateFilterMatchResult(EntryHeaderFilterBusinessObject.Constants.ExportDate, entry1, entry2, ZDateTime.Today, ZDateTime.Today.AddDays(1));
		}

		public void TestOriginDestinationfilter()
		{
			(var declaration1, var declaration2, var entry1, var entry2) = CreateDeclarationsAndEntries();
			declaration1.JE_RL_NKOrigin = "AUSYD";
			declaration1.JE_RL_NKFinalDestination = "NZAKL";
			declaration2.JE_RL_NKOrigin = "AUMEL";
			declaration2.JE_RL_NKFinalDestination = "USCHI";
			Factory.Save();
			AssertModuleLocationFilterMatchResult(EntryHeaderFilterBusinessObject.Constants.OriginDestination, entry1, entry2, "AUSYD", "NZAKL", "AUMEL", "USCHI");
		}

		public void TestLoadingDischargefilter()
		{
			(var declaration1, var declaration2, var entry1, var entry2) = CreateDeclarationsAndEntries();
			declaration1.JE_RL_NKPortOfLoading = "AUSYD";
			declaration1.JE_RL_NKPortOfArrival = "NZAKL";
			declaration2.JE_RL_NKPortOfLoading = "AUMEL";
			declaration2.JE_RL_NKPortOfArrival = "USCHI";
			Factory.Save();
			AssertModuleLocationFilterMatchResult(EntryHeaderFilterBusinessObject.Constants.LoadingDischarge, entry1, entry2, "AUSYD", "NZAKL", "AUMEL", "USCHI");
		}

		public void TestGetWarehouseTransactionStatusQuery()
		{
			(var declaration1, var declaration2, var entry1, var entry2) = CreateDeclarationsAndEntries();
			entry1.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCanceled;
			entry2.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
			Factory.Save();
			AssertModuleTextFilterMatchResult(EntryHeaderFilterBusinessObject.Constants.WarehouseTransactionStatus, entry1, entry2, WarehouseTransactionStatusList.Codes.OutwardCanceled, WarehouseTransactionStatusList.Codes.OutwardCreated);
		}

		public void TestDeclarationType()
		{
			(var declaration1, var declaration2, var entry1, var entry2) = CreateDeclarationsAndEntries();
			var instruction1 = Factory.New<CusEntryInstruction>();
			instruction1.CEI_Style = "C1";
			instruction1.CEI_ClusterKey = 1;
			instruction1.CEI_JE = declaration1.PK;
			var instruction2 = Factory.New<CusEntryInstruction>();
			instruction2.CEI_Style = "C2";
			instruction2.CEI_ClusterKey = 2;
			instruction2.CEI_JE = declaration2.PK;
			entry1.CH_CEI_Instruction = instruction1.PK;
			entry2.CH_CEI_Instruction = instruction2.PK;
			Factory.Save();
			AssertModuleTextFilterMatchResult(EntryHeaderFilterBusinessObject.Constants.DeclarationType, entry1, entry2, "C1", "C2");
		}

		public void TestGetEntryNumberQuery()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			var entryNum1 = Factory.New<CusEntryNumber>();
			entryNum1.CE_ParentID = entry1.PK;
			entryNum1.CE_ParentTable = CusEntryHeader.Schema.TableName;
			entryNum1.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			entryNum1.CE_EntryNum = "QW123456";
			var declaration2 = Factory.New<BaseJobDeclaration>();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			var entryNum2 = Factory.New<CusEntryNumber>();
			entryNum2.CE_ParentID = entry2.PK;
			entryNum2.CE_ParentTable = CusEntryHeader.Schema.TableName;
			entryNum2.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			entryNum2.CE_EntryNum = "AS123456";
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleNumberFilter)filterObj[EntryHeaderFilterBusinessObject.Constants.EntryNumber];
			filter.Property = "QW123456";
			filter.IsActive = true;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(!entry2.MatchesFilter(filterObj.Filter));
			filter.Property = "AS123456";
			filter.IsActive = true;
			Assert(!entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
			entryNum2.CE_EntryType = CusEntryNumberTypes.Standard.UniqueConsignementReference;
			Factory.Save();
			Assert(!entry2.MatchesFilter(filterObj.Filter));
			var declaration3 = Factory.New<BaseJobDeclaration>();
			var entry3 = declaration3.CustomsEntryHeaders.AddNew();
			var entryNum3 = Factory.New<CusEntryNumber>();
			entryNum3.CE_ParentID = entry3.PK;
			entryNum3.CE_ParentTable = CusEntryHeader.Schema.TableName;
			entryNum3.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			entryNum3.CE_EntryNum = "ZX123456";
			Factory.Save();
			filter.IsActive = true;
			filter.Property = "QW123456";
			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			Assert(!entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
			Assert(entry3.MatchesFilter(filterObj.Filter));
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			Assert(!entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
			Assert(entry3.MatchesFilter(filterObj.Filter));
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			Assert(!entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
			Assert(entry3.MatchesFilter(filterObj.Filter));
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			Assert(!entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
			Assert(!entry3.MatchesFilter(filterObj.Filter));
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(!entry2.MatchesFilter(filterObj.Filter));
			Assert(entry3.MatchesFilter(filterObj.Filter));
		}

		public void TestGetJobNumberQuery()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			var entry2 = declaration1.CustomsEntryHeaders.AddNew();
			var declaration2 = Factory.New<BaseJobDeclaration>();
			var entry3 = declaration2.CustomsEntryHeaders.AddNew();
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleNumberFilter)filterObj[EntryHeaderFilterBusinessObject.Constants.JobNumber];
			filter.Property = declaration1.JE_DeclarationReference;
			filter.IsActive = true;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
			Assert(!entry3.MatchesFilter(filterObj.Filter));
			filter.Property = declaration2.JE_DeclarationReference;
			filter.IsActive = true;
			Assert(!entry1.MatchesFilter(filterObj.Filter));
			Assert(!entry2.MatchesFilter(filterObj.Filter));
			Assert(entry3.MatchesFilter(filterObj.Filter));
		}

		public void TestGetMessageStatusQuery()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_Status = DeclarationFilterConstants.EntryStatus.NotSentForFilter;
			var entry2 = declaration1.CustomsEntryHeaders.AddNew();
			entry2.CH_Status = "ERR";
			var declaration2 = Factory.New<BaseJobDeclaration>();
			var entry3 = declaration2.CustomsEntryHeaders.AddNew();
			entry3.CH_Status = ZString.Empty;
			var declaration3 = Factory.New<BaseJobDeclaration>();
			var entry4 = declaration3.CustomsEntryHeaders.AddNew();
			entry4.CH_Status = "ACK";
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[EntryHeaderFilterBusinessObject.Constants.MessageStatus];
			filter.Property = DeclarationFilterConstants.EntryStatus.NotSentForFilter;
			filter.IsActive = true;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(!entry2.MatchesFilter(filterObj.Filter));
			Assert(entry3.MatchesFilter(filterObj.Filter));
			Assert(!entry4.MatchesFilter(filterObj.Filter));
			filter.Property = "ACK";
			filter.IsActive = true;
			Assert(!entry1.MatchesFilter(filterObj.Filter));
			Assert(!entry2.MatchesFilter(filterObj.Filter));
			Assert(!entry3.MatchesFilter(filterObj.Filter));
			Assert(entry4.MatchesFilter(filterObj.Filter));
		}

		public void TestGetReferenceNumberQuery()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_BGMReference = "TEST_REF_1";
			var entry2 = declaration1.CustomsEntryHeaders.AddNew();
			entry2.CH_BGMReference = "TEST_REF_2";
			var declaration2 = Factory.New<BaseJobDeclaration>();
			var entry3 = declaration2.CustomsEntryHeaders.AddNew();
			entry3.CH_BGMReference = "TEST_REF_1";
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleNumberFilter)filterObj[EntryHeaderFilterBusinessObject.Constants.ReferenceNumber];
			filter.Property = "TEST_REF_1";
			filter.IsActive = true;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(!entry2.MatchesFilter(filterObj.Filter));
			Assert(entry3.MatchesFilter(filterObj.Filter));
			filter.Property = "TEST_REF_2";
			filter.IsActive = true;
			Assert(!entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
			Assert(!entry3.MatchesFilter(filterObj.Filter));
		}

		public void TestGetSubmissionDateQuery()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_EntrySubmittedDate = new ZDateTime(1990, 01, 01);
			var entry2 = declaration1.CustomsEntryHeaders.AddNew();
			entry2.CH_EntrySubmittedDate = new ZDateTime(1990, 02, 01);
			var declaration2 = Factory.New<BaseJobDeclaration>();
			var entry3 = declaration2.CustomsEntryHeaders.AddNew();
			entry3.CH_EntrySubmittedDate = new ZDateTime(1990, 01, 01);
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleDateFilter)filterObj[EntryHeaderFilterBusinessObject.Constants.SubmissionDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(1990, 01, 01);
			filter.Property2 = new ZDateTime(1990, 02, 01);
			filter.IsActive = true;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
			Assert(entry3.MatchesFilter(filterObj.Filter));
			filter.Property1 = new ZDateTime(1990, 01, 01);
			filter.Property2 = new ZDateTime(1990, 01, 01);
			filter.IsActive = true;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(!entry2.MatchesFilter(filterObj.Filter));
			Assert(entry3.MatchesFilter(filterObj.Filter));
			filter.Property1 = new ZDateTime(1990, 02, 01);
			filter.Property2 = new ZDateTime(1990, 02, 01);
			filter.IsActive = true;
			Assert(!entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
			Assert(!entry3.MatchesFilter(filterObj.Filter));
		}

		public void TestGetReleaseDateQuery()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_EntryReleaseDate = new ZDateTime(1990, 01, 01);
			var entry2 = declaration1.CustomsEntryHeaders.AddNew();
			entry2.CH_EntryReleaseDate = new ZDateTime(1990, 02, 01);
			var declaration2 = Factory.New<BaseJobDeclaration>();
			var entry3 = declaration2.CustomsEntryHeaders.AddNew();
			entry3.CH_EntryReleaseDate = new ZDateTime(1990, 01, 01);
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleDateFilter)filterObj[EntryHeaderFilterBusinessObject.Constants.ReleaseDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(1990, 01, 01);
			filter.Property2 = new ZDateTime(1990, 02, 01);
			filter.IsActive = true;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
			Assert(entry3.MatchesFilter(filterObj.Filter));
			filter.Property1 = new ZDateTime(1990, 01, 01);
			filter.Property2 = new ZDateTime(1990, 01, 01);
			filter.IsActive = true;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(!entry2.MatchesFilter(filterObj.Filter));
			Assert(entry3.MatchesFilter(filterObj.Filter));
			filter.Property1 = new ZDateTime(1990, 02, 01);
			filter.Property2 = new ZDateTime(1990, 02, 01);
			filter.IsActive = true;
			Assert(!entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
			Assert(!entry3.MatchesFilter(filterObj.Filter));
		}

		public void TestArrivalDateFilterDescription()
		{
			var filterObj = GetNewFilterStripBusinessObject().ModuleFilters;
			var arrivalDateFilter = (ModuleDateFilter)filterObj[EntryHeaderFilterBusinessObject.Constants.ArrivalDate];
			AssertEquals(EntryHeaderFilterBusinessObject.Constants.ArrivalDate, arrivalDateFilter.LocalizedDescription);
		}

		public void TestExportDateFilterDescription()
		{
			var filterObj = GetNewFilterStripBusinessObject().ModuleFilters;
			var exportDateFilter = (ModuleDateFilter)filterObj[EntryHeaderFilterBusinessObject.Constants.ExportDate];
			AssertEquals(EntryHeaderFilterBusinessObject.Constants.ExportDate, exportDateFilter.LocalizedDescription);
		}

		public void TestFinalDestinationETAFilterDescription()
		{
			var filterObj = GetNewFilterStripBusinessObject().ModuleFilters;
			var finalDestinationETAFilter = (ModuleDateFilter)filterObj[EntryHeaderFilterBusinessObject.Constants.FinalDestinationETA];
			AssertEquals(EntryHeaderFilterBusinessObject.Constants.FinalDestinationETA, finalDestinationETAFilter.LocalizedDescription);
		}

		public void TestOriginETDFilterDescription()
		{
			var filterObj = GetNewFilterStripBusinessObject().ModuleFilters;
			var originETDFilter = (ModuleDateFilter)filterObj[EntryHeaderFilterBusinessObject.Constants.OriginETD];
			AssertEquals(EntryHeaderFilterBusinessObject.Constants.OriginETD, originETDFilter.LocalizedDescription);
		}

		public void TestOwnerReferenceFilterDescription()
		{
			var filterObj = GetNewFilterStripBusinessObject().ModuleFilters;
			var ownerReferenceFilter = (ModuleTextFilter)filterObj[EntryHeaderFilterBusinessObject.Constants.OwnerReference];
			AssertEquals(EntryHeaderFilterBusinessObject.Constants.OwnerReference, ownerReferenceFilter.LocalizedDescription);
		}

		public void TestDeclarantFilterDescription()
		{
			var filterObj = GetNewFilterStripBusinessObject().ModuleFilters;
			var declarantFilter = (ModuleGuidFilter)filterObj[EntryHeaderFilterBusinessObject.Constants.Declarant];
			AssertEquals(EntryHeaderFilterBusinessObject.Constants.Declarant, declarantFilter.LocalizedDescription);
		}

		public void TestDeclarationTypeFilterDescription()
		{
			var filterObj = GetNewFilterStripBusinessObject().ModuleFilters;
			var declarationTypeFilter = (ModuleTextFilter)filterObj[EntryHeaderFilterBusinessObject.Constants.DeclarationType];
			AssertEquals(EntryHeaderFilterBusinessObject.Constants.DeclarationType, declarationTypeFilter.LocalizedDescription);
		}

		public void TestOriginDestinationFilterDescription()
		{
			var filterObj = GetNewFilterStripBusinessObject().ModuleFilters;
			var originDestinationFilter = (ModuleLocationFilter)filterObj[EntryHeaderFilterBusinessObject.Constants.OriginDestination];
			AssertEquals(EntryHeaderFilterBusinessObject.Constants.OriginDestination, originDestinationFilter.LocalizedDescription);
		}

		public void TestLoadingDischargeFilterDescription()
		{
			var filterObj = GetNewFilterStripBusinessObject().ModuleFilters;
			var loadingDischargeFilter = (ModuleLocationFilter)filterObj[EntryHeaderFilterBusinessObject.Constants.LoadingDischarge];
			AssertEquals(EntryHeaderFilterBusinessObject.Constants.LoadingDischarge, loadingDischargeFilter.LocalizedDescription);
		}

		public void TestCustomsAgentFilterDescription()
		{
			var filterObj = GetNewFilterStripBusinessObject().ModuleFilters;
			var customsAgentFilter = (ModuleNkFilter)filterObj[EntryHeaderFilterBusinessObject.Constants.CustomsAgent];
			AssertEquals(EntryHeaderFilterBusinessObject.Constants.CustomsAgent, customsAgentFilter.LocalizedDescription);
		}

		public void TestBranchFilterDescription()
		{
			var filterObj = GetNewFilterStripBusinessObject().ModuleFilters;
			var branchFilter = (ModuleGuidFilter)filterObj[EntryHeaderFilterBusinessObject.Constants.Branch];
			AssertEquals(EntryHeaderFilterBusinessObject.Constants.Branch, branchFilter.LocalizedDescription);
		}

		public void TestImporterSupplierFilterDescription()
		{
			var filterObj = GetNewFilterStripBusinessObject().ModuleFilters;
			var importerSupplierFilter = (ModuleGuidsFilter)filterObj[DeclarationFilterConstants.OrgFilterTypes.ImporterSupplier];
			AssertEquals(DeclarationFilterConstants.OrgFilterTypes.ImporterSupplier, importerSupplierFilter.LocalizedDescription);
		}

		public void TestControllingAgentFilterDescription()
		{
			var filterObj = GetNewFilterStripBusinessObject().ModuleFilters;
			var controllingAgentFilter = (ModuleGuidFilter)filterObj[DeclarationFilterConstants.OrgFilterTypes.ControllingAgent];
			AssertEquals(DeclarationFilterConstants.OrgFilterTypes.ControllingAgent, controllingAgentFilter.LocalizedDescription);
		}

		public void TestControllingCustomerFilterDescription()
		{
			var filterObj = GetNewFilterStripBusinessObject().ModuleFilters;
			var controllingCustomerFilter = (ModuleGuidFilter)filterObj[DeclarationFilterConstants.OrgFilterTypes.ControllingCustomer];
			AssertEquals(DeclarationFilterConstants.OrgFilterTypes.ControllingCustomer, controllingCustomerFilter.LocalizedDescription);
		}

		public void TestShipmentTypeFilterDescription()
		{
			var filterObj = GetNewFilterStripBusinessObject().ModuleFilters;
			var shipmentTypeFilter = (ModuleTextFilter)filterObj[DeclarationFilterConstants.ShipmentType];
			AssertEquals(DeclarationFilterConstants.ShipmentType, shipmentTypeFilter.LocalizedDescription);
		}

		public void TestTransportModeFilterDescription()
		{
			var filterObj = GetNewFilterStripBusinessObject().ModuleFilters;
			var transportModeFilter = (ModuleTextFilter)filterObj[DeclarationFilterConstants.TransportMode];
			AssertEquals(DeclarationFilterConstants.TransportMode, transportModeFilter.LocalizedDescription);
		}

		public void TestAgentsReferenceFilterDescription()
		{
			var filterObj = GetNewFilterStripBusinessObject().ModuleFilters;
			var agentsReferenceFilter = (ModuleTextFilter)filterObj[DeclarationFilterConstants.NumberFilterTypes.AgentsReference];
			AssertEquals(DeclarationFilterConstants.NumberFilterTypes.AgentsReference, agentsReferenceFilter.LocalizedDescription);
		}

		public void TestEntryNumberFilterDescription()
		{
			var filterObj = GetNewFilterStripBusinessObject().ModuleFilters;
			var entryNumberFilter = (ModuleNumberFilter)filterObj[EntryHeaderFilterBusinessObject.Constants.EntryNumber];
			AssertEquals(EntryHeaderFilterBusinessObject.Constants.EntryNumber, entryNumberFilter.LocalizedDescription);
		}

		public void TestReferenceNumberFilterDescription()
		{
			var filterObj = GetNewFilterStripBusinessObject().ModuleFilters;
			var referenceNumberFilter = (ModuleNumberFilter)filterObj[EntryHeaderFilterBusinessObject.Constants.ReferenceNumber];
			AssertEquals(EntryHeaderFilterBusinessObject.Constants.ReferenceNumber, referenceNumberFilter.LocalizedDescription);
		}

		public void TestWarehouseTransactionStatusFilterDescription()
		{
			var filterObj = GetNewFilterStripBusinessObject().ModuleFilters;
			var whsTransStatusFilter = (ModuleTextFilter)filterObj[EntryHeaderFilterBusinessObject.Constants.WarehouseTransactionStatus];
			AssertEquals(EntryHeaderFilterBusinessObject.Constants.WarehouseTransactionStatus, whsTransStatusFilter.Description);
			AssertEquals(EntryHeaderFilterBusinessObject.Constants.WarehouseTransactionStatus, whsTransStatusFilter.LocalizedDescription);
		}

		public void TestMessageTypeFilterDescription()
		{
			var filterObj = GetNewFilterStripBusinessObject();
			var messageTypeFilter = (ModuleTextFilter)filterObj[EntryHeaderFilterBusinessObject.Constants.MessageType];
			AssertEquals(EntryHeaderFilterBusinessObject.Constants.MessageType, messageTypeFilter.LocalizedDescription);
		}

		public void TestModuleFiltersAreAdded()
		{
			var moduleFilters = new List<ZString>()
			{ EntryHeaderFilterBusinessObject.Constants.EntryNumber, EntryHeaderFilterBusinessObject.Constants.JobNumber, EntryHeaderFilterBusinessObject.Constants.ReferenceNumber, EntryHeaderFilterBusinessObject.Constants.SubmissionDate, EntryHeaderFilterBusinessObject.Constants.ReleaseDate, EntryHeaderFilterBusinessObject.Constants.EntryStatus, EntryHeaderFilterBusinessObject.Constants.MessageStatus, EntryHeaderFilterBusinessObject.Constants.WarehouseTransactionStatus, EntryHeaderFilterBusinessObject.Constants.MessageType, EntryHeaderFilterBusinessObject.Constants.ArrivalDate, EntryHeaderFilterBusinessObject.Constants.ExportDate, EntryHeaderFilterBusinessObject.Constants.FinalDestinationETA, EntryHeaderFilterBusinessObject.Constants.OriginETD, EntryHeaderFilterBusinessObject.Constants.OwnerReference, EntryHeaderFilterBusinessObject.Constants.Declarant, EntryHeaderFilterBusinessObject.Constants.DeclarationType, EntryHeaderFilterBusinessObject.Constants.OriginDestination, EntryHeaderFilterBusinessObject.Constants.LoadingDischarge, EntryHeaderFilterBusinessObject.Constants.CustomsAgent, EntryHeaderFilterBusinessObject.Constants.Branch, DeclarationFilterConstants.OrgFilterTypes.ImporterSupplier, DeclarationFilterConstants.OrgFilterTypes.ControllingAgent, DeclarationFilterConstants.OrgFilterTypes.ControllingCustomer, DeclarationFilterConstants.ShipmentType, DeclarationFilterConstants.TransportMode, DeclarationFilterConstants.NumberFilterTypes.AgentsReference, DeclarationFilterConstants.NumberFilterTypes.MasterBill, DeclarationFilterConstants.NumberFilterTypes.HouseBill, };
			var filterObj = GetNewFilterStripBusinessObject();
			foreach (var filter in moduleFilters)
			{
				AssertNotNull(filterObj[filter]);
			}
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new EntryHeaderFilterBusinessObject();
		protected override void SetUp()
		{
			base.SetUp();
			((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
			try
			{
				//GlbCompany.CurrentCompany.SetCountry changes GC_RN_NKCountryCode, and needs to be saved to db as JobDeclarationFilter(DBOnlyQuery) is performed in FilterObject.
				GlbCompany.CurrentCompany.Factory.Save();
			}
			finally
			{
				((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = false;
			}
		}

		void AssertModuleTextFilterMatchResult(ZString filterName, CusEntryHeader entry1, CusEntryHeader entry2, ZString property1, ZString property2)
		{
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.Property = property1;
			AssertEquals(true, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entry2.MatchesFilter(filterObj.Filter));
			filter.Property = property2;
			AssertEquals(false, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entry2.MatchesFilter(filterObj.Filter));
			filter.Property = "XXX";
			AssertEquals(false, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entry2.MatchesFilter(filterObj.Filter));
		}

		void AssertModuleGuidFilterMatchResult(ZString filterName, CusEntryHeader entry1, CusEntryHeader entry2, ZGuid property1, ZGuid property2)
		{
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleGuidFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.Property = property1;
			AssertEquals(true, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entry2.MatchesFilter(filterObj.Filter));
			filter.Property = property2;
			AssertEquals(false, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entry2.MatchesFilter(filterObj.Filter));
			filter.Property = ZGuid.NewZGuid();
			AssertEquals(false, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entry2.MatchesFilter(filterObj.Filter));
		}

		void AssertModuleDateFilterMatchResult(ZString filterName, CusEntryHeader entry1, CusEntryHeader entry2, ZDateTime property1, ZDateTime property2)
		{
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleDateFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = property1;
			AssertEquals(true, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entry2.MatchesFilter(filterObj.Filter));
			filter.Property1 = property2;
			AssertEquals(false, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entry2.MatchesFilter(filterObj.Filter));
			filter.Property1 = property2.AddDays(1);
			AssertEquals(false, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entry2.MatchesFilter(filterObj.Filter));
			filter.Property1 = property1.AddDays(-1);
			filter.Property2 = property2.AddDays(1);
			AssertEquals(true, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entry2.MatchesFilter(filterObj.Filter));
		}

		void AssertModuleLocationFilterMatchResult(ZString filterName, CusEntryHeader entry1, CusEntryHeader entry2, ZString property11, ZString property12, ZString property21, ZString property22)
		{
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleLocationFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.Property1 = property11;
			AssertEquals(true, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entry2.MatchesFilter(filterObj.Filter));
			filter.Property2 = property12;
			AssertEquals(true, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entry2.MatchesFilter(filterObj.Filter));
			filter.Property1 = property12;
			filter.Property2 = property11;
			AssertEquals(false, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entry2.MatchesFilter(filterObj.Filter));
			filter.Property1 = property21;
			filter.Property2 = property22;
			AssertEquals(false, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entry2.MatchesFilter(filterObj.Filter));
			filter.Property1 = "";
			filter.Property2 = property22;
			AssertEquals(false, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entry2.MatchesFilter(filterObj.Filter));
			filter.Property1 = "";
			filter.Property2 = "";
			AssertEquals(true, entry1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entry2.MatchesFilter(filterObj.Filter));
		}

		(OrgHeader orgHeader1, OrgHeader orgHeader2) CreateOrgheaders(ZString code1, ZString code2)
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = code1;
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = code2;
			Factory.Save();
			return (orgHeader1, orgHeader2);
		}

		(BaseJobDeclaration declaration1, BaseJobDeclaration declaration2, CusEntryHeader entry1, CusEntryHeader entry2) CreateDeclarationsAndEntries()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var declaration2 = Factory.New<BaseJobDeclaration>();
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			Factory.Save();
			return (declaration1, declaration2, entry1, entry2);
		}
	}
}
