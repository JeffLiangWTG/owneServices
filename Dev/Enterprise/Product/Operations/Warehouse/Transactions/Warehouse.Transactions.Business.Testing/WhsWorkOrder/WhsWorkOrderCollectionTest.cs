using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsWorkOrderCollection))]
	class WhsWorkOrderCollectionTest : WhsComponentOrderCollectionTest<WhsWorkOrderCollection>
	{
		#region Implementation

		protected override WhsWorkOrderCollection GetCollectionToTest()
		{
			return new WhsWorkOrderCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<WhsWorkOrder>();
		}

		protected override bool AllowNew
		{
			get { return true; }
		}

		protected override string[] DocketTypesForTest
		{
			get { return new[] { DocketType.Codes.WorkOrder }; }
		}

		#endregion
	}
}
