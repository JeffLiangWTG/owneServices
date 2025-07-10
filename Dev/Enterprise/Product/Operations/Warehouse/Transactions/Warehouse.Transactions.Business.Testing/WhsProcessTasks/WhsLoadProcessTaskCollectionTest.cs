using System;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsLoadProcessTaskCollection))]
	public class WhsLoadProcessTaskCollectionTest : ProcessTaskCollectionTest<WhsLoadProcessTaskCollection>
	{
		#region Overrides

		protected override WhsLoadProcessTaskCollection GetCollectionToTestCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL00000001", "CDS", startTime: DateTimeOffset.Now);
			return new WhsLoadProcessTaskCollection(load);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(WhsLoadProcessTaskCollection);
		}

		#endregion

		protected WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));

		WhsTestHelperFunctions helper;
	}
}
