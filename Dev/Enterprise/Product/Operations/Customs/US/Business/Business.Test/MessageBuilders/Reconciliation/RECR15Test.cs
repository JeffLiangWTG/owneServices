using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Business.MessageBuilders.Testing;
using Moq;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class RECR15Test : ReconciliationMessageBuilderAbstractTest<IReconciliation>
	{
		public void TestImportEntrySource()
		{
			var mock = new Mock<IReconciliation>();
			mock.SetupSequence(m => m.ImportEntrySource)
				.Returns(1)
				.Returns(2)
				.Returns(3);

			AssertEquals(1, RECR15Populator.Populate(mock.Object).ImportEntrySource);
			AssertEquals(2, RECR15Populator.Populate(mock.Object).ImportEntrySource);
			AssertEquals(3, RECR15Populator.Populate(mock.Object).ImportEntrySource);
		}

		public void TestPopulateWithShortComment()
		{
			var mock = new Mock<IReconciliation>();
			mock.Setup(m => m.TextComment).Returns(new ZString('A', 75));
			AssertEquals(new ZString('A', 75), RECR15Populator.Populate(mock.Object).TextComment);
		}

		public void TestPopulateWithLongComment()
		{
			var mock = new Mock<IReconciliation>();
			var x = new ZString('A', 75) + new ZString('B', 75);
			mock.Setup(m => m.TextComment).Returns(x);
			AssertEquals(new ZString('A', 75), RECR15Populator.Populate(mock.Object).TextComment);
		}
	}
}
