using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US.DIS;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	[TestedType(typeof(DISDocument))]
	sealed class DISDocumentTest : XmlSerializableNonPersistentBusinessObjectTest<DISDocument>
	{
		public void TestDefaultOtherFieldsBasedOneDoc()
		{
			var declaration = (IUSDISHost)JobDeclaration;
			var requiredDocument1 = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			requiredDocument1.EQ_DocType = Core.Constants.RefDocTypes.CommercialInvoice;
			var requiredDocument2 = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			requiredDocument2.EQ_DocType = Core.Constants.RefDocTypes.CertificateOfOrigin;
			var hostWrapper = new DISHostWrapper(declaration);
			var disDocument = new DISDocument(hostWrapper);
			var eDoc = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "Test", Core.Constants.RefDocTypes.CommercialInvoice);
			eDoc.Description = "SOME DESCRIPTION";
			disDocument.EDocsDocumentPK = eDoc.UniqueKey;
			AssertEquals(requiredDocument1.PK, disDocument.RequiredDocumentPK);
			AssertEquals("CBP02", disDocument.DocumentLabel);
			AssertEquals("SOME DESCRIPTION", disDocument.DocumentDescription);
		}

		public void TestCanDelete()
		{
			var usDISHost = new Mock<IUSDISHost>();
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			disDocument.Status = StatusList.Codes.EOS;
			Assert(disDocument.CanDelete);
			disDocument.Status = StatusList.Codes.AOS;
			Assert(!disDocument.CanDelete);
			AssertEquals("This document has been submitted to Customs. Delete is not allowed when a response from Customs is outstanding.", disDocument.ReasonForNotAbleToDelete);
			disDocument.Status = StatusList.Codes.COS;
			Assert(!disDocument.CanDelete);
			AssertEquals("This document has been accepted by Customs. Delete is not allowed once a document is accepted.", disDocument.ReasonForNotAbleToDelete);
			disDocument.Status = StatusList.Codes.CWS;
			Assert(!disDocument.CanDelete);
			AssertEquals("This document has been accepted by Customs. Delete is not allowed once a document is accepted.", disDocument.ReasonForNotAbleToDelete);
			usDISHost.VerifyAll();
		}

		public void TestRequiredDocumentPKReadOnly()
		{
			var usDISHost = new Mock<IUSDISHost>();
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			Assert(!disDocument.RequiredDocumentPKInfo.ReadOnly);
			disDocument.Status = StatusList.Codes.AOS;
			Assert(disDocument.RequiredDocumentPKInfo.ReadOnly);
			disDocument.Status = StatusList.Codes.EOS;
			Assert(!disDocument.RequiredDocumentPKInfo.ReadOnly);
			disDocument.Status = StatusList.Codes.COS;
			Assert(disDocument.RequiredDocumentPKInfo.ReadOnly);
			disDocument.Status = StatusList.Codes.CWS;
			Assert(disDocument.RequiredDocumentPKInfo.ReadOnly);
			usDISHost.VerifyAll();
		}

		public void TestIDSuffixReadOnly()
		{
			var usDISHost = new Mock<IUSDISHost>();
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			Assert(disDocument.IDSuffixInfo.ReadOnly);
			usDISHost.VerifyAll();
		}

		public void TestStatusDescription()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			AssertEquals("Not Sent", disDocument.StatusDescription);
			disDocument.Status = StatusList.Codes.AOS;
			AssertEquals(StatusList.Descriptions.AOS, disDocument.StatusDescription);
			usDISHost.VerifyAll();
		}

		public void TestDefaultInvoiceList()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var defaultValues = new Mock<IUSDISDefaultValues>();
			var defaultInvoiceMock = new Mock<ICommercialInvoiceDefault>();
			defaultInvoiceMock.Setup(m => m.InvoiceNumber).Returns(new ZString("423987432"));
			defaultInvoiceMock.Setup(m => m.Description).Returns(new ZString("Blah"));
			defaultValues.Setup(m => m.DefaultInvoiceData).Returns(new ICommercialInvoiceDefault[] { defaultInvoiceMock.Object });
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var list = disDocument.DefaultInvoiceList;
			AssertEquals(1, list.Count);
			AssertEquals("423987432", list[0].Code);
			AssertEquals("Blah", list[0].Description);
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
			defaultInvoiceMock.VerifyAll();
		}

		public void TestDefaultCBPRequests()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var defaultValues = new Mock<IUSDISDefaultValues>();
			var defaultRequestMock = new Mock<IDISCBPRequestDefault>();
			defaultRequestMock.Setup(m => m.ID).Returns(new ZString("423987432"));
			defaultRequestMock.Setup(m => m.Description).Returns(new ZString("Blah"));
			defaultValues.Setup(m => m.DefaultCBPRequests).Returns(new IDISCBPRequestDefault[] { defaultRequestMock.Object });
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var list = disDocument.DefaultCBPRequestList;
			AssertEquals(3, list.Count);
			AssertEquals("423987432", list[0].Code);
			AssertEquals("Blah", list[0].Description);
			Assert(list.ContainsCode(MiscCBPRequestIDList.Codes.Unknown));
			Assert(list.ContainsCode(MiscCBPRequestIDList.Codes.Unsolicited));
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
			defaultRequestMock.VerifyAll();
		}

		public void TestDefaultBondDetails()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var defaultValues = new Mock<IUSDISDefaultValues>();
			var bondDataListMock = new Mock<IDISBondDataDefault>();
			bondDataListMock.Setup(m => m.Code).Returns(new ZString("1"));
			bondDataListMock.Setup(m => m.Description).Returns(new ZString("blah"));
			defaultValues.Setup(m => m.DefaultBondData).Returns(new IDISBondDataDefault[] { bondDataListMock.Object });
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var list = disDocument.DefaultBondDataList;
			AssertEquals(1, list.Count);
			AssertEquals("1", list[0].Code);
			AssertEquals("blah", list[0].Description);
			AssertEquals(bondDataListMock.Object, disDocument.GetDefaultBondData("1"));
			AssertNull(disDocument.GetDefaultBondData("2"));
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
			bondDataListMock.VerifyAll();
		}

		public void TestDocumentLabel()
		{
			var helper = new DISUniversalReferenceHelper(Factory);
			helper.CreateDisCodeEntry("CBP01", "PACKING_LIST");
			helper.CreateDisCodeEntry("TRP01", "TRP_P1_PASTA_CERTIFICATE");
			Factory.Save();
			var disDocument = CreateDocument();
			disDocument.DocumentLabel = "CBP01";
			AssertNotNull("Should load CBP01", disDocument.DISFormCusCode);
			AssertEquals(disDocument.DISFormCusCode.ZZD_Code, "CBP01");
			Assert(disDocument.PGAs.Cast<DISPGA>().Any(p => p.Code == PGAList.Codes.CBP));
			disDocument.DocumentLabel = "TRP01";
			AssertNotNull("Should load TRP01", disDocument.DISFormCusCode);
			AssertEquals(disDocument.DISFormCusCode.ZZD_Code, "TRP01");
			Assert(disDocument.PGAs.Cast<DISPGA>().Any(p => p.Code == PGAList.Codes.TRP));
		}

		public void TestDocumentLabelsCollection()
		{
			var helper = new DISUniversalReferenceHelper(Factory);
			helper.CreateDisCodeEntry("CBP01", "PACKING_LIST");
			helper.CreateDisCodeEntry("TRP01", "TRP_P1_PASTA_CERTIFICATE");
			Factory.Save();
			var disDocument1 = CreateDocument();
			var collection1 = disDocument1.DocumentLabels;
			collection1.Load();
			AssertEquals("Collection has 2 entries", 2, collection1.Count);
			disDocument1.DocumentLabel = "TRP01";
			AssertEquals("Description matches Label TRP01", "TRP_P1_PASTA_CERTIFICATE", disDocument1.DocumentLabelUSDISDocCode);
			disDocument1.DocumentLabel = "CBP01";
			AssertEquals("Description matches Label CBP01", "PACKING_LIST", disDocument1.DocumentLabelUSDISDocCode);
			disDocument1.DocumentLabel = "XXX";
			AssertEquals("Description empty for invalid Label", "", disDocument1.DocumentLabelUSDISDocCode);
			disDocument1.DocumentLabel = "";
			AssertEquals("Description empty for empty Label", "", disDocument1.DocumentLabelUSDISDocCode);
		}

		[TestDate(2016, 12, 25)]
		public void TestDocumentLabelsCollection_ValidDISCode()
		{
			var helper = new DISUniversalReferenceHelper(Factory);
			helper.CreateDisCodeEntry("CBP01", "PACKING_LIST");
			helper.CreateDisCodeEntry("CBP44", "TRP_P1_PASTA_CERTIFICATE");
			helper.CreateDisCodeEntry("CBP134", "ROYALTY_AGREEMENT");
			Factory.Save();
			var disDocument = CreateDocument();
			var collection = disDocument.DocumentLabels;
			collection.Load();
			AssertEquals("Should load CBP01", true, collection.Cast<ZZRefCusCodeListCombined>().Any((o) => o.ZZD_Code == "CBP01"));
			AssertEquals("Should load CBP44", true, collection.Cast<ZZRefCusCodeListCombined>().Any((o) => o.ZZD_Code == "CBP44"));
			AssertEquals("Should load CBP134", true, collection.Cast<ZZRefCusCodeListCombined>().Any((o) => o.ZZD_Code == "CBP134"));
		}

		[TestDate(2016, 12, 25)]
		public void TestDocumentLabelsCollection_ExpiredDISCode()
		{
			var helper = new DISUniversalReferenceHelper(Factory);
			helper.CreateDisCodeEntry("CBP01", "PACKING_LIST");
			helper.CreateDisCodeEntry("CBP44", "TRP_P1_PASTA_CERTIFICATE", ZDateTime.MinSmallDateTimeValue, new ZDateTime(2016, 10, 31));
			helper.CreateDisCodeEntry("CBP134", "ROYALTY_AGREEMENT", new ZDateTime(2017, 1, 14), ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var disDocument = CreateDocument();
			var collection = disDocument.DocumentLabels;
			var dateFilteringQuery = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ZDateTime.Today);
			dateFilteringQuery.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today);
			collection.LoadWithMoreFiltering(dateFilteringQuery);
			AssertEquals("Should load CBP01", true, collection.Cast<ZZRefCusCodeListCombined>().Any((o) => o.ZZD_Code == "CBP01"));
			AssertEquals("Should not load CBP44", false, collection.Cast<ZZRefCusCodeListCombined>().Any((o) => o.ZZD_Code == "CBP44"));
			AssertEquals("Should not load CBP134", false, collection.Cast<ZZRefCusCodeListCombined>().Any((o) => o.ZZD_Code == "CBP134"));
		}

		public void TestDocumentLabelsCollection_ExcludedDISCode()
		{
			var helper = new DISUniversalReferenceHelper(Factory);
			helper.CreateDisCodeEntry("CBP01", "PACKING_LIST");
			helper.CreateDisCodeEntry("CBP110", "DRAWBACK_ACCOUNTS_PAYABLE", "DRW");
			helper.CreateDisCodeEntry("CBP134", "ROYALTY_AGREEMENT", "REC");
			Factory.Save();
			// for declaration exclude all codes with USDISFormGroup attribute
			var disDocument = CreateDocument();
			var collection = disDocument.DocumentLabels;
			collection.Load();
			AssertEquals("Should load CBP01", true, collection.Cast<ZZRefCusCodeListCombined>().Any((o) => o.ZZD_Code == "CBP01"));
			AssertEquals("Should not load CBP110", false, collection.Cast<ZZRefCusCodeListCombined>().Any((o) => o.ZZD_Code == "CBP110"));
			AssertEquals("Should not load CBP134", false, collection.Cast<ZZRefCusCodeListCombined>().Any((o) => o.ZZD_Code == "CBP134"));
			// for Drawback exclude all other codes
			var drwDISHost = new Mock<IUSDISHost>();
			drwDISHost.Setup(m => m.Factory).Returns(Factory);
			drwDISHost.Setup(m => m.FormGroups).Returns(new ZString[] { "DRW" });
			var drwDisDocument = new DISDocument(new DISHostWrapper(drwDISHost.Object));
			var drwCollection = drwDisDocument.DocumentLabels;
			drwCollection.Load();
			AssertEquals("Should load CBP01", true, drwCollection.Cast<ZZRefCusCodeListCombined>().Any((o) => o.ZZD_Code == "CBP01"));
			AssertEquals("Should load CBP110", true, drwCollection.Cast<ZZRefCusCodeListCombined>().Any((o) => o.ZZD_Code == "CBP110"));
			AssertEquals("Should not load CBP134", false, drwCollection.Cast<ZZRefCusCodeListCombined>().Any((o) => o.ZZD_Code == "CBP134"));
			// for Recon exclude all other codes
			var recDISHost = new Mock<IUSDISHost>();
			recDISHost.Setup(m => m.Factory).Returns(Factory);
			recDISHost.Setup(m => m.FormGroups).Returns(new ZString[] { "REC" });
			var recDisDocument = new DISDocument(new DISHostWrapper(recDISHost.Object));
			var recCollection = recDisDocument.DocumentLabels;
			recCollection.Load();
			AssertEquals("Should load CBP01", true, recCollection.Cast<ZZRefCusCodeListCombined>().Any((o) => o.ZZD_Code == "CBP01"));
			AssertEquals("Should not load CBP110", false, recCollection.Cast<ZZRefCusCodeListCombined>().Any((o) => o.ZZD_Code == "CBP110"));
			AssertEquals("Should load CBP134", true, recCollection.Cast<ZZRefCusCodeListCombined>().Any((o) => o.ZZD_Code == "CBP134"));
			// verify multiple groups by including ungrouped with Recon
			var recallDISHost = new Mock<IUSDISHost>();
			recallDISHost.Setup(m => m.Factory).Returns(Factory);
			recallDISHost.Setup(m => m.FormGroups).Returns(new ZString[] { "REC", DISFormGroupCodes.NoGroup });
			var recallDisDocument = new DISDocument(new DISHostWrapper(recallDISHost.Object));
			var recallCollection = recallDisDocument.DocumentLabels;
			recallCollection.Load();
			AssertEquals("Should load CBP01", true, recallCollection.Cast<ZZRefCusCodeListCombined>().Any((o) => o.ZZD_Code == "CBP01"));
			AssertEquals("Should not load CBP110", false, recallCollection.Cast<ZZRefCusCodeListCombined>().Any((o) => o.ZZD_Code == "CBP110"));
			AssertEquals("Should load CBP134", true, recallCollection.Cast<ZZRefCusCodeListCombined>().Any((o) => o.ZZD_Code == "CBP134"));
			drwDISHost.VerifyAll();
			recDISHost.VerifyAll();
			recallDISHost.VerifyAll();
		}

		public void TestGetOptionalDataTypes()
		{
			var helper = new DISUniversalReferenceHelper(Factory);
			var code1 = helper.CreateDisCodeEntry("COM01", "PACKING_LIST");
			helper.DataHelper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, "OptionalData", "COM");
			var code2 = helper.CreateDisCodeEntry("EPA05", "EPA05");
			helper.DataHelper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, "OptionalData", "COM");
			helper.DataHelper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, "OptionalData", "TOX");
			var code2a = helper.CreateDisCodeEntry("EPA06", "EPA01");
			helper.DataHelper.CreateNewOrGetExistingCusCodeListAttribute(code2a.PK, "OptionalData", "COM");
			helper.DataHelper.CreateNewOrGetExistingCusCodeListAttribute(code2a.PK, "OptionalData", "TOX");
			var code3 = helper.CreateDisCodeEntry("NMF02", "ROYALTY_AGREEMENT");
			helper.DataHelper.CreateNewOrGetExistingCusCodeListAttribute(code3.PK, "OptionalData", "COM");
			helper.DataHelper.CreateNewOrGetExistingCusCodeListAttribute(code3.PK, "OptionalData", "PER");
			var code4 = helper.CreateDisCodeEntry("CBP01", "PACKING_LIST");
			helper.DataHelper.CreateNewOrGetExistingCusCodeListAttribute(code4.PK, "OptionalData", "INV");
			helper.DataHelper.CreateNewOrGetExistingCusCodeListAttribute(code4.PK, "OptionalData", "PCK");
			var code5 = helper.CreateDisCodeEntry("CBP02", "TRP_P1_PASTA_CERTIFICATE");
			helper.DataHelper.CreateNewOrGetExistingCusCodeListAttribute(code5.PK, "OptionalData", "INV");
			var code6 = helper.CreateDisCodeEntry("CBP39", "ROYALTY_AGREEMENT");
			helper.DataHelper.CreateNewOrGetExistingCusCodeListAttribute(code6.PK, "OptionalData", "BND");
			var code7 = helper.CreateDisCodeEntry("COM02", "SOLAS_1");
			helper.DataHelper.CreateNewOrGetExistingCusCodeListAttribute(code7.PK, "OptionalData", "CER");
			Factory.Save();
			var disDocument = CreateDocument();
			AssertDocumentHasExpectedOptionalDataTypes(disDocument, OptionalDataTypes.None);
			disDocument.DocumentLabel = "COM01";
			AssertDocumentHasExpectedOptionalDataTypes(disDocument, OptionalDataTypes.Commodity);
			disDocument.DocumentLabel = "EPA06";
			AssertDocumentHasExpectedOptionalDataTypes(disDocument, (OptionalDataTypes.Commodity | OptionalDataTypes.ToxicSubstance));
			disDocument.DocumentLabel = "NMF02";
			AssertDocumentHasExpectedOptionalDataTypes(disDocument, (OptionalDataTypes.Commodity | OptionalDataTypes.Permit));
			disDocument.DocumentLabel = "CBP01";
			AssertDocumentHasExpectedOptionalDataTypes(disDocument, (OptionalDataTypes.Invoice | OptionalDataTypes.PackingList));
			disDocument.DocumentLabel = "CBP02";
			AssertDocumentHasExpectedOptionalDataTypes(disDocument, (OptionalDataTypes.Invoice));
			disDocument.DocumentLabel = "CBP39";
			AssertDocumentHasExpectedOptionalDataTypes(disDocument, (OptionalDataTypes.BondData));
			disDocument.DocumentLabel = "COM02";
			AssertDocumentHasExpectedOptionalDataTypes(disDocument, (OptionalDataTypes.Certificate));
		}

		void AssertDocumentHasExpectedOptionalDataTypes(DISDocument disDocument, OptionalDataTypes dataTypes)
		{
			AssertEquals(dataTypes, disDocument.OptionalDataTypes);
			AssertEquals((dataTypes == OptionalDataTypes.None), disDocument.NoOptionalDataVisible);
			AssertEquals(((dataTypes & OptionalDataTypes.BondData) > 0), disDocument.BondDataVisible);
			AssertEquals(((dataTypes & OptionalDataTypes.PackingList) > 0), disDocument.PackingListVisible);
			AssertEquals(((dataTypes & OptionalDataTypes.Invoice) > 0), disDocument.InvoiceVisible);
			AssertEquals(((dataTypes & OptionalDataTypes.ToxicSubstance) > 0), disDocument.ToxicSubstanceVisible);
			AssertEquals(((dataTypes & OptionalDataTypes.Permit) > 0), disDocument.PermitVisible);
			AssertEquals(((dataTypes & OptionalDataTypes.Certificate) > 0), disDocument.CertificateVisible);
			AssertEquals(((dataTypes & OptionalDataTypes.Commodity) > 0), disDocument.CommodityDataVisible);
		}

		public void TestNullifyRequiredDocumentAddInfoWhenRequiredDocumentChanges()
		{
			var jobDeclaration = (IUSDISHost)JobDeclaration;
			var requiredDocument = jobDeclaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var addInfo = requiredDocument.AddInfos.AddNew();
			addInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			var requiredDocument2 = jobDeclaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var hostWrapper = new DISHostWrapper(jobDeclaration);
			var disDocument = new DISDocument(hostWrapper);
			disDocument.RequiredDocumentPK = requiredDocument.PK;
			disDocument.RequiredDocumentAddInfo = addInfo;
			disDocument.RequiredDocumentPK = requiredDocument2.PK;
			AssertNull(disDocument.RequiredDocumentAddInfo);
		}

		public void TestOptionalDataUnavailableReason()
		{
			var helper = new DISUniversalReferenceHelper(Factory);
			helper.CreateDisCodeEntry("AMS01", "PACKING_LIST");
			Factory.Save();
			var disDocument = CreateDocument();
			AssertEquals(DISDocument.EnterAFormTypeMessage, disDocument.NoOptionalDataVisibleReason);
			disDocument.DocumentLabel = "XXX";
			AssertEquals(DISDocument.EnterAValidFormTypeMessage, disDocument.NoOptionalDataVisibleReason);
			disDocument.DocumentLabel = "AMS01";
			AssertEquals(DISDocument.FormTypeNotRequireDataMessage, disDocument.NoOptionalDataVisibleReason);
		}

		public void TestMessageSendingError()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var defaultValues = new Mock<IUSDISDefaultValues>();
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			usDISHost.Setup(m => m.MessageSendingError).Returns("Error");
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			AssertEquals("Error", disDocument.MessageSendingError);
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
		}

		public void TestFileNameHasInvalidCharacters()
		{
			var declaration = (IUSDISHost)JobDeclaration;
			var docManagerSupport = (IDocManagerSupport)declaration;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(JobDeclaration, "TST");
			var eDocs1 = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "ABC", false);
			var eDocs2 = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC×.pdf", "ABC", false);
			var eDocs3 = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC%.pdf", "ABC", false);
			var hostWrapper = new DISHostWrapper(declaration);
			var disDocument = hostWrapper.DISDocuments.AddNew();
			AssertCollectionNotContains(DISDocumentValidation.FileNameHasInvalidCharacters, disDocument.MessageSendingWarning.Split('\r', '\n'));
			disDocument.EDocsDocumentPK = eDocs1.UniqueKey;
			AssertCollectionNotContains(DISDocumentValidation.FileNameHasInvalidCharacters, disDocument.MessageSendingWarning.Split('\r', '\n'));
			disDocument.EDocsDocumentPK = eDocs2.UniqueKey;
			AssertCollectionContains(DISDocumentValidation.FileNameHasInvalidCharacters, disDocument.MessageSendingWarning.Split('\r', '\n'));
			disDocument.EDocsDocumentPK = eDocs3.UniqueKey;
			AssertCollectionContains(DISDocumentValidation.FileNameHasInvalidCharacters, disDocument.MessageSendingWarning.Split('\r', '\n'));
		}

		public void TestDefaultPGACode()
		{
			var jobDeclaration = (IUSDISHost)JobDeclaration;
			var hostWrapper = new DISHostWrapper(jobDeclaration);
			var disDocument = new DISDocument(hostWrapper);
			disDocument.DocumentLabel = "EPA04";
			AssertEquals("Should default EPA to the list", 1, disDocument.PGAs.Count);
			AssertEquals("Should default EPA to the list", "EPA", disDocument.PGAs[0].Code);
			disDocument.DocumentLabel = "APH01";
			AssertEquals("Should default APH to the list", 2, disDocument.PGAs.Count);
			AssertEquals("Should default APH to the list", "APH", disDocument.PGAs[1].Code);
		}

		public void TestDefaultPGACodeAboutFAS()
		{
			var jobDeclaration = (IUSDISHost)JobDeclaration;
			var hostWrapper = new DISHostWrapper(jobDeclaration);
			var disDocument = new DISDocument(hostWrapper);
			disDocument.DocumentLabel = "FAS01";
			AssertEquals("Should default EPA to the list", 1, disDocument.PGAs.Count);
			AssertEquals("Should default EPA to the list", "FAS", disDocument.PGAs[0].Code);
			disDocument.PGAs.RemoveAndDeleteAll();
			Factory.Save();
			disDocument.DocumentLabel = "FAS02";
			AssertEquals("Should default APH to the list", 1, disDocument.PGAs.Count);
			AssertEquals("Should default APH to the list", "FAS", disDocument.PGAs[0].Code);
			disDocument.PGAs.RemoveAndDeleteAll();
			Factory.Save();
			disDocument.DocumentLabel = "FAS03";
			AssertEquals("Should default APH to the list", 1, disDocument.PGAs.Count);
			AssertEquals("Should default APH to the list", "FAS", disDocument.PGAs[0].Code);
		}

		public void TestEDocsDocumentPK_ReadOnly()
		{
			var usDISHost = new Mock<IUSDISHost>();
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			Assert(!disDocument.EDocsDocumentPKInfo.ReadOnly);
			disDocument.Status = StatusList.Codes.EOS;
			Assert(!disDocument.EDocsDocumentPKInfo.ReadOnly);
			disDocument.Status = StatusList.Codes.AOS;
			Assert(disDocument.EDocsDocumentPKInfo.ReadOnly);
			disDocument.Status = StatusList.Codes.COS;
			Assert(disDocument.EDocsDocumentPKInfo.ReadOnly);
			disDocument.Status = StatusList.Codes.CWS;
			Assert(disDocument.EDocsDocumentPKInfo.ReadOnly);
			usDISHost.VerifyAll();
		}

		public void TestDocumentReviewResults()
		{
			var declaration = (IUSDISHost)JobDeclaration;
			var requiredDocument = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.CertificateOfOrigin;
			var hostWrapper = new DISHostWrapper(declaration);
			var disDocument = new DISDocument(hostWrapper);
			var ediMessage = Factory.New<EDIMessage>();
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_SystemCreateTimeUtc = ZDateTime.Today;
			ediMessage.EM_MessageType = MessageTypeList.Codes.DocumentReviewResponse;
			ediMessage.EM_MessageText = TestHelper.DocumentReviewRejectedResponseXml;
			disDocument.Messages.Add(ediMessage);
			AssertEquals("", disDocument.DocumentRejectReason);
			AssertEquals("", disDocument.DocumentReviewComment);
			disDocument.Status = StatusList.Codes.ERV;
			AssertEquals("INCOMPLETE_DOCUMENT_SET", disDocument.DocumentRejectReason);
			AssertEquals("BTA anticipated arrival information is missing", disDocument.DocumentReviewComment);
		}

		public void TestVDoNotDefaultFormTypeWhenTrackingDocIsCOO()
		{
			var jobDeclaration = (IUSDISHost)JobDeclaration;
			var requiredDocument = jobDeclaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.CertificateOfOrigin;
			var hostWrapper = new DISHostWrapper((IUSDISHost)JobDeclaration);
			var disDocument = hostWrapper.DISDocuments.AddNew();
			disDocument.RequiredDocumentPK = requiredDocument.PK;
			AssertEquals("Tracking DocumentType is Coo", "COO Document: Certificate of Origin", disDocument.RequiredDocument.HumanReadableName);
			AssertEquals("Do not Default Form Type when Document Type is Coo, User will select one", ZString.Empty, disDocument.DocumentLabel);
		}

		DISDocument CreateDocument()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			usDISHost.Setup(m => m.EDocs).Returns(new List<IeDoc>());
			usDISHost.Setup(m => m.FormGroups).Returns(System.Array.Empty<ZString>());
			var defaultValues = new Mock<IUSDISDefaultValues>();
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			return new DISDocument(hostWrapper);
		}

		public void TestDefaultShipmentNo()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var defaultValues = new Mock<IUSDISDefaultValues>();
			defaultValues.Setup(m => m.IsExport).Returns(true);
			var tradeTransaction = new Mock<IDISTradeTransaction>();
			tradeTransaction.Setup(m => m.ShipmentNo).Returns(new ZString("001"));
			tradeTransaction.Setup(m => m.Number).Returns(new ZString("123567"));
			tradeTransaction.Setup(m => m.XTN).Returns(new ZString("SV9-1234567"));
			defaultValues.Setup(m => m.DefaultTradeTransactions).Returns(new IDISTradeTransaction[] { tradeTransaction.Object });
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			disDocument.DefaultShipmentNo();
			AssertEquals("001", disDocument.ShipmentNo);
			AssertEquals("123567", disDocument.ITN);
			AssertEquals("SV9-1234567", disDocument.XTN);
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
			tradeTransaction.VerifyAll();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var hostWrapper = new DISHostWrapper((IUSDISHost)JobDeclaration);
			return new DISDocument(hostWrapper);
		}

		BusinessObject JobDeclaration => jobDeclaration ?? (jobDeclaration = new TestHelper(Factory).GetJobDeclaration());
		BusinessObject jobDeclaration;
	}
}
