using NUnit.Framework;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI.Tests
{
	class CompanyLookupItemModelTest : TestCase
	{
		public void TestCompanyLookupItemModel()
		{
			var identifiers = new[]
			{
				new Identifier { ID = "123", Type = IdentifierType.ABN },
				new Identifier { ID = "456", Type = IdentifierType.ACN },
				new Identifier { ID = "789", Type = IdentifierType.DUNS }
			};
			var companyItem = new ResponseCompanyItem
			{
				Country = "Dummy Test.",
				Identifiers = identifiers,
				Registered = true
			};

			var winFormModel = new CompanyLookupItemModel(companyItem) { Selected = true };
			AssertEquals(winFormModel.CompanyItem.Country, "Dummy Test.");
			AssertContainsExactElementsInExactOrder(identifiers, winFormModel.Identifiers);

			CombineAssertions(() =>
			{
				AssertEquals("123", winFormModel.ABN);
				AssertEquals("456", winFormModel.ACN);
				AssertEquals("789", winFormModel.DUNS);
				AssertEquals(true, winFormModel.Selected);
			});

			winFormModel.Selected = false;
			companyItem.Identifiers = null;
			companyItem.Registered = false;
			CombineAssertions(() =>
			{
				AssertNull(winFormModel.ABN);
				AssertNull(winFormModel.ACN);
				AssertNull(winFormModel.DUNS);
				AssertEquals(false, winFormModel.Selected);
			});
		}
	}
}
