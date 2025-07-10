using System;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsPickProcessTaskCollection))]
	public class WhsPickProcessTaskCollectionTest : ProcessTaskCollectionTest<WhsPickProcessTaskCollection>
	{
		protected override WhsPickProcessTaskCollection GetCollectionToTestCore()
		{
			return new WhsPickProcessTaskCollection(Helper.CreatePickNew());
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(WhsPickProcessTaskCollection);
		}

		#region Helper

		protected WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion
	}
}
