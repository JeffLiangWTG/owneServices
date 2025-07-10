using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	[TestedType(typeof(FZConcurrenceMessageSendingObject))]
	sealed class FZConcurrenceMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUpdateFTZConcurrenceQtyIfNeeded_DoNotUpdateIfSame()
		{
			var ftzConcurrence = new FTZConcurrenceForTesting(Factory);
			ftzConcurrence.FTZConcurrenceQty = 24m;
			var objA = new FZConcurrenceMessageSendingObject(ftzConcurrence);
			objA.MB_ConcurrenceQty = 24m;
			objA.UpdateFTZConcurrenceQtyIfNeeded();
			AssertEquals(24m, ftzConcurrence.FTZConcurrenceQty);
			AssertEquals("", ftzConcurrence.GetChangeLog());
			ftzConcurrence.ClearChangeLog();

			objA.MB_ConcurrenceQty = 13m;
			objA.UpdateFTZConcurrenceQtyIfNeeded();
			AssertEquals(13m, ftzConcurrence.FTZConcurrenceQty);
			AssertEquals("24 => 13\r\n", ftzConcurrence.GetChangeLog());
			ftzConcurrence.ClearChangeLog();

			objA.MB_ConcurrenceQty = 13m;
			objA.UpdateFTZConcurrenceQtyIfNeeded();
			AssertEquals(13m, ftzConcurrence.FTZConcurrenceQty);
			AssertEquals("", ftzConcurrence.GetChangeLog());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var ftzConcurrence = new FTZConcurrenceForTesting(Factory);
			return new FZConcurrenceMessageSendingObject(ftzConcurrence);
		}
	}
}
