using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class TripLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookups()
		{
			var lookups = Factory.New<Trip>().Lookups;
			AssertEquals("TransportModes", typeof(TransportModes), lookups.TransportModes.GetType());
			AssertEquals("TransitDirectionCodes", typeof(TransitDirectionCodes), lookups.TransitDirectionCodes.GetType());
			AssertEquals("ScheduleDPortCodes", typeof(ZZRefCusCodeListCombinedCollection), lookups.ScheduleDPortCodes.GetType());
			AssertEquals("ScheduleKPortCodes", typeof(ZZRefCusCodeListCombinedCollection), lookups.ScheduleKPortCodes.GetType());
			AssertEquals("SCACCarrierCodes", typeof(USCarrierCombinedCollection), lookups.SCACCarrierCodes.GetType());
			var filters = (lookups.SCACCarrierCodes as USCarrierCombinedCollection).FilterBusinessObjectDefaults;
			var motFilter = filters["Mode Of Transportation:Property"];
			AssertEquals(Trip.Truck, motFilter.Value);
			AssertEquals("Organizations", typeof(OrgHeaderCollection), lookups.Organizations.GetType());
			AssertEquals("Countries", typeof(RefCountryCollection), lookups.Countries.GetType());
			AssertEquals("Currencies", typeof(RefCurrencyCollection), lookups.Currencies.GetType());
			AssertEquals("Importers", typeof(DebtorCollection), lookups.Importers.GetType());
		}

		public void TestScheduleKPortCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			var foreignPort1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "port1", "port1", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			var foreignPort2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "port2", "port2", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort2.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.InBond);
			var foreignPort3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "port3", "port3", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort3.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.InBond);
			Factory.Save();
			var scheduleKCodeList = Factory.New<Trip>().Lookups.ScheduleKPortCodes;
			CombineAssertions(() =>
			{
				AssertNotNull(scheduleKCodeList);
				AssertSame("Cached", Factory.GetCachedValue("ScheduleKPortCodes", () => new ZZRefCusCodeListCombinedCollection(new BusinessObjectFactory())), scheduleKCodeList);
				AssertEquals(3, scheduleKCodeList.Count);
				Assert(scheduleKCodeList.Contains(foreignPort1));
				Assert(scheduleKCodeList.Contains(foreignPort2));
				Assert(scheduleKCodeList.Contains(foreignPort3));
			});
		}

		public void TestScheduleDPortCodes()
		{
			CreateLocoIfNotExists("USTES", "US");
			CreateLocoMapIfNotExists("4001", "USTES", USLocoMapSystemUsageList.Codes.Sea);
			CreateLocoMapIfNotExists("60001", "USTES", USLocoMapSystemUsageList.Codes.SCK);
			CreateLocoMapIfNotExists("4002", "USTES", USLocoMapSystemUsageList.Codes.Air);
			CreateLocoMapIfNotExists("40030", "USTES", USLocoMapSystemUsageList.Codes.All);
			CreateLocoMapIfNotExists("4004", "USTES", USLocoMapSystemUsageList.Codes.All);
			CreateLocoMapIfNotExists("4005", "USTES", USLocoMapSystemUsageList.Codes.All);

			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var port1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "4004", "Test Name", startDate, endDate);
			var port2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "4005", "Test Name", startDate, endDate);
			var port3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "4003", "Test Name", startDate, endDate);
			newFactory.Save();

			var trip = Factory.NewWithValidTestData<Trip>();
			AssertEquals(0, trip.Lookups.ScheduleDPortCodes.Count);

			trip.BH_RL_NKPortUnlading = "USTES";
			AssertEquals(3, trip.Lookups.ScheduleDPortCodes.Count);
			Assert(trip.Lookups.ScheduleDPortCodes.Contains(port1));
			Assert(trip.Lookups.ScheduleDPortCodes.Contains(port2));
			Assert(trip.Lookups.ScheduleDPortCodes.Contains(port3));
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

		public void TestMessageStatusList()
		{
			var trip = Factory.New<Trip>();
			var message = trip.Messages.AddNew(typeof(EDIMessage));
			message.EM_MessageType = MessageTypes.Codes.eManifest;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Sent;
			message.EM_MessageNum = "001";
			AssertEquals("Awaiting Complete e-Manifest w/ACE ID Change", trip.Lookups.MessageStatusList.GetDescriptionFromCode(MessageStatusList.Codes.AwaitingChange));
			message = trip.Messages.AddNew(typeof(EDIMessage));
			message.EM_MessageType = MessageTypes.Codes.CrewAndPassenger;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "002";
			message.EM_Status = EDIMessage.Status.Pending;
			AssertEquals("Awaiting Complete e-Manifest w/ACE ID Change", trip.Lookups.MessageStatusList.GetDescriptionFromCode(MessageStatusList.Codes.AwaitingChange));
			message.EM_Status = EDIMessage.Status.Queued;
			AssertEquals("Error Crew/Passengers Details Original", trip.Lookups.MessageStatusList.GetDescriptionFromCode(MessageStatusList.Codes.ErrorOriginal));
			message = trip.Messages.AddNew(typeof(EDIMessage));
			message.EM_MessageType = MessageTypes.Codes.CompleteTrip;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Discarded;
			message.EM_MessageNum = "003";
			AssertEquals("Error Complete Trip Details Original", trip.Lookups.MessageStatusList.GetDescriptionFromCode(MessageStatusList.Codes.ErrorOriginal));
			message.IsCancelled = true;
			AssertEquals("Error Crew/Passengers Details Original", trip.Lookups.MessageStatusList.GetDescriptionFromCode(MessageStatusList.Codes.ErrorOriginal));
		}

		public void TestReleaseStatusList()
		{
			var lookups = Factory.New<Trip>().Lookups;
			AssertEquals("Contains TripEntryStatusList", TripEntryStatusList.Descriptions.HoldTrip, lookups.ReleaseStatusList.GetDescriptionFromCode(TripEntryStatusList.Codes.HoldTrip));
			AssertEquals("Contains EntryStatusList", EntryStatusList.Descriptions.Error, lookups.ReleaseStatusList.GetDescriptionFromCode(EntryStatusList.Codes.Error));
			AssertEquals("Contains MessageTypes", MessageTypes.Descriptions.SyntaxError, lookups.ReleaseStatusList.GetDescriptionFromCode(MessageTypes.Codes.SyntaxError));
		}
	}
}
