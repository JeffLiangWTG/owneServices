using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(MergeOrgContact))]
	sealed class MergeOrgContactTest : MergeOrgElementTest
	{
		public override void TestValidateFuzzyMatching()
		{
			MergeOrgContact mergeOrgContact = NewTestMergeElement("jack the eviscerator", "a@b.c") as MergeOrgContact;
			AssertEquals("Default action should be ADD", "ADD", mergeOrgContact.Action);

			AddNewOrg(mergeOrgContact, "unknown", "a@b.c");
			AssertEquals("Action should be Merge", "MRG", mergeOrgContact.Action);
			OrgContact matchingContact = Factory.Load<OrgContact>(mergeOrgContact.NewObjectPK);
			AssertEquals("New Contact should be 'unknown'", "unknown", matchingContact.OC_ContactName);

			AddNewOrg(mergeOrgContact, "jack the eviscerator", "weird text");
			AssertEquals("Action should be Merge", "MRG", mergeOrgContact.Action);
			matchingContact = Factory.Load<OrgContact>(mergeOrgContact.NewObjectPK);
			AssertEquals("New Contact should be 'jack the eviscerator'", "jack the eviscerator", matchingContact.OC_ContactName);

			AddNewOrg(mergeOrgContact, "ivan durak", "admin@microsoft.com");
			AssertEquals("Action should be Add", "ADD", mergeOrgContact.Action);

			mergeOrgContact = NewTestMergeElement("julius caesar", "asd") as MergeOrgContact;
			AssertEquals("Default action should be ADD", "ADD", mergeOrgContact.Action);

			AddNewOrg(mergeOrgContact, "julius", "xxx");
			AssertEquals("Action should be Add", "ADD", mergeOrgContact.Action);

			mergeOrgContact = NewTestMergeElement("julius caesar", "asd") as MergeOrgContact;
			AssertEquals("Default action should be ADD", "ADD", mergeOrgContact.Action);

			AddNewOrg(mergeOrgContact, "JULIUS CAESAR", "xxx");
			AssertEquals("Action should be Merge", "MRG", mergeOrgContact.Action);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return NewTestMergeElement() as BusinessObject;
		}

		protected override IMergeOrgElement NewTestMergeElement(BusinessObjectCollection newOrgElementCollection)
		{
			OrgHeader newOrg = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact newContact = Factory.NewWithValidTestData<OrgContact>();
			newContact.OC_ContactName = "pupkin";
			newContact.OC_OH = newOrg.PK;

			OrgContact oldContact = Factory.NewWithValidTestData<OrgContact>();
			oldContact.OC_ContactName = "gimli";

			Factory.Save();

			return new MergeOrgContact(Factory, oldContact, newOrg, newOrgElementCollection);
		}

		protected override IMergeOrgElement NewTestMergeElement(ZString oldContactName, ZString email)
		{
			OrgContact oldContact = Factory.NewWithValidTestData<OrgContact>();
			oldContact.OC_ContactName = oldContactName;
			oldContact.OC_Email = email;

			Factory.Save();

			return new MergeOrgContact(Factory, oldContact, null);
		}

		void AddNewOrg(MergeOrgContact testMergeContact, ZString newContactName, ZString email)
		{
			OrgHeader newOrg = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact newContact = Factory.NewWithValidTestData<OrgContact>();
			newContact.OC_ContactName = newContactName;
			newContact.OC_Email = email;
			newContact.OC_OH = newOrg.PK;

			newOrg.Contacts.Add(newContact);

			Factory.Save();

			testMergeContact.NewOrganization = newOrg;
		}

		protected override BusinessObjectCollection GetMergeOrgElementCollection()
		{
			return new MergeOrgContactCollection(Factory);
		}

		protected override SchemaColumn[] ColumnsFotMatching
		{
			get { return new SchemaColumn[] { OrgContactSchema.OC_Email, OrgContactSchema.OC_ContactName }; }
		}

		protected override Type MasterBusinessObjectType { get { return typeof(OrgContact); } }
	}
}
