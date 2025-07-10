using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin;
using InvoiceType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice.InvoiceType.InvoiceTypes;

namespace Enterprise.Freight.Forwarding.Documents.CertificateOfOrigin.Testing
{
	sealed class COONZBuilderTests : CertificateOfOriginBuilderTests<COONZ, COONZLineItem, OriginCriterionListCOONZ>
	{
		protected override CertificateOfOriginBuilder<COONZ, COONZLineItem, OriginCriterionListCOONZ> CreateBuilder(ForwardingShipment shipment) => new COONZBuilder(shipment);

		public void TestCriterionCode() => AssertEquals(OriginCriterionList.Codes.WO, new COONZBuilder(CreateShipmentCore()).GetDefaultOriginCriterionCode());

		public void TestHarmonisedCodeCountry() => AssertEquals(Core.Constants.CountryCodes.NewZealand, new COONZBuilder(CreateShipmentCore()).HarmonisedCodeCountry);

		public void TestShipmentDocumentName() => AssertEquals(ShipmentDocumentNames.NZCertificateOfOrigin, new COONZBuilder(CreateShipmentCore()).ShipmentDocumentName);

		public void TestInitialFlags()
		{
			var builder = CreateBuilder(CreateShipmentCore());

			AssertEquals(AddressValidation.None, builder.InitialFlags.ValidateProducerAddress);

			AssertEquals(true, builder.InitialFlags.ValidateConsigneeAddress);
			AssertEquals(ContactValidation.Never, builder.InitialFlags.ValidateConsigneePhone);
			AssertEquals(ContactValidation.Never, builder.InitialFlags.ValidateConsigneeEmail);

			AssertEquals(false, builder.InitialFlags.ValidateConsignorPhone);
			AssertEquals(false, builder.InitialFlags.ValidateConsignorEmail);

			AssertEquals(false, builder.InitialFlags.ValidatePortOfLoading);
			AssertEquals(false, builder.InitialFlags.ValidatePortOfDischarge);
			AssertEquals(false, builder.InitialFlags.ValidatePortOfOrigin);
			AssertEquals(false, builder.InitialFlags.ValidatePortOfDestination);

			AssertEquals(false, builder.InitialFlags.ValidateTransportReference);

			AssertEquals(false, builder.InitialFlags.ValidateDepartureDate);
			AssertEquals(false, builder.InitialFlags.ValidateArrivalDate);

			AssertEquals(InvoiceType.None, builder.InitialFlags.InvoiceType);

			AssertEquals(true, builder.InitialFlags.ValidateSignature);

			AssertEquals(true, builder.InitialFlags.ValidateLineItemOrigin);
			AssertEquals(false, builder.InitialFlags.ValidateLineItemOriginCriterion);
			AssertEquals(false, builder.InitialFlags.ValidateLineItemItemNumber);
		}

		public void Test_PacklineOriginCountries_TwoCountries()
		{
			var shipment = CreateShipmentCore();
			CreateLineItemsCore(shipment, addHarmonizedCodeForCountry: true);
			CreateLineItemsCore(shipment, addHarmonizedCodeForCountry: true, originCode: "US");

			var builder = CreateBuilder(shipment);
			var certificate = builder.Build();

			AssertNotNull(certificate);
			AssertEquals("PacklineOriginCountries", "New Zealand, United States", certificate.LineItemOriginCountries);
		}

		public void Test_PacklineOriginCountries_OneCountry()
		{
			var shipment = CreateShipmentCore();
			CreateLineItemsCore(shipment, addHarmonizedCodeForCountry: true);

			var builder = CreateBuilder(shipment);
			var certificate = builder.Build();

			AssertNotNull(certificate);
			AssertEquals("PacklineOriginCountries", "New Zealand", certificate.LineItemOriginCountries);
		}
	}
}
