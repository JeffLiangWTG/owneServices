using System;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Agency.DataTransfer.Test.LogSubscribers.BookingConfirmation
{
	[TestedType(typeof(AgencyBookingUpdatedLogSubscriber))]
	class AgencyBookingUpdatedLogSubscriberTest : LogSubscriberTest<AgencyBookingUpdatedLogSubscriber>
	{
		public void TestProcessLogs()
		{
			var ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USCHI";
				voyage.JV_OH_Line = Factory.NewWithValidTestData<OrgHeader>().PK;
				voyage.GenerateSailings();

				var agencyBooking = Factory.NewWithValidTestData<AgencyBooking>();
				agencyBooking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking.JS_JX = voyage.Sailings[0].PK;
				agencyBooking.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;
				Factory.Save();
				Assert(!agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				RunLogWalkerCycleForTest();
				AssertEquals(ediMessageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));

				agencyBooking.JS_BookingReference = "234";
				Factory.Save();
				Assert(!agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				RunLogWalkerCycleForTest();
				AssertEquals(ediMessageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));

				agencyBooking.JS_CFSReference = "123";
				Factory.Save();
				AssertEquals(1, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				RunLogWalkerCycleForTest();
				AssertUniversalShipmentSentToRecipient(agencyBooking, ediMessageCount + 1);

				agencyBooking.JS_OA_BookedShippingLineAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
				Factory.Save();
				AssertEquals(2, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				RunLogWalkerCycleForTest();
				AssertUniversalShipmentSentToRecipient(agencyBooking, ediMessageCount + 2);

				var principals = new AllowSendingBookingConfirmationCollection();
				var principal = principals.AddNew();
				principal.PrincipalPK = Guid.Empty;
				principal.Enabled = false;

				using (AgencyRegistry.Instance.AllowSendingBookingConfirmationEDIAfterATD.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, principals))
				{
					var mainSeaLeg = agencyBooking.Transports.Cast<Transport>()
					.FirstOrDefault(transport => transport.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel && transport.JW_TransportMode == Core.Constants.TransportModes.Sea);
					mainSeaLeg.JW_ATD = ZDateTime.Today;
					Factory.Save();
					AssertEquals(2, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
						.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

					RunLogWalkerCycleForTest();
					AssertUniversalShipmentSentToRecipient(agencyBooking, ediMessageCount + 2);
				}

				principals.RemoveAndDeleteAll();
				principal = principals.AddNew();
				principal.PrincipalPK = Guid.Empty;
				principal.Enabled = true;

				using (AgencyRegistry.Instance.AllowSendingBookingConfirmationEDIAfterATD.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, principals))
				{
					var mainSeaLeg = agencyBooking.Transports.Cast<Transport>()
					.FirstOrDefault(transport => transport.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel && transport.JW_TransportMode == Core.Constants.TransportModes.Sea);
					mainSeaLeg.JW_ATD = ZDateTime.Today;
					Factory.Save();
					AssertEquals(3, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
						.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

					RunLogWalkerCycleForTest();
					AssertUniversalShipmentSentToRecipient(agencyBooking, ediMessageCount + 3);
				}
			}
		}

		public void TestProcessLogs_RegistryIsDisabled()
		{
			var ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var agencyBooking = Factory.NewWithValidTestData<AgencyBooking>();
				agencyBooking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;
				Factory.Save();
				Assert(!agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				RunLogWalkerCycleForTest();
				AssertEquals(ediMessageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));

				agencyBooking.JS_CFSReference = "123";
				Factory.Save();
				AssertEquals(1, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			}

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				RunLogWalkerCycleForTest();
				AssertEquals(ediMessageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));
			}
		}

		public void TestProcessLogs_Branch()
		{
			var ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
			var originalCurrentBranchPK = GlbBranch.CurrentBranch.PK;

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USCHI";
				voyage.JV_OH_Line = Factory.NewWithValidTestData<OrgHeader>().PK;
				voyage.GenerateSailings();

				var agencyBooking = Factory.NewWithValidTestData<AgencyBooking>();
				agencyBooking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking.JS_JX = voyage.Sailings[0].PK;
				agencyBooking.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;
				Factory.Save();
				Assert(!agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				RunLogWalkerCycleForTest();
				AssertEquals(ediMessageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));

				var otherBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, originalCurrentBranchPK));
				using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser.LoginName, otherBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					agencyBooking.JS_CFSReference = "123";
					Factory.Save();
					AssertEquals(1, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
						.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				}

				RunLogWalkerCycleForTest();
				Thread.Sleep(10);

				agencyBooking.Logs.GetAllLogs().Reload(true);
				var mostRecentDEXLog = agencyBooking.Logs.GetAllLogs().OfType<StmALog>().OrderByDescending(x => x.SL_PostedTimeUtc).First(x => x.SL_SE_NKEvent == Events.DataExportCode);
				AssertEquals(otherBranch.GB_Code, mostRecentDEXLog.SL_GB_NKBranch);
				AssertEquals(ediMessageCount + 1, Factory.GetDatabaseCount(typeof(EDIMessage)));

				var otherBranch2 = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, new[] { originalCurrentBranchPK, otherBranch.PK }));
				otherBranch2.GB_IsActive = false;
				Factory.Save();

				using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser.LoginName, otherBranch2.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					agencyBooking.JS_CFSReference = "234";
					Factory.Save();
					AssertEquals(2, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
						.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				}

				RunLogWalkerCycleForTest();
				Thread.Sleep(10);

				agencyBooking.Logs.GetAllLogs().Reload(true);
				mostRecentDEXLog = agencyBooking.Logs.GetAllLogs().OfType<StmALog>().OrderByDescending(x => x.SL_PostedTimeUtc).First(x => x.SL_SE_NKEvent == Events.DataExportCode);
				AssertEquals(otherBranch2.GB_Code, mostRecentDEXLog.SL_GB_NKBranch);
				AssertEquals(ediMessageCount + 2, Factory.GetDatabaseCount(typeof(EDIMessage)));

				agencyBooking.JS_CFSReference = "111";
				Factory.Save();
				AssertEquals(3, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				RunLogWalkerCycleForTest();
				Thread.Sleep(10);

				agencyBooking.Logs.GetAllLogs().Reload(true);
				mostRecentDEXLog = agencyBooking.Logs.GetAllLogs().OfType<StmALog>().OrderByDescending(x => x.SL_PostedTimeUtc).First(x => x.SL_SE_NKEvent == Events.DataExportCode);
				AssertEquals(GlbBranch.CurrentBranch.GB_Code, mostRecentDEXLog.SL_GB_NKBranch);
				AssertEquals(ediMessageCount + 3, Factory.GetDatabaseCount(typeof(EDIMessage)));
			}
		}

		public void TestProcessLogs_MainSeaRoutingLegATD()
		{
			var ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var agencyBooking = Factory.NewWithValidTestData<AgencyBooking>();
				agencyBooking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

				agencyBooking.Transports.RemoveAll();
				agencyBooking.JS_JX = ZGuid.Empty;

				var transport1 = agencyBooking.Transports.AddNew();
				transport1.JW_RL_NKLoadPort = "AUBNE";
				transport1.JW_RL_NKDiscPort = "USCHI";
				transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
				transport1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
				transport1.JW_Vessel = "COSCO NEBULA";
				transport1.JW_VoyageFlight = "85475";
				transport1.JW_ETD = new ZDateTime(2024, 01, 24, 8, 45, 00);
				transport1.JW_ETA = new ZDateTime(2024, 01, 26, 12, 15, 00);

				Factory.Save();
				Assert(!agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				RunLogWalkerCycleForTest();
				AssertEquals(ediMessageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));

				agencyBooking.JS_CFSReference = "123";
				AssertLogAndUniversalShipment(agencyBooking, 1, ediMessageCount + 1);

				transport1.JW_ATD = new ZDateTime(2021, 01, 24, 8, 45, 00);
				AssertLogAndUniversalShipment(agencyBooking, 1, ediMessageCount + 1);

				#region Actual Departure Date (ATD) of MAI SEA Leg has been set

				agencyBooking.JS_CFSReference = "345";
				AssertLogAndUniversalShipment(agencyBooking, 1, ediMessageCount + 1);

				var voyage = Factory.New<JobVoyage>();
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "CNSHG";
				voyage.JV_OH_Line = Factory.NewWithValidTestData<OrgHeader>().PK;
				voyage.GenerateSailings();

				agencyBooking.JS_JX = voyage.Sailings[0].PK;
				AssertLogAndUniversalShipment(agencyBooking, 1, ediMessageCount + 1);

				agencyBooking.JS_OA_BookedShippingLineAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
				AssertLogAndUniversalShipment(agencyBooking, 1, ediMessageCount + 1);

				agencyBooking.JS_OH_DeliveryAgent = Factory.NewWithValidTestData<OrgAddress>().Header.PK;
				AssertLogAndUniversalShipment(agencyBooking, 1, ediMessageCount + 1);

				#region New transport leg
				var transport2 = agencyBooking.Transports.AddNew();
				transport2.JW_TransportMode = Core.Constants.TransportModes.Other;
				transport2.JW_TransportType = Core.Constants.TransportPlanningType.Other;
				AssertLogAndUniversalShipment(agencyBooking, 1, ediMessageCount + 1);

				transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
				AssertLogAndUniversalShipment(agencyBooking, 1, ediMessageCount + 1);

				transport2.JW_Vessel = "COSCO NEBULA2";
				AssertLogAndUniversalShipment(agencyBooking, 1, ediMessageCount + 1);

				transport2.JW_VoyageFlight = "42039";
				AssertLogAndUniversalShipment(agencyBooking, 1, ediMessageCount + 1);

				transport2.JW_RL_NKLoadPort = "USCHI";
				AssertLogAndUniversalShipment(agencyBooking, 1, ediMessageCount + 1);

				transport2.JW_RL_NKDiscPort = "CNSHG";
				AssertLogAndUniversalShipment(agencyBooking, 1, ediMessageCount + 1);

				transport2.JW_ETD = new ZDateTime(2024, 01, 27, 8, 45, 00);
				AssertLogAndUniversalShipment(agencyBooking, 1, ediMessageCount + 1);

				transport2.JW_ETA = new ZDateTime(2024, 01, 28, 12, 15, 00);
				AssertLogAndUniversalShipment(agencyBooking, 1, ediMessageCount + 1);

				transport2.JW_OA_CarrierAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
				AssertLogAndUniversalShipment(agencyBooking, 1, ediMessageCount + 1);
				#endregion

				#region Main Sea Leg Fields
				transport1.JW_TerminalReceivalCommences = new ZDateTime(2024, 01, 26, 12, 15, 00);
				AssertLogAndUniversalShipment(agencyBooking, 1, ediMessageCount + 1);

				transport1.JW_DepotReceivalCommences = new ZDateTime(2024, 01, 26, 12, 15, 00);
				AssertLogAndUniversalShipment(agencyBooking, 1, ediMessageCount + 1);

				transport1.JW_TerminalCutOff = new ZDateTime(2024, 01, 26, 12, 15, 00);
				AssertLogAndUniversalShipment(agencyBooking, 1, ediMessageCount + 1);

				transport1.JW_DepotCutOff = new ZDateTime(2024, 01, 26, 12, 15, 00);
				AssertLogAndUniversalShipment(agencyBooking, 1, ediMessageCount + 1);

				transport1.JW_DocumentaryCutOff = new ZDateTime(2024, 01, 26, 12, 15, 00);
				AssertLogAndUniversalShipment(agencyBooking, 1, ediMessageCount + 1);

				transport1.JW_VGMCutOff = new ZDateTime(2024, 01, 26, 12, 15, 00);
				AssertLogAndUniversalShipment(agencyBooking, 1, ediMessageCount + 1);
				#endregion

				#endregion
			}
		}

		void AssertLogAndUniversalShipment(AgencyBooking agencyBooking, int logCount, int ediMessageCount)
		{
			Factory.Save();
			AssertEquals(logCount, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
				.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

			RunLogWalkerCycleForTest();
			AssertEquals(ediMessageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));
		}

		void AssertUniversalShipmentSentToRecipient(AgencyBooking agencyBooking, int ediMessageCount)
		{
			AssertEquals(ediMessageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));

			var ediMessage = Factory.Load<EDIMessage>(new ZQuery()).OrderByDescending(x => ((BusinessObject)x)[EDIMessageSchema.EM_SystemCreateTimeUtc]).First();
			AssertNotNull(ediMessage);

			var interchange = Factory.Load<EDIInterchange>(ediMessage.EM_EI);
			AssertNotNull(interchange);
			AssertEquals("SHIPPING_INSTRUCTION", interchange.EI_To);

			var universalShipment = XDocument.Parse(ediMessage.EM_MessageText).Root;
			AssertNotNull(universalShipment);

			var ns = universalShipment?.GetDefaultNamespace();
			var dataContext = universalShipment?.Elements(ns + "Shipment").Elements(ns + "DataContext").Single();
			var shipmentDataSource = dataContext?.Elements(ns + "DataSource").Single();

			AssertNotNull(shipmentDataSource);
			AssertEquals(agencyBooking.JS_UniqueConsignRef, shipmentDataSource?.Elements(ns + "Key").Single().Value);
			AssertEquals("AgencyBooking", shipmentDataSource?.Elements(ns + "Type").Single().Value);

			var recipientRole = dataContext?.Elements(ns + "Workflow").Elements(ns + "RecipientRoleCollection").Elements().Single().Value;
			AssertEquals(nameof(RecipientRoleType.NVO), recipientRole);

			var purposeCodeInDoc = dataContext?.Elements(ns + "DocumentaryOverride").Elements(ns + "Purpose").Single().Value;
			AssertEquals(Events.MessageAcceptedCode, purposeCodeInDoc);

			AssertEquals("http://www.cargowise.com/Schemas/Universal/2012/11/BookingConfirmation/1", ns.ToString());
		}

		protected override bool IKnowEDTEventsAreUsuallyOnlyLoggedWhenAFormIsPresent => true;
	}
}
