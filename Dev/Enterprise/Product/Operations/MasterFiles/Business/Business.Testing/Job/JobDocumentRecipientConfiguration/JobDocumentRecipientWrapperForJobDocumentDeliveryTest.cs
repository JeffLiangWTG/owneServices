using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobDocumentRecipientWrapperForJobDocumentDelivery))]
	public class JobDocumentRecipientWrapperForJobDocumentDeliveryTest : JobDocumentRecipientWrapperBaseTest
	{
		public override void TestOrganisation()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var wrapper = (JobDocumentRecipientWrapperForJobDocumentDelivery)GetNewBusinessObject();

			wrapper.OrganisationPK = orgHeader.PK;
			AssertEquals(orgHeader.PK, wrapper.Organisation.PK);
		}

		public override void TestContacts()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

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

			var wrapper = (JobDocumentRecipientWrapperForJobDocumentDelivery)GetNewBusinessObject();

			wrapper.OrganisationPK = org1.PK;
			AssertEquals(2, wrapper.Contacts.Count);
			AssertCollectionContains(wrapper.Contacts.Cast<OrgContact>(), c => c.PK == activeContactOrg1.PK);
			AssertCollectionContains(wrapper.Contacts.Cast<OrgContact>(), c => c.PK == anotherActiveContactOrg1.PK);

			wrapper.OrganisationPK = org2.PK;
			AssertEquals(1, wrapper.Contacts.Count);
			AssertCollectionContains(wrapper.Contacts.Cast<OrgContact>(), c => c.PK == activeContactOrg2.PK);

			wrapper.OrganisationPK = org3.PK;
			AssertEquals(0, wrapper.Contacts.Count);
		}

		public override void TestPropertiesReadonliness()
		{
			var wrapper = (JobDocumentRecipientWrapperForJobDocumentDelivery)GetNewBusinessObject();

			wrapper.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			var expectedWritableProperties = wrapper.ZPropertyInfoHash.GetPropertyInfos(PropertyInfoTypes.Wrapping)
				.Cast<ZPropertyInfo>()
				.Where(i => i.Name != wrapper.IsExclusionInfo.Name && i.Name != wrapper.FaxNumberInfo.Name)
				.ToList();
			Assert(expectedWritableProperties.Count > 0);

			foreach (var info in expectedWritableProperties)
			{
				Assert($"Property {info.Name} should not be read-only", !info.ReadOnly);
			}
			Assert($"Property {wrapper.IsExclusionInfo.Name} should be read-only", wrapper.IsExclusionInfo.ReadOnly);
		}

		public void TestFaxNumberReadonliness()
		{
			var wrapper = (JobDocumentRecipientWrapperForJobDocumentDelivery)GetNewBusinessObject();

			wrapper.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			Assert($"Property {wrapper.FaxNumberInfo.Name} should be read-only", wrapper.FaxNumberInfo.ReadOnly);

			wrapper.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			Assert($"Property {wrapper.FaxNumberInfo.Name} should not be read-only", !wrapper.FaxNumberInfo.ReadOnly);
		}

		public void TestWrappedPropertyInfos()
		{
			var jobDocumentDelivery = Factory.New<JobDocumentDelivery>();
			var wrapper = new JobDocumentRecipientWrapperForJobDocumentDelivery(jobDocumentDelivery, Configuration);
			AssertEquals(jobDocumentDelivery.JDC_OHInfo, ((ZWrappedPropertyInfo)wrapper.OrganisationPKInfo).InnerInfo);
			AssertEquals(jobDocumentDelivery.JDC_DeliveryMethodInfo, ((ZWrappedPropertyInfo)wrapper.DeliveryMethodInfo).InnerInfo);
			AssertEquals(jobDocumentDelivery.JDC_AttachmentTypeInfo, ((ZWrappedPropertyInfo)wrapper.AttachmentTypeInfo).InnerInfo);
			AssertEquals(jobDocumentDelivery.JDC_FaxNumberInfo, ((ZWrappedPropertyInfo)wrapper.FaxNumberInfo).InnerInfo);
			AssertEquals(jobDocumentDelivery.JDC_DocumentGroupInfo, ((ZWrappedPropertyInfo)wrapper.DocumentGroupInfo).InnerInfo);
			AssertEquals(jobDocumentDelivery.JDC_SU_MenuItemInfo, ((ZWrappedPropertyInfo)wrapper.DocumentPKInfo).InnerInfo);
			AssertEquals(jobDocumentDelivery.JDC_EmailToRecipientsAsStringInfo, ((ZWrappedPropertyInfo)wrapper.EmailToRecipientsAsStringInfo).InnerInfo);
			AssertEquals(jobDocumentDelivery.JDC_CarbonCopyRecipientsAsStringInfo, ((ZWrappedPropertyInfo)wrapper.CarbonCopyRecipientsAsStringInfo).InnerInfo);
			AssertEquals(jobDocumentDelivery.JDC_BlindCarbonCopyRecipientsAsStringInfo, ((ZWrappedPropertyInfo)wrapper.BlindCarbonCopyRecipientsAsStringInfo).InnerInfo);
			AssertEquals(jobDocumentDelivery.JDC_EmailSubjectMacroInfo, ((ZWrappedPropertyInfo)wrapper.EmailSubjectMacroInfo).InnerInfo);
			AssertNull(wrapper.ContactNameInfo as ZWrappedPropertyInfo);
			AssertNull(wrapper.DefaultedFromOrganisationRecordInfo as ZWrappedPropertyInfo);
			AssertNull(wrapper.IsExclusionInfo as ZWrappedPropertyInfo);
		}

		public override void TestWrappedProperties()
		{
			var organisationPK = ZGuid.NewZGuid();
			var contactPK = ZGuid.NewZGuid();
			var documentPK = ZGuid.NewZGuid();

			var jobDocumentDelivery = Factory.NewWithValidTestData<JobDocumentDelivery>();
			jobDocumentDelivery.JDC_OH = organisationPK;
			jobDocumentDelivery.JDC_OC_Contact = contactPK;
			jobDocumentDelivery.JDC_ContactName = "Contact";
			jobDocumentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			jobDocumentDelivery.JDC_AttachmentType = OrgConstants.AttachmentType.PDFA;
			jobDocumentDelivery.JDC_FaxNumber = "123456";
			jobDocumentDelivery.JDC_DocumentGroup = ContactType.Receivables.Code;
			jobDocumentDelivery.JDC_SU_MenuItem = documentPK;
			jobDocumentDelivery.JDC_EmailToRecipientsAsString = "a@a.aa";
			jobDocumentDelivery.JDC_CarbonCopyRecipientsAsString = "b@b.bb, c@c.cc";
			jobDocumentDelivery.JDC_BlindCarbonCopyRecipientsAsString = "d@d.dd";
			jobDocumentDelivery.JDC_EmailSubjectMacro = "<NotAValid Macro> subject";

			var wrapper = new JobDocumentRecipientWrapperForJobDocumentDelivery(jobDocumentDelivery, Configuration);
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
			AssertEquals(false, wrapper.DefaultedFromOrganisationRecord);
			AssertEquals(false, wrapper.IsExclusion);
		}

		public override void TestCanDelete()
		{
			var wrapper = (JobDocumentRecipientWrapperForJobDocumentDelivery)GetNewBusinessObject();

			Assert("JobDocumentDelivery wrapppers should be deletable.", wrapper.CanDelete);
		}

		public void TestDelete()
		{
			var jobDocumentDelivery = Factory.NewWithValidTestData<JobDocumentDelivery>();
			var wrapper = new JobDocumentRecipientWrapperForJobDocumentDelivery(jobDocumentDelivery, Configuration);
			Assert("Pre-condition", !jobDocumentDelivery.IsDeleted);

			wrapper.Delete();
			Assert(jobDocumentDelivery.IsDeleted);
		}

		public void TestHumanReadableName()
		{
			var wrapper = (JobDocumentRecipientWrapperForJobDocumentDelivery)GetNewBusinessObject();

			AssertEquals("A human readable name should be provided for this NonPersistentBusinessObject", "Recipient", wrapper.HumanReadableName);
		}

		public void TestContactName()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();

			var dexter = organisation.Contacts.AddNew();
			dexter.FillWithValidTestData();
			dexter.OC_ContactName = "Dexter Morgan";

			var debra = organisation.Contacts.AddNew();
			debra.FillWithValidTestData();
			debra.OC_ContactName = "Debra Morgan";

			Factory.Save();

			var jobDocumentDelivery = Factory.NewWithValidTestData<JobDocumentDelivery>();
			jobDocumentDelivery.JDC_OH = organisation.PK;
			var wrapper = new JobDocumentRecipientWrapperForJobDocumentDelivery(jobDocumentDelivery, Configuration);
			AssertEquals("Pre-condition", ZString.Empty, wrapper.ContactName);
			AssertEquals("Pre-condition", ZGuid.Empty, wrapper.JobDocumentDelivery.JDC_OC_Contact);
			AssertEquals("Pre-condition", ZString.Empty, wrapper.JobDocumentDelivery.JDC_ContactName);

			wrapper.ContactName = "Dexter Morgan";
			AssertEquals("Dexter Morgan", wrapper.ContactName);
			AssertEquals(dexter.PK, wrapper.JobDocumentDelivery.JDC_OC_Contact);
			AssertEquals(ZString.Empty, wrapper.JobDocumentDelivery.JDC_ContactName);

			wrapper.ContactName = "The Bay Harbor Butcher";
			AssertEquals("The Bay Harbor Butcher", wrapper.ContactName);
			AssertEquals(ZGuid.Empty, wrapper.JobDocumentDelivery.JDC_OC_Contact);
			AssertEquals("The Bay Harbor Butcher", wrapper.JobDocumentDelivery.JDC_ContactName);

			wrapper.ContactName = "Debra Morgan";
			AssertEquals("Debra Morgan", wrapper.ContactName);
			AssertEquals(debra.PK, wrapper.JobDocumentDelivery.JDC_OC_Contact);
			AssertEquals(ZString.Empty, wrapper.JobDocumentDelivery.JDC_ContactName);

			wrapper.ContactName = ZString.Empty;
			AssertEquals(ZString.Empty, wrapper.ContactName);
			AssertEquals(ZGuid.Empty, wrapper.JobDocumentDelivery.JDC_OC_Contact);
			AssertEquals(ZString.Empty, wrapper.JobDocumentDelivery.JDC_ContactName);
		}

		public void TestEmailToRecipients()
		{
			var jobDocumentDelivery = Factory.NewWithValidTestData<JobDocumentDelivery>();
			jobDocumentDelivery.JDC_EmailToRecipientsAsString = "a@a.aa, b@b.bb";
			var wrapper = new JobDocumentRecipientWrapperForJobDocumentDelivery(jobDocumentDelivery, Configuration);
			AssertEquals("Pre-condition", jobDocumentDelivery.JDC_EmailToRecipientsAsString, wrapper.EmailToRecipientsAsString);

			AssertEquals(2, wrapper.EmailToRecipients.Count);
			AssertCollectionContains(wrapper.EmailToRecipients.Cast<NonPersistentCopyRecipient>(), r => r.EmailAddress == "a@a.aa");
			AssertCollectionContains(wrapper.EmailToRecipients.Cast<NonPersistentCopyRecipient>(), r => r.EmailAddress == "b@b.bb");

			//Setting the wrapper property should refresh the list
			wrapper.EmailToRecipientsAsString = "c@c.cc, b@b.bb, d@d.dd";
			AssertEquals(3, wrapper.EmailToRecipients.Count);
			AssertCollectionContains(wrapper.EmailToRecipients.Cast<NonPersistentCopyRecipient>(), r => r.EmailAddress == "c@c.cc");
			AssertCollectionContains(wrapper.EmailToRecipients.Cast<NonPersistentCopyRecipient>(), r => r.EmailAddress == "b@b.bb");
			AssertCollectionContains(wrapper.EmailToRecipients.Cast<NonPersistentCopyRecipient>(), r => r.EmailAddress == "d@d.dd");

			//Setting the wrapped property should also refresh the list
			jobDocumentDelivery.JDC_EmailToRecipientsAsString = "f@f.ff, g@g.gg";
			AssertEquals(2, wrapper.EmailToRecipients.Count);
			AssertCollectionContains(wrapper.EmailToRecipients.Cast<NonPersistentCopyRecipient>(), r => r.EmailAddress == "f@f.ff");
			AssertCollectionContains(wrapper.EmailToRecipients.Cast<NonPersistentCopyRecipient>(), r => r.EmailAddress == "g@g.gg");
		}

		public void TestCarbonCopyRecipients()
		{
			var jobDocumentDelivery = Factory.NewWithValidTestData<JobDocumentDelivery>();
			jobDocumentDelivery.JDC_CarbonCopyRecipientsAsString = "a@a.aa, b@b.bb";
			var wrapper = new JobDocumentRecipientWrapperForJobDocumentDelivery(jobDocumentDelivery, Configuration);
			AssertEquals("Pre-condition", jobDocumentDelivery.JDC_CarbonCopyRecipientsAsString, wrapper.CarbonCopyRecipientsAsString);

			AssertEquals(2, wrapper.CarbonCopyRecipients.Count);
			AssertCollectionContains(wrapper.CarbonCopyRecipients.Cast<NonPersistentCopyRecipient>(), r => r.EmailAddress == "a@a.aa");
			AssertCollectionContains(wrapper.CarbonCopyRecipients.Cast<NonPersistentCopyRecipient>(), r => r.EmailAddress == "b@b.bb");

			//Setting the wrapper property should refresh the list
			wrapper.CarbonCopyRecipientsAsString = "c@c.cc, b@b.bb, d@d.dd";
			AssertEquals(3, wrapper.CarbonCopyRecipients.Count);
			AssertCollectionContains(wrapper.CarbonCopyRecipients.Cast<NonPersistentCopyRecipient>(), r => r.EmailAddress == "c@c.cc");
			AssertCollectionContains(wrapper.CarbonCopyRecipients.Cast<NonPersistentCopyRecipient>(), r => r.EmailAddress == "b@b.bb");
			AssertCollectionContains(wrapper.CarbonCopyRecipients.Cast<NonPersistentCopyRecipient>(), r => r.EmailAddress == "d@d.dd");

			//Setting the wrapped property should also refresh the list
			jobDocumentDelivery.JDC_CarbonCopyRecipientsAsString = "f@f.ff, g@g.gg";
			AssertEquals(2, wrapper.CarbonCopyRecipients.Count);
			AssertCollectionContains(wrapper.CarbonCopyRecipients.Cast<NonPersistentCopyRecipient>(), r => r.EmailAddress == "f@f.ff");
			AssertCollectionContains(wrapper.CarbonCopyRecipients.Cast<NonPersistentCopyRecipient>(), r => r.EmailAddress == "g@g.gg");
		}

		public void TestBlindCarbonCopyRecipients()
		{
			var jobDocumentDelivery = Factory.NewWithValidTestData<JobDocumentDelivery>();
			jobDocumentDelivery.JDC_BlindCarbonCopyRecipientsAsString = "a@a.aa, b@b.bb";
			var wrapper = new JobDocumentRecipientWrapperForJobDocumentDelivery(jobDocumentDelivery, Configuration);
			AssertEquals("Pre-condition", jobDocumentDelivery.JDC_BlindCarbonCopyRecipientsAsString, wrapper.BlindCarbonCopyRecipientsAsString);

			AssertEquals(2, wrapper.BlindCarbonCopyRecipients.Count);
			AssertCollectionContains(wrapper.BlindCarbonCopyRecipients.Cast<NonPersistentCopyRecipient>(), r => r.EmailAddress == "a@a.aa");
			AssertCollectionContains(wrapper.BlindCarbonCopyRecipients.Cast<NonPersistentCopyRecipient>(), r => r.EmailAddress == "b@b.bb");

			//Setting the wrapper property should refresh the list
			wrapper.BlindCarbonCopyRecipientsAsString = "c@c.cc, b@b.bb, d@d.dd";
			AssertEquals(3, wrapper.BlindCarbonCopyRecipients.Count);
			AssertCollectionContains(wrapper.BlindCarbonCopyRecipients.Cast<NonPersistentCopyRecipient>(), r => r.EmailAddress == "c@c.cc");
			AssertCollectionContains(wrapper.BlindCarbonCopyRecipients.Cast<NonPersistentCopyRecipient>(), r => r.EmailAddress == "b@b.bb");
			AssertCollectionContains(wrapper.BlindCarbonCopyRecipients.Cast<NonPersistentCopyRecipient>(), r => r.EmailAddress == "d@d.dd");

			//Setting the wrapped property should also refresh the list
			jobDocumentDelivery.JDC_BlindCarbonCopyRecipientsAsString = "f@f.ff, g@g.gg";
			AssertEquals(2, wrapper.BlindCarbonCopyRecipients.Count);
			AssertCollectionContains(wrapper.BlindCarbonCopyRecipients.Cast<NonPersistentCopyRecipient>(), r => r.EmailAddress == "f@f.ff");
			AssertCollectionContains(wrapper.BlindCarbonCopyRecipients.Cast<NonPersistentCopyRecipient>(), r => r.EmailAddress == "g@g.gg");
		}

		public void TestIsExclusion()
		{
			var jobDocumentDelivery = Factory.NewWithValidTestData<JobDocumentDelivery>();
			var wrapper = new JobDocumentRecipientWrapperForJobDocumentDelivery(jobDocumentDelivery, Configuration);
			var deliveryMethods = wrapper.DeliveryMethods.Cast<CodeDescriptionPair>().Where(p => p.Code != Core.Constants.ContactNotifyModes.DoNotDeliver);
			foreach (var deliveryMethod in deliveryMethods)
			{
				wrapper.DeliveryMethod = deliveryMethod.Code;
				Assert($"{nameof(wrapper.IsExclusion)} should be false for delivery method {deliveryMethod.Code}", !wrapper.IsExclusion);
			}
			wrapper.DeliveryMethod = Core.Constants.ContactNotifyModes.DoNotDeliver;
			Assert($"{nameof(wrapper.IsExclusion)} should be true for delivery method {Core.Constants.ContactNotifyModes.DoNotDeliver}", wrapper.IsExclusion);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var jobDocumentDelivery = Factory.New<JobDocumentDelivery>();
			return new JobDocumentRecipientWrapperForJobDocumentDelivery(jobDocumentDelivery, Configuration);
		}

		#endregion
	}
}
