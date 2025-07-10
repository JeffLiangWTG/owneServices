using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	[TestedType(typeof(FZConcurrenceMessageSendingObjectCollection))]
	sealed class FZConcurrenceMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<FZConcurrenceMessageSendingObjectCollection>
	{
		protected override FZConcurrenceMessageSendingObjectCollection GetCollectionToTest() => new FZConcurrenceMessageSendingObjectCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new FZConcurrenceMessageSendingObject(new FTZConcurrenceForTesting(Factory));
	}
}
