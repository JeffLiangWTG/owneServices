using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class MessageTriggerPartyCollectionTest : TestCaseWithFactory
	{
		public void TestNewMessageRecipientParty()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			var recipientParty = new MessageRecipientParty(org1, "a@b.c");
			AssertEquals(org1, recipientParty.Party);
			AssertEquals("a@b.c", recipientParty.FallbackEmail);

			recipientParty = new MessageRecipientParty(org2, ZString.Empty);
			AssertEquals(org2, recipientParty.Party);
			Assert(recipientParty.FallbackEmail.IsEmpty);

			recipientParty = new MessageRecipientParty(null, "x@y.z");
			AssertNull(recipientParty.Party);
			AssertEquals("x@y.z", recipientParty.FallbackEmail);

			recipientParty = new MessageRecipientParty(org1, ZString.Empty, "x@y.z");
			AssertEquals(org1, recipientParty.Party);
			AssertEquals("x@y.z", recipientParty.FallbackEmail);

			recipientParty = new MessageRecipientParty(org1, "a@b.c", ZString.Empty);
			AssertEquals(org1, recipientParty.Party);
			AssertEquals("a@b.c", recipientParty.FallbackEmail);

			recipientParty = new MessageRecipientParty(org1, "a@b.c", "x@y.z");
			AssertEquals(org1, recipientParty.Party);
			AssertEquals("a@b.c", recipientParty.FallbackEmail);
		}

		public void TestNewMessageRecipientPartyFromOrgAddress()
		{
			var org = Factory.New<OrgHeader>();
			var address = Factory.New<OrgAddress>();
			address.OA_OH = org.PK;
			address.OA_Email = "a@b.c";

			var recipientParty = new MessageRecipientParty(address);
			AssertEquals(org, recipientParty.Party);
			AssertEquals("a@b.c", recipientParty.FallbackEmail);

			address.OA_OH = ZGuid.Empty;
			recipientParty = new MessageRecipientParty(address);
			AssertNull(recipientParty.Party);
			Assert(recipientParty.FallbackEmail.IsEmpty);

			recipientParty = new MessageRecipientParty((OrgAddress)null);
			AssertNull(recipientParty.Party);
			Assert(recipientParty.FallbackEmail.IsEmpty);
		}

		public void TestNewMessageRecipientPartyFromJobDocAddress()
		{
			var org = Factory.New<OrgHeader>();

			var address = Factory.New<OrgAddress>();
			address.OA_OH = org.PK;
			address.OA_Email = "address1";

			var contact = Factory.New<OrgContact>();
			contact.OC_OH = org.PK;
			contact.OC_Email = "address2";

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_OA_Address = address.PK;
			docAddress.ContactPK = contact.PK;
			docAddress.E2_Email = "address3";

			docAddress.E2_AddressOverride = true;
			var recipientParty = new MessageRecipientParty(docAddress);
			AssertNull("Address is overridden - no org header", recipientParty.Party);
			Assert(recipientParty.FallbackEmail.IsEmpty);

			docAddress.E2_AddressOverride = false;
			docAddress.ContactPK = contact.PK;
			recipientParty = new MessageRecipientParty(docAddress);
			AssertEquals(org, recipientParty.Party);
			AssertEquals("address2", recipientParty.FallbackEmail);

			contact.OC_Email = ZString.Empty;
			recipientParty = new MessageRecipientParty(docAddress);
			AssertEquals(org, recipientParty.Party);
			AssertEquals("address1", recipientParty.FallbackEmail);

			docAddress = Factory.New<JobDocAddress>();
			recipientParty = new MessageRecipientParty(docAddress);
			AssertNull(recipientParty.Party);
			Assert(recipientParty.FallbackEmail.IsEmpty);
		}

		public void TestAddNotNullAndNotDuplicatedItem()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			var partyCollection = new MessageRecipientPartyCollection();
			AssertEquals(0, partyCollection.Count());

			partyCollection.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(org1, "a@b.c"));
			AssertEquals(1, partyCollection.Count());

			partyCollection.AddNotNullAndNotDuplicatedItem(null);
			AssertEquals("Null should not be added", 1, partyCollection.Count());

			partyCollection.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(null, "d@e.f"));
			AssertEquals("Null should not be added", 1, partyCollection.Count());

			partyCollection.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(org1, "g@h.i"));
			AssertEquals("Same party should not be added", 1, partyCollection.Count());

			partyCollection.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(org2, "j@k.l"));
			AssertEquals(2, partyCollection.Count());
		}
	}
}
