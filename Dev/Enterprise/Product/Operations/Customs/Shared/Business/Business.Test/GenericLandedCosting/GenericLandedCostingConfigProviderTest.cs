using System.Linq;
using System.Threading.Tasks;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class GenericLandedCostingConfigProviderTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetGenericLandedCostingConfig()
		{
			foreach (var countryCode in GenericLandedCostingConfigProvider.Instance.SupportedCountries)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					CombineAssertions(() =>
					{
						var config = GenericLandedCostingConfigProvider.Instance.GetGenericLandedCostingConfig(countryCode);
						AssertNotNull("There should be a GenericLandedCostingConfig for " + countryCode, config);

						var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
						var entryLine = Factory.New<CusEntryLine>();
						foreach (var mapping in config.LCEntryCustomsDisbursementCodeMappings)
						{
							Assert("EntryDisbursementCode should NOT be empty", !mapping.EntryDisbursementCode.IsEmpty);
							Assert("LCDisbursementCode should NOT be empty", !mapping.LCDisbursementCode.IsEmpty);

							var jobComInvoiceLineAmountPropertyName = mapping.JobComInvoiceLineAmountPropertyName;
							if (!jobComInvoiceLineAmountPropertyName.IsEmpty)
							{
								AssertNoExceptionThrown(() => invoiceLine.GetPropertyValue<ZDecimal>(jobComInvoiceLineAmountPropertyName));
							}

							var cusEntryLineApplicablePropertyName = mapping.CusEntryLineApplicablePropertyName;
							if (!cusEntryLineApplicablePropertyName.IsEmpty)
							{
								AssertNoExceptionThrown(() => entryLine.GetPropertyValue<ZBool>(cusEntryLineApplicablePropertyName));
							}
						}
						var redundantEntryDisbursementCodes = config.LCEntryCustomsDisbursementCodeMappings.GroupBy(x => x.EntryDisbursementCode).Where(x => x.Count() > 1).Select(x => x.First().EntryDisbursementCode);
						Assert("EntryDisbursementCode of LCEntryCustomsDisbursementCodeMappings should Not be redundant: " + string.Join(",", redundantEntryDisbursementCodes.ToList()), !redundantEntryDisbursementCodes.Any());

						foreach (var setting in config.LandedLineCostItemSettings)
						{
							Assert("CostType should NOT be empty", !setting.CostType.IsEmpty);
						}
						var redundantCostTypes = config.LandedLineCostItemSettings.GroupBy(x => x.CostType).Where(x => x.Count() > 1).Select(x => x.First().CostType);
						Assert("CostType of LandedLineCostItemSettings should Not be redundant: " + string.Join(",", redundantCostTypes.ToList()), !redundantCostTypes.Any());
					});
				}
			}
		}

		[ExpectNoExceptions]
		public void TestLockOnLoadingConfig()
		{
			var task1 = Task.Run(() => { _ = GenericLandedCostingConfigProvider.Instance.SupportedCountries; });
			var task2 = Task.Run(() => { _ = GenericLandedCostingConfigProvider.Instance.SupportedCountries; });
			Task.WaitAll(task1, task2);
		}
	}
}
