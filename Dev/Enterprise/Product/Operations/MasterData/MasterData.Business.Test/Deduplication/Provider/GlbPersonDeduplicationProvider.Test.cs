using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Glow.Model.Interfaces;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business.Tests
{
	public class GlbPersonDeduplicationProviderTest : TestCaseWithFactory
	{
		public void TestDeduplicationProvider()
		{
			var provider = new GlbPersonDeduplicationProvider();

			AssertEquals(DeduplicationDisplayMode.Detailed, provider.DisplayModeForType);
			AssertEquals(DeduplicationProvider.Constants.Person, provider.GroupNameForType);
			AssertEquals(typeof(IGlbPerson), provider.GlowType);
			AssertEquals(typeof(GlbPerson), provider.BusinessObjectType);
			AssertEquals(GlbPersonSchema.Constants.Prefix, provider.TablePrefix);
		}

		public void TestGetComparisonSource()
		{
			var provider = new GlbPersonDeduplicationProvider();
			var personInDB = Factory.NewWithValidTestData<GlbPerson>();

			Factory.Save();
			var dedupPerson = new MasterDataProvider().GetDeduplicationGlbPerson(personInDB) as DeduplicationGlbPerson;
			var source = provider.GetComparisonSource(dedupPerson, personInDB.PK.ToGuid(), null);

			AssertNotNull("Source is not null", source);
			AssertEquals(personInDB.PK, ((DeduplicationGlbPerson)source).PER_PK);

			var newPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dedupPersonNew = new MasterDataProvider().GetDeduplicationGlbPerson(newPerson) as DeduplicationGlbPerson;
			source = provider.GetComparisonSource(dedupPersonNew, newPerson.PK.ToGuid(), null);

			AssertNotNull("Source is not null", source);
			AssertEquals(newPerson.PK, ((DeduplicationGlbPerson)source).PER_PK);
		}

		public void TestGetHeading_WhenSourceIsKnown()
		{
			var provider = new GlbPersonDeduplicationProvider();
			var person = Factory.NewWithValidTestData<GlbPerson>();

			person.PER_FullName = "CCE";
			var dedupPerson = person.CreateIGlbPerson();
			var header = provider.GetHeading((IDeduplicationGlowObject)dedupPerson);

			AssertEquals("CCE", header);
		}

		public void TestGetHeading_WhenSourceIsEitherNewOrExisting()
		{
			var provider = new GlbPersonDeduplicationProvider();
			var person = Factory.NewWithValidTestData<GlbPerson>();

			person.PER_FriendlyName = "CEA";
			person.PER_FullName = "Capitol Hill";
			Factory.Save();
			var dedupePerson = new MasterDataProvider().GetDeduplicationGlbPerson(person) as DeduplicationGlbPerson;
			var dedupPerson = new List<IDeduplicationGlowObject>() { dedupePerson };
			var shortHeader = provider.GetHeading(dedupPerson, person.PK.ToGuid(), HeaderType.Short);
			var longHeader = provider.GetHeading(dedupPerson, person.PK.ToGuid(), HeaderType.Long);

			AssertEquals("Capitol Hill", shortHeader);
			AssertEquals("CEA: Capitol Hill", longHeader);
		}
	}
}
