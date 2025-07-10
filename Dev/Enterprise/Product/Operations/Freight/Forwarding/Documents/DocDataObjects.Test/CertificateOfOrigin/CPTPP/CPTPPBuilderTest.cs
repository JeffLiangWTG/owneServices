using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin;
using AddressValidation = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base.AddressValidation;
using InvoiceType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice.InvoiceType.InvoiceTypes;

namespace Enterprise.Freight.Forwarding.Documents.CertificateOfOrigin.Testing
{
	sealed class CPTPPBuilderTest : CertificateOfOriginBuilderTests<CPTPP, CPTPPLineItem, OriginCriterionListCPTPP>
	{
		protected override CertificateOfOriginBuilder<CPTPP, CPTPPLineItem, OriginCriterionListCPTPP> CreateBuilder(ForwardingShipment shipment) => new CPTPPBuilder(shipment);

		public void TestCriterionCode() => AssertEquals(OriginCriterionListCPTPP.Codes.WO, new CPTPPBuilder(CreateShipmentCore()).GetDefaultOriginCriterionCode());

		public void TestHarmonisedCodeCountry() => AssertEquals(ZString.Empty, new CPTPPBuilder(CreateShipmentCore()).HarmonisedCodeCountry);

		public void TestShipmentDocumentName() => AssertEquals(ShipmentDocumentNames.CPTPPCertificateOfOrigin, new CPTPPBuilder(CreateShipmentCore()).ShipmentDocumentName);

		public void TestInitialFlags()
		{
			var builder = CreateBuilder(CreateShipmentCore());

			AssertEquals(AddressValidation.None, builder.InitialFlags.ValidateProducerAddress);

			AssertEquals(true, builder.InitialFlags.ValidateConsigneeAddress);
			AssertEquals(ContactValidation.Always, builder.InitialFlags.ValidateConsigneePhone);
			AssertEquals(ContactValidation.Always, builder.InitialFlags.ValidateConsigneeEmail);

			AssertEquals(true, builder.InitialFlags.ValidateConsignorPhone);
			AssertEquals(true, builder.InitialFlags.ValidateConsignorEmail);

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
