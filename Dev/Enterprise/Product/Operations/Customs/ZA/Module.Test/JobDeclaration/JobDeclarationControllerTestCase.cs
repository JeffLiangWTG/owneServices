using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.Module.Testing
{
	sealed class JobDeclarationControllerTestCase : TestCaseWithFactory
	{
		public void TestMessageTypeListHasEXW()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			Factory.Save();
			var controller = new JobDeclarationController();
			try
			{
				controller.ShowEditForm(declaration);
				Assert("Should have EXW", ((BaseJobDeclaration)((ZForm)controller.LastShownForm).BusinessEntity).Lookups.MessageTypeList.ContainsCode(Customs.Business.JobMessageTypeList.Codes.ExWarehouse));
			}
			finally
			{
				if (controller.LastShownForm != null)
				{
					controller.LastShownForm.Dispose();
				}
			}
		}
	}
}
