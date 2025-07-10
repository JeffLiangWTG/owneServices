using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ACEAffirmationCode))]
	public class ACEAffirmationCodeTest : Customs.Business.Testing.CusCodeDataTest<ACEAffirmationCode>
	{
		public void TestProperties()
		{
			#region Create Reference Data
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			var codeTypeBIO = helper.CreateNewOrGetExistingCusCodeType("ACBIO", "FDA BIO Affirmation of Compliance Code", dataGrouping.ZZZ_DataGrouping);
			var codeListBLN = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeTypeBIO.ZZK_CodeType, ACE_AffirmationOfComplianceList.Codes.BLN, ACE_AffirmationOfComplianceList.Descriptions.BLN, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();
			#endregion

			var declaration = Factory.New<JobDeclaration>();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			affirmationCodes = fda.AffirmationCodes;
			var uscAffirmCode = affirmationCodes.AddNew();

			AssertEquals("Type", CusCodeDataTypeList.Codes.AffirmationCode, uscAffirmCode.CY_Type);
			AssertEquals("Validation", typeof(ACEAffirmationCodeValidation), uscAffirmCode.Validation.GetType());
			uscAffirmCode.CY_Code = "AAA";
			AssertEquals("FDA (AAA)", uscAffirmCode.CY_DataInfo.HumanReadableName);

			uscAffirmCode.CY_Code = "";
			AssertEquals("FDA", uscAffirmCode.CY_DataInfo.HumanReadableName);

			uscAffirmCode.CY_Code = ACE_AffirmationOfComplianceList.Codes.BLN;
			AssertEquals("Description", ACE_AffirmationOfComplianceList.Descriptions.BLN, uscAffirmCode.Description);
			uscAffirmCode.CY_Data = "testace";
			AssertEquals("TESTACE", uscAffirmCode.CY_Data);

			AssertNoExceptionThrown(() => uscAffirmCode.CY_Data = "MAXIMUMSHOULDDBETHIRTYLONGER25");
		}

		public void TestAOCList()
		{
			#region Create Reference Data
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			var codeTypeBIO = helper.CreateNewOrGetExistingCusCodeType("ACBIO", "FDA BIO Affirmation of Compliance Code", dataGrouping.ZZZ_DataGrouping);
			var attributeBIOUSAOCCodeMask = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USAOCCodeMask, "AOC Format Mask", codeTypeBIO.ZZK_CodeType, dataGrouping.ZZZ_DataGrouping);
			var attributeBIOUSAOCCodeErrorText = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USAOCCodeErrorText, "AOC Error Text", codeTypeBIO.ZZK_CodeType, dataGrouping.ZZZ_DataGrouping);
			var codeListSTN = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeTypeBIO.ZZK_CodeType, ACE_AffirmationOfComplianceList.Codes.STN, ACE_AffirmationOfComplianceList.Descriptions.STN, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListSTN.PK, attributeBIOUSAOCCodeMask.ZXE_Name, @"([0-9]{6})");
			var errorMessageSTN = @"a 6 digit number";
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListSTN.PK, attributeBIOUSAOCCodeErrorText.ZXE_Name, errorMessageSTN);
			var codeListDLS = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeTypeBIO.ZZK_CodeType, ACE_AffirmationOfComplianceList.Codes.DLS, ACE_AffirmationOfComplianceList.Descriptions.DLS, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListDLS.PK, attributeBIOUSAOCCodeMask.ZXE_Name, @"(([P|D|H|K][0-9]{6}|DEN[0-9]{6}|N[0-9]{4,6}))");
			var errorMessageDLS = @"one of: D, H, K or P followed by 6 digits OR N followed by 4 to 6 digits OR DEN followed by 6 digits";
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListDLS.PK, attributeBIOUSAOCCodeErrorText.ZXE_Name, errorMessageDLS);

			var codeTypeFOO = helper.CreateNewOrGetExistingCusCodeType("ACFOO", "FDA FOO Affirmation of Compliance Code", dataGrouping.ZZZ_DataGrouping);
			var attributeFOOUSAOCCodeMask = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USAOCCodeMask, "AOC Format Mask", codeTypeFOO.ZZK_CodeType, dataGrouping.ZZZ_DataGrouping);
			var attributeFOOUSAOCCodeErrorText = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USAOCCodeErrorText, "AOC Error Text", codeTypeFOO.ZZK_CodeType, dataGrouping.ZZZ_DataGrouping);
			var codeListFOO = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeTypeFOO.ZZK_CodeType, ACE_AffirmationOfComplianceList.Codes.CAN, ACE_AffirmationOfComplianceList.Descriptions.CAN, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListFOO.PK, attributeFOOUSAOCCodeMask.ZXE_Name, @"([0-9]{8})");
			var errorMessageFOO = @"a 8 digit number";
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListFOO.PK, attributeFOOUSAOCCodeErrorText.ZXE_Name, errorMessageFOO);

			Factory.Save();
			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_NSF;
			affirmationCodes = fda.AffirmationCodes;
			var uscAffirmCode1 = affirmationCodes.AddNew();

			AssertEquals(2, uscAffirmCode1.AOCList.Count);
			Assert(uscAffirmCode1.AOCList.ContainsCode(ACE_AffirmationOfComplianceList.Codes.STN));
			Assert(uscAffirmCode1.AOCList.ContainsCode(ACE_AffirmationOfComplianceList.Codes.DLS));
			Assert(!uscAffirmCode1.AOCList.ContainsCode(ACE_AffirmationOfComplianceList.Codes.VQI));

			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			affirmationCodes = fda.AffirmationCodes;
			var uscAffirmCode2 = affirmationCodes.AddNew();

			AssertEquals(1, uscAffirmCode2.AOCList.Count);
			Assert(uscAffirmCode2.AOCList.ContainsCode(ACE_AffirmationOfComplianceList.Codes.CAN));
			Assert(!uscAffirmCode2.AOCList.ContainsCode(ACE_AffirmationOfComplianceList.Codes.DA));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return AffirmationCodes.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<ACEAffirmationCode>();
		}

		protected override IEnumerable<ACEAffirmationCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var result = factory.NewWithValidTestData<ACEAffirmationCode>();

			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.AffirmationCodes.Add(result);

			yield return result;
		}

		ACEAffirmationCodeCollection AffirmationCodes
		{
			get
			{
				if (affirmationCodes == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
					var fda = invoiceLine.ACE_FDALines.AddNew();
					affirmationCodes = fda.AffirmationCodes;
				}
				return affirmationCodes;
			}
		}
		ACEAffirmationCodeCollection affirmationCodes;
	}
}
