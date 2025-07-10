using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Cache;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ContactPasswordEmail))]
	class ContactPasswordEmailTest : ContactPasswordEmailTestCase<ContactPasswordEmail>
	{
		public void TestGenerateRandomWebPassword()
		{
			string lastPassword = "";
			const string specialCharSet = " !\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";
			var minLength = 15;
			WebDataRegistry.Instance.WebPasswordMinLength.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, minLength);

			for (int i = 0; i < 5; i++) // Run 5 times to check for any problems with random pwd generation
			{
				string newPassword = ContactPasswordEmail.GenerateRandomWebPassword();
				AssertNotEquals("Contact password is now different", lastPassword, newPassword);
				AssertGreaterThanOrEqualTo("Contact password length should be greater than or equal to the minimum allowed length", newPassword.Length, minLength);
				lastPassword = newPassword;
				var hasUpperCaseLetter = false;
				var hasLowerCaseLetter = false;
				var hasNumeral = false;
				var hasSpecialCharacter = false;

				foreach (char character in newPassword)
				{
					bool charIsUpperCaseLetter = character >= 'A' && character <= 'Z';
					bool charIsLowerCaseLetter = character >= 'a' && character <= 'z';
					bool charIsNumber = character >= '0' && character <= '9';
					bool charIsSpecialCharacter = specialCharSet.IndexOf(character) != -1;

					hasUpperCaseLetter |= charIsUpperCaseLetter;
					hasLowerCaseLetter |= charIsLowerCaseLetter;
					hasNumeral |= charIsNumber;
					hasSpecialCharacter |= charIsSpecialCharacter;

					bool charIsValid = charIsUpperCaseLetter || charIsLowerCaseLetter || charIsNumber || charIsSpecialCharacter;
					Assert("Char must be an upper or lower case letter, or a number", charIsValid);
				}

				Assert("Password should have at least 1 upper case letter", hasUpperCaseLetter);
				Assert("Password should have at least 1 lower case letter", hasLowerCaseLetter);
				Assert("Password should have at least 1 numeric character", hasNumeral);
				Assert("Password should have at least 1 special character", hasSpecialCharacter);
				var (isValidPassword, _) = ContactPasswordValidator.IsValidPassword(null, newPassword);
				Assert("Password should pass validation", isValidPassword);
			}
		}

		public void TestSetDefaultEmailContent()
		{
			Env.Registry.AllowEmailsToBeSentFromUsersAddress = false;

			TestContact contact = Factory.New<TestContact>();
			contact.FillWithValidData();
			contact.Url = "http://testing.cargowise.com";
			contact.OrgCode = "CARGOWISE";
			contact.ExtraInstruction = "No more instruction.";
			EmailToContactBusinessObject email = new ContactPasswordEmail(contact);

			AssertEquals("EmailContactObject.FromDisplayName", GlbStaff.CurrentUser.GS_FullName, email.FromDisplayName);
			AssertEquals("EmailContactObject.FromAddress", Env.Registry.SMTPDefaultDoNotReplyEmailAddress, email.FromEmailAddress);
			AssertEquals("EmailContactObject.To", "Bluetooth", email.ToDisplayName);
			AssertEquals("EmailContactObject.ToEmailAddress", "bluetooth@cargowise.com", email.ToEmailAddress);
			AssertEquals("EmailContactObject.Subject", WebDataRegistry.Instance.EmailPasswordTemplate.Value.EmailSubject, email.Subject);

			AssertContains("EmailContactObject.Body contains Name", "Bluetooth", email.Body);
			AssertContains("EmailContactObject.Body contains Url", "http://testing.cargowise.com", email.Body);
			AssertContains("EmailContactObject.Body contains SiteName", "web site client area", email.Body);
			AssertContains("EmailContactObject.Body contains OrgCode", "CARGOWISE", email.Body);
			AssertContains("EmailContactObject.Body contains Email", "bluetooth@cargowise.com", email.Body);
			AssertContains("EmailContactObject.Body contains Password", "mehmeh", email.Body);
			AssertContains("EmailContactObject.Body contains ExtraInstruction", "No more instruction.", email.Body);
			AssertContains("EmailContactObject.Body contains CompanyName", GlbCompany.CurrentCompany.GC_Name, email.Body);
		}

		public void TestSetDefaultEmailContent_WithNullableGlbStaff()
		{
			TestContact contact = Factory.New<TestContact>();
			contact.FillWithValidData();
			contact.Url = "http://testing.cargowise.com";
			contact.Name = "";
			contact.Email = "somewhere@paradise.com";
			contact.OrgCode = "CARGOWISE";
			contact.ExtraInstruction = "No more instruction.";

			using (Env.SetTemporaryUserContext(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var email = new ContactPasswordEmail(contact);
				AssertEquals("EmailContactObject.FromDisplayName", "", email.FromDisplayName);
			}
		}

		public void TestEmailSubject()
		{
			var contact = Factory.New<TestContact>();
			contact.FillWithValidData();

			var email = new ContactPasswordEmail(contact);
			AssertEquals("EmailContactObject.Subject", WebDataRegistry.Instance.EmailPasswordTemplate.Value.EmailSubject, email.Subject);

			var overriddenEmailTemplate = WebDataRegistry.Instance.EmailPasswordTemplate.Value;
			contact.Salutation = "Hi Jenny";
			overriddenEmailTemplate.EmailSubject = "(*ContactSalutation*)";
			WebDataRegistry.Instance.EmailPasswordTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, overriddenEmailTemplate);
			email = new ContactPasswordEmail(contact);
			AssertEquals("EmailContactObject.Subject - Overridden", "Hi Jenny", email.Subject);
		}

		public void TestEmailBody()
		{
			var overriddenEmailTemplate = WebDataRegistry.Instance.EmailPasswordTemplate.Value;
			overriddenEmailTemplate.EmailBody = "Simple body";
			WebDataRegistry.Instance.EmailPasswordTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, overriddenEmailTemplate);

			var contact = Factory.New<TestContact>();
			contact.FillWithValidData();

			var email = new ContactPasswordEmail(contact);
			AssertEquals("EmailContactObject.Body", "Simple body", email.Body);

			contact.Name = "Jenny";
			contact.Email = "Jenny.Nguyen@wisetechglobal.com";
			overriddenEmailTemplate.EmailBody = "(*ContactName*) with username: (*UserName*)";
			WebDataRegistry.Instance.EmailPasswordTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, overriddenEmailTemplate);

			email = new ContactPasswordEmail(contact);
			AssertEquals("EmailContactObject.Body", "Jenny with username: Jenny.Nguyen@wisetechglobal.com", email.Body);
		}

		public void TestEmailSubjectLanguage()
		{
			var registryItem = WebDataRegistry.Instance.EmailPasswordTemplate;
			var emailTemplate = registryItem.Value;
			emailTemplate.EmailSubject = "Team";
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emailTemplate);

			string chsSubject = "小组";

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mocksChs = Res.UseMockData())
			{
				var captionSource = (IRegistryItemCaptionSource)registryItem;
				var keySubject = CustomizableDataResourceStrings.GetMultilingualString(captionSource, null, registryItem.Value.EmailSubject).ResourceKey;
				mocksChs.Put(keySubject, new ResourceStringData(keySubject, chsSubject));

				AssertEquals("Precondition", chsSubject, registryItem.Value.EmailSubject);

				var testContact = Factory.New<TestContact>();
				testContact.FillWithValidData();
				testContact.OC_Language = Core.SharedConstants.Languages.ChineseSimplified;
				var email = new ContactPasswordEmail(testContact);
				AssertEquals(chsSubject, email.Subject);
			}
		}

		public void TestEmailBodyLangauge()
		{
			var registryItem = WebDataRegistry.Instance.EmailPasswordTemplate;
			var emailTemplate = registryItem.Value;
			emailTemplate.EmailBody = @"Company Code: <br />
User Name: <br />
Password: <br /><br />";
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emailTemplate);

			string chsBody = @"公司代码： <br />
用户名： <br />
密码： <br /><br />";

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mocksChs = Res.UseMockData())
			{
				var captionSource = (IRegistryItemCaptionSource)registryItem;
				var keyBody = CustomizableDataResourceStrings.GetMultilingualString(captionSource, null, registryItem.Value.EmailBody).ResourceKey;
				mocksChs.Put(keyBody, new ResourceStringData(keyBody, chsBody));

				AssertEquals("Precondition", chsBody, registryItem.Value.EmailBody);

				var testContact = Factory.New<TestContact>();
				testContact.FillWithValidData();
				testContact.OC_Language = Core.SharedConstants.Languages.ChineseSimplified;
				var email = new ContactPasswordEmail(testContact);
				AssertEquals(chsBody, email.Body);
			}
		}

		protected override BusinessObject GetNewBusinessObjectSendingEmail()
		{
			return Factory.NewWithValidTestData<TestContact>();
		}
	}

	[TestsSubclassesOf(typeof(ContactPasswordEmail), ExcludePrivate = true)]
	public abstract class ContactPasswordEmailTestCase<T> : HtmlFormatEmailToContactBusinessObjectTestCase<T> where T : ContactPasswordEmail
	{
		#region Default Email Address / Display Name

		protected override string DefaultFromEmailAddress { get { return EnvProxy.Instance.Registry.SMTPDefaultDoNotReplyEmailAddress; } }
		protected override string DefaultFromDisplayName { get { return GlbStaff.CurrentUser.GS_FullName; } }

		#endregion Default Email Address / Display Name

		#region Implementation

		protected class TestContact : OrgContact, IPasswordEmailSource
		{
			public TestContact(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public virtual void FillWithValidData()
			{
				Name = "Bluetooth";
				Salutation = "Dear Bluetooth";
				Email = "bluetooth@cargowise.com";
				Mobile = "120938";
				Password = "mehmeh";
				IsActive = true;
			}

			#region IContactable Members

			public new string Name
			{
				set;
				get;
			}

			public new string Email
			{
				set;
				get;
			}

			public string Mobile
			{
				set;
				get;
			}

			public bool IsActive
			{
				set;
				get;
			}

			public IContactable[] GetNestedContacts(string parentContactDescription)
			{
				return null;
			}

			#endregion

			#region IPasswordEmailSource Members

			public ZString Password
			{
				set;
				get;
			}

			public new ZString Url { get; set; }

			public new ZString OrgCode { get; set; }

			public new ZString ExtraInstruction
			{
				get;
				set;
			}

			public new ZString Salutation
			{
				get;
				set;
			}

			#endregion
		}

		#endregion
	}
}
