using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin;
using InvoiceType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice.InvoiceType.InvoiceTypes;

namespace Enterprise.Freight.Forwarding.Documents.CertificateOfOrigin.Testing
{
	sealed class AanzftaBuilderTest : CertificateOfOriginBuilderTests<Aanzfta, AanzftaLineItem, OriginCriterionListAanzfta>
	{
		protected override CertificateOfOriginBuilder<Aanzfta, AanzftaLineItem, OriginCriterionListAanzfta> CreateBuilder(ForwardingShipment shipment) => new AanzftaBuilder(shipment);

		public void TestCriterionCode() => AssertEquals(OriginCriterionList.Codes.WO, new AanzftaBuilder(CreateShipmentCore()).GetDefaultOriginCriterionCode());

		public void TestHarmonisedCodeCountry() => AssertEquals(string.Empty, new AanzftaBuilder(CreateShipmentCore()).HarmonisedCodeCountry);

		public void TestShipmentDocumentName() => AssertEquals(ShipmentDocumentNames.AANZFTACertificateOfOrigin, new AanzftaBuilder(CreateShipmentCore()).ShipmentDocumentName);
		public void TestInitialFlags()
		{
			TestInitialFlags_DepartureDateValidation_DependentOnOrigin(Constants.CountryCodes.NewZealand, true);
			TestInitialFlags_DepartureDateValidation_DependentOnOrigin(Constants.CountryCodes.Australia, false);
		}

		public void Test_EmptyThirdPartyInvoiceIssuerThrowsValidationError_When_IsSubjectToThirdPartyInvoiceIsTrue()
		{
			var builder = CreateBuilder(CreateShipmentCore());
			var aanzfta = builder.Build();

			// Not SubjectToThirdPartyInvoice
			OptionallyAssert_ThirdPartyIssuerError(aanzfta, string.Empty, isSubjectToThirdPartyInvoice: false, expectError: false);
			OptionallyAssert_ThirdPartyIssuerError(aanzfta, null, isSubjectToThirdPartyInvoice: false, expectError: false);
			OptionallyAssert_ThirdPartyIssuerError(aanzfta, "Third Party Invoice Issuer Company", isSubjectToThirdPartyInvoice: false, expectError: false);

			// Is SubjectToThirdPartyInvoice
			OptionallyAssert_ThirdPartyIssuerError(aanzfta, string.Empty, isSubjectToThirdPartyInvoice: true, expectError: true);
			OptionallyAssert_ThirdPartyIssuerError(aanzfta, null, isSubjectToThirdPartyInvoice: true, expectError: true);
			OptionallyAssert_ThirdPartyIssuerError(aanzfta, "Third Party Invoice Issuer Company", isSubjectToThirdPartyInvoice: true, expectError: false);
		}

		void OptionallyAssert_ThirdPartyIssuerError(Aanzfta aanzfta, string thirdPartyInvoiceIssuer, bool isSubjectToThirdPartyInvoice, bool expectError)
		{
			aanzfta.ThirdPartyInvoiceIssuer = thirdPartyInvoiceIssuer;
			aanzfta.IsSubjectOfThirdPartyInvoice = isSubjectToThirdPartyInvoice;

			if (expectError)
			{
				AssertHasMessageError(aanzfta.ThirdPartyInvoiceIssuerInfo, "Third party invoice issuer details are required.");
			}
			else
			{
				AssertNoMessageErrors(aanzfta.ThirdPartyInvoiceIssuerInfo);
			}
		}

		void TestInitialFlags_DepartureDateValidation_DependentOnOrigin(string originCountry, bool shouldDepartureDateBeValidated)
		{
			var builder = CreateBuilder(CreateShipmentCore(originCountry: originCountry));

			AssertEquals(AddressValidation.None, builder.InitialFlags.ValidateProducerAddress);

			AssertEquals(true, builder.InitialFlags.ValidateConsigneeAddress);
			AssertEquals(ContactValidation.Never, builder.InitialFlags.ValidateConsigneePhone);
			AssertEquals(ContactValidation.Never, builder.InitialFlags.ValidateConsigneeEmail);

			AssertEquals(false, builder.InitialFlags.ValidateConsignorPhone);
			AssertEquals(false, builder.InitialFlags.ValidateConsignorEmail);

			AssertEquals(false, builder.InitialFlags.ValidatePortOfLoading);
			AssertEquals(false, builder.InitialFlags.ValidatePortOfDischarge);
			AssertEquals(true, builder.InitialFlags.ValidatePortOfOrigin);
			AssertEquals(true, builder.InitialFlags.ValidatePortOfDestination);

			AssertEquals(false, builder.InitialFlags.ValidateTransportReference);

			AssertEquals(shouldDepartureDateBeValidated, builder.InitialFlags.ValidateDepartureDate);
			AssertEquals(false, builder.InitialFlags.ValidateArrivalDate);

			AssertEquals(InvoiceType.Short, builder.InitialFlags.InvoiceType);

			AssertEquals(true, builder.InitialFlags.ValidateSignature);

			AssertEquals(false, builder.InitialFlags.ValidateLineItemOrigin);
			AssertEquals(true, builder.InitialFlags.ValidateLineItemOriginCriterion);
			AssertEquals(true, builder.InitialFlags.ValidateLineItemItemNumber);
		}
	}
}
