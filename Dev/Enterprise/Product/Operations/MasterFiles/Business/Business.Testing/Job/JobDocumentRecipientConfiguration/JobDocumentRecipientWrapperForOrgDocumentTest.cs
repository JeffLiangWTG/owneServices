using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobDocumentRecipientWrapperForOrgDocument))]
	public class JobDocumentRecipientWrapperForOrgDocumentTest : JobDocumentRecipientWrapperForOrgDocumentBaseTest<JobDocumentRecipientWrapperForOrgDocument>
	{
		#region Implementation

		protected override JobDocumentRecipientWrapperForOrgDocument GetWrapper(OrgDocument orgDocument, JobDocumentRecipientConfiguration configuration)
		{
			return new JobDocumentRecipientWrapperForOrgDocument(orgDocument, configuration);
		}

		protected override ZBool ExpectedValueForIsExclusion => false;

		protected override BusinessObject GetNewBusinessObject()
		{
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			var orgDocument = Factory.NewWithValidTestData<OrgDocument>();
			orgDocument.OD_OC = orgContact.PK;
			return new JobDocumentRecipientWrapperForOrgDocument(orgDocument, Configuration);
		}

		#endregion
	}

	public abstract class JobDocumentRecipientWrapperForOrgDocumentBaseTest<T> : JobDocumentRecipientWrapperBaseTest where T : JobDocumentRecipientWrapperBase
	{
		public override void TestOrganisation()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			var orgDocument = Factory.NewWithValidTestData<OrgDocument>();
			orgContact.OC_OH = orgHeader.PK;
			orgDocument.OD_OC = orgContact.PK;
			Factory.Save();

			var wrapper = GetWrapper(orgDocument, Configuration);
			AssertEquals(orgHeader.PK, wrapper.Organisation.PK);
		}

		public override void TestContacts()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			var contact3 = Factory.NewWithValidTestData<OrgContact>();

			var orgDocumentOrg1 = Factory.NewWithValidTestData<OrgDocument>();
			orgDocumentOrg1.OD_OC = contact1.PK;
			orgDocumentOrg1.Contact.OC_OH = org1.PK;

			var orgDocumentOrg2 = Factory.NewWithValidTestData<OrgDocument>();
			orgDocumentOrg2.OD_OC = contact2.PK;
			orgDocumentOrg2.Contact.OC_OH = org2.PK;

			var orgDocumentOrg3 = Factory.NewWithValidTestData<OrgDocument>();
			orgDocumentOrg3.OD_OC = contact3.PK;
			orgDocumentOrg3.Contact.OC_OH = org3.PK;

			var activeContactOrg1 = Factory.NewWithValidTestData<OrgContact>();
			activeContactOrg1.OC_OH = org1.PK;

			var anotherActiveContactOrg1 = Factory.NewWithValidTestData<OrgContact>();
			anotherActiveContactOrg1.OC_OH = org1.PK;

			var activeContactOrg2 = Factory.NewWithValidTestData<OrgContact>();
			activeContactOrg2.OC_OH = org2.PK;

			var inactiveContactOrg1 = Factory.NewWithValidTestData<OrgContact>();
			inactiveContactOrg1.OC_OH = org1.PK;
			inactiveContactOrg1.OC_IsActive = false;

			var inactiveContactOrg2 = Factory.NewWithValidTestData<OrgContact>();
			inactiveContactOrg2.OC_OH = org2.PK;
			inactiveContactOrg2.OC_IsActive = false;

			Factory.Save();

			var wrapper = GetWrapper(orgDocumentOrg1, Configuration);
			AssertEquals(3, wrapper.Contacts.Count);
			AssertCollectionContains(wrapper.Contacts.Cast<OrgContact>(), c => c.PK == orgDocumentOrg1.Contact.PK);
			AssertCollectionContains(wrapper.Contacts.Cast<OrgContact>(), c => c.PK == activeContactOrg1.PK);
			AssertCollectionContains(wrapper.Contacts.Cast<OrgContact>(), c => c.PK == anotherActiveContactOrg1.PK);

			wrapper = GetWrapper(orgDocumentOrg2, Configuration);
			AssertEquals(2, wrapper.Contacts.Count);
			AssertCollectionContains(wrapper.Contacts.Cast<OrgContact>(), c => c.PK == orgDocumentOrg2.Contact.PK);
			AssertCollectionContains(wrapper.Contacts.Cast<OrgContact>(), c => c.PK == activeContactOrg2.PK);

			wrapper = GetWrapper(orgDocumentOrg3, Configuration);
			AssertEquals(1, wrapper.Contacts.Count);
			AssertCollectionContains(wrapper.Contacts.Cast<OrgContact>(), c => c.PK == orgDocumentOrg3.Contact.PK);
		}

		public override void TestPropertiesReadonliness()
		{
			var wrapper = (T)GetNewBusinessObject();

			var expectedReadOnlyProperties = wrapper.ZPropertyInfoHash
				.Cast<ZPropertyInfo>()
				.ToList();
			Assert(expectedReadOnlyProperties.Count > 0);

			foreach (var info in expectedReadOnlyProperties)
			{
				Assert($"Property {info.Name} should be read-only", info.ReadOnly);
			}
		}

		public void TestPropertyInfos_ShouldNotWrapOrgDocumentPropertyInfos()
		{
			var orgDocument = Factory.NewWithValidTestData<OrgDocument>();
			var wrapper = GetWrapper(orgDocument, Configuration);
			var wrappedPropertyInfos = wrapper.ZPropertyInfoHash.GetPropertyInfos(PropertyInfoTypes.Wrapping).ToList<ZPropertyInfo>();
			AssertEquals(0, wrappedPropertyInfos.Count);
		}

		public override void TestWrappedProperties()
		{
			var organisationPK = ZGuid.NewZGuid();
			var documentPK = ZGuid.NewZGuid();
			var orgContact = Factory.NewWithValidTestData<OrgContact>();

			var orgDocument = Factory.NewWithValidTestData<OrgDocument>();
			orgDocument.OD_OC = orgContact.PK;
			orgDocument.Contact.OC_OH = organisationPK;
			orgDocument.Contact.OC_ContactName = "Contact";
			orgDocument.OD_DeliverBy = Core.Constants.ContactNotifyModes.Email;
			orgDocument.OD_AttachmentType = OrgConstants.AttachmentType.PDFA;
			orgDocument.Contact.OC_Fax = "123456";
			orgDocument.OD_DocumentGroup = ContactType.Receivables.Code;
			orgDocument.OD_SU_MenuItem = documentPK;
			orgDocument.Contact.OC_Email = "a@a.aa";
			orgDocument.OD_CarbonCopyRecipientsAsString = "b@b.bb, c@c.cc";
			orgDocument.OD_BlindCarbonCopyRecipientsAsString = "d@d.dd";
			orgDocument.OD_EmailSubjectMacro = "<NotAValid Macro> subject";

			var wrapper = GetWrapper(orgDocument, Configuration);
			AssertEquals(organisationPK, wrapper.OrganisationPK);
			AssertEquals("Contact", wrapper.ContactName);
			AssertEquals(Core.Constants.ContactNotifyModes.Email, wrapper.DeliveryMethod);
			AssertEquals(OrgConstants.AttachmentType.PDFA, wrapper.AttachmentType);
			AssertEquals("123456", wrapper.FaxNumber);
			AssertEquals(ContactType.Receivables.Code, wrapper.DocumentGroup);
			AssertEquals(documentPK, wrapper.DocumentPK);
			AssertEquals("a@a.aa", wrapper.EmailToRecipientsAsString);
			AssertEquals("b@b.bb, c@c.cc", wrapper.CarbonCopyRecipientsAsString);
			AssertEquals("d@d.dd", wrapper.BlindCarbonCopyRecipientsAsString);
			AssertEquals("<NotAValid Macro> subject", wrapper.EmailSubjectMacro);
			AssertEquals(true, wrapper.DefaultedFromOrganisationRecord);
			AssertEquals(ExpectedValueForIsExclusion, wrapper.IsExclusion);
		}

		public override void TestCanDelete()
		{
			var wrapper = (T)GetNewBusinessObject();

			Assert("OrgDocument wrapppers should not be deletable.", !wrapper.CanDelete);
			AssertEquals("Wrapper should provide a reason for not being able to be deleted.", "The selected recipient is defaulted from the Organization record and cannot be deleted. Please edit the organization directly.", wrapper.ReasonForNotAbleToDelete);
		}

		#region Implementation

		protected abstract T GetWrapper(OrgDocument orgDocument, JobDocumentRecipientConfiguration configuration);

		protected abstract ZBool ExpectedValueForIsExclusion { get; }

		#endregion
	}
}
