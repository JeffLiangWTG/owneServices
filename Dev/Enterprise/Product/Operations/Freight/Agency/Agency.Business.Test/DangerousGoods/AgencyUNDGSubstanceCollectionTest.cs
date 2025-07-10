using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyUNDGSubstanceCollection))]
	internal class AgencyUNDGSubstanceCollectionTest : ActiveBusinessObjectCollectionTestCase<AgencyUNDGSubstanceCollection>
	{
		public void TestFindBoxListProvider()
		{
			var query = new ZQuery();
			query.AddToFilter(UNDGSubstanceSchema.DG_UNNO, "TEST");
			var collection = new AgencyUNDGSubstanceCollectionForTest(Factory, query);
			AssertNotEquals(null, collection.FindBoxListProviderForTest);
			AssertType<AgencyUNDGSubstanceCollectionFindBoxListProvider>(collection.FindBoxListProviderForTest);
		}

		#region Implementation
		class AgencyUNDGSubstanceCollectionForTest : AgencyUNDGSubstanceCollection
		{
			public AgencyUNDGSubstanceCollectionForTest(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter, false)
			{
			}

			public IFindBoxListProvider FindBoxListProviderForTest => base.FindBoxListProvider;
		}

		#endregion Implementation
	}
}
