using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Moq;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class RECR89GeneratorTest : TestCaseWithFactory
	{
		public void TestGenerateWithNoFees()
		{
			var r89s = new RECR89Generator().Generate();
			var enumerator = r89s.GetEnumerator();
			Assert(enumerator.MoveNext());
			var r89 = enumerator.Current;
			AssertEquals("", r89.FeeClass);
			AssertEquals(1, r89.FeeSummaryTrailerNumber);
		}

		public void TestGenerateAggregateFees()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_IsAggregate = true;
			reconDeclaration.US_R_IsNoChangeAgg = true;
			var fee = reconDeclaration.AggregateRefundedFees.AddNew();
			fee.CY_Code = Core.Constants.USCustoms.FeeCodes.MerchandiseInformal;

			var r89Generator = new RECR89Generator();
			var iReconciliation = new ReconDeclarationIReconciliation(reconDeclaration);
			r89Generator.AddRefundedFees(iReconciliation.AggregateRefundedFees);
			var r89s = new List<RECR89>();
			r89s.AddRange(r89Generator.Generate());
			AssertEquals(1, r89s.Count);
			AssertEquals("first 89", "R89013110000000000000000000000                                                  ", r89s[0].Serialise());
		}

		public void TestGenerateWithMultipleFees()
		{
			var mocks = new MockRepository(MockBehavior.Default);

			var mock1 = new Mock<IReconciliationImportEntryFee>();
			var mock2 = new Mock<IReconciliationImportEntryFee>();
			var mock3 = new Mock<IReconciliationImportEntryFee>();
			var mock4 = new Mock<IReconciliationImportEntryFee>();

			mock1.Setup(m => m.FeeClass).Returns("053");
			mock1.Setup(m => m.OriginalFee).Returns(1m);
			mock1.Setup(m => m.EstimatedReconciliationFee).Returns(2m);

			mock2.Setup(m => m.FeeClass).Returns("054");
			mock2.Setup(m => m.OriginalFee).Returns(3m);
			mock2.Setup(m => m.EstimatedReconciliationFee).Returns(4m);

			mock3.Setup(m => m.FeeClass).Returns("055");
			mock3.Setup(m => m.OriginalFee).Returns(5m);
			mock3.Setup(m => m.EstimatedReconciliationFee).Returns(6m);

			mock4.Setup(m => m.FeeClass).Returns("056");
			mock4.Setup(m => m.OriginalFee).Returns(7m);
			mock4.Setup(m => m.EstimatedReconciliationFee).Returns(8m);

			var fees = new List<IReconciliationImportEntryFee>();
			fees.Add(mock1.Object);
			fees.Add(mock2.Object);
			fees.Add(mock3.Object);
			fees.Add(mock4.Object);

			var r89s = new List<RECR89>();
			var r89Generator = new RECR89Generator();
			r89Generator.AddFees(fees, false);
			r89s.AddRange(r89Generator.Generate());
			AssertEquals(2, r89s.Count);

			AssertEquals(1, r89s[0].FeeSummaryTrailerNumber);
			AssertEquals("053", r89s[0].FeeClass);
			AssertEquals(1m, r89s[0].TotalOriginalFee);
			AssertEquals(2m, r89s[0].TotalEstimateReconciliationFee);

			AssertEquals("054", r89s[0].FeeClass1);
			AssertEquals(3m, r89s[0].TotalOriginalFee1);
			AssertEquals(4m, r89s[0].TotalEstimateReconciliationFee1);

			AssertEquals("055", r89s[0].FeeClass2);
			AssertEquals(5m, r89s[0].TotalOriginalFee2);
			AssertEquals(6m, r89s[0].TotalReconciliationFee2);

			AssertEquals(2, r89s[1].FeeSummaryTrailerNumber);
			AssertEquals("056", r89s[1].FeeClass);
			AssertEquals("", r89s[1].FeeClass1);
			AssertEquals("", r89s[1].FeeClass2);

			var first89 = r89s[0].Serialise();
			AssertEquals("first 89", "R8901053000000001000000000020005400000000300000000004000550000000050000000000600", first89);

			var second89 = r89s[1].Serialise();
			AssertEquals("second 89", "R89020560000000070000000000800                                                  ", second89);
		}

		public void TestGenerateWithMultipleFeesWaiveRefund()
		{
			var mocks = new MockRepository(MockBehavior.Default);

			var mock1 = mocks.Create<IReconciliationImportEntryFee>();
			var mock2 = mocks.Create<IReconciliationImportEntryFee>();

			mock1.Setup(m => m.FeeClass).Returns("053");
			mock1.Setup(m => m.OriginalFee).Returns(1m);
			mock1.Setup(m => m.EstimatedReconciliationFee).Returns(2m);

			mock2.Setup(m => m.FeeClass).Returns("054");
			mock2.Setup(m => m.OriginalFee).Returns(7m);
			mock2.Setup(m => m.EstimatedReconciliationFee).Returns(4m);

			var fees = new List<IReconciliationImportEntryFee>();
			fees.Add(mock1.Object);
			fees.Add(mock2.Object);

			var r89s = new List<RECR89>();
			var r89Generator = new RECR89Generator();
			r89Generator.AddFees(fees, true);
			r89s.AddRange(r89Generator.Generate());
			AssertEquals(1, r89s.Count);

			AssertEquals(1, r89s[0].FeeSummaryTrailerNumber);
			AssertEquals("053", r89s[0].FeeClass);
			AssertEquals(1m, r89s[0].TotalOriginalFee);
			AssertEquals(2m, r89s[0].TotalEstimateReconciliationFee);

			AssertEquals("054", r89s[0].FeeClass1);
			AssertEquals(7m, r89s[0].TotalOriginalFee1);
			AssertEquals(7m, r89s[0].TotalEstimateReconciliationFee1);

			var first89 = r89s[0].Serialise();
			AssertEquals("first 89", "R890105300000000100000000002000540000000070000000000700                         ", first89);
		}
	}
}
