using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.NZ;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders.Testing
{
	public class OCRConsolWrapperTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorThrowsArgumentException()
		{
			new OCRConsolWrapper(null, null, null, "", "", "");
		}

		public void TestOCRConsolWrapper()
		{
			CreateExportSeaConsol();
			var wrappedConsol = new OCRConsolWrapper(Consol, null, null, "", "", "");
			AssertNotNull("OCRConsolWrapper", wrappedConsol);
			var oCRHeader = (IOutwardCargoReportHeader)wrappedConsol;
			AssertNotNull("Consolidator", oCRHeader.Consolidator);
			AssertEquals("IsSea", true, oCRHeader.IsSea);
			AssertEquals("TSWReferenceNumber", "", oCRHeader.TSWReferenceNumber);
			AssertEquals("SenderReferenceNumber", "SIS00039215", oCRHeader.SenderReferenceNumber);
			AssertEquals("MasterBillNumber", "OB528742", oCRHeader.MasterBillNumber);
			AssertEquals("CraftName", "TESTVESL", oCRHeader.CraftName);
			AssertEquals("LloydsNo", "1234567", oCRHeader.LloydsNo);
			AssertEquals("VoyageNo", "175E", oCRHeader.VoyageNo);
			AssertEquals("FlightNo", "", oCRHeader.FlightNo);
			AssertEquals("DepartureDate", new ZDateTime(2013, 07, 18), oCRHeader.DepartureDate);
			AssertNotNull("Carrier", oCRHeader.Carrier);
			AssertEquals("PortOfDeparture", "NZAKL", oCRHeader.PortOfDeparture);
			AssertNotNull("OCRLines", oCRHeader.OCRLines);
			AssertNotNull("Containers", oCRHeader.Containers);
			int expectedRoutingCountries = 1;
			int routingCountries = 0;
			foreach (ZString routingCountry in oCRHeader.RoutingCountryCodes)
			{
				routingCountries++;
				AssertEquals("RoutingCountryCodes", "AU", routingCountry);
			}

			AssertEquals("RoutingCountryCodes", expectedRoutingCountries, routingCountries);
		}

		public void TestOCRConsolWrapperParams()
		{
			var consol = Factory.New<ForwardingConsol>();
			var notifyOrg = OrgHeader.New(Factory);
			notifyOrg.OH_Code = "NZAKLBOND";
			notifyOrg.OH_FullName = "AUCKLAND BOND STORE";
			notifyOrg.MainAddress.OA_Address1 = "TEST CODE FOR NZ CUSTOMS";
			notifyOrg.MainAddress.OA_Address2 = "LOCATED IN NZAKL";
			notifyOrg.OH_RL_NKClosestPort = "NZAKL";
			var address = Factory.New<IOrgAddress>();
			address.OA_OH = notifyOrg.PK;
			address.OA_Address1 = "Address 1";
			address.OA_City = "Sydney";
			dummyWithAddress = Factory.New<DummyWithZAddress>();
			dummyWithAddress.Addy.OrgPK = notifyOrg.PK;
			dummyWithAddress.Z0_Guid = address.PK;
			var deliveryNotificationParty_ZAddress = new ZAddress(dummyWithAddress.Z0_GuidInfo);
			var wrappedConsol = new OCRConsolWrapper(consol, null, deliveryNotificationParty_ZAddress, "TEST PARTY", "test@email.com", "NZAKL");
			AssertNotNull("OCRConsolWrapper", wrappedConsol);
			var ocrHeader = (IOutwardCargoReportHeader)wrappedConsol;
			AssertEquals("NotifyPartyName", "TEST PARTY", ocrHeader.NotifyPartyName);
			AssertEquals("NotifyPartyEmail", "test@email.com", ocrHeader.NotifyPartyEmail);
			AssertEquals("Notify Party Code 0 should be Port code", "NZAKL", ocrHeader.NotifyPartyCodes.ToArray()[0]);
		}

		public void TestOCRItineraryCountryCodes()
		{
			CreateExportSeaConsol();
			var leg1 = Consol.Transports[0];
			leg1.JW_RL_NKLoadPort = "NZWLG";
			leg1.JW_RL_NKDiscPort = "AUBNE";
			leg1.JW_ETD = new ZDateTime(2013, 08, 06);
			leg1.JW_ETA = new ZDateTime(2013, 08, 08);
			var leg2 = Consol.Transports.AddNew();
			leg2.JW_RL_NKLoadPort = "AUBNE";
			leg2.JW_RL_NKDiscPort = "IDJKT";
			leg2.JW_ETD = new ZDateTime(2013, 08, 10);
			leg2.JW_ETA = new ZDateTime(2013, 08, 11);
			var leg3 = Consol.Transports.AddNew();
			leg3.JW_RL_NKLoadPort = "IDJKT";
			leg3.JW_RL_NKDiscPort = "SGSIN";
			leg3.JW_ETD = new ZDateTime(2013, 08, 15);
			leg3.JW_ETA = new ZDateTime(2013, 08, 17);
			var leg4 = Consol.Transports.AddNew();
			leg4.JW_RL_NKLoadPort = "SGSIN";
			leg4.JW_RL_NKDiscPort = "HKHKG";
			leg4.JW_ETD = new ZDateTime(2013, 08, 18);
			leg4.JW_ETA = new ZDateTime(2013, 08, 19);
			var leg5 = Consol.Transports.AddNew();
			leg5.JW_RL_NKLoadPort = "HKHKG";
			leg5.JW_RL_NKDiscPort = "JPTYO";
			leg5.JW_ETD = new ZDateTime(2013, 08, 21);
			leg5.JW_ETA = new ZDateTime(2013, 08, 22);
			var wrappedConsol = new OCRConsolWrapper(Consol, null, null, "", "", "");
			var oCRHeader = (IOutwardCargoReportHeader)wrappedConsol;
			int expectedRoutingCountries = 5;
			int routingCountries = 0;
			string itineraryCountryCodes = ZString.Empty;
			foreach (ZString routingCountry in oCRHeader.RoutingCountryCodes)
			{
				routingCountries++;
				itineraryCountryCodes = itineraryCountryCodes + routingCountry;
			}

			AssertEquals("RoutingCountryCodes", routingCountries, expectedRoutingCountries);
			AssertEquals("RoutingCountryCodes", "AUIDSGHKJP", itineraryCountryCodes);
		}

		public void TestOCRItineraryWhenMultiplePortsInSameCountryVisited()
		{
			CreateExportSeaConsol();
			var leg1 = Consol.Transports[0];
			leg1.JW_RL_NKLoadPort = "NZWLG";
			leg1.JW_RL_NKDiscPort = "AUSYD";
			leg1.JW_ETD = new ZDateTime(2013, 08, 06);
			leg1.JW_ETA = new ZDateTime(2013, 08, 08);
			var leg2 = Consol.Transports.AddNew();
			leg2.JW_RL_NKLoadPort = "AUSYD";
			leg2.JW_RL_NKDiscPort = "AUBNE";
			leg2.JW_ETD = new ZDateTime(2013, 08, 10);
			leg2.JW_ETA = new ZDateTime(2013, 08, 11);
			var leg3 = Consol.Transports.AddNew();
			leg3.JW_RL_NKLoadPort = "AUBNE";
			leg3.JW_RL_NKDiscPort = "AUDWN";
			leg3.JW_ETD = new ZDateTime(2013, 08, 15);
			leg3.JW_ETA = new ZDateTime(2013, 08, 17);
			var leg4 = Consol.Transports.AddNew();
			leg4.JW_RL_NKLoadPort = "AUDWN";
			leg4.JW_RL_NKDiscPort = "IDJKT";
			leg4.JW_ETD = new ZDateTime(2013, 08, 18);
			leg4.JW_ETA = new ZDateTime(2013, 08, 19);
			var leg5 = Consol.Transports.AddNew();
			leg5.JW_RL_NKLoadPort = "IDJKT";
			leg5.JW_RL_NKDiscPort = "SGSIN";
			leg5.JW_ETD = new ZDateTime(2013, 08, 21);
			leg5.JW_ETA = new ZDateTime(2013, 08, 22);
			var wrappedConsol = new OCRConsolWrapper(Consol, null, null, "", "", "");
			var oCRHeader = (IOutwardCargoReportHeader)wrappedConsol;
			int expectedRoutingCountries = 3;
			int routingCountries = 0;
			string itineraryCountryCodes = ZString.Empty;
			foreach (ZString routingCountry in oCRHeader.RoutingCountryCodes)
			{
				routingCountries++;
				itineraryCountryCodes = itineraryCountryCodes + routingCountry;
			}

			AssertEquals("RoutingCountryCodes", expectedRoutingCountries, routingCountries);
			AssertEquals("RoutingCountryCodes should only include AU once despite 3 ports visited.", "AUIDSG", itineraryCountryCodes);
		}

		public void TestMessageLodgedByCarrier()
		{
			CreateExportSeaConsol();
			var wrappedConsol = new OCRConsolWrapper(Consol, null, null, "", "", "");
			AssertNotNull("OCRConsolWrapper", wrappedConsol);
			var oCRHeader = (IOutwardCargoReportHeader)wrappedConsol;
			AssertEquals("IsConsolidation", true, oCRHeader.IsConsolidation);
			var carrierShippingLine = GlbBranch.CurrentBranch.OrgProxy;
			carrierShippingLine.Addresses.AddNewMainAddress();
			var cusCode = carrierShippingLine.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			cusCode.OK_CountryDefault = true;
			cusCode.OK_CustomsRegNo = "51358595A";
			cusCode.OK_RN_NKCodeCountry = "NZ";
			Consol.JK_OA_ShippingLineAddress = carrierShippingLine.MainAddress.PK;
			wrappedConsol = new OCRConsolWrapper(Consol, null, null, "", "", "");
			oCRHeader = wrappedConsol;
			AssertEquals("IsConsolidation should be false when message is being lodged by a Carrier", false, oCRHeader.IsConsolidation);
		}

		public void TestVoyageNumberIsTruncated()
		{
			CreateExportSeaConsol();
			Consol.Transports[0].JW_VoyageFlight = "HAPLLOYD17";
			var wrappedConsol = new OCRConsolWrapper(Consol, null, null, "", "", "");
			AssertNotNull("OCRConsolWrapper", wrappedConsol);
			var oCRHeader = (IOutwardCargoReportHeader)wrappedConsol;
			AssertEquals("VoyageNo should be truncated to 8 characters", "HAPLLOYD", oCRHeader.VoyageNo);
		}

		public void TestSenderReferenceNumberPlaceholder()
		{
			CreateExportSeaConsol();
			var rejectedEntryNumber = Factory.New<CusEntryNumber>();
			rejectedEntryNumber.CE_ParentID = Consol.PK;
			rejectedEntryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			rejectedEntryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.OutwardReportNumber;
			rejectedEntryNumber.CE_EntryStatus = "REJ";
			rejectedEntryNumber.CE_EntryNum = ZString.Empty;
			var wrappedConsol = new OCRConsolWrapper(Consol, null, null, "", "", "");
			AssertNotNull("OCRConsolWrapper", wrappedConsol);
			var oCRHeader = (IOutwardCargoReportHeader)wrappedConsol;
			AssertEquals("SenderReferenceNumber should return Manifest Senders Ref message place holder", "||SNDREFPHLDR||", oCRHeader.SenderReferenceNumber);
		}

		public void TestOutwardReportConsignments()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_MasterBillNum = "APLU13102015";
			var masterShipment = consol.Shipments.AddNew();
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			masterShipment.JS_HouseBill = "1131015";
			masterShipment.CustomsEntryNumberType = "CUS";
			masterShipment.CustomsEntryNumber = "12233";
			var childShipmentOne = masterShipment.CoLoadShipments.AddNew();
			childShipmentOne.JS_HouseBill = "2131015";
			var childShipmentTwo = masterShipment.CoLoadShipments.AddNew();
			childShipmentTwo.JS_HouseBill = "3131015";
			var wrappedConsol = new OCRConsolWrapper(consol, null, null, "", "", "");
			var oCRHeader = (IOutwardCargoReportHeader)wrappedConsol;
			var oCRLines = oCRHeader.OCRLines.ToList();
			AssertEquals(1, oCRLines.Count);
			AssertEquals("1131015", oCRLines[0].BillNumber.BillNumber);
			masterShipment.CustomsEntryNumber = ZString.Empty;
			childShipmentOne.CustomsEntryNumberType = "CUS";
			childShipmentOne.CustomsEntryNumber = "12345";
			childShipmentTwo.CustomsEntryNumberType = "CUS";
			childShipmentTwo.CustomsEntryNumber = "23456";
			oCRLines = oCRHeader.OCRLines.ToList();
			AssertEquals(2, oCRLines.Count);
			AssertEquals("2131015", oCRLines[0].BillNumber.BillNumber);
			AssertEquals("3131015", oCRLines[1].BillNumber.BillNumber);
		}

		public void TestNotifyPartyCode()
		{
			var notifyOrg = OrgHeader.New(Factory);
			notifyOrg.OH_Code = "NZAKLBOND";
			notifyOrg.OH_FullName = "AUCKLAND BOND STORE";
			notifyOrg.MainAddress.OA_Address1 = "TEST CODE FOR NZ CUSTOMS";
			notifyOrg.MainAddress.OA_Address2 = "LOCATED IN NZAKL";
			notifyOrg.OH_RL_NKClosestPort = "NZAKL";
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_MasterBillNum = "APLU13102015";
			var address = Factory.New<IOrgAddress>();
			address.OA_OH = notifyOrg.PK;
			address.OA_Address1 = "Address 1";
			address.OA_City = "Sydney";
			dummyWithAddress = Factory.New<DummyWithZAddress>();
			dummyWithAddress.Addy.OrgPK = notifyOrg.PK;
			dummyWithAddress.Z0_Guid = address.PK;
			ZAddress deliveryNotificationParty_ZAddress = new ZAddress(dummyWithAddress.Z0_GuidInfo);
			var wrappedConsol = new OCRConsolWrapper(consol, null, deliveryNotificationParty_ZAddress, "", "", "");
			var ocrHeader = (IOutwardCargoReportHeader)wrappedConsol;
			AssertEquals("Notify Party Code should be empty", false, ocrHeader.NotifyPartyCodes.Any());
			wrappedConsol = new OCRConsolWrapper(consol, null, deliveryNotificationParty_ZAddress, "", "", "NZAKL");
			ocrHeader = wrappedConsol;
			AssertEquals("Notify Party Code 0 should be Port code", "NZAKL", ocrHeader.NotifyPartyCodes.ToArray()[0]);
			var ccpCusCode = notifyOrg.CustomsCodes.AddNew();
			ccpCusCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			ccpCusCode.OK_OA_PremisesAddress = address.PK;
			ccpCusCode.OK_CustomsRegNo = "79458278";
			ccpCusCode.OK_RN_NKCodeCountry = "NZ";
			wrappedConsol = new OCRConsolWrapper(consol, null, deliveryNotificationParty_ZAddress, "", "", "");
			ocrHeader = wrappedConsol;
			AssertEquals("Notify Party Code 0 should be CCP code", "79458278", ocrHeader.NotifyPartyCodes.ToArray()[0]);
			var ccdCusCode = notifyOrg.CustomsCodes.AddNew();
			ccdCusCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			ccdCusCode.OK_OA_PremisesAddress = address.PK;
			ccdCusCode.OK_CustomsRegNo = "CC001";
			ccdCusCode.OK_RN_NKCodeCountry = "NZ";
			wrappedConsol = new OCRConsolWrapper(consol, null, deliveryNotificationParty_ZAddress, "", "", "NZAKL");
			ocrHeader = wrappedConsol;
			var partyCodes = ocrHeader.NotifyPartyCodes.ToArray();
			AssertEquals("Notify Party Code 0 should be CCP code", "79458278", partyCodes[0]);
			AssertEquals("Notify Party Code 1 should be CCD code", "CC001", partyCodes[1]);
			AssertEquals("Notify Party Code 2 should be Port code", "NZAKL", partyCodes[2]);
			var atfCusCode = notifyOrg.CustomsCodes.AddNew();
			atfCusCode.OK_CodeType = OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility;
			atfCusCode.OK_OA_PremisesAddress = address.PK;
			atfCusCode.OK_CustomsRegNo = "1234Z";
			atfCusCode.OK_RN_NKCodeCountry = "NZ";
			wrappedConsol = new OCRConsolWrapper(consol, null, deliveryNotificationParty_ZAddress, "", "", "NZAKL");
			ocrHeader = wrappedConsol;
			partyCodes = ocrHeader.NotifyPartyCodes.ToArray();
			AssertEquals("Notify Party Code 0 should be CCP code", "79458278", partyCodes[0]);
			AssertEquals("Notify Party Code 1 should now be ATF code", "1234Z", partyCodes[1]);
			AssertEquals("Notify Party Code 2 should be CCD code", "CC001", partyCodes[2]);
			AssertEquals("Notify Party Code 3 should be Port code", "NZAKL", partyCodes[3]);
		}

		protected DummyWithZAddress dummyWithAddress;
		#region Implementation
		protected void CreateExportSeaConsol()
		{
			SetUpShipmentsAndCommonDataForExport();
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Consol.JK_UniqueConsignRef = "SIS00039215";
			Consol.JK_MasterBillNum = "OB528742";
			transport.JW_VoyageFlight = "175E";
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			vessel.RV_Code = "TESTVESL";
			vessel.RV_LloydsNumber = "1234567";
			transport.JW_Vessel = vessel.RV_Code;
			transport.JW_ATD = new ZDateTime(2013, 07, 18);
			transport.JW_ATA = new ZDateTime(2013, 07, 25);
			container = Consol.Containers.AddNew();
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_ContainerNum = "FLMU4928475";
		}

		protected void SetUpShipmentsAndCommonDataForExport()
		{
			shipment = Consol.Shipments.AddNew();
			shipment.JS_HouseBill = "Ship1212";
			shipment.JS_UniqueConsignRef = "SI0004927";
			var notifyOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.StartsWith, "GE INDUSTRIAL"));
			shipment.DocAddresses.AddNew(notifyOrg.MainAddress, DocAddressType.NotifyParty);
			var shippingLine = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Consol.SetDefaultShippingLineAddress(shippingLine);
			Consol.JK_RL_NKLoadPort = "NZAKL";
			Consol.JK_RL_NKDischargePort = "AUSYD";
			transport = Consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2013, 07, 18);
		}

		protected ForwardingConsol Consol
		{
			get
			{
				if (consol == null)
				{
					consol = Factory.New<ForwardingConsol>();
				}

				return consol;
			}
		}
		ForwardingConsol consol;
		CommonShipment shipment;
		Transport transport;
		CommonContainer container;
		#endregion
	}
}
