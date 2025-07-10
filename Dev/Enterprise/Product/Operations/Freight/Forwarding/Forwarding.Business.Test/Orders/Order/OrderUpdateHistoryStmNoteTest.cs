using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(Order.OrderUpdateHistoryStmNote))]
	sealed class OrderUpdateHistoryStmNoteTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var note = Factory.NewWithValidTestData<Order.OrderUpdateHistoryStmNote>();
			note.ST_Description = ZString.Empty;
			return note;
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("OrderUpdateHistoryStmNote is not supposed to be saved by factory by default", true);
		}
	}
}
