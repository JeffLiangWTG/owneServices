using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Customs.ZA.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(SAD501DocumentPageWrapper))]
	sealed class SAD501DocumentPageWrapperTests : NonPersistentBusinessObjectTestCase
	{
		public void TestSAD501DocumentPageWrapper_ShowSeparatedDutiesOnSum()
		{
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "", "11", "00", "", "", "", false, false, false, false);
			Factory.Save();
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			testDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testInst = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_Style = "40";
			var invHeader = testDeclaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = testInst.PK;
			invLine.JI_Procedure = invLine.EntryInstruction.CEI_Style + "43";
			invLine.JI_Tariff = "00001000";
			invLine.JI_ZZF_NKTaxType = "VAT";
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			var testLine1 = testHeader.MergedLines.AddNew();
			testLine1.CL_CustomsValue = 20m;
			testLine1.Fees.AddOrUpdate("1P1", 500m);
			testLine1.Fees.AddOrUpdate("12A", 400m);
			testLine1.Fees.AddOrUpdate("12B", 300m);
			testLine1.Fees.AddOrUpdate("VAT", 200m);
			testLine1.ProvisionalPayments.AddNew("PPA", 50m);
			testLine1.ProvisionalPayments.AddNew("PPR", 40m);
			testLine1.ProvisionalPayments.AddNew("PEN", 30m);
			testLine1.InvoiceLines.Add(invLine);
			var firstLineWrapper = new SADLineDetailWrapper(testLine1, testHeader);
			AssertEquals(5, firstLineWrapper.CalcDutiesAndFees.Count);
			AssertEquals("1P1", firstLineWrapper.CalcDutiesAndFees[0].Code);
			AssertEquals("12A", firstLineWrapper.CalcDutiesAndFees[1].Code);
			AssertEquals("VAT", firstLineWrapper.CalcDutiesAndFees[2].Code);
			AssertEquals("12B", firstLineWrapper.CalcDutiesAndFees[3].Code);
			AssertEquals("PP's", firstLineWrapper.CalcDutiesAndFees[4].Code);
			AssertEquals(500m, firstLineWrapper.CalcDutiesAndFees[0].Value);
			AssertEquals(400m, firstLineWrapper.CalcDutiesAndFees[1].Value);
			AssertEquals(200m, firstLineWrapper.CalcDutiesAndFees[2].Value);
			AssertEquals(300m, firstLineWrapper.CalcDutiesAndFees[3].Value);
			AssertEquals(120m, firstLineWrapper.CalcDutiesAndFees[4].Value);
			var secondLineWrapper = new SADLineDetailWrapper(testLine1, testHeader);
			var thirdLineWrapper = new SADLineDetailWrapper(testLine1, testHeader);
			var tester = new SAD501DocumentPageWrapper(firstLineWrapper, secondLineWrapper, thirdLineWrapper, null);
			AssertNotNull(tester.FirstLine);
			AssertNotNull(tester.SecondLine);
			AssertNotNull(tester.ThirdLine);
			AssertNotNull(tester.RunningTotalDutiesAndFeesOfThisPage);
			AssertEquals(5, tester.RunningTotalDutiesAndFeesOfThisPage.Count);
			AssertEquals(1500m, tester.RunningTotalDutiesAndFeesOfThisPage.ToArray<DutyFeeInformationDocWrapper>().First(x => x.Code == "1P1").Value);
			AssertEquals(1200m, tester.RunningTotalDutiesAndFeesOfThisPage.ToArray<DutyFeeInformationDocWrapper>().First(x => x.Code == "12A").Value);
			AssertEquals(900m, tester.RunningTotalDutiesAndFeesOfThisPage.ToArray<DutyFeeInformationDocWrapper>().First(x => x.Code == "12B").Value);
			AssertEquals(600m, tester.RunningTotalDutiesAndFeesOfThisPage.ToArray<DutyFeeInformationDocWrapper>().First(x => x.Code == "VAT").Value);
			AssertEquals(360m, tester.RunningTotalDutiesAndFeesOfThisPage.ToArray<DutyFeeInformationDocWrapper>().First(x => x.Code == "PP's").Value);
		}

		public void TestSAD501DocumentPageWrapper_DutyConsolidationHappenedOnOneLineWrapper()
		{
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "", "11", "00", "", "", "", false, false, false, false);
			Factory.Save();
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			testDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testInst = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_Style = "40";
			var invHeader = testDeclaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = testInst.PK;
			invLine.JI_Procedure = invLine.EntryInstruction.CEI_Style + "43";
			invLine.JI_Tariff = "00001000";
			invLine.JI_ZZF_NKTaxType = "VAT";
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			var testLine1 = testHeader.MergedLines.AddNew();
			testLine1.CL_CustomsValue = 20m;
			testLine1.Fees.AddOrUpdate("1P1", 500m);
			testLine1.Fees.AddOrUpdate("12A", 400m);
			testLine1.Fees.AddOrUpdate("12B", 300m);
			testLine1.Fees.AddOrUpdate("VAT", 200m);
			testLine1.ProvisionalPayments.AddNew("PPA", 50m);
			testLine1.ProvisionalPayments.AddNew("PPR", 40m);
			testLine1.ProvisionalPayments.AddNew("PEN", 30m);
			testLine1.InvoiceLines.Add(invLine);
			var firstLineWrapper = new SADLineDetailWrapper(testLine1, testHeader);
			AssertEquals(5, firstLineWrapper.CalcDutiesAndFees.Count);
			AssertEquals("1P1", firstLineWrapper.CalcDutiesAndFees[0].Code);
			AssertEquals("12A", firstLineWrapper.CalcDutiesAndFees[1].Code);
			AssertEquals("VAT", firstLineWrapper.CalcDutiesAndFees[2].Code);
			AssertEquals("12B", firstLineWrapper.CalcDutiesAndFees[3].Code);
			AssertEquals("PP's", firstLineWrapper.CalcDutiesAndFees[4].Code);
			AssertEquals(500m, firstLineWrapper.CalcDutiesAndFees[0].Value);
			AssertEquals(400m, firstLineWrapper.CalcDutiesAndFees[1].Value);
			AssertEquals(200m, firstLineWrapper.CalcDutiesAndFees[2].Value);
			AssertEquals(300m, firstLineWrapper.CalcDutiesAndFees[3].Value);
			AssertEquals(120m, firstLineWrapper.CalcDutiesAndFees[4].Value);
			var testLine2 = testHeader.MergedLines.AddNew();
			testLine2.CL_CustomsValue = 20m;
			testLine2.Fees.AddOrUpdate("1P1", 500m);
			testLine2.Fees.AddOrUpdate("13A", 500m);
			testLine2.Fees.AddOrUpdate("12A", 400m);
			testLine2.Fees.AddOrUpdate("12B", 300m);
			testLine2.Fees.AddOrUpdate("VAT", 200m);
			testLine2.ProvisionalPayments.AddNew("PPA", 50m);
			testLine2.ProvisionalPayments.AddNew("PPR", 40m);
			testLine2.ProvisionalPayments.AddNew("PEN", 30m);
			testLine2.InvoiceLines.Add(invLine);
			var secondLineWrapper = new SADLineDetailWrapper(testLine2, testHeader);
			AssertEquals(4, secondLineWrapper.CalcDutiesAndFees.Count);
			AssertEquals("DTY", secondLineWrapper.CalcDutiesAndFees[0].Code);
			AssertEquals("VAT", secondLineWrapper.CalcDutiesAndFees[1].Code);
			AssertEquals("12B", secondLineWrapper.CalcDutiesAndFees[2].Code);
			AssertEquals("PP's", secondLineWrapper.CalcDutiesAndFees[3].Code);
			AssertEquals(1400m, secondLineWrapper.CalcDutiesAndFees[0].Value);
			AssertEquals(200m, secondLineWrapper.CalcDutiesAndFees[1].Value);
			AssertEquals(300m, secondLineWrapper.CalcDutiesAndFees[2].Value);
			AssertEquals(120m, secondLineWrapper.CalcDutiesAndFees[3].Value);
			var thirdLineWrapper = new SADLineDetailWrapper(testLine1, testHeader);
			var tester = new SAD501DocumentPageWrapper(firstLineWrapper, secondLineWrapper, thirdLineWrapper, null);
			AssertNotNull(tester.FirstLine);
			AssertNotNull(tester.SecondLine);
			AssertNotNull(tester.ThirdLine);
			AssertNotNull(tester.RunningTotalDutiesAndFeesOfThisPage);
			AssertEquals(4, tester.RunningTotalDutiesAndFeesOfThisPage.Count);
			AssertEquals(3200m, tester.RunningTotalDutiesAndFeesOfThisPage.ToArray<DutyFeeInformationDocWrapper>().First(x => x.Code == "DTY").Value);
			AssertEquals(900m, tester.RunningTotalDutiesAndFeesOfThisPage.ToArray<DutyFeeInformationDocWrapper>().First(x => x.Code == "12B").Value);
			AssertEquals(600m, tester.RunningTotalDutiesAndFeesOfThisPage.ToArray<DutyFeeInformationDocWrapper>().First(x => x.Code == "VAT").Value);
			AssertEquals(360m, tester.RunningTotalDutiesAndFeesOfThisPage.ToArray<DutyFeeInformationDocWrapper>().First(x => x.Code == "PP's").Value);
		}

		public void TestSAD501DocumentPageWrapper()
		{
			CombineAssertions(() =>
			{
				var nullLine = null as ILineLevelInformation;
				var nullLineWrapper = new SADLineDetailWrapper(nullLine, null);
				var tester = new SAD501DocumentPageWrapper(nullLineWrapper, nullLineWrapper, nullLineWrapper, null);
				AssertNotNull(tester.FirstLine);
				AssertNotNull(tester.SecondLine);
				AssertNotNull(tester.ThirdLine);
				AssertNotNull(tester.RunningTotalDutiesAndFeesOfThisPage);
				AssertEquals(0, tester.RunningTotalDutiesAndFeesOfThisPage.Count);
			});
			CombineAssertions(() =>
			{
				universalReferenceDataHelper.CreateRefCusProcedure("ZA", "", "11", "00", "", "", "", false, false, false, false);
				Factory.Save();
				var testDeclaration = Factory.New<JobDeclaration>();
				testDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				testDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				var testInst = testDeclaration.CustomsEntryInstructions.AddNew();
				testInst.CEI_Style = "11";
				var testInvoice = testDeclaration.Invoices.AddNew();
				var testInvLine1 = testInvoice.InvoiceLines.AddNew();
				var testInvLine2 = testInvoice.InvoiceLines.AddNew();
				var testInvLine3 = testInvoice.InvoiceLines.AddNew();
				testInvLine1.JI_CEI = testInst.PK;
				testInvLine2.JI_CEI = testInst.PK;
				testInvLine3.JI_CEI = testInst.PK;
				testInvLine1.JI_Procedure = testInvLine1.EntryInstruction.CEI_Style + "00";
				testInvLine2.JI_Procedure = testInvLine2.EntryInstruction.CEI_Style + "00";
				testInvLine3.JI_Procedure = testInvLine3.EntryInstruction.CEI_Style + "00";
				testInvLine1.JI_ZZF_NKTaxType = "VAT";
				testInvLine2.JI_ZZF_NKTaxType = "VAT";
				testInvLine3.JI_ZZF_NKTaxType = "VAT";
				var testHeader = testDeclaration.ActiveEntryHeaders.AddNew();
				testHeader.CH_CEI_Instruction = testInst.PK;
				var testLine1 = testHeader.MergedLines.AddNew();
				var testLine2 = testHeader.MergedLines.AddNew();
				var testLine3 = testHeader.MergedLines.AddNew();
				testLine1.CL_LineNumber = 1;
				testLine1.InvoiceLines.Add(testInvLine1);
				testLine2.InvoiceLines.Add(testInvLine2);
				testLine3.InvoiceLines.Add(testInvLine3);
				testInst.CEI_ProvisionalPaymentType = HeaderLevelProvisionalPayments.Codes.PPE;
				testInst.CEI_ProvisionalPaymentAmount = 10m;
				testLine1.Fees.AddOrUpdate("1P1", 11);
				testLine1.Fees.AddOrUpdate("12A", 12.1);
				testLine2.Fees.AddOrUpdate("1P1", 12);
				testLine2.Fees.AddOrUpdate("12B", 13.1);
				testLine2.ProvisionalPayments.AddNew("PPA", 16.1);
				testLine3.Fees.AddOrUpdate("1P1", 13);
				testLine3.Fees.AddOrUpdate("13A", 14.1);
				testLine3.Fees.AddOrUpdate("VAT", 15.1);
				testLine3.ProvisionalPayments.AddNew("PEN", 17.1);
				var firstLineWrapper = new SADLineDetailWrapper(testLine1, testHeader);
				var secondLineWrapper = new SADLineDetailWrapper(testLine2, testHeader);
				var thirdLineWrapper = new SADLineDetailWrapper(testLine3, testHeader);
				AssertEquals(3, firstLineWrapper.CalcDutiesAndFees.Count);
				AssertEquals(3, secondLineWrapper.CalcDutiesAndFees.Count);
				AssertEquals(4, thirdLineWrapper.CalcDutiesAndFees.Count);
				var tester = new SAD501DocumentPageWrapper(firstLineWrapper, secondLineWrapper, thirdLineWrapper, null);
				AssertNotNull(tester.FirstLine);
				AssertNotNull(tester.SecondLine);
				AssertNotNull(tester.ThirdLine);
				AssertNotNull(tester.RunningTotalDutiesAndFeesOfThisPage);
				AssertEquals(4, tester.RunningTotalDutiesAndFeesOfThisPage.Count);
				AssertEquals(62.2m, tester.RunningTotalDutiesAndFeesOfThisPage.ToArray<DutyFeeInformationDocWrapper>().First(x => x.Code == "DTY").Value);
				AssertEquals(13.1m, tester.RunningTotalDutiesAndFeesOfThisPage.ToArray<DutyFeeInformationDocWrapper>().First(x => x.Code == "12B").Value);
				AssertEquals(15.1m, tester.RunningTotalDutiesAndFeesOfThisPage.ToArray<DutyFeeInformationDocWrapper>().First(x => x.Code == "VAT").Value);
				AssertEquals(43.2m, tester.RunningTotalDutiesAndFeesOfThisPage.ToArray<DutyFeeInformationDocWrapper>().First(x => x.Code == "PP's").Value);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => new SAD501DocumentPageWrapper(null, null, null, null);

		protected override void SetUp()
		{
			universalReferenceDataHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			base.SetUp();
		}

		ZAUniversalReferenceTestDataHelper universalReferenceDataHelper;
	}
}
