using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.CodeLists;
using Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin;
using AddressValidation = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base.AddressValidation;
using InvoiceType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice.InvoiceType.InvoiceTypes;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.CertificateOfOrigin
{
	sealed class KAFTABuilderTest : CertificateOfOriginBuilderTests<KAFTA, KAFTALineItem, OriginCriterionListKAFTA>
	{
		protected override CertificateOfOriginBuilder<KAFTA, KAFTALineItem, OriginCriterionListKAFTA> CreateBuilder(ForwardingShipment shipment) => new KAFTABuilder(shipment);

		public void TestCriterionCode() => AssertEquals(OriginCriterionListKAFTA.Codes.WO, new KAFTABuilder(CreateShipmentCore()).GetDefaultOriginCriterionCode());

		public void TestHarmonisedCodeCountry() => AssertEquals(ZString.Empty, new KAFTABuilder(CreateShipmentCore()).HarmonisedCodeCountry);

		public void TestShipmentDocumentName() => AssertEquals(ShipmentDocumentNames.KAFTACertificateOfOrigin, new KAFTABuilder(CreateShipmentCore()).ShipmentDocumentName);

		public void TestInitialFlags()
		{
			var builder = CreateBuilder(CreateShipmentCore());

			AssertEquals(AddressValidation.None, builder.InitialFlags.ValidateProducerAddress);
			AssertEquals(ContactValidation.IfAddressSet, builder.InitialFlags.ValidateProducerEmail);
			AssertEquals(ContactValidation.IfAddressSet, builder.InitialFlags.ValidateProducerPhone);

			AssertEquals(ContactValidation.IfAddressSet, builder.InitialFlags.ValidateConsigneeEmail);
			AssertEquals(ContactValidation.IfAddressSet, builder.InitialFlags.ValidateConsigneePhone);

			AssertEquals(true, builder.InitialFlags.ValidateConsignorEmail);
			AssertEquals(true, builder.InitialFlags.ValidateConsignorPhone);

			AssertEquals(false, builder.InitialFlags.ValidatePortOfLoading);
			AssertEquals(false, builder.InitialFlags.ValidatePortOfDischarge);
			AssertEquals(true, builder.InitialFlags.ValidatePortOfOrigin);
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
