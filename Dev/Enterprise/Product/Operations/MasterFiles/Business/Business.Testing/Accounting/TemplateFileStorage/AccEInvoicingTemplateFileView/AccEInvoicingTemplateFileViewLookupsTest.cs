using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.JobConfigurationLookupsExtensions;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccEInvoicingTemplateFileViewLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookupsProvideCorrectJobTypeListOptions()
		{
			AssertOptionsAsExpected(l => l.JobTypesList, c => GetJobTypeList());
		}

		public void TestLookupsProvideCorrectDirectionListOptions()
		{
			var allJobTypes = GetJobTypeList().Cast<CodeDescriptionPair>().Select(x => x.Code).ToArray();
			foreach (var jobType in allJobTypes)
			{
				AssertOptionsAsExpected(l => l.DirectionsList, c => c.GetDirectionList(), jobType);
			}
		}

		public void TestLookupsProvideCorrectTransportModeListOptions()
		{
			var allJobTypes = GetJobTypeList().Cast<CodeDescriptionPair>().Select(x => x.Code).ToArray();
			foreach (var jobType in allJobTypes)
			{
				AssertOptionsAsExpected(l => l.TransportModesList, c => c.GetTransportModeList(), jobType);
			}
		}

		public void TestLookupsProvideCorrectTemplateCodeList()
		{
			var template1 = Factory.NewWithValidTestData<AccTemplateFileStorage>();
			var template2 = Factory.NewWithValidTestData<AccTemplateFileStorage>();

			var templateConfig = Factory.NewWithValidTestData<AccEInvoicingTemplateFileView>();
			var templateConfigLookup = new AccEInvoicingTemplateFileViewLookups(templateConfig);

			Assert(templateConfigLookup.TemplateCodesList.ContainsCode(template1.TFS_Code));
			Assert(templateConfigLookup.TemplateCodesList.ContainsCode(template2.TFS_Code));
		}

		void AssertOptionsAsExpected(Func<AccEInvoicingTemplateFileViewLookups, CodeDescriptionPairList> codePairListGetter, Func<IJobConfiguration, CodeDescriptionPairList> expectedResultGetter, string jobType = "ALL")
		{
			var templateConfig = Factory.New<AccEInvoicingTemplateFileView>();
			templateConfig.ETF_JobType = jobType;
			var templateConfigLookup = new AccEInvoicingTemplateFileViewLookups(templateConfig);

			var jobConfiguration = new DummyJobConfiguration() { JobType = jobType, IncludeOptionsForAllJobTypes = true };

			var optionsActual = codePairListGetter(templateConfigLookup).Cast<CodeDescriptionPair>().Select(c => (c.Code, c.Description)).ToArray();
			var optionsExpected = expectedResultGetter(jobConfiguration).Cast<CodeDescriptionPair>().Select(c => (c.Code, c.Description)).ToArray();

			AssertArrayEqualsByElements(optionsExpected, optionsActual);
		}

		sealed class DummyJobConfiguration : IJobConfiguration
		{
			public ZString JobType { get; set; }
			public ZString ServiceDirection { get; set; }
			public ZString TransportMode { get; set; }
			public bool IncludeOptionsForAllJobTypes { get; set; }
		}
	}
}
