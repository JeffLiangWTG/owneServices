using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class DeclarationInvoiceHeaderUserControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestControlVisibilityChangedCalled()
		{
			var mock = new Mock<DeclarationInvoiceHeaderUserControl>();
			mock.CallBase = true;
			using (DeclarationInvoiceHeaderUserControl userControl = mock.Object)
			{
				BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();

				userControl.JobDeclaration = declaration;

				mock.Protected().Setup("ChangeGridColumnsVisibility");
				mock.Protected().Setup("ChangeControlsVisibility");

				declaration.JE_MessageType = "CHA";

				mock.VerifyAll();
			}
		}
	}
}
