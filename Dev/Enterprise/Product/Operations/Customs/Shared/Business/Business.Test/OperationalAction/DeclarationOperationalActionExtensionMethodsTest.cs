using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Business.Testing
{
	sealed class DeclarationOperationalActionExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestGetDeclarationIdLink()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "B000006";
			AssertEquals(ControllerIDs.Customs.JobDeclaration, declaration.GetDeclarationIdLink().Controller);
		}
	}
}
