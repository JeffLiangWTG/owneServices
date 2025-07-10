using System;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(SupportingDocSendingObject))]
	public class SupportingDocSendingObjectTest : Customs.Business.Testing.JobDeclarationSupportingDocSendingObjectTest
	{
		public override void TestDefaultLocalReferenceNumber()
		{
			var sendingObject = SupportingDocSendingObject.New(declaration);
			AssertEquals("12341234", sendingObject.LocalReferenceNumber);
			declaration.ActiveEntryHeaders.AddNew();
			sendingObject = SupportingDocSendingObject.New(declaration);
			AssertEquals(string.Empty, sendingObject.LocalReferenceNumber);
		}

		public override void TestEntries()
		{
			var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
			entryHeader2.CH_BGMReference = "ABC";
			var sendingObject = SupportingDocSendingObject.New(declaration);
			AssertEquals(2, sendingObject.Entries.Count);
			AssertContains("12341234", sendingObject.Entries[0].Code);
			AssertContains("ABC", sendingObject.Entries[1].Code);
			entryHeader2.CH_BGMReference = string.Empty;
			AssertEquals(2, sendingObject.Entries.Count);
		}

		public void TestCasenumbers_CodeDescription()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("12341234", ZDateTime.Empty);
			entryHeader.CH_BGMReference = "432";

			var sendingObject = SupportingDocSendingObject.New(declaration);
			AssertEquals(0, sendingObject.CaseNumbers.Count);

			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			instruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._11;
			var caseNo = instruction.CaseNumbers.AddNew();
			caseNo.CY_Data = "123";
			caseNo.CY_Code = CaseNumberTypeList.Codes.ElectronicProvisionalPayments;
			instruction.CaseNumbers.AddNew().CY_Data = "456";
			sendingObject = SupportingDocSendingObject.New(declaration);
			sendingObject.LocalReferenceNumber = "432";
			AssertEquals(2, sendingObject.CaseNumbers.Count);
			AssertEquals(CaseNumberTypeList.Codes.ElectronicProvisionalPayments, sendingObject.CaseNumbers.GetDescriptionFromCode("123"));
			AssertEquals(CaseNumberTypeList.Codes.DocumentInspectionCases, sendingObject.CaseNumbers.GetDescriptionFromCode("456"));
		}

		public void TestDefaultDocType()
		{
			CreateCusMaps();

			sendingObject.EDoc = eDocOnDeclaration.UniqueKey;
			AssertEquals("Document Type mapping CIV -> INV", "INV", sendingObject.DocumentType);
			sendingObject.LocalReferenceNumber = "12341234";
			sendingObject.EDoc = eDocOnHeader.UniqueKey;
			AssertEquals("Document Type mapping DGF -> DGS", "DGS", sendingObject.DocumentType);
		}

		protected override Type ExpectedAvailableEDocListType => typeof(AvailableEDocList);

		protected override void SetLocalReferenceNumberOnEntryHeader(Customs.Business.CusEntryHeader entryHeader, string number) => entryHeader.CH_BGMReference = number;

		void CreateCusMaps()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var zaCountry = Core.Constants.CountryCodes.SouthAfrica;
			helper.CreateCusMapType("ZADOC", "OUT", "ZA Supporting Document Types", false);
			helper.CreateCusMap("ZADOC", "DGF", "DGS", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 06, 06), zaCountry);
			helper.CreateCusMap("ZADOC", "CIV", "INV", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 06, 06), zaCountry);
			Factory.Save();
		}
	}
}
