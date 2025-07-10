using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	public class SupportingDocSendingObjectValidationTest : Customs.Business.Testing.SupportingDocSendingObjectValidationTest
	{
		public void TestCheckCaseNumber()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._11;
			var caseNumber = instruction.CaseNumbers.AddNew();
			caseNumber.CY_Data = "123";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "432";
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var mergedLine = entryHeader.MergedLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var line = invoiceHeader.InvoiceLines.AddNew();
			line.JI_CEI = instruction.PK;
			line.JI_CL = mergedLine.PK;
			var sendingObject = SupportingDocSendingObject.New(declaration);
			sendingObject.LocalReferenceNumber = "432";
			CombineAssertions(() =>
			{
				sendingObject.CaseNumber = ZString.Empty;
				AssertHasError("CaseNumber is mandatory", sendingObject.CaseNumberInfo, MandatoryValidation.MustBeEnteredMessage(sendingObject.CaseNumberInfo.HumanReadableName));
				sendingObject.CaseNumber = "A#1";
				AssertNoError("CaseNumber is mandatory", sendingObject.CaseNumberInfo, MandatoryValidation.MustBeEnteredMessage(sendingObject.CaseNumberInfo.HumanReadableName));
				AssertHasMessageError("CaseNumber must be alphanumeric", sendingObject.CaseNumberInfo, ValidationConstants.SupportingDocSendingObject.CaseNumberOnlyAlphanumeric);
				sendingObject.CaseNumber = "123ABC";
				AssertNoMessageError("CaseNumber must be alphanumeric", sendingObject.CaseNumberInfo, ValidationConstants.SupportingDocSendingObject.CaseNumberOnlyAlphanumeric);
				AssertHasErrorContaining(sendingObject.CaseNumberInfo, ListValidation.InvalidCodeError);
				sendingObject.CaseNumber = "123";
				AssertNoErrorContaining(sendingObject.CaseNumberInfo, ListValidation.InvalidCodeError);
			});
		}

		public override void TestCheckLocalReferenceNumber()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "123";
			var sendingObject = SupportingDocSendingObject.New(declaration);
			CombineAssertions(() =>
			{
				sendingObject.LocalReferenceNumber = ZString.Empty;
				AssertHasError("LocalReferenceNumber is mandatory", sendingObject.LocalReferenceNumberInfo, MandatoryValidation.MustBeEnteredMessage(sendingObject.LocalReferenceNumberInfo.HumanReadableName));
				sendingObject.LocalReferenceNumber = "AAA";
				AssertNoError("LocalReferenceNumber is mandatory", sendingObject.LocalReferenceNumberInfo, MandatoryValidation.MustBeEnteredMessage(sendingObject.LocalReferenceNumberInfo.HumanReadableName));
				AssertHasErrorContaining(sendingObject.LocalReferenceNumberInfo, ListValidation.InvalidCodeError);
				sendingObject.LocalReferenceNumber = "123";
				AssertNoErrorContaining(sendingObject.LocalReferenceNumberInfo, ListValidation.InvalidCodeError);
			});
		}

		public override void TestCheckDocumentType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("ZA"))
			{
				var testHelper = new ZAUniversalReferenceTestDataHelper(Factory);
				testHelper.CreateNewOrGetExistingCusCodeType("ZADOC", "doc");
				testHelper.CreateNewOrGetExistingCusCodeList("ZA", "ZADOC", "INV", "Invoice", ZDateTime.UtcNow.AddDays(-2), ZDateTime.UtcNow.AddDays(2));
				Factory.Save();
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				var sendingObject = SupportingDocSendingObject.New(declaration);
				CombineAssertions(() =>
				{
					sendingObject.Validation.ValidateDocumentType();
					AssertHasError("DocumentType is mandatory", sendingObject.DocumentTypeInfo, MandatoryValidation.MustBeEnteredMessage(sendingObject.DocumentTypeInfo.HumanReadableName));
					sendingObject.DocumentType = "AAA";
					AssertNoError("DocumentType is mandatory", sendingObject.DocumentTypeInfo, MandatoryValidation.MustBeEnteredMessage(sendingObject.DocumentTypeInfo.HumanReadableName));
					AssertHasErrorContaining(sendingObject.DocumentTypeInfo, ListValidation.InvalidCodeError);
					sendingObject.DocumentType = "INV";
					AssertNoErrorContaining(sendingObject.DocumentTypeInfo, ListValidation.InvalidCodeError);
				});
			}
		}
	}
}
