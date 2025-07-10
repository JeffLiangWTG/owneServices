using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(GoodsCatalogController))]
	public class GoodsCatalogControllerTest : ZControllerBasherTest
	{
		public void TestSecurityCheckpoints()
		{
			var controller = GetNewGoodsCatalogController();
			CombineAssertions(() =>
			{
				AssertEquals("For View", Env.Security.GoodsCatalogView, controller.CheckPointForViewExposedForTest);
				AssertEquals("For Edit", Env.Security.GoodsCatalogEdit, controller.CheckPointForEditExposedForTest);
				AssertEquals("For New", Env.Security.GoodsCatalogNew, controller.CheckPointForNewExposedForTest);
				AssertEquals("For Delete", Env.Security.GoodsCatalogDelete, controller.CheckPointForDeleteExposedForTest);
			});
		}

		public void TestTopLevelBusinessObject()
		{
			var controller = GetNewGoodsCatalogController();
			AssertEquals(ExpectedBusinessObjectype, controller.TypeOfTopLevelBusinessObject);
		}

		public void TestGetForm()
		{
			var controller = GetNewGoodsCatalogController() as ZControllerInternals;
			using (var form = controller.GetForm(Factory.New<BaseCusGoodsCatalog>()))
			{
				AssertEquals(ExpectedFormType, form.GetType());
			}
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.GoodsCatalog;

		protected virtual GoodsCatalogController GetNewGoodsCatalogController() => new GoodsCatalogController();

		protected virtual Type ExpectedBusinessObjectype => typeof(BaseCusGoodsCatalog);

		protected virtual Type ExpectedFormType => typeof(CusGoodsCatalogForm);
	}
}
