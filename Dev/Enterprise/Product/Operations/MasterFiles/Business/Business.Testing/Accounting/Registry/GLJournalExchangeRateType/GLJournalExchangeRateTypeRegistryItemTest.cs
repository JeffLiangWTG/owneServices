using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GLJournalExchangeRateTypeRegistryItem))]
	sealed class GLJournalExchangeRateTypeRegistryItemTest : StronglyTypedRegistryItemTestCase<GLJournalExchangeRateType>
	{
		protected override StronglyTypedRegistryItem<GLJournalExchangeRateType, GLJournalExchangeRateType> GetNewRegistryItem()
		{
			return new GLJournalExchangeRateTypeRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new GLJournalExchangeRateType() { BalanceSheetAccountTypeExchangeRateType = "CUS", ProfitAndLossAccountTypeExchangeRateType = "CUS" });
		}
	}
}
