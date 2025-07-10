using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

class CarModelFindBoxTest : TestCaseWithFactory
{
	public void TestCarModelFindBox()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		var invLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		using (var form = new ZForm())
		{
			var findBox = new CarModelFindBox(invLine, form);
			form.Show();

			var allCarModelsFilterDefaults = findBox.allCarModels.FilterBusinessObjectDefaults;
			AssertEquals(2, allCarModelsFilterDefaults.Count);
			AssertEquals(true, allCarModelsFilterDefaults.ContainsDefaultFor("List Type:Property"));
			AssertEquals(true, allCarModelsFilterDefaults.ContainsDefaultFor("Effective Date:Property1"));
		}
	}
}
