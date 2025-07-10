using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(CusPackingListController))]
	public class CusPackingListControllerTestCase : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			var result = Factory.NewWithValidTestData(GetBusinessObjectType()) as CusPackingList;
			result.CUL_JE = declaration.PK;
			Factory.Save();
			return result;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.CusPackingList;
		}
	}
}
