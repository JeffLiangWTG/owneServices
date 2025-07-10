using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing.Organisation.Main.OrgCompanyData
{
	public class TransactionCreationRestrictionListTest : TestCaseWithFactory
	{
		public void TestCodeDescriptionPairs()
		{
			var list = new TransactionCreationRestrictionList();
			AssertEquals("Number of Types", 4, list.Count);
			AssertCodeAndDescription(list, "NON", "No Restriction");
			AssertCodeAndDescription(list, "ALL", "Restricted from creating New Transactions (All Transaction Types)");
			AssertCodeAndDescription(list, "INV", "Restricted from creating New Invoice, Credit Note and Adjustment Note");
			AssertCodeAndDescription(list, "BAL", "Restricted from creating Transactions that would cause the outstanding balance in local currency to increase in value");
		}

		void AssertCodeAndDescription(CodeDescriptionPairList list, ZString code, ZString description)
		{
			Assert("Code Should Exist", list.ContainsCode(code));
			AssertEquals("Description", description, list[code].Description);
		}
	}
}