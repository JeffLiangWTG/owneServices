using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Module;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Module.Declaration.FormalEntry.Testing
{
	[TestedType(typeof(NZJobDeclarationFilterBusinessObject))]
	sealed class NZJobDeclarationFilterBOTest : ZArchitecture.Modules.Testing.FilterStripBusinessObjectTestCase
	{
		public void TestLookups()
		{
			NZJobDeclarationFilterBusinessObject filterBizObj = new NZJobDeclarationFilterBusinessObject();
			AssertEquals("Lookups of the correct type", typeof(NZJobDeclarationFilterLookups), filterBizObj.Lookups.GetType());
		}

		public void TestNZJobDeclarationFiltersExists()
		{
			FilterStripBusinessObject filterStrip = GetNewFilterStripBusinessObject();
			ModuleFilterCollection filters = filterStrip.ModuleFilters;
			Assert("Missing MPI Status.", filters.Filter_List.ContainsCode(NZJobDeclarationFilterBusinessObject.MAFStatusFilterName));
			Assert("Missing MPI Consignment Number.", filters.Filter_List.ContainsCode(NZJobDeclarationFilterBusinessObject.MAFConsignmentNumberFilterName));
		}

		public void TestTSWCombinedStatusFilter()
		{
			FilterStripBusinessObject filterStrip = GetNewFilterStripBusinessObject();
			ModuleFilterCollection filters = filterStrip.ModuleFilters;
			AssertEquals("TSWCombinedStatusFilter should now be accesable", true, filters.Filter_List.ContainsCode(NZJobDeclarationFilterBusinessObject.TSWCombinedStatusFilterName));
			AssertEquals("Messaging Mode should now be accesable", true, filters.Filter_List.ContainsCode(Enterprise.Customs.Module.DeclarationFilterConstants.SubmitType));
		}

		public void TestGetMAFStatusQuery()
		{
			FilterStripBusinessObject filterStrip = GetNewFilterStripBusinessObject();
			ModuleFilterCollection filters = filterStrip.ModuleFilters;
			ModuleTextFilter filter = filters[NZJobDeclarationFilterBusinessObject.MAFStatusFilterName] as ModuleTextFilter;
			AssertNotNull("filters[NZJobDeclarationFilterBusinessObject.FilterNameMAFStatus]", filter);
			ZQuery mafStatusQuery;
			string message = "MPI Status Query invalid.";
			filter.Property = MessagingStatusList.Codes.CancelledByMpi;
			mafStatusQuery = filter.Query;
			AssertEquals(message, string.Format("{0} like '%{1}={2}%'", JobDeclarationSchema.Constants.JE_AddInfo, "MAF_MessagingStatus", MessagingStatusList.Codes.CancelledByMpi), mafStatusQuery.LiteralTextADO);
			filter.Property = MessagingStatusList.Codes.SentPendingAcknowledgement;
			mafStatusQuery = filter.Query;
			AssertNotEquals(message, string.Format("{0} like '%{1}={2}%'", JobDeclarationSchema.Constants.JE_AddInfo, "MAF_MessagingStatus", MessagingStatusList.Codes.SentAndAcknowledged), mafStatusQuery.LiteralTextADO);
		}

		public void TestGetMAFConsignmentNumberQuery()
		{
			FilterStripBusinessObject filterStrip = GetNewFilterStripBusinessObject();
			ModuleFilterCollection filters = filterStrip.ModuleFilters;
			ModuleTextFilter filter = filters[NZJobDeclarationFilterBusinessObject.MAFConsignmentNumberFilterName] as ModuleTextFilter;
			AssertNotNull("filters[NZJobDeclarationFilterBusinessObject.MAFConsignmentNumberFilterName]", filter);
			ZQuery mafConsignmentNumberQuery;
			string message = "MPI Consignment Number Query invalid";
			filter.Property = "C2009";
			mafConsignmentNumberQuery = filter.Query;
			AssertEquals(message, string.Format("{0} like '%{1}={2}%'", JobDeclarationSchema.Constants.JE_AddInfo, "MAF_ConsignmentNumber", filter.Property), mafConsignmentNumberQuery.LiteralTextADO);
			filter.Property = "C2009";
			mafConsignmentNumberQuery = filter.Query;
			AssertNotEquals(message, string.Format("{0} like '%{1}={2}%'", JobDeclarationSchema.Constants.JE_AddInfo, "MAF_ConsignmentNumber", "C2008"), mafConsignmentNumberQuery.LiteralTextADO);
		}

		public void TestEntryStatusFilterForConsolications()
		{
			BusinessObject[] filteredDecs = null;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.AppliedToConsolidation;
			declaration.JE_MessageStatus = ConsolidatedEntryStatusList.Codes.AppliedToConsolidation;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			declaration2.JE_MessageStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.JE_EntryStatus = FormalEntryStatusList.Codes.EntryRejected;
			declaration3.JE_MessageStatus = ConsolidatedEntryStatusList.Codes.AppliedToConsolidation;

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration4.JE_EntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;

			var declaration5 = Factory.New<JobDeclaration>();
			declaration5.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration5.JE_EntryStatus = FormalEntryStatusList.Codes.EntryInError;
			declaration5.JE_MessageStatus = ConsolidatedEntryStatusList.Codes.AppliedToConsolidation;
			Factory.Save();

			var filterBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var filter = (EntryStatusFilter)filterBO[DeclarationFilterConstants.EntryStatusText];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = FormalEntryStatusList.Codes.DeliveryOrderReceived;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found the 1 DOR Entry Status declaration", 1, filteredDecs.Length);
			AssertEquals("Should be declaration4", declaration4.PK, filteredDecs[0].PK);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = ConsolidatedEntryStatusList.Codes.AppliedToConsolidation;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Consolidation ATC declaration", 1, filteredDecs.Length);
			AssertEquals("Should be declaration1 - declaration3 is not in Pre-Consolidation state", declaration.PK, filteredDecs[0].PK);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			filter.Property = ConsolidatedEntryStatusList.Codes.AppliedToConsolidation;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 4 declarations not in ATC state for this filter", 4, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should not have found a declaration in RTC state", 0, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			filter.Property = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found all declarations - All declarations are not in Pre-Consolidation state - therefore any dec with MessageStatus = RTC is not applicable for this filter", 5, filteredDecs.Length);

			var declaration6 = Factory.New<JobDeclaration>();
			declaration6.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration6.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
			declaration6.JE_MessageStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
			Factory.Save();

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should now find 1 declaration in RTC state", 1, filteredDecs.Length);
			AssertEquals("Should be declaration6", declaration6.PK, filteredDecs[0].PK);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new NZJobDeclarationFilterBusinessObject();
		}
	}
}
