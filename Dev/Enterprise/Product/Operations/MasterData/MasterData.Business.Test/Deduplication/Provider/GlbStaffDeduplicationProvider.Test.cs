using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Glow.Model.Interfaces;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business.Tests
{
	public class GlbStaffDeduplicationProviderTest : TestCaseWithFactory
	{
		public void TestDeduplicationProvider()
		{
			var provider = new GlbStaffDeduplicationProvider();

			CombineAssertions(() =>
			{
				AssertEquals(DeduplicationDisplayMode.Detailed, provider.DisplayModeForType);
				AssertEquals(DeduplicationProvider.Constants.Staff, provider.GroupNameForType);
				AssertEquals(typeof(IGlbStaff), provider.GlowType);
				AssertEquals(typeof(GlbStaff), provider.BusinessObjectType);
				AssertEquals(GlbStaffSchema.Constants.Prefix, provider.TablePrefix);
			});
		}

		public void TestGetComparisonSource()
		{
			var provider = new GlbStaffDeduplicationProvider();
			var personInDB = Factory.NewWithValidTestData<GlbPerson>();
			var staffInDB = Factory.NewWithValidTestData<GlbStaff>();
			staffInDB.GS_PER = personInDB.PK;

			Factory.Save();
			var dedupPerson = new MasterDataProvider().GetDeduplicationGlbPerson(personInDB) as DeduplicationGlbPerson;
			var source = provider.GetComparisonSource(dedupPerson, staffInDB.PK.ToGuid(), null);

			AssertNotNull("Source is not null", source);
			AssertEquals(staffInDB.PK, ((DeduplicationGlbStaff)source).GS_PK);

			var newPerson = Factory.NewWithValidTestData<GlbPerson>();
			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_PER = newPerson.PK;
			var dedupPersonNew = new MasterDataProvider().GetDeduplicationGlbPerson(newPerson) as DeduplicationGlbPerson;
			source = provider.GetComparisonSource(dedupPersonNew, newStaff.PK.ToGuid(), null);

			AssertNotNull("Source is not null", source);
			AssertEquals(newStaff.PK, ((DeduplicationGlbStaff)source).GS_PK);

			source = provider.GetComparisonSource(new[] { dedupPersonNew }, newStaff.PK.ToGuid(), null);
			AssertNotNull("Source is not null", source);
			AssertEquals(newStaff.PK, ((DeduplicationGlbStaff)source).GS_PK);
		}

		public void TestGetHeading_WhenSourceIsKnown()
		{
			var provider = new GlbStaffDeduplicationProvider();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "ABC";

			var dedupStaff = new DeduplicationGlbStaff(staff, null);
			var header = provider.GetHeading(dedupStaff);

			AssertEquals("ABC", header);
		}

		public void TestGetHeading_WhenSourceIsEitherNewOrExisting()
		{
			var provider = new GlbStaffDeduplicationProvider();
			var person = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_PER = person.PK;
			staff.GS_FriendlyName = "CEA";
			staff.GS_FullName = "Capitol Hill";
			Factory.Save();

			var dedupPerson = new List<IGlbPerson>() { person.CreateIGlbPerson() };
			var shortHeader = provider.GetHeading(dedupPerson, staff.PK.ToGuid(), HeaderType.Short);
			var longHeader = provider.GetHeading(dedupPerson, staff.PK.ToGuid(), HeaderType.Long);

			AssertEquals("Capitol Hill", shortHeader);
			AssertEquals("CEA: Capitol Hill", longHeader);

			staff.GS_FriendlyName = "";
			dedupPerson = new List<IGlbPerson>() { person.CreateIGlbPerson() };
			longHeader = provider.GetHeading(dedupPerson, staff.PK.ToGuid(), HeaderType.Long);
			AssertEquals("Capitol Hill", longHeader);
		}

		public void TestGetChildObject()
		{
			var provider = new GlbStaffDeduplicationProvider();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var person = GlbPerson.CreateFromStaff(Factory, staff);
			var dedupPerson = person.CreateIGlbPerson();

			CombineAssertions(() =>
			{
				AssertNull(provider.GetChildObject(new object() as IDeduplicationMaster, staff.PK.ToGuid()));
				AssertNull(provider.GetChildObject(null, staff.PK.ToGuid()));
				AssertNull(provider.GetChildObject((IDeduplicationMaster)dedupPerson, Guid.NewGuid()));
				AssertNotNull(provider.GetChildObject((IDeduplicationMaster)dedupPerson, staff.PK.ToGuid()));
			});
		}
	}
}
