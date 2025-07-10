using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.NZ;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Address;
using Enterprise.MasterFiles.Business;
using AddressValidation = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base.AddressValidation;
using InvoiceType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice.InvoiceType.InvoiceTypes;

namespace Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin.NZ
{
	sealed class NZCFTABuilderTest : CertificateOfOriginBuilderTests<NZCFTA, NZCFTALineItem, OriginCriterionListNZCFTA>
	{
		protected override CertificateOfOriginBuilder<NZCFTA, NZCFTALineItem, OriginCriterionListNZCFTA> CreateBuilder(ForwardingShipment shipment) => new NZCFTABuilder(shipment);

		public void TestCriterionCode() => AssertEquals(OriginCriterionListNZCFTA.Codes.WO, new NZCFTABuilder(CreateShipmentCore()).GetDefaultOriginCriterionCode());

		public void TestHarmonisedCodeCountry() => AssertEquals(Core.Constants.CountryCodes.NewZealand, new NZCFTABuilder(CreateShipmentCore()).HarmonisedCodeCountry);

		public void TestShipmentDocumentName() => AssertEquals(ShipmentDocumentNames.NZCFTACertificateOfOrigin, new NZCFTABuilder(CreateShipmentCore()).ShipmentDocumentName);

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
			AssertEquals(true, builder.InitialFlags.ValidatePortOfOrigin);
			AssertEquals(true, builder.InitialFlags.ValidatePortOfDestination);

			AssertEquals(true, builder.InitialFlags.ValidateTransportReference);

			AssertEquals(true, builder.InitialFlags.ValidateDepartureDate);
			AssertEquals(true, builder.InitialFlags.ValidateArrivalDate);

			AssertEquals(InvoiceType.Complete, builder.InitialFlags.InvoiceType);

			AssertEquals(false, builder.InitialFlags.ValidateSignature);

			AssertEquals(false, builder.InitialFlags.ValidateLineItemOrigin);
			AssertEquals(true, builder.InitialFlags.ValidateLineItemOriginCriterion);
			AssertEquals(true, builder.InitialFlags.ValidateLineItemItemNumber);
			AssertEquals(false, builder.InitialFlags.ValidateLineItemWeight);
		}

		public void TestBuild_NZCFTA_PackLine()
		{
			var shipment = CreateShipmentCore();
			var packlines = CreateLineItemsCore(shipment).ToArray();
			var nzcfta = CreateBuilder(shipment).Build();

			AssertNotNull(nzcfta);

			AssertEquals("NZCFTA", nzcfta.AddressCollection.CertificateName);

			AssertEquals("LineItemsCount", 1, nzcfta.LineItems.Count);

			var nzcftaPacklines = nzcfta.LineItems.ToArray();

			AssertEquals("Packline1_GrossOrNetWeight", packlines[0].JL_ActualWeightUQ, nzcftaPacklines[0].Quantity.Unit.Code);
		}

		public void TestBuild_NZCFTA_Invoice()
		{
			var shipment = CreateShipmentCore();
			var declaration = CreateDeclarationNz(shipment);
			var nzcfta = CreateBuilder(shipment).Build();

			AssertNotNull(nzcfta);

			AssertEquals("NZCFTA", nzcfta.AddressCollection.CertificateName);

			AssertEquals("LineItemsCount", 1, nzcfta.LineItems.Count);

			var nzcftaPacklines = nzcfta.LineItems.ToArray();

			AssertEquals("LineItem1_MarksAndNumbers", ((Enterprise.Integration.Customs.NZ.IJobComInvoiceLine)declaration.Invoices[0].InvoiceLines[0]).PackagingMarks1, nzcftaPacklines[0].MarksAndNumbers);
			AssertEquals("LineItem1_IsMarksAndNumbersEditable", false, nzcftaPacklines[0].IsMarksAndNumbersEditable);
			AssertEquals("LineItem1_QuantityNumber", declaration.Invoices[0].InvoiceLines[0].JI_InvoiceQuantity.ToZInt(), nzcftaPacklines[0].QuantityNumber);
			AssertEquals("LineItem1_QuantityUnit", ZString.Empty, nzcftaPacklines[0].QuantityUnit.Code);
		}

		public void TestIsNewZealandToChina()
		{
			var shipmentNZCN = CreateShipmentCore(originCountry: Constants.CountryCodes.NewZealand, destinationCountry: Constants.CountryCodes.China);
			var builderNZCN = CreateBuilder(shipmentNZCN);
			var nzcftaNZCN = builderNZCN.Build();

			AssertNotNull(nzcftaNZCN);
			AssertEquals(true, nzcftaNZCN.IsNewZealandToChina);

			var shipmentCNNZ = CreateShipmentCore(originCountry: Constants.CountryCodes.China, destinationCountry: Constants.CountryCodes.NewZealand);
			var builderCNNZ = CreateBuilder(shipmentCNNZ);
			var nzcftaCNNZ = builderCNNZ.Build();

			AssertNotNull(nzcftaCNNZ);
			AssertEquals(false, nzcftaCNNZ.IsNewZealandToChina);
		}

		public void Test_Validation_ReturnErrorMessage_When_PacklineGrossWeightIsInvalidCode()
		{
			var shipment = CreateShipmentCore();
			var packlines = CreateLineItemsCore(shipment).ToArray();
			packlines[0].JL_ActualWeightUQ = Constants.Weight.Ounces;
			var nzcfta = CreateBuilder(shipment).Build();

			var packline = nzcfta.LineItems.First();
			var quantityUnit = (CodeDescription)packline.Quantity.Unit;

			AssertHasMessageError(quantityUnit.CodeInfo, "Gross Weight must be converted to an NZ Customs accepted unit.");

			packline.Quantity.Value = 0;
			packline.ValidateAllIncludingChildren(); // lineItem DocDataObject is not validated by default, but the error will always show up on OpenCOOrderForm Invocation
			AssertNoMessageErrors(quantityUnit.CodeInfo);

			packline.Quantity.Value = -1;
			packline.ValidateAllIncludingChildren();
			AssertNoMessageErrors(quantityUnit.CodeInfo);

			packline.Quantity.Value = 1;
			packline.ValidateAllIncludingChildren();
			AssertHasMessageError(quantityUnit.CodeInfo, "Gross Weight must be converted to an NZ Customs accepted unit.");
		}

		public void Test_Validation_ReturnErrorMessage_When_PacklineQuantityNumberIsInvalid()
		{
			var shipment = CreateShipmentCore();
			CreateLineItemsCore(shipment, addHarmonizedCodeForCountry: true).ToArray();
			var nzcfta = CreateBuilder(shipment).Build();

			var packline = nzcfta.LineItems.First();

			AssertNoErrors(packline.QuantityNumberInfo);

			packline.QuantityNumber = -1;
			AssertHasMessageError(packline.QuantityNumberInfo, "Quantity Number required in Box 12.");

			packline.QuantityNumber = 0;
			AssertHasMessageError(packline.QuantityNumberInfo, "Quantity Number required in Box 12.");
		}

		public void Test_Validation_ReturnErrorMessage_When_PacklineQuantityUnitIsInvalid()
		{
			var shipment = CreateShipmentCore();
			CreateLineItemsCore(shipment, addHarmonizedCodeForCountry: true).ToArray();
			var nzcfta = CreateBuilder(shipment).Build();

			var packline = nzcfta.LineItems.First();
			var quantity = (CodeDescription)packline.QuantityUnit;

			quantity.Code = NZCustomsTariffQuantityUnitList.Codes.NMP;

			AssertNoErrors(quantity.CodeInfo);

			quantity.Code = ZString.Empty;
			AssertHasMessageError(quantity.CodeInfo, "Quantity Unit Type required in Box 12.");

			quantity.Code = null;
			AssertHasMessageError(quantity.CodeInfo, "Quantity Unit Type required in Box 12.");

			quantity.Code = "Ounce";
			AssertHasMessageError(quantity.CodeInfo, "Quantity Unit Type must be a NZ Customs accepted Tariff Quantity Unit.");
		}

		public void TestBuild_NZCN_NoSignatureValidationErrorIsDisplayed_When_UserSignatureNotUploaded()
		{
			var shipment = CreateShipmentCore(originCountry: Constants.CountryCodes.NewZealand, destinationCountry: Constants.CountryCodes.China);
			var builder = CreateBuilder(shipment);

			GlbStaff.CurrentUser.SignatureImage = null;
			var nzcfta = builder.Build();
			AssertNoMessageErrors(nzcfta.SignatureErrorInfo);

			GlbStaff.CurrentUser.SignatureImage = new Bitmap(1, 1);
			nzcfta = builder.Build();
			AssertNoMessageErrors(nzcfta.SignatureErrorInfo);
		}

		public void TestBuild_Otherwise_SignatureValidationErrorIsDisplayed_When_UserSignatureNotUploaded()
		{
			var shipment = CreateShipmentCore(originCountry: Constants.CountryCodes.China, destinationCountry: Constants.CountryCodes.NewZealand);
			var builder = CreateBuilder(shipment);

			GlbStaff.CurrentUser.SignatureImage = null;
			var nzcfta = builder.Build();
			AssertHasMessageError(nzcfta.SignatureErrorInfo, SignatureRequiredMessage);

			GlbStaff.CurrentUser.SignatureImage = new Bitmap(1, 1);
			nzcfta = builder.Build();
			AssertNoMessageErrors(nzcfta.SignatureErrorInfo);
		}

		public override void Test_Validation_ReturnErrorMessage_When_LineItemGrossWeightIsInvalid()
		{
			var shipment = CreateShipmentCore();
			var packlines = CreateLineItemsCore(shipment).ToArray();
			var nzcfta = CreateBuilder(shipment).Build();
			var packline = nzcfta.LineItems.First();

			CombineAssertions(() =>
			{
				// Gross Weight value must be not negative if there to netWeight
				OptionallyAssert_PacklineGrossWeightError(packline, 0, 0, true);
				OptionallyAssert_PacklineGrossWeightError(packline, -1, 0, true);
				OptionallyAssert_PacklineGrossWeightError(packline, 0, 1, false);
				OptionallyAssert_PacklineGrossWeightError(packline, -1, 1, false);
				OptionallyAssert_PacklineGrossWeightError(packline, 1, 1, false);

				// Unit code must be not empty if gross weight is greater than zero
				OptionallyAssert_PacklineGrossWeightUnitError(
					packline,
					codeString: ZString.Empty,
					grossWeight: -1,
					expectError: false,
					errorString: string.Empty);
				OptionallyAssert_PacklineGrossWeightUnitError(
					packline,
					codeString: ZString.Empty,
					grossWeight: 1,
					expectError: true,
						errorString: "Gross weight quantity value and unit is mandatory.");
				OptionallyAssert_PacklineGrossWeightUnitError(
					packline,
					codeString: "KG",
					grossWeight: 1,
					expectError: false,
					errorString: string.Empty);

				// Unit code must be nzCustoms accepted if gross weight is greater than zero
				OptionallyAssert_PacklineGrossWeightUnitError(
					packline,
					codeString: "OZ",
					grossWeight: -1,
					expectError: false,
					errorString: string.Empty);
				OptionallyAssert_PacklineGrossWeightUnitError(
					packline,
					codeString: "OZ",
					grossWeight: 1,
					expectError: true,
					errorString: "Gross Weight must be converted to an NZ Customs accepted unit.");
				OptionallyAssert_PacklineGrossWeightUnitError(
					packline,
					codeString: "KG",
					grossWeight: 1,
					expectError: false,
					errorString: string.Empty);
			});
		}

		public void Test_Validation_ReturnErrorMessage_When_ProducerCityIsInvalid()
		{
			var builder = CreateBuilder(CreateShipmentCore());
			var nzcfta = builder.Build();

			Optionally_Assert_ProducerCityError(nzcfta, nzcfta.ProducerAddress.City, false);
			Optionally_Assert_ProducerCityError(nzcfta, null, true);

			nzcfta.ProducerAddressStateString = new AddressState { IsSameAsExporter = true, IsUnknown = false, ExcludeFromPDF = false }.GetAddressStateString();
			Optionally_Assert_ProducerCityError(nzcfta, "Test Exporter City", false);
			Optionally_Assert_ProducerCityError(nzcfta, null, false);

			nzcfta.ProducerAddressStateString = new AddressState { IsSameAsExporter = false, IsUnknown = true, ExcludeFromPDF = false }.GetAddressStateString();
			Optionally_Assert_ProducerCityError(nzcfta, nzcfta.ProducerAddress.City, false);
			Optionally_Assert_ProducerCityError(nzcfta, null, false);

			nzcfta.ProducerAddressStateString = new AddressState { IsSameAsExporter = false, IsUnknown = false, ExcludeFromPDF = true }.GetAddressStateString();
			Optionally_Assert_ProducerCityError(nzcfta, "Test Producer City", false);
			Optionally_Assert_ProducerCityError(nzcfta, null, true);
		}

		void Optionally_Assert_ProducerCityError(NZCFTA nzcfta, string city, bool expectError)
		{
			nzcfta.ProducerAddress.City = city;

			if (expectError)
			{
				AssertHasMessageError(nzcfta.ProducerAddress.AddressFormattedInfo, "Producer City is a mandatory field for a JEVS submission");
			}
			else
			{
				AssertNoMessageErrors(nzcfta.ProducerAddress.AddressFormattedInfo);
			}
			nzcfta.ProducerAddress.AddressFormattedInfo.ClearAllNotifications();
		}

		void OptionallyAssert_PacklineGrossWeightError(NZCFTALineItem packline, int grossWeight, int netWeight, bool expectError)
		{
			var nzMeasurement = (NZMeasurement)packline.Quantity;
			nzMeasurement.ValueInfo.ClearAllNotifications();

			nzMeasurement.Value = grossWeight;
			packline.QuantityNet.Value = netWeight;

			packline.ValidateAllIncludingChildren();

			if (expectError)
			{
				AssertHasMessageError(nzMeasurement.ValueInfo, "Weight is mandatory and must be greater than zero.");
			}
			else
			{
				AssertNoMessageErrors(nzMeasurement.ValueInfo);
			}
		}

		void OptionallyAssert_PacklineGrossWeightUnitError(NZCFTALineItem packline, string codeString, int grossWeight, bool expectError, string errorString)
		{
			var quantity = packline.Quantity;
			var weightUnit = (CodeDescription)quantity.Unit;
			weightUnit.CodeInfo.ClearAllNotifications();

			weightUnit.Code = codeString;
			quantity.Value = grossWeight;

			packline.ValidateAllIncludingChildren();

			if (expectError)
			{
				AssertHasMessageError(weightUnit.CodeInfo, errorString);
			}
			else
			{
				AssertNoMessageErrors(weightUnit.CodeInfo);
			}
		}
	}
}
