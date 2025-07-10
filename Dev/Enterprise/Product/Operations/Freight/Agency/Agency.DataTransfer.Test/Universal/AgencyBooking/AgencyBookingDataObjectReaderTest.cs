using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.UniversalDataBuss.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using IncoTerm = Enterprise.UniversalDataBuss.DataObjects.Universal.IncoTerm;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	sealed class AgencyBookingDataObjectReaderTest : AgencyShipmentDataObjectReaderTest<AgencyBooking>
	{
		#region ImportForServiceCode

		#region NoMatch

		public void TestImportForServiceCode_NoMatch_BRQServiceCode()
		{
			var logger = new TestErrorLogger();
			var result = ImportForServiceCode(AgencyShipmentType.None, ServiceCodeType.BRQ, logger);
			AssertNotNull("create new", result);
			var expectedMessage = @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - No matching AgencyBooking found, creating new AgencyBooking.
Information - Populating AgencyBooking...
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: SGSIN
Information - Attempting to get Schedule for the Transport Leg
Information - An existing vessel could not be found. A Schedule will not be created. Attempting to find an existing Schedule...
Information - An existing Schedule could not be found. The Transport Leg will not be linked.
Information - Transport Leg updated.
Information - Added Shipping Shipment  from UniversalShipment.";
			var actualMessage = string.Join(System.Environment.NewLine, logger.Logs).Trim();
			AssertMultilineASCIIEquals("import logs", expectedMessage, actualMessage);
		}

		public void TestImportForServiceCode_NoMatch_NoServiceCode_BillOfLadingTargetExcluded()
		{
			var logger = new TestErrorLogger();
			var result = ImportForServiceCode(AgencyShipmentType.None, null, null, logger, nameof(DataContextType.AgencyBooking));
			AssertNotNull("create new", result);
			var expectedMessage = @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - No matching AgencyBooking found, creating new AgencyBooking.
Information - Populating AgencyBooking...
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: SGSIN
Information - Attempting to get Schedule for the Transport Leg
Information - An existing vessel could not be found. A Schedule will not be created. Attempting to find an existing Schedule...
Information - An existing Schedule could not be found. The Transport Leg will not be linked.
Information - Transport Leg updated.
Information - Added Shipping Booking  from UniversalShipment.";
			var actualMessage = string.Join(System.Environment.NewLine, logger.Logs).Trim();
			AssertMultilineASCIIEquals("import logs", expectedMessage, actualMessage);
		}

		#endregion

		#region MatchBooking

		public void TestImportForServiceCode_MatchBooking_BRQServiceCode()
		{
			var logger = new TestErrorLogger();
			var result = ImportForServiceCode(AgencyShipmentType.Booking, ServiceCodeType.BRQ, logger);
			AssertNotNull("update", result);
			AssertEquals("shoud not be confirmed and change status to EBK", ShipmentStatusList.Codes.ElectronicBooking, result.JS_ShipmentStatus);
			var expectedMessage = @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching AgencyBooking.
Information - Populating AgencyBooking...
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: SGSIN
Information - Attempting to get Schedule for the Transport Leg
Information - A Schedule will not be created. Attempting to find an existing Schedule...
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Information - Updated Shipping Shipment V00001000 from UniversalShipment.";
			var actualMessage = string.Join(System.Environment.NewLine, logger.Logs).Trim();
			AssertMultilineASCIIEquals("import logs", expectedMessage, actualMessage);
		}

		public void TestImportForServiceCode_MatchBooking_VGMServiceCode()
		{
			var logger = new TestErrorLogger();
			var result = ImportForServiceCode(AgencyShipmentType.Booking, ServiceCodeType.VGM, logger);
			AssertNotNull("update", result);
			AssertEquals("shoud not be confirmed", ShipmentStatusList.Codes.Booked, result.JS_ShipmentStatus);
			var expectedMessage = @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching AgencyBooking.
Information - Populating AgencyBooking...
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: SGSIN
Information - Attempting to get Schedule for the Transport Leg
Information - A Schedule will not be created. Attempting to find an existing Schedule...
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Information - Updated Shipping Booking V00001000 from UniversalShipment.";
			var actualMessage = string.Join(System.Environment.NewLine, logger.Logs).Trim();
			AssertMultilineASCIIEquals("import logs", expectedMessage, actualMessage);
		}

		public void TestImportForServiceCode_MatchBooking_NoServiceCode()
		{
			var logger = new TestErrorLogger();
			var result = ImportForServiceCode(AgencyShipmentType.Booking, null, null, logger, null);
			AssertNotNull("update", result);
			AssertEquals("shoud not be confirmed", ShipmentStatusList.Codes.Booked, result.JS_ShipmentStatus);
			var expectedMessage = @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching AgencyBooking.
Information - Populating AgencyBooking...
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: SGSIN
Information - Attempting to get Schedule for the Transport Leg
Information - A Schedule will not be created. Attempting to find an existing Schedule...
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Information - Updated Shipping Booking V00001000 from UniversalShipment.";
			var actualMessage = string.Join(System.Environment.NewLine, logger.Logs).Trim();
			AssertMultilineASCIIEquals("import logs", expectedMessage, actualMessage);
		}

		#endregion

		#region MatchBOL

		public void TestImportForServiceCode_MatchBOL_BRQServiceCode()
		{
			var logger = new TestErrorLogger();
			var result = ImportForServiceCode(AgencyShipmentType.BillOfLading, ServiceCodeType.BRQ, logger);
			AssertNotNull("should matched the BillOfLading but cannot update", result);
			var expectedMessage = @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
Shipping Bill of Lading V00001000 has already been confirmed.";
			var actualMessage = string.Join(System.Environment.NewLine, logger.Logs).Trim();
			AssertMultilineASCIIEquals("import logs", expectedMessage, actualMessage);
		}

		public void TestImportForServiceCode_MatchBOL_BRQServiceCode_CARRecipientRole()
		{
			var logger = new TestErrorLogger();
			var result = ImportForServiceCode(AgencyShipmentType.BillOfLading, ServiceCodeType.BRQ, RecipientRoleType.CAR, logger, string.Empty);
			AssertNotNull("should matched the BillOfLading but cannot update", result);
			var expectedMessage = @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
Shipping Bill of Lading V00001000 has already been confirmed.";
			var actualMessage = string.Join(System.Environment.NewLine, logger.Logs).Trim();
			AssertMultilineASCIIEquals("import logs", expectedMessage, actualMessage);
		}

		public void TestImportForServiceCode_MatchBOL_VGMServiceCode()
		{
			var logger = new TestErrorLogger();
			var result = ImportForServiceCode(AgencyShipmentType.BillOfLading, ServiceCodeType.VGM, logger);
			AssertNull("Should not matched any shipment.", result);
			var expectedMessage = @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Error - Cannot populate AgencyBooking because:
XML file contains VGM service code and cannot find a matched Agency Shipment.";
			var actualMessage = string.Join(System.Environment.NewLine, logger.Logs).Trim();
			AssertMultilineASCIIEquals("import logs", expectedMessage, actualMessage);
		}

		public void TestImportForServiceCode_MatchBOL_NoServiceCode()
		{
			var logger = new TestErrorLogger();
			var result = ImportForServiceCode(AgencyShipmentType.BillOfLading, null, null, logger, null);
			AssertNotNull("should matched the BillOfLading but cannot update", result);
			var expectedMessage = @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
Shipping Bill of Lading V00001000 has already been confirmed.";
			var actualMessage = string.Join(System.Environment.NewLine, logger.Logs).Trim();
			AssertMultilineASCIIEquals("import logs", expectedMessage, actualMessage);
		}

		#endregion

		#endregion

		#region Processing

		public void TestSTUEventHasPurpose()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var booking = CreateAgencyShipment(ShipmentStatusList.Codes.WaitListed);
				booking.JS_BookingReference = "AGT001";
				var dataObject = CreateDataObject(serviceCode: ServiceCodeType.BRQ, recipientRoleType: RecipientRoleType.CAR);
				dataObject.AgentsReference = "AGT001";
				dataObject.ShipmentStatus = new CodeDescriptionPair { Code = ShipmentStatusList.Codes.ElectronicBooking };
				dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
						Address1 = BookingParty.MainAddress.OA_Address1,
						OrganizationCode = BookingParty.OH_Code
					}
				});

				var logger = new TestErrorLogger();
				var reader = GetReader(dataObject, logger);
				var readBizObj = reader.ReadIntoBusinessObject() as AgencyShipment;
				Factory.SaveForTesting();

				AssertEquals("matched booking", booking.PK, booking.PK);
				AssertEquals("should update shipment status", ShipmentStatusList.Codes.ElectronicBooking, booking.JS_ShipmentStatus);
				AssertMultilineASCIIEquals("import logs", @"
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching AgencyBooking.
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Populating AgencyBooking...
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: SGSIN
Information - Attempting to get Schedule for the Transport Leg
Information - A Schedule will not be created. Attempting to find an existing Schedule...
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Information - Updated Shipping Shipment V00001000 from UniversalShipment.
".Trim(), string.Join(System.Environment.NewLine, logger.Logs).Trim());
				AssertEquals("A status changed event should have been logged", "Original|NEW=EBK|OLD=WTL|RES=Electronic Booking Received|TYP=Shipment Status", booking.Logs.MostRecentLogByEventTime(Events.StatusUpdated).SL_Reference);
			}
		}

		public void TestSTUEventHasPurpose_AMD()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var factory = new BusinessObjectFactory();
				var booking = CreateAgencyShipment(ShipmentStatusList.Codes.WaitListed, factory);
				booking.JS_BookingReference = "AGT001";
				factory.Save();

				var dataObject = CreateDataObject(serviceCode: ServiceCodeType.BRQ, recipientRoleType: RecipientRoleType.CAR, purpose: "AMD", purposeDescription: "Amendment");
				dataObject.AgentsReference = "AGT001";
				dataObject.ShipmentStatus = new CodeDescriptionPair { Code = ShipmentStatusList.Codes.ElectronicBooking };
				dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
						Address1 = BookingParty.MainAddress.OA_Address1,
						OrganizationCode = BookingParty.OH_Code
					}
				});

				var logger = new TestErrorLogger();
				var reader = GetReader(dataObject, logger);
				var readBizObj = reader.ReadIntoBusinessObject() as AgencyShipment;
				Factory.SaveForTesting();

				booking = Factory.Load<AgencyShipment>(booking.PK);

				AssertEquals("matched booking", booking.PK, readBizObj.PK);
				AssertEquals("should update shipment status", ShipmentStatusList.Codes.ElectronicBooking, booking.JS_ShipmentStatus);
				AssertMultilineASCIIEquals("import logs", @"
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching AgencyBooking.
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Populating AgencyBooking...
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: SGSIN
Information - Attempting to get Schedule for the Transport Leg
Information - A Schedule will not be created. Attempting to find an existing Schedule...
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Information - Updated Shipping Shipment V00001000 from UniversalShipment.
".Trim(), string.Join(System.Environment.NewLine, logger.Logs).Trim());
				AssertEquals("A status changed event should have been logged", "Amendment|NEW=EBK|OLD=WTL|RES=Electronic Booking Received|TYP=Shipment Status", booking.Logs.MostRecentLogByEventTime(Events.StatusUpdated).SL_Reference);
			}
		}

		public void TestSTUEventHasPurposeWithAttachedDocumentCollection_AMD()
		{
			using (Factory.BOFactory.AddDisposableService())
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var factory = new BusinessObjectFactory();
				var booking = CreateAgencyShipment(ShipmentStatusList.Codes.WaitListed, factory);
				booking.JS_BookingReference = "AGT001";
				factory.Save();

				var dataObject = CreateDataObject(serviceCode: ServiceCodeType.BRQ, recipientRoleType: RecipientRoleType.CAR, purpose: "AMD", purposeDescription: "Amendment");
				dataObject.AgentsReference = "AGT001";
				dataObject.ShipmentStatus = new CodeDescriptionPair { Code = ShipmentStatusList.Codes.ElectronicBooking };
				dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
						Address1 = BookingParty.MainAddress.OA_Address1,
						OrganizationCode = BookingParty.OH_Code
					}
				});
				dataObject.SetAttachedDocumentCollection(() => new List<AttachedDocument>
				{
					AttachedDocumentCreator.Create("Booking Request.pdf","SGVsbG8sIFdvcmxkIQ==","BKG")
				});

				var logger = new TestErrorLogger();
				var reader = GetReader(dataObject, logger);
				var readBizObj = reader.ReadIntoBusinessObject() as AgencyShipment;
				Factory.SaveForTesting();

				booking = Factory.Load<AgencyShipment>(booking.PK);

				AssertMultilineASCIIEquals("import logs", @"
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching AgencyBooking.
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Populating AgencyBooking...
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: SGSIN
Information - Attempting to get Schedule for the Transport Leg
Information - A Schedule will not be created. Attempting to find an existing Schedule...
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Information - Successfully Added eDoc: Booking Request.pdf.
Information - Updated Shipping Shipment V00001000 from UniversalShipment.
".Trim(), string.Join(System.Environment.NewLine, logger.Logs).Trim());

				AssertEquals("matched booking", booking.PK, readBizObj.PK);
				AssertEquals("should update shipment status", ShipmentStatusList.Codes.ElectronicBooking, booking.JS_ShipmentStatus);
				AssertEquals("A status changed event should have been logged", "Amendment|NEW=EBK|OLD=WTL|RES=Electronic Booking Received|TYP=Shipment Status", booking.Logs.MostRecentLogByEventTime(Events.StatusUpdated).SL_Reference);
			}
		}

		public void TestSTUEventHasPurpose_NewBooking()
		{
			var dataObject = CreateDataObject(serviceCode: ServiceCodeType.BRQ, recipientRoleType: RecipientRoleType.CAR);
			dataObject.AgentsReference = "AGT001";
			dataObject.ShipmentStatus = new CodeDescriptionPair { Code = ShipmentStatusList.Codes.ElectronicBooking };
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			var organisationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OrganizationCode = "AUSPIN",
				AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
			};
			dataObject.OrganizationAddressCollection.Add(organisationAddress);
			dataObject.BookingConfirmationReference = ZString.Empty;

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Action<AgencyShipment, string> assertAction = (shipment, message) =>
				{
					AssertEquals("should update shipment status", ShipmentStatusList.Codes.ElectronicBooking, shipment.JS_ShipmentStatus);
					AssertMultilineASCIIEquals("import logs", @"
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'AUSPIN' by code, main address used.
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'AUSPIN' by code, main address used.
Information - No matching AgencyBooking found, creating new AgencyBooking.
Information - Populating AgencyBooking...
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'AUSPIN' by code, main address used.
Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: SGSIN
Information - Attempting to get Schedule for the Transport Leg
Information - An existing vessel could not be found. A Schedule will not be created. Attempting to find an existing Schedule...
Information - An existing Schedule could not be found. The Transport Leg will not be linked.
Information - Transport Leg updated.
Information - Added Shipping Shipment  from UniversalShipment.
".Trim(), message);
				};
				var logger = new TestErrorLogger();
				var reader = GetReader(dataObject, logger);
				var readBizObj = reader.ReadIntoBusinessObject() as AgencyShipment;
				var msg = string.Join(System.Environment.NewLine, logger.Logs).Trim();
				assertAction(readBizObj, msg);
				AssertEquals("Original", readBizObj.PurposeDescription);
				AssertEquals("A status changed event should have been logged", "Original|NEW=EBK|RES=Electronic Booking Received|TYP=Shipment Status", readBizObj.Logs.MostRecentLogByEventTime(Events.StatusUpdated).SL_Reference);
			}

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Action<AgencyShipment, string> assertAction = (shipment, message) =>
				{
					AssertEquals("should update shipment status", ShipmentStatusList.Codes.ElectronicBooking, shipment.JS_ShipmentStatus);
					AssertMultilineASCIIEquals("import logs", @"
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'AUSPIN' by code, main address used.
Information - No matching AgencyBooking found, creating new AgencyBooking.
Information - Populating AgencyBooking...
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'AUSPIN' by code, main address used.
Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: SGSIN
Information - Attempting to get Schedule for the Transport Leg
Information - An existing vessel could not be found. A Schedule will not be created. Attempting to find an existing Schedule...
Information - An existing Schedule could not be found. The Transport Leg will not be linked.
Information - Transport Leg updated.
Information - Added Shipping Shipment  from UniversalShipment.
".Trim(), message);
				};
				var logger = new TestErrorLogger();
				var reader = GetReader(dataObject, logger);
				var readBizObj = reader.ReadIntoBusinessObject() as AgencyShipment;
				var msg = string.Join(System.Environment.NewLine, logger.Logs).Trim();
				assertAction(readBizObj, msg);
				AssertNullOrEmpty(readBizObj.PurposeDescription);

				Factory.SaveForTesting();
				AssertEquals("A status changed event should have been logged", "|NEW=EBK|RES=Electronic Booking Received|TYP=Shipment Status", readBizObj.Logs.MostRecentLogByEventTime(Events.StatusUpdated).SL_Reference);
			}

			dataObject.DataContext.RecipientRoleCollection = new List<RecipientRole>();
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Action<AgencyShipment, string> assertAction = (shipment, message) =>
				{
					AssertEquals("should update shipment status", ShipmentStatusList.Codes.ElectronicBooking, shipment.JS_ShipmentStatus);
					AssertMultilineASCIIEquals("import logs", @"
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'AUSPIN' by code, main address used.
Information - No matching AgencyBooking found, creating new AgencyBooking.
Information - Populating AgencyBooking...
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'AUSPIN' by code, main address used.
Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: SGSIN
Information - Attempting to get Schedule for the Transport Leg
Information - An existing vessel could not be found. A Schedule will not be created. Attempting to find an existing Schedule...
Information - An existing Schedule could not be found. The Transport Leg will not be linked.
Information - Transport Leg updated.
Information - Added Shipping Shipment  from UniversalShipment.
".Trim(), message);
				};
				var logger = new TestErrorLogger();
				var reader = GetReader(dataObject, logger);
				var readBizObj = reader.ReadIntoBusinessObject() as AgencyShipment;
				var msg = string.Join(System.Environment.NewLine, logger.Logs).Trim();
				assertAction(readBizObj, msg);
				AssertNullOrEmpty(readBizObj.PurposeDescription);
				AssertEquals("A status changed event should have been logged", "|NEW=EBK|RES=Electronic Booking Received|TYP=Shipment Status", readBizObj.Logs.MostRecentLogByEventTime(Events.StatusUpdated).SL_Reference);
			}
		}

		public void TestSTUEventDoesNotHavePurpose()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var booking = CreateAgencyShipment(ShipmentStatusList.Codes.Booked);
				booking.JS_BookingReference = "AGT001";
				var dataObject = CreateDataObject();
				dataObject.AgentsReference = "AGT001";
				dataObject.ShipmentStatus = new CodeDescriptionPair { Code = ShipmentStatusList.Codes.WaitListed };
				Action<AgencyShipment, string> assertAction = (shipment, message) =>
				{
					AssertEquals("matched booking", booking.PK, shipment.PK);
					AssertEquals("should update shipment status", ShipmentStatusList.Codes.WaitListed, shipment.JS_ShipmentStatus);
					AssertMultilineASCIIEquals("import logs", @"
Information - Successfully loaded matching AgencyBooking.
Information - Populating AgencyBooking...
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: SGSIN
Information - Attempting to get Schedule for the Transport Leg
Information - A Schedule will not be created. Attempting to find an existing Schedule...
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Information - Updated Shipping Booking V00001000 from UniversalShipment.
".Trim(), message);
				};
				var logger = new TestErrorLogger();
				var reader = GetReader(dataObject, logger);
				var readBizObj = reader.ReadIntoBusinessObject() as AgencyShipment;
				var msg = string.Join(System.Environment.NewLine, logger.Logs).Trim();
				assertAction(readBizObj, msg);
				AssertNullOrEmpty(readBizObj.PurposeDescription);
				AssertEquals("A status changed event should have been logged", "|NEW=WTL|OLD=BKD|TYP=Shipment Status", readBizObj.Logs.MostRecentLogByEventTime(Events.StatusUpdated).SL_Reference);
			}
		}

		public void TestSetShipmentStatus()
		{
			var booking = CreateAgencyShipment(ShipmentStatusList.Codes.Booked);
			booking.JS_BookingReference = "AGT001";
			var dataObject = CreateDataObject();
			dataObject.AgentsReference = "AGT001";
			dataObject.ShipmentStatus = new CodeDescriptionPair { Code = ShipmentStatusList.Codes.WaitListed };
			Action<AgencyShipment, string> assertAction = (shipment, message) =>
			{
				AssertEquals("matched booking", booking.PK, shipment.PK);
				AssertEquals("should update shipment status", ShipmentStatusList.Codes.WaitListed, shipment.JS_ShipmentStatus);
				AssertMultilineASCIIEquals("import logs", @"
Information - Successfully loaded matching AgencyBooking.
Information - Populating AgencyBooking...
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: SGSIN
Information - Attempting to get Schedule for the Transport Leg
Information - A Schedule will not be created. Attempting to find an existing Schedule...
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Information - Updated Shipping Booking V00001000 from UniversalShipment.
".Trim(), message);
			};
			ReadDataObjectAndAssertResult(dataObject, assertAction);
		}

		public void TestMatchedCorrectBookingByUniversalShipment()
		{
			var booking1 = CreateAgencyShipment(ShipmentStatusList.Codes.Booked);
			booking1.JS_CFSReference = "BOOKING REF";
			booking1.JS_BookingReference = "AGT001";
			var booking2 = Factory.New<AgencyBooking>();
			booking2.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			booking2.JS_HouseBill = "S0002";
			booking2.JS_CFSReference = "BOOKING REF";
			booking2.JS_BookingReference = "AGT002";
			Factory.SaveForTesting();
			var dataObject = CreateDataObject();
			dataObject.BookingConfirmationReference = "BOOKING REF";
			dataObject.AgentsReference = "AGT001";
			Action<AgencyShipment, string> assertAction = (shipment, message) =>
			{
				AssertEquals("only match the Booking has linked sailing", booking1.PK, shipment.PK);
				AssertMultilineASCIIEquals("import logs", @"
Information - Successfully loaded matching AgencyBooking.
Information - Populating AgencyBooking...
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: SGSIN
Information - Attempting to get Schedule for the Transport Leg
Information - A Schedule will not be created. Attempting to find an existing Schedule...
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Information - Updated Shipping Booking V00001000 from UniversalShipment.
".Trim(), message);
			};
			ReadDataObjectAndAssertResult(dataObject, assertAction);
		}

		public void TestDisregardEmptyShipmentStatus()
		{
			var booking = CreateAgencyShipment(ShipmentStatusList.Codes.WaitListed);
			booking.JS_BookingReference = "AGT001";
			var dataObject = CreateDataObject();
			dataObject.AgentsReference = "AGT001";
			Action<AgencyShipment, string> assertAction = (shipment, message) =>
			{
				AssertEquals("matched booking", booking.PK, shipment.PK);
				AssertEquals("shipment status is unchanged", ShipmentStatusList.Codes.WaitListed, booking.JS_ShipmentStatus);
				AssertMultilineASCIIEquals("import logs", @"
Information - Successfully loaded matching AgencyBooking.
Information - Populating AgencyBooking...
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: SGSIN
Information - Attempting to get Schedule for the Transport Leg
Information - A Schedule will not be created. Attempting to find an existing Schedule...
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Information - Updated Shipping Booking V00001000 from UniversalShipment.
".Trim(), message);
			};
			ReadDataObjectAndAssertResult(dataObject, assertAction);
		}

		public void TestDisregardExistingBillOfLadings()
		{
			var billOfLading = CreateAgencyShipment(ShipmentStatusList.Codes.Confirmed);
			billOfLading.JS_BookingReference = "AGT001";
			var dataObject = CreateDataObject();
			dataObject.AgentsReference = "AGT001";
			dataObject.ShipmentStatus = new CodeDescriptionPair { Code = ShipmentStatusList.Codes.Booked };
			Action<AgencyShipment, string> assertAction = (shipment, message) =>
			{
				AssertEquals("matched bill of lading", billOfLading.PK, shipment.PK);
				AssertEquals("should not update bill of lading from UniversalShipment", ShipmentStatusList.Codes.Confirmed, billOfLading.JS_ShipmentStatus);
				AssertMultilineASCIIEquals("import logs", @"
Information - Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
Shipping Bill of Lading V00001000 has already been confirmed.
".Trim(), message);
			};
			ReadDataObjectAndAssertResult(dataObject, assertAction);
		}

		public void TestDisregardUniversalShipmentWithBillOfLadingStatus()
		{
			var booking = CreateAgencyShipment(ShipmentStatusList.Codes.Booked);
			booking.JS_BookingReference = "AGT001";
			var dataObject = CreateDataObject();
			dataObject.AgentsReference = "AGT001";
			dataObject.ShipmentStatus = new CodeDescriptionPair { Code = ShipmentStatusList.Codes.Confirmed };
			Action<AgencyShipment, string> assertAction = (shipment, message) =>
			{
				AssertEquals("matched booking", booking.PK, shipment.PK);
				AssertEquals("should not update booking from UniversalShipment", ShipmentStatusList.Codes.Booked, booking.JS_ShipmentStatus);
				AssertMultilineASCIIEquals("import logs", @"
Information - Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
XML file contains shipment status [CNF] that is invalid in this scope.
".Trim(), message);
			};
			ReadDataObjectAndAssertResult(dataObject, assertAction);
		}

		public void TestReadIntoBusinessObject_ElectronicBooking_SetEBKShipmentStatus()
		{
			var booking = Factory.New<AgencyBooking>();
			booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			booking.JS_UniqueConsignRef = "McLaren";

			Factory.SaveForTesting();

			var dataContext = new DataContext();
			dataContext.RecipientRoleCollection = new List<RecipientRole>() { new RecipientRole { ServiceCode = ServiceCodeType.BRQ } };
			dataContext.DataTargetCollection = new List<DataTarget>() { new DataTarget { Key = "McLaren", Type = "AgencyBooking" } };
			dataContext.DocumentaryOverride = new DocumentaryOverride { Purpose = new CodeDescriptionPair { Code = "ORG", Description = "Original" } };

			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.ShipmentStatus = new CodeDescriptionPair { Code = ShipmentStatusList.Codes.Booked };
			dataObject.DataContext = dataContext;

			Action<AgencyShipment, string> assertActionORG = (readBizObj, message) =>
			{
				AssertEquals("matched booking", booking.PK, readBizObj.PK);
				AssertEquals("New shipment status", ShipmentStatusList.Codes.ElectronicBooking, booking.JS_ShipmentStatus);
				AssertMultilineASCIIEquals("import logs", @"
Information - Successfully loaded matching AgencyBooking.
Information - Populating AgencyBooking...
Information - Updated Shipping Shipment McLaren from UniversalShipment.
".Trim(), message);
			};
			ReadDataObjectAndAssertResult(dataObject, assertActionORG);

			dataObject.DataContext.DocumentaryOverride.Purpose.Code = "WTH";
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Action<AgencyShipment, string> assertAction = (readBizObj, message) =>
				{
					AssertEquals("matched booking", booking.PK, readBizObj.PK);
					AssertEquals("Status", ShipmentStatusList.Codes.ElectronicBooking, booking.JS_ShipmentStatus);
					AssertMultilineASCIIEquals("import logs", @"
Information - Successfully loaded matching AgencyBooking.
Information - Populating AgencyBooking...
Information - Updated Shipping Shipment McLaren from UniversalShipment.
	".Trim(), message);
				};
				ReadDataObjectAndAssertResult(dataObject, assertAction);
			}

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Action<AgencyShipment, string> assertActionWTH = (readBizObj, message) =>
				{
					AssertEquals("matched booking", booking.PK, readBizObj.PK);
					AssertEquals("Status", ShipmentStatusList.Codes.EBookingCancellationRequest, booking.JS_ShipmentStatus);
					AssertMultilineASCIIEquals("import logs", @"
Information - Successfully loaded matching AgencyBooking.
Information - Populating AgencyBooking...
Information - Updated Shipping Shipment McLaren from UniversalShipment.
	".Trim(), message);
				};
				ReadDataObjectAndAssertResult(dataObject, assertActionWTH);
			}
		}

		public void TestImportingShipment_JobHeaderIsNull()
		{
			var expectedExceptionMessage = "Could not save Local Client Organization - Failed to create Shipment JobHeader with mutex.";
			var agencyBooking = Factory.New<AgencyBooking>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "TestOrgZz";
			Factory.SaveForTesting();
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.TransportMode = new CodeDescriptionPair()
			{ Code = "SEA", Description = "Sea Freight" };
			dataObject.PortOfOrigin = new UNLOCO()
			{ Code = "NZDUD", Name = "Dunedin" };
			dataObject.PortOfDestination = new UNLOCO()
			{ Code = "AUBDG", Name = "Bendigo" };
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>()
			{ new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{ AddressType = nameof(DocAddressType.LocalClient), OrganizationCode = "TestOrgZz", Address1 = "Local Client" }, new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{ AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress), OrganizationCode = "BKG", Address1 = "Booking Party Ln" } });
			var reader = GetReader(dataObject);
			var bizObj = agencyBooking as BusinessObject;
			AssertNoExceptionThrown(() => reader.ReadIntoBusinessObject(ref bizObj));
			var anotherFactory = new BusinessObjectFactory();
			var bookingInAnotherFactory = anotherFactory.Load<AgencyBooking>(agencyBooking.PK);
			var bizObjInAnotherFactory = bookingInAnotherFactory as BusinessObject;
			AssertExceptionThrown(typeof(DataObjectReadFailureException), expectedExceptionMessage, () => reader.ReadIntoBusinessObject(ref bizObjInAnotherFactory), true);
			var job = new JobHeader.Loader(agencyBooking).TryLoadOrCreate();
			job.Dispose();
		}

		public void TestReadIntoBusinessObject_ElectronicBooking_StatusUpdatedEvent()
		{
			var booking = Factory.New<AgencyBooking>();
			booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			booking.JS_UniqueConsignRef = "Test";

			using (Factory.BOFactory.AddDisposableService())
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var dataContext = new DataContext();
				dataContext.RecipientRoleCollection = new List<RecipientRole>() { new RecipientRole { ServiceCode = ServiceCodeType.BRQ } };
				dataContext.DataTargetCollection = new List<DataTarget>() { new DataTarget { Key = "Test", Type = "AgencyBooking" } };
				dataContext.DocumentaryOverride = new DocumentaryOverride { Purpose = new CodeDescriptionPair { Code = "ORG", Description = "Original" } };

				var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				dataObject.ShipmentStatus = new CodeDescriptionPair { Code = ShipmentStatusList.Codes.Booked };
				var attachedDocuments = new List<AttachedDocument>
				{
					AttachedDocumentCreator.Create(
						fileName: "Booking Request.pdf",
						base64ImageData: "SGVsbG8sIFdvcmxkIQ==",
						documentType: "BKG")
				};

				dataObject.SetAttachedDocumentCollection(() => attachedDocuments);
				dataObject.DataContext = dataContext;

				Action<AgencyShipment, string> assertActionORG = (readBizObj, message) =>
				{
					AssertEquals("matched booking", booking.PK, readBizObj.PK);
					AssertEquals("New shipment status", ShipmentStatusList.Codes.ElectronicBooking, booking.JS_ShipmentStatus);
					AssertMultilineASCIIEquals("import logs", @"
Information - Successfully loaded matching AgencyBooking.
Information - Populating AgencyBooking...
Information - Successfully Added eDoc: Booking Request.pdf.
Information - Updated Shipping Shipment Test from UniversalShipment.
".Trim(), message);
				};
				ReadDataObjectAndAssertResult(dataObject, assertActionORG);
				Factory.SaveForTesting();

				AssertEquals(1, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters["TYP"] == "Shipment Status" && x.Parameters["NEW"] == "EBK"));
			}
		}

		public void TestReadIntoBusinessObject_ElectronicBooking_StatusUpdatedEvent_DisableElectronicBookingAndShippingInstructions()
		{
			var booking = Factory.New<AgencyBooking>();
			booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			booking.JS_UniqueConsignRef = "Test";

			using (Factory.BOFactory.AddDisposableService())
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var dataContext = new DataContext();
				dataContext.RecipientRoleCollection = new List<RecipientRole>() { new RecipientRole { ServiceCode = ServiceCodeType.BRQ } };
				dataContext.DataTargetCollection = new List<DataTarget>() { new DataTarget { Key = "Test", Type = "AgencyBooking" } };
				dataContext.DocumentaryOverride = new DocumentaryOverride { Purpose = new CodeDescriptionPair { Code = "ORG", Description = "Original" } };

				var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				dataObject.ShipmentStatus = new CodeDescriptionPair { Code = ShipmentStatusList.Codes.Booked };
				var attachedDocuments = new List<AttachedDocument>
				{
					AttachedDocumentCreator.Create(
						fileName: "Booking Request.pdf",
						base64ImageData: "SGVsbG8sIFdvcmxkIQ==",
						documentType: "BKG")
				};

				dataObject.SetAttachedDocumentCollection(() => attachedDocuments);
				dataObject.DataContext = dataContext;

				Action<AgencyShipment, string> assertActionORG = (readBizObj, message) =>
				{
					AssertEquals("matched booking", booking.PK, readBizObj.PK);
					AssertEquals("New shipment status", ShipmentStatusList.Codes.ElectronicBooking, booking.JS_ShipmentStatus);
					AssertMultilineASCIIEquals("import logs", @"
Information - Successfully loaded matching AgencyBooking.
Information - Populating AgencyBooking...
Information - Successfully Added eDoc: Booking Request.pdf.
Information - Updated Shipping Shipment Test from UniversalShipment.
".Trim(), message);
				};
				ReadDataObjectAndAssertResult(dataObject, assertActionORG);
				Factory.SaveForTesting();

				AssertEquals(1, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters["TYP"] == "Shipment Status" && x.Parameters["NEW"] == "EBK"));
			}
		}

		#endregion

		#region Validations

		#region ORG and AMD Message

		public void TestBookingPartyDoesNotMatch_ORG_AMD()
		{
			var booking = CreateAgencyShipment(ShipmentStatusList.Codes.ElectronicBooking);
			booking.JS_BookingReference = "AGT001";
			var dataObject = CreateDataObject(serviceCode: ServiceCodeType.BRQ, recipientRoleType: RecipientRoleType.CAR);
			dataObject.AgentsReference = "AGT001";
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
					Address1 = BookingParty.MainAddress.OA_Address1,
					OrganizationCode = "Org Code x",
					CompanyName = "Company x"
				}
			});

			Action<AgencyShipment, string> assertAction = (shipment, message) =>
			{
				AssertMultilineASCIIEquals("import logs", @"Warning - Matching 'BookingPartyDocumentaryAddress':- No match found for '[Org. Code: Org Code x; Company Name: Company x; Address 1: Booking Party Ln]'.
Information - Successfully loaded matching AgencyBooking.
Warning - Matching 'BookingPartyDocumentaryAddress':- No match found for '[Org. Code: Org Code x; Company Name: Company x; Address 1: Booking Party Ln]'.
Error - Cannot populate AgencyBooking because:
[*Booking Request message is received from an unknown Booking Party COMPANY X. Booking Request rejected.*]
".Trim(), message);
			};

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ReadDataObjectAndAssertResult(dataObject, assertAction);

				dataObject.DataContext.DocumentaryOverride.Purpose.Code = "AMD";
				ReadDataObjectAndAssertResult(dataObject, assertAction);
			}
		}

		#endregion

		#region ORG and AMD and WTH Message

		public void TestBookingAlreadyConvertedToBillOfLading_ORG_AMD_WTH()
		{
			var booking = CreateAgencyShipment(ShipmentStatusList.Codes.WebFwdInstruction);
			booking.JS_BookingReference = "AGT001";
			var dataObject = CreateDataObject(serviceCode: ServiceCodeType.BRQ, recipientRoleType: RecipientRoleType.CAR);
			dataObject.DataContext.DocumentaryOverride.Purpose.Code = "ORG";
			dataObject.WayBillNumber = "V00001000";
			dataObject.BookingConfirmationReference = "BKG001";
			dataObject.AgentsReference = "AGT001";
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
					Address1 = BookingParty.MainAddress.OA_Address1,
					OrganizationCode = BookingParty.OH_Code
				}
			});

			Action<AgencyShipment, string> assertActionOriginal = (shipment, message) =>
			{
				AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*Booking Request original message cannot be processed once Booking is converted to Bill Of Lading. Booking Request rejected.*]
".Trim(), message);
			};

			Action<AgencyShipment, string> assertActionAmendment = (shipment, message) =>
			{
				AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching AgencyBooking.
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Error - Cannot populate AgencyBooking because:
[*Booking Request Amendment message cannot be processed once Booking is converted to Bill Of Lading. Booking Request rejected.*]
".Trim(), message);
			};

			Action<AgencyShipment, string> assertActionWithdraw = (shipment, message) =>
			{
				AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*Booking Request Withdraw/Cancel cannot be processed once Booking is converted to Bill of Lading. Booking Request Withdraw rejected.*]
".Trim(), message);
			};

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ReadDataObjectAndAssertResult(dataObject, assertActionOriginal);

				dataObject.DataContext.DocumentaryOverride.Purpose.Code = "AMD";
				ReadDataObjectAndAssertResult(dataObject, assertActionAmendment);

				dataObject.DataContext.DocumentaryOverride.Purpose.Code = "WTH";
				ReadDataObjectAndAssertResult(dataObject, assertActionWithdraw);
			}
		}
		#endregion

		#region ORG Message

		public void TestBookingAlreadyConfirmed_ORG()
		{
			var booking = CreateAgencyShipment(ShipmentStatusList.Codes.Booked);
			booking.JS_BookingReference = "AGT001";
			var dataObject = CreateDataObject(serviceCode: ServiceCodeType.BRQ, recipientRoleType: RecipientRoleType.CAR);
			dataObject.AgentsReference = "AGT001";
			dataObject.DataContext.DocumentaryOverride.Purpose.Code = "ORG";
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
					Address1 = BookingParty.MainAddress.OA_Address1,
					OrganizationCode = BookingParty.OH_Code
				}
			});

			Action<AgencyShipment, string> assertActionEmpty = (shipment, message) =>
			{
				AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*Booking Request original cannot be processed once Booking is confirmed (Booking Status BKD). Booking Request rejected.*]
".Trim(), message);
			};

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ReadDataObjectAndAssertResult(dataObject, assertActionEmpty);
			}
		}

		public void TestBookingNoMatchedBookingAndBookingConfirmationReferenceFilled_ORG()
		{
			var booking = CreateAgencyShipment(ShipmentStatusList.Codes.ElectronicBooking);
			booking.JS_CFSReference = ZString.Empty;
			booking.BookingPartyDocumentaryAddress.CompanyName = "Joske";
			var dataObject = CreateDataObject(serviceCode: ServiceCodeType.BRQ, recipientRoleType: RecipientRoleType.CAR);
			dataObject.AgentsReference = "AGT001";
			dataObject.DataContext.DocumentaryOverride.Purpose.Code = "ORG";
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
					Address1 = BookingParty.MainAddress.OA_Address1,
					OrganizationCode = BookingParty.OH_Code,
					CompanyName = "Joske"
				}
			});

			Action<AgencyShipment, string> assertActionEmpty = (shipment, message) =>
			{
				AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Error - Cannot populate AgencyBooking because:
[*Booking Request message with Booking Number BKG001, Shipper Reference AGT001 and Booking Party JOSKE cannot find a matching Booking to update, Booking Request rejected.*]
".Trim(), message);
			};

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ReadDataObjectAndAssertResult(dataObject, assertActionEmpty);
			}
		}

		#endregion

		#region AMD and WTH Message

		public void TestBookingPartyNameDoesNotMatch_AMD_WTH()
		{
			var booking = CreateAgencyShipment(ShipmentStatusList.Codes.ElectronicBooking);
			var dataObject = CreateDataObject(serviceCode: ServiceCodeType.BRQ, recipientRoleType: RecipientRoleType.CAR);
			dataObject.DataContext.DocumentaryOverride.Purpose.Code = "AMD";
			dataObject.WayBillNumber = "V00001000";
			dataObject.BookingConfirmationReference = "BKG001";
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
					Address1 = BookingParty.MainAddress.OA_Address1,
					OrganizationCode = BookingParty.OH_Code,
					CompanyName = "Companyxxx"
				}
			});

			Action<AgencyShipment, string> assertActionAmendment = (shipment, message) =>
			{
				AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching AgencyBooking.
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Error - Cannot populate AgencyBooking because:
[*Booking Request Amendment message is received with the wrong Booking Party COMPANYXXX. Booking Request rejected.*]
".Trim(), message);
			};
			Action<AgencyShipment, string> assertActionWithdraw = (shipment, message) =>
			{
				AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*Booking Request Withdraw/Cancel message is received with the wrong Booking Party COMPANYXXX. Booking Request rejected.*]
".Trim(), message);
			};

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ReadDataObjectAndAssertResult(dataObject, assertActionAmendment);

				dataObject.DataContext.DocumentaryOverride.Purpose.Code = "WTH";
				ReadDataObjectAndAssertResult(dataObject, assertActionWithdraw);
			}
		}

		public void TestBookingNumberEmpty_AMD_WTH()
		{
			var booking = CreateAgencyShipment(ShipmentStatusList.Codes.ElectronicBooking);
			var dataObject = CreateDataObject(serviceCode: ServiceCodeType.BRQ, recipientRoleType: RecipientRoleType.CAR);
			dataObject.DataContext.DocumentaryOverride.Purpose.Code = "AMD";
			dataObject.WayBillNumber = "V00001000";
			dataObject.BookingConfirmationReference = string.Empty;
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
					Address1 = BookingParty.MainAddress.OA_Address1,
					OrganizationCode = BookingParty.OH_Code
				}
			});

			Action<AgencyShipment, string> assertActionAmendment = (shipment, message) =>
			{
				AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Error - Cannot populate AgencyBooking because:
[*Mandatory Booking Number is missing in Booking Request Amendment message, Booking Request Amendment rejected.*]
".Trim(), message);
			};

			Action<AgencyShipment, string> assertActionWithdraw = (shipment, message) =>
			{
				AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Error - Cannot populate AgencyBooking because:
[*Mandatory Booking Number is missing in Booking Request Withdraw/Cancel message, Booking Request Withdraw rejected.*]
".Trim(), message);
			};

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ReadDataObjectAndAssertResult(dataObject, assertActionAmendment);

				dataObject.DataContext.DocumentaryOverride.Purpose.Code = "WTH";
				ReadDataObjectAndAssertResult(dataObject, assertActionWithdraw);
			}
		}

		public void TestCannotFindMatchingBookingNumber_AMD_WTH()
		{
			var booking = CreateAgencyShipment(ShipmentStatusList.Codes.ElectronicBooking);
			var dataObject = CreateDataObject(serviceCode: ServiceCodeType.BRQ, recipientRoleType: RecipientRoleType.CAR);
			dataObject.DataContext.DocumentaryOverride.Purpose.Code = "AMD";
			dataObject.BookingConfirmationReference = "BKG123456";
			dataObject.AgentsReference = "AgentRefxxx";
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
					Address1 = BookingParty.MainAddress.OA_Address1,
					OrganizationCode = BookingParty.OH_Code,
					CompanyName = "Companyxxx"
				}
			});

			Action<AgencyShipment, string> assertActionAmendment = (shipment, message) =>
			{
				AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Error - Cannot populate AgencyBooking because:
[*Booking Request Amendment message with Booking Number BKG123456, Shipper Reference AgentRefxxx and Booking Party COMPANYXXX cannot find a matching Booking to update, Booking Request Amendment rejected.*]
".Trim(), message);
			};

			Action<AgencyShipment, string> assertActionWithdraw = (shipment, message) =>
			{
				AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Error - Cannot populate AgencyBooking because:
[*Booking Request Withdraw/Cancel message with Booking Number BKG123456, Shipper Reference AgentRefxxx and Booking Party COMPANYXXX cannot find a matching Booking to update, Booking Request Withdraw rejected.*]
".Trim(), message);
			};

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ReadDataObjectAndAssertResult(dataObject, assertActionAmendment);

				dataObject.DataContext.DocumentaryOverride.Purpose.Code = "WTH";
				ReadDataObjectAndAssertResult(dataObject, assertActionWithdraw);
			}
		}

		public void TestShipperReferenceDoesNotMatch_AMD_WTH()
		{
			var booking = CreateAgencyShipment(ShipmentStatusList.Codes.ElectronicBooking);
			var dataObject = CreateDataObject(serviceCode: ServiceCodeType.BRQ, recipientRoleType: RecipientRoleType.CAR);
			dataObject.DataContext.DocumentaryOverride.Purpose.Code = "AMD";
			dataObject.WayBillNumber = "V00001000";
			dataObject.BookingConfirmationReference = "BKG001";
			dataObject.AgentsReference = "AgentRefxxx";
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
					Address1 = BookingParty.MainAddress.OA_Address1,
					OrganizationCode = BookingParty.OH_Code
				}
			});

			Action<AgencyShipment, string> assertActionAmendment = (shipment, message) =>
			{
				AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching AgencyBooking.
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Error - Cannot populate AgencyBooking because:
[*Booking Request message cannot update Booking BKG001 because the Shipper Reference in the message AgentRefxxx is different to one in the Booking. Booking Request rejected.*]
".Trim(), message);
			};

			Action<AgencyShipment, string> assertActionWithdraw = (shipment, message) =>
			{
				AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*Booking Request Withdraw/Cancel message cannot update Booking BKG001 because the Shipper Reference in the message AgentRefxxx is different to one in the Booking. Booking Request Withdraw rejected.*]
".Trim(), message);
			};

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ReadDataObjectAndAssertResult(dataObject, assertActionAmendment);

				dataObject.DataContext.DocumentaryOverride.Purpose.Code = "WTH";
				ReadDataObjectAndAssertResult(dataObject, assertActionWithdraw);
			}
		}

		public void TestBookingAlreadyReceivedShippingInstruction_AMD_WTH()
		{
			var booking = CreateAgencyShipment(ShipmentStatusList.Codes.ElectronicShippingInstruction);
			var dataObject = CreateDataObject(serviceCode: ServiceCodeType.BRQ, recipientRoleType: RecipientRoleType.CAR);
			dataObject.DataContext.DocumentaryOverride.Purpose.Code = "AMD";
			dataObject.WayBillNumber = "V00001000";
			dataObject.BookingConfirmationReference = "BKG001";
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
					Address1 = BookingParty.MainAddress.OA_Address1,
					OrganizationCode = BookingParty.OH_Code
				}
			});

			Action<AgencyShipment, string> assertActionAmendment = (shipment, message) =>
			{
				AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching AgencyBooking.
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Error - Cannot populate AgencyBooking because:
[*Booking Request Amendment message cannot be accepted once Shipping Instruction is processed. Booking Request Amendment rejected.*]
".Trim(), message);
			};
			Action<AgencyShipment, string> assertActionWithdraw = (shipment, message) =>
			{
				AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*Booking Request Withdraw/Cancel cannot be processed once Shipping Instruction is received. Booking Request Withdraw rejected.*]
".Trim(), message);
			};

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ReadDataObjectAndAssertResult(dataObject, assertActionAmendment);

				dataObject.DataContext.DocumentaryOverride.Purpose.Code = "WTH";
				ReadDataObjectAndAssertResult(dataObject, assertActionWithdraw);
			}
		}

		public void TestBookingAlreadyCancelled_AMD_WTH()
		{
			var booking = CreateAgencyShipment(ShipmentStatusList.Codes.BookingCancelled);
			var dataObject = CreateDataObject(serviceCode: ServiceCodeType.BRQ, recipientRoleType: RecipientRoleType.CAR);
			dataObject.DataContext.DocumentaryOverride.Purpose.Code = "AMD";
			dataObject.WayBillNumber = "V00001000";
			dataObject.BookingConfirmationReference = "BKG001";
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
					Address1 = BookingParty.MainAddress.OA_Address1,
					OrganizationCode = BookingParty.OH_Code
				}
			});

			Action<AgencyShipment, string> assertActionAmendment = (shipment, message) =>
			{
				AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching AgencyBooking.
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Error - Cannot populate AgencyBooking because:
[*Booking Request Amendment message cannot be accepted once Booking is Canceled. Booking Request Amendment rejected.*]
".Trim(), message);
			};

			Action<AgencyShipment, string> assertActionWithdraw = (shipment, message) =>
			{
				AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*Booking Request Withdraw/Cancel cannot be accepted once Booking is already Canceled. Booking Request Withdraw rejected.*]
".Trim(), message);
			};

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ReadDataObjectAndAssertResult(dataObject, assertActionAmendment);

				dataObject.DataContext.DocumentaryOverride.Purpose.Code = "WTH";
				ReadDataObjectAndAssertResult(dataObject, assertActionWithdraw);
			}
		}

		public void TestBookingAlreadyInProgress_AMD_WTH()
		{
			var booking = CreateAgencyShipment(ShipmentStatusList.Codes.EBookingCancellationRequest);
			var dataObject = CreateDataObject(serviceCode: ServiceCodeType.BRQ, recipientRoleType: RecipientRoleType.CAR);
			dataObject.DataContext.DocumentaryOverride.Purpose.Code = "AMD";
			dataObject.WayBillNumber = "V00001000";
			dataObject.BookingConfirmationReference = "BKG001";
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
					Address1 = BookingParty.MainAddress.OA_Address1,
					OrganizationCode = BookingParty.OH_Code
				}
			});

			Action<AgencyShipment, string> assertActionAmendment = (shipment, message) =>
			{
				AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching AgencyBooking.
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Error - Cannot populate AgencyBooking because:
[*Booking Request Amendment message cannot be accepted once Booking Withdrawal/Cancellation Request is in progress. Booking Request Amendment rejected.*]
".Trim(), message);
			};
			Action<AgencyShipment, string> assertActionWithdraw = (shipment, message) =>
			{
				AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*Booking Request Withdraw/Cancel cannot be accepted once Booking Withdrawal/Cancellation request is already in progress. Booking Request Withdraw rejected.*]
".Trim(), message);
			};

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ReadDataObjectAndAssertResult(dataObject, assertActionAmendment);

				dataObject.DataContext.DocumentaryOverride.Purpose.Code = "WTH";
				ReadDataObjectAndAssertResult(dataObject, assertActionWithdraw);
			}
		}

		#endregion

		#region WTH Message

		public void TestBookingAlreadyRejected_WTH()
		{
			var booking = CreateAgencyShipment(ShipmentStatusList.Codes.BookingRejected);
			var dataObject = CreateDataObject(serviceCode: ServiceCodeType.BRQ, recipientRoleType: RecipientRoleType.CAR);
			dataObject.DataContext.DocumentaryOverride.Purpose.Code = "WTH";
			dataObject.WayBillNumber = "V00001000";
			dataObject.BookingConfirmationReference = "BKG001";
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
					Address1 = BookingParty.MainAddress.OA_Address1,
					OrganizationCode = BookingParty.OH_Code
				}
			});

			Action<AgencyShipment, string> assertAction = (shipment, message) =>
			{
				AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*Booking Request Withdraw/Cancel cannot be processed once Booking is rejected. Booking Request Withdraw rejected.*]
".Trim(), message);
			};

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ReadDataObjectAndAssertResult(dataObject, assertAction);
			}
		}

		public void TestBookingAlreadyAccepted_WTH()
		{
			var booking = CreateAgencyShipment(ShipmentStatusList.Codes.Confirmed);
			var dataObject = CreateDataObject(serviceCode: ServiceCodeType.BRQ, recipientRoleType: RecipientRoleType.CAR);
			dataObject.DataContext.DocumentaryOverride.Purpose.Code = "WTH";
			dataObject.WayBillNumber = "V00001000";
			dataObject.BookingConfirmationReference = "BKG001";
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
					Address1 = BookingParty.MainAddress.OA_Address1,
					OrganizationCode = BookingParty.OH_Code
				}
			});

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Action<AgencyShipment, string> assertAction = (shipment, message) =>
				{
					AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*Booking Request Withdraw/Cancel cannot be processed once Shipping Instruction is accepted. Booking Request Withdraw rejected.*]
".Trim(), message);
				};

				ReadDataObjectAndAssertResult(dataObject, assertAction);
			}

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Action<AgencyShipment, string> assertAction = (shipment, message) =>
				{
					AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
Shipping Bill of Lading V00001000 has already been confirmed.
".Trim(), message);
				};

				ReadDataObjectAndAssertResult(dataObject, assertAction);
			}
		}

		public void TestShippingInstructionRejected_WTH()
		{
			var booking = CreateAgencyShipment(ShipmentStatusList.Codes.SIRejected);
			var dataObject = CreateDataObject(serviceCode: ServiceCodeType.BRQ, recipientRoleType: RecipientRoleType.CAR);
			dataObject.DataContext.DocumentaryOverride.Purpose.Code = "WTH";
			dataObject.WayBillNumber = "V00001000";
			dataObject.BookingConfirmationReference = "BKG001";
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
					Address1 = BookingParty.MainAddress.OA_Address1,
					OrganizationCode = BookingParty.OH_Code
				}
			});

			Action<AgencyShipment, string> assertAction = (shipment, message) =>
			{
				AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*Booking Request Withdraw/Cancel cannot be processed once Shipping Instruction is rejected. Booking Request Withdraw rejected.*]
".Trim(), message);
			};

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ReadDataObjectAndAssertResult(dataObject, assertAction);
			}
		}

		#endregion

		#region Carrier VGM Message

		public void TestImportCarrierVGMUniversalShipment_BookingAndBillOfLadingNumberAreEmpty()
		{
			#region xml

			var xml = @"<UniversalShipment>
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>CAR</Code>
				<Description>Carrier</Description>
				<ServiceCode>VGM</ServiceCode>
				<ServiceDescription>Verified Gross Container Weight</ServiceDescription>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<AgentsReference>SH0001</AgentsReference>
	<OrganizationAddressCollection>
		<OrganizationAddress>
			<AddressType>BookingPartyDocumentaryAddress</AddressType>
			<OrganizationCode>BKGPARTY</OrganizationCode>
			<Address1>Booking Party Address 1</Address1>
		</OrganizationAddress>
	</OrganizationAddressCollection>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TEST1111111</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
				<ISOCode>20GP</ISOCode>
			</ContainerType>
			<GrossWeight>25378.000</GrossWeight>
			<GrossWeightVerificationDateTime>2024-01-28T08:45:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>CNT</Code>
			</GrossWeightVerificationType>
			<Seal />
			<WeightUnit>
				<Code>KG</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>BKGPARTY</OrganizationCode>
					<Address1>Booking Party Address 1</Address1>
				</OrganizationAddress>
			</OrganizationAddressCollection>
		</Container>
	</ContainerCollection>
  </Shipment>
</UniversalShipment>";

			#endregion

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var message = GetQueuedUniversalShipmentMessage(xml);

				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(message);
				AssertMultilineASCIIEquals(@"Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate AgencyBooking because:
[*VGM message received without Booking or Bill of Lading number. Booking or Bill of Lading Number is mandatory to process the VGM. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());

				CreateAgencyBooking("BK0001", "TEST1111111", ShipmentStatusList.Codes.Booked, "SH0001");

				manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				message = GetQueuedUniversalShipmentMessage(xml);
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*VGM message received without Booking or Bill of Lading number. Booking or Bill of Lading Number is mandatory to process the VGM. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());
			}
		}

		public void TestImportCarrierVGMUniversalShipment_InvalidBookingNumber()
		{
			#region xml

			var xml = @"<UniversalShipment>
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>CAR</Code>
				<Description>Carrier</Description>
				<ServiceCode>VGM</ServiceCode>
				<ServiceDescription>Verified Gross Container Weight</ServiceDescription>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<BookingConfirmationReference>BK03262024</BookingConfirmationReference>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TEST1111112</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
				<ISOCode>20GP</ISOCode>
			</ContainerType>
			<GrossWeight>25378.000</GrossWeight>
			<GrossWeightVerificationDateTime>2024-01-28T08:45:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>CNT</Code>
			</GrossWeightVerificationType>
			<Seal />
			<WeightUnit>
				<Code>KG</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>BKGPARTY</OrganizationCode>
					<Address1>Booking Party Address 1</Address1>
				</OrganizationAddress>
			</OrganizationAddressCollection>
		</Container>
	</ContainerCollection>
  </Shipment>
</UniversalShipment>";

			#endregion

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var message = GetQueuedUniversalShipmentMessage(xml);

				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Error - Cannot populate AgencyBooking because:
[*VGM message with Booking Number BK03262024 cannot find a matching Booking. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());

				var agencyBooking = CreateAgencyBooking("BK00010", "TEST1111112", ShipmentStatusList.Codes.Booked);
				agencyBooking.JS_HouseBill = "BL00010";
				Factory.SaveForTesting();

				manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				message = GetQueuedUniversalShipmentMessage(xml);
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Error - Cannot populate AgencyBooking because:
[*VGM message with Booking Number BK03262024 cannot find a matching Booking. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());
			}
		}

		public void TestImportCarrierVGMUniversalShipment_InvalidBillNumber()
		{
			#region xml

			var xml = @"<UniversalShipment>
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>CAR</Code>
				<Description>Carrier</Description>
				<ServiceCode>VGM</ServiceCode>
				<ServiceDescription>Verified Gross Container Weight</ServiceDescription>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<BookingConfirmationReference>BK03262024</BookingConfirmationReference>
	<WayBillNumber>BL00010</WayBillNumber>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TEST1111112</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
				<ISOCode>20GP</ISOCode>
			</ContainerType>
			<GrossWeight>25378.000</GrossWeight>
			<GrossWeightVerificationDateTime>2024-01-28T08:45:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>CNT</Code>
			</GrossWeightVerificationType>
			<Seal />
			<WeightUnit>
				<Code>KG</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>BKGPARTY</OrganizationCode>
					<Address1>Booking Party Address 1</Address1>
				</OrganizationAddress>
			</OrganizationAddressCollection>
		</Container>
	</ContainerCollection>
  </Shipment>
</UniversalShipment>";

			var xmlWithoutBookingConfirmationReference = @"<UniversalShipment>
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>CAR</Code>
				<Description>Carrier</Description>
				<ServiceCode>VGM</ServiceCode>
				<ServiceDescription>Verified Gross Container Weight</ServiceDescription>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<WayBillNumber>BL00010</WayBillNumber>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TEST1111112</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
				<ISOCode>20GP</ISOCode>
			</ContainerType>
			<GrossWeight>25378.000</GrossWeight>
			<GrossWeightVerificationDateTime>2024-01-28T08:45:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>CNT</Code>
			</GrossWeightVerificationType>
			<Seal />
			<WeightUnit>
				<Code>KG</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>BKGPARTY</OrganizationCode>
					<Address1>Booking Party Address 1</Address1>
				</OrganizationAddress>
			</OrganizationAddressCollection>
		</Container>
	</ContainerCollection>
  </Shipment>
</UniversalShipment>";

			#endregion

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var message = GetQueuedUniversalShipmentMessage(xml);

				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Error - Cannot populate AgencyBooking because:
[*VGM message with Bill Number BL00010 cannot find a matching Bill of Lading to update. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());

				manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				message = GetQueuedUniversalShipmentMessage(xmlWithoutBookingConfirmationReference);
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Error - Cannot populate AgencyBooking because:
[*VGM message with Bill Number BL00010 cannot find a matching Bill of Lading to update. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());

				var agencyBooking = CreateAgencyBooking("BK00010", "TEST1111112", ShipmentStatusList.Codes.Booked);
				agencyBooking.JS_HouseBill = "BL00010";
				Factory.SaveForTesting();

				manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				message = GetQueuedUniversalShipmentMessage(xml);
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Error - Cannot populate AgencyBooking because:
[*VGM message with Bill Number BL00010 cannot find a matching Bill of Lading to update. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());

				manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				message = GetQueuedUniversalShipmentMessage(xmlWithoutBookingConfirmationReference);
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*VGM message with Bill Number BL00010 cannot find a matching Bill of Lading to update. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());
			}
		}

		public void TestImportCarrierVGMUniversalShipment_ContainerNumberIsEmpty()
		{
			#region xml

			var xml = @"<UniversalShipment>
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>CAR</Code>
				<Description>Carrier</Description>
				<ServiceCode>VGM</ServiceCode>
				<ServiceDescription>Verified Gross Container Weight</ServiceDescription>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<BookingConfirmationReference>BK0002</BookingConfirmationReference>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerType>
				<Code>20GP</Code>
				<ISOCode>20GP</ISOCode>
			</ContainerType>
			<GrossWeight>25378.000</GrossWeight>
			<GrossWeightVerificationDateTime>2024-01-28T08:45:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>CNT</Code>
			</GrossWeightVerificationType>
			<Seal />
			<WeightUnit>
				<Code>KG</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>BKGPARTY</OrganizationCode>
					<Address1>Booking Party Address 1</Address1>
				</OrganizationAddress>
			</OrganizationAddressCollection>
		</Container>
	</ContainerCollection>
  </Shipment>
</UniversalShipment>";

			#endregion

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var message = GetQueuedUniversalShipmentMessage(xml);

				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Error - Cannot populate AgencyBooking because:
[*VGM message received without Container Number. Container Number is mandatory to process the VGM. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());

				CreateAgencyBooking("BK0002", "", ShipmentStatusList.Codes.Booked);

				manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				message = GetQueuedUniversalShipmentMessage(xml);
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*VGM message received without Container Number. Container Number is mandatory to process the VGM. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());
			}
		}

		public void TestImportCarrierVGMUniversalShipment_CannotMatchContainerNumber()
		{
			#region xml

			var xml = @"<UniversalShipment>
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>CAR</Code>
				<Description>Carrier</Description>
				<ServiceCode>VGM</ServiceCode>
				<ServiceDescription>Verified Gross Container Weight</ServiceDescription>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<BookingConfirmationReference>BK0002</BookingConfirmationReference>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TEST1111114</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
				<ISOCode>20GP</ISOCode>
			</ContainerType>
			<GrossWeight>25378.000</GrossWeight>
			<GrossWeightVerificationDateTime>2024-01-28T08:45:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>CNT</Code>
			</GrossWeightVerificationType>
			<Seal />
			<WeightUnit>
				<Code>KG</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>BKGPARTY</OrganizationCode>
					<Address1>Booking Party Address 1</Address1>
				</OrganizationAddress>
			</OrganizationAddressCollection>
		</Container>
	</ContainerCollection>
  </Shipment>
</UniversalShipment>";

			#endregion

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var message = GetQueuedUniversalShipmentMessage(xml);

				var agencyBooking = CreateAgencyBooking("BK0002", "TEST1111115", ShipmentStatusList.Codes.Booked);

				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*VGM message failed because Container TEST1111114 does not exist in Booking BK0002. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());

				agencyBooking.JS_CFSReference = ZString.Empty;
				Factory.SaveForTesting();

				message = GetQueuedUniversalShipmentMessage(xml);
				manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Error - Cannot populate AgencyBooking because:
[*VGM message with Booking Number BK0002 cannot find a matching Booking. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());
			}
		}

		public void TestImportCarrierVGMUniversalShipment_BookingAndBillOfLadingNumberAndContainerNumberAreEmpty()
		{
			#region xml

			var xml = @"<UniversalShipment>
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>CAR</Code>
				<Description>Carrier</Description>
				<ServiceCode>VGM</ServiceCode>
				<ServiceDescription>Verified Gross Container Weight</ServiceDescription>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<BookingConfirmationReference></BookingConfirmationReference>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerType>
				<Code>20GP</Code>
				<ISOCode>20GP</ISOCode>
			</ContainerType>
			<GrossWeight>25378.000</GrossWeight>
			<GrossWeightVerificationDateTime>2024-01-28T08:45:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>CNT</Code>
			</GrossWeightVerificationType>
			<Seal />
			<WeightUnit>
				<Code>KG</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>BKGPARTY</OrganizationCode>
					<Address1>Booking Party Address 1</Address1>
				</OrganizationAddress>
			</OrganizationAddressCollection>
		</Container>
	</ContainerCollection>
  </Shipment>
</UniversalShipment>";

			#endregion

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var message = GetQueuedUniversalShipmentMessage(xml);

				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Error - Cannot populate AgencyBooking because:
[*VGM message received without Container Number. Container Number is mandatory to process the VGM. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());

				CreateAgencyBooking("", "", ShipmentStatusList.Codes.Booked);

				manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				message = GetQueuedUniversalShipmentMessage(xml);
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*VGM message received without Container Number. Container Number is mandatory to process the VGM. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());

				CreateAgencyBooking("BK00010", "TEST1111112", ShipmentStatusList.Codes.Booked, addSailing: false);

				manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				message = GetQueuedUniversalShipmentMessage(xml);
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*VGM message received without Container Number. Container Number is mandatory to process the VGM. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());
			}
		}

		public void TestVGMImport_CoLoadShipment_MBL_Reject_WhenVerificationTypeIsMissing()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var newFactory = new BusinessObjectFactory();

				var agencyBooking = CreateAgencyBooking("BK0002", "TEST1111115", ShipmentStatusList.Codes.Booked);

				AssertVerificationType(Core.Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified, verificationTime: ZString.Empty,  shouldBeRejected: false, "BK0002", "TEST1111115");
				AssertVerificationType(Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages, "2018-06-06T13:00:00", shouldBeRejected: false, "BK0002", "TEST1111115");
				AssertVerificationType(ZString.Empty, verificationTime: "2018-06-06T13:00:00", shouldBeRejected: true, "BK0002", "TEST1111115");
				AssertVerificationType(null, verificationTime: "2018-06-06T13:00:00", shouldBeRejected: true, "BK0002", "TEST1111115");
			}
		}

		void AssertVerificationType(ZString verificationType, ZString verificationTime, bool shouldBeRejected, string booking, string container)
		{
			#region XML Message

			string xmlMessage = $@"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>CAR</Code>
				<Description>Carrier</Description>
				<ServiceCode>VGM</ServiceCode>
				<ServiceDescription>Verified Gross Container Weight</ServiceDescription>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<BookingConfirmationReference>{booking}</BookingConfirmationReference>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>{container}</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
			</ContainerType>
			<GrossWeight>4500</GrossWeight>
			<GrossWeightVerificationDateTime>{verificationTime}</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>{verificationType}</Code>
			</GrossWeightVerificationType>
			<Seal>1108829</Seal>
			<WeightUnit>
				<Code>KG</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>TESTORG1</OrganizationCode>
					<Address1>Test Address 1</Address1>
				</OrganizationAddress>
			</OrganizationAddressCollection>
		</Container>
	</ContainerCollection>
  </Shipment>
</UniversalShipment>";

			#endregion

			var message = GetQueuedUniversalShipmentMessage(xmlMessage);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			if (shouldBeRejected)
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Error, message.EM_Status);
				AssertContains("Service Task Log", @"'GrossWeightVerificationType' not found in XML, Gross Weight, Unit, Verified By, Verified Date will not be imported.", manager.Logger.ToString(), ignoreCase: true);
			}
			else
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);
			}
		}

		public void TestVGMImport_CoLoadShipment_MBL_Reject_WhenGrossWeightIsEmptyOrZeroForVerifiedContainer()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var agencyBooking = CreateAgencyBooking("BK0002", "TEST1111115", ShipmentStatusList.Codes.Booked);

				AssertVerifiedContainer(Core.Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified, verificationTime: ZString.Empty, weight: "5400", shouldBeRejected: false, "BK0002", "TEST1111115");
				AssertVerifiedContainer(Core.Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified, verificationTime: "2018-06-06T13:00:00", weight: ZString.Empty, shouldBeRejected: true, "BK0002", "TEST1111115");
				AssertVerifiedContainer(Core.Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified, verificationTime: ZString.Empty, weight: "5400", shouldBeRejected: false, "BK0002", "TEST1111115");
				AssertVerifiedContainer(Core.Constants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal, verificationTime: "2018-06-06T13:00:00", weight: ZString.Empty, shouldBeRejected: true, "BK0002", "TEST1111115");
				AssertVerifiedContainer(Core.Constants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal, verificationTime: "2018-06-06T13:00:00", weight: "0", shouldBeRejected: true, "BK0002", "TEST1111115");
				AssertVerifiedContainer(Core.Constants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal, verificationTime: ZString.Empty, weight: "5400", shouldBeRejected: true, "BK0002", "TEST1111115");
				AssertVerifiedContainer(Core.Constants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal, verificationTime: "2018-06-06T13:00:00", weight: "5400", shouldBeRejected: false, "BK0002", "TEST1111115");
				AssertVerifiedContainer(Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container, verificationTime: "2018-06-06T13:00:00", weight: ZString.Empty, shouldBeRejected: true, "BK0002", "TEST1111115");
				AssertVerifiedContainer(Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container, verificationTime: "2018-06-06T13:00:00", weight: "0", shouldBeRejected: true, "BK0002", "TEST1111115");
				AssertVerifiedContainer(Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container, verificationTime: ZString.Empty, weight: "5400", shouldBeRejected: true, "BK0002", "TEST1111115");
				AssertVerifiedContainer(Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container, verificationTime: "2018-06-06T13:00:00", weight: "5400", shouldBeRejected: false, "BK0002", "TEST1111115");
				AssertVerifiedContainer(Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages, verificationTime: "2018-06-06T13:00:00", weight: ZString.Empty, shouldBeRejected: true, "BK0002", "TEST1111115");
				AssertVerifiedContainer(Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages, verificationTime: "2018-06-06T13:00:00", weight: "0", shouldBeRejected: true, "BK0002", "TEST1111115");
				AssertVerifiedContainer(Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages, verificationTime: ZString.Empty, weight: "5400", shouldBeRejected: true, "BK0002", "TEST1111115");
				AssertVerifiedContainer(Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages, verificationTime: "2018-06-06T13:00:00", weight: "5400", shouldBeRejected: false, "BK0002", "TEST1111115");
				AssertVerifiedContainer(Core.Constants.ContainerGrossWeightVerificationTypes.Codes.RationalMethod, verificationTime: "2018-06-06T13:00:00", weight: ZString.Empty, shouldBeRejected: true, "BK0002", "TEST1111115");
				AssertVerifiedContainer(Core.Constants.ContainerGrossWeightVerificationTypes.Codes.RationalMethod, verificationTime: "2018-06-06T13:00:00", weight: "0", shouldBeRejected: true, "BK0002", "TEST1111115");
				AssertVerifiedContainer(Core.Constants.ContainerGrossWeightVerificationTypes.Codes.RationalMethod, verificationTime: ZString.Empty, weight: "5400", shouldBeRejected: true, "BK0002", "TEST1111115");
				AssertVerifiedContainer(Core.Constants.ContainerGrossWeightVerificationTypes.Codes.RationalMethod, verificationTime: "2018-06-06T13:00:00", weight: "5400", shouldBeRejected: false, "BK0002", "TEST1111115");
				AssertVerifiedContainer(Core.Constants.ContainerGrossWeightVerificationTypes.Codes.NotRequired, verificationTime: "2018-06-06T13:00:00", weight: ZString.Empty, shouldBeRejected: true, "BK0002", "TEST1111115");
				AssertVerifiedContainer(Core.Constants.ContainerGrossWeightVerificationTypes.Codes.NotRequired, verificationTime: "2018-06-06T13:00:00", weight: "0", shouldBeRejected: true, "BK0002", "TEST1111115");
				AssertVerifiedContainer(Core.Constants.ContainerGrossWeightVerificationTypes.Codes.NotRequired, verificationTime: ZString.Empty, weight: "5400", shouldBeRejected: true, "BK0002", "TEST1111115");
				AssertVerifiedContainer(Core.Constants.ContainerGrossWeightVerificationTypes.Codes.NotRequired, verificationTime: "2018-06-06T13:00:00", weight: "5400", shouldBeRejected: false, "BK0002", "TEST1111115");
			}
		}

		void AssertVerifiedContainer(ZString verificationType, ZString verificationTime, ZString weight, bool shouldBeRejected, string booking, string container)
		{
			#region XML Message

			string xmlMessage = $@"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>CAR</Code>
				<Description>Carrier</Description>
				<ServiceCode>VGM</ServiceCode>
				<ServiceDescription>Verified Gross Container Weight</ServiceDescription>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<BookingConfirmationReference>{booking}</BookingConfirmationReference>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>{container}</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
			</ContainerType>
			<GrossWeight>{weight}</GrossWeight>
			<GrossWeightVerificationDateTime>{verificationTime}</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>{verificationType}</Code>
			</GrossWeightVerificationType>
			<Seal>1108829</Seal>
			<WeightUnit>
				<Code>KG</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>TESTORG1</OrganizationCode>
					<Address1>Test Address 1</Address1>
				</OrganizationAddress>
			</OrganizationAddressCollection>
		</Container>
	</ContainerCollection>
  </Shipment>
</UniversalShipment>";

			#endregion

			var message = GetQueuedUniversalShipmentMessage(xmlMessage);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			if (shouldBeRejected)
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Error, message.EM_Status);
				AssertContains("Service Task Log", @"
When element 'GrossWeightVerificationType' is 'NON' then element 'GrossWeightVerificationDateTime' will not be imported.
When element 'GrossWeightVerificationType' is not 'NON' then 'GrossWeightVerificationDateTime' must be entered and 'GrossWeight' must be greater than 0.".Trim(), manager.Logger.ToString());
			}
			else
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);
			}
		}

		public void TestImportCarrierVGMUniversalShipment_ElectronicBooking()
		{
			#region message

			var message = GetQueuedUniversalShipmentMessage(@"<UniversalShipment>
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>CAR</Code>
				<Description>Carrier</Description>
				<ServiceCode>VGM</ServiceCode>
				<ServiceDescription>Verified Gross Container Weight</ServiceDescription>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<BookingConfirmationReference>BK0004</BookingConfirmationReference>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TEST1111114</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
				<ISOCode>20GP</ISOCode>
			</ContainerType>
			<GrossWeight>6000.000</GrossWeight>
			<GrossWeightVerificationDateTime>2024-03-13T08:45:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>CNT</Code>
			</GrossWeightVerificationType>
			<Seal />
			<WeightUnit>
				<Code>KG</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>BKGPARTY</OrganizationCode>
					<Address1>Booking Party Address 1</Address1>
				</OrganizationAddress>
			</OrganizationAddressCollection>
		</Container>
	</ContainerCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CreateAgencyBooking("BK0004", "TEST1111114", ShipmentStatusList.Codes.ElectronicBooking);

				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*VGM message with Booking number BK0004 cannot be processed because the Booking is not confirmed. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());
			}
		}

		public void TestImportCarrierVGMUniversalShipment_InvalidContainerNumber()
		{
			#region message

			var message = GetQueuedUniversalShipmentMessage(@"<UniversalShipment>
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>CAR</Code>
				<Description>Carrier</Description>
				<ServiceCode>VGM</ServiceCode>
				<ServiceDescription>Verified Gross Container Weight</ServiceDescription>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<BookingConfirmationReference>BK0005</BookingConfirmationReference>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TEST1649263</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
				<ISOCode>20GP</ISOCode>
			</ContainerType>
			<GrossWeight>6000.000</GrossWeight>
			<GrossWeightVerificationDateTime>2024-03-13T08:45:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>CNT</Code>
			</GrossWeightVerificationType>
			<Seal />
			<WeightUnit>
				<Code>KG</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>BKGPARTY</OrganizationCode>
					<Address1>Booking Party Address 1</Address1>
				</OrganizationAddress>
			</OrganizationAddressCollection>
		</Container>
	</ContainerCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CreateAgencyBooking("BK0005", "TEST1111115", ShipmentStatusList.Codes.Booked);

				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*VGM message failed because Container TEST1649263 does not exist in Booking BK0005. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());
			}
		}

		public void TestImportCarrierVGMUniversalShipment_BookingCancelled()
		{
			#region message

			var message = GetQueuedUniversalShipmentMessage(@"<UniversalShipment>
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>CAR</Code>
				<Description>Carrier</Description>
				<ServiceCode>VGM</ServiceCode>
				<ServiceDescription>Verified Gross Container Weight</ServiceDescription>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<BookingConfirmationReference>BK0006</BookingConfirmationReference>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TEST1111116</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
				<ISOCode>20GP</ISOCode>
			</ContainerType>
			<GrossWeight>6000.000</GrossWeight>
			<GrossWeightVerificationDateTime>2024-03-13T08:45:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>CNT</Code>
			</GrossWeightVerificationType>
			<Seal />
			<WeightUnit>
				<Code>KG</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>BKGPARTY</OrganizationCode>
					<Address1>Booking Party Address 1</Address1>
				</OrganizationAddress>
			</OrganizationAddressCollection>
		</Container>
	</ContainerCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CreateAgencyBooking("BK0006", "TEST1111116", ShipmentStatusList.Codes.BookingCancelled);

				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*VGM message cannot be accepted since the Booking is canceled. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());
			}
		}

		public void TestImportCarrierVGMUniversalShipment_BookingRejected()
		{
			#region message

			var message = GetQueuedUniversalShipmentMessage(@"<UniversalShipment>
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>CAR</Code>
				<Description>Carrier</Description>
				<ServiceCode>VGM</ServiceCode>
				<ServiceDescription>Verified Gross Container Weight</ServiceDescription>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<BookingConfirmationReference>BK0006</BookingConfirmationReference>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TEST1111116</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
				<ISOCode>20GP</ISOCode>
			</ContainerType>
			<GrossWeight>6000.000</GrossWeight>
			<GrossWeightVerificationDateTime>2024-03-13T08:45:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>CNT</Code>
			</GrossWeightVerificationType>
			<Seal />
			<WeightUnit>
				<Code>KG</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>BKGPARTY</OrganizationCode>
					<Address1>Booking Party Address 1</Address1>
				</OrganizationAddress>
			</OrganizationAddressCollection>
		</Container>
	</ContainerCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CreateAgencyBooking("BK0006", "TEST1111116", ShipmentStatusList.Codes.BookingRejected);

				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*VGM message cannot be accepted since the Booking is rejected. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());
			}
		}

		public void TestImportCarrierVGMUniversalShipment_EBookingCancellationRequest()
		{
			#region message

			var message = GetQueuedUniversalShipmentMessage(@"<UniversalShipment>
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>CAR</Code>
				<Description>Carrier</Description>
				<ServiceCode>VGM</ServiceCode>
				<ServiceDescription>Verified Gross Container Weight</ServiceDescription>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<BookingConfirmationReference>BK0007</BookingConfirmationReference>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TEST1111117</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
				<ISOCode>20GP</ISOCode>
			</ContainerType>
			<GrossWeight>6000.000</GrossWeight>
			<GrossWeightVerificationDateTime>2024-03-13T08:45:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>CNT</Code>
			</GrossWeightVerificationType>
			<Seal />
			<WeightUnit>
				<Code>KG</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>BKGPARTY</OrganizationCode>
					<Address1>Booking Party Address 1</Address1>
				</OrganizationAddress>
			</OrganizationAddressCollection>
		</Container>
	</ContainerCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CreateAgencyBooking("BK0007", "TEST1111117", ShipmentStatusList.Codes.EBookingCancellationRequest);

				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*VGM message cannot be accepted once Booking Withdrawal/Cancellation request is in progress. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());
			}
		}

		[TestDate(2024, 02, 15)]
		public void TestImportCarrierVGMUniversalShipment_VGMReceivedAfterActualVesselDeparture()
		{
			#region message

			var message = GetQueuedUniversalShipmentMessage(@"<UniversalShipment>
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>CAR</Code>
				<Description>Carrier</Description>
				<ServiceCode>VGM</ServiceCode>
				<ServiceDescription>Verified Gross Container Weight</ServiceDescription>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<BookingConfirmationReference>BK0008</BookingConfirmationReference>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TEST1111118</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
				<ISOCode>20GP</ISOCode>
			</ContainerType>
			<GrossWeight>6000.000</GrossWeight>
			<GrossWeightVerificationDateTime>2024-03-13T08:45:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>CNT</Code>
			</GrossWeightVerificationType>
			<Seal />
			<WeightUnit>
				<Code>KG</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>BKGPARTY</OrganizationCode>
					<Address1>Booking Party Address 1</Address1>
				</OrganizationAddress>
			</OrganizationAddressCollection>
		</Container>
	</ContainerCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var booking = CreateAgencyBooking("BK0008", "TEST1111118", ShipmentStatusList.Codes.Booked);
				var legs = booking.Transports.Cast<Transport>();
				var mainSeaLeg = legs.FirstOrDefault(x => x.JW_TransportMode == Core.Constants.TransportModes.Sea &&
														x.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel);
				mainSeaLeg.JW_ATD = new ZDateTime(2024, 03, 01, 12, 15, 00);
				Factory.SaveForTesting();

				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*VGM message cannot be accepted after vessel ATD. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());
			}
		}

		[TestDate(2024, 03, 15)]
		public void TestImportCarrierVGMUniversalShipment_CurrentTimeAfterActualVesselDeparture()
		{
			#region message

			var message = GetQueuedUniversalShipmentMessage(@"<UniversalShipment>
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>CAR</Code>
				<Description>Carrier</Description>
				<ServiceCode>VGM</ServiceCode>
				<ServiceDescription>Verified Gross Container Weight</ServiceDescription>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<BookingConfirmationReference>BK0008</BookingConfirmationReference>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TEST1111118</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
				<ISOCode>20GP</ISOCode>
			</ContainerType>
			<GrossWeight>6000.000</GrossWeight>
			<GrossWeightVerificationDateTime>2024-02-13T08:45:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>CNT</Code>
			</GrossWeightVerificationType>
			<Seal />
			<WeightUnit>
				<Code>KG</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>BKGPARTY</OrganizationCode>
					<Address1>Booking Party Address 1</Address1>
				</OrganizationAddress>
			</OrganizationAddressCollection>
		</Container>
	</ContainerCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var booking = CreateAgencyBooking("BK0008", "TEST1111118", ShipmentStatusList.Codes.Booked);
				var legs = booking.Transports.Cast<Transport>();
				var mainSeaLeg = legs.FirstOrDefault(x => x.JW_TransportMode == Core.Constants.TransportModes.Sea &&
														x.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel);
				mainSeaLeg.JW_ATD = new ZDateTime(2024, 03, 01, 12, 15, 00);
				Factory.SaveForTesting();

				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*VGM message cannot be accepted after vessel ATD. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());
			}
		}

		public void TestImportCarrierVGMUniversalShipment_ContainerGrossWeightIsLessThanTareWeightOfContainerType()
		{
			#region message

			var message = GetQueuedUniversalShipmentMessage(@"<UniversalShipment>
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>CAR</Code>
				<Description>Carrier</Description>
				<ServiceCode>VGM</ServiceCode>
				<ServiceDescription>Verified Gross Container Weight</ServiceDescription>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<BookingConfirmationReference>BK0009</BookingConfirmationReference>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TEST1111119</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
				<ISOCode>20GP</ISOCode>
			</ContainerType>
			<GrossWeight>1200.000</GrossWeight>
			<GrossWeightVerificationDateTime>2024-03-13T08:45:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>CNT</Code>
			</GrossWeightVerificationType>
			<Seal />
			<WeightUnit>
				<Code>KG</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>BKGPARTY</OrganizationCode>
					<Address1>Booking Party Address 1</Address1>
				</OrganizationAddress>
			</OrganizationAddressCollection>
		</Container>
	</ContainerCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CreateAgencyBooking("BK0009", "TEST1111119", ShipmentStatusList.Codes.Booked);

				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*Container gross weight cannot be less than Container tare weight. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());
			}
		}

		public void TestImportCarrierVGMUniversalShipment_ContainerGrossWeightIsLessThanTareWeightOfContainerType_WeightUnit()
		{
			#region message

			var message = GetQueuedUniversalShipmentMessage(@"<UniversalShipment>
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>CAR</Code>
				<Description>Carrier</Description>
				<ServiceCode>VGM</ServiceCode>
				<ServiceDescription>Verified Gross Container Weight</ServiceDescription>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<BookingConfirmationReference>BK0010</BookingConfirmationReference>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TEST1111110</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
				<ISOCode>20GP</ISOCode>
			</ContainerType>
			<GrossWeight>21000.000</GrossWeight>
			<GrossWeightVerificationDateTime>2024-03-13T08:45:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>CNT</Code>
			</GrossWeightVerificationType>
			<Seal />
			<WeightUnit>
				<Code>G</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>BKGPARTY</OrganizationCode>
					<Address1>Booking Party Address 1</Address1>
				</OrganizationAddress>
			</OrganizationAddressCollection>
		</Container>
	</ContainerCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CreateAgencyBooking("BK0010", "TEST1111110", ShipmentStatusList.Codes.Booked);

				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*Container gross weight cannot be less than Container tare weight. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());
			}
		}

		public void TestImportCarrierVGMUniversalShipment_GoodsWeightExceedsMaxPayLoad()
		{
			#region message

			var message = GetQueuedUniversalShipmentMessage(@"<UniversalShipment>
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>CAR</Code>
				<Description>Carrier</Description>
				<ServiceCode>VGM</ServiceCode>
				<ServiceDescription>Verified Gross Container Weight</ServiceDescription>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<BookingConfirmationReference>BK0011</BookingConfirmationReference>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>BKCN1111111</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
				<ISOCode>20GP</ISOCode>
			</ContainerType>
			<GrossWeight>3000.000</GrossWeight>
			<GrossWeightVerificationDateTime>2024-03-13T08:45:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>CNT</Code>
			</GrossWeightVerificationType>
			<Seal />
			<WeightUnit>
				<Code>KG</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>BKGPARTY</OrganizationCode>
					<Address1>Booking Party Address 1</Address1>
				</OrganizationAddress>
			</OrganizationAddressCollection>
		</Container>
	</ContainerCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CreateAgencyBooking("BK0011", "BKCN1111111", ShipmentStatusList.Codes.Booked);

				var containerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
				containerType.RC_GrossWeight = 2400;
				Factory.SaveForTesting();

				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(message);
				AssertMultilineASCIIEquals(@"Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*Goods weight exceeds container maximum payload. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());
			}
		}

		public void TestImportCarrierVGMUniversalShipment_Successful()
		{
			#region message

			var message = GetQueuedUniversalShipmentMessage(@"<UniversalShipment>
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>CAR</Code>
				<Description>Carrier</Description>
				<ServiceCode>VGM</ServiceCode>
				<ServiceDescription>Verified Gross Container Weight</ServiceDescription>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<BookingConfirmationReference>BK0012</BookingConfirmationReference>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>BKCN1111112</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
				<ISOCode>20GP</ISOCode>
			</ContainerType>
			<GrossWeight>5000.000</GrossWeight>
			<GrossWeightVerificationDateTime>2024-03-13T08:45:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>CNT</Code>
			</GrossWeightVerificationType>
			<Seal />
			<WeightUnit>
				<Code>KG</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>BKGPARTY</OrganizationCode>
					<Address1>Booking Party Address 1</Address1>
				</OrganizationAddress>
			</OrganizationAddressCollection>
		</Container>
	</ContainerCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var booking = CreateAgencyBooking("BK0012", "BKCN1111112", ShipmentStatusList.Codes.Booked);

				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Successfully loaded matching AgencyBooking.
Populating AgencyBooking...
Successfully loaded matching AgencyBookingContainer.
Populating AgencyBookingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Updated Shipping Booking V00001000 from UniversalShipment.
Successfully saved Shipping Booking V00001000 with 1 x AgencyBookingContainer.", message.GetLogNoteText());

				var bookingUpdated = new BusinessObjectFactory().Load<AgencyBooking>(booking.PK);
				var containers = bookingUpdated.RealContainers.Cast<AgencyBookingContainer>();
				var container = containers.FirstOrDefault(x => x.JC_ContainerNum == "BKCN1111112");
				var vgmDate = new ZDateTime(2024, 03, 13, 8, 45, 00);

				AssertEquals("VGM Gross Weight:", (ZDecimal)5000, container.JC_GrossWeight);
				AssertEquals("VGM Verification Date:", vgmDate, container.JC_GrossWeightVerificationDateTime);
				AssertEquals("VGM Verified By:", bookingParty.PK, container.GrossWeightVerifiedByPK);
				AssertEquals("VGM Verification Type:", "CNT", container.JC_GrossWeightVerificationType);

				var qtvEvent = bookingUpdated.Logs.Find(l => l.SL_SE_NKEvent == "QTV").FirstOrDefault() ?? container.Logs.Find(l => l.SL_SE_NKEvent == "QTV").FirstOrDefault();
				AssertNotNull(qtvEvent);
			}
		}

		AgencyBooking CreateAgencyBooking(ZString bookingNumber, ZString containerNumber, ZString shipmentStatus, ZString shippersReference = new ZString(), bool addSailing = true)
		{
			var booking = Factory.New<AgencyBooking>();
			booking.JS_ShipmentStatus = shipmentStatus;
			booking.JS_HouseBill = ZString.Empty;
			booking.JS_CFSReference = bookingNumber;
			booking.JS_BookingReference = shippersReference;
			booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			if (addSailing)
			{
				var sailing = UniversalTestHelper.CreateSailingWithVoyage(Factory.BOFactory, "AUSYD", "SGSIN", "USS ZAMBEZI", "001");
				booking.JS_JX = sailing.PK;
			}

			var container = booking.RealContainers.AddNew();
			container.JC_ContainerNum = containerNumber;
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_TareWeight = 2000;
			container.JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			container.JC_GrossWeight = 4500m;

			var packLine = booking.OuterPackLines.AddNew();
			packLine.JL_Description = "shampoo";
			packLine.JL_PackageCount = 4;
			packLine.JL_ActualVolumeUQ = "M3";
			packLine.JL_ActualWeight = 2500;
			container.PackLines.Add(packLine);

			var transport = booking.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport.JW_Vessel = "USS ZAMBEZI";
			transport.JW_VoyageFlight = "001";
			transport.JW_ETD = new ZDateTime(2024, 03, 20, 8, 45, 00);
			transport.JW_ETA = new ZDateTime(2024, 03, 21, 12, 15, 00);

			Factory.SaveForTesting();
			return booking;
		}

		#endregion

		#endregion

		#region Implementation

		protected override ITopLevelDataObjectReader GetReader(UniversalShipment dataObject)
		{
			return GetReader(dataObject, new TestErrorLogger());
		}

		protected override AgencyShipmentDataObjectReader<AgencyBooking> GetReader(UniversalShipment dataObject, IXmlImportLogger logger)
		{
			return new AgencyBookingDataObjectReader(dataObject, logger, Factory);
		}

		protected override UniversalShipment GetDataObject()
		{
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.WayBillNumber = "SHP001";
			dataObject.TransportMode = new CodeDescriptionPair { Code = "SEA", Description = "Sea" };
			dataObject.PortOfOrigin = new UNLOCO { Code = "AUSYD", Name = "Sydney" };
			dataObject.PortOfDestination = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
			dataObject.ShipmentIncoTerm = new IncoTerm { Code = "FOB", Description = "Free On Board" };
			dataObject.ContainerMode = new ContainerMode { Code = "FCL", Description = "Full Container Load" };
			dataObject.GoodsDescription = "frozen ducks";
			dataObject.ReleaseType = new CodeDescriptionPair { Code = "EBL", Description = "Release Bill of Lading" };
			dataObject.HBLAWBChargesDisplay = new CodeDescriptionPair { Code = "SHW", Description = "Show Collect Charges" };
			dataObject.ShippedOnBoard = new CodeDescriptionPair { Code = "LDN", Description = "Laden" };
			dataObject.NoCopyBills = 2;
			dataObject.NoOriginalBills = 3;
			dataObject.TotalVolume = 11;
			dataObject.TotalVolumeUnit = new UnitOfVolume { Code = "M3", Description = "Cubic Meters" };
			dataObject.TotalWeight = 22;
			dataObject.TotalWeightUnit = new UnitOfWeight { Code = "KG", Description = "Kilograms" };
			dataObject.ActualChargeable = 33;
			dataObject.InterimReceiptNumber = "IR001";
			dataObject.ShipmentType = new CodeDescriptionPair { Code = "STD", Description = "Standard House" };
			dataObject.ShipperCODAmount = 44;
			dataObject.ShipperCODPayMethod = new CodeDescriptionPair { Code = "COC", Description = "Company Check" };
			dataObject.BookingConfirmationReference = "booking ref";
			dataObject.AgentsReference = "agent ref";
			dataObject.TotalNoOfPacks = 55;
			dataObject.TotalNoOfPacksPackageType = new PackageType { Code = "KEG", Description = "Keg" };
			dataObject.ManifestedVolume = 66;
			dataObject.ManifestedWeight = 77;
			dataObject.ManifestedChargeable = 88;
			dataObject.DocumentedVolume = 99;
			dataObject.DocumentedWeight = 111;
			dataObject.DocumentedChargeable = 222;
			dataObject.TranshipToOtherCFS = true;
			dataObject.ServiceLevel = new ServiceLevel { Code = "STD", Description = "Standard" };
			dataObject.FreightRate = 67.89m;
			dataObject.FreightRateCurrency = new Currency { Code = "NGN", Description = "Nigerian Naira" };
			dataObject.GoodsValue = 123.45m;
			dataObject.GoodsValueCurrency = new Currency { Code = "ZMK", Description = "Zambia Kwacha" };
			dataObject.InsuranceValue = 345.67m;
			dataObject.InsuranceValueCurrency = new Currency { Code = "CFA", Description = " Central African Franc" };
			dataObject.ShipmentStatus = new CodeDescriptionPair { Code = ShipmentStatusList.Codes.Booked };
			dataObject.SetDateCollection(() => new List<Date> { new Date { Type = DateType.BookingConfirmed, Value = new ZDateTime(2012, 2, 1) }, new Date { Type = DateType.Received, Value = new ZDateTime(2012, 2, 2) }, new Date { Type = DateType.Departure, Value = new ZDateTime(2012, 2, 3) }, new Date { Type = DateType.Arrival, Value = new ZDateTime(2012, 2, 4) }, new Date { Type = DateType.BillIssued, Value = new ZDateTime(2012, 2, 5) }, new Date { Type = DateType.ShippedOnBoard, Value = new ZDateTime(2012, 2, 6) } });
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{ AddressType = nameof(DocAddressType.ShippingLineAddress), Address1 = carrier.MainAddress.OA_Address1, OrganizationCode = carrier.OH_Code }, new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{ AddressType = nameof(DocAddressType.Principal), Address1 = principal.MainAddress.OA_Address1, OrganizationCode = principal.OH_Code }, new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{ AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress), Address1 = BookingParty.MainAddress.OA_Address1, OrganizationCode = BookingParty.OH_Code }, new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{ AddressType = nameof(DocAddressType.NotifyParty), Address1 = notifyParty.MainAddress.OA_Address1, OrganizationCode = notifyParty.OH_Code }, new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{ AddressType = nameof(DocAddressType.NotifyParty2), Address1 = notifyParty2.MainAddress.OA_Address1, OrganizationCode = notifyParty2.OH_Code }, new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{ AddressType = nameof(DocAddressType.NotifyParty3), Address1 = notifyParty3.MainAddress.OA_Address1, OrganizationCode = notifyParty3.OH_Code } });
			dataObject.SetNoteCollection(() => new DataObjectList<Note> { new Note { Description = "CAT EATER", Visibility = new CodeDescriptionPair { Code = "PUB", Description = "Public" }, NoteContext = new NoteContext { Code = "BEB", Description = "Baby Eats Banana" } } });
			dataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference> { new AdditionalReference { Type = new EntryType { Code = "CON", Description = "Contract Number" }, ReferenceNumber = "Additional reference number" } });
			dataObject.SetContainerCollection(() => new DataObjectList<Container> { new Container { ContainerNumber = "AAA", ContainerType = new ContainerType { Code = "20GP", ISOCode = "22G0" } }, new Container { ContainerNumber = "BBB", ContainerType = new ContainerType { Code = "20GP", ISOCode = "22G0" } }, new Container { Link = 3, ContainerType = new ContainerType { Code = "20GP", ISOCode = "22G0" } } });
			dataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{ GoodsDescription = "camel toes" }, new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{ ContainerNumber = "AAA", GoodsDescription = "cups" }, new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{ ContainerLink = 3, GoodsDescription = "balls" } });
			dataObject.PackingLineCollection.Content = CollectionContent.Complete;
			dataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg> { new TransportLeg { PortOfLoading = new UNLOCO { Code = "AUSYD" }, PortOfDischarge = new UNLOCO { Code = "NZAKL" } }, new TransportLeg { PortOfLoading = new UNLOCO { Code = "NZAKL" }, PortOfDischarge = new UNLOCO { Code = "USMIA" } } });
			return dataObject;
		}

		protected override string GetExpectedShipmentMap()
		{
			using (var retriever = new EmbeddedResourceRetriever())
			{
				return retriever.GetString("Enterprise.Freight.Agency.DataTransfer.Test.Universal.AgencyBooking.TestFiles.AgencyBooking_FieldMap.txt");
			}
		}

		protected override string CreateFieldMap(BusinessObject agencyBooking)
		{
			var mapper = new AgencyBookingFieldMapper((AgencyBooking)agencyBooking);
			return mapper.Map();
		}

		#endregion

		#region SetUp

		protected override void SetUp()
		{
			base.SetUp();

			carrier = Factory.New<OrgHeader>();
			carrier.MainAddress.OA_Address1 = "Carrier Ln";
			carrier.OH_Code = "CAR";

			principal = Factory.New<OrgHeader>();
			principal.MainAddress.OA_Address1 = "Principal Ave";
			principal.OH_Code = "PRC";

			notifyParty = Factory.New<OrgHeader>();
			notifyParty.MainAddress.OA_Address1 = "1 Notify Ln";
			notifyParty.OH_Code = "NP1";

			notifyParty2 = Factory.New<OrgHeader>();
			notifyParty2.MainAddress.OA_Address1 = "2 Notify Ln";
			notifyParty2.OH_Code = "NP2";

			notifyParty3 = Factory.New<OrgHeader>();
			notifyParty3.MainAddress.OA_Address1 = "3 Notify Ln";
			notifyParty3.OH_Code = "NP3";

			bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			Factory.SaveForTesting();
		}

		OrgHeader carrier;
		OrgHeader principal;
		OrgHeader notifyParty;
		OrgHeader notifyParty2;
		OrgHeader notifyParty3;
		OrgHeader bookingParty;

		#endregion
	}
}
