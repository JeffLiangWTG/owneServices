using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	[TestedType(typeof(USContactAddressSource))]
	sealed class USContactAddressSourceTest : USOrganisationWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				USContactAddressSource.New(USOrganisation, Factory)
			};
		}

		public void TestNew()
		{
			AssertNull("New", USContactAddressSource.New(null, Factory));
			USOrganisation.ZO_OH_Organisation = ZGuid.Empty;
			AssertNull("New", USContactAddressSource.New(USOrganisation, Factory));
			USOrganisation.ZO_OH_Organisation = Supplier.PK;
			AssertNotNull("New", USContactAddressSource.New(USOrganisation, Factory));
		}

		public void TestToString()
		{
			AssertEquals("ToString()", SupplierContact.OC_ContactName, contactWrapper.ToString());
		}

		public void TestContact()
		{
			USOrganisation.ZO_Contact = SupplierContact.OC_ContactName;
			AssertNotNull("Contact", contactWrapper.Contact);
			AssertEquals("Contact's type", typeof(Enterprise.DocumentWrappers.DocContacts), contactWrapper.Contact.GetType());
			USOrganisation.ZO_Contact = "";
			AssertNull("Contact", contactWrapper.Contact);
		}

		public void TestPostalAddress()
		{
			USOrganisation.ZO_Contact = "";
			USOrganisation.ZO_OA_Address = ZGuid.Empty;
			AssertNull("ContactWrapper.Contact", contactWrapper.Contact);
			AssertNull("ContactWrapper.OrgAddress", contactWrapper.OrgAddress);
			AssertEquals("PostalAddress", ZString.Empty, contactWrapper.PostalAddress);
			USOrganisation.ZO_Contact = SupplierContact.OC_ContactName;
			AssertNotNull("ContactWrapper.Contact", contactWrapper.Contact);
			AssertEquals("PostalAddress", contactWrapper.Contact.PostalAddress, contactWrapper.PostalAddress);
			USOrganisation.ZO_OA_Address = SupplierAddress.PK;
			AssertNotNull("ContactWrapper.OrgAddress", contactWrapper.OrgAddress);
			AssertEquals("PostalAddress", contactWrapper.OrgAddress.PostalAddress, contactWrapper.PostalAddress);
		}

		public void TestPostalAddressInEnglish()
		{
			var staffDE = Factory.New<GlbStaff>();
			staffDE.GS_Code = "ABC";
			staffDE.GS_LoginName = "Dieter";
			staffDE.GS_FullName = "Dieter";
			staffDE.GS_WorkingLanguage = Core.SharedConstants.Languages.German;

			Factory.Save();

			using (Env.Instance.SetTemporaryUserContext(staffDE.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				USOrganisation.ZO_Contact = "";
				USOrganisation.ZO_OA_Address = ZGuid.Empty;
				AssertNull("ContactWrapper.Contact", contactWrapper.Contact);
				AssertNull("ContactWrapper.OrgAddress", contactWrapper.OrgAddress);
				AssertEquals("PostalAddressInEnglish", ZString.Empty, contactWrapper.PostalAddressInEnglish);

				USOrganisation.ZO_Contact = SupplierContact.OC_ContactName;
				AssertNotNull("ContactWrapper.Contact", contactWrapper.Contact);
				AssertEquals("PostalAddressInEnglish", contactWrapper.Contact.PostalAddressInEnglish, contactWrapper.PostalAddressInEnglish);

				USOrganisation.ZO_OA_Address = SupplierAddress.PK;
				AssertNotNull("ContactWrapper.OrgAddress", contactWrapper.OrgAddress);
				AssertEquals("PostalAddressInEnglish", contactWrapper.OrgAddress.PostalAddressInEnglish, contactWrapper.PostalAddressInEnglish);
			}
		}

		public void TestName()
		{
			AssertEquals("Name", USOrganisation.ZO_Contact + "\n" + USOrganisation.Organisation.OH_FullName, contactWrapper.Name);
		}

		public void TestCode()
		{
			AssertNotNull("ContactWrapper.Contact", contactWrapper.Contact);
			AssertEquals("Code", contactWrapper.Contact.Code, contactWrapper.Code);
			USOrganisation.ZO_Contact = "";
			AssertNull("ContactWrapper.Contact", contactWrapper.Contact);
			AssertEquals("Code", ZString.Empty, contactWrapper.Code);
		}

		public void TestAttachmentType()
		{
			AssertNotNull("ContactWrapper.Contact", contactWrapper.Contact);
			AssertEquals("AttachmentType", contactWrapper.Contact.AttachmentType, contactWrapper.AttachmentType);
			USOrganisation.ZO_Contact = "";
			AssertNull("ContactWrapper.Contact", contactWrapper.Contact);
			AssertEquals("AttachmentType", ZString.Empty, contactWrapper.AttachmentType);
		}

		public void TestBirthday()
		{
			AssertNotNull("ContactWrapper.Contact", contactWrapper.Contact);
			AssertEquals("Birthday", contactWrapper.Contact.Birthday, contactWrapper.Birthday);
			USOrganisation.ZO_Contact = "";
			AssertNull("ContactWrapper.Contact", contactWrapper.Contact);
			AssertEquals("Birthday", ZDateTime.Empty, contactWrapper.Birthday);
		}

		public void TestContactName()
		{
			AssertEquals("ContactName", USOrganisation.ZO_Contact, contactWrapper.ContactName);
		}

		public void TestEmail()
		{
			AssertNotNull("ContactWrapper.Contact", contactWrapper.Contact);
			AssertEquals("Email", contactWrapper.Contact.Email, contactWrapper.Email);
			USOrganisation.ZO_Contact = "";
			AssertNull("ContactWrapper.Contact", contactWrapper.Contact);
			AssertEquals("Email", ZString.Empty, contactWrapper.Email);
		}

		public void TestFax()
		{
			AssertNotNull("ContactWrapper.Contact", contactWrapper.Contact);
			AssertEquals("Fax", contactWrapper.Contact.Fax, contactWrapper.Fax);
			USOrganisation.ZO_Contact = "";
			AssertNull("ContactWrapper.Contact", contactWrapper.Contact);
			AssertEquals("Fax", ZString.Empty, contactWrapper.Fax);
		}

		public void TestHomePhone()
		{
			AssertNotNull("ContactWrapper.Contact", contactWrapper.Contact);
			AssertEquals("HomePhone", contactWrapper.Contact.HomePhone, contactWrapper.HomePhone);
			USOrganisation.ZO_Contact = "";
			AssertNull("ContactWrapper.Contact", contactWrapper.Contact);
			AssertEquals("HomePhone", ZString.Empty, contactWrapper.HomePhone);
		}

		public void TestLanguage()
		{
			AssertNotNull("ContactWrapper.Contact", contactWrapper.Contact);
			AssertEquals("Language", contactWrapper.Contact.Language, contactWrapper.Language);
			USOrganisation.ZO_Contact = "";
			AssertNull("ContactWrapper.Contact", contactWrapper.Contact);
			AssertEquals("Language", ZString.Empty, contactWrapper.Language);
		}

		public void TestMobile()
		{
			AssertNotNull("ContactWrapper.Contact", contactWrapper.Contact);
			AssertEquals("Mobile", contactWrapper.Contact.Mobile, contactWrapper.Mobile);
			USOrganisation.ZO_Contact = "";
			AssertNull("ContactWrapper.Contact", contactWrapper.Contact);
			AssertEquals("Mobile", ZString.Empty, contactWrapper.Mobile);
		}

		public void TestNotifyMode()
		{
			AssertNotNull("ContactWrapper.Contact", contactWrapper.Contact);
			AssertEquals("NotifyMode", contactWrapper.Contact.NotifyMode, contactWrapper.NotifyMode);
			USOrganisation.ZO_Contact = "";
			AssertNull("ContactWrapper.Contact", contactWrapper.Contact);
			AssertEquals("NotifyMode", ZString.Empty, contactWrapper.NotifyMode);
		}

		public void TestOrgAddress()
		{
			USOrganisation.ZO_OA_Address = SupplierAddress.PK;
			AssertNotNull("OrgAddress", contactWrapper.OrgAddress);
			AssertEquals("OrgAddress's type", typeof(Enterprise.DocumentWrappers.DocAddress), contactWrapper.OrgAddress.GetType());
			USOrganisation.ZO_OA_Address = ZGuid.Empty;
			AssertNull("OrgAddress", contactWrapper.OrgAddress);
		}

		public void TestOrganisation()
		{
			USOrganisation.ZO_OH_Organisation = Supplier.PK;
			AssertNotNull("Organisation", contactWrapper.Organisation);
			AssertEquals("Organisation's type", typeof(Enterprise.DocumentWrappers.DocOrganisation), contactWrapper.Organisation.GetType());
			USOrganisation.ZO_OH_Organisation = ZGuid.Empty;
			AssertNull("Organisation", contactWrapper.Organisation);
		}

		public void TestAddressOverride()
		{
			USOrganisation.ZO_OH_Organisation = Supplier.PK;
			AssertNotNull("AddressOverride", contactWrapper.AddressOverride);
			AssertEquals("AddressOverride's type", typeof(Enterprise.DocumentWrappers.DocOrganisation), contactWrapper.AddressOverride.GetType());
			USOrganisation.ZO_OH_Organisation = ZGuid.Empty;
			AssertNull("AddressOverride", contactWrapper.AddressOverride);
		}

		public void TestOtherPhone()
		{
			AssertNotNull("ContactWrapper.Contact", contactWrapper.Contact);
			AssertEquals("OtherPhone", contactWrapper.Contact.OtherPhone, contactWrapper.OtherPhone);
			USOrganisation.ZO_Contact = "";
			AssertNull("ContactWrapper.Contact", contactWrapper.Contact);
			AssertEquals("OtherPhone", ZString.Empty, contactWrapper.OtherPhone);
		}

		public void TestPager()
		{
			AssertNotNull("ContactWrapper.Contact", contactWrapper.Contact);
			AssertEquals("Pager", contactWrapper.Contact.Pager, contactWrapper.Pager);
			USOrganisation.ZO_Contact = "";
			AssertNull("ContactWrapper.Contact", contactWrapper.Contact);
			AssertEquals("Pager", ZString.Empty, contactWrapper.Pager);
		}

		public void TestPassword()
		{
			AssertNotNull("ContactWrapper.Contact", contactWrapper.Contact);
			AssertEquals("Password", contactWrapper.Contact.Password, contactWrapper.Password);
			USOrganisation.ZO_Contact = "";
			AssertNull("ContactWrapper.Contact", contactWrapper.Contact);
			AssertEquals("Password", ZString.Empty, contactWrapper.Password);
		}

		public void TestPersonalInfo()
		{
			AssertNotNull("ContactWrapper.Contact", contactWrapper.Contact);
			AssertEquals("PersonalInfo", contactWrapper.Contact.PersonalInfo, contactWrapper.PersonalInfo);
			USOrganisation.ZO_Contact = "";
			AssertNull("ContactWrapper.Contact", contactWrapper.Contact);
			AssertEquals("PersonalInfo", ZString.Empty, contactWrapper.PersonalInfo);
		}

		public void TestPhone()
		{
			AssertEquals("Phone", USOrganisation.ZO_Phone, contactWrapper.Phone);
		}

		public void TestTitle()
		{
			AssertNotNull("ContactWrapper.Contact", contactWrapper.Contact);
			AssertEquals("Title", contactWrapper.Contact.Title, contactWrapper.Title);
			USOrganisation.ZO_Contact = "";
			AssertNull("ContactWrapper.Contact", contactWrapper.Contact);
			AssertEquals("Title", ZString.Empty, contactWrapper.Title);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			contactWrapper = USContactAddressSource.New(USOrganisation, Factory);
			AssertNotNull("Wrapper created not null", contactWrapper);
		}

		USContactAddressSource contactWrapper;

		#endregion
	}
}
