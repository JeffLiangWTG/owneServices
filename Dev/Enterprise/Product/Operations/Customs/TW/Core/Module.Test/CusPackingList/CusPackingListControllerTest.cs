using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.TW.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Module.Testing
{
	[TestedType(typeof(CusPackingListController))]
	sealed class CusPackingListControllerTest : Customs.Module.Testing.CusPackingListControllerTestCase
	{
		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals(typeof(CusPackingList), new CusPackingListController().TypeOfTopLevelBusinessObject);
		}

		public override Type ControllerToBashType => typeof(CusPackingListController);

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var result = Factory.NewWithValidTestData<CusPackingList>();
			result.CUL_JE = declaration.PK;
			Factory.Save();
			return result;
		}
	}
}
