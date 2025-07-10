using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using Context = Enterprise.UniversalDataBuss.DataObjects.Universal.Context;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportBookings.DataTransfer.Universal.Testing
{
	[TestedType(typeof(DtbBookingDataContextManager))]
	sealed class DtbBookingDataContextManagerTest : ShipmentDataContextManagerTestCase<DtbBookingDataContextManager, DtbBooking>
	{
		[TestDate(2010, 1, 1)]
		public void TestFieldsCanBeUpdatedFromUniversalEvent()
		{
			var testDataHelper = new TransportBookingTestHelper(Factory.BOFactory);

			var booking = testDataHelper.CreateBooking();
			var pickUpInstruction = testDataHelper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);

			Factory.SaveForTesting();

			AssertEquals("Precondition - no confirmations exist on pickUpInstruction", 0, pickUpInstruction.Confirmations.Count);

			string eventXmlText = DtbBookingConfirmationEventParentFinderTest.GetTestEventXmlText(booking.KM_JobID, 1, "Jack The Ripper", "SLICE101");
			var message = GetQueuedUniversalEventMessage(eventXmlText);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("Precondition: If no confirmation that much instruction type exist, then new one should be created.", 1, pickUpInstruction.Confirmations.Count);
				var confirmation = pickUpInstruction.Confirmations[0];

				var logs = confirmation.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "CCD"));
				AssertEquals("logs.Length", 1, logs.Length);
				var log = logs[0];
				AssertEquals("log.SL_EventTime", new ZDateTime(2010, 7, 10, 18, 0, 0), log.SL_EventTime);

				AssertEquals("confirmation.KK_ReceivedBy", "Jack The Ripper", confirmation.KK_ReceivedBy);
				AssertEquals("confirmation.KK_ReferenceNum", "SLICE101", confirmation.KK_ReferenceNum);
			});
		}

		public void TestContextInformationIsAllThere()
		{
			var booking = GetNewBusinessObjectForTesting();
			booking.KM_TransportReference = "BB123987123";

			var manager = booking.GetUniversalDataContextManager() as IEventDataContextManager;
			AssertMultilineASCIIEquals("manager.EventContextValues", @"
TransportBookingJobID - TB00002345
TransportReference - BB123987123
			".Trim(), manager.EventContextValues.ToStringContents(e => e.Key + " - " + e.Value));
		}

		public void TestOnUniversalEventAdded_ServiceCommenced()
		{
			var uEvent = new UniversalEvent() { EventType = AutoEvents.ServiceCommencedCode };
			uEvent.EventTime = ZDateTimeOffset.Now;
			uEvent.EventReference = "12345|FAC=Terminal|LOC=Alexandria";

			var booking = Factory.New<DtbBooking>();
			AssertEquals("Precondition: status should be Available", TransportStatuses.Codes.Available, booking.KM_Status);
			var manager = booking.GetUniversalDataContextManager() as IEventDataContextManager;
			var serviceLogger = new ServiceTaskLogForTesting();
			var messageLogger = new XmlSessionTracker(serviceLogger);
			manager.OnUniversalEventAdded(messageLogger, uEvent);
			AssertEquals("12345", booking.KM_TransportReference);
			AssertEquals("Should set status to service commenced", TransportStatuses.Codes.ServiceCommenced, booking.KM_Status);

			var anotherUEvent = new UniversalEvent() { EventType = AutoEvents.ServiceCommencedCode };
			anotherUEvent.EventTime = ZDateTimeOffset.Now;
			anotherUEvent.EventReference = "NewID|FAC=Terminal|LOC=Alexandria";
			manager.OnUniversalEventAdded(messageLogger, anotherUEvent);
			AssertEquals("Take new reference", "NewID", booking.KM_TransportReference);

			var largeUEvent = new UniversalEvent() { EventType = AutoEvents.ServiceCommencedCode };
			largeUEvent.EventTime = ZDateTimeOffset.Now;
			largeUEvent.EventReference = "ThisIsAReallyLongReferenceThatWontFit|FAC=Terminal|LOC=Alexandria";
			manager.OnUniversalEventAdded(messageLogger, largeUEvent);
			AssertEquals("Truncate reference", "ThisIsAReallyLongReferenceThatWontF", booking.KM_TransportReference);
		}

		public void TestOnUniversalEventAdded_ServiceCommenced_EndToEnd()
		{
			var booking = Helper.CreateBooking();
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var confirmation = Helper.CreateConfirmation(dlvInstruction, ConfirmationTypes.Codes.ConNoteNo);
			AssertEquals("Precondition: status should be Available", TransportStatuses.Codes.Available, booking.KM_Status);
			Factory.SaveForTesting();

			var uEvent = new UniversalEvent();
			uEvent.EventType = Events.ServiceCommencedCode;
			uEvent.EventTime = ZDateTimeOffset.Now;
			uEvent.EventReference = "12345|FAC=CTO|LOC=Alexandria";
			uEvent.DataContext = DataContextFactory.New();
			uEvent.DataContext.AddDataTarget(DataContextType.TransportBookingConfirmation, null);
			uEvent.ContextCollection = new List<Context>
			{
				new Context() { Type = nameof(UniversalEvent.ContextTypes.TransportBookingJobID), Value = booking.KM_JobID }
			};

			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			var message = GetQueuedUniversalEventMessage(string.Empty);
			manager.Process(message, uEvent);
			AssertEquals("Should have created Service Commenced log.", true, booking.Logs.HasLogWith(l => l.SL_SE_NKEvent == Events.ServiceCommencedCode));
			AssertEquals("12345", booking.KM_TransportReference);
			AssertEquals("Should set status to service commenced", TransportStatuses.Codes.ServiceCommenced, booking.KM_Status);

			picInstruction.KN_Status = TransportStatuses.Codes.PickedUp;
			booking.UpdateStatus();
			AssertEquals("Precondition: status should be picked up", TransportStatuses.Codes.PickedUp, booking.KM_Status);
			manager.Process(message, uEvent);
			AssertEquals("Status should not have been demoted to Service Commenced.", TransportStatuses.Codes.PickedUp, booking.KM_Status);
		}

		public void TestOnUniversalEventAdded_ServiceCancelled()
		{
			var uEvent = new UniversalEvent() { EventType = AutoEvents.ServiceCancelledCode };
			uEvent.EventTime = ZDateTimeOffset.Now;

			var booking = Factory.New<DtbBooking>();
			booking.KM_Status = TransportStatuses.Codes.ServiceCommenced;
			var manager = booking.GetUniversalDataContextManager() as IEventDataContextManager;
			var serviceLogger = new ServiceTaskLogForTesting();
			var messageLogger = new XmlSessionTracker(serviceLogger);
			manager.OnUniversalEventAdded(messageLogger, uEvent);
			AssertEquals("Should set status to Available", TransportStatuses.Codes.Available, booking.KM_Status);
		}

		public void TestOnUniversalEventAdded_ServiceCancelled_EndToEnd()
		{
			var booking = Helper.CreateBooking();
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var confirmation = Helper.CreateConfirmation(dlvInstruction, ConfirmationTypes.Codes.ConNoteNo);
			booking.KM_Status = TransportStatuses.Codes.ServiceCommenced;
			Factory.SaveForTesting();

			var uEvent = new UniversalEvent();
			uEvent.EventType = Events.ServiceCancelledCode;
			uEvent.EventTime = ZDateTimeOffset.Now;
			uEvent.DataContext = DataContextFactory.New();
			uEvent.DataContext.AddDataTarget(DataContextType.TransportBookingConfirmation, null);
			uEvent.ContextCollection = new List<Context>
			{
				new Context() { Type = nameof(UniversalEvent.ContextTypes.TransportBookingJobID), Value = booking.KM_JobID }
			};

			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			var message = GetQueuedUniversalEventMessage(string.Empty);
			manager.Process(message, uEvent);
			AssertEquals("Should have created Service Cancelled log.", true, booking.Logs.HasLogWith(l => l.SL_SE_NKEvent == Events.ServiceCancelledCode));
			AssertEquals("Should set status to Available", TransportStatuses.Codes.Available, booking.KM_Status);

			picInstruction.KN_Status = TransportStatuses.Codes.PickedUp;
			booking.UpdateStatus();
			AssertEquals("Precondition: status should be picked up", TransportStatuses.Codes.PickedUp, booking.KM_Status);
			manager.Process(message, uEvent);
			AssertEquals("Status should not have been demoted to Available.", TransportStatuses.Codes.PickedUp, booking.KM_Status);
		}

		public void TestReadFromTestFile_Update_ServiceLevelWithoutSpecifyingInstructions_ExistingTransportBooking()
		{
			var existingBooking = Helper.CreateBooking(Helper.CreateConsolidation(), "LC2C", "Desc", "ORG", "Ref");
			existingBooking.KM_RS_NKServiceLevel = "ABC";
			Factory.SaveForTesting();
			AssertEquals("Precondition: Existing TB should have Job Number 'TB00000001'", "TB00000001", existingBooking.KM_JobID);
			AssertEquals("Precondition: Existing TB should have 2 default instructions.", 2, existingBooking.Instructions.Count);

			var message = GetQueuedUniversalShipmentMessage(UpdateServiceLevelExistingTb);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var updatedBooking = new BusinessObjectFactory().LoadTop1<DtbBooking>(new ZQuery(DtbBookingSchema.KM_JobID, "TB00000001"));
			AssertEquals("AAA", updatedBooking.KM_RS_NKServiceLevel);
			AssertEquals("The instructions should not have been deleted.", 2, updatedBooking.Instructions.Count);
			Assert("The instructions should be the same.", !updatedBooking.Instructions.Any(i => !existingBooking.Instructions.Contains(i)));
		}

		public void TestReadFromTestFile_UpdateExistingTransportBooking_MAWBNotChangedWhenImportHasNoMAWB()
		{
			var existingBooking = Helper.CreateBooking(Helper.CreateConsolidation(), "LC2C", "Desc", "ORG", "Ref");
			existingBooking.ConsolidationSingleJob.AdditionalReferenceNumbers.AddNewIfNotExist("MAB", "111-111");
			Factory.SaveForTesting();
			AssertEquals("Precondition: Existing TB should have Job Number 'TB00000001'", "TB00000001", existingBooking.KM_JobID);
			AssertEquals("Precondition: Existing TB should have 2 default instructions.", 2, existingBooking.Instructions.Count);

			var message = GetQueuedUniversalShipmentMessage(UpdateServiceLevelExistingTb);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var updatedBooking = new BusinessObjectFactory().LoadTop1<DtbBooking>(new ZQuery(DtbBookingSchema.KM_JobID, "TB00000001"));
			AssertEquals("111-111", updatedBooking.WayBillNumber);
		}

		public void TestReadFromTestFile_Update_ServiceLevelDoesNotGetReset()
		{
			var existingBooking = Helper.CreateBooking(Helper.CreateConsolidation(), "LC2C", "Desc", "ORG", "Ref");
			existingBooking.KM_RS_NKServiceLevel = "ABC";
			Factory.SaveForTesting();
			AssertEquals("Precondition: Existing TB should have Job Number 'TB00000001'", "TB00000001", existingBooking.KM_JobID);
			AssertEquals("Precondition: Existing TB should have Service Level 'ABC'.", "ABC", existingBooking.KM_RS_NKServiceLevel);

			var updateTBValue = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - DoesNotOverrideServiceLevel.xml");
			var message = GetQueuedUniversalShipmentMessage(updateTBValue);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var updatedBooking = new BusinessObjectFactory().LoadTop1<DtbBooking>(new ZQuery(DtbBookingSchema.KM_JobID, "TB00000001"));
			AssertEquals("Service Level should remain 'ABC'.", "ABC", updatedBooking.KM_RS_NKServiceLevel);
		}

		public void TestReadFromTestFile_UpdateExistingTransportBooking_MAWBChangesWhenImportHasNewMAWB()
		{
			var existingBooking = Helper.CreateBooking(Helper.CreateConsolidation(), "LC2C", "Desc", "ORG", "Ref");
			existingBooking.ConsolidationSingleJob.AdditionalReferenceNumbers.AddNewIfNotExist("MAB", "111-111");
			Factory.SaveForTesting();
			AssertEquals("Precondition: Existing TB should have Job Number 'TB00000001'", "TB00000001", existingBooking.KM_JobID);
			AssertEquals("Precondition: Existing TB should have 2 default instructions.", 2, existingBooking.Instructions.Count);

			var updateMABValue = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - UpdateMABValue - Existing TB.xml");
			var message = GetQueuedUniversalShipmentMessage(updateMABValue);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var updatedBooking = new BusinessObjectFactory().LoadTop1<DtbBooking>(new ZQuery(DtbBookingSchema.KM_JobID, "TB00000001"));
			var mabsOnBooking = updatedBooking.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(AdditionalReferenceTypes.Codes.MasterBill);
			AssertContainsExactElementsInAnyOrder(mabsOnBooking, new ZString[] { "081-2222222", "081-3333333" });
		}

		public void TestReadFromTestFile_TransportBooking_DataTarget_TB_AdditionalReference()
		{
			var additionalReferences = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - DataTarget TB - AdditionalReferences.xml");
			var message = GetQueuedUniversalShipmentMessage(additionalReferences);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

			AssertMultilineASCIIEquals("Service Task Log", @"
Updated Transport Booking from UniversalShipment.
Added Transport Booking Consolidation  from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 1 x DtbBookingConsolidationPkgPackageJob, 2 x CusEntryNumber, 1 x DtbBooking.
".Trim(), serviceTaskLog.ToString());

			var logNoteText = message.GetLogNoteText();
			AssertMultilineASCIIEquals("message.GetLogNoteText()", @"Warning - Could not find Sending Party Organisation with eHub Client ID 'HYEUG1DZA'.
No matching DtbBookingConsolidation found, creating new DtbBookingConsolidation.
Populating DtbBookingConsolidation...
Warning - Could not find Sending Party Organisation with eHub Client ID 'HYEUG1DZA'.
Successfully loaded matching DtbBookingConsolidationPkgPackageJob.
Populating DtbBookingConsolidationPkgPackageJob...
Successfully loaded matching DtbBooking.
Populating DtbBooking...
No matching CusEntryNumber found, creating new CusEntryNumber.
Populating CusEntryNumber...
No matching CusEntryNumber found, creating new CusEntryNumber.
Populating CusEntryNumber...
Updated Transport Booking from UniversalShipment.
Added Transport Booking Consolidation  from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 1 x DtbBookingConsolidationPkgPackageJob, 2 x CusEntryNumber, 1 x DtbBooking.
".Trim(), logNoteText);

			var consolidation = Factory.LoadTop1<DtbBookingConsolidation>(new ZQuery(DtbBookingConsolidationSchema.KB_JobID, "CM00000001"));
			AssertEquals("Consolidation should not import additional reference numbers.", 0, consolidation.AdditionalReferenceNumbers.Count);

			var booking = consolidation.Bookings.Single();
			AssertEquals("Transport booking should have two additional reference numbers imported.", 2, booking.AdditionalReferenceNumbers.Count);

			var referenceNumbers = booking.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().Select(r => r.CE_EntryNum);
			AssertContainsExactElementsInAnyOrder("Correct additional reference numbers should be the imported.", referenceNumbers, new ZString[] { "2222", "5555" });
		}

		public void TestReadFromTestFile_Set_EmptyInstructionsOn_ExistingTransportBooking()
		{
			var existingBooking = Helper.CreateBooking(Helper.CreateConsolidation(), "LC2C", "Desc", "ORG", "Ref");
			existingBooking.KM_RS_NKServiceLevel = "ABC";
			Factory.SaveForTesting();
			AssertEquals("Precondition: Existing TB should have Job Number 'TB00000001'", "TB00000001", existingBooking.KM_JobID);
			AssertEquals("Precondition: Existing TB should have 2 default instructions.", 2, existingBooking.Instructions.Count);

			var setEmptyInstructions = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - SetEmptyInstructions - Existing TB.xml");
			var message = GetQueuedUniversalShipmentMessage(setEmptyInstructions);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var updatedBooking = new BusinessObjectFactory().LoadTop1<DtbBooking>(new ZQuery(DtbBookingSchema.KM_JobID, "TB00000001"));
			AssertEquals("The instructions should have been deleted.", 0, updatedBooking.Instructions.Count);
		}

		public void TestDataContextType()
		{
			AssertEquals(ExpectedDataContextType, new DtbBookingDataContextManager().DataContextType);
		}

		DataContextType ExpectedDataContextType => DataContextType.TransportBooking;

		public void TestDataContextKey()
		{
			var booking = Factory.New<DtbBooking>();
			booking.KM_JobID = "CN00001";
			AssertEquals("CN00001", booking.GetUniversalDataContextManager().DataContextKey);
		}

		public void TestGetDataContextKeyMatchingQuery()
		{
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();
			booking.KM_JobID = "CN00001";
			Factory.SaveForTesting();

			var eventDataObject = new UniversalEvent();
			eventDataObject.DataContext = DataContextFactory.New();
			eventDataObject.DataContext.AddDataTarget(ExpectedDataContextType, booking.KM_JobID);
			eventDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			eventDataObject.ContextCollection = new List<Context> { new Context { Type = nameof(UniversalEvent.ContextTypes.QuoteNumber), Value = "RandomValue" } };
			eventDataObject.EventType = Events.AuthorisedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;

			var message = GetQueuedUniversalEventMessage(eventDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			AssertEquals(string.Format("Linked Event to {0}.", booking.HumanReadableName), importResults.Single().ToString());
			AssertEquals(1, booking.Logs.Find(l => l.SL_SE_NKEvent == Events.AuthorisedCode).Count());
			AssertContains("", "".Trim(), serviceTaskLog.ToString());
		}

		public void TestDefaultOutputDirectory()
		{
			AssertNull(new DtbBookingDataContextManager().DefaultOutputDirectory);
		}

		public void TestManagesShipments()
		{
			AssertEquals(true, new DtbBookingDataContextManager().ManagesShipments);
		}

		public void TestShipmentDataObjectWriter()
		{
			IShipmentDataContextManager manager = new DtbBookingDataContextManager();
			AssertEquals(typeof(DtbBookingDataObjectWriter), manager.GetShipmentDataObjectWriter(new DataWritingManager(new DummyActionInfo())).GetType());
		}

		public void TestUniversalShipmentUpdateTransport_NonOverridenTranportWithCharges()
		{
			AssertUniversalShipmentUpdateTransport(false);
		}

		public void TestUniversalShipmentUpdateTransport_OverridenTranportWithCharges()
		{
			AssertUniversalShipmentUpdateTransport(true);
		}

		void AssertUniversalShipmentUpdateTransport(bool isOverridden)
		{
			var debtor = Helper.CreateOrganisation("Org1");
			debtor.OH_IsDebtor = true;
			var creditor = Helper.CreateOrganisation("Org2");
			creditor.OH_IsCreditor = true;

			var rate = Factory.New<RefAccTaxRate>();
			rate.ZAT_StartDate = ZDate.Today.AddDays(-100);
			rate.ZAT_EndDate = ZDate.Today.AddDays(100);
			rate.ZAT_ReferenceRateType = "STD";
			rate.ZAT_RN_NKCountry = "AU";

			var booking = Helper.CreateBooking();
			booking.KM_JobID = "TB0001";
			booking.ConsolidationSingleJob.KB_IsOverridden = isOverridden;
			new JobHeader.Loader(booking).TryLoadOrCreate();

			var code = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "OCART"));
			code.AC_DepartmentFilterList = "ALL";
			Factory.SaveForTesting();

			AssertNotNull("Precondition", booking.Job);

			var shipmentForBooking = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentForBooking.DataContext = DataContextFactory.New();
			shipmentForBooking.DataContext.AddDataTarget(ExpectedDataContextType, "TB0001");
			shipmentForBooking.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			AddJobCosting(shipmentForBooking);

			var message = GetQueuedUniversalShipmentMessage(shipmentForBooking);
			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			var results = manager.Process(message).ImportResults;
			var importResult = results.Single(o => o.DataContextType == ExpectedDataContextType);
			var importedBooking = (DtbBooking)importResult.GetBizOForTesting(shipmentForBooking, new BusinessObjectFactory());
			AssertEquals(true, importResult.WasSuccessful);
			AssertEquals(booking.PK, importedBooking.PK);

			var charge = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, importedBooking.Job.PK)).Single();
			AssertEquals(debtor.PK, charge.JR_OH_SellAccount);
			AssertEquals("OCART", charge.ChargeCode.AC_Code);
			AssertEquals(1500m, charge.JR_LocalSellAmt);
			AssertEquals("AUD", charge.JR_RX_NKCostCurrency);
		}

		void AddJobCosting(UniversalShipment shipment)
		{
			shipment.JobCosting = new JobCosting(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.JobCosting.SetChargeLineCollection(() =>
			{
				var chargeLine = new ChargeLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					ChargeCode = new ChargeCode { Code = "OCART" },
					CostLocalAmount = 1400m,
					CostOSAmount = 1500m,
					CostOSCurrency = new Currency { Code = "AUD" },
					Debtor = new OrganizationReference() { Type = "Organization", Key = "Org1" },
					Creditor = new OrganizationReference() { Type = "Organization", Key = "Org2" },
					Description = "My feelings, your skateboard",

					ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance)
					{
						Instruction = InstructionType.UpdateAndInsertIfNotFound,
					}
				};

				chargeLine.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>
				{
					new MatchingCriteria() { FieldName = "ChargeCode", Value = "OCART" }
				});

				return new List<ChargeLine> { chargeLine };
			});
		}

		public void TestImportEventWithEventTypeIRJAndMessageTypeCO2e()
		{
			// Arrange
			const string errorMessage = "ERROR! ERROR! and more ERROR...";
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();
			booking.KM_JobID = "CN00001";
			Factory.SaveForTesting();

			var eventDataObject = new UniversalEvent();
			eventDataObject.DataContext = DataContextFactory.New();
			eventDataObject.DataContext.AddDataTarget(ExpectedDataContextType, booking.KM_JobID);
			eventDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			eventDataObject.ContextCollection = new List<Context> { new Context { Type = nameof(UniversalEvent.ContextTypes.FailureReason), Value = errorMessage } };
			eventDataObject.EventType = Events.InterchangeRejectedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.EventParameters = new EventParameters();
			eventDataObject.EventParameters.MessageType = "WTG Greenhouse Gas Emission";

			// Act
			var message = GetQueuedUniversalEventMessage(eventDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			// Assert
			AssertEquals(CO2eStatusList.Codes.Rejected, booking.GetCO2eStatus());
			AssertEquals(1, booking.Logs.Find(log =>
				log.SL_SE_NKEvent == AutoEvents.GreenhouseGasEmissionsCalculationCode &&
				log.Parameters.TryGetValue(Params.Type, out var type) && type == nameof(CO2eEventType.Rejected) &&
				log.Parameters.TryGetValue(Params.Reason, out var reason) && reason == errorMessage).Count());
		}

		public void TestImportShipmentWithCO2eResponse()
		{
			// Arrange
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();
			booking.KM_JobID = "CN00001";
			Factory.SaveForTesting();

			var shipmentDataObject = CO2eTestHelper.CreateSampleCO2eResponse();
			shipmentDataObject.DataContext.AddDataTarget(DataContextType.TransportBooking, "CN00001");

			// Act
			var message = GetQueuedUniversalShipmentMessage(shipmentDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			// Assert
			AssertEquals(CO2eStatusList.Codes.Current, booking.GetCO2eStatus());
			AssertEquals(1, booking.Logs.Find(log =>
				log.SL_SE_NKEvent == AutoEvents.GreenhouseGasEmissionsCalculationCode &&
				log.Parameters.TryGetValue(Params.Type, out var type) && type == nameof(CO2eEventType.Updated) &&
				log.Parameters.TryGetValue(Params.New, out var newValue) && newValue == "10000").Count());
			AssertEquals(1, booking.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.DataImportCode).Count());
		}

		protected sealed override RecipientRoleType[] SupportedRecipientRoleTypes
		{
			get { return Array.Empty<RecipientRoleType>(); }
		}

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get { return "<UniversalShipment></UniversalShipment>"; }
		}

		protected override string GetExpectedDataSourceForUniversalShipmentTopLevelDataObject(IDataContextManager manager)
		{
			return "TransportBookingConsolidation [TBC00004567], TransportBooking [TB00002345]";
		}

		protected override DtbBooking GetNewBusinessObjectForTesting()
		{
			var result = base.GetNewBusinessObjectForTesting();
			result.KM_JobID = "TB00002345";
			result.ConsolidationSingleJob.KB_JobID = "TBC00004567";

			return result;
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

		string UpdateServiceLevelExistingTb => resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - UpdateServiceLevel - Existing TB.xml");

		TransportBookingTestHelper helper;
		TransportBookingTestHelper Helper => helper ?? (helper = new TransportBookingTestHelper(Factory.BOFactory));
	}
}
