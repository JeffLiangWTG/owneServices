using System;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using ExportAWBHeader = Enterprise.Freight.Forwarding.AWB.Business.ExportAWBHeader;
using ExportAWBSecurityStatusLine = Enterprise.Freight.Forwarding.AWB.Business.ExportAWBSecurityStatusLine;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	sealed class AWBHeaderDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestPopulateBeforeAWBHeaderPopulateDataObject()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JM"))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = "AIR";
				consol.JK_RL_NKLoadPort = "JMKIN";
				consol.JK_RL_NKDischargePort = "SGSIN";

				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_InspectionTypeCode = ScreeningMethods.Codes.ExplosivesTraceDetectionEquipment;

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_InspectionTypeCode = ExemptionCodes.Codes.NuclearMaterial;

				var awbHeader = consol.AWBHeader;

				Factory.Save();

				var writer = new AWBHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(null, awbHeader)));

				var populateTimes = 0;
				EventHandler handler = (sender, args) =>
				{
					populateTimes++;
				};

				Business.AWB.ExportAWBHeader.OnPopulated += handler;

				try
				{
					consol.JK_RL_NKLoadPort = "SGSIN";
					consol.JK_RL_NKDischargePort = "AUBNE";

					shipment1.JS_InspectionTypeCode = ScreeningMethods.Codes.SubjectedToAnyOtherMeans;
					shipment2.JS_InspectionTypeCode = ExemptionCodes.Codes.DiplomaticBagsOrDiplomaticMail;

					Factory.Save();

					AssertEquals("Should not populate", 0, populateTimes);
					AssertEquals("Should not populate and keep previous value.", "KIN", awbHeader.EH_AWBOriginCode);

					AssertEquals("Should not populate and keep previous value.", "ETD", awbHeader.CargoSecurityScreeningMethods.Cast<ExportAWBSecurityStatusLine>().First().EAS_ScreeningMethod);
					AssertEquals("Should not populate and keep previous value.", "NUCL", awbHeader.CargoSecurityExemptionGrounds.Cast<ExportAWBSecurityStatusLine>().First().EAS_ExemptionGround);

					var newAwbHeader = writer.GetDataObject(awbHeader);
					AssertEquals("Should populate again.", 2, populateTimes);
					AssertEquals("awbHeader should not be changed", "KIN", awbHeader.EH_AWBOriginCode);

					AssertEquals("awbHeader should not be changed", "ETD", awbHeader.CargoSecurityScreeningMethods.Cast<ExportAWBSecurityStatusLine>().First().EAS_ScreeningMethod);
					AssertEquals("awbHeader should not be changed", "NUCL", awbHeader.CargoSecurityExemptionGrounds.Cast<ExportAWBSecurityStatusLine>().First().EAS_ExemptionGround);

					AssertEquals("Should populate again.", "AOM", newAwbHeader.CargoSecurityDeclaration.ScreeningMethodCollection.First().Code);
					AssertEquals("Should populate again.", "DIPL", newAwbHeader.CargoSecurityDeclaration.GroundsForExemptionCollection.First().Code);
				}
				finally
				{
					Business.AWB.ExportAWBHeader.OnPopulated -= handler;
				}
			}
		}

		#region TestWriteAWBHeader_RateLineCollection

		public void TestWriteAWBHeader_RateLineCollection()
		{
			var awbHeader = Factory.New<ConsolExportAWBHeader>();

			awbHeader.AWBRateLine2.ER_NoOfPiecesOrRCP = "6";
			awbHeader.AWBRateLine5.ER_GrossWeight = 4.2;
			awbHeader.AWBRateLine12.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ShippersLoadAndCount;
			awbHeader.AWBRateLine12.NatureAndQtyOfGoodsSLAC.Count = 4;

			var writer = new AWBHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(null, awbHeader)));
			var awbHeaderDataObject = writer.GetDataObject(awbHeader);

			AssertEquals(3, awbHeaderDataObject.RateLineCollection.Count);
			AssertEquals(2, awbHeaderDataObject.RateLineCollection[0].LineNumber);
			AssertEquals("6", awbHeaderDataObject.RateLineCollection[0].NoOfPiecesOrRCP);
			AssertEquals(5, awbHeaderDataObject.RateLineCollection[1].LineNumber);
			AssertEquals(new ZDecimal(4.2), awbHeaderDataObject.RateLineCollection[1].GrossWeight);
			AssertEquals(12, awbHeaderDataObject.RateLineCollection[2].LineNumber);
			AssertEquals(4, awbHeaderDataObject.RateLineCollection[2].ShippersLoadAndCount);
		}

		#endregion

		#region TestWriteAWBHeader_BasicFields

		public void TestWriteAWBHeader_BasicFields()
		{
			var knownShipper = GlbBranch.CurrentBranch.OrgProxy.MainAddress.KnownShipperDetails.FirstOrDefault() ?? GlbBranch.CurrentBranch.OrgProxy.MainAddress.KnownShipperDetails.AddNew();
			knownShipper.OV_OH_OrgHeader = GlbBranch.CurrentBranch.OrgProxy.PK;
			knownShipper.OV_EXApprovedOrMajorExporter = "RA";
			knownShipper.OV_EXApprovalNumber = "8888";

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.IsAWBValuesOverriddenProperty = ZBool.True;
			var awbHeader = consol.AWBHeader;

			awbHeader.EH_AWBType = AWBTypeList.Codes.AgentMaster;
			consol.JK_MasterBillNum = "41233334585";
			awbHeader.EH_AWBOriginCode = "DE";

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "Frank";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity = "Dortmund";
			awbHeader.EH_AgentIATACode = "333";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentAccountNumber = "998899";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "78912345666";

			awbHeader.EH_IssuingAgentName = "Jim";
			awbHeader.EH_IssuingAgentAddress1 = "45 Carrot St";
			awbHeader.EH_IssuingAgentAddress2 = "Sydney, NSW";

			var accountingInfo1 = awbHeader.AWBAccountingInformations.AddNew();
			accountingInfo1.EA_Information = "accounting info 1";
			var accountingInfo2 = awbHeader.AWBAccountingInformations.AddNew();
			accountingInfo2.EA_Information = "accounting info 2";

			var specialHandling1 = awbHeader.AWBSpecialHandlingItems.AddNew();
			specialHandling1.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.Corrosive;
			var specialHandling2 = awbHeader.AWBSpecialHandlingItems.AddNew();
			specialHandling2.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.Mail;

			awbHeader.EH_AirportOfDepartureAndRequestRouteText = "Go via Jamaica please";

			awbHeader.EH_To1st = "ABC";
			awbHeader.EH_By1st = "AA";
			awbHeader.EH_To2nd = "PPP";
			awbHeader.EH_By2nd = "PP";
			awbHeader.EH_To3rd = "XYZ";
			awbHeader.EH_By3rd = "ZZ";

			awbHeader.EH_AirportOfDestinationCode = "AKL";
			awbHeader.EH_AirportOfDestinationText = "Auckland";

			awbHeader.EH_Booking1stCarrier = "CX";
			awbHeader.EH_Booking1stFlight = "812";
			awbHeader.EH_Booking1stFlightDate = "06";
			awbHeader.EH_Booking2ndCarrier = "QF";
			awbHeader.EH_Booking2ndFlight = "454";
			awbHeader.EH_Booking2ndFlightDate = "07";

			awbHeader.EH_HandlingInformation = "Just don't break anything";
			awbHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.All;
			awbHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.None;
			awbHeader.EH_SpecialHandlingCode = "NAH";
			awbHeader.EH_ShippingLoadAndCount = 12;

			awbHeader.EH_NetRateCode = "NET";
			awbHeader.EH_ManifestDescriptionOfGoods = "Descriptions";

			awbHeader.EH_OptionalShippingInformation = "Deliver ASAP";
			awbHeader.EH_OptionalShippingInformation2 = "Hurry up";

			awbHeader.EH_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			awbHeader.EH_ChargesCode = ExportAWBHeader.Constants.ChargeCodes.AllChargesPrepaidCash;
			awbHeader.EH_WeightPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			awbHeader.EH_OtherPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			awbHeader.EH_DeclaredValue = 40.50;
			awbHeader.EH_CustomsValue = 10.20;
			awbHeader.EH_InsuranceValue = 4.50;

			var otherCharge1 = awbHeader.AWBOtherCharges.AddNew();
			otherCharge1.EO_ChargeDescription = "charge 1";
			var otherCharge2 = awbHeader.AWBOtherCharges.AddNew();
			otherCharge2.EO_ChargeDescription = "charge 2";

			awbHeader.EH_ValuationPPD = 20.30;
			awbHeader.EH_ValuationCOL = 7.10;
			awbHeader.EH_TaxesPPD = 2.03;
			awbHeader.EH_TaxesCOL = 0.71;

			awbHeader.EH_ExtraShipperInfoLine1 = "I'm a dragon";
			awbHeader.EH_ExtraShipperInfoLine2 = "Watch out";
			awbHeader.EH_ShippersSignature = "X";

			awbHeader.EH_ExtraCarrierInfoLine2 = "Some guy";
			awbHeader.EH_AWBIssueDate = new ZDateTime(2013, 6, 6);
			awbHeader.EH_AWBIssuePlace = "Springfield";
			awbHeader.EH_AWBAgentsSignature = "BOB";

			var awbHeaderDataObjectWriter = new AWBHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(null, awbHeader)));
			var awbHeaderDataObject = awbHeaderDataObjectWriter.GetDataObject(awbHeader);

			AssertEquals(AWBTypeList.Codes.AgentMaster, awbHeaderDataObject.AWBType.Code);
			AssertEquals("AgentMaster", awbHeaderDataObject.AWBType.Description);
			AssertEquals("33334585", awbHeaderDataObject.AWBNumber);
			AssertEquals("412-33334585", awbHeaderDataObject.ForwardingAgentReference);
			AssertEquals("DE", awbHeaderDataObject.OriginCode);

			AssertEquals("Frank", awbHeaderDataObject.AgentName);
			AssertEquals("Dortmund", awbHeaderDataObject.AgentPlace);
			AssertEquals("78-9 1234/5666", awbHeaderDataObject.AgentIATACode);
			AssertEquals("998899", awbHeaderDataObject.AgentAccountNo);

			AssertEquals("Jim", awbHeaderDataObject.IssuedByName);
			AssertEquals("45 Carrot St", awbHeaderDataObject.IssuedByAddress1);
			AssertEquals("Sydney, NSW", awbHeaderDataObject.IssuedByAddress2);

			AssertEquals(2, awbHeaderDataObject.AccountingInfoCollection.Count);
			AssertEquals("accounting info 1", awbHeaderDataObject.AccountingInfoCollection[0].Information);
			AssertEquals("accounting info 2", awbHeaderDataObject.AccountingInfoCollection[1].Information);

			AssertEquals(2, awbHeaderDataObject.SpecialHandlingCollection.Count);
			AssertEquals(AWBSpecialHandlingCodeDescriptionPairList.Codes.Corrosive, awbHeaderDataObject.SpecialHandlingCollection[0].Code);
			AssertEquals("Corrosive", awbHeaderDataObject.SpecialHandlingCollection[0].Description);
			AssertEquals(AWBSpecialHandlingCodeDescriptionPairList.Codes.Mail, awbHeaderDataObject.SpecialHandlingCollection[1].Code);
			AssertEquals("Mail", awbHeaderDataObject.SpecialHandlingCollection[1].Description);

			AssertEquals("Go via Jamaica please", awbHeaderDataObject.AirportOfDepartureAndRequestRouteText);

			AssertEquals("ABC", awbHeaderDataObject.Routing1stTo);
			AssertEquals("AA", awbHeaderDataObject.Routing1stBy);
			AssertEquals("PPP", awbHeaderDataObject.Routing2ndTo);
			AssertEquals("PP", awbHeaderDataObject.Routing2ndBy);
			AssertEquals("XYZ", awbHeaderDataObject.Routing3rdTo);
			AssertEquals("ZZ", awbHeaderDataObject.Routing3rdBy);

			AssertEquals("AKL", awbHeaderDataObject.AirportOfDestinationCode);
			AssertEquals("Auckland", awbHeaderDataObject.AirportOfDestinationText);

			AssertEquals("CX", awbHeaderDataObject.Requested1stCarrier);
			AssertEquals("812", awbHeaderDataObject.Requested1stFlight);
			AssertEquals("06", awbHeaderDataObject.Requested1stFlightDate);
			AssertEquals("QF", awbHeaderDataObject.Requested2ndCarrier);
			AssertEquals("454", awbHeaderDataObject.Requested2ndFlight);
			AssertEquals("07", awbHeaderDataObject.Requested2ndFlightDate);

			AssertEquals("Just don't break anything", awbHeaderDataObject.HandlingInformation);
			AssertEquals(true, awbHeaderDataObject.AsAgreedOn1stAWBSet);
			AssertEquals(false, awbHeaderDataObject.AsAgreedOn2ndAWBSet);
			AssertEquals(Core.Constants.AWB.AsAgreedTypes.Codes.All, awbHeaderDataObject.AsAgreedTypeOn1stAWBSet);
			AssertEquals(Core.Constants.AWB.AsAgreedTypes.Codes.None, awbHeaderDataObject.AsAgreedTypeOn2ndAWBSet);
			AssertEquals("NAH", awbHeaderDataObject.SpecialHandlingCode);
			AssertEquals(12, awbHeaderDataObject.ShippersLoadAndCount);

			AssertEquals("NET", awbHeaderDataObject.NetRateCode);
			AssertEquals("Descriptions", awbHeaderDataObject.ManifestDescriptionOfGoods);

			AssertEquals("Deliver ASAP", awbHeaderDataObject.OptionalShippingInformation1);
			AssertEquals("Hurry up", awbHeaderDataObject.OptionalShippingInformation2);

			AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, awbHeaderDataObject.Currency.Code);
			AssertEquals("United States Dollar", awbHeaderDataObject.Currency.Description);

			AssertEquals(ExportAWBHeader.Constants.ChargeCodes.AllChargesPrepaidCash, awbHeaderDataObject.ChargesPayment.Code);
			AssertEquals("All Charges Prepaid Cash", awbHeaderDataObject.ChargesPayment.Description);
			AssertEquals(ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid, awbHeaderDataObject.WeightChargesPayment.Code);
			AssertEquals("Prepaid", awbHeaderDataObject.WeightChargesPayment.Description);
			AssertEquals(ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect, awbHeaderDataObject.OtherChargesPayment.Code);
			AssertEquals("Collect", awbHeaderDataObject.OtherChargesPayment.Description);
			AssertEquals(new ZDecimal(40.5), awbHeaderDataObject.ValueForCarriage);
			AssertEquals(new ZDecimal(10.2), awbHeaderDataObject.ValueForCustoms);
			AssertEquals(new ZDecimal(4.5), awbHeaderDataObject.AmountOfInsurance);

			AssertEquals(2, awbHeaderDataObject.OtherChargesCollection.Count);
			AssertEquals("charge 1", awbHeaderDataObject.OtherChargesCollection[0].Description);
			AssertEquals("charge 2", awbHeaderDataObject.OtherChargesCollection[1].Description);

			AssertEquals(new ZDecimal(20.3), awbHeaderDataObject.TotalValuationPrepaid);
			AssertEquals(new ZDecimal(7.1), awbHeaderDataObject.TotalValuationCollect);
			AssertEquals(new ZDecimal(2.03), awbHeaderDataObject.TotalTaxesPrepaid);
			AssertEquals(new ZDecimal(0.71), awbHeaderDataObject.TotalTaxesCollect);

			AssertEquals("I'm a dragon", awbHeaderDataObject.ShipperExtraInfoLine1);
			AssertEquals("Watch out", awbHeaderDataObject.ShipperExtraInfoLine2);
			AssertEquals("X", awbHeaderDataObject.ShippersSignature);

			AssertEquals("Some guy", awbHeaderDataObject.AWBIssuerExtraInfo);
			AssertEquals(new ZDateTime(2013, 6, 6), awbHeaderDataObject.AWBIssueDate);
			AssertEquals("Springfield", awbHeaderDataObject.AWBIssuePlace);
			AssertEquals("BOB", awbHeaderDataObject.AWBIssuerSignature);
			AssertEquals("", awbHeaderDataObject.AWBIssuerApprovedExporterNumber);

			var consolDataObjectWriter = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(null, consol)));
			var consolDataObject = consolDataObjectWriter.GetDataObject(consol);
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				ObjectFactory.Get<IXmlWriter>().WriteXML(consolDataObject, stream);
				using (var reader = new StreamReader(stream))
				{
					var xmlAsString = reader.ReadToEnd();
					AssertContains("AgentIATACode should not be truncated in USXML.", "<AgentIATACode>78-9 1234/5666</AgentIATACode>", xmlAsString);
				}
			}
		}

		public void TestAsAgreed()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.IsAWBValuesOverriddenProperty = true;

			var awbHeader = consol.AWBHeader;
			awbHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.All;
			awbHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.All;

			Factory.Save();

			var writer = new AWBHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(null, awbHeader)));
			var awbHeaderDataObject = writer.GetDataObject(awbHeader);

			AssertEquals(true, awbHeaderDataObject.AsAgreedOn1stAWBSet);
			AssertEquals(true, awbHeaderDataObject.AsAgreedOn2ndAWBSet);
			AssertEquals(Core.Constants.AWB.AsAgreedTypes.Codes.All, awbHeaderDataObject.AsAgreedTypeOn1stAWBSet);
			AssertEquals(Core.Constants.AWB.AsAgreedTypes.Codes.All, awbHeaderDataObject.AsAgreedTypeOn2ndAWBSet);

			awbHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.None;
			awbHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.None;

			awbHeaderDataObject = writer.GetDataObject(awbHeader);

			AssertEquals(false, awbHeaderDataObject.AsAgreedOn1stAWBSet);
			AssertEquals(false, awbHeaderDataObject.AsAgreedOn2ndAWBSet);
			AssertEquals(Core.Constants.AWB.AsAgreedTypes.Codes.None, awbHeaderDataObject.AsAgreedTypeOn1stAWBSet);
			AssertEquals(Core.Constants.AWB.AsAgreedTypes.Codes.None, awbHeaderDataObject.AsAgreedTypeOn2ndAWBSet);

			awbHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.Prepaid;
			awbHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.Collect;

			awbHeaderDataObject = writer.GetDataObject(awbHeader);

			AssertEquals(false, awbHeaderDataObject.AsAgreedOn1stAWBSet);
			AssertEquals(false, awbHeaderDataObject.AsAgreedOn2ndAWBSet);
			AssertEquals(Core.Constants.AWB.AsAgreedTypes.Codes.Prepaid, awbHeaderDataObject.AsAgreedTypeOn1stAWBSet);
			AssertEquals(Core.Constants.AWB.AsAgreedTypes.Codes.Collect, awbHeaderDataObject.AsAgreedTypeOn2ndAWBSet);
		}

		#endregion

		#region TestWriteAWBHeader_Parties

		public void TestWriteAWBHeader_Parties()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.IsAWBValuesOverriddenProperty = true;

			var awbHeader = consol.AWBHeader;

			#region Shipper

			awbHeader.EH_ShipperAccount = "1234";
			awbHeader.EH_ShipperName = "SHIPPER BRO";
			awbHeader.EH_ShipperAddress = "99 McDonald Ave";
			awbHeader.EH_ShipperAddress2 = "Mysteryville";
			awbHeader.EH_ShipperPlace = "Sydney";
			awbHeader.EH_ShipperState = "NSW";
			awbHeader.EH_ShipperPostCode = "2000";
			awbHeader.EH_ShipperCountryCode = Core.Constants.CountryCodes.Australia;
			awbHeader.EH_ShipperContactCode = Core.Constants.AWB.ContactCodes.TELEPHONE;
			awbHeader.EH_ShipperContactDetail = "99991234";
			awbHeader.EH_ShipperContactName = "MR. Smith";
			awbHeader.EH_ShipperTraderNoType = "VAT";
			awbHeader.EH_ShipperTraderNo = "CHE.123456";

			awbHeader.EH_IsShipperOverriden = ZBool.True;
			awbHeader.EH_ShipperOverride1 = "LINE 1";
			awbHeader.EH_ShipperOverride2 = "SECOND LINE";
			awbHeader.EH_ShipperOverride3 = "ANOTHER LINE";
			awbHeader.EH_ShipperOverride4 = "LINE D";
			awbHeader.EH_ShipperOverride5 = "LAST LINE";

			Factory.Save();

			var writer = new AWBHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(null, awbHeader)));
			var awbHeaderDataObject = writer.GetDataObject(awbHeader);

			AssertEquals("1234", awbHeaderDataObject.Shipper.AccountCode);
			AssertEquals("SHIPPER BRO", awbHeaderDataObject.Shipper.Name);
			AssertEquals("99 McDonald Ave", awbHeaderDataObject.Shipper.AddressLine1);
			AssertEquals("Mysteryville", awbHeaderDataObject.Shipper.AddressLine2);
			AssertEquals("Sydney", awbHeaderDataObject.Shipper.City);
			AssertEquals("NSW", awbHeaderDataObject.Shipper.State);
			AssertEquals("2000", awbHeaderDataObject.Shipper.PostCode);
			AssertEquals(Core.Constants.CountryCodes.Australia, awbHeaderDataObject.Shipper.Country.Code);
			AssertEquals("Australia", awbHeaderDataObject.Shipper.Country.Name);
			AssertEquals(Core.Constants.AWB.ContactCodes.TELEPHONE, awbHeaderDataObject.Shipper.ContactType.Code);
			AssertEquals("Telephone", awbHeaderDataObject.Shipper.ContactType.Description);
			AssertEquals("99991234", awbHeaderDataObject.Shipper.ContactDetail);
			AssertEquals("MR. Smith", awbHeaderDataObject.Shipper.ContactName);
			AssertEquals("VAT", awbHeaderDataObject.Shipper.CompanyIDCode);
			AssertEquals("CHE.123456", awbHeaderDataObject.Shipper.CompanyID);

			AssertEquals(ZBool.True, awbHeaderDataObject.Shipper.IsAddressOverriddenForPaperWaybill);
			AssertEquals("LINE 1", awbHeaderDataObject.Shipper.PaperOverrideLine1);
			AssertEquals("SECOND LINE", awbHeaderDataObject.Shipper.PaperOverrideLine2);
			AssertEquals("ANOTHER LINE", awbHeaderDataObject.Shipper.PaperOverrideLine3);
			AssertEquals("LINE D", awbHeaderDataObject.Shipper.PaperOverrideLine4);
			AssertEquals("LAST LINE", awbHeaderDataObject.Shipper.PaperOverrideLine5);

			#endregion

			#region Consignee

			awbHeader.EH_ConsigneeAccount = "555";
			awbHeader.EH_ConsigneeName = "CONSIGNEE GUY";
			awbHeader.EH_ConsigneeAddress = "34 Whatever St";
			awbHeader.EH_ConsigneeAddress2 = "Placetown";
			awbHeader.EH_ConsigneePlace = "London";
			awbHeader.EH_ConsigneeState = "XXX";
			awbHeader.EH_ConsigneePostCode = "4646";
			awbHeader.EH_ConsigneeCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			awbHeader.EH_ConsigneeContactCode = Core.Constants.AWB.ContactCodes.FAX;
			awbHeader.EH_ConsigneeContactDetail = "4687954";
			awbHeader.EH_ConsigneeContactName = "Lady Gaga";
			awbHeader.EH_ConsigneeTraderNoType = "EORI";
			awbHeader.EH_ConsigneeTraderNo = "3145";

			awbHeader.EH_IsConsigneeOverriden = ZBool.False;
			awbHeader.EH_ConsigneeOverride1 = "I";
			awbHeader.EH_ConsigneeOverride2 = "AM";
			awbHeader.EH_ConsigneeOverride3 = "OVERRIDING";
			awbHeader.EH_ConsigneeOverride4 = "THESE";
			awbHeader.EH_ConsigneeOverride5 = "LINES";

			Factory.Save();
			awbHeaderDataObject = writer.GetDataObject(awbHeader);

			AssertEquals("555", awbHeaderDataObject.Consignee.AccountCode);
			AssertEquals("CONSIGNEE GUY", awbHeaderDataObject.Consignee.Name);
			AssertEquals("34 Whatever St", awbHeaderDataObject.Consignee.AddressLine1);
			AssertEquals("Placetown", awbHeaderDataObject.Consignee.AddressLine2);
			AssertEquals("London", awbHeaderDataObject.Consignee.City);
			AssertEquals("XXX", awbHeaderDataObject.Consignee.State);
			AssertEquals("4646", awbHeaderDataObject.Consignee.PostCode);
			AssertEquals(Core.Constants.CountryCodes.UnitedKingdom, awbHeaderDataObject.Consignee.Country.Code);
			AssertEquals("United Kingdom", awbHeaderDataObject.Consignee.Country.Name);
			AssertEquals(Core.Constants.AWB.ContactCodes.FAX, awbHeaderDataObject.Consignee.ContactType.Code);
			AssertEquals("Fax", awbHeaderDataObject.Consignee.ContactType.Description);
			AssertEquals("4687954", awbHeaderDataObject.Consignee.ContactDetail);
			AssertEquals("Lady Gaga", awbHeaderDataObject.Consignee.ContactName);
			AssertEquals("EORI", awbHeaderDataObject.Consignee.CompanyIDCode);
			AssertEquals("3145", awbHeaderDataObject.Consignee.CompanyID);

			AssertEquals(ZBool.False, awbHeaderDataObject.Consignee.IsAddressOverriddenForPaperWaybill);
			AssertEquals("I", awbHeaderDataObject.Consignee.PaperOverrideLine1);
			AssertEquals("AM", awbHeaderDataObject.Consignee.PaperOverrideLine2);
			AssertEquals("OVERRIDING", awbHeaderDataObject.Consignee.PaperOverrideLine3);
			AssertEquals("THESE", awbHeaderDataObject.Consignee.PaperOverrideLine4);
			AssertEquals("LINES", awbHeaderDataObject.Consignee.PaperOverrideLine5);

			#endregion

			#region Also Notify

			awbHeader.EH_AlsoNotifyName = "NOTIFY PERSON";
			awbHeader.EH_AlsoNotifyAddress = "160 Meh Rd";
			awbHeader.EH_AlsoNotifyAddress2 = "Cityville";
			awbHeader.EH_AlsoNotifyPlace = "Detroit";
			awbHeader.EH_AlsoNotifyState = "ZZZ";
			awbHeader.EH_AlsoNotifyPostCode = "87988";
			awbHeader.EH_AlsoNotifyCountryCode = Core.Constants.CountryCodes.UnitedStates;
			awbHeader.EH_AlsoNotifyContactCode = Core.Constants.AWB.ContactCodes.TELEX;
			awbHeader.EH_AlsoNotifyContactDetail = "wtfistelex";
			awbHeader.EH_AlsoNotifyContactName = "M Jackson";
			awbHeader.EH_AlsoNotifyTraderNoType = "UST";
			awbHeader.EH_AlsoNotifyTraderNo = "UST12345";

			awbHeader.EH_IsNotifyOverriden = ZBool.True;
			awbHeader.EH_NotifyOverride1 = "YAY";
			awbHeader.EH_NotifyOverride2 = "MORE";
			awbHeader.EH_NotifyOverride3 = "LINES";
			awbHeader.EH_NotifyOverride4 = "TO";
			awbHeader.EH_NotifyOverride5 = "OVERRIDE";

			Factory.Save();
			awbHeaderDataObject = writer.GetDataObject(awbHeader);

			AssertEquals("NOTIFY PERSON", awbHeaderDataObject.AlsoNotify.Name);
			AssertEquals("160 Meh Rd", awbHeaderDataObject.AlsoNotify.AddressLine1);
			AssertEquals("Cityville", awbHeaderDataObject.AlsoNotify.AddressLine2);
			AssertEquals("Detroit", awbHeaderDataObject.AlsoNotify.City);
			AssertEquals("ZZZ", awbHeaderDataObject.AlsoNotify.State);
			AssertEquals("87988", awbHeaderDataObject.AlsoNotify.PostCode);
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, awbHeaderDataObject.AlsoNotify.Country.Code);
			AssertEquals("United States", awbHeaderDataObject.AlsoNotify.Country.Name);
			AssertEquals(Core.Constants.AWB.ContactCodes.TELEX, awbHeaderDataObject.AlsoNotify.ContactType.Code);
			AssertEquals("Telex", awbHeaderDataObject.AlsoNotify.ContactType.Description);
			AssertEquals("wtfistelex", awbHeaderDataObject.AlsoNotify.ContactDetail);
			AssertEquals("M Jackson", awbHeaderDataObject.AlsoNotify.ContactName);
			AssertEquals("UST", awbHeaderDataObject.AlsoNotify.CompanyIDCode);
			AssertEquals("UST12345", awbHeaderDataObject.AlsoNotify.CompanyID);

			AssertEquals(ZBool.True, awbHeaderDataObject.AlsoNotify.IsAddressOverriddenForPaperWaybill);
			AssertEquals("YAY", awbHeaderDataObject.AlsoNotify.PaperOverrideLine1);
			AssertEquals("MORE", awbHeaderDataObject.AlsoNotify.PaperOverrideLine2);
			AssertEquals("LINES", awbHeaderDataObject.AlsoNotify.PaperOverrideLine3);
			AssertEquals("TO", awbHeaderDataObject.AlsoNotify.PaperOverrideLine4);
			AssertEquals("OVERRIDE", awbHeaderDataObject.AlsoNotify.PaperOverrideLine5);

			#endregion
		}

		#endregion

		#region TestWriteAWBHeader_SecurityDeclaration

		public void TestWriteAWBHeader_SecurityDeclaration()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JM"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.IsAWBValuesOverriddenProperty = ZBool.True;
				consol.IsCSDValuesOverriddenProperty = ZBool.True;

				var transport = consol.Transports.AddNew();
				transport.JW_RL_NKLoadPort = "JMKIN";
				transport.JW_RL_NKDiscPort = "USLAX";

				var shipment = consol.Shipments.AddNew();
				shipment.FillWithValidTestData();
				shipment.JS_RL_NKOrigin = "JMKIN";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_InspectionTypeCode = ScreeningMethods.Codes.SubjectedToAnyOtherMeans;

				shipment = consol.Shipments.AddNew();
				shipment.FillWithValidTestData();
				shipment.JS_RL_NKOrigin = "JMALP";
				shipment.JS_RL_NKDestination = "USCHI";
				shipment.JS_InspectionTypeCode = ExemptionCodes.Codes.NuclearMaterial;

				using (FreightDataRegistry.Instance.AWBSecurityStatementTextToUSAAndUSTerritories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test Statement <TSASecurityStatementCountriesText>."))
				using (FreightDataRegistry.Instance.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Constants.CountryGuids.Egypt, Constants.CountryGuids.Somalia }))
				{
					var awbHeader = consol.AWBHeader;

					awbHeader.EH_AgentApprovalCategory = "RA";
					awbHeader.EH_AgentApprovalNumber = "12345";
					awbHeader.EH_AgentApprovalExpiryDate = new ZDateTime(2015, 10, 10);
					awbHeader.EH_RN_NKAgentApprovalCountryCode = "JM";

					awbHeader.EH_SecurityStatusIssuedBy = "Fred Flinstone";
					awbHeader.EH_SecurityStatusIssueDate = new ZDateTime(2015, 1, 1);

					var specialHandling = awbHeader.AWBSpecialHandlingItems.AddNew();
					specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;

					var knownShipper = awbHeader.CargoSecurityExemptionGrounds.AddNew();
					knownShipper.EAS_ApprovalCategory = AviationSecuritySchemeMembership.Codes.AccountConsignor;
					knownShipper.EAS_ApprovalNumber = "67890";

					awbHeader.EH_AdditionalScreeningMethods = "sniffing, poking with a stick";
					awbHeader.EH_AdditionalSecurityInformation = "Test Additional Security Information";
					awbHeader.EH_AdditionalSecurityInformationStatement = "Cargo is not required to be examined to receive clearance.";

					Factory.Save();

					var writer = new AWBHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(null, awbHeader)));
					var awbHeaderDataObject = writer.GetDataObject(awbHeader);

					AssertEquals("AgentApprovalCategory.Code", "RA", awbHeaderDataObject.CargoSecurityDeclaration.AgentApprovalCategory.Code);
					AssertEquals("AgentApprovalCategory.Description", "Regulated Agent", awbHeaderDataObject.CargoSecurityDeclaration.AgentApprovalCategory.Description);
					AssertEquals("AgentApprovalNumber", "12345", awbHeaderDataObject.CargoSecurityDeclaration.AgentApprovalNumber);
					AssertEquals("AgentApprovalExpiryDate", new ZDateTime(2015, 10, 10), awbHeaderDataObject.CargoSecurityDeclaration.AgentApprovalExpiryDate);
					AssertEquals("AgentApprovalCountry.Code", "JM", awbHeaderDataObject.CargoSecurityDeclaration.AgentApprovalCountry.Code);
					AssertEquals("AgentApprovalCountry.Name", "Jamaica", awbHeaderDataObject.CargoSecurityDeclaration.AgentApprovalCountry.Name);

					AssertEquals("AdditionalScreeningMethods", "sniffing, poking with a stick", awbHeaderDataObject.CargoSecurityDeclaration.AdditionalScreeningMethods);
					AssertEquals("AdditionalSecurityInformation", "Test Additional Security Information", awbHeaderDataObject.CargoSecurityDeclaration.AdditionalSecurityInformation);
					AssertEquals("AdditionalSecurityInformationStatement", "Cargo is not required to be examined to receive clearance.", awbHeaderDataObject.CargoSecurityDeclaration.AdditionalSecurityInformationStatement);

					AssertEquals("SecurityStatus.Code", "SCO", awbHeaderDataObject.CargoSecurityDeclaration.SecurityStatus.Code);
					AssertEquals("SecurityStatus.Description", "Cargo Secure for All-Cargo Aircraft only", awbHeaderDataObject.CargoSecurityDeclaration.SecurityStatus.Description);
					AssertEquals("SecurityStatusIssueDate", new ZDateTime(2015, 1, 1), awbHeaderDataObject.CargoSecurityDeclaration.SecurityStatusIssueDate);
					AssertEquals("SecurityStatusIssuedBy", "Fred Flinstone", awbHeaderDataObject.CargoSecurityDeclaration.SecurityStatusIssuedBy);
					AssertEquals("TSASecurityStatement", "Test Statement Egypt, Somalia.", awbHeaderDataObject.CargoSecurityDeclaration.TSASecurityStatement);

					AssertContainsExactElementsInAnyOrder("ReceivedFromShipperCollection",
						new[] { "AC|Account Consignor|67890" },
						awbHeaderDataObject.CargoSecurityDeclaration.ReceivedFromShipperCollection.Select(elem => string.Format("{0}|{1}|{2}", elem.Code, elem.Description, elem.Number)));

					AssertContainsExactElementsInAnyOrder("ScreeningMethodCollection",
						new[] { "AOM|Subjected to any other means" },
						awbHeaderDataObject.CargoSecurityDeclaration.ScreeningMethodCollection.Select(elem => string.Format("{0}|{1}", elem.Code, elem.Description)));

					AssertContainsExactElementsInAnyOrder("GroundsForExemptionCollection",
						new[] { "NUCL|" },
						awbHeaderDataObject.CargoSecurityDeclaration.GroundsForExemptionCollection.Select(elem => string.Format("{0}|{1}", elem.Code, elem.Description)));
				}
			}
		}

		#endregion

		#region TestAWBHeaderShouldNotBeChangedDuringExporting

		public void TestAWBHeaderShouldNotBeChangedDuringExporting_Consol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft;

			var awbHeader = consol.AWBHeader;
			awbHeader.EH_AWBType = AWBTypeList.Codes.AgentMaster;

			var specialHandling1 = awbHeader.AWBSpecialHandlingItems.AddNew();
			specialHandling1.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.Corrosive;
			var specialHandling2 = awbHeader.AWBSpecialHandlingItems.AddNew();
			specialHandling2.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.Mail;

			AssertEquals("Prerequisite", 3, awbHeader.AWBSpecialHandlingItems.Count);
			AssertEquals(AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft, awbHeader.AWBSpecialHandlingItems[0].EP_SpecialHandling);
			AssertEquals(AWBSpecialHandlingCodeDescriptionPairList.Codes.Corrosive, awbHeader.AWBSpecialHandlingItems[1].EP_SpecialHandling);
			AssertEquals(AWBSpecialHandlingCodeDescriptionPairList.Codes.Mail, awbHeader.AWBSpecialHandlingItems[2].EP_SpecialHandling);

			Factory.Save();

			var awbHeaderDataObjectWriter = new AWBHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(null, awbHeader)));
			var awbHeaderDataObject = awbHeaderDataObjectWriter.GetDataObject(awbHeader);

			AssertEquals(1, awbHeaderDataObject.SpecialHandlingCollection.Count);
			AssertEquals(AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft, awbHeaderDataObject.SpecialHandlingCollection[0].Code);

			AssertEquals("awbHeader should be unchanged", 3, awbHeader.AWBSpecialHandlingItems.Count);
			AssertEquals(AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft, awbHeader.AWBSpecialHandlingItems[0].EP_SpecialHandling);
			AssertEquals(AWBSpecialHandlingCodeDescriptionPairList.Codes.Corrosive, awbHeader.AWBSpecialHandlingItems[1].EP_SpecialHandling);
			AssertEquals(AWBSpecialHandlingCodeDescriptionPairList.Codes.Mail, awbHeader.AWBSpecialHandlingItems[2].EP_SpecialHandling);
		}

		public void TestAWBHeaderShouldNotBeChangedDuringExporting_Shipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			var awbHeader = shipment.AWBHeader;
			awbHeader.EH_AWBType = AWBTypeList.Codes.House;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ORG1";

			var controllingCustomerAddress1 = awbHeader.DocAddresses.AddNew(DocAddressType.ControllingCustomer);
			controllingCustomerAddress1.E2_OA_Address = org1.MainAddress.PK;

			Factory.Save();

			var awbHeaderDataObjectWriter = new AWBHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(null, awbHeader)));
			awbHeaderDataObjectWriter.GetDataObject(awbHeader);

			var controllingCustomerAddress2 = awbHeader.DocAddresses.FindByDocAddressType(DocAddressType.ControllingCustomer);
			AssertNotNull("Controlling customer address should not be deleted", controllingCustomerAddress2);
		}

		#endregion

		#region TestWriteAWBHeader_NoNRE_ForTemplateRecord

		public void TestWriteAWBHeader_NoNRE_ForTemplateRecord()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "NZAKL";

			var templateRecord = Factory.New<StmTemplateRecord>();
			var templateRecordProvider1 = shipment1 as ITemplateRecordProvider;
			templateRecordProvider1.TemplateRecord = templateRecord;

			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				templateRecordProvider1.SaveToTemplateRecord();
			}

			Factory.Save();

			var shipment2 = Factory.New<ForwardingShipment>();
			var templateRecordProvider2 = shipment2 as ITemplateRecordProvider;
			templateRecordProvider2.LoadFromTemplateRecord(templateRecord);

			AssertNoExceptionThrown(() => UniversalXmlWriter.GetDataObject(RecipientRoleType.PCA, (BusinessObject)templateRecordProvider2));
		}

		#endregion

		#region TestWriteAWBHeader_ShouldNotDeleteSpecialHandling

		public void TestWriteAWBHeader_ShouldNotDeleteSpecialHandling()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.IsAWBValuesOverriddenProperty = false;
			consol.JK_OverrideSecurityDeclarationDefaults = true;

			var awbHeader = consol.AWBHeader;
			var specialHandling = awbHeader.AWBSpecialHandlingItems.AddNew();
			specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;

			Factory.Save();

			Assert("SpecialHandling should exist in DB", DoesExportAWBSpecialHandlingExistInDB(specialHandling.PK));
			consol.HasChanges = true;
			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(null, consol)));
			writer.GetDataObject(consol);
			Assert("specialHandling.IsDeleted", specialHandling.IsDeleted);
			Assert("specialHandling.HasChanges", !specialHandling.HasChanges);
			Assert("specialHandling.IsSavedByFactory", !specialHandling.IsSavedByFactory);

			Factory.Save();

			Assert("SpecialHandling should not be deleted after writing UXML", DoesExportAWBSpecialHandlingExistInDB(specialHandling.PK));
		}

		bool DoesExportAWBSpecialHandlingExistInDB(ZGuid pk)
		{
			return new BusinessObjectFactory().Load<Business.AWB.ExportAWBSpecialHandling>(pk) != null;
		}

		#endregion
	}
}
