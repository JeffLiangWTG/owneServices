using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Forwarding;
using ExportAWBHeader = Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader;
using Money = Enterprise.Freight.Forwarding.Documents.DocDataObjects.Money;

namespace Enterprise.Freight.Forwarding.Documents.BR.Testing
{
	sealed class CargoControlAndTransitBuilderTest : TestCaseWithFactory
	{
		public void TestBuild()
		{
			var shipment = Factory.New<ForwardingShipment>();
			PopulateShipment(shipment, "081001", 12, 25, "goods", "Signature", "AUSYD", "BRSAO",
				new ZDateTime(2019, 11, 12), "notes");

			PopulateRateLines(shipment);
			PopulatePrepaidAndCollectValues(shipment);

			var builder = new CargoControlAndTransitBuilder(shipment);
			var cct = builder.Build();

			PopulateSpacialHandling(cct);

			AssertNotNull(cct);
			AssertEquals(nameof(cct.OptionalShippingInformation), shipment.AWBHeader.EH_OptionalShippingInformation, cct.OptionalShippingInformation);
			AssertEquals(nameof(cct.OptionalShippingInformation2), shipment.AWBHeader.EH_OptionalShippingInformation2, cct.OptionalShippingInformation2);
			AssertEquals(nameof(cct.Charges), shipment.AWBHeader.EH_ChargesCode, cct.Charges.Code);

			AssertHeader(cct, shipment);
			AssertAirportInformations(cct, shipment);
			AssertShipper(cct, shipment);
			AssertConsignee(cct, shipment);
			AssertCurrencyAndValues(cct, shipment);
			AssertRateLines(cct, shipment);
			AssertPrePaidAndCollectValues(cct, shipment);
			AssertSignature(cct, shipment);
		}

		public void TestAWBNumberValidation()
		{
			var shipment = Factory.New<ForwardingShipment>();
			PopulateShipment(shipment, ZString.Empty, 12, 25, "goods", "Signature", "AUSYD", "BRSAO",
				new ZDateTime(2019, 11, 12), "notes");

			var builder = new CargoControlAndTransitBuilder(shipment);
			var cct = builder.Build();

			var emptyErrorMessage = "HAWB number is required.";
			AssertEquals("AWB Number should have empty value.", ZString.Empty, cct.AWBNumber);
			AssertHasMessageError("AWB Number should have empty error message.", cct.AWBNumberInfo, emptyErrorMessage);

			var lengthLimitErrorMessage = "HAWB should be less than or equal to 11 characters";
			cct.AWBNumber = "A1234567890A";
			cct.ValidateAllIncludingChildren();
			AssertHasMessageError("AWB Number should have length limit error message.", cct.AWBNumberInfo, lengthLimitErrorMessage);

			cct.AWBNumber = "081001";
			cct.ValidateAllIncludingChildren();
			AssertNoMessageError("AWB Number should have no empty error message.", cct.AWBNumberInfo, emptyErrorMessage);
			AssertNoMessageError("AWB Number should have no length limit error message.", cct.AWBNumberInfo, lengthLimitErrorMessage);
		}

		public void TestShippersSignatoryValidation()
		{
			var shipment = Factory.New<ForwardingShipment>();
			PopulateShipment(shipment, ZString.Empty, 12, 25, "goods", "Signature", "AUSYD", "BRSAO",
				new ZDateTime(2019, 11, 12), "notes");

			shipment.AWBHeader.EH_ShippersSignature = ZString.Empty;
			shipment.AWBHeader.Parent.IsAWBValuesOverriddenProperty = true;

			var builder = new CargoControlAndTransitBuilder(shipment);
			var cct = builder.Build();

			var errorMessage = "Shippers Signature is required for CCT messaging.";
			AssertEquals("Shippers Signature should have empty value.", ZString.Empty, cct.ShippersSignature);
			AssertHasMessageError("Shippers Signature should have error message.", cct.ShippersSignatureInfo, errorMessage);

			cct.ShippersSignature = "Signature";
			AssertNoMessageError("Shippers Signature should have no error message.", cct.ShippersSignatureInfo, errorMessage);
		}

		public void TestAgentsSignatoryAndAgentApprovedExporterNumberValidation()
		{
			var shipment = Factory.New<ForwardingShipment>();
			PopulateShipment(shipment, ZString.Empty, 12, 25, "goods", "", "AUSYD", "BRSAO",
				new ZDateTime(2019, 11, 12), "notes");

			shipment.AWBHeader.EH_AWBAgentsSignature = ZString.Empty;

			var builder = new CargoControlAndTransitBuilder(shipment);
			var cct = builder.Build();
			cct.AgentsSignature = ZString.Empty;

			var errorMessage = "Agents Signature or Agent Approved Exporter Number is required for CCT messaging.";
			AssertEquals("Agents Signature should have empty value.", ZString.Empty, cct.AgentsSignature);
			AssertHasMessageError("Agents Signature should have error message.", cct.AgentsSignatureInfo, errorMessage);
			AssertHasMessageError("Agent Approved Exporter Number should have error message.", cct.AgentApprovedExporterNumberInfo, errorMessage);

			cct.AgentsSignature = "Signature";
			AssertNoMessageError("Agents Signature should have no error message.", cct.AgentsSignatureInfo, errorMessage);
			AssertNoMessageError("Agent Approved Exporter Numbers should have no error message.", cct.AgentApprovedExporterNumberInfo, errorMessage);
		}

		public void TestIsSignatureReadOnly()
		{
			var shipment = Factory.New<ForwardingShipment>();
			PopulateShipment(shipment, "081001", 12, 25, "goods", "Signature", "AUSYD", "BRSAO", new ZDateTime(), "notes");

			var documentData = CreateDocumentData(shipment);

			#region Test Case 1 - When Shipment Logs Has MSN Event With MST Parameter And FWB Value

			AddLog(documentData, Events.MessageSent, ZDateTimeOffset.UtcToday.AddHours(-5), "MST", "FWB");

			var builder = new CargoControlAndTransitBuilder(shipment);
			var cct = builder.Build();

			AssertEquals("IsSignatureReadOnly is false when MSN event is sent with MST != FHL", cct.IsSignatureReadOnly, false);

			#endregion

			#region Test Case 2 - When Shipment Logs Has MSN Event With No MST Parameter

			AddLog(documentData, Events.MessageSent, ZDateTimeOffset.UtcToday.AddHours(-5), "EQN", "SUDU7896583");

			cct = builder.Build();

			AssertEquals("IsSignatureReadOnly is false when MSN event is sent with No MST parameter", cct.IsSignatureReadOnly, false);

			#endregion

			#region Test Case 3 - When Shipment Logs Has MSN Event With MST Parameter And FHL Value

			AddLog(documentData, Events.MessageSent, ZDateTimeOffset.UtcToday.AddHours(-5), "MST", "FHL");

			cct = builder.Build();

			AssertEquals("IsSignatureReadOnly is true when MSN event is sent with MST = FHL", cct.IsSignatureReadOnly, true);

			#endregion

			#region CreateDocumentData

			VisualizerDocumentData CreateDocumentData(IForwardingShipment shipment)
			{
				var documentData = Factory.New<VisualizerDocumentData>();
				documentData.JDD_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				documentData.JDD_ParentID = shipment.PK;
				documentData.JDD_Name = "CCTCargoAndTransitControlBR";

				return documentData;
			}

			#endregion

			#region AddLog

			void AddLog(VisualizerDocumentData documentData, Event evenType, ZDateTimeOffset eventDateTime, ZString key, ZString value)
			{
				var paramList = new List<KeyValuePair<string, string>>
					{
						new KeyValuePair<string, string>(key, value),
						new KeyValuePair<string, string>("DEP", "Customs")
					};

				documentData.Logs.AddNew(evenType, eventDateTime, paramList.ToArray());

				Factory.Save();
			}

			#endregion
		}

		public void TestIssueDateValidation()
		{
			var shipment = Factory.New<ForwardingShipment>();
			PopulateShipment(shipment, "081001", 12, 25, "goods", "Signature", "AUSYD", "BRSAO", new ZDateTime(), "notes");

			var builder = new CargoControlAndTransitBuilder(shipment);
			var cct = builder.Build();
			cct.IssueDate = new ZDateTime();

			var errorMessage = "Issue Date is required for CCT messaging.";
			AssertEquals("Issue Date should have empty value.", ZDateTime.Empty, cct.IssueDate);
			AssertHasMessageError("Issue Date should have error message.", cct.IssueDateInfo, errorMessage);

			cct.IssueDate = ZDateTime.Now;
			AssertNoMessageError("Issue Date should have no error message.", cct.IssueDateInfo, errorMessage);
		}

		[TestDate(2024, 3, 1)]
		public void TestIssueDateValidation_Warning_CompareIssueDateWithCurrentDateInBrazilTimeZone()
		{
			const string warningMessage = "Future Issue Date detected for this Shipment which may result in the failure of the CCT House Manifest being sent at a later stage.\r\nPlease verify the date on the Shipment > Additional Details > View/Edit AWB > Executed on (date) OR Shipment > Basic Registration > Issue Date.";

			var issueDate = new ZDateTime(2024, 3, 1);
			var shipment = Factory.New<ForwardingShipment>();
			PopulateShipment(shipment, "081001", 12, 25, "goods", "Signature", "AUSYD", "BRSAO", new ZDateTime(), "notes");

			var builder = new CargoControlAndTransitBuilder(shipment);
			var cct = builder.Build();
			cct.IssueDate = issueDate.AddDays(1);

			AssertHasWarning("Issue Date should have warning message.", cct.IssueDateInfo, warningMessage);

			cct.IssueDate = issueDate.AddDays(-1);
			AssertNoWarning("Issue Date should have no warning message.", cct.IssueDateInfo, warningMessage);
		}

		public void TestIssuePlaceValidation()
		{
			var shipment = Factory.New<ForwardingShipment>();
			PopulateShipment(shipment, "081001", 12, 25, "goods", "Signature", "AUSYD", "BRSAO",
				new ZDateTime(2019, 11, 12), "notes");

			var builder = new CargoControlAndTransitBuilder(shipment);
			var cct = builder.Build();
			cct.IssuePlace = ZString.Empty;

			var errorMessage = "Issue Place is required for CCT messaging.";
			AssertEquals("Issue Place should have empty value.", ZString.Empty, cct.IssuePlace);
			AssertHasMessageError("Issue Place should have error message.", cct.IssuePlaceInfo, errorMessage);

			cct.IssuePlace = "Place";
			AssertNoMessageError("Issue Place should have no error message.", cct.IssuePlaceInfo, errorMessage);
		}

		public void TestConsignorValidation()
		{
			var shipment = Factory.New<ForwardingShipment>();
			PopulateShipment(shipment, "081001", 12, 25, "goods", "Signature", "AUSYD", "BRSAO",
				new ZDateTime(2019, 11, 12), "notes");

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "";
			shipper.MainAddress.Postcode = "";
			shipper.MainAddress.Address1 = "";
			shipper.MainAddress.City = "";
			shipper.MainAddress.OA_RN_NKCountryCode = "";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var builder = new CargoControlAndTransitBuilder(shipment);
			var cct = builder.Build();

			Address address = cct.Shipper as Address;

			cct.ValidateAllIncludingChildren();

			AssertEquals("Company Name should have empty value.", ZString.Empty, cct.Shipper.CompanyName);
			AssertEquals("Postcode should have empty value.", ZString.Empty, cct.Shipper.Postcode);
			AssertEquals("Street Name should have empty value.", ZString.Empty, cct.Shipper.AddressLine1);
			AssertEquals("City should have empty value.", ZString.Empty, cct.Shipper.City);
			AssertEquals("Country should have empty value.", ZString.Empty, cct.Shipper.Country.Code);

			AssertHasMessageError("Company Name should have error message.", address.CompanyNameInfo, "Shipper party name and address information is required.");
			AssertHasMessageError("Address Formatted should have error message.", address.AddressLine1Info, "Shipper Address is required for CCT messaging.");

			address.CompanyName = "Company";
			address.Postcode = "00666";
			address.AddressLine1 = "Street";
			address.City = "SAO PAULO";
			address.Country.Code = "BR";
			address.Contact = "Contact01";
			address.Phone = "Phone01";
			address.Email = "Email01";

			cct.ValidateAllIncludingChildren();

			AssertNotEquals("Company Name should not have empty value.", ZString.Empty, cct.Shipper.CompanyName);
			AssertNotEquals("Postcode should not have empty value.", ZString.Empty, cct.Shipper.Postcode);
			AssertNotEquals("Street Name should not have empty value.", ZString.Empty, cct.Shipper.AddressLine1);
			AssertNotEquals("City should not have empty value.", ZString.Empty, cct.Shipper.City);
			AssertNotEquals("Country should not have empty value.", ZString.Empty, cct.Shipper.Country.Code);

			AssertNoMessageError("Company Name should have no error message.", address.CompanyNameInfo, "Shipper party name and address information is required.");
			AssertNoMessageError("Address Formatted should have no error message.", address.AddressLine1Info, "Shipper Address is required for CCT messaging.");

			address.AddressLine1 = "very long street, very long";
			address.AddressLine2 = "and there is even more to come, it just keeps on coming";
			AssertHasWarning("Both Address Lines are too long", address.AddressLine1Info, "Shipper's Address line 1 & 2 should not exceed 68 characters to comply with Cargo Control and Transit (CCT) system requirements.\r\nOnly the first 68 characters will be sent in the message to CCT.");
		}

		public void TestConsigneeValidation()
		{
			var shipment = Factory.New<ForwardingShipment>();
			PopulateShipment(shipment, "081001", 12, 25, "goods", "Signature", "AUSYD", "BRSAO",
				new ZDateTime(2019, 11, 12), "notes");

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "";
			consignee.MainAddress.Postcode = "";
			consignee.MainAddress.Address1 = "";
			consignee.MainAddress.City = "";
			consignee.MainAddress.OA_RN_NKCountryCode = "";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var builder = new CargoControlAndTransitBuilder(shipment);
			var cct = builder.Build();

			Address address = cct.Consignee as Address;

			cct.ValidateAllIncludingChildren();

			AssertEquals("Company Name should have empty value.", ZString.Empty, cct.Consignee.CompanyName);
			AssertEquals("Postcode should have empty value.", ZString.Empty, cct.Consignee.Postcode);
			AssertEquals("Street Name should have empty value.", ZString.Empty, cct.Consignee.AddressLine1);
			AssertEquals("City should have empty value.", ZString.Empty, cct.Consignee.City);
			AssertEquals("Country should have empty value.", ZString.Empty, cct.Consignee.Country.Code);
			AssertEquals("Tax Number should have empty value.", ZString.Empty, cct.Consignee.TaxNumber);

			AssertHasMessageError("Company Name should have error message.", address.CompanyNameInfo, "Consignee party name and address information is required.");
			AssertHasMessageError("Address Formatted should have error message.", address.AddressLine1Info, "Consignee Address is required for CCT messaging.");
			AssertHasMessageError("Tax Number should have error message.", address.TaxNumberInfo, "CNPJ is required for CCT messaging.");

			address.CompanyName = "Company";
			address.Postcode = "00666";
			address.AddressLine1 = "Street";
			address.City = "SAO PAULO";
			address.Country.Code = "BR";
			address.TaxNumber = "159753684581460";
			address.Contact = "Contact01";
			address.Phone = "Phone01";
			address.Email = "Email01";

			cct.ValidateAllIncludingChildren();

			AssertNotEquals("Company Name should not have empty value.", ZString.Empty, cct.Consignee.CompanyName);
			AssertNotEquals("Postcode should not have empty value.", ZString.Empty, cct.Consignee.Postcode);
			AssertNotEquals("Street Name should not have empty value.", ZString.Empty, cct.Consignee.AddressLine1);
			AssertNotEquals("City should not have empty value.", ZString.Empty, cct.Consignee.City);
			AssertNotEquals("Country should not have empty value.", ZString.Empty, cct.Consignee.Country.Code);
			AssertNotEquals("Tax Number should not have empty value.", ZString.Empty, cct.Consignee.TaxNumber);

			AssertNoMessageError("Company Name should have no error message.", address.CompanyNameInfo, "Consignee party name and address information is required.");
			AssertNoMessageError("Address Formatted should have no error message.", address.AddressLine1Info, "Consignee Address is required for CCT messaging.");
			AssertNoMessageError("Tax Number should have no error message.", address.TaxNumberInfo, "CNPJ is required for CCT messaging.");

			address.AddressLine1 = "very long street, very long";
			address.AddressLine2 = "and there is even more to come, it just keeps on coming";
			AssertHasWarning("Both Address Lines are too long", address.AddressLine1Info, "Consignee's Address line 1 & 2 should not exceed 68 characters to comply with Cargo Control and Transit (CCT) system requirements.\r\nOnly the first 68 characters will be sent in the message to CCT.");
		}

		public void TestImportAgentValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol, "081001");

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, ZString.Empty, 12, 25, "goods", "Signature", "AUSYD", "BRSAO", ZDateTime.UtcNow, "notes");

			var importAgent = Factory.New<OrgHeader>();
			importAgent.OH_FullName = "";
			importAgent.MainAddress.Postcode = "";
			importAgent.MainAddress.Address1 = "";
			importAgent.MainAddress.City = "";
			importAgent.MainAddress.OA_RN_NKCountryCode = "";

			consol.JK_OA_ReceivingForwarderAddress = importAgent.MainAddress.PK;

			var builder = new CargoControlAndTransitBuilder(shipment);
			var cct = builder.Build();

			Address address = cct.ImportAgent as Address;
			address.TaxNumber = "";

			cct.ValidateAllIncludingChildren();

			AssertEquals("Company Name should have empty value.", ZString.Empty, cct.ImportAgent.CompanyName);
			AssertEquals("Postcode should have empty value.", ZString.Empty, cct.ImportAgent.Postcode);
			AssertEquals("Street Name should have empty value.", ZString.Empty, cct.ImportAgent.AddressLine1);
			AssertEquals("City should have empty value.", ZString.Empty, cct.ImportAgent.City);
			AssertEquals("Country should have empty value.", ZString.Empty, cct.ImportAgent.Country.Code);
			AssertEquals("Tax Number should have empty value.", ZString.Empty, cct.ImportAgent.TaxNumber);

			AssertHasMessageError("Company Name should have error message.", address.CompanyNameInfo, "Import Agent party name and address information is required.");
			AssertHasMessageError("Address Formatted should have error message.", address.AddressLine1Info, "Import Agent Address is required for CCT messaging.");
			AssertHasMessageError("Tax Number should have error message.", address.TaxNumberInfo, "CNPJ is required for CCT messaging.");

			address.CompanyName = "Company";
			address.Postcode = "00666";
			address.AddressLine1 = "Street";
			address.City = "SAO PAULO";
			address.Country.Code = "BR";
			address.TaxNumber = "159753684581460";
			address.Contact = "Contact01";
			address.Phone = "Phone01";
			address.Email = "Email01";

			cct.ValidateAllIncludingChildren();

			AssertNotEquals("Company Name should not have empty value.", ZString.Empty, cct.ImportAgent.CompanyName);
			AssertNotEquals("Postcode should not have empty value.", ZString.Empty, cct.ImportAgent.Postcode);
			AssertNotEquals("Street Name should not have empty value.", ZString.Empty, cct.ImportAgent.AddressLine1);
			AssertNotEquals("City should not have empty value.", ZString.Empty, cct.ImportAgent.City);
			AssertNotEquals("Country should not have empty value.", ZString.Empty, cct.ImportAgent.Country.Code);
			AssertNotEquals("Tax Number should not have empty value.", ZString.Empty, cct.ImportAgent.TaxNumber);

			AssertNoMessageError("Company Name should have no error message.", address.CompanyNameInfo, "Import Agent party name and address information is required.");
			AssertNoMessageError("Address Formatted should have no error message.", address.AddressFormattedInfo, "Import Agent Address is required for CCT messaging.");
			AssertNoMessageError("Tax Number should have no error message.", address.TaxNumberInfo, "CNPJ is required for CCT messaging.");

			address.AddressLine1 = "very long street, very long";
			address.AddressLine2 = "and there is even more to come, it just keeps on coming and coming and coming and coming and coming";
			AssertHasWarning("Both Address Lines are too long", address.AddressLine1Info, "Import Agent's Address line 1 & 2 should not exceed 68 characters to comply with Cargo Control and Transit (CCT) system requirements.\r\nOnly the first 68 characters will be sent in the message to CCT.");
		}

		public void TestAirportOfDepartureValidation()
		{
			var shipment = Factory.New<ForwardingShipment>();
			PopulateShipment(shipment, "081001", 12, 25, "goods", "Signature", "", "BRSAO", new ZDateTime(2019, 11, 12), "notes");

			var builder = new CargoControlAndTransitBuilder(shipment);
			var cct = builder.Build();

			CodeDescription codeDescription = cct.AirportOfDeparture as CodeDescription;

			var errorMessageDescription = "Airport Of Departure is required for CCT messaging.";
			var errorMessageCode = "Airport Of Departure Code is required for CCT messaging.";

			AssertEquals("Airport Of Departure Code should have empty value.", ZString.Empty, codeDescription.Code);
			AssertHasMessageError("Airport Of Departure Code should have error message.", codeDescription.CodeInfo, errorMessageCode);

			AssertEquals("Airport Of Departure should have empty value.", ZString.Empty, codeDescription.Description);
			AssertHasMessageError("Airport Of Departure should have error message.", codeDescription.DescriptionInfo, errorMessageDescription);

			cct.AirportOfDeparture.Code = "AUSYD";
			cct.AirportOfDeparture.Description = "Sydney";
			cct.ValidateAllIncludingChildren();
			AssertNoMessageError("Airport Of Departure Code should have no error message.", codeDescription.CodeInfo, errorMessageCode);
			AssertNoMessageError("Airport Of Departure should have no error message.", codeDescription.DescriptionInfo, errorMessageDescription);
		}

		public void TestAirportOfDestinationValidation()
		{
			var shipment = Factory.New<ForwardingShipment>();
			PopulateShipment(shipment, "081001", 12, 25, "goods", "Signature", "AUSYD", "", new ZDateTime(2019, 11, 12), "notes");

			var builder = new CargoControlAndTransitBuilder(shipment);
			var cct = builder.Build();

			CodeDescription codeDescription = cct.AirportOfDestination as CodeDescription;

			var errorMessage = "Airport Of Destination is required for CCT messaging.";
			AssertEquals("Airport Of Destination should have empty value.", ZString.Empty, codeDescription.Description);
			AssertHasMessageError("Airport Of Destination should have error message.", codeDescription.DescriptionInfo, errorMessage);

			cct.AirportOfDestination.Code = "BRSAO";
			cct.AirportOfDestination.Description = "Sao Paulo";
			cct.ValidateAllIncludingChildren();
			AssertNoMessageError("Airport Of Destination should have no error message.", codeDescription.DescriptionInfo, errorMessage);
		}

		public void TestSpecialHandlingValidation()
		{
			var shipment = Factory.New<ForwardingShipment>();
			PopulateShipment(shipment, "081001", 12, 25, "goods", "Signature", "AUSYD", "BRSAO", new ZDateTime(2019, 11, 12), "notes");

			PopulateRateLines(shipment);
			PopulatePrepaidAndCollectValues(shipment);

			var builder = new CargoControlAndTransitBuilder(shipment);
			var cct = builder.Build();

			PopulateAndAssertSpecialHandlingCodes(cct);
		}

		public void TestTotalNoOfFiecesAndGrossWeight()
		{
			var shipment = Factory.New<ForwardingShipment>();
			PopulateShipment(shipment, "081001", 12, 25, "goods", "Signature", "AUSYD", "BRSAO", new ZDateTime(2019, 11, 12), "notes");

			PopulateRateLines(shipment);
			PopulatePrepaidAndCollectValues(shipment);

			var builder = new CargoControlAndTransitBuilder(shipment);
			var cct = builder.Build();

			cct.TotalNoOfPieces = 0;
			cct.TotalGrossWeight.Value = 0;
			cct.ValidateAllIncludingChildren();

			AssertHasMessageError("Total No of Pieces should have error message", cct.TotalNoOfPiecesInfo,
				"Total No Of Pieces greater than zero is required for CCT messaging.");
			AssertHasMessageError("Total Gross Weight should have error message", ((Measurement)cct.TotalGrossWeight).ValueInfo,
				"Total Gross Weight greater than zero is required for CCT messaging.");

			cct.TotalNoOfPieces = 50;
			cct.TotalGrossWeight.Value = 30;
			cct.ValidateAllIncludingChildren();

			AssertNoMessageError("Total No of Pieces should have no error message", cct.TotalNoOfPiecesInfo,
				"Total No Of Pieces greater than zero is required for CCT messaging.");
			AssertNoMessageError("Total Gross Weight should have no error message", ((Measurement)cct.TotalGrossWeight).ValueInfo,
				"Total Gross Weight greater than zero is required for CCT messaging.");
		}

		public void TestValidCharacters()
		{
			var shipment = Factory.New<ForwardingShipment>();
			PopulateShipment(shipment, "081001", 12, 25, "goods", "Signature", "AUSYD", "BRSAO", new ZDateTime(2019, 11, 12), "notes");

			PopulateRateLines(shipment);
			PopulatePrepaidAndCollectValues(shipment);

			var builder = new CargoControlAndTransitBuilder(shipment);
			var cct = builder.Build();

			var rateLine = cct.RateLines.ElementAt(0);

			var errorMessage =
				"This text contains characters not supported by the Brazil Customs.";

			rateLine.NatureAndQtyOfGoods = "abcd 传/傳 片仮名 기윽 0123";
			cct.ValidateAllIncludingChildren();
			AssertHasMessageError("Nature And Qty Of Goods should have error message", rateLine.NatureAndQtyOfGoodsInfo, errorMessage);

			rateLine.NatureAndQtyOfGoods = "abcd àãóâ 0123";
			cct.ValidateAllIncludingChildren();
			AssertNoMessageError("Nature And Qty Of Goods should have no error message", rateLine.NatureAndQtyOfGoodsInfo, errorMessage);
		}

		public void TestValidGoodsDescription()
		{
			var shipment = Factory.New<ForwardingShipment>();
			PopulateShipment(shipment, "081001", 12, 25, "", "Signature", "AUSYD", "BRSAO", new ZDateTime(2019, 11, 12), "");

			AddRateLineToAWBHeader("5", 4, "K", "Q", "N", 81, 2, 3, new string('1', 35), shipment);
			AddRateLineToAWBHeader("5", 4, "K", "Q", "N", 81, 2, 3, new string('2', 35), shipment);
			AddRateLineToAWBHeader("5", 4, "K", "Q", "N", 81, 2, 3, new string('3', 35), shipment);
			AddRateLineToAWBHeader("5", 4, "K", "Q", "N", 81, 2, 3, new string('4', 35), shipment);
			AddRateLineToAWBHeader("5", 4, "K", "Q", "N", 81, 2, 3, new string('5', 35), shipment);
			AddRateLineToAWBHeader("5", 4, "K", "Q", "N", 81, 2, 3, new string('6', 35), shipment);
			AddRateLineToAWBHeader("5", 4, "K", "Q", "N", 81, 2, 3, new string('7', 35), shipment);
			AddRateLineToAWBHeader("5", 4, "K", "Q", "N", 81, 2, 3, new string('8', 35), shipment);
			AddRateLineToAWBHeader("5", 4, "K", "Q", "N", 81, 2, 3, new string('9', 35), shipment);
			AddRateLineToAWBHeader("5", 4, "K", "Q", "N", 81, 2, 3, new string('0', 35), shipment);
			AddRateLineToAWBHeader("5", 4, "K", "Q", "N", 81, 2, 3, new string('1', 35), shipment);
			AddRateLineToAWBHeader("5", 4, "K", "Q", "N", 81, 2, 3, new string('2', 35), shipment);
			AddRateLineToAWBHeader("5", 4, "K", "Q", "N", 81, 2, 3, new string('3', 35), shipment);
			AddRateLineToAWBHeader("5", 4, "K", "Q", "N", 81, 2, 3, new string('4', 35), shipment);
			AddRateLineToAWBHeader("5", 4, "K", "Q", "N", 81, 2, 3, new string('5', 35), shipment);
			AddRateLineToAWBHeader("5", 4, "K", "Q", "N", 81, 2, 3, new string('6', 35), shipment);
			AddRateLineToAWBHeader("5", 4, "K", "Q", "N", 81, 2, 3, new string('7', 35), shipment);
			AddRateLineToAWBHeader("5", 4, "K", "Q", "N", 81, 2, 3, new string('8', 35), shipment);
			AddRateLineToAWBHeader("5", 4, "K", "Q", "N", 81, 2, 3, new string('9', 35), shipment);

			var builder = new CargoControlAndTransitBuilder(shipment);
			var cct = builder.Build();
			var rateLine = cct.RateLines.ElementAt(0);
			AssertHasMessageError("Nature And Qty Of Goods should have error message", rateLine.NatureAndQtyOfGoodsInfo, "Goods Des is mandatory for Brazil import");
			AssertHasMessageError("Nature And Qty Of Goods should have error message", rateLine.NatureAndQtyOfGoodsInfo, "Exceeds 600 characters, not actually Goods Description");
		}

		public void TestValidRUCReferenceNumber()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var entryNum1 = shipment.Numbers.AddNew();
			entryNum1.CE_EntryType = BrazilAdditionalReferenceNumberTypes.Codes.RUC;
			entryNum1.CE_EntryNum = "6BR123456789D0VAHK001";

			PopulateShipment(shipment, "081001", 12, 25, "goods", "Signature", "AUSYD", "BRSAO", new ZDateTime(2019, 11, 12), "notes");
			var builder1 = new CargoControlAndTransitBuilder(shipment);
			var cct1 = builder1.Build();
			AssertEquals("RUC ReferenceNumber should have value.", entryNum1.CE_EntryNum, cct1.RUCReferenceNumber);
			AssertNoMessageError("RUC ReferenceNumber should not have error message", cct1.RUCReferenceNumberInfo, "CCT supports only one RUC and first available RUC will be sent.");

			var entryNum2 = shipment.Numbers.AddNew();
			entryNum2.CE_EntryType = BrazilAdditionalReferenceNumberTypes.Codes.RUC;
			entryNum2.CE_EntryNum = "6BR987654321DOVAHK002";

			var builder2 = new CargoControlAndTransitBuilder(shipment);
			var cct2 = builder2.Build();
			AssertEquals("RUC ReferenceNumber should have first value.", entryNum1.CE_EntryNum, cct2.RUCReferenceNumber);
			AssertHasWarningContaining(cct2.RUCReferenceNumberInfo, "CCT supports only one RUC and first available RUC will be sent.");
			AssertNoMessageError("RUC ReferenceNumber should not have error message", cct2.RUCReferenceNumberInfo, "CCT supports only one RUC and first available RUC will be sent.");
		}

		public void TestValidTotalCharges()
		{
			var shipment = Factory.New<ForwardingShipment>();
			PopulateShipment(shipment, "081001", 12, 25, "goods", "Signature", "AUSYD", "BRSAO", new ZDateTime(2019, 11, 12), "notes");

			var builder1 = new CargoControlAndTransitBuilder(shipment);
			var cct1 = builder1.Build();
			AssertEquals("Total Prepaid", 0M, cct1.TotalPrepaid);
			AssertHasMessageError("Total Prepaid should not have error message", cct1.TotalPrepaidInfo, "Total Charges are required for CCT Shipment messages.");
			AssertEquals("Total Collect", 0M, cct1.TotalCollect);
			AssertHasMessageError("Total Collect should not have error message", cct1.TotalCollectInfo, "Total Charges are required for CCT Shipment messages.");

			shipment.AWBHeader.EH_ValuationPPD = 1;
			var builder2 = new CargoControlAndTransitBuilder(shipment);
			var cct2 = builder2.Build();
			AssertEquals("Total Prepaid", 1M, cct2.TotalPrepaid);
			AssertNoMessageError("Total Prepaid should not have error message", cct2.TotalPrepaidInfo, "Total Charges are required for CCT Shipment messages.");
			AssertEquals("Total Collect", 0M, cct2.TotalCollect);
			AssertNoMessageError("Total Collect should not have error message", cct2.TotalCollectInfo, "Total Charges are required for CCT Shipment messages.");

			shipment.AWBHeader.EH_ValuationPPD = 0;
			shipment.AWBHeader.EH_ValuationCOL = 1;
			var builder3 = new CargoControlAndTransitBuilder(shipment);
			var cct3 = builder3.Build();
			AssertEquals("Total Prepaid", 0M, cct3.TotalPrepaid);
			AssertNoMessageError("Total Prepaid should not have error message", cct3.TotalPrepaidInfo, "Total Charges are required for CCT Shipment messages.");
			AssertEquals("Total Collect", 1M, cct3.TotalCollect);
			AssertNoMessageError("Total Collect should not have error message", cct3.TotalCollectInfo, "Total Charges are required for CCT Shipment messages.");
		}

		public void TestTaxNumberOfAddress()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol, "081001");

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, ZString.Empty, 12, 25, "goods", "Signature", "AUSYD", "BRSAO", ZDateTime.UtcNow, "notes");

			var importAgent = Factory.New<OrgHeader>();
			importAgent.OH_FullName = "";
			importAgent.MainAddress.Postcode = "";
			importAgent.MainAddress.Address1 = "";
			importAgent.MainAddress.City = "";
			importAgent.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;

			consol.JK_OA_ReceivingForwarderAddress = importAgent.MainAddress.PK;

			importAgent.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CMT, "90.117.749/7654-80", Core.Constants.CountryCodes.Brazil);
			importAgent.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.RSN, "800.000.08", Core.Constants.CountryCodes.Brazil);
			importAgent.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "PAS1245.00.25", Core.Constants.CountryCodes.Brazil);

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "";
			shipper.MainAddress.Postcode = "";
			shipper.MainAddress.Address1 = "";
			shipper.MainAddress.City = "";
			shipper.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			shipper.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CMT, "90.117.749/7654-80", Core.Constants.CountryCodes.Brazil);
			shipper.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration, "800.000.08", Core.Constants.CountryCodes.Brazil);
			shipper.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.RLP, "PAS1245.00.25", Core.Constants.CountryCodes.Brazil);

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "";
			consignee.MainAddress.Postcode = "";
			consignee.MainAddress.Address1 = "";
			consignee.MainAddress.City = "";
			consignee.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			consignee.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "90.117.749/7654-80", Core.Constants.CountryCodes.Brazil);
			consignee.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration, "800.000.08", Core.Constants.CountryCodes.Brazil);
			consignee.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "PAS1245.00.25", Core.Constants.CountryCodes.Brazil);

			var builder = new CargoControlAndTransitBuilder(shipment);
			var cct = builder.Build();

			AssertEquals("Consignee TaxNumber", "90.117.749/7654-80", cct.Consignee.TaxNumber);
			AssertEquals("Consignee TaxNumber Type", "CJN", cct.Consignee.TaxNumberType.Code);

			AssertEquals("ShipperTaxNumber", "800.000.08", cct.Shipper.TaxNumber);
			AssertEquals("Shipper TaxNumber Type", "CPF", cct.Shipper.TaxNumberType.Code);

			AssertEquals("ImportAgent TaxNumber", "PAS1245.00.25", cct.ImportAgent.TaxNumber);
			AssertEquals("ImportAgent TaxNumber Type", "PAS", cct.ImportAgent.TaxNumberType.Code);
		}

		public void TestPortOfFirstArrivalWithRouting()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_PortName = "BRTEST";
			unloco.RL_Code = "BRTTT";

			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol, "210910");
			consol.JK_RL_NKDischargePort = "BRTTT";

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, ZString.Empty, 12, 25, "goods", "Signature", "AUSYD", "BRSAO", ZDateTime.UtcNow, "notes");

			var builder = new CargoControlAndTransitBuilder(shipment);
			var cct = builder.Build();

			AssertEquals("BRTTT", cct.PortOfFirstArrival.Code);
		}

		public void TestPortOfFirstArrivalWithoutRouting()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_PortName = "BRTEST";
			unloco.RL_Code = "BRTTT";

			var shipment = Factory.New<ForwardingShipment>();
			PopulateShipment(shipment, ZString.Empty, 12, 25, "goods", "Signature", "AUSYD", "BRTTT", ZDateTime.UtcNow, "notes");

			var builder = new CargoControlAndTransitBuilder(shipment);
			var cct = builder.Build();

			AssertEquals("BRTTT", cct.PortOfFirstArrival.Code);
		}

		public void TestPopulateRateLines()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "AAA";

			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "BBB";

			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_Code = "CCC";
			branch1.GB_GC = company1.PK;
			branch1.GB_RL_NKHomePort = "USPHL";

			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_Code = "DDD";
			branch2.GB_RL_NKHomePort = "USLAX";
			branch2.GB_GC = company2.PK;

			var department = Factory.New<GlbDepartment>();
			department.GE_Code = "TTT";

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol, "081001");

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, ZString.Empty, 12, 25, "goods", "Signature", "AUSYD", "BRSAO", ZDateTime.UtcNow, "notes");

			var packline = shipment.OuterPackLines.Cast<ForwardingPackLine>().First();
			var hc1 = packline.HarmonisedCodes.AddNew();
			hc1.JLH_RN_NKCountry = "AU";
			hc1.JLH_Code = "111111111111111";
			var hc2 = packline.HarmonisedCodes.AddNew();
			hc2.JLH_RN_NKCountry = "AU";
			hc2.JLH_Code = "222222222222222";
			shipment.AWBHeader.Populate();

			AssertEquals("Pre-condition: AWBHeader.AWBRateLine1.NatureAndQtyOfGoods.Text", "notes", shipment.AWBHeader.AWBRateLine1.NatureAndQtyOfGoods.Text);
			AssertEquals("Pre-condition: AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.Text", "HS Codes: 111111111111111,", shipment.AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.Text);
			AssertEquals("Pre-condition: AWBHeader.AWBRateLine3.NatureAndQtyOfGoods.Text", "222222222222222", shipment.AWBHeader.AWBRateLine3.NatureAndQtyOfGoods.Text);
			AssertEquals("Pre-condition: AWBHeader.AWBRateLine4.NatureAndQtyOfGoods.Text", "No Dimensions Available", shipment.AWBHeader.AWBRateLine4.NatureAndQtyOfGoods.Text);

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				var job = Factory.NewJobForTesting<JobHeader>();
				job.JH_ParentID = shipment.PK;
				job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				job.JH_GC = company1.PK;

				var jobCharge1 = Factory.NewWithValidTestData<JobCharge>();
				jobCharge1.JR_JH = shipment.Job.PK;
				jobCharge1.JR_AC = Env.Registry.FreightChargeCode;
				jobCharge1.JR_OSSellAmt = 100m;

				Factory.Save();

				var builder = new CargoControlAndTransitBuilder(shipment);
				var cct = builder.Build();

				Assert(!cct.RateLines.First().RateChargeOrDiscount.IsEmpty);
				AssertNotEquals(0m, cct.RateLines.First().ChargeableWeight.Value * cct.RateLines.First().RateChargeOrDiscount);
				AssertEquals(cct.RateLines.ElementAt(0).NatureAndQtyOfGoods, shipment.AWBHeader.AWBRateLine1.NatureAndQtyOfGoods.Text);
				AssertEquals(cct.RateLines.ElementAt(1).NatureAndQtyOfGoods, shipment.AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.Text);
				AssertEquals(cct.RateLines.ElementAt(2).NatureAndQtyOfGoods, shipment.AWBHeader.AWBRateLine3.NatureAndQtyOfGoods.Text);
				AssertEquals(cct.RateLines.ElementAt(3).NatureAndQtyOfGoods, shipment.AWBHeader.AWBRateLine4.NatureAndQtyOfGoods.Text);
				Assert(!cct.RateLines.ElementAt(0).IsHSCodeLine);
				Assert(cct.RateLines.ElementAt(1).IsHSCodeLine);
				Assert(cct.RateLines.ElementAt(2).IsHSCodeLine);
				Assert(!cct.RateLines.ElementAt(3).IsHSCodeLine);
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch2.PK.ToGuid(), department.PK.ToGuid()))
			{
				var builder = new CargoControlAndTransitBuilder(shipment);
				var cct = builder.Build();

				Assert(!cct.RateLines.First().RateChargeOrDiscount.IsEmpty);
				AssertNotEquals(0m, cct.RateLines.First().ChargeableWeight.Value * cct.RateLines.First().RateChargeOrDiscount);
				AssertEquals(cct.RateLines.ElementAt(0).NatureAndQtyOfGoods, shipment.AWBHeader.AWBRateLine1.NatureAndQtyOfGoods.Text);
				AssertEquals(cct.RateLines.ElementAt(1).NatureAndQtyOfGoods, shipment.AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.Text);
				AssertEquals(cct.RateLines.ElementAt(2).NatureAndQtyOfGoods, shipment.AWBHeader.AWBRateLine3.NatureAndQtyOfGoods.Text);
				AssertEquals(cct.RateLines.ElementAt(3).NatureAndQtyOfGoods, shipment.AWBHeader.AWBRateLine4.NatureAndQtyOfGoods.Text);
				Assert(!cct.RateLines.ElementAt(0).IsHSCodeLine);
				Assert(cct.RateLines.ElementAt(1).IsHSCodeLine);
				Assert(cct.RateLines.ElementAt(2).IsHSCodeLine);
				Assert(!cct.RateLines.ElementAt(3).IsHSCodeLine);
			}

			var message = consol.CIMEDIMessages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = CIMEDIMessage.MessageTypes.Sent.FWB;
			message.EM_GB = branch1.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch2.PK.ToGuid(), department.PK.ToGuid()))
			{
				var builder = new CargoControlAndTransitBuilder(shipment);
				var cct = builder.Build();

				Assert(!cct.RateLines.First().RateChargeOrDiscount.IsEmpty);
				AssertNotEquals(0m, cct.RateLines.First().ChargeableWeight.Value * cct.RateLines.First().RateChargeOrDiscount);
				AssertEquals(cct.RateLines.ElementAt(0).NatureAndQtyOfGoods, shipment.AWBHeader.AWBRateLine1.NatureAndQtyOfGoods.Text);
				AssertEquals(cct.RateLines.ElementAt(1).NatureAndQtyOfGoods, shipment.AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.Text);
				AssertEquals(cct.RateLines.ElementAt(2).NatureAndQtyOfGoods, shipment.AWBHeader.AWBRateLine3.NatureAndQtyOfGoods.Text);
				AssertEquals(cct.RateLines.ElementAt(3).NatureAndQtyOfGoods, shipment.AWBHeader.AWBRateLine4.NatureAndQtyOfGoods.Text);
				Assert(!cct.RateLines.ElementAt(0).IsHSCodeLine);
				Assert(cct.RateLines.ElementAt(1).IsHSCodeLine);
				Assert(cct.RateLines.ElementAt(2).IsHSCodeLine);
				Assert(!cct.RateLines.ElementAt(3).IsHSCodeLine);
			}
		}

		public void TestPopulatePrepaidAndCollectValues()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "AAA";

			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "BBB";

			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_Code = "CCC";
			branch1.GB_GC = company1.PK;
			branch1.GB_RL_NKHomePort = "USPHL";

			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_Code = "DDD";
			branch2.GB_RL_NKHomePort = "USLAX";
			branch2.GB_GC = company2.PK;

			var department = Factory.New<GlbDepartment>();
			department.GE_Code = "TTT";

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol, "081001");

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, ZString.Empty, 12, 25, "goods", "Signature", "AUSYD", "BRSAO", ZDateTime.UtcNow, "notes");

			ZDecimal totalWeightPPD, totalWeightCOL,
				valuationPPD, valuationCOL,
				taxesPPD, taxesCOL,
				otherChargesDueAgentPPD, otherChargesDueAgentCOL,
				totalPrepaid, totalCollect;

			ZString weightPrepaidCollectCode, otherPrepaidCollectCode;

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				var job = Factory.NewJobForTesting<JobHeader>();
				job.JH_ParentID = shipment.PK;
				job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				job.JH_GC = company1.PK;

				var jobCharge1 = Factory.NewWithValidTestData<JobCharge>();
				jobCharge1.JR_JH = shipment.Job.PK;
				jobCharge1.JR_AC = Env.Registry.FreightChargeCode;
				jobCharge1.JR_OSSellAmt = 100m;

				Factory.Save();

				var builder = new CargoControlAndTransitBuilder(shipment);
				var cct = builder.Build();

				totalWeightPPD = cct.TotalWeightPPD;
				totalWeightCOL = cct.TotalWeightCOL;

				valuationPPD = cct.ValuationPPD;
				valuationCOL = cct.ValuationCOL;

				taxesPPD = cct.TaxesPPD;
				taxesCOL = cct.TaxesCOL;

				otherChargesDueAgentPPD = cct.OtherChargesDueAgentPPD;
				otherChargesDueAgentCOL = cct.OtherChargesDueAgentCOL;

				weightPrepaidCollectCode = cct.WeightPrepaidCollect.Code;
				otherPrepaidCollectCode = cct.OtherPrepaidCollect.Code;

				totalPrepaid = cct.TotalPrepaid;
				totalCollect = cct.TotalCollect;
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch2.PK.ToGuid(), department.PK.ToGuid()))
			{
				var builder = new CargoControlAndTransitBuilder(shipment);
				var cct = builder.Build();

				AssertEquals(totalWeightCOL, cct.TotalWeightCOL);
				AssertEquals(totalCollect, cct.TotalCollect);
			}

			var message = consol.CIMEDIMessages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = CIMEDIMessage.MessageTypes.Sent.FWB;
			message.EM_GB = branch1.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch2.PK.ToGuid(), department.PK.ToGuid()))
			{
				var builder = new CargoControlAndTransitBuilder(shipment);
				var cct = builder.Build();

				AssertEquals(totalWeightPPD, cct.TotalWeightPPD);
				AssertEquals(totalWeightCOL, cct.TotalWeightCOL);

				AssertEquals(valuationPPD, cct.ValuationPPD);
				AssertEquals(valuationCOL, cct.ValuationCOL);

				AssertEquals(otherChargesDueAgentPPD, cct.OtherChargesDueAgentPPD);
				AssertEquals(otherChargesDueAgentCOL, cct.OtherChargesDueAgentCOL);

				AssertEquals(weightPrepaidCollectCode, cct.WeightPrepaidCollect.Code);
				AssertEquals(otherPrepaidCollectCode, cct.OtherPrepaidCollect.Code);

				AssertEquals(totalPrepaid, cct.TotalPrepaid);
				AssertEquals(totalCollect, cct.TotalCollect);
			}
		}

		[TestDate(2023, 06, 06)]
		public void TestPopulateSignature()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "AAA";
			company1.GC_RN_NKCountryCode = CountryCodes.HongKong;

			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "BBB";
			company2.GC_RN_NKCountryCode = CountryCodes.Brazil;

			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_Code = "CCC";
			branch1.GB_GC = company1.PK;
			branch1.GB_City = "Hong Kong";
			branch1.GB_RL_NKHomePort = "HK8ST";

			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_Code = "DDD";
			branch2.GB_RL_NKHomePort = "BR6MO";
			branch2.GB_GC = company2.PK;
			branch2.GB_City = "Minas Gerais";

			var department = Factory.New<GlbDepartment>();
			department.GE_Code = "TTT";

			var user1 = Factory.New<GlbStaff>();
			user1.GS_FullName = "User1";
			user1.GS_Code = "UR1";
			user1.GS_LoginName = "User1";
			user1.GS_EmailAddress = "User1@mail.box";

			var certificate = user1.Certificates.AddNew();
			certificate.XZ_Type = StaffCertificateType.DG;
			certificate.XZ_RefNumber = "LicenseNumber";

			var user2 = Factory.New<GlbStaff>();
			user2.GS_FullName = "User2";
			user2.GS_Code = "UR2";
			user2.GS_LoginName = "User2";
			user2.GS_EmailAddress = "user2@mail.box";

			var user3 = Factory.New<GlbStaff>();
			user3.GS_FullName = "User3";
			user3.GS_Code = "UR3";
			user3.GS_LoginName = "User3";
			user3.GS_EmailAddress = "user3@mail.box";

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_Code = "ABC";

			var addressCountryData = sendingForwarder.MainAddress.KnownShipperDetails.AddNew();
			addressCountryData.OV_OH_OrgHeader = sendingForwarder.PK;
			addressCountryData.OV_OA_ApprovedLocation = sendingForwarder.MainAddress.PK;
			addressCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
			addressCountryData.OV_EXApprovalNumber = "RA12345";
			addressCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
			addressCountryData.OV_RN_NKClientCountryRelation = CountryCodes.HongKong;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol, "081001");
			consol.Transports[0].JW_DepotCutOff = new ZDateTime(2023, 06, 01);

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, ZString.Empty, 12, 25, "goods", "Signature", "HK8ST", "BR6MO", ZDateTime.UtcNow, "notes");
			shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = sendingForwarder.MainAddress.PK;

			Factory.Save();

			consol.JK_MasterBillIssueDate = ZDateTime.Empty;
			Factory.Save();

			FreightConfigurationRegistry.Instance.AWBIssueDate.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, FreightConfigurationRegistry.Instance.AWBIssueDateIsCutOffDate);
			FreightConfigurationRegistry.Instance.AWBIssueDate.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, FreightConfigurationRegistry.Instance.AWBIssueDateIsTodaysDate);

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				DataRegistry.Instance.Freight.AirWaybill.IssuingCarrierAgentName = "Test1";
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch2.PK.ToGuid(), department.PK.ToGuid()))
			{
				DataRegistry.Instance.Freight.AirWaybill.IssuingCarrierAgentName = "Test2";
			}

			using (Env.SetTemporaryUserContext(user1.PK.ToGuid(), branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				var builder = new CargoControlAndTransitBuilder(new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK));
				var cct = builder.Build();

				CombineAssertions(() =>
				{
					AssertEquals("User1 LicenseNumber", cct.ShippersSignature);
					AssertEquals(new ZDateTime(2023, 06, 01), cct.IssueDate);
					AssertEquals("Hong Kong", cct.IssuePlace);
					AssertEquals("Test1", cct.AgentsSignature);
					AssertEquals("RA12345", cct.AgentApprovedExporterNumber);
				});
			}

			using (Env.SetTemporaryUserContext(user2.PK.ToGuid(), branch2.PK.ToGuid(), department.PK.ToGuid()))
			{
				var builder = new CargoControlAndTransitBuilder(new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK));
				var cct = builder.Build();

				CombineAssertions(() =>
				{
					AssertEquals("User2", cct.ShippersSignature);
					AssertEquals(new ZDateTime(2023, 06, 06), cct.IssueDate);
					AssertEquals("Minas Gerais", cct.IssuePlace);
					AssertEquals("Test2", cct.AgentsSignature);
					AssertEquals(string.Empty, cct.AgentApprovedExporterNumber);
				});
			}

			using (Env.SetTemporaryUserContext(user3.PK.ToGuid(), branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				var builder = new CargoControlAndTransitBuilder(new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK));
				var cct = builder.Build();

				CombineAssertions(() =>
				{
					AssertEquals("User3", cct.ShippersSignature);
					AssertEquals(new ZDateTime(2023, 06, 01), cct.IssueDate);
					AssertEquals("Hong Kong", cct.IssuePlace);
					AssertEquals("Test1", cct.AgentsSignature);
					AssertEquals("RA12345", cct.AgentApprovedExporterNumber);
				});
			}

			shipment.IsAWBValuesOverriddenProperty = true;
			shipment.AWBHeader.EH_ShippersSignature = "Yes";
			shipment.AWBHeader.EH_AWBIssueDate = new ZDateTime(2000, 01, 01);
			shipment.AWBHeader.EH_AWBIssuePlace = "Nanjing";
			shipment.AWBHeader.EH_AWBAgentsSignature = "N0";
			Factory.Save();

			using (Env.SetTemporaryUserContext(user3.PK.ToGuid(), branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				var builder = new CargoControlAndTransitBuilder(new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK));
				var cct = builder.Build();

				CombineAssertions(() =>
				{
					AssertEquals("Yes", cct.ShippersSignature);
					AssertEquals(new ZDateTime(2000, 01, 01), cct.IssueDate);
					AssertEquals("Nanjing", cct.IssuePlace);
					AssertEquals("N0", cct.AgentsSignature);
					AssertEquals("RA12345", cct.AgentApprovedExporterNumber);
				});
			}
		}

		public void TestPopulateCurrency()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "AAA";
			company1.GC_RN_NKCountryCode = CountryCodes.HongKong;
			company1.SetCurrency("HKD");

			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "BBB";
			company2.GC_RN_NKCountryCode = CountryCodes.Brazil;
			company2.SetCurrency("BRL");

			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_Code = "CCC";
			branch1.GB_GC = company1.PK;
			branch1.GB_City = "Hong Kong";
			branch1.GB_RL_NKHomePort = "HK8ST";

			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_Code = "DDD";
			branch2.GB_RL_NKHomePort = "BR6MO";
			branch2.GB_GC = company2.PK;
			branch2.GB_City = "Minas Gerais";

			var department = Factory.New<GlbDepartment>();
			department.GE_Code = "TTT";

			var user1 = Factory.New<GlbStaff>();
			user1.GS_FullName = "User1";
			user1.GS_Code = "UR1";
			user1.GS_LoginName = "User1";
			user1.GS_EmailAddress = "User1@mail.box";

			var certificate = user1.Certificates.AddNew();
			certificate.XZ_Type = StaffCertificateType.DG;
			certificate.XZ_RefNumber = "LicenseNumber";

			var user2 = Factory.New<GlbStaff>();
			user2.GS_FullName = "User2";
			user2.GS_Code = "UR2";
			user2.GS_LoginName = "User2";
			user2.GS_EmailAddress = "user2@mail.box";

			var user3 = Factory.New<GlbStaff>();
			user3.GS_FullName = "User3";
			user3.GS_Code = "UR3";
			user3.GS_LoginName = "User3";
			user3.GS_EmailAddress = "user3@mail.box";

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol, "081001");

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, ZString.Empty, 12, 25, "goods", "Signature", "HK8ST", "BR6MO", ZDateTime.UtcNow, "notes");
			shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;

			Factory.Save();

			using (Env.SetTemporaryUserContext(user1.PK.ToGuid(), branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				var builder = new CargoControlAndTransitBuilder(new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK));
				AssertEquals("HKD", builder.Build().Currency.Code);
			}

			using (Env.SetTemporaryUserContext(user2.PK.ToGuid(), branch2.PK.ToGuid(), department.PK.ToGuid()))
			{
				var builder = new CargoControlAndTransitBuilder(new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK));
				AssertEquals("BRL", builder.Build().Currency.Code);
			}

			using (Env.SetTemporaryUserContext(user3.PK.ToGuid(), branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				var builder = new CargoControlAndTransitBuilder(new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK));
				AssertEquals("HKD", builder.Build().Currency.Code);
			}

			shipment.IsAWBValuesOverriddenProperty = true;
			shipment.AWBHeader.EH_Currency = "CNY";
			Factory.Save();

			using (Env.SetTemporaryUserContext(user3.PK.ToGuid(), branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				var builder = new CargoControlAndTransitBuilder(new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK));
				AssertEquals("CNY", builder.Build().Currency.Code);
			}
		}

		public void TestPopulateSignature_MessagesFromDifferentTimeZones()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "AAA";
			company1.GC_RN_NKCountryCode = CountryCodes.Australia;

			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "BBB";
			company2.GC_RN_NKCountryCode = CountryCodes.Brazil;

			var company3 = Factory.New<GlbCompany>();
			company3.GC_Code = "CCC";
			company3.GC_RN_NKCountryCode = CountryCodes.UnitedStates;

			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_Code = "DDD";
			branch1.GB_GC = company1.PK;
			branch1.GB_City = "Sydney";
			branch1.GB_RL_NKHomePort = "AUSYD";

			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_Code = "EEE";
			branch2.GB_RL_NKHomePort = "BR6MO";
			branch2.GB_GC = company2.PK;
			branch2.GB_City = "Minas Gerais";

			var branch3 = Factory.New<GlbBranch>();
			branch3.GB_Code = "FFF";
			branch3.GB_RL_NKHomePort = "US237";
			branch3.GB_GC = company3.PK;
			branch3.GB_City = "Great Mills";

			var department = Factory.New<GlbDepartment>();
			department.GE_Code = "TTT";

			var user1 = Factory.New<GlbStaff>();
			user1.GS_FullName = "User1";
			user1.GS_Code = "UR1";
			user1.GS_LoginName = "User1";
			user1.GS_EmailAddress = "User1@mail.box";

			var certificate = user1.Certificates.AddNew();
			certificate.XZ_Type = StaffCertificateType.DG;
			certificate.XZ_RefNumber = "LicenseNumber";

			var user2 = Factory.New<GlbStaff>();
			user2.GS_FullName = "User2";
			user2.GS_Code = "UR2";
			user2.GS_LoginName = "User2";
			user2.GS_EmailAddress = "user2@mail.box";

			var user3 = Factory.New<GlbStaff>();
			user3.GS_FullName = "User3";
			user3.GS_Code = "UR3";
			user3.GS_LoginName = "User3";
			user3.GS_EmailAddress = "user3@mail.box";

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol, "081001");
			consol.Transports[0].JW_DepotCutOff = new ZDateTime(2023, 06, 01);

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, ZString.Empty, 12, 25, "goods", "Signature", "HK8ST", "BR6MO", ZDateTime.UtcNow, "notes");
			shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;

			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				DataRegistry.Instance.Freight.AirWaybill.IssuingCarrierAgentName = "Test1";
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch2.PK.ToGuid(), department.PK.ToGuid()))
			{
				DataRegistry.Instance.Freight.AirWaybill.IssuingCarrierAgentName = "Test2";
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch3.PK.ToGuid(), department.PK.ToGuid()))
			{
				DataRegistry.Instance.Freight.AirWaybill.IssuingCarrierAgentName = "Test3";
			}

			using (Env.SetTemporaryUserContext(user1.PK.ToGuid(), branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				var builder = new CargoControlAndTransitBuilder(new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK));
				var cct = builder.Build();

				CombineAssertions(() =>
				{
					AssertEquals("LicenseNumber User1", cct.ShippersSignature);
					AssertEquals("Sydney", cct.IssuePlace);
					AssertEquals("Test1", cct.AgentsSignature);
				});
			}

			using (Env.SetTemporaryUserContext(user2.PK.ToGuid(), branch2.PK.ToGuid(), department.PK.ToGuid()))
			{
				var builder = new CargoControlAndTransitBuilder(new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK));
				var cct = builder.Build();

				CombineAssertions(() =>
				{
					AssertEquals("User2", cct.ShippersSignature);
					AssertEquals("Minas Gerais", cct.IssuePlace);
					AssertEquals("Test2", cct.AgentsSignature);
				});
			}

			using (Env.SetTemporaryUserContext(user3.PK.ToGuid(), branch3.PK.ToGuid(), department.PK.ToGuid()))
			{
				var builder = new CargoControlAndTransitBuilder(new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK));
				var cct = builder.Build();

				CombineAssertions(() =>
				{
					AssertEquals("User3", cct.ShippersSignature);
					AssertEquals("Great Mills", cct.IssuePlace);
					AssertEquals("Test3", cct.AgentsSignature);
				});
			}
		}

		public void TestExportAgentDefault()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol, "081001");

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, ZString.Empty, 12, 25, "goods", "Signature", "AUSYD", "BRSAO", ZDateTime.UtcNow, "notes");

			var builder = new CargoControlAndTransitBuilder(shipment);
			var cct = builder.Build();

			AssertEquals("ExportAgent Contact", "", cct.ExportAgent.Contact);
			AssertEquals("ExportAgent CompanyName", "", cct.ExportAgent.CompanyName);

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "Sending Forwarder";
			sendingForwarder.OH_RL_NKClosestPort = "BR6MO";
			sendingForwarder.MainAddress.Address1 = "Unit 13";
			sendingForwarder.MainAddress.Address2 = "4 Lost Lane";
			sendingForwarder.MainAddress.City = "Antwerp";
			sendingForwarder.MainAddress.Postcode = "2000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "BE";
			sendingForwarder.MainAddress.CompanyName = "Marvel";
			sendingForwarder.MainAddress.OA_Email = "spider@marvel.com";
			sendingForwarder.MainAddress.OA_Phone = "911";

			var sendingForwarderContact = sendingForwarder.Contacts.AddNew();
			sendingForwarderContact.OC_ContactName = "SPIDERMAN";

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			consol.JK_OC_SendingForwarderContact = sendingForwarderContact.PK;

			builder = new CargoControlAndTransitBuilder(shipment);
			cct = builder.Build();

			AssertEquals("ExportAgent Contact", "SPIDERMAN", cct.ExportAgent.Contact);
			AssertEquals("ExportAgent Email", "spider@marvel.com", cct.ExportAgent.Email);
			AssertEquals("ExportAgent Phone", "911", cct.ExportAgent.Phone);

			AssertEquals("ExportAgent CompanyName", "Marvel", cct.ExportAgent.CompanyName);
			AssertEquals("ExportAgent Address1", "Unit 13", cct.ExportAgent.AddressLine1);
			AssertEquals("ExportAgent Postcode", "2000", cct.ExportAgent.Postcode);
		}

		#region Assert

		void PopulateAndAssertSpecialHandlingCodes(CargoControlAndTransit cct)
		{
			PopulateSpecialHandlingCodes(cct, true);

			AssertEquals("Special Handling Codes should have duplicated value.", "ACT",
				cct.SpecialHandling.ElementAt(7).CodeAndDescription.Code);
			AssertEquals("Airport Of Destination should have two Security Status.", "SCO",
				cct.SpecialHandling.ElementAt(8).CodeAndDescription.Code);

			AssertHasMessageError("Special Handling Codes should have duplicated error message",
				cct.SpecialHandling.ElementAt(7).CodeAndDescription.CodeInfo,
				"Special Handling Codes cannot contain duplicated items");
			AssertHasMessageError("Special Handling Codes should have duplicated error message",
				cct.SpecialHandling.ElementAt(8).CodeAndDescription.CodeInfo,
				"Special Handling Codes can contain one Security Status only");

			PopulateSpecialHandlingCodes(cct, false);

			AssertNotEquals("Special Handling Codes should not have duplicated value.", "ACT",
				cct.SpecialHandling.ElementAt(7).CodeAndDescription.Code);
			AssertNotEquals("Airport Of Destination should not have two Security Status.", "SCO",
				cct.SpecialHandling.ElementAt(8).CodeAndDescription.Code);

			AssertNoMessageError("Special Handling Codes should have duplicated error message",
				cct.SpecialHandling.ElementAt(7).CodeAndDescription.CodeInfo,
				"Special Handling Codes cannot contain duplicated items");
			AssertNoMessageError("Special Handling Codes should have duplicated error message",
				cct.SpecialHandling.ElementAt(8).CodeAndDescription.CodeInfo,
				"Special Handling Codes can contain one Security Status only");
		}

		void AssertCurrencyAndValues(CargoControlAndTransit cct, ForwardingShipment shipment)
		{
			AssertEquals(nameof(cct.Currency), shipment.AWBHeader.EH_Currency, cct.Currency.Code);

			AssertEquals(nameof(cct.CarriageValue.Amount), shipment.AWBHeader.EH_DeclaredValue, cct.CarriageValue.Amount);
			AssertEquals(nameof(cct.CarriageValue.Currency), shipment.AWBHeader.EH_HouseDeclaredValueCurrency, cct.CarriageValue.Currency.Code);

			AssertEquals(nameof(cct.CustomsValue.Amount), shipment.AWBHeader.EH_CustomsValue, cct.CustomsValue.Amount);
			AssertEquals(nameof(cct.CustomsValue.Currency), shipment.AWBHeader.EH_HouseCustomsValueCurrency, cct.CustomsValue.Currency.Code);

			AssertEquals(nameof(cct.InsuranceValue.Amount), shipment.AWBHeader.EH_InsuranceValue, cct.InsuranceValue.Amount);
			AssertEquals(nameof(cct.InsuranceValue.Currency), shipment.AWBHeader.EH_HouseInsuranceValueCurrency, cct.InsuranceValue.Currency.Code);

			var insuranceValue = (Money)cct.InsuranceValue;
			var insuranceValueErrorMessage = "Insurance Value is required for CCT messaging.";

			insuranceValue.Amount = -10;

			cct.ValidateAllIncludingChildren();

			AssertHasMessageError("Insurance Value should have error message", insuranceValue.AmountInfo,
				insuranceValueErrorMessage);

			insuranceValue.Amount = 50;

			cct.ValidateAllIncludingChildren();

			AssertNoMessageError("Insurance Value should have no error message", insuranceValue.AmountInfo,
				insuranceValueErrorMessage);
		}

		void AssertHeader(CargoControlAndTransit cct, ForwardingShipment shipment)
		{
			AssertEquals(nameof(cct.AirlinePrefix), shipment.AWBHeader.EH_AirlinePrefix, cct.AirlinePrefix);
			AssertEquals(nameof(cct.SerialNo), shipment.AWBHeader.EH_AWBSerialNo, cct.SerialNo);

			var airlineErrorMessage = "Airline Prefix is required for CCT messaging.";
			var serialNoErrorMessage = "MAWB number is required.";

			AssertHasMessageError("Airline Prefix should have error message", cct.AirlinePrefixInfo, airlineErrorMessage);
			AssertHasMessageError("Serial Number should have error message", cct.SerialNoInfo, serialNoErrorMessage);

			cct.AirlinePrefix = "638";
			cct.SerialNo = "845684215";
			cct.ValidateAllIncludingChildren();

			AssertNoMessageError("Airline Prefix should not have error message", cct.AirlinePrefixInfo, airlineErrorMessage);
			AssertNoMessageError("Serial Number should not have error message", cct.SerialNoInfo, serialNoErrorMessage);
		}

		void AssertAirportInformations(CargoControlAndTransit cct, ForwardingShipment shipment)
		{
			AssertEquals(nameof(cct.AirportOfDeparture), shipment.AWBHeader.EH_AirportOfDepartureAndRequestRouteText, cct.AirportOfDeparture.Description);
			AssertEquals(nameof(cct.To1st), shipment.AWBHeader.EH_To1st, cct.To1st.Code);
			AssertEquals(nameof(cct.AirportOfDestination), shipment.AWBHeader.EH_AirportOfDestinationText, cct.AirportOfDestination.Description);
		}

		void AssertShipper(CargoControlAndTransit cct, ForwardingShipment shipment)
		{
			AssertEquals(nameof(cct.Shipper.CompanyName), shipment.AWBHeader.EH_ShipperName, cct.Shipper.CompanyName);
			AssertEquals(nameof(cct.Shipper.AddressLine1), shipment.AWBHeader.EH_ShipperAddress, cct.Shipper.AddressLine1);
			AssertEquals(nameof(cct.Shipper.AddressLine2), shipment.AWBHeader.EH_ShipperAddress2, cct.Shipper.AddressLine2);
			AssertEquals(nameof(cct.Shipper.City), shipment.AWBHeader.EH_ShipperPlace, cct.Shipper.City);
			AssertEquals(nameof(cct.Shipper.State), shipment.AWBHeader.EH_ShipperState, cct.Shipper.State);
			AssertEquals(nameof(cct.Shipper.Postcode), shipment.AWBHeader.EH_ShipperPostCode, cct.Shipper.Postcode);
			AssertEquals(nameof(cct.Shipper.Contact), shipment.AWBHeader.EH_ShipperContactName, cct.Shipper.Contact);
		}

		void AssertConsignee(CargoControlAndTransit cct, ForwardingShipment shipment)
		{
			AssertEquals(nameof(cct.Consignee.CompanyName), shipment.AWBHeader.EH_ConsigneeName, cct.Consignee.CompanyName);
			AssertEquals(nameof(cct.Consignee.AddressLine1), shipment.AWBHeader.EH_ConsigneeAddress, cct.Consignee.AddressLine1);
			AssertEquals(nameof(cct.Consignee.AddressLine2), shipment.AWBHeader.EH_ConsigneeAddress2, cct.Consignee.AddressLine2);
			AssertEquals(nameof(cct.Consignee.City), shipment.AWBHeader.EH_ConsigneePlace, cct.Consignee.City);
			AssertEquals(nameof(cct.Consignee.State), shipment.AWBHeader.EH_ConsigneeState, cct.Consignee.State);
			AssertEquals(nameof(cct.Consignee.Postcode), shipment.AWBHeader.EH_ConsigneePostCode, cct.Consignee.Postcode);
			AssertEquals(nameof(cct.Consignee.Contact), shipment.AWBHeader.EH_ConsigneeContactName, cct.Consignee.Contact);
		}

		void AssertRateLines(CargoControlAndTransit cct, ForwardingShipment shipment)
		{
			var count = shipment.AWBHeader.AWBRateLines.Count;

			for (var index = 0; index < count; index++)
			{
				var rateLine = cct.RateLines.ElementAt(index);

				if (shipment.AWBHeader.AWBRateLines[index].ER_NoOfPiecesOrRCP.IsEmpty)
				{
					AssertEquals(nameof(rateLine.NoOfPieces), 0, rateLine.NoOfPieces);
				}
				else
				{
					AssertEquals(nameof(rateLine.NoOfPieces),
						shipment.AWBHeader.AWBRateLines[index].ER_NoOfPiecesOrRCP.ToString(), rateLine.NoOfPieces.ToString());
				}

				var chargeableWeightType = Core.Constants.Weight.IsImperial(rateLine.ChargeableWeight.Unit.Code) ? Core.Constants.Weight.Pounds : Core.Constants.Weight.Kilograms;

				AssertEquals(nameof(rateLine.GrossWeight), shipment.AWBHeader.AWBRateLines[index].ER_GrossWeight, rateLine.GrossWeight.Value);
				AssertEquals(nameof(rateLine.RateClass), shipment.AWBHeader.AWBRateLines[index].ER_RateClass, rateLine.RateClass);
				AssertEquals(nameof(rateLine.CommodityItemNumber), shipment.AWBHeader.AWBRateLines[index].ER_CommodityItemNumber, rateLine.CommodityItemNumber);
				AssertEquals(nameof(rateLine.ChargeableWeight), shipment.AWBHeader.AWBRateLines[index].ER_ChargeableWeight, rateLine.ChargeableWeight.Value);
				AssertEquals(nameof(rateLine.RateChargeOrDiscount), shipment.AWBHeader.AWBRateLines[index].ER_RateChargeOrDiscount, rateLine.RateChargeOrDiscount);
				AssertEquals(nameof(rateLine.NatureAndQtyOfGoods), shipment.AWBHeader.AWBRateLines[index].NatureAndQtyOfGoodsText.Text, rateLine.NatureAndQtyOfGoods);
				AssertEquals(nameof(rateLine.Total), shipment.AWBHeader.AWBRateLines[index].ER_Total, rateLine.Total);

				AssertNoMessageErrors("No of Pieces should have no error message", rateLine.NoOfPiecesInfo);
				AssertNoMessageErrors("Gross Weight should have no error message", ((Measurement)rateLine.GrossWeight).ValueInfo);
				AssertNoMessageErrors("Chargeable Weight should have no error message",
					((Measurement)rateLine.ChargeableWeight).ValueInfo);
			}

			var totalNoOfPieces = cct.RateLines.Sum(item => item.NoOfPieces);
			var totalGrossWeight = cct.RateLines.Sum(item => item.GrossWeight.Value);

			cct.ValidateAllIncludingChildren();

			AssertEquals(nameof(cct.TotalNoOfPieces), totalNoOfPieces, cct.TotalNoOfPieces);
			AssertEquals(nameof(cct.TotalGrossWeight), totalGrossWeight, cct.TotalGrossWeight.Value);
		}

		void AssertPrePaidAndCollectValues(CargoControlAndTransit cct, ForwardingShipment shipment)
		{
			AssertEquals(nameof(cct.WeightPrepaidCollect), shipment.AWBHeader.EH_WeightPrepaidCollect, cct.WeightPrepaidCollect.Code);
			AssertEquals(nameof(cct.OtherPrepaidCollect), shipment.AWBHeader.EH_OtherPrepaidCollect, cct.OtherPrepaidCollect.Code);

			AssertEquals(nameof(cct.TotalWeightPPD), shipment.AWBHeader.EH_TotalWeightPPD, cct.TotalWeightPPD);
			AssertEquals(nameof(cct.TotalWeightCOL), shipment.AWBHeader.EH_TotalWeightCOL, cct.TotalWeightCOL);

			AssertEquals(nameof(cct.ValuationPPD), shipment.AWBHeader.EH_ValuationPPD, cct.ValuationPPD);
			AssertEquals(nameof(cct.ValuationCOL), shipment.AWBHeader.EH_ValuationCOL, cct.ValuationCOL);

			AssertEquals(nameof(cct.TaxesPPD), shipment.AWBHeader.EH_TaxesPPD, cct.TaxesPPD);
			AssertEquals(nameof(cct.TaxesCOL), shipment.AWBHeader.EH_TaxesCOL, cct.TaxesCOL);

			AssertEquals(nameof(cct.OtherChargesDueAgentPPD), shipment.AWBHeader.EH_OtherChargesDueAgentPPD, cct.OtherChargesDueAgentPPD);
			AssertEquals(nameof(cct.OtherChargesDueAgentCOL), shipment.AWBHeader.EH_OtherChargesDueAgentCOL, cct.OtherChargesDueAgentCOL);

			AssertEquals(nameof(cct.OtherChargesDueCarrierPPD), shipment.AWBHeader.EH_OtherChargesDueCarrierPPD, cct.OtherChargesDueCarrierPPD);
			AssertEquals(nameof(cct.OtherChargesDueCarrierCOL), shipment.AWBHeader.EH_OtherChargesDueCarrierCOL, cct.OtherChargesDueCarrierCOL);
		}

		void AssertSignature(CargoControlAndTransit cct, ForwardingShipment shipment)
		{
			AssertEquals(nameof(cct.AgentApprovedExporterNumber), shipment.AWBHeader.EH_AgentApprovedExporterNumber, cct.AgentApprovedExporterNumber);
			AssertEquals(nameof(cct.ShippersSignature), shipment.AWBHeader.EH_ShippersSignature, cct.ShippersSignature);
			AssertEquals(nameof(cct.IssueDate), shipment.AWBHeader.EH_AWBIssueDate, cct.IssueDate);
			AssertEquals(nameof(cct.IssuePlace), shipment.AWBHeader.EH_AWBIssuePlace, cct.IssuePlace);
			AssertEquals(nameof(cct.AgentsSignature), shipment.AWBHeader.EH_AWBAgentsSignature, cct.AgentsSignature);
		}

		#endregion Assert

		#region Implementation

		void PopulateConsol(ForwardingConsol consol, ZString mawb)
		{
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BRSAO";
			consol.JK_MasterBillNum = mawb;
			consol.JK_UniqueConsignRef = "C00001004";
		}

		void PopulateShipment(ForwardingShipment shipment, ZString hawb, ZDecimal weight, ZInt packs, ZString goodsDescription, ZString signature, ZString origin, ZString destination, ZDateTime dateIssue, ZString notes)
		{
			var shipper = GetShipper();
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var consignee = GetConsignee();
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			shipment.JS_UniqueConsignRef = "S54625711";

			shipment.JS_HouseBill = hawb;
			shipment.JS_ActualWeight = weight;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_GoodsDescription = goodsDescription;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_OuterPacks = packs;
			shipment.Notes.AddNew(true, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, notes);

			shipment.AWBHeader.EH_AWBOriginCode = "";
			shipment.AWBHeader.EH_AirportOfDepartureAndRequestRouteText = "";
			shipment.AWBHeader.EH_To1st = "";
			shipment.AWBHeader.EH_AirportOfDestinationText = "";
			shipment.AWBHeader.EH_OptionalShippingInformation = "TERMS: FOB";
			shipment.AWBHeader.EH_OptionalShippingInformation2 = "Optonal Info 2";

			shipment.AWBHeader.EH_Currency = "AUD";
			shipment.AWBHeader.EH_ChargesCode = "PP";
			shipment.AWBHeader.EH_WeightPrepaidCollect = "5";
			shipment.AWBHeader.EH_OtherPrepaidCollect = "X";

			shipment.AWBHeader.EH_DeclaredValue = 15;
			shipment.AWBHeader.EH_HouseDeclaredValueCurrency = "USD";
			shipment.AWBHeader.EH_CustomsValue = 16;
			shipment.AWBHeader.EH_InsuranceValue = 12;

			shipment.AWBHeader.EH_AWBAgentsSignature = signature;
			shipment.AWBHeader.EH_AWBIssueDate = dateIssue;
			shipment.AWBHeader.EH_AWBIssuePlace = "";
			shipment.AWBHeader.EH_ShippersSignature = "Signature";
		}

		void PopulatePrepaidAndCollectValues(ForwardingShipment shipment)
		{
			var totalWeightPPDRateLine = (ShipmentExportAWBRateLine)shipment.AWBHeader.AWBRateLines.AddNew();
			totalWeightPPDRateLine.ER_RateClass = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			totalWeightPPDRateLine.ER_Total = 20;

			var totalWeightCOLRateLine = (ShipmentExportAWBRateLine)shipment.AWBHeader.AWBRateLines.AddNew();
			totalWeightCOLRateLine.ER_RateClass = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			totalWeightCOLRateLine.ER_Total = 21;

			shipment.AWBHeader.EH_ValuationPPD = 1;
			shipment.AWBHeader.EH_ValuationCOL = 2;

			shipment.AWBHeader.EH_TaxesPPD = 2;
			shipment.AWBHeader.EH_TaxesCOL = 3;

			AddOtherChargeToAWBHeader(21.3m, Core.Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Agent, true, shipment.AWBHeader);
			AddOtherChargeToAWBHeader(22.4m, Core.Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Agent, false, shipment.AWBHeader);

			AddOtherChargeToAWBHeader(23.5m, Core.Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Carrier, true, shipment.AWBHeader);
			AddOtherChargeToAWBHeader(24.6m, Core.Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Carrier, false, shipment.AWBHeader);
		}

		OrgHeader GetShipper()
		{
			var shipper = Factory.New<OrgHeader>();

			shipper.OH_FullName = "Consignor";
			shipper.OH_RL_NKClosestPort = "AUSYD";
			shipper.MainAddress.Address1 = "Unit52";
			shipper.MainAddress.Address2 = "Dorcus yamadai";
			shipper.MainAddress.City = "Sydney";
			shipper.MainAddress.Postcode = "2017";
			shipper.MainAddress.OA_RN_NKCountryCode = "AU";

			return shipper;
		}

		OrgHeader GetConsignee()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "Consignee";
			consignee.OH_RL_NKClosestPort = "USLAX";
			consignee.MainAddress.Address1 = "801";
			consignee.MainAddress.Address2 = "Prismognathus delislei";
			consignee.MainAddress.City = "Somewhere";
			consignee.MainAddress.Postcode = "10043";
			consignee.MainAddress.OA_RN_NKCountryCode = "US";

			return consignee;
		}

		void PopulateSpacialHandling(CargoControlAndTransit cct)
		{
			CreateSpecialHandlingList(cct);

			cct.SpecialHandling.ElementAt(0).CodeAndDescription.Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.AircraftOnGround;
			cct.SpecialHandling.ElementAt(1).CodeAndDescription.Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.BulkUnitizationProgramShipperConsigneeHandledUnit;
			cct.SpecialHandling.ElementAt(2).CodeAndDescription.Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.DiplomaticMail;
			cct.SpecialHandling.ElementAt(3).CodeAndDescription.Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.FishSeafood;
			cct.SpecialHandling.ElementAt(4).CodeAndDescription.Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.GoodsAttachedToAirWaybill;
			cct.SpecialHandling.ElementAt(5).CodeAndDescription.Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.LicenseRequired;
			cct.SpecialHandling.ElementAt(6).CodeAndDescription.Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.RadioactiveMaterialCategoryIWhite;
			cct.SpecialHandling.ElementAt(7).CodeAndDescription.Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.SaveHumanLife;
			cct.SpecialHandling.ElementAt(8).CodeAndDescription.Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.VeryImportantCargo;
		}

		void CreateSpecialHandlingList(CargoControlAndTransit cct)
		{
			var specialHandlingList = new List<CargoControlAndTransitSpecialHandling>();

			var numberOfSpecialHandlingCodesAcceptedInTemplate = 9;

			for (var line = 0; line < numberOfSpecialHandlingCodesAcceptedInTemplate; line++)
			{
				var specialHandlingItems = new CodeDescription(new AWBSpecialHandlingCodeDescriptionPairList());

				var specialHandling = new CargoControlAndTransitSpecialHandling(line)
				{
					CodeAndDescription = specialHandlingItems
				};

				specialHandlingList.Add(specialHandling);
			}

			cct.SpecialHandling = specialHandlingList;
		}

		void PopulateRateLines(ForwardingShipment shipment)
		{
			AddRateLineToAWBHeader("3", 1, "K", "Q", "N", 256, 1, 5, "VOL 122.000 M3", shipment);
			AddRateLineToAWBHeader("2", 3, "K", "Q", "N", 124, 3, 11, "VOL 109.020 M3", shipment);
			AddRateLineToAWBHeader("5", 4, "K", "Q", "N", 81, 2, 3, "VOL 81.81 M3", shipment);
		}

		void AddRateLineToAWBHeader(ZString noOfPiecesOrRcp, ZDecimal grossWeight, ZString weightInLBsOrKGs, ZString rateClass, ZString commodityItemNumber, ZDecimal chargeableWeight, ZDecimal rateChargeOrDiscount, ZDecimal total, ZString natureAndQtyOfGoodsText, ForwardingShipment shipment)
		{
			var rateLine = shipment.AWBHeader.AWBRateLines.AddNew();
			rateLine.ER_NoOfPiecesOrRCP = noOfPiecesOrRcp;
			rateLine.ER_GrossWeight = grossWeight;
			rateLine.ER_WeightInLBsOrKGs = weightInLBsOrKGs;
			rateLine.ER_RateClass = rateClass;
			rateLine.ER_CommodityItemNumber = commodityItemNumber;
			rateLine.ER_ChargeableWeight = chargeableWeight;
			rateLine.ER_RateChargeOrDiscount = rateChargeOrDiscount;
			rateLine.ER_Total = total;
			rateLine.NatureAndQtyOfGoodsText.Text = natureAndQtyOfGoodsText;
		}

		void AddOtherChargeToAWBHeader(ZDecimal amount, ZString chargeCode, ZString description, EntitlementCodes entitlementCode, ZBool prePaid, ExportAWBHeader awbHeader)
		{
			var otherCharge = awbHeader.AWBOtherCharges.AddNew();
			otherCharge.EO_Amount = amount;
			otherCharge.EO_ChargeCode = chargeCode;
			otherCharge.EO_ChargeDescription = description;
			otherCharge.EO_EntitlementCode = entitlementCode == EntitlementCodes.Agent ? Core.Constants.AWB.EntitlementCode.Agent : Core.Constants.AWB.EntitlementCode.Carrier;
			otherCharge.EO_PPDCLT = prePaid ? "PPD" : "COL";
		}

		void PopulateSpecialHandlingCodes(CargoControlAndTransit cct, bool isWithErrors)
		{
			AddSpecialHandlingCode(cct, 0, "ACT");
			AddSpecialHandlingCode(cct, 1, "SPX");
			AddSpecialHandlingCode(cct, 2, "NCS");
			AddSpecialHandlingCode(cct, 3, "ICE");
			AddSpecialHandlingCode(cct, 4, "LIC");
			AddSpecialHandlingCode(cct, 5, "RMD");
			AddSpecialHandlingCode(cct, 6, "ROP");
			AddSpecialHandlingCode(cct, 7, isWithErrors ? "ACT" : "");
			AddSpecialHandlingCode(cct, 8, isWithErrors ? "SCO" : "");
		}

		void AddSpecialHandlingCode(CargoControlAndTransit cct, int lineIndex, ZString code)
		{
			cct.SpecialHandling.ElementAt(lineIndex).CodeAndDescription.Code = code;
		}

		#endregion
	}
}
