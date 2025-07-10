using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Forwarding.Documents.Testing.AssertionHelper;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;
using Constants = Enterprise.Core.Constants;
using ShippingLineMessagingRequirement = Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants.ShippingLineMessagingRequirement;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	abstract class CarrierMessageDataBuilderTest : TestCaseWithFactory
	{
		#region Data
		public void TestBookingRequestAndShippingInstructionsEnhancements()
		{
			var consol = CreateConsol();
			var creditor = consol.Creditor;

			consol.JK_AgentType = Constants.AgentType.Agent;
			AssertEquals("DKAAL", CreateDocDataObjectBuilder(consol).Build().Carrier.Unloco.Code);

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			AssertEquals("DKAAL", CreateDocDataObjectBuilder(consol).Build().Carrier.Unloco.Code);
		}

		public void TestCarrierMessageData()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_AgentsReference = "A128569";
			consol.JK_MasterBillNum = "MBL00023";
			consol.JK_CoLoadMasterBill = "CMBL00012";
			consol.JK_DatePortOfFirstArrival = new ZDateTime(2019, 11, 20);
			consol.JK_DateFirstForeignPort = new ZDateTime(2019, 11, 19);
			consol.JK_DateLastForeignPort = new ZDateTime(2019, 11, 25);
			consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			consol.JK_CarrierContractNumber = "33333";

			var number1 = consol.Numbers.AddNew();
			number1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			number1.CE_EntryNum = "22222";

			var number2 = consol.Numbers.AddNew();
			number2.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount;
			number2.CE_EntryNum = "Contract Named Account";

			var goodsHandlingNote = consol.Notes.AddNew();
			goodsHandlingNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			goodsHandlingNote.ST_NoteText = "goods handling instructions";

			var forwardingInstructionsNote = consol.Notes.AddNew();
			forwardingInstructionsNote.ST_Description = PredefinedNoteTypes.Instance.ForwardingInstructionNotes.Description;
			forwardingInstructionsNote.ST_NoteText = "forwarding instructions";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_BookingReference = "B00588";
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 1;
			packLine.JL_ActualWeight = 45;

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertionHelper.AssertCurrentUserAddressData(carrierMessageData.CurrentUser);

			CombineAssertions(() =>
			{
				AssertEquals("ShipmentType.Code", "AGT", carrierMessageData.AgentType.Code);
				AssertEquals("MasterBillNumber", "MBL00023", carrierMessageData.MasterBillNumber);
				AssertEquals("ShipperReference", "A128569", carrierMessageData.ShipperReference);
				AssertEquals("PortOfFirstArrivalDate", new ZDateTime(2019, 11, 20), carrierMessageData.PortOfFirstArrivalDate);
				AssertEquals("FirstForeignArrivalDate", new ZDateTime(2019, 11, 19), carrierMessageData.FirstForeignArrivalDate);
				AssertEquals("LastForeignDepartureDate", new ZDateTime(2019, 11, 25), carrierMessageData.LastForeignDepartureDate);
				AssertEquals("PaymentMethod", "PPD", carrierMessageData.PaymentMethod.Code);
				AssertContainsExactElementsInAnyOrder("Numbers", new[] { "BKG: 22222", "NAC: Contract Named Account" }, carrierMessageData.Numbers.Select(x => $"{x.Type.Code}: {x.Value}"));
				AssertEquals("CarrierContractNumberFormatted", "33333", carrierMessageData.CarrierContractNumbersFormatted);
				AssertEquals("ContractNamedAccount", "Contract Named Account", carrierMessageData.ContractNamedAccount);
				AssertEquals("GoodsHandlingInstructions", "goods handling instructions", carrierMessageData.GoodsHandlingInstructions);
				AssertEquals("ForwardingInstructions", "forwarding instructions", carrierMessageData.ForwardingInstructions);
				Assert("IsRORO", !carrierMessageData.IsRORO);
				Assert("IsColoadLCL", !carrierMessageData.IsColoadLCL);
				Assert("IsHazardous", !carrierMessageData.IsHazardous);
			});

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "";
			contact.OC_Phone = "";

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "6666";
			subs.DG_Variant = "E";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			var undg = packLine.UNDGs.AddNew();
			undg.LinkDefault(subs);
			undg.Substance.DG_PSN = "DG SHIPPER NAME";
			undg.DI_TechnicalName = "WHATEVER";
			undg.DI_IMOClass = "CLAS";
			undg.Substance.DG_PG = "GRO";
			undg.Substance.DG_SubLabel1 = "sub1";
			undg.Substance.DG_SubLabel2 = "su2";
			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 15.0m;
			undg.DI_MPMarinePollutant = "N";
			undg.DI_DGVolume = 2m;
			undg.DI_UnitOfVolume = "M3";
			undg.DI_DGWeight = 200m;
			undg.DI_UnitOfWeight = "KG";
			undg.DI_IsLimitedQuantity = true;
			undg.DI_PackageCount = 5;
			undg.DI_F3_NKPackType = "BAG";
			undg.DI_OC_DGContact = contact.PK;

			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			CombineAssertions(() =>
			{
				AssertEquals("ShipmentType.Code", "AGT", carrierMessageData.AgentType.Code);
				AssertEquals("MasterBillNumber", "MBL00023", carrierMessageData.MasterBillNumber);
				AssertEquals("ShipperReference", "A128569", carrierMessageData.ShipperReference);
				AssertEquals("PortOfFirstArrivalDate", new ZDateTime(2019, 11, 20), carrierMessageData.PortOfFirstArrivalDate);
				AssertEquals("FirstForeignArrivalDate", new ZDateTime(2019, 11, 19), carrierMessageData.FirstForeignArrivalDate);
				AssertEquals("LastForeignDepartureDate", new ZDateTime(2019, 11, 25), carrierMessageData.LastForeignDepartureDate);
				AssertEquals("PaymentMethod", "PPD", carrierMessageData.PaymentMethod.Code);
				AssertContainsExactElementsInAnyOrder("Numbers", new[] { "BKG: 22222", "NAC: Contract Named Account" }, carrierMessageData.Numbers.Select(x => $"{x.Type.Code}: {x.Value}"));
				AssertEquals("CarrierContractNumberFormatted", "33333", carrierMessageData.CarrierContractNumbersFormatted);
				AssertEquals("ContractNamedAccount", "Contract Named Account", carrierMessageData.ContractNamedAccount);
				AssertEquals("GoodsHandlingInstructions", "goods handling instructions", carrierMessageData.GoodsHandlingInstructions);
				AssertEquals("ForwardingInstructions", "forwarding instructions", carrierMessageData.ForwardingInstructions);
				Assert("IsRORO", !carrierMessageData.IsRORO);
				Assert("IsColoadLCL", !carrierMessageData.IsColoadLCL);
				Assert("IsHazardous", carrierMessageData.IsHazardous);
			});

			packLine.UNDGs.DeleteAll();
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			CombineAssertions(() =>
			{
				AssertEquals("ShipmentType.Code", "AGT", carrierMessageData.AgentType.Code);
				AssertEquals("MasterBillNumber", "MBL00023", carrierMessageData.MasterBillNumber);
				AssertEquals("ShipperReference", "A128569", carrierMessageData.ShipperReference);
				AssertEquals("PortOfFirstArrivalDate", new ZDateTime(2019, 11, 20), carrierMessageData.PortOfFirstArrivalDate);
				AssertEquals("FirstForeignArrivalDate", new ZDateTime(2019, 11, 19), carrierMessageData.FirstForeignArrivalDate);
				AssertEquals("LastForeignDepartureDate", new ZDateTime(2019, 11, 25), carrierMessageData.LastForeignDepartureDate);
				AssertEquals("PaymentMethod", "PPD", carrierMessageData.PaymentMethod.Code);
				AssertContainsExactElementsInAnyOrder("Numbers", new[] { "BKG: 22222", "NAC: Contract Named Account" }, carrierMessageData.Numbers.Select(x => $"{x.Type.Code}: {x.Value}"));
				AssertEquals("CarrierContractNumberFormatted", "33333", carrierMessageData.CarrierContractNumbersFormatted);
				AssertEquals("ContractNamedAccount", "Contract Named Account", carrierMessageData.ContractNamedAccount);
				AssertEquals("GoodsHandlingInstructions", "goods handling instructions", carrierMessageData.GoodsHandlingInstructions);
				AssertEquals("ForwardingInstructions", "forwarding instructions", carrierMessageData.ForwardingInstructions);
				Assert("IsRORO", !carrierMessageData.IsRORO);
				Assert("IsColoadLCL", !carrierMessageData.IsColoadLCL);
				Assert("IsHazardous", !carrierMessageData.IsHazardous);
			});

			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_ConsolMode = Constants.ContainerModes.RollOnRollOff;
			consol.JK_IsHazardous = true;

			var dangerousGood1 = consol.ConsolDGRestrictionCollection.AddNew();
			dangerousGood1.JKD_Class = "2.1";
			dangerousGood1.JKD_UNNO = "1001";

			var numberCQN = consol.Numbers.AddNew();
			numberCQN.CE_EntryType = DocDataConstants.AdditionalReferences.Codes.CarrierQuoteNumber;
			numberCQN.CE_EntryNum = "55555";
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			CombineAssertions(() =>
			{
				AssertEquals("ShipmentType.Code", "DRT", carrierMessageData.AgentType.Code);
				AssertEquals("MasterBillNumber", "MBL00023", carrierMessageData.MasterBillNumber);
				AssertEquals("ShipperReference", "B00588", carrierMessageData.ShipperReference);
				AssertEquals("PortOfFirstArrivalDate", new ZDateTime(2019, 11, 20), carrierMessageData.PortOfFirstArrivalDate);
				AssertEquals("FirstForeignArrivalDate", new ZDateTime(2019, 11, 19), carrierMessageData.FirstForeignArrivalDate);
				AssertEquals("LastForeignDepartureDate", new ZDateTime(2019, 11, 25), carrierMessageData.LastForeignDepartureDate);
				AssertEquals("PaymentMethod", "PPD", carrierMessageData.PaymentMethod.Code);
				AssertContainsExactElementsInAnyOrder("Numbers", new[] { "BKG: 22222", "NAC: Contract Named Account", "CQN: 55555" }, carrierMessageData.Numbers.Select(x => $"{x.Type.Code}: {x.Value}"));
				AssertEquals("CarrierContractNumberFormatted", "55555", carrierMessageData.CarrierContractNumbersFormatted);
				AssertEquals("ContractNamedAccount", "Contract Named Account", carrierMessageData.ContractNamedAccount);
				AssertEquals("GoodsHandlingInstructions", "goods handling instructions", carrierMessageData.GoodsHandlingInstructions);
				AssertEquals("ForwardingInstructions", "forwarding instructions", carrierMessageData.ForwardingInstructions);
				Assert("IsRORO", carrierMessageData.IsRORO);
				Assert("IsColoadLCL", !carrierMessageData.IsColoadLCL);
				Assert("IsHazardous", carrierMessageData.IsHazardous);
				AssertEquals("DGRestrictions", "2.1", consol.ConsolDGRestrictionCollection.FirstOrDefault().JKD_Class);
				AssertEquals("DGRestrictions", "1001", consol.ConsolDGRestrictionCollection.FirstOrDefault().JKD_Calc_Substance);
			});

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			CombineAssertions(() =>
			{
				AssertEquals("ShipmentType.Code", "CLD", carrierMessageData.AgentType.Code);
				AssertEquals("MasterBillNumber", "MBL00023", carrierMessageData.MasterBillNumber);
				AssertEquals("ShipperReference", "A128569", carrierMessageData.ShipperReference);
				AssertEquals("PortOfFirstArrivalDate", new ZDateTime(2019, 11, 20), carrierMessageData.PortOfFirstArrivalDate);
				AssertEquals("FirstForeignArrivalDate", new ZDateTime(2019, 11, 19), carrierMessageData.FirstForeignArrivalDate);
				AssertEquals("LastForeignDepartureDate", new ZDateTime(2019, 11, 25), carrierMessageData.LastForeignDepartureDate);
				AssertEquals("PaymentMethod", "PPD", carrierMessageData.PaymentMethod.Code);
				AssertContainsExactElementsInAnyOrder("Numbers", new[] { "BKG: 22222", "NAC: Contract Named Account", "CQN: 55555" }, carrierMessageData.Numbers.Select(x => $"{x.Type.Code}: {x.Value}"));
				AssertEquals("CarrierContractNumberFormatted", "55555", carrierMessageData.CarrierContractNumbersFormatted);
				AssertEquals("ContractNamedAccount", "Contract Named Account", carrierMessageData.ContractNamedAccount);
				AssertEquals("GoodsHandlingInstructions", "goods handling instructions", carrierMessageData.GoodsHandlingInstructions);
				AssertEquals("ForwardingInstructions", "forwarding instructions", carrierMessageData.ForwardingInstructions);
				Assert("IsRORO", carrierMessageData.IsRORO);
				Assert("IsColoadLCL", !carrierMessageData.IsColoadLCL);
				Assert("IsHazardous", carrierMessageData.IsHazardous);
				AssertEquals("DGRestrictions", "2.1", consol.ConsolDGRestrictionCollection.FirstOrDefault().JKD_Class);
				AssertEquals("DGRestrictions", "1001", consol.ConsolDGRestrictionCollection.FirstOrDefault().JKD_Calc_Substance);
			});

			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			CombineAssertions(() =>
			{
				AssertEquals("ShipmentType.Code", "CLD", carrierMessageData.AgentType.Code);
				AssertEquals("MasterBillNumber", "MBL00023", carrierMessageData.MasterBillNumber);
				AssertEquals("ShipperReference", "A128569", carrierMessageData.ShipperReference);
				AssertEquals("PortOfFirstArrivalDate", new ZDateTime(2019, 11, 20), carrierMessageData.PortOfFirstArrivalDate);
				AssertEquals("FirstForeignArrivalDate", new ZDateTime(2019, 11, 19), carrierMessageData.FirstForeignArrivalDate);
				AssertEquals("LastForeignDepartureDate", new ZDateTime(2019, 11, 25), carrierMessageData.LastForeignDepartureDate);
				AssertEquals("PaymentMethod", "PPD", carrierMessageData.PaymentMethod.Code);
				AssertContainsExactElementsInAnyOrder("Numbers", new[] { "BKG: 22222", "NAC: Contract Named Account", "CQN: 55555" }, carrierMessageData.Numbers.Select(x => $"{x.Type.Code}: {x.Value}"));
				AssertEquals("CarrierContractNumberFormatted", "55555", carrierMessageData.CarrierContractNumbersFormatted);
				AssertEquals("GoodsHandlingInstructions", "goods handling instructions", carrierMessageData.GoodsHandlingInstructions);
				AssertEquals("ForwardingInstructions", "forwarding instructions", carrierMessageData.ForwardingInstructions);
				Assert("IsRORO", !carrierMessageData.IsRORO);
				Assert("IsColoadLCL", carrierMessageData.IsColoadLCL);
				Assert("IsHazardous", carrierMessageData.IsHazardous);
				AssertEquals("DGRestrictions", "2.1", consol.ConsolDGRestrictionCollection.FirstOrDefault().JKD_Class);
				AssertEquals("DGRestrictions", "1001", consol.ConsolDGRestrictionCollection.FirstOrDefault().JKD_Calc_Substance);
			});
		}

		public void TestPopulateIsNVO()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_IsNVO = true;
			shippingLine.RSL_IsShippingLine = false;

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			Assert(carrierMessageData.IsNVO);

			shippingLine.RSL_IsShippingLine = true;
			consol.JK_ConsolMode = Constants.ContainerModes.LCL;

			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			Assert(carrierMessageData.IsNVO);

			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			Assert(!carrierMessageData.IsNVO);
		}

		public void TestPopulateCarrierContractNumbersFormattedAndContractNamedAccount()
		{
			var consol = Factory.New<ForwardingConsol>();
			var carrier = Factory.New<OrgHeader>();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_CarrierContractNumber = "11111, 33333";

			var numberNAC = consol.Numbers.AddNew();
			numberNAC.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount;
			numberNAC.CE_EntryNum = "ContractNamedAccount";

			consol.ShippingLine.OH_IsSeaWholesaler = false;
			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("CQN exists:", "11111, 33333", carrierMessageData.CarrierContractNumbersFormatted);
			AssertEquals("NAC exists:", "ContractNamedAccount", carrierMessageData.ContractNamedAccount);

			consol.JK_CarrierContractNumber = null;
			consol.Numbers.Remove(numberNAC);
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("CQN dose not exist.", ZString.Empty, carrierMessageData.CarrierContractNumbersFormatted);
			AssertEquals("NAC does not exist.", ZString.Empty, carrierMessageData.ContractNamedAccount);
		}

		public void TestPopulateGoodsHandlingInstructionsWithFallback()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_AgentsReference = "A128569";
			consol.JK_MasterBillNum = "MBL00023";
			consol.JK_CoLoadMasterBill = "CMBL00012";

			var consolGoodsHandlingNote = consol.Notes.AddNew();
			consolGoodsHandlingNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consolGoodsHandlingNote.ST_NoteText = "goods 1";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_BookingReference = "B00588";
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var shipmentGoodsHandlingNote = shipment.Notes.AddNew();
			shipmentGoodsHandlingNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			shipmentGoodsHandlingNote.ST_NoteText = "goods 2";

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("default from consol's handling instructions note", "goods 1", carrierMessageData.GoodsHandlingInstructions);

			consol.Notes.RemoveAndDeleteAll();

			consol.JK_AgentType = Constants.AgentType.Direct;

			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("fallback from direct shipment's handling instructions note if consol's note is empty", "goods 2", carrierMessageData.GoodsHandlingInstructions);
		}

		public void TestPopulateGoodsHandlingInstructionsExclusiveUse()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_AgentsReference = "A128569";
			consol.JK_MasterBillNum = "MBL00023";
			consol.JK_CoLoadMasterBill = "CMBL00012";
			var consolGoodsHandlingNote = consol.Notes.AddNew();
			consolGoodsHandlingNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consolGoodsHandlingNote.ST_NoteText = "goods 1";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_BookingReference = "B00588";
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			var packline = shipment.OuterPackLines.AddNew();
			var undgs = packline.UNDGs.AddNew();
			undgs.DI_IsExclusiveUse = true;
			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("Exclusive Use is appended", "goods 1\r\nExclusive Use", carrierMessageData.GoodsHandlingInstructions);
		}

		public void TestPacklineContainerNumberWhenAttachedToSecondConsol()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.Containers.RemoveAll();

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.Containers.RemoveAll();

			var container = consol2.Containers.AddNew();
			container.JC_ContainerNum = "X";

			var shipment = Factory.New<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();

			consol1.Shipments.Add(shipment);
			consol2.Shipments.Add(shipment);
			container.PackLines.Add(packline);

			var carrierMessageData = CreateDocDataObjectBuilder(consol2).Build();
			AssertEquals("ContainerNumber should be the same as the container from the second consol", container.JC_ContainerNum, carrierMessageData.Containers.ElementAt(0).PackingLines.ElementAt(0).ContainerNumber);
		}

		public void TestPacklineContainerNumberWhenAttachedToBothConsols()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.Containers.RemoveAll();

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.Containers.RemoveAll();

			var container = consol2.Containers.AddNew();
			container.JC_ContainerNum = "X";

			var container2 = consol1.Containers.AddNew();
			container2.JC_ContainerNum = "";

			var shipment = Factory.New<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();

			consol1.Shipments.Add(shipment);
			consol2.Shipments.Add(shipment);

			container.PackLines.Add(packline);
			container2.PackLines.Add(packline);

			var carrierMessageData = CreateDocDataObjectBuilder(consol2).Build();

			AssertEquals("ContainerNumber should be the same as the container from the second consol", container.JC_ContainerNum, carrierMessageData.Containers.ElementAt(0).PackingLines.ElementAt(0).ContainerNumber);
		}

		public void TestPopulateContainerIsNonOperativeReefer()
		{
			var consol = CreateConsol();

			var container1 = consol.Containers.First() as ForwardingContainer;
			container1.JC_ContainerNum = "AAAA0000006";
			container1.JC_IsNonOperativeReefer = true;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "AAAA0000008";
			container2.JC_DeliveryMode = "CFS/CY";
			container2.JC_IsShipperOwned = true;
			container2.JC_GrossWeightUQ = "KG";
			container2.JC_TareWeight = 1000;
			container2.JC_DunnageWeight = 1000;
			container2.JC_RC = (Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP")).PK;
			container2.JC_IsNonOperativeReefer = false;

			var data = CreateDocDataObjectBuilder(consol).Build();

			Assert(data.Containers.First(x => x.Number == "AAAA0000006").IsNonOperativeReefer);
			Assert(!data.Containers.First(x => x.Number == "AAAA0000008").IsNonOperativeReefer);
		}

		public void TestPopulateUnitsOfWeightVolumeAndDimension()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_AgentsReference = "A128569";
			consol.JK_MasterBillNum = "MBL00023";
			consol.JK_CoLoadMasterBill = "CMBL00012";
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "CNSHA";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_Vessel = "Dragon";
			transport.JW_VoyageFlight = "111";
			transport.JW_ETD = new ZDateTime(2018, 12, 1);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_TransportType = Constants.TransportPlanningType.Other;
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "NZAKL";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_BookingReference = "B00588";
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			AssertEquals("UnitOfWeight should be KG when pack lines are not ready", "KG", carrierMessageData.UnitOfWeight);
			AssertEquals("UnitOfVolume should be M3 when pack lines are not ready", "M3", carrierMessageData.UnitOfVolume);
			AssertEquals("UnitOfDimension should be M when pack lines are not ready", "M", carrierMessageData.UnitOfDimensions);

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_ActualWeight = 200;
			packline1.JL_ActualWeightUQ = "OZ";
			packline1.JL_ActualVolume = 300;
			packline1.JL_ActualVolumeUQ = "CI";
			packline1.JL_Length = 1;
			packline1.JL_Width = 1;
			packline1.JL_Height = 1;
			packline1.JL_UnitOfDimension = "IN";

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_ActualWeight = 200;
			packline2.JL_ActualWeightUQ = "LB";
			packline2.JL_ActualVolume = 300;
			packline2.JL_ActualVolumeUQ = "CF";
			packline2.JL_Length = 1;
			packline2.JL_Width = 1;
			packline2.JL_Height = 1;
			packline2.JL_UnitOfDimension = "IN";

			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("UnitOfWeight should be LB while all pack lines have imperial dimension units", "LB", carrierMessageData.UnitOfWeight);
			AssertEquals("UnitOfVolume should be CF while all pack lines have imperial dimension units", "CF", carrierMessageData.UnitOfVolume);
			AssertEquals("UnitOfDimension should be FT while all pack lines have imperial dimension units", "FT", carrierMessageData.UnitOfDimensions);

			packline1.JL_UnitOfDimension = "FT";
			packline2.JL_UnitOfDimension = "FT";
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("UnitOfDimension should be FT while all pack lines have imperial dimension units", "FT", carrierMessageData.UnitOfDimensions);

			packline1.JL_UnitOfDimension = "YD";
			packline2.JL_UnitOfDimension = "YD";
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("UnitOfDimension should be FT while all pack lines have imperial dimension units", "FT", carrierMessageData.UnitOfDimensions);

			packline1.JL_UnitOfDimension = "FT";
			packline2.JL_UnitOfDimension = "M";
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("UnitOfDimension should be M if any pack lines do not have an imperial dimension of unit", "M", carrierMessageData.UnitOfDimensions);

			packline1.JL_ActualWeightUQ = "OZ";
			packline2.JL_ActualWeightUQ = "TN";
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("UnitOfWeight should be LB while all pack lines have imperial weight units", "LB", carrierMessageData.UnitOfWeight);

			packline1.JL_ActualVolumeUQ = "CY";
			packline2.JL_ActualVolumeUQ = "CI";
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("UnitOfVolume should be CF when all pack lines have imperial volume units", "CF", carrierMessageData.UnitOfVolume);

			packline2.JL_UnitOfDimension = "FT";
			transport.JW_RL_NKLoadPort = "BRARC";
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			AssertEquals("Unit of weight should be KG when loading in Brazil", "KG", carrierMessageData.UnitOfWeight);
			AssertEquals("Unit of volume should be M3 when loading in Brazil", "M3", carrierMessageData.UnitOfVolume);
			AssertEquals("UnitOfDimension should be M when loading in Brazil", "M", carrierMessageData.UnitOfDimensions);

			transport.JW_RL_NKLoadPort = "CNSHA";
			transport2.JW_RL_NKDiscPort = "BRARC";
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			AssertEquals("Unit of weight should be KG when discharging in Brazil", "KG", carrierMessageData.UnitOfWeight);
			AssertEquals("Unit of volume should be M3 when discharging in Brazil", "M3", carrierMessageData.UnitOfVolume);
			AssertEquals("UnitOfDimension should be M when discharging in Brazil", "M", carrierMessageData.UnitOfDimensions);

			transport2.JW_RL_NKDiscPort = "NZAKL";
			packline1.JL_ActualWeightUQ = "KG";
			packline2.JL_ActualWeightUQ = "LB";
			packline1.JL_ActualVolumeUQ = "M3";
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			AssertEquals("Unit of weight should be KG when not all pack lines have the same UW", "KG", carrierMessageData.UnitOfWeight);
			AssertEquals("Unit of volume should be M3 when not all pack lines have the same UV", "M3", carrierMessageData.UnitOfVolume);
		}

		public void TestPopulateOriginAndDestination()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment1.JS_RL_NKOrigin = "CNNJG";
			shipment1.JS_RL_NKDestination = "AUMEL";

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment2.JS_RL_NKOrigin = "CNNJG";
			shipment2.JS_RL_NKDestination = "AUMEL";

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("use shipment port as origin when all the shipment have the same origin", "CNNJG", carrierMessageData.Origin.Code);
			AssertEquals("use shipment port as destination when all the shipment have the same destination", "AUMEL", carrierMessageData.Destination.Code);

			shipment1.JS_RL_NKOrigin = "CNBJS";
			shipment1.JS_RL_NKDestination = "AUADL";

			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("fall back from consol when all the shipment do not have the same origin", "CNSHA", carrierMessageData.Origin.Code);
			AssertEquals("fall back from consol when all the shipment do not have the same destination", "AUSYD", carrierMessageData.Destination.Code);

			consol.JK_AgentType = Constants.AgentType.Direct;

			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("from first shiment's origin port when consol is direct", "CNBJS", carrierMessageData.Origin.Code);
			AssertEquals("from first shiment's destination port when consol is direct", "AUADL", carrierMessageData.Destination.Code);
		}

		public void TestPopulateLegPorts()
		{
			var consol = CreateConsol();
			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			AssertEquals("CNSHA", carrierMessageData.PortOfLoading.Code);
			AssertEquals("AUSYD", carrierMessageData.PortOfDischarge.Code);
		}

		public void TestPopulatePorts()
		{
			var consol = CreateConsol();

			Factory.Save();

			using (Registry.ForwardingConfigurationRegistry.Instance.ShowCountryStateBookingRequestShippingInstruction.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

				AssertEquals("Shanghai Hongqiao International Apt", carrierMessageData.PortOfLoading.Name);
				AssertEquals("Sydney", carrierMessageData.PortOfDischarge.Name);
				AssertEquals("Shanghai Hongqiao International Apt", carrierMessageData.Origin.Name);
				AssertEquals("Sydney", carrierMessageData.Destination.Name);
				AssertEquals("Shanghai Hongqiao International Apt", carrierMessageData.PlaceOfReceipt.Name);
				AssertEquals("Sydney", carrierMessageData.PlaceOfDelivery.Name);
				AssertEquals("Aalborg", carrierMessageData.PlaceOfIssue.Name);
				AssertEquals("Test", carrierMessageData.CarrierBookingOffice.Name);
			}

			using (Registry.ForwardingConfigurationRegistry.Instance.ShowCountryStateBookingRequestShippingInstruction.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var carrierMessageData2 = CreateDocDataObjectBuilder(consol).Build();

				AssertEquals("Shanghai Hongqiao International Apt", carrierMessageData2.PortOfLoading.Name);
				AssertEquals("Sydney, Australia", carrierMessageData2.PortOfDischarge.Name);
				AssertEquals("Shanghai Hongqiao International Apt", carrierMessageData2.Origin.Name);
				AssertEquals("Sydney, Australia", carrierMessageData2.Destination.Name);
				AssertEquals("Shanghai Hongqiao International Apt", carrierMessageData2.PlaceOfReceipt.Name);
				AssertEquals("Sydney, Australia", carrierMessageData2.PlaceOfDelivery.Name);
				AssertEquals("Aalborg, Denmark", carrierMessageData2.PlaceOfIssue.Name);
				AssertEquals("Test", carrierMessageData2.CarrierBookingOffice.Name);
			}
		}

		public void TestPopulateNumbers()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var number1 = consol.Numbers.AddNew();
			number1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON;
			number1.CE_EntryNum = "11111";

			var number2 = consol.Numbers.AddNew();
			number2.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			number2.CE_EntryNum = "22222";

			var number3 = consol.Numbers.AddNew();
			number3.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount;
			number3.CE_EntryNum = "Contract Named Account";

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			AssertContainsExactElementsInAnyOrder("Numbers", new[] { "CON: 11111", "BKG: 22222", "NAC: Contract Named Account" }, carrierMessageData.Numbers.Select(x => $"{x.Type.Code}: {x.Value}"));
		}

		public void TestPopulateIsTaxIdModifiable()
		{
			var consol = Factory.New<ForwardingConsol>();
			var builder = CreateDocDataObjectBuilder(consol);

			using (FreightDataRegistry.Instance.AllowCompanyTaxIDOverride.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Assert(!builder.Build().IsTaxIdModifiable);
			}

			using (FreightDataRegistry.Instance.AllowCompanyTaxIDOverride.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert(builder.Build().IsTaxIdModifiable);
			}
		}

		public virtual void TestPopulateSpecialInstructions()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_AgentsReference = "A128569";
			consol.JK_MasterBillNum = "MBL00023";
			consol.JK_CoLoadMasterBill = "CMBL00012";

			var specialInstructionsNote1 = consol.Notes.AddNew();
			specialInstructionsNote1.ST_Description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;
			specialInstructionsNote1.ST_NoteText = "special instructions 1";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_BookingReference = "B00588";
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var specialInstructionsNote2 = shipment.Notes.AddNew();
			specialInstructionsNote2.ST_Description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;
			specialInstructionsNote2.ST_NoteText = "special instructions 2";

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			AssertEquals("special instructions 1", carrierMessageData.SpecialInstructions);

			consol.JK_AgentType = Constants.AgentType.Direct;
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			AssertEquals("special instructions 2", carrierMessageData.SpecialInstructions);
		}

		public virtual void TestPopulateSpecialInstructionsForDRTConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Direct;

			var specialInstructionsNote1 = consol.Notes.AddNew();
			specialInstructionsNote1.ST_Description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;
			specialInstructionsNote1.ST_NoteText = "special instructions 1";

			AssertNoExceptionThrown(() =>
			{
				var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

				AssertEquals("", carrierMessageData.SpecialInstructions);
			});

			var shipment = consol.Shipments.AddNew();
			shipment.JS_BookingReference = "B00588";
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var specialInstructionsNote2 = shipment.Notes.AddNew();
			specialInstructionsNote2.ST_Description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;
			specialInstructionsNote2.ST_NoteText = "special instructions 2";

			AssertNoExceptionThrown(() =>
			{
				var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

				AssertEquals("special instructions 2", carrierMessageData.SpecialInstructions);
			});
		}

		public void TestPopulateSourceIdFromCarrierShipperReference()
		{
			var consol = CreateConsol();
			var entryNum = consol.Numbers.AddNew();
			entryNum.CE_EntryType = ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierShipperReference;
			entryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNum.CE_EntryIsSystemGenerated = true;
			entryNum.CE_EntryNum = "C0000005-V8";

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("C0000005-V8", carrierMessageData.SourceID);
		}

		public void TestPopulatePackageGrouping()
		{
			var consol = CreateConsol();

			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var builder = CreateDocDataObjectBuilder(consol);
				AssertEquals(Constants.PackageGrouping.Codes.GroupByShipment, builder.Build().PackageGrouping.Code);
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var builder = CreateDocDataObjectBuilder(consol);
				AssertEquals(Constants.PackageGrouping.Codes.DoNotGroup, builder.Build().PackageGrouping.Code);
			}

			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.DoNotGroup;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var builder = CreateDocDataObjectBuilder(consol);
				AssertEquals(Constants.PackageGrouping.Codes.DoNotGroup, builder.Build().PackageGrouping.Code);
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var builder = CreateDocDataObjectBuilder(consol);
				AssertEquals(Constants.PackageGrouping.Codes.DoNotGroup, builder.Build().PackageGrouping.Code);
			}

			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByPackLine;
			consol.JK_ConsolMode = Constants.ContainerModes.RollOnRollOff;
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var builder = CreateDocDataObjectBuilder(consol);
				AssertEquals(Constants.PackageGrouping.Codes.DoNotGroup, builder.Build().PackageGrouping.Code);
			}
		}

		public void TestPopulatePackingLines_IsNonContainerized()
		{
			CreateRefCountryRules(Constants.CountryCodes.Australia, Constants.CountryCodes.China, ZString.Empty, ZGuid.Empty, true);
			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;
			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByPackLine;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHG";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONTAINER1";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONTAINER2";
			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerNum = "CONTAINER3";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "SHIPMENT1";
			shipment1.JS_UnitOfWeight = Constants.Weight.Grams;
			shipment1.JS_UnitOfVolume = Constants.Volume.MegaLitre;
			shipment1.DetailedGoodsDescriptionNoteText = "SHIPMENT1 DetailedGoodsDescriptionNoteText";
			shipment1.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
			shipment1.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bag;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "SHIPMENT2";
			shipment2.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment2.JS_UnitOfVolume = Constants.Volume.Litre;
			shipment2.DetailedGoodsDescriptionNoteText = "SHIPMENT2 DetailedGoodsDescriptionNoteText";
			shipment2.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
			shipment2.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bag;

			shipment1.InnerPackLines.RemoveAndDeleteAll();
			shipment1.OuterPackLines.RemoveAndDeleteAll();
			shipment2.InnerPackLines.RemoveAndDeleteAll();
			shipment2.OuterPackLines.RemoveAndDeleteAll();

			var s1OuterPackLine1 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 10, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, "Group1 MarksAndNumbers", "Group1 DetailedDescription", true, -2, 3, Constants.Temperature.Fahrenheit);
			s1OuterPackLine1.JL_JC = container1.PK;
			var s1OuterPackLine2 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 20, Constants.PkgUnit.Keg, 20, Constants.Weight.Grams, 20, Constants.Volume.MegaLitre, "Group2 MarksAndNumbers", "Group2 DetailedDescription", true, 2, 5, Constants.Temperature.Fahrenheit);
			s1OuterPackLine2.JL_JC = container1.PK;
			var s1OuterPackLine3 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 30, Constants.PkgUnit.Pail, 30, Constants.Weight.Grams, 30, Constants.Volume.MegaLitre, "Group1 MarksAndNumbers", "Group1 DetailedDescription");
			s1OuterPackLine3.JL_JC = container2.PK;
			PopulatePackLine(shipment1.OuterPackLines.AddNew(), 40, Constants.PkgUnit.Pail, 40, Constants.Weight.Grams, 40, Constants.Volume.MegaLitre).JL_JC = ZGuid.Empty;
			PopulatePackLine(shipment1.OuterPackLines.AddNew(), 50, Constants.PkgUnit.Pail, 50, Constants.Weight.Grams, 50, Constants.Volume.MegaLitre).JL_JC = ZGuid.Empty;

			var s1InnerPackLine1 = PopulatePackLine(shipment1.InnerPackLines.AddNew(), 40, Constants.PkgUnit.Keg, 40, Constants.Weight.Kilograms, 40, Constants.Volume.CubicMetres);
			s1InnerPackLine1.JL_JL_OuterPackLine = s1OuterPackLine1.PK;
			var s1InnerPackLine2 = PopulatePackLine(shipment1.InnerPackLines.AddNew(), 50, Constants.PkgUnit.Keg, 50, Constants.Weight.Kilograms, 50, Constants.Volume.CubicMetres);
			s1InnerPackLine2.JL_JL_OuterPackLine = s1OuterPackLine1.PK;
			var s1InnerPackLine3 = PopulatePackLine(shipment1.InnerPackLines.AddNew(), 60, Constants.PkgUnit.Gross, 60, Constants.Weight.Grams, 60, Constants.Volume.MegaLitre);
			s1InnerPackLine3.JL_JL_OuterPackLine = s1OuterPackLine2.PK;
			var s1InnerPackLine4 = PopulatePackLine(shipment1.InnerPackLines.AddNew(), 70, Constants.PkgUnit.Keg, 70, Constants.Weight.Grams, 70, Constants.Volume.MegaLitre);
			s1InnerPackLine4.JL_JL_OuterPackLine = s1OuterPackLine2.PK;
			PopulatePackLine(shipment1.InnerPackLines.AddNew(), 80, Constants.PkgUnit.Keg, 80, Constants.Weight.Grams, 80, Constants.Volume.MegaLitre);
			PopulatePackLine(shipment1.InnerPackLines.AddNew(), 90, Constants.PkgUnit.Keg, 90, Constants.Weight.Grams, 90, Constants.Volume.MegaLitre);

			var shipment2OuterPackLine = shipment2.OuterPackLines.AddNew();

			var undgSubstance1 = Factory.New<UNDGSubstance>();
			undgSubstance1.DG_Code = "UNDG1";
			var undgSubstance2 = Factory.New<UNDGSubstance>();
			undgSubstance2.DG_Code = "UNDG2";

			var undg1 = shipment2OuterPackLine.UNDGs.AddNew();
			undg1.DI_DG = undgSubstance1.PK;
			undg1.DI_DGWeight = 0;
			undg1.DI_UnitOfWeight = Constants.Weight.Tonnes;
			undg1.DI_DGVolume = 100;
			undg1.DI_UnitOfVolume = Constants.Volume.CubicMetres;

			var undg2 = shipment2OuterPackLine.UNDGs.AddNew();
			undg2.DI_DG = undgSubstance2.PK;
			undg2.DI_DGWeight = 10;
			undg2.DI_UnitOfWeight = Constants.Weight.Grams;
			undg2.DI_DGVolume = 20;
			undg2.DI_UnitOfVolume = Constants.Volume.MegaLitre;

			var undg3 = shipment2OuterPackLine.UNDGs.AddNew();
			undg3.DI_DG = undgSubstance2.PK;
			undg3.DI_DGWeight = 10;
			undg3.DI_UnitOfWeight = Constants.Weight.Grams;
			undg3.DI_DGVolume = 20;
			undg3.DI_UnitOfVolume = Constants.Volume.MegaLitre;

			PopulatePackLine(shipment2OuterPackLine, 100, Constants.PkgUnit.Keg, 100, Constants.Weight.Kilograms, 100, Constants.Volume.CubicMetres, "Group1 MarksAndNumbers", "Group1 DetailedDescription", true, -2, 3, Constants.Temperature.Centigrade).JL_JC = container2.PK;
			PopulatePackLine(shipment2.OuterPackLines.AddNew(), 110, Constants.PkgUnit.Keg, 110, Constants.Weight.Kilograms, 110, Constants.Volume.CubicMetres, "Group1 MarksAndNumbers", "Group1 DetailedDescription", true, -2, 3, Constants.Temperature.Fahrenheit).JL_JC = container3.PK;
			PopulatePackLine(shipment2.OuterPackLines.AddNew(), 120, Constants.PkgUnit.Keg, 120, Constants.Weight.Kilograms, 120, Constants.Volume.CubicMetres, "Group2 MarksAndNumbers", "Group2 DetailedDescription", true, -2, 3, Constants.Temperature.Centigrade).JL_JC = container3.PK;
			PopulatePackLine(shipment2.OuterPackLines.AddNew(), 130, Constants.PkgUnit.Keg, 130, Constants.Weight.Kilograms, 130, Constants.Volume.CubicMetres).JL_JC = ZGuid.Empty;
			PopulatePackLine(shipment2.OuterPackLines.AddNew(), 140, Constants.PkgUnit.Keg, 140, Constants.Weight.Kilograms, 140, Constants.Volume.CubicMetres).JL_JC = ZGuid.Empty;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var builder = CreateDocDataObjectBuilder(consol);
				var carrierMessageData = builder.Build();

				var shipmentDO1 = carrierMessageData.Shipments.Cast<Shipment>().First(x => x.ShipmentID == "SHIPMENT1");
				var shipmentDO2 = carrierMessageData.Shipments.Cast<Shipment>().First(x => x.ShipmentID == "SHIPMENT2");

				Assert("Shipment > PackingLines > PackingLines will be used", carrierMessageData.Shipments.All(shipment => shipment.PackingLines.Any()));
				Assert("Shipment > PackingLines > PackingLines will be used", carrierMessageData.Shipments.All(shipment => shipment.PackingLines.All(packingLine => packingLine.PackingLines != null)));

				AssertEquals(4, shipmentDO1.PackingLines.Count);
				AssertEquals(3, shipmentDO2.PackingLines.Count);

				var shipment1GroupedPackingLine1 = shipmentDO1.PackingLines.First(x => x.GoodsDescription.StartsWith("Group1") && x.PackageType.Code == Constants.PkgUnit.Keg);
				var shipment1GroupedPackingLine2 = shipmentDO1.PackingLines.First(x => x.GoodsDescription.StartsWith("Group1") && x.PackageType.Code != Constants.PkgUnit.Keg);
				var shipment1GroupedPackingLine3 = shipmentDO1.PackingLines.First(x => x.GoodsDescription.StartsWith("Group2"));
				var shipment1GroupedPackingLine4 = shipmentDO1.PackingLines.First(x => x.GoodsDescription.StartsWith("SHIPMENT1 DetailedGoodsDescriptionNoteText"));
				var shipment2GroupedPackingLine1 = shipmentDO2.PackingLines.First(x => x.GoodsDescription.StartsWith("Group1"));
				var shipment2GroupedPackingLine2 = shipmentDO2.PackingLines.First(x => x.GoodsDescription.StartsWith("Group2"));
				var shipment2GroupedPackingLine3 = shipmentDO2.PackingLines.First(x => x.GoodsDescription.StartsWith("SHIPMENT2 DetailedGoodsDescriptionNoteText"));

				AssertEquals(90, shipment1GroupedPackingLine1.Quantity);
				AssertEquals(10M, shipment1GroupedPackingLine1.Weight.Value);
				AssertEquals(carrierMessageData.UnitOfWeight, shipment1GroupedPackingLine1.Weight.Unit.Code);
				Assert("Same as Grouped Packing Line's unit", shipment1GroupedPackingLine1.PackingLines.All(x => x.Weight.Unit.Code == shipment1GroupedPackingLine1.Weight.Unit.Code));
				Assert("Convert", shipment1GroupedPackingLine1.PackingLines.SelectMany(x => x.DangerousGoods).All(x => x.Weight.Unit.Code == shipment1GroupedPackingLine1.Weight.Unit.Code));
				AssertEquals(10M, shipment1GroupedPackingLine1.Volume.Value);
				AssertEquals(carrierMessageData.UnitOfVolume, shipment1GroupedPackingLine1.Volume.Unit.Code);
				Assert("Same as Grouped Packing Line's unit", shipment1GroupedPackingLine1.PackingLines.All(x => x.Volume.Unit.Code == shipment1GroupedPackingLine1.Volume.Unit.Code));
				Assert("Convert", shipment1GroupedPackingLine1.PackingLines.SelectMany(x => x.DangerousGoods).All(x => x.Volume.Unit.Code == shipment1GroupedPackingLine1.Volume.Unit.Code));
				AssertEquals("Inner Package Type", Constants.PkgUnit.Keg, shipment1GroupedPackingLine1.PackageType.Code);

				AssertEquals("Group1 DetailedDescription", shipment1GroupedPackingLine1.GoodsDescription);
				AssertEquals("Group1 MarksAndNumbers", shipment1GroupedPackingLine1.MarksAndNumbers);

				Assert(shipment1GroupedPackingLine1.RequiresTemperatureControl);
				AssertEquals(-2M, shipment1GroupedPackingLine1.TemperatureMinimum.Value);
				AssertEquals(3M, shipment1GroupedPackingLine1.TemperatureMaximum.Value);
				AssertEquals(Constants.Temperature.Fahrenheit, shipment1GroupedPackingLine1.TemperatureMinimum.Unit.Code);

				var shipment1GroupedPackingLine1ConsolidatePackingLine = shipment1GroupedPackingLine1.PackingLines.First();
				AssertEquals("CONTAINER1", shipment1GroupedPackingLine1ConsolidatePackingLine.ContainerNumber);
				AssertEquals(90, shipment1GroupedPackingLine1ConsolidatePackingLine.Quantity);
				AssertEquals(10M, shipment1GroupedPackingLine1ConsolidatePackingLine.Weight.Value);
				AssertEquals(shipment1GroupedPackingLine1.Weight.Unit.Code, shipment1GroupedPackingLine1ConsolidatePackingLine.Weight.Unit.Code);
				AssertEquals(10M, shipment1GroupedPackingLine1ConsolidatePackingLine.Volume.Value);
				AssertEquals(shipment1GroupedPackingLine1.Volume.Unit.Code, shipment1GroupedPackingLine1ConsolidatePackingLine.Volume.Unit.Code);

				AssertEquals(30, shipment1GroupedPackingLine2.Quantity);
				AssertEquals(0.03M, shipment1GroupedPackingLine2.Weight.Value);
				AssertEquals(carrierMessageData.UnitOfWeight, shipment1GroupedPackingLine2.Weight.Unit.Code);
				Assert("Same as Grouped Packing Line's unit", shipment1GroupedPackingLine2.PackingLines.All(x => x.Weight.Unit.Code == shipment1GroupedPackingLine2.Weight.Unit.Code));
				Assert("Convert", shipment1GroupedPackingLine2.PackingLines.SelectMany(x => x.DangerousGoods).All(x => x.Weight.Unit.Code == shipment1GroupedPackingLine2.Weight.Unit.Code));
				AssertEquals(30000M, shipment1GroupedPackingLine2.Volume.Value);
				AssertEquals(carrierMessageData.UnitOfVolume, shipment1GroupedPackingLine2.Volume.Unit.Code);
				Assert("Same as Grouped Packing Line's unit", shipment1GroupedPackingLine2.PackingLines.All(x => x.Volume.Unit.Code == shipment1GroupedPackingLine2.Volume.Unit.Code));
				Assert("Convert", shipment1GroupedPackingLine2.PackingLines.SelectMany(x => x.DangerousGoods).All(x => x.Volume.Unit.Code == shipment1GroupedPackingLine2.Volume.Unit.Code));
				AssertEquals(Constants.PkgUnit.Pail, shipment1GroupedPackingLine2.PackageType.Code);

				AssertEquals("Group1 DetailedDescription", shipment1GroupedPackingLine2.GoodsDescription);
				AssertEquals("Group1 MarksAndNumbers", shipment1GroupedPackingLine2.MarksAndNumbers);

				Assert(!shipment1GroupedPackingLine2.RequiresTemperatureControl);

				var shipment1GroupedPackingLine2ConsolidatePackingLine = shipment1GroupedPackingLine2.PackingLines.First();
				AssertEquals("CONTAINER2", shipment1GroupedPackingLine2ConsolidatePackingLine.ContainerNumber);
				AssertEquals(30, shipment1GroupedPackingLine2ConsolidatePackingLine.Quantity);
				AssertEquals(0.03M, shipment1GroupedPackingLine2ConsolidatePackingLine.Weight.Value);
				AssertEquals(shipment1GroupedPackingLine2.Weight.Unit.Code, shipment1GroupedPackingLine2ConsolidatePackingLine.Weight.Unit.Code);
				AssertEquals(30000M, shipment1GroupedPackingLine2ConsolidatePackingLine.Volume.Value);
				AssertEquals(shipment1GroupedPackingLine2.Volume.Unit.Code, shipment1GroupedPackingLine2ConsolidatePackingLine.Volume.Unit.Code);

				AssertEquals(130, shipment1GroupedPackingLine3.Quantity);
				AssertEquals(0.02M, shipment1GroupedPackingLine3.Weight.Value);
				AssertEquals(carrierMessageData.UnitOfWeight, shipment1GroupedPackingLine3.Weight.Unit.Code);
				Assert("Same as Grouped Packing Line's unit", shipment1GroupedPackingLine3.PackingLines.All(x => x.Weight.Unit.Code == shipment1GroupedPackingLine3.Weight.Unit.Code));
				Assert("Convert", shipment1GroupedPackingLine3.PackingLines.SelectMany(x => x.DangerousGoods).All(x => x.Weight.Unit.Code == shipment1GroupedPackingLine3.Weight.Unit.Code));
				AssertEquals(20000M, shipment1GroupedPackingLine3.Volume.Value);
				AssertEquals(carrierMessageData.UnitOfVolume, shipment1GroupedPackingLine3.Volume.Unit.Code);
				Assert("Same as Grouped Packing Line's unit", shipment1GroupedPackingLine3.PackingLines.All(x => x.Volume.Unit.Code == shipment1GroupedPackingLine3.Volume.Unit.Code));
				Assert("Convert", shipment1GroupedPackingLine3.PackingLines.SelectMany(x => x.DangerousGoods).All(x => x.Volume.Unit.Code == shipment1GroupedPackingLine3.Volume.Unit.Code));
				AssertEquals("Inner Package Type different", Constants.PkgUnit.Package, shipment1GroupedPackingLine3.PackageType.Code);

				AssertEquals("Group2 DetailedDescription", shipment1GroupedPackingLine3.GoodsDescription);
				AssertEquals("Group2 MarksAndNumbers", shipment1GroupedPackingLine3.MarksAndNumbers);

				Assert(shipment1GroupedPackingLine3.RequiresTemperatureControl);
				AssertEquals(2M, shipment1GroupedPackingLine3.TemperatureMinimum.Value);
				AssertEquals(5M, shipment1GroupedPackingLine3.TemperatureMaximum.Value);
				AssertEquals(Constants.Temperature.Fahrenheit, shipment1GroupedPackingLine3.TemperatureMinimum.Unit.Code);

				var shipment1GroupedPackingLine3ConsolidatePackingLine = shipment1GroupedPackingLine3.PackingLines.First();
				AssertEquals("CONTAINER1", shipment1GroupedPackingLine3ConsolidatePackingLine.ContainerNumber);
				AssertEquals(130, shipment1GroupedPackingLine3ConsolidatePackingLine.Quantity);
				AssertEquals(0.02M, shipment1GroupedPackingLine3ConsolidatePackingLine.Weight.Value);
				AssertEquals(shipment1GroupedPackingLine3.Weight.Unit.Code, shipment1GroupedPackingLine3ConsolidatePackingLine.Weight.Unit.Code);
				AssertEquals(20000M, shipment1GroupedPackingLine3ConsolidatePackingLine.Volume.Value);
				AssertEquals(shipment1GroupedPackingLine3.Volume.Unit.Code, shipment1GroupedPackingLine3ConsolidatePackingLine.Volume.Unit.Code);

				AssertEquals(90, shipment1GroupedPackingLine4.Quantity);
				AssertEquals(0.09M, shipment1GroupedPackingLine4.Weight.Value);
				AssertEquals(carrierMessageData.UnitOfWeight, shipment1GroupedPackingLine4.Weight.Unit.Code);
				Assert("Same as Grouped Packing Line's unit", shipment1GroupedPackingLine4.PackingLines.All(x => x.Weight.Unit.Code == shipment1GroupedPackingLine4.Weight.Unit.Code));
				AssertEquals(90000M, shipment1GroupedPackingLine4.Volume.Value);
				AssertEquals(carrierMessageData.UnitOfVolume, shipment1GroupedPackingLine4.Volume.Unit.Code);
				Assert("Same as Grouped Packing Line's unit", shipment1GroupedPackingLine4.PackingLines.All(x => x.Volume.Unit.Code == shipment1GroupedPackingLine4.Volume.Unit.Code));

				AssertEquals("SHIPMENT1 DetailedGoodsDescriptionNoteText", shipment1GroupedPackingLine4.GoodsDescription);
				AssertEquals("", shipment1GroupedPackingLine4.MarksAndNumbers);

				Assert(!shipment1GroupedPackingLine4.RequiresTemperatureControl);

				var shipment1GroupedPackingLine4ConsolidatePackingLine = shipment1GroupedPackingLine4.PackingLines.First();
				AssertEquals("", shipment1GroupedPackingLine4ConsolidatePackingLine.ContainerNumber);
				AssertEquals(90, shipment1GroupedPackingLine4ConsolidatePackingLine.Quantity);
				AssertEquals(0.09M, shipment1GroupedPackingLine4ConsolidatePackingLine.Weight.Value);
				AssertEquals(shipment1GroupedPackingLine4.Weight.Unit.Code, shipment1GroupedPackingLine4ConsolidatePackingLine.Weight.Unit.Code);
				AssertEquals(90000M, shipment1GroupedPackingLine4ConsolidatePackingLine.Volume.Value);
				AssertEquals(shipment1GroupedPackingLine4.Volume.Unit.Code, shipment1GroupedPackingLine4ConsolidatePackingLine.Volume.Unit.Code);

				AssertEquals(210, shipment2GroupedPackingLine1.Quantity);
				AssertEquals(210M, shipment2GroupedPackingLine1.Weight.Value);
				AssertEquals(carrierMessageData.UnitOfWeight, shipment2GroupedPackingLine1.Weight.Unit.Code);
				Assert("Same as Grouped Packing Line's unit", shipment2GroupedPackingLine1.PackingLines.All(x => x.Volume.Unit.Code == shipment2GroupedPackingLine1.Volume.Unit.Code));
				Assert("Convert", shipment2GroupedPackingLine1.PackingLines.SelectMany(x => x.DangerousGoods).All(x => x.Weight.Unit.Code == shipment2GroupedPackingLine1.Weight.Unit.Code));
				AssertEquals(210M, shipment2GroupedPackingLine1.Volume.Value);
				AssertEquals(carrierMessageData.UnitOfVolume, shipment2GroupedPackingLine1.Volume.Unit.Code);
				Assert("Same as Grouped Packing Line's unit", shipment2GroupedPackingLine1.PackingLines.All(x => x.Volume.Unit.Code == shipment2GroupedPackingLine1.Volume.Unit.Code));
				Assert("Convert", shipment2GroupedPackingLine1.PackingLines.SelectMany(x => x.DangerousGoods).All(x => x.Volume.Unit.Code == shipment2GroupedPackingLine1.Volume.Unit.Code));
				AssertEquals(Constants.PkgUnit.Keg, shipment2GroupedPackingLine1.PackageType.Code);

				AssertEquals("Group1 DetailedDescription", shipment2GroupedPackingLine1.GoodsDescription);
				AssertEquals("Group1 MarksAndNumbers", shipment2GroupedPackingLine1.MarksAndNumbers);

				Assert(shipment2GroupedPackingLine1.RequiresTemperatureControl);
				AssertEquals(-2M, shipment2GroupedPackingLine1.TemperatureMinimum.Value);
				AssertEquals(Constants.Temperature.Convert(3, Constants.Temperature.Fahrenheit, Constants.Temperature.Centigrade), shipment2GroupedPackingLine1.TemperatureMaximum.Value);
				AssertEquals(Constants.Temperature.Centigrade, shipment2GroupedPackingLine1.TemperatureMinimum.Unit.Code);

				var shipment2GroupedPackingLine1ConsolidatePackingLine1 = shipment2GroupedPackingLine1.PackingLines.First(x => x.ContainerNumber == "CONTAINER2");
				AssertEquals("CONTAINER2", shipment2GroupedPackingLine1ConsolidatePackingLine1.ContainerNumber);
				AssertEquals(100, shipment2GroupedPackingLine1ConsolidatePackingLine1.Quantity);
				AssertEquals(100M, shipment2GroupedPackingLine1ConsolidatePackingLine1.Weight.Value);
				AssertEquals(shipment2GroupedPackingLine1.Weight.Unit.Code, shipment2GroupedPackingLine1ConsolidatePackingLine1.Weight.Unit.Code);
				AssertEquals(100M, shipment2GroupedPackingLine1ConsolidatePackingLine1.Volume.Value);
				AssertEquals(shipment2GroupedPackingLine1.Volume.Unit.Code, shipment2GroupedPackingLine1ConsolidatePackingLine1.Volume.Unit.Code);

				var shipment2GroupedPackingLine1ConsolidatePackingLine2 = shipment2GroupedPackingLine1.PackingLines.First(x => x.ContainerNumber == "CONTAINER3");
				AssertEquals("CONTAINER3", shipment2GroupedPackingLine1ConsolidatePackingLine2.ContainerNumber);
				AssertEquals(110, shipment2GroupedPackingLine1ConsolidatePackingLine2.Quantity);
				AssertEquals(110M, shipment2GroupedPackingLine1ConsolidatePackingLine2.Weight.Value);
				AssertEquals(shipment2GroupedPackingLine1.Weight.Unit.Code, shipment2GroupedPackingLine1ConsolidatePackingLine2.Weight.Unit.Code);
				AssertEquals(110M, shipment2GroupedPackingLine1ConsolidatePackingLine2.Volume.Value);
				AssertEquals(shipment2GroupedPackingLine1.Volume.Unit.Code, shipment2GroupedPackingLine1ConsolidatePackingLine2.Volume.Unit.Code);

				var dgPackLine = shipment2GroupedPackingLine1.PackingLines.First(x => x.DangerousGoods.Any());
				AssertEquals(2, dgPackLine.DangerousGoods.Count);

				var dgDO1 = dgPackLine.DangerousGoods.First(x => x.Code == "UNDG1");
				var dgDO2 = dgPackLine.DangerousGoods.First(x => x.Code == "UNDG2");

				AssertEquals("Multiple DG lines, won't fallback", 0M, dgDO1.Weight.Value);
				AssertEquals("Weight.Unit.Code Same as Parent PackingLine", dgPackLine.Weight.Unit.Code, dgDO1.Weight.Unit.Code);
				AssertEquals("Volume.Value", 100M, dgDO1.Volume.Value);
				AssertEquals("Volume.Unit.Code Same as Parent PackingLine", dgPackLine.Volume.Unit.Code, dgDO1.Volume.Unit.Code);

				AssertEquals(0.02M, dgDO2.Weight.Value);
				AssertEquals("Weight.Unit.Code Same as Parent PackingLine", dgPackLine.Weight.Unit.Code, dgDO2.Weight.Unit.Code);
				AssertEquals(40000M, dgDO2.Volume.Value);
				AssertEquals("Volume.Unit.Code Same as Parent PackingLine", dgPackLine.Volume.Unit.Code, dgDO2.Volume.Unit.Code);

				AssertEquals(120, shipment2GroupedPackingLine2.Quantity);
				AssertEquals(120M, shipment2GroupedPackingLine2.Weight.Value);
				AssertEquals(carrierMessageData.UnitOfWeight, shipment2GroupedPackingLine2.Weight.Unit.Code);
				Assert("Same as Grouped Packing Line's unit", shipment2GroupedPackingLine2.PackingLines.All(x => x.Volume.Unit.Code == shipment2GroupedPackingLine2.Volume.Unit.Code));
				Assert("Convert", shipment2GroupedPackingLine2.PackingLines.SelectMany(x => x.DangerousGoods).All(x => x.Weight.Unit.Code == shipment2GroupedPackingLine2.Weight.Unit.Code));
				AssertEquals(120M, shipment2GroupedPackingLine2.Volume.Value);
				AssertEquals(carrierMessageData.UnitOfVolume, shipment2GroupedPackingLine2.Volume.Unit.Code);
				Assert("Same as Grouped Packing Line's unit", shipment2GroupedPackingLine2.PackingLines.All(x => x.Volume.Unit.Code == shipment2GroupedPackingLine2.Volume.Unit.Code));
				Assert("Convert", shipment2GroupedPackingLine2.PackingLines.SelectMany(x => x.DangerousGoods).All(x => x.Volume.Unit.Code == shipment2GroupedPackingLine2.Volume.Unit.Code));
				AssertEquals(Constants.PkgUnit.Keg, shipment2GroupedPackingLine2.PackageType.Code);

				AssertEquals("Group2 DetailedDescription", shipment2GroupedPackingLine2.GoodsDescription);
				AssertEquals("Group2 MarksAndNumbers", shipment2GroupedPackingLine2.MarksAndNumbers);

				Assert(shipment2GroupedPackingLine2.RequiresTemperatureControl);
				AssertEquals(-2M, shipment2GroupedPackingLine2.TemperatureMinimum.Value);
				AssertEquals(3M, shipment2GroupedPackingLine2.TemperatureMaximum.Value);
				AssertEquals(Constants.Temperature.Centigrade, shipment2GroupedPackingLine2.TemperatureMinimum.Unit.Code);

				var shipment2GroupedPackingLine2ConsolidatePackingLine = shipment2GroupedPackingLine2.PackingLines.First();
				AssertEquals("CONTAINER3", shipment2GroupedPackingLine2ConsolidatePackingLine.ContainerNumber);
				AssertEquals(120, shipment2GroupedPackingLine2ConsolidatePackingLine.Quantity);
				AssertEquals(120M, shipment2GroupedPackingLine2ConsolidatePackingLine.Weight.Value);
				AssertEquals(shipment2GroupedPackingLine2.Weight.Unit.Code, shipment2GroupedPackingLine2ConsolidatePackingLine.Weight.Unit.Code);
				AssertEquals(120M, shipment2GroupedPackingLine2ConsolidatePackingLine.Volume.Value);
				AssertEquals(shipment2GroupedPackingLine2.Volume.Unit.Code, shipment2GroupedPackingLine2ConsolidatePackingLine.Volume.Unit.Code);

				AssertEquals(270, shipment2GroupedPackingLine3.Quantity);
				AssertEquals(270M, shipment2GroupedPackingLine3.Weight.Value);
				AssertEquals(carrierMessageData.UnitOfWeight, shipment2GroupedPackingLine3.Weight.Unit.Code);
				Assert("Same as Grouped Packing Line's unit", shipment2GroupedPackingLine3.PackingLines.All(x => x.Volume.Unit.Code == shipment2GroupedPackingLine3.Volume.Unit.Code));
				AssertEquals(270M, shipment2GroupedPackingLine3.Volume.Value);
				AssertEquals(carrierMessageData.UnitOfVolume, shipment2GroupedPackingLine3.Volume.Unit.Code);
				Assert("Same as Grouped Packing Line's unit", shipment2GroupedPackingLine3.PackingLines.All(x => x.Volume.Unit.Code == shipment2GroupedPackingLine3.Volume.Unit.Code));
				AssertEquals(Constants.PkgUnit.Keg, shipment2GroupedPackingLine3.PackageType.Code);

				AssertEquals("SHIPMENT2 DetailedGoodsDescriptionNoteText", shipment2GroupedPackingLine3.GoodsDescription);
				AssertEquals("", shipment2GroupedPackingLine3.MarksAndNumbers);

				Assert(!shipment2GroupedPackingLine3.RequiresTemperatureControl);

				var shipment2GroupedPackingLine3ConsolidatePackingLine = shipment2GroupedPackingLine3.PackingLines.First();
				AssertEquals("", shipment2GroupedPackingLine3ConsolidatePackingLine.ContainerNumber);
				AssertEquals(270, shipment2GroupedPackingLine3ConsolidatePackingLine.Quantity);
				AssertEquals(270M, shipment2GroupedPackingLine3ConsolidatePackingLine.Weight.Value);
				AssertEquals(shipment2GroupedPackingLine3.Weight.Unit.Code, shipment2GroupedPackingLine3ConsolidatePackingLine.Weight.Unit.Code);
				AssertEquals(270M, shipment2GroupedPackingLine3ConsolidatePackingLine.Volume.Value);
				AssertEquals(shipment2GroupedPackingLine3.Volume.Unit.Code, shipment2GroupedPackingLine3ConsolidatePackingLine.Volume.Unit.Code);

				AssertEquals(2, carrierMessageData.Containers.Cast<Container>().First(x => x.Number == "CONTAINER1").PackingLines.Count);
				AssertEquals(2, carrierMessageData.Containers.Cast<Container>().First(x => x.Number == "CONTAINER2").PackingLines.Count);
				AssertEquals(2, carrierMessageData.Containers.Cast<Container>().First(x => x.Number == "CONTAINER3").PackingLines.Count);
			}
		}

		#endregion

		#region Charges

		public void TestPopulateCharges()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			CombineAssertions(() =>
			{
				Assert("OptionalChargeBasicFreight.IsPrepaid", carrierMessageData.OptionalChargeBasicFreight.IsPrepaid);
				Assert("OptionalChargeBasicFreight.IsCollect", !carrierMessageData.OptionalChargeBasicFreight.IsCollect);

				Assert("OptionalChargeDestinationHaulage.IsPrepaid", !carrierMessageData.OptionalChargeDestinationHaulage.IsPrepaid);
				Assert("OptionalChargeDestinationHaulage.IsCollect", !carrierMessageData.OptionalChargeDestinationHaulage.IsCollect);

				Assert("OptionalChargeDestinationPort.IsPrepaid", !carrierMessageData.OptionalChargeDestinationPort.IsPrepaid);
				Assert("OptionalChargeDestinationPort.IsCollect", !carrierMessageData.OptionalChargeDestinationPort.IsCollect);

				Assert("OptionalChargeOriginHaulage.IsPrepaid", !carrierMessageData.OptionalChargeOriginHaulage.IsPrepaid);
				Assert("OptionalChargeOriginHaulage.IsCollect", !carrierMessageData.OptionalChargeOriginHaulage.IsCollect);

				Assert("OptionalChargeOriginPort.IsPrepaid", !carrierMessageData.OptionalChargeOriginPort.IsPrepaid);
				Assert("OptionalChargeOriginPort.IsCollect", !carrierMessageData.OptionalChargeOriginPort.IsCollect);
			});
		}

		public void TestChargesValidation()
		{
			var messageError = "One of the three types[Prepaid, Collect, PayableElsewhere] must be selected for 'Freight Charges'.";
			var warningIfIsPayableElsewhereSelected = "For carriers not supporting 'Payable Elsewhere', 'Collect' will be sent with the chosen Freight Payable At location.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			CombineAssertions(() =>
			{
				Assert("OptionalChargeBasicFreight.IsPrepaid", !carrierMessageData.OptionalChargeBasicFreight.IsPrepaid);
				Assert("OptionalChargeBasicFreight.IsCollect", !carrierMessageData.OptionalChargeBasicFreight.IsCollect);
				Assert("OptionalChargeBasicFreight.IsPayableElsewhere", !carrierMessageData.OptionalChargeBasicFreight.IsPayableElsewhere);

				AssertHasMessageError(((OptionalCharge)carrierMessageData.OptionalChargeBasicFreight).IsPrepaidInfo, messageError);
				AssertHasMessageError(((OptionalCharge)carrierMessageData.OptionalChargeBasicFreight).IsCollectInfo, messageError);
				AssertHasMessageError(((OptionalCharge)carrierMessageData.OptionalChargeBasicFreight).IsPayableElsewhereInfo, messageError);
				AssertNoWarning(((OptionalCharge)carrierMessageData.OptionalChargeBasicFreight).IsPayableElsewhereInfo, warningIfIsPayableElsewhereSelected);
			});

			consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			CombineAssertions(() =>
			{
				Assert("OptionalChargeBasicFreight.IsPrepaid", carrierMessageData.OptionalChargeBasicFreight.IsPrepaid);
				Assert("OptionalChargeBasicFreight.IsCollect", !carrierMessageData.OptionalChargeBasicFreight.IsCollect);
				Assert("OptionalChargeBasicFreight.IsPayableElsewhere", !carrierMessageData.OptionalChargeBasicFreight.IsPayableElsewhere);

				AssertNoMessageError(((OptionalCharge)carrierMessageData.OptionalChargeBasicFreight).IsPrepaidInfo, messageError);
				AssertNoMessageError(((OptionalCharge)carrierMessageData.OptionalChargeBasicFreight).IsCollectInfo, messageError);
				AssertNoMessageError(((OptionalCharge)carrierMessageData.OptionalChargeBasicFreight).IsPayableElsewhereInfo, messageError);
				AssertNoWarning(((OptionalCharge)carrierMessageData.OptionalChargeBasicFreight).IsPayableElsewhereInfo, warningIfIsPayableElsewhereSelected);
			});

			consol.JK_PrepaidCollect = Core.Constants.PaymentType.Collect;
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			CombineAssertions(() =>
			{
				Assert("OptionalChargeBasicFreight.IsPrepaid", !carrierMessageData.OptionalChargeBasicFreight.IsPrepaid);
				Assert("OptionalChargeBasicFreight.IsCollect", carrierMessageData.OptionalChargeBasicFreight.IsCollect);
				Assert("OptionalChargeBasicFreight.IsPayableElsewhere", !carrierMessageData.OptionalChargeBasicFreight.IsPayableElsewhere);

				AssertNoMessageError(((OptionalCharge)carrierMessageData.OptionalChargeBasicFreight).IsPrepaidInfo, messageError);
				AssertNoMessageError(((OptionalCharge)carrierMessageData.OptionalChargeBasicFreight).IsCollectInfo, messageError);
				AssertNoMessageError(((OptionalCharge)carrierMessageData.OptionalChargeBasicFreight).IsPayableElsewhereInfo, messageError);
				AssertNoWarning(((OptionalCharge)carrierMessageData.OptionalChargeBasicFreight).IsPayableElsewhereInfo, warningIfIsPayableElsewhereSelected);
			});

			((OptionalCharge)carrierMessageData.OptionalChargeBasicFreight).IsPayableElsewhere = true;
			CombineAssertions(() =>
			{
				Assert("OptionalChargeBasicFreight.IsPrepaid", !carrierMessageData.OptionalChargeBasicFreight.IsPrepaid);
				Assert("OptionalChargeBasicFreight.IsCollect", !carrierMessageData.OptionalChargeBasicFreight.IsCollect);
				Assert("OptionalChargeBasicFreight.IsPayableElsewhere", carrierMessageData.OptionalChargeBasicFreight.IsPayableElsewhere);

				AssertNoMessageError(((OptionalCharge)carrierMessageData.OptionalChargeBasicFreight).IsPrepaidInfo, messageError);
				AssertNoMessageError(((OptionalCharge)carrierMessageData.OptionalChargeBasicFreight).IsCollectInfo, messageError);
				AssertNoMessageError(((OptionalCharge)carrierMessageData.OptionalChargeBasicFreight).IsPayableElsewhereInfo, messageError);
				AssertHasWarning(((OptionalCharge)carrierMessageData.OptionalChargeBasicFreight).IsPayableElsewhereInfo, warningIfIsPayableElsewhereSelected);
			});
		}

		#endregion

		#region Freight Payer

		public void TestPopulateFreightPayer()
		{
			var frtChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			frtChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			frtChargeCode.AC_Code = "MYFREIGHT";
			frtChargeCode.AC_ChargeType = Constants.ChargeType.Overhead;
			frtChargeCode.AC_Desc = "Description";
			frtChargeCode.AC_ChargeGroup = "FRT";

			var nonFrtChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			nonFrtChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			nonFrtChargeCode.AC_Code = "NOTFREIGHT";
			nonFrtChargeCode.AC_ChargeType = Constants.ChargeType.Margin;
			nonFrtChargeCode.AC_Desc = "BLAH";
			nonFrtChargeCode.AC_ChargeGroup = "ORG";

			Env.Registry.FreightChargeCode = frtChargeCode.PK.ToGuid();

			Factory.Save();

			var consol = CreateConsol();
			var consolCharge = Factory.NewWithValidTestData<JobCharge>();
			consolCharge.JR_AC = frtChargeCode.PK;
			consolCharge.JR_JH = consol.PK;
			consolCharge.JR_JH_InternalJob = consol.PK;

			var shipmentCharge = Factory.NewWithValidTestData<JobCharge>();
			shipmentCharge.JR_AC = frtChargeCode.PK;
			shipmentCharge.JR_JH = consol.Shipments[0].PK;
			shipmentCharge.JR_JH_InternalJob = consol.Shipments[0].PK;

			var consolCost = (BusinessObject)Factory.New<IJobConsolCost>();
			consolCost[JobConsolCostSchema.E6_AC_ChargeCode] = frtChargeCode.PK;
			consolCost[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;
			consolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			consolCost[JobConsolCostSchema.E6_ParentID] = consol.PK;

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			AssertEquals("Unit 399", carrierMessageData.FreightPayer.AddressLine1);
			AssertEquals("I'm Receiving Stuff", carrierMessageData.FreightPayer.CompanyName);

			frtChargeCode.AC_Code = "FRT";
			consolCost[JobConsolCostSchema.E6_AC_ChargeCode] = frtChargeCode.PK;
			consolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			consolCost[JobConsolCostSchema.E6_ParentID] = consol.PK;
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			AssertEquals("Unit 399", carrierMessageData.FreightPayer.AddressLine1);
			AssertEquals("I'm Receiving Stuff", carrierMessageData.FreightPayer.CompanyName);

			consolCost[JobConsolCostSchema.E6_AC_ChargeCode] = nonFrtChargeCode.PK;
			consolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			consolCost[JobConsolCostSchema.E6_ParentID] = consol.PK;
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			AssertEquals("", carrierMessageData.FreightPayer.AddressLine1);
			AssertEquals("", carrierMessageData.FreightPayer.CompanyName);

			var freightPayer = Factory.New<OrgHeader>();
			freightPayer.OH_FullName = "I'm paying the freight";
			freightPayer.OH_RL_NKClosestPort = "AUMEL";
			freightPayer.MainAddress.Address1 = "FreightPayer";
			freightPayer.MainAddress.Address2 = "2 Pay Street";
			freightPayer.MainAddress.City = "Cashville";
			freightPayer.MainAddress.Postcode = "4999";
			freightPayer.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.FreightPayerDocumentaryAddress.E2_OA_Address = freightPayer.MainAddress.PK;

			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			AssertEquals("FreightPayer", carrierMessageData.FreightPayer.AddressLine1);
			AssertEquals("I'm paying the freight", carrierMessageData.FreightPayer.CompanyName);
		}

		#endregion

		#region Parties

		public void TestPartiesCompanyNameMaxLengthValidation()
		{
			var consol = CreateConsol();
			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			carrierMessageData.IsDoorDelivery = true;
			carrierMessageData.IsDoorPickup = true;

			CombineAssertions(() =>
			{
				AssertAddressCompanyNameLength("Shipper", carrierMessageData.Shipper);
				AssertAddressCompanyNameLength("Recipient", carrierMessageData.Recipient);
				AssertAddressCompanyNameLength("Consignee", carrierMessageData.Consignee);
				AssertAddressCompanyNameLength("NotifyParty", carrierMessageData.NotifyParty);
				AssertAddressCompanyNameLength("NotifyParty2", carrierMessageData.NotifyParty2);
				AssertAddressCompanyNameLength("NotifyParty3", carrierMessageData.NotifyParty3);
				AssertAddressCompanyNameLength("Forwarder", carrierMessageData.Forwarder);
				AssertAddressCompanyNameLength("Buyer", carrierMessageData.Buyer);
				AssertAddressCompanyNameLength("FreightPayer", carrierMessageData.FreightPayer);
				AssertAddressCompanyNameLength("PickupFrom", carrierMessageData.PickupFrom);
				AssertAddressCompanyNameLength("DeliverTo", carrierMessageData.DeliverTo);
			});
		}

		public void TestPartiesContactNameIsValid()
		{
			var consol = CreateConsol();
			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			carrierMessageData.IsDoorDelivery = true;
			carrierMessageData.IsDoorPickup = true;

			CombineAssertions(() =>
			{
				AssertAddressContactNameValid("Shipper", carrierMessageData.Shipper);
				AssertAddressContactNameValid("Consignee", carrierMessageData.Consignee);
				AssertAddressContactNameValid("NotifyParty", carrierMessageData.NotifyParty);
				AssertAddressContactNameValid("NotifyParty2", carrierMessageData.NotifyParty2);
				AssertAddressContactNameValid("NotifyParty3", carrierMessageData.NotifyParty3);
				AssertAddressContactNameValid("Forwarder", carrierMessageData.Forwarder);
				AssertAddressContactNameValid("Buyer", carrierMessageData.Buyer);
				AssertAddressContactNameValid("FreightPayer", carrierMessageData.FreightPayer);
				AssertAddressContactNameValid("PickupFrom", carrierMessageData.PickupFrom);
				AssertAddressContactNameValid("DeliverTo", carrierMessageData.DeliverTo);
			});
		}

		public void TestRecipientPartyNameAndAddressValidation()
		{
			var messageError = "{0} party name and address information is required.";
			var consol = CreateConsol();
			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			carrierMessageData.Recipient.CompanyName = string.Empty;
			AssertHasMessageError(carrierMessageData.Recipient.CompanyNameInfo, string.Format(messageError, "Carrier"));

			carrierMessageData.Recipient.CompanyName = "Line 1";
			AssertNoMessageError(carrierMessageData.Recipient.CompanyNameInfo, string.Format(messageError, "Carrier"));

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			carrierMessageData.Recipient.AddressLine1 = "Address 1";
			carrierMessageData.Recipient.AddressLine2 = "Address 2";
			carrierMessageData.Recipient.Country.Code = "CN";
			carrierMessageData.Recipient.CompanyName = string.Empty;
			AssertHasMessageError(carrierMessageData.Recipient.CompanyNameInfo, string.Format(messageError, "Co-Loader"));

			carrierMessageData.Recipient.CompanyName = "Line 1";
			AssertNoMessageError(carrierMessageData.Recipient.CompanyNameInfo, string.Format(messageError, "Co-Loader"));

			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			carrierMessageData.Recipient.AddressLine1 = "Address 1";
			carrierMessageData.Recipient.AddressLine2 = "Address 2";
			carrierMessageData.Recipient.Country.Code = "CN";
			carrierMessageData.Recipient.CompanyName = string.Empty;
			AssertHasMessageError(carrierMessageData.Recipient.CompanyNameInfo, string.Format(messageError, "Gateway Co-Loader"));

			carrierMessageData.Recipient.CompanyName = "Line 1";
			AssertNoMessageError(carrierMessageData.Recipient.CompanyNameInfo, string.Format(messageError, "Gateway Co-Loader"));
		}

		#endregion

		#region Transports

		public void TestTransportsValidation()
		{
			var errorMessage2 = "Main transport leg mode must be SEA.";
			var errorMessage3 = "ETD is required";
			var errorMessage4 = "Main Sea leg is required.";

			var consol = CreateConsol();

			var data = CreateDocDataObjectBuilder(consol).Build();
			var mainTransport = data.Transports.Main;
			AssertNoMessageError(((CodeDescription)mainTransport.Mode).CodeInfo, errorMessage2);
			AssertNoMessageError(mainTransport.ETDInfo, errorMessage3);
			AssertNoMessageError(data.ErrorPlaceHolderInfo, errorMessage4);

			var mainTransportRaw = consol.Transports.Cast<Freight.Business.Transport>().First(t => t.JW_TransportType == Constants.TransportPlanningType.MainVessel);
			mainTransportRaw.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			mainTransportRaw.JW_TransportMode = Constants.TransportModes.Road;
			mainTransportRaw.JW_ETD = ZDateTime.Empty;
			data = CreateDocDataObjectBuilder(consol).Build();
			mainTransport = data.Transports.Main;
			AssertHasMessageError(((CodeDescription)mainTransport.Mode).CodeInfo, errorMessage2);
			AssertHasMessageError(mainTransport.ETDInfo, errorMessage3);
			AssertNoMessageError(data.ErrorPlaceHolderInfo, errorMessage4);

			mainTransportRaw.JW_TransportType = Constants.TransportPlanningType.Other;
			data = CreateDocDataObjectBuilder(consol).Build();
			AssertHasMessageError("Main transport leg SEA mandatory error", data.ErrorPlaceHolderInfo, errorMessage4);
		}

		#endregion

		#region Containers

		public void TestContainerNumberISOValidation()
		{
			var consol = CreateConsol();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.Containers.RemoveAll();
			var newContainer = consol.Containers.AddNew();
			newContainer.JC_ContainerNum = "X";
			newContainer.JC_DeliveryMode = "CFS/CY";
			newContainer.JC_IsShipperOwned = false;
			newContainer.JC_GrossWeightUQ = "KG";
			newContainer.JC_TareWeight = 1000;
			newContainer.JC_DunnageWeight = 1000;
			newContainer.JC_RC = (Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP")).PK;
			newContainer.JC_IsEmptyContainer = false;
			var packline = consol.Shipments.Cast<CommonShipment>().First().OuterPackLines.AddNew();
			newContainer.PackLines.Add(packline);

			var data = CreateDocDataObjectBuilder(consol).Build();
			var container = data.Containers.Cast<Container>().First();

			void RefreshContainer()
			{
				data = CreateDocDataObjectBuilder(consol).Build();
				container = data.Containers.Cast<Container>().First();
			}

			var warningMessageContainerNumber = "Container number does not confirm to ISO standard of 4 letters followed by 6 digits and a check digit.";
			RefreshContainer();
			AssertHasMessageError(@"ONLY when 
										1. consol is NOT nonContainerized
										2. container number is NOT empty and NOT valid
										3. container is not shipper owned
									should we have this error message",
				container.NumberInfo,
				warningMessageContainerNumber);

			newContainer.JC_IsShipperOwned = true;
			RefreshContainer();
			AssertHasWarning(@"ONLY when 
									1.consol is NOT nonContainerized 
									2.container number is NOT empty and NOT valid
									3.container is shipped owned
								 should have this warning", container.NumberInfo, warningMessageContainerNumber);
			newContainer.JC_ContainerNum = ZString.Empty;
			RefreshContainer();
			AssertNoWarning(container.NumberInfo, warningMessageContainerNumber);
			AssertNoMessageError(container.NumberInfo, warningMessageContainerNumber);

			newContainer.JC_ContainerNum = "X";
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;
			RefreshContainer();
			AssertNoWarning(container.NumberInfo, warningMessageContainerNumber);
			AssertNoMessageError(container.NumberInfo, warningMessageContainerNumber);

			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			newContainer.JC_ContainerNum = "AAAA0000007";
			RefreshContainer();
			AssertNoWarning(container.NumberInfo, warningMessageContainerNumber);
			AssertNoMessageError(container.NumberInfo, warningMessageContainerNumber);
		}

		public void TestContainerTypeISOValidation()
		{
			var consol = CreateConsol();
			consol.Containers.RemoveAll();
			var newContainer = consol.Containers.AddNew();
			newContainer.JC_ContainerNum = "WEQQ0000009";
			newContainer.JC_DeliveryMode = "CFS/CY";
			newContainer.JC_IsShipperOwned = true;
			newContainer.JC_GrossWeightUQ = "KG";
			newContainer.JC_TareWeight = 1000;
			newContainer.JC_DunnageWeight = 1000;
			newContainer.JC_IsEmptyContainer = false;

			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "40GP";
			refContainer.RC_ISOType = "1234";
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			newContainer.JC_RC = refContainer.PK;

			var data = CreateDocDataObjectBuilder(consol).Build();
			var container = data.Containers.Cast<Container>().First();

			void RefreshContainer()
			{
				data = CreateDocDataObjectBuilder(consol).Build();
				container = data.Containers.Cast<Container>().First();
			}

			var invalidISOTypeWarning = "Container type entered does not have a valid ISO equipment code. Enter a valid ISO code to the matching Container Reference file.";
			var mandatoryISOTypeMessageError = "This container does not have a valid ISO Code. Enter a Valid ISO Code to the Container Reference File via Consol > Container > Container Type.";

			refContainer.RC_ISOType = string.Empty;
			RefreshContainer();
			AssertHasMessageError(container.Type.ISOCodeInfo, mandatoryISOTypeMessageError);

			refContainer.RC_ISOType = "1234";
			RefreshContainer();
			AssertNoMessageError(container.Type.ISOCodeInfo, mandatoryISOTypeMessageError);
			AssertHasWarning(@"ONLY when 
									1.consol is NOT nonContainerized 
									2.ISOContainer is NOT Valid", container.Type.ISOCodeInfo, invalidISOTypeWarning);

			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;
			RefreshContainer();
			AssertNoWarning(container.Type.ISOCodeInfo, invalidISOTypeWarning);

			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			refContainer.RC_ISOType = "44NJ";
			RefreshContainer();
			AssertNoWarning(container.Type.ISOCodeInfo, invalidISOTypeWarning);
		}

		public void TestContainerNumberTypeASCIIValidation()
		{
			var consol = CreateConsol();
			var data = CreateDocDataObjectBuilder(consol).Build();
			var container = data.Containers.Cast<Container>().First();
			Assert(!container.IsNull);
			AssertAsciiCharactersValidation("container.Number", container.NumberInfo);
			AssertAsciiCharactersValidation("container.Type.Code", container.Type.CodeInfo);
			AssertAsciiCharactersValidation("container.Type.ISOCode", container.Type.ISOCodeInfo);
		}

		public void TestContainerMeasurementValidation()
		{
			var consol = CreateConsol();
			Assert("precondition", consol.Containers.Any());
			var consolContainer = consol.Containers.Cast<CommonContainer>().First();
			consolContainer.JC_AirVentFlowRateUnit = ZString.Empty;
			consolContainer.JC_IsControlledAtmosphere = true;
			consolContainer.JC_SetPointTempUnit = ZString.Empty;
			var data = CreateDocDataObjectBuilder(consol).Build();
			var container = data.Containers.First();

			var mandatoryWarning = "You have not entered a value. This may result in processing delays.";
			container.TareWeight.Value = -1;
			AssertHasWarning(container.TareWeight.ValueInfo, mandatoryWarning);
			container.TareWeight.Value = ZDecimal.Zero;
			AssertHasWarning(container.TareWeight.ValueInfo, mandatoryWarning);
			container.TareWeight.Value = 1;
			AssertNoWarning(container.TareWeight.ValueInfo, mandatoryWarning);

			data.IsNonContainerized = true;
			container.TareWeight.Value = 0;
			AssertNoWarning(container.TareWeight.ValueInfo, mandatoryWarning);
			data.IsNonContainerized = false;

			container.GrossWeight.Value = -1;
			var grossWeightValueInfo = ((Measurement)container.GrossWeight)?.ValueInfo;
			AssertHasWarning(grossWeightValueInfo, mandatoryWarning);
			container.GrossWeight.Value = ZDecimal.Zero;
			AssertHasWarning(grossWeightValueInfo, mandatoryWarning);
			container.GrossWeight.Value = 1;
			AssertNoWarning(grossWeightValueInfo, mandatoryWarning);

			var errorMessageAirVent = "Selected Air Vent Setting measurement type is not supported by the carrier. Ensure each temperature controlled container has selected either '2L' or 'MQH' on the Containers > Refrigeration tab.";
			consolContainer.JC_AirVentFlow = 1;
			data = CreateDocDataObjectBuilder(consol).Build();
			container = data.Containers.First();
			AssertHasMessageError(((CodeDescription)container.AirVentFlow.Unit).CodeInfo, errorMessageAirVent);

			var errorMessageSetPointTemperature = "The default temperature has not yet been verified by the user. Please check the temperature and the unit of temperature against the container on the Consol";
			AssertHasMessageError(container.SetTemperature.ValueInfo, errorMessageSetPointTemperature);

			consolContainer.JC_IsControlledAtmosphere = false;
			data = CreateDocDataObjectBuilder(consol).Build();
			container = data.Containers.First();
			AssertNoMessageError(((CodeDescription)container.AirVentFlow.Unit).CodeInfo, errorMessageAirVent);
			AssertNoMessageError(container.SetTemperature.ValueInfo, errorMessageSetPointTemperature);

			consolContainer.JC_IsControlledAtmosphere = true;
			data = CreateDocDataObjectBuilder(consol).Build();
			container = data.Containers.First();
			AssertHasMessageError(container.SetTemperature.ValueInfo, errorMessageSetPointTemperature);

			consolContainer.JC_IsControlledAtmosphere = true;
			consolContainer.JC_SetPointTempUnit = "L";
			data = CreateDocDataObjectBuilder(consol).Build();
			container = data.Containers.First();
			AssertNoMessageError(container.SetTemperature.ValueInfo, errorMessageSetPointTemperature);

			consolContainer.JC_IsControlledAtmosphere = true;
			consolContainer.JC_AirVentFlowRateUnit = "2L";
			data = CreateDocDataObjectBuilder(consol).Build();
			container = data.Containers.First();
			AssertNoMessageError(((CodeDescription)container.AirVentFlow.Unit).CodeInfo, errorMessageAirVent);

			consolContainer.JC_IsControlledAtmosphere = true;
			consolContainer.JC_AirVentFlowRateUnit = "MQH";
			data = CreateDocDataObjectBuilder(consol).Build();
			container = data.Containers.First();
			AssertNoMessageError(((CodeDescription)container.AirVentFlow.Unit).CodeInfo, errorMessageAirVent);

			consolContainer.JC_IsControlledAtmosphere = true;
			consolContainer.JC_AirVentFlowRateUnit = "FOO";
			consolContainer.JC_AirVentFlow = 1;
			data = CreateDocDataObjectBuilder(consol).Build();
			container = data.Containers.First();
			AssertHasMessageError(((CodeDescription)container.AirVentFlow.Unit).CodeInfo, errorMessageAirVent);
		}

		public void TestPopulateContainerGenset()
		{
			var consol = CreateConsol();

			Assert("precondition", consol.Containers.Any());

			var consolContainer = consol.Containers.Cast<CommonContainer>().First();
			consolContainer.JC_RefrigGeneratorID = ZString.Empty;
			consolContainer.JC_IsControlledAtmosphere = true;
			var data = CreateDocDataObjectBuilder(consol).Build();
			var container = data.Containers.First();

			Assert(!container.Genset);

			consolContainer.JC_RefrigGeneratorID = "TestNumber1";
			data = CreateDocDataObjectBuilder(consol).Build();
			container = data.Containers.First();

			Assert(container.Genset);

			consolContainer.JC_IsControlledAtmosphere = false;
			data = CreateDocDataObjectBuilder(consol).Build();
			container = data.Containers.First();

			Assert(!container.Genset);
		}

		public void TestContainerVolumeValidation()
		{
			var errorMessage = "Volume exceeds the Capacity for this container type.";

			var consol = CreateConsol();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.Containers.RemoveAll();

			var newContainer = consol.Containers.AddNew();
			newContainer.JC_ContainerNum = "X";
			newContainer.JC_DeliveryMode = "CFS/CY";
			newContainer.JC_IsShipperOwned = false;
			newContainer.JC_GrossWeightUQ = "KG";
			newContainer.JC_TareWeight = 1000;
			newContainer.JC_DunnageWeight = 1000;
			newContainer.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP").PK;
			newContainer.JC_IsEmptyContainer = false;
			newContainer.JC_ContainerMode = "FCL";

			var packline = consol.Shipments.Cast<CommonShipment>().First().OuterPackLines.AddNew();
			packline.JL_ActualVolume = 1;
			newContainer.PackLines.Add(packline);

			Factory.Save();

			var data = CreateDocDataObjectBuilder(consol).Build();
			var container = data.Containers.First();

			AssertNotNull(container);
			AssertNoWarning(((Measurement)container.Volume).ValueInfo, errorMessage);

			packline.JL_ActualVolume = 1000;

			data = CreateDocDataObjectBuilder(consol).Build();
			container = data.Containers.First();

			AssertNotNull(container);
			AssertHasWarning(((Measurement)container.Volume).ValueInfo, errorMessage);

			var containerModes = new ZString[] { Constants.ContainerModes.Bulk, Constants.ContainerModes.BreakBulk, Constants.ContainerModes.RollOnRollOff };

			foreach (var containerMode in containerModes)
			{
				consol.JK_ConsolMode = containerMode;
				data = CreateDocDataObjectBuilder(consol).Build();
				container = data.Containers.First();

				AssertNotNull(container);
				AssertNoWarning(((Measurement)container.Volume).ValueInfo, errorMessage);
			}

			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			newContainer.JC_RC = Guid.NewGuid();

			data = CreateDocDataObjectBuilder(consol).Build();
			container = data.Containers.First();

			AssertNotNull(container);
			AssertNoWarning(((Measurement)container.Volume).ValueInfo, errorMessage);
		}

		public void TestContainerVolumeValidation_SkipWhenSpecialContainers()
		{
			var errorMessage = "Volume exceeds the Capacity for this container type.";

			var refContainer1 = Factory.New<RefContainer>();
			refContainer1.RC_Code = "20P0";
			refContainer1.RC_ISOType = "20P0";
			refContainer1.RC_CubicCapacity = 16m;

			var refContainer2 = Factory.New<RefContainer>();
			refContainer2.RC_Code = "20u0";
			refContainer2.RC_ISOType = "20u0";
			refContainer2.RC_CubicCapacity = 16m;

			Factory.Save();

			var consol = CreateConsol();
			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			consol.Containers.RemoveAll();

			var newContainer = consol.Containers.AddNew();
			newContainer.JC_ContainerNum = "X";
			newContainer.JC_DeliveryMode = "CFS/CY";
			newContainer.JC_IsShipperOwned = false;
			newContainer.JC_GrossWeightUQ = "KG";
			newContainer.JC_TareWeight = 1000;
			newContainer.JC_DunnageWeight = 1000;
			newContainer.JC_RC = refContainer1.PK;
			newContainer.JC_IsEmptyContainer = false;

			var packline = consol.Shipments.Cast<CommonShipment>().First().OuterPackLines.AddNew();
			packline.JL_ActualVolume = 17;
			newContainer.PackLines.Add(packline);

			Factory.Save();

			var data = CreateDocDataObjectBuilder(consol).Build();
			var container = data.Containers.First();

			AssertNotNull(container);
			AssertHasWarning(((Measurement)container.Volume).ValueInfo, errorMessage);

			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			data = CreateDocDataObjectBuilder(consol).Build();
			container = data.Containers.First();

			AssertNotNull(container);
			AssertNoWarning(((Measurement)container.Volume).ValueInfo, errorMessage);

			newContainer.JC_RC = refContainer2.PK;

			data = CreateDocDataObjectBuilder(consol).Build();
			container = data.Containers.First();

			AssertNotNull(container);
			AssertNoWarning(((Measurement)container.Volume).ValueInfo, errorMessage);
		}

		public void TestContainerVolumeMandatoryValidation()
		{
			var mandatoryWarning = "You have not entered a value. This may result in processing delays.";

			var consol = CreateConsol();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.Containers.RemoveAll();
			consol.Shipments.Cast<CommonShipment>().First().OuterPackLines.RemoveAll();

			var newContainer = consol.Containers.AddNew();
			newContainer.JC_ContainerNum = "X";
			newContainer.JC_DeliveryMode = "CFS/CY";
			newContainer.JC_IsShipperOwned = false;
			newContainer.JC_GrossWeightUQ = "KG";
			newContainer.JC_TareWeight = 1000;
			newContainer.JC_DunnageWeight = 1000;
			newContainer.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP").PK;
			newContainer.JC_IsEmptyContainer = false;
			newContainer.JC_ContainerMode = "FCL";

			var packline = consol.Shipments.Cast<CommonShipment>().First().OuterPackLines.AddNew();
			packline.JL_ActualVolume = -1;
			newContainer.PackLines.Add(packline);

			var data = CreateDocDataObjectBuilder(consol).Build();
			var container = data.Containers.First();

			AssertNotNull(container);
			AssertHasWarning(((Measurement)container.Volume).ValueInfo, mandatoryWarning);

			packline.JL_ActualVolume = 0;

			data = CreateDocDataObjectBuilder(consol).Build();
			container = data.Containers.First();

			AssertNotNull(container);
			AssertHasWarning(((Measurement)container.Volume).ValueInfo, mandatoryWarning);

			packline.JL_ActualVolume = 1;

			data = CreateDocDataObjectBuilder(consol).Build();
			container = data.Containers.First();

			AssertNotNull(container);
			AssertNoWarning(((Measurement)container.Volume).ValueInfo, mandatoryWarning);
		}

		#endregion

		#region Shipments and Packing Lines

		public void TestPopulateShipments()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packline11 = shipment1.OuterPackLines.AddNew();
			packline11.JL_PackageCount = 2;
			packline11.JL_F3_NKPackType = "PLT";

			var packline12 = shipment1.OuterPackLines.AddNew();
			packline12.JL_PackageCount = 2;
			packline12.JL_F3_NKPackType = "PLT";

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packline21 = shipment2.OuterPackLines.AddNew();
			packline21.JL_PackageCount = 2;
			packline21.JL_F3_NKPackType = "PLT";

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			var shipmentDOs = carrierMessageData.Shipments.Cast<Shipment>().ToArray();
			AssertEquals(2, shipmentDOs.Length);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1.PK, shipment2.PK }, shipmentDOs.Select(x => (ZGuid)x.Identifier));
			AssertContainsExactElementsInAnyOrder(new[] { packline11.PK, packline12.PK }, shipmentDOs[0].PackingLines.Cast<PackingLine>().Select(x => (ZGuid)x.Identifier));
			AssertContainsExactElementsInAnyOrder(new[] { packline21.PK }, shipmentDOs[1].PackingLines.Cast<PackingLine>().Select(x => (ZGuid)x.Identifier));

			if (carrierMessageData.DocumentName == DataContext.ShippingInstruction)
			{
				AssertNotNull(shipmentDOs[0].Buyer);
				AssertNotNull(shipmentDOs[1].Buyer);
				AssertNotNull(shipmentDOs[0].Supplier);
				AssertNotNull(shipmentDOs[1].Supplier);
			}
			else
			{
				AssertNull(shipmentDOs[0].Buyer);
				AssertNull(shipmentDOs[1].Buyer);
				AssertNull(shipmentDOs[0].Supplier);
				AssertNull(shipmentDOs[1].Supplier);
			}
		}

		public void TestPopulateColoadShipments()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var shipmentASM = consol.Shipments.AddNew();
			shipmentASM.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			var shipmentSTD1 = shipmentASM.CoLoadShipments.AddNew();
			shipmentSTD1.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packline11 = shipmentSTD1.OuterPackLines.AddNew();
			packline11.JL_PackageCount = 2;
			packline11.JL_F3_NKPackType = "PLT";

			var packline12 = shipmentSTD1.OuterPackLines.AddNew();
			packline12.JL_PackageCount = 2;
			packline12.JL_F3_NKPackType = "PLT";

			var shipmentSTD2 = shipmentASM.CoLoadShipments.AddNew();
			shipmentSTD2.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packline21 = shipmentSTD2.OuterPackLines.AddNew();
			packline21.JL_PackageCount = 2;
			packline21.JL_F3_NKPackType = "PLT";

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			var toplevelShipmentDOs = carrierMessageData.Shipments.ToArray();
			AssertEquals(1, toplevelShipmentDOs.Length);

			var subShipments = toplevelShipmentDOs.First().Shipments.ToArray();

			AssertContainsExactElementsInAnyOrder(new[] { shipmentSTD1.PK, shipmentSTD2.PK }, subShipments.Select(x => (ZGuid)x.Identifier));
			AssertContainsExactElementsInAnyOrder(new[] { packline11.PK, packline12.PK }, subShipments[0].PackingLines.Select(x => (ZGuid)x.Identifier));
			AssertContainsExactElementsInAnyOrder(new[] { packline21.PK }, subShipments[1].PackingLines.Select(x => (ZGuid)x.Identifier));

			if (carrierMessageData.DocumentName == DataContext.ShippingInstruction)
			{
				AssertNotNull(toplevelShipmentDOs[0].Buyer);
				AssertNotNull(toplevelShipmentDOs[0].Supplier);
			}
			else
			{
				AssertNull(toplevelShipmentDOs[0].Buyer);
				AssertNull(toplevelShipmentDOs[0].Supplier);
			}
		}

		public void TestPopulateColoadShipments_PackingLinesWithBCN()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";

			var shipmentBCN = consol.Shipments.AddNew();
			shipmentBCN.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;

			var packline1 = shipmentBCN.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 2;
			packline1.JL_F3_NKPackType = "PLT";

			var shipmentSTD1 = shipmentBCN.CoLoadShipments.AddNew();
			shipmentSTD1.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packline2 = shipmentSTD1.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 2;
			packline2.JL_F3_NKPackType = "PLT";

			var shipmentSTD2 = shipmentBCN.CoLoadShipments.AddNew();
			shipmentSTD2.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packline3 = shipmentSTD2.OuterPackLines.AddNew();
			packline3.JL_PackageCount = 2;
			packline3.JL_F3_NKPackType = "PLT";

			container.PackLines.Add(packline1);
			container.PackLines.Add(packline2);
			container.PackLines.Add(packline3);

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			var containerDO = carrierMessageData.Containers.FirstOrDefault();

			AssertContainsExactElementsInAnyOrder(new[] { packline1.PK, packline2.PK, packline3.PK }, containerDO.PackingLines.Cast<PackingLine>().Select(x => (ZGuid)x.Identifier));
		}

		public void TestPackingWeightAndVolumeValidation()
		{
			var consol = CreateConsol();
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;
			var data = CreateDocDataObjectBuilder(consol).Build();
			var container = data.Containers.FirstOrDefault();
			Assert("precondition", container != null);
			var packline = container.PackingLines.FirstOrDefault();
			Assert("precondition", packline != null);
			var errorMessageGoodsDescription = "Goods Description is required.";
			packline.GoodsDescription = "1";
			AssertNoMessageError(packline.GoodsDescriptionInfo, errorMessageGoodsDescription);
			packline.GoodsDescription = ZString.Empty;
			AssertHasMessageError(packline.GoodsDescriptionInfo, errorMessageGoodsDescription);

			container.IsEmpty = false;
			packline.Quantity = 1;
			var errorMessageQuantity = "You have not entered a value. If this is intended, please flag the container as empty.";
			AssertNoMessageError(@"ONLY when 
									1.quantity of packline is 0
									2.container is NOT empty or consol is NOT containerized 
								 should have this error", packline.QuantityInfo, errorMessageQuantity);
			packline.Quantity = ZInt.Zero;
			AssertHasMessageError(packline.QuantityInfo, errorMessageQuantity);
			data.IsNonContainerized = false;
			AssertHasMessageError(packline.QuantityInfo, errorMessageQuantity);
			data.IsNonContainerized = true;
			container.IsEmpty = true;
			AssertHasMessageError(packline.QuantityInfo, errorMessageQuantity);
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			data = CreateDocDataObjectBuilder(consol).Build();
			container = data.Containers.FirstOrDefault();
			container.IsEmpty = true;
			packline = container.PackingLines.FirstOrDefault();
			AssertNoMessageError(packline.QuantityInfo, errorMessageQuantity);

			var errorMessageEmptyWeight = "Total packing line weight is required. Please enter a value.";
			var errorMessageEmptyVolume = "The total packing line volume is zero. Please enter a value.";

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			data = CreateDocDataObjectBuilder(consol).Build();
			container = data.Containers.FirstOrDefault();
			packline = container.PackingLines.FirstOrDefault();
			packline.Weight.Value = ZDecimal.Zero;
			packline.Volume.Value = ZDecimal.Zero;
			AssertHasMessageError(packline.Weight.ValueInfo, errorMessageEmptyWeight);
			AssertHasMessageError(packline.Volume.ValueInfo, errorMessageEmptyVolume);

			packline.Weight.Value = 1.5;
			packline.Volume.Value = 2.5;
			AssertNoMessageError(packline.Weight.ValueInfo, errorMessageEmptyWeight);
			AssertNoMessageError(packline.Volume.ValueInfo, errorMessageEmptyVolume);

			consol.JK_AgentType = Constants.AgentType.Direct;
			data = CreateDocDataObjectBuilder(consol).Build();
			container = data.Containers.FirstOrDefault();
			packline = container.PackingLines.FirstOrDefault();
			packline.Weight.Value = ZDecimal.Zero;
			packline.Volume.Value = ZDecimal.Zero;
			AssertHasMessageError(packline.Weight.ValueInfo, errorMessageEmptyWeight);
			AssertNoMessageError(packline.Volume.ValueInfo, errorMessageEmptyVolume);

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_IsNVO = true;
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = "AUMEL";
			carrier.OH_RSL_ShippingLine = shippingLine.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			data = CreateDocDataObjectBuilder(consol).Build();
			container = data.Containers.FirstOrDefault();
			packline = container.PackingLines.FirstOrDefault();
			packline.Weight.Value = ZDecimal.Zero;
			packline.Volume.Value = ZDecimal.Zero;
			AssertHasMessageError(packline.Weight.ValueInfo, errorMessageEmptyWeight);
			AssertHasMessageError(packline.Volume.ValueInfo, errorMessageEmptyVolume);

			shippingLine.RSL_IsNVO = false;
			data = CreateDocDataObjectBuilder(consol).Build();
			container = data.Containers.FirstOrDefault();
			packline = container.PackingLines.FirstOrDefault();
			packline.Weight.Value = ZDecimal.Zero;
			packline.Volume.Value = ZDecimal.Zero;
			AssertHasMessageError(packline.Weight.ValueInfo, errorMessageEmptyWeight);
			AssertNoMessageError(packline.Volume.ValueInfo, errorMessageEmptyVolume);
		}

		public void TestMixedPackingLineUnits()
		{
			var warningMessage = "A mixed use of metric and imperial units has been detected, which may result in an inaccurate volume within the NVOCCs application.\r\nPlease ensure volume and weight are entered in the same unit system.";

			var consol = CreateConsol();

			var carrierShippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			carrierShippingLine.RSL_IsNVO = false;
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = "AUMEL";
			carrier.OH_RSL_ShippingLine = carrierShippingLine.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var coLoadShippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			coLoadShippingLine.RSL_IsNVO = false;
			var coLoad = Factory.New<OrgHeader>();
			coLoad.OH_FullName = "CoLoad";
			coLoad.OH_RL_NKClosestPort = "AUMEL";
			coLoad.OH_RSL_ShippingLine = coLoadShippingLine.PK;
			consol.JK_OA_CreditorAddress = coLoad.MainAddress.PK;

			consol.JK_AgentType = "AGT";
			carrierShippingLine.RSL_IsNVO = false;
			coLoadShippingLine.RSL_IsNVO = false;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = coLoad.MainAddress.PK;
			var data = CreateDocDataObjectBuilder(consol).Build();
			AssertNoWarning(data.Shipments.FirstOrDefault().PackingLines.FirstOrDefault().Volume.ValueInfo, warningMessage);

			consol.JK_AgentType = "DRT";
			carrierShippingLine.RSL_IsNVO = false;
			coLoadShippingLine.RSL_IsNVO = false;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = coLoad.MainAddress.PK;
			data = CreateDocDataObjectBuilder(consol).Build();
			AssertNoWarning(data.Shipments.FirstOrDefault().PackingLines.FirstOrDefault().Volume.ValueInfo, warningMessage);

			consol.JK_AgentType = "CLD";
			carrierShippingLine.RSL_IsNVO = false;
			coLoadShippingLine.RSL_IsNVO = false;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = coLoad.MainAddress.PK;
			data = CreateDocDataObjectBuilder(consol).Build();
			AssertNoWarning(data.Shipments.FirstOrDefault().PackingLines.FirstOrDefault().Volume.ValueInfo, warningMessage);

			consol.JK_AgentType = "COU";
			carrierShippingLine.RSL_IsNVO = false;
			coLoadShippingLine.RSL_IsNVO = false;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = coLoad.MainAddress.PK;
			data = CreateDocDataObjectBuilder(consol).Build();
			AssertNoWarning(data.Shipments.FirstOrDefault().PackingLines.FirstOrDefault().Volume.ValueInfo, warningMessage);

			consol.JK_AgentType = "AGT";
			carrierShippingLine.RSL_IsNVO = true;
			coLoadShippingLine.RSL_IsNVO = false;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = coLoad.MainAddress.PK;
			data = CreateDocDataObjectBuilder(consol).Build();
			AssertHasWarning(data.Shipments.FirstOrDefault().PackingLines.FirstOrDefault().Volume.ValueInfo, warningMessage);

			consol.JK_AgentType = "CLD";
			carrierShippingLine.RSL_IsNVO = true;
			coLoadShippingLine.RSL_IsNVO = false;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = coLoad.MainAddress.PK;
			data = CreateDocDataObjectBuilder(consol).Build();
			AssertHasWarning(data.Shipments.FirstOrDefault().PackingLines.FirstOrDefault().Volume.ValueInfo, warningMessage);

			consol.JK_AgentType = "DRT";
			carrierShippingLine.RSL_IsNVO = true;
			coLoadShippingLine.RSL_IsNVO = false;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = coLoad.MainAddress.PK;
			data = CreateDocDataObjectBuilder(consol).Build();
			AssertHasWarning(data.Shipments.FirstOrDefault().PackingLines.FirstOrDefault().Volume.ValueInfo, warningMessage);

			consol.JK_AgentType = "COU";
			carrierShippingLine.RSL_IsNVO = true;
			coLoadShippingLine.RSL_IsNVO = false;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = coLoad.MainAddress.PK;
			data = CreateDocDataObjectBuilder(consol).Build();
			AssertNoWarning(data.Shipments.FirstOrDefault().PackingLines.FirstOrDefault().Volume.ValueInfo, warningMessage);

			consol.JK_AgentType = "AGT";
			carrierShippingLine.RSL_IsNVO = false;
			coLoadShippingLine.RSL_IsNVO = true;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = coLoad.MainAddress.PK;
			data = CreateDocDataObjectBuilder(consol).Build();
			AssertNoWarning(data.Shipments.FirstOrDefault().PackingLines.FirstOrDefault().Volume.ValueInfo, warningMessage);

			consol.JK_AgentType = "DRT";
			carrierShippingLine.RSL_IsNVO = false;
			coLoadShippingLine.RSL_IsNVO = true;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = coLoad.MainAddress.PK;
			data = CreateDocDataObjectBuilder(consol).Build();
			AssertNoWarning(data.Shipments.FirstOrDefault().PackingLines.FirstOrDefault().Volume.ValueInfo, warningMessage);

			consol.JK_AgentType = "CLD";
			carrierShippingLine.RSL_IsNVO = false;
			coLoadShippingLine.RSL_IsNVO = true;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = coLoad.MainAddress.PK;
			data = CreateDocDataObjectBuilder(consol).Build();
			AssertHasWarning(data.Shipments.FirstOrDefault().PackingLines.FirstOrDefault().Volume.ValueInfo, warningMessage);

			consol.JK_AgentType = "COU";
			carrierShippingLine.RSL_IsNVO = false;
			coLoadShippingLine.RSL_IsNVO = true;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = coLoad.MainAddress.PK;
			data = CreateDocDataObjectBuilder(consol).Build();
			AssertNoWarning(data.Shipments.FirstOrDefault().PackingLines.FirstOrDefault().Volume.ValueInfo, warningMessage);

			consol.JK_AgentType = "AGT";
			carrierShippingLine.RSL_IsNVO = true;
			coLoadShippingLine.RSL_IsNVO = true;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = coLoad.MainAddress.PK;
			data = CreateDocDataObjectBuilder(consol).Build();
			AssertHasWarning(data.Shipments.FirstOrDefault().PackingLines.FirstOrDefault().Volume.ValueInfo, warningMessage);

			consol.JK_AgentType = "DRT";
			carrierShippingLine.RSL_IsNVO = true;
			coLoadShippingLine.RSL_IsNVO = true;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = coLoad.MainAddress.PK;
			data = CreateDocDataObjectBuilder(consol).Build();
			AssertHasWarning(data.Shipments.FirstOrDefault().PackingLines.FirstOrDefault().Volume.ValueInfo, warningMessage);

			consol.JK_AgentType = "CLD";
			carrierShippingLine.RSL_IsNVO = true;
			coLoadShippingLine.RSL_IsNVO = true;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = coLoad.MainAddress.PK;
			data = CreateDocDataObjectBuilder(consol).Build();
			AssertHasWarning(data.Shipments.FirstOrDefault().PackingLines.FirstOrDefault().Volume.ValueInfo, warningMessage);

			consol.JK_AgentType = "COU";
			carrierShippingLine.RSL_IsNVO = true;
			coLoadShippingLine.RSL_IsNVO = true;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = coLoad.MainAddress.PK;
			data = CreateDocDataObjectBuilder(consol).Build();
			AssertNoWarning(data.Shipments.FirstOrDefault().PackingLines.FirstOrDefault().Volume.ValueInfo, warningMessage);

			consol.JK_AgentType = "AGT";
			carrierShippingLine.RSL_IsNVO = true;
			coLoadShippingLine.RSL_IsNVO = true;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = coLoad.MainAddress.PK;
			consol.Shipments.ToList<ForwardingShipment>().FirstOrDefault().OuterPackLines.ToList<ForwardingPackLine>().FirstOrDefault().JL_ActualWeightUQ = "KG";
			consol.Shipments.ToList<ForwardingShipment>().FirstOrDefault().OuterPackLines.ToList<ForwardingPackLine>().FirstOrDefault().JL_ActualVolumeUQ = "M3";
			data = CreateDocDataObjectBuilder(consol).Build();
			AssertNoWarning(data.Shipments.FirstOrDefault().PackingLines.FirstOrDefault().Volume.ValueInfo, warningMessage);
		}

		public void TestPackingLinesQuantityValidation_IsGroupAndConsolidatePackingLines_WhenSubPackLineHasZeroPackCount()
		{
			AssertSubPackLineHasZeroPackCount(Constants.PackageGrouping.Codes.GroupByShipment);
			AssertSubPackLineHasZeroPackCount(Constants.PackageGrouping.Codes.GroupByPackLine);

			#region AssertSubPackLineHasZeroPackCount

			void AssertSubPackLineHasZeroPackCount(ZString packageGrouping)
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_PackageGrouping = packageGrouping;
				consol.JK_RL_NKLoadPort = "USCHI";
				consol.JK_RL_NKDischargePort = "CNSHG";
				consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;

				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = "CONTAINER1";
				container.JC_IsEmptyContainer = false;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_UniqueConsignRef = "SHIPMENT1";
				shipment.JS_UnitOfWeight = Constants.Weight.Grams;
				shipment.JS_UnitOfVolume = Constants.Volume.MegaLitre;
				shipment.DetailedGoodsDescriptionNoteText = "SHIPMENT1 DetailedGoodsDescriptionNoteText";
				shipment.JS_GoodsDescription = "SHIPMENT1 JS_GoodsDescription";
				shipment.JS_MarksAndNumbers = "SHIPMENT1 JS_MarksAndNumbers";
				shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
				shipment.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bag;

				shipment.InnerPackLines.RemoveAndDeleteAll();
				shipment.OuterPackLines.RemoveAndDeleteAll();

				var outerPackLine1 = PopulatePackLine(shipment.OuterPackLines.AddNew(), 1, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, null, null, true, -2, 3, Constants.Temperature.Fahrenheit);
				outerPackLine1.JL_JC = container.PK;

				var outerPackLine2 = PopulatePackLine(shipment.OuterPackLines.AddNew(), 2, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, null, null, true, -2, 3, Constants.Temperature.Fahrenheit);
				outerPackLine2.JL_JC = container.PK;

				using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var errorMessageQuantity = @"There are pack lines with zero (0) quantity.
Please verify in Shipment>Packing>Packs on following Shipments:
SHIPMENT1.";

					var builder = CreateDocDataObjectBuilder(consol);
					var data = builder.Build();
					var groupedPackingLine = data.Shipments.First().PackingLines.First();

					AssertNoMessageError(@"ONLY when
									1.part of the pack quantity of packline is 0
									2.container is NOT empty or consol is NOT containerized
								 should have this error", groupedPackingLine.QuantityInfo, errorMessageQuantity);

					outerPackLine1.JL_PackageCount = 0;

					data = builder.Build();
					groupedPackingLine = data.Shipments.First().PackingLines.First();

					AssertHasMessageError(groupedPackingLine.QuantityInfo, errorMessageQuantity);
				}
			}

			#endregion
		}

		public void TestPackingWeightAndVolumeValidation_IsGroupAndConsolidatePackingLines()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "CNSHG";
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER1";
			container.JC_IsEmptyContainer = false;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "SHIPMENT1";
			shipment.JS_UnitOfWeight = Constants.Weight.Grams;
			shipment.JS_UnitOfVolume = Constants.Volume.MegaLitre;
			shipment.DetailedGoodsDescriptionNoteText = "SHIPMENT1 DetailedGoodsDescriptionNoteText";
			shipment.JS_GoodsDescription = "SHIPMENT1 JS_GoodsDescription";
			shipment.JS_MarksAndNumbers = "SHIPMENT1 JS_MarksAndNumbers";
			shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
			shipment.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bag;

			shipment.InnerPackLines.RemoveAndDeleteAll();
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var outerPackLine = PopulatePackLine(shipment.OuterPackLines.AddNew(), 1, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, null, null, true, -2, 3, Constants.Temperature.Fahrenheit);
			outerPackLine.JL_JC = container.PK;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warningMessageEmptyWeight = @"There are pack lines with zero (0) Weight.
Please verify in Shipment>Packing>Weight on following Shipments:
SHIPMENT1.";

				var errorMessageEmptyWeight = @"Total packing line weight is required. Please enter a value.";

				var errorMessageEmptyVolume = @"There are pack lines with zero (0) Volume.
Please verify in Shipment>Packing>Volume on following Shipments:
SHIPMENT1.";

				consol.JK_AgentType = Constants.AgentType.CoLoad;
				consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByPackLine;
				outerPackLine.JL_ActualWeight = ZDecimal.Zero;
				outerPackLine.JL_ActualVolume = ZDecimal.Zero;
				var builder = CreateDocDataObjectBuilder(consol);
				var data = builder.Build();
				var groupedPackingLine = data.Shipments.First().PackingLines.First();
				var consolidatedPackingLine = groupedPackingLine.PackingLines.First();

				AssertHasWarning(groupedPackingLine.Weight.ValueInfo, warningMessageEmptyWeight);
				AssertHasMessageError(groupedPackingLine.Weight.ValueInfo, errorMessageEmptyWeight);
				AssertHasMessageError(groupedPackingLine.Volume.ValueInfo, errorMessageEmptyVolume);

				outerPackLine.JL_ActualWeight = 1.5M;
				outerPackLine.JL_ActualVolume = 2.5M;
				data = builder.Build();
				groupedPackingLine = data.Shipments.First().PackingLines.First();
				consolidatedPackingLine = groupedPackingLine.PackingLines.First();

				AssertNoWarning(groupedPackingLine.Weight.ValueInfo, warningMessageEmptyWeight);
				AssertNoMessageError(groupedPackingLine.Weight.ValueInfo, errorMessageEmptyWeight);
				AssertNoMessageError(groupedPackingLine.Volume.ValueInfo, errorMessageEmptyVolume);

				consol.JK_AgentType = Constants.AgentType.Direct;
				consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByPackLine;
				outerPackLine.JL_ActualWeight = ZDecimal.Zero;
				outerPackLine.JL_ActualVolume = ZDecimal.Zero;
				data = builder.Build();
				groupedPackingLine = data.Shipments.First().PackingLines.First();
				consolidatedPackingLine = groupedPackingLine.PackingLines.First();

				AssertHasWarning(groupedPackingLine.Weight.ValueInfo, warningMessageEmptyWeight);
				AssertHasMessageError(groupedPackingLine.Weight.ValueInfo, errorMessageEmptyWeight);
				AssertNoMessageError(groupedPackingLine.Volume.ValueInfo, errorMessageEmptyVolume);

				var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
				shippingLine.RSL_IsNVO = true;
				var carrier = Factory.New<OrgHeader>();
				carrier.OH_FullName = "Carrier";
				carrier.OH_RL_NKClosestPort = "AUMEL";
				carrier.OH_RSL_ShippingLine = shippingLine.PK;
				consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
				consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;
				data = builder.Build();

				groupedPackingLine = data.Shipments.First().PackingLines.First();
				consolidatedPackingLine = groupedPackingLine.PackingLines.First();

				AssertHasWarning(groupedPackingLine.Weight.ValueInfo, warningMessageEmptyWeight);
				AssertHasMessageError(groupedPackingLine.Weight.ValueInfo, errorMessageEmptyWeight);
				AssertHasMessageError(groupedPackingLine.Volume.ValueInfo, errorMessageEmptyVolume);

				shippingLine.RSL_IsNVO = false;
				data = builder.Build();
				groupedPackingLine = data.Shipments.First().PackingLines.First();
				consolidatedPackingLine = groupedPackingLine.PackingLines.First();

				AssertHasWarning(groupedPackingLine.Weight.ValueInfo, warningMessageEmptyWeight);
				AssertHasMessageError(groupedPackingLine.Weight.ValueInfo, errorMessageEmptyWeight);
				AssertNoMessageError(groupedPackingLine.Volume.ValueInfo, errorMessageEmptyVolume);

				PopulatePackLine(shipment.OuterPackLines.AddNew(), 1, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, null, null, true, -2, 3, Constants.Temperature.Fahrenheit);
				outerPackLine.JL_JC = container.PK;
				data = builder.Build();
				groupedPackingLine = data.Shipments.First().PackingLines.First();
				consolidatedPackingLine = groupedPackingLine.PackingLines.First();

				AssertHasWarning(groupedPackingLine.Weight.ValueInfo, warningMessageEmptyWeight);
				AssertNoMessageError(groupedPackingLine.Weight.ValueInfo, errorMessageEmptyWeight);
				AssertNoMessageError(groupedPackingLine.Volume.ValueInfo, errorMessageEmptyVolume);
			}
		}

		public void TestConsolidatedPackingLineWeightValidation()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "CNSHG";
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER1";
			container.JC_IsEmptyContainer = false;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "SHIPMENT1";
			shipment.JS_UnitOfWeight = Constants.Weight.Grams;
			shipment.JS_UnitOfVolume = Constants.Volume.MegaLitre;
			shipment.DetailedGoodsDescriptionNoteText = "SHIPMENT1 DetailedGoodsDescriptionNoteText";
			shipment.JS_GoodsDescription = "SHIPMENT1 JS_GoodsDescription";
			shipment.JS_MarksAndNumbers = "SHIPMENT1 JS_MarksAndNumbers";
			shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
			shipment.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bag;

			shipment.InnerPackLines.RemoveAndDeleteAll();
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var outerPackLine = PopulatePackLine(shipment.OuterPackLines.AddNew(), 1, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, null, null, true, -2, 3, Constants.Temperature.Fahrenheit);
			outerPackLine.JL_JC = container.PK;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var errorMessageEmptyWeight = @"No weight has been allocated to this container.
Please verify in Shipment > Packing > Weight.";

				consol.JK_AgentType = Constants.AgentType.CoLoad;
				consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByPackLine;
				outerPackLine.JL_ActualWeight = ZDecimal.Zero;
				outerPackLine.JL_ActualVolume = ZDecimal.Zero;
				var builder = CreateDocDataObjectBuilder(consol);
				var data = builder.Build();
				var groupedPackingLine = data.Shipments.First().PackingLines.First();
				var consolidatedPackingLine = groupedPackingLine.PackingLines.First();

				AssertHasMessageError(consolidatedPackingLine.Weight.ValueInfo, errorMessageEmptyWeight);

				outerPackLine.JL_ActualWeight = 1.5M;
				outerPackLine.JL_ActualVolume = 2.5M;
				data = builder.Build();
				groupedPackingLine = data.Shipments.First().PackingLines.First();
				consolidatedPackingLine = groupedPackingLine.PackingLines.First();

				AssertNoMessageError(consolidatedPackingLine.Weight.ValueInfo, errorMessageEmptyWeight);

				consol.JK_AgentType = Constants.AgentType.Direct;
				consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;
				outerPackLine.JL_ActualWeight = ZDecimal.Zero;
				outerPackLine.JL_ActualVolume = ZDecimal.Zero;
				data = builder.Build();
				groupedPackingLine = data.Shipments.First().PackingLines.First();
				consolidatedPackingLine = groupedPackingLine.PackingLines.First();

				AssertHasMessageError(consolidatedPackingLine.Weight.ValueInfo, errorMessageEmptyWeight);

				outerPackLine.JL_ActualWeight = 1.5M;
				outerPackLine.JL_ActualVolume = 2.5M;
				data = builder.Build();
				groupedPackingLine = data.Shipments.First().PackingLines.First();
				consolidatedPackingLine = groupedPackingLine.PackingLines.First();

				AssertNoMessageError(consolidatedPackingLine.Weight.ValueInfo, errorMessageEmptyWeight);
			}
		}

		public void TestAddPackingLineValidation_IsGroupAndConsolidatePackingLines()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "CNSHG";
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER1";
			container.JC_IsEmptyContainer = false;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "SHIPMENT1";
			shipment.JS_UnitOfWeight = Constants.Weight.Grams;
			shipment.JS_UnitOfVolume = Constants.Volume.MegaLitre;

			shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
			shipment.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bag;
			shipment.InnerPackLines.RemoveAndDeleteAll();
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var outerPackLine = PopulatePackLine(shipment.OuterPackLines.AddNew(), 1, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, null, null, true, -2, 3, Constants.Temperature.Fahrenheit);
			outerPackLine.JL_JC = container.PK;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var errorMessage = "Goods Description is required.";

				var builder = CreateDocDataObjectBuilder(consol);
				var data = builder.Build();
				var groupedPackingLine = data.Shipments.First().PackingLines.First();

				AssertHasMessageError(groupedPackingLine.GoodsDescriptionInfo, errorMessage);

				shipment.DetailedGoodsDescriptionNoteText = "SHIPMENT1 DetailedGoodsDescriptionNoteText";
				shipment.JS_GoodsDescription = "SHIPMENT1 JS_GoodsDescription";
				shipment.JS_MarksAndNumbers = "SHIPMENT1 JS_MarksAndNumbers";

				data = builder.Build();
				groupedPackingLine = data.Shipments.First().PackingLines.First();

				AssertNoMessageError(groupedPackingLine.GoodsDescriptionInfo, errorMessage);

				AssertAsciiCharactersValidation("packline.DetailedGoodsDescription", groupedPackingLine.DetailedGoodsDescriptionInfo);
				AssertAsciiCharactersValidation("packline.MarksAndNumbers", groupedPackingLine.MarksAndNumbersInfo);
				AssertAsciiCharactersValidation("packline.PackageType.CodeInfo", ((CodeDescription)groupedPackingLine.PackageType).CodeInfo);
				AssertAsciiCharactersValidation("packline.ShipmentEntryNumbers", groupedPackingLine.ShipmentEntryNumbersInfo);
			}
		}

		public void TestHarmonizedCodesValidation_IsGroupAndConsolidatePackingLines()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "CNSHG";
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER1";
			container.JC_IsEmptyContainer = false;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "SHIPMENT1";
			shipment.JS_UnitOfWeight = Constants.Weight.Grams;
			shipment.JS_UnitOfVolume = Constants.Volume.MegaLitre;

			shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
			shipment.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bag;
			shipment.InnerPackLines.RemoveAndDeleteAll();
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var outerPackLine1 = PopulatePackLine(shipment.OuterPackLines.AddNew(), 1, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, null, null, true, -2, 3, Constants.Temperature.Fahrenheit);
			outerPackLine1.JL_JC = container.PK;
			outerPackLine1.JL_HarmonisedCode = "HS1";

			var outerPackLine2 = PopulatePackLine(shipment.OuterPackLines.AddNew(), 1, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, null, null, true, -2, 3, Constants.Temperature.Fahrenheit);
			outerPackLine2.JL_JC = container.PK;
			outerPackLine2.JL_HarmonisedCode = "HS2";

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var errorMessageHarmonisedCode = "Harmonized Code should not start with dot or empty space.";

				var builder = CreateDocDataObjectBuilder(consol);
				var data = builder.Build();
				var groupedPackingLine = data.Shipments.First().PackingLines.First();

				AssertContains("HS1", ((HarmonizedCode)groupedPackingLine.HarmonizedCode).Code);
				AssertContains("HS2", ((HarmonizedCode)groupedPackingLine.HarmonizedCode).Code);
				AssertNoMessageError(((HarmonizedCode)groupedPackingLine.HarmonizedCode).CodeInfo, errorMessageHarmonisedCode);

				outerPackLine2.JL_HarmonisedCode = ".1123";
				data = builder.Build();
				groupedPackingLine = data.Shipments.First().PackingLines.First();

				AssertContains(".1123", ((HarmonizedCode)groupedPackingLine.HarmonizedCode).Code);
				AssertContains("HS1", ((HarmonizedCode)groupedPackingLine.HarmonizedCode).Code);
				AssertHasMessageError(((HarmonizedCode)groupedPackingLine.HarmonizedCode).CodeInfo, errorMessageHarmonisedCode);

				AssertAsciiCharactersValidation("HarmonizedCodeInfo", ((HarmonizedCode)groupedPackingLine.HarmonizedCode).CodeInfo);
			}
		}

		public void TestShipmentEntryNumbersValidation()
		{
			var consol = CreateConsol();
			consol.Transports.Cast<Freight.Business.Transport>().First().JW_RL_NKLoadPort = "USCHI";
			consol.Transports.Cast<Freight.Business.Transport>().Last().JW_RL_NKDiscPort = "AUSYD";
			var shipment = consol.Shipments.Cast<ForwardingShipment>().First();
			shipment.CusEntryNumbers.RemoveAndDeleteAll();
			var cusEntryNumber1 = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			cusEntryNumber1.CE_EntryNum = "11321";
			var cusEntryNumber2 = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber2.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.COC;
			cusEntryNumber2.CE_EntryNum = "22412";

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 4;
			packline.JL_F3_NKPackType = "PLT";
			packline.JL_ActualWeight = 21;
			packline.JL_ActualWeightUQ = "KG";
			packline.JL_ActualVolume = 300;
			packline.JL_ActualVolumeUQ = "M3";
			packline.JL_HarmonisedCode = "RRRRR";
			packline.JL_ExportRefNumber = "REF009";
			packline.JL_DetailedDescription = "pack4";
			packline.JL_ContainerPackingOrder = 4;
			consol.Containers.Cast<CommonContainer>().First().PackLines.Add(packline);

			var data = CreateDocDataObjectBuilder(consol).Build();
			Assert("PackingLine shipmentsEntryNumbers should be shown correctly", data.Containers.SelectMany(c => c.PackingLines).Cast<PackingLine>().Any(p => p.ShipmentEntryNumbers == "BKG:11321, COC:22412"));
			AssertAsciiCharactersValidation("PackingLine.ShipmentEntryNumbersInfo", data.Containers.First().PackingLines.Cast<PackingLine>().First().ShipmentEntryNumbersInfo);

			consol.Transports.Cast<Freight.Business.Transport>().First().JW_RL_NKLoadPort = "AUSYD";
			data = CreateDocDataObjectBuilder(consol).Build();
			Assert("PackingLine shipmentsEntryNumbers should not be shown when it not loads in US", !data.Containers.SelectMany(c => c.PackingLines).Cast<PackingLine>().Any(p => p.ShipmentEntryNumbers == "BKG:11321, COC:22412"));

			consol.Transports.Cast<Freight.Business.Transport>().First().JW_RL_NKLoadPort = "USLAX";
			consol.Transports.Cast<Freight.Business.Transport>().Last().JW_RL_NKDiscPort = "USCHI";
			data = CreateDocDataObjectBuilder(consol).Build();
			Assert("PackingLine shipmentsEntryNumbers should not be shown when it discharges in US", !data.Containers.SelectMany(c => c.PackingLines).Cast<PackingLine>().Any(p => p.ShipmentEntryNumbers == "BKG:11321, COC:22412"));
		}

		public void TestPopulateTopLevelShipments()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			var masterShipment = consol.Shipments.AddNew();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			masterShipment.JS_UniqueConsignRef = "S0000001";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment1.JS_UniqueConsignRef = "S0000002";

			var shipment2 = masterShipment.CoLoadShipments.AddNew();
			shipment2.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment2.JS_UniqueConsignRef = "S0000003";

			var data = CreateDocDataObjectBuilder(consol).Build();
			AssertContainsExactElementsInAnyOrder(new string[] { "S0000001", "S0000002" }, data.Shipments.Select(x => x.ShipmentID));
		}

		#endregion

		#region PopulateContact

		public void TestShouldPopulateContactInformation()
		{
			var consol = CreateConsol();
			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			consol.JK_AgentType = Constants.AgentType.Agent;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000008";
			container.JC_DeliveryMode = "CFS/CFS";

			FillWithContactInformation(consol.NotifyPartyDocumentaryAddress.Address);
			FillWithContactInformation(consol.NotifyParty2DocumentaryAddress.Address);
			FillWithContactInformation(consol.NotifyParty3DocumentaryAddress.Address);

			var shipment = consol.Shipments[0];

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			FillWithContactInformation(consignee.MainAddress);
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			FillWithContactInformation(consignor.MainAddress);
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			var consignorPickupAddress = Factory.NewWithValidTestData<OrgHeader>();
			FillWithContactInformation(consignorPickupAddress.MainAddress);
			shipment.ConsignorPickupAddress.E2_OA_Address = consignorPickupAddress.MainAddress.PK;

			var consigneeDeliveryAddress = Factory.NewWithValidTestData<OrgHeader>();
			FillWithContactInformation(consigneeDeliveryAddress.MainAddress);
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = consigneeDeliveryAddress.MainAddress.PK;
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = consol.NotifyPartyDocumentaryAddress.Address.PK;
			shipment.NotifyParty2DocumentaryAddress.E2_OA_Address = consol.NotifyParty2DocumentaryAddress.Address.PK;
			shipment.NotifyParty3DocumentaryAddress.E2_OA_Address = consol.NotifyParty3DocumentaryAddress.Address.PK;

			SetupJobConsolCost(consol);

			var wrapper = CreateDocDataObjectBuilder(consol).Build();

			AssertAddressContactIsEmpty(wrapper.Shipper, false);
			AssertAddressContactIsEmpty(wrapper.Carrier, true);
			AssertAddressContactIsEmpty(wrapper.Creditor, true);
			AssertAddressContactIsEmpty(wrapper.Forwarder, false);
			AssertAddressContactIsEmpty(wrapper.Consignee, false);
			AssertAddressContactIsEmpty(wrapper.FreightPayer, false);
			AssertAddressContactIsEmpty(wrapper.PickupFrom, false);
			AssertAddressContactIsEmpty(wrapper.DeliverTo, false);
			AssertAddressContactIsEmpty(wrapper.NotifyParty, false);
			AssertAddressContactIsEmpty(wrapper.NotifyParty2, false);
			AssertAddressContactIsEmpty(wrapper.NotifyParty3, false);

			consol.JK_AgentType = Constants.AgentType.Direct;
			wrapper = new ShippingInstructionBuilder(consol).Build();

			AssertAddressContactIsEmpty(wrapper.Shipper, false);
			AssertAddressContactIsEmpty(wrapper.Carrier, true);
			AssertAddressContactIsEmpty(wrapper.Creditor, true);
			AssertAddressContactIsEmpty(wrapper.Forwarder, false);
			AssertAddressContactIsEmpty(wrapper.Consignee, false);
			AssertAddressContactIsEmpty(wrapper.FreightPayer, false);
			AssertAddressContactIsEmpty(wrapper.PickupFrom, false);
			AssertAddressContactIsEmpty(wrapper.DeliverTo, false);
			AssertAddressContactIsEmpty(wrapper.NotifyParty, false);
			AssertAddressContactIsEmpty(wrapper.NotifyParty2, false);
			AssertAddressContactIsEmpty(wrapper.NotifyParty3, false);
		}

		void FillWithContactInformation(OrgAddress address)
		{
			address.OA_Phone = "12344";
			address.OA_Fax = "222";
			address.OA_Email = "aaa@test.com";
		}

		void AssertAddressContactIsEmpty(Address address, bool shouldBeEmpty)
		{
			AssertEquals(shouldBeEmpty, address.Phone.IsEmpty && address.Fax.IsEmpty && address.Email.IsEmpty);
		}

		#endregion

		#region Dangerous Goods

		public void TestHarmonizedCodeValidation()
		{
			var consol = CreateConsol();
			var data = CreateDocDataObjectBuilder(consol).Build();
			var packline = data.Containers.FirstOrDefault().PackingLines.FirstOrDefault();
			Assert("precondition", packline != null);
			var errorMessageHarmonisedCode = "Harmonized Code should not start with dot or empty space.";
			packline.HarmonizedCode.Code = "hc1123";
			AssertNoMessageError(((HarmonizedCode)packline.HarmonizedCode).CodeInfo, errorMessageHarmonisedCode);
			packline.HarmonizedCode.Code = ".1123";
			AssertHasMessageError(((HarmonizedCode)packline.HarmonizedCode).CodeInfo, errorMessageHarmonisedCode);

			var packageTypeCodeInfo = ((CodeDescription)packline.PackageType).CodeInfo;
			AssertAsciiCharactersValidation("HarmonizedCodeInfo", ((HarmonizedCode)packline.HarmonizedCode).CodeInfo);
			AssertAsciiCharactersValidation("packline.DetailedGoodsDescription", packline.DetailedGoodsDescriptionInfo);
			AssertAsciiCharactersValidation("packline.MarksAndNumbers", packline.MarksAndNumbersInfo);
			AssertAsciiCharactersValidation("packline.PackageType.CodeInfo", packageTypeCodeInfo);
		}

		public void TestDangerousGoodsValidation()
		{
			var consol = CreateConsol();
			var errorMessageFlashPoint = "Flashpoint temperature must contain up to 3 numeric digits (excluding plus/minus sign and decimal).";
			var errorMessageFlashPointOverflow = "System does not support Flashpoint temperature greater than or equal to 1000 (including values round to 1000).";
			var errorMessageContact = "Contact is required for dangerous goods.";
			var errorMessageContactName = "Contact Name is required for dangerous goods.";
			var errorMessageContactPhone = "Contact Phone is required for dangerous goods.";

			foreach (var container in consol.Containers.Cast<ForwardingContainer>())
			{
				foreach (var packLine in container.PackLines.Cast<PackLine>())
				{
					packLine.UNDGs.DeleteAll();
				}
			}

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Handsome";
			contact.OC_Phone = "1234567";

			var undgSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			undgSubstance.DG_Class = "CLAS";
			undgSubstance.DG_PSN = "CLASPSN";

			var undg = consol.Containers.Cast<ForwardingContainer>().First().PackLines.Cast<PackLine>().First().UNDGs.AddNew();
			undg.DI_TechnicalName = "WHATEVER";
			undg.DI_IMOClass = "CLAS";
			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 521.2m;
			undg.DI_IsCombustible = true;
			undg.DI_MPMarinePollutant = "N";
			undg.DI_OC_DGContact = contact.PK;
			undg.DI_DG = undgSubstance.PK;

			var data = CreateDocDataObjectBuilder(consol).Build();
			DangerousGood GetDangerousGood()
			{
				foreach (var container in data.Containers)
				{
					foreach (var packLine in container.PackingLines)
					{
						if (packLine.DangerousGoods.Any())
						{
							return packLine.DangerousGoods.First();
						}
					}
				}
				return null;
			}
			var dangerousGood = GetDangerousGood();

			Assert(!dangerousGood.Validator().Any());
			AssertNoNotifications(dangerousGood.Contact.FullNameInfo);
			AssertNoNotifications(dangerousGood.Contact.PhoneInfo);

			contact.OC_ContactName = ZString.Empty;
			contact.OC_Phone = ZString.Empty;
			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 1m;
			undg.DI_IsCombustible = false;
			undg.DI_OC_DGContact = contact.PK;

			data = CreateDocDataObjectBuilder(consol).Build();
			dangerousGood = GetDangerousGood();
			dangerousGood.FlashPoint.Value = 1000.2m;

			dangerousGood.ValidateAllIncludingChildren();
			AssertEquals(4, dangerousGood.Validator().Count());
			AssertContainsExactElementsInAnyOrder(dangerousGood.Validator(), new string[] { errorMessageFlashPoint, errorMessageFlashPointOverflow, errorMessageContactPhone, errorMessageContactName });

			dangerousGood.FlashPoint.Value = 1000;
			dangerousGood.ValidateAllIncludingChildren();
			AssertEquals(3, dangerousGood.Validator().Count());
			AssertContainsExactElementsInAnyOrder(dangerousGood.Validator(), new string[] { errorMessageFlashPointOverflow, errorMessageContactPhone, errorMessageContactName });

			dangerousGood.FlashPoint.Value = 999.6;
			dangerousGood.ValidateAllIncludingChildren();
			AssertEquals(3, dangerousGood.Validator().Count());
			AssertContainsExactElementsInAnyOrder(dangerousGood.Validator(), new string[] { errorMessageFlashPointOverflow, errorMessageContactPhone, errorMessageContactName });

			dangerousGood.FlashPoint.Value = -1000;
			dangerousGood.ValidateAllIncludingChildren();
			AssertEquals(3, dangerousGood.Validator().Count());
			AssertContainsExactElementsInAnyOrder(dangerousGood.Validator(), new string[] { errorMessageFlashPointOverflow, errorMessageContactPhone, errorMessageContactName });

			dangerousGood.FlashPoint.Value = -999.6;
			dangerousGood.ValidateAllIncludingChildren();
			AssertEquals(3, dangerousGood.Validator().Count());
			AssertContainsExactElementsInAnyOrder(dangerousGood.Validator(), new string[] { errorMessageFlashPointOverflow, errorMessageContactPhone, errorMessageContactName });

			data = CreateDocDataObjectBuilder(consol).Build();
			dangerousGood = GetDangerousGood();
			dangerousGood.Contact = null;
			dangerousGood.Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			dangerousGood.ValidateAllIncludingChildren();

			AssertEquals(1, dangerousGood.Validator().Count());
			AssertContainsExactElementsInAnyOrder(dangerousGood.Validator(), new string[] { errorMessageContact });

			data = CreateDocDataObjectBuilder(consol).Build();
			dangerousGood = GetDangerousGood();
			dangerousGood.Contact = null;
			dangerousGood.Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.ADN;
			dangerousGood.ValidateAllIncludingChildren();
			AssertEquals(0, dangerousGood.Validator().Count());
		}

		public void TestDangerousGoodsValidation_Substance()
		{
			var consol = CreateConsol();
			var errorMessage = "DG Class, UNDG and Proper Shipping Name are required for dangerous goods.\r\nPlease enter Shipment > Packing > Pack Lines > Dangerous Goods > DG Substance.";

			foreach (var container in consol.Containers.Cast<ForwardingContainer>())
			{
				foreach (var packLine in container.PackLines.Cast<PackLine>())
				{
					packLine.UNDGs.DeleteAll();
				}
			}

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Handsome";
			contact.OC_Phone = "1234567";

			var undgSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			undgSubstance.DG_Class = "CLAS";
			undgSubstance.DG_PSN = "CLASPSN";

			var undg = consol.Containers.Cast<ForwardingContainer>().First().PackLines.Cast<PackLine>().First().UNDGs.AddNew();
			undg.DI_TechnicalName = "WHATEVER";
			undg.DI_IMOClass = "CLAS";
			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 521.2m;
			undg.DI_IsCombustible = true;
			undg.DI_MPMarinePollutant = "N";
			undg.DI_OC_DGContact = contact.PK;
			undg.DI_DG = undgSubstance.PK;

			var data = CreateDocDataObjectBuilder(consol).Build();
			DangerousGood GetDangerousGood()
			{
				foreach (var container in data.Containers)
				{
					foreach (var packLine in container.PackingLines)
					{
						if (packLine.DangerousGoods.Any())
						{
							return packLine.DangerousGoods.First();
						}
					}
				}
				return null;
			}
			var dangerousGood = GetDangerousGood();

			Assert(!dangerousGood.Validator().Any());

			undg.DI_IMOClass = ZString.Empty;

			data = CreateDocDataObjectBuilder(consol).Build();
			dangerousGood = GetDangerousGood();
			Assert(!dangerousGood.Validator().Any());

			undgSubstance.DG_PSN = ZString.Empty;

			data = CreateDocDataObjectBuilder(consol).Build();
			dangerousGood = GetDangerousGood();
			Assert(dangerousGood.Validator().Contains(errorMessage));

			undgSubstance.DG_PSN = "CLASPSN";
			undgSubstance.DG_Class = ZString.Empty;

			data = CreateDocDataObjectBuilder(consol).Build();
			dangerousGood = GetDangerousGood();
			Assert(dangerousGood.Validator().Contains(errorMessage));

			undgSubstance.DG_Class = "CLAS";
			undgSubstance.DG_Code = ZString.Empty;

			data = CreateDocDataObjectBuilder(consol).Build();
			dangerousGood = GetDangerousGood();
			Assert(dangerousGood.Validator().Contains(errorMessage));

			undg.DI_DG = ZGuid.Empty;
			undg.DI_IMOClass = "CLAS";

			data = CreateDocDataObjectBuilder(consol).Build();
			dangerousGood = GetDangerousGood();
			Assert(dangerousGood.Validator().Contains(errorMessage));
		}

		public void TestDangerousGoodsASCIIValidation()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var container = Factory.NewWithValidTestData<ForwardingContainer>();
			consol.Containers.Add(container);
			var packline = Factory.NewWithValidTestData<PackLine>();
			container.PackLines.Add(packline);
			var undg = Factory.NewWithValidTestData<UNDGDataItem>();
			packline.UNDGs.Add(undg);
			var data = CreateDocDataObjectBuilder(CreateConsol()).Build();
			var dangerousGood = data.Containers.SelectMany(c => c.PackingLines.SelectMany(p => p.DangerousGoods)).First();

			AssertDGAsciiCharactersValidation(dangerousGood.IMOClassInfo.HumanReadableName, dangerousGood.IMOClassInfo, dangerousGood.Validator(), dangerousGood);
			AssertDGAsciiCharactersValidation("Contact Name", dangerousGood.Contact.FullNameInfo, dangerousGood.Validator(), dangerousGood);
			AssertDGAsciiCharactersValidation(dangerousGood.TechnicalNameInfo.HumanReadableName, dangerousGood.TechnicalNameInfo, dangerousGood.Validator(), dangerousGood);
			AssertDGAsciiCharactersValidation("Marine Pollutant", ((CodeDescription)dangerousGood.MarinePollutant)?.CodeInfo, dangerousGood.Validator(), dangerousGood);
		}

		public void TestDangerousGoodsValidation_IsGroupAndConsolidatePackingLines()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "CNSHG";
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER1";
			container.JC_IsEmptyContainer = false;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "SHIPMENT1";
			shipment.JS_UnitOfWeight = Constants.Weight.Grams;
			shipment.JS_UnitOfVolume = Constants.Volume.MegaLitre;

			shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
			shipment.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bag;
			shipment.InnerPackLines.RemoveAndDeleteAll();
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var outerPackLine1 = PopulatePackLine(shipment.OuterPackLines.AddNew(), 1, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, null, null, true, -2, 3, Constants.Temperature.Fahrenheit);
			outerPackLine1.JL_JC = container.PK;
			outerPackLine1.JL_HarmonisedCode = "HS1";

			var errorMessageFlashPoint = "Flashpoint temperature must contain up to 3 numeric digits (excluding plus/minus sign and decimal).";
			var errorMessageFlashPointOverflow = "System does not support Flashpoint temperature greater than or equal to 1000 (including values round to 1000).";
			var errorMessageContact = "Contact is required for dangerous goods.";
			var errorMessageContactName = "Contact Name is required for dangerous goods.";
			var errorMessageContactPhone = "Contact Phone is required for dangerous goods.";

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Handsome";
			contact.OC_Phone = "1234567";

			var undgSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			undgSubstance.DG_Class = "CLAS";
			undgSubstance.DG_PSN = "CLASPSN";

			var undg = outerPackLine1.UNDGs.AddNew();
			undg.DI_TechnicalName = "WHATEVER";
			undg.DI_IMOClass = "CLAS";
			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 521.2m;
			undg.DI_IsCombustible = true;
			undg.DI_MPMarinePollutant = "N";
			undg.DI_OC_DGContact = contact.PK;
			undg.DI_DG = undgSubstance.PK;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = CreateDocDataObjectBuilder(consol).Build();
				DangerousGood GetDangerousGood()
				{
					return data.Shipments.First().PackingLines.First().PackingLines.First().DangerousGoods.First();
				}
				var dangerousGood = GetDangerousGood();

				Assert(!dangerousGood.Validator().Any());
				AssertNoNotifications(dangerousGood.Contact.FullNameInfo);
				AssertNoNotifications(dangerousGood.Contact.PhoneInfo);

				contact.OC_ContactName = ZString.Empty;
				contact.OC_Phone = ZString.Empty;
				undg.DI_IsCombustible = true;
				undg.DI_DGFlashPoint = 1m;
				undg.DI_IsCombustible = false;
				undg.DI_OC_DGContact = contact.PK;

				data = CreateDocDataObjectBuilder(consol).Build();
				dangerousGood = GetDangerousGood();
				dangerousGood.FlashPoint.Value = 1000.2m;

				dangerousGood.ValidateAllIncludingChildren();
				AssertEquals(4, dangerousGood.Validator().Count());
				AssertContainsExactElementsInAnyOrder(dangerousGood.Validator(), new string[] { errorMessageFlashPoint, errorMessageFlashPointOverflow, errorMessageContactPhone, errorMessageContactName });

				dangerousGood.FlashPoint.Value = 1000;
				dangerousGood.ValidateAllIncludingChildren();
				AssertEquals(3, dangerousGood.Validator().Count());
				AssertContainsExactElementsInAnyOrder(dangerousGood.Validator(), new string[] { errorMessageFlashPointOverflow, errorMessageContactPhone, errorMessageContactName });

				dangerousGood.FlashPoint.Value = 999.6;
				dangerousGood.ValidateAllIncludingChildren();
				AssertEquals(3, dangerousGood.Validator().Count());
				AssertContainsExactElementsInAnyOrder(dangerousGood.Validator(), new string[] { errorMessageFlashPointOverflow, errorMessageContactPhone, errorMessageContactName });

				dangerousGood.FlashPoint.Value = -1000;
				dangerousGood.ValidateAllIncludingChildren();
				AssertEquals(3, dangerousGood.Validator().Count());
				AssertContainsExactElementsInAnyOrder(dangerousGood.Validator(), new string[] { errorMessageFlashPointOverflow, errorMessageContactPhone, errorMessageContactName });

				dangerousGood.FlashPoint.Value = -999.6;
				dangerousGood.ValidateAllIncludingChildren();
				AssertEquals(3, dangerousGood.Validator().Count());
				AssertContainsExactElementsInAnyOrder(dangerousGood.Validator(), new string[] { errorMessageFlashPointOverflow, errorMessageContactPhone, errorMessageContactName });

				data = CreateDocDataObjectBuilder(consol).Build();
				dangerousGood = GetDangerousGood();
				dangerousGood.Contact = null;
				dangerousGood.Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
				dangerousGood.ValidateAllIncludingChildren();

				AssertEquals(1, dangerousGood.Validator().Count());
				AssertContainsExactElementsInAnyOrder(dangerousGood.Validator(), new string[] { errorMessageContact });

				data = CreateDocDataObjectBuilder(consol).Build();
				dangerousGood = GetDangerousGood();
				dangerousGood.Contact = null;
				dangerousGood.Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.ADN;
				dangerousGood.ValidateAllIncludingChildren();
				AssertEquals(0, dangerousGood.Validator().Count());
			}
		}

		public void TestDangerousGoodsValidation_Substance_IsGroupAndConsolidatePackingLines()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "CNSHG";
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER1";
			container.JC_IsEmptyContainer = false;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "SHIPMENT1";
			shipment.JS_UnitOfWeight = Constants.Weight.Grams;
			shipment.JS_UnitOfVolume = Constants.Volume.MegaLitre;

			shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
			shipment.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bag;
			shipment.InnerPackLines.RemoveAndDeleteAll();
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var outerPackLine1 = PopulatePackLine(shipment.OuterPackLines.AddNew(), 1, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, null, null, true, -2, 3, Constants.Temperature.Fahrenheit);
			outerPackLine1.JL_JC = container.PK;
			outerPackLine1.JL_HarmonisedCode = "HS1";

			var errorMessage = "DG Class, UNDG and Proper Shipping Name are required for dangerous goods.\r\nPlease enter Shipment > Packing > Pack Lines > Dangerous Goods > DG Substance.";

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Handsome";
			contact.OC_Phone = "1234567";

			var undgSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			undgSubstance.DG_Class = "CLAS";
			undgSubstance.DG_PSN = "CLASPSN";

			var undg = outerPackLine1.UNDGs.AddNew();
			undg.DI_TechnicalName = "WHATEVER";
			undg.DI_IMOClass = "CLAS";
			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 521.2m;
			undg.DI_IsCombustible = true;
			undg.DI_MPMarinePollutant = "N";
			undg.DI_OC_DGContact = contact.PK;
			undg.DI_DG = undgSubstance.PK;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = CreateDocDataObjectBuilder(consol).Build();
				DangerousGood GetDangerousGood()
				{
					return data.Shipments.First().PackingLines.First().PackingLines.First().DangerousGoods.First();
				}
				var dangerousGood = GetDangerousGood();

				Assert(!dangerousGood.Validator().Any());

				undg.DI_IMOClass = ZString.Empty;

				data = CreateDocDataObjectBuilder(consol).Build();
				dangerousGood = GetDangerousGood();
				Assert(!dangerousGood.Validator().Any());

				undgSubstance.DG_PSN = ZString.Empty;

				data = CreateDocDataObjectBuilder(consol).Build();
				dangerousGood = GetDangerousGood();
				Assert(dangerousGood.Validator().Contains(errorMessage));

				undgSubstance.DG_PSN = "CLASPSN";
				undgSubstance.DG_Class = ZString.Empty;

				data = CreateDocDataObjectBuilder(consol).Build();
				dangerousGood = GetDangerousGood();
				Assert(dangerousGood.Validator().Contains(errorMessage));

				undgSubstance.DG_Class = "CLAS";
				undgSubstance.DG_Code = ZString.Empty;

				data = CreateDocDataObjectBuilder(consol).Build();
				dangerousGood = GetDangerousGood();
				Assert(dangerousGood.Validator().Contains(errorMessage));

				undg.DI_DG = ZGuid.Empty;
				undg.DI_IMOClass = "CLAS";

				data = CreateDocDataObjectBuilder(consol).Build();
				dangerousGood = GetDangerousGood();
				Assert(dangerousGood.Validator().Contains(errorMessage));
			}
		}

		public void TestDangerousGoodsASCIIValidation_IsGroupAndConsolidatePackingLines()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "CNSHG";
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER1";
			container.JC_IsEmptyContainer = false;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "SHIPMENT1";
			shipment.JS_UnitOfWeight = Constants.Weight.Grams;
			shipment.JS_UnitOfVolume = Constants.Volume.MegaLitre;

			shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
			shipment.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bag;
			shipment.InnerPackLines.RemoveAndDeleteAll();
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var outerPackLine1 = PopulatePackLine(shipment.OuterPackLines.AddNew(), 1, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, null, null, true, -2, 3, Constants.Temperature.Fahrenheit);
			outerPackLine1.JL_JC = container.PK;
			outerPackLine1.JL_HarmonisedCode = "HS1";

			var undg = Factory.NewWithValidTestData<UNDGDataItem>();
			outerPackLine1.UNDGs.Add(undg);

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = CreateDocDataObjectBuilder(consol).Build();
				var dangerousGood = data.Shipments.First().PackingLines.First().PackingLines.First().DangerousGoods.First();

				AssertDGAsciiCharactersValidation(dangerousGood.IMOClassInfo.HumanReadableName, dangerousGood.IMOClassInfo, dangerousGood.Validator(), dangerousGood);
				AssertDGAsciiCharactersValidation("Contact Name", dangerousGood.Contact.FullNameInfo, dangerousGood.Validator(), dangerousGood);
				AssertDGAsciiCharactersValidation(dangerousGood.TechnicalNameInfo.HumanReadableName, dangerousGood.TechnicalNameInfo, dangerousGood.Validator(), dangerousGood);
				AssertDGAsciiCharactersValidation("Marine Pollutant", ((CodeDescription)dangerousGood.MarinePollutant)?.CodeInfo, dangerousGood.Validator(), dangerousGood);
			}
		}

		static void AssertDGAsciiCharactersValidation(string propertyName, ZPropertyInfo propertyInfo, IEnumerable<string> errors, DangerousGood dangerousGood)
		{
			string errorMessage = $"{propertyName} do not support non ASCII characters.";

			propertyInfo.Value = (ZString)"天";
			dangerousGood.ValidateAllIncludingChildren();
			Assert($"{propertyName}", errors.Contains(errorMessage));

			propertyInfo.Value = (ZString)"இ";
			dangerousGood.ValidateAllIncludingChildren();
			Assert($"{propertyName}", errors.Contains(errorMessage));

			propertyInfo.Value = (ZString)"æ";
			dangerousGood.ValidateAllIncludingChildren();
			Assert($"{propertyName}", !errors.Contains(errorMessage));

			propertyInfo.Value = (ZString)"ß";
			dangerousGood.ValidateAllIncludingChildren();
			Assert($"{propertyName}", !errors.Contains(errorMessage));

			propertyInfo.Value = (ZString)"1";
			dangerousGood.ValidateAllIncludingChildren();
			Assert($"{propertyName}", !errors.Contains(errorMessage));
		}

		#endregion

		#region BookingReference And MasterBillNumber

		public void TestPopulateBookingReferenceAndMasterBillNumber()
		{
			var consol = CreateConsol();
			consol.JK_AgentType = "AGT";
			consol.JK_BookingReference = "BookRef";
			consol.JK_MasterBillNum = "M001";
			consol.JK_CoLoadBookingReference = "CLDBookRef";
			consol.JK_CoLoadMasterBill = "C001";

			var data = CreateDocDataObjectBuilder(consol).Build();
			CombineAssertions(() =>
			{
				AssertEquals("BookingReference", "BookRef", data.BookingReference);
				AssertEquals("MasterBillNumber", "M001", data.MasterBillNumber);
				AssertEquals("CoLoadBookingReference", string.Empty, data.CoLoadBookingReference);
				AssertEquals("CoLoadMasterBillNumber", string.Empty, data.CoLoadMasterBillNumber);
			});

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_CoLoadBookingReference = "CLDBookRef";
			consol.JK_CoLoadMasterBill = "C001";
			data = CreateDocDataObjectBuilder(consol).Build();
			CombineAssertions(() =>
			{
				AssertEquals("BookingReference", "BookRef", data.BookingReference);
				AssertEquals("MasterBillNumber", "M001", data.MasterBillNumber);
				AssertEquals("CoLoadBookingReference", "CLDBookRef", data.CoLoadBookingReference);
				AssertEquals("CoLoadMasterBillNumber", "C001", data.CoLoadMasterBillNumber);
			});
		}

		#endregion

		#region Carrier Contract Number

		public void TestCarrierContractNumberValidation()
		{
			var warningMessage = "It is recommended to fill in Carrier Contract or Quote Number to assist with faster booking and reconciliation processes.";

			var consol = CreateConsol();
			var data = CreateDocDataObjectBuilder(consol).Build();

			data.CarrierContractNumbersFormatted = string.Empty;
			AssertHasWarning(data.CarrierContractNumbersFormattedInfo, warningMessage);

			data.CarrierContractNumbersFormatted = null;
			AssertHasWarning(data.CarrierContractNumbersFormattedInfo, warningMessage);

			data.CarrierContractNumbersFormatted = "123456";
			AssertNoWarning(data.CarrierContractNumbersFormattedInfo, warningMessage);

			data.CarrierContractNumbersFormatted = "123456, 789";
			AssertNoWarning(data.CarrierContractNumbersFormattedInfo, warningMessage);
		}

		#endregion

		#region PupulateDetailedPortNameToUnlocos

		public void TestPupulateDetailedPortNameToUnlocos()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "CONSPA";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.Address1 = "Unit 15";
			org.MainAddress.Address2 = "5 Lost Lane";
			org.MainAddress.City = "Sydney";
			org.MainAddress.Postcode = "2000";
			org.MainAddress.OA_RN_NKCountryCode = "AU";

			var consol = CreateConsol();
			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_OA_ShippingLineAddress = org.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = org.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = org.MainAddress.PK;
			consol.CarrierHandlingAgentDocumentaryAddress.E2_OA_Address = org.MainAddress.PK;
			consol.CarrierBookingAgentDocumentaryAddress.E2_OA_Address = org.MainAddress.PK;
			consol.NotifyPartyDocumentaryAddress.E2_OA_Address = org.MainAddress.PK;
			consol.NotifyParty2DocumentaryAddress.E2_OA_Address = org.MainAddress.PK;
			consol.NotifyParty3DocumentaryAddress.E2_OA_Address = org.MainAddress.PK;
			consol.JK_OA_CreditorAddress = org.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = org.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = org.MainAddress.PK;
			consol.JK_OA_ContainerYardEmptyPickupAddress = org.MainAddress.PK;
			consol.JK_AgentType = Constants.AgentType.Direct;
			SetupJobConsolCost(consol);

			consol.Transports[0].JW_OA_CarrierAddress = org.MainAddress.PK;

			var container = consol.Containers[0];
			container.JC_ContainerNum = "AAAA0000008";
			container.JC_DeliveryMode = "CFS/CFS";
			container.GrossWeightVerifiedByAddress.E2_OA_Address = org.MainAddress.PK;
			container.JC_OA_DepartureContainerYardAddress = org.MainAddress.PK;

			var shipment = consol.Shipments[0];
			shipment.BuyerDocAddress.E2_OA_Address = org.MainAddress.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = org.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = org.MainAddress.PK;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = org.MainAddress.PK;
			shipment.ConsignorPickupAddress.E2_OA_Address = org.MainAddress.PK;
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = org.MainAddress.PK;
			shipment.NotifyParty2DocumentaryAddress.E2_OA_Address = org.MainAddress.PK;
			shipment.NotifyParty3DocumentaryAddress.E2_OA_Address = org.MainAddress.PK;
			shipment.JS_OA_ExportReceivingDepot = org.MainAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = org.MainAddress.PK;

			var wrapper = CreateDocDataObjectBuilder(consol).Build();

			CombineAssertions(() =>
			{
				var transport1 = wrapper.Transports.Main;
				AssertEquals("Shanghai Hongqiao International Apt", transport1.PortOfLoading.Name);
				AssertEquals("Singapore, Singapore", transport1.PortOfDischarge.Name);
				AssertEquals("Sydney, Australia", transport1.Carrier.Unloco.Name);
				AssertEquals("Sydney, Australia", wrapper.Shipper.Unloco.Name);
				AssertEquals("Sydney, Australia", wrapper.Carrier.Unloco.Name);
				AssertEquals("Sydney, Australia", wrapper.Creditor.Unloco.Name);
				AssertEquals("Sydney, Australia", wrapper.Forwarder.Unloco.Name);
				AssertEquals("Sydney, Australia", wrapper.SendingForwarder.Unloco.Name);
				AssertEquals("Sydney, Australia", wrapper.Buyer.Unloco.Name);
				AssertEquals("Sydney, Australia", wrapper.Consignee.Unloco.Name);
				AssertEquals("Sydney, Australia", wrapper.NotifyParty.Unloco.Name);
				AssertEquals("Sydney, Australia", wrapper.NotifyParty2.Unloco.Name);
				AssertEquals("Sydney, Australia", wrapper.NotifyParty3.Unloco.Name);
				AssertEquals("Sydney, Australia", wrapper.PickupFrom.Unloco.Name);
				AssertEquals("Sydney, Australia", wrapper.DeliverTo.Unloco.Name);
				AssertEquals("Sydney, Australia", wrapper.Recipient.Unloco.Name);
				AssertEquals("Brisbane, Australia", wrapper.CurrentUser.Unloco.Name);
				AssertEquals("Sydney, Australia", wrapper.FreightPayer.Unloco.Name);

				var subShipment = wrapper.Shipments.ToArray()[0];
				AssertEquals("Sydney, Australia", subShipment.Consignee.Unloco.Name);
				AssertEquals("Sydney, Australia", subShipment.Consignor.Unloco.Name);
				AssertEquals("Sydney, Australia", subShipment.PickupFrom.Unloco.Name);
				AssertEquals("Sydney, Australia", subShipment.PickupCFS.Unloco.Name);
				AssertEquals("Sydney, Australia", subShipment.DeliveryTo.Unloco.Name);
				AssertEquals("Sydney, Australia", subShipment.DeliveryCFS.Unloco.Name);

				var subContainer = wrapper.Containers.ToArray()[0];
				AssertEquals("Sydney, Australia", subContainer.DepartureContainerYard.Unloco.Name);
				AssertEquals("Sydney, Australia", subContainer.VerifiedByAddress.Unloco.Name);
			});

			using (Registry.ForwardingConfigurationRegistry.Instance.ShowCountryStateBookingRequestShippingInstruction.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				wrapper = CreateDocDataObjectBuilder(consol).Build();
				CombineAssertions(() =>
				{
					var transport1 = wrapper.Transports.Main;
					AssertEquals("Shanghai Hongqiao International Apt", transport1.PortOfLoading.Name);
					AssertEquals("Singapore", transport1.PortOfDischarge.Name);
					AssertEquals("Sydney", transport1.Carrier.Unloco.Name);

					AssertEquals("Sydney", wrapper.Shipper.Unloco.Name);
					AssertEquals("Sydney", wrapper.Carrier.Unloco.Name);
					AssertEquals("Sydney", wrapper.Creditor.Unloco.Name);
					AssertEquals("Sydney", wrapper.Forwarder.Unloco.Name);
					AssertEquals("Sydney", wrapper.Buyer.Unloco.Name);
					AssertEquals("Sydney", wrapper.Consignee.Unloco.Name);
					AssertEquals("Sydney", wrapper.NotifyParty.Unloco.Name);
					AssertEquals("Sydney", wrapper.NotifyParty2.Unloco.Name);
					AssertEquals("Sydney", wrapper.NotifyParty3.Unloco.Name);
					AssertEquals("Sydney", wrapper.PickupFrom.Unloco.Name);
					AssertEquals("Sydney", wrapper.DeliverTo.Unloco.Name);
					AssertEquals("Sydney", wrapper.Recipient.Unloco.Name);
					AssertEquals("Brisbane", wrapper.CurrentUser.Unloco.Name);
					AssertEquals("Sydney", wrapper.FreightPayer.Unloco.Name);

					var subShipment = wrapper.Shipments.ToArray()[0];
					AssertEquals("Sydney", subShipment.Consignee.Unloco.Name);
					AssertEquals("Sydney", subShipment.Consignor.Unloco.Name);
					AssertEquals("Sydney", subShipment.PickupFrom.Unloco.Name);
					AssertEquals("Sydney", subShipment.PickupCFS.Unloco.Name);
					AssertEquals("Sydney", subShipment.DeliveryTo.Unloco.Name);
					AssertEquals("Sydney", subShipment.DeliveryCFS.Unloco.Name);

					var subContainer = wrapper.Containers.ToArray()[0];
					AssertEquals("Sydney", subContainer.DepartureContainerYard.Unloco.Name);
					AssertEquals("Sydney", subContainer.VerifiedByAddress.Unloco.Name);
				});
			}
		}

		#endregion

		#region ASCII Validation

		public void TestAddAsciiCharactersValidation()
		{
			var consol = CreateConsol();
			var data = CreateDocDataObjectBuilder(CreateConsol()).Build();

			CombineAssertions(() =>
			{
				AssertAsciiCharactersValidation("CurrentUser", data.CurrentUser);
				AssertAsciiCharactersValidation("GoodsHandlingInstructions", data.GoodsHandlingInstructionsInfo);
				AssertAsciiCharactersValidation("ForwardingInstructionsInfo", data.ForwardingInstructionsInfo);
				AssertAsciiCharactersValidation("SpecialInstructionsInfo", data.SpecialInstructionsInfo);
			});
		}

		#endregion

		#region Ports Validation

		public void TestPortsInvalidCodeValidation()
		{
			var consol = CreateConsol();
			var data = CreateDocDataObjectBuilder(CreateConsol()).Build();
			var messageError = "You have not entered a valid unloco.";
			CombineAssertions(() =>
			{
				AssertInvalidCodeValidation("PortOfLoading", data.PortOfLoading, messageError);
				AssertInvalidCodeValidation("PortOfDischarge", data.PortOfDischarge, messageError);
				AssertInvalidCodeValidation("Origin", data.Origin, messageError);
				AssertInvalidCodeValidation("Destination", data.Destination, messageError);
				AssertInvalidCodeValidation("PlaceOfReceipt", data.PlaceOfReceipt, messageError);
				AssertInvalidCodeValidation("PlaceOfDelivery", data.PlaceOfReceipt, messageError);
				AssertInvalidCodeValidation("PlaceOfIssue", data.PlaceOfReceipt, messageError);
			});
		}

		#endregion

		#region PackingLines Validation

		public void TestUnAllocatedPackLinesValidationWarningAndMessageError()
		{
			var consol = CreateConsol();
			var data = CreateDocDataObjectBuilder(consol).Build();
			var message = "There are pack lines on the Consolidation not packed to a container, they may not be included in the message to the carrier. Please fix before sending the message."; // non-translatable registration number
			var allPackingLines = data.Containers.SelectMany(c => c.PackingLines).OfType<PackingLine>();
			data.ContainerMode.Code = Constants.ContainerModes.FCL;

			Assert("consol is not BBK, ROR and BLK", !data.IsNonContainerized);

			AssertNoWarning(data.ErrorPlaceHolderInfo, message);
			AssertNoMessageError(data.ErrorPlaceHolderInfo, message);

			var newPackLine = consol.Shipments.Cast<ForwardingShipment>().First().OuterPackLines.AddNew();
			newPackLine.JL_JC = ZGuid.Empty;
			Factory.Save();

			data = CreateDocDataObjectBuilder(consol).Build();
			AssertNoWarning(data.ErrorPlaceHolderInfo, message);
			AssertHasMessageError(data.ErrorPlaceHolderInfo, message);

			consol.JK_ConsolMode = Constants.ContainerModes.RollOnRollOff;
			Factory.Save();
			data = CreateDocDataObjectBuilder(consol).Build();
			Assert("consol is ROR", data.IsNonContainerized);
			AssertNoWarning(data.ErrorPlaceHolderInfo, message);
			AssertNoMessageError(data.ErrorPlaceHolderInfo, message);
		}

		public void TestUnAllocatedPackLinesValidationWarningAndMessageError_IsGroupAndConsolidatePackingLines()
		{
			var message = "There are pack lines on the Consolidation not packed to a container, they may not be included in the message to the carrier. Please fix before sending the message.";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "CNSHG";
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER1";
			container.JC_IsEmptyContainer = false;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "SHIPMENT1";
			shipment.JS_UnitOfWeight = Constants.Weight.Grams;
			shipment.JS_UnitOfVolume = Constants.Volume.MegaLitre;
			shipment.DetailedGoodsDescriptionNoteText = "SHIPMENT1 DetailedGoodsDescriptionNoteText";
			shipment.JS_GoodsDescription = "SHIPMENT1 JS_GoodsDescription";
			shipment.JS_MarksAndNumbers = "SHIPMENT1 JS_MarksAndNumbers";
			shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
			shipment.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bag;

			shipment.InnerPackLines.RemoveAndDeleteAll();
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var outerPackLine = PopulatePackLine(shipment.OuterPackLines.AddNew(), 1, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, null, null, true, -2, 3, Constants.Temperature.Fahrenheit);
			outerPackLine.JL_JC = container.PK;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = CreateDocDataObjectBuilder(consol).Build();

				AssertNoWarning(data.ErrorPlaceHolderInfo, message);
				AssertNoMessageError(data.ErrorPlaceHolderInfo, message);

				var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
				shippingLine.RSL_IsNVO = true;
				var carrier = Factory.New<OrgHeader>();
				carrier.OH_FullName = "Carrier";
				carrier.OH_RL_NKClosestPort = "AUMEL";
				carrier.OH_RSL_ShippingLine = shippingLine.PK;
				consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

				consol.JK_ConsolMode = Constants.ContainerModes.Groupage;
				consol.JK_AgentType = Constants.AgentType.CoLoad;
				consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;

				data = CreateDocDataObjectBuilder(consol).Build();
				AssertNoWarning(data.ErrorPlaceHolderInfo, message);
				AssertNoMessageError(data.ErrorPlaceHolderInfo, message);

				var packline = shipment.OuterPackLines.AddNew();
				packline.JL_PackageCount = 3;
				packline.JL_F3_NKPackType = "PLT";
				packline.JL_ActualWeight = 20000;
				packline.JL_ActualWeightUQ = "G";
				packline.JL_ActualVolume = 300;
				packline.JL_ActualVolumeUQ = "M3";
				packline.JL_HarmonisedCode = "EEEEE";
				packline.JL_ExportRefNumber = "REF002";
				packline.JL_DetailedDescription = "pack3";
				packline.JL_ContainerPackingOrder = 4;

				packline.JL_JC = ZGuid.Empty;
				Factory.Save();

				data = CreateDocDataObjectBuilder(consol).Build();
				AssertNoWarning(data.ErrorPlaceHolderInfo, message);
				AssertHasMessageError(data.ErrorPlaceHolderInfo, message);

				consol.JK_ConsolMode = Constants.ContainerModes.Bulk;
				Factory.Save();

				data = CreateDocDataObjectBuilder(consol).Build();
				AssertNoWarning(data.ErrorPlaceHolderInfo, message);
				AssertNoMessageError(data.ErrorPlaceHolderInfo, message);
			}
		}

		public void TestNoInnerPackLineValidation()
		{
			var warningMessage = @"There are pack lines with no inner quantity, outer pack quantity has been taken as inners.
Please verify in Shipment>Packing>Packs on following Shipments:
JSASM.";
			var standardServiceLevel = Factory.LoadFromUniqueKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, new ZString("STD"));
			var rule = CreateRefCountryRules(Constants.CountryCodes.UnitedStates, Constants.CountryCodes.China, Constants.TransportModes.Sea, standardServiceLevel.PK, false);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "CNSHG";
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONTAINER1";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONTAINER2";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "JSASM";
			shipment.JS_GoodsDescription = "ASM JS_GoodsDescription";
			shipment.JS_MarksAndNumbers = "ASM JS_MarksAndNumbers";

			shipment.InnerPackLines.RemoveAndDeleteAll();
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var outerPackLine1 = shipment.OuterPackLines.AddNew();
			PopulatePackLine(outerPackLine1
				, 130, Constants.PkgUnit.BaleCompressed
				, 130, Constants.Weight.Kilograms
				, 130, Constants.Volume.CubicMetres
				, "BaleCompressed MarksAndNumbers", "BaleCompressed DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade);

			var innerPackLine1 = shipment.InnerPackLines.AddNew();
			innerPackLine1.JL_PackageCount = 11;
			innerPackLine1.JL_F3_NKPackType = Core.Constants.PkgUnit.Unit;
			innerPackLine1.JL_JL_OuterPackLine = outerPackLine1.PK;

			PopulatePackLine(shipment.OuterPackLines.AddNew()
				, 120, Constants.PkgUnit.BaleCompressed
				, 120, Constants.Weight.Kilograms
				, 120, Constants.Volume.CubicMetres
				, "BaleCompressed MarksAndNumbers", "BaleCompressed DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade);

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentDOs = CreateDocDataObjectBuilder(consol).Build().Shipments;
				AssertEquals(1, shipmentDOs.Count);

				var shipmentDO = shipmentDOs.First();
				shipmentDO.ValidateAllIncludingChildren();

				var outerPackLine1DO = shipmentDO.PackingLines.First(x => x.Quantity == 130);
				var outerPackLine2DO = shipmentDO.PackingLines.First(x => x.Quantity == 120);

				outerPackLine1DO.ValidateAll();
				outerPackLine2DO.ValidateAll();

				AssertNoWarning(outerPackLine1DO.QuantityInfo, warningMessage);
				AssertNoWarning(outerPackLine2DO.QuantityInfo, warningMessage);
			}

			rule.R7_ShowInner = true;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentDOs = CreateDocDataObjectBuilder(consol).Build().Shipments;
				AssertEquals(1, shipmentDOs.Count);

				var shipmentDO = shipmentDOs.First();
				shipmentDO.ValidateAllIncludingChildren();

				var outerPackLine1DO = shipmentDO.PackingLines.First(x => x.Quantity == 11);
				var outerPackLine2DO = shipmentDO.PackingLines.First(x => x.Quantity == 120);

				outerPackLine1DO.ValidateAll();
				outerPackLine2DO.ValidateAll();

				AssertNoWarning(outerPackLine1DO.QuantityInfo, warningMessage);
				AssertHasWarning(outerPackLine2DO.QuantityInfo, warningMessage);
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var shipmentDOs = CreateDocDataObjectBuilder(consol).Build().Shipments;
				AssertEquals(1, shipmentDOs.Count);

				var shipmentDO = shipmentDOs.First();
				shipmentDO.ValidateAllIncludingChildren();

				var outerPackLine1DO = shipmentDO.PackingLines.First(x => x.Quantity == 130);
				var outerPackLine2DO = shipmentDO.PackingLines.First(x => x.Quantity == 120);

				outerPackLine1DO.ValidateAll();
				outerPackLine2DO.ValidateAll();

				AssertNoWarning(outerPackLine1DO.QuantityInfo, warningMessage);
				AssertNoWarning(outerPackLine2DO.QuantityInfo, warningMessage);
			}
		}

		#endregion

		#region Test Get Max Length

		public void TestGetMaxLength()
		{
			var consol = CreateConsol();
			var builder = CreateDocDataObjectBuilder(consol).Build();

			var dynamicData = builder.MakeDynamic(metaDataProvider: new DocDataObjectMetaDataProvider(), dynamicDataFactory: new DocDataObjectDynamicDataFactory());

			var dynamicDataContainers = (IDynamicDataCollection)dynamicData.GetDynamicProperty("Containers");
			AssertNotNull(dynamicDataContainers);

			var dynamicDataPackingLines = (IDynamicDataCollection)dynamicDataContainers.GetAllElements().FirstOrDefault().GetDynamicProperty("PackingLines");
			AssertNotNull(dynamicDataPackingLines);

			var dynamicDataGoodsDescription = dynamicDataPackingLines.GetAllElements().FirstOrDefault().GetDynamicProperty("GoodsDescription");
			AssertNotNull(dynamicDataGoodsDescription);

			AssertEquals(31981, dynamicDataGoodsDescription.GetMaxLength());

			var dynamicDataMarksAndNumbers = dynamicDataPackingLines.GetAllElements().FirstOrDefault().GetDynamicProperty("MarksAndNumbers");
			AssertNotNull(dynamicDataMarksAndNumbers);

			AssertEquals(31981, dynamicDataMarksAndNumbers.GetMaxLength());
		}

		#endregion

		#region ShippingLineMessagingRequirements Validation

		public void TestContractNumberValidation_CoLoad() => TestContractNumberValidation(true);
		public void TestNamedAccountValidation_CoLoad() => TestNamedAccountValidation(true);
		public void TestDangerousGoodsWeightValidation_CoLoad() => TestDangerousGoodsWeightValidation(true);
		public void TestAcceptEitherAirflowOrHumidityValidation_CoLoad() => TestAcceptEitherAirflowOrHumidityValidation(true);
		public void TestDimensionsForOOGValidationPackLine_CoLoad() => TestDimensionsForOOGValidationPackLine(true);
		public void TestDimensionsForOOGValidationContainer_CoLoad() => TestDimensionsForOOGValidationContainer(true);
		public void TestDimensionsForOOGValidation_IsGroupAndConsolidatePackingLines_CoLoad() => TestDimensionsForOOGValidation_IsGroupAndConsolidatePackingLines(true);
		public void TestDangerousGoodsWeightValidation_IsGroupAndConsolidatePackingLines_CoLoad() => TestDangerousGoodsWeightValidation_IsGroupAndConsolidatePackingLines(true);

		public void TestContractNumberValidation_NonCoLoad() => TestContractNumberValidation(false);
		public void TestNamedAccountValidation_NonCoLoad() => TestNamedAccountValidation(false);
		public void TestDangerousGoodsWeightValidation_NonCoLoad() => TestDangerousGoodsWeightValidation(false);
		public void TestAcceptEitherAirflowOrHumidityValidation_NonCoLoad() => TestAcceptEitherAirflowOrHumidityValidation(false);
		public void TestDimensionsForOOGValidationPackLine_NonCoLoad() => TestDimensionsForOOGValidationPackLine(false);
		public void TestDimensionsForOOGValidationContainer_NonCoLoad() => TestDimensionsForOOGValidationContainer(false);
		public void TestDimensionsForOOGValidation_IsGroupAndConsolidatePackingLines_NonCoLoad() => TestDimensionsForOOGValidation_IsGroupAndConsolidatePackingLines(false);
		public void TestDangerousGoodsWeightValidation_IsGroupAndConsolidatePackingLines_NonCoLoad() => TestDangerousGoodsWeightValidation_IsGroupAndConsolidatePackingLines(false);

		public void TestContractNumberValidation(bool isCoLoad)
		{
			var consol = CreateConsolWithRefShippingLineMessagingRequirement(isCoLoad, "CON");
			var data = CreateDocDataObjectBuilder(consol).Build();
			AssertNoMessageError(data.CarrierContractNumbersFormattedInfo, ShippingLineMessagingRequirement.ValidationMessages.ContractNumberMandatory);

			consol.JK_CarrierContractNumber = null;
			data = CreateDocDataObjectBuilder(consol).Build();
			AssertHasMessageError(data.CarrierContractNumbersFormattedInfo, ShippingLineMessagingRequirement.ValidationMessages.ContractNumberMandatory);

			consol = CreateConsolWithRefShippingLineMessagingRequirement(isCoLoad, "CON", false);
			data = CreateDocDataObjectBuilder(consol).Build();
			AssertNoMessageError(data.CarrierContractNumbersFormattedInfo, ShippingLineMessagingRequirement.ValidationMessages.ContractNumberMandatory);
		}

		public void TestNamedAccountValidation(bool isCoLoad)
		{
			var consol = CreateConsolWithRefShippingLineMessagingRequirement(isCoLoad, "NAM");
			var data = CreateDocDataObjectBuilder(consol).Build();
			AssertHasMessageError(data.ContractNamedAccountInfo, ShippingLineMessagingRequirement.ValidationMessages.NamedAccountMandatory);

			consol.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount, "Contract Named Account");
			data = CreateDocDataObjectBuilder(consol).Build();
			AssertNoMessageError(data.ContractNamedAccountInfo, ShippingLineMessagingRequirement.ValidationMessages.NamedAccountMandatory);

			consol = CreateConsolWithRefShippingLineMessagingRequirement(isCoLoad, "NAM", false);
			data = CreateDocDataObjectBuilder(consol).Build();
			AssertNoMessageError(data.ContractNamedAccountInfo, ShippingLineMessagingRequirement.ValidationMessages.NamedAccountMandatory);
		}

		public void TestDangerousGoodsWeightValidation(bool isCoLoad)
		{
			var consol = CreateConsolWithRefShippingLineMessagingRequirement(isCoLoad, "DGW");
			var data = CreateDocDataObjectBuilder(consol).Build();
			var dangerousGood = data.Containers.SelectMany(c => c.PackingLines.SelectMany(p => p.DangerousGoods)).First();
			AssertNoMessageError(dangerousGood.Weight.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.DGNetWeightMandatory);

			dangerousGood.Weight.Value = 0;
			AssertHasMessageError(dangerousGood.Weight.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.DGNetWeightMandatory);

			consol = CreateConsolWithRefShippingLineMessagingRequirement(isCoLoad, "DGW", false);
			data = CreateDocDataObjectBuilder(consol).Build();
			dangerousGood = data.Containers.SelectMany(c => c.PackingLines.SelectMany(p => p.DangerousGoods)).First();
			AssertNoMessageError(dangerousGood.Weight.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.DGNetWeightMandatory);
		}

		public void TestDangerousGoodsWeightValidation_IsGroupAndConsolidatePackingLines(bool isCoLoad)
		{
			void SetupAndAssertDangerousGoodsWeight(bool requirementFlag, bool hasWeight, bool shouldHaveError)
			{
				var shippingLine = Factory.New<RefShippingLine>();
				shippingLine.RSL_OceanCarrierMessagingAvailable = true;
				shippingLine.RSL_IsNVO = isCoLoad;

				var requirement = shippingLine.ShippingLineMessagingRequirements.AddNew();
				requirement.RSR_RST_NKType = "DGW";
				requirement.RSR_IsBookingRequest = requirementFlag;
				requirement.RSR_IsShippingInstruction = requirementFlag;

				var carrierOrCoLoader = Factory.New<OrgHeader>();
				carrierOrCoLoader.OH_FullName = "CarrierOrCoLoader";
				carrierOrCoLoader.OH_RL_NKClosestPort = "AUMEL";
				carrierOrCoLoader.OH_RSL_ShippingLine = shippingLine.PK;

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = "USCHI";
				consol.JK_RL_NKDischargePort = "CNSHG";
				consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;

				if (isCoLoad)
				{
					consol.JK_AgentType = Constants.AgentType.CoLoad;
					consol.JK_OA_CreditorAddress = carrierOrCoLoader.MainAddress.PK;
				}
				else
				{
					consol.JK_OA_ShippingLineAddress = carrierOrCoLoader.MainAddress.PK;
				}
				consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;

				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = "CONTAINER1";
				container.JC_IsEmptyContainer = false;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_UniqueConsignRef = "SHIPMENT1";
				shipment.JS_UnitOfWeight = Constants.Weight.Grams;
				shipment.JS_UnitOfVolume = Constants.Volume.MegaLitre;
				shipment.DetailedGoodsDescriptionNoteText = "SHIPMENT1 DetailedGoodsDescriptionNoteText";
				shipment.JS_GoodsDescription = "SHIPMENT1 JS_GoodsDescription";
				shipment.JS_MarksAndNumbers = "SHIPMENT1 JS_MarksAndNumbers";
				shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
				shipment.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bag;

				shipment.InnerPackLines.RemoveAndDeleteAll();
				shipment.OuterPackLines.RemoveAndDeleteAll();

				var outerPackLine = PopulatePackLine(shipment.OuterPackLines.AddNew(), 1, Constants.PkgUnit.Pail, 0, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, null, null, true, -2, 3, Constants.Temperature.Fahrenheit);
				outerPackLine.JL_JC = container.PK;

				var dangerousGood = outerPackLine.UNDGs.AddNew();

				using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					dangerousGood.DI_DGWeight = hasWeight ? 1 : 0;
					dangerousGood.DI_UnitOfWeight = Constants.Weight.Kilograms;

					var data = CreateDocDataObjectBuilder(consol).Build();
					var dangerousGoodDO = data.Shipments.First().PackingLines.First().PackingLines.First().DangerousGoods.First();

					if (shouldHaveError)
					{
						AssertHasMessageError(dangerousGoodDO.Weight.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.DGNetWeightMandatory);
					}
					else
					{
						AssertNoMessageError(dangerousGoodDO.Weight.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.DGNetWeightMandatory);
					}
				}
			}

			SetupAndAssertDangerousGoodsWeight(true, true, false);
			SetupAndAssertDangerousGoodsWeight(false, true, false);
			SetupAndAssertDangerousGoodsWeight(false, false, false);
			SetupAndAssertDangerousGoodsWeight(true, false, true);
		}

		public void TestAcceptEitherAirflowOrHumidityValidation(bool isCoLoad)
		{
			SetupAndAssertAirflowHumidity(true, false, false, false);
			SetupAndAssertAirflowHumidity(true, true, true, true);
			SetupAndAssertAirflowHumidity(true, false, true, false);
			SetupAndAssertAirflowHumidity(true, true, false, false);

			SetupAndAssertAirflowHumidity(false, false, false, false);
			SetupAndAssertAirflowHumidity(false, true, true, false);

			SetupAndAssertAirflowHumidityOtherCases(0, 0, ZString.Empty, false, true);
			SetupAndAssertAirflowHumidityOtherCases(5, 0, "MQH", true, true);

			void SetupAndAssertAirflowHumidity(bool requirementFlag, bool hasHumidity, bool hasAirVentFlow, bool shouldHaveError)
			{
				var consol = CreateConsolWithRefShippingLineMessagingRequirement(isCoLoad, "AOH", requirementFlag);
				var consolContainer = (ForwardingContainer)consol.Containers.First();
				consolContainer.JC_IsControlledAtmosphere = true;

				if (hasHumidity)
				{
					consolContainer.JC_HumidityPercent = 5;
				}

				if (hasAirVentFlow)
				{
					consolContainer.JC_AirVentFlow = 5;
					consolContainer.JC_AirVentFlowRateUnit = "MQH";
				}

				var data = CreateDocDataObjectBuilder(consol).Build();
				var container = data.Containers.First();

				if (shouldHaveError)
				{
					AssertHasMessageError(container.Humidity.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.AcceptEitherAirflowOrHumidity);
					AssertHasMessageError(container.AirVentFlow.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.AcceptEitherAirflowOrHumidity);
				}
				else
				{
					AssertNoMessageError(container.Humidity.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.AcceptEitherAirflowOrHumidity);
					AssertNoMessageError(container.AirVentFlow.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.AcceptEitherAirflowOrHumidity);
				}
			}

			void SetupAndAssertAirflowHumidityOtherCases(int humidityPercent, int airVentFlow, string airVentFlowRateUnit, bool shouldHaveError, bool requirementFlag)
			{
				var consol = CreateConsolWithRefShippingLineMessagingRequirement(isCoLoad, "AOH", requirementFlag);
				var consolContainer = (ForwardingContainer)consol.Containers.First();
				consolContainer.JC_IsControlledAtmosphere = true;

				consolContainer.JC_HumidityPercent = (ZByte)humidityPercent;
				consolContainer.JC_AirVentFlow = airVentFlow;
				consolContainer.JC_AirVentFlowRateUnit = airVentFlowRateUnit;

				var data = CreateDocDataObjectBuilder(consol).Build();
				var container = data.Containers.First();

				if (shouldHaveError)
				{
					AssertHasMessageError(container.Humidity.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.AcceptEitherAirflowOrHumidity);
					AssertHasMessageError(container.AirVentFlow.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.AcceptEitherAirflowOrHumidity);
				}
				else
				{
					AssertNoMessageError(container.Humidity.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.AcceptEitherAirflowOrHumidity);
					AssertNoMessageError(container.AirVentFlow.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.AcceptEitherAirflowOrHumidity);
				}
			}
		}

		public void TestContainerValidation_WithAirVentFlow()
		{
			var consol = CreateConsolWithRefShippingLineMessagingRequirement(true, "AOH", true);
			var consolContainer = (ForwardingContainer)consol.Containers.First();

			var data = CreateDocDataObjectBuilder(consol).Build();
			var container = data.Containers.First();

			var airVentUnitErrorMessage = "Selected Air Vent Setting measurement type is not supported by the carrier. Ensure each temperature controlled container has selected either '2L' or 'MQH' on the Containers > Refrigeration tab.";
			var airVentWarningMessageWhenFlowRateIs0AndUnitIsMQH = "Air Vent is marked as \"Open\".\r\nTo indicate \"Closed\", leave the Consol > Containers > Refrigeration > Air Vent Setting > Unit field blank.";
			var airVentWarningMessageWhenFlowRateIsNonZeroAndUnitIs2L = "Carriers accept airflow only in metric units.\r\nAny value entered as cubic feet per minute (2L) is automatically converted to cubic meters per hour (MQH) during transmission.";

			SetupAndAssertContainerValidation_WithAirVentFlow(true, "2L", 0, false, false, airVentUnitErrorMessage);
			SetupAndAssertContainerValidation_WithAirVentFlow(true, "ABC", 1, true, false, airVentUnitErrorMessage);
			SetupAndAssertContainerValidation_WithAirVentFlow(false, "ABC", 1, false, false, airVentUnitErrorMessage);
			SetupAndAssertContainerValidation_WithAirVentFlow(true, ZString.Empty, 0, false, false, airVentUnitErrorMessage);
			SetupAndAssertContainerValidation_WithAirVentFlow(true, "MQH", 0, false, true, airVentWarningMessageWhenFlowRateIs0AndUnitIsMQH);
			SetupAndAssertContainerValidation_WithAirVentFlow(true, "2L", 1, false, true, airVentWarningMessageWhenFlowRateIsNonZeroAndUnitIs2L);

			void SetupAndAssertContainerValidation_WithAirVentFlow(bool isControlledAtmosphere, string airVentFlowRateUnit, int airVentFlow, bool hasError, bool hasWarning, string errorMessage)
			{
				consolContainer.JC_AirVentFlowRateUnit = airVentFlowRateUnit;
				consolContainer.JC_AirVentFlow = airVentFlow;
				consolContainer.JC_IsControlledAtmosphere = isControlledAtmosphere;
				data = CreateDocDataObjectBuilder(consol).Build();
				container = data.Containers.First();

				if (hasError)
				{
					AssertHasMessageError(((CodeDescription)container.AirVentFlow.Unit).CodeInfo, errorMessage);
				}
				else
				{
					AssertNoMessageError(((CodeDescription)container.AirVentFlow.Unit).CodeInfo, errorMessage);
				}

				if (hasWarning)
				{
					AssertHasWarning(((CodeDescription)container.AirVentFlow.Unit).CodeInfo, errorMessage);
				}
				else
				{
					AssertNoWarning(((CodeDescription)container.AirVentFlow.Unit).CodeInfo, errorMessage);
				}
			}
		}

		public void TestDimensionsForOOGValidationPackLine(bool isCoLoad)
		{
			void SetupAndAssertDimensions(bool requirementFlag, string containerType, bool isOutOfGauge, bool hasDimensions, bool userFlagsOutOfGauge, bool shouldHaveError)
			{
				var consol = CreateConsolWithRefShippingLineMessagingRequirement(isCoLoad, "DOG", requirementFlag);

				var consolContainer = (ForwardingContainer)consol.Containers.First();
				var refContainer = Factory.New<RefContainer>();
				refContainer.RC_ISOType = containerType;
				refContainer.RC_Length = 10;
				refContainer.RC_Width = 10;
				refContainer.RC_Height = 10;
				consolContainer.JC_RC = refContainer.PK;

				if (isOutOfGauge)
				{
					consolContainer.JC_TotalLength = 555;
					consolContainer.JC_TotalWidth = 555;
					consolContainer.JC_TotalHeight = 555;
				}

				if (hasDimensions)
				{
					var consolPackline = (PackLine)consolContainer.PackLines.First();
					consolPackline.JL_Length = 1;
					consolPackline.JL_Width = 1;
					consolPackline.JL_Height = 1;
				}

				var data = CreateDocDataObjectBuilder(consol).Build();
				var packline = data.Containers.SelectMany(c => c.PackingLines).First();

				if (!isOutOfGauge)
				{
					data.IsOutOfGauge = userFlagsOutOfGauge;
				}

				if (shouldHaveError)
				{
					AssertHasMessageError(packline.Height.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.DimensionsMandatoryForOOGPackline);
					AssertHasMessageError(packline.Length.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.DimensionsMandatoryForOOGPackline);
					AssertHasMessageError(packline.Width.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.DimensionsMandatoryForOOGPackline);
				}
				else
				{
					AssertNoMessageError(packline.Height.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.DimensionsMandatoryForOOGPackline);
					AssertNoMessageError(packline.Length.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.DimensionsMandatoryForOOGPackline);
					AssertNoMessageError(packline.Width.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.DimensionsMandatoryForOOGPackline);
				}

				AssertNoMessageErrors(data.ErrorPlaceHolderInfo);
			}

			//user cannot flag OOG when already hasOverhang (OOG on template will not be modifiable)
			SetupAndAssertDimensions(true, "UUUU", true, false, false, true);
			SetupAndAssertDimensions(true, "PPPP", true, false, false, true);
			SetupAndAssertDimensions(true, "AAAA", true, false, false, false);

			SetupAndAssertDimensions(true, "UUUU", true, true, false, false);
			SetupAndAssertDimensions(true, "PPPP", true, true, false, false);
			SetupAndAssertDimensions(true, "AAAA", true, true, false, false);

			//user flags OOG
			SetupAndAssertDimensions(true, "UUUU", false, false, true, true);
			SetupAndAssertDimensions(true, "PPPP", false, false, true, true);
			SetupAndAssertDimensions(true, "AAAA", false, false, true, false);

			SetupAndAssertDimensions(true, "UUUU", false, true, true, false);
			SetupAndAssertDimensions(true, "PPPP", false, true, true, false);
			SetupAndAssertDimensions(true, "AAAA", false, true, true, false);

			//user flags NOT OOG
			SetupAndAssertDimensions(true, "UUUU", false, false, false, false);
			SetupAndAssertDimensions(true, "PPPP", false, false, false, false);
			SetupAndAssertDimensions(true, "AAAA", false, false, false, false);

			SetupAndAssertDimensions(true, "UUUU", false, true, false, false);
			SetupAndAssertDimensions(true, "PPPP", false, true, false, false);
			SetupAndAssertDimensions(true, "AAAA", false, true, false, false);
		}

		public void TestDimensionsForOOGValidationContainer(bool isCoLoad)
		{
			void SetupAndAssertDimensions(bool requirementFlag, string containerType, bool isOutOfGauge, bool userFlagsOutOfGauge, bool shouldHaveError)
			{
				var consol = CreateConsolWithRefShippingLineMessagingRequirement(isCoLoad, "DOG", requirementFlag);

				var consolContainer = (ForwardingContainer)consol.Containers.First();
				var refContainer = Factory.New<RefContainer>();
				refContainer.RC_ISOType = containerType;
				refContainer.RC_Length = 10;
				refContainer.RC_Width = 10;
				refContainer.RC_Height = 10;
				consolContainer.JC_RC = refContainer.PK;

				if (isOutOfGauge)
				{
					consolContainer.JC_TotalLength = 555;
					consolContainer.JC_TotalWidth = 555;
					consolContainer.JC_TotalHeight = 555;
				}

				var data = CreateDocDataObjectBuilder(consol).Build();

				if (!isOutOfGauge)
				{
					data.IsOutOfGauge = userFlagsOutOfGauge;
				}

				if (shouldHaveError)
				{
					AssertHasMessageError(data.IsOutOfGaugeInfo, ShippingLineMessagingRequirement.ValidationMessages.DimensionsMandatoryForOOGContainer);
				}
				else
				{
					AssertNoMessageErrors(data.IsOutOfGaugeInfo);
				}

				AssertNoMessageErrors(data.ErrorPlaceHolderInfo);
			}

			//user cannot flag OOG when already hasOverhang (OOG on template will not be modifiable)
			SetupAndAssertDimensions(false, "UUUU", true, false, false);
			SetupAndAssertDimensions(false, "PPPP", true, false, false);
			SetupAndAssertDimensions(false, "AAAA", true, false, false);

			//user flags OOG
			SetupAndAssertDimensions(false, "UUUU", false, true, true);
			SetupAndAssertDimensions(false, "PPPP", false, true, true);
			SetupAndAssertDimensions(false, "AAAA", false, true, false);

			//user flags NOT OOG
			SetupAndAssertDimensions(false, "UUUU", false, false, false);
			SetupAndAssertDimensions(false, "PPPP", false, false, false);
			SetupAndAssertDimensions(false, "AAAA", false, false, false);
		}

		public void TestDimensionsForOOGValidation_IsGroupAndConsolidatePackingLines(bool isCoLoad)
		{
			var message = "This carrier requires Dimensions for out of gauge cargo for Open Top or Flat Rack containers.\r\nPlease ensure Consol > Details > Docs > Package Grouping is set to 'DNG' and dimensions have been entered against each pack line.";

			void SetupAndAssertDimensions(bool requirementFlag, string containerType, bool hasDimensions, bool userFlagsOutOfGauge, bool shouldHaveError)
			{
				var shippingLine = Factory.New<RefShippingLine>();
				shippingLine.RSL_OceanCarrierMessagingAvailable = true;
				shippingLine.RSL_IsNVO = isCoLoad;

				var requirement = shippingLine.ShippingLineMessagingRequirements.AddNew();
				requirement.RSR_RST_NKType = "DOG";
				requirement.RSR_IsBookingRequest = requirementFlag;
				requirement.RSR_IsShippingInstruction = requirementFlag;

				var carrierOrCoLoader = Factory.New<OrgHeader>();
				carrierOrCoLoader.OH_FullName = "CarrierOrCoLoader";
				carrierOrCoLoader.OH_RL_NKClosestPort = "AUMEL";
				carrierOrCoLoader.OH_RSL_ShippingLine = shippingLine.PK;

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = "USCHI";
				consol.JK_RL_NKDischargePort = "CNSHG";
				consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;

				if (isCoLoad)
				{
					consol.JK_AgentType = Constants.AgentType.CoLoad;
					consol.JK_OA_CreditorAddress = carrierOrCoLoader.MainAddress.PK;
				}
				else
				{
					consol.JK_OA_ShippingLineAddress = carrierOrCoLoader.MainAddress.PK;
				}
				consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;

				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = "CONTAINER1";
				container.JC_IsEmptyContainer = false;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_UniqueConsignRef = "SHIPMENT1";
				shipment.JS_UnitOfWeight = Constants.Weight.Grams;
				shipment.JS_UnitOfVolume = Constants.Volume.MegaLitre;
				shipment.DetailedGoodsDescriptionNoteText = "SHIPMENT1 DetailedGoodsDescriptionNoteText";
				shipment.JS_GoodsDescription = "SHIPMENT1 JS_GoodsDescription";
				shipment.JS_MarksAndNumbers = "SHIPMENT1 JS_MarksAndNumbers";
				shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
				shipment.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bag;

				shipment.InnerPackLines.RemoveAndDeleteAll();
				shipment.OuterPackLines.RemoveAndDeleteAll();

				var outerPackLine = PopulatePackLine(shipment.OuterPackLines.AddNew(), 1, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, null, null, true, -2, 3, Constants.Temperature.Fahrenheit);
				outerPackLine.JL_JC = container.PK;

				using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var consolContainer = (ForwardingContainer)consol.Containers.First();
					var refContainer = Factory.New<RefContainer>();
					refContainer.RC_ISOType = containerType;
					consolContainer.JC_RC = refContainer.PK;

					outerPackLine.JL_Length = hasDimensions ? 1 : 0;
					outerPackLine.JL_Width = hasDimensions ? 1 : 0;
					outerPackLine.JL_Height = hasDimensions ? 1 : 0;

					var data = CreateDocDataObjectBuilder(consol).Build();
					var packline = data.Containers.SelectMany(c => c.PackingLines).First();

					data.IsOutOfGauge = userFlagsOutOfGauge;

					if (shouldHaveError)
					{
						AssertHasMessageError(data.ErrorPlaceHolderInfo, message);
					}
					else
					{
						AssertNoMessageError(data.ErrorPlaceHolderInfo, message);
					}
				}
			}

			SetupAndAssertDimensions(true, "UUUU", false, false, true);
			SetupAndAssertDimensions(true, "PPPP", false, false, true);
			SetupAndAssertDimensions(true, "AAAA", false, false, false);

			SetupAndAssertDimensions(true, "UUUU", false, true, true);
			SetupAndAssertDimensions(true, "PPPP", false, true, true);
			SetupAndAssertDimensions(true, "AAAA", false, true, true);

			SetupAndAssertDimensions(true, "UUUU", true, false, false);
			SetupAndAssertDimensions(true, "PPPP", true, false, false);
			SetupAndAssertDimensions(true, "AAAA", true, false, false);

			SetupAndAssertDimensions(true, "UUUU", true, true, true);
			SetupAndAssertDimensions(true, "PPPP", true, true, true);
			SetupAndAssertDimensions(true, "AAAA", true, true, true);

			SetupAndAssertDimensions(false, "UUUU", false, false, false);
			SetupAndAssertDimensions(false, "PPPP", false, false, false);
			SetupAndAssertDimensions(false, "AAAA", false, false, false);

			SetupAndAssertDimensions(false, "UUUU", false, true, false);
			SetupAndAssertDimensions(false, "PPPP", false, true, false);
			SetupAndAssertDimensions(false, "AAAA", false, true, false);
		}

		ForwardingConsol CreateConsolWithRefShippingLineMessagingRequirement(bool isCoLoad, string requirementType, bool requirementFlag = true)
		{
			var consol = CreateConsol();

			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_OceanCarrierMessagingAvailable = true;
			shippingLine.RSL_IsNVO = isCoLoad;

			var requirement = shippingLine.ShippingLineMessagingRequirements.AddNew();
			requirement.RSR_RST_NKType = requirementType;
			requirement.RSR_IsBookingRequest = requirementFlag;
			requirement.RSR_IsShippingInstruction = requirementFlag;

			var carrierOrCoLoader = Factory.New<OrgHeader>();
			carrierOrCoLoader.OH_FullName = "CarrierOrCoLoader";
			carrierOrCoLoader.OH_RL_NKClosestPort = "AUMEL";
			carrierOrCoLoader.OH_RSL_ShippingLine = shippingLine.PK;

			if (isCoLoad)
			{
				consol.JK_AgentType = Constants.AgentType.CoLoad;
				consol.JK_OA_CreditorAddress = carrierOrCoLoader.MainAddress.PK;
			}
			else
			{
				consol.JK_OA_ShippingLineAddress = carrierOrCoLoader.MainAddress.PK;
			}

			return consol;
		}

		#endregion

		#region TestPopulateShipper

		public void TestShipper()
		{
			var consol = CreateConsol();
			var builder = CreateDocDataObjectBuilder(consol).Build();
			var sendingAgentAddress = @"I'm Sending Stuff
Unit 200
55 Why Lane
Sender Name
name@sender.com
1111111
2222222";

			Assert("Precondition", !consol.IsDirect);
			AssertEquals("Shipper without MasterBillShipperOverride", sendingAgentAddress, builder.Shipper.ToAssertString());

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "MBSORG";
			org.OH_FullName = "MBS Organisation";
			org.MainAddress.Address1 = "MBS Address 1";
			org.MainAddress.Address2 = "MBS Address 2";
			consol.MasterBillShipperOverrideDocumentaryAddress.OrganisationPK = org.PK;

			builder = CreateDocDataObjectBuilder(consol).Build();
			var masterBillShipperOverrideAddress = @"MBS Organisation
MBS Address 1
MBS Address 2";

			AssertEquals("Shipper with MasterBillShipperOverride", masterBillShipperOverrideAddress, builder.Shipper.ToAssertString());
		}

		#endregion

		#region TestPopulateConsignee

		public void TestConsignee()
		{
			var consol = CreateConsol();
			var builder = CreateDocDataObjectBuilder(consol).Build();
			var receivingAgentAddress = @"I'm Receiving Stuff
Unit 399
50 What Lane
Receiver Name
name@receiver.com
3333333
4444444";

			Assert("Precondition", !consol.IsDirect);
			AssertEquals("Consignee without MasterBillConsigneeOverride", receivingAgentAddress, builder.Consignee.ToAssertString());

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "MBCORG";
			org.OH_FullName = "MBC Organisation";
			org.MainAddress.Address1 = "MBC Address 1";
			org.MainAddress.Address2 = "MBC Address 2";
			consol.MasterBillConsigneeOverrideDocumentaryAddress.OrganisationPK = org.PK;

			builder = CreateDocDataObjectBuilder(consol).Build();
			var masterBillConsigneeOverrideAddress = @"MBC Organisation
MBC Address 1
MBC Address 2";

			AssertEquals("Consignee with MasterBillConsigneeOverride", masterBillConsigneeOverrideAddress, builder.Consignee.ToAssertString());
		}

		#endregion

		#region TestPopulateShipperCompanyName

		public void TestPopulateShipperCompanyName()
		{
			VerifiedGrossMassTest.InitConsolAndShipmentForShipperCompanyNameTest(Factory, out var consol, out _, out var forwarder, out var consignor);

			var trasport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			trasport.JW_LegOrder = 1;
			trasport.JW_TransportMode = Constants.TransportModes.Sea;
			trasport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			trasport.JW_RL_NKLoadPort = "CNSHA";
			trasport.JW_RL_NKDiscPort = "SGSIN";
			trasport.JW_Vessel = "Sea Dragon";
			trasport.JW_VoyageFlight = "F9999";

			AssertEquals("Pre-Condition", false, consol.IsDirect);

			var builder = CreateDocDataObjectBuilder(consol);
			var carrierMessageData = builder.Build();
			AssertEquals($"{forwarder.OH_FullName} {forwarder.MiscServ.Lookups.AsAgentOptions.GetDescriptionFromCode(OrgMiscServLookups.AsAgentOption.AsAgentForCarrier)} AAA Lines", carrierMessageData.Shipper.CompanyName);

			consol.JK_AgentType = Constants.AgentType.Direct;
			AssertEquals(true, consol.IsDirect);

			builder = CreateDocDataObjectBuilder(consol);
			carrierMessageData = builder.Build();
			AssertEquals($"{consignor.OH_FullName} {consignor.MiscServ.Lookups.AsAgentOptions.GetDescriptionFromCode(OrgMiscServLookups.AsAgentOption.AsCarrier)} BBB Lines", carrierMessageData.Shipper.CompanyName);
		}

		#endregion

		#region CustomsBroker

		public void TestPopulateCustomsBroker()
		{
			var consol = CreateConsol();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "CONSPA";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.MainAddress.Address1 = "Unit 15";
			orgHeader.MainAddress.Address2 = "5 Lost Lane";
			orgHeader.MainAddress.City = "Sydney";
			orgHeader.MainAddress.Postcode = "2000";
			orgHeader.MainAddress.OA_RN_NKCountryCode = "AU";

			Factory.Save();

			var declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = "EXP";
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.JE_RL_NKFinalDestination = "ZAAAM";
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_JS = consol.Shipments[0].PK;
			declaration.JE_OH_ExternalBroker = orgHeader.PK;

			var wrapper = CreateDocDataObjectBuilder(consol).Build();

			if (wrapper.DocumentName == DataContext.BookingRequest)
			{
				AssertEquals("CONSPA", wrapper.CustomsBroker.CompanyName);
				AssertEquals("Sydney", wrapper.CustomsBroker.Unloco.Name);
			}
			else
			{
				AssertNull(wrapper.CustomsBroker);
			}
		}

		public void TestCustomsBrokerAddressValidation()
		{
			var warningMessage = "Customs Broker details may be required by the Carrier for Exports from certain locations.\r\nConfirm with the Carrier and update this information on the Shipment if necessary.";

			var consol = CreateConsol();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;

			GlbBranch.CurrentBranch.OrgProxy.OH_IsBroker = false;
			GlbCompany.CurrentCompany.OrgProxy.OH_IsBroker = false;

			Factory.Save();

			var wrapper = CreateDocDataObjectBuilder(consol).Build();

			if (wrapper.DocumentName == DataContext.BookingRequest)
			{
				AssertHasWarning(wrapper.CustomsBroker.CompanyNameInfo, warningMessage);
			}
			else
			{
				AssertNull(wrapper.CustomsBroker);
			}

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "CONSPA";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.MainAddress.Address1 = "Unit 15";
			orgHeader.MainAddress.Address2 = "5 Lost Lane";
			orgHeader.MainAddress.City = "Sydney";
			orgHeader.MainAddress.Postcode = "2000";
			orgHeader.MainAddress.OA_RN_NKCountryCode = "AU";

			var declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = "EXP";
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.JE_RL_NKFinalDestination = "ZAAAM";
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_JS = consol.Shipments[0].PK;
			declaration.JE_OH_ExternalBroker = orgHeader.PK;

			wrapper = CreateDocDataObjectBuilder(consol).Build();

			if (wrapper.DocumentName == DataContext.BookingRequest)
			{
				AssertNoWarning(wrapper.CustomsBroker.CompanyNameInfo, warningMessage);
			}
			else
			{
				AssertNull(wrapper.CustomsBroker);
			}
		}

		#endregion

		#region TestPopulateForwarderCompanyName_BookingRequest

		public void TestPopulateForwarderCompanyName_BookingRequest()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "Consignor";
			consignor.MiscServ.OM_FWAsAgentOption = OrgMiscServLookups.AsAgentOption.AsCarrier;
			consignor.MiscServ.OM_FWAsAgentName = "BBB Lines";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "CNSHA";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_Vessel = "Sea Dragon";
			transport.JW_VoyageFlight = "F9999";

			var builder = new BookingRequestBuilder(consol, null);
			var carrierMessageData = builder.Build();
			AssertEquals(true,
				carrierMessageData.Forwarder.CompanyNameInfo.HasMessageError(
					"Forwarder party name and address information is required."));

			var forwarder = Factory.New<OrgHeader>();
			forwarder.OH_FullName = "Forwarder";
			forwarder.MiscServ.OM_FWAsAgentOption = OrgMiscServLookups.AsAgentOption.AsAgentForCarrier;
			forwarder.MiscServ.OM_FWAsAgentName = "AAA Lines";
			forwarder.OH_FullName = "I'm Sending Stuff";
			forwarder.OH_RL_NKClosestPort = "CNNJI";
			forwarder.MainAddress.Address1 = "Unit 200";
			forwarder.MainAddress.Address2 = "55 Why Lane";
			forwarder.MainAddress.City = "Conficious Ave";
			forwarder.MainAddress.Postcode = "10000";
			forwarder.MainAddress.OA_RN_NKCountryCode = "CN";

			consol.JK_OA_SendingForwarderAddress = forwarder.MainAddress.PK;

			builder = new BookingRequestBuilder(consol, null);
			carrierMessageData = builder.Build();
			AssertEquals(false,
				carrierMessageData.Forwarder.CompanyNameInfo.HasMessageError(
					"Forwarder party name and address information is required."));
		}

		#endregion

		#region TestPopulateForwarderCompanyName_ShippingInstructions

		public void TestPopulateForwarderCompanyName_ShippingInstructions()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = Constants.AgentType.Direct;

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "Consignor";
			consignor.MiscServ.OM_FWAsAgentOption = OrgMiscServLookups.AsAgentOption.AsCarrier;
			consignor.MiscServ.OM_FWAsAgentName = "BBB Lines";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "CNSHA";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_Vessel = "Sea Dragon";
			transport.JW_VoyageFlight = "F9999";

			var builder = new ShippingInstructionBuilder(consol);
			var carrierMessageData = builder.Build();
			AssertEquals(true,
				carrierMessageData.Forwarder.CompanyNameInfo.HasMessageError(
					"Forwarder party name and address information is required for direct consolidations."));

			var forwarder = Factory.New<OrgHeader>();
			forwarder.OH_FullName = "Forwarder";
			forwarder.MiscServ.OM_FWAsAgentOption = OrgMiscServLookups.AsAgentOption.AsAgentForCarrier;
			forwarder.MiscServ.OM_FWAsAgentName = "AAA Lines";
			forwarder.OH_FullName = "I'm Sending Stuff";
			forwarder.OH_RL_NKClosestPort = "CNNJI";
			forwarder.MainAddress.Address1 = "Unit 200";
			forwarder.MainAddress.Address2 = "55 Why Lane";
			forwarder.MainAddress.City = "Conficious Ave";
			forwarder.MainAddress.Postcode = "10000";
			forwarder.MainAddress.OA_RN_NKCountryCode = "CN";

			consol.JK_OA_SendingForwarderAddress = forwarder.MainAddress.PK;

			builder = new ShippingInstructionBuilder(consol);
			carrierMessageData = builder.Build();
			AssertEquals(false,
				carrierMessageData.Forwarder.CompanyNameInfo.HasMessageError(
					"Forwarder party name and address information is required for direct consolidations."));
		}

		#endregion

		#region TestPopulateCarrierMessagingRequirements

		public void TestPopulateCarrierMessagingRequirements_BLP()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "MYABU";
			consol.JK_AgentType = Constants.AgentType.CoLoad;

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "OrgHeader";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.MainAddress.Address1 = "UNIT05";
			orgHeader.MainAddress.Address2 = "Haha Street";
			orgHeader.MainAddress.City = "AUCKLAND";
			orgHeader.MainAddress.Postcode = "1050";
			orgHeader.MainAddress.OA_RN_NKCountryCode = "NZ";
			orgHeader.MainAddress.OA_Email = "Flah@Floogle.com";

			consol.JK_OA_CreditorAddress = orgHeader.MainAddress.PK;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;

			var refShippingLine = Factory.New<RefShippingLine>();
			orgHeader.OH_RSL_ShippingLine = refShippingLine.PK;

			var eblProvider = refShippingLine.ShippingLineEBLProviders.AddNew();
			eblProvider.RSE_IsAvailable = true;
			eblProvider.RSE_Name = EBLProviderConstants.Codes.Bolero;

			AssertNull(refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.BillOfLadingProvider));
			Assert(!CreateDocDataObjectBuilder(consol).Build().ElectronicBillOfLadingProviderMandatory);

			var messagingRequirement = refShippingLine.ShippingLineMessagingRequirements.AddNew();
			messagingRequirement.RSR_RST_NKType = ShippingLineMessagingRequirement.Types.BillOfLadingProvider;
			messagingRequirement.RSR_IsShippingInstruction = true;
			messagingRequirement.RSR_IsBookingRequest = true;

			Assert(refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.BillOfLadingProvider).RSR_IsShippingInstruction);
			Assert(refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.BillOfLadingProvider).RSR_IsBookingRequest);
			Assert(CreateDocDataObjectBuilder(consol).Build().ElectronicBillOfLadingProviderMandatory);

			messagingRequirement.RSR_IsShippingInstruction = false;
			messagingRequirement.RSR_IsBookingRequest = false;

			Assert(!refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.BillOfLadingProvider).RSR_IsShippingInstruction);
			Assert(!refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.BillOfLadingProvider).RSR_IsBookingRequest);
			Assert(!CreateDocDataObjectBuilder(consol).Build().ElectronicBillOfLadingProviderMandatory);

			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = orgHeader.MainAddress.PK;

			refShippingLine.ShippingLineMessagingRequirements.DeleteAll();

			AssertNull(refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.BillOfLadingProvider));
			Assert(!CreateDocDataObjectBuilder(consol).Build().ElectronicBillOfLadingProviderMandatory);

			messagingRequirement = refShippingLine.ShippingLineMessagingRequirements.AddNew();
			messagingRequirement.RSR_RST_NKType = ShippingLineMessagingRequirement.Types.BillOfLadingProvider;
			messagingRequirement.RSR_IsShippingInstruction = true;
			messagingRequirement.RSR_IsBookingRequest = true;

			Assert(refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.BillOfLadingProvider).RSR_IsShippingInstruction);
			Assert(refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.BillOfLadingProvider).RSR_IsBookingRequest);
			Assert(CreateDocDataObjectBuilder(consol).Build().ElectronicBillOfLadingProviderMandatory);

			messagingRequirement.RSR_IsShippingInstruction = false;
			messagingRequirement.RSR_IsBookingRequest = false;

			Assert(!refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.BillOfLadingProvider).RSR_IsShippingInstruction);
			Assert(!refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.BillOfLadingProvider).RSR_IsBookingRequest);
			Assert(!CreateDocDataObjectBuilder(consol).Build().ElectronicBillOfLadingProviderMandatory);
		}

		public void TestPopulateCarrierMessagingRequirements_FOM()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "MYABU";
			consol.JK_AgentType = Constants.AgentType.Agent;

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "OrgHeader";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.MainAddress.Address1 = "UNIT05";
			orgHeader.MainAddress.Address2 = "Haha Street";
			orgHeader.MainAddress.City = "AUCKLAND";
			orgHeader.MainAddress.Postcode = "1050";
			orgHeader.MainAddress.OA_RN_NKCountryCode = "NZ";
			orgHeader.MainAddress.OA_Email = "Flah@Floogle.com";

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = orgHeader.MainAddress.PK;

			var refShippingLine = Factory.New<RefShippingLine>();
			orgHeader.OH_RSL_ShippingLine = refShippingLine.PK;

			AssertNull(refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage));
			Assert(!CreateDocDataObjectBuilder(consol).Build().IsRequiredSendAttachment);

			var messagingRequirement = refShippingLine.ShippingLineMessagingRequirements.AddNew();
			messagingRequirement.RSR_RST_NKType = ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage;
			messagingRequirement.RSR_IsShippingInstruction = true;
			messagingRequirement.RSR_IsBookingRequest = true;

			Assert(refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage).RSR_IsShippingInstruction);
			Assert(refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage).RSR_IsBookingRequest);
			Assert(CreateDocDataObjectBuilder(consol).Build().IsRequiredSendAttachment);

			messagingRequirement.RSR_IsShippingInstruction = false;
			messagingRequirement.RSR_IsBookingRequest = false;

			Assert(!refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage).RSR_IsShippingInstruction);
			Assert(!refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage).RSR_IsBookingRequest);
			Assert(!CreateDocDataObjectBuilder(consol).Build().IsRequiredSendAttachment);

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = orgHeader.MainAddress.PK;

			refShippingLine.ShippingLineMessagingRequirements.DeleteAll();

			AssertNull(refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage));
			Assert(!CreateDocDataObjectBuilder(consol).Build().IsRequiredSendAttachment);

			messagingRequirement = refShippingLine.ShippingLineMessagingRequirements.AddNew();
			messagingRequirement.RSR_RST_NKType = ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage;
			messagingRequirement.RSR_IsShippingInstruction = true;
			messagingRequirement.RSR_IsBookingRequest = true;

			Assert(refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage).RSR_IsShippingInstruction);
			Assert(refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage).RSR_IsBookingRequest);
			Assert(CreateDocDataObjectBuilder(consol).Build().IsRequiredSendAttachment);

			messagingRequirement.RSR_IsShippingInstruction = false;
			messagingRequirement.RSR_IsBookingRequest = false;

			Assert(!refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage).RSR_IsShippingInstruction);
			Assert(!refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage).RSR_IsBookingRequest);
			Assert(!CreateDocDataObjectBuilder(consol).Build().IsRequiredSendAttachment);
		}

		#endregion

		#region TestPickupFromValidation

		public void TestPickupFromValidation_WhenUncheckIsDoorPickup_ThenErrorMessageDisappear()
		{
			const string errorMessage = "Most messaging providers do not support non ASCII characters.";

			var data = CreateDocDataObjectBuilder(CreateConsol()).Build();

			data.IsDoorPickup = true;

			data.PickupFrom.CompanyName = "物";
			data.PickupFrom.Contact = "物";
			data.PickupFrom.AddressLine1 = "物";
			data.PickupFrom.AddressLine2 = "物";
			data.PickupFrom.City = "物";
			data.PickupFrom.State = "物";
			data.PickupFrom.Postcode = "物";

			CombineAssertions(() =>
			{
				AssertHasMessageError(data.PickupFrom.CompanyNameInfo, errorMessage);
				AssertHasMessageError(data.PickupFrom.ContactInfo, errorMessage);
				AssertHasMessageError(data.PickupFrom.AddressLine1Info, errorMessage);
				AssertHasMessageError(data.PickupFrom.AddressLine2Info, errorMessage);
				AssertHasMessageError(data.PickupFrom.CityInfo, errorMessage);
				AssertHasMessageError(data.PickupFrom.StateInfo, errorMessage);
				AssertHasMessageError(data.PickupFrom.PostcodeInfo, errorMessage);
			});

			data.IsDoorPickup = false;

			CombineAssertions(() =>
			{
				AssertNoMessageError(data.PickupFrom.CompanyNameInfo, errorMessage);
				AssertNoMessageError(data.PickupFrom.ContactInfo, errorMessage);
				AssertNoMessageError(data.PickupFrom.AddressLine1Info, errorMessage);
				AssertNoMessageError(data.PickupFrom.AddressLine2Info, errorMessage);
				AssertNoMessageError(data.PickupFrom.CityInfo, errorMessage);
				AssertNoMessageError(data.PickupFrom.StateInfo, errorMessage);
				AssertNoMessageError(data.PickupFrom.PostcodeInfo, errorMessage);
			});
		}

		public void TestDoorPickupConsolPickupFrom_WillFallBack_IsDoorPickupChange()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_AgentType = "DRT";

			var container = consol.Containers.AddNew();
			container.JC_DeliveryMode = Constants.DeliveryModes.Codes.CY_CY;

			var shipment = consol.Shipments.AddNew();

			var packDepotAddress = Factory.New<OrgHeader>();
			packDepotAddress.OH_FullName = "BLOOP";
			packDepotAddress.OH_RL_NKClosestPort = "USJFK";
			packDepotAddress.MainAddress.Address1 = "199 Crab Road";
			packDepotAddress.MainAddress.Address2 = "Crabby";
			packDepotAddress.MainAddress.City = "New York";
			packDepotAddress.MainAddress.Postcode = "10005";
			packDepotAddress.MainAddress.OA_RN_NKCountryCode = "US";

			consol.JK_OA_PackDepotAddress = packDepotAddress.MainAddress.PK;

			var consignorPickupAddress = Factory.New<OrgHeader>();
			consignorPickupAddress.OH_FullName = "CONSPA";
			consignorPickupAddress.OH_RL_NKClosestPort = "AUSYD";
			consignorPickupAddress.MainAddress.Address1 = "Unit 15";
			consignorPickupAddress.MainAddress.Address2 = "5 Lost Lane";
			consignorPickupAddress.MainAddress.City = "Sydney";
			consignorPickupAddress.MainAddress.Postcode = "2000";
			consignorPickupAddress.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsignorPickupAddress.E2_OA_Address = consignorPickupAddress.MainAddress.PK;

			var data = CreateDocDataObjectBuilder(consol).Build();
			CombineAssertions(() =>
			{
				AssertEquals("BLOOP", data.PickupFrom.CompanyName);
				AssertEquals("10005", data.PickupFrom.Postcode);
				AssertEquals("US", data.PickupFrom.Country.Code);
				AssertEquals("199 Crab Road", data.PickupFrom.AddressLine1);
				AssertEquals("Crabby", data.PickupFrom.AddressLine2);
				AssertEquals("New York", data.PickupFrom.City);
			});

			data.IsDoorPickup = true;
			CombineAssertions(() =>
			{
				AssertEquals("CONSPA", data.PickupFrom.CompanyName);
				AssertEquals("2000", data.PickupFrom.Postcode);
				AssertEquals("AU", data.PickupFrom.Country.Code);
				AssertEquals("Unit 15", data.PickupFrom.AddressLine1);
				AssertEquals("5 Lost Lane", data.PickupFrom.AddressLine2);
				AssertEquals("Sydney", data.PickupFrom.City);
			});

			var exportReceivingDepot = Factory.New<OrgHeader>();
			exportReceivingDepot.OH_FullName = "YUMMY";
			exportReceivingDepot.OH_RL_NKClosestPort = "FRPAR";
			exportReceivingDepot.MainAddress.Address1 = "Unit 200";
			exportReceivingDepot.MainAddress.Address2 = "55 Why Lane";
			exportReceivingDepot.MainAddress.City = "Paris";
			exportReceivingDepot.MainAddress.Postcode = "9000";
			exportReceivingDepot.MainAddress.OA_RN_NKCountryCode = "FR";

			shipment.JS_OA_ExportReceivingDepot = exportReceivingDepot.MainAddress.PK;

			data.IsDoorPickup = false;
			CombineAssertions(() =>
			{
				AssertEquals("BLOOP", data.PickupFrom.CompanyName);
				AssertEquals("10005", data.PickupFrom.Postcode);
				AssertEquals("US", data.PickupFrom.Country.Code);
				AssertEquals("199 Crab Road", data.PickupFrom.AddressLine1);
				AssertEquals("Crabby", data.PickupFrom.AddressLine2);
				AssertEquals("New York", data.PickupFrom.City);
			});

			data.IsDoorPickup = true;
			CombineAssertions(() =>
			{
				AssertEquals("YUMMY", data.PickupFrom.CompanyName);
				AssertEquals("9000", data.PickupFrom.Postcode);
				AssertEquals("FR", data.PickupFrom.Country.Code);
				AssertEquals("Unit 200", data.PickupFrom.AddressLine1);
				AssertEquals("55 Why Lane", data.PickupFrom.AddressLine2);
				AssertEquals("Paris", data.PickupFrom.City);
			});
		}

		public void TestPickupFromAddressValidation_IsDoorPickupChange()
		{
			var errorMessage = "Door Pickup is selected but the Pickup From address is incomplete.";

			var consol = CreateConsol();

			var exportReceivingDepot = Factory.New<OrgHeader>();
			exportReceivingDepot.OH_FullName = "YUMMY";
			exportReceivingDepot.OH_RL_NKClosestPort = "FRPAR";
			exportReceivingDepot.MainAddress.Address1 = "";
			exportReceivingDepot.MainAddress.Address2 = "55 Why Lane";
			exportReceivingDepot.MainAddress.City = "Paris";
			exportReceivingDepot.MainAddress.Postcode = "9000";
			exportReceivingDepot.MainAddress.OA_RN_NKCountryCode = "FR";

			var shipment = (ForwardingShipment)consol.Shipments.First();
			shipment.JS_OA_ExportReceivingDepot = exportReceivingDepot.MainAddress.PK;

			var data = CreateDocDataObjectBuilder(consol).Build();

			AssertEquals(true, data.PickupFrom.HasMessageErrors);
			AssertHasMessageError(data.PickupFrom.CompanyNameInfo, "Pickup from name and address are mandatory when 'Door Pickup' is selected.");
			AssertNoMessageError(data.ErrorPlaceHolderInfo, errorMessage);

			exportReceivingDepot.MainAddress.Address1 = "Address1";
			exportReceivingDepot.MainAddress.OA_Email = "123@123.com";
			consol.Containers[0].JC_DeliveryMode = Constants.DeliveryModes.Codes.CY_CY;
			data = CreateDocDataObjectBuilder(consol).Build();

			AssertEquals(false, data.PickupFrom.HasMessageErrors);
			AssertNoMessageError(data.ErrorPlaceHolderInfo, errorMessage);

			data.IsDoorPickup = true;

			AssertEquals(true, data.PickupFrom.HasMessageErrors);
			AssertHasMessageError(data.PickupFrom.ContactInfo, "Please enter both contact name and at least one communication: phone, email or fax.");
			AssertHasMessageError(data.ErrorPlaceHolderInfo, errorMessage);

			data.IsDoorPickup = false;

			AssertEquals(false, data.PickupFrom.HasMessageErrors);
			AssertNoMessageError(data.ErrorPlaceHolderInfo, errorMessage);

			data.IsDoorPickup = true;
			data.PickupFrom.Contact = "Contact";

			AssertEquals(false, data.PickupFrom.HasMessageErrors);
			AssertNoMessageError(data.ErrorPlaceHolderInfo, errorMessage);
		}

		#endregion

		#region TestDeliverToValidation

		public void TestDeliverToValidation_WhenUncheckIsDoorDelivery_ThenErrorMessageDisappear()
		{
			const string errorMessage = "Most messaging providers do not support non ASCII characters.";

			var data = CreateDocDataObjectBuilder(CreateConsol()).Build();

			data.IsDoorDelivery = true;

			data.DeliverTo.CompanyName = "物";
			data.DeliverTo.Contact = "物";
			data.DeliverTo.AddressLine1 = "物";
			data.DeliverTo.AddressLine2 = "物";
			data.DeliverTo.City = "物";
			data.DeliverTo.State = "物";
			data.DeliverTo.Postcode = "物";

			CombineAssertions(() =>
			{
				AssertHasMessageError(data.DeliverTo.CompanyNameInfo, errorMessage);
				AssertHasMessageError(data.DeliverTo.ContactInfo, errorMessage);
				AssertHasMessageError(data.DeliverTo.AddressLine1Info, errorMessage);
				AssertHasMessageError(data.DeliverTo.AddressLine2Info, errorMessage);
				AssertHasMessageError(data.DeliverTo.CityInfo, errorMessage);
				AssertHasMessageError(data.DeliverTo.StateInfo, errorMessage);
				AssertHasMessageError(data.DeliverTo.PostcodeInfo, errorMessage);
			});

			data.IsDoorDelivery = false;

			CombineAssertions(() =>
			{
				AssertNoMessageError(data.DeliverTo.CompanyNameInfo, errorMessage);
				AssertNoMessageError(data.DeliverTo.ContactInfo, errorMessage);
				AssertNoMessageError(data.DeliverTo.AddressLine1Info, errorMessage);
				AssertNoMessageError(data.DeliverTo.AddressLine2Info, errorMessage);
				AssertNoMessageError(data.DeliverTo.CityInfo, errorMessage);
				AssertNoMessageError(data.DeliverTo.StateInfo, errorMessage);
				AssertNoMessageError(data.DeliverTo.PostcodeInfo, errorMessage);
			});
		}

		public void TestDoorDeliveryConsolDeliveryTo_WillFallBack_IsDoorDeliveryChange()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_AgentType = "DRT";

			var container = consol.Containers.AddNew();
			container.JC_DeliveryMode = Constants.DeliveryModes.Codes.CY_CY;

			var shipment = consol.Shipments.AddNew();

			var unPackDepotAddress = Factory.New<OrgHeader>();
			unPackDepotAddress.OH_FullName = "BLOOP";
			unPackDepotAddress.OH_RL_NKClosestPort = "USJFK";
			unPackDepotAddress.MainAddress.Address1 = "199 Crab Road";
			unPackDepotAddress.MainAddress.Address2 = "Crabby";
			unPackDepotAddress.MainAddress.City = "New York";
			unPackDepotAddress.MainAddress.Postcode = "10005";
			unPackDepotAddress.MainAddress.OA_RN_NKCountryCode = "US";

			consol.JK_OA_UnpackDepotAddress = unPackDepotAddress.MainAddress.PK;

			var consigneeDeliveryAddress = Factory.New<OrgHeader>();
			consigneeDeliveryAddress.OH_FullName = "CONSPA";
			consigneeDeliveryAddress.OH_RL_NKClosestPort = "AUSYD";
			consigneeDeliveryAddress.MainAddress.Address1 = "Unit 15";
			consigneeDeliveryAddress.MainAddress.Address2 = "5 Lost Lane";
			consigneeDeliveryAddress.MainAddress.City = "Sydney";
			consigneeDeliveryAddress.MainAddress.Postcode = "2000";
			consigneeDeliveryAddress.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsigneeDeliveryAddress.E2_OA_Address = consigneeDeliveryAddress.MainAddress.PK;

			var data = CreateDocDataObjectBuilder(consol).Build();
			CombineAssertions(() =>
			{
				AssertEquals("BLOOP", data.DeliverTo.CompanyName);
				AssertEquals("10005", data.DeliverTo.Postcode);
				AssertEquals("US", data.DeliverTo.Country.Code);
				AssertEquals("199 Crab Road", data.DeliverTo.AddressLine1);
				AssertEquals("Crabby", data.DeliverTo.AddressLine2);
				AssertEquals("New York", data.DeliverTo.City);
			});

			data.IsDoorDelivery = true;
			CombineAssertions(() =>
			{
				AssertEquals("CONSPA", data.DeliverTo.CompanyName);
				AssertEquals("2000", data.DeliverTo.Postcode);
				AssertEquals("AU", data.DeliverTo.Country.Code);
				AssertEquals("Unit 15", data.DeliverTo.AddressLine1);
				AssertEquals("5 Lost Lane", data.DeliverTo.AddressLine2);
				AssertEquals("Sydney", data.DeliverTo.City);
			});

			var importReleaseDepot = Factory.New<OrgHeader>();
			importReleaseDepot.OH_FullName = "YUMMY";
			importReleaseDepot.OH_RL_NKClosestPort = "FRPAR";
			importReleaseDepot.MainAddress.Address1 = "Unit 200";
			importReleaseDepot.MainAddress.Address2 = "55 Why Lane";
			importReleaseDepot.MainAddress.City = "Paris";
			importReleaseDepot.MainAddress.Postcode = "9000";
			importReleaseDepot.MainAddress.OA_RN_NKCountryCode = "FR";

			shipment.JS_OA_ImportReleaseDepot = importReleaseDepot.MainAddress.PK;

			data.IsDoorDelivery = false;
			CombineAssertions(() =>
			{
				AssertEquals("BLOOP", data.DeliverTo.CompanyName);
				AssertEquals("10005", data.DeliverTo.Postcode);
				AssertEquals("US", data.DeliverTo.Country.Code);
				AssertEquals("199 Crab Road", data.DeliverTo.AddressLine1);
				AssertEquals("Crabby", data.DeliverTo.AddressLine2);
				AssertEquals("New York", data.DeliverTo.City);
			});

			data.IsDoorDelivery = true;
			CombineAssertions(() =>
			{
				AssertEquals("YUMMY", data.DeliverTo.CompanyName);
				AssertEquals("9000", data.DeliverTo.Postcode);
				AssertEquals("FR", data.DeliverTo.Country.Code);
				AssertEquals("Unit 200", data.DeliverTo.AddressLine1);
				AssertEquals("55 Why Lane", data.DeliverTo.AddressLine2);
				AssertEquals("Paris", data.DeliverTo.City);
			});
		}

		public void TestDeliverToAddressValidation_IsDoorDeliveryChange()
		{
			const string errorMessage = "Door Delivery is selected but the Deliver To address is incomplete.";

			var consol = CreateConsol();

			var importReleaseDepot = Factory.New<OrgHeader>();
			importReleaseDepot.OH_FullName = "YUMMY";
			importReleaseDepot.OH_RL_NKClosestPort = "FRPAR";
			importReleaseDepot.MainAddress.Address1 = "Address1";
			importReleaseDepot.MainAddress.Address2 = "55 Why Lane";
			importReleaseDepot.MainAddress.City = "Paris";
			importReleaseDepot.MainAddress.Postcode = "9000";
			importReleaseDepot.MainAddress.OA_RN_NKCountryCode = "FR";
			importReleaseDepot.MainAddress.CompanyName = "中国";

			consol.Containers[0].JC_DeliveryMode = Constants.DeliveryModes.Codes.CFS_CFS;
			consol.Shipments[0].JS_OA_ImportReleaseDepot = importReleaseDepot.MainAddress.PK;

			var data = CreateDocDataObjectBuilder(consol).Build();

			AssertEquals(true, data.DeliverTo.HasMessageErrors);
			AssertHasMessageError(data.DeliverTo.CompanyNameInfo, "Most messaging providers do not support non ASCII characters.");
			AssertNoMessageError(data.ErrorPlaceHolderInfo, errorMessage);

			importReleaseDepot.MainAddress.Address1 = "";
			importReleaseDepot.MainAddress.CompanyName = "";

			consol.Containers[0].JC_DeliveryMode = Constants.DeliveryModes.Codes.CY_CY;
			data = CreateDocDataObjectBuilder(consol).Build();

			AssertEquals(false, data.DeliverTo.HasMessageErrors);
			AssertNoMessageError(data.ErrorPlaceHolderInfo, errorMessage);

			data.IsDoorDelivery = true;

			AssertEquals(true, data.DeliverTo.HasMessageErrors);
			AssertHasMessageError(data.DeliverTo.CompanyNameInfo, "Deliver to name and address are mandatory when 'Door Delivery' is selected.");
			AssertHasMessageError(data.ErrorPlaceHolderInfo, errorMessage);

			data.IsDoorDelivery = false;
			AssertEquals(false, data.DeliverTo.HasMessageErrors);
			AssertNoMessageError(data.ErrorPlaceHolderInfo, errorMessage);

			data.IsDoorDelivery = true;
			data.DeliverTo.AddressLine1 = "Address1";

			AssertEquals(false, data.DeliverTo.HasMessageErrors);
			AssertNoMessageError(data.ErrorPlaceHolderInfo, errorMessage);
		}

		#endregion

		public void TestShipmentsIncludeCoLoadChildren()
		{
			var consol = CreateConsol();
			var builder = CreateDocDataObjectBuilder(consol);
			var carrierMessageData = builder.Build();

			AssertNotNull(carrierMessageData);
			AssertEquals(1, carrierMessageData.Shipments.Count);

			var shipment = consol.Shipments[0];
			shipment.CoLoadShipments.AddNew();
			shipment.CoLoadShipments.AddNew();
			builder = CreateDocDataObjectBuilder(consol);
			carrierMessageData = builder.Build();

			AssertNotNull(carrierMessageData);
			AssertEquals(3, carrierMessageData.Shipments.Count + carrierMessageData.Shipments.Sum(x => x.Shipments.Count));
		}

		public void TestMarksAndNumbersWithSpecialCharacter()
		{
			var asciiError = "Most messaging providers do not support non ASCII characters.";
			var consol = CreateConsol();
			consol.Shipments[0].OuterPackLines.AddNew();
			consol.Shipments[0].OuterPackLines[0].JL_MarksAndNumbers = "1447 IN²";
			consol.Shipments[0].OuterPackLines[1].JL_MarksAndNumbers = "1447 IN³";
			consol.Shipments[0].OuterPackLines[2].JL_MarksAndNumbers = "1447 IN";
			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			var packingLines = carrierMessageData.Shipments.First().PackingLines.ToArray();

			AssertHasMessageError(packingLines[0].MarksAndNumbersInfo, asciiError);
			AssertHasMessageError(packingLines[1].MarksAndNumbersInfo, asciiError);
			AssertNoMessageError(packingLines[2].MarksAndNumbersInfo, asciiError);
		}

		public void TestShipperReference()
		{
			var consol = CreateConsol();
			consol.JK_AgentType = Constants.AgentType.Direct;

			consol.Shipments[0].JS_BookingReference = "BookingReference";
			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertNotNull(carrierMessageData);
			AssertEquals("BookingReference", carrierMessageData.ShipperReference);

			consol.Shipments[0].JS_BookingReference = ZString.Empty;
			consol.Shipments[0].JS_UniqueConsignRef = "UniqueConsignRef";
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertNotNull(carrierMessageData);
			AssertEquals("UniqueConsignRef", carrierMessageData.ShipperReference);
		}

		public void TestCarrierStandardAddressValidation()
		{
			var emailError = "Please enter a valid email. Email must contain at least 6 characters, at least one dot '.' after '@' with at least one character in between and at least 2 characters after the dot. Email can only contain alphanumeric characters and '_', '-', '@', '.'.";

			var consol = CreateConsol();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_AgentType = Constants.AgentType.Agent;

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			carrierMessageData.Carrier.Email = "xxxx";
			carrierMessageData.Carrier.ValidateAll();
			AssertHasMessageError(carrierMessageData.Carrier.EmailInfo, emailError);

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			carrierMessageData.Carrier.Email = "xxxx";
			carrierMessageData.Carrier.ValidateAll();
			AssertHasMessageError(carrierMessageData.Carrier.EmailInfo, emailError);

			consol.JK_AgentType = Constants.AgentType.Agent;
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_IsNVO = true;
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_RSL_ShippingLine = shippingLine.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			carrierMessageData.Carrier.Email = "xxxx";
			carrierMessageData.Carrier.ValidateAll();
			AssertHasMessageError(carrierMessageData.Carrier.EmailInfo, emailError);
		}

		#region TestCarrierMessagingRequirementsValidation

		public void TestCarrierMessagingRequirementsValidation_HSC()
		{
			var errorMessage = "The carrier requires HS code for each pack line.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "MYABU";
			consol.JK_AgentType = Constants.AgentType.CoLoad;

			var creditor = Factory.New<OrgHeader>();
			creditor.OH_FullName = "Creditor";
			creditor.OH_RL_NKClosestPort = "AUSYD";
			creditor.MainAddress.Address1 = "UNIT05";
			creditor.MainAddress.Address2 = "Haha Street";
			creditor.MainAddress.City = "AUCKLAND";
			creditor.MainAddress.Postcode = "1050";
			creditor.MainAddress.OA_RN_NKCountryCode = "NZ";
			creditor.MainAddress.OA_Email = "Flah@Floogle.com";
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var creditorRefShippingLine = Factory.New<RefShippingLine>();
			creditor.OH_RSL_ShippingLine = creditorRefShippingLine.PK;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_DeliveryMode = "CFS/CY";

			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "CNSHA";
			transport.JW_RL_NKDiscPort = "MYABU";
			transport.JW_Vessel = "ANRO ASIA";
			transport.JW_VoyageFlight = "324443";
			transport.JW_ETD = new ZDateTime(2019, 12, 1);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "MYABU";
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_HarmonisedCode = string.Empty;
			packLine1.JL_PackageCount = 2;
			packLine1.JL_F3_NKPackType = "PLT";
			packLine1.JL_JC = container.PK;
			packLine1.JL_ItemNo = 1;

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_HarmonisedCode = "HSC_CODE";
			packLine2.JL_PackageCount = 2;
			packLine2.JL_F3_NKPackType = "PLT";
			packLine2.JL_JC = container.PK;
			packLine2.JL_ItemNo = 2;

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			var harmonizedCode1 = (HarmonizedCode)carrierMessageData.Containers.First().PackingLines.First().HarmonizedCode;
			var harmonizedCode2 = (HarmonizedCode)carrierMessageData.Containers.First().PackingLines.ElementAt(1).HarmonizedCode;

			AssertNull(creditorRefShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.HarmonisedCode));
			AssertNoMessageError(harmonizedCode1.CodeInfo, errorMessage);
			AssertNoMessageError(harmonizedCode2.CodeInfo, errorMessage);

			var harmonisedCodeMessagingRequirment = creditorRefShippingLine.ShippingLineMessagingRequirements.AddNew();
			harmonisedCodeMessagingRequirment.RSR_RST_NKType = ShippingLineMessagingRequirement.Types.HarmonisedCode;
			harmonisedCodeMessagingRequirment.RSR_IsBookingRequest = true;
			harmonisedCodeMessagingRequirment.RSR_IsShippingInstruction = true;

			Assert(creditorRefShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.HarmonisedCode).RSR_IsBookingRequest);
			Assert(creditorRefShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.HarmonisedCode).RSR_IsShippingInstruction);

			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			harmonizedCode1 = (HarmonizedCode)carrierMessageData.Containers.First().PackingLines.First().HarmonizedCode;
			harmonizedCode2 = (HarmonizedCode)carrierMessageData.Containers.First().PackingLines.ElementAt(1).HarmonizedCode;

			AssertHasMessageError(harmonizedCode1.CodeInfo, errorMessage);
			AssertNoMessageError(harmonizedCode2.CodeInfo, errorMessage);

			AssertEquals(2, carrierMessageData.Containers.First().PackingLines.Count);
			AssertEquals(2, carrierMessageData.Shipments.SelectMany(x => x.PackingLines).Count());

			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_HarmonisedCode = ZString.Empty;
			packLine3.JL_PackageCount = 2;
			packLine3.JL_F3_NKPackType = "PLT";
			packLine3.JL_JC = ZGuid.Empty;
			packLine3.JL_ItemNo = 3;

			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			var harmonizedCode3 = (HarmonizedCode)carrierMessageData.Shipments.SelectMany(x => x.PackingLines).First(x => x.ItemNumber == 3).HarmonizedCode;

			AssertEquals(2, carrierMessageData.Containers.First().PackingLines.Count);
			AssertEquals(3, carrierMessageData.Shipments.SelectMany(x => x.PackingLines).Count());

			AssertHasMessageError(harmonizedCode3.CodeInfo, errorMessage);

			var harmonisedCode1 = packLine1.HarmonisedCodes.AddNew();
			harmonisedCode1.JLH_Code = "HS1";
			harmonisedCode1.JLH_RN_NKCountry = "CN";

			var harmonisedCode2 = packLine3.HarmonisedCodes.AddNew();
			harmonisedCode2.JLH_Code = "HS2";
			harmonisedCode2.JLH_RN_NKCountry = "MY";

			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			harmonizedCode1 = (HarmonizedCode)carrierMessageData.Containers.First().PackingLines.First().HarmonizedCode;
			harmonizedCode3 = (HarmonizedCode)carrierMessageData.Shipments.SelectMany(x => x.PackingLines).First(x => x.ItemNumber == 3).HarmonizedCode;

			AssertNoMessageError(harmonizedCode1.CodeInfo, errorMessage);
			AssertNoMessageError(harmonizedCode3.CodeInfo, errorMessage);
		}

		#endregion

		#region TestCarrierLinkShippingLineValidation

		public void TestCarrierLinkShippingLineValidation()
		{
			var errorMessage = "This Carrier Organisation is not linked to a Shipping Line record. Please link this Carrier to Shipping Line record on Organization > Carrier > Shipping Line/NVOCC/Agent > Shipping Line.";
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "MYABU";
			consol.JK_AgentType = Constants.AgentType.Agent;

			var carrier = Factory.New<OrgHeader>();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			carrierMessageData.Carrier.ValidateAll();
			AssertHasMessageError(carrierMessageData.Carrier.CompanyNameInfo, errorMessage);

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertNoMessageError(carrierMessageData.Carrier.CompanyNameInfo, errorMessage);
		}

		public void TestCarrierLinkShippingLineValidation_IsCoLoad()
		{
			var errorMessage = "This Co-Load With Organisation is not linked to a Shipping Line record. Please link this Co-Loader to Shipping Line record on Organization > Carrier > Shipping Line/NVOCC/Agent > Shipping Line.";
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "MYABU";
			consol.JK_AgentType = Constants.AgentType.CoLoad;

			var creditor = Factory.New<OrgHeader>();
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			carrierMessageData.Carrier.ValidateAll();
			AssertHasMessageError(carrierMessageData.Recipient.CompanyNameInfo, errorMessage);

			var creditorRefShippingLine = Factory.New<RefShippingLine>();
			creditor.OH_RSL_ShippingLine = creditorRefShippingLine.PK;

			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertNoMessageError(carrierMessageData.Recipient.CompanyNameInfo, errorMessage);
		}

		#endregion

		#region TestDefaultContactAndEmail

		public void TestDefaultContactAndEmail()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "MYABU";
			consol.JK_AgentType = Constants.AgentType.Agent;

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			SetShippingLineOption(shippingLine, false);

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_RSL_ShippingLine = shippingLine.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var contact = carrier.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			contact.OC_ContactName = "TEST NAME";

			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = "ALL";

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("test@test.com", carrierMessageData.Recipient.Email);
			AssertEquals("TEST NAME", carrierMessageData.Recipient.Contact);

			SetShippingLineOption(shippingLine, true);
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("", carrierMessageData.Recipient.Email);
			AssertEquals("", carrierMessageData.Recipient.Contact);
		}

		public void TestDefaultContactAndEmail_IsCoload()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "MYABU";
			consol.JK_AgentType = Constants.AgentType.CoLoad;

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			SetShippingLineOption(shippingLine, false);

			var creditor = Factory.New<OrgHeader>();
			creditor.OH_RSL_ShippingLine = shippingLine.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var contact = creditor.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			contact.OC_ContactName = "TEST NAME";

			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = "SHP";

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("test@test.com", carrierMessageData.Recipient.Email);
			AssertEquals("TEST NAME", carrierMessageData.Recipient.Contact);

			SetShippingLineOption(shippingLine, true);
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("", carrierMessageData.Recipient.Email);
			AssertEquals("", carrierMessageData.Recipient.Contact);
		}

		#endregion

		#region TestDefaultContactAndEmailValidation

		public void TestDefaultContactAndEmailValidation_NoShippingLineRelevantMessageOptionEnabled()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "MYABU";
			consol.JK_AgentType = Constants.AgentType.Agent;

			var documentName = GetCarrierMessageDataBuilderName() == DataContext.BookingRequest ? "Booking Request" : "Shipping Instruction";

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			SetShippingLineOption(shippingLine, false);

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_RSL_ShippingLine = shippingLine.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var contact = carrier.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			contact.OC_ContactName = "TEST NAME";

			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = string.Empty;

			var errorMessage = $"This carrier does not support electronic {documentName}. Contact name and email address are required to send your {documentName} by email.\nPlease setup contact name and email address on Organization> Contact> Email and Receiving Documents> Group SHP";

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("", carrierMessageData.Recipient.Email);
			AssertEquals("", carrierMessageData.Recipient.Contact);
			AssertHasMessageError(carrierMessageData.Recipient.ContactInfo, errorMessage);

			carrierMessageData.Recipient.Contact = string.Empty;
			carrierMessageData.Recipient.Email = "email";
			AssertHasMessageError(carrierMessageData.Recipient.ContactInfo, errorMessage);

			carrierMessageData.Recipient.Contact = "ABC";
			carrierMessageData.Recipient.Email = string.Empty;
			AssertHasMessageError(carrierMessageData.Recipient.ContactInfo, errorMessage);

			carrierMessageData.Recipient.Email = "email";
			AssertNoMessageError(carrierMessageData.Recipient.ContactInfo, errorMessage);

			SetShippingLineOption(shippingLine, true);
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertNoMessageError(carrierMessageData.Recipient.ContactInfo, errorMessage);
		}

		public void TestDefaultContactAndEmailValidation_ShippingLineRelevantMessageOptionEnabled()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "MYABU";
			consol.JK_AgentType = Constants.AgentType.Agent;

			var documentName = GetCarrierMessageDataBuilderName() == DataContext.BookingRequest ? "Booking Request" : "Shipping Instruction";

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			SetShippingLineOption(shippingLine, true);

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_RSL_ShippingLine = shippingLine.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var contact = carrier.Contacts.AddNew();
			contact.OC_Fax = string.Empty;
			contact.OC_HomePhone = string.Empty;
			contact.OC_Email = string.Empty;
			contact.OC_ContactName = string.Empty;

			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = string.Empty;

			var errorMessage = "Please enter both contact name and at least one communication: phone, email or fax.";

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertNoMessageError(carrierMessageData.Recipient.ContactInfo, errorMessage);

			carrierMessageData.Recipient.Contact = "Contact";
			AssertHasMessageError(carrierMessageData.Recipient.ContactInfo, errorMessage);
		}

		public void TestDefaultContactAndEmailValidation_IsCoload()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "MYABU";
			consol.JK_AgentType = Constants.AgentType.CoLoad;

			var documentName = GetCarrierMessageDataBuilderName() == DataContext.BookingRequest ? "Booking Request" : "Shipping Instruction";

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			SetShippingLineOption(shippingLine, false);

			var creditor = Factory.New<OrgHeader>();
			creditor.OH_RSL_ShippingLine = shippingLine.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var contact = creditor.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			contact.OC_ContactName = "TEST NAME";

			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = "";

			var errorMessage = $"This NVOCC does not support electronic {documentName}. Contact name and email address are required to send your {documentName} by email.\nPlease setup contact name and email address on Organization> Contact> Email and Receiving Documents> Group SHP";

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("", carrierMessageData.Recipient.Email);
			AssertEquals("", carrierMessageData.Recipient.Contact);
			AssertHasMessageError(carrierMessageData.Recipient.ContactInfo, errorMessage);

			carrierMessageData.Recipient.Contact = string.Empty;
			carrierMessageData.Recipient.Email = "email";
			AssertHasMessageError(carrierMessageData.Recipient.ContactInfo, errorMessage);

			carrierMessageData.Recipient.Contact = "ABC";
			carrierMessageData.Recipient.Email = string.Empty;
			AssertHasMessageError(carrierMessageData.Recipient.ContactInfo, errorMessage);

			carrierMessageData.Recipient.Email = "email";
			AssertNoMessageError(carrierMessageData.Recipient.ContactInfo, errorMessage);
		}

		#endregion

		#region TestContextWithCarrierUnlocoMapping

		public void TestContextWithCarrierUnlocoMapping_Agent()
		{
			var consol = CreateConsol();
			var mapping1 = consol.ShippingLine.CreatePatternMatchOverrideForTest();

			mapping1.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			mapping1.OO_Context = Core.Constants.OrgPatternMatchOverrideContexts.Codes.OceanCarrierMessage;
			mapping1.OO_ForeignCode = "CNXXX";
			mapping1.OO_LocalCode = "CNSHA";

			var mapping2 = consol.Creditor.CreatePatternMatchOverrideForTest();

			mapping2.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			mapping2.OO_Context = Core.Constants.OrgPatternMatchOverrideContexts.Codes.OceanCarrierMessage;
			mapping2.OO_ForeignCode = "CNYYY";
			mapping2.OO_LocalCode = "CNSHA";

			Factory.Save();

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertType<RefUNLOCOCollectionWithCarrierMapping>(carrierMessageData.Origin.Unlocos);
			AssertEquals("Mapping should be from the carrier's foreign code", "CNXXX", carrierMessageData.Origin.Code);
		}

		public void TestContextWithCarrierUnlocoMapping_Coload()
		{
			var consol = CreateConsol();
			consol.JK_AgentType = Constants.AgentType.CoLoad;

			var creditor = Factory.New<OrgHeader>();
			creditor.OH_FullName = "Creditor";
			creditor.OH_RL_NKClosestPort = "AUSYD";
			creditor.MainAddress.Address1 = "UNIT05";
			creditor.MainAddress.Address2 = "Haha Street";
			creditor.MainAddress.City = "AUCKLAND";
			creditor.MainAddress.Postcode = "1050";
			creditor.MainAddress.OA_RN_NKCountryCode = "NZ";
			creditor.MainAddress.OA_Email = "Flah@Floogle.com";
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var mapping1 = consol.ShippingLine.CreatePatternMatchOverrideForTest();

			mapping1.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			mapping1.OO_Context = Core.Constants.OrgPatternMatchOverrideContexts.Codes.OceanCarrierMessage;
			mapping1.OO_ForeignCode = "CNXXX";
			mapping1.OO_LocalCode = "CNSHA";

			var mapping2 = consol.Creditor.CreatePatternMatchOverrideForTest();

			mapping2.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			mapping2.OO_Context = Core.Constants.OrgPatternMatchOverrideContexts.Codes.OceanCarrierMessage;
			mapping2.OO_ForeignCode = "CNYYY";
			mapping2.OO_LocalCode = "CNSHA";

			Factory.Save();

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertType<RefUNLOCOCollectionWithCarrierMapping>(carrierMessageData.Origin.Unlocos);
			AssertEquals("Mapping should be from the co-load with's foreign code", "CNYYY", carrierMessageData.Origin.Code);
		}

		#endregion

		#region TestPreAllocatedUNDGCollection

		public void TestPreAllocatedUNDGCollection()
		{
			var consol = CreateConsol();
			var builder = CreateDocDataObjectBuilder(consol);
			var carrierMessageData = builder.Build();

			AssertEquals("PreallocatedUNDGCollection", 2, carrierMessageData.PreallocatedUNDGCollection.Count);

			var dgRestriction = carrierMessageData.PreallocatedUNDGCollection.ToList()[0];
			CombineAssertions(() =>
			{
				AssertEquals(nameof(dgRestriction.EmergencyScheduleFire), "F-B", dgRestriction.EmergencyScheduleFire.Code);
				AssertEquals(nameof(dgRestriction.EmergencyScheduleSpillage), "S-Y", dgRestriction.EmergencyScheduleSpillage.Code);
				AssertEquals(nameof(dgRestriction.Standard), UNDGSubstanceStandardTypes.IMO, dgRestriction.Standard);
				AssertEquals(nameof(dgRestriction.ExceptedQuantityCode), string.Empty, dgRestriction.ExceptedQuantityCode);
				AssertEquals(nameof(dgRestriction.FlashPoint), string.Empty, dgRestriction.FlashPoint);
				AssertEquals(nameof(dgRestriction.IMOClass), "1.1D", dgRestriction.IMOClass);
				AssertEquals(nameof(dgRestriction.MarinePollutantCode), string.Empty, dgRestriction.MarinePollutantCode);
				AssertEquals(nameof(dgRestriction.PackedInLimitedQuantity), false, dgRestriction.PackedInLimitedQuantity);
				AssertEquals(nameof(dgRestriction.PackingGroup), string.Empty, dgRestriction.PackingGroup);
				AssertEquals(nameof(dgRestriction.ProperShippingName), "AMMONIUM PICRATE", dgRestriction.ProperShippingName);
				AssertEquals(nameof(dgRestriction.State), "E", dgRestriction.State);
				AssertEquals(nameof(dgRestriction.SubLabel1), string.Empty, dgRestriction.SubLabel1);
				AssertEquals(nameof(dgRestriction.SubLabel2), string.Empty, dgRestriction.SubLabel2);
				AssertEquals(nameof(dgRestriction.Code), "0004a", dgRestriction.Code);
				AssertEquals(nameof(dgRestriction.Unno), "0004", dgRestriction.Unno);
				AssertEquals(nameof(dgRestriction.Variant), "a", dgRestriction.Variant);
			});

			dgRestriction = carrierMessageData.PreallocatedUNDGCollection.ToList()[1];
			CombineAssertions(() =>
			{
				AssertEquals(nameof(dgRestriction.EmergencyScheduleFire), "F-E", dgRestriction.EmergencyScheduleFire.Code);
				AssertEquals(nameof(dgRestriction.EmergencyScheduleSpillage), "S-C", dgRestriction.EmergencyScheduleSpillage.Code);
				AssertEquals(nameof(dgRestriction.Standard), UNDGSubstanceStandardTypes.IMO, dgRestriction.Standard);
				AssertEquals(nameof(dgRestriction.ExceptedQuantityCode), "E0", dgRestriction.ExceptedQuantityCode);
				AssertEquals(nameof(dgRestriction.FlashPoint), "25 cc", dgRestriction.FlashPoint);
				AssertEquals(nameof(dgRestriction.IMOClass), "6.1", dgRestriction.IMOClass);
				AssertEquals(nameof(dgRestriction.MarinePollutantCode), "Y", dgRestriction.MarinePollutantCode);
				AssertEquals(nameof(dgRestriction.PackedInLimitedQuantity), false, dgRestriction.PackedInLimitedQuantity);
				AssertEquals(nameof(dgRestriction.PackingGroup), "I", dgRestriction.PackingGroup);
				AssertEquals(nameof(dgRestriction.ProperShippingName), "CHLOROACETONE, STABILIZED", dgRestriction.ProperShippingName);
				AssertEquals(nameof(dgRestriction.State), "L", dgRestriction.State);
				AssertEquals(nameof(dgRestriction.SubLabel1), "3", dgRestriction.SubLabel1);
				AssertEquals(nameof(dgRestriction.SubLabel2), "8", dgRestriction.SubLabel2);
				AssertEquals(nameof(dgRestriction.Code), "1695", dgRestriction.Code);
				AssertEquals(nameof(dgRestriction.Unno), "1695", dgRestriction.Unno);
				AssertEquals(nameof(dgRestriction.Variant), string.Empty, dgRestriction.Variant);
			});
		}

		#endregion

		#region TestPopulatePackingLines

		public void TestPopulatePackingLines_DisablePackageGrouping()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.DoNotGroup;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONTAINER1";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONTAINER2";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "SHIPMENT1";
			shipment1.JS_UnitOfWeight = Constants.Weight.Grams;
			shipment1.JS_UnitOfVolume = Constants.Volume.MegaLitre;
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "SHIPMENT2";
			shipment2.JS_UnitOfWeight = Constants.Weight.Grams;
			shipment2.JS_UnitOfVolume = Constants.Volume.MegaLitre;

			shipment1.InnerPackLines.RemoveAndDeleteAll();
			shipment1.OuterPackLines.RemoveAndDeleteAll();
			shipment2.InnerPackLines.RemoveAndDeleteAll();
			shipment2.OuterPackLines.RemoveAndDeleteAll();

			var s1OuterPackLine1 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 10, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres);
			s1OuterPackLine1.JL_JC = container1.PK;
			var s1OuterPackLine2 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 20, Constants.PkgUnit.Keg, 20, Constants.Weight.Grams, 20, Constants.Volume.MegaLitre);
			s1OuterPackLine2.JL_JC = container2.PK;

			var s1InnerPackLine1 = PopulatePackLine(shipment1.InnerPackLines.AddNew(), 30, Constants.PkgUnit.Keg, 30, Constants.Weight.Kilograms, 30, Constants.Volume.CubicMetres);
			s1InnerPackLine1.JL_JL_OuterPackLine = s1OuterPackLine1.PK;
			var s1InnerPackLine2 = PopulatePackLine(shipment1.InnerPackLines.AddNew(), 30, Constants.PkgUnit.Keg, 30, Constants.Weight.Kilograms, 30, Constants.Volume.CubicMetres);
			s1InnerPackLine2.JL_JL_OuterPackLine = s1OuterPackLine1.PK;
			var s1InnerPackLine3 = PopulatePackLine(shipment1.InnerPackLines.AddNew(), 40, Constants.PkgUnit.Keg, 40, Constants.Weight.Grams, 40, Constants.Volume.MegaLitre);
			s1InnerPackLine3.JL_JL_OuterPackLine = s1OuterPackLine2.PK;
			var s1InnerPackLine4 = PopulatePackLine(shipment1.InnerPackLines.AddNew(), 40, Constants.PkgUnit.Keg, 40, Constants.Weight.Grams, 40, Constants.Volume.MegaLitre);
			s1InnerPackLine4.JL_JL_OuterPackLine = s1OuterPackLine2.PK;
			PopulatePackLine(shipment1.InnerPackLines.AddNew(), 50, Constants.PkgUnit.Keg, 50, Constants.Weight.Grams, 50, Constants.Volume.MegaLitre);
			PopulatePackLine(shipment1.InnerPackLines.AddNew(), 60, Constants.PkgUnit.Keg, 60, Constants.Weight.Grams, 60, Constants.Volume.MegaLitre);
			PopulatePackLine(shipment1.OuterPackLines.AddNew(), 100, Constants.PkgUnit.Keg, 100, Constants.Weight.Kilograms, 100, Constants.Volume.CubicMetres).JL_JC = ZGuid.Empty;
			PopulatePackLine(shipment1.OuterPackLines.AddNew(), 110, Constants.PkgUnit.Keg, 110, Constants.Weight.Kilograms, 110, Constants.Volume.CubicMetres).JL_JC = ZGuid.Empty;

			PopulatePackLine(shipment2.OuterPackLines.AddNew(), 50, Constants.PkgUnit.Keg, 50, Constants.Weight.Kilograms, 50, Constants.Volume.CubicMetres).JL_JC = container1.PK;
			PopulatePackLine(shipment2.OuterPackLines.AddNew(), 60, Constants.PkgUnit.Keg, 60, Constants.Weight.Kilograms, 60, Constants.Volume.CubicMetres).JL_JC = container2.PK;
			PopulatePackLine(shipment2.OuterPackLines.AddNew(), 70, Constants.PkgUnit.Keg, 70, Constants.Weight.Kilograms, 70, Constants.Volume.CubicMetres).JL_JC = container2.PK;
			PopulatePackLine(shipment2.OuterPackLines.AddNew(), 80, Constants.PkgUnit.Keg, 80, Constants.Weight.Kilograms, 80, Constants.Volume.CubicMetres).JL_JC = ZGuid.Empty;
			PopulatePackLine(shipment2.OuterPackLines.AddNew(), 90, Constants.PkgUnit.Keg, 90, Constants.Weight.Kilograms, 90, Constants.Volume.CubicMetres).JL_JC = ZGuid.Empty;

			void AssertPopulatePackingLines_IsNotPackageGrouping()
			{
				var builder = CreateDocDataObjectBuilder(consol);
				var carrierMessageData = builder.Build();

				var shipmentDO1 = carrierMessageData.Shipments.Cast<Shipment>().First(x => x.ShipmentID == "SHIPMENT1");
				var shipmentDO2 = carrierMessageData.Shipments.Cast<Shipment>().First(x => x.ShipmentID == "SHIPMENT2");

				var containerDO1 = carrierMessageData.Containers.Cast<Container>().First(x => x.Number == "CONTAINER1");
				var containerDO2 = carrierMessageData.Containers.Cast<Container>().First(x => x.Number == "CONTAINER2");

				AssertEquals(2, containerDO1.PackingLines.Count);
				AssertEquals(60, containerDO1.PackCount);
				AssertEquals(60M, containerDO1.GoodsWeight.Value);
				AssertEquals(carrierMessageData.UnitOfWeight, containerDO1.GoodsWeight.Unit.Code);
				Assert("Registry is disabled. carrierMessageData.UnitOfWeight", containerDO1.PackingLines.All(x => x.Weight.Unit.Code == containerDO1.GoodsWeight.Unit.Code));
				AssertEquals(60M, containerDO1.Volume.Value);
				AssertEquals(carrierMessageData.UnitOfVolume, containerDO1.Volume.Unit.Code);
				Assert("Registry is disabled. carrierMessageData.UnitOfVolume", containerDO1.PackingLines.All(x => x.Volume.Unit.Code == containerDO1.Volume.Unit.Code));
				AssertContainsExactElementsInAnyOrder("Use PackLine's PackType", new string[] { Constants.PkgUnit.Pail, Constants.PkgUnit.Keg }, containerDO1.PackingLines.Select(x => x.PackageType.Code).Distinct());

				AssertEquals(3, containerDO2.PackingLines.Count);
				AssertEquals(150, containerDO2.PackCount);
				AssertEquals(130.02M, containerDO2.GoodsWeight.Value);
				AssertEquals(carrierMessageData.UnitOfWeight, containerDO2.GoodsWeight.Unit.Code);
				Assert("Registry is disabled. carrierMessageData.UnitOfWeight", containerDO2.PackingLines.All(x => x.Weight.Unit.Code == containerDO2.GoodsWeight.Unit.Code));
				AssertEquals(20130M, containerDO2.Volume.Value);
				AssertEquals(carrierMessageData.UnitOfVolume, containerDO2.Volume.Unit.Code);
				Assert("Registry is disabled. carrierMessageData.UnitOfVolume", containerDO2.PackingLines.All(x => x.Volume.Unit.Code == containerDO2.Volume.Unit.Code));
				AssertContainsExactElementsInAnyOrder("Registry is disabled. Use PackLine's PackType", new string[] { Constants.PkgUnit.Keg }, containerDO2.PackingLines.Select(x => x.PackageType.Code).Distinct());

				Assert("Registry is disabled. Shipment > PackingLines > PackingLines won't be used", carrierMessageData.Shipments.All(shipment => shipment.PackingLines.All(packingLine => packingLine.PackingLines == null)));
				AssertEquals(4, shipmentDO1.PackingLines.Count);
				AssertEquals(5, shipmentDO2.PackingLines.Count);

				var allPackingLinesIncludeCoLoad = carrierMessageData.Shipments.SelectMany(x => x.AllPackingLinesIncludeCoLoad);
				AssertEquals(9, allPackingLinesIncludeCoLoad.Count());
				Assert("Registry is disabled. carrierMessageData.UnitOfWeight", allPackingLinesIncludeCoLoad.All(packingLine => packingLine.Weight.Unit.Code == carrierMessageData.UnitOfWeight));
				Assert("Registry is disabled. carrierMessageData.UnitOfVolume", allPackingLinesIncludeCoLoad.All(packingLine => packingLine.Volume.Unit.Code == carrierMessageData.UnitOfVolume));
			}

			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.DoNotGroup;
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertPopulatePackingLines_IsNotPackageGrouping();
			}

			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertPopulatePackingLines_IsNotPackageGrouping();
			}
		}

		public void TestPopulatePackingLineMeasures_AllPackLinesForAllShipmentsAreRecordedInFeet()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.DoNotGroup;

			var container = consol.Containers.AddNew();
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();

			var s1PackLine1 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 10, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres);
			s1PackLine1.JL_Description = "s1PackLine1";
			s1PackLine1.JL_JC = container.PK;
			s1PackLine1.JL_Length = 1;
			s1PackLine1.JL_Width = 2;
			s1PackLine1.JL_Height = 3;
			s1PackLine1.JL_UnitOfDimension = Constants.Dimension.Feet;

			var s1PackLine2 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 10, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres);
			s1PackLine2.JL_Description = "s1PackLine2";
			s1PackLine2.JL_JC = container.PK;
			s1PackLine2.JL_Length = 4;
			s1PackLine2.JL_Width = 5;
			s1PackLine2.JL_Height = 6;
			s1PackLine2.JL_UnitOfDimension = Constants.Dimension.Feet;

			var s2PackLine1 = PopulatePackLine(shipment2.OuterPackLines.AddNew(), 10, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres);
			s2PackLine1.JL_Description = "s2PackLine1";
			s2PackLine1.JL_JC = container.PK;
			s2PackLine1.JL_Length = 7;
			s2PackLine1.JL_Width = 8;
			s2PackLine1.JL_Height = 9;
			s2PackLine1.JL_UnitOfDimension = Constants.Dimension.Feet;

			var builder = CreateDocDataObjectBuilder(consol);
			var carrierMessageData = builder.Build();

			var allPackingLines = carrierMessageData.Shipments.SelectMany(x => x.AllPackingLinesIncludeCoLoad);

			AssertEquals(3, allPackingLines.Count());
			Assert(allPackingLines.All(packingLine => packingLine.Height.Unit.Code == Constants.Dimension.Feet));
			Assert(allPackingLines.All(packingLine => packingLine.Width.Unit.Code == Constants.Dimension.Feet));
			Assert(allPackingLines.All(packingLine => packingLine.Length.Unit.Code == Constants.Dimension.Feet));

			var s1PackLine1DO = allPackingLines.FirstOrDefault(x => x.GoodsDescription == s1PackLine1.JL_Description);
			var s1PackLine2DO = allPackingLines.FirstOrDefault(x => x.GoodsDescription == s1PackLine2.JL_Description);
			var s2PackLine1DO = allPackingLines.FirstOrDefault(x => x.GoodsDescription == s2PackLine1.JL_Description);

			AssertNotNull(s1PackLine1DO);
			AssertNotNull(s1PackLine2DO);
			AssertNotNull(s2PackLine1DO);

			AssertEquals("3.00 FT", s1PackLine1DO.Height.ToString());
			AssertEquals("2.00 FT", s1PackLine1DO.Width.ToString());
			AssertEquals("1.00 FT", s1PackLine1DO.Length.ToString());

			AssertEquals("6.00 FT", s1PackLine2DO.Height.ToString());
			AssertEquals("5.00 FT", s1PackLine2DO.Width.ToString());
			AssertEquals("4.00 FT", s1PackLine2DO.Length.ToString());

			AssertEquals("9.00 FT", s2PackLine1DO.Height.ToString());
			AssertEquals("8.00 FT", s2PackLine1DO.Width.ToString());
			AssertEquals("7.00 FT", s2PackLine1DO.Length.ToString());
		}

		public void TestPopulatePackingLineMeasures()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.DoNotGroup;

			var container = consol.Containers.AddNew();
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();

			var s1PackLine1 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 10, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres);
			s1PackLine1.JL_Description = "s1PackLine1";
			s1PackLine1.JL_JC = container.PK;
			s1PackLine1.JL_Length = 1;
			s1PackLine1.JL_Width = 2;
			s1PackLine1.JL_Height = 3;
			s1PackLine1.JL_UnitOfDimension = Constants.Dimension.Feet;

			var s1PackLine2 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 10, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres);
			s1PackLine2.JL_Description = "s1PackLine2";
			s1PackLine2.JL_JC = container.PK;
			s1PackLine2.JL_Length = 4;
			s1PackLine2.JL_Width = 5;
			s1PackLine2.JL_Height = 6;
			s1PackLine2.JL_UnitOfDimension = Constants.Dimension.Metres;

			var s2PackLine1 = PopulatePackLine(shipment2.OuterPackLines.AddNew(), 10, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres);
			s2PackLine1.JL_Description = "s2PackLine1";
			s2PackLine1.JL_JC = container.PK;
			s2PackLine1.JL_Length = 7;
			s2PackLine1.JL_Width = 8;
			s2PackLine1.JL_Height = 9;
			s2PackLine1.JL_UnitOfDimension = Constants.Dimension.Metres;

			var builder = CreateDocDataObjectBuilder(consol);
			var carrierMessageData = builder.Build();

			var allPackingLines = carrierMessageData.Shipments.SelectMany(x => x.AllPackingLinesIncludeCoLoad);

			AssertEquals(3, allPackingLines.Count());
			Assert(allPackingLines.All(packingLine => packingLine.Height.Unit.Code == Constants.Dimension.Metres));
			Assert(allPackingLines.All(packingLine => packingLine.Width.Unit.Code == Constants.Dimension.Metres));
			Assert(allPackingLines.All(packingLine => packingLine.Length.Unit.Code == Constants.Dimension.Metres));

			var s1PackLine1DO = allPackingLines.FirstOrDefault(x => x.GoodsDescription == s1PackLine1.JL_Description);
			var s1PackLine2DO = allPackingLines.FirstOrDefault(x => x.GoodsDescription == s1PackLine2.JL_Description);
			var s2PackLine1DO = allPackingLines.FirstOrDefault(x => x.GoodsDescription == s2PackLine1.JL_Description);

			AssertNotNull(s1PackLine1DO);
			AssertNotNull(s1PackLine2DO);
			AssertNotNull(s2PackLine1DO);

			AssertEquals("0.91 M", s1PackLine1DO.Height.ToString());
			AssertEquals("0.61 M", s1PackLine1DO.Width.ToString());
			AssertEquals("0.30 M", s1PackLine1DO.Length.ToString());

			AssertEquals("6.00 M", s1PackLine2DO.Height.ToString());
			AssertEquals("5.00 M", s1PackLine2DO.Width.ToString());
			AssertEquals("4.00 M", s1PackLine2DO.Length.ToString());

			AssertEquals("9.00 M", s2PackLine1DO.Height.ToString());
			AssertEquals("8.00 M", s2PackLine1DO.Width.ToString());
			AssertEquals("7.00 M", s2PackLine1DO.Length.ToString());
		}

		public void TestPopulatePackingLines_DoNotGroup()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.DoNotGroup;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONTAINER1";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONTAINER2";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "SHIPMENT1";
			shipment1.JS_UnitOfWeight = Constants.Weight.Grams;
			shipment1.JS_UnitOfVolume = Constants.Volume.MegaLitre;
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "SHIPMENT2";
			shipment2.JS_UnitOfWeight = Constants.Weight.Grams;
			shipment2.JS_UnitOfVolume = Constants.Volume.MegaLitre;

			shipment1.InnerPackLines.RemoveAndDeleteAll();
			shipment1.OuterPackLines.RemoveAndDeleteAll();
			shipment2.InnerPackLines.RemoveAndDeleteAll();
			shipment2.OuterPackLines.RemoveAndDeleteAll();

			var s1OuterPackLine1 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 10, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres);
			s1OuterPackLine1.JL_JC = container1.PK;
			var s1OuterPackLine2 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 20, Constants.PkgUnit.Keg, 20, Constants.Weight.Grams, 20, Constants.Volume.MegaLitre);
			s1OuterPackLine2.JL_JC = container2.PK;
			PopulatePackLine(shipment1.OuterPackLines.AddNew(), 30, Constants.PkgUnit.Keg, 30, Constants.Weight.Grams, 30, Constants.Volume.MegaLitre).JL_JC = ZGuid.Empty;
			PopulatePackLine(shipment1.OuterPackLines.AddNew(), 40, Constants.PkgUnit.Keg, 40, Constants.Weight.Grams, 40, Constants.Volume.MegaLitre).JL_JC = ZGuid.Empty;

			var s1InnerPackLine1 = PopulatePackLine(shipment1.InnerPackLines.AddNew(), 30, Constants.PkgUnit.Keg, 30, Constants.Weight.Kilograms, 30, Constants.Volume.CubicMetres);
			s1InnerPackLine1.JL_JL_OuterPackLine = s1OuterPackLine1.PK;
			var s1InnerPackLine2 = PopulatePackLine(shipment1.InnerPackLines.AddNew(), 30, Constants.PkgUnit.Keg, 30, Constants.Weight.Kilograms, 30, Constants.Volume.CubicMetres);
			s1InnerPackLine2.JL_JL_OuterPackLine = s1OuterPackLine1.PK;
			var s1InnerPackLine3 = PopulatePackLine(shipment1.InnerPackLines.AddNew(), 40, Constants.PkgUnit.Keg, 40, Constants.Weight.Grams, 40, Constants.Volume.MegaLitre);
			s1InnerPackLine3.JL_JL_OuterPackLine = s1OuterPackLine2.PK;
			var s1InnerPackLine4 = PopulatePackLine(shipment1.InnerPackLines.AddNew(), 40, Constants.PkgUnit.Keg, 40, Constants.Weight.Grams, 40, Constants.Volume.MegaLitre);
			s1InnerPackLine4.JL_JL_OuterPackLine = s1OuterPackLine2.PK;
			PopulatePackLine(shipment1.InnerPackLines.AddNew(), 50, Constants.PkgUnit.Keg, 50, Constants.Weight.Grams, 50, Constants.Volume.MegaLitre).JL_JC = ZGuid.Empty;
			PopulatePackLine(shipment1.InnerPackLines.AddNew(), 60, Constants.PkgUnit.Keg, 60, Constants.Weight.Grams, 60, Constants.Volume.MegaLitre).JL_JC = ZGuid.Empty;

			var shipment2OuterPackLine = shipment2.OuterPackLines.AddNew();

			var undgSubstance1 = Factory.New<UNDGSubstance>();
			undgSubstance1.DG_Code = "UNDG1";
			var undgSubstance2 = Factory.New<UNDGSubstance>();
			undgSubstance2.DG_Code = "UNDG2";

			var undg1 = shipment2OuterPackLine.UNDGs.AddNew();
			undg1.DI_DG = undgSubstance1.PK;
			undg1.DI_DGWeight = 0.01;
			undg1.DI_UnitOfWeight = Constants.Weight.Tonnes;
			undg1.DI_DGVolume = 100;
			undg1.DI_UnitOfVolume = Constants.Volume.CubicYards;

			var undg2 = shipment2OuterPackLine.UNDGs.AddNew();
			undg2.DI_DG = undgSubstance2.PK;
			undg2.DI_DGWeight = 10;
			undg2.DI_UnitOfWeight = Constants.Weight.Grams;
			undg2.DI_DGVolume = 20;
			undg2.DI_UnitOfVolume = Constants.Volume.MegaLitre;

			var undg3 = shipment2OuterPackLine.UNDGs.AddNew();
			undg3.DI_DG = undgSubstance2.PK;
			undg3.DI_DGWeight = 10;
			undg3.DI_UnitOfWeight = Constants.Weight.Grams;
			undg3.DI_DGVolume = 20;
			undg3.DI_UnitOfVolume = Constants.Volume.MegaLitre;

			PopulatePackLine(shipment2OuterPackLine, 30, Constants.PkgUnit.Keg, 30, Constants.Weight.Kilograms, 30, Constants.Volume.CubicMetres).JL_JC = container1.PK;
			PopulatePackLine(shipment2.OuterPackLines.AddNew(), 30, Constants.PkgUnit.Keg, 30, Constants.Weight.Kilograms, 30, Constants.Volume.CubicMetres).JL_JC = container2.PK;
			PopulatePackLine(shipment2.OuterPackLines.AddNew(), 30, Constants.PkgUnit.Keg, 30, Constants.Weight.Kilograms, 30, Constants.Volume.CubicMetres).JL_JC = container2.PK;
			PopulatePackLine(shipment2.OuterPackLines.AddNew(), 40, Constants.PkgUnit.Keg, 40, Constants.Weight.Kilograms, 40, Constants.Volume.CubicMetres).JL_JC = ZGuid.Empty;
			PopulatePackLine(shipment2.OuterPackLines.AddNew(), 50, Constants.PkgUnit.Keg, 50, Constants.Weight.Kilograms, 50, Constants.Volume.CubicMetres).JL_JC = ZGuid.Empty;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var builder = CreateDocDataObjectBuilder(consol);
				var carrierMessageData = builder.Build();

				var shipmentDO1 = carrierMessageData.Shipments.Cast<Shipment>().First(x => x.ShipmentID == "SHIPMENT1");
				var shipmentDO2 = carrierMessageData.Shipments.Cast<Shipment>().First(x => x.ShipmentID == "SHIPMENT2");

				var containerDO1 = carrierMessageData.Containers.Cast<Container>().First(x => x.Number == "CONTAINER1");
				var containerDO2 = carrierMessageData.Containers.Cast<Container>().First(x => x.Number == "CONTAINER2");

				AssertEquals(2, containerDO1.PackingLines.Count);
				AssertEquals(40, containerDO1.PackCount);
				AssertEquals(40M, containerDO1.GoodsWeight.Value);
				AssertEquals(carrierMessageData.UnitOfWeight, containerDO1.GoodsWeight.Unit.Code);
				Assert(containerDO1.PackingLines.All(x => x.Weight.Unit.Code == containerDO1.GoodsWeight.Unit.Code));
				AssertEquals(40M, containerDO1.Volume.Value);
				AssertEquals(carrierMessageData.UnitOfVolume, containerDO1.Volume.Unit.Code);
				Assert(containerDO1.PackingLines.All(x => x.Volume.Unit.Code == containerDO1.Volume.Unit.Code));
				AssertContainsExactElementsInAnyOrder("Use PackLine's PackType", new string[] { Constants.PkgUnit.Pail, Constants.PkgUnit.Keg }, containerDO1.PackingLines.Select(x => x.PackageType.Code).Distinct());

				AssertEquals(3, containerDO2.PackingLines.Count);
				AssertEquals(80, containerDO2.PackCount);
				AssertEquals(60.02M, containerDO2.GoodsWeight.Value);
				AssertEquals(carrierMessageData.UnitOfWeight, containerDO2.GoodsWeight.Unit.Code);
				Assert(containerDO2.PackingLines.All(x => x.Weight.Unit.Code == containerDO2.GoodsWeight.Unit.Code));
				AssertEquals(20060M, containerDO2.Volume.Value);
				AssertEquals(carrierMessageData.UnitOfVolume, containerDO2.Volume.Unit.Code);
				Assert(containerDO2.PackingLines.All(x => x.Volume.Unit.Code == containerDO2.Volume.Unit.Code));
				AssertContainsExactElementsInAnyOrder("Use PackLine's PackType", new string[] { Constants.PkgUnit.Keg }, containerDO2.PackingLines.Select(x => x.PackageType.Code).Distinct());

				Assert("Do Not Group. Shipment > PackingLines > PackingLines won't be used", carrierMessageData.Shipments.All(shipment => shipment.PackingLines.All(packingLine => packingLine.PackingLines == null)));
				AssertEquals(4, shipmentDO1.PackingLines.Count);
				AssertEquals(5, shipmentDO2.PackingLines.Count);

				var allPackingLinesIncludeCoLoad = carrierMessageData.Shipments.SelectMany(x => x.AllPackingLinesIncludeCoLoad);
				AssertEquals(9, allPackingLinesIncludeCoLoad.Count());
				Assert(allPackingLinesIncludeCoLoad.All(packingLine => packingLine.Weight.Unit.Code == carrierMessageData.UnitOfWeight));
				Assert(allPackingLinesIncludeCoLoad.All(packingLine => packingLine.Volume.Unit.Code == carrierMessageData.UnitOfVolume));

				var dgPackLine = allPackingLinesIncludeCoLoad.First(x => x.DangerousGoods.Any());
				AssertEquals("Group DangerousGoods", 2, dgPackLine.DangerousGoods.Count);

				var dgDO1 = dgPackLine.DangerousGoods.First(x => x.Code == "UNDG1");
				var dgDO2 = dgPackLine.DangerousGoods.First(x => x.Code == "UNDG2");

				AssertEquals(10M, dgDO1.Weight.Value);
				AssertEquals(carrierMessageData.UnitOfWeight, dgDO1.Weight.Unit.Code);
				AssertEquals(76.455486M, dgDO1.Volume.Value);
				AssertEquals(carrierMessageData.UnitOfVolume, dgDO1.Volume.Unit.Code);

				AssertEquals(0.02M, dgDO2.Weight.Value);
				AssertEquals(carrierMessageData.UnitOfWeight, dgDO2.Weight.Unit.Code);
				AssertEquals(40000M, dgDO2.Volume.Value);
				AssertEquals(carrierMessageData.UnitOfVolume, dgDO2.Volume.Unit.Code);
			}
		}

		public void TestPopulatePackingLines_GroupByShipment()
		{
			CreateRefCountryRules(Constants.CountryCodes.UnitedStates, Constants.CountryCodes.China, ZString.Empty, ZGuid.Empty, true);
			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "CNSHG";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONTAINER1";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONTAINER2";
			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerNum = "CONTAINER3";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "SHIPMENT1";
			shipment1.JS_UnitOfWeight = Constants.Weight.Grams;
			shipment1.JS_UnitOfVolume = Constants.Volume.MegaLitre;
			shipment1.DetailedGoodsDescriptionNoteText = "SHIPMENT1 DetailedGoodsDescriptionNoteText";
			shipment1.JS_GoodsDescription = "SHIPMENT1 JS_GoodsDescription";
			shipment1.JS_MarksAndNumbers = "SHIPMENT1 JS_MarksAndNumbers";
			shipment1.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
			shipment1.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bag;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "SHIPMENT2";
			shipment2.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment2.JS_UnitOfVolume = Constants.Volume.Litre;
			shipment2.DetailedGoodsDescriptionNoteText = "SHIPMENT2 DetailedGoodsDescriptionNoteText";
			shipment2.JS_GoodsDescription = "SHIPMENT2 JS_GoodsDescription";
			shipment2.JS_MarksAndNumbers = "SHIPMENT2 JS_MarksAndNumbers";
			shipment2.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
			shipment2.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bag;

			shipment1.InnerPackLines.RemoveAndDeleteAll();
			shipment1.OuterPackLines.RemoveAndDeleteAll();
			shipment2.InnerPackLines.RemoveAndDeleteAll();
			shipment2.OuterPackLines.RemoveAndDeleteAll();
			shipment1.CusEntryNumbers.RemoveAndDeleteAll();

			var cusEntryNumber1 = shipment1.CusEntryNumbers.AddNew();
			cusEntryNumber1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			cusEntryNumber1.CE_EntryNum = "11321";
			var cusEntryNumber2 = shipment1.CusEntryNumbers.AddNew();
			cusEntryNumber2.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.COC;
			cusEntryNumber2.CE_EntryNum = "22412";

			var s1OuterPackLine1 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 10, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, null, null, true, -2, 3, Constants.Temperature.Fahrenheit);
			s1OuterPackLine1.JL_JC = container1.PK;

			s1OuterPackLine1.JL_HarmonisedCode = "12345";

			var s1OuterPackLine1HarmonisedCode1 = s1OuterPackLine1.HarmonisedCodes.AddNew();
			s1OuterPackLine1HarmonisedCode1.JLH_Code = "HS1";
			s1OuterPackLine1HarmonisedCode1.JLH_RN_NKCountry = "US";

			var s1OuterPackLine1HarmonisedCode2 = s1OuterPackLine1.HarmonisedCodes.AddNew();
			s1OuterPackLine1HarmonisedCode2.JLH_Code = "HS2";
			s1OuterPackLine1HarmonisedCode2.JLH_RN_NKCountry = "CN";

			var s1OuterPackLine1HarmonisedCode3 = s1OuterPackLine1.HarmonisedCodes.AddNew();
			s1OuterPackLine1HarmonisedCode3.JLH_Code = "HS3";
			s1OuterPackLine1HarmonisedCode3.JLH_RN_NKCountry = "US";

			var s1OuterPackLine2 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 20, Constants.PkgUnit.Keg, 20, Constants.Weight.Grams, 20, Constants.Volume.MegaLitre, null, null, true, 2, 5, Constants.Temperature.Fahrenheit);
			s1OuterPackLine2.JL_JC = container1.PK;

			s1OuterPackLine2.JL_HarmonisedCode = "12346";

			var s1OuterPackLine2HarmonisedCode1 = s1OuterPackLine1.HarmonisedCodes.AddNew();
			s1OuterPackLine2HarmonisedCode1.JLH_Code = "HS3";
			s1OuterPackLine2HarmonisedCode1.JLH_RN_NKCountry = "US";
			var s1OuterPackLine2HarmonisedCode2 = s1OuterPackLine1.HarmonisedCodes.AddNew();
			s1OuterPackLine2HarmonisedCode1.JLH_Code = "HS3";
			s1OuterPackLine2HarmonisedCode1.JLH_RN_NKCountry = "CN";

			var s1OuterPackLine3 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 30, Constants.PkgUnit.Pail, 30, Constants.Weight.Grams, 30, Constants.Volume.MegaLitre);
			s1OuterPackLine3.JL_JC = container2.PK;
			PopulatePackLine(shipment1.OuterPackLines.AddNew(), 40, Constants.PkgUnit.Pail, 40, Constants.Weight.Grams, 40, Constants.Volume.MegaLitre).JL_JC = ZGuid.Empty;
			PopulatePackLine(shipment1.OuterPackLines.AddNew(), 50, Constants.PkgUnit.Pail, 50, Constants.Weight.Grams, 50, Constants.Volume.MegaLitre).JL_JC = ZGuid.Empty;

			var s1InnerPackLine1 = PopulatePackLine(shipment1.InnerPackLines.AddNew(), 40, Constants.PkgUnit.Keg, 40, Constants.Weight.Kilograms, 40, Constants.Volume.CubicMetres);
			s1InnerPackLine1.JL_JL_OuterPackLine = s1OuterPackLine1.PK;
			var s1InnerPackLine2 = PopulatePackLine(shipment1.InnerPackLines.AddNew(), 50, Constants.PkgUnit.Keg, 50, Constants.Weight.Kilograms, 50, Constants.Volume.CubicMetres);
			s1InnerPackLine2.JL_JL_OuterPackLine = s1OuterPackLine1.PK;
			var s1InnerPackLine3 = PopulatePackLine(shipment1.InnerPackLines.AddNew(), 60, Constants.PkgUnit.Gross, 60, Constants.Weight.Grams, 60, Constants.Volume.MegaLitre);
			s1InnerPackLine3.JL_JL_OuterPackLine = s1OuterPackLine2.PK;
			var s1InnerPackLine4 = PopulatePackLine(shipment1.InnerPackLines.AddNew(), 70, Constants.PkgUnit.Keg, 70, Constants.Weight.Grams, 70, Constants.Volume.MegaLitre);
			s1InnerPackLine4.JL_JL_OuterPackLine = s1OuterPackLine2.PK;
			PopulatePackLine(shipment1.InnerPackLines.AddNew(), 80, Constants.PkgUnit.Keg, 80, Constants.Weight.Grams, 80, Constants.Volume.MegaLitre);
			PopulatePackLine(shipment1.InnerPackLines.AddNew(), 90, Constants.PkgUnit.Keg, 90, Constants.Weight.Grams, 90, Constants.Volume.MegaLitre);

			var shipment2OuterPackLine1 = shipment2.OuterPackLines.AddNew();

			var undgSubstance1 = Factory.New<UNDGSubstance>();
			undgSubstance1.DG_Code = "UNDG1";
			var undgSubstance2 = Factory.New<UNDGSubstance>();
			undgSubstance2.DG_Code = "UNDG2";
			var undgSubstance3 = Factory.New<UNDGSubstance>();
			undgSubstance3.DG_Code = "UNDG3";

			var undg1 = shipment2OuterPackLine1.UNDGs.AddNew();
			undg1.DI_DG = undgSubstance1.PK;
			undg1.DI_DGWeight = 0;
			undg1.DI_UnitOfWeight = Constants.Weight.Tonnes;
			undg1.DI_DGVolume = 100;
			undg1.DI_UnitOfVolume = Constants.Volume.CubicMetres;

			var undg2 = shipment2OuterPackLine1.UNDGs.AddNew();
			undg2.DI_DG = undgSubstance2.PK;
			undg2.DI_DGWeight = 0;
			undg2.DI_UnitOfWeight = Constants.Weight.Grams;
			undg2.DI_DGVolume = 20;
			undg2.DI_UnitOfVolume = Constants.Volume.MegaLitre;

			var undg3 = shipment2OuterPackLine1.UNDGs.AddNew();
			undg3.DI_DG = undgSubstance2.PK;
			undg3.DI_DGWeight = 0;
			undg3.DI_UnitOfWeight = Constants.Weight.Grams;
			undg3.DI_DGVolume = 20;
			undg3.DI_UnitOfVolume = Constants.Volume.MegaLitre;

			var undg4 = shipment2OuterPackLine1.UNDGs.AddNew();
			undg4.DI_DG = undgSubstance3.PK;
			undg4.DI_DGWeight = 2;
			undg4.DI_UnitOfWeight = Constants.Weight.Grams;
			undg4.DI_DGVolume = 20;
			undg4.DI_UnitOfVolume = Constants.Volume.MegaLitre;

			var shipment2OuterPackLine2 = shipment2.OuterPackLines.AddNew();

			var undg5 = shipment2OuterPackLine2.UNDGs.AddNew();
			undg5.DI_DG = undgSubstance3.PK;
			undg5.DI_DGWeight = 0;
			undg5.DI_UnitOfWeight = Constants.Weight.Tonnes;
			undg5.DI_DGVolume = 100;
			undg5.DI_UnitOfVolume = Constants.Volume.CubicMetres;

			var undg6 = shipment2OuterPackLine2.UNDGs.AddNew();
			undg6.DI_DG = undgSubstance3.PK;
			undg6.DI_DGWeight = 0;
			undg6.DI_UnitOfWeight = Constants.Weight.Tonnes;
			undg6.DI_DGVolume = 100;
			undg6.DI_UnitOfVolume = Constants.Volume.CubicMetres;

			PopulatePackLine(shipment2OuterPackLine1, 100, Constants.PkgUnit.Keg, 100, Constants.Weight.Kilograms, 100, Constants.Volume.CubicMetres, null, null, true, -2, 3, Constants.Temperature.Centigrade).JL_JC = container2.PK;
			PopulatePackLine(shipment2OuterPackLine2, 110, Constants.PkgUnit.Keg, 110, Constants.Weight.Kilograms, 110, Constants.Volume.CubicMetres, null, null, true, -2, 3, Constants.Temperature.Fahrenheit).JL_JC = container3.PK;
			PopulatePackLine(shipment2.OuterPackLines.AddNew(), 120, Constants.PkgUnit.Keg, 120, Constants.Weight.Kilograms, 120, Constants.Volume.CubicMetres, null, null, true, -2, 3, Constants.Temperature.Centigrade).JL_JC = container3.PK;
			PopulatePackLine(shipment2.OuterPackLines.AddNew(), 130, Constants.PkgUnit.Keg, 130, Constants.Weight.Kilograms, 130, Constants.Volume.CubicMetres).JL_JC = ZGuid.Empty;
			PopulatePackLine(shipment2.OuterPackLines.AddNew(), 140, Constants.PkgUnit.Keg, 140, Constants.Weight.Kilograms, 140, Constants.Volume.CubicMetres).JL_JC = ZGuid.Empty;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var builder = CreateDocDataObjectBuilder(consol);
				var carrierMessageData = builder.Build();

				var shipmentDO1 = carrierMessageData.Shipments.Cast<Shipment>().First(x => x.ShipmentID == "SHIPMENT1");
				var shipmentDO2 = carrierMessageData.Shipments.Cast<Shipment>().First(x => x.ShipmentID == "SHIPMENT2");

				Assert("Shipment > PackingLines > PackingLines will be used", carrierMessageData.Shipments.All(shipment => shipment.PackingLines.Count == 1));
				Assert("Shipment > PackingLines > PackingLines will be used", carrierMessageData.Shipments.All(shipment => shipment.PackingLines.All(packingLine => packingLine.PackingLines != null)));

				var shipment1GroupedPackingLine = shipmentDO1.PackingLines.First();
				var shipment2GroupedPackingLine = shipmentDO2.PackingLines.First();

				AssertEquals(2, shipment1GroupedPackingLine.PackingLines.Count);
				AssertEquals(2, shipment2GroupedPackingLine.PackingLines.Count);

				AssertEquals(250, shipment1GroupedPackingLine.Quantity);
				AssertEquals(10.05M, shipment1GroupedPackingLine.Weight.Value);
				AssertEquals(carrierMessageData.UnitOfWeight, shipment1GroupedPackingLine.Weight.Unit.Code);
				Assert("Same as Grouped Packing Line's unit", shipment1GroupedPackingLine.PackingLines.All(x => x.Weight.Unit.Code == shipment1GroupedPackingLine.Weight.Unit.Code));
				Assert("Convert", shipment1GroupedPackingLine.PackingLines.SelectMany(x => x.DangerousGoods).All(x => x.Weight.Unit.Code == shipment1GroupedPackingLine.Weight.Unit.Code));
				AssertEquals(50010M, shipment1GroupedPackingLine.Volume.Value);
				AssertEquals(carrierMessageData.UnitOfVolume, shipment1GroupedPackingLine.Volume.Unit.Code);
				Assert("Same as Grouped Packing Line's unit", shipment1GroupedPackingLine.PackingLines.All(x => x.Volume.Unit.Code == shipment1GroupedPackingLine.Volume.Unit.Code));
				Assert("Convert", shipment1GroupedPackingLine.PackingLines.SelectMany(x => x.DangerousGoods).All(x => x.Volume.Unit.Code == shipment1GroupedPackingLine.Volume.Unit.Code));
				AssertEquals(Constants.PkgUnit.Package, shipment1GroupedPackingLine.PackageType.Code);

				AssertEquals("SHIPMENT1 DetailedGoodsDescriptionNoteText", shipment1GroupedPackingLine.GoodsDescription);
				AssertEquals("SHIPMENT1 JS_MarksAndNumbers", shipment1GroupedPackingLine.MarksAndNumbers);

				AssertEquals("12345, 12346", shipment1GroupedPackingLine.HarmonizedCode.Code);
				AssertEquals("HS1, HS3", shipment1GroupedPackingLine.ExportHarmonizedCode.Code);
				AssertEquals("US", shipment1GroupedPackingLine.ExportHarmonizedCode.Country.Code);
				AssertEquals("HS2, HS3", shipment1GroupedPackingLine.ImportHarmonizedCode.Code);
				AssertEquals("CN", shipment1GroupedPackingLine.ImportHarmonizedCode.Country.Code);

				AssertContains("BKG:11321", shipment1GroupedPackingLine.ShipmentEntryNumbers);
				AssertContains("COC:22412", shipment1GroupedPackingLine.ShipmentEntryNumbers);

				Assert(shipment1GroupedPackingLine.RequiresTemperatureControl);
				AssertEquals(2M, shipment1GroupedPackingLine.TemperatureMinimum.Value);
				AssertEquals(3M, shipment1GroupedPackingLine.TemperatureMaximum.Value);
				AssertEquals(Constants.Temperature.Fahrenheit, shipment1GroupedPackingLine.TemperatureMinimum.Unit.Code);

				AssertEquals(330, shipment2GroupedPackingLine.Quantity);
				AssertEquals(330M, shipment2GroupedPackingLine.Weight.Value);
				AssertEquals(carrierMessageData.UnitOfWeight, shipment2GroupedPackingLine.Weight.Unit.Code);
				Assert("Same as Grouped Packing Line's unit", shipment2GroupedPackingLine.PackingLines.All(x => x.Volume.Unit.Code == shipment2GroupedPackingLine.Volume.Unit.Code));
				Assert("Convert", shipment2GroupedPackingLine.PackingLines.SelectMany(x => x.DangerousGoods).All(x => x.Weight.Unit.Code == shipment2GroupedPackingLine.Weight.Unit.Code));
				AssertEquals(330M, shipment2GroupedPackingLine.Volume.Value);
				AssertEquals(carrierMessageData.UnitOfVolume, shipment2GroupedPackingLine.Volume.Unit.Code);
				Assert("Same as Grouped Packing Line's unit", shipment2GroupedPackingLine.PackingLines.All(x => x.Volume.Unit.Code == shipment2GroupedPackingLine.Volume.Unit.Code));
				Assert("Convert", shipment2GroupedPackingLine.PackingLines.SelectMany(x => x.DangerousGoods).All(x => x.Volume.Unit.Code == shipment2GroupedPackingLine.Volume.Unit.Code));
				AssertEquals(shipment2.JS_F3_NKPackType, shipment2GroupedPackingLine.PackageType.Code);

				AssertEquals("SHIPMENT2 DetailedGoodsDescriptionNoteText", shipment2GroupedPackingLine.GoodsDescription);
				AssertEquals("SHIPMENT2 JS_MarksAndNumbers", shipment2GroupedPackingLine.MarksAndNumbers);

				Assert(shipment2GroupedPackingLine.RequiresTemperatureControl);
				AssertEquals(-2M, shipment2GroupedPackingLine.TemperatureMinimum.Value);
				AssertEquals(Constants.Temperature.Convert(3, Constants.Temperature.Fahrenheit, Constants.Temperature.Centigrade), shipment2GroupedPackingLine.TemperatureMaximum.Value);
				AssertEquals(Constants.Temperature.Centigrade, shipment2GroupedPackingLine.TemperatureMinimum.Unit.Code);

				var dgPackLine1 = shipment2GroupedPackingLine.PackingLines.First(x => x.DangerousGoods.Count == 3);
				AssertEquals(3, dgPackLine1.DangerousGoods.Count);

				var dgDO1 = dgPackLine1.DangerousGoods.First(x => x.Code == "UNDG1");
				var dgDO2 = dgPackLine1.DangerousGoods.First(x => x.Code == "UNDG2");
				var dgDO3 = dgPackLine1.DangerousGoods.First(x => x.Code == "UNDG3");

				AssertEquals("Multiple DG lines, won't fallback", 0M, dgDO1.Weight.Value);
				AssertEquals("Weight.Unit.Code Same as Parent PackingLine", dgPackLine1.Weight.Unit.Code, dgDO1.Weight.Unit.Code);
				AssertEquals(100M, dgDO1.Volume.Value);
				AssertEquals("Volume.Unit.Code Same as Parent PackingLine", dgPackLine1.Volume.Unit.Code, dgDO1.Volume.Unit.Code);

				AssertEquals("Multiple DG lines, won't fallback", 0M, dgDO2.Weight.Value);
				AssertEquals("Weight.Unit.Code Same as Parent PackingLine", dgPackLine1.Weight.Unit.Code, dgDO2.Weight.Unit.Code);
				AssertEquals(40000M, dgDO2.Volume.Value);
				AssertEquals("Volume.Unit.Code Same as Parent PackingLine", dgPackLine1.Volume.Unit.Code, dgDO2.Volume.Unit.Code);

				AssertEquals(0.002M, dgDO3.Weight.Value);
				AssertEquals("Weight.Unit.Code Same as Parent PackingLine", dgPackLine1.Weight.Unit.Code, dgDO3.Weight.Unit.Code);
				AssertEquals(20000M, dgDO3.Volume.Value);
				AssertEquals("Volume.Unit.Code Same as Parent PackingLine", dgPackLine1.Volume.Unit.Code, dgDO3.Volume.Unit.Code);

				var dgPackLine2 = shipment2GroupedPackingLine.PackingLines.First(x => x.DangerousGoods.Count == 1);
				AssertEquals(1, dgPackLine2.DangerousGoods.Count);

				var dgDO4 = dgPackLine2.DangerousGoods.First(x => x.Code == "UNDG3");

				AssertEquals("Only one DG line, fallback", 230M, dgDO4.Weight.Value);
				AssertEquals(230M, dgPackLine2.Weight.Value);
				AssertEquals("Weight.Unit.Code Same as Parent PackingLine", dgPackLine2.Weight.Unit.Code, dgDO4.Weight.Unit.Code);
				AssertEquals(200M, dgDO4.Volume.Value);
				AssertEquals("Volume.Unit.Code Same as Parent PackingLine", dgPackLine2.Volume.Unit.Code, dgDO4.Volume.Unit.Code);

				var shipment1consolidatedPackingLine1 = shipment1GroupedPackingLine.PackingLines.First(x => x.ContainerNumber == "CONTAINER1");
				AssertEquals(10.02M, shipment1consolidatedPackingLine1.Weight.Value);
				AssertEquals(shipment1GroupedPackingLine.Weight.Unit.Code, shipment1consolidatedPackingLine1.Weight.Unit.Code);
				AssertEquals(20010M, shipment1consolidatedPackingLine1.Volume.Value);
				AssertEquals(shipment1GroupedPackingLine.Volume.Unit.Code, shipment1consolidatedPackingLine1.Volume.Unit.Code);

				var shipment1consolidatedPackingLine2 = shipment1GroupedPackingLine.PackingLines.First(x => x.ContainerNumber == "CONTAINER2");
				AssertEquals(0.03M, shipment1consolidatedPackingLine2.Weight.Value);
				AssertEquals(shipment1GroupedPackingLine.Weight.Unit.Code, shipment1consolidatedPackingLine2.Weight.Unit.Code);
				AssertEquals(30000M, shipment1consolidatedPackingLine2.Volume.Value);
				AssertEquals(shipment1GroupedPackingLine.Volume.Unit.Code, shipment1consolidatedPackingLine2.Volume.Unit.Code);

				var shipment2consolidatedPackingLine1 = shipment2GroupedPackingLine.PackingLines.First(x => x.ContainerNumber == "CONTAINER2");
				AssertEquals(100M, shipment2consolidatedPackingLine1.Weight.Value);
				AssertEquals(shipment2GroupedPackingLine.Weight.Unit.Code, shipment2consolidatedPackingLine1.Weight.Unit.Code);
				AssertEquals(100M, shipment2consolidatedPackingLine1.Volume.Value);
				AssertEquals(shipment2GroupedPackingLine.Volume.Unit.Code, shipment2consolidatedPackingLine1.Volume.Unit.Code);

				var shipment2consolidatedPackingLine2 = shipment2GroupedPackingLine.PackingLines.First(x => x.ContainerNumber == "CONTAINER3");
				AssertEquals(230M, shipment2consolidatedPackingLine2.Weight.Value);
				AssertEquals(shipment2GroupedPackingLine.Weight.Unit.Code, shipment2consolidatedPackingLine2.Weight.Unit.Code);
				AssertEquals(230M, shipment2consolidatedPackingLine2.Volume.Value);
				AssertEquals(shipment2GroupedPackingLine.Volume.Unit.Code, shipment2consolidatedPackingLine2.Volume.Unit.Code);

				AssertEquals(1, carrierMessageData.Containers.Cast<Container>().First(x => x.Number == "CONTAINER1").PackingLines.Count);
				AssertEquals(2, carrierMessageData.Containers.Cast<Container>().First(x => x.Number == "CONTAINER2").PackingLines.Count);
				AssertEquals(1, carrierMessageData.Containers.Cast<Container>().First(x => x.Number == "CONTAINER3").PackingLines.Count);
			}
		}

		public void TestPopulatePackingLines_GroupByPackLine()
		{
			CreateRefCountryRules(Constants.CountryCodes.Australia, Constants.CountryCodes.China, ZString.Empty, ZGuid.Empty, true);
			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByPackLine;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHG";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONTAINER1";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONTAINER2";
			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerNum = "CONTAINER3";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "SHIPMENT1";
			shipment1.JS_UnitOfWeight = Constants.Weight.Grams;
			shipment1.JS_UnitOfVolume = Constants.Volume.MegaLitre;
			shipment1.DetailedGoodsDescriptionNoteText = "SHIPMENT1 DetailedGoodsDescriptionNoteText";
			shipment1.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
			shipment1.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bag;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "SHIPMENT2";
			shipment2.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment2.JS_UnitOfVolume = Constants.Volume.Litre;
			shipment2.DetailedGoodsDescriptionNoteText = "SHIPMENT2 DetailedGoodsDescriptionNoteText";
			shipment2.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
			shipment2.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bag;

			shipment1.InnerPackLines.RemoveAndDeleteAll();
			shipment1.OuterPackLines.RemoveAndDeleteAll();
			shipment2.InnerPackLines.RemoveAndDeleteAll();
			shipment2.OuterPackLines.RemoveAndDeleteAll();

			var s1OuterPackLine1 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 10, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, "Group1 MarksAndNumbers", "Group1 DetailedDescription", true, -2, 3, Constants.Temperature.Fahrenheit);
			s1OuterPackLine1.JL_JC = container1.PK;
			var s1OuterPackLine2 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 20, Constants.PkgUnit.Keg, 20, Constants.Weight.Grams, 20, Constants.Volume.MegaLitre, "Group2 MarksAndNumbers", "Group2 DetailedDescription", true, 2, 5, Constants.Temperature.Fahrenheit);
			s1OuterPackLine2.JL_JC = container1.PK;
			var s1OuterPackLine3 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 30, Constants.PkgUnit.Pail, 30, Constants.Weight.Grams, 30, Constants.Volume.MegaLitre, "Group1 MarksAndNumbers", "Group1 DetailedDescription");
			s1OuterPackLine3.JL_JC = container2.PK;
			PopulatePackLine(shipment1.OuterPackLines.AddNew(), 40, Constants.PkgUnit.Pail, 40, Constants.Weight.Grams, 40, Constants.Volume.MegaLitre).JL_JC = ZGuid.Empty;
			PopulatePackLine(shipment1.OuterPackLines.AddNew(), 50, Constants.PkgUnit.Pail, 50, Constants.Weight.Grams, 50, Constants.Volume.MegaLitre).JL_JC = ZGuid.Empty;

			var s1InnerPackLine1 = PopulatePackLine(shipment1.InnerPackLines.AddNew(), 40, Constants.PkgUnit.Keg, 40, Constants.Weight.Kilograms, 40, Constants.Volume.CubicMetres);
			s1InnerPackLine1.JL_JL_OuterPackLine = s1OuterPackLine1.PK;
			var s1InnerPackLine2 = PopulatePackLine(shipment1.InnerPackLines.AddNew(), 50, Constants.PkgUnit.Keg, 50, Constants.Weight.Kilograms, 50, Constants.Volume.CubicMetres);
			s1InnerPackLine2.JL_JL_OuterPackLine = s1OuterPackLine1.PK;
			var s1InnerPackLine3 = PopulatePackLine(shipment1.InnerPackLines.AddNew(), 60, Constants.PkgUnit.Gross, 60, Constants.Weight.Grams, 60, Constants.Volume.MegaLitre);
			s1InnerPackLine3.JL_JL_OuterPackLine = s1OuterPackLine2.PK;
			var s1InnerPackLine4 = PopulatePackLine(shipment1.InnerPackLines.AddNew(), 70, Constants.PkgUnit.Keg, 70, Constants.Weight.Grams, 70, Constants.Volume.MegaLitre);
			s1InnerPackLine4.JL_JL_OuterPackLine = s1OuterPackLine2.PK;
			PopulatePackLine(shipment1.InnerPackLines.AddNew(), 80, Constants.PkgUnit.Keg, 80, Constants.Weight.Grams, 80, Constants.Volume.MegaLitre);
			PopulatePackLine(shipment1.InnerPackLines.AddNew(), 90, Constants.PkgUnit.Keg, 90, Constants.Weight.Grams, 90, Constants.Volume.MegaLitre);

			var shipment2OuterPackLine = shipment2.OuterPackLines.AddNew();

			var undgSubstance1 = Factory.New<UNDGSubstance>();
			undgSubstance1.DG_Code = "UNDG1";
			var undgSubstance2 = Factory.New<UNDGSubstance>();
			undgSubstance2.DG_Code = "UNDG2";

			var undg1 = shipment2OuterPackLine.UNDGs.AddNew();
			undg1.DI_DG = undgSubstance1.PK;
			undg1.DI_DGWeight = 0;
			undg1.DI_UnitOfWeight = Constants.Weight.Tonnes;
			undg1.DI_DGVolume = 100;
			undg1.DI_UnitOfVolume = Constants.Volume.CubicMetres;

			var undg2 = shipment2OuterPackLine.UNDGs.AddNew();
			undg2.DI_DG = undgSubstance2.PK;
			undg2.DI_DGWeight = 10;
			undg2.DI_UnitOfWeight = Constants.Weight.Grams;
			undg2.DI_DGVolume = 20;
			undg2.DI_UnitOfVolume = Constants.Volume.MegaLitre;

			var undg3 = shipment2OuterPackLine.UNDGs.AddNew();
			undg3.DI_DG = undgSubstance2.PK;
			undg3.DI_DGWeight = 10;
			undg3.DI_UnitOfWeight = Constants.Weight.Grams;
			undg3.DI_DGVolume = 20;
			undg3.DI_UnitOfVolume = Constants.Volume.MegaLitre;

			PopulatePackLine(shipment2OuterPackLine, 100, Constants.PkgUnit.Keg, 100, Constants.Weight.Kilograms, 100, Constants.Volume.CubicMetres, "Group1 MarksAndNumbers", "Group1 DetailedDescription", true, -2, 3, Constants.Temperature.Centigrade).JL_JC = container2.PK;
			PopulatePackLine(shipment2.OuterPackLines.AddNew(), 110, Constants.PkgUnit.Keg, 110, Constants.Weight.Kilograms, 110, Constants.Volume.CubicMetres, "Group1 MarksAndNumbers", "Group1 DetailedDescription", true, -2, 3, Constants.Temperature.Fahrenheit).JL_JC = container3.PK;
			PopulatePackLine(shipment2.OuterPackLines.AddNew(), 120, Constants.PkgUnit.Keg, 120, Constants.Weight.Kilograms, 120, Constants.Volume.CubicMetres, "Group2 MarksAndNumbers", "Group2 DetailedDescription", true, -2, 3, Constants.Temperature.Centigrade).JL_JC = container3.PK;
			PopulatePackLine(shipment2.OuterPackLines.AddNew(), 130, Constants.PkgUnit.Keg, 130, Constants.Weight.Kilograms, 130, Constants.Volume.CubicMetres).JL_JC = ZGuid.Empty;
			PopulatePackLine(shipment2.OuterPackLines.AddNew(), 140, Constants.PkgUnit.Keg, 140, Constants.Weight.Kilograms, 140, Constants.Volume.CubicMetres).JL_JC = ZGuid.Empty;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var builder = CreateDocDataObjectBuilder(consol);
				var carrierMessageData = builder.Build();

				var shipmentDO1 = carrierMessageData.Shipments.Cast<Shipment>().First(x => x.ShipmentID == "SHIPMENT1");
				var shipmentDO2 = carrierMessageData.Shipments.Cast<Shipment>().First(x => x.ShipmentID == "SHIPMENT2");

				Assert("Shipment > PackingLines > PackingLines will be used", carrierMessageData.Shipments.All(shipment => shipment.PackingLines.Any()));
				Assert("Shipment > PackingLines > PackingLines will be used", carrierMessageData.Shipments.All(shipment => shipment.PackingLines.All(packingLine => packingLine.PackingLines != null)));

				AssertEquals(3, shipmentDO1.PackingLines.Count);
				AssertEquals(2, shipmentDO2.PackingLines.Count);

				var shipment1GroupedPackingLine1 = shipmentDO1.PackingLines.First(x => x.GoodsDescription.StartsWith("Group1") && x.PackageType.Code == Constants.PkgUnit.Keg);
				var shipment1GroupedPackingLine2 = shipmentDO1.PackingLines.First(x => x.GoodsDescription.StartsWith("Group1") && x.PackageType.Code != Constants.PkgUnit.Keg);
				var shipment1GroupedPackingLine3 = shipmentDO1.PackingLines.First(x => x.GoodsDescription.StartsWith("Group2"));
				var shipment2GroupedPackingLine1 = shipmentDO2.PackingLines.First(x => x.GoodsDescription.StartsWith("Group1"));
				var shipment2GroupedPackingLine2 = shipmentDO2.PackingLines.First(x => x.GoodsDescription.StartsWith("Group2"));

				AssertEquals(90, shipment1GroupedPackingLine1.Quantity);
				AssertEquals(10M, shipment1GroupedPackingLine1.Weight.Value);
				AssertEquals(carrierMessageData.UnitOfWeight, shipment1GroupedPackingLine1.Weight.Unit.Code);
				Assert("Same as Grouped Packing Line's unit", shipment1GroupedPackingLine1.PackingLines.All(x => x.Weight.Unit.Code == shipment1GroupedPackingLine1.Weight.Unit.Code));
				Assert("Convert", shipment1GroupedPackingLine1.PackingLines.SelectMany(x => x.DangerousGoods).All(x => x.Weight.Unit.Code == shipment1GroupedPackingLine1.Weight.Unit.Code));
				AssertEquals(10M, shipment1GroupedPackingLine1.Volume.Value);
				AssertEquals(carrierMessageData.UnitOfVolume, shipment1GroupedPackingLine1.Volume.Unit.Code);
				Assert("Same as Grouped Packing Line's unit", shipment1GroupedPackingLine1.PackingLines.All(x => x.Volume.Unit.Code == shipment1GroupedPackingLine1.Volume.Unit.Code));
				Assert("Convert", shipment1GroupedPackingLine1.PackingLines.SelectMany(x => x.DangerousGoods).All(x => x.Volume.Unit.Code == shipment1GroupedPackingLine1.Volume.Unit.Code));
				AssertEquals("Inner Package Type", Constants.PkgUnit.Keg, shipment1GroupedPackingLine1.PackageType.Code);

				AssertEquals("Group1 DetailedDescription", shipment1GroupedPackingLine1.GoodsDescription);
				AssertEquals("Group1 MarksAndNumbers", shipment1GroupedPackingLine1.MarksAndNumbers);

				Assert(shipment1GroupedPackingLine1.RequiresTemperatureControl);
				AssertEquals(-2M, shipment1GroupedPackingLine1.TemperatureMinimum.Value);
				AssertEquals(3M, shipment1GroupedPackingLine1.TemperatureMaximum.Value);
				AssertEquals(Constants.Temperature.Fahrenheit, shipment1GroupedPackingLine1.TemperatureMinimum.Unit.Code);

				var shipment1GroupedPackingLine1ConsolidatePackingLine = shipment1GroupedPackingLine1.PackingLines.First();
				AssertEquals("CONTAINER1", shipment1GroupedPackingLine1ConsolidatePackingLine.ContainerNumber);
				AssertEquals(90, shipment1GroupedPackingLine1ConsolidatePackingLine.Quantity);
				AssertEquals(10M, shipment1GroupedPackingLine1ConsolidatePackingLine.Weight.Value);
				AssertEquals(shipment1GroupedPackingLine1.Weight.Unit.Code, shipment1GroupedPackingLine1ConsolidatePackingLine.Weight.Unit.Code);
				AssertEquals(10M, shipment1GroupedPackingLine1ConsolidatePackingLine.Volume.Value);
				AssertEquals(shipment1GroupedPackingLine1.Volume.Unit.Code, shipment1GroupedPackingLine1ConsolidatePackingLine.Volume.Unit.Code);

				AssertEquals(30, shipment1GroupedPackingLine2.Quantity);
				AssertEquals(0.03M, shipment1GroupedPackingLine2.Weight.Value);
				AssertEquals(carrierMessageData.UnitOfWeight, shipment1GroupedPackingLine2.Weight.Unit.Code);
				Assert("Same as Grouped Packing Line's unit", shipment1GroupedPackingLine2.PackingLines.All(x => x.Weight.Unit.Code == shipment1GroupedPackingLine2.Weight.Unit.Code));
				Assert("Convert", shipment1GroupedPackingLine2.PackingLines.SelectMany(x => x.DangerousGoods).All(x => x.Weight.Unit.Code == shipment1GroupedPackingLine2.Weight.Unit.Code));
				AssertEquals(30000M, shipment1GroupedPackingLine2.Volume.Value);
				AssertEquals(carrierMessageData.UnitOfVolume, shipment1GroupedPackingLine2.Volume.Unit.Code);
				Assert("Same as Grouped Packing Line's unit", shipment1GroupedPackingLine2.PackingLines.All(x => x.Volume.Unit.Code == shipment1GroupedPackingLine2.Volume.Unit.Code));
				Assert("Convert", shipment1GroupedPackingLine2.PackingLines.SelectMany(x => x.DangerousGoods).All(x => x.Volume.Unit.Code == shipment1GroupedPackingLine2.Volume.Unit.Code));
				AssertEquals(Constants.PkgUnit.Pail, shipment1GroupedPackingLine2.PackageType.Code);

				AssertEquals("Group1 DetailedDescription", shipment1GroupedPackingLine2.GoodsDescription);
				AssertEquals("Group1 MarksAndNumbers", shipment1GroupedPackingLine2.MarksAndNumbers);

				Assert(!shipment1GroupedPackingLine2.RequiresTemperatureControl);

				var shipment1GroupedPackingLine2ConsolidatePackingLine = shipment1GroupedPackingLine2.PackingLines.First();
				AssertEquals("CONTAINER2", shipment1GroupedPackingLine2ConsolidatePackingLine.ContainerNumber);
				AssertEquals(30, shipment1GroupedPackingLine2ConsolidatePackingLine.Quantity);
				AssertEquals(0.03M, shipment1GroupedPackingLine2ConsolidatePackingLine.Weight.Value);
				AssertEquals(shipment1GroupedPackingLine2.Weight.Unit.Code, shipment1GroupedPackingLine2ConsolidatePackingLine.Weight.Unit.Code);
				AssertEquals(30000M, shipment1GroupedPackingLine2ConsolidatePackingLine.Volume.Value);
				AssertEquals(shipment1GroupedPackingLine2.Volume.Unit.Code, shipment1GroupedPackingLine2ConsolidatePackingLine.Volume.Unit.Code);

				AssertEquals(130, shipment1GroupedPackingLine3.Quantity);
				AssertEquals(0.02M, shipment1GroupedPackingLine3.Weight.Value);
				AssertEquals(carrierMessageData.UnitOfWeight, shipment1GroupedPackingLine3.Weight.Unit.Code);
				Assert("Same as Grouped Packing Line's unit", shipment1GroupedPackingLine3.PackingLines.All(x => x.Weight.Unit.Code == shipment1GroupedPackingLine3.Weight.Unit.Code));
				Assert("Convert", shipment1GroupedPackingLine3.PackingLines.SelectMany(x => x.DangerousGoods).All(x => x.Weight.Unit.Code == shipment1GroupedPackingLine3.Weight.Unit.Code));
				AssertEquals(20000M, shipment1GroupedPackingLine3.Volume.Value);
				AssertEquals(carrierMessageData.UnitOfVolume, shipment1GroupedPackingLine3.Volume.Unit.Code);
				Assert("Same as Grouped Packing Line's unit", shipment1GroupedPackingLine3.PackingLines.All(x => x.Volume.Unit.Code == shipment1GroupedPackingLine3.Volume.Unit.Code));
				Assert("Convert", shipment1GroupedPackingLine3.PackingLines.SelectMany(x => x.DangerousGoods).All(x => x.Volume.Unit.Code == shipment1GroupedPackingLine3.Volume.Unit.Code));
				AssertEquals("Inner Package Type different", Constants.PkgUnit.Package, shipment1GroupedPackingLine3.PackageType.Code);

				AssertEquals("Group2 DetailedDescription", shipment1GroupedPackingLine3.GoodsDescription);
				AssertEquals("Group2 MarksAndNumbers", shipment1GroupedPackingLine3.MarksAndNumbers);

				Assert(shipment1GroupedPackingLine3.RequiresTemperatureControl);
				AssertEquals(2M, shipment1GroupedPackingLine3.TemperatureMinimum.Value);
				AssertEquals(5M, shipment1GroupedPackingLine3.TemperatureMaximum.Value);
				AssertEquals(Constants.Temperature.Fahrenheit, shipment1GroupedPackingLine3.TemperatureMinimum.Unit.Code);

				var shipment1GroupedPackingLine3ConsolidatePackingLine = shipment1GroupedPackingLine3.PackingLines.First();
				AssertEquals("CONTAINER1", shipment1GroupedPackingLine3ConsolidatePackingLine.ContainerNumber);
				AssertEquals(130, shipment1GroupedPackingLine3ConsolidatePackingLine.Quantity);
				AssertEquals(0.02M, shipment1GroupedPackingLine3ConsolidatePackingLine.Weight.Value);
				AssertEquals(shipment1GroupedPackingLine3.Weight.Unit.Code, shipment1GroupedPackingLine3ConsolidatePackingLine.Weight.Unit.Code);
				AssertEquals(20000M, shipment1GroupedPackingLine3ConsolidatePackingLine.Volume.Value);
				AssertEquals(shipment1GroupedPackingLine3.Volume.Unit.Code, shipment1GroupedPackingLine3ConsolidatePackingLine.Volume.Unit.Code);

				AssertEquals(210, shipment2GroupedPackingLine1.Quantity);
				AssertEquals(210M, shipment2GroupedPackingLine1.Weight.Value);
				AssertEquals(carrierMessageData.UnitOfWeight, shipment2GroupedPackingLine1.Weight.Unit.Code);
				Assert("Same as Grouped Packing Line's unit", shipment2GroupedPackingLine1.PackingLines.All(x => x.Volume.Unit.Code == shipment2GroupedPackingLine1.Volume.Unit.Code));
				Assert("Convert", shipment2GroupedPackingLine1.PackingLines.SelectMany(x => x.DangerousGoods).All(x => x.Weight.Unit.Code == shipment2GroupedPackingLine1.Weight.Unit.Code));
				AssertEquals(210M, shipment2GroupedPackingLine1.Volume.Value);
				AssertEquals(carrierMessageData.UnitOfVolume, shipment2GroupedPackingLine1.Volume.Unit.Code);
				Assert("Same as Grouped Packing Line's unit", shipment2GroupedPackingLine1.PackingLines.All(x => x.Volume.Unit.Code == shipment2GroupedPackingLine1.Volume.Unit.Code));
				Assert("Convert", shipment2GroupedPackingLine1.PackingLines.SelectMany(x => x.DangerousGoods).All(x => x.Volume.Unit.Code == shipment2GroupedPackingLine1.Volume.Unit.Code));
				AssertEquals(Constants.PkgUnit.Keg, shipment2GroupedPackingLine1.PackageType.Code);

				AssertEquals("Group1 DetailedDescription", shipment2GroupedPackingLine1.GoodsDescription);
				AssertEquals("Group1 MarksAndNumbers", shipment2GroupedPackingLine1.MarksAndNumbers);

				Assert(shipment2GroupedPackingLine1.RequiresTemperatureControl);
				AssertEquals(-2M, shipment2GroupedPackingLine1.TemperatureMinimum.Value);
				AssertEquals(Constants.Temperature.Convert(3, Constants.Temperature.Fahrenheit, Constants.Temperature.Centigrade), shipment2GroupedPackingLine1.TemperatureMaximum.Value);
				AssertEquals(Constants.Temperature.Centigrade, shipment2GroupedPackingLine1.TemperatureMinimum.Unit.Code);

				var shipment2GroupedPackingLine1ConsolidatePackingLine1 = shipment2GroupedPackingLine1.PackingLines.First(x => x.ContainerNumber == "CONTAINER2");
				AssertEquals("CONTAINER2", shipment2GroupedPackingLine1ConsolidatePackingLine1.ContainerNumber);
				AssertEquals(100, shipment2GroupedPackingLine1ConsolidatePackingLine1.Quantity);
				AssertEquals(100M, shipment2GroupedPackingLine1ConsolidatePackingLine1.Weight.Value);
				AssertEquals(shipment2GroupedPackingLine1.Weight.Unit.Code, shipment2GroupedPackingLine1ConsolidatePackingLine1.Weight.Unit.Code);
				AssertEquals(100M, shipment2GroupedPackingLine1ConsolidatePackingLine1.Volume.Value);
				AssertEquals(shipment2GroupedPackingLine1.Volume.Unit.Code, shipment2GroupedPackingLine1ConsolidatePackingLine1.Volume.Unit.Code);

				var shipment2GroupedPackingLine1ConsolidatePackingLine2 = shipment2GroupedPackingLine1.PackingLines.First(x => x.ContainerNumber == "CONTAINER3");
				AssertEquals("CONTAINER3", shipment2GroupedPackingLine1ConsolidatePackingLine2.ContainerNumber);
				AssertEquals(110, shipment2GroupedPackingLine1ConsolidatePackingLine2.Quantity);
				AssertEquals(110M, shipment2GroupedPackingLine1ConsolidatePackingLine2.Weight.Value);
				AssertEquals(shipment2GroupedPackingLine1.Weight.Unit.Code, shipment2GroupedPackingLine1ConsolidatePackingLine2.Weight.Unit.Code);
				AssertEquals(110M, shipment2GroupedPackingLine1ConsolidatePackingLine2.Volume.Value);
				AssertEquals(shipment2GroupedPackingLine1.Volume.Unit.Code, shipment2GroupedPackingLine1ConsolidatePackingLine2.Volume.Unit.Code);

				var dgPackLine = shipment2GroupedPackingLine1.PackingLines.First(x => x.DangerousGoods.Any());
				AssertEquals(2, dgPackLine.DangerousGoods.Count);

				var dgDO1 = dgPackLine.DangerousGoods.First(x => x.Code == "UNDG1");
				var dgDO2 = dgPackLine.DangerousGoods.First(x => x.Code == "UNDG2");

				AssertEquals("Multiple DG lines, won't fallback", 0M, dgDO1.Weight.Value);
				AssertEquals("Weight.Unit.Code Same as Parent PackingLine", dgPackLine.Weight.Unit.Code, dgDO1.Weight.Unit.Code);
				AssertEquals(100M, dgDO1.Volume.Value);
				AssertEquals("Volume.Unit.Code Same as Parent PackingLine", dgPackLine.Volume.Unit.Code, dgDO1.Volume.Unit.Code);

				AssertEquals(0.02M, dgDO2.Weight.Value);
				AssertEquals("Weight.Unit.Code Same as Parent PackingLine", dgPackLine.Weight.Unit.Code, dgDO2.Weight.Unit.Code);
				AssertEquals(40000M, dgDO2.Volume.Value);
				AssertEquals("Volume.Unit.Code Same as Parent PackingLine", dgPackLine.Volume.Unit.Code, dgDO2.Volume.Unit.Code);

				AssertEquals(120, shipment2GroupedPackingLine2.Quantity);
				AssertEquals(120M, shipment2GroupedPackingLine2.Weight.Value);
				AssertEquals(carrierMessageData.UnitOfWeight, shipment2GroupedPackingLine2.Weight.Unit.Code);
				Assert("Same as Grouped Packing Line's unit", shipment2GroupedPackingLine2.PackingLines.All(x => x.Volume.Unit.Code == shipment2GroupedPackingLine2.Volume.Unit.Code));
				Assert("Convert", shipment2GroupedPackingLine2.PackingLines.SelectMany(x => x.DangerousGoods).All(x => x.Weight.Unit.Code == shipment2GroupedPackingLine2.Weight.Unit.Code));
				AssertEquals(120M, shipment2GroupedPackingLine2.Volume.Value);
				AssertEquals(carrierMessageData.UnitOfVolume, shipment2GroupedPackingLine2.Volume.Unit.Code);
				Assert("Same as Grouped Packing Line's unit", shipment2GroupedPackingLine2.PackingLines.All(x => x.Volume.Unit.Code == shipment2GroupedPackingLine2.Volume.Unit.Code));
				Assert("Convert", shipment2GroupedPackingLine2.PackingLines.SelectMany(x => x.DangerousGoods).All(x => x.Volume.Unit.Code == shipment2GroupedPackingLine2.Volume.Unit.Code));
				AssertEquals(Constants.PkgUnit.Keg, shipment2GroupedPackingLine2.PackageType.Code);

				AssertEquals("Group2 DetailedDescription", shipment2GroupedPackingLine2.GoodsDescription);
				AssertEquals("Group2 MarksAndNumbers", shipment2GroupedPackingLine2.MarksAndNumbers);

				Assert(shipment2GroupedPackingLine2.RequiresTemperatureControl);
				AssertEquals(-2M, shipment2GroupedPackingLine2.TemperatureMinimum.Value);
				AssertEquals(3M, shipment2GroupedPackingLine2.TemperatureMaximum.Value);
				AssertEquals(Constants.Temperature.Centigrade, shipment2GroupedPackingLine2.TemperatureMinimum.Unit.Code);

				var shipment2GroupedPackingLine2ConsolidatePackingLine = shipment2GroupedPackingLine2.PackingLines.First();
				AssertEquals("CONTAINER3", shipment2GroupedPackingLine2ConsolidatePackingLine.ContainerNumber);
				AssertEquals(120, shipment2GroupedPackingLine2ConsolidatePackingLine.Quantity);
				AssertEquals(120M, shipment2GroupedPackingLine2ConsolidatePackingLine.Weight.Value);
				AssertEquals(shipment2GroupedPackingLine2.Weight.Unit.Code, shipment2GroupedPackingLine2ConsolidatePackingLine.Weight.Unit.Code);
				AssertEquals(120M, shipment2GroupedPackingLine2ConsolidatePackingLine.Volume.Value);
				AssertEquals(shipment2GroupedPackingLine2.Volume.Unit.Code, shipment2GroupedPackingLine2ConsolidatePackingLine.Volume.Unit.Code);

				AssertEquals(2, carrierMessageData.Containers.Cast<Container>().First(x => x.Number == "CONTAINER1").PackingLines.Count);
				AssertEquals(2, carrierMessageData.Containers.Cast<Container>().First(x => x.Number == "CONTAINER2").PackingLines.Count);
				AssertEquals(2, carrierMessageData.Containers.Cast<Container>().First(x => x.Number == "CONTAINER3").PackingLines.Count);
			}
		}

		#endregion

		public void TestIsToSpecificAfricanCountry()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "GHACC";

			var builder = CreateDocDataObjectBuilder(consol);
			var carrierMessageData = builder.Build();

			Assert(carrierMessageData.IsToSpecificAfricanCountry);

			consol.JK_RL_NKDischargePort = "AUSYD";
			builder = CreateDocDataObjectBuilder(consol);
			carrierMessageData = builder.Build();

			Assert(!carrierMessageData.IsToSpecificAfricanCountry);
		}

		public void TestUnlocoIsSetToNotLongerModifiable()
		{
			var consol = CreateConsol();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_AgentType = Constants.AgentType.Agent;

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			Assert("Origin should be read-only for UNLOCO", (carrierMessageData.Origin).Code_DisableModifiable);
			Assert("Destination should be read-only for UNLOCO", (carrierMessageData.Destination).Code_DisableModifiable);

			Assert("Port of Loading should be read-only for UNLOCO", (carrierMessageData.PortOfLoading).Code_DisableModifiable);
			Assert("Port of Discharge should be read-only for UNLOCO", (carrierMessageData.PortOfDischarge).Code_DisableModifiable);

			Assert("Place of Receipt should be read-only for UNLOCO", (carrierMessageData.PlaceOfReceipt).Code_DisableModifiable);
			Assert("Place of Delivery should be read-only for UNLOCO", (carrierMessageData.PlaceOfDelivery).Code_DisableModifiable);
			Assert("Place of Issue should be read-only for UNLOCO", (carrierMessageData.PlaceOfIssue).Code_DisableModifiable);
		}

		#region TestContainerMode_ShippersConsol

		public void TestContainerMode_ShippersConsol()
		{
			var consol = CreateConsol();
			consol.JK_ConsolMode = Constants.ContainerModes.ShippersConsol;

			var data = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals(Constants.ContainerModes.FCL, data.ContainerMode.Code);
			AssertEquals(Constants.ContainerModeDescriptions.FCL, data.ContainerMode.Description);
		}

		#endregion

		#region Sort PackingLines

		public void TestSortPackingLines_DoNotGroup()
		{
			var consol = SetupConsolAndPackLines(Constants.PackageGrouping.Codes.DoNotGroup);

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertSortPackingLines_DoNotGroup();
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertSortPackingLines_DoNotGroup();
			}

			#region Helpers

			void AssertSortPackingLines_DoNotGroup()
			{
				var builder = CreateDocDataObjectBuilder(consol);
				var carrierMessageData = builder.Build();

				var containers = carrierMessageData.Containers.ToArray();
				AssertEquals(4, containers.Length);
				Assert(containers.Select(c => c.Number.ToString()).SequenceEqual(["CAIU9141974", "DEMO7777030", "KKFU7560350", "TEMU8897929"]));

				AssertPackLinesSequence(containers[0], ["Test005", "Test006", "Test007"]);
				AssertPackLinesSequence(containers[1], ["Test011", "Test012", "Test013", "Test010", "Test003", "Test004"]);
				AssertPackLinesSequence(containers[2], ["Test009", "Test002"]);
				AssertPackLinesSequence(containers[3], ["Test008", "Test001"]);
			}

			void AssertPackLinesSequence(Container containerDO, IEnumerable<string> packLineIds)
			{
				Assert(containerDO.PackingLines.Select(p => p.PackingLineID.ToString()).SequenceEqual(packLineIds));
			}

			#endregion
		}

		public void TestSortPackingLines_GroupByShipment()
		{
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = SetupConsolAndPackLines(Constants.PackageGrouping.Codes.GroupByShipment);

				var builder = CreateDocDataObjectBuilder(consol);
				var carrierMessageData = builder.Build();

				var shipments = carrierMessageData.Shipments.ToArray();
				Assert(shipments.Select(s => s.ShipmentID.ToString()).SequenceEqual(["SHIPMENT1", "SHIPMENT2", "SHIPMENT3"]));

				AssertPackingLinesOrderedByContainerNumber(shipments[0], "SHIPMENT1", ["DEMO7777030"]);
				AssertPackingLinesOrderedByContainerNumber(shipments[1], "SHIPMENT2", ["DEMO7777030", "KKFU7560350", "TEMU8897929"]);
				AssertPackingLinesOrderedByContainerNumber(shipments[2], "SHIPMENT3", ["CAIU9141974", "DEMO7777030", "KKFU7560350", "TEMU8897929"]);
			}

			void AssertPackingLinesOrderedByContainerNumber(Shipment shipment, ZString shipmentID, IEnumerable<string> containerNumbers)
			{
				AssertEquals(shipmentID, shipment.ShipmentID);

				AssertEquals(1, shipment.PackingLines.Count);
				Assert(shipment.PackingLines.First().PackingLines.Select(p => p.ContainerNumber.ToString()).SequenceEqual(containerNumbers));
			}
		}

		#region Sort PackingLines Helpers

		ForwardingConsol SetupConsolAndPackLines(ZString packageGrouping)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_PackageGrouping = packageGrouping;

			var container4 = CreateContainer(consol, "TEMU8897929");
			var container3 = CreateContainer(consol, "KKFU7560350");
			var container2 = CreateContainer(consol, "DEMO7777030");
			var container1 = CreateContainer(consol, "CAIU9141974");

			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_UniqueConsignRef = "SHIPMENT3";

			CreatePackLine(container4, shipment3, "Test001");
			CreatePackLine(container3, shipment3, "Test002");
			CreatePackLine(container2, shipment3, "Test004");
			CreatePackLine(container2, shipment3, "Test003");
			CreatePackLine(container1, shipment3, "Test006");
			CreatePackLine(container1, shipment3, "Test005");
			CreatePackLine(container1, shipment3, "Test007");

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "SHIPMENT2";
			CreatePackLine(container4, shipment2, "Test008");
			CreatePackLine(container3, shipment2, "Test009");
			CreatePackLine(container2, shipment2, "Test010");

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "SHIPMENT1";
			CreatePackLine(container2, shipment1, "Test011");
			CreatePackLine(container2, shipment1, "Test012");
			CreatePackLine(container2, shipment1, "Test013");

			return consol;
		}

		ForwardingContainer CreateContainer(ForwardingConsol consol, ZString containerNumber)
		{
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = containerNumber;

			return container;
		}

		void CreatePackLine(ForwardingContainer container, ForwardingShipment shipment, string packLineId)
		{
			var packLine = PopulatePackLine(shipment.OuterPackLines.AddNew(), 2, Constants.PkgUnit.Pallet, 1, Constants.Weight.Kilograms, 1, Constants.Volume.CubicMetres, packLineId: packLineId);
			packLine.JL_JC = container.PK;
		}

		#endregion

		#endregion

		#region Implementation

		protected abstract CarrierMessageDataBuilder CreateDocDataObjectBuilder(ForwardingConsol consol);

		protected abstract string GetCarrierMessageDataBuilderName();

		protected ForwardingConsol CreateConsol()
		{
			return CreateConsol("CNSHA", "AUSYD");
		}

		void SetShippingLineOption(RefShippingLine shippingLine, bool value)
		{
			if (GetCarrierMessageDataBuilderName() == DataContext.BookingRequest)
			{
				shippingLine.RSL_BookingRequestAvailable = value;
			}
			else
			{
				shippingLine.RSL_ShippingInstructionAvailable = value;
			}
		}

		protected ForwardingConsol CreateConsol(string loadPort, string dischargePort)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001001";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = loadPort;
			consol.JK_RL_NKDischargePort = dischargePort;
			consol.JK_BookingReference = "驴100";
			consol.JK_NoOriginalBills = 1;
			consol.JK_NoCopyBills = 3;
			consol.JK_MasterBillIssueDate = new ZDateTime(2018, 10, 1);

			consol.JK_CarrierContractNumber = "11111";

			var unpackDepotAddress = Factory.New<OrgHeader>();
			unpackDepotAddress.OH_FullName = "BLOOP";
			unpackDepotAddress.OH_RL_NKClosestPort = "USJFK";
			unpackDepotAddress.MainAddress.Address1 = "199 Crab Road";
			unpackDepotAddress.MainAddress.Address2 = "Crabby";
			unpackDepotAddress.MainAddress.City = "New York";
			unpackDepotAddress.MainAddress.Postcode = "10005";
			unpackDepotAddress.MainAddress.OA_RN_NKCountryCode = "US";
			consol.JK_OA_UnpackDepotAddress = unpackDepotAddress.MainAddress.PK;

			var dangerousGood1 = consol.ConsolDGRestrictionCollection.AddNew();
			dangerousGood1.JKD_Class = "1.1D";
			dangerousGood1.JKD_UNNO = "0004";
			dangerousGood1.JKD_Variant = "a";
			var dangerousGood2 = consol.ConsolDGRestrictionCollection.AddNew();
			dangerousGood2.JKD_Class = "6.1";
			dangerousGood2.JKD_UNNO = "1695";
			dangerousGood2.JKD_Variant = "";

			var letterOfCredit = consol.Numbers.AddNew();
			letterOfCredit.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.LetterOfCreditNumber;
			letterOfCredit.CE_EntryNum = "22222";

			var sldNumber = consol.Numbers.AddNew();
			sldNumber.CE_EntryType = ChinaAdditionalReferenceNumberTypes.Codes.ShippingOrderNumber;
			sldNumber.CE_EntryNum = "12345";

			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = loadPort;
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_Vessel = "Dragon";
			transport.JW_VoyageFlight = "111";
			transport.JW_ETD = new ZDateTime(2018, 12, 1);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_TransportType = Constants.TransportPlanningType.Other;
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "NZAKL";
			transport2.JW_Vessel = "Steven";
			transport2.JW_VoyageFlight = "222";

			var transport3 = consol.Transports.AddNew();
			transport3.JW_LegOrder = 3;
			transport3.JW_TransportMode = Constants.TransportModes.Sea;
			transport3.JW_TransportType = Constants.TransportPlanningType.Other;
			transport3.JW_RL_NKLoadPort = "NZAKL";
			transport3.JW_RL_NKDiscPort = dischargePort;
			transport3.JW_Vessel = "Miranda";
			transport3.JW_VoyageFlight = "333";

			PopulateAddresses(consol);

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_DeliveryMode = "CFS/CY";
			container.JC_IsShipperOwned = true;
			container.JC_GrossWeightUQ = "KG";
			container.JC_TareWeight = 1000;
			container.JC_DunnageWeight = 1000;
			container.JC_RC = (Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP")).PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "SH0001000";
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = loadPort;
			shipment.JS_RL_NKDestination = dischargePort;
			shipment.JS_HouseBillIssueDate = new ZDateTime(2018, 10, 1);
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_CY;
			shipment.JS_INCO = Constants.IncoTerms.CostAndFreight;
			shipment.JS_ShippedOnBoard = "SHP";
			shipment.JS_ShippedOnBoardDate = ZDate.Today;
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(2);
			shipment.JS_GoodsDescription = "goods description";
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";
			shipment.JS_NoOriginalBills = 1;
			shipment.JS_NoCopyBills = 2;

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var consignorPickupAddress = Factory.New<OrgHeader>();
			consignorPickupAddress.OH_FullName = "CONSPA";
			consignorPickupAddress.OH_RL_NKClosestPort = "AUSYD";
			consignorPickupAddress.MainAddress.Address1 = "Unit 15";
			consignorPickupAddress.MainAddress.Address2 = "5 Lost Lane";
			consignorPickupAddress.MainAddress.City = "Sydney";
			consignorPickupAddress.MainAddress.Postcode = "2000";
			consignorPickupAddress.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment.ConsignorPickupAddress.E2_OA_Address = consignorPickupAddress.MainAddress.PK;
			FillWithContactInformation(consignorPickupAddress.MainAddress);

			var consigneeDeliveryAddress = Factory.New<OrgHeader>();
			consigneeDeliveryAddress.OH_FullName = "LCMSIN";
			consigneeDeliveryAddress.OH_RL_NKClosestPort = "SGSIN";
			consigneeDeliveryAddress.MainAddress.Address1 = "Unit 155";
			consigneeDeliveryAddress.MainAddress.Address2 = "55 Lost Lane";
			consigneeDeliveryAddress.MainAddress.City = "SINGAPORE";
			consigneeDeliveryAddress.MainAddress.Postcode = "2215";
			consigneeDeliveryAddress.MainAddress.OA_RN_NKCountryCode = "SG";
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = consigneeDeliveryAddress.MainAddress.PK;
			FillWithContactInformation(consigneeDeliveryAddress.MainAddress);

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 2;
			packline1.JL_F3_NKPackType = "PLT";
			packline1.JL_ActualWeight = 200;
			packline1.JL_ActualWeightUQ = "KG";
			packline1.JL_ActualVolume = 300;
			packline1.JL_ActualVolumeUQ = "CF";
			packline1.JL_HarmonisedCode = "ABCDE";
			packline1.JL_ExportRefNumber = "REF001";
			packline1.JL_DetailedDescription = "pack1";
			packline1.JL_ContainerPackingOrder = 1;

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Handsome";
			contact.OC_Phone = "1234567";

			var subs = UNDGSubstanceLoader.LoadSubstances(Factory, "6666", "E", "IMO").FirstOrDefault();
			if (subs == null)
			{
				subs = Factory.New<UNDGSubstance>();
				subs.DG_UNNO = "6666";
				subs.DG_Variant = "E";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			}
			var undg = packline1.UNDGs.AddNew();
			undg.LinkDefault(subs);
			undg.Substance.DG_PSN = "DG SHIPPER NAME";
			undg.DI_TechnicalName = "WHATEVER";
			undg.DI_IMOClass = "CLAS";
			undg.Substance.DG_PG = "GRO";
			undg.Substance.DG_SubLabel1 = "sub1";
			undg.Substance.DG_SubLabel2 = "su2";
			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 15.0m;
			undg.DI_MPMarinePollutant = "N";
			undg.DI_DGVolume = 2m;
			undg.DI_UnitOfVolume = "M3";
			undg.DI_DGWeight = 200m;
			undg.DI_UnitOfWeight = "KG";
			undg.DI_IsLimitedQuantity = true;
			undg.DI_PackageCount = 5;
			undg.DI_F3_NKPackType = "BAG";
			undg.DI_OC_DGContact = contact.PK;

			var hc = Factory.New<JobPackLineHarmonisedCode>();
			hc.JLH_RN_NKCountry = "CN";
			hc.JLH_Code = "1234";

			packline1.HarmonisedCodes.Add(hc);

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_ExportRefNumber = "BBB";
			packline2.JL_PackageCount = 5;
			packline2.JL_ExportRefNumber = "REF001";
			packline2.JL_DetailedDescription = "pack2";
			packline2.JL_ContainerPackingOrder = 2;
			packline2.JL_ActualWeightUQ = "KG";
			packline2.JL_ActualVolumeUQ = "CF";

			var packline3 = shipment.OuterPackLines.AddNew();
			packline3.JL_PackageCount = 3;
			packline3.JL_F3_NKPackType = "PLT";
			packline3.JL_ActualWeight = 20000;
			packline3.JL_ActualWeightUQ = "G";
			packline3.JL_ActualVolume = 300;
			packline3.JL_ActualVolumeUQ = "CF";
			packline3.JL_HarmonisedCode = "EEEEE";
			packline3.JL_ExportRefNumber = "REF002";
			packline3.JL_DetailedDescription = "pack3";
			packline3.JL_ContainerPackingOrder = 3;

			container.PackLines.Add(packline1);
			container.PackLines.Add(packline2);
			container.PackLines.Add(packline3);

			return consol;
		}

		void PopulateAddresses(ForwardingConsol consol)
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
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = consol.JK_RL_NKLoadPort.Left(2);

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var sendingForwarderContact = Factory.New<OrgContact>();
			sendingForwarderContact.OC_OH = sendingForwarder.PK;
			sendingForwarderContact.OC_ContactName = "Sender Name";
			sendingForwarderContact.OC_Email = "name@sender.com";
			sendingForwarderContact.OC_Phone = "1111111";
			sendingForwarderContact.OC_Fax = "2222222";

			consol.JK_OC_SendingForwarderContact = sendingForwarderContact.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "I'm Receiving Stuff";
			receivingForwarder.OH_RL_NKClosestPort = "AUSYD";
			receivingForwarder.MainAddress.Address1 = "Unit 399";
			receivingForwarder.MainAddress.Address2 = "50 What Lane";
			receivingForwarder.MainAddress.City = "Sydney";
			receivingForwarder.MainAddress.Postcode = "5023";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = consol.JK_RL_NKDischargePort.Left(2);

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var receivingForwarderContact = Factory.New<OrgContact>();
			receivingForwarderContact.OC_OH = receivingForwarder.PK;
			receivingForwarderContact.OC_ContactName = "Receiver Name";
			receivingForwarderContact.OC_Email = "name@receiver.com";
			receivingForwarderContact.OC_Phone = "3333333";
			receivingForwarderContact.OC_Fax = "4444444";

			consol.JK_OC_ReceivingForwarderContact = receivingForwarderContact.PK;

			var handlingAgent = Factory.New<OrgHeader>();
			handlingAgent.OH_FullName = "I'm Handling Stuff";
			handlingAgent.OH_RL_NKClosestPort = "AUSYD";
			handlingAgent.MainAddress.Address1 = "Unit 2";
			handlingAgent.MainAddress.Address2 = "60 What Lane";
			handlingAgent.MainAddress.City = "Sydney";
			handlingAgent.MainAddress.Postcode = "2023";
			handlingAgent.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.CarrierHandlingAgentDocumentaryAddress.E2_OA_Address = handlingAgent.MainAddress.PK;

			var bookingAgent = Factory.New<OrgHeader>();
			bookingAgent.OH_FullName = "I'm Booking Stuff";
			bookingAgent.OH_RL_NKClosestPort = "AUSYD";
			bookingAgent.MainAddress.Address1 = "Unit 2";
			bookingAgent.MainAddress.Address2 = "60 What Lane";
			bookingAgent.MainAddress.City = "Sydney";
			bookingAgent.MainAddress.Postcode = "2023";
			bookingAgent.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.CarrierBookingAgentDocumentaryAddress.E2_OA_Address = bookingAgent.MainAddress.PK;

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "Stay in touch";
			notifyParty.OH_RL_NKClosestPort = "AUSYD";
			notifyParty.MainAddress.Address1 = "Unit 205";
			notifyParty.MainAddress.Address2 = "128 Why Lane";
			notifyParty.MainAddress.City = "Sydney";
			notifyParty.MainAddress.Postcode = "2000";
			notifyParty.MainAddress.OA_RN_NKCountryCode = consol.JK_RL_NKDischargePort.Left(2);
			notifyParty.MainAddress.OA_Email = "stayintouch@test.com";

			consol.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var notifyParty2 = Factory.New<OrgHeader>();
			notifyParty2.OH_FullName = "I'm Notifying About Stuff";
			notifyParty2.OH_RL_NKClosestPort = "AUSYD";
			notifyParty2.MainAddress.Address1 = "Unit 2";
			notifyParty2.MainAddress.Address2 = "60 What Lane";
			notifyParty2.MainAddress.City = "Sydney";
			notifyParty2.MainAddress.Postcode = "2023";
			notifyParty2.MainAddress.OA_RN_NKCountryCode = consol.JK_RL_NKDischargePort.Left(2);

			consol.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2.MainAddress.PK;

			var notifyParty3 = Factory.New<OrgHeader>();
			notifyParty3.OH_FullName = "Notifying 3";
			notifyParty3.OH_RL_NKClosestPort = "AUSYD";
			notifyParty3.MainAddress.Address1 = "Unit 2";
			notifyParty3.MainAddress.Address2 = "60 What Kine";
			notifyParty3.MainAddress.City = "Sydney";
			notifyParty3.MainAddress.Postcode = "2029";
			notifyParty3.MainAddress.OA_RN_NKCountryCode = consol.JK_RL_NKDischargePort.Left(2);

			consol.NotifyParty3DocumentaryAddress.E2_OA_Address = notifyParty3.MainAddress.PK;

			var creditor = Factory.New<OrgHeader>();
			creditor.OH_FullName = "I'm The Money";
			creditor.OH_RL_NKClosestPort = "AUMEL";
			creditor.MainAddress.Address1 = "Cashed up";
			creditor.MainAddress.Address2 = "1 Moolah St";
			creditor.MainAddress.City = "Moneyville";
			creditor.MainAddress.Postcode = "3999";
			creditor.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_PortName = "Test";
			unloco.RL_Code = "12345";

			consol.JK_RL_NKCarrierBookingOffice = unloco.RL_Code;
		}

		void SetupJobConsolCost(ForwardingConsol consol)
		{
			var chargeCodeFilter = new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			chargeCodeFilter.AddToFilter(AccChargeCodeSchema.AC_Code, ChargeCodeGroupList.Codes.Freight);
			var accChargeCode = consol.Factory.LoadTop1<IAccChargeCode>(chargeCodeFilter);
			if (accChargeCode != null)
			{
				var cost = (BusinessObject)Factory.New<IJobConsolCost>();
				cost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
				try
				{
					cost[JobConsolCostSchema.E6_ParentID] = consol.PK;
					cost[JobConsolCostSchema.E6_AC_ChargeCode] = accChargeCode.PK;
				}
				finally
				{
					cost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
				}
				cost[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;
			}
		}

		protected RefCountryRules CreateRefCountryRules(ZString originCountry, ZString destinationCountry, ZString transportMode, ZGuid serviceLevelPK, ZBool isShowInner)
		{
			var rule = Factory.NewWithValidTestData<RefCountryRules>();

			rule.R7_RN_NKOrigin = originCountry;
			rule.R7_RN_NKDestination = destinationCountry;
			rule.R7_TransportMode = transportMode;
			rule.R7_RS = serviceLevelPK;
			rule.R7_ShowInner = isShowInner;

			return rule;
		}

		protected PackLine PopulatePackLine(PackLine packLine, ZInt packageCount, ZString packType, ZDecimal weight, ZString unitOfWeight, ZDecimal volume, ZString unitOfVolume, string marksAndNumbers = null, string detailedDescription = null,
			bool requiresTemperatureControl = false, int requiredTemperatureMinimum = -10, int requiredTemperatureMaximum = 10, string requiredTemperatureUnit = Constants.Temperature.Centigrade, string packLineId = "")
		{
			packLine.JL_PackageCount = packageCount;
			packLine.JL_F3_NKPackType = packType;
			packLine.JL_ActualWeight = weight;
			packLine.JL_ActualWeightUQ = unitOfWeight;
			packLine.JL_ActualVolume = volume;
			packLine.JL_ActualVolumeUQ = unitOfVolume;
			packLine.JL_MarksAndNumbers = marksAndNumbers ?? ZString.Empty;
			packLine.JL_DetailedDescription = detailedDescription ?? ZString.Empty;
			packLine.JL_RequiredTemperatureMinimum = requiredTemperatureMinimum;
			packLine.JL_RequiredTemperatureMaximum = requiredTemperatureMaximum;
			packLine.JL_RequiredTemperatureUnit = requiredTemperatureUnit;
			packLine.JL_RequiresTemperatureControl = requiresTemperatureControl;
			packLine.JL_PackLineId = packLineId;

			return packLine;
		}

		#endregion
	}
}
