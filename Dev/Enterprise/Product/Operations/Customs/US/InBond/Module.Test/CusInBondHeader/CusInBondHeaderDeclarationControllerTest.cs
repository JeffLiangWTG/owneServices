using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Module.Testing
{
	[TestedType(typeof(CusInBondHeaderDeclarationController))]
	sealed class CusInBondHeaderDeclarationControllerTest : ZControllerBasherTest
	{
		public void TestLastSavedPK()
		{
			var header = GetBusinessObjectThatIsInTheDatabase();
			using (Controller.ShowEditForm(header))
			{
				AssertEquals("Declaration form is shown for PlugIn Dec", typeof(JobDeclarationForm), Controller.LastShownForm.GetType());
				AssertEquals("Dec PK should be returned though", Controller.LastSavedPK, header.PK);
			}
		}

		public void TestOpenDeclarationFormFromInBondModuleForPlugInInBond()
		{
			using (Controller.ShowEditForm(GetBusinessObjectThatIsInTheDatabase()))
			{
				AssertEquals("Declaration form is shown for PlugIn In-Bond", typeof(JobDeclarationForm), Controller.LastShownForm.GetType());
			}
		}

		public void TestShowInBondTabWhenDeclarationFormIsOpened()
		{
			Controller.ShowEditForm(GetBusinessObjectThatIsInTheDatabase());
			using (var form = Controller.LastShownForm as JobDeclarationForm)
			{
				CustomsBrokerageUserControl brokerageControl = null;

				foreach (var control in form.Controls)
				{
					brokerageControl = control as CustomsBrokerageUserControl;
					if (brokerageControl != null)
					{
						break;
					}
				}

				UserIdleWorker.Flush();

				Assert("Selected plugIn ID", brokerageControl.MainTabControl.SelectedTab.Controls[0] is GUI.USInBondUserControl);
			}
		}

		public void TestFormCacheShipmentFormFromDeclarationModule()
		{
			var testInBond = GetBusinessObjectThatIsInTheDatabase() as CusInBondHeader;
			var declaration = (JobDeclaration)testInBond.Parent;

			var declarationController = ZControllerFactory.Create(ControllerIDs.Customs.JobDeclaration);

			try
			{
				Controller.ShowEditForm(testInBond);
				declarationController.ShowEditForm(declaration);

				AssertEquals("Declaration form opened for testInBond is also shown for 'Declaration'", Controller.LastShownForm, declarationController.LastShownForm);
			}
			finally
			{
				if (Controller.LastShownForm != null)
				{
					Controller.LastShownForm.Dispose();
				}

				if (declarationController.LastShownForm != null)
				{
					declarationController.LastShownForm.Dispose();
				}
			}
		}

		public void TestSkipRecentItems()
		{
			var inBond = (CusInBondHeader)GetBusinessObjectThatIsInTheDatabase();
			Factory.Save();

			var controller = new CusInBondHeaderDeclarationController();
			try
			{
				var form = (JobDeclarationForm)controller.ShowEditForm(inBond);
				AssertNull(form.SkipRecentItems);

				OpenedFormCache.GetInstance().CloseAllCachedForms();
				form = (JobDeclarationForm)controller.ShowLoadedForm(inBond, FormAction.Edit, true);
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
			var inBond = Factory.New<CusInBondHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			inBond.BH_ParentID = declaration.PK;
			inBond.BH_ParentTableCode = declaration.TablePrefix;
			Factory.Save();
			return inBond;
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.US.InBondPluggedIntoDeclaration;
	}
}
