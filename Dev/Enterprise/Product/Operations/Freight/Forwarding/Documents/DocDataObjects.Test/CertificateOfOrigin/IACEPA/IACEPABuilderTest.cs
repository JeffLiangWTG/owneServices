using System;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin;
using InvoiceType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice.InvoiceType.InvoiceTypes;

namespace Enterprise.Freight.Forwarding.Documents.CertificateOfOrigin.Testing
{
	sealed class IACEPABuilderTest : CertificateOfOriginBuilderTests<IACEPA, IACEPALineItem, OriginCriterionListIACEPA>
	{
		protected override CertificateOfOriginBuilder<IACEPA, IACEPALineItem, OriginCriterionListIACEPA> CreateBuilder(ForwardingShipment shipment) => new IACEPABuilder(shipment);

		public void TestCriterionCode() => AssertEquals(OriginCriterionListIACEPA.Codes.WO, new IACEPABuilder(CreateShipmentCore()).GetDefaultOriginCriterionCode());

		public void TestHarmonisedCodeCountry() => AssertEquals(ZString.Empty, new IACEPABuilder(CreateShipmentCore()).HarmonisedCodeCountry);

		public void TestInitialFlags()
		{
			var builder = CreateBuilder(CreateShipmentCore());

			AssertEquals(AddressValidation.None, builder.InitialFlags.ValidateProducerAddress);

			AssertEquals(true, builder.InitialFlags.ValidateConsigneeAddress);
			AssertEquals(ContactValidation.Never, builder.InitialFlags.ValidateConsigneePhone);
			AssertEquals(ContactValidation.Never, builder.InitialFlags.ValidateConsigneeEmail);

			AssertEquals(false, builder.InitialFlags.ValidateConsignorPhone);
			AssertEquals(true, builder.InitialFlags.ValidateConsignorEmail);

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
			AssertEquals(false, builder.InitialFlags.ValidateLineItemWeight);
			AssertEquals(false, builder.InitialFlags.ValidateLineItemMarksAndNumbers);
		}

		public void TestShipmentDocumentName() => AssertEquals("IACEPA Certificate of Origin", new IACEPABuilder(CreateShipmentCore()).ShipmentDocumentName);

		public void TestValidationOnThirdPartyInvoiceDetail()
		{
			var builder = CreateBuilder(CreateShipmentCore());
			var iacepa = builder.Build();

			OptionallyAssert_ThirdPartyInvoiceDetailError(iacepa, string.Empty, isSubjectOfThirdPartyInvoice: false, expectError: false);
			OptionallyAssert_ThirdPartyInvoiceDetailError(iacepa, null, isSubjectOfThirdPartyInvoice: false, expectError: false);
			OptionallyAssert_ThirdPartyInvoiceDetailError(iacepa, "Third Party Invoice Detail", isSubjectOfThirdPartyInvoice: false, expectError: false);

			OptionallyAssert_ThirdPartyInvoiceDetailError(iacepa, string.Empty, isSubjectOfThirdPartyInvoice: true, expectError: true);
			OptionallyAssert_ThirdPartyInvoiceDetailError(iacepa, null, isSubjectOfThirdPartyInvoice: true, expectError: true);
			OptionallyAssert_ThirdPartyInvoiceDetailError(iacepa, "Third Party Invoice Detail", isSubjectOfThirdPartyInvoice: true, expectError: false);
		}

		public void TestValidationOnExhibitionDetail()
		{
			var builder = CreateBuilder(CreateShipmentCore());
			var iacepa = builder.Build();

			OptionallyAssert_ExhibitionDetailError(iacepa, string.Empty, isExhibition: false, expectError: false);
			OptionallyAssert_ExhibitionDetailError(iacepa, null, isExhibition: false, expectError: false);
			OptionallyAssert_ExhibitionDetailError(iacepa, "Exhibition Detail", isExhibition: false, expectError: false);

			OptionallyAssert_ExhibitionDetailError(iacepa, string.Empty, isExhibition: true, expectError: true);
			OptionallyAssert_ExhibitionDetailError(iacepa, null, isExhibition: true, expectError: true);
			OptionallyAssert_ExhibitionDetailError(iacepa, "Exhibition Detail", isExhibition: true, expectError: false);
		}

		void OptionallyAssert_ThirdPartyInvoiceDetailError(IACEPA iacepa, string thirdPartyInvoiceDetail, bool isSubjectOfThirdPartyInvoice, bool expectError)
		{
			iacepa.IsSubjectOfThirdPartyInvoice = isSubjectOfThirdPartyInvoice;
			iacepa.ThirdPartyInvoiceIssuer = thirdPartyInvoiceDetail;

			if (expectError)
			{
				AssertHasMessageError(iacepa.ThirdPartyInvoiceIssuerInfo, "Third Party Invoice detail is required.");
			}
			else
			{
				AssertNoMessageErrors(iacepa.ThirdPartyInvoiceIssuerInfo);
			}
		}

		void OptionallyAssert_ExhibitionDetailError(IACEPA iacepa, String exhibitionDetail, bool isExhibition, bool expectError)
		{
			iacepa.IsExhibition = isExhibition;
			iacepa.ExhibitionDetail = exhibitionDetail;

			if (expectError)
			{
				AssertHasMessageError(iacepa.ExhibitionDetailInfo, "Exhibition detail is required.");
			}
			else
			{
				AssertNoMessageErrors(iacepa.ExhibitionDetailInfo);
			}
		}
	}
}
