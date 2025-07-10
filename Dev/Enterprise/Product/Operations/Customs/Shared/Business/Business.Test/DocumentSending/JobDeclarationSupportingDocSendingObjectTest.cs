using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(JobDeclarationSupportingDocSendingObject))]
	public class JobDeclarationSupportingDocSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => sendingObject;

		public void TestDefaultValues()
		{
			AssertEquals(true, sendingObject.ShouldSend);
		}

		public void TestDocument()
		{
			sendingObject.EDoc = eDocOnDeclaration.UniqueKey;
			AssertEquals(eDocOnDeclaration.UniqueKey, sendingObject.Document.UniqueKey);
			AssertEquals(new ZDecimal(0.00000095367431640625), sendingObject.EDocFileSizeInMB);

			sendingObject.EDoc = eDocOnHeader.UniqueKey;
			AssertEquals(eDocOnHeader.UniqueKey, sendingObject.Document.UniqueKey);
			AssertEquals(new ZDecimal(0.0000019073486328125), sendingObject.EDocFileSizeInMB);

			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			var eDocOnShipment = shipment.DocManagerInfo.AddFileOrDocument(new byte[3], "InvoiceShipment.pdf", "CIV");

			sendingObject.EDoc = eDocOnShipment.UniqueKey;
			AssertEquals(eDocOnShipment.UniqueKey, sendingObject.Document.UniqueKey);
			AssertEquals(new ZDecimal(0.00000286102294921875), sendingObject.EDocFileSizeInMB);

			eDocOnShipment = shipment.DocManagerInfo.AddFileOrDocument(new byte[2097152], "InvoiceShipment1.pdf", "CIV");

			sendingObject.EDoc = eDocOnShipment.UniqueKey;
			AssertEquals(eDocOnShipment.UniqueKey, sendingObject.Document.UniqueKey);
			AssertEquals(new ZDecimal(2), sendingObject.EDocFileSizeInMB);
		}

		public void TestAvailableEDocs()
		{
			AssertType(ExpectedAvailableEDocListType, sendingObject.AvailableEDocs);
			AssertEquals(2, sendingObject.AvailableEDocs.Count);
			AssertEquals(eDocOnDeclaration.UniqueKey, sendingObject.AvailableEDocs[0].PK);
			AssertEquals(eDocOnHeader.UniqueKey, sendingObject.AvailableEDocs[1].PK);

			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			shipment.DocManagerInfo.AddFileOrDocument(new byte[1], "InvoiceShipment.pdf", "CIV");
			AssertEquals(3, sendingObject.AvailableEDocs.Count);

			var eDoc4 = shipment.DocManagerInfo.AddFileOrDocument(new byte[1], "InvoiceShipment2.pdf", "CIV", description: "InvoiceShipment2");
			AssertEquals(4, sendingObject.AvailableEDocs.Count);

			shipment.DocManagerInfo.AllEDocs.Remove(eDoc4);
			shipment.DocManagerInfo.AddFileOrDocument(new byte[1], "InvoiceShipment3.pdf", "VIC", description: "InvoiceShipment3");
			AssertEquals(4, sendingObject.AvailableEDocs.Count);
			AssertContains("InvoiceShipment3.pdf", sendingObject.AvailableEDocs.CodesAsString);
		}

		public virtual void TestDefaultLocalReferenceNumber()
		{
			var sendingObject = SupportingDocSendingObject.New(declaration);
			AssertEquals("12341234", sendingObject.LocalReferenceNumber);

			var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
			sendingObject = SupportingDocSendingObject.New(declaration);
			AssertEquals("12341234", sendingObject.LocalReferenceNumber);

			SetLocalReferenceNumberOnEntryHeader(entryHeader2, "99999");
			sendingObject = SupportingDocSendingObject.New(declaration);
			AssertEquals(string.Empty, sendingObject.LocalReferenceNumber);

			SetLocalReferenceNumberOnEntryHeader(entryHeader2, string.Empty);
			sendingObject = SupportingDocSendingObject.New(declaration);
			AssertEquals("12341234", sendingObject.LocalReferenceNumber);
		}

		public virtual void TestEntries()
		{
			var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
			entryHeader2.CH_BGMReference = "ABC";
			AssertEquals(2, sendingObject.Entries.Count);
			AssertContains("12341234", sendingObject.Entries[0].Code);
			AssertContains("ABC", sendingObject.Entries[1].Code);
			entryHeader2.CH_BGMReference = string.Empty;
			AssertEquals(1, sendingObject.Entries.Count);
		}

		public virtual void TestHeader()
		{
			AssertSame(entryHeader, sendingObject.Header);

			var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
			SetLocalReferenceNumberOnEntryHeader(entryHeader2, "23452345");

			sendingObject.LocalReferenceNumber = "23452345";
			AssertSame(entryHeader2, sendingObject.Header);

			sendingObject.LocalReferenceNumber = ZString.Empty;
			AssertNull(sendingObject.Header);

			sendingObject.LocalReferenceNumber = "12341234";
			AssertSame(entryHeader, sendingObject.Header);

			sendingObject.LocalReferenceNumber = "11111111";
			AssertNull(sendingObject.Header);
		}

		#region Implementation

		protected virtual Type ExpectedAvailableEDocListType => typeof(AvailableEDocList);

		protected virtual void SetLocalReferenceNumberOnEntryHeader(CusEntryHeader entryHeader, string number) => entryHeader.MovementReferenceNumberSetter(number, ZDateTime.Empty);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<BaseJobDeclaration>();
			eDocOnDeclaration = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");

			entryHeader = declaration.ActiveEntryHeaders.AddNew();
			SetLocalReferenceNumberOnEntryHeader(entryHeader, "12341234");
			eDocOnHeader = (entryHeader as IDocManagerSupport).DocManagerInfo.AddFileOrDocument(new byte[2], "Invoice.pdf", "DGF");

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			sendingObject = SupportingDocSendingObject.New(declaration) as JobDeclarationSupportingDocSendingObject;
			sendingObject.LocalReferenceNumber = "12341234";
		}

		protected BaseJobDeclaration declaration;
		protected CusEntryHeader entryHeader;
		protected CusEntryHeader entryHeader2;
		protected JobDeclarationSupportingDocSendingObject sendingObject;
		protected IeDoc eDocOnDeclaration;
		protected IeDoc eDocOnHeader;

		#endregion

	}
}
