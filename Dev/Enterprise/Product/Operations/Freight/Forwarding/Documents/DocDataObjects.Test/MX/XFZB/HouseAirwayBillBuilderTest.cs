using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.MX;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Documents.MX.Testing
{
	sealed class HouseAirwayBillBuilderTest : TestCaseWithFactory
	{
		public void TestBuild()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);
			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, false);
			PopulatePrepaidAndCollectValues(shipment);
			PopulateRateLines(shipment);
			PopulateShipper(shipment);
			PopulateConsginee(shipment);

			var builder = new HouseAirwayBillBuilder(shipment);
			var hawb = builder.Build();

			AssertNotNull(hawb);

			AssertBusinessHeaderDocument(hawb);
			AssertMasterConsignment(hawb);
			AssertIncludedHouseConsignment(hawb);
			AssertRateLines(hawb, shipment);
			AssertShipper(hawb);
			AssertConsignee(hawb);
			AssertExportAgent(hawb, consol?.SendingForwarderAddress);
		}

		void AssertBusinessHeaderDocument(HouseAirwayBill hawb)
		{
			AssertEquals("AWB Number", "HB111", hawb.AWBNumber);
			AssertNoMessageErrors("AWB Number no message errors", hawb.AWBNumberInfo);
			AssertEquals("Shippers Signature", "ShipSIG", hawb.ShippersSignature);
			AssertNoMessageErrors("Shippers Signature no message errors", hawb.ShippersSignatureInfo);
			AssertEquals("Issue Date", new ZDateTime(2020, 03, 03), hawb.IssueDate);
			AssertNoMessageErrors("Issue Date no message errors", hawb.IssueDateInfo);
			AssertEquals("Agents Signature", "CarAgtSIG", hawb.AgentsSignature);
			AssertNoMessageErrors("Agents Signature no message errors", hawb.AgentsSignatureInfo);
			AssertEquals("Issue Place", "OverTheRainbow", hawb.IssuePlace);
			AssertNoMessageErrors("Issue Place no message errors", hawb.IssuePlaceInfo);
		}

		void AssertMasterConsignment(HouseAirwayBill hawb)
		{
			AssertEquals("Airline Prefix", "PRF", hawb.AirlinePrefix);
			AssertNoMessageErrors("Airline Prefix no message errors", hawb.AirlinePrefixInfo);
			AssertEquals("Serial No", "12345678", hawb.SerialNo);
			AssertNoMessageErrors("Serial No no message errors", hawb.SerialNoInfo);
			AssertEquals("Airport of Origin", "MXCUU", hawb.AirportOfDeparture.Code);
			AssertNoMessageErrors("Airport of Origin no message errors", ((Unloco)hawb.AirportOfDeparture).CodeInfo);
			AssertEquals("Airport of Destination", "USNYC", hawb.AirportOfDestination.Code);
			AssertNoMessageErrors("Airport of Destination no message errors", ((Unloco)hawb.AirportOfDestination).CodeInfo);
		}

		void AssertIncludedHouseConsignment(HouseAirwayBill hawb)
		{
			AssertEquals("Carriage Value", 1.1m, hawb.CarriageValue.Amount);
			AssertEquals("Carriage Value Currency", "EUR", hawb.CarriageValue.Currency.Code);
			AssertEquals("Customs Value", 2.2m, hawb.CustomsValue.Amount);
			AssertEquals("Customs Value Currency", "EUR", hawb.CustomsValue.Currency.Code);
			AssertEquals("Insurance Value", 3.3m, hawb.InsuranceValue.Amount);
			AssertEquals("Insurance Value Currency", "EUR", hawb.InsuranceValue.Currency.Code);

			AssertEquals("Total Weight PPD", 20m, hawb.TotalWeightPPD);
			AssertEquals("Total Weight COL", 20m, hawb.TotalWeightCOL);

			AssertEquals("Currency", "EUR", hawb.Currency.Code);
			AssertEquals("Valuation PPD", 6.6m, hawb.ValuationPPD);
			AssertEquals("Valuation COL", 7.7m, hawb.ValuationCOL);
			AssertEquals("Taxes PPD", 8.8m, hawb.TaxesPPD);
			AssertEquals("Taxes COL", 9.9m, hawb.TaxesCOL);

			AssertEquals("Other Charges Due Agent PPD", 21.3m, hawb.OtherChargesDueAgentPPD);
			AssertEquals("Other Charges Due Agent COL", 22.4m, hawb.OtherChargesDueAgentCOL);
			AssertEquals("Other Charges Due Carrier PPD", 23.5m, hawb.OtherChargesDueCarrierPPD);
			AssertEquals("Other Charges Due Carrier COL", 24.6m, hawb.OtherChargesDueCarrierCOL);

			AssertEquals("Total Gross Weight", 20.5m, hawb.TotalGrossWeight.Value);
			AssertNoErrors("Total Gross Weight no message errors", ((Measurement)hawb.TotalGrossWeight).ValueInfo);
			AssertEquals("Total Gross Weight Unit", "K", hawb.TotalGrossWeight.Unit.Code);
			AssertEquals("Total No of Pieces", 65, hawb.TotalNoOfPieces);
			AssertNoErrors("Total No of Pieces no messages errors", hawb.TotalNoOfPiecesInfo);

			AssertEquals("WeightPrepaidCollect", "PPD", hawb.WeightPrepaidCollect.Code);
			AssertEquals("OtherPrepaidCollect", "COL", hawb.OtherPrepaidCollect.Code);
		}

		void AssertRateLines(HouseAirwayBill hawb, ForwardingShipment shipment)
		{
			var i = 0;
			foreach (ExportAWBRateLine awbRateLine in shipment.AWBHeader.AWBRateLines)
			{
				var rateLine = hawb.RateLines.ElementAt(i);

				AssertEquals("RateLine GrossWeight", awbRateLine.ER_GrossWeight, rateLine.GrossWeight.Value);
				AssertEquals("RateLine GrossWeight Unit", awbRateLine.ER_WeightInLBsOrKGs, rateLine.GrossWeight.Unit.Code);
				AssertEquals("RateLine NumberOfPieces", awbRateLine.ER_NoOfPiecesOrRCP, rateLine.NoOfPieces.ToString());
				AssertEquals("RateLine Item Description", awbRateLine.NatureAndQtyOfGoodsText.Text, rateLine.NatureAndQtyOfGoods);
				AssertNoMessageErrors("ReateLine Item Description no message errors", rateLine.NatureAndQtyOfGoodsInfo);
				AssertEquals("RateLine Rate Class", awbRateLine.ER_RateClass, rateLine.RateClass);
				AssertEquals("RateLine Commodity Item Number", awbRateLine.ER_CommodityItemNumber, rateLine.CommodityItemNumber);
				AssertEquals("RateLine Chargeable Weight", awbRateLine.ER_ChargeableWeight, rateLine.ChargeableWeight.Value);
				AssertEquals("RateLine Chargeable Weight Unit", awbRateLine.ER_WeightInLBsOrKGs, rateLine.ChargeableWeight.Unit.Code);
				AssertEquals("RateLine Rate Charge or Discount Amount", awbRateLine.ER_RateChargeOrDiscount, rateLine.RateChargeOrDiscount);
				AssertEquals("RateLine Total", awbRateLine.ER_ChargeableWeight * awbRateLine.ER_RateChargeOrDiscount, rateLine.Total);

				i++;
			}

			hawb.ValidateAllIncludingChildren();
		}

		void AssertShipper(HouseAirwayBill hawb)
		{
			AssertEquals("Shipper CompanyName", "SHIPPER", hawb.Shipper.CompanyName);
			AssertEquals("Shipper AddressLine1", "Address1", hawb.Shipper.AddressLine1);
			AssertEquals("Shipper AddressLine2", "Address2", hawb.Shipper.AddressLine2);
			AssertEquals("Shipper City", "Chihuahua", hawb.Shipper.City);
			AssertEquals("Shipper State", "State", hawb.Shipper.State);
			AssertEquals("Shipper PostCode", "2000", hawb.Shipper.Postcode);
			AssertEquals("Shipper Country", Constants.CountryCodes.Mexico, hawb.Shipper.Country.Code);
			AssertEquals("Shipper Contact", "Contact", hawb.Shipper.Contact);
			AssertEquals("Shipper Phone", "555666777", hawb.Shipper.Phone);
			AssertEquals("Shipper Fax", "555666777", hawb.Shipper.Fax);
			AssertEquals("Shipper Email", "jef@company.com", hawb.Shipper.Email);
		}

		void AssertConsignee(HouseAirwayBill hawb)
		{
			AssertEquals("Consignee CompanyName", "CONSIGNEE", hawb.Consignee.CompanyName);
			AssertEquals("Consignee AddressLine1", "Address1", hawb.Consignee.AddressLine1);
			AssertEquals("Consignee AddressLine2", "Address2", hawb.Consignee.AddressLine2);
			AssertEquals("Consignee City", "Chihuahua", hawb.Consignee.City);
			AssertEquals("Consignee State", "State", hawb.Consignee.State);
			AssertEquals("Consignee PostCode", "2000", hawb.Consignee.Postcode);
			AssertEquals("Consignee Country", Constants.CountryCodes.Mexico, hawb.Consignee.Country.Code);
			AssertEquals("Consignee Contact", "Contact", hawb.Consignee.Contact);
			AssertEquals("Consignee Phone", "555666777", hawb.Consignee.Phone);
			AssertEquals("Consignee Fax", "555666777", hawb.Consignee.Fax);
			AssertEquals("Consignee Email", "john@company.com", hawb.Consignee.Email);
		}

		void AssertExportAgent(HouseAirwayBill hawb, OrgAddress sendingForwarderAddress)
		{
			AssertionHelper.AssertAddressData(sendingForwarderAddress, hawb.ExportAgent);
		}

		#region ExtraTests

		public void TestSendingPartyCodeOfCurrentBranch()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);
			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, false);

			var testObjectCreator = new TestObjectCreator(Factory);
			var testOrgProxy = testObjectCreator.CreateOrgHeader("MXCOMP", true, true, "MXCUU");
			testOrgProxy.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Mexico;
			var testCompany = testObjectCreator.CreateNewCompany("MX", "MX", orgProxy: testOrgProxy);
			var testBranch = testObjectCreator.CreateBranch("YYY", "YYYBranch", testCompany, testOrgProxy);

			testBranch.OrgProxy.MainAddress.CustomsCodes.DeleteAll();
			testBranch.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			testBranch.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PortSystemNumber, "PSN1", Constants.CountryCodes.Mexico);
			Factory.Save();
			using (testBranch.SetAsTemporaryContext())
			{
				var builder = new HouseAirwayBillBuilder(shipment);
				var hawb = builder.Build();

				AssertEquals("Sending Party Code", "PSN1", hawb.SendingPartyCode.Value);
				AssertNoMessageErrors("Sending Party Code no message errors", hawb.SendingPartyCode.ValueInfo);
				AssertNoMessageErrors("Sending Party Code no message errors", hawb.ErrorPlaceHolderInfo);
			}

			testBranch.OrgProxy.MainAddress.CustomsCodes.DeleteAll();
			testBranch.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			testBranch.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PortSystemNumber, "PSN12345", Constants.CountryCodes.Mexico);
			Factory.Save();
			using (testBranch.SetAsTemporaryContext())
			{
				var builder = new HouseAirwayBillBuilder(shipment);
				var hawb = builder.Build();

				const string expectedError = "Incorrect format - Sender's ID must be maximum 4 characters long.";

				AssertHasMessageError("Sender's ID is too long", hawb.SendingPartyCode.ValueInfo, expectedError);
				AssertHasMessageError("Sender's ID is too long", hawb.ErrorPlaceHolderInfo, expectedError);
			}

			testBranch.OrgProxy.MainAddress.CustomsCodes.DeleteAll();
			testBranch.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			Factory.Save();
			using (testBranch.SetAsTemporaryContext())
			{
				var builder = new HouseAirwayBillBuilder(shipment);
				var hawb = builder.Build();

				const string expectedError = "Sender's ID is mandatory. Please maintain it in branch or company organization. proxy Organization > Config > Registration Numbers/Codes - type PSN.";

				AssertHasMessageError("Sender's ID not filled in", hawb.SendingPartyCode.ValueInfo, expectedError);
				AssertHasMessageError("Sender's ID not filled in", hawb.ErrorPlaceHolderInfo, expectedError);
			}
		}

		public void TestSendingPartyCodeOfCurrentCompany()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);
			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, false);

			var testObjectCreator = new TestObjectCreator(Factory);
			var testOrgProxy = testObjectCreator.CreateOrgHeader("MXCOMP", true, true, "MXCUU");
			testOrgProxy.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Mexico;
			var testCompany = testObjectCreator.CreateNewCompany("MX", "MX", orgProxy: testOrgProxy);
			var testBranch = testObjectCreator.CreateBranch("YYY", "YYYBranch", testCompany);

			testCompany.OrgProxy.MainAddress.CustomsCodes.DeleteAll();
			testCompany.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			testCompany.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PortSystemNumber, "PSN1", Constants.CountryCodes.Mexico);
			Factory.Save();

			using (testBranch.SetAsTemporaryContext())
			{
				var builder = new HouseAirwayBillBuilder(shipment);
				var hawb = builder.Build();

				AssertEquals("Sending Party Code", "PSN1", hawb.SendingPartyCode.Value);
				AssertNoMessageErrors("Sending Party Code no message errors", hawb.SendingPartyCode.ValueInfo);
				AssertNoMessageErrors("Sending Party Code no message errors", hawb.ErrorPlaceHolderInfo);
			}

			testCompany.OrgProxy.MainAddress.CustomsCodes.DeleteAll();
			testCompany.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			testCompany.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PortSystemNumber, "PSN12345", Constants.CountryCodes.Mexico);
			Factory.Save();
			using (testBranch.SetAsTemporaryContext())
			{
				var builder = new HouseAirwayBillBuilder(shipment);
				var hawb = builder.Build();

				const string expectedError = "Incorrect format - Sender's ID must be maximum 4 characters long.";

				AssertHasMessageError("Sender's ID is too long", hawb.SendingPartyCode.ValueInfo, expectedError);
				AssertHasMessageError("Sender's ID is too long", hawb.ErrorPlaceHolderInfo, expectedError);
			}

			testCompany.OrgProxy.MainAddress.CustomsCodes.DeleteAll();
			testCompany.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			Factory.Save();
			using (testBranch.SetAsTemporaryContext())
			{
				var builder = new HouseAirwayBillBuilder(shipment);
				var hawb = builder.Build();

				const string expectedError = "Sender's ID is mandatory. Please maintain it in branch or company organization. proxy Organization > Config > Registration Numbers/Codes - type PSN.";

				AssertHasMessageError("Sender's ID not filled in", hawb.SendingPartyCode.ValueInfo, expectedError);
				AssertHasMessageError("Sender's ID not filled in", hawb.ErrorPlaceHolderInfo, expectedError);
			}
		}

		public void TestExportAgentCode()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);
			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, false);

			consol?.SendingForwarderAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PortSystemNumber, "PSN1", Constants.CountryCodes.Mexico);
			var builder = new HouseAirwayBillBuilder(shipment);
			var hawb = builder.Build();
			AssertEquals("Export Agent Code", "PSN1", hawb.ExportAgentCode.Value);
			AssertNoMessageErrors("Export Agent Code no message errors", hawb.ExportAgentCode.ValueInfo);

			consol?.SendingForwarderAddress.CustomsCodes.DeleteAll();
			consol?.SendingForwarderAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PortSystemNumber, "PSN12345", Constants.CountryCodes.Mexico);
			hawb = builder.Build();
			const string expectedError = "Incorrect format - Export Agent's ID must be maximum 4 characters long.";
			AssertHasMessageError("Export Agent's ID is too long", hawb.ExportAgentCode.ValueInfo, expectedError);

			consol?.SendingForwarderAddress.CustomsCodes.DeleteAll();
			hawb = builder.Build();
			const string expectedError2 = "Export Agent's ID is mandatory. Please maintain it in Sending Freight Forwarder organization. proxy Organization > Config > Registration Numbers/Codes - type PSN.";
			AssertHasMessageError("Export Agent's ID not filled in", hawb.ExportAgentCode.ValueInfo, expectedError2);
		}

		public void TestAWBNumberValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);
			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, false);

			var builder = new HouseAirwayBillBuilder(shipment);
			var hawb = builder.Build();

			AssertNoMessageErrors("AWB Number no message errors", hawb.AWBNumberInfo);

			hawb.AWBNumber = "";
			hawb.ValidateAllIncludingChildren();
			var expectedError = "AWB Number is a mandatory field.";
			AssertHasMessageError("AWB Number not filled in", hawb.AWBNumberInfo, expectedError);

			hawb.AWBNumber = "123456789012345678901234567890";
			hawb.ValidateAllIncludingChildren();
			expectedError = "Incorrect format - AWB Number must be maximum 25 characters long.";
			AssertHasMessageError("AWB Number too long", hawb.AWBNumberInfo, expectedError);
		}

		public void TestShippersSignatureValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);
			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, true);

			var builder = new HouseAirwayBillBuilder(shipment);
			var hawb = builder.Build();

			AssertNoMessageErrors("Shippers Signature no message errors", hawb.ShippersSignatureInfo);

			AssertEquals("Shippers Signature", "ShipSIG0123456789012", hawb.ShippersSignature);
			var expectedWarning = "Incorrect format - Shippers Signature must be maximum 20 characters long.";
			AssertHasWarning("Shippers Signature too long", hawb.ShippersSignatureInfo, expectedWarning);

			hawb.ShippersSignature = "";
			hawb.ValidateAllIncludingChildren();
			var expectedError = "Shippers Signature is a mandatory field.";
			AssertHasMessageError("Shippers Signature not filled in", hawb.ShippersSignatureInfo, expectedError);
		}

		public void TestIssueDateValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);
			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, false);

			var builder = new HouseAirwayBillBuilder(shipment);
			var hawb = builder.Build();

			AssertNoMessageErrors("Issue Date no message errors", hawb.IssueDateInfo);

			hawb.IssueDate = new ZDateTime();
			hawb.ValidateAllIncludingChildren();
			var expectedError = "Issue Date is a mandatory field.";
			AssertHasMessageError("Issue Date not filled in", hawb.IssueDateInfo, expectedError);
		}

		public void TestAgentsSignatureValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);
			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, true);

			var builder = new HouseAirwayBillBuilder(shipment);
			var hawb = builder.Build();

			AssertNoMessageErrors("Agents Signature no message errors", hawb.AgentsSignatureInfo);

			AssertEquals("Agents Signature", "CarAgtSIG01234567890", hawb.AgentsSignature);
			var expectedWarning = "Incorrect format - Agents Signature must be maximum 20 characters long.";
			AssertHasWarning("Agents Signature too long", hawb.AgentsSignatureInfo, expectedWarning);

			hawb.AgentsSignature = "";
			hawb.ValidateAllIncludingChildren();
			var expectedError = "Agents Signature is a mandatory field.";
			AssertHasMessageError("Agents Signature not filled in", hawb.AgentsSignatureInfo, expectedError);
		}

		public void TestIssuePlaceValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);
			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, false);

			var builder = new HouseAirwayBillBuilder(shipment);
			var hawb = builder.Build();

			AssertNoMessageErrors("Issue Place no message errors", hawb.IssuePlaceInfo);

			hawb.IssuePlace = "";
			hawb.ValidateAllIncludingChildren();
			var expectedError = "Issue Place is a mandatory field.";
			AssertHasMessageError("Issue Place not filled in", hawb.IssuePlaceInfo, expectedError);

			hawb.IssuePlace = "12345678901234567890";
			hawb.ValidateAllIncludingChildren();
			expectedError = "Incorrect format - Issue Place must be maximum 17 characters long.";
			AssertHasMessageError("Issue Place too long", hawb.IssuePlaceInfo, expectedError);
		}

		public void TestSendingParty()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var testOrgProxy = testObjectCreator.CreateOrgHeader("MXCOMP", true, true, "MXCUU");
			testOrgProxy.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Mexico;
			var testCompany = testObjectCreator.CreateNewCompany("MX", "MX", orgProxy: testOrgProxy);
			var testBranch = testObjectCreator.CreateBranch("YYY", "YYYBranch", testCompany, testOrgProxy);
			Factory.Save();
			using (testBranch.SetAsTemporaryContext())
			{
				var consol = Factory.New<ForwardingConsol>();
				PopulateConsol(consol);
				var shipment = consol.Shipments.AddNew();
				PopulateShipment(shipment, false);

				var builder = new HouseAirwayBillBuilder(shipment);
				var hawb = builder.Build();

				AssertionHelper.AssertAddressData(testCompany.OrgProxy?.MainAddress, hawb.SendingParty);
			}
		}

		public void TestShipperValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);
			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, false);

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "";
			shipper.MainAddress.Postcode = "";
			shipper.MainAddress.Address1 = "";
			shipper.MainAddress.City = "";
			shipper.MainAddress.OA_RN_NKCountryCode = "";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var builder = new HouseAirwayBillBuilder(shipment);
			var hawb = builder.Build();

			Address address = hawb.Shipper as Address;

			hawb.ValidateAllIncludingChildren();

			AssertEquals("Company Name should have empty value.", ZString.Empty, hawb.Shipper.CompanyName);
			AssertEquals("Postcode should have empty value.", ZString.Empty, hawb.Shipper.Postcode);
			AssertEquals("Street Name should have empty value.", ZString.Empty, hawb.Shipper.AddressLine1);
			AssertEquals("City should have empty value.", ZString.Empty, hawb.Shipper.City);
			AssertEquals("Country should have empty value.", ZString.Empty, hawb.Shipper.Country.Code);

			AssertHasMessageError("Company Name should have error message.", address.CompanyNameInfo, "Shipper party name and address information is required.");
			AssertHasMessageError("Address Formatted should have error message.", address.AddressFormattedInfo, "Shipper Address is required for VUCEM messaging.");

			address.CompanyName = "Company";
			address.Postcode = "00666";
			address.AddressLine1 = "Street";
			address.City = "Somewhere";
			address.Country.Code = "MX";
			address.Contact = "Contact01";
			address.Phone = "Phone01";
			address.Email = "Email01";

			hawb.ValidateAllIncludingChildren();

			AssertNotEquals("Company Name should not have empty value.", ZString.Empty, hawb.Shipper.CompanyName);
			AssertNotEquals("Postcode should not have empty value.", ZString.Empty, hawb.Shipper.Postcode);
			AssertNotEquals("Street Name should not have empty value.", ZString.Empty, hawb.Shipper.AddressLine1);
			AssertNotEquals("City should not have empty value.", ZString.Empty, hawb.Shipper.City);
			AssertNotEquals("Country should not have empty value.", ZString.Empty, hawb.Shipper.Country.Code);

			AssertNoMessageError("Company Name should have no error message.", address.CompanyNameInfo, "Shipper party name and address information is required.");
			AssertNoMessageError("Address Formatted should have no error message.", address.AddressFormattedInfo, "Shipper Address is required for VUCEM messaging.");
		}

		public void TestConsigneeValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);
			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, false);

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "";
			consignee.MainAddress.Postcode = "";
			consignee.MainAddress.Address1 = "";
			consignee.MainAddress.City = "";
			consignee.MainAddress.OA_RN_NKCountryCode = "";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var builder = new HouseAirwayBillBuilder(shipment);
			var hawb = builder.Build();

			Address address = hawb.Consignee as Address;

			hawb.ValidateAllIncludingChildren();

			AssertEquals("Company Name should have empty value.", ZString.Empty, hawb.Consignee.CompanyName);
			AssertEquals("Postcode should have empty value.", ZString.Empty, hawb.Consignee.Postcode);
			AssertEquals("Street Name should have empty value.", ZString.Empty, hawb.Consignee.AddressLine1);
			AssertEquals("City should have empty value.", ZString.Empty, hawb.Consignee.City);
			AssertEquals("Country should have empty value.", ZString.Empty, hawb.Consignee.Country.Code);

			AssertHasMessageError("Company Name should have error message.", address.CompanyNameInfo, "Consignee party name and address information is required.");
			AssertHasMessageError("Address Formatted should have error message.", address.AddressFormattedInfo, "Consignee Address is required for VUCEM messaging.");

			address.CompanyName = "Company";
			address.Postcode = "00666";
			address.AddressLine1 = "Street";
			address.City = "Somewhere";
			address.Country.Code = "MX";
			address.Contact = "Contact01";
			address.Phone = "Phone01";
			address.Email = "Email01";

			hawb.ValidateAllIncludingChildren();

			AssertNotEquals("Company Name should not have empty value.", ZString.Empty, hawb.Consignee.CompanyName);
			AssertNotEquals("Postcode should not have empty value.", ZString.Empty, hawb.Consignee.Postcode);
			AssertNotEquals("Street Name should not have empty value.", ZString.Empty, hawb.Consignee.AddressLine1);
			AssertNotEquals("City should not have empty value.", ZString.Empty, hawb.Consignee.City);
			AssertNotEquals("Country should not have empty value.", ZString.Empty, hawb.Consignee.Country.Code);

			AssertNoMessageError("Company Name should have no error message.", address.CompanyNameInfo, "Consignee party name and address information is required.");
			AssertNoMessageError("Address Formatted should have no error message.", address.AddressFormattedInfo, "Consignee Address is required for VUCEM messaging.");
		}

		public void TestExportAgentValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);
			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, false);

			var exportAgent = Factory.New<OrgHeader>();
			exportAgent.OH_FullName = "";
			exportAgent.MainAddress.Postcode = "";
			exportAgent.MainAddress.Address1 = "";
			exportAgent.MainAddress.City = "";
			exportAgent.MainAddress.OA_RN_NKCountryCode = "";

			consol.JK_OA_SendingForwarderAddress = exportAgent.MainAddress.PK;

			var builder = new HouseAirwayBillBuilder(shipment);
			var hawb = builder.Build();

			Address address = hawb.ExportAgent as Address;

			hawb.ValidateAllIncludingChildren();

			AssertEquals("Company Name should have empty value.", ZString.Empty, hawb.ExportAgent.CompanyName);
			AssertEquals("Postcode should have empty value.", ZString.Empty, hawb.ExportAgent.Postcode);
			AssertEquals("Street Name should have empty value.", ZString.Empty, hawb.ExportAgent.AddressLine1);
			AssertEquals("City should have empty value.", ZString.Empty, hawb.ExportAgent.City);
			AssertEquals("Country should have empty value.", ZString.Empty, hawb.ExportAgent.Country.Code);

			AssertHasMessageError("Company Name should have error message.", address.CompanyNameInfo, "Export Agent party name and address information is required.");
			AssertHasMessageError("Address Formatted should have error message.", address.AddressFormattedInfo, "Export Agent Address is required for VUCEM messaging.");

			address.CompanyName = "Company";
			address.Postcode = "00666";
			address.AddressLine1 = "Street";
			address.City = "Somewhere";
			address.Country.Code = "MX";
			address.Contact = "Contact01";
			address.Phone = "Phone01";
			address.Email = "Email01";

			hawb.ValidateAllIncludingChildren();

			AssertNotEquals("Company Name should not have empty value.", ZString.Empty, hawb.ExportAgent.CompanyName);
			AssertNotEquals("Postcode should not have empty value.", ZString.Empty, hawb.ExportAgent.Postcode);
			AssertNotEquals("Street Name should not have empty value.", ZString.Empty, hawb.ExportAgent.AddressLine1);
			AssertNotEquals("City should not have empty value.", ZString.Empty, hawb.ExportAgent.City);
			AssertNotEquals("Country should not have empty value.", ZString.Empty, hawb.ExportAgent.Country.Code);

			AssertNoMessageError("Company Name should have no error message.", address.CompanyNameInfo, "Export Agent party name and address information is required.");
			AssertNoMessageError("Address Formatted should have no error message.", address.AddressFormattedInfo, "Export Agent Address is required for VUCEM messaging.");
		}

		public void TestTotalPrepaidAndTotalCollect()
		{
			var shipment = Factory.New<ForwardingShipment>();
			PopulateShipment(shipment, false);

			var builder1 = new HouseAirwayBillBuilder(shipment);
			var hawb1 = builder1.Build();
			AssertEquals("Total Prepaid 1", 0M, hawb1.TotalPrepaid);
			AssertEquals("Total Collect 1", 0M, hawb1.TotalCollect);

			shipment.AWBHeader.EH_ValuationPPD = 1;
			shipment.AWBHeader.EH_TaxesPPD = 4;
			AddOtherChargeToAWBHeader(5, Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Agent, true, shipment.AWBHeader);
			AddOtherChargeToAWBHeader(5, Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Carrier, true, shipment.AWBHeader);
			var builder2 = new HouseAirwayBillBuilder(shipment);
			var hawb2 = builder2.Build();
			AssertEquals("Total Prepaid 2", 15M, hawb2.TotalPrepaid);
			AssertEquals("Total Collect 2", 0M, hawb2.TotalCollect);

			shipment.AWBHeader.EH_ValuationCOL = 1;
			shipment.AWBHeader.EH_TaxesCOL = 4;
			AddOtherChargeToAWBHeader(5, Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Agent, false, shipment.AWBHeader);
			AddOtherChargeToAWBHeader(5, Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Carrier, false, shipment.AWBHeader);
			var builder3 = new HouseAirwayBillBuilder(shipment);
			var hawb3 = builder3.Build();
			AssertEquals("Total Prepaid 3", 15M, hawb3.TotalPrepaid);
			AssertEquals("Total Collect 3", 15M, hawb3.TotalCollect);
		}

		public void TestAirlinePrefixSerialNoValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);
			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, false);

			var builder = new HouseAirwayBillBuilder(shipment);
			var hawb = builder.Build();

			AssertNoMessageErrors("Airline Prefix no message errors", hawb.AirlinePrefixInfo);
			AssertNoMessageErrors("Serial No no message errors", hawb.SerialNoInfo);

			hawb.AirlinePrefix = "";
			hawb.SerialNo = "";
			hawb.ValidateAllIncludingChildren();
			var expectedError = "Airline Prefix is a mandatory field.";
			AssertHasMessageError("Airline Prefix not filled in", hawb.AirlinePrefixInfo, expectedError);

			expectedError = "Serial No is a mandatory field.";
			AssertHasMessageError("Serial No not filled in", hawb.SerialNoInfo, expectedError);
		}

		public void TestAirPortsValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);
			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, false);

			var builder = new HouseAirwayBillBuilder(shipment);
			var hawb = builder.Build();

			AssertNoMessageErrors("Airport of Origin no message errors", ((Unloco)hawb.AirportOfDeparture).CodeInfo);
			AssertNoMessageErrors("Airport of Destination no message errors", ((Unloco)hawb.AirportOfDestination).CodeInfo);

			hawb.AirportOfDeparture.Code = "";
			hawb.AirportOfDeparture.Name = "";
			hawb.AirportOfDestination.Code = "";
			hawb.AirportOfDestination.Name = "";
			hawb.ValidateAllIncludingChildren();

			var expectedError = "Airport of Departure is a mandatory field.";
			AssertHasMessageError("Airport of Departure not filled in", ((Unloco)hawb.AirportOfDeparture).CodeInfo, expectedError);
			AssertHasMessageError("Airport of Departure not filled in", ((Unloco)hawb.AirportOfDeparture).NameInfo, expectedError);

			expectedError = "Airport of Destination is a mandatory field.";
			AssertHasMessageError("Airport of Destination not filled in", ((Unloco)hawb.AirportOfDestination).CodeInfo, expectedError);
			AssertHasMessageError("Airport of Destination not filled in", ((Unloco)hawb.AirportOfDestination).NameInfo, expectedError);
		}

		public void TestTotalNoOfFiecesAndGrossWeight()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);
			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, false);
			PopulatePrepaidAndCollectValues(shipment);
			PopulateRateLines(shipment);

			var builder = new HouseAirwayBillBuilder(shipment);
			var hawb = builder.Build();

			AssertNoErrors("Total Gross Weight no message errors", ((Measurement)hawb.TotalGrossWeight).ValueInfo);
			AssertNoErrors("Total No of Pieces no messages errors", hawb.TotalNoOfPiecesInfo);

			hawb.TotalGrossWeight.Value = 0;
			hawb.TotalNoOfPieces = 0;
			hawb.ValidateAllIncludingChildren();

			AssertHasMessageError("Total Gross Weight not filled in", ((Measurement)hawb.TotalGrossWeight).ValueInfo,
				"Total Gross Weight greater than zero is required for VUCEM messaging.");
			AssertHasMessageError("Total No of Pieces not filled in", hawb.TotalNoOfPiecesInfo, "Total No Of Pieces greater than zero is required for VUCEM messaging.");
		}

		public void TestRateLines()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);
			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, false);
			PopulatePrepaidAndCollectValues(shipment);
			PopulateRateLines(shipment);

			var builder = new HouseAirwayBillBuilder(shipment);
			var hawb = builder.Build();

			foreach (HouseAirwayBillRateLine rateLine in hawb.RateLines)
			{
				AssertNoMessageErrors("RateLine Item Description no message errors", rateLine.NatureAndQtyOfGoodsInfo);

				rateLine.NatureAndQtyOfGoods = "This is a text longer than 256 chars..This is a text longer than 256 chars..This is a text longer than 256 chars..This is a text longer than 256 chars..This is a text longer than 256 chars..This is a text longer than 256 chars..This is a text longer than 256 chars..";
				hawb.ValidateAllIncludingChildren();

				var expectedError = "Incorrect format - Rateline item description must be maximum 256 characters long.";
				AssertHasMessageError("RateLine Item Description not filled in", rateLine.NatureAndQtyOfGoodsInfo, expectedError);
			}
		}

		public void TestSpecialHandlingValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);
			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, false);

			var builder = new HouseAirwayBillBuilder(shipment);
			var hawb = builder.Build();

			SetSpecialHandlingNoMessageErrors(hawb);
			hawb.ValidateAllIncludingChildren();

			AssertEquals("Special Handling 0", AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.AircraftOnGround, hawb.SpecialHandling.ElementAt(0).CodeAndDescription.Code);
			AssertEquals("Special Handling 1", AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.BulkUnitizationProgramShipperConsigneeHandledUnit, hawb.SpecialHandling.ElementAt(1).CodeAndDescription.Code);
			AssertEquals("Special Handling 2", AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.DiplomaticMail, hawb.SpecialHandling.ElementAt(2).CodeAndDescription.Code);
			AssertEquals("Special Handling 3", AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.FishSeafood, hawb.SpecialHandling.ElementAt(3).CodeAndDescription.Code);
			AssertEquals("Special Handling 4", AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.GoodsAttachedToAirWaybill, hawb.SpecialHandling.ElementAt(4).CodeAndDescription.Code);
			AssertEquals("Special Handling 5", AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.LicenseRequired, hawb.SpecialHandling.ElementAt(5).CodeAndDescription.Code);
			AssertEquals("Special Handling 6", AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.RadioactiveMaterialCategoryIWhite, hawb.SpecialHandling.ElementAt(6).CodeAndDescription.Code);
			AssertEquals("Special Handling 7", AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.SaveHumanLife, hawb.SpecialHandling.ElementAt(7).CodeAndDescription.Code);
			AssertEquals("Special Handling 8", AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.VeryImportantCargo, hawb.SpecialHandling.ElementAt(8).CodeAndDescription.Code);

			var i = 0;
			foreach (HouseAirwayBillSpecialHandling handling in hawb.SpecialHandling)
			{
				AssertNoMessageErrors("Special Handling no message errors " + i, handling.CodeAndDescription.CodeInfo);
				i++;
			}

			SetSpecialHandlingDuplicateMessageErrors(hawb);
			hawb.ValidateAllIncludingChildren();

			i = 0;
			foreach (HouseAirwayBillSpecialHandling handling in hawb.SpecialHandling)
			{
				if (i == 0 || i == 1 || i == 3 || i == 4 || i == 5 || i == 6)
				{
					var expectedError = "Special Handling Codes cannot contain duplicated items.";
					AssertHasMessageError("Special Handling duplicate " + i, handling.CodeAndDescription.CodeInfo, expectedError);
				}
				else
				{
					AssertNoMessageErrors("Special Handling no message errors duplicates " + i, handling.CodeAndDescription.CodeInfo);
				}

				i++;
			}

			SetSpecialHandlingSecurityMessageErrors(hawb);
			hawb.ValidateAllIncludingChildren();

			i = 0;
			foreach (HouseAirwayBillSpecialHandling handling in hawb.SpecialHandling)
			{
				if (i == 0 || i == 8)
				{
					var expectedError = "Special Handling Codes can contain one Security Status only.";
					AssertHasMessageError("Special Handling security " + i, handling.CodeAndDescription.CodeInfo, expectedError);
				}
				else
				{
					AssertNoMessageErrors("Special Handling no message errors security " + i, handling.CodeAndDescription.CodeInfo);
				}

				i++;
			}
		}

		#endregion

		#region Implementation

		void PopulateShipment(ForwardingShipment shipment, bool testSignatureValidations)
		{
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "MXCUU";
			shipment.JS_RL_NKDestination = "USNYC";
			shipment.JS_UniqueConsignRef = "S20210303";

			shipment.JS_HouseBill = "HB111";
			shipment.JS_ActualWeight = 12.5;
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_GoodsDescription = "GoodsDescr456";
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_OuterPacks = 55;

			if (testSignatureValidations)
			{
				shipment.AWBHeader.EH_ShippersSignature = "ShipSIG01234567890123456789";
			}
			else
			{
				shipment.AWBHeader.EH_ShippersSignature = "ShipSIG";
			}
			shipment.AWBHeader.EH_AWBIssueDate = new ZDateTime(2020, 03, 03);
			if (testSignatureValidations)
			{
				shipment.AWBHeader.EH_AWBAgentsSignature = "CarAgtSIG01234567890123456789";
			}
			else
			{
				shipment.AWBHeader.EH_AWBAgentsSignature = "CarAgtSIG";
			}
			shipment.AWBHeader.EH_AWBIssuePlace = "OverTheRainbow";

			shipment.AWBHeader.EH_DeclaredValue = 1.1;
			shipment.AWBHeader.EH_HouseDeclaredValueCurrency = "EUR";
			shipment.AWBHeader.EH_CustomsValue = 2.2;
			shipment.AWBHeader.EH_HouseCustomsValueCurrency = "EUR";
			shipment.AWBHeader.EH_InsuranceValue = 3.3;
			shipment.AWBHeader.EH_HouseInsuranceValueCurrency = "EUR";

			shipment.AWBHeader.EH_Currency = "EUR";

			shipment.AWBHeader.EH_WeightPrepaidCollect = "P";
			shipment.AWBHeader.EH_OtherPrepaidCollect = "C";
		}

		void PopulateConsol(ForwardingConsol consol)
		{
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "MXCUU";
			consol.JK_RL_NKDischargePort = "USNYC";
			consol.JK_MasterBillNum = "PRF-12345678";
			consol.JK_UniqueConsignRef = "C20210303";

			PopulateForwarders(consol);
		}

		void PopulatePrepaidAndCollectValues(ForwardingShipment shipment)
		{
			shipment.AWBHeader.EH_ValuationPPD = 6.6;
			shipment.AWBHeader.EH_ValuationCOL = 7.7;
			shipment.AWBHeader.EH_TaxesPPD = 8.8;
			shipment.AWBHeader.EH_TaxesCOL = 9.9;

			AddOtherChargeToAWBHeader(21.3, Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Agent, true, shipment.AWBHeader);
			AddOtherChargeToAWBHeader(22.4, Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Agent, false, shipment.AWBHeader);
			AddOtherChargeToAWBHeader(23.5, Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Carrier, true, shipment.AWBHeader);
			AddOtherChargeToAWBHeader(24.6, Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Carrier, false, shipment.AWBHeader);
		}

		void AddOtherChargeToAWBHeader(ZDecimal amount, ZString chargeCode, ZString description, EntitlementCodes entitlementCode, ZBool prePaid, ExportAWBHeader awbHeader)
		{
			var otherCharge = awbHeader.AWBOtherCharges.AddNew();
			otherCharge.EO_Amount = amount;
			otherCharge.EO_ChargeCode = chargeCode;
			otherCharge.EO_ChargeDescription = description;
			otherCharge.EO_EntitlementCode = entitlementCode == EntitlementCodes.Agent ? Constants.AWB.EntitlementCode.Agent : Constants.AWB.EntitlementCode.Carrier;
			otherCharge.EO_PPDCLT = prePaid ? "PPD" : "COL";
		}

		void PopulateRateLines(ForwardingShipment shipment)
		{
			AddRateLineToAWBHeader(1, "K", "3", "VOL 122.000 M3", "Q", "N", 2, 1, shipment);
			AddRateLineToAWBHeader(3, "K", "2", "VOL 109.020 M3", "Q", "N", 3, 2, shipment);
			AddRateLineToAWBHeader(4, "K", "5", "VOL 181.081 M3", "Q", "N", 4, 3, shipment);
		}

		void AddRateLineToAWBHeader(ZDecimal grossWeight, ZString weightInLBsOrKGs, ZString noOfPiecesOrRcp, ZString natureAndQtyOfGoodsText, ZString rateClass, ZString commodityItemNumber, ZDecimal chargeableWeight, ZDecimal rateChargeOrDiscount, ForwardingShipment shipment)
		{
			var rateLine = shipment.AWBHeader.AWBRateLines.AddNew();

			rateLine.ER_GrossWeight = grossWeight;
			rateLine.ER_WeightInLBsOrKGs = weightInLBsOrKGs;
			rateLine.ER_NoOfPiecesOrRCP = noOfPiecesOrRcp;
			rateLine.NatureAndQtyOfGoodsText.Text = natureAndQtyOfGoodsText;
			rateLine.ER_RateClass = rateClass;
			rateLine.ER_CommodityItemNumber = commodityItemNumber;
			rateLine.ER_ChargeableWeight = chargeableWeight;
			rateLine.ER_RateChargeOrDiscount = rateChargeOrDiscount;
			rateLine.ER_Total = chargeableWeight * rateChargeOrDiscount;
		}

		void SetSpecialHandlingNoMessageErrors(HouseAirwayBill hawb)
		{
			hawb.SpecialHandling.ElementAt(0).CodeAndDescription.Code = AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.AircraftOnGround;
			hawb.SpecialHandling.ElementAt(1).CodeAndDescription.Code = AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.BulkUnitizationProgramShipperConsigneeHandledUnit;
			hawb.SpecialHandling.ElementAt(2).CodeAndDescription.Code = AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.DiplomaticMail;
			hawb.SpecialHandling.ElementAt(3).CodeAndDescription.Code = AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.FishSeafood;
			hawb.SpecialHandling.ElementAt(4).CodeAndDescription.Code = AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.GoodsAttachedToAirWaybill;
			hawb.SpecialHandling.ElementAt(5).CodeAndDescription.Code = AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.LicenseRequired;
			hawb.SpecialHandling.ElementAt(6).CodeAndDescription.Code = AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.RadioactiveMaterialCategoryIWhite;
			hawb.SpecialHandling.ElementAt(7).CodeAndDescription.Code = AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.SaveHumanLife;
			hawb.SpecialHandling.ElementAt(8).CodeAndDescription.Code = AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.VeryImportantCargo;
		}

		void SetSpecialHandlingDuplicateMessageErrors(HouseAirwayBill hawb)
		{
			hawb.SpecialHandling.ElementAt(0).CodeAndDescription.Code = AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.AircraftOnGround;
			hawb.SpecialHandling.ElementAt(1).CodeAndDescription.Code = AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.AircraftOnGround;
			hawb.SpecialHandling.ElementAt(2).CodeAndDescription.Code = AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.DiplomaticMail;
			hawb.SpecialHandling.ElementAt(3).CodeAndDescription.Code = AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.FishSeafood;
			hawb.SpecialHandling.ElementAt(4).CodeAndDescription.Code = AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.FishSeafood;
			hawb.SpecialHandling.ElementAt(5).CodeAndDescription.Code = AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.LicenseRequired;
			hawb.SpecialHandling.ElementAt(6).CodeAndDescription.Code = AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.LicenseRequired;
			hawb.SpecialHandling.ElementAt(7).CodeAndDescription.Code = AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.SaveHumanLife;
			hawb.SpecialHandling.ElementAt(8).CodeAndDescription.Code = AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.VeryImportantCargo;
		}

		void SetSpecialHandlingSecurityMessageErrors(HouseAirwayBill hawb)
		{
			hawb.SpecialHandling.ElementAt(0).CodeAndDescription.Code = AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
			hawb.SpecialHandling.ElementAt(1).CodeAndDescription.Code = AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.BulkUnitizationProgramShipperConsigneeHandledUnit;
			hawb.SpecialHandling.ElementAt(2).CodeAndDescription.Code = AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.DiplomaticMail;
			hawb.SpecialHandling.ElementAt(3).CodeAndDescription.Code = AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.FishSeafood;
			hawb.SpecialHandling.ElementAt(4).CodeAndDescription.Code = AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.GoodsAttachedToAirWaybill;
			hawb.SpecialHandling.ElementAt(5).CodeAndDescription.Code = AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.LicenseRequired;
			hawb.SpecialHandling.ElementAt(6).CodeAndDescription.Code = AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.RadioactiveMaterialCategoryIWhite;
			hawb.SpecialHandling.ElementAt(7).CodeAndDescription.Code = AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.SaveHumanLife;
			hawb.SpecialHandling.ElementAt(8).CodeAndDescription.Code = AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
		}

		void PopulateShipper(ForwardingShipment shipment)
		{
			shipment.AWBHeader.EH_ShipperName = "SHIPPER";
			shipment.AWBHeader.EH_ShipperAddress = "Address1";
			shipment.AWBHeader.EH_ShipperAddress2 = "Address2";
			shipment.AWBHeader.EH_ShipperPlace = "Chihuahua";
			shipment.AWBHeader.EH_ShipperState = "State";
			shipment.AWBHeader.EH_ShipperPostCode = "2000";
			shipment.AWBHeader.EH_ShipperCountryCode = Constants.CountryCodes.Mexico;
			shipment.AWBHeader.EH_ShipperContactName = "Contact";
			shipment.ConsignorDocumentaryAddress.E2_Phone = "555666777";
			shipment.ConsignorDocumentaryAddress.E2_Fax = "555666777";
			shipment.ConsignorDocumentaryAddress.E2_Email = "jef@company.com";
		}

		void PopulateConsginee(ForwardingShipment shipment)
		{
			shipment.AWBHeader.EH_ConsigneeName = "CONSIGNEE";
			shipment.AWBHeader.EH_ConsigneeAddress = "Address1";
			shipment.AWBHeader.EH_ConsigneeAddress2 = "Address2";
			shipment.AWBHeader.EH_ConsigneePlace = "Chihuahua";
			shipment.AWBHeader.EH_ConsigneeState = "State";
			shipment.AWBHeader.EH_ConsigneePostCode = "2000";
			shipment.AWBHeader.EH_ConsigneeCountryCode = Constants.CountryCodes.Mexico;
			shipment.AWBHeader.EH_ConsigneeContactName = "Contact";
			shipment.ConsigneeDocumentaryAddress.E2_Phone = "555666777";
			shipment.ConsigneeDocumentaryAddress.E2_Fax = "555666777";
			shipment.ConsigneeDocumentaryAddress.E2_Email = "john@company.com";
		}

		void PopulateForwarders(ForwardingConsol consol)
		{
			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			sendingForwarder.OH_FullName = "Sending Forwarder";
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
		}

		#endregion
	}
}
