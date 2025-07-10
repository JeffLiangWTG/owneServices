using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Global.GlbPerson;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(PersonFormForTest))]
	sealed class PersonAssociationsControlTest : ZFormBasherTest
	{
		public void TestAttachButton()
		{
			Assert(true); // throw new NotImplementedException();
		}

		[RequiresSTA]
		public void TestDetachMultiple()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "name";
			person.PER_EmailAddress = "email@addr.ess";
			person.PER_BirthDate = new ZDate(2001, 1, 1);
			person.PER_Gender = Core.Constants.Genders.Man;

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "org name";
			org.OH_Code = "~code";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "contact1@email.com";
			contact1.OC_ContactName = "contact1";
			contact1.OC_PER = person.PK;
			contact1.OC_Birthday = new ZDateTime(2002, 1, 1);

			var contact2 = org.Contacts.AddNew();
			contact2.OC_Email = "contact2@email.com";
			contact2.OC_ContactName = "contact2";
			contact2.OC_PER = person.PK;
			contact2.OC_Birthday = new ZDateTime(2003, 1, 1);

			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_EmailAddress = "staff1@email.com";
			staff1.GS_FullName = "staff name 1";
			staff1.GS_PER = person.PK;
			staff1.GS_Birthdate = new ZDate(2004, 1, 1);
			staff1.GS_LoginName = "login1";

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_EmailAddress = "staff2@email.com";
			staff2.GS_FullName = "staff name 2";
			staff2.GS_PER = person.PK;
			staff2.GS_Birthdate = new ZDate(2005, 1, 1);
			staff1.GS_LoginName = "login2";

			var applicant1 = Factory.New<IHRJobApplicant>();
			applicant1.HA_FullName = "applicant 1";
			applicant1.HA_EmailAddress = "applicant1@email.com";
			applicant1.HA_Birthdate = new ZDate(2006, 1, 1);
			applicant1.HA_PER = person.PK;

			var applicant2 = Factory.New<IHRJobApplicant>();
			applicant2.HA_FullName = "applicant 2";
			applicant2.HA_EmailAddress = "applicant2@email.com";
			applicant2.HA_Birthdate = new ZDate(2007, 1, 1);
			applicant2.HA_PER = person.PK;

			person.SetPrimaryRelationship(contact1);

			person.IsMovingFromAnotherPerson = true; //to prevent data propagation
			Factory.Save();
			person.IsMovingFromAnotherPerson = false;

			var model = new PersonAssociationsTreeModel(person);

			using (var form = new PersonFormForTest(model))
			{
				form.Show();
				Application.DoEvents();

				foreach (var node in form.PersonAssociationsControl.Tree.AllNodes)
				{
					var staffWrapper = ((PersonAssociationsTreeNode)node.Tag).BizObj as PersonAssociationsStaffWrapper;
					var applicantWrapper = ((PersonAssociationsTreeNode)node.Tag).BizObj as PersonAssociationsJobApplicantWrapper;
					var contactWrapper = ((PersonAssociationsTreeNode)node.Tag).BizObj as PersonAssociationsOrganizationWrapper;

					if ((staffWrapper != null && staffWrapper.Staff.PK == staff1.PK)
						|| (applicantWrapper != null && applicantWrapper.JobApplicant.PK == applicant1.PK)
						|| (contactWrapper != null && contactWrapper.Contact.PK == contact1.PK))
					{
						node.IsSelected = true;
					}
				}

				AssertEquals(3, form.PersonAssociationsControl.Tree.SelectedNodes.Count);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PersonAssociationsControl.DetachToolStripButton.PerformClick();

				AssertEquals("You are about to detach the selected association(s) and attach them to a new Person. Would you like to continue?", UnitTestUserNotification.Instance.LastMessage.Text);

				GlbPersonNewForm newPersonForm = null;
				for (int i = 0; i < 10; i++)
				{
					newPersonForm = ZFormModaliser.LastFormShownForTest as GlbPersonNewForm;
					if (newPersonForm != null)
					{
						break;
					}
				}

				AssertNotNull(newPersonForm);

				newPersonForm.FireSaveButton();
				newPersonForm.Close();
				newPersonForm.Dispose();

				ZFormModaliser.LastFormShownForTest.Close();
				ZFormModaliser.LastFormShownForTest.Dispose();

				AssertEquals(1, person.ApplicantCollection.Count);
				var applicant = person.ApplicantCollection.First() as IHRJobApplicant;
				AssertEquals(applicant2.PK, applicant.PK);

				person.ContactCollection.Load();
				AssertEquals(1, person.ContactCollection.Count);
				AssertEquals(contact2.PK, person.ContactCollection[0].PK);

				AssertEquals(1, person.StaffCollection.Count);
				AssertEquals(staff2.PK, person.StaffCollection[0].PK);

				form.FireSaveButton();

				AssertEquals(1, person.ApplicantCollection.Count);
				AssertEquals(1, person.ContactCollection.Count);
				AssertEquals(1, person.StaffCollection.Count);

				var newPerson = staff1.Person;
				AssertNotEquals(newPerson.PK, staff2.GS_PER);

				AssertEquals(1, newPerson.ApplicantCollection.Count);
				var newApplicant = newPerson.ApplicantCollection.First() as IHRJobApplicant;
				AssertEquals(applicant1.PK, newApplicant.PK);
				AssertEquals("name", newApplicant.HA_FullName);
				AssertEquals("applicant1@email.com", newApplicant.HA_EmailAddress);
				AssertEquals(new ZDate(2001, 1, 1), newApplicant.HA_Birthdate);

				AssertEquals(1, newPerson.ContactCollection.Count);
				AssertEquals(contact1.PK, newPerson.ContactCollection[0].PK);
				AssertEquals("name", newPerson.ContactCollection[0].OC_ContactName);
				AssertEquals("contact1@email.com", newPerson.ContactCollection[0].OC_Email);
				AssertEquals(new ZDate(2001, 1, 1), newPerson.ContactCollection[0].OC_Birthday);

				AssertEquals(1, newPerson.StaffCollection.Count);
				AssertEquals(staff1.PK, newPerson.StaffCollection[0].PK);
				AssertEquals("name", newPerson.StaffCollection[0].GS_FullName);
				AssertEquals("staff1@email.com", newPerson.StaffCollection[0].GS_EmailAddress);
				AssertEquals(new ZDate(2001, 1, 1), newPerson.StaffCollection[0].GS_Birthdate);

				AssertHasRowError(person, "Please set the person's primary workplace.");
			}
		}

		public void TestNewButton()
		{
			using (PersonFormForTest form = GetFormToBashCore() as PersonFormForTest)
			{
				form.Show();

				Application.DoEvents();

				AssertArrayEqualsByElements(
					new[] { "New Staff", "New Applicant" },
					form.PersonAssociationsControl.NewToolStripDropDownButton.DropDownItems.Cast<ToolStripItem>().Select(item => item.Text).ToArray());
			}
		}

		public void TestNewStaff()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "name";

			var jobApplicant = Factory.New<IHRJobApplicant>();
			jobApplicant.HA_FullName = "name";
			jobApplicant.HA_PER = person.PK;

			var model = new PersonAssociationsTreeModel(person);

			using (PersonFormForTest form = new PersonFormForTest(model))
			{
				form.Show();

				Application.DoEvents();

				var button = form.PersonAssociationsControl.NewToolStripDropDownButton.DropDownItems[0];
				AssertNotNull(button);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

				button.PerformClick();

				var controller = (form.PersonAssociationsControl as PersonAssociationsControlForTest).StaffController;
				AssertNotNull(controller);

				AssertEquals(typeof(GlbStaffForm), controller.LastShownForm.GetType());
				controller.LastShownForm.Dispose();
			}
		}

		public void TestNewStaff_AlreadyExists()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "name";

			var jobApplicant = Factory.New<IHRJobApplicant>();
			jobApplicant.HA_FullName = "name";
			jobApplicant.HA_PER = person.PK;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_City = "Sydney";
			staff.GS_FullName = "name";
			staff.GS_PER = person.PK;

			var model = new PersonAssociationsTreeModel(person);

			using (PersonFormForTest form = new PersonFormForTest(model))
			{
				form.Show();

				Application.DoEvents();

				var button = form.PersonAssociationsControl.NewToolStripDropDownButton.DropDownItems[0];
				AssertNotNull(button);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				button.PerformClick();

				var controller = (form.PersonAssociationsControl as PersonAssociationsControlForTest).StaffController;
				AssertNull(controller);
				AssertEquals("There is already a Staff for this Person.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestNewApplicant()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "name";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_City = "Sydney";
			staff.GS_FullName = "name";
			staff.GS_PER = person.PK;

			var model = new PersonAssociationsTreeModel(person);

			using (PersonFormForTest form = new PersonFormForTest(model))
			{
				form.Show();

				Application.DoEvents();

				var button = form.PersonAssociationsControl.NewToolStripDropDownButton.DropDownItems[1];
				AssertNotNull(button);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

				button.PerformClick();

				var controller = (form.PersonAssociationsControl as PersonAssociationsControlForTest).ApplicantController;
				AssertNotNull(controller);

				AssertEquals("Enterprise.Recruiter.GUI.HRJobApplicantForm", controller.LastShownForm.GetType().ToString());
				controller.LastShownForm.Dispose();
			}
		}

		public void TestNewApplicant_AlreadyExists()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "name";

			var jobApplicant = Factory.New<IHRJobApplicant>();
			jobApplicant.HA_FullName = "name";
			jobApplicant.HA_PER = person.PK;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_City = "Sydney";
			staff.GS_FullName = "name";
			staff.GS_PER = person.PK;

			var model = new PersonAssociationsTreeModel(person);

			using (PersonFormForTest form = new PersonFormForTest(model))
			{
				form.Show();

				Application.DoEvents();

				var button = form.PersonAssociationsControl.NewToolStripDropDownButton.DropDownItems[1];
				AssertNotNull(button);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				button.PerformClick();

				var controller = (form.PersonAssociationsControl as PersonAssociationsControlForTest).StaffController;
				AssertNull(controller);
				AssertEquals("There is already an Applicant for this Person.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestNewStaffPrimary()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "name";

			var jobApplicant = Factory.New<IHRJobApplicant>();
			jobApplicant.HA_FullName = "name";
			jobApplicant.HA_PER = person.PK;
			Factory.Save();

			var model = new PersonAssociationsTreeModel(person);

			using (PersonFormForTest form = new PersonFormForTest(model))
			{
				form.Show();

				Application.DoEvents();

				var button = form.PersonAssociationsControl.NewToolStripDropDownButton.DropDownItems[0];
				AssertNotNull(button);

				button.PerformClick();

				var controller = (form.PersonAssociationsControl as PersonAssociationsControlForTest).StaffController;
				AssertNotNull(controller);

				AssertEquals(typeof(GlbStaffForm), controller.LastShownForm.GetType());
				var staffForm = (GlbStaffForm)controller.LastShownForm;
				var staff = (GlbStaff)staffForm.BusinessEntity;
				SetupStaff(staff, branch, department);

				staffForm.FireSaveButton();
				staff.Factory.Save();
				AssertEquals("Staff should be set as Primary through trigger", staff.PK, person.PrimaryRelationship.PPR_PrimaryId);

				controller.LastShownForm.Dispose();
			}
		}

		public void TestNewStaffWithContactPrimary()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "name";

			var jobApplicant = Factory.New<IHRJobApplicant>();
			jobApplicant.HA_FullName = "name";
			jobApplicant.HA_PER = person.PK;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "name";
			contact.OC_PER = person.PK;
			contact.OC_OA_OrgAddress = address.PK;
			address.OA_CompanyNameOverride = "Bass";

			person.SetPrimaryRelationship(contact);

			Factory.Save();

			var model = new PersonAssociationsTreeModel(person);

			using (var form = new PersonFormForTest(model))
			{
				form.Show();

				Application.DoEvents();

				var button = form.PersonAssociationsControl.NewToolStripDropDownButton.DropDownItems[0];
				AssertNotNull(button);

				button.PerformClick();

				var controller = (form.PersonAssociationsControl as PersonAssociationsControlForTest).StaffController;
				AssertNotNull(controller);

				AssertEquals(typeof(GlbStaffForm), controller.LastShownForm.GetType());
				var staffForm = (GlbStaffForm)controller.LastShownForm;
				var staff = (GlbStaff)staffForm.BusinessEntity;
				SetupStaff(staff, branch, department);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				staffForm.FireSaveButton();
				AssertEquals("Message should be triggered", "This Person's current Primary Workplace is Bass. Would you like to use the new staff's company as the Primary Workplace?", UnitTestUserNotification.Instance.LastMessage.Text);

				staffForm.Close();

				AssertEquals("Staff should be set as Primary", staff.PK, person.PrimaryRelationship.PPR_PrimaryId);

				controller.LastShownForm.Dispose();
			}
		}

		[RequiresSTA]
		public void TestNewStaffWithContactPrimaryCancel()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "name";

			var jobApplicant = Factory.New<IHRJobApplicant>();
			jobApplicant.HA_FullName = "name";
			jobApplicant.HA_PER = person.PK;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "name";
			contact.OC_PER = person.PK;
			contact.OC_OA_OrgAddress = address.PK;
			address.OA_CompanyNameOverride = "Bass";

			person.SetPrimaryRelationship(contact);

			Factory.Save();

			var model = new PersonAssociationsTreeModel(person);

			using (PersonFormForTest form = new PersonFormForTest(model))
			{
				form.Show();

				Application.DoEvents();

				var button = form.PersonAssociationsControl.NewToolStripDropDownButton.DropDownItems[0];
				AssertNotNull(button);

				button.PerformClick();

				var controller = (form.PersonAssociationsControl as PersonAssociationsControlForTest).StaffController;
				AssertNotNull(controller);

				AssertEquals(typeof(GlbStaffForm), controller.LastShownForm.GetType());
				var staffForm = (GlbStaffForm)controller.LastShownForm;
				var staff = (GlbStaff)staffForm.BusinessEntity;
				SetupStaff(staff, branch, department);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				staffForm.FireSaveButton();
				AssertEquals("Message should be triggered", "This Person's current Primary Workplace is Bass. Would you like to use the new staff's company as the Primary Workplace?", UnitTestUserNotification.Instance.LastMessage.Text);

				staffForm.Close();

				AssertEquals("Contact should still be the Primary", contact.PK, person.PrimaryRelationship.PPR_PrimaryId);

				controller.LastShownForm.Dispose();
			}
		}

		void SetupStaff(GlbStaff staff, GlbBranch branch, GlbDepartment department)
		{
			staff.GS_FullName = "Test P";
			staff.GS_Code = "TSP";
			staff.StaffPlainTextPassword = "1234";
			staff.GS_UserAddress1 = "1 O'Riordan St";
			staff.GS_City = "Sydney";
			staff.GS_LoginName = "test.p";
			staff.GS_RN_NKCountryCode = "AU";
			staff.StaffPlainTextPassword = "1234";
			staff.StaffConfirmPassword = "1234";
			staff.GS_GB_HomeBranch = branch.PK;
			staff.GS_GE_HomeDepartment = department.PK;
		}

		public void TestEditButton()
		{
			Assert(true); // throw new NotImplementedException();
		}

		public void TestShowInactiveCheckBox()
		{
			Assert(true); // throw new NotImplementedException();
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			const string OrganizationCountryCode1 = "AUSYD";
			const string OrganizationCountryCode2 = "USLAX";
			const string FullName = "John Smith";

			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = FullName;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_City = "Sydney";
			staff.GS_FullName = FullName;
			staff.GS_PER = person.PK;

			var jobApplicant = Factory.New<IHRJobApplicant>();
			jobApplicant.HA_FullName = FullName;
			jobApplicant.HA_PER = person.PK;

			var organizationParent1 = Factory.NewWithValidTestData<OrgHeader>();
			organizationParent1.OH_Code = "TSTORGPNT1";
			organizationParent1.OH_FullName = "Test Organization Parent 1";
			organizationParent1.OH_RL_NKClosestPort = OrganizationCountryCode1;

			var organizationParent2 = Factory.NewWithValidTestData<OrgHeader>();
			organizationParent2.OH_Code = "TSTORGPNT2";
			organizationParent2.OH_FullName = "Test Organization Parent 2";
			organizationParent2.OH_RL_NKClosestPort = OrganizationCountryCode2;

			var organization1 = Factory.NewWithValidTestData<OrgHeader>();
			organization1.OH_Code = "TSTORG1";
			organization1.OH_FullName = "Test Organization 1";
			organization1.OH_RL_NKClosestPort = OrganizationCountryCode1;

			var organization2 = Factory.NewWithValidTestData<OrgHeader>();
			organization2.OH_Code = "TSTORG2";
			organization2.OH_FullName = "Test Organization 2";
			organization2.OH_RL_NKClosestPort = OrganizationCountryCode1;

			var organization3 = Factory.NewWithValidTestData<OrgHeader>();
			organization3.OH_Code = "TSTORG3";
			organization3.OH_FullName = "Test Organization 3";
			organization3.OH_RL_NKClosestPort = OrganizationCountryCode2;

			var orgContact1 = Factory.NewWithValidTestData<OrgContact>();
			orgContact1.OC_ContactName = FullName;
			orgContact1.OC_OH = organizationParent1.PK;
			orgContact1.OC_PER = person.PK;

			var orgContact2 = Factory.NewWithValidTestData<OrgContact>();
			orgContact2.OC_ContactName = FullName;
			orgContact2.OC_OH = organizationParent2.PK;
			orgContact2.OC_PER = person.PK;

			var orgContact3 = Factory.NewWithValidTestData<OrgContact>();
			orgContact3.OC_ContactName = FullName;
			orgContact3.OC_OH = organization1.PK;
			orgContact3.OC_PER = person.PK;

			var orgContact4 = Factory.NewWithValidTestData<OrgContact>();
			orgContact4.OC_ContactName = FullName;
			orgContact4.OC_OH = organization2.PK;
			orgContact4.OC_PER = person.PK;

			var orgContact5 = Factory.NewWithValidTestData<OrgContact>();
			orgContact5.OC_ContactName = FullName;
			orgContact5.OC_OH = organization3.PK;
			orgContact5.OC_PER = person.PK;

			var orgRelatedParty1 = Factory.NewWithValidTestData<OrgRelatedParty>();
			orgRelatedParty1.PR_OH_Parent = organizationParent1.PK;
			orgRelatedParty1.PR_OH_RelatedParty = organization1.PK;

			var orgRelatedParty2 = Factory.NewWithValidTestData<OrgRelatedParty>();
			orgRelatedParty2.PR_OH_Parent = organizationParent1.PK;
			orgRelatedParty2.PR_OH_RelatedParty = organization2.PK;

			var orgRelatedParty3 = Factory.NewWithValidTestData<OrgRelatedParty>();
			orgRelatedParty3.PR_OH_Parent = organizationParent1.PK;
			orgRelatedParty3.PR_OH_RelatedParty = organization3.PK;

			var model = new PersonAssociationsTreeModel(person);

			return new PersonFormForTest(model);
		}

		public class PersonFormForTest : ZForm
		{
			public PersonFormForTest(PersonAssociationsTreeModel model)
				: base(model)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 768);
				Controls.Add(PersonAssociationsControl);
				BindingSource.SetBindingMember(PersonAssociationsControl, ".");
				CaptionRenderingEnabled = true;
			}

			public readonly PersonAssociationsControl PersonAssociationsControl = new PersonAssociationsControlForTest();
		}

		public class PersonAssociationsControlForTest : PersonAssociationsControl
		{
			public ZController StaffController;

			protected override ZController GetStaffController()
			{
				return StaffController = base.GetStaffController();
			}

			public ZController ApplicantController;

			protected override ZController GetApplicantController()
			{
				return ApplicantController = base.GetApplicantController();
			}
		}

		#endregion
	}
}
