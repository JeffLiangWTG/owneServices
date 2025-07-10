using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class CommercialInvoiceLineDataObjectReaderTest : TestCaseWithUniversalObjectFactory
	{
		public void TestAddPackLineFromDataObject()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var invoiceLine = new CommercialInvoiceLine
			{
				Description = "Guitars",
				InvoiceQuantity = 11.2m,
				InvoiceQuantityUnit = new CodeDescriptionPair { Code = "BOT", Description = "Bottle" },
				Weight = 23.4m,
				WeightUnit = new UnitOfWeight { Code = "KT", Description = "Kilotons" }
			};

			var reader = new CommercialInvoiceLineDataObjectReader(invoiceLine, shipment, new DummyLogger(), Factory);
			var packLine = reader.ReadIntoBusinessObject();

			AssertEquals(shipment.PK, packLine.JL_JS);
			AssertEquals("Guitars", packLine.JL_Description);
			AssertEquals(11, packLine.JL_PackageCount);
			AssertEquals("BOT", packLine.JL_F3_NKPackType);
			AssertEquals(23.4m, packLine.JL_ActualWeight);
			AssertEquals("KT", packLine.JL_ActualWeightUQ);
		}

		public void TestCreateNewPacklineBySpecifiedFactory()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var invoiceLine = new CommercialInvoiceLine
			{
				Description = "Guitars",
				InvoiceQuantity = 11.2m,
				InvoiceQuantityUnit = new CodeDescriptionPair { Code = "BOT", Description = "Bottle" },
				Weight = 23.4m,
				WeightUnit = new UnitOfWeight { Code = "KT", Description = "Kilotons" }
			};

			var anotherFactory = new BusinessObjectFactory();
			var reader = new CommercialInvoiceLineDataObjectReader(invoiceLine, shipment, new DummyLogger(), anotherFactory);
			var packLine = reader.ReadIntoBusinessObject();

			AssertEquals(shipment.PK, packLine.JL_JS);
			AssertEquals("Guitars", packLine.JL_Description);
			Assert(!packLine.IsInDatabase);
			AssertNotNull("packLine is created by anotherFactory", anotherFactory.Load<ForwardingPackLine>(packLine.PK));
		}
	}
}
