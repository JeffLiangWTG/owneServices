using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(ProvisionalPaymentEntryPayInfoCollection))]
	sealed class ProvisionalPaymentEntryPayInfoCollectionTest : ActiveBusinessObjectCollectionTestCase<ProvisionalPaymentEntryPayInfoCollection>
	{
		public void TestCollectionLoading()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			var instruction3 = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader11 = declaration.ActiveEntryHeaders.AddNew();
			var entryHeader12 = declaration.ActiveEntryHeaders.AddNew();
			var entryHeader21 = declaration.ActiveEntryHeaders.AddNew();
			var entryHeader01 = declaration.ActiveEntryHeaders.AddNew();
			var anotherDeclaration = Factory.New<JobDeclaration>();
			var anotherEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader11.CH_CEI_Instruction = instruction1.PK;
			entryHeader12.CH_CEI_Instruction = instruction1.PK;
			entryHeader21.CH_CEI_Instruction = instruction2.PK;
			anotherEntryHeader.CH_CEI_Instruction = instruction1.PK;
			entryHeader11.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "VAT", 1m, "1", "REF");
			entryHeader11.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "", 1m, "1", "REF");
			entryHeader11.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "DTY", 1m, "1", "REF");
			entryHeader11.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "VAT", 1m, "1", "REF");
			entryHeader11.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "", 1m, "1", "REF");
			entryHeader11.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "DTY", 1m, "1", "REF");
			entryHeader11.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "OTH", 1m, "1", "REF");
			var pick1 = entryHeader11.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "PPE", 1m, "1", "REF");
			entryHeader11.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "PEN", 1m, "1", "REF");
			var pick2 = entryHeader11.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "FOR", 1m, "1", "REF");
			entryHeader11.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "PEN", 1m, "1", "");
			entryHeader11.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "XXX", 1m, "1", "REF");
			entryHeader12.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "VAT", 1m, "1", "REF");
			entryHeader12.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "", 1m, "1", "REF");
			entryHeader12.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "DTY", 1m, "1", "REF");
			entryHeader12.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "OTH", 1m, "1", "REF");
			var pick3 = entryHeader12.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "PEN", 1m, "1", "REF");
			var pick4 = entryHeader12.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "PPA", 1m, "1", "REF");
			entryHeader12.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "PPX", 1m, "1", "REF");
			entryHeader12.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "PEN", 1m, "1", "");
			entryHeader12.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "XXX", 1m, "1", "REF");
			entryHeader01.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "VAT", 1m, "1", "REF");
			entryHeader01.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "", 1m, "1", "REF");
			entryHeader01.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "DTY", 1m, "1", "REF");
			entryHeader01.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "OTH", 1m, "1", "REF");
			entryHeader01.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "PEN", 1m, "1", "REF");
			entryHeader01.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "PEN", 1m, "1", "");
			entryHeader01.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "XXX", 1m, "1", "REF");
			anotherEntryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "VAT", 1m, "1", "REF");
			anotherEntryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "", 1m, "1", "REF");
			anotherEntryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "DTY", 1m, "1", "REF");
			anotherEntryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "OTH", 1m, "1", "REF");
			anotherEntryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "PEN", 1m, "1", "REF");
			anotherEntryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "PEN", 1m, "1", "");
			anotherEntryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "XXX", 1m, "1", "REF");
			Factory.Save();
			CombineAssertions(() =>
			{
				var collection1 = new ProvisionalPaymentEntryPayInfoCollection(instruction1.Factory, instruction1.LinkedEntryHeaders);
				var collection2 = new ProvisionalPaymentEntryPayInfoCollection(instruction2.Factory, instruction2.LinkedEntryHeaders);
				var collection3 = new ProvisionalPaymentEntryPayInfoCollection(instruction3.Factory, instruction3.LinkedEntryHeaders);
				AssertContainsExactElementsInAnyOrder(new CusEntryPayInfo[] { pick1, pick2, pick3, pick4 }, collection1.ToArray());
				AssertContainsExactElementsInAnyOrder(System.Array.Empty<CusEntryPayInfo>(), collection2.ToArray());
				AssertContainsExactElementsInAnyOrder(System.Array.Empty<CusEntryPayInfo>(), collection3.ToArray());
			});
		}

		public void TestGetFetchHintQueryForProvisionalPaymentPayInfos()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var query = ProvisionalPaymentEntryPayInfoCollection.GetFetchHintQueryForProvisionalPaymentPayInfos(Factory, entryHeader);
			AssertEquals($"C9_PaymentReference <> '' and C9_PaymentParty = 'C' and (C9_TransactionType in ('FOR', 'PEN', 'PPA', 'PPC', 'PPE', 'PPG', 'PPR', 'PPT')) and C9_CH = CONVERT('{entryHeader.PK}', 'System.Guid')", query.LiteralTextADO);
		}

		public void TestHasPPTypeBeenLiquidated()
		{
			var testEntry = Factory.New<CusEntryHeader>();
			var testPayInfo1 = testEntry.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", LineLevelProvisionalPayments.Codes.PPA, 0, "1", "REF1", true);
			var testPayInfo2 = testEntry.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", LineLevelProvisionalPayments.Codes.PPA, 0, "1", "REF2", false);
			var testPayInfo3 = testEntry.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", LineLevelProvisionalPayments.Codes.PPC, 0, "1", "REF3", true);
			var testPayInfo4 = testEntry.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", LineLevelProvisionalPayments.Codes.PPG, 0, "1", "REF4", false);
			var testPayInfo5 = testEntry.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", LineLevelProvisionalPayments.Codes.PPG, 0, "2", "REF5", true);
			var testPayInfo6 = testEntry.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", LineLevelProvisionalPayments.Codes.PPR, 0, "2", "REF6", true);
			CombineAssertions(() =>
			{
				AssertEquals("1 - PPA", true, testEntry.ProvisionalPaymentPayInfos.HasPPTypeBeenLiquidated(LineLevelProvisionalPayments.Codes.PPA, 1));
				AssertEquals("1 - PPC", true, testEntry.ProvisionalPaymentPayInfos.HasPPTypeBeenLiquidated(LineLevelProvisionalPayments.Codes.PPC, 1));
				AssertEquals("1 - PPG", false, testEntry.ProvisionalPaymentPayInfos.HasPPTypeBeenLiquidated(LineLevelProvisionalPayments.Codes.PPG, 1));
				AssertEquals("1 - PPR", false, testEntry.ProvisionalPaymentPayInfos.HasPPTypeBeenLiquidated(LineLevelProvisionalPayments.Codes.PPR, 1));
				AssertEquals("1 - PEN", false, testEntry.ProvisionalPaymentPayInfos.HasPPTypeBeenLiquidated(LineLevelProvisionalPayments.Codes.PEN, 1));
				AssertEquals("1 - XXX", false, testEntry.ProvisionalPaymentPayInfos.HasPPTypeBeenLiquidated("XXX", 1));
				AssertEquals("2 - PPA", false, testEntry.ProvisionalPaymentPayInfos.HasPPTypeBeenLiquidated(LineLevelProvisionalPayments.Codes.PPA, 2));
				AssertEquals("2 - PPC", false, testEntry.ProvisionalPaymentPayInfos.HasPPTypeBeenLiquidated(LineLevelProvisionalPayments.Codes.PPC, 2));
				AssertEquals("2 - PPG", true, testEntry.ProvisionalPaymentPayInfos.HasPPTypeBeenLiquidated(LineLevelProvisionalPayments.Codes.PPG, 2));
				AssertEquals("2 - PPR", true, testEntry.ProvisionalPaymentPayInfos.HasPPTypeBeenLiquidated(LineLevelProvisionalPayments.Codes.PPR, 2));
				AssertEquals("2 - PEN", false, testEntry.ProvisionalPaymentPayInfos.HasPPTypeBeenLiquidated(LineLevelProvisionalPayments.Codes.PEN, 2));
				AssertEquals("2 - XXX", false, testEntry.ProvisionalPaymentPayInfos.HasPPTypeBeenLiquidated("XXX", 2));
				AssertEquals("3 - PPA", false, testEntry.ProvisionalPaymentPayInfos.HasPPTypeBeenLiquidated(LineLevelProvisionalPayments.Codes.PPA, 3));
				AssertEquals("3 - PPC", false, testEntry.ProvisionalPaymentPayInfos.HasPPTypeBeenLiquidated(LineLevelProvisionalPayments.Codes.PPC, 3));
				AssertEquals("3 - PPG", false, testEntry.ProvisionalPaymentPayInfos.HasPPTypeBeenLiquidated(LineLevelProvisionalPayments.Codes.PPG, 3));
				AssertEquals("3 - PPR", false, testEntry.ProvisionalPaymentPayInfos.HasPPTypeBeenLiquidated(LineLevelProvisionalPayments.Codes.PPR, 3));
				AssertEquals("3 - PEN", false, testEntry.ProvisionalPaymentPayInfos.HasPPTypeBeenLiquidated(LineLevelProvisionalPayments.Codes.PEN, 3));
				AssertEquals("3 - XXX", false, testEntry.ProvisionalPaymentPayInfos.HasPPTypeBeenLiquidated("XXX", 3));
			});
		}

		public new void TestDelete()
		{
			var testEntry = Factory.New<CusEntryHeader>();
			var testPayInfo1 = testEntry.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", LineLevelProvisionalPayments.Codes.PPA, 0, "1", "REF1", true);
			var testPayInfo2 = testEntry.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", LineLevelProvisionalPayments.Codes.PPR, 0, "2", "REF2", true);
			CombineAssertions(() =>
			{
				var tester = testEntry.ProvisionalPaymentPayInfos;
				AssertEquals("Initial", 2, tester.Count);
				tester.Delete(tester[0]);
				AssertEquals("Test Delete", 2, tester.Count);
				tester.DeleteAll();
				AssertEquals("Test Delete All", 2, tester.Count);
			});
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<ProvisionalPaymentCusEntryPayInfo>();
			result.C9_PaymentDate = ZDateTime.Now;
			result.C9_CusResReceived = true;
			result.C9_RemAdvReceived = false;
			result.C9_PaymentParty = "C";
			result.C9_TransactionType = "PPA";
			result.C9_PaymentAmount = 1m;
			result.C9_PaymentReference = "REF";
			result.C9_IncomingPayResponseNo = "1";
			result.C9_CH = entryHeader1.PK;
			return result;
		}

		protected override ProvisionalPaymentEntryPayInfoCollection GetCollectionToTest()
		{
			declaration = Factory.New<JobDeclaration>();
			instruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
			entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
			entryHeader1.CH_CEI_Instruction = instruction.PK;
			entryHeader2.CH_CEI_Instruction = instruction.PK;
			var pick1 = entryHeader1.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "PEN", 1m, "1", "REF");
			var pick2 = entryHeader1.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "FOR", 1m, "1", "REF");
			var pick3 = entryHeader2.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "PEN", 1m, "1", "REF");
			var pick4 = entryHeader2.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "PPA", 1m, "1", "REF");
			Factory.Save();
			return new ProvisionalPaymentEntryPayInfoCollection(Factory, instruction.LinkedEntryHeaders);
		}

		JobDeclaration declaration;
		CusEntryInstruction instruction;
		CusEntryHeader entryHeader1;
		CusEntryHeader entryHeader2;
	}
}
