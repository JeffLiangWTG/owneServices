using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US.DIS;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.MasterFiles.Integration;
using Moq;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class DISDocumentWrapperTest : TestCaseWithFactory
	{
		public void TestSimpleProperties()
		{
			var usDISHost = GetMockDISHost();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			usDISHost.Setup(m => m.JobNumber).Returns(new ZString("B00001000"));
			usDISHost.Setup(m => m.EDocs).Returns(System.Array.Empty<IeDoc>());
			var defaultValues = new Mock<IUSDISDefaultValues>();
			defaultValues.Setup(m => m.DefaultCBPRequests).Returns(System.Array.Empty<IDISCBPRequestDefault>());
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			disDocument.IDSuffix = 2;
			disDocument.Comment = "Comment";
			disDocument.DocumentLabel = "APH01";
			disDocument.DocumentDescription = "desc";
			disDocument.SubmitDateUTC = new ZDateTime(2013, 12, 25, 12, 25, 25);
			disDocument.Status = StatusList.Codes.AOS;
			disDocument.PGAs.AddNew().Code = "FDA";
			disDocument.PGAs.AddNew().Code = "FCC";
			var addInfo = disDocument.AdditionalData.AddNew();
			addInfo.Name = "Invoice Amount";
			addInfo.Data = "31267.90";
			disDocument.CBPRequest.Type = CBPRequestTypeList.Codes.ACEActionNumber;
			disDocument.CBPRequest.ID = "324890";
			disDocument.CBPRequest.RequestDate = ZDateTime.BrettsBirthday;
			var docWrapper = (IDISDocument)new DISDocumentWrapper(disDocument);
			AssertEquals("B00001000_DIS2", docWrapper.DocumentID);
			AssertEquals("Comment", docWrapper.Comment);
			AssertEquals("APH01", docWrapper.DocumentLabel);
			AssertEquals("desc", docWrapper.DocumentDescription);
			AssertEquals(new ZDateTime(2013, 12, 25, 7, 25, 25), docWrapper.DocumentSentDateEST);
			AssertEquals(false, docWrapper.PreviouslySubmitted);
			AssertEquals(true, docWrapper.PGAs.Contains("FDA"));
			AssertEquals(true, docWrapper.PGAs.Contains("FCC"));
			AssertEquals(false, docWrapper.PGAs.Contains("DOT"));
			AssertEquals(1, docWrapper.AdditionalData.Count());
			AssertEquals("Invoice Amount", docWrapper.AdditionalData.ElementAt(0).FieldName);
			AssertEquals(CBPRequestTypeList.Codes.ACEActionNumber, docWrapper.CBPRequest.Type);
			AssertEquals("324890", docWrapper.CBPRequest.ID);
			AssertEquals(ZDateTime.BrettsBirthday, docWrapper.CBPRequest.RequestDate);
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
		}

		public void TestFileName()
		{
			var usDISHost = GetMockDISHost();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			usDISHost.Setup(m => m.JobNumber).Returns(new ZString("B00001000"));
			usDISHost.Setup(m => m.RequiredDocumentsProvider).Returns(((IDocsAndCartageParent)new TestHelper(Factory).GetJobDeclaration()).RequiredDocumentsProvider);
			var eDoc1 = new Mock<IeDoc>();
			var pk1 = ZGuid.NewZGuid();
			eDoc1.Setup(m => m.UniqueKey).Returns(pk1);
			eDoc1.Setup(m => m.FileName).Returns(new ZString("test.txt"));
			eDoc1.Setup(m => m.IsDeleted).Returns(ZBool.False);
			eDoc1.Setup(m => m.DocType).Returns(new ZString("AAA"));
			eDoc1.Setup(m => m.DateAdded).Returns(ZDateTime.BrettsBirthday);
			eDoc1.Setup(m => m.Description).Returns(new ZString("AAA"));
			var eDoc2 = new Mock<IeDoc>();
			var pk2 = ZGuid.NewZGuid();
			eDoc2.Setup(m => m.UniqueKey).Returns(pk2);
			eDoc2.Setup(m => m.FileName).Returns(new ZString("1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890.txt"));
			eDoc2.Setup(m => m.FileNameOnly).Returns(new ZString("1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890"));
			eDoc2.Setup(m => m.IsDeleted).Returns(ZBool.False);
			eDoc2.Setup(m => m.DocType).Returns(new ZString("AAB"));
			eDoc2.Setup(m => m.DateAdded).Returns(ZDateTime.BrettsBirthday);
			eDoc2.Setup(m => m.Description).Returns(new ZString("AAA"));
			usDISHost.Setup(m => m.EDocs).Returns(new IeDoc[] { eDoc1.Object, eDoc2.Object });
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			disDocument.IDSuffix = 1;
			disDocument.EDocsDocumentPK = pk2;
			var docWrapper = (IDISDocument)new DISDocumentWrapper(disDocument);
			AssertEquals("B00001000_DIS1-123456789012345678901234567890123456789012345678901234567890123456789012345678901.txt", docWrapper.CompleteFileName);
			AssertEquals(100, docWrapper.CompleteFileName.Length);
			AssertEquals(pk2, docWrapper.eDocsDocumentPK);
			usDISHost.VerifyAll();
			eDoc1.VerifyAll();
			eDoc2.VerifyAll();
		}

		public void TestFileName_ReplacementMessage()
		{
			var usDISHost = GetMockDISHost();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			usDISHost.Setup(m => m.JobNumber).Returns(new ZString("B00001000"));
			usDISHost.Setup(m => m.RequiredDocumentsProvider).Returns(((IDocsAndCartageParent)new TestHelper(Factory).GetJobDeclaration()).RequiredDocumentsProvider);
			var defaultValues = new Mock<IUSDISDefaultValues>();
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var eDoc1 = new Mock<IeDoc>();
			var pk1 = ZGuid.NewZGuid();
			eDoc1.Setup(m => m.UniqueKey).Returns(pk1);
			eDoc1.Setup(m => m.FileName).Returns(new ZString("test.txt"));
			eDoc1.Setup(m => m.FileNameOnly).Returns(new ZString("test"));
			eDoc1.Setup(m => m.IsDeleted).Returns(ZBool.False);
			eDoc1.Setup(m => m.DocType).Returns(new ZString("AAA"));
			eDoc1.Setup(m => m.DateAdded).Returns(ZDateTime.BrettsBirthday);
			eDoc1.Setup(m => m.Description).Returns(new ZString("AAA"));
			usDISHost.Setup(m => m.EDocs).Returns(new IeDoc[] { eDoc1.Object });
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			disDocument.IDSuffix = 1;
			disDocument.EDocsDocumentPK = pk1;
			var docWrapper = (IDISDocument)new DISDocumentWrapper(disDocument);
			AssertEquals("B00001000_DIS1-test.txt", docWrapper.CompleteFileName);
			var ediMessage = Factory.New<EDIMessage>();
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			ediMessage.EM_MessageText = TestHelper.DocumentSubmissionXml;
			disDocument.Status = StatusList.Codes.COS;
			disDocument.Messages.Add(ediMessage);
			AssertEquals("B00001000_DIS1.pdf", docWrapper.CompleteFileName);
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
			eDoc1.VerifyAll();
		}

		public void TestCommodityLineAndInvoiceData()
		{
			var helper = new DISUniversalReferenceHelper(Factory);
			var codeEPA01 = helper.CreateDisCodeEntry("EPA01", "EPA01");
			helper.DataHelper.CreateNewOrGetExistingCusCodeListAttribute(codeEPA01.PK, RefCusCodeListAttributeTypes.Codes.OptionalData, OptionalDataTypeCodes.Commodity);
			var codeCBP02 = helper.CreateDisCodeEntry("CBP02", "CBP02");
			helper.DataHelper.CreateNewOrGetExistingCusCodeListAttribute(codeCBP02.PK, RefCusCodeListAttributeTypes.Codes.OptionalData, OptionalDataTypeCodes.Invoice);
			Factory.Save();
			var usDISHost = GetMockDISHost();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			usDISHost.Setup(m => m.EDocs).Returns(System.Array.Empty<IeDoc>());
			var defaultValues = new Mock<IUSDISDefaultValues>();
			var invoiceMock = new Mock<ICommercialInvoiceDefault>();
			invoiceMock.Setup(m => m.InvoiceNumber).Returns(new ZString("423987432"));
			invoiceMock.Setup(m => m.Description).Returns(new ZString("Blah"));
			var invoiceLineMock = new Mock<IDISInvoiceLineDefault>();
			invoiceLineMock.Setup(m => m.InvoiceLineNumber).Returns(new ZInt(1));
			invoiceLineMock.Setup(m => m.CommodityDetails).Returns((IDISCommodityLine)null);
			invoiceLineMock.Setup(m => m.CommodityDescription).Returns(new ZString("1"));
			var invoiceLineMock2 = new Mock<IDISInvoiceLineDefault>();
			invoiceLineMock2.Setup(m => m.InvoiceLineNumber).Returns(new ZInt(2));
			invoiceLineMock2.Setup(m => m.CommodityDetails).Returns((IDISCommodityLine)null);
			var invoiceLineMock3 = new Mock<IDISInvoiceLineDefault>();
			invoiceLineMock3.Setup(m => m.InvoiceLineNumber).Returns(new ZInt(3));
			invoiceLineMock3.Setup(m => m.CommodityDetails).Returns((IDISCommodityLine)null);
			invoiceLineMock3.Setup(m => m.CommodityDescription).Returns(new ZString("3"));
			invoiceMock.Setup(m => m.InvoiceLines).Returns(new IDISInvoiceLineDefault[] { invoiceLineMock.Object, invoiceLineMock2.Object, invoiceLineMock3.Object });
			defaultValues.Setup(m => m.DefaultInvoiceData).Returns(new ICommercialInvoiceDefault[] { invoiceMock.Object });
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			disDocument.DocumentLabel = "EPA01";
			Assert("PreCondition", disDocument.CommodityDataVisible);
			var commodityLine = disDocument.CommodityData.AddNew();
			commodityLine.InvoiceNumber = "423987432";
			commodityLine.InvoiceLineNumber = 1;
			var commodityLine2 = disDocument.CommodityData.AddNew();
			commodityLine2.InvoiceNumber = "423987432";
			commodityLine2.InvoiceLineNumber = 3;
			var docWrapper = (IDISDocument)new DISDocumentWrapper(disDocument);
			var lines = docWrapper.CommodityData;
			AssertEquals(2, lines.Count());
			AssertEquals("1", lines.ElementAt(0).CommodityDescription);
			AssertEquals("3", lines.ElementAt(1).CommodityDescription);
			disDocument.DocumentLabel = "CBP02";
			disDocument.Invoice.InvoiceNumber = "423987432";
			AssertEquals("423987432", docWrapper.Invoice.InvoiceNumber);
			Assert(docWrapper.CommodityData.IsNullOrEmpty());
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
			invoiceMock.VerifyAll();
			invoiceLineMock.VerifyAll();
			invoiceLineMock2.VerifyAll();
			invoiceLineMock3.VerifyAll();
		}

		public void TestPermitAndCertificateData()
		{
			var helper = new DISUniversalReferenceHelper(Factory);
			var codeNMF02 = helper.CreateDisCodeEntry("NMF02", "APH_STAT");
			helper.DataHelper.CreateNewOrGetExistingCusCodeListAttribute(codeNMF02.PK, RefCusCodeListAttributeTypes.Codes.OptionalData, OptionalDataTypeCodes.Permit);
			var codeCOM01 = helper.CreateDisCodeEntry("COM11", "COFR");
			helper.DataHelper.CreateNewOrGetExistingCusCodeListAttribute(codeCOM01.PK, RefCusCodeListAttributeTypes.Codes.OptionalData, OptionalDataTypeCodes.Certificate);
			Factory.Save();
			var usDISHost = GetMockDISHost();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			usDISHost.Setup(m => m.EDocs).Returns(System.Array.Empty<IeDoc>());
			var defaultValues = new Mock<IUSDISDefaultValues>();
			defaultValues.Setup(m => m.ImporterOfRecordID).Returns(new ZString("32-123832-00"));
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var docWrapper = (IDISDocument)new DISDocumentWrapper(disDocument);
			disDocument.DocumentLabel = "NMF02";
			disDocument.PermitData.PermitNumber = "p1";
			AssertEquals("32-123832-00", docWrapper.PermitData.ImporterOfRecord);
			AssertEquals("p1", docWrapper.PermitData.Number);
			disDocument.DocumentLabel = "COM11";
			AssertNull(docWrapper.PermitData);
			disDocument.CertificateData.CertificateNumber = "C1";
			AssertEquals("32-123832-00", docWrapper.CertificateData.ImporterOfRecord);
			AssertEquals("C1", docWrapper.CertificateData.Number);
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
		}

		public void TestBondPackingListAndToxicSubstance()
		{
			var helper = new DISUniversalReferenceHelper(Factory);
			var codeCBP38 = helper.CreateDisCodeEntry("CBP38", "CBP38");
			helper.DataHelper.CreateNewOrGetExistingCusCodeListAttribute(codeCBP38.PK, RefCusCodeListAttributeTypes.Codes.OptionalData, OptionalDataTypeCodes.BondData);
			var codeCBP01 = helper.CreateDisCodeEntry("CBP01", "CBP01");
			helper.DataHelper.CreateNewOrGetExistingCusCodeListAttribute(codeCBP01.PK, RefCusCodeListAttributeTypes.Codes.OptionalData, OptionalDataTypeCodes.PackingList);
			var codeEPA03 = helper.CreateDisCodeEntry("EPA03", "EPA03");
			helper.DataHelper.CreateNewOrGetExistingCusCodeListAttribute(codeEPA03.PK, RefCusCodeListAttributeTypes.Codes.OptionalData, OptionalDataTypeCodes.ToxicSubstance);
			Factory.Save();
			var usDISHost = GetMockDISHost();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			usDISHost.Setup(m => m.EDocs).Returns(System.Array.Empty<IeDoc>());
			var defaultValues = new Mock<IUSDISDefaultValues>();
			defaultValues.Setup(m => m.PreparerID).Returns(new ZString("XJ5"));
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var docWrapper = (IDISDocument)new DISDocumentWrapper(disDocument);
			disDocument.DocumentLabel = "CBP38";
			var bondData = disDocument.BondData;
			bondData.BondNumber = "342789";
			AssertEquals("342789", docWrapper.BondData.BondNumber);
			disDocument.DocumentLabel = "CBP01";
			AssertNull(docWrapper.BondData);
			var packingList = disDocument.PackingList;
			packingList.PackingListNumber = "234897";
			AssertEquals("234897", docWrapper.PackingList.PackingListNumber);
			disDocument.DocumentLabel = "EPA03";
			AssertNull(docWrapper.PackingList);
			var toxicSubstance = disDocument.ToxicSubstanceData;
			toxicSubstance.EPAProducerEstNumber = "93248432";
			AssertEquals("93248432", docWrapper.ToxicSubstanceData.EPAProducerEstNumber);
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
		}

		public void TestTradeTransactionsAndCategory()
		{
			var usDISHost = GetMockDISHost();
			var tradeTransactionMock = new Mock<IDISTradeTransaction>();
			tradeTransactionMock.Setup(m => m.Type).Returns(TradeTransactionType.Entry);
			tradeTransactionMock.Setup(m => m.Number).Returns(new ZString("342789"));
			tradeTransactionMock.Setup(m => m.FilerOrSCAC).Returns(new ZString("XJH"));
			tradeTransactionMock.Setup(m => m.AdditionalNumbers).Returns(new ZString[] { new ZString("1"), new ZString("2"), new ZString("4") });
			var defaultValues = new Mock<IUSDISDefaultValues>();
			defaultValues.Setup(m => m.IsExport).Returns(false);
			defaultValues.Setup(m => m.PortOfEntry).Returns(new ZString("1111"));
			defaultValues.Setup(m => m.PreparerID).Returns(new ZString("XJ3"));
			defaultValues.Setup(m => m.PreparerSiteCode).Returns(new ZString("9999"));
			defaultValues.Setup(m => m.TransactionCategory).Returns(TransactionCategory.SingleTransaction);
			defaultValues.Setup(m => m.DefaultTradeTransactions).Returns(new IDISTradeTransaction[] { tradeTransactionMock.Object });
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var docWrapper = (IDISDocument)new DISDocumentWrapper(disDocument);
			AssertEquals("1111", docWrapper.PortCode);
			AssertEquals("9999", docWrapper.PreparerSiteCode);
			AssertEquals("XJ3", docWrapper.PreparerID);
			AssertEquals(TransactionCategory.SingleTransaction, docWrapper.TransactionCategory);
			var tradeTransactions = docWrapper.TradeTransactions;
			AssertEquals(1, tradeTransactions.Count());
			var tradeTransaction = tradeTransactions.ElementAt(0);
			AssertEquals(TradeTransactionType.Entry, tradeTransaction.Type);
			AssertEquals("342789", tradeTransaction.Number);
			AssertEquals("XJH", tradeTransaction.FilerOrSCAC);
			AssertEquals("1,2,4", new ZStringBuilder(tradeTransaction.AdditionalNumbers).ToStringWithDelimiterBetweenAppends(","));
			defaultValues.VerifyAll();
		}

		public void TestTradeTransactionsForExport()
		{
			var usDISHost = GetMockDISHost();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var defaultValues = new Mock<IUSDISDefaultValues>();
			defaultValues.Setup(m => m.IsExport).Returns(true);
			var defaultTradeTransaction1 = new Mock<IDISTradeTransaction>();
			defaultTradeTransaction1.Setup(m => m.ShipmentNo).Returns(new ZString("001"));
			defaultTradeTransaction1.Setup(m => m.Number).Returns(new ZString("123567"));
			defaultTradeTransaction1.Setup(m => m.XTN).Returns(new ZString("SV9-1234567"));
			var defaultTradeTransaction2 = new Mock<IDISTradeTransaction>();
			defaultTradeTransaction2.Setup(m => m.ShipmentNo).Returns(new ZString("002"));
			defaultTradeTransaction2.Setup(m => m.XTN).Returns(new ZString("SV9-222222"));
			defaultValues.Setup(m => m.DefaultTradeTransactions).Returns(new IDISTradeTransaction[] { defaultTradeTransaction1.Object, defaultTradeTransaction2.Object });
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			disDocument.ShipmentNo = "001";
			var docWrapper = (IDISDocument)new DISDocumentWrapper(disDocument);
			var tradeTransactions = docWrapper.TradeTransactions;
			AssertEquals(1, tradeTransactions.Count());
			var tradeTransaction = tradeTransactions.ElementAt(0);
			AssertEquals("001", tradeTransaction.ShipmentNo);
			AssertEquals("123567", tradeTransaction.Number);
			defaultTradeTransaction1.VerifyAll();
			defaultTradeTransaction2.VerifyAll();
		}

		public void TestCreateMessage()
		{
			var declaration = (IUSDISHost)new TestHelper(Factory).GetJobDeclaration();
			var requiredDocument = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var documentAddInfo = requiredDocument.AddInfos.AddNew();
			documentAddInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			var requiredDocument2 = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var documentAddInfo2 = requiredDocument2.AddInfos.AddNew();
			documentAddInfo2.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			Factory.Save();
			var hostWrapper = new DISHostWrapper(declaration);
			var disDocument = new DISDocument(hostWrapper);
			disDocument.RequiredDocumentPK = requiredDocument2.PK;
			disDocument.RequiredDocumentAddInfo = documentAddInfo;
			disDocument.EDocsDocumentPK = ZGuid.NewZGuid();
			var docWrapper = (IDISDocument)new DISDocumentWrapper(disDocument);
			var message = docWrapper.CreateMessage(MessageTypeList.Codes.Submission);
			AssertEquals(true, message.IsTransmitMessage);
			AssertEquals(documentAddInfo, message.EM_LinkedObject);
			AssertEquals(MessageTypeList.Codes.Submission, message.EM_MessageType);
			AssertEquals(MessageTypeList.Codes.Submission, message.EM_MessageSubType);
			AssertEquals(1, message.MessageAttachments.Count);
			AssertEquals(disDocument.EDocsDocumentPK, message.MessageAttachments[0].EG_StorageDocsGuid);
		}

		public void TestCreateMessageWhenEDocsDocumentPKIsNotSelected()
		{
			var declaration = (IUSDISHost)new TestHelper(Factory).GetJobDeclaration();
			var requiredDocument = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var requiredDocument2 = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			Factory.Save();
			var hostWrapper = new DISHostWrapper(declaration);
			var disDocument = new DISDocument(hostWrapper);
			disDocument.RequiredDocumentPK = requiredDocument2.PK;
			disDocument.EDocsDocumentPK = ZGuid.Empty;
			var docWrapper = (IDISDocument)new DISDocumentWrapper(disDocument);
			var message = docWrapper.CreateMessage(MessageTypeList.Codes.Submission);
			AssertEquals(0, message.MessageAttachments.Count);
		}

		public void TestActionCode()
		{
			var declaration = (IUSDISHost)new TestHelper(Factory).GetJobDeclaration();
			var hostWrapper = new DISHostWrapper(declaration);
			var disDocument = new DISDocument(hostWrapper);
			var docWrapper = (IDISDocument)new DISDocumentWrapper(disDocument);
			AssertEquals(ActionCodeList.Codes.Add, docWrapper.ActionCodeForSubmission);
			disDocument.Status = StatusList.Codes.COS;
			AssertEquals(ActionCodeList.Codes.Replace, docWrapper.ActionCodeForSubmission);
		}

		Mock<IUSDISHost> GetMockDISHost()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.FormGroups).Returns(System.Array.Empty<ZString>());
			return usDISHost;
		}
	}
}
