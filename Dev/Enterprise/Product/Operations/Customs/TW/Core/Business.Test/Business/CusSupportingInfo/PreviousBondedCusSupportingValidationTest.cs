using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class PreviousBondedCusSupportingValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_LineNo()
		{
			SetUp();
			string messageError = MandatoryValidation.YouHaveNotEntered;
			previousBondedCusSupporting.CSI_LineNo = 1111;
			previousBondedCusSupporting.CSI_ReferenceNumber = ZString.Empty;
			previousBondedCusSupporting.CSI_LineNo = 0;
			AssertNoMessageErrorContaining(previousBondedCusSupporting.CSI_LineNoInfo, messageError);
			previousBondedCusSupporting.CSI_ReferenceNumber = "123465";
			previousBondedCusSupporting.CSI_LineNo = 0;
			AssertHasMessageErrorContaining(previousBondedCusSupporting.CSI_LineNoInfo, messageError);
			previousBondedCusSupporting.CSI_LineNo = 20;
			AssertNoMessageErrorContaining(previousBondedCusSupporting.CSI_LineNoInfo, messageError);
			AssertNoErrorContaining(previousBondedCusSupporting.CSI_LineNoInfo, MandatoryValidation.ValueCannotBeNegative);
			previousBondedCusSupporting.CSI_LineNo = -1;
			AssertHasErrorContaining(previousBondedCusSupporting.CSI_LineNoInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCheckCSI_ReferenceNumberLength()
		{
			SetUp();
			string messageError = ValidationConstants.PreviousBonded.PreviousBondedEntryNumberLength;
			previousBondedCusSupporting.CSI_ReferenceNumber = ZString.Empty;
			AssertNoMessageError(previousBondedCusSupporting.CSI_ReferenceNumberInfo, messageError);
			previousBondedCusSupporting.CSI_ReferenceNumber = "123456789";
			AssertHasMessageError(previousBondedCusSupporting.CSI_ReferenceNumberInfo, messageError);
			previousBondedCusSupporting.CSI_ReferenceNumber = "01234567890123";
			AssertNoMessageError(previousBondedCusSupporting.CSI_ReferenceNumberInfo, messageError);
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			SetUp();
			string messageError = MandatoryValidation.YouHaveNotEntered;
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			testOrg.MainAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.FTZ, "66666666", Core.Constants.CountryCodes.Taiwan);
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			entryInstruction.CEI_Style = ZString.Empty;
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.PreviousBondedEntryNumber = "XXX";
			invoiceLine.PreviousBondedEntryLineNumber = 0;
			invoiceLine.PreviousBondedEntryNumber = ZString.Empty;
			AssertNoMessageErrorContaining(previousBondedCusSupporting.CSI_ReferenceNumberInfo, messageError);
			foreach (var declarationType in new string[] { "B6", "F2", "D2", "D7", "B9", "F4", "D5" })
			{
				entryInstruction.CEI_Style = declarationType;
				invoiceLine.PreviousBondedEntryNumber = "XXX";
				invoiceLine.PreviousBondedEntryLineNumber = 6666;
				invoiceLine.PreviousBondedEntryNumber = ZString.Empty;
				AssertHasMessageErrorContaining(previousBondedCusSupporting.CSI_ReferenceNumberInfo, messageError);
				invoiceLine.PreviousBondedEntryLineNumber = 0;
				switch (declarationType)
				{
					case Constants.DeclarationTypes.Import.B6:
					case Constants.DeclarationTypes.Import.F2:
						jobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
						entryInstruction.CEI_Style = declarationType;
						invoiceLine.JI_Procedure = "EF";
						invoiceLine.PreviousBondedEntryNumber = "XXX";
						AssertNoMessageErrorContaining(previousBondedCusSupporting.CSI_ReferenceNumberInfo, messageError);
						entryInstruction.CEI_OA_Warehouse = testOrg.MainAddress.PK;
						invoiceLine.PreviousBondedEntryNumber = ZString.Empty;
						AssertHasMessageErrorContaining(previousBondedCusSupporting.CSI_ReferenceNumberInfo, messageError);
						invoiceLine.PreviousBondedEntryNumber = "XXX";
						AssertNoMessageErrorContaining(previousBondedCusSupporting.CSI_ReferenceNumberInfo, messageError);
						entryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
						invoiceLine.PreviousBondedEntryNumber = ZString.Empty;
						AssertNoMessageErrorContaining(previousBondedCusSupporting.CSI_ReferenceNumberInfo, messageError);
						break;
					case Constants.DeclarationTypes.Import.D2:
					case Constants.DeclarationTypes.Import.D7:
						jobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
						entryInstruction.CEI_Style = declarationType;
						invoiceLine.PreviousBondedEntryNumber = "XXX";
						AssertNoMessageErrorContaining(previousBondedCusSupporting.CSI_ReferenceNumberInfo, messageError);
						invoiceLine.PreviousBondedEntryNumber = ZString.Empty;
						AssertHasMessageErrorContaining(previousBondedCusSupporting.CSI_ReferenceNumberInfo, messageError);
						break;
					case Constants.DeclarationTypes.Export.B9:
					case Constants.DeclarationTypes.Export.F4:
						jobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
						entryInstruction.CEI_Style = declarationType;
						invoiceLine.JI_Procedure = "YZ";
						invoiceLine.PreviousBondedEntryNumber = "XXX";
						AssertNoMessageErrorContaining(previousBondedCusSupporting.CSI_ReferenceNumberInfo, messageError);
						entryInstruction.CEI_OA_Warehouse2 = testOrg.MainAddress.PK;
						invoiceLine.PreviousBondedEntryNumber = ZString.Empty;
						AssertHasMessageErrorContaining(previousBondedCusSupporting.CSI_ReferenceNumberInfo, messageError);
						invoiceLine.PreviousBondedEntryNumber = "XXX";
						AssertNoMessageErrorContaining(previousBondedCusSupporting.CSI_ReferenceNumberInfo, messageError);
						entryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
						invoiceLine.PreviousBondedEntryNumber = ZString.Empty;
						AssertNoMessageErrorContaining(previousBondedCusSupporting.CSI_ReferenceNumberInfo, messageError);
						break;
					case Constants.DeclarationTypes.Export.D5:
						jobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
						entryInstruction.CEI_Style = declarationType;
						entryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
						invoiceLine.PreviousBondedEntryNumber = "XXX";
						AssertNoMessageErrorContaining(previousBondedCusSupporting.CSI_ReferenceNumberInfo, messageError);
						invoiceLine.PreviousBondedEntryNumber = ZString.Empty;
						AssertHasMessageErrorContaining(previousBondedCusSupporting.CSI_ReferenceNumberInfo, messageError);
						invoiceLine.PreviousBondedEntryNumber = "XXX";
						entryInstruction.CEI_OA_Warehouse2 = testOrg.MainAddress.PK;
						invoiceLine.PreviousBondedEntryNumber = ZString.Empty;
						AssertNoMessageErrorContaining(previousBondedCusSupporting.CSI_ReferenceNumberInfo, messageError);
						break;
				}
			}
		}

		#region Implementation
		JobDeclaration jobDeclaration;
		JobComInvoiceLine invoiceLine;
		PreviousBondedCusSupporting previousBondedCusSupporting;
		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			invoiceLine = jobDeclaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			previousBondedCusSupporting = invoiceLine.PreviousBondedCusSupporting;
		}
		#endregion
	}
}
