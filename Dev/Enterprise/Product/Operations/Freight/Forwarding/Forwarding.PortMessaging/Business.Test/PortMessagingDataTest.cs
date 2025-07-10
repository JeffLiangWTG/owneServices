using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Integration;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business.Testing
{
	abstract class PortMessagingDataTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2012, 09, 25)]
		public void TestProperties()
		{
			var shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_FullName = "shipping line";
			var forwarder = Factory.New<OrgHeader>();
			forwarder.OH_FullName = "shipper";
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			var cfs = Factory.New<OrgHeader>();
			var cto = Factory.New<OrgHeader>();

			forwarder.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "dakosysendercode", Constants.CountryCodes.Germany);
			forwarder.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ZAP, "zapsendercode", Constants.CountryCodes.Germany);
			cfs.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyBerthCode, "dakosysfc", Constants.CountryCodes.Germany);
			cto.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "PART", Constants.CountryCodes.Germany);
			cto.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyBerthCode, "BRT", Constants.CountryCodes.Germany);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_OA_ShippingLineAddress = shippingLine.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = forwarder.MainAddress.PK;
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;
			consol.JK_OA_DepartureCTOAddress = cto.MainAddress.PK;
			consol.JK_MasterBillNum = "billno0000-00112";
			consol.JK_RL_NKDischargePort = "UAIEV";
			consol.JK_BookingReference = "BOOKINGREF";

			var transport = consol.Transports[0];
			transport.JW_JX = CreateNewSailing(vessel, "voyageNo", "DEHAM", "AUSYD", ZDateTime.Today.AddDays(-7), ZDateTime.Today.AddDays(1)).PK;
			transport.JW_Vessel = vessel.RV_FK;
			transport.JW_ETD = ZDateTime.BrettsBirthday;

			var origin1 = consol.Voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "UAODS";
			origin1.JA_Berth = "AAA";
			var origin2 = consol.Voyage.Origins.OfType<VoyageOrigin>().First(o => o.JA_RL_NKPortOfLoading.Equals("DEHAM"));
			origin2.JA_RL_NKPortOfLoading = "DEHAM";
			origin2.JA_Berth = "BBB";

			AssertEquals("Pre-Condition", false, consol.IsCoLoad);

			var consolPortMessaging = new ConsolPortMessagingData(consol);
			consolPortMessaging.ValidateAll();

			AssertEquals(GlbStaff.CurrentUser.GS_FullName, consolPortMessaging.Operator);
			AssertEquals(GlbStaff.CurrentUser.GS_EmailAddress, consolPortMessaging.OperatorEmail);
			AssertEquals(GlbStaff.CurrentUser.GS_WorkPhone, consolPortMessaging.OperatorPhone);
			AssertEquals(GlbStaff.CurrentUser.GS_FaxNum, consolPortMessaging.OperatorFax);
			AssertEquals(ZDateTime.Now, consolPortMessaging.Date);
			AssertEquals("BBB", consolPortMessaging.Berth);
			AssertEquals("shipping line", consolPortMessaging.ShippingLine);
			AssertEquals("dakosysendercode", consolPortMessaging.SenderCode);
			AssertEquals(GlbCompany.CurrentCompany.GC_Name, consolPortMessaging.SenderName);
			AssertEquals("zapsendercode", consolPortMessaging.AccountNo);
			AssertEquals(vessel.RV_Name, consolPortMessaging.VesselName);
			AssertEquals("billno0000", consolPortMessaging.BillNo);
			AssertEquals(ZDateTime.BrettsBirthday, consolPortMessaging.Departure);
			AssertEquals("voyageNo", consolPortMessaging.VoyageNo);
			AssertEquals("UAIEV", consolPortMessaging.Destination);
			AssertEquals("PART", consolPortMessaging.Warehouse);
			AssertEquals("dakosysendercode", consolPortMessaging.ShipperCode);
			AssertEquals("shipper", consolPortMessaging.ShipperName);
			AssertEquals("zapsendercode", consolPortMessaging.ShipperPortAccount);
			AssertEquals("BOOKINGREF", consolPortMessaging.BookingReference);

			origin2.JA_Berth = ZString.Empty;
			origin2.JA_Calc_DepartureCTOAddressOrg = cto.PK;
			AssertEquals("BRT", consolPortMessaging.Berth);

			AssertEquals("Shipping Line - Carrier has errors (ZAP)", true,
				consolPortMessaging.ShippingLineInfo.HasMessageError(
					"The Carrier Code for Hamburg (ZAP) must be populated. Consol -> Organizations -> Carrier -> Details -> Config -> Registration Numbers / Codes -> ZAP Code for DE."));

			AssertEquals("Shipping Line - Carrier has errors (DPC)", true,
				consolPortMessaging.ShippingLineInfo.HasMessageError(
					"The Dakosy Participant Code (DPC) must be populated. Consol -> Organizations -> Carrier -> Details -> Config -> Registration Numbers / Codes -> DPC Code for DE."));

			consol.ShippingLine.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ZAP, "ZZZ", Constants.CountryCodes.Germany);
			consol.ShippingLine.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "DDD", Constants.CountryCodes.Germany);
			consolPortMessaging = new ConsolPortMessagingData(consol);
			consolPortMessaging.ValidateAll();

			AssertEquals("Shipping Line - Carrier not has errors (ZAP)", false,
				consolPortMessaging.ShippingLineInfo.HasMessageError(
					"The Carrier Code for Hamburg (ZAP) must be populated. Consol -> Organizations -> Carrier -> Details -> Config -> Registration Numbers / Codes -> ZAP Code for DE."));

			AssertEquals("Shipping Line - Carrier has errors (DPC)", false,
				consolPortMessaging.ShippingLineInfo.HasMessageError(
					"The Dakosy Participant Code (DPC) must be populated. Consol -> Organizations -> Carrier -> Details -> Config -> Registration Numbers / Codes -> DPC Code for DE."));
		}

		public void TestPropertiesForCoLoad()
		{
			var coLoadWith = Factory.New<OrgHeader>();
			coLoadWith.OH_FullName = "CoLoad Shipping";
			var shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_FullName = "Shipping Line";
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "DEHAM";
			consol.JK_RL_NKDischargePort = "UAIEV";
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "M00000123";
			consol.JK_CoLoadMasterBill = "C00000123";
			consol.JK_BookingReference = "BOOKING1";
			consol.JK_CoLoadBookingReference = "BOOKING2";
			AssertEquals("Pre-Condition", true, consol.IsCoLoad);
			var consolPortMessaging = new ConsolPortMessagingData(consol);

			consolPortMessaging = new ConsolPortMessagingData(consol);
			consolPortMessaging.ValidateAll();
			AssertEquals("Shipping Line - Creditor is empty", true,
						consolPortMessaging.ShippingLineInfo.HasMessageError("Co-Load With must be populated. Consol -> Organizations -> Co-Load With."));

			consol.JK_OA_CreditorAddress = coLoadWith.MainAddress.PK;

			CombineAssertions(() =>
			{
				AssertEquals("ShippingLine", "CoLoad Shipping", consolPortMessaging.ShippingLine);
				AssertEquals("BillNo", "C00000123", consolPortMessaging.BillNo);
				AssertEquals("BookingReference", "BOOKING2", consolPortMessaging.BookingReference);
			});

			consolPortMessaging = new ConsolPortMessagingData(consol);
			consolPortMessaging.ValidateAll();

			AssertEquals("Shipping Line - Creditor is empty", false,
				consolPortMessaging.ShippingLineInfo.HasMessageError("Shipping Line must be populated. Consol -> Organizations -> Co-Load With."));

			AssertEquals("Shipping Line - Creditor has errors (ZAP)", true,
				consolPortMessaging.ShippingLineInfo.HasMessageError(
					"The Carrier Code for Hamburg (ZAP) must be populated. Consol -> Organizations -> Co-Load With -> Details -> Config -> Registration Numbers / Codes -> ZAP Code for DE."));

			AssertEquals("Shipping Line - Carrier has errors (DPC)", true,
				consolPortMessaging.ShippingLineInfo.HasMessageError(
					"The Dakosy Participant Code (DPC) must be populated. Consol -> Organizations -> Co-Load With -> Details -> Config -> Registration Numbers / Codes -> DPC Code for DE."));

			consol.Creditor.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ZAP, "ZZZ", Constants.CountryCodes.Germany);
			consol.Creditor.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "DDD", Constants.CountryCodes.Germany);
			consolPortMessaging = new ConsolPortMessagingData(consol);
			consolPortMessaging.ValidateAll();

			AssertEquals("Shipping Line - Creditor not has errors (ZAP)", false,
				consolPortMessaging.ShippingLineInfo.HasMessageError(
					"The Carrier Code for Hamburg (ZAP) must be populated. Consol -> Organizations -> Co-Load With -> Details -> Config -> Registration Numbers / Codes -> ZAP Code for DE."));

			AssertEquals("Shipping Line - Carrier has errors (DPC)", false,
				consolPortMessaging.ShippingLineInfo.HasMessageError(
					"The Dakosy Participant Code (DPC) must be populated. Consol -> Organizations -> Co-Load With -> Details -> Config -> Registration Numbers / Codes -> DPC Code for DE."));
		}

		public void TestDestinationProperty()
		{
			var countryStates = Factory.New<RefCountryStates>();
			countryStates.RW_RN_NKCountryCode = Constants.CountryCodes.UnitedKingdom;
			countryStates.RW_RegionName = RefUNLOCO.Regions.NorthernIreland;

			var belfast = new RefUNLOCO.Loader(Factory).Load("GBBEL");
			belfast.RL_RW = countryStates.PK;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "GBBEL";

			var portMessagingData = new ConsolPortMessagingData(consol);

			AssertEquals(
				"When the consol's discharge port is in GB and its region name is NORTHERN IRELAND, Destination should start with XI.",
				"XIBEL",
				portMessagingData.Destination);

			countryStates.RW_RegionName = "SKYRIM";

			Factory.Save();

			AssertEquals(
				"When the consol's discharge port is in GB but its region name is not NORTHERN IRELAND, Destination should start with GB.",
				"GBBEL",
				portMessagingData.Destination);
		}

		public void TestValidateAll()
		{
			var data = GetNewPortMessagingData();
			data.ValidateAll();

			CombineAssertions(() =>
			{
				AssertHasMessageError(data.SenderCodeInfo, "Paying Party Code must be populated. Consol -> Organizations -> Sending Agent -> Details -> Config -> Registration Numbers / Codes -> DPC Code for DE.");
				AssertHasMessageError(data.AccountNoInfo, "Port Account must be populated. Consol -> Organizations -> Sending Agent -> Details -> Config -> Registration Numbers / Codes -> ZAP Code for DE.");

				AssertHasMessageError(data.ShippingLineInfo, "Shipping Line must be populated. Consol -> Organizations -> Carrier.");
				AssertHasMessageError(data.VesselNameInfo, "Vessel must be populated. Consol -> Vessel.");
				AssertHasMessageError(data.DepartureInfo, "Departure Date must be populated. Consol -> ETD.");
				AssertHasMessageError(data.VoyageNoInfo, "Voyage Number must be populated. Consol -> Voyage.");
				AssertHasMessageError(data.DestinationInfo, "Destination Code must be populated. Consol -> Last Disc.");
				AssertHasMessageError(data.WarehouseInfo, "The DAKOSY Participant Code (DPC) must be populated. Consol > Departure > CTO Address > Details > Config > Registration Numbers / Codes > DPC Code for DE.");

				AssertHasMessageError(data.ShipperNameInfo, "Shipping Line must be populated. Consol -> Organizations -> Sending Agent.");
				AssertHasMessageError(data.ShipperCodeInfo, "Agent /Issuer Code must be populated. Consol -> Organizations -> Sending Agent -> Details -> Config -> Registration Numbers / Codes -> DPC Code for DE.");
				AssertHasMessageError(data.ShipperPortAccountInfo, "Shipping Line must be populated. Consol -> Organizations -> Sending Agent -> Details -> Config -> Registration Numbers / Codes -> ZAP Code for DE.");
			});
		}

		public void TestDakosyLogs()
		{
			var data = GetNewPortMessagingData();
			var logParent = GetLogParent();

			logParent.Logs.AddNew(Events.Departure, "|LOC=AUSYD|FAC=TERMINAL");
			logParent.Logs.AddNew(Events.MessageAccepted, "|LOC=AUSYD|FAC=TERMINAL");
			logParent.Logs.AddNew(Events.MessageSent, "|DEP=Dakosy|LOC=DEHAM");

			var relatedIRJ = logParent.Logs.AddNew(Events.InterchangeRejected, "|DEP=Dakosy");
			var unrelatedDEX = logParent.Logs.AddNew(Events.DataExport, "|Not-related");
			var relatedDEX = logParent.Logs.AddNew(Events.DataExport, "|Related");

			AddMessageAndInterchange(unrelatedDEX, "BLAH", "SOMEONE");
			AddMessageAndInterchange(relatedDEX, "BLAH", "DAKOSYHAM");

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { "MSN|DEP=Dakosy|LOC=DEHAM", "DEX|Related", "IRJ|DEP=Dakosy" },
				data.DakosyLogs.Cast<StmALog>().Select(x => x.SL_SE_NKEvent + x.SL_Reference));
		}

		[TestDate(2016, 09, 25)]
		public void TestDakosyBerthCodeFallback()
		{
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			var sailingCTO = Factory.New<OrgHeader>();
			var consolCTO = Factory.New<OrgHeader>();

			sailingCTO.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyBerthCode, "BSL", Constants.CountryCodes.Germany);
			consolCTO.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyBerthCode, "BCN", Constants.CountryCodes.Germany);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_OA_DepartureCTOAddress = consolCTO.MainAddress.PK;

			consol.Transports[0].JW_JX = CreateNewSailing(vessel, "voyageNo", "DEHAM", "AUSYD", ZDateTime.Today.AddDays(-7), ZDateTime.Today.AddDays(1)).PK;

			var origin = consol.Voyage.Origins.OfType<VoyageOrigin>().First(o => o.JA_RL_NKPortOfLoading.Equals("DEHAM"));
			origin.JA_RL_NKPortOfLoading = "DEHAM";
			origin.JA_Berth = "BOR";

			var consolPortMessaging = new ConsolPortMessagingData(consol);
			AssertEquals("BOR", consolPortMessaging.Berth);

			origin.JA_Berth = ZString.Empty;
			origin.JA_OA_DepartureCTOAddress = sailingCTO.MainAddress.PK;

			AssertEquals("BSL", consolPortMessaging.Berth);

			origin.JA_OA_DepartureCTOAddress = ZGuid.Empty;
			origin.JA_Berth = ZString.Empty;

			AssertEquals("BCN", consolPortMessaging.Berth);
		}

		[TestDate(2016, 09, 25)]
		public void TestDakosyParticipantCodeFallback()
		{
			var consolCTO = Factory.New<OrgHeader>();

			consolCTO.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "BCN",
				Constants.CountryCodes.Germany);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_OA_DepartureCTOAddress = consolCTO.MainAddress.PK;

			var consolPortMessaging = new ConsolPortMessagingData(consol);
			AssertEquals("BCN", consolPortMessaging.Warehouse);
		}

		void AddMessageAndInterchange(StmALog log, ZString from, ZString to)
		{
			var pivot = Factory.New<IGenPivot>();
			var message = Factory.New<IXmlEDIMessage>();
			var interchange = Factory.New<IXmlEDIInterchange>();
			pivot.XX_RelationType = "XEM";
			pivot.XX_Relation1ID = log.PK;
			pivot.XX_Relation1TableCode = "SL";
			pivot.XX_Relation2ID = message.PK;
			pivot.XX_Relation2TableCode = "EM";
			message.EM_EI = interchange.PK;
			interchange.EI_From = from;
			interchange.EI_To = to;
		}

		#region Implementation

		JobSailing CreateNewSailing(RefVessel vessel, ZString voyageNo, ZString loadPort, ZString dischargePort, ZDateTime departureTime, ZDateTime arrivalTime)
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = voyageNo;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = loadPort;
			origin.JA_E_DEP = departureTime;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = dischargePort;
			destination.JB_E_ARV = arrivalTime;
			voyage.GenerateSailings();

			var result = voyage.Sailings.AddNew();
			result.JX_JB = destination.PK;
			result.JX_JA = origin.PK;
			return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewPortMessagingData();
		}

		protected abstract PortMessagingData GetNewPortMessagingData();

		protected abstract IStmALogParent GetLogParent();

		#endregion
	}
}
