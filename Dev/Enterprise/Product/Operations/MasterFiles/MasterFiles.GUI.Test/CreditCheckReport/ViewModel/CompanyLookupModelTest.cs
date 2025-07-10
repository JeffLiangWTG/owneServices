using System.Linq;
using NUnit.Framework;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI.Tests
{
	class CompanyLookupModelTest : TestCase
	{
		public void TestCompanyLookupModel()
		{
			var winFormModel = new CompanyLookupModel(new[] { new ResponseCompanyItem { Name = "TestCompany" } });
			CombineAssertions(() =>
			{
				AssertEquals("Confirm the organization", winFormModel.Title);
				AssertEquals("Confirm", winFormModel.Confirm);
				AssertEquals("Cancel", winFormModel.Cancel);
				AssertEquals("TestCompany", winFormModel.CompanyLookupItemModels.FirstOrDefault()?.CompanyItem.Name);
			});
		}
	}
}
