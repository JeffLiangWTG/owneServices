using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.CodeLists;
using Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin;
using InvoiceType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice.InvoiceType.InvoiceTypes;

namespace Enterprise.Freight.Forwarding.Documents.CertificateOfOrigin.Testing
{
	sealed class AUKFTABuilderTest : CertificateOfOriginBuilderTests<AUKFTA, AUKFTALineItem, OriginCriterionListAUKFTA>
	{
		protected override CertificateOfOriginBuilder<AUKFTA, AUKFTALineItem, OriginCriterionListAUKFTA> CreateBuilder(ForwardingShipment shipment) => new AUKFTABuilder(shipment);

		public void TestCriterionCode() => AssertEquals(OriginCriterionListAUKFTA.Codes.WO, new AUKFTABuilder(CreateShipmentCore()).GetDefaultOriginCriterionCode());

		public void TestHarmonisedCodeCountry() => AssertEquals(ZString.Empty, new AUKFTABuilder(CreateShipmentCore()).HarmonisedCodeCountry);

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
			AssertEquals(false, builder.InitialFlags.ValidatePortOfOrigin);
			AssertEquals(false, builder.InitialFlags.ValidatePortOfDestination);

			AssertEquals(false, builder.InitialFlags.ValidateTransportReference);

			AssertEquals(false, builder.InitialFlags.ValidateDepartureDate);
			AssertEquals(false, builder.InitialFlags.ValidateArrivalDate);

			AssertEquals(InvoiceType.None, builder.InitialFlags.InvoiceType);

			AssertEquals(true, builder.InitialFlags.ValidateSignature);

			AssertEquals(false, builder.InitialFlags.ValidateLineItemOrigin);
			AssertEquals(true, builder.InitialFlags.ValidateLineItemOriginCriterion);
			AssertEquals(true, builder.InitialFlags.ValidateLineItemItemNumber);
		}

		public void Test_Validate_ReturnsErrorMessage_When_DeclarationCompletedByIsEmpty()
		{
			var shipmentAUGB = CreateShipmentCore(createConsol: true, originCountry: Constants.CountryCodes.Australia, destinationCountry: Constants.CountryCodes.UnitedKingdom);
			var aukfta = CreateBuilder(shipmentAUGB).Build();

			//asserting message error on initial build
			AssertHasMessageError(aukfta.DeclarationCompletedBy.ValueInfo, "You must indicate who the declaration was completed by.");

			//Setting one flag as true and checking if no message error
			aukfta.DeclarationCompletedBy.IsExporter = true;
			AssertNoMessageErrors(aukfta.DeclarationCompletedBy.ValueInfo);

			//Again setting the same flag as false and asserting message error
			aukfta.DeclarationCompletedBy.IsExporter = false;
			AssertHasMessageError(aukfta.DeclarationCompletedBy.ValueInfo, "You must indicate who the declaration was completed by.");
		}
	}
}
