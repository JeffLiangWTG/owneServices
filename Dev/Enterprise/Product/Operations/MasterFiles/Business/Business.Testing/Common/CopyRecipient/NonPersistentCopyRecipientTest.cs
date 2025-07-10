using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(NonPersistentCopyRecipient))]
	class NonPersistentCopyRecipientTest : NonPersistentBusinessObjectTestCase
	{
		public void TestOrganizationAndType()
		{
			// Arrange
			var copyRecipient = (NonPersistentCopyRecipient)CachedBusinessObject;
			// Assert
			AssertEquals(Organization, copyRecipient.Organization);
			AssertEquals(Constants.CopyRecipientType.CarbonCopyRecipient, copyRecipient.Type);
		}

		public void TestAvailableEmailAddressList()
		{
			// Arrange
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_Email = "test1@test.com";
			contact1.OC_OH = Organization.PK;
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_Email = "test2@test.com";
			contact2.OC_OH = Organization.PK;
			var copyRecipient = (NonPersistentCopyRecipient)CachedBusinessObject;
			// Act
			var availableEmailAddresses = copyRecipient.AvailableEmailAddressList.ToArray();
			// Assert
			AssertEquals(2, availableEmailAddresses.Length);
			AssertCollectionContains(availableEmailAddresses, cdp => cdp.Code == contact1.Email);
			AssertCollectionContains(availableEmailAddresses, cdp => cdp.Code == contact2.Email);
		}

		public void TestEmailAddressAttributeIsDefined()
		{
			Assert(Attribute.IsDefined(typeof(NonPersistentCopyRecipient).GetProperty("EmailAddress"), typeof(EmailAddressAttribute)));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NonPersistentCopyRecipient(Organization, Constants.CopyRecipientType.CarbonCopyRecipient);
		}

		OrgHeader Organization
		{
			get { return organization ?? (organization = Factory.NewWithValidTestData<OrgHeader>()); }
		}

		OrgHeader organization;
	}
}
