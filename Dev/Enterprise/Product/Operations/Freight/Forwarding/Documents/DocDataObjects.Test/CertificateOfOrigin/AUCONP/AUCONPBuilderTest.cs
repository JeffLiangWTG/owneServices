using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.CodeLists;
using Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin;
using InvoiceType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice.InvoiceType.InvoiceTypes;

namespace Enterprise.Freight.Forwarding.Documents.CertificateOfOrigin.Testing
{
	sealed class AUCONPBuilderTest : CertificateOfOriginBuilderTests<AUCONP, AUCONPLineItem, OriginCriterionListEmpty>
	{
		protected override CertificateOfOriginBuilder<AUCONP, AUCONPLineItem, OriginCriterionListEmpty> CreateBuilder(ForwardingShipment shipment) => new AUCONPBuilder(shipment);

		public void TestCriterionCode() => AssertEquals(string.Empty, new AUCONPBuilder(CreateShipmentCore()).GetDefaultOriginCriterionCode());

		public void TestHarmonisedCodeCountry() => AssertEquals(ZString.Empty, new AUCONPBuilder(CreateShipmentCore()).HarmonisedCodeCountry);

		public void TestShipmentDocumentName() => AssertEquals(ShipmentDocumentNames.AUCONPCertificateOfOrigin, new AUCONPBuilder(CreateShipmentCore()).ShipmentDocumentName);

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
			AssertEquals(true, builder.InitialFlags.ValidatePortOfDestination);

			AssertEquals(false, builder.InitialFlags.ValidateTransportReference);

			AssertEquals(false, builder.InitialFlags.ValidateDepartureDate);
			AssertEquals(false, builder.InitialFlags.ValidateArrivalDate);

			AssertEquals(InvoiceType.None, builder.InitialFlags.InvoiceType);

			AssertEquals(false, builder.InitialFlags.ValidateSignature);

			AssertEquals(false, builder.InitialFlags.ValidateLineItemOrigin);
			AssertEquals(false, builder.InitialFlags.ValidateLineItemOriginCriterion);
			AssertEquals(true, builder.InitialFlags.ValidateLineItemItemNumber);
			AssertEquals(true, builder.InitialFlags.ValidateLineItemWeight);
		}
	}
}
