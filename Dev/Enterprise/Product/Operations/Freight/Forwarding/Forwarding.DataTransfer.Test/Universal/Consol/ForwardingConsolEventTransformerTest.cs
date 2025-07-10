using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using EventConstants = CargoWise.EventReference.Constants;
using EventReferenceParameterTypes = Enterprise.Core.Constants.EventReferenceParameterTypes;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ForwardingConsolEventTransformerTest : TestCaseWithFactory
	{
		public void TestTransformFromToAndTotalEventParameters()
		{
			foreach (var eventTypePair in new (ZArchitecture.Business.Event, bool)[]
			{
				(Events.BookingConfirmed, false),
				(Events.Departure, false),
				(Events.Arrival, false),
				(Events.Arrival, true),
				(Events.FreightUnloaded, false)
			})
			{
				AssertTransformFromToAndTotalEventParameters(eventTypePair.Item1, eventTypePair.Item2);
			}

			#region AssertTransformFromToAndTotalEventParameters

			void AssertTransformFromToAndTotalEventParameters(ZArchitecture.Business.Event eventType, bool isEstimate)
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_ConsolMode = Core.Constants.ContainerModes.Loose;
				consol.JK_RL_NKLoadPort = "AUMEL";
				consol.JK_RL_NKDischargePort = "NZAKL";

				Factory.Save();

				var universalEvent = new UniversalEvent();
				universalEvent.DataContext = DataContextFactory.New();
				universalEvent.DataContext.AddDataTarget(DataContextType.ForwardingConsol, null);
				universalEvent.EventTime = new ZDateTimeOffset(2023, 4, 9, 14, 0, 0);
				universalEvent.ContextCollection = new List<Context>
				{
					new Context()
					{
						Type = "OriginIATAAirportCode",
						Value = "MEL"
					},
					new Context()
					{
						Type = "DestinationIATAAirportCode",
						Value = "AKL"
					},
					new Context()
					{
						Type = "MAWBNumberOfPieces",
						Value = "10"
					},
					new Context()
					{
						Type = "FlightDate",
						Value = "2023-04-09 15:30:00"
					}
				};

				var eventValue = new EventValue(eventType, isEstimate: isEstimate, reference: "freetext|FDT=2023-04-09|VFL=EK142");

				var transformedEventValue = ForwardingConsolEventTransformer.Transform(eventValue, universalEvent, consol);

				AssertNotNull(transformedEventValue.Parameters);
				Assert(transformedEventValue.Parameters.ContainsKey(EventConstants.EventReferenceParameters.Codes.From));
				AssertEquals("AUMEL", transformedEventValue.Parameters[EventConstants.EventReferenceParameters.Codes.From]);
				Assert(transformedEventValue.Parameters.ContainsKey(EventConstants.EventReferenceParameters.Codes.To));
				AssertEquals("NZAKL", transformedEventValue.Parameters[EventConstants.EventReferenceParameters.Codes.To]);
				Assert(transformedEventValue.Parameters.ContainsKey(EventConstants.EventReferenceParameters.Codes.Total));
				AssertEquals("10", transformedEventValue.Parameters[EventConstants.EventReferenceParameters.Codes.Total]);
				Assert(transformedEventValue.Parameters.ContainsKey(EventConstants.EventReferenceParameters.Codes.FlightDate));

				if (eventType.Code == Events.BookingConfirmedCode)
				{
					AssertEquals("From, To and Total were added", "freetext|FDT=09-Apr-23 15:30|FRM=AUMEL|TO=NZAKL|TTL=10|VFL=EK142", transformedEventValue.Reference);
					AssertEquals("Should use FlightDate context", "09-Apr-23 15:30", transformedEventValue.Parameters[EventConstants.EventReferenceParameters.Codes.FlightDate]);
				}
				else
				{
					AssertEquals("From, To and Total were added", "freetext|FDT=2023-04-09|FRM=AUMEL|TO=NZAKL|TTL=10|VFL=EK142", transformedEventValue.Reference);
					AssertEquals("Should not transform FlightDate parameter", "2023-04-09", transformedEventValue.Parameters[EventConstants.EventReferenceParameters.Codes.FlightDate]);
				}
			}

			#endregion
		}

		public void TestTransform_BKC_AddEstimateToEventParametersFromEstimatedTimeOfArrivalOfContext()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			Factory.Save();

			var universalEvent = new UniversalEvent();
			universalEvent.DataContext = DataContextFactory.New();
			universalEvent.DataContext.AddDataTarget(DataContextType.ForwardingConsol, null);
			universalEvent.EventTime = new ZDateTimeOffset(2023, 4, 9, 14, 0, 0);
			universalEvent.ContextCollection = new List<Context>
			{
				new Context()
				{
					Type = "EstimatedTimeOfArrival",
					Value = "2023-04-09 15:30:00"
				}
			};

			var eventValue = new EventValue(Events.BookingConfirmed, reference: "Dummy Description|FAC=CTO|LOC=NZAKL", isEstimate: false);

			var transformedEventValue = ForwardingConsolEventTransformer.Transform(eventValue, universalEvent, consol);
			Assert("EST parameter", transformedEventValue.Parameters.Contains(new KeyValuePair<string, string>("ETA", "2023-04-09 15:30:00")));
		}

		public void TestTransform_AddTypeEventParametersForPartialLogs_ContextCollectionHas_IsPartial()
		{
			AssertTypeEventParameters(Events.FreightLoaded, true);
			AssertTypeEventParameters(Events.FreightUnloaded, true);
			AssertTypeEventParameters(Events.Received, true);
			AssertTypeEventParameters(Events.BookingConfirmed, false);

			void AssertTypeEventParameters(ZArchitecture.Business.Event eventType, bool hasTypeParameter)
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_ConsolMode = Core.Constants.ContainerModes.Loose;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "NZAKL";

				Factory.Save();

				var universalEvent = new UniversalEvent();
				universalEvent.DataContext = DataContextFactory.New();
				universalEvent.DataContext.AddDataTarget(DataContextType.ForwardingConsol, null);
				universalEvent.EventTime = new ZDateTimeOffset(2023, 4, 9, 14, 0, 0);
				universalEvent.ContextCollection = new List<Context>
				{
					new Context() { Type = "IsPartial", Value = "Y" },
					new Context() { Type = "NumberOfPieces", Value = "4" },
					new Context() { Type = "MAWBNumberOfPieces", Value = "8" }
				};

				var eventValue = new EventValue(eventType, reference: "|LOC=AUSYD|PTL=4|TTL=8|VFL=Q123|FAC=CTO",
					isEstimate: false);

				var transformedEventValue =
					ForwardingConsolEventTransformer.Transform(eventValue, universalEvent, consol);

				AssertEquals(hasTypeParameter, transformedEventValue.Parameters.Contains(
					new KeyValuePair<string, string>(Params.Type, EventReferenceParameterTypes.Partial)));
			}
		}

		public void TestTransform_AddTypeEventParametersForPartialLogs_EventReferenceHasTtlPtlParameter()
		{
			AssertTypeEventParameters(Events.FreightLoaded, true);
			AssertTypeEventParameters(Events.FreightUnloaded, true);
			AssertTypeEventParameters(Events.Received, true);
			AssertTypeEventParameters(Events.BookingConfirmed, false);

			void AssertTypeEventParameters(ZArchitecture.Business.Event eventType, bool hasTypeParameter)
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_ConsolMode = Core.Constants.ContainerModes.Loose;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "NZAKL";

				Factory.Save();

				var universalEvent = new UniversalEvent();
				universalEvent.DataContext = DataContextFactory.New();
				universalEvent.DataContext.AddDataTarget(DataContextType.ForwardingConsol, null);
				universalEvent.EventTime = new ZDateTimeOffset(2023, 4, 9, 14, 0, 0);
				universalEvent.ContextCollection = new List<Context>
				{
					new Context() { Type = "FlightNumber", Value = "Q123" },
				};

				var eventValue = new EventValue(eventType, reference: "|LOC=AUSYD|PTL=4|TTL=8|VFL=Q123|FAC=CTO",
					isEstimate: false);

				var transformedEventValue =
					ForwardingConsolEventTransformer.Transform(eventValue, universalEvent, consol);

				AssertEquals(hasTypeParameter, transformedEventValue.Parameters.Contains(
					new KeyValuePair<string, string>(Params.Type, EventReferenceParameterTypes.Partial)));
			}
		}

		public void TestTransform_NoTypeEventParametersAddedForNotPartialLogs()
		{
			AssertTypeEventParameters(Events.FreightLoaded, false);
			AssertTypeEventParameters(Events.FreightUnloaded, false);
			AssertTypeEventParameters(Events.Received, false);

			void AssertTypeEventParameters(ZArchitecture.Business.Event eventType, bool hasTypeParameter)
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_ConsolMode = Core.Constants.ContainerModes.Loose;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "NZAKL";

				Factory.Save();

				var universalEvent = new UniversalEvent();
				universalEvent.DataContext = DataContextFactory.New();
				universalEvent.DataContext.AddDataTarget(DataContextType.ForwardingConsol, null);
				universalEvent.EventTime = new ZDateTimeOffset(2023, 4, 9, 14, 0, 0);
				universalEvent.ContextCollection = new List<Context>
				{
					new Context()
					{
						Type = "EstimatedTimeOfArrival",
						Value = "2023-04-09 15:30:00"
					}
				};

				var eventValue = new EventValue(eventType, reference: "|LOC=AUSYD|VFL=Q123|FAC=CTO",
					isEstimate: false);

				var transformedEventValue =
					ForwardingConsolEventTransformer.Transform(eventValue, universalEvent, consol);

				AssertEquals(hasTypeParameter, transformedEventValue.Parameters.Contains(
					new KeyValuePair<string, string>(Params.Type, EventReferenceParameterTypes.Partial)));
			}
		}
	}
}
