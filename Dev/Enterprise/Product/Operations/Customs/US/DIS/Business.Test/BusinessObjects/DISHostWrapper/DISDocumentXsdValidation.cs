using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class DISDocumentXsdValidation : DISDocumentXsdValidationBase<DISDocument>
	{
		protected override string GetExpectedXmlData()
		{
			var retriever = new EmbeddedResourceRetriever();
			var expectedXmlText = retriever.GetString("Enterprise.Customs.US.DIS.Business.Testing.BusinessObjects.DISHostWrapper.TestFiles.DISDocument.xml");
			return string.Format(expectedXmlText, eDocsUniqueKey);
		}
		ZGuid eDocsUniqueKey;

		protected override string GetExpectedXsdData()
		{
			var retriever = new EmbeddedResourceRetriever();
			return retriever.GetString("Enterprise.Customs.US.DIS.Business.Testing.BusinessObjects.DISHostWrapper.TestFiles.DISDocument.xsd");
		}

		protected override XmlSerializableNonPersistentBusinessObject GetFullyPopulatedBizObj()
		{
			var helper = new DISUniversalReferenceHelper(Factory);
			var codeEPA01 = helper.CreateDisCodeEntry("APH01", "APH_STAT");
			Factory.Save();
			var declaration = new TestHelper(Factory).GetJobDeclaration();
			var uSDISHost = (IUSDISHost)declaration;
			var hostWrapper = new DISHostWrapper(uSDISHost);
			var requiredDocument = uSDISHost.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			requiredDocument.EQ_DocNumber = "DN21";
			var docManagerSupport = (IDocManagerSupport)declaration;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(declaration, "TST");
			var eDocs = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "ABC", false);
			var disDocument = hostWrapper.DISDocuments.AddNew();
			disDocument.RequiredDocumentPK = requiredDocument.PK;
			disDocument.IDSuffix = 2;
			disDocument.Comment = "Comment";
			disDocument.DocumentLabel = "APH01";
			disDocument.DocumentDescription = "desc";
			disDocument.SubmitDateUTC = new ZDateTime(2013, 12, 25, 12, 25, 25);
			disDocument.Status = Enterprise.Customs.Common.US.DIS.StatusList.Codes.AOS;
			eDocsUniqueKey = eDocs.UniqueKey;
			disDocument.EDocsDocumentPK = eDocsUniqueKey;
			var cBPRequest = disDocument.CBPRequest;
			cBPRequest.ID = "ID123456";
			cBPRequest.Type = "ID6";
			cBPRequest.RequestDate = new ZDateTime(2014, 1, 31, 2, 3, 40);
			var invoice = disDocument.Invoice;
			invoice.InvoiceNumber = "INV2342";
			var invoiceLineRange1 = invoice.InvoiceLineRanges.AddNew();
			invoiceLineRange1.InvoiceLineFrom = 1;
			invoiceLineRange1.InvoiceLineTo = 2;
			var invoiceLineRange2 = invoice.InvoiceLineRanges.AddNew();
			invoiceLineRange2.InvoiceLineFrom = 1;
			invoiceLineRange2.InvoiceLineTo = 2;
			var bondData = disDocument.BondData;
			bondData.DefaultBondCode = "DF1";
			bondData.BondName = "BN2";
			bondData.BondNumber = "BN21312312";
			bondData.BondType = "BT4";
			bondData.SuretyCode = "SC3";
			bondData.AgentIDNumber = "AIDN4242323";
			bondData.BondAmount = 1568656.25m;
			var packingList = disDocument.PackingList;
			packingList.PackingListNumber = "PLN3242";
			packingList.InvoiceNumber = "INV32432";
			packingList.PurchaseOrderNumber = "PON2343223";
			var certificateData = disDocument.CertificateData;
			certificateData.CertificateNumber = "CN38656";
			certificateData.CertificateType = "CT9";
			certificateData.Statement = "ST BOB IS NOT SO ST";
			certificateData.IssueDate = new ZDateTime(2014, 11, 11, 10, 32, 54);
			certificateData.ExpiryDate = new ZDateTime(2014, 12, 22, 10, 32, 54);
			certificateData.InspectionLocation = "IL23423";
			var permitData = disDocument.PermitData;
			permitData.PermitNumber = "PN965456";
			permitData.PermitType = "PT8";
			permitData.ApprovalNumber = "AN896355";
			permitData.Statement = "BOB THE BUILDER";
			permitData.StartDate = new ZDateTime(2015, 2, 6, 5, 4, 3);
			permitData.EndDate = new ZDateTime(2015, 2, 7, 6, 5, 4);
			var toxicSubstanceData = disDocument.ToxicSubstanceData;
			toxicSubstanceData.EPARegistrationNumber = "EPARN865";
			toxicSubstanceData.EPAProducerEstNumber = "EPAPEN6355";
			var cASNumber1 = toxicSubstanceData.CASNumbers.AddNew();
			cASNumber1.Number = "CSN1";
			var cASNumber2 = toxicSubstanceData.CASNumbers.AddNew();
			cASNumber2.Number = "CSN2";
			var commodity1 = disDocument.CommodityData.AddNew();
			commodity1.InvoiceNumber = "INV2342";
			commodity1.InvoiceLineNumber = 1;
			commodity1.InvoiceLineTo = 2;
			commodity1.VNELineNumber = 1;
			commodity1.VNELineNumberTo = 2;
			var commodity2 = disDocument.CommodityData.AddNew();
			commodity2.InvoiceNumber = "INV2342";
			commodity2.InvoiceLineNumber = 3;
			commodity2.InvoiceLineTo = 4;
			commodity2.VNELineNumber = 3;
			commodity2.VNELineNumberTo = 4;
			var addInfo1 = disDocument.AdditionalData.AddNew();
			addInfo1.Name = "Invoice Amount";
			addInfo1.Data = "31267.90";
			var addInfo2 = disDocument.AdditionalData.AddNew();
			addInfo2.Name = "Invoice Amount 2";
			addInfo2.Data = "86563.56";
			var pga1 = disDocument.PGAs.AddNew();
			pga1.Code = "FDA";
			var pga2 = disDocument.PGAs.AddNew();
			pga2.Code = "FCC";
			return disDocument;
		}

		protected override string XsdNamespace => "http://www.cargowise.com/Schemas/DISDocument";

		protected override XmlSerializableNonPersistentBusinessObject GetEmptyBizObj()
		{
			var helper = new DISUniversalReferenceHelper(Factory);
			var codeEPA01 = helper.CreateDisCodeEntry("APH01", "APH_STAT");
			Factory.Save();
			var declaration = new TestHelper(Factory).GetJobDeclaration();
			var uSDISHost = (IUSDISHost)declaration;
			var hostWrapper = new DISHostWrapper(uSDISHost);
			var requiredDocument = uSDISHost.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			requiredDocument.EQ_DocNumber = "DN21";
			var docManagerSupport = (IDocManagerSupport)declaration;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(declaration, "TST");
			var eDocs = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "ABC", false);
			var disDocument = hostWrapper.DISDocuments.AddNew();
			disDocument.RequiredDocumentPK = requiredDocument.PK;
			var cBPRequest = disDocument.CBPRequest;
			cBPRequest.ID = "ID123456";
			var invoice = disDocument.Invoice;
			var invoiceLineRange1 = invoice.InvoiceLineRanges.AddNew();
			invoiceLineRange1.InvoiceLineFrom = 1;
			var bondData = disDocument.BondData;
			bondData.DefaultBondCode = "DF1";
			var packingList = disDocument.PackingList;
			packingList.PackingListNumber = "PLN3242";
			var certificateData = disDocument.CertificateData;
			certificateData.CertificateNumber = "CN38656";
			var permitData = disDocument.PermitData;
			permitData.PermitNumber = "PN965456";
			var toxicSubstanceData = disDocument.ToxicSubstanceData;
			toxicSubstanceData.EPARegistrationNumber = "EPARN865";
			var cASNumber1 = toxicSubstanceData.CASNumbers.AddNew();
			cASNumber1.Number = "CSN1";
			var commodity1 = disDocument.CommodityData.AddNew();
			commodity1.InvoiceNumber = "INV2342";
			var addInfo1 = disDocument.AdditionalData.AddNew();
			addInfo1.Name = "Invoice Amount";
			var pga1 = disDocument.PGAs.AddNew();
			pga1.Code = "FDA";
			return disDocument;
		}

		protected override string GetExpectedEmptyXmlData()
		{
			var retriever = new EmbeddedResourceRetriever();
			return retriever.GetString("Enterprise.Customs.US.DIS.Business.Testing.BusinessObjects.DISHostWrapper.TestFiles.DISDocumentWithMinimumData.xml");
		}

		protected override DISHostWrapperBase<DISDocument> GetDISHostWrapper(IDISHost disHost) => new DISHostWrapper(disHost as IUSDISHost);

		protected override DISDocument GetDISDocument(DISHostWrapperBase<DISDocument> hostWrapper) => new DISDocument(hostWrapper as DISHostWrapper);
	}
}
