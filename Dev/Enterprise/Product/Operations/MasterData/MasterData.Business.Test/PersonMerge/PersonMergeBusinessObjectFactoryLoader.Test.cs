using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business.Tests
{
	class PersonMergeBusinessObjectFactoryLoaderTest : TestCaseWithFactory
	{
		public void TestReload_MultiMergeMode()
		{
			var emailAddress = $"{Guid.NewGuid()}@email.com";
			var retainedCollection = new PersonMergeBusinessObjectCollection();
			var dissolvedCollection = new PersonMergeBusinessObjectCollection();
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();

			dissolvedPerson1.PER_EmailAddress = emailAddress;
			dissolvedPerson2.PER_HomePhone = "+61 3 1415 9265";

			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson1));
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson2));
			retainedCollection.Add(new PersonMergeBusinessObject(retainedPerson));

			Factory.Save();

			var merger = new MultiPersonMergerTest.MultiPersonMergerForTest(retainedCollection, dissolvedCollection);
			merger.MergeSelected().Wait();

			CombineAssertions(() =>
			{
				AssertNull(Factory.Load<GlbPerson>(dissolvedPerson1.PK));
				AssertEquals(emailAddress, retainedPerson.PER_EmailAddress);
				AssertEquals("+61 3 1415 9265", retainedPerson.PER_HomePhone);
			});
		}

		public void TestReload_SingleMergeMode()
		{
			var emailAddress = $"{Guid.NewGuid()}@email.com";
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var idpUserId = Guid.NewGuid();
			dissolvedPerson.PER_EmailAddress = emailAddress;
			dissolvedPerson.PER_HomePhone = "+61 3 1415 9265";
			dissolvedPerson.PER_MobilePhone = "+61123456780";
			dissolvedPerson.PER_MobilePhone2 = "+61123456781";
			dissolvedPerson.PER_FaxNumber = "+61123456782";
			dissolvedPerson.PER_IDPUserId = idpUserId;

			Factory.Save();

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				merger.Merge();
			}

			CombineAssertions(() =>
			{
				AssertNull(Factory.Load<GlbPerson>(dissolvedPerson.PK));
				AssertEquals(emailAddress, retainedPerson.PER_EmailAddress);
				AssertEquals("+61 3 1415 9265", retainedPerson.PER_HomePhone);
				AssertEquals("+61123456780", retainedPerson.PER_MobilePhone);
				AssertEquals("+61123456781", retainedPerson.PER_MobilePhone2);
				AssertEquals("+61123456782", retainedPerson.PER_FaxNumber);
				AssertEquals(idpUserId, retainedPerson.PER_IDPUserId);
			});
		}

		public void TestReload_ReloadsStaffCorrectly()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var retainedStaff = Factory.NewWithValidTestData<GlbStaff>();
			retainedStaff.GS_PER = retainedPerson.PK;

			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedStaff = Factory.NewWithValidTestData<GlbStaff>();
			dissolvedStaff.GS_PER = dissolvedPerson.PK;

			Factory.Save();

			var retainedCollection = new PersonMergeBusinessObjectCollection();
			var dissolvedCollection = new PersonMergeBusinessObjectCollection();
			retainedCollection.Add(new PersonMergeBusinessObject(retainedPerson));
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson));

			var retainedPersonMergeObject = new PersonMergeBusinessObject(retainedPerson);

			AssertEquals("Precondition: retained person has 1 staff", "Staff (1)", retainedPersonMergeObject.ActiveAssociations);

			var merger = new MultiPersonMergerTest.MultiPersonMergerForTest(retainedCollection, dissolvedCollection);
			merger.MergeSelected().Wait();

			AssertEquals("StaffCollection of retained person is updated from database", "Staff (2)", retainedPersonMergeObject.ActiveAssociations);
		}
	}
}
