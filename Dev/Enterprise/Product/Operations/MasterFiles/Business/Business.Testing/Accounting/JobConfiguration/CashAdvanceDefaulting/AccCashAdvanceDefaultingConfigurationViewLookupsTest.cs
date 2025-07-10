using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccCashAdvanceDefaultingConfigurationViewLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestChargeCodeFindBoxCollection()
		{
			var cashAdvanceConfigLookup = new AccCashAdvanceDefaultingConfigurationViewLookups(cashAdvanceConfig);
			var findBoxCollection = cashAdvanceConfigLookup.ChargeCodeFindBoxCollection;
			Assert("Should have default 'Description' filter.", findBoxCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Description:Property"));
		}

		public void TestLedgerList()
		{
			AssertOptionsAsExpected(l => l.LedgerList, c => new[]
				{
					(string.Empty, "Both Accounts Receivable and Payable"),
					(LedgerTypes.AccountsReceivable, AccPaymentApprovalLookups.AccountsReceivableDescription),
					(LedgerTypes.AccountsPayable, AccPaymentApprovalLookups.AccountsPayableDescription),
				});
		}

		public void TestJobTypeList()
		{
			var jobTypes = JobConfigurationLookupsExtensions.GetJobTypeList();
			AssertOptionsAsExpected(l => l.JobTypeList, c => jobTypes);
		}

		public void TestDirectionsList()
		{
			foreach (var jobType in allJobTypes)
			{
				AssertOptionsAsExpected(l => l.DirectionsList, c => c.GetDirectionList(), jobType);
			}
		}

		public void TestTransportModesList()
		{
			foreach (var jobType in allJobTypes)
			{
				AssertOptionsAsExpected(l => l.TransportModesList, c => c.GetTransportModeList(), jobType);
			}
		}

		public void TestDefaultingOptionList()
		{
			AssertOptionsAsExpected(l => l.DefaultingOptions, c => CashAdvanceDefaultingOption.CodesList);
		}

		void AssertOptionsAsExpected(Func<AccCashAdvanceDefaultingConfigurationViewLookups, CodeDescriptionPairList> codePairListGetter, Func<IJobConfiguration, CodeDescriptionPairList> expectedResultGetter,
			string jobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All)
		{
			AssertOptionsAsExpected(codePairListGetter, c => expectedResultGetter(c).Cast<CodeDescriptionPair>().Select(x => (x.Code, x.Description)).ToArray(), jobType);
		}

		void AssertOptionsAsExpected(Func<AccCashAdvanceDefaultingConfigurationViewLookups, CodeDescriptionPairList> codePairListGetter, Func<IJobConfiguration, (string, string)[]> expectedResultGetter,
			string jobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All)
		{
			cashAdvanceConfig.CAC_JobType = jobType;
			var cashAdvanceConfigLookup = new AccCashAdvanceDefaultingConfigurationViewLookups(cashAdvanceConfig);

			var optionsActual = codePairListGetter(cashAdvanceConfigLookup).Cast<CodeDescriptionPair>().Select(c => (c.Code, c.Description)).ToArray();
			var optionsExpected = expectedResultGetter(cashAdvanceConfig);

			AssertArrayEqualsByElements(optionsExpected, optionsActual);
		}

		protected override void SetUp()
		{
			base.SetUp();

			cashAdvanceConfig = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			allJobTypes = JobConfigurationLookupsExtensions.GetJobTypeList().Cast<CodeDescriptionPair>().Select(x => x.Code).ToArray();
		}
		AccCashAdvanceDefaultingConfiguration cashAdvanceConfig;
		string[] allJobTypes;
	}
}
