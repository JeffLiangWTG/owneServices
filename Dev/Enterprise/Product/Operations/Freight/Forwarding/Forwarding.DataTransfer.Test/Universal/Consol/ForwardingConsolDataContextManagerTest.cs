using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
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
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.UniversalDataBuss.Matching.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using EventReferenceParameterTypes = Enterprise.Core.Constants.EventReferenceParameterTypes;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(ForwardingConsolDataContextManager))]
	public class ForwardingConsolDataContextManagerTest : ShipmentDataContextManagerTestCase<ForwardingConsolDataContextManager, ForwardingConsol>
	{
		#region Reference and Party ID Matching

		public void TestShipmentsDoNotMatchOnCombinedReferencesWhenImportedOnAConsolWhereReferenceAndPartyIDMatchingIsTurnedOn()
		{
			eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			eAdaptorRegistry.Instance.UniversalXMLEnableVerboseLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var consolDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataContext = consolDataObject.DataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.ForwardingConsol, null);

			consolDataObject.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master };
			consolDataObject.WayBillNumber = "MB29-5874236";

			var orgGenerator = new OrganisationTestHelper(Factory);

			var sendingFwdrDataObject = orgGenerator.CreateDataObject("JACK", "SCHMIDT", "7643");
			sendingFwdrDataObject.AddressType = nameof(DocAddressType.SendingForwarderAddress);

			var receivingFwdrDataObject = orgGenerator.CreateDataObject("PHIL", "MCKRACKIN", "2153");
			receivingFwdrDataObject.AddressType = nameof(DocAddressType.ReceivingForwarderAddress);

			var shippingLineDataObject = orgGenerator.CreateDataObject("SILVERWATER", "CHUTE", "4792");
			shippingLineDataObject.AddressType = nameof(DocAddressType.ShippingLineAddress);

			consolDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { sendingFwdrDataObject, receivingFwdrDataObject, shippingLineDataObject });

			var sendingFwdrBO = orgGenerator.CreateBusinessObject("JACK", "SCHMIDT", "7643");
			var receivingFwdrBO = orgGenerator.CreateBusinessObject("PHIL", "MCKRACKIN", "2153");
			var shippingLineBO = orgGenerator.CreateBusinessObject("SILVERWATER", "CHUTE", "4792");

			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consolDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { shipmentDataObject });
			dataContext = shipmentDataObject.DataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataSource(DataContextType.ForwardingShipment, null);

			shipmentDataObject.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House };
			shipmentDataObject.WayBillNumber = "HB43-123890712";

			var consignorDataObject = orgGenerator.CreateDataObject("JOHN", "THOMPSON", "7808");
			consignorDataObject.AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress);

			var consigneeDataObject = orgGenerator.CreateDataObject("HONEST", "JOE", "3189");
			consigneeDataObject.AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress);

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { consignorDataObject, consigneeDataObject });

			shipmentDataObject.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.LocalProcessing.SetOrderNumberCollection(() => new DataObjectList<OrderNumber>()
					{
						new OrderNumber() { OrderReference = "ORDERME-123456" },
						new OrderNumber() { OrderReference = "ORDERME-098765" },
					});

			var shipmentBO = Factory.New<ForwardingShipment>();
			var consignorBO = orgGenerator.CreateBusinessObject("SOMEONE", "ELSE", "2078");
			shipmentBO.ConsignorPK = consignorBO.PK;
			var consigneeBO = orgGenerator.CreateBusinessObject("DON'T", "KNOW-WHO", "8734");
			shipmentBO.ConsigneePK = consigneeBO.PK;
			var orders = shipmentBO.DocsAndCartage.OrderItems;
			orders.AddNew().JT_OrderReference = "ORDERME-123456";
			orders.AddNew().JT_OrderReference = "ORDERME-098765";
			Factory.SaveForTesting();

			AssertEquals("Precondition: Should be no Consols in the DB yet.", 0, new BusinessObjectFactory().Load<ForwardingConsol>(new ZQuery()).Length);
			AssertEquals("Precondition: Should be one Shipment in the DB.", 1, new BusinessObjectFactory().Load<ForwardingShipment>(new ZQuery()).Length);

			CombineAssertions(delegate
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(consolDataObject);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='HB43-123890712') from UniversalShipment.
Added Consol (Master Bill='MB29-5874236') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='MB29-5874236') with 2 x OrderItem, 1 x ForwardingShipment.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Starting Reference/Party ID scoring.
Have incoming values for WayBillNumber.
Found no potential matches using any of the incoming values.
No Reference/Party ID matches were found in this module.
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
Starting Reference/Party ID scoring.
Have incoming values for WayBillNumber.
Found no potential matches using any of the incoming values.
No Reference/Party ID matches were found in this module.
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: JOHN THOMPSON ENTERPRISES; Address 1: 808 JOHN BOULEVARDE; City: THOMPSONVILLE]'.
Warning - Matching 'ConsigneeDocumentaryAddress':- No match found for '[Company Name: HONEST JOE ENTERPRISES; Address 1: 189 HONEST BOULEVARDE; City: JOEVILLE]'.
No matching OrderItem found, creating new OrderItem.
Populating OrderItem...
No matching OrderItem found, creating new OrderItem.
Populating OrderItem...
Added Shipment (House Bill='HB43-123890712') from UniversalShipment.
Matching 'SendingForwarderAddress':- Matched to 'JACSCHBNE' address '643 JACK BOULEVARDE' with a score of 560.
Matching 'ReceivingForwarderAddress':- Matched to 'PHIMCKSYD' address '153 PHIL BOULEVARDE' with a score of 560.
Matching 'ShippingLineAddress':- Matched to 'SILCHUXRH' address '792 SILVERWATER BOULEVARD' with a score of 560.
Added Consol (Master Bill='MB29-5874236') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='MB29-5874236') with 2 x OrderItem, 1 x ForwardingShipment.
".Trim(), logNoteText);

				var consols = new BusinessObjectFactory().Load<ForwardingConsol>(new ZQuery());
				AssertEquals("consols.Count()", 1, consols.Length);

				var shipments = new BusinessObjectFactory().Load<ForwardingShipment>(new ZQuery());
				AssertEquals("shipments.Count()", 2, shipments.Length);
			});
		}

		public void TestReferenceAndPartyIDMatchingOnAgentsReference()
		{
			eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataContext = shipment.DataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.ForwardingConsol, null);

			shipment.AgentsReference = "AGR-00348790";
			shipment.ReleaseType = new CodeDescriptionPair() { Code = "OMO", Description = "Whiter than White" };

			var orgGenerator = new OrganisationTestHelper(Factory);

			var sendingFwdrDataObject = orgGenerator.CreateDataObject("JACK", "SCHMIDT", "7643");
			sendingFwdrDataObject.AddressType = nameof(DocAddressType.SendingForwarderAddress);

			var receivingFwdrDataObject = orgGenerator.CreateDataObject("PHIL", "MCKRACKIN", "2153");
			receivingFwdrDataObject.AddressType = nameof(DocAddressType.ReceivingForwarderAddress);

			var shippingLineDataObject = orgGenerator.CreateDataObject("SILVERWATER", "CHUTE", "4792");
			shippingLineDataObject.AddressType = nameof(DocAddressType.ShippingLineAddress);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { sendingFwdrDataObject, receivingFwdrDataObject, shippingLineDataObject });

			var sendingFwdrBO = orgGenerator.CreateBusinessObject("JACK", "SCHMIDT", "7643");
			var receivingFwdrBO = orgGenerator.CreateBusinessObject("PHIL", "MCKRACKIN", "2153");
			var shippingLineBO = orgGenerator.CreateBusinessObject("SILVERWATER", "CHUTE", "4792");
			Factory.SaveForTesting();

			AssertEquals("Precondition: Should be no Consols in the DB yet.", null, new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery()));

			CombineAssertions(delegate
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(shipment);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Consol from UniversalShipment.
Successfully saved Consol C00001000.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No Reference/Party ID matches were found in this module.
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
Matching 'SendingForwarderAddress':- Matched to 'JACSCHBNE' address '643 JACK BOULEVARDE' with a score of 560.
Matching 'ReceivingForwarderAddress':- Matched to 'PHIMCKSYD' address '153 PHIL BOULEVARDE' with a score of 560.
Matching 'ShippingLineAddress':- Matched to 'SILCHUXRH' address '792 SILVERWATER BOULEVARD' with a score of 560.
Added Consol from UniversalShipment.
Successfully saved Consol C00001000.
".Trim(), logNoteText);

				var consols = new BusinessObjectFactory().Load<ForwardingConsol>(new ZQuery());
				AssertEquals("consols.Count()", 1, consols.Length);
				var consol = consols[0];
				AssertEquals("consol.JK_AgentsReference", "AGR-00348790", consol.JK_AgentsReference);
				AssertEquals("consol.JK_ReleaseType", "OMO", consol.JK_ReleaseType);
			});

			shipment.ReleaseType = new CodeDescriptionPair() { Code = "OXO", Description = "Gravy just got Groovy" };

			CombineAssertions(delegate
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(shipment);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Updated Consol C00001000 from UniversalShipment.
Successfully saved Consol C00001000.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Matched to Consol C00001000 with a Reference/Party ID match score of 90.
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
Matching 'SendingForwarderAddress':- Matched to 'JACSCHBNE' address '643 JACK BOULEVARDE' with a score of 560.
Matching 'ReceivingForwarderAddress':- Matched to 'PHIMCKSYD' address '153 PHIL BOULEVARDE' with a score of 560.
Matching 'ShippingLineAddress':- Matched to 'SILCHUXRH' address '792 SILVERWATER BOULEVARD' with a score of 560.
Updated Consol C00001000 from UniversalShipment.
Successfully saved Consol C00001000.
".Trim(), logNoteText);

				var consols = new BusinessObjectFactory().Load<ForwardingConsol>(new ZQuery());
				AssertEquals("consols.Count()", 1, consols.Length);
				var consol = consols[0];
				AssertEquals("consol.JK_AgentsReference", "AGR-00348790", consol.JK_AgentsReference);
				AssertEquals("consol.JK_ReleaseType", "OXO", consol.JK_ReleaseType);
			});
		}

		public void TestReferenceAndPartyIDMatchingOnBookingReference()
		{
			eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataContext = shipment.DataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.ForwardingConsol, null);

			shipment.BookingConfirmationReference = "C2015442375";
			shipment.ReleaseType = new CodeDescriptionPair() { Code = "OMO", Description = "Whiter than White" };

			var orgGenerator = new OrganisationTestHelper(Factory);

			var sendingFwdrDataObject = orgGenerator.CreateDataObject("JACK", "SCHMIDT", "7643");
			sendingFwdrDataObject.AddressType = nameof(DocAddressType.SendingForwarderAddress);

			var receivingFwdrDataObject = orgGenerator.CreateDataObject("PHIL", "MCKRACKIN", "2153");
			receivingFwdrDataObject.AddressType = nameof(DocAddressType.ReceivingForwarderAddress);

			var shippingLineDataObject = orgGenerator.CreateDataObject("SILVERWATER", "CHUTE", "4792");
			shippingLineDataObject.AddressType = nameof(DocAddressType.ShippingLineAddress);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { sendingFwdrDataObject, receivingFwdrDataObject, shippingLineDataObject });

			var sendingFwdrBO = orgGenerator.CreateBusinessObject("JACK", "SCHMIDT", "7643");
			var receivingFwdrBO = orgGenerator.CreateBusinessObject("PHIL", "MCKRACKIN", "2153");
			var shippingLineBO = orgGenerator.CreateBusinessObject("SILVERWATER", "CHUTE", "4792");
			Factory.SaveForTesting();

			AssertEquals("Precondition: Should be no Consols in the DB yet.", null, new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery()));

			CombineAssertions(delegate
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(shipment);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Consol from UniversalShipment.
Successfully saved Consol C00001000.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No Reference/Party ID matches were found in this module.
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
Matching 'SendingForwarderAddress':- Matched to 'JACSCHBNE' address '643 JACK BOULEVARDE' with a score of 560.
Matching 'ReceivingForwarderAddress':- Matched to 'PHIMCKSYD' address '153 PHIL BOULEVARDE' with a score of 560.
Matching 'ShippingLineAddress':- Matched to 'SILCHUXRH' address '792 SILVERWATER BOULEVARD' with a score of 560.
Added Consol from UniversalShipment.
Successfully saved Consol C00001000.
".Trim(), logNoteText);

				var consols = new BusinessObjectFactory().Load<ForwardingConsol>(new ZQuery());
				AssertEquals("consols.Count()", 1, consols.Length);
				var consol = consols[0];
				AssertEquals("consol.JK_BookingReference", "C2015442375", consol.JK_BookingReference);
				AssertEquals("consol.JK_ReleaseType", "OMO", consol.JK_ReleaseType);
			});

			shipment.ReleaseType = new CodeDescriptionPair() { Code = "OXO", Description = "Gravy just got Groovy" };

			CombineAssertions(delegate
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(shipment);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Updated Consol C00001000 from UniversalShipment.
Successfully saved Consol C00001000.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Matched to Consol C00001000 with a Reference/Party ID match score of 90.
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
Matching 'SendingForwarderAddress':- Matched to 'JACSCHBNE' address '643 JACK BOULEVARDE' with a score of 560.
Matching 'ReceivingForwarderAddress':- Matched to 'PHIMCKSYD' address '153 PHIL BOULEVARDE' with a score of 560.
Matching 'ShippingLineAddress':- Matched to 'SILCHUXRH' address '792 SILVERWATER BOULEVARD' with a score of 560.
Updated Consol C00001000 from UniversalShipment.
Successfully saved Consol C00001000.
".Trim(), logNoteText);

				var consols = new BusinessObjectFactory().Load<ForwardingConsol>(new ZQuery());
				AssertEquals("consols.Count()", 1, consols.Length);
				var consol = consols[0];
				AssertEquals("consol.JK_BookingReference", "C2015442375", consol.JK_BookingReference);
				AssertEquals("consol.JK_ReleaseType", "OXO", consol.JK_ReleaseType);
			});
		}

		public void TestReferenceAndPartyIDMatchingOnWaybillNumber()
		{
			eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataContext = shipment.DataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.ForwardingConsol, null);

			shipment.WayBillNumber = "MBL08814324";
			shipment.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master };
			shipment.ReleaseType = new CodeDescriptionPair() { Code = "OMO", Description = "Whiter than White" };

			var orgGenerator = new OrganisationTestHelper(Factory);

			var sendingFwdrDataObject = orgGenerator.CreateDataObject("JACK", "SCHMIDT", "7643");
			sendingFwdrDataObject.AddressType = nameof(DocAddressType.SendingForwarderAddress);

			var receivingFwdrDataObject = orgGenerator.CreateDataObject("PHIL", "MCKRACKIN", "2153");
			receivingFwdrDataObject.AddressType = nameof(DocAddressType.ReceivingForwarderAddress);

			var shippingLineDataObject = orgGenerator.CreateDataObject("SILVERWATER", "CHUTE", "4792");
			shippingLineDataObject.AddressType = nameof(DocAddressType.ShippingLineAddress);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { sendingFwdrDataObject, receivingFwdrDataObject, shippingLineDataObject });

			var sendingFwdrBO = orgGenerator.CreateBusinessObject("JACK", "SCHMIDT", "7643");
			var receivingFwdrBO = orgGenerator.CreateBusinessObject("PHIL", "MCKRACKIN", "2153");
			var shippingLineBO = orgGenerator.CreateBusinessObject("SILVERWATER", "CHUTE", "4792");
			Factory.SaveForTesting();

			AssertEquals("Precondition: Should be no Consols in the DB yet.", null, new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery()));

			CombineAssertions(delegate
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(shipment);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Consol (Master Bill='MBL08814324') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='MBL08814324').
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No Reference/Party ID matches were found in this module.
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
Matching 'SendingForwarderAddress':- Matched to 'JACSCHBNE' address '643 JACK BOULEVARDE' with a score of 560.
Matching 'ReceivingForwarderAddress':- Matched to 'PHIMCKSYD' address '153 PHIL BOULEVARDE' with a score of 560.
Matching 'ShippingLineAddress':- Matched to 'SILCHUXRH' address '792 SILVERWATER BOULEVARD' with a score of 560.
Added Consol (Master Bill='MBL08814324') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='MBL08814324').
".Trim(), logNoteText);

				var consols = new BusinessObjectFactory().Load<ForwardingConsol>(new ZQuery());
				AssertEquals("consols.Count()", 1, consols.Length);
				var consol = consols[0];
				AssertEquals("consol.JK_MasterBillNum", "MBL08814324", consol.JK_MasterBillNum);
				AssertEquals("consol.JK_ReleaseType", "OMO", consol.JK_ReleaseType);
			});

			shipment.ReleaseType = new CodeDescriptionPair() { Code = "OXO", Description = "Gravy just got Groovy" };

			CombineAssertions(delegate
			{
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(shipment);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Updated Consol C00001000 (Master Bill='MBL08814324') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='MBL08814324').
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Matched to Consol C00001000 (Master Bill='MBL08814324') with a Reference/Party ID match score of 180.
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
Matching 'SendingForwarderAddress':- Matched to 'JACSCHBNE' address '643 JACK BOULEVARDE' with a score of 560.
Matching 'ReceivingForwarderAddress':- Matched to 'PHIMCKSYD' address '153 PHIL BOULEVARDE' with a score of 560.
Matching 'ShippingLineAddress':- Matched to 'SILCHUXRH' address '792 SILVERWATER BOULEVARD' with a score of 560.
Updated Consol C00001000 (Master Bill='MBL08814324') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='MBL08814324').
".Trim(), logNoteText);

				var consols = new BusinessObjectFactory().Load<ForwardingConsol>(new ZQuery());
				AssertEquals("consols.Count()", 1, consols.Length);
				var consol = consols[0];
				AssertEquals("consol.JK_MasterBillNum", "MBL08814324", consol.JK_MasterBillNum);
				AssertEquals("consol.JK_ReleaseType", "OXO", consol.JK_ReleaseType);
			});
		}

		#endregion

		public void TestUseUnmatchedOrganisationForMatchingFunctionalityWorks()
		{
			var unmatchedOrgPK = OrganizationAddressTestHelper.SetUseUnmatchedOrganisationForMatchingRegistry(true);
			var message = GetQueuedUniversalShipmentMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalShipmentWithUnmatchedOrganisations.xml")));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Consol (Master Bill='I_DO_NOT_EXIST') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='I_DO_NOT_EXIST').
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
Matching 'ReceivingForwarderAddress':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Matching 'SendingForwarderAddress':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Warning - Matching 'NotifyParty':- No match found for '[Company Name: TERRY TOWELLERS INC; Address 1: 238 APTITUDE PLAZA; Address 2: FANTASY VALLEY BUSINESS CENTRE; City: FANTASY VALLEY]'.
Matching 'ShippingLineAddress':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Matching 'Creditor':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Added Consol (Master Bill='I_DO_NOT_EXIST') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='I_DO_NOT_EXIST').
".Trim(), logNoteText);
			});

			var reloadedConsol = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001000"));

			var unmatchedOrgNotes = reloadedConsol.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
			AssertEquals("unmatchedOrgNotes.Length", 1, unmatchedOrgNotes.Length);
			var unmatchedOrgNote = unmatchedOrgNotes[0];
			AssertMultilineASCIIEquals("Unmatched Orgs Note Content", @"
Organisation Type: Receiving Forwarder
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
 
Organisation Type: Sending Forwarder
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
 
Organisation Type: Carrier
Owner Code: 
EDI Code: 
Organisation Name: FLOGGED OGGIN LTD
Address Line 1: 556 WORN OUT ALLEY
Address Line 2: 
City: NIGHTMAREIA
Post Code: 7007
State or Province: WA
Country: AU
Doc Address Type: 
 
Organisation Type: Creditor
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
 ".TrimStart(), unmatchedOrgNote.ST_NoteText);

			AssertEquals("reloadedConsol.CreditorPK", unmatchedOrgPK, reloadedConsol.CreditorPK);
			AssertEquals("reloadedConsol.ReceivingForwarderPK", unmatchedOrgPK, reloadedConsol.ReceivingForwarderPK);
			AssertEquals("reloadedConsol.SendingForwarderPK", unmatchedOrgPK, reloadedConsol.SendingForwarderPK);
			AssertEquals("reloadedConsol.ShippingLinePK", unmatchedOrgPK, reloadedConsol.ShippingLinePK);

			var docAddressTypesPresent = reloadedConsol.DocAddresses
				.OfType<JobDocAddress>()
				.Select(a => a.DocAddressType.ToString())
				.OrderBy(t => t)
				.ToArray();
			AssertEquals("docAddressTypesPresent should not include the 'Client' address or any other 'Extras' not present in the incoming XML", @"
NotifyParty
".Trim(), string.Join("\r\n", docAddressTypesPresent));
		}

		public void TestCreatesNewConsolIfAdditionalReferencesAreCorrectButHasNonMatchingEnterpriseAndServerID()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001010";

			var additionalReference1 = consol.Numbers.AddNew();
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
Added Consol (Master Bill='FUL423189120') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FUL423189120') with 1 x CusEntryNumber.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching CusEntryNumber found, creating new CusEntryNumber.
Populating CusEntryNumber...
Added Consol (Master Bill='FUL423189120') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FUL423189120') with 1 x CusEntryNumber.
".Trim(), logNoteText);
			});
		}

		public void TestImportConsolThroughAdditionalReferencesIfEnterpriseAndServerIDMatch()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001010";

			var additionalReference1 = consol.Numbers.AddNew();
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
Added Shipment (House Bill='HAL3289901890') from UniversalShipment.
Updated Consol C00001010 (Master Bill='FUL423189120') from UniversalShipment.
Successfully saved Consol C00001010 (Master Bill='FUL423189120') with 1 x ForwardingContainer, 1 x Transport, 1 x ForwardingPackLine, 1 x ForwardingShipment, 1 x CusEntryNumber.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingContainer found, creating new ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
No matching Transport found, creating new Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: ZACPT
Attempting to get Schedule for the Transport Leg
An existing Schedule could not be found. The Transport Leg will not be linked.
Transport Leg updated.
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Matching 'ConsignorDocumentaryAddress':- Matched to 'KEMLTD' by code, address 'Pickup and Delivery Addre' with a score of 240.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'DOWCHE' by code, address 'Pickup and Delivery Addre' with a score of 160.
No matching ForwardingPackLine found, creating new ForwardingPackLine.
Populating ForwardingPackLine...
Added Shipment (House Bill='HAL3289901890') from UniversalShipment.
Successfully loaded matching CusEntryNumber.
Populating CusEntryNumber...
Updated Consol C00001010 (Master Bill='FUL423189120') from UniversalShipment.
Successfully saved Consol C00001010 (Master Bill='FUL423189120') with 1 x ForwardingContainer, 1 x Transport, 1 x ForwardingPackLine, 1 x ForwardingShipment, 1 x CusEntryNumber.
".Trim(), logNoteText);
			});
		}

		public void TestDoesNotLinkEventOnAdditionalReferencesIfNonMatchingEnterpriseAndServerID()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001010";

			var additionalReference1 = consol.Numbers.AddNew();
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

				var logs = consol.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookedCode));
				AssertEquals("[BKD] - Booked event count", 0, logs.Length);
			});
		}

		public void TestLinkEventOnAdditionalReferencesIfEnterpriseAndServerIDMatch()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001010";

			var additionalReference1 = consol.Numbers.AddNew();
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
Linked Event to Consol C00001010.
				".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Consol C00001010.
				".Trim(), message.GetLogNoteText());

				var logs = consol.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookedCode));
				AssertEquals("[BKD] - Booked event count", 1, logs.Length);
			});
		}

		public void TestImportConsolWithLegsAndThenShipmentWithLegsDoesNotCreateExtraTransport()
		{
			var message = GetQueuedUniversalShipmentMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalShipmentWithLegsOnConsol.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='IQQ11003862') from UniversalShipment.
Added Consol (Master Bill='14553645692') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='14553645692') with 3 x Transport, 1 x ForwardingShipment.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertEndsWith("message.GetLogNoteText()", @"
Added Shipment (House Bill='IQQ11003862') from UniversalShipment.
Added Consol (Master Bill='14553645692') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='14553645692') with 3 x Transport, 1 x ForwardingShipment.
".Trim(), logNoteText);

				var consol = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, "14553645692"));
				AssertNotNull("Forwarding Consol should exist with a Master Bill of [14553645692].", consol);
				AssertEquals("consol.Shipments.Count", 1, consol.Shipments.Count);
				AssertEquals("consol.Transports.Count", 3, consol.Transports.Count);

				var consolTransport1 = consol.Transports[0];
				AssertEquals("consolTransport1.JW_RL_NKLoadPort", "CLIQQ", consolTransport1.JW_RL_NKLoadPort);
				AssertEquals("consolTransport1.JW_RL_NKDiscPort", "CLSCL", consolTransport1.JW_RL_NKDiscPort);

				var consolTransport2 = consol.Transports[1];
				AssertEquals("consolTransport2.JW_RL_NKLoadPort", "CLSCL", consolTransport2.JW_RL_NKLoadPort);
				AssertEquals("consolTransport2.JW_RL_NKDiscPort", "AUSYD", consolTransport2.JW_RL_NKDiscPort);

				var consolTransport3 = consol.Transports[2];
				AssertEquals("consolTransport3.JW_RL_NKLoadPort", "AUSYD", consolTransport3.JW_RL_NKLoadPort);
				AssertEquals("consolTransport3.JW_RL_NKDiscPort", "AUBNE", consolTransport3.JW_RL_NKDiscPort);

				var shipment = consol.Shipments[0];
				AssertEquals("shipment.JS_HouseBill", "IQQ11003862", shipment.JS_HouseBill);
				AssertEquals("shipment.Transports.Count", 0, shipment.Transports.Count);
			});

			message = GetQueuedUniversalShipmentMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalShipmentWithLegsOnShipment.xml")));

			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Updated Shipment S00001000 (House Bill='IQQ11003862') from UniversalShipment.
Updated Consol C00001000 (Master Bill='14553645692') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='14553645692') with 3 x Transport, 1 x ForwardingPackLine, 1 x ForwardingShipment.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertContains("message.GetLogNoteText()", "Updated Shipment S00001000 (House Bill='IQQ11003862') from UniversalShipment.", logNoteText);
				AssertContains("message.GetLogNoteText()", "Updated Consol C00001000 (Master Bill='14553645692') from UniversalShipment.", logNoteText);

				var consol = new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, "14553645692"));
				AssertNotNull("Forwarding Consol should exist with a Master Bill of [14553645692].", consol);
				AssertEquals("consol.Shipments.Count", 1, consol.Shipments.Count);
				AssertEquals("consol.Transports.Count", 3, consol.Transports.Count);

				var consolTransport1 = consol.Transports[0];
				AssertEquals("consolTransport1.JW_RL_NKLoadPort", "CLIQQ", consolTransport1.JW_RL_NKLoadPort);
				AssertEquals("consolTransport1.JW_RL_NKDiscPort", "CLSCL", consolTransport1.JW_RL_NKDiscPort);

				var consolTransport2 = consol.Transports[1];
				AssertEquals("consolTransport2.JW_RL_NKLoadPort", "CLSCL", consolTransport2.JW_RL_NKLoadPort);
				AssertEquals("consolTransport2.JW_RL_NKDiscPort", "AUSYD", consolTransport2.JW_RL_NKDiscPort);

				var consolTransport3 = consol.Transports[2];
				AssertEquals("consolTransport3.JW_RL_NKLoadPort", "AUSYD", consolTransport3.JW_RL_NKLoadPort);
				AssertEquals("consolTransport3.JW_RL_NKDiscPort", "AUBNE", consolTransport3.JW_RL_NKDiscPort);

				var shipment = consol.Shipments[0];
				AssertEquals("shipment.JS_HouseBill", "IQQ11003862", shipment.JS_HouseBill);
				AssertEquals("shipment.Transports.Count", 0, shipment.Transports.Count);
			});
		}

		public void TestImportShipmentWithLegsAndThenConsolWithLegsDoesNotCreateExtraTransport()
		{
			var message = GetQueuedUniversalShipmentMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalShipmentWithLegsOnShipment.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='IQQ11003862') from UniversalShipment.
Added Consol (Master Bill='14553645692') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='14553645692') with 3 x Transport, 1 x ForwardingPackLine, 1 x ForwardingShipment.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertEndsWith("message.GetLogNoteText()", @"
Added Shipment (House Bill='IQQ11003862') from UniversalShipment.
Added Consol (Master Bill='14553645692') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='14553645692') with 3 x Transport, 1 x ForwardingPackLine, 1 x ForwardingShipment.
".Trim(), logNoteText);

				var consol = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, "14553645692"));
				AssertNotNull("Forwarding Consol should exist with a Master Bill of [14553645692].", consol);
				AssertEquals("consol.Shipments.Count", 1, consol.Shipments.Count);
				AssertEquals("consol.Transports.Count", 1, consol.Transports.Count);

				var shipment = consol.Shipments[0];
				AssertEquals("shipment.JS_HouseBill", "IQQ11003862", shipment.JS_HouseBill);
				AssertEquals("shipment.Transports.Count", 3, shipment.Transports.Count);

				var shipmentTransport1 = shipment.Transports[0];
				AssertEquals("shipmentTransport1.JW_RL_NKLoadPort", "CLIQQ", shipmentTransport1.JW_RL_NKLoadPort);
				AssertEquals("shipmentTransport1.JW_RL_NKDiscPort", "CLSCL", shipmentTransport1.JW_RL_NKDiscPort);

				var shipmentTransport2 = shipment.Transports[1];
				AssertEquals("shipmentTransport2.JW_RL_NKLoadPort", "CLSCL", shipmentTransport2.JW_RL_NKLoadPort);
				AssertEquals("shipmentTransport2.JW_RL_NKDiscPort", "AUSYD", shipmentTransport2.JW_RL_NKDiscPort);

				var shipmentTransport3 = shipment.Transports[2];
				AssertEquals("shipmentTransport3.JW_RL_NKLoadPort", "AUSYD", shipmentTransport3.JW_RL_NKLoadPort);
				AssertEquals("shipmentTransport3.JW_RL_NKDiscPort", "AUBNE", shipmentTransport3.JW_RL_NKDiscPort);
			});

			message = GetQueuedUniversalShipmentMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalShipmentWithLegsOnConsol.xml")));

			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Updated Shipment S00001000 (House Bill='IQQ11003862') from UniversalShipment.
Updated Consol C00001000 (Master Bill='14553645692') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='14553645692') with 3 x Transport, 1 x ForwardingShipment.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertContains("message.GetLogNoteText()", "Updated Shipment S00001000 (House Bill='IQQ11003862') from UniversalShipment.", logNoteText);
				AssertContains("message.GetLogNoteText()", "Updated Consol C00001000 (Master Bill='14553645692') from UniversalShipment.", logNoteText);

				var consol = new BusinessObjectFactory().LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, "14553645692"));
				AssertNotNull("Forwarding Consol should exist with a Master Bill of [14553645692].", consol);
				AssertEquals("consol.Shipments.Count", 1, consol.Shipments.Count);
				AssertEquals("consol.Transports.Count", 3, consol.Transports.Count);

				var consolTransport1 = consol.Transports[0];
				AssertEquals("consolTransport1.JW_RL_NKLoadPort", "CLIQQ", consolTransport1.JW_RL_NKLoadPort);
				AssertEquals("consolTransport1.JW_RL_NKDiscPort", "CLSCL", consolTransport1.JW_RL_NKDiscPort);

				var consolTransport2 = consol.Transports[1];
				AssertEquals("consolTransport2.JW_RL_NKLoadPort", "CLSCL", consolTransport2.JW_RL_NKLoadPort);
				AssertEquals("consolTransport2.JW_RL_NKDiscPort", "AUSYD", consolTransport2.JW_RL_NKDiscPort);

				var consolTransport3 = consol.Transports[2];
				AssertEquals("consolTransport3.JW_RL_NKLoadPort", "AUSYD", consolTransport3.JW_RL_NKLoadPort);
				AssertEquals("consolTransport3.JW_RL_NKDiscPort", "AUBNE", consolTransport3.JW_RL_NKDiscPort);

				var shipment = consol.Shipments[0];
				AssertEquals("shipment.JS_HouseBill", "IQQ11003862", shipment.JS_HouseBill);
				AssertEquals("shipment.Transports.Count", 0, shipment.Transports.Count);
			});
		}

		public void TestCanImportConsolViaUniversalDataBuss()
		{
			var message = GetQueuedUniversalShipmentMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalShipment.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='HAL3289901890') from UniversalShipment.
Added Consol (Master Bill='FUL423189120') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FUL423189120') with 1 x ForwardingContainer, 1 x Transport, 1 x ForwardingPackLine, 1 x ForwardingShipment, 1 x CusEntryNumber.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertContains("message.GetLogNoteText()", "No matching ForwardingConsol found, creating new ForwardingConsol.", logNoteText);
				AssertContains("message.GetLogNoteText()", "Added Consol (Master Bill='FUL423189120') from UniversalShipment.", logNoteText);

				var consol = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, "FUL423189120"));
				AssertNotNull("Forwarding Consol should exist with a Master Bill of [FUL423189120].", consol);
				AssertEquals("consol.Shipments.Count", 1, consol.Shipments.Count);

				var shipment = consol.Shipments[0];
				AssertEquals("shipment.JS_HouseBill", "HAL3289901890", shipment.JS_HouseBill);
			});
		}

		public void TestImporUniversalShipment_WithDataContextKeyNotMatchedBusinessObjectType()
		{
			var newFactory = new BusinessObjectFactory();

			var workflowTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			workflowTemplate.P0_ProcessType = JobInvoicingConsumerTypes.CFSLoadList.Code;
			workflowTemplate.P0_SubType1 = Constants.TransportModes.Sea;
			workflowTemplate.WorkflowItems.AddNew();
			newFactory.Save();

			var cfsConsol = (CommonConsol)newFactory.New<CFS.ICFSLoadListConsol>();
			cfsConsol.JK_UniqueConsignRef = "L00100001";

			var container = cfsConsol.Containers.AddNew();
			container.JC_ContainerNum = "VNXU2505692";

			var workflowItem = ((IWorkflowProvider)cfsConsol).WorkflowItems.AddNew();
			workflowItem.P9_Description = "Test Milestone";
			workflowItem.P9_IsPublished = true;
			workflowItem.SetMilestoneActualDateForTest(DateTime.Now);
			workflowItem.P9_Sequence = 1;
			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Shipment>
    <DataContext>
      <EnterpriseID>MFI</EnterpriseID>
      <CodesMappedToTarget>true</CodesMappedToTarget>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingConsol</Type>
          <Key>L00100001</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <ContainerCollection Content=""Partial"">
      <Container>
        <ContainerNumber>VNXU2505692</ContainerNumber>
        <ContainerCount>1</ContainerCount>
        <ArrivalCartageComplete>2017-08-05T13:15:51</ArrivalCartageComplete>
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
Successfully loaded matching ForwardingConsol.
Error - Cannot populate ForwardingConsol because:
Matched consol L00100001 is not a Forwarding Consol
Successfully saved, but nothing was reported as being updated.
".Trim(), manager.Logger.ToString());
			});
		}

		public void TestImportConsolThroughJobNumber()
		{
			var consolBOToLoad = Factory.New<ForwardingConsol>();
			consolBOToLoad.JK_UniqueConsignRef = "C00001003";
			consolBOToLoad.JK_BookingReference = "THISFIELDWILLNOTBETOUCHED";
			consolBOToLoad.JK_MasterBillNum = "IAMTHEMASTER";

			var dummyConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			dummyConsol.JK_UniqueConsignRef = "C00002010";
			dummyConsol.JK_TransportMode = Constants.TransportModes.Sea;
			dummyConsol.JK_MasterBillNum = "FUL423189120";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalShipmentWithKey.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='HAL3289901890') from UniversalShipment.
Updated Consol C00001003 (Master Bill='FUL423189120') from UniversalShipment.
Successfully saved Consol C00001003 (Master Bill='FUL423189120') with 1 x ForwardingContainer, 1 x Transport, 1 x ForwardingPackLine, 1 x ForwardingShipment, 1 x CusEntryNumber.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertContains("message.GetLogNoteText()", "Successfully loaded matching ForwardingConsol.", logNoteText);

				var consol = new BusinessObjectFactory().LoadFromUniqueKey<ForwardingConsol>(JobConsolSchema.JK_UniqueConsignRef, new ZString("C00001003"));
				AssertNotNull("Loaded Consol.", consol);
				AssertEquals("consol.JK_MasterBillNum", "FUL423189120", consol.JK_MasterBillNum);
				AssertEquals("consol.JK_BookingReference", "THISFIELDWILLNOTBETOUCHED", consol.JK_BookingReference);
				AssertEquals("consol.Shipments.Count", 1, consol.Shipments.Count);

				var shipment = consol.Shipments[0];
				AssertEquals("shipment.JS_HouseBill", "HAL3289901890", shipment.JS_HouseBill);
			});
		}

		public void TestCanImportEventViaUniversalDataBuss()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001010";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "FUL423189120";
			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalEvent.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Consol C00001010 (Master Bill='FUL423189120').
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Consol C00001010 (Master Bill='FUL423189120').
".Trim(), message.GetLogNoteText());

				consol.Reload();
				var logs = consol.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookedCode));
				AssertEquals("[BKD] - Booked event count", 1, logs.Length);
				var log = logs[0];

				var contextItems = log.SourceInfoItems;
				var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select((item) => item.Key + " - " + item.Data).ToArray());
				AssertEquals("Context Items on Event", @"
MBOL Number - FUL423189120
MBOL Origin UNLOCO - AUSYD
MBOL Destination UNLOCO - ZAJNB
AMS Number - 134FREGT
COC - 267AIRGT
Data Source Company - EDI - Eagle Datamation International
Data Source Enterprise ID - EDI
Data Source Server ID - DAT
".Trim(), actualContextItems);
			});
		}

		public void TestNonCFSConsolIsNotMatchedAsAnEventParent()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_UniqueConsignRef = "C00001286";
			consol.JK_MasterBillNum = "FUL423189121";
			consol.JK_BookingReference = "YCH452872";
			consol.JK_IsCFS = false;

			var containerConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			containerConsol.JK_TransportMode = Constants.TransportModes.Sea;
			containerConsol.JK_UniqueConsignRef = "C00001287";

			var container = containerConsol.Containers.AddNew();
			container.JC_ContainerNum = "OCGU2102894";
			Factory.SaveForTesting();

			const string consolLevelEventXmlText = @"
<UniversalEvent>
	<Event>
	<DataContext>
		<DataProvider>CargoWise One</DataProvider>
	</DataContext>
	<EventTime>2015-09-02T08:40:00</EventTime>
	<EventType>FLO</EventType>
	<EventReference>Loaded on Rail</EventReference>
	<EventParameters>
		<Facility>CTO</Facility>
		<Location>USSTP</Location>
	</EventParameters>
	<ContextCollection>
		<Context>
			<Type>CarriersBookingReference</Type>
			<Value>YCH452872</Value>
		</Context>
		<Context>
			<Type>ContainerNumber</Type>
			<Value>OCGU2102894</Value>
		</Context>
		<Context>
			<Type>EventActionUNLOCO</Type>
			<Value>USSTP</Value>
		</Context>
	</ContextCollection>
	</Event>
</UniversalEvent>";

			var message = GetQueuedUniversalEventMessage(consolLevelEventXmlText);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertContainsExactLinesInAnyOrder("Message Log Note", @"
Container number advised: OCGU2102894.
Created new container OCGU2102894.
Linked Event to Container 'OCGU2102894'.
".Trim(), message.GetLogNoteText());

				AssertNotContains("Linked Event to Load List.", message.GetLogNoteText());
			});
		}

		public void TestNonCFSConsolIsNotMatchedAsAnEventParent_NoDuplicates()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_UniqueConsignRef = "C00001286";
			consol.JK_MasterBillNum = "FUL423189121";
			consol.JK_BookingReference = "YCH452872";
			consol.JK_IsCFS = false;

			var containerConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			containerConsol.JK_TransportMode = Constants.TransportModes.Sea;
			containerConsol.JK_UniqueConsignRef = "C00001287";

			var container = containerConsol.Containers.AddNew();
			container.JC_ContainerNum = "OCGU2102894";
			Factory.SaveForTesting();

			const string consolLevelEventXmlText = @"
<UniversalEvent>
	<Event>
	<DataContext>
		<DataProvider>CargoWise One</DataProvider>
	</DataContext>
	<EventTime>2015-09-02T08:40:00</EventTime>
	<EventType>FLO</EventType>
	<EventReference>Loaded on Rail</EventReference>
	<EventParameters>
		<Facility>CTO</Facility>
		<Location>USSTP</Location>
	</EventParameters>
	<ContextCollection>
		<Context>
			<Type>CarriersBookingReference</Type>
			<Value>YCH452872</Value>
		</Context>
		<Context>
			<Type>ContainerNumber</Type>
			<Value>OCGU2102894</Value>
		</Context>
		<Context>
			<Type>ContainerNumber</Type>
			<Value>OCGU2102894</Value>
		</Context>
		<Context>
			<Type>EventActionUNLOCO</Type>
			<Value>USSTP</Value>
		</Context>
	</ContextCollection>
	</Event>
</UniversalEvent>";

			var message = GetQueuedUniversalEventMessage(consolLevelEventXmlText);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertContainsExactLinesInAnyOrder("Message Log Note", @"
Warning - Ignoring 1 duplicate(s) of Container Number [OCGU2102894]
Container number advised: OCGU2102894.
Created new container OCGU2102894.
Linked Event to Container 'OCGU2102894'.
".Trim(), message.GetLogNoteText());
				consol.Containers.Reload(true);
				AssertEquals(1, consol.Containers.Count);

				AssertNotContains("Linked Event to Load List.", message.GetLogNoteText());
			});
		}

		public void TestImportEventWithDataTargetDoesNotUseContextInformationIfDataTargetMatches()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001010";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "IAMTHEMASTER";

			var dummyConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			dummyConsol.JK_UniqueConsignRef = "C00002010";
			dummyConsol.JK_TransportMode = Constants.TransportModes.Sea;
			dummyConsol.JK_MasterBillNum = "FUL423189120";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalEventWithDataTarget.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Consol C00001010 (Master Bill='IAMTHEMASTER').
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Consol C00001010 (Master Bill='IAMTHEMASTER').
".Trim(), message.GetLogNoteText());

				consol.Reload();
				var logs = consol.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookedCode));
				AssertEquals("[BKD] - Booked event count", 1, logs.Length);
				var log = logs[0];

				var contextItems = log.SourceInfoItems;
				var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select((item) => item.Key + " - " + item.Data).ToArray());
				AssertEquals("Context Items on Event", @"
MBOL Number - FUL423189120
MBOL Origin UNLOCO - AUSYD
MBOL Destination UNLOCO - ZAJNB
Data Source Company - EDI - Eagle Datamation International
Data Source Enterprise ID - EDI
Data Source Server ID - DAT
".Trim(), actualContextItems);
			});
		}

		public void TestMultipleConsolsInOneYearRangeAreNotMatched()
		{
			var now = ZDateTime.Now;

			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_UniqueConsignRef = "CON01";
			consol1.JK_BookingReference = "BR0000";
			consol1.JK_MasterBillNum = "20170820A";
			consol1.JK_SystemCreateTimeUtc = now;

			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_UniqueConsignRef = "CON02";
			consol2.JK_BookingReference = "BR0000";
			consol2.JK_MasterBillNum = "20170820B";
			consol2.JK_SystemCreateTimeUtc = now.AddDays(10);

			Factory.SaveForTesting();

			const string consolLevelEventXmlText = @"
<UniversalEvent>
	<Event>
	<DataContext>
		<DataProvider>CargoWise One</DataProvider>
	</DataContext>
	<EventTime>2017-08-21T08:40:00</EventTime>
	<EventType>FLO</EventType>
	<EventReference>Loaded on Rail</EventReference>
	<EventParameters>
		<Facility>CTO</Facility>
		<Location>USSTP</Location>
	</EventParameters>
	<ContextCollection>
		<Context>
			<Type>CarriersBookingReference</Type>
			<Value>BR0000</Value>
		</Context>
		<Context>
			<Type>MBOLNumber</Type>
			<Value>20170820</Value>
		</Context>
	</ContextCollection>
	</Event>
</UniversalEvent>";

			var message = GetQueuedUniversalEventMessage(consolLevelEventXmlText);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);

				AssertContainsExactLinesInAnyOrder("Message Log Note", @"
System cannot find a Consol to link as there are multiple consols with the same Booking Number BR0000.
Consolidations with the same Booking Number:
CON01,CON02
Warning - No Module found a Business Entity to link this Universal Event to.
Message Discarded.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestMultipleCoLoadConsolsInOneYearRangeAreNotMatched()
		{
			var now = ZDateTime.Now;

			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_AgentType = Constants.AgentType.CoLoad;
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_UniqueConsignRef = "CON01";
			consol1.JK_CoLoadBookingReference = "BR0000";
			consol1.JK_CoLoadMasterBill = "20170820A";
			consol1.JK_SystemCreateTimeUtc = now;

			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.JK_AgentType = Constants.AgentType.CoLoad;
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_UniqueConsignRef = "CON02";
			consol2.JK_CoLoadBookingReference = "BR0000";
			consol2.JK_CoLoadMasterBill = "20170820A";
			consol2.JK_SystemCreateTimeUtc = now.AddDays(10);

			Factory.SaveForTesting();

			const string consolLevelEventXmlText = @"
<UniversalEvent>
	<Event>
	<DataContext>
		<DataProvider>CargoWise One</DataProvider>
	</DataContext>
	<EventTime>2017-08-21T08:40:00</EventTime>
	<EventType>FLO</EventType>
	<EventReference>Loaded on Rail</EventReference>
	<EventParameters>
		<Facility>CTO</Facility>
		<Location>USSTP</Location>
	</EventParameters>
	<ContextCollection>
		<Context>
			<Type>CarriersBookingReference</Type>
			<Value>BR0000</Value>
		</Context>
		<Context>
			<Type>MBOLNumber</Type>
			<Value>20170820</Value>
		</Context>
	</ContextCollection>
	</Event>
</UniversalEvent>";

			var message = GetQueuedUniversalEventMessage(consolLevelEventXmlText);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			//CombineAssertions(delegate
			//{
				AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);

				AssertContainsExactLinesInAnyOrder("Message Log Note", @"
System cannot find a Consol to link as there are multiple consols with the same Booking Number BR0000.
Consolidations with the same Booking Number:
CON01,CON02
Warning - No Module found a Business Entity to link this Universal Event to.
Message Discarded.
".Trim(), message.GetLogNoteText());
			//});
		}

		public void TestContextInformationIsAllThereForAir()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_BookingReference = "BOOKME";
			consol.JK_AgentsReference = "DOUBLEAGENT";
			consol.JK_MasterBillNum = "08112345675";
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_ShippingLineAddress = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "11111", "US").MainAddress.PK;

			var additionalReference1 = consol.Numbers.AddNew();
			additionalReference1.CE_EntryNum = "CE00001";
			additionalReference1.CE_EntryType = "AMS";
			additionalReference1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var additionalReference2 = consol.Numbers.AddNew();
			additionalReference2.CE_EntryNum = "CE00002";
			additionalReference2.CE_EntryType = "COC";
			additionalReference2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var manager = consol.GetUniversalDataContextManager() as IEventDataContextManager;

			var eventContextValues = string.Join("\r\n", manager.EventContextValues.Select(o => o.Key + " - " + o.Value).ToArray());

			AssertMultilineASCIIEquals("manager.EventContextValues", @"
MAWBNumber - 081-12345675
MAWBOriginIATAAirportCode - LAX
MAWBDestinationIATAAirportCode - SYD
MBOLOriginUNLOCO - USLAX
MBOLDestinationUNLOCO - AUSYD
CarriersBookingReference - BOOKME
AgentsReference - DOUBLEAGENT
CarrierCode - 11111
AMS Number - CE00001
Customs Office Code (Override) - CE00002
".Trim(), eventContextValues);
		}

		public void TestContextInformationIsAllThereForSea()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_BookingReference = "BOOKME";
			consol.JK_AgentsReference = "DOUBLEAGENT";
			consol.JK_MasterBillNum = "MB8112345675";
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var additionalReference1 = consol.Numbers.AddNew();
			additionalReference1.CE_EntryNum = "CE00001";
			additionalReference1.CE_EntryType = "AMS";
			additionalReference1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var additionalReference2 = consol.Numbers.AddNew();
			additionalReference2.CE_EntryNum = "CE00002";
			additionalReference2.CE_EntryType = "COC";
			additionalReference2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var manager = consol.GetUniversalDataContextManager() as IEventDataContextManager;

			var eventContextValues = string.Join("\r\n", manager.EventContextValues.Select(o => o.Key + " - " + o.Value).ToArray());

			AssertMultilineASCIIEquals("manager.EventContextValues", @"
MBOLNumber - MB8112345675
MBOLOriginUNLOCO - USLAX
MBOLDestinationUNLOCO - AUSYD
CarriersBookingReference - BOOKME
AgentsReference - DOUBLEAGENT
AMS Number - CE00001
Customs Office Code (Override) - CE00002
".Trim(), eventContextValues);
		}

		public void TestCoLoadConsolUniversalExportBookingReferenceIsNotEmpty()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_AgentType = Constants.AgentType.CoLoad;
			consol1.JK_MasterBillNum = "1234";
			consol1.JK_BookingReference = "BOOK";
			consol1.JK_CoLoadMasterBill = "CLDMB1";
			consol1.Transports[0].JW_ETD = ZDateTime.Now.AddDays(7);

			var manager = consol1.GetUniversalDataContextManager() as IEventDataContextManager;
			var elem = manager.EventContextValues.First(v => v.Key.Type == "CarriersBookingReference");
			AssertNotNull(elem);
			AssertEquals("Co-Load consol CarriersBookingReference", "BOOK", elem.Value);

			consol1.JK_AgentType = Constants.AgentType.Agent;
			manager = consol1.GetUniversalDataContextManager() as IEventDataContextManager;
			elem = manager.EventContextValues.First(v => v.Key.Type == "CarriersBookingReference");
			AssertNotNull(elem);
			AssertEquals("Agent consol CarriersBookingReference", "BOOK", elem.Value);
		}

		public void TestConsolMatchesWithVersionedCSRNumber()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001010";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "FUL423189120";

			var entryNum = consol.Numbers.AddNew();
			entryNum.CE_EntryType = ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierShipperReference;
			entryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNum.CE_EntryIsSystemGenerated = true;
			entryNum.CE_EntryNum = "C00001010-V3";

			Factory.SaveForTesting();

			const string xmlMessage = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
    <Shipment>
        <DataContext>
            <Action>LinkOnly</Action>
            <DocumentaryOverride>
                <DocumentName>Booking Confirmation</DocumentName>
            </DocumentaryOverride>
            <DataTargetCollection>
                <DataTarget>
                    <Key>C00001010-V3</Key>
                    <Type>ForwardingConsol</Type>
                </DataTarget>
            </DataTargetCollection>
        </DataContext>
    </Shipment>
</UniversalShipment>";

			var message = GetQueuedUniversalShipmentMessage(xmlMessage);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("Service Task Log", @"
Universal Shipment data was linked to Consol C00001010 (Master Bill='FUL423189120').
Successfully saved, but nothing was reported as being updated.
".Trim(), manager.Logger.ToString());

			AssertMultilineASCIIEquals("Message Log Note", @"
Universal Shipment data was linked to Consol C00001010 (Master Bill='FUL423189120').
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestConsolMatchesWithVersionedCSRNumber_WithLowerCSRVersionforDLI()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001010";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "FUL423189120";

			var entryNum = consol.Numbers.AddNew();
			entryNum.CE_EntryType = ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierShipperReference;
			entryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNum.CE_EntryIsSystemGenerated = true;
			entryNum.CE_EntryNum = "C00001010-V4";

			Factory.SaveForTesting();

			const string xmlMessage = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
    <Shipment>
        <DataContext>
            <Action>LinkOnly</Action>
            <DocumentaryOverride>
                <DocumentName>Booking Confirmation</DocumentName>
            </DocumentaryOverride>
            <DataTargetCollection>
                <DataTarget>
                    <Key>C00001010-V3</Key>
                    <Type>ForwardingConsol</Type>
                </DataTarget>
            </DataTargetCollection>
        </DataContext>
    </Shipment>
</UniversalShipment>";

			var message = GetQueuedUniversalShipmentMessage(xmlMessage);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("Service Task Log", @"
Error - Received for previous 'reset to original' version. Check with carrier for possible duplication.
No Module used this Universal Shipment data.
Message Discarded.
".Trim(), manager.Logger.ToString());

			AssertMultilineASCIIEquals("Message Log Note", @"
Error - Received for previous 'reset to original' version. Check with carrier for possible duplication.
No Module used this Universal Shipment data.
Message Discarded.
".Trim(), message.GetLogNoteText());
		}

		public void TestMatchForCarrierShipperReferenceNumber_MatchTargetUsingTrimmedTargetKey()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001010";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "FUL423189120";

			var entryNum = consol.Numbers.AddNew();
			entryNum.CE_EntryType = ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierShipperReference;
			entryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNum.CE_EntryIsSystemGenerated = true;
			entryNum.CE_EntryNum = "C00001010-V3";

			Factory.SaveForTesting();

			var universalEvent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
    <Event>
        <DataContext>
            <DocumentaryOverride>
                <DocumentName>Shipping Instruction</DocumentName>
            </DocumentaryOverride>
            <DataTargetCollection>
                <DataTarget>
                    <Key>C00001010-V3</Key>
                    <Type>ForwardingConsol</Type>
                </DataTarget>
            </DataTargetCollection>
        </DataContext>
        <EventTime>2020-05-11T10:16:28.17</EventTime>
        <EventType>MRJ</EventType>
        <EventParameters>
            <Department>Carrier</Department>
            <MessageType>Shipping Instruction</MessageType>
            <Reason>Shipping Instruction Rejected, MISSED THE CUTOFF TIME</Reason>
        </EventParameters>
        <EventReference />
    </Event>
</UniversalEvent>";

			var message = GetQueuedUniversalEventMessage(universalEvent);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Consol C00001010 (Master Bill='FUL423189120').
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Consol C00001010 (Master Bill='FUL423189120').
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestMatchForCarrierShipperReferenceNumber_NotFallbackUsingContext()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLGB";
			consol.JK_MasterBillNum = "MEDUMH836105";
			consol.JK_BookingReference = "363IN0359170620";
			consol.JK_UniqueConsignRef = "C00001011";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "FUL423189120";

			var entryNum = consol.Numbers.AddNew();
			entryNum.CE_EntryType = ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierShipperReference;
			entryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNum.CE_EntryIsSystemGenerated = true;
			entryNum.CE_EntryNum = "C00001011-V3";

			Factory.SaveForTesting();

			var universalEvent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
    <Event>
        <DataContext>
            <DocumentaryOverride>
                <DocumentName>Shipping Instruction</DocumentName>
            </DocumentaryOverride>
            <DataTargetCollection>
                <DataTarget>
                    <Key>C00001010-V3</Key>
                    <Type>ForwardingConsol</Type>
                </DataTarget>
            </DataTargetCollection>
        </DataContext>
        <EventTime>2020-05-11T10:16:28.17</EventTime>
        <EventType>MRJ</EventType>
        <EventReference>Booking Confirmed</EventReference>
        <ContextCollection>
            <Context>
                <Type>MBOLNumber</Type>
                <Value>MEDUMH836105</Value>
            </Context>
            <Context>
                <Type>MBOLOriginUNLOCO</Type>
                <Value>AUSYD</Value>
            </Context>
            <Context>
                <Type>MBOLDestinationUNLOCO</Type>
                <Value>USLGB</Value>
            </Context>
            <Context>
                <Type>CarriersBookingReference</Type>
                <Value>363IN0359170620</Value>
            </Context>
            <Context>
                <Type>CarrierCode</Type>
                <Value>INTT</Value>
            </Context>
            <Context>
                <Type>MessageReference</Type>
                <Value>CEFS0000687169</Value>
            </Context>
        </ContextCollection>
    </Event>
</UniversalEvent>";

			var message = GetQueuedUniversalEventMessage(universalEvent);

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
			});
		}

		public void TestMatchForCarrierShipperReferenceNumber_ReceivedLowerCSR()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001010";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "FUL423189120";

			var entryNum = consol.Numbers.AddNew();
			entryNum.CE_EntryType = ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierShipperReference;
			entryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNum.CE_EntryIsSystemGenerated = true;
			entryNum.CE_EntryNum = "C00001010-V3";

			Factory.SaveForTesting();

			var xmlMessage = @"
<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
        <DataContext>
            <DocumentaryOverride>
                <DocumentName>Booking Request</DocumentName>
            </DocumentaryOverride>
            <DataTargetCollection>
                <DataTarget>
                    <Key>C00001010-V2</Key>
                    <Type>ForwardingConsol</Type>
                </DataTarget>
            </DataTargetCollection>
        </DataContext>
        <BookingConfirmationReference>002929384</BookingConfirmationReference>
        <WayBillNumber>EGLV002929384</WayBillNumber>
    </Shipment>
</UniversalShipment>";

			var message = GetQueuedUniversalShipmentMessage(xmlMessage);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Error, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"
Successfully loaded matching ForwardingConsol.
Error - Cannot populate ForwardingConsol because:
Received for previous 'reset to original' version. Check with carrier for possible duplication.
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
			});
		}

		#region Partial Events

		#region TestCancelDuplicatedPartialEvent

		public void TestCancelIncomingPartialEvent()
		{
			AssertCancelIncomingPartialEvent(Events.FreightLoadedCode);
			AssertCancelIncomingPartialEvent(Events.FreightUnloadedCode);
			AssertCancelIncomingPartialEvent(Events.ReceivedCode);
		}

		void AssertCancelIncomingPartialEvent(string eventCode)
		{
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			var consolNumber = string.Format("C{0}01010", eventCode);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = consolNumber;
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var universalEvent1 = CreateUniversaEventXML_Partial(consolNumber, eventCode, 4, 12, "QF001");
			var message1 = GetQueuedUniversalEventMessage(universalEvent1);
			manager.Process(message1);

			var universalEvent2 = CreateUniversaEventXML_Partial(consolNumber, eventCode, 2, 12, "QF002");
			var message2 = GetQueuedUniversalEventMessage(universalEvent2);
			manager.Process(message2);

			Factory.SaveForTesting();

			var logs = consol.Logs
				.GetAllLogs()
				.Cast<StmALog>()
				.Where(log => log.SL_SE_NKEvent == eventCode);

			AssertContainsExactElementsInAnyOrder(string.Format("Prerequsite; {0} events before processing", eventCode),
				new[]
				{
string.Format(@"{0}|Not Cancelled
Parameters
    FAC|CTO
    FDT|2015-01-01
    LOC|AUSYD
    PTL|4
    TTL|12
    TYP|PARTIAL
    VFL|QF001", eventCode),

string.Format(@"{0}|Not Cancelled
Parameters
    FAC|CTO
    FDT|2015-01-01
    LOC|AUSYD
    PTL|2
    TTL|12
    TYP|PARTIAL
    VFL|QF002", eventCode) },
			logs.Select(FormatLog));

			var universalEvent3 = CreateUniversaEventXML_Partial(consolNumber, eventCode, 4, 12, "QF001");
			var message3 = GetQueuedUniversalEventMessage(universalEvent3);
			manager.Process(message3);

			Factory.SaveForTesting();

			AssertContainsExactElementsInAnyOrder(string.Format("{0} events", eventCode),
				new[]
				{
string.Format(@"{0}|Not Cancelled
Parameters
    FAC|CTO
    FDT|2015-01-01
    LOC|AUSYD
    PTL|4
    TTL|12
    TYP|PARTIAL
    VFL|QF001", eventCode),

string.Format(@"{0}|Not Cancelled
Parameters
    FAC|CTO
    FDT|2015-01-01
    LOC|AUSYD
    PTL|2
    TTL|12
    TYP|PARTIAL
    VFL|QF002", eventCode),

string.Format(@"{0}|Cancelled
Parameters
    FAC|CTO
    FDT|2015-01-01
    LOC|AUSYD
    PTL|4
    TTL|12
    TYP|PARTIAL
    VFL|QF001", eventCode) },
			logs.Select(FormatLog));
		}

		#endregion

		#region TestFullCompletion

		public void TestFullCompletion()
		{
			AssertFullCompletion(Events.FreightLoadedCode);
			AssertFullCompletion(Events.FreightUnloadedCode);
			AssertFullCompletion(Events.ReceivedCode);
		}

		void AssertFullCompletion(string eventCode)
		{
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			var consolNumber = string.Format("C{0}01010", eventCode);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = consolNumber;
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var universalEvent1 = CreateUniversaEventXML_Partial(consolNumber, eventCode, 4, 12, "QF001");
			var message1 = GetQueuedUniversalEventMessage(universalEvent1);
			manager.Process(message1);

			var universalEvent2 = CreateUniversaEventXML_Partial(consolNumber, eventCode, 5, 12, "QF002");
			var message2 = GetQueuedUniversalEventMessage(universalEvent2);
			manager.Process(message2);

			Factory.SaveForTesting();

			var logs = consol.Logs
				.GetAllLogs()
				.Cast<StmALog>()
				.Where(log => log.SL_SE_NKEvent == eventCode);

			AssertContainsExactElementsInAnyOrder(string.Format("Prerequsite; {0} events before processing", eventCode),
				new[]
				{
string.Format(@"{0}|Not Cancelled
Parameters
    FAC|CTO
    FDT|2015-01-01
    LOC|AUSYD
    PTL|4
    TTL|12
    TYP|PARTIAL
    VFL|QF001", eventCode),

string.Format(@"{0}|Not Cancelled
Parameters
    FAC|CTO
    FDT|2015-01-01
    LOC|AUSYD
    PTL|5
    TTL|12
    TYP|PARTIAL
    VFL|QF002", eventCode) },
		logs.Select(FormatLog));

			var universalEvent3 = CreateUniversaEventXML_Partial(consolNumber, eventCode,3,12, "QF003");
			var message3 = GetQueuedUniversalEventMessage(universalEvent3);
			manager.Process(message3);

			Factory.SaveForTesting();

			AssertContainsExactElementsInAnyOrder(string.Format("{0} events", eventCode),
				new[]
				{
string.Format(@"{0}|Not Cancelled
Parameters
    FAC|CTO
    FDT|2015-01-01
    LOC|AUSYD
    PTL|4
    TTL|12
    TYP|PARTIAL
    VFL|QF001", eventCode),

string.Format(@"{0}|Not Cancelled
Parameters
    FAC|CTO
    FDT|2015-01-01
    LOC|AUSYD
    PTL|5
    TTL|12
    TYP|PARTIAL
    VFL|QF002", eventCode),

string.Format(@"{0}|Not Cancelled
Parameters
    FAC|CTO
    FDT|2015-01-01
    LOC|AUSYD
    PTL|3
    TTL|12
    TYP|PARTIAL
    VFL|QF003", eventCode),

string.Format(@"{0}|Not Cancelled
Parameters
    FAC|CTO
    LOC|AUSYD
    TTL|12
    TYP|COMPLETE", eventCode) },
		logs.Select(FormatLog));
		}

		#endregion

		string CreateUniversaEventXML_Partial(string consolNumber, string eventCode, int partial, int total, string voyageFlightNumber)
		{
			var universalEvent = string.Format(
				@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent Version=""1.0"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingConsol</Type>
          <Key>{0}</Key>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>EDI</Code>
        <Name>Eagle Datamation International</Name>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>

    <EventTime>2015-01-12T07:00:00.000</EventTime>
    <EventType>{1}</EventType>
    <EventParameters>
      <Location>AUSYD</Location>
      <Partial>{2}</Partial>
      <Total>{3}</Total>
      <VoyageFlightNumber>{4}</VoyageFlightNumber>
      <FlightDate>2015-01-01</FlightDate>
      <Facility>CTO</Facility>
    </EventParameters>
    <IsEstimate>false</IsEstimate>
    <ContextCollection>
      <Context>
        <Code>FlightNumber</Code>
        <Name>{4}</Name>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>", consolNumber, eventCode, partial, total, voyageFlightNumber);

			return universalEvent;
		}

		string FormatLog(StmALog log)
		{
			var parameters = log.Parameters
				.OrderBy(parameter => parameter.Key)
				.Select(parameter => string.Format("    {0}|{1}", parameter.Key, parameter.Value));

			return string.Format("{0}|{1}\r\nParameters\r\n{2}", log.SL_SE_NKEvent, log.SL_IsCancelled ? "Cancelled" : "Not Cancelled",
				string.Join("\r\n", parameters));
		}

		#endregion

		public void TestEmptyReturnedByIsPopulatedAfterFreightUnloadedEvent()
		{
			var eventCode = Events.FreightUnloadedCode;
			var consolNumber = string.Format("C{0}01010", eventCode);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = consolNumber;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.Transports[0].JW_ETDForBinding = new ZDateTime(2021, 1, 1);
			consol.Transports[0].JW_ETAForBinding = new ZDateTime(2021, 1, 3);
			consol.Transports[0].JW_TerminalAvailabilityDateForBinding = new ZDateTime(2022, 1, 5);
			var container = Factory.New<ForwardingContainer>();
			container.JC_ContainerNum = "ZIMU3144818";
			consol.Containers.Add(container);

			Factory.SaveForTesting();

			var universalEvent = string.Format(
@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent Version=""1.0"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingConsol</Type>
          <Key>{0}</Key>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>EDI</Code>
        <Name>Eagle Datamation International</Name>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>

    <EventTime>2023-04-16T20:15</EventTime>
      <EventType>FUL</EventType>
      <EventParameters>
        <Facility>CTO</Facility>
        <Location>ESVLC</Location>
        <TransportMode>SEA</TransportMode>
      </EventParameters>
      <EventReference />
      <IsEstimate>false</IsEstimate>
      <ContextCollection>
        <Context>
          <Type>Reference</Type>
          <Value>410052aa-86c2-ed11-b390-005056a592a9</Value>
        </Context>
        <Context>
          <Type>CarrierCode</Type>
          <Value>ZIMU</Value>
        </Context>
        <Context>
          <Type>ContainerNumber</Type>
          <Value>ZIMU3144818</Value>
        </Context>
        <Context>
          <Type>VesselName</Type>
          <Value>ZIM VANCOUVER</Value>
        </Context>
        <Context>
          <Type>VoyageNumber</Type>
          <Value>67W</Value>
        </Context>
        <Context>
          <Type>LegOriginUNLOCO</Type>
          <Value>USORF</Value>
        </Context>
        <Context>
          <Type>LegDestinationUNLOCO</Type>
          <Value>ESVLC</Value>
        </Context>
        <Context>
          <Type>ContainerISOCode</Type>
          <Value>22G0</Value>
        </Context>
        <Context>
          <Type>EventSource</Type>
          <Value>Carrier</Value>
        </Context>
        <Context>
          <Type>PreviousRunTime</Type>
          <Value>2023-04-13T10:13:26+10:00</Value>
        </Context>
        <Context>
          <Type>LloydsNumber</Type>
          <Value>9322334</Value>
        </Context>
        <Context>
          <Type>CarriersBookingReference</Type>
          <Value>ZIMUVLC0154985</Value>
        </Context>
        <Context>
          <Type>MBOLNumber</Type>
          <Value>ZIMUVLC0154985</Value>
        </Context>
      </ContextCollection>

  </Event>
</UniversalEvent>", consolNumber, eventCode);

			var message = GetQueuedUniversalEventMessage(universalEvent);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			Factory.SaveForTesting();

			AssertEquals(false, container.JC_EmptyReturnedBy.IsEmpty);
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
          <Type>ForwardingConsol</Type>
          <Key>C00001003</Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
        <Code>EDI</Code>
        <Name>Eagle Datamation International</Name>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <EventType>
        <Code>BKD</Code>
        <Description>Booked</Description>
      </EventType>
      <ServerID>DAT</ServerID>
      <TriggerDescription>Job Booked</TriggerDescription>
      <TriggerType>Trigger</TriggerType>
    </DataContext>

    <AWBServiceLevel>
      <Code>STD</Code>
      <Description>Standard</Description>
    </AWBServiceLevel>
    <ContainerMode>
      <Code>FCL</Code>
      <Description>Full Container Load</Description>
    </ContainerMode>
    <DocumentedChargeable>23.782</DocumentedChargeable>
    <DocumentedVolume>14.7</DocumentedVolume>
    <DocumentedWeight>23782</DocumentedWeight>
    <IsForwardRegistered>true</IsForwardRegistered>
    <ManifestedChargeable>23.782</ManifestedChargeable>
    <ManifestedVolume>14.7</ManifestedVolume>
    <ManifestedWeight>23782</ManifestedWeight>
    <OuterPacks>144</OuterPacks>
    <PaymentMethod>
      <Code>PPD</Code>
      <Description>Prepaid</Description>
    </PaymentMethod>
    <PortOfDischarge>
      <Code>ZAJNB</Code>
      <Name>Johannesburg</Name>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>AUSYD</Code>
      <Name>Sydney</Name>
    </PortOfLoading>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
    <TotalNoOfPacks>12</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>PKG</Code>
      <Description>Package</Description>
    </TotalNoOfPacksPackageType>
    <TotalVolume>14.7</TotalVolume>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Meters</Description>
    </TotalVolumeUnit>
    <TotalWeight>23782</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <VesselName>BUNGA TERATAI 3</VesselName>
    <VoyageFlightNo>343</VoyageFlightNo>
    <WayBillNumber>FUL423189120</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
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

    <ContainerCollection>
      <Container Action=""MERGE"">
        <ContainerNumber>OOCL0000006</ContainerNumber>
        <ContainerType>
          <Code>20GP</Code>
          <Description>Twenty foot general purpose</Description>
          <ISOCode>22G0</ISOCode>
        </ContainerType>
        <DeliveryMode>CY/CY</DeliveryMode>
        <FCL_LCL_AIR>
          <Code>FCL</Code>
          <Description>Full Container Load</Description>
        </FCL_LCL_AIR>
        <GoodsWeight>23782</GoodsWeight>
        <Seal>SEL2389</Seal>
        <WeightUnit>
          <Code>KG</Code>
          <Description>Kilograms</Description>
        </WeightUnit>
      </Container>
    </ContainerCollection>

    <SubShipmentCollection>
      <SubShipment Action=""MERGE"">

        <ActualChargeable>23.782</ActualChargeable>
        <ContainerMode>
          <Code>FCL</Code>
          <Description>Full Container Load</Description>
        </ContainerMode>
        <DocumentedChargeable>23.782</DocumentedChargeable>
        <DocumentedVolume>14.7</DocumentedVolume>
        <DocumentedWeight>23782</DocumentedWeight>
        <GoodsDescription>CONCRETE CLEANER</GoodsDescription>
        <GoodsValue>12900.00</GoodsValue>
        <GoodsValueCurrency>
          <Code>AUD</Code>
          <Description>Australia, Dollars</Description>
        </GoodsValueCurrency>
        <IsForwardRegistered>true</IsForwardRegistered>
        <ManifestedChargeable>23.782</ManifestedChargeable>
        <ManifestedVolume>14.7</ManifestedVolume>
        <ManifestedWeight>23782</ManifestedWeight>
        <NoCopyBills>1</NoCopyBills>
        <NoOriginalBills>0</NoOriginalBills>
        <OuterPacks>12</OuterPacks>
        <OuterPacksPackageType>
          <Code>PLT</Code>
          <Description>Pallet</Description>
        </OuterPacksPackageType>
        <PortOfDestination>
          <Code>AUMEL</Code>
          <Name>Melbourne</Name>
        </PortOfDestination>
        <PortOfDischarge>
          <Code>ZAJNB</Code>
          <Name>Johannesburg</Name>
        </PortOfDischarge>
        <PortOfLoading>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </PortOfLoading>
        <PortOfOrigin>
          <Code>ZAJNB</Code>
          <Name>Johannesburg</Name>
        </PortOfOrigin>
        <ReleaseType>
          <Code>EBL</Code>
          <Description>Express Bill of Lading</Description>
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
        <TotalNoOfPacks>144</TotalNoOfPacks>
        <TotalNoOfPacksPackageType>
          <Code>CTN</Code>
          <Description>Carton</Description>
        </TotalNoOfPacksPackageType>
        <TotalVolume>14.7</TotalVolume>
        <TotalVolumeUnit>
          <Code>M3</Code>
          <Description>Cubic Meters</Description>
        </TotalVolumeUnit>
        <TotalWeight>23782</TotalWeight>
        <TotalWeightUnit>
          <Code>KG</Code>
          <Description>Kilograms</Description>
        </TotalWeightUnit>
        <TransportMode>
          <Code>SEA</Code>
          <Description>Sea Freight</Description>
        </TransportMode>
        <VesselName>BUNGA TERATAI 3</VesselName>
        <VoyageFlightNo>343</VoyageFlightNo>
        <WarehouseLocation></WarehouseLocation>
        <WayBillNumber>HAL3289901890</WayBillNumber>
        <WayBillType>
          <Code>HWB</Code>
          <Description>House Waybill</Description>
        </WayBillType>

        <DateCollection>
          <Date>
            <Type>Departure</Type>
            <IsEstimate>false</IsEstimate>
            <Value>2011-04-19T07:51:00</Value>
          </Date>
          <Date>
            <Type>Arrival</Type>
            <IsEstimate>false</IsEstimate>
            <Value>2011-04-25T07:51:00</Value>
          </Date>
        </DateCollection>

        <OrganizationAddressCollection>
          <OrganizationAddress Action=""MERGE"">
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <OrganizationCode>KEMLTD</OrganizationCode>
            <Address1>KEMIX HOUSE</Address1>
            <Address2>60, KYALAMI BOULEVARD</Address2>
            <AddressOverride>false</AddressOverride>
            <City>MIDRAND, SOUTH AFRICA</City>
            <CompanyName>KEMIX PTY LTD</CompanyName>
            <Country>
              <Code>ZA</Code>
              <Name>South Africa</Name>
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
            <OrganizationCode>DOWCHE</OrganizationCode>
            <Address1>KOROROT CREEK RD</Address1>
            <Address2>ALTONA                                C/- HCA</Address2>
            <AddressOverride>false</AddressOverride>
            <City>AGENCIES</City>
            <CompanyName>DOW CHEMICAL (AUSTRALIA) LTD</CompanyName>
            <Country>
              <Code>AU</Code>
              <Name>Australia</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <Phone></Phone>
            <Postcode></Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State>VIC</State>
          </OrganizationAddress>
        </OrganizationAddressCollection>

        <PackingLineCollection>
          <PackingLine Action=""MERGE"">
            <Commodity>
              <Code>GEN</Code>
              <Description>General</Description>
            </Commodity>
            <ContainerNumber>OOCL0000006</ContainerNumber>
            <ContainerPackingOrder>1</ContainerPackingOrder>
            <PackQty>12</PackQty>
            <PackType>
              <Code>PLT</Code>
              <Description>Pallet</Description>
            </PackType>
            <Volume>14.7</Volume>
            <VolumeUnit>
              <Code>M3</Code>
              <Description>Cubic Meters</Description>
            </VolumeUnit>
            <Weight>23782</Weight>
            <WeightUnit>
              <Code>KG</Code>
              <Description>Kilograms</Description>
            </WeightUnit>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
    </SubShipmentCollection>

    <TransportLegCollection>
      <TransportLeg Action=""MERGE"">
        <PortOfDischarge>
          <Code>ZACPT</Code>
          <Name>Cape Town</Name>
        </PortOfDischarge>
        <PortOfLoading>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </PortOfLoading>
        <LegOrder>1</LegOrder>
        <TransportMode>Sea</TransportMode>
        <EstimatedArrival>2011-04-25T07:51:00</EstimatedArrival>
        <EstimatedDeparture>2011-04-19T07:51:00</EstimatedDeparture>
        <LegType>Main</LegType>
        <VesselName>BUNGA TERATAI 3</VesselName>
        <VoyageFlightNo>343</VoyageFlightNo>
      </TransportLeg>
    </TransportLegCollection>
  </Shipment>
</UniversalShipment>
";
			}
		}

		public void TestImportEventWithEventTypeIRJAndMessageTypeCO2e()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001008";

			var leg1 = consol.Transports.AddNew();
			leg1.JW_RL_NKLoadPort = "AUMEL";
			leg1.JW_RL_NKDiscPort = "SGSIN";

			var leg2 = consol.Transports.AddNew();
			leg2.JW_RL_NKLoadPort = "AUMEL";
			leg2.JW_RL_NKDiscPort = ZString.Empty;

			var leg3 = consol.Transports.AddNew();
			leg3.JW_RL_NKLoadPort = ZString.Empty;
			leg3.JW_RL_NKDiscPort = "SGSIN";

			var leg4 = consol.Transports.AddNew();
			leg4.JW_RL_NKLoadPort = ZString.Empty;
			leg4.JW_RL_NKDiscPort = ZString.Empty;

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalEventWithEventTypeIRJAndMessageTypeCO2e.xml")));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals(CO2eStatusList.Codes.Rejected, consol.GetCO2eStatus());
			AssertEquals(CO2eStatusList.Codes.Rejected, leg1.GetCO2eStatus());
			AssertEquals(CO2eStatusList.Codes.Rejected, leg2.GetCO2eStatus());
			AssertEquals(CO2eStatusList.Codes.Rejected, leg3.GetCO2eStatus());
			AssertEquals(CO2eStatusList.Codes.Rejected, leg4.GetCO2eStatus());

			AssertLog(consol, 1);
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

		protected override void MakeShipmentUsableForThisRole(RecipientRoleType recipientRoleType, UniversalShipment shipmentWithRecipientRole)
		{
			if (recipientRoleType == RecipientRoleType.FOR || recipientRoleType == RecipientRoleType.RAG || recipientRoleType == RecipientRoleType.SAG
						|| recipientRoleType == RecipientRoleType.PAG || recipientRoleType == RecipientRoleType.DAG)
			{
				shipmentWithRecipientRole.DataContext.AddDataSource(DataContextType.ForwardingConsol, null);
			}
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes
		{
			get { return new RecipientRoleType[] { RecipientRoleType.FOR, RecipientRoleType.RAG, RecipientRoleType.SAG, RecipientRoleType.PAG, RecipientRoleType.DAG }; }
		}

		string GetResourcePathFor(string fileName)
		{
			return $"Enterprise.Freight.Forwarding.DataTransfer.Test.Universal.Consol.TestFiles.{fileName}";
		}

		EmbeddedResourceRetriever ResourceRetriever => new EmbeddedResourceRetriever(GetType().Assembly);

		#region VGM Import for Agent Consol

		public void TestVGMImport_Consol_NoMatching()
		{
			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
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
Error - Cannot populate ForwardingConsol because:
XML file contains VGM service code and cannot find a matched consol.
Successfully saved, but nothing was reported as being updated.
".Trim(), manager.Logger.ToString());
			});
		}

		public void TestVGMImport_AgentConsol_MBL_BookingRef()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;
			consol1.JK_MasterBillNum = "PFGBNE224673";
			consol1.JK_UniqueConsignRef = "C00005000";

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_BookingReference = "BOOKREF30000";
			consol2.JK_UniqueConsignRef = "C00005001";

			var consol3 = newFactory.New<ForwardingConsol>();
			consol3.JK_AgentType = Core.Constants.AgentType.Agent;
			consol3.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol3.JK_BookingReference = "BOOKREF30000";
			consol3.JK_MasterBillNum = "PFGBNE224673";
			consol3.JK_UniqueConsignRef = "C00005002";
			SetupContainer(consol3);

			var consol4 = newFactory.New<ForwardingConsol>();
			consol4.JK_AgentType = Core.Constants.AgentType.Agent;
			consol4.JK_BookingReference = "BOOKREF30001";
			consol4.JK_MasterBillNum = "PFGBNE224674";
			consol4.JK_UniqueConsignRef = "C00005003";

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
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
	<BookingConfirmationReference>BOOKREF30000</BookingConfirmationReference>
	<WayBillNumber>PFGBNE224673</WayBillNumber>
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
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'TESTORG9' by code, address 'Test Address1' (only address).
Updated Consol C00005002 (Master Bill='PFGBNE224673') from UniversalShipment.
Successfully saved Consol C00005002 (Master Bill='PFGBNE224673') with 1 x ForwardingContainer.
".Trim(), manager.Logger.ToString());
			});
		}

		public void TestVGMImport_AgentConsol_BookingRef()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;
			consol1.JK_MasterBillNum = "PFGBNE224673";
			consol1.JK_UniqueConsignRef = "C00005000";

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol2.JK_BookingReference = "BOOKREF30000";
			consol2.JK_UniqueConsignRef = "C00005001";
			SetupContainer(consol2);

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
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
	<BookingConfirmationReference>BOOKREF30000</BookingConfirmationReference>
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
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'TESTORG9' by code, address 'Test Address1' (only address).
Updated Consol C00005001 from UniversalShipment.
Successfully saved Consol C00005001 with 1 x ForwardingContainer.
".Trim(), manager.Logger.ToString());
			});
		}

		public void TestVGMImport_AgentConsol_MBL()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;
			consol1.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol1.JK_MasterBillNum = "PFGBNE224673";
			consol1.JK_UniqueConsignRef = "C00005000";
			consol1.JK_BookingReference = ZString.Empty;
			SetupContainer(consol1);

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol2.JK_BookingReference = "BOOKREF30000";
			consol2.JK_UniqueConsignRef = "C00005001";

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
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
	<WayBillNumber>PFGBNE224673</WayBillNumber>
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
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'TESTORG9' by code, address 'Test Address1' (only address).
Updated Consol C00005000 (Master Bill='PFGBNE224673') from UniversalShipment.
Successfully saved Consol C00005000 (Master Bill='PFGBNE224673') with 1 x ForwardingContainer.
".Trim(), manager.Logger.ToString());
			});
		}

		public void TestVGMImport_AgentConsol_MBL_UpdateContainerOnly()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;
			consol1.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol1.JK_MasterBillNum = "PFGBNE224673";
			consol1.JK_UniqueConsignRef = "C00005000";
			consol1.JK_BookingReference = ZString.Empty;
			var container1 = SetupContainer(consol1);

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol2.JK_BookingReference = "BOOKREF30000";
			consol2.JK_UniqueConsignRef = "C00005001";

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
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
	<BookingConfirmationReference>BOOKREF70000</BookingConfirmationReference>
	<WayBillNumber>PFGBNE224673</WayBillNumber>
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
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'TESTORG9' by code, address 'Test Address1' (only address).
Updated Consol C00005000 (Master Bill='PFGBNE224673') from UniversalShipment.
Successfully saved Consol C00005000 (Master Bill='PFGBNE224673') with 1 x ForwardingContainer.
".Trim(), manager.Logger.ToString());

				var newConsol = Factory.Load<ForwardingConsol>(consol1.PK);
				AssertNotNull("newConsol should be loaded", newConsol);
				AssertEquals("JK_BookingReference should not be updated", ZString.Empty, newConsol.JK_BookingReference);

				var newContainer = Factory.Load<ForwardingContainer>(container1.PK);
				AssertNotNull("newContainer should be loaded", newContainer);
				AssertEquals("JC_GrossWeightVerificationDateTime should be updated", new ZDateTime(2018, 6, 6, 13, 0, 0), newContainer.JC_GrossWeightVerificationDateTime);
			});
		}

		public void TestVGMImport_AgentConsol_SCAC_MBL_BookingRef()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;
			consol1.JK_MasterBillNum = "PFGBNE224673";
			consol1.JK_UniqueConsignRef = "C00005000";

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_BookingReference = "BOOKREF30001";
			consol2.JK_UniqueConsignRef = "C00005001";

			var consol3 = newFactory.New<ForwardingConsol>();
			consol3.JK_AgentType = Core.Constants.AgentType.Agent;
			consol3.JK_BookingReference = "BOOKREF30001";
			consol3.JK_MasterBillNum = "PFGBNE224673";
			consol3.JK_UniqueConsignRef = "C00005002";

			var consol4 = newFactory.New<ForwardingConsol>();
			consol4.JK_AgentType = Core.Constants.AgentType.Agent;
			consol4.JK_MasterBillNum = "PFGBNE224674";
			consol4.JK_UniqueConsignRef = "C00005003";

			var consol5 = newFactory.New<ForwardingConsol>();
			consol5.JK_AgentType = Core.Constants.AgentType.Agent;
			consol5.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol5.JK_BookingReference = "BOOKREF30000";
			consol5.JK_MasterBillNum = "PFGBNE224673";
			consol5.JK_UniqueConsignRef = "C00005004";
			var orgAddress = SetupOrgWithSCAC(newFactory);
			consol5.JK_OA_ShippingLineAddress = orgAddress.PK;
			SetupContainer(consol5);

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
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
	<BookingConfirmationReference>BOOKREF30000</BookingConfirmationReference>
	<WayBillNumber>PFGBNE224673</WayBillNumber>
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
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'TESTORG9' by code, address 'Test Address1' (only address).
Updated Consol C00005004 (Master Bill='PFGBNE224673') from UniversalShipment.
Successfully saved Consol C00005004 (Master Bill='PFGBNE224673') with 1 x ForwardingContainer.
".Trim(), manager.Logger.ToString());
			});
		}

		public void TestVGMImport_AgentConsol_SCAC_BookingRef()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;
			consol1.JK_MasterBillNum = "PFGBNE224673";
			consol1.JK_UniqueConsignRef = "C00005000";

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_BookingReference = "BOOKREF30001";
			consol2.JK_UniqueConsignRef = "C00005001";

			var consol3 = newFactory.New<ForwardingConsol>();
			consol3.JK_AgentType = Core.Constants.AgentType.Agent;
			consol3.JK_BookingReference = "BOOKREF30001";
			consol3.JK_MasterBillNum = "PFGBNE224673";
			consol3.JK_UniqueConsignRef = "C00005002";

			var consol4 = newFactory.New<ForwardingConsol>();
			consol4.JK_AgentType = Core.Constants.AgentType.Agent;
			consol4.JK_MasterBillNum = "PFGBNE224674";
			consol4.JK_UniqueConsignRef = "C00005003";

			var consol5 = newFactory.New<ForwardingConsol>();
			consol5.JK_AgentType = Core.Constants.AgentType.Agent;
			consol5.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol5.JK_BookingReference = "BOOKREF30000";
			consol5.JK_UniqueConsignRef = "C00005004";
			var orgAddress = SetupOrgWithSCAC(newFactory);
			consol5.JK_OA_ShippingLineAddress = orgAddress.PK;
			SetupContainer(consol5);

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
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
	<BookingConfirmationReference>BOOKREF30000</BookingConfirmationReference>
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
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'TESTORG9' by code, address 'Test Address1' (only address).
Updated Consol C00005004 from UniversalShipment.
Successfully saved Consol C00005004 with 1 x ForwardingContainer.
".Trim(), manager.Logger.ToString());
			});
		}

		public void TestVGMImport_AgentConsol_SCAC_MBL()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;
			consol1.JK_MasterBillNum = "PFGBNE224673";
			consol1.JK_UniqueConsignRef = "C00005000";

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_BookingReference = "BOOKREF30001";
			consol2.JK_UniqueConsignRef = "C00005001";

			var consol3 = newFactory.New<ForwardingConsol>();
			consol3.JK_AgentType = Core.Constants.AgentType.Agent;
			consol3.JK_BookingReference = "BOOKREF30001";
			consol3.JK_MasterBillNum = "PFGBNE224673";
			consol3.JK_UniqueConsignRef = "C00005002";

			var consol4 = newFactory.New<ForwardingConsol>();
			consol4.JK_AgentType = Core.Constants.AgentType.Agent;
			consol4.JK_MasterBillNum = "PFGBNE224674";
			consol4.JK_UniqueConsignRef = "C00005003";

			var consol5 = newFactory.New<ForwardingConsol>();
			consol5.JK_AgentType = Core.Constants.AgentType.Agent;
			consol5.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol5.JK_MasterBillNum = "PFGBNE224673";
			consol5.JK_UniqueConsignRef = "C00005004";
			var orgAddress = SetupOrgWithSCAC(newFactory);
			consol5.JK_OA_ShippingLineAddress = orgAddress.PK;
			SetupContainer(consol5);

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
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
	<WayBillNumber>PFGBNE224673</WayBillNumber>
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
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'TESTORG9' by code, address 'Test Address1' (only address).
Updated Consol C00005004 (Master Bill='PFGBNE224673') from UniversalShipment.
Successfully saved Consol C00005004 (Master Bill='PFGBNE224673') with 1 x ForwardingContainer.
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

		public void TestVGMImport_AgentConsol_BookingRef_GrossWeightVerificationDateTime_DefaultToNow()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;
			consol1.JK_MasterBillNum = "PFGBNE224673";
			consol1.JK_UniqueConsignRef = "C00005000";

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol2.JK_BookingReference = "BOOKREF30000";
			consol2.JK_UniqueConsignRef = "C00005001";

			var container1 = consol2.Containers.AddNew();
			container1.JC_ContainerNum = "TCLU3117180";
			var refContainer = newFactory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			AssertNotNull(refContainer);
			container1.JC_RC = refContainer.PK;
			container1.JC_ContainerMode = Constants.ContainerModes.FCL;

			var org = newFactory.New<OrgHeader>();
			org.OH_Code = "TESTORG9";
			org.MainAddress.Address1 = "Test Address1";
			container1.GrossWeightVerifiedByAddress.OrganisationPK = org.PK;

			var shipment = consol2.Shipments.AddNew();
			var outerPack = shipment.OuterPackLines.AddNew();
			outerPack.SetContainer(consol2, container1);

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
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
	<BookingConfirmationReference>BOOKREF30000</BookingConfirmationReference>
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
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'TESTORG9' by code, address 'Test Address1' (only address).
Updated Consol C00005001 from UniversalShipment.
Successfully saved Consol C00005001 with 1 x ForwardingContainer.
".Trim(), manager.Logger.ToString());

				var container2 = Factory.Load<CommonContainer>(container1.PK);
				AssertNotNull(container2);
				Assert(!container2.JC_GrossWeightVerificationDateTime.IsEmpty);
			});
		}

		public void TestVGMImport_AgentConsol_BookingRef_Reject_NotExistedContainer()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;
			consol1.JK_MasterBillNum = "PFGBNE224673";
			consol1.JK_UniqueConsignRef = "C00005000";

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol2.JK_BookingReference = "BOOKREF30000";
			consol2.JK_UniqueConsignRef = "C00005001";

			var container1 = consol2.Containers.AddNew();
			container1.JC_ContainerNum = "TCLU3117199";
			var refContainer = newFactory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			AssertNotNull(refContainer);
			container1.JC_RC = refContainer.PK;
			container1.JC_ContainerMode = Constants.ContainerModes.FCL;

			var org = newFactory.New<OrgHeader>();
			org.OH_Code = "TESTORG9";
			org.MainAddress.Address1 = "Test Address1";
			container1.GrossWeightVerifiedByAddress.OrganisationPK = org.PK;

			var shipment = consol2.Shipments.AddNew();
			var outerPack = shipment.OuterPackLines.AddNew();
			outerPack.SetContainer(consol2, container1);

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
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
	<BookingConfirmationReference>BOOKREF30000</BookingConfirmationReference>
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
Successfully loaded matching ForwardingConsol.
Error - Cannot populate ForwardingConsol because:
One or multiple specified containers cannot be found in the matched consol.
Successfully saved, but nothing was reported as being updated.
".Trim(), manager.Logger.ToString());
			});
		}

		public void TestVGMImport_AgentConsol_BookingRef_Reject_ContainerNumberIsEmpty()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;
			consol1.JK_MasterBillNum = "PFGBNE224673";
			consol1.JK_UniqueConsignRef = "C00005000";

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol2.JK_BookingReference = "BOOKREF30000";
			consol2.JK_UniqueConsignRef = "C00005001";

			var container1 = consol2.Containers.AddNew();
			container1.JC_ContainerNum = "TCLU3117199";
			var refContainer = newFactory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			AssertNotNull(refContainer);
			container1.JC_RC = refContainer.PK;

			var container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = ZString.Empty;
			container2.JC_RC = refContainer.PK;
			container2.JC_ContainerMode = Constants.ContainerModes.FCL;

			var org = newFactory.New<OrgHeader>();
			org.OH_Code = "TESTORG9";
			org.MainAddress.Address1 = "Test Address1";
			container1.GrossWeightVerifiedByAddress.OrganisationPK = org.PK;

			var shipment = consol2.Shipments.AddNew();
			var outerPack = shipment.OuterPackLines.AddNew();
			outerPack.SetContainer(consol2, container1);

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
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
	<BookingConfirmationReference>BOOKREF30000</BookingConfirmationReference>
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
Successfully loaded matching ForwardingConsol.
Error - Cannot populate ForwardingConsol because:
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
			consol.JK_BookingReference = "BOOKREF30000";

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
		<DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
				<Key>C00005000</Key>
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
	<BookingConfirmationReference>BOOKREF30000</BookingConfirmationReference>
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
			consol.JK_BookingReference = "BOOKREF30000";

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
		<DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
				<Key>C00005000</Key>
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
	<BookingConfirmationReference>BOOKREF30000</BookingConfirmationReference>
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
Successfully loaded matching ForwardingConsol.
Error - Cannot populate ForwardingConsol because:
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

		public void TestVGMImport_AgentConsol_BookingRef_Reject_NoContainerCollectionDataObject()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;
			consol1.JK_MasterBillNum = "PFGBNE224673";
			consol1.JK_UniqueConsignRef = "C00005000";

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol2.JK_BookingReference = "BOOKREF30000";
			consol2.JK_UniqueConsignRef = "C00005001";

			var container1 = consol2.Containers.AddNew();
			container1.JC_ContainerNum = "TCLU3117199";
			var refContainer = newFactory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			AssertNotNull(refContainer);
			container1.JC_RC = refContainer.PK;
			container1.JC_ContainerMode = Constants.ContainerModes.FCL;

			var container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = ZString.Empty;
			container2.JC_RC = refContainer.PK;
			container2.JC_ContainerMode = Constants.ContainerModes.FCL;

			var org = newFactory.New<OrgHeader>();
			org.OH_Code = "TESTORG9";
			org.MainAddress.Address1 = "Test Address1";
			container1.GrossWeightVerifiedByAddress.OrganisationPK = org.PK;

			var shipment = consol2.Shipments.AddNew();
			var outerPack = shipment.OuterPackLines.AddNew();
			outerPack.SetContainer(consol2, container1);

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
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
	<BookingConfirmationReference>BOOKREF30000</BookingConfirmationReference>
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
Successfully loaded matching ForwardingConsol.
Error - Cannot populate ForwardingConsol because:
XML file does not contain any containers.
Successfully saved, but nothing was reported as being updated.
".Trim(), manager.Logger.ToString());
			});
		}

		public void TestVGMImport_AgentConsol_BookingRef_Reject_NoContainerDataObject()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;
			consol1.JK_MasterBillNum = "PFGBNE224673";
			consol1.JK_UniqueConsignRef = "C00005000";

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol2.JK_BookingReference = "BOOKREF30000";
			consol2.JK_UniqueConsignRef = "C00005001";

			var container1 = consol2.Containers.AddNew();
			container1.JC_ContainerNum = "TCLU3117199";
			var refContainer = newFactory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			AssertNotNull(refContainer);
			container1.JC_RC = refContainer.PK;
			container1.JC_ContainerMode = Constants.ContainerModes.FCL;

			var container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = ZString.Empty;
			container2.JC_RC = refContainer.PK;
			container2.JC_ContainerMode = Constants.ContainerModes.FCL;

			var org = newFactory.New<OrgHeader>();
			org.OH_Code = "TESTORG1";
			org.MainAddress.Address1 = "Test Address1";
			container1.GrossWeightVerifiedByAddress.OrganisationPK = org.PK;

			var shipment = consol2.Shipments.AddNew();
			var outerPack = shipment.OuterPackLines.AddNew();
			outerPack.SetContainer(consol2, container1);

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
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
	<BookingConfirmationReference>BOOKREF30000</BookingConfirmationReference>
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
Successfully loaded matching ForwardingConsol.
Error - Cannot populate ForwardingConsol because:
XML file does not contain any containers.
Successfully saved, but nothing was reported as being updated.
".Trim(), manager.Logger.ToString());
			});
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

		public void TestVGMImport_AgentConsol_MBL_ContainerMode_FCL()
		{
			AssertVGMImport_AgentConsol_MBL_ContainerMode_Applicable(Constants.ContainerModes.FCL);
		}

		public void TestVGMImport_AgentConsol_MBL_ContainerMode_GRP()
		{
			AssertVGMImport_AgentConsol_MBL_ContainerMode_Applicable(Constants.ContainerModes.Groupage);
		}

		public void TestVGMImport_AgentConsol_MBL_ContainerMode_BCN()
		{
			AssertVGMImport_AgentConsol_MBL_ContainerMode_Applicable(Constants.ContainerModes.BuyersConsol);
		}

		public void TestVGMImport_AgentConsol_MBL_ContainerMode_LCL()
		{
			AssertVGMImport_AgentConsol_MBL_ContainerMode_NotApplicable(Constants.ContainerModes.LCL);
		}

		public void TestVGMImport_AgentConsol_MBL_ContainerMode_BLK()
		{
			AssertVGMImport_AgentConsol_MBL_ContainerMode_NotApplicable(Constants.ContainerModes.Bulk);
		}

		public void TestVGMImport_AgentConsol_MBL_ContainerMode_LQD()
		{
			AssertVGMImport_AgentConsol_MBL_ContainerMode_NotApplicable(Constants.ContainerModes.Liquid);
		}

		public void TestVGMImport_AgentConsol_MBL_ContainerMode_BBK()
		{
			AssertVGMImport_AgentConsol_MBL_ContainerMode_NotApplicable(Constants.ContainerModes.BreakBulk);
		}

		public void TestVGMImport_AgentConsol_MBL_ContainerMode_ROR()
		{
			AssertVGMImport_AgentConsol_MBL_ContainerMode_NotApplicable(Constants.ContainerModes.RollOnRollOff);
		}

		public void TestVGMImport_AgentConsol_MBL_ContainerMode_OTH()
		{
			AssertVGMImport_AgentConsol_MBL_ContainerMode_NotApplicable(Constants.ContainerModes.Other);
		}

		#region Implementation

		void AssertVGMImport_AgentConsol_MBL_ContainerMode_Applicable(ZString containerMode)
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;
			consol1.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol1.JK_MasterBillNum = "PFGBNE224673";
			consol1.JK_UniqueConsignRef = "C00005000";
			SetupContainer(consol1, containerMode);

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_BookingReference = "BOOKREF30000";
			consol2.JK_UniqueConsignRef = "C00005001";

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
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
	<WayBillNumber>PFGBNE224673</WayBillNumber>
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
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'TESTORG9' by code, address 'Test Address1' (only address).
Updated Consol C00005000 (Master Bill='PFGBNE224673') from UniversalShipment.
Successfully saved Consol C00005000 (Master Bill='PFGBNE224673') with 1 x ForwardingContainer.
".Trim(), manager.Logger.ToString());
			});
		}

		void AssertVGMImport_AgentConsol_MBL_ContainerMode_NotApplicable(ZString containerMode)
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;
			consol1.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol1.JK_MasterBillNum = "PFGBNE224673";
			consol1.JK_UniqueConsignRef = "C00005000";
			SetupContainer(consol1, containerMode);

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_BookingReference = "BOOKREF30000";
			consol2.JK_UniqueConsignRef = "C00005001";

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
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
	<WayBillNumber>PFGBNE224673</WayBillNumber>
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
Successfully loaded matching ForwardingConsol.
Error - Cannot populate ForwardingConsol because:
Container Mode should be FCL, GRP or BCN.
Successfully saved, but nothing was reported as being updated.
".Trim(), manager.Logger.ToString());
			});
		}

		#endregion

		#endregion

		#region VGM Import for Co-Load Consol with BookingConfirmationReference/WayBillNumber

		public void TestVGMImport_CoLoadConsol_MBL_BookingRef()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol1.JK_CoLoadMasterBill = "PFGBNE224673";
			consol1.JK_UniqueConsignRef = "C00005000";

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol2.JK_CoLoadBookingReference = "BOOKREF30000";
			consol2.JK_UniqueConsignRef = "C00005001";

			var consol3 = newFactory.New<ForwardingConsol>();
			consol3.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol3.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol3.JK_CoLoadBookingReference = "BOOKREF30000";
			consol3.JK_CoLoadMasterBill = "PFGBNE224673";
			consol3.JK_UniqueConsignRef = "C00005002";
			SetupContainer(consol3);

			var consol4 = newFactory.New<ForwardingConsol>();
			consol4.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol4.JK_BookingReference = "BOOKREF30001";
			consol4.JK_CoLoadMasterBill = "PFGBNE224674";
			consol4.JK_UniqueConsignRef = "C00005003";

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
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
	<BookingConfirmationReference>BOOKREF30000</BookingConfirmationReference>
	<WayBillNumber>PFGBNE224673</WayBillNumber>
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
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'TESTORG9' by code, address 'Test Address1' (only address).
Updated Consol C00005002 from UniversalShipment.
Successfully saved Consol C00005002 with 1 x ForwardingContainer.
".Trim(), manager.Logger.ToString());
			});
		}

		public void TestVGMImport_CoLoadConsol_BookingRef()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol1.JK_CoLoadMasterBill = "PFGBNE224673";
			consol1.JK_UniqueConsignRef = "C00005000";

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol2.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol2.JK_CoLoadBookingReference = "BOOKREF30000";
			consol2.JK_UniqueConsignRef = "C00005001";
			SetupContainer(consol2);

			var consol3 = newFactory.New<ForwardingConsol>();
			consol3.JK_AgentType = Core.Constants.AgentType.Agent;
			consol3.JK_CoLoadBookingReference = "BOOKREF30000";
			consol3.JK_UniqueConsignRef = "C00005002";

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
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
	<BookingConfirmationReference>BOOKREF30000</BookingConfirmationReference>
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
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'TESTORG9' by code, address 'Test Address1' (only address).
Updated Consol C00005001 from UniversalShipment.
Successfully saved Consol C00005001 with 1 x ForwardingContainer.
".Trim(), manager.Logger.ToString());
			});
		}

		public void TestVGMImport_CoLoadConsol_MBL()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol1.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			consol1.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol1.JK_CoLoadMasterBill = "PFGBNE224673";
			consol1.JK_UniqueConsignRef = "C00005000";
			SetupContainer(consol1);

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol2.JK_CoLoadBookingReference = "BOOKREF30000";
			consol2.JK_UniqueConsignRef = "C00005001";

			var consol3 = newFactory.New<ForwardingConsol>();
			consol3.JK_AgentType = Core.Constants.AgentType.Agent;
			consol3.JK_CoLoadMasterBill = "PFGBNE224673";
			consol3.JK_UniqueConsignRef = "C00005002";

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
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
	<WayBillNumber>PFGBNE224673</WayBillNumber>
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
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'TESTORG9' by code, address 'Test Address1' (only address).
Updated Consol C00005000 from UniversalShipment.
Successfully saved Consol C00005000 with 1 x ForwardingContainer.
".Trim(), manager.Logger.ToString());
			});
		}

		public void TestVGMImport_CoLoadConsol_SCAC_MBL_BookingRef()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol1.JK_CoLoadMasterBill = "PFGBNE224673";
			consol1.JK_UniqueConsignRef = "C00005000";

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol2.JK_CoLoadBookingReference = "BOOKREF30001";
			consol2.JK_UniqueConsignRef = "C00005001";

			var consol3 = newFactory.New<ForwardingConsol>();
			consol3.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol3.JK_CoLoadBookingReference = "BOOKREF30001";
			consol3.JK_CoLoadMasterBill = "PFGBNE224673";
			consol3.JK_UniqueConsignRef = "C00005002";

			var consol4 = newFactory.New<ForwardingConsol>();
			consol4.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol4.JK_CoLoadMasterBill = "PFGBNE224673";
			consol4.JK_CoLoadBookingReference = "BOOKREF30000";
			consol4.JK_UniqueConsignRef = "C00005003";

			var consol5 = newFactory.New<ForwardingConsol>();
			consol5.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol5.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol5.JK_CoLoadBookingReference = "BOOKREF30000";
			consol5.JK_CoLoadMasterBill = "PFGBNE224673";
			consol5.JK_UniqueConsignRef = "C00005004";
			var orgAddress = SetupOrgWithSCAC(newFactory);
			consol5.JK_OA_CreditorAddress = orgAddress.PK;
			SetupContainer(consol5);

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
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
	<BookingConfirmationReference>BOOKREF30000</BookingConfirmationReference>
	<WayBillNumber>PFGBNE224673</WayBillNumber>
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
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'TESTORG9' by code, address 'Test Address1' (only address).
Updated Consol C00005004 from UniversalShipment.
Successfully saved Consol C00005004 with 1 x ForwardingContainer.
".Trim(), manager.Logger.ToString());
			});
		}

		public void TestVGMImport_CoLoadConsol_SCAC_BookingRef()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol1.JK_CoLoadMasterBill = "PFGBNE224673";
			consol1.JK_UniqueConsignRef = "C00005000";

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol2.JK_CoLoadBookingReference = "BOOKREF30000";
			consol2.JK_UniqueConsignRef = "C00005001";

			var consol3 = newFactory.New<ForwardingConsol>();
			consol3.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol3.JK_CoLoadBookingReference = "BOOKREF30000";
			consol3.JK_CoLoadMasterBill = "PFGBNE224673";
			consol3.JK_UniqueConsignRef = "C00005002";
			var orgAddress1 = SetupOrgWithSCAC(newFactory, "TestOrg1", "ABCD");
			consol3.JK_OA_CreditorAddress = orgAddress1.PK;

			var consol4 = newFactory.New<ForwardingConsol>();
			consol4.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol4.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol4.JK_CoLoadBookingReference = "BOOKREF30000";
			consol4.JK_CoLoadMasterBill = "PFGBNE224673";
			consol4.JK_UniqueConsignRef = "C00005003";
			var orgAddress2 = SetupOrgWithSCAC(newFactory, "TestOrg2");
			consol4.JK_OA_CreditorAddress = orgAddress2.PK;
			SetupContainer(consol4);

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
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
	<BookingConfirmationReference>BOOKREF30000</BookingConfirmationReference>
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
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'TESTORG9' by code, address 'Test Address1' (only address).
Updated Consol C00005003 from UniversalShipment.
Successfully saved Consol C00005003 with 1 x ForwardingContainer.
".Trim(), manager.Logger.ToString());
			});
		}

		public void TestVGMImport_CoLoadConsol_SCAC_MBL()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol1.JK_CoLoadMasterBill = "PFGBNE224673";
			consol1.JK_UniqueConsignRef = "C00005000";

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol2.JK_CoLoadBookingReference = "BOOKREF30000";
			consol2.JK_UniqueConsignRef = "C00005001";

			var consol3 = newFactory.New<ForwardingConsol>();
			consol3.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol3.JK_CoLoadBookingReference = "BOOKREF30000";
			consol3.JK_CoLoadMasterBill = "PFGBNE224673";
			consol3.JK_UniqueConsignRef = "C00005002";

			var consol4 = newFactory.New<ForwardingConsol>();
			consol4.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol4.JK_CoLoadMasterBill = "PFGBNE224673";
			consol4.JK_CoLoadBookingReference = "BOOKREF30000";
			consol4.JK_UniqueConsignRef = "C00005003";
			var orgAddress1 = SetupOrgWithSCAC(newFactory, "TestOrg1", "ABCD");
			consol4.JK_OA_CreditorAddress = orgAddress1.PK;

			var consol5 = newFactory.New<ForwardingConsol>();
			consol5.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol5.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol5.JK_CoLoadBookingReference = "BOOKREF30000";
			consol5.JK_CoLoadMasterBill = "PFGBNE224673";
			consol5.JK_UniqueConsignRef = "C00005004";
			var orgAddress2 = SetupOrgWithSCAC(newFactory, "TestOrg2");
			consol5.JK_OA_CreditorAddress = orgAddress2.PK;
			SetupContainer(consol5);

			var consol6 = newFactory.New<ForwardingConsol>();
			consol6.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol6.JK_CoLoadMasterBill = "PFGBNE224673";
			consol6.JK_UniqueConsignRef = "C00005005";
			var orgAddress3 = SetupOrgWithSCAC(newFactory, "TestOrg3", regType: OrgCusCode.CodeTypes.CargoWiseOneCarrierCode);
			consol6.JK_OA_CreditorAddress = orgAddress3.PK;

			var consol7 = newFactory.New<ForwardingConsol>();
			consol7.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol7.JK_CoLoadMasterBill = "PFGBNE224673";
			consol7.JK_UniqueConsignRef = "C00005006";
			var orgAddress4 = SetupOrgWithSCAC(newFactory, "TestOrg4", country: Constants.CountryCodes.Canada);
			consol7.JK_OA_CreditorAddress = orgAddress4.PK;

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
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
	<WayBillNumber>PFGBNE224673</WayBillNumber>
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
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'TESTORG9' by code, address 'Test Address1' (only address).
Updated Consol C00005004 from UniversalShipment.
Successfully saved Consol C00005004 with 1 x ForwardingContainer.
".Trim(), manager.Logger.ToString());
			});
		}

		public void TestVGMImport_AgentAndCoLoadConsol_SCAC_MBL_BookingRef()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol1.JK_CoLoadBookingReference = "BOOKREF30000";
			consol1.JK_CoLoadMasterBill = "PFGBNE224673";
			consol1.JK_UniqueConsignRef = "C00005000";
			var orgAddress1 = SetupOrgWithSCAC(newFactory, "TestOrg1");
			consol1.JK_OA_CreditorAddress = orgAddress1.PK;

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_BookingReference = "BOOKREF30000";
			consol2.JK_MasterBillNum = "PFGBNE224673";
			consol2.JK_UniqueConsignRef = "C00005001";
			var orgAddress2 = SetupOrgWithSCAC(newFactory, "TestOrg2");
			consol2.JK_OA_ShippingLineAddress = orgAddress2.PK;

			var consol3 = newFactory.New<ForwardingConsol>();
			consol3.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol3.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol3.JK_CoLoadBookingReference = "BOOKREF30000";
			consol3.JK_CoLoadMasterBill = "PFGBNE224673";
			consol3.JK_BookingReference = "BOOKREF30000";
			consol3.JK_MasterBillNum = "PFGBNE224673";
			consol3.JK_UniqueConsignRef = "C00005002";
			var orgAddress3 = SetupOrgWithSCAC(newFactory, "TestOrg3");
			consol3.JK_OA_CreditorAddress = orgAddress3.PK;
			var orgAddress4 = SetupOrgWithSCAC(newFactory, "TestOrg4");
			consol3.JK_OA_ShippingLineAddress = orgAddress4.PK;
			SetupContainer(consol3);

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
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
	<BookingConfirmationReference>BOOKREF30000</BookingConfirmationReference>
	<WayBillNumber>PFGBNE224673</WayBillNumber>
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
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'TestOrg1' by code, address 'Test Street' (only address).
Updated Consol C00005002 (Master Bill='PFGBNE224673') from UniversalShipment.
Successfully saved Consol C00005002 (Master Bill='PFGBNE224673') with 1 x ForwardingContainer.
".Trim(), manager.Logger.ToString());
			});
		}

		#endregion

		#region VGM Import for Co-Load Consol with CoLoadBookingConfirmationReference/CoLoadMasterBillNumber

		public void TestVGMImport_CoLoadConsol_CoLoadMBL_CoLoadBookingRef()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol1.JK_CoLoadMasterBill = "PFGBNE224673";
			consol1.JK_UniqueConsignRef = "C00005000";

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol2.JK_CoLoadBookingReference = "BOOKREF30000";
			consol2.JK_UniqueConsignRef = "C00005001";

			var consol3 = newFactory.New<ForwardingConsol>();
			consol3.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol3.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol3.JK_CoLoadBookingReference = "BOOKREF30000";
			consol3.JK_CoLoadMasterBill = "PFGBNE224673";
			consol3.JK_UniqueConsignRef = "C00005002";
			SetupContainer(consol3);

			var consol4 = newFactory.New<ForwardingConsol>();
			consol4.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol4.JK_BookingReference = "BOOKREF30001";
			consol4.JK_CoLoadMasterBill = "PFGBNE224674";
			consol4.JK_UniqueConsignRef = "C00005003";

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
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
	<CoLoadBookingConfirmationReference>BOOKREF30000</CoLoadBookingConfirmationReference>
	<CoLoadMasterBillNumber>PFGBNE224673</CoLoadMasterBillNumber>
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
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'TESTORG9' by code, address 'Test Address1' (only address).
Updated Consol C00005002 from UniversalShipment.
Successfully saved Consol C00005002 with 1 x ForwardingContainer.
".Trim(), manager.Logger.ToString());
			});
		}

		public void TestVGMImport_CoLoadConsol_CoLoadBookingRef()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol1.JK_CoLoadMasterBill = "PFGBNE224673";
			consol1.JK_UniqueConsignRef = "C00005000";

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol2.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol2.JK_CoLoadBookingReference = "BOOKREF30000";
			consol2.JK_UniqueConsignRef = "C00005001";
			SetupContainer(consol2);

			var consol3 = newFactory.New<ForwardingConsol>();
			consol3.JK_AgentType = Core.Constants.AgentType.Agent;
			consol3.JK_CoLoadBookingReference = "BOOKREF30000";
			consol3.JK_UniqueConsignRef = "C00005002";

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
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
	<CoLoadBookingConfirmationReference>BOOKREF30000</CoLoadBookingConfirmationReference>
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
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'TESTORG9' by code, address 'Test Address1' (only address).
Updated Consol C00005001 from UniversalShipment.
Successfully saved Consol C00005001 with 1 x ForwardingContainer.
".Trim(), manager.Logger.ToString());
			});
		}

		public void TestVGMImport_CoLoadConsol_CoLoadMBL()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol1.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			consol1.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol1.JK_CoLoadMasterBill = "PFGBNE224673";
			consol1.JK_UniqueConsignRef = "C00005000";
			SetupContainer(consol1);

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol2.JK_CoLoadBookingReference = "BOOKREF30000";
			consol2.JK_UniqueConsignRef = "C00005001";

			var consol3 = newFactory.New<ForwardingConsol>();
			consol3.JK_AgentType = Core.Constants.AgentType.Agent;
			consol3.JK_CoLoadMasterBill = "PFGBNE224673";
			consol3.JK_UniqueConsignRef = "C00005002";

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
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
	<CoLoadMasterBillNumber>PFGBNE224673</CoLoadMasterBillNumber>
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
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'TESTORG9' by code, address 'Test Address1' (only address).
Updated Consol C00005000 from UniversalShipment.
Successfully saved Consol C00005000 with 1 x ForwardingContainer.
".Trim(), manager.Logger.ToString());
			});
		}

		public void TestVGMImport_CoLoadConsol_CoLoadSCAC_CoLoadMBL_CoLoadBookingRef()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol1.JK_CoLoadMasterBill = "PFGBNE224673";
			consol1.JK_UniqueConsignRef = "C00005000";

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol2.JK_CoLoadBookingReference = "BOOKREF30001";
			consol2.JK_UniqueConsignRef = "C00005001";

			var consol3 = newFactory.New<ForwardingConsol>();
			consol3.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol3.JK_CoLoadBookingReference = "BOOKREF30001";
			consol3.JK_CoLoadMasterBill = "PFGBNE224673";
			consol3.JK_UniqueConsignRef = "C00005002";

			var consol4 = newFactory.New<ForwardingConsol>();
			consol4.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol4.JK_CoLoadMasterBill = "PFGBNE224673";
			consol4.JK_CoLoadBookingReference = "BOOKREF30000";
			consol4.JK_UniqueConsignRef = "C00005003";

			var consol5 = newFactory.New<ForwardingConsol>();
			consol5.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol5.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol5.JK_CoLoadBookingReference = "BOOKREF30000";
			consol5.JK_CoLoadMasterBill = "PFGBNE224673";
			consol5.JK_UniqueConsignRef = "C00005004";
			var orgAddress = SetupOrgWithSCAC(newFactory);
			consol5.JK_OA_CreditorAddress = orgAddress.PK;
			SetupContainer(consol5);

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
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
	<CoLoadBookingConfirmationReference>BOOKREF30000</CoLoadBookingConfirmationReference>
	<CoLoadMasterBillNumber>PFGBNE224673</CoLoadMasterBillNumber>
	<OrganizationAddressCollection>
		<OrganizationAddress>
			<AddressType>CoLoadWith</AddressType>
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
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'TESTORG9' by code, address 'Test Address1' (only address).
Updated Consol C00005004 from UniversalShipment.
Successfully saved Consol C00005004 with 1 x ForwardingContainer.
".Trim(), manager.Logger.ToString());
			});
		}

		public void TestVGMImport_CoLoadConsol_CoLoadSCAC_CoLoadBookingRef()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol1.JK_CoLoadMasterBill = "PFGBNE224673";
			consol1.JK_UniqueConsignRef = "C00005000";

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol2.JK_CoLoadBookingReference = "BOOKREF30000";
			consol2.JK_UniqueConsignRef = "C00005001";

			var consol3 = newFactory.New<ForwardingConsol>();
			consol3.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol3.JK_CoLoadBookingReference = "BOOKREF30000";
			consol3.JK_CoLoadMasterBill = "PFGBNE224673";
			consol3.JK_UniqueConsignRef = "C00005002";
			var orgAddress1 = SetupOrgWithSCAC(newFactory, "TestOrg1", "ABCD");
			consol3.JK_OA_CreditorAddress = orgAddress1.PK;

			var consol4 = newFactory.New<ForwardingConsol>();
			consol4.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol4.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol4.JK_CoLoadBookingReference = "BOOKREF30000";
			consol4.JK_CoLoadMasterBill = "PFGBNE224673";
			consol4.JK_UniqueConsignRef = "C00005003";
			var orgAddress2 = SetupOrgWithSCAC(newFactory, "TestOrg2");
			consol4.JK_OA_CreditorAddress = orgAddress2.PK;
			SetupContainer(consol4);

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
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
	<CoLoadBookingConfirmationReference>BOOKREF30000</CoLoadBookingConfirmationReference>
	<OrganizationAddressCollection>
		<OrganizationAddress>
			<AddressType>CoLoadWith</AddressType>
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
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'TESTORG9' by code, address 'Test Address1' (only address).
Updated Consol C00005003 from UniversalShipment.
Successfully saved Consol C00005003 with 1 x ForwardingContainer.
".Trim(), manager.Logger.ToString());
			});
		}

		public void TestVGMImport_CoLoadConsol_CoLoadSCAC_CoLoadMBL()
		{
			var newFactory = new BusinessObjectFactory();

			var consol1 = newFactory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol1.JK_CoLoadMasterBill = "PFGBNE224673";
			consol1.JK_UniqueConsignRef = "C00005000";

			var consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol2.JK_CoLoadBookingReference = "BOOKREF30000";
			consol2.JK_UniqueConsignRef = "C00005001";

			var consol3 = newFactory.New<ForwardingConsol>();
			consol3.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol3.JK_CoLoadBookingReference = "BOOKREF30000";
			consol3.JK_CoLoadMasterBill = "PFGBNE224673";
			consol3.JK_UniqueConsignRef = "C00005002";

			var consol4 = newFactory.New<ForwardingConsol>();
			consol4.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol4.JK_CoLoadMasterBill = "PFGBNE224673";
			consol4.JK_CoLoadBookingReference = "BOOKREF30000";
			consol4.JK_UniqueConsignRef = "C00005003";
			var orgAddress1 = SetupOrgWithSCAC(newFactory, "TestOrg1", "ABCD");
			consol4.JK_OA_CreditorAddress = orgAddress1.PK;

			var consol5 = newFactory.New<ForwardingConsol>();
			consol5.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol5.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol5.JK_CoLoadBookingReference = "BOOKREF30000";
			consol5.JK_CoLoadMasterBill = "PFGBNE224673";
			consol5.JK_UniqueConsignRef = "C00005004";
			var orgAddress2 = SetupOrgWithSCAC(newFactory, "TestOrg2");
			consol5.JK_OA_CreditorAddress = orgAddress2.PK;
			SetupContainer(consol5);

			var consol6 = newFactory.New<ForwardingConsol>();
			consol6.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol6.JK_CoLoadMasterBill = "PFGBNE224673";
			consol6.JK_UniqueConsignRef = "C00005005";
			var orgAddress3 = SetupOrgWithSCAC(newFactory, "TestOrg3", regType: OrgCusCode.CodeTypes.CargoWiseOneCarrierCode);
			consol6.JK_OA_CreditorAddress = orgAddress3.PK;

			var consol7 = newFactory.New<ForwardingConsol>();
			consol7.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol7.JK_CoLoadMasterBill = "PFGBNE224673";
			consol7.JK_UniqueConsignRef = "C00005006";
			var orgAddress4 = SetupOrgWithSCAC(newFactory, "TestOrg4", country: Constants.CountryCodes.Canada);
			consol7.JK_OA_CreditorAddress = orgAddress4.PK;

			newFactory.Save();

			#region XML Message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
	<DataContext>
		<DataSourceCollection>
			<DataSource>
				<Type>ForwardingConsol</Type>
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
	<CoLoadMasterBillNumber>PFGBNE224673</CoLoadMasterBillNumber>
	<OrganizationAddressCollection>
		<OrganizationAddress>
			<AddressType>CoLoadWith</AddressType>
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
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
Successfully loaded matching ForwardingContainer.
Populating ForwardingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'TESTORG9' by code, address 'Test Address1' (only address).
Updated Consol C00005004 from UniversalShipment.
Successfully saved Consol C00005004 with 1 x ForwardingContainer.
".Trim(), manager.Logger.ToString());
			});
		}

		#endregion

		#region Transform

		public void TestTransportEventTransform_DepartureToStatusUpdated()
		{
			#region XML Message

			const string eventXmlMessage = @"
<UniversalEvent>
  <Event>
    <EventTime>2018-11-26T10:54:50.227</EventTime>
    <EventType>DEP</EventType>
	<EventReference>Dummy Description|LOC=AUSYD|FAC=CTO</EventReference>
    <IsEstimate>true</IsEstimate>

    <ContextCollection>
	  <Context>
		  <Type>MBOLNumber</Type>
		  <Value>02012345675</Value>
	  </Context>
	  <Context>
		  <Type>VoyageNumber</Type>
		  <Value>11224</Value>
	  </Context>
	  <Context>
		  <Type>VesselName</Type>
		  <Value>MAGIC SCHOOL BUS</Value>
	  </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";

			#endregion

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "02012345675";

			var firstLeg = consol.Transports[0];
			firstLeg.JW_LegOrder = 1;
			firstLeg.JW_VoyageFlight = "11224";
			firstLeg.JW_RL_NKLoadPort = "USGLE";
			firstLeg.JW_RL_NKDiscPort = "AUSYD";
			firstLeg.JW_TransportMode = Constants.TransportModes.Sea;

			var secondLeg = consol.Transports.AddNew();
			secondLeg.JW_LegOrder = 2;
			secondLeg.JW_VoyageFlight = "11224";
			secondLeg.JW_Vessel = "MAGIC SCHOOL BUS";
			secondLeg.JW_RL_NKLoadPort = "AUSYD";
			secondLeg.JW_RL_NKDiscPort = "HKHKG";
			secondLeg.JW_TransportMode = Constants.TransportModes.Sea;
			secondLeg.JW_ETD = ZDateTime.Today.AddDays(-2);
			secondLeg.JW_ATD = ZDateTime.Today;

			Factory.SaveForTesting();

			var departureQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DepartureCode);
			var statusUpdatedQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusUpdatedCode);

			var departureQueryLogs = secondLeg.Logs.Find(departureQuery);
			AssertEquals($"Two Departure events should be added to {secondLeg.HumanReadableName}", 2, departureQueryLogs.Length);
			var statusUpdatedLogs = secondLeg.Logs.Find(statusUpdatedQuery);
			AssertEquals($"No StatusUpdated event was added to {secondLeg.HumanReadableName}", 0, statusUpdatedLogs.Length);

			var message = GetQueuedUniversalEventMessage(eventXmlMessage);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("Prerequisite: message processed", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			departureQueryLogs = secondLeg.Logs.Find(departureQuery);
			AssertEquals($"No new Departure event was added to {secondLeg.HumanReadableName}", 2, departureQueryLogs.Length);
			statusUpdatedLogs = secondLeg.Logs.Find(statusUpdatedQuery);
			AssertEquals($"StatusUpdated event should be added to {secondLeg.HumanReadableName}", 1, statusUpdatedLogs.Length);

			var reference = statusUpdatedLogs.First().SL_Reference;
			var parameters = StmALog.GetParametersFromReference(reference);
			Assert("TYP parameter", parameters.Contains(new KeyValuePair<string, string>("TYP", "Change of ETD rejected")));
			Assert("RES parameter", parameters.Contains(new KeyValuePair<string, string>("RES", "ETD received after ATD")));
			Assert("LOC parameter", parameters.Contains(new KeyValuePair<string, string>("LOC", "AUSYD")));
			AssertEquals("FreeText", "Dummy Description", StmALog.GetFreeTextFromReference(reference));
		}

		public void TestTransportEventTransform_ArrivalToStatusUpdated()
		{
			#region XML Message

			const string eventXmlMessage = @"
<UniversalEvent>
  <Event>
    <EventTime>2018-11-26T10:54:50.227</EventTime>
    <EventType>ARV</EventType>
	<EventReference>Dummy Description|LOC=HKHKG|FAC=CTO</EventReference>
    <IsEstimate>true</IsEstimate>

    <ContextCollection>
	  <Context>
		  <Type>MBOLNumber</Type>
		  <Value>02012345675</Value>
	  </Context>
	  <Context>
		  <Type>VoyageNumber</Type>
		  <Value>11224</Value>
	  </Context>
	  <Context>
		  <Type>VesselName</Type>
		  <Value>MAGIC SCHOOL BUS</Value>
	  </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";

			#endregion

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "02012345675";

			var firstLeg = consol.Transports[0];
			firstLeg.JW_LegOrder = 1;
			firstLeg.JW_VoyageFlight = "11224";
			firstLeg.JW_RL_NKLoadPort = "USGLE";
			firstLeg.JW_RL_NKDiscPort = "AUSYD";
			firstLeg.JW_TransportMode = Constants.TransportModes.Sea;

			var secondLeg = consol.Transports.AddNew();
			secondLeg.JW_LegOrder = 2;
			secondLeg.JW_VoyageFlight = "11224";
			secondLeg.JW_Vessel = "MAGIC SCHOOL BUS";
			secondLeg.JW_RL_NKLoadPort = "AUSYD";
			secondLeg.JW_RL_NKDiscPort = "HKHKG";
			secondLeg.JW_TransportMode = Constants.TransportModes.Sea;
			secondLeg.JW_ETA = ZDateTime.Today.AddDays(-1);
			secondLeg.JW_ATA = ZDateTime.Today;

			Factory.SaveForTesting();

			var arrivalQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ArrivalCode);
			var statusUpdatedQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusUpdatedCode);

			var arrivalQueryLogs = secondLeg.Logs.Find(arrivalQuery);
			AssertEquals($"Two Arrival events should be added to {secondLeg.HumanReadableName}", 2, arrivalQueryLogs.Length);
			var statusUpdatedLogs = secondLeg.Logs.Find(statusUpdatedQuery);
			AssertEquals($"No StatusUpdated event was added to {secondLeg.HumanReadableName}", 0, statusUpdatedLogs.Length);

			var message = GetQueuedUniversalEventMessage(eventXmlMessage);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("Prerequisite: message processed", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			arrivalQueryLogs = secondLeg.Logs.Find(arrivalQuery);
			AssertEquals($"No new Arrival event was be added to {secondLeg.HumanReadableName}", 2, arrivalQueryLogs.Length);
			statusUpdatedLogs = secondLeg.Logs.Find(statusUpdatedQuery);
			AssertEquals($"StatusUpdated event should be added to {secondLeg.HumanReadableName}", 1, statusUpdatedLogs.Length);

			var reference = statusUpdatedLogs.First().SL_Reference;
			var parameters = StmALog.GetParametersFromReference(reference);
			Assert("TYP parameter", parameters.Contains(new KeyValuePair<string, string>("TYP", "Change of ETA rejected")));
			Assert("RES parameter", parameters.Contains(new KeyValuePair<string, string>("RES", "ETA received after ATA")));
			Assert("LOC parameter", parameters.Contains(new KeyValuePair<string, string>("LOC", "HKHKG")));
			AssertEquals("FreeText", "Dummy Description", StmALog.GetFreeTextFromReference(reference));
		}

		#region ForwardingConsolEventTransformer

		public void TestForwardingConsolEventTransformer_LogParent_Transport()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "02077777770";
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "USLAX";

			var transport = consol.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_LegOrder = 1;
			transport.JW_VoyageFlight = "LH123T";
			transport.JW_RL_NKLoadPort = "AUMEL";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_ETD = new ZDateTime(2023, 5, 20, 9, 40, 0);
			transport.JW_ETA = new ZDateTime(2023, 5, 20, 15, 0, 0);

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(BookingConfirmedEventXMLMessage);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			var bkcEvent1 = transport.Logs.MostRecentLogByEventTime(Events.BookingConfirmed);
			AssertNotNull("Transport has added BKC event", bkcEvent1);
			var expectedReference = "|DEP=Carrier|FDT=20-May-23 09:40|FRM=AUMEL|LOC=AUMEL|TO=AUSYD|TTL=46|TYP=AWB|VFL=LH123T";
			AssertEquals(expectedReference, bkcEvent1.SL_Reference);

			var bkcEvent2 = consol.Logs.MostRecentLogByEventTime(Events.BookingConfirmed);
			AssertNotNull("Consol has BKC event after Propagation", bkcEvent2);
			AssertEquals(expectedReference, bkcEvent2.SL_Reference);
		}

		public void TestForwardingConsolEventTransformer_LogParent_Consol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "02077777770";
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "USLAX";

			var transport = consol.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_LegOrder = 1;
			transport.JW_VoyageFlight = "QF30";
			transport.JW_RL_NKLoadPort = "AUMEL";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_ETD = new ZDateTime(2023, 5, 20, 9, 40, 0);
			transport.JW_ETA = new ZDateTime(2023, 5, 20, 15, 0, 0);

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(BookingConfirmedEventXMLMessage);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			var bkcEvent1 = transport.Logs.MostRecentLogByEventTime(Events.BookingConfirmed);
			AssertNull("Transport has no BKC event", bkcEvent1);

			var bkcEvent2 = consol.Logs.MostRecentLogByEventTime(Events.BookingConfirmed);
			AssertNotNull("Consol has added BKC event", bkcEvent2);
			var expectedReference = "|DEP=Carrier|FDT=20-May-23 09:40|FRM=AUMEL|LOC=AUMEL|TO=AUSYD|TTL=46|TYP=AWB|VFL=LH123T";
			AssertEquals(expectedReference, bkcEvent2.SL_Reference);
		}

		#region BookingConfirmedEventXMLMessage

		string BookingConfirmedEventXMLMessage => @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>ForwardingConsol</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2023-05-20T09:40:00</EventTime>
		<EventType>BKC</EventType>
		<EventParameters>
			<Department>Carrier</Department>
			<Location>MEL</Location>
			<Type>AWB</Type>
			<VoyageFlightNumber>LH123T</VoyageFlightNumber>
			<FlightDate>2023-05-20T09:40:00</FlightDate>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>MAWBNumber</Type>
				<Value>020-77777770</Value>
			</Context>
			<Context>
				<Type>MAWBOriginIATAAirportCode</Type>
				<Value>MEL</Value>
			</Context>
			<Context>
				<Type>MAWBDestinationIATAAirportCode</Type>
				<Value>LAX</Value>
			</Context>
			<Context>
				<Type>OriginIATAAirportCode</Type>
				<Value>MEL</Value>
			</Context>
			<Context>
				<Type>DestinationIATAAirportCode</Type>
				<Value>SYD</Value>
			</Context>
			<Context>
				<Type>MAWBNumberOfPieces</Type>
				<Value>46</Value>
			</Context>
			<Context>
				<Type>FlightNumber</Type>
				<Value>LH123T</Value>
			</Context>
			<Context>
				<Type>FlightDate</Type>
				<Value>2023-05-20T09:40:00</Value>
			</Context>
			<Context>
				<Type>NumberOfPieces</Type>
				<Value>46</Value>
			</Context>
			<Context>
				<Type>WeightOfGoods</Type>
				<Value>250.0KG</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		#endregion

		#endregion

		#endregion

		#region Logging

		string BlankDepartureEventXml => @"
<UniversalEvent>
    <Event>
        <DataContext>
            <DataTargetCollection>
                <DataTarget>
                    <Type>ForwardingConsol</Type>
                    <Key>C00001210</Key>
                </DataTarget>
            </DataTargetCollection>
        </DataContext>
        <EventType>ARV</EventType>
        <EventTime>2016-02-26T14:11:59</EventTime>
        <IsEstimate>true</IsEstimate>
        <ContextCollection>
            <Context>
                <Type>FlightNumber</Type>
                <Value>LH7609B</Value>
            </Context>
            <Context>
                <Type>DummyText</Type>
                <Value>Here is my dummy text</Value>
            </Context>
        </ContextCollection>
        <IsEstimate>True</IsEstimate>
        <EventReference>|FAC=CTO|LOC=CHBSL|VFL=LH7609B</EventReference>
    </Event>
</UniversalEvent>";

		string DepartureEventXml => @"
<UniversalEvent>
    <Event>
        <DataContext>
            <DataTargetCollection>
                <DataTarget>
                    <Type>ForwardingConsol</Type>
                    <Key>C00001210</Key>
                </DataTarget>
            </DataTargetCollection>
        </DataContext>
        <EventType>DEP</EventType>
        <EventTime>2016-02-26T14:11:59</EventTime>
        <IsEstimate>true</IsEstimate>
        <ContextCollection>
            <Context>
                <Type>FlightNumber</Type>
                <Value>LH7609B</Value>
            </Context>
            <Context>
                <Type>DummyText</Type>
                <Value>Here is my dummy text</Value>
        </Context>
        </ContextCollection>
        <IsEstimate>True</IsEstimate>
        <EventReference>|FAC=CTO|LOC=CHBSL|VFL=LH7609B</EventReference>
    </Event>
</UniversalEvent>";

		public void TestForwardingConsolEventTransformer_LogParent_TransportLogsMessage()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_UniqueConsignRef = "C00001210";
			consol.JK_RL_NKLoadPort = "CHBSL";
			consol.JK_RL_NKDischargePort = "USLAX";

			var transport = consol.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_LegOrder = 1;
			transport.JW_VoyageFlight = "LH123T";
			transport.JW_RL_NKLoadPort = "CHBSL";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_ETD = new ZDateTime(2023, 5, 20, 9, 40, 0);
			transport.JW_ETA = new ZDateTime(2023, 5, 20, 15, 0, 0);

			Factory.SaveForTesting();

			AssertNotEquals(ZDateTime.Empty, new BusinessObjectFactory().Load<Transport>(transport.PK).JW_ETD);

			// We set JW_ETD to empty here because we can't reproduce the case where it happens during UE.
			// An example is an IFC trigger to clear JW_ETD --> no user would do this, right?
			transport.JW_ETD = ZDateTime.Empty;

			var message = GetQueuedUniversalEventMessage(BlankDepartureEventXml, false);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			var lastKeyReporter = ErrorReporter.LastKeyReported;
			var lastMessageReported = ErrorReporter.LastMessageReported;
			CombineAssertions(() =>
			{
				AssertEquals("TransportTimeUnexpectedlyClearedOut_UE", lastKeyReporter);
				AssertContains("CHBSL->AUSYD:ETD is cleared:", lastMessageReported);
				AssertContains("<Value>Here is my dummy text</Value>", lastMessageReported);
			});

			ErrorReporter.Clear();
		}

		public void TestTryLogWhileClearTimeOfTransports_EventCancelled()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_UniqueConsignRef = "C00001210";
			consol.JK_RL_NKLoadPort = "CHBSL";
			consol.JK_RL_NKDischargePort = "USLAX";

			var transport = consol.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_LegOrder = 1;
			transport.JW_VoyageFlight = "LH123T";
			transport.JW_RL_NKLoadPort = "CHBSL";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_ETD = new ZDateTime(2023, 5, 20, 9, 40, 0);
			transport.JW_ETA = new ZDateTime(2023, 5, 20, 15, 0, 0);

			Factory.SaveForTesting();

			AssertNotEquals("Precondition", ZDateTime.Empty, new BusinessObjectFactory().Load<Transport>(transport.PK).JW_ETD);

			var message = GetQueuedUniversalEventMessage(DepartureEventXml, false);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			Factory.SaveForTesting();

			transport.Reload();
			AssertNotEquals("Precondition", ZDateTime.Empty, transport.JW_ETD);

			var depLogs = transport.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.Departure);
			foreach (var log in depLogs)
			{
				log.Cancel();
			}
			AssertEquals("Precondition: ETD should be cleared", ZDateTime.Empty, transport.JW_ETD);

			var lastKeyReporter = ErrorReporter.LastKeyReported;
			var lastMessageReported = ErrorReporter.LastMessageReported;
			CombineAssertions(() =>
			{
				AssertEquals("TransportTimeUnexpectedlyClearedOut_CAN", lastKeyReporter);
				AssertContains("CHBSL->AUSYD:ETD is cleared:", lastMessageReported);
				AssertContains("Event Cancelled Context: DEP", lastMessageReported);
			});

			ErrorReporter.Clear();
		}

		#endregion

		#region Airline Tracking

		[TestDate(2024, 2, 1)]
		public void TestProcessActualDepartureEvent_WhenLocationIsUnmatched_DoesNotDuplicateCID()
		{
			const string depEventXmlText = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingConsol</Type>
            </DataTarget>
            <DataTarget>
              <Type>CustomsDeclaration</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2024-05-28T10:45:00</EventTime>
        <EventType>DEP</EventType>
        <IsEstimate>false</IsEstimate>
        <EventParameters>
          <Facility>CTO</Facility>
          <Location>AKL</Location>
          <VoyageFlightNumber>SQ333</VoyageFlightNumber>
          <FlightDate>2024-05-29T20:00:00</FlightDate>
        </EventParameters>
        <ContextCollection>
          <Context>
            <Type>MAWBNumber</Type>
            <Value>618-22222222</Value>
          </Context>
          <Context>
            <Type>MAWBOriginIATAAirportCode</Type>
            <Value>SYD</Value>
          </Context>
          <Context>
            <Type>MAWBDestinationIATAAirportCode</Type>
            <Value>SIN</Value>
          </Context>
          <Context>
            <Type>OriginIATAAirportCode</Type>
            <Value>SYD</Value>
          </Context>
          <Context>
            <Type>DestinationIATAAirportCode</Type>
            <Value>SIN</Value>
          </Context>
          <Context>
            <Type>MAWBNumberOfPieces</Type>
            <Value>1</Value>
          </Context>
          <Context>
            <Type>FlightNumber</Type>
            <Value>SQ333</Value>
          </Context>
          <Context>
            <Type>FlightDate</Type>
            <Value>2024-05-29T20:00:00</Value>
          </Context>
          <Context>
            <Type>NumberOfPieces</Type>
            <Value>1</Value>
          </Context>
          <Context>
            <Type>WeightOfGoods</Type>
            <Value>1.0KG</Value>
          </Context>
        </ContextCollection>
      </Event>
    </UniversalEvent>
";
			TestProcessActualDateEvent_WhenLocationIsUnmatched_DoesNotDuplicateCID(depEventXmlText, Events.Departure);
		}

		[TestDate(2024, 2, 1)]
		public void TestProcessActualArrivalEvent_WhenLocationIsUnmatched_DoesNotDuplicateCID()
		{
			var arvEventXmlText = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingConsol</Type>
            </DataTarget>
            <DataTarget>
              <Type>CustomsDeclaration</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2024-05-28T09:05:00</EventTime>
        <EventType>ARV</EventType>
        <IsEstimate>false</IsEstimate>
        <EventParameters>
          <Facility>CTO</Facility>
          <Location>AKL</Location>
          <VoyageFlightNumber>SQ333</VoyageFlightNumber>
          <FlightDate>2024-05-29</FlightDate>
        </EventParameters>
        <ContextCollection>
          <Context>
            <Type>MAWBNumber</Type>
            <Value>618-22222222</Value>
          </Context>
          <Context>
            <Type>MAWBOriginIATAAirportCode</Type>
            <Value>SYD</Value>
          </Context>
          <Context>
            <Type>MAWBDestinationIATAAirportCode</Type>
            <Value>SIN</Value>
          </Context>
          <Context>
            <Type>OriginIATAAirportCode</Type>
            <Value>SYD</Value>
          </Context>
          <Context>
            <Type>DestinationIATAAirportCode</Type>
            <Value>SIN</Value>
          </Context>
          <Context>
            <Type>MAWBNumberOfPieces</Type>
            <Value>1</Value>
          </Context>
          <Context>
            <Type>FlightNumber</Type>
            <Value>SQ333</Value>
          </Context>
          <Context>
            <Type>FlightDate</Type>
            <Value>2024-05-29</Value>
          </Context>
          <Context>
            <Type>NumberOfPieces</Type>
            <Value>1</Value>
          </Context>
          <Context>
            <Type>WeightOfGoods</Type>
            <Value>1.0KG</Value>
          </Context>
        </ContextCollection>
      </Event>
    </UniversalEvent>
";
			TestProcessActualDateEvent_WhenLocationIsUnmatched_DoesNotDuplicateCID(arvEventXmlText, Events.Arrival);
		}

		[TestDate(2024, 2, 1)]
		public void TestProcessActualDateEvent_WhenLocationIsUnmatched_DoesNotDuplicateCID(string dateEventXmlText, ZArchitecture.Business.Event dateEventType)
		{
			using (FreightDataRegistry.Instance.AutomaticUpdatingofPlannedLegs_Air.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_MasterBillNum = "618-22222222";

				var transport = consol.Transports.AddNew();
				transport.JW_TransportMode = Constants.TransportModes.Air;
				transport.JW_RL_NKLoadPort = "AUSYD";
				transport.JW_RL_NKDiscPort = "SGSIN";
				transport.JW_VoyageFlight = "SQ333";
				transport.JW_ETD = new ZDateTime(2024, 5, 29, 23, 0, 0);
				transport.JW_ETA = new ZDateTime(2023, 5, 30, 23, 0, 0);

				var eventDeserializer = new XmlEventDeserializer();
				var xmlEvent = eventDeserializer.Parse(dateEventXmlText) as UniversalEvent;
				AssertNotEquals("Precondition: Location should be different to the Load Port", transport.JW_RL_NKLoadPort.Right(3), xmlEvent.EventParameters.Location);

				var bkcReference = "|ETA=2024-05-30 05:00|FDT=29-May-24 20:00|FRM=AUSYD|LOC=AUSYD|TO=SGSIN|TTL=1|TYP=AWB|VFL=SQ333";
				var log = transport.Logs.AddNew();
				using (log.LockForUpdatingKeyFieldsForTesting())
				{
					log.SL_SE_NKEvent = Events.BookingConfirmedCode;
					log.SL_Reference = bkcReference;
					log.SL_EventTime = ZDateTime.Now;
					log.SL_IsEstimate = false;
				}

				Factory.SaveForTesting();

				var bkcEvent = consol.Logs.MostRecentLogByPostedTime(Events.BookingConfirmed);
				AssertNotNull("Precondition", bkcEvent);
				Factory.SaveForTesting();

				var dateTimeBeforeSave = TestDateAttribute.Date.AddHours(1);
				TestDateAttribute.Date = dateTimeBeforeSave;

				var contextManager = consol.GetUniversalDataContextManager() as IEventDataContextManager;
				var parentFinder = new ConsolEventParentFinder<ForwardingConsol, ForwardingShipment, ForwardingContainer>(Factory.BOFactory, contextManager, new DummyLogger(), new UniversalForwardingHelper());
				var logParents = parentFinder.GetLogParentsForEvent(xmlEvent);
				if (dateEventType == Events.Departure)
				{
					AssertEquals("Precondition", typeof(Transport), logParents[0].GetType());
				}
				else if (dateEventType == Events.Arrival)
				{
					AssertEquals("Precondition", typeof(ForwardingConsol), logParents[0].GetType());
				}

				var message = GetQueuedUniversalEventMessage(dateEventXmlText);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				var dateEventLogs = consol.Logs.GetAllLogs().Where(x => x.SL_SE_NKEvent == dateEventType.Code && !x.SL_IsEstimate);
				var cidEventLogs = consol.Logs.GetAllLogs().Where(x => x.SL_SE_NKEvent == Events.ChangeOfIdentifierCode && x.Parameters["EVT"] == dateEventType.Code);
				AssertEquals(1, dateEventLogs.Count());
				AssertEquals(1, cidEventLogs.Count());

				var dateTimeAfterSave = TestDateAttribute.Date.AddHours(2);
				TestDateAttribute.Date = dateTimeAfterSave;
				Factory.SaveForTesting();

				dateEventLogs = consol.Logs.GetAllLogs().Where(x => x.SL_SE_NKEvent == dateEventType.Code && !x.SL_IsEstimate);
				cidEventLogs = consol.Logs.GetAllLogs().Where(x => x.SL_SE_NKEvent == Events.ChangeOfIdentifierCode && x.Parameters["EVT"] == dateEventType.Code);
				AssertEquals("There should no additional 1og created", 1, dateEventLogs.Count());
				AssertEquals("There should no additional 1og created", 1, cidEventLogs.Count());
			}
		}

		#endregion

		public void TestConsolCompletionEventSustained_WhenPartialEventPropagatedFromShipment()
		{
			AssertPartialEventReferenceTypes(Events.FreightLoadedCode, "S00010001", "C00001001", "MASTERB1");
			AssertPartialEventReferenceTypes(Events.FreightUnloadedCode, "S00010002", "C00001002", "MASTERB2");
			AssertPartialEventReferenceTypes(Events.ReceivedCode, "S00010003", "C00001003", "MASTERB3");
		}

		void AssertPartialEventReferenceTypes(string eventCode, string shipmentReference, string consolReference, string masterBillNum)
		{
			var shipment = Factory.BOFactory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_UniqueConsignRef = shipmentReference;

			var declaration = Factory.BOFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_GB = Env.CurrentBranch.PK;
			declaration.JE_JS = shipment.PK;
			declaration.JE_MasterBill = masterBillNum;
			declaration.JE_DeclarationReference = shipmentReference;
			Factory.SaveForTesting();

			var consol = Factory.BOFactory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_MasterBillNum = masterBillNum;
			consol.JK_UniqueConsignRef = consolReference;
			consol.Shipments.Add(shipment);
			Factory.SaveForTesting();

			string universalEvent1 = string.Format(@"
<UniversalEvent>
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingConsol</Type>
              <Key>{0}</Key>
            </DataTarget>
            <DataTarget>
              <Type>CustomsDeclaration</Type>
              <Key>{1}</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2024-04-10T03:52:00</EventTime>
        <EventType>{2}</EventType>
        <EventParameters>
          <Facility>CTO</Facility>
          <Location>LHR</Location>
          <VoyageFlightNumber>TG916</VoyageFlightNumber>
          <FlightDate>2024-04-09</FlightDate>
          <Partial>2</Partial>
          <Total>3</Total>
        </EventParameters>
        <ContextCollection>
          <Context>
            <Type>MAWBNumber</Type>
            <Value>{3}</Value>
          </Context>
          <Context>
            <Type>MAWBNumberOfPieces</Type>
            <Value>3</Value>
          </Context>
          <Context>
            <Type>FlightNumber</Type>
            <Value>TG916</Value>
          </Context>
          <Context>
            <Type>NumberOfPieces</Type>
            <Value>2</Value>
          </Context>
          <Context>
            <Type>IsPartial</Type>
            <Value>Y</Value>
          </Context>
          <Context>
            <Type>WeightOfGoods</Type>
            <Value>90.00000KG</Value>
          </Context>
        </ContextCollection>
      </Event>
    </UniversalEvent>
", consolReference, shipmentReference, eventCode, masterBillNum);

			string universalEvent2 = string.Format(@"
<UniversalEvent>
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingConsol</Type>
              <Key>{0}</Key>
            </DataTarget>
            <DataTarget>
              <Type>CustomsDeclaration</Type>
              <Key>{1}</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2024-04-10T03:52:00</EventTime>
        <EventType>{2}</EventType>
        <EventParameters>
          <Facility>CTO</Facility>
          <Location>LHR</Location>
          <VoyageFlightNumber>TG916</VoyageFlightNumber>
          <FlightDate>2024-04-09</FlightDate>
          <Partial>1</Partial>
          <Total>3</Total>
        </EventParameters>
        <ContextCollection>
          <Context>
            <Type>MAWBNumber</Type>
            <Value>{3}</Value>
          </Context>
          <Context>
            <Type>MAWBNumberOfPieces</Type>
            <Value>3</Value>
          </Context>
          <Context>
            <Type>FlightNumber</Type>
            <Value>TG916</Value>
          </Context>
          <Context>
            <Type>NumberOfPieces</Type>
            <Value>1</Value>
          </Context>
          <Context>
            <Type>IsPartial</Type>
            <Value>Y</Value>
          </Context>
          <Context>
            <Type>WeightOfGoods</Type>
            <Value>90.00000KG</Value>
          </Context>
        </ContextCollection>
      </Event>
    </UniversalEvent>
", consolReference, shipmentReference, eventCode, masterBillNum);
			var localMessage = GetQueuedUniversalEventMessage(universalEvent1);
			localMessage.EM_GB = Env.CurrentBranch.PK;

			var localMessage2 = GetQueuedUniversalEventMessage(universalEvent2);
			localMessage2.EM_GB = Env.CurrentBranch.PK;

			Factory.SaveForTesting();

			var serviceTaskLog = new ServiceTaskLogForTesting();
			new UniversalMessageProcessingManager(serviceTaskLog).Process(localMessage);
			new UniversalMessageProcessingManager(serviceTaskLog).Process(localMessage2);

			Factory.SaveForTesting();

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, localMessage.EM_Status);

				var consolLogs = consol.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, eventCode)).OrderBy(_ => _.SL_Reference).ToList();
				var shipmentLogs = shipment.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, eventCode)).ToList();
				var shipmentLogsPropagatedToConsol = consolLogs.Where(_ => _.SL_Reference.Contains("Propagated: All Shipments")).ToList();

				AssertEquals("Events found in Consol", 5, consolLogs.Count);
				AssertEquals("Events found in Shipment", 2, shipmentLogs.Count);
				AssertEquals("Events found in Consol propagated from Shipment", 2, shipmentLogsPropagatedToConsol.Count);

				AssertEquals("Type", EventReferenceParameterTypes.Partial, consolLogs[0].Parameters[Params.Type]);
				AssertEquals("Type", EventReferenceParameterTypes.Partial, consolLogs[1].Parameters[Params.Type]);
				AssertEquals("Type", EventReferenceParameterTypes.Complete, consolLogs[2].Parameters[Params.Type]);
			});
		}

		#region BillNote

		[TestDate(2024, 10, 31, 9, 32, 21)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestConsolPopulateOriginalBillNotes_Created()
		{
			TestDateAttribute.UseUNLOCO = true;
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001241";

			var uOriginalBillPublishedEvent = GetQueuedUniversalEventMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalEventBLU_OriginalBillPublished.xml")), createInterchange: true);
			var uShipment = GetQueuedUniversalShipmentMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalShipmentBLU.xml")));
			uShipment.EM_EI = uOriginalBillPublishedEvent.EM_EI;
			var uSurrenderedEvent = GetQueuedUniversalEventMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalEventBLU_Surrendered.xml")), createInterchange: true);
			var uInvalidTypeEvent = GetQueuedUniversalEventMessage(ResourceRetriever.GetString(GetResourcePathFor("UniversalEventBLU_InvalidType.xml")), createInterchange: true);

			Factory.SaveForTesting();

			using (FreightDataRegistry.Instance.EnableBoleroEBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration() { EnableEBLIntegration = true, GalileoEndPointUrl = "http://test.test", GalileoAudience = Guid.NewGuid().ToString(), GalileoTestEndPointUrl = "http://test.test", GalileoTestAudience = Guid.NewGuid().ToString() }))
			{
				var query = new ZQuery(StmNoteSchema.ST_ParentID, consol.PK);
				query.AddToFilter(StmNoteSchema.ST_Table, consol.TableName);
				query.AddToFilter(StmNoteSchema.ST_Description, PredefinedNoteTypes.Instance.OriginalBillNotes.ToString());
				query.AddToFilter(StmNoteSchema.ST_NoteType, nameof(CargoWise.Definitions.StmNoteVisibility.INT));

				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(uOriginalBillPublishedEvent);
				var originalBillNote = Factory.LoadTop1<StmNote>(query);
				AssertNull("Should be null.", originalBillNote);

				uShipment.EM_EI = ZGuid.NewZGuid();
				manager.Process(uOriginalBillPublishedEvent);
				originalBillNote = Factory.LoadTop1<StmNote>(query);
				AssertEquals(@"31-Oct-24 09:32:21 +00:00 CMATESTBOL0004 Original Bill Received [Reference:CMATESTBOL0002]", originalBillNote.ST_NoteText);

				manager.Process(uSurrenderedEvent);
				originalBillNote = Factory.LoadTop1<StmNote>(query);
				AssertEquals(@"31-Oct-24 09:32:21 +00:00 CMATESTBOL0004 Surrendered [Reference:CMATESTBOL0002]

31-Oct-24 09:32:21 +00:00 CMATESTBOL0004 Original Bill Received [Reference:CMATESTBOL0002]", originalBillNote.ST_NoteText);

				manager.Process(uInvalidTypeEvent);
				originalBillNote = Factory.LoadTop1<StmNote>(query);
				AssertEquals(@"31-Oct-24 09:32:21 +00:00 CMATESTBOL0004 Surrendered [Reference:CMATESTBOL0002]

31-Oct-24 09:32:21 +00:00 CMATESTBOL0004 Original Bill Received [Reference:CMATESTBOL0002]", originalBillNote.ST_NoteText);
			}
		}

		#endregion BillNote
	}
}
