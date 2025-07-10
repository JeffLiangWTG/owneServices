using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
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
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.UniversalDataBuss.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using IncoTerm = Enterprise.UniversalDataBuss.DataObjects.Universal.IncoTerm;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	sealed class BillOfLadingDataObjectReaderTest : AgencyShipmentDataObjectReaderTest<BillOfLading>
	{
		#region ImportForServiceCode

		#region NoMatch

		public void TestImportForServiceCode_NoMatch_SINServiceCode()
		{
			var logger = new TestErrorLogger();
			var result = ImportForServiceCode(AgencyShipmentType.None, ServiceCodeType.SIN, logger);
			AssertNotNull("create new", result);
			var expectedMessage = @"Information - No matching BillOfLading found, creating new BillOfLading.
Information - Populating BillOfLading...
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

		public void TestImportForServiceCode_NoMatch_VGMServiceCode()
		{
			var logger = new TestErrorLogger();
			var result = ImportForServiceCode(AgencyShipmentType.None, ServiceCodeType.VGM, logger);
			AssertNull("import rejected", result);
			var expectedMessage = @"Error - Cannot populate BillOfLading because:
XML file contains VGM service code and cannot find a matched Agency Shipment.";
			var actualMessage = string.Join(System.Environment.NewLine, logger.Logs).Trim();
			AssertMultilineASCIIEquals("import logs", expectedMessage, actualMessage);
		}

		public void TestImportForServiceCode_NoMatch_NoServiceCode_DataTargetIsBOL()
		{
			var logger = new TestErrorLogger();
			var result = ImportForServiceCode(AgencyShipmentType.None, null, null, logger, nameof(DataContextType.BillOfLading));
			AssertNotNull("create new", result);
			var expectedMessage = @"Information - No matching BillOfLading found, creating new BillOfLading.
Information - Populating BillOfLading...
Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: SGSIN
Information - Attempting to get Schedule for the Transport Leg
Information - An existing vessel could not be found. A Schedule will not be created. Attempting to find an existing Schedule...
Information - An existing Schedule could not be found. The Transport Leg will not be linked.
Information - Transport Leg updated.
Information - Added Shipping Bill of Lading  from UniversalShipment.";
			var actualMessage = string.Join(System.Environment.NewLine, logger.Logs).Trim();
			AssertMultilineASCIIEquals("import logs", expectedMessage, actualMessage);
		}

		#endregion

		#region Validation

		public void TestBookingNumberIsMissing_Rejected()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var billOfLading = CreateAgencyShipment(ShipmentStatusList.Codes.ElectronicShippingInstruction);
				var dataObject = CreateDataObject(serviceCode: ServiceCodeType.SIN, recipientRoleType: RecipientRoleType.CAR);
				dataObject.AgentsReference = "TS001";
				dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
						Address1 = BookingParty.MainAddress.OA_Address1,
						OrganizationCode = BookingParty.OH_Code,
						CompanyName = BookingParty.OH_FullName
					}
				});
				dataObject.BookingConfirmationReference = ZString.Empty;

				Action<AgencyShipment, string> assertAction = (readBizOjb, message) =>
				{
					AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Error - Cannot populate BillOfLading because:
[*Shipping Instruction message received without Booking Number. Booking Number is mandatory to process the Shipping Instruction. Message rejected.*]".Trim(), message);
				};
				ReadDataObjectAndAssertResult(dataObject, assertAction);
			}
		}

		public void TestBookingPartyIsMissing_Rejected()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var billOfLading = CreateAgencyShipment(ShipmentStatusList.Codes.ElectronicShippingInstruction);
				var dataObject = CreateDataObject(serviceCode: ServiceCodeType.SIN, recipientRoleType: RecipientRoleType.CAR);
				dataObject.AgentsReference = "TS001";
				dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());

				Action<AgencyShipment, string> assertAction = (readBizOjb, message) =>
				{
					AssertMultilineASCIIEquals("import logs", @"
Information - Successfully loaded matching BillOfLading.
Error - Cannot populate BillOfLading because:
[*Shipping Instruction message received without Booking Party. Booking Party is mandatory to process the Shipping Instruction. Message rejected.*]".Trim(), message);
				};
				ReadDataObjectAndAssertResult(dataObject, assertAction);
			}
		}

		public void TestShipperReferenceIsMissing_Rejected()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var billOfLading = CreateAgencyShipment(ShipmentStatusList.Codes.ElectronicShippingInstruction);
				var dataObject = CreateDataObject(serviceCode: ServiceCodeType.SIN, recipientRoleType: RecipientRoleType.CAR);
				dataObject.AgentsReference = ZString.Empty;
				dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
						Address1 = BookingParty.MainAddress.OA_Address1,
						OrganizationCode = BookingParty.OH_Code,
						CompanyName = BookingParty.OH_FullName
					}
				});

				Action<AgencyShipment, string> assertAction = (readBizOjb, message) =>
				{
					AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching BillOfLading.
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Error - Cannot populate BillOfLading because:
[*Shipping Instruction message received without Shipper's Reference. Shipper's Reference is mandatory to process the Shipping Instruction. Message rejected.*]".Trim(), message);
				};
				ReadDataObjectAndAssertResult(dataObject, assertAction);
			}
		}

		public void TestBookingPartyDoesNotExistInSystem_Rejected()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var billOfLading = CreateAgencyShipment(ShipmentStatusList.Codes.ElectronicShippingInstruction);
				var dataObject = CreateDataObject(serviceCode: ServiceCodeType.SIN, recipientRoleType: RecipientRoleType.CAR);
				dataObject.AgentsReference = "TS001";
				dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
						Address1 = "test address1",
						OrganizationCode = "ERR",
						CompanyName = "Invalid"
					}
				});

				Action<AgencyShipment, string> assertAction = (readBizOjb, message) =>
				{
					AssertMultilineASCIIEquals("import logs", @"Warning - Matching 'BookingPartyDocumentaryAddress':- No match found for '[Org. Code: ERR; Company Name: Invalid; Address 1: test address1]'.
Information - Successfully loaded matching BillOfLading.
Warning - Matching 'BookingPartyDocumentaryAddress':- No match found for '[Org. Code: ERR; Company Name: Invalid; Address 1: test address1]'.
Error - Cannot populate BillOfLading because:
[*Shipping Instruction message received from an unknown Booking Party INVALID. Message rejected.*]".Trim(), message);
				};
				ReadDataObjectAndAssertResult(dataObject, assertAction);
			}
		}

		public void TestIncorrectBookingParty_Rejected()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var billOfLading = CreateAgencyShipment(ShipmentStatusList.Codes.ElectronicShippingInstruction);
				var dataObject = CreateDataObject(serviceCode: ServiceCodeType.SIN, recipientRoleType: RecipientRoleType.CAR);
				dataObject.AgentsReference = "TS001";
				dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
						Address1 = BookingParty.MainAddress.OA_Address1,
						OrganizationCode = BookingParty.OH_Code,
						CompanyName = "Incorrect Company Name"
					}
				});

				Action<AgencyShipment, string> assertAction = (readBizOjb, message) =>
				{
					AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching BillOfLading.
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Error - Cannot populate BillOfLading because:
[*Shipping Instruction message received with an incorrect Booking Party INCORRECT COMPANY NAME. Message rejected.*]".Trim(), message);
				};
				ReadDataObjectAndAssertResult(dataObject, assertAction);
			}
		}

		public void TestInvalidBookingNumber_Rejected()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var billOfLading = CreateAgencyShipment(ShipmentStatusList.Codes.ElectronicShippingInstruction);
				var dataObject = CreateDataObject(serviceCode: ServiceCodeType.SIN, recipientRoleType: RecipientRoleType.CAR);
				dataObject.AgentsReference = "TS001";
				dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
						Address1 = BookingParty.MainAddress.OA_Address1,
						OrganizationCode = BookingParty.OH_Code,
						CompanyName = BookingParty.OH_FullName
					}
				});
				dataObject.WayBillNumber = ZString.Empty;
				dataObject.BookingConfirmationReference = "Invalid Booking Number";

				Action<AgencyShipment, string> assertAction = (readBizOjb, message) =>
				{
					AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Error - Cannot populate BillOfLading because:
[*Shipping Instruction message with Booking Number Invalid Booking Number cannot find a matching Booking. Message rejected.*]".Trim(), message);
				};
				ReadDataObjectAndAssertResult(dataObject, assertAction);
			}
		}

		public void TestBookingNotConfirmed_Rejected()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var bookingNotConfirmed = CreateAgencyShipment(ShipmentStatusList.Codes.ElectronicBooking);
				var dataObject = CreateDataObject(serviceCode: ServiceCodeType.SIN, recipientRoleType: RecipientRoleType.CAR);
				dataObject.AgentsReference = "TS001";
				dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
						Address1 = BookingParty.MainAddress.OA_Address1,
						OrganizationCode = BookingParty.OH_Code,
						CompanyName = BookingParty.OH_FullName
					}
				});

				Action<AgencyShipment, string> assertAction = (readBizOjb, message) =>
				{
					AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching BillOfLading.
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Error - Cannot populate BillOfLading because:
[*Shipping Instruction cannot be processed, because Booking BKG001 is not confirmed by carrier. Message rejected.*]".Trim(), message);
				};
				ReadDataObjectAndAssertResult(dataObject, assertAction);
			}
		}

		public void TestBillNumberIsMissing_AMD_Rejected()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var billOfLading = CreateAgencyShipment(ShipmentStatusList.Codes.ElectronicShippingInstruction);
				var dataObject = CreateDataObject(serviceCode: ServiceCodeType.SIN, recipientRoleType: RecipientRoleType.CAR, purpose: MessagePurposes.Codes.Amendment);
				dataObject.AgentsReference = "TS001";
				dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
						Address1 = BookingParty.MainAddress.OA_Address1,
						OrganizationCode = BookingParty.OH_Code,
						CompanyName = BookingParty.OH_FullName
					}
				});
				dataObject.WayBillNumber = ZString.Empty;

				Action<AgencyShipment, string> assertAction = (readBizOjb, message) =>
				{
					AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching BillOfLading.
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Error - Cannot populate BillOfLading because:
[*Shipping Instruction Amendment message received without Bill Number. Bill Number is mandatory to process the Shipping Instruction Amendment. Message rejected.*]".Trim(), message);
				};
				ReadDataObjectAndAssertResult(dataObject, assertAction);
			}
		}

		public void TestBookingWasNotConvertedToBillOfLading_AMD_Rejected()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var booking = CreateAgencyShipment(ShipmentStatusList.Codes.Booked);
				var dataObject = CreateDataObject(serviceCode: ServiceCodeType.SIN, recipientRoleType: RecipientRoleType.CAR, purpose: MessagePurposes.Codes.Amendment);
				dataObject.AgentsReference = "TS001";
				dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
						Address1 = BookingParty.MainAddress.OA_Address1,
						OrganizationCode = BookingParty.OH_Code,
						CompanyName = BookingParty.OH_FullName
					}
				});

				Action<AgencyShipment, string> assertAction = (readBizOjb, message) =>
				{
					AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching BillOfLading.
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Error - Cannot populate BillOfLading because:
[*Shipping Instruction Amendment message with Booking Number BKG001 cannot be processed. Shipping Instruction original message is required. Message rejected.*]".Trim(), message);
				};
				ReadDataObjectAndAssertResult(dataObject, assertAction);
			}
		}

		public void TestInvalidBillNumber_AMD_Rejected()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var billOfLading = CreateAgencyShipment(ShipmentStatusList.Codes.ElectronicShippingInstruction);
				billOfLading.JS_HouseBill = ZString.Empty;

				var dataObject = CreateDataObject(serviceCode: ServiceCodeType.SIN, recipientRoleType: RecipientRoleType.CAR, purpose: MessagePurposes.Codes.Amendment);
				dataObject.AgentsReference = "TS001";
				dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
						Address1 = BookingParty.MainAddress.OA_Address1,
						OrganizationCode = BookingParty.OH_Code,
						CompanyName = BookingParty.OH_FullName
					}
				});

				Action<AgencyShipment, string> assertAction = (readBizOjb, message) =>
				{
					AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching BillOfLading.
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Error - Cannot populate BillOfLading because:
[*Shipping Instruction Amendment message with Bill Number S0001 cannot find a matching Bill of Lading to update. Message rejected.*]".Trim(), message);
				};
				ReadDataObjectAndAssertResult(dataObject, assertAction);
			}
		}

		public void TestBillOfLadingNotConfirmed_AMD_Rejected()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var billOfLading = CreateAgencyShipment(ShipmentStatusList.Codes.ElectronicShippingInstruction);
				var dataObject = CreateDataObject(serviceCode: ServiceCodeType.SIN, recipientRoleType: RecipientRoleType.CAR, purpose: MessagePurposes.Codes.Amendment);
				dataObject.AgentsReference = "TS001";
				dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
						Address1 = BookingParty.MainAddress.OA_Address1,
						OrganizationCode = BookingParty.OH_Code,
						CompanyName = BookingParty.OH_FullName
					}
				});

				Action<AgencyShipment, string> assertAction = (readBizOjb, message) =>
				{
					AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching BillOfLading.
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Error - Cannot populate BillOfLading because:
[*Shipping Instruction Amendment message with Bill Number S0001 cannot be processed because Shipping Instruction is not confirmed. Message rejected.*]".Trim(), message);
				};
				ReadDataObjectAndAssertResult(dataObject, assertAction);
			}
		}

		public void TestBillOfLadingConfirmed_AMD()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var billOfLading = CreateAgencyShipment(ShipmentStatusList.Codes.Confirmed);
				var dataObject = CreateDataObject(serviceCode: ServiceCodeType.SIN, recipientRoleType: RecipientRoleType.CAR, purpose: MessagePurposes.Codes.Amendment);
				dataObject.AgentsReference = "TS001";
				dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
						Address1 = BookingParty.MainAddress.OA_Address1,
						OrganizationCode = BookingParty.OH_Code,
						CompanyName = BookingParty.OH_FullName
					}
				});

				Action<AgencyShipment, string> assertAction = (readBizOjb, message) =>
				{
					AssertEquals(ShipmentStatusList.Codes.ElectronicShippingInstruction, readBizOjb.JS_ShipmentStatus);
					Assert(readBizOjb.Logs.GetAllLogs().OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == Constants.EventReferenceMessageTypes.ShipmentStatus && x.Parameters[Params.New] == ShipmentStatusList.Codes.ElectronicShippingInstruction));
					AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching BillOfLading.
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Populating BillOfLading...
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: SGSIN
Information - Attempting to get Schedule for the Transport Leg
Information - A Schedule will not be created. Attempting to find an existing Schedule...
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Information - Updated Shipping Shipment V00001000 from UniversalShipment.
".Trim(), message);
				};
				ReadDataObjectAndAssertResult(dataObject, assertAction);
			}
		}

		public void TestBookingCancellationRequestIsInProcess_Rejected()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var billOfLading = CreateAgencyShipment(ShipmentStatusList.Codes.EBookingCancellationRequest);
				var dataObject = CreateDataObject(serviceCode: ServiceCodeType.SIN, recipientRoleType: RecipientRoleType.CAR, purpose: MessagePurposes.Codes.Amendment);
				dataObject.AgentsReference = "TS001";
				dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
						Address1 = BookingParty.MainAddress.OA_Address1,
						OrganizationCode = BookingParty.OH_Code,
						CompanyName = BookingParty.OH_FullName
					}
				});

				Action<AgencyShipment, string> assertAction = (readBizOjb, message) =>
				{
					AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching BillOfLading.
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Error - Cannot populate BillOfLading because:
[*Shipping Instruction message cannot be accepted once Booking Withdrawal/Cancellation request is in progress. Message rejected.*]".Trim(), message);
				};
				ReadDataObjectAndAssertResult(dataObject, assertAction);
			}
		}

		public void TestBookingCancelled_Rejected()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var booking = CreateAgencyShipment(ShipmentStatusList.Codes.BookingCancelled);
				var dataObject = CreateDataObject(serviceCode: ServiceCodeType.SIN, recipientRoleType: RecipientRoleType.CAR, purpose: MessagePurposes.Codes.Amendment);
				dataObject.AgentsReference = "TS001";
				dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
						Address1 = BookingParty.MainAddress.OA_Address1,
						OrganizationCode = BookingParty.OH_Code,
						CompanyName = BookingParty.OH_FullName
					}
				});

				Action<AgencyShipment, string> assertAction = (readBizOjb, message) =>
				{
					AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching BillOfLading.
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Error - Cannot populate BillOfLading because:
[*Shipping Instruction message cannot be accepted since Booking is Canceled. Message rejected.*]".Trim(), message);
				};
				ReadDataObjectAndAssertResult(dataObject, assertAction);
			}
		}

		#endregion

		#region MatchBooking

		public void TestImportForServiceCode_MatchBooking_SINServiceCode()
		{
			var logger = new TestErrorLogger();
			var result = ImportForServiceCode(AgencyShipmentType.Booking, ServiceCodeType.SIN, logger);
			AssertNotNull("should matched the Booking but cannot update", result);
			var expectedMessage = @"Information - Successfully loaded matching BillOfLading.
Information - Populating BillOfLading...
Information - Confirmed Shipping Bill of Lading V00001000.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: SGSIN
Information - Attempting to get Schedule for the Transport Leg
Information - A Schedule will not be created. Attempting to find an existing Schedule...
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Information - Updated Shipping Shipment V00001000 from UniversalShipment.
";
			var actualMessage = string.Join(System.Environment.NewLine, logger.Logs).Trim();
			AssertMultilineASCIIEquals("import logs", expectedMessage, actualMessage);
		}

		public void TestImportForServiceCode_MatchBooking_VGMServiceCode()
		{
			var logger = new TestErrorLogger();
			var result = ImportForServiceCode(AgencyShipmentType.Booking, ServiceCodeType.VGM, logger);
			AssertNull("should nit matched any shipment.", result);
			var expectedMessage = @"Error - Cannot populate BillOfLading because:
XML file contains VGM service code and cannot find a matched Agency Shipment.";
			var actualMessage = string.Join(System.Environment.NewLine, logger.Logs).Trim();
			AssertMultilineASCIIEquals("import logs", expectedMessage, actualMessage);
		}

		public void TestImportForServiceCode_MatchBooking_NoServiceCode_DataTargetIsBOL()
		{
			var logger = new TestErrorLogger();
			var result = ImportForServiceCode(AgencyShipmentType.Booking, null, null, logger, nameof(DataContextType.BillOfLading));
			AssertNotNull("update", result);
			AssertEquals("shoud be confirmed", ShipmentStatusList.Codes.Confirmed, result.JS_ShipmentStatus);
			var expectedMessage = @"Information - Successfully loaded matching BillOfLading.
Information - Populating BillOfLading...
Information - Confirmed Shipping Bill of Lading V00001000.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: SGSIN
Information - Attempting to get Schedule for the Transport Leg
Information - A Schedule will not be created. Attempting to find an existing Schedule...
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Information - Updated Shipping Bill of Lading V00001000 from UniversalShipment.";
			var actualMessage = string.Join(System.Environment.NewLine, logger.Logs).Trim();
			AssertMultilineASCIIEquals("import logs", expectedMessage, actualMessage);
		}

		#endregion

		#region MatchBOL

		public void TestImportForServiceCode_MatchBOL_SINServiceCode_AMD()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var booking = CreateAgencyShipment(ShipmentStatusList.Codes.Confirmed);
				var dataObject = CreateDataObject(serviceCode: ServiceCodeType.SIN, recipientRoleType: RecipientRoleType.CAR, purpose: MessagePurposes.Codes.Amendment);
				dataObject.AgentsReference = "TS001";
				dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
						Address1 = BookingParty.MainAddress.OA_Address1,
						OrganizationCode = BookingParty.OH_Code,
						CompanyName = BookingParty.OH_FullName
					}
				});

				Action<AgencyShipment, string> assertAction = (readBizOjb, message) =>
				{
					AssertMultilineASCIIEquals("import logs", @"Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching BillOfLading.
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Populating BillOfLading...
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: SGSIN
Information - Attempting to get Schedule for the Transport Leg
Information - A Schedule will not be created. Attempting to find an existing Schedule...
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Information - Updated Shipping Shipment V00001000 from UniversalShipment.".Trim(), message);
				};
				ReadDataObjectAndAssertResult(dataObject, assertAction);
			}
		}

		public void TestImportForServiceCode_MatchBOL_SINServiceCode()
		{
			var logger = new TestErrorLogger();
			var result = ImportForServiceCode(AgencyShipmentType.BillOfLading, ServiceCodeType.SIN, logger);
			AssertNotNull("update", result);
			var expectedMessage = @"Information - Successfully loaded matching BillOfLading.
Information - Populating BillOfLading...
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

		public void TestImportForServiceCode_MatchBOL_VGMServiceCode()
		{
			var logger = new TestErrorLogger();
			var result = ImportForServiceCode(AgencyShipmentType.BillOfLading, ServiceCodeType.VGM, logger);
			AssertNotNull("update", result);
			var expectedMessage = @"Information - Successfully loaded matching BillOfLading.
Information - Populating BillOfLading...
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: SGSIN
Information - Attempting to get Schedule for the Transport Leg
Information - A Schedule will not be created. Attempting to find an existing Schedule...
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Information - Updated Shipping Bill of Lading V00001000 from UniversalShipment.";
			var actualMessage = string.Join(System.Environment.NewLine, logger.Logs).Trim();
			AssertMultilineASCIIEquals("import logs", expectedMessage, actualMessage);
		}

		public void TestImportForServiceCode_MatchBOL_NoServiceCode()
		{
			var logger = new TestErrorLogger();
			var result = ImportForServiceCode(AgencyShipmentType.BillOfLading, null, null, logger, null);
			AssertNotNull("update", result);
			var expectedMessage = @"Information - Successfully loaded matching BillOfLading.
Information - Populating BillOfLading...
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: SGSIN
Information - Attempting to get Schedule for the Transport Leg
Information - A Schedule will not be created. Attempting to find an existing Schedule...
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Information - Updated Shipping Bill of Lading V00001000 from UniversalShipment.";
			var actualMessage = string.Join(System.Environment.NewLine, logger.Logs).Trim();
			AssertMultilineASCIIEquals("import logs", expectedMessage, actualMessage);
		}

		#endregion

		#endregion

		public void TestPopulateBillNumber_SINServiceCode_ORG()
		{
			var billOfLading = CreateAgencyShipment(ShipmentStatusList.Codes.Confirmed);
			var dataObject = CreateDataObject(serviceCode: ServiceCodeType.SIN, recipientRoleType: RecipientRoleType.CAR, purpose: MessagePurposes.Codes.Original);
			dataObject.AgentsReference = "TS001";
			dataObject.WayBillNumber = ZString.Empty;
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
					Address1 = BookingParty.MainAddress.OA_Address1,
					OrganizationCode = BookingParty.OH_Code,
					CompanyName = BookingParty.OH_FullName
				}
			});

			Action<AgencyShipment, string> assertAction = (readBizOjb, message) =>
			{
				AssertEquals("S0001", readBizOjb.JS_HouseBill);
			};
			ReadDataObjectAndAssertResult(dataObject, assertAction);
		}

		public void TestSetShipmentStatus()
		{
			var billOfLading = CreateAgencyShipment(ShipmentStatusList.Codes.Confirmed);
			var dataObject = CreateDataObject();
			dataObject.ShipmentStatus = new CodeDescriptionPair { Code = ShipmentStatusList.Codes.WebFwdInstruction };
			dataObject.PaymentMethod = new CodeDescriptionPair { Code = Constants.DomesticPaymentTerms.Prepaid, Description = "Prepaid" };
			Action<AgencyShipment, string> assertAction = (readBizObj, message) =>
			{
				AssertEquals("matched bill of lading", billOfLading.PK, readBizObj.PK);
				AssertEquals("Payment Method", Constants.DomesticPaymentTerms.Prepaid, billOfLading.JS_INCO);
				AssertEquals("should update shipment status", ShipmentStatusList.Codes.WebFwdInstruction, readBizObj.JS_ShipmentStatus);
				AssertMultilineASCIIEquals("import logs", @"
Information - Successfully loaded matching BillOfLading.
Information - Populating BillOfLading...
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: SGSIN
Information - Attempting to get Schedule for the Transport Leg
Information - A Schedule will not be created. Attempting to find an existing Schedule...
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Information - Updated Shipping Forwarding Instruction V00001000 from UniversalShipment.
".Trim(), message);
			};
			ReadDataObjectAndAssertResult(dataObject, assertAction);
		}

		public void TestMatchedCorrectBoLByUniversalShipment()
		{
			var billOfLading1 = CreateAgencyShipment(ShipmentStatusList.Codes.Confirmed);
			billOfLading1.JS_HouseBill = "S0001";
			billOfLading1.JS_CFSReference = "BOOKING REF";
			var billOfLading2 = Factory.New<BillOfLading>();
			billOfLading2.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			billOfLading2.JS_HouseBill = "S0001";
			billOfLading2.JS_CFSReference = "BOOKING REF";
			var dataObject = CreateDataObject();
			dataObject.BookingConfirmationReference = "BOOKING REF";
			Factory.SaveForTesting();
			Action<AgencyShipment, string> assertAction = (readBizObj, message) =>
			{
				AssertEquals("only match the bill of lading has linked sailing", billOfLading1.PK, readBizObj.PK);
				AssertMultilineASCIIEquals("import logs", @"
Information - Successfully loaded matching BillOfLading.
Information - Populating BillOfLading...
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: SGSIN
Information - Attempting to get Schedule for the Transport Leg
Information - A Schedule will not be created. Attempting to find an existing Schedule...
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Information - Updated Shipping Bill of Lading V00001000 from UniversalShipment.
".Trim(), message);
			};
			ReadDataObjectAndAssertResult(dataObject, assertAction);
		}

		public void TestConfirm()
		{
			var booking = CreateAgencyShipment(ShipmentStatusList.Codes.Booked);
			var container = booking.BookedContainers.AddNew();
			container.JC_ContainerNum = "AAA";
			Factory.SaveForTesting();
			var dataObject = CreateDataObject();
			dataObject.ShipmentStatus = new CodeDescriptionPair { Code = ShipmentStatusList.Codes.WebFwdInstruction };
			dataObject.SetContainerCollection(() => new DataObjectList<Container> { new Container { ContainerNumber = "AAA" }, new Container { ContainerNumber = "BBB" } });
			Action<AgencyShipment, string> assertAction = (readBizObj, message) =>
			{
				AssertEquals("matched booking", booking.PK, readBizObj.PK);
				AssertEquals("confimed booking", true, booking.IsBillOfLadingStage);
				AssertEquals("updated shipment status", ShipmentStatusList.Codes.WebFwdInstruction, readBizObj.JS_ShipmentStatus);
				AssertContainsExactElementsInAnyOrder("updated containers", new[] { "AAA", "BBB" }, readBizObj.RealContainers.Cast<BillOfLadingContainer>().Select(c => c.JC_ContainerNum.ToString()));
				AssertMultilineASCIIEquals("import logs", @"
Information - Successfully loaded matching BillOfLading.
Information - Populating BillOfLading...
Information - Confirmed Shipping Bill of Lading V00001000.
Information - Successfully loaded matching BillOfLadingContainer.
Information - Populating BillOfLadingContainer...
Information - No matching BillOfLadingContainer found, creating new BillOfLadingContainer.
Information - Populating BillOfLadingContainer...
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: SGSIN
Information - Attempting to get Schedule for the Transport Leg
Information - A Schedule will not be created. Attempting to find an existing Schedule...
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Information - Updated Shipping Forwarding Instruction V00001000 from UniversalShipment.
".Trim(), message);
			};
			ReadDataObjectAndAssertResult(dataObject, assertAction);
		}

		public void TestConfirmWhenNoShipmentStatusIsSpecified()
		{
			var booking = CreateAgencyShipment(ShipmentStatusList.Codes.Booked);
			var container = booking.BookedContainers.AddNew();
			container.JC_ContainerNum = "AAA";
			Factory.SaveForTesting();
			var dataObject = CreateDataObject();
			dataObject.SetContainerCollection(() => new DataObjectList<Container> { new Container { ContainerNumber = "AAA" }, new Container { ContainerNumber = "BBB" } });
			Action<AgencyShipment, string> assertAction = (readBizObj, message) =>
			{
				AssertEquals("matched booking", booking.PK, readBizObj.PK);
				AssertEquals("confirmed booking", true, booking.IsBillOfLadingStage);
				AssertEquals("updated shipment status", ShipmentStatusList.Codes.Confirmed, readBizObj.JS_ShipmentStatus);
				AssertContainsExactElementsInAnyOrder("updated containers", new[] { "AAA", "BBB" }, readBizObj.RealContainers.Cast<BillOfLadingContainer>().Select(c => c.JC_ContainerNum.ToString()));
				AssertMultilineASCIIEquals("import logs", @"
Information - Successfully loaded matching BillOfLading.
Information - Populating BillOfLading...
Information - Confirmed Shipping Bill of Lading V00001000.
Information - Successfully loaded matching BillOfLadingContainer.
Information - Populating BillOfLadingContainer...
Information - No matching BillOfLadingContainer found, creating new BillOfLadingContainer.
Information - Populating BillOfLadingContainer...
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: SGSIN
Information - Attempting to get Schedule for the Transport Leg
Information - A Schedule will not be created. Attempting to find an existing Schedule...
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.
Information - Updated Shipping Bill of Lading V00001000 from UniversalShipment.
".Trim(), message);
			};
			ReadDataObjectAndAssertResult(dataObject, assertAction);
		}

		public void TestDisregardUniversalShipmentWithBookingStatus()
		{
			var billOfLading = CreateAgencyShipment(ShipmentStatusList.Codes.Confirmed);
			var dataObject = CreateDataObject();
			dataObject.ShipmentStatus = new CodeDescriptionPair { Code = ShipmentStatusList.Codes.Booked };
			Action<AgencyShipment, string> assertAction = (readBizObj, message) =>
			{
				AssertEquals("matched bill of lading", billOfLading.PK, readBizObj.PK);
				AssertEquals("should not update bill of lading from UniversalShipment", ShipmentStatusList.Codes.Confirmed, billOfLading.JS_ShipmentStatus);
				AssertMultilineASCIIEquals("import logs", @"
Information - Successfully loaded matching BillOfLading.
Error - Cannot populate BillOfLading because:
Data object contains shipment status [BKD] that is invalid in this scope.
".Trim(), message);
			};
			ReadDataObjectAndAssertResult(dataObject, assertAction);
		}

		public void TestReadIntoBusinessObject_ShippingInstruction_SetESIShipmentStatus()
		{
			var billOfLading = Factory.New<BillOfLading>();
			billOfLading.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			billOfLading.JS_UniqueConsignRef = "McLaren";
			Factory.SaveForTesting();
			var dataContext = new DataContext();
			dataContext.RecipientRoleCollection = new List<RecipientRole>();
			dataContext.RecipientRoleCollection.Add(new RecipientRole { ServiceCode = ServiceCodeType.SIN });
			dataContext.DataTargetCollection = new List<DataTarget>()
			{ new DataTarget { Key = "McLaren", Type = "BillOfLading" } };
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.ShipmentStatus = new CodeDescriptionPair { Code = ShipmentStatusList.Codes.Confirmed };
			dataObject.DataContext = dataContext;
			Action<AgencyShipment, string> assertAction = (readBizObj, message) =>
			{
				AssertEquals("matched booking", billOfLading.PK, readBizObj.PK);
				AssertEquals("New shipment status", ShipmentStatusList.Codes.ElectronicShippingInstruction, billOfLading.JS_ShipmentStatus);
				AssertMultilineASCIIEquals("import logs", @"
Information - Successfully loaded matching BillOfLading.
Information - Populating BillOfLading...
Information - Updated Shipping Shipment McLaren from UniversalShipment.
".Trim(), message);
			};
			ReadDataObjectAndAssertResult(dataObject, assertAction);
		}

		public void TestReadIntoBusinessObject_ShippingInstruction_ConfirmBooking()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var booking = Factory.New<AgencyBooking>();
				booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				booking.JS_UniqueConsignRef = "V00001000";
				Factory.SaveForTesting();

				var dataContext = new DataContext();
				dataContext.RecipientRoleCollection = new List<RecipientRole>();
				dataContext.RecipientRoleCollection.Add(new RecipientRole { ServiceCode = ServiceCodeType.SIN });
				dataContext.DataTargetCollection = new List<DataTarget>() { new DataTarget { Key = "V00001000", Type = "BillOfLading" } };

				var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				dataObject.ShipmentStatus = new CodeDescriptionPair { Code = ShipmentStatusList.Codes.Confirmed };
				dataObject.DataContext = dataContext;

				Action<AgencyShipment, string> assertAction = (readBizObj, message) =>
				{
					Assert(readBizObj.Logs.GetAllLogs().OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == Constants.EventReferenceMessageTypes.ShipmentStatus && x.Parameters[Params.New] == ShipmentStatusList.Codes.ElectronicShippingInstruction));
					AssertEquals("matched booking", booking.PK, readBizObj.PK);
					AssertEquals("New shipment status", ShipmentStatusList.Codes.ElectronicShippingInstruction, booking.JS_ShipmentStatus);
					AssertMultilineASCIIEquals("import logs", @"
Information - Successfully loaded matching BillOfLading.
Information - Populating BillOfLading...
Information - Confirmed Shipping Bill of Lading V00001000.
Information - Updated Shipping Shipment V00001000 from UniversalShipment.
".Trim(), message);
				};
				ReadDataObjectAndAssertResult(dataObject, assertAction);
			}
		}

		public void TestReadIntoBusinessObject_ShippingInstruction_StatusUpdatedEvent()
		{
			var factory = new BusinessObjectFactory();
			var booking = factory.New<AgencyBooking>();
			booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			booking.JS_UniqueConsignRef = "V00001000";
			booking.BookingPartyDocumentaryAddress.E2_OA_Address = BookingParty.MainAddress.PK;
			factory.Save();

			using (Factory.BOFactory.AddDisposableService())
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var dataContext = new DataContext();
				dataContext.RecipientRoleCollection = new List<RecipientRole>() { new RecipientRole { ServiceCode = ServiceCodeType.SIN, Code = RecipientRoleType.CAR } };
				dataContext.DataTargetCollection = new List<DataTarget>() { new DataTarget { Key = "V00001000", Type = "BillOfLading" } };
				dataContext.DocumentaryOverride = new DocumentaryOverride { Purpose = new CodeDescriptionPair { Code = "ORG", Description = "Original" } };

				var dataObject = CreateDataObject();
				dataObject.ShipmentStatus = new CodeDescriptionPair { Code = ShipmentStatusList.Codes.Confirmed };
				var attachedDocuments = new List<AttachedDocument>
				{
					AttachedDocumentCreator.Create(
						fileName: "Shipping Instruction.pdf",
						base64ImageData: "SGVsbG8sIFdvcmxkIQ==",
						documentType: "PDF")
				};

				dataObject.SetAttachedDocumentCollection(() => attachedDocuments);
				dataObject.DataContext = dataContext;
				dataObject.AgentsReference = "TS001";
				dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
						Address1 = BookingParty.MainAddress.OA_Address1,
						OrganizationCode = BookingParty.OH_Code,
						CompanyName = BookingParty.OH_FullName
					}
				});

				Action<AgencyShipment, string> assertActionORG = (readBizObj, message) =>
				{
					AssertEquals("matched booking", booking.PK, readBizObj.PK);
					AssertMultilineASCIIEquals("import logs", @"
Information - Successfully loaded matching BillOfLading.
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Populating BillOfLading...
Information - Confirmed Shipping Bill of Lading V00001000.
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: SGSIN
Information - Attempting to get Schedule for the Transport Leg
Information - An existing vessel could not be found. A Schedule will not be created. Attempting to find an existing Schedule...
Information - An existing Schedule could not be found. The Transport Leg will not be linked.
Information - Transport Leg updated.
Information - Successfully Added eDoc: Shipping Instruction.pdf.
Information - Updated Shipping Shipment V00001000 from UniversalShipment.
".Trim(), message);
				};
				ReadDataObjectAndAssertResult(dataObject, assertActionORG);
				Factory.SaveForTesting();

				var billOfLading = Factory.Load<BillOfLading>(booking.PK);

				AssertEquals("New shipment status", ShipmentStatusList.Codes.ElectronicShippingInstruction, billOfLading.JS_ShipmentStatus);
				AssertEquals(1, billOfLading.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters["TYP"] == "Shipment Status" && x.Parameters["NEW"] == "ESI"));
				AssertEquals("Original|NEW=ESI|OLD=BKD|RES=Electronic Shipping Instruction Received|TYP=Shipment Status", billOfLading.Logs.MostRecentLogByEventTime(Events.StatusUpdated).SL_Reference);
			}
		}

		public void TestReadIntoBusinessObject_ShippingInstruction_StatusUpdatedEvent_AMD()
		{
			var factory = new BusinessObjectFactory();
			var billOfLading = factory.New<BillOfLading>();
			billOfLading.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			billOfLading.JS_UniqueConsignRef = "V00001000";
			billOfLading.JS_HouseBill = "S0001";
			billOfLading.BookingPartyDocumentaryAddress.E2_OA_Address = BookingParty.MainAddress.PK;
			factory.Save();

			using (Factory.BOFactory.AddDisposableService())
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var dataContext = new DataContext();
				dataContext.RecipientRoleCollection = new List<RecipientRole>() { new RecipientRole { ServiceCode = ServiceCodeType.SIN, Code = RecipientRoleType.CAR } };
				dataContext.DataTargetCollection = new List<DataTarget>() { new DataTarget { Key = "V00001000", Type = "BillOfLading" } };
				dataContext.DocumentaryOverride = new DocumentaryOverride { Purpose = new CodeDescriptionPair { Code = "AMD", Description = "Amendment" } };

				var dataObject = CreateDataObject();
				dataObject.ShipmentStatus = new CodeDescriptionPair { Code = ShipmentStatusList.Codes.Confirmed };
				var attachedDocuments = new List<AttachedDocument>
				{
					AttachedDocumentCreator.Create(
						fileName: "Shipping Instruction.pdf",
						base64ImageData: "SGVsbG8sIFdvcmxkIQ==",
						documentType: "PDF")
				};

				dataObject.SetAttachedDocumentCollection(() => attachedDocuments);
				dataObject.DataContext = dataContext;
				dataObject.AgentsReference = "TS001";
				dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
						Address1 = BookingParty.MainAddress.OA_Address1,
						OrganizationCode = BookingParty.OH_Code,
						CompanyName = BookingParty.OH_FullName
					}
				});

				Action<AgencyShipment, string> assertActionORG = (readBizObj, message) =>
				{
					AssertEquals("matched billOfLading", billOfLading.PK, readBizObj.PK);
					AssertMultilineASCIIEquals("import logs", @"
Information - Successfully loaded matching BillOfLading.
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Populating BillOfLading...
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: SGSIN
Information - Attempting to get Schedule for the Transport Leg
Information - An existing vessel could not be found. A Schedule will not be created. Attempting to find an existing Schedule...
Information - An existing Schedule could not be found. The Transport Leg will not be linked.
Information - Transport Leg updated.
Information - Successfully Added eDoc: Shipping Instruction.pdf.
Information - Updated Shipping Shipment V00001000 from UniversalShipment.
".Trim(), message);
				};
				ReadDataObjectAndAssertResult(dataObject, assertActionORG);
				Factory.SaveForTesting();

				billOfLading = Factory.Load<BillOfLading>(billOfLading.PK);

				AssertEquals("New shipment status", ShipmentStatusList.Codes.ElectronicShippingInstruction, billOfLading.JS_ShipmentStatus);
				AssertEquals(1, billOfLading.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters["TYP"] == "Shipment Status" && x.Parameters["NEW"] == "ESI"));
				AssertEquals("Amendment|NEW=ESI|OLD=CNF|RES=Electronic Shipping Instruction Received|TYP=Shipment Status", billOfLading.Logs.MostRecentLogByEventTime(Events.StatusUpdated).SL_Reference);
			}
		}

		public void TestReadIntoBusinessObject_ShippingInstruction_StatusUpdatedEvent_AMD_DisableRegistry()
		{
			var factory = new BusinessObjectFactory();
			var billOfLading = factory.New<BillOfLading>();
			billOfLading.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			billOfLading.JS_UniqueConsignRef = "V00001000";
			billOfLading.JS_HouseBill = "S0001";
			billOfLading.BookingPartyDocumentaryAddress.E2_OA_Address = BookingParty.MainAddress.PK;
			factory.Save();

			using (Factory.BOFactory.AddDisposableService())
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var dataContext = new DataContext();
				dataContext.RecipientRoleCollection = new List<RecipientRole>() { new RecipientRole { ServiceCode = ServiceCodeType.SIN, Code = RecipientRoleType.CAR } };
				dataContext.DataTargetCollection = new List<DataTarget>() { new DataTarget { Key = "V00001000", Type = "BillOfLading" } };
				dataContext.DocumentaryOverride = new DocumentaryOverride { Purpose = new CodeDescriptionPair { Code = "AMD", Description = "Amendment" } };

				var dataObject = CreateDataObject();
				dataObject.ShipmentStatus = new CodeDescriptionPair { Code = ShipmentStatusList.Codes.Confirmed };
				var attachedDocuments = new List<AttachedDocument>
				{
					AttachedDocumentCreator.Create(
						fileName: "Shipping Instruction.pdf",
						base64ImageData: "SGVsbG8sIFdvcmxkIQ==",
						documentType: "PDF")
				};

				dataObject.SetAttachedDocumentCollection(() => attachedDocuments);
				dataObject.DataContext = dataContext;
				dataObject.AgentsReference = "TS001";
				dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
						Address1 = BookingParty.MainAddress.OA_Address1,
						OrganizationCode = BookingParty.OH_Code,
						CompanyName = BookingParty.OH_FullName
					}
				});

				Action<AgencyShipment, string> assertActionORG = (readBizObj, message) =>
				{
					AssertEquals("matched billOfLading", billOfLading.PK, readBizObj.PK);
					AssertMultilineASCIIEquals("import logs", @"
Information - Successfully loaded matching BillOfLading.
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - Populating BillOfLading...
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKG' by code, address 'Booking Party Ln' (only address).
Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: SGSIN
Information - Attempting to get Schedule for the Transport Leg
Information - An existing vessel could not be found. A Schedule will not be created. Attempting to find an existing Schedule...
Information - An existing Schedule could not be found. The Transport Leg will not be linked.
Information - Transport Leg updated.
Information - Successfully Added eDoc: Shipping Instruction.pdf.
Information - Updated Shipping Shipment V00001000 from UniversalShipment.
".Trim(), message);
				};
				ReadDataObjectAndAssertResult(dataObject, assertActionORG);
				Factory.SaveForTesting();

				billOfLading = Factory.Load<BillOfLading>(billOfLading.PK);

				AssertEquals("New shipment status", ShipmentStatusList.Codes.ElectronicShippingInstruction, billOfLading.JS_ShipmentStatus);
				AssertEquals(1, billOfLading.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters["TYP"] == "Shipment Status" && x.Parameters["NEW"] == "ESI"));
				AssertEquals("|NEW=ESI|OLD=CNF|RES=Electronic Shipping Instruction Received|TYP=Shipment Status", billOfLading.Logs.MostRecentLogByEventTime(Events.StatusUpdated).SL_Reference);
			}
		}

		public void TestReadIntoBusinessObject_ShippingInstruction_StatusUpdatedEvent_DisableElectronicBookingAndShippingInstructions()
		{
			var booking = Factory.New<BillOfLading>();
			booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			booking.JS_UniqueConsignRef = "V00001000";
			Factory.SaveForTesting();

			using (Factory.BOFactory.AddDisposableService())
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var dataContext = new DataContext();
				dataContext.RecipientRoleCollection = new List<RecipientRole>() { new RecipientRole { ServiceCode = ServiceCodeType.SIN } };
				dataContext.DataTargetCollection = new List<DataTarget>() { new DataTarget { Key = "V00001000", Type = "BillOfLading" } };
				dataContext.DocumentaryOverride = new DocumentaryOverride { Purpose = new CodeDescriptionPair { Code = "ORG", Description = "Original" } };

				var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				dataObject.ShipmentStatus = new CodeDescriptionPair { Code = ShipmentStatusList.Codes.Confirmed };
				var attachedDocuments = new List<AttachedDocument>
				{
					AttachedDocumentCreator.Create(
						fileName: "Shipping Instruction.pdf",
						base64ImageData: "SGVsbG8sIFdvcmxkIQ==",
						documentType: "PDF")
				};

				dataObject.SetAttachedDocumentCollection(() => attachedDocuments);
				dataObject.DataContext = dataContext;

				Action<AgencyShipment, string> assertActionORG = (readBizObj, message) =>
				{
					AssertEquals("matched booking", booking.PK, readBizObj.PK);
					AssertEquals("New shipment status", ShipmentStatusList.Codes.ElectronicShippingInstruction, booking.JS_ShipmentStatus);
					AssertMultilineASCIIEquals("import logs", @"
Information - Successfully loaded matching BillOfLading.
Information - Populating BillOfLading...
Information - Confirmed Shipping Bill of Lading V00001000.
Information - Successfully Added eDoc: Shipping Instruction.pdf.
Information - Updated Shipping Shipment V00001000 from UniversalShipment.
".Trim(), message);
				};
				ReadDataObjectAndAssertResult(dataObject, assertActionORG);
				Factory.SaveForTesting();

				AssertEquals(1, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters["TYP"] == "Shipment Status" && x.Parameters["NEW"] == "ESI"));
			}
		}

		#region Carrier VGM Message ReasonForNotAbleToUpdate

		public void TestImportCarrierVGMUniversalShipment_ContainerNumberIsEmpty()
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
	<BookingConfirmationReference>BK00012</BookingConfirmationReference>
	<WayBillNumber>BL00012</WayBillNumber>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber></ContainerNumber>
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
</UniversalShipment>");

			#endregion

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CreateBillOfLading("BK00012", "BL00012", "TEST1111114", ShipmentStatusList.Codes.Confirmed);

				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Error - Cannot populate AgencyBooking because:
[*VGM message received without Container Number. Container Number is mandatory to process the VGM. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());
			}
		}

		public void TestImportCarrierVGMUniversalShipment_CannotMatchContainerNumber()
		{
			#region message

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
	<BookingConfirmationReference>BK00012</BookingConfirmationReference>
	<WayBillNumber>BL00012</WayBillNumber>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TEST1111113</ContainerNumber>
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
				var billOfLading = CreateBillOfLading("BK00012", "BL00012", "TEST1111114", ShipmentStatusList.Codes.Confirmed);

				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Error - Cannot populate AgencyBooking because:
[*VGM message failed because Container TEST1111113 does not exist in Bill of Lading BL00012. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());

				billOfLading.JS_CFSReference = string.Empty;
				Factory.SaveForTesting();

				manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				message = GetQueuedUniversalShipmentMessage(xml);
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Error - Cannot populate AgencyBooking because:
[*VGM message failed because Container TEST1111113 does not exist in Bill of Lading BL00012. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());

				billOfLading.JS_CFSReference = "BK00012";
				billOfLading.JS_HouseBill = ZString.Empty;
				Factory.SaveForTesting();

				manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				message = GetQueuedUniversalShipmentMessage(xml);
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Error - Cannot populate AgencyBooking because:
[*VGM message failed because Container TEST1111113 does not exist in Bill of Lading with Booking number BK00012. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());
			}
		}

		public void TestImportCarrierVGMUniversalShipment_CannotMatchContainerNumber_LoadAgencyBooking_BookingConfirmationReferenceIsNotEmpty()
		{
			#region message

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
	<WayBillNumber>BL00012</WayBillNumber>
	<BookingConfirmationReference>BK00012</BookingConfirmationReference>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TEST1111113</ContainerNumber>
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
				CreateBillOfLading("BK00012", ZString.Empty, ZString.Empty, ShipmentStatusList.Codes.Booked, addSailing: false);

				var message = GetQueuedUniversalShipmentMessage(xml);
				var billOfLading = CreateBillOfLading("BK00012", "BL00012", "TEST1111114", ShipmentStatusList.Codes.Confirmed, addSailing: false);

				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*VGM message failed because Container TEST1111113 does not exist in Bill of Lading BL00012. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());

				billOfLading.JS_CFSReference = string.Empty;
				Factory.SaveForTesting();

				manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				message = GetQueuedUniversalShipmentMessage(xml);
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*VGM message failed because Container TEST1111113 does not exist in Bill of Lading BL00012. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());

				billOfLading.JS_CFSReference = "BK00012";
				billOfLading.JS_HouseBill = ZString.Empty;
				Factory.SaveForTesting();

				manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				message = GetQueuedUniversalShipmentMessage(xml);
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*VGM message failed because Container TEST1111113 does not exist in Bill of Lading with Booking number BK00012. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());
			}
		}

		public void TestImportCarrierVGMUniversalShipment_CannotMatchContainerNumber_LoadAgencyBooking_BookingConfirmationReferenceIsEmpty()
		{
			#region message

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
	<WayBillNumber>BL00012</WayBillNumber>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TEST1111113</ContainerNumber>
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
				CreateBillOfLading("BK00012", ZString.Empty, ZString.Empty, ShipmentStatusList.Codes.Booked, addSailing: false);

				var message = GetQueuedUniversalShipmentMessage(xml);
				var billOfLading = CreateBillOfLading("BK00012", "BL00012", "TEST1111114", ShipmentStatusList.Codes.Confirmed, addSailing: false);

				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*VGM message failed because Container TEST1111113 does not exist in Bill of Lading BL00012. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());

				billOfLading.JS_CFSReference = string.Empty;
				Factory.SaveForTesting();

				manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				message = GetQueuedUniversalShipmentMessage(xml);
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*VGM message failed because Container TEST1111113 does not exist in Bill of Lading BL00012. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());

				billOfLading.JS_CFSReference = "BK00012";
				billOfLading.JS_HouseBill = ZString.Empty;
				Factory.SaveForTesting();

				manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				message = GetQueuedUniversalShipmentMessage(xml);
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
[*VGM message with Bill Number BL00012 cannot find a matching Bill of Lading to update. Message rejected.*]
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
	<WayBillNumber>BL00001</WayBillNumber>
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
				CreateBillOfLading(new ZString(), "BL00001", "TEST1111111", ShipmentStatusList.Codes.Confirmed);

				var containerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
				containerType.RC_GrossWeight = 25379;
				Factory.SaveForTesting();

				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Successfully loaded matching BillOfLading.
Error - Cannot populate BillOfLading because:
[*VGM message failed because Container TEST1649263 does not exist in Bill of Lading BL00001. Message rejected.*]
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
	<WayBillNumber>BL00004</WayBillNumber>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TEST4444444</ContainerNumber>
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
				var billOfLading = CreateBillOfLading("BK00004", "BL00004", "TEST4444444", ShipmentStatusList.Codes.Confirmed);
				var legs = billOfLading.Transports.Cast<Transport>();
				var mainSeaLeg = legs.FirstOrDefault(x => x.JW_TransportMode == Constants.TransportModes.Sea &&
														x.JW_TransportType == Constants.TransportPlanningType.MainVessel);
				mainSeaLeg.JW_ATD = new ZDateTime(2024, 03, 01, 12, 15, 00);
				Factory.SaveForTesting();

				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Successfully loaded matching BillOfLading.
Error - Cannot populate BillOfLading because:
[*VGM message cannot be accepted after vessel ATD. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());
			}
		}

		[TestDate(2024, 03, 05)]
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
	<WayBillNumber>BL00004</WayBillNumber>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TEST4444444</ContainerNumber>
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
				var billOfLading = CreateBillOfLading("BK00004", "BL00004", "TEST4444444", ShipmentStatusList.Codes.Confirmed);
				var legs = billOfLading.Transports.Cast<Transport>();
				var mainSeaLeg = legs.FirstOrDefault(x => x.JW_TransportMode == Constants.TransportModes.Sea &&
														x.JW_TransportType == Constants.TransportPlanningType.MainVessel);
				mainSeaLeg.JW_ATD = new ZDateTime(2024, 03, 01, 12, 15, 00);
				Factory.SaveForTesting();

				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Successfully loaded matching BillOfLading.
Error - Cannot populate BillOfLading because:
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
	<WayBillNumber>BL00005</WayBillNumber>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TEST5555555</ContainerNumber>
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
				var billOfLading = CreateBillOfLading("BK00005", "BL00005", "TEST5555555", ShipmentStatusList.Codes.Confirmed);

				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Successfully loaded matching BillOfLading.
Error - Cannot populate BillOfLading because:
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
	<BookingConfirmationReference>BK00006</BookingConfirmationReference>
	<WayBillNumber>BL00006</WayBillNumber>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TEST6666666</ContainerNumber>
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
				var billOfLading = CreateBillOfLading("BK00006", "BL00006", "TEST6666666", ShipmentStatusList.Codes.Confirmed);

				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Successfully loaded matching BillOfLading.
Error - Cannot populate BillOfLading because:
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
	<BookingConfirmationReference>BK00007</BookingConfirmationReference>
	<WayBillNumber>BL00007</WayBillNumber>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TEST7777777</ContainerNumber>
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
				var billOfLading = CreateBillOfLading("BK00007", "BL00007", "TEST7777777", ShipmentStatusList.Codes.Confirmed);

				var containerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
				containerType.RC_GrossWeight = 2400;
				Factory.SaveForTesting();

				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Successfully loaded matching BillOfLading.
Error - Cannot populate BillOfLading because:
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
	<BookingConfirmationReference>BK00008</BookingConfirmationReference>
	<WayBillNumber>BL00008</WayBillNumber>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TEST8888888</ContainerNumber>
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
				var billOfLading = CreateBillOfLading("BK00008", "BL00008", "TEST8888888", ShipmentStatusList.Codes.Confirmed);

				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Successfully loaded matching BillOfLading.
Populating BillOfLading...
Successfully loaded matching BillOfLadingContainer.
Populating BillOfLadingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Updated Shipping Bill of Lading V00001000 from UniversalShipment.
Successfully saved Shipping Bill of Lading V00001000 with 1 x BillOfLadingContainer.", message.GetLogNoteText());

				var billOfLadingUpdated = new BusinessObjectFactory().Load<BillOfLading>(billOfLading.PK);
				var containers = billOfLadingUpdated.RealContainers.Cast<BillOfLadingContainer>();
				var container = containers.FirstOrDefault(x => x.JC_ContainerNum == "TEST8888888");
				var vgmDate = new ZDateTime(2024, 03, 13, 8, 45, 00);

				AssertEquals("VGM Gross Weight:", (ZDecimal)5000, container.JC_GrossWeight);
				AssertEquals("VGM Verification Date:", vgmDate, container.JC_GrossWeightVerificationDateTime);
				AssertEquals("VGM Verified By:", bookingParty.PK, container.GrossWeightVerifiedByPK);
				AssertEquals("VGM Verification Type:", "CNT", container.JC_GrossWeightVerificationType);

				var qtvEvent = billOfLadingUpdated.Logs.Find(l => l.SL_SE_NKEvent == "QTV").FirstOrDefault() ?? container.Logs.Find(l => l.SL_SE_NKEvent == "QTV").FirstOrDefault();
				AssertNotNull(qtvEvent);
			}
		}

		BillOfLading CreateBillOfLading(ZString bookingNumber, ZString billOfLadingNumber, ZString containerNumber, ZString shipmentStatus, ZString shippersReference = new ZString(), bool addSailing = true)
		{
			var billOfLading = Factory.New<BillOfLading>();
			billOfLading.JS_ShipmentStatus = shipmentStatus;
			billOfLading.JS_HouseBill = billOfLadingNumber;
			billOfLading.JS_CFSReference = bookingNumber;
			billOfLading.JS_BookingReference = shippersReference;
			billOfLading.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			if (addSailing)
			{
				var sailing = UniversalTestHelper.CreateSailingWithVoyage(Factory.BOFactory, "AUSYD", "SGSIN", "USS ZAMBEZI", "001");
				billOfLading.JS_JX = sailing.PK;
			}

			var container = billOfLading.RealContainers.AddNew();
			container.JC_ContainerNum = containerNumber;
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_TareWeight = 2000;
			container.JC_GrossWeightUQ = Constants.Weight.Kilograms;
			container.JC_GrossWeight = 4500m;

			var packLine = billOfLading.OuterPackLines.AddNew();
			packLine.JL_Description = "shampoo";
			packLine.JL_PackageCount = 4;
			packLine.JL_ActualVolumeUQ = "M3";
			packLine.JL_ActualWeight = 2500;
			container.PackLines.Add(packLine);

			var transport = billOfLading.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport.JW_Vessel = "USS ZAMBEZI";
			transport.JW_VoyageFlight = "001";
			transport.JW_ETD = new ZDateTime(2024, 03, 20, 8, 45, 00);
			transport.JW_ETA = new ZDateTime(2024, 03, 21, 12, 15, 00);

			Factory.SaveForTesting();

			return billOfLading;
		}

		#endregion

		#region Implementation

		protected override ITopLevelDataObjectReader GetReader(UniversalShipment dataObject)
		{
			return new BillOfLadingDataObjectReader(dataObject, new TestErrorLogger(), Factory);
		}

		protected override AgencyShipmentDataObjectReader<BillOfLading> GetReader(UniversalShipment dataObject, IXmlImportLogger logger)
		{
			return new BillOfLadingDataObjectReader(dataObject, logger, Factory);
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
			dataObject.ShipmentStatus = new CodeDescriptionPair { Code = ShipmentStatusList.Codes.Confirmed };
			dataObject.SetDateCollection(() => new List<Date> { new Date { Type = DateType.BookingConfirmed, Value = new ZDateTime(2012, 2, 1) }, new Date { Type = DateType.Received, Value = new ZDateTime(2012, 2, 2) }, new Date { Type = DateType.Departure, Value = new ZDateTime(2012, 2, 3) }, new Date { Type = DateType.Arrival, Value = new ZDateTime(2012, 2, 4) }, new Date { Type = DateType.BillIssued, Value = new ZDateTime(2012, 2, 5) }, new Date { Type = DateType.ShippedOnBoard, Value = new ZDateTime(2012, 2, 6) } });
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{ AddressType = nameof(DocAddressType.ShippingLineAddress), Address1 = carrier.MainAddress.OA_Address1, OrganizationCode = carrier.OH_Code }, new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{ AddressType = nameof(DocAddressType.Principal), Address1 = principal.MainAddress.OA_Address1, OrganizationCode = principal.OH_Code }, new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{ AddressType = nameof(DocAddressType.LocalClient), Address1 = localClient.MainAddress.OA_Address1, OrganizationCode = localClient.OH_Code }, new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{ AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress), Address1 = consignor.MainAddress.OA_Address1, OrganizationCode = consignor.OH_Code }, new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{ AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress), Address1 = consignee.MainAddress.OA_Address1, OrganizationCode = consignee.OH_Code }, new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{ AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress), Address1 = BookingParty.MainAddress.OA_Address1, OrganizationCode = BookingParty.OH_Code }, new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{ AddressType = nameof(DocAddressType.NotifyParty), Address1 = notifyParty.MainAddress.OA_Address1, OrganizationCode = notifyParty.OH_Code }, new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{ AddressType = nameof(DocAddressType.NotifyParty2), Address1 = notifyParty2.MainAddress.OA_Address1, OrganizationCode = notifyParty2.OH_Code }, new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{ AddressType = nameof(DocAddressType.NotifyParty3), Address1 = notifyParty3.MainAddress.OA_Address1, OrganizationCode = notifyParty3.OH_Code } });
			dataObject.SetNoteCollection(() => new DataObjectList<Note> { new Note { Description = "CAT EATER", Visibility = new CodeDescriptionPair { Code = "PUB", Description = "Public" }, NoteContext = new NoteContext { Code = "BEB", Description = "Baby Eats Banana" } } });
			dataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference> { new AdditionalReference { Type = new EntryType { Code = "CON", Description = "Contract Number" }, ReferenceNumber = "Additional reference number" } });
			dataObject.SetContainerCollection(() => new DataObjectList<Container> { new Container { ContainerNumber = "AAA", ContainerType = new ContainerType { Code = "20GP", ISOCode = "22G0" } }, new Container { ContainerNumber = "BBB", ContainerType = new ContainerType { Code = "20GP", ISOCode = "22G0" } }, new Container { ContainerNumber = "CCC", ContainerType = new ContainerType { Code = "20GP", ISOCode = "22G0" }, Link = 4 } });
			dataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{ GoodsDescription = "camel toes" }, new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{ ContainerNumber = "AAA", GoodsDescription = "cups" }, new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{ ContainerLink = 4, GoodsDescription = "balls" } });
			dataObject.PackingLineCollection.Content = CollectionContent.Complete;
			dataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg> { new TransportLeg { PortOfLoading = new UNLOCO { Code = "AUSYD" }, PortOfDischarge = new UNLOCO { Code = "NZAKL" } }, new TransportLeg { PortOfLoading = new UNLOCO { Code = "NZAKL" }, PortOfDischarge = new UNLOCO { Code = "USMIA" } } });
			return dataObject;
		}

		protected override string GetExpectedShipmentMap()
		{
			using (var retriever = new EmbeddedResourceRetriever())
			{
				return retriever.GetString("Enterprise.Freight.Agency.DataTransfer.Test.Universal.BillOfLading.TestFiles.BillOfLading_FieldMap.txt");
			}
		}

		protected override string CreateFieldMap(BusinessObject billOfLading)
		{
			var mapper = new BillOfLadingFieldMapper((BillOfLading)billOfLading);
			return mapper.Map();
		}

		#endregion

		#region SetUp

		protected override void SetUp()
		{
			base.SetUp();
			BookingParty.OH_FullName = "TEST COMPANY";
			Factory.SaveForTesting();

			carrier = Factory.New<OrgHeader>();
			carrier.MainAddress.OA_Address1 = "Carrier Ln";
			carrier.OH_Code = "CAR";

			principal = Factory.New<OrgHeader>();
			principal.MainAddress.OA_Address1 = "Principal Ave";
			principal.OH_Code = "PRC";

			localClient = Factory.New<OrgHeader>();
			localClient.MainAddress.OA_Address1 = "Local Client";
			localClient.OH_Code = "LOC";

			consignor = Factory.New<OrgHeader>();
			consignor.MainAddress.OA_Address1 = "Consignor St";
			consignor.OH_Code = "CNR";

			consignee = Factory.New<OrgHeader>();
			consignee.MainAddress.OA_Address1 = "Consignee Ave";
			consignee.OH_Code = "CNE";

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
		OrgHeader localClient;
		OrgHeader consignor;
		OrgHeader consignee;
		OrgHeader notifyParty;
		OrgHeader notifyParty2;
		OrgHeader notifyParty3;
		OrgHeader bookingParty;

		#endregion
	}
}
