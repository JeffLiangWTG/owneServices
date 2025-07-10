using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(OrgContactSelectionForm))]
	sealed class OrgContactSelectionFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new OrgContactSelectionForm(new FilteredContactsCollectionWrapper(new OrgContactCollection(Factory)));
		}

		[RequiresSTA]
		public void TestDefaultSelectedOrgContact()
		{
			OrgContact orgContact = Factory.NewWithValidTestData<OrgContact>();
			string email = "jason.zhu@wisetechglobal.com";
			orgContact.OC_Email = email;
			Factory.Save();

			var contactQuery = new ZQuery(OrgContactSchema.OC_Email, email);
			var contacts = new OrgContactCollection(Factory, contactQuery);
			contacts.Load();

			using (var form = new OrgContactSelectionFormForTesting(new FilteredContactsCollectionWrapper(contacts)))
			{
				AssertNull(form.SelectedOrgContact);
			}
		}

		[RequiresSTA]
		public void TestInactiveContactsCheckBoxChecked()
		{
			OrgContact orgContact1 = Factory.NewWithValidTestData<OrgContact>();
			string email1 = "jason.zhu@wisetechglobal.com";
			orgContact1.OC_Email = email1;
			orgContact1.OC_IsActive = false;

			OrgContact orgContact2 = Factory.NewWithValidTestData<OrgContact>();
			string email2 = "jason2.zhu@wisetechglobal.com";
			orgContact2.OC_Email = email2;
			orgContact2.OC_IsActive = true;

			Factory.Save();

			string[] emails = new string[] { email1, email2 };
			var contactQuery = new ZQuery(OrgContactSchema.OC_Email, emails);
			var contacts = new OrgContactCollection(Factory, contactQuery);
			contacts.Load();

			var wrapper = new FilteredContactsCollectionWrapper(contacts);
			wrapper.IncludeInactiveContacts = true;
			using (var form = new OrgContactSelectionFormForTesting(wrapper))
			{
				form.Show();
				AssertEquals(true, form.ShowInactiveContactsCheckBox.Checked);
				AssertEquals(2, ((FilteredContactsCollectionWrapper)form.ContactGrid.DataSource).Collection.Count);
			}
		}

		[RequiresSTA]
		public void TestInactiveContactsCheckBoxUnChecked()
		{
			OrgContact orgContact1 = Factory.NewWithValidTestData<OrgContact>();
			string email1 = "jason.zhu@wisetechglobal.com";
			orgContact1.OC_Email = email1;
			orgContact1.OC_IsActive = false;

			OrgContact orgContact2 = Factory.NewWithValidTestData<OrgContact>();
			string email2 = "jason2.zhu@wisetechglobal.com";
			orgContact2.OC_Email = email2;
			orgContact2.OC_IsActive = true;

			Factory.Save();

			string[] emails = new string[] { email1, email2 };
			var contactQuery = new ZQuery(OrgContactSchema.OC_Email, emails);
			var contacts = new OrgContactCollection(Factory, contactQuery);
			contacts.Load();

			var wrapper = new FilteredContactsCollectionWrapper(contacts);
			wrapper.IncludeInactiveContacts = false;
			using (var form = new OrgContactSelectionFormForTesting(wrapper))
			{
				form.Show();
				AssertEquals(false, form.ShowInactiveContactsCheckBox.Checked);
				AssertEquals(1, ((FilteredContactsCollectionWrapper)form.ContactGrid.DataSource).Collection.Count);
			}
		}
	}
}
