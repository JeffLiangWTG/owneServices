using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed partial class ShipmentCMRConsignmentNoteBuilderTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestSupplierAddress()
		{
			var cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.SupplierAddress, Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison), "SupplierAddress at this stage should be empty.");

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "MAERSK Name";
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignor.MainAddress.Address1 = "Address 1 Unit 13";
			consignor.MainAddress.Address2 = "ADDRESS 24 Lost Lane";
			consignor.MainAddress.City = "Sydney";
			consignor.MainAddress.Postcode = "2229";
			consignor.MainAddress.State = "NSW";
			consignor.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.SupplierAddress, Is.EqualTo($"MAERSK NAME{System.Environment.NewLine}ADDRESS 1 UNIT 13{System.Environment.NewLine}ADDRESS 24 LOST LANE{System.Environment.NewLine}SYDNEY - 2229 - AUSTRALIA").Using(CustomComparers.TypeComparison), "SupplierAddress");

			//override shipment's consignor address
			SetOverrideAddress(shipment.ConsignorDocumentaryAddress);

			cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.SupplierAddress, Is.EqualTo($"DHL{System.Environment.NewLine}UNIT 13-1{System.Environment.NewLine}4 LOST LANE-1{System.Environment.NewLine}BRISBANE - 1234 - HONG KONG").Using(CustomComparers.TypeComparison), "SupplierAddress when E2_AddressOverride is true");
		}

		[ExpectNoExceptions]
		public void TestImporterAddress()
		{
			var cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.ImporterAddress, Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison), "ImporterAddress at this stage should be empty.");

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "MAERSK";
			consignee.OH_RL_NKClosestPort = "AUSYD";
			consignee.MainAddress.Address1 = "Unit 13";
			consignee.MainAddress.Address2 = "4 Lost Lane";
			consignee.MainAddress.City = "Sydney";
			consignee.MainAddress.Postcode = "2229";
			consignee.MainAddress.State = "NSW";
			consignee.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.ImporterAddress, Is.EqualTo($"MAERSK{System.Environment.NewLine}UNIT 13{System.Environment.NewLine}4 LOST LANE{System.Environment.NewLine}SYDNEY - 2229 - AUSTRALIA").Using(CustomComparers.TypeComparison), "ImporterAddress");

			//override shipment's consignee address
			SetOverrideAddress(shipment.ConsigneeDocumentaryAddress);

			cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.ImporterAddress, Is.EqualTo($"DHL{System.Environment.NewLine}UNIT 13-1{System.Environment.NewLine}4 LOST LANE-1{System.Environment.NewLine}BRISBANE - 1234 - HONG KONG").Using(CustomComparers.TypeComparison), "ImporterAddress when E2_AddressOverride is true");
		}

		[ExpectNoExceptions]
		public void TestInternationalConsignmentNote()
		{
			var cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.InternationalConsignmentNote, Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison), "InternationalConsignmentNote should be empty.");
		}

		[ExpectNoExceptions]
		public void TestPlaceOfDelivery()
		{
			var cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.PlaceOfDelivery, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "The place of delivery at this stage should be empty.");

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "MAERSK";
			consignee.OH_RL_NKClosestPort = "AUSYD";
			consignee.MainAddress.Address1 = "Unit 13";
			consignee.MainAddress.Address2 = "4 Lost Lane";
			consignee.MainAddress.City = "Sydney";
			consignee.MainAddress.Postcode = "2229";
			consignee.MainAddress.State = "NSW";
			consignee.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.PlaceOfDelivery, Is.EqualTo("SYDNEY AU").Using(CustomComparers.TypeComparison), "The place of delivery must be filled in with the consignee's address if there is no delivery address.");

			//override shipment's consignee address
			SetOverrideAddress(shipment.ConsigneeDocumentaryAddress);

			cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.PlaceOfDelivery, Is.EqualTo("BRISBANE HK").Using(CustomComparers.TypeComparison), "The place of delivery must be filled in with the consignee's address if there is no delivery address and E2_AddressOverride is true.");

			var deliverTo = Factory.New<OrgHeader>();
			deliverTo.OH_FullName = "DeliverToCo";
			deliverTo.OH_RL_NKClosestPort = "SGSIN";
			deliverTo.MainAddress.Address1 = "Unit 1";
			deliverTo.MainAddress.Address2 = "4 What Lane";
			deliverTo.MainAddress.City = "Auckland";
			deliverTo.MainAddress.Postcode = "5022";
			deliverTo.MainAddress.OA_RN_NKCountryCode = "SG";
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliverTo.MainAddress.PK;

			cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.PlaceOfDelivery, Is.EqualTo("5022 AUCKLAND SG").Using(CustomComparers.TypeComparison), "The place of delivery must include the delivery address.");

			//override shipment's consignee address
			SetOverrideAddress(shipment.ConsigneeDeliveryAddress);

			cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.PlaceOfDelivery, Is.EqualTo("1234 BRISBANE HK").Using(CustomComparers.TypeComparison), "The place of delivery must include the delivery address when E2_AddressOverride is true.");
		}

		[ExpectNoExceptions]
		public void TestGoodsTakingOverPlaceAndDate()
		{
			var cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.CityCountryDateOfGoodsTakingOver, Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison), "When no address is provided, CityCountryDateOfGoodsTakingOver will be empty.");

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "MAERSK";
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignor.MainAddress.Address1 = "Unit 13";
			consignor.MainAddress.Address2 = "4 Lost Lane";
			consignor.MainAddress.City = "Sydney";
			consignor.MainAddress.Postcode = "2229";
			consignor.MainAddress.State = "NSW";
			consignor.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.CityCountryDateOfGoodsTakingOver, Is.EqualTo("SYDNEY AU").Using(CustomComparers.TypeComparison), "CityCountryDateOfGoodsTakingOver must be filled with consignor's address when there is no Pickup From address");

			//override shipment's consignor address
			SetOverrideAddress(shipment.ConsignorDocumentaryAddress);

			cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.CityCountryDateOfGoodsTakingOver, Is.EqualTo("BRISBANE HK").Using(CustomComparers.TypeComparison), "CityCountryDateOfGoodsTakingOver must be filled with consignor's address when there is no Pickup From address and E2_AddressOverride is true.");

			var today = ZDate.Today;
			var todayAsString = today.ToString("dd/MM/yyyy");

			shipment.DocsAndCartage.JP_EstimatedPickup = today;

			cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.CityCountryDateOfGoodsTakingOver, Is.EqualTo("BRISBANE HK").Using(CustomComparers.TypeComparison), "CityCountryDateOfGoodsTakingOver must be filled in with the consignor's address when there is no Pickup From address, even if there is a JP_EstimatedPickup.");

			//unset the override address
			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = false;

			cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.CityCountryDateOfGoodsTakingOver, Is.EqualTo("SYDNEY AU").Using(CustomComparers.TypeComparison), "CityCountryDateOfGoodsTakingOver must be filled in with the consignor's address when there is no Pickup From address, even if there is a JP_EstimatedPickup and E2_AddressOverride is false.");

			var pickupFrom = Factory.New<OrgHeader>();
			pickupFrom.OH_FullName = "PickupFromCo";
			pickupFrom.OH_RL_NKClosestPort = "AUSYD";
			pickupFrom.MainAddress.Address1 = "Unit 13";
			pickupFrom.MainAddress.Address2 = "4 Lost Lane";
			pickupFrom.MainAddress.City = "Sydney";
			pickupFrom.MainAddress.Postcode = "2000";
			pickupFrom.MainAddress.OA_RN_NKCountryCode = "SG";
			shipment.ConsignorPickupAddress.E2_OA_Address = pickupFrom.MainAddress.PK;

			cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.CityCountryDateOfGoodsTakingOver, Is.EqualTo($"SYDNEY SG {todayAsString}").Using(CustomComparers.TypeComparison), "When Pickup From and JP_EstimatedPickup are provided, they will be available in CityCountryDateOfGoodsTakingOver.");

			//override shipment's pickup address
			SetOverrideAddress(shipment.ConsignorPickupAddress);

			cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.CityCountryDateOfGoodsTakingOver, Is.EqualTo($"BRISBANE HK {todayAsString}").Using(CustomComparers.TypeComparison), "When Pickup From and JP_EstimatedPickup are provided, they will be available in CityCountryDateOfGoodsTakingOver when E2_AddressOverride is true.");
		}

		[ExpectNoExceptions]
		public void TestGoodsAttachedDocuments()
		{
			var cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.GoodsAttachedDocuments, Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison), "GoodsAttachedDocuments is empty");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsBox6_7_8_9()
		{
			shipment.JS_MarksAndNumbersShort = "MNM";
			shipment.JS_OuterPacks = 2;
			shipment.JS_F3_NKPackType = PkgUnit.Piece;
			shipment.JS_GoodsDescription = "Goods description";
			shipment.OuterPackLines.RemoveAll();
			var cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.LineDetailsBox6_7_8_9, Is.EqualTo("MNM; 2 PCE; Goods description").Using(CustomComparers.TypeComparison), "LineDetailsBox6_7_8_9 must be filled with the shipment's outer packaging when there is no packing line.");

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 10;
			packLine1.JL_F3_NKPackType = PkgUnit.Package;
			packLine1.JL_MarksAndNumbers = "MN1";
			packLine1.JL_Description = "THIS IS THE DESCRIPTION FOR ENTRY LINE 1 WHICH WILL BE CUT HERE --->THIS PART OF THE STRING WILL NOT BE SHOWN";

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 12;
			packLine2.JL_F3_NKPackType = PkgUnit.Skid;
			packLine2.JL_MarksAndNumbers = "MN2";
			packLine2.JL_Description = "Description2";

			cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			var expectedLineDescription = $"MN1; 10 PKG; THIS IS THE DESCRIPTION FOR ENTRY LINE 1 WHICH WILL BE CUT HERE ---{System.Environment.NewLine}MN2; 12 SKD; Description2";

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.LineDetailsBox6_7_8_9, Is.EqualTo(expectedLineDescription).Using(CustomComparers.TypeComparison), "LineDetailsBox6_7_8_9 provides details from the pack lines.");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsTariffCodeBox10()
		{
			shipment.JS_LoadingMeters = 3;
			var cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.LineDetailsTariffCodeBox10, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "LineDetailsTariffCodeBox10 should be empty");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsGrossWeightInKGBox11()
		{
			shipment.JS_ActualWeight = 2;
			shipment.JS_UnitOfWeight = Weight.Tonnes;
			shipment.OuterPackLines.RemoveAll();
			var cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.LineDetailsGrossWeightInKGBox11, Is.EqualTo("2000.00    ").Using(CustomComparers.TypeComparison), "LineDetailsGrossWeightInKGBox11 must be filled with the shipment's weight in kilograms when there is no packing line.");

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_ActualWeight = 10;
			packLine1.JL_ActualWeightUQ = Weight.Kilograms;

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_ActualWeight = 100;
			packLine2.JL_ActualWeightUQ = Weight.Grams;

			cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			var expectedLineDescription = $"10.00    {System.Environment.NewLine}0.1     ";

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.LineDetailsGrossWeightInKGBox11, Is.EqualTo(expectedLineDescription).Using(CustomComparers.TypeComparison), "LineDetailsGrossWeightInKGBox11 provides details from the pack lines in Kilograms (KG).");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsVolumeInM3Box12()
		{
			shipment.JS_ActualVolume = 20000;
			shipment.JS_UnitOfVolume = Volume.CubicCentimeters;
			shipment.OuterPackLines.RemoveAll();
			var cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.LineDetailsVolumeInM3Box12, Is.EqualTo("0.02    ").Using(CustomComparers.TypeComparison), "LineDetailsVolumeInM3Box12 must be filled with the shipment's volume in cubic meters (M3) when there is no pack line");

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_ActualVolume = 2;
			packLine1.JL_ActualVolumeUQ = Volume.CubicMetres;

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_ActualVolume = 100;
			packLine2.JL_ActualVolumeUQ = Volume.Litre;

			cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			var expectedLineDescription = $"2.00    {System.Environment.NewLine}0.1     ";

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.LineDetailsVolumeInM3Box12, Is.EqualTo(expectedLineDescription).Using(CustomComparers.TypeComparison), "LineDetailsVolumeInM3Box12 provides details from the pack lines.in cubic meters (M3).");
		}

		[ExpectNoExceptions]
		public void TestIncotermAndTextBox14()
		{
			var cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.IncotermAndTextBox14, Is.EqualTo(ZString.Empty), "When no Incoterm is provided, an empty string is expected.");

			shipment.JS_INCO = IncoTerms.FreeOnBoard;
			cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.IncotermAndTextBox14, Is.EqualTo("FOB - Free On Board").Using(CustomComparers.TypeComparison), "When an Incoterm is provided, it must be available in the code-description.");
		}

		[ExpectNoExceptions]
		public void TestSendersInstructions()
		{
			var cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.SendersInstructions, Is.EqualTo(ZString.Empty), "When no HandlingInstructions or DangerousGoodsAdditionalHandlingInformation is provided, an empty string is expected.");

			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "We have Handling Instructions");
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "We have Dangerous Goods Additional Handling Information");

			cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			var expectedLineDescription = $"WE HAVE HANDLING INSTRUCTIONS{System.Environment.NewLine}WE HAVE DANGEROUS GOODS ADDITIONAL HANDLING INFORM";

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.SendersInstructions, Is.EqualTo(expectedLineDescription).Using(CustomComparers.TypeComparison), "When Handling Instructions or Dangerous Goods Additional Handling Information is provided, it must be available.");
		}

		[ExpectNoExceptions]
		public void TestCarrierAddress()
		{
			var cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.CarrierAddress, Is.EqualTo(ZString.Empty), "When no console is provided, an empty string is expected.");

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "The Best Carrier Company";
			var carrierAddress = carrier.Addresses.AddNew();
			carrierAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			carrierAddress.OA_Address1 = "109 Main St";
			carrierAddress.City = "Sydney";
			carrierAddress.OA_RN_NKCountryCode = "AU";

			var refVessel = Factory.NewWithValidTestData<RefVessel>();
			refVessel.RV_OH = carrier.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			var mainTransport = consol.Transports[0];
			mainTransport.JW_IsLinked = false;
			mainTransport.JW_VoyageFlight = "123";
			mainTransport.JW_Vessel = refVessel.RV_Code;

			shipment.Consols.Add(consol);
			cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.CarrierAddress, Is.EqualTo($"THE BEST CARRIER COMPANY{System.Environment.NewLine}109 MAIN ST{System.Environment.NewLine}SYDNEY - AUSTRALIA").Using(CustomComparers.TypeComparison), "Carrier Address");
		}

		[ExpectNoExceptions]
		public void TestSpecialAgreements()
		{
			var cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.SpecialAgreements, Is.EqualTo(ZString.Empty), "When no pickup instructions note is provided, an empty string is expected.");

			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.PickupInstructionsNote.Description, "We have Pickup Instructions Note");

			cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);
			var expectedLineDescription = "WE HAVE PICKUP INSTRUCTIONS NOTE";

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.SpecialAgreements, Is.EqualTo(expectedLineDescription).Using(CustomComparers.TypeComparison), "When pickup instructions have been provided, they must be available.");
		}

		[ExpectNoExceptions]
		public void TestJobNumber()
		{
			shipment.JS_UniqueConsignRef = "S99937323";
			var cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.JobNumber, Is.EqualTo("S99937323").Using(CustomComparers.TypeComparison), "Job Number should be the shipment job number.");
		}

		[ExpectNoExceptions]
		[TestDate(2025, 2, 04)]
		public void TestEstablishedInDate()
		{
			var cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);

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
				var cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);

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
			shipment.OuterPackLines.RemoveAll();
			var cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsClass, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "DangerousGoodsClass is empty.");
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsNumber, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "DangerousGoodsNumber is empty.");
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsLetter, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "DangerousGoodsLetter is empty.");

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 10;
			packLine1.JL_F3_NKPackType = PkgUnit.Package;
			packLine1.JL_MarksAndNumbers = "MN1";
			packLine1.JL_Description = "THIS IS THE DESCRIPTION FOR ENTRY LINE 1 WHICH WILL BE CUT HERE -->THIS PART OF THE STRING WILL NOT BE SHOWN";
			packLine1.JL_ContainerPackingOrder = 1;

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 12;
			packLine2.JL_F3_NKPackType = PkgUnit.Skid;
			packLine2.JL_MarksAndNumbers = "MN2";
			packLine2.JL_Description = "Description2";
			packLine2.JL_ContainerPackingOrder = 2;

			cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsClass, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "There is no dangerous goods, DangerousGoodsClass is empty.");
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsNumber, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "There is no dangerous goods, DangerousGoodsNumber is empty.");
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsLetter, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "There is no dangerous goods, DangerousGoodsLetter is empty.");

			var dg = packLine2.UNDGs.AddNew();
			dg.UNDGSubstancePivotCollection.UpdateDefaultPivot(UNDGSubstanceLoader.LoadSubstances(Factory, "0113", "", "IMO").First());
			dg.DI_IMOClass = "1.1A";

			cmrConsignmentNoteDocDataObject = GetShipmentCMRConsignmentNoteDocData(shipment);

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsClass, Is.EqualTo("1.1A").Using(CustomComparers.TypeComparison), "DangerousGoodsClass value comes from pack line.");
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsNumber, Is.EqualTo("0113").Using(CustomComparers.TypeComparison), "DangerousGoodsNumber value comes from pack line.");
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsLetter, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "DangerousGoodsLetter is empty.");
		}

		protected override void SetUp()
		{
			base.SetUp();
			shipment = Factory.NewWithValidTestData<ForwardingShipment>();
		}

		CMRConsignmentNoteDocDataObject GetShipmentCMRConsignmentNoteDocData(ForwardingShipment shipment)
		{
			var shipmentCMRConsignmentNoteBuilder = new ShipmentCMRConsignmentNoteBuilder(shipment);
			return shipmentCMRConsignmentNoteBuilder.Build().CMRConsignmentNoteDocuments.First();
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

		ForwardingShipment shipment;
	}
}
