using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.CodeLists;
using Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin;
using InvoiceType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice.InvoiceType.InvoiceTypes;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.CertificateOfOrigin
{
	sealed class IAECTABuilderTest : CertificateOfOriginBuilderTests<IAECTA, IAECTALineItem, OriginCriterionListIAECTA>
	{
		protected override CertificateOfOriginBuilder<IAECTA, IAECTALineItem, OriginCriterionListIAECTA> CreateBuilder(ForwardingShipment shipment) => new IAECTABuilder(shipment);

		public void TestCriterionCode() => AssertEquals(OriginCriterionListIAECTA.Codes.WO, new IAECTABuilder(CreateShipmentCore()).GetDefaultOriginCriterionCode());

		public void TestHarmonisedCodeCountry() => AssertEquals(ZString.Empty, new IAECTABuilder(CreateShipmentCore()).HarmonisedCodeCountry);

		public void TestShipmentDocumentName() => AssertEquals(ShipmentDocumentNames.IAECTACertificateOfOrigin, new IAECTABuilder(CreateShipmentCore()).ShipmentDocumentName);

		public void TestInitialFlags()
		{
			var builder = CreateBuilder(CreateShipmentCore());

			AssertEquals(AddressValidation.FullIfDifferentFromConsignor, builder.InitialFlags.ValidateProducerAddress);

			AssertEquals(false, builder.InitialFlags.ValidateConsigneeAddress);
			AssertEquals(ContactValidation.IfAddressSet, builder.InitialFlags.ValidateConsigneePhone);
			AssertEquals(ContactValidation.IfAddressSet, builder.InitialFlags.ValidateConsigneeEmail);

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

		public void Test_EmptyThirdPartyInvoiceIssuerThrowsValidationError_When_IsSubjectToThirdPartyInvoiceIsTrue()
		{
			var builder = CreateBuilder(CreateShipmentCore());
			var iaecta = builder.Build();

			// Not SubjectToThirdPartyInvoice
			OptionallyAssert_RemarksError(iaecta, string.Empty, isSubjectToThirdPartyInvoice: false, expectError: false);
			OptionallyAssert_RemarksError(iaecta, null, isSubjectToThirdPartyInvoice: false, expectError: false);
			OptionallyAssert_RemarksError(iaecta, "Third Party Invoice Issuer Company", isSubjectToThirdPartyInvoice: false, expectError: false);

			// Is SubjectToThirdPartyInvoice
			OptionallyAssert_RemarksError(iaecta, string.Empty, isSubjectToThirdPartyInvoice: true, expectError: true);
			OptionallyAssert_RemarksError(iaecta, null, isSubjectToThirdPartyInvoice: true, expectError: true);
			OptionallyAssert_RemarksError(iaecta, "Third Party Invoice Issuer Company", isSubjectToThirdPartyInvoice: true, expectError: false);
		}

		void OptionallyAssert_RemarksError(IAECTA iaecta, string thirdPartyInvoiceIssuer, bool isSubjectToThirdPartyInvoice, bool expectError)
		{
			iaecta.ThirdPartyInvoiceIssuer = thirdPartyInvoiceIssuer;
			iaecta.IsSubjectOfThirdPartyInvoice = isSubjectToThirdPartyInvoice;

			if (expectError)
			{
				AssertHasMessageError(iaecta.ThirdPartyInvoiceIssuerInfo, "Third party invoice issuer details are required.");
			}
			else
			{
				AssertNoMessageErrors(iaecta.ThirdPartyInvoiceIssuerInfo);
			}
		}
	}
}
