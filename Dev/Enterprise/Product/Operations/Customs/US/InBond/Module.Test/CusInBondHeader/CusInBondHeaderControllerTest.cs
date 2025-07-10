using CargoWise.EntityFramework;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Customs.US.InBond.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Module.Testing
{
	[TestedType(typeof(CusInBondHeaderController))]
	sealed class CusInBondHeaderControllerTest : ZControllerBasherTest
	{
		public void TestOpenInBondFormForStandAlone()
		{
			var standAloneInBond = Factory.New<CusInBondHeader>();
			Controller.ShowEditForm(GetBusinessObjectThatIsInTheDatabase());
			Assert("In-Bond form is shown for stand-alone", Controller.LastShownForm is USInBondForm);
		}

		public void TestTabText()
		{
			var controller = new CusInBondHeaderController();

			AssertEquals("In-Bond", controller.PluginTabPageCaption.Caption);
		}

		public void TestSkipRecentItems()
		{
			var inBond = Factory.NewWithValidTestData<CusInBondHeader>();
			Factory.Save();

			var controller = new CusInBondHeaderController();
			try
			{
				var form = (USInBondForm)controller.ShowEditForm(inBond);
				AssertNull(form.SkipRecentItems);

				OpenedFormCache.GetInstance().CloseAllCachedForms();
				form = (USInBondForm)controller.ShowLoadedForm(inBond, FormAction.Edit, true);
				AssertNotNull(form.SkipRecentItems);
				Assert(form.SkipRecentItems.GetValueOrDefault());
			}
			finally
			{
				UserIdleWorker.Flush();

				if (controller.LastShownForm is IZForm lastShownForm)
				{
					lastShownForm.Dispose();
				}
			}
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var testInBond = Factory.New<CusInBondHeader>();
			Factory.Save();
			return testInBond;
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.US.InBond;
	}
}
