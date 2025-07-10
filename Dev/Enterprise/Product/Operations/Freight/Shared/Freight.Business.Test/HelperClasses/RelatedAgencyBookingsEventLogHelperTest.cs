using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Freight;
using Enterprise.ZArchitecture.Business;
using Moq;
using static Enterprise.Freight.Integration.Agency;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Business.Testing
{
	class RelatedAgencyBookingsEventLogHelperTest : TestCaseWithFactory
	{
		public void TestUpdateRelatedBookedAgencyBookingEventForJobSailings()
		{
			var voyage = Factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USCHI";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "CNSHG";
			voyage.GenerateSailings();

			var agencyBooking1 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking1.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			agencyBooking1.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var agencyBooking1MainTransport = agencyBooking1.Transports.AddNew();
			agencyBooking1MainTransport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			agencyBooking1MainTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			agencyBooking1MainTransport.JW_IsLinked = true;
			agencyBooking1MainTransport.JW_JX = voyage.Sailings[1].PK;

			var agencyBooking1OtherTransport = agencyBooking1.Transports.AddNew();
			agencyBooking1OtherTransport.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			agencyBooking1OtherTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			agencyBooking1OtherTransport.JW_IsLinked = true;
			agencyBooking1OtherTransport.JW_JX = voyage.Sailings[0].PK;

			var agencyBooking2 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking2.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			agencyBooking2.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var agencyBooking2MainTransport = agencyBooking2.Transports.AddNew();
			agencyBooking2MainTransport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			agencyBooking2MainTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			agencyBooking2MainTransport.JW_IsLinked = false;

			var agencyBooking2OtherTransport = agencyBooking2.Transports.AddNew();
			agencyBooking2OtherTransport.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			agencyBooking2OtherTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			agencyBooking2OtherTransport.JW_IsLinked = true;
			agencyBooking2OtherTransport.JW_JX = voyage.Sailings[0].PK;

			var agencyBooking3 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking3.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			agencyBooking3.JS_JX = voyage.Sailings[0].PK;
			agencyBooking3.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var billOfLading1 = (CommonShipment)Factory.New<IBillOfLading>();
			billOfLading1.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			billOfLading1.JS_JX = voyage.Sailings[1].PK;

			var agencyBooking4 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking4.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;

			var agencyBooking4MainTransport = agencyBooking4.Transports.AddNew();
			agencyBooking4MainTransport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			agencyBooking4MainTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			agencyBooking4MainTransport.JW_IsLinked = true;
			agencyBooking4MainTransport.JW_JX = voyage.Sailings[1].PK;

			var agencyBooking4OtherTransport = agencyBooking4.Transports.AddNew();
			agencyBooking4OtherTransport.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			agencyBooking4OtherTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			agencyBooking4OtherTransport.JW_IsLinked = true;
			agencyBooking4OtherTransport.JW_JX = voyage.Sailings[0].PK;

			var agencyBooking5 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
			agencyBooking5.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var agencyBooking5MainTransport = agencyBooking5.Transports.AddNew();
			agencyBooking5MainTransport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			agencyBooking5MainTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			agencyBooking5MainTransport.JW_IsLinked = true;
			agencyBooking5MainTransport.JW_JX = voyage.Sailings[1].PK;

			var agencyBooking5OtherTransport = agencyBooking5.Transports.AddNew();
			agencyBooking5OtherTransport.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			agencyBooking5OtherTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			agencyBooking5OtherTransport.JW_IsLinked = true;
			agencyBooking5OtherTransport.JW_JX = voyage.Sailings[0].PK;

			Factory.Save();

			agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			agencyBooking5.Logs.AddNew(Events.StatusUpdated,
				new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.Booked),
				new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));

			AssertEquals(voyage.Sailings[1].PK, agencyBooking1.JS_JX);
			AssertEquals(ZGuid.Empty, agencyBooking2.JS_JX);

			AssertEquals(voyage.Sailings[1].PK, agencyBooking1MainTransport.JW_JX);
			AssertEquals(voyage.Sailings[0].PK, agencyBooking1OtherTransport.JW_JX);
			AssertEquals(ZGuid.Empty, agencyBooking2MainTransport.JW_JX);
			AssertEquals(voyage.Sailings[0].PK, agencyBooking2OtherTransport.JW_JX);
			AssertEquals(voyage.Sailings[1].PK, agencyBooking4MainTransport.JW_JX);
			AssertEquals(voyage.Sailings[0].PK, agencyBooking4OtherTransport.JW_JX);
			AssertEquals(voyage.Sailings[1].PK, agencyBooking5MainTransport.JW_JX);
			AssertEquals(voyage.Sailings[0].PK, agencyBooking5OtherTransport.JW_JX);

			Assert(!agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

			var registry = new Mock<IAgencyRegistry>(MockBehavior.Strict);
			using (ObjectFactory.Substitute(registry.Object))
			{
				registry.Setup(m => m.ElectronicBookingAndShippingInstructions).Returns(false);

				RelatedAgencyBookingsEventLogHelper.UpdateRelatedBookedAgencyBookingEvent(origin.FetchSailings());

				Assert(!agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			}

			using (ObjectFactory.Substitute(registry.Object))
			{
				registry.Setup(m => m.ElectronicBookingAndShippingInstructions).Returns(true);

				RelatedAgencyBookingsEventLogHelper.UpdateRelatedBookedAgencyBookingEvent(origin.FetchSailings());

				AssertEquals(1, agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				AssertEquals(1, agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			}
		}

		public void TestUpdateRelatedBookedAgencyBookingEventForJobSailingsOnlyForMainSeaTransport()
		{
			var voyage = Factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USCHI";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "CNSHG";
			voyage.GenerateSailings();

			var agencyBooking1 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking1.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			agencyBooking1.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var mainTransport = agencyBooking1.Transports.AddNew();
			mainTransport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			mainTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			mainTransport.JW_IsLinked = false;

			var otherTransport = agencyBooking1.Transports.AddNew();
			otherTransport.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			otherTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			otherTransport.JW_IsLinked = true;
			otherTransport.JW_JX = voyage.Sailings[0].PK;

			var agencyBooking2 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking2.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			agencyBooking2.JS_JX = voyage.Sailings[1].PK;
			agencyBooking2.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var mainSeaLeg = agencyBooking2.Transports.Cast<Transport>()
					.FirstOrDefault(transport =>
						transport.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel &&
						transport.JW_TransportMode == Core.Constants.TransportModes.Sea);

			var agencyBooking3 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking3.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			agencyBooking3.JS_JX = voyage.Sailings[1].PK;

			var booking3MainSeaLeg = agencyBooking3.Transports.Cast<Transport>()
					.FirstOrDefault(transport =>
						transport.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel &&
						transport.JW_TransportMode == Core.Constants.TransportModes.Sea);

			var agencyBooking4 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking4.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
			agencyBooking4.JS_JX = voyage.Sailings[1].PK;
			agencyBooking4.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var booking4MainSeaLeg = agencyBooking2.Transports.Cast<Transport>()
					.FirstOrDefault(transport =>
						transport.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel &&
						transport.JW_TransportMode == Core.Constants.TransportModes.Sea);

			Factory.Save();

			AssertEquals(ZGuid.Empty, agencyBooking1.JS_JX);
			AssertEquals(voyage.Sailings[0].PK, otherTransport.JW_JX);
			AssertEquals(voyage.Sailings[1].PK, agencyBooking2.JS_JX);
			AssertEquals(voyage.Sailings[1].PK, agencyBooking3.JS_JX);
			AssertEquals(voyage.Sailings[1].PK, agencyBooking4.JS_JX);

			Assert(!agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

			var registry = new Mock<IAgencyRegistry>(MockBehavior.Strict);
			mainTransport.JW_ATD = ZDateTime.Today;
			mainSeaLeg.JW_ATD = ZDateTime.Today;
			booking3MainSeaLeg.JW_ATD = ZDateTime.Today;
			booking4MainSeaLeg.JW_ATD = ZDateTime.Today;
			Factory.Save();

			agencyBooking4.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			agencyBooking4.Logs.AddNew(Events.StatusUpdated,
				new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.Booked),
				new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));

			using (ObjectFactory.Substitute(registry.Object))
			{
				registry.Setup(m => m.ElectronicBookingAndShippingInstructions).Returns(true);
				registry.Setup(m => m.IsDeliveryAgentAllowedSendingBookingConfirmationEDIAfterATD(ZGuid.Empty)).Returns(false);

				RelatedAgencyBookingsEventLogHelper.UpdateRelatedBookedAgencyBookingEvent(origin.FetchSailings(), true);

				Assert(!agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			}

			mainTransport.JW_ATD = ZDateTime.Empty;
			mainSeaLeg.JW_ATD = ZDateTime.Empty;
			booking3MainSeaLeg.JW_ATD = ZDateTime.Empty;
			booking4MainSeaLeg.JW_ATD = ZDateTime.Empty;
			Factory.Save();

			agencyBooking4.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			agencyBooking4.Logs.AddNew(Events.StatusUpdated,
				new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.Booked),
				new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));

			using (ObjectFactory.Substitute(registry.Object))
			{
				registry.Setup(m => m.ElectronicBookingAndShippingInstructions).Returns(true);
				registry.Setup(m => m.IsDeliveryAgentAllowedSendingBookingConfirmationEDIAfterATD(ZGuid.Empty)).Returns(false);

				var agencyBooking1LogNumber = agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber);
				var agencyBooking2LogNumber = agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber);

				RelatedAgencyBookingsEventLogHelper.UpdateRelatedBookedAgencyBookingEvent(origin.FetchSailings(), true);

				AssertEquals(agencyBooking1LogNumber, agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				AssertEquals(agencyBooking2LogNumber + 1, agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			}

			mainTransport.JW_ATD = ZDateTime.Today;
			mainSeaLeg.JW_ATD = ZDateTime.Today;
			booking3MainSeaLeg.JW_ATD = ZDateTime.Today;
			booking4MainSeaLeg.JW_ATD = ZDateTime.Today;
			Factory.Save();

			agencyBooking4.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			agencyBooking4.Logs.AddNew(Events.StatusUpdated,
				new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.Booked),
				new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));

			using (ObjectFactory.Substitute(registry.Object))
			{
				registry.Setup(m => m.ElectronicBookingAndShippingInstructions).Returns(true);
				registry.Setup(m => m.IsDeliveryAgentAllowedSendingBookingConfirmationEDIAfterATD(ZGuid.Empty)).Returns(true);

				var agencyBooking1LogNumber = agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber);
				var agencyBooking2LogNumber = agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber);

				RelatedAgencyBookingsEventLogHelper.UpdateRelatedBookedAgencyBookingEvent(origin.FetchSailings(), true);

				AssertEquals(agencyBooking1LogNumber, agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				AssertEquals(agencyBooking2LogNumber + 1, agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			}

			mainTransport.JW_ATD = ZDateTime.Empty;
			mainSeaLeg.JW_ATD = ZDateTime.Empty;
			booking3MainSeaLeg.JW_ATD = ZDateTime.Empty;
			booking4MainSeaLeg.JW_ATD = ZDateTime.Empty;
			Factory.Save();

			agencyBooking4.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			agencyBooking4.Logs.AddNew(Events.StatusUpdated,
				new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.Booked),
				new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));

			using (ObjectFactory.Substitute(registry.Object))
			{
				registry.Setup(m => m.ElectronicBookingAndShippingInstructions).Returns(true);
				registry.Setup(m => m.IsDeliveryAgentAllowedSendingBookingConfirmationEDIAfterATD(ZGuid.Empty)).Returns(true);

				var agencyBooking1LogNumber = agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber);
				var agencyBooking2LogNumber = agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber);

				RelatedAgencyBookingsEventLogHelper.UpdateRelatedBookedAgencyBookingEvent(origin.FetchSailings(), true);

				AssertEquals(agencyBooking1LogNumber, agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				AssertEquals(agencyBooking2LogNumber + 1, agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			}
		}

		public void TestUpdateRelatedBookedAgencyBookingEventForJobVoyage()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USCHI";
			voyage.GenerateSailings();

			var agencyBooking1 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking1.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			agencyBooking1.JS_JX = voyage.Sailings[0].PK;
			agencyBooking1.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var agencyBooking2 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking2.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			agencyBooking2.JS_JX = voyage.Sailings[0].PK;
			agencyBooking2.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var agencyBooking3 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking3.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			agencyBooking3.JS_JX = voyage.Sailings[0].PK;
			agencyBooking3.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var billOfLading1 = (CommonShipment)Factory.New<IBillOfLading>();
			billOfLading1.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			billOfLading1.JS_JX = voyage.Sailings[0].PK;
			billOfLading1.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var agencyBooking4 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking4.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			agencyBooking4.JS_JX = voyage.Sailings[0].PK;

			var agencyBooking5 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
			agencyBooking5.JS_JX = voyage.Sailings[0].PK;
			agencyBooking5.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			Factory.Save();

			agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			agencyBooking5.Logs.AddNew(Events.StatusUpdated,
				new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.Booked),
				new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));

			Assert(!agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

			var registry = new Mock<IAgencyRegistry>(MockBehavior.Strict);
			using (ObjectFactory.Substitute(registry.Object))
			{
				registry.Setup(m => m.ElectronicBookingAndShippingInstructions).Returns(false);

				RelatedAgencyBookingsEventLogHelper.UpdateRelatedBookedAgencyBookingEvent(voyage);

				Assert(!agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			}

			using (ObjectFactory.Substitute(registry.Object))
			{
				registry.Setup(m => m.ElectronicBookingAndShippingInstructions).Returns(true);

				RelatedAgencyBookingsEventLogHelper.UpdateRelatedBookedAgencyBookingEvent(voyage);

				AssertEquals(1, agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				AssertEquals(1, agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			}
		}
	}
}
