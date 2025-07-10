using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.Business.Testing
{
	sealed class MasterBillValidationTest : BaseFreightTest
	{
		public void TestIsDuplicate_Consol_CoLoadType()
		{
			const string testMasterBillNum = "08187443521";

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_AgentType = Constants.AgentType.Agent;
			consol1.JK_TransportMode = Constants.TransportModes.Air;
			consol1.JK_ConsolMode = Constants.ContainerModes.Loose;

			var transport = consol1.Transports[0];
			transport.JW_JX = ExportSailing.PK;

			consol1.JK_MasterBillNum = testMasterBillNum;

			Factory.Save();

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_AgentType = Constants.AgentType.Agent;
			consol2.JK_TransportMode = Constants.TransportModes.Air;
			consol2.JK_ConsolMode = Constants.ContainerModes.Loose;

			var transport2 = consol2.Transports[0];

			transport2.JW_JX = ExportSailing.PK;
			consol2.JK_MasterBillNum = testMasterBillNum;

			Assert("Duplicate Master Bill -> Expecting Error", MasterBillValidator.IsDuplicate(consol2, ZDateTime.Empty, ZDateTime.Empty));

			consol2.JK_MasterBillNum = "53535879654";

			Assert("Different Master Bill -> Expecting No Error", !MasterBillValidator.IsDuplicate(consol2, ZDateTime.Empty, ZDateTime.Empty));

			consol2.JK_AgentType = Constants.AgentType.CoLoad;
			consol2.JK_MasterBillNum = testMasterBillNum;

			Assert("Only One Co-Load Consol -> Expecting Error", MasterBillValidator.IsDuplicate(consol2, ZDateTime.Empty, ZDateTime.Empty));

			consol1.JK_AgentType = Constants.AgentType.CoLoad;
			consol2.JK_MasterBillNum = "08621542154";
			Factory.Save();

			Assert("Different Master Bill -> Expecting No Error", !MasterBillValidator.IsDuplicate(consol2, ZDateTime.Empty, ZDateTime.Empty));

			consol2.JK_MasterBillNum = testMasterBillNum;

			Assert("Two Co-Load Consols -> Expecting No Error", !MasterBillValidator.IsDuplicate(consol2, ZDateTime.Empty, ZDateTime.Empty));

			consol2.JK_AgentType = Constants.AgentType.Agent;

			Assert("Only One Co-Load Consol -> Expecting Error", MasterBillValidator.IsDuplicate(consol2, ZDateTime.Empty, ZDateTime.Empty));
		}

		public void TestIsDuplicate_Consol_GatewayCoLoadType()
		{
			const string testMasterBillNum = "08187443521";

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_AgentType = Constants.AgentType.Agent;
			consol1.JK_TransportMode = Constants.TransportModes.Air;
			consol1.JK_ConsolMode = Constants.ContainerModes.Loose;

			var transport = consol1.Transports[0];
			transport.JW_JX = ExportSailing.PK;

			consol1.JK_MasterBillNum = testMasterBillNum;

			Factory.Save();

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_AgentType = Constants.AgentType.Agent;
			consol2.JK_TransportMode = Constants.TransportModes.Air;
			consol2.JK_ConsolMode = Constants.ContainerModes.Loose;

			var transport2 = consol2.Transports[0];

			transport2.JW_JX = ExportSailing.PK;
			consol2.JK_MasterBillNum = testMasterBillNum;

			Assert("Duplicate Master Bill -> Expecting Error", MasterBillValidator.IsDuplicate(consol2, ZDateTime.Empty, ZDateTime.Empty));

			consol2.JK_MasterBillNum = "53535879654";

			Assert("Different Master Bill -> Expecting No Error", !MasterBillValidator.IsDuplicate(consol2, ZDateTime.Empty, ZDateTime.Empty));

			consol2.JK_AgentType = Constants.AgentType.CoLoad;
			consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol2.JK_MasterBillNum = testMasterBillNum;

			Assert("Only One Gateway Co-Load Consol -> Expecting Error", MasterBillValidator.IsDuplicate(consol2, ZDateTime.Empty, ZDateTime.Empty));

			consol1.JK_AgentType = Constants.AgentType.CoLoad;
			consol1.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol2.JK_MasterBillNum = "08621542154";
			Factory.Save();

			Assert("Different Master Bill -> Expecting No Error", !MasterBillValidator.IsDuplicate(consol2, ZDateTime.Empty, ZDateTime.Empty));

			consol2.JK_MasterBillNum = testMasterBillNum;

			Assert("Two Gateway Co-Load Consols -> Expecting No Error", !MasterBillValidator.IsDuplicate(consol2, ZDateTime.Empty, ZDateTime.Empty));

			consol2.JK_AgentType = Constants.AgentType.Agent;

			Assert("Only One Gateway Co-Load Consol -> Expecting Error", MasterBillValidator.IsDuplicate(consol2, ZDateTime.Empty, ZDateTime.Empty));
		}

		public void TestIsDuplicate_Consol_CoLoadAndGatewayCoLoadTypes()
		{
			const string testMasterBillNum = "08187443521";

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_AgentType = Constants.AgentType.Agent;
			consol1.JK_TransportMode = Constants.TransportModes.Air;
			consol1.JK_ConsolMode = Constants.ContainerModes.Loose;

			var transport = consol1.Transports[0];
			transport.JW_JX = ExportSailing.PK;

			consol1.JK_MasterBillNum = testMasterBillNum;

			Factory.Save();

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_AgentType = Constants.AgentType.CoLoad;
			consol2.JK_TransportMode = Constants.TransportModes.Air;
			consol2.JK_ConsolMode = Constants.ContainerModes.Loose;

			var transport2 = consol2.Transports[0];

			transport2.JW_JX = ExportSailing.PK;
			consol2.JK_MasterBillNum = testMasterBillNum;
			Assert("Duplicate Master Bill -> Expecting Error", MasterBillValidator.IsDuplicate(consol2, ZDateTime.Empty, ZDateTime.Empty));

			consol2.JK_MasterBillNum = "53535879654";
			Assert("Different Master Bill -> Expecting No Error", !MasterBillValidator.IsDuplicate(consol2, ZDateTime.Empty, ZDateTime.Empty));

			consol1.JK_AgentType = Constants.AgentType.CoLoad;
			consol1.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol2.JK_MasterBillNum = "08621542154";
			Factory.Save();

			Assert("Different Master Bill -> Expecting No Error", !MasterBillValidator.IsDuplicate(consol2, ZDateTime.Empty, ZDateTime.Empty));

			consol2.JK_MasterBillNum = testMasterBillNum;
			Assert("Co-Load and Gateway Co-Load Consols -> Expecting No Error", !MasterBillValidator.IsDuplicate(consol2, ZDateTime.Empty, ZDateTime.Empty));
		}

		public void TestIsDuplicate_Booking()
		{
			const string testMasterBillNum = "08187443521";

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_AgentType = Constants.AgentType.Agent;
			consol1.JK_TransportMode = Constants.TransportModes.Air;
			consol1.JK_ConsolMode = Constants.ContainerModes.Loose;

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_AgentType = Constants.AgentType.CoLoad;
			consol2.JK_TransportMode = Constants.TransportModes.Air;
			consol2.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol2.JK_MasterBillNum = testMasterBillNum;

			Factory.Save();

			var booking = CommonShipment.New(Factory);

			var helper = new SailingsForTestClasses(Factory);
			var sailing = helper.SydLaxSailing;
			sailing.Voyage.JV_VoyageFlight = "QF123";
			booking.JS_JX = sailing.PK;

			booking.JS_IsDirectBooking = true;
			booking.JS_TransportMode = Constants.TransportModes.Air;
			booking.JS_IsNeutralMaster = true;

			booking.JS_HouseBill = testMasterBillNum;

			Assert("Duplicate Master Bill on unrelated Consol and Booking -> Expecting Error", MasterBillValidator.IsDuplicate(booking, ZDateTime.Empty, ZDateTime.Empty));

			consol1.JK_MasterBillNum = "22324234235";
			booking.JS_HouseBill = "15637839654";
			consol2.Shipments.Add(booking);

			Factory.Save();

			booking.JS_HouseBill = testMasterBillNum;

			Assert("Duplicate Master Bill when Consol is related -> Expecting Error", MasterBillValidator.IsDuplicate(booking, ZDateTime.Empty, ZDateTime.Empty));

			consol2.JK_MasterBillNum = "46197135464";
			Factory.Save();

			Assert("Different Master Bill -> Expecting No Error", !MasterBillValidator.IsDuplicate(booking, ZDateTime.Empty, ZDateTime.Empty));
		}

		public void TestCancelledDuplicates()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_HouseBill = "73821042313";
			SetBookingAndForwardRegistered(shipment);

			CommonConsol consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_MasterBillNum = "73821042314";
			Factory.Save();

			CommonShipment duplicateShipment = Factory.NewWithValidTestData<CommonShipment>();
			duplicateShipment.JS_HouseBill = "73821042313";

			CommonConsol duplicateConsol = Factory.NewWithValidTestData<CommonConsol>();
			duplicateConsol.JK_MasterBillNum = "73821042314";

			Assert(MasterBillValidator.IsDuplicate(duplicateShipment, ZDateTime.Empty, ZDateTime.Empty));
			Assert(MasterBillValidator.IsDuplicate(duplicateConsol, ZDateTime.Empty, ZDateTime.Empty));

			shipment.JS_IsCancelled = true;
			consol.JK_IsCancelled = true;
			Factory.Save();

			Assert(!MasterBillValidator.IsDuplicate(duplicateShipment, ZDateTime.Empty, ZDateTime.Empty));
			Assert(!MasterBillValidator.IsDuplicate(duplicateConsol, ZDateTime.Empty, ZDateTime.Empty));
		}

		public void TestBookingAndForwardRegisteredDuplicates()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_HouseBill = "20141221220";
			SetBookingAndForwardRegistered(shipment);
			Factory.Save();

			var duplicateShipment = Factory.NewWithValidTestData<CommonShipment>();
			duplicateShipment.JS_HouseBill = "20141221220";

			Assert(MasterBillValidator.IsDuplicate(duplicateShipment, ZDateTime.Empty, ZDateTime.Empty));

			shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_HouseBill = "20141221221";
			shipment.JS_IsBooking = false;
			shipment.JS_IsForwardRegistered = false;

			duplicateShipment.JS_HouseBill = "20141221221";

			Factory.Save();

			Assert(!MasterBillValidator.IsDuplicate(duplicateShipment, ZDateTime.Empty, ZDateTime.Empty));

			shipment.JS_IsBooking = true;
			shipment.JS_IsForwardRegistered = true;
			Factory.Save();

			Assert(!MasterBillValidator.IsDuplicate(duplicateShipment, ZDateTime.Empty, ZDateTime.Empty));

			shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_HouseBill = "20141221222";
			shipment.JS_IsBooking = false;
			shipment.JS_IsForwardRegistered = true;

			duplicateShipment.JS_HouseBill = "20141221222";

			Factory.Save();

			Assert(!MasterBillValidator.IsDuplicate(duplicateShipment, ZDateTime.Empty, ZDateTime.Empty));
		}

		public void TestGetMAWBFormatValidMessage()
		{
			AssertEquals("The MAWB should contain 11 digits.", MasterBillValidator.GetMAWBFormatValidMessage("0811212", Factory));
			AssertEquals("The MAWB can only contain numbers.", MasterBillValidator.GetMAWBFormatValidMessage("0811A212111", Factory));
			AssertEquals("Invalid check digit. The last digit should be '1'", MasterBillValidator.GetMAWBFormatValidMessage("08310000000", Factory));
			AssertEquals("", MasterBillValidator.GetMAWBFormatValidMessage("08310000001", Factory));
		}

		public void TestGetMAWBFormatValidMessage_NumericCheck_WhenConsolCarrierAirLineIsIATAMember()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "A12";
			airline.RM_MembershipFlagIATA = false;

			var airline2 = Factory.New<RefAirline>();
			airline2.RM_EagleAddedAirlinePrefixOrAccountingCode = "B12";
			airline2.RM_MembershipFlagIATA = true;

			var consol1 = (CommonConsol)Factory.New<IForwardingConsol>();
			consol1.JK_MasterBillNum = "A1213712113";

			var consol2 = (CommonConsol)Factory.New<IForwardingConsol>();
			consol2.JK_MasterBillNum = "B1213212113";

			Factory.Save();

			AssertEquals("An error message should be generated as the airline is NOT an IATA member, therefore not allowing alpha-numeric characters in the MAWB", "The MAWB can only contain numbers.", MasterBillValidator.GetMAWBFormatValidMessage(String.Concat(((IMAWBAllocationParent)consol1).MasterBillAirlinePrefix, ((IMAWBAllocationParent)consol1).MasterBillMAWB), consol1.Factory));
			AssertEquals("No message should be generated as the airline is an IATA member, hence allowing alpha-numeric characters in the MAWB", "", MasterBillValidator.GetMAWBFormatValidMessage(String.Concat(((IMAWBAllocationParent)consol2).MasterBillAirlinePrefix, ((IMAWBAllocationParent)consol2).MasterBillMAWB), consol2.Factory));
		}

		public void TestGetMAWBFormatValidMessage_HandlesNullFactory()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "A12";
			airline.RM_MembershipFlagIATA = false;

			var consol1 = (CommonConsol)Factory.New<IForwardingConsol>();
			consol1.JK_MasterBillNum = "A1213712113";

			Factory.Save();

			AssertNoExceptionThrown(() => MasterBillValidator.GetMAWBFormatValidMessage(String.Concat(((IMAWBAllocationParent)consol1).MasterBillAirlinePrefix, ((IMAWBAllocationParent)consol1).MasterBillMAWB), null));
			AssertEquals("Validation is still run despite a null factory, as a new factory should be created and used.", "The MAWB can only contain numbers.", MasterBillValidator.GetMAWBFormatValidMessage(String.Concat(((IMAWBAllocationParent)consol1).MasterBillAirlinePrefix, ((IMAWBAllocationParent)consol1).MasterBillMAWB), null));
		}

		public void TestNonLatinDigitsAreStillInvalid()
		{
			const string nonLatinDigits = "\u0663\u096d\u0e53\u0beb\u0bee\uff12";
			for (int i = 0; i < nonLatinDigits.Length; i++)
			{
				char digit = nonLatinDigits[i];
				AssertEquals(string.Format("precondition: '{0}'(i={1}) should be a numeric character", digit, i), true, char.IsDigit(digit));
				AssertEquals(string.Format("precondition: '{0}'(i={1}) should be a non-latin numeric character", digit, i), -1, "0123456789".IndexOf(digit));

				string brokenMawb = string.Format("0123456789{0}", digit);
				AssertEquals(string.Format("i={0}, mawb='{1}'", i, brokenMawb), "The MAWB can only contain western Arabic numerals (0-9).", MasterBillValidator.GetMAWBFormatValidMessage(brokenMawb, Factory));
			}
		}

		public void TestDontMatchOldConsols()
		{
			const string BillNum = "snth";
			ZDateTime now = ZDateTime.Now;

			CommonConsol oldConsol = Factory.New<CommonConsol>();
			oldConsol.JK_SystemCreateTimeUtc = now.AddYears(-1);
			oldConsol.JK_MasterBillNum = BillNum;

			Factory.Save();

			CommonConsol newConsol = Factory.New<CommonConsol>();
			newConsol.JK_SystemCreateTimeUtc = now;
			newConsol.JK_MasterBillNum = BillNum;

			CommonShipment newShipment = Factory.New<CommonShipment>();
			newShipment.JS_SystemCreateTimeUtc = now;
			newShipment.JS_HouseBill = BillNum;

			AssertEquals("", true, MasterBillValidator.IsDuplicate(newConsol, ZDateTime.Empty, ZDateTime.Empty));
			AssertEquals("", false, MasterBillValidator.IsDuplicate(newConsol, now.AddMonths(-11), ZDateTime.Empty));
			AssertEquals("", true, MasterBillValidator.IsDuplicate(newConsol, now.AddMonths(-13), ZDateTime.Empty));

			AssertEquals("", true, MasterBillValidator.IsDuplicate(newShipment, ZDateTime.Empty, ZDateTime.Empty));
			AssertEquals("", false, MasterBillValidator.IsDuplicate(newShipment, now.AddMonths(-11), ZDateTime.Empty));
			AssertEquals("", true, MasterBillValidator.IsDuplicate(newShipment, now.AddMonths(-13), ZDateTime.Empty));
		}

		public void TestDontMatchOldShipments()
		{
			const string BillNum = "snth";
			ZDateTime now = ZDateTime.Now;

			CommonShipment oldShipment = Factory.New<CommonShipment>();
			oldShipment.JS_SystemCreateTimeUtc = now.AddYears(-1);
			oldShipment.JS_HouseBill = BillNum;
			SetBookingAndForwardRegistered(oldShipment);

			Factory.Save();

			CommonConsol newConsol = Factory.New<CommonConsol>();
			newConsol.JK_SystemCreateTimeUtc = now;
			newConsol.JK_MasterBillNum = BillNum;

			CommonShipment newShipment = Factory.New<CommonShipment>();
			newShipment.JS_SystemCreateTimeUtc = now;
			newShipment.JS_HouseBill = BillNum;

			AssertEquals("", true, MasterBillValidator.IsDuplicate(newConsol, ZDateTime.Empty, ZDateTime.Empty));
			AssertEquals("", false, MasterBillValidator.IsDuplicate(newConsol, now.AddMonths(-11), ZDateTime.Empty));
			AssertEquals("", true, MasterBillValidator.IsDuplicate(newConsol, now.AddMonths(-13), ZDateTime.Empty));

			AssertEquals("", true, MasterBillValidator.IsDuplicate(newShipment, ZDateTime.Empty, ZDateTime.Empty));
			AssertEquals("", false, MasterBillValidator.IsDuplicate(newShipment, now.AddMonths(-11), ZDateTime.Empty));
			AssertEquals("", true, MasterBillValidator.IsDuplicate(newShipment, now.AddMonths(-13), ZDateTime.Empty));
		}

		public void TestDontMatchOldShipments_TwoShipments()
		{
			const string BillNum = "snth";
			ZDateTime now = ZDateTime.Now;

			CommonShipment oldShipment1 = Factory.New<CommonShipment>();
			oldShipment1.JS_SystemCreateTimeUtc = now.AddYears(-1);
			oldShipment1.JS_HouseBill = BillNum;
			SetBookingAndForwardRegistered(oldShipment1);

			CommonShipment oldShipment2 = Factory.New<CommonShipment>();
			oldShipment2.JS_SystemCreateTimeUtc = now.AddYears(-1);
			oldShipment2.JS_HouseBill = BillNum;
			SetBookingAndForwardRegistered(oldShipment2);

			Factory.Save();

			CommonConsol newConsol = Factory.New<CommonConsol>();
			newConsol.JK_SystemCreateTimeUtc = now;
			newConsol.JK_MasterBillNum = BillNum;

			CommonShipment newShipment = Factory.New<CommonShipment>();
			newShipment.JS_SystemCreateTimeUtc = now;
			newShipment.JS_HouseBill = BillNum;

			AssertEquals("", true, MasterBillValidator.IsDuplicate(newConsol, ZDateTime.Empty, ZDateTime.Empty));
			AssertEquals("", false, MasterBillValidator.IsDuplicate(newConsol, now.AddMonths(-11), ZDateTime.Empty));
			AssertEquals("", true, MasterBillValidator.IsDuplicate(newConsol, now.AddMonths(-13), ZDateTime.Empty));

			AssertEquals("", true, MasterBillValidator.IsDuplicate(newShipment, ZDateTime.Empty, ZDateTime.Empty));
			AssertEquals("", false, MasterBillValidator.IsDuplicate(newShipment, now.AddMonths(-11), ZDateTime.Empty));
			AssertEquals("", true, MasterBillValidator.IsDuplicate(newShipment, now.AddMonths(-13), ZDateTime.Empty));
		}

		public void TestShipmentsDetachedFromConsol()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			CommonConsol consol = (CommonConsol)factory.New<IForwardingConsol>();

			CommonShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "AAAA";

			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "BBBB";

			factory.Save();

			consol = Factory.Load<CommonConsol>(consol.PK);
			consol.JK_MasterBillNum = "AAAA";
			AssertEquals(false, MasterBillValidator.IsDuplicate(consol, ZDateTime.Empty, ZDateTime.Empty));

			shipment1 = consol.Shipments.Cast<CommonShipment>().First(shipment => shipment.PK == shipment1.PK);
			SetBookingAndForwardRegistered(shipment1);
			Factory.Save();

			consol.Shipments.Remove(shipment1);
			AssertEquals(true, MasterBillValidator.IsDuplicate(consol, ZDateTime.Empty, ZDateTime.Empty));
		}

		public void TestShipmentsDetachedFromConsol_ChangedHouseBillNotSaved()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			CommonConsol consol = (CommonConsol)factory.New<IForwardingConsol>();

			CommonShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "AAAA";

			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "BBBB";

			factory.Save();

			consol = (CommonConsol)Factory.Load<IForwardingConsol>(consol.PK);

			consol.JK_MasterBillNum = "AAAA";
			AssertEquals(false, MasterBillValidator.IsDuplicate(consol, ZDateTime.Empty, ZDateTime.Empty));

			shipment1 = consol.Shipments.Cast<CommonShipment>().First(shipment => shipment.PK == shipment1.PK);
			SetBookingAndForwardRegistered(shipment1);
			Factory.Save();

			shipment1.JS_HouseBill = "CCCC";
			consol.Shipments.Remove(shipment1);
			AssertEquals(true, MasterBillValidator.IsDuplicate(consol, ZDateTime.Empty, ZDateTime.Empty));

			Factory.Save();
			AssertEquals(false, MasterBillValidator.IsDuplicate(consol, ZDateTime.Empty, ZDateTime.Empty));
		}

		public void TestShipmentsAttachedFromConsol()
		{
			CommonConsol consol = Factory.New<CommonConsol>();

			CommonShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "BBBB";
			SetBookingAndForwardRegistered(shipment1);

			CommonShipment shipment2 = Factory.New<CommonShipment>();
			shipment2.JS_HouseBill = "AAAA";
			SetBookingAndForwardRegistered(shipment2);

			Factory.Save();

			consol.JK_MasterBillNum = "AAAA";
			AssertEquals(true, MasterBillValidator.IsDuplicate(consol, ZDateTime.Empty, ZDateTime.Empty));

			consol.Shipments.Add(shipment2);
			AssertEquals(false, MasterBillValidator.IsDuplicate(consol, ZDateTime.Empty, ZDateTime.Empty));

			Factory.Save();

			AssertEquals(false, MasterBillValidator.IsDuplicate(consol, ZDateTime.Empty, ZDateTime.Empty));
		}

		public void TestDontMatchConsolsAfterCutOffEnd()
		{
			const string BillNum = "snth";
			ZDateTime now = ZDateTime.Now;

			CommonConsol existingConsol = Factory.New<CommonConsol>();
			existingConsol.JK_SystemCreateTimeUtc = now.AddYears(-1);
			existingConsol.JK_MasterBillNum = BillNum;

			Factory.Save();

			CommonConsol newConsol = Factory.New<CommonConsol>();
			newConsol.JK_SystemCreateTimeUtc = now;
			newConsol.JK_MasterBillNum = BillNum;

			AssertEquals("Duplicate consol", true, MasterBillValidator.IsDuplicate(newConsol, ZDateTime.Empty, ZDateTime.Empty));
			AssertEquals("Duplicate as there is an existing consol within the date range", true, MasterBillValidator.IsDuplicate(newConsol, now.AddMonths(-24), now.AddMonths(-11)));
			AssertEquals("Not duplicate as the existing consol is after the cut off end date", false, MasterBillValidator.IsDuplicate(newConsol, now.AddMonths(-24), now.AddMonths(-13)));
		}

		public void TestDuplicateMAWB_SameBOLExist()
		{
			const string BillNum = "1000";

			CommonConsol seaConsol = Factory.New<CommonConsol>();
			seaConsol.JK_TransportMode = Constants.TransportModes.Sea;
			seaConsol.JK_MasterBillNum = BillNum;

			Factory.Save();

			AssertEquals("Not duplicated when same BOL number existed", ZString.Empty, MasterBillValidator.DuplicateMAWB(Factory, "1000", "1010", Constants.TransportModes.Air));
		}

		public void TestIsDuplicate_Sea_SameMAWBExist()
		{
			const string BillNum = "snth";

			CommonConsol airConsol = Factory.New<CommonConsol>();
			airConsol.JK_TransportMode = Constants.TransportModes.Air;
			airConsol.JK_MasterBillNum = BillNum;

			Factory.Save();

			CommonConsol seaConsol = Factory.New<CommonConsol>();
			seaConsol.JK_TransportMode = Constants.TransportModes.Sea;
			seaConsol.JK_MasterBillNum = BillNum;

			CommonShipment seaShipment = Factory.New<CommonShipment>();
			seaShipment.JS_TransportMode = Constants.TransportModes.Sea;
			seaShipment.JS_HouseBill = BillNum;

			AssertEquals("Not duplicated when same MAWB number existed", false, MasterBillValidator.IsDuplicate(seaConsol, ZDateTime.Empty, ZDateTime.Empty));
			AssertEquals("Not duplicated when same MAWB number existed", false, MasterBillValidator.IsDuplicate(seaShipment, ZDateTime.Empty, ZDateTime.Empty));
		}

		public void TestIsDuplicate_Air_SameBOLExist()
		{
			const string BillNum = "snth";

			CommonConsol seaConsol = Factory.New<CommonConsol>();
			seaConsol.JK_TransportMode = Constants.TransportModes.Sea;
			seaConsol.JK_MasterBillNum = BillNum;

			Factory.Save();

			CommonConsol airConsol = Factory.New<CommonConsol>();
			airConsol.JK_TransportMode = Constants.TransportModes.Air;
			airConsol.JK_MasterBillNum = BillNum;

			CommonShipment airShipment = Factory.New<CommonShipment>();
			airShipment.JS_TransportMode = Constants.TransportModes.Air;
			airShipment.JS_HouseBill = BillNum;

			AssertEquals("Not duplicated when same BOL number existed", false, MasterBillValidator.IsDuplicate(airConsol, ZDateTime.Empty, ZDateTime.Empty));
			AssertEquals("Not duplicated when same BOL number existed", false, MasterBillValidator.IsDuplicate(airShipment, ZDateTime.Empty, ZDateTime.Empty));
		}

		#region TestValidateNeutralMAWB

		public void TestValidateMasterBillNum_DuplicateReusePeriod()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			JobMawb mawb = AddMawb(factory, "081", "00000011", GlbBranch.CurrentBranch, "STD");
			mawb.JM_IsPaper = false;
			factory.Save();

			var consol = (CommonConsol)Factory.New<IForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_IsNeutralMaster = false;
			consol.JK_MasterBillNum = "08100000011";

			AssertHasErrorContaining(((IMAWBAllocationParent)consol).MasterBillMAWBInfo,
				"This Master Bill Number is already in stock.\r\nIt is flagged as a Neutral Number.\r\nPlease enter another number.");

			ZInt savedRecyclePeriod = FreightDataRegistry.Instance.MAWBRecyclePeriod.Value;
			try
			{
				FreightDataRegistry.Instance.MAWBRecyclePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 12);
				mawb.JM_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-13);
				factory.Save();

				consol = (CommonConsol)Factory.New<IForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_IsNeutralMaster = false;
				consol.JK_MasterBillNum = "08100000011";

				AssertNoErrorContaining(((IMAWBAllocationParent)consol).MasterBillMAWBInfo,
					"This Master Bill Number is already in stock.\r\nIt is flagged as a Neutral Number.\r\nPlease enter another number.");
			}
			finally
			{
				FreightDataRegistry.Instance.MAWBRecyclePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, savedRecyclePeriod);
			}
		}

		public void TestValidateMasterBillNum_BorrowedOut()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			JobMawb mawb = AddMawb(factory, "081", "00000011", GlbBranch.CurrentBranch, "STD");
			mawb.JM_OH_AllocatedTo = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.OrgProxy.PK)).PK;
			factory.Save();

			var consol = (CommonConsol)Factory.New<IForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_IsNeutralMaster = true;
			consol.JK_MasterBillNum = "08100000011";

			AssertHasErrorContaining(((IMAWBAllocationParent)consol).MasterBillMAWBInfo,
				"MAWB has been 'borrowed out' to a customer. Please enter another number.");

			mawb.JM_OH_AllocatedTo = ZGuid.Empty;
			factory.Save();

			consol = (CommonConsol)Factory.New<IForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_IsNeutralMaster = true;
			consol.JK_MasterBillNum = "08100000011";

			AssertNoErrorContaining(((IMAWBAllocationParent)consol).MasterBillMAWBInfo,
				"MAWB has been 'borrowed out' to a customer. Please enter another number.");
		}

		public void TestValidateMasterBillNum_NumberLeft()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			AddMawb(factory, "081", "00000000", GlbBranch.CurrentBranch, "STD");
			AddMawb(factory, "081", "00000011", GlbBranch.CurrentBranch, "STD");
			AddMawb(factory, "081", "00000022", GlbBranch.CurrentBranch, "STD");
			factory.Save();

			var consol1 = (CommonConsol)Factory.New<IForwardingConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Air;
			consol1.JK_RL_NKLoadPort = HomePort;
			consol1.JK_IsNeutralMaster = true;
			consol1.JK_MasterBillNum = "08100000000";
			Factory.Save();

			AssertHasWarningContaining(((IMAWBAllocationParent)consol1).MasterBillNeutralMAWBInfo,
				"There are only 2 MAWBs left for this Airline. Please add more numbers to your stock.");

			var consol2 = (CommonConsol)Factory.New<IForwardingConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Air;
			consol2.JK_RL_NKLoadPort = HomePort;
			consol2.JK_IsNeutralMaster = true;
			consol2.JK_MasterBillNum = "08100000011";

			Factory.Save();
			AssertHasWarningContaining(((IMAWBAllocationParent)consol2).MasterBillNeutralMAWBInfo,
				"There is only 1 MAWB left for this Airline. Please add more numbers to your stock.");

			var consol3 = (CommonConsol)Factory.New<IForwardingConsol>();
			consol3.JK_TransportMode = Constants.TransportModes.Air;
			consol3.JK_RL_NKLoadPort = HomePort;
			consol3.JK_IsNeutralMaster = true;
			consol3.JK_MasterBillNum = "08100000022";

			Factory.Save();
			AssertHasWarningContaining(((IMAWBAllocationParent)consol3).MasterBillNeutralMAWBInfo,
				"There are no MAWBs left for this Airline. Please add more numbers to your stock.");
		}

		public void TestValidateMasterBillNum_Format()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			AddMawb(factory, "081", "00000000", GlbBranch.CurrentBranch, "STD");
			factory.Save();

			var consol = (CommonConsol)Factory.New<IForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_IsNeutralMaster = true;
			consol.JK_MasterBillNum = "1234";

			AssertHasWarningContaining(((IMAWBAllocationParent)consol).MasterBillMAWBInfo, "The MAWB should contain 11 digits.");

			consol.JK_MasterBillNum = "1234567890A";

			AssertHasWarningContaining(((IMAWBAllocationParent)consol).MasterBillMAWBInfo, "The MAWB can only contain numbers.");

			consol.JK_MasterBillNum = "08100000001";

			AssertHasWarningContaining(((IMAWBAllocationParent)consol).MasterBillMAWBInfo, "Invalid check digit. The last digit should be '0'");
		}

		public void TestValidateMasterBillNum_FallBackToRegistry()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			AddMawb(factory, "081", "00000000", GlbBranch.CurrentBranch, "STD");
			AddMawb(factory, "081", "00000011", GlbBranch.CurrentBranch, "STD");
			AddMawb(factory, "081", "00000022", GlbBranch.CurrentBranch, "STD");
			AddMawb(factory, "081", "00000033", GlbBranch.CurrentBranch, "STD");
			factory.Save();

			using (FreightDataRegistry.Instance.MAWBDefaultLowStockLevel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3))
			{
				var consol1 = (CommonConsol)Factory.New<IForwardingConsol>();
				consol1.JK_TransportMode = Constants.TransportModes.Air;
				consol1.JK_RL_NKLoadPort = HomePort;
				consol1.JK_IsNeutralMaster = true;
				consol1.JK_MasterBillNum = "08100000000";
				Factory.Save();

				AssertNoWarnings("There are more MAWBs than the registry value", ((IMAWBAllocationParent)consol1).MasterBillNeutralMAWBInfo);

				var consol2 = (CommonConsol)Factory.New<IForwardingConsol>();
				consol2.JK_TransportMode = Constants.TransportModes.Air;
				consol2.JK_RL_NKLoadPort = HomePort;
				consol2.JK_IsNeutralMaster = true;
				consol2.JK_MasterBillNum = "08100000011";
				Factory.Save();

				AssertHasWarning("There are 2 MAWBs left, same as registry value",
					((IMAWBAllocationParent)consol2).MasterBillNeutralMAWBInfo,
					"There are only 2 MAWBs left for this Airline. Please add more numbers to your stock.");
			}
		}

		public void TestValidateMasterBillNum_WhenMAWMStockManagementExists()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			AddMawb(factory, "081", "00000000", GlbBranch.CurrentBranch, "STD");
			AddMawb(factory, "081", "00000011", GlbBranch.CurrentBranch, "STD");
			AddMawb(factory, "081", "00000022", GlbBranch.CurrentBranch, "STD");
			AddMawb(factory, "081", "00000033", GlbBranch.CurrentBranch, "STD");
			factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var misc = Factory.NewWithValidTestData<OrgMiscServ>();
			misc.OM_OH = org.PK;
			misc.OM_RM_Airline = RefAirline.LoadFromAirlinePrefix(Factory, "081").PK;

			AddMAWBStockManagement(org, GlbBranch.CurrentBranch, 3);
			Factory.Save();

			using (FreightDataRegistry.Instance.MAWBDefaultLowStockLevel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 10))
			{
				var consol1 = (CommonConsol)Factory.New<IForwardingConsol>();
				consol1.JK_TransportMode = Constants.TransportModes.Air;
				consol1.JK_RL_NKLoadPort = HomePort;
				consol1.JK_IsNeutralMaster = true;
				consol1.JK_MasterBillNum = "08100000000";
				Factory.Save();

				AssertNoWarnings("Registry value should not have added an error", ((IMAWBAllocationParent)consol1).MasterBillNeutralMAWBInfo);

				var consol2 = (CommonConsol)Factory.New<IForwardingConsol>();
				consol2.JK_TransportMode = Constants.TransportModes.Air;
				consol2.JK_RL_NKLoadPort = HomePort;
				consol2.JK_IsNeutralMaster = true;
				consol2.JK_MasterBillNum = "08100000011";
				Factory.Save();

				AssertHasWarning("There are 2 MAWBs left, same as StockManagement Level for this company",
					((IMAWBAllocationParent)consol2).MasterBillNeutralMAWBInfo,
					"There are only 2 MAWBs left for this Airline. Please add more numbers to your stock.");
			}
		}

		JobMawb AddMawb(BusinessObjectFactory factory, string prefix, string mawbNo, GlbBranch branch, string serviceLevel)
		{
			JobMawb mawb = factory.NewWithValidTestData<JobMawb>();
			mawb.JM_Airline3DigitPrefix = prefix;
			mawb.JM_MAWB = mawbNo;
			mawb.JM_GB = branch.PK;
			mawb.JM_ServiceLevel = serviceLevel;

			return mawb;
		}

		void AddMAWBStockManagement(OrgHeader org, GlbBranch branch, ZShort amount)
		{
			var mawbStockMangement = org.OrgAirlineMAWBStockManagementCollection.AddNew();
			mawbStockMangement.OHM_GB_Branch = branch?.PK ?? ZGuid.Empty;
			mawbStockMangement.OHM_MAWBStockThreshold = amount;
			if (mawbStockMangement.OHM_GB_Branch != ZGuid.Empty)
			{
				mawbStockMangement.OHM_GC_Company = branch.Company.PK;
			}
		}

		#endregion

		void SetBookingAndForwardRegistered(CommonShipment shipment)
		{
			if (shipment != null)
			{
				shipment.JS_IsBooking = true;
				shipment.JS_IsForwardRegistered = false;
			}
		}
	}
}
