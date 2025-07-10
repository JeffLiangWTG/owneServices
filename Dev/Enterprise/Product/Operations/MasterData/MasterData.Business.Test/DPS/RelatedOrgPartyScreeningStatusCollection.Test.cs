using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterData.Business.Tests
{
	[TestedType(typeof(RelatedOrgPartyScreeningStatusCollection))]
	class RelatedOrgPartyScreeningStatusCollectionTest : ActiveBusinessObjectCollectionTestCase<RelatedOrgPartyScreeningStatusCollection>
	{
		protected override RelatedOrgPartyScreeningStatusCollection GetCollectionToTest()
		{
			return new RelatedOrgPartyScreeningStatusCollection(Factory, new ZQuery());
		}

		public void TestSort()
		{
			var collection = new RelatedOrgPartyScreeningStatusCollection(Factory, new ZQuery());
			AssertEquals(StmEntityScreeningLogSchema.PJ_SystemCreateTimeUtc.Name, ((IBindingList)collection).SortProperty.Name);
			AssertEquals(ListSortDirection.Descending, ((IBindingList)collection).SortDirection);
		}
	}
}
