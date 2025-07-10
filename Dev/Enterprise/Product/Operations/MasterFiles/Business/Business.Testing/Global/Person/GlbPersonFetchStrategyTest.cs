using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbPersonFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForViewCore()
		{
			var factory = new BusinessObjectFactory();
			var person1 = factory.NewWithValidTestData<GlbPerson>();
			person1.PER_FullName = "JEFF1";
			var primaryRelationship1 = factory.NewWithValidTestData<GlbPersonPrimaryRelationship>();
			primaryRelationship1.PPR_PER = person1.PK;
			primaryRelationship1.PPR_PrimaryTableCode = "OC";

			var person2 = factory.NewWithValidTestData<GlbPerson>();
			person2.PER_FullName = "JEFF2";
			var primaryRelationship2 = factory.NewWithValidTestData<GlbPersonPrimaryRelationship>();
			primaryRelationship2.PPR_PER = person2.PK;
			primaryRelationship2.PPR_PrimaryTableCode = "OC";

			var person3 = factory.NewWithValidTestData<GlbPerson>();
			person3.PER_FullName = "JEFF3";
			var primaryRelationship3 = factory.NewWithValidTestData<GlbPersonPrimaryRelationship>();
			primaryRelationship3.PPR_PER = person3.PK;
			primaryRelationship3.PPR_PrimaryTableCode = "OC";

			var person4 = factory.NewWithValidTestData<GlbPerson>();
			person4.PER_FullName = "JEFF4";
			var primaryRelationship4 = factory.NewWithValidTestData<GlbPersonPrimaryRelationship>();
			primaryRelationship4.PPR_PER = person4.PK;
			primaryRelationship4.PPR_PrimaryTableCode = "OC";

			factory.Save();

			var anotherFactory = new BusinessObjectFactory();

			var persons = anotherFactory.Load<GlbPerson>(new ZQuery(GlbPersonSchema.PER_FullName, SQLComparisonOperator.StartsWith, "JEFF"));

			foreach (var person in persons)
			{
				person.FetchStrategy.FetchForView(new[] { new TableColumn("", "PrimarySource") });
			}

			foreach (var person in persons)
			{
				var primaryRelationship = person.PrimaryRelationship;
			}

			var expectedConditionDbHits = new Dictionary<string, int>()
			{
				{ GlbPersonSchema.Constants.TableName, 1 },
				{ GlbPersonPrimaryRelationshipSchema.Constants.TableName, 1 }
			};

			AssertDbHits(expectedConditionDbHits, anotherFactory);
		}
	}
}
