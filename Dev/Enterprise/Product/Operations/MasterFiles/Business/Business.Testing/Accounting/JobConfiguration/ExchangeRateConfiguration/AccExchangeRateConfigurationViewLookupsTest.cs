using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccExchangeRateConfigurationViewLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLedgerList()
		{
			// System Level
			AssertOptionsAsExpected(l => l.LedgerList, c => new[]
				{
					(string.Empty, "Both Accounts Receivable and Payable"),
					(LedgerTypes.AccountsReceivable, AccPaymentApprovalLookups.AccountsReceivableDescription),
					(LedgerTypes.AccountsPayable, AccPaymentApprovalLookups.AccountsPayableDescription),
				}, additionalCfgSetUp: x => { x.JCE_GC = ZGuid.Empty; x.JCE_ParentTableCode = ""; x.JCE_ParentID = ZGuid.Empty; });

			// Company Level
			AssertOptionsAsExpected(l => l.LedgerList, c => new[]
				{
					(string.Empty, "Both Accounts Receivable and Payable"),
					(LedgerTypes.AccountsReceivable, AccPaymentApprovalLookups.AccountsReceivableDescription),
					(LedgerTypes.AccountsPayable, AccPaymentApprovalLookups.AccountsPayableDescription),
				}, additionalCfgSetUp: x => { x.JCE_ParentTableCode = ""; x.JCE_ParentID = ZGuid.Empty; });

			// Debtor Group
			AssertOptionsAsExpected(l => l.LedgerList, c => new[]
				{
					(LedgerTypes.AccountsReceivable, AccPaymentApprovalLookups.AccountsReceivableDescription),
				}, additionalCfgSetUp: x => { x.JCE_ParentTableCode = OrgDebtorGroupSchema.Constants.Prefix; x.JCE_ParentID = ZGuid.NewZGuid(); });

			// Creditor Group
			AssertOptionsAsExpected(l => l.LedgerList, c => new[]
				{
					(LedgerTypes.AccountsPayable, AccPaymentApprovalLookups.AccountsPayableDescription),
				}, additionalCfgSetUp: x => { x.JCE_ParentTableCode = OrgCreditorGroupSchema.Constants.Prefix; x.JCE_ParentID = ZGuid.NewZGuid(); });

			// Debtor & Creditor
			AssertOptionsAsExpected(l => l.LedgerList, c => new[]
				{
					(LedgerTypes.AccountsReceivable, AccPaymentApprovalLookups.AccountsReceivableDescription),
					(LedgerTypes.AccountsPayable, AccPaymentApprovalLookups.AccountsPayableDescription),
				}, additionalCfgSetUp: x => { x.JCE_ParentTableCode = OrgHeaderSchema.Constants.Prefix; x.JCE_ParentID = ZGuid.NewZGuid(); });
		}

		public void TestJobTypeList()
		{
			var jobTypes = JobConfigurationLookupsExtensions.GetJobTypeList();
			jobTypes.InsertInSortOrder(new CodeDescriptionPair("NJR", "Non-Job"));
			AssertOptionsAsExpected(l => l.JobTypeList, c => jobTypes);
		}

		public void TestDirectionList()
		{
			foreach (var jobType in allJobTypes)
			{
				AssertOptionsAsExpected(l => l.DirectionList, c => c.GetDirectionList(), jobType);
			}
		}

		public void TestTransportModeList()
		{
			foreach (var jobType in allJobTypes)
			{
				AssertOptionsAsExpected(l => l.TransportModeList, c => c.GetTransportModeList(), jobType);
			}
		}

		public void TestPreferencesList()
		{
			foreach (var jobType in allJobTypes)
			{
				AssertOptionsAsExpected(l => l.PreferenceList, c => c.GetPreferenceList());
			}
		}

		public void TestCurrencyTypeList()
		{
			var currencyLookup = new AccExchangeRateConfigurationViewLookups(exRateConfig).CurrencyTypeList;

			AssertEquals(2, currencyLookup.Count);
			AssertEquals("Applies to all currencies", currencyLookup["ALL"].Description);
			AssertEquals("Applies only to currencies listed", currencyLookup["CUR"].Description);
		}

		#region Implementation

		void AssertOptionsAsExpected(Func<AccExchangeRateConfigurationViewLookups, CodeDescriptionPairList> codePairListGetter, Func<IJobConfiguration, CodeDescriptionPairList> expectedResultGetter,
			string jobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All)
		{
			AssertOptionsAsExpected(codePairListGetter, c => expectedResultGetter(c).Cast<CodeDescriptionPair>().Select(x => (x.Code, x.Description)).ToArray(), jobType);
		}

		void AssertOptionsAsExpected(Func<AccExchangeRateConfigurationViewLookups, CodeDescriptionPairList> codePairListGetter, Func<IJobConfiguration, (string, string)[]> expectedResultGetter,
			string jobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All, Action<AccExchangeRateConfiguration> additionalCfgSetUp = null)
		{
			exRateConfig.JCE_JobType = jobType;
			if (additionalCfgSetUp != null)
			{
				additionalCfgSetUp(exRateConfig);
			}
			var exRateConfigLookup = new AccExchangeRateConfigurationViewLookups(exRateConfig);

			var optionsActual = codePairListGetter(exRateConfigLookup).Cast<CodeDescriptionPair>().Select(c => (c.Code, c.Description)).ToArray();
			var optionsExpected = expectedResultGetter(exRateConfig);

			AssertArrayEqualsByElements(optionsExpected, optionsActual);
		}

		protected override void SetUp()
		{
			base.SetUp();

			exRateConfig = Factory.New<AccExchangeRateConfiguration>();
			allJobTypes = JobConfigurationLookupsExtensions.GetJobTypeList().Cast<CodeDescriptionPair>().Select(x => x.Code).ToArray();
		}
		AccExchangeRateConfiguration exRateConfig;
		string[] allJobTypes;

		#endregion
	}
}
