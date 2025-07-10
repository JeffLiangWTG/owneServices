using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterData.Business.Tests
{
	[TestedType(typeof(PersonMergeBusinessObject))]
	public class PersonMergeBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestActiveAssociationsAsPerPersonIsDissolving()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();

			PersonMergerTest.PersonAssociations.AddNewContactToPerson(Factory, person);
			var bizO = new PersonMergeBusinessObjectForTest(person);
			PersonMergerTest.PersonAssociations.AddNewHRJobApplicantToPerson(Factory, bizO.Person);

			AssertEquals("activeAssociationsCache should be Contact (1)", "Contact (1)", bizO.activeAssociationsCacheForTest);
			AssertEquals("person's TypeInfo should be Contact (1), Applicant (1)", "Contact (1), Applicant (1)", person.TypeInfo);

			person.IsDissolving = true;
			AssertEquals("If person's IsDissolving is true, ActiveAssociations should equal activeAssociationsCache", "Contact (1)", bizO.ActiveAssociations);

			person.IsDissolving = false;
			AssertEquals("If person's IsDissolving is false, ActiveAssociations should equal person's TypeInfo", "Contact (1), Applicant (1)", bizO.ActiveAssociations);
		}

		public void TestPersonMergeBusinessObject_Default()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "Juan Dela Cruz";

			PersonMergerTest.PersonAssociations.AddNewStaffToPerson(Factory, person);
			PersonMergerTest.PersonAssociations.AddNewContactToPerson(Factory, person);
			PersonMergerTest.PersonAssociations.AddNewHRJobApplicantToPerson(Factory, person);

			Factory.Save();

			var bizO = new PersonMergeBusinessObject(person);

			Assert("Precondition", string.IsNullOrEmpty(bizO.MergeStatus));
			AssertEquals(ZString.Empty, bizO.MergeErrorMessage);
			bizO.MergeStatus = ParticipantStatus.Queued;
			bizO.MergeErrorMessage = new Exception().ToString();

			AssertEquals("System.Exception: Exception of type 'System.Exception' was thrown.", bizO.MergeErrorMessage);
			AssertEquals(person.PER_FullName, bizO.FullName);
			AssertEquals("Contact (1), Staff (1), Applicant (1)", bizO.ActiveAssociations);
			AssertEquals("Queued", bizO.MergeStatus);
		}
	}

	public class PersonMergeBusinessObjectForTest : PersonMergeBusinessObject
	{
		public PersonMergeBusinessObjectForTest(GlbPerson person) : base(person)
		{
		}

		internal ZString activeAssociationsCacheForTest => activeAssociationsCache;
	}
}
