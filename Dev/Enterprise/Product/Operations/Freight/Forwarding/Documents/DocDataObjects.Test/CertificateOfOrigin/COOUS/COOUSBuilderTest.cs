using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.CodeLists;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice;
using Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin;
using COOUSCertificate = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.COOUS;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.CertificateOfOrigin.COOUS
{
	sealed class COOUSBuilderTest : CertificateOfOriginBuilderTests<COOUSCertificate, COOUSLineItem, OriginCriterionListEmpty>
	{
		protected override CertificateOfOriginBuilder<COOUSCertificate, COOUSLineItem, OriginCriterionListEmpty> CreateBuilder(ForwardingShipment shipment) => new COOUSBuilder(shipment);

		public void TestCriterionCode() => AssertEquals(string.Empty, new COOUSBuilder(CreateShipmentCore()).GetDefaultOriginCriterionCode());

		public void TestShipmentDocumentName() => AssertEquals("COOUS Certificate of Origin", new COOUSBuilder(CreateShipmentCore()).ShipmentDocumentName);

		public void TestInitialFlags()
		{
			var builder = CreateBuilder(CreateShipmentCore());

			AssertEquals(ContactValidation.Never, builder.InitialFlags.ValidateProducerPhone);
			AssertEquals(ContactValidation.Never, builder.InitialFlags.ValidateProducerEmail);
			AssertEquals(AddressValidation.None, builder.InitialFlags.ValidateProducerAddress);

			AssertEquals(false, builder.InitialFlags.ValidateConsignorPhone);
			AssertEquals(false, builder.InitialFlags.ValidateConsignorEmail);

			AssertEquals(ContactValidation.Never, builder.InitialFlags.ValidateConsigneePhone);
			AssertEquals(ContactValidation.Never, builder.InitialFlags.ValidateConsigneeEmail);
			AssertEquals(true, builder.InitialFlags.ValidateConsigneeAddress);

			AssertEquals(false, builder.InitialFlags.ValidatePortOfLoading);
			AssertEquals(false, builder.InitialFlags.ValidatePortOfDischarge);
			AssertEquals(false, builder.InitialFlags.ValidatePortOfOrigin);
			AssertEquals(false, builder.InitialFlags.ValidatePortOfDestination);

			AssertEquals(false, builder.InitialFlags.ValidateTransportReference);

			AssertEquals(false, builder.InitialFlags.ValidateDepartureDate);
			AssertEquals(false, builder.InitialFlags.ValidateArrivalDate);

			AssertEquals(InvoiceType.InvoiceTypes.None, builder.InitialFlags.InvoiceType);

			AssertEquals(true, builder.InitialFlags.ValidateSignature);

			AssertEquals(false, builder.InitialFlags.ValidateLineHarmonizedCode);

			AssertEquals(true, builder.InitialFlags.ValidateLineItemOrigin);
			AssertEquals(false, builder.InitialFlags.ValidateLineItemOriginCriterion);
			AssertEquals(true, builder.InitialFlags.ValidateLineItemItemNumber);
			AssertEquals(true, builder.InitialFlags.ValidateLineItemWeight);

			AssertEquals(false, builder.InitialFlags.ValidateCurrentUserCity);
		}

		public void Test_ShipmentNumber_ShouldBePopulated()
		{
			var shipment = CreateShipmentCore();
			var builder = CreateBuilder(shipment);

			var coous = builder.Build();

			AssertNotNull(coous);
			AssertNotNull(coous.ShipmentNumber);
		}

		public void Test_CountryOfOriginCollection_ShouldBePopulated_WithUniqueValues()
		{
			var shipment = CreateShipmentCore();
			CreateLineItemsCore(shipment, numberOfLineItems: 2);
			CreateLineItemsCore(shipment, originCode: Constants.CountryCodes.UnitedStates, numberOfLineItems: 2);
			var builder = CreateBuilder(shipment);

			var coous = builder.Build();

			var lineItemOriginCountries = coous.LineItemOriginCountries;

			AssertNotNull(coous);
			AssertNotNull(lineItemOriginCountries);

			AssertEquals("LineItemOriginCountries should contain only unique values", "New Zealand, United States", lineItemOriginCountries);
		}

		public void Test_Validation_ShipmentNumber()
		{
			var shipment = CreateShipmentCore();
			CreateLineItemsCore(shipment, originCode: Constants.CountryCodes.UnitedStates);
			var builder = CreateBuilder(shipment);

			var coous = builder.Build();

			AssertNoMessageErrors(coous.ShipmentNumberInfo);

			coous.ShipmentNumber = null;
			AssertHasMessageError(coous.ShipmentNumberInfo, "Shipment Number is required");
		}
	}
}
