using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Documents.Testing.BR
{
	sealed class CargoControlAndTransitHouseManifestBuilderTest : TestCaseWithFactory
	{
		#region Tests

		public void TestBuild()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var cctHouseManifest = new CargoControlAndTransitHouseManifestBuilder(consol).Build();
			AssertHasMessageError("No shipments error", cctHouseManifest.ErrorPlaceHolderInfo, "Shipment Data is required to send CCT House Manifest.");

			var shipment1 = consol.Shipments.AddNew();
			PopulateShipment(shipment1, "081001", 12, 25, "goods1");

			var shipment2 = consol.Shipments.AddNew();
			PopulateShipment(shipment2, "081002", 14, 27, "goods2");

			cctHouseManifest = new CargoControlAndTransitHouseManifestBuilder(consol).Build();
			AssertNoMessageError("No error", cctHouseManifest.ErrorPlaceHolderInfo, "Shipment Data is required to send CCT House Manifest.");

			AssertNotNull(consol);
			AssertNotNull(consol.Shipments);
			AssertNotNull(cctHouseManifest);

			AssertHeader(consol, cctHouseManifest);
			AssertShipments(consol, cctHouseManifest);
		}

		public void TestSendingParty()
		{
			CommonContext context = null;
			var cctHouseManifest = ConfigCctHouseManifest(ref context);
			var sendingParty = (Address)cctHouseManifest.SendingParty;

			AssertEquals("Company Name should not have empty value.", "EDI CUSTOMS BROKERS", sendingParty.CompanyName);
			AssertEquals("Postcode should not have empty value.", "4010", sendingParty.Postcode);
			AssertEquals("Street Name should not have empty value.", "10 HUTCHESON STREET", sendingParty.AddressLine1);
			AssertEquals("City should not have empty value.", "", sendingParty.City);
			AssertEquals("Country should not have empty value.", "AU", sendingParty.Country.Code);
			AssertNoMessageError("Company Name should have no error message.", sendingParty.CompanyNameInfo, "Sending Party party name and address information is required.");

			sendingParty.CompanyName = ZString.Empty;
			sendingParty.AddressLine1 = ZString.Empty;
			sendingParty.City = ZString.Empty;
			sendingParty.State = ZString.Empty;
			sendingParty.Country.Code = ZString.Empty;
			sendingParty.Postcode = ZString.Empty;
			sendingParty.CompanyName = ZString.Empty;
			cctHouseManifest.SendingParty = sendingParty;
			cctHouseManifest.ValidateAllIncludingChildren();

			AssertEquals("Company Name should have empty value.", ZString.Empty, sendingParty.CompanyName);
			AssertEquals("Postcode should have empty value.", ZString.Empty, sendingParty.Postcode);
			AssertEquals("Street Name should have empty value.", ZString.Empty, sendingParty.AddressLine1);
			AssertEquals("City should have empty value.", ZString.Empty, sendingParty.City);
			AssertEquals("Country should have empty value.", ZString.Empty, sendingParty.Country.Code);
			AssertHasMessageError("Company Name should have error message.", sendingParty.CompanyNameInfo, "Sending Party party name and address information is required.");

			var testObjectCreator = new TestObjectCreator(Factory);
			var testCompany = testObjectCreator.CreateNewCompany("BR", "BR", null);
			var testBranch = testObjectCreator.CreateBranch("SSA", "SalvadorBranche", testCompany, null);
			Factory.Save();
			using (testBranch.SetAsTemporaryContext())
			{
				cctHouseManifest = ConfigCctHouseManifest(ref context);
				sendingParty = (Address)cctHouseManifest.SendingParty;

				AssertEquals("Company Name should not have empty value.", "SendingForwarder", sendingParty.CompanyName);
				AssertEquals("Postcode should not have empty value.", "11157802", sendingParty.Postcode);
				AssertEquals("Street Name should not have empty value.", "Av Paulista 291", sendingParty.AddressLine1);
				AssertEquals("City should not have empty value.", "Salvador", sendingParty.City);
				AssertEquals("Country should not have empty value.", "BR", sendingParty.Country.Code);
				AssertNoMessageError("Company Name should have no error message.", sendingParty.CompanyNameInfo, "Sending Party party name and address information is required.");
			}
		}

		public void TestReceivingAgent()
		{
			CommonContext context = null;
			var cctHouseManifest = ConfigCctHouseManifest(ref context);

			var receivingAgent = (Address)cctHouseManifest.ReceivingAgent;

			receivingAgent.CompanyName = ZString.Empty;
			receivingAgent.AddressLine1 = ZString.Empty;
			receivingAgent.City = ZString.Empty;
			receivingAgent.State = ZString.Empty;
			receivingAgent.Country.Code = ZString.Empty;
			receivingAgent.Postcode = ZString.Empty;
			receivingAgent.CompanyName = ZString.Empty;

			cctHouseManifest.ValidateAllIncludingChildren();

			cctHouseManifest.SendingParty = receivingAgent;

			AssertEquals("Company Name should have empty value.", ZString.Empty, receivingAgent.CompanyName);
			AssertEquals("Postcode should have empty value.", ZString.Empty, receivingAgent.Postcode);
			AssertEquals("Street Name should have empty value.", ZString.Empty, receivingAgent.AddressLine1);
			AssertEquals("City should have empty value.", ZString.Empty, receivingAgent.City);
			AssertEquals("Country should have empty value.", ZString.Empty, receivingAgent.Country.Code);
			AssertEquals("Tax Number should have empty value.", ZString.Empty, receivingAgent.TaxNumber);

			AssertHasMessageError("Company Name should have error message.", receivingAgent.CompanyNameInfo, "Receiving Agent party name and address information is required.");
			AssertHasMessageError("Tax Number should have error message.", receivingAgent.TaxNumberInfo, "CNPJ is required for CCT messaging.");

			receivingAgent.CompanyName = "Company";
			receivingAgent.Postcode = "00333";
			receivingAgent.AddressLine1 = "Street";
			receivingAgent.City = "SAO PAULO";
			receivingAgent.Country.Code = "BR";
			receivingAgent.TaxNumber = "159753684581460";

			cctHouseManifest.ValidateAllIncludingChildren();

			AssertNotEquals("Company Name should not have empty value.", ZString.Empty, receivingAgent.CompanyName);
			AssertNotEquals("Postcode should not have empty value.", ZString.Empty, receivingAgent.Postcode);
			AssertNotEquals("Street Name should not have empty value.", ZString.Empty, receivingAgent.AddressLine1);
			AssertNotEquals("City should not have empty value.", ZString.Empty, receivingAgent.City);
			AssertNotEquals("Country should not have empty value.", ZString.Empty, receivingAgent.Country.Code);
			AssertNotEquals("Tax Number should not have empty value.", ZString.Empty, receivingAgent.TaxNumber);

			AssertNoMessageError("Company Name should have no error message.", receivingAgent.CompanyNameInfo, "Receiving Agent party name and address information is required.");
			AssertNoMessageError("Tax Number should have no error message.", receivingAgent.TaxNumberInfo, "CNPJ is required for CCT messaging.");
		}

		public void TestReceivingAgentCnpj()
		{
			CommonContext context = null;
			var cctHouseManifest = ConfigCctHouseManifest(ref context);

			cctHouseManifest.ReceivingAgent.TaxNumber = ZString.Empty;

			AssertHasMessageError(((Address)cctHouseManifest.ReceivingAgent).TaxNumberInfo, "CNPJ is required for CCT messaging.");
			Assert("CNPJ is required for CCT messaging", cctHouseManifest.ReceivingAgent.TaxNumber.IsEmpty);

			cctHouseManifest.ReceivingAgent.TaxNumber = "010101010101";

			AssertNoMessageError(((Address)cctHouseManifest.ReceivingAgent).TaxNumberInfo, "CNPJ is required for CCT messaging.");
		}

		public void TestMawb()
		{
			CommonContext context = null;
			var cctHouseManifest = ConfigCctHouseManifest(ref context);

			cctHouseManifest.Mawb = ZString.Empty;

			AssertHasMessageError(cctHouseManifest.MawbInfo,
				"Master Air Waybill Number must be entered to use CCT House Manifest");
			Assert("Master Air Waybill number is required in CCT House Manifest", cctHouseManifest.Mawb.IsEmpty);

			cctHouseManifest.Mawb = "215-98757411";

			AssertNoMessageError(cctHouseManifest.MawbInfo, "Master Air Waybill Number must be entered to use CCT House Manifest");
		}

		public void TestConsolNumber()
		{
			CommonContext context = null;
			var cctHouseManifest = ConfigCctHouseManifest(ref context);

			cctHouseManifest.ConsolNumber = ZString.Empty;

			AssertHasMessageError(cctHouseManifest.ConsolNumberInfo, "Consol Number is required in CCT House Manifest");
			Assert("Consol Number is required in CCT House Manifest", cctHouseManifest.ConsolNumber.IsEmpty);

			cctHouseManifest.ConsolNumber = "120159875";

			AssertNoMessageError(cctHouseManifest.ConsolNumberInfo, "Consol Number is required in CCT House Manifest");
		}

		public void TestPacks()
		{
			CommonContext context = null;
			var cctHouseManifest = ConfigCctHouseManifest(ref context);

			cctHouseManifest.Packs = 0;

			AssertHasMessageError(cctHouseManifest.PacksInfo, "Packs number is required in CCT House Manifest");
			Assert("Packs number is required in CCT House Manifest", cctHouseManifest.Packs.IsEmpty);

			cctHouseManifest.Packs = 159;

			AssertNoMessageError(cctHouseManifest.PacksInfo, "Packs number is required in CCT House Manifest");
		}

		public void TestWeight()
		{
			CommonContext context = null;
			var cctHouseManifest = ConfigCctHouseManifest(ref context);

			cctHouseManifest.Weight.Value = 0;

			AssertHasMessageError(((Measurement)cctHouseManifest.Weight).ValueInfo, "Weight is required in CCT House Manifest");
			Assert("Weight is required in CCT House Manifest", cctHouseManifest.Weight.Value.IsEmpty);

			cctHouseManifest.Weight.Value = 1504;

			AssertNoMessageError(((Measurement)cctHouseManifest.Weight).ValueInfo, "Weight is required in CCT House Manifest");
		}

		public void TestAirportOrigin()
		{
			CommonContext context = null;
			var cctHouseManifest = ConfigCctHouseManifest(ref context);

			cctHouseManifest.AirportOfDeparture.Code = ZString.Empty;

			AssertHasMessageError(((Unloco)cctHouseManifest.AirportOfDeparture).CodeInfo, "Airport Of Departure is required for Shipments of CCT House Manifest");
			Assert("Airport Of Departure is required for Shipments of CCT House Manifest", ((Unloco)cctHouseManifest.AirportOfDeparture).Code.IsEmpty);

			cctHouseManifest.AirportOfDeparture.Code = "AUSYD";

			AssertNoMessageError(((Unloco)cctHouseManifest.AirportOfDeparture).CodeInfo, "Airport Of Departure is required for Shipments of CCT House Manifest");
		}

		public void TestAirportDestination()
		{
			CommonContext context = null;
			var cctHouseManifest = ConfigCctHouseManifest(ref context);

			cctHouseManifest.AirportOfDestination.Code = ZString.Empty;

			AssertHasMessageError(((Unloco)cctHouseManifest.AirportOfDestination).CodeInfo, "Airport Of Destination is required for Shipments of CCT House Manifest");
			Assert("Airport Of Destination is required for Shipments of CCT House Manifest", ((Unloco)cctHouseManifest.AirportOfDestination).Code.IsEmpty);

			cctHouseManifest.AirportOfDestination.Code = "BRSAO";

			AssertNoMessageError(((Unloco)cctHouseManifest.AirportOfDestination).CodeInfo, "Airport Of Destination is required for Shipments of CCT House Manifest");
		}

		public void TestShipmentNumber()
		{
			CommonContext context = null;
			var cctHouseManifest = ConfigCctHouseManifest(ref context, true);

			cctHouseManifest.Shipments.First().ShipmentNumber = ZString.Empty;

			AssertHasMessageError(cctHouseManifest.Shipments.First().ShipmentNumberInfo, "Shipment Number is required for Shipments of CCT House Manifest");
			Assert("Shipment Number is required for Shipments of CCT House Manifest", cctHouseManifest.Shipments.First().ShipmentNumber.IsEmpty);

			cctHouseManifest.Shipments.First().ShipmentNumber = "1548456";

			AssertNoMessageError(cctHouseManifest.Shipments.First().ShipmentNumberInfo, "Shipment Number is required for Shipments of CCT House Manifest");
		}

		public void TestShipmentHawb()
		{
			CommonContext context = null;
			var cctHouseManifest = ConfigCctHouseManifest(ref context, true);

			cctHouseManifest.Shipments.First().Hawb = ZString.Empty;

			AssertHasMessageError(cctHouseManifest.Shipments.First().HawbInfo, "House Air Waybill number is required for Shipments of CCT House Manifest");
			Assert("House Air Waybill number is required for Shipments of CCT House Manifest", cctHouseManifest.Shipments.First().Hawb.IsEmpty);

			cctHouseManifest.Shipments.First().Hawb = "945155";

			AssertNoMessageError(cctHouseManifest.Shipments.First().HawbInfo, "House Air Waybill number is required for Shipments of CCT House Manifest");
		}

		public void TestShipmentAirportOrigin()
		{
			CommonContext context = null;
			var cctHouseManifest = ConfigCctHouseManifest(ref context, true);

			cctHouseManifest.Shipments.First().Origin.Code = ZString.Empty;

			AssertHasMessageError(((Unloco)cctHouseManifest.Shipments.First().Origin).CodeInfo, "Origin is required for Shipments of CCT House Manifest");
			Assert("Origin is required for Shipments of CCT House Manifest", ((Unloco)cctHouseManifest.Shipments.First().Origin).Code.IsEmpty);

			cctHouseManifest.Shipments.First().Origin.Code = "AUSYD";

			AssertNoMessageError(((Unloco)cctHouseManifest.Shipments.First().Origin).CodeInfo, "Origin is required for Shipments of CCT House Manifest");
		}

		public void TestShipmentAirportDestination()
		{
			CommonContext context = null;
			var cctHouseManifest = ConfigCctHouseManifest(ref context, true);

			cctHouseManifest.Shipments.First().Destination.Code = ZString.Empty;

			AssertHasMessageError(((Unloco)cctHouseManifest.Shipments.First().Destination).CodeInfo, "Destination is required for Shipments of CCT House Manifest");
			Assert("Destination is required for Shipments of CCT House Manifest", ((Unloco)cctHouseManifest.Shipments.First().Destination).Code.IsEmpty);

			cctHouseManifest.Shipments.First().Destination.Code = "BRSAO";

			AssertNoMessageError(((Unloco)cctHouseManifest.Shipments.First().Destination).CodeInfo, "Destination is required for Shipments of CCT House Manifest");
		}

		public void TestShipmentPacks()
		{
			CommonContext context = null;
			var cctHouseManifest = ConfigCctHouseManifest(ref context, true);

			cctHouseManifest.Shipments.First().Packs = 0;

			AssertHasMessageError(cctHouseManifest.Shipments.First().PacksInfo, "Packs number is required for Shipments of CCT House Manifest");
			Assert("Packs number is required for Shipments of CCT House Manifest", cctHouseManifest.Shipments.First().Packs.IsEmpty || cctHouseManifest.Shipments.First().Packs == 0);

			cctHouseManifest.Shipments.First().Packs = 3;

			AssertNoMessageError(cctHouseManifest.Shipments.First().PacksInfo, "Packs number is required for Shipments of CCT House Manifest");
		}

		public void TestShipmentWeight()
		{
			CommonContext context = null;
			var cctHouseManifest = ConfigCctHouseManifest(ref context, true);

			cctHouseManifest.Shipments.First().Weight.Value = 0;

			AssertHasMessageError(cctHouseManifest.Shipments.First().Weight.ValueInfo, "Weight is required for Shipments of CCT House Manifest");
			Assert("Weight is required for Shipments of CCT House Manifest", cctHouseManifest.Shipments.First().Weight.Value.IsEmpty || cctHouseManifest.Shipments.First().Weight.Value == 0);

			cctHouseManifest.Shipments.First().Weight.Value = 42;

			AssertNoMessageError(cctHouseManifest.Shipments.First().Weight.ValueInfo, "Weight is required for Shipments of CCT House Manifest");
		}

		public void TestShipmentGoodsDescription()
		{
			CommonContext context = null;
			var cctHouseManifest = ConfigCctHouseManifest(ref context, true);

			var shipment = cctHouseManifest.Shipments.First();
			shipment.GoodsDescription = ZString.Empty;

			AssertHasMessageError(shipment.GoodsDescriptionInfo, "Goods Description is required for Shipments of CCT House Manifest");
			Assert("Goods Description is required for Shipments of CCT House Manifest", shipment.GoodsDescription.IsEmpty);

			shipment.GoodsDescription = "Goods 001";

			AssertNoMessageError(shipment.GoodsDescriptionInfo, "Goods Description is required for Shipments of CCT House Manifest");

			var errorMessage =
				"This text contains characters not supported by the Brazil Customs.";

			shipment.GoodsDescription = "abcd 传/傳 片仮名 기윽 0123";
			cctHouseManifest.ValidateAllIncludingChildren();
			AssertHasMessageError("Nature And Qty Of Goods should have error message", shipment.GoodsDescriptionInfo, errorMessage);

			shipment.GoodsDescription = "abcd àãóâ 0123";
			cctHouseManifest.ValidateAllIncludingChildren();
			AssertNoMessageError("Nature And Qty Of Goods should have no error message", shipment.GoodsDescriptionInfo, errorMessage);
		}

		public void TestStatusAndEventNoDocumentData()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, "081001", 12, 25, "goods1");

			AssertStatusAndEvent("Date is empty as no document data", consol, shipment, ZDateTime.Empty,
				"No Advance Cargo Report Messages Have Been Sent.");
		}

		public void TestStatusAndEventNoLogs()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, "081001", 12, 25, "goods1");

			AssertStatusAndEvent("Date is empty as no CCT logs", consol, shipment, ZDateTime.Empty,
				"No Advance Cargo Report Messages Have Been Sent.");
		}

		public void TestStatusAndEventMessageSent()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, "081001", 12, 25, "goods1");

			var documentData = CreateDocumentData(shipment);

			AddLog(shipment, documentData, Events.MessageSent, ZDateTimeOffset.UtcToday.AddHours(-5));

			AssertStatusAndEvent("CCT message has been sent", consol, shipment, ZDateTime.UtcToday.AddHours(-5),
				"Advanced Cargo Report Message Sent to Customs, BR");
		}

		public void TestStatusAndEventInterchangeSent()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, "081001", 12, 25, "goods1");

			var documentData = CreateDocumentData(shipment);

			AddLog(shipment, documentData, Events.InterchangeSent, ZDateTimeOffset.UtcToday.AddHours(-4));

			AssertStatusAndEvent("The interchange message has been sent", consol, shipment,
				ZDateTime.UtcToday.AddHours(-4), "Advanced Cargo Report Interchange Sent by Customs, BR");
		}

		public void TestStatusAndEventInterchangeRejected()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, "081001", 12, 25, "goods1");

			var documentData = CreateDocumentData(shipment);

			AddLog(shipment, documentData, Events.InterchangeSent, ZDateTimeOffset.UtcToday.AddHours(-4));

			AssertStatusAndEvent("The interchange message has been sent", consol, shipment,
				ZDateTime.UtcToday.AddHours(-4), "Advanced Cargo Report Interchange Sent by Customs, BR");

			AddLog(shipment, documentData, Events.InterchangeRejected, ZDateTimeOffset.UtcToday.AddHours(-3));

			AssertStatusAndEvent("CCT message has been rejected", consol, shipment, ZDateTime.UtcToday.AddHours(-3),
				"Advanced Cargo Report Interchange Rejected by Customs, BR");
		}

		public void TestStatusAndEventMessageRejected()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, "081001", 12, 25, "goods1");

			var documentData = CreateDocumentData(shipment);

			AddLog(shipment, documentData, Events.MessageSent, ZDateTimeOffset.UtcToday.AddHours(-5));

			AssertStatusAndEvent("CCT message has been sent", consol, shipment, ZDateTime.UtcToday.AddHours(-5),
				"Advanced Cargo Report Message Sent to Customs, BR");

			AddLog(shipment, documentData, Events.MessageRejected, ZDateTimeOffset.UtcToday.AddHours(-3));

			AssertStatusAndEvent("CCT message has been rejected", consol, shipment, ZDateTime.UtcToday.AddHours(-3),
				"Advanced Cargo Report Message Rejected by Customs, BR");
		}

		public void TestStatusAndEventMessagePendingProcessing()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, "081001", 12, 25, "goods1");

			var documentData = CreateDocumentData(shipment);

			AddLog(shipment, documentData, Events.MessagePendingProcessing, ZDateTimeOffset.UtcToday.AddHours(-2));

			AssertStatusAndEvent("CCT message has been pending processing", consol, shipment,
				ZDateTime.UtcToday.AddHours(-2), "Advanced Cargo Report Message Pending Processing by Customs, BR");
		}

		public void TestStatusAndEventMessageAccepted()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, "081001", 12, 25, "goods1");

			var documentData = CreateDocumentData(shipment);

			AddLog(shipment, documentData, Events.MessageSent, ZDateTimeOffset.UtcToday.AddHours(-5));

			AssertStatusAndEvent("CCT message has been sent", consol, shipment, ZDateTime.UtcToday.AddHours(-5),
				"Advanced Cargo Report Message Sent to Customs, BR");

			AddLog(shipment, documentData, Events.MessageAccepted, ZDateTimeOffset.UtcToday.AddHours(-3));

			AssertStatusAndEvent("CCT message has been accepted", consol, shipment, ZDateTime.UtcToday.AddHours(-3),
				"Advanced Cargo Report Message Accepted by Customs, BR");
		}

		public void TestStatusAndEventMessageWithdrawCancelAcceptedCode()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, "081001", 12, 25, "goods1");

			var documentData = CreateDocumentData(shipment);

			AddLog(shipment, documentData, Events.MessageWithdrawCancelRequest, ZDateTimeOffset.UtcToday.AddHours(-5));

			AssertStatusAndEvent("CCT message has been accepted", consol, shipment, ZDateTime.UtcToday.AddHours(-5),
				"Advanced Cargo Report Message Withdraw/Cancel Request sent to Customs, BR");

			AddLog(shipment, documentData, Events.MessageWithdrawCancelAccepted, ZDateTimeOffset.UtcToday.AddHours(-3));

			AssertStatusAndEvent("CCT message has been accepted", consol, shipment, ZDateTime.UtcToday.AddHours(-3),
				"Advanced Cargo Report Message Withdraw/Cancel Accepted by Customs, BR");
		}

		public void TestStatusAndEventOrderByPostTime()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, "081001", 12, 25, "goods1");

			var documentData = CreateDocumentData(shipment);

			AddLog(shipment, documentData, Events.MessageWithdrawCancelRequest, ZDateTimeOffset.UtcToday.AddHours(-3));

			AssertStatusAndEvent("CCT message has been accepted", consol, shipment, ZDateTime.UtcToday.AddHours(-3),
				"Advanced Cargo Report Message Withdraw/Cancel Request sent to Customs, BR");

			AddLog(shipment, documentData, Events.MessageWithdrawCancelAccepted, ZDateTimeOffset.UtcToday.AddHours(-5));

			AssertStatusAndEvent("CCT message has been accepted", consol, shipment, ZDateTime.UtcToday.AddHours(-5),
				"Advanced Cargo Report Message Withdraw/Cancel Accepted by Customs, BR");
		}

		public void TestShipmentMessageAccepted()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, "102081001", 10, 20, "goods desc");

			var documentData = CreateDocumentData(shipment);

			var build = new CargoControlAndTransitHouseManifestBuilder(consol).Build();
			AssertHasMessageError(build.Shipments.First().StatusInfo, "The CCT House Manifest can only be sent when all the House Bills have been accepted by CCT");

			AddLog(shipment, documentData, Events.MessageSent, ZDateTimeOffset.UtcToday.AddHours(-5));
			build = new CargoControlAndTransitHouseManifestBuilder(consol).Build();
			AssertHasMessageError(build.Shipments.First().StatusInfo, "The CCT House Manifest can only be sent when all the House Bills have been accepted by CCT");

			AddLog(shipment, documentData, Events.MessageAccepted, ZDateTimeOffset.UtcToday.AddHours(1));
			build = new CargoControlAndTransitHouseManifestBuilder(consol).Build();

			AssertNoMessageError(build.Shipments.First().StatusInfo, "The CCT House Manifest can only be sent when all the House Bills have been accepted by CCT");
		}

		#region Shipment Types

		public void TestShipments_WhenCLD_IncludeCLDExcludeSubShipments()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, "081001", 12, 25, "goods1");
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			var subshipment1 = shipment.CoLoadShipments.AddNew();
			PopulateShipment(subshipment1, "081001", 1, 2, "goods1_1");

			var houseManifest = new CargoControlAndTransitHouseManifestBuilder(consol).Build();
			AssertEquals(1, houseManifest.Shipments.Count);
			AssertEquals(shipment.JS_UniqueConsignRef, houseManifest.Shipments.First().ShipmentNumber);
		}

		public void TestShipments_WhenCLDMasterShipmentIsNotInConsol_ExcludeSubShipments()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var shipment = Factory.New<ForwardingShipment>();
			PopulateShipment(shipment, "081001", 12, 25, "goods1");
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			var subshipment1 = shipment.CoLoadShipments.AddNew();
			PopulateShipment(subshipment1, "081001", 1, 2, "goods1_1");
			consol.Shipments.Add(subshipment1);

			var subshipment2 = shipment.CoLoadShipments.AddNew();
			PopulateShipment(subshipment2, "081001", 1, 2, "goods1_2");
			consol.Shipments.Add(subshipment2);

			var houseManifest = new CargoControlAndTransitHouseManifestBuilder(consol).Build();
			AssertEquals(0, houseManifest.Shipments.Count);
		}

		public void TestShipments_WhenCLB_IncludeSubShipmentsExcludeCLB()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, "081001", 12, 25, "goods1");
			shipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;

			var subshipment1 = shipment.CoLoadShipments.AddNew();
			PopulateShipment(subshipment1, "081001", 1, 2, "goods1_1");

			var subshipment2 = shipment.CoLoadShipments.AddNew();
			PopulateShipment(subshipment2, "081001", 1, 2, "goods1_2");

			var houseManifest = new CargoControlAndTransitHouseManifestBuilder(consol).Build();
			AssertEquals(2, houseManifest.Shipments.Count);
			AssertEquals(subshipment1.JS_UniqueConsignRef, houseManifest.Shipments.ElementAt(0).ShipmentNumber);
			AssertEquals(subshipment2.JS_UniqueConsignRef, houseManifest.Shipments.ElementAt(1).ShipmentNumber);
		}

		public void TestShipments_WhenASM_IncludeSubShipmentsExcludeASM()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, "081001", 12, 25, "goods1");
			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			var subshipment1 = shipment.CoLoadShipments.AddNew();
			PopulateShipment(subshipment1, "081001", 1, 2, "goods1_1");

			var subshipment2 = shipment.CoLoadShipments.AddNew();
			PopulateShipment(subshipment2, "081001", 1, 2, "goods1_2");

			var houseManifest = new CargoControlAndTransitHouseManifestBuilder(consol).Build();
			AssertEquals(2, houseManifest.Shipments.Count);
			AssertEquals(subshipment1.JS_UniqueConsignRef, houseManifest.Shipments.ElementAt(0).ShipmentNumber);
			AssertEquals(subshipment2.JS_UniqueConsignRef, houseManifest.Shipments.ElementAt(1).ShipmentNumber);
		}

		#endregion

		public void TestShouldExcludeFromShipments_ASM()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var asmShipment1 = consol.Shipments.AddNew();
			PopulateShipment(asmShipment1, "102081010", 10, 20, "goods desc");
			asmShipment1.JS_UniqueConsignRef = "102081010";
			asmShipment1.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			var asmShipment1Sub1 = Factory.New<ForwardingShipment>();
			PopulateShipment(asmShipment1Sub1, "102081011", 10, 20, "goods desc");
			asmShipment1Sub1.JS_UniqueConsignRef = "102081011";
			asmShipment1Sub1.JS_JS_ColoadMasterShipment = asmShipment1.PK;

			var asmShipment1Sub2 = Factory.New<ForwardingShipment>();
			PopulateShipment(asmShipment1Sub2, "102081012", 10, 20, "goods desc");
			asmShipment1Sub2.JS_UniqueConsignRef = "102081012";
			asmShipment1Sub2.JS_JS_ColoadMasterShipment = asmShipment1.PK;

			var asmShipment1DocumentData = CreateDocumentData(asmShipment1);

			var asmShipment2 = consol.Shipments.AddNew();
			PopulateShipment(asmShipment2, "102081020", 10, 20, "goods desc");
			asmShipment2.JS_UniqueConsignRef = "102081020";
			asmShipment2.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			var asmShipment2Sub1 = Factory.New<ForwardingShipment>();
			PopulateShipment(asmShipment2Sub1, "102081021", 10, 20, "goods desc");
			asmShipment2Sub1.JS_UniqueConsignRef = "102081021";
			asmShipment2Sub1.JS_JS_ColoadMasterShipment = asmShipment2.PK;

			var asmShipment2Sub2 = Factory.New<ForwardingShipment>();
			PopulateShipment(asmShipment2Sub2, "102081022", 10, 20, "goods desc");
			asmShipment2Sub2.JS_UniqueConsignRef = "102081022";
			asmShipment2Sub2.JS_JS_ColoadMasterShipment = asmShipment2.PK;

			var asmShipment2Sub2DocumentData = CreateDocumentData(asmShipment2Sub2);

			var asmShipment3 = consol.Shipments.AddNew();
			PopulateShipment(asmShipment3, "102081030", 10, 20, "goods desc");
			asmShipment3.JS_UniqueConsignRef = "102081030";
			asmShipment3.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			var asmShipment3DocumentData = CreateDocumentData(asmShipment3);

			var asmShipment3Sub1 = Factory.New<ForwardingShipment>();
			PopulateShipment(asmShipment3Sub1, "102081031", 10, 20, "goods desc");
			asmShipment3Sub1.JS_UniqueConsignRef = "102081031";
			asmShipment3Sub1.JS_JS_ColoadMasterShipment = asmShipment3.PK;

			var asmShipment3Sub2 = Factory.New<ForwardingShipment>();
			PopulateShipment(asmShipment3Sub2, "102081032", 10, 20, "goods desc");
			asmShipment3Sub2.JS_UniqueConsignRef = "102081032";
			asmShipment3Sub2.JS_JS_ColoadMasterShipment = asmShipment3.PK;

			var asmShipment4 = consol.Shipments.AddNew();
			PopulateShipment(asmShipment4, "102081040", 10, 20, "goods desc");
			asmShipment4.JS_UniqueConsignRef = "102081040";
			asmShipment4.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			var asmShipment4Sub1 = Factory.New<ForwardingShipment>();
			PopulateShipment(asmShipment4Sub1, "102081041", 10, 20, "goods desc");
			asmShipment4Sub1.JS_UniqueConsignRef = "102081041";
			asmShipment4Sub1.JS_JS_ColoadMasterShipment = asmShipment4.PK;

			var asmShipment4Sub2 = Factory.New<ForwardingShipment>();
			PopulateShipment(asmShipment4Sub2, "102081042", 10, 20, "goods desc");
			asmShipment4Sub2.JS_UniqueConsignRef = "102081042";
			asmShipment4Sub2.JS_JS_ColoadMasterShipment = asmShipment4.PK;

			var asmShipment5 = consol.Shipments.AddNew();
			PopulateShipment(asmShipment5, "102081050", 10, 20, "goods desc");
			asmShipment5.JS_UniqueConsignRef = "102081050";
			asmShipment5.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			var asmShipment5DocumentData = CreateDocumentData(asmShipment5);

			var asmShipment5Sub1 = Factory.New<ForwardingShipment>();
			PopulateShipment(asmShipment5Sub1, "102081051", 10, 20, "goods desc");
			asmShipment5Sub1.JS_UniqueConsignRef = "102081051";
			asmShipment5Sub1.JS_JS_ColoadMasterShipment = asmShipment5.PK;

			var asmShipment5Sub2 = Factory.New<ForwardingShipment>();
			PopulateShipment(asmShipment5Sub2, "102081052", 10, 20, "goods desc");
			asmShipment5Sub2.JS_UniqueConsignRef = "102081052";
			asmShipment5Sub2.JS_JS_ColoadMasterShipment = asmShipment5.PK;

			var asmShipment6 = consol.Shipments.AddNew();
			PopulateShipment(asmShipment6, "102081060", 10, 20, "goods desc");
			asmShipment6.JS_UniqueConsignRef = "102081060";
			asmShipment6.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			var asmShipment6DocumentData = CreateDocumentData(asmShipment6);

			var asmShipment6Sub1 = Factory.New<ForwardingShipment>();
			PopulateShipment(asmShipment6Sub1, "102081061", 10, 20, "goods desc");
			asmShipment6Sub1.JS_UniqueConsignRef = "102081061";
			asmShipment6Sub1.JS_JS_ColoadMasterShipment = asmShipment6.PK;

			var asmShipment6Sub2 = Factory.New<ForwardingShipment>();
			PopulateShipment(asmShipment6Sub2, "102081062", 10, 20, "goods desc");
			asmShipment6Sub2.JS_UniqueConsignRef = "102081062";
			asmShipment6Sub2.JS_JS_ColoadMasterShipment = asmShipment6.PK;

			var asmShipment7 = consol.Shipments.AddNew();
			PopulateShipment(asmShipment7, "102081070", 10, 20, "goods desc");
			asmShipment7.JS_UniqueConsignRef = "102081070";
			asmShipment7.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			var asmShipment7DocumentData = CreateDocumentData(asmShipment7);

			var asmShipment7Sub1 = Factory.New<ForwardingShipment>();
			PopulateShipment(asmShipment7Sub1, "102081071", 10, 20, "goods desc");
			asmShipment7Sub1.JS_UniqueConsignRef = "102081071";
			asmShipment7Sub1.JS_JS_ColoadMasterShipment = asmShipment7.PK;

			var asmShipment7Sub2 = Factory.New<ForwardingShipment>();
			PopulateShipment(asmShipment7Sub2, "102081072", 10, 20, "goods desc");
			asmShipment7Sub2.JS_UniqueConsignRef = "102081072";
			asmShipment7Sub2.JS_JS_ColoadMasterShipment = asmShipment7.PK;

			Factory.Save();

			AddLog(asmShipment1, asmShipment1DocumentData, Events.MessageSent, ZDateTimeOffset.UtcToday.AddHours(-5));
			AddLog(asmShipment1, asmShipment1DocumentData, Events.InterchangeSent, ZDateTimeOffset.UtcToday.AddHours(-4));
			AddLog(asmShipment1, asmShipment1DocumentData, Events.MessageAccepted, ZDateTimeOffset.UtcToday.AddHours(-3));
			AddLog(asmShipment1, asmShipment1DocumentData, Events.MessagePendingProcessing, ZDateTimeOffset.UtcToday.AddHours(-2));

			AddLog(asmShipment2Sub2, asmShipment2Sub2DocumentData, Events.MessageSent, ZDateTimeOffset.UtcToday.AddHours(-5));
			AddLog(asmShipment2Sub2, asmShipment2Sub2DocumentData, Events.InterchangeSent, ZDateTimeOffset.UtcToday.AddHours(-4));
			AddLog(asmShipment2Sub2, asmShipment2Sub2DocumentData, Events.MessageAccepted, ZDateTimeOffset.UtcToday.AddHours(1));

			AddLog(asmShipment3, asmShipment3DocumentData, Events.MessageSent, ZDateTimeOffset.UtcToday.AddHours(-5));
			AddLog(asmShipment3, asmShipment3DocumentData, Events.InterchangeSent, ZDateTimeOffset.UtcToday.AddHours(-4));
			AddLog(asmShipment3, asmShipment3DocumentData, Events.MessageAccepted, ZDateTimeOffset.UtcToday.AddHours(-3));
			AddLog(asmShipment3, asmShipment3DocumentData, Events.MessagePendingProcessing, ZDateTimeOffset.UtcToday.AddHours(-2));
			AddLog(asmShipment3, asmShipment3DocumentData, Events.MessageRejected, ZDateTimeOffset.UtcToday.AddHours(-1));

			AddLog(asmShipment5, asmShipment5DocumentData, Events.MessageSent, ZDateTimeOffset.UtcToday.AddHours(-5));
			AddLog(asmShipment5, asmShipment5DocumentData, Events.InterchangeSent, ZDateTimeOffset.UtcToday.AddHours(-4));
			AddLog(asmShipment5, asmShipment5DocumentData, Events.MessageAccepted, ZDateTimeOffset.UtcToday.AddHours(-3));
			AddLog(asmShipment5, asmShipment5DocumentData, Events.MessagePendingProcessing, ZDateTimeOffset.UtcToday.AddHours(-2));
			AddLog(asmShipment5, asmShipment5DocumentData, Events.MessageRejected, ZDateTimeOffset.UtcToday.AddHours(-1));
			AddLog(asmShipment5, asmShipment5DocumentData, Events.MessageSent, ZDateTimeOffset.UtcToday);

			AddLog(asmShipment6, asmShipment6DocumentData, Events.MessageSent, ZDateTimeOffset.UtcToday.AddHours(-5));
			AddLog(asmShipment6, asmShipment6DocumentData, Events.InterchangeSent, ZDateTimeOffset.UtcToday.AddHours(-4));
			AddLog(asmShipment6, asmShipment6DocumentData, Events.MessageAccepted, ZDateTimeOffset.UtcToday.AddHours(-3));
			AddLog(asmShipment6, asmShipment6DocumentData, Events.MessagePendingProcessing, ZDateTimeOffset.UtcToday.AddHours(-2));
			AddLog(asmShipment6, asmShipment6DocumentData, Events.MessageRejected, ZDateTimeOffset.UtcToday.AddHours(-1));
			AddLog(asmShipment6, asmShipment6DocumentData, Events.MessageSent, ZDateTimeOffset.UtcToday);
			AddLog(asmShipment6, asmShipment6DocumentData, Events.InterchangeRejected, ZDateTimeOffset.UtcToday.AddHours(1));

			AddLog(asmShipment7, asmShipment7DocumentData, Events.MessageSent, ZDateTimeOffset.UtcToday.AddHours(-5));
			AddLog(asmShipment7, asmShipment7DocumentData, Events.InterchangeSent, ZDateTimeOffset.UtcToday.AddHours(-4));
			AddLog(asmShipment7, asmShipment7DocumentData, Events.MessageAccepted, ZDateTimeOffset.UtcToday.AddHours(-3));
			AddLog(asmShipment7, asmShipment7DocumentData, Events.MessagePendingProcessing, ZDateTimeOffset.UtcToday.AddHours(-2));
			AddLog(asmShipment7, asmShipment7DocumentData, Events.MessageWithdrawCancelRequest, ZDateTimeOffset.UtcToday.AddHours(-1));

			var cctHouseManifest = new CargoControlAndTransitHouseManifestBuilder(consol).Build();

			AssertEquals(11, cctHouseManifest.Shipments.Count);

			Assert(cctHouseManifest.Shipments.Any(x => x.ShipmentNumber == "102081010"));

			Assert(cctHouseManifest.Shipments.Any(x => x.ShipmentNumber == "102081021"));
			Assert(cctHouseManifest.Shipments.Any(x => x.ShipmentNumber == "102081022"));

			Assert(cctHouseManifest.Shipments.Any(x => x.ShipmentNumber == "102081031"));
			Assert(cctHouseManifest.Shipments.Any(x => x.ShipmentNumber == "102081032"));

			Assert(cctHouseManifest.Shipments.Any(x => x.ShipmentNumber == "102081041"));
			Assert(cctHouseManifest.Shipments.Any(x => x.ShipmentNumber == "102081042"));

			Assert(cctHouseManifest.Shipments.Any(x => x.ShipmentNumber == "102081050"));

			Assert(cctHouseManifest.Shipments.Any(x => x.ShipmentNumber == "102081061"));
			Assert(cctHouseManifest.Shipments.Any(x => x.ShipmentNumber == "102081062"));

			Assert(cctHouseManifest.Shipments.Any(x => x.ShipmentNumber == "102081070"));
		}

		public void TestLogIsApplicable()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, "081001", 12, 25, "goods1");

			var documentData = CreateDocumentData(shipment);

			AddLog(shipment, documentData, Events.MessageSent, ZDateTimeOffset.UtcToday.AddHours(-5), location: "BRSAO");

			AssertStatusAndEvent("CCT message has been sent", consol, shipment, ZDateTime.UtcToday.AddHours(-5),
				"Advanced Cargo Report Message Sent to Customs, Sao Paulo, SP, BR");

			AddLog(shipment, documentData, Events.MessageAccepted, ZDateTimeOffset.UtcToday.AddHours(-3));

			AssertStatusAndEvent("CCT message has been accepted", consol, shipment, ZDateTime.UtcToday.AddHours(-3),
				"Advanced Cargo Report Message Accepted by Customs, BR");
		}

		#endregion Tests

		#region Assert
		void AssertHeader(ForwardingConsol consol, CargoControlAndTransitHouseManifest cctHouseManifest)
		{
			AssertionHelper.AssertAddressData(GlbBranch.CurrentBranch.OrgProxy.MainAddress, cctHouseManifest.SendingParty);
			AssertionHelper.AssertAddressData(consol.ReceivingForwarderAddress, cctHouseManifest.ReceivingAgent);

			AssertEquals(consol.JK_MasterBillNum, cctHouseManifest.Mawb.Replace("-", ""));
			AssertEquals(consol.JK_UniqueConsignRef, cctHouseManifest.ConsolNumber);
			AssertEquals((ZInt)consol.JK_TotalShipmentQuantity, cctHouseManifest.Packs);
			AssertEquals(consol.JK_TotalShipmentWeight, cctHouseManifest.Weight.Value);
			AssertEquals(consol.JK_TotalShipmentWeightUnit, cctHouseManifest.Weight.Unit.Code);
			AssertEquals(consol.LoadPort.Code, cctHouseManifest.AirportOfDeparture.Code);
			AssertEquals(consol.DischargePort.Code, cctHouseManifest.AirportOfDestination.Code);

			var firstAirLegDischargingInBR = GetFirstAirLegDischargingInBR(consol);
			AssertEquals(firstAirLegDischargingInBR?.DiscPort?.Code, cctHouseManifest.PortOfFirstArrival.Code);
			AssertEquals(firstAirLegDischargingInBR?.LoadPort?.Code, cctHouseManifest.PortOfOrigin.Code);
		}

		void AssertShipments(ForwardingConsol consol, CargoControlAndTransitHouseManifest cctHouseManifest)
		{
			for (int index = 0; index < consol.ShipmentCount; index++)
			{
				var shipmentDetail = cctHouseManifest.Shipments.ElementAt(index);

				AssertEquals(nameof(shipmentDetail.ShipmentNumber), consol.Shipments[index].JS_UniqueConsignRef, shipmentDetail.ShipmentNumber);
				AssertEquals(nameof(shipmentDetail.Hawb), consol.Shipments[index].JS_HouseBill, shipmentDetail.Hawb);
				AssertEquals(nameof(shipmentDetail.Packs), consol.Shipments[index].JS_OuterPacks, shipmentDetail.Packs);
				AssertEquals(nameof(shipmentDetail.Origin.Code), consol.Shipments[index].Origin.Code, shipmentDetail.Origin.Code);
				AssertEquals(nameof(shipmentDetail.Destination.Code), consol.Shipments[index].Destination.Code, shipmentDetail.Destination.Code);
			}
		}

		void AssertStatusAndEvent(ZString message, ForwardingConsol consol, ForwardingShipment shipment, ZDateTime eventDate, ZString status)
		{
			var builder = new CargoControlAndTransitHouseManifestBuilder(consol);
			var cct = builder.Build();
			var cctShipment = cct.Shipments.First();

			AssertEquals(message, eventDate, cctShipment.EventDateTime);
			AssertEquals(message, status, cctShipment.Status);
		}

		#endregion

		#region Implementation

		CargoControlAndTransitHouseManifest ConfigCctHouseManifest(ref CommonContext context, bool withShipment = false)
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			context = new CommonContext(consol.Factory.GetCachedReadOnlyFactory());

			if (withShipment)
			{
				var shipment = consol.Shipments.AddNew();
				PopulateShipment(shipment, "081001", 12, 25, "goods1");
			}

			return new CargoControlAndTransitHouseManifestBuilder(consol).Build();
		}

		Freight.Business.Transport GetFirstAirLegDischargingInBR(ForwardingConsol consol)
		{
			var airLegDischargingInBR = consol.Transports
				.OfType<Freight.Business.Transport>()
				.FirstOrDefault(transport => transport.IsAir &&
					!transport.JW_RL_NKLoadPort.StartsWith(Constants.CountryCodes.Brazil, StringComparison.OrdinalIgnoreCase) &&
					transport.JW_RL_NKDiscPort.StartsWith(Constants.CountryCodes.Brazil, StringComparison.OrdinalIgnoreCase));

			return airLegDischargingInBR;
		}

		void PopulateConsol(ForwardingConsol consol)
		{
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "215-98757411";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BRSAO";
			consol.JK_UniqueConsignRef = "C00001004";

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = "AUMEL";
			carrier.MainAddress.Address1 = "Unit 000";
			carrier.MainAddress.Address2 = "Hypocrea astronidii";
			carrier.MainAddress.City = "Mel";
			carrier.MainAddress.Postcode = "2019";
			carrier.MainAddress.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "ReceivingForwarder";
			receivingForwarder.OH_RL_NKClosestPort = "BRSAO";
			receivingForwarder.MainAddress.Address1 = "Av Paulista 291";
			receivingForwarder.MainAddress.Address2 = "Consolacao";
			receivingForwarder.MainAddress.City = "Sao Paulo";
			receivingForwarder.MainAddress.Postcode = "11157802";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "CN";
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "SendingForwarder";
			sendingForwarder.OH_RL_NKClosestPort = "BRSAO";
			sendingForwarder.MainAddress.Address1 = "Av Paulista 291";
			sendingForwarder.MainAddress.Address2 = "Consolacao";
			sendingForwarder.MainAddress.City = "Salvador";
			sendingForwarder.MainAddress.Postcode = "11157802";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "BR";
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var transportLeg1 = consol.Transports[0];
			transportLeg1.JW_LegOrder = 1;
			transportLeg1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transportLeg1.JW_RL_NKLoadPort = "AUSYD";
			transportLeg1.JW_RL_NKDiscPort = "BRAQA";

			var transportLeg2 = consol.Transports.AddNew();
			transportLeg2.JW_LegOrder = 2;
			transportLeg2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transportLeg2.JW_RL_NKLoadPort = "BRAQA";
			transportLeg2.JW_RL_NKDiscPort = "BRGRU";

			var transportLeg3 = consol.Transports.AddNew();
			transportLeg3.JW_LegOrder = 3;
			transportLeg3.JW_TransportMode = Core.Constants.TransportModes.Air;
			transportLeg3.JW_RL_NKLoadPort = "BRGRU";
			transportLeg3.JW_RL_NKDiscPort = "BRSAO";
			transportLeg3.JW_ETD = ZDateTime.Today;
		}

		void PopulateShipment(ForwardingShipment shipment, ZString hawb, ZDecimal weight, ZInt packs, ZString desc)
		{
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_HouseBill = hawb;
			shipment.JS_ActualWeight = weight;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_GoodsDescription = desc;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_OuterPacks = packs;

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "Consignor";
			shipper.OH_RL_NKClosestPort = "AUSYD";
			shipper.MainAddress.Address1 = "Unit52";
			shipper.MainAddress.Address2 = "Dorcus yamadai";
			shipper.MainAddress.City = "Sydney";
			shipper.MainAddress.Postcode = "2017";
			shipper.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "Consignee";
			consignee.OH_RL_NKClosestPort = "BRSAO";
			consignee.MainAddress.Address1 = "801";
			consignee.MainAddress.Address2 = "Prismognathus delislei";
			consignee.MainAddress.City = "Somewhere";
			consignee.MainAddress.Postcode = "10043";
			consignee.MainAddress.OA_RN_NKCountryCode = "BR";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
		}

		void AddLog(ForwardingShipment shipment, VisualizerDocumentData documentData, Event evenType, ZDateTimeOffset eventDateTime, string location = Core.Constants.CountryCodes.Brazil)
		{
			var paramList = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("MST", DocumentNames.AdvancedCargoReport),
				new KeyValuePair<string, string>("LOC", location),
				new KeyValuePair<string, string>("DEP", "Customs")
			};

			documentData.Logs.AddNew(evenType, eventDateTime, paramList.ToArray());

			Thread.Sleep(1);
			Factory.Save();
		}

		VisualizerDocumentData CreateDocumentData(ForwardingShipment shipment)
		{
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			documentData.JDD_ParentID = shipment.PK;
			documentData.JDD_Name = ShipmentDocumentDataStoreNames.AdvancedCargoReportBR;

			return documentData;
		}

		#endregion	Implementation
	}
}
