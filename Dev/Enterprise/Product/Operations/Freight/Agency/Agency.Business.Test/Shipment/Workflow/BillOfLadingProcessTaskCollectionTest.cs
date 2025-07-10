using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(BillOfLadingProcessTaskCollection))]
	internal class BillOfLadingProcessTaskCollectionTest : ProcessTaskCollectionTest<BillOfLadingProcessTaskCollection>
	{
		public void TestParent()
		{
			AssertEquals(typeof(BillOfLading), Collection.Parent.GetType());
		}

		#region Implementation
		protected override BillOfLadingProcessTaskCollection GetCollectionToTestCore()
		{
			return new BillOfLadingProcessTaskCollection(BOL);
		}

		BillOfLading BOL
		{
			get
			{
				return bol ?? (bol = Factory.NewWithValidTestData<BillOfLading>());
			}
		}

		BillOfLading bol;
		#endregion
	}
}
