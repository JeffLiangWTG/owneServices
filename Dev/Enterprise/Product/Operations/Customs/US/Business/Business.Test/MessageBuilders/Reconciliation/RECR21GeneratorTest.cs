using System.Collections.Generic;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class RECR21GeneratorTest : TestCase
	{
		public void TestGenerate()
		{
			var mocks = new MockRepository(MockBehavior.Default);
			var mock1 = mocks.Create<IReconciliationImportEntryFee>();
			var mock2 = mocks.Create<IReconciliationImportEntryFee>();
			var mock3 = mocks.Create<IReconciliationImportEntryFee>();
			var mock4 = mocks.Create<IReconciliationImportEntryFee>();

			mock1.Setup(m => m.FeeClass).Returns("1");
			mock1.Setup(m => m.OriginalFee).Returns(1.1);
			mock1.Setup(m => m.EstimatedReconciliationFee).Returns(1.2);
			mock2.Setup(m => m.FeeClass).Returns("2");
			mock2.Setup(m => m.OriginalFee).Returns(2.1);
			mock2.Setup(m => m.EstimatedReconciliationFee).Returns(2.2);
			mock3.Setup(m => m.FeeClass).Returns("3");
			mock3.Setup(m => m.OriginalFee).Returns(3.1);
			mock3.Setup(m => m.EstimatedReconciliationFee).Returns(3.2);
			mock4.Setup(m => m.FeeClass).Returns("4");
			mock4.Setup(m => m.OriginalFee).Returns(4.1);
			mock4.Setup(m => m.EstimatedReconciliationFee).Returns(4.2);

			var fees = new List<IReconciliationImportEntryFee>();
			fees.Add(mock1.Object);
			fees.Add(mock2.Object);
			fees.Add(mock3.Object);
			fees.Add(mock4.Object);
			var r21s = new List<RECR21>();
			r21s.AddRange(new RECR21Generator().Generate(fees));
			AssertEquals(2, r21s.Count);
			AssertEquals(4.2m, r21s[1].FirstEstimateReconciliationFee);
		}
	}
}
