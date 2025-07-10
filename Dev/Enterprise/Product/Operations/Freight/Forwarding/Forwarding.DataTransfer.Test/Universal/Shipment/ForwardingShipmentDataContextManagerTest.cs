using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.DocumentEngine;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.Testing;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.UniversalDataBuss.Matching.Testing;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static CargoWise.EventReference.Constants;
using Constants = Enterprise.Core.Constants;
using ContextTypes = Enterprise.UniversalDataBuss.DataObjects.Universal.Event.ContextTypes;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(ForwardingShipmentDataContextManager))]
	class ForwardingShipmentDataContextManagerTest : BaseShipmentDataContextManagerTest<ForwardingShipmentDataContextManager, ForwardingShipment>
	{
		#region Reference and Party ID Matching

		public void TestReferenceAndPartyIDMatchingOnBookingReferenceWithOverriddenConsignorWillMatchOnContentsOfUnmatchedOrgNote()
		{
			var unmatchedOrgPK = OrganizationAddressTestHelper.SetUseUnmatchedOrganisationForMatchingRegistry(true);
			eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataContext = shipment.DataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.ForwardingShipment, null);

			shipment.InterimReceiptNumber = "IRC24398724";
			shipment.GoodsDescription = "RAGING RABID ROGER RABBIT";

			var orgGenerator = new OrganisationTestHelper(Factory);

			var consignorDataObject = orgGenerator.CreateDataObject("JACK", "SCHMIDT", "7643");
			consignorDataObject.AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress);

			var consigneeDataObject = orgGenerator.CreateDataObject("PHIL", "MCKRACKIN", "2153");
			consigneeDataObject.AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { consignorDataObject, consigneeDataObject });

			Factory.SaveForTesting();

			AssertEquals("Precondition: Should be no Shipments in the DB yet.", null, new BusinessObjectFactory().LoadTop1<ForwardingShipment>(new ZQuery()));

			CombineAssertions(delegate
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(shipment);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment from UniversalShipment.
Successfully saved Shipment S00001000.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No Reference/Party ID matches were found in this module.
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Matching 'ConsignorDocumentaryAddress':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Matching 'ConsigneeDocumentaryAddress':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Added Shipment from UniversalShipment.
Successfully saved Shipment S00001000.
".Trim(), logNoteText);

				var shipments = new BusinessObjectFactory().Load<ForwardingShipment>(new ZQuery());
				AssertEquals("shipments.Count()", 1, shipments.Length);
				var shipmentBO = shipments[0];
				AssertEquals("shipmentBO.JS_InterimReceipt", "IRC24398724", shipmentBO.JS_InterimReceipt);
				AssertEquals("shipmentBO.JS_GoodsDescription", "RAGING RABID ROGER RABBIT", shipmentBO.JS_GoodsDescription);
				var docAddresses = shipmentBO.DocAddresses;
				var consignor = docAddresses.FindByDocAddressType(DocAddressType.ConsignorDocumentaryAddress);
				AssertEquals("consignor.E2_AddressOverride", false, consignor.E2_AddressOverride);
				var consignee = docAddresses.FindByDocAddressType(DocAddressType.ConsigneeDocumentaryAddress);
				AssertEquals("consignee.E2_AddressOverride", false, consignee.E2_AddressOverride);
			});

			shipment.GoodsDescription = "TASTES LIKE CHICKEN";

			CombineAssertions(delegate
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(shipment);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Updated Shipment S00001000 from UniversalShipment.
Successfully saved Shipment S00001000.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Matched to Shipment S00001000 with a Reference/Party ID match score of 90.
Successfully loaded matching ForwardingShipment.
Populating ForwardingShipment...
Matching 'ConsignorDocumentaryAddress':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Matching 'ConsigneeDocumentaryAddress':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Updated Shipment S00001000 from UniversalShipment.
Successfully saved Shipment S00001000.
".Trim(), logNoteText);

				var shipments = new BusinessObjectFactory().Load<ForwardingShipment>(new ZQuery());
				AssertEquals("shipments.Count()", 1, shipments.Length);
				var shipmentBO = shipments[0];
				AssertEquals("shipmentBO.JS_InterimReceipt", "IRC24398724", shipmentBO.JS_InterimReceipt);
				AssertEquals("shipmentBO.JS_GoodsDescription", "TASTES LIKE CHICKEN", shipmentBO.JS_GoodsDescription);
			});
		}

		public void TestReferenceAndPartyIDMatchingOnBookingReferenceWithOverriddenConsignorWillNotMatchOnUnmatchedOrgNoteWhereContentsAreDifferent()
		{
			var unmatchedOrgPK = OrganizationAddressTestHelper.SetUseUnmatchedOrganisationForMatchingRegistry(true);
			eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataContext = shipment.DataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.ForwardingShipment, null);

			shipment.InterimReceiptNumber = "IRC24398724";
			shipment.GoodsDescription = "RAGING RABID ROGER RABBIT";

			var orgGenerator = new OrganisationTestHelper(Factory);

			var consignorDataObject = orgGenerator.CreateDataObject("JACK", "SCHMIDT", "7643");
			consignorDataObject.AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress);

			var consigneeDataObject = orgGenerator.CreateDataObject("PHIL", "MCKRACKIN", "2153");
			consigneeDataObject.AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { consignorDataObject, consigneeDataObject });

			Factory.SaveForTesting();

			AssertEquals("Precondition: Should be no Shipments in the DB yet.", null, new BusinessObjectFactory().LoadTop1<ForwardingShipment>(new ZQuery()));

			CombineAssertions(delegate
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(shipment);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment from UniversalShipment.
Successfully saved Shipment S00001000.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No Reference/Party ID matches were found in this module.
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Matching 'ConsignorDocumentaryAddress':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Matching 'ConsigneeDocumentaryAddress':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Added Shipment from UniversalShipment.
Successfully saved Shipment S00001000.
".Trim(), logNoteText);

				var shipments = new BusinessObjectFactory().Load<ForwardingShipment>(new ZQuery());
				AssertEquals("shipments.Count()", 1, shipments.Length);
				var shipmentBO = shipments[0];
				AssertEquals("shipmentBO.JS_InterimReceipt", "IRC24398724", shipmentBO.JS_InterimReceipt);
				AssertEquals("shipmentBO.JS_GoodsDescription", "RAGING RABID ROGER RABBIT", shipmentBO.JS_GoodsDescription);
				var docAddresses = shipmentBO.DocAddresses;
				var consignor = docAddresses.FindByDocAddressType(DocAddressType.ConsignorDocumentaryAddress);
				AssertEquals("consignor.E2_AddressOverride", false, consignor.E2_AddressOverride);
				var consignee = docAddresses.FindByDocAddressType(DocAddressType.ConsigneeDocumentaryAddress);
				AssertEquals("consignee.E2_AddressOverride", false, consignee.E2_AddressOverride);
			});

			shipment.GoodsDescription = "TASTES LIKE CHICKEN";
			consignorDataObject.CompanyName = "SOMEONE ELSE";
			consigneeDataObject.Address1 = "COMPLETELY DIFFERENT";

			CombineAssertions(delegate
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(shipment);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment from UniversalShipment.
Successfully saved Shipment S00001001.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Best Reference/Party ID match is Shipment S00001000 with a score of 30, but the minimum match score is 90. Match failed.
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Matching 'ConsignorDocumentaryAddress':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Matching 'ConsigneeDocumentaryAddress':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Added Shipment from UniversalShipment.
Successfully saved Shipment S00001001.
".Trim(), logNoteText);

				var shipments = new BusinessObjectFactory().Load<ForwardingShipment>(new ZQuery());
				AssertEquals("shipments.Count()", 2, shipments.Length);
			});
		}

		public void TestReferenceAndPartyIDMatchingOnBookingReferenceWithOverriddenConsignorWillMatchOnContentsOfJobDocAddress()
		{
			eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataContext = shipment.DataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.ForwardingShipment, null);

			shipment.BookingConfirmationReference = "BM24398724";
			shipment.GoodsDescription = "RAGING RABID ROGER RABBIT";

			var orgGenerator = new OrganisationTestHelper(Factory);

			var consignorDataObject = orgGenerator.CreateDataObject("JACK", "SCHMIDT", "7643");
			consignorDataObject.AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress);
			consignorDataObject.AddressOverride = ZBool.True;

			var consigneeDataObject = orgGenerator.CreateDataObject("PHIL", "MCKRACKIN", "2153");
			consigneeDataObject.AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress);
			consigneeDataObject.AddressOverride = ZBool.True;

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { consignorDataObject, consigneeDataObject });

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
Added Shipment from UniversalShipment.
Successfully saved Shipment S00001000.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No Reference/Party ID matches were found in this module.
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: JACK SCHMIDT ENTERPRISES; Address 1: 643 JACK BOULEVARDE; City: SCHMIDTVILLE]'.
Warning - Matching 'ConsigneeDocumentaryAddress':- No match found for '[Company Name: PHIL MCKRACKIN ENTERPRISES; Address 1: 153 PHIL BOULEVARDE; City: MCKRACKINVILLE]'.
Added Shipment from UniversalShipment.
Successfully saved Shipment S00001000.
".Trim(), logNoteText);

				var shipments = new BusinessObjectFactory().Load<ForwardingShipment>(new ZQuery());
				AssertEquals("shipments.Count()", 1, shipments.Length);
				var shipmentBO = shipments[0];
				AssertEquals("shipmentBO.JS_BookingReference", "BM24398724", shipmentBO.JS_BookingReference);
				AssertEquals("shipmentBO.JS_GoodsDescription", "RAGING RABID ROGER RABBIT", shipmentBO.JS_GoodsDescription);
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
Updated Shipment S00001000 from UniversalShipment.
Successfully saved Shipment S00001000.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Matched to Shipment S00001000 with a Reference/Party ID match score of 90.
Successfully loaded matching ForwardingShipment.
Populating ForwardingShipment...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: JACK SCHMIDT ENTERPRISES; Address 1: 643 JACK BOULEVARDE; City: SCHMIDTVILLE]'.
Warning - Matching 'ConsigneeDocumentaryAddress':- No match found for '[Company Name: PHIL MCKRACKIN ENTERPRISES; Address 1: 153 PHIL BOULEVARDE; City: MCKRACKINVILLE]'.
Updated Shipment S00001000 from UniversalShipment.
Successfully saved Shipment S00001000.
".Trim(), logNoteText);

				var shipments = new BusinessObjectFactory().Load<ForwardingShipment>(new ZQuery());
				AssertEquals("shipments.Count()", 1, shipments.Length);
				var shipmentBO = shipments[0];
				AssertEquals("shipmentBO.JS_BookingReference", "BM24398724", shipmentBO.JS_BookingReference);
				AssertEquals("shipmentBO.JS_GoodsDescription", "TASTES LIKE CHICKEN", shipmentBO.JS_GoodsDescription);
			});
		}

		public void TestIncomingShipmentDoesNotMatchToExistingBookingViaBookingReference()
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
			});

			dataContext = shipment.DataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.ForwardingShipment, null);

			shipment.GoodsDescription = "GOLD, PURE GOLD.";

			CombineAssertions(delegate
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(shipment);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment from UniversalShipment.
Successfully saved Shipment S00001001.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No Reference/Party ID matches were found in this module.
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Matching 'ConsignorDocumentaryAddress':- Matched to 'JACSCHBNE' address '643 JACK BOULEVARDE' with a score of 560.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'PHIMCKSYD' address '153 PHIL BOULEVARDE' with a score of 560.
Added Shipment from UniversalShipment.
Successfully saved Shipment S00001001.
".Trim(), logNoteText);

				var shipments = new BusinessObjectFactory().Load<ForwardingShipment>(new ZQuery());
				AssertEquals("shipments.Count()", 2, shipments.Length);
			});
		}

		public void TestReferenceAndPartyIDMatchingOnBookingReference()
		{
			eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataContext = shipment.DataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.ForwardingShipment, null);

			shipment.BookingConfirmationReference = "BM24398724";
			shipment.GoodsDescription = "RAGING RABID ROGER RABBIT";

			var orgGenerator = new OrganisationTestHelper(Factory);

			var consignorDataObject = orgGenerator.CreateDataObject("JACK", "SCHMIDT", "7643");
			consignorDataObject.AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress);

			var consigneeDataObject = orgGenerator.CreateDataObject("PHIL", "MCKRACKIN", "2153");
			consigneeDataObject.AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { consignorDataObject, consigneeDataObject });

			var consignorBO = orgGenerator.CreateBusinessObject("JACK", "SCHMIDT", "7643");
			var consigneeBO = orgGenerator.CreateBusinessObject("PHIL", "MCKRACKIN", "2153");
			Factory.SaveForTesting();

			AssertEquals("Precondition: Should be no Shipments in the DB yet.", null, new BusinessObjectFactory().LoadTop1<ForwardingShipment>(new ZQuery()));

			CombineAssertions(delegate
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(shipment);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment from UniversalShipment.
Successfully saved Shipment S00001000.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No Reference/Party ID matches were found in this module.
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Matching 'ConsignorDocumentaryAddress':- Matched to 'JACSCHBNE' address '643 JACK BOULEVARDE' with a score of 560.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'PHIMCKSYD' address '153 PHIL BOULEVARDE' with a score of 560.
Added Shipment from UniversalShipment.
Successfully saved Shipment S00001000.
".Trim(), logNoteText);

				var shipments = new BusinessObjectFactory().Load<ForwardingShipment>(new ZQuery());
				AssertEquals("shipments.Count()", 1, shipments.Length);
				var shipmentBO = shipments[0];
				AssertEquals("shipmentBO.JS_BookingReference", "BM24398724", shipmentBO.JS_BookingReference);
				AssertEquals("shipmentBO.JS_GoodsDescription", "RAGING RABID ROGER RABBIT", shipmentBO.JS_GoodsDescription);
			});

			shipment.GoodsDescription = "TASTES LIKE CHICKEN";

			CombineAssertions(delegate
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(shipment);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Updated Shipment S00001000 from UniversalShipment.
Successfully saved Shipment S00001000.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Matched to Shipment S00001000 with a Reference/Party ID match score of 90.
Successfully loaded matching ForwardingShipment.
Populating ForwardingShipment...
Matching 'ConsignorDocumentaryAddress':- Matched to 'JACSCHBNE' address '643 JACK BOULEVARDE' with a score of 560.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'PHIMCKSYD' address '153 PHIL BOULEVARDE' with a score of 560.
Updated Shipment S00001000 from UniversalShipment.
Successfully saved Shipment S00001000.
".Trim(), logNoteText);

				var shipments = new BusinessObjectFactory().Load<ForwardingShipment>(new ZQuery());
				AssertEquals("shipments.Count()", 1, shipments.Length);
				var shipmentBO = shipments[0];
				AssertEquals("shipmentBO.JS_BookingReference", "BM24398724", shipmentBO.JS_BookingReference);
				AssertEquals("shipmentBO.JS_GoodsDescription", "TASTES LIKE CHICKEN", shipmentBO.JS_GoodsDescription);
			});
		}

		public void TestReferenceAndPartyIDMatchingOnInterimReceiptNo()
		{
			eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataContext = shipment.DataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.ForwardingShipment, null);

			shipment.InterimReceiptNumber = "IRN111-1321U2";
			shipment.GoodsDescription = "INCONTINENTIA FELICIMA";

			var orgGenerator = new OrganisationTestHelper(Factory);

			var consignorDataObject = orgGenerator.CreateDataObject("JACK", "SCHMIDT", "7643");
			consignorDataObject.AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress);

			var consigneeDataObject = orgGenerator.CreateDataObject("PHIL", "MCKRACKIN", "2153");
			consigneeDataObject.AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { consignorDataObject, consigneeDataObject });

			var consignorBO = orgGenerator.CreateBusinessObject("JACK", "SCHMIDT", "7643");
			var consigneeBO = orgGenerator.CreateBusinessObject("PHIL", "MCKRACKIN", "2153");
			Factory.SaveForTesting();

			AssertEquals("Precondition: Should be no Shipments in the DB yet.", null, new BusinessObjectFactory().LoadTop1<ForwardingShipment>(new ZQuery()));

			CombineAssertions(delegate
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(shipment);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment from UniversalShipment.
Successfully saved Shipment S00001000.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No Reference/Party ID matches were found in this module.
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Matching 'ConsignorDocumentaryAddress':- Matched to 'JACSCHBNE' address '643 JACK BOULEVARDE' with a score of 560.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'PHIMCKSYD' address '153 PHIL BOULEVARDE' with a score of 560.
Added Shipment from UniversalShipment.
Successfully saved Shipment S00001000.
".Trim(), logNoteText);

				var shipments = new BusinessObjectFactory().Load<ForwardingShipment>(new ZQuery());
				AssertEquals("shipments.Count()", 1, shipments.Length);
				var shipmentBO = shipments[0];
				AssertEquals("shipmentBO.JS_InterimReceipt", "IRN111-1321U2", shipmentBO.JS_InterimReceipt);
				AssertEquals("shipmentBO.JS_GoodsDescription", "INCONTINENTIA FELICIMA", shipmentBO.JS_GoodsDescription);
			});

			shipment.GoodsDescription = "TASTES NOTHING LIKE CHICKEN";

			CombineAssertions(delegate
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(shipment);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Updated Shipment S00001000 from UniversalShipment.
Successfully saved Shipment S00001000.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Matched to Shipment S00001000 with a Reference/Party ID match score of 90.
Successfully loaded matching ForwardingShipment.
Populating ForwardingShipment...
Matching 'ConsignorDocumentaryAddress':- Matched to 'JACSCHBNE' address '643 JACK BOULEVARDE' with a score of 560.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'PHIMCKSYD' address '153 PHIL BOULEVARDE' with a score of 560.
Updated Shipment S00001000 from UniversalShipment.
Successfully saved Shipment S00001000.
".Trim(), logNoteText);

				var shipments = new BusinessObjectFactory().Load<ForwardingShipment>(new ZQuery());
				AssertEquals("shipments.Count()", 1, shipments.Length);
				var shipmentBO = shipments[0];
				AssertEquals("shipmentBO.JS_InterimReceipt", "IRN111-1321U2", shipmentBO.JS_InterimReceipt);
				AssertEquals("shipmentBO.JS_GoodsDescription", "TASTES NOTHING LIKE CHICKEN", shipmentBO.JS_GoodsDescription);
			});
		}

		public void TestReferenceAndPartyIDMatchingOnHouseBill()
		{
			eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataContext = shipment.DataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.ForwardingShipment, null);

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
Added Shipment (House Bill='HBL05387944') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='HBL05387944').
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No Reference/Party ID matches were found in this module.
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Matching 'ConsignorDocumentaryAddress':- Matched to 'JACSCHBNE' address '643 JACK BOULEVARDE' with a score of 560.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'PHIMCKSYD' address '153 PHIL BOULEVARDE' with a score of 560.
Matching 'SendingForwarderAddress':- Matched to 'SILCHUXRH' address '792 SILVERWATER BOULEVARD' with a score of 560.
Warning - Unknown Address Type [SendingForwarderAddress] found. Job Document Address not imported.
Added Shipment (House Bill='HBL05387944') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='HBL05387944').
".Trim(), logNoteText);

				var shipments = new BusinessObjectFactory().Load<ForwardingShipment>(new ZQuery());
				AssertEquals("shipments.Count()", 1, shipments.Length);
				var shipmentBO = shipments[0];
				AssertEquals("shipmentBO.JS_HouseBill", "HBL05387944", shipmentBO.JS_HouseBill);
				AssertEquals("shipmentBO.JS_GoodsDescription", "RAGING RABID ROGER RABBIT", shipmentBO.JS_GoodsDescription);
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
Updated Shipment S00001000 (House Bill='HBL05387944') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='HBL05387944').
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Matched to Shipment S00001000 (House Bill='HBL05387944') with a Reference/Party ID match score of 180.
Successfully loaded matching ForwardingShipment.
Populating ForwardingShipment...
Matching 'ConsignorDocumentaryAddress':- Matched to 'JACSCHBNE' address '643 JACK BOULEVARDE' with a score of 560.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'PHIMCKSYD' address '153 PHIL BOULEVARDE' with a score of 560.
Matching 'SendingForwarderAddress':- Matched to 'SILCHUXRH' address '792 SILVERWATER BOULEVARD' with a score of 560.
Warning - Unknown Address Type [SendingForwarderAddress] found. Job Document Address not imported.
Updated Shipment S00001000 (House Bill='HBL05387944') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='HBL05387944').
".Trim(), logNoteText);

				var shipments = new BusinessObjectFactory().Load<ForwardingShipment>(new ZQuery());
				AssertEquals("shipments.Count()", 1, shipments.Length);
				var shipmentBO = shipments[0];
				AssertEquals("shipmentBO.JS_HouseBill", "HBL05387944", shipmentBO.JS_HouseBill);
				AssertEquals("shipmentBO.JS_GoodsDescription", "TASTES LIKE CHICKEN", shipmentBO.JS_GoodsDescription);
			});
		}

		#endregion

		public void TestTransformWithEventTypeSBR()
		{
			var universalEvent = new UniversalEvent();
			var referenceContext = new Context() { Type = new ContextType { Type = "Reference" }, Value = "reference" };
			var c1cContext = new Context() { Type = new ContextType { Type = "CarrierC1CCode" }, Value = "C1CO" };
			universalEvent.ContextCollection = new List<Context> { referenceContext, c1cContext };

			var eventValue = new EventValue(Events.SubscriptionRequested);
			var result = new ForwardingShipmentDataContextManager().Transform(eventValue, universalEvent, null);

			AssertEquals("Shipment Visibility", result.Parameters[EventReferenceParameters.Codes.Type]);
			AssertEquals(null, result.Parameters[EventReferenceParameters.Codes.MessageType]);
			AssertEquals("reference", result.Parameters[EventReferenceParameters.Codes.InterchangeNumber]);
			AssertEquals("C1CO", result.Parameters[EventReferenceParameters.Codes.Organization]);
		}

		public void TestDeclarationDataObjectWriterUsedWhenBrokeragePrimarySourceSelected()
		{
			var filter = Factory.New<EDIMessageContentFilter>();
			filter.ECF_Name = "ABC";
			filter.UniversalShipment.AdditionalConfiguration.PrimaryDataSource = EDIMessageContentPrimaryDataSource.Codes.Brokerage;

			var purpose = Factory.New<EDIMessagePurpose>();
			purpose.EMP_Code = "FFF";
			purpose.EMP_Description = "FFF";
			purpose.EMP_ECF_Filter = filter.PK;

			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			Factory.SaveForTesting();

			var manager = shipment.GetUniversalDataContextManager() as IShipmentDataContextManager;
			var actionInfo = new ActionInfo(RecipientRoleType.ORP, shipment);
			actionInfo.PurposeCode = purpose.EMP_Code;
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(actionInfo));

			Assert(writer is DeclarationDataObjectWriter);
		}

		public void TestUniversalShipmentWithBranchGetsAddedWithCorrectBranchSelected()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "ZXR";
			company.GC_Name = "Zanzibar XR";

			var branch1 = company.Branches.AddNew();
			branch1.GB_Code = "ZX1";
			branch1.GB_BranchName = "Zanzibar X-1";

			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "ZX2";
			branch2.GB_BranchName = "Zanzibar X-2";

			Factory.SaveForTesting();

			var shipmentNoSetting = new BillOfLadingNumberCustomisation();

			shipmentNoSetting.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode].Include = true;
			shipmentNoSetting.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.BranchCode].Include = true;

			FreightDataRegistry.Instance.HouseBillShipmentNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, shipmentNoSetting);

			var message = GetQueuedUniversalShipmentMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalShipmentWithBranch.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log - Shipment Number should have right branch prefix", @"
Added Shipment (House Bill='FIGGIN327893') from UniversalShipment.
Successfully saved Shipment SZXRZX200001000 (House Bill='FIGGIN327893').
".Trim(), serviceTaskLog.ToString());
			});
		}

		public void TestUseUnmatchedOrganisationForMatchingFunctionalityWorks()
		{
			var unmatchedOrgPK = OrganizationAddressTestHelper.SetUseUnmatchedOrganisationForMatchingRegistry(true);
			var message = GetQueuedUniversalShipmentMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalShipmentWithUnmatchedOrganisations.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='I_DO_NOT_EXIST') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='I_DO_NOT_EXIST').
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Matching 'ConsignorDocumentaryAddress':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Matching 'ConsigneeDocumentaryAddress':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Matching 'LocalClient':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Added Shipment (House Bill='I_DO_NOT_EXIST') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='I_DO_NOT_EXIST').
".Trim(), logNoteText);
			});

			var reloadedShipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, "S00001000"));

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

			var jobHeader = reloadedShipment.ShipmentJobHeader;
			AssertNotNull("shipment.JobHeader", jobHeader);

			AssertEquals("Local Charges (Client) on JobHeader", unmatchedAddressPK, jobHeader.JH_OA_LocalChargesAddr);
		}

		public void TestNOTUsingUnmatchedOrganisationForMatchingFunctionalityWorks()
		{
			var unmatchedOrgPK = OrganizationAddressTestHelper.SetUseUnmatchedOrganisationForMatchingRegistry(false);
			var message = GetQueuedUniversalShipmentMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalShipmentWithUnmatchedOrganisations.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='I_DO_NOT_EXIST') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='I_DO_NOT_EXIST').
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: VAPOUR CORPORATION; Address 1: UNIT 0, -1 FANTASY LANE; City: FAKE HILL]'.
Warning - Matching 'ConsigneeDocumentaryAddress':- No match found for '[Company Name: TERRY TOWELLERS INC; Address 1: 238 APTITUDE PLAZA; Address 2: FANTASY VALLEY BUSINESS CENTRE; City: FANTASY VALLEY]'.
Warning - Matching 'LocalClient':- No match found for '[Company Name: TERRY TOWELLERS INC; Address 1: 238 APTITUDE PLAZA; Address 2: FANTASY VALLEY BUSINESS CENTRE; City: FANTASY VALLEY]'.
Added Shipment (House Bill='I_DO_NOT_EXIST') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='I_DO_NOT_EXIST').
".Trim(), logNoteText);
			});

			var reloadedShipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, "S00001000"));

			AssertEquals("reloadedShipment.JS_GoodsDescription", "HATS", reloadedShipment.JS_GoodsDescription);

			var unmatchedOrgNotes = reloadedShipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
			AssertEquals("unmatchedOrgNotes.Length", 0, unmatchedOrgNotes.Length);

			var miscOrgAddressPK = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.PrimaryOfficeAddress;

			CombineAssertions("shipment.Consignor", delegate
			{
				var consignor = reloadedShipment.ConsignorDocumentaryAddress;
				AssertEquals("E2_AddressOverride", true, consignor.E2_AddressOverride);
				AssertEquals("E2_CompanyName", "VAPOUR CORPORATION", consignor.E2_CompanyName);
			});

			CombineAssertions("shipment.Consignee", delegate
			{
				var consignee = reloadedShipment.ConsigneeDocumentaryAddress;
				AssertEquals("E2_AddressOverride", true, consignee.E2_AddressOverride);
				AssertEquals("E2_CompanyName", "TERRY TOWELLERS INC", consignee.E2_CompanyName);
			});

			var jobHeader = reloadedShipment.ShipmentJobHeader;
			AssertNull("shipment.JobHeader should be null if there is no client matched to store on the JobHeader.", jobHeader);
		}

		public void TestCreatesNewShipmentIfAdditionalReferencesAreCorrectButHasNonMatchingEnterpriseAndServerID()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001010";

			var additionalReference1 = shipment.Numbers.AddNew();
			additionalReference1.CE_EntryNum = "CE00001";
			additionalReference1.CE_EntryType = "AMS";
			additionalReference1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalShipmentWithDifferentIDs.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='FRED235478923') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='FRED235478923') with 1 x CusEntryNumber.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
No matching CusEntryNumber found, creating new CusEntryNumber.
Populating CusEntryNumber...
Added Shipment (House Bill='FRED235478923') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='FRED235478923') with 1 x CusEntryNumber.
".Trim(), logNoteText);
			});
		}

		public void TestImportShipmentThroughAdditionalReferencesIfEnterpriseAndServerIDMatch()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001010";

			var additionalReference1 = shipment.Numbers.AddNew();
			additionalReference1.CE_EntryNum = "CE00001";
			additionalReference1.CE_EntryType = "AMS";
			additionalReference1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalShipment.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Updated Shipment S00001010 (House Bill='FRED235478923') from UniversalShipment.
Successfully saved Shipment S00001010 (House Bill='FRED235478923') with 1 x CusEntryNumber.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Successfully loaded matching ForwardingShipment.
Populating ForwardingShipment...
Matching 'ConsignorDocumentaryAddress':- Matched to 'ABABEU' by code, address 'PST: DIESLSTR 11' with a score of 240.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'AASDRA' by code, address 'PST: UNIT A1, 4TH FLOOR,' with a score of 240.
Successfully loaded matching CusEntryNumber.
Populating CusEntryNumber...
Updated Shipment S00001010 (House Bill='FRED235478923') from UniversalShipment.
Successfully saved Shipment S00001010 (House Bill='FRED235478923') with 1 x CusEntryNumber.
".Trim(), logNoteText);
			});
		}

		public void TestDoesNotLinkEventOnAdditionalReferencesIfNonMatchingEnterpriseAndServerID()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001010";

			var additionalReference1 = shipment.Numbers.AddNew();
			additionalReference1.CE_EntryNum = "134FREGT";
			additionalReference1.CE_EntryType = "AMS";
			additionalReference1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalEventWithDifferentIDs.xml")));

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

				var logs = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AuthorisedCode));
				AssertEquals("[ATH] - Action Authorized event count", 0, logs.Length);
			});
		}

		[TestDate(2024, 10, 31, 9, 32, 21)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestShipmentPopulateOriginalBillNotes_Created()
		{
			TestDateAttribute.UseUNLOCO = true;
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001001";

			var uEvent = GetQueuedUniversalEventMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalEventBLU.xml")), createInterchange: true);
			var uShipment = GetQueuedUniversalShipmentMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalShipmentBLU.xml")));
			uShipment.EM_EI = uEvent.EM_EI;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "WiseTech";
			AssertNotNull("orgHeader.MainAddress", orgHeader.MainAddress);
			orgHeader.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			orgHeader.MainAddress.Address1 = "Address1";
			orgHeader.MainAddress.Address2 = "Address2";
			orgHeader.MainAddress.City = "City";
			orgHeader.MainAddress.State = "State";
			orgHeader.MainAddress.Postcode = "Postcode";

			var arAddress = orgHeader.Addresses.AddNew(OrgAddressType.Receivables, true);
			arAddress.OA_RL_NKRelatedPortCode = "CNSHG";
			arAddress.Address1 = "Jianye";
			arAddress.Address2 = "Huaxin";
			arAddress.City = "Nanjing";
			arAddress.State = "JiangSu";
			arAddress.Postcode = "o.O";

			var triCusCode1 = orgHeader.CustomsCodes.AddNew();
			triCusCode1.OK_CodeType = OrgCusCode.CodeTypes.BoleroTitleRegisterID;
			triCusCode1.OK_CustomsRegNo = "SH1234567890123455";

			var triCusCode2 = orgHeader.CustomsCodes.AddNew();
			triCusCode2.OK_CodeType = OrgCusCode.CodeTypes.BoleroTitleRegisterID;
			triCusCode2.OK_CustomsRegNo = "SH1234567890123456";
			triCusCode2.OK_OA_PremisesAddress = arAddress.PK;

			Factory.SaveForTesting();

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration() { EnableEBLIntegration = true, GalileoEndPointUrl = "http://test.test", GalileoAudience = Guid.NewGuid().ToString(), GalileoTestEndPointUrl = "http://test.test", GalileoTestAudience = Guid.NewGuid().ToString() }))
			{
				var originalBillNote = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.OriginalBillNotes.ToString()).FirstOrDefault();

				AssertNull("Should be null.", originalBillNote);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(uEvent);
				originalBillNote = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.OriginalBillNotes.ToString()).FirstOrDefault();

				AssertNotNull("Should not be null.", originalBillNote);
				AssertEquals(@"31-Oct-24 09:32:21 +00:00 WayBill pd-230619-ebl Amendment Granted
Publisher : WiseTech, Jianye, Huaxin, Nanjing Jiangsu o.O, China
Current Holder : WiseTech, Address1, Address2, City State Postcode, Australia
Consignee : YOUR CHINA COMPANY(SHANG HAI) sdfdsf, ROOM 1501, BUILDING 1, SHENGBANG INTERNATIONAL, BUILDING, 1318 NORTH SICHUAN ROAD,HONGKOU DISTRICT, SHANGHAI CITY 31 200080, China", originalBillNote.ST_NoteText);
			}
		}

		[TestDate(2024, 8, 7, 13, 20, 30)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestShipmentPopulateOriginalBillNotes_Updated()
		{
			TestDateAttribute.UseUNLOCO = true;
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001001";
			var originalBillNote = shipment.Notes.AddNew();
			originalBillNote.ST_ParentID = shipment.PK;
			originalBillNote.ST_Table = JobShipmentSchema.Constants.TableName;
			originalBillNote.ST_Description = PredefinedNoteTypes.Instance.OriginalBillNotes.ToString();
			originalBillNote.ST_NoteType = nameof(CargoWise.Definitions.StmNoteVisibility.INT);
			originalBillNote.ST_NoteText = @"08-Aug-24 10:00:00 +10:00 WayBill 123456 Amendment Granted

06-Aug-24 10:00:00 +10:00 WayBill 123456 Amendment Granted";

			var uEvent = GetQueuedUniversalEventMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalEventBLU.xml")), createInterchange: true);
			var uShipment = GetQueuedUniversalShipmentMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalShipmentBLU.xml")));
			uShipment.EM_EI = uEvent.EM_EI;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "WiseTech";
			AssertNotNull("orgHeader.MainAddress", orgHeader.MainAddress);
			orgHeader.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			orgHeader.MainAddress.Address1 = "Address1";
			orgHeader.MainAddress.Address2 = "Address2";
			orgHeader.MainAddress.City = "City";
			orgHeader.MainAddress.State = "State";
			orgHeader.MainAddress.Postcode = "Postcode";

			var arAddress = orgHeader.Addresses.AddNew(OrgAddressType.Receivables, true);
			arAddress.OA_RL_NKRelatedPortCode = "CNSHG";
			arAddress.Address1 = "Jianye";
			arAddress.Address2 = "Huaxin";
			arAddress.City = "Nanjing";
			arAddress.State = "JiangSu";
			arAddress.Postcode = "o.O";

			var triCusCode1 = orgHeader.CustomsCodes.AddNew();
			triCusCode1.OK_CodeType = OrgCusCode.CodeTypes.BoleroTitleRegisterID;
			triCusCode1.OK_CustomsRegNo = "SH1234567890123455";

			var triCusCode2 = orgHeader.CustomsCodes.AddNew();
			triCusCode2.OK_CodeType = OrgCusCode.CodeTypes.BoleroTitleRegisterID;
			triCusCode2.OK_CustomsRegNo = "SH1234567890123456";
			triCusCode2.OK_OA_PremisesAddress = arAddress.PK;

			Factory.SaveForTesting();

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration() { EnableEBLIntegration = true, GalileoEndPointUrl = "http://test.test", GalileoAudience = Guid.NewGuid().ToString(), GalileoTestEndPointUrl = "http://test.test", GalileoTestAudience = Guid.NewGuid().ToString() }))
			{
				originalBillNote = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.OriginalBillNotes.ToString()).FirstOrDefault();

				AssertNotNull("Should not be null.", originalBillNote);
				AssertEquals(@"08-Aug-24 10:00:00 +10:00 WayBill 123456 Amendment Granted

06-Aug-24 10:00:00 +10:00 WayBill 123456 Amendment Granted", originalBillNote.ST_NoteText);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(uEvent);
				originalBillNote = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.OriginalBillNotes.ToString()).FirstOrDefault();

				AssertNotNull("Should not be null.", originalBillNote);
				AssertEquals(@"07-Aug-24 13:20:30 +00:00 WayBill pd-230619-ebl Amendment Granted
Publisher : WiseTech, Jianye, Huaxin, Nanjing Jiangsu o.O, China
Current Holder : WiseTech, Address1, Address2, City State Postcode, Australia
Consignee : YOUR CHINA COMPANY(SHANG HAI) sdfdsf, ROOM 1501, BUILDING 1, SHENGBANG INTERNATIONAL, BUILDING, 1318 NORTH SICHUAN ROAD,HONGKOU DISTRICT, SHANGHAI CITY 31 200080, China

08-Aug-24 10:00:00 +10:00 WayBill 123456 Amendment Granted

06-Aug-24 10:00:00 +10:00 WayBill 123456 Amendment Granted", originalBillNote.ST_NoteText);
			}
		}

		public void TestLinkEventOnAdditionalReferencesIfEnterpriseAndServerIDMatch()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001010";

			var additionalReference1 = shipment.Numbers.AddNew();
			additionalReference1.CE_EntryNum = "134FREGT";
			additionalReference1.CE_EntryType = "AMS";
			additionalReference1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalEvent.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Shipment S00001010.
				".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Shipment S00001010.
				".Trim(), message.GetLogNoteText());

				var logs = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AuthorisedCode));
				AssertEquals("[ATH] - Action Authorized event count", 1, logs.Length);
			});
		}

		public void TestImportEventViaUniversalDataBussMatchesToShipmentIfShipmentLevelReferencesArePresent()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001011";
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_HouseBill = "FRED235478923";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "10137654321";
			consol.JK_UniqueConsignRef = "C00001010";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalEvent.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Shipment S00001011 (House Bill='FRED235478923').
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Shipment S00001011 (House Bill='FRED235478923').
".Trim(), message.GetLogNoteText());

				shipment.Reload();
				var logs = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AuthorisedCode));
				AssertEquals("[ATH] - Action Authorised event count", 1, logs.Length);
				var log = logs[0];

				var contextItems = log.SourceInfoItems;
				var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select((item) => item.Key + " - " + item.Data).ToArray());
				AssertEquals("Context Items on Event", @"
MAWB Number - 10137654321
HAWB Number - FRED235478923
HAWB Origin IATA Airport Code - FRA
HAWB Destination IATA Airport Code - HKG
Carriers Booking Reference - 20257654321
Shippers Reference - FUL423189120
Interim Receipt - INTERIM
AMS Number - 134FREGT
COC - 267AIRGT
Data Source Company - EDI - Eagle Datamation International
Data Source Enterprise ID - EDI
Data Source Server ID - DAT
".Trim(), actualContextItems);
			});
		}

		public void TestImportEventViaUniversalDataBussIgnoresMatchingConsolIfDataTargetIsSpecified()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "10137654321";
			consol.JK_UniqueConsignRef = "C00001010";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalEventWithDataTarget.xml")));

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

				consol.Reload();
				var logs = consol.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AuthorisedCode));
				AssertEquals("[ATH] - Action Authorised event count", 0, logs.Length);
			});
		}

		public void TestImportEventViaUniversalDataBussWithBothConsolAndShipmentReferencesAndNoDataTarget()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001010";
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_HouseBill = "FRED235478923";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_BookingReference = "20257654321";
			consol.JK_UniqueConsignRef = "C00001010";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalEvent.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Shipment S00001010 (House Bill='FRED235478923').
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Shipment S00001010 (House Bill='FRED235478923').
".Trim(), message.GetLogNoteText());

				shipment.Reload();
				var logs = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AuthorisedCode));
				AssertEquals("[ATH] - Action Authorised event count", 1, logs.Length);
				var log = logs[0];

				var contextItems = log.SourceInfoItems;
				var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select((item) => item.Key + " - " + item.Data).ToArray());
				AssertEquals("Context Items on Event", @"
MAWB Number - 10137654321
HAWB Number - FRED235478923
HAWB Origin IATA Airport Code - FRA
HAWB Destination IATA Airport Code - HKG
Carriers Booking Reference - 20257654321
Shippers Reference - FUL423189120
Interim Receipt - INTERIM
AMS Number - 134FREGT
COC - 267AIRGT
Data Source Company - EDI - Eagle Datamation International
Data Source Enterprise ID - EDI
Data Source Server ID - DAT
".Trim(), actualContextItems);
			});
		}

		public void TestImportEventViaUniversalDataBussWithBothConsolAndShipmentReferencesAndNoDataTarget_GetKeys()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001010";
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_HouseBill = "FRED235478923";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_BookingReference = "20257654321";
			consol.JK_UniqueConsignRef = "C00001010";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalEvent.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			AssertContainsExactElementsInAnyOrder(new[] { shipment.PK.ToStringKey(), "FUL423189120", "10137654321", "FRED235478923", "20257654321", "CCUSAMS134FREGT", "CCUSCOC267AIRGT" }, manager.GetKeysForBlockingParallelImport(message).Keys);
		}

		public void TestAdditionalReferencesAreNotDuplicated()
		{
			var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Constants.TransportModes.Air;
			shipmentBO.JS_BookingReference = "FUL423189120";
			shipmentBO.JS_InterimReceipt = "INTERIM";
			shipmentBO.JS_HouseBill = "AirHouse";
			shipmentBO.JS_RL_NKOrigin = "USDAL";
			shipmentBO.JS_RL_NKDestination = "AUBDG";

			var cusEntryNumber1 = shipmentBO.Numbers.AddNew();
			cusEntryNumber1.CE_EntryNum = "CE00001";
			cusEntryNumber1.CE_EntryType = "AMS";
			cusEntryNumber1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var cusEntryNumber2 = shipmentBO.Numbers.AddNew();
			cusEntryNumber2.CE_EntryNum = "CE00002";
			cusEntryNumber2.CE_EntryType = "COC";
			cusEntryNumber2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var consol = shipmentBO.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "08112345675";
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var additionalReference1 = consol.Numbers.AddNew();
			additionalReference1.CE_EntryNum = "CE00001";
			additionalReference1.CE_EntryType = "AMS";
			additionalReference1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var additionalReference2 = consol.Numbers.AddNew();
			additionalReference2.CE_EntryNum = "CE00077";
			additionalReference2.CE_EntryType = "COC";
			additionalReference2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var manager = shipmentBO.GetUniversalDataContextManager() as IEventDataContextManager;

			var eventContextValues = string.Join("\r\n", manager.EventContextValues.Select(o => o.Key + " - " + o.Value).ToArray());

			AssertMultilineASCIIEquals("manager.EventContextValues", @"
MAWBNumber - 081-12345675
MAWBOriginIATAAirportCode - LAX
MAWBDestinationIATAAirportCode - SYD
MBOLOriginUNLOCO - USLAX
MBOLDestinationUNLOCO - AUSYD
AMS Number - CE00001
Customs Office Code (Override) - CE00077
HAWBNumber - AIRHOUSE
HAWBOriginIATAAirportCode - DFW
HAWBDestinationIATAAirportCode - BXG
HBOLOriginUNLOCO - USDAL
HBOLDestinationUNLOCO - AUBDG
ShippersReference - FUL423189120
InterimReceipt - INTERIM
Customs Office Code (Override) - CE00002
".Trim(), eventContextValues);
		}

		public void TestImportOfSubShipmentOnConsolEachWithATransportLegDoesNotError()
		{
			var message = GetQueuedUniversalShipmentMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalShipmentAndTransportLegs.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='LEHS1100000057') from UniversalShipment.
Added Shipment (House Bill='LEHS1100000058') from UniversalShipment.
Added Shipment (House Bill='LEHS1300000059') from UniversalShipment.
Added Consol (Master Bill='C1360') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='C1360') with 1 x Transport, 2 x ForwardingPackLine, 3 x ForwardingShipment.
".Trim(), serviceTaskLog.ToString());
			});

			var consol = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, "C1360"));
			var shipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "LEHS1300000059"));
			AssertEquals("consol.GridShipments[0].PK", shipment.PK, consol.GridShipments[0].PK);
			var coLoadShipments = shipment.CoLoadShipments.ToArray<ForwardingShipment>();
			var subShipment1 = coLoadShipments.Single(o => o.JS_HouseBill == "LEHS1100000057");
			var subShipment2 = coLoadShipments.Single(o => o.JS_HouseBill == "LEHS1100000058");

			serviceTaskLog.ClearLogs();
			((BusinessObject)message).GetNotes().RemoveAndDeleteAll();
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", string.Format(@"
Updated Shipment {0} (House Bill='LEHS1100000057') from UniversalShipment.
Updated Shipment {1} (House Bill='LEHS1100000058') from UniversalShipment.
Updated Shipment {2} (House Bill='LEHS1300000059') from UniversalShipment.
Updated Consol C00001000 (Master Bill='C1360') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='C1360') with 1 x Transport, 2 x ForwardingPackLine, 3 x ForwardingShipment.
", subShipment1.JS_UniqueConsignRef, subShipment2.JS_UniqueConsignRef, shipment.JS_UniqueConsignRef).Trim(), serviceTaskLog.ToString());
			});
		}

		public void TestImportOfSubShipmentOnConsolEachWithATransportLegDoesNotError_GetKeys()
		{
			var message = GetQueuedUniversalShipmentMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalShipmentAndTransportLegs.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			AssertContainsExactElementsInAnyOrder(new string[] { "C1360", "ANL YARRUNGA789", "78920-Mar-11 18:11:00" }, manager.GetKeysForBlockingParallelImport(message).Keys);
		}

		public void TestImportShipmentWithEmptyConsol()
		{
			var message = GetQueuedUniversalShipmentMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalShipmentWithAnEmptyConsol.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='HOU21000757') from UniversalShipment.
Added Consol from UniversalShipment.
Successfully saved Consol C00001000 with 1 x ForwardingContainer, 2 x ForwardingPackLine, 1 x ForwardingShipment.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
No matching ForwardingContainer found, creating new ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
No matching ForwardingPackLine found, creating new ForwardingPackLine.
Populating ForwardingPackLine...
No matching ForwardingPackLine found, creating new ForwardingPackLine.
Populating ForwardingPackLine...
Warning - Attempted to insert 81 characters into Field [JS_GoodsDescription] which has a maximum length of 35 characters. Field was truncated.
Added Shipment (House Bill='HOU21000757') from UniversalShipment.
Added Consol from UniversalShipment.
Successfully saved Consol C00001000 with 1 x ForwardingContainer, 2 x ForwardingPackLine, 1 x ForwardingShipment.
".Trim(), logNoteText);

				var consol = Factory.LoadTop1<ForwardingConsol>(new ZQuery());
				var shipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, new ZString("HOU21000757")));
				AssertNotNull("Loaded Shipment.", shipment);
				AssertNotNull("Loaded Consol.", consol);

				AssertEquals("shipment.Consols.Count", 1, shipment.Consols.Count);
				AssertEquals("Shipment is a child of the same Consol that was created", consol.PK, shipment.Consols[0].PK);
				AssertEquals("Consol contains the same Shipment that was created", shipment.PK, consol.Shipments[0].PK);
			});
		}

		public void TestImportShipmentWithEmptyConsol_GetKeys()
		{
			var message = GetQueuedUniversalShipmentMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalShipmentWithAnEmptyConsol.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), manager.GetKeysForBlockingParallelImport(message).Keys);
		}

		public void TestImportEventWithDataTargetDoesNotUseContextInformationIfDataTargetMatches()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001010";
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_HouseBill = "ONTHEHOUSE";

			var dummyShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			dummyShipment.JS_UniqueConsignRef = "S00002010";
			dummyShipment.JS_TransportMode = Constants.TransportModes.Air;
			dummyShipment.JS_HouseBill = "FRED235478923";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalEventWithDataTarget.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Shipment S00001010 (House Bill='ONTHEHOUSE').
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Shipment S00001010 (House Bill='ONTHEHOUSE').
".Trim(), message.GetLogNoteText());

				shipment.Reload();
				var logs = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AuthorisedCode));
				AssertEquals("[ATH] - Action Authorised event count", 1, logs.Length);
				var log = logs[0];

				var contextItems = log.SourceInfoItems;
				var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select((item) => item.Key + " - " + item.Data).ToArray());
				AssertEquals("Context Items on Event", @"
MAWB Number - 10137654321
HAWB Number - FRED235478923
HAWB Origin IATA Airport Code - FRA
HAWB Destination IATA Airport Code - HKG
Data Source Company - EDI - Eagle Datamation International
Data Source Enterprise ID - EDI
Data Source Server ID - DAT
".Trim(), actualContextItems);
			});
		}

		public void TestImportShipmentThroughJobNumber()
		{
			var shipmentBOToLoad = Factory.New<ForwardingShipment>();
			shipmentBOToLoad.JS_UniqueConsignRef = "S00001003";
			shipmentBOToLoad.JS_BookingReference = "CANTTOUCHTHIS";
			shipmentBOToLoad.JS_HouseBill = "ONTHEHOUSE";

			var dummyShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			dummyShipment.JS_UniqueConsignRef = "S00002010";
			dummyShipment.JS_TransportMode = Constants.TransportModes.Sea;
			dummyShipment.JS_HouseBill = "FRED235478923";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalShipmentWithKey.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Updated Shipment S00001003 (House Bill='FRED235478923') from UniversalShipment.
Successfully saved Shipment S00001003 (House Bill='FRED235478923') with 1 x CusEntryNumber.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertContains("message.GetLogNoteText()", "Successfully loaded matching ForwardingShipment.", logNoteText);
				AssertContains("message.GetLogNoteText()", "Updated Shipment S00001003 (House Bill='FRED235478923') from UniversalShipment.", logNoteText);

				var shipment = new BusinessObjectFactory().LoadFromUniqueKey<ForwardingShipment>(JobShipmentSchema.JS_UniqueConsignRef, new ZString("S00001003"));
				AssertNotNull("Loaded Shipment.", shipment);
				AssertEquals("shipment.JS_HouseBill", "FRED235478923", shipment.JS_HouseBill);
				AssertEquals("shipment.JS_BookingReference", "CANTTOUCHTHIS", shipment.JS_BookingReference);
			});
		}

		public void TestImportShipmentThroughJobNumber_GetKeys()
		{
			var shipmentBOToLoad = Factory.New<ForwardingShipment>();
			shipmentBOToLoad.JS_UniqueConsignRef = "S00001003";
			shipmentBOToLoad.JS_BookingReference = "CANTTOUCHTHIS";
			shipmentBOToLoad.JS_HouseBill = "ONTHEHOUSE";

			var dummyShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			dummyShipment.JS_UniqueConsignRef = "S00002010";
			dummyShipment.JS_TransportMode = Constants.TransportModes.Sea;
			dummyShipment.JS_HouseBill = "FRED235478923";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalShipmentWithKey.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			AssertContainsExactElementsInAnyOrder(new[] { "FRED235478923", "AMSCE00001", "CCUSAMSCE00001", shipmentBOToLoad.PK.ToStringKey() }, manager.GetKeysForBlockingParallelImport(message).Keys);
		}

		public void TestIfImportReturnsMoreThanOneBusinessObjectThroughUniqueQueryItThrowsAnException()
		{
			var shipmentBOToLoad = Factory.New<ForwardingShipment>();
			shipmentBOToLoad.JS_BookingReference = "S00001003";

			var duplicateShipmentBO = Factory.New<ForwardingShipment>();
			duplicateShipmentBO.JS_BookingReference = "S00001003";

			Factory.SaveForTesting();

			var pk1 = shipmentBOToLoad.JS_UniqueConsignRef == "S00001000" ? shipmentBOToLoad.PK : duplicateShipmentBO.PK;
			var pk2 = shipmentBOToLoad.PK == pk1 ? duplicateShipmentBO.PK : shipmentBOToLoad.PK;
			var message = GetQueuedUniversalShipmentMessage(ResourceRetriever.GetString(GetResourcePathFor("DuplicateUniversalShipment.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", string.Format(@"
ERROR - Found more than one Parent Business Object using the DataTarget Key S00001003. Parents found were:
Shipment S00001000 PK: [{0}]
Shipment S00001001 PK: [{1}]
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
				".Trim(), pk1, pk2), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", string.Format(@"
Error - Found more than one Parent Business Object using the DataTarget Key S00001003. Parents found were:
Shipment S00001000 PK: [{0}]
Shipment S00001001 PK: [{1}]
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.
				".Trim(), pk1, pk2), logNoteText);
			});
		}

		public void TestCanImportShipmentWithConsolViaUniversalDataBuss()
		{
			var message = GetQueuedUniversalShipmentMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalShipmentWithConsol.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='FRED235478923') from UniversalShipment.
Added Consol (Master Bill='1811111111') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='1811111111') with 1 x ForwardingShipment.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("", @"
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='FRED235478923') from UniversalShipment.
Added Consol (Master Bill='1811111111') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='1811111111') with 1 x ForwardingShipment.
".Trim(), logNoteText);

				var shipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "FRED235478923"));
				AssertNotNull("Forwarding Shipment should exist with a House Bill of [FRED235478923].", shipment);

				var consol = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, "1811111111"));
				AssertNotNull("Forwarding Consol should exist with a Master Bill of [1811111111].", consol);
				AssertEquals("Shipment on Consol should match imported Shipment", shipment.PK, consol.GridShipments[0].PK);
			});
		}

		public void TestCanImportSubSubShipmentWithConsolAtTopLevelViaUniversalDataBuss()
		{
			var message = GetQueuedUniversalShipmentMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalShipmentAsSubSubShipment.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				var subSubShipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "ONTHEHOUSE"));
				AssertNotNull("Forwarding Shipment should exist with a House Bill of [ONTHEHOUSE].", subSubShipment);

				var subShipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "FRED235478923"));
				AssertNotNull("Forwarding Shipment should exist with a House Bill of [FRED235478923].", subShipment);

				var consol = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, "1811111111"));
				AssertNotNull("Forwarding Consol should exist with a Master Bill of [1811111111].", consol);
				AssertEquals("Shipment on Consol should match imported Shipment", subShipment.PK, consol.GridShipments[0].PK);
				AssertEquals("SubSubShipment on SubShipment should match imported SubSubShipment", subSubShipment.PK, subShipment.CoLoadShipments[0].PK);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='ONTHEHOUSE') from UniversalShipment.
Added Shipment (House Bill='FRED235478923') from UniversalShipment.
Added Consol (Master Bill='1811111111') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='1811111111') with 2 x ForwardingShipment.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("logNoteText", @"
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='ONTHEHOUSE') from UniversalShipment.
Added Shipment (House Bill='FRED235478923') from UniversalShipment.
Added Consol (Master Bill='1811111111') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='1811111111') with 2 x ForwardingShipment.
".Trim(), logNoteText);
			});
		}

		public void TestCanImportShipmentViaUniversalDataBuss()
		{
			var message = GetQueuedUniversalShipmentMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalShipment.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='FRED235478923') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='FRED235478923') with 1 x CusEntryNumber.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertContains("message.GetLogNoteText()", "No matching ForwardingShipment found, creating new ForwardingShipment.", logNoteText);
				AssertContains("message.GetLogNoteText()", "Added Shipment (House Bill='FRED235478923') from UniversalShipment.", logNoteText);

				var shipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "FRED235478923"));
				AssertNotNull("Forwarding Shipment should exist with a House Bill of [FRED235478923].", shipment);
			});
		}

		public void TestCanImportEventViaUniversalDataBuss()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001010";
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_HouseBill = "FRED235478923";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalEvent.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Shipment S00001010 (House Bill='FRED235478923').
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Shipment S00001010 (House Bill='FRED235478923').
".Trim(), message.GetLogNoteText());

				shipment.Reload();
				var logs = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AuthorisedCode));
				AssertEquals("[ATH] - Action Authorised event count", 1, logs.Length);
				var log = logs[0];

				var contextItems = log.SourceInfoItems;
				var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select((item) => item.Key + " - " + item.Data).ToArray());
				AssertEquals("Context Items on Event", @"
MAWB Number - 10137654321
HAWB Number - FRED235478923
HAWB Origin IATA Airport Code - FRA
HAWB Destination IATA Airport Code - HKG
Carriers Booking Reference - 20257654321
Shippers Reference - FUL423189120
Interim Receipt - INTERIM
AMS Number - 134FREGT
COC - 267AIRGT
Data Source Company - EDI - Eagle Datamation International
Data Source Enterprise ID - EDI
Data Source Server ID - DAT
".Trim(), actualContextItems);
			});
		}

		public void TestContextInformationIsAllThereForAir()
		{
			var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Constants.TransportModes.Air;
			shipmentBO.JS_BookingReference = "FUL423189120";
			shipmentBO.JS_InterimReceipt = "INTERIM";
			shipmentBO.JS_HouseBill = "AirHouse";
			shipmentBO.DocsAndCartage.JP_OrderItemsAsString = "Order Items #1";
			shipmentBO.JS_RL_NKOrigin = "USDAL";
			shipmentBO.JS_RL_NKDestination = "AUBDG";

			var cusEntryNumber1 = shipmentBO.Numbers.AddNew();
			cusEntryNumber1.CE_EntryNum = "CE00001";
			cusEntryNumber1.CE_EntryType = "AMS";
			cusEntryNumber1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var cusEntryNumber2 = shipmentBO.Numbers.AddNew();
			cusEntryNumber2.CE_EntryNum = "CE00002";
			cusEntryNumber2.CE_EntryType = "COC";
			cusEntryNumber2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var consol = shipmentBO.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_BookingReference = "BOOKME";
			consol.JK_AgentsReference = "DOUBLEAGENT";
			consol.JK_MasterBillNum = "08112345675";
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var cusEntryNumber3 = consol.Numbers.AddNew();
			cusEntryNumber3.CE_EntryNum = "CE00003";
			cusEntryNumber3.CE_EntryType = "UBR";
			cusEntryNumber3.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var manager = shipmentBO.GetUniversalDataContextManager() as IEventDataContextManager;

			var eventContextValues = string.Join("\r\n", manager.EventContextValues.Select(o => o.Key + " - " + o.Value).ToArray());

			AssertMultilineASCIIEquals("manager.EventContextValues", @"
MAWBNumber - 081-12345675
MAWBOriginIATAAirportCode - LAX
MAWBDestinationIATAAirportCode - SYD
MBOLOriginUNLOCO - USLAX
MBOLDestinationUNLOCO - AUSYD
CarriersBookingReference - BOOKME
AgentsReference - DOUBLEAGENT
Under Bond Approval Reference Number - CE00003
HAWBNumber - AIRHOUSE
HAWBOriginIATAAirportCode - DFW
HAWBDestinationIATAAirportCode - BXG
HBOLOriginUNLOCO - USDAL
HBOLDestinationUNLOCO - AUBDG
ShippersReference - FUL423189120
OrderNumber - Order
OrderNumber - Items
OrderNumber - #1
InterimReceipt - INTERIM
AMS Number - CE00001
Customs Office Code (Override) - CE00002
".Trim(), eventContextValues);
		}

		public void TestContextInformationIsAllThereForSea()
		{
			var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Constants.TransportModes.Sea;
			shipmentBO.JS_BookingReference = "FUL423189120";
			shipmentBO.JS_InterimReceipt = "INTERIM";
			shipmentBO.JS_HouseBill = "SeaHouse";
			shipmentBO.DocsAndCartage.JP_OrderItemsAsString = "Order Items #1";
			shipmentBO.JS_RL_NKOrigin = "USDAL";
			shipmentBO.JS_RL_NKDestination = "AUBDG";

			var cusEntryNumber1 = shipmentBO.Numbers.AddNew();
			cusEntryNumber1.CE_EntryNum = "CE00001";
			cusEntryNumber1.CE_EntryType = "AMS";
			cusEntryNumber1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var cusEntryNumber2 = shipmentBO.Numbers.AddNew();
			cusEntryNumber2.CE_EntryNum = "CE00002";
			cusEntryNumber2.CE_EntryType = "COC";
			cusEntryNumber2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var consol = shipmentBO.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_BookingReference = "BOOKME";
			consol.JK_AgentsReference = "DOUBLEAGENT";
			consol.JK_MasterBillNum = "MB8112345675";
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var cusEntryNumber3 = consol.Numbers.AddNew();
			cusEntryNumber3.CE_EntryNum = "CE00003";
			cusEntryNumber3.CE_EntryType = "UBR";
			cusEntryNumber3.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var manager = shipmentBO.GetUniversalDataContextManager() as IEventDataContextManager;

			var eventContextValues = string.Join("\r\n", manager.EventContextValues.Select(o => o.Key + " - " + o.Value).ToArray());

			AssertMultilineASCIIEquals("manager.EventContextValues", @"
MBOLNumber - MB8112345675
MBOLOriginUNLOCO - USLAX
MBOLDestinationUNLOCO - AUSYD
CarriersBookingReference - BOOKME
AgentsReference - DOUBLEAGENT
Under Bond Approval Reference Number - CE00003
HBOLNumber - SEAHOUSE
HBOLOriginUNLOCO - USDAL
HBOLDestinationUNLOCO - AUBDG
ShippersReference - FUL423189120
OrderNumber - Order
OrderNumber - Items
OrderNumber - #1
InterimReceipt - INTERIM
AMS Number - CE00001
Customs Office Code (Override) - CE00002
".Trim(), eventContextValues);
		}

		public void TestImportEventWithEventTypeIRJAndMessageTypeCO2e()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001016";

			var leg1 = shipment.TransportsIncludingRelated.AddNew();
			leg1.JW_RL_NKLoadPort = "AUMEL";
			leg1.JW_RL_NKDiscPort = "SGSIN";

			var leg2 = shipment.TransportsIncludingRelated.AddNew();
			leg2.JW_RL_NKLoadPort = "AUMEL";
			leg2.JW_RL_NKDiscPort = ZString.Empty;

			var leg3 = shipment.TransportsIncludingRelated.AddNew();
			leg3.JW_RL_NKLoadPort = ZString.Empty;
			leg3.JW_RL_NKDiscPort = "SGSIN";

			var leg4 = shipment.TransportsIncludingRelated.AddNew();
			leg4.JW_RL_NKLoadPort = ZString.Empty;
			leg4.JW_RL_NKDiscPort = ZString.Empty;

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalEventWithEventTypeIRJAndMessageTypeCO2e.xml")));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals(CO2eStatusList.Codes.Rejected, shipment.GetCO2eStatus());
			AssertEquals(CO2eStatusList.Codes.Rejected, leg1.GetCO2eStatus());
			AssertEquals(CO2eStatusList.Codes.Rejected, leg2.GetCO2eStatus());
			AssertEquals(CO2eStatusList.Codes.Rejected, leg3.GetCO2eStatus());
			AssertEquals(CO2eStatusList.Codes.Rejected, leg4.GetCO2eStatus());

			AssertLog(shipment, 1);
			AssertLog(leg1, 1);
			AssertLog(leg2, 0);
			AssertLog(leg3, 0);
			AssertLog(leg4, 0);
		}

		static void AssertLog(EnterpriseBusinessObject bizo, int expected)
		{
			const string failureReason = "ERROR! ERROR! and more ERROR...";
			AssertEquals(expected, bizo.Logs.Find(log =>
				log.SL_SE_NKEvent == AutoEvents.GreenhouseGasEmissionsCalculationCode &&
				log.Parameters.TryGetValue(Params.Type, out var type) && type == nameof(CO2eEventType.Rejected) &&
				log.Parameters.TryGetValue(Params.Reason, out var reason) && reason == failureReason).Count());
		}

		#region TestOnUniversalEventAdded

		public void TestOnUniversalEventAdded_Error()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_OuterPacks = 10;
			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Now; // prerequisite - Adds a Confirmation

			var uEvent = new UniversalEvent() { EventType = Events.DeliveryCartageCompleteFinalisedCode };

			var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;

			var serviceLogger = new ServiceTaskLogForTesting();
			var messageLogger = new XmlSessionTracker(serviceLogger);
			manager.OnUniversalEventAdded(messageLogger, uEvent);
			AssertEquals("", shipment.DeliveryConfirms[0].EU_GoodsSignForBy);
			AssertEquals("'Goods Signed By' has not been updated, because there was no ContextType ReceivedFromName specified or was empty.", messageLogger.ToString());

			uEvent.ContextCollection = new List<Context>() { new Context() { Type = nameof(ContextTypes.ReceivedFromName), Value = null } };
			serviceLogger = new ServiceTaskLogForTesting();
			messageLogger = new XmlSessionTracker(serviceLogger);
			manager.OnUniversalEventAdded(messageLogger, uEvent);
			AssertEquals("", shipment.DeliveryConfirms[0].EU_GoodsSignForBy);
			AssertEquals("'Goods Signed By' has not been updated, because there was no ContextType ReceivedFromName specified or was empty.", messageLogger.ToString());
		}

		public void TestOnUniversalEventAdded_Loose()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_OuterPacks = 10;
			shipment.JS_UniqueConsignRef = "S00001234";
			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Now; // prerequisite - Adds a Confirmation

			var uEvent = new UniversalEvent() { EventType = Events.DeliveryCartageCompleteFinalisedCode };
			uEvent.ContextCollection = new List<Context>() { new Context() { Type = nameof(ContextTypes.ReceivedFromName), Value = "Bob" } };

			var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;

			var serviceLogger = new ServiceTaskLogForTesting();
			var messageLogger = new XmlSessionTracker(serviceLogger);
			manager.OnUniversalEventAdded(messageLogger, uEvent);
			AssertEquals("Bob", shipment.DeliveryConfirms[0].EU_GoodsSignForBy);
			AssertEquals("'Goods Signed By' has been updated to value 'Bob' on the confirmation of Shipment S00001234.", messageLogger.ToString());
		}

		public void TestOnUniversalEventAdded_Containerised()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			var consol = shipment.Consols.AddNew();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT1234";
			shipment.OuterPackLines.AddNew().SetContainer(consol, container);

			var uEvent = new UniversalEvent() { EventType = Events.DeliveryCartageCompleteFinalisedCode };
			uEvent.ContextCollection = new List<Context>() { new Context() { Type = nameof(ContextTypes.ReceivedFromName), Value = "Bob" } };

			var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;

			var serviceLogger = new ServiceTaskLogForTesting();
			var messageLogger = new XmlSessionTracker(serviceLogger);
			manager.OnUniversalEventAdded(messageLogger, uEvent);
			AssertEquals("Bob", container.DestinationConfirm.EU_GoodsSignForBy);
			AssertEquals("'Goods Signed By' has been updated to value 'Bob' on the confirmation of Container 'CONT1234'.", messageLogger.ToString());
		}

		#endregion

		#region IEventTransformer

		public void TestIEventTransformer_Integration_EventFromTransportBooking_PickUpEventIsTransformedAndSavedOnContainersAsGateOut()
		{
			const string eventXmlMessage = @"
<UniversalEvent>
	<Event>

	<DataContext>
		<DataSourceCollection>
		<DataSource>
			<Type>TransportBookingConfirmation</Type>
			<Key></Key>
		</DataSource>
		</DataSourceCollection>

		<DataTargetCollection>
		<DataTarget>
			<Type>ForwardingShipment</Type>
			<Key>HELLO</Key>
		</DataTarget>
		</DataTargetCollection>
	</DataContext>

	<EventType>PUP</EventType>
	<EventTime>10-SEP-2016 18:00</EventTime>
	<EventReference>|FAC=CY</EventReference>
	<IsEstimate>false</IsEstimate>

	<ContextCollection>
		<Context>
		<Type>ContainerNumber</Type>
		<Value>CONA</Value>
		</Context>
		<Context>
		<Type>ContainerNumber</Type>
		<Value>CONB</Value>
		</Context>
	</ContextCollection>

	</Event>
</UniversalEvent>
";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var containerA = consol.Containers.AddNew();
			containerA.JC_ContainerNum = "CONA";

			var containerB = consol.Containers.AddNew();
			containerB.JC_ContainerNum = "CONB";

			var containerC = consol.Containers.AddNew();
			containerC.JC_ContainerNum = "CONC";

			var containerD = consol.Containers.AddNew();
			containerD.JC_ContainerNum = "COND";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "HELLO";

			shipment.OuterPackLines.RemoveAndDeleteAll();

			shipment.OuterPackLines.AddNew().SetContainer(consol, containerA);
			shipment.OuterPackLines.AddNew().SetContainer(consol, containerB);
			shipment.OuterPackLines.AddNew().SetContainer(consol, containerC);

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(eventXmlMessage);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("Prerequisite: message processed", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			CombineAssertions(() =>
			{
				var allInvolvedBusinessObjects = new IStmALogParent[]
					{
						consol,
						shipment,
						containerA,
						containerB,
						containerC,
						containerD
					};

				var expectedObjectsWithGateOutEvent = new IStmALogParent[]
					{
						containerA,
						containerB
					};

				var pickedUpQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.PickedUpCode);
				var gateOutQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.GateOutCode);

				foreach (var bizo in allInvolvedBusinessObjects)
				{
					AssertEquals($"PickedUp event should not be added to {bizo.HumanReadableName}", 0, bizo.Logs.Find(pickedUpQuery).Length);

					if (expectedObjectsWithGateOutEvent.Contains(bizo))
					{
						var gateOutLogs = bizo.Logs.Find(gateOutQuery);
						AssertEquals($"GateOut event should be added to {bizo.HumanReadableName}", 1, gateOutLogs.Length);
						AssertEquals("Event reference has location injected from consol", "|FAC=CY|LOC=AUSYD", gateOutLogs.First().SL_Reference);
					}
					else
					{
						AssertEquals($"GateOut event should not be added to {bizo.HumanReadableName}", 0, bizo.Logs.Find(gateOutQuery).Length);
					}
				}
			});
		}

		#endregion

		#region CTStatus on Shipment

		public void TestUpdateCTStatusByUniversalShipment()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;
			consol1.JK_UniqueConsignRef = "C00005000";
			consol1.JK_ConsolMode = Constants.ContainerModes.FCL;
			var container1 = SetupContainer(consol1);

			var shipment = consol1.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S009900";
			shipment.JS_HouseBill = "HB003333";
			shipment.JS_RL_NKOrigin = "GBSOU";
			shipment.JS_RL_NKDestination = "GBCMG";

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>FOR</Code>
				<Description>Forwarder</Description>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<WayBillNumber>HB003333</WayBillNumber>
	<CommunityTransitStatus>
		<Code>C</Code>
	</CommunityTransitStatus>
	</Shipment>
</UniversalShipment>
";

			#endregion

			var message = GetQueuedUniversalShipmentMessage(xmlMessage);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"
Successfully loaded matching ForwardingShipment.
Populating ForwardingShipment...
Updated Shipment S009900 (House Bill='HB003333') from UniversalShipment.
Successfully saved Shipment S009900 (House Bill='HB003333').
".Trim(), manager.Logger.ToString());

				var newShipment = Factory.Load<ForwardingShipment>(shipment.PK);
				AssertNotNull("newShipment should be loaded", newShipment);
				AssertEquals("JS_CommunityTransitStatus should have been updated", "C", newShipment.JS_CommunityTransitStatus);
			});
		}

		#endregion

		#region VGM Import for Co-Load Shipment

		public void TestVGMImport_CoLoadShipment_NoMatching_NoMBLAndNoBookingRef()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;
			consol1.JK_UniqueConsignRef = "C00005000";

			var shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_TransportMode = "SEA";
			shipment1.JS_PackingMode = "FCL";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment1.JS_UniqueConsignRef = "S009900";
			shipment1.JS_HouseBill = "HB003333";

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>FOR</Code>
				<Description>Forwarder</Description>
				<ServiceCode>VGM</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<WayBillType>
		<Code>HWB</Code>
		<Description>House Waybill</Description>
	</WayBillType>
	</Shipment>
</UniversalShipment>";

			#endregion

			var message = GetQueuedUniversalShipmentMessage(xmlMessage);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Error, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"
Error - Cannot populate ForwardingShipment because:
XML file contains VGM service code and cannot find a matched shipment.
Successfully saved, but nothing was reported as being updated.
".Trim(), manager.Logger.ToString());
			});
		}

		public void TestVGMImport_CoLoadShipment_NoMatching_Not_HWB()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;
			consol1.JK_UniqueConsignRef = "C00005000";

			var shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_TransportMode = "SEA";
			shipment1.JS_PackingMode = "FCL";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment1.JS_UniqueConsignRef = "S009900";
			shipment1.JS_HouseBill = "HB003333";

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>FOR</Code>
				<Description>Forwarder</Description>
				<ServiceCode>VGM</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<BookingConfirmationReference>S009900</BookingConfirmationReference>
	<WayBillType>
		<Code>SWB</Code>
	</WayBillType>
	</Shipment>
</UniversalShipment>";

			#endregion

			var message = GetQueuedUniversalShipmentMessage(xmlMessage);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Error, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"
Error - Cannot populate ForwardingShipment because:
XML file contains VGM service code and cannot find a matched shipment.
Successfully saved, but nothing was reported as being updated.
".Trim(), manager.Logger.ToString());
			});
		}

		public void TestVGMImport_CoLoadShipment_BookingRef()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;
			consol1.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol1.JK_UniqueConsignRef = "C00005000";
			SetupContainer(consol1);

			var shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_TransportMode = "SEA";
			shipment1.JS_PackingMode = "FCL";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment1.JS_UniqueConsignRef = "S009900";
			shipment1.JS_HouseBill = "HB003333";

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_UniqueConsignRef = "C00005001";

			var shipment2 = consol2.Shipments.AddNew();
			shipment2.JS_TransportMode = "SEA";
			shipment2.JS_PackingMode = "FCL";
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment2.JS_UniqueConsignRef = "S009901";
			shipment2.JS_HouseBill = "HB003333";

			var consol3 = newFactory.New<ForwardingConsol>();
			consol3.JK_AgentType = Core.Constants.AgentType.Agent;
			consol3.JK_UniqueConsignRef = "C00005002";

			var shipment3 = consol3.Shipments.AddNew();
			shipment3.JS_TransportMode = "SEA";
			shipment3.JS_PackingMode = "FCL";
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment3.JS_UniqueConsignRef = "S009902";
			shipment3.JS_HouseBill = "HB003333";

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>FOR</Code>
				<Description>Forwarder</Description>
				<ServiceCode>VGM</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<BookingConfirmationReference>S009900</BookingConfirmationReference>
	<WayBillType>
		<Code>HWB</Code>
		<Description>House WayBill</Description>
	</WayBillType>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TCLU3117199</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
			</ContainerType>
			<GrossWeight>7923.000</GrossWeight>
			<GrossWeightVerificationDateTime>2018-06-06T13:00:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>PKG</Code>
			</GrossWeightVerificationType>
			<Seal>1108829</Seal>
			<WeightUnit>
				<Code>KG</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>TESTORG9</OrganizationCode>
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

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"
Successfully loaded matching ForwardingShipment.
Populating ForwardingShipment...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'TESTORG9' by code, address 'Test Address1' (only address).
Updated Shipment S009900 (House Bill='HB003333') from UniversalShipment.
Successfully saved Shipment S009900 (House Bill='HB003333') with 1 x ForwardingContainer.
".Trim(), manager.Logger.ToString());
			});
		}

		public void TestVGMImport_CoLoadShipment_MBL()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;
			consol1.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol1.JK_UniqueConsignRef = "C00005000";
			SetupContainer(consol1);

			var shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_TransportMode = "SEA";
			shipment1.JS_PackingMode = "FCL";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment1.JS_UniqueConsignRef = "S009900";
			shipment1.JS_HouseBill = "HB003333";

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_UniqueConsignRef = "C00005001";

			var shipment2 = consol2.Shipments.AddNew();
			shipment2.JS_TransportMode = "SEA";
			shipment2.JS_PackingMode = "FCL";
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment2.JS_UniqueConsignRef = "S009901";
			shipment2.JS_HouseBill = "HB003334";

			var consol3 = newFactory.New<ForwardingConsol>();
			consol3.JK_AgentType = Core.Constants.AgentType.Agent;
			consol3.JK_UniqueConsignRef = "C00005002";

			var shipment3 = consol3.Shipments.AddNew();
			shipment3.JS_TransportMode = "SEA";
			shipment3.JS_PackingMode = "FCL";
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment3.JS_UniqueConsignRef = "S009902";
			shipment3.JS_HouseBill = "HB003335";

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<DataSourceCollection>
			<DataSource>
				<Type>ForwardingShipment</Type>
			</DataSource>
		</DataSourceCollection>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>FOR</Code>
				<Description>Forwarder</Description>
				<ServiceCode>VGM</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<WayBillNumber>HB003333</WayBillNumber>
	<WayBillType>
		<Code>HWB</Code>
		<Description>House WayBill</Description>
	</WayBillType>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TCLU3117199</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
			</ContainerType>
			<GrossWeight>7923.000</GrossWeight>
			<GrossWeightVerificationDateTime>2018-06-06T13:00:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>PKG</Code>
			</GrossWeightVerificationType>
			<Seal>1108829</Seal>
			<WeightUnit>
				<Code>KG</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>TESTORG9</OrganizationCode>
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

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"
Successfully loaded matching ForwardingShipment.
Populating ForwardingShipment...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'TESTORG9' by code, address 'Test Address1' (only address).
Updated Shipment S009900 (House Bill='HB003333') from UniversalShipment.
Successfully saved Shipment S009900 (House Bill='HB003333') with 1 x ForwardingContainer.
".Trim(), manager.Logger.ToString());
			});
		}

		public void TestVGMImport_CoLoadShipment_MBL_UpdateContainerOnly()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;
			consol1.JK_UniqueConsignRef = "C00005000";
			consol1.JK_ConsolMode = Constants.ContainerModes.FCL;
			var container1 = SetupContainer(consol1);

			var shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_TransportMode = "SEA";
			shipment1.JS_PackingMode = "FCL";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment1.JS_UniqueConsignRef = "S009900";
			shipment1.JS_HouseBill = "HB003333";
			shipment1.JS_ReleaseType = "BRR";

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_UniqueConsignRef = "C00005001";

			var shipment2 = consol2.Shipments.AddNew();
			shipment2.JS_TransportMode = "SEA";
			shipment2.JS_PackingMode = "FCL";
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment2.JS_UniqueConsignRef = "S009901";
			shipment2.JS_HouseBill = "HB003334";

			var consol3 = newFactory.New<ForwardingConsol>();
			consol3.JK_AgentType = Core.Constants.AgentType.Agent;
			consol3.JK_UniqueConsignRef = "C00005002";

			var shipment3 = consol3.Shipments.AddNew();
			shipment3.JS_TransportMode = "SEA";
			shipment3.JS_PackingMode = "FCL";
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment3.JS_UniqueConsignRef = "S009902";
			shipment3.JS_HouseBill = "HB003335";

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>FOR</Code>
				<Description>Forwarder</Description>
				<ServiceCode>VGM</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<WayBillNumber>HB003333</WayBillNumber>
	<WayBillType>
		<Code>HWB</Code>
		<Description>House WayBill</Description>
	</WayBillType>
	<ReleaseType>
		<Code>BSD</Code>
	</ReleaseType>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TCLU3117199</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
			</ContainerType>
			<GrossWeight>7923.000</GrossWeight>
			<GrossWeightVerificationDateTime>2018-06-06T13:00:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>PKG</Code>
			</GrossWeightVerificationType>
			<Seal>1108829</Seal>
			<WeightUnit>
				<Code>KG</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>TESTORG9</OrganizationCode>
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

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"
Successfully loaded matching ForwardingShipment.
Populating ForwardingShipment...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'TESTORG9' by code, address 'Test Address1' (only address).
Updated Shipment S009900 (House Bill='HB003333') from UniversalShipment.
Successfully saved Shipment S009900 (House Bill='HB003333') with 1 x ForwardingContainer.
".Trim(), manager.Logger.ToString());

				var newShipment = Factory.Load<ForwardingShipment>(shipment1.PK);
				AssertNotNull("newShipment should be loaded", newShipment);
				AssertEquals("JS_ReleaseType should not be updated", "BRR", newShipment.JS_ReleaseType);

				var newContainer = Factory.Load<ForwardingContainer>(container1.PK);
				AssertNotNull("newContainer should be loaded", newContainer);
				AssertEquals("JC_GrossWeightVerificationDateTime should be updated", new ZDateTime(2018, 6, 6, 13, 0, 0), newContainer.JC_GrossWeightVerificationDateTime);
			});
		}

		public void TestVGMImport_CoLoadShipment_MBL_Reject_NotExistedContainer()
		{
			var newFactory = new BusinessObjectFactory();

			var consol = newFactory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = "C00005000";

			var container = consol.Containers.AddNew();
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_ContainerNum = "TCLU3117199";
			var refContainer = newFactory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			AssertNotNull(refContainer);
			container.JC_RC = refContainer.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment.JS_UniqueConsignRef = "S009900";
			shipment.JS_HouseBill = "HB003333";

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>FOR</Code>
				<Description>Forwarder</Description>
				<ServiceCode>VGM</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<WayBillNumber>HB003333</WayBillNumber>
	<WayBillType>
		<Code>HWB</Code>
		<Description>House WayBill</Description>
	</WayBillType>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TCLU3117180</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
			</ContainerType>
			<GrossWeight>7923.000</GrossWeight>
			<GrossWeightVerificationDateTime>2018-06-06T13:00:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>PKG</Code>
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

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Error, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"
Successfully loaded matching ForwardingShipment.
Error - Cannot populate ForwardingShipment because:
One or multiple specified containers cannot be found in the matched shipment's consol.
Successfully saved, but nothing was reported as being updated.
".Trim(), manager.Logger.ToString());
			});
		}

		public void TestVGMImport_CoLoadShipment_MBL_Reject_ContainerNumberIsEmpty()
		{
			var newFactory = new BusinessObjectFactory();

			var consol = newFactory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = "C00005000";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container1.JC_ContainerNum = "TCLU3117199";
			var refContainer = newFactory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			AssertNotNull(refContainer);
			container1.JC_RC = refContainer.PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container2.JC_ContainerNum = ZString.Empty;
			container2.JC_RC = refContainer.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment.JS_UniqueConsignRef = "S009900";
			shipment.JS_HouseBill = "HB003333";

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>FOR</Code>
				<Description>Forwarder</Description>
				<ServiceCode>VGM</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<WayBillNumber>HB003333</WayBillNumber>
	<WayBillType>
		<Code>HWB</Code>
		<Description>House WayBill</Description>
	</WayBillType>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber></ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
			</ContainerType>
			<GrossWeight>7923.000</GrossWeight>
			<GrossWeightVerificationDateTime>2018-06-06T13:00:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>PKG</Code>
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

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Error, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"
Successfully loaded matching ForwardingShipment.
Error - Cannot populate ForwardingShipment because:
The container number of one or multiple specified containers is empty.
Successfully saved, but nothing was reported as being updated.
".Trim(), manager.Logger.ToString());
			});
		}

		public void TestVGMImport_CoLoadShipment_MBL_Reject_WhenVerificationTypeIsMissing()
		{
			var newFactory = new BusinessObjectFactory();

			var consol = newFactory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = "C00005000";

			var container = consol.Containers.AddNew();
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_ContainerNum = "TCLU3117199";
			var refContainer = newFactory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			AssertNotNull(refContainer);
			container.JC_RC = refContainer.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment.JS_UniqueConsignRef = "S009900";
			shipment.JS_HouseBill = "HB003333";
			newFactory.Save();

			AssertVerificationType(Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified, verificationTime: ZString.Empty,  shouldBeRejected: false);
			AssertVerificationType(Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages, "2018-06-06T13:00:00", shouldBeRejected: false);
			AssertVerificationType(ZString.Empty, verificationTime: "2018-06-06T13:00:00", shouldBeRejected: true);
			AssertVerificationType(null, verificationTime: "2018-06-06T13:00:00", shouldBeRejected: true);
		}

		void AssertVerificationType(ZString verificationType, ZString verificationTime, bool shouldBeRejected)
		{
			#region XML Message

			string xmlMessage = $@"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>FOR</Code>
				<Description>Forwarder</Description>
				<ServiceCode>VGM</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<WayBillNumber>HB003333</WayBillNumber>
	<WayBillType>
	  <Code>HWB</Code>
	  <Description>House WayBill</Description>
	</WayBillType>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TCLU3117199</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
			</ContainerType>
			<GrossWeight>5</GrossWeight>
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
			var newFactory = new BusinessObjectFactory();

			var consol = newFactory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = "C00005000";

			var container = consol.Containers.AddNew();
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_ContainerNum = "TCLU3117199";
			var refContainer = newFactory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			AssertNotNull(refContainer);
			container.JC_RC = refContainer.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment.JS_UniqueConsignRef = "S009900";
			shipment.JS_HouseBill = "HB003333";
			newFactory.Save();

			AssertVerifiedContainer(Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified, verificationTime: ZString.Empty, weight: ZString.Empty, shouldBeRejected: false);
			AssertVerifiedContainer(Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified, verificationTime: ZString.Empty, weight: "0", shouldBeRejected: false);
			AssertVerifiedContainer(Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified, verificationTime: "2018-06-06T13:00:00", weight: ZString.Empty, shouldBeRejected: true);
			AssertVerifiedContainer(Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified, verificationTime: ZString.Empty, weight: "5", shouldBeRejected: false);
			AssertVerifiedContainer(Constants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal, verificationTime: "2018-06-06T13:00:00", weight: ZString.Empty, shouldBeRejected: true);
			AssertVerifiedContainer(Constants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal, verificationTime: "2018-06-06T13:00:00", weight: "0", shouldBeRejected: true);
			AssertVerifiedContainer(Constants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal, verificationTime: ZString.Empty, weight: "5", shouldBeRejected: false);
			AssertVerifiedContainer(Constants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal, verificationTime: "2018-06-06T13:00:00", weight: "5", shouldBeRejected: false);
			AssertVerifiedContainer(Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container, verificationTime: "2018-06-06T13:00:00", weight: ZString.Empty, shouldBeRejected: true);
			AssertVerifiedContainer(Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container, verificationTime: "2018-06-06T13:00:00", weight: "0", shouldBeRejected: true);
			AssertVerifiedContainer(Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container, verificationTime: ZString.Empty, weight: "5", shouldBeRejected: false);
			AssertVerifiedContainer(Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container, verificationTime: "2018-06-06T13:00:00", weight: "5", shouldBeRejected: false);
			AssertVerifiedContainer(Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages, verificationTime: "2018-06-06T13:00:00", weight: ZString.Empty, shouldBeRejected: true);
			AssertVerifiedContainer(Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages, verificationTime: "2018-06-06T13:00:00", weight: "0", shouldBeRejected: true);
			AssertVerifiedContainer(Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages, verificationTime: ZString.Empty, weight: "5", shouldBeRejected: false);
			AssertVerifiedContainer(Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages, verificationTime: "2018-06-06T13:00:00", weight: "5", shouldBeRejected: false);
			AssertVerifiedContainer(Constants.ContainerGrossWeightVerificationTypes.Codes.RationalMethod, verificationTime: "2018-06-06T13:00:00", weight: ZString.Empty, shouldBeRejected: true);
			AssertVerifiedContainer(Constants.ContainerGrossWeightVerificationTypes.Codes.RationalMethod, verificationTime: "2018-06-06T13:00:00", weight: "0", shouldBeRejected: true);
			AssertVerifiedContainer(Constants.ContainerGrossWeightVerificationTypes.Codes.RationalMethod, verificationTime: ZString.Empty, weight: "5", shouldBeRejected: false);
			AssertVerifiedContainer(Constants.ContainerGrossWeightVerificationTypes.Codes.RationalMethod, verificationTime: "2018-06-06T13:00:00", weight: "5", shouldBeRejected: false);
			AssertVerifiedContainer(Constants.ContainerGrossWeightVerificationTypes.Codes.NotRequired, verificationTime: "2018-06-06T13:00:00", weight: ZString.Empty, shouldBeRejected: true);
			AssertVerifiedContainer(Constants.ContainerGrossWeightVerificationTypes.Codes.NotRequired, verificationTime: "2018-06-06T13:00:00", weight: "0", shouldBeRejected: true);
			AssertVerifiedContainer(Constants.ContainerGrossWeightVerificationTypes.Codes.NotRequired, verificationTime: ZString.Empty, weight: "5", shouldBeRejected: true);
			AssertVerifiedContainer(Constants.ContainerGrossWeightVerificationTypes.Codes.NotRequired, verificationTime: "2018-06-06T13:00:00", weight: "5", shouldBeRejected: false);
		}

		void AssertVerifiedContainer(ZString verificationType, ZString verificationTime, ZString weight, bool shouldBeRejected)
		{
			#region XML Message

			string xmlMessage = $@"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>FOR</Code>
				<Description>Forwarder</Description>
				<ServiceCode>VGM</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<WayBillNumber>HB003333</WayBillNumber>
	<WayBillType>
	  <Code>HWB</Code>
	  <Description>House WayBill</Description>
	</WayBillType>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TCLU3117199</ContainerNumber>
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
				AssertMultilineASCIIEquals("Service Task Log", @"
Successfully loaded matching ForwardingShipment.
Error - Cannot populate ForwardingShipment because:
When element 'GrossWeightVerificationType' is 'NON' then element 'GrossWeightVerificationDateTime' will not be imported.
When element 'GrossWeightVerificationType' is not 'NON' then 'GrossWeightVerificationDateTime' must be entered and 'GrossWeight' must be greater than 0.
Successfully saved, but nothing was reported as being updated.
".Trim(), manager.Logger.ToString());
			}
			else
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);
			}
		}

		public void TestVGMImport_CoLoadShipment_MBL_Reject_NoContainerCollectionDataObject()
		{
			var newFactory = new BusinessObjectFactory();

			var consol = newFactory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = "C00005000";

			var container = consol.Containers.AddNew();
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_ContainerNum = "TCLU3117199";
			var refContainer = newFactory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			AssertNotNull(refContainer);
			container.JC_RC = refContainer.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment.JS_UniqueConsignRef = "S009900";
			shipment.JS_HouseBill = "HB003333";

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>FOR</Code>
				<Description>Forwarder</Description>
				<ServiceCode>VGM</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<WayBillNumber>HB003333</WayBillNumber>
	<WayBillType>
		<Code>HWB</Code>
		<Description>House WayBill</Description>
	</WayBillType>
	</Shipment>
</UniversalShipment>";

			#endregion

			var message = GetQueuedUniversalShipmentMessage(xmlMessage);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Error, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"
Successfully loaded matching ForwardingShipment.
Error - Cannot populate ForwardingShipment because:
XML file does not contain any containers.
Successfully saved, but nothing was reported as being updated.
".Trim(), manager.Logger.ToString());
			});
		}

		public void TestVGMImport_CoLoadShipment_MBL_Reject_NoContainerDataObject()
		{
			var newFactory = new BusinessObjectFactory();

			var consol = newFactory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = "C00005000";

			var container = consol.Containers.AddNew();
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_ContainerNum = "TCLU3117199";
			var refContainer = newFactory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			AssertNotNull(refContainer);
			container.JC_RC = refContainer.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment.JS_UniqueConsignRef = "S009900";
			shipment.JS_HouseBill = "HB003333";

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>FOR</Code>
				<Description>Forwarder</Description>
				<ServiceCode>VGM</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<WayBillNumber>HB003333</WayBillNumber>
	<WayBillType>
		<Code>HWB</Code>
		<Description>House WayBill</Description>
	</WayBillType>
	<ContainerCollection Content=""Partial"">
	</ContainerCollection>
	</Shipment>
</UniversalShipment>";

			#endregion

			var message = GetQueuedUniversalShipmentMessage(xmlMessage);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Error, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"
Successfully loaded matching ForwardingShipment.
Error - Cannot populate ForwardingShipment because:
XML file does not contain any containers.
Successfully saved, but nothing was reported as being updated.
".Trim(), manager.Logger.ToString());
			});
		}

		public void TestVGMImport_CoLoadShipment_MBL_GrossWeightVerificationDateTime_DefaultToNow()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;
			consol1.JK_UniqueConsignRef = "C00005000";
			consol1.JK_ConsolMode = Constants.ContainerModes.FCL;

			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container1.JC_ContainerNum = "TCLU3117180";
			var refContainer = newFactory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			AssertNotNull(refContainer);
			container1.JC_RC = refContainer.PK;

			var shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_TransportMode = "SEA";
			shipment1.JS_PackingMode = "FCL";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment1.JS_UniqueConsignRef = "S009900";
			shipment1.JS_HouseBill = "HB003333";

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_UniqueConsignRef = "C00005001";

			var shipment2 = consol2.Shipments.AddNew();
			shipment2.JS_TransportMode = "SEA";
			shipment2.JS_PackingMode = "FCL";
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment2.JS_UniqueConsignRef = "S009901";
			shipment2.JS_HouseBill = "HB003334";

			var consol3 = newFactory.New<ForwardingConsol>();
			consol3.JK_AgentType = Core.Constants.AgentType.Agent;
			consol3.JK_UniqueConsignRef = "C00005002";

			var shipment3 = consol3.Shipments.AddNew();
			shipment3.JS_TransportMode = "SEA";
			shipment3.JS_PackingMode = "FCL";
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment3.JS_UniqueConsignRef = "S009902";
			shipment3.JS_HouseBill = "HB003335";

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>FOR</Code>
				<Description>Forwarder</Description>
				<ServiceCode>VGM</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<WayBillNumber>HB003333</WayBillNumber>
	<WayBillType>
		<Code>HWB</Code>
		<Description>House WayBill</Description>
	</WayBillType>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TCLU3117180</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
			</ContainerType>
			<GrossWeight>7923.000</GrossWeight>
			<GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>PKG</Code>
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

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"
Successfully loaded matching ForwardingShipment.
Populating ForwardingShipment...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Warning - Matching 'GrossWeightVerifiedBy':- No match found for '[Org. Code: TESTORG1; Address 1: Test Address 1]'.
Updated Shipment S009900 (House Bill='HB003333') from UniversalShipment.
Successfully saved Shipment S009900 (House Bill='HB003333') with 1 x ForwardingContainer.
".Trim(), manager.Logger.ToString());

				var container2 = Factory.Load<CommonContainer>(container1.PK);
				AssertNotNull(container2);
				Assert(!container2.JC_GrossWeightVerificationDateTime.IsEmpty);
			});
		}

		public void TestVGMImport_CoLoadShipment_MBL_BookingRef()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;
			consol1.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol1.JK_UniqueConsignRef = "C00005000";
			SetupContainer(consol1);

			var shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_TransportMode = "SEA";
			shipment1.JS_PackingMode = "FCL";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment1.JS_UniqueConsignRef = "S009900";
			shipment1.JS_HouseBill = "HB003333";

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_UniqueConsignRef = "C00005001";

			var shipment2 = consol2.Shipments.AddNew();
			shipment2.JS_TransportMode = "SEA";
			shipment2.JS_PackingMode = "FCL";
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment2.JS_UniqueConsignRef = "S009901";
			shipment2.JS_HouseBill = "HB003333";

			var consol3 = newFactory.New<ForwardingConsol>();
			consol3.JK_AgentType = Core.Constants.AgentType.Agent;
			consol3.JK_UniqueConsignRef = "C00005002";

			var shipment3 = consol3.Shipments.AddNew();
			shipment3.JS_TransportMode = "SEA";
			shipment3.JS_PackingMode = "FCL";
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment3.JS_UniqueConsignRef = "S009902";
			shipment3.JS_HouseBill = "HB003333";

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>FOR</Code>
				<Description>Forwarder</Description>
				<ServiceCode>VGM</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<BookingConfirmationReference>S009900</BookingConfirmationReference>
	<WayBillNumber>HB003333</WayBillNumber>
	<WayBillType>
		<Code>HWB</Code>
		<Description>House WayBill</Description>
	</WayBillType>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TCLU3117199</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
			</ContainerType>
			<GrossWeight>7923.000</GrossWeight>
			<GrossWeightVerificationDateTime>2018-06-06T13:00:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>PKG</Code>
			</GrossWeightVerificationType>
			<Seal>1108829</Seal>
			<WeightUnit>
				<Code>KG</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>TESTORG9</OrganizationCode>
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

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"
Successfully loaded matching ForwardingShipment.
Populating ForwardingShipment...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'TESTORG9' by code, address 'Test Address1' (only address).
Updated Shipment S009900 (House Bill='HB003333') from UniversalShipment.
Successfully saved Shipment S009900 (House Bill='HB003333') with 1 x ForwardingContainer.
".Trim(), manager.Logger.ToString());
			});
		}

		public void TestVGMImport_CoLoadShipment_MBL_DoNotUpdateConsolMBL()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;
			consol1.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol1.JK_UniqueConsignRef = "C00005000";
			consol1.JK_MasterBillNum = ZString.Empty;
			SetupContainer(consol1);

			var shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_TransportMode = "SEA";
			shipment1.JS_PackingMode = "FCL";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment1.JS_UniqueConsignRef = "S009900";
			shipment1.JS_HouseBill = "HB003333";

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_UniqueConsignRef = "C00005001";

			var shipment2 = consol2.Shipments.AddNew();
			shipment2.JS_TransportMode = "SEA";
			shipment2.JS_PackingMode = "FCL";
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment2.JS_UniqueConsignRef = "S009901";
			shipment2.JS_HouseBill = "HB003334";

			var consol3 = newFactory.New<ForwardingConsol>();
			consol3.JK_AgentType = Core.Constants.AgentType.Agent;
			consol3.JK_UniqueConsignRef = "C00005002";

			var shipment3 = consol3.Shipments.AddNew();
			shipment3.JS_TransportMode = "SEA";
			shipment3.JS_PackingMode = "FCL";
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment3.JS_UniqueConsignRef = "S009902";
			shipment3.JS_HouseBill = "HB003335";

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
<Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>FOR</Code>
				<Description>Forwarder</Description>
				<ServiceCode>VGM</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<WayBillNumber>HB003333</WayBillNumber>
	<WayBillType>
		<Code>HWB</Code>
		<Description>House WayBill</Description>
	</WayBillType>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TCLU3117199</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
			</ContainerType>
			<GrossWeight>7923.000</GrossWeight>
			<GrossWeightVerificationDateTime>2018-06-06T13:00:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>PKG</Code>
			</GrossWeightVerificationType>
			<Seal>1108829</Seal>
			<WeightUnit>
				<Code>KG</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>TESTORG9</OrganizationCode>
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

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"
Successfully loaded matching ForwardingShipment.
Populating ForwardingShipment...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'TESTORG9' by code, address 'Test Address1' (only address).
Updated Shipment S009900 (House Bill='HB003333') from UniversalShipment.
Successfully saved Shipment S009900 (House Bill='HB003333') with 1 x ForwardingContainer.
		".Trim(), manager.Logger.ToString());

				var consol1FromAnotherFactory = Factory.Load<ForwardingConsol>(consol1.PK);
				AssertNotNull(consol1FromAnotherFactory);
				AssertEquals("consol JK_MasterBillNum should not be update with shipment MBL.", ZString.Empty, consol1FromAnotherFactory.JK_MasterBillNum);
			});
		}

		public void TestVGMImport_CoLoadShipment_SCAC_MBL()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;
			consol1.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol1.JK_UniqueConsignRef = "C00005000";
			var orgAddress1 = SetupOrgWithSCAC(newFactory, "TestOrg1");
			consol1.JK_OA_SendingForwarderAddress = orgAddress1.PK;
			SetupContainer(consol1);

			var shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_TransportMode = "SEA";
			shipment1.JS_PackingMode = "FCL";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment1.JS_UniqueConsignRef = "S009900";
			shipment1.JS_HouseBill = "HB003333";

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_UniqueConsignRef = "C00005001";
			var orgAddress2 = SetupOrgWithSCAC(newFactory, "TestOrg2", "ABCD");
			consol2.JK_OA_SendingForwarderAddress = orgAddress2.PK;

			var shipment2 = consol2.Shipments.AddNew();
			shipment2.JS_TransportMode = "SEA";
			shipment2.JS_PackingMode = "FCL";
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment2.JS_UniqueConsignRef = "S009901";
			shipment2.JS_HouseBill = "HB003333";

			var consol3 = newFactory.New<ForwardingConsol>();
			consol3.JK_AgentType = Core.Constants.AgentType.Agent;
			consol3.JK_UniqueConsignRef = "C00005002";

			var shipment3 = consol3.Shipments.AddNew();
			shipment3.JS_TransportMode = "SEA";
			shipment3.JS_PackingMode = "FCL";
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment3.JS_UniqueConsignRef = "S009902";
			shipment3.JS_HouseBill = "HB003333";

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>FOR</Code>
				<Description>Forwarder</Description>
				<ServiceCode>VGM</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<WayBillNumber>HB003333</WayBillNumber>
	<WayBillType>
		<Code>HWB</Code>
		<Description>House WayBill</Description>
	</WayBillType>
	<OrganizationAddressCollection>
		<OrganizationAddress>
			<AddressType>ShippingLineAddress</AddressType>
			<RegistrationNumberCollection>
				<RegistrationNumber>
					<Type>
						<Code>CCC</Code>
					</Type>
					<CountryOfIssue>
						<Code>US</Code>
					</CountryOfIssue>
					<Value>YMLU</Value>
				</RegistrationNumber>
			</RegistrationNumberCollection>
		</OrganizationAddress>
	</OrganizationAddressCollection>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TCLU3117199</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
			</ContainerType>
			<GrossWeight>7923.000</GrossWeight>
			<GrossWeightVerificationDateTime>2018-06-06T13:00:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>PKG</Code>
			</GrossWeightVerificationType>
			<Seal>1108829</Seal>
			<WeightUnit>
				<Code>KG</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>TESTORG9</OrganizationCode>
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

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"
Successfully loaded matching ForwardingShipment.
Populating ForwardingShipment...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'TESTORG9' by code, address 'Test Address1' (only address).
Updated Shipment S009900 (House Bill='HB003333') from UniversalShipment.
Successfully saved Shipment S009900 (House Bill='HB003333') with 1 x ForwardingContainer.
".Trim(), manager.Logger.ToString());
			});
		}

		public void TestVGMImport_CoLoadShipment_SCAC_BookingRef()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;
			consol1.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol1.JK_UniqueConsignRef = "C00005000";
			var orgAddress1 = SetupOrgWithSCAC(newFactory, "TestOrg1");
			consol1.JK_OA_SendingForwarderAddress = orgAddress1.PK;
			SetupContainer(consol1);

			var shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_TransportMode = "SEA";
			shipment1.JS_PackingMode = "FCL";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment1.JS_UniqueConsignRef = "S009900";
			shipment1.JS_HouseBill = "HB003333";

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_UniqueConsignRef = "C00005001";
			var orgAddress2 = SetupOrgWithSCAC(newFactory, "TestOrg2", "ABCD");
			consol2.JK_OA_SendingForwarderAddress = orgAddress2.PK;

			var shipment2 = consol2.Shipments.AddNew();
			shipment2.JS_TransportMode = "SEA";
			shipment2.JS_PackingMode = "FCL";
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment2.JS_UniqueConsignRef = "S009901";
			shipment2.JS_HouseBill = "HB003333";

			var consol3 = newFactory.New<ForwardingConsol>();
			consol3.JK_AgentType = Core.Constants.AgentType.Agent;
			consol3.JK_UniqueConsignRef = "C00005002";

			var shipment3 = consol3.Shipments.AddNew();
			shipment3.JS_TransportMode = "SEA";
			shipment3.JS_PackingMode = "FCL";
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment3.JS_UniqueConsignRef = "S009902";
			shipment3.JS_HouseBill = "HB003333";

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>FOR</Code>
				<Description>Forwarder</Description>
				<ServiceCode>VGM</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<BookingConfirmationReference>S009900</BookingConfirmationReference>
	<WayBillType>
		<Code>HWB</Code>
		<Description>House WayBill</Description>
	</WayBillType>
	<OrganizationAddressCollection>
		<OrganizationAddress>
			<AddressType>ShippingLineAddress</AddressType>
			<RegistrationNumberCollection>
				<RegistrationNumber>
					<Type>
						<Code>CCC</Code>
					</Type>
					<CountryOfIssue>
						<Code>US</Code>
					</CountryOfIssue>
					<Value>YMLU</Value>
				</RegistrationNumber>
			</RegistrationNumberCollection>
		</OrganizationAddress>
	</OrganizationAddressCollection>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TCLU3117199</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
			</ContainerType>
			<GrossWeight>7923.000</GrossWeight>
			<GrossWeightVerificationDateTime>2018-06-06T13:00:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>PKG</Code>
			</GrossWeightVerificationType>
			<Seal>1108829</Seal>
			<WeightUnit>
				<Code>KG</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>TESTORG9</OrganizationCode>
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

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"
Successfully loaded matching ForwardingShipment.
Populating ForwardingShipment...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'TESTORG9' by code, address 'Test Address1' (only address).
Updated Shipment S009900 (House Bill='HB003333') from UniversalShipment.
Successfully saved Shipment S009900 (House Bill='HB003333') with 1 x ForwardingContainer.
".Trim(), manager.Logger.ToString());
			});
		}

		public void TestVGMImport_CoLoadShipment_SCAC_MBL_BookingRef()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;
			consol1.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol1.JK_UniqueConsignRef = "C00005000";
			var orgAddress1 = SetupOrgWithSCAC(newFactory, "TestOrg1", "yMLu");
			consol1.JK_OA_SendingForwarderAddress = orgAddress1.PK;
			SetupContainer(consol1);

			var shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_TransportMode = "SEA";
			shipment1.JS_PackingMode = "FCL";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment1.JS_UniqueConsignRef = "S009900";
			shipment1.JS_HouseBill = "HB003333";

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_UniqueConsignRef = "C00005001";
			var orgAddress2 = SetupOrgWithSCAC(newFactory, "TestOrg2", "ABCD");
			consol2.JK_OA_SendingForwarderAddress = orgAddress2.PK;

			var shipment2 = consol2.Shipments.AddNew();
			shipment2.JS_TransportMode = "SEA";
			shipment2.JS_PackingMode = "FCL";
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment2.JS_UniqueConsignRef = "S009901";
			shipment2.JS_HouseBill = "HB003333";

			var consol3 = newFactory.New<ForwardingConsol>();
			consol3.JK_AgentType = Core.Constants.AgentType.Agent;
			consol3.JK_UniqueConsignRef = "C00005002";

			var shipment3 = consol3.Shipments.AddNew();
			shipment3.JS_TransportMode = "SEA";
			shipment3.JS_PackingMode = "FCL";
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment3.JS_UniqueConsignRef = "S009902";
			shipment3.JS_HouseBill = "HB003333";

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>FOR</Code>
				<Description>Forwarder</Description>
				<ServiceCode>VGM</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<BookingConfirmationReference>S009900</BookingConfirmationReference>
	<WayBillNumber>HB003333</WayBillNumber>
	<WayBillType>
		<Code>HWB</Code>
		<Description>House WayBill</Description>
	</WayBillType>
	<OrganizationAddressCollection>
		<OrganizationAddress>
			<AddressType>ShippingLineAddress</AddressType>
			<RegistrationNumberCollection>
				<RegistrationNumber>
					<Type>
						<Code>CCC</Code>
					</Type>
					<CountryOfIssue>
						<Code>US</Code>
					</CountryOfIssue>
					<Value>YMLU</Value>
				</RegistrationNumber>
			</RegistrationNumberCollection>
		</OrganizationAddress>
	</OrganizationAddressCollection>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TCLU3117199</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
			</ContainerType>
			<GrossWeight>7923.000</GrossWeight>
			<GrossWeightVerificationDateTime>2018-06-06T13:00:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>PKG</Code>
			</GrossWeightVerificationType>
			<Seal>1108829</Seal>
			<WeightUnit>
				<Code>KG</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>TESTORG9</OrganizationCode>
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

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"
Successfully loaded matching ForwardingShipment.
Populating ForwardingShipment...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'TESTORG9' by code, address 'Test Address1' (only address).
Updated Shipment S009900 (House Bill='HB003333') from UniversalShipment.
Successfully saved Shipment S009900 (House Bill='HB003333') with 1 x ForwardingContainer.
".Trim(), manager.Logger.ToString());
			});
		}

		OrgAddress SetupOrgWithSCAC(BusinessObjectFactory factory,
			string code = "TestOrg",
			string regNo = "YMLU",
			string regType = OrgCusCode.CodeTypes.CarrierCode,
			string country = Constants.CountryCodes.UnitedStates)
		{
			var org = factory.New<OrgHeader>();
			org.OH_Code = code;
			org.MainAddress.Address1 = "Test Street";
			var orgCusCode = org.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = regType;
			orgCusCode.OK_CustomsRegNo = regNo;
			orgCusCode.OK_RN_NKCodeCountry = country;

			return org.MainAddress;
		}

		ForwardingContainer SetupContainer(ForwardingConsol consol, string containerMode = Constants.ContainerModes.FCL)
		{
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "TCLU3117199";
			container1.JC_ContainerMode = containerMode;
			var refContainer = consol.Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			AssertNotNull(refContainer);
			container1.JC_RC = refContainer.PK;

			var org = consol.Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG9";
			org.MainAddress.Address1 = "Test Address1";
			container1.GrossWeightVerifiedByAddress.OrganisationPK = org.PK;

			return container1;
		}

		#endregion

		#region VGM Import for Applicable ConsolMode/ContainerMode

		public void TestVGMImport_CoLoadShipment_MBL_ContainerMode_FCL()
		{
			AssertVGMImport_CoLoadShipment_MBL_ContainerMode_Applicable(Constants.ContainerModes.FCL);
		}

		public void TestVGMImport_CoLoadShipment_MBL_ContainerMode_GRP()
		{
			AssertVGMImport_CoLoadShipment_MBL_ContainerMode_Applicable(Constants.ContainerModes.Groupage);
		}

		public void TestVGMImport_CoLoadShipment_MBL_ContainerMode_BCN()
		{
			AssertVGMImport_CoLoadShipment_MBL_ContainerMode_Applicable(Constants.ContainerModes.BuyersConsol);
		}

		public void TestVGMImport_CoLoadShipment_MBL_ContainerMode_LCL()
		{
			AssertVGMImport_CoLoadShipment_MBL_ContainerMode_NotApplicable(Constants.ContainerModes.LCL);
		}

		public void TestVGMImport_CoLoadShipment_MBL_ContainerMode_BLK()
		{
			AssertVGMImport_CoLoadShipment_MBL_ContainerMode_NotApplicable(Constants.ContainerModes.Bulk);
		}

		public void TestVGMImport_CoLoadShipment_MBL_ContainerMode_LQD()
		{
			AssertVGMImport_CoLoadShipment_MBL_ContainerMode_NotApplicable(Constants.ContainerModes.Liquid);
		}

		public void TestVGMImport_CoLoadShipment_MBL_ContainerMode_BBK()
		{
			AssertVGMImport_CoLoadShipment_MBL_ContainerMode_NotApplicable(Constants.ContainerModes.BreakBulk);
		}

		public void TestVGMImport_CoLoadShipment_MBL_ContainerMode_ROR()
		{
			AssertVGMImport_CoLoadShipment_MBL_ContainerMode_NotApplicable(Constants.ContainerModes.RollOnRollOff);
		}

		public void TestVGMImport_CoLoadShipment_MBL_ContainerMode_OTH()
		{
			AssertVGMImport_CoLoadShipment_MBL_ContainerMode_NotApplicable(Constants.ContainerModes.Other);
		}

		#region Implementation

		void AssertVGMImport_CoLoadShipment_MBL_ContainerMode_Applicable(ZString containerMode)
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;
			consol1.JK_UniqueConsignRef = "C00005000";
			consol1.JK_ConsolMode = Constants.ContainerModes.FCL;
			SetupContainer(consol1, containerMode);

			var shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_TransportMode = "SEA";
			shipment1.JS_PackingMode = "FCL";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment1.JS_UniqueConsignRef = "S009900";
			shipment1.JS_HouseBill = "HB003333";

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_UniqueConsignRef = "C00005001";

			var shipment2 = consol2.Shipments.AddNew();
			shipment2.JS_TransportMode = "SEA";
			shipment2.JS_PackingMode = "FCL";
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment2.JS_UniqueConsignRef = "S009901";
			shipment2.JS_HouseBill = "HB003334";

			var consol3 = newFactory.New<ForwardingConsol>();
			consol3.JK_AgentType = Core.Constants.AgentType.Agent;
			consol3.JK_UniqueConsignRef = "C00005002";

			var shipment3 = consol3.Shipments.AddNew();
			shipment3.JS_TransportMode = "SEA";
			shipment3.JS_PackingMode = "FCL";
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment3.JS_UniqueConsignRef = "S009902";
			shipment3.JS_HouseBill = "HB003335";

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>FOR</Code>
				<Description>Forwarder</Description>
				<ServiceCode>VGM</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<WayBillNumber>HB003333</WayBillNumber>
	<WayBillType>
		<Code>HWB</Code>
		<Description>House WayBill</Description>
	</WayBillType>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TCLU3117199</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
			</ContainerType>
			<GrossWeight>7923.000</GrossWeight>
			<GrossWeightVerificationDateTime>2018-06-06T13:00:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>PKG</Code>
			</GrossWeightVerificationType>
			<Seal>1108829</Seal>
			<WeightUnit>
				<Code>KG</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>TESTORG9</OrganizationCode>
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

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"
Successfully loaded matching ForwardingShipment.
Populating ForwardingShipment...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'TESTORG9' by code, address 'Test Address1' (only address).
Updated Shipment S009900 (House Bill='HB003333') from UniversalShipment.
Successfully saved Shipment S009900 (House Bill='HB003333') with 1 x ForwardingContainer.
".Trim(), manager.Logger.ToString());
			});
		}

		void AssertVGMImport_CoLoadShipment_MBL_ContainerMode_NotApplicable(ZString containerMode)
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;
			consol1.JK_UniqueConsignRef = "C00005000";
			consol1.JK_ConsolMode = Constants.ContainerModes.FCL;
			SetupContainer(consol1, containerMode);

			var shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_TransportMode = "SEA";
			shipment1.JS_PackingMode = "FCL";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment1.JS_UniqueConsignRef = "S009900";
			shipment1.JS_HouseBill = "HB003333";

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_UniqueConsignRef = "C00005001";

			var shipment2 = consol2.Shipments.AddNew();
			shipment2.JS_TransportMode = "SEA";
			shipment2.JS_PackingMode = "FCL";
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment2.JS_UniqueConsignRef = "S009901";
			shipment2.JS_HouseBill = "HB003334";

			var consol3 = newFactory.New<ForwardingConsol>();
			consol3.JK_AgentType = Core.Constants.AgentType.Agent;
			consol3.JK_UniqueConsignRef = "C00005002";

			var shipment3 = consol3.Shipments.AddNew();
			shipment3.JS_TransportMode = "SEA";
			shipment3.JS_PackingMode = "FCL";
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment3.JS_UniqueConsignRef = "S009902";
			shipment3.JS_HouseBill = "HB003335";

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>FOR</Code>
				<Description>Forwarder</Description>
				<ServiceCode>VGM</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<WayBillNumber>HB003333</WayBillNumber>
	<WayBillType>
		<Code>HWB</Code>
		<Description>House WayBill</Description>
	</WayBillType>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TCLU3117199</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
			</ContainerType>
			<GrossWeight>7923.000</GrossWeight>
			<GrossWeightVerificationDateTime>2018-06-06T13:00:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>PKG</Code>
			</GrossWeightVerificationType>
			<Seal>1108829</Seal>
			<WeightUnit>
				<Code>KG</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>TESTORG9</OrganizationCode>
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

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Error, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"
Successfully loaded matching ForwardingShipment.
Error - Cannot populate ForwardingShipment because:
Container Mode should be FCL, GRP or BCN.
Successfully saved, but nothing was reported as being updated.
".Trim(), manager.Logger.ToString());
			});
		}

		#endregion

		#endregion

		#region NVOCC Import

		public void TestNVOCC_BookingPartyIsMissing()
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
				<ServiceCode>SIN</ServiceCode>
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
Error - Cannot populate ForwardingShipment because:
[*Shipping instruction message received without Booking Party, Booking Party is mandatory to process the Shipping instruction, message rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestNVOCC_BookingNumberIsMissing()
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
				<ServiceCode>SIN</ServiceCode>
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
Error - Cannot populate ForwardingShipment because:
[*Shipping instruction message received without Booking Number, Booking Number is mandatory to process the Shipping instruction, message rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestNVOCC_BookingPartyDocumentaryAddressDoesNotMatch()
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
				<ServiceCode>SIN</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>A</CoLoadBookingConfirmationReference>
	<AgentsReference>AGENTREF</AgentsReference>
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
Error - Cannot populate ForwardingShipment because:
[*Shipping instruction message received from an unknown Booking Party TESTCOMPANY, message rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestNVOCC_BookingOrShipmentDoesNotExist()
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
				<ServiceCode>SIN</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00001000</CoLoadBookingConfirmationReference>
	<AgentsReference>AGENTREF</AgentsReference>
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
Error - Cannot populate ForwardingShipment because:
[*Shipping instruction message with Booking Number S00001000 cannot find a matching Booking, message rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestNVOCC_BookingConvertedToShipment()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var booking1 = Factory.New<ForwardingShipment>();
			booking1.JS_UniqueConsignRef = "S00001000";
			booking1.JS_HouseBill = "BOOK";
			booking1.JS_BookingReference = "AGENTREF";
			booking1.JS_IsForwardRegistered = false;
			booking1.JS_IsBooking = true;
			booking1.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

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
				<ServiceCode>SIN</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00001000</CoLoadBookingConfirmationReference>
	<CoLoadMasterBillNumber>BOOK</CoLoadMasterBillNumber>
	<AgentsReference>AGENTREF</AgentsReference>
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
Successfully loaded matching ForwardingShipment.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Populating ForwardingShipment...
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Updated Shipment S00001000 (House Bill='BOOK') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='BOOK').
".Trim(), message.GetLogNoteText());

			var factory2 = new BusinessObjectFactory();
			var booking2 = factory2.Load<ForwardingShipment>(booking1.PK);
			AssertEquals("Booking should be converted to shipment", true, booking2.JS_IsForwardRegistered);
		}

		[ExpectNoExceptions]
		public void TestNVOCC_BookingConvertedToShipment_DoNotCreateConsolAndImportContainers()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var booking1 = Factory.New<ForwardingShipment>();
			booking1.JS_UniqueConsignRef = "S00001000";
			booking1.JS_HouseBill = "BOOK";
			booking1.JS_BookingReference = "AGENTREF";
			booking1.JS_IsForwardRegistered = false;
			booking1.JS_IsBooking = true;
			booking1.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

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
				<ServiceCode>SIN</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00001000</CoLoadBookingConfirmationReference>
	<CoLoadMasterBillNumber>BOOK</CoLoadMasterBillNumber>
	<AgentsReference>AGENTREF</AgentsReference>
	<OrganizationAddressCollection>
		<OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
		</OrganizationAddress>
	</OrganizationAddressCollection>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TCLU3117199</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
			</ContainerType>
			<GrossWeight>7923.000</GrossWeight>
			<GrossWeightVerificationDateTime>2018-06-06T13:00:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>PKG</Code>
			</GrossWeightVerificationType>
			<Seal>1108829</Seal>
			<WeightUnit>
				<Code>KG</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>TESTORG9</OrganizationCode>
					<Address1>Test Address 1</Address1>
				</OrganizationAddress>
			</OrganizationAddressCollection>
		</Container>
	</ContainerCollection>
	</Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching ForwardingShipment.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Populating ForwardingShipment...
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Updated Shipment S00001000 (House Bill='BOOK') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='BOOK').
".Trim(), message.GetLogNoteText());

			var factory2 = new BusinessObjectFactory();
			var shipment2 = factory2.Load<ForwardingShipment>(booking1.PK);
			AssertEquals("Booking should be converted to shipment", true, shipment2.JS_IsForwardRegistered);
			AssertEquals("No consol will be created and no container will be added to consol.", 0, shipment2.Consols.Count);
		}

		public void TestNVOCC_BookingConvertedToShipment_WhenWayBillTypeIsMaster()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var booking1 = Factory.New<ForwardingShipment>();
			booking1.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			booking1.JS_HouseBill = "BOOK";
			booking1.JS_UniqueConsignRef = "S00001000";
			booking1.JS_BookingReference = "AGENTREF";
			booking1.JS_IsForwardRegistered = false;
			booking1.JS_IsBooking = true;
			booking1.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
	<Shipment>
	<DataContext>
		<DocumentaryOverride>
			<DataVersion>7</DataVersion>
			<Purpose>
				<Code>AMD</Code>
				<Description>Amendment</Description>
			</Purpose>
		</DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>SIN</ServiceCode>
				<ServiceDescription>Shipping Instruction</ServiceDescription>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00001000</CoLoadBookingConfirmationReference>
	<CoLoadMasterBillNumber>BOOK</CoLoadMasterBillNumber>
	<AgentsReference>AGENTREF</AgentsReference>
	<WayBillNumber>BOOK</WayBillNumber>
	<WayBillType>
		<Code>MWB</Code>
		<Description>Master Waybill</Description>
	</WayBillType>
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
Successfully loaded matching ForwardingShipment.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Populating ForwardingShipment...
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Updated Shipment S00001000 (House Bill='BOOK') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='BOOK').
".Trim(), message.GetLogNoteText());

			var factory2 = new BusinessObjectFactory();
			var booking2 = factory2.Load<ForwardingShipment>(booking1.PK);
			AssertEquals("Booking should be converted to shipment", true, booking2.JS_IsForwardRegistered);
		}

		public void TestNVOCC_BillOfLadingDataPopulation()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var booking1 = Factory.New<ForwardingShipment>();
			booking1.JS_UniqueConsignRef = "S00001000";
			booking1.JS_HouseBill = "BOOK";
			booking1.JS_BookingReference = "AGENTREF";
			booking1.JS_RL_NKLoadPort = "AUBNE";
			booking1.JS_RL_NKDestination = "HKHKG";
			booking1.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

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
					<ServiceCode>SIN</ServiceCode>
				</RecipientRole>
			</RecipientRoleCollection>
		</DataContext>
		<CoLoadBookingConfirmationReference>S00001000</CoLoadBookingConfirmationReference>
		<CoLoadMasterBillNumber>BOOK</CoLoadMasterBillNumber>
		<AgentsReference>AGENTREF</AgentsReference>
		<PortOfOrigin>
			<Code>KRSEL</Code>
			<Name>Seoul</Name>
		</PortOfOrigin>
		<PortOfLoading>
			<Code>AUSYD</Code>
			<Name>Sydney</Name>
		</PortOfLoading>
		<PortOfDestination>
			<Code>AUBNE</Code>
			<Name>Brisbane</Name>
		</PortOfDestination>
		<PortOfDischarge>
			<Code>NZAKL</Code>
			<Name>Auckland</Name>
		</PortOfDischarge>
		<OrganizationAddressCollection>
			<OrganizationAddress>
				<AddressType>BookingPartyDocumentaryAddress</AddressType>
				<OrganizationCode>BKGPARTY</OrganizationCode>
				<Address1>Booking Party Address 1</Address1>
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>ConsignorDocumentaryAddress</AddressType>
				<Address1>46, MOONJEONG-DONG</Address1>
				<Address2>SONGPA-KU SEOUL, 138-200</Address2>
				<AddressOverride>false</AddressOverride>
				<AddressShortCode>Pickup and Delivery Addre</AddressShortCode>
				<City>KOREA</City>
				<CompanyName>DAE-IL CORPORATION</CompanyName>
				<Country>
					<Code>KR</Code>
					<Name>Korea, Republic of</Name>
				</Country>
				<Email/>
				<Fax/>
				<OrganizationCode>DAECOR</OrganizationCode>
				<Phone>02 3333 4444</Phone>
				<Port>
					<Code>KRSEL</Code>
					<Name>Seoul</Name>
				</Port>
				<Postcode>2000</Postcode>
				<ScreeningStatus>
					<Code>UNK</Code>
					<Description>Unknown</Description>
				</ScreeningStatus>
				<State>11</State>
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>ConsigneeDocumentaryAddress</AddressType>
				<Address1>UNIT 3 CENTRE PARK</Address1>
				<Address2>211 BRISBANE ROAD                        LABRADOR</Address2>
				<AddressOverride>false</AddressOverride>
				<AddressShortCode>Delivery Address</AddressShortCode>
				<City>QLD LABRADOR</City>
				<CompanyName>GALA BRAS</CompanyName>
				<Country>
					<Code>AU</Code>
					<Name>Australia</Name>
				</Country>
				<Email/>
				<Fax/>
				<OrganizationCode>GALBRA</OrganizationCode>
				<Phone>02 5555 6666</Phone>
				<Port>
					<Code>AUBNE</Code>
					<Name>Brisbane</Name>
				</Port>
				<Postcode>4215</Postcode>
				<ScreeningStatus>
					<Code>UNK</Code>
					<Description>Unknown</Description>
				</ScreeningStatus>
				<State>QLD</State>
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>NotifyParty</AddressType>
				<Address1>XIAMEN CHINA</Address1>
				<Address2/>
				<AddressOverride>false</AddressOverride>
				<AddressShortCode>PST: XIAMEN CHINA</AddressShortCode>
				<City>HONGKONG</City>
				<CompanyName>XIAMEN C &amp; D INC 5-7TH FLR SEASIDE BLDG</CompanyName>
				<Country>
					<Code>HK</Code>
					<Name>Hong Kong</Name>
				</Country>
				<Email/>
				<Fax/>
				<OrganizationCode>XIAMEN</OrganizationCode>
				<Phone>02 7777 8888</Phone>
				<Port>
					<Code>HKHKG</Code>
					<Name>Hong Kong</Name>
				</Port>
				<Postcode/>
				<ScreeningStatus>
					<Code>UNK</Code>
					<Description>Unknown</Description>
				</ScreeningStatus>
				<State/>
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>NotifyParty2</AddressType>
				<Address1>VIA PETTINI 13</Address1>
				<Address2>RIMINI</Address2>
				<AddressOverride>false</AddressOverride>
				<AddressShortCode>PST: VIA PETTINI 13</AddressShortCode>
				<City>RIMINI</City>
				<CompanyName>MAQBO SAS</CompanyName>
				<Country>
					<Code>IT</Code>
					<Name>Italy</Name>
				</Country>
				<Email/>
				<Fax/>
				<OrganizationCode>MABSAS</OrganizationCode>
				<Phone>+390244445555</Phone>
				<Port>
					<Code>ITTRS</Code>
					<Name>Trieste</Name>
				</Port>
				<Postcode>47853</Postcode>
				<ScreeningStatus>
					<Code>UNK</Code>
					<Description>Unknown</Description>
				</ScreeningStatus>
				<State>RN</State>
			</OrganizationAddress>
		</OrganizationAddressCollection>
		<DateCollection>
			<Date>
				<Type>BillIssued</Type>
				<IsEstimate>false</IsEstimate>
				<Value>2018-10-19T10:23:00</Value>
			</Date>
		</DateCollection>
	</Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching ForwardingShipment.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Populating ForwardingShipment...
Matching 'ConsignorDocumentaryAddress':- Matched to 'DAECOR' by code, address 'Pickup and Delivery Addre' by short code.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'GALBRA' by code, address 'Delivery Address' by short code.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Matching 'NotifyParty':- Matched to 'XIAMEN' by code, address 'PST: XIAMEN CHINA' by short code.
Matching 'NotifyParty2':- Matched to 'MABSAS' by code, address 'PST: VIA PETTINI 13' by short code.
Matching 'ConsignorDocumentaryAddress':- Matched to 'DAECOR' by code, address 'Pickup and Delivery Addre' by short code.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'GALBRA' by code, address 'Delivery Address' by short code.
Matching 'NotifyParty':- Matched to 'XIAMEN' by code, address 'PST: XIAMEN CHINA' by short code.
Matching 'NotifyParty2':- Matched to 'MABSAS' by code, address 'PST: VIA PETTINI 13' by short code.
Updated Shipment S00001000 (House Bill='BOOK') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='BOOK').
".Trim(), message.GetLogNoteText());

			var factory2 = new BusinessObjectFactory();
			var booking2 = factory2.Load<ForwardingShipment>(booking1.PK);
			var docNote = DocumentNote.LoadNote(booking2);
			AssertNotNull(docNote);

			var originPort = docNote.GetFieldValueAsString("Origin Port - Place Of Receipt");
			AssertEquals("Seoul", originPort);
			var destinationPort = docNote.GetFieldValueAsString("Destination Port - Place Of Delivery");
			AssertEquals("Brisbane", destinationPort);
			var portOfLoading = docNote.GetFieldValueAsString("Port Of Loading");
			AssertEquals("Sydney", portOfLoading);
			var portOfDischarge = docNote.GetFieldValueAsString("Port Of Discharge");
			AssertEquals("Auckland", portOfDischarge);

			var dateOfIssueDate = docNote.GetFieldValueAsString("Date of Issue");
			AssertNull("Field not available in Unit Test", dateOfIssueDate);

			var shipperAddress = docNote.GetFieldValueAsString("Consignor - Shipper");
			AssertEquals(@"DAE-IL CORPORATION
46, MOONJEONG-DONG
SONGPA-KU SEOUL, 138-200
KOREA
KOREA, REPUBLIC OF", shipperAddress);

			var shipperPhone = docNote.GetFieldValueAsString("Consignor - Shipper Phone");
			AssertEquals(@"02 3333 4444", shipperPhone);

			var consigneeAddress = docNote.GetFieldValueAsString("Consignee - Importer");
			AssertEquals(@"GALA BRAS
UNIT 3 CENTRE PARK
211 BRISBANE ROAD LABRADOR
QLD LABRADOR QLD 4215
AUSTRALIA", consigneeAddress);

			var consigneePhone = docNote.GetFieldValueAsString("Consignee - consignee Phone");
			AssertNull("Field not available in Unit Test", consigneePhone);

			var notifyPartyAddress = docNote.GetFieldValueAsString("Notify Party");
			AssertEquals(@"XIAMEN C & D INC 5-7TH FLR SEASIDE BLDG
XIAMEN CHINA
HONG KONG", notifyPartyAddress);

			var notifyPhone = docNote.GetFieldValueAsString("Notify - Notify Phone");
			AssertNull("Field not available in Unit Test", notifyPhone);

			var notifyParty2Address = docNote.GetFieldValueAsString("Also Notify");
			AssertNull(@"Field not available in Unit Test", notifyParty2Address);
		}

		public void TestNVOCC_StatusUpdatedEventForOriginalMessage()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var bookingParty = Factory.New<OrgHeader>();
				bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
				bookingParty.OH_Code = "BKGPARTY";
				bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

				var booking1 = Factory.New<ForwardingShipment>();
				booking1.JS_ShipmentStatus = ShipmentStatusList.Codes.SIRejected;
				booking1.JS_HouseBill = "BOOK";
				booking1.JS_BookingReference = "AGENTREF";
				booking1.JS_UniqueConsignRef = "S00001000";
				booking1.JS_RL_NKLoadPort = "AUBNE";
				booking1.JS_RL_NKDestination = "HKHKG";
				booking1.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

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
				<ServiceCode>SIN</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00001000</CoLoadBookingConfirmationReference>
	<CoLoadMasterBillNumber>BOOK</CoLoadMasterBillNumber>
	<AgentsReference>AGENTREF</AgentsReference>
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
Successfully loaded matching ForwardingShipment.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Populating ForwardingShipment...
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Updated Shipment S00001000 (House Bill='BOOK') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='BOOK').
".Trim(), message.GetLogNoteText());

				var factory2 = new BusinessObjectFactory();
				var booking2 = factory2.Load<ForwardingShipment>(booking1.PK);
				var log = booking2.Logs.MostRecentLogByEventTime(Events.StatusUpdated);
				CombineAssertions(() =>
				{
					AssertNotNull(log);
					AssertEquals("ShipmentStatus", ShipmentStatusList.Codes.ElectronicShippingInstruction, booking2.JS_ShipmentStatus);
					AssertEquals("Free text", "Original", log.ReferenceFreeText);
					AssertEquals("Event new status", ShipmentStatusList.Codes.ElectronicShippingInstruction, log.Parameters[Params.New]);
					AssertEquals("Event old status", ShipmentStatusList.Codes.SIRejected, log.Parameters[Params.Old]);
					AssertEquals("Event reason", "Electronic Shipping Instruction Received", log.Parameters[Params.Reason]);
					AssertEquals("Event type", "Shipment Status", log.Parameters[Params.Type]);
				});
			}
		}

		public void TestNVOCC_ShipmentIsUpdated_WhenUniversalXMLUseCombinedReferenceAndPartyIDMatchIsEnabled()
		{
			using (eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var bookingParty = Factory.New<OrgHeader>();
				bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
				bookingParty.OH_Code = "BKGPARTY";
				bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

				var booking1 = Factory.New<ForwardingShipment>();
				booking1.JS_ShipmentStatus = ShipmentStatusList.Codes.SIRejected;
				booking1.JS_HouseBill = "BOOK";
				booking1.JS_BookingReference = "AGENTREF";
				booking1.JS_UniqueConsignRef = "S00001000";
				booking1.JS_RL_NKLoadPort = "AUBNE";
				booking1.JS_RL_NKDestination = "HKHKG";
				booking1.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

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
				<ServiceCode>SIN</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00001000</CoLoadBookingConfirmationReference>
	<CoLoadMasterBillNumber>BOOK</CoLoadMasterBillNumber>
	<AgentsReference>AGENTREF</AgentsReference>
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
Successfully loaded matching ForwardingShipment.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Populating ForwardingShipment...
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Updated Shipment S00001000 (House Bill='BOOK') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='BOOK').
".Trim(), message.GetLogNoteText());
			}
		}

		public void TestNVOCC_StatusUpdatedEventForAmendmentMessage()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var bookingParty = Factory.New<OrgHeader>();
				bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
				bookingParty.OH_Code = "BKGPARTY";
				bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

				var booking1 = Factory.New<ForwardingShipment>();
				booking1.JS_ShipmentStatus = ShipmentStatusList.Codes.SIRejected;
				booking1.JS_UniqueConsignRef = "S00001000";
				booking1.JS_HouseBill = "BOOK";
				booking1.JS_BookingReference = "AGENTREF";
				booking1.JS_RL_NKLoadPort = "AUBNE";
				booking1.JS_RL_NKDestination = "HKHKG";
				booking1.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

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
				<ServiceCode>SIN</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00001000</CoLoadBookingConfirmationReference>
	<CoLoadMasterBillNumber>BOOK</CoLoadMasterBillNumber>
	<AgentsReference>AGENTREF</AgentsReference>
	<WayBillNumber>BOOK</WayBillNumber>
	<WayBillType>
		<Code>HWB</Code>
		<Description>House WayBill</Description>
	</WayBillType>
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
Successfully loaded matching ForwardingShipment.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Populating ForwardingShipment...
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Updated Shipment S00001000 (House Bill='BOOK') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='BOOK').
".Trim(), message.GetLogNoteText());

				var factory2 = new BusinessObjectFactory();
				var booking2 = factory2.Load<ForwardingShipment>(booking1.PK);
				var log = booking2.Logs.MostRecentLogByEventTime(Events.StatusUpdated);
				CombineAssertions(() =>
				{
					AssertNotNull(log);
					AssertEquals("ShipmentStatus", ShipmentStatusList.Codes.ElectronicShippingInstruction, booking2.JS_ShipmentStatus);
					AssertEquals("Free text", "Amendment", log.ReferenceFreeText);
					AssertEquals("Event new status", ShipmentStatusList.Codes.ElectronicShippingInstruction, log.Parameters[Params.New]);
					AssertEquals("Event old status", ShipmentStatusList.Codes.SIRejected, log.Parameters[Params.Old]);
					AssertEquals("Event reason", "Electronic Shipping Instruction Received", log.Parameters[Params.Reason]);
					AssertEquals("Event type", "Shipment Status", log.Parameters[Params.Type]);
				});
			}
		}

		public void TestNVOCC_Original_JS_ShipmentStatusIsNotBookedOrElectronicShippingInstructionOrSIRejected()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var booking1 = Factory.New<ForwardingShipment>();
			booking1.JS_UniqueConsignRef = "S00001000";
			booking1.JS_ShipmentStatus = ShipmentStatusList.Codes.BookingRejected;
			booking1.JS_HouseBill = "BOOK";
			booking1.JS_BookingReference = "AGENTREF";
			booking1.JS_RL_NKLoadPort = "AUBNE";
			booking1.JS_RL_NKDestination = "HKHKG";
			booking1.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

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
				<ServiceCode>SIN</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00001000</CoLoadBookingConfirmationReference>
	<CoLoadMasterBillNumber>BOOK</CoLoadMasterBillNumber>
	<AgentsReference>AGENTREF</AgentsReference>
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
Successfully loaded matching ForwardingShipment.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate ForwardingShipment because:
[*Shipping instruction cannot be processed, because Booking S00001000 is not confirmed by carrier, message rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestNVOCC_Original_JS_ShipmentStatusIsAmendment()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var booking1 = Factory.New<ForwardingShipment>();
			booking1.JS_UniqueConsignRef = "S00001000";
			booking1.JS_ShipmentStatus = ShipmentStatusList.Codes.Amendment;
			booking1.JS_HouseBill = "BOOK";
			booking1.JS_BookingReference = "AGENTREF";
			booking1.JS_RL_NKLoadPort = "AUBNE";
			booking1.JS_RL_NKDestination = "HKHKG";
			booking1.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

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
				<ServiceCode>SIN</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00001000</CoLoadBookingConfirmationReference>
	<CoLoadMasterBillNumber>BOOK</CoLoadMasterBillNumber>
	<AgentsReference>AGENTREF</AgentsReference>
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
Successfully loaded matching ForwardingShipment.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Populating ForwardingShipment...
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Updated Shipment S00001000 (House Bill='BOOK') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='BOOK').
".Trim(), message.GetLogNoteText());
		}

		public void TestNVOCC_Amendment_JS_ShipmentStatusIsNotConfirmedOrSIRejected()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var booking1 = Factory.New<ForwardingShipment>();
			booking1.JS_UniqueConsignRef = "S00001000";
			booking1.JS_ShipmentStatus = ShipmentStatusList.Codes.BookingRejected;
			booking1.JS_BookingReference = "AGENTREF";
			booking1.JS_HouseBill = "BOOK";
			booking1.JS_RL_NKLoadPort = "AUBNE";
			booking1.JS_RL_NKDestination = "HKHKG";
			booking1.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

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
				<ServiceCode>SIN</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00001000</CoLoadBookingConfirmationReference>
	<CoLoadMasterBillNumber>BOOK</CoLoadMasterBillNumber>
	<AgentsReference>AGENTREF</AgentsReference>
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
Successfully loaded matching ForwardingShipment.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate ForwardingShipment because:
[*Shipping instruction Amendment message with Bill Number BOOK cannot process. Shipping instruction not confirmed, message rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestNVOCC_AgentsReferenceIsEmpty()
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
				<Description>Amendment</Description>
			</Purpose>
		 </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>SIN</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00001000</CoLoadBookingConfirmationReference>
	<CoLoadMasterBillNumber>BOOK</CoLoadMasterBillNumber>
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
Error - Cannot populate ForwardingShipment because:
[*Shipping instruction message received without Shipper's Reference, Shipper's Reference is mandatory to process the Shipping instruction, message rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestNVOCC_Amendment_CoLoadMasterBillNumberIsEmpty()
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
				<Description>Amendment</Description>
			</Purpose>
		 </DocumentaryOverride>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>NVO</Code>
				<Description>NVOCC</Description>
				<ServiceCode>SIN</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00001000</CoLoadBookingConfirmationReference>
	<AgentsReference>BOOK1</AgentsReference>
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
Error - Cannot populate ForwardingShipment because:
[*Shipping instruction Amendment message received without Bill Number, Bill Number is mandatory to process the Shipping instruction Amendment, message rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestNVOCC_BookingPartyIsNotMatched()
		{
			var bookingParty1 = Factory.New<OrgHeader>();
			bookingParty1.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty1.OH_Code = "BKGPARTY";
			bookingParty1.OH_FullName = "BKG COMPANY PTY LTD";

			var bookingParty2 = Factory.New<OrgHeader>();
			bookingParty2.MainAddress.OA_Address1 = "Booking Party Address 2";
			bookingParty2.OH_Code = "PARTY2";
			bookingParty2.OH_FullName = "COMPANY 2";

			var booking1 = Factory.New<ForwardingShipment>();
			booking1.JS_UniqueConsignRef = "S00001000";
			booking1.JS_HouseBill = "BOOK";
			booking1.JS_BookingReference = "AGENTREF";
			booking1.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty2.MainAddress.PK;

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
				<ServiceCode>SIN</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00001000</CoLoadBookingConfirmationReference>
	<CoLoadMasterBillNumber>BOOK</CoLoadMasterBillNumber>
	<AgentsReference>AGENTREF</AgentsReference>
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
Successfully loaded matching ForwardingShipment.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate ForwardingShipment because:
[*Shipping instruction message received with a wrong Booking Party TESTCOMPANY, message rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestNVOCC_Original_CoLoadMasterBillNumberDoesNotMatchHouseBill()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var booking1 = Factory.New<ForwardingShipment>();
			booking1.JS_UniqueConsignRef = "S00001000";
			booking1.JS_HouseBill = "BOOK";
			booking1.JS_BookingReference = "AGENTREF";
			booking1.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicShippingInstruction;
			booking1.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

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
				<ServiceCode>SIN</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00001000</CoLoadBookingConfirmationReference>
	<CoLoadMasterBillNumber>CAR</CoLoadMasterBillNumber>
	<AgentsReference>AGENTREF</AgentsReference>
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
Successfully loaded matching ForwardingShipment.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate ForwardingShipment because:
[*Shipping instruction Original message with Bill Number CAR cannot find a matching Shipment to update, message rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestNVOCC_Amendment_CoLoadMasterBillNumberDoesNotMatchHouseBill()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var booking1 = Factory.New<ForwardingShipment>();
			booking1.JS_UniqueConsignRef = "S00001000";
			booking1.JS_HouseBill = "HB003333";
			booking1.JS_BookingReference = "AGENTREF";
			booking1.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			booking1.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

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
				<ServiceCode>SIN</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00001000</CoLoadBookingConfirmationReference>
	<CoLoadMasterBillNumber>CAR</CoLoadMasterBillNumber>
	<AgentsReference>AGENTREF</AgentsReference>
	<WayBillNumber>HB003333</WayBillNumber>
	<WayBillType>
		<Code>HWB</Code>
		<Description>House WayBill</Description>
	</WayBillType>
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
Successfully loaded matching ForwardingShipment.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate ForwardingShipment because:
[*Shipping instruction Amendment message with Bill Number CAR cannot find a matching Shipment to update, message rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestNVOCC_Amendment_CoLoadMasterBillNumberDoesNotMatchHouseBillAndStatusIsNotConfirmed()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var booking1 = Factory.New<ForwardingShipment>();
			booking1.JS_UniqueConsignRef = "S00001000";
			booking1.JS_HouseBill = "HB003333";
			booking1.JS_BookingReference = "AGENTREF";
			booking1.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicShippingInstruction;
			booking1.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

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
				<ServiceCode>SIN</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00001000</CoLoadBookingConfirmationReference>
	<CoLoadMasterBillNumber>CAR</CoLoadMasterBillNumber>
	<AgentsReference>AGENTREF</AgentsReference>
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
Successfully loaded matching ForwardingShipment.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate ForwardingShipment because:
[*Shipping instruction Amendment message with Bill Number CAR cannot find a matching Shipment to update, message rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestNVOCC_BookingNumberDoesNotMatchJS_UniqueConsignRef()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var booking1 = Factory.New<ForwardingShipment>();
			booking1.JS_UniqueConsignRef = "S00001000";
			booking1.JS_HouseBill = "HB003333";
			booking1.JS_BookingReference = "AGENTSREF";
			booking1.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			booking1.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

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
				<ServiceCode>SIN</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00002000</CoLoadBookingConfirmationReference>
	<CoLoadMasterBillNumber>HB003333</CoLoadMasterBillNumber>
	<AgentsReference>AGENTSREF</AgentsReference>
	<WayBillNumber>HB003333</WayBillNumber>
	<WayBillType>
		<Code>HWB</Code>
		<Description>House WayBill</Description>
	</WayBillType>
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
Successfully loaded matching ForwardingShipment.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate ForwardingShipment because:
[*Shipping instruction message with Booking Number S00002000 cannot find a matching Booking, message rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestNVOCC_ShipmentStatusIsCancelledMessageRejection()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var booking1 = Factory.New<ForwardingShipment>();
			booking1.JS_HouseBill = "BOOK";
			booking1.JS_BookingReference = "CAR";
			booking1.JS_IsForwardRegistered = false;
			booking1.JS_IsBooking = true;
			booking1.JS_ShipmentStatus = ShipmentStatusList.Codes.BookingCancelled;
			booking1.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

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
				<ServiceCode>SIN</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00001000</CoLoadBookingConfirmationReference>
	<CoLoadMasterBillNumber>BOOK</CoLoadMasterBillNumber>
	<AgentsReference>AGENTREF</AgentsReference>
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
Successfully loaded matching ForwardingShipment.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate ForwardingShipment because:
[*Shipping Instruction message cannot be accepted once Booking is Canceled. message rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestNVOCC_ShipmentStatusIsCancellationReceivedMessageRejection()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var booking1 = Factory.New<ForwardingShipment>();
			booking1.JS_HouseBill = "BOOK";
			booking1.JS_BookingReference = "CAR";
			booking1.JS_IsForwardRegistered = false;
			booking1.JS_IsBooking = true;
			booking1.JS_ShipmentStatus = ShipmentStatusList.Codes.EBookingCancellationRequest;
			booking1.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

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
				<ServiceCode>SIN</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<CoLoadBookingConfirmationReference>S00001000</CoLoadBookingConfirmationReference>
	<CoLoadMasterBillNumber>BOOK</CoLoadMasterBillNumber>
	<AgentsReference>AGENTREF</AgentsReference>
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
Successfully loaded matching ForwardingShipment.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate ForwardingShipment because:
[*Shipping Instruction message cannot be accepted once Booking Withdrawal/Cancellation request is in progress. message rejected.*]
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestNVOCC_ForActionLinkOnly()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001111";
			shipment.JS_BookingReference = "C00001111";

			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";
			shipment.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
	<Shipment>
	<DataContext>
		<Action>LinkOnly</Action>
		<DocumentaryOverride>
		<DocumentName>Shipping Instruction</DocumentName>
		</DocumentaryOverride>
		<DataTargetCollection>
		<DataTarget>
			<Type>ForwardingShipment</Type>
		</DataTarget>
		</DataTargetCollection>
		<RecipientRoleCollection>
		<RecipientRole>
			<Code>NVO</Code>
			<Description>NVOCC</Description>
			<ServiceCode>SIN</ServiceCode>
		</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<AgentsReference>C00001111</AgentsReference>
	<CoLoadBookingConfirmationReference>S00001111</CoLoadBookingConfirmationReference>
	<CoLoadMasterBillNumber>VERSJU2104661454</CoLoadMasterBillNumber>
	<OrganizationAddressCollection>
		<OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<Address1>Booking Party Address 1</Address1>
		<Address2 />
		<AddressOverride>false</AddressOverride>
		<City>PORT MELBOURNE</City>
		<CompanyName>BKG COMPANY PTY LTD</CompanyName>
		<Contact>CargoWise Support</Contact>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		</OrganizationAddress>
	</OrganizationAddressCollection>
	</Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Shipment status of Shipment S00001111 has been updated.
Universal Shipment data was linked to Shipment S00001111.
Successfully saved, but nothing was reported as being updated.".Trim(), message.GetLogNoteText());
			var shipmentInDB = new BusinessObjectFactory().LoadTop1<ForwardingShipment>(new ZQuery());
			AssertEquals("JS_ShipmentStatus is updated.", ShipmentStatusList.Codes.ElectronicShippingInstruction, shipmentInDB.JS_ShipmentStatus);
			AssertNotNull("STU event is added.", shipmentInDB.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(s => s.SL_SE_NKEvent == Events.StatusUpdatedCode && !s.IsCancelled));
		}

		public void TestNVOCC_ForActionLinkOnly_WithAttachedDocumentCollection()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001111";
			shipment.JS_BookingReference = "C00001111";

			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";
			shipment.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
	<Shipment>
	<DataContext>
		<Action>LinkOnly</Action>
		<DocumentaryOverride>
		<DocumentName>Shipping Instruction</DocumentName>
		</DocumentaryOverride>
		<DataTargetCollection>
		<DataTarget>
			<Type>ForwardingShipment</Type>
		</DataTarget>
		</DataTargetCollection>
		<RecipientRoleCollection>
		<RecipientRole>
			<Code>NVO</Code>
			<Description>NVOCC</Description>
			<ServiceCode>SIN</ServiceCode>
		</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<AgentsReference>C00001111</AgentsReference>
	<CoLoadBookingConfirmationReference>S00001111</CoLoadBookingConfirmationReference>
	<CoLoadMasterBillNumber>VERSJU2104661454</CoLoadMasterBillNumber>
	<OrganizationAddressCollection>
		<OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<Address1>Booking Party Address 1</Address1>
		<Address2 />
		<AddressOverride>false</AddressOverride>
		<City>PORT MELBOURNE</City>
		<CompanyName>BKG COMPANY PTY LTD</CompanyName>
		<Contact>CargoWise Support</Contact>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		</OrganizationAddress>
	</OrganizationAddressCollection>
	<AttachedDocumentCollection>
		<AttachedDocument>
		<FileName>hello1.txt</FileName>
		<ImageData>SGVsbG8sIFdvcmxkIQ==</ImageData>
		<Type>
			<Code>TSR</Code>
		</Type>
		<IsPublished>true</IsPublished>
		<VisibleCompanyCode></VisibleCompanyCode>
		<VisibleBranchCode></VisibleBranchCode>
		<VisibleDepartmentCode></VisibleDepartmentCode>
		</AttachedDocument>
		<AttachedDocument>
		<FileName>hello2.txt</FileName>
		<ImageData>SGVsbG8sIFdvcmxkIQ==</ImageData>
		<Type>
			<Code>MSC</Code>
		</Type>
		<IsPublished>true</IsPublished>
		<VisibleCompanyCode></VisibleCompanyCode>
		<VisibleBranchCode></VisibleBranchCode>
		<VisibleDepartmentCode></VisibleDepartmentCode>
		</AttachedDocument>
	</AttachedDocumentCollection>
	</Shipment>
</UniversalShipment>");

			#endregion

			var manager = new UniversalMessageProcessingManager(Factory, new ServiceTaskLogForTesting());

			using (Factory.BOFactory.AddDisposableService())
			{
				manager.Process(message);

				AssertMultilineASCIIEquals("import log", @"Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully Added eDoc: hello1.txt.
Successfully Added eDoc: hello2.txt.
Shipment status of Shipment S00001111 has been updated.
Universal Shipment data was linked to Shipment S00001111.
Successfully saved, but nothing was reported as being updated.".Trim(), message.GetLogNoteText());

				var shipmentInDB = new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK);

				var savedEDocs = ((IDocManagerSupport)shipmentInDB).DocManagerInfo.Files.Cast<IeDoc>();
				AssertEquals(2, savedEDocs.Count());
				var eDoc1 = savedEDocs.FirstOrDefault(e => e.FileName == "hello1.txt");
				var eDoc2 = savedEDocs.FirstOrDefault(e => e.FileName == "hello2.txt");
				AssertNotNull(eDoc1);
				AssertNotNull(eDoc2);

				var ddiLogs = shipmentInDB.GetLogs().Find(l => l.SL_SE_NKEvent == Events.DocumentImportedCode);
				var ddi1 = ddiLogs.FirstOrDefault(l => l.SL_Reference == eDoc1.DocType + "|" + eDoc1.UniqueKey.ToString());
				var ddi2 = ddiLogs.FirstOrDefault(l => l.SL_Reference == eDoc2.DocType + "|" + eDoc2.UniqueKey.ToString());
				AssertNotNull("Expecting one DDI event per document", ddi1);
				AssertNotNull("Expecting one DDI event per document", ddi2);
			}
		}

		public void TestNVOCC_ForActionLinkOnly_WithHIR()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001111";
			shipment.JS_BookingReference = "C00001111";

			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";
			shipment.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			var additionalReference1 = shipment.Numbers.AddNew();
			additionalReference1.CE_EntryNum = "HIR001";
			additionalReference1.CE_EntryType = "HIR";
			additionalReference1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
	<Shipment>
	<DataContext>
		<Action>LinkOnly</Action>
		<DocumentaryOverride>
		<DocumentName>Shipping Instruction</DocumentName>
		</DocumentaryOverride>
		<DataTargetCollection>
		<DataTarget>
			<Type>ForwardingShipment</Type>
		</DataTarget>
		</DataTargetCollection>
		<RecipientRoleCollection>
		<RecipientRole>
			<Code>NVO</Code>
			<Description>NVOCC</Description>
			<ServiceCode>SIN</ServiceCode>
		</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<AgentsReference>C00001111</AgentsReference>
	<CoLoadBookingConfirmationReference>S00001111</CoLoadBookingConfirmationReference>
	<CoLoadMasterBillNumber>VERSJU2104661454</CoLoadMasterBillNumber>
	<AdditionalReferenceCollection>
		<AdditionalReference>
		<Type>
			<Code>HIR</Code>
			<Description>HIR Number</Description>
		</Type>
		<ReferenceNumber>HIR001</ReferenceNumber>
		</AdditionalReference>
	</AdditionalReferenceCollection>
	<OrganizationAddressCollection>
		<OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<Address1>Booking Party Address 1</Address1>
		<Address2 />
		<AddressOverride>false</AddressOverride>
		<City>PORT MELBOURNE</City>
		<CompanyName>BKG COMPANY PTY LTD</CompanyName>
		<Contact>CargoWise Support</Contact>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		</OrganizationAddress>
	</OrganizationAddressCollection>
	</Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Shipment status of Shipment S00001111 has been updated.
Universal Shipment data was linked to Shipment S00001111.
Successfully saved, but nothing was reported as being updated.".Trim(), message.GetLogNoteText());
			var shipmentInDB = new BusinessObjectFactory().LoadTop1<ForwardingShipment>(new ZQuery());
			AssertEquals("JS_ShipmentStatus is updated.", ShipmentStatusList.Codes.ElectronicShippingInstruction, shipmentInDB.JS_ShipmentStatus);
			AssertNotNull("STU event is added.", shipmentInDB.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(s => s.SL_SE_NKEvent == Events.StatusUpdatedCode && !s.IsCancelled));
		}

		public void TestNVOCC_ForActionLinkOnly_AddHIR()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001111";
			shipment.JS_BookingReference = "C00001111";

			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";
			shipment.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
	<Shipment>
	<DataContext>
		<Action>LinkOnly</Action>
		<DocumentaryOverride>
		<DocumentName>Shipping Instruction</DocumentName>
		</DocumentaryOverride>
		<DataTargetCollection>
		<DataTarget>
			<Type>ForwardingShipment</Type>
		</DataTarget>
		</DataTargetCollection>
		<RecipientRoleCollection>
		<RecipientRole>
			<Code>NVO</Code>
			<Description>NVOCC</Description>
			<ServiceCode>SIN</ServiceCode>
		</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<AgentsReference>C00001111</AgentsReference>
	<CoLoadBookingConfirmationReference>S00001111</CoLoadBookingConfirmationReference>
	<CoLoadMasterBillNumber>VERSJU2104661454</CoLoadMasterBillNumber>
	<OrganizationAddressCollection>
		<OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<Address1>Booking Party Address 1</Address1>
		<Address2 />
		<AddressOverride>false</AddressOverride>
		<City>PORT MELBOURNE</City>
		<CompanyName>BKG COMPANY PTY LTD</CompanyName>
		<Contact>CargoWise Support</Contact>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		</OrganizationAddress>
	</OrganizationAddressCollection>
	<AdditionalReferenceCollection>
		<AdditionalReference>
		<Type>
			<Code>HIR</Code>
			<Description>HIR Number</Description>
		</Type>
		<ReferenceNumber>HIR001</ReferenceNumber>
		</AdditionalReference>
	</AdditionalReferenceCollection>
	</Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Shipment status of Shipment S00001111 has been updated.
Universal Shipment data was linked to Shipment S00001111.
Successfully saved, but nothing was reported as being updated.".Trim(), message.GetLogNoteText());
			var shipmentInDB = new BusinessObjectFactory().LoadTop1<ForwardingShipment>(new ZQuery());
			AssertEquals("JS_ShipmentStatus is updated.", ShipmentStatusList.Codes.ElectronicShippingInstruction, shipmentInDB.JS_ShipmentStatus);
			AssertNotNull("STU event is added.", shipmentInDB.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(s => s.SL_SE_NKEvent == Events.StatusUpdatedCode && !s.IsCancelled));
			AssertEquals("shipmentInDB.Numbers.GetFirstReferenceNumberByType('HIR')", "HIR001", shipmentInDB.Numbers.GetFirstReferenceNumberByType("HIR").CE_EntryNum);
		}

		public void TestNVOCC_ForActionLinkOnly_WithBooking()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var shipmentBOToLoad = quotedBooking.ForwardingShipment as ForwardingShipment;
			shipmentBOToLoad.JS_UniqueConsignRef = "S00001111";
			shipmentBOToLoad.JS_BookingReference = "C00001111";
			shipmentBOToLoad.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			Factory.SaveForTesting();

			Assert("Precondition: JS_IsForwardRegistered is false.", !shipmentBOToLoad.JS_IsForwardRegistered);

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
	<Shipment>
	<DataContext>
		<Action>LinkOnly</Action>
		<DocumentaryOverride>
		<DocumentName>Shipping Instruction</DocumentName>
		</DocumentaryOverride>
		<DataTargetCollection>
		<DataTarget>
			<Type>ForwardingShipment</Type>
		</DataTarget>
		</DataTargetCollection>
		<RecipientRoleCollection>
		<RecipientRole>
			<Code>NVO</Code>
			<Description>NVOCC</Description>
			<ServiceCode>SIN</ServiceCode>
		</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<AgentsReference>C00001111</AgentsReference>
	<CoLoadBookingConfirmationReference>S00001111</CoLoadBookingConfirmationReference>
	<CoLoadMasterBillNumber>VERSJU2104661454</CoLoadMasterBillNumber>
	<OrganizationAddressCollection>
		<OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<Address1>Booking Party Address 1</Address1>
		<Address2 />
		<AddressOverride>false</AddressOverride>
		<City>PORT MELBOURNE</City>
		<CompanyName>BKG COMPANY PTY LTD</CompanyName>
		<Contact>CargoWise Support</Contact>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		</OrganizationAddress>
	</OrganizationAddressCollection>
	</Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Shipment status of Shipment S00001111 has been updated.
Universal Shipment data was linked to Shipment S00001111.
Successfully saved, but nothing was reported as being updated.".Trim(), message.GetLogNoteText());
			var shipmentInDB = new BusinessObjectFactory().LoadTop1<ForwardingShipment>(new ZQuery());
			Assert("JS_IsForwardRegistered is true.", shipmentInDB.JS_IsForwardRegistered);
			AssertEquals("JS_ShipmentStatus is updated.", ShipmentStatusList.Codes.ElectronicShippingInstruction, shipmentInDB.JS_ShipmentStatus);
			AssertNotNull("STU event is added.", shipmentInDB.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(s => s.SL_SE_NKEvent == Events.StatusUpdatedCode && !s.IsCancelled));
		}

		public void TestNVOCC_ForActionLinkOnly_Rejected()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001111";
			shipment.JS_BookingReference = "C00001111";

			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";
			shipment.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			Factory.SaveForTesting();

			#region Message

			var message = GetQueuedUniversalShipmentMessage(
@"<UniversalShipment>
	<Shipment>
	<DataContext>
		<Action>LinkOnly</Action>
		<DocumentaryOverride>
		<DocumentName>Shipping Instruction</DocumentName>
		</DocumentaryOverride>
		<DataTargetCollection>
		<DataTarget>
			<Type>ForwardingShipment</Type>
		</DataTarget>
		</DataTargetCollection>
		<RecipientRoleCollection>
		<RecipientRole>
			<Code>NVO</Code>
			<Description>NVOCC</Description>
			<ServiceCode>SIN</ServiceCode>
		</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<AgentsReference>C00001111</AgentsReference>
	<CoLoadBookingConfirmationReference>S00001112</CoLoadBookingConfirmationReference>
	<CoLoadMasterBillNumber>VERSJU2104661454</CoLoadMasterBillNumber>
	<OrganizationAddressCollection>
		<OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<Address1>Booking Party Address 1</Address1>
		<Address2 />
		<AddressOverride>false</AddressOverride>
		<City>PORT MELBOURNE</City>
		<CompanyName>BKG COMPANY PTY LTD</CompanyName>
		<Contact>CargoWise Support</Contact>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		</OrganizationAddress>
	</OrganizationAddressCollection>
	</Shipment>
</UniversalShipment>");

			#endregion

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("import log", @"Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - [*Unable to link because existing business object could not be found.*]
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.".Trim(), message.GetLogNoteText());
			var shipmentInDB = new BusinessObjectFactory().LoadTop1<ForwardingShipment>(new ZQuery());
			AssertEquals("JS_ShipmentStatus is not updated.", "CNF", shipmentInDB.JS_ShipmentStatus);
			AssertNull("STU event is not added.", shipmentInDB.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(s => s.SL_SE_NKEvent == Events.StatusUpdatedCode && !s.IsCancelled));
		}

		#endregion

		#region Cargo Receipt Advice

		public void TestImportShipmentCargoReceiptAdviceWithMRREvent()
		{
			var shipmentBOToLoad = Factory.New<ForwardingShipment>();
			shipmentBOToLoad.JS_UniqueConsignRef = "SESYDDAU54652003";
			shipmentBOToLoad.JS_BookingReference = "BookingReference111";
			shipmentBOToLoad.JS_HouseBill = "HouseBill111";

			var dummyShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			dummyShipment.JS_UniqueConsignRef = "S00002010";
			dummyShipment.JS_TransportMode = Constants.TransportModes.Sea;
			dummyShipment.JS_HouseBill = "FRED235478923";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalShipmentCargoReceiptAdvice.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			AssertMultilineASCIIEquals("Service Task Log", @"Updated Shipment SESYDDAU54652003 (House Bill='HOUSEBILL111') from UniversalShipment.
Successfully saved Shipment SESYDDAU54652003 (House Bill='HOUSEBILL111') with 1 x ForwardingShipmentStmNote, 2 x JobPackLineHarmonisedCode, 2 x ForwardingPackLine, 1 x UNDGDataItem.".Trim(), serviceTaskLog.ToString());

			var logNoteText = message.GetLogNoteText();
			AssertContains("message.GetLogNoteText()", "Successfully loaded matching ForwardingShipment.", logNoteText);
			AssertContains("message.GetLogNoteText()", "Updated Shipment SESYDDAU54652003 (House Bill='HOUSEBILL111') from UniversalShipment.", logNoteText);

			var mrrEvent = shipmentBOToLoad.Logs.MostRecentLogByEventTime(Events.MessageReceived);
			AssertNotNull("MRR Event", mrrEvent);
			AssertEquals("MessageType", "Cargo Receipt Advice", mrrEvent.Parameters[Params.MessageType]);
			AssertStartsWith("Reference", "from Carrier", mrrEvent.ReferenceFreeText);
		}

		public void TestImportShipmentCargoReceiptAdviceFailedToGetMatch()
		{
			var message = GetQueuedUniversalShipmentMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalShipmentCargoReceiptAdvice.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("Service Task Log", @"ERROR - [*Match couldn't be found for ForwardingShipment with Key SESYDDAU54652003*]
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.".Trim(), serviceTaskLog.ToString());
		}

		#endregion

		#region Import event auto recalculate DeliveryDueDate

		[TestDate(2022, 09, 15)]
		public void TestImportPickupCartageComplete_TriggerDDDRecalculation_WithoutDataTarget()
		{
			ImportUniversalEventAndAssertDDDRecalculation(Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR,
				"UniversalEventPCFWithoutDataTarget.xml", "Actual Pickup Date changed");
		}

		[TestDate(2022, 09, 15)]
		public void TestImportPickupCartageComplete_TriggerDDDRecalculation_WithDataTarget()
		{
			ImportUniversalEventAndAssertDDDRecalculation(Core.Constants.HBLDeliveryModes.Codes.DOOR_CFS,
				"UniversalEventPCFWithDataTarget.xml", "Actual Pickup Date changed");
		}

		[TestDate(2022, 09, 15)]
		public void TestImportInterimReceiptProduced_TriggerDDDRecalculation_WithoutDataTarget()
		{
			ImportUniversalEventAndAssertDDDRecalculation(Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR,
				"UniversalEventIRPWithoutDataTarget.xml", "Interim Receipt Date changed");
		}

		[TestDate(2022, 09, 15)]
		public void TestImportInterimReceiptProduced_TriggerDDDRecalculation_WithDataTarget()
		{
			ImportUniversalEventAndAssertDDDRecalculation(Core.Constants.HBLDeliveryModes.Codes.CFS_CFS,
				"UniversalEventIRPWithDataTarget.xml", "Interim Receipt Date changed");
		}

		void ImportUniversalEventAndAssertDDDRecalculation(string containerPackMode, string xmlFileName, string reason)
		{
			var oldDeliveryDueDate = ZDateTime.Today.AddDays(10);
			var newDeliveryDueDate = ZDateTime.Today.AddDays(15);
			var deliveryDueDateCalculatorManagerMock = DeliveryDueDateCalculationTestHelper.SetupDeliveryDueDateCalculatorManagerMockWithoutSubstitution(newDeliveryDueDate);
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var newFactory = new BusinessObjectFactory();

				var shipment = newFactory.New<ForwardingShipment>();
				shipment.GetReasonForChangingDeliveryDueDateEventHandler += (sender, arg) => arg.Reason = "I want to override";
				shipment.JS_UniqueConsignRef = "S00001111";
				shipment.JS_HouseBill = "S00001111";
				shipment.JS_TransportMode = "AIR";
				shipment.ConsignorDocumentaryAddress.OrganisationPK = newFactory.NewWithValidTestData<OrgHeader>().PK;
				shipment.ConsigneeDocumentaryAddress.OrganisationPK = newFactory.NewWithValidTestData<OrgHeader>().PK;
				shipment.JS_OA_ExportReceivingDepot = newFactory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
				shipment.JS_OA_ImportReleaseDepot = newFactory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
				shipment.JS_HBLContainerPackModeOverride = containerPackMode;
				shipment.JS_RS_NKServiceLevel = "STD";
				shipment.DocsAndCartage.JP_PickupRequiredBy = ZDateTime.Now;
				shipment.JS_DeliveryDueDate = oldDeliveryDueDate;

				newFactory.Save();

				using (ObjectFactory.Substitute(deliveryDueDateCalculatorManagerMock.Object))
				{
					var message = GetQueuedUniversalEventMessage(ResourceRetriever.GetString(GetResourcePathFor(xmlFileName)));

					var serviceTaskLog = new ServiceTaskLogForTesting();
					var manager = new UniversalMessageProcessingManager(serviceTaskLog);
					manager.Process(message);

					var messageLogger = new XmlSessionTracker(serviceTaskLog);
					Factory.SaveAtEndOfImport(messageLogger);
					deliveryDueDateCalculatorManagerMock.Verify(mock => mock.Calculate(It.IsAny<ForwardingShipment>()), Times.Once, "Factor change triggered Delivery Due Date recalculation");

					shipment = Factory.Load<ForwardingShipment>(shipment.PK);
					CombineAssertions("Import PCF triggers recalculation", () =>
					{
						AssertEquals("Delivery Due Date is recalculated", newDeliveryDueDate, shipment.JS_DeliveryDueDate);
						var ddeEvent = shipment.Logs.MostRecentLogByEventTime(Events.DeliveryDateUpdated);
						AssertEquals("DDE is logged", $"|ACT=FactorChanged|NEW={newDeliveryDueDate.ToLongTimeString()}|OLD={oldDeliveryDueDate.ToLongTimeString()}|RES={reason}|TYP=Original", ddeEvent.SL_Reference);
					});
				}
			}
		}

		#endregion

		#region GetReasonForNotAbleToUpdateLogParentFromPickupCartageCompleteFinalisedEvent

		public void TestGetReasonForNotAbleToUpdateLogParentFromPickupCartageCompleteFinalisedEvent()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKLoadPort = "CNSHA";
			shipment.JS_RL_NKDischargePort = "AUSYD";
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "AUSYD";

			var universalEvent = new UniversalEvent()
			{
				EventType = Events.PickupCartageCompleteFinalisedCode,
				EventTime = shipment.Origin.LocationDateTime.AddMinutes(10).ToOffset(),
				IsEstimate = false
			};

			var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			AssertEquals(false, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out var failureReason1));
			var pattern = "The Actual PCF date .* is detected in the future, and cannot be saved.";
			Assert(Regex.IsMatch(failureReason1, pattern));

			universalEvent.EventTime = shipment.Origin.LocationDateTime.AddMinutes(-10).ToOffset();
			AssertEquals(true, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out var failureReason2));
			AssertEquals(ZString.Empty, failureReason2);

			var consignorPickupAddress = Factory.New<OrgHeader>();
			consignorPickupAddress.MainAddress.Address1 = "Address 1";
			consignorPickupAddress.MainAddress.OA_RL_NKRelatedPortCode = "DEHAM";
			shipment.ConsignorPickupAddress.E2_OA_Address = consignorPickupAddress.MainAddress.PK;

			universalEvent.EventTime = ((ILocation)shipment.ConsignorPickupAddress).UNLOCO.LocationDateTime.AddMinutes(10).ToOffset();
			AssertEquals(false, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out var failureReason3));
			Assert(Regex.IsMatch(failureReason3, pattern));

			universalEvent.EventTime = ((ILocation)shipment.ConsignorPickupAddress).UNLOCO.LocationDateTime.AddMinutes(-10).ToOffset();
			AssertEquals(true, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out var failureReason4));
			AssertEquals(ZString.Empty, failureReason4);
		}

		#endregion

		#region GetReasonForNotAbleToUpdateLogParentFromDeliveryCartageCompleteFinalisedEvent

		public void TestGetReasonForNotAbleToUpdateLogParentFromDeliveryCartageCompleteFinalisedEvent()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKLoadPort = "CNSHA";
			shipment.JS_RL_NKDischargePort = "AUSYD";
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "AUSYD";

			var universalEvent = new UniversalEvent()
			{
				EventType = Events.DeliveryCartageCompleteFinalisedCode,
				EventTime = shipment.Destination.LocationDateTime.AddMinutes(10).ToOffset(),
				IsEstimate = false
			};

			var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			AssertEquals(false, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out var failureReason1));
			var pattern = @"The Actual DCF date .* is detected in the future, and cannot be saved.";
			Assert(Regex.IsMatch(failureReason1, pattern));

			universalEvent.EventTime = shipment.Destination.LocationDateTime.AddMinutes(-10).ToOffset();
			AssertEquals(true, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out var failureReason2));
			AssertEquals(ZString.Empty, failureReason2);

			var consigneeDeliveryAddress = Factory.New<OrgHeader>();
			consigneeDeliveryAddress.MainAddress.Address1 = "Address 1";
			consigneeDeliveryAddress.MainAddress.OA_RL_NKRelatedPortCode = "DEHAM";
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = consigneeDeliveryAddress.MainAddress.PK;

			universalEvent.EventTime = ((ILocation)shipment.ConsigneeDeliveryAddress).UNLOCO.LocationDateTime.AddMinutes(10).ToOffset();
			AssertEquals(false, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out var failureReason3));
			Assert(Regex.IsMatch(failureReason3, pattern));

			universalEvent.EventTime = ((ILocation)shipment.ConsigneeDeliveryAddress).UNLOCO.LocationDateTime.AddMinutes(-10).ToOffset();
			AssertEquals(true, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out var failureReason4));
			AssertEquals(ZString.Empty, failureReason4);
		}

		#endregion

		#region Bolero eHBL

		public void TestBoleroProcessAmendmentRequestedEvent_UpdatesShipmentStatus_AmendmentRequested()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_UniqueConsignRef = "S00001234";
			shipment.JS_ElectronicBillOfLadingReference = "WTLDAUILA-MEL_SHAS00002367_1";
			shipment.JS_ElectronicBillOfLadingStatus = "OBP";
			Factory.SaveForTesting();

			var boleroEBLConfiguration = new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = false,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};
			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var message = GetQueuedUniversalEventMessage(BoleroAmendmentDeniedEventXml);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);
				Factory.SaveForTesting();

				AssertNotEquals("OBA", shipment.JS_ElectronicBillOfLadingStatus);
			}

			boleroEBLConfiguration.EnableEBLIntegration = true;
			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalEventMessage(BoleroAmendmentRequestedEventXml);
				manager.Process(message);
				Factory.SaveForTesting();

				AssertEquals("OBA", shipment.JS_ElectronicBillOfLadingStatus);
				AssertEquals("Please delete fuel type and add HS Code.", shipment.JS_ElectronicBillOfLadingAmendmentRequestDetailsForBinding);

				message = GetQueuedUniversalEventMessage(BoleroSurrenderedEventXml);
				manager.Process(message);
				Factory.SaveForTesting();

				AssertEquals(FreightConstants.BillOfLadingBillStatus.Codes.Surrendered, shipment.JS_ElectronicBillOfLadingStatus);
				AssertEquals("Surrendered details", shipment.JS_ElectronicBillOfLadingAmendmentRequestDetailsForBinding);
			}

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalEventMessage(BoleroDenyAmendmentRejectedEventXml);
				manager.Process(message);
				Factory.SaveForTesting();

				AssertEquals("OBA", shipment.JS_ElectronicBillOfLadingStatus);
				AssertEquals("The Deny Amendment request is Rejected.\r\nPlease contact Consignee.", shipment.JS_ElectronicBillOfLadingAmendmentRequestDetailsForBinding);
			}
		}

		#region Auto Reject Amendment Requested Event From Bolero

		public void TestAutoRejectBoleroAmendmentRequestedEvent()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001234";
			Factory.SaveForTesting();

			var universalEvent = new UniversalEvent()
			{
				EventType = Events.BillStatusUpdatedCode,
				EventParameters = new EventParameters
				{
					Department = ElectronicBOLConstants.EHBLEventDepartments.TitleRegistry,
					Type = "Amendment Requested",
					ReferenceNumber = "WTLDAUILA-MEL_SHAS00002367_1"
				},
			};

			var boleroEBLConfiguration = new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = false,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
				AssertEquals(true, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out var failureReason));
				AssertEquals("There is no failure reason", ZString.Empty, failureReason);
			}

			boleroEBLConfiguration.EnableEBLIntegration = true;
			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
				AssertEquals(false, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out var failureReason));
				AssertEquals("Error Message: The Electronic Bill of Lading was not issued yet. Message rejected.", failureReason);

				shipment.JS_ElectronicBillOfLadingStatus = "STP";
				manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
				AssertEquals(false, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out var failureReason2));
				AssertEquals("Error Message: The Electronic Bill of Lading was switched to paper and amendments are no longer accepted. Message rejected.", failureReason2);

				shipment.JS_ElectronicBillOfLadingStatus = "SUR";
				manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
				AssertEquals(false, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out var failureReason3));
				AssertEquals("Error Message: The Electronic Bill of Lading was surrendered, and amendments are no longer accepted. Message rejected.", failureReason3);

				shipment.JS_ElectronicBillOfLadingStatus = "OBP";
				shipment.JS_ElectronicBillOfLadingReference = "WTLDAUILA-MEL_SHAS00002367_2";
				manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
				AssertEquals(false, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out var failureReason4));
				AssertEquals("Error Message: The electronic Bill Identifier does not match with Reference Number. Message rejected.", failureReason4);

				shipment.JS_ElectronicBillOfLadingReference = "WTLDAUILA-MEL_SHAS00002367_1";
				manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
				AssertEquals("The event from Bolero has been successfully received", true, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out var failureReason5));
				AssertEquals("There is no failure reason", ZString.Empty, failureReason5);
			}
		}

		public void TestAutoRejectBoleroAmendmentRequestedEvent_NotYetDenied()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_UniqueConsignRef = "S00001234";
			shipment.JS_ElectronicBillOfLadingReference = "WTLDAUILA-MEL_SHAS00002367_1";
			shipment.JS_ElectronicBillOfLadingStatus = "OBP";
			Factory.SaveForTesting();

			var boleroEBLConfiguration = new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var universalMessageProcessingManager = new UniversalMessageProcessingManager(serviceTaskLog);

				var message = GetQueuedUniversalEventMessage(BoleroAmendmentRequestedEventXml);
				universalMessageProcessingManager.Process(message);
				Factory.SaveForTesting();

				var universalEvent = new UniversalEvent()
				{
					EventType = Events.BillStatusUpdatedCode,
					EventParameters = new EventParameters
					{
						Department = ElectronicBOLConstants.EHBLEventDepartments.TitleRegistry,
						Type = "Amendment Requested",
						ReferenceNumber = "WTLDAUILA-MEL_SHAS00002367_1"
					},
				};

				var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
				AssertEquals(false, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out var failureReason));
				AssertEquals("Error Message: A previous Amendment Request was not processed yet. Message rejected.", failureReason);

				message = GetQueuedUniversalEventMessage(BoleroAmendmentDeniedEventXml);
				universalMessageProcessingManager.Process(message);
				Factory.SaveForTesting();

				manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
				AssertEquals("The event from Bolero has been successfully received", true, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out var failureReason2));
				AssertEquals("There is no failure reason", ZString.Empty, failureReason2);
			}
		}

		public void TestAutoRejectBoleroAmendmentRequestedEvent_NotYetAccepted()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_UniqueConsignRef = "S00001234";
			shipment.JS_ElectronicBillOfLadingReference = "WTLDAUILA-MEL_SHAS00002367_1";
			shipment.JS_ElectronicBillOfLadingStatus = "OBP";
			Factory.SaveForTesting();

			var boleroEBLConfiguration = new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var universalMessageProcessingManager = new UniversalMessageProcessingManager(serviceTaskLog);

				var message = GetQueuedUniversalEventMessage(BoleroAmendmentRequestedEventXml);
				universalMessageProcessingManager.Process(message);
				Factory.SaveForTesting();

				var universalEvent = new UniversalEvent()
				{
					EventType = Events.BillStatusUpdatedCode,
					EventParameters = new EventParameters
					{
						Department = ElectronicBOLConstants.EHBLEventDepartments.TitleRegistry,
						Type = "Amendment Requested",
						ReferenceNumber = "WTLDAUILA-MEL_SHAS00002367_1"
					},
				};

				var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
				AssertEquals(false, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out var failureReason));
				AssertEquals("Error Message: A previous Amendment Request was not processed yet. Message rejected.", failureReason);

				message = GetQueuedUniversalEventMessage(BoleroGrantAmendmentEventXml);
				universalMessageProcessingManager.Process(message);
				Factory.SaveForTesting();

				manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
				AssertEquals("The event from Bolero has been successfully received", true, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out var failureReason2));
				AssertEquals("There is no failure reason", ZString.Empty, failureReason2);
			}
		}

		const string BoleroAmendmentRequestedEventXml = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Key>S00001234</Key>
					<Type>ForwardingShipment</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2024-10-22T14:10:00Z</EventTime>
		<EventType>BLU</EventType>
		<EventParameters>
			<Department>Title Registry</Department>
			<Type>Amendment Requested</Type>
			<ReferenceNumber>WTLDAUILA-MEL_SHAS00002367_1</ReferenceNumber>
			<RequestNumber>9645</RequestNumber>
		</EventParameters>
		<IsEstimate>false</IsEstimate>
		<ContextCollection>
			<Context>
				<Type>NotificationDetails</Type>
				<Value>Please delete fuel type and add HS Code.</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		const string BoleroDenyAmendmentRejectedEventXml = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Key>S00001234</Key>
					<Type>ForwardingShipment</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2024-11-20T14:10:00Z</EventTime>
		<EventType>BLU</EventType>
		<EventParameters>
			<Department>Title Registry</Department>
			<Type>Deny Amendment Rejected</Type>
			<ReferenceNumber>WTLDAUILA-MEL_SHAS00002367_1</ReferenceNumber>
			<RequestNumber>9645</RequestNumber>
		</EventParameters>
		<IsEstimate>false</IsEstimate>
		<ContextCollection>
			<Context>
				<Type>NotificationDetails</Type>
				<Value>
					The Deny Amendment request is Rejected.
					Please contact Consignee.
				</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		const string BoleroAmendmentDeniedEventXml = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Key>S00001234</Key>
					<Type>ForwardingShipment</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2024-11-18T14:10:00Z</EventTime>
		<EventType>MSN</EventType>
		<EventParameters>
			<Department>Title Registry</Department>
			<Type>Amendment Denied</Type>
			<ReferenceNumber>WTLDAUILA-MEL_SHAS00002367_1</ReferenceNumber>
			<RequestNumber>9645</RequestNumber>
		</EventParameters>
		<IsEstimate>false</IsEstimate>
		<ContextCollection>
			<Context>
				<Type>NotificationDetails</Type>
				<Value>The requested changes are too comprehensive</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		const string BoleroGrantAmendmentEventXml = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Key>S00001234</Key>
					<Type>ForwardingShipment</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2024-11-19T14:10:00Z</EventTime>
		<EventType>MSN</EventType>
		<EventParameters>
			<Department>Title Registry</Department>
			<Type>Original Bill Sent for Publication</Type>
			<ReferenceNumber>WTLDAUILA-MEL_SHAS00002367_1</ReferenceNumber>
			<RequestNumber>9645</RequestNumber>
		</EventParameters>
		<IsEstimate>false</IsEstimate>
		<ContextCollection/>
	</Event>
</UniversalEvent>";

		const string BoleroSurrenderedEventXml = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Key>S00001234</Key>
					<Type>ForwardingShipment</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2024-10-22T14:10:00Z</EventTime>
		<EventType>BLU</EventType>
		<EventParameters>
			<Department>Title Registry</Department>
			<Type>Surrendered</Type>
			<ReferenceNumber>WTLDAUILA-MEL_SHAS00002367_1</ReferenceNumber>
			<RequestNumber>9645</RequestNumber>
		</EventParameters>
		<IsEstimate>false</IsEstimate>
		<ContextCollection>
			<Context>
				<Type>NotificationDetails</Type>
				<Value>Surrendered details</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		public void TestBoleroProcessAmendmentRequestedEvent_OriginalBillNotPublished()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001234";
			shipment.JS_ElectronicBillOfLadingStatus = ZString.Empty;
			shipment.JS_ElectronicBillOfLadingVersion = (ZShort)2;
			Factory.SaveForTesting();

			const string eventXML = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Key>S00001234</Key>
					<Type>ForwardingShipment</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2024-10-22T14:10:00Z</EventTime>
		<EventType>BLU</EventType>
		<EventParameters>
			<Department>Title Registry</Department>
			<Type>Original Bill not Published</Type>
			<ReferenceNumber>WTLDAUILA-MEL_SHAS00002367_1</ReferenceNumber>
			<RequestNumber>9645</RequestNumber>
		</EventParameters>
		<IsEstimate>false</IsEstimate>
		<ContextCollection>
			<Context>
				<Type>NotificationDetails</Type>
				<Value>Please delete fuel type and add HS Code.</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var boleroEBLConfiguration = new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = false,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};
			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var message = GetQueuedUniversalEventMessage(eventXML);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				AssertNullOrEmpty(shipment.JS_ElectronicBillOfLadingStatus);
				AssertEquals((ZShort)2, shipment.JS_ElectronicBillOfLadingVersion);
			}

			boleroEBLConfiguration.EnableEBLIntegration = true;
			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var message = GetQueuedUniversalEventMessage(eventXML);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				AssertEquals(FreightConstants.BillOfLadingBillStatus.Codes.PublishingRejected, shipment.JS_ElectronicBillOfLadingStatus);
				AssertEquals((ZShort)1, shipment.JS_ElectronicBillOfLadingVersion);
			}
		}

		#endregion

		public void TestBoleroProcessSurrenderedEvent_UpdatesShipmentStatus_Surrendered()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_UniqueConsignRef = "S00001234";
			shipment.JS_ElectronicBillOfLadingStatus = "OBP";
			shipment.JS_ElectronicBillOfLadingReference = "WTLDAUILA-MEL_SHAS00002367_1";
			Factory.SaveForTesting();

			const string eventXML = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Key>S00001234</Key>
					<Type>ForwardingShipment</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2024-11-05T14:10:00Z</EventTime>
		<EventType>BLU</EventType>
		<EventParameters>
			<Department>Title Registry</Department>
			<Type>Surrendered</Type>
			<ReferenceNumber>WTLDAUILA-MEL_SHAS00002367_1</ReferenceNumber>
			<RequestNumber>9645</RequestNumber>
		</EventParameters>
		<IsEstimate>false</IsEstimate>
		<ContextCollection>
			<Context>
				<Type>NotificationDetails</Type>
				<Value>Bill surrendered</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var boleroEBLConfiguration = new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = false,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};
			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var message = GetQueuedUniversalEventMessage(eventXML);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				AssertNotEquals("SUR", shipment.JS_ElectronicBillOfLadingStatus);
			}

			boleroEBLConfiguration.EnableEBLIntegration = true;
			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var message = GetQueuedUniversalEventMessage(eventXML);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				AssertEquals("SUR", shipment.JS_ElectronicBillOfLadingStatus);
			}
		}

		public void TestAutoRejectBoleroSurrenderedEventWithFailureReason_Surrendered()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001234";
			Factory.SaveForTesting();

			var universalEvent = new UniversalEvent()
			{
				EventType = Events.BillStatusUpdatedCode,
				EventParameters = new EventParameters
				{
					Department = ElectronicBOLConstants.EHBLEventDepartments.TitleRegistry,
					Type = "Surrendered",
					ReferenceNumber = "WTLDAUILA-MEL_SHAS00002367_1"
				},
			};

			var boleroEBLConfiguration = new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = false,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
				AssertEquals(true, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out var failureReason));
				AssertEquals("There is no failure reason", ZString.Empty, failureReason);
			}

			boleroEBLConfiguration.EnableEBLIntegration = true;
			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
				AssertEquals(false, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out var failureReason));
				AssertEquals("Error Message: The Electronic Bill of Lading was not issued yet. Message rejected.", failureReason);

				shipment.JS_ElectronicBillOfLadingStatus = "STP";
				manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
				AssertEquals(false, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out var failureReason2));
				AssertEquals("Error Message: The Electronic Bill of Lading was switched to paper and Electronic Surrender Request is no longer accepted. Message rejected.", failureReason2);

				shipment.JS_ElectronicBillOfLadingStatus = "SUR";
				manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
				AssertEquals(false, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out var failureReason3));
				AssertEquals("Error Message: The Electronic Bill of Lading was already surrendered. Message rejected.", failureReason3);

				shipment.JS_ElectronicBillOfLadingStatus = "OBP";
				shipment.JS_ElectronicBillOfLadingReference = "WTLDAUILA-MEL_SHAS00002367_2";
				manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
				AssertEquals(false, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out var failureReason4));
				AssertEquals("Error Message: The electronic Bill Identifier does not match with Reference Number. Message rejected.", failureReason4);

				shipment.JS_ElectronicBillOfLadingReference = "WTLDAUILA-MEL_SHAS00002367_1";
				manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
				AssertEquals("The event from Bolero has been successfully received", true, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out var failureReason5));
				AssertEquals("There is no failure reason", ZString.Empty, failureReason5);
			}
		}

		public void TestBoleroProcessAmendmentRequestedEvent_UpdatesShipmentStatus_SwitchedToPaper()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001234";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_ElectronicBillOfLadingReference = "WTLDAUILA-MEL_SHAS00002367_1";
			shipment.JS_ElectronicBillOfLadingStatus = FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillAmendmentInProgress;
			Factory.SaveForTesting();

			const string eventXML = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Key>S00001234</Key>
					<Type>ForwardingShipment</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2024-10-22T14:10:00Z</EventTime>
		<EventType>BLU</EventType>
		<EventParameters>
			<Department>Title Registry</Department>
			<Type>Switched To Paper</Type>
			<ReferenceNumber>WTLDAUILA-MEL_SHAS00002367_1</ReferenceNumber>
			<RequestNumber>9645</RequestNumber>
		</EventParameters>
		<IsEstimate>false</IsEstimate>
		<ContextCollection>
			<Context>
				<Type>NotificationDetails</Type>
				<Value>Switching To Paper.</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var boleroEBLConfiguration = new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = false,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};
			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var message = GetQueuedUniversalEventMessage(eventXML);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				AssertNotEquals("STP", shipment.JS_ElectronicBillOfLadingStatus);
			}

			boleroEBLConfiguration.EnableEBLIntegration = true;
			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var message = GetQueuedUniversalEventMessage(eventXML);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				AssertEquals("STP", shipment.JS_ElectronicBillOfLadingStatus);
				AssertEquals("Switching To Paper.", shipment.JS_ElectronicBillOfLadingAmendmentRequestDetailsForBinding);
			}
		}

		public void TestAutoRejectBoleroSurrenderedEventWithFailureReason_SwitchedToPaper()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001234";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_ElectronicBillOfLadingReference = "HBL123";
			shipment.JS_ElectronicBillOfLadingStatus = FreightConstants.BillOfLadingBillStatus.Codes.SwitchedToPaper;
			Factory.SaveForTesting();

			var universalEvent = new UniversalEvent()
			{
				EventType = Events.BillStatusUpdatedCode,
				EventParameters = new EventParameters()
				{
					Type = Core.Constants.BillStatusUpdatedTypes.SwitchedToPaper,
					Department = ElectronicBOLConstants.EHBLEventDepartments.TitleRegistry,
					ReferenceNumber = "HBL456"
				},
				EventTime = ZDateTimeOffset.Today,
				IsEstimate = false,
				IsCancelled = false
			};

			var boleroEBLConfiguration = new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = false,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
				AssertEquals(true, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out var failureReason));
				AssertEquals("There is no failure reason", ZString.Empty, failureReason);
			}

			boleroEBLConfiguration.EnableEBLIntegration = true;
			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
				AssertEquals(false, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out var failureReason));
				AssertEquals("Error Message: The Electronic Bill of Lading was already switched to paper. Message rejected.", failureReason);

				shipment.JS_ElectronicBillOfLadingStatus = FreightConstants.BillOfLadingBillStatus.Codes.Surrendered;
				Factory.SaveForTesting();

				manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
				AssertEquals(false, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out failureReason));
				AssertEquals("Error Message: The Electronic Bill of Lading was already surrendered. Message rejected.", failureReason);

				shipment.JS_ElectronicBillOfLadingStatus = ZString.Empty;
				Factory.SaveForTesting();

				manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
				AssertEquals(false, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out failureReason));
				AssertEquals("Error Message: The Electronic Bill of Lading was not issued yet. Message rejected.", failureReason);

				shipment.JS_ElectronicBillOfLadingStatus = FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillAmendmentInProgress;
				Factory.SaveForTesting();

				manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
				AssertEquals(false, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out failureReason));
				AssertEquals("Error Message: The Electronic Bill Identifier does not match with Reference Number. Message rejected.", failureReason);

				shipment.JS_ElectronicBillOfLadingReference = "HBL456";
				Factory.SaveForTesting();

				manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
				AssertEquals(true, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out failureReason));
				AssertEquals("There is no failure reason", ZString.Empty, failureReason);
			}
		}

		public void TestAutoRejectBoleroOriginalOriginalBillPublishedEvent()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001234";
			Factory.SaveForTesting();

			var universalEvent = new UniversalEvent()
			{
				EventType = Events.BillStatusUpdatedCode,
				EventParameters = new EventParameters
				{
					Department = "Title Registry",
					Type = "Original Bill Published",
					ReferenceNumber = "WTLDAUILA-MEL_SHAS00002367_1"
				},
			};

			var boleroEBLConfiguration = new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = false,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
				AssertEquals(true, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out var failureReason));
				AssertEquals("There is no failure reason", ZString.Empty, failureReason);
			}

			boleroEBLConfiguration.EnableEBLIntegration = true;
			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				shipment.JS_ElectronicBillOfLadingStatus = "OBP";
				shipment.JS_ElectronicBillOfLadingReference = "WTLDAUILA-MEL_SHAS00002367_1";
				var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
				AssertEquals(false, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out var failureReason));
				AssertEquals("Error Message: A previous 'Original Bill Published' Notification was already received and processed. Message Rejected.", failureReason);

				shipment.JS_ElectronicBillOfLadingReference = "WTLDAUILA-MEL_SHAS00002367_2";
				manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
				AssertEquals("The event from Bolero has been successfully received", true, manager.CanUpdateLogParentFromEvent(shipment, universalEvent, out var failureReason2));
				AssertEquals("There is no failure reason", ZString.Empty, failureReason2);
			}
		}

		#endregion

		#region Implementation
		CalculateDeliveryDueDateTransportModeCollection ActiveTransportModesForCalculateDeliveryDateOption()
		{
			var activeTransportModes = new CalculateDeliveryDueDateTransportModeCollection();
			activeTransportModes.Add(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail, true);
			return activeTransportModes;
		}

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
			<Type>ForwardingShipment</Type>
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
	<IsForwardRegistered>true</IsForwardRegistered>
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
	<ReleaseType>
		<Code>OBR</Code>
		<Description>Original Bill Required at Destinati</Description>
	</ReleaseType>
	<ServiceLevel>
		<Code>STD</Code>
		<Description>Standard</Description>
	</ServiceLevel>
	<ShipmentIncoTerm>
		<Code>FOB</Code>
		<Description>Free On Board</Description>
	</ShipmentIncoTerm>
	<ShipmentType>
		<Code>STD</Code>
		<Description>Standard House</Description>
	</ShipmentType>
	<ShippedOnBoard>
		<Code>SHP</Code>
		<Description>Shipped</Description>
	</ShippedOnBoard>
	<TotalNoOfPacks>12</TotalNoOfPacks>
	<TotalNoOfPacksPackageType>
		<Code>CTN</Code>
		<Description>Carton</Description>
	</TotalNoOfPacksPackageType>
	<TotalVolume>2.89</TotalVolume>
	<TotalVolumeUnit>
		<Code>M3</Code>
		<Description>Cubic Meters</Description>
	</TotalVolumeUnit>
	<TotalWeight>440.00</TotalWeight>
	<TotalWeightUnit>
		<Code>KG</Code>
		<Description>Kilograms</Description>
	</TotalWeightUnit>
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

	<OrganizationAddressCollection>
		<OrganizationAddress Action=""MERGE"">
		<AddressType>ConsignorDocumentaryAddress</AddressType>
		<OrganizationCode>ABABEU</OrganizationCode>
		<Address1>DIESLSTR 11</Address1>
		<Address2>57439 ATTENDORN, GERMANY</Address2>
		<AddressOverride>false</AddressOverride>
		<City></City>
		<CompanyName>ABA BEUL</CompanyName>
		<Country>
			<Code>DE</Code>
			<Name>Germany</Name>
		</Country>
		<Email></Email>
		<Fax></Fax>
		<Phone></Phone>
		<Postcode></Postcode>
		<ScreeningStatus>
			<Code>UNK</Code>
			<Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
		</OrganizationAddress>
		<OrganizationAddress Action=""MERGE"">
		<AddressType>ConsigneeDocumentaryAddress</AddressType>
		<OrganizationCode>AASDRA</OrganizationCode>
		<Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
		<Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
		<AddressOverride>false</AddressOverride>
		<City></City>
		<CompanyName>A&amp;S FURNISHING CO LTD</CompanyName>
		<Country>
			<Code>HK</Code>
			<Name>Hong Kong</Name>
		</Country>
		<Email></Email>
		<Fax></Fax>
		<Phone></Phone>
		<Postcode></Postcode>
		<ScreeningStatus>
			<Code>UNK</Code>
			<Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
		</OrganizationAddress>
	</OrganizationAddressCollection>
	</Shipment>
</UniversalShipment>
";
			}
		}

		protected override void MakeShipmentUsableForThisRole(RecipientRoleType recipientRoleType, UniversalShipment shipmentWithRecipientRole)
		{
			if (recipientRoleType == RecipientRoleType.FOR || recipientRoleType == RecipientRoleType.RAG || recipientRoleType == RecipientRoleType.SAG
						|| recipientRoleType == RecipientRoleType.PAG || recipientRoleType == RecipientRoleType.DAG
						|| recipientRoleType == RecipientRoleType.NVO)
			{
				shipmentWithRecipientRole.DataContext.ClearDataSourceCollection();
				shipmentWithRecipientRole.DataContext.AddDataSource(DataContextType.ForwardingShipment, null);
			}
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes
		{
			get { return new RecipientRoleType[] { RecipientRoleType.FOR, RecipientRoleType.RAG, RecipientRoleType.SAG, RecipientRoleType.DAG, RecipientRoleType.PAG, RecipientRoleType.NVO }; }
		}

		protected override ServiceCodeType?[] SupportedRecipientServices(RecipientRoleType recipientRole)
		{
			return recipientRole == RecipientRoleType.NVO ? new ServiceCodeType?[] { ServiceCodeType.SIN } : base.SupportedRecipientServices(recipientRole);
		}

		internal static string GetResourcePathFor(string fileName)
		{
			return $"Enterprise.Freight.Forwarding.DataTransfer.Test.Universal.Shipment.TestFiles.{fileName}";
		}

		EmbeddedResourceRetriever ResourceRetriever => new EmbeddedResourceRetriever(GetType().Assembly);

		#endregion
	}
}
