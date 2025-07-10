using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Business.MessageBuilders.Testing;
using Moq;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class RECR16Test : ReconciliationMessageBuilderAbstractTest<IReconciliation>
	{
		public void TestPopulateWithShortComment()
		{
			var mock = new Mock<IReconciliation>();
			mock.Setup(m => m.TextComment).Returns(new ZString('A', 75));
			AssertNull(RECR16Populator.Populate(mock.Object));
		}

		public void TestPopulateWithLongComment()
		{
			var mock = new Mock<IReconciliation>();
			var x = new ZString('A', 75) + new ZString('B', 76);
			mock.Setup(m => m.TextComment).Returns(x);
			AssertEquals(new ZString('B', 75), RECR16Populator.Populate(mock.Object).TextComment);
		}
	}
}
