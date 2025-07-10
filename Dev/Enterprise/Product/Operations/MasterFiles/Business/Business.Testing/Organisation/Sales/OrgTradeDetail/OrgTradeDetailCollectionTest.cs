using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgTradeDetailCollection))]
	sealed class OrgTradeDetailCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgSales sales = Factory.NewWithValidTestData<OrgHeader>().SalesCollection.AddNew();
			return new OrgTradeDetailCollection(sales);
		}

		#endregion
	}
}
