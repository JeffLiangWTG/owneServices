using System.Collections.Generic;
using Enterprise.Customs.US.Business.MessageBuilders;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class RECR21Test : TestCase
	{
		public void TestPopulate()
		{
			var mocks = new MockRepository(MockBehavior.Default);
			var entryFee1 = mocks.Create<IReconciliationImportEntryFee>();
			var entryFee2 = mocks.Create<IReconciliationImportEntryFee>();
			var entryFee3 = mocks.Create<IReconciliationImportEntryFee>();

			entryFee1.Setup(m => m.FeeClass).Returns("001");
			entryFee1.Setup(m => m.OriginalFee).Returns(1.1m);
			entryFee1.Setup(m => m.EstimatedReconciliationFee).Returns(1.2m);

			entryFee2.Setup(m => m.FeeClass).Returns("002");
			entryFee2.Setup(m => m.OriginalFee).Returns(2.1m);
			entryFee2.Setup(m => m.EstimatedReconciliationFee).Returns(2.2m);

			entryFee3.Setup(m => m.FeeClass).Returns("003");
			entryFee3.Setup(m => m.OriginalFee).Returns(3.1m);
			entryFee3.Setup(m => m.EstimatedReconciliationFee).Returns(3.2m);

			var list = new List<IReconciliationImportEntryFee>();
			list.Add(entryFee1.Object);
			list.Add(entryFee2.Object);
			list.Add(entryFee3.Object);

			var r21 = RECR21Populator.Populate(1, list);
			AssertEquals(1, r21.TrailerNumber);
			AssertEquals("001", r21.FirstFeeClass);
			AssertEquals(1.1m, r21.FirstOriginalFee);
			AssertEquals(1.2m, r21.FirstEstimateReconciliationFee);

			AssertEquals("002", r21.SecondFeeClass);
			AssertEquals(2.1m, r21.SecondOriginalFee);
			AssertEquals(2.2m, r21.SecondEstimateReconciliationFee);

			AssertEquals("003", r21.ThirdFeeClass);
			AssertEquals(3.1m, r21.ThirdOriginalFee);
			AssertEquals(3.2m, r21.ThirdEstimatedReconciliationFee);
		}
	}
}
