using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgTradeProspectByCompetitorCollection))]
	sealed class OrgTradeProspectByCompetitorCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgTradeProspectByCompetitorCollection(Factory.NewWithValidTestData<OrgHeader>());
		}

		#endregion
	}
}
