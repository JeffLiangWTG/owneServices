using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccCashAdvanceRequestLineCollection))]
	sealed class AccCashAdvanceRequestLineCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNew()
		{
			var lineCollection = GetCollectionToTest();
			Assert(!lineCollection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var lineCollection = GetCollectionToTest();
			Assert(!lineCollection.AllowRemove);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccCashAdvanceRequestLineCollection(Factory);
		}

		#endregion
	}
}
