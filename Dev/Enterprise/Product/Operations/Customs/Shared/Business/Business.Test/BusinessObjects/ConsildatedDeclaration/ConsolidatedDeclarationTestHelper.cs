using CargoWise.EntityFramework;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public static class ConsolidatedDeclarationTestHelper
	{
		public static TConsolidatedDeclaration CreateConsolidatedDeclaration<TConsolidatedDeclaration>(BusinessObjectFactory factory, int countOfJobDeclaration = 1, string applicationCode = "") where TConsolidatedDeclaration : ConsolidatedDeclaration
		{
			var consolidatedDeclaration = factory.NewWithValidTestData<TConsolidatedDeclaration>();
			consolidatedDeclaration.CRD_OA_DeclarantAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses[0].PK;
			consolidatedDeclaration.CRD_DataModel = GlbCompany.CurrentCompany.Country.Code;
			for (int i = 0; i < countOfJobDeclaration; i++)
			{
				var jobDeclaration = factory.NewWithValidTestData(consolidatedDeclaration.JobDeclarations.GetType().GetGenericArguments()[0]) as BaseJobDeclaration;
				if (!string.IsNullOrEmpty(applicationCode))
				{
					jobDeclaration.JE_ApplicationCode = applicationCode;
				}
				jobDeclaration.ActiveEntryHeaders.AddNew();
				jobDeclaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
				consolidatedDeclaration.JobDeclarations.Add(jobDeclaration);
			}
			return consolidatedDeclaration;
		}

		public static TJobDeclaration CreateJobDeclarationReadyForConsolidation<TJobDeclaration>(BusinessObjectFactory factory) where TJobDeclaration : BaseJobDeclaration
		{
			var jobDeclaration = factory.NewWithValidTestData<TJobDeclaration>();
			jobDeclaration.ActiveEntryHeaders.AddNew();
			jobDeclaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
			return jobDeclaration;
		}
	}
}
