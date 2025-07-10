using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[SetGlobalsIsWeb]
	[TestedType(typeof(TrackingWhsReceiveLine))]
	sealed class TrackingWhsReceiveLineForNonPersistentBoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestClonable()
		{
			var trackingLine = TrackingHelper.Get(Factory.New<WhsReceiveLine>());
			Assert(trackingLine.SupportsClone());
			trackingLine.WhsReceiveLine.WE_OP = ZGuid.Invalid;
			trackingLine.WhsReceiveLine.ProductCode = "TempProduct";
			trackingLine.WhsReceiveLine.ProductDesc = "TempDesc";
			trackingLine.WhsReceiveLine.CommodityCode = "TEMP";
			trackingLine.WhsReceiveLine.ProductUQ = "XXX";

			CombineAssertions(() =>
			{
				AssertEquals("Precondition", ZGuid.Invalid, trackingLine.WhsReceiveLine.WE_OP);
				AssertEquals("Precondition", "TempProduct", trackingLine.WhsReceiveLine.ProductCode);
				AssertEquals("Precondition", "TempDesc", trackingLine.WhsReceiveLine.ProductDesc);
				AssertEquals("Precondition", "TEMP", trackingLine.WhsReceiveLine.CommodityCode);
				AssertEquals("Precondition", "XXX", trackingLine.WhsReceiveLine.ProductUQ);
			});

			var clone = (TrackingWhsReceiveLine)trackingLine.Clone();

			CombineAssertions(() =>
			{
				AssertEquals("WhsReceiveLine properties should be cloned.", ZGuid.Invalid, clone.WhsReceiveLine.WE_OP);
				AssertEquals("WhsReceiveLine properties should be cloned.", "TempProduct", clone.WhsReceiveLine.ProductCode);
				AssertEquals("WhsReceiveLine properties should be cloned.", "TempDesc", clone.WhsReceiveLine.ProductDesc);
				AssertEquals("WhsReceiveLine properties should be cloned.", "TEMP", clone.WhsReceiveLine.CommodityCode);
				AssertEquals("WhsReceiveLine properties should be cloned.", "XXX", clone.WhsReceiveLine.ProductUQ);
			});
		}

		public void TestGetWrappedBizO()
		{
			var trackingLine = TrackingHelper.Get(Factory.New<WhsReceiveLine>());
			var wrappedBizO = trackingLine.GetWrappedBizO();

			AssertEquals(trackingLine.WhsReceiveLine, wrappedBizO);
		}

		public void TestGetWrappedBindTo()
		{
			var trackingLine = TrackingHelper.Get(Factory.New<WhsReceiveLine>());
			var wrappedBindTo = trackingLine.GetWrappedBindTo("Lookups");

			AssertEquals("WhsReceiveLine+Lookups", wrappedBindTo);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return TrackingHelper.Get(Factory.New<WhsReceiveLine>());
		}
	}
}
