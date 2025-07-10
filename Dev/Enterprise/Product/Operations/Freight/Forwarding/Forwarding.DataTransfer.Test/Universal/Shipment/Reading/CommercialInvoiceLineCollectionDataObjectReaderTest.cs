using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(CommercialInvoiceLineCollectionDataObjectReader))]
	public class CommercialInvoiceLineCollectionDataObjectReaderTest : DataObjectCollectionReaderTest
	{
		public override void TestReadIntoCollection()
		{
			var invoiceLineCollection = new List<CommercialInvoiceLine>();
			var invoiceLine1 = CreateCommercialInvoice(1m, "M3", "line1");
			var invoiceLine2 = CreateCommercialInvoice(2m, "M3", "line2");

			invoiceLineCollection.AddRange(new[] { invoiceLine1, invoiceLine2 });

			var shipment = Factory.New<ForwardingShipment>();
			var packingLine1 = shipment.OuterPackLines.AddNew();
			packingLine1.JL_PackageCount = 1;
			packingLine1.JL_F3_NKPackType = "M3";
			packingLine1.JL_Description = "line1";
			packingLine1.JL_Height = 13m;

			var packingLine2 = shipment.OuterPackLines.AddNew();
			packingLine2.JL_PackageCount = 3;
			packingLine2.JL_F3_NKPackType = "KG";
			packingLine2.JL_Description = "line2";
			packingLine2.JL_Height = 15m;

			var reader = new CommercialInvoiceLineCollectionDataObjectReader(invoiceLineCollection.ToArray(), shipment, logger, Factory);
			reader.ReadIntoCollection();

			AssertEquals(2, shipment.OuterPackLines.Count);

			AssertEquals(1, shipment.OuterPackLines[0].JL_PackageCount);
			AssertEquals("M3", shipment.OuterPackLines[0].JL_F3_NKPackType);
			AssertEquals("line1", shipment.OuterPackLines[0].JL_Description);
			AssertEquals(13m, shipment.OuterPackLines[0].JL_Height);

			AssertEquals(2, shipment.OuterPackLines[1].JL_PackageCount);
			AssertEquals("M3", shipment.OuterPackLines[1].JL_F3_NKPackType);
			AssertEquals("line2", shipment.OuterPackLines[1].JL_Description);
			AssertEquals(0m, shipment.OuterPackLines[1].JL_Height);
		}

		public void TestReadIntoCollection_MutilMatches()
		{
			var invoiceLineCollection = new List<CommercialInvoiceLine>();
			var invoiceLine1 = CreateCommercialInvoice(1m, "M3", "line1");

			invoiceLineCollection.Add(invoiceLine1);

			var shipment = Factory.New<ForwardingShipment>();
			var packingLine1 = shipment.OuterPackLines.AddNew();
			packingLine1.JL_PackageCount = 1;
			packingLine1.JL_F3_NKPackType = "M3";
			packingLine1.JL_Description = "line1";
			packingLine1.JL_Height = 13m;

			var packingLine2 = shipment.OuterPackLines.AddNew();
			packingLine2.JL_PackageCount = 1;
			packingLine2.JL_F3_NKPackType = "KG";
			packingLine2.JL_Description = "line1";
			packingLine2.JL_Height = 15m;

			var reader = new CommercialInvoiceLineCollectionDataObjectReader(invoiceLineCollection.ToArray(), shipment, logger, Factory);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);

			AssertEquals(1, shipment.OuterPackLines[0].JL_PackageCount);
			AssertEquals("M3", shipment.OuterPackLines[0].JL_F3_NKPackType);
			AssertEquals("line1", shipment.OuterPackLines[0].JL_Description);
			AssertEquals(13m, shipment.OuterPackLines[0].JL_Height);
		}

		public void TestReadIntoCollection_DuplicatedCommercialInvoiceLine()
		{
			var invoiceLineCollection = new List<CommercialInvoiceLine>();
			var invoiceLine1 = CreateCommercialInvoice(1m, "M3", "line");
			var invoiceLine2 = CreateCommercialInvoice(1m, "M3", "line");

			invoiceLineCollection.AddRange(new[] { invoiceLine1, invoiceLine2 });

			var shipment = Factory.New<ForwardingShipment>();
			var packingLine1 = shipment.OuterPackLines.AddNew();
			packingLine1.JL_PackageCount = 1;
			packingLine1.JL_F3_NKPackType = "M3";
			packingLine1.JL_Description = "line";
			packingLine1.JL_Height = 13m;

			var reader = new CommercialInvoiceLineCollectionDataObjectReader(invoiceLineCollection.ToArray(), shipment, logger, Factory);
			reader.ReadIntoCollection();

			AssertEquals(2, shipment.OuterPackLines.Count);

			AssertEquals(1, shipment.OuterPackLines[0].JL_PackageCount);
			AssertEquals("M3", shipment.OuterPackLines[0].JL_F3_NKPackType);
			AssertEquals("line", shipment.OuterPackLines[0].JL_Description);
			AssertEquals(0m, shipment.OuterPackLines[0].JL_Height);

			AssertEquals(1, shipment.OuterPackLines[1].JL_PackageCount);
			AssertEquals("M3", shipment.OuterPackLines[1].JL_F3_NKPackType);
			AssertEquals("line", shipment.OuterPackLines[1].JL_Description);
			AssertEquals(0m, shipment.OuterPackLines[1].JL_Height);
		}

		CommercialInvoiceLine CreateCommercialInvoice(ZDecimal quantity, ZString unit, ZString description)
		{
			var invoiceLine = new CommercialInvoiceLine();
			invoiceLine.InvoiceQuantity = quantity;
			invoiceLine.InvoiceQuantityUnit = new CodeDescriptionPair
			{
				Code = unit
			};

			invoiceLine.Description = description;

			return invoiceLine;
		}

		TestErrorLogger logger;

		protected override void SetUp()
		{
			logger = new TestErrorLogger();

			base.SetUp();
		}
	}
}
