using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Global.GlbPerson;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(GlbPersonNewForm))]
	sealed class GlbPersonNewFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();
			return new GlbPersonNewForm(person);
		}

		public void TestIdentificationGroup()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();

			using (var form = new GlbPersonNewForm(person))
			{
				var identificationGroupBox = (ZGroupBox)form.Controls.Find("IdentificationGroupBox", true).FirstOrDefault();
				AssertNotNull(identificationGroupBox);

				AssertEquals("Should have 4 boxes", 4, identificationGroupBox.Controls.Count);
			}
		}

		public void TestPER_MobilePhone_FormattedValidation()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			using (person.GetValidationSuspender())
			{
				AssertEquals("Precondition", ZString.Empty, person.PER_MobilePhone_Formatted);
				AssertEquals("Precondition", ZString.Empty, person.PER_MobilePhone);
				AssertEquals("Precondition", ZString.Empty, person.PER_Passport);
				AssertEquals("Precondition", ZString.Empty, person.PER_EmailAddress);
				AssertEquals("Precondition", ZString.Empty, person.PER_DriversLicenseNumber);
			}

			using (var form = new GlbPersonNewForm(person))
			{
				form.Show();

				AssertNoErrors("Precondition", person.PER_PassportInfo);
				AssertNoErrors("Precondition", person.PER_MobilePhone_FormattedInfo);

				person.PER_EmailAddress = "a@b.com";
				person.PER_Gender = "M";

				form.FireSaveButton();
				AssertNoErrors("Should have no errors after save", person.PER_PassportInfo);
				AssertNoErrors("Should have no errors after save", person.PER_MobilePhone_FormattedInfo);

				var phoneUserControl = (PhoneNumberUserControl)form.Controls.Find("MobilePhoneTextBox", true).First();
				AssertNoErrors("Should have no errors after save", phoneUserControl.NumberTextBox.PhoneNumberProperty);

				var notificationExtension = phoneUserControl.NumberTextBox.Extensions.Get<NotificationExtension>();
				AssertEquals("Should not have any notifications", 0, notificationExtension.Notifications.Count());
			}
		}
	}
}
