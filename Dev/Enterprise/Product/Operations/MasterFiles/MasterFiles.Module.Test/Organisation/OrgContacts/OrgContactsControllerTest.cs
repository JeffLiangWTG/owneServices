using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgContactsController))]
	sealed class OrgContactsControllerTest : ZControllerBasherTest
	{
		public void TestModuleID()
		{
			ZController controller = ZControllerFactory.Create(ControllerIDs.OrgContacts);
			AssertEquals(ModuleIDs.OrgContacts, controller.ModuleID);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			OrgHeader org = TestHeaders.AddNew();
			org.OH_FullName = "Name";
			org.MainAddress.OA_Address1 = "Address";
			org.OH_RL_NKClosestPort = "USLAX";

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Contact";

			Factory.Save();

			return contact;
		}

		OrgHeaderCollection TestHeaders
		{
			get
			{
				if (ftestHeaders == null)
				{
					ftestHeaders = new OrgHeaderCollection(Factory);
				}

				return ftestHeaders;
			}
		}
		OrgHeaderCollection ftestHeaders;

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.OrgContacts;
		}

		public override void TestNewForm()
		{
			Assert(true);
		}

		[ExpectNoExceptions()]
		public void TestShowNewForm()
		{
			ZController controller = ZControllerFactory.Create(ControllerIDs.OrgContacts);
			using (IZForm form = controller.ShowNewForm())
			{
				AssertNull("Should return null as Contact does not have a Header", form);
			}
		}

		public void TestGetForm()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "Vegetables";
			OrgContact contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "Lettuce";
			OrgContact contact2 = org1.Contacts.AddNew();
			contact2.OC_ContactName = "Tomato";

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "Fruits";
			OrgContact contact3 = org2.Contacts.AddNew();
			contact3.OC_ContactName = "Tomato";
			Factory.Save();

			ZController controller = ZControllerFactory.Create(ControllerIDs.OrgContacts);
			using (IZForm shownForm = controller.ShowViewForm(contact2))
			{
				ZOrganisationsForm orgForm = shownForm as ZOrganisationsForm;
				AssertEquals(ControllerIDs.Organisation, orgForm.ControllerID);
				AssertNotNull("ShownForm is OrgForm", orgForm);
				OrgHeader orgOnForm = orgForm.BusinessEntity as OrgHeader;
				AssertNotNull("BusinessEntity on ShownForm is OrgHeader", orgForm);
				AssertEquals("Correct organisation is loaded on the form", org1.OH_FullName, orgOnForm.OH_FullName);

				ContactsUserControl foundTab = FindControlByName(orgForm, "ContactsControl") as ContactsUserControl;
				AssertNotNull("Found the Contacts tab", foundTab);
				ZGrid foundGrid = FindControlByName(foundTab, "OrgContactBoundGrid") as ZGrid;
				AssertNotNull("Found the Contacts grid", foundGrid);
				OrgContact selectedContact = foundGrid.ListManager.GetCurrent() as OrgContact;
				AssertNotNull("A contact is selected", selectedContact);
				AssertEquals("Correct contact is selected!", contact2.OC_ContactName, selectedContact.OC_ContactName);
			}
		}

		public void TestOrgFormIsShown()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "Name";
			org.MainAddress.OA_Address1 = "Address";
			org.OH_RL_NKClosestPort = "USLAX";

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "CName";
			Factory.Save();

			ZController controller = ZControllerFactory.Create(ControllerIDs.OrgContacts);
			using (IZForm shownForm = controller.ShowEditForm(contact))
			{
				ZOrganisationsForm orgForm = shownForm as ZOrganisationsForm;
				AssertEquals(ControllerIDs.Organisation, orgForm.ControllerID);
				AssertNotNull("ShownForm is OrgForm", orgForm);

				ZTemplateTabControl foundControl = FindControlByName(orgForm, "OrganisationsTabControl") as ZTemplateTabControl;
				AssertNotNull("Found the Tab Control", foundControl);
				AssertEquals("The selected Index is the Contacts Tabpage", OrganisationTabPages.Contacts.Name, foundControl.SelectedTab.Name);
			}
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

		public void TestLastSavedPK()
		{
			OrgContact contact = (OrgContact)GetBusinessObjectThatIsInTheDatabase();

			ZController controller = ZControllerFactory.Create(ControllerIDs.OrgContacts);
			using (IZForm form = controller.ShowEditForm(contact))
			{
				AssertEquals(ControllerIDs.Organisation, form.ControllerID);
				AssertEquals("Contact PK should be persisted in the LastSavedPK property", contact.PK, controller.LastSavedPK);
			}
		}

		#region CRM Security

		public void TestCRMSecurityCheckpoints()
		{
			var bizObjWithoutAccess = Factory.NewWithValidTestData<OrgContact>();
			bizObjWithoutAccess.Header.MiscServ.OM_GG_OrgSecurityGroup = Factory.NewWithValidTestData<GlbGroup>().PK;
			CRMSecurityProviderTest<OrgContact>.AssertController(new OrgContactsController(), bizObjWithoutAccess, Env.Security.OrganisationCRMSecurity);
		}

		#endregion
	}
}
