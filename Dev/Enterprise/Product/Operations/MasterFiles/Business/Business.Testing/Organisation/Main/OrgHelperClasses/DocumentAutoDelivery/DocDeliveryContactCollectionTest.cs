using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DocDeliveryContactCollection))]
	sealed class DocDeliveryContactCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocDeliveryContactCollection>
	{
		public void TestInitiliseChild()
		{
			var menu = Factory.New<StmMenuItem>();
			var overridenAddress = Factory.New<JobDocAddress>();
			var collection = new DocDeliveryContactCollection(menu, overridenAddress, Factory);
			var child = collection.AddNew();
			AssertEquals(menu.PK, child.MenuItem.PK);
			AssertEquals(overridenAddress.PK, ((JobDocAddress)child.OverridenAddress).PK);
		}

		public void TestDeliverables()
		{
			AssertNull("Deliverables", Collection.Deliverables);
			var deliverables = new DocDeliveryContactCollection(Factory);
			Collection.Deliverables = deliverables;
			AssertEquals("Deliverables", deliverables, Collection.Deliverables);
		}

		public void TestDisableAttachmentType()
		{
			var existingContact = Collection.AddNew();
			Assert("Existing contact has normal attachment types.", existingContact.AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.PDF));
			Assert("Existing contact has normal attachment types.", existingContact.AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.XLS));
			Assert("Existing contact has normal attachment types.", existingContact.AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.TIF));

			Collection.DisableAttachmentType(true);

			AssertEquals("Existing contact has no attachment types.", 0, existingContact.AttachmentTypes.Count);

			var newContact = Collection.AddNew();

			AssertEquals("New contact gets no attachment types", 0, newContact.AttachmentTypes.Count);

			var newContact2 = new DocDeliveryContact(Factory);
			Collection.Add(newContact2);

			AssertEquals("Existing contact added to this collection gets no attachment types.", 0, newContact2.AttachmentTypes.Count);
		}

		public void TestOverrideAttachmentTypes()
		{
			var existingContact = Collection.AddNew();
			Assert("Existing contact has normal attachment types.", existingContact.AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.PDF));
			Assert("Existing contact has normal attachment types.", existingContact.AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.XLS));
			Assert("Existing contact has normal attachment types.", existingContact.AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.TIF));

			Collection.OverrideAttachmentTypeListOnChildren(GetOverriddenList());

			AssertEquals("Existing contact has the new attachment types.", 2, existingContact.AttachmentTypes.Count);
			Assert("Existing contact has new attachment types.", existingContact.AttachmentTypes.ContainsCode("Daph"));
			Assert("Existing contact has new attachment types.", existingContact.AttachmentTypes.ContainsCode("Zubs"));

			var newContact = Collection.AddNew();

			AssertEquals("New contact gets the new attachment types", 2, newContact.AttachmentTypes.Count);
			Assert("New contact gets the new attachment types", newContact.AttachmentTypes.ContainsCode("Daph"));
			Assert("New contact gets the new attachment types", newContact.AttachmentTypes.ContainsCode("Zubs"));

			var newContact2 = new DocDeliveryContact(Factory);
			Collection.Add(newContact2);

			AssertEquals("Existing contact added to this collection gets the new attachment types.", 2, newContact2.AttachmentTypes.Count);
			Assert("Existing contact added to this collection gets the new attachment types.", newContact2.AttachmentTypes.ContainsCode("Daph"));
			Assert("Existing contact added to this collection gets the new attachment types.", newContact2.AttachmentTypes.ContainsCode("Zubs"));

			Collection.OverrideAttachmentTypeListOnChildren(null);

			Assert("Existing contact has normal attachment types.", existingContact.AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.PDF));
			Assert("Existing contact has normal attachment types.", existingContact.AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.XLS));
			Assert("Existing contact has normal attachment types.", existingContact.AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.TIF));

			Assert("New contact has normal attachment types.", newContact.AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.PDF));
			Assert("New contact has normal attachment types.", newContact.AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.XLS));
			Assert("New contact has normal attachment types.", newContact.AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.TIF));

			Assert("New contact 2 has normal attachment types.", newContact2.AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.PDF));
			Assert("New contact 2 has normal attachment types.", newContact2.AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.XLS));
			Assert("New contact 2 has normal attachment types.", newContact2.AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.TIF));
		}

		#region Implementation

		new DocDeliveryContactCollection Collection => base.Collection;

		protected override DocDeliveryContactCollection GetCollectionToTest()
		{
			return new DocDeliveryContactCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DocDeliveryContact(Factory);
		}

		CodeDescriptionPairList GetOverriddenList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair("Daph", "Daph");
			result.AddPair("Zubs", "Zubs");
			return result;
		}

		#endregion
	}
}
