using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11;
using Enterprise.UniversalDataBuss.Integration;
using Moq;
using NUnit.Framework;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	internal class AgencyShipmentMatcherTest : TestCaseWithFactory
	{
		public void TestBestMatchByShipperRef()
		{
			var sailing = UniversalTestHelper.CreateSailingWithVoyage(Factory, "AUSYD", "SGSIN", "MAERSK", "001");
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			Factory.Save();
			var billOfLading = GetNewBillOfLading("OBL_001", "BKG_001", "AGT_001", sailing);
			billOfLading.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			Factory.Save();

			var references = AgencyUniversalTestHelper.CreateReferences("OBL_001", "BKG_001", "AGT_001", bookingParty, ZString.Empty, new[] { new SailingReference { VesselName = "MAERSK", VoyageNumber = "001" } });
			var matcher = new AgencyShipmentMatcher<BillOfLading>(Factory, references, new DummyLogger(), DataContextType.BillOfLading);
			var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("`", billOfLading.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);

			references = AgencyUniversalTestHelper.CreateReferences("OBL_001", "BKG_001", "", bookingParty, ZString.Empty, new[] { new SailingReference { VesselName = "MAERSK", VoyageNumber = "001" } });
			matcher = new AgencyShipmentMatcher<BillOfLading>(Factory, references, new DummyLogger(), DataContextType.BillOfLading);
			(matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Shipper Ref not provided in XML, matched OBL Number, Booking Ref", billOfLading.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);

			references = AgencyUniversalTestHelper.CreateReferences("OBL_001", "BKG_001", "AGT_111", bookingParty, ZString.Empty, new[] { new SailingReference { VesselName = "MAERSK", VoyageNumber = "001" } });
			matcher = new AgencyShipmentMatcher<BillOfLading>(Factory, references, new DummyLogger(), DataContextType.BillOfLading);
			(matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertNull("XML's Agent Ref different with BOL's Shipper Ref, should not have any matched BOL", matchedShipment);
			AssertEquals("Master Bill Number is Invalid.", reason );

			billOfLading.JS_BookingReference = "BKG_999";
			Factory.Save();
			references = AgencyUniversalTestHelper.CreateReferences("OBL_001", "BKG_001", "AGT_001", bookingParty, ZString.Empty, new[] { new SailingReference { VesselName = "MAERSK", VoyageNumber = "001" } });
			matcher = new AgencyShipmentMatcher<BillOfLading>(Factory, references, new DummyLogger(), DataContextType.BillOfLading);
			(matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertNull("XML's Agent Ref different with BOL's Shipper Ref, should not have any matched BOL", matchedShipment);
			AssertEquals("Master Bill Number is Invalid.", reason);

			billOfLading.JS_BookingReference = "";
			Factory.Save();
			references = AgencyUniversalTestHelper.CreateReferences("OBL_001", "BKG_001", "AGT_001", bookingParty, ZString.Empty, new[] { new SailingReference { VesselName = "MAERSK", VoyageNumber = "001" } });
			matcher = new AgencyShipmentMatcher<BillOfLading>(Factory, references, new DummyLogger(), DataContextType.BillOfLading);
			(matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("XML's Agent Ref provided, existing BOL's Shipper Ref is blank, should matched BOL", billOfLading.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);

			billOfLading.JS_BookingReference = "AGT_001";
			billOfLading.JS_CFSReference = "BKG_001";
			Factory.Save();

			references = AgencyUniversalTestHelper.CreateReferences("OBL_001", "BKG_001", "", bookingParty, ZString.Empty, new[] { new SailingReference { VesselName = "MAERSK", VoyageNumber = "001" } });
			matcher = new AgencyShipmentMatcher<BillOfLading>(Factory, references, new DummyLogger(), DataContextType.BillOfLading);
			(matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Shipper Ref not provided in XML, matched OBL Number, Booking Ref", billOfLading.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);
		}

		public void TestBestMatch_BillOfLading()
		{
			var sailing = UniversalTestHelper.CreateSailingWithVoyage(Factory, "AUSYD", "SGSIN", "MAERSK", "001");
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			Factory.Save();
			var billOfLading1 = Factory.New<BillOfLading>();
			billOfLading1.JS_HouseBill = "";
			billOfLading1.JS_CFSReference = "BKG001";
			billOfLading1.JS_BookingReference = "AGT001";
			billOfLading1.JS_JX = sailing.PK;
			billOfLading1.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			var billOfLading2 = Factory.New<BillOfLading>();
			billOfLading2.JS_HouseBill = "OBL001";
			billOfLading2.JS_CFSReference = "";
			billOfLading2.JS_BookingReference = "";
			billOfLading2.JS_JX = sailing.PK;
			billOfLading2.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			Factory.Save();

			var references = AgencyUniversalTestHelper.CreateReferences("OBL001", "BKG001", "AGT001", bookingParty, ZString.Empty, new[] { new SailingReference { VesselName = "MAERSK", VoyageNumber = "001" } });
			var matcher = new AgencyShipmentMatcher<BillOfLading>(Factory, references, new DummyLogger(), DataContextType.BillOfLading);
			var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Ocean Bill Number takes higher priority than Booking Ref, Shipper Ref", billOfLading2.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);
		}

		public void TestBestMatch_AgencyBooking()
		{
			var sailing = UniversalTestHelper.CreateSailingWithVoyage(Factory, "AUSYD", "SGSIN", "MAERSK", "001");
			var agencyBooking1 = GetNewAgencyBooking("", "BKG001", "", sailing);
			var agencyBooking2 = GetNewAgencyBooking("", "", "AGT001", sailing);
			Factory.Save();

			var references = AgencyUniversalTestHelper.CreateReferences("BOL001", "BKG001", "AGT001", null, ZString.Empty, new[] { new SailingReference { VesselName = "MAERSK", VoyageNumber = "001" } });
			var matcher = new AgencyShipmentMatcher<AgencyBooking>(Factory, references, new DummyLogger(), DataContextType.AgencyBooking);
			var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Booking Ref takes higher priority than Shipper Ref", agencyBooking1.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);

			agencyBooking2.JS_CFSReference = "BKG001";
			Factory.Save();
			matcher = new AgencyShipmentMatcher<AgencyBooking>(Factory, references, new DummyLogger(), DataContextType.AgencyBooking);
			(matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Best matched with Booking Ref, Shipper Ref", agencyBooking2.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);
		}

		public void TestBestMatch_AgencyBooking_CarrierBookingReferenceAgentReference()
		{
			var sailing = UniversalTestHelper.CreateSailingWithVoyage(Factory, "AUSYD", "SGSIN", "MAERSK", "001");
			var agencyBooking = GetNewAgencyBooking("", "BKG001", "AGT001", sailing);
			Factory.Save();

			var references = AgencyUniversalTestHelper.CreateReferences("", "BKG001", "", null, ZString.Empty, new[] { new SailingReference { VesselName = "MAERSK", VoyageNumber = "001" } });
			var matcher = new AgencyShipmentMatcher<AgencyBooking>(Factory, references, new DummyLogger(), DataContextType.AgencyBooking);
			var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Should matched the current agency booking", agencyBooking.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);

			references = AgencyUniversalTestHelper.CreateReferences("", "BKG001", "AGT002", null, ZString.Empty, new[] { new SailingReference { VesselName = "MAERSK", VoyageNumber = "001" } });
			matcher = new AgencyShipmentMatcher<AgencyBooking>(Factory, references, new DummyLogger(), DataContextType.AgencyBooking);
			(matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertNull("Should not matched any agency booking", matchedShipment);
			AssertEquals(reason, "No matching agency shipment.");

			references = AgencyUniversalTestHelper.CreateReferences("", "BKG001", "AGT001", null, ZString.Empty, new[] { new SailingReference { VesselName = "MAERSK", VoyageNumber = "001" } });
			matcher = new AgencyShipmentMatcher<AgencyBooking>(Factory, references, new DummyLogger(), DataContextType.AgencyBooking);
			(matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Should matched the current agency booking", agencyBooking.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);
		}

		public void TestBestMatch_AgencyBooking_MatchedAgentReference()
		{
			var sailing = UniversalTestHelper.CreateSailingWithVoyage(Factory, "AUSYD", "SGSIN", "MAERSK", "001");
			var agencyBooking1 = GetNewAgencyBooking("", "BKG001", "", sailing);
			Factory.Save();
			var references = AgencyUniversalTestHelper.CreateReferences("", "", "AGT001", null, ZString.Empty, new[] { new SailingReference { VesselName = "MAERSK", VoyageNumber = "001" } });
			var matcher = new AgencyShipmentMatcher<AgencyBooking>(Factory, references, new DummyLogger(), DataContextType.AgencyBooking);
			var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertNull("Should not matched any agency booking", matchedShipment);
			AssertEquals(reason, "No matching agency shipment.");

			var agencyBooking2 = GetNewAgencyBooking("", "", "AGT001", sailing);
			Factory.Save();
			agencyBooking2.JS_CFSReference = ZString.Empty;
			Factory.Save();
			references = AgencyUniversalTestHelper.CreateReferences("", "", "AGT001", null, ZString.Empty, new[] { new SailingReference { VesselName = "MAERSK", VoyageNumber = "001" } });
			matcher = new AgencyShipmentMatcher<AgencyBooking>(Factory, references, new DummyLogger(), DataContextType.AgencyBooking);
			(matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Should matched the current agency booking", agencyBooking2.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);

			var agencyBooking3 = GetNewAgencyBooking("", "BKG001", "AGT001", sailing);
			Factory.Save();
			references = AgencyUniversalTestHelper.CreateReferences("", "", "AGT002", null, ZString.Empty, new[] { new SailingReference { VesselName = "MAERSK", VoyageNumber = "001" } });
			matcher = new AgencyShipmentMatcher<AgencyBooking>(Factory, references, new DummyLogger(), DataContextType.AgencyBooking);
			(matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertNull("Should not matched any agency booking", matchedShipment);
			AssertEquals(reason, "No matching agency shipment.");

			references = AgencyUniversalTestHelper.CreateReferences("", "", "AGT001", null, ZString.Empty, new[] { new SailingReference { VesselName = "MAERSK", VoyageNumber = "001" } });
			matcher = new AgencyShipmentMatcher<AgencyBooking>(Factory, references, new DummyLogger(), DataContextType.AgencyBooking);
			(matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Should matched the current agency booking", agencyBooking2.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);
		}

		public void TestBestMatch_AgencyBooking_MatchedCarrierBookingReference()
		{
			var sailing = UniversalTestHelper.CreateSailingWithVoyage(Factory, "AUSYD", "SGSIN", "MAERSK", "001");
			var agencyBooking1 = GetNewAgencyBooking("", "", "AGT001", sailing);
			Factory.Save();
			var references = AgencyUniversalTestHelper.CreateReferences("", "BKG001", "", null, ZString.Empty, new[] { new SailingReference { VesselName = "MAERSK", VoyageNumber = "001" } });
			var matcher = new AgencyShipmentMatcher<AgencyBooking>(Factory, references, new DummyLogger(), DataContextType.AgencyBooking);
			var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertNull("Should not matched any agency booking", matchedShipment);
			AssertEquals(reason, "No matching agency shipment.");

			var agencyBooking2 = GetNewAgencyBooking("", "BKG001", "", sailing);
			Factory.Save();
			references = AgencyUniversalTestHelper.CreateReferences("", "BKG001", "", null, ZString.Empty, new[] { new SailingReference { VesselName = "MAERSK", VoyageNumber = "001" } });
			matcher = new AgencyShipmentMatcher<AgencyBooking>(Factory, references, new DummyLogger(), DataContextType.AgencyBooking);
			(matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Should matched the current agency booking", agencyBooking2.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);

			var agencyBooking3 = GetNewAgencyBooking("", "BKG001", "AGT001", sailing);
			Factory.Save();
			references = AgencyUniversalTestHelper.CreateReferences("", "BKG002", "", null, ZString.Empty, new[] { new SailingReference { VesselName = "MAERSK", VoyageNumber = "001" } });
			matcher = new AgencyShipmentMatcher<AgencyBooking>(Factory, references, new DummyLogger(), DataContextType.AgencyBooking);
			(matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertNull("Should not matched any agency booking", matchedShipment);
			AssertEquals(reason, "No matching agency shipment.");

			references = AgencyUniversalTestHelper.CreateReferences("", "BKG001", "", null, ZString.Empty, new[] { new SailingReference { VesselName = "MAERSK", VoyageNumber = "001" } });
			matcher = new AgencyShipmentMatcher<AgencyBooking>(Factory, references, new DummyLogger(), DataContextType.AgencyBooking);
			(matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Should matched the current agency booking", agencyBooking2.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);
		}

		public void TestBestMatch()
		{
			var sailing1 = UniversalTestHelper.CreateSailingWithVoyage(Factory, "AUSYD", "USMIA", "GENERAL FRANCO", "001");
			var sailing2 = UniversalTestHelper.CreateSailingWithVoyage(Factory, "AUSYD", "USMIA", "GENERAL FRANCO", "002");
			var sailing3 = UniversalTestHelper.CreateSailingWithVoyage(Factory, "AUSYD", "NZAKL", "PINOKIO", "001");
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_HouseBill = "S0001";
			var booking = Factory.New<AgencyBooking>();
			booking.JS_HouseBill = "S0002";
			booking.JS_JX = sailing2.PK;
			var billOfLading1 = Factory.New<BillOfLading>();
			billOfLading1.JS_HouseBill = "S0001";
			billOfLading1.JS_JX = sailing1.PK;
			Factory.Save();
			var billOfLading2 = Factory.New<BillOfLading>();
			billOfLading2.JS_HouseBill = "S0001";
			billOfLading2.JS_JX = sailing2.PK;
			var billOfLading3 = Factory.New<BillOfLading>();
			billOfLading3.JS_HouseBill = "S0001";
			billOfLading3.JS_JX = sailing3.PK;
			var billOfLading4 = Factory.New<BillOfLading>();
			billOfLading4.JS_HouseBill = "S0001";
			Factory.Save();

			var references = AgencyUniversalTestHelper.CreateReferences("S0001", new SailingReference { VesselName = "GENERAL FRANCO", VoyageNumber = "002" });
			var matcher = new AgencyShipmentMatcher<BillOfLading>(Factory, references, new DummyLogger(), DataContextType.BillOfLading);
			var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("matches bill of lading with matching ocean bill, vessel and voyage", billOfLading2, matchedShipment);
			AssertEquals(ZString.Empty, reason);
		}

		public void TestBestMatchForBillOfLadingsOnTheSameVoyage()
		{
			var voyage = UniversalTestHelper.CreateSeaVoyage(Factory, "HAKUBA MARU", "001");
			var sailing1 = UniversalTestHelper.AddSailing(voyage, "AUSYD", "SGSIN");
			var sailing2 = UniversalTestHelper.AddSailing(voyage, "SGSIN", "USSFO");
			var billOfLading1 = Factory.New<BillOfLading>();
			billOfLading1.JS_HouseBill = "S0001";
			billOfLading1.JS_JX = sailing1.PK;
			var billOfLading2 = Factory.New<BillOfLading>();
			billOfLading2.JS_HouseBill = "S0002";
			billOfLading2.JS_JX = sailing2.PK;
			Factory.Save();

			var references = AgencyUniversalTestHelper.CreateReferences("S0002", new SailingReference { VesselName = "HAKUBA MARU", VoyageNumber = "001" });
			var matcher = new AgencyShipmentMatcher<BillOfLading>(Factory, references, new DummyLogger(), DataContextType.BillOfLading);
			var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("matches bill of lading with matching ocean bill, vessel and voyage", billOfLading2, matchedShipment);
			AssertEquals(ZString.Empty, reason);
		}

		public void TestNoMatchOnUnknownOceanBill()
		{
			Factory.New<BillOfLading>();
			Factory.Save();
			var references = AgencyUniversalTestHelper.CreateReferences(ZString.Empty, new SailingReference { VesselName = "GENERAL FRANCO", VoyageNumber = "002" });
			var matcher = new AgencyShipmentMatcher<BillOfLading>(Factory, references, new DummyLogger(), DataContextType.BillOfLading);
			var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertNull("matches bill of lading with matching ocean bill, vessel and voyage", matchedShipment);
			AssertEquals(reason, "No matching agency shipment.");
		}

		public void TestBestMatchConflictingVesselNameAndLloyds()
		{
			var vessel1 = Factory.New<RefVessel>();
			vessel1.RV_LloydsNumber = "111";
			vessel1.RV_Name = "WATERMELON";
			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_LloydsNumber = "222";
			vessel2.RV_Name = "MANGO";
			var sailing1 = UniversalTestHelper.CreateSailingWithVoyage(Factory, "AUSYD", "USCHI", vessel1.RV_Name, "001");
			var sailing2 = UniversalTestHelper.CreateSailingWithVoyage(Factory, "AUSYD", "USCHI", vessel2.RV_Name, "001");
			var billOfLading1 = Factory.New<BillOfLading>();
			billOfLading1.JS_HouseBill = "S0001";
			billOfLading1.JS_JX = sailing1.PK;
			billOfLading1.JS_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			var billOfLading2 = Factory.New<BillOfLading>();
			billOfLading2.JS_HouseBill = "S0001";
			billOfLading2.JS_JX = sailing2.PK;
			billOfLading2.JS_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			Factory.Save();

			var references = AgencyUniversalTestHelper.CreateReferences("S0001", new SailingReference { VesselName = "WATERMELON", LloydsNumber = "222", VoyageNumber = "001" });
			var matcher = new AgencyShipmentMatcher<BillOfLading>(Factory, references, new DummyLogger(), DataContextType.BillOfLading);
			var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("lloyds number takes pririty over vessel name", billOfLading2, matchedShipment);
			AssertEquals(ZString.Empty, reason);
		}

		public void TestMatchByLloydsNumber()
		{
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = "12345";
			vessel.RV_Name = "TASMANIA";
			RefVessel vessel1 = Factory.New<RefVessel>();
			vessel1.RV_LloydsNumber = ZString.Empty;
			vessel1.RV_Name = "HOBART";
			var sailing = UniversalTestHelper.CreateSailingWithVoyage(Factory, "AUSYD", "USCHI", vessel.RV_Name, "004");
			var sailing2 = UniversalTestHelper.CreateSailingWithVoyage(Factory, "AUSYD", "USCHI", vessel1.RV_Name, "004");
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_HouseBill = "S0001";
			var booking = Factory.New<BillOfLading>();
			booking.JS_HouseBill = "S0001";
			booking.JS_JX = sailing2.PK;
			var billOfLading = Factory.New<BillOfLading>();
			billOfLading.JS_HouseBill = "S0001";
			billOfLading.JS_JX = sailing.PK;
			Factory.Save();

			var references = AgencyUniversalTestHelper.CreateReferences("S0001", new SailingReference { LloydsNumber = "12345", VoyageNumber = "004" });
			var matcher = new AgencyShipmentMatcher<BillOfLading>(Factory, references, new DummyLogger(), DataContextType.BillOfLading);
			var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("matches bill of lading with matching ocean bill, Lloyds Number", billOfLading,  matchedShipment);
			AssertEquals(ZString.Empty, reason);

			RefVessel vessel2 = Factory.New<RefVessel>();
			vessel2.RV_LloydsNumber = "12345";
			vessel2.RV_Name = "KITES";
			var sailing3 = UniversalTestHelper.CreateSailingWithVoyage(Factory, "AUSYD", "USCHI", vessel2.RV_Name, "004");
			var shipment1 = Factory.New<CommonShipment>();
			shipment1.JS_HouseBill = "S0001";
			var booking1 = Factory.New<BillOfLading>();
			booking1.JS_HouseBill = "S0001";
			booking1.JS_JX = sailing3.PK;
			var billOfLading1 = Factory.New<BillOfLading>();
			billOfLading1.JS_HouseBill = "S0001";
			billOfLading1.JS_JX = sailing.PK;
			Factory.Save();

			var references1 = AgencyUniversalTestHelper.CreateReferences("S0001", new SailingReference { LloydsNumber = "12345", VesselName = "KITES", VoyageNumber = "004" });
			var matcher1 = new AgencyShipmentMatcher<BillOfLading>(Factory, references1, new DummyLogger(), DataContextType.BillOfLading);
			(matchedShipment, reason) = matcher1.GetBestMatchWithReason();
			AssertEquals("matches bill of lading with matching Vessel Name", booking1, matchedShipment);
			AssertEquals(ZString.Empty, reason);

			RefVessel vessel3 = Factory.New<RefVessel>();
			vessel3.RV_LloydsNumber = "4562";
			vessel3.RV_Name = "BALOONS";
			var sailing4 = UniversalTestHelper.CreateSailingWithVoyage(Factory, "AUSYD", "USCHI", vessel3.RV_Name, "004");
			var shipment2 = Factory.New<CommonShipment>();
			shipment2.JS_HouseBill = "S0001";
			var booking2 = Factory.New<BillOfLading>();
			booking2.JS_HouseBill = "S0001";
			booking2.JS_JX = sailing4.PK;
			var billOfLading2 = Factory.New<BillOfLading>();
			billOfLading2.JS_HouseBill = "S0001";
			billOfLading2.JS_JX = sailing3.PK;
			Factory.Save();

			var references2 = AgencyUniversalTestHelper.CreateReferences("S0001", new SailingReference { VesselName = "BALOONS", VoyageNumber = "004" });
			var matcher2 = new AgencyShipmentMatcher<BillOfLading>(Factory, references2, new DummyLogger(), DataContextType.BillOfLading);
			(matchedShipment, reason) = matcher2.GetBestMatchWithReason();
			AssertEquals("matches bill of lading with matching ocean bill, Vessel Name", booking2, matchedShipment);
			AssertEquals(ZString.Empty, reason);

			RefVessel vessel4 = Factory.New<RefVessel>();
			vessel4.RV_LloydsNumber = ZString.Empty;
			vessel4.RV_Name = "CANDIES";
			var sailing5 = UniversalTestHelper.CreateSailingWithVoyage(Factory, "AUSYD", "USCHI", vessel4.RV_Name, "004");
			var shipment3 = Factory.New<CommonShipment>();
			shipment3.JS_HouseBill = "S0001";
			var booking3 = Factory.New<BillOfLading>();
			booking3.JS_HouseBill = "S0001";
			booking3.JS_JX = sailing2.PK;
			var billOfLading3 = Factory.New<BillOfLading>();
			billOfLading3.JS_HouseBill = "S0001";
			billOfLading3.JS_JX = sailing5.PK;
			Factory.Save();

			var references3 = AgencyUniversalTestHelper.CreateReferences("S0001", new SailingReference { LloydsNumber = "99545", VesselName = "CANDIES", VoyageNumber = "004" });
			var matcher3 = new AgencyShipmentMatcher<BillOfLading>(Factory, references3, new DummyLogger(), DataContextType.BillOfLading);
			(matchedShipment, reason) = matcher3.GetBestMatchWithReason();
			AssertEquals("matches bill of lading with matching Vessel Name", billOfLading3, matchedShipment);
			AssertEquals(ZString.Empty, reason);
		}

		public void TestConflictingMatchIsResovledByGettingLatest()
		{
			var sailing1 = UniversalTestHelper.CreateSailingWithVoyage(Factory, "AUSYD", "SGSIN", "ZUMBA", "001");
			var sailing2 = UniversalTestHelper.CreateSailingWithVoyage(Factory, "SGSIN", "USMIA", "KARATE KID", "002");
			var billOfLading1 = GetNewBillOfLading("S0001", "", "", sailing1);
			billOfLading1.JS_SystemCreateTimeUtc = new ZDateTime(2012, 2, 1);
			var billOfLading2 = GetNewBillOfLading("S0002", "", "", sailing2);
			billOfLading2.JS_SystemCreateTimeUtc = new ZDateTime(2012, 2, 2);
			Factory.Save();

			var sailingRefrerence1 = new SailingReference { VesselName = "ZUMBA", VoyageNumber = "001" };
			var sailingRefrerence2 = new SailingReference { VesselName = "KARATE KID", VoyageNumber = "002" };
			var references = AgencyUniversalTestHelper.CreateReferences("S0002", new[] { sailingRefrerence1, sailingRefrerence2 });
			var matcher = new AgencyShipmentMatcher<BillOfLading>(Factory, references, new DummyLogger(), DataContextType.BillOfLading);
			var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("matches latest bill of lading", billOfLading2, matchedShipment);
			AssertEquals(ZString.Empty, reason);
		}

		public void TestGetBestMatch_UniqueConsignRefAndHouseBillOnBookingMatchExist()
		{
			var billOfLading1 = Factory.New<BillOfLading>();
			billOfLading1.JS_HouseBill = "S0001";
			billOfLading1.JS_UniqueConsignRef = "V0001";

			var billOfLading2 = Factory.New<BillOfLading>();
			billOfLading2.JS_HouseBill = "S0002";
			billOfLading2.JS_UniqueConsignRef = "V0002";

			Factory.Save();

			var references = AgencyUniversalTestHelper.CreateReferences("S0001", new SailingReference { VesselName = "TITANIC", VoyageNumber = "001" });
			references.ShipmentID = "V0002";

			var matcher = new AgencyShipmentMatcher<BillOfLading>(Factory, references, new DummyLogger(), DataContextType.BillOfLading);
			var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Matched by UniqueConsignRef as it has priority over HouseBill when SubscriptionType is not specified", billOfLading2, matchedShipment);
			AssertEquals(ZString.Empty, reason);

			var context = new Mock<IXmlEventValueObject>();
			context.Setup(c => c.Context.SubscriptionType).Returns("CarrierBookingReference");
			matcher = new AgencyShipmentMatcher<BillOfLading>(Factory, references, new DummyLogger(), context.Object);
			(matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Matched by UniqueConsignRef as it has priority over HouseBill when SubscriptionType is CarrierBookingReference", billOfLading2, matchedShipment);
			AssertEquals(ZString.Empty, reason);

			context.Setup(c => c.Context.SubscriptionType).Returns("MasterBillNumber");
			matcher = new AgencyShipmentMatcher<BillOfLading>(Factory, references, new DummyLogger(), context.Object);
			(matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Matched by HouseBill as it has priority over UniqueConsignRef when SubscriptionType is MasterBillNumber", billOfLading1, matchedShipment);
			AssertEquals(ZString.Empty, reason);
		}

		public void TestGetBestMatch_BookingReferenceAndHouseBillOnBOLMatchExist_ShouldMatchByHouseBill()
		{
			var voyage = UniversalTestHelper.CreateSeaVoyage(Factory, "TITANIC", "001");
			var sailing = UniversalTestHelper.AddSailing(voyage, "AUSYD", "SGSIN");
			var billOfLading1 = GetNewBillOfLading("S0001", "McLaren", "", sailing);
			var billOfLading2 = GetNewBillOfLading("", "Ron Dennis", "", sailing);
			Factory.Save();
			var references = AgencyUniversalTestHelper.CreateReferences("S0001", "Ron Dennis", new SailingReference { VesselName = "TITANIC", VoyageNumber = "001" });
			var matcher = new AgencyShipmentMatcher<BillOfLading>(Factory, references, new DummyLogger(), DataContextType.BillOfLading);
			var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Matched by HouseBill as it has priority over BookingReference", billOfLading1, matchedShipment);
			AssertEquals(ZString.Empty, reason);
		}

		public void TestGetBestMatch_BookingReferenceAndHouseBillOnBookingMatchExist_ShouldMatchByBookingReference()
		{
			var voyage = UniversalTestHelper.CreateSeaVoyage(Factory, "TITANIC", "001");
			var sailing = UniversalTestHelper.AddSailing(voyage, "AUSYD", "SGSIN");
			var booking1 = GetNewBillOfLading("S0001", "McLaren", "", sailing);
			var booking2 = GetNewBillOfLading("S0002", "Ron Dennis", "", sailing);
			Factory.Save();
			var references = AgencyUniversalTestHelper.CreateReferences("S0001", "Ron Dennis", new SailingReference { VesselName = "TITANIC", VoyageNumber = "001" });
			var matcher = new AgencyShipmentMatcher<BillOfLading>(Factory, references, new DummyLogger(), DataContextType.BillOfLading);
			var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Matched by HouseBill as it has priority over BookingReference", booking1, matchedShipment);
			AssertEquals(ZString.Empty, reason);
		}

		public void TestGetBestMatch_BookingReferenceMatchExist_ShouldFindMatch()
		{
			var voyage = UniversalTestHelper.CreateSeaVoyage(Factory, "TITANIC", "001");
			var sailing1 = UniversalTestHelper.AddSailing(voyage, "AUSYD", "SGSIN");
			var sailing2 = UniversalTestHelper.AddSailing(voyage, "SGSIN", "USSFO");
			var billOfLading1 = GetNewBillOfLading("", "McLaren", "", sailing1);
			var billOfLading2 = GetNewBillOfLading("", "Ron Dennis", "", sailing2);
			Factory.Save();
			var references = AgencyUniversalTestHelper.CreateReferences("S0001", "Ron Dennis", new SailingReference { VesselName = "TITANIC", VoyageNumber = "001" });
			var matcher = new AgencyShipmentMatcher<BillOfLading>(Factory, references, new DummyLogger(), DataContextType.BillOfLading);
			var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Matched by BookingReference", billOfLading2, matchedShipment);
			AssertEquals(ZString.Empty, reason);
		}

		public void TestGetBestMatch_BillOfLadingAgencyBooking()
		{
			var voyage = UniversalTestHelper.CreateSeaVoyage(Factory, "MAERSK", "001");
			var sailing = UniversalTestHelper.AddSailing(voyage, "AUSYD", "SGSIN");
			var billOfLading = Factory.New<BillOfLading>();
			billOfLading.JS_HouseBill = "BOL001";
			billOfLading.JS_JX = sailing.PK;
			billOfLading.JS_SystemCreateTimeUtc = new ZDateTime(2016, 09, 01, 12, 30, 0);
			var agencyBooking = Factory.New<AgencyBooking>();
			agencyBooking.JS_CFSReference = "BKG001";
			agencyBooking.JS_BookingReference = "AGT001";
			agencyBooking.JS_JX = sailing.PK;
			agencyBooking.JS_SystemCreateTimeUtc = new ZDateTime(2016, 09, 01, 13, 30, 0);
			Factory.Save();
			var references = AgencyUniversalTestHelper.CreateReferences("BOL001", "BKG001", "AGT001", null, ZString.Empty, new[] { new SailingReference { VesselName = "MAERSK", VoyageNumber = "001" } });
			var matcher = new AgencyShipmentMatcher<BillOfLading>(Factory, references, new DummyLogger(), DataContextType.BillOfLading);
			var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Matched by Bill Of Lading, OBL has highest priority.", billOfLading.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);

			agencyBooking.JS_HouseBill = "BOL001";
			Factory.Save();
			matcher = new AgencyShipmentMatcher<BillOfLading>(Factory, references, new DummyLogger(), DataContextType.BillOfLading);
			(matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Matched by Agency Booking based on latest creation date.", agencyBooking.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);
		}

		public void TestBuildMatchingQueryAndMatchDelegates_ElectronicBookingAndShippingInstructionsRegistryAndPurpose()
		{
			var voyage = UniversalTestHelper.CreateSeaVoyage(Factory, "MAERSK", "001");
			var sailing = UniversalTestHelper.AddSailing(voyage, "AUSYD", "SGSIN");

			var agencyBooking = Factory.New<AgencyBooking>();
			agencyBooking.JS_CFSReference = "BKG001";
			agencyBooking.JS_BookingReference = "BOL001";
			agencyBooking.JS_JX = sailing.PK;
			agencyBooking.JS_SystemCreateTimeUtc = new ZDateTime(2016, 09, 01, 13, 30, 0);
			Factory.Save();

			var dataContext = new DataContext();
			dataContext.RecipientRoleCollection = new List<RecipientRole>() { new RecipientRole { ServiceCode = ServiceCodeType.BRQ } };
			dataContext.DataTargetCollection = new List<DataTarget>() { new DataTarget { Key = "McLaren", Type = "AgencyBooking" } };
			dataContext.DocumentaryOverride = new DocumentaryOverride { Purpose = new CodeDescriptionPair { Code = "ORG", Description = "Original" } };

			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.ShipmentStatus = new CodeDescriptionPair { Code = ShipmentStatusList.Codes.Booked };
			dataObject.DataContext = dataContext;

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var references = AgencyUniversalTestHelper.CreateReferences(string.Empty, string.Empty, "BOL001", null, ZString.Empty, new[] { new SailingReference { VesselName = "MAERSK", VoyageNumber = "001" } });
				var matcher = new AgencyShipmentMatcher<AgencyBooking>(Factory, references, new DummyLogger(), DataContextType.AgencyBooking);
				var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
				AssertEquals(agencyBooking.PK, matchedShipment.PK);
				AssertEquals(ZString.Empty, reason);
			}

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var references = AgencyUniversalTestHelper.CreateReferences(string.Empty, string.Empty, string.Empty, null, ZString.Empty, new[] { new SailingReference { VesselName = "MAERSK", VoyageNumber = "001" } });
				var matcher = new AgencyShipmentMatcher<AgencyBooking>(Factory, references, new DummyLogger(), DataContextType.AgencyBooking);
				var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
				AssertNull(matchedShipment);
				AssertEquals(reason, "No matching agency shipment.");
			}

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var references = AgencyUniversalTestHelper.CreateReferences(string.Empty, string.Empty, "BOL001", null, ZString.Empty, new[] { new SailingReference { VesselName = "MAERSK", VoyageNumber = "001" } });
				var matcher = new AgencyShipmentMatcher<AgencyBooking>(Factory, references, new DummyLogger(), DataContextType.AgencyBooking);
				var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
				AssertEquals(agencyBooking.PK, matchedShipment.PK);
				AssertEquals(ZString.Empty, reason);
			}

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var references = AgencyUniversalTestHelper.CreateReferences(string.Empty, "BKG001", "BOL001", null, ZString.Empty, new[] { new SailingReference { VesselName = "MAERSK", VoyageNumber = "001" } }, MessagePurposes.Codes.Original);
				var matcher = new AgencyShipmentMatcher<AgencyBooking>(Factory, references, new DummyLogger(), DataContextType.AgencyBooking);
				var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
				AssertEquals(agencyBooking.PK, matchedShipment.PK);
				AssertEquals(ZString.Empty, reason);
			}

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var references = AgencyUniversalTestHelper.CreateReferences(string.Empty, string.Empty, "BOL001", null, ZString.Empty, new[] { new SailingReference { VesselName = "MAERSK", VoyageNumber = "001" } }, MessagePurposes.Codes.Original);
				var matcher = new AgencyShipmentMatcher<AgencyBooking>(Factory, references, new DummyLogger(), DataContextType.AgencyBooking);
				var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
				AssertNull(matchedShipment);
				AssertEquals(reason, "No matching agency shipment.");
			}

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var references = AgencyUniversalTestHelper.CreateReferences(string.Empty, "BKG001", string.Empty, null, ZString.Empty, new[] { new SailingReference { VesselName = "MAERSK", VoyageNumber = "001" } }, MessagePurposes.Codes.Original);
				var matcher = new AgencyShipmentMatcher<AgencyBooking>(Factory, references, new DummyLogger(), DataContextType.AgencyBooking);
				var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
				AssertNull(matchedShipment);
				AssertEquals(reason, "No matching agency shipment.");
			}

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var references = AgencyUniversalTestHelper.CreateReferences(string.Empty, "BKG001", string.Empty, null, ZString.Empty, new[] { new SailingReference { VesselName = "MAERSK", VoyageNumber = "001" } }, MessagePurposes.Codes.Withdrawal);
				var matcher = new AgencyShipmentMatcher<AgencyBooking>(Factory, references, new DummyLogger(), DataContextType.AgencyBooking);
				var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
				AssertEquals(agencyBooking.PK, matchedShipment.PK);
				AssertEquals(ZString.Empty, reason);
			}

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var references = AgencyUniversalTestHelper.CreateReferences(string.Empty, string.Empty, "BOL001", null, ZString.Empty, new[] { new SailingReference { VesselName = "MAERSK", VoyageNumber = "001" } }, MessagePurposes.Codes.Withdrawal);
				var matcher = new AgencyShipmentMatcher<AgencyBooking>(Factory, references, new DummyLogger(), DataContextType.AgencyBooking);
				var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
				AssertEquals(agencyBooking.PK, matchedShipment.PK);
				AssertEquals(ZString.Empty, reason);
			}
		}

		public void TestGetBestMatch_BookingPartyPKAndName()
		{
			var voyage = UniversalTestHelper.CreateSeaVoyage(Factory, "MAERSK", "001");
			var sailing = UniversalTestHelper.AddSailing(voyage, "AUSYD", "SGSIN");
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";
			Factory.Save();
			var agencyBooking1 = Factory.New<AgencyBooking>();
			agencyBooking1.JS_CFSReference = "BKG001";
			agencyBooking1.JS_BookingReference = "AGT001";
			agencyBooking1.JS_JX = sailing.PK;
			agencyBooking1.JS_SystemCreateTimeUtc = new ZDateTime(2016, 09, 01, 13, 30, 0);
			agencyBooking1.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			agencyBooking1.BookingPartyDocumentaryAddress.E2_AddressOverride = true;
			agencyBooking1.BookingPartyDocumentaryAddress.E2_CompanyName = "BKG COMPANY PTY LTD";
			var agencyBooking2 = Factory.New<AgencyBooking>();
			agencyBooking2.JS_CFSReference = "BKG001";
			agencyBooking2.JS_BookingReference = "AGT001";
			agencyBooking2.JS_JX = sailing.PK;
			agencyBooking2.JS_SystemCreateTimeUtc = new ZDateTime(2016, 09, 01, 14, 30, 0);
			agencyBooking2.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			agencyBooking2.BookingPartyDocumentaryAddress.E2_AddressOverride = true;
			agencyBooking2.BookingPartyDocumentaryAddress.E2_CompanyName = "TEST COMPANY";
			var agencyBooking3 = Factory.New<AgencyBooking>();
			agencyBooking3.JS_CFSReference = "BKG001";
			agencyBooking3.JS_BookingReference = "AGT001";
			agencyBooking3.JS_JX = sailing.PK;
			agencyBooking3.JS_SystemCreateTimeUtc = new ZDateTime(2016, 09, 01, 15, 30, 0);
			agencyBooking3.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			Factory.Save();

			var references1 = AgencyUniversalTestHelper.CreateReferences("BOL001", "BKG001", "AGT001", null, bookingParty.OH_FullName, new[] { new SailingReference { VesselName = "MAERSK", VoyageNumber = "001" } });
			var matcher = new AgencyShipmentMatcher<AgencyBooking>(Factory, references1, new DummyLogger(), DataContextType.AgencyBooking);
			var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Matched by Booking Ref, Shipper Ref and overrided Booking Party Company Name", agencyBooking1.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);

			agencyBooking2.BookingPartyDocumentaryAddress.E2_CompanyName = "BKG COMPANY PTY LTD";
			Factory.Save();
			matcher = new AgencyShipmentMatcher<AgencyBooking>(Factory, references1, new DummyLogger(), DataContextType.AgencyBooking);
			(matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Matched by Booking Ref, Shipper Ref and overrided Booking Party Company Name (Latest)", agencyBooking2.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);

			var references2 = AgencyUniversalTestHelper.CreateReferences("BOL001", "BKG001", "AGT001", bookingParty, ZString.Empty, new[] { new SailingReference { VesselName = "MAERSK", VoyageNumber = "001" } });
			matcher = new AgencyShipmentMatcher<AgencyBooking>(Factory, references2, new DummyLogger(), DataContextType.AgencyBooking);
			(matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Matched by Booking Ref, Shipper Ref and Booking Party (Latest)", agencyBooking3.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);
		}

		public void TestBestMatch_AgencyBookingPartyIsNull()
		{
			var voyage = UniversalTestHelper.CreateSeaVoyage(Factory, "MAERSK", "001");
			var sailing = UniversalTestHelper.AddSailing(voyage, "AUSYD", "SGSIN");
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			Factory.Save();

			var agencyBooking1 = Factory.New<AgencyBooking>();
			agencyBooking1.JS_CFSReference = "BKG001";
			agencyBooking1.JS_BookingReference = ZString.Empty;
			agencyBooking1.JS_JX = sailing.PK;
			agencyBooking1.JS_SystemCreateTimeUtc = new ZDateTime(2016, 09, 01, 13, 30, 0);

			var agencyBooking2 = Factory.New<AgencyBooking>();
			agencyBooking2.JS_CFSReference = "BKG001";
			agencyBooking2.JS_BookingReference = ZString.Empty;
			agencyBooking2.JS_JX = sailing.PK;
			agencyBooking2.JS_SystemCreateTimeUtc = new ZDateTime(2016, 09, 01, 14, 30, 0);

			var agencyBooking3 = Factory.New<AgencyBooking>();
			agencyBooking3.JS_CFSReference = "BKG001";
			agencyBooking3.JS_BookingReference = ZString.Empty;
			agencyBooking3.JS_JX = sailing.PK;
			agencyBooking3.JS_SystemCreateTimeUtc = new ZDateTime(2016, 09, 01, 15, 30, 0);

			Factory.Save();

			var references1 = AgencyUniversalTestHelper.CreateReferences("BOL001", "BKG001", "", bookingParty, bookingParty.OH_FullName, new[] { new SailingReference { VesselName = "MAERSK", VoyageNumber = "001" } });
			var matcher = new AgencyShipmentMatcher<AgencyBooking>(Factory, references1, new DummyLogger(), DataContextType.AgencyBooking);
			var (matchedShipment, reason) = matcher.GetBestMatchWithReason();

			AssertEquals("Matched by Booking Ref, Shipper Ref and overrided Booking Party Company Name", agencyBooking3.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);
		}

		public void TestBestMatch_CarrierVGM_AgencyBooking()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestCaseHelper.ClearTable(AgencyShipment.Schema.TableName);

				var references = AgencyUniversalTestHelper.CreateReferences("BOL001", "BKG001", "CTN0001", true);
				var matcher = new AgencyShipmentMatcher<AgencyBooking>(Factory, references, new DummyLogger(), DataContextType.AgencyBooking);

				AssertNull(matcher.GetBestMatch());

				var agencyBooking = Factory.New<AgencyBooking>();
				agencyBooking.JS_CFSReference = ZString.Empty;
				agencyBooking.JS_HouseBill = ZString.Empty;
				agencyBooking.JS_SystemCreateTimeUtc = new ZDateTime(2016, 09, 01, 15, 30, 0);
				Factory.Save();

				matcher = new AgencyShipmentMatcher<AgencyBooking>(new BusinessObjectFactory(), references, new DummyLogger(), DataContextType.AgencyBooking);
				AssertNull(matcher.GetBestMatch());

				var agencyBooking1 = Factory.New<AgencyBooking>();
				agencyBooking1.JS_CFSReference = "BKG001";
				agencyBooking1.JS_HouseBill = ZString.Empty;
				agencyBooking1.JS_SystemCreateTimeUtc = new ZDateTime(2016, 09, 01, 13, 30, 0);

				var agencyBooking2 = Factory.New<AgencyBooking>();
				agencyBooking2.JS_CFSReference = ZString.Empty;
				agencyBooking2.JS_HouseBill = "BOL001";
				agencyBooking2.JS_SystemCreateTimeUtc = new ZDateTime(2016, 09, 01, 14, 30, 0);

				var container = agencyBooking.RealContainers.AddNew();
				container.JC_ContainerNum = "CTN0001";
				Factory.Save();

				matcher = new AgencyShipmentMatcher<AgencyBooking>(new BusinessObjectFactory(), references, new DummyLogger(), DataContextType.AgencyBooking);
				AssertEquals("Matching CarriersBookingReference.", matcher.GetBestMatch().PK, agencyBooking1.PK);

				var agencyBooking1Container = agencyBooking1.BookedContainers.AddNew();
				agencyBooking1Container.JC_ContainerNum = "CTN0001";
				Factory.Save();

				matcher = new AgencyShipmentMatcher<AgencyBooking>(new BusinessObjectFactory(), references, new DummyLogger(), DataContextType.AgencyBooking);
				AssertEquals(matcher.GetBestMatch().PK, agencyBooking1.PK);

				agencyBooking.JS_CFSReference = "BKG001";
				Factory.Save();

				matcher = new AgencyShipmentMatcher<AgencyBooking>(new BusinessObjectFactory(), references, new DummyLogger(), DataContextType.AgencyBooking);
				AssertEquals("RealContainer is higher priority than BookedContainer.", matcher.GetBestMatch().PK, agencyBooking.PK);

				var agencyBooking1Container2 = agencyBooking1.RealContainers.AddNew();
				agencyBooking1Container2.JC_ContainerNum = "CTN0001";
				Factory.Save();

				matcher = new AgencyShipmentMatcher<AgencyBooking>(new BusinessObjectFactory(), references, new DummyLogger(), DataContextType.AgencyBooking);
				AssertEquals("AgencyBooking1 has RealContainers and BookedContainer.", matcher.GetBestMatch().PK, agencyBooking1.PK);

				var container2 = agencyBooking.BookedContainers.AddNew();
				container2.JC_ContainerNum = "CTN0001";
				Factory.Save();

				matcher = new AgencyShipmentMatcher<AgencyBooking>(new BusinessObjectFactory(), references, new DummyLogger(), DataContextType.AgencyBooking);
				AssertEquals("AgencyBooking's JS_SystemCreateTimeUtc is the latest.", matcher.GetBestMatch().PK, agencyBooking.PK);
			}
		}

		public void TestBestMatch_CarrierVGM_BillOfLading()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestCaseHelper.ClearTable(AgencyShipment.Schema.TableName);

				var references = AgencyUniversalTestHelper.CreateReferences("BOL001", "BKG001", "CTN0001", true);
				var matcher = new AgencyShipmentMatcher<AgencyBooking>(Factory, references, new DummyLogger(), DataContextType.BillOfLading);

				AssertNull(matcher.GetBestMatch());

				var billOfLading = Factory.New<BillOfLading>();
				billOfLading.JS_CFSReference = ZString.Empty;
				billOfLading.JS_HouseBill = ZString.Empty;
				billOfLading.JS_SystemCreateTimeUtc = new ZDateTime(2016, 09, 01, 15, 30, 0);
				Factory.Save();

				matcher = new AgencyShipmentMatcher<AgencyBooking>(new BusinessObjectFactory(), references, new DummyLogger(), DataContextType.BillOfLading);
				AssertNull(matcher.GetBestMatch());

				var container = billOfLading.RealContainers.AddNew();
				container.JC_ContainerNum = "CTN0001";

				var billOfLading1 = Factory.New<BillOfLading>();
				billOfLading1.JS_CFSReference = "BKG001";
				billOfLading1.JS_HouseBill = ZString.Empty;
				billOfLading1.JS_SystemCreateTimeUtc = new ZDateTime(2016, 09, 01, 13, 30, 0);
				Factory.Save();

				matcher = new AgencyShipmentMatcher<AgencyBooking>(new BusinessObjectFactory(), references, new DummyLogger(), DataContextType.BillOfLading);
				AssertEquals("Matching CarriersBookingReference", matcher.GetBestMatch().PK, billOfLading1.PK);

				var billOfLading2 = Factory.New<BillOfLading>();
				billOfLading2.JS_CFSReference = ZString.Empty;
				billOfLading2.JS_HouseBill = "BOL001";
				billOfLading2.JS_SystemCreateTimeUtc = new ZDateTime(2016, 09, 01, 14, 30, 0);
				Factory.Save();

				matcher = new AgencyShipmentMatcher<AgencyBooking>(new BusinessObjectFactory(), references, new DummyLogger(), DataContextType.BillOfLading);
				AssertEquals("Matching OceanBill", matcher.GetBestMatch().PK, billOfLading2.PK);

				var billOfLading3 = Factory.New<BillOfLading>();
				billOfLading3.JS_CFSReference = "BKG001";
				billOfLading3.JS_HouseBill = "BOL001";
				billOfLading3.JS_SystemCreateTimeUtc = new ZDateTime(2016, 09, 01, 14, 30, 0);
				Factory.Save();

				matcher = new AgencyShipmentMatcher<AgencyBooking>(new BusinessObjectFactory(), references, new DummyLogger(), DataContextType.BillOfLading);
				AssertEquals("Matching OceanBill and CarriersBookingReference", matcher.GetBestMatch().PK, billOfLading3.PK);

				var billOfLading1Container = billOfLading1.BookedContainers.AddNew();
				billOfLading1Container.JC_ContainerNum = "CTN0001";
				Factory.Save();

				matcher = new AgencyShipmentMatcher<AgencyBooking>(new BusinessObjectFactory(), references, new DummyLogger(), DataContextType.BillOfLading);
				AssertEquals("Matching OceanBill and CarriersBookingReference, RealContainer is necessary for BOL", matcher.GetBestMatch().PK, billOfLading3.PK);

				billOfLading1.RealContainers.AddNew().JC_ContainerNum = "CTN0001";
				Factory.Save();

				matcher = new AgencyShipmentMatcher<AgencyBooking>(new BusinessObjectFactory(), references, new DummyLogger(), DataContextType.BillOfLading);
				AssertEquals("Matching Container and CarriersBookingReference", matcher.GetBestMatch().PK, billOfLading1.PK);

				billOfLading2.RealContainers.AddNew().JC_ContainerNum = "CTN0001";
				Factory.Save();

				matcher = new AgencyShipmentMatcher<AgencyBooking>(new BusinessObjectFactory(), references, new DummyLogger(), DataContextType.BillOfLading);
				AssertEquals("Matching Container and OceanBill", matcher.GetBestMatch().PK, billOfLading2.PK);

				billOfLading3.RealContainers.AddNew().JC_ContainerNum = "CTN0001";
				Factory.Save();

				matcher = new AgencyShipmentMatcher<AgencyBooking>(new BusinessObjectFactory(), references, new DummyLogger(), DataContextType.BillOfLading);
				AssertEquals("Matching Container and OceanBill and CarriersBookingReference", matcher.GetBestMatch().PK, billOfLading3.PK);
			}
		}

		public void TestBestMatch_CarrierVGM_EmptyReference()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestCaseHelper.ClearTable(AgencyShipment.Schema.TableName);

				var references = AgencyUniversalTestHelper.CreateReferences(ZString.Empty, ZString.Empty, "CTN0001", true);
				var matcher = new AgencyShipmentMatcher<AgencyBooking>(Factory, references, new DummyLogger(), DataContextType.BillOfLading);

				AssertNull(matcher.GetBestMatch());

				var billOfLading = Factory.New<BillOfLading>();
				billOfLading.JS_CFSReference = ZString.Empty;
				billOfLading.JS_HouseBill = ZString.Empty;
				billOfLading.JS_SystemCreateTimeUtc = new ZDateTime(2016, 09, 01, 15, 30, 0);
				Factory.Save();

				matcher = new AgencyShipmentMatcher<AgencyBooking>(new BusinessObjectFactory(), references, new DummyLogger(), DataContextType.BillOfLading);
				AssertEquals("Only one", matcher.GetBestMatch().PK, billOfLading.PK);

				var billOfLading1 = Factory.New<BillOfLading>();
				billOfLading1.JS_CFSReference = "BKG001";
				billOfLading1.JS_HouseBill = ZString.Empty;
				billOfLading1.JS_SystemCreateTimeUtc = new ZDateTime(2016, 09, 01, 16, 30, 0);
				Factory.Save();

				matcher = new AgencyShipmentMatcher<AgencyBooking>(new BusinessObjectFactory(), references, new DummyLogger(), DataContextType.BillOfLading);
				AssertEquals("JS_SystemCreateTimeUtc", matcher.GetBestMatch().PK, billOfLading1.PK);

				var billOfLading2 = Factory.New<BillOfLading>();
				billOfLading2.JS_CFSReference = "BKG001";
				billOfLading2.JS_HouseBill = ZString.Empty;
				billOfLading2.RealContainers.AddNew().JC_ContainerNum = "CTN0001";
				billOfLading2.JS_SystemCreateTimeUtc = new ZDateTime(2016, 09, 01, 15, 30, 0);
				Factory.Save();

				matcher = new AgencyShipmentMatcher<AgencyBooking>(new BusinessObjectFactory(), references, new DummyLogger(), DataContextType.BillOfLading);
				AssertEquals("MatchCount - RealContainers", matcher.GetBestMatch().PK, billOfLading2.PK);

				references = AgencyUniversalTestHelper.CreateReferences(ZString.Empty, "BKG002", "CTN0001", true);

				var billOfLading3 = Factory.New<BillOfLading>();
				billOfLading3.JS_CFSReference = "BKG002";
				billOfLading3.JS_HouseBill = ZString.Empty;
				billOfLading3.RealContainers.AddNew().JC_ContainerNum = "CTN0002";
				billOfLading3.JS_SystemCreateTimeUtc = new ZDateTime(2016, 09, 01, 15, 30, 0);
				Factory.Save();

				matcher = new AgencyShipmentMatcher<AgencyBooking>(new BusinessObjectFactory(), references, new DummyLogger(), DataContextType.BillOfLading);
				AssertEquals("CarriersBookingReference is more important than Container number in Sql filter.", matcher.GetBestMatch().PK, billOfLading3.PK);
			}
		}

		#region Implementation

		BillOfLading GetNewBillOfLading(string houseBill, string bookingReference, string agentReference, JobSailing sailing)
		{
			return GetNewAgencyShipment<BillOfLading>(houseBill, bookingReference, agentReference, sailing);
		}

		AgencyBooking GetNewAgencyBooking(string houseBill, string bookingReference, string agentReference, JobSailing sailing)
		{
			return GetNewAgencyShipment<AgencyBooking>(houseBill, bookingReference, agentReference, sailing);
		}

		T GetNewAgencyShipment<T>(string houseBill, string bookingReference, string agentReference, JobSailing sailing)
			where T : AgencyShipment
		{
			var agencyShipment = Factory.New<T>();
			agencyShipment.JS_CFSReference = bookingReference;
			agencyShipment.JS_HouseBill = houseBill;
			agencyShipment.JS_BookingReference = agentReference;
			if (sailing != null)
			{
				agencyShipment.JS_JX = sailing.PK;
			}

			return agencyShipment;
		}

		#endregion
	}
}
