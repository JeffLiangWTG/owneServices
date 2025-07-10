using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class FZConcurrenceMessageSendingObjectLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUQ_List()
		{
			AssertSame(RefPackTypeCollection.GetAsCodeDescriptionPairWithStandardUnits(Factory),
				new FZConcurrenceMessageSendingObject(new FTZConcurrenceForTesting(Factory)).Lookups.UQ_List);
		}
	}
}
