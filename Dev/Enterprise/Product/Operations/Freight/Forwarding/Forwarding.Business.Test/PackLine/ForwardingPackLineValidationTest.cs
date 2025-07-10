using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business.AWB.Testing;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingPackLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestJL_JL_OuterPackLine()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packline1 = shipment.OuterPackLines.AddNew();

			Factory.Save();

			var innerPackLine1 = shipment.InnerPackLines.AddNew();
			var innerPackLine2 = shipment.InnerPackLines.AddNew();

			innerPackLine1.JL_JL_OuterPackLine = packline1.PK;

			innerPackLine1.Validation.ValidateAll();

			AssertHasError("All inner packlines should be linked.", innerPackLine1.JL_JL_OuterPackLineInfo, "If at least one inner pack line is linked, it is mandatory for all inner pack lines to be linked to an outer pack.");

			innerPackLine2.JL_JL_OuterPackLine = packline1.PK;

			innerPackLine1.Validation.ValidateAll();
			AssertNoError("All inner packlines are linked", innerPackLine1.JL_JL_OuterPackLineInfo, "If at least one inner pack line is linked, it is mandatory for all inner pack lines to be linked to an outer pack.");
		}

		public void TestJL_ContainerPackingOrderIsUnique_PacklineInOneContainer()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S00001001";
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S00001002";
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();

			var packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.Containers.RemoveAll();
			packLine1.JL_JC = container1.PK;
			var packLine2 = shipment1.OuterPackLines.AddNew();
			packLine2.Containers.RemoveAll();
			packLine2.JL_JC = container1.PK;
			var packLine3 = shipment2.OuterPackLines.AddNew();
			packLine3.Containers.RemoveAll();
			packLine3.JL_JC = container1.PK;
			var packLine4 = shipment2.OuterPackLines.AddNew();
			packLine4.Containers.RemoveAll();
			packLine4.JL_JC = container2.PK;
			var packLine5 = shipment2.OuterPackLines.AddNew();
			packLine5.Containers.RemoveAll();

			AssertNoErrors(packLine1.JL_ContainerPackingOrderInfo);
			AssertNoErrors(packLine2.JL_ContainerPackingOrderInfo);
			AssertNoErrors(packLine3.JL_ContainerPackingOrderInfo);
			AssertNoErrors(packLine4.JL_ContainerPackingOrderInfo);
			AssertNoErrors(packLine5.JL_ContainerPackingOrderInfo);

			packLine2.JL_ContainerPackingOrder = 1;
			packLine3.JL_ContainerPackingOrder = 1;
			packLine4.JL_ContainerPackingOrder = 1;
			packLine5.JL_ContainerPackingOrder = 1;
			AssertNoErrors(packLine1.JL_ContainerPackingOrderInfo);
			AssertHasError(packLine2.JL_ContainerPackingOrderInfo, "This value must be unique on the container. Packing Order '1' is entered on the shipment(s) S00001001.");
			AssertHasError(packLine3.JL_ContainerPackingOrderInfo, "This value must be unique on the container. Packing Order '1' is entered on the shipment(s) S00001001, S00001002.");
			AssertNoErrors(packLine4.JL_ContainerPackingOrderInfo);
			AssertNoErrors(packLine5.JL_ContainerPackingOrderInfo);

			packLine2.JL_ContainerPackingOrder = 2;
			AssertNoErrors(packLine1.JL_ContainerPackingOrderInfo);
			AssertNoErrors(packLine2.JL_ContainerPackingOrderInfo);
			AssertHasError(packLine3.JL_ContainerPackingOrderInfo, "This value must be unique on the container. Packing Order '1' is entered on the shipment(s) S00001001, S00001002.");
			AssertNoErrors(packLine4.JL_ContainerPackingOrderInfo);
			AssertNoErrors(packLine5.JL_ContainerPackingOrderInfo);

			var packLine6 = shipment2.OuterPackLines.AddNew();
			packLine6.Containers.RemoveAndDeleteAll();
			packLine6.JL_JC = container1.PK;
			packLine1.JL_ContainerPackingOrder = 0;
			packLine6.JL_ContainerPackingOrder = 0;
			AssertHasWarning(packLine6.JL_ContainerPackingOrderInfo, "This value must be unique on the container.");
		}

		public void TestJL_ContainerPackingOrderIsUnique_PacklineInMultipleContainers()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var consol1 = shipment.Consols.AddNew();
			var container1 = consol1.Containers.AddNew();

			var consol2 = shipment.Consols.AddNew();
			var container2 = consol2.Containers.AddNew();

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.SetContainer(container1.PK);
			packline1.JL_ContainerPackingOrder = 1;

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.SetContainer(container1.PK);
			packline2.JL_ContainerPackingOrder = 1;
			shipment.RunPreSaveValidation();
			AssertHasError(packline1.JL_ContainerPackingOrderInfo, "This value must be unique on the container. Packing Order '1' is entered on the shipment(s) .");
			AssertHasError(packline2.JL_ContainerPackingOrderInfo, "This value must be unique on the container. Packing Order '1' is entered on the shipment(s) .");

			packline2.SetContainer(container2.PK);
			shipment.RunPreSaveValidation();
			AssertNoError(packline1.JL_ContainerPackingOrderInfo, "This value must be unique on the container. Packing Order '1' is entered on the shipment(s) .");
			AssertNoError(packline2.JL_ContainerPackingOrderInfo, "This value must be unique on the container. Packing Order '1' is entered on the shipment(s) .");
		}

		#region JL_ExportRefNumber

		public void TestValidateJL_ExportRefNumberHasOnlyOneRefNumber()
		{
			var packline = Factory.New<ForwardingPackLine>();
			packline.JL_ExportRefNumber = "EXP0001,EXP0002,EXP003";
			AssertHasError(packline.JL_ExportRefNumberInfo, "Enter just one Export Reference per pack line.");

			packline.JL_ExportRefNumber = "";
			AssertNoErrors(packline.JL_ExportRefNumberInfo);
		}

		public void TestValidateJL_ExportRefNumber_MRNFormatting()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.CustomsEntryNumberType = "MRN";
			shipment.CustomsEntryNumber = ZString.Empty;

			var packline = Factory.New<ForwardingPackLine>();
			packline.JL_JS = shipment.PK;
			packline.JL_ExportRefNumber = "Xylotrupes gideon";

			var error = @"MRN does not conform to the correct format. Please enter the MRN in the following format with only numbers and upper case letters:
• two numbers for the year of issue,
• two letters for the ISO country/region code for country/region of issue,
• thirteen alphanumeric characters for unique identification and
• one number check digit";

			AssertHasWarning(packline.JL_ExportRefNumberInfo, error);

			packline.JL_ExportRefNumber = "18DE123456789987M8";
			AssertHasWarning(packline.JL_ExportRefNumberInfo, "MRN does not have a valid check (last) digit. The check digit should be 0");

			packline.JL_ExportRefNumber = "10DE210119797085T1";
			AssertNoWarnings(packline.JL_ExportRefNumberInfo);
		}

		#endregion

		#region JL_ImportRefNumber

		public void TestValidateJL_ImportRefNumberHasOnlyOneRefNumber()
		{
			var packline = Factory.New<ForwardingPackLine>();
			packline.JL_ImportRefNumber = "IMP0001,IMP0002,IMP003";
			AssertHasError(packline.JL_ImportRefNumberInfo, "Enter just one Import Reference per pack line.");

			packline.JL_ImportRefNumber = "";
			AssertNoErrors(packline.JL_ImportRefNumberInfo);
		}

		#endregion

		#region JL_AdditionalInspectionTypeCode

		public void TestCheckJL_AdditionalIInspectionTypeCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "FRPAR";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_TransportMode = "AIR";
				shipment.JS_IsHighRisk = true;
				var shipmentPack = shipment.OuterPackLines.AddNew();
				shipmentPack.JL_IsHighRisk = false;
				AssertNoErrors("There shouldn't be any mistakes because Is High Risk is not checked", shipmentPack.JL_AdditionalInspectionTypeCodeInfo);

				shipmentPack.JL_IsHighRisk = true;
				shipmentPack.JL_AdditionalInspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;
				AssertNoErrors(shipmentPack.JL_AdditionalInspectionTypeCodeInfo);

				shipmentPack.JL_AdditionalInspectionTypeCode = "KKK";
				AssertHasError(shipmentPack.JL_AdditionalInspectionTypeCodeInfo, "Enter a valid selection.");

				shipmentPack.JL_AdditionalInspectionTypeCode = ZString.Empty;
				AssertHasError(shipmentPack.JL_AdditionalInspectionTypeCodeInfo, "Please enter a value.");

				var pack = Factory.New<ForwardingPackLine>();
				AssertNoErrors("There shouldn't be any mistakes because there is no supply chain security configuration", pack.JL_AdditionalInspectionTypeCodeInfo);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "FRPAR";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_TransportMode = "AIR";
				shipment.JS_IsHighRisk = true;
				var shipmentPack = shipment.OuterPackLines.AddNew();
				shipmentPack.JL_IsHighRisk = true;
				AssertEquals(ZString.Empty, shipmentPack.JL_AdditionalInspectionTypeCode);
				AssertNoErrors("There shouldn't be any mistakes because shipment is not High Risk applicable", shipmentPack.JL_AdditionalInspectionTypeCodeInfo);
			}
		}

		#endregion

		#region JL_InspectionTypeCode

		public void TestCheckJL_InspectionTypeCode_Sea()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "USCHI";
			shipment.JS_TransportMode = "SEA";

			var pack = shipment.OuterPackLines.AddNew();
			pack.JL_InspectionTypeCode = "UNK";
			AssertNoErrors(pack.JL_InspectionTypeCodeInfo);

			pack.JL_InspectionTypeCode = "XYZ";
			AssertNoErrors(pack.JL_InspectionTypeCodeInfo);

			pack.JL_InspectionTypeCode = "";
			AssertNoErrors(pack.JL_InspectionTypeCodeInfo);
		}

		public void TestCheckJL_InspectionTypeCode_Import()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_TransportMode = "AIR";

				var pack = shipment.OuterPackLines.AddNew();
				pack.JL_InspectionTypeCode = "UNK";
				AssertNoErrors(pack.JL_InspectionTypeCodeInfo);

				pack.JL_InspectionTypeCode = "XYZ";
				AssertNoErrors(pack.JL_InspectionTypeCodeInfo);

				pack.JL_InspectionTypeCode = "";
				AssertNoErrors(pack.JL_InspectionTypeCodeInfo);
			}
		}

		public void TestCheckJL_InspectionTypeCode_Disabled()
		{
			using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_AU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_TransportMode = "AIR";

				var pack = shipment.OuterPackLines.AddNew();
				pack.JL_InspectionTypeCode = "UNK";
				AssertNoErrors(pack.JL_InspectionTypeCodeInfo);

				pack.JL_InspectionTypeCode = "XYZ";
				AssertNoErrors(pack.JL_InspectionTypeCodeInfo);

				pack.JL_InspectionTypeCode = "";
				AssertNoErrors(pack.JL_InspectionTypeCodeInfo);
			}
		}

		public void TestCheckJL_InspectionTypeCode_MRA()
		{
			var expectedWarning = "This Shipment is destined for the United States, and as part of TSA's National Security Program (NSCP), MRAs are currently in place between the United States and your country, so in line with 100% screening legislation, a screening status must be recorded for all packages.";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_TransportMode = "AIR";
			shipment.JS_InspectionTypeCode = "UNK";

			var pack = shipment.OuterPackLines.AddNew();
			pack.JL_InspectionTypeCode = "UNK";
			pack.Validation.ValidateAll();
			AssertHasWarning(pack.JL_InspectionTypeCodeInfo, expectedWarning);

			pack.JL_InspectionTypeCode = "XYZ";
			AssertHasError(pack.JL_InspectionTypeCodeInfo, "Enter a valid Inspection.");
			AssertNoWarnings(pack.JL_InspectionTypeCodeInfo);

			pack.JL_InspectionTypeCode = "MAI";
			AssertNoNotifications(pack.JL_InspectionTypeCodeInfo);

			pack.JL_InspectionTypeCode = "";
			AssertHasError(pack.JL_InspectionTypeCodeInfo, "Please enter an Inspection.");

			shipment.JS_InspectionTypeCode = "APP";
			AssertNoNotifications(pack.JL_InspectionTypeCodeInfo);
		}

		public void TestCheckJL_InspectionTypeCode_NonMRA()
		{
			var expectedWarning = "This Shipment is destined for the United States so in line with 100% screening legislation, a screening status must be recorded for all packages.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "CNCHN";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_TransportMode = "AIR";
				shipment.JS_InspectionTypeCode = "UNK";

				var pack = shipment.OuterPackLines.AddNew();
				pack.JL_InspectionTypeCode = "UNK";
				pack.Validation.ValidateAll();
				AssertHasWarning(pack.JL_InspectionTypeCodeInfo, expectedWarning);

				pack.JL_InspectionTypeCode = "XYZ";
				AssertHasError(pack.JL_InspectionTypeCodeInfo, "Enter a valid Inspection.");

				pack.JL_InspectionTypeCode = "MAI";
				AssertNoWarnings(pack.JL_InspectionTypeCodeInfo);

				pack.JL_InspectionTypeCode = "";
				AssertHasError(pack.JL_InspectionTypeCodeInfo, "Please enter an Inspection.");

				shipment.JS_InspectionTypeCode = "APP";
				AssertHasWarning(pack.JL_InspectionTypeCodeInfo, expectedWarning);

				pack.JL_InspectionTypeCode = "UNK";
				AssertHasWarning(pack.JL_InspectionTypeCodeInfo, expectedWarning);
			}
		}

		public void TestCheckJL_InspectionTypeCode_USTranshipment_MRA()
		{
			var expectedWarning = "This Shipment is destined for the United States, and as part of TSA's National Security Program (NSCP), MRAs are currently in place between the United States and your country, so in line with 100% screening legislation, a screening status must be recorded for all packages.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = "AIR";
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "USLAX";

				consol.Transports[0].JW_TransportMode = "AIR";
				consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
				consol.Transports[0].JW_RL_NKDiscPort = "USLAX";

				var shipment = consol.Shipments.AddNew();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USCHI";
				shipment.JS_TransportMode = "AIR";
				shipment.JS_InspectionTypeCode = "UNK";

				var onForwardingTransport = shipment.Transports.AddNew();
				onForwardingTransport.JW_TransportMode = "AIR";
				onForwardingTransport.JW_RL_NKLoadPort = "USLAX";
				onForwardingTransport.JW_RL_NKDiscPort = "BRSAO";

				var pack = shipment.OuterPackLines.AddNew();
				pack.JL_InspectionTypeCode = "UNK";
				pack.Validation.ValidateAll();
				AssertHasWarning(pack.JL_InspectionTypeCodeInfo, expectedWarning);

				pack.JL_InspectionTypeCode = "XYZ";
				AssertHasError(pack.JL_InspectionTypeCodeInfo, "Enter a valid Inspection.");
				AssertNoWarnings(pack.JL_InspectionTypeCodeInfo);

				pack.JL_InspectionTypeCode = "MAI";
				AssertNoNotifications(pack.JL_InspectionTypeCodeInfo);

				pack.JL_InspectionTypeCode = "";
				AssertHasError(pack.JL_InspectionTypeCodeInfo, "Please enter an Inspection.");

				shipment.JS_InspectionTypeCode = "APP";
				AssertNoNotifications(pack.JL_InspectionTypeCodeInfo);
			}
		}

		public void TestCheckJL_InspectionTypeCode_USTranshipment_NonMRA()
		{
			var expectedWarning = "This Shipment is destined for the United States so in line with 100% screening legislation, a screening status must be recorded for all packages.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = "AIR";
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "BRSAO";

				var transport1 = consol.Transports[0];
				transport1.JW_TransportMode = "AIR";
				transport1.JW_RL_NKLoadPort = "AUSYD";
				transport1.JW_RL_NKDiscPort = "USLAX";

				var transport2 = consol.Transports.AddNew();
				transport2.JW_TransportMode = "AIR";
				transport2.JW_RL_NKLoadPort = "USLAX";
				transport2.JW_RL_NKDiscPort = "BRSAO";

				var shipment = consol.Shipments.AddNew();
				shipment.JS_RL_NKOrigin = "CNCHN";
				shipment.JS_RL_NKDestination = "USCHI";
				shipment.JS_TransportMode = "AIR";
				shipment.JS_InspectionTypeCode = "UNK";

				var pack = shipment.OuterPackLines.AddNew();
				pack.JL_InspectionTypeCode = "UNK";
				pack.Validation.ValidateAll();
				AssertHasWarning(pack.JL_InspectionTypeCodeInfo, expectedWarning);

				pack.JL_InspectionTypeCode = "XYZ";
				AssertHasError(pack.JL_InspectionTypeCodeInfo, "Enter a valid Inspection.");

				pack.JL_InspectionTypeCode = "MAI";
				AssertNoWarnings(pack.JL_InspectionTypeCodeInfo);

				pack.JL_InspectionTypeCode = "";
				AssertHasError(pack.JL_InspectionTypeCodeInfo, "Please enter an Inspection.");

				shipment.JS_InspectionTypeCode = "APP";
				AssertHasWarning(pack.JL_InspectionTypeCodeInfo, expectedWarning);

				pack.JL_InspectionTypeCode = "UNK";
				AssertHasWarning(pack.JL_InspectionTypeCodeInfo, expectedWarning);
			}
		}

		public void TestCheckJL_InspectionTypeCode_DestinationNonUS()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("IL"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "ILTLV";
				shipment.JS_RL_NKDestination = "DEFRA";
				shipment.JS_TransportMode = "AIR";
				shipment.JS_InspectionTypeCode = "UNK";

				var pack = shipment.OuterPackLines.AddNew();
				pack.JL_InspectionTypeCode = "UNK";
				AssertNoErrors(pack.JL_InspectionTypeCodeInfo);

				pack.JL_InspectionTypeCode = "XYZ";
				AssertNoErrors(pack.JL_InspectionTypeCodeInfo);

				pack.JL_InspectionTypeCode = "MAI";
				AssertNoErrors(pack.JL_InspectionTypeCodeInfo);

				pack.JL_InspectionTypeCode = "";
				AssertNoErrors(pack.JL_InspectionTypeCodeInfo);

				shipment.JS_InspectionTypeCode = "APP";
				AssertNoErrors(pack.JL_InspectionTypeCodeInfo);

				pack.JL_InspectionTypeCode = "UNK";
				AssertNoErrors(pack.JL_InspectionTypeCodeInfo);
			}
		}

		public void TestCheckJL_InspectionTypeCode_NoSCSModule_MRA()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Israel))
			{
				var supplyChainSecurityConfiguration = SupplyChainSecurityConfiguration.New();
				Assert("Precondition", supplyChainSecurityConfiguration.UsesGenericScheme);

				var expectedWarning = "This Shipment is destined for the United States, and as part of TSA's National Security Program (NSCP), MRAs are currently in place between the United States and your country, so in line with 100% screening legislation, a screening status must be recorded for all packages.";

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "ILTLV";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_TransportMode = "AIR";
				shipment.JS_InspectionTypeCode = "UNK";

				var pack = shipment.OuterPackLines.AddNew();
				pack.JL_InspectionTypeCode = "UNK";
				pack.Validation.ValidateAll();
				AssertHasWarning(pack.JL_InspectionTypeCodeInfo, expectedWarning);

				pack.JL_InspectionTypeCode = "XYZ";
				AssertHasError(pack.JL_InspectionTypeCodeInfo, "Enter a valid Inspection.");
				AssertNoWarnings(pack.JL_InspectionTypeCodeInfo);

				pack.JL_InspectionTypeCode = "MAI";
				AssertNoNotifications(pack.JL_InspectionTypeCodeInfo);

				pack.JL_InspectionTypeCode = "";
				AssertHasError(pack.JL_InspectionTypeCodeInfo, "Please enter an Inspection.");

				shipment.JS_InspectionTypeCode = "APP";
				AssertNoNotifications(pack.JL_InspectionTypeCodeInfo);
			}
		}

		public void TestCheckJL_InspectionTypeCode_NoSCSModule_NonMRA()
		{
			var expectedWarning = "This Shipment is destined for the United States so in line with 100% screening legislation, a screening status must be recorded for all packages.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JM"))
			{
				var supplyChainSecurityConfiguration = SupplyChainSecurityConfiguration.New();
				Assert("Precondition", supplyChainSecurityConfiguration.UsesGenericScheme);

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "JMKIN";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_TransportMode = "AIR";
				shipment.JS_InspectionTypeCode = "UNK";

				var pack = shipment.OuterPackLines.AddNew();
				pack.JL_InspectionTypeCode = "UNK";
				pack.Validation.ValidateAll();
				AssertHasWarning(pack.JL_InspectionTypeCodeInfo, expectedWarning);

				pack.JL_InspectionTypeCode = "XYZ";
				AssertHasError(pack.JL_InspectionTypeCodeInfo, "Enter a valid Inspection.");

				pack.JL_InspectionTypeCode = "MAI";
				AssertNoWarnings(pack.JL_InspectionTypeCodeInfo);

				pack.JL_InspectionTypeCode = "";
				AssertHasError(pack.JL_InspectionTypeCodeInfo, "Please enter an Inspection.");

				shipment.JS_InspectionTypeCode = "APP";
				AssertHasWarning(pack.JL_InspectionTypeCodeInfo, expectedWarning);

				pack.JL_InspectionTypeCode = "UNK";
				AssertHasWarning(pack.JL_InspectionTypeCodeInfo, expectedWarning);
			}
		}
		public void TestCheckJL_InspectionTypeCode_ShipmentDestinationIsNull()
		{
			var expectedWarning = "This Shipment is destined for the United States, and as part of TSA's National Security Program (NSCP), MRAs are currently in place between the United States and your country, so in line with 100% screening legislation, a screening status must be recorded for all packages.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Denmark))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "DKCPH";
				shipment.JS_RL_NKDestination = "US";
				shipment.JS_TransportMode = "AIR";

				AssertNull("Precondition: shipment destination should be null", shipment.Destination);

				var pack = shipment.OuterPackLines.AddNew();
				pack.JL_InspectionTypeCode = "UNK";
				pack.Validation.ValidateJL_InspectionTypeCode();
				AssertHasWarning(pack.JL_InspectionTypeCodeInfo, expectedWarning);
			}
		}

		public void TestCheckJL_InspectionTypeCode_AUExport()
		{
			var expectedWarning = "In line with Air Cargo Piece Level Security Screening legislation, a screening status must be recorded for all packages, unless the shipment is APP.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUBNE";
				shipment.JS_RL_NKDestination = "HKHKG";
				shipment.JS_TransportMode = "AIR";
				shipment.JS_InspectionTypeCode = "UNK";

				var pack = shipment.OuterPackLines.AddNew();
				pack.JL_InspectionTypeCode = "UNK";
				pack.Validation.ValidateAll();
				AssertHasWarning(pack.JL_InspectionTypeCodeInfo, expectedWarning);

				pack.JL_InspectionTypeCode = "XYZ";
				AssertHasError(pack.JL_InspectionTypeCodeInfo, "Enter a valid Inspection.");

				pack.JL_InspectionTypeCode = "MAI";
				AssertNoWarnings(pack.JL_InspectionTypeCodeInfo);

				pack.JL_InspectionTypeCode = "";
				AssertHasError(pack.JL_InspectionTypeCodeInfo, "Please enter an Inspection.");

				shipment.JS_InspectionTypeCode = "APP";
				AssertNoWarnings(pack.JL_InspectionTypeCodeInfo);

				pack.JL_InspectionTypeCode = "UNK";
				AssertHasWarning(pack.JL_InspectionTypeCodeInfo, expectedWarning);

				shipment.JS_InspectionTypeCode = "SCR";
				AssertHasError("Packline cannot be UNK if shipment is SCR", pack.JL_InspectionTypeCodeInfo, "The packline Inspection cannot be UNK as the Shipment Inspection method is SCR – Screened, which means each packline of the shipment is screened.");
			}
		}

		public void TestCheckJL_InspectionTypeCode_HKExport()
		{
			var expectedWarning = "In line with Air Cargo Piece Level Security Screening legislation, a screening status must be recorded for all packages, unless the shipment is APP.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "HKHKG";
				shipment.JS_RL_NKDestination = "DKCPH";
				shipment.JS_TransportMode = "AIR";
				shipment.JS_InspectionTypeCode = "UNK";

				var pack = shipment.OuterPackLines.AddNew();
				pack.JL_InspectionTypeCode = "UNK";
				pack.Validation.ValidateAll();
				AssertHasWarning(pack.JL_InspectionTypeCodeInfo, expectedWarning);

				pack.JL_InspectionTypeCode = "XYZ";
				AssertHasError(pack.JL_InspectionTypeCodeInfo, "Enter a valid Inspection.");

				pack.JL_InspectionTypeCode = "MAI";
				AssertNoWarnings(pack.JL_InspectionTypeCodeInfo);

				pack.JL_InspectionTypeCode = "";
				AssertHasError(pack.JL_InspectionTypeCodeInfo, "Please enter an Inspection.");

				shipment.JS_InspectionTypeCode = "APP";
				AssertNoWarnings(pack.JL_InspectionTypeCodeInfo);

				pack.JL_InspectionTypeCode = "UNK";
				AssertHasWarning(pack.JL_InspectionTypeCodeInfo, expectedWarning);

				shipment.JS_InspectionTypeCode = "SCR";
				AssertHasError("Packline cannot be UNK if shipment is SCR", pack.JL_InspectionTypeCodeInfo, "The packline Inspection cannot be UNK as the Shipment Inspection method is SCR – Screened, which means each packline of the shipment is screened.");
			}
		}

		public void TestJL_InspectionTypeCode_EU()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "FRCDG";
				shipment.JS_RL_NKDestination = "HKHKG";
				shipment.JS_TransportMode = "AIR";
				shipment.JS_InspectionTypeCode = "UNK";

				var pack1 = shipment.OuterPackLines.AddNew();
				var pack2 = shipment.OuterPackLines.AddNew();

				pack1.JL_InspectionTypeCode = "";
				pack2.JL_InspectionTypeCode = "";

				AssertNoErrors("Blank is allowed", pack1.JL_InspectionTypeCodeInfo);
				AssertNoErrors("Blank is allowed", pack2.JL_InspectionTypeCodeInfo);

				pack1.JL_InspectionTypeCode = "PHS";
				pack2.JL_InspectionTypeCode = "";

				AssertHasError("If any packline has an inspection then they all must", pack2.JL_InspectionTypeCodeInfo, "Please enter an Inspection.");

				pack1.JL_InspectionTypeCode = "UNK";
				pack2.JL_InspectionTypeCode = "";

				AssertHasError("Also applies if any packline has 'UNK'", pack2.JL_InspectionTypeCodeInfo, "Please enter an Inspection.");

				pack2.JL_InspectionTypeCode = "XRY";

				AssertNoErrors("All packlines have inspection", pack2.JL_InspectionTypeCodeInfo);

				shipment.JS_InspectionTypeCode = "SCR";

				AssertHasError("Packline cannot be UNK if shipment is SCR", pack1.JL_InspectionTypeCodeInfo, "The packline Inspection cannot be UNK as the Shipment Inspection method is SCR – Screened, which means each packline of the shipment is screened.");
				AssertNoError("XRY is OK", pack2.JL_InspectionTypeCodeInfo, "The packline Inspection cannot be UNK as the Shipment Inspection method is SCR – Screened, which means each packline of the shipment is screened.");

				pack1.JL_InspectionTypeCode = "";

				AssertHasError("Packline inspection cannot be blank as shipment is SCR", pack1.JL_InspectionTypeCodeInfo, "Please enter an Inspection.");

				shipment.JS_InspectionTypeCode = "UNK";
				pack1.JL_InspectionTypeCode = "UNK";

				AssertNoError("UNK is OK if shipment is also UNK", pack1.JL_InspectionTypeCodeInfo, "The packline Inspection cannot be UNK as the Shipment Inspection method is SCR – Screened, which means each packline of the shipment is screened.");
			}
		}

		public void TestJL_InspectionTypeCode_Transhipment()
		{
			ZGuid shipmentPK;
			var message = "Please enter an Inspection.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipmentPK = shipment.PK;
				shipment.JS_InspectionTypeCode = "XRY";
				shipment.JS_RL_NKOrigin = "CNSHA";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

				var consol1 = shipment.Consols.AddNew();
				consol1.JK_RL_NKLoadPort = "CNSHA";
				consol1.JK_RL_NKDischargePort = "DEHAM";
				consol1.JK_TransportMode = Core.Constants.TransportModes.Air;

				var consol2 = shipment.Consols.AddNew();
				consol2.JK_RL_NKLoadPort = "DEHAM";
				consol2.JK_RL_NKDischargePort = "USLAX";
				consol2.JK_TransportMode = Core.Constants.TransportModes.Air;

				var pack1 = shipment.OuterPackLines.AddNew();
				pack1.JL_PackageCount = 1;
				var pack2 = shipment.OuterPackLines.AddNew();
				pack2.JL_PackageCount = 2;

				shipment.OuterPackLines.Cast<ForwardingPackLine>()
					.ForEach(p =>
					{
						AssertEquals("UNK", p.JL_InspectionTypeCode);
						Assert(!p.JL_InspectionTypeCodeInfo.ReadOnly);
						AssertNoError(p.JL_InspectionTypeCodeInfo, message);
					});

				pack1.JL_InspectionTypeCode = ZString.Empty;
				AssertHasError(pack1.JL_InspectionTypeCodeInfo, message);

				pack1.JL_InspectionTypeCode = "XRY";
				AssertNoError(pack1.JL_InspectionTypeCodeInfo, message);

				Factory.Save();
			}
		}

		public void TestWarningForShiLianDanIsUsedInMoreThanOneShipment()
		{
			InsertPackLineTestData("CNSHA", "S00001001", "SLD001");
			InsertEntryNumTestData("CNSHA", "S00001002", "SLD001");

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var packLine = InsertPackLineTestData("CNSHA", "S00001003", "SLD001");
				var expectWarning = "This Shi Lian Dan/Shipping Order Number is already in use on: S00001001, S00001002";
				AssertHasWarning(packLine.JL_ExportRefNumberInfo, expectWarning);
			}

			ForwardingPackLine InsertPackLineTestData(string origin, string consignRef, string exportRefNumber)
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = origin;
				shipment.JS_UniqueConsignRef = consignRef;
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

				var consol = shipment.Consols.AddNew();
				var container = consol.Containers.AddNew();

				var packline = shipment.OuterPackLines.AddNew();
				packline.SetContainer(container.PK);
				packline.JL_ContainerPackingOrder = 1;
				packline.JL_ExportRefNumber = exportRefNumber;
				return packline;
			}

			CusEntryNumber InsertEntryNumTestData(string origin, string consignRef, string exportRefNumber)
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_UniqueConsignRef = consignRef;
				shipment.JS_RL_NKOrigin = origin;
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

				var shipmentEntryNum = shipment.Numbers.AddNew();
				shipmentEntryNum.CE_EntryType = ChinaAdditionalReferenceNumberTypes.Codes.ShippingOrderNumber;
				shipmentEntryNum.CE_EntryNum = exportRefNumber;
				return shipmentEntryNum;
			}
		}

		public void TestJL_InspectionTypeCode_InnerPackage_Disabled()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "DEFRA";
				shipment.JS_RL_NKDestination = "SGSIN";
				shipment.JS_TransportMode = "AIR";
				shipment.JS_InspectionTypeCode = "UNK";
				shipment.OuterPackLines.AddNew();
				shipment.InnerPackLines.AddNew();

				var outerPack = shipment.OuterPackLines.Cast<ForwardingPackLine>().First();
				var innerPack = shipment.InnerPackLines.Cast<ForwardingPackLine>().First();

				outerPack.JL_InspectionTypeCode = "";
				innerPack.JL_InspectionTypeCode = "";
				outerPack.Validation.ValidateJL_InspectionTypeCode();
				AssertNoErrors(outerPack.JL_InspectionTypeCodeInfo);
				AssertNoErrors(innerPack.JL_InspectionTypeCodeInfo);

				shipment.JS_InspectionTypeCode = "XRY";
				outerPack.JL_InspectionTypeCode = "XRY";
				innerPack.JL_InspectionTypeCode = "";
				outerPack.Validation.ValidateJL_InspectionTypeCode();
				AssertNoErrors(outerPack.JL_InspectionTypeCodeInfo);
				innerPack.Validation.ValidateJL_InspectionTypeCode();
				AssertNoErrors(innerPack.JL_InspectionTypeCodeInfo);
			}
		}

		public void TestJL_InspectionTypeCode_APP_EU() => AssertJL_InspectionTypeCode_APP("DEHAM");

		public void TestJL_InspectionTypeCode_APP_UK() => AssertJL_InspectionTypeCode_APP("GBLON");

		void AssertJL_InspectionTypeCode_APP(ZString origin)
		{
			const string errorMessage = "'Approved' cannot be selected here. It is provided as an automatic update from the linked Transit Warehouse.";
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(origin.Left(2)))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = origin;
				shipment.JS_RL_NKDestination = "HKHKG";
				shipment.JS_TransportMode = "AIR";
				shipment.JS_InspectionTypeCode = "UNK";

				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_InspectionTypeCode = "";

				Factory.Save();

				packLine.JL_InspectionTypeCode = FreightDataRegistry.AviationSecurity_PackLine_IsSecured_Code;
				packLine.Validation.ValidateJL_InspectionTypeCode();
				AssertHasError(packLine.JL_InspectionTypeCodeInfo, errorMessage);
				packLine.JL_InspectionTypeCode = "";

				using (shipment.SetIsMarkingPackLinesAsSecuredAllowed())
				{
					packLine.JL_InspectionTypeCode = FreightDataRegistry.AviationSecurity_PackLine_IsSecured_Code;
					packLine.Validation.ValidateJL_InspectionTypeCode();
					AssertNoError(packLine.JL_InspectionTypeCodeInfo, errorMessage);
				}
			}
		}

		public void TestJL_InspectionTypeCode_APP_Update_After_Saved_EU() => AssertJL_InspectionTypeCode_APP_Update_After_Saved("DEHAM");

		public void TestJL_InspectionTypeCode_APP_Update_After_Saved_UK() => AssertJL_InspectionTypeCode_APP_Update_After_Saved("GBLON");

		void AssertJL_InspectionTypeCode_APP_Update_After_Saved(ZString origin)
		{
			// Arrange
			const string errorMessage = "'Approved' cannot be selected here. It is provided as an automatic update from the linked Transit Warehouse.";
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(origin.Left(2)))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = origin;
				shipment.JS_RL_NKDestination = "HKHKG";
				shipment.JS_TransportMode = "AIR";
				shipment.JS_InspectionTypeCode = "UNK";

				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_InspectionTypeCode = "UNK";

				Factory.Save();

				var packLine2 = shipment.OuterPackLines.AddNew();
				packLine2.JL_InspectionTypeCode = FreightDataRegistry.AviationSecurity_PackLine_IsSecured_Code;
				AssertHasError(packLine2.JL_InspectionTypeCodeInfo, errorMessage);
				packLine2.JL_InspectionTypeCode = "";

				using (shipment.SetIsMarkingPackLinesAsSecuredAllowed())
				{
					packLine2.JL_InspectionTypeCode = FreightDataRegistry.AviationSecurity_PackLine_IsSecured_Code;
					AssertNoError(packLine2.JL_InspectionTypeCodeInfo, errorMessage);
				}

				Factory.Save();

				// Act
				packLine2.JL_Description = "Update after save";
				packLine2.Validation.ValidateJL_InspectionTypeCode();

				// Assert
				AssertNoError(packLine2.JL_InspectionTypeCodeInfo, errorMessage);
			}
		}

		public void TestJL_InspectionTypeCode_SkipValidationForBooking()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_IsForwardRegistered = false;
				shipment.JS_RL_NKOrigin = "FRCDG";
				shipment.JS_RL_NKDestination = "HKHKG";
				shipment.JS_TransportMode = "AIR";
				shipment.JS_InspectionTypeCode = "UNK";

				var pack1 = shipment.OuterPackLines.AddNew();
				var pack2 = shipment.OuterPackLines.AddNew();

				pack1.JL_InspectionTypeCode = "UNK";
				pack2.JL_InspectionTypeCode = "";

				AssertEquals(false, pack2.JL_InspectionTypeCodeInfo.HasErrors());
			}
		}

		#endregion

		#region JL_PackageCount

		public void TestJL_PackageCount()
		{
			var expectedWarning = "Zero packages is valid only for an empty container or an unknown number of packages. If your container is empty, flag it accordingly from Consol > Containers tab.";
			var expectedError = "Container 'XYZ' cannot have pack lines with pack count greater than 0 as it is flagged empty.";

			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var container = consol.Containers.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			container.JC_ContainerNum = "XYZ";
			container.PackLines.Add(packLine);
			packLine.JL_PackageCount = 0;

			packLine.RunPreSaveValidation();
			AssertHasWarning(packLine.JL_PackageCountInfo, expectedWarning);
			AssertNoError(packLine.JL_PackageCountInfo, expectedError);

			packLine.JL_PackageCount = 1;
			AssertNoWarning(packLine.JL_PackageCountInfo, expectedWarning);

			container.JC_IsEmptyContainer = true;
			packLine.JL_PackageCount = 0;
			AssertNoWarning(packLine.JL_PackageCountInfo, expectedWarning);

			packLine.JL_PackageCount = 1;
			AssertHasError(packLine.JL_PackageCountInfo, expectedError);
		}

		#endregion

		#region JL_Dimensions

		public void TestValidateJK_MaximumPackageDimensions_Packlines_Errors()
		{
			PreAllocationCheckCollection checks = ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.Value;
			checks.Dimensions.Action = PreAllocationCheck.Actions.Restriction;
			checks.Dimensions.Percentage = 100m;

			using (ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, checks))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_MaximumAllowablePackageLength = 7;
				consol.JK_MaximumAllowablePackageWidth = 6;
				consol.JK_MaximumAllowablePackageHeight = 5;
				consol.JK_MaximumAllowablePackageUnit = Core.Constants.Length.Metres;

				var shipment = consol.Shipments.AddNew();

				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_Length = 7;
				packLine.JL_Width = 6;
				packLine.JL_Height = 5;
				packLine.JL_UnitOfDimension = Core.Constants.Length.Metres;

				AssertGreaterThanOrEqualTo("Precondition: length is less than allowed length", consol.JK_MaximumAllowablePackageLength, packLine.JL_Length);
				AssertGreaterThanOrEqualTo("Precondition: width is less than allowed width", consol.JK_MaximumAllowablePackageWidth, packLine.JL_Width);
				AssertGreaterThanOrEqualTo("Precondition: height is less than allowed height", consol.JK_MaximumAllowablePackageHeight, packLine.JL_Height);

				var errorMessage = MaximumPackageDimensionsErrorMessage;

				AssertNoError(packLine.JL_LengthInfo, errorMessage);
				AssertNoError(packLine.JL_WidthInfo, errorMessage);
				AssertNoError(packLine.JL_HeightInfo, errorMessage);

				packLine.JL_Length = 6;
				packLine.JL_Width = 7;
				packLine.JL_Height = 10;

				AssertNoError(packLine.JL_LengthInfo, errorMessage);
				AssertNoError(packLine.JL_WidthInfo, errorMessage);
				AssertHasError(packLine.JL_HeightInfo, errorMessage);

				packLine.JL_Length = 8;
				packLine.JL_Width = 8;
				packLine.JL_Height = 5;

				AssertHasError(packLine.JL_LengthInfo, errorMessage);
				AssertHasError(packLine.JL_WidthInfo, errorMessage);
				AssertNoError(packLine.JL_HeightInfo, errorMessage);
			}
		}

		public void TestValidateJK_MaximumPackageDimensions_PacklinesConvertedUnits_Errors()
		{
			PreAllocationCheckCollection checks = ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.Value;
			checks.Dimensions.Action = PreAllocationCheck.Actions.Restriction;
			checks.Dimensions.Percentage = 100m;

			using (ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, checks))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_MaximumAllowablePackageLength = 5;
				consol.JK_MaximumAllowablePackageWidth = 5;
				consol.JK_MaximumAllowablePackageHeight = 5;
				consol.JK_MaximumAllowablePackageUnit = Core.Constants.Length.Metres;

				var shipment = consol.Shipments.AddNew();

				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_Length = 10;
				packLine.JL_Width = 10;
				packLine.JL_Height = 10;
				packLine.JL_UnitOfDimension = Core.Constants.Length.Feet;

				AssertGreaterThan<ZDecimal>("Precondition: length is less than allowed length", consol.JK_MaximumAllowablePackageLength, Core.Constants.Length.Convert(packLine.JL_Length, Core.Constants.Length.Feet, consol.JK_MaximumAllowablePackageUnit));
				AssertGreaterThan<ZDecimal>("Precondition: width is less than allowed width", consol.JK_MaximumAllowablePackageWidth, Core.Constants.Length.Convert(packLine.JL_Width, Core.Constants.Length.Feet, consol.JK_MaximumAllowablePackageUnit));
				AssertGreaterThan<ZDecimal>("Precondition: height is less than allowed height", consol.JK_MaximumAllowablePackageHeight, Core.Constants.Length.Convert(packLine.JL_Height, Core.Constants.Length.Feet, consol.JK_MaximumAllowablePackageUnit));

				var errorMessage = MaximumPackageDimensionsErrorMessage;

				AssertNoError(packLine.JL_LengthInfo, errorMessage);
				AssertNoError(packLine.JL_WidthInfo, errorMessage);
				AssertNoError(packLine.JL_HeightInfo, errorMessage);

				packLine.JL_Length = 20;
				packLine.JL_Width = 20;
				packLine.JL_Height = 20;

				AssertLessThan<ZDecimal>("Precondition: length is greater than allowed length", consol.JK_MaximumAllowablePackageLength, Core.Constants.Length.Convert(packLine.JL_Length, Core.Constants.Length.Feet, consol.JK_MaximumAllowablePackageUnit));
				AssertLessThan<ZDecimal>("Precondition: width is greater than allowed width", consol.JK_MaximumAllowablePackageWidth, Core.Constants.Length.Convert(packLine.JL_Width, Core.Constants.Length.Feet, consol.JK_MaximumAllowablePackageUnit));
				AssertLessThan<ZDecimal>("Precondition: height is greater than allowed height", consol.JK_MaximumAllowablePackageHeight, Core.Constants.Length.Convert(packLine.JL_Height, Core.Constants.Length.Feet, consol.JK_MaximumAllowablePackageUnit));

				AssertHasError(packLine.JL_LengthInfo, errorMessage);
				AssertHasError(packLine.JL_WidthInfo, errorMessage);
				AssertHasError(packLine.JL_HeightInfo, errorMessage);
			}
		}

		public void TestValidateJK_MaximumPackageDimensions_Packlines_MultipleConsols()
		{
			PreAllocationCheckCollection checks = ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.Value;
			checks.Dimensions.Action = PreAllocationCheck.Actions.Restriction;
			checks.Dimensions.Percentage = 100m;

			using (ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, checks))
			{
				var consol1 = Factory.New<ForwardingConsol>();
				consol1.JK_MaximumAllowablePackageLength = 7;
				consol1.JK_MaximumAllowablePackageWidth = 6;
				consol1.JK_MaximumAllowablePackageHeight = 5;
				consol1.JK_MaximumAllowablePackageUnit = Core.Constants.Length.Metres;

				var consol2 = Factory.New<ForwardingConsol>();
				consol2.JK_MaximumAllowablePackageLength = 6;
				consol2.JK_MaximumAllowablePackageWidth = 7;
				consol2.JK_MaximumAllowablePackageHeight = 10;
				consol2.JK_MaximumAllowablePackageUnit = Core.Constants.Length.Metres;

				var shipment = consol1.Shipments.AddNew();
				consol2.Shipments.Add(shipment);

				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_Length = 7;
				packLine.JL_Width = 6;
				packLine.JL_Height = 5;
				packLine.JL_UnitOfDimension = Core.Constants.Length.Metres;

				AssertGreaterThanOrEqualTo("Precondition: length is less than allowed length", consol1.JK_MaximumAllowablePackageLength, packLine.JL_Length);
				AssertGreaterThanOrEqualTo("Precondition: width is less than allowed width", consol1.JK_MaximumAllowablePackageWidth, packLine.JL_Width);
				AssertGreaterThanOrEqualTo("Precondition: height is less than allowed height", consol1.JK_MaximumAllowablePackageHeight, packLine.JL_Height);

				var errorMessage = MaximumPackageDimensionsErrorMessage;

				AssertNoError(packLine.JL_LengthInfo, errorMessage);
				AssertNoError(packLine.JL_WidthInfo, errorMessage);
				AssertNoError(packLine.JL_HeightInfo, errorMessage);

				packLine.JL_Length = 6;
				packLine.JL_Width = 7;
				packLine.JL_Height = 10;

				AssertNoError(packLine.JL_LengthInfo, errorMessage);
				AssertNoError(packLine.JL_WidthInfo, errorMessage);
				AssertHasError(packLine.JL_HeightInfo, errorMessage);

				packLine.JL_Length = 8;
				packLine.JL_Width = 8;
				packLine.JL_Height = 5;

				AssertHasError(packLine.JL_LengthInfo, errorMessage);
				AssertHasError(packLine.JL_WidthInfo, errorMessage);
				AssertNoError(packLine.JL_HeightInfo, errorMessage);
			}
		}

		const string MaximumPackageDimensionsErrorMessage = "This dimension exceeds the maximum dimensions set on the consol.";

		#endregion

		#region JL_JC

		public void TestJL_JC()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			var pack1_1 = shipment1.OuterPackLines.AddNew();
			var pack1_2 = shipment1.OuterPackLines.AddNew();
			var pack2_1 = shipment2.OuterPackLines.AddNew();
			var pack2_2 = shipment2.OuterPackLines.AddNew();

			shipment1.JS_UniqueConsignRef = "SHIP1";
			shipment2.JS_UniqueConsignRef = "SHIP2";

			container1.JC_ContainerNum = "CON1";
			container1.JC_IsEmptyContainer = true;

			container2.JC_ContainerNum = "CON2";

			pack1_1.JL_JC = container1.PK;
			pack1_2.JL_JC = container1.PK;

			AssertHasErrorContaining(pack1_2.JL_JCInfo, "cannot contain more than one pack line as it is flagged empty.");

			pack1_1.JL_JC = container2.PK;
			pack1_2.JL_JC = container2.PK;

			AssertNoErrorContaining(pack1_2.JL_JCInfo, "cannot contain more than one pack line as it is flagged empty.");

			pack1_1.JL_JC = container1.PK;
			pack2_1.JL_JC = container1.PK;

			AssertHasErrorContaining(pack2_1.JL_JCInfo, "cannot contain more than one pack line as it is flagged empty. The following shipments have pack lines which are packed in this container:");

			pack1_2.JL_JC = container2.PK;
			pack2_2.JL_JC = container2.PK;

			AssertNoErrorContaining(pack2_2.JL_JCInfo, "cannot contain more than one pack line as it is flagged empty. The following shipments have pack lines which are packed in this container:");
		}

		#endregion

		#region JL_HarmonisedCode

		public void TestCheckJL_HarmonisedCode()
		{
			var errorMessage = "Invalid Harmonized Code. Only numeric characters and dots are allowed.";

			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();

			packLine.JL_HarmonisedCode = "123ABC";
			AssertHasError(packLine.JL_HarmonisedCodeInfo, errorMessage);

			packLine.JL_HarmonisedCode = "123A$#";
			AssertHasError(packLine.JL_HarmonisedCodeInfo, errorMessage);

			packLine.JL_HarmonisedCode = "......";
			AssertHasError(packLine.JL_HarmonisedCodeInfo, errorMessage);

			packLine.JL_HarmonisedCode = ".123";
			AssertHasError(packLine.JL_HarmonisedCodeInfo, errorMessage);

			packLine.JL_HarmonisedCode = string.Empty;
			AssertNoErrors(packLine.JL_HarmonisedCodeInfo);

			packLine.JL_HarmonisedCode = "13.3.1";
			AssertNoErrors(packLine.JL_HarmonisedCodeInfo);

			var dbPackline = shipment.OuterPackLines.AddNew();

			using (dbPackline.SuspendValidationTesting())
			{
				dbPackline.JL_HarmonisedCode = "123ABC";
				Factory.Save();
			}

			dbPackline.Validation.ValidateJL_HarmonisedCode();
			AssertHasWarning(dbPackline.JL_HarmonisedCodeInfo, errorMessage);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				consol.JK_RL_NKLoadPort = "JPTYO";
				consol.JK_RL_NKDischargePort = "IDJKT";
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				packLine.JL_HarmonisedCode = "";
				packLine.Validation.ValidateJL_HarmonisedCode();
				AssertNoMessageError("CMD validation is only required for Consols relevant to SG. i.e. SG Import or Export Consol", packLine.JL_HarmonisedCodeInfo, "Harmonized Code is required for SG CMD messaging.");

				consol.JK_RL_NKLoadPort = "SGSIN";
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				packLine.JL_HarmonisedCode = "";
				packLine.Validation.ValidateJL_HarmonisedCode();
				AssertHasMessageError(packLine.JL_HarmonisedCodeInfo, "Harmonized Code is required for SG CMD messaging.");

				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				packLine.JL_HarmonisedCode = "";
				packLine.Validation.ValidateJL_HarmonisedCode();
				AssertNoMessageError(packLine.JL_HarmonisedCodeInfo, "Harmonized Code is required for SG CMD messaging.");

				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				var innerPackLine = shipment.InnerPackLines.AddNew();
				innerPackLine.JL_HarmonisedCode = "";
				innerPackLine.Validation.ValidateJL_HarmonisedCode();
				AssertNoMessageError(innerPackLine.JL_HarmonisedCodeInfo, "Harmonized Code is required for SG CMD messaging.");
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				packLine.JL_HarmonisedCode = "";
				packLine.Validation.ValidateJL_HarmonisedCode();
				AssertNoMessageError(packLine.JL_HarmonisedCodeInfo, "Harmonized Code is required for SG CMD messaging.");
			}
		}

		public void TestCheckJL_HarmonisedCodeForMalaysia()
		{
			var errorMessage = "HS Code is required for exports and imports to/from Malaysia.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "MYABU";

			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "CNSHA";
			transport.JW_RL_NKDiscPort = "MYABU";
			transport.JW_Vessel = "ANRO ASIA";
			transport.JW_VoyageFlight = "324443";
			transport.JW_ETD = new ZDateTime(2019, 12, 1);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "MYABU";
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_HarmonisedCode = string.Empty;
			packLine.JL_PackageCount = 2;
			packLine.JL_F3_NKPackType = "PLT";

			packLine.Validation.ValidateJL_HarmonisedCode();
			AssertHasWarning(packLine.JL_HarmonisedCodeInfo, errorMessage);

			shipment.JS_RL_NKOrigin = "MYABU";
			shipment.JS_RL_NKDestination = "CNSHA";
			packLine.Validation.ValidateJL_HarmonisedCode();
			AssertHasWarning(packLine.JL_HarmonisedCodeInfo, errorMessage);

			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "AUSYD";
			packLine.Validation.ValidateJL_HarmonisedCode();
			AssertNoWarning(packLine.JL_HarmonisedCodeInfo, errorMessage);

			shipment.JS_RL_NKOrigin = "MYABU";
			shipment.JS_RL_NKDestination = "CNSHA";
			packLine.JL_HarmonisedCode = "11111";
			packLine.Validation.ValidateJL_HarmonisedCode();
			AssertNoWarning(packLine.JL_HarmonisedCodeInfo, errorMessage);

			shipment.JS_RL_NKOrigin = "MYABU";
			shipment.JS_RL_NKDestination = "CNSHA";
			packLine.JL_HarmonisedCode = string.Empty;
			var harmonizedCodes = packLine.HarmonisedCodes.AddNew();
			harmonizedCodes.JLH_RN_NKCountry = "MY";
			harmonizedCodes.JLH_Code = "55555";
			packLine.Validation.ValidateJL_HarmonisedCode();
			AssertNoWarning(packLine.JL_HarmonisedCodeInfo, errorMessage);

			harmonizedCodes.JLH_RN_NKCountry = "FR";
			harmonizedCodes.JLH_Code = "55555";
			AssertHasWarning(packLine.JL_HarmonisedCodeInfo, errorMessage);

			harmonizedCodes.JLH_RN_NKCountry = "MY";
			harmonizedCodes.JLH_Code = string.Empty;
			AssertHasWarning(packLine.JL_HarmonisedCodeInfo, errorMessage);

			shipment.JS_RL_NKOrigin = "MYABU";
			shipment.JS_RL_NKDestination = "CNSHA";
			packLine.JL_HarmonisedCode = string.Empty;
			packLine.HarmonisedCodes.DeleteAll();
			packLine.Validation.ValidateJL_HarmonisedCode();
			AssertHasWarning(packLine.JL_HarmonisedCodeInfo, errorMessage);
		}

		public void TestCheckJL_HarmonisedCode_LessThan6Characters()
		{
			HarmonisedCodeHelperTest.LoadOrCreateNewTariff(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, "1000", Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			HarmonisedCodeHelperTest.LoadOrCreateNewTariff(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, "200010", Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			HarmonisedCodeHelperTest.LoadOrCreateNewTariff(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, "200090", Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);

			var warningMessage = "Harmonized Code is less than 6 characters.";

			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_HarmonisedCode = "1000";
			AssertNoWarning("No warning when Tariff is found for 4 characters HS code", packLine.JL_HarmonisedCodeInfo, warningMessage);

			packLine.JL_HarmonisedCode = "2000";
			AssertHasWarning(packLine.JL_HarmonisedCodeInfo, warningMessage);

			packLine.JL_HarmonisedCode = "200090";
			AssertNoWarning(packLine.JL_HarmonisedCodeInfo, warningMessage);

			packLine.JL_HarmonisedCode = "";
			AssertNoWarning(packLine.JL_HarmonisedCodeInfo, warningMessage);
		}

		public void TestCheckJL_HarmonisedCode_WCOTariffList()
		{
			HarmonisedCodeHelperTest.LoadOrCreateNewTariff(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, "1234567", Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			HarmonisedCodeHelperTest.LoadOrCreateNewTariff(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, "12345.", Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			HarmonisedCodeHelperTest.LoadOrCreateNewTariff(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, "123456789123456789", Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);

			var warningMessage = "Harmonized Code is not from WCO tariff list.";
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();

			packLine.JL_HarmonisedCode = "1234566";
			AssertHasWarning(packLine.JL_HarmonisedCodeInfo, warningMessage);

			packLine.JL_HarmonisedCode = "12345.";
			AssertNoWarning("No warning when Tariff is found for 6 characters HS code", packLine.JL_HarmonisedCodeInfo, warningMessage);

			packLine.JL_HarmonisedCode = "123456789123456789";
			AssertNoWarning("No warning when Tariff is found for 18 characters HS code", packLine.JL_HarmonisedCodeInfo, warningMessage);
		}

		#endregion

		public void TestValidateJL_RequiresTemperatureControl_ErrorWhenNotSetWithSpecifiedRange()
		{
			var packline = Factory.New<ForwardingPackLine>();
			packline.JL_RequiredTemperatureMinimum = 0;
			packline.JL_RequiredTemperatureMaximum = 0;
			packline.JL_RequiresTemperatureControl = false;

			AssertNoError(packline.JL_RequiresTemperatureControlInfo, "Is Temperature Control flag should be set when a temperature range is specified.");

			packline.JL_RequiredTemperatureMaximum = 1;
			packline.JL_RequiresTemperatureControl = false;
			AssertHasError(packline.JL_RequiresTemperatureControlInfo, "Is Temperature Control flag should be set when a temperature range is specified.");
		}

		public void TestValidateJL_RequiresTemperatureControl_WarningWhenConsolRequiresTempControlButPackLineDoesNot()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packLine = shipment.OuterPackLines.AddNew();

			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_UniqueConsignRef = "CONSOLA";
			consol1.Shipments.Add(shipment);

			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.JK_UniqueConsignRef = "CONSOLB";
			consol2.Shipments.Add(shipment);

			var warningPrefix = "This shipment is attached to the following temperature controlled Consol(s). Please verify the temperature range of the Consol(s) is suitable for this cargo: ";

			consol1.RequiresTemperatureControl = true;
			consol2.RequiresTemperatureControl = false;
			packLine.RequiresTemperatureControl = false;

			packLine.Validation.ValidateJL_RequiresTemperatureControl();

			AssertHasWarning("PackLine that does not require temperature control but is attached to a temperature controlled consol displays a warning",
				packLine.RequiresTemperatureControlInfo, warningPrefix + "Consol CONSOLA");

			consol2.RequiresTemperatureControl = true;

			packLine.Validation.ValidateJL_RequiresTemperatureControl();

			AssertHasWarning("PackLine that does not require temperature control displays a warning of all temperature controlled consols it's attached to",
				packLine.RequiresTemperatureControlInfo, warningPrefix + "Consol CONSOLA" + System.Environment.NewLine + "Consol CONSOLB");

			packLine.RequiresTemperatureControl = true;

			packLine.Validation.ValidateJL_RequiresTemperatureControl();

			AssertNoWarning("PackLine that requires temperature control attached to a temperature controlled consol displays no warning",
				packLine.RequiresTemperatureControlInfo, warningPrefix + "Consol CONSOLA" + System.Environment.NewLine + "Consol CONSOLB");
		}

		public void TestValidateJL_RequiredTemperatureUnit_AcceptedValues()
		{
			var packline = Factory.New<ForwardingPackLine>();
			var errorNotification = "Temperature must be set to C (Celsius) or F (Fahrenheit)";

			var validTemps = new string[] { Core.Constants.Temperature.Centigrade, Core.Constants.Temperature.Fahrenheit };
			var invalidTemps = new string[] { Core.Constants.Temperature.Kelvin, "A", "Z", ".", "*", "Y" };

			CombineAssertions("No error should display for accepted temperature units", () =>
			{
				foreach (var validTemp in validTemps)
				{
					packline.JL_RequiredTemperatureUnit = validTemp;
					AssertNoError(packline.JL_RequiredTemperatureUnitInfo, errorNotification);
				}
			});

			CombineAssertions("Error should display for invalid temperature units", () =>
			{
				foreach (var invalidTemp in invalidTemps)
				{
					packline.JL_RequiredTemperatureUnit = invalidTemp;
					AssertHasError(packline.JL_RequiredTemperatureUnitInfo, errorNotification);
				}
			});
		}

		public void TestValidateJL_RequiredTemperatureMinimum_Centigrade()
		{
			var packline = Factory.New<ForwardingPackLine>();
			packline.JL_RequiredTemperatureUnit = "C";

			var validDecimals = new ZDecimal[] { -273.1, -273.0, -100, -1.9, 0, 0.1, 123, 999, 999.9 };
			var invalidDecimals = new ZDecimal[] { -999.9, -500, -459.6, -273.2 };
			var errorSuffix = "°C is below the minimum possible temperature of absolute zero (-273.15°C)";

			foreach (var validDecimal in validDecimals)
			{
				packline.JL_RequiredTemperatureMinimum = validDecimal;

				AssertNoErrorContaining(packline.JL_RequiredTemperatureMinimumInfo, errorSuffix);
			}

			foreach (var invalidDecimal in invalidDecimals)
			{
				packline.JL_RequiredTemperatureMinimum = invalidDecimal;

				AssertHasErrorContaining(packline.JL_RequiredTemperatureMinimumInfo, errorSuffix);
			}
		}

		public void TestValidateJL_RequiredTemperatureMinimum_Fahrenheit()
		{
			var packline = Factory.New<ForwardingPackLine>();
			packline.JL_RequiredTemperatureUnit = "F";

			var validDecimals = new ZDecimal[] { -459.6, -459.5, -333, -273.2, -273.1, -100, -1.9, 0, 0.1, 123, 999, 999.9 };
			var invalidDecimalsBelowMinimum = new ZDecimal[] { -999.9, -500, -459.7 };
			var errorSuffix = "°F is below the minimum possible temperature of absolute zero (-459.67°F)";

			foreach (var validDecimal in validDecimals)
			{
				packline.JL_RequiredTemperatureMinimum = validDecimal;

				AssertNoErrorContaining(packline.JL_RequiredTemperatureMinimumInfo, errorSuffix);
			}

			foreach (var invalidDecimal in invalidDecimalsBelowMinimum)
			{
				packline.JL_RequiredTemperatureMinimum = invalidDecimal;

				AssertHasErrorContaining(packline.JL_RequiredTemperatureMinimumInfo, errorSuffix);
			}
		}

		public void TestValidateJL_RequiredTemperatureMinimum_MatchesCommodity()
		{
			var packline = Factory.New<ForwardingPackLine>();
			packline.JL_RequiresTemperatureControl = true;
			packline.JL_RequiredTemperatureMinimum = -10;
			packline.JL_RequiredTemperatureUnit = "C";

			var matchingCommodity = Factory.NewWithValidTestData<RefCommodityCode>();
			matchingCommodity.RH_ReeferMinTemperature = -10;

			var nonMatchingCommodity = Factory.NewWithValidTestData<RefCommodityCode>();
			nonMatchingCommodity.RH_ReeferMinTemperature = -15;

			var warning = "The temperature range entered does not match the temperature range of commodity COM of this pack line";

			matchingCommodity.RH_Code = "COM";
			packline.JL_RH_NKCommodityCode = matchingCommodity.RH_Code;

			AssertNoWarning(packline.JL_RequiredTemperatureMinimumInfo, warning);

			matchingCommodity.RH_Code = "DUM";
			nonMatchingCommodity.RH_Code = "COM";
			packline.JL_RH_NKCommodityCode = nonMatchingCommodity.RH_Code;

			packline.Validation.ValidateJL_RequiredTemperatureMinimum();

			AssertHasWarning(packline.JL_RequiredTemperatureMinimumInfo, warning);
		}

		public void TestValidateJL_RequiredTemperatureMaximum_Centigrade()
		{
			var packline = Factory.New<ForwardingPackLine>();
			packline.JL_RequiredTemperatureUnit = "C";

			var validDecimals = new ZDecimal[] { -273.1, -273.0, -100, -1.9, 0, 0.1, 123, 999, 999.9 };
			var invalidDecimals = new ZDecimal[] { -999.9, -500, -459.6, -273.2 };
			var errorSuffix = "°C is below the minimum possible temperature of absolute zero (-273.15°C)";

			foreach (var validDecimal in validDecimals)
			{
				packline.JL_RequiredTemperatureMaximum = validDecimal;

				AssertNoErrorContaining(packline.JL_RequiredTemperatureMaximumInfo, errorSuffix);
			}

			foreach (var invalidDecimal in invalidDecimals)
			{
				packline.JL_RequiredTemperatureMaximum = invalidDecimal;

				AssertHasErrorContaining(packline.JL_RequiredTemperatureMaximumInfo, errorSuffix);
			}
		}

		public void TestValidateJL_RequiredTemperatureMaximum_Fahrenheit()
		{
			var packline = Factory.New<ForwardingPackLine>();
			packline.JL_RequiredTemperatureUnit = "F";

			var validDecimals = new ZDecimal[] { -459.6, -459.5, -333, -273.2, -273.1, -100, -1.9, 0, 0.1, 123, 999, 999.9 };
			var invalidDecimals = new ZDecimal[] { -999.9, -500, -459.7 };
			var errorSuffix = "°F is below the minimum possible temperature of absolute zero (-459.67°F)";

			foreach (var validDecimal in validDecimals)
			{
				packline.JL_RequiredTemperatureMaximum = validDecimal;

				AssertNoErrorContaining(packline.JL_RequiredTemperatureMaximumInfo, errorSuffix);
			}

			foreach (var invalidDecimal in invalidDecimals)
			{
				packline.JL_RequiredTemperatureMaximum = invalidDecimal;

				AssertHasErrorContaining(packline.JL_RequiredTemperatureMaximumInfo, errorSuffix);
			}
		}

		public void TestValidateJL_RequiredTemperatureMaximum_MatchesCommodity()
		{
			var packline = Factory.New<ForwardingPackLine>();
			packline.JL_RequiresTemperatureControl = true;
			packline.JL_RequiredTemperatureMaximum = 10;
			packline.JL_RequiredTemperatureUnit = "C";

			var matchingCommodity = Factory.NewWithValidTestData<RefCommodityCode>();
			matchingCommodity.RH_ReeferMaxTemperature = 10;

			var nonMatchingCommodity = Factory.NewWithValidTestData<RefCommodityCode>();
			nonMatchingCommodity.RH_ReeferMaxTemperature = 15;

			var warning = "The temperature range entered does not match the temperature range of commodity COM of this pack line";

			matchingCommodity.RH_Code = "COM";
			packline.JL_RH_NKCommodityCode = matchingCommodity.RH_Code;

			AssertNoWarning(packline.JL_RequiredTemperatureMaximumInfo, warning);

			matchingCommodity.RH_Code = "DUM";
			nonMatchingCommodity.RH_Code = "COM";
			packline.JL_RH_NKCommodityCode = nonMatchingCommodity.RH_Code;

			packline.Validation.ValidateJL_RequiredTemperatureMaximum();

			AssertHasWarning(packline.JL_RequiredTemperatureMaximumInfo, warning);
		}

		public void TestValidateJL_RequiredTemperatureMinimumAndMaximum_Comparison()
		{
			var minGreaterThanMaxWarning = "Minimum temperature cannot be higher than maximum temperature";
			var maxGreaterThanMinWarning = "Maximum temperature cannot be lower than minimum temperature";

			var packline = Factory.New<ForwardingPackLine>();
			packline.JL_RequiredTemperatureMinimum = 0;
			packline.JL_RequiredTemperatureMaximum = 0;

			AssertNoError(packline.JL_RequiredTemperatureMinimumInfo, minGreaterThanMaxWarning);
			AssertNoError(packline.JL_RequiredTemperatureMaximumInfo, maxGreaterThanMinWarning);

			packline.JL_RequiredTemperatureMinimum = 1;
			packline.JL_RequiredTemperatureMaximum = 0;

			AssertHasError(packline.JL_RequiredTemperatureMinimumInfo, minGreaterThanMaxWarning);
			AssertHasError(packline.JL_RequiredTemperatureMaximumInfo, maxGreaterThanMinWarning);

			packline.JL_RequiredTemperatureMinimum = 0;
			packline.JL_RequiredTemperatureMaximum = 1;

			AssertNoError(packline.JL_RequiredTemperatureMinimumInfo, minGreaterThanMaxWarning);
			AssertNoError(packline.JL_RequiredTemperatureMaximumInfo, maxGreaterThanMinWarning);

			packline.JL_RequiredTemperatureMinimum = -1.0;
			packline.JL_RequiredTemperatureMaximum = -1.1;

			AssertHasError(packline.JL_RequiredTemperatureMinimumInfo, minGreaterThanMaxWarning);
			AssertHasError(packline.JL_RequiredTemperatureMaximumInfo, maxGreaterThanMinWarning);
		}

		public void TestLooseCargoContainerType()
		{
			var akeContainer = Factory.NewWithValidTestData<RefContainer>();
			akeContainer.RC_Code = "AKE";

			var rknContainer = Factory.NewWithValidTestData<RefContainer>();
			rknContainer.RC_Code = "RKN";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var builder = ObjectFactory.New<IQuotedBookingBuilder>();
			var quotedBooking = builder.InitializeFrom(shipment.PK, Factory);

			var container = (ForwardingContainer)quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_RC = akeContainer.PK;

			var packLine = shipment.OuterPackLines.AddNew();

			packLine.LooseCargoContainerType = "AKE";
			AssertEquals("AKE", packLine.LooseCargoContainerType);
			AssertNoErrors("Line container type set to AKE, there is container AKE, so, no errors", packLine.LooseCargoContainerTypeInfo);

			packLine.LooseCargoContainerType = "XXX";
			AssertEquals("Line container type set to XXX, there are no container XXX, so, the value reset to blank", "", packLine.LooseCargoContainerType);
			AssertNoErrors(packLine.LooseCargoContainerTypeInfo);

			packLine.LooseCargoContainerType = "AKE";
			AssertNoErrors("Line container type set to AKE, there is container AKE, so, no errors", packLine.LooseCargoContainerTypeInfo);

			container.JC_RC = rknContainer.PK;
			shipment.RunPreSaveValidation();
			AssertEquals("AKE", packLine.LooseCargoContainerType);
			AssertHasError("Container type on the container change to RKN, so, the line with AKE container must have an error now", packLine.LooseCargoContainerTypeInfo, "Enter a valid selection.");

			container.JC_RC = akeContainer.PK;
			shipment.RunPreSaveValidation();
			AssertEquals("AKE", packLine.LooseCargoContainerType);
			AssertNoErrors("The container type on the container changed back to AKE, so, no errors expected anymore", packLine.LooseCargoContainerTypeInfo);

			quotedBooking.QuotedBookingContainers.Delete(container);
			shipment.RunPreSaveValidation();
			AssertEquals("AKE", packLine.LooseCargoContainerType);
			AssertHasError("AKE container has been deleted, so, the line with AKE container is not valid anymore", packLine.LooseCargoContainerTypeInfo, "Enter a valid selection.");
		}
	}
}
