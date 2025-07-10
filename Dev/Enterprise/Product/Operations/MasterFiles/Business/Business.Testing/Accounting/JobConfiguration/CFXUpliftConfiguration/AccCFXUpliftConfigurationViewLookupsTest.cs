using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.JobConfigurationLookupsExtensions;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccCFXUpliftConfigurationViewLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookupsProvideCorrectJobTypeListOptions()
		{
			CodeDescriptionPairList GetJobTypeListForCFXUplift()
			{
				var jobTypeList = GetJobTypeList();
				jobTypeList.RemoveCode(JobInvoicingConsumerTypes.ForwardingConsol);
				return jobTypeList;
			}

			AssertOptionsAsExpected(l => l.JobTypesList, c => GetJobTypeListForCFXUplift());
		}

		public void TestLookupsProvideCorrectDirectionListOptions()
		{
			foreach (var jobType in allJobTypes)
			{
				AssertOptionsAsExpected(l => l.DirectionsList, c => c.GetDirectionList(), jobType);
			}
		}

		public void TestLookupsProvideCorrectTransportModeListOptions()
		{
			foreach (var jobType in allJobTypes)
			{
				AssertOptionsAsExpected(l => l.TransportModesList, c => c.GetTransportModeList(), jobType);
			}
		}

		void AssertOptionsAsExpected(Func<AccCFXUpliftConfigurationViewLookups, CodeDescriptionPairList> codePairListGetter, Func<IJobConfiguration, CodeDescriptionPairList> expectedResultGetter, string jobType = "ALL")
		{
			cfxConfig.JCF_JobType = jobType;
			var exRateConfigLookup = new AccCFXUpliftConfigurationViewLookups(cfxConfig);

			var optionsActual = codePairListGetter(exRateConfigLookup).Cast<CodeDescriptionPair>().Select(c => (c.Code, c.Description)).ToArray();
			var optionsExpected = expectedResultGetter(cfxConfig).Cast<CodeDescriptionPair>().Select(c => (c.Code, c.Description)).ToArray();

			AssertArrayEqualsByElements(optionsExpected, optionsActual);
		}

		protected override void SetUp()
		{
			base.SetUp();

			cfxConfig = Factory.New<AccCFXUpliftConfiguration>();
			allJobTypes = GetJobTypeList().Cast<CodeDescriptionPair>().Select(x => x.Code).ToArray();
		}

		AccCFXUpliftConfiguration cfxConfig;
		string[] allJobTypes;
	}
}
