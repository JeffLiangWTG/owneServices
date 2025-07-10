using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11;
using Enterprise.UniversalDataBuss.Integration;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	internal abstract class AgencyShipmentDataObjectReaderTest<T> : UniversalShipmentDataObjectReaderTest where T : AgencyShipment
	{
		public void TestImportingShipmentWithNoPacklinesDoesNotCreatePackLine()
		{
			var dataObject = GetNewUniversalShipment();
			dataObject.BookingConfirmationReference = testBookingConfirmationReference;
			dataObject.WayBillNumber = "SHP001";
			dataObject.GoodsDescription = "frozen ducks";
			var reader = GetReader(dataObject);
			BusinessObject businessObject = null;
			reader.ReadIntoBusinessObject(ref businessObject);
			var readBizObj = (CommonShipment)businessObject;
			AssertEquals("no packlines have been created", 0, readBizObj.OuterPackLines.Count);
		}

		public void TestPlaceOfReceiptAndDelivery()
		{
			var dataObject = GetNewUniversalShipment();
			dataObject.BookingConfirmationReference = testBookingConfirmationReference;
			dataObject.PlaceOfReceipt = new UNLOCO { Code = "SGSIN" };
			dataObject.PlaceOfDelivery = new UNLOCO { Code = "HKHKC" };
			Action<AgencyShipment, string> assertAction = (readBizObj, message) =>
			{
				AssertEquals("SGSIN", readBizObj.JS_RL_NKPlaceOfReceipt.ToString());
				AssertEquals("HKHKC", readBizObj.JS_RL_NKPlaceOfDischarge.ToString());
			};
			ReadDataObjectAndAssertResult(dataObject, assertAction);
		}

		public void TestReferences()
		{
			var dataObject = GetNewUniversalShipment();
			dataObject.BookingConfirmationReference = testBookingConfirmationReference;
			dataObject.CFSReference = "BOOKINGNUM";
			Action<AgencyShipment, string> assertAction = (readBizObj, message) =>
			{
				AssertEquals("", readBizObj.JS_BookingReference.ToString());
				AssertEquals(testBookingConfirmationReference, readBizObj.JS_CFSReference.ToString());
			};
			ReadDataObjectAndAssertResult(dataObject, assertAction);
		}

		public void TestGoodsValueAndCurrency()
		{
			var dataObject = GetNewUniversalShipment();
			dataObject.BookingConfirmationReference = testBookingConfirmationReference;
			dataObject.GoodsValue = 2015m;
			dataObject.GoodsValueCurrency = new Currency { Code = "RMB" };
			Action<AgencyShipment, string> assertAction = (readBizObj, message) =>
			{
				AssertEquals(2015m, readBizObj.JS_GoodsValue);
				AssertEquals("RMB", readBizObj.JS_RX_NKGoodsValueCurr.ToString());
			};
			ReadDataObjectAndAssertResult(dataObject, assertAction);
		}

		public void TestPaymentMethods()
		{
			var dataObject = GetNewUniversalShipment();
			dataObject.BookingConfirmationReference = testBookingConfirmationReference;
			dataObject.PaymentMethod = new CodeDescriptionPair { Code = Core.Constants.DomesticPaymentTerms.Prepaid, Description = "Prepaid" };
			Action<AgencyShipment, string> assertAction = (readizObj, message) =>
			{
				AssertEquals("Payment Method", Core.Constants.DomesticPaymentTerms.Prepaid, readizObj.JS_INCO);
			};
			ReadDataObjectAndAssertResult(dataObject, assertAction);
		}

		public void TestHouseBillIssuePlace()
		{
			var dataObject = GetNewUniversalShipment();
			dataObject.BookingConfirmationReference = testBookingConfirmationReference;
			dataObject.PlaceOfIssue = new UNLOCO { Code = "AUSYD", Name = "Sydney" };
			Action<AgencyShipment, string> assertAction = (readBizObj, message) =>
			{
				AssertEquals("AUSYD", readBizObj.HouseBillIssuePlace.RL_Code);
				AssertEquals("Sydney", readBizObj.HouseBillIssuePlace.RL_PortName);
			};
			ReadDataObjectAndAssertResult(dataObject, assertAction);
		}

		public void TestMatchExistingSailing()
		{
			var sailing1 = UniversalTestHelper.CreateSailingWithVoyage(Factory.BOFactory, "AUSYD", "SGSIN", "USS ZAMBEZI", "001");
			UniversalTestHelper.CreateSailingWithVoyage(Factory.BOFactory, "SGSIN", "USSFO", "TAIKO", "001");
			Factory.SaveForTesting();
			var dataObject = CreateDataObject();
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{ AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress), OrganizationCode = "BKG", Address1 = "Booking Party Ln" } });
			dataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg> { new TransportLeg { TransportMode = TransportMode.Sea, LegType = LegType.Other, PortOfLoading = new UNLOCO { Code = "AUMEL" }, PortOfDischarge = new UNLOCO { Code = "AUSYD" }, VesselName = "USS ZAMBEZI", VoyageFlightNo = "001" }, new TransportLeg { TransportMode = TransportMode.Sea, LegType = LegType.Main, PortOfLoading = new UNLOCO { Code = "AUSYD" }, PortOfDischarge = new UNLOCO { Code = "SGSIN" }, VesselName = "USS ZAMBEZI", VoyageFlightNo = "001" }, new TransportLeg { TransportMode = TransportMode.Sea, LegType = LegType.Other, PortOfLoading = new UNLOCO { Code = "SGSIN" }, PortOfDischarge = new UNLOCO { Code = "USSFO" }, VesselName = "TAIKO", VoyageFlightNo = "001" } });
			Action<AgencyShipment, string> assertAction = (shipment, message) =>
			{
				AssertEquals("found matching sailing", sailing1.PK, shipment.JS_JX);
				AssertContainsExactElementsInAnyOrder("correct legs imported", new[] { "AUMEL->AUSYD|USS ZAMBEZI|001", "AUSYD->SGSIN|USS ZAMBEZI|001", "SGSIN->USSFO|TAIKO|001", }, shipment.Transports.Cast<Transport>().Select(FormatTransportLeg));
			};
			ReadDataObjectAndAssertResult(dataObject, assertAction);
		}

		public void TestSetCarrier_UpdateFromSailingIfCarrierIsNotSpecified()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "CA1";
			var sailing = UniversalTestHelper.CreateSailingWithVoyage(Factory.BOFactory, "AUSYD", "SGSIN", "USS ZAMBEZI", "001");
			sailing.Voyage.JV_OH_Line = carrier.PK;
			Factory.SaveForTesting();
			var dataObject = CreateDataObject();
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{ AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress), OrganizationCode = "BKG", Address1 = "Booking Party Ln" } });
			Action<AgencyShipment, string> assertAction = (shipment, message) =>
			{
				AssertEquals("Defaulted carrier from sailing", carrier.MainAddress.PK, shipment.JS_OA_BookedShippingLineAddress);
			};
			ReadDataObjectAndAssertResult(dataObject, assertAction);
		}

		public void TestSetCarrier_DoNotUpdateFromSailingIfCarrierIsSpecifiedButInvalid()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "CA1";
			var sailing = UniversalTestHelper.CreateSailingWithVoyage(Factory.BOFactory, "AUSYD", "SGSIN", "USS ZAMBEZI", "001");
			sailing.Voyage.JV_OH_Line = carrier.PK;
			Factory.SaveForTesting();
			var dataObject = CreateDataObject();
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{ AddressType = nameof(DocAddressType.ShippingLineAddress), OrganizationCode = "ZZZ" }, new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{ AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress), OrganizationCode = "BKG", Address1 = "Booking Party Ln" } });
			Action<AgencyShipment, string> assertAction = (shipment, message) =>
			{
				AssertEquals("Did not default from sailing even though it couldn't be found", ZGuid.Empty, shipment.JS_OA_BookedShippingLineAddress);
			};
			ReadDataObjectAndAssertResult(dataObject, assertAction);
		}

		public void TestSetCarrier_DoNotUpdateFromSailingIfCarrierIsValidAndSpecified()
		{
			var carrier1 = Factory.New<OrgHeader>();
			carrier1.OH_Code = "CA1";
			var carrier2 = Factory.New<OrgHeader>();
			carrier2.OH_Code = "CA2";
			var sailing = UniversalTestHelper.CreateSailingWithVoyage(Factory.BOFactory, "AUSYD", "SGSIN", "USS ZAMBEZI", "001");
			sailing.Voyage.JV_OH_Line = carrier1.PK;
			Factory.SaveForTesting();
			var dataObject = CreateDataObject();
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{ AddressType = nameof(DocAddressType.ShippingLineAddress), OrganizationCode = carrier2.OH_Code }, new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{ AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress), OrganizationCode = "BKG", Address1 = "Booking Party Ln" } });
			Action<AgencyShipment, string> assertAction = (shipment, message) =>
			{
				AssertEquals("Defaulted carrier from sailing", carrier2.MainAddress.PK, shipment.JS_OA_BookedShippingLineAddress);
			};
			ReadDataObjectAndAssertResult(dataObject, assertAction);
		}

		UniversalShipment GetNewUniversalShipment()
		{
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>()
			{ new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{ AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress), OrganizationCode = "BKG", Address1 = "Booking Party Ln" } });
			return dataObject;
		}

		string FormatTransportLeg(Transport transport)
		{
			return string.Format("{0}->{1}|{2}|{3}", transport.JW_RL_NKLoadPort, transport.JW_RL_NKDiscPort, transport.JW_Vessel, transport.JW_VoyageFlight);
		}

		#region Impelement
		OrgHeader bookingParty;
		protected override void SetUp()
		{
			base.SetUp();
			bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Ln";
			bookingParty.OH_Code = "BKG";
			Factory.SaveForTesting();
		}

		protected enum AgencyShipmentType
		{
			Booking,
			BillOfLading,
			None
		}

		const string testWayBillNumber = "S0001";
		const string testBookingConfirmationReference = "BKG001";
		protected AgencyShipment ImportForServiceCode(AgencyShipmentType shipmentType, ServiceCodeType? serviceCode, IXmlImportLogger logger, string purpose = "ORG")
		{
			return ImportForServiceCode(shipmentType, serviceCode, null, logger, null, purpose);
		}

		protected AgencyShipment ImportForServiceCode(AgencyShipmentType shipmentType, ServiceCodeType? serviceCode, RecipientRoleType? recipientRoleType, IXmlImportLogger logger, string purpose = "ORG")
		{
			return ImportForServiceCode(shipmentType, serviceCode, recipientRoleType, logger, null, purpose);
		}

		protected AgencyShipment ImportForServiceCode(AgencyShipmentType shipmentType, ServiceCodeType? serviceCode, RecipientRoleType? recipientRoleType, IXmlImportLogger logger, string dataTargetType, string purpose = "ORG")
		{
			if (shipmentType != AgencyShipmentType.None)
			{
				var status = shipmentType == AgencyShipmentType.BillOfLading ? ShipmentStatusList.Codes.Confirmed : ShipmentStatusList.Codes.Booked;
				var booking = CreateAgencyShipment(status);
				booking.JS_BookingReference = "BKG001";
			}

			var dataObject = CreateDataObject(dataTargetType, serviceCode, recipientRoleType, purpose);
			dataObject.AgentsReference = "BKG001";
			var reader = GetReader(dataObject, logger);
			if (reader.DataContextType == DataContextType.AgencyBooking)
			{
				dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{ AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress), OrganizationCode = "BKG", Address1 = "Booking Party Ln" } });
			}

			var readBizObj = reader.ReadIntoBusinessObject() as AgencyShipment;
			return readBizObj;
		}

		protected AgencyShipment CreateAgencyShipment(string shipmentStatus, BusinessObjectFactory factory = null)
		{
			var sailing = UniversalTestHelper.CreateSailingWithVoyage(factory ?? Factory.BOFactory, "AUSYD", "SGSIN", "USS ZAMBEZI", "001");
			var shipment = factory?.New<AgencyShipment>() ?? Factory.New<AgencyShipment>();
			shipment.JS_ShipmentStatus = shipmentStatus;
			shipment.JS_HouseBill = testWayBillNumber;
			shipment.JS_CFSReference = testBookingConfirmationReference;
			shipment.JS_JX = sailing.PK;
			shipment.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			if (factory != null)
			{
				factory.Save();
			}
			else
			{
				Factory.SaveForTesting();
			}

			return shipment;
		}

		protected UniversalShipment CreateDataObject(string targetType = null, ServiceCodeType? serviceCode = null, RecipientRoleType? recipientRoleType = null, string purpose = "ORG", string purposeDescription = "Original")
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = testWayBillNumber,
				BookingConfirmationReference = testBookingConfirmationReference,
				DataContext = new DataContext
				{
					RecipientRoleCollection = new List<RecipientRole> { new RecipientRole { Code = recipientRoleType, ServiceCode = serviceCode } },
					DataTargetCollection = new List<DataTarget> { new DataTarget { Type = targetType } },
					DocumentaryOverride = new DocumentaryOverride { Purpose = new CodeDescriptionPair { Code = purpose, Description = purposeDescription } }
				}
			};
			shipment.SetTransportLegCollection(() => new DataObjectList<TransportLeg>
				{
					new TransportLeg
						{
							TransportMode = TransportMode.Sea,
							LegType = LegType.Main,
							PortOfLoading = new UNLOCO { Code = "AUSYD" },
							PortOfDischarge = new UNLOCO { Code = "SGSIN" },
							VesselName = "USS ZAMBEZI",
							VoyageFlightNo = "001"
						}
				});
			return shipment;
		}

		protected OrgHeader BookingParty
		{
			get
			{
				return bookingParty;
			}
		}

		protected void ReadDataObjectAndAssertResult(UniversalShipment dataObject, Action<AgencyShipment, string> assertAction)
		{
			var logger = new TestErrorLogger();
			var reader = GetReader(dataObject, logger);
			var readBizObj = reader.ReadIntoBusinessObject() as AgencyShipment;
			var msg = string.Join(System.Environment.NewLine, logger.Logs).Trim();
			assertAction(readBizObj, msg);
		}

		protected abstract AgencyShipmentDataObjectReader<T> GetReader(UniversalShipment dataObject, IXmlImportLogger logger);
		#endregion
	}
}
