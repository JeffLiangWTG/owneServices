using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.AWB;
using ExportAWBHeader = Enterprise.Freight.Forwarding.AWB.Business.ExportAWBHeader;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class AWBHeaderDataObjectReaderTest : OrganizationAddressTestHelper
	{
		[NUnit.Framework.ExpectNoExceptions]
		public void TestNoExceptionWhenMoreThan195SymbolsSetToEH_HandlingInformation()
		{
			#region Setup

			var awbDataObject = new AWBHeader(DefaultDataObjectWriterStrategy.TestInstance);

			awbDataObject.AWBType = new CodeDescriptionPair { Code = AWBTypeList.Codes.AgentMaster };
			awbDataObject.OriginCode = "NZ";

			awbDataObject.IssuedByName = "Greg";
			awbDataObject.IssuedByAddress1 = "37 Something St";
			awbDataObject.IssuedByAddress2 = "Somewhere, NSW";

			awbDataObject.SetAccountingInfoCollection(() => new List<AWBAccountingInfo>());
			awbDataObject.AccountingInfoCollection.Add(new AWBAccountingInfo { Information = "Accounting Info 1" });
			awbDataObject.AccountingInfoCollection.Add(new AWBAccountingInfo { Information = "Accounting Info 2" });

			awbDataObject.SetSpecialHandlingCollection(() => new List<CodeDescriptionPair>());
			awbDataObject.SpecialHandlingCollection.Add(new CodeDescriptionPair { Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CoolGoods });
			awbDataObject.SpecialHandlingCollection.Add(new CodeDescriptionPair { Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.Flowers });

			awbDataObject.AirportOfDepartureAndRequestRouteText = "Sydney Airport";

			awbDataObject.Routing1stTo = "AB";
			awbDataObject.Routing1stBy = "CD";
			awbDataObject.Routing2ndTo = "EF";
			awbDataObject.Routing2ndBy = "GH";
			awbDataObject.Routing3rdTo = "IJ";
			awbDataObject.Routing3rdBy = "KL";

			awbDataObject.AirportOfDestinationCode = "AKL";
			awbDataObject.AirportOfDestinationText = "Auckland";

			awbDataObject.Requested1stCarrier = "XX";
			awbDataObject.Requested1stFlight = "777";
			awbDataObject.Requested1stFlightDate = "02";
			awbDataObject.Requested2ndCarrier = "ZZ";
			awbDataObject.Requested2ndFlight = "888";
			awbDataObject.Requested2ndFlightDate = "03";

			awbDataObject.HandlingInformation = new ZString('*', 200);
			awbDataObject.AsAgreedOn1stAWBSet = ZBool.True;
			awbDataObject.AsAgreedOn2ndAWBSet = ZBool.False;
			awbDataObject.SpecialHandlingCode = "BLA";
			awbDataObject.ShippersLoadAndCount = 14;

			awbDataObject.NetRateCode = "NET";

			awbDataObject.OptionalShippingInformation1 = "Extra pepperoni";
			awbDataObject.OptionalShippingInformation2 = "And no olives";

			awbDataObject.Currency = new Currency { Code = Core.Constants.CurrencyCodes.Australia };

			awbDataObject.ChargesPayment = new CodeDescriptionPair2Char { Code = ExportAWBHeader.Constants.ChargeCodes.AllChargesPrepaidCash };
			awbDataObject.WeightChargesPayment = new CodeDescriptionPair { Code = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid };
			awbDataObject.OtherChargesPayment = new CodeDescriptionPair1Char { Code = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect };
			awbDataObject.ValueForCarriage = 27.5;
			awbDataObject.ValueForCustoms = 30.1;
			awbDataObject.AmountOfInsurance = 39.9;

			awbDataObject.SetRateLineCollection(() => new List<AWBRateLine>());
			awbDataObject.RateLineCollection.Add(new AWBRateLine { LineNumber = 1, NoOfPiecesOrRCP = "5" });
			awbDataObject.RateLineCollection.Add(new AWBRateLine { LineNumber = 2, NoOfPiecesOrRCP = "6" });
			awbDataObject.RateLineCollection.Add(new AWBRateLine { LineNumber = 7, GrossWeight = 22.2 });

			awbDataObject.SetOtherChargesCollection(() => new List<AWBOtherCharges>());
			awbDataObject.OtherChargesCollection.Add(new AWBOtherCharges { Description = "Charge 1" });
			awbDataObject.OtherChargesCollection.Add(new AWBOtherCharges { Description = "Charge 2" });

			awbDataObject.TotalValuationPrepaid = 19.3;
			awbDataObject.TotalValuationCollect = 18.2;
			awbDataObject.TotalTaxesPrepaid = 8.4;
			awbDataObject.TotalTaxesCollect = 8.2;

			awbDataObject.ShipperExtraInfoLine1 = "I want my goods";
			awbDataObject.ShipperExtraInfoLine2 = "Please";
			awbDataObject.ShippersSignature = "XYZ";

			awbDataObject.AWBIssuerExtraInfo = "Extra info";
			awbDataObject.AWBIssueDate = new ZDateTime(2013, 10, 23);
			awbDataObject.AWBIssuePlace = "Seattle";
			awbDataObject.AWBIssuerSignature = "ZYX";

			#endregion

			#region Assertions

			var awbParent = Factory.New<ForwardingConsol>();
			awbParent.JK_TransportMode = Core.Constants.TransportModes.Air;
			var reader = new AWBHeaderDataObjectReader(awbDataObject, Logger, Factory, awbParent);
			reader.ReadIntoBusinessObject();

			AssertEquals(new ZString('*', 195), awbParent.AWBHeader.EH_HandlingInformation);

			#endregion
		}

		public void TestReadBasicFields()
		{
			#region Setup

			var awbDataObject = new AWBHeader(DefaultDataObjectWriterStrategy.TestInstance);

			awbDataObject.AWBType = new CodeDescriptionPair { Code = AWBTypeList.Codes.AgentMaster };
			awbDataObject.OriginCode = "NZ";

			awbDataObject.IssuedByName = "Greg";
			awbDataObject.IssuedByAddress1 = "37 Something St";
			awbDataObject.IssuedByAddress2 = "Somewhere, NSW";

			awbDataObject.SetAccountingInfoCollection(() => new List<AWBAccountingInfo>());
			awbDataObject.AccountingInfoCollection.Add(new AWBAccountingInfo { Information = "Accounting Info 1" });
			awbDataObject.AccountingInfoCollection.Add(new AWBAccountingInfo { Information = "Accounting Info 2" });

			awbDataObject.SetSpecialHandlingCollection(() => new List<CodeDescriptionPair>());
			awbDataObject.SpecialHandlingCollection.Add(new CodeDescriptionPair { Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CoolGoods });
			awbDataObject.SpecialHandlingCollection.Add(new CodeDescriptionPair { Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.Flowers });

			awbDataObject.AirportOfDepartureAndRequestRouteText = "Sydney Airport";

			awbDataObject.Routing1stTo = "AB";
			awbDataObject.Routing1stBy = "CD";
			awbDataObject.Routing2ndTo = "EF";
			awbDataObject.Routing2ndBy = "GH";
			awbDataObject.Routing3rdTo = "IJ";
			awbDataObject.Routing3rdBy = "KL";

			awbDataObject.AirportOfDestinationCode = "AKL";
			awbDataObject.AirportOfDestinationText = "Auckland";

			awbDataObject.Requested1stCarrier = "XX";
			awbDataObject.Requested1stFlight = "777";
			awbDataObject.Requested1stFlightDate = "02";
			awbDataObject.Requested2ndCarrier = "ZZ";
			awbDataObject.Requested2ndFlight = "888";
			awbDataObject.Requested2ndFlightDate = "03";

			awbDataObject.HandlingInformation = "Handle me with care";
			awbDataObject.AsAgreedOn1stAWBSet = ZBool.True;
			awbDataObject.AsAgreedOn2ndAWBSet = ZBool.False;
			awbDataObject.SpecialHandlingCode = "BLA";
			awbDataObject.ShippersLoadAndCount = 14;

			awbDataObject.NetRateCode = "NET";

			awbDataObject.OptionalShippingInformation1 = "Extra pepperoni";
			awbDataObject.OptionalShippingInformation2 = "And no olives";

			awbDataObject.Currency = new Currency { Code = Core.Constants.CurrencyCodes.Australia };

			awbDataObject.ChargesPayment = new CodeDescriptionPair2Char { Code = ExportAWBHeader.Constants.ChargeCodes.AllChargesPrepaidCash };
			awbDataObject.WeightChargesPayment = new CodeDescriptionPair { Code = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid };
			awbDataObject.OtherChargesPayment = new CodeDescriptionPair1Char { Code = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect };
			awbDataObject.ValueForCarriage = 27.5;
			awbDataObject.ValueForCustoms = 30.1;
			awbDataObject.AmountOfInsurance = 39.9;

			awbDataObject.SetRateLineCollection(() => new List<AWBRateLine>());
			awbDataObject.RateLineCollection.Add(new AWBRateLine { LineNumber = 1, NoOfPiecesOrRCP = "5" });
			awbDataObject.RateLineCollection.Add(new AWBRateLine { LineNumber = 2, NoOfPiecesOrRCP = "6" });
			awbDataObject.RateLineCollection.Add(new AWBRateLine { LineNumber = 7, GrossWeight = 22.2 });

			awbDataObject.SetOtherChargesCollection(() => new List<AWBOtherCharges>());
			awbDataObject.OtherChargesCollection.Add(new AWBOtherCharges { Description = "Charge 1" });
			awbDataObject.OtherChargesCollection.Add(new AWBOtherCharges { Description = "Charge 2" });

			awbDataObject.TotalValuationPrepaid = 19.3;
			awbDataObject.TotalValuationCollect = 18.2;
			awbDataObject.TotalTaxesPrepaid = 8.4;
			awbDataObject.TotalTaxesCollect = 8.2;

			awbDataObject.ShipperExtraInfoLine1 = "I want my goods";
			awbDataObject.ShipperExtraInfoLine2 = "Please";
			awbDataObject.ShippersSignature = "XYZ";

			awbDataObject.AWBIssuerExtraInfo = "Extra info";
			awbDataObject.AWBIssueDate = new ZDateTime(2013, 10, 23);
			awbDataObject.AWBIssuePlace = "Seattle";
			awbDataObject.AWBIssuerSignature = "ZYX";

			#endregion

			#region Assertions

			var awbParent = Factory.New<ForwardingConsol>();
			awbParent.JK_TransportMode = Core.Constants.TransportModes.Air;
			var reader = new AWBHeaderDataObjectReader(awbDataObject, Logger, Factory, awbParent);
			reader.ReadIntoBusinessObject();
			var awbHeader = awbParent.AWBHeader;

			AssertEquals(ZBool.True, awbParent.IsAWBValuesOverriddenProperty);
			AssertEquals(AWBTypeList.Codes.AgentMaster, awbHeader.EH_AWBType);
			AssertEquals("NZ", awbHeader.EH_AWBOriginCode);

			AssertEquals("Greg", awbHeader.EH_IssuingAgentName);
			AssertEquals("37 Something St", awbHeader.EH_IssuingAgentAddress1);
			AssertEquals("Somewhere, NSW", awbHeader.EH_IssuingAgentAddress2);

			AssertEquals(2, awbHeader.AWBAccountingInformations.Count);
			AssertEquals("Accounting Info 1", awbHeader.AWBAccountingInformations[0].EA_Information);
			AssertEquals("Accounting Info 2", awbHeader.AWBAccountingInformations[1].EA_Information);

			AssertEquals(2, awbHeader.AWBSpecialHandlingItems.Count);
			AssertEquals(AWBSpecialHandlingCodeDescriptionPairList.Codes.CoolGoods, awbHeader.AWBSpecialHandlingItems[0].EP_SpecialHandling);
			AssertEquals(AWBSpecialHandlingCodeDescriptionPairList.Codes.Flowers, awbHeader.AWBSpecialHandlingItems[1].EP_SpecialHandling);

			AssertEquals("Sydney Airport", awbHeader.EH_AirportOfDepartureAndRequestRouteText);

			AssertEquals("AB", awbHeader.EH_To1st);
			AssertEquals("CD", awbHeader.EH_By1st);
			AssertEquals("EF", awbHeader.EH_To2nd);
			AssertEquals("GH", awbHeader.EH_By2nd);
			AssertEquals("IJ", awbHeader.EH_To3rd);
			AssertEquals("KL", awbHeader.EH_By3rd);

			AssertEquals("AKL", awbHeader.EH_AirportOfDestinationCode);
			AssertEquals("Auckland", awbHeader.EH_AirportOfDestinationText);

			AssertEquals("XX", awbHeader.EH_Booking1stCarrier);
			AssertEquals("777", awbHeader.EH_Booking1stFlight);
			AssertEquals("02", awbHeader.EH_Booking1stFlightDate);
			AssertEquals("ZZ", awbHeader.EH_Booking2ndCarrier);
			AssertEquals("888", awbHeader.EH_Booking2ndFlight);
			AssertEquals("03", awbHeader.EH_Booking2ndFlightDate);

			AssertEquals("Handle me with care", awbHeader.EH_HandlingInformation);
			AssertEquals(Core.Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed1st);
			AssertEquals(Core.Constants.AWB.AsAgreedTypes.Codes.None, awbHeader.EH_AsAgreed2nd);
			AssertEquals("BLA", awbHeader.EH_SpecialHandlingCode);
			AssertEquals(14, awbHeader.EH_ShippingLoadAndCount);

			AssertEquals("NET", awbHeader.EH_NetRateCode);

			AssertEquals("Extra pepperoni", awbHeader.EH_OptionalShippingInformation);
			AssertEquals("And no olives", awbHeader.EH_OptionalShippingInformation2);

			AssertEquals(Core.Constants.CurrencyCodes.Australia, awbHeader.EH_Currency);

			AssertEquals(ExportAWBHeader.Constants.ChargeCodes.AllChargesPrepaidCash, awbHeader.EH_ChargesCode);
			AssertEquals(ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid, awbHeader.EH_WeightPrepaidCollect);
			AssertEquals(ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect, awbHeader.EH_OtherPrepaidCollect);
			AssertEquals(new ZDecimal(27.5), awbHeader.EH_DeclaredValue);
			AssertEquals(new ZDecimal(30.1), awbHeader.EH_CustomsValue);
			AssertEquals(new ZDecimal(39.9), awbHeader.EH_InsuranceValue);

			AssertEquals("5", awbHeader.AWBRateLine1.ER_NoOfPiecesOrRCP);
			AssertEquals("6", awbHeader.AWBRateLine2.ER_NoOfPiecesOrRCP);
			AssertEquals(new ZDecimal(22.2), awbHeader.AWBRateLine7.ER_GrossWeight);

			AssertEquals(2, awbHeader.AWBOtherCharges.Count);
			AssertEquals("Charge 1", awbHeader.AWBOtherCharges[0].EO_ChargeDescription);
			AssertEquals("Charge 2", awbHeader.AWBOtherCharges[1].EO_ChargeDescription);

			AssertEquals(new ZDecimal(19.3), awbHeader.EH_ValuationPPD);
			AssertEquals(new ZDecimal(18.2), awbHeader.EH_ValuationCOL);
			AssertEquals(new ZDecimal(8.4), awbHeader.EH_TaxesPPD);
			AssertEquals(new ZDecimal(8.2), awbHeader.EH_TaxesCOL);

			AssertEquals("I want my goods", awbHeader.EH_ExtraShipperInfoLine1);
			AssertEquals("Please", awbHeader.EH_ExtraShipperInfoLine2);
			AssertEquals("XYZ", awbHeader.EH_ShippersSignature);

			AssertEquals("Extra info", awbHeader.EH_ExtraCarrierInfoLine2);
			AssertEquals(new ZDateTime(2013, 10, 23), awbHeader.EH_AWBIssueDate);
			AssertEquals("Seattle", awbHeader.EH_AWBIssuePlace);
			AssertEquals("ZYX", awbHeader.EH_AWBAgentsSignature);

			#endregion
		}

		public void TestReadParties()
		{
			var awbDataObject = new AWBHeader(DefaultDataObjectWriterStrategy.TestInstance);
			var awbParent = Factory.New<ForwardingConsol>();
			awbParent.JK_TransportMode = Core.Constants.TransportModes.Air;
			var reader = new AWBHeaderDataObjectReader(awbDataObject, Logger, Factory, awbParent);
			var awbHeader = awbParent.AWBHeader;

			#region Shipper

			awbDataObject.Shipper = new AWBParty();

			awbDataObject.Shipper.AccountCode = "900";
			awbDataObject.Shipper.Name = "Glen";
			awbDataObject.Shipper.AddressLine1 = "66 Abc St";
			awbDataObject.Shipper.AddressLine2 = "Sometown";
			awbDataObject.Shipper.City = "Brisbane";
			awbDataObject.Shipper.State = "QLD";
			awbDataObject.Shipper.PostCode = "3123";
			awbDataObject.Shipper.Country = new Country { Code = Core.Constants.CountryCodes.Australia };
			awbDataObject.Shipper.ContactType = new CodeDescriptionPair { Code = Core.Constants.AWB.ContactCodes.TELEPHONE };
			awbDataObject.Shipper.ContactDetail = "8000 1000";

			awbDataObject.Shipper.IsAddressOverriddenForPaperWaybill = ZBool.True;
			awbDataObject.Shipper.PaperOverrideLine1 = "I";
			awbDataObject.Shipper.PaperOverrideLine2 = "Just";
			awbDataObject.Shipper.PaperOverrideLine3 = "Love";
			awbDataObject.Shipper.PaperOverrideLine4 = "Overriding";
			awbDataObject.Shipper.PaperOverrideLine5 = "Lines";

			reader.ReadIntoBusinessObject();

			AssertEquals("900", awbHeader.EH_ShipperAccount);
			AssertEquals("Glen", awbHeader.EH_ShipperName);
			AssertEquals("66 Abc St", awbHeader.EH_ShipperAddress);
			AssertEquals("Sometown", awbHeader.EH_ShipperAddress2);
			AssertEquals("Brisbane", awbHeader.EH_ShipperPlace);
			AssertEquals("QLD", awbHeader.EH_ShipperState);
			AssertEquals("3123", awbHeader.EH_ShipperPostCode);
			AssertEquals(Core.Constants.CountryCodes.Australia, awbHeader.EH_ShipperCountryCode);
			AssertEquals(Core.Constants.AWB.ContactCodes.TELEPHONE, awbHeader.EH_ShipperContactCode);
			AssertEquals("8000 1000", awbHeader.EH_ShipperContactDetail);

			AssertEquals(ZBool.True, awbHeader.EH_IsShipperOverriden);
			AssertEquals("I", awbHeader.EH_ShipperOverride1);
			AssertEquals("Just", awbHeader.EH_ShipperOverride2);
			AssertEquals("Love", awbHeader.EH_ShipperOverride3);
			AssertEquals("Overriding", awbHeader.EH_ShipperOverride4);
			AssertEquals("Lines", awbHeader.EH_ShipperOverride5);

			#endregion

			#region Consignee

			awbDataObject.Consignee = new AWBParty();

			awbDataObject.Consignee.AccountCode = "1200";
			awbDataObject.Consignee.Name = "Joeben";
			awbDataObject.Consignee.AddressLine1 = "77 Xyz Ave";
			awbDataObject.Consignee.AddressLine2 = "Placeville";
			awbDataObject.Consignee.City = "Adelaide";
			awbDataObject.Consignee.State = "SA";
			awbDataObject.Consignee.PostCode = "5123";
			awbDataObject.Consignee.Country = new Country { Code = Core.Constants.CountryCodes.Australia };
			awbDataObject.Consignee.ContactType = new CodeDescriptionPair { Code = Core.Constants.AWB.ContactCodes.FAX };
			awbDataObject.Consignee.ContactDetail = "1122 3344";

			awbDataObject.Consignee.IsAddressOverriddenForPaperWaybill = ZBool.False;
			awbDataObject.Consignee.PaperOverrideLine1 = "Does";
			awbDataObject.Consignee.PaperOverrideLine2 = "Anybody";
			awbDataObject.Consignee.PaperOverrideLine3 = "Even";
			awbDataObject.Consignee.PaperOverrideLine4 = "Read";
			awbDataObject.Consignee.PaperOverrideLine5 = "This";

			reader.ReadIntoBusinessObject();

			AssertEquals("1200", awbHeader.EH_ConsigneeAccount);
			AssertEquals("Joeben", awbHeader.EH_ConsigneeName);
			AssertEquals("77 Xyz Ave", awbHeader.EH_ConsigneeAddress);
			AssertEquals("Placeville", awbHeader.EH_ConsigneeAddress2);
			AssertEquals("Adelaide", awbHeader.EH_ConsigneePlace);
			AssertEquals("SA", awbHeader.EH_ConsigneeState);
			AssertEquals("5123", awbHeader.EH_ConsigneePostCode);
			AssertEquals(Core.Constants.CountryCodes.Australia, awbHeader.EH_ConsigneeCountryCode);
			AssertEquals(Core.Constants.AWB.ContactCodes.FAX, awbHeader.EH_ConsigneeContactCode);
			AssertEquals("1122 3344", awbHeader.EH_ConsigneeContactDetail);

			AssertEquals(ZBool.False, awbHeader.EH_IsConsigneeOverriden);
			AssertEquals("Does", awbHeader.EH_ConsigneeOverride1);
			AssertEquals("Anybody", awbHeader.EH_ConsigneeOverride2);
			AssertEquals("Even", awbHeader.EH_ConsigneeOverride3);
			AssertEquals("Read", awbHeader.EH_ConsigneeOverride4);
			AssertEquals("This", awbHeader.EH_ConsigneeOverride5);

			#endregion

			#region AlsoNotify

			awbDataObject.AlsoNotify = new AWBParty();

			awbDataObject.AlsoNotify.Name = "Zoltan";
			awbDataObject.AlsoNotify.AddressLine1 = "88 Qwerty St";
			awbDataObject.AlsoNotify.AddressLine2 = "Nowhere";
			awbDataObject.AlsoNotify.City = "New York";
			awbDataObject.AlsoNotify.State = "NY";
			awbDataObject.AlsoNotify.PostCode = "22123";
			awbDataObject.AlsoNotify.Country = new Country { Code = Core.Constants.CountryCodes.UnitedStates };
			awbDataObject.AlsoNotify.ContactType = new CodeDescriptionPair { Code = Core.Constants.AWB.ContactCodes.TELEX };
			awbDataObject.AlsoNotify.ContactDetail = "999999";

			awbDataObject.AlsoNotify.IsAddressOverriddenForPaperWaybill = ZBool.True;
			awbDataObject.AlsoNotify.PaperOverrideLine1 = "Almost";
			awbDataObject.AlsoNotify.PaperOverrideLine2 = "Finished";
			awbDataObject.AlsoNotify.PaperOverrideLine3 = "This";
			awbDataObject.AlsoNotify.PaperOverrideLine4 = "Test";
			awbDataObject.AlsoNotify.PaperOverrideLine5 = "Data";

			reader.ReadIntoBusinessObject();

			AssertEquals("Zoltan", awbHeader.EH_AlsoNotifyName);
			AssertEquals("88 Qwerty St", awbHeader.EH_AlsoNotifyAddress);
			AssertEquals("Nowhere", awbHeader.EH_AlsoNotifyAddress2);
			AssertEquals("New York", awbHeader.EH_AlsoNotifyPlace);
			AssertEquals("NY", awbHeader.EH_AlsoNotifyState);
			AssertEquals("22123", awbHeader.EH_AlsoNotifyPostCode);
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, awbHeader.EH_AlsoNotifyCountryCode);
			AssertEquals(Core.Constants.AWB.ContactCodes.TELEX, awbHeader.EH_AlsoNotifyContactCode);
			AssertEquals("999999", awbHeader.EH_AlsoNotifyContactDetail);

			AssertEquals(ZBool.True, awbHeader.EH_IsNotifyOverriden);
			AssertEquals("Almost", awbHeader.EH_NotifyOverride1);
			AssertEquals("Finished", awbHeader.EH_NotifyOverride2);
			AssertEquals("This", awbHeader.EH_NotifyOverride3);
			AssertEquals("Test", awbHeader.EH_NotifyOverride4);
			AssertEquals("Data", awbHeader.EH_NotifyOverride5);

			#endregion
		}

		public void TestAsAgreed()
		{
			var awbDataObject = new AWBHeader(DefaultDataObjectWriterStrategy.TestInstance);
			var awbParent = Factory.New<ForwardingConsol>();
			awbParent.JK_TransportMode = Core.Constants.TransportModes.Air;
			var reader = new AWBHeaderDataObjectReader(awbDataObject, Logger, Factory, awbParent);
			var awbHeader = awbParent.AWBHeader;

			awbDataObject.AsAgreedOn1stAWBSet = true;
			awbDataObject.AsAgreedOn2ndAWBSet = true;

			reader.ReadIntoBusinessObject();

			AssertEquals(Core.Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed1st);
			AssertEquals(Core.Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed2nd);

			awbDataObject.AsAgreedOn1stAWBSet = false;
			awbDataObject.AsAgreedOn2ndAWBSet = false;

			reader.ReadIntoBusinessObject();

			AssertEquals(Core.Constants.AWB.AsAgreedTypes.Codes.None, awbHeader.EH_AsAgreed1st);
			AssertEquals(Core.Constants.AWB.AsAgreedTypes.Codes.None, awbHeader.EH_AsAgreed2nd);

			awbDataObject.AsAgreedTypeOn1stAWBSet = Core.Constants.AWB.AsAgreedTypes.Codes.Prepaid;
			awbDataObject.AsAgreedTypeOn2ndAWBSet = Core.Constants.AWB.AsAgreedTypes.Codes.Collect;

			reader.ReadIntoBusinessObject();

			AssertEquals(Core.Constants.AWB.AsAgreedTypes.Codes.Prepaid, awbHeader.EH_AsAgreed1st);
			AssertEquals(Core.Constants.AWB.AsAgreedTypes.Codes.Collect, awbHeader.EH_AsAgreed2nd);
		}

		public void TestWeightChargesPaymentWithMoreThanOneChar()
		{
			var awbDataObject = new AWBHeader(DefaultDataObjectWriterStrategy.TestInstance);
			awbDataObject.WeightChargesPayment = new CodeDescriptionPair { Code = "CC" };

			var awbParent = Factory.New<ForwardingConsol>();
			awbParent.JK_TransportMode = Core.Constants.TransportModes.Air;
			var reader = new AWBHeaderDataObjectReader(awbDataObject, Logger, Factory, awbParent);

			var errorMessage = "Attempted to import Weight Charges Payment with an invalid value: 'CC'. Maximum of 1 digit is allowed.";
			AssertExceptionThrown<DataObjectReadFailureException>(errorMessage, () => reader.ReadIntoBusinessObject());
		}

		public void TestPopulateBusinessObjectWithSPXInAWBHeader()
		{
			var awbDataObject = new AWBHeader(DefaultDataObjectWriterStrategy.TestInstance);
			awbDataObject.SetSpecialHandlingCollection(() => new List<CodeDescriptionPair>());
			awbDataObject.SpecialHandlingCollection.Add(new CodeDescriptionPair { Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft });

			var awbParent = Factory.New<ForwardingConsol>();
			awbParent.JK_TransportMode = Core.Constants.TransportModes.Air;
			awbParent.JK_RL_NKLoadPort = "AUMEL";
			((ISupportDataImporting)awbParent).IsImportingData = true;

			var reader = new AWBHeaderDataObjectReader(awbDataObject, Logger, Factory, awbParent);
			reader.ReadIntoBusinessObject();

			AssertEquals(1, awbParent.AWBHeader.AWBSpecialHandlingItems.Count);
			AssertEquals(AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft, awbParent.AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandling);
		}
	}
}
