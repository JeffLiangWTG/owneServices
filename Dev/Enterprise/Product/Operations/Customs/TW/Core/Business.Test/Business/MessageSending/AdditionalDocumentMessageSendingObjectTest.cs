using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(AdditionalDocumentMessageSendingObject))]
	sealed class AdditionalDocumentMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestDefaultAction()
		{
			var declaration = this.declaration;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = "A";
			entryHeader.CH_Status = "B";
			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;

			var sendingObject = new AdditionalDocumentMessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(sendingObject.Action, NUnit.Framework.Is.EqualTo(ActionCodeList.Codes.Create).Using(CustomComparers.TypeComparison));

			var cusDisposition = entryHeader.CusDispositions.AddNew();
			cusDisposition.CDI_StatusKey = CusDispositionStatusKeyList.Codes.RFM;
			sendingObject = new AdditionalDocumentMessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(sendingObject.Action, NUnit.Framework.Is.EqualTo(ActionCodeList.Codes.Update).Using(CustomComparers.TypeComparison));

			entryHeader.CusDispositions.RemoveAndDeleteAll();
			cusDisposition = entryHeader.CusDispositions.AddNew();
			cusDisposition.CDI_StatusKey = CusDispositionStatusKeyList.Codes.ARM;
			sendingObject = new AdditionalDocumentMessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(sendingObject.Action, NUnit.Framework.Is.EqualTo(ActionCodeList.Codes.Create).Using(CustomComparers.TypeComparison));

			entryHeader.CusDispositions.RemoveAndDeleteAll();
			cusDisposition = entryHeader.CusDispositions.AddNew();
			cusDisposition.CDI_StatusKey = CusDispositionStatusKeyList.Codes.ARM;
			cusDisposition.CDI_Status = "B04";
			sendingObject = new AdditionalDocumentMessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(sendingObject.Action, NUnit.Framework.Is.EqualTo(ActionCodeList.Codes.Update).Using(CustomComparers.TypeComparison));

			entryHeader.CusDispositions.RemoveAndDeleteAll();
			cusDisposition = entryHeader.CusDispositions.AddNew();
			cusDisposition.CDI_StatusKey = CusDispositionStatusKeyList.Codes.ARM;
			cusDisposition.CDI_Status = "F04";
			sendingObject = new AdditionalDocumentMessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(sendingObject.Action, NUnit.Framework.Is.EqualTo(ActionCodeList.Codes.Update).Using(CustomComparers.TypeComparison));

			entryHeader.CusDispositions.RemoveAndDeleteAll();
			cusDisposition = entryHeader.CusDispositions.AddNew();
			cusDisposition.CDI_StatusKey = CusDispositionStatusKeyList.Codes.CLR;
			cusDisposition.CDI_Status = "F04";
			sendingObject = new AdditionalDocumentMessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(sendingObject.Action, NUnit.Framework.Is.EqualTo(ActionCodeList.Codes.Create).Using(CustomComparers.TypeComparison));

			entryHeader.CusDispositions.RemoveAndDeleteAll();
			cusDisposition = entryHeader.CusDispositions.AddNew();
			cusDisposition.CDI_StatusKey = CusDispositionStatusKeyList.Codes.ARM;
			cusDisposition.CDI_Status = "D04";
			sendingObject = new AdditionalDocumentMessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(sendingObject.Action, NUnit.Framework.Is.EqualTo(ActionCodeList.Codes.Create).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDefaultShouldSendToTrue()
		{
			var declaration = this.declaration;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			var sendingObject = new AdditionalDocumentMessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(sendingObject.ShouldSend, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestActionList()
		{
			var declaration = this.declaration;
			var cusHead1 = Factory.NewWithValidTestData<CusEntryHeader>();
			cusHead1.CH_JE = declaration.PK;
			var cusNum1 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum1.CE_ParentID = cusHead1.PK;
			cusNum1.CE_Category = "CUS";
			cusNum1.CE_EntryType = SharedJobMessageTypeList.Codes.Import;
			cusNum1.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum1.CE_EntryNum = "NO1";
			cusHead1.CH_EntryStatus = "A";
			cusHead1.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			Factory.Save();
			var action = new AdditionalDocumentMessageSendingObject(cusHead1);
			var list = action.ActionList;
			NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(ActionCodeList.Codes.Create), NUnit.Framework.Is.EqualTo(ActionCodeList.Descriptions.Create));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(ActionCodeList.Codes.Update), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(ActionCodeList.Codes.Delete), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestContactOffice()
		{
			var declaration = this.declaration;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = "A";
			entryHeader.CH_Status = "B";
			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			var sendingObject = new AdditionalDocumentMessageSendingObject(entryHeader);
			sendingObject.ContactOffice = "YY";
			NUnit.Framework.Assert.That(sendingObject.ContactOffice, NUnit.Framework.Is.EqualTo("YY").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestFriendlyNameForMessageManager()
		{
			var declaration = this.declaration;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			var sendingObject = new AdditionalDocumentMessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(sendingObject.FriendlyNameForMessageManager, NUnit.Framework.Is.EqualTo("Send Additional Document Message").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestShouldMessageTypeisADM()
		{
			var declaration = this.declaration;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = "A";
			entryHeader.CH_Status = "B";
			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			var sendingObject = new AdditionalDocumentMessageSendingObject(entryHeader);
			sendingObject.MessageType = "XXX";
			NUnit.Framework.Assert.That(!sendingObject.IsMessageTypeADM, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			sendingObject.MessageType = MessageTypeList.Codes.ADM;
			NUnit.Framework.Assert.That(sendingObject.IsMessageTypeADM, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestAvailableEDocs()
		{
			var declaration = this.declaration;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = "A";
			entryHeader.CH_Status = "B";
			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			IeDoc GetEDoc(string fileName, string documentType)
			{
				var fullFileName = Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\{fileName}");
				return ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(fullFileName, documentType, true);
			}

			GetEDoc("sample.pdf", Core.Constants.FileFormats.PDF);
			GetEDoc("Test.xls", Core.Constants.FileFormats.XLS);
			var sendingObject = new AdditionalDocumentMessageSendingObject(entryHeader);
			var availableEDocs = sendingObject.AvailableEDocs;
			NUnit.Framework.Assert.That(availableEDocs.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(availableEDocs.GetDescriptionFromCode("sample.pdf"), NUnit.Framework.Is.Not.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestAllEDocs()
		{
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			var importerDoc = importer.DocManagerInfo().AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), Core.Constants.FileFormats.PDF);
			var supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();
			var supplierDoc = supplier.DocManagerInfo().AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), Core.Constants.FileFormats.PDF);
			var exporter = Factory.New<OrgHeader>();
			exporter.FillWithValidTestData();
			var exporterDoc = exporter.DocManagerInfo().AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), Core.Constants.FileFormats.PDF);
			var consignee = Factory.New<OrgHeader>();
			consignee.FillWithValidTestData();
			var consigneeDoc = consignee.DocManagerInfo().AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), Core.Constants.FileFormats.PDF);
			var shippingLine = Factory.New<OrgHeader>();
			shippingLine.FillWithValidTestData();
			var shippingLineDoc = shippingLine.DocManagerInfo().AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), Core.Constants.FileFormats.PDF);
			var forwarder = Factory.New<OrgHeader>();
			forwarder.FillWithValidTestData();
			var forwarderDoc = forwarder.DocManagerInfo().AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), Core.Constants.FileFormats.PDF);
			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.FillWithValidTestData();
			var notifyPartyDoc = notifyParty.DocManagerInfo().AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), Core.Constants.FileFormats.PDF);
			var containerTerminalOperatorDocAddress = Factory.New<OrgHeader>();
			containerTerminalOperatorDocAddress.FillWithValidTestData();
			var containerTerminalOperatorDocAddressDoc = containerTerminalOperatorDocAddress.DocManagerInfo().AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), Core.Constants.FileFormats.PDF);
			var depotDocAddress = Factory.New<OrgHeader>();
			depotDocAddress.FillWithValidTestData();
			var depotDocAddressDoc = depotDocAddress.DocManagerInfo().AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), Core.Constants.FileFormats.PDF);
			var containerYardDocAddress = Factory.New<OrgHeader>();
			containerYardDocAddress.FillWithValidTestData();
			var containerYardDocAddressDoc = containerYardDocAddress.DocManagerInfo().AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), Core.Constants.FileFormats.PDF);
			var declarantAddress = Factory.New<OrgHeader>();
			declarantAddress.FillWithValidTestData();
			var declarantAddressDoc = declarantAddress.DocManagerInfo().AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), Core.Constants.FileFormats.PDF);
			var controllingAgent = Factory.New<OrgHeader>();
			controllingAgent.FillWithValidTestData();
			var controllingAgentDoc = controllingAgent.DocManagerInfo().AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), Core.Constants.FileFormats.PDF);
			var controllingCustomer = Factory.New<OrgHeader>();
			controllingCustomer.FillWithValidTestData();
			var controllingCustomerDoc = controllingCustomer.DocManagerInfo().AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), Core.Constants.FileFormats.PDF);
			var externalBroker = Factory.New<OrgHeader>();
			externalBroker.FillWithValidTestData();
			var externalBrokerDoc = externalBroker.DocManagerInfo().AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), Core.Constants.FileFormats.PDF);
			var product = (MasterFiles.Business.OrgSupplierPart)Factory.New<Integration.Customs.TW.IOrgSupplierPart>();
			product.OP_PartNum = "TestTEST";
			product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			var productDocManagerInfo = product.DocManagerInfo();
			var productDoc = productDocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), Core.Constants.FileFormats.PDF);
			productDocManagerInfo.SetupEDocsFactoryToBeSavedWithMainFactory(true);
			Factory.Save();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HBL1";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;
			declaration.ImporterDocumentaryAddress.OrganisationPK = importer.PK;
			declaration.SupplierDocumentaryAddress.OrganisationPK = supplier.PK;
			declaration.JE_OH_Exporter = exporter.PK;
			declaration.JE_OH_Consignee = consignee.PK;
			declaration.JE_OH_ShippingLine = shippingLine.PK;
			declaration.JE_OH_Forwarder = forwarder.PK;
			declaration.JE_OH_NotifyParty = notifyParty.PK;
			declaration.ContainerTerminalOperatorDocAddress.OrganisationPK = containerTerminalOperatorDocAddress.PK;
			declaration.DepotDocAddress.OrganisationPK = depotDocAddress.PK;
			declaration.ContainerYardDocAddress.OrganisationPK = containerYardDocAddress.PK;
			declaration.JE_OA_DeclarantAddress = declarantAddress.MainAddress.PK;
			declaration.JE_OH_ControllingAgent = controllingAgent.PK;
			declaration.JE_OH_ControllingCustomer = controllingCustomer.PK;
			declaration.JE_OH_ExternalBroker = externalBroker.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = "A";
			entryHeader.CH_Status = "B";
			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "TestTEST";
			var doc1 = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), Core.Constants.FileFormats.PDF);
			var doc2 = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Test.xls"), Core.Constants.FileFormats.XLS);
			var doc3 = ((IDocManagerSupport)shipment).DocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), Core.Constants.FileFormats.PDF);
			var doc4 = ((IDocManagerSupport)shipment).DocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Test.xls"), Core.Constants.FileFormats.XLS);
			var doc5 = invoice.DocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), Core.Constants.FileFormats.PDF);
			var doc6 = invoice.DocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Test.xls"), Core.Constants.FileFormats.XLS);
			var sendingObject = new AdditionalDocumentMessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(sendingObject.GetIeDocFromUniqueKey(doc1.UniqueKey.ToGuid()).UniqueKey, NUnit.Framework.Is.EqualTo(doc1.UniqueKey));
			NUnit.Framework.Assert.That(sendingObject.GetIeDocFromUniqueKey(doc2.UniqueKey.ToGuid()).UniqueKey, NUnit.Framework.Is.EqualTo(doc2.UniqueKey));
			NUnit.Framework.Assert.That(sendingObject.GetIeDocFromUniqueKey(doc3.UniqueKey.ToGuid()).UniqueKey, NUnit.Framework.Is.EqualTo(doc3.UniqueKey));
			NUnit.Framework.Assert.That(sendingObject.GetIeDocFromUniqueKey(doc4.UniqueKey.ToGuid()).UniqueKey, NUnit.Framework.Is.EqualTo(doc4.UniqueKey));
			NUnit.Framework.Assert.That(sendingObject.GetIeDocFromUniqueKey(doc5.UniqueKey.ToGuid()).UniqueKey, NUnit.Framework.Is.EqualTo(doc5.UniqueKey));
			NUnit.Framework.Assert.That(sendingObject.GetIeDocFromUniqueKey(doc6.UniqueKey.ToGuid()).UniqueKey, NUnit.Framework.Is.EqualTo(doc6.UniqueKey));
			NUnit.Framework.Assert.That(sendingObject.GetIeDocFromUniqueKey(importerDoc.UniqueKey.ToGuid()).UniqueKey, NUnit.Framework.Is.EqualTo(importerDoc.UniqueKey));
			NUnit.Framework.Assert.That(sendingObject.GetIeDocFromUniqueKey(supplierDoc.UniqueKey.ToGuid()).UniqueKey, NUnit.Framework.Is.EqualTo(supplierDoc.UniqueKey));
			NUnit.Framework.Assert.That(sendingObject.GetIeDocFromUniqueKey(productDoc.UniqueKey.ToGuid()).UniqueKey, NUnit.Framework.Is.EqualTo(productDoc.UniqueKey));
			NUnit.Framework.Assert.That(sendingObject.GetIeDocFromUniqueKey(exporterDoc.UniqueKey.ToGuid()).UniqueKey, NUnit.Framework.Is.EqualTo(exporterDoc.UniqueKey));
			NUnit.Framework.Assert.That(sendingObject.GetIeDocFromUniqueKey(consigneeDoc.UniqueKey.ToGuid()).UniqueKey, NUnit.Framework.Is.EqualTo(consigneeDoc.UniqueKey));
			NUnit.Framework.Assert.That(sendingObject.GetIeDocFromUniqueKey(shippingLineDoc.UniqueKey.ToGuid()).UniqueKey, NUnit.Framework.Is.EqualTo(shippingLineDoc.UniqueKey));
			NUnit.Framework.Assert.That(sendingObject.GetIeDocFromUniqueKey(forwarderDoc.UniqueKey.ToGuid()).UniqueKey, NUnit.Framework.Is.EqualTo(forwarderDoc.UniqueKey));
			NUnit.Framework.Assert.That(sendingObject.GetIeDocFromUniqueKey(notifyPartyDoc.UniqueKey.ToGuid()).UniqueKey, NUnit.Framework.Is.EqualTo(notifyPartyDoc.UniqueKey));
			NUnit.Framework.Assert.That(sendingObject.GetIeDocFromUniqueKey(containerTerminalOperatorDocAddressDoc.UniqueKey.ToGuid()).UniqueKey, NUnit.Framework.Is.EqualTo(containerTerminalOperatorDocAddressDoc.UniqueKey));
			NUnit.Framework.Assert.That(sendingObject.GetIeDocFromUniqueKey(depotDocAddressDoc.UniqueKey.ToGuid()).UniqueKey, NUnit.Framework.Is.EqualTo(depotDocAddressDoc.UniqueKey));
			NUnit.Framework.Assert.That(sendingObject.GetIeDocFromUniqueKey(containerYardDocAddressDoc.UniqueKey.ToGuid()).UniqueKey, NUnit.Framework.Is.EqualTo(containerYardDocAddressDoc.UniqueKey));
			NUnit.Framework.Assert.That(sendingObject.GetIeDocFromUniqueKey(declarantAddressDoc.UniqueKey.ToGuid()).UniqueKey, NUnit.Framework.Is.EqualTo(declarantAddressDoc.UniqueKey));
			NUnit.Framework.Assert.That(sendingObject.GetIeDocFromUniqueKey(controllingAgentDoc.UniqueKey.ToGuid()).UniqueKey, NUnit.Framework.Is.EqualTo(controllingAgentDoc.UniqueKey));
			NUnit.Framework.Assert.That(sendingObject.GetIeDocFromUniqueKey(controllingCustomerDoc.UniqueKey.ToGuid()).UniqueKey, NUnit.Framework.Is.EqualTo(controllingCustomerDoc.UniqueKey));
			NUnit.Framework.Assert.That(sendingObject.GetIeDocFromUniqueKey(externalBrokerDoc.UniqueKey.ToGuid()).UniqueKey, NUnit.Framework.Is.EqualTo(externalBrokerDoc.UniqueKey));
			NUnit.Framework.Assert.That(sendingObject.GetIeDocFromUniqueKey(controllingAgentDoc.UniqueKey.ToGuid()).UniqueKey, NUnit.Framework.Is.EqualTo(controllingAgentDoc.UniqueKey));
		}

		#region override
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			return new AdditionalDocumentMessageSendingObject(entryHeader);
		}

		JobDeclaration declaration;
		#endregion
	}
}
