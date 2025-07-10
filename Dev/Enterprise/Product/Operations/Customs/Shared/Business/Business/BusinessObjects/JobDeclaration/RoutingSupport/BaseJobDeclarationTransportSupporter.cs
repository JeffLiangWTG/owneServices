using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using EventConstants = CargoWise.EventReference.Constants;

namespace Enterprise.Customs.Business
{
	public class BaseJobDeclarationTransportSupporter<T> : TransportSupporter<T>, IJobDeclarationTransportSupporter
		where T : BaseJobDeclaration
	{
		public BaseJobDeclarationTransportSupporter(T parent)
			: base(parent) { }

		public override ZString BillOfLading
		{
			get { return Parent.JE_MasterBill; }
		}

		public override bool BillOfLadingHasChanges
		{
			get { return Parent.JE_MasterBillInfo.HasChanges; }
		}

		public override ZString ConsignmentRef
		{
			get { return Parent.JE_DeclarationReference; }
		}

		public override ZString ContainerMode
		{
			get { return Parent.JE_ContainerMode; }
		}

		public override ZString Description
		{
			get { return Parent.JE_DeclarationReference; }
		}

		public override ZGuid ShippingLine
		{
			get { return Parent.JE_OH_ShippingLine; }
			set
			{
				if (!Parent.DeclarationMessagesHaveBeenSent() && value.IsValid)
				{
					Parent.JE_OH_ShippingLine = value;
				}
			}
		}

		public override ZString TransportMode
		{
			get { return Parent.JE_TransportMode; }
		}

		public override bool DontErrorOnMissingDetails
		{
			get { return true; }
		}

		public override void SetConsignmentRefIfNotSet()
		{
			base.SetConsignmentRefIfNotSet();
			Parent.PopulateJE_DeclarationReferenceIfNeeded();
		}

		protected override void NotifySailingChangedCore(Transport transport, ZGuid previousValue)
		{
			base.NotifySailingChangedCore(transport, previousValue);
			UpdateAllDeclarationTransportDataCore(transport);
		}

		protected override void ETASetFromSailingCore(Transport transport, ZDateTime oldValue, ZDateTime newValue)
		{
			base.ETASetFromSailingCore(transport, oldValue, newValue);

			Parent.UpdateRoutingDefaultIfAllowedAndSingleLeg(delegate
			{
				if (Parent.JE_DateOfArrival == oldValue)
				{
					Parent.JE_DateOfArrival = newValue;
				}
			}, transport);
		}

		protected override void ETDSetFromSailingCore(Transport transport, ZDateTime oldValue, ZDateTime newValue)
		{
			base.ETDSetFromSailingCore(transport, oldValue, newValue);
			if (!Parent.DeclarationMessagesHaveBeenSent())
			{
				Parent.UpdateRoutingDefaultIfAllowedAndSingleLeg(delegate
				{
					if (Parent.JE_ExportDate == oldValue)
					{
						Parent.JE_ExportDate = newValue;
					}
				}, transport);
			}
		}

		protected override void NotifyATAChangedCore(Transport transport, ZDateTime previousValue)
		{
			base.NotifyATAChangedCore(transport, previousValue);

			Parent.UpdateRoutingDefaultIfAllowedEvenIfMultiLeg(delegate
			{
				if (Parent.JE_RL_NKPortOfArrival == transport.JW_RL_NKDiscPort)
				{
					Parent.JE_DateOfArrival = transport.JW_ATA.IsEmpty ? transport.JW_ETA : transport.JW_ATA;
				}
				if (Parent.JE_RL_NKFinalDestination == transport.JW_RL_NKDiscPort)
				{
					Parent.Logs.CreateRecreateOrUpdateEventLog(Events.Arrival, EstimateActual.Actual,
						transport.JW_ATA.ToOffset(), "", GetParametersForEvent(transport, Events.Arrival));
				}
			}, transport);
		}

		protected override void NotifyATDChangedCore(Transport transport, ZDateTime previousValue)
		{
			base.NotifyATDChangedCore(transport, previousValue);

			Parent.UpdateRoutingDefaultIfAllowedEvenIfMultiLeg(delegate
			{
				if (Parent.JE_RL_NKPortOfLoading == transport.JW_RL_NKLoadPort)
				{
					Parent.JE_ExportDate = transport.JW_ATD.IsEmpty ? transport.JW_ETD : transport.JW_ATD;
				}
				if (Parent.JE_RL_NKOrigin == transport.JW_RL_NKLoadPort)
				{
					Parent.Logs.CreateRecreateOrUpdateEventLog(Events.Departure, EstimateActual.Actual,
						transport.JW_ATD.ToOffset(), "", GetParametersForEvent(transport, Events.Departure));
				}
			}, transport);
		}

		KeyValuePair<string, string>[] GetParametersForEvent(Transport transport, Event eventType)
		{
			var parameters = new Dictionary<string, string>();

			if (eventType == Events.Arrival)
			{
				parameters = GetTransportEventParameters(transport, transport.JW_RL_NKDiscPort, transport.JW_ETA);
			}
			else if (eventType == Events.Departure)
			{
				parameters = GetTransportEventParameters(transport, transport.JW_RL_NKLoadPort, transport.JW_ETD);
			}

			return parameters.ToArray();
		}

		Dictionary<string, string> GetTransportEventParameters(Transport transport, ZString location, ZDateTime flightDate)
		{
			var parameters = new Dictionary<string, string>();
			parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;
			parameters[EventConstants.EventReferenceParameters.Codes.Location] = location;

			if (transport.JW_TransportMode == Constants.TransportModes.Air)
			{
				parameters[EventConstants.EventReferenceParameters.Codes.VoyageFlightNumber] = transport.JW_VoyageFlight;
				parameters[EventConstants.EventReferenceParameters.Codes.FlightDate] = flightDate.ToISO8601ShortDateString();
			}

			return parameters;
		}

		protected override void NotifyTerminalAvailabilityDateChangedCore(Transport transport, ZDateTime previousValue)
		{
			base.NotifyTerminalAvailabilityDateChangedCore(transport, previousValue);

			foreach (BaseCusContainer container in Parent.CusContainers)
			{
				container.JobContainer?.JC_FCLAvailableInfo.RefreshBinding();
			}
		}

		protected override void NotifyTerminalStorageDateChangedCore(Transport transport, ZDateTime previousValue)
		{
			base.NotifyTerminalStorageDateChangedCore(transport, previousValue);

			foreach (BaseCusContainer container in Parent.CusContainers)
			{
				container.JobContainer?.JC_ArrivalCTOStorageStartDateInfo.RefreshBinding();
			}
		}

		protected override void NotifyDepotAvailabilityDateChangedCore(Transport transport, ZDateTime previousValue)
		{
			base.NotifyDepotAvailabilityDateChangedCore(transport, previousValue);

			foreach (BaseCusContainer container in Parent.CusContainers)
			{
				container.JobContainer?.JC_LCLAvailableInfo.RefreshBinding();
			}
		}

		protected override void NotifyDepotStorageDateChangedCore(Transport transport, ZDateTime previousValue)
		{
			base.NotifyDepotStorageDateChangedCore(transport, previousValue);

			foreach (BaseCusContainer container in Parent.CusContainers)
			{
				container.JobContainer?.JC_LCLStorageCommencesInfo.RefreshBinding();
			}
		}

		protected override void NotifyCarrierAddressChangedCore(Transport transport, ZGuid previousValue)
		{
			base.NotifyCarrierAddressChangedCore(transport, previousValue);
			Parent.UpdateRoutingDefaultIfAllowedAndSingleLeg(() => Parent.JE_OH_ShippingLine = transport.CarrierPK, transport);
		}

		protected override void NotifyDischargeChangedCore(Transport transport, ZString previousValue)
		{
			base.NotifyDischargeChangedCore(transport, previousValue);

			Parent.UpdateRoutingDefaultIfAllowedAndSingleLeg(delegate
			{
				Parent.JE_RL_NKPortOfArrival = transport.JW_RL_NKDiscPort;
			}, transport);
		}

		protected override void NotifyETAChangedCore(Transport transport, ZDateTime previousValue)
		{
			base.NotifyETAChangedCore(transport, previousValue);

			Parent.UpdateRoutingDefaultIfAllowedEvenIfMultiLeg(delegate
			{
				if (transport.JW_ATA.IsEmpty && Parent.JE_RL_NKPortOfArrival == transport.JW_RL_NKDiscPort)
				{
					Parent.JE_DateOfArrival = transport.JW_ETA;
				}
			}, transport);
		}

		protected override void NotifyETDChangedCore(Transport transport, ZDateTime previousValue)
		{
			base.NotifyETDChangedCore(transport, previousValue);

			Parent.UpdateRoutingDefaultIfAllowedEvenIfMultiLeg(delegate
			{
				if (transport.JW_ATD.IsEmpty && Parent.JE_RL_NKPortOfLoading == transport.JW_RL_NKLoadPort)
				{
					Parent.JE_ExportDate = transport.JW_ETD;
				}
			}, transport);
		}

		protected override void NotifyLoadChangedCore(Transport transport, ZString previousValue)
		{
			base.NotifyLoadChangedCore(transport, previousValue);

			Parent.UpdateRoutingDefaultIfAllowedAndSingleLeg(delegate
			{
				Parent.JE_RL_NKPortOfLoading = transport.JW_RL_NKLoadPort;
			}, transport);
		}

		protected override void NotifyTransportTypeChangedCore(Transport transport, ZString previousValue)
		{
			base.NotifyTransportTypeChangedCore(transport, previousValue);

			Parent.UpdateRoutingDefaultIfAllowedAndSingleLeg(delegate
			{
				Parent.JE_TransportMode = transport.JW_TransportMode;
			}, transport);
		}

		protected override void NotifyVesselChangedCore(Transport transport, ZString previousValue)
		{
			base.NotifyVesselChangedCore(transport, previousValue);

			Parent.UpdateRoutingDefaultIfAllowedAndSingleLeg(delegate
			{
				Parent.JE_VesselName = Parent.IsSea ? transport.JW_Vessel : ZString.Empty;
			}, transport);
		}

		protected override void NotifyVoyageFlightChangedCore(Transport transport, ZString previousValue)
		{
			base.NotifyVoyageFlightChangedCore(transport, previousValue);

			Parent.UpdateRoutingDefaultIfAllowedAndSingleLeg(delegate
			{
				Parent.JE_VoyageFlightNo = transport.JW_VoyageFlight;
			}, transport);
		}

		protected override void NotifyVoyageUpdatedCore(Transport transport)
		{
			UpdateAllDeclarationTransportDataIfEmpty(transport);
		}

		public void UpdateAllDeclarationTransportDataIfEmpty(Transport transport)
		{
			UpdateAllDeclarationTransportDataCore(transport);
			if (Parent.DocsAndCartage != null)
			{
				Parent.Logs.CreateRecreateOrUpdateEventLog(
					Events.CargoAvailable,
					EstimateActual.Actual,
					Parent.DocsAndCartage.AvailableDate.ToOffset());
			}
		}

		void UpdateAllDeclarationTransportDataCore(Transport transport)
		{
			NotifyCarrierAddressChangedCore(transport, ZGuid.Empty);
			NotifyDischargeChangedCore(transport, ZString.Empty);
			NotifyATAChangedCore(transport, ZDateTime.Empty);
			NotifyETAChangedCore(transport, ZDateTime.Empty);
			NotifyLoadChangedCore(transport, ZString.Empty);
			NotifyATDChangedCore(transport, ZDateTime.Empty);
			NotifyETDChangedCore(transport, ZDateTime.Empty);
			NotifyTransportTypeChangedCore(transport, ZString.Empty);
			NotifyVesselChangedCore(transport, ZString.Empty);
			NotifyVoyageFlightChangedCore(transport, ZString.Empty);
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceCustoms; }
		}

		public override JobConsolTransportValidation GetNewTransportValidator(Transport transport)
		{
			return new BaseJobDeclarationTransportValidation(transport);
		}
	}
}
