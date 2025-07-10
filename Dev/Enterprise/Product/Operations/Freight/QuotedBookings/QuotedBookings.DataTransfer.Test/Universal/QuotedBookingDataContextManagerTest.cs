using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.Registry.Business.WorkflowManager;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.UniversalDataBuss.Matching.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static CargoWise.EventReference.Constants;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal.Test
{
	[TestedType(typeof(QuotedBookingDataContextManager))]
	sealed class QuotedBookingDataContextManagerTest : ShipmentDataContextManagerTestCase<QuotedBookingDataContextManager, QuotedBooking>
	{
		#region Reference and Party ID Matching

		public void TestReferenceAndPartyIDMatchingOnHouseBill()
		{
			eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataContext = shipment.DataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.ForwardingBooking, null);

			shipment.WayBillNumber = "HBL05387944";
			shipment.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House };

			shipment.GoodsDescription = "RAGING RABID ROGER RABBIT";

			var orgGenerator = new OrganisationTestHelper(Factory);

			var consignorDataObject = orgGenerator.CreateDataObject("JACK", "SCHMIDT", "7643");
			consignorDataObject.AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress);

			var consigneeDataObject = orgGenerator.CreateDataObject("PHIL", "MCKRACKIN", "2153");
			consigneeDataObject.AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress);

			var sendingForwarderDataObject = orgGenerator.CreateDataObject("SILVERWATER", "CHUTE", "4792");
			sendingForwarderDataObject.AddressType = nameof(DocAddressType.SendingForwarderAddress);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { consignorDataObject, consigneeDataObject, sendingForwarderDataObject });

			var consignorBO = orgGenerator.CreateBusinessObject("JACK", "SCHMIDT", "7643");
			var consigneeBO = orgGenerator.CreateBusinessObject("PHIL", "MCKRACKIN", "2153");
			var sendingForwarderBO = orgGenerator.CreateBusinessObject("SILVERWATER", "CHUTE", "4792");
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = sendingForwarderBO.PK;
			Factory.SaveForTesting();

			AssertEquals("Precondition: Should be no Shipments in the DB yet.", null, new BusinessObjectFactory().LoadTop1<ForwardingShipment>(new ZQuery()));

			CombineAssertions(delegate
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(shipment);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000).
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No Reference/Party ID matches were found in this module.
No matching QuotedBooking found, creating new QuotedBooking.
Populating QuotedBooking...
Matching 'ConsignorDocumentaryAddress':- Matched to 'JACSCHBNE' address '643 JACK BOULEVARDE' with a score of 560.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'PHIMCKSYD' address '153 PHIL BOULEVARDE' with a score of 560.
Matching 'SendingForwarderAddress':- Matched to 'SILCHUXRH' address '792 SILVERWATER BOULEVARD' with a score of 560.
Warning - Unknown Address Type [SendingForwarderAddress] found. Job Document Address not imported.
Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000).
".Trim(), logNoteText);

				var bookings = new BusinessObjectFactory().Load<ViewQuotedBooking>(new ZQuery());
				AssertEquals("bookings.Count()", 1, bookings.Length);
				var booking = bookings[0].QuotedBooking.Booking;
				AssertEquals("booking.JS_HouseBill", "HBL05387944", booking.JS_HouseBill);
				AssertEquals("booking.JS_GoodsDescription", "RAGING RABID ROGER RABBIT", booking.JS_GoodsDescription);
			});

			shipment.GoodsDescription = "TASTES LIKE CHICKEN";

			CombineAssertions(delegate
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(shipment);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Updated Quick Booking - Booking (S00001000) from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000).
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Matched to Quick Booking - Booking (S00001000) with a Reference/Party ID match score of 180.
Successfully loaded matching QuotedBooking.
Populating QuotedBooking...
Matching 'ConsignorDocumentaryAddress':- Matched to 'JACSCHBNE' address '643 JACK BOULEVARDE' with a score of 560.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'PHIMCKSYD' address '153 PHIL BOULEVARDE' with a score of 560.
Matching 'SendingForwarderAddress':- Matched to 'SILCHUXRH' address '792 SILVERWATER BOULEVARD' with a score of 560.
Warning - Unknown Address Type [SendingForwarderAddress] found. Job Document Address not imported.
Updated Quick Booking - Booking (S00001000) from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000).
".Trim(), logNoteText);

				var bookings = new BusinessObjectFactory().Load<ViewQuotedBooking>(new ZQuery());
				AssertEquals("bookings.Count()", 1, bookings.Length);
				var booking = bookings[0].QuotedBooking.Booking;
				AssertEquals("booking.JS_HouseBill", "HBL05387944", booking.JS_HouseBill);
				AssertEquals("booking.JS_GoodsDescription", "TASTES LIKE CHICKEN", booking.JS_GoodsDescription);
			});
		}

		public void TestReferenceAndPartyIDMatchOnBookingReferenceWithoutMatchingConsignorOrConsigneeFails()
		{
			eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataContext = shipment.DataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.ForwardingBooking, null);

			shipment.BookingConfirmationReference = "BCR58094289";
			shipment.GoodsDescription = "BUCKET OF MUD";

			var orgGenerator = new OrganisationTestHelper(Factory);

			var consignorDataObject = orgGenerator.CreateDataObject("JACK", "SCHMIDT", "7643");
			consignorDataObject.AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress);

			var consigneeDataObject = orgGenerator.CreateDataObject("PHIL", "MCKRACKIN", "2153");
			consigneeDataObject.AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress);

			var cfsDataObject = orgGenerator.CreateDataObject("CONTAINER", "JACK", "4739");
			cfsDataObject.AddressType = nameof(DocAddressType.DepartureCFSAddress);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { consignorDataObject, consigneeDataObject, cfsDataObject });

			var consignorBO = orgGenerator.CreateBusinessObject("JACK", "SCHMIDT", "7643");
			var consigneeBO = orgGenerator.CreateBusinessObject("PHIL", "MCKRACKIN", "2153");
			var cfsBO = orgGenerator.CreateBusinessObject("CONTAINER", "JACK", "4739");
			Factory.SaveForTesting();

			AssertEquals("Precondition: Should be no Quoted Bookings in the DB yet.", null, new BusinessObjectFactory().LoadTop1<ViewQuotedBooking>(new ZQuery()));

			CombineAssertions(delegate
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(shipment);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000).
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No Reference/Party ID matches were found in this module.
No matching QuotedBooking found, creating new QuotedBooking.
Populating QuotedBooking...
Matching 'ConsignorDocumentaryAddress':- Matched to 'JACSCHBNE' address '643 JACK BOULEVARDE' with a score of 560.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'PHIMCKSYD' address '153 PHIL BOULEVARDE' with a score of 560.
Matching 'DepartureCFSAddress':- Matched to 'CONJACXRH' address '739 CONTAINER BOULEVARDE' with a score of 560.
Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000).
".Trim(), logNoteText);

				var bookings = new BusinessObjectFactory().Load<ViewQuotedBooking>(new ZQuery());
				AssertEquals("bookings.Count()", 1, bookings.Length);
				var booking = bookings[0].QuotedBooking.Booking;
				AssertEquals("booking.JS_BookingReference", "BCR58094289", booking.JS_BookingReference);
				AssertEquals("booking.JS_GoodsDescription", "BUCKET OF MUD", booking.JS_GoodsDescription);
			});

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { cfsDataObject });

			CombineAssertions(delegate
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(shipment);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001001).
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No Reference/Party ID matches were found in this module.
No matching QuotedBooking found, creating new QuotedBooking.
Populating QuotedBooking...
Matching 'DepartureCFSAddress':- Matched to 'CONJACXRH' address '739 CONTAINER BOULEVARDE' with a score of 560.
Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001001).
".Trim(), logNoteText);

				var bookings = new BusinessObjectFactory().Load<ViewQuotedBooking>(new ZQuery());
				AssertEquals("bookings.Count()", 2, bookings.Length);
			});
		}

		public void TestReferenceAndPartyIDMatchingWithOrganisationCodesOnly()
		{
			eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataContext = shipment.DataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.ForwardingBooking, null);

			shipment.BookingConfirmationReference = "C2015442375";
			shipment.GoodsDescription = "BUCKET OF MUD";

			var orgGenerator = new OrganisationTestHelper(Factory);

			var consignorDataObject = orgGenerator.CreateDataObject("JACK", "SCHMIDT", "7643");
			consignorDataObject.AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress);

			var consigneeDataObject = orgGenerator.CreateDataObject("PHIL", "MCKRACKIN", "2153");
			consigneeDataObject.AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { consignorDataObject, consigneeDataObject });

			var consignorBO = orgGenerator.CreateBusinessObject("JACK", "SCHMIDT", "7643");
			var consigneeBO = orgGenerator.CreateBusinessObject("PHIL", "MCKRACKIN", "2153");
			Factory.SaveForTesting();

			AssertEquals("Precondition: Should be no Quoted Bookings in the DB yet.", null, new BusinessObjectFactory().LoadTop1<ViewQuotedBooking>(new ZQuery()));

			CombineAssertions(delegate
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(shipment);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000).
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No Reference/Party ID matches were found in this module.
No matching QuotedBooking found, creating new QuotedBooking.
Populating QuotedBooking...
Matching 'ConsignorDocumentaryAddress':- Matched to 'JACSCHBNE' address '643 JACK BOULEVARDE' with a score of 560.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'PHIMCKSYD' address '153 PHIL BOULEVARDE' with a score of 560.
Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000).
".Trim(), logNoteText);

				var bookings = new BusinessObjectFactory().Load<ViewQuotedBooking>(new ZQuery());
				AssertEquals("bookings.Count()", 1, bookings.Length);
				var booking = bookings[0].QuotedBooking.Booking;
				AssertEquals("booking.JS_BookingReference", "C2015442375", booking.JS_BookingReference);
				AssertEquals("booking.JS_GoodsDescription", "BUCKET OF MUD", booking.JS_GoodsDescription);
			});

			shipment.GoodsDescription = "GOLD, PURE GOLD.";
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>()
			{
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = consignorDataObject.AddressType, OrganizationCode = consignorBO.OH_Code },
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = consigneeDataObject.AddressType, OrganizationCode = consigneeBO.OH_Code },
			});

			CombineAssertions(delegate
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(shipment);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Updated Quick Booking - Booking (S00001000) from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000).
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Matched to Quick Booking - Booking (S00001000) with a Reference/Party ID match score of 90.
Successfully loaded matching QuotedBooking.
Populating QuotedBooking...
Matching 'ConsignorDocumentaryAddress':- Matched to 'JACSCHBNE' by code, address '643 JACK BOULEVARDE' (only address).
Matching 'ConsigneeDocumentaryAddress':- Matched to 'PHIMCKSYD' by code, address '153 PHIL BOULEVARDE' (only address).
Updated Quick Booking - Booking (S00001000) from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000).
".Trim(), logNoteText);

				var bookings = new BusinessObjectFactory().Load<ViewQuotedBooking>(new ZQuery());
				AssertEquals("bookings.Count()", 1, bookings.Length);
				var booking = bookings[0].QuotedBooking.Booking;
				AssertEquals("booking.JS_BookingReference", "C2015442375", booking.JS_BookingReference);
				AssertEquals("booking.JS_GoodsDescription", "GOLD, PURE GOLD.", booking.JS_GoodsDescription);
			});
		}

		public void TestReferenceAndPartyIDMatchingOnBookingReference()
		{
			eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataContext = shipment.DataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.ForwardingBooking, null);

			shipment.BookingConfirmationReference = "C2015442375";
			shipment.GoodsDescription = "BUCKET OF MUD";

			var orgGenerator = new OrganisationTestHelper(Factory);

			var consignorDataObject = orgGenerator.CreateDataObject("JACK", "SCHMIDT", "7643");
			consignorDataObject.AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress);

			var consigneeDataObject = orgGenerator.CreateDataObject("PHIL", "MCKRACKIN", "2153");
			consigneeDataObject.AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { consignorDataObject, consigneeDataObject });

			var consignorBO = orgGenerator.CreateBusinessObject("JACK", "SCHMIDT", "7643");
			var consigneeBO = orgGenerator.CreateBusinessObject("PHIL", "MCKRACKIN", "2153");
			Factory.SaveForTesting();

			AssertEquals("Precondition: Should be no Quoted Bookings in the DB yet.", null, new BusinessObjectFactory().LoadTop1<ViewQuotedBooking>(new ZQuery()));

			CombineAssertions(delegate
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(shipment);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000).
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No Reference/Party ID matches were found in this module.
No matching QuotedBooking found, creating new QuotedBooking.
Populating QuotedBooking...
Matching 'ConsignorDocumentaryAddress':- Matched to 'JACSCHBNE' address '643 JACK BOULEVARDE' with a score of 560.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'PHIMCKSYD' address '153 PHIL BOULEVARDE' with a score of 560.
Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000).
".Trim(), logNoteText);

				var bookings = new BusinessObjectFactory().Load<ViewQuotedBooking>(new ZQuery());
				AssertEquals("bookings.Count()", 1, bookings.Length);
				var booking = bookings[0].QuotedBooking.Booking;
				AssertEquals("booking.JS_BookingReference", "C2015442375", booking.JS_BookingReference);
				AssertEquals("booking.JS_GoodsDescription", "BUCKET OF MUD", booking.JS_GoodsDescription);
			});

			shipment.GoodsDescription = "GOLD, PURE GOLD.";

			CombineAssertions(delegate
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(shipment);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Updated Quick Booking - Booking (S00001000) from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000).
			".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Matched to Quick Booking - Booking (S00001000) with a Reference/Party ID match score of 90.
Successfully loaded matching QuotedBooking.
Populating QuotedBooking...
Matching 'ConsignorDocumentaryAddress':- Matched to 'JACSCHBNE' address '643 JACK BOULEVARDE' with a score of 560.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'PHIMCKSYD' address '153 PHIL BOULEVARDE' with a score of 560.
Updated Quick Booking - Booking (S00001000) from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000).
				".Trim(), logNoteText);

				var bookings = new BusinessObjectFactory().Load<ViewQuotedBooking>(new ZQuery());
				AssertEquals("bookings.Count()", 1, bookings.Length);
				var booking = bookings[0].QuotedBooking.Booking;
				AssertEquals("booking.JS_BookingReference", "C2015442375", booking.JS_BookingReference);
				AssertEquals("booking.JS_GoodsDescription", "GOLD, PURE GOLD.", booking.JS_GoodsDescription);
			});
		}

		public void TestReferenceAndPartyIDMatchingOnInterimReceiptNumber()
		{
			eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataContext = shipment.DataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.ForwardingBooking, null);

			shipment.InterimReceiptNumber = "IRC00342189";
			shipment.GoodsDescription = "BUCKET OF MUD";

			var orgGenerator = new OrganisationTestHelper(Factory);

			var consignorDataObject = orgGenerator.CreateDataObject("JOHN", "SCHMIDT", "7646");
			consignorDataObject.AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress);

			var consigneeDataObject = orgGenerator.CreateDataObject("TOMMY", "HADDOCK", "2122");
			consigneeDataObject.AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { consignorDataObject, consigneeDataObject });

			var consignorBO = orgGenerator.CreateBusinessObject("JOHN", "SCHMIDT", "7646");
			var consigneeBO = orgGenerator.CreateBusinessObject("TOMMY", "HADDOCK", "2122");
			Factory.SaveForTesting();

			AssertEquals("Precondition: Should be no Quoted Bookings in the DB yet.", null, new BusinessObjectFactory().LoadTop1<ViewQuotedBooking>(new ZQuery()));

			CombineAssertions(delegate
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(shipment);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000).
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No Reference/Party ID matches were found in this module.
No matching QuotedBooking found, creating new QuotedBooking.
Populating QuotedBooking...
Matching 'ConsignorDocumentaryAddress':- Matched to 'JOHSCHBNE' address '646 JOHN BOULEVARDE' with a score of 560.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'TOMHADSYD' address '122 TOMMY BOULEVARDE' with a score of 560.
Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000).
".Trim(), logNoteText);

				var bookings = new BusinessObjectFactory().Load<ViewQuotedBooking>(new ZQuery());
				AssertEquals("bookings.Count()", 1, bookings.Length);
				var booking = bookings[0].QuotedBooking.Booking;
				AssertEquals("booking.JS_InterimReceipt", "IRC00342189", booking.JS_InterimReceipt);
				AssertEquals("booking.JS_GoodsDescription", "BUCKET OF MUD", booking.JS_GoodsDescription);
			});

			shipment.GoodsDescription = "GOLD, PURE GOLD.";

			CombineAssertions(delegate
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(shipment);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Updated Quick Booking - Booking (S00001000) from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000).
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Matched to Quick Booking - Booking (S00001000) with a Reference/Party ID match score of 90.
Successfully loaded matching QuotedBooking.
Populating QuotedBooking...
Matching 'ConsignorDocumentaryAddress':- Matched to 'JOHSCHBNE' address '646 JOHN BOULEVARDE' with a score of 560.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'TOMHADSYD' address '122 TOMMY BOULEVARDE' with a score of 560.
Updated Quick Booking - Booking (S00001000) from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000).
".Trim(), logNoteText);

				var bookings = new BusinessObjectFactory().Load<ViewQuotedBooking>(new ZQuery());
				AssertEquals("bookings.Count()", 1, bookings.Length);
				var booking = bookings[0].QuotedBooking.Booking;
				AssertEquals("booking.JS_InterimReceipt", "IRC00342189", booking.JS_InterimReceipt);
				AssertEquals("booking.JS_GoodsDescription", "GOLD, PURE GOLD.", booking.JS_GoodsDescription);
			});
		}

		#endregion

		public void TestShipmentDataObjectWriterType()
		{
			var factory = new BusinessObjectFactory();

			foreach (QuoteBookingType quoteBookingType in Enum.GetValues(typeof(QuoteBookingType)))
			{
				var expectedWriterType = GetExpectedType(quoteBookingType);
				AssertWriterType(QuotedBooking.New(quoteBookingType, factory), expectedWriterType);
			}

			Type GetExpectedType(QuoteBookingType bookingType)
				=> bookingType switch
				{
					QuoteBookingType.SpotQuote => typeof(OneOffQuoteDataObjectWriter),
					QuoteBookingType.QuickBooking => typeof(ForwardingBookingDataObjectWriter),
					QuoteBookingType.BookingWithQuote => typeof(BookingWithQuoteDataObjectWriter),
					_ => throw new NotImplementedException($"Missing expected writer type for {bookingType}")
				};

			void AssertWriterType(QuotedBooking quotedBooking, Type expectedType)
			{
				var writeManager = new Mock<IDataWritingManager>();
				var action = new Mock<IUniversalActionInfo>();
				action.SetupGet(m => m.ParentBO).Returns(quotedBooking);
				writeManager.SetupGet(m => m.Action).Returns(action.Object);

				IShipmentDataContextManager dataContextManager = new QuotedBookingDataContextManager();
				var result = dataContextManager.GetShipmentDataObjectWriter(writeManager.Object);

				AssertNotNull(result);
				AssertType(expectedType, result);
			}
		}

		public void TestShipmentDataObjectReaderType()
		{
			var bwqUniversalShipment = new UniversalShipment { QuoteNumber = "00001234" };
			var bookingUniversalShipment = new UniversalShipment();

			IShipmentDataContextManagerInternal dataContextManager = new QuotedBookingDataContextManager();

			AssertType<BookingWithQuoteDataObjectReader>(dataContextManager.GetShipmentDataObjectReader(bwqUniversalShipment, new DummyLogger(), Factory));
			AssertType<ForwardingBookingDataObjectReader>(dataContextManager.GetShipmentDataObjectReader(bookingUniversalShipment, new DummyLogger(), Factory));
		}

		public void TestUseUnmatchedOrganisationForMatchingFunctionalityWorks()
		{
			var unmatchedOrgPK = OrganizationAddressTestHelper.SetUseUnmatchedOrganisationForMatchingRegistry(true);
			var universalShipmentWithUnmatchedOrganisations = resourceRetriever.Value.GetString("Enterprise.Freight.QuotedBookings.DataTransfer.Test.Universal.TestFiles.UniversalShipmentWithUnmatchedOrganisations.xml");
			var message = GetQueuedUniversalShipmentMessage(universalShipmentWithUnmatchedOrganisations);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000).
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No matching QuotedBooking found, creating new QuotedBooking.
Populating QuotedBooking...
Matching 'ConsignorDocumentaryAddress':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Matching 'ConsigneeDocumentaryAddress':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Matching 'LocalClient':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000).
".Trim(), logNoteText);
			});

			var bookings = Factory.Load<ViewQuotedBooking>(new ZQuery());
			AssertEquals("Precondition: bookings.Length", 1, bookings.Length);
			var reloadedShipment = bookings[0].QuotedBooking.Booking;
			AssertEquals("reloadedShipment.JS_GoodsDescription", "HATS", reloadedShipment.JS_GoodsDescription);

			var unmatchedOrgNotes = reloadedShipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
			AssertEquals("unmatchedOrgNotes.Length", 1, unmatchedOrgNotes.Length);
			var unmatchedOrgNote = unmatchedOrgNotes[0];
			AssertMultilineASCIIEquals("Unmatched Orgs Note Content", @"
Organisation Type: Consignor
Owner Code: 
EDI Code: 
Organisation Name: VAPOUR CORPORATION
Address Line 1: UNIT 0, -1 FANTASY LANE
Address Line 2: 
City: FAKE HILL
Post Code: 2987
State or Province: NSW
Country: AU
Doc Address Type: 
 
Organisation Type: Consignee
Owner Code: 
EDI Code: 
Organisation Name: TERRY TOWELLERS INC
Address Line 1: 238 APTITUDE PLAZA
Address Line 2: FANTASY VALLEY BUSINESS CENTRE
City: FANTASY VALLEY
Post Code: 4006
State or Province: QLD
Country: AU
Doc Address Type: 
 ".TrimStart(), unmatchedOrgNote.ST_NoteText);

			var unmatchedAddressPK = Factory.Load<OrgHeader>(unmatchedOrgPK).MainAddress.PK;
			AssertEquals("shipment.ConsignorDocumentaryAddress", unmatchedAddressPK, reloadedShipment.ConsignorDocumentaryAddress.E2_OA_Address);
			AssertEquals("shipment.ConsigneeDocumentaryAddress", unmatchedAddressPK, reloadedShipment.ConsigneeDocumentaryAddress.E2_OA_Address);

			var docAddressTypesPresent = reloadedShipment.DocAddresses
				.OfType<JobDocAddress>()
				.Select(a => a.DocAddressType.ToString())
				.OrderBy(t => t)
				.ToArray();
			AssertEquals("docAddressTypesPresent should not include the 'Client' address or any other 'Extras' not present in the incoming XML", @"
ConsigneeDocumentaryAddress
ConsigneePickupDeliveryAddress
ConsignorDocumentaryAddress
ConsignorPickupDeliveryAddress".Trim(), string.Join("\r\n", docAddressTypesPresent));
		}

		public void TestImportPickupAgent_OverrideAddress()
		{
			var universalShipmentOverrideAddress = resourceRetriever.Value.GetString("Enterprise.Freight.QuotedBookings.DataTransfer.Test.Universal.TestFiles.UniversalShipmentOverrideAddress.xml");
			var message = GetQueuedUniversalShipmentMessage(universalShipmentOverrideAddress);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var bookings = Factory.Load<ViewQuotedBooking>(new ZQuery());
			var reloadedShipment = bookings[0].QuotedBooking.Booking;

			AssertEquals(true, reloadedShipment.PickupAgentDocumentaryAddress.E2_AddressOverride);
			AssertEquals("T.H.I. LOGISTICS CO., LTD.-TAIPEI OFFICE", reloadedShipment.PickupAgentDocumentaryAddress.CompanyName);
			AssertEquals("12F.,NO.563,SEC.4,ZHONGXIAO E. RD., XINYI DISTRICT", reloadedShipment.PickupAgentDocumentaryAddress.E2_Address1);
			AssertEquals(",TAIPEI CITY,TAIWAN", reloadedShipment.PickupAgentDocumentaryAddress.E2_Address2);
			AssertEquals("KEELUNG, TAIWAN", reloadedShipment.PickupAgentDocumentaryAddress.City);
		}

		public void TestImportingMultipleAdditionalReferencesWhereTheTypeIsNonUniqueDoesNotTryAndDeDuplicate()
		{
			var universalShipmentWithMultipleAdditionalReferencesOfTheSameType = resourceRetriever.Value.GetString("Enterprise.Freight.QuotedBookings.DataTransfer.Test.Universal.TestFiles.UniversalShipmentWithMultipleAdditionalReferencesOfTheSameType.xml");

			var message = GetQueuedUniversalShipmentMessage(universalShipmentWithMultipleAdditionalReferencesOfTheSameType);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000) with 3 x CusEntryNumber.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No matching QuotedBooking found, creating new QuotedBooking.
Populating QuotedBooking...
No matching CusEntryNumber found, creating new CusEntryNumber.
Populating CusEntryNumber...
No matching CusEntryNumber found, creating new CusEntryNumber.
Populating CusEntryNumber...
Successfully loaded matching CusEntryNumber.
Populating CusEntryNumber...
No matching CusEntryNumber found, creating new CusEntryNumber.
Populating CusEntryNumber...
Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000) with 3 x CusEntryNumber.
".Trim(), logNoteText);

				var bookings = Factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "WB123456"));
				AssertEquals(1, bookings.Length);
				var booking = bookings[0];

				AssertEquals("WB123456", booking.JS_HouseBill);
				AssertMultilineASCIIEquals("", @"
AMS - AMS1201
CLC - CON1234
COC - COC1234
				".Trim()
					 , string.Join("\r\n", booking.Numbers.ToArray<CusEntryNumber>()
						.OrderBy(o => o.CE_EntryType + o.CE_EntryNum)
						.Select(o => o.CE_EntryType + " - " + o.CE_EntryNum).ToArray()));
			});
		}

		public void TestCreatesNewForwardingBookingIfAdditionalReferencesAreCorrectButHasNonMatchingEnterpriseAndServerID()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			quotedBooking.Booking.JS_UniqueConsignRef = "S00001010";

			var additionalReference1 = quotedBooking.Booking.Numbers.AddNew();
			additionalReference1.CE_EntryNum = "CE00001";
			additionalReference1.CE_EntryType = "AMS";
			additionalReference1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			Factory.SaveForTesting();

			var universalShipmentWithDifferentIDs = resourceRetriever.Value.GetString("Enterprise.Freight.QuotedBookings.DataTransfer.Test.Universal.TestFiles.UniversalShipmentWithDifferentIDs.xml");
			var message = GetQueuedUniversalShipmentMessage(universalShipmentWithDifferentIDs);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000) with 1 x CusEntryNumber.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No matching QuotedBooking found, creating new QuotedBooking.
Populating QuotedBooking...
No matching CusEntryNumber found, creating new CusEntryNumber.
Populating CusEntryNumber...
Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000) with 1 x CusEntryNumber.
".Trim(), logNoteText);
			});
		}

		public void TestImportForwardingBookingThroughAdditionalReferencesIfEnterpriseAndServerIDMatch()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			quotedBooking.Booking.JS_UniqueConsignRef = "S00001010";

			var additionalReference1 = quotedBooking.Booking.Numbers.AddNew();
			additionalReference1.CE_EntryNum = "CE00001";
			additionalReference1.CE_EntryType = "AMS";
			additionalReference1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			Factory.SaveForTesting();
			var message = GetQueuedUniversalShipmentMessage(UniversalShipment);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Updated Quick Booking - Booking (S00001010) from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001010) with 1 x CusEntryNumber.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Successfully loaded matching QuotedBooking.
Populating QuotedBooking...
Successfully loaded matching CusEntryNumber.
Populating CusEntryNumber...
Updated Quick Booking - Booking (S00001010) from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001010) with 1 x CusEntryNumber.
".Trim(), logNoteText);

				var shipment = new BusinessObjectFactory().LoadFromUniqueKey<ForwardingShipment>(JobShipmentSchema.JS_UniqueConsignRef, new ZString("S00001010"));
				AssertNotNull("Loaded Shipment.", shipment);
				AssertEquals("shipment.JS_HouseBill", "FRED235478923", shipment.JS_HouseBill);
				AssertEquals("shipment.JS_BookingReference", "FUL423189120", shipment.JS_BookingReference);
				AssertEquals("shipment.PK", quotedBooking.Booking.PK, shipment.PK);
			});
		}

		public void TestDoesNotLinkEventOnAdditionalReferencesIfNonMatchingEnterpriseAndServerID()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			quotedBooking.Booking.JS_UniqueConsignRef = "S00001010";

			var additionalReference1 = quotedBooking.Booking.Numbers.AddNew();
			additionalReference1.CE_EntryNum = "134FREGT";
			additionalReference1.CE_EntryType = "AMS";
			additionalReference1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			Factory.SaveForTesting();

			var universalEventWithDifferentIDs = resourceRetriever.Value.GetString("Enterprise.Freight.QuotedBookings.DataTransfer.Test.Universal.TestFiles.UniversalEventWithDifferentIDs.xml");
			var message = GetQueuedUniversalEventMessage(universalEventWithDifferentIDs);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Warning - No Module found a Business Entity to link this Universal Event to.
				".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Warning - No Module found a Business Entity to link this Universal Event to.
Message Discarded.
				".Trim(), message.GetLogNoteText());

				var logs = quotedBooking.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookedCode));
				AssertEquals("[BKD] - Booked event count", 0, logs.Length);
			});
		}

		public void TestLinkEventOnAdditionalReferencesIfEnterpriseAndServerIDMatch()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			quotedBooking.Booking.JS_UniqueConsignRef = "S00001010";

			var additionalReference1 = quotedBooking.Booking.Numbers.AddNew();
			additionalReference1.CE_EntryNum = "134FREGT";
			additionalReference1.CE_EntryType = "AMS";
			additionalReference1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(UniversalEvent);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Quick Booking - Booking (S00001010).
				".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Quick Booking - Booking (S00001010).
				".Trim(), message.GetLogNoteText());

				var logs = quotedBooking.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookedCode));
				AssertEquals("[BKD] - Booked event count", 1, logs.Length);
			});
		}

		public void TestCancelledBookingIsNotMatched()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);

			var shipmentBOToLoad = quotedBooking.Booking;
			shipmentBOToLoad.JS_UniqueConsignRef = "S00001003";
			shipmentBOToLoad.JS_BookingReference = "FUL423189120";
			shipmentBOToLoad.JS_HouseBill = "ONTHEHOUSE";
			shipmentBOToLoad.JS_IsCancelled = true;

			Factory.SaveForTesting();

			var universalShipmentWithTarget = resourceRetriever.Value.GetString("Enterprise.Freight.QuotedBookings.DataTransfer.Test.Universal.TestFiles.UniversalShipmentWithTarget.xml");
			var message = GetQueuedUniversalShipmentMessage(universalShipmentWithTarget);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var factory = new BusinessObjectFactory();
			var viewBooking = factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_JS, shipmentBOToLoad.PK));

			CombineAssertions(delegate
			{
				Assert(viewBooking.VB_IsCanceled);
				AssertMultilineASCIIEquals("Service Task Log", @"
ERROR - Match couldn't be found for ForwardingBooking with Key S00001003
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.".Trim(), serviceTaskLog.ToString());
			});
		}

		public void TestImportForwardingBookingThroughJobNumber()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);

			var shipmentBOToLoad = quotedBooking.Booking;
			shipmentBOToLoad.JS_UniqueConsignRef = "S00001003";
			shipmentBOToLoad.JS_BookingReference = "FUL423189120";
			shipmentBOToLoad.JS_HouseBill = "ONTHEHOUSE";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(UniversalShipment);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Updated Quick Booking - Booking (S00001003) from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001003) with 1 x CusEntryNumber.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Successfully loaded matching QuotedBooking.
Populating QuotedBooking...
No matching CusEntryNumber found, creating new CusEntryNumber.
Populating CusEntryNumber...
Updated Quick Booking - Booking (S00001003) from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001003) with 1 x CusEntryNumber.
".Trim(), logNoteText);

				var shipment = new BusinessObjectFactory().LoadFromUniqueKey<ForwardingShipment>(JobShipmentSchema.JS_UniqueConsignRef, new ZString("S00001003"));
				AssertNotNull("Loaded Shipment.", shipment);
				AssertEquals("shipment.JS_HouseBill", "FRED235478923", shipment.JS_HouseBill);
				AssertEquals("shipment.JS_BookingReference", "FUL423189120", shipment.JS_BookingReference);
				AssertEquals("shipment.PK", quotedBooking.Booking.PK, shipment.PK);
			});
		}

		public void TestCanImportForwardingBookingViaUniversalDataBuss()
		{
			var message = GetQueuedUniversalShipmentMessage(UniversalShipment);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000) with 1 x CusEntryNumber.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No matching QuotedBooking found, creating new QuotedBooking.
Populating QuotedBooking...
No matching CusEntryNumber found, creating new CusEntryNumber.
Populating CusEntryNumber...
Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000) with 1 x CusEntryNumber.
".Trim(), logNoteText);

				var booking = Factory.Load<ViewQuotedBooking>(new ZQuery())[0];
				AssertNotNull("Forwarding Shipment should exist with a House Bill of [FRED235478923].", booking);
			});
		}

		public void TestProcessUniversalShipmentTrigger_Booking()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, new BusinessObjectFactory());
			quotedBooking.Booking.JS_BookingReference = "FUL423189120";
			quotedBooking.Booking.JS_UniqueConsignRef = "S100110011";

			AssertProcessUniversalShipmentTrigger(quotedBooking, ExpectedUniversalShipmentMessageBooking.Trim());
		}

		public void TestProcessUniversalShipmentTrigger_OneOffQuote()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.SpotQuote, new BusinessObjectFactory());

			AssertProcessUniversalShipmentTrigger(quotedBooking, ExpectedUniversalShipmentMessageOneOffQuote.Trim());
		}

		#region ExpectedUniversalShipmentMessage

		const string ExpectedUniversalShipmentMessageBooking = @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingBooking</Type>
          <Key>S100110011</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>APP</Code>
        <Description>As Per Payload</Description>
      </ActionPurpose>
      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <EventBranch>
        <Code>BNE</Code>
        <Name>BN - AUBNE</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>ATH</Code>
        <Description>Authorized</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2010-12-25T00:00:00.000+10:00</TriggerDate>
      <TriggerDescription>Send Data</TriggerDescription>
      <TriggerType>Trigger</TriggerType>
    </DataContext>
";

		const string ExpectedUniversalShipmentMessageOneOffQuote = @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>OneOffQuote</Type>
          <Key>00001000</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>APP</Code>
        <Description>As Per Payload</Description>
      </ActionPurpose>
      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <EventBranch>
        <Code>BNE</Code>
        <Name>BN - AUBNE</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>ATH</Code>
        <Description>Authorized</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2010-12-25T00:00:00.000+10:00</TriggerDate>
      <TriggerDescription>Send Data</TriggerDescription>
      <TriggerType>Trigger</TriggerType>
    </DataContext>
";

		#endregion

		void AssertProcessUniversalShipmentTrigger(QuotedBooking quotedBooking, string expectedMessage)
		{
			var factory = quotedBooking.Factory;
			using (factory.AddDisposableService())
			{
				AssertEquals("Pre-condition: Expected quotedBooking type to be a QuotedBooking. This may fail is another test has not reset OverriddenNewDelegate",
					typeof(QuotedBooking), quotedBooking.GetType());

				var trigger = quotedBooking.WorkflowItems.AddNew();
				trigger.P9_Description = "Send Data";
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.AsPerPayload;

				var communicationsMode = factory.New<EDICommunicationsMode>();
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				communicationsMode.EK_Destination = "9CHARCODE";

				factory.Save();

				var logBO = quotedBooking.GetLogs().AddNew(new EventValue(Events.Authorised, eventTime: new ZDateTimeOffset(2010, 12, 25)));

				var dataContextManager = new QuotedBookingDataContextManager() as IShipmentDataContextManager;

				var logger = new TestLogger();
				var actionInfo = new ActionWrapper(action, quotedBooking, null);
				var processor = new UniversalXmlWorkflowProcessor(actionInfo, new UniversalXmlCommunicationModeProvider(() => (new[] { communicationsMode }, null)), (outboundSessionTracker) => dataContextManager.GetShipmentDataObjectWriter(outboundSessionTracker), quotedBooking);

				processor.Process(logger);
				factory.Save();

				AssertMultilineASCIIEquals("Logs generated while processing - Apparently no news is good news."
					, @"".Trim()
					, logger.GetAllLogsAsString());

				var messages = Factory.Load<XmlEDIMessage>(new ZQuery());
				AssertEquals("messages.Length", 1, messages.Length);

				var message = messages[0];
				CombineAssertions(delegate
				{
					AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
					AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
					AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);

					AssertEquals("message.EM_ApplicationReference", "", message.EM_ApplicationReference);
					AssertEquals("message.EM_LinkTable", "ViewQuotedBooking", message.EM_LinkTable);
					AssertEquals("message.EM_LinkUniqueID", quotedBooking.PK, message.EM_LinkUniqueID);

					AssertContains("message.EM_MessageText", expectedMessage, message.EM_MessageText);

					AssertEquals("message.EM_IsActive", ZBool.True, message.EM_IsActive);
					AssertEquals("message.EM_IsTestMessage", ZBool.False, message.EM_IsTestMessage);
				});
			}
		}

		public void TestProcessUniversalEventTriggerViaEHub()
		{
			var factory = new BusinessObjectFactory();
			using (factory.AddDisposableService())
			{
				var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
				quotedBooking.Booking.JS_BookingReference = "FUL423189120";
				quotedBooking.Booking.JS_UniqueConsignRef = "S100110011";
				quotedBooking.Booking.JS_CFSReference = "CANTFRIGGENSAVE";
				quotedBooking.Booking.JS_InterimReceipt = "INTERIM";
				quotedBooking.Origin = "AUMEL";
				quotedBooking.Destination = "USLAX";
				quotedBooking.Booking.DocsAndCartage.JP_OrderItemsAsString = "Order Items #1";

				AssertEquals("Pre-condition: Expected quotedBooking type to be a generic QuotedBooking. This may fail is another test has not reset OverriddenNewDelegate",
					typeof(QuotedBooking), quotedBooking.GetType());

				var trigger = quotedBooking.WorkflowItems.AddNew();
				trigger.P9_Description = "Received Goods";
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = Events.ReceivedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
				action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.Event;

				var communicationsMode = factory.New<EDICommunicationsMode>();
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				communicationsMode.EK_Destination = "UNVRSLVNT";

				var logBO = quotedBooking.Logs.AddNew(new EventValue(Events.Received, eventTime: new ZDateTimeOffset(2010, 12, 25)));

				var logger = new TestLogger();
				var actionInfo = new ActionWrapper(action, quotedBooking, null);
				var processor = new UniversalXmlWorkflowProcessor(
					actionInfo,
					new UniversalXmlCommunicationModeProvider(() => (new[] { communicationsMode }, null)),
					(outboundSessionTracker) => new EventDataObjectWriter(outboundSessionTracker),
					logBO);

				factory.Save();

				processor.Process(logger);
				factory.Save();

				AssertMultilineASCIIEquals("Logs generated while processing - Apparently no news is good news."
					, @"".Trim()
					, logger.GetAllLogsAsString());

				var messages = Factory.Load<XmlEDIMessage>(new ZQuery());
				AssertEquals("messages.Length", 1, messages.Length);

				var newExpectedUniversalEventMessage = ExpectedUniversalEventMessage.Insert(ExpectedUniversalEventMessage.IndexOf("<IsEstimate>false</IsEstimate>"),
					string.Format("<CreatedTime>{0}</CreatedTime>{1}    ", SimpleTypeFormatter.GetFormattedValueForWritingToXml(logBO.SL_PostedTimeUtc.ToOffset(), () => 0), System.Environment.NewLine));

				var message = messages[0];
				var interchange = message.Interchange;
				CombineAssertions(delegate
				{
					AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
					AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
					AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalEvent, message.EM_MessageSubType);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);

					AssertEquals("message.EM_ApplicationReference", "", message.EM_ApplicationReference);
					AssertEquals("message.EM_LinkTable", "ViewQuotedBooking", message.EM_LinkTable);
					AssertEquals("message.EM_LinkUniqueID", quotedBooking.PK, message.EM_LinkUniqueID);

					AssertMultilineASCIIEquals("message.EM_MessageText", string.Format(newExpectedUniversalEventMessage.Trim(), interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum), message.EM_MessageText);

					AssertEquals("message.EM_IsActive", ZBool.True, message.EM_IsActive);
					AssertEquals("message.EM_IsTestMessage", ZBool.False, message.EM_IsTestMessage);
				});
			}
		}

		#region ExpectedUniversalEventMessage

		const string ExpectedUniversalEventMessage = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingBooking</Type>
          <Key>S100110011</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>EVT</Code>
        <Description>Event</Description>
      </ActionPurpose>
      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <EventBranch>
        <Code>BNE</Code>
        <Name>BN - AUBNE</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>RCV</Code>
        <Description>Received</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2010-12-25T00:00:00.000+10:00</TriggerDate>
      <TriggerDescription>Received Goods</TriggerDescription>
      <TriggerType>Trigger</TriggerType>
    </DataContext>

    <EventTime>2010-12-25T00:00:00.000+10:00</EventTime>
    <EventType>RCV</EventType>
    <IsEstimate>false</IsEstimate>

    <ContextCollection>
      <Context>
        <Type>HBOLOriginUNLOCO</Type>
        <Value>AUMEL</Value>
      </Context>
      <Context>
        <Type>HBOLDestinationUNLOCO</Type>
        <Value>USLAX</Value>
      </Context>
      <Context>
        <Type>ShippersReference</Type>
        <Value>FUL423189120</Value>
      </Context>
      <Context>
        <Type>OrderNumber</Type>
        <Value>Order</Value>
      </Context>
      <Context>
        <Type>OrderNumber</Type>
        <Value>Items</Value>
      </Context>
      <Context>
        <Type>OrderNumber</Type>
        <Value>#1</Value>
      </Context>
      <Context>
        <Type>InterimReceipt</Type>
        <Value>INTERIM</Value>
      </Context>
      <Context>
        <Type>CFSReference</Type>
        <Value>CANTFRIGGENSAVE</Value>
      </Context>
    </ContextCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
  </Event>
</UniversalEvent>";

		#endregion

		public void TestImportEventWithNoDataTargetDoesNotMatchConsolidatedBooking()
		{
			var factory = new BusinessObjectFactory();
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			quotedBooking.Booking.JS_UniqueConsignRef = "S00001016";
			quotedBooking.Booking.JS_HouseBill = "SAFEHOUSE2021X";

			var shipment = factory.Load<CommonShipment>(quotedBooking.Booking.PK);
			var helper = new BuildConsolHelper();
			helper.TurnBookingIntoShipment(shipment, null, quotedBooking.PK);

			factory.Save();
			Factory.SaveForTesting();

			var factory2 = new BusinessObjectFactory();
			var viewBooking = factory2.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_JS, shipment.PK));
			AssertEquals("Precondition: VB_IsConsolidated", true, viewBooking.VB_IsConsolidated);

			var universalEventWithNoDataTarget = resourceRetriever.Value.GetString("Enterprise.Freight.QuotedBookings.DataTransfer.Test.Universal.TestFiles.UniversalEventWithNoDataTarget.xml");
			var message = GetQueuedUniversalEventMessage(universalEventWithNoDataTarget);
			message = factory2.Load<XmlEDIMessage>(message.PK);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", "Linked Event to Shipment S00001016 (House Bill='SAFEHOUSE2021X').".Trim(), serviceTaskLog.ToString());
				AssertMultilineASCIIEquals("Message Log Note", "'Goods Signed By' has not been updated, because there was no ContextType ReceivedFromName specified or was empty.\r\nLinked Event to Shipment S00001016 (House Bill='SAFEHOUSE2021X').".Trim(), message.GetLogNoteText());
			});
		}

		public void TestImportEventWithDataTargetDoesNotUseContextInformationIfDataTargetMatches()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			quotedBooking.Booking.JS_UniqueConsignRef = "S00001003";

			var dummyBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			dummyBooking.Booking.JS_UniqueConsignRef = "S00002010";
			dummyBooking.Booking.JS_BookingReference = "FUL423189120";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(UniversalEvent);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Quick Booking - Booking (S00001003).
				".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Quick Booking - Booking (S00001003).
				".Trim(), message.GetLogNoteText());

				var logs = quotedBooking.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookedCode));
				AssertEquals("[BKD] - Booked event count", 1, logs.Length);
				var log = logs[0];

				var contextItems = log.SourceInfoItems;
				var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select((item) => item.Key + " - " + item.Data).ToArray());
				AssertMultilineASCIIEquals("Context Items on Event", @"
Shippers Reference - FUL423189120
Order Number - Order
Order Number - Items
Order Number - #1
Interim Receipt - INTERIM
CFS Reference - CANTFRIGGENSAVE
HBOL Origin UNLOCO - AU
HBOL Destination UNLOCO - US
AMS Number - 134FREGT
COC - 267AIRGT
Data Source Company - EDI - Eagle Datamation International
Data Source Enterprise ID - EDI
Data Source Server ID - DAT
				".Trim(), actualContextItems);
			});
		}

		public void TestCanImportEventViaUniversalDataBuss()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			quotedBooking.Booking.JS_UniqueConsignRef = "S00001010";
			quotedBooking.Booking.JS_BookingReference = "FUL423189120";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(UniversalEvent);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Quick Booking - Booking (S00001010).
				".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Quick Booking - Booking (S00001010).
				".Trim(), message.GetLogNoteText());

				var logs = quotedBooking.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookedCode));
				AssertEquals("[BKD] - Booked event count", 1, logs.Length);
				var log = logs[0];

				var contextItems = log.SourceInfoItems;
				var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select((item) => item.Key + " - " + item.Data).ToArray());
				AssertMultilineASCIIEquals("Context Items on Event", @"
Shippers Reference - FUL423189120
Order Number - Order
Order Number - Items
Order Number - #1
Interim Receipt - INTERIM
CFS Reference - CANTFRIGGENSAVE
HBOL Origin UNLOCO - AU
HBOL Destination UNLOCO - US
AMS Number - 134FREGT
COC - 267AIRGT
Data Source Company - EDI - Eagle Datamation International
Data Source Enterprise ID - EDI
Data Source Server ID - DAT
				".Trim(), actualContextItems);
			});
		}

		public void TestContextInformationIsAllThereForSea()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			quotedBooking.Booking.JS_TransportMode = Constants.TransportModes.Sea;
			quotedBooking.Booking.JS_HouseBill = "ONTHEHOUSE";
			quotedBooking.Booking.JS_BookingReference = "FUL423189120";
			quotedBooking.Booking.JS_CFSReference = "CANTFRIGGENSAVE";
			quotedBooking.Booking.JS_InterimReceipt = "INTERIM";
			quotedBooking.Origin = "AUMEL";
			quotedBooking.Destination = "USLAX";
			quotedBooking.Booking.DocsAndCartage.JP_OrderItemsAsString = "Order Items #1";

			var additionalReference1 = quotedBooking.Booking.Numbers.AddNew();
			additionalReference1.CE_EntryNum = "CE00001";
			additionalReference1.CE_EntryType = "AMS";
			additionalReference1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var additionalReference2 = quotedBooking.Booking.Numbers.AddNew();
			additionalReference2.CE_EntryNum = "CE00002";
			additionalReference2.CE_EntryType = "COC";
			additionalReference2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var manager = (IEventDataContextManager)quotedBooking.GetUniversalDataContextManager();

			var eventContextValues = string.Join("\r\n", manager.EventContextValues.Select(o => o.Key + " - " + o.Value).ToArray());

			AssertMultilineASCIIEquals("manager.EventContextValues", @"
HBOLNumber - ONTHEHOUSE
HBOLOriginUNLOCO - AUMEL
HBOLDestinationUNLOCO - USLAX
ShippersReference - FUL423189120
OrderNumber - Order
OrderNumber - Items
OrderNumber - #1
InterimReceipt - INTERIM
AMS Number - CE00001
Customs Office Code (Override) - CE00002
CFSReference - CANTFRIGGENSAVE
			".Trim(), eventContextValues);
		}

		public void TestContextInformationIsAllThereForAir()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			quotedBooking.Booking.JS_TransportMode = Constants.TransportModes.Air;
			quotedBooking.Booking.JS_HouseBill = "ONTHEHOUSE";
			quotedBooking.Booking.JS_BookingReference = "FUL423189120";
			quotedBooking.Booking.JS_CFSReference = "CANTFRIGGENSAVE";
			quotedBooking.Booking.JS_InterimReceipt = "INTERIM";
			quotedBooking.Origin = "AUMEL";
			quotedBooking.Destination = "USLAX";
			quotedBooking.Booking.DocsAndCartage.JP_OrderItemsAsString = "Order Items #1";

			var additionalReference1 = quotedBooking.Booking.Numbers.AddNew();
			additionalReference1.CE_EntryNum = "CE00001";
			additionalReference1.CE_EntryType = "AMS";
			additionalReference1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var additionalReference2 = quotedBooking.Booking.Numbers.AddNew();
			additionalReference2.CE_EntryNum = "CE00002";
			additionalReference2.CE_EntryType = "COC";
			additionalReference2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var manager = quotedBooking.GetUniversalDataContextManager() as IEventDataContextManager;

			var eventContextValues = string.Join("\r\n", manager.EventContextValues.Select(o => o.Key + " - " + o.Value).ToArray());

			AssertMultilineASCIIEquals("manager.EventContextValues", @"
HAWBNumber - ONTHEHOUSE
HAWBOriginIATAAirportCode - MEL
HAWBDestinationIATAAirportCode - LAX
HBOLOriginUNLOCO - AUMEL
HBOLDestinationUNLOCO - USLAX
ShippersReference - FUL423189120
OrderNumber - Order
OrderNumber - Items
OrderNumber - #1
InterimReceipt - INTERIM
AMS Number - CE00001
Customs Office Code (Override) - CE00002
CFSReference - CANTFRIGGENSAVE
			".Trim(), eventContextValues);
		}

		public void TestDataContextKey()
		{
			var bookingWithQuote = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory.BOFactory);
			bookingWithQuote.Booking.JS_UniqueConsignRef = "S00001";
			bookingWithQuote.Quote.TH_QuoteNumber = "Q00001";

			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			quickBooking.Booking.JS_UniqueConsignRef = "S000002";

			var spotQuote = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory.BOFactory);
			spotQuote.Quote.TH_QuoteNumber = "Q00002";

			var contextManager = (IDataContextManager)new QuotedBookingDataContextManager();
			AssertEquals(string.Empty, contextManager.DataContextKey);

			contextManager = bookingWithQuote.GetUniversalDataContextManager();
			AssertEquals("S00001", contextManager.DataContextKey);

			contextManager = quickBooking.GetUniversalDataContextManager();
			AssertEquals("S000002", contextManager.DataContextKey);

			contextManager = spotQuote.GetUniversalDataContextManager();
			AssertEquals("Q00002", contextManager.DataContextKey);
		}

		public void TestDataContextType()
		{
			var bookingWithQuote = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory.BOFactory);
			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var spotQuote = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory.BOFactory);

			var contextManager = (IDataContextManager)new QuotedBookingDataContextManager();
			AssertEquals(DataContextType.ForwardingBooking, contextManager.DataContextType);

			contextManager = bookingWithQuote.GetUniversalDataContextManager();
			AssertEquals(DataContextType.ForwardingBooking, contextManager.DataContextType);

			contextManager = quickBooking.GetUniversalDataContextManager();
			AssertEquals(DataContextType.ForwardingBooking, contextManager.DataContextType);

			contextManager = spotQuote.GetUniversalDataContextManager();
			AssertEquals(DataContextType.OneOffQuote, contextManager.DataContextType);
		}

		public void TestManagesEventsAndShipments()
		{
			var contextManager = (IDataContextManager)new QuotedBookingDataContextManager();
			AssertEquals(true, contextManager.ManagesEvents());
			AssertEquals(true, contextManager.ManagesShipments());

			var quotedBooking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory.BOFactory);
			contextManager = quotedBooking.GetUniversalDataContextManager();
			AssertEquals(false, contextManager.ManagesEvents());
			AssertEquals(true, contextManager.ManagesShipments());

			quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			contextManager = quotedBooking.GetUniversalDataContextManager();
			AssertEquals(true, contextManager.ManagesEvents());
			AssertEquals(true, contextManager.ManagesShipments());

			quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory.BOFactory);
			contextManager = quotedBooking.GetUniversalDataContextManager();
			AssertEquals(true, contextManager.ManagesEvents());
			AssertEquals(true, contextManager.ManagesShipments());
		}

		public void TestGetReasonForNotAbleToUpdateLogParentFromEvent()
		{
			var quotedBooking1 = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var quotedBooking2 = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);

			var shipment2 = Factory.Load<CommonShipment>(quotedBooking2.Booking.PK);
			var helper = new BuildConsolHelper();
			helper.TurnBookingIntoShipment(shipment2, null, quotedBooking2.PK);

			var contextManager = (IEventDataContextManager)new QuotedBookingDataContextManager();

			var universalEvent = new UniversalEvent()
			{
				EventType = Events.PickupCartageCompleteFinalisedCode
			};

			AssertEquals(true, contextManager.CanUpdateLogParentFromEvent(quotedBooking1, universalEvent, out var failureReason1));
			AssertEquals(ZString.Empty, failureReason1);

			AssertEquals(false, contextManager.CanUpdateLogParentFromEvent(quotedBooking2, universalEvent, out var failureReason2));
			AssertEquals("[*XML Targeted a converted Booking. Target Type should be ForwardingShipment for updating converted Bookings.*]", failureReason2);
		}

		#region NVOCC RecipientRole

		public void TestImportWithNVOCCRecipientRole()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>A</CoLoadBookingConfirmationReference>
	<AgentsReference>BOOKS</AgentsReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
No matching QuotedBooking found, creating new QuotedBooking.
Populating QuotedBooking...
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000).
".Trim(), message.GetLogNoteText());
		}

		public void TestImportWithNVOCCRecipientRole_WithoutBRQServiceCode()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>TWR</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>A</CoLoadBookingConfirmationReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
No Module used this Universal Shipment data.
Hint: Adding an element in the DataTargetCollection will make the specified Module import where no existing data matches.
Message Discarded.
".Trim(), message.GetLogNoteText());
		}

		public void TestImportWithoutNVOCCRecipientRole()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<CoLoadBookingConfirmationReference>A</CoLoadBookingConfirmationReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
No Module used this Universal Shipment data.
Hint: Adding a DataContext element with an element in the DataTargetCollection will make the specified Module import where no existing data matches.
Message Discarded.
".Trim(), message.GetLogNoteText());
		}

		#endregion

		#region NVOCC Importing

		public void TestBookingPartyIsMissing()
		{
			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>A</CoLoadBookingConfirmationReference>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Error - Cannot populate QuotedBooking because:
[*Booking party is missing.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestBookingPartyDocumentaryAddressDoesNotMatch()
		{
			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<AgentsReference>BOOKS</AgentsReference>
	<CoLoadBookingConfirmationReference>A</CoLoadBookingConfirmationReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
		<CompanyName>TESTCOMPANY</CompanyName>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Warning - Matching 'BookingPartyDocumentaryAddress':- No match found for '[Org. Code: BKGPARTY; Company Name: TESTCOMPANY; Address 1: Booking Party Address 1]'.
Warning - Matching 'BookingPartyDocumentaryAddress':- No match found for '[Org. Code: BKGPARTY; Company Name: TESTCOMPANY; Address 1: Booking Party Address 1]'.
Error - Cannot populate QuotedBooking because:
[*Booking Request message is received from an unknown Booking Party TESTCOMPANY. Booking rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestCoLoadBookingConfirmationReferenceIsMissingForAmendmentMessage()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		 <DocumentaryOverride>
            <DataVersion>1</DataVersion>
            <Purpose>
				<Code>AMD</Code>
			</Purpose>
         </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate QuotedBooking because:
[*Mandatory Booking Number is missing in Booking Request Amendment message, amendment rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestBookingWithSameAgentsReferenceIsUpdatedForAmendmentMessage()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";
			quotedBooking1.Booking.JS_BookingReference = "BOOKS";
			quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		 <DocumentaryOverride>
            <DataVersion>1</DataVersion>
            <Purpose>
				<Code>AMD</Code>
			</Purpose>
         </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<AgentsReference>BOOKS</AgentsReference>
	<CoLoadBookingConfirmationReference>S00005001</CoLoadBookingConfirmationReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching QuotedBooking.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Populating QuotedBooking...
Updated Booking with Quote - Quote (00001000) - Booking (S00005001) from UniversalShipment.
Successfully saved Booking with Quote - Quote (00001000) - Booking (S00005001).
".Trim(), message.GetLogNoteText());
		}

		public void TestBookingWithDifferentAgentsReferenceForAmendmentMessage()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";
			quotedBooking1.Booking.JS_BookingReference = "BOOKS";
			quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		 <DocumentaryOverride>
            <DataVersion>1</DataVersion>
            <Purpose>
				<Code>AMD</Code>
			</Purpose>
         </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00005001</CoLoadBookingConfirmationReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching QuotedBooking.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate QuotedBooking because:
[*Booking Amendment message with Booking Number S00005001 cannot find a matching Booking to update, amendment rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestShipperReferenceIsMissing()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>A</CoLoadBookingConfirmationReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate QuotedBooking because:
[*Shipper reference is missing.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestBookingDoesNotExistForOriginalMessage_OldParameterShouldNotExistForStatusUpdatedLog()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		 <DocumentaryOverride>
            <DataVersion>1</DataVersion>
            <Purpose>
				<Code>ORG</Code>
			</Purpose>
         </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<AgentsReference>BOOKS</AgentsReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
No matching QuotedBooking found, creating new QuotedBooking.
Populating QuotedBooking...
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000).
".Trim(), message.GetLogNoteText());

			var factory2 = new BusinessObjectFactory();
			var query = new ZQuery(JobShipmentSchema.JS_BookingReference, "BOOKS");
			var booking = factory2.Load<ForwardingShipment>(query).FirstOrDefault();
			AssertNotNull(booking);

			var statusUpdatedLog = booking.Logs.MostRecentLogByEventTime(Events.StatusUpdated);
			AssertNotNull(statusUpdatedLog);
			Assert("Old parameter should not be used", !statusUpdatedLog.Parameters.ContainsKey(Params.Old));
		}

		public void TestBookingAlreadyExistsForOriginalMessage_OldParameterShouldExistForStatusUpdatedLog()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var bookingParty = Factory.New<OrgHeader>();
				bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
				bookingParty.OH_Code = "BKGPARTY";
				bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

				var quotedBooking1 = CreateQuotedBooking();
				quotedBooking1.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.BookingRejected;
				quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";
				quotedBooking1.Booking.JS_BookingReference = "BOOKS";
				quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

				Factory.SaveForTesting();

				#region Message

				var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		 <DocumentaryOverride>
            <DataVersion>1</DataVersion>
            <Purpose>
				<Code>ORG</Code>
			</Purpose>
         </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00005001</CoLoadBookingConfirmationReference>
	<AgentsReference>BOOKS</AgentsReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

				#endregion

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching QuotedBooking.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Populating QuotedBooking...
Updated Booking with Quote - Quote (00001000) - Booking (S00005001) from UniversalShipment.
Successfully saved Booking with Quote - Quote (00001000) - Booking (S00005001).
".Trim(), message.GetLogNoteText());

				var factory2 = new BusinessObjectFactory();
				var booking2 = factory2.Load<ForwardingShipment>(quotedBooking1.Booking.PK);
				AssertNotNull(booking2);

				var statusUpdatedLog = booking2.Logs.MostRecentLogByEventTime(Events.StatusUpdated);
				AssertNotNull(statusUpdatedLog);
				AssertEquals("Old parameter should be used", ShipmentStatusList.Codes.BookingRejected, statusUpdatedLog.Parameters[Params.Old]);
			}
		}

		public void TestBookingAlreadyExistsForOriginalMessage_NoStatusUpdatedLog_WhenShipmentStatusIsNotChanged()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var bookingParty = Factory.New<OrgHeader>();
				bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
				bookingParty.OH_Code = "BKGPARTY";
				bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

				var quotedBooking1 = CreateQuotedBooking();
				quotedBooking1.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
				quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";
				quotedBooking1.Booking.JS_BookingReference = "BOOKS";
				quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

				Factory.SaveForTesting();

				#region Message

				var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		 <DocumentaryOverride>
            <DataVersion>1</DataVersion>
            <Purpose>
				<Code>ORG</Code>
			</Purpose>
         </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00005001</CoLoadBookingConfirmationReference>
	<AgentsReference>BOOKS</AgentsReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

				#endregion

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching QuotedBooking.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Populating QuotedBooking...
Updated Booking with Quote - Quote (00001000) - Booking (S00005001) from UniversalShipment.
Successfully saved Booking with Quote - Quote (00001000) - Booking (S00005001).
".Trim(), message.GetLogNoteText());

				var factory2 = new BusinessObjectFactory();
				var booking2 = factory2.Load<ForwardingShipment>(quotedBooking1.Booking.PK);
				AssertNotNull(booking2);

				var statusUpdatedLog = booking2.Logs.MostRecentLogByEventTime(Events.StatusUpdated);
				AssertNull(statusUpdatedLog);
			}
		}

		public void TestBookingAlreadyExistsForOriginalMessage()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";
			quotedBooking1.Booking.JS_BookingReference = "CAR";
			quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		 <DocumentaryOverride>
            <DataVersion>1</DataVersion>
            <Purpose>
				<Code>ORG</Code>
			</Purpose>
         </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00005001</CoLoadBookingConfirmationReference>
	<AgentsReference>BOOKS</AgentsReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching QuotedBooking.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate QuotedBooking because:
[*Booking Request message cannot update Booking S00005001 because Shipper Reference in the message BOOKS is different to one in the Booking. Booking rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestBookingShippmentStatusIsBookedForOriginalMessage()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";
			quotedBooking1.Booking.JS_BookingReference = "CAR";
			quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		 <DocumentaryOverride>
            <DataVersion>1</DataVersion>
            <Purpose>
				<Code>ORG</Code>
			</Purpose>
         </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00005001</CoLoadBookingConfirmationReference>
	<AgentsReference>CAR</AgentsReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching QuotedBooking.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate QuotedBooking because:
[*Booking Request original cannot be processed once Booking is confirmed (Booking Status BKD). Booking rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestBookingHasBeenConvertedToShippmentForOriginalMessage()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
			quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";
			quotedBooking1.Booking.JS_BookingReference = "CAR";
			quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			quotedBooking1.Booking.JS_IsForwardRegistered = true;

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		 <DocumentaryOverride>
            <DataVersion>1</DataVersion>
            <Purpose>
				<Code>ORG</Code>
			</Purpose>
         </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00005001</CoLoadBookingConfirmationReference>
	<AgentsReference>CAR</AgentsReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching QuotedBooking.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate QuotedBooking because:
[*Booking Request original message cannot be processed once Booking is converted to Shipment. Booking rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestBookingHasStatusCancelledAndAmendmentReceived_MessageRejection()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.BookingCancelled;
			quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";
			quotedBooking1.Booking.JS_BookingReference = "CAR";
			quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			quotedBooking1.Booking.JS_IsForwardRegistered = true;

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		 <DocumentaryOverride>
            <DataVersion>1</DataVersion>
            <Purpose>
				<Code>AMD</Code>
			</Purpose>
         </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00005001</CoLoadBookingConfirmationReference>
	<AgentsReference>CAR</AgentsReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching QuotedBooking.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate QuotedBooking because:
[*Booking Request amendment message cannot be accepted once Booking is Canceled. Amendment rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestBookingHasStatusCancellationReceivedAndAmendmentReceived_MessageRejection()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.EBookingCancellationRequest;
			quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";
			quotedBooking1.Booking.JS_BookingReference = "CAR";
			quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			quotedBooking1.Booking.JS_IsForwardRegistered = true;

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		 <DocumentaryOverride>
            <DataVersion>1</DataVersion>
            <Purpose>
				<Code>AMD</Code>
			</Purpose>
         </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00005001</CoLoadBookingConfirmationReference>
	<AgentsReference>CAR</AgentsReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching QuotedBooking.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate QuotedBooking because:
[*Booking Request amendment message cannot be accepted once Booking Withdrawal/Cancellation request is in progress. Amendment rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestBookingHasStatusCancelledAndWithdrawalReceived_MessageRejection()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.BookingCancelled;
			quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";
			quotedBooking1.Booking.JS_BookingReference = "CAR";
			quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		 <DocumentaryOverride>
            <DataVersion>1</DataVersion>
            <Purpose>
				<Code>WTH</Code>
			</Purpose>
         </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00005001</CoLoadBookingConfirmationReference>
	<AgentsReference>CAR</AgentsReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching QuotedBooking.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate QuotedBooking because:
[*Booking Request Withdrawal/Cancellation message cannot be accepted once Booking is already Canceled. Withdrawal/Cancellation rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestBookingHasStatusCancellationReceivedAndWithdrawalReceived_MessageRejection()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.EBookingCancellationRequest;
			quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";
			quotedBooking1.Booking.JS_BookingReference = "CAR";
			quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		 <DocumentaryOverride>
            <DataVersion>1</DataVersion>
            <Purpose>
				<Code>WTH</Code>
			</Purpose>
         </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00005001</CoLoadBookingConfirmationReference>
	<AgentsReference>CAR</AgentsReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching QuotedBooking.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate QuotedBooking because:
[*Booking Request Withdrawal/Cancellation message cannot be accepted once Booking Withdrawal/Cancellation request is already in progress. Withdrawal/Cancellation rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestBookingHasBeenConvertedToShippmentForAmendmentMessage_WithElectronicBookingShipmentStatus()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
			quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";
			quotedBooking1.Booking.JS_BookingReference = "CAR";
			quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			quotedBooking1.Booking.JS_IsForwardRegistered = true;

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		 <DocumentaryOverride>
            <DataVersion>1</DataVersion>
            <Purpose>
				<Code>AMD</Code>
			</Purpose>
         </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00005001</CoLoadBookingConfirmationReference>
	<AgentsReference>CAR</AgentsReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching QuotedBooking.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate QuotedBooking because:
[*Booking Amendment message cannot be processed once Booking is converted to Shipment. Amendment rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		#region BookingHasBeenConvertedToShippmentForAmendmentMessage_AllowToUpdate

		public void TestBookingHasBeenConvertedToShippmentForAmendmentMessage_WithBookedShipmentStatus()
		{
			AssertBookingHasBeenConvertedToShippmentForAmendmentMessage_AllowToUpdate(ShipmentStatusList.Codes.Booked);
		}

		public void TestBookingHasBeenConvertedToShippmentForAmendmentMessage_WithAmendmentShipmentStatus()
		{
			AssertBookingHasBeenConvertedToShippmentForAmendmentMessage_AllowToUpdate(ShipmentStatusList.Codes.Amendment);
		}

		void AssertBookingHasBeenConvertedToShippmentForAmendmentMessage_AllowToUpdate(ZString shipmentStatus)
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var bookingParty = Factory.New<OrgHeader>();
				bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
				bookingParty.OH_Code = "BKGPARTY";
				bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

				var quotedBooking1 = CreateQuotedBooking();
				quotedBooking1.Booking.JS_ShipmentStatus = shipmentStatus;
				quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";
				quotedBooking1.Booking.JS_BookingReference = "CAR";
				quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
				quotedBooking1.Booking.JS_IsForwardRegistered = true;

				Factory.SaveForTesting();

				#region Message

				var message = GetQueuedUniversalShipmentMessage(
	@"<UniversalShipment>
  <Shipment>
	<DataContext>
		 <DocumentaryOverride>
            <DataVersion>1</DataVersion>
            <Purpose>
				<Code>AMD</Code>
				<Description>Amendment</Description>
			</Purpose>
         </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00005001</CoLoadBookingConfirmationReference>
	<AgentsReference>CAR</AgentsReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

				#endregion

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching QuotedBooking.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Populating QuotedBooking...
Updated Booking with Quote - Quote (00001000) - Booking (S00005001) from UniversalShipment.
Successfully saved Booking with Quote - Quote (00001000) - Booking (S00005001).
".Trim(), message.GetLogNoteText());

				var factory2 = new BusinessObjectFactory();
				var booking2 = factory2.Load<ForwardingShipment>(quotedBooking1.Booking.PK);
				var log = booking2.Logs.MostRecentLogByEventTime(Events.StatusUpdated);
				if (shipmentStatus == ShipmentStatusList.Codes.Amendment)
				{
					AssertNull(log);
				}
				else
				{
					CombineAssertions(() =>
					{
						AssertNotNull(log);
						AssertEquals("ShipmentStatus", ShipmentStatusList.Codes.Amendment, booking2.JS_ShipmentStatus);
						AssertEquals("Free text", "Amendment", log.ReferenceFreeText);
						AssertEquals("Event new status", ShipmentStatusList.Codes.Amendment, log.Parameters[Params.New]);
						AssertEquals("Event old status", shipmentStatus, log.Parameters[Params.Old]);
						AssertEquals("Event reason", "Booking Request Amendment Received", log.Parameters[Params.Reason]);
						AssertEquals("Event type", "Shipment Status", log.Parameters[Params.Type]);
					});
				}
			}
		}

		#endregion

		public void TestBookingIsUpdatedWhenAgentsReferenceIsMatchedForOriginalMessage()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
			quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";
			quotedBooking1.Booking.JS_BookingReference = "BOOKS";
			quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		 <DocumentaryOverride>
            <DataVersion>1</DataVersion>
            <Purpose>
				<Code>ORG</Code>
			</Purpose>
         </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00005001</CoLoadBookingConfirmationReference>
	<AgentsReference>BOOKS</AgentsReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching QuotedBooking.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Populating QuotedBooking...
Updated Booking with Quote - Quote (00001000) - Booking (S00005001) from UniversalShipment.
Successfully saved Booking with Quote - Quote (00001000) - Booking (S00005001).
".Trim(), message.GetLogNoteText());
		}

		public void TestBookingIsUpdated_WhenUniversalXMLUseCombinedReferenceAndPartyIDMatchIsEnabled()
		{
			using (eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var bookingParty = Factory.New<OrgHeader>();
				bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
				bookingParty.OH_Code = "BKGPARTY";
				bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

				var quotedBooking1 = CreateQuotedBooking();
				quotedBooking1.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
				quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";
				quotedBooking1.Booking.JS_BookingReference = "BOOKS";
				quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

				Factory.SaveForTesting();

				#region Message

				var message = GetQueuedUniversalShipmentMessage(
	@"<UniversalShipment>
  <Shipment>
	<DataContext>
		 <DocumentaryOverride>
            <DataVersion>1</DataVersion>
            <Purpose>
				<Code>ORG</Code>
			</Purpose>
         </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00005001</CoLoadBookingConfirmationReference>
	<AgentsReference>BOOKS</AgentsReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

				#endregion

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching QuotedBooking.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Populating QuotedBooking...
Updated Booking with Quote - Quote (00001000) - Booking (S00005001) from UniversalShipment.
Successfully saved Booking with Quote - Quote (00001000) - Booking (S00005001).
".Trim(), message.GetLogNoteText());
			}
		}

		public void TestJS_ShipmentStatusIsNotElectronicBookingOrBookedOrBookingRejected_ForOriginal()
		{
			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";

			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		<DocumentaryOverride>
            <DataVersion>1</DataVersion>
            <Purpose>
				<Code>ORG</Code>
				<Description>Original</Description>
			</Purpose>
        </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00005001</CoLoadBookingConfirmationReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching QuotedBooking.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate QuotedBooking because:
[*Forwarder Booking has already been processed.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestJS_ShipmentStatusIsNotElectronicBookingOrBookedOrBookingRejected_ForAmendment()
		{
			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";

			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		<DocumentaryOverride>
            <DataVersion>1</DataVersion>
            <Purpose>
				<Code>AMD</Code>
				<Description>Amendment</Description>
			</Purpose>
        </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00005001</CoLoadBookingConfirmationReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching QuotedBooking.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate QuotedBooking because:
[*Booking Request amendment message cannot be accepted once Shipping Instruction is processed. Amendment rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestJS_ShipmentStatusIsBookingRejected()
		{
			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.BookingRejected;
			quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";

			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";
			quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		<DocumentaryOverride>
            <DataVersion>1</DataVersion>
            <Purpose>
				<Code>ORG</Code>
				<Description>Original</Description>
			</Purpose>
         </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00005001</CoLoadBookingConfirmationReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching QuotedBooking.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Populating QuotedBooking...
Updated Booking with Quote - Quote (00001000) - Booking (S00005001) from UniversalShipment.
Successfully saved Booking with Quote - Quote (00001000) - Booking (S00005001).
".Trim(), message.GetLogNoteText());
		}

		public void TestBookingShippmentStatus_WithdrawMessage_BookingRejected()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.BookingRejected;
			quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";
			quotedBooking1.Booking.JS_BookingReference = "CAR";
			quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		 <DocumentaryOverride>
            <DataVersion>1</DataVersion>
            <Purpose>
				<Code>WTH</Code>
				<Description>Withdrawal</Description>
			</Purpose>
         </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00005001</CoLoadBookingConfirmationReference>
	<AgentsReference>CAR</AgentsReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching QuotedBooking.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate QuotedBooking because:
[*Booking Request Withdrawal cannot be processed once Booking is rejected. Booking Request Withdrawal rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestBookingShippmentStatus_WithdrawMessage_ElectronicShippingInstruction()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicShippingInstruction;
			quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";
			quotedBooking1.Booking.JS_BookingReference = "CAR";
			quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		 <DocumentaryOverride>
            <DataVersion>1</DataVersion>
            <Purpose>
				<Code>WTH</Code>
				<Description>Withdrawal</Description>
			</Purpose>
         </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00005001</CoLoadBookingConfirmationReference>
	<AgentsReference>CAR</AgentsReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching QuotedBooking.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate QuotedBooking because:
[*Booking Request Withdrawal cannot be processed once Shipping Instruction is received. Booking Request Withdrawal rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestBookingShippmentStatus_WithdrawMessage_RejectedWhenNoBookingMatched()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		 <DocumentaryOverride>
            <DataVersion>1</DataVersion>
            <Purpose>
				<Code>WTH</Code>
				<Description>Withdrawal</Description>
			</Purpose>
         </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00005001</CoLoadBookingConfirmationReference>
	<AgentsReference>CAR</AgentsReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate QuotedBooking because:
[*No booking matched for Booking Request Withdrawal message. Booking Request Withdrawal rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestBookingShippmentStatus_AmendmentMessage_RejectedWhenNoBookingMatched()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		 <DocumentaryOverride>
            <DataVersion>1</DataVersion>
            <Purpose>
				<Code>AMD</Code>
				<Description>Withdrawal</Description>
			</Purpose>
         </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00005001</CoLoadBookingConfirmationReference>
	<AgentsReference>CAR</AgentsReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate QuotedBooking because:
[*No booking matched for Booking Request Amendment message. Booking Request Amendment rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestBookingShippmentStatus_WithdrawMessage_Confirmed()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";
			quotedBooking1.Booking.JS_BookingReference = "CAR";
			quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		 <DocumentaryOverride>
            <DataVersion>1</DataVersion>
            <Purpose>
				<Code>WTH</Code>
				<Description>Withdrawal</Description>
			</Purpose>
         </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00005001</CoLoadBookingConfirmationReference>
	<AgentsReference>CAR</AgentsReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching QuotedBooking.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate QuotedBooking because:
[*Booking Request Withdrawal cannot be processed once Shipping Instruction is accepted. Booking Request Withdrawal rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestBookingShippmentStatus_WithdrawMessage_RejectedWhenConvertedAsForwardingShipment()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";
			quotedBooking1.Booking.JS_BookingReference = "CAR";
			quotedBooking1.Booking.JS_IsForwardRegistered = true;
			quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		 <DocumentaryOverride>
            <DataVersion>1</DataVersion>
            <Purpose>
				<Code>WTH</Code>
				<Description>Withdrawal</Description>
			</Purpose>
         </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00005001</CoLoadBookingConfirmationReference>
	<AgentsReference>CAR</AgentsReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching QuotedBooking.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate QuotedBooking because:
[*Booking Request Withdrawal message cannot be processed once Booking is converted to Shipment. Booking Request Withdrawal rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestBookingShippmentStatus_WithdrawMessage_SIRejected()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var quotedBooking = CreateQuotedBooking();
			quotedBooking.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.SIRejected;
			quotedBooking.Booking.JS_UniqueConsignRef = "S00005001";
			quotedBooking.Booking.JS_BookingReference = "CAR";
			quotedBooking.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		 <DocumentaryOverride>
            <DataVersion>1</DataVersion>
            <Purpose>
				<Code>WTH</Code>
				<Description>Withdrawal</Description>
			</Purpose>
         </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00005001</CoLoadBookingConfirmationReference>
	<AgentsReference>CAR</AgentsReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching QuotedBooking.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate QuotedBooking because:
[*Booking Request Withdrawal cannot be processed once Shipping Instruction is rejected. Booking Request Withdrawal rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		QuotedBooking CreateQuotedBooking()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory.BOFactory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory.BOFactory);

			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory.BOFactory);
			quotedBooking.ClientPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			quotedBooking.Mode = Core.Constants.RateMode.FCL;
			quotedBooking.TryLoadOrCreateJob();
			quotedBooking.Job.JH_GE = Env.CurrentDepartment.PK;

			return quotedBooking;
		}

		public void TestBookingPartyPKIsNotMatched()
		{
			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";

			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var bookingParty2 = Factory.New<OrgHeader>();
			bookingParty2.MainAddress.OA_Address1 = "Booking Party Address 2";
			bookingParty2.OH_Code = "PARTY2";
			bookingParty2.OH_FullName = "COMPANY 2";
			quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty2.MainAddress.PK;

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00005001</CoLoadBookingConfirmationReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
		<CompanyName>TESTCOMPANY</CompanyName>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching QuotedBooking.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate QuotedBooking because:
[*Booking Request message is received with a wrong Booking Party TESTCOMPANY. Booking rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestBookingPartyNameIsNotMatched()
		{
			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";

			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";
			bookingParty.MainAddress.OA_CompanyNameOverride = "BKG COMPANY";

			quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_AddressOverride = true;
			quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_CompanyName = "TEST Company";

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00005001</CoLoadBookingConfirmationReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<AddressOverride>true</AddressOverride>
		<CompanyName>BKG COMPANY</CompanyName>
		<Address1>Booking Party Address 1</Address1>
		<CompanyName>TESTCOMPANY</CompanyName>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' address 'Booking Party Address 1' with a score of 120.
Successfully loaded matching QuotedBooking.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' address 'Booking Party Address 1' with a score of 120.
Error - Cannot populate QuotedBooking because:
[*Booking Request message is received with a wrong Booking Party TESTCOMPANY. Booking rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestDoNotOverrideBookingPartyDuringImporting()
		{
			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";

			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";
			bookingParty.MainAddress.OA_CompanyNameOverride = "BKG COMPANY";
			bookingParty.MainAddress.OA_PostCode = "2000";
			quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00005001</CoLoadBookingConfirmationReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<AddressOverride>true</AddressOverride>
		<CompanyName>BKG COMPANY</CompanyName>
		<Address1>Booking Party Address 1</Address1>
		<Postcode>4000</Postcode>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' address 'Booking Party Address 1' with a score of 205.
Successfully loaded matching QuotedBooking.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' address 'Booking Party Address 1' with a score of 205.
Populating QuotedBooking...
Updated Booking with Quote - Quote (00001000) - Booking (S00005001) from UniversalShipment.
Successfully saved Booking with Quote - Quote (00001000) - Booking (S00005001).
".Trim(), message.GetLogNoteText());

			var factory2 = new BusinessObjectFactory();
			var booking2 = factory2.Load<ForwardingShipment>(quotedBooking1.Booking.PK);
			AssertNotNull(booking2);
			AssertEquals("Booking Party's post code should not be updated.", "2000", booking2.BookingPartyDocumentaryAddress.E2_Postcode);
		}

		public void TestBookingNumberMismatchesForOriginalMessage()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
			quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";
			quotedBooking1.Booking.JS_BookingReference = "CAR";
			quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		 <DocumentaryOverride>
            <DataVersion>1</DataVersion>
            <Purpose>
				<Code>ORG</Code>
			</Purpose>
         </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00006000</CoLoadBookingConfirmationReference>
	<AgentsReference>CAR</AgentsReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching QuotedBooking.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate QuotedBooking because:
[*Booking message with Booking Number S00006000 cannot find a matching Booking to update. Booking rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestUpdateBookingWhenBookingNumberIsEmptyForOriginalMessage()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
			quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";
			quotedBooking1.Booking.JS_BookingReference = "CAR";
			quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
  <Shipment>
	<DataContext>
		 <DocumentaryOverride>
            <DataVersion>1</DataVersion>
            <Purpose>
				<Code>ORG</Code>
			</Purpose>
         </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<AgentsReference>CAR</AgentsReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching QuotedBooking.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Populating QuotedBooking...
Updated Booking with Quote - Quote (00001000) - Booking (S00005001) from UniversalShipment.
Successfully saved Booking with Quote - Quote (00001000) - Booking (S00005001).
".Trim(), message.GetLogNoteText());
		}

		public void TestStatusUpdatedEventForOriginalMessage()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var bookingParty = Factory.New<OrgHeader>();
				bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
				bookingParty.OH_Code = "BKGPARTY";
				bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

				var quotedBooking1 = CreateQuotedBooking();
				quotedBooking1.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.BookingRejected;
				quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";
				quotedBooking1.Booking.JS_BookingReference = "CAR";
				quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

				Factory.SaveForTesting();

				#region Message

				var message = GetQueuedUniversalShipmentMessage(
	@"<UniversalShipment>
	  <Shipment>
		<DataContext>
			 <DocumentaryOverride>
				<DataVersion>1</DataVersion>
				<Purpose>
					<Code>ORG</Code>
					<Description>Original</Description>
				</Purpose>
			 </DocumentaryOverride>
			<RecipientRoleCollection>
				<RecipientRole>
					<Code>NVO</Code>
					<Description>NVOCC</Description>
					<ServiceCode>BRQ</ServiceCode>
				</RecipientRole>
			</RecipientRoleCollection>
		</DataContext>
		<AgentsReference>CAR</AgentsReference>
		<OrganizationAddressCollection>
		  <OrganizationAddress>
			<AddressType>BookingPartyDocumentaryAddress</AddressType>
			<OrganizationCode>BKGPARTY</OrganizationCode>
			<Address1>Booking Party Address 1</Address1>
		  </OrganizationAddress>
		</OrganizationAddressCollection>
	  </Shipment>
	</UniversalShipment>");

				#endregion

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching QuotedBooking.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Populating QuotedBooking...
Updated Booking with Quote - Quote (00001000) - Booking (S00005001) from UniversalShipment.
Successfully saved Booking with Quote - Quote (00001000) - Booking (S00005001).
".Trim(), message.GetLogNoteText());

				var factory2 = new BusinessObjectFactory();
				var booking2 = factory2.Load<ForwardingShipment>(quotedBooking1.Booking.PK);
				var log = booking2.Logs.MostRecentLogByEventTime(Events.StatusUpdated);
				CombineAssertions(() =>
				{
					AssertNotNull(log);
					AssertEquals("ShipmentStatus", ShipmentStatusList.Codes.ElectronicBooking, booking2.JS_ShipmentStatus);
					AssertEquals("Free text", "Original", log.ReferenceFreeText);
					AssertEquals("Event new status", ShipmentStatusList.Codes.ElectronicBooking, log.Parameters[Params.New]);
					AssertEquals("Event old status", ShipmentStatusList.Codes.BookingRejected, log.Parameters[Params.Old]);
					AssertEquals("Event reason", "Electronic Booking Request Received", log.Parameters[Params.Reason]);
					AssertEquals("Event type", "Shipment Status", log.Parameters[Params.Type]);
				});
			}
		}

		public void TestStatusUpdatedEventForAmendmentMessage()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var bookingParty = Factory.New<OrgHeader>();
				bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
				bookingParty.OH_Code = "BKGPARTY";
				bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

				var quotedBooking1 = CreateQuotedBooking();
				quotedBooking1.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.BookingRejected;
				quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";
				quotedBooking1.Booking.JS_BookingReference = "CAR";
				quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

				Factory.SaveForTesting();

				#region Message

				var message = GetQueuedUniversalShipmentMessage(
	@"<UniversalShipment>
  <Shipment>
	<DataContext>
		 <DocumentaryOverride>
            <DataVersion>1</DataVersion>
            <Purpose>
				<Code>AMD</Code>
				<Description>Amendment</Description>
			</Purpose>
         </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00005001</CoLoadBookingConfirmationReference>
	<AgentsReference>CAR</AgentsReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");

				#endregion

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching QuotedBooking.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Populating QuotedBooking...
Updated Booking with Quote - Quote (00001000) - Booking (S00005001) from UniversalShipment.
Successfully saved Booking with Quote - Quote (00001000) - Booking (S00005001).
".Trim(), message.GetLogNoteText());

				var factory2 = new BusinessObjectFactory();
				var booking2 = factory2.Load<ForwardingShipment>(quotedBooking1.Booking.PK);
				var log = booking2.Logs.MostRecentLogByEventTime(Events.StatusUpdated);
				CombineAssertions(() =>
				{
					AssertNotNull(log);
					AssertEquals("ShipmentStatus", ShipmentStatusList.Codes.ElectronicBooking, booking2.JS_ShipmentStatus);
					AssertEquals("Free text", "Amendment", log.ReferenceFreeText);
					AssertEquals("Event new status", ShipmentStatusList.Codes.ElectronicBooking, log.Parameters[Params.New]);
					AssertEquals("Event old status", ShipmentStatusList.Codes.BookingRejected, log.Parameters[Params.Old]);
					AssertEquals("Event reason", "Electronic Booking Request Received", log.Parameters[Params.Reason]);
					AssertEquals("Event type", "Shipment Status", log.Parameters[Params.Type]);
				});
			}
		}

		public void TestStatusUpdatedEventForWithdrawalMessage()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var bookingParty = Factory.New<OrgHeader>();
				bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
				bookingParty.OH_Code = "BKGPARTY";
				bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

				var quotedBooking1 = CreateQuotedBooking();
				quotedBooking1.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";
				quotedBooking1.Booking.JS_BookingReference = "CAR";
				quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

				Factory.SaveForTesting();

				#region Message

				var message = GetQueuedUniversalShipmentMessage(
	@"<UniversalShipment>
	  <Shipment>
		<DataContext>
			 <DocumentaryOverride>
				<DataVersion>1</DataVersion>
				<Purpose>
					<Code>WTH</Code>
					<Description>Withdrawal</Description>
				</Purpose>
			 </DocumentaryOverride>
			<RecipientRoleCollection>
				<RecipientRole>
					<Code>NVO</Code>
					<Description>NVOCC</Description>
					<ServiceCode>BRQ</ServiceCode>
				</RecipientRole>
			</RecipientRoleCollection>
		</DataContext>
		<CoLoadBookingConfirmationReference>S00005001</CoLoadBookingConfirmationReference>
		<AgentsReference>CAR</AgentsReference>
		<OrganizationAddressCollection>
		  <OrganizationAddress>
			<AddressType>BookingPartyDocumentaryAddress</AddressType>
			<OrganizationCode>BKGPARTY</OrganizationCode>
			<Address1>Booking Party Address 1</Address1>
		  </OrganizationAddress>
		</OrganizationAddressCollection>
	  </Shipment>
	</UniversalShipment>");

				#endregion

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching QuotedBooking.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Populating QuotedBooking...
Updated Booking with Quote - Quote (00001000) - Booking (S00005001) from UniversalShipment.
Successfully saved Booking with Quote - Quote (00001000) - Booking (S00005001).
".Trim(), message.GetLogNoteText());

				var factory2 = new BusinessObjectFactory();
				var booking2 = factory2.Load<ForwardingShipment>(quotedBooking1.Booking.PK);
				var log = booking2.Logs.MostRecentLogByEventTime(Events.StatusUpdated);
				CombineAssertions(() =>
				{
					AssertNotNull(log);
					AssertEquals("ShipmentStatus", ShipmentStatusList.Codes.EBookingCancellationRequest, booking2.JS_ShipmentStatus);
					AssertEquals("Free text", "Withdrawal", log.ReferenceFreeText);
					AssertEquals("Event new status", ShipmentStatusList.Codes.EBookingCancellationRequest, log.Parameters[Params.New]);
					AssertEquals("Event old status", ShipmentStatusList.Codes.Booked, log.Parameters[Params.Old]);
					AssertEquals("Event reason", "Electronic Booking Cancellation Request Received", log.Parameters[Params.Reason]);
					AssertEquals("Event type", "Shipment Status", log.Parameters[Params.Type]);
				});
			}
		}

		#endregion

		public void TestTransformWithEventTypeSBR()
		{
			var universalEvent = new UniversalEvent();
			var referenceContext = new Context() { Type = new ContextType { Type = "Reference" }, Value = "reference" };
			var c1cContext = new Context() { Type = new ContextType { Type = "CarrierC1CCode" }, Value = "C1CO" };
			universalEvent.ContextCollection = new List<Context> { referenceContext, c1cContext };

			var eventValue = new EventValue(Events.SubscriptionRequested);
			var result = new QuotedBookingDataContextManager().Transform(eventValue, universalEvent, null);

			AssertEquals("Shipment Visibility", result.Parameters[EventReferenceParameters.Codes.Type]);
			AssertEquals(null, result.Parameters[EventReferenceParameters.Codes.MessageType]);
			AssertEquals("reference", result.Parameters[EventReferenceParameters.Codes.InterchangeNumber]);
			AssertEquals("C1CO", result.Parameters[EventReferenceParameters.Codes.Organization]);
		}

		public void TestImportEventWithEventTypeIRJAndMessageTypeCO2e()
		{
			var quotedBooking = CreateQuotedBooking();
			quotedBooking.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			quotedBooking.Booking.JS_UniqueConsignRef = "S00005001";

			var sailing = Factory.NewWithValidTestData<JobSailing>();
			var jobVoyage = Factory.NewWithValidTestData<JobVoyage>();

			var origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "UAIEV";
			origin.JA_JV = jobVoyage.PK;

			var destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "CLESR";
			destination.JB_JV = jobVoyage.PK;

			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			jobVoyage.JV_RV_NKVessel = vessel.RV_FK;

			((ISailingChooserParent)quotedBooking).SailingJX = sailing.PK;

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(string.Format(UniversalEventWithEventTypeIRJAndMessageTypeCO2e, quotedBooking.Booking.JS_UniqueConsignRef));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals(CO2eStatusList.Codes.Rejected, quotedBooking.GetCO2eStatus());

			AssertEquals(1, quotedBooking.Logs.Find(log =>
			log.SL_SE_NKEvent == AutoEvents.GreenhouseGasEmissionsCalculationCode &&
			log.Parameters.TryGetValue(Params.Type, out var type) && type == nameof(CO2eEventType.Rejected) &&
			log.Parameters.TryGetValue(Params.Reason, out var reason) && reason == "ERROR! Something went wrong.").Count());

			AssertEquals(1, sailing.Logs.Find(log =>
			log.SL_SE_NKEvent == AutoEvents.GreenhouseGasEmissionsCalculationCode &&
			log.Parameters.TryGetValue(Params.Type, out var type) && type == nameof(CO2eEventType.Rejected) &&
			log.Parameters.TryGetValue(Params.Reason, out var reason) && reason == "ERROR! Something went wrong.").Count());
		}

		const string UniversalEventWithEventTypeIRJAndMessageTypeCO2e = @"
<UniversalEvent Version=""1.0"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingBooking</Type>
          <Key>{0}</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <EventTime>2020-02-02T20:20:22</EventTime>
    <EventType>IRJ</EventType>
    <EventParameters>
      <Department>WiseTech Global</Department>
      <MessageType>WTG Greenhouse Gas Emission</MessageType>
      <ReferenceNumber>C2300188272</ReferenceNumber>
    </EventParameters>
    <ContextCollection>
      <Context>
        <Type>FailureReason</Type>
        <Value>ERROR! Something went wrong.</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get
			{
				return @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment Version=""0.1"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Shipment Action=""MERGE"">
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingBooking</Type>
          <Key>S00001003</Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
        <Code>EDI</Code>
        <Name>Eagle Datamation International</Name>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <EventType>
        <Code>ATH</Code>
        <Description>Action Authorised</Description>
      </EventType>
      <ServerID>DAT</ServerID>
      <TriggerDate>2011-03-27T11:13:00</TriggerDate>
      <TriggerDescription>Test Trigger</TriggerDescription>
      <TriggerType>Trigger</TriggerType>
    </DataContext>

    <BookingConfirmationReference>FUL423189120</BookingConfirmationReference>
    <ContainerMode>
      <Code>LSE</Code>
      <Description>Loose</Description>
    </ContainerMode>
    <GoodsDescription>CHOCOLATE EGGS</GoodsDescription>
    <GoodsValue>49.9900</GoodsValue>
    <GoodsValueCurrency>
      <Code>AUD</Code>
      <Description>Australia, Dollars</Description>
    </GoodsValueCurrency>
    <IsBooking>true</IsBooking>
    <OuterPacks>1</OuterPacks>
    <OuterPacksPackageType>
      <Code>PLT</Code>
      <Description>Pallet</Description>
    </OuterPacksPackageType>
    <PortOfDestination>
      <Code>HKHKG</Code>
      <Name>Hong Kong</Name>
    </PortOfDestination>
    <PortOfOrigin>
      <Code>DEFRA</Code>
      <Name>Frankfurt am Main</Name>
    </PortOfOrigin>
    <ShipmentIncoTerm>
      <Code>FOB</Code>
      <Description>Free On Board</Description>
    </ShipmentIncoTerm>
    <TransportMode>
      <Code>AIR</Code>
      <Description>Air Freight</Description>
    </TransportMode>
    <WayBillNumber>FRED235478923</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>

    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type>
          <Code>AMS</Code>
          <Description>AMS Number</Description>
        </Type>
        <ReferenceNumber>CE00001</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>
  </Shipment>
</UniversalShipment>
";
			}
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes
		{
			get { return new RecipientRoleType[] { RecipientRoleType.NVO }; }
		}

		protected override ServiceCodeType?[] SupportedRecipientServices(RecipientRoleType recipientRole)
		{
			return SupportedRecipientRoleTypes.Contains(recipientRole) ? new ServiceCodeType?[] { ServiceCodeType.BRQ } : base.SupportedRecipientServices(recipientRole);
		}

		protected override QuotedBooking GetNewBusinessObjectForTesting()
		{
			return QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
		}

		protected override void MakeShipmentUsableForThisRole(RecipientRoleType recipientRoleType, UniversalShipment shipmentWithRecipientRole)
		{
			if (recipientRoleType == RecipientRoleType.NVO)
			{
				shipmentWithRecipientRole.DataContext.ClearDataSourceCollection();
				shipmentWithRecipientRole.DataContext.AddDataSource(DataContextType.ForwardingBooking, null);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;

		string UniversalShipment => resourceRetriever.Value.GetString("Enterprise.Freight.QuotedBookings.DataTransfer.Test.Universal.TestFiles.UniversalShipment.xml");

		string UniversalEvent => resourceRetriever.Value.GetString("Enterprise.Freight.QuotedBookings.DataTransfer.Test.Universal.TestFiles.UniversalEvent.xml");
	}
}
