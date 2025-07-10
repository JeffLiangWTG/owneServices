using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.US;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Freight.Forwarding.Documents.Testing.AssertionHelper;
using Event = Enterprise.ZArchitecture.Business.Event;

namespace Enterprise.Freight.Forwarding.Documents.US.Testing
{
	sealed class AirCargoAdvanceScreeningBuilderTest : TestCaseWithFactory
	{
		[TestDate(2018, 6, 6)]
		public void TestBuild_WithConsol()
		{
			var shipment = Factory.New<ForwardingShipment>();
			PopulateShipmentWithConsol(shipment);

			Factory.Save();
			var parameters = new DummyDocDataObjectParameters();
			var builder = new AirCargoAdvanceScreeningBuilder(shipment, parameters);
			var acas = builder.Build();

			AssertNotNull(acas);

			AssertAddressData(shipment.Consignor, acas.Shipper);
			AssertAddressData(shipment.Consignee, acas.Consignee);
			AssertAddressData(shipment.NotifyParty, acas.NotifyParty);

			CombineAssertions(() =>
			{
				AssertEquals("HAWB", "HAWB", acas.HAWB);
				AssertEquals("MAWB", "123-45678912", acas.MAWB);
				AssertEquals("Flight Number", "AW1234", acas.FlightNumber);
				AssertEquals("ETA", ZDateTime.Today, acas.ETA);
				AssertEquals("Goods Description", "Pizza Boxes", acas.GoodsDescription);
				AssertEquals("NumberOfPacks", 1, acas.NumberOfPacks);
				AssertEquals("Senders ACAS Code", "123", acas.SendersAcasCode);
				AssertEquals("Consol Type", "AGT", acas.ConsolType.Code);
			});
		}

		[TestDate(2018, 6, 6)]
		public void TestBuild_WithoutConsol()
		{
			var shipment = Factory.New<ForwardingShipment>();
			PopulateShipmentWithoutConsol(shipment);

			Factory.Save();
			var parameters = new DummyDocDataObjectParameters();
			var builder = new AirCargoAdvanceScreeningBuilder(shipment, parameters);
			var acas = builder.Build();

			AssertNotNull(acas);

			AssertAddressData(shipment.Consignor, acas.Shipper);
			AssertAddressData(shipment.Consignee, acas.Consignee);
			AssertAddressData(shipment.NotifyParty, acas.NotifyParty);

			CombineAssertions(() =>
			{
				AssertEquals("HAWB", "HAWB", acas.HAWB);
				AssertEquals("MAWB", ZString.Empty, acas.MAWB);
				AssertEquals("Flight Number", "QQ4321", acas.FlightNumber);
				AssertEquals("ETA", ZDateTime.Today.AddDays(3), acas.ETA);
				AssertEquals("Goods Description", "Pizza Boxes", acas.GoodsDescription);
				AssertEquals("NumberOfPacks", 1, acas.NumberOfPacks);
				AssertEquals("Senders ACAS Code", "123", acas.SendersAcasCode);
				AssertEquals("Consol Type", "", acas.ConsolType.Code);
			});

			AssertNotNull(acas);
		}

		public void TestBuild_Validation()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var parameters = new DummyDocDataObjectParameters();
			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();

			AssertHasMessageError(acas.HAWBInfo, "Either HAWB Number or Consol Number is required.");
			AssertHasMessageError(acas.ConsolNumberInfo, "Either HAWB Number or Consol Number is required.");

			AssertHasMessageError(acas.GoodsDescriptionInfo, "Goods Description is required.");
			AssertHasMessageError(acas.NumberOfPacksInfo, "Packs are required.");
			AssertHasMessageError(acas.Weight.ValueInfo, "Weight is required.");

			var populatedShipment = Factory.New<ForwardingShipment>();
			PopulateShipment(populatedShipment);

			parameters = new DummyDocDataObjectParameters();
			var populatedAcas = new AirCargoAdvanceScreeningBuilder(populatedShipment, parameters).Build();

			AssertNoMessageError(populatedAcas.HAWBInfo, "Either HAWB Number or Consol Number is required.");
			AssertNoMessageError(populatedAcas.ConsolNumberInfo, "Either HAWB Number or Consol Number is required.");

			AssertNoMessageError(populatedAcas.GoodsDescriptionInfo, "Goods Description is required.");
			AssertNoMessageError(populatedAcas.NumberOfPacksInfo, "Packs are required.");
			AssertNoMessageError(populatedAcas.Weight.ValueInfo, "Weight is required.");
		}

		public void TestBuild_ValidationMAWB()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "12345678911";

			Factory.Save();

			var parameters = new DummyDocDataObjectParameters();
			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();
			AssertHasWarning(acas.MAWBInfo, "Invalid check digit. The last digit should be '6'");

			consol.JK_MasterBillNum = "12345678916";

			Factory.Save();

			parameters = new DummyDocDataObjectParameters();
			acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();
			AssertNoWarnings(acas.MAWBInfo);
		}

		public void TestBuild_ValidationFirms()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "001";

			var ctoAddress = Factory.New<OrgHeader>();
			ctoAddress.OH_FullName = "CTO";
			ctoAddress.OH_RL_NKClosestPort = "USLAX";
			ctoAddress.MainAddress.Address1 = "House 16777214";
			ctoAddress.MainAddress.Address2 = "Coelosis inermis";
			ctoAddress.MainAddress.City = "The Big City";
			ctoAddress.MainAddress.Postcode = "1234";
			ctoAddress.MainAddress.OA_RN_NKCountryCode = "US";
			ctoAddress.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "123", "US");
			consol.JK_OA_ArrivalCTOAddress = ctoAddress.MainAddress.PK;

			Factory.Save();

			var parameters = new DummyDocDataObjectParameters();
			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();
			AssertNoMessageError(acas.CTO.CompanyNameInfo, "CTO FIRMS code is required when CTO is entered.");

			shipment = Factory.New<ForwardingShipment>();
			consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "002";

			ctoAddress = Factory.New<OrgHeader>();
			ctoAddress.OH_FullName = "CTO";
			ctoAddress.OH_RL_NKClosestPort = "USLAX";
			ctoAddress.MainAddress.Address1 = "House 16777214";
			ctoAddress.MainAddress.Address2 = "Coelosis inermis";
			ctoAddress.MainAddress.City = "The Big City";
			ctoAddress.MainAddress.Postcode = "1234";
			ctoAddress.MainAddress.OA_RN_NKCountryCode = "US";
			consol.JK_OA_ArrivalCTOAddress = ctoAddress.MainAddress.PK;

			Factory.Save();

			parameters = new DummyDocDataObjectParameters();
			acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();
			AssertHasMessageError(acas.CTO.CompanyNameInfo, "CTO FIRMS code is required when CTO is entered.");
		}

		public void TestBuild_IfConsolIsDirectRequireMAWB()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			shipment.JS_HouseBill = "HAWB";

			var parameters = new DummyDocDataObjectParameters();
			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();

			AssertHasMessageError(acas.MAWBInfo, "MAWB number is required for direct consolidations.");
			Assert("HAWB should not be set if consolType is DRT", acas.HAWB.IsEmpty);

			consol.JK_MasterBillNum = "12345678916";
			Factory.Save();

			acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();

			AssertNoMessageError(acas.MAWBInfo, "MAWB number is required for direct consolidations.");
		}

		public void TestBuild_IfConsolIsNotDirectRequireHAWB()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			consol.JK_UniqueConsignRef = "CS1234567";
			consol.JK_AgentType = Core.Constants.AgentType.Agent;

			var parameters = new DummyDocDataObjectParameters();
			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();

			AssertHasMessageError(acas.HAWBInfo, "HAWB number is required for non-direct consolidations.");

			shipment.JS_HouseBill = "HAWB";
			Factory.Save();

			acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();

			AssertNoMessageError(acas.HAWBInfo, "HAWB number is required for non-direct consolidations.");
		}

		public void TestBuild_ValidationSendersAcasCode_NotEnteredAssertsError()
		{
			var shipment = Factory.New<ForwardingShipment>();
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.RemoveAll();

			var parameters = new DummyDocDataObjectParameters();
			var builder = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();

			AssertHasMessageError(builder.SendersAcasCodeInfo, "This code is required for ACAS messaging. Raise an eRequest to register your interest. Once provided by WTG, enter the code against the Branch or Company Organization Proxy > Config > Registration Numbers/Codes tab using Type = US ACA.");
		}

		public void TestBuild_ValidationSendersAcasCode_EnteredCorrectlyNotAssertsError()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var parameters = new DummyDocDataObjectParameters();
			var builder = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();

			AssertNoNotifications(builder.SendersAcasCodeInfo);
		}

		public void TestBuild_NotifyPartyCode()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "888", "US");

			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var parameters = new DummyDocDataObjectParameters();
			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();
			AssertEquals("888", acas.NotifyPartysAcasCode);

			notifyParty.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ACASOriginatorCode, "111", "US");
			acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();
			AssertEquals("111", acas.NotifyPartysAcasCode);
		}

		public void TestBuild_CorrectAirLegDischarchingInUS()
		{
			var today = ZDateTime.Today;

			var shipment = Factory.New<ForwardingShipment>();
			var transport1 = shipment.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_RL_NKDiscPort = "USJFK";
			transport1.JW_ETD = today;
			transport1.JW_VoyageFlight = "A1234A";

			var transport2 = shipment.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport2.JW_RL_NKDiscPort = "USLAX";
			transport2.JW_ETD = today.AddDays(1);
			transport2.JW_VoyageFlight = "B1234B";

			var transport3 = shipment.Transports.AddNew();
			transport3.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport3.JW_RL_NKDiscPort = "USLDQ";
			transport3.JW_ETD = today.AddDays(3);
			transport3.JW_VoyageFlight = "C1234C";

			var transport4 = shipment.Transports.AddNew();
			transport4.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport4.JW_RL_NKDiscPort = "USLET";
			transport4.JW_ETD = today.AddDays(4);
			transport4.JW_VoyageFlight = "D1234D";

			Factory.Save();

			var parameters = new DummyDocDataObjectParameters();
			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();
			AssertEquals("A1234A", acas.FlightNumber);
		}

		public void TestBuild_AirLegFallbackToLegOrder()
		{
			var today = ZDateTime.Today;

			var shipment = Factory.New<ForwardingShipment>();
			var transport1 = shipment.Transports.AddNew();
			transport1.JW_IsLinked = false;
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_RL_NKDiscPort = "USJFK";
			transport1.JW_LegOrder = 2;
			transport1.JW_VoyageFlight = "A1234A";

			var transport2 = shipment.Transports.AddNew();
			transport2.JW_IsLinked = false;
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport2.JW_RL_NKDiscPort = "USLAX";
			transport2.JW_LegOrder = 3;
			transport2.JW_VoyageFlight = "B1234B";

			var transport3 = shipment.Transports.AddNew();
			transport3.JW_IsLinked = false;
			transport3.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport3.JW_RL_NKDiscPort = "USLDQ";
			transport3.JW_LegOrder = 1;
			transport3.JW_VoyageFlight = "C1234C";

			var transport4 = shipment.Transports.AddNew();
			transport4.JW_IsLinked = false;
			transport4.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport4.JW_RL_NKDiscPort = "USLET";
			transport4.JW_LegOrder = 5;
			transport4.JW_VoyageFlight = "D1234D";

			Factory.Save();

			var parameters = new DummyDocDataObjectParameters();
			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();
			AssertEquals("C1234C", acas.FlightNumber);
		}

		public void TestBuild_GoodsDescriptionLengthValidation()
		{
			var today = ZDateTime.Today;

			var shipment = Factory.New<ForwardingShipment>();

			var description = shipment.Notes.AddNew();
			description.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			description.ST_NoteDataAsText = @"We need a lot of characters Bacon ipsum dolor amet tongue meatloaf turkey prosciutto filet mignon. 
Drumstick ball tip boudin, ham fatback rump burgdoggen prosciutto. Andouille tenderloin bresaola alcatra doner. 
Beef ham hock alcatra, short ribs pork belly landjaeger swine. Chuck hamburger jowl alcatra brisket. 
Filet mignon boudin salami landjaeger, meatloaf ball tip buffalo cow meatball shank ribeye beef ribs. 
Corned beef turkey tongue cow ball tip. Tongue biltong landjaeger turducken, t-bone capicola shank drumstick. 
Shoulder turducken porchetta sausage rump tenderloin.";

			Factory.Save();

			var parameters = new DummyDocDataObjectParameters();
			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();
			AssertHasMessageError(acas.GoodsDescriptionInfo, "Goods Description has a limit of 490 characters.");

			shipment = Factory.New<ForwardingShipment>();
			description = shipment.Notes.AddNew();
			description.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			description.ST_NoteDataAsText = "An appropriate length description";

			Factory.Save();

			parameters = new DummyDocDataObjectParameters();
			acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();
			AssertNoMessageError(acas.GoodsDescriptionInfo, "Goods Description has a limit of 490 characters.");

			var exactly490Characters = @"We need lot of characters Bacon ipsum dolor amet tongue meatloaf turkey prosciutto filet mignon. 
Drumstick ball tip boudin, ham fatback rump burgdoggen prosciutto. Andouille tenderloin bresaola alcatra doner.
Beef ham hock alcatra, short ribs pork belly landjaeger swine. Chuck hamburger jowl alcatra brisket.
Filet mignon boudin salami landjaeger, meatloaf ball tip buffalo cow meatball shank ribeye beef ribs.
Corned to beef turkey tongue cow ball tip. Tongue biltong landjaeger lamb";

			AssertEquals("Precondition: string is 490 characters", 490, exactly490Characters.Length);

			shipment = Factory.New<ForwardingShipment>();
			description = shipment.Notes.AddNew();
			description.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			description.ST_NoteDataAsText = exactly490Characters;

			Factory.Save();

			parameters = new DummyDocDataObjectParameters();
			acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();
			AssertNoMessageError(acas.GoodsDescriptionInfo, "Goods Description has a limit of 490 characters.");

			var over490Characters = exactly490Characters + "a";

			shipment = Factory.New<ForwardingShipment>();
			description = shipment.Notes.AddNew();
			description.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			description.ST_NoteDataAsText = over490Characters;

			Factory.Save();

			parameters = new DummyDocDataObjectParameters();
			acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();
			AssertHasMessageError(acas.GoodsDescriptionInfo, "Goods Description has a limit of 490 characters.");
		}

		public void TestBuild_NoStatus()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var parameters = new DummyDocDataObjectParameters();
			parameters.LogProvider = Factory.New<DummyEnterpriseBusinessObject>();

			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();
			AssertEquals("State: None", AcasState.None, acas.State);
		}

		public void TestBuild_OriginalSent()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var parameters = new DummyDocDataObjectParameters();
			parameters.LogProvider = Factory.New<DummyEnterpriseBusinessObject>();
			CreateLog(parameters.LogProvider, Events.MessageSent);

			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();
			AssertEquals("State: Original Sent", AcasState.OriginalSent, acas.State);

			AssertEquals("DisplayInformation", @"The message previously sent has not yet received a response.
Please wait for a response before resending.", acas.DisplayInformation);
		}

		public void TestBuild_Acknowledgement_Required()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var parameters = new DummyDocDataObjectParameters();
			parameters.LogProvider = Factory.New<DummyEnterpriseBusinessObject>();
			var logProvider = parameters.LogProvider;

			CreateLog(logProvider, Events.MessageSent);
			CreateLog(logProvider, Events.InterchangeSent);
			CreateLog(logProvider, Events.Held, "6H");

			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();
			AssertEquals("State: Acknowledgement Required", AcasState.AcknowledgementRequired, acas.State);

			AssertEquals("DisplayInformation", @"The latest response from CBP is ""On Hold"" (6H, 7H or 8H).
Use the 'Send Message' option to send an Acknowledgement message.", acas.DisplayInformation);
		}

		public void TestBuild_Acknowledgement_AcknowledgementSentState()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var parameters = new DummyDocDataObjectParameters();
			parameters.LogProvider = Factory.New<DummyEnterpriseBusinessObject>();

			var logProvider = parameters.LogProvider;
			CreateLog(logProvider, Events.MessageSent);
			CreateLog(logProvider, Events.InterchangeSent);
			CreateLog(logProvider, Events.Held, "6H");
			CreateLog(logProvider, Events.MessageSent);

			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();
			AssertEquals("State: AcknowledgementSent", AcasState.AcknowledgementSent, acas.State);

			AssertEquals("DisplayInformation", @"An Acknowledgement message has been sent and has not received a response.
Please wait for a response before further action.", acas.DisplayInformation);
		}

		public void TestBuild_Acknowledgement_AmendmentRequiredAfter7J()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var parameters = new DummyDocDataObjectParameters();
			parameters.LogProvider = Factory.New<DummyEnterpriseBusinessObject>();
			CreateLog(parameters.LogProvider, Events.Held, "7J");

			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();
			AssertEquals("DisplayInformation: Amendment Required", @"The latest status received from US Customs is Selectee Data Issue Hold.
Amend the data and resend the message to resolve the hold.", acas.DisplayInformation);
			AssertEquals("State: Amentment Required", AcasState.AmendmentRequired, acas.State);
			AssertEquals("Can send message", true, acas.CanSendMessage);

			shipment = Factory.New<ForwardingShipment>();

			parameters = new DummyDocDataObjectParameters();
			parameters.LogProvider = Factory.New<DummyEnterpriseBusinessObject>();
			CreateLog(parameters.LogProvider, Events.Held, "8J");

			acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();

			AssertEquals("DisplayInformation: Hold Currently in Place", @"This Shipment is currently on ""Hold Currently in Place"" with CBP, per the latest response.
Wait for a 6I, 7I or 8I ""Hold Removed"" response before resending the message.", acas.DisplayInformation);
			AssertEquals("Cant send message", false, acas.CanSendMessage);
		}

		public void TestBuild_Acknowledgement_AmendmentSentAfter7J()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var parameters = new DummyDocDataObjectParameters();
			parameters.LogProvider = Factory.New<DummyEnterpriseBusinessObject>();
			var logProvider = parameters.LogProvider;

			CreateLog(logProvider, Events.Held, "7J");
			CreateLog(logProvider, Events.MessageSent);

			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();
			AssertEquals("DisplayInformation: Amendment Sent", @"An amendment message in response to Selectee Data Issues has been sent.
Please wait for a response before further action.", acas.DisplayInformation);
			AssertEquals("State: Amentment Sent", AcasState.AmendmentSent, acas.State);
			AssertEquals("Can send message", false, acas.CanSendMessage);
		}

		public void TestBuild_Acknowledgement_AmendmentRequiredAfter7H()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var parameters = new DummyDocDataObjectParameters();
			parameters.LogProvider = Factory.New<DummyEnterpriseBusinessObject>();
			var logProvider = parameters.LogProvider;

			CreateLog(logProvider, Events.Held, "7H");
			CreateLog(logProvider, Events.MessageSent);
			CreateLog(logProvider, Events.InterchangeSent);

			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();
			AssertEquals("DisplayInformation: Amendment Required", @"The latest status received from US Customs is Selectee Data Issue Hold.
Amend the data and resend the message to resolve the hold.", acas.DisplayInformation);
		}

		public void TestBuild_Acknowledgement_AmendmentSentAfter7H()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var parameters = new DummyDocDataObjectParameters();
			parameters.LogProvider = Factory.New<DummyEnterpriseBusinessObject>();
			var logProvider = parameters.LogProvider;

			CreateLog(logProvider, Events.Held, "7H");
			CreateLog(logProvider, Events.MessageSent);
			CreateLog(logProvider, Events.InterchangeSent);
			CreateLog(logProvider, Events.MessageSent);

			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();
			AssertEquals("DisplayInformation: Amendment Sent", @"An amendment message in response to Selectee Data Issues has been sent.
Please wait for a response before further action.", acas.DisplayInformation);
			AssertEquals("State: None", AcasState.AmendmentSent, acas.State);
		}

		public void TestBuild_Acknowledgement_AssessmentOngoing()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var parameters = new DummyDocDataObjectParameters();
			parameters.LogProvider = Factory.New<DummyEnterpriseBusinessObject>();
			var logProvider = parameters.LogProvider;

			CreateLog(logProvider, Events.MessagePendingProcessing);

			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();

			AssertEquals("State: Assessment Ongoing", AcasState.AssessmentOngoing, acas.State);
			AssertEquals("DisplayInformation", @"A risk assessment is currently being conducted by CBP.
Please wait for an additional response before further action.", acas.DisplayInformation);
		}

		public void TestBuild_Acknowledgement_AssessmentComplete()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var parameters = new DummyDocDataObjectParameters();
			parameters.LogProvider = Factory.New<DummyEnterpriseBusinessObject>();
			var logProvider = parameters.LogProvider;

			CreateLog(logProvider, Events.ClearanceCompleted, "SF");

			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();

			AssertEquals("State: Assessment Complete", AcasState.AssessmentComplete, acas.State);
			AssertEquals("DisplayInformation", @"This Shipment has been approved to be uplifted/loaded on a flight to United States.", acas.DisplayInformation);
		}

		public void TestBuild_Acknowledgement_HoldInPlace()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var parameters = new DummyDocDataObjectParameters();
			parameters.LogProvider = Factory.New<DummyEnterpriseBusinessObject>();
			CreateLog(parameters.LogProvider, Events.Held, "6J");

			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();

			AssertEquals("State: Hold In Place", AcasState.HoldInPlace, acas.State);
			AssertEquals("DisplayInformation: Hold Currently in Place", @"This Shipment is currently on ""Hold Currently in Place"" with CBP, per the latest response.
Wait for a 6I, 7I or 8I ""Hold Removed"" response before resending the message.", acas.DisplayInformation);
		}

		public void TestBuild_Acknowledgement_HoldRemovedResponse()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var parameters = new DummyDocDataObjectParameters();
			parameters.LogProvider = Factory.New<DummyEnterpriseBusinessObject>();
			CreateLog(parameters.LogProvider, Events.ClearedHold, "6I");

			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();

			Assert("Can Send Message", acas.CanSendMessage);
			AssertEquals("State: HoldRemoved", AcasState.HoldRemoved, acas.State);
			AssertEquals("DisplayInformation", @"The ""Hold"" status of this Shipment has been removed by CBP.
This Shipment has been approved to be uplifted/loaded on a flight to United States.", acas.DisplayInformation);
		}

		public void TestBuild_Acknowledgement_MostRecentSHLStatus()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var parameters = new DummyDocDataObjectParameters();
			parameters.LogProvider = Factory.New<DummyEnterpriseBusinessObject>();
			CreateLog(parameters.LogProvider, Events.ClearedHold, "6I");
			CreateLog(parameters.LogProvider, Events.ClearedHold, "7I");
			CreateLog(parameters.LogProvider, Events.Held, "8H");

			Factory.Save();

			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();
			Assert("Can Send Message", acas.CanSendMessage);
			AssertEquals("State: AcknowledgementRequired", AcasState.AcknowledgementRequired, acas.State);
		}

		public void TestBuild_Acknowledgement_FallbackOnRecentValidEvent_NoOtherEvents()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var parameters = new DummyDocDataObjectParameters();
			parameters.LogProvider = Factory.New<DummyEnterpriseBusinessObject>();
			CreateLog(parameters.LogProvider, Events.MessageSent);
			CreateLog(parameters.LogProvider, Events.InterchangeRejected);

			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();
			Assert("Can Send Message", acas.CanSendMessage);
			AssertEquals("State: None", AcasState.None, acas.State);
		}

		public void TestBuild_NonAcasLogsAreIgnored()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var parameters = new DummyDocDataObjectParameters();
			parameters.LogProvider = Factory.New<DummyEnterpriseBusinessObject>();
			CreateLog(parameters.LogProvider, Events.MessageSent);
			CreateLog(parameters.LogProvider, Events.InterchangeRejected, messageType: "Not an ACAS Message Type");
			CreateLog(parameters.LogProvider, Events.InterchangeRejected, messageType: "");

			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();
			AssertEquals("Non ACAS logs are ignored. State: Original Sent", AcasState.OriginalSent, acas.State);
			AssertEquals("DisplayInformation", @"The message previously sent has not yet received a response.
Please wait for a response before resending.", acas.DisplayInformation);
		}

		public void TestBuild_AfterIRJ_MostRecentMSNStatus()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var parameters = new DummyDocDataObjectParameters();
			parameters.LogProvider = Factory.New<DummyEnterpriseBusinessObject>();
			CreateLog(parameters.LogProvider, Events.MessageSent);
			CreateLog(parameters.LogProvider, Events.InterchangeRejected);
			CreateLog(parameters.LogProvider, Events.MessageSent);
			CreateLog(parameters.LogProvider, Events.InterchangeSent);

			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();
			AssertEquals("State: Original Sent", AcasState.OriginalSent, acas.State);

			AssertEquals("DisplayInformation", @"The message previously sent has not yet received a response.
Please wait for a response before resending.", acas.DisplayInformation);
		}

		public void TestBuild_NotifyPartyIsConsolSendingAgent_NotifyTypeDefaultsToAGT()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var notifyParty = Factory.New<OrgHeader>();
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var consol = shipment.Consols.AddNew();
			consol.JK_OA_ReceivingForwarderAddress = notifyParty.MainAddress.PK;

			AssertEquals("Shipment notify party should be consol receiving agent", consol.ReceivingForwarderPK, shipment.NotifyParty.PK);

			var parameters = new DummyDocDataObjectParameters();

			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();

			AssertEquals("ACAS notify party type defaults to AGT", PartyTypes.Codes.Agent, acas.NotifyPartyType.Code.ToString());
		}

		public void TestBuild_NotifyPartyIsConsolReceivingAgent_NotifyTypeDefaultsToAGT()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var notifyParty = Factory.New<OrgHeader>();
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var consol = shipment.Consols.AddNew();
			consol.JK_OA_SendingForwarderAddress = notifyParty.MainAddress.PK;

			AssertEquals("Shipment notify party should be consol sending agent", consol.SendingForwarderPK, shipment.NotifyParty.PK);

			var parameters = new DummyDocDataObjectParameters();

			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();

			AssertEquals("ACAS notify party type defaults to AGT", PartyTypes.Codes.Agent, acas.NotifyPartyType.Code.ToString());
		}

		public void TestBuild_NotifyPartyIsConsignor_NotifyTypeDefaultsToSEL()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var notifyParty = Factory.New<OrgHeader>();
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;
			shipment.ConsignorPK = notifyParty.PK;

			var parameters = new DummyDocDataObjectParameters();
			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();

			AssertEquals("ACAS notify party type defaults to SEL", PartyTypes.Codes.SellingParty, acas.NotifyPartyType.Code.ToString());
		}

		public void TestBuild_NotifyPartyIsConsignee_NotifyTypeDefaultsToBUY()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var notifyParty = Factory.New<OrgHeader>();
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;
			shipment.ConsigneePK = notifyParty.PK;

			var parameters = new DummyDocDataObjectParameters();
			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();

			AssertEquals("ACAS notify party type defaults to BUY", PartyTypes.Codes.BuyingParty, acas.NotifyPartyType.Code.ToString());
		}

		public void TestBuild_NotifyPartyDoesNotMatch_NotifyTypeDefaultsToCTC()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var notifyParty = Factory.New<OrgHeader>();
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var parameters = new DummyDocDataObjectParameters();
			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();

			AssertEquals("ACAS notify party type defaults to CTC", PartyTypes.Codes.AdditionalContact, acas.NotifyPartyType.Code.ToString());
		}

		public void TestBuild_NotifyPartyDoesNotExist_NotifyTypeIsEmpty()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var parameters = new DummyDocDataObjectParameters();
			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();

			AssertEquals("ACAS notify party type is empty", "", acas.NotifyPartyType.Code.ToString());
		}

		public void TestLogIsApplicable()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var parameters = new DummyDocDataObjectParameters();
			parameters.LogProvider = Factory.New<DummyEnterpriseBusinessObject>();
			CreateLog(parameters.LogProvider, Events.MessageSent, location: "USJFK");
			CreateLog(parameters.LogProvider, Events.InterchangeRejected, location: "AUSYD");
			CreateLog(parameters.LogProvider, Events.InterchangeRejected, location: "");

			var acas = new AirCargoAdvanceScreeningBuilder(shipment, parameters).Build();
			AssertEquals("Non ACAS logs are ignored. State: Original Sent", AcasState.OriginalSent, acas.State);
			AssertEquals("DisplayInformation", @"The message previously sent has not yet received a response.
Please wait for a response before resending.", acas.DisplayInformation);
		}

		void PopulateShipmentWithConsol(ForwardingShipment shipment)
		{
			PopulateShipment(shipment);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "12345678912";
			consol.JK_AgentType = Core.Constants.AgentType.Agent;

			var transportLeg = consol.Transports.AddNew();
			transportLeg.JW_TransportMode = Core.Constants.TransportModes.Air;
			transportLeg.JW_RL_NKLoadPort = "AUSYD";
			transportLeg.JW_RL_NKDiscPort = "USLAX";
			transportLeg.JW_VoyageFlight = "AW1234";
			transportLeg.JW_ETA = ZDateTime.Today;

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = "AUSYD";
			carrier.MainAddress.Address1 = "Unit 159";
			carrier.MainAddress.Address2 = "Dorcus alcides";
			carrier.MainAddress.City = "Sydney";
			carrier.MainAddress.Postcode = "2015";
			carrier.MainAddress.OA_RN_NKCountryCode = "AU";

			transportLeg.JW_OA_CarrierAddress = carrier.MainAddress.PK;
		}

		void PopulateShipmentWithoutConsol(ForwardingShipment shipment)
		{
			PopulateShipment(shipment);

			var transportLeg = shipment.Transports.AddNew();
			transportLeg.JW_TransportMode = Core.Constants.TransportModes.Air;
			transportLeg.JW_RL_NKDiscPort = "USLAX";
			transportLeg.JW_VoyageFlight = "QQ4321";
			transportLeg.JW_ETA = ZDateTime.Today.AddDays(3);

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = "AUSYD";
			carrier.MainAddress.Address1 = "Unit 222";
			carrier.MainAddress.Address2 = "Macrodorcas davidi";
			carrier.MainAddress.City = "Sydney";
			carrier.MainAddress.Postcode = "2015";
			carrier.MainAddress.OA_RN_NKCountryCode = "AU";

			transportLeg.JW_OA_CarrierAddress = carrier.MainAddress.PK;
		}

		void PopulateShipment(ForwardingShipment shipment)
		{
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_HouseBill = "HAWB";
			shipment.JS_ActualWeight = 123;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_GoodsDescription = "Pizza Boxes";
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_OuterPacks = 1;

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
			consignee.OH_RL_NKClosestPort = "CNNBO";
			consignee.MainAddress.Address1 = "801";
			consignee.MainAddress.Address2 = "Prismognathus delislei";
			consignee.MainAddress.City = "Somewhere";
			consignee.MainAddress.Postcode = "10043";
			consignee.MainAddress.OA_RN_NKCountryCode = "CN";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "Notify Party";
			notifyParty.OH_RL_NKClosestPort = "NZAKL";
			notifyParty.MainAddress.Address1 = "115";
			notifyParty.MainAddress.Address2 = "Coelosis sylvanus";
			notifyParty.MainAddress.City = "Auckland";
			notifyParty.MainAddress.Postcode = "1050";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;
		}

		protected override void SetUp()
		{
			base.SetUp();
			AssertNotNull("Precondition: Proxy of current company is null.", GlbCompany.CurrentCompany.OrgProxy);

			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ACASOriginatorCode, "123", "US");
		}

		protected override void TearDown()
		{
			base.TearDown();

			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.RemoveAll();
		}

		void CreateLog(IStmALogProvider logProvider, Event @event, string reason = "", string messageType = DocumentNames.AdvancedCargoReport, string location = Core.Constants.CountryCodes.UnitedStates)
		{
			var messageTypeParameter = messageType.IsNullOrEmpty()
				? string.Empty
				: $"|MST={messageType}";

			var resParameter = reason.IsNullOrEmpty()
				? string.Empty
				: $"|RES={reason}";

			var locParameter = location.IsNullOrEmpty()
				? string.Empty
				: $"|LOC={location}";

			logProvider?.Logs.CreateOrRecreateEventLog(
				@event,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				$"{messageTypeParameter}{locParameter}{resParameter}");

			Thread.Sleep(1);
			Factory.Save();
		}
	}
}
