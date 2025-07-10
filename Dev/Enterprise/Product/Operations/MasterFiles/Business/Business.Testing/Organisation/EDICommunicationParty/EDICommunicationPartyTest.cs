using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EDICommunicationParty))]
	public class EDICommunicationPartyTest : EnterpriseBusinessObjectTestCase
	{
		public void TestMandatoryFieldsForNewEDICommunicationParty()
		{
			var commParty = Factory.NewWithValidTestData<EDICommunicationParty>();

			commParty.ECP_Name = string.Empty;
			commParty.ECP_OC_TechnicalContact = ZGuid.Empty;
			commParty.ECP_GS_SecurityProxy = ZGuid.Empty;

			commParty.Validation.ValidateAll();

			CombineAssertions(delegate
			{
				AssertEquals("ECP_Name should error when empty", true, commParty.ECP_NameInfo.HasErrors());
				AssertEquals("ECP_OC_TechnicalContact should error when empty", true, commParty.ECP_NameInfo.HasErrors());
				AssertEquals("ECP_GS_SecurityProxy should error when empty", true, commParty.ECP_NameInfo.HasErrors());
			});
		}

		public void TestUniquenessCheckOfName()
		{
			var oldCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			oldCommParty.ECP_Name = "Test";

			Factory.Save();

			var newCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			newCommParty.ECP_Name = "Test";

			AssertExceptionThrown<Exception>("Save() should fail trying to save edi client with duplicate name", () => Factory.Save());
		}

		public void TestTechnicalContactValidation()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = header.Contacts.AddNew();
			var contact2 = header.Contacts.AddNew();
			var contact3 = header.Contacts.AddNew();

			contact1.OC_ContactName = "Test Name 1";
			contact1.OC_Email = "test@test.mail.com";

			contact1.Delete();

			contact2.OC_ContactName = "Test Name 2";
			contact2.OC_Email = ZString.Empty;

			contact3.OC_ContactName = "Test Name 3";
			contact3.OC_Email = "test@test.mail.com";

			Factory.Save();

			var commParty1 = Factory.NewWithValidTestData<EDICommunicationParty>();
			var commParty2 = Factory.NewWithValidTestData<EDICommunicationParty>();
			var commParty3 = Factory.NewWithValidTestData<EDICommunicationParty>();

			commParty1.ECP_OC_TechnicalContact = contact1.PK;
			commParty2.ECP_OC_TechnicalContact = contact2.PK;
			commParty3.ECP_OC_TechnicalContact = contact3.PK;

			Assert("Technical Contact has Error", commParty1.ECP_OC_TechnicalContactInfo.HasError("Technical contact must exist."));
			Assert("Technical Contact has Error", commParty2.ECP_OC_TechnicalContactInfo.HasError("Technical contact must have an email address."));
			Assert("Technical Contact does not have Error", !commParty3.ECP_OC_TechnicalContactInfo.HasErrors());
		}

		public void TestInvalidValueForApplicationCode()
		{
			var commParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			commParty.ECP_ApplicationCode = "AAA";

			AssertHasError(commParty.ECP_ApplicationCodeInfo, "Enter a valid selection.");
		}

		public void TestEmptyValueForApplicationCode()
		{
			var commParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			commParty.ECP_ApplicationCode = "";

			AssertHasError(commParty.ECP_ApplicationCodeInfo, "Please enter a value.");
		}

		public void TestNameMaxLengthExceededException()
		{
			var commParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			using (new DisposableAction(ErrorReporter.Clear))
			{
				AssertExceptionThrown(typeof(MaxLengthExceededException), () => commParty.ECP_Name = new string('A', 51));
				AssertContains("The maximum length of 'ECP_Name' has been exceeded.", ErrorReporter.LastMessageReported);
			}
		}

		public void TestStaffProxyValidation()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_IsActive = false;
			staff2.GS_IsActive = true;

			Factory.Save();

			var commParty1 = Factory.NewWithValidTestData<EDICommunicationParty>();
			var commParty2 = Factory.NewWithValidTestData<EDICommunicationParty>();

			commParty1.ECP_GS_SecurityProxy = staff1.PK;
			commParty2.ECP_GS_SecurityProxy = staff2.PK;

			Assert("Staff Proxy is inactive", commParty1.ECP_GS_SecurityProxyInfo.HasError("This selection is inactive - it may not be used."));
			Assert("Staff Proxy is active", !commParty2.ECP_GS_SecurityProxyInfo.HasErrors());
		}
	}
}
