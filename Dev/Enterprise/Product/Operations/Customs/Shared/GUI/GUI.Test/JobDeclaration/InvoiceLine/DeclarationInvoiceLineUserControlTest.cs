using Enterprise.Customs.Business;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class DeclarationInvoiceLineUserControlBaseOnlyTest : BaseInvoiceLineUserControlForVirtualPropertiesTest<DeclarationInvoiceLineUserControl>
	{
		[ExpectNoExceptions]
		public void TestControlVisibilityChangedCalled()
		{
			var mock = new Mock<DeclarationInvoiceLineUserControl>();
			mock.CallBase = true;
			using (DeclarationInvoiceLineUserControl userControl = mock.Object)
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
