using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class RECR10Test : TestCase
	{
		public void TestPopulate()
		{
			var mock = GetIReconciliationMockObject();

			var r10 = RECR10Populator.Populate("A", mock.Object);

			AssertEquals("A", r10.ActionCode);
			AssertEquals(MQEDIMessage.USEntryFilerEntryNumberPlaceHolder, r10.ReconciliationEntryNumber);
			AssertEquals("2304", r10.ReconciliationPort);
			AssertEquals("IID", r10.ImporterID);
			AssertEquals("SCD", r10.SuretyCode);
			AssertEquals(ZDate.BrettsBirthday, r10.EstimatedReconciliationEntrySummaryDate);
			AssertEquals("R1R", r10.ReconciliationTeam);
			AssertEquals("IC", r10.IssueCode);
			AssertEquals("Y", r10.AggregateReconciliationIndicator);
			AssertEquals("2", r10.IncreaseRefundIndicator);
			AssertEquals(new ZDate(2000, 1, 1), r10.EarliestImportDate);
			AssertEquals(new ZDate(2000, 1, 2), r10.EarliestEntrySummaryDate);
			AssertEquals("ABR", r10.AgentBroker4811ReferenceID);
			AssertEquals("000000009", r10.BrokerReferenceNumber);
		}

		public void TestNAIssueCodeDoesSendsBlankReconIssue()
		{
			var mock = GetIReconciliationNAIssueCodeMockObject();

			var r10 = RECR10Populator.Populate("A", mock.Object);

			AssertEquals("A", r10.ActionCode);
			AssertEquals(MQEDIMessage.USEntryFilerEntryNumberPlaceHolder, r10.ReconciliationEntryNumber);
			AssertEquals("2304", r10.ReconciliationPort);
			AssertEquals("IID", r10.ImporterID);
			AssertEquals("SCD", r10.SuretyCode);
			AssertEquals(ZDate.BrettsBirthday, r10.EstimatedReconciliationEntrySummaryDate);
			AssertEquals("R1R", r10.ReconciliationTeam);
			AssertEquals("N/A selected on the declaration should result in no recon issue being sent to customs", ZString.Empty, r10.IssueCode);
			AssertEquals("Y", r10.AggregateReconciliationIndicator);
			AssertEquals("2", r10.IncreaseRefundIndicator);
			AssertEquals(new ZDate(2000, 1, 1), r10.EarliestImportDate);
			AssertEquals(new ZDate(2000, 1, 2), r10.EarliestEntrySummaryDate);
			AssertEquals("ABR", r10.AgentBroker4811ReferenceID);
			AssertEquals("000000009", r10.BrokerReferenceNumber);
		}

		public void TestPopulateDeletionTransaction()
		{
			var mock = GetIReconciliationMockObject();

			var r10 = RECR10Populator.Populate("D", mock.Object);

			AssertEquals("D", r10.ActionCode);
			AssertEquals(MQEDIMessage.USEntryFilerEntryNumberPlaceHolder, r10.ReconciliationEntryNumber);
			AssertEquals("2304", r10.ReconciliationPort);
			AssertEquals(ZString.Empty, r10.ImporterID);
			AssertEquals(ZString.Empty, r10.SuretyCode);
			AssertEquals(ZDate.Empty, r10.EstimatedReconciliationEntrySummaryDate);
			AssertEquals(ZString.Empty, r10.ReconciliationTeam);
			AssertEquals(ZString.Empty, r10.IssueCode);
			AssertEquals(ZString.Empty, r10.AggregateReconciliationIndicator);
			AssertEquals(ZString.Empty, r10.IncreaseRefundIndicator);
			AssertEquals(ZDate.Empty, r10.EarliestImportDate);
			AssertEquals(ZDate.Empty, r10.EarliestEntrySummaryDate);
			AssertEquals(ZString.Empty, r10.AgentBroker4811ReferenceID);
			AssertEquals(ZString.Empty, r10.BrokerReferenceNumber);
		}

		Mock<IReconciliation> GetIReconciliationMockObject()
		{
			var mock = new Mock<IReconciliation>();
			mock.Setup(m => m.EntryNumber).Returns("EN");
			mock.Setup(m => m.ProcessingDistrictPort).Returns("2304");
			mock.Setup(m => m.SuretyCode).Returns("SCD");
			mock.Setup(m => m.ImporterID).Returns("IID");
			mock.Setup(m => m.TeamNumber).Returns("R1R");
			mock.Setup(m => m.EstimatedReconciliationEntrySummaryDate).Returns(ZDate.BrettsBirthday);
			mock.Setup(m => m.IssueCode).Returns("IC");
			mock.Setup(m => m.AggregateReconciliationIndicator).Returns(true);
			mock.Setup(m => m.IncreaseRefundIndicator).Returns("2");
			mock.Setup(m => m.EarliestImportDate).Returns(new ZDate(2000, 1, 1));
			mock.Setup(m => m.EarliestEntrySummaryDate).Returns(new ZDate(2000, 1, 2));
			mock.Setup(m => m.AgentBrokerReferenceID).Returns("ABR");
			mock.Setup(m => m.BrokerReferenceNumber).Returns("BRN000000009");
			return mock;
		}

		Mock<IReconciliation> GetIReconciliationNAIssueCodeMockObject()
		{
			var mock = new Mock<IReconciliation>();
			mock.Setup(m => m.EntryNumber).Returns("EN");
			mock.Setup(m => m.ProcessingDistrictPort).Returns("2304");
			mock.Setup(m => m.SuretyCode).Returns("SCD");
			mock.Setup(m => m.ImporterID).Returns("IID");
			mock.Setup(m => m.TeamNumber).Returns("R1R");
			mock.Setup(m => m.EstimatedReconciliationEntrySummaryDate).Returns(ZDate.BrettsBirthday);
			mock.Setup(m => m.IssueCode).Returns(ReconIssueCodeList.Codes.NotApplicable);
			mock.Setup(m => m.AggregateReconciliationIndicator).Returns(true);
			mock.Setup(m => m.IncreaseRefundIndicator).Returns("2");
			mock.Setup(m => m.EarliestImportDate).Returns(new ZDate(2000, 1, 1));
			mock.Setup(m => m.EarliestEntrySummaryDate).Returns(new ZDate(2000, 1, 2));
			mock.Setup(m => m.AgentBrokerReferenceID).Returns("ABR");
			mock.Setup(m => m.BrokerReferenceNumber).Returns("BRN000000009");
			return mock;
		}
	}
}
