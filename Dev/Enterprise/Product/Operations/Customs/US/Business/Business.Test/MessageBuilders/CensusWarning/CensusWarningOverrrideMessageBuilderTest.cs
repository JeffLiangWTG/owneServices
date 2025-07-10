using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class CensusWarningOverrrideMessageBuilderTest : TestCaseWithFactory
	{
		public void TestEndToEnd()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			var one = invoiceLine.CensusWarningOverrides.AddNew();
			one.CY_Code = CensusWarningCodeList.Codes.GrossWeightVessel;
			one.CY_Data = CensusOverrideCodeList.Codes._01;

			var two = invoiceLine.CensusWarningOverrides.AddNew();
			two.CY_Code = CensusWarningCodeList.Codes.GrossWeightVessel;
			two.CY_Data = CensusOverrideCodeList.Codes._02;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var message = new CensusWarningOverrrideMessageBuilder(entry).PopulateMessage();

			var cw01 = message.MessageBlock.MessageBlocks.OfType<ACWOCW01>().FirstOrDefault();
			var cw02s = message.MessageBlock.MessageBlocks.FindAll(x => x is ACWOCW02);

			AssertEquals("XJ5", cw01.EntryFilerCode);
			AssertEquals(1, cw02s.Count);

			var cw02 = (ACWOCW02)cw02s[0];

			AssertEquals("001", cw02.EntrySummaryLineItemIdentifier);
			AssertEquals(CensusWarningCodeList.Codes.GrossWeightVessel, cw02.CensusWarningConditionCode1);
			AssertEquals(CensusOverrideCodeList.Codes._01, cw02.CensusWarningConditionOverrideCode1);

			AssertEquals(CensusWarningCodeList.Codes.GrossWeightVessel, cw02.CensusWarningConditionCode2);
			AssertEquals(CensusOverrideCodeList.Codes._02, cw02.CensusWarningConditionOverrideCode2);

			var three = invoiceLine.CensusWarningOverrides.AddNew();
			three.CY_Code = CensusWarningCodeList.Codes.GrossWeightVessel;
			three.CY_Data = CensusOverrideCodeList.Codes._03;

			var four = invoiceLine.CensusWarningOverrides.AddNew();
			four.CY_Code = CensusWarningCodeList.Codes.GrossWeightVessel;
			four.CY_Data = CensusOverrideCodeList.Codes._04;

			var five = invoiceLine.CensusWarningOverrides.AddNew();
			five.CY_Code = CensusWarningCodeList.Codes.GrossWeightVessel;
			five.CY_Data = CensusOverrideCodeList.Codes._05;

			var six = invoiceLine.CensusWarningOverrides.AddNew();
			six.CY_Code = CensusWarningCodeList.Codes.GrossWeightVessel;
			six.CY_Data = CensusOverrideCodeList.Codes._06;

			var seven = invoiceLine.CensusWarningOverrides.AddNew();
			seven.CY_Code = CensusWarningCodeList.Codes.GrossWeightVessel;
			seven.CY_Data = CensusOverrideCodeList.Codes._07;

			var eight = invoiceLine.CensusWarningOverrides.AddNew();
			eight.CY_Code = CensusWarningCodeList.Codes.GrossWeightVessel;
			eight.CY_Data = CensusOverrideCodeList.Codes._08;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			message = new CensusWarningOverrrideMessageBuilder(entry).PopulateMessage();
			cw02s = message.MessageBlock.MessageBlocks.FindAll(x => x is ACWOCW02);

			AssertEquals(2, cw02s.Count);

			cw02 = (ACWOCW02)cw02s[1];
			AssertEquals("001", cw02.EntrySummaryLineItemIdentifier);
			AssertEquals(CensusWarningCodeList.Codes.GrossWeightVessel, cw02.CensusWarningConditionCode1);
			AssertEquals(CensusOverrideCodeList.Codes._08, cw02.CensusWarningConditionOverrideCode1);
		}

		public void TestEndToEndWithTwoLines()
		{
			Business.Testing.DeclarationTestHelper.SetEntryFilerCode("SV9");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			declaration.US_PreparerDistrictPort = "3910";
			declaration.US_SchDEntry = "4601";

			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var one = invoiceLine.CensusWarningOverrides.AddNew();
			one.CY_Code = CensusWarningCodeList.Codes.GrossWeightVessel;
			one.CY_Data = CensusOverrideCodeList.Codes._01;

			var two = invoiceLine.CensusWarningOverrides.AddNew();
			two.CY_Code = CensusWarningCodeList.Codes.GrossWeightVessel;
			two.CY_Data = CensusOverrideCodeList.Codes._02;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var three = invoiceLine2.CensusWarningOverrides.AddNew();
			three.CY_Code = CensusWarningCodeList.Codes.GrossWeightVessel;
			three.CY_Data = CensusOverrideCodeList.Codes._03;

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var message = new CensusWarningOverrrideMessageBuilder(entry).PopulateMessage();

			var cw01 = message.MessageBlock.MessageBlocks.OfType<ACWOCW01>().FirstOrDefault();
			var cw02s = message.MessageBlock.MessageBlocks.FindAll(x => x is ACWOCW02);

			AssertEquals("XJ5", cw01.EntryFilerCode);
			AssertEquals(2, cw02s.Count);

			var cw02 = (ACWOCW02)cw02s[0];

			AssertEquals("001", cw02.EntrySummaryLineItemIdentifier);
			AssertEquals(CensusWarningCodeList.Codes.GrossWeightVessel, cw02.CensusWarningConditionCode1);
			AssertEquals(CensusOverrideCodeList.Codes._01, cw02.CensusWarningConditionOverrideCode1);

			AssertEquals(CensusWarningCodeList.Codes.GrossWeightVessel, cw02.CensusWarningConditionCode2);
			AssertEquals(CensusOverrideCodeList.Codes._02, cw02.CensusWarningConditionOverrideCode2);

			cw02 = (ACWOCW02)cw02s[1];

			AssertEquals("002", cw02.EntrySummaryLineItemIdentifier);
			AssertEquals(CensusWarningCodeList.Codes.GrossWeightVessel, cw02.CensusWarningConditionCode1);
			AssertEquals(CensusOverrideCodeList.Codes._03, cw02.CensusWarningConditionOverrideCode1);

			var b = (AABIInputB)message.MessageBlock.B;
			AssertEquals("B  3910SV9CW                                               <<MSGNO PLACEHOLDER>>", b.Serialise());
		}
	}
}
