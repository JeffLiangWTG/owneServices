using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class AdditionalSupplementaryCodesProviderTest : TestCaseWithFactory
	{
		public void TestTaxImmunitysMembers()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				var headerJobDeclaration = helper.GetProviderHeader();
				var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
				var entryLines = declaration.EntryLines.ToArray()[0];
				var taxImmunitys = entryLines.TaxImmunitys.ToArray();

				CombineAssertions("Additional Supplementary Codes Provider Test", () =>
				{
					AssertEquals("First | TaxImmunityCode", "AHSKA", taxImmunitys[0].TaxImmunityCode);
					AssertEquals("Second | TaxImmunityCode", "BSİZ", taxImmunitys[1].TaxImmunityCode);
				});
			}
		}
	}
}
