using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class ReconciliationMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGenerateForBBlock()
		{
			Mock<IReconciliation> mock = GetTheMock();
			IReconciliation reconciliation = mock.Object;
			MQEDIMessage message = new ReconciliationMessageBuilder("A", reconciliation).Generate();

			APLB b = new APLB();
			b.Deserialise(message.EM_MessageText.Left(80));

			AssertEquals("ProcessingDistrictPortCode", "2304", b.ProcessingDistrictPortCode);
			AssertEquals("PreparerDistrictPort", "1234", b.PreparerDistrictPort);
			AssertEquals("PreparerIndicator", "2", b.PreparerIndicator);
			AssertEquals("PreparerFilerCode", "XJ5", b.PreparerFilerCode);
			AssertEquals("PreparerOfficeCode", "1", b.PreparerOfficeCode);

			mock.Setup(m => m.PreparerDistrictPort).Returns(reconciliation.ProcessingDistrictPort);
			message = new ReconciliationMessageBuilder("A", reconciliation).Generate();
			b = new APLB();
			b.Deserialise(message.EM_MessageText.Left(80));
			AssertEquals("ProcessingDistrictPortCode", "2304", b.ProcessingDistrictPortCode);
			AssertEquals("PreparerDistrictPort is not filled in if it is same as ProcessingDistrictPortCode", "", b.PreparerDistrictPort);
			AssertEquals("PreparerIndicator", "", b.PreparerIndicator);
			AssertEquals("PreparerFilerCode", "", b.PreparerFilerCode);
			AssertEquals("PreparerOfficeCode", "", b.PreparerOfficeCode);
		}

		public void TestGenerate21RecordsWithRefundedFessForNonAggregate()
		{
			var mock = new Mock<IReconciliation>();

			// R10
			mock.Setup(m => m.EntryFilerCode).Returns("XJ5");
			mock.Setup(m => m.Factory).Returns(Factory);
			mock.Setup(m => m.EntryNumber).Returns("EN");
			mock.Setup(m => m.ProcessingDistrictPort).Returns("2304");
			mock.Setup(m => m.PreparerDistrictPort).Returns("1234");
			mock.Setup(m => m.OfficeCode).Returns("1");
			mock.Setup(m => m.ImporterID).Returns("IID");
			mock.Setup(m => m.SuretyCode).Returns("SCD");
			mock.Setup(m => m.EstimatedReconciliationEntrySummaryDate).Returns(ZDate.BrettsBirthday);
			mock.Setup(m => m.IssueCode).Returns("IC");
			mock.Setup(m => m.AggregateReconciliationIndicator).Returns(false);
			mock.Setup(m => m.IncreaseRefundIndicator).Returns("2");
			mock.Setup(m => m.IsWaiveRefund).Returns(false);
			mock.Setup(m => m.EarliestImportDate).Returns(new ZDate(2000, 1, 1));
			mock.Setup(m => m.EarliestEntrySummaryDate).Returns(new ZDate(2000, 1, 2));
			mock.Setup(m => m.AgentBrokerReferenceID).Returns("ABR");
			mock.Setup(m => m.BrokerReferenceNumber).Returns("BRN");

			// R15 + R16
			mock.Setup(m => m.ImportEntrySource).Returns(2);
			mock.Setup(m => m.TextComment).Returns(new ZString('A', 75) + new ZString('B', 76));

			mock.Setup(m => m.PaymentTypeIndicator).Returns(PaymentTypeList.Codes.IndividualBasis);
			mock.Setup(m => m.PreliminaryStatementPrintDate).Returns(ZDate.BrettsBirthday);
			mock.Setup(m => m.ClientBranchDesignation).Returns("");

			mock.Setup(m => m.DutyPaymentAmount).Returns(10m);
			mock.Setup(m => m.TaxPaymentAmount).Returns(20m);
			mock.Setup(m => m.FeePaymentAmount).Returns(30m);
			mock.Setup(m => m.InterestPaymentAmount).Returns(40m);
			mock.Setup(m => m.TeamNumber).Returns(new ZString("R1R"));
			mock.Setup(m => m.AggregateRefundedFees).Returns(System.Array.Empty<ZString>());

			var importEntryMock = new Mock<IReconciliationImportEntry>();
			importEntryMock.Setup(m => m.ImportEntryFilerCodeNumber).Returns("IEFCN");
			importEntryMock.Setup(m => m.Port).Returns("3901");
			importEntryMock.Setup(m => m.OriginalDuty).Returns(100m);
			importEntryMock.Setup(m => m.EstimatedReconciliationDuty).Returns(120m);
			importEntryMock.Setup(m => m.OriginalTax).Returns(200m);
			importEntryMock.Setup(m => m.EstimatedReconciliationTax).Returns(220m);
			importEntryMock.Setup(m => m.EstimatedReconciliationInterest).Returns(30m);

			var refundFeeMock = new Mock<IReconciliationImportEntryFee>();
			refundFeeMock.Setup(m => m.FeeClass).Returns(new ZString("499"));
			refundFeeMock.Setup(m => m.OriginalFee).Returns(ZDecimal.Zero);
			refundFeeMock.Setup(m => m.EstimatedReconciliationFee).Returns(ZDecimal.Zero);

			importEntryMock.Setup(m => m.Fees).Returns(new IReconciliationImportEntryFee[] { refundFeeMock.Object });

			mock.Setup(m => m.ImportEntries).Returns(new IReconciliationImportEntry[] { importEntryMock.Object });
			mock.Setup(m => m.IsNoChangeAggregate).Returns(false);

			var message = new ReconciliationMessageBuilder("A", mock.Object).Generate();
			var r21s = message.MessageBlock.MessageBlocks.FindAll(x => x is RECR21);
			AssertEquals(1, r21s.Count);

			var r21 = (RECR21)r21s[0];
			AssertEquals("499", r21.FirstFeeClass);
			AssertEquals(0m, r21.FirstOriginalFee);
			AssertEquals(0m, r21.FirstEstimateReconciliationFee);
		}

		public void TestGenerateForAggregatedRecon()
		{
			Mock<IReconciliation> mock = GetTheMock();

			IReconciliation reconciliation = mock.Object;
			MQEDIMessage message = new ReconciliationMessageBuilder("A", reconciliation).Generate();
			ZString text = message.EM_FormattedMessageText;

			BlockControlGenerator messageBlock = new ABIInputBlockControlGenerator(reconciliation.EntryFilerCode, reconciliation.ProcessingDistrictPort, ZString.Empty);
			messageBlock.Deserialise(BlockPadder.Pad(message.EM_MessageText));

			var r10 = messageBlock.MessageBlocks.OfType<RECR10>().FirstOrDefault();
			AssertEquals("PreCondition:Aggregate", "Y", r10.AggregateReconciliationIndicator);
			List<IReconciliationImportEntry> importEntries = new List<IReconciliationImportEntry>(reconciliation.ImportEntries);

			AssertEquals("PreCondition:OriginalDuty > 0", true, importEntries[0].OriginalDuty > 0);
			AssertEquals("PreCondition:EstimatedReconciliationDuty > 0", true, importEntries[0].EstimatedReconciliationDuty > 0);
			AssertEquals("PreCondition:OriginalTax > 0", true, importEntries[0].OriginalTax > 0);
			AssertEquals("PreCondition:EstimatedReconciliationTax > 0", true, importEntries[0].EstimatedReconciliationTax > 0);
			AssertEquals("PreCondition:EstimatedReconciliationInterest > 0", true, importEntries[0].EstimatedReconciliationInterest > 0);

			List<MessageBlock> r20s = messageBlock.MessageBlocks.FindAll((MessageBlock block) => block is RECR20);
			AssertEquals("one 20 record is expected", 1, r20s.Count);

			RECR20 r20 = (RECR20)r20s[0];
			AssertEquals("OriginalDuty should be zero filled for aggregated", 0m, r20.OriginalDuty);
			AssertEquals("EstimatedReconciliationDuty should be zero filled for aggregated", 0m, r20.EstimatedReconciliationDuty);
			AssertEquals("OriginalTax should be zero filled for aggregated", 0m, r20.OriginalTax);
			AssertEquals("EstimatedReconciliationTax should be zero filled for aggregated", 0m, r20.EstimatedReconciliationTax);
			AssertEquals("EstimatedReconciliationInterest should be zero filled for aggregated", 0m, r20.EstimatedReconciliationInterest);

			var r90 = messageBlock.MessageBlocks.OfType<RECR90>().FirstOrDefault();
			AssertEquals("OriginalDuty correctly filled in", 100m, r90.TotalOriginalDuty);
			AssertEquals("EstimatedReconciliationDuty correctly filled in", 120m, r90.TotalEstimateReconciliationDuty);
			AssertEquals("OriginalTax correctly filled in", 200m, r90.TotalOriginalTax);
			AssertEquals("EstimatedReconciliationTax correctly filled in", 220m, r90.TotalEstimateReconciliationTax);

			var r91 = messageBlock.MessageBlocks.OfType<RECR91>().FirstOrDefault();
			AssertEquals("EstimatedReconciliationInterest correctly filled in from reconciliation data", 40m, r91.TotalEstimateReconciliationInterest);
		}

		public void TestGenerateForAggregatedOverriddenRecon()
		{
			Mock<IReconciliation> mock = GetTheMock();
			mock.Setup(m => m.IsNoChangeAggregate).Returns(true);

			mock.Setup(m => m.AggregateRefundedFees).Returns(new ZString[] { "499" });

			IReconciliation reconciliation = mock.Object;
			MQEDIMessage message = new ReconciliationMessageBuilder("A", reconciliation).Generate();
			ZString text = message.EM_FormattedMessageText;

			BlockControlGenerator messageBlock = new ABIInputBlockControlGenerator(reconciliation.EntryFilerCode, reconciliation.ProcessingDistrictPort, ZString.Empty);
			messageBlock.Deserialise(BlockPadder.Pad(message.EM_MessageText));

			var r90 = messageBlock.MessageBlocks.OfType<RECR90>().FirstOrDefault();
			AssertEquals("OriginalDuty should be zero", 0m, r90.TotalOriginalDuty);
			AssertEquals("EstimatedReconciliationDuty should be zero", 0m, r90.TotalEstimateReconciliationDuty);
			AssertEquals("OriginalTax should be zero", 0m, r90.TotalOriginalTax);
			AssertEquals("EstimatedReconciliationTax should be zero", 0m, r90.TotalEstimateReconciliationTax);

			var r89s = messageBlock.MessageBlocks.OfType<RECR89>();
			AssertEquals(1, r89s.Count());

			var r89 = r89s.FirstOrDefault();
			AssertEquals("Fee", "499", r89.FeeClass);
			AssertEquals("Fee original amount", 0m, r89.TotalOriginalFee);
			AssertEquals("Fee recon amount", 0m, r89.TotalEstimateReconciliationFee);

			AssertEquals("", r89.FeeClass1);
			AssertEquals("", r89.FeeClass2);
		}

		public void TestSendMessagesForNoChangeAggregateRecon()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_PreliminaryStatementPrintDate = new ZDate(1999, 9, 15);
			reconDeclaration.US_IsAggregate = true;
			reconDeclaration.US_R_IsNoChangeAgg = true;

			reconDeclaration.AggregateRefundedFees.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
			reconDeclaration.AggregateRefundedFees.AddNew(Core.Constants.USCustoms.FeeCodes.HMF);

			var entry1 = reconDeclaration.OriginalEntries.AddNew();
			entry1.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4300m);
			entry1.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4301m);
			entry1.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 43m);
			entry1.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 44m);
			entry1.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.OtherExcise, 44m);

			entry1.US_PaymentDate = new ZDate(1999, 1, 5);

			var entry2 = reconDeclaration.OriginalEntries.AddNew();
			entry2.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4000m);
			entry2.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4001m);
			entry2.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 40m);
			entry2.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 41m);
			entry2.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.OtherExcise, 44m);
			entry2.US_PaymentDate = new ZDate(1999, 4, 12);

			reconDeclaration.CalculateDutyFeesForAllEntries();

			var message = new ReconciliationMessageBuilder("A", new ReconDeclarationIReconciliation(reconDeclaration)).Generate();

			var r17 = message.MessageBlock.MessageBlocks.OfType<RECR17>().FirstOrDefault();
			AssertEquals(0m, r17.DutyPaymentAmount);
			AssertEquals(0m, r17.FeePaymentAmount);
			AssertEquals(0m, r17.TaxPaymentAmount);

			var r89s = message.MessageBlock.MessageBlocks.OfType<RECR89>();
			AssertEquals(1, r89s.Count());
			var r89 = r89s.FirstOrDefault();
			AssertEquals("Fee", Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, r89.FeeClass);
			AssertEquals("Fee original amount", 0m, r89.TotalOriginalFee);
			AssertEquals("Fee recon amount", 0m, r89.TotalEstimateReconciliationFee);

			AssertEquals("Fee", Core.Constants.USCustoms.FeeCodes.HMF, r89.FeeClass1);
			AssertEquals("Fee original amount", 0m, r89.TotalOriginalFee1);
			AssertEquals("Fee recon amount", 0m, r89.TotalEstimateReconciliationFee1);

			AssertEquals("Fee", "", r89.FeeClass2);
			AssertEquals("Fee original amount", 0m, r89.TotalOriginalFee2);
			AssertEquals("Fee recon amount", 0m, r89.TotalReconciliationFee2);

			var r90 = message.MessageBlock.MessageBlocks.OfType<RECR90>().FirstOrDefault();
			AssertEquals("OriginalDuty should be zero", 0m, r90.TotalOriginalDuty);
			AssertEquals("EstimatedReconciliationDuty should be zero", 0m, r90.TotalEstimateReconciliationDuty);
			AssertEquals("OriginalTax should be zero", 0m, r90.TotalOriginalTax);
			AssertEquals("EstimatedReconciliationTax should be zero", 0m, r90.TotalEstimateReconciliationTax);
		}

		public void TestSendMessagesWithRefundedFeeCodes()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			reconDeclaration.US_PreliminaryStatementPrintDate = new ZDate(1999, 9, 15);

			var entry1 = reconDeclaration.OriginalEntries.AddNew();
			entry1.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4300m);
			entry1.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4301m);

			entry1.US_PaymentDate = new ZDate(1999, 1, 5);

			var entry2 = reconDeclaration.OriginalEntries.AddNew();
			entry2.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4000m);
			entry2.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4001m);
			entry2.US_PaymentDate = new ZDate(1999, 4, 12);
			entry2.RefundedFees.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);

			reconDeclaration.CalculateDutyFeesForAllEntries();

			var message = new ReconciliationMessageBuilder("A", new ReconDeclarationIReconciliation(reconDeclaration)).Generate();

			var r89s = message.MessageBlock.MessageBlocks.OfType<RECR89>();
			AssertEquals(1, r89s.Count());

			var r89 = r89s.FirstOrDefault();
			AssertEquals("Fee", Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, r89.FeeClass);
			AssertEquals("Fee original amount", 0m, r89.TotalOriginalFee);
			AssertEquals("Fee recon amount", 0m, r89.TotalEstimateReconciliationFee);

			var r90 = message.MessageBlock.MessageBlocks.OfType<RECR90>().FirstOrDefault();
			AssertEquals("OriginalDuty", 8300m, r90.TotalOriginalDuty);
			AssertEquals("EstimatedReconciliationDuty", 8302m, r90.TotalEstimateReconciliationDuty);
			AssertEquals("OriginalTax", 0m, r90.TotalOriginalTax);
			AssertEquals("EstimatedReconciliationTax", 0m, r90.TotalEstimateReconciliationTax);
		}

		public void TestGenerateForNonAggregatedRecon()
		{
			Mock<IReconciliation> mock = GetTheMock();
			mock.Setup(m => m.AggregateReconciliationIndicator).Returns(false);

			IReconciliation reconciliation = mock.Object;
			MQEDIMessage message = new ReconciliationMessageBuilder("A", reconciliation).Generate();
			ZString text = message.EM_FormattedMessageText;

			BlockControlGenerator messageBlock = new ABIInputBlockControlGenerator(reconciliation.EntryFilerCode, reconciliation.ProcessingDistrictPort, ZString.Empty);
			messageBlock.Deserialise(BlockPadder.Pad(message.EM_MessageText));

			var r10 = messageBlock.MessageBlocks.OfType<RECR10>().FirstOrDefault();
			AssertEquals("PreCondition:Aggregate", "N", r10.AggregateReconciliationIndicator);
			List<IReconciliationImportEntry> importEntries = new List<IReconciliationImportEntry>(reconciliation.ImportEntries);

			AssertEquals("PreCondition:OriginalDuty > 0", true, importEntries[0].OriginalDuty > 0);
			AssertEquals("PreCondition:EstimatedReconciliationDuty > 0", true, importEntries[0].EstimatedReconciliationDuty > 0);
			AssertEquals("PreCondition:OriginalTax > 0", true, importEntries[0].OriginalTax > 0);
			AssertEquals("PreCondition:EstimatedReconciliationTax > 0", true, importEntries[0].EstimatedReconciliationTax > 0);
			AssertEquals("PreCondition:EstimatedReconciliationInterest > 0", true, importEntries[0].EstimatedReconciliationInterest > 0);

			List<MessageBlock> r20s = messageBlock.MessageBlocks.FindAll((MessageBlock block) => block is RECR20);
			AssertEquals("one 20 record is expected", 1, r20s.Count);

			RECR20 r20 = (RECR20)r20s[0];

			AssertEquals("OriginalDuty correctly filled in", 100m, r20.OriginalDuty);
			AssertEquals("EstimatedReconciliationDuty correctly filled in", 120m, r20.EstimatedReconciliationDuty);
			AssertEquals("OriginalTax correctly filled in", 200m, r20.OriginalTax);
			AssertEquals("EstimatedReconciliationTax correctly filled in", 220m, r20.EstimatedReconciliationTax);
			AssertEquals("EstimatedReconciliationInterest correctly filled in", 30m, r20.EstimatedReconciliationInterest);

			var r90 = messageBlock.MessageBlocks.OfType<RECR90>().FirstOrDefault();
			AssertEquals("OriginalDuty correctly filled in", 100m, r90.TotalOriginalDuty);
			AssertEquals("EstimatedReconciliationDuty correctly filled in", 120m, r90.TotalEstimateReconciliationDuty);
			AssertEquals("OriginalTax correctly filled in", 200m, r90.TotalOriginalTax);
			AssertEquals("EstimatedReconciliationTax correctly filled in", 220m, r90.TotalEstimateReconciliationTax);

			var r91 = messageBlock.MessageBlocks.OfType<RECR91>().FirstOrDefault();
			AssertEquals("EstimatedReconciliationInterest correctly filled in", 30m, r91.TotalEstimateReconciliationInterest);
		}

		[TestDate(2007, 9, 18)]
		public void TestElementsAreAdded()
		{
			DeclarationTestHelper.SetEntryFilerCode("XXX");

			Mock<IReconciliation> mock = GetTheMock();

			IReconciliation reconciliation = mock.Object;
			MQEDIMessage message = new ReconciliationMessageBuilder("A", reconciliation).Generate();
			ZString text = message.EM_FormattedMessageText;
			Assert(text.Contains("R10"));
			Assert(text.Contains("R15"));
			Assert(text.Contains("R16"));
			Assert(text.Contains("R17"));
			Assert(text.Contains("R20"));
			Assert(text.Contains("R900001"));
			Assert(text.Contains("R91"));

			BlockControlGenerator messageBlock = new ABIInputBlockControlGenerator(reconciliation.EntryFilerCode, reconciliation.ProcessingDistrictPort, ZString.Empty);
			messageBlock.Deserialise(BlockPadder.Pad(message.EM_MessageText));

			List<MessageBlock> r21s = messageBlock.MessageBlocks.FindAll((MessageBlock block) => block is RECR21);
			AssertEquals("no 21 blocks should have been created for AggregateRecon", 0, r21s.Count);

			var r91 = messageBlock.MessageBlocks.OfType<RECR91>().FirstOrDefault();
			AssertEquals("InterestAmount should be there", 40m, r91.TotalEstimateReconciliationInterest);
		}

		public void TestGenerateDeleteReconciliation()
		{
			DeclarationTestHelper.SetEntryFilerCode("XXX");

			var mock = GetTheMock();

			var reconciliation = mock.Object;
			var message = new ReconciliationMessageBuilder("D", reconciliation).Generate();
			var text = message.EM_FormattedMessageText;
			AssertEquals("Message Text", "B012304XJ5RA                                1 1234XJ51 2   <<MSGNO PLACEHOLDER>>R10D<XXXE#PLCH>2304                                                             Y  2304XJ5RA00001", message.EM_MessageText);

			var messageBlock = new ABIInputBlockControlGenerator(reconciliation.EntryFilerCode, reconciliation.ProcessingDistrictPort, ZString.Empty);
			messageBlock.Deserialise(BlockPadder.Pad(message.EM_MessageText));

			var r15s = messageBlock.MessageBlocks.FindAll((MessageBlock block) => block is RECR15);
			AssertEquals("no 15 blocks should have been created for Delete transaction", 0, r15s.Count);

			var r20s = messageBlock.MessageBlocks.FindAll((MessageBlock block) => block is RECR20);
			AssertEquals("no 20 blocks should have been created for Delete transaction", 0, r20s.Count);

			var r90s = messageBlock.MessageBlocks.FindAll((MessageBlock block) => block is RECR90);
			AssertEquals("no 90 blocks should have been created for Delete transaction", 0, r90s.Count);
		}

		public void TestGenerateWithFeesForWaiveRefund()
		{
			var mock = new Mock<IReconciliation>();

			// R10
			mock.Setup(m => m.EntryFilerCode).Returns("XJ5");
			mock.Setup(m => m.Factory).Returns(Factory);
			mock.Setup(m => m.EntryNumber).Returns("EN");
			mock.Setup(m => m.ProcessingDistrictPort).Returns("2304");
			mock.Setup(m => m.PreparerDistrictPort).Returns("1234");
			mock.Setup(m => m.OfficeCode).Returns("1");
			mock.Setup(m => m.ImporterID).Returns("IID");
			mock.Setup(m => m.SuretyCode).Returns("SCD");
			mock.Setup(m => m.EstimatedReconciliationEntrySummaryDate).Returns(ZDate.BrettsBirthday);
			mock.Setup(m => m.IssueCode).Returns("IC");
			mock.Setup(m => m.AggregateReconciliationIndicator).Returns(false);
			mock.Setup(m => m.IncreaseRefundIndicator).Returns("2");
			mock.Setup(m => m.IsWaiveRefund).Returns(true);
			mock.Setup(m => m.EarliestImportDate).Returns(new ZDate(2000, 1, 1));
			mock.Setup(m => m.EarliestEntrySummaryDate).Returns(new ZDate(2000, 1, 2));
			mock.Setup(m => m.AgentBrokerReferenceID).Returns("ABR");
			mock.Setup(m => m.BrokerReferenceNumber).Returns("BRN");

			// R15 + R16
			mock.Setup(m => m.ImportEntrySource).Returns(2);
			mock.Setup(m => m.TextComment).Returns(new ZString('A', 75) + new ZString('B', 76));

			mock.Setup(m => m.PaymentTypeIndicator).Returns(PaymentTypeList.Codes.IndividualBasis);
			mock.Setup(m => m.PreliminaryStatementPrintDate).Returns(ZDate.BrettsBirthday);
			mock.Setup(m => m.ClientBranchDesignation).Returns("");

			mock.Setup(m => m.DutyPaymentAmount).Returns(10m);
			mock.Setup(m => m.TaxPaymentAmount).Returns(20m);
			mock.Setup(m => m.FeePaymentAmount).Returns(30m);
			mock.Setup(m => m.InterestPaymentAmount).Returns(40m);
			mock.Setup(m => m.TeamNumber).Returns(new ZString("R1R"));
			mock.Setup(m => m.AggregateRefundedFees).Returns(System.Array.Empty<ZString>());

			var importEntryMock = new Mock<IReconciliationImportEntry>();
			importEntryMock.Setup(m => m.ImportEntryFilerCodeNumber).Returns("IEFCN");
			importEntryMock.Setup(m => m.Port).Returns("3901");
			importEntryMock.Setup(m => m.OriginalDuty).Returns(100m);
			importEntryMock.Setup(m => m.EstimatedReconciliationDuty).Returns(120m);
			importEntryMock.Setup(m => m.OriginalTax).Returns(200m);
			importEntryMock.Setup(m => m.EstimatedReconciliationTax).Returns(220m);
			importEntryMock.Setup(m => m.EstimatedReconciliationInterest).Returns(30m);

			var refundFeeMock = new Mock<IReconciliationImportEntryFee>();
			refundFeeMock.Setup(m => m.FeeClass).Returns(new ZString("499"));
			refundFeeMock.Setup(m => m.OriginalFee).Returns(49m);
			refundFeeMock.Setup(m => m.EstimatedReconciliationFee).Returns(42.50m);

			importEntryMock.Setup(m => m.Fees).Returns(new IReconciliationImportEntryFee[] { refundFeeMock.Object });

			mock.Setup(m => m.ImportEntries).Returns(new IReconciliationImportEntry[] { importEntryMock.Object });
			mock.Setup(m => m.IsNoChangeAggregate).Returns(false);

			var message = new ReconciliationMessageBuilder("A", mock.Object).Generate();
			var r21s = message.MessageBlock.MessageBlocks.FindAll(x => x is RECR21);
			AssertEquals(1, r21s.Count);

			var r21 = (RECR21)r21s[0];
			AssertEquals("499", r21.FirstFeeClass);
			AssertEquals(49m, r21.FirstOriginalFee);
			AssertEquals(42.50m, r21.FirstEstimateReconciliationFee);

			var r89s = message.MessageBlock.MessageBlocks.FindAll(x => x is RECR89);
			AssertEquals(1, r89s.Count);

			var r89 = (RECR89)r89s[0];
			AssertEquals(49m, r89.TotalEstimateReconciliationFee);
			AssertEquals(0m, r89.TotalEstimateReconciliationFee1);
			AssertEquals(49m, r89.TotalOriginalFee);
			AssertEquals(0m, r89.TotalOriginalFee1);
			AssertEquals(0m, r89.TotalOriginalFee2);
			AssertEquals(0m, r89.TotalReconciliationFee2);

			var r90s = message.MessageBlock.MessageBlocks.FindAll(x => x is RECR90);
			AssertEquals(1, r90s.Count);

			var r90 = (RECR90)r90s[0];
			AssertEquals(100m, r90.TotalOriginalDuty);
			AssertEquals(120m, r90.TotalEstimateReconciliationDuty);
			AssertEquals(200m, r90.TotalOriginalTax);
			AssertEquals(220m, r90.TotalEstimateReconciliationTax);
		}

		Mock<IReconciliation> GetTheMock()
		{
			var mock = new Mock<IReconciliation>();

			// R10
			mock.Setup(m => m.EntryFilerCode).Returns("XJ5");
			mock.Setup(m => m.Factory).Returns(Factory);
			mock.Setup(m => m.EntryNumber).Returns("EN");
			mock.Setup(m => m.ProcessingDistrictPort).Returns("2304");
			mock.Setup(m => m.PreparerDistrictPort).Returns("1234");
			mock.Setup(m => m.OfficeCode).Returns("1");
			mock.Setup(m => m.ImporterID).Returns("IID");
			mock.Setup(m => m.SuretyCode).Returns("SCD");
			mock.Setup(m => m.EstimatedReconciliationEntrySummaryDate).Returns(ZDate.BrettsBirthday);
			mock.Setup(m => m.IssueCode).Returns("IC");
			mock.Setup(m => m.AggregateReconciliationIndicator).Returns(true);
			mock.Setup(m => m.IncreaseRefundIndicator).Returns("2");
			mock.Setup(m => m.IsWaiveRefund).Returns(false);
			mock.Setup(m => m.EarliestImportDate).Returns(new ZDate(2000, 1, 1));
			mock.Setup(m => m.EarliestEntrySummaryDate).Returns(new ZDate(2000, 1, 2));
			mock.Setup(m => m.AgentBrokerReferenceID).Returns("ABR");
			mock.Setup(m => m.BrokerReferenceNumber).Returns("BRN");

			// R15 + R16
			mock.Setup(m => m.ImportEntrySource).Returns(2);
			mock.Setup(m => m.TextComment).Returns(new ZString('A', 75) + new ZString('B', 76));

			mock.Setup(m => m.PaymentTypeIndicator).Returns(PaymentTypeList.Codes.IndividualBasis);
			mock.Setup(m => m.PreliminaryStatementPrintDate).Returns(ZDate.BrettsBirthday);
			mock.Setup(m => m.ClientBranchDesignation).Returns("");

			mock.Setup(m => m.DutyPaymentAmount).Returns(10m);
			mock.Setup(m => m.TaxPaymentAmount).Returns(20m);
			mock.Setup(m => m.FeePaymentAmount).Returns(30m);
			mock.Setup(m => m.InterestPaymentAmount).Returns(40m);
			mock.Setup(m => m.TeamNumber).Returns(new ZString("R1R"));
			mock.Setup(m => m.AggregateRefundedFees).Returns(System.Array.Empty<ZString>());

			var importEntries = new List<IReconciliationImportEntry>();
			var importEntryMock = new Mock<IReconciliationImportEntry>();
			importEntryMock.Setup(m => m.ImportEntryFilerCodeNumber).Returns("IEFCN");
			importEntryMock.Setup(m => m.Port).Returns("3901");
			importEntryMock.Setup(m => m.OriginalDuty).Returns(100m);
			importEntryMock.Setup(m => m.EstimatedReconciliationDuty).Returns(120m);
			importEntryMock.Setup(m => m.OriginalTax).Returns(200m);
			importEntryMock.Setup(m => m.EstimatedReconciliationTax).Returns(220m);
			importEntryMock.Setup(m => m.EstimatedReconciliationInterest).Returns(30m);

			importEntryMock.Setup(m => m.Fees).Returns(System.Array.Empty<IReconciliationImportEntryFee>());
			importEntries.Add(importEntryMock.Object);

			mock.Setup(m => m.ImportEntries).Returns(importEntries);

			mock.Setup(m => m.IsNoChangeAggregate).Returns(false);

			return mock;
		}
	}
}
