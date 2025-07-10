using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.GUI.Testing.ContactsUserControlTest;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrganisationController))]
	public class OrganisationControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			OrgHeader org = factory.New<OrgHeader>();
			org.OH_FullName = "Name";
			org.MainAddress.OA_Address1 = "Address";
			org.OH_RL_NKClosestPort = "USLAX";

			factory.Save();

			return org;
		}

		public void TestOrganisationController_UrlsCanBeOpenedByAnyCompany()
		{
			AssertEquals("Organisation Hyperlinks should not be restricted to the current Company", false, Controller.MakeUrlsOnlyOpenableForCurrentCompany);
		}

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var orgHeader = base.GetBusinessObjectWithoutValidationErrors() as OrgHeader;
			orgHeader.MainAddress.OA_PostCode = "123456";
			return orgHeader;
		}

		public void TestSecurityCheckpoints()
		{
			var controller = new OrganisationControllerForTest();
			AssertEquals("For New", Env.Security.OrganisationNew, controller.CheckPointForNew);
			AssertEquals("For View", Env.Security.OrganisationView, controller.CheckPointForView);
			AssertEquals("For Edit", Env.Security.OrganisationModify, controller.CheckPointForEdit);
			AssertEquals("For Delete", Env.Security.OrganisationDelete, controller.CheckPointForDelete);
		}

		public void TestCRMSecurityCheckpoints()
		{
			var bizObj = Factory.NewWithValidTestData<OrgHeader>();
			bizObj.MiscServ.OM_GG_OrgSecurityGroup = Factory.NewWithValidTestData<GlbGroup>().PK;
			CRMSecurityProviderTest<OrgHeader>.AssertController(new OrganisationControllerForTest(), bizObj, Env.Security.OrganisationCRMSecurity);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Organisation;
		}

		public void TestEditFormNotShownIfEditingUnmatchedOrg()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			OrgHeader newHeader = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
			AssertShowForm(newHeader, OrganisationTabPages.Details, FormAction.Edit, false);
			AssertShowForm(newHeader, OrganisationTabPages.Details, FormAction.View, false);
			AssertShowForm(newHeader, OrganisationTabPages.Details, FormAction.Delete, false);
			AssertShowForm(newHeader, OrganisationTabPages.Address, FormAction.Edit, false);
			AssertShowForm(newHeader, OrganisationTabPages.Address, FormAction.View, false);
			AssertShowForm(newHeader, OrganisationTabPages.Contacts, FormAction.Delete, false);

			OrgHeader unmatchedOrg = Factory.Load<OrgHeader>(OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AssertShowForm(unmatchedOrg, OrganisationTabPages.Details, FormAction.Edit, true);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AssertShowForm(unmatchedOrg, OrganisationTabPages.Details, FormAction.View, true);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AssertShowForm(unmatchedOrg, OrganisationTabPages.Details, FormAction.Delete, true);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AssertShowForm(unmatchedOrg, OrganisationTabPages.Address, FormAction.Edit, true);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AssertShowForm(unmatchedOrg, OrganisationTabPages.Address, FormAction.View, true);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AssertShowForm(unmatchedOrg, OrganisationTabPages.Contacts, FormAction.Delete, true);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			using (TestOrganisationModule moduleForTest = new TestOrganisationModule())
			{
				using (ZOrganisationsForm orgForm = (ZOrganisationsForm)moduleForTest.ShowEditForm(unmatchedOrg))
				{
					AssertNull("OrgForm not shown - Responded No to showing View form", orgForm);
				}
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		void AssertShowForm(OrgHeader orgToDisplay, OrganisationTabPageType tabPageToDisplay, FormAction formDisplayMode, bool checkShouldBeReadOnly)
		{
			using (TestOrganisationModule moduleForTest = new TestOrganisationModule())
			{
				using (ZOrganisationsForm orgForm = (ZOrganisationsForm)new OrganisationController().ShowForm(orgToDisplay, tabPageToDisplay, formDisplayMode))
				{
					orgForm.Show();

					AssertFormDisplayMode(orgForm, (checkShouldBeReadOnly ? FormAction.View : formDisplayMode));

					ZTemplateTabControl orgTabControl = (ZTemplateTabControl)FindControlByName(orgForm, "OrganisationsTabControl");
					AssertEquals(tabPageToDisplay.Name, orgTabControl.SelectedTab.Name);
				}
			}
		}

		void AssertFormDisplayMode(ZOrganisationsForm form, FormAction expectedFormMode)
		{
			ODisplayMode displayMode = ODisplayMode.Undefined;

			if (expectedFormMode == FormAction.View)
			{
				displayMode = ODisplayMode.ReadOnly;
			}
			else if (expectedFormMode == FormAction.Edit)
			{
				displayMode = ODisplayMode.Browse;
			}
			else if (expectedFormMode == FormAction.Delete)
			{
				displayMode = ODisplayMode.Delete;
			}

			AssertEquals("Form should be in " + expectedFormMode.ToString() + " mode", displayMode, form.DisplayMode);
		}

		Control FindControlByName(Control outerControl, string name)
		{
			Control result = null;
			foreach (Control ctrl in outerControl.Controls)
			{
				if (ctrl.Name == name)
				{
					result = ctrl;
					break;
				}
				result = FindControlByName(ctrl, name);
				if (result != null)
				{
					break;
				}
			}
			return result;
		}

		[RequiresSTA]
		public void TestShowFormWithInitialTabPreSet()
		{
			OrgHeader org = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
			using (TestOrganisationModule moduleForTest = new TestOrganisationModule())
			{
				//Edit form
				OrganisationControllerForTest controller = new OrganisationControllerForTest();
				using (ZOrganisationsForm form = (ZOrganisationsForm)controller.ShowEditForm(org))
				{
					form.Show();
					ZTemplateTabControl orgTabControl = (ZTemplateTabControl)FindControlByName(form, "OrganisationsTabControl");
					AssertEquals(OrganisationTabPages.Details.Name, orgTabControl.SelectedTab.Name);
				}
				controller.SetInitialTabPageNameToSelectWhenAFormIsShown_Exposed(OrganisationTabPages.Contacts.Name);
				using (ZOrganisationsForm form = (ZOrganisationsForm)controller.ShowEditForm(org))
				{
					form.Show();
					ZTemplateTabControl orgTabControl = (ZTemplateTabControl)FindControlByName(form, "OrganisationsTabControl");
					AssertEquals(OrganisationTabPages.Contacts.Name, orgTabControl.SelectedTab.Name);
					form.Close();
				}

				//Delete form
				controller = new OrganisationControllerForTest();
				using (ZOrganisationsForm form = (ZOrganisationsForm)controller.ShowDeleteForm(org))
				{
					form.Show();
					ZTemplateTabControl orgTabControl = (ZTemplateTabControl)FindControlByName(form, "OrganisationsTabControl");
					AssertEquals(OrganisationTabPages.Details.Name, orgTabControl.SelectedTab.Name);
				}
				controller.SetInitialTabPageNameToSelectWhenAFormIsShown_Exposed(OrganisationTabPages.Contacts.Name);
				using (ZOrganisationsForm form = (ZOrganisationsForm)controller.ShowDeleteForm(org))
				{
					form.Show();
					ZTemplateTabControl orgTabControl = (ZTemplateTabControl)FindControlByName(form, "OrganisationsTabControl");
					AssertEquals(OrganisationTabPages.Contacts.Name, orgTabControl.SelectedTab.Name);
					form.Close();
				}

				//View form
				controller = new OrganisationControllerForTest();
				using (ZOrganisationsForm form = (ZOrganisationsForm)controller.ShowViewForm(org))
				{
					form.Show();
					ZTemplateTabControl orgTabControl = (ZTemplateTabControl)FindControlByName(form, "OrganisationsTabControl");
					AssertEquals(OrganisationTabPages.Details.Name, orgTabControl.SelectedTab.Name);
				}
				controller.SetInitialTabPageNameToSelectWhenAFormIsShown_Exposed(OrganisationTabPages.Contacts.Name);
				using (ZOrganisationsForm form = (ZOrganisationsForm)controller.ShowViewForm(org))
				{
					form.Show();
					ZTemplateTabControl orgTabControl = (ZTemplateTabControl)FindControlByName(form, "OrganisationsTabControl");
					AssertEquals(OrganisationTabPages.Contacts.Name, orgTabControl.SelectedTab.Name);
					form.Close();
				}
			}
		}

		#region TestShowEditFormForCanceledEntity

		public void TestShowEditFormForCancelableEntity()
		{
			OrgHeader dummy = Factory.NewWithValidTestData<OrgHeader>();
			dummy.OH_Code = "ttt";
			Factory.Save();

			OrganisationControllerForTest testController = new OrganisationControllerForTest();

			try
			{
				dummy.OH_IsActive = false;
				Factory.Save();
				testController.ShowEditForm(dummy);
				AssertEquals("DisplayMode", ODisplayMode.Browse, testController.LastShownForm.DisplayMode);
			}
			finally
			{
				testController.LastShownForm.Dispose();
			}

			try
			{
				dummy.OH_IsActive = true;
				Factory.Save();
				testController.ShowEditForm(dummy);
				AssertEquals("DisplayMode", ODisplayMode.Browse, testController.LastShownForm.DisplayMode);
			}
			finally
			{
				testController.LastShownForm.Dispose();
			}
		}

		#endregion

		[RequiresSTA]
		public void TestShowViewFormCotnactsFilterNotReadonly()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ttt";
			Factory.Save();

			var controller = new OrganisationControllerForTestContactsControl();

			using (controller.ShowViewForm(org))
			{
				controller.TabControl.SelectTab("Contact");

				AssertEquals(false, controller.ContactsControl.ContactsFilterStringTextBox.ReadOnly);
				AssertEquals(false, controller.ContactsControl.ShowInactiveContactsCheckBox.ReadOnly);
			}
		}

		class OrganisationControllerForTest : OrganisationController
		{
			public void SetInitialTabPageNameToSelectWhenAFormIsShown_Exposed(string name)
			{
				SetInitialTabPageNameToSelectWhenAFormIsShown(name);
			}

			public new SecurityCheckpoint CheckPointForNew
			{
				get { return base.CheckPointForNew; }
			}

			public new SecurityCheckpoint CheckPointForView
			{
				get { return base.CheckPointForView; }
			}

			public new SecurityCheckpoint CheckPointForEdit
			{
				get { return base.CheckPointForEdit; }
			}

			public new SecurityCheckpoint CheckPointForDelete
			{
				get { return base.CheckPointForDelete; }
			}
		}

		class OrganisationControllerForTestContactsControl : OrganisationController
		{
			public MockContactsPageControl ContactsControl { get; set; }
			public TabControl TabControl { get; set; }

			protected override IZForm GetForm(IBusiness businessEntity)
			{
				var form = new ZForm(businessEntity);
				ContactsControl = new MockContactsPageControl();

				TabControl = new ZTabControl();
				TabControl.TabPages.Add(new ZTabPage());
				var contactTabPages = new ZTabPage();
				TabControl.TabPages.Add(contactTabPages);
				contactTabPages.Name = "Contact";
				contactTabPages.RunWhenBindingOrFirstShown((sender, e) =>
				{
					contactTabPages.Controls.Add(ContactsControl);
				});

				form.Controls.Add(TabControl);

				return form;
			}
		}
	}
}
