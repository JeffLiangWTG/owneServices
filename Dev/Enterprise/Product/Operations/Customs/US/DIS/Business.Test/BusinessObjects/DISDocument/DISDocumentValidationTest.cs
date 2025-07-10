using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class DISDocumentValidationTest : TestCaseWithFactory
	{
		public void TestValidateRequiredDocumentPK()
		{
			var jobDeclaration = (IUSDISHost)JobDeclaration;
			var requiredDocument = jobDeclaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var disDocument = HostWrapper.DISDocuments.AddNew();
			disDocument.RequiredDocumentPK = ZGuid.Empty;
			AssertHasErrorContaining(disDocument.RequiredDocumentPKInfo, MandatoryValidation.MustBeEntered);
			disDocument.RequiredDocumentPK = requiredDocument.PK;
			AssertNoErrorContaining(disDocument.RequiredDocumentPKInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestValidateDocumentLabel()
		{
			var helper = new DISUniversalReferenceHelper(Factory);
			helper.CreateDisCodeEntry("APH01", "APH_STAT");
			Factory.Save();
			var jobDeclaration = (IUSDISHost)JobDeclaration;
			var disDocument = HostWrapper.DISDocuments.AddNew();
			disDocument.DocumentLabel = ZString.Empty;
			AssertHasMessageErrorContaining(disDocument.DocumentLabelInfo, MandatoryValidation.YouHaveNotEntered);
			disDocument.DocumentLabel = "~~~";
			AssertNoMessageErrorContaining(disDocument.DocumentLabelInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(disDocument.DocumentLabelInfo, ListValidation.InvalidCodeMessageError);
			disDocument.DocumentLabel = "APH01";
			AssertNoMessageErrorContaining(disDocument.DocumentLabelInfo, ListValidation.InvalidCodeMessageError);
			Assert("Precondition: PGAs have been defaulted", disDocument.PGAs.Count > 0);
			AssertNoMessageErrorContaining(disDocument.DocumentLabelInfo, DISDocumentValidation.NoPGAsSelected);
			disDocument.PGAs.RemoveAll();
			AssertHasMessageErrorContaining(disDocument.DocumentLabelInfo, DISDocumentValidation.NoPGAsSelected);
		}

		public void TestValidateDocumentLabel_OldCode()
		{
			var helper = new DISUniversalReferenceHelper(Factory);
			helper.CreateDisCodeEntry("EPA05", "EPA_V_E_EXEMPTION");
			Factory.Save();
			var jobDeclaration = (IUSDISHost)JobDeclaration;
			var disDocument = HostWrapper.DISDocuments.AddNew();
			disDocument.DocumentLabel = ZString.Empty;
			AssertHasMessageErrorContaining(disDocument.DocumentLabelInfo, MandatoryValidation.YouHaveNotEntered);
			disDocument.DocumentLabel = "~~~";
			AssertNoMessageErrorContaining(disDocument.DocumentLabelInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(disDocument.DocumentLabelInfo, ListValidation.InvalidCodeMessageError);
			disDocument.DocumentLabel = "EPA05";
			AssertNoMessageErrorContaining(disDocument.DocumentLabelInfo, ListValidation.InvalidCodeMessageError);
		}

		[TestDate(2016, 10, 5)]
		public void TestValidateDocumentLabelWithDeleteFormType()
		{
			var cBP38DeleteEffectiveDate = new ZDateTime(2016, 8, 20);
			var cBP3134DeleteEffectiveDate = new ZDateTime(2016, 8, 27);
			var cBP109NMF07NMF21DeleteEffectiveDate = new ZDateTime(2016, 10, 5);
			var cBP44DeleteCBC0102AddedEffectiveDate = new ZDateTime(2016, 10, 31);
			string effectiveDateForCBP3134 = $" (effective {cBP3134DeleteEffectiveDate.ToBestReadableDateString()}).";
			string effectiveDateForCBP38 = $" (effective {cBP38DeleteEffectiveDate.ToBestReadableDateString()}).";
			string effectiveDateForCBP109NMF07NMF21 = $" (effective {cBP109NMF07NMF21DeleteEffectiveDate.ToBestReadableDateString()}).";
			string effectiveDateForCBP44CBC0102 = $" (effective {cBP44DeleteCBC0102AddedEffectiveDate.ToBestReadableDateString()}).";
			var helper = new DISUniversalReferenceHelper(Factory);
			helper.CreateDisCodeEntry("APH01", "APH_STAT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateDisCodeEntry("CBP31", "PROTEST", ZDateTime.MinSmallDateTimeValue, cBP3134DeleteEffectiveDate);
			helper.CreateDisCodeEntry("CBP34", "520D", ZDateTime.MinSmallDateTimeValue, cBP3134DeleteEffectiveDate);
			helper.CreateDisCodeEntry("CBP109", "XXX", ZDateTime.MinSmallDateTimeValue, cBP109NMF07NMF21DeleteEffectiveDate);
			helper.CreateDisCodeEntry("NMF07", "NMF_HIGHLY_MIGRATORY_SPECIES_INTL_TRADE", ZDateTime.MinSmallDateTimeValue, cBP109NMF07NMF21DeleteEffectiveDate);
			helper.CreateDisCodeEntry("NMF21", "NMF_AMLR_DEALER", ZDateTime.MinSmallDateTimeValue, cBP109NMF07NMF21DeleteEffectiveDate);
			helper.CreateDisCodeEntry("CBP38", "STB_TYPE_1", ZDateTime.MinSmallDateTimeValue, cBP38DeleteEffectiveDate);
			helper.CreateDisCodeEntry("CBP44", "STB_TYPE_1", ZDateTime.MinSmallDateTimeValue, cBP44DeleteCBC0102AddedEffectiveDate);
			helper.CreateDisCodeEntry("CBC01", "STB_TYPE_1", cBP44DeleteCBC0102AddedEffectiveDate, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateDisCodeEntry("CBC02", "STB_TYPE_1", cBP44DeleteCBC0102AddedEffectiveDate, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateDisCodeEntry("XXX01", "DUMMY_CODE_EXPIRED", ZDateTime.MinSmallDateTimeValue, cBP38DeleteEffectiveDate);
			var replacedCode = helper.DataHelper.CreateNewOrGetExistingCusCodeList("US", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USDISFormList, "XXX01", "DUMMY_CODE_REPLACED", cBP38DeleteEffectiveDate.AddDays(1), ZDateTime.MaxSmallDateTimeValue);
			helper.DataHelper.CreateNewOrGetExistingCusCodeListAttribute(replacedCode.PK, "USDISFormGroup", "NOGROUP");
			Factory.Save();
			var jobDeclaration = (IUSDISHost)JobDeclaration;
			var disDocument = HostWrapper.DISDocuments.AddNew();
			disDocument.DocumentLabel = ZString.Empty;
			AssertHasMessageErrorContaining(disDocument.DocumentLabelInfo, MandatoryValidation.YouHaveNotEntered);
			var errMsg3134 = DISDocumentValidation.DocTypeIsDeleted + effectiveDateForCBP3134;
			disDocument.DocumentLabel = "CBP31";
			AssertHasMessageErrorContaining(disDocument.DocumentLabelInfo, errMsg3134);
			disDocument.DocumentLabel = "CBP34";
			AssertHasMessageErrorContaining(disDocument.DocumentLabelInfo, errMsg3134);
			var errMsg38 = DISDocumentValidation.DocTypeIsDeleted + effectiveDateForCBP38;
			disDocument.DocumentLabel = "CBP38";
			AssertHasMessageErrorContaining(disDocument.DocumentLabelInfo, errMsg38);
			var errMsgMF0721CBP109 = DISDocumentValidation.DocTypeIsDeleted + effectiveDateForCBP109NMF07NMF21;
			disDocument.DocumentLabel = "CBP109";
			AssertHasMessageErrorContaining(disDocument.DocumentLabelInfo, errMsgMF0721CBP109);
			disDocument.DocumentLabel = "NMF07";
			AssertHasMessageErrorContaining(disDocument.DocumentLabelInfo, errMsgMF0721CBP109);
			disDocument.DocumentLabel = "NMF21";
			AssertHasMessageErrorContaining(disDocument.DocumentLabelInfo, errMsgMF0721CBP109);
			disDocument.DocumentLabel = "CBP44";
			Assert(!disDocument.DocumentLabelInfo.HasMessageErrors());
			var errMsgCBC0102 = DISDocumentValidation.DocTypeIsUnreleased + effectiveDateForCBP44CBC0102;
			disDocument.DocumentLabel = "CBC01";
			AssertHasMessageErrorContaining(disDocument.DocumentLabelInfo, errMsgCBC0102);
			disDocument.DocumentLabel = "CBC02";
			AssertHasMessageErrorContaining(disDocument.DocumentLabelInfo, errMsgCBC0102);
			disDocument.DocumentLabel = "XXX01";
			Assert("Validation checked " + disDocument.DocumentLabelUSDISDocCode, !disDocument.DocumentLabelInfo.HasMessageErrors());
		}

		public void TestCheckDocumentDescription()
		{
			var message = string.Format(DISDocumentValidation.DataHasInvalidCharacters, DISDocument.Schema.DocumentDescription);
			var jobDeclaration = (IUSDISHost)JobDeclaration;
			var requiredDocument = jobDeclaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var disDocument = HostWrapper.DISDocuments.AddNew();
			disDocument.DocumentDescription = "Where are you D%?";
			disDocument.Validation.ValidateDocumentDescription();
			AssertNoWarningContaining(disDocument.DocumentDescriptionInfo, message);
			disDocument.DocumentDescription = "Where are you Dㅇ?";
			disDocument.Validation.ValidateDocumentDescription();
			AssertHasWarningContaining(disDocument.DocumentDescriptionInfo, message);
		}

		public void TestCheckComment()
		{
			var message = string.Format(DISDocumentValidation.DataHasInvalidCharacters, DISDocument.Schema.Comment);
			var jobDeclaration = (IUSDISHost)JobDeclaration;
			var requiredDocument = jobDeclaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var disDocument = HostWrapper.DISDocuments.AddNew();
			disDocument.Comment = "Where are you D%?";
			disDocument.Validation.ValidateComment();
			AssertNoWarningContaining(disDocument.CommentInfo, message);
			disDocument.Comment = "Where are you Dㅇ?";
			disDocument.Validation.ValidateComment();
			AssertHasWarningContaining(disDocument.CommentInfo, message);
		}

		public void TestRegexWorks()
		{
			var jobDeclaration = (IUSDISHost)JobDeclaration;
			var requiredDocument = jobDeclaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var disDocument = HostWrapper.DISDocuments.AddNew();
			var fileName = "FileNAME";
			AssertEquals("filename looks good", false, DISDocumentValidation.HasDocFileNameInvalidCharacters(fileName));
			fileName = "fileNAME%&";
			AssertEquals("Has Invalid characters for FileName", true, DISDocumentValidation.HasDocFileNameInvalidCharacters(fileName));
			var elementValue = "Everybody is?`~¢ready.Yay";
			AssertEquals("Test value looks good", false, DISDocumentValidation.HasDocFileNameInvalidCharacters(elementValue));
			elementValue = "Everybody is?`~¢ready.Yayㅇ";
			AssertEquals("Has Invalid characters", true, DISDocumentValidation.HasDocFileNameInvalidCharacters(elementValue));
		}

		public void TestCheckEDocsDocumentPK()
		{
			var declaration = (IUSDISHost)JobDeclaration;
			var docManagerSupport = (IDocManagerSupport)declaration;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(JobDeclaration, "TST");
			var eDocs = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "ABC", false);
			var hostWrapper = new DISHostWrapper(declaration);
			var disDocument = hostWrapper.DISDocuments.AddNew();
			disDocument.EDocsDocumentPK = ZGuid.Empty;
			AssertHasMessageErrorContaining(disDocument.EDocsDocumentPKInfo, MandatoryValidation.YouHaveNotEntered);
			disDocument.EDocsDocumentPK = ZGuid.NewZGuid();
			AssertNoMessageErrorContaining(disDocument.EDocsDocumentPKInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(disDocument.EDocsDocumentPKInfo, DISDocumentValidation.SelectValidEDocs);
			disDocument.EDocsDocumentPK = eDocs.UniqueKey;
			AssertNoMessageErrorContaining(disDocument.EDocsDocumentPKInfo, DISDocumentValidation.SelectValidEDocs);
		}

		public void TestCheckEDocsDocumentPKWithInvalidCharaters()
		{
			var declaration = (IUSDISHost)JobDeclaration;
			var docManagerSupport = (IDocManagerSupport)declaration;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(JobDeclaration, "TST");
			var eDocs1 = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "ABC", false);
			var eDocs2 = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC×.pdf", "ABC", false);
			var eDocs3 = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC&.pdf", "ABC", false);
			var hostWrapper = new DISHostWrapper(declaration);
			var disDocument = hostWrapper.DISDocuments.AddNew();
			disDocument.EDocsDocumentPK = eDocs1.UniqueKey;
			AssertNoWarningContaining(disDocument.EDocsDocumentPKInfo, DISDocumentValidation.FileNameHasInvalidCharacters);
			disDocument.EDocsDocumentPK = eDocs2.UniqueKey;
			AssertHasWarningContaining(disDocument.EDocsDocumentPKInfo, DISDocumentValidation.FileNameHasInvalidCharacters);
			disDocument.EDocsDocumentPK = eDocs3.UniqueKey;
			AssertHasWarningContaining(disDocument.EDocsDocumentPKInfo, DISDocumentValidation.FileNameHasInvalidCharacters);
		}

		public void TestCheckEDocsDocumentFormat()
		{
			var declaration = (IUSDISHost)JobDeclaration;
			var docManagerSupport = (IDocManagerSupport)declaration;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(JobDeclaration, "TST");
			var eDocs1 = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "ABC", false);
			var eDocs2 = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.txt", "ABC", false);
			var eDocs3 = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.xls", "ABC", false);
			var eDocs4 = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.xlsx", "ABC", false);
			var hostWrapper = new DISHostWrapper(declaration);
			var disDocument = hostWrapper.DISDocuments.AddNew();
			disDocument.EDocsDocumentPK = eDocs1.UniqueKey;
			AssertNoError(disDocument.EDocsDocumentPKInfo, DISDocumentValidation.FileIsAnInvalidFormat);
			disDocument.EDocsDocumentPK = eDocs2.UniqueKey;
			AssertHasError(disDocument.EDocsDocumentPKInfo, DISDocumentValidation.FileIsAnInvalidFormat);
			disDocument.EDocsDocumentPK = eDocs3.UniqueKey;
			AssertNoError(disDocument.EDocsDocumentPKInfo, DISDocumentValidation.FileIsAnInvalidFormat);
			disDocument.EDocsDocumentPK = eDocs4.UniqueKey;
			AssertNoError(disDocument.EDocsDocumentPKInfo, DISDocumentValidation.FileIsAnInvalidFormat);
		}

		public void TestCheckEDocsDocumentFormatKimberleyCertificate()
		{
			var helper = new DISUniversalReferenceHelper(Factory);
			helper.CreateDisCodeEntry("APH01", "APH_STAT");
			var cbc01 = helper.CreateDisCodeEntry("CBC01", "APH_STAT");
			helper.DataHelper.CreateNewOrGetExistingCusCodeListAttribute(cbc01.PK, RefCusCodeListAttributeTypes.Codes.USDISSupportedFileTypes, "PDF");
			var cbc02 = helper.CreateDisCodeEntry("CBC02", "APH_STAT");
			helper.DataHelper.CreateNewOrGetExistingCusCodeListAttribute(cbc02.PK, RefCusCodeListAttributeTypes.Codes.USDISSupportedFileTypes, "PDF");
			helper.DataHelper.CreateNewOrGetExistingCusCodeListAttribute(cbc02.PK, RefCusCodeListAttributeTypes.Codes.USDISSupportedFileTypes, "DOC");
			Factory.Save();
			var declaration = (IUSDISHost)JobDeclaration;
			var docManagerSupport = (IDocManagerSupport)declaration;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(JobDeclaration, "TST");
			var eDocsPdf = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "ABC", false);
			var eDocsDoc = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.doc", "ABC", false);
			var eDocsPpt = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.ppt", "ABC", false);
			var pdfErrorMessage = DISDocumentValidation.FileIsAnExcludedFormat("pdf");
			var hostWrapper = new DISHostWrapper(declaration);
			var disDocument = hostWrapper.DISDocuments.AddNew();
			disDocument.DocumentLabel = "CBC01";
			disDocument.EDocsDocumentPK = eDocsPdf.UniqueKey;
			AssertNoMessageError(disDocument.EDocsDocumentPKInfo, pdfErrorMessage);
			disDocument.EDocsDocumentPK = eDocsDoc.UniqueKey;
			AssertHasMessageError(disDocument.EDocsDocumentPKInfo, pdfErrorMessage);
			disDocument.DocumentLabel = "CBC02";
			AssertNoMessageErrors(disDocument.EDocsDocumentPKInfo);
			var pdfDocErrorMessage = DISDocumentValidation.FileIsAnExcludedFormat("doc, pdf");
			disDocument.EDocsDocumentPK = eDocsPpt.UniqueKey;
			AssertHasMessageError(disDocument.EDocsDocumentPKInfo, pdfDocErrorMessage);
			disDocument.EDocsDocumentPK = eDocsPdf.UniqueKey;
			AssertNoMessageError(disDocument.EDocsDocumentPKInfo, pdfDocErrorMessage);
			disDocument.DocumentLabel = "APH01";
			disDocument.Validation.ValidateAll();
			AssertNoMessageErrors(disDocument.EDocsDocumentPKInfo);
		}

		public void TestComment_MaxLength()
		{
			var disDocument = HostWrapper.DISDocuments.AddNew();
			AssertNoExceptionThrown(() => disDocument.Comment = ZString.Replicate('X', 100));
			AssertExceptionThrown<MaxLengthExceededException>(() => disDocument.Comment += 'X');
			ErrorReporter.Clear();
		}

		public void TestCheckShipmentNo()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var defaultValues = new Mock<IUSDISDefaultValues>();
			defaultValues.Setup(m => m.IsExport).Returns(true);
			var tradeTransaction1 = new Mock<IDISTradeTransaction>();
			tradeTransaction1.Setup(m => m.ShipmentNo).Returns(new ZString("001"));
			tradeTransaction1.Setup(m => m.Number).Returns(new ZString("123567"));
			tradeTransaction1.Setup(m => m.XTN).Returns(new ZString("SV9-1234567"));
			var tradeTransaction2 = new Mock<IDISTradeTransaction>();
			tradeTransaction2.Setup(m => m.ShipmentNo).Returns(new ZString("002"));
			tradeTransaction2.Setup(m => m.XTN).Returns(new ZString("SV9-222222"));
			defaultValues.Setup(m => m.DefaultTradeTransactions).Returns(new IDISTradeTransaction[] { tradeTransaction1.Object, tradeTransaction2.Object });
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			disDocument.ShipmentNo = "003";
			AssertHasMessageErrorContaining(disDocument.ShipmentNoInfo, "The code you have selected is not in the list");
			disDocument.ShipmentNo = "001";
			AssertNoMessageErrorContaining(disDocument.ShipmentNoInfo, "The code you have selected is not in the list");
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
			tradeTransaction1.VerifyAll();
			tradeTransaction2.VerifyAll();
		}

		DISHostWrapper hostWrapper;
		DISHostWrapper HostWrapper => hostWrapper ?? (hostWrapper = new DISHostWrapper((IUSDISHost)JobDeclaration));

		BusinessObject jobDeclaration;
		BusinessObject JobDeclaration => jobDeclaration ?? (jobDeclaration = new TestHelper(Factory).GetJobDeclaration());
	}
}
