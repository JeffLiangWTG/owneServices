using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Business.MessageBuilders.Testing;
using Moq;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class RECR20Test : ReconciliationMessageBuilderAbstractTest<IReconciliationImportEntry>
	{
		public void TestPopulate()
		{
			var mock = new Mock<IReconciliationImportEntry>();
			mock.Setup(m => m.ImportEntryFilerCodeNumber).Returns("IE");
			mock.Setup(m => m.Port).Returns("3901");
			mock.Setup(m => m.OriginalDuty).Returns(1m);
			mock.Setup(m => m.EstimatedReconciliationDuty).Returns(2m);
			mock.Setup(m => m.OriginalTax).Returns(3m);
			mock.Setup(m => m.EstimatedReconciliationTax).Returns(4m);
			mock.Setup(m => m.EstimatedReconciliationInterest).Returns(5m);

			var r20 = RECR20Populator.Populate(mock.Object, 1, false);

			AssertEquals("IE", r20.ImportEntry);
			AssertEquals("3901", r20.EntryPort);
			AssertEquals(1m, r20.OriginalDuty);
			AssertEquals(2m, r20.EstimatedReconciliationDuty);
			AssertEquals(3m, r20.OriginalTax);
			AssertEquals(4m, r20.EstimatedReconciliationTax);
			AssertEquals(5m, r20.EstimatedReconciliationInterest);
			AssertEquals(0, r20.FeeTrailerCounter);//should be serialised into '00'

			string serialised = r20.Serialise();
			AssertEquals("R200001IE         3901000000001000000000020000000000300000000004000000000050000 ", serialised);
		}
	}
}
