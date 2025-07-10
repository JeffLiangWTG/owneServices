using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.CodeLists;
using Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin;
using Enterprise.MasterFiles.Business;
using static Enterprise.Freight.Forwarding.Documents.Testing.AssertionHelper;
using AddressValidation = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base.AddressValidation;
using InvoiceType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice.InvoiceType.InvoiceTypes;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.CertificateOfOrigin
{
	sealed class TAFTABuilderTest : CertificateOfOriginBuilderTests<TAFTA, TAFTALineItem, OriginCriterionListTAFTA>
	{
		protected override CertificateOfOriginBuilder<TAFTA, TAFTALineItem, OriginCriterionListTAFTA> CreateBuilder(ForwardingShipment shipment) => new TAFTABuilder(shipment);

		public void TestCriterionCode() => AssertEquals(OriginCriterionListTAFTA.Codes.WO, new TAFTABuilder(CreateShipmentCore()).GetDefaultOriginCriterionCode());

		public void TestHarmonisedCodeCountry() => AssertEquals(ZString.Empty, new TAFTABuilder(CreateShipmentCore()).HarmonisedCodeCountry);

		public void TestInitialFlags()
		{
			var builder = CreateBuilder(CreateShipmentCore());

			AssertEquals(AddressValidation.None, builder.InitialFlags.ValidateProducerAddress);
			AssertEquals(InvoiceType.None, builder.InitialFlags.InvoiceType);

			AssertEquals(true, builder.InitialFlags.ValidateDepartureDate);
			AssertEquals(true, builder.InitialFlags.ValidatePortOfLoading);
			AssertEquals(true, builder.InitialFlags.ValidatePortOfDischarge);
			AssertEquals(false, builder.InitialFlags.ValidatePortOfDestination);
			AssertEquals(false, builder.InitialFlags.ValidateLineItemItemNumber);
			AssertEquals(false, builder.InitialFlags.ValidatePortOfOrigin);
			AssertEquals(true, builder.InitialFlags.ValidateSignature);
			AssertEquals(true, builder.InitialFlags.ValidateTransportReference);
			AssertEquals(false, builder.InitialFlags.ValidateArrivalDate);
			AssertEquals(false, builder.InitialFlags.ValidateLineItemOrigin);
			AssertEquals(true, builder.InitialFlags.ValidateLineItemOriginCriterion);
			AssertEquals(false, builder.InitialFlags.ValidateLineItemWeight);
			AssertEquals(false, builder.InitialFlags.ValidateLineItemMarksAndNumbers);
		}

		public void Test_BuildTAFTA_PackLine()
		{
			var shipmentAUTH = CreateShipmentCore(createConsol: true, originCountry: Constants.CountryCodes.Australia, destinationCountry: Constants.CountryCodes.Thailand);
			var packlines = CreateLineItemsCore(shipmentAUTH, addHarmonizedCodeForCountry: false, originCode: "AU").ToArray();

			var tafta = CreateBuilder(shipmentAUTH).Build();
			AssertNotNull(tafta);

			var taftaPacklines = tafta.LineItems.ToArray();

			AssertEquals("PacklinesCount", 1, tafta.LineItems.Count);
			AssertEquals("PacklinesCount", 1, taftaPacklines.Length);
			AssertEquals("Packline1 ProductNumber", packlines[0].Products.PackProductManager.Value, taftaPacklines[0].ProductNumber);
		}

		public void Test_BuildTAFTA_Invoice()
		{
			var shipmentAUTH = CreateShipmentCore(createConsol: true, originCountry: Constants.CountryCodes.Australia, destinationCountry: Constants.CountryCodes.Thailand);
			var declaration = CreateDeclaration(shipmentAUTH);

			var tafta = CreateBuilder(shipmentAUTH).Build();
			AssertNotNull(tafta);

			var taftaPacklines = tafta.LineItems.ToArray();

			AssertEquals("PacklinesCount", 1, tafta.LineItems.Count);
			AssertEquals("PacklinesCount", 1, taftaPacklines.Length);
			AssertEquals("Packline1 ProductNumber", declaration.Invoices[0].InvoiceLines[0].JI_PartNo, taftaPacklines[0].ProductNumber);
		}

		public void Test_BuyerAddress()
		{
			var shipmentAUTH = CreateShipmentCore(createConsol: true, originCountry: Constants.CountryCodes.Australia, destinationCountry: Constants.CountryCodes.Thailand);
			var tafta = CreateBuilder(shipmentAUTH).Build();

			AssertNotNull(tafta);
			AssertAddressData(shipmentAUTH.BuyerDocAddress, tafta.BuyerAddress);
		}

		#region Preparation

		protected override void PopulateAddresses(ForwardingShipment shipment)
		{
			var buyerDocAddress = Factory.New<OrgHeader>();
			buyerDocAddress.OH_FullName = "Buyer";
			buyerDocAddress.OH_RL_NKClosestPort = "THPHI";
			buyerDocAddress.MainAddress.Address1 = "Unit 100";
			buyerDocAddress.MainAddress.Address2 = "11 Why Street";
			buyerDocAddress.MainAddress.City = "Phichit";
			buyerDocAddress.MainAddress.Postcode = "66120";
			buyerDocAddress.MainAddress.OA_RN_NKCountryCode = "TH";

			shipment.BuyerDocAddress.E2_OA_Address = buyerDocAddress.MainAddress.PK;
		}

		protected override void PopulatePackLine(ForwardingPackLine packline)
		{
			packline.Products.PackProductManager.Value = "P123";
		}

		#endregion
	}
}
