using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.PickByLabel.Testing
{
	[TestedType(typeof(WhsPickByLabelLabelCollection))]
	public class WhsPickByLabelLabelCollectionTestCase : WhsBusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var master = Factory.New<WhsPickByLabelJob>();
			return new WhsPickByLabelLabelCollection(master, Factory);
		}
	}
}
