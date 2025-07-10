using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccCashAdvanceRequestHeaderCollection))]
	sealed class AccCashAdvanceRequestHeaderCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNew()
		{
			var headerCollection = GetCollectionToTest();
			Assert(!headerCollection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var headerCollection = GetCollectionToTest();
			Assert(!headerCollection.AllowRemove);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccCashAdvanceRequestHeaderCollection(Factory);
		}

		#endregion
	}
}
