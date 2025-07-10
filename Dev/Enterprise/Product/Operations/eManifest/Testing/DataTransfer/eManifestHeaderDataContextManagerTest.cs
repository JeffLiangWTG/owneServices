using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.eManifest.Business;
using Enterprise.eManifest.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eManifest.DataTransfer.Testing
{
	[TestedType(typeof(eManifestDataContextManager))]
	[DatCapabilityRequirement("SOURCE_CODE")]
	[SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplification hides desired base class")]
	sealed class eManifestHeaderDataContextManagerTest : ShipmentDataContextManagerTestCase<eManifestDataContextManager, SupplierBookingHeader>
	{
		public void TestImportUniversalShipment()
		{
			TestCaseHelper.ClearTable(JobShipmentSchema.Constants.TableName);
			TestCaseHelper.ClearTable(SupplierBookingHeaderSchema.Constants.TableName);
			TestCaseHelper.ClearTable(SupplierBookingLineSchema.Constants.TableName);

			var mainAddress = SetupSampleConsignor(Factory.BOFactory);
			var dispatchAddress = SetupSampleDispatchAddress(Factory.BOFactory);
			Factory.SaveForTesting();

			string universalXml = File.ReadAllText(TestFiles.GetPathFor("eManifestImport.xml"));

			var message = DataTransferTestHelper.GetQueuedUniversalDataMessage(Factory, universalXml, EDIMessageSubTypeList.Codes.XmlUniversalShipment, true);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"
Added Supplier Booking Line Z00003(Consignee='Joe Jones') from UniversalShipment.
Added Supplier Booking Line Z00005(Consignee='John Smith') from UniversalShipment.
Added Supplier Booking Line Z00012(Consignee='John Smith') from UniversalShipment.
Added Supplier Booking (Supplier='CARGOWSYD') from UniversalShipment.
Successfully saved Supplier Booking M00000001 (Supplier='CARGOWSYD') with 3 x SupplierBookingLine.".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
No matching SupplierBookingHeader found, creating new SupplierBookingHeader.
Populating SupplierBookingHeader...
Matching 'ConsignorDocumentaryAddress':- Matched to 'CARGOWSYD' by code, address 'P.O. BOX 6094' (only address).
Matching 'PickUpAddress':- Matched to 'SERVINSYD' by code, address 'Suite 81A/8-24 Kippax Str' (only address).
No matching SupplierBookingLine found, creating new SupplierBookingLine.
Populating SupplierBookingLine...
Added Supplier Booking Line Z00003(Consignee='Joe Jones') from UniversalShipment.
No matching SupplierBookingLine found, creating new SupplierBookingLine.
Populating SupplierBookingLine...
Added Supplier Booking Line Z00005(Consignee='John Smith') from UniversalShipment.
No matching SupplierBookingLine found, creating new SupplierBookingLine.
Populating SupplierBookingLine...
Added Supplier Booking Line Z00012(Consignee='John Smith') from UniversalShipment.
Added Supplier Booking (Supplier='CARGOWSYD') from UniversalShipment.
Successfully saved Supplier Booking M00000001 (Supplier='CARGOWSYD') with 3 x SupplierBookingLine.".Trim(), message.GetLogNoteText());

				var bookingHeaders = Factory.Load<SupplierBookingHeader>(new ZQuery());
				AssertEquals("SupplierBookingHeader", 1, bookingHeaders.Length);

				var bookingLines = Factory.Load<SupplierBookingLine>(new ZQuery());
				AssertEquals("SupplierBookingLine", 3, bookingLines.Length);

				var header = bookingHeaders[0];
				AssertEquals(SupplierBookingHeader.Schema.DH_CubicInM3, new ZDecimal(6676.000), header.DH_CubicInM3);
				AssertEquals(SupplierBookingHeader.Schema.DH_GrossWeightInKg, new ZDecimal(4545.000), header.DH_GrossWeightInKg);
				AssertEquals(SupplierBookingHeader.Schema.DH_IsShipperApproved, false, header.DH_IsShipperApproved);
				AssertEquals(SupplierBookingHeader.Schema.DH_OA_Consignor, mainAddress.PK, header.DH_OA_Consignor);
				AssertEquals(SupplierBookingHeader.Schema.DH_OA_DispatchAddress, dispatchAddress.PK, header.DH_OA_DispatchAddress);
				AssertEquals(SupplierBookingHeader.Schema.DH_OA_OriginDepot, Guid.Empty, header.DH_OA_OriginDepot);
				AssertEquals(SupplierBookingHeader.Schema.DH_OC_BookedBy, Guid.Empty, header.DH_OC_BookedBy);
				AssertEquals(SupplierBookingHeader.Schema.DH_OC_ConsignorContact, Guid.Empty, header.DH_OC_ConsignorContact);
				AssertEquals(SupplierBookingHeader.Schema.DH_OH_ConsignmentBroker, Guid.Empty, header.DH_OH_ConsignmentBroker);
				AssertEquals(SupplierBookingHeader.Schema.DH_PiecesManifested, 3213, header.DH_PiecesManifested);
				AssertEquals(SupplierBookingHeader.Schema.DH_SupplierReference, "M00000001", header.DH_SupplierReference);

				var lines = bookingLines.OrderBy(l => l.DL_ConsigneeReference).ToArray();

				var line1 = lines[0];
				AssertEquals(SupplierBookingLine.Schema.DL_DH_BookingHeader, header.PK, line1.DL_DH_BookingHeader);
				AssertEquals(SupplierBookingLine.Schema.DL_ConsigneeReference, "Z00003", line1.DL_ConsigneeReference);
				AssertEquals(SupplierBookingLine.Schema.DL_GoodsValue, new ZDecimal(1043.0330), line1.DL_GoodsValue);
				AssertEquals(SupplierBookingLine.Schema.DL_RX_NKGoodsValueCurrency, "AUD", line1.DL_RX_NKGoodsValueCurrency);
				AssertEquals(SupplierBookingLine.Schema.DL_GrossWeight, new ZDecimal(33451.040), line1.DL_GrossWeight);
				AssertEquals(SupplierBookingLine.Schema.DL_GrossWeightUQ, "KG", line1.DL_GrossWeightUQ);
				AssertEquals(SupplierBookingLine.Schema.DL_Cubic, new ZDecimal(3220.044), line1.DL_Cubic);
				AssertEquals(SupplierBookingLine.Schema.DL_CubicUQ, "M3", line1.DL_CubicUQ);
				AssertEquals(SupplierBookingLine.Schema.DL_GoodsDescription, "Balalayka", line1.DL_GoodsDescription);
				AssertEquals(SupplierBookingLine.Schema.DL_MarksAndNumbers, "This is really long description", line1.DL_MarksAndNumbers);
				AssertEquals(SupplierBookingLine.Schema.DL_PiecesManifested, 888, line1.DL_PiecesManifested);
				AssertEquals(SupplierBookingLine.Schema.DL_JS_ApprovedShipment, Guid.Empty, line1.DL_JS_ApprovedShipment);
				AssertEquals(SupplierBookingLine.Schema.DL_DO_LoadList, Guid.Empty, line1.DL_DO_LoadList);
				AssertEquals(SupplierBookingLine.Schema.DL_OrderTrackingNumber, "Reference_3323", line1.DL_OrderTrackingNumber);
				AssertEquals(SupplierBookingLine.Schema.DL_RS_NKServiceLevel, "D2D", line1.DL_RS_NKServiceLevel);
				AssertEquals(SupplierBookingLine.Schema.DL_IsDeliveryTransportSelfBooked, true, line1.DL_IsDeliveryTransportSelfBooked);
				AssertEquals(SupplierBookingLine.Schema.DL_SignatureRequired, true, line1.DL_SignatureRequired);
				AssertEquals(SupplierBookingLine.Schema.DL_F3_NKPackType, "PKG", line1.DL_F3_NKPackType);
				AssertEquals(SupplierBookingLine.Schema.DL_IsHazardous, true, line1.DL_IsHazardous);
				AssertEquals(SupplierBookingLine.Schema.DL_RequiresFumigation, false, line1.DL_RequiresFumigation);
				AssertEquals(SupplierBookingLine.Schema.DL_IsPersonalEffects, true, line1.DL_IsPersonalEffects);
				AssertEquals(SupplierBookingLine.Schema.DL_IsTimber, false, line1.DL_IsTimber);
				AssertEquals(SupplierBookingLine.Schema.DL_IsPerishable, true, line1.DL_IsPerishable);
				AssertEquals(SupplierBookingLine.Schema.DL_VendorIdentifier, "SELLINGCRAPCO", line1.DL_VendorIdentifier);

				AssertEquals(SupplierBookingLine.Schema.DL_DeliveryBarcode, "", line1.DL_DeliveryBarcode);
				AssertEquals(SupplierBookingLine.Schema.DL_Status, Constants.SupplierBookingLineStatus.Codes.Incomplete, line1.DL_Status);
				AssertEquals(SupplierBookingLine.Schema.DL_KM_LastMileTransportBooking, ZGuid.Empty, line1.DL_KM_LastMileTransportBooking);

				AssertEquals(SupplierBookingLine.Schema.DL_Index, 1, line1.DL_Index);
				AssertEquals(SupplierBookingLine.Schema.DL_Index, 2, lines[1].DL_Index);
				AssertEquals(SupplierBookingLine.Schema.DL_Index, 3, lines[2].DL_Index);

				var addresses = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_ParentID, line1.PK));
				AssertEquals("There should be no addresses linked to the line", 0, addresses.Length);

				AssertEquals(SupplierBookingLine.Schema.DL_ConsignorContact, ZString.Empty, line1.DL_ConsignorContact);
				AssertEquals(SupplierBookingLine.Schema.DL_ConsignorName, "Ebooks USA", line1.DL_ConsignorName);
				AssertEquals(SupplierBookingLine.Schema.DL_ConsignorAddress1, "1256 Santa Ana Blv", line1.DL_ConsignorAddress1);
				AssertEquals(SupplierBookingLine.Schema.DL_ConsignorCity, "Highland Park", line1.DL_ConsignorCity);
				AssertEquals(SupplierBookingLine.Schema.DL_ConsignorPostCode, "90042", line1.DL_ConsignorPostCode);
				AssertEquals(SupplierBookingLine.Schema.DL_ConsignorState, "CA", line1.DL_ConsignorState);
				AssertEquals(SupplierBookingLine.Schema.DL_RN_NKConsignorCountryCode, ZString.Empty, line1.DL_RN_NKConsignorCountryCode);
				AssertEquals(SupplierBookingLine.Schema.DL_ConsignorPhone, ZString.Empty, line1.DL_ConsignorPhone);
				AssertEquals(SupplierBookingLine.Schema.DL_ConsignorEmail, ZString.Empty, line1.DL_ConsignorEmail);

				AssertEquals(SupplierBookingLine.Schema.DL_ConsigneeContact, "Joe Jones", line1.DL_ConsigneeContact);
				AssertEquals(SupplierBookingLine.Schema.DL_ConsigneeName, "Joe Jones", line1.DL_ConsigneeName);
				AssertEquals(SupplierBookingLine.Schema.DL_ConsigneeAddress1, "1 Long Rd", line1.DL_ConsigneeAddress1);
				AssertEquals(SupplierBookingLine.Schema.DL_ConsigneeCity, "Kogarah", line1.DL_ConsigneeCity);
				AssertEquals(SupplierBookingLine.Schema.DL_ConsigneePostCode, "2217", line1.DL_ConsigneePostCode);
				AssertEquals(SupplierBookingLine.Schema.DL_ConsigneeState, "NSW", line1.DL_ConsigneeState);
				AssertEquals(SupplierBookingLine.Schema.DL_RN_NKConsigneeCountryCode, "AU", line1.DL_RN_NKConsigneeCountryCode);
				AssertEquals(SupplierBookingLine.Schema.DL_ConsigneePhone, "6199999999", line1.DL_ConsigneePhone);
				AssertEquals(SupplierBookingLine.Schema.DL_ConsigneeEmail, ZString.Empty, line1.DL_ConsigneeEmail);

				var line2 = lines[1];
				AssertEquals("line2 DL_SignatureRequired", false, line2.DL_SignatureRequired);
				AssertEquals("line2 DL_F3_NKPackType", "UNT", line2.DL_F3_NKPackType);
				AssertEquals("line2 DL_PiecesManifested", 2, line2.DL_PiecesManifested);
				AssertEquals("line2 DL_VendorIdentifier", "VENDORSHMENDOR", line2.DL_VendorIdentifier);

				var line3 = lines[2];
				AssertEquals("line3 DL_SignatureRequired", false, line3.DL_SignatureRequired);
				AssertEquals("line3 DL_F3_NKPackType", string.Empty, line3.DL_F3_NKPackType);
				AssertEquals("line3 DL_PiecesManifested", 1, line3.DL_PiecesManifested);
			});
		}

		public void TestUpdateUniversalShipmentByDataContextKey()
		{
			TestCaseHelper.ClearTable(JobShipmentSchema.Constants.TableName);
			TestCaseHelper.ClearTable(SupplierBookingHeaderSchema.Constants.TableName);
			TestCaseHelper.ClearTable(SupplierBookingLineSchema.Constants.TableName);

			var mainAddress = SetupSampleConsignor(Factory.BOFactory);
			var dispatchAddress = SetupSampleDispatchAddress(Factory.BOFactory);
			Factory.SaveForTesting();

			string universalXml;
			using (var stream = new StreamReader(TestFiles.GetPathFor("eManifestImport.xml")))
			{
				universalXml = stream.ReadToEnd();
			}

			string universalXmlToUpdate;
			using (var stream = new StreamReader(TestFiles.GetPathFor("eManifestUpdateByKey.xml")))
			{
				universalXmlToUpdate = stream.ReadToEnd();
			}

			var message = DataTransferTestHelper.GetQueuedUniversalDataMessage(Factory, universalXml, EDIMessageSubTypeList.Codes.XmlUniversalShipment, true);
			var messageToUpdate = DataTransferTestHelper.GetQueuedUniversalDataMessage(Factory, universalXmlToUpdate, EDIMessageSubTypeList.Codes.XmlUniversalShipment, true);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertMultilineASCIIEquals("PREREQUISITE: Service Task Log", @"Added Supplier Booking Line Z00003(Consignee='Joe Jones') from UniversalShipment.
Added Supplier Booking Line Z00005(Consignee='John Smith') from UniversalShipment.
Added Supplier Booking Line Z00012(Consignee='John Smith') from UniversalShipment.
Added Supplier Booking (Supplier='CARGOWSYD') from UniversalShipment.
Successfully saved Supplier Booking M00000001 (Supplier='CARGOWSYD') with 3 x SupplierBookingLine.".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("PREREQUISITE: Message Log", @"
No matching SupplierBookingHeader found, creating new SupplierBookingHeader.
Populating SupplierBookingHeader...
Matching 'ConsignorDocumentaryAddress':- Matched to 'CARGOWSYD' by code, address 'P.O. BOX 6094' (only address).
Matching 'PickUpAddress':- Matched to 'SERVINSYD' by code, address 'Suite 81A/8-24 Kippax Str' (only address).
No matching SupplierBookingLine found, creating new SupplierBookingLine.
Populating SupplierBookingLine...
Added Supplier Booking Line Z00003(Consignee='Joe Jones') from UniversalShipment.
No matching SupplierBookingLine found, creating new SupplierBookingLine.
Populating SupplierBookingLine...
Added Supplier Booking Line Z00005(Consignee='John Smith') from UniversalShipment.
No matching SupplierBookingLine found, creating new SupplierBookingLine.
Populating SupplierBookingLine...
Added Supplier Booking Line Z00012(Consignee='John Smith') from UniversalShipment.
Added Supplier Booking (Supplier='CARGOWSYD') from UniversalShipment.
Successfully saved Supplier Booking M00000001 (Supplier='CARGOWSYD') with 3 x SupplierBookingLine.".Trim(), message.GetLogNoteText());

				var bookingHeaders = Factory.Load<SupplierBookingHeader>(new ZQuery());
				AssertEquals("PREREQUISITE: SupplierBookingHeader", 1, bookingHeaders.Length);

				var bookingLines = Factory.Load<SupplierBookingLine>(new ZQuery());
				AssertEquals("PREREQUISITE: SupplierBookingLine", 3, bookingLines.Length);

				AssertEquals("PREREQUISITE: DH_GrossWeightInKg", new ZDecimal(4545), bookingHeaders[0].DH_GrossWeightInKg);

				var lines = bookingLines.OrderBy(l => l.DL_ConsigneeReference).ToArray();
				var line1 = lines[0];
				AssertEquals("PREREQUISITE: DL_GrossWeight", new ZDecimal(33451.040), line1.DL_GrossWeight);

				AssertEquals("PREREQUISITE: DL_ConsignorName", "Ebooks USA", line1.DL_ConsignorName);

				AssertEquals("PREREQUISITE: DL_ConsigneeAddress1", "1 Long Rd", line1.DL_ConsigneeAddress1);

				line1.DL_Status = "CNF";
				Factory.SaveForTesting();
			});

			var serviceTaskLogToUpdate = new ServiceTaskLogForTesting();
			var managerToUpdate = new UniversalMessageProcessingManager(serviceTaskLogToUpdate);
			managerToUpdate.Process(messageToUpdate);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"Updated Supplier Booking Line Z00003(Consignee='Joe Jones') from UniversalShipment.
Updated Supplier Booking Line Z00005(Consignee='John Smith') from UniversalShipment.
Updated Supplier Booking Line Z00012(Consignee='John Smith') from UniversalShipment.
Updated Supplier Booking M00000001 (Supplier='CARGOWSYD') from UniversalShipment.
Successfully saved Supplier Booking M00000001 (Supplier='CARGOWSYD') with 3 x SupplierBookingLine.".Trim(), serviceTaskLogToUpdate.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"Successfully loaded matching SupplierBookingHeader.
Populating SupplierBookingHeader...
Matching 'ConsignorDocumentaryAddress':- Matched to 'CARGOWSYD' by code, address 'P.O. BOX 6094' (only address).
Warning - Matching 'PickUpAddress':- No match found for '[Org. Code: FOOORGSYD]'.
Successfully loaded matching SupplierBookingLine.
Populating SupplierBookingLine...
Updated Supplier Booking Line Z00003(Consignee='Joe Jones') from UniversalShipment.
Successfully loaded matching SupplierBookingLine.
Populating SupplierBookingLine...
Updated Supplier Booking Line Z00005(Consignee='John Smith') from UniversalShipment.
Successfully loaded matching SupplierBookingLine.
Populating SupplierBookingLine...
Updated Supplier Booking Line Z00012(Consignee='John Smith') from UniversalShipment.
Updated Supplier Booking M00000001 (Supplier='CARGOWSYD') from UniversalShipment.
Successfully saved Supplier Booking M00000001 (Supplier='CARGOWSYD') with 3 x SupplierBookingLine.".Trim(), messageToUpdate.GetLogNoteText());

				var newFactory = new BusinessObjectFactory();

				var bookingHeaders = newFactory.Load<SupplierBookingHeader>(new ZQuery());
				AssertEquals("SupplierBookingHeader not created", 1, bookingHeaders.Length);
				AssertEquals(SupplierBookingHeader.Schema.DH_GrossWeightInKg + " should be updated", new ZDecimal(9999), bookingHeaders[0].DH_GrossWeightInKg);

				var bookingLines = newFactory.Load<SupplierBookingLine>(new ZQuery());
				AssertEquals("SupplierBookingLine not created", 3, bookingLines.Length);
				var lines = bookingLines.OrderBy(l => l.DL_ConsigneeReference).ToArray();
				var line1 = lines[0];
				AssertEquals(SupplierBookingLine.Schema.DL_GrossWeight + " should be updated", new ZDecimal(8888), line1.DL_GrossWeight);

				var addresses = newFactory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_ParentID, line1.PK));
				AssertEquals("addresses should not be added", 0, addresses.Length);

				AssertEquals("DL_ConsignorName should be updated", "Ebooks USA 2", line1.DL_ConsignorName);

				AssertEquals(SupplierBookingLine.Schema.DL_ConsigneeAddress1, "2 Long Road", line1.DL_ConsigneeAddress1);
				AssertEquals(SupplierBookingLine.Schema.DL_Status + " should not be updated", "CNF", line1.DL_Status);
			});
		}

		public void TestImportUniversalShipment_SupplierNotExistsException()
		{
			string universalXml = File.ReadAllText(TestFiles.GetPathFor("eManifestImport.xml"));
			var message = DataTransferTestHelper.GetQueuedUniversalDataMessage(Factory, universalXml, EDIMessageSubTypeList.Codes.XmlUniversalShipment, true);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"ERROR - Cannot import Supplier Booking Header
Unable to match Consignor Address, please make sure the supplied Consignor Address is valid. Details were:
Address1: P.O. BOX 6094
AddressShortCode: ARM: P.O. BOX 6094
City: ALEXANDRIA
CompanyName: CARGOWISE EDI PTY LTD
Country: AU - Australia
Email: brendon.paine@cargowise.com
Fax: +61 (2) 9025-1122
GovRegNum: 41065894724
GovRegNumType: ABN - Australian Business Number (GST Reg
OrganizationCode: CARGOWSYD
Phone: +61 (2) 8001-2222
Port: AUSYD - Sydney
Postcode: 2015
State: NSW
RegistrationNumber 1:
CountryOfIssue: AU - Australia
Type: 1ST - 1-Stop Trading Code
Value: CWSYD
RegistrationNumber 2:
CountryOfIssue: AU - Australia
Type: CCC - Customs Carrier Code
Value: C001191402
RegistrationNumber 3:
CountryOfIssue: GB - United Kingdom
Type: DAN - Deferment Approval Number
Value: dantheman
RegistrationNumber 4:
CountryOfIssue: US - United States
Type: DUN - Data Universal Numbering System
Value: 404586593
RegistrationNumber 5:
CountryOfIssue: US - United States
Type: EIN - Employer Identification Number
Value: 91-013199000
RegistrationNumber 6:
CountryOfIssue: AU - Australia
Type: CAC - Credit Agency Code
Value: 1
RegistrationNumber 7:
CountryOfIssue: US - United States
Type: CCC - Standard Carrier Alpha Code
Value: XRXE
RegistrationNumber 8:
CountryOfIssue: AU - Australia
Type: CCD - Customs Client Code
Value: 0587649B
RegistrationNumber 9:
CountryOfIssue: AU - Australia
Type: CMP - Customs Manifest Provider Code
Value: C001191402
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
No matching SupplierBookingHeader found, creating new SupplierBookingHeader.
Populating SupplierBookingHeader...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Org. Code: CARGOWSYD; Company Name: CARGOWISE EDI PTY LTD; Address Code: ARM: P.O. BOX 6094; Address 1: P.O. BOX 6094; City: ALEXANDRIA]'.
Error - Cannot import Supplier Booking Header
Unable to match Consignor Address, please make sure the supplied Consignor Address is valid. Details were:
Address1: P.O. BOX 6094
AddressShortCode: ARM: P.O. BOX 6094
City: ALEXANDRIA
CompanyName: CARGOWISE EDI PTY LTD
Country: AU - Australia
Email: brendon.paine@cargowise.com
Fax: +61 (2) 9025-1122
GovRegNum: 41065894724
GovRegNumType: ABN - Australian Business Number (GST Reg
OrganizationCode: CARGOWSYD
Phone: +61 (2) 8001-2222
Port: AUSYD - Sydney
Postcode: 2015
State: NSW
RegistrationNumber 1:
CountryOfIssue: AU - Australia
Type: 1ST - 1-Stop Trading Code
Value: CWSYD
RegistrationNumber 2:
CountryOfIssue: AU - Australia
Type: CCC - Customs Carrier Code
Value: C001191402
RegistrationNumber 3:
CountryOfIssue: GB - United Kingdom
Type: DAN - Deferment Approval Number
Value: dantheman
RegistrationNumber 4:
CountryOfIssue: US - United States
Type: DUN - Data Universal Numbering System
Value: 404586593
RegistrationNumber 5:
CountryOfIssue: US - United States
Type: EIN - Employer Identification Number
Value: 91-013199000
RegistrationNumber 6:
CountryOfIssue: AU - Australia
Type: CAC - Credit Agency Code
Value: 1
RegistrationNumber 7:
CountryOfIssue: US - United States
Type: CCC - Standard Carrier Alpha Code
Value: XRXE
RegistrationNumber 8:
CountryOfIssue: AU - Australia
Type: CCD - Customs Client Code
Value: 0587649B
RegistrationNumber 9:
CountryOfIssue: AU - Australia
Type: CMP - Customs Manifest Provider Code
Value: C001191402
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestImportUniversalShipment_NoExceptionShouldBeThrownIfNoWayBill()
		{
			TestCaseHelper.ClearTable(JobShipmentSchema.Constants.TableName);
			TestCaseHelper.ClearTable(SupplierBookingHeaderSchema.Constants.TableName);
			TestCaseHelper.ClearTable(SupplierBookingLineSchema.Constants.TableName);

			var mainAddress = SetupSampleConsignor(Factory.BOFactory);
			Factory.SaveForTesting();

			string universalXml = File.ReadAllText(TestFiles.GetPathFor("eManifestImport_NoWayBill.xml"));

			var message = DataTransferTestHelper.GetQueuedUniversalDataMessage(Factory, universalXml, EDIMessageSubTypeList.Codes.XmlUniversalShipment, true);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			AssertNoExceptionThrown(() => manager.Process(message));
		}

		public void TestImportUniversalShipment_NoExceptionShouldBeThrownIfNoCurrency()
		{
			TestCaseHelper.ClearTable(JobShipmentSchema.Constants.TableName);
			TestCaseHelper.ClearTable(SupplierBookingHeaderSchema.Constants.TableName);
			TestCaseHelper.ClearTable(SupplierBookingLineSchema.Constants.TableName);

			var mainAddress = SetupSampleConsignor(Factory.BOFactory);
			Factory.SaveForTesting();

			string universalXml = File.ReadAllText(TestFiles.GetPathFor("eManifestImport_NoCurrency.xml"));

			var message = DataTransferTestHelper.GetQueuedUniversalDataMessage(Factory, universalXml, EDIMessageSubTypeList.Codes.XmlUniversalShipment, true);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			AssertNoExceptionThrown(() => manager.Process(message));

			CombineAssertions(delegate
			{
				var bookingHeaders = Factory.Load<SupplierBookingHeader>(new ZQuery());
				AssertEquals("SupplierBookingHeader", 1, bookingHeaders.Length);

				var bookingLines = Factory.Load<SupplierBookingLine>(new ZQuery());
				AssertEquals("SupplierBookingLine", 1, bookingLines.Length);

				AssertEquals(SupplierBookingLine.Schema.DL_RX_NKGoodsValueCurrency, string.Empty, bookingLines[0].DL_RX_NKGoodsValueCurrency);
			});
		}

		public void TestImportUniversalShipmentWithDepotCarrierServiceLevel()
		{
			#region Setup Data

			TestCaseHelper.ClearTable(JobShipmentSchema.Constants.TableName);
			TestCaseHelper.ClearTable(SupplierBookingHeaderSchema.Constants.TableName);
			TestCaseHelper.ClearTable(SupplierBookingLineSchema.Constants.TableName);

			var mainAddress = SetupSampleConsignor(Factory.BOFactory);

			var orgDepot = Factory.New<OrgHeader>();
			orgDepot.OH_Code = "DEPOT";
			orgDepot.OH_FullName = "Destination Depot Org";
			orgDepot.OH_IsShippingProvider = false;

			var orgCarrier1 = Factory.New<OrgHeader>();
			orgCarrier1.OH_Code = "CARRIER1";
			orgCarrier1.OH_FullName = "Carrier 1 Org";
			orgCarrier1.OH_IsShippingProvider = true;

			var orgCarrier2 = Factory.New<OrgHeader>();
			orgCarrier2.OH_Code = "CARRIER2";
			orgCarrier2.OH_FullName = "Carrier 2 Org";
			orgCarrier2.OH_IsShippingProvider = true;

			var addressDepot = Factory.New<OrgAddress>();
			addressDepot.OA_OH = orgDepot.PK;
			addressDepot.OA_Code = "DEP";
			addressDepot.OA_Address1 = "Destination Depot Org Address";

			var addressCarrier1 = Factory.New<OrgAddress>();
			addressCarrier1.OA_OH = orgCarrier1.PK;
			addressCarrier1.OA_Code = "CR1";
			addressCarrier1.OA_Address1 = "Carrier 1 Org Address";

			var addressCarrier2 = Factory.New<OrgAddress>();
			addressCarrier2.OA_OH = orgCarrier2.PK;
			addressCarrier2.OA_Code = "CR2";
			addressCarrier2.OA_Address1 = "Carrier 2 Org Address";

			var miscCarrier1 = Factory.LoadTop1<OrgMiscServ>(new ZQuery(OrgMiscServSchema.OM_OH, orgCarrier1.PK));
			var miscCarrier2 = Factory.LoadTop1<OrgMiscServ>(new ZQuery(OrgMiscServSchema.OM_OH, orgCarrier2.PK));

			var serviceLevelMiscCarrier11 = Factory.New<OrgCarrierServiceLevel>();
			serviceLevelMiscCarrier11.PL_Code = "D2D";
			serviceLevelMiscCarrier11.PL_CarrierServiceLevelDescription = "Door To Door";
			serviceLevelMiscCarrier11.PL_OM = miscCarrier1.PK;

			var serviceLevelMiscCarrier12 = Factory.New<OrgCarrierServiceLevel>();
			serviceLevelMiscCarrier12.PL_Code = "TSP";
			serviceLevelMiscCarrier12.PL_CarrierServiceLevelDescription = "Transhipment";
			serviceLevelMiscCarrier12.PL_OM = miscCarrier1.PK;

			var serviceLevelMiscCarrier2 = Factory.New<OrgCarrierServiceLevel>();
			serviceLevelMiscCarrier2.PL_Code = "DEF";
			serviceLevelMiscCarrier2.PL_CarrierServiceLevelDescription = "Deferred";
			serviceLevelMiscCarrier2.PL_OM = miscCarrier2.PK;

			var refPostCode1 = Factory.New<RefPostCode>();
			refPostCode1.RK_CityTownPostCode = "3500";
			refPostCode1.RK_RN_NKCountry = "AU";
			refPostCode1.RK_Lattitude = 101.2;
			refPostCode1.RK_Longitude = 20.5;

			var refPostCode2 = Factory.New<RefPostCode>();
			refPostCode2.RK_CityTownPostCode = "3600";
			refPostCode2.RK_RN_NKCountry = "AU";
			refPostCode2.RK_Lattitude = 121.2;
			refPostCode2.RK_Longitude = 25.5;

			Factory.SaveForTesting();

			var query1 = new ZQuery(RefCityTownSchema.R9_RN_NKCountry, "AU");
			query1.AddToFilter(new ZQuery(RefCityTownSchema.R9_RW_NKState, "SA"));
			query1.AddToFilter(new ZQuery(RefCityTownSchema.R9_InternationalName, "Adelaide"));
			var reCityTown1 = Factory.BOFactory.LoadTop1<RefCityTown>(query1);

			var query2 = new ZQuery(RefCityTownSchema.R9_RN_NKCountry, "AU");
			query2.AddToFilter(new ZQuery(RefCityTownSchema.R9_RW_NKState, "NSW"));
			query2.AddToFilter(new ZQuery(RefCityTownSchema.R9_InternationalName, "Sydney"));
			var reCityTown2 = Factory.BOFactory.LoadTop1<RefCityTown>(query2);

			TestConnection.ExecuteNonQuery(string.Format(@"INSERT INTO dbo.PortHubSelection (TY_PK, TY_Direction, TY_UndgClass, TY_RS_NKServiceLevel, TY_RatingFreightMode, TY_OA_DepotAddress) 
SELECT 'DDE7B656-1D62-491C-A543-CBB01FEA1D8C','DLV', 'ALL', 'DIR', 'ALL', '{0}' UNION ALL
SELECT '24988795-58B3-411F-844C-51EB00016B72','DLV', 'ALL', 'DIR', 'AIR', '{0}'", addressDepot.PK));

			TestConnection.ExecuteNonQuery(string.Format(@"INSERT INTO dbo.RateTransportProvider (TP_PK, TP_RN_NKCountry, TP_OH_RelatedParty, TP_SystemLastEditTimeUtc, TP_SystemLastEditUser, TP_SystemCreateTimeUtc, TP_SystemCreateUser)
SELECT '968FA263-04D6-4D77-B15D-3D26ED22E08F','US','{0}', GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT '377657A5-8111-4CB9-80AF-FC0D941BA553','US','{1}', GetUtcDate(), '~BP', GetUtcDate(), '~BP'", orgCarrier1.PK, orgCarrier2.PK));

			TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.RateTransportZones (TZ_PK, TZ_ZoneName, TZ_TP, TZ_SystemLastEditTimeUtc, TZ_SystemLastEditUser, TZ_SystemCreateTimeUtc, TZ_SystemCreateUser) 
SELECT '5F3A8CBF-65DA-44A3-9100-8CF91E983C0C','Melbourne Metro', '968FA263-04D6-4D77-B15D-3D26ED22E08F', GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT 'A7A8B7F4-F7B3-4411-98C4-FBB5DCF29A98','Adelaide Metro', '968FA263-04D6-4D77-B15D-3D26ED22E08F', GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT '62D0F80C-23A5-4896-A302-DF5759EFEDC1','Sydney Metro', '377657A5-8111-4CB9-80AF-FC0D941BA553', GetUtcDate(), '~BP', GetUtcDate(), '~BP'");

			TestConnection.ExecuteNonQuery(string.Format(@"INSERT INTO dbo.RateTransportZoneItem (TQ_PK, TQ_TZ_DomesticZone, TQ_R9_CityTown, TQ_FromPostCode, TQ_ToPostCode, TQ_RN_NKCountry, TQ_SystemLastEditTimeUtc, TQ_SystemLastEditUser, TQ_SystemCreateTimeUtc, TQ_SystemCreateUser) 
SELECT '29FC9164-FC58-4E6E-A6D3-783F6FB37123','5F3A8CBF-65DA-44A3-9100-8CF91E983C0C', NULL , '{0}', '{1}', 'AU', GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT '35FAD11E-9E83-4BA2-A1F6-6F3D56117F21', 'A7A8B7F4-F7B3-4411-98C4-FBB5DCF29A98', '{2}', '', '', 'AU', GetUtcDate(), '~BP', GetUtcDate(), '~BP' UNION ALL
SELECT 'AC0E302D-5046-4643-9646-D5E72917C639', '62D0F80C-23A5-4896-A302-DF5759EFEDC1', '{3}', '', '', 'AU', GetUtcDate(), '~BP', GetUtcDate(), '~BP'", refPostCode1.RK_CityTownPostCode, refPostCode2.RK_CityTownPostCode, reCityTown1.PK, reCityTown2.PK));

			TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.PortHubZonePivot (TX_PK, TX_TY_Hub, TX_TZ_Zone, TX_PL_NKCarrierServiceLevel)
SELECT '5CFB04C4-2F1C-47F8-9B20-BA96FECCA961', 'DDE7B656-1D62-491C-A543-CBB01FEA1D8C', '5F3A8CBF-65DA-44A3-9100-8CF91E983C0C', 'D2D' UNION ALL
SELECT 'E24BA2E5-3869-4703-9AB0-FEA08AF6772B', 'DDE7B656-1D62-491C-A543-CBB01FEA1D8C', 'A7A8B7F4-F7B3-4411-98C4-FBB5DCF29A98', 'TSP' UNION ALL
SELECT '68CBF4C2-A1C9-48D8-96B9-88E078383CFF', '24988795-58B3-411F-844C-51EB00016B72', '62D0F80C-23A5-4896-A302-DF5759EFEDC1', 'DEF'");

			Factory.SaveForTesting();

			#endregion

			string universalXml = File.ReadAllText(TestFiles.GetPathFor("eManifestImportDepotCarrierServiceLevel.xml"));

			var message = DataTransferTestHelper.GetQueuedUniversalDataMessage(Factory, universalXml, EDIMessageSubTypeList.Codes.XmlUniversalShipment, true);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				var bookingHeaders = Factory.Load<SupplierBookingHeader>(new ZQuery());
				AssertEquals("SupplierBookingHeader", 1, bookingHeaders.Length);

				var bookingLines = Factory.Load<SupplierBookingLine>(new ZQuery());
				AssertEquals("SupplierBookingLine", 3, bookingLines.Length);

				var header = bookingHeaders[0];
				AssertEquals(SupplierBookingHeader.Schema.DH_CubicInM3, new ZDecimal(6676.000), header.DH_CubicInM3);
				AssertEquals(SupplierBookingHeader.Schema.DH_GrossWeightInKg, new ZDecimal(4545.000), header.DH_GrossWeightInKg);
				AssertEquals(SupplierBookingHeader.Schema.DH_IsShipperApproved, false, header.DH_IsShipperApproved);
				AssertEquals(SupplierBookingHeader.Schema.DH_OA_Consignor, mainAddress.PK, header.DH_OA_Consignor);
				AssertEquals(SupplierBookingHeader.Schema.DH_OA_DispatchAddress, Guid.Empty, header.DH_OA_DispatchAddress);
				AssertEquals(SupplierBookingHeader.Schema.DH_OA_OriginDepot, Guid.Empty, header.DH_OA_OriginDepot);
				AssertEquals(SupplierBookingHeader.Schema.DH_OC_BookedBy, Guid.Empty, header.DH_OC_BookedBy);
				AssertEquals(SupplierBookingHeader.Schema.DH_OC_ConsignorContact, Guid.Empty, header.DH_OC_ConsignorContact);
				AssertEquals(SupplierBookingHeader.Schema.DH_OH_ConsignmentBroker, Guid.Empty, header.DH_OH_ConsignmentBroker);
				AssertEquals(SupplierBookingHeader.Schema.DH_PiecesManifested, 3213, header.DH_PiecesManifested);
				AssertEquals(SupplierBookingHeader.Schema.DH_SupplierReference, "M00000001", header.DH_SupplierReference);

				var lines = bookingLines.OrderBy(l => l.DL_ConsigneeReference).ToArray();

				var line1 = lines[0];
				AssertEquals(SupplierBookingLine.Schema.DL_DH_BookingHeader, header.PK, line1.DL_DH_BookingHeader);
				AssertEquals(SupplierBookingLine.Schema.DL_ConsigneeReference, "Z00003", line1.DL_ConsigneeReference);
				AssertEquals(SupplierBookingLine.Schema.DL_GoodsValue, new ZDecimal(1043.0330), line1.DL_GoodsValue);
				AssertEquals(SupplierBookingLine.Schema.DL_RX_NKGoodsValueCurrency, "AUD", line1.DL_RX_NKGoodsValueCurrency);
				AssertEquals(SupplierBookingLine.Schema.DL_GrossWeight, new ZDecimal(33451.040), line1.DL_GrossWeight);
				AssertEquals(SupplierBookingLine.Schema.DL_GrossWeightUQ, "KG", line1.DL_GrossWeightUQ);
				AssertEquals(SupplierBookingLine.Schema.DL_Cubic, new ZDecimal(3220.044), line1.DL_Cubic);
				AssertEquals(SupplierBookingLine.Schema.DL_CubicUQ, "M3", line1.DL_CubicUQ);
				AssertEquals(SupplierBookingLine.Schema.DL_GoodsDescription, "Balalayka", line1.DL_GoodsDescription);
				AssertEquals(SupplierBookingLine.Schema.DL_MarksAndNumbers, "This is really long description", line1.DL_MarksAndNumbers);
				AssertEquals(SupplierBookingLine.Schema.DL_PiecesManifested, 888, line1.DL_PiecesManifested);
				AssertEquals(SupplierBookingLine.Schema.DL_JS_ApprovedShipment, Guid.Empty, line1.DL_JS_ApprovedShipment);
				AssertEquals(SupplierBookingLine.Schema.DL_DO_LoadList, Guid.Empty, line1.DL_DO_LoadList);
				AssertEquals(SupplierBookingLine.Schema.DL_OrderTrackingNumber, "Reference_3323", line1.DL_OrderTrackingNumber);
				AssertEquals(SupplierBookingLine.Schema.DL_RS_NKServiceLevel, "DIR", line1.DL_RS_NKServiceLevel);

				AssertEquals(SupplierBookingLine.Schema.DL_DeliveryBarcode, "", line1.DL_DeliveryBarcode);
				AssertEquals(SupplierBookingLine.Schema.DL_Status, Constants.SupplierBookingLineStatus.Codes.Incomplete, line1.DL_Status);
				AssertEquals(SupplierBookingLine.Schema.DL_Index, 1, line1.DL_Index);
				AssertEquals(SupplierBookingLine.Schema.DL_KM_LastMileTransportBooking, ZGuid.Empty, line1.DL_KM_LastMileTransportBooking);

				AssertEquals(SupplierBookingLine.Schema.DL_KM_LastMileTransportBooking, ZGuid.Empty, line1.DL_KM_LastMileTransportBooking);

				var addresses = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_ParentID, line1.PK));
				AssertEquals("There should be no JobDocAddresses linked to the line", 0, addresses.Length);

				AssertEquals(SupplierBookingLine.Schema.DL_ConsignorContact, ZString.Empty, line1.DL_ConsignorContact);
				AssertEquals(SupplierBookingLine.Schema.DL_ConsignorName, "Ebooks USA", line1.DL_ConsignorName);
				AssertEquals(SupplierBookingLine.Schema.DL_ConsignorAddress1, "1256 Santa Ana Blv", line1.DL_ConsignorAddress1);
				AssertEquals(SupplierBookingLine.Schema.DL_ConsignorCity, "Highland Park", line1.DL_ConsignorCity);
				AssertEquals(SupplierBookingLine.Schema.DL_ConsignorPostCode, "90042", line1.DL_ConsignorPostCode);
				AssertEquals(SupplierBookingLine.Schema.DL_ConsignorState, "CA", line1.DL_ConsignorState);
				AssertEquals(SupplierBookingLine.Schema.DL_RN_NKConsignorCountryCode, ZString.Empty, line1.DL_RN_NKConsignorCountryCode);
				AssertEquals(SupplierBookingLine.Schema.DL_ConsignorPhone, ZString.Empty, line1.DL_ConsignorPhone);
				AssertEquals(SupplierBookingLine.Schema.DL_ConsignorEmail, ZString.Empty, line1.DL_ConsignorEmail);

				AssertEquals(SupplierBookingLine.Schema.DL_ConsigneeContact, "Joe Jones", line1.DL_ConsigneeContact);
				AssertEquals(SupplierBookingLine.Schema.DL_ConsigneeName, "Joe Jones", line1.DL_ConsigneeName);
				AssertEquals(SupplierBookingLine.Schema.DL_ConsigneeAddress1, "Test Address 11", line1.DL_ConsigneeAddress1);
				AssertEquals(SupplierBookingLine.Schema.DL_ConsigneeCity, "Melbourne", line1.DL_ConsigneeCity);
				AssertEquals(SupplierBookingLine.Schema.DL_ConsigneePostCode, "3560", line1.DL_ConsigneePostCode);
				AssertEquals(SupplierBookingLine.Schema.DL_ConsigneeState, "VIC", line1.DL_ConsigneeState);
				AssertEquals(SupplierBookingLine.Schema.DL_RN_NKConsigneeCountryCode, "AU", line1.DL_RN_NKConsigneeCountryCode);
				AssertEquals(SupplierBookingLine.Schema.DL_ConsigneePhone, "6199999999", line1.DL_ConsigneePhone);
				AssertEquals(SupplierBookingLine.Schema.DL_ConsigneeEmail, ZString.Empty, line1.DL_ConsigneeEmail);

				AssertEquals(SupplierBookingLine.Schema.DL_OA_DestinationDepot, addressDepot.PK, line1.DL_OA_DestinationDepot);
				AssertEquals(SupplierBookingLine.Schema.DL_OH_LastMileCarrier, orgCarrier1.PK, line1.DL_OH_LastMileCarrier);
				AssertEquals(SupplierBookingLine.Schema.DL_PL_NKCarrierServiceLevel, "D2D", line1.DL_PL_NKCarrierServiceLevel);
			});
		}

		public void TestSupplierBookingLineReader_ImportConsignorDocumentaryAddress()
		{
			var boFactory = new BusinessObjectFactory();
			var bookingHeader = boFactory.NewWithValidTestData<SupplierBookingHeader>();
			var consignor = boFactory.NewWithValidTestData<OrgHeader>();
			var consignorAddress = consignor.MainAddress;
			consignorAddress.OA_City = "SHANGHAI";
			consignor.OH_IsConsignor = true;
			bookingHeader.DH_OA_Consignor = consignor.MainAddress.PK;

			boFactory.Save();

			var eManifestLine = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var logger = new DummyLogger();

			var lineReader = new SupplierBookingLineReader(eManifestLine, logger, Factory, bookingHeader, 0);
			var outLine1 = lineReader.ReadIntoBusinessObject();
			AssertEquals("Consignor Address not specified on Line, use the consignor address from header", "SHANGHAI", outLine1.DL_ConsignorCity);

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress),
				City = "XUZHOU"
			};

			eManifestLine.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { orgAddress });
			var outLine2 = lineReader.ReadIntoBusinessObject();
			AssertEquals("AddressOverride is true", "XUZHOU", outLine2.DL_ConsignorCity);
		}

		#region Implementation

		static OrgAddress SetupSampleConsignor(BusinessObjectFactory factory)
		{
			var consignorOrganization = factory.NewWithValidTestData<OrgHeader>();
			consignorOrganization.OH_FullName = "CARGOWISE EDI PTY LTD";
			consignorOrganization.OH_Code = "CARGOWSYD";
			consignorOrganization.OH_IsConsignor = true;

			var mainAddress = consignorOrganization.MainAddress;
			mainAddress.OA_Address1 = "P.O. BOX 6094";
			mainAddress.OA_City = "ALEXANDRIA";

			return mainAddress;
		}

		static OrgAddress SetupSampleDispatchAddress(BusinessObjectFactory factory)
		{
			var dispatchOrganization = factory.NewWithValidTestData<OrgHeader>();
			dispatchOrganization.OH_FullName = "Servinc Australia Pty Ltd";
			dispatchOrganization.OH_Code = "SERVINSYD";
			dispatchOrganization.OH_IsConsignor = true;

			var mainAddress = dispatchOrganization.MainAddress;
			mainAddress.OA_Address1 = "Suite 81A/8-24 Kippax Street";
			mainAddress.OA_City = "Surry Hills";

			return mainAddress;
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes
		{
			get { return Array.Empty<RecipientRoleType>(); }
		}

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get
			{
				SetupSampleConsignor(Factory.BOFactory);
				Factory.SaveForTesting();
				return File.ReadAllText(TestFiles.GetPathFor("eManifestImport.xml"));
			}
		}

		#endregion
	}
}
