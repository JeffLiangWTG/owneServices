using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsDocketReferenceCollection))]
	internal class WhsDocketReferenceCollectionTestCase : WhsBusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			WhsDocket master = Factory.New<WhsReceive>();
			return new WhsDocketReferenceCollection(master, Factory);
		}
	}
}
