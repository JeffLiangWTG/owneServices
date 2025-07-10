using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static CargoWise.EventReference.Constants;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;

namespace Enterprise.Freight.Forwarding.Documents.Testing.FR
{
	sealed class CresaBuilderTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();

			Freight.Business.Testing.FreightTestHelper.TryRemovePackLineIdSequence();
		}

		protected override void TearDown()
		{
			base.TearDown();

			Freight.Business.Testing.FreightTestHelper.TryRemovePackLineIdSequence();
		}

		[TestDate(2020, 7, 3, 10, 15, 59, 124)]
		public void TestHeaderAddressDetails()
		{
			var shipment = CreateShipment(false);

			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();
			AssertNotNull(cresa);

			AssertionHelper.AssertAddressData(shipment.ExportReceivingDepot, cresa.SendingParty);
			AssertionHelper.AssertAddressData(shipment.DocsAndCartage.PickupCartageCoAddr, cresa.Transporter);
			AssertionHelper.AssertAddressData(shipment.PickupAgentDocumentaryAddress, cresa.Agent);
			AssertionHelper.AssertAddressData(shipment.ConsigneeDocumentaryAddress, cresa.Buyer);
			AssertionHelper.AssertAddressData(shipment.ConsignorDocumentaryAddress, cresa.Supplier);
			AssertionHelper.AssertAddressData(GlbBranch.CurrentBranch.OrgProxy.MainAddress, cresa.SendingForwarder);
		}

		public void TestHeaderPopulateAPPlusCodes()
		{
			var shipment = CreateShipment(false);

			AssertionHelper.AssertPopulateAPPlusCodes(() => new CresaBuilder(shipment).Build(), shipment.ExportReceivingDepot, OrgCusCode.FranceCodeTypes.SOW, "SendingPartySOW", "SendingPartyCI5", "FormattedSendingPartyProviderIDInfo");
			AssertionHelper.AssertPopulateAPPlusCodes(() => new CresaBuilder(shipment).Build(), shipment.DocsAndCartage.PickupCartageCoAddr, OrgCusCode.FranceCodeTypes.SON, "TransporterSON", "TransporterCI5");
			AssertionHelper.AssertPopulateAPPlusCodes(() => new CresaBuilder(shipment).Build(), shipment.PickupAgentDocumentaryAddress.Address, OrgCusCode.FranceCodeTypes.SOA, "AgentSOA", "AgentCI5", "FormattedAgentProviderIDInfo");
			AssertionHelper.AssertPopulateAPPlusCodes(() => new CresaBuilder(shipment).Build(), shipment.ConsigneeDocumentaryAddress.Address, OrgCusCode.FranceCodeTypes.SON, "BuyerSON", "BuyerCI5");
			AssertionHelper.AssertPopulateAPPlusCodes(() => new CresaBuilder(shipment).Build(), shipment.ConsignorDocumentaryAddress.Address, OrgCusCode.FranceCodeTypes.SON, "SupplierSON", "SupplierCI5");
			AssertionHelper.AssertPopulateAPPlusCodes(() => new CresaBuilder(shipment).Build(), GlbBranch.CurrentBranch.OrgProxy.MainAddress, OrgCusCode.FranceCodeTypes.SON, "SendingForwarderSON", "SendingForwarderCI5", "FormattedSendingForwarderProviderIDInfo");

			AssertPopulateAPPlusCodesUseRegistry("AgentSOA", "AgentCI5", false);
			AssertPopulateAPPlusCodesUseRegistry("SendingForwarderSON", "SendingForwarderCI5", true);
		}

		void AssertPopulateAPPlusCodesUseRegistry(string sPropertyName, string ci5PropertyName, bool isForwarderCode)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var build = new CresaBuilder(shipment);

			var operationalAddress = Factory.NewWithValidTestData<OrgAddress>();
			shipment.JS_OA_ExportReceivingDepot = operationalAddress.PK;

			var query = new ZQuery(RefUNLOCOSchema.RL_Code, "FRFR2");
			var unloco = Factory.LoadTop1<RefUNLOCO>(query);
			if (unloco == null)
			{
				unloco = Factory.NewWithValidTestData<RefUNLOCO>();
				unloco.Code = "FRFR2";
			}

			operationalAddress.OA_RL_NKRelatedPortCode = unloco.Code;

			var registryCodes = new Freight.Business.CommunitySystemCodesOfForwarderAndAgentCollection();
			var code = registryCodes.AddNew();
			code.AgentCode = "AGENT";
			code.ForwarderCode = "FORWARDER";
			code.Port = unloco.Code;
			code.PCS = FrenchPortSystemCodeList.Codes.MGI;

			using (Freight.Business.PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryCodes))
			{
				AssertEquals($"{ci5PropertyName} should be from registry", isForwarderCode ? "FORWARDER" : "AGENT", ((RegistrationNumber)build.Build()[ci5PropertyName]).Value);
			}

			code.PCS = FrenchPortSystemCodeList.Codes.SOGET;

			using (Freight.Business.PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryCodes))
			{
				AssertEquals($"{sPropertyName} should be from registry", isForwarderCode ? "FORWARDER" : "AGENT", ((RegistrationNumber)build.Build()[sPropertyName]).Value);
			}
		}

		public void TestHeaderTransportDetails()
		{
			var shipment = CreateShipment(false);

			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();
			AssertNotNull(cresa);

			AssertEquals("PortOfTranshipment.Code", "FRMRS", cresa.PortOfTranshipment.Code);
			AssertEquals("PortOfArrival.Code", "FRNCE", cresa.PortOfArrival.Code);
			AssertEquals("ETA", ZDate.Today.AddDays(6), cresa.ETA);
			AssertEquals("TransportMode", "RTE", cresa.TransportMode);

			AssertNoMessageErrors("Port Of Transhipment mandatory", ((Unloco)cresa.PortOfTranshipment).CodeInfo);
			AssertNoMessageErrors("Port Of Arrival mandatory", ((Unloco)cresa.PortOfTranshipment).CodeInfo);
		}

		[TestDate(2021, 06, 11, 10, 0, 0)]
		public void TestHeaderTransportDetailsStandAloneShipment()
		{
			var shipment = CreateShipment(true);

			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();
			AssertNotNull(cresa);

			AssertEquals("PortOfTranshipment.Code", "FRMRS", cresa.PortOfTranshipment.Code);
			AssertEquals("PortOfArrival.Code", "AUMEL", cresa.PortOfArrival.Code);
			AssertEquals("ETA", ZDate.Today.AddDays(5), cresa.ETA);
			AssertEquals("TransportMode", "RTE", cresa.TransportMode);

			AssertNoMessageErrors("Port Of Transhipment is required no errors", ((Unloco)cresa.PortOfTranshipment).CodeInfo);
			AssertNoMessageErrors("Port Of Arrival is required no errors", ((Unloco)cresa.PortOfArrival).CodeInfo);

			shipment.JS_RL_NKDischargePort = ZString.Empty;
			shipment.JS_RL_NKDestination = "FRNCE";
			shipment.JS_E_ARV = ZDateTime.Empty;
			cresa = builder.Build();
			AssertEquals("PortOfTranshipment.Code", "FRNCE", cresa.PortOfTranshipment.Code);
			AssertEquals("ETA", String.Format("{0:dd/MM/yyyy HH:mm:ss}", ZDateTime.Now), String.Format("{0:dd/MM/yyyy HH:mm:ss}", cresa.ETA));

			AssertNoMessageErrors("Port Of Transhipment is required no errors", ((Unloco)cresa.PortOfTranshipment).CodeInfo);
			AssertNoMessageErrors("Port Of Arrival is required no errors", ((Unloco)cresa.PortOfArrival).CodeInfo);

			shipment.JS_RL_NKDischargePort = ZString.Empty;
			shipment.JS_RL_NKDestination = ZString.Empty;
			cresa = builder.Build();

			AssertHasMessageError("Port Of Transhipment is required", ((Unloco)cresa.PortOfTranshipment).CodeInfo, "Port of Transhipment is required.");
			AssertHasMessageError("Port Of Arrival is required", ((Unloco)cresa.PortOfArrival).CodeInfo, "Port of Arrival is required.");
		}
		public void TestHeaderTransportValidationTransportMode()
		{
			var shipment = CreateShipment(false);

			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();

			AssertNoMessageErrors("Transport Mode should not have error message", cresa.TransportModeInfo);

			cresa.TransportMode = "ACDE";
			cresa.ValidateAllIncludingChildren();

			AssertHasMessageError("Transport Mode should have error message", cresa.TransportModeInfo, "Transport Mode must not exceed three (3) characters.");

			cresa.TransportMode = ZString.Empty;
			cresa.ValidateAllIncludingChildren();

			AssertHasMessageError("Transport Mode should have error message", cresa.TransportModeInfo, "Transport mode is required.");
		}
		public void TestHeaderTransportValidationPortOfArrival()
		{
			var shipment = CreateShipment(false);

			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();

			var portOfArrivalErrorMessage = "Port of Arrival is required.";
			AssertNoMessageError("Port of Arrival should not have error message", ((Unloco)cresa.PortOfArrival).CodeInfo, portOfArrivalErrorMessage);
			((Unloco)cresa.PortOfArrival).Code = "";
			cresa.ValidateAllIncludingChildren();

			AssertHasMessageError("Port of Arrival should have error message", ((Unloco)cresa.PortOfArrival).CodeInfo, portOfArrivalErrorMessage);
		}

		public void TestHeaderPortServiceInformation()
		{
			var shipment = CreateShipment(false);

			var exportReceivingDepot = shipment.ExportReceivingDepot;
			var psnCode = exportReceivingDepot.Header.CustomsCodes.AddNew();
			psnCode.OK_CodeType = OrgCusCode.CodeTypes.PortSystemNumber;
			psnCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Reunion;
			psnCode.OK_CustomsRegNo = "Area\\Location";

			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();
			AssertNotNull(cresa);

			AssertEquals("PortServiceCodeReference", "", cresa.PortServiceCodeReference);
			AssertEquals("PortLocation", "Location", cresa.PortLocation);
			AssertEquals("PortArea", "Area", cresa.PortArea);

			AssertHasMessageError("Port Service Code mandatory", cresa.PortServiceCodeReferenceInfo, "Port Service Reference is missing from Organization Shipment > Pickup > CFS > Config > Registration Numbers/Codes [Type=PSR].");

			var psrCode = exportReceivingDepot.Header.CustomsCodes.AddNew();
			psrCode.OK_CodeType = OrgCusCode.CodeTypes.PortServiceReference;
			psrCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Reunion;
			psrCode.OK_CustomsRegNo = "PortServRef";

			cresa = builder.Build();
			AssertEquals("PortServiceCodeReference", "PortServRef", cresa.PortServiceCodeReference);
			AssertNoMessageErrors("Port Service Code mandatory", cresa.PortServiceCodeReferenceInfo);
		}

		public void TestHeaderPortServiceInformationValidationPortArea()
		{
			var shipment = CreateShipment(false);

			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();

			AssertHasMessageErrorContaining("Port Area is required", cresa.PortAreaInfo, "Port Area is missing from Organization Shipment > Pickup > CFS > Config > Registration Numbers/Codes [Type=PSN] (The code before '\\').");
			AssertHasMessageErrorContaining("Port Location is required.", cresa.PortLocationInfo, "Port Location is missing from Organization Shipment > Pickup > CFS > Config > Registration Numbers/Codes [Type=PSN] (The code after '\\').");

			cresa.PortArea = "Area";
			cresa.PortLocation = "Location";
			cresa.ValidateAllIncludingChildren();

			AssertNoMessageErrors("Port Area should not have error message", cresa.PortAreaInfo);
			AssertNoMessageErrors("Port Location should not have error message", cresa.PortLocationInfo);
		}

		public void TestAdditionalReferences()
		{
			var shipment = CreateShipment(false);

			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();
			AssertNotNull(cresa);

			AssertEquals("CarrierBookingReference", "SH0001000", cresa.CarrierBookingReference);
		}

		public void TestGoodsDetails()
		{
			var shipment = CreateShipment(false);

			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();
			AssertNotNull(cresa);

			AssertEquals("ShipmentNumber", shipment.JS_UniqueConsignRef, cresa.ShipmentNumber);
			AssertEquals(nameof(Cresa.GoodsInDateTimeInfo), false, cresa.GoodsInDateTime.IsEmpty);

			AssertEquals("TotalPackCount", 15, cresa.TotalPackCount);
			AssertEquals("PackType", "PLT", cresa.PackType.Code);
			AssertEquals("TotalWeight", 1500m, cresa.TotalWeight.Value);
			AssertEquals("TotalWeight Unit", "KG", cresa.TotalWeight.Unit.Code);
			AssertEquals("TotalVolume", 4875m, cresa.TotalVolume.Value);
			AssertEquals("TotalVolume Unit", "M3", cresa.TotalVolume.Unit.Code);
			AssertEquals("Goods Description", "goods description", cresa.GoodsDescription);

			var packlinex = shipment.OuterPackLines.AddNew();
			packlinex.JL_PackageCount = 4;
			packlinex.JL_F3_NKPackType = "PLT";
			packlinex.JL_ActualWeight = 400;
			packlinex.JL_ActualWeightUQ = "KG";
			packlinex.JL_ActualVolume = 300;
			packlinex.JL_ActualVolumeUQ = "M3";
			packlinex.JL_Length = 1000;
			packlinex.JL_Width = 1000;
			packlinex.JL_Height = 300;
			packlinex.JL_UnitOfDimension = "CM";
			packlinex.JL_HarmonisedCode = "WHISKY";
			packlinex.JL_RefNumber = "AMR-57";
			packlinex.JL_ExportRefNumber = "AMRUT57%";
			packlinex.JL_DetailedDescription = "Amrut Indian Peated Single Malt Detailed Description";
			packlinex.JL_MarksAndNumbers = "MarksAndNumbers1";
			packlinex.JL_Description = "ALCOHOLIC BEVERAGES 57%";
			packlinex.JL_LastKnownTransitWarehouseStatus = ZString.Empty;

			cresa = builder.Build();
			AssertNotNull(cresa);

			AssertEquals("ShipmentNumber", shipment.JS_UniqueConsignRef, cresa.ShipmentNumber);
			AssertEquals(nameof(Cresa.GoodsInDateTimeInfo), false, cresa.GoodsInDateTime.IsEmpty);

			AssertEquals("TotalPackCount with packline without TW status", 15, cresa.TotalPackCount);
			AssertEquals("PackType with packline without TW status", "PLT", cresa.PackType.Code);
			AssertEquals("TotalWeight with packline without TW status", 1500m, cresa.TotalWeight.Value);
			AssertEquals("TotalWeight Unit with packline without TW status", "KG", cresa.TotalWeight.Unit.Code);
			AssertEquals("TotalVolume with packline without TW status", 4875m, cresa.TotalVolume.Value);
			AssertEquals("TotalVolume Unit with packline without TW status", "M3", cresa.TotalVolume.Unit.Code);
			AssertEquals("Goods Description with packline without TW status", "goods description", cresa.GoodsDescription);

			shipment.JS_GoodsDescription = ZString.Empty;
			shipment.DetailedGoodsDescriptionNoteText = "Detailed Goods Description Note Text";
			cresa = builder.Build();

			AssertEquals("Goods Description from Detailed Goods Description Note Text", "Detailed Goods Description Note Text", cresa.GoodsDescription);
		}

		public void TestGoodsDetailsValidationGoodsInDateTime()
		{
			var shipment = CreateShipment(false);

			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();

			var goodsInDateTimeErrorMessage = "Date/Time goods arrived at the warehouse is required";

			AssertNoMessageError("Goods In DateTime should not have error message", cresa.GoodsInDateTimeInfo, goodsInDateTimeErrorMessage);
			cresa.GoodsInDateTime = ZDateTime.Empty;
			cresa.ValidateAllIncludingChildren();

			AssertHasMessageError("Goods In DateTime should have error message", cresa.GoodsInDateTimeInfo, goodsInDateTimeErrorMessage);
		}
		public void TestGoodsDetailsValidationShipmentNumber()
		{
			var shipment = CreateShipment(false);

			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();

			var shipmentNumberErrorMessage = "Shipment Number (which is used as a Document Reference for this message) is too long.  Maximum 17 characters are allowed.";

			AssertNoMessageError("Shipment Number should not have error message", cresa.ShipmentNumberInfo, shipmentNumberErrorMessage);
			cresa.ShipmentNumber = "01234567890123456789";
			cresa.ValidateAllIncludingChildren();

			AssertHasMessageError("Shipment Number should have error message", cresa.ShipmentNumberInfo, shipmentNumberErrorMessage);
		}

		public void TestGoodsDetailsValidationTotalVolume()
		{
			var shipment = CreateShipment(false);

			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();

			var totalVolumeErrorMessage = "Total Volume is required.";
			AssertNoMessageError("Total volume should not have error message", ((Measurement)cresa.TotalVolume).ValueInfo, totalVolumeErrorMessage);
			cresa.TotalVolume.Value = 0m;
			cresa.ValidateAllIncludingChildren();

			AssertHasMessageError("Total volume should have error message", ((Measurement)cresa.TotalVolume).ValueInfo, totalVolumeErrorMessage);
		}
		public void TestGoodsDetailsValidationTotalWeight()
		{
			var shipment = CreateShipment(false);

			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();

			var totalWeightErrorMessage = "Weight cannot be zero.\r\nIt is either entered as zero or rounded to the nearest kilogram value to zero. The CI5 and S)One systems supports only integer value of weight. Please review the pack line weight and input a valid weight.";
			AssertNoMessageError("Total weight should not have error message, sum of actual weight is 1500", ((Measurement)cresa.TotalWeight).ValueInfo, totalWeightErrorMessage);
			AssertEquals("TotalWeight, sum of actual weight is 1500", 1500m, cresa.TotalWeight.Value);

			foreach (ForwardingPackLine packingLine in shipment.OuterPackLines)
			{
				packingLine.JL_ActualWeight = 0.1;
			}
			cresa = builder.Build();

			AssertHasMessageError("Total weight should have error message, sum of actual weight is 0.4", ((Measurement)cresa.TotalWeight).ValueInfo, totalWeightErrorMessage);
			AssertEquals("TotalWeight, sum of actual weight is 0.4", 0m, cresa.TotalWeight.Value);

			shipment.OuterPackLines[0].JL_ActualWeight = 0.2;
			cresa = builder.Build();

			AssertNoMessageError("Total weight should not have error message, sum of actual weight is 0.5", ((Measurement)cresa.TotalWeight).ValueInfo, totalWeightErrorMessage);
			AssertEquals("TotalWeight, sum of actual weight is 0.5", 1m, cresa.TotalWeight.Value);
		}
		public void TestGoodsDetailsValidationTotalPack()
		{
			var shipment = CreateShipment(false);

			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();

			var totalPackCountErrorMessage = "Total number of packs is required.";
			AssertNoMessageError("Total Pack count should not have error message", cresa.TotalPackCountInfo, totalPackCountErrorMessage);
			cresa.TotalPackCount = 0;
			cresa.ValidateAllIncludingChildren();

			AssertHasMessageError("Total Pack count should have error message", cresa.TotalPackCountInfo, totalPackCountErrorMessage);
		}

		public void TestGoodsDetailsValidationPackType()
		{
			var shipment = CreateShipment(false);

			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();

			var packTypeErrorMessage = "Pack Type is required.";
			AssertNoMessageError("Pack Type should not have error message", ((CodeDescription)cresa.PackType).CodeInfo, packTypeErrorMessage);
			cresa.PackType.Code = "";
			cresa.ValidateAllIncludingChildren();

			AssertHasMessageError("Pack Type should have error message", ((CodeDescription)cresa.PackType).CodeInfo, packTypeErrorMessage);
		}

		public void TestGoodsDetailsValidationGoodsDescription()
		{
			var shipment = CreateShipment(false);

			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();

			var goodsDescriptionErrorMessage = "Goods Description is required";
			AssertNoMessageError("Goods Description should not have error message", cresa.GoodsDescriptionInfo, goodsDescriptionErrorMessage);
			cresa.GoodsDescription = "";
			cresa.ValidateAllIncludingChildren();

			AssertHasMessageError("Goods Description should have error message", cresa.GoodsDescriptionInfo, goodsDescriptionErrorMessage);
		}

		public void TestPackingLines()
		{
			var shipment = CreateShipment(false);

			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();
			AssertNotNull(cresa);

			AssertEquals("PackingLines.Count", 4, cresa.PackingLines.Count);

			//Packing Line 1
			AssertEquals("ID (good 1)", "EDIDAT00000001", cresa.PackingLines.ElementAt(0).PackingLineID);
			AssertEquals("ReferenceNumber (good 1)", "AMR-57", cresa.PackingLines.ElementAt(0).ReferenceNumber);
			AssertEquals("Packs (good 1)", 4, cresa.PackingLines.ElementAt(0).Quantity);
			AssertEquals("Weight (good 1)", 400m, cresa.PackingLines.ElementAt(0).Weight.Value);
			AssertEquals("Weight UM (good 1)", "KG", cresa.PackingLines.ElementAt(0).Weight.Unit.Code);
			AssertEquals("Volume (good 1)", 1200m, cresa.PackingLines.ElementAt(0).Volume.Value);
			AssertEquals("Volume UM (good 1)", "M3", cresa.PackingLines.ElementAt(0).Volume.Unit.Code);
			AssertEquals("Length (good 1)", 1000m, cresa.PackingLines.ElementAt(0).Length.Value);
			AssertEquals("Width (good 1)", 1000m, cresa.PackingLines.ElementAt(0).Width.Value);
			AssertEquals("Height (good 1)", 300m, cresa.PackingLines.ElementAt(0).Height.Value);
			AssertEquals("Description (good 1)", "Amrut Indian Peated Single Malt Detailed Description", cresa.PackingLines.ElementAt(0).GoodsDescription);
			AssertEquals("Marks And Numbers (good 1)", "MarksAndNumbers1", cresa.PackingLines.ElementAt(0).MarksAndNumbers);

			//Packing Line 2
			AssertEquals("ID (good 2)", "EDIDAT00000002", cresa.PackingLines.ElementAt(1).PackingLineID);
			AssertEquals("ReferenceNumber (good 2)", "AMR-43", cresa.PackingLines.ElementAt(1).ReferenceNumber);
			AssertEquals("Packs (good 2)", 6, cresa.PackingLines.ElementAt(1).Quantity);
			AssertEquals("Weight (good 2)", 600m, cresa.PackingLines.ElementAt(1).Weight.Value);
			AssertEquals("Weight UM (good 2)", "KG", cresa.PackingLines.ElementAt(1).Weight.Unit.Code);
			AssertEquals("Volume (good 2)", 2700m, cresa.PackingLines.ElementAt(1).Volume.Value);
			AssertEquals("Volume UM (good 2)", "M3", cresa.PackingLines.ElementAt(1).Volume.Unit.Code);
			AssertEquals("Length (good 2)", 1500m, cresa.PackingLines.ElementAt(1).Length.Value);
			AssertEquals("Width (good 2)", 1000m, cresa.PackingLines.ElementAt(1).Width.Value);
			AssertEquals("Height (good 2)", 300m, cresa.PackingLines.ElementAt(1).Height.Value);
			AssertEquals("Description (good 2)", "Amrut Indian Chill Filter Single Malt Detailed Description", cresa.PackingLines.ElementAt(1).GoodsDescription);
			AssertEquals("Marks And Numbers (good 2)", "MarksAndNumbers2", cresa.PackingLines.ElementAt(1).MarksAndNumbers);

			//Packing Line 3
			AssertEquals("ID (good 3)", "EDIDAT00000003", cresa.PackingLines.ElementAt(2).PackingLineID);
			AssertEquals("ReferenceNumber (good 3)", "AMR-50", cresa.PackingLines.ElementAt(2).ReferenceNumber);
			AssertEquals("Packs (good 3)", 3, cresa.PackingLines.ElementAt(2).Quantity);
			AssertEquals("Weight (good 3)", 300m, cresa.PackingLines.ElementAt(2).Weight.Value);
			AssertEquals("Weight UM (good 3)", "KG", cresa.PackingLines.ElementAt(2).Weight.Unit.Code);
			AssertEquals("Volume (good 3)", 675m, cresa.PackingLines.ElementAt(2).Volume.Value);
			AssertEquals("Volume UM (good 3)", "M3", cresa.PackingLines.ElementAt(2).Volume.Unit.Code);
			AssertEquals("Length (good 3)", 1500m, cresa.PackingLines.ElementAt(2).Length.Value);
			AssertEquals("Width (good 3)", 500m, cresa.PackingLines.ElementAt(2).Width.Value);
			AssertEquals("Height (good 3)", 300m, cresa.PackingLines.ElementAt(2).Height.Value);
			AssertEquals("Description (good 3)", "ALCOHOLIC BEVERAGES 50%", cresa.PackingLines.ElementAt(2).GoodsDescription);
			AssertEquals("Marks And Numbers (good 3)", "MarksAndNumbers3", cresa.PackingLines.ElementAt(2).MarksAndNumbers);

			//Packing Line 4
			AssertEquals("ID (good 4)", "EDIDAT00000004", cresa.PackingLines.ElementAt(3).PackingLineID);
			AssertEquals("ReferenceNumber (good 4)", "AMR-64", cresa.PackingLines.ElementAt(3).ReferenceNumber);
			AssertEquals("Packs (good 4)", 2, cresa.PackingLines.ElementAt(3).Quantity);
			AssertEquals("Weight (good 4)", 200m, cresa.PackingLines.ElementAt(3).Weight.Value);
			AssertEquals("Weight UM (good 4)", "KG", cresa.PackingLines.ElementAt(3).Weight.Unit.Code);
			AssertEquals("Volume (good 4)", 300m, cresa.PackingLines.ElementAt(3).Volume.Value);
			AssertEquals("Volume UM (good 4)", "M3", cresa.PackingLines.ElementAt(3).Volume.Unit.Code);
			AssertEquals("Length (good 4)", 1000m, cresa.PackingLines.ElementAt(3).Length.Value);
			AssertEquals("Width (good 4)", 500m, cresa.PackingLines.ElementAt(3).Width.Value);
			AssertEquals("Height (good 4)", 300m, cresa.PackingLines.ElementAt(3).Height.Value);
			AssertEquals("Description (good 4)", "goods description", cresa.PackingLines.ElementAt(3).GoodsDescription);
			AssertEquals("Marks And Numbers (good 4)", "marks & numbers", cresa.PackingLines.ElementAt(3).MarksAndNumbers);

			AssertNoMessageErrors("No error because last known transit warehouse status is filled in.", cresa.ErrorPlaceHolderInfo);

			foreach (ForwardingPackLine packingLine in shipment.OuterPackLines)
			{
				packingLine.JL_PackageCount = 0;
			}
			cresa = builder.Build();
			AssertNotNull(cresa);
			AssertHasMessageError("Quantity required Error", cresa.PackingLines.ElementAt(0).QuantityInfo, "Number of Packs is required.");
			AssertHasMessageError("Quantity required Error", cresa.PackingLines.ElementAt(1).QuantityInfo, "Number of Packs is required.");
			AssertHasMessageError("Quantity required Error", cresa.PackingLines.ElementAt(2).QuantityInfo, "Number of Packs is required.");
			AssertHasMessageError("Quantity required Error", cresa.PackingLines.ElementAt(3).QuantityInfo, "Number of Packs is required.");

			shipment.OuterPackLines[0].JL_LastKnownTransitWarehouseStatus = ZString.Empty;
			cresa = builder.Build();
			AssertNotNull(cresa);
			AssertHasWarning("Warning because there are packing lines without a last known transit warehouse status but also where it is filled in.", cresa.ErrorPlaceHolderInfo, "There are pack lines in the shipment without the Last Know Transit Warehouse Status value.  These pack lines will not be sent.");

			foreach (ForwardingPackLine packingLine in shipment.OuterPackLines)
			{
				packingLine.JL_LastKnownTransitWarehouseStatus = ZString.Empty;
			}
			cresa = builder.Build();
			AssertNotNull(cresa);

			AssertHasMessageError("Error because last known transit warehouse status is not filled in.", cresa.ErrorPlaceHolderInfo, "There are no pack lines in the shipment or pack lines in the shipment are without the Last Know Transit Warehouse Status value. The Goods Received (CRESA) message cannot be sent.");
		}

		public void TestPacklineValidation()
		{
			const string expectedError = "ECV Reference is required. Export Ref Number missing from Shipment > Packing > Pack Line.";

			var shipment = CreateShipment(false);
			shipment.OuterPackLines[0].JL_ExportRefNumber = "";
			shipment.OuterPackLines[1].JL_ExportRefNumber = "12345";

			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();

			AssertHasMessageError(cresa.PackingLines.First(x => x.ExportReferenceNumber.IsEmpty).ExportReferenceNumberInfo, expectedError);
			AssertNoMessageError(cresa.PackingLines.First(x => x.ExportReferenceNumber == "12345").ExportReferenceNumberInfo, expectedError);
			AssertNoMessageErrors(cresa.PackingLines.ElementAt(0).PackingLineIDInfo);

			cresa.PackingLines.ElementAt(0).PackingLineID = "";
			cresa.PackingLines.ElementAt(1).PackingLineID = "HYEUAT000000012861";
			cresa.PackingLines.ElementAt(2).PackingLineID = "HYEUAT00012861";

			cresa.ValidateAllIncludingChildren();

			AssertHasMessageError("PackLine 0 ID", cresa.PackingLines.ElementAt(0).PackingLineIDInfo, "Pack Line ID is required.");
			AssertHasMessageError("PackLine 1 ID", cresa.PackingLines.ElementAt(1).PackingLineIDInfo, "Packing Line ID is too long. Maximum 17 characters allowed.");
			AssertNoMessageError("PackLine 2 ID", cresa.PackingLines.ElementAt(2).PackingLineIDInfo, "Packing Line ID is too long. Maximum 17 characters allowed.");
		}

		public void TestGoodsReceiptNotes()
		{
			var shipment = CreateShipment(false);

			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();
			AssertNotNull(cresa);

			AssertEquals("DeliveryOrderReceiptNote", "Delivery Order Receipt Note", cresa.GoodsReceiptNotes);
		}

		public void TestCargoReceiptDate()
		{
			var shipment = CreateShipment(false);

			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();
			var cargoReceiptDateErrorMessage = "Cargo Receipt Date is required.";

			AssertNotNull(cresa);
			AssertNoMessageError("Cargo Receipt Date should not have error message", cresa.CargoReceiptDateInfo, cargoReceiptDateErrorMessage);
			AssertEquals("CargoReceiptDate", ZDateTime.Now.ToLongTimeString(), cresa.CargoReceiptDate.ToLongTimeString());

			cresa.CargoReceiptDate = ZDateTime.Empty;
			AssertEquals("Cargo Receipt Date should not have a default value", ZDateTime.Empty, cresa.CargoReceiptDate);
			AssertHasMessageError("Cargo Receipt Date should have error message if empty", cresa.CargoReceiptDateInfo, cargoReceiptDateErrorMessage);

			cresa.CargoReceiptDate = new ZDateTime(2020, 12, 12, 6, 6, 0);
			cresa.ValidateAllIncludingChildren();
			AssertEquals("CargoReceiptDate", new ZDateTime(2020, 12, 12, 6, 6, 0), cresa.CargoReceiptDate);
			AssertNoMessageError("Cargo Receipt Date should not have error message if populated", cresa.CargoReceiptDateInfo, cargoReceiptDateErrorMessage);
		}

		public void TestPCSAndOperationalPortWithFallback()
		{
			var shipment = CreateShipment(true);
			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();
			AssertEquals("PCS", FrenchPortsConstants.PCS.MGI, cresa.PCS);
			AssertEquals("OperationalPort", "FRPAR", cresa.OperationalPort.Code);

			shipment.ExportReceivingDepot.Delete();
			cresa = builder.Build();
			AssertEquals("PCS", ZString.Empty, cresa.PCS);
			AssertHasMessageError("PCS Error", cresa.PCSInfo, "PCS Code is required. (Maintain > Locations > UNLOCO of Operational Port > Local Codes - Usage PCS)");
			AssertHasMessageError("OperationalPort Error", ((Unloco)cresa.OperationalPort)?.CodeInfo, "Operational Port is required, UNLOCO missing from Shipment > Pickup > CFS.");
		}

		public void TestAddressValidations()
		{
			var shipment = CreateShipment(true);
			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();

			AssertNoMessageError(cresa.SendingParty.CompanyNameInfo, "Sending Party name and address is required.");
			AssertNoMessageError(cresa.Agent.CompanyNameInfo, "Agent name and address is required.");
			AssertNoMessageError(cresa.SendingForwarder.CompanyNameInfo, "Forwarder name and address is required.");

			cresa.SendingParty.CompanyName = "";
			cresa.SendingParty.Country.Code = "";
			cresa.SendingParty.AddressLine1 = "";

			cresa.Agent.CompanyName = "";
			cresa.Agent.Country.Code = "";
			cresa.Agent.AddressLine1 = "";

			cresa.SendingForwarder.CompanyName = "";
			cresa.SendingForwarder.Country.Code = "";
			cresa.SendingForwarder.AddressLine1 = "";

			AssertHasMessageError(cresa.SendingParty.CompanyNameInfo, "Sending Party name and address is required.");
			AssertHasMessageError(cresa.Agent.CompanyNameInfo, "Agent name and address is required.");
			AssertHasMessageError(cresa.SendingForwarder.CompanyNameInfo, "Forwarder name and address is required.");
		}
		public void TestGeneralFields()
		{
			var shipment = CreateShipment(false);
			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();

			AssertEquals("Warehouse Entry Number", shipment.JS_UniqueConsignRef, cresa.EntryNumber);
			AssertEquals("Commodity Reference", shipment.JS_UniqueConsignRef, cresa.CommodityReference);

			AssertNoMessageErrors("Commodiy Reference no message errors", cresa.CommodityReferenceInfo);

			cresa.CommodityReference = "1717171717171717171717171717171717";
			cresa.ValidateAllIncludingChildren();

			AssertHasMessageError("Commodiy Reference max length error", cresa.CommodityReferenceInfo, "Commodity Reference is too long. Maximum 17 characters allowed.");

			cresa.CommodityReference = "";
			cresa.ValidateAllIncludingChildren();

			AssertHasMessageError("Commodiy Reference required error", cresa.CommodityReferenceInfo, "Commodity Reference is required.");
		}

		public void TestEventReferences()
		{
			var shipment = CreateShipment(false);
			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();

			AssertEquals("CRESA Reference should be empty when no messages have been sent yet", ZString.Empty, cresa.CRESAReference);
			AssertNoMessageErrors("CRESA no message errors when no messages have been sent yet", cresa.CRESAReferenceInfo);

			CreateLog(shipment,
				Events.MessageSent,
				new ZDateTime(2020, 02, 01),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA));
			cresa = builder.Build();
			AssertEquals("CRESA Reference should be empty when the request was sent", ZString.Empty, cresa.CRESAReference);
			AssertNoMessageErrors("CRESA no message errors when the request was sent", cresa.CRESAReferenceInfo);

			CreateLog(shipment,
				Events.InterchangeSent,
				new ZDateTime(2020, 02, 02),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA));
			cresa = builder.Build();
			AssertEquals("CRESA Reference should still be empty when the interchange message was sent", ZString.Empty, cresa.CRESAReference);
			AssertNoMessageErrors("CRESA no message errors when the interchange message was sent", cresa.CRESAReferenceInfo);

			CreateLog(shipment,
				Events.MessageAccepted,
				new ZDateTime(2020, 02, 03),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, "CRESAREF123"));
			cresa = builder.Build();
			AssertEquals("CRESA Reference should be filled in", "CRESAREF123", cresa.CRESAReference);
			AssertNoMessageErrors("CRESA no message errors when the message was accepted", cresa.CRESAReferenceInfo);

			CreateLog(shipment,
				Events.MessageWithdrawCancelRequest,
				new ZDateTime(2020, 02, 04),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, "CRESAREF123"));
			cresa = builder.Build();
			AssertEquals("CRESA Reference should still be filled in after sending WithdrawCancelRequest", "CRESAREF123", cresa.CRESAReference);
			AssertNoMessageErrors("CRESA no message errors after sending WithdrawCancelRequest", cresa.CRESAReferenceInfo);
		}

		public void TestEventReferencesWithdrawCancelAccepted()
		{
			var shipment = CreateShipment(false);
			var builder = new CresaBuilder(shipment);

			CreateLog(shipment,
				Events.MessageSent,
				new ZDateTime(2020, 02, 01),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA));

			CreateLog(shipment,
				Events.InterchangeSent,
				new ZDateTime(2020, 02, 02),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA));

			CreateLog(shipment,
				Events.MessageAccepted,
				new ZDateTime(2020, 02, 03),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, "CRESAREF123"));

			CreateLog(shipment,
				Events.MessageWithdrawCancelRequest,
				new ZDateTime(2020, 02, 04),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, "CRESAREF123"));

			CreateLog(shipment,
				Events.MessageWithdrawCancelAccepted,
				new ZDateTime(2020, 02, 05),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, "CRESAREF123"));
			var cresa = builder.Build();
			AssertEquals("CRESA Reference should be empty when Message Cancellation accepted", ZString.Empty, cresa.CRESAReference);
			AssertNoMessageErrors("CRESA no message errors when Message Cancellation accepted", cresa.CRESAReferenceInfo);
		}

		public void TestECVReference()
		{
			var shipment = CreateShipment(false);
			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();

			AssertEquals("ECV Reference should not be filled in", ZString.Empty, cresa.ECVReference);
			AssertNoMessageErrors("ECV has no message error when no MAA present", cresa.ECVReferenceInfo);

			var ecvReference = shipment.Numbers.AddNew();
			ecvReference.CE_EntryNum = "ECV123";
			ecvReference.CE_EntryType = FranceAdditionalReferenceNumberTypes.Codes.ExportConventional;
			ecvReference.CE_RN_NKCountryCode = Constants.CountryCodes.France;
			cresa = builder.Build();

			AssertEquals("ECV Reference should be filled in", "ECV123", cresa.ECVReference);
			AssertNoMessageErrors("ECV has no message error when no MAA present", cresa.ECVReferenceInfo);

			CreateLog(shipment,
				Events.MessageSent,
				new ZDateTime(2020, 02, 01),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA));
			CreateLog(shipment,
				Events.InterchangeSent,
				new ZDateTime(2020, 02, 02),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA));
			CreateLog(shipment,
				Events.MessageAccepted,
				new ZDateTime(2020, 02, 03),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA));
			cresa = builder.Build();

			AssertEquals("ECV Reference should be filled in when MAA present", "ECV123", cresa.ECVReference);
			AssertNoMessageErrors("ECV has no message error when MAA present", cresa.ECVReferenceInfo);

			shipment.Numbers.RemoveAndDeleteAll();
			cresa = builder.Build();

			AssertEquals("ECV Reference should not be filled in", ZString.Empty, cresa.ECVReference);
			AssertHasMessageError("ECV has message error when MAA present", cresa.ECVReferenceInfo, "ECV Reference is required when sending an amendment or withdrawal. Export Ref Number missing from Shipment > Packing > Pack Line.");
		}

		public void TestEventReferencesValidations()
		{
			var shipment = CreateShipment(false);
			var builder = new CresaBuilder(shipment);

			CreateLog(shipment,
				Events.MessageSent,
				new ZDateTime(2020, 02, 01),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA));
			CreateLog(shipment,
				Events.InterchangeSent,
				new ZDateTime(2020, 02, 02),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA));
			CreateLog(shipment,
				Events.MessageAccepted,
				new ZDateTime(2020, 02, 03),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA));
			var cresa = builder.Build();

			AssertEquals("ECV Reference should not be filled in", ZString.Empty, cresa.ECVReference);
			AssertEquals("CRESA Reference should not be filled in", ZString.Empty, cresa.CRESAReference);
			AssertHasMessageError("ECV has message error when empty after the message was accepcted", cresa.ECVReferenceInfo, "ECV Reference is required when sending an amendment or withdrawal. Export Ref Number missing from Shipment > Packing > Pack Line.");
			AssertHasMessageError("CRESA has message error when empty after the message was accepcted", cresa.CRESAReferenceInfo, "CRESA Reference is required when sending an amendment or withdrawal. Goods Received (CRESA) original message should be accepted by the the Port Community System before sending an amendment or withdrawal.");
		}

		void CreateLog(ForwardingShipment shipment, Event @event, ZDateTime time, params KeyValuePair<string, string>[] parameters)
		{
			shipment.Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, time.ToOffset(), "", parameters);
			Thread.Sleep(1);
			Factory.Save();
		}

		#region Implementation

		ForwardingShipment CreateShipment(ZBool isStandAloneShipment, string pcs = FrenchPortsConstants.PCS.MGI)
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SH0001000";
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_RL_NKDischargePort = "FRMRS";
			shipment.JS_RL_NKDestination = "FRNCE";
			shipment.JS_RL_NKLoadPort = "FRPAR";
			shipment.JS_HBLContainerPackModeOverride = Constants.HBLDeliveryModes.Codes.CFS_CY;
			shipment.JS_INCO = Constants.IncoTerms.CostAndFreight;
			shipment.JS_ShippedOnBoard = "SHP";
			shipment.JS_ShippedOnBoardDate = ZDate.Today;
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(5);
			shipment.JS_ConsolReference = "WhiskyTreasure";
			shipment.JS_F3_NKPackType = "PLT";
			shipment.JS_GoodsDescription = "goods description";
			shipment.DetailedGoodsDescriptionNoteText = ZString.Empty;
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";
			shipment.JS_UnitOfWeight = "T";
			shipment.JS_UnitOfVolume = "D3";

			/*var ecvReference = shipment.Numbers.AddNew();
			ecvReference.CE_EntryNum = "ECV123";
			ecvReference.CE_EntryType = FranceAdditionalReferenceNumberTypes.Codes.ExportConventional;
			ecvReference.CE_RN_NKCountryCode = Constants.CountryCodes.France;*/

			var deliveryOrderReceiptNote = shipment.Notes.AddNew();
			deliveryOrderReceiptNote.ST_Description = PredefinedNoteTypes.Instance.DeliveryOrderReceiptNotes.Description;
			deliveryOrderReceiptNote.ST_NoteText = "Delivery Order Receipt Note";

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 4;
			packline1.JL_F3_NKPackType = "PLT";
			packline1.JL_ActualWeight = 400;
			packline1.JL_ActualWeightUQ = "KG";
			packline1.JL_ActualVolume = 300;
			packline1.JL_ActualVolumeUQ = "M3";
			packline1.JL_Length = 1000;
			packline1.JL_Width = 1000;
			packline1.JL_Height = 300;
			packline1.JL_UnitOfDimension = "CM";
			packline1.JL_HarmonisedCode = "WHISKY";
			packline1.JL_RefNumber = "AMR-57";
			packline1.JL_ExportRefNumber = "AMRUT57%";
			packline1.JL_DetailedDescription = "Amrut Indian Peated Single Malt Detailed Description";
			packline1.JL_MarksAndNumbers = "MarksAndNumbers1";
			packline1.JL_Description = "ALCOHOLIC BEVERAGES 57%";
			packline1.JL_LastKnownTransitWarehouseStatus = "RCV";

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 6;
			packline2.JL_F3_NKPackType = "PLT";
			packline2.JL_ActualWeight = 600;
			packline2.JL_ActualWeightUQ = "KG";
			packline2.JL_ActualVolume = 450;
			packline2.JL_ActualVolumeUQ = "M3";
			packline2.JL_Length = 15;
			packline2.JL_Width = 10;
			packline2.JL_Height = 3;
			packline2.JL_UnitOfDimension = "M";
			packline2.JL_HarmonisedCode = "WHISKY";
			packline2.JL_RefNumber = "AMR-43";
			packline2.JL_ExportRefNumber = "AMRUT43%";
			packline2.JL_DetailedDescription = "Amrut Indian Chill Filter Single Malt Detailed Description";
			packline2.JL_MarksAndNumbers = "MarksAndNumbers2";
			packline2.JL_Description = "ALCOHOLIC BEVERAGES 43%";
			packline2.JL_LastKnownTransitWarehouseStatus = "RCV";

			// No Detailed Description
			var packline3 = shipment.OuterPackLines.AddNew();
			packline3.JL_PackageCount = 3;
			packline3.JL_F3_NKPackType = "PLT";
			packline3.JL_ActualWeight = 300;
			packline3.JL_ActualWeightUQ = "KG";
			packline3.JL_ActualVolume = 225;
			packline3.JL_ActualVolumeUQ = "M3";
			packline3.JL_Length = 15;
			packline3.JL_Width = 5;
			packline3.JL_Height = 3;
			packline3.JL_UnitOfDimension = "M";
			packline3.JL_HarmonisedCode = "WHISKY";
			packline3.JL_RefNumber = "AMR-50";
			packline3.JL_ExportRefNumber = "AMRUT50%";
			packline3.JL_MarksAndNumbers = "MarksAndNumbers3";
			packline3.JL_Description = "ALCOHOLIC BEVERAGES 50%";
			packline3.JL_LastKnownTransitWarehouseStatus = "RCV";

			// No Detailed Description, No Description, No Marks And Numbers
			var packline4 = shipment.OuterPackLines.AddNew();
			packline4.JL_PackageCount = 2;
			packline4.JL_F3_NKPackType = "PLT";
			packline4.JL_ActualWeight = 200;
			packline4.JL_ActualWeightUQ = "KG";
			packline4.JL_ActualVolume = 150;
			packline4.JL_ActualVolumeUQ = "M3";
			packline4.JL_Length = 10;
			packline4.JL_Width = 5;
			packline4.JL_Height = 3;
			packline4.JL_UnitOfDimension = "M";
			packline4.JL_HarmonisedCode = "WHISKY";
			packline4.JL_RefNumber = "AMR-64";
			packline4.JL_ExportRefNumber = "AMRUT64%";
			packline4.JL_LastKnownTransitWarehouseStatus = "RCV";

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Handsome";
			contact.OC_Phone = "1234567";

			if (!isStandAloneShipment)
			{
				var consol = shipment.Consols.AddNew();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_AgentType = Constants.AgentType.Agent;
				consol.JK_ReleaseType = Constants.ShipmentReleaseTypes.OriginalReq;
				consol.JK_NoOriginalBills = 3;
				consol.JK_NoCopyBills = 4;
				consol.JK_RL_NKLoadPort = "AUMEL";
				consol.JK_RL_NKDischargePort = "FRNCE";
				consol.JK_BookingReference = "WhiskyTreasure";
				consol.JK_MasterBillIssueDate = new ZDateTime(2018, 10, 2);
				consol.JK_MasterBillNum = "BILLNUMBER";
				consol.JK_AgentsReference = "AGTREF";
				consol.JK_UniqueConsignRef = "CON0001";
				consol.JK_PrepaidCollect = Constants.PaymentType.Collect;

				PopulateConsolAddresses(consol);

				var transport1 = consol.Transports.OfType<Freight.Business.Transport>().Single();
				transport1.JW_LegOrder = 1;
				transport1.JW_TransportMode = Constants.TransportModes.Sea;
				transport1.JW_TransportType = Constants.TransportPlanningType.MainVessel;
				transport1.JW_RL_NKLoadPort = "AUMEL";
				transport1.JW_RL_NKDiscPort = "AUSYD";
				transport1.JW_Vessel = "Dragon";
				transport1.JW_VoyageFlight = "666";
				transport1.JW_ETD = ZDate.Today.AddDays(1);
				transport1.JW_ETA = ZDate.Today.AddDays(6);

				var transport2 = consol.Transports.AddNew();
				transport2.JW_LegOrder = 2;
				transport2.JW_TransportMode = Constants.TransportModes.Sea;
				transport2.JW_TransportType = Constants.TransportPlanningType.Other;
				transport2.JW_RL_NKLoadPort = "AUSYD";
				transport2.JW_RL_NKDiscPort = "FRMRS";
				transport2.JW_Vessel = "StarShip";
				transport2.JW_VoyageFlight = "666";
				transport2.JW_ETD = ZDate.Today.AddDays(3);
				transport2.JW_ETA = ZDate.Today.AddDays(4);

				var transport3 = consol.Transports.AddNew();
				transport3.JW_LegOrder = 3;
				transport3.JW_TransportMode = Constants.TransportModes.Sea;
				transport3.JW_TransportType = Constants.TransportPlanningType.Other;
				transport3.JW_RL_NKLoadPort = "FRMRS";
				transport3.JW_RL_NKDiscPort = "FRNCE";
				transport3.JW_Vessel = "Enterprise";
				transport3.JW_VoyageFlight = "666";
				transport3.JW_ETD = ZDate.Today.AddDays(5);
				transport3.JW_ETA = ZDate.Today.AddDays(6);

				var refContainer = Factory.NewWithValidTestData<RefContainer>();
				refContainer.RC_ISOType = "22P1";
				refContainer.RC_ContainerType = "RFG";
				refContainer.RC_TareWeight = 222;

				var container1 = consol.Containers.AddNew();
				container1.JC_ContainerNum = "AAAA0000007";
				container1.JC_RC = refContainer.PK;
				container1.JC_DeliveryMode = "CFS/CY";
				container1.JC_IsShipperOwned = true;
				container1.JC_GrossWeightUQ = "KG";
				container1.JC_TareWeight = 1000;
				container1.JC_DunnageWeight = 1000;

				container1.PackLines.Add(packline1);
				container1.PackLines.Add(packline2);
				container1.PackLines.Add(packline3);
				container1.PackLines.Add(packline4);
			}

			PopulateShipmentAddresses(shipment, pcs);

			Factory.Save();

			return shipment;
		}

		void PopulateShipmentAddresses(ForwardingShipment shipment, string pcs)
		{
			ZQuery query = new ZQuery(RefUNLOCOSchema.RL_Code, "FRPAR");
			var unloco = Factory.LoadTop1<RefUNLOCO>(query);
			unloco.RefLocoMaps.DeleteAll();
			var mapFRSoget = unloco.RefLocoMaps.AddNew();
			mapFRSoget.RY_RN = Constants.CountryGuids.France;
			mapFRSoget.RY_SystemUsage = "PCS";
			mapFRSoget.RY_LocalPortCode = pcs;

			var cfs = Factory.New<OrgHeader>();
			cfs.OH_FullName = "CONSPA";
			cfs.OH_RL_NKClosestPort = "FRPAR";
			cfs.MainAddress.Address1 = "Unit 15";
			cfs.MainAddress.Address2 = "5 Lost Lane";
			cfs.MainAddress.City = "Marseille";
			cfs.MainAddress.Postcode = "2000";
			cfs.MainAddress.OA_RN_NKCountryCode = "FR";
			cfs.MainAddress.OA_RL_NKRelatedPortCode = "FRPAR";
			shipment.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "I'm consignor";
			consignor.OH_RL_NKClosestPort = "CNBSX";
			consignor.MainAddress.Address1 = "Unit 200";
			consignor.MainAddress.Address2 = "55 haha Lane";
			consignor.MainAddress.City = "wahaha Ave";
			consignor.MainAddress.Postcode = "10000";
			consignor.MainAddress.OA_RN_NKCountryCode = "CN";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "I'm consignee";
			consignee.OH_RL_NKClosestPort = "AUMEL";
			consignee.MainAddress.Address1 = "Unit 223";
			consignee.MainAddress.Address2 = "553 What Lane";
			consignee.MainAddress.City = "Melbourne";
			consignee.MainAddress.Postcode = "5023";
			consignee.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "Notify Me";
			notifyParty.OH_RL_NKClosestPort = "NZAKL";
			notifyParty.MainAddress.Address1 = "Unit 888";
			notifyParty.MainAddress.Address2 = "8 What Lane";
			notifyParty.MainAddress.City = "Auckland";
			notifyParty.MainAddress.Postcode = "5012";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var notifyParty2 = Factory.New<OrgHeader>();
			notifyParty2.OH_FullName = "Notify Me Two";
			notifyParty2.OH_RL_NKClosestPort = "NZAKL";
			notifyParty2.MainAddress.Address1 = "Unit 666";
			notifyParty2.MainAddress.Address2 = "8 How Lane";
			notifyParty2.MainAddress.City = "Auckland";
			notifyParty2.MainAddress.Postcode = "5032";
			notifyParty2.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2.MainAddress.PK;

			var consignorPickupAddress = Factory.New<OrgHeader>();
			consignorPickupAddress.OH_FullName = "consignor Pickup org";
			consignorPickupAddress.OH_RL_NKClosestPort = "CNBZN";
			consignorPickupAddress.MainAddress.Address1 = "Unit 645";
			consignorPickupAddress.MainAddress.Address2 = "234 Drive";
			consignorPickupAddress.MainAddress.City = "unknown city";
			consignorPickupAddress.MainAddress.Postcode = "3243";
			consignorPickupAddress.MainAddress.OA_RN_NKCountryCode = "CN";

			shipment.ConsignorPickupAddress.E2_OA_Address = consignorPickupAddress.MainAddress.PK;

			var consigneeDeliveryAddress = Factory.New<OrgHeader>();
			consigneeDeliveryAddress.OH_FullName = "consignee delivery org";
			consigneeDeliveryAddress.OH_RL_NKClosestPort = "SGJUR";
			consigneeDeliveryAddress.MainAddress.Address1 = "Unit 563";
			consigneeDeliveryAddress.MainAddress.Address2 = "435 Drive";
			consigneeDeliveryAddress.MainAddress.City = "unknown city";
			consigneeDeliveryAddress.MainAddress.Postcode = "4356";
			consigneeDeliveryAddress.MainAddress.OA_RN_NKCountryCode = "SG";

			shipment.ConsigneeDeliveryAddress.E2_OA_Address = consigneeDeliveryAddress.MainAddress.PK;

			var pickupAgentAddress = Factory.New<OrgHeader>();
			pickupAgentAddress.OH_FullName = "pickup agent org";
			pickupAgentAddress.OH_RL_NKClosestPort = "FRNCE";
			pickupAgentAddress.MainAddress.Address1 = "Unit 283";
			pickupAgentAddress.MainAddress.Address2 = "283 Drive";
			pickupAgentAddress.MainAddress.City = "unknown city";
			pickupAgentAddress.MainAddress.Postcode = "2836";
			pickupAgentAddress.MainAddress.OA_RN_NKCountryCode = "FR";

			shipment.PickupAgentDocumentaryAddress.E2_OA_Address = pickupAgentAddress.MainAddress.PK;

			var docsAndCartage = shipment.DocsAndCartage;
			docsAndCartage.JP_OA_PickupCartageCoAddr = pickupAgentAddress.MainAddress.PK;
		}

		void PopulateConsolAddresses(ForwardingConsol consol)
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
			carrier.MainAddress.Address1 = "Unit 13";
			carrier.MainAddress.Address2 = "4 Lost Lane";
			carrier.MainAddress.City = "Aalborg";
			carrier.MainAddress.Postcode = "2000";
			carrier.MainAddress.OA_RN_NKCountryCode = "DK";

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "I'm Sending Stuff";
			sendingForwarder.OH_RL_NKClosestPort = "CNNJI";
			sendingForwarder.MainAddress.Address1 = "Unit 200";
			sendingForwarder.MainAddress.Address2 = "55 Why Lane";
			sendingForwarder.MainAddress.City = "Conficious Ave";
			sendingForwarder.MainAddress.Postcode = "10000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "CN";
			sendingForwarder.CustomsCodes.AddNew(OrgCusCode.ChinaCodeTypes.USC, "1234567890", "CN");

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "I'm Receiving Stuff";
			receivingForwarder.OH_RL_NKClosestPort = "AUSYD";
			receivingForwarder.MainAddress.Address1 = "Unit 399";
			receivingForwarder.MainAddress.Address2 = "50 What Lane";
			receivingForwarder.MainAddress.City = "Sydney";
			receivingForwarder.MainAddress.Postcode = "5023";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";
			receivingForwarder.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "410 10 10 10", "AU");

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "consol notify party";
			notifyParty.OH_RL_NKClosestPort = "GBLON";
			notifyParty.MainAddress.Address1 = "Unit 400";
			notifyParty.MainAddress.Address2 = "443 How Lane";
			notifyParty.MainAddress.City = "Angel";
			notifyParty.MainAddress.Postcode = "8888";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "GB";
			notifyParty.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "123 123 123", "GB");

			consol.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var notifyParty2 = Factory.New<OrgHeader>();
			notifyParty2.OH_FullName = "consol notify party 2";
			notifyParty2.OH_RL_NKClosestPort = "CNCAN";
			notifyParty2.MainAddress.Address1 = "Unit 460";
			notifyParty2.MainAddress.Address2 = "333 How Lane";
			notifyParty2.MainAddress.City = "Wonderland";
			notifyParty2.MainAddress.Postcode = "7777";
			notifyParty2.MainAddress.OA_RN_NKCountryCode = "CN";
			notifyParty2.CustomsCodes.AddNew(OrgCusCode.ChinaCodeTypes.USC, "1111111111", "CN");

			consol.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2.MainAddress.PK;

			var carrierHandlingAgent = Factory.New<OrgHeader>();
			carrierHandlingAgent.OH_FullName = "consol carrier handling agent";
			carrierHandlingAgent.OH_RL_NKClosestPort = "CNCAN";
			carrierHandlingAgent.MainAddress.Address1 = "Unit 990";
			carrierHandlingAgent.MainAddress.Address2 = "245 Drive";
			carrierHandlingAgent.MainAddress.City = "unknown city";
			carrierHandlingAgent.MainAddress.Postcode = "4689";
			carrierHandlingAgent.MainAddress.OA_RN_NKCountryCode = "CN";

			consol.CarrierHandlingAgentDocumentaryAddress.E2_OA_Address = carrierHandlingAgent.MainAddress.PK;

			var carrierBookingAgent = Factory.New<OrgHeader>();
			carrierBookingAgent.OH_FullName = "consol carrier booking agent";
			carrierBookingAgent.OH_RL_NKClosestPort = "CNCAN";
			carrierBookingAgent.MainAddress.Address1 = "Unit 990";
			carrierBookingAgent.MainAddress.Address2 = "245 Drive";
			carrierBookingAgent.MainAddress.City = "unknown city";
			carrierBookingAgent.MainAddress.Postcode = "4689";
			carrierBookingAgent.MainAddress.OA_RN_NKCountryCode = "CN";

			consol.CarrierBookingAgentDocumentaryAddress.E2_OA_Address = carrierBookingAgent.MainAddress.PK;

			var packDepotOrg = Factory.New<OrgHeader>();
			packDepotOrg.OH_FullName = "pack depot org";
			packDepotOrg.OH_RL_NKClosestPort = "CNCAN";
			packDepotOrg.MainAddress.Address1 = "Unit 888";
			packDepotOrg.MainAddress.Address2 = "111 Drive";
			packDepotOrg.MainAddress.City = "unknown city";
			packDepotOrg.MainAddress.Postcode = "4679";
			packDepotOrg.MainAddress.OA_RN_NKCountryCode = "CN";

			consol.JK_OA_PackDepotAddress = packDepotOrg.MainAddress.PK;

			var unpackDepotOrg = Factory.New<OrgHeader>();
			unpackDepotOrg.OH_FullName = "unpack depot org";
			unpackDepotOrg.OH_RL_NKClosestPort = "SGSIN";
			unpackDepotOrg.MainAddress.Address1 = "Unit 589";
			unpackDepotOrg.MainAddress.Address2 = "625 Drive";
			unpackDepotOrg.MainAddress.City = "unknown city";
			unpackDepotOrg.MainAddress.Postcode = "9541";
			unpackDepotOrg.MainAddress.OA_RN_NKCountryCode = "SG";

			consol.JK_OA_UnpackDepotAddress = unpackDepotOrg.MainAddress.PK;
		}

		#endregion

	}
}
