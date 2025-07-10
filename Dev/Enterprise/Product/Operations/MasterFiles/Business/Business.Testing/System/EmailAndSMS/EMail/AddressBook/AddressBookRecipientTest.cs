using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AddressBookRecipient))]
	sealed class AddressBookRecipientTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			IAddressBookRecipient recipient = Factory.New<OrgContact>();
			return new AddressBookRecipient(recipient);
		}

		#endregion

		public void TestEquals()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "org1";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "org2";

			var pk1 = ZGuid.NewZGuid();
			var pk2 = ZGuid.NewZGuid();

			var recipientWithSamePK1 = AddressBookRecipientHelper.CreateRecipient(pk1, org1);
			var recipientWithSamePK2 = AddressBookRecipientHelper.CreateRecipient(pk1, org2);

			AssertEquals(false, recipientWithSamePK1.Equals(recipientWithSamePK2));

			var recipientWithSameOrg1 = AddressBookRecipientHelper.CreateRecipient(pk1, org1);
			var recipientWithSameOrg2 = AddressBookRecipientHelper.CreateRecipient(pk2, org1);

			AssertEquals(false, recipientWithSameOrg1.Equals(recipientWithSameOrg2));

			var sameRecipient1 = AddressBookRecipientHelper.CreateRecipient(pk1, org1);
			var sameRecipient2 = AddressBookRecipientHelper.CreateRecipient(pk1, org1);

			AssertEquals(true, sameRecipient1.Equals(sameRecipient2));
		}

		public void TestHashCode()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "org1";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "org2";

			var pk1 = ZGuid.NewZGuid();
			var pk2 = ZGuid.NewZGuid();

			var recipientWithSamePK1 = AddressBookRecipientHelper.CreateRecipient(pk1, org1);
			var recipientWithSamePK2 = AddressBookRecipientHelper.CreateRecipient(pk1, org2);

			AssertNotEquals(recipientWithSamePK1.GetHashCode(), recipientWithSamePK2.GetHashCode());

			var recipientWithSameOrg1 = AddressBookRecipientHelper.CreateRecipient(pk1, org1);
			var recipientWithSameOrg2 = AddressBookRecipientHelper.CreateRecipient(pk2, org1);

			AssertNotEquals(recipientWithSameOrg1.GetHashCode(), recipientWithSameOrg2.GetHashCode());

			var sameRecipient1 = AddressBookRecipientHelper.CreateRecipient(pk1, org1);
			var sameRecipient2 = AddressBookRecipientHelper.CreateRecipient(pk1, org1);

			AssertEquals(sameRecipient1.GetHashCode(), sameRecipient2.GetHashCode());
		}
	}
}
