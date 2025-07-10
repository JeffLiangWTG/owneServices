using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	sealed class MiscOptionsUserControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions()]
		public void TestMiscOptionsUserControlTest()
		{
			var detailsUserControl = new MiscOptionsUserControl();
			detailsUserControl.Dispose();
		}

		public void TestPaidByDropEditNotVisible()
		{
			using (var userControl = new MiscOptionsUserControl())
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				userControl.SetDataBinding(declaration, "");
				var paidByDropEdit = userControl.FindSingle<ZDropEdit>("PaidByDropEdit");
				AssertEquals("PaidByDropEdit should not be visible for SG", false, paidByDropEdit.Visible);
			}
		}
	}
}
