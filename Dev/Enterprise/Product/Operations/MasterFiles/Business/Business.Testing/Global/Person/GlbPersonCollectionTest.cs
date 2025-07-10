using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbPersonCollection))]
	sealed class GlbPersonCollectionTest : ActiveBusinessObjectCollectionTestCase<GlbPersonCollection>
	{
		public void TestIsNoResultsQueryWhenMasterIsSpecified()
		{
			var collection1 = new GlbPersonCollection(Factory);
			var collection2 = new GlbPersonCollection(Factory.New<GlbPerson>(), Array.Empty<ZGuid>(), Array.Empty<ZGuid>(), Array.Empty<ZGuid>());

			AssertEquals(false, collection1.CompleteFilter.IsNoResultQuery);
			AssertEquals(true, collection2.CompleteFilter.IsNoResultQuery);
		}

		public void TestSetDefaults()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "name";
			person.PER_EmailAddress = "email@addr.ess";
			person.PER_BirthDate = new ZDate(2001, 1, 1);
			person.PER_Gender = Core.Constants.Genders.Man;
			person.PER_WebAccessEnabled = true;

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "org name";
			org.OH_Code = "~code";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "contact1@email.com";
			contact1.OC_ContactName = "contact1";
			contact1.OC_PER = person.PK;
			contact1.OC_Birthday = new ZDateTime(2002, 1, 1);
			person.SetPrimaryRelationship(contact1);

			var contact2 = org.Contacts.AddNew();
			contact2.OC_Email = "contact2@email.com";
			contact2.OC_ContactName = "contact2";
			contact2.OC_PER = person.PK;
			contact2.OC_Birthday = new ZDateTime(2003, 1, 1);

			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_EmailAddress = "staff1@email.com";
			staff1.GS_FullName = "staff name 1";
			staff1.GS_PER = person.PK;
			staff1.GS_Birthdate = new ZDate(2004, 1, 1);
			staff1.GS_LoginName = "login1";

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_EmailAddress = "staff2@email.com";
			staff2.GS_FullName = "staff name 2";
			staff2.GS_PER = person.PK;
			staff2.GS_Birthdate = new ZDate(2005, 1, 1);
			staff2.GS_LoginName = "login2";

			var applicant1 = Factory.New<IHRJobApplicant>();
			applicant1.HA_FullName = "applicant 1";
			applicant1.HA_EmailAddress = "applicant1@email.com";
			applicant1.HA_Birthdate = new ZDate(2006, 1, 1);
			applicant1.HA_PER = person.PK;

			var applicant2 = Factory.New<IHRJobApplicant>();
			applicant2.HA_FullName = "applicant 2";
			applicant2.HA_EmailAddress = "applicant2@email.com";
			applicant2.HA_Birthdate = new ZDate(2007, 1, 1);
			applicant2.HA_PER = person.PK;

			Factory.Save();

			var collection = new GlbPersonCollection(person, new[] { staff1.PK }, new[] { contact1.PK }, new[] { applicant1.PK });
			var newPerson = collection.AddNew();

			newPerson.ApplicantCollection.Load();
			AssertEquals(1, newPerson.ApplicantCollection.Count);
			var applicant = newPerson.ApplicantCollection.First() as IHRJobApplicant;
			AssertEquals(applicant1.PK, applicant.PK);

			newPerson.ContactCollection.Load();
			AssertEquals(1, newPerson.ContactCollection.Count);
			AssertEquals(contact1.PK, newPerson.ContactCollection[0].PK);

			AssertEquals(1, newPerson.StaffCollection.Count);
			AssertEquals(staff1.PK, newPerson.StaffCollection[0].PK);

			AssertEquals(false, newPerson.PER_WebAccessEnabled);

			AssertEquals(contact1.PK, newPerson.PrimarySource.PK);
		}

		public void TestModuleID()
		{
			var attributes = (ModuleIDAttribute[])typeof(GlbPersonCollection).GetCustomAttributes(typeof(ModuleIDAttribute), true);
			AssertEquals(1, attributes.Length);
			AssertEquals(ModuleIDs.CusPerson, attributes[0].ModuleIdentifier);
		}

		protected override GlbPersonCollection GetCollectionToTest()
		{
			return new GlbPersonCollection(Factory);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(GlbPersonCollection);
		}

		[StressTest]
		public override void TestAdd()
		{
			base.TestAdd();
		}

		[StressTest]
		public override void TestAddNew()
		{
			base.TestAddNew();
		}

		[StressTest]
		public override void TestAddAndDeleteOfElementAsThoughBinding()
		{
			base.TestAddAndDeleteOfElementAsThoughBinding();
		}

		[StressTest]
		public override void TestRemoveFromRelationship()
		{
			base.TestRemoveFromRelationship();
		}

		[StressTest]
		public override void TestCancelNew_DoesNotReport()
		{
			base.TestCancelNew_DoesNotReport();
		}

		[StressTest]
		public override void TestDelete()
		{
			base.TestDelete();
		}

		[StressTest]
		public override void TestTypedget_Item()
		{
			base.TestTypedget_Item();
		}

		[StressTest]
		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			base.TestAddAndCancelOfElementAsThoughBinding();
		}
	}
}
