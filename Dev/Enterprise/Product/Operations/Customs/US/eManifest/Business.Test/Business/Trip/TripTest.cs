using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class TripSingleTransactionTest : TestCase
	{
		class TripForMessageDeleteTest : Trip
		{
			public TripForMessageDeleteTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
				exceptionCount = 0;
			}

			int exceptionCount;
			public ZString failedMessageNum { get; set; }

			public override void OnSaving()
			{
				base.OnSaving();
				if (exceptionCount > 0)
				{
					failedMessageNum = ((EDIMessage)Messages.LastOrDefault())?.EM_MessageNum ?? ZString.Empty;
					exceptionCount--;
					throw new ConcurrencyConflictException();
				}
			}

			public void SetExceptionCount(int count)
			{
				exceptionCount = count;
			}
		}

		public void TestDeleteUnsavedMessages()
		{
			var newFactory = new BusinessObjectFactory();

			Db.Connection.BeginTransaction();
			var trip = newFactory.New<TripForMessageDeleteTest>();
			trip.SetExceptionCount(1);
			var message1 = newFactory.New<EDIMessage>();
			trip.Messages.Add(message1);
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			try
			{
				message1.OnSaving();
				newFactory.Save();
			}
			catch (Exception) { }
			var messageNum1 = trip.failedMessageNum;

			Db.Connection.RollbackTransaction();
			AssertEquals(false, message1.IsInDatabase);
			AssertEquals(true, message1.IsDeleted);

			Db.Connection.BeginTransaction();
			var message2 = newFactory.New<EDIMessage>();
			trip.Messages.Add(message2);
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			newFactory.Save();

			AssertEquals(messageNum1, message2.EM_MessageNum);
			AssertEquals(false, message1.IsInDatabase);
			AssertEquals(true, message2.IsInDatabase);
			AssertEquals(true, message1.IsDeleted);
			AssertEquals(false, message2.IsDeleted);

			Db.Connection.RollbackTransaction();
			Db.Connection.CloseConnection();
		}

		[UseSnapshotProtection]
		public void TestResetProperties()
		{
			var newFactory = new BusinessObjectFactory();

			var trip = newFactory.New<TripForMessageDeleteTest>();
			trip.SetExceptionCount(1);
			AssertEquals("", trip.BH_JobReference);
			AssertEquals("", trip.BH_MessageStatus);

			trip.BH_JobReference = "XXX";
			trip.BH_MessageStatus = MessageStatusList.Codes.AwaitingChange;
			try
			{
				newFactory.Save();
			}
			catch (Exception) { }
			AssertEquals("", trip.BH_JobReference);
			AssertEquals("", trip.BH_MessageStatus);

			trip.BH_JobReference = "XXX";
			trip.BH_MessageStatus = MessageStatusList.Codes.AwaitingChange;
			newFactory.Save();

			trip.SetExceptionCount(1);
			trip.BH_MessageStatus = MessageStatusList.Codes.AcknowledgedDelete;
			try
			{
				newFactory.Save();
			}
			catch (Exception) { }
			AssertEquals("XXX", trip.BH_JobReference);
			AssertEquals(MessageStatusList.Codes.AwaitingChange, trip.BH_MessageStatus);
		}
	}

	[TestedType(typeof(Trip))]
	sealed class TripTest : EnterpriseBusinessObjectTestCase
	{
		public void TestBH_MessageStatusIsUsedAsIRelatedJob_JobStatus()
		{
			var trip = Factory.NewWithValidTestData<Trip>();
			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedComplete;
			trip.BH_MessageStatus = MessageStatusList.Codes.AwaitingReplace;

			var relatedJob = trip as IRelatedJob;
			AssertEquals("JobStatus should return BH_MessageStatus", MessageStatusList.Codes.AwaitingReplace, relatedJob.JobStatus);

			trip.BH_MessageStatus = MessageStatusList.Codes.ClearOriginal;
			AssertEquals("JobStatus should return BH_MessageStatus", MessageStatusList.Codes.ClearOriginal, relatedJob.JobStatus);
		}

		public void TestPortUnladingDRefLocoMappings()
		{
			CreateLocoIfNotExists("USTES", "US");
			CreateLocoMapIfNotExists("4001", "USTES", USLocoMapSystemUsageList.Codes.Sea);
			CreateLocoMapIfNotExists("60001", "USTES", USLocoMapSystemUsageList.Codes.SCK);
			CreateLocoMapIfNotExists("4002", "USTES", USLocoMapSystemUsageList.Codes.Air);
			CreateLocoMapIfNotExists("4003", "USTES", USLocoMapSystemUsageList.Codes.All);
			CreateLocoMapIfNotExists("4004", "USTES", USLocoMapSystemUsageList.Codes.All);
			CreateLocoMapIfNotExists("40000", "USTES", USLocoMapSystemUsageList.Codes.All);

			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "4004", "Test Name", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "4003", "Test Name", startDate, endDate);
			Factory.Save();

			var trip = Factory.NewWithValidTestData<Trip>();
			Assert(!trip.PortUnladingDCodeIsDropEdit);
			AssertEquals(0, trip.PortUnladingDRefLocoMappings.Count);
			trip.BH_RL_NKPortUnlading = "USTES";
			Assert(trip.PortUnladingDCodeIsDropEdit);
			AssertEquals(2, trip.PortUnladingDRefLocoMappings.Count);
			Assert(trip.PortUnladingDRefLocoMappings.OfType<ICodeDescription>().Any(x => x.Code == "4003"));
			Assert(trip.PortUnladingDRefLocoMappings.OfType<ICodeDescription>().Any(x => x.Code == "4004"));
		}

		void CreateLocoIfNotExists(string locoCode, string country = "US")
		{
			var codeFilter = new ZQuery(RefUNLOCOSchema.RL_Code, locoCode);
			var unLoco = Factory.LoadTop1<RefUNLOCO>(codeFilter);
			if (unLoco == null)
			{
				var testUSLoco = Factory.NewWithValidTestData<RefUNLOCO>();
				testUSLoco.RL_Code = locoCode;
				testUSLoco.RL_PortName = "TEST Port - " + locoCode;
				testUSLoco.RL_IsSystem = true;
				testUSLoco.RL_HasAirport = true;
				testUSLoco.RL_HasSeaport = true;
				testUSLoco.RL_RN_NKCountryCode = country;
				Factory.Save();
			}
		}

		RefLocoMap CreateLocoMapIfNotExists(string localPort, string unLoco, string usage, bool isSystem = false)
		{
			var codeFilter = new ZQuery(RefLocoMapSchema.RY_LocalPortCode, localPort);
			codeFilter.AddToFilter(RefLocoMapSchema.RY_RL_NKLocoPort, unLoco);
			codeFilter.AddToFilter(RefLocoMapSchema.RY_SystemUsage, usage);
			codeFilter.AddToFilter(RefLocoMapSchema.RY_IsSystem, isSystem);

			var locoMap = Factory.LoadTop1<RefLocoMap>(codeFilter);
			if (locoMap == null)
			{
				locoMap = Factory.NewWithValidTestData<RefLocoMap>();
				locoMap.RY_LocalPortCode = localPort;
				locoMap.RY_RL_NKLocoPort = unLoco;
				locoMap.RY_SystemUsage = usage;
				locoMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
				locoMap.RY_IsSystem = isSystem;
			}

			return locoMap;
		}

		public void TestGetLastAcceptedDateAndStatus()
		{
			var trip = Factory.NewWithValidTestData<Trip>();

			var completeEManifestMessage = GetSentEDIMessage(Factory, MessageTypes.Codes.eManifest, MessageActionCodes.Codes.Original, "1", new ZDateTime(2021, 06, 01, 10, 15, 00));
			var unassociateMessage = GetSentEDIMessage(Factory, MessageTypes.Codes.UnassociatedShipments, MessageActionCodes.Codes.Original, "2", new ZDateTime(2021, 06, 02, 10, 15, 00));
			var preliminaryTripMessage = GetSentEDIMessage(Factory, MessageTypes.Codes.PreliminaryTrip, MessageActionCodes.Codes.Change, "3", new ZDateTime(2021, 06, 03, 10, 15, 00));
			var crewPassengerMessage = GetSentEDIMessage(Factory, MessageTypes.Codes.CrewAndPassenger, MessageActionCodes.Codes.Original, "4", new ZDateTime(2021, 06, 04, 10, 15, 00));
			var completeTripMessage = GetSentEDIMessage(Factory, MessageTypes.Codes.PreliminaryTrip, MessageActionCodes.Codes.Confirmation, "5", new ZDateTime(2021, 06, 05, 10, 15, 00));
			var cancelTripAndLinkedShipmentsMessage = GetSentEDIMessage(Factory, MessageTypes.Codes.PreliminaryTrip, MessageActionCodes.Codes.Cancellation, "6", new ZDateTime(2021, 06, 06, 10, 15, 00));
			var registerCrewInformationMessage = GetSentEDIMessage(Factory, MessageTypes.Codes.CrewOrEquipmentRegistration, MessageActionCodes.Codes.Original, "7", new ZDateTime(2021, 07, 07, 10, 15, 00));
			trip.Messages.Add(completeEManifestMessage);
			trip.Messages.Add(unassociateMessage);
			trip.Messages.Add(preliminaryTripMessage);
			trip.Messages.Add(crewPassengerMessage);
			trip.Messages.Add(completeTripMessage);
			trip.Messages.Add(cancelTripAndLinkedShipmentsMessage);
			trip.Messages.Add(registerCrewInformationMessage);

			var completeEManifestRecieveMessage = GetReceivedEDIMessage(Factory, MessageTypes.Codes.eManifest, EntryStatusList.Codes.Error, "1", new ZDateTime(2021, 06, 01, 10, 16, 00));
			trip.Messages.Add(completeEManifestRecieveMessage);
			AssertEquals("Last Accepted Complete eManifest Date", Env.Time.GetLocalTimeFromUtc(new DateTime(2021, 06, 01, 10, 16, 00)), trip.CompleteEManifestLastAcceptedDate);
			AssertEquals("Last Accepted Complete eManifest Status", "Error in last message, please fix and re-submit", trip.CompleteEManifestLastAcceptedStatus);

			var unassociateMessageRecieveMessage = GetReceivedEDIMessage(Factory, MessageTypes.Codes.UnassociatedShipments, EntryStatusList.Codes.Error, "2", new ZDateTime(2021, 06, 02, 10, 16, 00));
			trip.Messages.Add(unassociateMessageRecieveMessage);
			AssertEquals("Last Accepted Unassociated Shipments Date", Env.Time.GetLocalTimeFromUtc(new DateTime(2021, 06, 02, 10, 16, 00)), trip.UnassociatedShipmentsLastAcceptedDate);
			AssertEquals("Last Accepted Unassociated Shipments Status", "Error in last message, please fix and re-submit", trip.UnassociatedShipmentsLastAcceptedStatus);

			var preliminaryTripRecieveMessage = GetReceivedEDIMessage(Factory, MessageTypes.Codes.PreliminaryTrip, TripEntryStatusList.Codes.AcceptedPreliminary, "3", new ZDateTime(2021, 06, 03, 10, 16, 00));
			trip.Messages.Add(preliminaryTripRecieveMessage);
			AssertEquals("Last Accepted Preliminary Trip Details Date", Env.Time.GetLocalTimeFromUtc(new DateTime(2021, 06, 03, 10, 16, 00)), trip.PreliminaryTripDetailsLastAcceptedDate);
			AssertEquals("Last Accepted Preliminary Trip Details Status", "Preliminary e-Manifest accepted", trip.PreliminaryTripDetailsLastAcceptedStatus);

			var crewPassengerRecieveMessage = GetReceivedEDIMessage(Factory, MessageTypes.Codes.CrewAndPassenger, TripEntryStatusList.Codes.AcceptedPreliminary, "4", new ZDateTime(2021, 06, 04, 10, 16, 00));
			trip.Messages.Add(crewPassengerRecieveMessage);
			AssertEquals("Last Accepted Crew/Passengers Details Date", Env.Time.GetLocalTimeFromUtc(new DateTime(2021, 06, 04, 10, 16, 00)), trip.CrewPassengersLastAcceptedDate);
			AssertEquals("Last Accepted Crew/Passengers Details Status", "Preliminary e-Manifest accepted", trip.CrewPassengersLastAcceptedStatus);

			var completeTripRecieveMessage = GetReceivedEDIMessage(Factory, MessageTypes.Codes.PreliminaryTrip, TripEntryStatusList.Codes.AcceptedComplete, "5", new ZDateTime(2021, 06, 05, 10, 16, 00));
			trip.Messages.Add(completeTripRecieveMessage);
			AssertEquals("Last Accepted Confirm Trip Details Complete Date", Env.Time.GetLocalTimeFromUtc(new DateTime(2021, 06, 05, 10, 16, 00)), trip.CompleteTripDetailsLastAcceptedDate);
			AssertEquals("Last Accepted Confirm Trip Details Complete Status", "Complete e-Manifest accepted", trip.CompleteTripDetailsLastAcceptedStatus);

			var cancelTripAndLinkedShipmentsRecieveMessage = GetReceivedEDIMessage(Factory, MessageTypes.Codes.PreliminaryTrip, TripEntryStatusList.Codes.Cancelled, "6", new ZDateTime(2021, 06, 06, 10, 16, 00));
			trip.Messages.Add(cancelTripAndLinkedShipmentsRecieveMessage);
			AssertEquals("Last Accepted Cancel Trip And Linked Shipments Date", Env.Time.GetLocalTimeFromUtc(new DateTime(2021, 06, 06, 10, 16, 00)), trip.CancelTripAndLinkedShipmentsLastAcceptedDate);
			AssertEquals("Last Accepted Cancel Trip And Linked Shipments Status", "Canceled", trip.CancelTripAndLinkedShipmentsLastAcceptedStatus);

			var registerCrewInformationRecieveMessage = GetReceivedEDIMessage(Factory, MessageTypes.Codes.CrewOrEquipmentRegistration, TripEntryStatusList.Codes.Error, "7", new ZDateTime(2021, 06, 07, 10, 16, 00));
			trip.Messages.Add(registerCrewInformationRecieveMessage);
			AssertEquals("Last Accepted Register Crew Information Date", Env.Time.GetLocalTimeFromUtc(new DateTime(2021, 06, 07, 10, 16, 00)), trip.RegisterCrewInformationLastAcceptedDate);
			AssertEquals("Last Accepted Register Crew Information Status", "Error in last message, please fix and re-submit", trip.RegisterCrewInformationLastAcceptedStatus);

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			trip = newFactory.Load<Trip>(trip.PK);

			var unassociateMessage2 = GetSentEDIMessage(newFactory, MessageTypes.Codes.UnassociatedShipments, MessageActionCodes.Codes.Change, "8", new ZDateTime(2021, 06, 03, 10, 15, 00));
			trip.Messages.Add(unassociateMessage2);

			var unassociateMessageRecieveMessage2 = GetReceivedEDIMessage(newFactory, MessageTypes.Codes.UnassociatedShipments, TripEntryStatusList.Codes.AcceptedPreliminary, "8", new ZDateTime(2021, 06, 04, 10, 16, 00));
			trip.Messages.Add(unassociateMessageRecieveMessage2);
			AssertEquals("Get latest date", Env.Time.GetLocalTimeFromUtc(new DateTime(2021, 06, 04, 10, 16, 00)), trip.UnassociatedShipmentsLastAcceptedDate);
			AssertEquals("Get latest Status", "Preliminary e-Manifest accepted", trip.UnassociatedShipmentsLastAcceptedStatus);
		}

		[UseSnapshotProtection]
		public void TestShouldDefaultUniqueJobReference()
		{
			var factory1 = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();

			var trip1 = factory1.New<Trip>();
			var dbConnection = ((IDbConnected)factory1).Connection;
			trip1.PopulateJobReferenceIfNeeded();
			var reference1 = trip1.BH_JobReference;
			dbConnection.RollbackTransaction();

			var trip2 = factory2.New<Trip>();
			factory2.Save();
			var reference2 = trip2.BH_JobReference;

			AssertEquals(false, reference1.IsEmpty);
			AssertEquals(false, reference2.IsEmpty);
			AssertEquals(reference1, reference2);

			dbConnection.BeginTransaction();
			AssertEquals(reference1, trip1.BH_JobReference);

			factory1.Save();
			AssertNotEquals(reference1, trip1.BH_JobReference);
		}

		public void TestInstanceOfType()
		{
			var parentType = CargoWise.Application.ObjectFactory.GetType<Integration.Customs.ICusInBondHeader>();
			var trip = Factory.New<Trip>();
			AssertEquals("Trip", true, parentType.IsInstanceOfType(trip));
		}

		public void TestIsFromHVLV()
		{
			var trip = Factory.New<Trip>();
			Assert("Trip is not from HVHV", !trip.IsFromHVLV);

			trip.Logs.AddNew(AutoEvents.Transferred, new KeyValuePair<string, string>[] {
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, "HVL"),
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.JobNumber, "job123") });
			Factory.Save();

			Assert("Trip is from HVHV", trip.IsFromHVLV);
		}

		public void TestLatestSentPTRMessageDateTime()
		{
			var trip = Factory.New<Trip>();
			var ediMessage1 = Factory.New<EDIMessage>();
			ediMessage1.EM_ApplicationCode = "MAN";
			ediMessage1.EM_MessageType = "PTR";
			ediMessage1.EM_ReceiveTransmit = "TRX";
			ediMessage1.EM_MessageNum = "1";
			ediMessage1.EM_Status = "QUE";
			ediMessage1.EM_SystemCreateTimeUtc = new ZDateTime(2017, 11, 27, 11, 14, 0);
			trip.Messages.Add(ediMessage1);

			AssertEquals(ZDateTime.Empty, trip.LatestSentMessageDateTime);

			var ediMessage2 = Factory.New<EDIMessage>();
			ediMessage2.EM_ApplicationCode = "MAN";
			ediMessage2.EM_MessageType = "ACK";
			ediMessage2.EM_ReceiveTransmit = "TRX";
			ediMessage2.EM_MessageNum = "2";
			ediMessage2.EM_Status = "SNT";
			ediMessage2.EM_SystemCreateTimeUtc = new ZDateTime(2017, 11, 27, 11, 15, 0);
			trip.Messages.Add(ediMessage2);

			AssertEquals(ediMessage2.EM_SystemCreateTimeUtc, trip.LatestSentMessageDateTime);

			var ediMessage3 = Factory.New<EDIMessage>();
			ediMessage3.EM_ApplicationCode = "MAN";
			ediMessage3.EM_MessageType = "PTR";
			ediMessage3.EM_ReceiveTransmit = "TRX";
			ediMessage3.EM_MessageNum = "3";
			ediMessage3.EM_Status = "SNT";
			ediMessage3.EM_SystemCreateTimeUtc = new ZDateTime(2017, 11, 27, 11, 16, 0);
			trip.Messages.Add(ediMessage3);

			AssertEquals(ediMessage3.EM_SystemCreateTimeUtc, trip.LatestSentMessageDateTime);
		}

		public void TestHumanReadableShortcutNameInTrip()
		{
			var trip = Factory.New<Trip>();
			trip.BH_JobReference = "MAN0000001";
			AssertEquals("HumanReadableShortCutName", trip.BH_JobReference + " - TRP: " + trip.BH_VoyageNumber, trip.HumanReadableShortcutName);

			trip.BH_VoyageNumber = ZString.Empty;
			AssertEquals("HumanReadableShortCutName", trip.BH_JobReference, trip.HumanReadableShortcutName);

			trip.BH_OA_Importer = Factory.NewWithValidTestData<OrgAddress>().PK;

			AssertNotNull("BH_OA_Importer_Address", trip.BH_OA_Importer_ZAddress);
			AssertNotNull("BH_OA_Importer_Address.OrgHeader", trip.BH_OA_Importer_ZAddress.OrgHeader);
			var importer = ((OrgHeader)trip.BH_OA_Importer_ZAddress.OrgHeader).OH_FullName;
			AssertEquals("HumanReadableShortCutName", trip.BH_JobReference + " - " + importer, trip.HumanReadableShortcutName);

			string tripReference = "112233";
			trip.BH_VoyageNumber = tripReference;
			AssertEquals("HumanReadableShortCutName", trip.BH_JobReference + " - TRP: " + tripReference + ", " + importer, trip.HumanReadableShortcutName);
		}

		[TestDate(2015, 08, 22, 14, 01, 00)]
		public void TestDefaultValues()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2772", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var trip = Factory.New<Trip>();
			trip.BH_JobReference = "123456";
			AssertEquals("BH_ImportTransportMode", TransportModes.Codes.Road, trip.BH_ImportTransportMode);
			AssertEquals("Voyage Number as Trip Reference", "123456", trip.BH_VoyageNumber);
			AssertEquals("Voyage Number as Trip Reference is editable", false, trip.BH_VoyageNumberInfo.ReadOnly);
			AssertEquals("BH_TransitDirection", TransitDirectionCodes.Codes.Importation, trip.BH_TransitDirection);
			AssertEquals("HumanReadableName", "Trip: 123456", trip.HumanReadableName);
			AssertEquals("ETA = now+2h", new ZDateTime(2015, 08, 22, 16, 01, 00), trip.BH_ETA);

			trip.BH_RL_NKPortUnlading = "USLAX";
			AssertEquals("2772", trip.BH_PortUnladingDCode);
		}

		public void TestSetCarrierAndScac()
		{
			var trip = Factory.New<Trip>();
			var carrier = Factory.New<OrgHeader>();
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "1234", Core.Constants.CountryCodes.UnitedStates);
			var cusCode = carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.TruckCarrierCode, "4566", Core.Constants.CountryCodes.UnitedStates);
			trip.BH_OH_Carrier = carrier.PK;
			AssertEquals("4566", trip.BH_CarrierSCAC);
			cusCode.Delete();
			trip.BH_OH_Carrier = ZGuid.Empty;
			trip.BH_CarrierSCAC = ZString.Empty;
			trip.BH_OH_Carrier = carrier.PK;
			AssertEquals("1234", trip.BH_CarrierSCAC);
		}

		public void TestSavingSetsJobReference()
		{
			TestConnection.BeginTransaction(); // Updating next number fountain value for the test
			try
			{
				Env.NumberFountains.USeManifestTripReference.SetNext(Factory, 6789);
				var trip = Factory.New<Trip>();
				AssertEquals("BH_JobReference ReadOnly", true, trip.BH_JobReferenceInfo.ReadOnly);
				trip.OnSaving();
				AssertEquals("MAN0006789", trip.BH_JobReference);

				var participant = Factory as ITransactionParticipant;
				participant.OnAllTransactionsRolledBack();
				AssertEquals(string.Empty, trip.BH_JobReference);

				trip.OnSaving();
				AssertEquals("MAN0006790", trip.BH_JobReference);
				Factory.Save();
				AssertEquals("MAN0006790", trip.BH_JobReference);
			}
			finally
			{
				TestConnection.RollbackTransaction(); // Updating next number fountain value for the test
			}
		}

		[TestDate(2012, 05, 23)]
		public void TestSavingSetsJobReferenceUsingCustomisation()
		{
			var trip = Factory.New<Trip>();

			var customisation = new TripNumberCustomisation();
			TripNumberCustomisationTest.Set(customisation, BillOfLadingNumberCustomisationElement.Keys.YearAsDigit, 1, true, "1");
			TripNumberCustomisationTest.Set(customisation, BillOfLadingNumberCustomisationElement.Keys.MonthAsLetter, 2, true);
			TripNumberCustomisationTest.Set(customisation, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 50, false, "3");
			USeManifestDataRegistry.Instance.NumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation);

			trip.BH_JobReference = ZString.Empty;
			trip.OnSaving();
			AssertEquals("BF_JobReference", "MAN2E001", trip.BH_JobReference);

			customisation = new TripNumberCustomisation();
			TripNumberCustomisationTest.Set(customisation, BillOfLadingNumberCustomisationElement.Keys.BranchCode, 1, true);
			TripNumberCustomisationTest.Set(customisation, BillOfLadingNumberCustomisationElement.Keys.MonthAsLetter, 2, true);
			TripNumberCustomisationTest.Set(customisation, BillOfLadingNumberCustomisationElement.Keys.YearAsLetter, 3, true);
			TripNumberCustomisationTest.Set(customisation, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 50, false, "3");

			USeManifestDataRegistry.Instance.NumberCustomisation.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, customisation);

			trip.BH_JobReference = ZString.Empty;
			trip.OnSaving();
			AssertEquals("BF_JobReference", "MANBNEEL001", trip.BH_JobReference);
		}

		public void TestPopulateEquipmentForSiblings()
		{
			var mockShipmentPK = ZGuid.NewZGuid();

			var trip1 = Factory.NewWithValidTestData<Trip>();
			trip1.BH_ParentID = mockShipmentPK;
			trip1.BH_ParentTableCode = "JS";

			var trip2 = Factory.NewWithValidTestData<Trip>();
			trip2.BH_ParentID = mockShipmentPK;
			trip2.BH_ParentTableCode = "JS";

			Assert(!(trip2.Equipment.Count > 0));

			var equipment = trip1.Equipment.AddNew();
			equipment.BJ_RegistrationNumber = "equipment";
			Factory.Save();

			CombineAssertions("Populate Equipment For Siblings", () =>
			{
				Assert(trip2.Equipment.Count > 0);
				AssertEquals("equipment", trip2.Equipment.SingleOrDefault().BJ_RegistrationNumber);
			});
		}

		public void TestPopulateCrewMembersForSiblings()
		{
			var mockShipmentPK = ZGuid.NewZGuid();

			var trip1 = Factory.NewWithValidTestData<Trip>();
			trip1.BH_ParentID = mockShipmentPK;
			trip1.BH_ParentTableCode = "JS";

			var trip2 = Factory.NewWithValidTestData<Trip>();
			trip2.BH_ParentID = mockShipmentPK;
			trip2.BH_ParentTableCode = "JS";

			Assert(!(trip2.CrewMembers.Count > 0));

			var crewMember = trip1.CrewMembers.AddNew();
			crewMember.CP_FullName = "CREWMEMBERS";
			Factory.Save();

			CombineAssertions("Populate CrewMembers For Siblings", () =>
			{
				Assert(trip2.CrewMembers.Count > 0);
				AssertEquals("CREWMEMBERS", trip2.CrewMembers.SingleOrDefault().CP_FullName);
			});
		}

		public void TestPopulateCommodityEquipment()
		{
			var trip = Factory.New<Trip>();
			trip.BH_GB = Factory.NewWithValidTestData<GlbBranch>().PK;
			trip.BH_OA_Importer = Factory.NewWithValidTestData<OrgAddress>().PK;

			var shipment = trip.Shipments.AddNew();
			var commodity1 = Factory.NewWithValidTestData<Commodity>();
			var commodity2 = Factory.NewWithValidTestData<Commodity>();
			shipment.Commodities.Add(commodity1);
			shipment.Commodities.Add(commodity2);

			Assert("Commodity1 has not been given a default equipment value", commodity1.BY_BJ_Equipment.IsEmpty);
			Assert("Commodity2 has not been given a default equipment value", commodity2.BY_BJ_Equipment.IsEmpty);

			trip.PopulateCommodityWithEquipment(ZGuid.Empty);
			Assert("Commodity1 is still not given a default equipment value", commodity1.BY_BJ_Equipment.IsEmpty);
			Assert("Commodity2 is still not given a default equipment value", commodity2.BY_BJ_Equipment.IsEmpty);

			var equipment1 = trip.AllEquipmentIncludingMainConveyance.AddNew();

			AssertEquals("The BY_BJ_Equipment of Commodity1 is equipment1 PK", commodity1.BY_BJ_Equipment, equipment1.PK);
			AssertEquals("The BY_BJ_Equipment of Commodity2 is equipment1 PK", commodity2.BY_BJ_Equipment, equipment1.PK);

			var equipment2 = trip.AllEquipmentIncludingMainConveyance.AddNew();

			trip.PopulateCommodityWithEquipment(equipment2.PK);
			AssertEquals("The BY_BJ_Equipment of Commodity1 is equipment2 PK", commodity1.BY_BJ_Equipment, equipment2.PK);
			AssertEquals("The BY_BJ_Equipment of Commodity2 is equipment2 PK", commodity2.BY_BJ_Equipment, equipment2.PK);

			var equipment3 = trip.AllEquipmentIncludingMainConveyance.AddNew();
			equipment3.BJ_IsConveyance = true;
			
			AssertEquals("The BY_BJ_Equipment of Commodity1 is equipment3 PK", commodity1.BY_BJ_Equipment, equipment3.PK);
			AssertEquals("The BY_BJ_Equipment of Commodity2 is equipment3 PK", commodity2.BY_BJ_Equipment, equipment3.PK);
		}

		public void TestDelete()
		{
			var trip = Factory.New<Trip>();
			var shipment = trip.Shipments.AddNew();
			var equipment = trip.Equipment.AddNew();
			var crewMember = trip.CrewMembers.AddNew();
			var conveyance = trip.Conveyance;
			var processTask = trip.WorkflowItems.AddNew();
			var doc = trip.RequiredDocuments.AddNew();

			trip.Delete();

			Assert("Conveyance should be deleted", conveyance.IsDeleted);
			Assert("Shipments should be deleted", shipment.IsDeleted);
			Assert("Equipment should be deleted", equipment.IsDeleted);
			Assert("CrewMembers should be deleted", crewMember.IsDeleted);
			Assert("CrewMembers should be deleted", crewMember.IsDeleted);
			Assert("WorkflowItems should be deleted", processTask.IsDeleted);
			Assert("RequiredDocuments should be deleted", doc.IsDeleted);
		}

		public void TestBusinessObjectsWithRelatedEvents()
		{
			var trip = Factory.New<Trip>();
			var shipment1 = trip.Shipments.AddNew();
			var shipment2 = trip.Shipments.AddNew();

			AssertEquals("BusinessObjectsWithRelatedEvents.Count", 2, trip.BusinessObjectsWithRelatedEvents.Length);
			AssertEquals("Shipment 1", shipment1.PK, trip.BusinessObjectsWithRelatedEvents[0].PK);
			AssertEquals("Shipment 2", shipment2.PK, trip.BusinessObjectsWithRelatedEvents[1].PK);
		}

		public void TestStatusChangedLog()
		{
			var trip = Factory.New<Trip>();
			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.Error;
			var log = trip.Logs.MostRecentLogByEventTime(Events.CustomsManifestStatus, trip.BH_ReleaseStatusCodeDescription);
			AssertNotNull("Error status has been set", log);

			var count = trip.Logs.GetAllLogs().Count;
			trip.BH_ReleaseStatus = ZString.Empty;
			AssertEquals("No logs if status cleared", count, trip.Logs.GetAllLogs().Count);

			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedComplete;
			log = trip.Logs.MostRecentLogByEventTime(Events.CustomsManifestStatus, trip.BH_ReleaseStatusCodeDescription);
			AssertNotNull("Clear status has been set", log);
		}

		public void TestIsFinalized()
		{
			var trip = Factory.New<Trip>();
			Assert("IsFinalized", !trip.IsFinalized);

			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.HoldTrip;
			Assert("IsFinalized", trip.IsFinalized);

			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedComplete;
			Assert("IsFinalized", trip.IsFinalized);

			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedPreliminary;
			Assert("IsFinalized", !trip.IsFinalized);

			trip.BH_ReleaseStatus = MessageTypes.Codes.SyntaxError;
			Assert("IsFinalized", !trip.IsFinalized);

			trip.BH_ReleaseStatus = EntryStatusList.Codes.Error;
			Assert("IsFinalized", !trip.IsFinalized);

			trip.BH_ReleaseStatus = EntryStatusList.Codes.Cancelled;
			Assert("IsFinalized", !trip.IsFinalized);
		}

		public void TestIsLodged()
		{
			var trip = Factory.New<Trip>();
			Assert("IsLodged", !trip.IsLodged);

			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.HoldTrip;
			Assert("IsLodged", trip.IsLodged);

			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedComplete;
			Assert("IsLodged", trip.IsLodged);

			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedPreliminary;
			Assert("IsLodged", trip.IsLodged);

			trip.BH_ReleaseStatus = MessageTypes.Codes.SyntaxError;
			Assert("IsLodged", !trip.IsLodged);

			trip.BH_ReleaseStatus = EntryStatusList.Codes.Error;
			Assert("IsLodged", !trip.IsLodged);

			trip.BH_ReleaseStatus = EntryStatusList.Codes.Cancelled;
			Assert("IsLodged", !trip.IsLodged);
		}

		[ExpectNoExceptions]
		public void TestIMultiSelectHandler()
		{
			var trip1 = Factory.New<Trip>();
			var implementation = (IMultiSelectHandler)trip1;

			AssertEquals("FilterModuleId", ModuleIDs.Customs.US.eManifestShipment, implementation.FilterModuleId);
			AssertEquals("AdditionalFilter", string.Format("B0_BH <> '{0}'\r\n", trip1.PK), implementation.AdditionalFilter.LiteralTextSqlFormatted);

			var trip2 = Factory.New<Trip>();
			var shipment1 = trip2.Shipments.AddNew();
			var shipment2 = trip2.Shipments.AddNew();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			trip1 = newFactory.Load<Trip>(trip1.PK);
			trip2 = newFactory.Load<Trip>(trip2.PK);
			implementation = trip1;
			implementation.HandleSelectedObjects(new BusinessObject[] { shipment1, shipment2 });

			AssertEquals("Trip 1 shipment count", 2, trip1.Shipments.Count);
			AssertNotNull("Trip 1 contains shipment 1", trip1.Shipments.FindByPK(shipment1.PK));
			AssertNotNull("Trip 1 contains shipment 2", trip1.Shipments.FindByPK(shipment2.PK));
			Assert("Trip 1 has changes", trip1.HasChanges);
			AssertEquals("Trip 2 shipment count", 0, trip2.Shipments.Count);
		}

		public void TestIEDocsProvider()
		{
			var trip = (IEDocsProvider)Factory.New<Trip>();
			AssertEquals("EDocsProviderSupporter", typeof(JobInvoicingEDocsProviderSupporter), trip.GetEDocsProviderSupporter().GetType());
			AssertEquals("DocumentSupporter", typeof(TripDocumentSupporter), trip.DocumentSupporter.GetType());
			AssertEquals("DocManagerInfo", typeof(DocManagerInfo), trip.DocManagerInfo.GetType());
			AssertEquals("DocManagerCode", Constants.DocManagerCodes.USeManifest, trip.DocManagerInfo.DocManagerCode);
		}

		public void TestSettingScheduleDSetsUnloco()
		{
			var trip = Factory.New<Trip>();
			trip.BH_PortUnladingDCode = "0901";
			AssertEquals("USBUF", trip.BH_RL_NKPortUnlading);
		}

		public void TestIHaveRequiredDocuments()
		{
			var trip = Factory.New<Trip>();
			trip.BH_JobReference = "MAN0000001";
			var implementation = (IHaveRequiredDocuments)trip;
			AssertNull("AdditionalRefTypes", implementation.AdditionalRefTypes);
			AssertNull("ExportBroker", implementation.ExportBroker);
			AssertEquals("MasterBill", ZString.Empty, implementation.MasterBill);
			AssertEquals("HouseBill", ZString.Empty, implementation.HouseBill);
			AssertEquals("Logs", trip.Logs, implementation.Logs);
			AssertEquals("RequiredDocuments", typeof(JobRequiredDocumentDependentCollection), implementation.RequiredDocuments.GetType());
			AssertEquals("TableCode", trip.TablePrefix, implementation.TableCode);
			AssertEquals("UltimateDocumentParent", trip.PK, implementation.UltimateDocumentParent.PK);
			AssertEquals("UniqueConsignRef", "MAN0000001", implementation.UniqueConsignRef);
		}

		public void TestIJobInvoicingPlugIn()
		{
			var implementation = (IJobInvoicingPlugIn)Factory.New<Trip>();

			Db.Connection.BeginTransaction(); // Updating next number fountain value for the test
			try
			{
				implementation.SetJobNumberFieldOnSaving();
			}
			finally
			{
				Db.Connection.CommitTransaction(); // Updating next number fountain value for the test
			}

			AssertEquals("JobNumber", "MAN0000001", implementation.JobNumber);
			AssertEquals("AllowInvoiceDeletion", true, implementation.AllowInvoiceDeletion);
			AssertEquals("InvoicingSupporter", typeof(eManifestJobInvoicingSupporter), implementation.InvoicingSupporter.GetType());
		}

		public void TestCanSetEquipmentDetailsDirectlyWithoutSelectingAnExistingConveyanceAndItWillLazyCreateTheRQ()
		{
			var trip = Factory.New<Trip>();
			trip.Conveyance.BJ_RegistrationNumber = "Truck 123";
			AssertEquals("Equipment was lazy-created - the above line and this line didn't explode", "Truck 123", trip.Conveyance.BJ_RegistrationNumber);
			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.SetCountrySpecificContainerCode(ConveyanceTypes.Codes.PickupTruck, Enterprise.Core.Constants.CountryCodes.UnitedStates);
			var refEquipment = Factory.NewWithValidTestData<RefEquipment>();
			refEquipment.RQ_RC_RoadContainerType = refContainer.PK;
			refEquipment.RQ_Registration = "REGO";
			refEquipment.RQ_RN_NKRegistrationCountry = "AA";
			refEquipment.RQ_RegState = "BB";
			var ace = refEquipment.Certificates.AddNew();
			ace.XZ_Type = ConveyanceReferences.Codes.ACEId;
			ace.XZ_RefNumber = "VENTURA";
			refEquipment.RQ_EquipmentType = "DC";
			var trip2 = Factory.New<Trip>();
			trip2.Conveyance.BJ_RQ_Equipment = refEquipment.PK;
			AssertEquals("Properties are proxied when setting BJ_RQ_Equipment - rego", "REGO", trip2.Conveyance.BJ_RegistrationNumber);
			AssertEquals(true, trip2.Conveyance.BJ_RegistrationNumberInfo.ReadOnly);
			AssertEquals("Properties are proxied when setting BJ_RQ_Equipment - ACE", "VENTURA", trip2.Conveyance.BJ_ACEID);
			AssertEquals(true, trip2.Conveyance.BJ_ACEIDInfo.ReadOnly);
			AssertEquals("Properties are proxied when setting BJ_RQ_Equipment - type", "DC", trip2.Conveyance.BJ_ContainerType);
			AssertEquals(true, trip2.Conveyance.BJ_ContainerTypeInfo.ReadOnly);
			AssertEquals("Properties are proxied when setting BJ_RQ_Equipment - type", refContainer.PK, trip2.Conveyance.BJ_RC_RoadContainerType);
			AssertEquals(true, trip2.Conveyance.BJ_RC_RoadContainerTypeInfo.ReadOnly);
			AssertEquals("Properties are proxied when setting BJ_RQ_Equipment - type", "AA", trip2.Conveyance.BJ_RN_NKRegistrationCountry);
			AssertEquals(true, trip2.Conveyance.BJ_RN_NKRegistrationCountryInfo.ReadOnly);
			AssertEquals("Properties are proxied when setting BJ_RQ_Equipment - type", "BB", trip2.Conveyance.BJ_RW_NKRegistrationState);
			AssertEquals(true, trip2.Conveyance.BJ_RW_NKRegistrationStateInfo.ReadOnly);

			trip2.Conveyance.BJ_RQ_Equipment = ZGuid.Empty;
			AssertEquals(false, trip2.Conveyance.BJ_RegistrationNumberInfo.ReadOnly);
			AssertEquals(false, trip2.Conveyance.BJ_ACEIDInfo.ReadOnly);
			AssertEquals(false, trip2.Conveyance.BJ_ContainerTypeInfo.ReadOnly);
			AssertEquals(false, trip2.Conveyance.BJ_RC_RoadContainerTypeInfo.ReadOnly);
			AssertEquals(false, trip2.Conveyance.BJ_RN_NKRegistrationCountryInfo.ReadOnly);
			AssertEquals(false, trip2.Conveyance.BJ_RW_NKRegistrationStateInfo.ReadOnly);
		}

		public void TestJobHeaderIsNotDeactivatedWhenFactoryHasInvoicingPlugInGUIBusinessContext()
		{
			AssertJobHeaderDeactivation(true);
		}

		public void TestJobHeaderIsDeactivatedWhenFactoryDoesNotHaveInvoicingPlugInGUIBusinessContext()
		{
			AssertJobHeaderDeactivation(false);
		}

		public void TestGetNeweManifestProcessTaskCollection()
		{
			var trip = Factory.New<Trip>();
			AssertType("WorkflowItems's type should be ProcessTaskCollection<eManifestProcessTask, Trip>", typeof(ProcessTaskCollection<eManifestProcessTask, Trip>), ((IWorkflowProvider)trip).WorkflowItems);
		}

		static EDIMessage GetReceivedEDIMessage(BusinessObjectFactory factory, string type, string subType, string messageNum, ZDateTime createTime)
		{
			var message = factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.USeManifest;
			message.EM_MessageType = type;
			message.EM_MessageSubType = subType;
			message.EM_MessageText = "Test Message Placeholder";
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message.EM_Status = EDIMessageStatusList.Codes.Received;
			message.EM_SystemCreateTimeUtc = createTime;
			message.EM_MessageNum = messageNum;
			return message;
		}

		static EDIMessage GetSentEDIMessage(BusinessObjectFactory factory, string messageType, string messageSubType, string messageNum, ZDateTime createTime)
		{
			var sentMessage = factory.New<EDIMessage>();
			sentMessage.EM_MessageType = messageType;
			sentMessage.EM_MessageNum = messageNum;
			sentMessage.EM_ApplicationReference = messageNum;
			sentMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			sentMessage.EM_Status = EDIMessageStatusList.Codes.Sent;
			sentMessage.EM_MessageText = "Test Message Placeholder";
			sentMessage.EM_SystemCreateTimeUtc = createTime;
			sentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USeManifest;
			sentMessage.EM_MessageSubType = messageSubType;
			return sentMessage;
		}

		void AssertJobHeaderDeactivation(bool hasInvoicingPlugInGUIContext)
		{
			var trip = Factory.New<Trip>();
			var jobLoader = new JobHeader.Loader(trip);
			var job = jobLoader.TryCreate();
			Factory.Save();

			trip.IsCancelled = true;
			if (hasInvoicingPlugInGUIContext)
			{
				Factory.SetContext(BusinessContext.InvoicingPlugInGUI);
			}

			var assertionMessage1 = string.Format("trip {0} have InvoicingPluginGUI Business Context", hasInvoicingPlugInGUIContext ? "should" : "should not");
			var assertionMessage2 = string.Format("Job {0} be deactivated by OnSaving method", hasInvoicingPlugInGUIContext ? "should not" : "should");

			Assert("Deactivating trip, IsCancelled flag should be set to true", trip.IsCancelled);
			Assert("Deactivating trip, IsCancelledInfo should have changes", trip.IsCancelledHasChanged);
			AssertEquals(assertionMessage1, hasInvoicingPlugInGUIContext, trip.HasContext(BusinessContext.InvoicingPlugInGUI));
			Assert("Job is not yet deactivated", !job.IsCancelled);

			Factory.Save();

			AssertEquals(assertionMessage2, !hasInvoicingPlugInGUIContext, job.IsCancelled);
		}
	}
}
