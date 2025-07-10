using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin;
using AddressValidation = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base.AddressValidation;
using InvoiceType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice.InvoiceType.InvoiceTypes;

namespace Enterprise.Freight.Forwarding.Documents.CertificateOfOrigin.Testing
{
	sealed class ChaftaBuilderTest : CertificateOfOriginBuilderTests<Chafta, ChaftaLineItem, OriginCriterionList>
	{
		protected override CertificateOfOriginBuilder<Chafta, ChaftaLineItem, OriginCriterionList> CreateBuilder(ForwardingShipment shipment)
			=> new ChaftaBuilder(shipment);

		public void TestCriterionCode() => AssertEquals(OriginCriterionList.Codes.WO, new ChaftaBuilder(CreateShipmentCore()).GetDefaultOriginCriterionCode());

		public void TestHarmonisedCodeCountry() => AssertEquals(Core.Constants.CountryCodes.Australia, new ChaftaBuilder(CreateShipmentCore()).HarmonisedCodeCountry);

		public void TestShipmentDocumentName() => AssertEquals(ShipmentDocumentNames.ChaftaCertificateOfOrigin, new ChaftaBuilder(CreateShipmentCore()).ShipmentDocumentName);

		public void TestInitialFlags()
		{
			var builder = CreateBuilder(CreateShipmentCore());

			AssertEquals(AddressValidation.None, builder.InitialFlags.ValidateProducerAddress);

			AssertEquals(false, builder.InitialFlags.ValidateConsigneeAddress);
			AssertEquals(ContactValidation.Never, builder.InitialFlags.ValidateConsigneePhone);
			AssertEquals(ContactValidation.Never, builder.InitialFlags.ValidateConsigneeEmail);

			AssertEquals(false, builder.InitialFlags.ValidateConsignorPhone);
			AssertEquals(false, builder.InitialFlags.ValidateConsignorEmail);

			AssertEquals(false, builder.InitialFlags.ValidatePortOfLoading);
			AssertEquals(false, builder.InitialFlags.ValidatePortOfDischarge);
			AssertEquals(false, builder.InitialFlags.ValidatePortOfOrigin);
			AssertEquals(true, builder.InitialFlags.ValidatePortOfDestination);

			AssertEquals(false, builder.InitialFlags.ValidateTransportReference);

			AssertEquals(false, builder.InitialFlags.ValidateDepartureDate);
			AssertEquals(false, builder.InitialFlags.ValidateArrivalDate);

			AssertEquals(InvoiceType.Short, builder.InitialFlags.InvoiceType);

			AssertEquals(true, builder.InitialFlags.ValidateSignature);

			AssertEquals(false, builder.InitialFlags.ValidateLineItemOrigin);
			AssertEquals(true, builder.InitialFlags.ValidateLineItemOriginCriterion);
			AssertEquals(true, builder.InitialFlags.ValidateLineItemItemNumber);
		}
	}
}
