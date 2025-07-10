using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USExportNMFSAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_ProgramCode()
		{
			ExportNMFS.AddInfoValidation.ValidateUS_ProgramType();
			AssertHasMessageErrorContaining(ExportNMFS.US_ProgramTypeInfo, MandatoryValidation.YouHaveNotEntered);

			ExportNMFS.US_ProgramType = "~";
			AssertNoMessageErrorContaining(ExportNMFS.US_ProgramTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_ProcessingType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);

			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NMFSCategoryCode, "NMFSCategoryCode", dataGrouping.ZZZ_DataGrouping);
			var code = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "BBF", "Baitboat", startDate, endDate);
			Factory.Save();

			ExportNMFS.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			ExportNMFS.AddInfoValidation.ValidateUS_ProcessingType();
			AssertHasMessageErrorContaining(ExportNMFS.US_ProcessingTypeInfo, MandatoryValidation.YouHaveNotEntered);

			ExportNMFS.US_ProcessingType = NMFSProductCategoryCodeList.Codes.Dressed;
			AssertNoMessageErrorContaining(ExportNMFS.US_ProcessingTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(ExportNMFS.US_ProcessingTypeInfo, ListValidation.InvalidCodeMessageError);

			ExportNMFS.US_ProcessingType = "BBF";
			AssertNoMessageErrorContaining(ExportNMFS.US_ProcessingTypeInfo, ListValidation.InvalidCodeMessageError);

			ExportNMFS.US_ProgramType = ZString.Empty;
			ExportNMFS.US_ProcessingType = ZString.Empty;
			AssertNoMessageErrorContaining(ExportNMFS.US_ProcessingTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_DocumentType()
		{
			ExportNMFS.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			ExportNMFS.AddInfoValidation.ValidateUS_DocumentType();
			AssertHasMessageErrorContaining(ExportNMFS.US_DocumentTypeInfo, MandatoryValidation.YouHaveNotEntered);

			ExportNMFS.US_DocumentType = "~";
			AssertNoMessageErrorContaining(ExportNMFS.US_DocumentTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(ExportNMFS.US_DocumentTypeInfo, ListValidation.InvalidCodeMessageError);

			ExportNMFS.US_DocumentType = NMFSHMSDocumentIdentifierList.Codes.BluefinTunaCatchDocument;
			AssertNoMessageErrorContaining(ExportNMFS.US_DocumentTypeInfo, ListValidation.InvalidCodeMessageError);

			ExportNMFS.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			ExportNMFS.US_DocumentType = ZString.Empty;
			AssertHasMessageErrorContaining(ExportNMFS.US_DocumentTypeInfo, MandatoryValidation.YouHaveNotEntered);

			ExportNMFS.US_DocumentType = "~";
			AssertNoMessageErrorContaining(ExportNMFS.US_DocumentTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(ExportNMFS.US_DocumentTypeInfo, ListValidation.InvalidCodeMessageError);

			ExportNMFS.US_DocumentType = NMFSAMRDocumentIdentifierList.Codes.DissostichusCatchDocument;
			AssertNoMessageErrorContaining(ExportNMFS.US_DocumentTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_DocumentNumber()
		{
			ExportNMFS.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			ExportNMFS.AddInfoValidation.ValidateUS_DocumentType();
			AssertHasMessageErrorContaining(ExportNMFS.US_DocumentTypeInfo, MandatoryValidation.YouHaveNotEntered);

			ExportNMFS.US_DocumentType = "~";
			AssertNoMessageErrorContaining(ExportNMFS.US_DocumentTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_Quantity()
		{
			ExportNMFS.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			ExportNMFS.AddInfoValidation.ValidateUS_PreApprovalIssuedQuantity();
			AssertHasMessageErrorContaining(ExportNMFS.US_QuantityInfo, MandatoryValidation.YouHaveNotEntered);

			ExportNMFS.US_Quantity = 123m;
			AssertNoMessageErrorContaining(ExportNMFS.US_QuantityInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_UQ()
		{
			ExportNMFS.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			ExportNMFS.AddInfoValidation.ValidateUS_PreApprovalIssuedQuantityUQ();
			AssertHasMessageErrorContaining(ExportNMFS.US_UnitOfMeasureInfo, MandatoryValidation.YouHaveNotEntered);

			ExportNMFS.US_UnitOfMeasure = "~";
			AssertNoMessageErrorContaining(ExportNMFS.US_UnitOfMeasureInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(ExportNMFS.US_UnitOfMeasureInfo, ListValidation.InvalidCodeMessageError);

			ExportNMFS.US_UnitOfMeasure = "KG";
			AssertNoMessageErrorContaining(ExportNMFS.US_UnitOfMeasureInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_CatchDocument()
		{
			ExportNMFS.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			ExportNMFS.AddInfoValidation.ValidateUS_CatchDocument();
			AssertHasMessageErrorContaining(ExportNMFS.US_CatchDocumentInfo, MandatoryValidation.YouHaveNotEntered);

			ExportNMFS.US_CatchDocument = "~";
			AssertNoMessageErrorContaining(ExportNMFS.US_CatchDocumentInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_ReExportNumber()
		{
			ExportNMFS.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			ExportNMFS.AddInfoValidation.ValidateUS_ReExportNumber();
			AssertHasMessageErrorContaining(ExportNMFS.US_ReExportNumberInfo, MandatoryValidation.YouHaveNotEntered);

			ExportNMFS.US_ReExportNumber = "~";
			AssertNoMessageErrorContaining(ExportNMFS.US_ReExportNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestValidationDoesNotCreateNewHarvestingDetails()
		{
			AssertEquals("(pre-condition)", 0, ExportNMFS.HarvestingDetails.Count);
			ExportNMFS.AddInfoValidation.ValidateAll();
			AssertEquals(0, ExportNMFS.HarvestingDetails.Count);
		}

		#region Implementation

		NMFSLine ExportNMFS
		{
			get
			{
				if (exportNMFS == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.InvoiceLines.AddNew();
					invoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Declared;
					exportNMFS = invoiceLine.NMFSLines.AddNew();
				}

				return exportNMFS;
			}
		}
		NMFSLine exportNMFS;

		#endregion
	}
}
