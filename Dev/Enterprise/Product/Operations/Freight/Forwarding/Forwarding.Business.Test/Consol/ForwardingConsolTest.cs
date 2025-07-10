using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Macros;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Integration;
using Enterprise.ContractManagement.Business;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Freight.Business.HelperClasses;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business.AWB.Testing;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Messaging.Business;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;
using AWBSpecialHandlingCodeDescriptionPairList = Enterprise.Freight.Forwarding.AWB.Business.AWBSpecialHandlingCodeDescriptionPairList;
using CO2eBusinessTestHelper = Enterprise.Freight.CarbonEmissions.Business.Testing.CO2eTestHelper;
using CO2eTestHelper = Enterprise.Freight.DataTransfer.Universal.Testing.CO2eTestHelper;
using Constants = Enterprise.Core.Constants;
using EventConstants = CargoWise.EventReference.Constants;
using IAUCusMAWB = Enterprise.Integration.Customs.AU.ICusMAWB;
using IAUCusSCAOceanBill = Enterprise.Integration.Customs.AU.ICusSCAOceanBill;
using PrepaidCollectCodes = Enterprise.Accounting.Integration.PrepaidCollectFreightForwardingList.Codes;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	public class ForwardingConsolTest : CommonConsolTest2
	{
		#region TryLogWhileClearTimeOfTransports

		public void TestTryLogWhileClearTimeOfTransports()
		{
			var message = new BusinessObjectFactory().New<EDIMessage>();
			message.EM_MessageNum = "00000000000000000346";
			message.EM_MessageText = "<UniversalShipment>dummy text</UniversalShipment>";

			var loggerMock = new Mock<IXmlSessionTracker>();

			ErrorReporter.Clear();
			var consol = BuildConsol();
			var expectedOriginalTransportMessage = "Original Transports:\n" + getExpectedTransportMessageFromDB(consol.Transports[0]);
			var expectedOriginalSailingMessage = "Original Schedules:\n" + getExpectedSailingMessageFromDB(consol.Transports[0]);
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2012_11_DO_NOT_USE))
			{
				consol.Transports[0].JW_ETDForBinding = ZDateTime.Empty;
				var expectedCurrentTransportMessage = $"Current Transports:\n" + getExpectedTransportMessage(consol.Transports[0]);
				var expectedCurrentSailingMessage = "Current Schedules:\n" + getExpectedSailingMessage(consol.Transports[0]);

				resetMock();
				consol.TryLogWhileClearTimeOfTransports(loggerMock.Object);
				Factory.Save();

				loggerMock.Verify(logger => logger.Log(LogType.Information, "SGSIN->AUSYD:ETD is cleared"), Times.Once);
				var lastKeyReporter = ErrorReporter.LastKeyReported;
				var lastMessageReported = ErrorReporter.LastMessageReported;
				CombineAssertions(() =>
				{
					AssertEquals("TransportTimeUnexpectedlyClearedOut", lastKeyReporter);
					AssertContains("SGSIN->AUSYD:ETD is cleared:", lastMessageReported);
					AssertContains("UseDate2012_11NamespaceAndFormat = True", lastMessageReported);
					AssertContains("EM_MessageNum = 00000000000000000346", lastMessageReported);
					AssertContains("EM_MessageText = <UniversalShipment>dummy text</UniversalShipment>", lastMessageReported);
					AssertContains(expectedOriginalTransportMessage, lastMessageReported);
					AssertContains(expectedCurrentTransportMessage, lastMessageReported);
					AssertContains(expectedOriginalTransportMessage, lastMessageReported);
					AssertContains(expectedCurrentSailingMessage, lastMessageReported);
				});
			}

			ErrorReporter.Clear();
			consol = BuildConsol();
			expectedOriginalTransportMessage = "Original Transports:\n" + getExpectedTransportMessageFromDB(consol.Transports[0]);
			expectedOriginalSailingMessage = "Original Schedules:\n" + getExpectedSailingMessageFromDB(consol.Transports[0]);
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2012_11_DO_NOT_USE))
			{
				consol.Transports[0].JW_ETDForBinding = ZDateTime.Empty;
				consol.Transports[0].JW_ETAForBinding = ZDateTime.Empty;
				var expectedCurrentTransportMessage = $"Current Transports:\n" + getExpectedTransportMessage(consol.Transports[0]);
				var expectedCurrentSailingMessage = "Current Schedules:\n" + getExpectedSailingMessage(consol.Transports[0]);

				resetMock();
				consol.TryLogWhileClearTimeOfTransports(loggerMock.Object);
				Factory.Save();

				loggerMock.Verify(logger => logger.Log(LogType.Information, "SGSIN->AUSYD:ETD is cleared,ETA is cleared"), Times.Once);
				var lastKeyReporter = ErrorReporter.LastKeyReported;
				var lastMessageReported = ErrorReporter.LastMessageReported;
				CombineAssertions(() =>
				{
					AssertEquals("TransportTimeUnexpectedlyClearedOut", lastKeyReporter);
					AssertContains("SGSIN->AUSYD:ETD is cleared:", lastMessageReported);
					AssertContains("SGSIN->AUSYD:ETA is cleared:", lastMessageReported);
					AssertContains("UseDate2012_11NamespaceAndFormat = True", lastMessageReported);
					AssertContains("EM_MessageNum = 00000000000000000346", lastMessageReported);
					AssertContains("EM_MessageText = <UniversalShipment>dummy text</UniversalShipment>", lastMessageReported);
					AssertContains(expectedOriginalTransportMessage, lastMessageReported);
					AssertContains(expectedCurrentTransportMessage, lastMessageReported);
					AssertContains(expectedOriginalTransportMessage, lastMessageReported);
					AssertContains(expectedCurrentSailingMessage, lastMessageReported);
				});
			}

			ErrorReporter.Clear();
			consol = BuildConsol();
			AssertEquals(false, consol.Logs.GetAllLogs().OfType<StmALog>().Any(log => log.SL_SE_NKEvent == "EDT" && log.SL_Reference == "SGSIN->AUSYD:ETD is cleared,ETA is cleared"));
			{
				consol.Transports[0].JW_TransportMode = TransportModes.Sea;
				consol.Transports[0].JW_ETDForBinding = ZDateTime.Empty;
				consol.Transports[0].JW_ETAForBinding = ZDateTime.Empty;

				resetMock();
				consol.TryLogWhileClearTimeOfTransports(loggerMock.Object);
				Factory.Save();

				loggerMock.Verify(logger => logger.Log(LogType.Information, It.IsAny<string>()), Times.Never);
				AssertNullOrEmpty(ErrorReporter.LastKeyReported);
			}

			#region Implementation

			ForwardingConsol BuildConsol()
			{
				var resultConsol = Factory.NewWithValidTestData<ForwardingConsol>();

				resultConsol.JK_TransportMode = TransportModes.Air;
				resultConsol.JK_RL_NKLoadPort = "SGSIN";
				resultConsol.JK_RL_NKDischargePort = "AUSYD";
				resultConsol.Transports[0].JW_ETDForBinding = ZDateTime.Now.AddDays(-2);
				resultConsol.Transports[0].JW_ETAForBinding = ZDateTime.Now.AddDays(-1);

				var sailing = CreateJobSailing("QF001", "SGSIN", "AUSYD", ZDateTime.Now.AddDays(-2), ZDateTime.Now.AddDays(-1));
				resultConsol.Transports[0].JW_IsLinked = true;
				resultConsol.Transports[0].JW_JX = sailing.PK;

				resetMock();
				resultConsol.TryLogWhileClearTimeOfTransports(loggerMock.Object);
				loggerMock.Verify(lm => lm.Log(LogType.Information, It.IsAny<string>()), Times.Never);

				Factory.Save();

				return resultConsol;
			}

			void resetMock()
			{
				loggerMock.Reset();
				loggerMock.Setup(mockObject => mockObject.Log(LogType.Information, It.IsAny<string>()));
				loggerMock.SetupGet(logger => logger.SourceMessage).Returns(() => message);
			}

			string getExpectedTransportMessageFromDB(Transport transport)
			{
				var transportSaved = new ReadOnlyBusinessObjectFactory().Load<Transport>(transport.PK);
				return getExpectedTransportMessage(transportSaved);
			}

			string getExpectedTransportMessage(Transport transport)
			{
				return $"Transport PK = {transport.PK}, JW_RL_NKLoadPort = {transport.JW_RL_NKLoadPort}, JW_RL_NKDiscPort = {transport.JW_RL_NKDiscPort}, JW_ETD = {transport.JW_ETD}, JW_ETA = {transport.JW_ETA}, JW_ATD = , JW_ATA = , JW_JX = {transport.JW_JX}, JW_IsLinked = {transport.JW_IsLinked}, JW_Vessel = , JW_VoyageFlight = {transport.JW_VoyageFlight}";
			}

			string getExpectedSailingMessageFromDB(Transport transport)
			{
				var sailingSaved = new ReadOnlyBusinessObjectFactory().Load<JobSailing>(transport.JW_JX);
				return getExpectedSailingMessageCore(sailingSaved);
			}

			string getExpectedSailingMessage(Transport transport)
			{
				if (transport.JW_JX.IsEmpty)
				{
					return string.Empty;
				}

				var sailing = transport.Factory.Load<JobSailing>(transport.JW_JX);
				return getExpectedSailingMessageCore(sailing);
			}

			string getExpectedSailingMessageCore(JobSailing sailing)
			{
				return $"Sailing PK = {sailing.PK}, JX_JA_RL_NKPortOfLoading = {sailing.JX_JA_RL_NKPortOfLoading}, JX_JB_RL_NKPortOfDischarge = {sailing.JX_JB_RL_NKPortOfDischarge}, JX_JA_E_DEP = {sailing.JX_JA_E_DEP}, JX_JA_A_DEP = {sailing.JX_JA_A_DEP}, JX_JA_S_DEP = {sailing.JX_JA_S_DEP}, JX_JB_E_ARV = {sailing.JX_JB_E_ARV}, JX_JB_A_ARV = {sailing.JX_JB_A_ARV}, JX_JB_S_ARV= {sailing.JX_JB_S_ARV}, JX_JV_NKVessel = {sailing.JX_JV_NKVessel}, JX_JV_VoyageFlight = {sailing.JX_JV_VoyageFlight}";
			}

			#endregion
		}

		#endregion

		public void TestDefaultCarrierBookingOffice()
		{
			var org1 = Factory.NewWithValidTestData<OrgAddress>();
			org1.Header.OH_RL_NKClosestPort = "CNBJS";
			org1.OA_RL_NKRelatedPortCode = "AUSYD";

			var org2 = Factory.NewWithValidTestData<OrgAddress>();
			org2.Header.OH_RL_NKClosestPort = "AUSYD";
			org2.OA_RL_NKRelatedPortCode = "CNBJS";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_OA_ShippingLineAddress = org2.PK;
			consol.JK_OA_CreditorAddress = org1.PK;

			AssertEquals("AUSYD", consol.JK_RL_NKCarrierBookingOffice);

			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_OA_CreditorAddress = org1.PK;

			AssertEquals("CNBJS", consol.JK_RL_NKCarrierBookingOffice);

			org2.OA_RL_NKRelatedPortCode = "";
			consol.JK_OA_CreditorAddress = org2.PK;

			AssertNullOrEmpty(consol.JK_RL_NKCarrierBookingOffice);

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			AssertNullOrEmpty(consol.JK_RL_NKCarrierBookingOffice);
		}

		#region DefaultJK_PackageGrouping

		public void TestDefaultJK_PackageGrouping_TransportMode()
		{
			var org = Factory.NewWithValidTestData<OrgAddress>();
			org.Header.OH_RL_NKClosestPort = "CNBJS";
			org.Header.MiscServ.OM_FWAgentPackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;
			org.Header.MiscServ.OM_CarrierPackageGrouping = Constants.PackageGrouping.Codes.GroupByPackLine;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			AssertEquals("Default Value", Constants.PackageGrouping.Codes.DoNotGroup, consol.JK_PackageGrouping);

			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = org.PK;
			consol.JK_OA_ShippingLineAddress = org.PK;

			foreach (ICodeDescription transportMode in consol.JK_TransportMode_List)
			{
				if (transportMode.Code != Core.Constants.TransportModes.Sea)
				{
					consol.JK_TransportMode = transportMode.Code;
					AssertEquals("JK_PackageGrouping is only for SEA", Constants.PackageGrouping.Codes.DoNotGroup, consol.JK_PackageGrouping);
				}
			}

			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Take the value from OM_CarrierPackageGrouping", Constants.PackageGrouping.Codes.GroupByPackLine, consol.JK_PackageGrouping);
		}

		public void TestDefaultJK_PackageGrouping_JK_OA_SendingForwarderAddress()
		{
			var org1 = Factory.NewWithValidTestData<OrgAddress>();
			org1.Header.OH_RL_NKClosestPort = "CNBJS";
			org1.Header.MiscServ.OM_FWAgentPackageGrouping = Constants.PackageGrouping.Codes.DefaultFromCarrier;
			org1.Header.MiscServ.OM_CarrierPackageGrouping = Constants.PackageGrouping.Codes.GroupByPackLine;

			var org2 = Factory.NewWithValidTestData<OrgAddress>();
			org2.Header.OH_RL_NKClosestPort = "AUSYD";
			org2.Header.MiscServ.OM_FWAgentPackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;
			org2.Header.MiscServ.OM_CarrierPackageGrouping = Constants.PackageGrouping.Codes.GroupByPackLine;

			var org3 = Factory.NewWithValidTestData<OrgAddress>();
			org3.Header.OH_RL_NKClosestPort = "CNNJA";
			org3.Header.MiscServ.OM_FWAgentPackageGrouping = Constants.PackageGrouping.Codes.GroupByPackLine;
			org3.Header.MiscServ.OM_CarrierPackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			AssertEquals("Default Value", Constants.PackageGrouping.Codes.DoNotGroup, consol.JK_PackageGrouping);

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_OA_CreditorAddress = org2.PK;
			consol.JK_OA_ShippingLineAddress = org3.PK;

			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			AssertEquals("CoLoad Consol without Sending Agency. Take the value from Creditor's OM_CarrierPackageGrouping", Constants.PackageGrouping.Codes.GroupByPackLine, consol.JK_PackageGrouping);

			consol.JK_OA_SendingForwarderAddress = org2.PK;
			AssertEquals("CoLoad Consol. Take the value from Sending Agency's OM_FWAgentPackageGrouping", Constants.PackageGrouping.Codes.GroupByShipment, consol.JK_PackageGrouping);

			consol.JK_OA_SendingForwarderAddress = org3.PK;
			AssertEquals("CoLoad Consol. Take the value from Sending Agency's OM_FWAgentPackageGrouping", Constants.PackageGrouping.Codes.GroupByPackLine, consol.JK_PackageGrouping);

			consol.JK_OA_SendingForwarderAddress = org1.PK;
			AssertEquals("CoLoad Consol but Sending Agency's OM_FWAgentPackageGrouping is CAR. Take the value from Creditor's OM_CarrierPackageGrouping", Constants.PackageGrouping.Codes.GroupByPackLine, consol.JK_PackageGrouping);

			foreach (ICodeDescription agentType in consol.JK_AgentType_List)
			{
				if (agentType.Code != Constants.AgentType.CoLoad)
				{
					consol.JK_AgentType = agentType.Code;
					consol.JK_OA_CreditorAddress = org2.PK;
					consol.JK_OA_ShippingLineAddress = org3.PK;

					consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
					AssertEquals("Non-CoLoad Consol without Sending Agency. Take the value from Shipping Line's OM_CarrierPackageGrouping", Constants.PackageGrouping.Codes.GroupByShipment, consol.JK_PackageGrouping);

					consol.JK_OA_SendingForwarderAddress = org2.PK;
					AssertEquals("Non-CoLoad Consol. Take the value from Sending Agency's OM_FWAgentPackageGrouping", Constants.PackageGrouping.Codes.GroupByShipment, consol.JK_PackageGrouping);

					consol.JK_OA_SendingForwarderAddress = org3.PK;
					AssertEquals("Non-CoLoad Consol. Take the value from Sending Agency's OM_FWAgentPackageGrouping", Constants.PackageGrouping.Codes.GroupByPackLine, consol.JK_PackageGrouping);

					consol.JK_OA_SendingForwarderAddress = org1.PK;
					AssertEquals("Non-CoLoad Consol. Sending Agency's OM_FWAgentPackageGrouping is CAR. Take the value from Shipping Line's OM_CarrierPackageGrouping", Constants.PackageGrouping.Codes.GroupByShipment, consol.JK_PackageGrouping);
				}
			}
		}

		public void TestDefaultJK_PackageGrouping_JK_OA_CreditorAddress()
		{
			var org1 = Factory.NewWithValidTestData<OrgAddress>();
			org1.Header.OH_RL_NKClosestPort = "CNBJS";
			org1.Header.MiscServ.OM_FWAgentPackageGrouping = Constants.PackageGrouping.Codes.DefaultFromCarrier;
			org1.Header.MiscServ.OM_CarrierPackageGrouping = Constants.PackageGrouping.Codes.GroupByPackLine;

			var org2 = Factory.NewWithValidTestData<OrgAddress>();
			org2.Header.OH_RL_NKClosestPort = "AUSYD";
			org2.Header.MiscServ.OM_FWAgentPackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;
			org2.Header.MiscServ.OM_CarrierPackageGrouping = Constants.PackageGrouping.Codes.GroupByPackLine;

			var org3 = Factory.NewWithValidTestData<OrgAddress>();
			org3.Header.OH_RL_NKClosestPort = "CNNJA";
			org3.Header.MiscServ.OM_FWAgentPackageGrouping = Constants.PackageGrouping.Codes.GroupByPackLine;
			org3.Header.MiscServ.OM_CarrierPackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			AssertEquals("Default Value", Constants.PackageGrouping.Codes.DoNotGroup, consol.JK_PackageGrouping);

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;

			consol.JK_OA_ShippingLineAddress = org3.PK;
			consol.JK_OA_CreditorAddress = org2.PK;
			AssertEquals("CoLoad Consol without Sending Agency. Take the value from Creditor's OM_CarrierPackageGrouping", Constants.PackageGrouping.Codes.GroupByPackLine, consol.JK_PackageGrouping);

			consol.JK_OA_ShippingLineAddress = org2.PK;
			consol.JK_OA_CreditorAddress = org3.PK;
			AssertEquals("CoLoad Consol without Sending Agency. Take the value from Creditor's OM_CarrierPackageGrouping", Constants.PackageGrouping.Codes.GroupByShipment, consol.JK_PackageGrouping);

			consol.JK_OA_ShippingLineAddress = org3.PK;
			consol.JK_OA_CreditorAddress = org1.PK;
			AssertEquals("CoLoad Consol without Sending Agency. Take the value from Creditor's OM_CarrierPackageGrouping", Constants.PackageGrouping.Codes.GroupByPackLine, consol.JK_PackageGrouping);

			consol.JK_OA_SendingForwarderAddress = org1.PK;

			consol.JK_OA_ShippingLineAddress = org3.PK;
			consol.JK_OA_CreditorAddress = org2.PK;
			AssertEquals("CoLoad Consol. Sending Agency's OM_FWAgentPackageGrouping is CAR. Take the value from Creditor's OM_CarrierPackageGrouping", Constants.PackageGrouping.Codes.GroupByPackLine, consol.JK_PackageGrouping);

			consol.JK_OA_ShippingLineAddress = org2.PK;
			consol.JK_OA_CreditorAddress = org3.PK;
			AssertEquals("CoLoad Consol. Sending Agency's OM_FWAgentPackageGrouping is CAR. Take the value from Creditor's OM_CarrierPackageGrouping", Constants.PackageGrouping.Codes.GroupByShipment, consol.JK_PackageGrouping);

			consol.JK_OA_ShippingLineAddress = org3.PK;
			consol.JK_OA_CreditorAddress = org1.PK;
			AssertEquals("CoLoad Consol. Sending Agency's OM_FWAgentPackageGrouping is CAR. Take the value from Creditor's OM_CarrierPackageGrouping", Constants.PackageGrouping.Codes.GroupByPackLine, consol.JK_PackageGrouping);

			foreach (ICodeDescription agentType in consol.JK_AgentType_List)
			{
				if (agentType.Code != Constants.AgentType.CoLoad)
				{
					consol.JK_AgentType = agentType.Code;
					consol.JK_OA_ShippingLineAddress = ZGuid.Empty;

					consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;

					consol.JK_OA_CreditorAddress = org1.PK;
					AssertEquals("Non-CoLoad Consol without Sending Agency and Shipping Line.", Constants.PackageGrouping.Codes.DoNotGroup, consol.JK_PackageGrouping);

					consol.JK_OA_CreditorAddress = org2.PK;
					AssertEquals("Non-CoLoad Consol without Sending Agency and Shipping Line.", Constants.PackageGrouping.Codes.DoNotGroup, consol.JK_PackageGrouping);

					consol.JK_OA_CreditorAddress = org3.PK;
					AssertEquals("Non-CoLoad Consol without Sending Agency and Shipping Line.", Constants.PackageGrouping.Codes.DoNotGroup, consol.JK_PackageGrouping);

					consol.JK_OA_SendingForwarderAddress = org1.PK;

					consol.JK_OA_CreditorAddress = org1.PK;
					AssertEquals("Non-CoLoad Consol without Shipping Line. Sending Agency's OM_FWAgentPackageGrouping is CAR.", Constants.PackageGrouping.Codes.DoNotGroup, consol.JK_PackageGrouping);

					consol.JK_OA_CreditorAddress = org2.PK;
					AssertEquals("Non-CoLoad Consol without Shipping Line. Sending Agency's OM_FWAgentPackageGrouping is CAR.", Constants.PackageGrouping.Codes.DoNotGroup, consol.JK_PackageGrouping);

					consol.JK_OA_CreditorAddress = org3.PK;
					AssertEquals("Non-CoLoad Consol without Shipping Line. Sending Agency's OM_FWAgentPackageGrouping is CAR.", Constants.PackageGrouping.Codes.DoNotGroup, consol.JK_PackageGrouping);
				}
			}
		}
		public void TestDefaultJK_PackageGrouping_JK_OA_ShippingLineAddress()
		{
			var org1 = Factory.NewWithValidTestData<OrgAddress>();
			org1.Header.OH_RL_NKClosestPort = "CNBJS";
			org1.Header.MiscServ.OM_FWAgentPackageGrouping = Constants.PackageGrouping.Codes.DefaultFromCarrier;
			org1.Header.MiscServ.OM_CarrierPackageGrouping = Constants.PackageGrouping.Codes.GroupByPackLine;

			var org2 = Factory.NewWithValidTestData<OrgAddress>();
			org2.Header.OH_RL_NKClosestPort = "AUSYD";
			org2.Header.MiscServ.OM_FWAgentPackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;
			org2.Header.MiscServ.OM_CarrierPackageGrouping = Constants.PackageGrouping.Codes.GroupByPackLine;

			var org3 = Factory.NewWithValidTestData<OrgAddress>();
			org3.Header.OH_RL_NKClosestPort = "CNNJA";
			org3.Header.MiscServ.OM_FWAgentPackageGrouping = Constants.PackageGrouping.Codes.GroupByPackLine;
			org3.Header.MiscServ.OM_CarrierPackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			AssertEquals("Default Value", Constants.PackageGrouping.Codes.DoNotGroup, consol.JK_PackageGrouping);

			foreach (ICodeDescription agentType in consol.JK_AgentType_List)
			{
				if (agentType.Code != Constants.AgentType.CoLoad)
				{
					consol.JK_AgentType = agentType.Code;
					consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;

					consol.JK_OA_CreditorAddress = org2.PK;
					consol.JK_OA_ShippingLineAddress = org3.PK;
					AssertEquals("Non-CoLoad Consol without Sending Agency. Take the value from Shipping Line's OM_CarrierPackageGrouping", Constants.PackageGrouping.Codes.GroupByShipment, consol.JK_PackageGrouping);

					consol.JK_OA_CreditorAddress = org3.PK;
					consol.JK_OA_ShippingLineAddress = org2.PK;
					AssertEquals("Non-CoLoad Consol without Sending Agency. Take the value from Shipping Line's OM_CarrierPackageGrouping", Constants.PackageGrouping.Codes.GroupByPackLine, consol.JK_PackageGrouping);

					consol.JK_OA_CreditorAddress = org3.PK;
					consol.JK_OA_ShippingLineAddress = org1.PK;
					AssertEquals("Non-CoLoad Consol without Sending Agency. Take the value from Shipping Line's OM_CarrierPackageGrouping", Constants.PackageGrouping.Codes.GroupByPackLine, consol.JK_PackageGrouping);

					consol.JK_OA_SendingForwarderAddress = org1.PK;

					consol.JK_OA_CreditorAddress = org2.PK;
					consol.JK_OA_ShippingLineAddress = org3.PK;
					AssertEquals("Non-CoLoad Consol. Sending Agency's OM_FWAgentPackageGrouping is CAR. Take the value from Shipping Line's OM_CarrierPackageGrouping", Constants.PackageGrouping.Codes.GroupByShipment, consol.JK_PackageGrouping);

					consol.JK_OA_CreditorAddress = org3.PK;
					consol.JK_OA_ShippingLineAddress = org2.PK;
					AssertEquals("Non-CoLoad Consol. Sending Agency's OM_FWAgentPackageGrouping is CAR. Take the value from Shipping Line's OM_CarrierPackageGrouping", Constants.PackageGrouping.Codes.GroupByPackLine, consol.JK_PackageGrouping);

					consol.JK_OA_CreditorAddress = org3.PK;
					consol.JK_OA_ShippingLineAddress = org1.PK;
					AssertEquals("Non-CoLoad Consol. Sending Agency's OM_FWAgentPackageGrouping is CAR. Take the value from Shipping Line's OM_CarrierPackageGrouping", Constants.PackageGrouping.Codes.GroupByPackLine, consol.JK_PackageGrouping);
				}
			}

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_OA_CreditorAddress = ZGuid.Empty;

			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;

			consol.JK_OA_ShippingLineAddress = org1.PK;
			AssertEquals("CoLoad Consol without Sending Agency and Creditor.", Constants.PackageGrouping.Codes.DoNotGroup, consol.JK_PackageGrouping);

			consol.JK_OA_ShippingLineAddress = org2.PK;
			AssertEquals("CoLoad Consol without Sending Agency and Creditor.", Constants.PackageGrouping.Codes.DoNotGroup, consol.JK_PackageGrouping);

			consol.JK_OA_ShippingLineAddress = org3.PK;
			AssertEquals("CoLoad Consol without Sending Agency and Creditor.", Constants.PackageGrouping.Codes.DoNotGroup, consol.JK_PackageGrouping);

			consol.JK_OA_SendingForwarderAddress = org1.PK;

			consol.JK_OA_ShippingLineAddress = org1.PK;
			AssertEquals("CoLoad Consol without Creditor. Sending Agency's OM_FWAgentPackageGrouping is CAR.", Constants.PackageGrouping.Codes.DoNotGroup, consol.JK_PackageGrouping);

			consol.JK_OA_ShippingLineAddress = org2.PK;
			AssertEquals("CoLoad Consol without Creditor. Sending Agency's OM_FWAgentPackageGrouping is CAR.", Constants.PackageGrouping.Codes.DoNotGroup, consol.JK_PackageGrouping);

			consol.JK_OA_ShippingLineAddress = org3.PK;
			AssertEquals("CoLoad Consol without Creditor. Sending Agency's OM_FWAgentPackageGrouping is CAR.", Constants.PackageGrouping.Codes.DoNotGroup, consol.JK_PackageGrouping);
		}

		public void TestJK_DepartureForFirstTransportInfoDoesNotThrowException()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			// This removes consol.Transports
			consol.Delete();

			PropertyInfoShouldNotThrowifTransportsNullOrEmpty(
				(ZWrappedPropertyInfo)consol.JK_DepartureForFirstTransportInfo);
		}

		public void TestJK_ATAForLastTransportInfoDoesNotThrowIfTransportsNullOrEmpty()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			// This removes consol.Transports
			consol.Delete();

			PropertyInfoShouldNotThrowifTransportsNullOrEmpty((ZWrappedPropertyInfo)consol.JK_ATAForLastTransportInfo);
		}

		void PropertyInfoShouldNotThrowifTransportsNullOrEmpty(ZWrappedPropertyInfo propertyInfo)
		{
			AssertNotEquals(propertyInfo, null);

			// This triggers the exception before the fix.
			var innerInfo = propertyInfo.InnerInfo;
			AssertNotEquals(innerInfo, null);
		}

		#endregion

		public void TestSetDomainContext()
		{
			AssertEquals("domain context has not been set", FreightDomainContext.Unspecified, Factory.GetFreightDomainContext());

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			AssertEquals("domain context has been set", FreightDomainContext.Forwarding, Factory.GetFreightDomainContext());
		}

		public void TestJK_Calc_AirBookingStatus()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "AUSYD";

			AssertEquals(Constants.TransportStatus.Planned, consol.JK_Calc_AirBookingStatus);

			foreach (var statusCode in consol.Transports[0].JW_Status_List.GetAllCodes())
			{
				consol.Transports[0].JW_Status = statusCode;
				AssertEquals(statusCode, consol.JK_Calc_AirBookingStatus);
			}

			var lastLeg = consol.Transports.AddNew();
			lastLeg.JW_TransportMode = Core.Constants.TransportModes.Air;
			lastLeg.JW_Status = Constants.TransportStatus.Planned;
			consol.Transports[0].JW_Status = Constants.TransportStatus.Queued;
			AssertEquals("QUE-PLN", consol.JK_Calc_AirBookingStatus);
		}

		public void TestJK_Calc_CarrierBookingLatestStatus()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var documentData = Factory.NewWithValidTestData<VisualizerDocumentData>();
			documentData[JobDocumentDataSchema.JDD_ParentTableCode] = consol.TablePrefix;
			documentData[JobDocumentDataSchema.JDD_ParentID] = consol.PK;
			documentData[JobDocumentDataSchema.JDD_Name] = ConsolDocumentDataStoreNames.SeaBookingRequest2;

			AssertEquals(FreightConstants.CarrierBookingStatus.Codes.NotSent, consol.JK_Calc_CarrierBookingLatestStatus);

			KeyValuePair<string, string>[] shippingInstructionEventParameter = new[]
			{
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.MessageType, ConsolDocumentNames.ShippingInstruction)
			};
			KeyValuePair<string, string>[] bookingRequestEventParameter = new[]
			{
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.MessageType, ConsolDocumentNames.BookingRequest)
			};

			AddLog(documentData, Events.InterchangeSent, new ZDateTimeOffset(2021, 12, 09, 0, 0, 1), shippingInstructionEventParameter);
			AssertEquals(FreightConstants.CarrierBookingStatus.Codes.NotSent, consol.JK_Calc_CarrierBookingLatestStatus);

			AddLog(documentData, Events.MessageSent, new ZDateTimeOffset(2021, 12, 10, 0, 0, 1), shippingInstructionEventParameter);
			AssertEquals(FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.Sent, consol.JK_Calc_CarrierBookingLatestStatus);

			AddLog(documentData, Events.InterchangeSent, new ZDateTimeOffset(2021, 12, 10, 0, 0, 2), shippingInstructionEventParameter);
			AssertEquals(FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.Sent, consol.JK_Calc_CarrierBookingLatestStatus);

			AddLog(documentData, Events.MessageSent, new ZDateTimeOffset(2021, 12, 10, 0, 0, 2), shippingInstructionEventParameter);
			AssertEquals(FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.Sent, consol.JK_Calc_CarrierBookingLatestStatus);

			AddLog(documentData, Events.InterchangeSent, new ZDateTimeOffset(2021, 12, 10, 0, 0, 3), shippingInstructionEventParameter);
			AssertEquals(FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.Sent, consol.JK_Calc_CarrierBookingLatestStatus);

			AddLog(documentData, Events.MessageAccepted, new ZDateTimeOffset(2021, 12, 10, 0, 0, 3), shippingInstructionEventParameter);
			AssertEquals(FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.Confirmed, consol.JK_Calc_CarrierBookingLatestStatus);

			AddLog(documentData, Events.MessagePendingProcessing, new ZDateTimeOffset(2021, 12, 10, 0, 0, 4), shippingInstructionEventParameter);
			AssertEquals(FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.PendingProcessing, consol.JK_Calc_CarrierBookingLatestStatus);

			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();

			var documentData2 = Factory.NewWithValidTestData<VisualizerDocumentData>();
			documentData2[JobDocumentDataSchema.JDD_ParentTableCode] = consol2.TablePrefix;
			documentData2[JobDocumentDataSchema.JDD_ParentID] = consol2.PK;
			documentData2[JobDocumentDataSchema.JDD_Name] = ConsolDocumentDataStoreNames.SeaBookingRequest2;

			AddLog(documentData2, Events.InterchangeSent, new ZDateTimeOffset(2021, 12, 08), bookingRequestEventParameter);
			AssertEquals(FreightConstants.CarrierBookingStatus.Codes.NotSent, consol2.JK_Calc_CarrierBookingLatestStatus);

			AddLog(documentData2, Events.InterchangeSent, new ZDateTimeOffset(2021, 12, 09), bookingRequestEventParameter);
			AssertEquals(FreightConstants.CarrierBookingStatus.Codes.NotSent, consol2.JK_Calc_CarrierBookingLatestStatus);

			AddLog(documentData2, Events.MessageSent, new ZDateTimeOffset(2021, 12, 10), bookingRequestEventParameter);
			AssertEquals(FreightConstants.CarrierBookingStatus.Codes.BookingRequest.Sent, consol2.JK_Calc_CarrierBookingLatestStatus);

			AddLog(documentData2, Events.InterchangeSent, new ZDateTimeOffset(2021, 12, 10), bookingRequestEventParameter);
			AssertEquals(FreightConstants.CarrierBookingStatus.Codes.BookingRequest.Sent, consol2.JK_Calc_CarrierBookingLatestStatus);

			AddLog(documentData2, Events.MessageRejected, new ZDateTimeOffset(2021, 12, 11), bookingRequestEventParameter);
			AssertEquals(FreightConstants.CarrierBookingStatus.Codes.BookingRequest.Rejected, consol2.JK_Calc_CarrierBookingLatestStatus);

			AddLog(documentData2, Events.MessageWithdrawCancelRequest, new ZDateTimeOffset(2021, 12, 12), bookingRequestEventParameter);
			AssertEquals(FreightConstants.CarrierBookingStatus.Codes.BookingRequest.WithdrawalSent, consol2.JK_Calc_CarrierBookingLatestStatus);

			AddLog(documentData2, Events.MessageRejected, new ZDateTimeOffset(2021, 12, 13), bookingRequestEventParameter);
			AssertEquals(FreightConstants.CarrierBookingStatus.Codes.BookingRequest.WithdrawalRejected, consol2.JK_Calc_CarrierBookingLatestStatus);

			AddLog(documentData2, Events.MessagePendingProcessing, new ZDateTimeOffset(2021, 12, 14), bookingRequestEventParameter);
			AssertEquals(FreightConstants.CarrierBookingStatus.Codes.BookingRequest.PendingProcessing, consol2.JK_Calc_CarrierBookingLatestStatus);

			var consol3 = Factory.NewWithValidTestData<ForwardingConsol>();

			var documentData3 = Factory.NewWithValidTestData<VisualizerDocumentData>();
			documentData3[JobDocumentDataSchema.JDD_ParentTableCode] = consol3.TablePrefix;
			documentData3[JobDocumentDataSchema.JDD_ParentID] = consol3.PK;
			documentData3[JobDocumentDataSchema.JDD_Name] = ConsolDocumentDataStoreNames.SeaBookingRequest2;

			AddLog(documentData3, Events.MessageSent, new ZDateTimeOffset(2022, 03, 20), bookingRequestEventParameter);
			AssertEquals(FreightConstants.CarrierBookingStatus.Codes.BookingRequest.Sent, consol3.JK_Calc_CarrierBookingLatestStatus);

			AddLog(documentData3, Events.StatusUpdated, new ZDateTimeOffset(2022, 03, 21), bookingRequestEventParameter);
			AssertEquals(FreightConstants.CarrierBookingStatus.Codes.NotSent, consol3.JK_Calc_CarrierBookingLatestStatus);

			var log = documentData3.Logs.MostRecentLogByPostedTime(Events.StatusUpdated);
			log.IsCancelled = true;
			AssertEquals(FreightConstants.CarrierBookingStatus.Codes.BookingRequest.Sent, consol3.JK_Calc_CarrierBookingLatestStatus);

			var consol4 = Factory.NewWithValidTestData<ForwardingConsol>();
			AssertEquals(FreightConstants.CarrierBookingStatus.Codes.NotSent, consol4.JK_Calc_CarrierBookingLatestStatus);
		}

		public void TestJK_Calc_CarrierBookingLatestDate()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var documentData = Factory.NewWithValidTestData<VisualizerDocumentData>();
			documentData[JobDocumentDataSchema.JDD_ParentTableCode] = consol.TablePrefix;
			documentData[JobDocumentDataSchema.JDD_ParentID] = consol.PK;
			documentData[JobDocumentDataSchema.JDD_Name] = ConsolDocumentDataStoreNames.SeaBookingRequest2;

			AssertNullOrEmpty(consol.JK_Calc_CarrierBookingLatestDate.ToString());

			KeyValuePair<string, string>[] shippingInstructionEventParameter = new[]
			{
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.MessageType, ConsolDocumentNames.ShippingInstruction)
			};

			var baseZDateTimeOffset = new ZDateTimeOffset(2021, 12, 10, 0, 0, 1);

			AddLog(documentData, Events.InterchangeSent, baseZDateTimeOffset, shippingInstructionEventParameter);
			AssertEquals(ZDateTime.Empty, consol.JK_Calc_CarrierBookingLatestDate);

			AddLog(documentData, Events.InterchangeSent, baseZDateTimeOffset, shippingInstructionEventParameter);
			AssertEquals(ZDateTime.Empty, consol.JK_Calc_CarrierBookingLatestDate);

			AddLog(documentData, Events.MessageSent, baseZDateTimeOffset, shippingInstructionEventParameter);
			var postedTime = documentData.Logs.MostRecentLogByPostedDate.SL_PostedTimeUtc;
			AssertEquals(postedTime, consol.JK_Calc_CarrierBookingLatestDate);

			AddLog(documentData, Events.MessageSent, baseZDateTimeOffset.AddDays(1), shippingInstructionEventParameter);
			postedTime = documentData.Logs.MostRecentLogByPostedDate.SL_PostedTimeUtc;
			AssertEquals(postedTime, consol.JK_Calc_CarrierBookingLatestDate);

			AddLog(documentData, Events.InterchangeSent, baseZDateTimeOffset.AddDays(2), shippingInstructionEventParameter);
			AssertEquals(postedTime, consol.JK_Calc_CarrierBookingLatestDate);

			AddLog(documentData, Events.MessageAccepted, baseZDateTimeOffset.AddDays(2), shippingInstructionEventParameter);
			postedTime = documentData.Logs.MostRecentLogByPostedDate.SL_PostedTimeUtc;
			AssertEquals(postedTime, consol.JK_Calc_CarrierBookingLatestDate);

			AddLog(documentData, Events.StatusUpdated, baseZDateTimeOffset.AddDays(3), shippingInstructionEventParameter);
			AssertEquals(ZDateTime.Empty, consol.JK_Calc_CarrierBookingLatestDate);

			var log = documentData.Logs.MostRecentLogByPostedTime(Events.StatusUpdated);
			log.IsCancelled = true;
			AssertEquals(postedTime, consol.JK_Calc_CarrierBookingLatestDate);

			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			AssertEquals(ZDateTime.Empty, consol2.JK_Calc_CarrierBookingLatestDate);
		}

		void AddLog(VisualizerDocumentData documentData, Event @event, ZDateTimeOffset dateTimeOffset, KeyValuePair<string, string>[] eventParameters)
		{
			Thread.Sleep(500);
			documentData.Logs.CreateOrRecreateEventLog(
				@event,
				EstimateActual.Actual,
				dateTimeOffset,
				ZString.Empty,
				eventParameters);

			Factory.Save();
		}

		#region IDtbBookingParent Members

		public void TestIDtbBookingParent()
		{
			var consol = Factory.New<ForwardingConsol>();
			var dtbBookingParent = consol as IDtbBookingParent;

			consol.JK_UniqueConsignRef = "123";
			AssertEquals("Consol", dtbBookingParent.JobDescription);
			AssertEquals("123", dtbBookingParent.JobNumber);
			AssertEquals(JobInvoicingConsumerTypes.Consol.Code, dtbBookingParent.JobType);

			AssertContainsExactElementsInAnyOrder(new DtbBookingDirection[] { DtbBookingDirection.PIC, DtbBookingDirection.DLV }, dtbBookingParent.GetSupportedDirections());
		}

		public void TestIDtbBookingParent_ControllerID()
		{
			var consol = Factory.New<ForwardingConsol>();
			var dtbBookingParent = consol as IDtbBookingParent;
			AssertEquals(ControllerIDs.JobConsol, dtbBookingParent.ControllerID);
		}

		public void TestCanCreateTransportBooking()
		{
			var consol = Factory.New<ForwardingConsol>();
			var dtbBookingParent = consol as IDtbBookingParent;
			AssertEquals("CanCreateTransportBooking should always return true", true, dtbBookingParent.CanCreateTransportBooking);
		}

		public void TestBookingParentPK()
		{
			var consol = Factory.New<ForwardingConsol>();
			var dtbBookingParent = consol as IDtbBookingParent;
			AssertEquals("BookingParentPK should be the Consol PK.", consol.PK, dtbBookingParent.BookingParentPK);
		}

		public void TestBookingParentTablePrefix()
		{
			var consol = Factory.New<ForwardingConsol>();
			var dtbBookingParent = consol as IDtbBookingParent;
			AssertEquals("BookingParentTablePrefix should be the Consol table prefix.", consol.TablePrefix, dtbBookingParent.BookingParentTablePrefix);
		}

		public void TestGetExtendingConfirmMessageBeforeCreateTransportBooking()
		{
			var consol = Factory.New<ForwardingConsol>();
			var dtbBookingParent = consol as IDtbBookingParent;

			var (isShouldShow, caption, message, confirmation) = dtbBookingParent.GetExtendingConfirmMessageBeforeCreateTransportBooking();
			AssertEquals("IsShouldShow should return false.", false, isShouldShow);
			AssertNullOrEmpty("Caption should be null.", caption);
			AssertNullOrEmpty("Message should be null.", message);
			AssertNullOrEmpty("Confirmation should be null.", confirmation);
		}

		(ForwardingConsol, IDtbBooking, IDtbBooking) NewForwardingConsolWithPICDLVBookings(string loadPort, string dischargePort, string transportMode)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = loadPort;
			consol.JK_RL_NKDischargePort = dischargePort;
			consol.JK_TransportMode = transportMode;

			var consol1TransportBookingPIC = CO2eTestHelper.CreateTransportBooking(consol, nameof(DtbBookingDirection.PIC), Factory);
			var consol1TransportBookingDLV = CO2eTestHelper.CreateTransportBooking(consol, nameof(DtbBookingDirection.DLV), Factory);

			return (consol, consol1TransportBookingPIC, consol1TransportBookingDLV);
		}

		void SetupAndAssertShipmentStatusChanges(ForwardingConsol consol, ForwardingShipment shipment, IDtbBooking booking)
		{
			// Arrange
			shipment.SetCO2eStatus(CO2eStatusList.Codes.Current);
			var statusChangedCalled = false;
			shipment.JobCO2eCollection.JobCO2e_StatusChanged += delegate { statusChangedCalled = true; };

			// Act
			((ICO2ePrePostCarriage)consol).OnTransportBookingActiveStatusChanged(booking);

			// Assert
			Assert(statusChangedCalled);
			AssertEquals("shipment status changes to NCU", CO2eStatusList.Codes.NotCurrent, shipment.GetCO2eStatus());
			AssertHasWarning(shipment.TotalCO2eForBindingInfo, "Greenhouse gas emissions recalculation is required because the input data has changed.");
			AssertHasWarning(shipment.TotalCO2eForSortingInfo, "Greenhouse gas emissions recalculation is required because the input data has changed.");
		}

		void SetupAndAssertShipmentStatusDoesNotChange(ForwardingConsol consol, ForwardingShipment shipment, IDtbBooking booking)
		{
			// Arrange
			shipment.SetCO2eStatus(CO2eStatusList.Codes.Current);
			var statusChangedCalled = false;
			shipment.JobCO2eCollection.JobCO2e_StatusChanged += delegate { statusChangedCalled = true; };

			// Act
			((ICO2ePrePostCarriage)consol).OnTransportBookingActiveStatusChanged(booking);

			// Assert
			Assert(!statusChangedCalled);
			AssertEquals("shipment status doesnt change to NCU", CO2eStatusList.Codes.Current, shipment.GetCO2eStatus());
			AssertNoWarning(shipment.TotalCO2eForBindingInfo, "Greenhouse gas emissions recalculation is required because the input data has changed.");
			AssertNoWarning(shipment.TotalCO2eForSortingInfo, "Greenhouse gas emissions recalculation is required because the input data has changed.");
		}

		public void TestOnTransportBookingActiveStatusChanged_UpdateShipmentsAttachedToConsolToNCUIfStatusIsNotNON()
		{
			// Arrange
			var consol = Factory.New<ForwardingConsol>();
			var consolTransportBooking = CO2eTestHelper.CreateTransportBooking(consol, nameof(DtbBookingDirection.PIC), Factory);
			var consolShipment1 = consol.Shipments.AddNew();
			var consolShipment2 = consol.Shipments.AddNew();

			consolShipment1.SetCO2eStatus(CO2eStatusList.Codes.Current);
			consol.SetCO2eStatus(CO2eStatusList.Codes.Current);

			// Preassertions
			AssertEquals("consol status is current", CO2eStatusList.Codes.Current, consol.GetCO2eStatus());
			AssertEquals("shipment1 status is current", CO2eStatusList.Codes.Current, consolShipment1.GetCO2eStatus());
			AssertEquals("shipment1 status is not calculated", CO2eStatusList.Codes.NotCalculated, consolShipment2.GetCO2eStatus());

			// Act
			((ICO2ePrePostCarriage)consol).OnTransportBookingActiveStatusChanged(consolTransportBooking);

			// Assert
			AssertEquals("consol status does not change", CO2eStatusList.Codes.Current, consol.GetCO2eStatus());
			AssertEquals("shipment1 status changes to NCU", CO2eStatusList.Codes.NotCurrent, consolShipment1.GetCO2eStatus());
			AssertEquals("shipment2 status does not change from NON", CO2eStatusList.Codes.NotCalculated, consolShipment2.GetCO2eStatus());
		}

		public void TestOnTransportBookingActiveStatusChanged_AdditonalCalculationSupporter_BookingsActiveAndInactive()
		{
			// Arrange
			var (consol1, consol1TransportBookingPIC, consol1TransportBookingDLV) = NewForwardingConsolWithPICDLVBookings("AUSYD", "CNSHA", "AIR");
			var (consol2, consol2TransportBookingPIC, consol2TransportBookingDLV) = NewForwardingConsolWithPICDLVBookings("AUMEL", "DEFRA", "AIR");

			var shipment = Factory.New<ForwardingShipment>();

			consol1.Shipments.Add(shipment);
			consol2.Shipments.Add(shipment);

			// Assert
			AssertEquals(2, ((ICO2eCalculationSupporter)shipment).AdditionalCalculationSupporters.Length);
			AssertEquals(2, shipment.GetAdditionalCalculationSupporters(includeInactiveTBs: true).Length);
			AssertEquals(consol1TransportBookingPIC, (shipment.GetAdditionalCalculationSupporters(includeInactiveTBs: true)[0].Supporter));
			AssertEquals(consol2TransportBookingDLV, (shipment.GetAdditionalCalculationSupporters(includeInactiveTBs: true)[1].Supporter));

			// Act
			(consol1TransportBookingPIC as ICancellable).IsCancelled = true;
			(consol1TransportBookingDLV as ICancellable).IsCancelled = true;
			(consol2TransportBookingPIC as ICancellable).IsCancelled = true;
			(consol2TransportBookingDLV as ICancellable).IsCancelled = true;

			// Assert
			AssertEquals(0, ((ICO2eCalculationSupporter)shipment).AdditionalCalculationSupporters.Length);
			AssertEquals(2, shipment.GetAdditionalCalculationSupporters(includeInactiveTBs: true).Length);
			AssertEquals(consol1TransportBookingPIC, (shipment.GetAdditionalCalculationSupporters(includeInactiveTBs: true)[0].Supporter));
			AssertEquals(consol2TransportBookingDLV, (shipment.GetAdditionalCalculationSupporters(includeInactiveTBs: true)[1].Supporter));
		}

		public void TestOnTransportBookingActiveStatusChanged_UpdateShipmentsAttachedToConsolToNCUOnlyIfConsolTBPartOfShipmentCalculation()
		{
			// Arrange
			var (consol1, consol1TransportBookingPIC, consol1TransportBookingDLV) = NewForwardingConsolWithPICDLVBookings("AUSYD", "CNSHA", "AIR");
			var (consol2, consol2TransportBookingPIC, consol2TransportBookingDLV) = NewForwardingConsolWithPICDLVBookings("AUMEL", "DEFRA", "AIR");
			var (consol3, consol3TransportBookingPIC, consol3TransportBookingDLV) = NewForwardingConsolWithPICDLVBookings("AUMEL", "DEFRA", "AIR");

			var shipment = Factory.New<ForwardingShipment>();

			consol1.Shipments.Add(shipment);
			consol2.Shipments.Add(shipment);
			consol3.Shipments.Add(shipment);

			// Assert
			SetupAndAssertShipmentStatusChanges(consol1, shipment, consol1TransportBookingPIC);
			SetupAndAssertShipmentStatusDoesNotChange(consol1, shipment, consol1TransportBookingDLV);
			SetupAndAssertShipmentStatusDoesNotChange(consol2, shipment, consol2TransportBookingPIC);
			SetupAndAssertShipmentStatusDoesNotChange(consol2, shipment, consol2TransportBookingDLV);
			SetupAndAssertShipmentStatusDoesNotChange(consol3, shipment, consol3TransportBookingPIC);
			SetupAndAssertShipmentStatusChanges(consol3, shipment, consol3TransportBookingDLV);

			// Arrange
			(consol1TransportBookingPIC as ICancellable).IsCancelled = true;
			(consol1TransportBookingDLV as ICancellable).IsCancelled = true;
			(consol2TransportBookingPIC as ICancellable).IsCancelled = true;
			(consol2TransportBookingDLV as ICancellable).IsCancelled = true;
			(consol3TransportBookingPIC as ICancellable).IsCancelled = true;
			(consol3TransportBookingDLV as ICancellable).IsCancelled = true;

			// Assert
			SetupAndAssertShipmentStatusChanges(consol1, shipment, consol1TransportBookingPIC);
			SetupAndAssertShipmentStatusDoesNotChange(consol1, shipment, consol1TransportBookingDLV);
			SetupAndAssertShipmentStatusDoesNotChange(consol2, shipment, consol2TransportBookingPIC);
			SetupAndAssertShipmentStatusDoesNotChange(consol2, shipment, consol2TransportBookingDLV);
			SetupAndAssertShipmentStatusDoesNotChange(consol3, shipment, consol3TransportBookingPIC);
			SetupAndAssertShipmentStatusChanges(consol3, shipment, consol3TransportBookingDLV);
		}

		#endregion

		public void TestSplitBookingReferenceNumbersIntoAdditionalReferenceCollection()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_BookingReference = "111,222;333:444";
			consol.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG, "222");
			consol.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS, "333");

			AssertEquals(2, consol.Numbers.Count);

			consol.SplitBookingReferenceNumbersIntoAdditionalReferenceCollection();
			AssertEquals(4, consol.Numbers.Count);
			AssertEquals("111", consol.JK_BookingReference);
			var bkgNumbers = consol.Numbers.GetAllReferenceNumbersByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG).ToList();

			AssertEquals(3, bkgNumbers.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "222", "333", "444" }, bkgNumbers);
		}

		public void TestReferenceNumberShouldBeSplitIntoNumbers()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			AssertEquals(true, consol.ReferenceNumberShouldBeSplitIntoNumbers("123 456"));
			AssertEquals(true, consol.ReferenceNumberShouldBeSplitIntoNumbers("123,456"));
			AssertEquals(true, consol.ReferenceNumberShouldBeSplitIntoNumbers("123;456"));
			AssertEquals(true, consol.ReferenceNumberShouldBeSplitIntoNumbers("123:456"));

			AssertEquals(false, consol.ReferenceNumberShouldBeSplitIntoNumbers("123/456"));
			AssertEquals(false, consol.ReferenceNumberShouldBeSplitIntoNumbers("123-456"));
		}

		public void TestEditShipmentInConsolFormSyncConsolScreeningStatus()
		{
			var matchedOrg = Factory.NewWithValidTestData<OrgHeader>();
			var unknownOrg = Factory.NewWithValidTestData<OrgHeader>();
			var unknownOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			matchedOrg.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			unknownOrg.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			unknownOrg2.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			Factory.Save();

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			Factory.Save();
			orgAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();
			AssertEquals("Precondition", ScreeningStatusesList.Codes.Clear, orgAddress.Header.OH_ScreeningStatus);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_OA_SendingForwarderAddress = orgAddress.PK;
			consol.Transports.Cast<Transport>().ForEach(t => t.JW_VesselScreeningStatus = ScreeningStatusesList.Codes.Clear);
			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_OH_ExportBroker = unknownOrg.PK;
			Factory.Save();

			consol.Shipments.Add(shipment);
			Factory.Save();
			CombineAssertions("Precondition", () =>
			{
				AssertEquals(ScreeningStatusesList.Codes.Unknown, consol.JK_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Unknown, shipment.JS_ScreeningStatus);
			});

			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			Factory.Save();
			CombineAssertions("Precondition", () =>
			{
				AssertEquals(ScreeningStatusesList.Codes.JobCleared, consol.JK_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.JobCleared, shipment.JS_ScreeningStatus);
			});

			consol.JK_OA_SendingForwarderAddress = matchedOrg.PK;
			consol.JK_OA_SendingForwarderAddress = orgAddress.PK;
			AssertEquals(true, (consol as IShouldUpdateScreeningStatus).ShouldUpdateScreeningStatus);

			consol.Shipments[0].JS_OH_ExportBroker = unknownOrg2.PK;
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals(ScreeningStatusesList.Codes.Unknown, consol.JK_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Unknown, shipment.JS_ScreeningStatus);
			});
		}

		public void TestGetWorstScreeningStatusTransportWithoutVesselCode()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var transport = consol.Transports.AddNew();
			transport.JW_VesselScreeningStatus = ScreeningStatusesList.Codes.Matched;
			transport.JW_IsLinked = false;
			transport.JW_Vessel = string.Empty;

			var result = (consol as IScreeningPartyProvider).GetWorstScreeningStatus();
			AssertEquals(ScreeningStatusesList.Codes.PermanentClear, result);
		}

		public void TestGetWorstTransportScreeningStatus()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var transport = consol.Transports.AddNew();
			transport.JW_VesselScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_IsLinked = true;
			transport.JW_Vessel = vessel.RV_FK;

			var result = (consol as IScreeningPartyProvider).GetWorstScreeningStatus();
			CombineAssertions(() =>
			{
				AssertEquals(ScreeningStatusesList.Codes.Matched, result);
			});

			transport.JW_IsLinked = false;
			result = (consol as IScreeningPartyProvider).GetWorstScreeningStatus();
			AssertEquals(ScreeningStatusesList.Codes.Matched, result);
		}

		public void TestGetWorstTransportScreeningStatus_ShouldScrrenTransportLegVessel_WhenTransportModeIsIWT()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var transport = consol.Transports.AddNew();
			transport.JW_VesselScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			transport.JW_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			transport.JW_IsLinked = true;
			transport.JW_Vessel = vessel.RV_FK;

			var result = (consol as IScreeningPartyProvider).GetWorstScreeningStatus();
			CombineAssertions(() =>
			{
				AssertEquals(ScreeningStatusesList.Codes.Matched, result);
			});

			transport.JW_IsLinked = false;
			result = (consol as IScreeningPartyProvider).GetWorstScreeningStatus();
			AssertEquals(ScreeningStatusesList.Codes.Matched, result);
		}

		public void TestGetWorstScreeningStatusUnlessManuallyCleared()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			var result = (consol as IScreeningPartyProvider).GetWorstScreeningStatusUnlessManuallyCleared();
			CombineAssertions(() =>
			{
				AssertNotEquals(ScreeningStatusesList.Codes.JobCleared, result);
				AssertEquals(ScreeningStatusesList.Codes.PermanentClear, result);
			});

			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			result = (consol as IScreeningPartyProvider).GetWorstScreeningStatusUnlessManuallyCleared();
			AssertEquals(ScreeningStatusesList.Codes.JobCleared, result);
		}

		public void TestIsDPSFreightMovementRestrictedCore_InternationalJobs()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.Int);

				var consol = Factory.NewWithValidTestData<ForwardingConsolForTest>();
				consol.JK_ScreeningStatus = "MAT";
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "USLAX";

				CombineAssertions(() =>
				{
					AssertEquals(true, consol.IsExport());
					AssertEquals(false, consol.IsImport());
					AssertEquals(true, consol.IsDPSFreightMovementRestrictedCore_Exposed());
				});

				consol.JK_RL_NKDischargePort = "AUSYD";
				CombineAssertions(() =>
				{
					AssertEquals(false, consol.IsExport());
					AssertEquals(false, consol.IsImport());
					AssertEquals(false, consol.IsDPSFreightMovementRestrictedCore_Exposed());
				});

				consol.JK_RL_NKLoadPort = "USLAX";
				CombineAssertions(() =>
				{
					AssertEquals(false, consol.IsExport());
					AssertEquals(true, consol.IsImport());
					AssertEquals(true, consol.IsDPSFreightMovementRestrictedCore_Exposed());
				});
			}
		}

		public void TestVisualizableDocumentsSupportableIsRegisteredInObjectFactory()
		{
			var attribute = typeof(ForwardingConsol)
				.GetCustomAttributes(false)
				.OfType<VisualizableDocumentsSupportableAttribute>()
				.FirstOrDefault();

			AssertNotNull("Expected VisualizableDocumentsSupportableAttribute on ForwardingConsol.", attribute);

			var supporterType = ObjectFactory.GetType(attribute.SupporterType.Name);

			AssertNotNull("Expected VisualizableDocumentsSupportableAttribute is registered with ObjectFactory.", supporterType);
		}

		public void TestGetShipmentSpecialHandlingCodes()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			var container = consol.Containers.AddNew();

			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(consol, container);
			var undg = packLine.UNDGs.AddNew();

			AssertEquals(false, consol.GetShipmentSpecialHandlingCodes().Any());

			void AssertSpecialHandlingCodeWithCombinationOfUNDGSection(string code, string section, string specialHandlingCode1, params string[] expectedSpecialHandlingCodes)
			{
				var substance = Factory.NewWithValidTestData<UNDGSubstance>();
				substance.DG_Standard = UNDGSubstanceStandardTypes.IATA;
				substance.DG_Code = code;
				substance.DG_UNNO = code.Substring(0, 4);
				substance.DG_Variant = code.Length > 4 ? code[4].ToString() : string.Empty;
				substance.DG_SpecialHandlingCodes = specialHandlingCode1;
				undg.DI_DG = substance.PK;
				undg.DI_PackingInstructionSection = section;

				AssertContainsExactElementsInAnyOrder(consol.GetShipmentSpecialHandlingCodes(), expectedSpecialHandlingCodes);
			}

			AssertSpecialHandlingCodeWithCombinationOfUNDGSection("3480", "IA", "", "RBI");
			AssertSpecialHandlingCodeWithCombinationOfUNDGSection("3480", "IB", "", "RBI");
			AssertSpecialHandlingCodeWithCombinationOfUNDGSection("3480", "II", "", "EBI");

			AssertSpecialHandlingCodeWithCombinationOfUNDGSection("3481A", "I", "", "RLI");
			AssertSpecialHandlingCodeWithCombinationOfUNDGSection("3481A", "II", "", "ELI");
			AssertSpecialHandlingCodeWithCombinationOfUNDGSection("3481B", "I", "", "RLI");
			AssertSpecialHandlingCodeWithCombinationOfUNDGSection("3481B", "II", "", "ELI");

			AssertSpecialHandlingCodeWithCombinationOfUNDGSection("3090", "IA", "", "RBM");
			AssertSpecialHandlingCodeWithCombinationOfUNDGSection("3090", "IB", "", "RBM");
			AssertSpecialHandlingCodeWithCombinationOfUNDGSection("3090", "II", "", "EBM");

			AssertSpecialHandlingCodeWithCombinationOfUNDGSection("3091A", "I", "", "RLM");
			AssertSpecialHandlingCodeWithCombinationOfUNDGSection("3091A", "II", "", "ELM");
			AssertSpecialHandlingCodeWithCombinationOfUNDGSection("3091B", "I", "", "RLM");
			AssertSpecialHandlingCodeWithCombinationOfUNDGSection("3091B", "II", "", "ELM");

			AssertSpecialHandlingCodeWithCombinationOfUNDGSection("1845", "", "ACE", "ACE");
			AssertSpecialHandlingCodeWithCombinationOfUNDGSection("3373", "", "RDS", "RDS");
			AssertSpecialHandlingCodeWithCombinationOfUNDGSection("3056", "", "RFL", "RFL");
		}

		public void TestGetShipmentSpecialHandlingCodes_CAO()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var container = consol.Containers.AddNew();

			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(consol, container);

			var undgForPackLine1 = packLine.UNDGs.AddNew();
			var undgSubstance1 = Factory.New<UNDGSubstance>();
			undgSubstance1.DG_UNNO = "0001";
			undgSubstance1.DG_Code = "0001";
			undgSubstance1.DG_Standard = UNDGSubstanceStandardTypes.IATA;

			undgSubstance1.DG_CargoMaxAmt = 60.0;
			undgSubstance1.DG_CargoMaxAmtUQ = "KG";
			undgSubstance1.DG_LQ2OrPaxMaxAmt = 5.0;
			undgSubstance1.DG_LQ2OrPaxMaxAmtUQ = "KG";

			undgForPackLine1.DI_DGWeight = 20;
			undgForPackLine1.DI_DG = undgSubstance1.PK;
			undgForPackLine1.DI_PackageCount = 1;
			consol.DefaultSpecialHandlingItems();

			Assert("Assert consol.AWBSpecialHandlingItems has no items as no unit was specified.", consol.AWBSpecialHandlingItems.IsNullOrEmpty());

			undgForPackLine1.DI_UnitOfVolume = "L";
			consol.DefaultSpecialHandlingItems();

			Assert("Assert consol.AWBSpecialHandlingItems has no items as wrong unit was specified.", consol.AWBSpecialHandlingItems.IsNullOrEmpty());

			undgForPackLine1.DI_UnitOfWeight = "KG";
			consol.DefaultSpecialHandlingItems();

			var handlingItems = new List<ZString>();
			foreach (var item in consol.AWBSpecialHandlingItems.Cast<NonSecurityJobConsolAWBSpecialHandling>())
			{
				handlingItems.Add(item.JKH_Code);
			}
			AssertContainsExactElementsInAnyOrder("Assert consol.AWBSpecialHandlingItems has CAO code.", new ZString[] { "CAO" }, handlingItems.ToArray());
			consol.AWBSpecialHandlingItems.Cast<NonSecurityJobConsolAWBSpecialHandling>().ForEach(x => AssertNoWarnings("Check Special Handling Items have no warning", x.JKH_CodeInfo));

			var undgForPackLine2 = packLine.UNDGs.AddNew();
			var undgSubstance2 = Factory.New<UNDGSubstance>();
			undgSubstance2.DG_UNNO = "0002";
			undgSubstance2.DG_Code = "0002";
			undgSubstance2.DG_Standard = UNDGSubstanceStandardTypes.IATA;

			undgSubstance2.DG_CargoMaxAmt = 60.0;
			undgSubstance2.DG_LQ2OrPaxMaxAmt = 25.0;
			undgSubstance2.DG_CargoMaxAmtUQ = "L";

			undgForPackLine2.DI_DGVolume = 20;
			undgForPackLine2.DI_DG = undgSubstance2.PK;
			undgForPackLine2.DI_UnitOfVolume = "L";
			consol.DefaultSpecialHandlingItems();

			consol.AWBSpecialHandlingItems.Cast<NonSecurityJobConsolAWBSpecialHandling>().ForEach(x => AssertNoWarnings("Check Special Handling Items have no warning", x.JKH_CodeInfo));

			undgForPackLine1.DI_DG = ZGuid.Empty;
			consol.DefaultSpecialHandlingItems();

			consol.AWBSpecialHandlingItems.Cast<NonSecurityJobConsolAWBSpecialHandling>().ForEach(x => AssertHasWarnings("Check Special Handling Items have a warning when no DG assigned to the shipment", x.JKH_CodeInfo));
		}

		public void TestGetShipmentSpecialHandlingCodes_CAO_LimitedQuantityTypes()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var container = consol.Containers.AddNew();

			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(consol, container);

			var undgForPackLine1 = packLine.UNDGs.AddNew();
			var undgSubstance1 = Factory.New<UNDGSubstance>();
			undgSubstance1.DG_UNNO = "0001";
			undgSubstance1.DG_Code = "0001";
			undgSubstance1.DG_Standard = UNDGSubstanceStandardTypes.IATA;

			undgSubstance1.DG_CargoMaxAmt = 60.0;
			undgSubstance1.DG_CargoMaxAmtUQ = "KG";
			undgSubstance1.DG_LQ2OrPaxMaxAmt = 5.0;
			undgSubstance1.DG_CargoPackAmtType = LimitedQuantityTypes.NLTCode;

			undgForPackLine1.DI_DGWeight = 20;
			undgForPackLine1.DI_DG = undgSubstance1.PK;
			undgForPackLine1.DI_UnitOfWeight = "KG";
			undgForPackLine1.DI_PackageCount = 1;
			consol.DefaultSpecialHandlingItems();

			Assert("Assert consol.AWBSpecialHandlingItems has no items as CargoPackAmtType was NLT.", consol.AWBSpecialHandlingItems.IsNullOrEmpty());

			undgSubstance1.DG_CargoPackAmtType = LimitedQuantityTypes.FOBCode;
			consol.DefaultSpecialHandlingItems();

			Assert("Assert consol.AWBSpecialHandlingItems has no items as CargoPackAmtType was FOB.", consol.AWBSpecialHandlingItems.IsNullOrEmpty());

			undgSubstance1.DG_CargoPackAmtType = LimitedQuantityTypes.NLMCode;
			undgSubstance1.DG_LQ2OrPaxMaxAmtUQ = "KG";
			consol.DefaultSpecialHandlingItems();

			var handlingItems = new List<ZString>();
			foreach (var item in consol.AWBSpecialHandlingItems.Cast<NonSecurityJobConsolAWBSpecialHandling>())
			{
				handlingItems.Add(item.JKH_Code);
			}
			AssertContainsExactElementsInAnyOrder("Assert consol.AWBSpecialHandlingItems has CAO code.", new ZString[] { "CAO" }, handlingItems.ToArray());
			consol.AWBSpecialHandlingItems.Cast<NonSecurityJobConsolAWBSpecialHandling>().ForEach(x => AssertNoWarnings("Check Special Handling Items have no warning", x.JKH_CodeInfo));
		}

		public void TestGetShipmentSpecialHandlingCodes_CAO_WithLithiumBattery()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var container = consol.Containers.AddNew();

			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(consol, container);

			var undgForPackLine1 = packLine.UNDGs.AddNew();
			var undgSubstance1 = Factory.New<UNDGSubstance>();
			undgSubstance1.DG_UNNO = LithiumBatteryConstants.UNNOCodes.LithiumIonBatteries;
			undgSubstance1.DG_Code = LithiumBatteryConstants.UNNOCodes.LithiumIonBatteries;
			undgSubstance1.DG_Standard = UNDGSubstanceStandardTypes.IATA;

			undgSubstance1.DG_CargoMaxAmt = 60.0;
			undgSubstance1.DG_CargoMaxAmtUQ = "KG";
			undgSubstance1.DG_LQ2OrPaxMaxAmt = 5.0;
			undgSubstance1.DG_LQ2OrPaxMaxAmtUQ = "KG";

			undgForPackLine1.DI_DGWeight = 20;
			undgForPackLine1.DI_DG = undgSubstance1.PK;
			undgForPackLine1.DI_PackageCount = 1;
			
			undgForPackLine1.DI_UnitOfWeight = "KG";
			consol.DefaultSpecialHandlingItems();

			var handlingItems = new List<ZString>();
			foreach (var item in consol.AWBSpecialHandlingItems.Cast<NonSecurityJobConsolAWBSpecialHandling>())
			{
				handlingItems.Add(item.JKH_Code);
			}
			AssertContainsExactElementsInAnyOrder("Assert consol.AWBSpecialHandlingItems has CAO code.", new ZString[] { LithiumBatteryConstants.SpecialHandlingCodes.RBI, AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoAircraftOnly }, handlingItems.ToArray());
			consol.AWBSpecialHandlingItems.Cast<NonSecurityJobConsolAWBSpecialHandling>().ForEach(x => AssertNoWarnings("Check Special Handling Items have no warning", x.JKH_CodeInfo));
		}

		public void TestIsValidForNeutralMaster()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			Assert(consol.IsValidForNeutralMaster);

			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			Assert(consol.IsValidForNeutralMaster);

			consol.JK_AgentType = Core.Constants.AgentType.AWBCoload;
			Assert(consol.IsValidForNeutralMaster);

			consol.JK_AgentType = Core.Constants.AgentType.Charter;
			Assert(!consol.IsValidForNeutralMaster);

			consol.JK_AgentType = Core.Constants.AgentType.AWBMaster;
			Assert(!consol.IsValidForNeutralMaster);

			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			Assert(!consol.IsValidForNeutralMaster);

			consol.JK_AgentType = Core.Constants.AgentType.OnBoardCourier;
			Assert(!consol.IsValidForNeutralMaster);

			consol.JK_AgentType = Core.Constants.AgentType.Other;
			Assert(!consol.IsValidForNeutralMaster);

			consol.JK_AgentType = Core.Constants.AgentType.Courier;
			Assert(!consol.IsValidForNeutralMaster);

			consol.JK_RL_NKLoadPort = "JPOSA";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			Assert(!consol.IsValidForNeutralMaster);

			var japan = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Japan));

			using (FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { japan.PK.ToGuid() }))
			{
				Assert("Consol is considered an export as Japan is in community region", consol.IsValidForNeutralMaster);
			}
		}

		public void TestCLMLinkedCLATotals()
		{
			var clm = Factory.NewWithValidTestData<ForwardingConsol>();
			clm.JK_TransportMode = Constants.TransportModes.Air;
			clm.JK_RL_NKLoadPort = "AUSYD";
			clm.JK_RL_NKDischargePort = "NZAKL";
			clm.JK_AgentType = Constants.AgentType.AWBMaster;

			var cla1 = clm.ColoadConsols.AddNew();
			cla1.JK_TransportMode = Constants.TransportModes.Air;
			cla1.JK_AgentType = Constants.AgentType.AWBCoload;
			AddContainer(cla1, "45HA", 1);
			AddContainer(cla1, "45HA", 3);
			AddContainer(cla1, "45HB", 3);
			AssertEquals("4x45HA, 3x45HB", cla1.JK_Calc_ContainerTypesSummary);

			var s11 = cla1.Shipments.AddNew();
			s11.JS_TransportMode = Constants.TransportModes.Air;
			s11.JS_UnitFreightRate = 1m;
			s11.JS_INCO = Constants.IncoTerms.CostInsuranceAndFreight;
			s11.JS_ActualWeight = 1.2m;
			s11.JS_UnitOfWeight = Constants.Weight.Pounds;
			s11.JS_ActualVolume = 3.333m;
			s11.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			s11.JS_OuterPacks = 7;
			s11.JS_ActualChargeable = 1111m;

			var s12 = cla1.Shipments.AddNew();
			s12.JS_TransportMode = Constants.TransportModes.Air;
			s12.JS_UnitFreightRate = 3m;
			s12.JS_INCO = Constants.IncoTerms.FreeOnBoard;
			s12.JS_ActualWeight = 1.5m;
			s12.JS_UnitOfWeight = Constants.Weight.Pounds;
			s12.JS_ActualVolume = 4.444m;
			s12.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			s12.JS_OuterPacks = 8;
			s12.JS_ActualChargeable = 1111m;

			var s13 = cla1.Shipments.AddNew();
			s13.JS_TransportMode = Constants.TransportModes.Air;
			s13.JS_UnitFreightRate = 5m;
			s13.JS_INCO = Constants.IncoTerms.CostInsuranceAndFreight;
			s13.JS_ActualWeight = 2.8m;
			s13.JS_UnitOfWeight = Constants.Weight.Pounds;
			s13.JS_ActualVolume = 5.555m;
			s13.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			s13.JS_OuterPacks = 9;
			s13.JS_ActualChargeable = 1111m;

			var cla2 = clm.ColoadConsols.AddNew();
			cla2.JK_TransportMode = Constants.TransportModes.Air;
			cla1.JK_AgentType = Constants.AgentType.Direct;
			AddContainer(cla2, "45HC", 2);
			AddContainer(cla2, "45HC", 3);
			AddContainer(cla2, "45HD", 5);
			AssertEquals("5x45HC, 5x45HD", cla2.JK_Calc_ContainerTypesSummary);

			var s21 = cla2.Shipments.AddNew();
			s21.JS_TransportMode = Constants.TransportModes.Air;
			s21.JS_UnitFreightRate = 7m;
			s21.JS_INCO = Constants.IncoTerms.FreeOnBoard;
			s21.JS_ActualWeight = 3m;
			s21.JS_UnitOfWeight = Constants.Weight.Kilograms;
			s21.JS_ActualVolume = 6.666m;
			s21.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			s21.JS_OuterPacks = 10;
			s21.JS_ActualChargeable = 1111m;

			AssertEquals(2, clm.ColoadConsolCount);
			AssertEquals("7 + 8 + 9 + 10", 34, clm.JK_Calc_TotalColoadConsolQuantity);

			AssertEquals("1.2 + 1.5 + 2.8 + 6.613868LB(3KG)", 12.113868m, clm.JK_Calc_TotalColoadConsolWeight);
			AssertEquals(Constants.Weight.Pounds, clm.JK_Calc_TotalColoadConsolWeightUnit);

			AssertEquals("3.333 + 4.444 + 5.555 + 6.666", 19.998m, clm.JK_Calc_TotalColoadConsolVolume);
			AssertEquals(Constants.Volume.CubicMetres, clm.JK_Calc_TotalColoadConsolVolumeUnit);

			AssertEquals("1111 + 1111 + 1111 + 1111", 4444m, clm.JK_Calc_TotalColoadConsolChargeable);
			AssertEquals(Constants.Weight.Kilograms, clm.JK_Calc_TotalColoadConsolChargeableUnit);

			AssertEquals(s11.ChargeableAmount + s13.ChargeableAmount, clm.JK_Calc_TotalPrepaidColoadConsolChargeableAmount);
			AssertEquals("AUD", clm.JK_Calc_TotalPrepaidColoadConsolChargeableAmountCurrencyCode);

			AssertEquals(s12.ChargeableAmount + s21.ChargeableAmount, clm.JK_Calc_TotalCollectColoadConsolChargeableAmount);
			AssertEquals("AUD", clm.JK_TotalCollectColoadConsolChargeableAmountCurrencyCode);
		}

		void AddContainer(ForwardingConsol forwaringConsol, string code, ZShort count)
		{
			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = code;
			AddContainer(forwaringConsol, refContainer, count);
		}

		public void TestCLMTotalsAndTopLevels()
		{
			var clm = Factory.NewWithValidTestData<ForwardingConsol>();
			clm.JK_TransportMode = Constants.TransportModes.Air;
			clm.JK_RL_NKLoadPort = "AUSYD";
			clm.JK_RL_NKDischargePort = "NZAKL";
			clm.JK_AgentType = Constants.AgentType.AWBMaster;

			var cla1 = clm.ColoadConsols.AddNew();
			var s11 = cla1.Shipments.AddNew();
			var s12 = cla1.Shipments.AddNew();
			var s13 = cla1.Shipments.AddNew();

			var cla2 = clm.ColoadConsols.AddNew();
			var s21 = cla2.Shipments.AddNew();

			AssertEquals(4, clm.TopLevelShipments.Count);
			AssertEquals(4, clm.ShipmentsForTotalling.Count);

			s13.JS_JS_ColoadMasterShipment = s11.PK;

			AssertEquals(3, clm.TopLevelShipments.Count);
			AssertEquals(3, clm.ShipmentsForTotalling.Count);

			for (int i = clm.ColoadConsols.Count - 1; i >= 0; i--)
			{
				clm.ColoadConsols[i].JK_JK_MasterConsol = ZGuid.Empty;
			}

			clm.JK_AgentType = Constants.AgentType.Agent;
			clm.Shipments.AddNew();
			AssertEquals(1, clm.GridShipments.Count);
		}

		public void TestISupportsPostingOverseasAgentCharge()
		{
			var consol = Factory.New<ForwardingConsol>();
			AssertNotNull(@"ForwardingConsol is made to implement the empty interface ISupportsPostingOverseasAgentCharge so that action trigger POA can be made
					applicable specifically to this type"
				, consol);
		}

		public void TestGetCreditorPK()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;

			var carrierAddress = Factory.NewWithValidTestData<OrgAddress>();
			carrierAddress.OA_Code = "CarrierAdr";
			carrierAddress.OA_OH = carrier.PK;

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			sendingForwarder.OH_IsCreditor = true;

			var sendingForwarderAddress = Factory.NewWithValidTestData<OrgAddress>();
			sendingForwarderAddress.OA_Code = "SendingForwarderAdr";
			sendingForwarderAddress.OA_OH = sendingForwarder.PK;

			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			receivingForwarder.OH_IsCreditor = true;

			var receivingForwarderAddress = Factory.NewWithValidTestData<OrgAddress>();
			receivingForwarderAddress.OA_Code = "ReceivingForwarderAdr";
			receivingForwarderAddress.OA_OH = receivingForwarder.PK;

			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();

			forwardingConsol.JK_RL_NKDischargePort = "SGSIN";
			forwardingConsol.JK_OA_ShippingLineAddress = carrierAddress.PK;
			forwardingConsol.JK_OA_SendingForwarderAddress = sendingForwarderAddress.PK;
			forwardingConsol.JK_OA_ReceivingForwarderAddress = receivingForwarderAddress.PK;

			forwardingConsol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			forwardingConsol.CreditorPK = creditor.PK;
			Factory.Save();
			AssertCreditorPKWithDifferentChargeGroup(forwardingConsol);

			forwardingConsol.JK_PrepaidCollect = Core.Constants.PaymentType.Collect;
			Factory.Save();
			AssertCreditorPKWithDifferentChargeGroup(forwardingConsol);

			forwardingConsol.JK_RL_NKLoadPort = "AUSYD";
			forwardingConsol.JK_RL_NKLoadPort = "SGSIN";
			Factory.Save();
			AssertCreditorPKWithDifferentChargeGroup(forwardingConsol);

			forwardingConsol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			Factory.Save();
			AssertCreditorPKWithDifferentChargeGroup(forwardingConsol);
		}

		public void TestGetCreditorPKForConsolCost_WhenRateProviderOrgPKIsEmpty_WithConsolIsDomesticAndNotCoLoad()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			forwardingConsol.JK_RL_NKLoadPort = "AUSYD";
			forwardingConsol.JK_RL_NKDischargePort = "AUADL";
			forwardingConsol.JK_OA_ShippingLineAddress = carrier.Addresses[0].PK;
			forwardingConsol.CreditorPK = creditor.PK;
			Factory.Save();

			var creditorPK = forwardingConsol.GetCreditorPKForConsolCost(ZGuid.Empty);
			AssertEquals("When the not Co-Load Domestic Consol has a creditor, then the charge creditor becomes the Carrier Export Creditor"
					, forwardingConsol.CarrierExportCreditorAddress.OrganisationPK
					, creditorPK);

			forwardingConsol.CreditorPK = ZGuid.Empty;

			creditorPK = forwardingConsol.GetCreditorPKForConsolCost(ZGuid.Empty);
			AssertEquals("When the not Co-Load Domestic Consol has no creditor, then the charge creditor becomes the Consol Carrier"
					, forwardingConsol.JK_OA_ShippingLineAddress_ZAddress.OrgHeader.PK
					, creditorPK);
		}

		public void TestGetCreditorPKForConsolCost_WhenRateProviderOrgPKIsEmpty_WithConsolIsDomesticAndCoLoad()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;

			var coLoadWith = Factory.NewWithValidTestData<OrgHeader>();
			coLoadWith.OH_IsCreditor = true;

			var exportCreditorAddress = Factory.NewWithValidTestData<OrgHeader>();
			exportCreditorAddress.OH_IsCreditor = true;

			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			forwardingConsol.JK_RL_NKLoadPort = "AUSYD";
			forwardingConsol.JK_RL_NKDischargePort = "AUADL";
			forwardingConsol.JK_AgentType = AgentType.CoLoad;
			forwardingConsol.JK_OA_ShippingLineAddress = carrier.Addresses[0].PK;
			forwardingConsol.CreditorPK = coLoadWith.PK;
			forwardingConsol.CarrierExportCreditorAddress.OrganisationPK = exportCreditorAddress.PK;
			Factory.Save();

			var creditorPK = forwardingConsol.GetCreditorPKForConsolCost(ZGuid.Empty);
			AssertEquals("When the Co-Load Domestic Consol has a Carrier Export Creditor, then the charge creditor becomes the Carrier Export Creditor"
					, forwardingConsol.CarrierExportCreditorAddress.OrganisationPK
					, creditorPK);

			forwardingConsol.CarrierExportCreditorAddress.OrganisationPK = ZGuid.Empty;

			creditorPK = forwardingConsol.GetCreditorPKForConsolCost(ZGuid.Empty);
			AssertEquals("When the not Co-Load Domestic Consol has no Carrier Export Creditor, then the charge creditor becomes the Consol Co-Load with"
					, forwardingConsol.CreditorPK
					, creditorPK);
		}

		void AssertCreditorPKWithDifferentChargeGroup(ForwardingConsol forwardingConsol)
		{
			var creditorPK = forwardingConsol.GetCreditorPKForConsolCost(ZGuid.Empty);
			Assert(!creditorPK.IsEmpty);
			AssertEquals(ZGuid.Empty, forwardingConsol.CostSupporter.GetCreditorPK(ZString.Empty, ZGuid.Empty));
			AssertEquals(creditorPK, forwardingConsol.CostSupporter.GetCreditorPK(ChargeCodeGroupList.Codes.Transport, ZGuid.Empty));
			AssertEquals(creditorPK, forwardingConsol.CostSupporter.GetCreditorPK(ChargeCodeGroupList.Codes.Freight, ZGuid.Empty));
		}

		public void TestProfitLossContainer()
		{
			var consol = Factory.New<ForwardingConsol>();
			AssertNotNull("This needs to exist and be instantiated so that in accounting we can add an item to it for binding purposes", consol.ProfitLossContainer);
			AssertEquals("There should be no items in it - the single item is added in accounting", 0, consol.ProfitLossContainer.Count);
		}

		public void TestAUCMRCusSCAOceanBill()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var oceanBill = Factory.New<IAUCusSCAOceanBill>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			AssertEquals(oceanBill.PK, consol.AUCMRCusSCAOceanBill.PK);

			((BusinessObject)oceanBill).Delete();
			consol.Delete();

			consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			var anotherFactory = new BusinessObjectFactory();
			oceanBill = anotherFactory.New<IAUCusSCAOceanBill>();
			((BusinessObject)oceanBill).FillWithValidTestData();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			anotherFactory.Save();
			AssertEquals(oceanBill.PK, consol.AUCMRCusSCAOceanBill.PK);
		}

		public void TestEntityToValidate()
		{
			var consol = Factory.New<ForwardingConsol>();
			var entity = (IValidateForCustomsMessagingSupporter)consol;

			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			try
			{
				entity.GetEntityToValidate(WorkflowTriggerActionTypeConstants.Codes.ValidateForAUCargoMessaging);
			}
			catch (NullReferenceException)
			{
				Assert("GetEntityToValidate should return null, not a null reference exception", false);
			}

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var mawb1 = Factory.New<IAUCusMAWB>();
			mawb1.CM_JK = consol.PK;
			var oceanBill = Factory.New<IAUCusSCAOceanBill>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			AssertEquals((BusinessObject)mawb1, entity.GetEntityToValidate(WorkflowTriggerActionTypeConstants.Codes.ValidateForAUCargoMessaging));
			AssertEquals("TriggerAction ROT, TransportMode AIR", (BusinessObject)mawb1, entity.GetEntityToValidate(WorkflowTriggerActionTypeConstants.Codes.ReconcileOutturn));

			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals((BusinessObject)oceanBill, entity.GetEntityToValidate(WorkflowTriggerActionTypeConstants.Codes.ValidateForAUCargoMessaging));
			AssertNull("TriggerAction ROT, TransportMode SEA", entity.GetEntityToValidate(WorkflowTriggerActionTypeConstants.Codes.ReconcileOutturn));
		}

		public void TestDensity()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_CorrectedConsolWeightUnit = Constants.Weight.Kilograms;
			consol.JK_CorrectedConsolVolumeUnit = Constants.Volume.CubicMetres;
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_ActualWeight = 40m;
			shipment1.JS_ActualVolume = 7m;
			shipment1.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment1.JS_UnitOfVolume = Constants.Volume.CubicMetres;

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_ActualWeight = 40m;
			shipment2.JS_ActualVolume = 8m;
			shipment2.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment2.JS_UnitOfVolume = Constants.Volume.CubicMetres;

			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			var density = consol.Density;
			CombineAssertions(() =>
			{
				AssertNotNull("Density should be initialized", density);
				AssertEquals("Density Factor should be updated", 31.25m, density.DensityFactor);
				AssertEquals("Density Remark should be updated", "Volume +++++", density.DensityRemark);
				AssertEquals("Volume Ratio should be updated", "1:12", density.VolumeRatio);
			});
		}

		public void TestTriggerShouldNotUpdateJK_UniqueConsignRef()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_UniqueConsignRef = "JK_AgentsReference";

			Factory.Save();

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_RL_NKLoadPort = "AUSYD";
			consol2.JK_RL_NKDischargePort = "USLAX";
			consol2.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol2.JK_TransportMode = Constants.TransportModes.Sea;

			var trigger = consol2.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "IFC Trigger for test";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01.Code;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = JobConsolSchema.JK_UniqueConsignRef.Name;
			action.PQ_FieldValue = consol1.JK_UniqueConsignRef;
			AssertEquals("", consol2.JK_UniqueConsignRef);

			consol2.Logs.AddNew(Events.CustomisableEvent01);
			AssertEquals("", consol2.JK_UniqueConsignRef);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestJK_Calc_CostFreePercentage()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			AssertEquals("Precondition: consol's chargeable unit is weight", Constants.Weight.Kilograms, consol.JK_ConsolChargeableUnit);
			AssertEquals("Total shipment chargeable", 0m, consol.JK_TotalShipmentChargeable);

			consol.Shipments.AddNew().JS_ActualChargeable = 20m;
			AssertEquals("Total shipment chargeable", 20m, consol.JK_TotalShipmentChargeable);

			consol.Shipments.AddNew().JS_ActualWeight = 100m;
			AssertEquals(20m, consol.JK_Calc_CostFreePercentage);
		}

		public void TestJK_Calc_ConsolidatedFreightCostChargeable()
		{
			var nonFreightCharge = Factory.NewWithValidTestData<AccChargeCode>();
			nonFreightCharge.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			nonFreightCharge.AC_Code = "OTT";
			nonFreightCharge.Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ActualWeight = 1500m;
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_ActualVolume = 2m;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;

			AssertEquals(consol.JK_Calc_ConsolidatedFreightCostChargeable, 0m);

			var freightCost = consol.CreateConsolCost("AUD");
			freightCost[JobConsolCostSchema.E6_LocalCostAmount] = 300m;

			var nonFreightCost = consol.CreateConsolCost("AUD");
			nonFreightCost[JobConsolCostSchema.E6_LocalCostAmount] = 500m;
			nonFreightCost[JobConsolCostSchema.E6_AC_ChargeCode] = nonFreightCharge.PK;

			AssertEquals(consol.JK_Calc_ConsolidatedFreightCostChargeable, 150m);

			freightCost[JobConsolCostSchema.E6_LocalCostAmount] = 1000m;
			AssertEquals(consol.JK_Calc_ConsolidatedFreightCostChargeable, 500m);
		}

		public void TestJK_Calc_ShipmentFreightCostChargeable()
		{
			var nonFreightCharge = Factory.NewWithValidTestData<AccChargeCode>();
			nonFreightCharge.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			nonFreightCharge.AC_Code = "OTT";
			nonFreightCharge.Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.Loose;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualChargeable = 1500m;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ActualChargeable = 3000m;

			var decimalPoints = consol.LocalCurrencyDecimals;

			AssertEquals(consol.JK_Calc_ShipmentFreightCostChargeable, 0m);

			var freightCost1 = consol.CreateConsolCost("AUD");
			freightCost1[JobConsolCostSchema.E6_LocalCostAmount] = 1500m;

			var freightCost2 = consol.CreateConsolCost("AUD");
			freightCost2[JobConsolCostSchema.E6_LocalCostAmount] = 2000m;

			var nonFreightCost = consol.CreateConsolCost("AUD");
			nonFreightCost[JobConsolCostSchema.E6_LocalCostAmount] = 3000m;
			nonFreightCost[JobConsolCostSchema.E6_AC_ChargeCode] = nonFreightCharge.PK;

			AssertEquals(consol.JK_Calc_ShipmentFreightCostChargeable.Round(decimalPoints), 0.78m);

			freightCost2[JobConsolCostSchema.E6_LocalCostAmount] = 600m;
			AssertEquals(consol.JK_Calc_ShipmentFreightCostChargeable.Round(decimalPoints), 0.47m);

			shipment1.JS_ActualChargeable = 500;
			AssertEquals(consol.JK_Calc_ShipmentFreightCostChargeable.Round(decimalPoints), 0.6m);

			shipment2.JS_ActualWeight = 7500;
			AssertEquals(consol.JK_Calc_ShipmentFreightCostChargeable.Round(decimalPoints), 0.26m);

			shipment1.JS_ActualVolume = 50;
			AssertEquals(consol.JK_Calc_ShipmentFreightCostChargeable.Round(decimalPoints), 0.13m);

			consol.Shipments.Remove(shipment2.PK);
			AssertEquals(consol.JK_Calc_ShipmentFreightCostChargeable.Round(decimalPoints), 0.25m);
		}

		public void TestJK_Calc_ConsolidatedFreightCostChargeableDesc()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ActualWeight = 1500m;
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_ActualVolume = 2m;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;

			var chargeableDesc = $"{GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency} PER {consol.JK_ConsolChargeableUnit}";
			AssertEquals(chargeableDesc, consol.JK_Calc_ConsolidatedFreightCostChargeableDesc);
		}

		public void TestJK_Calc_ShipmentFreightCostChargeableDesc()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ActualWeight = 20000m;
			shipment.JS_UnitOfWeight = Constants.Weight.LongTons;

			var chargeableDesc = $"{GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency} PER {consol.JK_Calc_TotalShipmentChargeableUnit}";
			AssertEquals(chargeableDesc, consol.JK_Calc_ShipmentFreightCostChargeableDesc);
		}

		public void TestBusinessObjectWithRelatedEventsOnManifest()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C111223";
			var header1 = Factory.New<ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			((BusinessObject)header1).FillWithValidTestData();
			header1.AMA_ParentId = consol.PK;
			header1.AMA_ParentTableCode = consol.TablePrefix;
			header1.AMA_JobReference = "1";
			var bill1 = Factory.New<ASYCUDA.ZAManifest.IAsycudaBill>();
			bill1.ABL_AMA = header1.PK;
			bill1.ABL_BillNumber = "bill1";

			var pack1 = Factory.New<ASYCUDA.ZAManifest.IAsycudaPack>();
			pack1.APA_ABL_Bill = bill1.PK;
			pack1.APA_GoodsDescription = "pack1";

			var header2 = Factory.New<ASYCUDA.NZManifest.IAsycudaManifestHeader>();
			((BusinessObject)header2).FillWithValidTestData();
			header2.AMA_ParentId = consol.PK;
			header2.AMA_ParentTableCode = consol.TablePrefix;
			header2.AMA_JobReference = "2";

			var header3 = Factory.New<ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			((BusinessObject)header3).FillWithValidTestData();
			header3.AMA_ParentId = consol.PK;
			header3.AMA_ParentTableCode = consol.TablePrefix;
			var bill3 = Factory.New<ASYCUDA.ACEManifest.IAsycudaBill>();
			bill3.ABL_AMA = header3.PK;

			var vocHeader = Factory.New<ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			vocHeader.AMA_ApplicationCode = "VOC";
			vocHeader.AMA_ParentId = consol.PK;
			vocHeader.AMA_ParentTableCode = consol.TablePrefix;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newConsol = newFactory.Load<ForwardingConsol>(consol.PK);
			Assert("Header1 should be in the list of related objects", newConsol.BusinessObjectsWithRelatedEvents.Any(x => x.PK == header1.PK));
			Assert("Header2 should not be in the list of related objects, NZ manifests are all Shipping Line manifests.", !newConsol.BusinessObjectsWithRelatedEvents.Any(x => x.PK == header2.PK));
			Assert("Header3 should be in the list of related objects", newConsol.BusinessObjectsWithRelatedEvents.Any(x => x.PK == header3.PK));

			Assert("Bill1 should be in the list of related objects", newConsol.BusinessObjectsWithRelatedEvents.Any(x => x.PK == bill1.PK));
			Assert("Pack1 should be in the list of related objects", newConsol.BusinessObjectsWithRelatedEvents.Any(x => x.PK == pack1.PK));

			Assert("Bill3 should be in the list of related objects", newConsol.BusinessObjectsWithRelatedEvents.Any(x => x.PK == bill3.PK));

			Assert("VOC manifest should not be in the list of related objects", !newConsol.BusinessObjectsWithRelatedEvents.Any(x => x.PK == vocHeader.PK));
		}

		public void TestBusinessObjectsWithRelatedEvents()
		{
			var consol = Factory.New<ForwardingConsol>();
			var gbMawb = Factory.New<GB.CCSUK.ICusMAWB>();
			var gbHawb = Factory.New<GB.CCSUK.ICusHAWB>();
			var jpAFRHeader = Factory.New<JP.AFR.IJPAFRHeader>();
			jpAFRHeader.JPH_Voyage = "008N";
			var jpAFRHeader2 = Factory.New<JP.AFR.IJPAFRHeader>();
			gbHawb.CS_CM = ((BusinessObject)gbMawb).PK;
			gbMawb.CM_JK = consol.PK;
			jpAFRHeader.JPH_ParentId = consol.PK;
			jpAFRHeader.JPH_ParentTableCode = consol.TablePrefix;
			jpAFRHeader.JPH_IsShippingLineEntry = false;
			jpAFRHeader2.JPH_ParentId = Guid.NewGuid();
			jpAFRHeader2.JPH_ParentTableCode = consol.TablePrefix;
			jpAFRHeader2.JPH_IsShippingLineEntry = false;
			var nctsheader = Factory.New<EU.NCTS.ICusInBondHeader>();
			nctsheader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsheader.BH_ParentID = consol.PK;
			Factory.Save();

			jpAFRHeader.JPH_Voyage = "009N";

			var amsHeader = Factory.New<US.USAMS.ICusInBondHeader>();
			amsHeader.BH_ParentID = consol.PK;
			amsHeader.BH_ParentTableCode = consol.TablePrefix;
			var amsBill = Factory.New<US.USAMS.ICusInBondBill>();
			amsBill.B0_BH = amsHeader.PK;
			Factory.Save();

			AssertCollectionContains(gbMawb, consol.BusinessObjectsWithRelatedEvents);
			AssertCollectionNotContains(gbHawb, consol.BusinessObjectsWithRelatedEvents);
			AssertCollectionContains(jpAFRHeader, consol.BusinessObjectsWithRelatedEvents);
			AssertCollectionNotContains(jpAFRHeader2, consol.BusinessObjectsWithRelatedEvents);
			var foundJPAFRHeaders = consol.BusinessObjectsWithRelatedEvents.OfType<JP.AFR.IJPAFRHeader>();
			AssertEquals(1, foundJPAFRHeaders.Count());
			AssertEquals("009N", foundJPAFRHeaders.FirstOrDefault().JPH_Voyage);
			AssertCollectionContains(amsHeader, consol.BusinessObjectsWithRelatedEvents);
			AssertCollectionContains(nctsheader, consol.BusinessObjectsWithRelatedEvents);
			AssertCollectionContains(amsBill, consol.BusinessObjectsWithRelatedEvents);

			var caeMaster = Factory.New<CA.ICusCAeMHMaster>();
			caeMaster.BP_ParentID = consol.PK;
			caeMaster.BP_ParentTableCode = consol.TablePrefix;
			Factory.Save();
			AssertCollectionContains(caeMaster, consol.BusinessObjectsWithRelatedEvents);

			using (var job = Factory.NewJobForTesting<JobHeader>())
			{
				job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				job.JH_ParentID = consol.PK;
				AssertNotNull(consol.Job);
				AssertCollectionContains(consol.Job, consol.BusinessObjectsWithRelatedEvents);
			}
		}

		public void TestBusinessObjectsWithRelatedEvents_ContainsVisualizerDocumentData()
		{
			var consol = Factory.New<ForwardingConsol>();

			var documentData1 = (BusinessObject)Factory.New<IVisualizerDocumentData>();
			documentData1[JobDocumentDataSchema.JDD_ParentTableCode] = consol.TablePrefix;
			documentData1[JobDocumentDataSchema.JDD_ParentID] = consol.PK;

			var documentData2 = (BusinessObject)Factory.New<IVisualizerDocumentData>();
			documentData2[JobDocumentDataSchema.JDD_ParentTableCode] = consol.TablePrefix;
			documentData2[JobDocumentDataSchema.JDD_ParentID] = consol.PK;

			var documentData3 = (BusinessObject)Factory.New<IVisualizerDocumentData>();
			documentData3[JobDocumentDataSchema.JDD_ParentTableCode] = "JS";
			documentData3[JobDocumentDataSchema.JDD_ParentID] = new ZGuid();

			AssertContainsExactElementsInAnyOrder("BusinessObjectsWithRelatedEvents contains sent document data",
				new[] { documentData1, documentData2 },
				consol.BusinessObjectsWithRelatedEvents.OfType<IVisualizerDocumentData>());
		}

		public void TestJK_Calc_ContainerTypesSummary()
		{
			var consol = Factory.New<ForwardingConsol>();
			var refContainer_20NOR = Factory.New<RefContainer>();
			refContainer_20NOR.RC_Code = "20NOR";

			AddContainer(consol, refContainer_20NOR, 1);
			AddContainer(consol, refContainer_20NOR, 2);
			AddContainer(consol, refContainer_20NOR, 1);

			var refContainer_45HC = Factory.New<RefContainer>();
			refContainer_45HC.RC_Code = "45HC";

			AddContainer(consol, refContainer_45HC, 2);
			AddContainer(consol, refContainer_45HC, 1);

			var expected = "4x20NOR, 3x45HC";

			AssertEquals(expected, consol.JK_Calc_ContainerTypesSummary);
		}

		public void TestJK_Calc_ContainerTypesSummary_NoContainers()
		{
			var consol = Factory.New<ForwardingConsol>();
			AssertEquals(ZString.Empty, consol.JK_Calc_ContainerTypesSummary);
		}

		public void TestJK_Calc_ContainerStorageClassesSummary()
		{
			var consol = Factory.New<ForwardingConsol>();
			var refContainer_40F = Factory.New<RefContainer>();
			refContainer_40F.RC_StorageClass = "40F";

			AddContainer(consol, refContainer_40F, 1);
			AddContainer(consol, refContainer_40F, 2);
			AddContainer(consol, refContainer_40F, 1);

			var refContainer_20F = Factory.New<RefContainer>();
			refContainer_20F.RC_StorageClass = "20F";

			AddContainer(consol, refContainer_20F, 2);
			AddContainer(consol, refContainer_20F, 1);

			var expected = "3x20F, 4x40F";

			AssertEquals(expected, consol.JK_Calc_ContainerStorageClassesSummary);
		}

		public void TestJK_Calc_ContainerStorageClassesSummary_NoContainers()
		{
			var consol = Factory.New<ForwardingConsol>();
			AssertEquals(ZString.Empty, consol.JK_Calc_ContainerStorageClassesSummary);
		}

		void AddContainer(ForwardingConsol consol, RefContainer refContainer, ZShort count)
		{
			var container = consol.Containers.AddNew();
			container.JC_RC = refContainer.PK;
			container.JC_ContainerCount = count;
		}

		public void TestIControllerIDProviderMembers()
		{
			var consol = Factory.New<ForwardingConsol>();
			IControllerIDProvider provider = consol;
			AssertEquals("ControllerID", ControllerIDs.JobConsol, provider.ControllerID);
			AssertEquals("BusinessObjectPK", consol.PK.ToGuid(), provider.BusinessObjectPK);
		}

		public void TestTemperatureControl_SecurityRights_PreventsGUIChanges()
		{
			var securityInstance = SecurityTestHelper.CreateSecurityInstance(Factory);
			using (Env.SetTemporarySecurityInstanceForTest(securityInstance))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();

				securityInstance.ConsolTempControl.IsAllowed = true;
				CombineAssertions("Temperature Control properties on consol should be GUI editable when ConsolTempControl security is granted", () =>
				{
					Assert(!consol.JK_RequiresTemperatureControl_ReadOnly);
					Assert(!consol.JK_RequiredTemperatureMinimum_ReadOnly);
					Assert(!consol.JK_RequiredTemperatureMaximum_ReadOnly);
					Assert(!consol.JK_RequiredTemperatureUnit_ReadOnly);
				});

				securityInstance.ConsolTempControl.IsAllowed = false;
				CombineAssertions("Temperature Control properties on consol should not be GUI editable when ConsolTempControl security is denied", () =>
				{
					Assert(consol.JK_RequiresTemperatureControl_ReadOnly);
					Assert(consol.JK_RequiredTemperatureMinimum_ReadOnly);
					Assert(consol.JK_RequiredTemperatureMaximum_ReadOnly);
					Assert(consol.JK_RequiredTemperatureUnit_ReadOnly);
				});
			}
		}

		public void TestTemperatureControl_SecurityRights_DoesntPreventProgramaticChanges()
		{
			var securityInstance = SecurityTestHelper.CreateSecurityInstance(Factory);
			using (Env.SetTemporarySecurityInstanceForTest(securityInstance))
			{
				securityInstance.ConsolTempControl.IsAllowed = false;

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_RequiresTemperatureControl = true;
				consol.JK_RequiredTemperatureMinimum = 5;
				consol.JK_RequiredTemperatureMaximum = 15;
				consol.JK_RequiredTemperatureUnit = Constants.Temperature.Fahrenheit;

				CombineAssertions("Temperature Control properties on consol should be not affected programatically by ConsolTempControl security", () =>
				{
					AssertEquals(true, consol.JK_RequiresTemperatureControl);
					AssertEquals(new ZDecimal(5), consol.JK_RequiredTemperatureMinimum);
					AssertEquals(new ZDecimal(15), consol.JK_RequiredTemperatureMaximum);
					AssertEquals(Constants.Temperature.Fahrenheit, consol.JK_RequiredTemperatureUnit);
				});
			}
		}

		public void TestIsAllowedToAttachShipment_CheckMaximumAllowableContainerAndPacklineDimensions()
		{
			PreAllocationCheckCollection checks = ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.Value;
			checks.Dimensions.Action = PreAllocationCheck.Actions.Restriction;
			checks.Dimensions.Percentage = 100m;

			using (ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, checks))
			{
				IsAllowedToAttachShipment_CheckMaximumAllowableContainerAndPacklineDimensions_Helper(true);
			}

			checks.Dimensions.Action = PreAllocationCheck.Actions.None;
			checks.Dimensions.Percentage = 0m;
			using (ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, checks))
			{
				IsAllowedToAttachShipment_CheckMaximumAllowableContainerAndPacklineDimensions_Helper(false);
			}
		}

		void IsAllowedToAttachShipment_CheckMaximumAllowableContainerAndPacklineDimensions_Helper(bool expectsError)
		{
			IShipmentVsConsolMessageHelper helper = FreightShipmentVsConsolMessageHelper.Instance;
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MaximumAllowablePackageLength = 5;
			consol.JK_MaximumAllowablePackageWidth = 5;
			consol.JK_MaximumAllowablePackageHeight = 5;
			consol.JK_MaximumAllowablePackageUnit = Constants.Length.Metres;

			var shipment = Factory.New<ForwardingShipment>();

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_Length = 10;
			packLine.JL_Width = 10;
			packLine.JL_Height = 10;
			packLine.JL_UnitOfDimension = Constants.Length.Metres;

			var shipmentAttachErrorMesssage = "Cannot attach shipment [new shipment] as it contains cargo dimensions that exceed the maximum dimensions set by the consol [new consol].";

			var message = helper.IsAllowedToAttachConsol(shipment, consol);
			if (expectsError)
			{
				AssertContains(shipmentAttachErrorMesssage, message.Errors);
			}
			else
			{
				AssertNotContains(shipmentAttachErrorMesssage, message.Errors);
			}

			packLine.JL_Length = 3;
			packLine.JL_Width = 3;
			packLine.JL_Height = 3;
			packLine.JL_UnitOfDimension = Constants.Length.Metres;

			message = helper.IsAllowedToAttachConsol(shipment, consol);
			AssertNotContains(shipmentAttachErrorMesssage, message.Errors);
		}

		#region TestEventPropagation

		public void TestArrivalAndDepartureEventsOnConsolShouldBePropagatedToTransportLeg()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";

			var firstTransport = consol.Transports[0];
			firstTransport.JW_LegOrder = 1;
			firstTransport.JW_RL_NKLoadPort = "AUSYD";
			firstTransport.JW_RL_NKDiscPort = "AUMEL";
			firstTransport.JW_VoyageFlight = "QF111";
			firstTransport.JW_ETA = new ZDateTime(2015, 05, 20);

			var secondTransport = consol.Transports.AddNew();
			secondTransport.JW_LegOrder = 2;
			secondTransport.JW_RL_NKLoadPort = "AUMEL";
			secondTransport.JW_RL_NKDiscPort = "DEFRA";
			secondTransport.JW_VoyageFlight = "QF111";
			secondTransport.JW_ETD = new ZDateTime(2015, 05, 20);

			AssertEquals("Precondition: Arrival event not recorded", null, firstTransport.Logs.MostRecentLogByEventTime(Events.Arrival));
			AssertEquals("Precondition: Departure event not recorded", null, secondTransport.Logs.MostRecentLogByEventTime(Events.Departure));

			KeyValuePair<string, string>[] arrivalEventParameters = new[]
			{
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Facility, EventConstants.Facilities.Code.Terminal),
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.VoyageFlightNumber, "QF111"),
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.FlightDate, "20-May-2015")
			};

			consol.Logs.AddNew(Events.Arrival, new ZDateTimeOffset(2015, 05, 20), arrivalEventParameters);
			AssertNull("Arrival Event with incomplete parameters has not been propagated", firstTransport.Logs.MostRecentLogByEventTime(Events.Arrival));

			KeyValuePair<string, string>[] departureEventParameters = new[]
			{
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Location, "AUMEL"),
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.VoyageFlightNumber, "QF111"),
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.FlightDate, "20-May-2015")
			};

			consol.Logs.AddNew(Events.Departure, new ZDateTimeOffset(2015, 05, 20), departureEventParameters);
			AssertNull("Departure Event with incomplete parameters has not been propagated", secondTransport.Logs.MostRecentLogByEventTime(Events.Departure));

			var eventParameters = new[]
			{
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Facility, EventConstants.Facilities.Code.Terminal),
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Location, "AUMEL"),
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.VoyageFlightNumber, "QF111"),
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.FlightDate, "20-May-2015")
			};

			var arrivalEvent = consol.Logs.AddNew(Events.Arrival, new ZDateTimeOffset(2015, 05, 20), eventParameters);
			var departureEvent = consol.Logs.AddNew(Events.Departure, new ZDateTimeOffset(2015, 05, 18), eventParameters);

			AssertNotNull("Arrival Event has been propagated to arrival transport leg", firstTransport.Logs.MostRecentLogByEventTime(Events.Arrival));
			AssertNull("Departure Event has not been propagated to arrival transport leg", firstTransport.Logs.MostRecentLogByEventTime(Events.Departure));
			AssertNotNull("Departure Event has been propagated to departure transport leg", secondTransport.Logs.MostRecentLogByEventTime(Events.Departure));
			AssertNull("Arrival Event has not been propagated to departure transport leg", secondTransport.Logs.MostRecentLogByEventTime(Events.Arrival));

			AssertEquals("Arrival Date should be correct", arrivalEvent.SL_EventTime, firstTransport.JW_ATA);
			AssertEquals("Departure Date should be correct", departureEvent.SL_EventTime, secondTransport.JW_ATD);

			AssertEquals("Arrival Event from transport leg should not cascade back to Consol", arrivalEvent, arrivalEvent);
			AssertEquals("Departure Event from transport leg should not cascade back to Consol", departureEvent, departureEvent);
		}

		public void TestArrivalDepartureEventOnForwardingConsolIsNotPropagatedWhenTransportHasTheSameDate()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";

			var firstTransport = consol.Transports.AddNew();
			firstTransport.JW_LegOrder = 1;
			firstTransport.JW_RL_NKLoadPort = "AUSYD";
			firstTransport.JW_RL_NKDiscPort = "AUMEL";
			firstTransport.JW_ETA = new ZDateTime(2015, 05, 20);

			var secondTransport = consol.Transports.AddNew();
			secondTransport.JW_LegOrder = 2;
			secondTransport.JW_RL_NKLoadPort = "AUMEL";
			secondTransport.JW_RL_NKDiscPort = "DEFRA";
			secondTransport.JW_ATD = new ZDateTime(2015, 05, 18);

			var eventParameters = new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Facility, EventConstants.Facilities.Code.Terminal),
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Location, "AUMEL")
			};

			var arrivalEvent = consol.Logs.AddNew(Events.Arrival, new ZDateTimeOffset(2015, 05, 20), true, eventParameters);
			var departureEvent = consol.Logs.AddNew(Events.Departure, new ZDateTimeOffset(2015, 05, 18), false, eventParameters);

			AssertNull("Arrival Event has not been propagated transport leg", firstTransport.Logs.MostRecentLogByEventTime(Events.Arrival));
			AssertNull("Departure Event has not been propagated transport leg", secondTransport.Logs.MostRecentLogByEventTime(Events.Departure));
		}

		public void TestArrivalDepartureEventOnForwardingConsolIsNotPropagatedWhenLogPropagetedFromShipment()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_RL_NKDestination = "DEFRA";

			var firstTransport = consol.Transports[0];
			firstTransport.JW_LegOrder = 1;
			firstTransport.JW_RL_NKLoadPort = "AUSYD";
			firstTransport.JW_RL_NKDiscPort = "AUMEL";
			firstTransport.JW_VoyageFlight = "QF111";
			firstTransport.JW_ETA = new ZDateTime(2015, 05, 20);

			var secondTransport = consol.Transports.AddNew();
			secondTransport.JW_LegOrder = 2;
			secondTransport.JW_RL_NKLoadPort = "AUMEL";
			secondTransport.JW_RL_NKDiscPort = "DEFRA";
			secondTransport.JW_VoyageFlight = "QF111";
			secondTransport.JW_ETD = new ZDateTime(2015, 05, 20);

			var eventParameters = new[]
			{
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Facility, EventConstants.Facilities.Code.Terminal),
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Location, "AUMEL"),
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.VoyageFlightNumber, "QF111"),
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.FlightDate, "20-May-2015")
			};

			shipment.Logs.AddNew(Events.Arrival, new ZDateTimeOffset(2015, 05, 20), eventParameters);
			shipment.Logs.AddNew(Events.Departure, new ZDateTimeOffset(2015, 05, 18), eventParameters);

			AssertEquals("Precondition: Arrival event not propagated to transports when event is propagated from shipment", null, firstTransport.Logs.MostRecentLogByEventTime(Events.Arrival));
			AssertEquals("Precondition: Departure event not propagated to transports when event is propagated from shipment", null, secondTransport.Logs.MostRecentLogByEventTime(Events.Departure));
		}

		public void TestArrivalDepartureOnTransportLegIsNotPropagatedToShipment()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.CoLoad;

			var consolTransport = consol.Transports.AddNew("AUMEL", "SGSIN");
			consolTransport.JW_LegOrder = 1;
			consolTransport.JW_ETD = new ZDateTime(2016, 3, 19);
			consolTransport.JW_ETA = new ZDateTime(2016, 4, 1);

			Factory.Save();

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_RL_NKDestination = "ESBCN";
			shipment.JS_E_DEP = new ZDateTime(2016, 3, 19);
			shipment.JS_E_ARV = new ZDateTime(2016, 5, 5);

			var transportShipment = shipment.Transports.AddNew();
			transportShipment.JW_LegOrder = 2;
			transportShipment.JW_RL_NKLoadPort = "SGSIN";
			transportShipment.JW_RL_NKDiscPort = "ESBCN";

			transportShipment.JW_ETD = new ZDateTime(2016, 4, 13);
			transportShipment.JW_ETA = new ZDateTime(2016, 5, 2);

			Factory.Save();

			AssertEquals(new ZDateTime(2016, 3, 19), shipment.JS_E_DEP);
			AssertEquals(new ZDateTime(2016, 5, 5), shipment.JS_E_ARV);
		}

		public void TestProcessDataLinkedEvent()
		{
			var logHasBeenProcessed = false;

			var processor = new DummyBookingConfirmationDataLinkedProcessor();
			processor.OnProcess = (biz, log) => logHasBeenProcessed = true;

			using (ObjectFactory.Substitute<ILinkedDocumentMessageProcessorProvider>(processor))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.Logs.AddNew(Events.DataLinked, ZDateTimeOffset.Now, false);
			}

			Assert("log has been processed", logHasBeenProcessed);
		}

		sealed class DummyBookingConfirmationDataLinkedProcessor : ILinkedDocumentMessageProcessorProvider
		{
			public void Process(IBusiness bizObj, IStmALog log)
			{
				OnProcess?.Invoke(bizObj, log);
			}

			public Action<IBusiness, IStmALog> OnProcess
			{
				get;
				set;
			}
		}

		public void TestLinkedDocumentMessageProcessorProviderProcessorRegistration()
		{
			var processor = ObjectFactory.Get<ILinkedDocumentMessageProcessorProvider>();

			AssertNotNull("ILinkedDocumentMessageProcessorProvider has been registered in ObjectFactory", processor);
		}

		#endregion

		#region TestCorrectDocumentReceivedEventLogging

		public void TestNoAIDEventLoggingOnConsolWithCountryRequiredDocuments()
		{
			var testClasses = new RefCountryRequiredDocumentCollectionTest();
			testClasses.CreateRequiredDocumentsForAustraliaAndOriginSingapore();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "AUBNE";

			var requiredDocument = consol.RequiredDocuments.AddNew();

			requiredDocument.EQ_DocCategory = Constants.ReferenceTypes.SupplyChainLogistics;
			requiredDocument.EQ_DocType = Constants.RefDocTypes.BeneficiaryCertificate;
			requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Import;
			requiredDocument.EQ_DocPeriod = Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			requiredDocument.EQ_DateReceived = DateTime.Today;
			requiredDocument.EQ_ValidToDate = DateTime.Today;
			requiredDocument.EQ_DocNumber = "0001";

			Factory.Save();

			var logs = consol.Logs.GetAllLogs();
			AssertNull("AID event should not be logged", consol.Logs.MostRecentLogByEventTime(Events.AllImportDocumentsReceived));
		}

		public void TestAIDEventLoggingOnConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "AUBNE";

			var requiredDocument = consol.RequiredDocuments.AddNew();

			requiredDocument.EQ_DocCategory = Constants.ReferenceTypes.SupplyChainLogistics;
			requiredDocument.EQ_DocType = Constants.RefDocTypes.BeneficiaryCertificate;
			requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Import;
			requiredDocument.EQ_DocPeriod = Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			requiredDocument.EQ_DateReceived = DateTime.Today;
			requiredDocument.EQ_ValidToDate = DateTime.Today;
			requiredDocument.EQ_DocNumber = "0001";

			Factory.Save();

			var logs = consol.Logs.GetAllLogs();
			AssertNotNull("AID event should be logged", consol.Logs.MostRecentLogByEventTime(Events.AllImportDocumentsReceived));
		}

		public void TestNoAEDEventLoggingOnConsolWithCountryRequiredDocuments()
		{
			var testClasses = new RefCountryRequiredDocumentCollectionTest();
			testClasses.CreateRequiredDocumentsForAustraliaAndOriginSingapore();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "AUBNE";

			var requiredDocument = consol.RequiredDocuments.AddNew();

			requiredDocument.EQ_DocCategory = Constants.ReferenceTypes.SupplyChainLogistics;
			requiredDocument.EQ_DocType = Constants.RefDocTypes.BeneficiaryCertificate;
			requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Export;
			requiredDocument.EQ_DocPeriod = Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			requiredDocument.EQ_DateReceived = DateTime.Today;
			requiredDocument.EQ_ValidToDate = DateTime.Today;
			requiredDocument.EQ_DocNumber = "0001";

			Factory.Save();

			var logs = consol.Logs.GetAllLogs();
			AssertNull("AED event should not be logged", consol.Logs.MostRecentLogByEventTime(Events.AllExportDocumentsReceived));
		}

		public void TestAEDEventLoggingOnConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "AUBNE";

			var requiredDocument = consol.RequiredDocuments.AddNew();

			requiredDocument.EQ_DocCategory = Constants.ReferenceTypes.SupplyChainLogistics;
			requiredDocument.EQ_DocType = Constants.RefDocTypes.BeneficiaryCertificate;
			requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Export;
			requiredDocument.EQ_DocPeriod = Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			requiredDocument.EQ_DateReceived = DateTime.Today;
			requiredDocument.EQ_ValidToDate = DateTime.Today;
			requiredDocument.EQ_DocNumber = "0001";

			Factory.Save();

			var logs = consol.Logs.GetAllLogs();
			AssertNotNull("AED event should be logged", consol.Logs.MostRecentLogByEventTime(Events.AllExportDocumentsReceived));
		}

		#endregion

		#region TestFreightPayableAt

		public void TestFreightPayableAt()
		{
			AssertNull("FreightPayableAt null", Consol.FreightPayableAt);

			var loadPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			var discPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "NZAKL"));
			var pickupPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUMEL"));
			var deliveryPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "SGSIN"));
			var anotherPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUBNE"));
			AssertNull("FreightPayableAt", Consol.FreightPayableAt);

			Consol.JK_PrepaidCollect = Core.Constants.PaymentType.Collect;
			AssertNull("FreightPayableAt null", Consol.FreightPayableAt);

			Consol.JK_RL_NKDischargePort = discPort.RL_Code;
			AssertEquals("FreightPayableAt Discharge Port", discPort.RL_PortName, Consol.FreightPayableAt.RL_PortName);

			Consol.JK_RL_NKLoadPort = loadPort.RL_Code;
			AssertEquals("FreightPayableAt Discharge Port", discPort.RL_PortName, Consol.FreightPayableAt.RL_PortName);

			Consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			AssertEquals("FreightPayableAt Load Port", loadPort.RL_PortName, Consol.FreightPayableAt.RL_PortName);

			ForwardingShipment shipment = Consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUBNE";

			Consol.JK_AgentType = Constants.AgentType.Direct;
			AssertNull("FreightPayableAt null", Consol.FreightPayableAt);

			shipment.JS_RL_NKOrigin = pickupPort.RL_Code;
			shipment.JS_INCO = Constants.DomesticPaymentTerms.Prepaid;
			AssertEquals("FreightPayableAt Pickup Port", pickupPort.RL_PortName, Consol.FreightPayableAt.RL_PortName);

			shipment.JS_RL_NKDestination = deliveryPort.RL_Code;
			shipment.JS_INCO = "FOB";
			AssertEquals("FreightPayableAt Pickup Port", deliveryPort.RL_PortName, Consol.FreightPayableAt.RL_PortName);
		}

		public void TestDirectShipment()
		{
			AssertNull(Consol.DirectShipment);

			Consol.JK_AgentType = Constants.AgentType.Direct;
			AssertNull(Consol.DirectShipment);

			Consol.JK_AgentType = Constants.AgentType.Other;
			ForwardingShipment shipment1 = Consol.Shipments.AddNew();
			AssertEquals(false, shipment1.IsDirectShipment);
			AssertNull(Consol.DirectShipment);

			Consol.JK_AgentType = Constants.AgentType.Direct;
			AssertEquals(true, shipment1.IsDirectShipment);
			AssertEquals("DirectShipment should be first shipment from Consol.Shipments collection", shipment1, Consol.DirectShipment);

			ForwardingShipment shipment2 = Consol.Shipments.AddNew();

			shipment1.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals(false, shipment1.IsDirectShipment);

			shipment2.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			AssertEquals(true, shipment2.IsDirectShipment);

			AssertEquals("DirectShipment should be second shipment from Consol.Shipments collection", shipment2, Consol.DirectShipment);
		}

		#endregion

		#region TestPickupLocation & TestDeliveryLocation

		public void TestPickupLocation()
		{
			var consolPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			var shipmentPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUMEL"));
			var anotherPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUBNE"));
			var cfsPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "NZAKL"));

			AssertNull(Consol.PickupLocation);

			Consol.JK_RL_NKLoadPort = consolPort.RL_Code;
			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = "";

			AssertEquals("Shipments.Count", 0, Consol.Shipments.Count);
			AssertEquals("PickupLocation should be Consol Orgin", consolPort.Code, Consol.PickupLocation.Code);

			CommonShipment shipment = Consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = shipmentPort.RL_Code;
			AssertEquals("PickupLocation should be Shipments Orgin", shipmentPort.Code, Consol.PickupLocation.Code);

			shipment = Consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = shipmentPort.RL_Code;
			AssertEquals("PickupLocation should be Shipments Orgin", shipmentPort.Code, Consol.PickupLocation.Code);

			shipment.JS_RL_NKOrigin = anotherPort.RL_Code;
			AssertEquals("PickupLocation should be Consol Orgin", consolPort.Code, Consol.PickupLocation.Code);

			OrgHeader header = Factory.New<OrgHeader>();
			OrgAddress address1 = header.Addresses.AddNew();
			OrgAddress address2 = header.Addresses.AddNew();

			address1.OA_RL_NKRelatedPortCode = cfsPort.RL_Code;
			address2.OA_RL_NKRelatedPortCode = anotherPort.RL_Code;

			Consol.JK_OA_PackDepotAddress = header.Addresses[1].PK;
			AssertEquals("PickupLocation should be CFS Main Address", cfsPort.Code, Consol.PickupLocation.Code);

			header.OH_RL_NKClosestPort = cfsPort.RL_Code;
			Consol.JK_OA_PackDepotAddress = header.Addresses[2].PK;
			AssertEquals("PickupLocation should be CFS Main Address", anotherPort.Code, Consol.PickupLocation.Code);

			shipment.JS_RL_NKOrigin = shipmentPort.RL_Code;
			AssertEquals("PickupLocation should be Shipments Orgin", shipmentPort.Code, Consol.PickupLocation.Code);
		}

		public void TestDeliveryLocation()
		{
			var consolPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			var shipmentPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUMEL"));
			var anotherPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUBNE"));
			var cfsPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "NZAKL"));

			AssertNull(Consol.DeliveryLocation);

			Consol.JK_RL_NKDischargePort = consolPort.RL_Code;
			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKDiscPort = "";

			AssertEquals("Shipments.Count should be zero", 0, Consol.Shipments.Count);
			AssertEquals("DeliveryLocation should be Consol Destination", consolPort.Code, Consol.DeliveryLocation.Code);

			CommonShipment shipment = Consol.Shipments.AddNew();
			shipment.JS_RL_NKDestination = shipmentPort.RL_Code;
			AssertEquals("DeliveryLocation Shipments Destination", shipmentPort.Code, Consol.DeliveryLocation.Code);

			shipment = Consol.Shipments.AddNew();
			shipment.JS_RL_NKDestination = shipmentPort.RL_Code;
			AssertEquals("DeliveryLocation Shipments Destination", shipmentPort.Code, Consol.DeliveryLocation.Code);

			shipment.JS_RL_NKDestination = anotherPort.RL_Code;
			AssertEquals("DeliveryLocation should be Consol Destination", consolPort.Code, Consol.DeliveryLocation.Code);

			OrgHeader header = Factory.New<OrgHeader>();
			OrgAddress address1 = header.Addresses.AddNew();
			OrgAddress address2 = header.Addresses.AddNew();

			address1.OA_RL_NKRelatedPortCode = cfsPort.RL_Code;
			address2.OA_RL_NKRelatedPortCode = anotherPort.RL_Code;

			Consol.JK_OA_UnpackDepotAddress = header.Addresses[1].PK;
			AssertEquals("DeliveryLocation should be CFS Main Address", cfsPort.Code, Consol.DeliveryLocation.Code);

			header.OH_RL_NKClosestPort = cfsPort.RL_Code;
			Consol.JK_OA_UnpackDepotAddress = header.Addresses[2].PK;
			AssertEquals("DeliveryLocation should be CFS Main Address", anotherPort.Code, Consol.DeliveryLocation.Code);

			shipment.JS_RL_NKDestination = shipmentPort.RL_Code;
			AssertEquals("DeliveryLocation Shipments Destination", shipmentPort.Code, Consol.DeliveryLocation.Code);
		}

		#endregion

		[TestDate(2023, 2, 22, 10, 11, 48)]
		public void TestGetAWBIssueDateOnAirConsol()
		{
			var now = ZDateTime.Now;
			var jx_CTOCutOff = now.AddDays(1);
			var jx_LCLCutOff = now.AddDays(2);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(now, consol.GetAWBIssueDate());

			var transport = consol.Transports[0];
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.Flight2;
			transport.JW_TerminalCutOff = now.AddDays(10);
			transport.JW_DepotCutOff = now.AddDays(20);
			AssertEquals(now, consol.GetAWBIssueDate());

			consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;
			AssertEquals(now, consol.GetAWBIssueDate());

			FreightConfigurationRegistry.Instance.AWBIssueDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, FreightConfigurationRegistry.Instance.AWBIssueDateIsCutOffDate);
			AssertEquals(now, consol.GetAWBIssueDate());

			transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;
			transport.JW_TerminalCutOff = jx_CTOCutOff;
			transport.JW_DepotCutOff = jx_LCLCutOff;
			AssertEquals("Time portion of JX_CTOCutOff is not used", jx_CTOCutOff, consol.GetAWBIssueDate());

			consol.JK_ConsolMode = Core.Constants.ContainerModes.Loose;
			AssertEquals("Time portion of JX_LCLCutOff is not used", jx_LCLCutOff, consol.GetAWBIssueDate());

			consol.JK_ConsolMode = Core.Constants.ContainerModes.BuyersConsol;
			AssertEquals("Time portion of JX_CTOCutOff is not used", jx_CTOCutOff, consol.GetAWBIssueDate());
		}

		[TestDate(2023, 4, 18, 10, 10, 48)]
		public void TestGetAWBIssueDateOnSeaConsol()
		{
			var now = ZDateTime.Now;
			var today = now.Date;
			var jx_CTOCutOff = now.AddDays(1);
			var jx_LCLCutOff = now.AddDays(2);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(today, consol.GetAWBIssueDate());

			var transport = consol.Transports[0];
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport.JW_TerminalCutOff = now.AddDays(10);
			transport.JW_DepotCutOff = now.AddDays(20);
			AssertEquals(today, consol.GetAWBIssueDate());

			consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;
			AssertEquals(today, consol.GetAWBIssueDate());
		}

		#region AWBSpecialHandlingItems

		public void TestDefaultSpecialHandlingItemsDoesNotAddDuplicateAWBSpecialHandlingItems()
		{
			var defaultJKHCode = "EAW";
			var accountingCode = "020";
			var query = new ZQuery(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, accountingCode);
			var airline = Factory.LoadTop1<RefAirline>(query);
			airline.HasSignedEAWBAgreement = true;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			consol.AWBSpecialHandlingItems.AddNew().JKH_Code = defaultJKHCode;

			consol.JK_MasterBillNum = accountingCode;
			consol.DefaultSpecialHandlingItems();

			var defaultSpecialHandling = consol.AWBSpecialHandlingItems.Cast<JobConsolAWBSpecialHandling>().FirstOrDefault();
			AssertEquals("The JKH code of the first or default SpecialHandlingItems should remain unchanged", defaultSpecialHandling.JKH_Code, defaultJKHCode);
			AssertEquals("Duplicated item should not be added.", consol.AWBSpecialHandlingItems.Cast<JobConsolAWBSpecialHandling>().Count(), 1);
		}

		public void TestAWBSpecialHandlingItems()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var specialHandlingItem1 = consol.AWBSpecialHandlingItems.AddNew();
			specialHandlingItem1.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.ConsignmentEstablishedWithAnElectronicallyConcludedCargoContractEccWithNoAccompanyingPaperAirWaybill;
			AssertEquals("Last handling item should be the newly added one.", consol.AWBSpecialHandlingItems.Cast<JobConsolAWBSpecialHandling>().Last().JKH_Code, AWBSpecialHandlingCodeDescriptionPairList.Codes.ConsignmentEstablishedWithAnElectronicallyConcludedCargoContractEccWithNoAccompanyingPaperAirWaybill);

			var specialHandlingItem2 = consol.AWBSpecialHandlingItems.AddNew();
			specialHandlingItem2.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.HuntingTrophiesSkinHideAndAllArticlesMadeFromOrContainingPartsOfSpeciesListedInTheCitesConventionOnInternationalTradeInEndangeredSpeciesAppendices;
			AssertEquals("Last handling item should be the newly added one.", consol.AWBSpecialHandlingItems.Cast<JobConsolAWBSpecialHandling>().Last().JKH_Code, AWBSpecialHandlingCodeDescriptionPairList.Codes.HuntingTrophiesSkinHideAndAllArticlesMadeFromOrContainingPartsOfSpeciesListedInTheCitesConventionOnInternationalTradeInEndangeredSpeciesAppendices);

			consol.Delete();
			Assert("handling item should be deleted when consol is deleted", specialHandlingItem1.IsDeleted);
			Assert("handling item should be deleted when consol is deleted", specialHandlingItem2.IsDeleted);
		}

		public void TestAWBSpecialHandlingItemsFromShipment()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();

			var shipment = consol.Shipments.AddNew();
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, container1);
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.SetContainer(consol, container2);

			var undg1ForPackLine1 = packLine1.UNDGs.AddNew();
			undg1ForPackLine1.DI_DG = CreateUNDGSubstance("0001", "RFL").PK;
			var undg2ForPackLine1 = packLine1.UNDGs.AddNew();
			undg2ForPackLine1.DI_DG = CreateUNDGSubstance("0002", "ICE MAG").PK;

			var undg1ForPackLine2 = packLine2.UNDGs.AddNew();
			undg1ForPackLine2.DI_DG = CreateUNDGSubstance("0003", "RCX").PK;
			var undg2ForPackLine2 = packLine2.UNDGs.AddNew();
			undg2ForPackLine2.DI_DG = CreateUNDGSubstance("0004", "").PK;

			consol.DefaultSpecialHandlingItems();

			Assert(consol.AWBSpecialHandlingItems.Cast<NonSecurityJobConsolAWBSpecialHandling>().Select(x => x.JKH_Code).OrderBy(x => x).SequenceEqual(new ZString[] { "ICE", "MAG", "RCX", "RFL" }));

			packLine2.UNDGs.RemoveAllFromRelationship();
			consol.DefaultSpecialHandlingItems();
			Assert(consol.AWBSpecialHandlingItems.Cast<NonSecurityJobConsolAWBSpecialHandling>().Select(x => x.JKH_Code).OrderBy(x => x).SequenceEqual(new ZString[] { "ICE", "MAG", "RCX", "RFL" }));
			AssertHasWarning(consol.AWBSpecialHandlingItems.Cast<NonSecurityJobConsolAWBSpecialHandling>().FirstOrDefault(x => x.JKH_Code == "RCX").JKH_CodeInfo, "Check if this Special Handling Code is still relevant as it does not exist on linked Shipment's Dangerous Goods Substances.");
		}

		public void TestAWBSpecialHandlingItemsIsAWBHeaderAccessibleOnly()
		{
			AddMawb(Factory, "176", "10000001", GlbBranch.CurrentBranch, "STD");
			var carrier = RefAirline.LoadFromAirlinePrefix(Factory, "176");
			carrier.HasSignedEAWBAgreement = true;
			var rule = carrier.EFreightStatusCollection.AddNew();
			rule.RME_OriginLocation = HomePort;
			rule.RME_DestinationLocation = OverseasPort;
			rule.RME_EFreightStatus = Constants.EFreightStatus.Code.ECC;
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = "AGT";
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "17610000001";
			consol.JK_IsNeutralMaster = true;
			consol.MasterBillAirlinePrefix = "176";
			AssertNoExceptionThrown(() => consol.DefaultSpecialHandlingItems());

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "17610000001";
			AssertNoExceptionThrown(() => consol.DefaultSpecialHandlingItems());

			consol.JK_AgentType = Constants.AgentType.AWBMaster;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "17610000001";
			AssertEquals("Precondition: IsMultiAWBMaster", true, consol.IsMultiAWBMaster);
			AssertNoExceptionThrown(() => consol.DefaultSpecialHandlingItems());

			consol.JK_AgentType = Constants.AgentType.Courier;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "17610000001";
			AssertEquals("Precondition: IsCourier", true, consol.IsCourier);
			AssertNoExceptionThrown(() => consol.DefaultSpecialHandlingItems());
		}

		public void TestAWBSpecialHandlingItemsFromShipmentAndAirTransport()
		{
			AddMawb(Factory, "176", "10000001", GlbBranch.CurrentBranch, "STD");
			var carrier = RefAirline.LoadFromAirlinePrefix(Factory, "176");
			carrier.HasSignedEAWBAgreement = true;
			var rule = carrier.EFreightStatusCollection.AddNew();
			rule.RME_OriginLocation = HomePort;
			rule.RME_DestinationLocation = OverseasPort;
			rule.RME_EFreightStatus = Constants.EFreightStatus.Code.ECC;
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = "AGT";
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "17610000001";
			consol.JK_IsNeutralMaster = true;
			consol.MasterBillAirlinePrefix = "176";
			var container = consol.Containers.AddNew();

			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(consol, container);

			var undg2ForPackLine1 = packLine.UNDGs.AddNew();
			undg2ForPackLine1.DI_DG = CreateUNDGSubstance("0002", "ICE MAG").PK;

			consol.DefaultSpecialHandlingItems();

			var mannualSpecialHandlingItem = consol.AWBSpecialHandlingItems.AddNew();
			mannualSpecialHandlingItem.JKH_Code = "COM";

			var eFreightStatus = consol.GetEFreightStatus();
			AssertNoWarning(consol.AWBSpecialHandlingItems.Cast<NonSecurityJobConsolAWBSpecialHandling>().FirstOrDefault(x => x.JKH_Code == eFreightStatus).JKH_CodeInfo, "Check if this Special Handling Code is still relevant as it does not exist on linked Shipment's Dangerous Goods Substances.");
			AssertHasWarning(consol.AWBSpecialHandlingItems.Cast<NonSecurityJobConsolAWBSpecialHandling>().FirstOrDefault(x => x.JKH_Code == "COM").JKH_CodeInfo, "Check if this Special Handling Code is still relevant as it does not exist on linked Shipment's Dangerous Goods Substances.");
		}

		public void TestCloneInternal_AWBSpecialHandlingItemsFromCopiedConsol()
		{
			var consolWithSpecialHandlingItems = Factory.New<ForwardingConsol>();
			consolWithSpecialHandlingItems.JK_TransportMode = Constants.TransportModes.Air;

			var specialHandlingItem1 = consolWithSpecialHandlingItems.AWBSpecialHandlingItems.AddNew();
			var specialHandlingItem2 = consolWithSpecialHandlingItems.AWBSpecialHandlingItems.AddNew();
			var specialHandlingItem3 = consolWithSpecialHandlingItems.AWBSpecialHandlingItems.AddNew();
			var specialHandlingItem4 = consolWithSpecialHandlingItems.AWBSpecialHandlingItems.AddNew();
			var specialHandlingItem5 = consolWithSpecialHandlingItems.AWBSpecialHandlingItems.AddNew();

			specialHandlingItem1.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.LiveAnimal;
			specialHandlingItem2.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.Oxidizer;
			specialHandlingItem3.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.HatchingEggs;
			specialHandlingItem4.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.PerishableCargo;
			specialHandlingItem5.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.SaveHumanLife;

			var copiedConsolWithSpecialHandlingItems = (ForwardingConsol)consolWithSpecialHandlingItems.Clone();
			AssertEquals(copiedConsolWithSpecialHandlingItems.AWBSpecialHandlingItems.Count, 5);
			AssertContainsExactElementsInAnyOrder("Assert the Special Handling Item Codes from the copied consol match the original", new ZString[] { "AVI", "HEG", "PER", "ROX", "SHL" }, copiedConsolWithSpecialHandlingItems.AWBSpecialHandlingItems.Cast<NonSecurityJobConsolAWBSpecialHandling>().Select(x => x.JKH_Code));

			var consolWithNoSpecialHandlingItems = Factory.New<ForwardingConsol>();
			consolWithNoSpecialHandlingItems.JK_TransportMode = Constants.TransportModes.Air;

			var copiedConsolWithNoSpecialHandlingItems = (ForwardingConsol)consolWithNoSpecialHandlingItems.Clone();
			Assert(copiedConsolWithNoSpecialHandlingItems.AWBSpecialHandlingItems.IsNullOrEmpty());
		}

		const string MasterBillNumber = "MAWB002";

		public void TestAWBSpecialHandlingItemsAddNonSecurityItemsToConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_MasterBillNum = MasterBillNumber;

			Factory.Save();

			ValidateSpecialHandlingItems("Constructed", consol, "");

			var consol2 = ReloadConsol();

			ValidateSpecialHandlingItems("Reloaded", consol2, "");

			consol2.AWBSpecialHandlingItems.AddNew().JKH_Code = "CRT";
			consol2.AWBSpecialHandlingItems.AddNew().JKH_Code = "EAP";

			consol2.Factory.Save();

			ValidateSpecialHandlingItems("Modified", consol2, "", "CRT", "EAP");

			var consol3 = ReloadConsol();

			ValidateSpecialHandlingItems("Reload after modification", consol3, "", "CRT", "EAP");
		}

		public void TestAWBSpecialHandlingItemsAddedSecurityItemToConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_MasterBillNum = MasterBillNumber;

			Factory.Save();

			var consol2 = ValidateSpecialHandlingItems("Constructed", consol, "");

			consol2.SecurityStatusCode = "SCO";

			consol2.AWBSpecialHandlingItems.AddNew().JKH_Code = "CRT";
			consol2.AWBSpecialHandlingItems.AddNew().JKH_Code = "EAP";

			consol2.Factory.Save();

			ValidateSpecialHandlingItems("Modified", consol2, "SCO", "CRT", "EAP");
		}

		public void TestAWBSpecialHandlingItemsModifySecurityItemInConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_MasterBillNum = MasterBillNumber;

			consol.SecurityStatusCode = "SCO";

			consol.AWBSpecialHandlingItems.AddNew().JKH_Code = "CRT";
			consol.AWBSpecialHandlingItems.AddNew().JKH_Code = "EAP";

			Factory.Save();

			var consol2 = ValidateSpecialHandlingItems("Constructed", consol, "SCO", "CRT", "EAP");

			consol2.SecurityStatusCode = "NSC";

			var crt = consol2.AWBSpecialHandlingItems.Find(x => x.JKH_Code == "CRT").FirstOrDefault();
			consol2.AWBSpecialHandlingItems.RemoveAndDelete(crt);

			consol2.AWBSpecialHandlingItems.AddNew().JKH_Code = "QRT";

			consol2.Factory.Save();

			ValidateSpecialHandlingItems("Modified", consol2, "NSC", "EAP", "QRT");
		}

		public void TestAWBSpecialHandlingItemsDeleteSecurityItemInConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_MasterBillNum = MasterBillNumber;

			consol.SecurityStatusCode = "SCO";

			consol.AWBSpecialHandlingItems.AddNew().JKH_Code = "CRT";
			consol.AWBSpecialHandlingItems.AddNew().JKH_Code = "EAP";

			Factory.Save();

			var consol2 = ValidateSpecialHandlingItems("Constructed", consol, "SCO", "CRT", "EAP");

			consol2.SecurityStatusCode = "";

			var crt = consol2.AWBSpecialHandlingItems.Find(x => x.JKH_Code == "CRT").FirstOrDefault();
			consol2.AWBSpecialHandlingItems.RemoveAndDelete(crt);

			consol2.Factory.Save();

			ValidateSpecialHandlingItems("Modified", consol2, "", "EAP");
		}

		public void TestAWBSpecialHandlingItemsClearSecurityItemInConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_MasterBillNum = MasterBillNumber;

			consol.SecurityStatusCode = "SCO";

			consol.AWBSpecialHandlingItems.AddNew().JKH_Code = "CRT";
			consol.AWBSpecialHandlingItems.AddNew().JKH_Code = "EAP";

			Factory.Save();

			var consol2 = ValidateSpecialHandlingItems("Constructed", consol, "SCO", "CRT", "EAP");

			consol2.SecurityStatusCode = "";

			var crt = consol2.AWBSpecialHandlingItems.Find(x => x.JKH_Code == "CRT").FirstOrDefault();
			consol2.AWBSpecialHandlingItems.RemoveAndDelete(crt);

			var eap = consol2.AWBSpecialHandlingItems.Find(x => x.JKH_Code == "EAP").FirstOrDefault();
			consol2.AWBSpecialHandlingItems.RemoveAndDelete(eap);

			consol2.Factory.Save();

			ValidateSpecialHandlingItems("Modified", consol2, "");
		}

		static ForwardingConsol ValidateSpecialHandlingItems(string message, ForwardingConsol consol, ZString expectedSecurityItem, params ZString[] expectedNonSecurityItems)
		{
			var sorted = consol.AWBSpecialHandlingItems.Select(x => x.JKH_Code).OrderBy(x => x).ToArray();
			AssertArrayEqualsByElements(message + "/non security items", expectedNonSecurityItems, sorted);
			AssertEquals(message + "/security item", expectedSecurityItem, consol.SecurityStatusCode);

			var consol2 = ReloadConsol();
			var sorted2 = consol2.AWBSpecialHandlingItems.Select(x => x.JKH_Code).OrderBy(x => x).ToArray();
			AssertArrayEqualsByElements(message + "&Reloaded/non security items", expectedNonSecurityItems, sorted2);
			AssertEquals(message + "&Reloaded/security item", expectedSecurityItem, consol2.SecurityStatusCode);

			return consol2;
		}

		static ForwardingConsol ReloadConsol()
		{
			var factory = new BusinessObjectFactory();
			var consol = factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, MasterBillNumber));
			return consol;
		}

		public void TestCloneInternal_SecurityStatusFromCopiedConsol()
		{
			var consolWithSpecialHandlingItems = Factory.New<ForwardingConsol>();
			consolWithSpecialHandlingItems.JK_TransportMode = Constants.TransportModes.Air;
			var securityStatusCode1 = consolWithSpecialHandlingItems.AWBSpecialHandlingItems.AddNew();

			securityStatusCode1.JKH_Code = "SCO";

			var copiedConsolWithSpecialHandlingItems = (ForwardingConsol)consolWithSpecialHandlingItems.Clone();
			AssertEquals("Assert the Security Status Code from the copied consol matches the original",
					"SCO",
					copiedConsolWithSpecialHandlingItems.SecurityStatusCode);

			var consolWithNoSpecialHandlingItems = Factory.New<ForwardingConsol>();
			consolWithNoSpecialHandlingItems.JK_TransportMode = Constants.TransportModes.Air;
			var copiedConsolWithNoSpecialHandlingItems = (ForwardingConsol)consolWithNoSpecialHandlingItems.Clone();

			AssertEquals(copiedConsolWithNoSpecialHandlingItems.SecurityStatusCode.IsEmpty, true);
		}

		UNDGSubstance CreateUNDGSubstance(string code, string specialHandlingCodes)
		{
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = code.Substring(0, 4);
			substance.DG_Code = code;
			substance.DG_Standard = UNDGSubstanceStandardTypes.IATA;
			substance.DG_SpecialHandlingCodes = specialHandlingCodes;
			return substance;
		}

		#endregion

		#region SecurityStatusCode

		public void TestSecurityStatusCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.HongKong))
			{
				var accountConsignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "GLOKWN"));
				var knownShipperDetails = accountConsignor.MainAddress.KnownShipperDetails.AddNew();
				knownShipperDetails.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;
				knownShipperDetails.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
				knownShipperDetails.OV_OH_OrgHeader = accountConsignor.PK;

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;

				var shipment = consol.Shipments.AddNew();
				shipment.ConsignorDocumentaryAddress.OrganisationPK = accountConsignor.PK;
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "HKHKG";
				shipment.JS_RL_NKDestination = "DEHAM";
				shipment.JS_InspectionTypeCode = "APP";

				var transport = consol.Transports[0];
				transport.JW_RL_NKLoadPort = "HKHKG";
				transport.JW_RL_NKDiscPort = "DEHAM";
				transport.JW_TransportMode = Constants.TransportModes.Air;

				Factory.Save();

				Assert("Precondition", !shipment.AviationSecurity.RelevantOrganisationsAreApprovedForShippingOnPassengerFlights);
				AssertEquals("Security status should default to 'SCO' as there is a shipment where the relevent org isn't for passenger flight shipping",
					"SCO",
					consol.SecurityStatusCode);

				Assert("No JobConsolAWBSpecialHandling created when get from defaulting", !Factory.Load<JobConsolAWBSpecialHandling>(new ZQuery()).Any());

				consol.SecurityStatusCode = "SPX";

				var specialHandling = Factory.LoadTop1<SecurityJobConsolAWBSpecialHandling>(new ZQuery());
				AssertNotNull("JobConsolAWBSpecialHandling is created when override security status", specialHandling);

				consol.SecurityStatusCode = "SCO";
				Assert("JobConsolAWBSpecialHandling should be deleted when reset security status as default", specialHandling.IsDeleted);

				Factory.Save();

				consol.SecurityStatusCode = "SPX";

				specialHandling = Factory.LoadTop1<SecurityJobConsolAWBSpecialHandling>(new ZQuery());
				AssertNotNull("JobConsolAWBSpecialHandling is created when override security status", specialHandling);

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var reloadedConsol = newFactory.Load<ForwardingConsol>(consol.PK);
				specialHandling = newFactory.LoadTop1<SecurityJobConsolAWBSpecialHandling>(new ZQuery());

				AssertEquals("SPX", reloadedConsol.SecurityStatusCode);
				Assert("JobConsolAWBSpecialHandling is saved", specialHandling.IsInDatabase);
				AssertEquals("SecurityStatusCode is from dbo.JobConsolAWBSpecialHandling", "SPX", specialHandling.JKH_Code);
			}
		}

		public void TestSecurityStatusCode_US()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				var knownShipper = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "USLAX"));
				var knownShipperDetails = knownShipper.MainAddress.KnownShipperDetails.AddNew();
				knownShipperDetails.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				knownShipperDetails.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
				knownShipperDetails.OV_OH_OrgHeader = knownShipper.PK;

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;

				var shipment = consol.Shipments.AddNew();
				shipment.ConsignorDocumentaryAddress.OrganisationPK = knownShipper.PK;
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "USLAX";
				shipment.JS_RL_NKDestination = "DEHAM";
				shipment.JS_InspectionTypeCode = "APP";

				var transport = consol.Transports[0];
				transport.JW_RL_NKLoadPort = "USLAX";
				transport.JW_RL_NKDiscPort = "DEHAM";
				transport.JW_TransportMode = Constants.TransportModes.Air;

				Factory.Save();

				Assert("Precondition", !shipment.AviationSecurity.RelevantOrganisationsAreApprovedForShippingOnPassengerFlights);
				AssertEquals("Security status should default to 'NSC' for US companies, even where shipper is known and shipment is APP",
					"NSC",
					consol.SecurityStatusCode);
			}
		}

		public void TestSecurityStatusCode_Empty()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			Factory.Save();

			consol.SecurityStatusCode = "SPX";

			Factory.Save();

			consol.SecurityStatusCode = string.Empty;

			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestSecurityStatusCode_EmptyNotInDatabase()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.SecurityStatusCode = "SPX";

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			consol.SecurityStatusCode = string.Empty;

			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestSecurityStatusCode_NotDefaultingWhenOverriden()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.HongKong))
			{
				var accountConsignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "GLOKWN"));
				var knownShipperDetails = accountConsignor.MainAddress.KnownShipperDetails.AddNew();
				knownShipperDetails.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;
				knownShipperDetails.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
				knownShipperDetails.OV_OH_OrgHeader = accountConsignor.PK;

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;

				consol.SecurityStatusCode = "SPX";

				Factory.Save();

				var shipment = consol.Shipments.AddNew();
				shipment.ConsignorDocumentaryAddress.OrganisationPK = accountConsignor.PK;
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "HKHKG";
				shipment.JS_RL_NKDestination = "DEHAM";
				shipment.JS_InspectionTypeCode = "APP";

				var transport = consol.Transports[0];
				transport.JW_RL_NKLoadPort = "HKHKG";
				transport.JW_RL_NKDiscPort = "DEHAM";
				transport.JW_TransportMode = Constants.TransportModes.Air;

				Factory.Save();

				Assert("Precondition", !shipment.AviationSecurity.RelevantOrganisationsAreApprovedForShippingOnPassengerFlights);
				AssertEquals("Security status should NOT default to 'SCO' as security status has been overriden as 'SPX'", "SPX", consol.SecurityStatusCode);
			}
		}

		public void TestSecurityStatusCode_RefreshedWhenShipmentCountChanged()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.HongKong))
			{
				var accountConsignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "GLOKWN"));
				var knownShipperDetails = accountConsignor.MainAddress.KnownShipperDetails.AddNew();
				knownShipperDetails.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;
				knownShipperDetails.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
				knownShipperDetails.OV_OH_OrgHeader = accountConsignor.PK;

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;

				var transport = consol.Transports[0];
				transport.JW_RL_NKLoadPort = "HKHKG";
				transport.JW_RL_NKDiscPort = "DEHAM";
				transport.JW_TransportMode = Constants.TransportModes.Air;

				Factory.Save();

				AssertEquals("Security status should be empty by default", string.Empty, consol.SecurityStatusCode);

				var shipment = consol.Shipments.AddNew();
				shipment.ConsignorDocumentaryAddress.OrganisationPK = accountConsignor.PK;
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "HKHKG";
				shipment.JS_RL_NKDestination = "DEHAM";
				shipment.JS_InspectionTypeCode = "APP";

				Assert("Precondition", !shipment.AviationSecurity.RelevantOrganisationsAreApprovedForShippingOnPassengerFlights);
				AssertEquals("Re-calculated as SCO as there is a shipment where the relevent org isn't for passenger flight shipping", "SCO", consol.SecurityStatusCode);
			}
		}

		public void TestSecurityStatusCode_RefreshedWhenShipmentInspectionStatusChanged()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.HongKong))
			{
				var accountConsignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "GLOKWN"));
				var knownShipperDetails = accountConsignor.MainAddress.KnownShipperDetails.AddNew();
				knownShipperDetails.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;
				knownShipperDetails.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
				knownShipperDetails.OV_OH_OrgHeader = accountConsignor.PK;

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;

				var shipment = consol.Shipments.AddNew();
				shipment.ConsignorDocumentaryAddress.OrganisationPK = accountConsignor.PK;
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "HKHKG";
				shipment.JS_RL_NKDestination = "DEHAM";
				shipment.JS_InspectionTypeCode = "APP";

				var transport = consol.Transports[0];
				transport.JW_RL_NKLoadPort = "HKHKG";
				transport.JW_RL_NKDiscPort = "DEHAM";
				transport.JW_TransportMode = Constants.TransportModes.Air;

				Factory.Save();

				Assert("Precondition", !shipment.AviationSecurity.RelevantOrganisationsAreApprovedForShippingOnPassengerFlights);
				AssertEquals("Security status should default to 'SCO' as there is a shipment where the relevent org isn't for passenger flight shipping",
					"SCO",
					consol.SecurityStatusCode);

				shipment.JS_InspectionTypeCode = "UNK";

				AssertEquals("Security status changed as 'NSC' when at least one Shipment attached to the consol has Inspection 'UNK'", "NSC", consol.SecurityStatusCode);
			}
		}

		public void TestSecurityStatusCode_MaxLength()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			AssertEquals("MaxLength", 3, consol.SecurityStatusCodeInfo.MaxLength);
		}

		public void TestSecurityStatusList()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			var validCodes = new[]
			{
				AWBSpecialHandlingCodeDescriptionPairList.Codes.SecureForPassengerAllCargoAndAllMailAircraftInAccordanceWithHighRiskRequirements,
				AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft,
				AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft,
				AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly,
				SecurityJobConsolAWBSpecialHandling.NotSecured
			};

			AssertContainsExactElementsInAnyOrder(validCodes, consol.SecurityStatusList.GetAllCodes());
		}

		public void TestSecurityStatusCodeRemainsUnchanged_WhenSetToSPXOnSave()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "VNVNH";
			consol.SecurityStatusCode = "NSC";

			Factory.Save();

			Factory.Saving += delegate (BusinessObjectFactory factory)
			{
				consol.SecurityStatusCode = "SPX";
			};
			Factory.SetValue<ISecuredFreightVerificationChecker, FreightVerifiedStub>();
			Factory.Save();

			AssertEquals(false, ((FreightVerifiedStub)Factory.GetValue<ISecuredFreightVerificationChecker>()).StubCalled);
			AssertEquals("SecurityStatusCode remains unchanged.", "NSC", consol.SecurityStatusCode);
		}

		#endregion

		public void TestAddRemoveOrInsertPartyScreeningLogWhenSavingConsole()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "Org1";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "Org2";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();

			consol.JK_OA_ShippingLineAddress = org1.MainAddress.PK;
			Factory.Save();

			var screeningStatus1 = consol.RelatedOrgPartyScreeningStatusCollection.ToList<IRelatedOrgPartyScreeningStatus>();
			CombineAssertions(() =>
			{
				AssertEquals(1, screeningStatus1.Count);
				AssertEquals(true, screeningStatus1.Any(u => u.PJ_Status == "PAA" && u.PJ_ClearedReason == "Parties Info:Org1(Carrier)"));
			});

			consol.JK_OA_ShippingLineAddress = org2.MainAddress.PK;
			Factory.Save();

			var screeningStatus2 = consol.RelatedOrgPartyScreeningStatusCollection.ToList<IRelatedOrgPartyScreeningStatus>();
			CombineAssertions(() =>
			{
				AssertEquals(true, screeningStatus2.Any(u => u.PJ_Status == "PAA" && u.PJ_ClearedReason == "Parties Info:Org2(Carrier)"));
				AssertEquals(true, screeningStatus2.Any(u => u.PJ_Status == "PAR" && u.PJ_ClearedReason == "Parties Info:Org1(Carrier)"));
			});
		}

		public void TestSetCreditorAsEmptyWhenAgentTypeFromOrToCLD()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_IsCreditor = true;
			orgHeader.OH_IsForwarder = true;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_OA_CreditorAddress = orgHeader.MainAddress.PK;
			consol.JK_CoLoadBookingReference = "BOOKREF";
			consol.JK_CoLoadMasterBill = "TEST123456";

			AssertEquals(orgHeader, consol.Creditor);
			AssertEquals("BOOKREF", consol.JK_CoLoadBookingReference);
			AssertEquals("TEST123456", consol.JK_CoLoadMasterBill);

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			AssertEquals(ZGuid.Empty, consol.JK_OA_CreditorAddress);
			AssertNull(consol.Creditor);
			AssertEquals(ZString.Empty, consol.JK_CoLoadBookingReference);
			AssertEquals(ZString.Empty, consol.JK_CoLoadMasterBill);

			consol.JK_OA_CreditorAddress = orgHeader.MainAddress.PK;
			consol.JK_CoLoadBookingReference = "BOOKREF1";
			consol.JK_CoLoadMasterBill = "TEST1234567";

			AssertEquals(orgHeader, consol.Creditor);
			AssertEquals("BOOKREF1", consol.JK_CoLoadBookingReference);
			AssertEquals("TEST1234567", consol.JK_CoLoadMasterBill);

			consol.JK_AgentType = Constants.AgentType.Agent;
			AssertEquals(ZGuid.Empty, consol.JK_OA_CreditorAddress);
			AssertNull(consol.Creditor);
			AssertEquals(ZString.Empty, consol.JK_CoLoadBookingReference);
			AssertEquals(ZString.Empty, consol.JK_CoLoadMasterBill);
		}

		public void TestSetCreditorAsEmptyWhenAgentTypeFromOrToGatewayCoLoad()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_IsCreditor = true;
			orgHeader.OH_IsForwarder = true;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_OA_CreditorAddress = orgHeader.MainAddress.PK;
			consol.JK_CoLoadBookingReference = "BOOKREF";
			consol.JK_CoLoadMasterBill = "TEST123456";

			AssertEquals(orgHeader, consol.Creditor);
			AssertEquals("BOOKREF", consol.JK_CoLoadBookingReference);
			AssertEquals("TEST123456", consol.JK_CoLoadMasterBill);

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			AssertEquals(ZGuid.Empty, consol.JK_OA_CreditorAddress);
			AssertNull(consol.Creditor);
			AssertEquals(ZString.Empty, consol.JK_CoLoadBookingReference);
			AssertEquals(ZString.Empty, consol.JK_CoLoadMasterBill);

			consol.JK_OA_CreditorAddress = orgHeader.MainAddress.PK;
			consol.JK_CoLoadBookingReference = "BOOKREF1";
			consol.JK_CoLoadMasterBill = "TEST1234567";

			AssertEquals(orgHeader, consol.Creditor);
			AssertEquals("BOOKREF1", consol.JK_CoLoadBookingReference);
			AssertEquals("TEST1234567", consol.JK_CoLoadMasterBill);

			consol.JK_AgentType = Constants.AgentType.Agent;
			AssertEquals(ZGuid.Empty, consol.JK_OA_CreditorAddress);
			AssertNull(consol.Creditor);
			AssertEquals(ZString.Empty, consol.JK_CoLoadBookingReference);
			AssertEquals(ZString.Empty, consol.JK_CoLoadMasterBill);
		}

		public void TestKeepCreditorWhenAgentTypeChangesBetweenCoLoadAndGatewayCoLoad()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_IsCreditor = true;
			orgHeader.OH_IsForwarder = true;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			AssertEquals(ZGuid.Empty, consol.JK_OA_CreditorAddress);
			AssertNull(consol.Creditor);

			consol.JK_OA_CreditorAddress = orgHeader.MainAddress.PK;
			consol.JK_CoLoadBookingReference = "BOOKREF";
			consol.JK_CoLoadMasterBill = "TEST123456";

			AssertEquals(orgHeader, consol.Creditor);
			AssertEquals("BOOKREF", consol.JK_CoLoadBookingReference);
			AssertEquals("TEST123456", consol.JK_CoLoadMasterBill);

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			AssertEquals(orgHeader, consol.Creditor);
			AssertEquals("BOOKREF", consol.JK_CoLoadBookingReference);
			AssertEquals("TEST123456", consol.JK_CoLoadMasterBill);
		}

		public void TestJobHeaderJobNumberNoSuffixForGateways()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "GBDTE";
			Factory.Save();

			AssertEquals("C00001000", ((IJobNumber)consol).JobNumber);

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_RL_NKLoadPort = "GBDTE";
			Factory.Save();

			AssertEquals("C00001001", ((IJobNumber)consol2).JobNumber);

			var consol3 = Factory.New<ForwardingConsol>();
			consol3.JK_TransportMode = Constants.TransportModes.Sea;
			consol3.JK_AgentType = Constants.AgentType.Agent;
			consol3.JK_RL_NKLoadPort = "GBDTE";
			consol3.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol3.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var port = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			port.O5_PortOrCountry = "GBDTE";
			port.O5_OA_AgentOfficeAddress = consol3.JK_OA_SendingForwarderAddress;
			port.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			consol3.SendingForwarder.AppointedGatewayAgentPorts.Add(port);

			Factory.Save();

			AssertEquals(true, ((IGateway)consol3).GatewayBillingSupporter.IsGatewayBillingEnabled());
			AssertEquals("C00001002", ((IJobNumber)consol3).JobNumber);
		}

		public void TestIServiceLocatorICustomsChargesForNZ()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			IServiceLocator serviceLocator = consol;
			AssertNull(serviceLocator.GetService(typeof(ForwardingShipment)));
			AssertEquals(ObjectFactory.GetType<NZ.IConsolCustomsCharges>(), serviceLocator.GetService(typeof(ICustomsCharges)).GetType());
		}

		public void TestPrintedLogReferencesAreNonTranslatable()
		{
			var resourceDemandedHandler = new EventHandler<ResourceStringDemandedEventArgs>((s, e) =>
			{
				Fail("PrintedLogReferences must be constants and should not access ResourceStrings");
			});

			Res.ResourceDemanded += resourceDemandedHandler;

			try
			{
				AssertEquals("MAWB Printed", ForwardingConsol.MAWBPrintedLogReference);
				AssertEquals("HAWB Printed", ForwardingConsol.HAWBPrintedLogReference);
			}
			finally
			{
				Res.ResourceDemanded -= resourceDemandedHandler;
			}
		}

		public void TestCanCancel()
		{
			var consol = Factory.New<ForwardingConsol>();
			var afrHeader1 = Factory.New<JP.AFR.IJPAFRHeader>();
			afrHeader1.JPH_ParentId = consol.PK;
			afrHeader1.JPH_ParentTableCode = consol.TablePrefix;
			afrHeader1.JPH_JobReference = "";
			afrHeader1.JPH_MessageStatus = "AHC";
			AssertEquals(@"This record cannot be deactivated as one of its related records cannot be deactivated due to the following reason.
Advance Filing Rules: This job may not be deactivated because status indicates that the Advance Filing Rules portion of the job is active with Japan Customs. The bills would need to be deleted from Japan Customs before the job is deactivated.",
				consol.CanCancel());
		}

		public void TestCancellation()
		{
			var consol = Factory.New<ForwardingConsol>();
			var mawb1 = Factory.New<IAUCusMAWB>();
			mawb1.CM_JK = consol.PK;
			var mawb2 = Factory.New<IAUCusMAWB>();
			mawb2.CM_JK = consol.PK;
			var inbondHeader1 = Factory.New<US.USAMS.ICusInBondHeader>();
			inbondHeader1.BH_ParentID = consol.PK;
			inbondHeader1.BH_ParentTableCode = consol.TablePrefix;
			var inbondHeader2 = Factory.New<US.USAMS.ICusInBondHeader>();
			inbondHeader2.BH_ParentID = consol.PK;
			inbondHeader2.BH_ParentTableCode = consol.TablePrefix;
			var afrHeader1 = Factory.New<JP.AFR.IJPAFRHeader>();
			afrHeader1.JPH_ParentId = consol.PK;
			afrHeader1.JPH_ParentTableCode = consol.TablePrefix;
			var afrHeader2 = Factory.New<JP.AFR.IJPAFRHeader>();
			afrHeader2.JPH_ParentId = consol.PK;
			afrHeader2.JPH_ParentTableCode = consol.TablePrefix;

			consol.IsCancelled = true;
			Assert(mawb1.IsCancelled);
			Assert(mawb2.IsCancelled);
			Assert(inbondHeader1.IsCancelled);
			Assert(inbondHeader2.IsCancelled);
			Assert(afrHeader1.IsCancelled);
			Assert(afrHeader2.IsCancelled);

			consol.IsCancelled = false;
			Assert(!mawb1.IsCancelled);
			Assert(!mawb2.IsCancelled);
			Assert(!inbondHeader1.IsCancelled);
			Assert(!inbondHeader2.IsCancelled);
			Assert(!afrHeader1.IsCancelled);
			Assert(!afrHeader2.IsCancelled);
		}

		public void TestUSAMS()
		{
			var consol = Factory.New<ForwardingConsol>();
			var usAMS = Factory.New<US.USAMS.ICusInBondHeader>();
			usAMS.BH_ParentID = consol.PK;
			usAMS.BH_ParentTableCode = consol.TablePrefix;
			AssertEquals(usAMS.PK, consol.USAMS.PK);
		}

		public void TestMasterBillMAWB_ReadOnly()
		{
			JobMawb mawb = NewMawb("081", "00000033");

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consol.JK_AgentType = Constants.AgentType.AWBMaster;
			consol.JK_IsNeutralMaster = true;
			AssertEquals("Agent Type :  AWBMaster", false, consol.MasterBillMAWBInfo.ReadOnly);

			consol.JK_AgentType = Constants.AgentType.Charter;
			consol.JK_IsNeutralMaster = true;
			AssertEquals("Agent Type :  Charter", false, consol.MasterBillMAWBInfo.ReadOnly);

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_IsNeutralMaster = true;
			AssertEquals("Agent Type :  CoLoad", false, consol.MasterBillMAWBInfo.ReadOnly);

			consol.JK_AgentType = Constants.AgentType.OnBoardCourier;
			consol.JK_IsNeutralMaster = true;
			AssertEquals("Agent Type :  OnBoardCourier", false, consol.MasterBillMAWBInfo.ReadOnly);

			consol.JK_AgentType = Constants.AgentType.Other;
			consol.JK_IsNeutralMaster = true;
			AssertEquals("Agent Type :  Other", false, consol.MasterBillMAWBInfo.ReadOnly);

			consol.JK_AgentType = Constants.AgentType.AWBCoload;
			consol.JK_IsNeutralMaster = true;
			AssertEquals("Agent Type :  AWBCoload", true, consol.MasterBillMAWBInfo.ReadOnly);

			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_IsNeutralMaster = true;
			AssertEquals("Agent Type :  Direct", true, consol.MasterBillMAWBInfo.ReadOnly);

			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_IsNeutralMaster = true;
			AssertEquals("Agent Type :  Agent", true, consol.MasterBillMAWBInfo.ReadOnly);

			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_IsNeutralMaster = false;
			AssertEquals("Agent Type :  Agent", false, consol.MasterBillMAWBInfo.ReadOnly);
		}

		public void TestSavingDeactivatedJob()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();

			var job = Factory.NewJobForTesting<JobHeader>();
			job.Parent = consol;
			job.JH_JobNum += Constants.GatewaySuffixForJobHeaderDeprecated;
			Factory.Save();
			Assert("consol.IsLegacyGateway", consol.Job.IsGatewayLegacyJob);

			var orgAppointedAgentPorts = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(orgAppointedAgentPorts);
			consol.CostSupporter.Shipments.Add(shipment);

			consol.Job.MarkAsInactive();

			AssertNoExceptionThrown("No exception is expected here", () => Factory.Save());
		}

		#region IAWBParent

		public void TestNewlyCreatedAWBHeaderDoesNotAffectHasChanges()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			Factory.Save();

			AssertEquals(false, consol.HasChanges);
			AssertNotNull("Touching AWBHeader - new one would be created", consol.AWBHeader);

			AssertEquals("AWBHeader's HasChanges forced to False", false, consol.AWBHeader.HasChanges);
			AssertEquals("Parent's HasChanges not affected", false, consol.HasChanges);
		}

		#endregion

		#region Shipments_ListFilterDefaults

		public void TestShipmentsListFilterDefaults()
		{
			string transportFilterKey = "Transport Mode" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property";
			string eTDToKey = "ETD" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property2";
			string eTAFromKey = "ETA" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1";

			ZDateTime currentTime = ZDateTime.Now;
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			Transport transport = Consol.Transports[0];
			Consol.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Consol.JK_RL_NKDischargePort = "USLAX";
			transport.JW_ETD = currentTime;
			transport.JW_ETA = currentTime.AddDays(1);
			AssertEquals("Shipments filter - Transport mode", Core.Constants.TransportModes.Air, Consol.Shipments_List.FilterBusinessObjectDefaults[transportFilterKey].Value);
			AssertEquals("Shipments filter - Date should be truncated", currentTime.Date, Consol.Shipments_List.FilterBusinessObjectDefaults[eTDToKey].Value);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			transport = Consol.Transports[0];
			Consol.JK_RL_NKLoadPort = "GBLON";
			Consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			AssertEquals("Consol Transport mode changed", Core.Constants.TransportModes.Sea, Consol.Shipments_List.FilterBusinessObjectDefaults[transportFilterKey].Value);
			AssertEquals("Consol changed to import - ETA should be truncated", currentTime.AddDays(1).Date, Consol.Shipments_List.FilterBusinessObjectDefaults[eTAFromKey].Value);
		}

		#endregion

		#region Flight Tracking Subscription

		public void TestSaving_SBREventLog_RegistryDisabled()
		{
			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var consol = GetNewConsolWithMAWB("AIRCONSOL", Constants.TransportModes.Air, "08187443521", "AUSYD", "SGSIN");

				var transport = consol.Transports[0];
				transport.JW_VoyageFlight = "QF001";
				transport.JW_ETD = new ZDateTime(2017, 01, 01);
				transport.JW_ETA = new ZDateTime(2017, 01, 02);

				Factory.Save();

				var logs = consol.Logs.GetAllLogs()
					.Cast<StmALog>()
					.Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code)
					.OrderByDescending(x => x.SL_EventTime)
					.ToList();

				AssertNotNull(logs);
				AssertEquals(0, logs.Count);
			}
		}

		public void TestSaving_CreateSBREventLog_AirConsol_NoLinkedTransport()
		{
			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = GetNewConsolWithMAWB("AIRCONSOL", Constants.TransportModes.Air, "08187443521", "AUSYD", "SGSIN");

				var transport = consol.Transports[0];
				transport.JW_IsLinked = false;
				transport.JW_VoyageFlight = "QF001";
				transport.JW_ETD = 3.DaysAgo();
				transport.JW_ETA = 1.DaysAgo();

				Factory.Save();
				AssertFlightTrackingEvent("Precondition: should not create new SBR event", consol, 1, "|RFN=08187443521|TYP=AWB Automation");

				consol.JK_MasterBillNum = "46197135463";
				Factory.Save();
				AssertFlightTrackingEvent("MAWB changed, should be created new SBR event", consol, 2, "|RFN=46197135463|TYP=AWB Automation");

				transport.JW_VoyageFlight = "QF002";
				Factory.Save();
				AssertFlightTrackingEvent("Flight number changed, should be created new SBR event", consol, 3, "|RFN=46197135463|TYP=AWB Automation");

				transport.JW_RL_NKLoadPort = "AUMEL";
				Factory.Save();
				AssertFlightTrackingEvent("Load Port changed, should not create new SBR event", consol, 4, "|RFN=46197135463|TYP=AWB Automation");

				transport.JW_RL_NKDiscPort = "NZAKL";
				Factory.Save();
				AssertFlightTrackingEvent("Discharge Port changed, should not create new SBR event", consol, 5, "|RFN=46197135463|TYP=AWB Automation");

				transport.JW_ETD = 2.DaysAgo().AddMinutes(1);
				Factory.Save();
				AssertFlightTrackingEvent("ETD date changed, should be created new SBR event", consol, 6, "|RFN=46197135463|TYP=AWB Automation");

				transport.JW_ETA = 2.DaysAgo().AddMinutes(2);
				Factory.Save();
				AssertFlightTrackingEvent("ETA date changed, should be created new SBR event", consol, 7, "|RFN=46197135463|TYP=AWB Automation");
			}
		}

		public void TestSaving_AirConsol_ShouldCreateSBREventLog_WithInvalidFlightNumber()
		{
			var consol = GetNewConsolWithMAWB("AIRCONSOL", Constants.TransportModes.Air, "08187443521", "AUSYD", "SGSIN");

			var transport = consol.Transports[0];
			transport.JW_IsLinked = false;
			transport.JW_VoyageFlight = "QFX001XX";
			transport.JW_ETD = 3.DaysAgo();
			transport.JW_ETA = 1.DaysAgo();

			Factory.Save();
			AssertFlightTrackingEvent("Should create SBR event, even with invalid flight number", consol, 1, "|RFN=08187443521|TYP=AWB Automation");
		}

		public void TestSaving_AirConsol_ShouldNotCreateSBREventLog_WithEmptyFlightNumber()
		{
			var consol = GetNewConsolWithMAWB("AIRCONSOL", Constants.TransportModes.Air, "08187443521", "AUSYD", "SGSIN");

			var transport = consol.Transports[0];
			transport.JW_IsLinked = false;
			transport.JW_VoyageFlight = string.Empty;
			transport.JW_ETD = new ZDateTime(2017, 01, 01);
			transport.JW_ETA = new ZDateTime(2017, 01, 02);

			Factory.Save();
			AssertFlightTrackingEvent("Flight number is empty, Should not create SBR event", consol, 0, null);
		}

		public void TestSaving_AirCoLoadConsol_ShouldNotCreateSBREventLog_WithEmptyCoLoadMasterBillAndBookingReference()
		{
			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = GetNewConsolWithMAWB("AIRCONSOL", Constants.TransportModes.Air, "08187443521", "AUSYD", "SGSIN");
				consol.JK_AgentType = Constants.AgentType.CoLoad;
				consol.JK_CoLoadMasterBill = ZString.Empty;
				consol.JK_CoLoadBookingReference = ZString.Empty;

				var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
				shippingLine.RSL_CargoWiseOneCode = "CODE";
				var coLoadWith = Factory.New<OrgHeader>();
				coLoadWith.OH_Code = "CREDITOR";
				coLoadWith.OH_RSL_ShippingLine = shippingLine.PK;

				consol.JK_OA_CreditorAddress = coLoadWith.MainAddress.PK;

				Factory.Save();
				AssertFlightTrackingEvent("CoLoad MBL and BookingRef are empty, Should not create SBR event", consol, 0, "|RFN=CLDMBL|TYP=AWB Automation");
			}
		}

		public void TestSaving_AirCoLoadConsol_ShouldNotCreateSBREventLog_WithEmptyCoLoadWithParty()
		{
			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = GetNewConsolWithMAWB("AIRCONSOL", Constants.TransportModes.Air, "08187443521", "AUSYD", "SGSIN");
				consol.JK_AgentType = Constants.AgentType.CoLoad;
				consol.JK_CoLoadMasterBill = "CLDMBL";
				consol.JK_CoLoadBookingReference = "CLDBR";

				Factory.Save();
				AssertFlightTrackingEvent("CoLoad with party is empty, Should not create SBR event", consol, 0, "|RFN=CLDMBL|TYP=AWB Automation");
			}
		}

		public void TestSaving_AirCoLoadConsol_ShouldCreateSBREventLog_WithNonEmptyCoLoadMasterBill()
		{
			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = GetNewConsolWithMAWB("AIRCONSOL", Constants.TransportModes.Air, "08187443521", "AUSYD", "SGSIN");
				consol.JK_AgentType = Constants.AgentType.CoLoad;
				consol.JK_CoLoadMasterBill = "CLDMBL";
				consol.JK_CoLoadBookingReference = ZString.Empty;

				var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
				shippingLine.RSL_CargoWiseOneCode = "CODE";
				var coLoadWith = Factory.New<OrgHeader>();
				coLoadWith.OH_Code = "CREDITOR";
				coLoadWith.OH_RSL_ShippingLine = shippingLine.PK;

				consol.JK_OA_CreditorAddress = coLoadWith.MainAddress.PK;

				Factory.Save();
				AssertFlightTrackingEvent("CoLoad MBL is not empty, Should create SBR event", consol, 1, "|RFN=CLDMBL|TYP=AWB Automation");
			}
		}

		public void TestSaving_AirCoLoadConsol_ShouldCreateSBREventLog_WithNonEmptyCoLoadBookingReference()
		{
			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = GetNewConsolWithMAWB("AIRCONSOL", Constants.TransportModes.Air, "08187443521", "AUSYD", "SGSIN");
				consol.JK_AgentType = Constants.AgentType.CoLoad;
				consol.JK_CoLoadMasterBill = ZString.Empty;
				consol.JK_CoLoadBookingReference = "CLDBR";

				var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
				shippingLine.RSL_CargoWiseOneCode = "CODE";
				var coLoadWith = Factory.New<OrgHeader>();
				coLoadWith.OH_Code = "CREDITOR";
				coLoadWith.OH_RSL_ShippingLine = shippingLine.PK;

				consol.JK_OA_CreditorAddress = coLoadWith.MainAddress.PK;

				Factory.Save();
				AssertFlightTrackingEvent("CoLoad BookingRef is not empty, Should create SBR event", consol, 1, "|RFN=CLDBR|TYP=AWB Automation");
			}
		}

		public void TestSaving_CreateSBREventLog_NewConsolAndSailing()
		{
			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var departureDate = 3.DaysAgo();
				var arrivalDate = 1.DaysAgo();
				var sailing = CreateJobSailing("QF001", "AUSYD", "SGSIN", departureDate, arrivalDate, false);

				var consol = GetNewConsolWithMAWB("AIRCONSOL", Constants.TransportModes.Air, "08187443521", "AUSYD", "SGSIN");

				var transport = consol.Transports[0];
				transport.JW_IsLinked = true;
				transport.JW_JX = sailing.PK;

				Factory.Save();

				var transportLogs = consol.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).OrderByDescending(x => x.SL_EventTime).ToList();
				var sailingLogs = sailing.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).OrderByDescending(x => x.SL_EventTime).ToList();

				AssertNotNull(transportLogs);
				AssertEquals("Should not create SBR event", 0, transportLogs.Count);

				AssertNotNull(sailingLogs);
				AssertEquals("Should be created new SBR event", 1, sailingLogs.Count);
				AssertEquals("Event Reference", $"|FDT={departureDate:yyyy-MM-dd}|TYP=AWB Automation|VFL=QF001", sailingLogs[0].SL_Reference);
			}
		}

		public void TestSaving_CreateSBREventLog_AirConsol_JW_JXChanged()
		{
			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var sailing1 = CreateJobSailing("QF001", "AUSYD", "SGSIN", 3.DaysAgo(), 1.DaysAgo());
				var sailing2 = CreateJobSailing("QF002", "AUSYD", "SGSIN", 3.DaysAgo(), 1.DaysAgo());

				var consol = GetNewConsolWithMAWB("AIRCONSOL", Constants.TransportModes.Air, "08187443521", "AUSYD", "SGSIN");

				var transport = consol.Transports[0];
				transport.JW_IsLinked = true;
				transport.JW_JX = sailing1.PK;

				Factory.Save();
				AssertFlightTrackingEvent("Precondition: should be created new SBR event", consol, 1, "|RFN=08187443521|TYP=AWB Automation");

				transport.JW_JX = sailing2.PK;
				Factory.Save();
				AssertFlightTrackingEvent("JW_JX changed, should be created new SBR event", consol, 2, "|RFN=08187443521|TYP=AWB Automation");
			}
		}

		public void TestSaving_CreateSBREventLog_AirConsol_MultipleTransportLegs()
		{
			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var sailing1 = CreateJobSailing("QF111", "AUSYD", "SGSIN", 3.DaysAgo(), 1.DaysAgo());
				var sailing2 = CreateJobSailing("QF222", "SGSIN", "HKHKG", 2.DaysAgo(), 1.DaysAgo());
				var sailing3 = CreateJobSailing("QF333", "HKHKG", "USLAX", 1.DaysAgo(), 1.DaysAgo());

				var consol = GetNewConsolWithMAWB("AIRCONSOL", Constants.TransportModes.Air, "08187443521", "AUSYD", "SGSIN");

				var leg1 = consol.Transports[0];
				leg1.JW_IsLinked = true;
				leg1.JW_JX = sailing1.PK;

				var leg2 = consol.Transports.AddNew();
				leg2.JW_IsLinked = true;
				leg2.JW_JX = sailing2.PK;

				var leg3 = consol.Transports.AddNew();
				leg3.JW_IsLinked = false;
				Factory.Save();

				AssertFlightTrackingEvent("Precondition: Should be created one new SBR event, for all legs", consol, 1, "|RFN=08187443521|TYP=AWB Automation");

				leg2.JW_ETD = ZDateTime.Empty;
				leg3.JW_IsLinked = true;
				leg3.JW_JX = sailing3.PK;
				Factory.Save();

				AssertFlightTrackingEvent("Should be created new SBR event, because at least one leg changed", consol, 2, "|RFN=08187443521|TYP=AWB Automation");
			}
		}

		public void TestSaving_CreateSBREventLog_SeaConsol()
		{
			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = GetNewConsolWithMAWB("SEACONSOL", Constants.TransportModes.Sea, "08187443521", "AUSYD", "SGSIN");

				var transport = consol.Transports[0];
				transport.JW_TransportMode = Constants.TransportModes.Sea;
				transport.JW_IsLinked = true;
				transport.JW_Vessel = "CONDOR";
				transport.JW_VoyageFlight = "111";
				transport.JW_RL_NKLoadPort = "AUSYD";
				transport.JW_RL_NKDiscPort = "SGSIN";
				transport.JW_ETD = new ZDateTime(2017, 01, 01);
				transport.JW_ETA = new ZDateTime(2017, 01, 02);

				Factory.Save();

				var logs = consol.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).OrderByDescending(x => x.SL_EventTime).ToList();

				AssertNotNull(logs);
				AssertEquals("Should have no SBR event for SEA ", 0, logs.Count);
			}
		}

		public void TestSaving_CreateSBREventLog_AirConsol_MAWB()
		{
			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var sailing = CreateJobSailing("QF111", "AUSYD", "SGSIN", 3.DaysAgo(), 1.DaysAgo());
				var consol = GetNewConsolWithMAWB("AIRCONSOL", Constants.TransportModes.Air, "08187443521", "AUSYD", "SGSIN");

				var transport = consol.Transports[0];
				transport.JW_IsLinked = true;
				transport.JW_JX = sailing.PK;

				Factory.Save();
				AssertFlightTrackingEvent("Precondition: Should be created new SBR event", consol, 1, "|RFN=08187443521|TYP=AWB Automation");

				consol.JK_MasterBillNum = "081";
				Factory.Save();
				AssertFlightTrackingEvent("Invalid MAWB number, SBR event should be canceled", consol, 1, "|RFN=08187443521|TYP=AWB Automation", true);
			}
		}

		void AssertFlightTrackingEvent(ZString message, ForwardingConsol consol, int expectedLogCount, ZString expectedEventReference, bool expectedIsCancelled = false)
		{
			var logs = consol.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).OrderByDescending(x => x.SL_EventTime).ToList();

			AssertNotNull(logs);
			AssertEquals(message, expectedLogCount, logs.Count);

			if (expectedLogCount > 0)
			{
				AssertEquals("Event Reference", expectedEventReference, logs[0].SL_Reference);
				AssertEquals("Event IsCancelled", expectedIsCancelled, logs[0].SL_IsCancelled);
			}

			for (int i = 1; i < logs.Count; i++)
			{
				var log = logs[i];
				AssertEquals("Event should be canceled", true, log.SL_IsCancelled);
			}
		}

		JobSailing CreateJobSailing(ZString flightNumber, ZString loadingPort, ZString dischargePort, ZDateTime etdDate, ZDateTime etaDate, bool saveData = true)
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = flightNumber;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = loadingPort;
			origin.JA_E_DEP = etdDate;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = dischargePort;
			destination.JB_E_ARV = etaDate;
			voyage.GenerateSailings();

			if (saveData)
			{
				Factory.Save();
			}

			return voyage.Sailings[0];
		}

		ForwardingConsol GetNewConsolWithMAWB(ZString consolRef, ZString transportMode, ZString masterBillNum, ZString loadingPort, ZString dischargePort)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = transportMode;
			consol.JK_UniqueConsignRef = consolRef;
			consol.JK_MasterBillNum = masterBillNum;
			consol.JK_RL_NKLoadPort = loadingPort;
			consol.JK_RL_NKDischargePort = dischargePort;

			return consol;
		}

		#endregion

		#region OnSaving

		public void TestOnSaving()
		{
			RefCountry countryAU = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Australia);
			ZString aUS = Constants.CountryCodes.Australia;

			RefCountryRequiredDocument requiredDoc1 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.AgentsInvoice, aUS, "NZ", JobRequiredDocument.DocUsage.Import, Constants.TransportModes.Sea, true);
			RefCountryRequiredDocument requiredDoc2 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.AgentsInstruction, "", "NZ", JobRequiredDocument.DocUsage.Import, Constants.TransportModes.All, true);
			RefCountryRequiredDocument requiredDoc3 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.ArrivalNotice, aUS, "", JobRequiredDocument.DocUsage.Export, Constants.TransportModes.Sea, true);
			RefCountryRequiredDocument requiredDoc4 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.BankDraft, "", "", JobRequiredDocument.DocUsage.Both, Constants.TransportModes.All, true);
			RefCountryRequiredDocument requiredDoc5 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.BillOfEntry, aUS, "NZ", JobRequiredDocument.DocUsage.Import, Constants.TransportModes.Rail, true);
			RefCountryRequiredDocument requiredDoc6 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.CartageAdvice, aUS, "NZ", JobRequiredDocument.DocUsage.Import, Constants.TransportModes.All, false);
			RefCountryRequiredDocument requiredDoc7 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.ChargeSheet, "US", "NZ", JobRequiredDocument.DocUsage.Import, Constants.TransportModes.All, true);
			RefCountryRequiredDocument requiredDoc8 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.DelayAlert, aUS, "US", JobRequiredDocument.DocUsage.Import, Constants.TransportModes.All, true);
			RefCountryRequiredDocument requiredDoc9 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.AgentsInvoice, aUS, "NZ", JobRequiredDocument.DocUsage.Export, Constants.TransportModes.Sea, true);

			Factory.Save();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_OA_SendingForwarderAddress = LocalConsignor.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = OverseasConsignee.MainAddress.PK;
			consol.JK_RL_NKLoadPort = "AU";
			consol.JK_RL_NKDischargePort = "NZ";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = "FCL";
			Factory.Save();

			consol.JK_MasterBillNum = "17610000001";
			AssertEquals(0, consol.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WaybillBillOfLadingAssigned.Code)).Length);
			Factory.Save();
			AssertEquals(1, consol.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WaybillBillOfLadingAssigned.Code)).Length);
			AssertEquals("Master Bill Number \"17610000001\" Was Entered", ((StmALog)consol.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WaybillBillOfLadingAssigned.Code))[0]).SL_Reference);
			consol.JK_MasterBillNum = ZString.Empty;
			AssertEquals(0, consol.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WaybillBillOfLadingUnassigned.Code)).Length);
			Factory.Save();
			AssertEquals(1, consol.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WaybillBillOfLadingUnassigned.Code)).Length);

			AssertNull("Doesn't match, shouldn't be in the list", consol.RequiredDocuments.GetDocByType(Constants.RefDocTypes.BillOfEntry));
			AssertNull(consol.RequiredDocuments.GetDocByType(Constants.RefDocTypes.CartageAdvice));
			AssertNull(consol.RequiredDocuments.GetDocByType(Constants.RefDocTypes.ChargeSheet));
			AssertNull(consol.RequiredDocuments.GetDocByType(Constants.RefDocTypes.DelayAlert));

			AssertEquals(4, consol.RequiredDocuments.Count);

			JobRequiredDocument rdAgentInvoice = consol.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInvoice);
			AssertNotNull("AgentInvoice: Should be in the list", rdAgentInvoice);
			AssertEquals("AgentInvoice: DocUsage should be BTH", "BTH", rdAgentInvoice.EQ_DocUsage);

			JobRequiredDocument rdAgentsInstruction = consol.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInstruction);
			AssertNotNull("Should be in the list", rdAgentsInstruction);
			AssertEquals("DocUsage should be IMP", "IMP", rdAgentsInstruction.EQ_DocUsage);

			JobRequiredDocument rdArrivalNotice = consol.RequiredDocuments.GetDocByType(Constants.RefDocTypes.ArrivalNotice);
			AssertNotNull("Should be in the list", rdArrivalNotice);
			AssertEquals("DocUsage should be EXP", "EXP", rdArrivalNotice.EQ_DocUsage);

			JobRequiredDocument rdBankDraft = consol.RequiredDocuments.GetDocByType(Constants.RefDocTypes.BankDraft);
			AssertNotNull("Should be in the list", rdBankDraft);
			AssertEquals("DocUsage should be BTH", "BTH", rdBankDraft.EQ_DocUsage);

			RefCountry countryAU2 = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Australia);
			RefCountryRequiredDocument requiredDoc10 = GetNewRequiredDoc(countryAU2, Constants.RefDocTypes.AgentsInvoice, aUS, aUS, JobRequiredDocument.DocUsage.Domestic, Constants.TransportModes.Sea, true);
			RefCountryRequiredDocument requiredDoc11 = GetNewRequiredDoc(countryAU2, Constants.RefDocTypes.ChargeSheet, aUS, aUS, JobRequiredDocument.DocUsage.Import, Constants.TransportModes.Sea, true);
			RefCountryRequiredDocument requiredDoc12 = GetNewRequiredDoc(countryAU2, Constants.RefDocTypes.DangerousGoodsForm, "", "", JobRequiredDocument.DocUsage.Domestic, Constants.TransportModes.Sea, true);
			RefCountryRequiredDocument requiredDoc13 = GetNewRequiredDoc(countryAU2, Constants.RefDocTypes.EFTRequest, aUS, aUS, JobRequiredDocument.DocUsage.All, Constants.TransportModes.Sea, true);
			RefCountryRequiredDocument requiredDoc14 = GetNewRequiredDoc(countryAU2, Constants.RefDocTypes.EntryPrint, "", "", JobRequiredDocument.DocUsage.All, Constants.TransportModes.Sea, true);
			RefCountryRequiredDocument requiredDoc15 = GetNewRequiredDoc(countryAU2, Constants.RefDocTypes.HouseBill, aUS, "CY", JobRequiredDocument.DocUsage.All, Constants.TransportModes.Sea, true);
			RefCountryRequiredDocument requiredDoc16 = GetNewRequiredDoc(countryAU2, Constants.RefDocTypes.MasterHouse, aUS, "", JobRequiredDocument.DocUsage.All, Constants.TransportModes.Sea, true);

			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_OA_SendingForwarderAddress = LocalConsignor.MainAddress.PK;
			consol2.JK_OA_ReceivingForwarderAddress = LocalConsignee.MainAddress.PK;
			consol2.JK_RL_NKLoadPort = "AU";
			consol2.JK_RL_NKDischargePort = "AU";
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_ConsolMode = "FCL";
			consol2.IsDomesticFreight = true;
			Factory.Save();

			AssertNull("Doesn't match, shouldn't be in the list", consol2.RequiredDocuments.GetDocByType(Constants.RefDocTypes.ChargeSheet));
			AssertNull(consol2.RequiredDocuments.GetDocByType(Constants.RefDocTypes.HouseBill));

			JobRequiredDocument rdAgentInvoice2 = consol2.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInvoice);
			AssertNotNull("AgentInvoice: Should be in the list", rdAgentInvoice2);
			AssertEquals("AgentInvoice: DocUsage should be DOM", "DOM", rdAgentInvoice2.EQ_DocUsage);

			JobRequiredDocument dangerousGoodsForm = consol2.RequiredDocuments.GetDocByType(Constants.RefDocTypes.DangerousGoodsForm);
			AssertNotNull("AgentInvoice: Should be in the list", dangerousGoodsForm);
			AssertEquals("AgentInvoice: DocUsage should be DOM", "DOM", dangerousGoodsForm.EQ_DocUsage);

			JobRequiredDocument entryPrint = consol2.RequiredDocuments.GetDocByType(Constants.RefDocTypes.EntryPrint);
			AssertNotNull("AgentInvoice: Should be in the list", entryPrint);
			AssertEquals("AgentInvoice: DocUsage should be DOM", "DOM", entryPrint.EQ_DocUsage);

			JobRequiredDocument eFTRequest = consol2.RequiredDocuments.GetDocByType(Constants.RefDocTypes.EFTRequest);
			AssertNotNull("AgentInvoice: Should be in the list", eFTRequest);
			AssertEquals("AgentInvoice: DocUsage should be DOM", "DOM", eFTRequest.EQ_DocUsage);

			JobRequiredDocument masterHouse = consol2.RequiredDocuments.GetDocByType(Constants.RefDocTypes.MasterHouse);
			AssertNotNull("AgentInvoice: Should be in the list", masterHouse);
			AssertEquals("AgentInvoice: DocUsage should be DOM", "DOM", masterHouse.EQ_DocUsage);
		}

		public void TestOnSaving_WayBillNumberMissing()
		{
			var newCustomisation = new BillOfLadingNumberCustomisation();
			newCustomisation.AutoAllocateMasterBillNumbersToConsols = true;

			using (FreightConfigurationRegistry.Instance.RoadConsolMasterBillNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, newCustomisation))
			{
				var consol1 = Factory.New<ForwardingConsol>();
				consol1.JK_TransportMode = Constants.TransportModes.Road;
				consol1.JK_MasterBillNum = ZString.Empty;
				Factory.Save();

				AssertEquals("Forwading Consol master bill number should not empty and auto allocate a value", "C00001000", consol1.JK_MasterBillNum);
			}

			newCustomisation = new BillOfLadingNumberCustomisation();
			newCustomisation.AutoAllocateMasterBillNumbersToConsols = false;

			using (FreightConfigurationRegistry.Instance.RoadConsolMasterBillNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, newCustomisation))
			{
				var consol2 = Factory.New<ForwardingConsol>();
				consol2.JK_TransportMode = Constants.TransportModes.Road;
				consol2.JK_MasterBillNum = ZString.Empty;
				Factory.Save();

				AssertEquals("Forwading Consol master bill number should be empty", ZString.Empty, consol2.JK_MasterBillNum);
			}
		}

		public void TestWhenSavingConsolShouldNotCalculateContainerEmptyReturnedByMultipleTimesForTheSameContainer()
		{
			var calculateRequiredByTimesCalled = 0;

			var mockStrategy = new Mock<IContainerDefaultingStrategy>();
			mockStrategy.Setup(s => s.CalculateRequiredBy())
				.Returns((ZDateTime.Empty, ZDateTime.Empty, ZString.Empty))
				.Callback(() => ++calculateRequiredByTimesCalled);

			using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", mockStrategy.Object))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_AgentType = Constants.AgentType.Direct;
				consol.JK_ConsolMode = Constants.ContainerModes.LCL;
				consol.JK_UniqueConsignRef = "CON1111";

				AssertEquals("prerequisite; consol has no containers", 0, consol.Containers.Count);
				Factory.Save();

				consol.JK_ConsolMode = Constants.ContainerModes.FCL;
				var container = consol.Containers.AddNew();

				Factory.Save();
				AssertEquals("expected CalculateRequiredBy called 1 time", 1, calculateRequiredByTimesCalled);

				calculateRequiredByTimesCalled = 0;

				container.JC_FCLAvailable = ZDateTime.Now;
				Factory.Save();
				AssertEquals("expected CalculateRequiredBy called 1 time", 1, calculateRequiredByTimesCalled);
			}
		}

		public void TestGetNewJK_UniqueConsignRefAndMasterBillNumber()
		{
			var newCustomisation = new BillOfLadingNumberCustomisation();
			newCustomisation.AutoAllocateMasterBillNumbersToConsols = true;

			using (FreightConfigurationRegistry.Instance.RoadConsolMasterBillNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, newCustomisation))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Road;
				consol.JK_MasterBillNum = ZString.Empty;
				var consolUniqueConsignRef = consol.GetNewJK_UniqueConsignRef(Factory);
				AssertEquals("Common consol generate unique consign ref", "C00001000", consolUniqueConsignRef);
				AssertEquals("Common consol generate master bill number", "C00001000", consol.JK_MasterBillNum);
			}
		}

		public void TestAWBNotPopulatingIfNotRequested()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			AssertEquals(false, consol.IsAWBLoaded);

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			AssertEquals(false, consol.IsAWBLoaded);

			Factory.Save();
			AssertEquals(false, consol.IsAWBLoaded);

			var awb = consol.AWBHeader;
			AssertEquals(true, consol.IsAWBLoaded);
		}

		public void TestAWBNotPopulatedAfterConsolSaved()
		{
			#region The usual test setup

			Consol.JK_TransportMode = Constants.TransportModes.Air;
			Transport tran = Consol.MostInterestingTransportForBinding[0];
			tran.JW_ETD = new DateTime(2009, 03, 24);
			tran.JW_TransportMode = Constants.TransportModes.Air;
			tran.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;
			tran.JW_VoyageFlight = "BA123";

			#endregion

			#region Our first awb get data from consol

			AssertEquals("PreCondition: Dont Overide", false, Consol.JK_OverrideWaybillDefaults);
			AssertEquals("PreCondition: AWB date is set", "24", Consol.AWBHeader.EH_Booking1stFlightDate);
			AssertEquals("PreCondition: AWB 1st Flight is set", "123", Consol.AWBHeader.EH_Booking1stFlight);
			AssertEquals("PreCondition: AWB 1st Carrier is set", "BA", Consol.AWBHeader.EH_Booking1stCarrier);
			Consol.Factory.Save();

			#endregion

			#region Edit of consol data, should result in AWB being updated NOT on save but when PopulateAWB() is called explicitly

			tran.JW_ETD = new DateTime(2009, 03, 23);
			tran.JW_VoyageFlight = "AF987";
			AssertNotEquals("Editing the consol/transport data, doesn't immediately affect AWB", "23", consol.AWBHeader.EH_Booking1stFlightDate);
			AssertNotEquals("Editing the consol/transport data, doesn't immediately affect AWB", "987", Consol.AWBHeader.EH_Booking1stFlight);
			AssertNotEquals("Editing the consol/transport data, doesn't immediately affect AWB", "AF", Consol.AWBHeader.EH_Booking1stCarrier);

			Consol.Factory.Save();

			AssertNotEquals("Editing the consol/transport data, doesn't immediately affect AWB", "23", consol.AWBHeader.EH_Booking1stFlightDate);
			AssertNotEquals("Editing the consol/transport data, doesn't immediately affect AWB", "987", Consol.AWBHeader.EH_Booking1stFlight);
			AssertNotEquals("Editing the consol/transport data, doesn't immediately affect AWB", "AF", Consol.AWBHeader.EH_Booking1stCarrier);

			Consol.PopulateAWB();

			AssertEquals("Saving consol will update the AWB", "23", consol.AWBHeader.EH_Booking1stFlightDate);
			AssertEquals("Saving consol will update the AWB", "987", Consol.AWBHeader.EH_Booking1stFlight);
			AssertEquals("Saving consol will update the AWB", "AF", Consol.AWBHeader.EH_Booking1stCarrier);

			#endregion
		}

		RefCountryRequiredDocument GetNewRequiredDoc(RefCountry country, ZString docType, ZString orig, ZString dest, ZString usage, ZString transport, ZBool isConsol)
		{
			RefCountryRequiredDocument result = country.RequiredDocuments.AddNew();
			result.RD_DocType = docType;
			result.RD_RN_NKOrigin = orig;
			result.RD_RN_NKDestination = dest;
			result.RD_DocUsage = usage;
			result.RD_TransportMode = transport;
			result.RD_OnConsol = isConsol;
			return result;
		}

		public void TestTotalShipmentActWeightAndVolumeCheckRounding()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			consol.WeightVerificationUnit = ZString.Empty;
			consol.JK_TotalShipmentActWeightCheck = 123.789m;

			consol.VolumeVerificationUnit = ZString.Empty;
			consol.JK_TotalShipmentActVolumeCheck = 342.111m;

			AssertEquals(123.789m, consol.JK_TotalShipmentActWeightCheck);
			AssertEquals(342.111m, consol.JK_TotalShipmentActVolumeCheck);

			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfWeightDecimals1 = collection.AddNew();
			defaultNumberOfWeightDecimals1.UnitOfMeasure = Constants.Weight.Kilograms;
			defaultNumberOfWeightDecimals1.TransportMode = Constants.TransportModes.Sea;
			defaultNumberOfWeightDecimals1.NumberOfDecimals = 2;
			defaultNumberOfWeightDecimals1.RoundingMode = RoundingModes.Up;
			var defaultNumberOfVolumeDecimals1 = collection.AddNew();
			defaultNumberOfVolumeDecimals1.UnitOfMeasure = Constants.Volume.CubicMetres;
			defaultNumberOfVolumeDecimals1.TransportMode = Constants.TransportModes.Sea;
			defaultNumberOfVolumeDecimals1.NumberOfDecimals = 2;

			defaultNumberOfVolumeDecimals1.RoundingMode = RoundingModes.Down;
			var defaultNumberOfWeightDecimals2 = collection.AddNew();
			defaultNumberOfWeightDecimals2.UnitOfMeasure = Constants.Weight.Grams;
			defaultNumberOfWeightDecimals2.TransportMode = Constants.TransportModes.Sea;
			defaultNumberOfWeightDecimals2.NumberOfDecimals = 1;
			defaultNumberOfWeightDecimals2.RoundingMode = RoundingModes.Up;
			var defaultNumberOfVolumeDecimals2 = collection.AddNew();
			defaultNumberOfVolumeDecimals2.UnitOfMeasure = Constants.Volume.CubicYards;
			defaultNumberOfVolumeDecimals2.TransportMode = Constants.TransportModes.Sea;
			defaultNumberOfVolumeDecimals2.NumberOfDecimals = 1;
			defaultNumberOfVolumeDecimals2.RoundingMode = RoundingModes.Down;

			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			consol.WeightVerificationUnit = ZString.Empty;
			consol.JK_TotalShipmentActWeightCheck = 124.989m;

			consol.VolumeVerificationUnit = ZString.Empty;
			consol.JK_TotalShipmentActVolumeCheck = 321.122m;

			AssertEquals(124.99m, consol.JK_TotalShipmentActWeightCheck);
			AssertEquals(321.12m, consol.JK_TotalShipmentActVolumeCheck);

			consol.WeightVerificationUnit = Constants.Weight.Grams;
			consol.JK_TotalShipmentActWeightCheck = 111.123m;

			consol.VolumeVerificationUnit = Constants.Volume.CubicYards;
			consol.JK_TotalShipmentActVolumeCheck = 121.231m;

			AssertEquals(111.2m, consol.JK_TotalShipmentActWeightCheck);
			AssertEquals(121.2m, consol.JK_TotalShipmentActVolumeCheck);
		}

		public void TestPreAllocationPercentageWarningEvent()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TotalShipmentActWeightCheck = 1000m;
			consol.JK_TotalShipmentActVolumeCheck = 100m;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ActualWeight = 800m;
			shipment.JS_ActualVolume = 60m;

			Factory.Save();
			AssertEquals("Event logged", 1, consol.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.PreAllocatedAmountExceeded.Code)).Length);
			AssertEquals("Event is canceled", true, ((StmALog)consol.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.PreAllocatedAmountExceeded.Code))[0]).SL_IsCancelled);

			var checks = ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.Value;
			checks.Weight.Action = PreAllocationCheck.Actions.Warning;
			checks.Weight.Percentage = 40m;
			checks.Volume.Action = PreAllocationCheck.Actions.Warning;
			checks.Volume.Percentage = 40m;
			ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, checks);

			consol.JK_TotalShipmentActVolumeCheck = 110m;
			Factory.Save();
			AssertEquals("No new event of the same type was created", 1, consol.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.PreAllocatedAmountExceeded.Code)).Length);
			AssertEquals("Event is active", false, ((StmALog)consol.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.PreAllocatedAmountExceeded.Code))[0]).SL_IsCancelled);

			consol.JK_TotalShipmentActWeightCheck = 10000m;
			consol.JK_TotalShipmentActVolumeCheck = 1000m;
			Factory.Save();
			AssertEquals("Event is there", 1, consol.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.PreAllocatedAmountExceeded.Code)).Length);
			AssertEquals("Event is canceled", true, ((StmALog)consol.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.PreAllocatedAmountExceeded.Code))[0]).SL_IsCancelled);

			consol.JK_TotalShipmentActVolumeCheck = 120m;
			Factory.Save();
			AssertEquals("Event is there", 1, consol.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.PreAllocatedAmountExceeded.Code)).Length);
			AssertEquals("Event is active", false, ((StmALog)consol.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.PreAllocatedAmountExceeded.Code))[0]).SL_IsCancelled);
		}

		public void TestPreAllocationPercentageWarningEvent_ShipmentCountChanged()
		{
			// Arrange
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "ConsolRef";
			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			consol.JK_TotalShipmentActWeightCheck = 500m;
			consol.JK_TotalShipmentActVolumeCheck = 50m;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "MasterRef";
			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			shipment.JS_ActualWeight = 800m;
			shipment.JS_ActualVolume = 60m;

			Factory.Save();
			AssertEquals("Pre-condition", 1, consol.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.PreAllocatedAmountExceeded.Code)).Length);
			AssertEquals("Event is canceled", true, ((StmALog)consol.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.PreAllocatedAmountExceeded.Code))[0]).SL_IsCancelled);

			// Act
			consol.Shipments.Add(shipment);
			Factory.Save();

			// Assert
			AssertEquals("Event is there", 1, consol.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.PreAllocatedAmountExceeded.Code)).Length);
			AssertEquals("Event is active", true, ((StmALog)consol.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.PreAllocatedAmountExceeded.Code))[0]).SL_IsCancelled);
		}

		public void TestPreAllocationLogIsUpdatedToCurrentBranchAndDepartment()
		{
			var currentBranch = GlbBranch.CurrentBranch;
			var currentDepartment = GlbDepartment.CurrentDepartment;

			var anotherBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, currentBranch.PK));
			var anotherDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.PK, SQLComparisonOperator.NotEqual, currentDepartment.PK));

			var checks = ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.Value;
			checks.Weight.Action = PreAllocationCheck.Actions.Warning;
			checks.Weight.Percentage = 40m;
			using (ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.SetTemporaryValue(Guid.Empty, anotherBranch.PK.ToGuid(), anotherDepartment.PK.ToGuid(), checks))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TotalShipmentActWeightCheck = 1000;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_ActualWeight = 500;

				Factory.Save();
				AssertEquals("One event logged", 1, consol.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.PreAllocatedAmountExceeded.Code)).Length);

				var log = ((StmALog)consol.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.PreAllocatedAmountExceeded.Code))[0]);
				var logPK = log.PK;

				AssertEquals("Event is cancelled", true, log.SL_IsCancelled);
				AssertEquals("Event is logged against current branch", currentBranch.GB_Code, log.SL_GB_NKBranch);
				AssertEquals("Event is logged against current department", currentDepartment.GE_Code, log.SL_GE_NKDepartment);

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, anotherBranch.PK.ToGuid(), anotherDepartment.PK.ToGuid()))
				{
					shipment.JS_ActualWeight = 550;

					Factory.Save();
					AssertEquals("One event logged", 1, consol.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.PreAllocatedAmountExceeded.Code)).Length);

					log = ((StmALog)consol.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.PreAllocatedAmountExceeded.Code))[0]);
					AssertEquals("It's the same event", logPK, log.PK);
					AssertEquals("Event is active", false, log.SL_IsCancelled);
					AssertEquals("Event is logged against the other branch", anotherBranch.GB_Code, log.SL_GB_NKBranch);
					AssertEquals("Event is logged against the other department", anotherDepartment.GE_Code, log.SL_GE_NKDepartment);
				}

				shipment.JS_ActualWeight = 600;

				Factory.Save();
				AssertEquals("One event logged", 1, consol.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.PreAllocatedAmountExceeded.Code)).Length);

				log = ((StmALog)consol.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.PreAllocatedAmountExceeded.Code))[0]);
				AssertEquals("It's still the same event", logPK, log.PK);
				AssertEquals("Event is cancelled again", true, log.SL_IsCancelled);
				AssertEquals("Event is back to the current branch", currentBranch.GB_Code, log.SL_GB_NKBranch);
				AssertEquals("Event is back to the current department", currentDepartment.GE_Code, log.SL_GE_NKDepartment);
			}
		}

		#endregion

		#region HasChanges

		public void TestMasterBillShipperOverrideDocumentaryAddressHasChanges()
		{
			AssertAgencyRelatedDocumentaryAddressHasChanges("MasterBillShipperOverrideDocumentaryAddress");
		}

		public void TestMasterBillConsigneeOverrideDocumentaryAddressHasChanges()
		{
			AssertAgencyRelatedDocumentaryAddressHasChanges("MasterBillConsigneeOverrideDocumentaryAddress");
		}

		public void TestNotifyPartyDocumentaryAddressHasChanges()
		{
			AssertAgencyRelatedDocumentaryAddressHasChanges("NotifyPartyDocumentaryAddress");
		}

		void AssertAgencyRelatedDocumentaryAddressHasChanges(ZString agencyRelatedDocumentaryAddressName)
		{
			var consol = Factory.New<ForwardingConsol>();

			Assert(!consol.IsInDatabase);
			Assert(!consol.HasChanges);

			Assert("Consol was SuspendSettingHasChanges, Address is empty", ((JobDocAddress)consol[agencyRelatedDocumentaryAddressName]).IsEmpty);
			Assert("Consol was SuspendSettingHasChanges, Address is empty", !((JobDocAddress)consol[agencyRelatedDocumentaryAddressName]).HasChanges);

			using (consol.SuspendSettingHasChanges())
			{
				consol.JK_AgentType = Core.Constants.AgentType.CoLoad;

				((JobDocAddress)consol[agencyRelatedDocumentaryAddressName]).E2_OA_Address = ZGuid.NewZGuid();
				Assert(!((JobDocAddress)consol[agencyRelatedDocumentaryAddressName]).HasChanges);
			}

			Assert("Consol was SuspendSettingHasChanges, Address is not empty", !((JobDocAddress)consol[agencyRelatedDocumentaryAddressName]).IsEmpty);
			Assert("Consol was SuspendSettingHasChanges, Address is not empty", !((JobDocAddress)consol[agencyRelatedDocumentaryAddressName]).HasChanges);

			using (consol.SuspendSettingHasChanges())
			{
				((JobDocAddress)consol[agencyRelatedDocumentaryAddressName]).E2_OA_Address = ZGuid.Empty;
				Assert(((JobDocAddress)consol[agencyRelatedDocumentaryAddressName]).IsEmpty);
			}

			consol.JK_AgentType = Core.Constants.AgentType.Charter;

			Assert(consol.HasChanges);
			Assert("Consol was not SuspendSettingHasChanges, Address is empty", ((JobDocAddress)consol[agencyRelatedDocumentaryAddressName]).IsEmpty);
			Assert("Consol was not SuspendSettingHasChanges, Address is empty", !((JobDocAddress)consol[agencyRelatedDocumentaryAddressName]).HasChanges);

			using (consol.SuspendSettingHasChanges())
			{
				((JobDocAddress)consol[agencyRelatedDocumentaryAddressName]).E2_OA_Address = ZGuid.NewZGuid();
				Assert(!((JobDocAddress)consol[agencyRelatedDocumentaryAddressName]).HasChanges);
			}

			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;

			Assert(consol.HasChanges);
			Assert("Consol was not SuspendSettingHasChanges, Address is not empty", !((JobDocAddress)consol[agencyRelatedDocumentaryAddressName]).IsEmpty);
			Assert("Consol was not SuspendSettingHasChanges, Address is not empty", ((JobDocAddress)consol[agencyRelatedDocumentaryAddressName]).HasChanges);
		}

		#endregion

		#region TestIsCargoOnly

		public void TestIsCargoOnly_TransportModeIsAir_SailingIsCargoOnly()
		{
			var consol = Factory.New<ForwardingConsol>();

			LinkedTransportWithSailing(consol.Transports[0], "AUBNE", "SGSIN");
			LinkedTransportWithSailing(consol.Transports.AddNew(), "SGSIN", "HKHKG");
			LinkedTransportWithSailing(consol.Transports.AddNew(), "HKHKG", "USLAX");

			Factory.Save();

			AssertEquals("Transport is Is Cargo Only", true, consol.JK_Calc_IsCargoOnly);
		}

		public void TestIsCargoOnly_TransportModeIsAir_NotAllSailingIsCargoOnly()
		{
			var consol = Factory.New<ForwardingConsol>();

			LinkedTransportWithSailing(consol.Transports[0], "AUBNE", "SGSIN");
			LinkedTransportWithSailing(consol.Transports.AddNew(), "SGSIN", "HKHKG");
			LinkedTransportWithSailing(consol.Transports.AddNew(), "HKHKG", "USLAX", isCargoOnly: false);

			Factory.Save();

			AssertEquals("Transport is not Is Cargo Only", false, consol.JK_Calc_IsCargoOnly);
		}

		public void TestIsCargoOnly_TransportModeIsAir_SailingIsNotCargoOnly()
		{
			var consol = Factory.New<ForwardingConsol>();

			LinkedTransportWithSailing(consol.Transports[0], "AUBNE", "SGSIN", isCargoOnly: false);
			LinkedTransportWithSailing(consol.Transports.AddNew(), "SGSIN", "HKHKG", isCargoOnly: false);
			LinkedTransportWithSailing(consol.Transports.AddNew(), "HKHKG", "USLAX", isCargoOnly: false);

			Factory.Save();

			AssertEquals("Transport is not Is Cargo Only", false, consol.JK_Calc_IsCargoOnly);
		}

		public void TestIsCargoOnly_TransportModeIsSea_SailingIsCargoOnly()
		{
			var consol = Factory.New<ForwardingConsol>();

			LinkedTransportWithSailing(consol.Transports[0], "AUBNE", "SGSIN", isAir: false);
			LinkedTransportWithSailing(consol.Transports.AddNew(), "SGSIN", "HKHKG", isAir: false);
			LinkedTransportWithSailing(consol.Transports.AddNew(), "HKHKG", "USLAX", isAir: false);

			Factory.Save();

			AssertEquals("Transport is not Is Cargo Only", false, consol.JK_Calc_IsCargoOnly);
		}

		public void TestIsCargoOnly_TransportModeIsAirSea_SailingIsCargoOnly()
		{
			var consol = Factory.New<ForwardingConsol>();

			LinkedTransportWithSailing(consol.Transports[0], "AUBNE", "SGSIN");
			LinkedTransportWithSailing(consol.Transports.AddNew(), "SGSIN", "HKHKG", isAir: false);
			LinkedTransportWithSailing(consol.Transports.AddNew(), "HKHKG", "USLAX");

			Factory.Save();

			AssertEquals("Transport is Is Cargo Only", true, consol.JK_Calc_IsCargoOnly);
		}

		public void TestIsCargoOnly_TransportModeIsAirSea_SailingIsNotCargoOnly()
		{
			var consol = Factory.New<ForwardingConsol>();

			LinkedTransportWithSailing(consol.Transports[0], "AUBNE", "SGSIN", isCargoOnly: false);
			LinkedTransportWithSailing(consol.Transports.AddNew(), "SGSIN", "HKHKG", isAir: false, isCargoOnly: false);
			LinkedTransportWithSailing(consol.Transports.AddNew(), "HKHKG", "USLAX", isCargoOnly: false);

			Factory.Save();

			AssertEquals("Transport is not Is Cargo Only", false, consol.JK_Calc_IsCargoOnly);
		}

		Transport LinkedTransportWithSailing(Transport transport, string portOfLoading, string portOfDischarge, bool isAir = true, bool isCargoOnly = true)
		{
			transport.JW_TransportMode = isAir ? Constants.TransportModes.Air : Constants.TransportModes.Sea;
			transport.JW_IsLinked = true;

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_IsCargoOnly = isCargoOnly;
			voyage.JV_VoyageFlight = "DI56";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = portOfLoading;
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = portOfDischarge;
			voyage.GenerateSailings();
			transport.JW_JX = voyage.Sailings[0].PK;

			return transport;
		}

		#endregion

		#region Cargo Only Validation

		public void TestCargoOnlyValidationIsCalledOnShipmentsCountChanged()
		{
			var inspectionTypes = FreightDataRegistry.Instance.ShipmentInspectionTypes_HongKong.Value;
			((ShipmentInspectionType)inspectionTypes.Types.FindByCode("XRY")).AllowedOnPassengerFlights = false;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			using (FreightDataRegistry.Instance.ShipmentInspectionTypes_HongKong.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, inspectionTypes))
			{
				ForwardingConsol consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = "AIR";

				JobVoyage voyage = Factory.NewWithValidTestData<JobVoyage>();
				voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
				voyage.JV_IsCargoOnly = false;

				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "HKHKG";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";

				consol.Transports[0].JW_JX = voyage.Sailings[0].PK;

				AssertNoErrors("Precondition", consol.Transports[0].JW_IsCargoOnlyInfo);

				ForwardingShipment shipment = Factory.New<ForwardingShipment>();
				shipment.ConsignorPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
				shipment.JS_InspectionTypeCode = "XRY";

				AssertNoErrors("Shipment has not been attached", consol.Transports[0].JW_IsCargoOnlyInfo);

				consol.Shipments.Add(shipment);
				AssertHasErrorContaining(consol.Transports[0].JW_IsCargoOnlyInfo, "For a voyage that is not Cargo Only, all Shipments must be Aviation Security approved or Exempt");
			}
		}

		#endregion

		#region PreAllocation Validation

		public void TestPreAllocationValidationIsCalledOnShipmentsCountChanged()
		{
			PreAllocationCheckCollection checks = ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.Value;
			foreach (PreAllocationCheck check in checks)
			{
				if (check.Measure != PreAllocationCheck.Measures.Dimensions)
				{
					check.Action = PreAllocationCheck.Actions.Warning;
					check.Percentage = 50m;
				}
			}

			ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, checks);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TotalShipmentActWeightCheck = 1000m;
			consol.JK_TotalShipmentActVolumeCheck = 100m;
			consol.JK_TotalShipmentChargableCheck = 1000m;
			consol.JK_TotalShipmentCountCheck = 1;

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ActualWeight = 800m;
			shipment.JS_ActualVolume = 60m;
			shipment.JS_ActualChargeable = 700m;

			AssertPreAllocationWarnings("Precondition: no shipments, no warnings", consol, false);

			consol.Shipments.Add(shipment);
			AssertPreAllocationWarnings("Has warning", consol, true);

			consol.Shipments.Remove(shipment);
			AssertPreAllocationWarnings("No warning", consol, false);
		}

		void AssertPreAllocationWarnings(string message, ForwardingConsol consol, bool expectWarning)
		{
			AssertEquals(message, expectWarning, consol.JK_TotalShipmentActWeightCheckInfo.HasWarnings());
			AssertEquals(message, expectWarning, consol.JK_TotalShipmentActVolumeCheckInfo.HasWarnings());
			AssertEquals(message, expectWarning, consol.JK_TotalShipmentChargableCheckInfo.HasWarnings());
			AssertEquals(message, expectWarning, consol.JK_TotalShipmentCountCheckInfo.HasWarnings());
		}

		public void TestIsPreAllocationPercentageExceededAndRestricted()
		{
			PreAllocationCheckCollection checks = ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.Value;
			foreach (PreAllocationCheck check in checks)
			{
				check.Action = PreAllocationCheck.Actions.Restriction;
				check.Percentage = 50m;
			}

			ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, checks);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TotalShipmentActWeightCheck = 1000m;
			consol.JK_TotalShipmentActVolumeCheck = 100m;
			consol.JK_TotalShipmentChargableCheck = 1000m;
			consol.JK_TotalShipmentCountCheck = 3;

			AssertEquals(false, consol.IsPreAllocationExceededAndRestricted);

			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_ActualWeight = 800m;
			AssertEquals(true, consol.IsPreAllocationExceededAndRestricted);

			shipment.JS_ActualWeight = 400m;
			AssertEquals(false, consol.IsPreAllocationExceededAndRestricted);

			shipment.JS_ActualVolume = 60m;
			AssertEquals(true, consol.IsPreAllocationExceededAndRestricted);

			shipment.JS_ActualVolume = 40m;
			AssertEquals(false, consol.IsPreAllocationExceededAndRestricted);

			shipment.JS_ActualChargeable = 700m;
			AssertEquals(true, consol.IsPreAllocationExceededAndRestricted);

			shipment.JS_ActualChargeable = 400m;
			AssertEquals(false, consol.IsPreAllocationExceededAndRestricted);

			consol.Shipments.AddNew();
			AssertEquals(true, consol.IsPreAllocationExceededAndRestricted);

			consol.Shipments.RemoveAll();
			AssertEquals(false, consol.IsPreAllocationExceededAndRestricted);

			checks = ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.Value;
			foreach (PreAllocationCheck check in checks)
			{
				if (check.Measure != PreAllocationCheck.Measures.Dimensions)
				{
					check.Action = PreAllocationCheck.Actions.Warning;
					check.Percentage = 50m;
				}
			}

			ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, checks);

			shipment = consol.Shipments.AddNew();
			shipment.JS_ActualWeight = 800m;
			shipment.JS_ActualVolume = 60m;
			shipment.JS_ActualChargeable = 700m;
			AssertEquals("Pre-allocation values exceeded, but not restricted", false, consol.IsPreAllocationExceededAndRestricted);
		}

		public void TestPreAllocationChecks_CalledForBatchProcessor_CalledForUserInteractive_NotCheckingHasChanges()
		{
			var isUserInteractive = Globals.IsUserInteractive;
			try
			{
				foreach (var userInteractive in new[] { true, false })
				{
					var consol = Factory.New<ForwardingConsol>();
					consol.JK_TotalShipmentCountCheck = 3;
					consol.Shipments.AddNew();
					consol.Shipments.AddNew();

					var checks = ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.Value;
					checks.ShipmentCount.Action = PreAllocationCheck.Actions.Warning;
					checks.ShipmentCount.Percentage = 50m;

					Factory.Save();

					using (ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, checks))
					{
						Globals.IsUserInteractive = userInteractive;
						consol.JK_TotalShipmentCountCheckInfo.ClearValue();

						consol.HasChanges = true;
						Factory.Save();
						AssertNoWarnings("HasChanges is not checked to initiate pre-allocation checks", consol.JK_TotalShipmentCountCheckInfo);

						consol.JK_TotalShipmentCountCheck = 2;
						Factory.Save();
						AssertHasWarnings("Changes in properties directly involved in pre-allocation would trigger checks for all users", consol.JK_TotalShipmentCountCheckInfo);
					}
				}
			}
			finally
			{
				Globals.IsUserInteractive = isUserInteractive;
			}
		}

		#endregion

		#region Consol Cut Off Date

		[TestDate(2012, 5, 01)]
		public void TestConsolCutOffDate_ShipmentsAttachedDetachedThisSession()
		{
			var creationFactory = new BusinessObjectFactory();

			var consol = creationFactory.New<ForwardingConsol>();
			var shipment1 = creationFactory.New<ForwardingShipment>();
			var shipment2 = creationFactory.New<ForwardingShipment>();
			var shipment3 = creationFactory.New<ForwardingShipment>();

			consol.Shipments.Add(shipment1);
			creationFactory.Save();

			consol = Factory.Load<ForwardingConsol>(consol.PK);

			TestDateAttribute.Date = new DateTime(2012, 5, 02);
			consol.Shipments.Remove(shipment1);

			TestDateAttribute.Date = new DateTime(2012, 5, 03);
			consol.Shipments.Add(shipment2);

			TestDateAttribute.Date = new DateTime(2012, 5, 04);
			consol.Shipments.Add(shipment3);
			consol.Shipments.Remove(shipment3);

			AssertEquals(1, consol.ShipmentsDetachedThisSession.Count);
			AssertEquals(new DateTime(2012, 5, 02), consol.ShipmentsDetachedThisSession[shipment1.PK]);

			AssertEquals(1, consol.ShipmentsAttachedThisSession.Count);
			AssertEquals(new DateTime(2012, 5, 03), consol.ShipmentsAttachedThisSession[shipment2.PK]);
		}

		[TestDate(2020, 6, 12)]
		public void TestConsolCutOffDate_ConsolsAttachedDetachedThisSession()
		{
			var creationFactory = new BusinessObjectFactory();
			var mainConsol = creationFactory.New<ForwardingConsol>();
			var coloadConsol1 = creationFactory.New<ForwardingConsol>();
			var coloadConsol2 = creationFactory.New<ForwardingConsol>();

			mainConsol.ColoadConsols.Add(coloadConsol1);
			mainConsol.ColoadConsols.Add(coloadConsol2);
			creationFactory.Save();

			AssertEquals(2, mainConsol.ColoadConsols.Count);

			mainConsol = Factory.Load<ForwardingConsol>(mainConsol.PK);
			coloadConsol1 = Factory.Load<ForwardingConsol>(coloadConsol1.PK);
			coloadConsol2 = Factory.Load<ForwardingConsol>(coloadConsol2.PK);
			var coloadConsol3 = Factory.New<ForwardingConsol>();
			var coloadConsol4 = Factory.New<ForwardingConsol>();

			TestDateAttribute.Date = new DateTime(2020, 6, 13);
			mainConsol.ColoadConsols.RemoveFromRelationship(coloadConsol1);

			TestDateAttribute.Date = new DateTime(2020, 6, 14);
			mainConsol.ColoadConsols.RemoveFromRelationship(coloadConsol2);
			mainConsol.ColoadConsols.Add(coloadConsol2);

			TestDateAttribute.Date = new DateTime(2020, 6, 15);
			mainConsol.ColoadConsols.Add(coloadConsol3);

			TestDateAttribute.Date = new DateTime(2020, 6, 16);
			mainConsol.ColoadConsols.Add(coloadConsol4);
			mainConsol.ColoadConsols.RemoveFromRelationship(coloadConsol4);

			AssertEquals(2, mainConsol.ConsolsAttachedThisSession.Count);
			AssertEquals(2, mainConsol.ConsolsDetachedThisSession.Count);

			AssertCollectionNotContains(coloadConsol1.PK, mainConsol.ConsolsAttachedThisSession.Keys);
			AssertEquals(new DateTime(2020, 6, 14), mainConsol.ConsolsAttachedThisSession[coloadConsol2.PK]);
			AssertEquals(new DateTime(2020, 6, 15), mainConsol.ConsolsAttachedThisSession[coloadConsol3.PK]);
			AssertCollectionNotContains(coloadConsol4.PK, mainConsol.ConsolsAttachedThisSession.Keys);

			AssertEquals(new DateTime(2020, 6, 13), mainConsol.ConsolsDetachedThisSession[coloadConsol1.PK]);
			AssertCollectionNotContains(coloadConsol2.PK, mainConsol.ConsolsDetachedThisSession.Keys);
			AssertCollectionNotContains(coloadConsol3.PK, mainConsol.ConsolsDetachedThisSession.Keys);
			AssertEquals(new DateTime(2020, 6, 16), mainConsol.ConsolsDetachedThisSession[coloadConsol4.PK]);
		}

		public void TestIsShipmentAttachedThisSession()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Direct;

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			consol.Shipments.Add(shipment1);
			AssertEquals(true, consol.HasUpdatedAssemblyMasterAsDirectMaster());

			consol.Shipments.RemoveAll();
			AssertEquals(false, consol.HasUpdatedAssemblyMasterAsDirectMaster());

			shipment2.Consols.Add(consol);
			AssertEquals(true, consol.HasUpdatedAssemblyMasterAsDirectMaster());
		}

		public void TestHasDirectConsolAndAssemblyMasterShipmentChanged()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Direct;
			var shipment = consol.Shipments.AddNew();
			AssertEquals(false, consol.HasUpdatedAssemblyMasterAsDirectMaster());

			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			AssertEquals(true, consol.HasUpdatedAssemblyMasterAsDirectMaster());
		}

		public void TestConsolCutOffDateSecurity()
		{
			try
			{
				ForwardingConsol consol = Factory.New<ForwardingConsol>();
				consol.JK_ConsolCutOffDateLocal = ZDateTime.Now.AddHours(-1);
				Assert("Non-saved consol. IsAllowed = true. Can edit Cut Off Date.", !consol.JK_ConsolCutOffDateLocalInfo.ReadOnly);
				Assert("Non-saved consol. IsAllowed = true. Can add shipments.", consol.Shipments.AllowAddNew);

				consol.Factory.Save();
				ZGuid consolPK = consol.PK;

				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				consol = newFactory.Load<ForwardingConsol>(consolPK);

				Assert("Saved-consol. IsAllowed = true. Can edit Cut Off Date.", !consol.JK_ConsolCutOffDateLocalInfo.ReadOnly);
				Assert("Saved consol. IsAllowed = true. Can add shipments.", consol.Shipments.AllowAddNew);

				Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed = false;

				consol = Factory.New<ForwardingConsol>();
				consol.JK_ConsolCutOffDateLocal = ZDateTime.Now.AddHours(-1);
				Assert("Non-saved consol. IsAllowed = false. Can edit Cut Off Date.", !consol.JK_ConsolCutOffDateLocalInfo.ReadOnly);
				Assert("Non-saved consol. IsAllowed = false. Can add shipments.", consol.Shipments.AllowAddNew);

				consol.Factory.Save();
				consolPK = consol.PK;

				consol = newFactory.Load<ForwardingConsol>(consolPK);

				Assert("Saved-consol. IsAllowed = false. Can't edit Cut Off Date.", consol.JK_ConsolCutOffDateLocalInfo.ReadOnly);
				Assert("Saved consol. IsAllowed = false. Can't add shipments.", !consol.Shipments.AllowAddNew);

				consol.JK_ConsolCutOffDateLocal = ZDateTime.Now.AddHours(1);
				consol.Factory.Save();

				newFactory = new BusinessObjectFactory();
				consol = newFactory.Load<ForwardingConsol>(consolPK);

				Assert("Still can't edit Cut Off Date.", consol.JK_ConsolCutOffDateLocalInfo.ReadOnly);
				Assert("Can add shipment as Cut Off Date not yet breached.", consol.Shipments.AllowAddNew);
			}
			finally
			{
				Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed = true;
			}
		}

		#endregion

		#region MAWB Allocation

		public void TestNeutralPrintedMAWBCanBeUnallocated()
		{
			JobMawb mawb = Factory.New<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "081";
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_MAWB = "55555625";
			mawb.JM_ServiceLevel = "STD";

			JobMawb mawb2 = Factory.New<JobMawb>();
			mawb2.JM_Airline3DigitPrefix = "081";
			mawb2.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb2.JM_MAWB = "55555626";
			mawb2.JM_ServiceLevel = "STD";
			Factory.Save();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;
			consol.MasterBillAirlinePrefix = "081";

			consol.JK_IsNeutralMaster = true;
			Factory.Save();
			AssertEquals("08155555625", consol.JK_MasterBillNum);
			mawb.JM_IsPrinted = true;
			Factory.Save();
			AssertEquals(true, consol.IsNeutralMAWBPrinted);

			consol.JK_IsNeutralMaster = false;
			Factory.Save();
			AssertNull(consol.MAWBAllocation.AllocatedMawb);
			AssertEquals("081", consol.JK_MasterBillNum);
			AssertEquals(false, consol.IsNeutralMAWBPrinted);

			AssertEquals(false, mawb.JM_IsPrinted);
			AssertEquals("Unallocated", ZGuid.Empty, mawb.JM_ParentID);

			consol.JK_IsNeutralMaster = true;
			bool shouldCancelMAWBUnallocation = false;
			consol.DeallocatePrintedNeutralMAWB += (sender, e) => e.Cancel = shouldCancelMAWBUnallocation;
			Factory.Save();
			AssertEquals("08155555625", consol.JK_MasterBillNum);
			mawb.JM_IsPrinted = true;
			Factory.Save();
			AssertEquals(true, consol.IsNeutralMAWBPrinted);

			shouldCancelMAWBUnallocation = true;
			consol.JK_IsNeutralMaster = false;
			AssertEquals(true, consol.JK_IsNeutralMaster);
			AssertEquals(mawb, consol.MAWBAllocation.AllocatedMawb);
			AssertEquals("08155555625", consol.JK_MasterBillNum);
			AssertEquals(true, consol.IsNeutralMAWBPrinted);

			shouldCancelMAWBUnallocation = false;
			consol.JK_IsNeutralMaster = false;
			Factory.Save();
			AssertNull(consol.MAWBAllocation.AllocatedMawb);
			AssertEquals("081", consol.JK_MasterBillNum);
			AssertEquals(false, consol.IsNeutralMAWBPrinted);
		}

		public void TestNeutralPrintedMAWBCanBeUnallocated_NeedsJobMAWBResetPrintedFlagAccess()
		{
			var mawb = AddMawb(Factory, "081", "55555625", GlbBranch.CurrentBranch, "STD");

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;
			consol.MasterBillAirlinePrefix = "081";

			consol.JK_IsNeutralMaster = true;
			Factory.Save();
			AssertEquals("08155555625", consol.JK_MasterBillNum);
			mawb.JM_IsPrinted = true;
			Factory.Save();
			AssertEquals(true, consol.IsNeutralMAWBPrinted);

			bool deallocateCalled = false;

			consol.DeallocatePrintedNeutralMAWB += (sender, e) => { deallocateCalled = true; e.Cancel = false; };

			Env.Security.JobMAWBResetPrintedFlag.IsAllowed = false;

			consol.JK_IsNeutralMaster = false;
			Factory.Save();
			AssertNotNull(consol.MAWBAllocation.AllocatedMawb);
			AssertEquals("08155555625", consol.JK_MasterBillNum);
			AssertEquals(true, consol.IsNeutralMAWBPrinted);
			Assert("Deallocate should not be called", !deallocateCalled);

			AssertEquals(true, mawb.JM_IsPrinted);
			AssertEquals("Still allocated", consol.PK, mawb.JM_ParentID);

			Env.Security.JobMAWBResetPrintedFlag.IsAllowed = true;

			consol.JK_IsNeutralMaster = false;
			Factory.Save();
			AssertNull(consol.MAWBAllocation.AllocatedMawb);
			AssertEquals("081", consol.JK_MasterBillNum);
			AssertEquals(false, consol.IsNeutralMAWBPrinted);
			Assert("Deallocate should be called", deallocateCalled);

			AssertEquals(false, mawb.JM_IsPrinted);
			AssertEquals("Unallocated", ZGuid.Empty, mawb.JM_ParentID);
		}

		public void TestAllocateMAWB_ShouldNotReallocatePrintedUnallocatedMAWB_WhenMasterBillAirlinePrefixHasChanged()
		{
			var mawb1 = AddMawb(Factory, "081", "55555625", GlbBranch.CurrentBranch, "STD");
			var mawb2 = AddMawb(Factory, "076", "12445628", GlbBranch.CurrentBranch, "STD");

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;
			consol.MasterBillAirlinePrefix = "081";

			consol.JK_IsNeutralMaster = true;
			Factory.Save();
			AssertEquals("08155555625", consol.JK_MasterBillNum);
			mawb1.JM_IsPrinted = true;
			Factory.Save();
			AssertEquals(true, consol.IsNeutralMAWBPrinted);

			bool deallocateCalled = false;
			bool reallocateCalled = false;
			consol.OnReallocatingPrintedMawb += (sender, e) => { reallocateCalled = true; e.Cancel = false; };
			consol.DeallocatePrintedNeutralMAWB += (sender, e) => { deallocateCalled = true; e.Cancel = false; };
			Env.Security.JobMAWBResetPrintedFlag.IsAllowed = true;
			consol.JK_IsNeutralMaster = false;
			Factory.Save();

			AssertNull(consol.MAWBAllocation.AllocatedMawb);
			AssertEquals("081", consol.JK_MasterBillNum);
			AssertEquals(false, consol.IsNeutralMAWBPrinted);

			consol.MasterBillAirlinePrefix = "076";
			consol.JK_IsNeutralMaster = true;

			Factory.Save();

			AssertEquals(true, deallocateCalled);
			AssertEquals(false, reallocateCalled);
			AssertEquals("MAWB2 should be allocated", $"{mawb2.JM_Airline3DigitPrefix}{mawb2.JM_MAWB}", consol.JK_MasterBillNum);

			var mawb3 = AddMawb(Factory, "174", "36759181", GlbBranch.CurrentBranch, "STD");
			mawb2.JM_IsPrinted = true;
			Factory.Save();
			AssertEquals(true, consol.IsNeutralMAWBPrinted);

			reallocateCalled = false;
			consol.JK_IsNeutralMaster = false;
			Factory.Save();

			AssertNull(consol.MAWBAllocation.AllocatedMawb);
			AssertEquals("076", consol.JK_MasterBillNum);
			consol.JK_IsNeutralMaster = true;
			AssertEquals("MAWB2 should be reallocated", true, reallocateCalled);
			consol.MasterBillAirlinePrefix = "174";

			reallocateCalled = false;
			Factory.Save();

			AssertEquals("MAWB2 should not be reallocated because master bill airline prefix has been changed", false, reallocateCalled);
			AssertEquals("MAWB3 should be allocated", $"{mawb3.JM_Airline3DigitPrefix}{mawb3.JM_MAWB}", consol.JK_MasterBillNum);
		}

		public void TestAgentTypeSetter_AgentTypeIsDirectMAWBIsAllocatedFinalMasterIsPrintedUnallocationIsNotAllowed_DoNotChangeAgentType()
		{
			var mawb = AddMawb(Factory, "081", "55555625", GlbBranch.CurrentBranch, "STD");
			var mawb2 = AddMawb(Factory, "081", "55555626", GlbBranch.CurrentBranch, "STD");

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;
			consol.MasterBillAirlinePrefix = "081";
			consol.JK_IsNeutralMaster = true;
			Factory.Save();

			consol.MAWBAllocation.AllocatedMawb.JM_IsPrinted = true;
			Factory.Save();

			consol.DeallocatePrintedNeutralMAWB += (s, e) => e.Cancel = true;

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			AssertEquals("JK_AgentType should not be changed", Constants.AgentType.Direct, consol.JK_AgentType);
			AssertEquals("MAWB should still be allocated to consol", "08155555625", consol.JK_MasterBillNum);
			AssertEquals("JK_IsNeutralMaster", true, consol.JK_IsNeutralMaster);
		}

		public void TestAgentTypeSetter_AgentTypeIsDirectMAWBIsAllocatedFinalMasterIsPrintedUnallocationIsAllowed_ChangeAgentType()
		{
			var mawb = AddMawb(Factory, "081", "55555625", GlbBranch.CurrentBranch, "STD");
			var mawb2 = AddMawb(Factory, "081", "55555626", GlbBranch.CurrentBranch, "STD");

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;
			consol.MasterBillAirlinePrefix = "081";
			consol.JK_IsNeutralMaster = true;
			mawb.JM_IsPrinted = true;

			Factory.Save();

			consol.DeallocatePrintedNeutralMAWB += (s, e) => e.Cancel = false;

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			AssertEquals("JK_AgentType should not be changed", Constants.AgentType.CoLoad, consol.JK_AgentType);
			AssertEquals("JK_IsNeutralMaster", false, consol.JK_IsNeutralMaster);
		}

		public void TestAgentTypeSetter_AgentTypeCourierNotAllowedWhenAWBIsPrintedAndUnalloacatactionProhibited()
		{
			var mawb = AddMawb(Factory, "081", "55555625", GlbBranch.CurrentBranch, "STD");

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;
			consol.MasterBillAirlinePrefix = "081";
			consol.JK_IsNeutralMaster = true;

			Factory.Save();

			mawb.JM_IsPrinted = true;
			consol.DeallocatePrintedNeutralMAWB += (s, e) => e.Cancel = true;
			consol.JK_AgentType = Constants.AgentType.Courier;

			AssertEquals("JK_AgentType should not be changed", Constants.AgentType.Agent, consol.JK_AgentType);
			AssertEquals("JK_IsNeutralMaster", true, consol.JK_IsNeutralMaster);
		}

		public void TestAgentTypeSetter_AgentTypeCourierAllowedWhenAWBIsPrintedAndUnalloacatactionAllowed()
		{
			var mawb = AddMawb(Factory, "081", "55555625", GlbBranch.CurrentBranch, "STD");

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;
			consol.MasterBillAirlinePrefix = "081";
			consol.JK_IsNeutralMaster = true;

			Factory.Save();

			mawb.JM_IsPrinted = true;
			consol.DeallocatePrintedNeutralMAWB += (s, e) => e.Cancel = false;
			consol.JK_AgentType = Constants.AgentType.Courier;

			AssertEquals("JK_AgentType change to Courier is allowed", Constants.AgentType.Courier, consol.JK_AgentType);
			AssertEquals("JK_IsNeutralMaster", false, consol.JK_IsNeutralMaster);
		}

		public void TestTransportModeSetter_NewValueIsNotAirANDFinalMasterIsPrintedANDUnallocationIsAllowed_ChangeTransportType()
		{
			var mawb = AddMawb(Factory, "081", "55555625", GlbBranch.CurrentBranch, "STD");
			var mawb2 = AddMawb(Factory, "081", "55555626", GlbBranch.CurrentBranch, "STD");

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;
			consol.MasterBillAirlinePrefix = "081";
			consol.JK_IsNeutralMaster = true;
			Factory.Save();

			mawb.JM_IsPrinted = true;
			Factory.Save();

			consol.DeallocatePrintedNeutralMAWB += (s, e) => e.Cancel = false;

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("MAWB should be marked for unallocation", true, consol.MAWBAllocation.GetShouldDeallocate());
			AssertEquals("JK_TransportMode", Constants.TransportModes.Sea, consol.JK_TransportMode);
			AssertEquals("JK_MasterBillNum", ZString.Empty, consol.JK_MasterBillNum);
			AssertEquals("JK_IsNeutralMaster", false, consol.JK_IsNeutralMaster);
		}

		public void TestTransportModeSetter_NewValueIsNotAirANDFinalMasterIsPrintedANDUnallocationIsNotAllowed_DoNotChangeTransportType()
		{
			var mawb = AddMawb(Factory, "081", "55555625", GlbBranch.CurrentBranch, "STD");

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;
			consol.MasterBillAirlinePrefix = "081";
			consol.JK_IsNeutralMaster = true;
			Factory.Save();

			consol.MAWBAllocation.AllocatedMawb.JM_IsPrinted = true;
			Factory.Save();

			consol.DeallocatePrintedNeutralMAWB += (s, e) => e.Cancel = true;

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("JK_TransportMode should not be changed", Constants.TransportModes.Air, consol.JK_TransportMode);
			AssertEquals("JK_MasterBillNum", "08155555625", consol.JK_MasterBillNum);
			AssertEquals("JK_IsNeutralMaster", true, consol.JK_IsNeutralMaster);
			AssertEquals("MAWB should not be unallocated", consol.PK, mawb.JM_ParentID);
		}

		public void TestImportMawbs()
		{
			RefAirline airline = Factory.LoadTop1<RefAirline>(new ZQuery());

			JobMawb mawb = Factory.New<JobMawb>();
			mawb.JM_Airline3DigitPrefix = airline.RM_EagleAddedAirlinePrefixOrAccountingCode;
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_MAWB = "55555625";
			mawb.JM_ServiceLevel = "STD";

			Factory.Save();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AWBServiceLevel = "STD";
			consol.Transports[0].JW_RL_NKLoadPort = OverseasPort;
			consol.Transports[0].JW_RL_NKDiscPort = HomePort;
			consol.MasterBillAirlinePrefix = "081";

			AssertEquals("MasterBillMAWB", "", consol.MasterBillMAWB);
			AssertEquals("MasterBillMAWBInfo.ReadOnly", false, consol.MasterBillMAWBInfo.ReadOnly);
			AssertEquals("JK_IsNeutralMaster", false, consol.JK_IsNeutralMaster);
		}

		public void TestMAWBShouldBeUnallocated()
		{
			JobMawb mawb = Factory.New<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "081";
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_MAWB = "55555625";
			mawb.JM_ServiceLevel = "STD";

			JobMawb mawb2 = Factory.New<JobMawb>();
			mawb2.JM_Airline3DigitPrefix = "081";
			mawb2.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb2.JM_MAWB = "55555626";
			mawb2.JM_ServiceLevel = "STD";

			JobMawb mawb3 = Factory.New<JobMawb>();
			mawb3.JM_Airline3DigitPrefix = "081";
			mawb3.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb3.JM_MAWB = "55555627";
			mawb3.JM_ServiceLevel = "STD";

			Factory.Save();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;
			consol.MasterBillAirlinePrefix = "081";

			consol.JK_IsNeutralMaster = true;

			consol.Factory.Save();

			AssertEquals("08155555625", consol.JK_MasterBillNum);
			AssertEquals("mawb parent", consol.PK, mawb.JM_ParentID);
			AssertEquals("mawb parent table code", consol.Prefix, mawb.JM_ParentTableCode);

			Factory.Save();

			consol.JK_IsNeutralMaster = false;
			AssertEquals("MAWB should be marked for unallocation", true, consol.MAWBAllocation.GetShouldDeallocate());
			AssertEquals("081", consol.JK_MasterBillNum);
			consol.Factory.Save();
			AssertEquals("Unallocated", ZGuid.Empty, mawb.JM_ParentID);
			AssertEquals("Unallocated", "", mawb.JM_ParentTableCode);

			consol.JK_IsNeutralMaster = true;
			consol.Factory.Save();
			AssertEquals("08155555625", consol.JK_MasterBillNum);
			AssertEquals("mawb parent", consol.PK, mawb.JM_ParentID);
			AssertEquals("mawb parent table code", consol.Prefix, mawb.JM_ParentTableCode);

			consol.JK_IsNeutralMaster = false;
			Factory.Save();

			AssertEquals(null, consol.MAWBAllocation.AllocatedMawb);
			AssertEquals("081", consol.JK_MasterBillNum);
			AssertEquals("Unallocated", ZGuid.Empty, mawb.JM_ParentID);
			AssertEquals("Unallocated", "", mawb.JM_ParentTableCode);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ForwardingConsol consol2 = newFactory.New<ForwardingConsol>();
			consol2.JK_AgentType = Constants.AgentType.Agent;
			consol2.JK_TransportMode = Constants.TransportModes.Air;
			consol2.JK_AWBServiceLevel = "STD";
			consol2.JK_RL_NKLoadPort = HomePort;
			consol2.JK_RL_NKDischargePort = OverseasPort;
			consol2.MasterBillAirlinePrefix = "081";

			consol2.JK_IsNeutralMaster = true;
			consol2.Factory.Save();
			AssertEquals("08155555625", consol2.JK_MasterBillNum);
			JobMawb mawb_Factory2 = newFactory.Load<JobMawb>(mawb.PK);
			AssertEquals("mawb parent", consol2.PK, mawb_Factory2.JM_ParentID);
			AssertEquals("mawb parent table code", consol2.Prefix, mawb_Factory2.JM_ParentTableCode);

			consol.JK_IsNeutralMaster = true;
			consol.Factory.Save();
			AssertEquals("08155555626", consol.JK_MasterBillNum);
			AssertEquals("mawb parent", consol.PK, mawb2.JM_ParentID);
			AssertEquals("mawb parent table code", consol.Prefix, mawb2.JM_ParentTableCode);

			consol.JK_IsNeutralMaster = false;
			consol.Factory.Save();

			consol.JK_IsNeutralMaster = true;
			consol.Factory.Save();
			AssertEquals("08155555626", consol.JK_MasterBillNum);
			AssertEquals("mawb parent", consol.PK, mawb2.JM_ParentID);
			AssertEquals("mawb parent table code", consol.Prefix, mawb2.JM_ParentTableCode);
		}

		public void TestIMAWBAllocationParentNoteParent()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			IMAWBAllocationParent parent = consol;
			AssertEquals(consol, parent.NotesParent);
		}

		public void TestOnReallocatingPrintedMawb()
		{
			EventHandler<JobMawbReallocationEventArgs> handler = (s, e) => { };

			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			MulticastDelegate mawbAllocatiorEventHandlers = GetEventHandler("OnReallocatingPrintedMawb", consol.MAWBAllocation);
			AssertNull(mawbAllocatiorEventHandlers);

			consol.OnReallocatingPrintedMawb += handler;

			mawbAllocatiorEventHandlers = GetEventHandler("OnReallocatingPrintedMawb", consol.MAWBAllocation);
			AssertNotNull(mawbAllocatiorEventHandlers);
			AssertEquals(true, mawbAllocatiorEventHandlers.GetInvocationList().Contains(handler));

			consol.OnReallocatingPrintedMawb -= handler;

			mawbAllocatiorEventHandlers = GetEventHandler("OnReallocatingPrintedMawb", consol.MAWBAllocation);
			AssertNull(mawbAllocatiorEventHandlers);
		}

		MulticastDelegate GetEventHandler(string eventName, object source)
		{
			System.Reflection.FieldInfo fieldInfo = source.GetType().GetField(eventName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
			return fieldInfo != null ? (MulticastDelegate)fieldInfo.GetValue(source) : null;
		}

		public void TestPrintedMawbCanBeReallocated()
		{
			JobMawb mawb1 = NewMawb("081", "00000011");

			Factory.Save();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;
			consol.MasterBillAirlinePrefix = "081";

			consol.Factory.Save();

			AssertEquals("08100000011", consol.JK_MasterBillNum);
			AssertEquals("mawb parent", consol.PK, mawb1.JM_ParentID);
			AssertEquals("mawb parent table code", consol.Prefix, mawb1.JM_ParentTableCode);

			mawb1.JM_IsPrinted = true;

			Factory.Save();

			JobMawb mawbPassedToEvent = null;
			consol.OnReallocatingPrintedMawb += (s, e) =>
			{
				mawbPassedToEvent = e.Mawb;
			};

			consol.JK_IsNeutralMaster = false;
			AssertEquals("081", consol.JK_MasterBillNum);
			AssertEquals("MAWB should be marked for unallocation", true, consol.MAWBAllocation.GetShouldDeallocate());

			Factory.Save();

			consol.JK_IsNeutralMaster = true;
			consol.Factory.Save();
			AssertEquals("08100000011", consol.JK_MasterBillNum);
			AssertEquals("mawb parent", consol.PK, mawb1.JM_ParentID);
			AssertEquals("mawb parent table code", consol.Prefix, mawb1.JM_ParentTableCode);

			AssertEquals(mawb1, mawbPassedToEvent);
			consol.Factory.Save();
			mawb1.Reload();
			AssertEquals("mawb parent", consol.PK, mawb1.JM_ParentID);
			AssertEquals("mawb parent table code", consol.Prefix, mawb1.JM_ParentTableCode);
		}

		public void TestPrintedUnallocatedMawbDoesReallocateByDefault()
		{
			JobMawb mawb1 = NewMawb("081", "00000011");
			JobMawb mawb2 = NewMawb("081", "00000022");
			JobMawb mawb3 = NewMawb("081", "00000033");

			Factory.Save();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;
			consol.MasterBillAirlinePrefix = "081";

			consol.Factory.Save();

			AssertEquals("08100000011", consol.JK_MasterBillNum);
			AssertEquals("mawb parent", consol.PK, mawb1.JM_ParentID);
			AssertEquals("mawb parent table code", consol.Prefix, mawb1.JM_ParentTableCode);

			mawb1.JM_IsPrinted = true;

			Factory.Save();

			consol.JK_IsNeutralMaster = false;
			AssertEquals("081", consol.JK_MasterBillNum);
			AssertEquals("Mawb should be marked for unallocation", true, consol.MAWBAllocation.GetShouldDeallocate());

			Factory.Save();

			consol.JK_IsNeutralMaster = true;
			consol.Factory.Save();
			AssertEquals("08100000011", consol.JK_MasterBillNum);

			AssertEquals("mawb parent", consol.PK, mawb1.JM_ParentID);
			AssertEquals("mawb parent table code", consol.Prefix, mawb1.JM_ParentTableCode);
		}

		JobMawb NewMawb(ZString airlinePrefix, ZString mawbNo)
		{
			JobMawb mawb = Factory.New<JobMawb>();
			mawb.JM_Airline3DigitPrefix = airlinePrefix;
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_MAWB = mawbNo;
			mawb.JM_ServiceLevel = OrgCarrierServiceLevel.StandardCode;

			return mawb;
		}

		#endregion

		#region TestIsOverrideAllowed

		public void TestForwardingConsol_IsOverrideAllowed_WhenSecurityCheckpoint_IsGranted()
		{
			var securityInstance = SecurityTestHelper.CreateSecurityInstance(Factory);
			securityInstance.MaintainConsolAWBOverride.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(securityInstance))
			{
				var forwardingConsol = Factory.New<ForwardingConsol>();

				Assert("IsOverrideAllowed returns true when security checkpoint permission is granted", forwardingConsol.IsOverrideAllowed);
			}
		}

		public void TestForwardingConsol_IsOverrideAllowed_WhenSecurityCheckpoint_IsDenied()
		{
			var securityInstance = SecurityTestHelper.CreateSecurityInstance(Factory);
			securityInstance.MaintainConsolAWBOverride.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityInstance))
			{
				var forwardingConsol = Factory.New<ForwardingConsol>();

				AssertEquals("IsOverrideAllowed returns false when security checkpoint permission is denied", false, forwardingConsol.IsOverrideAllowed);
			}
		}

		#endregion

		#region TestMaintainConsol_OverrideMaintainConsol_WaybillDefaultsSecurityCheckpoint

		public void TestForwardingConsol_OverrideWaybillDefaults_WithPermission_WorksAsExpected()
		{
			var forwardingConsol1 = Factory.New<ForwardingConsol>();
			forwardingConsol1.JK_OverrideWaybillDefaults = false;
			var forwardingConsol2 = Factory.New<ForwardingConsol>();
			forwardingConsol2.JK_OverrideWaybillDefaults = true;

			var securityInstance = SecurityTestHelper.CreateSecurityInstance(Factory);
			securityInstance.MaintainConsolAWBOverride.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(securityInstance))
			{
				forwardingConsol1.JK_OverrideWaybillDefaults = true;
				forwardingConsol2.JK_OverrideWaybillDefaults = false;

				CombineAssertions("Changing JK_OverrideWaybillDefaults with permission should succeed", () =>
				{
					AssertEquals("forwardingConsol1.JK_OverrideWaybillDefaults should now be true", true, forwardingConsol1.JK_OverrideWaybillDefaults);
					AssertEquals("forwardingConsol2.JK_OverrideWaybillDefaults should now be false", false, forwardingConsol2.JK_OverrideWaybillDefaults);
				});
			}
		}

		public void TestForwardingConsol_OverrideWaybillDefaults_WithoutPermission_Fails()
		{
			var forwardingConsol1 = Factory.New<ForwardingConsol>();
			forwardingConsol1.JK_OverrideWaybillDefaults = false;
			var forwardingConsol2 = Factory.New<ForwardingConsol>();
			forwardingConsol2.JK_OverrideWaybillDefaults = true;

			var securityInstance = SecurityTestHelper.CreateSecurityInstance(Factory);
			securityInstance.MaintainConsolAWBOverride.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityInstance))
			{
				forwardingConsol1.JK_OverrideWaybillDefaults = true;
				forwardingConsol2.JK_OverrideWaybillDefaults = false;

				CombineAssertions("Changing JK_OverrideWaybillDefaults without permission should fail", () =>
				{
					AssertEquals("forwardingConsol1.JK_OverrideWaybillDefaults should still be false", false, forwardingConsol1.JK_OverrideWaybillDefaults);
					AssertEquals("forwardingConsol2.JK_OverrideWaybillDefaults should still be true", true, forwardingConsol2.JK_OverrideWaybillDefaults);
				});
			}
		}

		#endregion

		#region AreAllShipmentsApprovedForAviationSecurity

		public void TestAreAllShipmentsApprovedForAviationSecurity()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			ForwardingShipment shipment3 = consol.Shipments.AddNew();

			shipment1.JS_InspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;
			shipment2.JS_InspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;
			shipment3.JS_InspectionTypeCode = ZString.Empty;

			AssertEquals(consol.AreAllShipmentsApprovedForAviationSecurity, false);

			shipment1.JS_InspectionTypeCode = "XRY";
			AssertEquals(consol.AreAllShipmentsApprovedForAviationSecurity, false);

			shipment2.JS_InspectionTypeCode = "XRY";
			AssertEquals(consol.AreAllShipmentsApprovedForAviationSecurity, false);

			shipment3.JS_InspectionTypeCode = "XRY";
			Assert(consol.AreAllShipmentsApprovedForAviationSecurity);
		}

		#endregion

		#region TestGetAviationSecurityCode

		public void TestGetAviationSecurityCode()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_InspectionTypeCode = ZString.Empty;

			FreightDataRegistry.Instance.EnableSupplyChainSecurity_AU.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "GBLON";

			var aviationSecurityCode = consol.GetAviationSecurityCode();

			AssertEquals("NSC", aviationSecurityCode);
		}

		#endregion

		#region TestGetAviationSecurityCodeNotDefaultToSPX

		public void TestGetAviationSecurityCodeDefaultToSPX()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "DE222";

			var aviationSecurityCode = consol.GetAviationSecurityCode();
			Assert(consol.AreAllShipmentsApprovedForAviationSecurity);
			AssertEquals("SPX", aviationSecurityCode);
		}

		#endregion

		#region TestForwardingChangingContainerOnAPacklineRecalculatesGrossWeight

		public void TestForwardingChangingContainerOnAPacklineRecalculatesGrossWeight()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.Groupage;

			ForwardingContainer container = consol.Containers.AddNew();
			var refC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			container.JC_RC = refC.PK;
			container.JC_ContainerMode = Constants.ContainerModes.LCL;

			Factory.Save();

			AssertEquals("Gross weight should be tare weight", 2280m, container.JC_GrossWeight);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ActualWeight = 1000m;
			shipment.JS_ActualVolume = 500m;
			shipment.JS_OuterPacks = 50;

			factory2.Save();

			shipment = Factory.Load<ForwardingShipment>(shipment.PK);
			consol.Shipments.Add(shipment);

			Factory.Save();

			shipment = factory2.Load<ForwardingShipment>(shipment.PK);
			consol = factory2.Load<ForwardingConsol>(consol.PK);
			container = factory2.Load<ForwardingContainer>(container.PK);

			AssertEquals("Shipment packline has been packed into container on consol", container.PK, shipment.OuterPackLines[0].JL_JC);
			AssertEquals("Container GrossWeight should include packline weight", 3280m, container.JC_GrossWeight);
			AssertEquals("Container GrossWeight should be saved", false, container.HasChanges);

			shipment.OuterPackLines[0].JL_JC = ZGuid.Empty;
			factory2.Save();

			AssertEquals("Shipment packline has been packed into container on consol", ZGuid.Empty, shipment.OuterPackLines[0].JL_JC);
			AssertEquals("Container GrossWeight, packline removed", 2280m, container.JC_GrossWeight);
			AssertEquals("Container GrossWeight should be saved", false, container.HasChanges);

			shipment.OuterPackLines[0].JL_JC = container.PK;
			factory2.Save();

			AssertEquals("Shipment packline has been packed into container on consol", container.PK, shipment.OuterPackLines[0].JL_JC);
			AssertEquals("Container GrossWeight, packline re-attached", 3280m, container.JC_GrossWeight);
			AssertEquals("Container GrossWeight should be saved", false, container.HasChanges);
		}

		#endregion

		#region TestDefaultingMasterBillAirlinePrefixFromFlight1

		public void TestDefaultingMasterBillAirlinePrefixFromFlight1()
		{
			RefAirline qF = RefAirline.LoadFromAirline2LetterCode(Factory, "QF");
			RefAirline cX = RefAirline.LoadFromAirline2LetterCode(Factory, "CX");

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_IsNeutralMaster = true;

			Transport transport1 = consol.Transports[0];

			transport1.JW_VoyageFlight = "QF1234";
			AssertEquals("Default MAWB from Flight1.", qF.RM_EagleAddedAirlinePrefixOrAccountingCode, consol.MasterBillAirlinePrefix);

			transport1.JW_TransportType = Core.Constants.TransportPlanningType.Flight2;
			AssertEquals("Don't clear MAWB yet, it might still be valid.", qF.RM_EagleAddedAirlinePrefixOrAccountingCode, consol.MasterBillAirlinePrefix);

			Transport transport2 = consol.Transports.AddNew();
			transport2.JW_VoyageFlight = "CX1234";
			AssertEquals("Don't clear MAWB yet, as it corresponds to another flight leg.", qF.RM_EagleAddedAirlinePrefixOrAccountingCode, consol.MasterBillAirlinePrefix);

			transport1.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;
			AssertEquals("Don't update MAWB yet, as it corresponds to another flight leg. There are 2 conflicting flights of type Flight1.", qF.RM_EagleAddedAirlinePrefixOrAccountingCode, consol.MasterBillAirlinePrefix);

			transport2.JW_TransportType = Core.Constants.TransportPlanningType.Flight2;
			AssertEquals("Don't update MAWB yet, as it corresponds to another flight leg.", qF.RM_EagleAddedAirlinePrefixOrAccountingCode, consol.MasterBillAirlinePrefix);

			transport2.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;
			AssertEquals("Don't update MAWB yet, as it corresponds to another flight leg. There are 2 conflicting flights of type Flight1.", qF.RM_EagleAddedAirlinePrefixOrAccountingCode, consol.MasterBillAirlinePrefix);

			transport1.JW_VoyageFlight = "CX4321";
			AssertEquals("Update MAWB, as it no longer corresponds to any flight legs.", cX.RM_EagleAddedAirlinePrefixOrAccountingCode, consol.MasterBillAirlinePrefix);
		}

		public void TestDefaultingMasterBillAirlinePrefixFromFlight1_OnlyWhenFL1IsInvolved()
		{
			RefAirline qF = RefAirline.LoadFromAirline2LetterCode(Factory, "QF");
			RefAirline cX = RefAirline.LoadFromAirline2LetterCode(Factory, "CX");

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_IsNeutralMaster = true;

			Transport transport1 = consol.Transports[0];

			transport1.JW_VoyageFlight = "QF1234";
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;

			Transport transport2 = consol.Transports.AddNew();
			transport2.JW_VoyageFlight = "CX1234";
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;

			consol.MasterBillAirlinePrefix = cX.RM_EagleAddedAirlinePrefixOrAccountingCode;
			AssertEquals("Don't update MAWB yet, as it corresponds to another flight leg. There are 2 conflicting flights of type Flight1.", cX.RM_EagleAddedAirlinePrefixOrAccountingCode, consol.MasterBillAirlinePrefix);

			transport2.JW_TransportType = Core.Constants.TransportPlanningType.Flight2;
			AssertEquals("Don't update MAWB yet, as it corresponds to another flight leg.", cX.RM_EagleAddedAirlinePrefixOrAccountingCode, consol.MasterBillAirlinePrefix);

			consol.MasterBillAirlinePrefix = qF.RM_EagleAddedAirlinePrefixOrAccountingCode;
			AssertEquals("Manually overriden MAWB", qF.RM_EagleAddedAirlinePrefixOrAccountingCode, consol.MasterBillAirlinePrefix);

			Transport transport3 = consol.Transports.AddNew();
			AssertEquals("Ensure Transport3 is type Flight 3", Core.Constants.TransportPlanningType.Flight3, transport3.JW_TransportType);
			AssertEquals("Ensure overriden MAWB doesn't change.", qF.RM_EagleAddedAirlinePrefixOrAccountingCode, consol.MasterBillAirlinePrefix);

			transport3.JW_VoyageFlight = "CX4321";
			AssertEquals("Flight changed to type Flight 3. Don't update MAWB yet, as it corresponds to another flight leg.", qF.RM_EagleAddedAirlinePrefixOrAccountingCode, consol.MasterBillAirlinePrefix);

			transport3.JW_TransportType = Core.Constants.TransportPlanningType.Flight2;
			AssertEquals("Flight change doesn't invlove Flight 1. Don't update MAWB yet, as it corresponds to another flight leg.", qF.RM_EagleAddedAirlinePrefixOrAccountingCode, consol.MasterBillAirlinePrefix);

			transport3.JW_TransportType = Core.Constants.TransportPlanningType.Flight3;
			AssertEquals("Flight change doesn't invlove Flight 1. Don't update MAWB yet, as it corresponds to another flight leg.", qF.RM_EagleAddedAirlinePrefixOrAccountingCode, consol.MasterBillAirlinePrefix);

			transport2.JW_TransportType = Core.Constants.TransportPlanningType.Flight2;
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;
			transport1.JW_VoyageFlight = "CX3333";
			AssertEquals("Flight of type Flight 1 changed. Update MAWB, as it no longer corresponds to any flight leg", cX.RM_EagleAddedAirlinePrefixOrAccountingCode, consol.MasterBillAirlinePrefix);
		}

		public void TestDefaultingMasterBillAirlinePrefixServiceLevels()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			AddMawb(factory, "081", "00000011", GlbBranch.CurrentBranch, "STD");
			AddMawb(factory, "001", "00000011", GlbBranch.CurrentBranch, "ALL");

			factory.Save();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.MasterBillAirlinePrefix = "081";

			AssertEquals("should auto allocate MAWB with STD service level", true, consol.JK_IsNeutralMaster);
			AssertEquals("should be pending allocation", true, consol.MAWBAllocation.GetShouldAllocate());

			consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.MasterBillAirlinePrefix = "001";

			AssertEquals("should auto allocate MAWB with ALL service level", true, consol.JK_IsNeutralMaster);
			AssertEquals("should be pending allocation", true, consol.MAWBAllocation.GetShouldAllocate());
		}

		#endregion

		#region TestChangingTheMasterBillShouldUpdateTheHouseBillsIfTheSame

		public void TestMasetrHouseBillNonPropagation_DirectSea()
		{
			GenericMasterHouseBillDoesNotPropagate(Core.Constants.TransportModes.Sea, Core.Constants.AgentType.Direct);
		}

		public void TestMasetrHouseBillNonPropagation_AgentAir()
		{
			GenericMasterHouseBillDoesNotPropagate(Core.Constants.TransportModes.Air, Core.Constants.AgentType.Agent);
		}

		public void TestMasetrHouseBillNonPropagation_DirectAir()
		{
			GenericMasterHouseBillDoesNotPropagate(Core.Constants.TransportModes.Air, Core.Constants.AgentType.Direct);
		}

		public void GenericMasterHouseBillDoesNotPropagate(ZString transportMode, ZString agentType)
		{
			string bill1 = "08111111111";
			string bill2 = "08122222222";
			string bill3 = "08133333333";

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = transportMode;
			consol.JK_AgentType = agentType;
			consol.JK_MasterBillNum = bill1;

			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = bill1;

			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = bill2;

			consol.JK_MasterBillNum = bill3;

			AssertEquals("Shipment1 should not be updated", bill1, shipment1.JS_HouseBill);

			AssertEquals("Shipment2 should not be changed as it had a different bill", bill2, shipment2.JS_HouseBill);
		}

		#endregion

		#region TestNoteTypes

		public void TestForwardingNoteTypes()
		{
			var consolNoteTypes = Factory.New<ForwardingConsol>().NoteTypes.Cast<PredefinedNoteType>();
			AssertEquals("Expecting AgentNotes", true, consolNoteTypes.Any(x => x == PredefinedNoteTypes.Instance.AgentNotes));
			AssertEquals("Expecting CarrierBookingRequestNotes", true, consolNoteTypes.Any(x => x == PredefinedNoteTypes.Instance.CarrierBookingRequest));
			AssertEquals("Expecting PortMessageRemarks", true, consolNoteTypes.Any(x => x == PredefinedNoteTypes.Instance.PortMessageRemarks));
		}

		#endregion

		#region TestRemovingShipmentFromConsolRemovesTheShipmentFromAnyContainersOnTheConsol

		public void TestRemovingShipmentFromConsolRemovesTheShipmentFromAnyContainersOnTheConsol()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ForwardingPackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 5;

			ForwardingConsol consol = (ForwardingConsol)GetNewConsol();
			ForwardingContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			consol.Shipments.Add(shipment);
			AssertEquals("precondition: Should be packed ", true, packLine.Containers.Contains(container.PK));
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			ForwardingConsol consol2 = factory2.Load<ForwardingConsol>(consol.PK);
			consol2.Shipments.RemoveAll();

			ForwardingPackLine packLine2 = factory2.Load<ForwardingPackLine>(packLine.PK);
			AssertEquals("The packLine should not be packed into the container", false, packLine2.Containers.Contains(container.PK));
		}

		#endregion

		#region TestSeaCargoOceanBillAndContainersIncludedInLogs

		public void TestSeaCargoOceanBillAndContainersAndHousesIncludedInLogs()
		{
			var consol = (ForwardingConsol)GetNewConsol();
			var initialLogsCount = consol.Logs.GetAllLogs().Count;
			var seaCargoOceanBill = (BusinessObject)Factory.New<IAUCusSCAOceanBill>();
			seaCargoOceanBill[CusSCAOceanBillSchema.CB_ParentId.Name] = consol.PK;
			seaCargoOceanBill[CusSCAOceanBillSchema.CB_ParentTableCode.Name] = JobConsolSchema.Constants.Prefix;
			seaCargoOceanBill.GetLogs().AddNew(Events.DeclarationSentToCustoms);
			var consolLogs = new StmALogCollectionView(consol);
			var countWithOceanBill = consolLogs.Count;
			Assert("Ocean Bill Logs should appear in Logs", countWithOceanBill > initialLogsCount);

			var seaCargoContainer = (BusinessObject)Factory.New<AU.ICusSCAContainer>();
			seaCargoContainer[CusSCAContainerSchema.CN_CB.Name] = seaCargoOceanBill.PK;
			seaCargoContainer.GetLogs().AddNew(Events.Arrival);
			consolLogs = new StmALogCollectionView(consol);
			var countWithContainer = consolLogs.Count;
			Assert("Sea Cargo Container Logs should appear in Logs", countWithContainer > countWithOceanBill);

			var seaCargoHouse = (BusinessObject)Factory.New<AU.ICusSCAHouse>();
			seaCargoHouse[CusSCAHouseSchema.CA_CB.Name] = seaCargoOceanBill.PK;
			seaCargoHouse.GetLogs().AddNew(Events.MessageStatusChange);
			consolLogs = new StmALogCollectionView(consol);
			var countWithHouse = consolLogs.Count;
			Assert("Sea Cargo House Logs should appear in Logs", countWithHouse > countWithContainer);
		}

		#endregion

		#region TestSettingInvalidDateOnSailingThenChangingTheSailingAndSavingDoesNotBlowUp

		[ExpectNoExceptions]
		public void TestSettingInvalidDateOnSailingThenChangingTheSailingAndSavingDoesNotBlowUp()
		{
			ForwardingConsol consol = (ForwardingConsol)GetNewConsol();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = OverseasPort;
			transport.JW_RL_NKDiscPort = HomePort;
			transport.JW_Vessel = TestVessel1.RV_FK;
			transport.JW_VoyageFlight = "5057";
			transport.JW_ETD = new ZDateTime(2005, 07, 29);
			transport.JW_ETA = ZDateTime.Invalid;

			transport.JW_VoyageFlight = "5030";
			transport.JW_ETA = new ZDateTime(2005, 06, 05);

			Factory.Save();
		}

		#endregion

		#region TestDocumentSupporter

		#region TestGetContactOrganisation

		public void TestColoadContact()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AssertNull("CoLoadContact should be null", consol.DocumentSupporter.GetContactOrganisation("", ContactType.ShippingLine, DocumentDirection.ANY).OrgContact);

			var org1 = Factory.New<OrgHeader>();
			consol.JK_OA_ShippingLineAddress = org1.MainAddress.PK;
			AssertEquals("ColoadContact should be Org1", org1.PK, consol.DocumentSupporter.GetContactOrganisation("", ContactType.ShippingLine, DocumentDirection.ANY).OrgHeader.PK);

			var org2 = Factory.New<OrgHeader>();
			consol.JK_OA_CreditorAddress = org2.MainAddress.PK;
			AssertEquals("ColoadContact should be Org1", org1.PK, consol.DocumentSupporter.GetContactOrganisation("", ContactType.ShippingLine, DocumentDirection.ANY).OrgHeader.PK);

			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_OA_CreditorAddress = org2.MainAddress.PK;
			AssertEquals("ColoadContact should be Org2", org2.PK, consol.DocumentSupporter.GetContactOrganisation("", ContactType.ShippingLine, DocumentDirection.ANY).OrgHeader.PK);
		}

		#endregion

		public void TestLocalTransportContact()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			OrgHeader arrivalTransport = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader departureTransport = Factory.NewWithValidTestData<OrgHeader>();

			consol.JK_OA_ArrivalUnpackCFSTransportAddress = arrivalTransport.MainAddress.PK;
			consol.JK_OA_DeparturePackCFSTransportAddress = departureTransport.MainAddress.PK;

			ZString storedCompany = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
				AssertEquals("Departure cartage local transport", departureTransport.PK, consol.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.DEP).OrgHeader.PK);

				GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.UnitedStates);
				AssertEquals("Arrival cartage local transport", arrivalTransport.PK, consol.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ARV).OrgHeader.PK);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(storedCompany);
			}
		}

		#endregion

		#region TestGridShipmentsIsForwardingCollection

		public void TestGridShipmentsIsForwardingCollection()
		{
			ForwardingConsol consol = (ForwardingConsol)GetNewConsol();
			AssertEquals(typeof(TopLevelForwardingShipmentCollection), consol.GridShipments.GetType());
		}

		#endregion

		#region Notify Transport Fields Changed

		#region TestLoadDiscForImportExportTransportValueChanged

		public void TestLoadDiscForImportExportTransportValueChanged()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadForExportTransportInfo.ValueChanged += new EventHandler(ValueChanged);
			consol.JK_RL_NKDiscForExportTransportInfo.ValueChanged += new EventHandler(ValueChanged);
			consol.JK_RL_NKLoadForImportTransportInfo.ValueChanged += new EventHandler(ValueChanged);
			consol.JK_RL_NKLoadForFirstImportTransportInfo.ValueChanged += new EventHandler(ValueChanged);
			consol.JK_RL_NKDiscForImportTransportInfo.ValueChanged += new EventHandler(ValueChanged);

			AssertEquals("No Events should be fired by attaching", 0, EventHitCount);

			Transport transport = consol.Transports[0];

			transport.JW_RL_NKLoadPort = HomePort;
			AssertEquals("Changing the load port should refresh the values.", 5, EventHitCount);

			EventHitCount = 0;
			transport.JW_RL_NKDiscPort = "GBLON";
			AssertEquals("Changing the discharge port should refresh the values.", 5, EventHitCount);
		}

		void ValueChanged(object sender, EventArgs e)
		{
			EventHitCount++;
		}

		int EventHitCount;

		#endregion

		#region TestLoadDiscForImportExportTransport

		public void TestLoadDiscForImportExportTransport()
		{
			using (RowFactory.SetCachedTables())
			{
				ForwardingConsol consol = Factory.New<ForwardingConsol>();
				Transport transport1 = consol.Transports[0];
				transport1.JW_ATA = new ZDateTime(2009, 1, 1, 3, 4, 5);
				transport1.JW_ETA = new ZDateTime(2009, 1, 2, 3, 4, 5);

				AssertEquals("", consol.JK_RL_NKLoadForExportTransport);
				AssertEquals("", consol.JK_RL_NKDiscForExportTransport);
				AssertEquals("", consol.JK_RL_NKLoadForImportTransport);
				AssertEquals("", consol.JK_RL_NKDiscForImportTransport);
				AssertEquals(ZDateTime.Empty, consol.JK_ATAForImportTransport);
				AssertEquals(ZDateTime.Empty, consol.JK_ETAForImportTransport);

				transport1.JW_RL_NKLoadPort = HomePort;
				AssertEquals(HomePort, consol.JK_RL_NKLoadForExportTransport);
				AssertEquals("", consol.JK_RL_NKDiscForExportTransport);
				AssertEquals("", consol.JK_RL_NKLoadForImportTransport);
				AssertEquals("", consol.JK_RL_NKDiscForImportTransport);
				AssertEquals(ZDateTime.Empty, consol.JK_ATAForImportTransport);
				AssertEquals(ZDateTime.Empty, consol.JK_ETAForImportTransport);

				transport1.JW_RL_NKDiscPort = AlternateHomePort;
				AssertEquals("", consol.JK_RL_NKLoadForExportTransport);
				AssertEquals("", consol.JK_RL_NKDiscForExportTransport);
				AssertEquals("", consol.JK_RL_NKLoadForImportTransport);
				AssertEquals("", consol.JK_RL_NKDiscForImportTransport);
				AssertEquals(ZDateTime.Empty, consol.JK_ATAForImportTransport);
				AssertEquals(ZDateTime.Empty, consol.JK_ETAForImportTransport);

				transport1.JW_RL_NKLoadPort = OverseasPort;
				AssertEquals("", consol.JK_RL_NKLoadForExportTransport);
				AssertEquals("", consol.JK_RL_NKDiscForExportTransport);
				AssertEquals(OverseasPort, consol.JK_RL_NKLoadForImportTransport);
				AssertEquals(AlternateHomePort, consol.JK_RL_NKDiscForImportTransport);
				AssertEquals(new ZDateTime(2009, 1, 1, 3, 4, 5), consol.JK_ATAForImportTransport);
				AssertEquals(new ZDateTime(2009, 1, 2, 3, 4, 5), consol.JK_ETAForImportTransport);

				transport1.JW_RL_NKDiscPort = OverseasPort2;
				AssertEquals("", consol.JK_RL_NKLoadForExportTransport);
				AssertEquals("", consol.JK_RL_NKDiscForExportTransport);
				AssertEquals("", consol.JK_RL_NKLoadForImportTransport);
				AssertEquals("", consol.JK_RL_NKDiscForImportTransport);
				AssertEquals(ZDateTime.Empty, consol.JK_ATAForImportTransport);
				AssertEquals(ZDateTime.Empty, consol.JK_ETAForImportTransport);

				transport1.JW_RL_NKLoadPort = HomePort;
				AssertEquals(HomePort, consol.JK_RL_NKLoadForExportTransport);
				AssertEquals(OverseasPort2, consol.JK_RL_NKDiscForExportTransport);
				AssertEquals("", consol.JK_RL_NKLoadForImportTransport);
				AssertEquals("", consol.JK_RL_NKDiscForImportTransport);
				AssertEquals(ZDateTime.Empty, consol.JK_ATAForImportTransport);
				AssertEquals(ZDateTime.Empty, consol.JK_ETAForImportTransport);

				Transport transport2 = consol.Transports.AddNew();
				transport2.JW_RL_NKLoadPort = OverseasPort3;
				transport2.JW_RL_NKDiscPort = HomePort;
				transport2.JW_ATA = new ZDateTime(2009, 1, 3, 3, 4, 5);
				transport2.JW_ETA = new ZDateTime(2009, 1, 4, 3, 4, 5);

				AssertEquals(HomePort, consol.JK_RL_NKLoadForExportTransport);
				AssertEquals(OverseasPort2, consol.JK_RL_NKDiscForExportTransport);
				AssertEquals(OverseasPort3, consol.JK_RL_NKLoadForImportTransport);
				AssertEquals(HomePort, consol.JK_RL_NKDiscForImportTransport);
				AssertEquals(new ZDateTime(2009, 1, 3, 3, 4, 5), consol.JK_ATAForImportTransport);
				AssertEquals(new ZDateTime(2009, 1, 4, 3, 4, 5), consol.JK_ETAForImportTransport);

				consol.JK_RL_NKLoadPort = OverseasPort;
				transport1.JW_RL_NKLoadPort = OverseasPort2;
				transport1.JW_RL_NKDiscPort = OverseasPort3;
				transport2.JW_RL_NKLoadPort = OverseasPort3;
				transport2.JW_RL_NKDiscPort = HomePort;
				AssertEquals(OverseasPort3, consol.JK_RL_NKLoadForFirstImportTransport);

				transport1.JW_TransportMode = "ROA";
				AssertEquals(OverseasPort3, consol.JK_RL_NKLoadForFirstImportTransport);

				transport2.JW_TransportMode = "ROA";
				AssertEquals(OverseasPort3, consol.JK_RL_NKLoadForFirstImportTransport);

				consol.JK_RL_NKLoadPort = OverseasPort;
				consol.JK_RL_NKDischargePort = HomePort;
				consol.JK_TransportMode = "SEA";
				transport1.JW_RL_NKLoadPort = OverseasPort;
				transport1.JW_RL_NKDiscPort = AlternateHomePort;
				transport1.JW_TransportMode = "ROA";
				transport1.JW_LegOrder = 1;
				transport2.JW_RL_NKLoadPort = AlternateHomePort;
				transport2.JW_RL_NKDiscPort = AlternateHomePort2;
				transport2.JW_TransportMode = "ROA";
				transport2.JW_LegOrder = 2;
				AssertEquals(HomePort, consol.JK_RL_NKDiscForLastImportTransport);
				transport1.JW_TransportMode = "SEA";
				AssertEquals(AlternateHomePort, consol.JK_RL_NKDiscForLastImportTransport);
				transport2.JW_TransportMode = "SEA";
				AssertEquals(AlternateHomePort2, consol.JK_RL_NKDiscForLastImportTransport);
				transport1.JW_Vessel = TestVessel1.RV_FK;
				transport1.JW_VoyageFlight = "V1";
				transport2.JW_Vessel = TestVessel1.RV_FK;
				transport2.JW_VoyageFlight = "V1";
				AssertEquals(AlternateHomePort2, consol.JK_RL_NKDiscForLastImportTransport);
				AssertEquals(new ZDateTime(2009, 1, 3, 3, 4, 5), consol.JK_ArrivalForLastImportTransport);
				AssertEquals("V1", consol.JK_VoyageFlightForLastImportTransport);
				transport2.JW_Vessel = TestVessel2.RV_FK;
				transport2.JW_VoyageFlight = "V2";
				AssertEquals(AlternateHomePort, consol.JK_RL_NKDiscForLastImportTransport);
				AssertEquals(new ZDateTime(2009, 1, 1, 3, 4, 5), consol.JK_ATAForImportTransport);
				AssertEquals(new ZDateTime(2009, 1, 2, 3, 4, 5), consol.JK_ETAForImportTransport);
				AssertEquals(new ZDateTime(2009, 1, 1, 3, 4, 5), consol.JK_ArrivalForLastImportTransport);
				AssertEquals("V1", consol.JK_VoyageFlightForLastImportTransport);
			}
		}

		#endregion

		#endregion

		#region IJobHeaderParent Members

		[ExpectNoExceptions]
		public void TestSavingJobHeader()
		{
			// JobHeader is created first so we're not depending on the order of OnSaving()
			var job = Factory.NewJobForTesting<JobHeader>();
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			job.JH_ParentID = consol.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			job.Parent = consol;
			Factory.Save();
			AssertEquals("JH_JobNum should be populated", consol.JK_UniqueConsignRef, job.JH_JobNum);
		}

		public void TestIJobHeaderParent_AllowInvoiceDeletion()
		{
			IJobHeaderParent forwardingConsol = Factory.New<ForwardingConsol>();
			Assert(forwardingConsol.AllowInvoiceDeletion);
		}

		#endregion

		[ExpectNoExceptions]
		public void TestCountrySpecificJobSupportUnregister_Canada()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Canada))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_AgentsReference = "REFERENCE";
				consol.Delete();

				Factory.Save();
			}
		}

		#region Sendingarnumer

		public void TestSendingarnumer()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Iceland);

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			AssertEquals("should be empty", "", consol.JK_CRN);

			consol.JK_CRN = "A^%%^MA()**E05----087ISR----__EYJ-------1230";
			AssertEquals("should be properly formatted", "A-MAE-0508-7-IS-REY-J123-0", consol.JK_CRN);
		}

		public void TestAutoFillCRN()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "ISREY";
			consol.JK_UniqueConsignRef = "BOBBY";
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			OrgHeader shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_IsShippingProvider = ZBool.True;
			OrgCusCode cusCode = shippingLine.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Iceland;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_CustomsRegNo = "Z";
			cusCode.OK_OH = shippingLine.PK;

			Transport transport = consol.Transports.MostInterestingTransport;

			RefVessel vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			vessel.RV_CarrierCode = "F22";
			transport.JW_Vessel = vessel.RV_FK;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_ATA = new ZDateTime(2008, 12, 4);
			transport.CarrierPK = shippingLine.PK;

			Factory.Save();

			AssertEquals(ZString.Empty, consol.JK_CRN);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Iceland);
			consol.OnLoaded();
			AssertEquals("Z-F22-0412-8-AU-SYD", consol.JK_CRN);
		}

		public void TestAutoFillCRN_WhenCurrentBranchIsNull()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "ISREY";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				var transport = consol.Transports.MostInterestingTransport;
				AssertNoExceptionThrown(() => consol.AutoFillCRN(transport));
			}
		}

		#endregion

		public void TestIsAWBHeaderAccessible_HasTypeZBool()
		{
			AssertType("IsAWBHeaderAccessible should have type ZBool, as it is used in 'Consol --> Documents'", typeof(ZBool), Consol.IsAWBHeaderAccessible);
		}

		public void TestIsAWBHeaderAccessible()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AssertEquals("Should be true when is transport mode is air, agent type is neither courier, nor MAWB", true, Consol.IsAWBHeaderAccessible);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Should be false when transport mode is sea", false, Consol.IsAWBHeaderAccessible);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Consol.JK_AgentType = Core.Constants.AgentType.Courier;
			AssertEquals("Should be false when agent type is courier", false, Consol.IsAWBHeaderAccessible);

			Consol.JK_AgentType = Core.Constants.AgentType.AWBMaster;
			AssertEquals("Should be false when agent type is multi AWB master", false, Consol.IsAWBHeaderAccessible);
		}

		public void TestUpdateOverrideRateSectionWhenOverrideAWBHeader()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Consol.JK_AgentType = Core.Constants.AgentType.Agent;

			Assert("AWBHeader.EH_AreRateLinesOverridden should be false as default", !Consol.AWBHeader.EH_AreRateLinesOverridden);

			Consol.JK_OverrideWaybillDefaults = true;
			Assert("AWBHeader.EH_AreRateLinesOverridden should be updated as true", Consol.AWBHeader.EH_AreRateLinesOverridden);

			Consol.JK_OverrideWaybillDefaults = false;
			Assert("AWBHeader.EH_AreRateLinesOverridden should keep as true", Consol.AWBHeader.EH_AreRateLinesOverridden);
		}

		#region TestNumbers

		public void TestNumbersAndCusEntryNumsIntersection()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Iceland);
			CusEntryNumber additionalNumber = Consol.Numbers.AddNew();
			additionalNumber.CE_EntryType = IcelandAdditionalReferenceNumberTypes.Codes.OriginalCustomsReference;
			additionalNumber.CE_EntryNum = "~~~";
			AssertNotEquals("CRN and additional CEN must be different", Consol.JK_CRN, additionalNumber.CE_EntryNum);
			Consol.JK_CRN = "111";
			AssertNotEquals("CRN and additional CEN must be different", Consol.JK_CRN, additionalNumber.CE_EntryNum);
			AssertEquals("CRN new code", "1-11", Consol.JK_CRN);
			AssertEquals("CusEntryNums holds only CRN", 1, Consol.CusEntryNums.Count);
			Assert("CusEntryNums don't holds number from Numbers", !Consol.CusEntryNums.Contains(additionalNumber.PK));
			Factory.Save();

			ForwardingConsol loadedConsol = NewFactory().Load<ForwardingConsol>(Consol.PK);
			AssertEquals("CRN new code", "1-11", loadedConsol.JK_CRN);
			AssertEquals("CusEntryNums holds only CRN", 1, loadedConsol.CusEntryNums.Count);
			Assert("Numbers holds at least added before number", loadedConsol.Numbers.Contains(additionalNumber.PK));
		}

		public void TestUAEInstalmentNumber()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.UnitedArabEmirates);
			CusEntryNumber num = Consol.Numbers.AddNew();
			num.CE_EntryType = UnitedArabEmiratesAdditionalReferenceNumberTypes.Codes.UAEInstalmentNumber;
			num.CE_EntryNum = "1";
			Factory.Save();

			AssertEquals("1", Consol.UAEInstalmentNumber);
		}

		public void TestCarrierShipperReferenceWithFallback()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C0000005";

			AssertEquals("C0000005", consol.CarrierShipperReferenceWithFallback);

			var entryNum = consol.Numbers.AddNew();
			entryNum.CE_EntryType = ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierShipperReference;
			entryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNum.CE_EntryIsSystemGenerated = true;
			entryNum.CE_EntryNum = "C0000005-V8";

			AssertEquals("C0000005-V8", consol.CarrierShipperReferenceWithFallback);
		}

		#endregion

		#region Clone

		public void TestCloneSetsMAWB()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_IsNeutralMaster = true;
			consol.MasterBillAirlinePrefix = "300";
			consol.JK_MasterBillNum = "300900";

			ForwardingConsol clone = (ForwardingConsol)consol.Clone();
			AssertEquals("300", clone.MasterBillAirlinePrefix);
			AssertEquals("300", clone.JK_MasterBillNum);
			AssertEquals(true, clone.JK_IsNeutralMaster);

			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			consol2.MasterBillAirlinePrefix = "900";

			ForwardingConsol clone2 = (ForwardingConsol)consol2.Clone();
			AssertEquals("900", clone2.MasterBillAirlinePrefix);
			AssertEquals("900", clone2.JK_MasterBillNum);
			AssertEquals(false, clone2.JK_IsNeutralMaster);
		}

		public void TestCloneDoesNotAllocateMAWB()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			JobMawb mawb1 = AddMawb(factory, "081", "00000011", GlbBranch.CurrentBranch, "STD");

			factory.Save();

			ForwardingConsol consol = factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C0001";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_MasterBillNum = "081";
			consol.JK_IsNeutralMaster = true;

			consol.Factory.Save();

			AssertEquals("expected mawb 08100000011 allocated to consol", mawb1.PK, consol.MAWBAllocation.AllocatedMawb.PK);

			ForwardingConsol clone = (ForwardingConsol)consol.Clone();

			AssertEquals("expected clone is pending MAWB allocation", "Pending Allocation...", clone.MasterBillNeutralMAWB);
		}

		JobMawb AddMawb(BusinessObjectFactory factory, string prefix, string mawbNo, GlbBranch branch, string serviceLevel)
		{
			JobMawb mawb = factory.NewWithValidTestData<JobMawb>();
			mawb.JM_Airline3DigitPrefix = prefix;
			mawb.JM_MAWB = mawbNo;
			mawb.JM_GB = branch.PK;
			mawb.JM_ServiceLevel = serviceLevel;

			return mawb;
		}

		public void TestCloneIgnoresOverrideWaybillDefaults()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_OverrideWaybillDefaults = true;

			var result = (ForwardingConsol)consol.Clone();
			AssertEquals("AWB should not be overridden for copied jobs", false, result.JK_OverrideWaybillDefaults);
		}

		public void TestCloneDoesNotCopyCustomsReference()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_CustomsReference = "OCR00000001";

			var result = (ForwardingConsol)consol.Clone();
			AssertEquals("Customs Reference should not be populated from copied job", "", result.JK_CustomsReference);
		}

		public void TestCloneSetsVerificationUnits()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.WeightVerificationUnit = Weight.Decitons;
			consol.VolumeVerificationUnit = Volume.CubicDecimetres;

			var clone = (ForwardingConsol)consol.Clone();
			AssertEquals(Weight.Decitons, clone.WeightVerificationUnit);
			AssertEquals(Volume.CubicDecimetres, clone.VolumeVerificationUnit);
		}

		public void TestCo2IsCopiedForClonedConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.WeightVerificationUnit = Weight.Decitons;
			consol.VolumeVerificationUnit = Volume.CubicDecimetres;
			consol.JK_TotalShipmentActWeightCheck = 10;
			consol.SetCO2ePerTonneInKg(20);
			consol.SetTotalCO2e(200);

			var clone = (ForwardingConsol)consol.Clone();
			AssertEquals(20m, clone.GetCO2ePerTonneInKg());
			AssertEquals(200m, clone.GetTotalCO2e());
			AssertEquals(CO2eStatusList.Codes.Current, clone.GetCO2eStatus());
		}

		#endregion

		#region OnImportExportChanged

		public void TestImportExportChanged()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.ImportExportChangedAfterDischargePortChange += delegate
			{ ImportExportChangedHandlerWasRun = true; };
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUDRW";
			ImportExportChangedHandlerWasRun = false;
			consol.JK_RL_NKDischargePort = "AUMEL";
			Assert(!ImportExportChangedHandlerWasRun);
			consol.JK_RL_NKDischargePort = "USCHI";
			Assert(ImportExportChangedHandlerWasRun);
			ImportExportChangedHandlerWasRun = false;
			consol.JK_RL_NKDischargePort = "USNYC";
			Assert(!ImportExportChangedHandlerWasRun);
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "AUSYD";
			Assert(ImportExportChangedHandlerWasRun);
		}

		bool ImportExportChangedHandlerWasRun;

		#endregion

		#region IDocAddresses

		public void TestSupportedAddressType()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			IDocAddresses addresses = consol;

			AssertEquals(consol.NotifyPartyDocumentaryAddress.DocAddressType, addresses.GetDocAddressRequirement(DocAddressType.NotifyParty).DefaultDocAddressType);
			AssertEquals(consol.NotifyParty2DocumentaryAddress.DocAddressType, addresses.GetDocAddressRequirement(DocAddressType.NotifyParty2).DefaultDocAddressType);
			AssertEquals(consol.NotifyParty3DocumentaryAddress.DocAddressType, addresses.GetDocAddressRequirement(DocAddressType.NotifyParty3).DefaultDocAddressType);
			AssertEquals(consol.MasterBillIssuingPartyDocumentaryAddress.DocAddressType, addresses.GetDocAddressRequirement(DocAddressType.MasterBillIssuingParty).DefaultDocAddressType);
			AssertEquals(consol.CarrierBookingAgentDocumentaryAddress.DocAddressType, addresses.GetDocAddressRequirement(DocAddressType.CarrierBookingAgent).DefaultDocAddressType);
			AssertEquals(consol.CarrierHandlingAgentDocumentaryAddress.DocAddressType, addresses.GetDocAddressRequirement(DocAddressType.CarrierHandlingAgent).DefaultDocAddressType);
			AssertEquals(consol.MasterBillShipperOverrideDocumentaryAddress.DocAddressType, addresses.GetDocAddressRequirement(DocAddressType.MasterBillShipperOverride).DefaultDocAddressType);
			AssertEquals(consol.MasterBillConsigneeOverrideDocumentaryAddress.DocAddressType, addresses.GetDocAddressRequirement(DocAddressType.MasterBillConsigneeOverride).DefaultDocAddressType);

			AssertEquals(consol.CarrierExportCreditorAddress.DocAddressType, addresses.GetDocAddressRequirement(DocAddressType.CarrierExportCreditor).DefaultDocAddressType);
			AssertEquals(consol.CarrierImportCreditorAddress.DocAddressType, addresses.GetDocAddressRequirement(DocAddressType.CarrierImportCreditor).DefaultDocAddressType);

			AssertEquals(consol.FreightPayerDocumentaryAddress.DocAddressType, addresses.GetDocAddressRequirement(DocAddressType.FreightPayer).DefaultDocAddressType);

			Assert(!consol.IsDirect);
			var nonDirectConsoleAddressTypes = addresses.SupportedAddressTypes;

			AssertCollectionContains("should support notify party", DocAddressType.NotifyParty, nonDirectConsoleAddressTypes);
			AssertCollectionContains("should support notify party 2", DocAddressType.NotifyParty2, nonDirectConsoleAddressTypes);
			AssertCollectionContains("should support notify party 3", DocAddressType.NotifyParty3, nonDirectConsoleAddressTypes);
			AssertCollectionContains("should support master bill issuing party", DocAddressType.MasterBillIssuingParty, nonDirectConsoleAddressTypes);
			AssertCollectionContains("should support carrier booking agent", DocAddressType.CarrierBookingAgent, nonDirectConsoleAddressTypes);
			AssertCollectionContains("should support carrier handling agent", DocAddressType.CarrierHandlingAgent, nonDirectConsoleAddressTypes);
			AssertCollectionContains("should support master bill shipper override", DocAddressType.MasterBillShipperOverride, nonDirectConsoleAddressTypes);
			AssertCollectionContains("should support master bill consignee override", DocAddressType.MasterBillConsigneeOverride, nonDirectConsoleAddressTypes);

			consol.JK_AgentType = Constants.AgentType.Direct;
			var directConsoleAddressTypes = addresses.SupportedAddressTypes;

			AssertCollectionNotContains("direct consol should not support master bill shipper override", DocAddressType.MasterBillShipperOverride, directConsoleAddressTypes);
			AssertCollectionNotContains("direct consol should not support master bill consignee override", DocAddressType.MasterBillConsigneeOverride, directConsoleAddressTypes);
		}

		public void TestCarrierExportCreditorAddress()
		{
			var consol = Factory.New<ForwardingConsol>();
			AssertEquals(false, consol.CarrierExportCreditorAddress.IsValidAddress);
			AssertEquals(null, consol.CarrierExportCreditor);

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "MISC";
			consol.CarrierExportCreditorAddress.OrganisationPK = org.PK;
			AssertEquals(false, consol.CarrierExportCreditorAddress.HasRealOrganisation);
			AssertEquals(null, consol.CarrierExportCreditor);

			org.OH_Code = "aaa";
			org.OH_FullName = "bbb";
			AssertEquals(true, consol.CarrierExportCreditorAddress.HasRealOrganisation);
			AssertEquals(org, consol.CarrierExportCreditor);

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "ccc";

			consol.CarrierExportCreditorAddress.ContactPK = contact.PK;
			AssertEquals(org.PK, consol.CarrierExportCreditorAddress.OrganisationPK);
			AssertEquals(contact.PK, consol.CarrierExportCreditorAddress.ContactPK);
		}

		public void TestCarrierExportCreditorAddress_WhenConsolIsExportAndSettingExportCreditorAddress_UpdatesCreditor()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;

			AssertEquals("Preconditions: export consol expected.", true, consol.IsExport());

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsCreditor = false;

			var orgImport = Factory.NewWithValidTestData<OrgHeader>();
			consol.CarrierImportCreditorAddress.E2_OA_Address = orgImport.MainAddress.PK;

			consol.CarrierExportCreditorAddress.E2_OA_Address = org.MainAddress.PK;
			consol.CarrierExportCreditorAddress.Validation.ValidateOrganisationPK();

			AssertEquals(true, consol.CarrierExportCreditorAddress.HasRealOrganisation);
			AssertEquals(org, consol.CarrierExportCreditor);
			//check WI00532393 - Remove Valiadtion for Carrier Import/ Export addresses to see why we decided to remove validations for Carrier Export Creditor Address
			AssertNoErrors("We should not validate Carrier Export Creditor Address", consol.CarrierExportCreditorAddress.OrganisationPKInfo);
			AssertEquals("Should not update creditor if export creditor address is invalid", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			org.OH_IsCreditor = true;

			consol.CarrierExportCreditorAddress.E2_OA_Address = ZGuid.Empty;
			consol.CarrierExportCreditorAddress.E2_OA_Address = org.MainAddress.PK;

			Factory.Save();

			consol.CarrierExportCreditorAddress.Validation.ValidateOrganisationPK();

			AssertNoErrors(consol.CarrierExportCreditorAddress.OrganisationPKInfo);
			AssertEquals("Should update creditor if export creditor address is valid and consol is export", consol.CarrierExportCreditorAddress.E2_OA_Address, consol.JK_OA_CreditorAddress);

			consol.CarrierExportCreditorAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals("Should clear creditor if we remove export creditor address and consol is export", ZGuid.Empty, consol.JK_OA_CreditorAddress);
			AssertEquals("Import creditor address should not change if consol is export", orgImport.MainAddress.PK, consol.CarrierImportCreditorAddress.E2_OA_Address);
		}

		public void TestCarrierExportCreditorAddress_WhenConsolIsColoadExportAndSettingExportCreditorAddress_ShouldNotUpdateColoadWith()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_AgentType = AgentType.CoLoad;

			AssertEquals("Preconditions: export consol expected.", true, consol.IsExport());

			var coloadOrg = Factory.NewWithValidTestData<OrgHeader>();
			coloadOrg.OH_IsCreditor = true;

			var orgAddress = Factory.NewWithValidTestData<OrgHeader>();
			orgAddress.OH_IsCreditor = true;

			Factory.Save();

			consol.JK_OA_CreditorAddress = coloadOrg.MainAddress.PK;
			consol.CarrierExportCreditorAddress.E2_OA_Address = orgAddress.MainAddress.PK;

			AssertEquals("Should NOT change coload with if we update export address and consol is export", coloadOrg.MainAddress.PK, consol.JK_OA_CreditorAddress);

			consol.CarrierExportCreditorAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals("Should NOT clear coload with if we remove export creditor address and consol is export", coloadOrg.MainAddress.PK, consol.JK_OA_CreditorAddress);
		}

		public void TestDefaultCreditorExportAddressFromColoadRelatedParty_WhenCosolIsColoadExport()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKA";
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_AgentType = AgentType.CoLoad;

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "NZABY";

			AssertEquals("Preconditions: Export Consol expected.", true, consol.IsExport());

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			relatedParty.OH_IsCreditor = true;

			var coloadWithOrg = Factory.NewWithValidTestData<OrgHeader>();
			coloadWithOrg.SetRelatedParty(relatedParty
				, RelatedPartyTypeList.Codes.ServiceProviderCreditor
				, RelatedPartyDirectionList.Codes.PickupAndDelivery
				, consol.JK_TransportMode
				, consol.JK_ConsolMode
				, transport.JW_RL_NKLoadPort);

			Factory.Save();

			var parties = coloadWithOrg.AllRelatedParties;
			var partyRecord = parties.First() as OrgRelatedParty;

			AssertEquals("Preconditions: partyRecord is ENT", "ENT", partyRecord.CompanyLevel);

			consol.JK_OA_CreditorAddress = coloadWithOrg.MainAddress.PK;

			var relatedParty1 = Factory.NewWithValidTestData<OrgHeader>();
			relatedParty1.OH_IsCreditor = true;

			var partyRecord1 = parties.AddNew();
			partyRecord1.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProviderCreditor;
			partyRecord1.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			partyRecord1.PR_OH_RelatedParty = relatedParty1.PK;
			partyRecord1.PR_FreightTransportMode = consol.JK_TransportMode;
			partyRecord1.PR_FreightContainerMode = consol.JK_ConsolMode;
			partyRecord1.PR_Location = transport.JW_RL_NKLoadPort;

			Factory.Save();

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = coloadWithOrg.MainAddress.PK;

			AssertEquals("Pickup direction overtakes Pickup And Delivery when Transport/Container/Location/Company are the same", relatedParty1.MainAddress.PK, consol.CarrierExportCreditorAddress.E2_OA_Address);

			partyRecord1.PR_FreightTransportMode = TransportModes.All;

			AssertEquals("Preconditions: partyRecord is SEA", "SEA", partyRecord.PR_FreightTransportMode);
			AssertEquals("Preconditions: partyRecord1 is ALL", "ALL", partyRecord1.PR_FreightTransportMode);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = coloadWithOrg.MainAddress.PK;

			AssertEquals("Transport SEA  overtakes ALL", relatedParty.MainAddress.PK, consol.CarrierExportCreditorAddress.E2_OA_Address);

			partyRecord1.PR_FreightTransportMode = consol.JK_TransportMode;

			partyRecord.PR_FreightContainerMode = "";
			partyRecord1.PR_FreightContainerMode = consol.JK_ConsolMode;

			AssertEquals("Preconditions: partyRecord Container mode is balnk", "", partyRecord.PR_FreightContainerMode);
			AssertEquals("Preconditions: partyRecord1 Container mode is FCL", "FCL", partyRecord1.PR_FreightContainerMode);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = coloadWithOrg.MainAddress.PK;

			AssertEquals("Container mode FCL overtakes blank", relatedParty1.MainAddress.PK, consol.CarrierExportCreditorAddress.E2_OA_Address);

			partyRecord.PR_FreightContainerMode = consol.JK_ConsolMode;
			partyRecord1.PR_Location = "";

			AssertEquals("Preconditions: partyRecord Location is NZABY", "NZABY", partyRecord.PR_Location);
			AssertEquals("Preconditions: partyRecord1 Location is blank", "", partyRecord1.PR_Location);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = coloadWithOrg.MainAddress.PK;

			AssertEquals("Location Port overtakes blank", relatedParty.MainAddress.PK, consol.CarrierExportCreditorAddress.E2_OA_Address);

			partyRecord1.PR_Location = "NZ";

			AssertEquals("Preconditions: partyRecord Location is NZABY", "NZABY", partyRecord.PR_Location);
			AssertEquals("Preconditions: partyRecord1 Location is NZ", "NZ", partyRecord1.PR_Location);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = coloadWithOrg.MainAddress.PK;

			AssertEquals("Location Port overtakes Country", relatedParty.MainAddress.PK, consol.CarrierExportCreditorAddress.E2_OA_Address);

			partyRecord.PR_Location = "";

			AssertEquals("Preconditions: partyRecord Location is blank", "", partyRecord.PR_Location);
			AssertEquals("Preconditions: partyRecord1 Location is NZ", "NZ", partyRecord1.PR_Location);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = coloadWithOrg.MainAddress.PK;

			AssertEquals("Location Country overtakes blank", relatedParty1.MainAddress.PK, consol.CarrierExportCreditorAddress.E2_OA_Address);

			partyRecord.CompanyLevel = "COM";
			partyRecord1.CompanyLevel = "ENT";

			AssertEquals("Preconditions: partyRecord Company Level is COM", "COM", partyRecord.CompanyLevel);
			AssertEquals("Preconditions: partyRecord1 Company Level is ENT", "ENT", partyRecord1.CompanyLevel);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = coloadWithOrg.MainAddress.PK;

			AssertEquals("Company level overtakes ENT", relatedParty.MainAddress.PK, consol.CarrierExportCreditorAddress.E2_OA_Address);

			partyRecord.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
			partyRecord1.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
			coloadWithOrg.OH_IsCreditor = false; // No fall back to coload with

			Factory.Save();

			AssertEquals("Preconditions: partyRecord Direction is Delivery", "DLV", partyRecord.PR_FreightDirection);
			AssertEquals("Preconditions: partyRecord1 Direction is Delivery", "DLV", partyRecord1.PR_FreightDirection);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = coloadWithOrg.MainAddress.PK;

			AssertEquals("For Export consol, should igonre Delivery parties ", ZGuid.Empty, consol.CarrierExportCreditorAddress.E2_OA_Address);
		}

		public void TestDefaultCreditorExportAddressUpdatedFromPayableColoadWithWhenReceivingAgentIsPayableAndConsolIsColoadCollect()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;
			consol.JK_PrepaidCollect = PaymentType.Collect;
			consol.JK_AgentType = AgentType.CoLoad;

			AssertEquals("Preconditions: Export Consol expected.", true, consol.IsExport());

			var coloadWithOrg = Factory.NewWithValidTestData<OrgHeader>();
			coloadWithOrg.OH_IsCreditor = false;

			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			receivingForwarder.OH_IsCreditor = false; // Not Payable

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			consol.JK_OA_CreditorAddress = coloadWithOrg.MainAddress.PK;

			AssertEquals("Receiving and coload with are not payable ", ZGuid.Empty, consol.CarrierExportCreditorAddress.E2_OA_Address);

			coloadWithOrg.OH_IsCreditor = true;
			Factory.Save();

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = coloadWithOrg.MainAddress.PK;

			AssertEquals("Receiving is not payable, we set export address to payable coload with", coloadWithOrg.MainAddress.PK, consol.CarrierExportCreditorAddress.E2_OA_Address);

			receivingForwarder.OH_IsCreditor = true; // Payable

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = coloadWithOrg.MainAddress.PK;

			AssertEquals("Even receiving is payable, we set export address to payable coload with", coloadWithOrg.MainAddress.PK, consol.CarrierExportCreditorAddress.E2_OA_Address);
		}

		public void TestDefaultCreditorExportAddressUpdatedFromColoadWithIfItIsPayableAndConsolIsColoadPrepiad()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_AgentType = AgentType.CoLoad;

			AssertEquals("Preconditions: Export Consol expected.", true, consol.IsExport());

			var coloadWithOrg = Factory.NewWithValidTestData<OrgHeader>();
			coloadWithOrg.OH_IsCreditor = false; // Not Payable

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = coloadWithOrg.MainAddress.PK;

			AssertEquals("coload with is not payable", ZGuid.Empty, consol.CarrierExportCreditorAddress.E2_OA_Address);

			coloadWithOrg.OH_IsCreditor = true; // Payable
			Factory.Save();

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = coloadWithOrg.MainAddress.PK;

			AssertEquals("We set export address to payable coload with", coloadWithOrg.MainAddress.PK, consol.CarrierExportCreditorAddress.E2_OA_Address);
		}

		public void TestCarrierExportCreditorAddress_WhenConsolIsExportAndSettingCreditor_UpdatesExportCreditorAddress()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			AssertEquals("Preconditions: export consol expected.", true, consol.IsExport());

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsCreditor = false;

			var orgImport = Factory.NewWithValidTestData<OrgHeader>();
			orgImport.OH_IsCreditor = false;
			consol.CarrierImportCreditorAddress.E2_OA_Address = orgImport.MainAddress.PK;

			consol.JK_OA_CreditorAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(org.PK);
			consol.JK_OA_CreditorAddress = ZGuid.Empty;

			CombineAssertions("Creditor doesn't have valid address, we shouldn't set export creditor address", () =>
			{
				AssertEquals(false, consol.CarrierExportCreditorAddress.IsValidAddress);
				AssertEquals(null, consol.CarrierExportCreditor);
			});

			org.OH_IsCreditor = true;
			Factory.Save();

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = org.MainAddress.PK;

			CombineAssertions("Creditor has valid address, we should set export creditor address", () =>
			{
				AssertEquals(true, consol.CarrierExportCreditorAddress.HasRealOrganisation);
				AssertEquals(org, consol.CarrierExportCreditor);
			});

			consol.JK_OA_CreditorAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.Empty);
			consol.JK_OA_CreditorAddress = ZGuid.Empty;

			AssertEquals("Should clear export creditor address if we remove creditor and consol is export", ZGuid.Empty, consol.CarrierExportCreditorAddress.E2_OA_Address);
			AssertEquals("Should clear export creditor organisation if we remove creditor and consol is export", ZGuid.Empty, consol.CarrierExportCreditorAddress.OrganisationPK);
			AssertEquals("Import creditor address should not change if consol is export", orgImport.MainAddress.PK, consol.CarrierImportCreditorAddress.E2_OA_Address);
		}

		public void TestCarrierImportCreditorAddress()
		{
			var consol = Factory.New<ForwardingConsol>();
			AssertEquals(false, consol.CarrierImportCreditorAddress.IsValidAddress);
			AssertEquals(null, consol.CarrierImportCreditor);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "MISC";
			consol.CarrierImportCreditorAddress.OrganisationPK = org.PK;
			AssertEquals(false, consol.CarrierImportCreditorAddress.HasRealOrganisation);
			AssertEquals(null, consol.CarrierImportCreditor);

			org.OH_Code = "aaa";
			org.OH_FullName = "bbb";
			AssertEquals(true, consol.CarrierImportCreditorAddress.HasRealOrganisation);
			AssertEquals(org, consol.CarrierImportCreditor);

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "ccc";

			consol.CarrierImportCreditorAddress.ContactPK = contact.PK;
			AssertEquals(org.PK, consol.CarrierImportCreditorAddress.OrganisationPK);
			AssertEquals(contact.PK, consol.CarrierImportCreditorAddress.ContactPK);
		}

		public void TestCarrierImportCreditorAddress_WhenConsolIsImportAndSettingImportCreditorAddress_UpdatesCreditor()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			consol.JK_RL_NKLoadPort = OverseasPort;
			consol.JK_RL_NKDischargePort = HomePort;
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;

			AssertEquals("Preconditions: import consol expected.", true, consol.IsImport());

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsCreditor = false;

			var orgExport = Factory.NewWithValidTestData<OrgHeader>();
			orgExport.OH_IsCreditor = true;
			consol.CarrierExportCreditorAddress.E2_OA_Address = orgExport.MainAddress.PK;
			consol.CarrierImportCreditorAddress.E2_OA_Address = org.MainAddress.PK;

			Factory.Save();

			consol.CarrierImportCreditorAddress.Validation.ValidateOrganisationPK();

			AssertEquals(true, consol.CarrierImportCreditorAddress.HasRealOrganisation);
			AssertEquals(org, consol.CarrierImportCreditor);
			//check WI00532393 - Remove Validation for Carrier Import/ Export addresses to see why we decided to remove validations for Carrier Export Creditor Address
			AssertNoErrors("We should not validate Carrier Import Creditor Address", consol.CarrierImportCreditorAddress.OrganisationPKInfo);
			AssertEquals("Should not update creditor if import creditor address is invalid", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			org.OH_IsCreditor = true;

			consol.CarrierImportCreditorAddress.E2_OA_Address = ZGuid.Empty;
			consol.CarrierImportCreditorAddress.E2_OA_Address = org.MainAddress.PK;

			Factory.Save();

			consol.CarrierImportCreditorAddress.Validation.ValidateOrganisationPK();

			AssertNoErrors(consol.CarrierImportCreditorAddress.OrganisationPKInfo);
			AssertEquals("Should update creditor if import creditor address is valid and consol is import", consol.CarrierImportCreditorAddress.E2_OA_Address, consol.JK_OA_CreditorAddress);

			consol.CarrierImportCreditorAddress.E2_OA_Address = ZGuid.Empty;

			AssertEquals("Should clear creditor if we remove import creditor address and consol is import", ZGuid.Empty, consol.JK_OA_CreditorAddress);
			AssertEquals("Export creditor address should not change if consol is import", orgExport.MainAddress.PK, consol.CarrierExportCreditorAddress.E2_OA_Address);
		}

		public void TestCarrierImportCreditorAddress_WhenConsolIsColoadImportAndSettingImportCreditorAddress_ShouldNotUpdateColoadWith()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			consol.JK_RL_NKLoadPort = OverseasPort;
			consol.JK_RL_NKDischargePort = HomePort;
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_AgentType = AgentType.CoLoad;

			AssertEquals("Preconditions: Import consol expected.", true, consol.IsImport());

			var coloadOrg = Factory.NewWithValidTestData<OrgHeader>();
			coloadOrg.OH_IsCreditor = true;

			var orgAddress = Factory.NewWithValidTestData<OrgHeader>();
			orgAddress.OH_IsCreditor = true;

			Factory.Save();

			consol.JK_OA_CreditorAddress = coloadOrg.MainAddress.PK;
			consol.CarrierImportCreditorAddress.E2_OA_Address = orgAddress.MainAddress.PK;

			AssertEquals("Should NOT change coload with if we update import address and consol is import", coloadOrg.MainAddress.PK, consol.JK_OA_CreditorAddress);

			consol.CarrierImportCreditorAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals("Should NOT clear coload with if we remove import creditor address and consol is import", coloadOrg.MainAddress.PK, consol.JK_OA_CreditorAddress);
		}

		public void TestDefaultCreditorImportAddressFromColoadRelatedParty_WhenCosolIsColoadImport()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "NZAKA";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_AgentType = AgentType.CoLoad;

			var transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = "AUMEL";
			AssertEquals("Preconditions: Import Consol expected.", true, consol.IsImport());

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			relatedParty.OH_IsCreditor = true;

			var coloadWithOrg = Factory.NewWithValidTestData<OrgHeader>();
			coloadWithOrg.SetRelatedParty(relatedParty
				, RelatedPartyTypeList.Codes.ServiceProviderCreditor
				, RelatedPartyDirectionList.Codes.PickupAndDelivery
				, consol.JK_TransportMode
				, consol.JK_ConsolMode
				, transport.JW_RL_NKDiscPort);

			Factory.Save();

			var parties = coloadWithOrg.AllRelatedParties;
			var partyRecord = parties.First() as OrgRelatedParty;

			AssertEquals("Preconditions: partyRecord is ENT", "ENT", partyRecord.CompanyLevel);

			consol.JK_OA_CreditorAddress = coloadWithOrg.MainAddress.PK;

			var relatedParty1 = Factory.NewWithValidTestData<OrgHeader>();
			relatedParty1.OH_IsCreditor = true;

			var partyRecord1 = parties.AddNew();
			partyRecord1.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProviderCreditor;
			partyRecord1.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
			partyRecord1.PR_OH_RelatedParty = relatedParty1.PK;
			partyRecord1.PR_FreightTransportMode = consol.JK_TransportMode;
			partyRecord1.PR_FreightContainerMode = consol.JK_ConsolMode;
			partyRecord1.PR_Location = transport.JW_RL_NKDiscPort;

			Factory.Save();

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = coloadWithOrg.MainAddress.PK;

			AssertEquals("Delivery direction overtakes Pickup And Delivery when Transport/Container/Location/Company are the same", relatedParty1.MainAddress.PK, consol.CarrierImportCreditorAddress.E2_OA_Address);

			partyRecord1.PR_FreightTransportMode = TransportModes.All;

			AssertEquals("Preconditions: partyRecord is SEA", "SEA", partyRecord.PR_FreightTransportMode);
			AssertEquals("Preconditions: partyRecord1 is ALL", "ALL", partyRecord1.PR_FreightTransportMode);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = coloadWithOrg.MainAddress.PK;

			AssertEquals("Transport SEA  overtakes ALL", relatedParty.MainAddress.PK, consol.CarrierImportCreditorAddress.E2_OA_Address);

			partyRecord1.PR_FreightTransportMode = consol.JK_TransportMode;

			partyRecord.PR_FreightContainerMode = "";
			partyRecord1.PR_FreightContainerMode = consol.JK_ConsolMode;

			AssertEquals("Preconditions: partyRecord Container mode is balnk", "", partyRecord.PR_FreightContainerMode);
			AssertEquals("Preconditions: partyRecord1 Container mode is FCL", "FCL", partyRecord1.PR_FreightContainerMode);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = coloadWithOrg.MainAddress.PK;

			AssertEquals("Container mode FCL overtakes blank", relatedParty1.MainAddress.PK, consol.CarrierImportCreditorAddress.E2_OA_Address);

			partyRecord.PR_FreightContainerMode = consol.JK_ConsolMode;
			partyRecord1.PR_Location = "";

			AssertEquals("Preconditions: partyRecord Location is AUMEL", "AUMEL", partyRecord.PR_Location);
			AssertEquals("Preconditions: partyRecord1 Location is blank", "", partyRecord1.PR_Location);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = coloadWithOrg.MainAddress.PK;

			AssertEquals("Location Port overtakes blank", relatedParty.MainAddress.PK, consol.CarrierImportCreditorAddress.E2_OA_Address);

			partyRecord1.PR_Location = "AU";

			AssertEquals("Preconditions: partyRecord Location is AUMEL", "AUMEL", partyRecord.PR_Location);
			AssertEquals("Preconditions: partyRecord1 Location is AU", "AU", partyRecord1.PR_Location);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = coloadWithOrg.MainAddress.PK;

			AssertEquals("Location Port overtakes Country", relatedParty.MainAddress.PK, consol.CarrierImportCreditorAddress.E2_OA_Address);

			partyRecord.PR_Location = "";

			AssertEquals("Preconditions: partyRecord Location is blank", "", partyRecord.PR_Location);
			AssertEquals("Preconditions: partyRecord1 Location is AU", "AU", partyRecord1.PR_Location);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = coloadWithOrg.MainAddress.PK;

			AssertEquals("Location Country overtakes blank", relatedParty1.MainAddress.PK, consol.CarrierImportCreditorAddress.E2_OA_Address);

			partyRecord.CompanyLevel = "COM";
			partyRecord1.CompanyLevel = "ENT";

			AssertEquals("Preconditions: partyRecord Company Level is COM", "COM", partyRecord.CompanyLevel);
			AssertEquals("Preconditions: partyRecord1 Company Level is ENT", "ENT", partyRecord1.CompanyLevel);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = coloadWithOrg.MainAddress.PK;

			AssertEquals("Company level overtakes ENT", relatedParty.MainAddress.PK, consol.CarrierImportCreditorAddress.E2_OA_Address);

			partyRecord.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			partyRecord1.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			coloadWithOrg.OH_IsCreditor = false; // No fall back to coload with

			Factory.Save();

			AssertEquals("Preconditions: partyRecord Direction is Pickup", "PIC", partyRecord.PR_FreightDirection);
			AssertEquals("Preconditions: partyRecord1 Direction is Pickup", "PIC", partyRecord1.PR_FreightDirection);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = coloadWithOrg.MainAddress.PK;

			AssertEquals("For Import consol, should igonre Pickup parties ", ZGuid.Empty, consol.CarrierImportCreditorAddress.E2_OA_Address);
		}

		public void TestDefaultCreditorImportAddressUpdatedFromPayableColoadWithWhenSendingAgenIsPayableAndConsolIsColoadPrepaid()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = OverseasPort;
			consol.JK_RL_NKDischargePort = HomePort;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_AgentType = AgentType.CoLoad;

			AssertEquals("Preconditions: Import Consol expected.", true, consol.IsImport());

			var coloadWithOrg = Factory.NewWithValidTestData<OrgHeader>();
			coloadWithOrg.OH_IsCreditor = false;

			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			sendingForwarder.OH_IsCreditor = false; // Not Payable
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = coloadWithOrg.MainAddress.PK;
			AssertEquals("Sending and coload with are not payable ", ZGuid.Empty, consol.CarrierImportCreditorAddress.E2_OA_Address);

			coloadWithOrg.OH_IsCreditor = true;
			Factory.Save();

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = coloadWithOrg.MainAddress.PK;

			AssertEquals("Sending is not payable, we set import address to payable coload with", coloadWithOrg.MainAddress.PK, consol.CarrierImportCreditorAddress.E2_OA_Address);

			sendingForwarder.OH_IsCreditor = true; // Payable

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = coloadWithOrg.MainAddress.PK;

			AssertEquals("Even sending is payable, we set import address to payable coload with", coloadWithOrg.MainAddress.PK, consol.CarrierImportCreditorAddress.E2_OA_Address);
		}

		public void TestDefaultCreditorImportAddressUpdatedFromColoadWithIfItIsPayableAndConsolIsCollect()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = OverseasPort;
			consol.JK_RL_NKDischargePort = HomePort;
			consol.JK_PrepaidCollect = PaymentType.Collect;
			consol.JK_AgentType = AgentType.CoLoad;

			AssertEquals("Preconditions: Import Consol expected.", true, consol.IsImport());

			var coloadWithOrg = Factory.NewWithValidTestData<OrgHeader>();
			coloadWithOrg.OH_IsCreditor = false; // Not Payable

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = coloadWithOrg.MainAddress.PK;

			AssertEquals("coload with is not payable", ZGuid.Empty, consol.CarrierImportCreditorAddress.E2_OA_Address);

			coloadWithOrg.OH_IsCreditor = true; // Payable
			Factory.Save();

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = coloadWithOrg.MainAddress.PK;

			AssertEquals("We set import address to payable coload with", coloadWithOrg.MainAddress.PK, consol.CarrierImportCreditorAddress.E2_OA_Address);
		}

		public void TestCarrierImportCreditorAddress_WhenConsolIsImportAndSettingCreditor_UpdatesImportCreditorAddress()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			consol.JK_RL_NKLoadPort = "NZAKA";
			consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.HomePort.Code;
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			AssertEquals("Preconditions: import consol expected.", true, consol.IsImport());

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsCreditor = false;

			var orgExport = Factory.NewWithValidTestData<OrgHeader>();
			orgExport.OH_IsCreditor = false;
			consol.CarrierExportCreditorAddress.E2_OA_Address = orgExport.MainAddress.PK;

			consol.JK_OA_CreditorAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(org.PK);
			consol.JK_OA_CreditorAddress = ZGuid.Empty;

			CombineAssertions("Creditor doesn't have valid address, we shouldn't set import creditor address", () =>
			{
				AssertEquals(false, consol.CarrierImportCreditorAddress.IsValidAddress);
				AssertEquals(null, consol.CarrierImportCreditor);
			});

			org.OH_IsCreditor = true;

			Factory.Save();

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = org.MainAddress.PK;

			CombineAssertions("Creditor has valid address, we should set import creditor address", () =>
			{
				AssertEquals(true, consol.CarrierImportCreditorAddress.HasRealOrganisation);
				AssertEquals(org, consol.CarrierImportCreditor);
			});

			consol.JK_OA_CreditorAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.Empty);
			consol.JK_OA_CreditorAddress = ZGuid.Empty;

			AssertEquals("Should clear import creditor address if we remove creditor and consol is import", ZGuid.Empty, consol.CarrierImportCreditorAddress.E2_OA_Address);
			AssertEquals("Should clear import creditor organisation if we remove creditor and consol is import", ZGuid.Empty, consol.CarrierImportCreditorAddress.OrganisationPK);
			AssertEquals("Export creditor address should not change if consol is import", orgExport.MainAddress.PK, consol.CarrierExportCreditorAddress.E2_OA_Address);
		}

		public void TestCarrierExportCreditorAddress_WhenNonCLDDomesticConsolAndSettingExportCreditorAddress_UpdatesConsolCreditor()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = AlternateHomePort;
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_AgentType = AgentType.Agent;

			AssertEquals("Preconditions: domestic consol expected.", true, consol.IsDomestic());

			var exportOrg = Factory.NewWithValidTestData<OrgHeader>();
			exportOrg.OH_IsCreditor = false;
			consol.CarrierExportCreditorAddress.E2_OA_Address = exportOrg.MainAddress.PK;

			AssertEquals(true, consol.CarrierExportCreditorAddress.HasRealOrganisation);
			AssertEquals(exportOrg, consol.CarrierExportCreditor);
			AssertNoErrors("We should not validate Carrier Export Creditor Address", consol.CarrierExportCreditorAddress.OrganisationPKInfo);
			AssertEquals("Should not update creditor if export creditor address is invalid", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			exportOrg.OH_IsCreditor = true;
			consol.CarrierExportCreditorAddress.E2_OA_Address = ZGuid.Empty;
			consol.CarrierExportCreditorAddress.E2_OA_Address = exportOrg.MainAddress.PK;
			Factory.Save();

			AssertEquals("Should update creditor if Carrier Export Ceditor Address is valid and consol is non-CLD domestic ", consol.CarrierExportCreditorAddress.E2_OA_Address, consol.JK_OA_CreditorAddress);

			consol.CarrierExportCreditorAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals("Should clear creditor if we remove Carrier Export Ceditor Address and consol is non-CLD domestic", ZGuid.Empty, consol.JK_OA_CreditorAddress);
		}

		public void TestCreditorAddress_WhenNonCLDDomesticConsolAndSettingCreditor_UpdatesExportCreditorAddress()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = AlternateHomePort;
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			AssertEquals("Preconditions: domestic consol expected.", true, consol.IsDomestic());

			var exportOrg = Factory.NewWithValidTestData<OrgHeader>();
			exportOrg.OH_IsCreditor = false;

			consol.JK_OA_CreditorAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(exportOrg.PK);
			consol.JK_OA_CreditorAddress = ZGuid.Empty;

			CombineAssertions("Creditor doesn't have valid address, we shouldn't set export creditor address", () =>
			{
				AssertEquals(false, consol.CarrierExportCreditorAddress.IsValidAddress);
				AssertEquals(null, consol.CarrierExportCreditor);
			});

			exportOrg.OH_IsCreditor = true;
			Factory.Save();

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = exportOrg.MainAddress.PK;

			CombineAssertions("Creditor has valid address, we should set export creditor address", () =>
			{
				AssertEquals(true, consol.CarrierExportCreditorAddress.HasRealOrganisation);
				AssertEquals(exportOrg, consol.CarrierExportCreditor);
			});

			consol.JK_OA_CreditorAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.Empty);
			consol.JK_OA_CreditorAddress = ZGuid.Empty;

			AssertEquals("Should clear export creditor address if we remove creditor for non-CLD domestic consol", ZGuid.Empty, consol.CarrierExportCreditorAddress.E2_OA_Address);
			AssertEquals("Should clear export creditor organisation if we remove creditor for non-CLD domestic consol", ZGuid.Empty, consol.CarrierExportCreditorAddress.OrganisationPK);
		}

		public void TestCarrierExportCreditorAddress_WhenCLDDomesticConsolAndSettingColoadWith_UpdatesFromColoadWithRelatedParty()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = AlternateHomePort;
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_AgentType = AgentType.CoLoad;

			AssertEquals("Preconditions: Domestic Consol expected.", true, consol.IsDomestic());
			AssertEquals("Preconditions: CLD mode.", true, consol.IsCoLoad);

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var coloadOrg = Factory.NewWithValidTestData<OrgHeader>();
			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();

			consol.JK_OA_CreditorAddress = ZGuid.Empty; // Clear coload
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK; //Set carrier
			Factory.Save();

			AssertEquals("CLD domestic Consol will not set Co-Load With from Carrier", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			coloadOrg.OH_IsCreditor = true;
			consol.JK_OA_CreditorAddress = coloadOrg.MainAddress.PK;
			AssertEquals("CLD domestic consol will set Carrier Export Creditor to Co-Load With if cannot find related party from Co-Load With"
				, consol.JK_OA_CreditorAddress, consol.CarrierExportCreditor.MainAddress.PK);

			coloadOrg.SetRelatedParty(relatedParty
				, RelatedPartyTypeList.Codes.ServiceProviderCreditor
				, RelatedPartyDirectionList.Codes.PickupAndDelivery
				, consol.JK_TransportMode
				, consol.JK_ConsolMode);

			relatedParty.OH_IsCreditor = true;
			Factory.Save();

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = coloadOrg.MainAddress.PK;

			AssertEquals("CLD domestic consol will set Carrier Export Creditor from Co-Load With related party"
				, relatedParty.MainAddress.PK, consol.CarrierExportCreditor.MainAddress.PK);
		}

		public void TestColoadWith_WhenCLDDomesticConsolAndSettingCarrierExportCreditorAddress_ShouldNotUpdateColoadWith()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = AlternateHomePort;
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_AgentType = AgentType.CoLoad;

			AssertEquals("Preconditions: Domestic Consol expected.", true, consol.IsDomestic());
			AssertEquals("Preconditions: CLD mode.", true, consol.IsCoLoad);

			var coloadOrg = Factory.NewWithValidTestData<OrgHeader>();
			var exportCreditor = Factory.NewWithValidTestData<OrgHeader>();
			coloadOrg.OH_IsCreditor = true;
			exportCreditor.OH_IsCreditor = true;
			Factory.Save();

			consol.JK_OA_CreditorAddress = coloadOrg.MainAddress.PK;

			AssertEquals("CLD domestic consol will set Carrier Export Creditor to Co-Load With if cannot find related party from Co-Load With"
				, consol.JK_OA_CreditorAddress, consol.CarrierExportCreditor.MainAddress.PK);

			consol.CarrierExportCreditorAddress.E2_OA_Address = exportCreditor.MainAddress.PK;
			AssertEquals("CLD domestic consol will not update Co-load With when Carrier Export Creditor is updated"
				, coloadOrg.MainAddress.PK, consol.JK_OA_CreditorAddress);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.CarrierExportCreditorAddress.E2_OA_Address = exportCreditor.MainAddress.PK;
			AssertEquals("CLD domestic consol will not set Co-load With when Carrier Export Creditor is set"
				, ZGuid.Empty, consol.JK_OA_CreditorAddress);
		}

		public void TestCrossTradeNonCoLoadConsol_WhenCarrierImportCreditorAddressChange_UpdateConsolCreditor()
		{
			var origin = OverseasPort;
			var destination = OverseasPort2;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;

			Factory.Save();

			AssertEquals("Preconditions: crosstrade consol expected.", true, consol.IsCrossTrade());

			consol.CarrierImportCreditorAddress.E2_OA_Address = carrier.MainAddress.PK;
			Factory.Save();

			AssertEquals(true, consol.CarrierImportCreditorAddress.HasRealOrganisation);
			AssertEquals(carrier, consol.CarrierImportCreditor);
			AssertNoErrors("We should not validate Carrier Import Creditor Address", consol.CarrierImportCreditorAddress.OrganisationPKInfo);

			carrier.OH_IsCreditor = true;
			consol.CarrierImportCreditorAddress.E2_OA_Address = ZGuid.Empty;
			Factory.Save();

			AssertEquals("Should not update Carrier Import Creditor Address when address is invalid", ZGuid.Empty, consol.CarrierImportCreditorAddress.E2_OA_Address);

			consol.CarrierImportCreditorAddress.E2_OA_Address = carrier.MainAddress.PK;
			Factory.Save();

			AssertEquals("Should update consol creditor if Carrier Import Ceditor Address is valid in cross-trade non co-load consol", consol.CarrierImportCreditorAddress.E2_OA_Address, consol.JK_OA_CreditorAddress);

			consol.CarrierImportCreditorAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals("Should clear consol creditor if we remove Carrier Import Ceditor Address in cross-trade non co-load consol", ZGuid.Empty, consol.JK_OA_CreditorAddress);
		}

		public void TestCreditorAddress_WhenNonCoLoadCrossTradeConsolAndSettingCreditor_UpdatesImportCreditorAddress()
		{
			var origin = OverseasPort;
			var destination = OverseasPort2;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;

			Factory.Save();

			AssertEquals("Preconditions: crosstrade consol expected.", true, consol.IsCrossTrade());

			consol.JK_OA_CreditorAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(carrier.PK);
			consol.JK_OA_CreditorAddress = ZGuid.Empty;

			CombineAssertions("Creditor doesn't have valid address, we shouldn't set import creditor address", () =>
			{
				AssertEquals(false, consol.CarrierImportCreditorAddress.IsValidAddress);
				AssertEquals(null, consol.CarrierImportCreditor);
			});

			carrier.OH_IsCreditor = true;
			Factory.Save();

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;

			CombineAssertions("Creditor has valid address, we should set import creditor address", () =>
			{
				AssertEquals(true, consol.CarrierImportCreditorAddress.HasRealOrganisation);
				AssertEquals(carrier, consol.CarrierImportCreditor);
			});

			consol.JK_OA_CreditorAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.Empty);
			consol.JK_OA_CreditorAddress = ZGuid.Empty;

			AssertEquals("Should clear import creditor address if we remove creditor in cross-trade non co-load consol", ZGuid.Empty, consol.CarrierImportCreditorAddress.E2_OA_Address);
			AssertEquals("Should clear import creditor organisation if we remove creditor in cross-trade non co-load consol", ZGuid.Empty, consol.CarrierImportCreditorAddress.OrganisationPK);
		}

		public void TestNotifyPartyAddress()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			AssertEquals(false, consol.NotifyPartyDocumentaryAddress.IsValidAddress);
			AssertEquals(null, consol.NotifyParty);

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "MISC";
			consol.NotifyPartyDocumentaryAddress.OrganisationPK = org.PK;
			AssertEquals(false, consol.NotifyPartyDocumentaryAddress.HasRealOrganisation);
			AssertEquals(null, consol.NotifyParty);

			org.OH_Code = "aaa";
			org.OH_FullName = "bbb";
			AssertEquals(true, consol.NotifyPartyDocumentaryAddress.HasRealOrganisation);
			AssertEquals(org, consol.NotifyParty);

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "ccc";
			OrgDocument document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.NotifyParty.Code;

			consol.NotifyPartyDocumentaryAddress.ContactPK = contact.PK;
			AssertEquals(org.PK, consol.NotifyPartyDocumentaryAddress.OrganisationPK);
			AssertEquals(contact.PK, consol.NotifyPartyDocumentaryAddress.ContactPK);
		}

		public void TestNotifyParty2Address()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			AssertEquals(false, consol.NotifyParty2DocumentaryAddress.IsValidAddress);
			AssertEquals(null, consol.NotifyParty2);

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "MISC";
			consol.NotifyParty2DocumentaryAddress.OrganisationPK = org.PK;
			AssertEquals(false, consol.NotifyParty2DocumentaryAddress.HasRealOrganisation);
			AssertEquals(null, consol.NotifyParty2);

			org.OH_Code = "aaa";
			org.OH_FullName = "bbb";
			AssertEquals(true, consol.NotifyParty2DocumentaryAddress.HasRealOrganisation);
			AssertEquals(org, consol.NotifyParty2);

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "ccc";
			OrgDocument document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.NotifyParty.Code;

			consol.NotifyParty2DocumentaryAddress.ContactPK = contact.PK;
			AssertEquals(org.PK, consol.NotifyParty2DocumentaryAddress.OrganisationPK);
			AssertEquals(contact.PK, consol.NotifyParty2DocumentaryAddress.ContactPK);
		}

		public void TestNotifyParty3Address()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			AssertEquals(false, consol.NotifyParty3DocumentaryAddress.IsValidAddress);
			AssertEquals(null, consol.NotifyParty3);

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "MISC";
			consol.NotifyParty3DocumentaryAddress.OrganisationPK = org.PK;
			AssertEquals(false, consol.NotifyParty3DocumentaryAddress.HasRealOrganisation);
			AssertEquals(null, consol.NotifyParty3);

			org.OH_Code = "aaa";
			org.OH_FullName = "bbb";
			AssertEquals(true, consol.NotifyParty3DocumentaryAddress.HasRealOrganisation);
			AssertEquals(org, consol.NotifyParty3);

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "ccc";
			OrgDocument document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.NotifyParty.Code;

			consol.NotifyParty3DocumentaryAddress.ContactPK = contact.PK;
			AssertEquals(org.PK, consol.NotifyParty3DocumentaryAddress.OrganisationPK);
			AssertEquals(contact.PK, consol.NotifyParty3DocumentaryAddress.ContactPK);
		}

		public void TestMasterBillIssuingPartyAddress()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			AssertEquals(false, consol.MasterBillIssuingPartyDocumentaryAddress.IsValidAddress);
			AssertEquals(null, consol.MasterBillIssuingParty);

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "MISC";
			consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = org.PK;
			AssertEquals(false, consol.MasterBillIssuingPartyDocumentaryAddress.HasRealOrganisation);
			AssertEquals(null, consol.MasterBillIssuingParty);

			org.OH_Code = "aaa";
			org.OH_FullName = "bbb";
			AssertEquals(true, consol.MasterBillIssuingPartyDocumentaryAddress.HasRealOrganisation);
			AssertEquals(org, consol.MasterBillIssuingParty);

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "ccc";
			OrgDocument document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.NotifyParty.Code;

			consol.MasterBillIssuingPartyDocumentaryAddress.ContactPK = contact.PK;
			AssertEquals(org.PK, consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK);
			AssertEquals(contact.PK, consol.MasterBillIssuingPartyDocumentaryAddress.ContactPK);
		}

		public void TestCarrierBookingAgent()
		{
			var consol = Factory.New<ForwardingConsol>();
			AssertEquals(false, consol.CarrierBookingAgentDocumentaryAddress.IsValidAddress);
			AssertEquals(null, consol.CarrierBookingAgent);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "CBAORG";
			org.OH_FullName = "CBA Full";
			org.MainAddress.Address1 = "CBA Address 1";
			org.MainAddress.Address2 = "CBA Address 2";

			consol.CarrierBookingAgentDocumentaryAddress.OrganisationPK = org.PK;

			AssertEquals("CBA Full", consol.CarrierBookingAgentDocumentaryAddress.CompanyName);
			AssertEquals("CBA Address 1", consol.CarrierBookingAgentDocumentaryAddress.Address1);
			AssertEquals("CBA Address 2", consol.CarrierBookingAgentDocumentaryAddress.Address2);
		}

		public void TestCarrierBookingAgent_OnLoadPortCarrierChanged()
		{
			var orgAgentAUMEL = Factory.New<OrgHeader>();
			orgAgentAUMEL.OH_Code = "AGN01";
			orgAgentAUMEL.OH_FullName = "Carrier Booking Agent - AUMEL";
			orgAgentAUMEL.MainAddress.Address1 = "Address 1-1";

			var orgAgentAUSYD = Factory.New<OrgHeader>();
			orgAgentAUSYD.OH_Code = "AGN02";
			orgAgentAUSYD.OH_FullName = "Carrier Booking Agent - AUSYD";
			orgAgentAUSYD.MainAddress.Address1 = "Address 1-2";

			var agentPort1 = ShippingCompany1.CarrierAppointedAgentPorts_Agency.AddNew();
			agentPort1.O5_PortOrCountry = "AUMEL";
			agentPort1.OrganisationPK = orgAgentAUMEL.PK;

			var agentPort2 = ShippingCompany1.CarrierAppointedAgentPorts_Agency.AddNew();
			agentPort2.O5_PortOrCountry = "AUSYD";
			agentPort2.OrganisationPK = orgAgentAUSYD.PK;
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			AssertEquals(null, consol.CarrierBookingAgent);
			AssertEquals(false, consol.CarrierBookingAgentDocumentaryAddress.IsValidAddress);

			consol.JK_RL_NKLoadPort = "AUMEL";
			AssertNull(consol.CarrierBookingAgent);
			AssertEquals(false, consol.CarrierBookingAgentDocumentaryAddress.IsValidAddress);

			consol.JK_RL_NKLoadPort = ZString.Empty;
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			AssertNull(consol.CarrierBookingAgent);
			AssertEquals(false, consol.CarrierBookingAgentDocumentaryAddress.IsValidAddress);

			consol.JK_RL_NKLoadPort = "AUMEL";
			AssertNotNull(consol.CarrierBookingAgent);
			AssertEquals(orgAgentAUMEL, consol.CarrierBookingAgent);
			AssertEquals(true, consol.CarrierBookingAgentDocumentaryAddress.IsValidAddress);
			AssertEquals(orgAgentAUMEL.MainAddress.PK, consol.CarrierBookingAgentDocumentaryAddress.E2_OA_Address);

			consol.JK_RL_NKLoadPort = "AUSYD";
			AssertNotNull(consol.CarrierBookingAgent);
			AssertEquals(orgAgentAUSYD, consol.CarrierBookingAgent);
			AssertEquals(true, consol.CarrierBookingAgentDocumentaryAddress.IsValidAddress);
			AssertEquals(orgAgentAUSYD.MainAddress.PK, consol.CarrierBookingAgentDocumentaryAddress.E2_OA_Address);
		}

		public void TestCarrierHandlingAgent()
		{
			var consol = Factory.New<ForwardingConsol>();
			AssertEquals(false, consol.CarrierHandlingAgentDocumentaryAddress.IsValidAddress);
			AssertEquals(null, consol.CarrierHandlingAgent);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "CHAORG";
			org.OH_FullName = "CHA Full";
			org.MainAddress.Address1 = "CHA Address 1";
			org.MainAddress.Address2 = "CHA Address 2";

			consol.CarrierHandlingAgentDocumentaryAddress.OrganisationPK = org.PK;

			AssertEquals("CHA Full", consol.CarrierHandlingAgentDocumentaryAddress.CompanyName);
			AssertEquals("CHA Address 1", consol.CarrierHandlingAgentDocumentaryAddress.Address1);
			AssertEquals("CHA Address 2", consol.CarrierHandlingAgentDocumentaryAddress.Address2);
		}

		public void TestMasterBillShipperOverrideAddress()
		{
			var consol = Factory.New<ForwardingConsol>();
			AssertEquals(false, consol.MasterBillShipperOverrideDocumentaryAddress.IsValidAddress);
			AssertEquals(null, consol.MasterBillShipperOverride);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "MISC";
			consol.MasterBillShipperOverrideDocumentaryAddress.OrganisationPK = org.PK;
			AssertEquals(false, consol.MasterBillShipperOverrideDocumentaryAddress.HasRealOrganisation);
			AssertEquals(null, consol.MasterBillShipperOverride);

			org.OH_Code = "aaa";
			org.OH_FullName = "bbb";
			AssertEquals(true, consol.MasterBillShipperOverrideDocumentaryAddress.HasRealOrganisation);
			AssertEquals(org, consol.MasterBillShipperOverride);

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "ccc";

			consol.MasterBillShipperOverrideDocumentaryAddress.ContactPK = contact.PK;
			AssertEquals(org.PK, consol.MasterBillShipperOverrideDocumentaryAddress.OrganisationPK);
			AssertEquals(contact.PK, consol.MasterBillShipperOverrideDocumentaryAddress.ContactPK);
		}

		public void TestMasterBillConsigneeOverrideAdress()
		{
			var consol = Factory.New<ForwardingConsol>();
			AssertEquals(false, consol.MasterBillConsigneeOverrideDocumentaryAddress.IsValidAddress);
			AssertEquals(null, consol.MasterBillConsigneeOverride);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "MISC";
			consol.MasterBillConsigneeOverrideDocumentaryAddress.OrganisationPK = org.PK;
			AssertEquals(false, consol.MasterBillConsigneeOverrideDocumentaryAddress.HasRealOrganisation);
			AssertEquals(null, consol.MasterBillConsigneeOverride);

			org.OH_Code = "aaa";
			org.OH_FullName = "bbb";
			AssertEquals(true, consol.MasterBillConsigneeOverrideDocumentaryAddress.HasRealOrganisation);
			AssertEquals(org, consol.MasterBillConsigneeOverride);

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "ccc";

			consol.MasterBillConsigneeOverrideDocumentaryAddress.ContactPK = contact.PK;
			AssertEquals(org.PK, consol.MasterBillConsigneeOverrideDocumentaryAddress.OrganisationPK);
			AssertEquals(contact.PK, consol.MasterBillConsigneeOverrideDocumentaryAddress.ContactPK);
		}

		public void TestFreightPayerAddress()
		{
			var consol = Factory.New<ForwardingConsol>();
			AssertEquals(false, consol.FreightPayerDocumentaryAddress.IsValidAddress);
			AssertEquals(null, consol.FreightPayer);

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "MISC";
			consol.FreightPayerDocumentaryAddress.OrganisationPK = org.PK;
			AssertEquals(false, consol.FreightPayerDocumentaryAddress.HasRealOrganisation);
			AssertEquals(null, consol.FreightPayer);

			org.OH_Code = "aaa";
			org.OH_FullName = "bbb";
			AssertEquals(true, consol.FreightPayerDocumentaryAddress.HasRealOrganisation);
			AssertEquals(org, consol.FreightPayer);

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "ccc";

			consol.FreightPayerDocumentaryAddress.ContactPK = contact.PK;
			AssertEquals(org.PK, consol.FreightPayerDocumentaryAddress.OrganisationPK);
			AssertEquals(contact.PK, consol.FreightPayerDocumentaryAddress.ContactPK);
		}

		public void TestWhenReceivingForwarderIsSetToEmptyThenHandlingTypeIsSetToEmpty()
		{
			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol.ReceivingForwarderWithContact.OrgPK = receivingForwarder.PK;

			AssertEquals(AgentStatusList.Codes.GatewayAgentWithTariff, consol.JK_SendingForwarderHandlingType);

			consol.ReceivingForwarderWithContact.OrgPK = Guid.Empty;

			AssertEquals(string.Empty, consol.JK_ReceivingForwarderHandlingType);
		}

		public void TestSetNotifyPartyAfterReceivingForwarderChanges_DirectConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Direct;
			AssertEquals(ZGuid.Empty, consol.NotifyPartyDocumentaryAddress.OrganisationPK);

			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			var receivingForwarderNotifyParty = Factory.NewWithValidTestData<OrgHeader>();

			var receivingForwarderContact = receivingForwarder.Contacts.AddNew();
			var notifyPartyDocument = receivingForwarderContact.Documents.AddNew();
			notifyPartyDocument.OD_DocumentGroup = ContactType.NotifyParty.Code;

			var receivingForwarderNotifyPartyContact = receivingForwarderNotifyParty.Contacts.AddNew();
			notifyPartyDocument = receivingForwarderNotifyPartyContact.Documents.AddNew();
			notifyPartyDocument.OD_DocumentGroup = ContactType.NotifyParty.Code;

			receivingForwarder.NotifyPartyDocumentaryAddress.OrganisationPK = receivingForwarderNotifyParty.PK;

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			AssertEquals("Direct Consol's NotifyParty defaults to ReceivingForwarder", receivingForwarder.PK, consol.NotifyPartyDocumentaryAddress.OrganisationPK);
			AssertEquals("Direct Consol's NotifyParty address contact defaults to ReceivingForwarder contact", receivingForwarderContact.PK, consol.NotifyPartyDocumentaryAddress.ContactPK);

			var anotherReceivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			var anotherReceivingForwarderContact = anotherReceivingForwarder.Contacts.AddNew();
			notifyPartyDocument = anotherReceivingForwarderContact.Documents.AddNew();
			notifyPartyDocument.OD_DocumentGroup = ContactType.NotifyParty.Code;

			consol.JK_OA_ReceivingForwarderAddress = anotherReceivingForwarder.MainAddress.PK;
			AssertEquals("Direct Consol's NotifyParty re-defaults to new ReceivingForwarder", anotherReceivingForwarder.PK, consol.NotifyPartyDocumentaryAddress.OrganisationPK);
			AssertEquals(anotherReceivingForwarderContact.PK, consol.NotifyPartyDocumentaryAddress.ContactPK);

			var receivingForwarderWithoutContact = Factory.NewWithValidTestData<OrgHeader>();

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarderWithoutContact.MainAddress.PK;
			AssertEquals("Direct Consol's NotifyParty does not re-default when new ReceivingForwarder has no contact with type NotifyParty", anotherReceivingForwarder.PK, consol.NotifyPartyDocumentaryAddress.OrganisationPK);
			AssertEquals(anotherReceivingForwarderContact.PK, consol.NotifyPartyDocumentaryAddress.ContactPK);
		}

		public void TestSetMasterBillConsigneeOverrideAfterReceivingForwarderChanges_DirectConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Direct;
			AssertEquals(ZGuid.Empty, consol.MasterBillConsigneeOverrideDocumentaryAddress.OrganisationPK);

			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			var receivingForwarderConsignee = Factory.NewWithValidTestData<OrgHeader>();

			var receivingForwarderContact = receivingForwarder.Contacts.AddNew();
			var notifyPartyDocument = receivingForwarderContact.Documents.AddNew();
			notifyPartyDocument.OD_DocumentGroup = ContactType.NotifyParty.Code;

			var receivingForwarderConsigneeContact = receivingForwarderConsignee.Contacts.AddNew();
			notifyPartyDocument = receivingForwarderConsigneeContact.Documents.AddNew();
			notifyPartyDocument.OD_DocumentGroup = ContactType.NotifyParty.Code;

			receivingForwarder.MasterBillConsigneeOverrideDocumentaryAddress.OrganisationPK = receivingForwarderConsignee.PK;

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			AssertEquals("Direct Consol's MasterBillConsigneeOverride defaults to ReceivingForwarder", ZGuid.Empty, consol.MasterBillConsigneeOverrideDocumentaryAddress.OrganisationPK);
			AssertEquals("Direct Consol's MasterBillConsigneeOverride address contact defaults to ReceivingForwarder contact", ZGuid.Empty, consol.MasterBillConsigneeOverrideDocumentaryAddress.ContactPK);

			var anotherReceivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			var anotherReceivingForwarderContact = anotherReceivingForwarder.Contacts.AddNew();
			notifyPartyDocument = anotherReceivingForwarderContact.Documents.AddNew();
			notifyPartyDocument.OD_DocumentGroup = ContactType.NotifyParty.Code;

			consol.JK_OA_ReceivingForwarderAddress = anotherReceivingForwarder.MainAddress.PK;
			AssertEquals("Direct Consol's MasterBillConsigneeOverride re-defaults to new ReceivingForwarder", ZGuid.Empty, consol.MasterBillConsigneeOverrideDocumentaryAddress.OrganisationPK);
			AssertEquals(ZGuid.Empty, consol.MasterBillConsigneeOverrideDocumentaryAddress.ContactPK);

			var receivingForwarderWithoutContact = Factory.NewWithValidTestData<OrgHeader>();

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarderWithoutContact.MainAddress.PK;
			AssertEquals("Direct Consol's MasterBillConsigneeOverride re-defaults when new ReceivingForwarder has no contact with type NotifyParty", ZGuid.Empty, consol.MasterBillConsigneeOverrideDocumentaryAddress.OrganisationPK);
			AssertEquals(ZGuid.Empty, consol.MasterBillConsigneeOverrideDocumentaryAddress.ContactPK);
		}

		public void TestSetMasterBillShipperOverrideAfterSendingForwarderChanges_DirectConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Direct;
			AssertEquals(ZGuid.Empty, consol.MasterBillShipperOverrideDocumentaryAddress.OrganisationPK);

			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			var sendingForwarderShipper = Factory.NewWithValidTestData<OrgHeader>();

			var sendingForwarderContact = sendingForwarder.Contacts.AddNew();
			var notifyPartyDocument = sendingForwarderContact.Documents.AddNew();
			notifyPartyDocument.OD_DocumentGroup = ContactType.NotifyParty.Code;

			var sendingForwarderShipperContact = sendingForwarderShipper.Contacts.AddNew();
			notifyPartyDocument = sendingForwarderShipperContact.Documents.AddNew();
			notifyPartyDocument.OD_DocumentGroup = ContactType.NotifyParty.Code;

			sendingForwarder.MasterBillShipperOverrideDocumentaryAddress.OrganisationPK = sendingForwarderShipper.PK;

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			AssertEquals("Direct Consol's MasterBillShipperOverride defaults from SendingForwarder", ZGuid.Empty, consol.MasterBillShipperOverrideDocumentaryAddress.OrganisationPK);
			AssertEquals("Direct Consol's MasterBillShipperOverride address contact defaults from SendingForwarder contact", ZGuid.Empty, consol.MasterBillShipperOverrideDocumentaryAddress.ContactPK);

			var anotherSendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			var anotherSendingForwarderContact = anotherSendingForwarder.Contacts.AddNew();
			notifyPartyDocument = anotherSendingForwarderContact.Documents.AddNew();
			notifyPartyDocument.OD_DocumentGroup = ContactType.NotifyParty.Code;

			consol.JK_OA_SendingForwarderAddress = anotherSendingForwarder.MainAddress.PK;
			AssertEquals("Direct Consol's MasterBillShipperOverride re-defaults to new SendingForwarder", ZGuid.Empty, consol.MasterBillShipperOverrideDocumentaryAddress.OrganisationPK);
			AssertEquals(ZGuid.Empty, consol.MasterBillShipperOverrideDocumentaryAddress.ContactPK);

			var sendingForwarderWithoutContact = Factory.NewWithValidTestData<OrgHeader>();

			consol.JK_OA_SendingForwarderAddress = sendingForwarderWithoutContact.MainAddress.PK;
			AssertEquals("Direct Consol's MasterBillShipperOverride re-defaults when new SendingForwarder has no contact with type NotifyParty", ZGuid.Empty, consol.MasterBillShipperOverrideDocumentaryAddress.OrganisationPK);
			AssertEquals(ZGuid.Empty, consol.MasterBillShipperOverrideDocumentaryAddress.ContactPK);
		}

		public void TestSetNotifyPartyAfterReceivingForwarderChanges_NonDirectConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(ZGuid.Empty, consol.NotifyPartyDocumentaryAddress.OrganisationPK);

			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			AssertEquals("Consol's NotifyParty will not be defaulted when ReceivingForwarder does not have NotifyParty", ZGuid.Empty, consol.NotifyPartyDocumentaryAddress.OrganisationPK);

			receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			var receivingForwarderNotifyParty = Factory.NewWithValidTestData<OrgHeader>();

			var marketingContact = receivingForwarderNotifyParty.Contacts.AddNew();
			var marketingDocument = marketingContact.Documents.AddNew();
			marketingDocument.OD_DocumentGroup = ContactType.Marketing.Code;

			var notifyPartyContact = receivingForwarderNotifyParty.Contacts.AddNew();
			var notifyPartyDocument = notifyPartyContact.Documents.AddNew();
			notifyPartyDocument.OD_DocumentGroup = ContactType.NotifyParty.Code;

			receivingForwarder.NotifyPartyDocumentaryAddress.OrganisationPK = receivingForwarderNotifyParty.PK;

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			AssertEquals("Consol's NotifyParty defaults from ReceivingForwarder's NotifyParty when it has NotifyParty", receivingForwarderNotifyParty.PK, consol.NotifyPartyDocumentaryAddress.OrganisationPK);
			AssertEquals("Consol's NotifyParty address contact defaults from Contact with NotifyParty as DocumentGroup", notifyPartyContact.PK, consol.NotifyPartyDocumentaryAddress.ContactPK);

			var anotherReceivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			var anotherReceivingForwarderNotifyParty = Factory.NewWithValidTestData<OrgHeader>();
			var anotherNotifyPartyContact = anotherReceivingForwarderNotifyParty.Contacts.AddNew();
			notifyPartyDocument = anotherNotifyPartyContact.Documents.AddNew();
			notifyPartyDocument.OD_DocumentGroup = ContactType.NotifyParty.Code;

			anotherReceivingForwarder.NotifyPartyDocumentaryAddress.OrganisationPK = anotherReceivingForwarderNotifyParty.PK;

			consol.JK_OA_ReceivingForwarderAddress = anotherReceivingForwarder.MainAddress.PK;
			AssertEquals("Consol's NotifyParty re-defaults to new ReceivingForwarder's NotifyParty when it has NotifyParty", anotherReceivingForwarderNotifyParty.PK, consol.NotifyPartyDocumentaryAddress.OrganisationPK);
			AssertEquals(anotherNotifyPartyContact.PK, consol.NotifyPartyDocumentaryAddress.ContactPK);

			receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			var receivingForwarderContact = receivingForwarder.Contacts.AddNew();
			notifyPartyDocument = receivingForwarderContact.Documents.AddNew();
			notifyPartyDocument.OD_DocumentGroup = ContactType.NotifyParty.Code;

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			AssertEquals("Consol's NotifyParty re-defaults to new ReceivingForwarder when ReceivingForwarder does not have NotifyParty", receivingForwarder.PK, consol.NotifyPartyDocumentaryAddress.OrganisationPK);
			AssertEquals(receivingForwarderContact.PK, consol.NotifyPartyDocumentaryAddress.ContactPK);

			var receivingForwarderWithoutNotifyParty = Factory.NewWithValidTestData<OrgHeader>();

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarderWithoutNotifyParty.MainAddress.PK;
			AssertEquals("Consol's NotifyParty does not re-default when new ReceivingForwarder has no NotifyParty and no contact with type NotifyParty", receivingForwarder.PK, consol.NotifyPartyDocumentaryAddress.OrganisationPK);
			AssertEquals(receivingForwarderContact.PK, consol.NotifyPartyDocumentaryAddress.ContactPK);
		}

		public void TestNonDirectConsolSetNotifyParty_WhenReceivingForwarderChanges_ThenAddressIsSelectedAddress()
		{
			var receivingForwarderNotifyParty = Factory.NewWithValidTestData<OrgHeader>();
			var selectedAddress = receivingForwarderNotifyParty.Addresses.AddNew(OrgAddressType.Office, false);
			Debug.Assert(selectedAddress.PK != receivingForwarderNotifyParty.MainAddress.PK, "SelectedAddress Should Not be MainAddress");

			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			receivingForwarder.NotifyPartyDocumentaryAddress.OrganisationPK = receivingForwarderNotifyParty.PK;
			receivingForwarder.NotifyPartyDocumentaryAddress.E2_OA_Address = selectedAddress.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			AssertEquals("Consol's NotifyParty Address re-defaults to new ReceivingForwarder's Selected NotifyParty Address when it has NotifyParty", selectedAddress.PK, consol.NotifyPartyDocumentaryAddress.E2_OA_Address);
		}

		public void TestSetMasterBillConsigneeOverrideAfterReceivingForwarderChanges_NonDirectConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(ZGuid.Empty, consol.MasterBillConsigneeOverrideDocumentaryAddress.OrganisationPK);

			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			AssertEquals("Consol's MasterBillConsigneeOverride will not be defaulted when ReceivingForwarder does not have MasterBillConsigneeOverride", ZGuid.Empty, consol.MasterBillConsigneeOverrideDocumentaryAddress.OrganisationPK);

			receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			var receivingForwarderConsignee = Factory.NewWithValidTestData<OrgHeader>();

			var marketingContact = receivingForwarderConsignee.Contacts.AddNew();
			var marketingDocument = marketingContact.Documents.AddNew();
			marketingDocument.OD_DocumentGroup = ContactType.Marketing.Code;

			var consigneeContact = receivingForwarderConsignee.Contacts.AddNew();
			var consgineeDocument = consigneeContact.Documents.AddNew();
			consgineeDocument.OD_DocumentGroup = ContactType.Consignee.Code;

			receivingForwarder.MasterBillConsigneeOverrideDocumentaryAddress.OrganisationPK = receivingForwarderConsignee.PK;

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			AssertEquals("Consol's MasterBillConsigneeOverride defaults to ReceivingForwarder's MasterBillConsigneeOverride when it has MasterBillConsigneeOverride", receivingForwarderConsignee.PK, consol.MasterBillConsigneeOverrideDocumentaryAddress.OrganisationPK);
			AssertEquals("Consol's MasterBillConsigneeOverride address contact defaults from Contact with Consignee as DocumentGroup", consigneeContact.PK, consol.MasterBillConsigneeOverrideDocumentaryAddress.ContactPK);

			var anotherReceivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			var anotherReceivingForwarderConsignee = Factory.NewWithValidTestData<OrgHeader>();
			var anotherConsigneeContact = anotherReceivingForwarderConsignee.Contacts.AddNew();
			consgineeDocument = anotherConsigneeContact.Documents.AddNew();
			consgineeDocument.OD_DocumentGroup = ContactType.Consignee.Code;

			anotherReceivingForwarder.MasterBillConsigneeOverrideDocumentaryAddress.OrganisationPK = anotherReceivingForwarderConsignee.PK;

			consol.JK_OA_ReceivingForwarderAddress = anotherReceivingForwarder.MainAddress.PK;
			AssertEquals("Consol's MasterBillConsigneeOverride re-defaults to new ReceivingForwarder's MasterBillConsigneeOverride when it has MasterBillConsigneeOverride", anotherReceivingForwarderConsignee.PK, consol.MasterBillConsigneeOverrideDocumentaryAddress.OrganisationPK);
			AssertEquals(anotherConsigneeContact.PK, consol.MasterBillConsigneeOverrideDocumentaryAddress.ContactPK);

			receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			var receivingForwarderConsigneeWithoutContact = Factory.NewWithValidTestData<OrgHeader>();
			receivingForwarder.MasterBillConsigneeOverrideDocumentaryAddress.OrganisationPK = receivingForwarderConsigneeWithoutContact.PK;

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			AssertEquals("Consol's MasterBillConsigneeOverride re-defaults when new ReceivingForwarder's MasterBillConsigneeOverride has no contact with type Consignee", receivingForwarderConsigneeWithoutContact.PK, consol.MasterBillConsigneeOverrideDocumentaryAddress.OrganisationPK);
			AssertEquals(ZGuid.Empty, consol.MasterBillConsigneeOverrideDocumentaryAddress.ContactPK);

			receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			AssertEquals("Consol's MasterBillConsigneeOverride does re-default when new ReceivingForwarder is without MasterBillConsigneeOverride", ZGuid.Empty, consol.MasterBillConsigneeOverrideDocumentaryAddress.OrganisationPK);
			AssertEquals(ZGuid.Empty, consol.MasterBillConsigneeOverrideDocumentaryAddress.ContactPK);
		}

		public void TestNonDirectConsolSetMasterBillConsigneeOverride_WhenReceivingForwarderChanges_ThenAddressIsSelectedAddress()
		{
			var receivingForwarderConsignee = Factory.NewWithValidTestData<OrgHeader>();
			var selectedAddress = receivingForwarderConsignee.Addresses.AddNew(OrgAddressType.Office, false);
			Debug.Assert(selectedAddress.PK != receivingForwarderConsignee.MainAddress.PK, "SelectedAddress Should Not be MainAddress");

			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			receivingForwarder.MasterBillConsigneeOverrideDocumentaryAddress.OrganisationPK = receivingForwarderConsignee.PK;
			receivingForwarder.MasterBillConsigneeOverrideDocumentaryAddress.E2_OA_Address = selectedAddress.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			AssertEquals("Consol's MasterBillConsigneeOverride Address re-defaults to new ReceivingForwarder's Selected MasterBillConsigneeOverride Address when it has MasterBillConsigneeOverride", selectedAddress.PK, consol.MasterBillConsigneeOverrideDocumentaryAddress.E2_OA_Address);
		}

		public void TestSetMasterBillShipperOverrideAfterSendingForwarderChanges_NonDirectConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(ZGuid.Empty, consol.MasterBillShipperOverrideDocumentaryAddress.OrganisationPK);

			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			AssertEquals("Consol's MasterBillShipperOverride will not be defaulted when SendingForwarder does not have MasterBillShipperOverride", ZGuid.Empty, consol.MasterBillShipperOverrideDocumentaryAddress.OrganisationPK);

			sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			var sendingForwarderShipper = Factory.NewWithValidTestData<OrgHeader>();

			var marketingContact = sendingForwarderShipper.Contacts.AddNew();
			var marketingDocument = marketingContact.Documents.AddNew();
			marketingDocument.OD_DocumentGroup = ContactType.Marketing.Code;

			var shipperContact = sendingForwarderShipper.Contacts.AddNew();
			var shipperDocument = shipperContact.Documents.AddNew();
			shipperDocument.OD_DocumentGroup = ContactType.Consignor.Code;

			sendingForwarder.MasterBillShipperOverrideDocumentaryAddress.OrganisationPK = sendingForwarderShipper.PK;

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			AssertEquals("Consol's MasterBillShipperOverride defaults to SendingForwarder's MasterBillShipperOverride when it has MasterBillShipperOverride", sendingForwarderShipper.PK, consol.MasterBillShipperOverrideDocumentaryAddress.OrganisationPK);
			AssertEquals("Consol's MasterBillShipperOverride address contact defaults from Contact with Consignor as DocumentGroup", shipperContact.PK, consol.MasterBillShipperOverrideDocumentaryAddress.ContactPK);

			var anotherSendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			var anotherSendingForwarderShipper = Factory.NewWithValidTestData<OrgHeader>();
			var anotherShipperContact = anotherSendingForwarderShipper.Contacts.AddNew();
			shipperDocument = anotherShipperContact.Documents.AddNew();
			shipperDocument.OD_DocumentGroup = ContactType.Consignor.Code;

			anotherSendingForwarder.MasterBillShipperOverrideDocumentaryAddress.OrganisationPK = anotherSendingForwarderShipper.PK;

			consol.JK_OA_SendingForwarderAddress = anotherSendingForwarder.MainAddress.PK;
			AssertEquals("Consol's MasterBillShipperOverride re-defaults to new SendingForwarder's MasterBillShipperOverride when it has MasterBillShipperOverride", anotherSendingForwarderShipper.PK, consol.MasterBillShipperOverrideDocumentaryAddress.OrganisationPK);
			AssertEquals(anotherShipperContact.PK, consol.MasterBillShipperOverrideDocumentaryAddress.ContactPK);

			sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			var sendingForwarderShipperWithoutContact = Factory.NewWithValidTestData<OrgHeader>();
			sendingForwarder.MasterBillShipperOverrideDocumentaryAddress.OrganisationPK = sendingForwarderShipperWithoutContact.PK;

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			AssertEquals("Consol's MasterBillShipperOverride re-defaults when new SendingForwarder's MasterBillShipperOverride has no contact with type Consignor", sendingForwarderShipperWithoutContact.PK, consol.MasterBillShipperOverrideDocumentaryAddress.OrganisationPK);
			AssertEquals(ZGuid.Empty, consol.MasterBillShipperOverrideDocumentaryAddress.ContactPK);

			sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			AssertEquals("Consol's MasterBillShipperOverride does re-default when new SendingForwarder is without MasterBillShipperOverride", ZGuid.Empty, consol.MasterBillShipperOverrideDocumentaryAddress.OrganisationPK);
			AssertEquals(ZGuid.Empty, consol.MasterBillShipperOverrideDocumentaryAddress.ContactPK);
		}

		public void TestNonDirectConsolSetMasterBillShipperOverride_WhenSendingForwarderChanges_ThenAddressIsSelectedAddress()
		{
			var sendingForwarderShipper = Factory.NewWithValidTestData<OrgHeader>();
			var selectedAddress = sendingForwarderShipper.Addresses.AddNew(OrgAddressType.Office, false);
			Debug.Assert(selectedAddress.PK != sendingForwarderShipper.MainAddress.PK, "SelectedAddress Should Not be MainAddress");

			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			sendingForwarder.MasterBillShipperOverrideDocumentaryAddress.OrganisationPK = sendingForwarderShipper.PK;
			sendingForwarder.MasterBillShipperOverrideDocumentaryAddress.E2_OA_Address = selectedAddress.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			AssertEquals("Consol's MasterBillShipperOverride Address re-defaults to new SendingForwarder's Selected MasterBillShipperOverride Address when it has MasterBillShipperOverride", selectedAddress.PK, consol.MasterBillShipperOverrideDocumentaryAddress.E2_OA_Address);
		}

		public void TestDefaultSelfFiler_OnSavingAfterSwitchingJK_AgentType()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "NO!2#";

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "org1";
			org1.OH_IsConsignee = true;
			org1.MiscServ.OM_IMAdvanceCargoReportingSelfFiler = true;

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "org2";
			org2.OH_IsForwarder = true;
			org2.MiscServ.OM_FWAdvanceCargoReportingSelfFiler = true;

			var existingRelationShip = org1.AllRelatedParties.AddNew();
			existingRelationShip.PR_PartyType = RelatedPartyTypeList.Codes.SelfFilerForICS2;
			existingRelationShip.PR_OH_RelatedParty = org2.PK;
			existingRelationShip.PR_Location = unloco.RL_Code;
			existingRelationShip.PR_FreightTransportMode = TransportModes.Sea;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Direct;
			consol.JK_RL_NKDischargePort = "NO!2#";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_OA_ReceivingForwarderAddress = org1.MainAddress.PK;

			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NO!2#";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment.ConsigneePK = org1.PK;

			Factory.Save();

			consol.JK_AgentType = AgentType.Agent;
			var selfFiler = consol.DocAddresses.FindByDocAddressType(DocAddressType.SelfFiler);

			AssertNull(selfFiler);

			Factory.Save();

			selfFiler = consol.DocAddresses.FindByDocAddressType(DocAddressType.SelfFiler);

			AssertNotNull(selfFiler);
			AssertEquals(org2.PK, selfFiler.OrganisationPK);

			consol.JK_AgentType = AgentType.Direct;
			selfFiler = consol.DocAddresses.FindByDocAddressType(DocAddressType.SelfFiler);

			AssertNotNull(selfFiler);
			AssertEquals(org1.PK, selfFiler.OrganisationPK);

			Factory.Save();

			selfFiler = consol.DocAddresses.FindByDocAddressType(DocAddressType.SelfFiler);

			AssertNull(selfFiler);
		}

		public void TestDefaultSelfFiler_OnSavingAfterChangingJK_OA_ReceivingForwarderAddress()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "NO!2#";

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "org1";

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "org2";
			org2.OH_IsForwarder = true;
			org2.MiscServ.OM_FWAdvanceCargoReportingSelfFiler = true;

			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "org3";
			org3.OH_IsForwarder = true;
			org3.MiscServ.OM_FWAdvanceCargoReportingSelfFiler = true;

			var existingRelationShip = org1.AllRelatedParties.AddNew();
			existingRelationShip.PR_PartyType = RelatedPartyTypeList.Codes.SelfFilerForICS2;
			existingRelationShip.PR_OH_RelatedParty = org2.PK;
			existingRelationShip.PR_Location = unloco.RL_Code;
			existingRelationShip.PR_FreightTransportMode = TransportModes.Sea;

			var existingRelationShip2 = org1.AllRelatedParties.AddNew();
			existingRelationShip2.PR_PartyType = RelatedPartyTypeList.Codes.SelfFilerForICS2;
			existingRelationShip2.PR_OH_RelatedParty = org3.PK;
			existingRelationShip2.PR_Location = unloco.RL_Code;
			existingRelationShip2.PR_FreightTransportMode = TransportModes.All;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "NO!2#";
			consol.JK_RL_NKLoadPort = "AUSYD";

			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NO!2#";

			Factory.Save();

			consol.JK_OA_ReceivingForwarderAddress = org1.MainAddress.PK;
			var selfFiler = consol.DocAddresses.FindByDocAddressType(DocAddressType.SelfFiler);

			AssertNull(selfFiler);

			Factory.Save();

			selfFiler = consol.DocAddresses.FindByDocAddressType(DocAddressType.SelfFiler);

			AssertNotNull(selfFiler);
			AssertEquals(org2.PK, selfFiler.OrganisationPK);

			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			selfFiler = consol.DocAddresses.FindByDocAddressType(DocAddressType.SelfFiler);

			AssertNotNull(selfFiler);
			AssertEquals(org2.PK, selfFiler.OrganisationPK);

			Factory.Save();

			selfFiler = consol.DocAddresses.FindByDocAddressType(DocAddressType.SelfFiler);

			AssertNull(selfFiler);

			org1.OH_IsForwarder = true;
			org1.MiscServ.OM_FWAdvanceCargoReportingSelfFiler = true;

			consol.JK_OA_ReceivingForwarderAddress = org1.MainAddress.PK;

			Factory.Save();

			selfFiler = consol.DocAddresses.FindByDocAddressType(DocAddressType.SelfFiler);

			AssertNotNull(selfFiler);
			AssertEquals(org1.PK, selfFiler.OrganisationPK);
		}

		public void TestSefFiler_CanDeleteDocAddress()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "NO!2#";

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "org1";

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "org2";
			org2.OH_IsForwarder = true;
			org2.MiscServ.OM_FWAdvanceCargoReportingSelfFiler = true;

			var existingRelationShip = org1.AllRelatedParties.AddNew();
			existingRelationShip.PR_PartyType = RelatedPartyTypeList.Codes.SelfFilerForICS2;
			existingRelationShip.PR_OH_RelatedParty = org2.PK;
			existingRelationShip.PR_Location = unloco.RL_Code;
			existingRelationShip.PR_FreightTransportMode = TransportModes.Sea;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "NO!2#";
			consol.JK_RL_NKLoadPort = "AUSYD";

			var transport = consol.Transports[0];
			transport.JW_TransportMode = TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NO!2#";

			consol.JK_OA_ReceivingForwarderAddress = org1.MainAddress.PK;

			Factory.Save();

			var selfFiler = consol.DocAddresses.FindByDocAddressType(DocAddressType.SelfFiler);

			AssertNotNull(selfFiler);
			Assert(!((IDocAddresses)consol).CanDeleteAddress(selfFiler));

			existingRelationShip.PR_Location = "AUSYD";

			Factory.Save();

			selfFiler = consol.DocAddresses.FindByDocAddressType(DocAddressType.SelfFiler);

			Assert(((IDocAddresses)consol).CanDeleteAddress(selfFiler));

			existingRelationShip.PR_Location = unloco.RL_Code;
			transport.JW_RL_NKDiscPort = "NZAKL";

			Factory.Save();

			selfFiler = consol.DocAddresses.FindByDocAddressType(DocAddressType.SelfFiler);

			Assert(((IDocAddresses)consol).CanDeleteAddress(selfFiler));
		}

		public void TestDefaultSelfFiler_OnSavingAfterChangingJK_RL_NKLoadPort()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "NO!2#";

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "org1";
			org1.OH_IsConsignee = true;
			org1.MiscServ.OM_IMAdvanceCargoReportingSelfFiler = true;

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "org2";
			org2.OH_IsForwarder = true;
			org2.MiscServ.OM_FWAdvanceCargoReportingSelfFiler = true;

			var existingRelationShip = org1.AllRelatedParties.AddNew();
			existingRelationShip.PR_PartyType = RelatedPartyTypeList.Codes.SelfFilerForICS2;
			existingRelationShip.PR_OH_RelatedParty = org2.PK;
			existingRelationShip.PR_Location = unloco.RL_Code;
			existingRelationShip.PR_FreightTransportMode = TransportModes.Sea;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NO!2#";
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_OA_ReceivingForwarderAddress = org1.MainAddress.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment.ConsigneePK = org1.PK;

			Factory.Save();

			var selfFiler = consol.DocAddresses.FindByDocAddressType(DocAddressType.SelfFiler);

			AssertNotNull(selfFiler);
			AssertEquals(org2.PK, selfFiler.OrganisationPK);

			consol.JK_RL_NKLoadPort = "NOSYD";

			Factory.Save();

			selfFiler = consol.DocAddresses.FindByDocAddressType(DocAddressType.SelfFiler);

			AssertNull(selfFiler);
		}

		public void TestDefaultSelfFiler_OnSavingAfterChangingJK_RL_NKDischargePort()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "NO!2#";

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "org1";
			org1.OH_IsConsignee = true;
			org1.MiscServ.OM_IMAdvanceCargoReportingSelfFiler = true;

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "org2";
			org2.OH_IsForwarder = true;
			org2.MiscServ.OM_FWAdvanceCargoReportingSelfFiler = true;

			var existingRelationShip = org1.AllRelatedParties.AddNew();
			existingRelationShip.PR_PartyType = RelatedPartyTypeList.Codes.SelfFilerForICS2;
			existingRelationShip.PR_OH_RelatedParty = org2.PK;
			existingRelationShip.PR_Location = unloco.RL_Code;
			existingRelationShip.PR_FreightTransportMode = TransportModes.Sea;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NO!2#";
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_OA_ReceivingForwarderAddress = org1.MainAddress.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment.ConsigneePK = org1.PK;

			Factory.Save();

			var selfFiler = consol.DocAddresses.FindByDocAddressType(DocAddressType.SelfFiler);

			AssertNotNull(selfFiler);
			AssertEquals(org2.PK, selfFiler.OrganisationPK);

			consol.JK_RL_NKDischargePort = "NA!2#";

			Factory.Save();

			selfFiler = consol.DocAddresses.FindByDocAddressType(DocAddressType.SelfFiler);

			AssertNull(selfFiler);
		}

		public void TestDefaultSelfFiler_OnSavingAfterAnyTransportChange()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "NO!2#";

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "org1";
			org1.OH_IsConsignee = true;
			org1.MiscServ.OM_IMAdvanceCargoReportingSelfFiler = true;

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "org2";
			org2.OH_IsForwarder = true;
			org2.MiscServ.OM_FWAdvanceCargoReportingSelfFiler = true;

			var existingRelationShip = org1.AllRelatedParties.AddNew();
			existingRelationShip.PR_PartyType = RelatedPartyTypeList.Codes.SelfFilerForICS2;
			existingRelationShip.PR_OH_RelatedParty = org2.PK;
			existingRelationShip.PR_Location = unloco.RL_Code;
			existingRelationShip.PR_FreightTransportMode = TransportModes.Sea;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "NO!2#";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_OA_ReceivingForwarderAddress = org1.MainAddress.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment.ConsigneePK = org1.PK;

			var selfFiler = consol.DocAddresses.FindByDocAddressType(DocAddressType.SelfFiler);

			AssertNull(selfFiler);

			Factory.Save();

			selfFiler = consol.DocAddresses.FindByDocAddressType(DocAddressType.SelfFiler);

			AssertNotNull(selfFiler);
			AssertEquals(org2.PK, selfFiler.OrganisationPK);

			consol.Transports.RemoveAll();

			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NA!2#";

			consol.JK_CoLoadMasterBill = "MBL0001";

			Factory.Save();

			selfFiler = consol.DocAddresses.FindByDocAddressType(DocAddressType.SelfFiler);

			AssertNull(selfFiler);
		}

		public void TestIsICS2()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001001";
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";

			var transport = consol.Transports[0];
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = TransportModes.Sea;
			transport.JW_TransportType = TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_Vessel = "Dragon";
			transport.JW_VoyageFlight = "111";
			transport.JW_ETD = new ZDateTime(2018, 12, 1);

			Assert("AUSYD -> SGSIN - SEA", !consol.IsICS2);

			transport.JW_RL_NKLoadPort = "DE222";
			Assert("DE222 -> SGSIN - SEA", !consol.IsICS2);

			transport.JW_RL_NKDiscPort = "DEBRM";
			Assert("DE222 -> DEBRM - SEA", !consol.IsICS2);

			transport.JW_RL_NKLoadPort = "ESBCN";
			Assert("ESBCN -> DEBRM - SEA", !consol.IsICS2);

			transport.JW_TransportMode = TransportModes.Air;
			Assert("ESBCN -> DEBRM - Air", !consol.IsICS2);

			transport.JW_TransportMode = TransportModes.InlandWaterwayTransport;
			Assert("ESBCN -> DEBRM - IWT", !consol.IsICS2);

			transport.JW_RL_NKDiscPort = "ESALJ";
			Assert("ESBCN -> ESALJ - IWT", !consol.IsICS2);

			transport.JW_RL_NKLoadPort = "AUSYD";
			Assert("AUSYD -> ESALJ - IWT", consol.IsICS2);

			transport.JW_TransportMode = TransportModes.Sea;
			Assert("AUSYD -> ESALJ - Sea", consol.IsICS2);

			var ports = new string[] { "BGVAR", "HRSPU", "CYLMS", "DKCPH", "EETLL", "FIHEL", "FRMRS", "GBBEL", "NOOSL", "SKBTS", "CHBSL" };

			foreach (var loadPort in ports)
			{
				foreach (var dischargePort in ports)
				{
					if (loadPort == dischargePort)
					{
						continue;
					}

					transport.JW_TransportMode = TransportModes.Sea;
					transport.JW_RL_NKLoadPort = loadPort;
					transport.JW_RL_NKDiscPort = dischargePort;

					Assert($"{loadPort} -> {dischargePort} - Sea", !consol.IsICS2);

					transport.JW_TransportMode = TransportModes.InlandWaterwayTransport;
					transport.JW_RL_NKLoadPort = loadPort;
					transport.JW_RL_NKDiscPort = dischargePort;
					Assert($"{loadPort} -> {dischargePort} - IWT", !consol.IsICS2);
				}
			}

			foreach (var port in ports)
			{
				transport.JW_TransportMode = TransportModes.Sea;
				transport.JW_RL_NKLoadPort = "AUSYD";
				transport.JW_RL_NKDiscPort = port;
				Assert($"AUSYD -> {port} - SEA", consol.IsICS2);

				transport.JW_TransportMode = TransportModes.Sea;
				transport.JW_RL_NKLoadPort = port;
				transport.JW_RL_NKDiscPort = "AUSYD";
				Assert($"{port} -> AUSYD - SEA", !consol.IsICS2);
			}
		}

		public void TestIsSelfFiler()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "NO!2#";

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "org1";
			org1.OH_IsConsignee = true;
			org1.MiscServ.OM_IMAdvanceCargoReportingSelfFiler = true;

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "org2";
			org2.OH_IsForwarder = true;
			org2.MiscServ.OM_FWAdvanceCargoReportingSelfFiler = true;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Direct;
			consol.JK_RL_NKDischargePort = "NO!2#";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_OA_ReceivingForwarderAddress = org1.MainAddress.PK;

			var transport = consol.Transports[0];
			transport.JW_TransportMode = TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NO!2#";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;

			Factory.Save();

			shipment.ConsigneePK = org1.PK;
			Assert(consol.IsSelfFiler);

			org1.MiscServ.OM_IMAdvanceCargoReportingSelfFiler = false;
			Assert(!consol.IsSelfFiler);

			consol.JK_AgentType = AgentType.Agent;
			Assert(!consol.IsSelfFiler);

			var existingRelationShip = org1.AllRelatedParties.AddNew();
			existingRelationShip.PR_PartyType = RelatedPartyTypeList.Codes.SelfFilerForICS2;
			existingRelationShip.PR_OH_RelatedParty = org2.PK;
			existingRelationShip.PR_Location = unloco.RL_Code;
			existingRelationShip.PR_FreightTransportMode = TransportModes.Sea;

			Assert(consol.IsSelfFiler);
		}

		public void TestGetMatchedOrgRelatedPartyRecord()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "NO!2#";

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "org1";

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "org2";
			org2.OH_IsForwarder = true;
			org2.MiscServ.OM_FWAdvanceCargoReportingSelfFiler = true;

			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "org3";
			org3.OH_IsForwarder = true;
			org3.MiscServ.OM_FWAdvanceCargoReportingSelfFiler = true;

			var org4 = Factory.New<OrgHeader>();
			org4.OH_Code = "org4";
			org4.OH_IsForwarder = true;
			org4.MiscServ.OM_FWAdvanceCargoReportingSelfFiler = true;

			var existingRelationShip = org1.AllRelatedParties.AddNew();
			existingRelationShip.PR_PartyType = RelatedPartyTypeList.Codes.SelfFilerForICS2;
			existingRelationShip.PR_OH_RelatedParty = org2.PK;
			existingRelationShip.PR_Location = unloco.RL_Code;
			existingRelationShip.PR_FreightTransportMode = TransportModes.Sea;

			var existingRelationShip2 = org1.AllRelatedParties.AddNew();
			existingRelationShip2.PR_PartyType = RelatedPartyTypeList.Codes.SelfFilerForICS2;
			existingRelationShip2.PR_OH_RelatedParty = org3.PK;
			existingRelationShip2.PR_Location = unloco.RL_Code;
			existingRelationShip2.PR_FreightTransportMode = TransportModes.All;

			var existingRelationShipWithOtherPartyType = org1.AllRelatedParties.AddNew();
			existingRelationShipWithOtherPartyType.PR_PartyType = RelatedPartyTypeList.Codes.NotifyParty;
			existingRelationShipWithOtherPartyType.PR_OH_RelatedParty = org4.PK;
			existingRelationShipWithOtherPartyType.PR_Location = unloco.RL_Code;
			existingRelationShipWithOtherPartyType.PR_FreightTransportMode = TransportModes.Sea;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_RL_NKDischargePort = "NO!2#";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_OA_ReceivingForwarderAddress = org1.MainAddress.PK;

			var transport = consol.Transports[0];
			transport.JW_TransportMode = TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NO!2#";

			Factory.Save();

			var relatedPartyType = consol.GetMatchedOrgRelatedPartyRecord(org1);
			AssertNotNull(relatedPartyType);
			AssertEquals(org2.PK, relatedPartyType.PR_OH_RelatedParty);
		}

		public void TestSetShipment_JS_OH_HandledOnBehalfOfForwarder_FromReceivingForwarder()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "aaa";
			org1.OH_FullName = "bbb";
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "ccc";
			org2.OH_FullName = "ddd";
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_Code = "eee";
			org3.OH_FullName = "fff";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_IsCFS = true;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_IsCFSRegistered = true;

			consol.JK_OA_ReceivingForwarderAddress = org1.MainAddress.PK;

			Factory.Save();

			AssertEquals("shipment HandledOnBehalfOfForwarder and Receiving agent are same", shipment.HandledOnBehalfOfForwarder.MainAddress.PK, consol.JK_OA_ReceivingForwarderAddress);
			AssertEquals("JS_OH_HandledOnBehalfOfForwarder", shipment.JS_OH_HandledOnBehalfOfForwarder, org1.PK);

			shipment.JS_IsCFSRegistered = true;
			consol.JK_OA_ReceivingForwarderAddress = org2.MainAddress.PK;
			Factory.Save();

			AssertEquals("shipment HandledOnBehalfOfForwarder and Receiving agent are same", shipment.HandledOnBehalfOfForwarder.MainAddress.PK, consol.JK_OA_ReceivingForwarderAddress);
			AssertEquals("JS_OH_HandledOnBehalfOfForwarder", shipment.JS_OH_HandledOnBehalfOfForwarder, org2.PK);

			shipment.JS_OH_HandledOnBehalfOfForwarder = org3.PK;
			Factory.Save();

			AssertNotEquals("shipment HandledOnBehalfOfForwarder and Receiving agent are not same", shipment.HandledOnBehalfOfForwarder.MainAddress.PK, consol.JK_OA_ReceivingForwarderAddress);
			AssertEquals("JS_OH_HandledOnBehalfOfForwarder", shipment.JS_OH_HandledOnBehalfOfForwarder, org3.PK);
		}

		public void TestSetShipment_JS_OH_HandledOnBehalfOfForwarder_FromSendingForwarder()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "aaa";
			org1.OH_FullName = "bbb";
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "ccc";
			org2.OH_FullName = "ddd";
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_Code = "eee";
			org3.OH_FullName = "fff";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_RL_NKLoadPort = "AUSYD";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_IsCFSRegistered = true;

			consol.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;

			Factory.Save();

			AssertEquals("shipment HandledOnBehalfOfForwarder and Sending agent are same", shipment.HandledOnBehalfOfForwarder.MainAddress.PK, consol.JK_OA_SendingForwarderAddress);
			AssertEquals("JS_OH_HandledOnBehalfOfForwarder", shipment.JS_OH_HandledOnBehalfOfForwarder, org1.PK);

			shipment.JS_IsCFSRegistered = true;
			consol.JK_OA_SendingForwarderAddress = org2.MainAddress.PK;

			AssertEquals("shipment HandledOnBehalfOfForwarder and Sending agent are same", shipment.HandledOnBehalfOfForwarder.MainAddress.PK, consol.JK_OA_SendingForwarderAddress);
			AssertEquals("JS_OH_HandledOnBehalfOfForwarder", shipment.JS_OH_HandledOnBehalfOfForwarder, org2.PK);

			shipment.JS_OH_HandledOnBehalfOfForwarder = org3.PK;
			Factory.Save();

			AssertNotEquals("shipment HandledOnBehalfOfForwarder and Sending agent are not same", shipment.HandledOnBehalfOfForwarder.MainAddress.PK, consol.JK_OA_SendingForwarderAddress);
			AssertEquals("JS_OH_HandledOnBehalfOfForwarder", shipment.JS_OH_HandledOnBehalfOfForwarder, org3.PK);
		}

		public void TestSetShipment_JS_OH_HandledOnBehalfOfForwarder_LoadPortChanged()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "aaa";
			org1.OH_FullName = "bbb";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_IsCFSRegistered = true;

			Factory.Save();

			AssertEquals("shipment HandledOnBehalfOfForwarder and Sending agent are same", shipment.HandledOnBehalfOfForwarder.MainAddress.PK, consol.JK_OA_SendingForwarderAddress);
			AssertEquals("JS_OH_HandledOnBehalfOfForwarder", shipment.JS_OH_HandledOnBehalfOfForwarder, org1.PK);

			shipment.JS_IsCFSRegistered = true;
			consol.JK_RL_NKLoadPort = "USCHI";
			Factory.Save();

			AssertEquals("JS_OH_HandledOnBehalfOfForwarder", shipment.JS_OH_HandledOnBehalfOfForwarder, ZGuid.Empty);

			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;
			Factory.Save();

			AssertEquals("shipment HandledOnBehalfOfForwarder and Sending agent are same", shipment.HandledOnBehalfOfForwarder.MainAddress.PK, consol.JK_OA_SendingForwarderAddress);
			AssertEquals("JS_OH_HandledOnBehalfOfForwarder", shipment.JS_OH_HandledOnBehalfOfForwarder, org1.PK);
		}

		public void TestSetShipment_JS_OH_HandledOnBehalfOfForwarder_DischargePortChanged()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "aaa";
			org1.OH_FullName = "bbb";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_OA_ReceivingForwarderAddress = org1.MainAddress.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_IsCFSRegistered = true;

			Factory.Save();

			AssertEquals("shipment HandledOnBehalfOfForwarder and Receiving agent are same", shipment.HandledOnBehalfOfForwarder.MainAddress.PK, consol.JK_OA_ReceivingForwarderAddress);
			AssertEquals("JS_OH_HandledOnBehalfOfForwarder", shipment.JS_OH_HandledOnBehalfOfForwarder, org1.PK);

			shipment.JS_IsCFSRegistered = true;
			consol.JK_RL_NKDischargePort = "USCHI";
			Factory.Save();

			AssertEquals("JS_OH_HandledOnBehalfOfForwarder", shipment.JS_OH_HandledOnBehalfOfForwarder, ZGuid.Empty);

			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_ReceivingForwarderAddress = org1.MainAddress.PK;
			Factory.Save();

			AssertEquals("shipment HandledOnBehalfOfForwarder and Receiving agent are same", shipment.HandledOnBehalfOfForwarder.MainAddress.PK, consol.JK_OA_ReceivingForwarderAddress);
			AssertEquals("JS_OH_HandledOnBehalfOfForwarder", shipment.JS_OH_HandledOnBehalfOfForwarder, org1.PK);
		}

		#endregion

		#region IJobInvoicingPlugIn

		public void TestIJobInvoicingPlugIn()
		{
			AssertEquals("Expected InvoicingSupporter", typeof(ForwardingConsolInvoicingSupporter), Consol.InvoicingSupporter.GetType());
		}

		public void TestGetOperationsSignificantDate()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Transport transport1 = consol.Transports[0];
			Transport transport2 = consol.Transports.AddNew();

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "SGSIN";
			transport1.JW_ATD = new ZDateTime(2006, 5, 1);
			transport1.JW_ETD = new ZDateTime(2006, 5, 2);
			transport1.JW_ATA = new ZDateTime(2006, 5, 7);
			transport1.JW_ETA = new ZDateTime(2006, 5, 16);

			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "USLAX";
			transport2.JW_ATD = new ZDateTime(2006, 5, 7);
			transport2.JW_ATA = new ZDateTime(2006, 5, 15);
			transport2.JW_ETD = new ZDateTime(2006, 5, 14);
			transport2.JW_ETA = new ZDateTime(2006, 5, 22);

			AssertEquals(new ZDateTime(2006, 5, 7), consol.InvoicingSupporter.GetOperationsSignificantDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate));

			transport1.JW_ATA = ZDateTime.Empty;
			AssertEquals(new ZDateTime(2006, 5, 16), consol.InvoicingSupporter.GetOperationsSignificantDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate));

			transport1.JW_ETA = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, consol.InvoicingSupporter.GetOperationsSignificantDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate));

			AssertEquals(new ZDateTime(2006, 5, 1), consol.InvoicingSupporter.GetOperationsSignificantDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate));

			transport1.JW_ATD = ZDateTime.Empty;
			AssertEquals(new ZDateTime(2006, 5, 2), consol.InvoicingSupporter.GetOperationsSignificantDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate));

			transport1.JW_ETD = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, consol.InvoicingSupporter.GetOperationsSignificantDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate));
		}

		#endregion

		#region IScreeningPartyProvider

		public void TestScreeningPartiesVesselWhenTransportModeIsSeaOrInlandWaterwayTransport()
		{
			var consol = Factory.New<ForwardingConsol>();
			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Rail;
			transport.JW_Vessel = "RAIL";
			transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Road;
			transport.JW_Vessel = "ROAD";
			transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_Vessel = "AIR";
			transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Storage;
			transport.JW_Vessel = "STORAGE";
			transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_Vessel = "MAERSK2";
			transport.JW_IsLinked = true;
			transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_Vessel = "MAERSK1";
			transport.JW_IsLinked = false;
			transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			transport.JW_Vessel = "MAERSK2";
			transport.JW_IsLinked = true;
			transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			transport.JW_Vessel = "MAERSK1";
			transport.JW_IsLinked = false;

			ScreeningParty[] parties = (consol as IScreeningPartyProvider).ScreeningParties;

			AssertEquals(4, parties.Count(p => p.Description == "Vessel"));
		}

		public void TestUpdateStatusFromIWTAndSEAVessel()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_ScreeningStatus = "CLR";
			Factory.Save();
			AssertEquals("CLR", consol.JK_ScreeningStatus);

			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = TransportModes.Sea;
			transport.JW_IsLinked = false;
			transport.JW_Vessel = "DEF";
			transport.JW_VesselScreeningStatus = "NOT";
			Factory.Save();
			consol.UpdateStatusFromScreeningParties(true);
			AssertEquals("NOT", consol.JK_ScreeningStatus);

			transport = consol.Transports.AddNew();
			transport.JW_TransportMode = TransportModes.InlandWaterwayTransport;
			transport.JW_IsLinked = false;
			transport.JW_Vessel = "DEF2";
			transport.JW_VesselScreeningStatus = "MAT";
			Factory.Save();
			consol.UpdateStatusFromScreeningParties(true);
			AssertEquals("MAT", consol.JK_ScreeningStatus);
		}

		public void TestScreeningPartiesMergeDuplicatesIfSameVesselOnUnlinkedTransports_AllUnlinkedWithSameVessel()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "TestVessel1";
			var transport1 = consol.Transports.AddNew();
			transport1.JW_TransportMode = TransportModes.Sea;
			transport1.JW_Vessel = "TestVessel1";
			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = TransportModes.Sea;
			transport2.JW_Vessel = "TestVessel1";

			Factory.Save();

			var parties = new[]
			{
				new ScreeningParty(transport1, string.Empty, transport1),
				new ScreeningParty(transport2, string.Empty, transport2),
			};
			var result = DeniedPartyScreenerAsync.MergeDuplicateParties(parties);
			AssertEquals("Only one unlinked transport should be screened", 1, result.Length);
		}
		public void TestScreeningPartiesMergeDuplicatesIfSameVesselOnUnlinkedTransports_AllUnlinkedWithDifferentVessel()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "TestVessel1";
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Name = "TestVessel2";
			var transport1 = consol.Transports.AddNew();
			transport1.JW_TransportMode = TransportModes.Sea;
			transport1.JW_Vessel = "TestVessel1";
			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = TransportModes.Sea;
			transport2.JW_Vessel = "TestVessel2";

			Factory.Save();

			var parties = new[]
			{
				new ScreeningParty(transport1, string.Empty, transport1),
				new ScreeningParty(transport2, string.Empty, transport2),
			};
			var result = DeniedPartyScreenerAsync.MergeDuplicateParties(parties);
			AssertEquals("Both transports should be screened", 2, result.Length);
		}
		public void TestScreeningPartiesMergeDuplicatesIfSameVesselOnUnlinkedTransports_AllLinkedWithSameVessel()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "TestVessel1";
			var transport1 = consol.Transports.AddNew();
			transport1.JW_TransportMode = TransportModes.Sea;
			transport1.JW_Vessel = "TestVessel1";
			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = TransportModes.Sea;
			transport2.JW_Vessel = "TestVessel1";

			Factory.Save();

			transport1.JW_IsLinked = true;
			transport2.JW_IsLinked = true;
			var parties = new[]
			{
				new ScreeningParty(transport1, string.Empty, transport1),
				new ScreeningParty(transport2, string.Empty, transport2),
			};
			var result = DeniedPartyScreenerAsync.MergeDuplicateParties(parties);
			AssertEquals("Merged as normal with linked transports", 1, result.Length);
		}
		public void TestScreeningPartiesMergeDuplicatesIfSameVesselOnUnlinkedTransports_AllLinkedWithDifferentVessel()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "TestVessel1";
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Name = "TestVessel2";
			var transport1 = consol.Transports.AddNew();
			transport1.JW_TransportMode = TransportModes.Sea;
			transport1.JW_Vessel = "TestVessel1";
			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = TransportModes.Sea;
			transport2.JW_Vessel = "TestVessel2";

			Factory.Save();

			transport1.JW_IsLinked = true;
			transport2.JW_IsLinked = true;
			var parties = new[]
			{
				new ScreeningParty(transport1, string.Empty, transport1),
				new ScreeningParty(transport2, string.Empty, transport2),
			};
			var result = DeniedPartyScreenerAsync.MergeDuplicateParties(parties);
			AssertEquals("Both transports should be screened", 2, result.Length);
		}
		public void TestScreeningPartiesMergeDuplicatesIfSameVesselOnUnlinkedTransports_Integration()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "TestVessel1";
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Name = "TestVessel2";
			var transport1 = consol.Transports.AddNew();
			transport1.JW_TransportMode = TransportModes.Sea;
			transport1.JW_Vessel = "TestVessel1";
			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = TransportModes.Sea;
			transport2.JW_Vessel = "TestVessel1";
			var transport3 = consol.Transports.AddNew();
			transport3.JW_TransportMode = TransportModes.Sea;
			transport3.JW_Vessel = "TestVessel1";
			var transport4 = consol.Transports.AddNew();
			transport4.JW_TransportMode = TransportModes.Sea;
			transport4.JW_Vessel = "TestVessel2";

			Factory.Save();

			transport3.JW_IsLinked = true;
			var parties = new[]
			{
				new ScreeningParty(transport1, string.Empty, transport1),
				new ScreeningParty(transport2, string.Empty, transport2),
				new ScreeningParty(transport3, string.Empty, transport3),
				new ScreeningParty(transport4, string.Empty, transport4),
			};
			var result = DeniedPartyScreenerAsync.MergeDuplicateParties(parties);
			AssertEquals("Transport 1, 2 and 3 should be merged, 4 should not be merged", 2, result.Length);
		}

		public void TestScreeningPartiesTransportWithoutVesselCode()
		{
			var consol = Factory.New<ForwardingConsol>();
			var transport = consol.Transports.AddNew();
			transport.JW_IsLinked = false;
			var parties = (consol as IScreeningPartyProvider).ScreeningParties;
			AssertEquals(false, parties.Any(o => o.NotLinkedVessel != null && (o.NotLinkedVessel as Transport).PK == transport.PK));
		}

		public void TestScreeningParties_JW_IsLinked_False()
		{
			var consol = Factory.New<ForwardingConsol>();
			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_IsLinked = false;
			transport.JW_Vessel = "Titanic";

			var parties = (consol as IScreeningPartyProvider).ScreeningParties;
			AssertEquals(true, parties.Any(o => o.NotLinkedVessel != null && (o.NotLinkedVessel as Transport).PK == transport.PK));
		}

		public void TestIScreeningPartyProvider()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "UAIEV";
			consol.JK_RL_NKDischargePort = "AUSYD";
			AssertNotNull(consol);

			OrgHeader localClient = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader exportBroker = Factory.NewWithValidTestData<OrgHeader>();

			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_ParentID = consol.PK;
			job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
			job.JH_JobNum = "ABC123";

			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Titanic";
			vessel.RV_OH = ShippingCompany2.PK;

			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "UAIEV";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_OH_ExportBroker = exportBroker.PK;

			Factory.Save();

			consol.Transports[0].JW_TransportMode = Core.Constants.TransportModes.Sea;
			consol.Transports[0].JW_Vessel = vessel.RV_FK;
			consol.Transports[0].JW_IsLinked = true;
			consol.JK_OA_SendingForwarderAddress = LocalConsignor.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = OverseasConsignee.MainAddress.PK;
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			consol.JK_OA_CreditorAddress = LocalForwarder.MainAddress.PK;

			consol.JK_OA_DepartureCTOAddress = LocalCTO.MainAddress.PK;
			consol.JK_OA_PackDepotAddress = LocalDepot.MainAddress.PK;
			consol.JK_OA_ContainerYardEmptyPickupAddress = LocalContainerYard.MainAddress.PK;

			consol.JK_OA_ArrivalCTOAddress = OverseasCTO.MainAddress.PK;
			consol.JK_OA_UnpackDepotAddress = OverseasDepot.MainAddress.PK;
			consol.JK_OA_ContainerYardEmptyReturnAddress = OverseasContainerYard.MainAddress.PK;

			ScreeningParty[] deniedCandidates = ((IScreeningPartyProvider)consol).ScreeningParties;
			AssertContainsDeniedCandidate("Local Client", localClient, deniedCandidates);

			AssertContainsDeniedCandidate("Sending Agent", LocalConsignor, deniedCandidates);
			AssertContainsDeniedCandidate("Receiving Agent", OverseasConsignee, deniedCandidates);
			AssertContainsDeniedCandidate("Carrier", ShippingCompany1, deniedCandidates);
			AssertContainsDeniedCandidate("Creditor", LocalForwarder, deniedCandidates);

			AssertContainsDeniedCandidate("Departure CTO", LocalCTO, deniedCandidates);
			AssertContainsDeniedCandidate("Pack Depot", LocalDepot, deniedCandidates);
			AssertContainsDeniedCandidate("Pickup Container Yard", LocalContainerYard, deniedCandidates);

			AssertContainsDeniedCandidate("Arrival CTO", OverseasCTO, deniedCandidates);
			AssertContainsDeniedCandidate("Unpack Depot", OverseasDepot, deniedCandidates);
			AssertContainsDeniedCandidate("Return Container Yard", OverseasContainerYard, deniedCandidates);

			AssertNotNull("Contains denied candidates from vessels", Array.Find(deniedCandidates, x => x.Header == ShippingCompany2));
			AssertNotNull("Contains denied candidates from shipments", Array.Find(deniedCandidates, x => x.Header == exportBroker));
		}

		public void TestNotLinkedVesselInheritVesselScreeningStatusAsPerVesselCode()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "ABC";
			vessel.RV_ScreeningStatus = "CLR";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = TransportModes.Sea;
			transport.JW_IsLinked = false;
			transport.JW_Vessel = "DEF";
			transport.JW_VesselScreeningStatus = "NOT";
			Factory.Save();

			bool ExistsTransportParty(ScreeningParty party) => party.NotLinkedVessel != null && (party.NotLinkedVessel as Transport).PK == transport.PK;
			var consolAsScreeningPartyProvider = consol as IScreeningPartyProvider;

			AssertEquals("Precondition 1: Not linked vessel.", false, transport.JW_IsLinked);
			AssertNull("Precondition 2: No identical vessel code in database.", Factory.LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, transport.JW_Vessel));
			var transportParty = consolAsScreeningPartyProvider.ScreeningParties.Single(ExistsTransportParty);
			AssertEquals("NOT", transportParty.CurrentScreeningStatus);
			AssertEquals("NOT", consol.JK_ScreeningStatus);

			transport.JW_Vessel = "ABC";
			Factory.Save();
			consol.UpdateStatusFromScreeningParties(true);

			AssertEquals("Precondition 1: Not linked vessel.", false, transport.JW_IsLinked);
			AssertEquals("Precondition 2: The vessel is in database and is clear.", "CLR", Factory.LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, transport.JW_Vessel).RV_ScreeningStatus);
			Assert(!consolAsScreeningPartyProvider.ScreeningParties.Any(ExistsTransportParty));
			AssertEquals("CLR", consol.JK_ScreeningStatus);

			vessel.RV_ScreeningStatus = "MAT";
			Factory.Save();
			consol.UpdateStatusFromScreeningParties(true);

			AssertEquals("Precondition 1: Not linked vessel.", false, transport.JW_IsLinked);
			AssertEquals("Precondition 2: The vessel is in database and is matched.", "MAT", Factory.LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, transport.JW_Vessel).RV_ScreeningStatus);
			Assert(!consolAsScreeningPartyProvider.ScreeningParties.Any(ExistsTransportParty));
			AssertEquals("MAT", consol.JK_ScreeningStatus);
		}

		void AssertContainsDeniedCandidate(string description, OrgHeader orgHeader, ScreeningParty[] deniedCandidates)
		{
			AssertEquals(description, orgHeader, Array.Find(deniedCandidates, x => x.Description == description).Header);
		}

		#endregion

		public void TestIGateway()
		{
			var consol = Factory.New<ForwardingConsol>();
			var gateway = (IGateway)consol;
			var costSell = new CostSell();
			AssertType<ForwardingConsolGatewayBillingSupporter>(gateway.GatewayBillingSupporter);

			var consolRatingRoute = new ConsolRatingRoute(consol);
			var consolRatingAdapter = new ForwardingConsolRatingAdapter(consolRatingRoute);
			AssertEquals(((IGateway)consolRatingAdapter.Parent).GatewayBillingSupporter, gateway.GatewayBillingSupporter);

			AssertContainsExactElementsInAnyOrder(new List<ZGuid>(), gateway.SortedGatewayAgentPKs);
			AssertContainsExactElementsInAnyOrder(new List<ZGuid>(), gateway.GatewayAgentPKsForIntercompanyTariff);
			AssertContainsExactElementsInAnyOrder(new List<ILocation>(), gateway.SortedOverridenPlannedLoad);
			AssertContainsExactElementsInAnyOrder(new List<ILocation>(), gateway.SortedOverridenPlannedDischarge);
			AssertContainsExactElementsInAnyOrder(new List<ZGuid>(), gateway.SortedControllingCustomerPKs);

			var billingType = new BillingType();
			var agentType = "test";
			var gatewayAgentPk = new ZGuid();

			AssertEquals(false, gateway.IsIntercompanyTariffApplicable(billingType, costSell));
			AssertEquals(true, gateway.ContinueWithDefaultCosting(billingType));
			AssertEquals(ZString.Empty, gateway.GatewayAgentTypeFilteredReason(agentType, gatewayAgentPk, billingType, costSell));

			AssertEquals(false, consol.ShouldRemoveNonIntercompanyTariffFRTEntries(billingType));
		}

		#region IRelatedOrgDeniedPartyScreenable

		public void TestIRelatedOrgDeniedPartyScreenable()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "UAIEV";
			consol.JK_RL_NKDischargePort = "AUSYD";
			AssertNotNull(consol);

			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			var exportBroker = Factory.NewWithValidTestData<OrgHeader>();

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_ParentID = consol.PK;
			job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
			job.JH_JobNum = "ABC123";

			consol.Transports[0].JW_TransportMode = Core.Constants.TransportModes.Sea;
			consol.Transports[0].JW_Vessel = "ABC";
			consol.Transports[0].CarrierPK = ShippingCompany2.PK;
			consol.Transports[0].JW_IsLinked = false;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "UAIEV";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_OH_ExportBroker = exportBroker.PK;

			var status1 = Factory.New<IRelatedOrgPartyScreeningStatus>();
			status1.PJ_ParentID = localClient.PK;
			status1.PJ_ParentTableCode = OrgHeaderSchema.Constants.Prefix;

			var status2 = Factory.New<IRelatedOrgPartyScreeningStatus>();
			status2.PJ_ParentID = ShippingCompany2.PK;
			status2.PJ_ParentTableCode = OrgHeaderSchema.Constants.Prefix;

			var status3 = Factory.New<IRelatedOrgPartyScreeningStatus>();
			status3.PJ_ParentID = exportBroker.PK;
			status3.PJ_ParentTableCode = OrgHeaderSchema.Constants.Prefix;

			var status4 = Factory.New<IRelatedOrgPartyScreeningStatus>();
			status4.PJ_ParentID = consol.PK;
			status4.PJ_ParentTableCode = JobConsolSchema.Constants.Prefix;

			var relatedScreeningStatus = consol.RelatedOrgPartyScreeningStatusCollection.ToList<IRelatedOrgPartyScreeningStatus>();

			CombineAssertions(() =>
			{
				AssertEquals(4, relatedScreeningStatus.Count);
				AssertEquals($"{localClient.OH_Code}(Local Client)", relatedScreeningStatus.Single(u => u.PJ_ParentID == localClient.PK && u.PK == status1.PK).RelatedOrganization);
				AssertEquals($"{ShippingCompany2.OH_Code}(Carrier)", relatedScreeningStatus.Single(u => u.PJ_ParentID == ShippingCompany2.PK && u.PK == status2.PK).RelatedOrganization);
				AssertEquals($"{exportBroker.OH_Code}(Export Broker)", relatedScreeningStatus.Single(u => u.PJ_ParentID == exportBroker.PK && u.PK == status3.PK).RelatedOrganization);
				AssertEquals(true, relatedScreeningStatus.Any(u => u.PJ_ParentID == consol.PK && u.PK == status4.PK));

				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_OH = ShippingCompany2.PK;
				consol.Transports[0].JW_Vessel = vessel.RV_FK;
				relatedScreeningStatus = consol.RelatedOrgPartyScreeningStatusCollection.ToList<IRelatedOrgPartyScreeningStatus>();
				AssertEquals($"{ShippingCompany2.OH_Code}(Carrier|Shipping Provider)", relatedScreeningStatus.Single(u => u.PJ_ParentID == ShippingCompany2.PK && u.PK == status2.PK).RelatedOrganization);

				consol.Transports[0].JW_IsLinked = true;
				relatedScreeningStatus = consol.RelatedOrgPartyScreeningStatusCollection.ToList<IRelatedOrgPartyScreeningStatus>();
				AssertEquals($"{ShippingCompany2.OH_Code}(Carrier|Shipping Provider)", relatedScreeningStatus.Single(u => u.PJ_ParentID == ShippingCompany2.PK && u.PK == status2.PK).RelatedOrganization);
			});
		}

		public void TestRelatedOrgPartyScreeningStatusCollectionWithSubshipments()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "UAIEV";
			consol.JK_RL_NKDischargePort = "AUSYD";
			AssertNotNull(consol);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "UAIEV";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_ShipmentType = ShipmentTypes.CoLoadMaster;

			var shipmentRelatedShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipmentRelatedShipment.JS_JS_ColoadMasterShipment = shipment.PK;

			var status1 = Factory.New<IRelatedOrgPartyScreeningStatus>();
			status1.PJ_ParentID = shipment.PK;
			status1.PJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var status2 = Factory.New<IRelatedOrgPartyScreeningStatus>();
			status2.PJ_ParentID = shipmentRelatedShipment.PK;
			status2.PJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var status3 = Factory.New<IRelatedOrgPartyScreeningStatus>();
			status3.PJ_ParentID = consol.PK;
			status3.PJ_ParentTableCode = JobConsolSchema.Constants.Prefix;

			var relatedScreeningStatus = consol.RelatedOrgPartyScreeningStatusCollection.ToList<IRelatedOrgPartyScreeningStatus>();

			CombineAssertions(() =>
			{
				AssertEquals(3, relatedScreeningStatus.Count);
				AssertEquals(true, relatedScreeningStatus.Any(u => u.PJ_ParentID == shipment.PK && u.PK == status1.PK));
				AssertEquals(true, relatedScreeningStatus.Any(u => u.PJ_ParentID == shipmentRelatedShipment.PK && u.PK == status2.PK));
				AssertEquals(true, relatedScreeningStatus.Any(u => u.PJ_ParentID == consol.PK && u.PK == status3.PK));
			});
		}

		#endregion

		#region ScreeningStatus

		public void TestScreeningStatusUpdatedWithReplacedParty()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var docAddressOrg = Factory.NewWithValidTestData<OrgHeader>();
			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			var departureCTO = Factory.NewWithValidTestData<OrgHeader>();
			var packDepot = Factory.NewWithValidTestData<OrgHeader>();
			var emptyPickup = Factory.NewWithValidTestData<OrgHeader>();
			var arrivalCTO = Factory.NewWithValidTestData<OrgHeader>();
			var unpackDepot = Factory.NewWithValidTestData<OrgHeader>();
			var emptyReturn = Factory.NewWithValidTestData<OrgHeader>();
			var vessel = Factory.NewWithValidTestData<RefVessel>();

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_AddressOverride = false;
			docAddress.E2_ParentID = consol.PK;
			docAddress.E2_ParentTableCode = "JK";
			docAddress.E2_OA_Address = docAddressOrg.MainAddress.PK;

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_ParentID = consol.PK;
			job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
			job.JH_JobNum = "ABC123";
			job.Parent = consol;

			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			consol.JK_OA_ShippingLineAddress = shippingLine.MainAddress.PK;
			consol.JK_OA_DepartureCTOAddress = departureCTO.MainAddress.PK;
			consol.JK_OA_PackDepotAddress = packDepot.MainAddress.PK;
			consol.JK_OA_ContainerYardEmptyPickupAddress = emptyPickup.MainAddress.PK;
			consol.JK_OA_ArrivalCTOAddress = arrivalCTO.MainAddress.PK;
			consol.JK_OA_UnpackDepotAddress = unpackDepot.MainAddress.PK;
			consol.JK_OA_ContainerYardEmptyReturnAddress = emptyReturn.MainAddress.PK;

			var transport = Factory.New<Transport>();
			consol.Transports.Add(transport);
			transport.JW_Vessel = vessel.RV_FK;

			Factory.Save();
			AssertEquals("Consol's Screening Status", ScreeningStatusesList.Codes.NotScreened, consol.JK_ScreeningStatus);

			var docAddressOrgNew = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			docAddressOrgNew.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			docAddress.E2_OA_Address = docAddressOrgNew.MainAddress.PK;
			Factory.Save();
			AssertEquals("Consol's Screening Status", ScreeningStatusesList.Codes.Matched, consol.JK_ScreeningStatus);
			docAddress.E2_OA_Address = docAddressOrg.MainAddress.PK;
			Factory.Save();
			AssertEquals("Consol's Screening Status", ScreeningStatusesList.Codes.NotScreened, consol.JK_ScreeningStatus);

			var localClientNew = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			localClientNew.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			job.JH_OA_LocalChargesAddr = localClientNew.MainAddress.PK;
			Factory.Save();
			AssertEquals("Consol's Screening Status", ScreeningStatusesList.Codes.Matched, consol.JK_ScreeningStatus);
			job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
			Factory.Save();
			AssertEquals("Consol's Screening Status", ScreeningStatusesList.Codes.NotScreened, consol.JK_ScreeningStatus);

			var creditorNew = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			creditorNew.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			consol.JK_OA_CreditorAddress = creditorNew.MainAddress.PK;
			Factory.Save();
			AssertEquals("Consol's Screening Status", ScreeningStatusesList.Codes.Matched, consol.JK_ScreeningStatus);
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			Factory.Save();
			AssertEquals("Consol's Screening Status", ScreeningStatusesList.Codes.NotScreened, consol.JK_ScreeningStatus);

			var sendingForwarderNew = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			sendingForwarderNew.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			consol.JK_OA_SendingForwarderAddress = sendingForwarderNew.MainAddress.PK;
			Factory.Save();
			AssertEquals("Consol's Screening Status", ScreeningStatusesList.Codes.Matched, consol.JK_ScreeningStatus);
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			Factory.Save();
			AssertEquals("Consol's Screening Status", ScreeningStatusesList.Codes.NotScreened, consol.JK_ScreeningStatus);

			var receivingForwarderNew = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			receivingForwarderNew.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarderNew.MainAddress.PK;
			Factory.Save();
			AssertEquals("Consol's Screening Status", ScreeningStatusesList.Codes.Matched, consol.JK_ScreeningStatus);
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			Factory.Save();
			AssertEquals("Consol's Screening Status", ScreeningStatusesList.Codes.NotScreened, consol.JK_ScreeningStatus);

			var shippingLineNew = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			shippingLineNew.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			consol.JK_OA_ShippingLineAddress = shippingLineNew.MainAddress.PK;
			Factory.Save();
			AssertEquals("Consol's Screening Status", ScreeningStatusesList.Codes.Matched, consol.JK_ScreeningStatus);
			consol.JK_OA_ShippingLineAddress = shippingLine.MainAddress.PK;
			Factory.Save();
			AssertEquals("Consol's Screening Status", ScreeningStatusesList.Codes.NotScreened, consol.JK_ScreeningStatus);

			var departureCTONew = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			departureCTONew.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			consol.JK_OA_DepartureCTOAddress = departureCTONew.MainAddress.PK;
			Factory.Save();
			AssertEquals("Consol's Screening Status", ScreeningStatusesList.Codes.Matched, consol.JK_ScreeningStatus);
			consol.JK_OA_DepartureCTOAddress = departureCTO.MainAddress.PK;
			Factory.Save();
			AssertEquals("Consol's Screening Status", ScreeningStatusesList.Codes.NotScreened, consol.JK_ScreeningStatus);

			var packDepotNew = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			packDepotNew.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			consol.JK_OA_PackDepotAddress = packDepotNew.MainAddress.PK;
			Factory.Save();
			AssertEquals("Consol's Screening Status", ScreeningStatusesList.Codes.Matched, consol.JK_ScreeningStatus);
			consol.JK_OA_PackDepotAddress = packDepot.MainAddress.PK;
			Factory.Save();
			AssertEquals("Consol's Screening Status", ScreeningStatusesList.Codes.NotScreened, consol.JK_ScreeningStatus);

			var emptyPickupNew = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			emptyPickupNew.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			consol.JK_OA_ContainerYardEmptyPickupAddress = emptyPickupNew.MainAddress.PK;
			Factory.Save();
			AssertEquals("Consol's Screening Status", ScreeningStatusesList.Codes.Matched, consol.JK_ScreeningStatus);
			consol.JK_OA_ContainerYardEmptyPickupAddress = emptyPickup.MainAddress.PK;
			Factory.Save();
			AssertEquals("Consol's Screening Status", ScreeningStatusesList.Codes.NotScreened, consol.JK_ScreeningStatus);

			var arrivalCTONew = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			arrivalCTONew.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			consol.JK_OA_ArrivalCTOAddress = arrivalCTONew.MainAddress.PK;
			Factory.Save();
			AssertEquals("Consol's Screening Status", ScreeningStatusesList.Codes.Matched, consol.JK_ScreeningStatus);
			consol.JK_OA_ArrivalCTOAddress = arrivalCTO.MainAddress.PK;
			Factory.Save();
			AssertEquals("Consol's Screening Status", ScreeningStatusesList.Codes.NotScreened, consol.JK_ScreeningStatus);

			var unpackDepotNew = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			unpackDepotNew.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			consol.JK_OA_UnpackDepotAddress = unpackDepotNew.MainAddress.PK;
			Factory.Save();
			AssertEquals("Consol's Screening Status", ScreeningStatusesList.Codes.Matched, consol.JK_ScreeningStatus);
			consol.JK_OA_UnpackDepotAddress = unpackDepot.MainAddress.PK;
			Factory.Save();
			AssertEquals("Consol's Screening Status", ScreeningStatusesList.Codes.NotScreened, consol.JK_ScreeningStatus);

			var emptyReturnNew = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			emptyReturnNew.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			consol.JK_OA_ContainerYardEmptyReturnAddress = emptyReturnNew.MainAddress.PK;
			Factory.Save();
			AssertEquals("Consol's Screening Status", ScreeningStatusesList.Codes.Matched, consol.JK_ScreeningStatus);
			consol.JK_OA_ContainerYardEmptyReturnAddress = emptyReturn.MainAddress.PK;
			Factory.Save();
			AssertEquals("Consol's Screening Status", ScreeningStatusesList.Codes.NotScreened, consol.JK_ScreeningStatus);

			var vesselNew = Factory.NewWithValidTestData<RefVessel>();
			consol.Transports[0].JW_TransportMode = TransportModes.Sea;
			Factory.Save();
			vesselNew.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			consol.Transports[0].JW_Vessel = vesselNew.RV_FK;
			consol.Transports[0].JW_IsLinked = true;
			Factory.Save();
			AssertEquals("Consol's Screening Status", ScreeningStatusesList.Codes.Matched, consol.JK_ScreeningStatus);
			consol.Transports[0].JW_Vessel = vessel.RV_FK;
			Factory.Save();
			AssertEquals("Consol's Screening Status", ScreeningStatusesList.Codes.NotScreened, consol.JK_ScreeningStatus);

			var shipmentExportBroker = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_OH_ExportBroker = shipmentExportBroker.PK;
			Factory.Save();
			shipmentExportBroker.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			consol.Shipments.Add(shipment);
			Factory.Save();
			AssertEquals("Consol's Screening Status", ScreeningStatusesList.Codes.Matched, consol.JK_ScreeningStatus);
			consol.Shipments.Remove(shipment);
			Factory.Save();
			AssertEquals("Consol's Screening Status", ScreeningStatusesList.Codes.NotScreened, consol.JK_ScreeningStatus);
		}

		public void TestSetShouldUpdateScreeningStatus()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			AssertSetShouldUpdateScreeningStatus((orgAddressPK) => { consol.JK_OA_CreditorAddress = orgAddressPK; });
			AssertSetShouldUpdateScreeningStatus((orgAddressPK) => { consol.JK_OA_SendingForwarderAddress = orgAddressPK; });
			AssertSetShouldUpdateScreeningStatus((orgAddressPK) => { consol.JK_OA_ShippingLineAddress = orgAddressPK; });
			AssertSetShouldUpdateScreeningStatus((orgAddressPK) => { consol.JK_OA_DepartureCTOAddress = orgAddressPK; });
			AssertSetShouldUpdateScreeningStatus((orgAddressPK) => { consol.JK_OA_PackDepotAddress = orgAddressPK; });
			AssertSetShouldUpdateScreeningStatus((orgAddressPK) => { consol.JK_OA_ContainerYardEmptyPickupAddress = orgAddressPK; });
			AssertSetShouldUpdateScreeningStatus((orgAddressPK) => { consol.JK_OA_ArrivalCTOAddress = orgAddressPK; });
			AssertSetShouldUpdateScreeningStatus((orgAddressPK) => { consol.JK_OA_UnpackDepotAddress = orgAddressPK; });
			AssertSetShouldUpdateScreeningStatus((orgAddressPK) => { consol.JK_OA_ContainerYardEmptyReturnAddress = orgAddressPK; });
			AssertSetShouldUpdateScreeningStatus((orgAddressPK) => { consol.JK_OA_ReceivingForwarderAddress = orgAddressPK; });

			void AssertSetShouldUpdateScreeningStatus(Action<ZGuid> setAddressValue)
			{
				consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
				((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus = false;
				AssertEquals("Precondition", false, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);

				orgAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				setAddressValue(orgAddress.PK);
				AssertEquals(false, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);

				setAddressValue(ZGuid.Empty);
				AssertEquals(false, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);

				orgAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
				setAddressValue(orgAddress.PK);
				AssertEquals(false, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);

				setAddressValue(ZGuid.Empty);
				AssertEquals(false, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);

				orgAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				setAddressValue(orgAddress.PK);
				AssertEquals(true, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);

				consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus = false;
				setAddressValue(ZGuid.Empty);
				AssertEquals(true, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);
			}
		}

		#endregion

		#region GetWorkflowInformationProvider

		public void TestGetWorkflowInformationProvider()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "UAIEV";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var company1 = Factory.New<GlbCompany>();
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_GC = company1.PK;

			var company2 = Factory.New<GlbCompany>();
			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_GC = company2.PK;

			company1.GC_OH_OrgProxy = LocalConsignor.PK;
			company2.GC_OH_OrgProxy = OverseasConsignee.PK;

			consol.JK_OA_SendingForwarderAddress = LocalConsignor.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = OverseasConsignee.MainAddress.PK;

			var workflowInformationProvider = (consol as IWorkflowProvider).GetWorkflowInformationProvider();

			AssertEquals("Origin", "Kiev", workflowInformationProvider.Origin);
			AssertEquals("Destination", "Sydney", workflowInformationProvider.Destination);
			AssertEquals("Business Context", TrackingConstants.BusinessContext.Consol, workflowInformationProvider.BusinessContext);
			AssertContainsExactElementsInAnyOrder("Companies", new[] { company1.PK, company2.PK }, workflowInformationProvider.Companies);
		}

		#endregion

		#region TestJK_ScreeningStatus

		public void TestJK_ScreeningStatus()
		{
			AssertEquals("JK_ScreeningStatus must be readonly", true, Consol.JK_ScreeningStatusInfo.ReadOnly);
		}

		public void TestScreeningStatusesList()
		{
			AssertEquals(typeof(ScreeningStatusesList), Consol.ScreeningStatusesList.GetType());
		}

		#endregion

		#region JK_ConsolChargeableRate

		public void TestJK_ConsolChargeableRate()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Precondition", 0m, consol.JK_ConsolChargeableRate);

			consol.OnOverridingChargeableRate += (object sender, CancelEventArgs e) => { Fail("Should not fire event for non-AIR consol"); };

			consol.JK_ConsolChargeableRate = 10m;
			AssertEquals("Chargeable was changed", 10m, consol.JK_ConsolChargeableRate);

			consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			consol.JK_ConsolChargeableRate = 10m;
			AssertEquals("Chargeable was changed", 10m, consol.JK_ConsolChargeableRate);

			BusinessObject consolCost = (BusinessObject)Factory.New<IJobConsolCost>();

			consolCost.SetContext(Enterprise.Integration.Accounting.BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				consolCost[JobConsolCostSchema.E6_ParentID] = consol.PK;
				consolCost[JobConsolCostSchema.E6_ParentTableCode] = "JK";
			}
			finally
			{
				consolCost.RemoveContext(Enterprise.Integration.Accounting.BusinessContext.EnableDirectSettingConsolCostParent);
			}

			consolCost[JobConsolCostSchema.E6_AC_ChargeCode] = Env.Registry.FreightChargeCode;
			consolCost[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;

			CalculationLogsWrapper logsWrapper = new CalculationLogsWrapper();
			AssertEquals("Precondition", false, logsWrapper.IsDisabled);
			CalculationLogsLoader.Save(consolCost, logsWrapper);

			consol.JK_ConsolChargeableRate = 20m;
			AssertEquals("Chargeable was changed", 20m, consol.JK_ConsolChargeableRate);

			logsWrapper = consol.AWBHeader.CalculationLogsAnalyzer.LogsWrapper;
			AssertEquals("Calculation logs were disabled", true, logsWrapper.IsDisabled);

			consol.OnOverridingChargeableRate += (object sender, CancelEventArgs e) => e.Cancel = true;

			logsWrapper.IsDisabled = false;
			AssertEquals("Precondition", false, logsWrapper.IsDisabled);

			consol.JK_ConsolChargeableRate = 30m;
			AssertEquals("Chargeable rate not changed, as changing was canceled", 20m, consol.JK_ConsolChargeableRate);
			AssertEquals("Calculation logs NOT disabled, as changing was canceled", false, logsWrapper.IsDisabled);
		}

		#endregion

		#region TestJK_Phase

		public void TestJK_Phase_Readonly()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Env.Security.ConsolPhaseSecurityOverride.IsAllowed = false;
			AssertEquals("Readonly, before save", true, consol.JK_PhaseInfo.ReadOnly);

			Factory.Save();
			AssertEquals("Readonly, after save", true, consol.JK_PhaseInfo.ReadOnly);

			Env.Security.ConsolPhaseSecurityOverride.IsAllowed = true;
			AssertEquals("Writeable if user have rights", false, consol.JK_PhaseInfo.ReadOnly);
		}

		public void TestJK_Phase_Lookups()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			CodeDescriptionPairList commonList = PhaseConstants.GetCommonPhaseList();
			AssertContainsExactElementsInAnyOrder(commonList, consol.Phases);

			PhaseSecurity security = new PhaseSecurity(PhaseConstants.GetConsolLocationsList());

			Phase phase1 = security.Phases.AddNew();
			phase1.Code = "AAA";
			phase1.Description = (NoResString)"Hello";

			Phase phase2 = security.Phases.AddNew();
			phase2.Code = "BBB";
			phase2.Description = (NoResString)"World";

			PhaseRule rule = phase1.Rules.AddNew();
			rule.Location = PhaseConstants.Locations.AnyLocation;
			rule.DepartmentPK = GlbDepartment.CurrentDepartment.PK;

			phase2.Rules.Add(rule);
			ForwardingConfigurationRegistry.Instance.ConsolPhaseSecurity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, security);

			CodeDescriptionPairList expectedList = PhaseConstants.GetCommonPhaseList();
			expectedList.AddPair("AAA", "Hello");
			expectedList.AddPair("BBB", "World");

			consol = new BusinessObjectFactory().New<ForwardingConsol>();
			AssertContainsExactElementsInAnyOrder(expectedList, consol.Phases);
		}

		#endregion

		#region TestJK_RL_NKMasterBillIssuePlace

		public void TestJK_RL_NKMasterBillIssuePlace()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			var carrier1 = Factory.New<OrgHeader>();
			carrier1.OH_FullName = "Test Carrier1";
			carrier1.OH_RL_NKClosestPort = "AUMEL";
			carrier1.MainAddress.OA_Address1 = "Test Carrier1 Address";

			var carrier2 = Factory.New<OrgHeader>();
			carrier2.OH_FullName = "Test Carrier2";
			carrier2.OH_RL_NKClosestPort = "AUSYD";
			carrier2.MainAddress.OA_Address1 = "Test Carrier2 Address";

			consol.JK_OA_ShippingLineAddress = carrier1.MainAddress.PK;
			AssertEquals("Should be default to Carrier1 Unloco", "AUMEL", consol.JK_RL_NKMasterBillIssuePlace);

			consol.JK_OA_ShippingLineAddress = carrier2.MainAddress.PK;
			AssertEquals("Should be default to Carrier2 Unloco", "AUSYD", consol.JK_RL_NKMasterBillIssuePlace);

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			AssertEquals("Should be blank", "", consol.JK_RL_NKMasterBillIssuePlace);
		}

		public void TestJK_RL_NKMasterBillIssuePlace_WithTransportMode()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "Test Carrier1";
			carrier.OH_RL_NKClosestPort = "AUMEL";
			carrier.MainAddress.OA_Address1 = "Test Carrier1 Address";
			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			AssertEquals("", consol.JK_RL_NKMasterBillIssuePlace);

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			AssertEquals("", consol.JK_RL_NKMasterBillIssuePlace);

			consol.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals("", consol.JK_RL_NKMasterBillIssuePlace);

			consol.JK_TransportMode = Constants.TransportModes.Rail;
			AssertEquals("", consol.JK_RL_NKMasterBillIssuePlace);

			consol.JK_TransportMode = Constants.TransportModes.Road;
			AssertEquals("", consol.JK_RL_NKMasterBillIssuePlace);

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("AUMEL", consol.JK_RL_NKMasterBillIssuePlace);
		}

		#endregion

		#region Test IBillDetails

		public void TestIBillDetails()
		{
			Consol.JK_MasterBillNum = "APLU500206";
			CusEntryNumber number = Consol.Numbers.AddNew();
			number.CE_EntryNum = "GTRE50060023";
			number.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS;
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "aaa";
			org1.OH_FullName = "bbb";
			Consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = org1.PK;
			var org2 = Factory.New<OrgHeader>();
			Consol.JK_OA_ShippingLineAddress = org2.MainAddress.PK;
			var org3 = Factory.New<OrgHeader>();
			Consol.JK_OA_CoLoadAddress = org3.MainAddress.PK;

			var billDetails = (IBillDetails)Consol;
			AssertEquals("APLU500206", billDetails.BillNumberInfo.Value);
			AssertEquals("GTRE50060023", billDetails.AMSBillNumberInfo.Value);
			AssertEquals(0, billDetails.GetNumberOfPackesInfos(null).Length);
			AssertEquals(0, billDetails.GetTypeOfPackesInfos(null).Length);
			AssertArrayEqualsByElements(new[] { org1, org2 }, billDetails.SCACIssuers.ToArray());
		}

		public void TestSCACIssuers()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;

			var org01 = Factory.New<OrgHeader>();
			var org02 = Factory.New<OrgHeader>();
			var org03 = Factory.New<OrgHeader>();

			org01.OH_FullName = "Org 01";
			org02.OH_FullName = "Org 02";
			org03.OH_FullName = "Org 03";

			org01.OH_Code = "ORG01";
			org02.OH_Code = "ORG02";
			org03.OH_Code = "ORG03";

			var cusCode01 = org01.CustomsCodes.AddNew();
			var cusCode02 = org02.CustomsCodes.AddNew();
			var cusCode03 = org03.CustomsCodes.AddNew();

			cusCode01.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode02.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode03.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;

			cusCode01.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedStates;
			cusCode02.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedStates;
			cusCode03.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedStates;

			cusCode01.OK_CustomsRegNo = "ABCD";
			cusCode02.OK_CustomsRegNo = "EFGH";
			cusCode03.OK_CustomsRegNo = "IJKL";

			var orgAddress01 = org01.Addresses.AddNew();
			var orgAddress03 = org03.Addresses.AddNew();

			consol.JK_OA_ShippingLineAddress = orgAddress03.PK;
			var billDetails = consol as IBillDetails;
			AssertEquals(1, billDetails.SCACIssuers.Count());
			AssertEquals(org03.PK, billDetails.SCACIssuers.ToArray()[0].PK);

			consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = org02.PK;
			var scacIssuers = billDetails.SCACIssuers.ToArray();
			AssertEquals(2, scacIssuers.Length);
			AssertEquals(org02.PK, scacIssuers[0].PK);
			AssertEquals(org03.PK, scacIssuers[1].PK);

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_OA_CreditorAddress = orgAddress01.PK;
			scacIssuers = billDetails.SCACIssuers.ToArray();
			AssertEquals(2, scacIssuers.Length);
			AssertEquals(org02.PK, scacIssuers[0].PK);
			AssertEquals(org03.PK, scacIssuers[1].PK);

			AssertEquals("EFGH", scacIssuers[0].SCACCode);
			AssertEquals("IJKL", scacIssuers[1].SCACCode);
		}

		#endregion

		#region ICreditControlledDocumentDelivery Members

		public void TestIsDPSFreightMovementRestricted()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var registryItem = OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions;
				var registryValue = DPSFreightMovementRestrictionsOptions.Codes.All;
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var creditControlledConsol = consol as ICreditControlledDocumentDelivery;
				Assert(creditControlledConsol != null);

				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
				AssertEquals("Reg option is All. Consol should be DPSFreightMovementRestricted:", true, creditControlledConsol.IsDPSFreightMovementRestricted);
			}
		}

		public void TestIsDPSFreightMovementRestrictedIsTrueIfComplianceRiskEnabled()
		{
			using (OrganisationsDataRegistry.Instance.ComplianceRiskFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var type = ObjectFactory.GetType("ComplianceRiskStatus");
				var consol = Factory.New<ForwardingConsol>();
				var instance = Factory.New(type);
				instance[ComplianceRiskStatusSchema.COR_ParentTableCode] = consol.TablePrefix;
				instance[ComplianceRiskStatusSchema.COR_ParentID] = consol.PK;
				instance[ComplianceRiskStatusSchema.COR_OverallRisk] = "PSK";
				instance[ComplianceRiskStatusSchema.COR_PartyRisk] = "PSK";

				Factory.Save();

				AssertEquals(true, ((ICreditControlledDocumentDelivery)consol).IsDPSFreightMovementRestricted);
			}
		}

		public void TestIsDPSFreightMovementRestrictedIsFalseIfComplianceRiskEnabled()
		{
			using (OrganisationsDataRegistry.Instance.ComplianceRiskFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var type = ObjectFactory.GetType("ComplianceRiskStatus");
				var consol = Factory.New<ForwardingConsol>();
				var instance = Factory.New(type);
				instance[ComplianceRiskStatusSchema.COR_ParentTableCode] = consol.TablePrefix;
				instance[ComplianceRiskStatusSchema.COR_ParentID] = consol.PK;
				instance[ComplianceRiskStatusSchema.COR_OverallRisk] = "OVR";

				AssertEquals(false, ((ICreditControlledDocumentDelivery)consol).IsDPSFreightMovementRestricted);
			}
		}

		public void TestIsAviationSecurityFreightMovementRestricted()
		{
			var consol = (ForwardingConsol)GetNewConsol();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "DEFRA";
			consol.JK_RL_NKDischargePort = "USLAX";

			var orgProxy = GlbBranch.CurrentBranch.OrgProxy;
			var orgProxyApproval = orgProxy.MainAddress.KnownShipperDetails.AddNew();
			orgProxyApproval.OV_OH_OrgHeader = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			orgProxyApproval.OV_EXApprovedOrMajorExporter = "RA";
			orgProxyApproval.OV_EXApprovalNumber = "12345-01";
			orgProxyApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(100);

			GlbStaff.CurrentUser.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Germany))
			using (FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AviationSecurityTrainingRestriction(true)))
			{
				GlbStaff.CurrentUser.Certificates.DeleteAll();
				AssertEquals(true, ((ICreditControlledDocumentDelivery)consol).IsAviationSecurityFreightMovementRestricted);

				var bkgCertificate = GlbStaff.CurrentUser.Certificates.AddNew();
				bkgCertificate.XZ_Type = StaffDefaultCertificateIDAndTrainingTypes.BKG;

				var dtaCertificate = GlbStaff.CurrentUser.Certificates.AddNew();
				dtaCertificate.XZ_Type = StaffDefaultCertificateIDAndTrainingTypes.DTA;

				AssertEquals(false, ((ICreditControlledDocumentDelivery)consol).IsAviationSecurityFreightMovementRestricted);

				consol.JK_RL_NKDischargePort = "DEHAM";
				GlbStaff.CurrentUser.Certificates.DeleteAll();
				AssertEquals(false, ((ICreditControlledDocumentDelivery)consol).IsAviationSecurityFreightMovementRestricted);
			}

			using (FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AviationSecurityTrainingRestriction(false)))
			{
				consol.JK_RL_NKDischargePort = "USLAX";
				GlbStaff.CurrentUser.Certificates.DeleteAll();
				AssertEquals(false, ((ICreditControlledDocumentDelivery)consol).IsAviationSecurityFreightMovementRestricted);
			}
		}

		#endregion

		#region ForwardingConsolCustomFieldProviderTest

		public void TestConsolCustomFields()
		{
			var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);

			OrgCustomLabels label = orgProxy.CustomLabels.AddNew();
			label.OT_Caption = "String1";
			label.OT_FieldName = "Consol.CustomString1";

			label = orgProxy.CustomLabels.AddNew();
			label.OT_Caption = "String2";
			label.OT_FieldName = "Consol.CustomString2";

			label = orgProxy.CustomLabels.AddNew();
			label.OT_Caption = "Date1";
			label.OT_FieldName = "Consol.CustomDate1";

			label = orgProxy.CustomLabels.AddNew();
			label.OT_Caption = "Date2";
			label.OT_FieldName = "Consol.CustomDate2";

			label = orgProxy.CustomLabels.AddNew();
			label.OT_Caption = "Number1";
			label.OT_FieldName = "Consol.CustomNumber1";

			label = orgProxy.CustomLabels.AddNew();
			label.OT_Caption = "Number2";
			label.OT_FieldName = "Consol.CustomNumber2";

			label = orgProxy.CustomLabels.AddNew();
			label.OT_Caption = "Flag1";
			label.OT_FieldName = "Consol.CustomFlag1";

			label = orgProxy.CustomLabels.AddNew();
			label.OT_Caption = "Flag2";
			label.OT_FieldName = "Consol.CustomFlag2";

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();

			consol.JK_CustomString1 = "test string1";
			consol.JK_CustomString2 = "test string2";
			consol.JK_CustomDate1 = new ZDateTime(2010, 08, 30);
			consol.JK_CustomDate2 = new ZDateTime(2010, 08, 31);
			consol.JK_CustomNumber1 = 123.45;
			consol.JK_CustomNumber2 = 54.321;
			consol.JK_CustomFlag1 = true;
			consol.JK_CustomFlag2 = false;

			ICustomFieldProvider customFieldProvider = consol;

			AssertEquals("Attrib1 value", "test string1", customFieldProvider.GetCustomField("String1", null));
			AssertEquals("Attrib2 value", "test string2", customFieldProvider.GetCustomField("String2", null));
			AssertEquals("Date1 value", new ZDateTime(2010, 08, 30), customFieldProvider.GetCustomField("Date1", null));
			AssertEquals("Date2 value", new ZDateTime(2010, 08, 31), customFieldProvider.GetCustomField("Date2", null));
			AssertEquals("Decimal1 value", (ZDecimal)123.45, customFieldProvider.GetCustomField("Number1", null));
			AssertEquals("Decimal2 value", (ZDecimal)54.321, customFieldProvider.GetCustomField("Number2", null));
			AssertEquals("Flag1 value", (ZBool)true, customFieldProvider.GetCustomField("Flag1", null));
			AssertEquals("Flag2 value", (ZBool)false, customFieldProvider.GetCustomField("Flag2", null));
		}

		public void TestCustomBusinessObjectIsReloadedOnActiveTemplateChange()
		{
			var airTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			airTemplate.P0_ProcessType = "CON";
			airTemplate.P0_SubType1 = "AIR";

			var airColumn = airTemplate.GenCustomColumnDefinitions.AddNew();
			airColumn.XC_Name = "Air Field 1";
			airColumn.XC_Type = AddOnColumnDataType.Codes.String;

			var seaTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			seaTemplate.P0_ProcessType = "CON";
			seaTemplate.P0_SubType1 = "SEA";

			var seaColumn = seaTemplate.GenCustomColumnDefinitions.AddNew();
			seaColumn.XC_Name = "Sea Field 1";
			seaColumn.XC_Type = AddOnColumnDataType.Codes.String;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";

			var customBusinessObject = ((ICustomFieldProvider)consol).GetCustomBusinessObject() as IDynamicBusinessObject;
			AssertContainsExactElementsInAnyOrder(new[] { "__AIR FIELD 1__prop__ZString", "__AIR FIELD 1__prop__ZStringInfo" }, customBusinessObject.PropertyNames);

			consol.JK_TransportMode = "SEA";
			customBusinessObject = ((ICustomFieldProvider)consol).GetCustomBusinessObject();

			AssertContainsExactElementsInAnyOrder(new[] { "__SEA FIELD 1__prop__ZString", "__SEA FIELD 1__prop__ZStringInfo" }, customBusinessObject.PropertyNames);
		}

		#endregion

		#region Phase Security

		public void TestReadOnlySecurity_PhaseResolverType()
		{
			ForwardingConsolForReadOnlySecurityTesting consol = Factory.New<ForwardingConsolForReadOnlySecurityTesting>();
			AssertEquals("Correct resolver type", true, consol.GetPhaseSecurityResolver() is ConsolPhaseSecurityResolver);
		}

		public void TestReadOnlySecurity_Phase_Disabled()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			AssertEquals("Precondition: phase security", false, consol.IsReadOnlyDueToPhase);
			AssertPropertiesAndChildrenAreReadonly("Not readonly by default", consol, false);

			consol.ReadOnly = true;
			AssertEquals("Forced to true when consol set to readonly", true, consol.IsReadOnlyDueToPhase);

			consol.ReadOnly = false;
			AssertEquals("Not readonly due to phase", false, consol.IsReadOnlyDueToPhase);
		}

		public void TestReadOnlySecurity_Phase_Enabled()
		{
			// Can't use mock here as CommonConsol calls RegisterEditable(...) from it's constructor
			// => GetPhaseResolver() is called before we can substitute it with mock
			PhaseSecurityTestHelper.Consol.SetupRestricted("XXX");

			ForwardingConsol consol = new BusinessObjectFactory().New<ForwardingConsol>();
			consol.JK_Phase = "XXX";
			consol.Factory.Save();

			consol = Factory.Load<ForwardingConsol>(consol.PK);

			AssertEquals("Precondition: phase security", true, consol.IsReadOnlyDueToPhase);
			AssertPropertiesAndChildrenAreReadonly("Readonly because of phase security", consol, true);

			Env.Security.ConsolPhaseSecurityOverride.IsAllowed = true;
			AssertEquals("Phase security does not apply to phase if user have rights", false, consol.JK_PhaseInfo.ReadOnly);

			consol.ReadOnly = false;
			AssertEquals("Still readonly due to phase", true, consol.IsReadOnlyDueToPhase);
		}

		void AssertPropertiesAndChildrenAreReadonly(string message, ForwardingConsol consol, bool expectedReadonly)
		{
			CombineAssertions(delegate
			{
				AssertEquals(message, expectedReadonly, consol.JK_ConsolModeInfo.ReadOnly);
				AssertEquals(message, expectedReadonly, consol.JK_TransportModeInfo.ReadOnly);
				AssertEquals(message, expectedReadonly, consol.JK_RL_NKLoadPortInfo.ReadOnly);
			});

			CombineAssertions(delegate
			{
				AssertEquals(message, expectedReadonly, consol.Shipments.ReadOnly);
				AssertEquals(message, expectedReadonly, consol.Transports.ReadOnly);
			});

			CombineAssertions(delegate
			{
				string excludedMessage = "This child should be excluded from phase security";

				var job = Factory.NewJobForTesting<JobHeader>();
				job.Parent = consol;
				job.JH_JobNum += Constants.GatewaySuffixForJobHeaderDeprecated;
				Factory.Save();
				Assert("consol.IsLegacyGateway", consol.Job.IsGatewayLegacyJob);

				AssertEquals(excludedMessage, false, consol.Job.ReadOnly);

				DummyApportionmentListing apportionmentListing = Factory.New<DummyApportionmentListing>();
				consol.RegisterEditableChildObject(apportionmentListing);
				AssertEquals(excludedMessage, false, apportionmentListing.ReadOnly);
			});
		}

		public void TestReadOnlySecurity_PropertiesAreForcedToReadOnly()
		{
			var security = PhaseSecurityTestHelper.Consol.SetupAllowed("XXX");

			var consol = new BusinessObjectFactory().New<ForwardingConsol>();
			consol.JK_Phase = "XXX";
			consol.Factory.Save();

			consol = Factory.Load<ForwardingConsol>(consol.PK);

			AssertEquals("Precondition: phase security", false, consol.IsReadOnlyDueToPhase);
			AssertEquals(false, consol.JK_ConsolModeInfo.ReadOnly);
			AssertEquals(false, consol.JK_TransportModeInfo.ReadOnly);
			AssertEquals(false, consol.Containers.ReadOnly);
			AssertEquals(false, consol.Transports.ReadOnly);

			var rule = security.Phases[0].Rules[0];

			var dependant1 = rule.Dependants.AddNew();
			dependant1.Name = "JK_ConsolMode";
			dependant1.IsReadOnly = true;

			var dependant2 = rule.Dependants.AddNew();
			dependant2.Name = "Containers";
			dependant2.IsReadOnly = true;

			var mandatoryDependant = rule.Dependants.AddNew();
			mandatoryDependant.Name = "JK_TransportMode";
			mandatoryDependant.IsMandatory = true;

			ForwardingConfigurationRegistry.Instance.ConsolPhaseSecurity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, security);

			consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);

			AssertEquals("Forced to readonly", true, consol.JK_ConsolModeInfo.ReadOnly);
			AssertEquals("Not readonly", false, consol.JK_TransportModeInfo.ReadOnly);
			AssertEquals("Forced to readonly", true, consol.Containers.ReadOnly);
			AssertEquals("Not readonly", false, consol.Transports.ReadOnly);
		}

		public void TestReadOnlySecurity_RegisterEditableChildObjectWithName()
		{
			ForwardingConsolForReadOnlySecurityTesting consol = Factory.New<ForwardingConsolForReadOnlySecurityTesting>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			foreach (System.Reflection.PropertyInfo propertyInfo in typeof(ForwardingConsol).GetProperties())
			{
				if (typeof(IBusiness).IsAssignableFrom(propertyInfo.PropertyType))
				{
					propertyInfo.GetValue(consol, null);
				}
			}

			// Some children are registered with names in CommonConsol
			ZString[] expectedRegisteredNames = new ZString[]
			{
				"Containers",
				"Shipments",
				"Routing"
			};

			AssertContainsExactElementsInAnyOrder("Expected registered children with names", expectedRegisteredNames, consol.RegisteredChildrenNames);

			ConsolPhaseDependantsProvider dependantsProvider = new ConsolPhaseDependantsProvider();
			List<ZString> supportedChildrenNamesFromRegistry = new List<ZString>();
			supportedChildrenNamesFromRegistry.AddRange(dependantsProvider.GetChildDependants().Select(x => x.Name));
			supportedChildrenNamesFromRegistry.AddRange(dependantsProvider.GetChildExpandableDependants().Keys.Select(key => key.Name));

			string message = "\r\n\r\nRegistered children with names should be in sync with the following class:\r\n" +
							"Enterprise.Freight.Forwarding.Registry.ConsolPhaseDependantsProvider";

			AssertContainsExactElementsInAnyOrder(message, consol.RegisteredChildrenNames, supportedChildrenNamesFromRegistry);
		}

		public void TestReadOnlySecurity_ShowSubHouseBillShipments()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();

			AssertEquals("Pre-condition", false, consol.ReadOnly);
			AssertEquals("ShowSubHouseBillShipments is not readonly by default", false, consol.ShowSubHouseBillShipmentsInfo.ReadOnly);

			consol.ReadOnly = true;
			AssertEquals("ShowSubHouseBillShipments is not readonly when consol is made readonly so it can be used in Consol Planning Board", false, consol.ShowSubHouseBillShipmentsInfo.ReadOnly);
		}

		public void TestPhaseSecurity_AllProperties_InitializationIsSafe()
		{
			Action<bool> setupAndAssert = (isReadonly) =>
			{
				var creationFactory = new BusinessObjectFactory();

				var consol = creationFactory.New<ForwardingConsol>();
				consol.JK_Phase = "XXX";

				creationFactory.Save();

				var security = PhaseSecurityTestHelper.Consol.SetupAllowed("XXX");
				var rule = security.Phases[0].Rules[0];

				var allCustomizablePropertyNames = new ConsolPhaseDependantsProvider()
					.GetIZTypeProperties()
					.Select(p => p.Name)
					.ToArray();

				foreach (var propertyName in allCustomizablePropertyNames)
				{
					var dependant = rule.Dependants.AddNew();
					dependant.Name = propertyName;
					dependant.IsReadOnly = isReadonly;
					dependant.IsMandatory = !isReadonly;
				}

				ForwardingConfigurationRegistry.Instance.ConsolPhaseSecurity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, security);

				AssertNoExceptionThrown(() =>
				{
					consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);

					foreach (var propertyInfo in consol.ZPropertyInfoHash.Cast<ZPropertyInfo>())
					{
						var touchAndGo = propertyInfo.Value;
					}
				});
			};

			setupAndAssert(true);
			setupAndAssert(false);
		}

		public void TestPhaseDependantMandatoryValidation_PropertyChange()
		{
			var phaseSecurity = GetPhaseSecurityForTesting();

			using (ForwardingConfigurationRegistry.Instance.ConsolPhaseSecurity.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, phaseSecurity))
			{
				var consol = Factory.New<ForwardingConsol>();

				consol.JK_PrepaidCollect = "PPD";
				consol.JK_AWBServiceLevel = "STD";
				consol.JK_ScreeningStatus = "UNK";
				consol.JK_Phase = "AAA";

				AssertNoErrors(consol.JK_PrepaidCollectInfo);
				AssertNoErrors(consol.JK_AWBServiceLevelInfo);
				AssertNoErrors(consol.JK_ScreeningStatusInfo);

				consol.JK_PrepaidCollect = "";
				consol.JK_AWBServiceLevel = "";
				consol.JK_RL_NKDischargePort = "";

				AssertHasError(consol.JK_PrepaidCollectInfo, "Payment Type has been made mandatory in the selected Phase. Please refer to Registry -> Freight -> Consolidations -> Phases for details.");
				AssertHasError(consol.JK_AWBServiceLevelInfo, "Service Level has been made mandatory in the selected Phase. Please refer to Registry -> Freight -> Consolidations -> Phases for details.");
				AssertNoErrors("JK_ScreeningStatus is not required", consol.JK_ScreeningStatusInfo);
			}
		}

		public void TestPhaseDependantMandatoryValidation_PhaseChange()
		{
			var phaseSecurity = GetPhaseSecurityForTesting();

			using (ForwardingConfigurationRegistry.Instance.ConsolPhaseSecurity.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, phaseSecurity))
			{
				var consol = Factory.New<ForwardingConsol>();

				consol.JK_PrepaidCollect = "PPD";
				consol.JK_AWBServiceLevel = "STD";
				consol.JK_ScreeningStatus = "UNK";

				AssertEquals("Precondition", "ALL", consol.JK_Phase);

				consol.JK_PrepaidCollect = string.Empty;
				consol.JK_AWBServiceLevel = string.Empty;
				consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;

				AssertNoErrors(consol.JK_PrepaidCollectInfo);
				AssertNoErrors(consol.JK_AWBServiceLevelInfo);
				AssertNoErrors(consol.JK_ScreeningStatusInfo);

				consol.JK_Phase = "AAA";
				consol.RunPreSaveValidation();

				AssertHasError(consol.JK_PrepaidCollectInfo, "Payment Type has been made mandatory in the selected Phase. Please refer to Registry -> Freight -> Consolidations -> Phases for details.");
				AssertHasError(consol.JK_AWBServiceLevelInfo, "Service Level has been made mandatory in the selected Phase. Please refer to Registry -> Freight -> Consolidations -> Phases for details.");
				AssertNoErrors("JK_ScreeningStatus is not required", consol.JK_ScreeningStatusInfo);

				consol.JK_Phase = "ALL";
				consol.RunPreSaveValidation();

				AssertNoErrors(consol.JK_PrepaidCollectInfo);
				AssertNoErrors(consol.JK_AWBServiceLevelInfo);
				AssertNoErrors(consol.JK_ScreeningStatusInfo);
			}
		}

		PhaseSecurity GetPhaseSecurityForTesting()
		{
			var locationsList = PhaseConstants.GetConsolLocationsList();

			var security = new PhaseSecurity(locationsList);
			security.IsEnabled = true;

			var phase = security.Phases.AddNew();
			phase.Code = "AAA";
			phase.Description = (NoResString)"AAA Phase Description";

			var rule = phase.Rules.AddNew();
			rule.Location = PhaseConstants.Locations.AnyLocation;
			rule.DepartmentPK = GlbDepartment.CurrentDepartment.PK;

			Action<PhaseRule, string> addDependant = (phaseRule, dependantName) =>
			{
				var dependant = phaseRule.Dependants.AddNew();
				dependant.DependantType = PhaseConstants.DependantType.Property;
				dependant.Name = dependantName;
				dependant.Description = dependantName + "Description";
				dependant.IsMandatory = true;
			};

			addDependant(rule, "JK_PrepaidCollect");
			addDependant(rule, "JK_AWBServiceLevel");

			security.Phases.Add(phase);

			return security;
		}

		#region Shipping Line

		public void TestIsAllowedToChangeShippingLineAllMessages()
		{
			var currentCompany = GlbCompany.CurrentCompany;
			var consol = Factory.New<ForwardingConsol>();
			var transport = consol.Transports.AddNew();
			var container = consol.Containers.AddNew();
			container.JC_ContainerCount = 5;

			var consolCost = (BusinessObject)Factory.New<IJobConsolCost>();
			consolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				consolCost[JobConsolCostSchema.E6_ParentID] = consol.PK;
				consolCost[JobConsolCostSchema.E6_ParentTableCode] = "JK";
			}
			finally
			{
				consolCost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}

			consolCost[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;

			AssertEquals("Precondition: Has Routing Legs", true, consol.Transports.Any());
			AssertEquals("Precondition: Has Container Info", true, !consol.JK_Calc_ContainerCount.IsEmpty);
			AssertEquals("Precondition: Has Consol Cost", true, consol.HasConsolCosts(currentCompany));

			var message = ZString.Empty;
			consol.IsAllowedToChangeShippingLine(ref message);
			const string expectedMessage = "Please verify and manually change the following information: Routing Legs\r\nContainer Count and Container Numbers\r\nConsol Costing\r\n";
			AssertEquals("Messages are not empty", expectedMessage, message);
		}

		public void TestIsAllowedToChangeShippingLineSomeMessages()
		{
			var currentCompany = GlbCompany.CurrentCompany;
			var consol = Factory.New<ForwardingConsol>();
			var transport = consol.Transports.AddNew();

			var consolCost = (BusinessObject)Factory.New<IJobConsolCost>();
			consolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				consolCost[JobConsolCostSchema.E6_ParentID] = consol.PK;
				consolCost[JobConsolCostSchema.E6_ParentTableCode] = "JK";
			}
			finally
			{
				consolCost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}

			consolCost[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;

			AssertEquals("Precondition: Has Routing Legs", true, consol.Transports.Any());
			AssertEquals("Precondition: Has No Container Info", true, consol.JK_Calc_ContainerCount.IsEmpty);
			AssertEquals("Precondition: Has Consol Cost", true, consol.HasConsolCosts(currentCompany));

			var message = ZString.Empty;
			consol.IsAllowedToChangeShippingLine(ref message);
			const string expectedMessage = "Please verify and manually change the following information: Routing Legs\r\nConsol Costing\r\n";
			AssertEquals("Messages are not empty", expectedMessage, message);
		}

		public void TestIsAllowedToChangeShippingLineEvents()
		{
			var consol = Factory.New<ForwardingConsol>();

			ZString mstValue = "Booking Requested";

			var initialMessage = ZString.Empty;
			var initialResult = consol.IsAllowedToChangeShippingLine(ref initialMessage);
			AssertEquals("Should be allowed to change shipping line", initialResult, true);

			var acceptedLog = consol.Logs.AddNew(Events.MessageAccepted, new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.MessageType, mstValue));
			using (acceptedLog.LockForUpdatingKeyFieldsForTesting())
			{
				acceptedLog.SL_EventTime = ZDateTime.UtcNow;
			}
			acceptedLog.Factory.Save();

			var afterAcceptedMessage = ZString.Empty;
			var afterAcceptedResult = consol.IsAllowedToChangeShippingLine(ref afterAcceptedMessage);
			AssertEquals("Should not be allowed to change shipping line", afterAcceptedResult, false);

			var withdrawnLog = consol.Logs.AddNew(Events.MessageWithdrawCancelRequest, new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.MessageType, mstValue));
			using (withdrawnLog.LockForUpdatingKeyFieldsForTesting())
			{
				withdrawnLog.SL_EventTime = DateTime.UtcNow.AddMinutes(10);
			}
			withdrawnLog.Factory.Save();

			var withdrawnMessage = ZString.Empty;
			var withdrawnResult = consol.IsAllowedToChangeShippingLine(ref withdrawnMessage);
			AssertEquals("Should be allowed to change shipping line", withdrawnResult, true);

			var reacceptedLog = consol.Logs.AddNew(Events.MessageAccepted, new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.MessageType, mstValue));
			using (reacceptedLog.LockForUpdatingKeyFieldsForTesting())
			{
				reacceptedLog.SL_EventTime = DateTime.UtcNow.AddMinutes(20);
			}
			reacceptedLog.Factory.Save();

			var reacceptedMessage = ZString.Empty;
			var reacceptedResult = consol.IsAllowedToChangeShippingLine(ref reacceptedMessage);
			AssertEquals("Should not be allowed to change shipping line", reacceptedResult, false);
		}

		public void TestSetShippingLineRedefaults()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var creditorAddress = Factory.NewWithValidTestData<OrgAddress>();
			var arrivalCTOAddress = Factory.New<OrgAddress>();
			var departureCTOAddress = Factory.New<OrgAddress>();
			var containerYardEmptyPickUpAddress = Factory.New<OrgAddress>();
			var containerYardEmptyReturnAddress = Factory.New<OrgAddress>();

			var shippingLineAddress = Factory.NewWithValidTestData<OrgAddress>();
			shippingLineAddress.OA_Code = "OLD1";

			consol.JK_OA_ShippingLineAddress = shippingLineAddress.PK;
			consol.JK_OA_CreditorAddress = creditorAddress.PK;
			consol.JK_OA_ArrivalCTOAddress = arrivalCTOAddress.PK;
			consol.JK_OA_DepartureCTOAddress = departureCTOAddress.PK;
			consol.JK_OA_ContainerYardEmptyPickupAddress = containerYardEmptyPickUpAddress.PK;
			consol.JK_OA_ContainerYardEmptyReturnAddress = containerYardEmptyReturnAddress.PK;
			consol.JK_RL_NKMasterBillIssuePlace = "ABC12";

			AssertNull("Precondition: No Status Updated logs", consol.Logs.MostRecentLogByEventTime(Events.StatusUpdated));
			AssertEquals("Precondition: Old Shipping line address", consol.JK_OA_ShippingLineAddress, shippingLineAddress.PK);

			var newShippingLineAddress = Factory.NewWithValidTestData<OrgAddress>();
			newShippingLineAddress.OA_Code = "NEW1";

			ZString reason = "A good reason";
			consol.SetShippingLine(newShippingLineAddress.PK, reason);

			var stuEvent = consol.Logs.MostRecentLogByEventTime(Events.StatusUpdated);
			AssertNotNull("Expected STU event", stuEvent);
			ZString expectedMessage = string.Format("|NEW={0}|OLD={1}|RES=A good reason|TYP=Carrier Changed", newShippingLineAddress.OA_Code, shippingLineAddress.OA_Code);

			AssertEquals("Incorrect STU Message", expectedMessage, stuEvent.SL_Reference);
			AssertEquals("Expected redefaulted JK_OA_CreditorAddress", creditorAddress.PK, consol.JK_OA_CreditorAddress);
			AssertEquals("Expected redefaulted JK_OA_ArrivalCTOAddress", ZGuid.Empty, consol.JK_OA_ArrivalCTOAddress);
			AssertEquals("Expected redefaulted JK_OA_DepartureCTOAddress", ZGuid.Empty, consol.JK_OA_DepartureCTOAddress);
			AssertEquals("Expected redefaulted JK_OA_ContainerYardEmptyPickupAddress", ZGuid.Empty, consol.JK_OA_ContainerYardEmptyPickupAddress);
			AssertEquals("Expected redefaulted JK_OA_ContainerYardEmptyReturnAddress", ZGuid.Empty, consol.JK_OA_ContainerYardEmptyReturnAddress);
			AssertEquals("Expected redefaulted JK_RL_NKMasterBillIssuePlace", ZString.Empty, consol.JK_RL_NKMasterBillIssuePlace);
		}

		#endregion

		#region Test Classes

		class DummyApportionmentListing : DummyBusinessObject, IApportionmentListing
		{
			public DummyApportionmentListing(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public void UpdateOrCreateCost(AccChargeCode chargeCode, OrgHeader creditor, decimal amount)
			{
				throw new NotImplementedException();
			}
		}

		class ForwardingConsolForReadOnlySecurityTesting : ForwardingConsol
		{
			public ForwardingConsolForReadOnlySecurityTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new IPhaseSecurityResolver GetPhaseSecurityResolver()
			{
				return base.GetPhaseSecurityResolver();
			}

			protected override void RegisterEditableChildObject(IBusiness child, ZString childName)
			{
				base.RegisterEditableChildObject(child, childName);
				RegisteredChildrenNames.Add(childName);
			}

			public List<ZString> RegisteredChildrenNames = new List<ZString>();
		}

		#endregion

		#endregion

		#region JK_SZB

		public void TestJK_SZB()
		{
			CombineAssertions("By default there should be no SZB", () =>
			{
				AssertEquals(ZString.Empty, Consol.JK_SZB);
				AssertEquals(ZString.Empty, Consol.JK_SZBInformation);
				AssertEquals(ZDateTime.Empty, Consol.JK_SZBIssueDate);
				AssertNull(Consol.CusEntryNums.OfType<CusEntryNumber>().FirstOrDefault(n => n.CE_EntryType == GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber));
			});

			Consol.JK_SZB = "FOO";
			AssertEquals("SZB was added", "FOO", Consol.JK_SZB);
			AssertEquals("SZB was added", 1, Consol.CusEntryNums.OfType<CusEntryNumber>().Count(n => n.CE_EntryType == GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber));

			Consol.JK_SZBInformation = "BAR";
			AssertEquals("BAR", Consol.JK_SZBInformation);
			AssertEquals("Only one SZB", 1, Consol.CusEntryNums.OfType<CusEntryNumber>().Count(n => n.CE_EntryType == GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber));

			Consol.JK_SZBIssueDate = new ZDateTime(2013, 10, 31);
			AssertEquals(new ZDateTime(2013, 10, 31), Consol.JK_SZBIssueDate);
			AssertEquals("Only one SZB", 1, Consol.CusEntryNums.OfType<CusEntryNumber>().Count(n => n.CE_EntryType == GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber));

			var szbNumber = Consol.CusEntryNums.OfType<CusEntryNumber>().FirstOrDefault(n => n.CE_EntryType == GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber);
			CombineAssertions("SZB CusEntryNumber properties", () =>
			{
				AssertEquals(Consol.PK, szbNumber.CE_ParentID);
				AssertEquals(Consol.TableName, szbNumber.CE_ParentTable);
				AssertEquals(GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber, szbNumber.CE_EntryType);
				AssertEquals(CusEntryNumber.Categories.CustomsPermitClearanceNumber, szbNumber.CE_Category);
				AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, szbNumber.CE_RN_NKCountryCode);
				AssertEquals(false, szbNumber.CE_EntryIsSystemGenerated);
			});

			Factory.Save();

			var consolReloaded = new BusinessObjectFactory().Load<ForwardingConsol>(Consol.PK);
			CombineAssertions("SZB persisted", () =>
			{
				AssertEquals("FOO", consolReloaded.JK_SZB);
				AssertEquals("BAR", consolReloaded.JK_SZBInformation);
				AssertEquals(new ZDateTime(2013, 10, 31), consolReloaded.JK_SZBIssueDate);
			});
		}

		#endregion

		#region Multi AWB Master support

		public void TestColoadConsols()
		{
			var consol = Factory.New<ForwardingConsol>();
			AssertType(typeof(ColoadConsolCollection), consol.ColoadConsols);
			Assert("IsRegisteredEditableChildObject", consol.IsRegisteredEditableChildObject(consol.ColoadConsols));
		}

		public void TestColoadConsols_List_FilterDefaults()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var filterDefaults = ((IFilterBusinessObjectDefaultsProvider)consol.ColoadConsols_List).FilterBusinessObjectDefaults;

			Func<string, string, string> createKey = (filterName, propertyName) =>
			{
				return filterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + propertyName;
			};

			string transportModeKey = createKey("Transport Mode", "Property");
			AssertEquals("Transport mode", Constants.TransportModes.Air, filterDefaults[transportModeKey].Value);

			string consolTypeKey = createKey("Consol Type", "Property");
			AssertEquals("Consol type", Constants.AgentType.AWBCoload, filterDefaults[consolTypeKey].Value);

			string loadPortKey = createKey("End Ports (First Load / Last Disch.)", "Property1");
			AssertEquals("Load port", "AUSYD", filterDefaults[loadPortKey].Value);

			string dischargePortKey = createKey("End Ports (First Load / Last Disch.)", "Property2");
			AssertEquals("Discharge port", "NZAKL", filterDefaults[dischargePortKey].Value);
		}

		public void TestColoadConsols_ConsolRemoved_ConsolShipmentsUnpacked()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.AWBMaster;

			var coloadConsol1 = consol.ColoadConsols.AddNew();
			var shipment11 = coloadConsol1.Shipments.AddNew();
			var packline11 = shipment11.OuterPackLines.AddNew();

			var shipment12 = coloadConsol1.Shipments.AddNew();
			var packline12 = shipment12.OuterPackLines.AddNew();

			var coloadConsol2 = consol.ColoadConsols.AddNew();
			var shipment21 = coloadConsol2.Shipments.AddNew();
			var packline21 = shipment21.OuterPackLines.AddNew();

			var container = consol.Containers.AddNew();
			packline11.SetContainer(consol, container);
			packline12.SetContainer(consol, container);
			packline21.SetContainer(consol, container);

			AssertContainsExactElementsInAnyOrder(new[] { packline11, packline12, packline21 }, container.PackLines);

			consol.ColoadConsols.RemoveFromRelationship(coloadConsol1);
			AssertContainsExactElementsInAnyOrder(new[] { packline21 }, container.PackLines);
		}

		public void TestMultiAWBMaster_RelatedPacklinesAreCollectedFromShipmentsFromColoadConsols()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.AWBMaster;

			var consol1 = consol.ColoadConsols.AddNew();
			var shipment1 = consol1.Shipments.AddNew();
			var packline11 = shipment1.OuterPackLines.AddNew();
			var packline12 = shipment1.OuterPackLines.AddNew();

			var shipment2 = consol1.Shipments.AddNew();
			var packline21 = shipment2.OuterPackLines.AddNew();

			var consol2 = consol.ColoadConsols.AddNew();
			var shipment3 = consol2.Shipments.AddNew();
			var packline31 = shipment3.OuterPackLines.AddNew();

			AssertContainsExactElementsInAnyOrder(new[] { packline11, packline12, packline21, packline31 }, consol.RelatedPackLines);

			consol.ColoadConsols.RemoveFromRelationship(consol2);
			AssertContainsExactElementsInAnyOrder("RelatedPackLines were refreshed when coload consol was detached",
				new[] { packline11, packline12, packline21 },
				consol.RelatedPackLines);

			consol.ColoadConsols.Add(consol2);
			AssertContainsExactElementsInAnyOrder("RelatedPackLines were refreshed when coload consol was attached",
				new[] { packline11, packline12, packline21, packline31 },
				consol.RelatedPackLines);
		}

		public void TestMultiAWBMaster_UnAllocatedPackLines()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.AWBMaster;

			var container = consol.Containers.AddNew();

			var consolA = consol.ColoadConsols.AddNew();
			var shipmentA1 = consolA.Shipments.AddNew();
			var packlineA1_1 = shipmentA1.OuterPackLines.AddNew();
			var packlineA1_2 = shipmentA1.OuterPackLines.AddNew();

			packlineA1_2.SetContainer(consol, container);

			var shipmentA2 = consolA.Shipments.AddNew();
			var packlineA2_1 = shipmentA2.OuterPackLines.AddNew();

			var consolB = consol.ColoadConsols.AddNew();
			var shipmentB1 = consolB.Shipments.AddNew();
			var packlineB1_1 = shipmentB1.OuterPackLines.AddNew();

			AssertContainsExactElementsInAnyOrder("Master has one packline already allocated",
				new[] { packlineA1_1, packlineA2_1, packlineB1_1 },
				consol.UnAllocatedPackLines);

			AssertContainsExactElementsInAnyOrder("Sub consol still has all packlines unallocated",
				new[] { packlineA1_1, packlineA1_2, packlineA2_1 },
				consolA.UnAllocatedPackLines);
		}

		public void TestJK_AgentType_NotAllowedToChangeToMultiAWBMasterWhenHasShipments()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;

			string caption = "";
			string message = "";
			consol.ShowMessageOnGUI += (s, e) =>
			{
				caption = e.Title;
				message = e.Message;
			};

			consol.Shipments.AddNew();

			consol.JK_AgentType = Constants.AgentType.AWBMaster;
			AssertEquals("Change restricted", Constants.AgentType.Agent, consol.JK_AgentType);
			AssertEquals("Event was raised", string.Format("Could not change to {0}", Constants.AgentTypeDescriptions.AWBMaster), caption);
			AssertEquals("Event was raised", "All shipments should be detached first.", message);

			message = "";
			consol.Shipments.RemoveAll();

			consol.JK_AgentType = Constants.AgentType.AWBMaster;
			AssertEquals("Change allowed", Constants.AgentType.AWBMaster, consol.JK_AgentType);
			AssertEquals("Event not raised", "", message);
		}

		public void TestJK_AgentType_NotAllowedToChangeFromMultiAWBMasterWhenHasColoadConsols()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.AWBMaster;

			string caption = "";
			string message = "";
			consol.ShowMessageOnGUI += (s, e) =>
			{
				caption = e.Title;
				message = e.Message;
			};

			consol.ColoadConsols.AddNew();

			consol.JK_AgentType = Constants.AgentType.Agent;
			AssertEquals("Change restricted", Constants.AgentType.AWBMaster, consol.JK_AgentType);
			AssertEquals("Event was raised", string.Format("Could not change from {0}", Constants.AgentTypeDescriptions.AWBMaster), caption);
			AssertEquals("Event was raised", "All coload consols should be detached first.", message);

			message = "";
			consol.ColoadConsols.RemoveFromRelationship(consol.ColoadConsols[0]);

			consol.JK_AgentType = Constants.AgentType.Agent;
			AssertEquals("Change allowed", Constants.AgentType.Agent, consol.JK_AgentType);
			AssertEquals("Event not raised", "", message);
		}

		#endregion

		#region IAutoRatingWeightBreakOverrideProvider Members

		public void TestWeightBreakOverrideProvider()
		{
			var orgProxyAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			var testConsol = Factory.New<ForwardingConsol>();
			testConsol.JK_TransportMode = Constants.TransportModes.Air;
			testConsol.JK_AgentType = Constants.AgentType.Agent;
			testConsol.JK_RL_NKLoadPort = "AUBNE";
			testConsol.JK_OA_SendingForwarderAddress = orgProxyAddress;
			testConsol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var adapter = (IAutoRatingWeightBreakOverrideProvider)testConsol.RatingAdapter;
			AssertEquals(null, adapter.WeightBreakOverride);

			var port = Factory.New<OrgAppointedAgentPorts>();
			port.O5_PortOrCountry = "AUBNE";
			port.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;

			if (testConsol.SendingForwarder == null)
			{
				Assert("If there is no sending forwarder (i.e. CFS won't allow it to be set) then consol cannot be a gateway consol and this test is irrelevant", true);
				return;
			}

			testConsol.SendingForwarder.AppointedGatewayAgentPorts.Add(port);

			Assert("Pre-condition", testConsol.IsGateway());
			AssertEquals(0m, adapter.WeightBreakOverride.Value);

			RatingDataRegistry.Instance.GatewayBillingUseTotalWeightForWeightBreak.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertNull(adapter.WeightBreakOverride);

			var shipment1 = testConsol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 300m;
			shipment1.JS_ActualVolume = 1m;
			var shipment2 = testConsol.Shipments.AddNew();
			shipment2.JS_ActualWeight = 200m;
			shipment2.JS_ActualVolume = 2m;

			RatingDataRegistry.Instance.GatewayBillingUseTotalWeightForWeightBreak.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, adapter.WeightBreakOverride.HasValue);
			AssertEquals(500m, adapter.WeightBreakOverride.Value);

			testConsol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertNull(adapter.WeightBreakOverride);
		}

		#endregion

		#region IJobCostingPlugIn Members / FreightCost / ExchangeRates

		public void TestReceivingAgent()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			OrgHeader recAgent = Factory.New<OrgHeader>();
			recAgent.OH_IsForwarder = ZBool.True;
			consol.JK_OA_ReceivingForwarderAddress = recAgent.MainAddress.PK;
			AssertEquals("Should be correct rec forwarder", recAgent.PK, ((IJobCostingPlugIn)consol).ReceivingAgent.PK);
		}

		public void TestReceivingAgentAPInvoicingParty()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			OrgHeader recAgent = Factory.New<OrgHeader>();
			recAgent.OH_IsForwarder = ZBool.True;
			consol.JK_OA_ReceivingForwarderAddress = recAgent.MainAddress.PK;
			AssertEquals("Should be correct rec forwarder", recAgent.PK, ((IJobCostingPlugIn)consol).ReceivingAgentAPInvoicingParty.PK);

			OrgHeader aPGroup = Factory.New<OrgHeader>();
			aPGroup.OH_IsForwarder = ZBool.True;
			recAgent.SetRelatedParty(aPGroup, RelatedPartyTypeList.Codes.APNettingGroup, RelatedPartyDirectionList.Codes.Forwarder);
			AssertEquals("Should be correct rec forwarder when overridden", aPGroup.PK, ((IJobCostingPlugIn)consol).ReceivingAgentAPInvoicingParty.PK);
		}

		public void TestReceivingAgentARInvoicingParty()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			OrgHeader recAgent = Factory.New<OrgHeader>();
			recAgent.OH_IsForwarder = ZBool.True;
			consol.JK_OA_ReceivingForwarderAddress = recAgent.MainAddress.PK;
			AssertEquals("Should be correct rec forwarder", recAgent.PK, ((IJobCostingPlugIn)consol).ReceivingAgentARInvoicingParty.PK);

			OrgHeader aRGroup = Factory.New<OrgHeader>();
			aRGroup.OH_IsForwarder = ZBool.True;
			recAgent.SetRelatedParty(aRGroup, RelatedPartyTypeList.Codes.ARNettingGroup, RelatedPartyDirectionList.Codes.Forwarder);
			AssertEquals("Should be correct rec forwarder when overridden", aRGroup.PK, ((IJobCostingPlugIn)consol).ReceivingAgentARInvoicingParty.PK);
		}

		public void TestSendingAgent()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			OrgHeader sendAgent = Factory.New<OrgHeader>();
			sendAgent.OH_IsForwarder = ZBool.True;
			consol.JK_OA_SendingForwarderAddress = sendAgent.MainAddress.PK;
			AssertEquals("Should be correct rec forwarder", sendAgent.PK, ((IJobCostingPlugIn)consol).SendingAgent.PK);
		}

		public void TestSendingAgentAPInvoicingParty()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			OrgHeader sendAgent = Factory.New<OrgHeader>();
			sendAgent.OH_IsForwarder = ZBool.True;
			consol.JK_OA_SendingForwarderAddress = sendAgent.MainAddress.PK;
			AssertEquals("Should be correct rec forwarder", sendAgent.PK, ((IJobCostingPlugIn)consol).SendingAgentAPInvoicingParty.PK);

			OrgHeader aPGroup = Factory.New<OrgHeader>();
			aPGroup.OH_IsForwarder = ZBool.True;
			sendAgent.SetRelatedParty(aPGroup, RelatedPartyTypeList.Codes.APNettingGroup, RelatedPartyDirectionList.Codes.Forwarder);
			AssertEquals("Should be correct rec forwarder when overridden", aPGroup.PK, ((IJobCostingPlugIn)consol).SendingAgentAPInvoicingParty.PK);
		}

		public void TestSendingAgentARInvoicingParty()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			OrgHeader sendAgent = Factory.New<OrgHeader>();
			sendAgent.OH_IsForwarder = ZBool.True;
			consol.JK_OA_SendingForwarderAddress = sendAgent.MainAddress.PK;
			AssertEquals("Should be correct rec forwarder", sendAgent.PK, ((IJobCostingPlugIn)consol).SendingAgentARInvoicingParty.PK);

			OrgHeader aRGroup = Factory.New<OrgHeader>();
			aRGroup.OH_IsForwarder = ZBool.True;
			sendAgent.SetRelatedParty(aRGroup, RelatedPartyTypeList.Codes.ARNettingGroup, RelatedPartyDirectionList.Codes.Forwarder);
			AssertEquals("Should be correct rec forwarder when overridden", aRGroup.PK, ((IJobCostingPlugIn)consol).SendingAgentARInvoicingParty.PK);
		}

		#region TestPrepaidCollectList

		public void TestPrepaidCollectList()
		{
			var consol = (IJobCostingPlugIn)Factory.New<ForwardingConsol>();

			AssertContainsExactElementsInAnyOrder(new[] {
				PrepaidCollectCodes.All,
				PrepaidCollectCodes.CTS,
				PrepaidCollectCodes.CCX,
				PrepaidCollectCodes.PPD,
				PrepaidCollectCodes.LOG,
				PrepaidCollectCodes.FOG,
				PrepaidCollectCodes.LDT,
				PrepaidCollectCodes.FDT,
			}, consol.PrepaidCollectList.ToArray().Select(x => x.Code));
		}

		#endregion

		#region Test Container Client Updates From Consol

		public void TestReceivingAgentUpdatesJC_OH_CFSClient()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";

			var receivingClient = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_IsCFS = true;
			consol.JK_RL_NKLoadPort = "HKHKG";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var container = Factory.NewWithValidTestData<ForwardingContainer>();
			container.JC_IsCFSRegistered = true;
			consol.Containers.RemoveAll();
			consol.Containers.Add(container);

			consol.JK_OA_ReceivingForwarderAddress_ZAddress.OrgPK = receivingClient.PK;
			consol.JK_OA_UnpackDepotAddress_ZAddress.OrgPK = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			Factory.Save();

			AssertEquals("Client should be loaded from consol on the CFS Container", receivingClient.PK, container.JC_OH_CFSClient);

			var newClient = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ReceivingForwarderAddress_ZAddress.OrgPK = newClient.PK;
			Factory.Save();

			AssertEquals("Client should be updated from consol on the CFS Container", newClient.PK, container.JC_OH_CFSClient);
		}

		public void TestSendingAgentUpdatesJC_OH_CFSClient()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";

			var sendingClient = Factory.NewWithValidTestData<OrgHeader>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_IsCFS = true;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "HKHKG";

			var container = Factory.NewWithValidTestData<ForwardingContainer>();
			container.JC_IsCFSRegistered = true;
			consol.Containers.RemoveAll();
			consol.Containers.Add(container);

			consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK = sendingClient.PK;
			consol.JK_OA_PackDepotAddress_ZAddress.OrgPK = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			Factory.Save();

			AssertEquals("Client should be loaded from forwarding consol on the CFS Container", sendingClient.PK, container.JC_OH_CFSClient);

			var newClient = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK = newClient.PK;
			Factory.Save();

			AssertEquals("Client should be updated from forwarding consol on the CFS Container", newClient.PK, container.JC_OH_CFSClient);
		}

		#endregion

		public void TestLoadAndDischargePortsForIGenericConsol()
		{
			var consol = GetNewConsol();
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var internationalTransport = consol.Transports.AddNew();
			internationalTransport.JW_RL_NKLoadPort = "USLAX";
			internationalTransport.JW_RL_NKDiscPort = "AUMEL";

			var domesticTransport = consol.Transports.AddNew();
			domesticTransport.JW_RL_NKLoadPort = "AUMEL";
			domesticTransport.JW_RL_NKDiscPort = "AUSYD";

			AssertEquals("Port of Loading for Generic Consol should be consol load port, not most interesting transport load port", "USLAX", ((IJobCostingPlugIn)consol).CostSupporter.PortOfLoading);
			AssertEquals("Port of Discharge for Generic Consol should be consol discharge port, not most interesting transport discharge port", "AUSYD", ((IJobCostingPlugIn)consol).CostSupporter.PortOfDischarge);
		}

		public void TestContainerMode()
		{
			IJobCostingPlugIn jobCostingPlugIn = Consol;

			Consol.JK_ConsolMode = "XXX";
			AssertEquals("XXX", jobCostingPlugIn.ContainerMode);

			Consol.JK_ConsolMode = "AAA";
			AssertEquals("AAA", jobCostingPlugIn.ContainerMode);
		}

		public void TestModule()
		{
			var consol = GetNewConsol();
			IJobCostingPlugIn jobCostingPlugIn = Consol;

			AssertEquals(ApportionmentMethodModules.Forwarding, jobCostingPlugIn.Module);
		}

		public void TestConsolType()
		{
			IJobCostingPlugIn jobCostingPlugIn = Consol;

			Consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			AssertEquals(Core.Constants.AgentType.CoLoad, jobCostingPlugIn.ConsolType);

			Consol.JK_AgentType = Core.Constants.AgentType.AWBCoload;
			AssertEquals(Core.Constants.AgentType.AWBCoload, jobCostingPlugIn.ConsolType);
		}

		public void TestDirection()
		{
			IJobCostingPlugIn jobCostingPlugIn = Consol;

			Consol.JK_RL_NKDischargePort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
			Consol.JK_RL_NKLoadPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;

			AssertEquals(IJobCostingPlugInHelper.GetDirectionCode(MasterFiles.Business.Directions.Domestic), jobCostingPlugIn.Direction);

			Consol.JK_RL_NKDischargePort = "USLAX";
			AssertEquals(IJobCostingPlugInHelper.GetDirectionCode(MasterFiles.Business.Directions.Export), jobCostingPlugIn.Direction);
		}

		#endregion

		#region Aviation Security

		public void TestSetMAWBIssueDateUpdateShipmentInspectionTypeCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				var shipper = Factory.LoadTop1<OrgHeader>(new ZQuery());
				var approval = shipper.MainAddress.KnownShipperDetails.AddNew();
				approval.OV_OH_OrgHeader = shipper.PK;
				approval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(-1);
				approval.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				approval.OV_EXApprovalNumber = "12345";

				var consol = Factory.New<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();
				shipment.JS_TransportMode = "AIR";
				shipment.ConsignorPK = shipper.PK;
				shipment.JS_RL_NKOrigin = "JPOSA";
				shipment.JS_RL_NKDestination = "USCHI";

				AssertEquals("Shipper's approval has expired so Inspection Type Code = 'UNK'", "UNK", shipment.JS_InspectionTypeCode);
				AssertEquals("No recalculation message as Inspection Type has its default value", ZString.Empty, shipment.MostRecentInspectionTypeChangeReason);

				consol.JK_MasterBillIssueDate = ZDate.Today.AddDays(-2);

				AssertEquals("MAWB was issued before the approval expired", "APP", shipment.JS_InspectionTypeCode);
				AssertEquals("Recalculation message as Inspection Type has been automatically calculated", "Master Bill Issue Date has been changed", shipment.MostRecentInspectionTypeChangeReason);

				consol.JK_MasterBillIssueDate = ZDate.Today;

				AssertEquals("MAWB issued after the approval expired", "UNK", shipment.JS_InspectionTypeCode);
				AssertEquals("Recalculation message as Inspection Type has been automatically calculated", "Master Bill Issue Date has been changed", shipment.MostRecentInspectionTypeChangeReason);
			}
		}

		#endregion

		#region Container Tracking subsciption test

		public void TestSaving_RequestTracking_NoShippingLine_NoSubscription()
		{
			TestSubscriptionCase(
				shippingLine: null,
				bookingReference: "12345678",
				isEventExpected: false);
		}

		public void TestSaving_RequestTracking_NoSCAC_RequestSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>(),
				bookingReference: "12345678",
				isEventExpected: true);
		}

		public void TestSaving_RequestTracking_NoBookingReference_NoMasterBill_NoSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US"),
				isEventExpected: false);
		}

		public void TestSaving_RequestTracking_InvalidBookingReference1_RequestSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US"),
				bookingReference: "1234567",
				isEventExpected: true);
		}

		public void TestSaving_RequestTracking_InvalidBookingReference2_RequestSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US"),
				bookingReference: "123456789012345678901",
				isEventExpected: true);
		}

		public void TestSaving_RequestTracking_InvalidBookingReference3_RequestSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US"),
				bookingReference: "1234567@",
				isEventExpected: true);
		}

		public void TestSaving_RequestTracking_DisabledTracking_NoSubscription()
		{
			TestSubscriptionCase(
				isTrackingEnabled: false,
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US"),
				bookingReference: "12345678",
				isEventExpected: false);
		}

		public void TestSaving_RequestTracking_InvalidContainerNumber_RequestSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US"),
				bookingReference: "12345678",
				containerNum: "AAAA",
				isEventExpected: true);
		}

		public void TestSaving_RequestTracking_InvalidSCAC_RequestSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "12345", "US"),
				bookingReference: "12345678",
				containerNum: "AAAA",
				isEventExpected: true);
		}

		public void TestSaving_RequestTracking_EverythingProvided_RequestSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US"),
				bookingReference: @"12345678901234567-/\",
				isEventExpected: true);
		}

		public void TestSaving_RequestTracking_ArrivalDateIsTooOld_WithSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US"),
				masterBillNumber: "12345678",
				containerNum: "AAAA1234566",
				isEventExpected: true,
				additionalSetup: (consol, container) =>
				{
					consol.Transports[0].JW_ATA = ZDateTime.Today.AddMonths(-2).AddDays(-1);
				});
		}

		public void TestSaving_RequestTracking_HasNoArrivalDate_DepartureDateIsTooOld_WithSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US"),
				masterBillNumber: "12345678",
				containerNum: "AAAA1234566",
				isEventExpected: true,
				additionalSetup: (consol, container) =>
				{
					consol.Transports[0].JW_ETA = ZDateTime.Empty;
					consol.Transports[0].JW_ETD = ZDateTime.Today.AddMonths(-6).AddDays(-1);
				}
			);
		}

		public void TestSaving_RequestTracking_ArrivalDateIsRecent_RequestSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US"),
				masterBillNumber: "12345678",
				containerNum: "AAAA1234566",
				isEventExpected: true,
				additionalSetup: (consol, container) => { consol.Transports[0].JW_ATA = 1.DaysAgo(); });
		}

		public void TestSaving_RequestTracking_HasDehireEvent_NoSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US"),
				masterBillNumber: "12345678",
				containerNum: "AAAA1234566",
				isEventExpected: false,
				additionalSetup: (consol, container) => { consol.Logs.AddNew(Events.Dehire); });
		}

		public void TestSaving_RequestTracking_SCACIsNotForUS_RequestSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "AU"),
				bookingReference: "12345678",
				isEventExpected: true);
		}

		public void TestSaving_RequestTracking_TransportModeIsNotSEA_NoSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US"),
				bookingReference: "12345678",
				transportMode: Constants.TransportModes.Air,
				isEventExpected: false);
		}

		public void TestSaving_RequestTracking_InvalidMasterBill1_RequestSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US"),
				bookingReference: "12345678",
				masterBillNumber: "Master1",
				isEventExpected: true);
		}

		public void TestSaving_RequestTracking_InvalidMasterBill2_RequestSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US"),
				bookingReference: "12345678",
				masterBillNumber: "Master1Master1Master1",
				isEventExpected: true);
		}

		public void TestSaving_RequestTracking_InvalidMasterBill3_RequestSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US"),
				bookingReference: "12345678",
				masterBillNumber: "Master1@",
				isEventExpected: true);
		}

		public void TestSaving_RequestTracking_Resave_NoBookingReference_NoMasterBill_CancelSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US"),
				eventAlreadyExists: true,
				isEventExpected: false); //TODO: Shouldn't we resend such events?
		}

		public void TestSaving_RequestTracking_Resave_NoSCAC_ResendSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>(),
				bookingReference: "12345678",
				eventAlreadyExists: true,
				isEventExpected: true);
		}

		public void TestSaving_RequestTracking_Resave_NoShippingLine_CancelSubscription()
		{
			TestSubscriptionCase(
				shippingLine: null,
				bookingReference: "12345678",
				eventAlreadyExists: true,
				isEventExpected: false); //TODO: Shouldn't we resend such events?
		}

		public void TestSaving_RequestTracking_Resave_EverythingProvided_ResendSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US"),
				bookingReference: "12345678",
				eventAlreadyExists: true,
				masterBillNumber: "MasterBill1",
				isEventExpected: true);
		}

		public void TestSaving_SubscriptionDetailsAreModified_RenewSubscription()
		{
			using (FreightDataRegistry.Instance.ContainerAutomation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var eventParameters = new[] { Enterprise.Freight.Business.Extensions.StringExtensions.AsKeyFor(EventConstants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.ContainerTracking) };
				var eventReference = StmALog.GenerateEventReference("", eventParameters);

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_OA_ShippingLineAddress = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US").MainAddress.PK;
				consol.Transports[0].JW_ETA = ZDateTime.Today;

				Factory.Save();
				var log0 = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				AssertNull("No container subscription If there is no masterbillnumber and BookingReference", log0);

				consol.JK_BookingReference = "12345678";
				Factory.Save();
				var log1 = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				AssertNotNull("Log", log1);

				// Booking reference change
				consol.JK_BookingReference = "23456789";
				Factory.Save();
				var log2 = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				Assert("Event has been recreated after booking reference change", log2.SL_EventTime > log1.SL_EventTime);
				AssertEquals("Event should have local datetime", ZDateTime.Now.ToSmallDateTime(), log2.SL_EventTime.ToSmallDateTime());

				// Shipping Line change
				consol.JK_OA_ShippingLineAddress = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "BBBB", "US").MainAddress.PK;
				Factory.Save();
				var log3 = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				Assert("Event has been recreated after shipping line change", log3.SL_EventTime > log2.SL_EventTime);

				// mawb was added. Send event. Will be suppress in eHub
				consol.MasterBillMAWB = "MAWB1234";
				Factory.Save();
				var log4 = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				Assert("Event has been recreated after mawb added", log4.SL_EventTime > log3.SL_EventTime);

				// mawb was added. Send event. Will be suppress in eHub. mawb was changed
				consol.MasterBillMAWB = "MAWB2345";
				Factory.Save();
				var log5 = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				Assert("Event has been recreated after mawb changed", log5.SL_EventTime > log4.SL_EventTime);

				// ContainerMode has changed
				consol.JK_ConsolMode = "XXX";
				Factory.Save();
				var log6 = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				Assert("Event has been recreated after ContainerMode changed", log6.SL_EventTime > log5.SL_EventTime);
			}
		}

		public void TestSaving_ContainerSubscriptionCreatedForCoLoad()
		{
			// Arrange
			using (FreightDataRegistry.Instance.ContainerAutomation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var eventParameters = new[] { Enterprise.Freight.Business.Extensions.StringExtensions.AsKeyFor(EventConstants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.ContainerTracking) };
				var eventReference = StmALog.GenerateEventReference("", eventParameters);

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
				consol.JK_OA_ShippingLineAddress = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US").MainAddress.PK;
				consol.Transports[0].JW_ETA = ZDateTime.Today;
				consol.MasterBillMAWB = "MAWB2345";

				int ConsolSBRCount()
				{
					return consol.Logs.GetAllLogs().Cast<StmALog>()
						.Count(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code);
				}

				AssertEquals("Prereq: 0 consol SBR events", 0, ConsolSBRCount());

				// Act
				Factory.Save();

				// Assert
				AssertEquals("1 consol SBR event", 1, ConsolSBRCount());

				// Act
				consol.JK_BookingReference = "12345678";
				Factory.Save();

				// Assert
				AssertEquals("Changing booking reference on CoLoad does not create SBR", 1, ConsolSBRCount());
			}
		}

		public void TestSaving_ContainerSubscription_CheckingConsolArrivalDepartureDatesAreNotTooOld_ETA()
		{
			var thresholdDate = ZDateTime.Today.AddMonths(-2);
			var notTooOld = thresholdDate.AddMinutes(1);
			using (FreightDataRegistry.Instance.ContainerAutomation.SetTemporaryValue(Guid.Empty, Guid.Empty,
				Guid.Empty, true))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_OA_ShippingLineAddress = Factory.NewWithValidTestData<OrgHeader>()
					.WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US").MainAddress.PK;
				consol.JK_BookingReference = "12345678";

				consol.Transports[0].JW_ETA = notTooOld;
				var anotherTransport = consol.Transports.AddNew();
				anotherTransport.JW_ATA = thresholdDate;

				Factory.Save();

				var eventReference = ContainerTrackingSubscriptionRequestedManager.SubscriptionRequestedEventReference;

				var log = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				AssertEquals("SBR must be created for consol departed more than three days ago", "SBR", ((ICodeDescription)log.Event).Code);
			}
		}

		public void TestSaving_ContainerSubscription_CheckingConsolArrivalDepartureDatesAreNotTooOld_ETD()
		{
			var thresholdDate = ZDateTime.Today.AddMonths(-6);
			var tooOld = thresholdDate.AddMinutes(-1);
			var notTooOld = thresholdDate.AddMinutes(1);
			using (FreightDataRegistry.Instance.ContainerAutomation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_OA_ShippingLineAddress = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US").MainAddress.PK;
				consol.JK_BookingReference = "12345678";

				consol.Transports[0].JW_ETD = tooOld;

				var anotherTransport = consol.Transports.AddNew();
				anotherTransport.JW_ETD = thresholdDate;

				Factory.Save();

				var eventReference = ContainerTrackingSubscriptionRequestedManager.SubscriptionRequestedEventReference;

				var log = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				AssertEquals("SBR must be created for consol departed more than three days ago", "SBR", ((ICodeDescription)log.Event).Code);

				consol.Transports[0].JW_ETD = notTooOld;
				consol.JK_BookingReference = "hitmebaby";
				Factory.Save();

				log = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				AssertEquals("SBR must be created for consol departed more than three days ago", "SBR", ((ICodeDescription)log.Event).Code);
			}
		}

		void TestSubscriptionCase(
			bool isTrackingEnabled = true,
			OrgHeader shippingLine = null,
			string bookingReference = "",
			string containerNum = null,
			string transportMode = Constants.TransportModes.Sea,
			bool eventAlreadyExists = false,
			bool isEventExpected = false,
			string masterBillNumber = "",
			Action<ForwardingConsol, ForwardingContainer> additionalSetup = null)
		{
			var eventParameters = new[] { Enterprise.Freight.Business.Extensions.StringExtensions.AsKeyFor(EventConstants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.ContainerTracking) };
			var eventReference = StmALog.GenerateEventReference("", eventParameters);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_BookingReference = bookingReference;
			consol.JK_TransportMode = transportMode;
			consol.JK_MasterBillNum = masterBillNumber;
			consol.JK_OA_ShippingLineAddress = shippingLine != null ? shippingLine.MainAddress.PK : ZGuid.Empty;

			consol.Transports[0].JW_ETA = ZDateTime.Today;

			ForwardingContainer container = null;
			if (containerNum != null)
			{
				container = consol.Containers.AddNew();
				container.JC_ContainerNum = containerNum;
			}

			if (eventAlreadyExists)
			{
				consol.Logs.AddNew(Events.SubscriptionRequested, eventReference, ZDateTimeOffset.Now, false);
			}

			additionalSetup?.Invoke(consol, container);

			using (FreightDataRegistry.Instance.ContainerAutomation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isTrackingEnabled))
			{
				Factory.Save();
			}

			var log = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
			AssertEquals("Event created", isEventExpected, log != null);
		}

		#endregion

		#region IContainerTrackingProvider

		public void TestTrackingProvider()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = "AGT";
			consol.JK_BookingReference = "BOOKS";
			consol.JK_CoLoadMasterBill = "CLDMB1";
			Factory.Save();

			var trackingProvider = consol as IContainerTrackingProvider;
			AssertEquals(false, trackingProvider.SubscribeToContainersOnlyHasChanges);
			AssertEquals(false, trackingProvider.CarrierBookingReferenceHasChanges);
			AssertEquals("BOOKS", trackingProvider.CarrierBookingReference);
			AssertEquals(false, trackingProvider.RoutingLegsHaveChanges);
			consol.JK_BookingReference = "CARS";
			AssertEquals(true, trackingProvider.CarrierBookingReferenceHasChanges);
			AssertEquals("CARS", trackingProvider.CarrierBookingReference);

			consol.JK_AgentType = "DRT";
			AssertEquals(true, trackingProvider.SubscribeToContainersOnlyHasChanges);
			consol.JK_AgentType = "CLD";
			Factory.Save();

			AssertEquals(false, trackingProvider.SubscribeToContainersOnlyHasChanges);
			AssertEquals(false, trackingProvider.CarrierBookingReferenceHasChanges);
			AssertEquals("CARS", trackingProvider.CarrierBookingReference);
			consol.JK_CoLoadMasterBill = "CLDMB2";
			AssertEquals(false, trackingProvider.CarrierBookingReferenceHasChanges);
			AssertEquals("CARS", trackingProvider.CarrierBookingReference);

			consol.JK_AgentType = "DRT";
			AssertEquals(true, trackingProvider.SubscribeToContainersOnlyHasChanges);
			consol.JK_AgentType = "AGT";
			AssertEquals(true, trackingProvider.SubscribeToContainersOnlyHasChanges);
			consol.JK_AgentType = "CLD";
			AssertEquals(false, trackingProvider.SubscribeToContainersOnlyHasChanges);
			Factory.Save();
			AssertEquals(false, trackingProvider.SubscribeToContainersOnlyHasChanges);

			var transport = consol.Transports.AddNew("DEFRA", "AUBNE");
			AssertEquals(true, trackingProvider.RoutingLegsHaveChanges);
			Factory.Save();
			transport.JW_RL_NKDiscPort = "AUMEL";
			AssertEquals(true, trackingProvider.RoutingLegsHaveChanges);
			Factory.Save();
			transport.JW_VoyageFlight = "122";
			AssertEquals(true, trackingProvider.RoutingLegsHaveChanges);
			Factory.Save();
			transport.JW_VoyageFlight = "122S";
			AssertEquals(true, trackingProvider.RoutingLegsHaveChanges);
			Factory.Save();
			consol.Transports.Remove(transport);
			AssertEquals(true, trackingProvider.RoutingLegsHaveChanges);
		}

		#endregion

		#region Events/Milestones

		public void TestMilestonesNotDuplicatedWhenSavingConsolInDifferentLoggedInCompanies_WithDepartureEvent()
		{
			var processTaskTemplates = Factory.Load<ProcessTaskTemplate>(new ZQuery());

			foreach (var processTaskTemplate in processTaskTemplates)
			{
				processTaskTemplate.P0_IsActive = false;
			}

			Factory.Save();

			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_Name = "Consol Template";
			template.P0_ProcessType = "CON";
			template.P0_GC = ZGuid.Empty;

			var templateProcessTask1 = template.WorkflowItems.Milestones.AddNew();
			templateProcessTask1.P9_Description = "Departure from First Load Port";
			templateProcessTask1.TriggerConditions.TriggerEventCode = "DEP";
			templateProcessTask1.P9_SE_NKExceptionEvent = "EXC";

			var templateProcessTask2 = template.WorkflowItems.Milestones.AddNew();
			templateProcessTask2.P9_Description = "1st Transport Leg Arrival";
			templateProcessTask2.TriggerConditions.TriggerEventCode = "ARV";
			templateProcessTask2.P9_SE_NKExceptionEvent = "EXC";
			templateProcessTask2.TemplateConditions.TemplateCondition1 = "2LG";

			var templateProcessTask3 = template.WorkflowItems.Milestones.AddNew();
			templateProcessTask3.P9_Description = "2nd Transport Leg Departure";
			templateProcessTask3.TriggerConditions.TriggerEventCode = "DEP";
			templateProcessTask3.P9_SE_NKExceptionEvent = "EXC";
			templateProcessTask3.TemplateConditions.TemplateCondition1 = "2LG";

			var templateProcessTask4 = template.WorkflowItems.Milestones.AddNew();
			templateProcessTask4.P9_Description = "3rd Transport Leg Departure";
			templateProcessTask4.TriggerConditions.TriggerEventCode = "ARV";
			templateProcessTask4.P9_SE_NKExceptionEvent = "EXC";
			templateProcessTask4.TemplateConditions.TemplateCondition1 = "3LG";

			var templateProcessTask5 = template.WorkflowItems.Milestones.AddNew();
			templateProcessTask5.P9_Description = "3rd Transport Leg Departure";
			templateProcessTask5.TriggerConditions.TriggerEventCode = "DEP";
			templateProcessTask5.P9_SE_NKExceptionEvent = "EXC";
			templateProcessTask5.TemplateConditions.TemplateCondition1 = "3LG";

			var templateProcessTask6 = template.WorkflowItems.Milestones.AddNew();
			templateProcessTask6.P9_Description = "4rd Transport Leg Departure";
			templateProcessTask6.TriggerConditions.TriggerEventCode = "ARV";
			templateProcessTask6.P9_SE_NKExceptionEvent = "EXC";
			templateProcessTask6.TemplateConditions.TemplateCondition1 = "4LG";

			var templateProcessTask7 = template.WorkflowItems.Milestones.AddNew();
			templateProcessTask7.P9_Description = "4rd Transport Leg Departure";
			templateProcessTask7.TriggerConditions.TriggerEventCode = "DEP";
			templateProcessTask7.P9_SE_NKExceptionEvent = "EXC";
			templateProcessTask7.TemplateConditions.TemplateCondition1 = "4LG";

			var templateProcessTask8 = template.WorkflowItems.Milestones.AddNew();
			templateProcessTask8.P9_Description = "Arrival at Final Discharge Port";
			templateProcessTask8.TriggerConditions.TriggerEventCode = "ARV";
			templateProcessTask8.P9_SE_NKExceptionEvent = "EXC";

			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "USLAX";
			transport1.JW_RL_NKDiscPort = "DEFRA";
			transport1.JW_TransportType = "MAI";

			var transport2 = consol.Transports.AddNew("DEFRA", "AUBNE");
			transport2.JW_TransportType = "OTH";

			var transport3 = consol.Transports.AddNew("AUBNE", "AUSYD");
			transport3.JW_TransportType = "OTH";

			var transport4 = consol.Transports.AddNew("AUSYD", "NZAKL");
			transport4.JW_TransportType = "OTH";

			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "NZAKL";

			Factory.Save();

			var consolProcessTasks = Factory.Load<ForwardingConsolProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, consol.PK));
			AssertEquals(8, consolProcessTasks.Length);

			var anotherCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual,
					GlbCompany.CurrentCompany.PK));

			Assert(anotherCompany.Branches.Count > 0);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, anotherCompany.Branches[0].PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var newFactory = new BusinessObjectFactory();
				consol = newFactory.Load<ForwardingConsol>(consol.PK);
				consol.JK_AgentsReference = "Change";

				newFactory.Save();

				consolProcessTasks = newFactory.Load<ForwardingConsolProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, consol.PK));
				AssertEquals(8, consolProcessTasks.Length);
			}
		}

		public void TestMilestonesNotDuplicatedWhenSavingConsolInDifferentLoggedInCompanies_WithCutOffEvent()
		{
			var processTaskTemplates = Factory.Load<ProcessTaskTemplate>(new ZQuery());

			foreach (var processTaskTemplate in processTaskTemplates)
			{
				processTaskTemplate.P0_IsActive = false;
			}

			Factory.Save();

			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_Name = "Consol Template";
			template.P0_ProcessType = "CON";
			template.P0_GC = ZGuid.Empty;

			var templateProcessTask1 = template.WorkflowItems.Milestones.AddNew();
			templateProcessTask1.P9_Description = "Departure from First Load Port";
			templateProcessTask1.TriggerConditions.TriggerEventCode = "COF";
			templateProcessTask1.P9_SE_NKExceptionEvent = "EXC";

			var templateProcessTask2 = template.WorkflowItems.Milestones.AddNew();
			templateProcessTask2.P9_Description = "1st Transport Leg Arrival";
			templateProcessTask2.TriggerConditions.TriggerEventCode = "ARV";
			templateProcessTask2.P9_SE_NKExceptionEvent = "EXC";
			templateProcessTask2.TemplateConditions.TemplateCondition1 = "2LG";

			var templateProcessTask3 = template.WorkflowItems.Milestones.AddNew();
			templateProcessTask3.P9_Description = "2nd Transport Leg Departure";
			templateProcessTask3.TriggerConditions.TriggerEventCode = "COF";
			templateProcessTask3.P9_SE_NKExceptionEvent = "EXC";
			templateProcessTask3.TemplateConditions.TemplateCondition1 = "2LG";

			var templateProcessTask4 = template.WorkflowItems.Milestones.AddNew();
			templateProcessTask4.P9_Description = "3rd Transport Leg Departure";
			templateProcessTask4.TriggerConditions.TriggerEventCode = "ARV";
			templateProcessTask4.P9_SE_NKExceptionEvent = "EXC";
			templateProcessTask4.TemplateConditions.TemplateCondition1 = "3LG";

			var templateProcessTask5 = template.WorkflowItems.Milestones.AddNew();
			templateProcessTask5.P9_Description = "3rd Transport Leg Departure";
			templateProcessTask5.TriggerConditions.TriggerEventCode = "COF";
			templateProcessTask5.P9_SE_NKExceptionEvent = "EXC";
			templateProcessTask5.TemplateConditions.TemplateCondition1 = "3LG";

			var templateProcessTask6 = template.WorkflowItems.Milestones.AddNew();
			templateProcessTask6.P9_Description = "4rd Transport Leg Departure";
			templateProcessTask6.TriggerConditions.TriggerEventCode = "ARV";
			templateProcessTask6.P9_SE_NKExceptionEvent = "EXC";
			templateProcessTask6.TemplateConditions.TemplateCondition1 = "4LG";

			var templateProcessTask7 = template.WorkflowItems.Milestones.AddNew();
			templateProcessTask7.P9_Description = "4rd Transport Leg Departure";
			templateProcessTask7.TriggerConditions.TriggerEventCode = "COF";
			templateProcessTask7.P9_SE_NKExceptionEvent = "EXC";
			templateProcessTask7.TemplateConditions.TemplateCondition1 = "4LG";

			var templateProcessTask8 = template.WorkflowItems.Milestones.AddNew();
			templateProcessTask8.P9_Description = "Arrival at Final Discharge Port";
			templateProcessTask8.TriggerConditions.TriggerEventCode = "ARV";
			templateProcessTask8.P9_SE_NKExceptionEvent = "EXC";

			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "USLAX";
			transport1.JW_RL_NKDiscPort = "DEFRA";
			transport1.JW_TransportType = "MAI";

			var transport2 = consol.Transports.AddNew("DEFRA", "AUBNE");
			transport2.JW_TransportType = "OTH";

			var transport3 = consol.Transports.AddNew("AUBNE", "AUSYD");
			transport3.JW_TransportType = "OTH";

			var transport4 = consol.Transports.AddNew("AUSYD", "NZAKL");
			transport4.JW_TransportType = "OTH";

			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "NZAKL";

			Factory.Save();

			var consolProcessTasks = Factory.Load<ForwardingConsolProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, consol.PK));
			AssertEquals(8, consolProcessTasks.Length);

			var anotherCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual,
					GlbCompany.CurrentCompany.PK));

			Assert(anotherCompany.Branches.Count > 0);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, anotherCompany.Branches[0].PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var newFactory = new BusinessObjectFactory();
				consol = newFactory.Load<ForwardingConsol>(consol.PK);
				consol.JK_AgentsReference = "Change";

				newFactory.Save();

				consolProcessTasks = newFactory.Load<ForwardingConsolProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, consol.PK));
				AssertEquals(8, consolProcessTasks.Length);
			}
		}

		public void TestTriggersAreAddedWhenDifferentTriggerAlreadyInDb()
		{
			var descriptionUnrestricted = "Trigger";
			var description2lg = "Trigger-2LG";

			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_Name = "Container Template";
			template.P0_ProcessType = "CNT";
			template.P0_GC = ZGuid.Empty;

			var processTask1 = template.WorkflowItems.Triggers.AddNew();
			processTask1.P9_Description = descriptionUnrestricted;
			processTask1.TriggerConditions.TriggerEventCode = "GIM";

			var processTask2 = template.WorkflowItems.Triggers.AddNew();
			processTask2.P9_Description = description2lg;
			processTask2.TriggerConditions.TriggerEventCode = "GIM";
			processTask2.TriggerConditions.TriggerCondition = "RFP";
			processTask2.TemplateConditions.TemplateCondition1 = "2LG";

			Factory.Save(); // note: this line is required

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "USLAX";
			transport1.JW_RL_NKDiscPort = "NZAKL";
			transport1.JW_TransportType = "MAI";

			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var container = consol.Containers.AddNew();
			Factory.Save();

			var containerProcessTasks = Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, container.PK));
			AssertEquals(1, containerProcessTasks.Length);
			AssertEquals("Should contain non-restricted trigger", processTask1.LongDescription, containerProcessTasks.First().LongDescription);

			transport1.JW_RL_NKDiscPort = "AUBNE";
			var transport2 = consol.Transports.AddNew("AUBNE", "NZAKL");
			transport2.JW_TransportType = "OTH";

			Factory.Save();
			containerProcessTasks = Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, container.PK));
			AssertEquals(2, containerProcessTasks.Length);
			var descriptions = containerProcessTasks.Select(t => t.LongDescription);
			AssertContainsExactElementsInAnyOrder("Should now also contain the restricted (2LG - 2 leg) trigger", new[] { processTask1.LongDescription, processTask2.LongDescription }, descriptions);
		}

		public void TestConsolTransportReceiveDepartureEventWithoutParameters_ParametersUpdatedOnSaving_TriggerMilestoneUpdate()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var departureMilestone = consol.WorkflowItems.Milestones.AddNew();
			departureMilestone.TriggerConditions.TriggerEventCode = Events.DepartureCode;
			departureMilestone.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			departureMilestone.TriggerConditions.TriggerConditionValue = "LOC=<FirstLeg.Origin>";

			Factory.Save();

			var transport = consol.MostInterestingTransportForBinding[0];
			AssertEquals("prerequisite", true, transport.JW_ATD.IsEmpty);
			AssertEquals("prerequisite", true, departureMilestone.P9_ActualDate.IsEmpty);

			var actualDepartureTime = ZDateTimeOffset.Now.AddMinutes(-10);
			transport.Logs.AddNew(Events.Departure, actualDepartureTime);
			AssertEquals("Milestone actual time not updated: parameters not matched", true, departureMilestone.P9_ActualDate.IsEmpty);

			Factory.Save();

			var departureLog = transport.Logs.MostRecentLogByEventTime(Events.Departure);
			AssertEquals("Transport event log was updated with current parameters on saving", "AUSYD", departureLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
			AssertEquals("Milestone actual time updated: parameters matched", actualDepartureTime, departureMilestone.P9_ActualDateForBinding);
		}

		#endregion

		public void TestSystemOnlyAdditionalReferenceNumber()
		{
			var consol = Factory.New<ForwardingConsol>();
			var entryTypesForSystemOnly = new ZString[]
			{
				ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierShipperReference,
				ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierMessageReference
			};

			foreach (var entryType in entryTypesForSystemOnly)
			{
				var entryNum = consol.Numbers.AddNew();
				entryNum.CE_EntryType = entryType;
				entryNum.CE_EntryIsSystemGenerated = true;
			}

			Factory.Save();

			foreach (var entryType in entryTypesForSystemOnly)
			{
				var reloadedConsol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
				var reloadedEntryNum = reloadedConsol.Numbers.Cast<CusEntryNumber>().First(x => x.CE_EntryType == entryType);
				AssertEquals(entryType, reloadedEntryNum.CE_EntryType);

				Assert($"{entryType} additional ref number should be read-only after loading", reloadedEntryNum.ReadOnly);
				Assert($"{entryType} cannot be deleted", !reloadedEntryNum.CanDelete);
				AssertEquals($"The {entryType} is system generated and cannot be deleted.", reloadedEntryNum.ReasonForNotAbleToDelete);
			}
		}

		public void TestAddRulesToNotes()
		{
			var rule1 = Factory.NewWithValidTestData<RefCountryRules>();
			rule1.R7_RN_NKOrigin = "AU";
			rule1.R7_RN_NKDestination = "DE";
			rule1.R7_Notes = "This is a client visible Notes";
			rule1.R7_IsClientVisible = ZBool.True;

			var rule2 = Factory.NewWithValidTestData<RefCountryRules>();
			rule2.R7_RN_NKOrigin = "AU";
			rule2.R7_RN_NKDestination = "DE";
			rule2.R7_Notes = "This is an internal Notes";
			rule2.R7_IsClientVisible = ZBool.False;

			Factory.Save();

			Consol.JK_RL_NKLoadPort = "AUBNE";
			Consol.JK_RL_NKDischargePort = "DEHAM";

			Factory.Save();
			Assert(Consol.Notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRules.Description).FirstOrDefault().ST_NoteText.Contains("This is a client visible Notes"));
			Assert(Consol.Notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRulesInternal.Description).FirstOrDefault().ST_NoteText.Contains("This is an internal Notes"));
		}

		#region TestJK_Calc_DGClass

		public void TestJK_Calc_DGClass()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_IsHazardous = true;

			var consolDGRestrictions = consol.ConsolDGRestrictionCollection.AddNew();
			consolDGRestrictions.JKD_Class = "2.1";
			Factory.Save();

			AssertEquals("Single UNDG Class", "2.1", consol.JK_Calc_DGClass);

			var consolDGRestrictions2 = consol.ConsolDGRestrictionCollection.AddNew();
			consolDGRestrictions2.JKD_Class = "2.1";
			Factory.Save();

			AssertEquals("Non-distinct multiple UNDG Classes", "2.1", consol.JK_Calc_DGClass);

			var consolDGRestrictions3 = consol.ConsolDGRestrictionCollection.AddNew();
			consolDGRestrictions3.JKD_Class = "6.1";
			Factory.Save();

			AssertEquals("Multiple types of UNDG Class", "Mixed", consol.JK_Calc_DGClass);
		}

		public void TestDefaultCarrierBookingAgent_OrgAddressIsNull_NoExceptionThrow()
		{
			var orgWithNoAgencies = Factory.NewWithValidTestData<OrgHeader>();
			var orgWithAgencies = Factory.NewWithValidTestData<OrgHeader>();

			var angency = orgWithAgencies.CarrierAppointedAgentPorts_Agency.AddNew();
			angency.O5_PortOrCountry = "USORD";
			angency.O5_OA_AgentOfficeAddress = orgWithAgencies.MainAddress.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "USORD";
			consol.JK_OA_ShippingLineAddress = orgWithNoAgencies.MainAddress.PK;
			consol.BookingAgentDefaultingAsker = new ConfirmationPrompt();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Vessel";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = TransportModes.Sea;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "Flight";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "USORD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.GenerateSailings();

			var transport = (Transport)consol.Transports.First();
			transport.JW_OA_CarrierAddress = orgWithNoAgencies.MainAddress.PK;
			transport.JW_Vessel = Factory.NewWithValidTestData<RefVessel>().RV_FK;
			transport.JW_RL_NKLoadPort = "USORD";
			transport.JW_IsLinked = true;
			transport.JW_JX = voyage.Sailings[0].PK;

			Factory.Save();

			var carrierBookingAgentAddress = consol.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.CarrierBookingAgent);

			AssertEquals("Precondition", false, carrierBookingAgentAddress.IsInDatabase);
			AssertNull("Precondition", carrierBookingAgentAddress.Address);

			AssertNoExceptionThrown(() =>
			{
				transport.JW_OA_CarrierAddress = orgWithAgencies.MainAddress.PK;
			});

			AssertNotNull("Address now have value", carrierBookingAgentAddress.Address);
			AssertEquals("Default to the related Agent Office Address", orgWithAgencies.MainAddress.PK, carrierBookingAgentAddress.Address.PK);
		}

		public void TestDefaultTransportStatusWhenTransportModeChangedToAir()
		{
			var consol = Factory.New<ForwardingConsol>();
			var transport = (Transport)consol.Transports.First();
			Assert("precondition: default TransportMode is not Air", consol.TransportMode != TransportModes.Air);

			consol.JK_TransportMode = TransportModes.Air;
			Assert("Transport status should be PLN for air transport", transport.JW_Status == TransportStatus.Planned);

			transport.JW_Status = TransportStatus.Confirmed;
			Factory.Save();

			var transportNew = consol.Transports.AddNew();
			Assert("New Transport status should be PLN for air transport", transportNew.JW_Status == TransportStatus.Planned);
			Assert("Saved Transport status should be unchanged", transport.JW_Status == TransportStatus.Confirmed);
		}

		#endregion

		#region TestJK_Calc_DGSubstance

		public void TestJK_Calc_DGSubstance()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_IsHazardous = true;

			var consolDGRestrictions = consol.ConsolDGRestrictionCollection.AddNew();
			consolDGRestrictions.JKD_Calc_Substance = "3258A";
			Factory.Save();

			AssertEquals("Single UNDG Substance", "3258A", consol.JK_Calc_DGSubstance);

			var consolDGRestrictions2 = consol.ConsolDGRestrictionCollection.AddNew();
			consolDGRestrictions2.JKD_Calc_Substance = "3258A";
			Factory.Save();

			AssertEquals("Non-distinct UNDG Substances", "3258A", consol.JK_Calc_DGSubstance);

			var consolDGRestrictions3 = consol.ConsolDGRestrictionCollection.AddNew();
			consolDGRestrictions3.JKD_Calc_Substance = "3020A";
			Factory.Save();

			AssertEquals("Multiple types of UNDG substance", "Mixed", consol.JK_Calc_DGSubstance);
		}

		#endregion

		#region Temperature Controlled Cargo Tests

		public void TestShipmentTemperatureRangeIsValid()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var packline = shipment.OuterPackLines.AddNew();

			consol.JK_RequiresTemperatureControl = false;
			packline.JL_RequiresTemperatureControl = false;

			Assert("Temperature control not required", consol.ShipmentTemperatureRangeIsValid(shipment));

			packline.JL_RequiresTemperatureControl = true;
			packline.JL_RequiredTemperatureMinimum = 5;
			packline.JL_RequiredTemperatureMaximum = 10;
			packline.JL_RequiredTemperatureUnit = "C";

			Assert("Temperature controlled packline on non-temperature controlled consol", !consol.ShipmentTemperatureRangeIsValid(shipment));

			consol.JK_RequiresTemperatureControl = true;
			consol.JK_RequiredTemperatureMinimum = 2;
			consol.JK_RequiredTemperatureMaximum = 12;
			consol.JK_RequiredTemperatureUnit = "C";

			Assert("Consol temperature bounds outside packline temperature bounds", !consol.ShipmentTemperatureRangeIsValid(shipment));

			consol.JK_RequiredTemperatureMinimum = 7;
			consol.JK_RequiredTemperatureMaximum = 9;

			Assert("Consol temperature bounds inside packline temperature bounds", consol.ShipmentTemperatureRangeIsValid(shipment));

			consol.JK_RequiredTemperatureUnit = "F";

			Assert("Consol temperature bounds outside packline temperature bounds after unit conversion", !consol.ShipmentTemperatureRangeIsValid(shipment));

			consol.JK_RequiredTemperatureUnit = "X";

			Assert("Invalid consol temperature unit", !consol.ShipmentTemperatureRangeIsValid(shipment));

			consol.JK_RequiredTemperatureUnit = "C";
			packline.JL_RequiredTemperatureUnit = "X";

			Assert("Invalid packline temperature unit", !consol.ShipmentTemperatureRangeIsValid(shipment));

			consol.JK_RequiredTemperatureMinimum = 2;
			consol.JK_RequiredTemperatureMaximum = 12;

			packline.JL_RequiredTemperatureMinimum = 5;
			packline.JL_RequiredTemperatureMaximum = 10;
			packline.JL_RequiredTemperatureUnit = "C";

			var packlineTwo = shipment.OuterPackLines.AddNew();
			packlineTwo.JL_RequiresTemperatureControl = true;
			packlineTwo.JL_RequiredTemperatureMinimum = -1;
			packlineTwo.JL_RequiredTemperatureMaximum = 4;
			packlineTwo.JL_RequiredTemperatureUnit = "C";

			Assert("Consol temperature bounds outside two packline temperature", !consol.ShipmentTemperatureRangeIsValid(shipment));

			packlineTwo.JL_RequiresTemperatureControl = true;
			packlineTwo.JL_RequiredTemperatureMinimum = 1;
			packlineTwo.JL_RequiredTemperatureMaximum = 13;
			packlineTwo.JL_RequiredTemperatureUnit = "C";
			Assert("Consol temperature bounds inside one packline temperature, but outside another packline temperature", !consol.ShipmentTemperatureRangeIsValid(shipment));

			packline.JL_RequiredTemperatureMinimum = 0;
			packline.JL_RequiredTemperatureMaximum = 14;
			packline.JL_RequiredTemperatureUnit = "C";

			Assert("Consol temperature bounds inside both two packline temperature bounds", consol.ShipmentTemperatureRangeIsValid(shipment));
		}

		#endregion

		public void TestShipmentDangerousGoodsIsCompatible_DG_CodeOrdinalIgnoreCase()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_IsHazardous = true;

			var consolDGRestrictions = consol.ConsolDGRestrictionCollection.AddNew();
			consolDGRestrictions.JKD_Calc_Substance = "3258A";
			consolDGRestrictions.JKD_Class = "3";

			var substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = "3258";
			substance.DG_Variant = "a";
			substance.DG_Standard = "IAT";

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "3258";
			subs.DG_Variant = "a";
			subs.DG_Standard = "IAT";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			var packline = shipment.OuterPackLines.AddNew();
			var undg = packline.UNDGs.AddNew();
			undg.LinkDefault(substance);
			undg.DI_IMOClass = "3";

			var result = consol.ShipmentDangerousGoodsIsCompatible(shipment);
			AssertEquals("DG_Code is case insensitive", true, result);
		}

		public void TestAllowsForShipmentsDangerousGoods_FalseWhenShipmentIsHazardousForNonHazardousConsol()
		{
			// Arrange
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_IsHazardous = false;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_RH_NKCommodityCode = "HAZ";

			// Act
			var result = consol.AllowsForShipmentsDangerousGoods(shipment);

			// Assert
			Assert("Non-Hazardous consol should not allow hazardous shipment", !result);
		}

		#region TestITransitWarehouseInstructionSupporter

		public void TestITransitWarehouseInstructionSupporter()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_OA_PackDepotAddress = Factory.New<OrgAddress>().PK;
			consol.JK_OA_UnpackDepotAddress = Factory.New<OrgAddress>().PK;

			var supporter = consol as ITransitWarehouseInstructionSupporter;
			AssertEquals("PickupTransitWarehouse", consol.JK_OA_PackDepotAddress, supporter.PickupTransitWarehouse.PK);
			AssertEquals("DeliveryTransitWarehouse", consol.JK_OA_UnpackDepotAddress, supporter.DeliveryTransitWarehouse.PK);

			supporter.PickupReceiptRequestedDate = ZDate.Today;
			supporter.PickupDispatchRequestedDate = ZDate.Today.AddDays(1);
			supporter.DeliveryReceiptRequestedDate = ZDate.Today.AddDays(2);
			supporter.DeliveryDispatchRequestedDate = ZDate.Today.AddDays(3);

			AssertEquals("Pickup Receipt Date", consol.JK_PackDepotReceiptRequested, ZDate.Today);
			AssertEquals("Pickup Dispatch Date", consol.JK_PackDepotDispatchRequested, ZDate.Today.AddDays(1));
			AssertEquals("Delivery Receipt Date", consol.JK_UnpackDepotReceiptRequested, ZDate.Today.AddDays(2));
			AssertEquals("Delivery Dispatch Date", consol.JK_UnpackDepotDispatchRequested, ZDate.Today.AddDays(3));

			AssertEquals("Departure", supporter.PickupDescription);
			AssertEquals("Arrival", supporter.DeliveryDescription);
			AssertEquals("CFS", supporter.TransitWarehouseDescription);
		}

		#endregion

		public void TestConsolsExemptFromCONEntryTypeUniquenessValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			var duplicateEntryTypeMessage = "The Number Type has been duplicated and must be unique.";

			var referenceNumbers = FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.Value;
			var conReferenceNumber = referenceNumbers
				.OfType<CustomsReferenceNumberType>()
				.First(n => n.Code == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON);
			conReferenceNumber.IsUnique = true;

			using (FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, referenceNumbers))
			{
				var entryNumCON1 = consol.Numbers.AddNew();
				entryNumCON1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON;
				entryNumCON1.CE_EntryNum = "6969";

				var entryNumCON2 = consol.Numbers.AddNew();
				entryNumCON2.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON;
				entryNumCON2.CE_EntryNum = "420420";

				Factory.Save();

				AssertNoError("CON numbers should not be checked for uniqueness on a consol.", entryNumCON2.CE_EntryTypeInfo, duplicateEntryTypeMessage);

				entryNumCON1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.COC;
				entryNumCON2.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.COC;

				AssertHasError("Other number types besides CON should still be checked for uniqueness on a consol.", entryNumCON2.CE_EntryTypeInfo, duplicateEntryTypeMessage);
			}
		}

		public void TestHasContainerizedDGs()
		{
			var substance = DGSubstanceTestHelper.Create("1234", "a", "IMO");
			var consol = (ForwardingConsol)GetNewConsol();
			var container = consol.Containers.AddNew();
			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			var dgItem = packLine.UNDGs.AddNew();
			dgItem.DI_DG = substance.PK;
			container.RemovePackLine(packLine);

			AssertEquals("Precondition: container has no packlines packed", 0, container.PackLines.Count);
			AssertEquals("no packlines have be packed", false, consol.HasContainerizedDGs);

			container.AddPackLine(packLine);
			AssertEquals("a packline with dgs has been packed", true, consol.HasContainerizedDGs);

			packLine.UNDGs.RemoveAllFromRelationship();
			AssertEquals("a packline without dgs has been packed", false, consol.HasContainerizedDGs);
		}

		#region ShowSecureContainerReleaseDocument

		public void TestHasSecureContainerReleaseEvent()
		{
			var consol = (ForwardingConsol)GetNewConsol();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "MAUE6541121";
			var shipment = consol.Shipments.AddNew();

			AssertEquals("Consol as no valid Secure Container Release containers for Transfer", false, consol.HasSecureContainerReleaseAuthorisedEvent);
			AssertEquals("Consol as no valid Secure Container Release containers for Revoke", false, consol.HasSecureContainerReleaseMessageAcceptedEvent);

			var eventParameters = new[]
				{
					new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.MessageType, "Secure Container Release")
				};
			container.Logs.AddNew(Events.Authorised, eventParameters);
			eventParameters = new[]
				{
					new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.MessageType, "Secure Container Release - Transfer")
				};
			container.Logs.AddNew(Events.MessageAccepted, eventParameters);

			AssertEquals("Consol as valid Secure Container Release containers for Transfer", true, consol.HasSecureContainerReleaseAuthorisedEvent);
			AssertEquals("Consol as valid Secure Container Release containers for Revoke", true, consol.HasSecureContainerReleaseMessageAcceptedEvent);
		}

		public void TestHasSecureContainerReleaseTransfer()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "BOL_Reference";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1111111";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONT2222222";

			Assert(!consol.HasSecureContainerReleaseTransfer);

			SecureContainerReleaseContainerEventHelperTest.AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.Assigned);
			Assert(consol.HasSecureContainerReleaseTransfer);

			SecureContainerReleaseContainerEventHelperTest.AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.Accepted);
			Assert(!consol.HasSecureContainerReleaseTransfer);

			SecureContainerReleaseContainerEventHelperTest.AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.TransferSentAwaitingResponse);
			Assert(consol.HasSecureContainerReleaseTransfer);

			SecureContainerReleaseContainerEventHelperTest.AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.TransferSent);
			Assert(consol.HasSecureContainerReleaseTransfer);

			SecureContainerReleaseContainerEventHelperTest.AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.TransferRejected);
			Assert(consol.HasSecureContainerReleaseTransfer);

			SecureContainerReleaseContainerEventHelperTest.AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.RevokeSentAwaitingResponse);
			Assert(!consol.HasSecureContainerReleaseTransfer);

			SecureContainerReleaseContainerEventHelperTest.AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.RevokeSent);
			Assert(!consol.HasSecureContainerReleaseTransfer);

			SecureContainerReleaseContainerEventHelperTest.AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.RevokeRejected);
			Assert(!consol.HasSecureContainerReleaseTransfer);

			SecureContainerReleaseContainerEventHelperTest.AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.Revoked);
			Assert(consol.HasSecureContainerReleaseTransfer);
		}

		public void TestHasSecureContainerReleaseRevoke()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "BOL_Reference";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1111111";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONT2222222";

			Assert(!consol.HasSecureContainerReleaseRevoke);

			SecureContainerReleaseContainerEventHelperTest.AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.Assigned);
			Assert(!consol.HasSecureContainerReleaseRevoke);

			SecureContainerReleaseContainerEventHelperTest.AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.Accepted);
			Assert(consol.HasSecureContainerReleaseRevoke);

			SecureContainerReleaseContainerEventHelperTest.AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.TransferSentAwaitingResponse);
			Assert(!consol.HasSecureContainerReleaseRevoke);

			SecureContainerReleaseContainerEventHelperTest.AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.TransferSent);
			Assert(!consol.HasSecureContainerReleaseRevoke);

			SecureContainerReleaseContainerEventHelperTest.AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.TransferRejected);
			Assert(!consol.HasSecureContainerReleaseRevoke);

			SecureContainerReleaseContainerEventHelperTest.AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.RevokeSentAwaitingResponse);
			Assert(consol.HasSecureContainerReleaseRevoke);

			SecureContainerReleaseContainerEventHelperTest.AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.RevokeSent);
			Assert(consol.HasSecureContainerReleaseRevoke);

			SecureContainerReleaseContainerEventHelperTest.AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.RevokeRejected);
			Assert(consol.HasSecureContainerReleaseRevoke);

			SecureContainerReleaseContainerEventHelperTest.AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.Revoked);
			Assert(!consol.HasSecureContainerReleaseRevoke);
		}

		#endregion

		#region ForwarderAddressWithContact

		public void TestJK_OC_SendingForwarderContact_Defaulting()
		{
			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			sendingForwarder.OH_IsCreditor = true;

			var sendingForwarderAddress = Factory.NewWithValidTestData<OrgAddress>();
			sendingForwarderAddress.OA_Code = "SNDFORADR";
			sendingForwarderAddress.OA_OH = sendingForwarder.PK;

			var sendingForwarderContact = sendingForwarder.Contacts.AddNew();
			sendingForwarderContact.OC_ContactName = "Shadowy Super Coder";
			sendingForwarderContact.OC_Email = "shadowy@dontexist.zz";
			sendingForwarderContact.WorkingAddressPK = sendingForwarderAddress.PK;

			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_OA_SendingForwarderAddress = sendingForwarderAddress.PK;

			Factory.Save();

			AssertEquals("JK_OC_SendingForwarderContact should be defaulted", sendingForwarderContact.PK, consol.JK_OC_SendingForwarderContact);

			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			consol.JK_OC_SendingForwarderContact = ZGuid.Empty;
			Factory.Save();

			consol.JK_OA_SendingForwarderAddress = sendingForwarderAddress.PK;
			Factory.Save();
			AssertEquals("JK_OC_SendingForwarderContact should be defaulted", sendingForwarderContact.PK, consol.JK_OC_SendingForwarderContact);
		}

		public void TestJK_OC_ReceivingForwarderContact_Defaulting()
		{
			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			receivingForwarder.OH_IsCreditor = true;

			var receivingForwarderContact = receivingForwarder.Contacts.AddNew();
			receivingForwarderContact.OC_ContactName = "Shadowy Super Coder";
			receivingForwarderContact.OC_Email = "shadowy@dontexist.zz";

			var receivingForwarderAddress = Factory.NewWithValidTestData<OrgAddress>();
			receivingForwarderAddress.OA_Code = "SNDFORADR";
			receivingForwarderAddress.OA_OH = receivingForwarder.PK;
			receivingForwarderContact.WorkingAddressPK = receivingForwarderAddress.PK;

			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarderAddress.PK;

			Factory.Save();

			AssertEquals("JK_OA_ReceivingForwarderAddress should be defaulted", receivingForwarderContact.PK, consol.JK_OC_ReceivingForwarderContact);

			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			consol.JK_OC_ReceivingForwarderContact = ZGuid.Empty;
			Factory.Save();

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarderAddress.PK;
			Factory.Save();
			AssertEquals("JK_OC_SendingForwarderContact should be defaulted", receivingForwarderContact.PK, consol.JK_OC_ReceivingForwarderContact);
		}

		public void TestReceivingForwarderWithContact_DefaultContact_SingleMatch()
		{
			AssertForwarderAddressWithContact_DefaultContact_SingleMatch(consol => consol.ReceivingForwarderWithContact);
		}

		public void TestSendingForwarderWithContact_DefaultContact_SingleMatch()
		{
			AssertForwarderAddressWithContact_DefaultContact_SingleMatch(consol => consol.SendingForwarderWithContact);
		}

		public void TestReceivingForwarderWithContact_DefaultContact_MultipleMatches()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var otherOrg = Factory.NewWithValidTestData<OrgHeader>();
			var addr = CreateOrgAddress("Address 1", org);
			CreateContactWithDocumentForOrgAddress(addr, otherOrg, ContactType.ImportFreightAgent, true);
			CreateContactWithDocumentForOrgAddress(addr, org, ContactType.ImportFreightAgent, false);
			CreateContactWithDocumentForOrgAddress(addr, org, ContactType.ImportAirFreightAgent, true);
			CreateContactWithDocumentForOrgAddress(addr, org, ContactType.ImportSeaFreightAgent, true);
			CreateContactWithDocumentForOrgAddress(addr, org, ContactType.Consignor, true);
			CreateContactForOrgAddress(addr, org);

			var consol = Factory.New<ForwardingConsol>();
			consol.ReceivingForwarderWithContact.OrgPK = org.PK;
			Factory.Save();

			consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			consol.ReceivingForwarderWithContact.AddressFK = addr.PK;
			AssertEquals("Empty guid should be returned when there are multiple matches and no official contact for FWI document group", ZGuid.Empty, consol.ReceivingForwarderWithContact.ContactFK);

			var matchingContact = CreateContactWithDocumentForOrgAddress(addr, org, ContactType.ImportFreightAgent, true);
			Factory.Save();
			consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			consol.ReceivingForwarderWithContact.AddressFK = addr.PK;
			AssertEquals("Official contact for FWI document group should be selected when there are multiple matches", matchingContact.PK, consol.ReceivingForwarderWithContact.ContactFK);
		}

		public void TestSendingForwarderWithContact_DefaultContact_MultipleMatches()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var otherOrg = Factory.NewWithValidTestData<OrgHeader>();
			var addr = CreateOrgAddress("Address 1", org);
			CreateContactWithDocumentForOrgAddress(addr, otherOrg, ContactType.ExportFreightAgent, true);
			CreateContactWithDocumentForOrgAddress(addr, org, ContactType.ExportFreightAgent, false);
			CreateContactWithDocumentForOrgAddress(addr, org, ContactType.ExportAirFreightAgent, true);
			CreateContactWithDocumentForOrgAddress(addr, org, ContactType.ExportSeaFreightAgent, true);
			CreateContactWithDocumentForOrgAddress(addr, org, ContactType.Consignor, true);
			CreateContactForOrgAddress(addr, org);

			var consol = Factory.New<ForwardingConsol>();
			consol.SendingForwarderWithContact.OrgPK = org.PK;
			Factory.Save();

			consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			consol.SendingForwarderWithContact.AddressFK = addr.PK;
			AssertEquals("Empty guid should be returned when there are multiple matches and no official contact for FWE document group", ZGuid.Empty, consol.SendingForwarderWithContact.ContactFK);

			var matchingContact = CreateContactWithDocumentForOrgAddress(addr, org, ContactType.ExportFreightAgent, true);
			Factory.Save();
			consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			consol.SendingForwarderWithContact.AddressFK = addr.PK;
			AssertEquals("Official contact for FWE document group should be selected when there are multiple matches", matchingContact.PK, consol.SendingForwarderWithContact.ContactFK);
		}

		public void TestReceivingForwarderWithContact_DefaultContact_MultipleMatches_Air()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var otherOrg = Factory.NewWithValidTestData<OrgHeader>();
			var addr = CreateOrgAddress("Address 1", org);
			CreateContactWithDocumentForOrgAddress(addr, otherOrg, ContactType.ImportAirFreightAgent, true);
			CreateContactWithDocumentForOrgAddress(addr, otherOrg, ContactType.ImportFreightAgent, true);
			CreateContactWithDocumentForOrgAddress(addr, org, ContactType.ImportAirFreightAgent, false);
			CreateContactWithDocumentForOrgAddress(addr, org, ContactType.ImportFreightAgent, false);
			CreateContactWithDocumentForOrgAddress(addr, org, ContactType.ImportSeaFreightAgent, true);
			var defaultFreightAgent = CreateContactWithDocumentForOrgAddress(addr, org, ContactType.ImportFreightAgent, true);
			CreateContactWithDocumentForOrgAddress(addr, org, ContactType.Consignor, true);
			CreateContactForOrgAddress(addr, org);

			var consol = Factory.New<ForwardingConsol>();
			consol.ReceivingForwarderWithContact.OrgPK = org.PK;
			consol.JK_TransportMode = TransportModes.Air;
			Factory.Save();

			consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			consol.ReceivingForwarderWithContact.AddressFK = addr.PK;
			AssertEquals("Official contact for FWI document group should be selected when there are multiple matches and no official contact for FIA document group", defaultFreightAgent.PK, consol.ReceivingForwarderWithContact.ContactFK);

			var airFreightAgent = CreateContactWithDocumentForOrgAddress(addr, org, ContactType.ImportAirFreightAgent, true);
			Factory.Save();
			consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			consol.ReceivingForwarderWithContact.AddressFK = addr.PK;
			AssertEquals("Official contact for FIA document group should be selected when there are multiple matches", airFreightAgent.PK, consol.ReceivingForwarderWithContact.ContactFK);

			defaultFreightAgent.Delete();
			airFreightAgent.Delete();
			Factory.Save();
			consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			consol.ReceivingForwarderWithContact.AddressFK = addr.PK;
			AssertEquals("Empty guid should be returned when there are multiple matches and no official contact for the FIA or FWI document groups", ZGuid.Empty, consol.ReceivingForwarderWithContact.ContactFK);
		}

		public void TestSendingForwarderWithContact_DefaultContact_MultipleMatches_Air()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var otherOrg = Factory.NewWithValidTestData<OrgHeader>();
			var addr = CreateOrgAddress("Address 1", org);
			CreateContactWithDocumentForOrgAddress(addr, otherOrg, ContactType.ExportAirFreightAgent, true);
			CreateContactWithDocumentForOrgAddress(addr, otherOrg, ContactType.ExportFreightAgent, true);
			CreateContactWithDocumentForOrgAddress(addr, org, ContactType.ExportAirFreightAgent, false);
			CreateContactWithDocumentForOrgAddress(addr, org, ContactType.ExportFreightAgent, false);
			CreateContactWithDocumentForOrgAddress(addr, org, ContactType.ExportSeaFreightAgent, true);
			var defaultFreightAgent = CreateContactWithDocumentForOrgAddress(addr, org, ContactType.ExportFreightAgent, true);
			CreateContactWithDocumentForOrgAddress(addr, org, ContactType.Consignor, true);
			CreateContactForOrgAddress(addr, org);

			var consol = Factory.New<ForwardingConsol>();
			consol.SendingForwarderWithContact.OrgPK = org.PK;
			consol.JK_TransportMode = TransportModes.Air;
			Factory.Save();

			consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			consol.SendingForwarderWithContact.AddressFK = addr.PK;
			AssertEquals("Official contact for FWE document group should be selected when there are multiple matches and no official contact for FEA document group", defaultFreightAgent.PK, consol.SendingForwarderWithContact.ContactFK);

			var airFreightAgent = CreateContactWithDocumentForOrgAddress(addr, org, ContactType.ExportAirFreightAgent, true);
			Factory.Save();
			consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			consol.SendingForwarderWithContact.AddressFK = addr.PK;
			AssertEquals("Official contact for FEA document group should be selected when there are multiple matches", airFreightAgent.PK, consol.SendingForwarderWithContact.ContactFK);

			defaultFreightAgent.Delete();
			airFreightAgent.Delete();
			Factory.Save();
			consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			consol.SendingForwarderWithContact.AddressFK = addr.PK;
			AssertEquals("Empty guid should be returned when there are multiple matches and no official contact for the FEA or FWE document groups", ZGuid.Empty, consol.SendingForwarderWithContact.ContactFK);
		}

		public void TestReceivingForwarderWithContact_DefaultContact_MultipleMatches_Sea()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var otherOrg = Factory.NewWithValidTestData<OrgHeader>();
			var addr = CreateOrgAddress("Address 1", org);
			CreateContactWithDocumentForOrgAddress(addr, otherOrg, ContactType.ImportSeaFreightAgent, true);
			CreateContactWithDocumentForOrgAddress(addr, otherOrg, ContactType.ImportFreightAgent, true);
			CreateContactWithDocumentForOrgAddress(addr, org, ContactType.ImportAirFreightAgent, true);
			CreateContactWithDocumentForOrgAddress(addr, org, ContactType.ImportFreightAgent, false);
			CreateContactWithDocumentForOrgAddress(addr, org, ContactType.ImportSeaFreightAgent, false);
			var defaultFreightAgent = CreateContactWithDocumentForOrgAddress(addr, org, ContactType.ImportFreightAgent, true);
			CreateContactWithDocumentForOrgAddress(addr, org, ContactType.Consignor, true);
			CreateContactForOrgAddress(addr, org);

			var consol = Factory.New<ForwardingConsol>();
			consol.ReceivingForwarderWithContact.OrgPK = org.PK;
			consol.JK_TransportMode = TransportModes.Sea;
			Factory.Save();

			consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			consol.ReceivingForwarderWithContact.AddressFK = addr.PK;
			AssertEquals("Official contact for FWI document group should be selected when there are multiple matches and no official contact for FIS document group", defaultFreightAgent.PK, consol.ReceivingForwarderWithContact.ContactFK);

			var seaFreightAgent = CreateContactWithDocumentForOrgAddress(addr, org, ContactType.ImportSeaFreightAgent, true);
			Factory.Save();
			consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			consol.ReceivingForwarderWithContact.AddressFK = addr.PK;
			AssertEquals("Official contact for FIS document group should be selected when there are multiple matches", seaFreightAgent.PK, consol.ReceivingForwarderWithContact.ContactFK);

			defaultFreightAgent.Delete();
			seaFreightAgent.Delete();
			Factory.Save();
			consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			consol.ReceivingForwarderWithContact.AddressFK = addr.PK;
			AssertEquals("Empty guid should be returned when there are multiple matches and no official contact for the FIS or FWI document groups", ZGuid.Empty, consol.ReceivingForwarderWithContact.ContactFK);
		}

		public void TestSendingForwarderWithContact_DefaultContact_MultipleMatches_Sea()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var otherOrg = Factory.NewWithValidTestData<OrgHeader>();
			var addr = CreateOrgAddress("Address 1", org);
			CreateContactWithDocumentForOrgAddress(addr, otherOrg, ContactType.ExportSeaFreightAgent, true);
			CreateContactWithDocumentForOrgAddress(addr, otherOrg, ContactType.ExportFreightAgent, true);
			CreateContactWithDocumentForOrgAddress(addr, org, ContactType.ExportAirFreightAgent, true);
			CreateContactWithDocumentForOrgAddress(addr, org, ContactType.ExportFreightAgent, false);
			CreateContactWithDocumentForOrgAddress(addr, org, ContactType.ExportSeaFreightAgent, false);
			var defaultFreightAgent = CreateContactWithDocumentForOrgAddress(addr, org, ContactType.ExportFreightAgent, true);
			CreateContactWithDocumentForOrgAddress(addr, org, ContactType.Consignor, true);
			CreateContactForOrgAddress(addr, org);

			var consol = Factory.New<ForwardingConsol>();
			consol.SendingForwarderWithContact.OrgPK = org.PK;
			consol.JK_TransportMode = TransportModes.Sea;
			Factory.Save();

			consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			consol.SendingForwarderWithContact.AddressFK = addr.PK;
			AssertEquals("Official contact for FWE document group should be selected when there are multiple matches and no official contact for FES document group", defaultFreightAgent.PK, consol.SendingForwarderWithContact.ContactFK);

			var seaFreightAgent = CreateContactWithDocumentForOrgAddress(addr, org, ContactType.ExportSeaFreightAgent, true);
			Factory.Save();
			consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			consol.SendingForwarderWithContact.AddressFK = addr.PK;
			AssertEquals("Official contact for FES document group should be selected when there are multiple matches", seaFreightAgent.PK, consol.SendingForwarderWithContact.ContactFK);

			defaultFreightAgent.Delete();
			seaFreightAgent.Delete();
			Factory.Save();
			consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			consol.SendingForwarderWithContact.AddressFK = addr.PK;
			AssertEquals("Empty guid should be returned when there are multiple matches and no official contact for the FES or FWE document groups", ZGuid.Empty, consol.SendingForwarderWithContact.ContactFK);
		}

		public void TestReceivingForwarderWithContact_DefaultContact_SuspendSettingHasChanges()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var defaultContact = CreateContactForOrgAddress(org.MainAddress, org);

			var consol = Factory.New<ForwardingConsol>();
			consol.ReceivingForwarderWithContact.OrgPK = org.PK;
			AssertEquals("Should be set to contact that matches organisation's main address by default", defaultContact.PK, consol.ReceivingForwarderWithContact.ContactFK);
			Factory.Save();

			consol.JK_OC_ReceivingForwarderContact = ZGuid.Empty;
			Factory.Save();

			var consol2 = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			AssertEquals("Should be set to contact that matches organisation's main address by default", defaultContact.PK, consol2.ReceivingForwarderWithContact.ContactFK);
			AssertEquals("Consol should not set HasChanges to true", false, consol2.HasChanges);
		}

		public void TestSendingForwarderWithContact_DefaultContact_SuspendSettingHasChanges()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var defaultContact = CreateContactForOrgAddress(org.MainAddress, org);

			var consol = Factory.New<ForwardingConsol>();
			consol.SendingForwarderWithContact.OrgPK = org.PK;
			AssertEquals("Should be set to contact that matches organisation's main address by default", defaultContact.PK, consol.SendingForwarderWithContact.ContactFK);
			Factory.Save();

			consol.JK_OC_SendingForwarderContact = ZGuid.Empty;
			Factory.Save();

			var consol2 = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			AssertEquals("Should be set to contact that matches organisation's main address by default", defaultContact.PK, consol2.SendingForwarderWithContact.ContactFK);
			AssertEquals("Consol should not set HasChanges to true", false, consol2.HasChanges);
		}

		void AssertForwarderAddressWithContact_DefaultContact_SingleMatch(Func<ForwardingConsol, ZAddressWithContact> getAddressWithContactDelegate)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var otherOrg = Factory.NewWithValidTestData<OrgHeader>();
			var addr1 = CreateOrgAddress("Address 1", org);
			var addr2 = CreateOrgAddress("Address 2", org);
			CreateContactForOrgAddress(addr1, otherOrg);
			CreateContactForOrgAddress(addr2, org);
			var defaultContact = CreateContactForOrgAddress(org.MainAddress, org);
			var inactiveContact = CreateContactForOrgAddress(addr1, org);
			inactiveContact.OC_IsActive = false;

			var consol = Factory.New<ForwardingConsol>();
			getAddressWithContactDelegate(consol).OrgPK = org.PK;
			Factory.Save();

			consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			AssertEquals("Should be set to contact that matches organisation's main address by default", defaultContact.PK, getAddressWithContactDelegate(consol).ContactFK);

			consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			getAddressWithContactDelegate(consol).AddressFK = addr1.PK;
			AssertEquals("Organisation has no matching contacts for selected address so should return empty guid", ZGuid.Empty, getAddressWithContactDelegate(consol).ContactFK);

			var matchingContact = CreateContactForOrgAddress(addr1, org);
			Factory.Save();
			consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			getAddressWithContactDelegate(consol).AddressFK = addr1.PK;
			AssertEquals("Organisation only has one matching contact for selected address so should return that contact's PK", matchingContact.PK, getAddressWithContactDelegate(consol).ContactFK);
		}

		OrgAddress CreateOrgAddress(string code, OrgHeader org)
		{
			var addr = Factory.NewWithValidTestData<OrgAddress>();
			addr.OA_Code = code;
			addr.OA_OH = org.PK;
			return addr;
		}

		OrgContact CreateContactForOrgAddress(OrgAddress addr, OrgHeader org)
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.WorkingAddressPK = addr.PK;
			org.Contacts.Add(contact);
			return contact;
		}

		OrgContact CreateContactWithDocumentForOrgAddress(OrgAddress addr, OrgHeader org, ContactType contactType, ZBool isDefault)
		{
			var contact = CreateContactForOrgAddress(addr, org);
			var doc = contact.Documents.AddNew();
			doc.OD_DocumentGroup = contactType.Code;
			doc.OD_DefaultContact = isDefault;
			return contact;
		}

		#endregion

		#region Template Records

		public void TestTemplateRecord_SaveAndLoad()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "ABC";
			consol.IsTemplateRecord = true;

			var templateRecord = Factory.New<StmTemplateRecord>();
			templateRecord.STR_ModuleID = ModuleIDs.JobConsol.Name;
			templateRecord.STR_ReferenceId = "TR0000001";

			var templateRecordProvider1 = consol as ITemplateRecordProvider;
			templateRecordProvider1.TemplateRecord = templateRecord;

			Factory.Save();

			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				templateRecordProvider1.SaveToTemplateRecord();
			}

			var consol2 = Factory.New<ForwardingConsol>();
			var templateRecordProvider2 = consol2 as ITemplateRecordProvider;

			AssertNotEquals(consol.JK_MasterBillNum, consol2.JK_MasterBillNum);

			templateRecordProvider2.LoadFromTemplateRecord(templateRecord);

			AssertEquals(
				"Should have copied from the template",
				consol.JK_MasterBillNum,
				consol2.JK_MasterBillNum
			);
		}

		public void TestTemplateRecord_LoadTemplateRelatedLogs()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.IsTemplateRecord = true;

			var templateRecord = Factory.New<StmTemplateRecord>();
			templateRecord.STR_ModuleID = ModuleIDs.JobConsol.Name;
			templateRecord.STR_ReferenceId = "TR0000001";

			var templateRecordProvider1 = consol as ITemplateRecordProvider;
			templateRecordProvider1.TemplateRecord = templateRecord;

			Factory.Save();

			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				templateRecordProvider1.SaveToTemplateRecord();
			}

			var consol2 = Factory.New<ForwardingConsol>();
			var templateRecordProvider2 = consol2 as ITemplateRecordProvider;

			templateRecordProvider2.LoadFromTemplateRecord(templateRecord);

			var relatedBOs = consol2.GetBusinessObjectsWithRelatedEvents();
			var templateBO = relatedBOs.FirstOrDefault(x => x is StmTemplateRecord);

			AssertNotNull(templateBO);
			AssertEquals(templateRecord.STR_ReferenceId, ((StmTemplateRecord)templateBO).STR_ReferenceId);
		}

		public void TestTemplateRecord_InstantiateFromTemplateRecord()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "ABC";
			consol.IsTemplateRecord = true;

			var templateRecord = Factory.New<StmTemplateRecord>();
			templateRecord.STR_ModuleID = ModuleIDs.JobConsol.Name;
			templateRecord.STR_ReferenceId = "TR0000001";

			var templateRecordProvider = consol as ITemplateRecordProvider;
			templateRecordProvider.TemplateRecord = templateRecord;

			Factory.Save();

			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				templateRecordProvider.SaveToTemplateRecord();
			}

			var consol2 = templateRecordProvider.InstantiateFromTemplateRecord(Factory, typeof(ForwardingConsol), templateRecord) as ForwardingConsol;

			AssertEquals(
				"Should have created an identical record from the template",
				consol.JK_MasterBillNum,
				consol2.JK_MasterBillNum
			);
		}

		public void TestTemplateRecord_LoadsFromDatabaseIfCreatedFromTemplate()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "ABC";
			consol.IsTemplateRecord = true;

			var templateRecord = Factory.New<StmTemplateRecord>();
			templateRecord.STR_ModuleID = ModuleIDs.JobConsol.Name;
			templateRecord.STR_ReferenceId = "TR0000001";

			var templateRecordProvider = consol as ITemplateRecordProvider;
			templateRecordProvider.TemplateRecord = templateRecord;

			Factory.Save();

			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				templateRecordProvider.SaveToTemplateRecord();
			}

			CombineAssertions("Pre-Condition: Template records should not load from JK_STR", () =>
			{
				Assert(consol.IsTemplateRecord);
				AssertEquals(templateRecord, consol.TemplateRecord);
			});

			var otherTemplateRecord = Factory.New<StmTemplateRecord>();
			otherTemplateRecord.STR_ModuleID = ModuleIDs.JobConsol.Name;
			otherTemplateRecord.STR_ReferenceId = "TR0000002";
			consol.JK_STR = otherTemplateRecord.PK;
			consol.IsTemplateRecord = false;

			Factory.Save();

			CombineAssertions("Concrete consols (created from templates) should load template record from JK_STR", () =>
			{
				Assert(!consol.IsTemplateRecord);
				AssertEquals(otherTemplateRecord, consol.TemplateRecord);
			});
		}

		public void TestTemplateRecord_ShouldNotBeAccessedIfConsolDeleted()
		{
			var templateRecord = Factory.New<StmTemplateRecord>();
			templateRecord.STR_ModuleID = ModuleIDs.JobConsol.Name;
			templateRecord.STR_ReferenceId = "TR0000001";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "ABC";
			consol.IsTemplateRecord = false;
			consol.JK_STR = templateRecord.PK;

			var templateRecordProvider = consol as ITemplateRecordProvider;
			templateRecordProvider.TemplateRecord = templateRecord;

			Factory.Save();

			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				templateRecordProvider.SaveToTemplateRecord();
			}

			consol.Delete();

			ITemplateRecord templateRecord2 = null;
			AssertNoExceptionThrown(() => { templateRecord2 = templateRecordProvider.TemplateRecord; });
			AssertNull(templateRecord2);
		}

		public void TestTemplateRecord_AddStmNote_ReloadTemplateRecord()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.IsTemplateRecord = true;
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			var note = consol.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			note.ST_NoteText = "test";

			var templateRecord = Factory.New<StmTemplateRecord>();
			templateRecord.STR_ModuleID = ModuleIDs.JobConsol.Name;
			templateRecord.STR_ReferenceId = "TR0000001";

			var templateRecordProvider = consol as ITemplateRecordProvider;
			templateRecordProvider.TemplateRecord = templateRecord;

			Factory.Save();

			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				templateRecordProvider.SaveToTemplateRecord();
			}

			var consol2 = Factory.New<ForwardingConsol>();
			var templateRecordProvider2 = consol2 as ITemplateRecordProvider;

			AssertNoExceptionThrown(() => templateRecordProvider2.LoadFromTemplateRecord(templateRecord));
		}

		#endregion

		#region TemplateCopy

		public void TestTemplateCopy()
		{
			var consol = Factory.New<ForwardingConsol>();
			var consolDGRestriction = consol.ConsolDGRestrictionCollection.AddNew();
			consolDGRestriction.JKD_UNNO = "1001";

			var copiedConsol = (ForwardingConsol)((ITemplateCopyable)consol).TemplateCopy();
			AssertEquals("DG restrictions collection should have been copied from template.", 1, copiedConsol.ConsolDGRestrictionCollection.Count);
			AssertEquals("DG restriction should be identical to template.", consolDGRestriction.JKD_UNNO, copiedConsol.ConsolDGRestrictionCollection[0].JKD_UNNO);
		}

		public void TestTemplateCopyHandlesNotes()
		{
			ErrorReporter.Clear();
			var consol = Factory.New<ForwardingConsol>();
			var consolNote = consol.Notes.AddNew();
			consolNote.ST_Description = PredefinedNoteTypes.Instance.OriginalBillNotes.Description;
			consolNote = consol.Notes.AddNew();
			consolNote.ST_Description = "Test Note";

			var copiedConsol = (ForwardingConsol)((ITemplateCopyable)consol).TemplateCopy();

			AssertNullOrEmpty(ErrorReporter.LastKeyReported);
			AssertEquals("Notes collection should have been copied from template.", 1, copiedConsol.Notes.VisibleNotes.Count);
			AssertEquals("Note should be identical to template.", consolNote.ST_Description, copiedConsol.Notes.VisibleNotes[0].ST_Description);

			ErrorReporter.Clear();
		}

		public void TestTemplateCopyUsesAlternativeFactoryToCloneDGRestrictions()
		{
			var consol = Factory.New<ForwardingConsol>();
			var consolDGRestriction = consol.ConsolDGRestrictionCollection.AddNew();
			consolDGRestriction.JKD_UNNO = "1001";

			var alternativeFactory = new BusinessObjectFactory();
			var copiedConsol = (ForwardingConsol)consol.TemplateCopy(true, true, alternativeFactory: alternativeFactory);
			AssertEquals("Precondition.", alternativeFactory._Instance, copiedConsol.Factory._Instance);
			copiedConsol.Factory.Save();

			AssertEquals("DG restrictions collection should have been copied from template into alternative factory.", 1, copiedConsol.ConsolDGRestrictionCollection.Count);
			AssertEquals("DG restriction should be identical to template.", consolDGRestriction.JKD_UNNO, copiedConsol.ConsolDGRestrictionCollection[0].JKD_UNNO);
		}

		public void TestTemplateCopy_CopyUnLinkedLegsJobCO2e()
		{
			// Arrange
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			var transport = consol.Transports[0];
			transport.JW_IsLinked = false;
			transport.JW_TransportMode = TransportModes.Air;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "HKHKG";
			transport.JW_VoyageFlight = "QF123";
			transport.JW_ETD = new ZDateTime(2023, 5, 17, 9, 0, 0);
			transport.JW_ETA = new ZDateTime(2023, 5, 17, 20, 0, 0);
			transport.SetCO2ePerTonneInKg(2m);
			consol.WeightVerificationUnit = "KG";
			consol.JK_TotalShipmentActWeightCheck = 300m;
			consol.SetCO2ePerTonneInKg(2m);
			consol.SetCO2eDistanceInKM(1000m);
			consol.SetTotalCO2e(0.6m);
			AssertEquals("Pre-condition.", consol.GetCO2eStatus(), CO2eStatusList.Codes.Current);
			AssertEquals("Pre-condition.", consol.GetTotalCO2e(), 0.6m);
			Assert("Pre-condition.", !transport.JW_IsLinked);

			// Act
			var copiedConsol = (ForwardingConsol)consol.TemplateCopy(true, true);

			// Assert
			AssertEquals(copiedConsol.GetCO2eStatus(), CO2eStatusList.Codes.NotCurrent);
			AssertEquals(copiedConsol.GetTotalCO2e(), 0.6m);
			Assert(!copiedConsol.Transports[0].JW_IsLinked);
			AssertNullOrEmpty(copiedConsol.Transports[0].JW_VoyageFlight);
			AssertEquals(copiedConsol.Transports[0].GetCO2ePerTonneInKg(), 2m);
			AssertEquals(copiedConsol.Transports[0].GetCO2eStatus(), CO2eStatusList.Codes.NotCurrent);
			AssertEquals("No STU event created", 0, copiedConsol.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode).Count());
		}

		public void TestTemplateCopy_CopyLinkedLegsJobCO2e()
		{
			// Arrange
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "AUSYD";
			var transport = consol.Transports[0];
			var sailing = CreateJobSailing("QF001", "SGSIN", "AUSYD", ZDateTime.Now.AddDays(-2), ZDateTime.Now.AddDays(-1));
			sailing.SetCO2ePerTonneInKg(2m);
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;
			consol.WeightVerificationUnit = "KG";
			consol.JK_TotalShipmentActWeightCheck = 300m;
			consol.SetCO2ePerTonneInKg(2m);
			consol.SetCO2eDistanceInKM(1000m);
			consol.SetTotalCO2e(0.6m);
			AssertEquals("Pre-condition.", consol.GetCO2eStatus(), CO2eStatusList.Codes.Current);
			AssertEquals("Pre-condition.", consol.GetTotalCO2e(), 0.6m);
			Assert("Pre-condition.", transport.JW_IsLinked);
			AssertEquals("Pre-condition.", transport.GetCO2ePerTonneInKg(), 2m);

			// Act
			var copiedConsol = (ForwardingConsol)consol.TemplateCopy(true, true);

			// Assert
			AssertEquals(copiedConsol.GetCO2eStatus(), CO2eStatusList.Codes.Current);
			AssertEquals(copiedConsol.GetTotalCO2e(), 0.6m);
			Assert(copiedConsol.Transports[0].JW_IsLinked);
			AssertNullOrEmpty(copiedConsol.Transports[0].JW_VoyageFlight);
			AssertEquals(copiedConsol.Transports[0].GetCO2ePerTonneInKg(), 0m);
			AssertEquals(copiedConsol.Transports[0].GetCO2eStatus(), CO2eStatusList.Codes.NotCalculated);
		}

		#endregion

		#region Clone Internal

		public void TestTemplateCopyDocAddresses()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.DocAddresses.AddNew(DocAddressType.MasterBillShipperOverride);
			Factory.Save();

			var copiedConsol = (ForwardingConsol)consol.Clone();

			AssertEquals(1, copiedConsol.DocAddresses.Count);
			AssertEquals(AutoDocAddressTypes.Codes.MasterBillShipperOverride, copiedConsol.DocAddresses[0].E2_AddressType);
		}

		#endregion

		#region Universal Copy

		public void TestAfterUniversalCopyContainerPackLinkCreateMissingConShipLinks()
		{
			var (sourceConsol, sourceShipment) = CreateConsolAndShipmentWithConShipLink();
			CopyAndAssertCopiedConsol(CreateConsolCopyTemplateTree("Copy Container and Link Container.PackLine", false), sourceConsol, sourceShipment);

			(sourceConsol, sourceShipment) = CreateConsolAndShipmentWithConShipLink();
			CopyAndAssertCopiedConsol(CreateConsolCopyTemplateTree("Copy Container and Link Container.PackLine with ConsolShip link", true), sourceConsol, sourceShipment);

			var shipmentRelatedEntityNode = new RelatedEntityCopyTemplateNode
			{
				Name = "JobShipment",
				RelatedEntityTableName = "JobShipment",
				RelatedPropertyName = "JL_JS",
				CopyMethod = RelatedEntityCopyMethod.Link
			};
			var packLineEntityNode = new EntityCopyTemplateNode
			{
				Name = "JobPackLine"
			};
			packLineEntityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = "JL_FreightMode", CopyMethod = CopyMethod.Copy });
			packLineEntityNode.Nodes.Add(shipmentRelatedEntityNode);

			(sourceConsol, sourceShipment) = CreateConsolAndShipmentWithConShipLink();
			CopyAndAssertCopiedConsol(CreateConsolCopyTemplateTree("Copy Container + PackLine and Link Container.PackLine.Shipment", false, packLineEntityNode), sourceConsol, sourceShipment);

			(sourceConsol, sourceShipment) = CreateConsolAndShipmentWithConShipLink();
			CopyAndAssertCopiedConsol(CreateConsolCopyTemplateTree("Copy Container + PackLine and Link Container.PackLine.Shipment with ConsolShip link", true, packLineEntityNode), sourceConsol, sourceShipment);
		}

		(ForwardingConsol, ForwardingShipment) CreateConsolAndShipmentWithConShipLink()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_ConsolMode = "FCL";
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "N95";
			container.JC_ContainerMode = "FCL";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ActualWeight = 5;
			shipment.JS_ActualVolume = 5;
			shipment.JS_OuterPacks = 5;

			Factory.Save();
			AssertEquals("Pre-condition: shipment has one packline.", 1, shipment.OuterPackLines.Count);
			AssertEquals("Pre-condition: shipment's packline is linked to consol's container.", "N95", shipment.OuterPackLines[0].JL_Calc_ContainerNum);

			return (consol, shipment);
		}

		void CopyAndAssertCopiedConsol(CopyTemplateTree copyTree, ForwardingConsol sourceConsol, ForwardingShipment sourceShipment)
		{
			var copyManager = new BusinessObjectCopyManager(Factory);
			var copiedConsol = copyManager.Copy(sourceConsol, copyTree).Object as ForwardingConsol;

			CombineAssertions($"UC template: {copyTree.Description}", () =>
			{
				AssertNoExceptionThrown("Saving should not throw missing Con-Ship link exception.", () => Factory.Save());
				AssertEquals("Container is copied.", 1, copiedConsol.Containers.Count);
				AssertEquals("Container is copied.", "N95", copiedConsol.Containers[0].JC_ContainerNum);
				AssertEquals("Consol-Shipment link is created.", 1, copiedConsol.Shipments.Count);
				AssertEquals("Consol-Shipment link is created.", sourceShipment.PK, copiedConsol.Shipments[0].PK);
				AssertEquals("Container is linked to packline.", 1, copiedConsol.Containers[0].PackLines.Count);
				AssertEquals("Container is linked to packline.", copiedConsol.Shipments[0].PK, copiedConsol.Containers[0].PackLines[0].JL_JS);
			});
		}

		CopyTemplateTree CreateConsolCopyTemplateTree(string templateDescription, bool withConShipLink, EntityCopyTemplateNode packLineEntityNode = null)
		{
			var packLineRelatedEntityNode = new RelatedEntityCopyTemplateNode
			{
				Name = "JobPackLine",
				RelatedEntityTableName = "JobPackLines",
				RelatedPropertyName = "J6_JL",
				CopyMethod = packLineEntityNode == null ? RelatedEntityCopyMethod.Link : RelatedEntityCopyMethod.Copy,
				InnerNode = packLineEntityNode
			};

			var containerPackPivotEntityNode = new EntityCopyTemplateNode
			{
				Name = "JobContainerPackPivot",
			};
			containerPackPivotEntityNode.Nodes.Add(packLineRelatedEntityNode);

			var containerPackPivotsCollectionNode = new CollectionCopyTemplateNode()
			{
				Name = "JobContainerPackPivots",
				ItemPropertyName = "J6_JC",
				ItemsTableName = "JobContainerPackPivot",
				InnerNode = containerPackPivotEntityNode,
				CopyMethod = CollectionCopyMethod.All
			};

			var containerEntityNode = new EntityCopyTemplateNode
			{
				Name = "JobContainer"
			};
			containerEntityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = "JC_ContainerMode", CopyMethod = CopyMethod.Copy });
			containerEntityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = "JC_ContainerNum", CopyMethod = CopyMethod.Copy });
			containerEntityNode.Nodes.Add(containerPackPivotsCollectionNode);

			var containersCollectionNode = new CollectionCopyTemplateNode
			{
				Name = "JobContainers",
				ItemPropertyName = "JC_JK",
				ItemsTableName = "JobContainer",
				InnerNode = containerEntityNode,
				CopyMethod = CollectionCopyMethod.All
			};

			var consolEntityNode = new EntityCopyTemplateNode
			{
				Name = "JobConsol"
			};
			consolEntityNode.Nodes.Add(containersCollectionNode);

			if (withConShipLink)
			{
				var shipmentRelatedEntityNode = new RelatedEntityCopyTemplateNode
				{
					Name = "JobShipment",
					RelatedEntityTableName = "JobShipment",
					RelatedPropertyName = "JN_JS",
					CopyMethod = RelatedEntityCopyMethod.Link
				};

				var conShipLinkEntityNode = new EntityCopyTemplateNode
				{
					Name = "JobConShipLink",
				};
				conShipLinkEntityNode.Nodes.Add(shipmentRelatedEntityNode);

				var conShipLinkCollectionNode = new CollectionCopyTemplateNode()
				{
					Name = "JobConShipLinks",
					ItemPropertyName = "JN_JK",
					ItemsTableName = "JobConShipLink",
					InnerNode = conShipLinkEntityNode,
					CopyMethod = CollectionCopyMethod.All
				};

				consolEntityNode.Nodes.Add(conShipLinkCollectionNode);
			}

			var copyTree = new CopyTemplateTree
			{
				Name = "JobConsol",
				InnerNode = consolEntityNode,
				TableName = "JobConsol",
				Description = templateDescription
			};

			return copyTree;
		}

		public void TestUniversalCopyIgnoreProperties()
		{
			var ignoreElementAttributes = (UniversalCopyIgnoreElementAttribute[])(typeof(ForwardingConsol).GetCustomAttributes(typeof(UniversalCopyIgnoreElementAttribute), false));
			AssertEquals(1, ignoreElementAttributes.Length);
			AssertCollectionContains("JK_OA_CoLoadAddress", ignoreElementAttributes[0].ElementNames);
		}

		public void TestAfterUniversalCopy()
		{
			AssertAfterUniversalCopy(Core.Constants.TransportModes.Air);
			AssertAfterUniversalCopy(Core.Constants.TransportModes.Sea);
		}

		void AssertAfterUniversalCopy(string transportMode)
		{
			var consol = Factory.New<ForwardingConsol>();
			var universalCopyAttribute = consol.GetType()
				.GetCustomAttributes(typeof(UniversalCopyWithExtendedEntitiesAttribute), true)
				.Cast<UniversalCopyWithExtendedEntitiesAttribute>()
				.FirstOrDefault();
			AssertNotNull(universalCopyAttribute);
			AssertEquals("AfterUniversalCopy", universalCopyAttribute.FinishCopyMethod);

			consol.JK_TransportMode = transportMode;
			consol.JK_TotalShipmentChargeableUnit = Constants.Weight.Kilograms;
			consol.JK_TotalShipmentActOtherUnit = Constants.Volume.CubicMetres;

			ClearFieldValue(consol, "fWeightVerificationUnit");
			ClearFieldValue(consol, "fVolumeVerificationUnit");
			AssertEquals(ZString.Empty, GetFieldValue(consol, "fWeightVerificationUnit"));
			AssertEquals(ZString.Empty, GetFieldValue(consol, "fVolumeVerificationUnit"));

			consol.GetType().InvokeMember(universalCopyAttribute.FinishCopyMethod,
				System.Reflection.BindingFlags.InvokeMethod
				| System.Reflection.BindingFlags.NonPublic
				| System.Reflection.BindingFlags.Instance,
				null, consol, null);

			var expectedWeightUnit = consol.IsAir ? Constants.Weight.Kilograms : Constants.Volume.CubicMetres;
			var expectedVolumeUnit = consol.IsAir ? Constants.Volume.CubicMetres : Constants.Weight.Kilograms;
			AssertEquals(expectedWeightUnit, GetFieldValue(consol, "fWeightVerificationUnit"));
			AssertEquals(expectedVolumeUnit, GetFieldValue(consol, "fVolumeVerificationUnit"));
		}

		ZString GetFieldValue(ForwardingConsol consol, string fieldName)
		{
			var fieldInfo = GetFieldInfo(consol, fieldName);
			return (ZString)fieldInfo.GetValue(consol);
		}

		void ClearFieldValue(ForwardingConsol consol, string fieldName)
		{
			var fieldInfo = GetFieldInfo(consol, fieldName);
			fieldInfo.SetValue(consol, ZString.Empty);
		}

		System.Reflection.FieldInfo GetFieldInfo(ForwardingConsol consol, string fieldName)
		{
			var bindingAttr = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
			var consolType = consol.GetType();

			var fieldInfo = consolType.GetField(fieldName, bindingAttr)
				?? consolType.BaseType.GetField(fieldName, bindingAttr);
			AssertNotNull(fieldInfo);

			return fieldInfo;
		}

		#endregion

		#region Carrier Contract functionality

		public void TestShouldSetAllocationRoute()
		{
			var contract = Factory.New<RatingContract>();
			var allocationLine1 = contract.Allocations.AddNew();
			var allocationLine2 = contract.Allocations.AddNew();

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "BG";

			var fallbackAccountPivot1 = allocationLine1.Contract.NamedAccountPivots.AddNew();
			var fallbackAccountPivot2 = allocationLine2.Contract.NamedAccountPivots.AddNew();

			fallbackAccountPivot1.RNP_OH_NamedAccount = carrier.PK;
			fallbackAccountPivot2.RNP_OH_NamedAccount = carrier.PK;

			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			using (consol.GetValidationSuspender())
			{
				AssertEquals(ZGuid.Empty, consol.JK_RCA_AllocationLine);

				consol.JK_RCA_AllocationLine = allocationLine1.PK;

				AssertEquals(
					"Should update JK_RCA_AllocationLine when the allocation line has an associated rating contract & not all shipments have a matching org in fallback header named account collection",
					allocationLine1.PK,
					consol.JK_RCA_AllocationLine
				);

				shipment.ConsigneePK = carrier.PK;
				consol.JK_RCA_AllocationLine = allocationLine2.PK;

				AssertEquals(
					"Should update JK_RCA_AllocationLine when all shipments have a matching org in fallback header named account collection",
					allocationLine2.PK,
					consol.JK_RCA_AllocationLine
				);

				var accountPivot = allocationLine1.NamedAccountPivots.AddNew();
				accountPivot.RNP_OH_NamedAccount = carrier.PK;

				shipment.ConsigneePK = ZGuid.Empty;
				consol.JK_RCA_AllocationLine = allocationLine1.PK;

				AssertEquals(
					"Should update JK_RCA_AllocationLine when the allocation line has an associated rating contract & not all shipments have a matching org in header named account collection",
					allocationLine1.PK,
					consol.JK_RCA_AllocationLine
				);

				shipment.ConsigneePK = carrier.PK;
				consol.JK_RCA_AllocationLine = allocationLine2.PK;

				AssertEquals(
					"Should update JK_RCA_AllocationLine when all shipments have a matching org in header named account collection",
					allocationLine2.PK,
					consol.JK_RCA_AllocationLine
				);

				consol.JK_RCA_AllocationLine = ZGuid.Empty;

				AssertEquals(
					"Should update JK_RCA_AllocationLine when allocation line PK is empty",
					ZGuid.Empty,
					consol.JK_RCA_AllocationLine
				);

				var nonExistentAllocationLineGuid = ZGuid.NewZGuid();
				consol.JK_RCA_AllocationLine = nonExistentAllocationLineGuid;

				AssertEquals(
					"Should update JK_RCA_AllocationLine when no allocation line is found",
					nonExistentAllocationLineGuid,
					consol.JK_RCA_AllocationLine
				);
			}
		}

		public void TestAllocationRouteDefaultsToContainers()
		{
			var contract = Factory.New<RatingContract>();
			var allocationLine = contract.Allocations.AddNew();

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "BG";

			var consol = Factory.New<ForwardingConsol>();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();

			consol.JK_RCA_AllocationLine = allocationLine.PK;

			AssertEquals("Allocation route should be defaulted onto containers", allocationLine.PK, container1.JC_RCA_AllocationLine);
			AssertEquals("Allocation route should be defaulted onto containers", allocationLine.PK, container2.JC_RCA_AllocationLine);
		}

		public void TestContractShouldSetAllocationRoute()
		{
			var contract = Factory.New<RatingContract>();
			var allocationLine1 = contract.Allocations.AddNew();
			var allocationLine2 = contract.Allocations.AddNew();

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "BG";

			var fallbackAccountPivot1 = allocationLine1.Contract.NamedAccountPivots.AddNew();
			var fallbackAccountPivot2 = allocationLine2.Contract.NamedAccountPivots.AddNew();

			fallbackAccountPivot1.RNP_OH_NamedAccount = carrier.PK;
			fallbackAccountPivot2.RNP_OH_NamedAccount = carrier.PK;

			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			using (consol.GetValidationSuspender())
			{
				AssertEquals(ZGuid.Empty, consol.JK_RCA_AllocationLine);

				consol.JK_RCA_AllocationLine = allocationLine1.PK;

				AssertEquals(
					"Should update JK_RCA_AllocationLine when the allocation line has an associated rating contract & not all shipments have a matching org in fallback header named account collection",
					allocationLine1.PK,
					consol.JK_RCA_AllocationLine
				);

				shipment.ConsigneePK = carrier.PK;
				consol.JK_RCA_AllocationLine = allocationLine2.PK;

				AssertEquals(
					"Should update JK_RCA_AllocationLine when all shipments have a matching org in fallback header named account collection",
					allocationLine2.PK,
					consol.JK_RCA_AllocationLine
				);

				var accountPivot = allocationLine1.NamedAccountPivots.AddNew();
				accountPivot.RNP_OH_NamedAccount = carrier.PK;

				shipment.ConsigneePK = ZGuid.Empty;
				consol.JK_RCA_AllocationLine = allocationLine1.PK;

				AssertEquals(
					"Should update JK_RCA_AllocationLine when the allocation line has an associated rating contract & not all shipments have a matching org in header named account collection",
					allocationLine1.PK,
					consol.JK_RCA_AllocationLine
				);

				shipment.ConsigneePK = carrier.PK;
				consol.JK_RCA_AllocationLine = allocationLine2.PK;

				AssertEquals(
					"Should update JK_RCA_AllocationLine when all shipments have a matching org in header named account collection",
					allocationLine2.PK,
					consol.JK_RCA_AllocationLine
				);

				consol.JK_RCA_AllocationLine = ZGuid.Empty;

				AssertEquals(
					"Should update JK_RCA_AllocationLine when allocation line PK is empty",
					ZGuid.Empty,
					consol.JK_RCA_AllocationLine
				);

				var nonExistentAllocationLineGuid = ZGuid.NewZGuid();
				consol.JK_RCA_AllocationLine = nonExistentAllocationLineGuid;

				AssertEquals(
					"Should update JK_RCA_AllocationLine when no allocation line is found",
					nonExistentAllocationLineGuid,
					consol.JK_RCA_AllocationLine
				);
			}
		}

		public void TestConsolVoyageFieldsArePopulatedFromRouteIfBlank()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "NEWCARD";

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_OH = carrier.PK;

			var contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_OH = carrier.PK;
			contract.RCT_ContractNumber = "VERYNICE";
			contract.RCT_ContractType = RatingContractTypes.Provider;

			var allocationRoute = contract.Allocations.AddNew();
			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "GBFXT";
			allocationRoute.RCA_AllocationLineID = "NOCANDO";
			allocationRoute.RCA_RV_NKVessel = "GOTAN830RESATDORSIA";
			allocationRoute.RCA_VoyageNumber = "IMPRESSIVE";
			allocationRoute.RCA_ServiceLoop = "DORYA";
			allocationRoute.RCA_AllocatedQuantity = 420;
			allocationRoute.RCA_AllocatedUQ = "CN";

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.Transports[0].JW_TransportMode = Core.Constants.TransportModes.Sea;

			AssertEquals("Precondition: one transport leg", 1, consol.Transports.Count);

			consol.JK_RCA_AllocationLine = allocationRoute.PK;

			AssertEquals("Consol load port is populated from route", allocationRoute.RCA_LoadLocation, consol.JK_RL_NKLoadPort);
			AssertEquals("Consol discharge port is populated from route", allocationRoute.RCA_DischargeLocation, consol.JK_RL_NKDischargePort);
			AssertEquals("Consol voyage load port is populated from route", allocationRoute.RCA_LoadLocation, consol.Transports[0].JW_RL_NKLoadPort);
			AssertEquals("Consol voyage discharge port is populated from route", allocationRoute.RCA_DischargeLocation, consol.Transports[0].JW_RL_NKDiscPort);
			AssertEquals("Consol voyage vessel is populated from route", allocationRoute.RCA_RV_NKVessel, consol.Transports[0].JW_Vessel);
			AssertEquals("Consol voyage number is populated from route", allocationRoute.RCA_VoyageNumber, consol.Transports[0].JW_VoyageFlight);
			AssertEquals("Consol service string is populated from route", allocationRoute.RCA_ServiceLoop, consol.Transports[0].JW_ServiceString);
		}

		public void TestConsolVoyageFieldsAreNotPopulatedFromRouteIfNotBlank()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "NEWCARD";

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_OH = carrier.PK;

			var contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_OH = carrier.PK;
			contract.RCT_ContractNumber = "VERYNICE";
			contract.RCT_ContractType = RatingContractTypes.Provider;

			var allocationRoute = contract.Allocations.AddNew();
			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "GBFXT";
			allocationRoute.RCA_AllocationLineID = "NOCANDO";
			allocationRoute.RCA_RV_NKVessel = "GOTAN830RESATDORSIA";
			allocationRoute.RCA_VoyageNumber = "IMPRESSIVE";
			allocationRoute.RCA_AllocatedQuantity = 420;
			allocationRoute.RCA_AllocatedUQ = "CN";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "USNYC";
			consol.JK_RL_NKDischargePort = "AUMEL";
			consol.Transports[0].JW_TransportMode = Core.Constants.TransportModes.Sea;
			consol.Transports[0].JW_RL_NKLoadPort = "USLAX";
			consol.Transports[0].JW_RL_NKDiscPort = "NZAKL";
			consol.Transports[0].JW_Vessel = "MAERSK TOBA";
			consol.Transports[0].JW_VoyageFlight = "326";

			Factory.Save();

			AssertEquals("Precondition: one transport leg", 1, consol.Transports.Count);

			consol.JK_RCA_AllocationLine = allocationRoute.PK;

			AssertNotEquals("Consol load port is populated from route", allocationRoute.RCA_LoadLocation, consol.JK_RL_NKLoadPort);
			AssertNotEquals("Consol discharge port is populated from route", allocationRoute.RCA_DischargeLocation, consol.JK_RL_NKDischargePort);
			AssertNotEquals("Consol voyage load port is not populated from route", allocationRoute.RCA_LoadLocation, consol.Transports[0].JW_RL_NKLoadPort);
			AssertNotEquals("Consol voyage discharge port is not populated from route", allocationRoute.RCA_DischargeLocation, consol.Transports[0].JW_RL_NKDiscPort);
			AssertNotEquals("Consol voyage vessel is not populated from route", allocationRoute.RCA_RV_NKVessel, consol.Transports[0].JW_Vessel);
			AssertNotEquals("Consol voyage number is not populated from route", allocationRoute.RCA_VoyageNumber, consol.Transports[0].JW_VoyageFlight);
		}

		public void TestAllocationRouteScheduleDetailsDefaulting()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "NEWCARD";

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_OH = carrier.PK;

			var contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_OH = carrier.PK;
			contract.RCT_ContractNumber = "VERYNICE";
			contract.RCT_ContractType = RatingContractTypes.Provider;

			var allocationRoute = contract.Allocations.AddNew();
			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "GBFXT";
			allocationRoute.RCA_AllocationLineID = "NOCANDO";
			allocationRoute.RCA_RV_NKVessel = "GOTAN830RESATDORSIA";
			allocationRoute.RCA_VoyageNumber = "IMPRESSIVE";
			allocationRoute.RCA_AllocatedQuantity = 420;
			allocationRoute.RCA_AllocatedUQ = "CN";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "USNYC";
			consol.JK_RL_NKDischargePort = "AUMEL";
			consol.Transports[0].JW_RL_NKLoadPort = "USLAX";
			consol.Transports[0].JW_RL_NKDiscPort = "NZAKL";
			consol.Transports[0].JW_VoyageFlight = "IMPRESSIVE";
			consol.Transports[0].JW_TransportMode = Core.Constants.TransportModes.Sea;

			consol.JK_RCA_AllocationLine = allocationRoute.PK;

			AssertEquals("Partial match: Consol vessel is populated from route", allocationRoute.RCA_RV_NKVessel, consol.Transports[0].JW_Vessel);
			AssertEquals("Partial match: Consol service string is populated from route", allocationRoute.RCA_ServiceLoop, consol.Transports[0].JW_ServiceString);
		}

		public void TestSettingJK_IsHazardousTriggersContractNumberValidation()
		{
			var consol = Factory.New<ForwardingConsol>();

			var wasContractNumberValidationRun = false;
			consol.JK_CarrierContractNumberInfo.AdditionalValidation += () => wasContractNumberValidationRun = true;

			consol.JK_IsHazardous = true;
			Assert("Setting JK_IsHazardous should have triggered validation of JK_CarrierContractNumber.", wasContractNumberValidationRun);
		}

		public void TestAllocationRouteValidationWhenJK_ConsolModeIsUpdated()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_ContractNumber = "TOAST";
			contract.RCT_TransportMode = Core.Constants.TransportModes.Sea;
			contract.RCT_ContractType = RatingContractTypes.Provider;
			contract.RCT_OH = carrier.PK;

			var allocationRoute = contract.Allocations.AddNew();
			allocationRoute.RCA_AllocationLineID = "BEANS";
			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "NZAKL";
			allocationRoute.RCA_AllowGroupageOnly = true;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_ConsolMode = Constants.ContainerModes.Groupage;
			consol.JK_CarrierContractNumber = contract.RCT_ContractNumber;
			consol.JK_RCA_AllocationLine = allocationRoute.PK;

			const string errorMessage = "Allocation Route BEANS is restricted to Consols with Groupage container mode. The Consol's current container mode is FCL.";

			AssertNoErrorContaining("Consol's container mode is GRP", consol.JK_RCA_AllocationLineInfo, errorMessage);

			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			AssertHasError("Consol's container mode is not GRP", consol.JK_RCA_AllocationLineInfo, errorMessage);
		}

		public void TestContainerAllocationRouteValidationWhenJK_ConsolModeIsUpdated()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_ContractNumber = "TOAST";
			contract.RCT_TransportMode = Core.Constants.TransportModes.Sea;
			contract.RCT_ContractType = RatingContractTypes.Provider;
			contract.RCT_OH = carrier.PK;

			var allocationRoute = contract.Allocations.AddNew();
			allocationRoute.RCA_AllocationLineID = "BEANS";
			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "NZAKL";
			allocationRoute.RCA_AllowGroupageOnly = true;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_ConsolMode = Constants.ContainerModes.Groupage;
			consol.JK_CarrierContractNumber = contract.RCT_ContractNumber;

			var container = consol.Containers.AddNew();
			container.JC_RCA_AllocationLine = allocationRoute.PK;

			const string errorMessage = "Allocation Route BEANS is restricted to Consols with Groupage container mode. The Consol's current container mode is FCL.";

			AssertNoErrorContaining("Consol's container mode is GRP", container.JC_RCA_AllocationLineInfo, errorMessage);

			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			AssertHasError("Consol's container mode is not GRP", container.JC_RCA_AllocationLineInfo, errorMessage);
		}

		public void TestAllocationRouteValidationWhenForwardersAreUpdated()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierContract = Factory.NewWithValidTestData<RatingContract>();
			carrierContract.RCT_ContractNumber = "CHICKENJOCKEY";
			carrierContract.RCT_TransportMode = TransportModes.Sea;
			carrierContract.RCT_ContractType = RatingContractTypes.Provider;
			carrierContract.RCT_OH = carrier.PK;

			var allocationRoute = carrierContract.Allocations.AddNew();
			allocationRoute.RCA_AllocationLineID = "CHICKEN";
			allocationRoute.RCA_StartDate = ZDate.Today.AddDays(-5);
			allocationRoute.RCA_AllocatedQuantity = 5;
			allocationRoute.RCA_AllocatedUQ = "TU";
			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "NZAKL";

			var agent = Factory.NewWithValidTestData<OrgHeader>();
			agent.OH_Code = "DENNIS";
			var randomOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			randomOrg1.OH_Code = "STEVE";
			var randomOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			randomOrg2.OH_Code = "GARRETT";
			allocationRoute.AgentPivots.AddRelatedIfNotExist(agent);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_CarrierContractNumber = carrierContract.RCT_ContractNumber;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = agent.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = randomOrg2.MainAddress.PK;
			consol.JK_RCA_AllocationLine = allocationRoute.PK;

			const string errorMessage = "Neither Sending Agent STEVE nor Receiving Agent GARRETT of the Consol matches with the Agents specified on Allocation Route CHICKEN under Carrier Contract CHICKENJOCKEY.";

			consol.JK_RCA_AllocationLine = allocationRoute.PK;
			AssertNoErrorContaining("No errors when Consol's Sending/Receiving Agent contains Allocation Route's Agent.", consol.JK_RCA_AllocationLineInfo, errorMessage);

			consol.JK_OA_SendingForwarderAddress = randomOrg1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = randomOrg2.MainAddress.PK;
			AssertHasErrorContaining("Error when Consol's Sending/Receiving Agent does not contain Allocation Route's Agent.", consol.JK_RCA_AllocationLineInfo, errorMessage);

			consol.JK_OA_SendingForwarderAddress = randomOrg1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = agent.MainAddress.PK;
			AssertNoErrorContaining("No errors when Consol's Sending/Receiving Agent contains Allocation Route's Agent.", consol.JK_RCA_AllocationLineInfo, errorMessage);

			consol.JK_OA_ReceivingForwarderAddress = randomOrg2.MainAddress.PK;
			AssertHasErrorContaining("Error when Consol's Sending/Receiving Agent does not contain Allocation Route's Agent.", consol.JK_RCA_AllocationLineInfo, errorMessage);
		}

		public void TestContainerAllocationRouteValidationWhenForwardersAreUpdated()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierContract = Factory.NewWithValidTestData<RatingContract>();
			carrierContract.RCT_ContractNumber = "CHICKENJOCKEY";
			carrierContract.RCT_TransportMode = TransportModes.Sea;
			carrierContract.RCT_ContractType = RatingContractTypes.Provider;
			carrierContract.RCT_OH = carrier.PK;

			var allocationRoute = carrierContract.Allocations.AddNew();
			allocationRoute.RCA_AllocationLineID = "CHICKEN";
			allocationRoute.RCA_StartDate = ZDate.Today.AddDays(-5);
			allocationRoute.RCA_AllocatedQuantity = 5;
			allocationRoute.RCA_AllocatedUQ = "TU";
			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "NZAKL";

			var agent = Factory.NewWithValidTestData<OrgHeader>();
			agent.OH_Code = "DENNIS";
			var randomOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			randomOrg1.OH_Code = "STEVE";
			var randomOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			randomOrg2.OH_Code = "GARRETT";
			allocationRoute.AgentPivots.AddRelatedIfNotExist(agent);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_CarrierContractNumber = carrierContract.RCT_ContractNumber;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = agent.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = randomOrg2.MainAddress.PK;
			consol.JK_RCA_AllocationLine = allocationRoute.PK;

			const string errorMessage = "Neither Sending Agent STEVE nor Receiving Agent GARRETT of the Consol matches with the Agents specified on Allocation Route CHICKEN under Carrier Contract CHICKENJOCKEY.";

			var container = consol.Containers.AddNew();
			container.JC_RCA_AllocationLine = allocationRoute.PK;
			AssertNoErrorContaining("No errors when Consol's Sending/Receiving Agent contains Allocation Route's Agent.", container.JC_RCA_AllocationLineInfo, errorMessage);

			consol.JK_OA_SendingForwarderAddress = randomOrg1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = randomOrg2.MainAddress.PK;
			AssertHasErrorContaining("Error when Consol's Sending/Receiving Agent does not contain Allocation Route's Agent.", container.JC_RCA_AllocationLineInfo, errorMessage);

			consol.JK_OA_SendingForwarderAddress = randomOrg1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = agent.MainAddress.PK;
			AssertNoErrorContaining("No errors when Consol's Sending/Receiving Agent contains Allocation Route's Agent.", container.JC_RCA_AllocationLineInfo, errorMessage);

			consol.JK_OA_ReceivingForwarderAddress = randomOrg2.MainAddress.PK;
			AssertHasErrorContaining("Error when Consol's Sending/Receiving Agent does not contain Allocation Route's Agent.", container.JC_RCA_AllocationLineInfo, errorMessage);
		}

		#endregion

		#region Implementation

		ForwardingConsol Consol
		{
			get { return consol ?? (consol = (ForwardingConsol)GetNewConsol()); }
		}
		ForwardingConsol consol;

		protected override CommonConsol GetNewConsol()
		{
			return Factory.New<ForwardingConsol>();
		}

		#endregion

		#region TestConsolAddressContactOverride

		public void TestConsolAddressContactOverride()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			var organization = Factory.New<OrgHeader>();
			organization.OH_Code = "TestOrg";

			var address = Factory.New<OrgAddress>();
			address.OA_OH = organization.PK;

			var contact = Factory.New<OrgContact>();
			contact.OC_ContactName = "Tom";
			contact.OC_OH = organization.PK;

			var target = organization.MasterBillShipperOverrideDocumentaryAddress;
			target.E2_OA_Address = address.PK;
			target.E2_Contact = contact.OC_ContactName;

			consol.JK_OA_SendingForwarderAddress = address.PK;
			AssertEquals("defaulted from the contact of sender agent's MWB shipper overridden address", contact.OC_ContactName, consol.MasterBillShipperOverrideDocumentaryAddress.E2_Contact);
		}

		#endregion

		#region Electronic Bill Of Lading

		public void TestJK_ElectronicBillOfLadingType()
		{
			var consol = (ForwardingConsol)GetNewConsol();
			consol.JK_ElectronicBillOfLadingType = ZString.Empty;
			Factory.Save();
			AssertNullOrEmpty(consol.JK_ElectronicBillOfLadingType);
			Assert(consol.JK_ElectronicBillOfLadingTypeInfo.ReadOnly);

			consol.JK_ElectronicBillOfLadingType = BillOfLadingBillType.Codes.Straight;
			Factory.Save();
			AssertEquals(BillOfLadingBillType.Codes.Straight, consol.JK_ElectronicBillOfLadingType);
			Assert(consol.JK_ElectronicBillOfLadingTypeInfo.ReadOnly);

			consol.JK_ElectronicBillOfLadingType = BillOfLadingBillType.Codes.ToOrder;
			Factory.Save();
			AssertEquals(BillOfLadingBillType.Codes.ToOrder, consol.JK_ElectronicBillOfLadingType);
			Assert(consol.JK_ElectronicBillOfLadingTypeInfo.ReadOnly);

			consol.JK_ElectronicBillOfLadingType = BillOfLadingBillType.Codes.BlankEndorse;
			Factory.Save();
			AssertEquals(BillOfLadingBillType.Codes.BlankEndorse, consol.JK_ElectronicBillOfLadingType);
			Assert(consol.JK_ElectronicBillOfLadingTypeInfo.ReadOnly);
		}

		public void TestJK_ElectronicBillOfLadingTerms()
		{
			var consol = (ForwardingConsol)GetNewConsol();
			consol.JK_ElectronicBillOfLadingTerms = ZString.Empty;
			Factory.Save();
			AssertNullOrEmpty(consol.JK_ElectronicBillOfLadingTerms);
			Assert(consol.JK_ElectronicBillOfLadingTermsInfo.ReadOnly);

			consol.JK_ElectronicBillOfLadingTerms = BillOfLadingBillTerms.Codes.Transferable;
			Factory.Save();
			AssertEquals(BillOfLadingBillTerms.Codes.Transferable, consol.JK_ElectronicBillOfLadingTerms);
			Assert(consol.JK_ElectronicBillOfLadingTermsInfo.ReadOnly);

			consol.JK_ElectronicBillOfLadingTerms = BillOfLadingBillTerms.Codes.NonTransferable;
			Factory.Save();
			AssertEquals(BillOfLadingBillTerms.Codes.NonTransferable, consol.JK_ElectronicBillOfLadingTerms);
			Assert(consol.JK_ElectronicBillOfLadingTermsInfo.ReadOnly);
		}

		public void TestJK_Calc_BillOfLadingBillStatus()
		{
			var consol = (ForwardingConsol)GetNewConsol();
			Factory.Save();

			AssertNullOrEmpty(consol.JK_Calc_BillOfLadingBillStatus);

			AddBillStatusUpdatedEvent(consol, "InvalidType");
			AssertNullOrEmpty(consol.JK_Calc_BillOfLadingBillStatus);

			AddBillStatusUpdatedEvent(consol);
			AssertEquals(FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillReceived, consol.JK_Calc_BillOfLadingBillStatus);

			AddBillStatusUpdatedEvent(consol, BillStatusUpdatedTypes.AmendmentDenied);
			AssertEquals(FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillReceived, consol.JK_Calc_BillOfLadingBillStatus);

			AddBillStatusUpdatedEvent(consol, BillStatusUpdatedTypes.AmendmentBillReceived);
			AssertEquals(FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillReceived, consol.JK_Calc_BillOfLadingBillStatus);

			AddBillStatusUpdatedEvent(consol, BillStatusUpdatedTypes.OriginalBillTransferred);
			AssertEquals(FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillTransferred, consol.JK_Calc_BillOfLadingBillStatus);

			AddBillStatusUpdatedEvent(consol, BillStatusUpdatedTypes.AmendmentRequested);
			AssertEquals(FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillAmendmentInProgress, consol.JK_Calc_BillOfLadingBillStatus);

			AddBillStatusUpdatedEvent(consol, BillStatusUpdatedTypes.AmendmentGranted);
			AssertEquals(FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillAmendmentInProgress, consol.JK_Calc_BillOfLadingBillStatus);

			AddBillStatusUpdatedEvent(consol, BillStatusUpdatedTypes.SwitchedToPaper, true);
			AssertEquals(FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillAmendmentInProgress, consol.JK_Calc_BillOfLadingBillStatus);

			AddBillStatusUpdatedEvent(consol, BillStatusUpdatedTypes.SwitchedToPaper);
			AssertEquals(FreightConstants.BillOfLadingBillStatus.Codes.SwitchedToPaper, consol.JK_Calc_BillOfLadingBillStatus);

			AddBillStatusUpdatedEvent(consol, BillStatusUpdatedTypes.Surrendered);
			AssertEquals(FreightConstants.BillOfLadingBillStatus.Codes.Surrendered, consol.JK_Calc_BillOfLadingBillStatus);

			AddBillStatusUpdatedEvent(consol, "InvalidType");
			AssertEquals(FreightConstants.BillOfLadingBillStatus.Codes.Surrendered, consol.JK_Calc_BillOfLadingBillStatus);
		}

		public void TestJK_Calc_BillOfLadingBillDate()
		{
			var consol = (ForwardingConsol)GetNewConsol();
			Factory.Save();

			AssertEquals(ZDateTime.Empty, consol.JK_Calc_BillOfLadingBillDate);

			AddBillStatusUpdatedEvent(consol, "InvalidType");
			AssertEquals(ZDateTime.Empty, consol.JK_Calc_BillOfLadingBillDate);

			AddBillStatusUpdatedEvent(consol);
			AssertEquals(consol.Logs.MostRecentLogByPostedDate.PostedLocalBranchTime, consol.JK_Calc_BillOfLadingBillDate);

			AddBillStatusUpdatedEvent(consol, BillStatusUpdatedTypes.AmendmentDenied);

			AssertEquals(consol.Logs.MostRecentLogByPostedDate.PostedLocalBranchTime, consol.JK_Calc_BillOfLadingBillDate);

			AddBillStatusUpdatedEvent(consol, BillStatusUpdatedTypes.AmendmentRequested);
			var postedLocalBranchTime = consol.Logs.MostRecentLogByPostedDate.PostedLocalBranchTime;
			AssertEquals(postedLocalBranchTime, consol.JK_Calc_BillOfLadingBillDate);

			AddBillStatusUpdatedEvent(consol, BillStatusUpdatedTypes.AmendmentDenied, true);
			AssertEquals(postedLocalBranchTime, consol.JK_Calc_BillOfLadingBillDate);

			AddBillStatusUpdatedEvent(consol, "InvalidType");
			AssertEquals(postedLocalBranchTime, consol.JK_Calc_BillOfLadingBillDate);
		}

		void AddBillStatusUpdatedEvent(ForwardingConsol consol, string type = BillStatusUpdatedTypes.OriginalBillPublished, bool isCancel = false)
		{
			var eventParameters = new[]
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, type)
			};

			var log = consol.Logs.AddNew(Events.BillStatusUpdated, eventParameters);

			if (isCancel)
			{
				log.Cancel();
			}

			Factory.Save();
			Thread.Sleep(1);
		}

		#endregion

		#region Co2e

		public void TestIfStatusIsNotChangedToNCU_For_Shipment_When_LegsAreAddedOrDeletedFromConsol()
		{
			// Arrange
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "BEANR";
			consol.JK_RL_NKDischargePort = "AUFRE";
			consol.SetCO2eStatus(CO2eStatusList.Codes.Current);

			var transport = consol.Transports.AddNew("BEANR", "PAMIT");
			consol.Transports.AddNew("PAMIT", "AUMEL");
			consol.Transports.AddNew("AUMEL", "AUFRE");

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKLoadPort = "BEANR";
			shipment.JS_RL_NKDischargePort = "AUFRE";

			AssertEquals(4, ((IRoutingSupport)consol).TransportsIncludingRelated.Count);
			AssertEquals(4, consol.Transports.Count);
			AssertEquals(4, shipment.TransportsIncludingRelated.Count);
			shipment.SetCO2eStatus(CO2eStatusList.Codes.Current);

			// Act
			consol.Transports.RemoveAndDelete(transport);

			// Assert
			AssertEquals(3, consol.Transports.Count);
			AssertEquals(3, shipment.TransportsIncludingRelated.Count);
			AssertEquals(CO2eStatusList.Codes.NotCurrent, shipment.GetCO2eStatus());

			// Arrange
			shipment.SetCO2eStatus(CO2eStatusList.Codes.Current);

			// Act
			consol.Transports.AddNew("BEANR", "PAMIT");

			// Assert
			AssertEquals(4, consol.Transports.Count);
			AssertEquals(4, shipment.TransportsIncludingRelated.Count);
			AssertEquals(CO2eStatusList.Codes.NotCurrent, shipment.GetCO2eStatus());
		}

		public void TestIfStatusIsChangedToNCU_For_AttachedConsolAndShipments_When_ContainerTypeIsChanged()
		{
			//Arrange
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_TransportMode = TransportModes.Sea;
			var shipment = consol.Shipments.AddNew();
			var container = consol.Containers.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();

			container.JC_RC = RC_20GP_PK;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			container.AddPackLine(packLine);

			shipment.SetCO2eStatus(CO2eStatusList.Codes.Current);
			consol.SetCO2eStatus(CO2eStatusList.Codes.Current);

			//Action
			container.JC_RC = RC_40GP_PK;

			//Assert
			AssertEquals(CO2eStatusList.Codes.NotCurrent, shipment.GetCO2eStatus());
			AssertEquals(CO2eStatusList.Codes.NotCurrent, consol.GetCO2eStatus());
			CO2eBusinessTestHelper.AssertSTUEvent(shipment, $"JC_RC [{RC_20GP_PK}]->[{RC_40GP_PK}]");
			CO2eBusinessTestHelper.AssertSTUEvent(consol, $"JC_RC [{RC_20GP_PK}]->[{RC_40GP_PK}]");
		}

		public void TestIfStatusIsChangedToNCU_For_AttachedConsolAndShipments_When_ContainerCountIsChanged()
		{
			//Arrange
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();

			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();

			var packLine1 = shipment1.OuterPackLines.AddNew();
			var packLine2 = shipment2.OuterPackLines.AddNew();

			container1.JC_RC = RC_20GP_PK;
			shipment1.JS_PackingMode = Constants.ContainerModes.FCL;
			container1.AddPackLine(packLine1);
			container2.AddPackLine(packLine2);

			shipment1.SetCO2eStatus(CO2eStatusList.Codes.Current);
			shipment2.SetCO2eStatus(CO2eStatusList.Codes.Current);
			consol.SetCO2eStatus(CO2eStatusList.Codes.Current);
			container1.JC_ContainerCount = 1;

			//Action
			container1.JC_ContainerCount = 2;

			//Assert
			AssertEquals(CO2eStatusList.Codes.NotCurrent, shipment1.GetCO2eStatus());
			AssertEquals(CO2eStatusList.Codes.NotCurrent, consol.GetCO2eStatus());
			CO2eBusinessTestHelper.AssertSTUEvent(shipment1, "JC_ContainerCount [1]->[2]");
			CO2eBusinessTestHelper.AssertSTUEvent(consol, "JC_ContainerCount [1]->[2]");
			AssertEquals(CO2eStatusList.Codes.Current, shipment2.GetCO2eStatus());
		}

		public void TestCo2eStatusIsNCU_ForContainer_WhenConsolDepartureCFSAddressIsChanged()
		{
			/* con1 and con2 are defined in the setter of JK_OA_PackDepotAddress */
			//Arrange
			var address1 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			address1.OA_RL_NKRelatedPortCode = "AUADL";

			var address2 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			address2.OA_RL_NKRelatedPortCode = "AUMEL";

			var pickUpFromAddress = Factory.NewWithValidTestData<OrgHeader>();
			var pickUpCFSAddress = Factory.NewWithValidTestData<OrgHeader>();

			var consol = CO2eTestHelper.CreateForwardingConsolWithLegs(Factory) as ForwardingConsol;
			consol.JK_OA_PackDepotAddress = address1.PK;
			consol.JK_AgentType = Constants.AgentType.Direct;

			var container = Factory.NewWithValidTestData<ForwardingContainer>();
			container.GetOrCreateJobCO2e(CO2eTypes.EmptyPickup);
			container.SetCO2eStatus(CO2eStatusList.Codes.Current, CO2eTypes.EmptyPickup);

			consol.Containers.Add(container);
			consol.SetCO2eStatus(CO2eStatusList.Codes.Current);

			var shipment = consol.Shipments.AddNew();
			//con1 && !con2
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.ConsignorPickupAddress.E2_OA_Address = pickUpFromAddress.PK;
			shipment.JS_OA_ExportReceivingDepot = pickUpCFSAddress.PK;

			container.SetCO2eStatus(CO2eStatusList.Codes.Current, CO2eTypes.EmptyPickup);

			AssertEquals(CO2eStatusList.Codes.Current, container.GetCO2eStatus(CO2eTypes.EmptyPickup));

			//Action
			consol.JK_OA_PackDepotAddress = address2.PK;

			//Assert
			AssertEquals(CO2eStatusList.Codes.Current, container.GetCO2eStatus(CO2eTypes.EmptyPickup));
			AssertNoWarning(container.TotalCO2eForEmptyPickupForBindingInfo, CO2eBusinessTestHelper.CO2eStaleWarning);

			//Arrange
			//!con1 && con2
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.ConsignorPickupAddress.E2_OA_Address = pickUpFromAddress.PK;
			shipment.JS_OA_ExportReceivingDepot = Guid.Empty;

			container.SetCO2eStatus(CO2eStatusList.Codes.Current, CO2eTypes.EmptyPickup);

			AssertEquals(CO2eStatusList.Codes.Current, container.GetCO2eStatus(CO2eTypes.EmptyPickup));

			//Action
			consol.JK_OA_PackDepotAddress = address1.PK;

			//Assert
			AssertEquals(CO2eStatusList.Codes.Current, container.GetCO2eStatus(CO2eTypes.EmptyPickup));
			AssertNoWarning(container.TotalCO2eForEmptyPickupForBindingInfo, CO2eBusinessTestHelper.CO2eStaleWarning);

			//Arrange
			//!con1 && !con2
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.ConsignorPickupAddress.E2_OA_Address = Guid.Empty;
			shipment.JS_OA_ExportReceivingDepot = Guid.Empty;

			container.SetCO2eStatus(CO2eStatusList.Codes.Current, CO2eTypes.EmptyPickup);

			AssertEquals(CO2eStatusList.Codes.Current, container.GetCO2eStatus(CO2eTypes.EmptyPickup));
			AssertNoWarning(container.TotalCO2eForEmptyPickupForBindingInfo, CO2eBusinessTestHelper.CO2eStaleWarning);

			//Action
			Factory.Save();
			consol.JK_OA_PackDepotAddress = address2.PK;

			//Assert
			AssertEquals(CO2eStatusList.Codes.NotCurrent, container.GetCO2eStatus(CO2eTypes.EmptyPickup));
			AssertHasWarning(container.TotalCO2eForEmptyPickupForBindingInfo, CO2eBusinessTestHelper.CO2eStaleWarning);
			CO2eBusinessTestHelper.AssertSTUEvent(container, $"JK_OA_PackDepotAddress [{address1.PK}]->[{address2.PK}]");
		}

		public void TestCo2eStatusIsNCU_WhenConsolDepartureCFSAddressIsChanged()
		{
			//Arrange
			var address1 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			address1.OA_RL_NKRelatedPortCode = "AUADL";

			var address2 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			address2.OA_RL_NKRelatedPortCode = "AUMEL";

			var consol = CO2eTestHelper.CreateForwardingConsolWithLegs(Factory) as ForwardingConsol;
			consol.JK_OA_PackDepotAddress = address1.PK;
			consol.JK_AgentType = Constants.AgentType.AWBCoload;

			var shipment = consol.Shipments.AddNew();

			var container = Factory.NewWithValidTestData<ForwardingContainer>();
			container.GetOrCreateJobCO2e(CO2eTypes.EmptyPickup);
			consol.Containers.Add(container);
			container.SetCO2eStatus(CO2eStatusList.Codes.Current, CO2eTypes.EmptyPickup);
			shipment.SetCO2eStatus(CO2eStatusList.Codes.Current);

			AssertEquals(CO2eStatusList.Codes.Current, shipment.GetCO2eStatus());
			AssertEquals(CO2eStatusList.Codes.Current, container.GetCO2eStatus(CO2eTypes.EmptyPickup));

			//Action
			Factory.Save();
			consol.JK_OA_PackDepotAddress = address2.PK;

			//Assert
			AssertEquals(CO2eStatusList.Codes.NotCurrent, shipment.GetCO2eStatus());
			AssertEquals(CO2eStatusList.Codes.NotCurrent, container.GetCO2eStatus(CO2eTypes.EmptyPickup));
			AssertHasWarning(shipment.TotalCO2eForBindingInfo, CO2eBusinessTestHelper.CO2eStaleWarning);
			AssertHasWarning(container.TotalCO2eForEmptyPickupForBindingInfo, CO2eBusinessTestHelper.CO2eStaleWarning);
			CO2eBusinessTestHelper.AssertSTUEvent(shipment, $"JK_OA_PackDepotAddress [{address1.PK}]->[{address2.PK}]");
		}

		public void TestCo2eStatusIsNCU_WhenConsolDepartureCFSAddressTransportModeIsChanged()
		{
			//Arrange
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_OA_PackDepotAddress_ZAddress.OrgPK = Guid.NewGuid();
			consol.CFSDepartureByTransportMode = "ROA";

			var shipment = consol.Shipments.AddNew();
			shipment.SetCO2eStatus(CO2eStatusList.Codes.Current);

			AssertEquals("ROA", consol.CFSDepartureByTransportMode);
			AssertEquals(CO2eStatusList.Codes.Current, shipment.GetCO2eStatus());

			//Action
			consol.CFSDepartureByTransportMode = "IWT";

			//Assert
			AssertEquals("IWT", consol.CFSDepartureByTransportMode);
			AssertEquals(CO2eStatusList.Codes.NotCurrent, shipment.GetCO2eStatus());
			AssertHasWarning(shipment.TotalCO2eForBindingInfo, CO2eBusinessTestHelper.CO2eStaleWarning);
			CO2eBusinessTestHelper.AssertSTUEvent(shipment, "CFSDepartureByTransportMode [ROA]->[IWT]");
		}

		[TestDate(2024, 1, 1)]
		public void TestCo2eStatusIsNCU_ForShipments_WhenContainerCo2eStatusIsChanged()
		{
			//Arrange
			var shipment = Factory.New<ForwardingShipment>();
			var packLine1 = shipment.OuterPackLines.AddNew();
			var consol = shipment.Consols.AddNew();
			shipment.SetCO2eStatus(CO2eStatusList.Codes.Current);
			var container = consol.Containers.AddNew();
			container.AddPackLine(packLine1);

			//Act
			consol.SetCO2eStatus(CO2eStatusList.Codes.Current);
			shipment.SetCO2eStatus(CO2eStatusList.Codes.Current);
			container.SetCO2eStatus(CO2eStatusList.Codes.Current, CO2eTypes.EmptyReturn);
			container.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent, CO2eTypes.EmptyReturn);

			//Assert
			AssertEquals(CO2eStatusList.Codes.NotCurrent, shipment.GetCO2eStatus());
			AssertHasWarning(shipment.TotalCO2eForBindingInfo, CO2eBusinessTestHelper.CO2eStaleWarning);
			CO2eBusinessTestHelper.AssertSTUEvent(shipment, "Container");

			//Act
			TestDateAttribute.AddMinutes(1);
			consol.SetCO2eStatus(CO2eStatusList.Codes.Current);
			shipment.SetCO2eStatus(CO2eStatusList.Codes.Current);
			container.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent, CO2eTypes.EmptyPickup);

			//Assert
			AssertEquals(CO2eStatusList.Codes.NotCurrent, shipment.GetCO2eStatus());
			AssertHasWarning(shipment.TotalCO2eForBindingInfo, CO2eBusinessTestHelper.CO2eStaleWarning);
			CO2eBusinessTestHelper.AssertSTUEvent(shipment, "Container");
		}

		public void TestCo2eStatusIsNCU_ForContainer_WhenConsolArrivalCFSAddressIsChanged()
		{
			/* con1 and con2 are defined in the setter of JK_OA_UnpackDepotAddress */
			// Arrange
			var address1 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			address1.OA_RL_NKRelatedPortCode = "AUADL";

			var address2 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			address2.OA_RL_NKRelatedPortCode = "AUMEL";

			var deliveryCFSAddress = Factory.NewWithValidTestData<OrgHeader>();
			var consigneeDeliveryAddress = Factory.NewWithValidTestData<OrgHeader>();

			var consol = CO2eTestHelper.CreateForwardingConsolWithLegs(Factory) as ForwardingConsol;
			consol.JK_OA_UnpackDepotAddress = address1.PK;
			consol.JK_AgentType = Constants.AgentType.Direct;

			var container = Factory.NewWithValidTestData<ForwardingContainer>();
			container.GetOrCreateJobCO2e(CO2eTypes.EmptyReturn);
			container.SetCO2eStatus(CO2eStatusList.Codes.Current, CO2eTypes.EmptyReturn);
			consol.Containers.Add(container);

			var shipment = consol.Shipments.AddNew();
			//con1 && !con2
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.ConsigneeDeliveryAddress.OrganisationPK = deliveryCFSAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = consigneeDeliveryAddress.PK;

			container.SetCO2eStatus(CO2eStatusList.Codes.Current, CO2eTypes.EmptyReturn);

			AssertEquals(CO2eStatusList.Codes.Current, container.GetCO2eStatus(CO2eTypes.EmptyReturn));

			//Action
			consol.JK_OA_UnpackDepotAddress = address2.PK;

			//Assert
			AssertEquals(CO2eStatusList.Codes.Current, container.GetCO2eStatus(CO2eTypes.EmptyReturn));
			AssertNoWarning(container.TotalCO2eForEmptyReturnForBindingInfo, CO2eBusinessTestHelper.CO2eStaleWarning);

			//Arrange
			//!con1 && con2
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.ConsigneeDeliveryAddress.OrganisationPK = Guid.Empty;
			shipment.JS_OA_ImportReleaseDepot = consigneeDeliveryAddress.PK;

			container.SetCO2eStatus(CO2eStatusList.Codes.Current, CO2eTypes.EmptyReturn);

			AssertEquals(CO2eStatusList.Codes.Current, container.GetCO2eStatus(CO2eTypes.EmptyReturn));

			//Action
			consol.JK_OA_UnpackDepotAddress = address1.PK;

			//Assert
			AssertEquals(CO2eStatusList.Codes.Current, container.GetCO2eStatus(CO2eTypes.EmptyReturn));
			AssertNoWarning(container.TotalCO2eForEmptyReturnForBindingInfo, CO2eBusinessTestHelper.CO2eStaleWarning);

			//Arrange
			//!con1 && !con2
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.ConsigneeDeliveryAddress.OrganisationPK = Guid.Empty;
			shipment.JS_OA_ImportReleaseDepot = Guid.Empty;

			container.SetCO2eStatus(CO2eStatusList.Codes.Current, CO2eTypes.EmptyReturn);

			AssertEquals(CO2eStatusList.Codes.Current, container.GetCO2eStatus(CO2eTypes.EmptyReturn));

			//Action
			Factory.Save();
			consol.JK_OA_UnpackDepotAddress = address2.PK;

			//Assert
			AssertEquals(CO2eStatusList.Codes.NotCurrent, container.GetCO2eStatus(CO2eTypes.EmptyReturn));
			AssertHasWarning(container.TotalCO2eForEmptyReturnForBindingInfo, CO2eBusinessTestHelper.CO2eStaleWarning);
			CO2eBusinessTestHelper.AssertSTUEvent(container, $"JK_OA_UnpackDepotAddress [{address1.PK}]->[{address2.PK}]");
		}

		public void TestCo2eStatusIsNCU_WhenConsolArrivalCFSAddressIsChanged()
		{
			// Arrange
			var address1 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			address1.OA_RL_NKRelatedPortCode = "AUADL";

			var address2 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			address2.OA_RL_NKRelatedPortCode = "AUMEL";

			var consol = CO2eTestHelper.CreateForwardingConsolWithLegs(Factory) as ForwardingConsol;
			consol.JK_OA_UnpackDepotAddress = address1.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.SetCO2eStatus(CO2eStatusList.Codes.Current);

			//Action
			Factory.Save();
			consol.JK_OA_UnpackDepotAddress = address2.PK;

			//Assert
			AssertEquals(CO2eStatusList.Codes.NotCurrent, shipment.GetCO2eStatus());
			AssertHasWarning(shipment.TotalCO2eForBindingInfo, CO2eBusinessTestHelper.CO2eStaleWarning);
			CO2eBusinessTestHelper.AssertSTUEvent(shipment, $"JK_OA_UnpackDepotAddress [{address1.PK}]->[{address2.PK}]");
		}

		public void TestCo2eStatusIsNCU_WhenConsolArrivalCFSAddressTransportModeIsChanged()
		{
			//Arrange
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_OA_UnpackDepotAddress_ZAddress.OrgPK = Guid.NewGuid();
			consol.CFSArrivalByTransportMode = "ROA";

			var shipment = consol.Shipments.AddNew();
			shipment.SetCO2eStatus(CO2eStatusList.Codes.Current);

			AssertEquals("ROA", consol.CFSArrivalByTransportMode);
			AssertEquals(CO2eStatusList.Codes.Current, shipment.GetCO2eStatus());

			//Action
			consol.CFSArrivalByTransportMode = "IWT";

			//Assert
			AssertEquals("IWT", consol.CFSArrivalByTransportMode);
			AssertEquals(CO2eStatusList.Codes.NotCurrent, shipment.GetCO2eStatus());
			AssertHasWarning(shipment.TotalCO2eForBindingInfo, CO2eBusinessTestHelper.CO2eStaleWarning);
			CO2eBusinessTestHelper.AssertSTUEvent(shipment, "CFSArrivalByTransportMode [ROA]->[IWT]");
		}

		public void TestTotalCO2eForBinding_Sorting()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.SetCO2eStatus(CO2eStatusList.Codes.Pending);
			AssertEquals("Pending", consol1.TotalCO2eForBinding);
			AssertEquals(0m, consol1.TotalCO2eForSorting);

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.SetCO2ePerTonneInKg(100.1111111m);
			consol2.SetCO2eStatus(CO2eStatusList.Codes.NotCalculated);
			AssertEquals(ZString.Empty, consol2.TotalCO2eForBinding);
			AssertEquals(0m, consol2.TotalCO2eForSorting);

			var consol3 = Factory.New<ForwardingConsol>();
			var shipment3 = consol3.Shipments.AddNew();
			shipment3.JS_ActualWeight = 5m;
			shipment3.JS_UnitOfWeight = Weight.Tonnes;
			consol3.SetCO2ePerTonneInKg(100.1111111m);
			consol3.SetTotalCO2e(500.5555555m);
			Factory.Save();
			AssertEquals("500.556", consol3.TotalCO2eForBinding);
			AssertEquals(500.5555555m, consol3.GetTotalCO2e());
			AssertEquals(500.5555555m, consol3.TotalCO2eForSorting);

			var consol4 = Factory.New<ForwardingConsol>();
			var shipment4 = consol4.Shipments.AddNew();
			shipment4.JS_ActualWeight = 30m;
			shipment4.JS_UnitOfWeight = Weight.Tonnes;
			consol4.SetCO2ePerTonneInKg(100.1111111m);
			consol4.SetTotalCO2e(3003.333333m);
			Factory.Save();
			AssertEquals("3,003.333", consol4.TotalCO2eForBinding);
			AssertEquals(3003.333333m, consol4.GetTotalCO2e());
			AssertEquals(3003.333333m, consol4.TotalCO2eForSorting);

			var consol5 = Factory.New<ForwardingConsol>();
			var shipment5 = consol5.Shipments.AddNew();
			shipment5.JS_ActualWeight = 999999.999m;
			shipment5.JS_UnitOfWeight = Weight.Kilograms;
			consol5.SetCO2ePerTonneInKg(99999.9999999m);
			consol5.SetTotalCO2e(99999999.8999000m);
			Factory.Save();
			AssertEquals("99,999,999.9", consol5.TotalCO2eForBinding);
			AssertEquals(99999999.8999000m, consol5.GetTotalCO2e());
			AssertEquals(99999999.8999000m, consol5.TotalCO2eForSorting);

			Assert("0 equals to 0", consol1.TotalCO2eForSorting == consol2.TotalCO2eForSorting);
			Assert("0 is smaller than 500.556", consol2.TotalCO2eForSorting < consol3.TotalCO2eForSorting);
			Assert("500.556 is smaller than 3003.333", consol3.TotalCO2eForSorting < consol4.TotalCO2eForSorting);
			Assert("3003.333 is smaller than 99999999.9", consol4.TotalCO2eForSorting < consol5.TotalCO2eForSorting);
		}

		public void TestTotalCO2eForSorting_DecimalPlaces()
		{
			var co2eForSortingDp = typeof(ForwardingConsol)
				.GetProperty(nameof(ForwardingConsol.TotalCO2eForSorting))
				.GetCustomAttributes(typeof(DecimalPlacesAttribute), true)[0] as DecimalPlacesAttribute;

			AssertEquals("TotalCO2eForSorting should display using 3dp", 3, co2eForSortingDp.DecimalPlaces);
		}

		public void TestTotalCO2eForBinding_ByWeight()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ActualWeight = 5m;
			shipment.JS_UnitOfWeight = Weight.Tonnes;
			consol.SetCO2ePerTonneInKg(100.1111111m);
			consol.SetTotalCO2e(500.5555555m);
			Factory.Save();

			AssertEquals(CO2eStatusList.Codes.Current, consol.GetCO2eStatus());
			AssertEquals("500.556", consol.TotalCO2eForBinding);
			AssertEquals(500.5555555m, consol.GetTotalCO2e());
			AssertEquals(500.5555555m, consol.TotalCO2eForSorting);

			consol.SetCO2eStatus(CO2eStatusList.Codes.Pending);
			AssertEquals("Pending", consol.TotalCO2eForBinding);
			AssertEquals(500.5555555m, consol.TotalCO2eForSorting);

			consol.SetCO2eStatus(CO2eStatusList.Codes.NotCalculated);
			AssertEquals(ZString.Empty, consol.TotalCO2eForBinding);
		}

		public void TestTotalCO2eForBinding_ByTEU()
		{
			// Arrange
			var consol = CO2eTestHelper.CreateForwardingConsolRequiringTEU(Factory) as ForwardingConsol;
			var supporter = (ICO2eLegBasedSupporter)consol;

			// Act & Assert
			consol.SetCO2ePerTEUInKg(123.456m);
			consol.SetTotalCO2e(530.8608m);
			AssertEquals(CO2eStatusList.Codes.Current, consol.GetCO2eStatus());
			Assert(supporter.RequireTEU);
			AssertEquals("shipment.NumberOfTEU", 1 * 2 + 2.3m * 1m, supporter.NumberOfTEU);
			AssertEquals("530.861", consol.TotalCO2eForBinding);
			AssertEquals(530.8608m, consol.GetTotalCO2e());
			AssertEquals(530.8608m, consol.TotalCO2eForSorting);
		}

		public void TestRequireTEU()
		{
			var refContainerNoTEU = NewRefContainer("20GP111", "22G0", 0m, 2280m);
			void AssertRequireTEU(Action<ForwardingConsol> setup, bool expected = true)
			{
				// Arrange
				var consol = Factory.New<ForwardingConsol>();
				setup(consol);

				// Act & Assert
				AssertEquals(expected, (consol as ICO2eCalculationSupporter).RequireTEU);
			}

			AssertRequireTEU((forwardingConsol) =>
			{
				forwardingConsol.JK_TransportMode = TransportModes.Sea;
				forwardingConsol.JK_ConsolMode = ContainerModes.FCL;
				forwardingConsol.Containers.AddNew();
			});

			AssertRequireTEU((forwardingConsol) =>
			{
				forwardingConsol.JK_TransportMode = TransportModes.Road;
				forwardingConsol.JK_ConsolMode = ContainerModes.FCL;
				forwardingConsol.Containers.AddNew();
				forwardingConsol.JK_TotalShipmentActWeightCheck = 0;
			});

			AssertRequireTEU((forwardingConsol) =>
			{
				forwardingConsol.JK_TransportMode = TransportModes.Rail;
				forwardingConsol.JK_ConsolMode = ContainerModes.FCL;
				forwardingConsol.Containers.AddNew();
				forwardingConsol.JK_TotalShipmentActWeightCheck = 0;
			});

			AssertRequireTEU((forwardingConsol) =>
			{
				forwardingConsol.JK_TransportMode = TransportModes.Sea;
				forwardingConsol.JK_ConsolMode = ContainerModes.FCL;
				var container = forwardingConsol.Containers.AddNew();
				container.JC_RC = refContainerNoTEU.PK;
			}, false);
		}

		public void TestIncludeTEU()
		{
			// Arrange
			var consol = CO2eTestHelper.CreateForwardingConsolRequiringTEU(Factory) as ForwardingConsol;
			var supporter = (ICO2eLegBasedSupporter)consol;
			Assert("Pre-condition", supporter.RequireTEU);
			Assert("Pre-condition", supporter.IncludeTEU);

			// Act & Assert
			consol.JK_TransportMode = TransportModes.Road;
			consol.JK_ConsolMode = ContainerModes.FCL;
			Assert(!supporter.RequireTEU);
			Assert(supporter.IncludeTEU);

			consol.JK_ConsolMode = ContainerModes.ShippersConsol;
			Assert(supporter.IncludeTEU);

			consol.JK_ConsolMode = ContainerModes.LCL;
			Assert(!supporter.RequireTEU);
			Assert(!supporter.IncludeTEU);
		}

		[TestDate(2024, 1, 1)]
		public void TestUpdateCO2eStatusToNotCurrent_WhenWeightChanges()
		{
			// Arrange
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.SetCO2ePerTonneInKg(10);
			AssertEquals(0m, consol.GetTotalCO2e());
			Factory.Save();

			// Act & Assert
			consol.JK_TotalShipmentActWeightCheck = 1;
			consol.WeightVerificationUnit = Weight.Tonnes;
			AssertStatus("JK_TotalShipmentActWeightCheck [0]->[1]");
			Factory.Save();

			TestDateAttribute.AddMinutes(1);
			consol.JK_TotalShipmentActWeightCheck = 500;
			AssertStatus("JK_TotalShipmentActWeightCheck [1]->[500]");

			TestDateAttribute.AddMinutes(1);
			consol.WeightVerificationUnit = Weight.Kilograms;
			AssertStatus("JK_TotalShipmentActOtherUnit [T]->[KG]");

			TestDateAttribute.AddMinutes(1);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ActualWeight = 1000;
			shipment.JS_UnitOfWeight = Weight.Kilograms;
			consol.Shipments.Add(shipment);
			Factory.Save();
			AssertStatus("Shipment added");

			void AssertStatus(string stuEvent)
			{
				AssertEquals(CO2eStatusList.Codes.NotCurrent, consol.GetCO2eStatus());
				AssertEquals(0m, consol.GetTotalCO2e());
				CO2eBusinessTestHelper.AssertSTUEvent(consol, stuEvent);
				consol.SetCO2eStatus(CO2eStatusList.Codes.Current);
			}
		}

		[TestDate(2024, 1, 1)]
		public void TestUpdateCO2eStatusToNotCurrent_WhenShipmentWeightChanges_InAnotherFactory()
		{
			AssertStatus((shipment, _) => shipment.JS_ActualWeight = 500, "Shipment Weight");
			AssertStatus((shipment, _) => shipment.JS_UnitOfWeight = Weight.Tonnes, "Shipment Weight");
			AssertStatus((_, consol) => consol.Shipments.RemoveAll(), "Shipment removed");

			void AssertStatus(Action<ForwardingShipment, ForwardingConsol> change, string stuReason)
			{
				// Arrange
				TestDateAttribute.AddMinutes(1);
				var consol = Factory.New<ForwardingConsol>();
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_ActualWeight = 1000;
				shipment.JS_UnitOfWeight = Weight.Kilograms;
				consol.Shipments.Add(shipment);
				consol.SetCO2ePerTonneInKg(10);
				AssertEquals(CO2eStatusList.Codes.Current, consol.GetCO2eStatus());
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var consol2 = newFactory.Load<ForwardingConsol>(consol.PK);

				// Act
				change(shipment, consol);
				Factory.Save();

				// Assert
				AssertEquals(CO2eStatusList.Codes.NotCurrent, consol2.GetCO2eStatus());
				CO2eBusinessTestHelper.AssertSTUEvent(consol, stuReason);
			}
		}

		[TestDate(2024, 1, 1)]
		public void TestUpdateCO2eStatusToNotCurrent_WhenCO2eStatusCurrent()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.SetCO2ePerTonneInKg(100.1111111m);
			AssertEquals(CO2eStatusList.Codes.Current, consol.GetCO2eStatus());

			Factory.Save();

			TestDateAttribute.AddMinutes(1);
			consol.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals(CO2eStatusList.Codes.NotCurrent, consol.GetCO2eStatus());
			CO2eBusinessTestHelper.AssertSTUEvent(consol, "JK_TransportMode [SEA]->[AIR]");

			TestDateAttribute.AddMinutes(1);
			consol.SetCO2eStatus(CO2eStatusList.Codes.Current);
			consol.JK_RL_NKLoadPort = "AUSYD";
			AssertEquals(CO2eStatusList.Codes.NotCurrent, consol.GetCO2eStatus());
			CO2eBusinessTestHelper.AssertSTUEvent(consol, "JK_RL_NKLoadPort []->[AUSYD]");

			TestDateAttribute.AddMinutes(1);
			consol.SetCO2eStatus(CO2eStatusList.Codes.Current);
			consol.JK_RL_NKDischargePort = "NZAKL";
			AssertEquals(CO2eStatusList.Codes.NotCurrent, consol.GetCO2eStatus());
			CO2eBusinessTestHelper.AssertSTUEvent(consol, "JK_RL_NKDischargePort []->[NZAKL]");
		}

		public void TestUpdateCO2eStatusToNotCurrent_WhenConsoleModeIsChanged()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;

			var refContainer = NewRefContainer("20GP111", "22G0", 1m, 2280m);
			AddContainer(consol, refContainer, 1);

			consol.JK_ConsolMode = ContainerModes.LCL;
			consol.Transports.ForEach(x => (x as Transport).SetCO2eStatus(CO2eStatusList.Codes.Current));
			consol.SetCO2eStatus(CO2eStatusList.Codes.Current);

			AssertEquals(CO2eStatusList.Codes.Current, consol.GetCO2eStatus());

			Factory.Save();

			consol.JK_ConsolMode = ContainerModes.FCL;
			AssertEquals(CO2eStatusList.Codes.NotCurrent, consol.GetCO2eStatus());
			CO2eBusinessTestHelper.AssertSTUEvent(consol, "JK_ConsolMode [LCL]->[FCL]");
		}

		[TestDate(2024, 1, 1)]
		public void TestUpdateCO2eStatusToNotCurrent_WhenRelatedTransportIsNotCurrent()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.SetCO2eStatus(CO2eStatusList.Codes.Current);
			Factory.Save();

			var routingSupport = consol as IRoutingSupport;
			var transport = routingSupport.TransportsIncludingRelated[0];
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
			AssertEquals(CO2eStatusList.Codes.NotCurrent, consol.GetCO2eStatus());
			CO2eBusinessTestHelper.AssertSTUEvent(consol, "Transport");

			TestDateAttribute.AddMinutes(1);
			consol.SetCO2eStatus(CO2eStatusList.Codes.Current);
			transport = routingSupport.TransportsIncludingRelated.AddNew();
			transport.JW_TransportMode = Constants.TransportModes.Air;
			AssertEquals(CO2eStatusList.Codes.NotCurrent, consol.GetCO2eStatus());
			CO2eBusinessTestHelper.AssertSTUEvent(consol, "Transport added");

			TestDateAttribute.AddMinutes(1);
			consol.SetCO2eStatus(CO2eStatusList.Codes.Current);
			transport.SetCO2eStatus(CO2eStatusList.Codes.Current);
			transport.JW_RL_NKLoadPort = "AUMEL";
			AssertEquals(CO2eStatusList.Codes.NotCurrent, consol.GetCO2eStatus());
			CO2eBusinessTestHelper.AssertSTUEvent(consol, "Transport");

			TestDateAttribute.AddMinutes(1);
			consol.SetCO2eStatus(CO2eStatusList.Codes.Current);
			transport.SetCO2eStatus(CO2eStatusList.Codes.Current);
			transport.JW_RL_NKDiscPort = "CNSHA";
			AssertEquals(CO2eStatusList.Codes.NotCurrent, consol.GetCO2eStatus());
			CO2eBusinessTestHelper.AssertSTUEvent(consol, "Transport");

			TestDateAttribute.AddMinutes(1);
			consol.SetCO2eStatus(CO2eStatusList.Codes.Current);
			routingSupport.TransportsIncludingRelated.Remove(transport);
			AssertEquals(CO2eStatusList.Codes.NotCurrent, consol.GetCO2eStatus());
			CO2eBusinessTestHelper.AssertSTUEvent(consol, "Transport removed");
		}

		[TestDate(2024, 1, 1)]
		public void TestUpdateCO2eStatusToNotCurrent_WhenRequireTEUAndContainerChange()
		{
			// Arrange
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.SetCO2eStatus(CO2eStatusList.Codes.Current);

			consol.Containers.RemoveAll();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 2;
			var refContainer1 = NewRefContainer("20GP111", "22G0", 1m, 2280m);
			container1.JC_RC = refContainer1.PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerCount = 1;
			var refContainer2 = NewRefContainer("40REHC111", "45R0", 2.3m, 4420m);
			container2.JC_RC = refContainer2.PK;

			Factory.Save();

			ForwardingContainer container3 = null;

			// Act & Assert
			AssertCO2eStatus(() => container2.JC_ContainerCount = 2, "JC_ContainerCount [1]->[2]");

			AssertCO2eStatus(() => container1.JC_RC = refContainer2.PK, $"JC_RC [{refContainer1.PK}]->[{refContainer2.PK}]");

			AssertCO2eStatus(() => container1.JC_TareWeight = 1000m, $"JC_TareWeight [8840]->[1000]");

			AssertCO2eStatus(() => container1.Delete(), "Container removed");

			AssertCO2eStatus(() => container2.Delete(), "Container removed");

			AssertCO2eStatus(() =>
			{
				container3 = consol.Containers.AddNew();
				container3.JC_ContainerCount = 1;
				container3.JC_RC = refContainer1.PK;
			}, "Container added");

			AssertCO2eStatus(() => consol.Containers.RemoveAndDelete(container3), "Container removed");

			void AssertCO2eStatus(Action action, string stuReason)
			{
				TestDateAttribute.AddMinutes(1);
				consol.SetCO2eStatus(CO2eStatusList.Codes.Current);
				Factory.Save();
				action.Invoke();
				AssertEquals(CO2eStatusList.Codes.NotCurrent, consol.GetCO2eStatus());
				CO2eBusinessTestHelper.AssertSTUEvent(consol, stuReason);
			}
		}

		public void TestDoNotUpdateCO2eStatusToNotCurrent_WhenContainerTareWeightChangesForAir()
		{
			// Arrange
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = ContainerModes.ULD;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.SetCO2eStatus(CO2eStatusList.Codes.Current);

			var container = consol.Containers.AddNew();
			container.JC_TareWeight = 100m;
			var airRefContainer = NewRefContainer("ZZZ", "10A0", 0m, 10m);
			container.JC_RC = airRefContainer.PK;

			Factory.Save();

			// Act
			container.JC_TareWeight = 10000m;

			// Assert
			AssertEquals("RequireTEU is false for AIR, container tare weight does not change CO2 status", CO2eStatusList.Codes.Current, consol.GetCO2eStatus());
		}

		public void TestShouldNotCallRequireTEU_WhenSkipCO2eStatusCheck()
		{
			// Arrange
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			consol.Containers.RemoveAll();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 2;
			var refContainer1 = NewRefContainer("20GP111", "22G0", 1m, 2280m);
			container1.JC_RC = refContainer1.PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerCount = 1;
			var refContainer2 = NewRefContainer("40REHC111", "45R0", 2.3m, 4420m);
			container2.JC_RC = refContainer2.PK;

			Factory.Save();

			ForwardingContainer container3 = null;
			var requireTEUCalled = 0;
			consol.OnRequireTEUCalled += delegate { requireTEUCalled++; };

			// Act & Assert
			AssertRequireTEUNotCalled(() => container2.JC_ContainerCount = 2);
			AssertRequireTEUNotCalled(() => container1.JC_RC = refContainer2.PK);
			AssertRequireTEUNotCalled(() => container1.JC_TareWeight = 1000m);
			AssertRequireTEUNotCalled(() => container1.Delete());
			AssertRequireTEUNotCalled(() => container2.Delete());
			AssertRequireTEUNotCalled(() =>
			{
				container3 = consol.Containers.AddNew();
				container3.JC_ContainerCount = 1;
				container3.JC_RC = refContainer1.PK;
			});
			AssertRequireTEUNotCalled(() => consol.Containers.RemoveAndDelete(container3));

			void AssertRequireTEUNotCalled(Action action)
			{
				Assert(consol.SkipCO2eStatusCheck());
				action.Invoke();
				AssertEquals(0, requireTEUCalled);
			}
		}

		public void TestUpdateCO2eStatusToNotCurrent_WhenMostInterestingTransportChanges()
		{
			// Arrange
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.Transports[0].SetCO2eStatus(CO2eStatusList.Codes.Current);
			consol.SetCO2eStatus(CO2eStatusList.Codes.Current);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var consolInNewFac = newFactory.Load<ForwardingConsol>(consol.PK);
			AssertEquals("pre-condition", CO2eStatusList.Codes.Current, consolInNewFac.GetCO2eStatus());
			AssertEquals("pre-condition", CO2eStatusList.Codes.Current, consolInNewFac.MostInterestingTransportForBinding[0].GetCO2eStatus());

			// Act
			consolInNewFac.MostInterestingTransportForBinding[0].SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);

			// Assert
			AssertEquals(CO2eStatusList.Codes.NotCurrent, consolInNewFac.GetCO2eStatus());
			CO2eBusinessTestHelper.AssertSTUEvent(consolInNewFac, "Transport");
		}

		public void TestUpdateCO2eStatusToNotCurrent_WhenTemperatureControlChanges()
		{
			// Arrange
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RequiresTemperatureControl = true;
			consol.SetCO2eStatus(CO2eStatusList.Codes.Current);
			Factory.Save();

			// Act & Assert
			consol.JK_RequiresTemperatureControl = false;
			Factory.Save();
			AssertHasWarning(consol.TotalCO2eForBindingInfo, CO2eBusinessTestHelper.CO2eStaleWarning);
			CO2eBusinessTestHelper.AssertSTUEvent(consol, "JK_RequiresTemperatureControl [Y]->[N]");

			consol.SetCO2eStatus(CO2eStatusList.Codes.Current);
			consol.JK_RequiresTemperatureControl = true;
			Factory.Save();
			AssertHasWarning(consol.TotalCO2eForBindingInfo, CO2eBusinessTestHelper.CO2eStaleWarning);
			CO2eBusinessTestHelper.AssertSTUEvent(consol, "JK_RequiresTemperatureControl [N]->[Y]");

			consol.SetCO2eStatus(CO2eStatusList.Codes.Current);
			consol.JK_RequiresTemperatureControl = true;
			Factory.Save();
			AssertEquals(consol.GetCO2eStatus(), CO2eStatusList.Codes.Current);
			AssertNoWarning(consol.TotalCO2eForBindingInfo, CO2eBusinessTestHelper.CO2eStaleWarning);
		}

		RefContainer NewRefContainer(ZString code, ZString isoType, decimal teu, decimal tareWeight)
		{
			var refContainer = RefContainer.New(Factory);
			refContainer.RC_Code = code;
			refContainer.RC_ISOType = isoType;
			refContainer.RC_TEU = teu;
			refContainer.RC_TareWeight = tareWeight;

			return refContainer;
		}

		#endregion

		#region CO2e Copying

		public void TestShouldNotUpdateCO2eStatusToNotCurrentWhenCopyingConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			((IBusinessObjectInternals)consol).IsCopying = false;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.SetCO2ePerTonneInKg(100.1111111m);
			consol.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals(CO2eStatusList.Codes.NotCurrent, consol.GetCO2eStatus());

			var consol2 = Factory.New<ForwardingConsol>();
			((IBusinessObjectInternals)consol2).IsCopying = true;
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.SetCO2ePerTonneInKg(100.1111111m);
			consol2.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals(CO2eStatusList.Codes.Current, consol2.GetCO2eStatus());
		}

		#endregion

		#region Pre/Post Carriage

		public void TestCO2eCalculationSupporterRequiresPrePostCarriageLegs()
		{
			var consol = Factory.New<ForwardingConsol>();
			AssertEquals(false, ((ICO2eLegBasedSupporter)consol).RequiresPrePostCarriageLegs);
		}

		public void TestCO2eCalculationSupporterPreCarriageLegs()
		{
			var consol = Factory.New<ForwardingConsol>();
			AssertCarriageLegs(consol.GetPreCarriageLegs().ToArray());

			var depatureCFSAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			consol.JK_OA_PackDepotAddress = depatureCFSAddress.PK;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";
			AssertCarriageLegs(consol.GetPreCarriageLegs().ToArray(),
				(("", depatureCFSAddress.PK), ("AUSYD", ZGuid.Empty)));

			consol.Transports[0].JW_RL_NKLoadPort = "AUMEL";
			AssertCarriageLegs(consol.GetPreCarriageLegs().ToArray(),
				(("", depatureCFSAddress.PK), ("AUSYD", ZGuid.Empty)));
		}

		public void TestCO2eCalculationSupporterPreCarriageLegs_HBLDeliveryModes()
		{
			// Arrange
			var consol = Factory.New<ForwardingConsol>();
			var address = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			consol.JK_OA_PackDepotAddress = address.PK;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";

			// Assert DOOR_X
			AssertCarriageLegs(consol.GetPreCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR).ToArray(),
				(("", address.PK), ("AUSYD", ZGuid.Empty)));
			AssertCarriageLegs(consol.GetPreCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.DOOR_CY).ToArray(),
				(("", address.PK), ("AUSYD", ZGuid.Empty)));
			AssertCarriageLegs(consol.GetPreCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.DOOR_CFS).ToArray(),
				(("", address.PK), ("AUSYD", ZGuid.Empty)));
			AssertCarriageLegs(consol.GetPreCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.DOOR_ARPT).ToArray(),
				(("", address.PK), ("AUSYD", ZGuid.Empty)));
			AssertCarriageLegs(consol.GetPreCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.DOOR_PORT).ToArray(),
				(("", address.PK), ("AUSYD", ZGuid.Empty)));

			// Assert CFS_X
			AssertCarriageLegs(consol.GetPreCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.CFS_CY).ToArray(),
				(("", address.PK), ("AUSYD", ZGuid.Empty)));
			AssertCarriageLegs(consol.GetPreCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.CFS_CFS).ToArray(),
				(("", address.PK), ("AUSYD", ZGuid.Empty)));
			AssertCarriageLegs(consol.GetPreCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.CFS_ARPT).ToArray(),
				(("", address.PK), ("AUSYD", ZGuid.Empty)));
			AssertCarriageLegs(consol.GetPreCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR).ToArray(),
				(("", address.PK), ("AUSYD", ZGuid.Empty)));

			// Assert CY_X, ARPT_X, PORT_X
			AssertCarriageLegs(consol.GetPreCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.CY_CY).ToArray());
			AssertCarriageLegs(consol.GetPreCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.CY_CFS).ToArray());
			AssertCarriageLegs(consol.GetPreCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.CY_DOOR).ToArray());
			AssertCarriageLegs(consol.GetPreCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.ARPT_CFS).ToArray());
			AssertCarriageLegs(consol.GetPreCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.ARPT_ARPT).ToArray());
			AssertCarriageLegs(consol.GetPreCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.ARPT_DOOR).ToArray());
			AssertCarriageLegs(consol.GetPreCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.PORT_PORT).ToArray());
			AssertCarriageLegs(consol.GetPreCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.PORT_DOOR).ToArray());
		}

		public void TestCO2eCalculationSupporterPostCarriageLegs()
		{
			var consol = Factory.New<ForwardingConsol>();
			AssertCarriageLegs(consol.GetPostCarriageLegs().ToArray());

			var arrivalCFSAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			consol.JK_OA_UnpackDepotAddress = arrivalCFSAddress.PK;
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "AUSYD";
			AssertCarriageLegs(consol.GetPostCarriageLegs().ToArray(),
				(("AUSYD", ZGuid.Empty), ("", arrivalCFSAddress.PK)));

			consol.Transports[0].JW_RL_NKDiscPort = "AUMEL";
			AssertCarriageLegs(consol.GetPostCarriageLegs().ToArray(),
				(("AUSYD", ZGuid.Empty), ("", arrivalCFSAddress.PK)));
		}

		public void TestCO2eCalculationSupporterPostCarriageLegs_HBLDeliveryModes()
		{
			var consol = Factory.New<ForwardingConsol>();
			var address = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			consol.JK_OA_UnpackDepotAddress = address.PK;
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "AUSYD";

			// Assert X_DOOR
			AssertCarriageLegs(consol.GetPostCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.CY_DOOR).ToArray(),
				(("AUSYD", ZGuid.Empty), ("", address.PK)));
			AssertCarriageLegs(consol.GetPostCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR).ToArray(),
				(("AUSYD", ZGuid.Empty), ("", address.PK)));
			AssertCarriageLegs(consol.GetPostCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.ARPT_DOOR).ToArray(),
				(("AUSYD", ZGuid.Empty), ("", address.PK)));
			AssertCarriageLegs(consol.GetPostCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR).ToArray(),
				(("AUSYD", ZGuid.Empty), ("", address.PK)));
			AssertCarriageLegs(consol.GetPostCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.PORT_DOOR).ToArray(),
				(("AUSYD", ZGuid.Empty), ("", address.PK)));

			// Assert X_CFS
			AssertCarriageLegs(consol.GetPostCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.CY_CFS).ToArray(),
				(("AUSYD", ZGuid.Empty), ("", address.PK)));
			AssertCarriageLegs(consol.GetPostCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.CFS_CFS).ToArray(),
				(("AUSYD", ZGuid.Empty), ("", address.PK)));
			AssertCarriageLegs(consol.GetPostCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.ARPT_CFS).ToArray(),
				(("AUSYD", ZGuid.Empty), ("", address.PK)));
			AssertCarriageLegs(consol.GetPostCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.DOOR_CFS).ToArray(),
				(("AUSYD", ZGuid.Empty), ("", address.PK)));

			// Assert X_CY, X_ARPT, X_PORT
			AssertCarriageLegs(consol.GetPostCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.CY_CY).ToArray());
			AssertCarriageLegs(consol.GetPostCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.CFS_CY).ToArray());
			AssertCarriageLegs(consol.GetPostCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.DOOR_CY).ToArray());
			AssertCarriageLegs(consol.GetPostCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.CFS_ARPT).ToArray());
			AssertCarriageLegs(consol.GetPostCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.ARPT_ARPT).ToArray());
			AssertCarriageLegs(consol.GetPostCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.DOOR_ARPT).ToArray());
			AssertCarriageLegs(consol.GetPostCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.DOOR_PORT).ToArray());
			AssertCarriageLegs(consol.GetPostCarriageLegs(Core.Constants.HBLDeliveryModes.Codes.PORT_PORT).ToArray());
		}

		void AssertCarriageLegs(PrePostCarriageLegWrapper[] actual, params ((ZString, ZGuid) From, (ZString, ZGuid) To)[] expected)
		{
			AssertEquals(expected.Length, actual.Length);
			for (var idx = 0; idx < actual.Length; idx++)
			{
				CombineAssertions($"Carriage leg {idx}", () =>
				{
					AssertEquals(false, actual[idx].From.IsEmpty);
					AssertEquals(false, actual[idx].To.IsEmpty);
					AssertEquals(expected[idx].From.Item1, actual[idx].From.UNLOCO);
					AssertEquals(expected[idx].From.Item2, (actual[idx].From.Address?.EntityPK ?? ZGuid.Empty));
					AssertEquals(expected[idx].To.Item1, actual[idx].To.UNLOCO);
					AssertEquals(expected[idx].To.Item2, (actual[idx].To.Address?.EntityPK ?? ZGuid.Empty));
				});
			}
		}

		public void TestCO2eAddressValidationAddressesToValidate()
		{
			// Arrange
			var consol = Factory.New<ForwardingConsol>();
			var depatureCFSAddressPK = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var arrivalCFSAddressPK = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			consol.JK_OA_PackDepotAddress = depatureCFSAddressPK;
			consol.JK_OA_UnpackDepotAddress = arrivalCFSAddressPK;

			// Act & Assert
			var addressValidation = consol as IAddressesValidation;
			AssertEquals(addressValidation.AddressesToValidate.Length, 2);
			AssertContainsExactElementsInAnyOrder(new[] { depatureCFSAddressPK, arrivalCFSAddressPK },
				addressValidation.AddressesToValidate.Select(address => address.EntityPK));
		}

		public void TestOnTransportBookingCalculated()
		{
			using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// Arrange
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = "AIR";
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "CNSHA";
				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_RL_NKOrigin = "AUSYD";
				shipment1.JS_RL_NKDestination = "CNSHA";
				shipment1.JS_TransportMode = "AIR";
				shipment1.JS_ActualWeight = 100m;
				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_RL_NKOrigin = "NZAKL";
				shipment2.JS_RL_NKDestination = "CNSHA";
				shipment2.JS_TransportMode = "AIR";
				shipment2.JS_ActualWeight = 400m;
				var shipment2DepartureConsol = shipment2.Consols.AddNew();
				shipment2DepartureConsol.JK_TransportMode = "AIR";
				shipment2DepartureConsol.JK_RL_NKLoadPort = "NZAKL";
				shipment2DepartureConsol.JK_RL_NKDischargePort = "AUSYD";

				var consolTransportBookingPIC = CO2eTestHelper.CreateTransportBooking(consol, nameof(DtbBookingDirection.PIC), Factory);
				((ICO2eCalculationSupporter)consolTransportBookingPIC).SetTotalCO2e(100m);
				((ICO2eCalculationSupporter)consolTransportBookingPIC).SetCO2eStatus(CO2eStatusList.Codes.Current);
				AssertContainsExactElementsInAnyOrder("Pre-condition",
					new[] { consolTransportBookingPIC }, ((ICO2eCalculationSupporter)shipment1).AdditionalCalculationSupporters.Select(x => x.Supporter));
				AssertEquals("Pre-condition", 0, ((ICO2eCalculationSupporter)shipment2).AdditionalCalculationSupporters.Length);

				shipment1.SetTotalCO2e(1000m);
				shipment1.SetCO2eStatus(CO2eStatusList.Codes.Pending);
				shipment2.SetTotalCO2e(2000m);
				shipment2.SetCO2eStatus(CO2eStatusList.Codes.Pending);

				// Act
				((ICO2ePrePostCarriage)consol).OnTransportBookingCalculated(consolTransportBookingPIC);

				// Assert
				CombineAssertions("Shipment1 should be updated, Shipment2 should not be updated", () =>
				{
					AssertEquals(1000m + 100m * (100m / 500m), shipment1.GetTotalCO2e());
					AssertEquals(CO2eStatusList.Codes.Current, shipment1.GetCO2eStatus());
					AssertEquals(2000m, shipment2.GetTotalCO2e());
					AssertEquals(CO2eStatusList.Codes.Pending, shipment2.GetCO2eStatus());
				});
			}
		}

		public void TestTransportBookingCreated_SetShipmentsCO2eStatusToNotCurrent()
		{
			// Act
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			shipment1.SetCO2eStatus(CO2eStatusList.Codes.Current);

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.SetCO2eStatus(CO2eStatusList.Codes.Current);

			Factory.Save();

			// Arrange
			((IDtbBookingParent)consol).TransportBookingCreatedOrUpdated();

			// Assert
			AssertEquals(CO2eStatusList.Codes.NotCurrent, shipment1.GetCO2eStatus());
			AssertHasWarning(shipment2.TotalCO2eForSortingInfo, CO2eBusinessTestHelper.CO2eStaleWarning);
			AssertEquals(CO2eStatusList.Codes.NotCurrent, shipment2.GetCO2eStatus());
			AssertHasWarning(shipment2.TotalCO2eForSortingInfo, CO2eBusinessTestHelper.CO2eStaleWarning);

			var newFactory = new BusinessObjectFactory();
			var shipment1InNewFac = newFactory.Load<ForwardingShipment>(shipment1.PK);
			var shipment2InNewFac = newFactory.Load<ForwardingShipment>(shipment2.PK);
			AssertEquals(CO2eStatusList.Codes.NotCurrent, shipment1InNewFac.GetCO2eStatus());
			AssertEquals(CO2eStatusList.Codes.NotCurrent, shipment2InNewFac.GetCO2eStatus());
		}

		public void TestSaveEmissionsLogToNoteOnCalculated()
		{
			var consol = Factory.New<ForwardingConsol>() as ICO2eCalculationSupporter;
			AssertEquals(false, consol.SaveEmissionsLogToNoteOnCalculated);
		}

		#endregion

		#region ShowBECertifiedPickupDocument

		public void TestHasCertifiedPickupAcceptOrDecline()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "BOL_Reference";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1111111";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONT2222222";

			Assert(!consol.HasCertifiedPickupAcceptOrDecline);

			CertifiedPickupContainerEventHelperTest.AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.Assigned);
			Assert(consol.HasCertifiedPickupAcceptOrDecline);

			CertifiedPickupContainerEventHelperTest.AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.TransferSentAwaitingResponse);
			Assert(!consol.HasCertifiedPickupAcceptOrDecline);

			CertifiedPickupContainerEventHelperTest.AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.TransferSent);
			Assert(!consol.HasCertifiedPickupAcceptOrDecline);

			CertifiedPickupContainerEventHelperTest.AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.Accepted);
			Assert(!consol.HasCertifiedPickupAcceptOrDecline);

			CertifiedPickupContainerEventHelperTest.AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.Revoked);
			Assert(!consol.HasCertifiedPickupAcceptOrDecline);

			CertifiedPickupContainerEventHelperTest.AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.DeclinedByNextPartyForAcceptDecline);
			Assert(!consol.HasCertifiedPickupAcceptOrDecline);

			CertifiedPickupContainerEventHelperTest.AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.DeclinedByNextPartyForTransferRevoke);
			Assert(!consol.HasCertifiedPickupAcceptOrDecline);

			CertifiedPickupContainerEventHelperTest.AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.DeclinedByOtherReason);
			Assert(!consol.HasCertifiedPickupAcceptOrDecline);
		}

		public void TestHasCertifiedPickupTransfer()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "BOL_Reference";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1111111";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONT2222222";

			Assert(!consol.HasCertifiedPickupTransfer);

			CertifiedPickupContainerEventHelperTest.AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.Assigned);
			Assert(!consol.HasCertifiedPickupTransfer);

			CertifiedPickupContainerEventHelperTest.AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.TransferSentAwaitingResponse);
			Assert(consol.HasCertifiedPickupTransfer);

			CertifiedPickupContainerEventHelperTest.AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.TransferSent);
			Assert(!consol.HasCertifiedPickupTransfer);

			CertifiedPickupContainerEventHelperTest.AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.Accepted);
			Assert(consol.HasCertifiedPickupTransfer);

			CertifiedPickupContainerEventHelperTest.AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.Revoked);
			Assert(consol.HasCertifiedPickupTransfer);

			CertifiedPickupContainerEventHelperTest.AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.DeclinedByNextPartyForAcceptDecline);
			Assert(!consol.HasCertifiedPickupTransfer);

			CertifiedPickupContainerEventHelperTest.AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.DeclinedByNextPartyForTransferRevoke);
			Assert(consol.HasCertifiedPickupTransfer);

			CertifiedPickupContainerEventHelperTest.AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.DeclinedByOtherReason);
			Assert(consol.HasCertifiedPickupTransfer);
		}

		public void TestHasCertifiedPickupRevoke()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "BOL_Reference";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1111111";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONT2222222";

			Assert(!consol.HasCertifiedPickupRevoke);

			CertifiedPickupContainerEventHelperTest.AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.Assigned);
			Assert(!consol.HasCertifiedPickupRevoke);

			CertifiedPickupContainerEventHelperTest.AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.TransferSentAwaitingResponse);
			Assert(!consol.HasCertifiedPickupRevoke);

			CertifiedPickupContainerEventHelperTest.AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.TransferSent);
			Assert(consol.HasCertifiedPickupRevoke);

			CertifiedPickupContainerEventHelperTest.AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.Accepted);
			Assert(!consol.HasCertifiedPickupRevoke);

			CertifiedPickupContainerEventHelperTest.AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.Revoked);
			Assert(!consol.HasCertifiedPickupRevoke);

			CertifiedPickupContainerEventHelperTest.AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.DeclinedByNextPartyForAcceptDecline);
			Assert(!consol.HasCertifiedPickupRevoke);

			CertifiedPickupContainerEventHelperTest.AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.DeclinedByNextPartyForTransferRevoke);
			Assert(!consol.HasCertifiedPickupRevoke);

			CertifiedPickupContainerEventHelperTest.AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.DeclinedByOtherReason);
			Assert(!consol.HasCertifiedPickupRevoke);
		}

		#endregion

		#region Housebill Pending Allocation

		public void TestHouseBillPendingAllocationWhenAgentTypeIsSetToDirect()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "PENDING ALLOCATION..";
			shipment.IsPendingAllocationSetBySystem = true;

			AssertEquals("Consol not in database", false, consol.IsInDatabase);
			AssertEquals("Count shipments linked to consol", 1, consol.Shipments.Count);
			AssertEquals("Shipment not in database", false, shipment.IsInDatabase);
			AssertEquals("HouseBill", "PENDING ALLOCATION..", shipment.JS_HouseBill);
			AssertEquals("PendingAllocation", true, shipment.IsPendingAllocationSetBySystem);

			consol.JK_AgentType = Constants.AgentType.Direct;

			AssertEquals("Consol not in database", false, consol.IsInDatabase);
			AssertEquals("Count shipments linked to consol", 1, consol.Shipments.Count);
			AssertEquals("Shipment not in database", false, shipment.IsInDatabase);
			AssertEquals("HouseBill", ZString.Empty, shipment.JS_HouseBill);
			AssertEquals("PendingAllocation", false, shipment.IsPendingAllocationSetBySystem);

			Factory.Save();

			consol.JK_AgentType = Constants.AgentType.Agent;
			shipment.JS_HouseBill = "PENDING ALLOCATION..";
			shipment.IsPendingAllocationSetBySystem = true;

			AssertEquals("Consol in database", true, consol.IsInDatabase);
			AssertEquals("Count shipments linked to consol", 1, consol.Shipments.Count);
			AssertEquals("Shipment in database", true, shipment.IsInDatabase);
			AssertEquals("HouseBill", "PENDING ALLOCATION..", shipment.JS_HouseBill);
			AssertEquals("PendingAllocation", true, shipment.IsPendingAllocationSetBySystem);

			Factory.Save();

			consol.JK_AgentType = Constants.AgentType.Direct;

			AssertEquals("Consol in database", true, consol.IsInDatabase);
			AssertEquals("Count shipments linked to consol", 1, consol.Shipments.Count);
			AssertEquals("Shipment in database", true, shipment.IsInDatabase);
			AssertEquals("HouseBill", "PENDING ALLOCATION..", shipment.JS_HouseBill);
			AssertEquals("PendingAllocation", false, shipment.IsPendingAllocationSetBySystem);
		}

		#region TestRoutingLegColumns

		public void TestFirstSeaLegLoad_LastSeaLegDischarge_GeneralFirstAndLastLeg()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;

			var transport1 = (Transport)consol.Transports.First();
			transport1.JW_TransportMode = TransportModes.Air;
			transport1.JW_RL_NKLoadPort = "DEHAM";
			transport1.JW_RL_NKDiscPort = "USORD";
			transport1.JW_ATD = new ZDateTime(2009, 1, 2, 3, 4, 1);
			transport1.JW_ETD = new ZDateTime(2009, 1, 2, 3, 4, 2);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = TransportModes.Sea;
			transport2.JW_RL_NKLoadPort = "USORD";
			transport2.JW_RL_NKDiscPort = "USLAX";
			transport2.JW_ETD = new ZDateTime(2009, 1, 2, 3, 4, 6);

			var transport3 = consol.Transports.AddNew();
			transport3.JW_TransportMode = TransportModes.Sea;
			transport3.JW_RL_NKLoadPort = "USLAX";
			transport3.JW_RL_NKDiscPort = "DEHAM";
			transport3.JW_ETD = new ZDateTime(2009, 1, 2, 3, 4, 6);
			transport3.JW_ATD = new ZDateTime(2009, 1, 2, 3, 4, 6);
			transport3.JW_ETA = new ZDateTime(2009, 1, 2, 3, 4, 9);
			transport3.JW_ATA = new ZDateTime(2009, 1, 2, 3, 4, 8);

			var transport4 = consol.Transports.AddNew();
			transport4.JW_TransportMode = TransportModes.Sea;
			transport4.JW_RL_NKLoadPort = "DEHAM";
			transport4.JW_RL_NKDiscPort = "AUSYD";
			transport4.JW_ATA = new ZDateTime(2009, 1, 2, 3, 4, 12);

			var transport5 = consol.Transports.AddNew();
			transport5.JW_TransportMode = TransportModes.Air;
			transport5.JW_RL_NKLoadPort = "AUSYD";
			transport5.JW_RL_NKDiscPort = "AUMIL";
			transport5.JW_ATA = new ZDateTime(2009, 1, 2, 3, 4, 15);
			transport5.JW_ETA = new ZDateTime(2009, 1, 2, 3, 4, 16);

			AssertEquals("FirstLegLoadPortETDForBinding", consol.FirstLegLoadPortETDForBinding, new ZDateTime(2009, 1, 2, 3, 4, 2));
			AssertEquals("FirstSeaLegLoadPortForBinding", consol.FirstSeaLegLoadPortForBinding, "USORD");
			AssertEquals("FirstSeaLegLoadPortETDForBinding", consol.FirstSeaLegLoadPortETDForBinding, new ZDateTime(2009, 1, 2, 3, 4, 6));
			AssertEquals("FirstSeaLegLoadPortATDForBinding", consol.FirstSeaLegLoadPortATDForBinding, ZDateTime.Empty);
			AssertEquals("LastSeaLegDischargePortForBinding", consol.LastSeaLegDischargePortForBinding, "AUSYD");
			AssertEquals("LastSeaLegDischargePortETAForBinding", consol.LastSeaLegDischargePortETAForBinding, ZDateTime.Empty);
			AssertEquals("LastSeaLegDischargePortATAForBinding", consol.LastSeaLegDischargePortATAForBinding, new ZDateTime(2009, 1, 2, 3, 4, 12));
			AssertEquals("LastLegDischargePortETAForBinding", consol.LastLegDischargePortETAForBinding, new ZDateTime(2009, 1, 2, 3, 4, 16));

			transport4.JW_ETA = new ZDateTime(2009, 1, 2, 3, 4, 13);
			AssertEquals("LastSeaLegDischargePortETAForBinding", consol.LastSeaLegDischargePortETAForBinding, new ZDateTime(2009, 1, 2, 3, 4, 13));

			transport2.JW_ATD = new ZDateTime(2009, 1, 2, 3, 4, 5);
			AssertEquals("FirstSeaLegLoadPortATDForBinding", consol.FirstSeaLegLoadPortATDForBinding, new ZDateTime(2009, 1, 2, 3, 4, 5));
		}

		#endregion

		#endregion

		#region Earliest CTO Storage Start

		public void TestEarliestCTOStorageStartForBinding()
		{
			var consol = Factory.New<ForwardingConsol>();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();

			AssertEquals("EarliestCTOStorageStartForBinding Is Empty", ZDateTime.Empty, consol.EarliestCTOStorageStartForBinding);

			container1.JC_ArrivalCTOStorageStartDate = new ZDateTime(2009, 1, 2, 3, 4, 5);
			container2.JC_ArrivalCTOStorageStartDate = new ZDateTime(2009, 1, 2, 3, 4, 1);

			AssertEquals("EarliestCTOStorageStartForBinding is ArrivalCTOStorageStartDate Container2", new ZDateTime(2009, 1, 2, 3, 4, 1), consol.EarliestCTOStorageStartForBinding);
		}

		#endregion

		#region Earliest Empty Required By Date

		public void TestEarliestEmptyRequiredByDateForBinding()
		{
			var consol = Factory.New<ForwardingConsol>();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();

			AssertEquals("EarliestEmptyRequiredByDateForBinding Is Empty", ZDateTime.Empty, consol.EarliestEmptyRequiredByDateForBinding);

			container1.JC_EmptyReturnedBy = new ZDateTime(2009, 1, 2, 3, 4, 5);
			container2.JC_EmptyReturnedBy = new ZDateTime(2009, 1, 2, 3, 4, 1);

			AssertEquals("EarliestEmptyRequiredByDateForBinding is EmptyReturnedBy Container2", new ZDateTime(2009, 1, 2, 3, 4, 1), consol.EarliestEmptyRequiredByDateForBinding);
		}

		#endregion

		public void TestContainersAccessDeletedContainer()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			consol.SetCO2ePerTonneInKg(10);
			consol.SetCO2ePerTEUInKg(10);

			Factory.Save();
			Assert(((ICO2eCalculationSupporter)consol).RequireTEU);

			var newFactory = new BusinessObjectFactory();
			var moduleConsol = newFactory.Load<ForwardingModuleConsol>(consol.PK);
			AssertEquals(2, moduleConsol.Containers.Count);

			consol.Containers.RemoveAndDelete(container1);
			consol.Containers.RemoveAndDelete(container2);

			AssertNoExceptionThrown("Data Refresh Bus does not cause exception", () => Factory.Save());
		}

		#region Consol CY Change Refreshes Container CY Bindings

		public void TestDepartureContainerCYTransportModeClearedIfConsolCYChangesAndContainerCYFallenBackToConsolCY()
		{
			// Arrange
			var container = Factory.New<ForwardingContainer>();
			var consol = Factory.New<ForwardingConsol>();
			consol.Containers.Add(container);

			container.JC_OA_DepartureContainerYardAddress = Guid.Empty;
			consol.JK_OA_ContainerYardEmptyPickupAddress = Factory.New<OrgAddress>().PK;
			container.EmptyPickupByTransportMode = "RAI";

			AssertEquals("RAI", container.EmptyPickupByTransportMode);

			// Action
			consol.JK_OA_ContainerYardEmptyPickupAddress = Factory.New<OrgAddress>().PK;

			// Assert
			AssertEquals("Departure Container CY Cleared", string.Empty, container.EmptyPickupByTransportMode);

			// Arrange
			container.JC_OA_DepartureContainerYardAddress = Guid.Empty;
			consol.JK_OA_ContainerYardEmptyPickupAddress = Factory.New<OrgAddress>().PK;
			container.EmptyPickupByTransportMode = "RAI";

			AssertEquals("RAI", container.EmptyPickupByTransportMode);

			// Action
			consol.JK_OA_ContainerYardEmptyPickupAddress = Guid.Empty;

			// Assert
			AssertEquals("Departure Container CY Cleared", string.Empty, container.EmptyPickupByTransportMode);
		}

		public void TestDepartureContainerCYTransportModeNotClearedIfConsolCYChangesAndContainerCYNotFallenBackToConsolCY()
		{
			// Arrange
			var container = Factory.New<ForwardingContainer>();
			var consol = Factory.New<ForwardingConsol>();
			consol.Containers.Add(container);

			container.JC_OA_DepartureContainerYardAddress = Factory.New<OrgAddress>().PK;
			consol.JK_OA_ContainerYardEmptyPickupAddress = Factory.New<OrgAddress>().PK;
			container.EmptyPickupByTransportMode = "RAI";

			AssertEquals("RAI", container.EmptyPickupByTransportMode);

			// Action
			consol.JK_OA_ContainerYardEmptyPickupAddress = Factory.New<OrgAddress>().PK;

			// Assert
			AssertEquals("Departure Container CY not cleared", "RAI", container.EmptyPickupByTransportMode);
		}

		public void TestDepartureContainerCYBindingsResetIfConsolCYChangesAndContainerCYFallenBackToConsolCY()
		{
			// Arrange
			var container = Factory.New<ForwardingContainer>();
			var consol = Factory.New<ForwardingConsol>();
			consol.Containers.Add(container);
			AssertEquals(Guid.Empty, container.JC_OA_DepartureContainerYardAddress_ZAddress.OrgPKInfo.Value);

			// Act
			consol.JK_OA_ContainerYardEmptyPickupAddress = OverseasContainerYard.MainAddress.PK;

			// Assert
			AssertEquals("JK_OA_ContainerYardEmptyPickupAddress bindings refreshed", OverseasContainerYard.PK, container.JC_OA_DepartureContainerYardAddress_ZAddress.OrgPKInfo.Value);

			// Act
			consol.JK_OA_ContainerYardEmptyPickupAddress = LocalContainerYard.MainAddress.PK;

			// Assert
			AssertEquals("JK_OA_ContainerYardEmptyPickupAddress bindings refreshed", LocalContainerYard.PK, container.JC_OA_DepartureContainerYardAddress_ZAddress.OrgPKInfo.Value);
		}

		public void TestArrivalContainerCYTransportModeClearedIfConsolCYChangesAndContainerCYFallenBackToConsolCY()
		{
			// Arrange
			var container = Factory.New<ForwardingContainer>();
			var consol = Factory.New<ForwardingConsol>();
			consol.Containers.Add(container);

			container.JC_OA_ArrivalContainerYardAddress = Guid.Empty;
			consol.JK_OA_ContainerYardEmptyReturnAddress = Factory.New<OrgAddress>().PK;
			container.EmptyReturnToTransportMode = "RAI";

			AssertEquals("RAI", container.EmptyReturnToTransportMode);

			// Action
			consol.JK_OA_ContainerYardEmptyReturnAddress = Factory.New<OrgAddress>().PK;

			// Assert
			AssertEquals("Arrival Container CY cleared", string.Empty, container.EmptyReturnToTransportMode);

			// Arrange
			container.JC_OA_ArrivalContainerYardAddress = Guid.Empty;
			consol.JK_OA_ContainerYardEmptyReturnAddress = Factory.New<OrgAddress>().PK;
			container.EmptyReturnToTransportMode = "RAI";

			// Action
			consol.JK_OA_ContainerYardEmptyReturnAddress = Guid.Empty;

			// Assert
			AssertEquals("Arrival Container CY cleared", string.Empty, container.EmptyReturnToTransportMode);
		}

		public void TestArrivalContainerCYTransportModeNotClearedIfConsolCYChangesAndContainerCYNotFallenbackToConsolCY()
		{
			// Arrange
			var container = Factory.New<ForwardingContainer>();
			var consol = Factory.New<ForwardingConsol>();
			consol.Containers.Add(container);

			container.JC_OA_ArrivalContainerYardAddress = Factory.New<OrgAddress>().PK;
			consol.JK_OA_ContainerYardEmptyReturnAddress = Factory.New<OrgAddress>().PK;
			container.EmptyReturnToTransportMode = "RAI";

			AssertEquals("RAI", container.EmptyReturnToTransportMode);

			// Action
			consol.JK_OA_ContainerYardEmptyReturnAddress = Factory.New<OrgAddress>().PK;

			// Assert
			AssertEquals("Arrival Container CY not cleared", "RAI", container.EmptyReturnToTransportMode);
		}

		public void TestArrivalContainerCYBindingsResetIfConsolCYChangesAndContainerCYIsFallenBackToConsolCY()
		{
			// Arrange
			var container = Factory.New<ForwardingContainer>();
			var consol = Factory.New<ForwardingConsol>();
			consol.Containers.Add(container);
			AssertEquals(Guid.Empty, container.JC_OA_ArrivalContainerYardAddress_ZAddress.OrgPKInfo.Value);

			// Act
			consol.JK_OA_ContainerYardEmptyReturnAddress = OverseasContainerYard.MainAddress.PK;

			// Assert
			AssertEquals("JC_OA_ArrivalContainerYardAddress bindings refreshed", OverseasContainerYard.PK, container.JC_OA_ArrivalContainerYardAddress_ZAddress.OrgPKInfo.Value);

			// Act
			consol.JK_OA_ContainerYardEmptyReturnAddress = LocalContainerYard.MainAddress.PK;

			// Assert
			AssertEquals("JC_OA_ArrivalContainerYardAddress bindings refreshed", LocalContainerYard.PK, container.JC_OA_ArrivalContainerYardAddress_ZAddress.OrgPKInfo.Value);
		}

		#endregion

		#region GMN

		public void TestJK_GMN()
		{
			var consol = Factory.New<ForwardingConsol>();

			CombineAssertions("By default there should be no JK_GMN", () =>
			{
				AssertEquals(ZString.Empty, consol.JK_GMN);
				AssertNull(CusEntryNumber.Load<CusEntryNumber>(consol, CusEntryNumberTypes.Israel.GatepassMovementNumber, CountryCodes.Israel));
			});

			consol.JK_GMN = "123";
			AssertEquals("JK_GMN was added", "123", consol.JK_GMN);
			var gmnNumber = CusEntryNumber.Load<CusEntryNumber>(consol, CusEntryNumberTypes.Israel.GatepassMovementNumber, CountryCodes.Israel);
			AssertNotNull("JK_GMN was added", gmnNumber);

			CombineAssertions("JK_GMN CusEntryNumber properties", () =>
			{
				AssertEquals(consol.PK, gmnNumber.CE_ParentID);
				AssertEquals(consol.TableName, gmnNumber.CE_ParentTable);
				AssertEquals(CusEntryNumberTypes.Israel.GatepassMovementNumber, gmnNumber.CE_EntryType);
				AssertEquals(CountryCodes.Israel, gmnNumber.CE_RN_NKCountryCode);
				AssertEquals(true, gmnNumber.CE_EntryIsSystemGenerated);
			});

			Factory.Save();

			var consolReloaded = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			CombineAssertions("JK_GMN persisted", () =>
			{
				AssertEquals("123", consolReloaded.JK_GMN);
			});
		}

		#endregion GMN

		public void TestGetAWBAgentApprovalCountryCode()
		{
			const string ApprovalCountryCode = "IT";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var awbHeader = consol.AWBHeader;
			awbHeader.EH_ParentID = consol.PK;
			awbHeader.EH_Table = consol.TableName;
			awbHeader.EH_RN_NKAgentApprovalCountryCode = ApprovalCountryCode;

			AssertEquals("The AWB Agent Approval Country Code is incorrect", ApprovalCountryCode, consol.AWBAgentApprovalCountryCode);
		}

		public void TestGetAWBAgentApprovalCountryCodeWhenAWBHeaderIsNull()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			AssertNull("Consol AWBHeader should be null", consol.AWBHeader);

			AssertEquals("The AWB Agent Approval Country Code should be blank", ZString.Empty, consol.AWBAgentApprovalCountryCode);
		}

		public void TestGetAWBAgentApprovalNumber()
		{
			const string RegulatedAgentID = "0079-00";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var awbHeader = consol.AWBHeader;
			awbHeader.EH_ParentID = consol.PK;
			awbHeader.EH_Table = consol.TableName;
			awbHeader.EH_AgentApprovalNumber = RegulatedAgentID;

			AssertEquals("The AWB Agent Approval Number is incorrect", RegulatedAgentID, consol.AWBAgentApprovalNumber);
		}

		public void TestGetAWBAgentApprovalNumberWhenAWBHeaderIsNull()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			AssertNull("Consol AWBHeader should be null", consol.AWBHeader);

			AssertEquals("The AWB Agent Approval Number should be blank", ZString.Empty, consol.AWBAgentApprovalNumber);
		}

		public void TestRemoveSCOFromSecurityStatusOptions()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Switzerland))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "CHCHP";

				CodeDescriptionPairList removeSCOSecurityStatusList = consol.SecurityStatusList;
				Assert(!removeSCOSecurityStatusList.ContainsCode(AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly));
			}
		}

		public void TestSCOErrorAndWarningMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var consol = Factory.New<ForwardingConsol>();
				LinkedTransportWithSailing(consol.Transports[0], "AUBNE", "SGSIN", isCargoOnly: false);
				LinkedTransportWithSailing(consol.Transports.AddNew(), "SGSIN", "HKHKG", isCargoOnly: false);
				LinkedTransportWithSailing(consol.Transports.AddNew(), "HKHKG", "USLAX", isCargoOnly: false);
				Factory.Save();

				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "AU2CO";
				consol.SecurityStatusCode = "SCO";
				
				consol.AWBHeader.Populate();
				AssertHasNotifications(consol.AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandlingInfo);
				AssertHasError(consol.AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandlingInfo, "The Special Handling Code of 'SCO - Cargo Secure for All-Cargo Aircraft only' is invalid as not all flights on this Consol are cargo flights. Tick the 'Is Cargo Only' checkbox or override this Security Status.");
				
				consol.JK_RL_NKLoadPort = "CHCHP";
				consol.AWBHeader.Populate();
				AssertHasWarning(consol.AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandlingInfo, "The Special Handling Code of ‘SCO - Cargo secure for All-Cargo Aircraft only’ is invalid for Air Consolidations departing the European Union, Switzerland, Iceland, Liechtenstein or Norway.");
				AssertNoError(consol.AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandlingInfo, "The Special Handling Code of 'SCO - Cargo Secure for All-Cargo Aircraft only' is invalid as not all flights on this Consol are cargo flights. Tick the 'Is Cargo Only' checkbox or override this Security Status.");
			}
		}

		public void TestGetDefaultContactHasFetchHints()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var addr = CreateOrgAddress("Address 1", org);
			var contact = CreateContactForOrgAddress(addr, org);
			contact.OC_IsActive = true;
			contact.WorkingAddressPK = addr.PK;
			org.ContactsActive.Add(contact);
			var doc = contact.Documents.AddNew();
			doc.OD_DocumentGroup = ContactType.Consignor.Code;
			doc.OD_DefaultContact = true;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_OA_ReceivingForwarderAddress = addr.PK;
			consol.ReceivingForwarderWithContact.OrgPK = org.PK;
			AssertEquals(1, Factory.ActiveFetchHintsForTable(OrgDocumentSchema.Constants.TableName));
		}

		public void TestGetDefaultContactDbHits()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgPk = org.PK;

			var contact1 = CreateContactForOrgAddress(org.MainAddress, org);
			contact1.OC_IsActive = true;
			contact1.WorkingAddressPK = org.MainAddress.PK;
			org.ContactsActive.Add(contact1);
			var doc11 = Factory.NewWithValidTestData<OrgDocument>();
			doc11.OD_OC = contact1.PK;
			doc11.OD_DocumentGroup = ContactType.ImportAirFreightAgent.Code;
			contact1.Documents.Add(doc11);
			var doc12 = Factory.NewWithValidTestData<OrgDocument>();
			doc12.OD_OC = contact1.PK;
			doc12.OD_DocumentGroup = ContactType.ImportAirFreightAgent.Code;
			contact1.Documents.Add(doc12);

			var contact2 = CreateContactForOrgAddress(org.MainAddress, org);
			contact2.OC_IsActive = true;
			contact2.WorkingAddressPK = org.MainAddress.PK;
			org.ContactsActive.Add(contact2);
			var doc21 = Factory.NewWithValidTestData<OrgDocument>();
			doc21.OD_OC = contact2.PK;
			doc21.OD_DocumentGroup = ContactType.ImportAirFreightAgent.Code;
			contact2.Documents.Add(doc21);
			var doc22 = Factory.NewWithValidTestData<OrgDocument>();
			doc22.OD_OC = contact2.PK;
			doc22.OD_DocumentGroup = ContactType.ImportAirFreightAgent.Code;
			contact2.Documents.Add(doc22);

			var contact3 = CreateContactForOrgAddress(org.MainAddress, org);
			contact3.OC_IsActive = true;
			contact3.WorkingAddressPK = org.MainAddress.PK;
			org.ContactsActive.Add(contact3);
			var doc31 = Factory.NewWithValidTestData<OrgDocument>();
			doc31.OD_OC = contact3.PK;
			doc31.OD_DocumentGroup = ContactType.ImportAirFreightAgent.Code;
			contact3.Documents.Add(doc31);
			var doc32 = Factory.NewWithValidTestData<OrgDocument>();
			doc32.OD_OC = contact3.PK;
			doc32.OD_DocumentGroup = ContactType.ImportAirFreightAgent.Code;
			contact3.Documents.Add(doc32);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var sameConsol = newFactory.Load<ForwardingConsol>(consol.PK);
			var thisOrg = newFactory.Load<OrgHeader>(orgPk);
			var expectedDbHits = new Dictionary<string, int>
			{
				{ OrgDocumentSchema.Constants.TableName, 1 },
			};
			sameConsol.JK_OA_SendingForwarderAddress = thisOrg.MainAddress.PK;
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory, true))
			{
				var address = sameConsol.SendingForwarderWithContact;
			}
		}

		#region IExternalRequestGenerationProvider

		public void TestGetRequestJobID()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "JK125";

			AssertEquals("JK125", consol.GetRequestJobID());
		}

		public void TestGetRequestTypeCode()
		{
			var consol = Factory.New<ForwardingConsol>();
			AssertEquals(ExternalRequestTypes.Codes.Consol, consol.GetRequestTypeCode());
		}

		public void TestGetRequestSupportedAddressInfo()
		{
			var consol = Factory.New<ForwardingConsol>();

			AssertEquals(ZGuid.Empty, consol.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.ConsigneeDocumentaryAddress).OrginzationPK);
			AssertEquals(ZGuid.Empty, consol.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.ConsigneeDocumentaryAddress).ContactPK);

			AssertEquals(ZGuid.Empty, consol.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.ConsignorDocumentaryAddress).OrginzationPK);
			AssertEquals(ZGuid.Empty, consol.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.ConsignorDocumentaryAddress).ContactPK);
		}

		#endregion
	}
}
