using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsPickableDocketLineCollectionAdHoc))]
	public class WhsPickableDocketLineCollectionAdHocTest : WhsDocketLineCollectionAdHocTest<WhsPickableDocketLineCollectionAdHoc, WhsPickableDocketLine>
	{
		#region Implementation

		protected override WhsPickableDocketLineCollectionAdHoc GetCollectionToTest()
		{
			return new WhsPickableDocketLineCollectionAdHoc(Factory);
		}

		protected override WhsDocket GetNewDocket()
		{
			return Factory.New<WhsOrder>();
		}

		#endregion
	}
}
