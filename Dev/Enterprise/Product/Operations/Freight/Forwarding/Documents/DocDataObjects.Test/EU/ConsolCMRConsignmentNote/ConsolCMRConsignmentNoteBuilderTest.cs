using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed partial class ConsolCMRConsignmentNoteBuilderTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestSupplierAddress()
		{
			var cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.SupplierAddress, Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison), "SupplierAddress should be empty at this stage.");

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ActualWeight = 10;
			shipment.JS_UnitOfWeight = Weight.Kilograms;

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "MAERSK Name";
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignor.MainAddress.Address1 = "Address 2 Unit 14";
			consignor.MainAddress.Address2 = "ADDRESS 25 Lost Lane";
			consignor.MainAddress.City = "Sydney";
			consignor.MainAddress.Postcode = "2230";
			consignor.MainAddress.State = "NSW";
			consignor.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			consol.JK_AgentType = AgentType.Agent;

			NUnit.Framework.Assert.That(consol.JK_AgentType, Is.EqualTo((ZString)AgentType.Agent), "Agent type should be set to Agent.");

			cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.SupplierAddress, Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison), "SupplierAddress should be empty because the type is not Direct.");

			consol.JK_AgentType = AgentType.Direct;
			NUnit.Framework.Assert.That(consol.JK_AgentType, Is.EqualTo((ZString)AgentType.Direct), "Agent type should be set to Direct.");

			cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.SupplierAddress, Is.EqualTo($"MAERSK NAME{System.Environment.NewLine}ADDRESS 2 UNIT 14{System.Environment.NewLine}ADDRESS 25 LOST LANE{System.Environment.NewLine}SYDNEY - 2230 - AUSTRALIA").Using(CustomComparers.TypeComparison), "SupplierAddress should be filled from the shipment's consignor for a Direct consol when the consol does not have a sending agent.");

			// override shipment's consignor address
			SetOverrideAddress(shipment.ConsignorDocumentaryAddress);

			cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.SupplierAddress, Is.EqualTo($"DHL{System.Environment.NewLine}UNIT 13-1{System.Environment.NewLine}4 LOST LANE-1{System.Environment.NewLine}BRISBANE - 1234 - HONG KONG").Using(CustomComparers.TypeComparison), "SupplierAddress should be filled from the shipment's consignor for a Direct consol when the consol does not have a sending agent and the shipment has an override address.");

			var sendingAgent = Factory.New<OrgHeader>();
			sendingAgent.OH_FullName = "MAERSK Name";
			sendingAgent.OH_RL_NKClosestPort = "AUSYD";
			sendingAgent.MainAddress.Address1 = "Address 1 Unit 13";
			sendingAgent.MainAddress.Address2 = "ADDRESS 24 Lost Lane";
			sendingAgent.MainAddress.City = "Sydney";
			sendingAgent.MainAddress.Postcode = "2229";
			sendingAgent.MainAddress.State = "NSW";
			sendingAgent.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;

			cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.SupplierAddress, Is.EqualTo($"MAERSK NAME{System.Environment.NewLine}ADDRESS 1 UNIT 13{System.Environment.NewLine}ADDRESS 24 LOST LANE{System.Environment.NewLine}SYDNEY - 2229 - AUSTRALIA").Using(CustomComparers.TypeComparison), "SupplierAddress should be filled from the consol's sending agent.");
		}

		[ExpectNoExceptions]
		public void TestImporterAddress()
		{
			var cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.ImporterAddress, Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison), "ImporterAddress should be empty at this stage.");

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ActualWeight = 10;
			shipment.JS_UnitOfWeight = Weight.Kilograms;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "MAERSK Name";
			consignee.OH_RL_NKClosestPort = "AUSYD";
			consignee.MainAddress.Address1 = "Address 2 Unit 14";
			consignee.MainAddress.Address2 = "ADDRESS 25 Lost Lane";
			consignee.MainAddress.City = "Sydney";
			consignee.MainAddress.Postcode = "2230";
			consignee.MainAddress.State = "NSW";
			consignee.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			consol.JK_AgentType = AgentType.Agent;
			NUnit.Framework.Assert.That(consol.JK_AgentType, Is.EqualTo((ZString)AgentType.Agent), "Agent type should be set to Agent.");

			cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.ImporterAddress, Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison), "ImporterAddress should be empty because the type is not Direct.");

			consol.JK_AgentType = AgentType.Direct;
			NUnit.Framework.Assert.That(consol.JK_AgentType, Is.EqualTo((ZString)AgentType.Direct), "Agent type should be set to Direct.");

			cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.ImporterAddress, Is.EqualTo($"MAERSK NAME{System.Environment.NewLine}ADDRESS 2 UNIT 14{System.Environment.NewLine}ADDRESS 25 LOST LANE{System.Environment.NewLine}SYDNEY - 2230 - AUSTRALIA").Using(CustomComparers.TypeComparison), "ImporterAddress should be filled from the shipment's consignee for a Direct consol when the consol does not have a receiving agent.");

			// override shipment's consignee address
			SetOverrideAddress(shipment.ConsigneeDocumentaryAddress);

			cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.ImporterAddress, Is.EqualTo($"DHL{System.Environment.NewLine}UNIT 13-1{System.Environment.NewLine}4 LOST LANE-1{System.Environment.NewLine}BRISBANE - 1234 - HONG KONG").Using(CustomComparers.TypeComparison), "ImporterAddress should be filled from the shipment's consignee for a Direct consol when the consol does not have a receiving agent and the shipment has an override address.");

			var receivingAgent = Factory.New<OrgHeader>();
			receivingAgent.OH_FullName = "MAERSK";
			receivingAgent.OH_RL_NKClosestPort = "AUSYD";
			receivingAgent.MainAddress.Address1 = "Unit 13";
			receivingAgent.MainAddress.Address2 = "4 Lost Lane";
			receivingAgent.MainAddress.City = "Sydney";
			receivingAgent.MainAddress.Postcode = "2229";
			receivingAgent.MainAddress.State = "NSW";
			receivingAgent.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;

			cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.ImporterAddress, Is.EqualTo($"MAERSK{System.Environment.NewLine}UNIT 13{System.Environment.NewLine}4 LOST LANE{System.Environment.NewLine}SYDNEY - 2229 - AUSTRALIA").Using(CustomComparers.TypeComparison), "ImporterAddress should be filled correctly from the consol's receiving agent.");
		}

		[ExpectNoExceptions]
		public void TestInternationalConsignmentNote()
		{
			var cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.InternationalConsignmentNote, Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison), "InternationalConsignmentNote should be empty.");
		}

		[ExpectNoExceptions]
		public void TestPlaceOfDelivery()
		{
			var cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.PlaceOfDelivery, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "PlaceOfDelivery should be empty at this stage.");

			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.DischargePort.RL_PortName = "Sydney";

			AssertNull("Prerequisite", consol.ReceivingForwarderAddress);

			cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.PlaceOfDelivery, Is.EqualTo("SYDNEY AU").Using(CustomComparers.TypeComparison), "PlaceOfDelivery should include the delivery address.");

			var receivingAgent = Factory.New<OrgHeader>();
			receivingAgent.OH_FullName = "MAERSK";
			receivingAgent.OH_RL_NKClosestPort = "AUSYD";
			receivingAgent.MainAddress.Address1 = "Unit 13";
			receivingAgent.MainAddress.Address2 = "4 Lost Lane";
			receivingAgent.MainAddress.City = "Como";
			receivingAgent.MainAddress.Postcode = "2229";
			receivingAgent.MainAddress.State = "NSW";
			receivingAgent.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;

			cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.PlaceOfDelivery, Is.EqualTo("2229 COMO AU").Using(CustomComparers.TypeComparison), "PlaceOfDelivery should be filled with the consignee's address if there is no delivery address.");
		}

		[ExpectNoExceptions]
		public void TestGoodsTakingOverPlaceAndDate()
		{
			var cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.CityCountryDateOfGoodsTakingOver, Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison), "CityCountryDateOfGoodsTakingOver should be empty when no address is provided.");

			var today = ZDate.Today;
			var todayAsString = today.ToString("dd/MM/yyyy");

			consol.Transports.AddNew();
			consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "AUSYD";
			consol.Transports.MostInterestingTransport.LoadPort.RL_PortName = "Sydney";

			AssertNull("Prerequisite", consol.SendingForwarderAddress);

			cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.CityCountryDateOfGoodsTakingOver, Is.EqualTo("SYDNEY AU").Using(CustomComparers.TypeComparison), "CityCountryDateOfGoodsTakingOver should be filled with the load port when there is no sending agent and ETD.");

			consol.Transports.MostInterestingTransport.JW_ETD = today;

			cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.CityCountryDateOfGoodsTakingOver, Is.EqualTo($"SYDNEY AU {todayAsString}").Using(CustomComparers.TypeComparison), "CityCountryDateOfGoodsTakingOver should be filled with the load port and ETD when there is no sending agent.");

			var sendingAgent = Factory.New<OrgHeader>();
			sendingAgent.OH_FullName = "MAERSK";
			sendingAgent.OH_RL_NKClosestPort = "AUSYD";
			sendingAgent.MainAddress.Address1 = "Unit 13";
			sendingAgent.MainAddress.Address2 = "4 Lost Lane";
			sendingAgent.MainAddress.City = "Miranda";
			sendingAgent.MainAddress.Postcode = "2229";
			sendingAgent.MainAddress.State = "NSW";
			sendingAgent.MainAddress.OA_RN_NKCountryCode = "SG";

			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;
			consol.Transports.MostInterestingTransport.JW_ETD = ZDate.Empty;

			cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.CityCountryDateOfGoodsTakingOver, Is.EqualTo("MIRANDA SG").Using(CustomComparers.TypeComparison), "CityCountryDateOfGoodsTakingOver should be filled with the sending agent's address when there is no ETD.");

			consol.Transports.MostInterestingTransport.JW_ETD = today;

			cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.CityCountryDateOfGoodsTakingOver, Is.EqualTo($"MIRANDA SG {todayAsString}").Using(CustomComparers.TypeComparison), "CityCountryDateOfGoodsTakingOver should be filled with the sending agent's address and ETD.");
		}

		[ExpectNoExceptions]
		public void TestGoodsAttachedDocuments()
		{
			var cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.GoodsAttachedDocuments, Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison), "GoodsAttachedDocuments should be empty.");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsBox6_7_8_9WithShipments()
		{
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_MarksAndNumbersShort = "MNM1";
			shipment1.JS_OuterPacks = 2;
			shipment1.JS_F3_NKPackType = PkgUnit.Piece;
			shipment1.JS_GoodsDescription = "Goods description 1";

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_MarksAndNumbersShort = "MNM2";
			shipment2.JS_OuterPacks = 3;
			shipment2.JS_F3_NKPackType = PkgUnit.Package;
			shipment2.JS_GoodsDescription = "Goods description 2";

			consol.Containers.RemoveAll();

			AssertEquals("Prerequisite", 0, consol.Containers.Count);

			var cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.LineDetailsBox6_7_8_9, Is.EqualTo($"MNM1; 2 PCE; Goods description 1{System.Environment.NewLine}MNM2; 3 PKG; Goods description 2").Using(CustomComparers.TypeComparison), "LineDetailsBox6_7_8_9 should be filled with the shipments attached to the consol when there is no container.");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsBox6_7_8_9WithSelectedContainer()
		{
			var containerWithShipments = AddAllocatedPackLinesToConsol();

			var parameter = new DocDataObjectParameters("Test", "Test", containerWithShipments.container);
			var cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol, parameter);
			var expectedLineDescription = $"MN2; 12 SKD; Description2{System.Environment.NewLine}MN1; 10 PKG; THIS IS THE DESCRIPTION FOR ENTRY LINE 1 WHICH WILL BE CUT HERE ---";

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.LineDetailsBox6_7_8_9, Is.EqualTo(expectedLineDescription).Using(CustomComparers.TypeComparison), "LineDetailsBox6_7_8_9 should provide details from the pack lines of the selected container.");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsTariffCodeBox10()
		{
			var cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.LineDetailsTariffCodeBox10, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "LineDetailsTariffCodeBox10 should be empty.");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsGrossWeightInKGBox11WithShipments()
		{
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 10;
			shipment1.JS_UnitOfWeight = Weight.Kilograms;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ActualWeight = 100;
			shipment2.JS_UnitOfWeight = Weight.Grams;

			consol.Containers.RemoveAll();

			AssertEquals("Prerequisite", 0, consol.Containers.Count);

			var cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			var expectedLineDescription = $"10.00    {System.Environment.NewLine}0.1     ";
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.LineDetailsGrossWeightInKGBox11, Is.EqualTo(expectedLineDescription).Using(CustomComparers.TypeComparison), "LineDetailsGrossWeightInKGBox11 should provide details from the shipments attached to the consol in kilograms (KG).");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsGrossWeightInKGBox11WithSelectedContainer()
		{
			var containerWithShipments = AddAllocatedPackLinesToConsol();

			var parameter = new DocDataObjectParameters("Test", "Test", containerWithShipments.container);
			var cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol, parameter);
			var expectedLineDescription = $"0.1     {System.Environment.NewLine}10.00    ";

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.LineDetailsGrossWeightInKGBox11, Is.EqualTo(expectedLineDescription).Using(CustomComparers.TypeComparison), "LineDetailsGrossWeightInKGBox11 should provide details from the allocated pack lines in kilograms (KG).");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsVolumeInM3Box12WithShipments()
		{
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualVolume = 2;
			shipment1.JS_UnitOfVolume = Volume.CubicMetres;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ActualVolume = 100;
			shipment2.JS_UnitOfVolume = Volume.Litre;

			consol.Containers.RemoveAll();

			AssertEquals("Prerequisite", 0, consol.Containers.Count);

			var cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			var expectedLineDescription = $"2.00    {System.Environment.NewLine}0.1     ";

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.LineDetailsVolumeInM3Box12, Is.EqualTo(expectedLineDescription).Using(CustomComparers.TypeComparison), "LineDetailsVolumeInM3Box12 should provide details from the shipments attached to the consol in cubic meters (M3).");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsVolumeInM3Box12WithSelectedContainer()
		{
			var containerWithShipments = AddAllocatedPackLinesToConsol();

			var parameter = new DocDataObjectParameters("Test", "Test", containerWithShipments.container);
			var cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol, parameter);

			var expectedLineDescription = $"0.1     {System.Environment.NewLine}2.00    ";

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.LineDetailsVolumeInM3Box12, Is.EqualTo(expectedLineDescription).Using(CustomComparers.TypeComparison), "LineDetailsVolumeInM3Box12 should provide details from the pack lines in cubic meters (M3).");
		}

		[ExpectNoExceptions]
		public void TestIncotermAndTextBox14()
		{
			consol.Containers.RemoveAll();
			consol.Shipments.RemoveAll();

			AssertEquals("Prerequisite", 0, consol.Containers.Count);
			AssertEquals("Prerequisite", 0, consol.Shipments.Count);

			var cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.IncotermAndTextBox14, Is.EqualTo(ZString.Empty), "When neither a container nor a shipment is provided, an empty string is expected.");

			var shipment1 = consol.Shipments.AddNew();

			cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.IncotermAndTextBox14, Is.EqualTo(ZString.Empty), "When the shipment does not have an Incoterm, an empty string is expected.");

			shipment1.JS_INCO = IncoTerms.CostAndFreight;

			AssertEquals("Prerequisite", 1, consol.Shipments.Count);
			AssertEquals("Prerequisite", 0, consol.Containers.Count);
			cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.IncotermAndTextBox14, Is.EqualTo("CFR - Cost And Freight").Using(CustomComparers.TypeComparison), "When an Incoterm is provided for the shipment and there is no container, it should be shown in the code-description format.");

			consol.Shipments.RemoveAll();
			var containerWithShipments = AddAllocatedPackLinesToConsol();
			containerWithShipments.shipment1.JS_INCO = ZString.Empty;

			AssertEquals("Prerequisite", 1, consol.Containers.Count);
			var parameter = new DocDataObjectParameters("Test", "Test", containerWithShipments.container);

			cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol, parameter);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.IncotermAndTextBox14, Is.EqualTo(ZString.Empty), "When the container does not have an Incoterm, an empty string is expected.");

			var anotherShipment = consol.Shipments.AddNew();
			anotherShipment.JS_INCO = IncoTerms.CostAndFreight;

			cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol, parameter);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.IncotermAndTextBox14, Is.EqualTo(ZString.Empty), "When the container does not have an Incoterm, an empty string is expected even if another shipment has an Incoterm.");

			containerWithShipments.shipment1.JS_INCO = IncoTerms.FreeOnBoard;

			cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol, parameter);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.IncotermAndTextBox14, Is.EqualTo("FOB - Free On Board").Using(CustomComparers.TypeComparison), "When an Incoterm is provided for container, it should be available in the code-description.");
		}

		[ExpectNoExceptions]
		public void TestSendersInstructions()
		{
			var cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.SendersInstructions, Is.EqualTo(ZString.Empty), "When no HandlingInstructions or DangerousGoodsAdditionalHandlingInformation is provided, an empty string is expected.");

			consol.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "We have Handling Instructions");
			consol.Notes.AddNew(false, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "We have Dangerous Goods Additional Handling Information");

			cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			var expectedLineDescription = $"WE HAVE HANDLING INSTRUCTIONS{System.Environment.NewLine}WE HAVE DANGEROUS GOODS ADDITIONAL HANDLING INFORM";

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.SendersInstructions, Is.EqualTo(expectedLineDescription).Using(CustomComparers.TypeComparison), "When Handling Instructions or Dangerous Goods Additional Handling Information is provided, it should be available.");
		}

		[ExpectNoExceptions]
		public void TestCarrierAddress()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "The Best Carrier Company";
			var carrierAddress = carrier.Addresses.AddNew();
			carrierAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			carrierAddress.OA_Address1 = "109 Main St";
			carrierAddress.City = "Sydney";
			carrierAddress.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_ShippingLineAddress = carrierAddress.PK;

			var cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(this.consol);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.CarrierAddress, Is.EqualTo($"THE BEST CARRIER COMPANY{System.Environment.NewLine}109 MAIN ST{System.Environment.NewLine}SYDNEY - AUSTRALIA").Using(CustomComparers.TypeComparison), "Carrier address should be filled correctly.");
		}

		[ExpectNoExceptions]
		public void TestSpecialAgreements()
		{
			var cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.SpecialAgreements, Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison), "SpecialAgreements should be empty when no pickup instructions note is provided.");

			consol.Notes.AddNew(false, PredefinedNoteTypes.Instance.PickupInstructionsNote.Description, "We have Pickup Instructions Note");

			cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			var expectedLineDescription = "WE HAVE PICKUP INSTRUCTIONS NOTE";

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.SpecialAgreements, Is.EqualTo(expectedLineDescription).Using(CustomComparers.TypeComparison), "SpecialAgreements should include the pickup instructions note when provided.");
		}

		[ExpectNoExceptions]
		public void TestJobNumber()
		{
			var cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.JobNumber, Is.EqualTo(consol.JobNumber).Using(CustomComparers.TypeComparison), "JobNumber should be the consol job number.");
		}

		[ExpectNoExceptions]
		[TestDate(2025, 2, 04)]
		public void TestEstablishedInDate()
		{
			var cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.EstablishedInDate, Is.EqualTo("04 Feb 2025").Using(CustomComparers.TypeComparison), "EstablishedInDate should be in the format dd MMM yyyy.");
		}

		[ExpectNoExceptions]
		public void TestEstablishedInPlace()
		{
			ZString currentBranchPort = ZString.Empty;
			try
			{
				currentBranchPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "SGSIN";
				var cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);

				NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.EstablishedInPlace, Is.EqualTo("Singapore").Using(CustomComparers.TypeComparison), "EstablishedInPlace should be the current branch's home port.");
			}
			finally
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = currentBranchPort;
			}
		}

		[ExpectNoExceptions]
		public void TestDangerousGoods()
		{
			var shipment1 = consol.Shipments.AddNew();
			var packLine1 = shipment1.OuterPackLines.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			var packLine2 = shipment2.OuterPackLines.AddNew();

			var dg = packLine2.UNDGs.AddNew();
			dg.UNDGSubstancePivotCollection.UpdateDefaultPivot(UNDGSubstanceLoader.LoadSubstances(Factory, "0113", "", "IMO").First());
			dg.DI_IMOClass = "1.1B";
			consol.Containers.RemoveAll();

			AssertEquals("Prerequisite", 0, consol.Containers.Count);

			var cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol);
			// when there is no container, the dangerous goods information should be filled by shipment
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsClass, Is.EqualTo("1.1B").Using(CustomComparers.TypeComparison), "DangerousGoodsClass should be filled with the value from the shipment pack line.");
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsNumber, Is.EqualTo("0113").Using(CustomComparers.TypeComparison), "DangerousGoodsNumber should be filled with the value from the shipment pack line.");
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsLetter, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "DangerousGoodsLetter should be empty.");

			consol.Shipments.RemoveAll();

			var containerWithShipments = AddAllocatedPackLinesToConsol();

			dg = containerWithShipments.packline1.UNDGs.AddNew();
			dg.UNDGSubstancePivotCollection.UpdateDefaultPivot(UNDGSubstanceLoader.LoadSubstances(Factory, "0113", "", "IMO").First());
			dg.DI_IMOClass = "1.1A";

			var parameter = new DocDataObjectParameters("Test", "Test", containerWithShipments.container);
			cmrConsignmentNoteDocDataObject = GetConsolCMRConsignmentNoteDocData(consol, parameter);

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsClass, Is.EqualTo("1.1A").Using(CustomComparers.TypeComparison), "DangerousGoodsClass should be filled with the value from the pack line.");
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsNumber, Is.EqualTo("0113").Using(CustomComparers.TypeComparison), "DangerousGoodsNumber should be filled with the value from the pack line.");
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsLetter, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "DangerousGoodsLetter should be empty.");
		}

		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.NewWithValidTestData<ForwardingConsol>();
		}

		CMRConsignmentNoteDocDataObject GetConsolCMRConsignmentNoteDocData(ForwardingConsol consol, DocDataObjectParameters parameters = null)
		{
			if (parameters == null)
			{
				parameters = new DocDataObjectParameters("Test", "Test");
			}

			var consolCMRConsignmentNoteBuilder = new ConsolCMRConsignmentNoteBuilder(consol, parameters);
			return consolCMRConsignmentNoteBuilder.Build().CMRConsignmentNoteDocuments.First();
		}

		(ForwardingContainer container, ForwardingShipment shipment1, ForwardingPackLine packline1) AddAllocatedPackLinesToConsol()
		{
			var shipment1 = consol.Shipments.AddNew();
			var line11 = shipment1.OuterPackLines.AddNew();

			var shipment2 = consol.Shipments.AddNew();
			var line21 = shipment2.OuterPackLines.AddNew();

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "1";

			var pivot1 = Factory.New<JobContainerPackPivot>();
			pivot1.J6_JL = line11.PK;
			pivot1.J6_JC = container1.PK;

			var pivot2 = Factory.New<JobContainerPackPivot>();
			pivot2.J6_JL = line21.PK;
			pivot2.J6_JC = container1.PK;

			line11.JL_ContainerPackingOrder = 2;
			line21.JL_ContainerPackingOrder = 1;

			line11.JL_PackageCount = 10;
			line11.JL_F3_NKPackType = PkgUnit.Package;
			line11.JL_MarksAndNumbers = "MN1";
			line11.JL_Description = "THIS IS THE DESCRIPTION FOR ENTRY LINE 1 WHICH WILL BE CUT HERE --->THIS PART OF THE STRING WILL NOT BE SHOWN";
			line11.JL_ActualWeight = 10;
			line11.JL_ActualWeightUQ = Weight.Kilograms;
			line11.JL_ActualVolume = 2;
			line11.JL_ActualVolumeUQ = Volume.CubicMetres;

			line21.JL_PackageCount = 12;
			line21.JL_F3_NKPackType = PkgUnit.Skid;
			line21.JL_MarksAndNumbers = "MN2";
			line21.JL_Description = "Description2";
			line21.JL_ActualWeight = 100;
			line21.JL_ActualWeightUQ = Weight.Grams;
			line21.JL_ActualVolume = 100;
			line21.JL_ActualVolumeUQ = Volume.Litre;

			Factory.Save();
			return (container1, shipment1, line11);
		}

		void SetOverrideAddress(JobDocAddress jobDocAddress)
		{
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_CompanyName = "DHL";
			jobDocAddress.E2_Postcode = "1234";
			jobDocAddress.E2_City = "Brisbane";
			jobDocAddress.E2_RN_NKCountryCode = "HK";
			jobDocAddress.E2_Address1 = "Unit 13-1";
			jobDocAddress.E2_Address2 = "4 Lost Lane-1";
		}

		ForwardingConsol consol;
	}
}
