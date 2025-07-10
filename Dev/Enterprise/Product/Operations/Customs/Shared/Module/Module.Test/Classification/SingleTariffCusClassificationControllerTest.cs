using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(SingleTariffClassificationController))]
	public class SingleTariffClassificationControllerTest : ZControllerBasherTest
	{
		public void TestModuleID()
		{
			SingleTariffClassificationController controller = new SingleTariffClassificationController();
			AssertEquals("controller.ModuleID", ModuleIDs.SingleTariffClassification, controller.ModuleID);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.SingleTariffClassification;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			BaseCusClassification testClass = Factory.New<BaseCusClassification>();
			testClass.CC_Description = "TestDescription";
			testClass.CC_LookupCode = "TestTest";
			testClass.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			Factory.Save();
			return testClass;
		}

		public void TestSecurityCheckPoints()
		{
			CombineAssertions(() => 
			{
				AssertEquals(Env.Security.CusClassification, Controller.CheckPointForViewExposedForTest);
				AssertEquals(Env.Security.CusClassificationNew, Controller.CheckPointForNewExposedForTest);
				AssertEquals(Env.Security.CusClassification, Controller.CheckPointForEditExposedForTest);
				AssertEquals(Env.Security.CusClassificationDelete, Controller.CheckPointForDeleteExposedForTest);
			});
		}
	}
}
