using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(BorderWiseWebReturnHookController))]
	sealed class BorderWiseWebReturnHookControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.BorderWiseWebReturnHook;
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new BorderWiseReturnHookNonPersistentBizo();
		}

		public override void TestNewForm()
		{
			Assert("This form is shown manually rather than from the module tree. It's only a module so we can access it via the edient hyperlink architecture, in order to support integration with BorderWise Web.", true);
		}

		public override void TestViewForm()
		{
			Assert("This form is shown manually rather than from the module tree. It's only a module so we can access it via the edient hyperlink architecture, in order to support integration with BorderWise Web.", true);
		}

		public override void TestEditForm()
		{
			Assert("This form is shown manually rather than from the module tree. It's only a module so we can access it via the edient hyperlink architecture, in order to support integration with BorderWise Web.", true);
		}

		public override void TestDeleteForm()
		{
			Assert("This form is shown manually rather than from the module tree. It's only a module so we can access it via the edient hyperlink architecture, in order to support integration with BorderWise Web.", true);
		}

		public void TestMakeUrlsOnlyOpenableForCurrentCompany_ShouldBeTrue_SoThatLicenceCodeIsIncludedInHyperlink()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			Assert(controller.MakeUrlsOnlyOpenableForCurrentCompany);
		}
	}
}
