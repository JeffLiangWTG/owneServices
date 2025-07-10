using System;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterData.Common;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EmailContactItem))]
	sealed class EmailContactItemTest : NonPersistentBusinessObjectTestCase
	{
		public void TestOI_Address_MaxLength()
		{
			var emailItem = GetNewEmailContactItem();
			AssertEquals(OrgContact.Schema.OC_EmailMaxLength, emailItem.OI_AddressInfo.MaxLength);
			Assert("Max length must be less than or equal OI_AddressMaxLength", emailItem.OI_AddressInfo.MaxLength <= OrgContactItem.Schema.OI_AddressMaxLength);
		}

		public void TestDefaultValues()
		{
			var emailItem = GetNewEmailContactItem();
			AssertEquals(OrgContactItemTypes.Codes.Email, emailItem.OI_ContactItemType);
		}

		public void TestSettingAddressForOrgInvokesDeduplication()
		{
			//Arrange
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			var dedupeStarted = false;
			testOrg.DeduplicationStarted += (o, e) => { dedupeStarted = true; };
			((IDeduplicatable)testOrg).ShouldRunDeduplication = true;

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				//Act
				var contact = testOrg.Contacts.AddNew();
				var item = contact.EmailContactItems.AddNew();
				item.OI_Address = "test@email.com";

				//Assert
				AssertEquals(true, dedupeStarted);

				contact.EmailContactItems.RemoveAndDeleteAll();
			}
		}

		public void TestSettingAddressForOrgDoesNotInvokeDeduplication()
		{
			//Arrange
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			var dedupeStarted = false;
			testOrg.DeduplicationStarted += (o, e) => { dedupeStarted = true; };
			((IDeduplicatable)testOrg).ShouldRunDeduplication = false;

			//Act
			var contact = testOrg.Contacts.AddNew();
			var item = contact.EmailContactItems.AddNew();
			item.OI_Address = "test@email.com";

			//Assert
			AssertEquals(false, dedupeStarted);

			contact.EmailContactItems.RemoveAndDeleteAll();
		}

		public void TestNotInvokeDeduplicationWhenEmailContactItemsContainsError()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var testPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dedupeStarted = false;
			testPerson.DeduplicationStarted += (o, e) => { dedupeStarted = true; };
			((IDeduplicatable)testPerson).ShouldRunDeduplication = true;
			var contact = testPerson.ContactCollection.AddNew();
			contact.OC_PER = testPerson.PK;

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				contact.OC_OH = orgHeader.PK;
				var item = contact.EmailContactItems.AddNew();
				item.OI_Address = "INVALID";
				AssertEquals(false, dedupeStarted);

				item.OI_Address = "test@email.com";
				AssertEquals(true, dedupeStarted);

				contact.EmailContactItems.RemoveAndDeleteAll();
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewEmailContactItem();
		}

		EmailContactItem GetNewEmailContactItem()
		{
			var contact = Factory.New<OrgContact>();
			var emailContactItem = new EmailContactItem(contact);
			return emailContactItem;
		}

		#endregion
	}
}
