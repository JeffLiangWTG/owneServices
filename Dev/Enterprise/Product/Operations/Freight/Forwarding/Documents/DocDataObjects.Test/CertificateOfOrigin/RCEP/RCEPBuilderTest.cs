using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin;
using Enterprise.MasterFiles.Business;
using AddressValidation = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base.AddressValidation;
using InvoiceType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice.InvoiceType.InvoiceTypes;

namespace Enterprise.Freight.Forwarding.Documents.CertificateOfOrigin.Testing
{
	sealed class RCEPBuilderTest : CertificateOfOriginBuilderTests<RCEP, RCEPLineItem, OriginCriterionListRCEP>
	{
		protected override CertificateOfOriginBuilder<RCEP, RCEPLineItem, OriginCriterionListRCEP> CreateBuilder(ForwardingShipment shipment) => new RCEPBuilder(shipment);

		public void TestCriterionCode() => AssertEquals(OriginCriterionListRCEP.Codes.WO, new RCEPBuilder(CreateShipmentCore()).GetDefaultOriginCriterionCode());

		public void TestHarmonisedCodeCountry() => AssertEquals(ZString.Empty, new RCEPBuilder(CreateShipmentCore()).HarmonisedCodeCountry);

		public void TestShipmentDocumentName() => AssertEquals(ShipmentDocumentNames.RCEPCertificateOfOrigin, new RCEPBuilder(CreateShipmentCore()).ShipmentDocumentName);

		public void TestInitialFlags()
		{
			var builder = CreateBuilder(CreateShipmentCore());

			AssertEquals(AddressValidation.Complex, builder.InitialFlags.ValidateProducerAddress);

			AssertEquals(true, builder.InitialFlags.ValidateConsigneeAddress);
			AssertEquals(ContactValidation.Never, builder.InitialFlags.ValidateConsigneePhone);
			AssertEquals(ContactValidation.Never, builder.InitialFlags.ValidateConsigneeEmail);

			AssertEquals(false, builder.InitialFlags.ValidateConsignorPhone);
			AssertEquals(false, builder.InitialFlags.ValidateConsignorEmail);

			AssertEquals(true, builder.InitialFlags.ValidatePortOfLoading);
			AssertEquals(true, builder.InitialFlags.ValidatePortOfDischarge);
			AssertEquals(false, builder.InitialFlags.ValidatePortOfOrigin);
			AssertEquals(true, builder.InitialFlags.ValidatePortOfDestination);

			AssertEquals(true, builder.InitialFlags.ValidateTransportReference);

			AssertEquals(true, builder.InitialFlags.ValidateDepartureDate);
			AssertEquals(false, builder.InitialFlags.ValidateArrivalDate);

			AssertEquals(InvoiceType.Short, builder.InitialFlags.InvoiceType);

			AssertEquals(true, builder.InitialFlags.ValidateSignature);

			AssertEquals(true, builder.InitialFlags.ValidateLineItemOrigin);
			AssertEquals(true, builder.InitialFlags.ValidateLineItemOriginCriterion);
			AssertEquals(true, builder.InitialFlags.ValidateLineItemItemNumber);
		}

		protected override string InvalidPacklineOriginError => "RCEP Country/Region of Origin is required.";

		public void TestBuild_RCEP()
		{
			var rcep = CreateBuilder(CreateShipmentCore()).Build();

			AssertNotNull(rcep);
			AssertEquals("RCEP", rcep.AddressCollection.CertificateName);
		}

		public override void TestPorts()
		{
			var certificate = CreateBuilder(CreateShipmentCore()).Build();
			AssertNotNull(certificate);

			CombineAssertions(() =>
			{
				AssertEquals("PortOfLoading", "NZAKL", certificate.PortOfLoading.Code);
				AssertEquals("PortOfDischarge", "CNSZX", certificate.PortOfDischarge.Code);
				AssertEquals("PortOfOrigin", "NZAKL", certificate.PortOfOrigin.Code);
				AssertEquals("PortOfDestination", "CNSZX", certificate.PortOfDestination.Code);
			});
		}

		public void Test_Validation_IsSubjectOfThirdPartyInvoice_RemarksRequired()
		{
			var certificate = CreateBuilder(CreateShipmentCore()).Build();

			AssertNoMessageErrors(certificate.RemarksInfo);

			certificate.IsSubjectOfThirdPartyInvoice = true;
			certificate.Remarks = ZString.Empty;

			AssertHasMessageError(certificate.RemarksInfo, "The name and country/region of the company issuing the invoice should be provided in Remarks.");

			certificate.RemarksInfo.ClearAllNotifications();
			certificate.Remarks = "Something";
			certificate.Remarks = null;

			AssertHasMessageError(certificate.RemarksInfo, "The name and country/region of the company issuing the invoice should be provided in Remarks.");

			certificate.Remarks = "Because the first two weren't enough";

			AssertNoMessageErrors(certificate.RemarksInfo);

			certificate.IsSubjectOfThirdPartyInvoice = false;
			certificate.Remarks = ZString.Empty;

			AssertNoMessageErrors(certificate.RemarksInfo);

			certificate.Remarks = "Something";
			certificate.Remarks = null;

			AssertNoMessageErrors(certificate.RemarksInfo);
		}

		public void Test_Validation_BacktobackCertificateOfOrigin_RemarksRequired()
		{
			var certificate = CreateBuilder(CreateShipmentCore()).Build();

			AssertNoMessageErrors(certificate.RemarksInfo);

			certificate.IsBacktobackCertificateOfOrigin = true;
			certificate.Remarks = ZString.Empty;

			AssertHasMessageError(certificate.RemarksInfo, "The Original Proof of Origin reference no.; date of issuance; issuing country; RCEP country of origin of the first exporting party; and if applicable, the approved exporter authorization code of the first exporting Party should be entered in Remarks Box 14.");

			certificate.RemarksInfo.ClearAllNotifications();
			certificate.Remarks = "Something";
			certificate.Remarks = null;

			AssertHasMessageError(certificate.RemarksInfo, "The Original Proof of Origin reference no.; date of issuance; issuing country; RCEP country of origin of the first exporting party; and if applicable, the approved exporter authorization code of the first exporting Party should be entered in Remarks Box 14.");

			certificate.Remarks = "Various important details";

			AssertNoMessageErrors(certificate.RemarksInfo);

			certificate.IsBacktobackCertificateOfOrigin = false;
			certificate.Remarks = ZString.Empty;

			AssertNoMessageErrors(certificate.RemarksInfo);

			certificate.Remarks = "Something";
			certificate.Remarks = null;

			AssertNoMessageErrors(certificate.RemarksInfo);
		}

		public void TestPackLineFOB()
		{
			var shipment = CreateShipmentCore();
			CreateLineItemsCore(shipment);
			var rcep = new RCEPBuilder(shipment).Build();
			var rcepPacklines = rcep.LineItems.ToArray();

			AssertNotNull(rcep);
			AssertEquals("Packline1 FOB", GlbCompany.CurrentCompany.LocalCurrency.Code, rcepPacklines[0].FOB.Currency.Code);
		}

		public void Test_Validation_Packline_FOB_IfOriginCriterionRVC()
		{
			var shipment = CreateShipmentCore();
			CreateLineItemsCore(shipment);
			var certificate = CreateBuilder(shipment).Build();
			var rcepPacklines = certificate.LineItems.ToArray();

			rcepPacklines[0].OriginCriterion.Code = OriginCriterionListRCEP.Codes.WO;
			rcepPacklines[0].FOB.Amount = 42.42;

			AssertNoMessageErrors(rcepPacklines[0].FOB.AmountInfo);

			rcepPacklines[0].FOB.Amount = ZDecimal.Zero;

			AssertNoMessageErrors(rcepPacklines[0].FOB.AmountInfo);

			rcepPacklines[0].OriginCriterion.Code = OriginCriterionListRCEP.Codes.RVC;

			AssertHasMessageError(rcepPacklines[0].FOB.AmountInfo, "The FOB value is required for goods with Origin Conferring Criterion. Enter the FOB value in the Quantity box.");

			rcepPacklines[0].FOB.Amount = 42.42;

			AssertNoMessageErrors(rcepPacklines[0].FOB.AmountInfo);

			rcepPacklines[0].FOB.Amount = ZDecimal.Zero;

			AssertHasMessageError(rcepPacklines[0].FOB.AmountInfo, "The FOB value is required for goods with Origin Conferring Criterion. Enter the FOB value in the Quantity box.");

			rcepPacklines[0].OriginCriterion.Code = OriginCriterionListRCEP.Codes.WO;

			AssertNoMessageErrors(rcepPacklines[0].FOB.AmountInfo);
		}

		public void Test_Validation_TransportReference()
		{
			ConditionallyAssertMessageErrorIfTransportReferenceIsEmpty(Core.Constants.CountryCodes.NewZealand, Core.Constants.CountryCodes.China, true);
			ConditionallyAssertMessageErrorIfTransportReferenceIsEmpty(Core.Constants.CountryCodes.China, Core.Constants.CountryCodes.NewZealand, false);
			ConditionallyAssertMessageErrorIfTransportReferenceIsEmpty(Core.Constants.CountryCodes.NewZealand, Core.Constants.CountryCodes.Australia, false);
			ConditionallyAssertMessageErrorIfTransportReferenceIsEmpty(Core.Constants.CountryCodes.Australia, Core.Constants.CountryCodes.NewZealand, false);
			ConditionallyAssertMessageErrorIfTransportReferenceIsEmpty(Core.Constants.CountryCodes.Australia, Core.Constants.CountryCodes.Singapore, false);
			ConditionallyAssertMessageErrorIfTransportReferenceIsEmpty(Core.Constants.CountryCodes.Australia, Core.Constants.CountryCodes.Japan, false);
		}

		void ConditionallyAssertMessageErrorIfTransportReferenceIsEmpty(string originCountry, string destinationCountry, bool expectValue)
		{
			var builder = CreateBuilder(CreateShipmentCore(
				createConsol: true,
				originCountry: originCountry,
				destinationCountry: destinationCountry,
				applyCustomisations: false));
			var certificate = builder.Build();

			ConditionallyAssertMessageErrorIfTransportReferenceIsEmpty(certificate, expectValue);
		}
	}
}
