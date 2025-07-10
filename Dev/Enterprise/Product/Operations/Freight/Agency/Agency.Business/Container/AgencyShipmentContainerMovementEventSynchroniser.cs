using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EventReference;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Agency.Business
{
	class AgencyShipmentContainerMovementEventSynchroniser
	{
		public AgencyShipmentContainerMovementEventSynchroniser(AgencyShipmentContainer parentContainer)
		{
			if (parentContainer == null)
			{
				throw new ArgumentNullException(nameof(parentContainer));
			}

			this.parentContainer = parentContainer;
		}

		readonly AgencyShipmentContainer parentContainer;
		long lastFactoryTransactionId;
		ContainerMovement deletingMovement;

		public void UpdateContainerMovementEvents(ContainerMovement movement)
		{
			this.deletingMovement = movement;

			UpdateContainerMovementEventsCore();
		}

		void UpdateContainerMovementEventsCore()
		{
			var factory = parentContainer.Factory;
			if ((lastFactoryTransactionId != factory.TransactionId || factory.TransactionId == 0) && !parentContainer.IsTopLevelPack)
			{
				lastFactoryTransactionId = factory.TransactionId;
				CancelEventLogsWithoutCorrespondingMovements();
				CreateEventLogsForMissingMovements();
			}
		}

		void CreateEventLogsForMissingMovements()
		{
			var existingEvents = GetActiveLogsFromMovementEvents().ToList();
			var movements = GetActiveMovements();

			foreach (var movement in movements)
			{
				var mappedEvents = MovementTypeCodeEventMap[movement.E9_MovementType];
				var newReference = ContainerMovementHelper.GetEventReferenceForMovement(movement);
				var newParameters = ContainerMovementHelper.GetEventReferenceForMovementDictionary(movement);
				var newLocation = newParameters.GetValueSafe(Constants.EventReferenceParameters.Codes.Location);
				var newFacility = newParameters.GetValueSafe(Constants.EventReferenceParameters.Codes.Facility);

				foreach (var mappedEvent in mappedEvents)
				{
					var log = existingEvents.FirstOrDefault(l => l.SL_EventTime == movement.E9_MovementDate
						&& l.SL_SE_NKEvent == mappedEvent
						&& l.Parameters.GetValueSafe(Constants.EventReferenceParameters.Codes.Location) == newLocation
						&& l.Parameters.GetValueSafe(Constants.EventReferenceParameters.Codes.Facility) == newFacility);

					if (log == null)
					{
						// Assumingthat E9_Movement timezone is the same as current branch
						log = parentContainer.Logs.AddNew(Events.All[mappedEvent], newReference, movement.E9_MovementDate.ToOffset());

						existingEvents.Add(log);
					}
				}
			}
		}

		void CancelEventLogsWithoutCorrespondingMovements()
		{
			var existingLogs = GetActiveLogsFromMovementEvents();
			var movements = GetActiveMovements();

			foreach (var log in existingLogs)
			{
				var mappedMovements = GetMovementTypesFromEventCode(log.SL_SE_NKEvent);
				var movement = movements.FirstOrDefault(m =>
				{
					var newParameters = ContainerMovementHelper.GetEventReferenceForMovementDictionary(m);
					var newLocation = newParameters.GetValueSafe(Constants.EventReferenceParameters.Codes.Location);
					var newFacility = newParameters.GetValueSafe(Constants.EventReferenceParameters.Codes.Facility);

					return m.E9_MovementDate == log.SL_EventTime
									&& mappedMovements.Contains(m.E9_MovementType.ToString())
									&& log.Parameters.GetValueSafe(Constants.EventReferenceParameters.Codes.Location) == newLocation
									&& log.Parameters.GetValueSafe(Constants.EventReferenceParameters.Codes.Facility) == newFacility;
				});

				if (movement == null)
				{
					log.Cancel();
				}
			}
		}

		IEnumerable<StmALog> GetActiveLogsFromMovementEvents()
		{
			var events = MovementTypeCodeEventMap.SelectMany(p => p.Value).ToList();

			return parentContainer.Logs.Find(l => !l.IsCancelled && events.Contains(l.SL_SE_NKEvent.ToString())).ToList();
		}

		IEnumerable<ContainerMovement> GetActiveMovements()
		{
			return parentContainer.Movements.Where(m => m != deletingMovement
				&& !m.DepotPort.IsEmpty
				&& MovementTypeCodeEventMap.ContainsKey(m.E9_MovementType.ToString()));
		}

		#region Mapping

		static IEnumerable<string> GetMovementTypesFromEventCode(string eventCode)
		{
			return MovementTypeCodeEventMap.Where(m => m.Value.Contains(eventCode)).Select(p => p.Key).ToList();
		}

		public static Dictionary<string, string[]> MovementTypeCodeEventMap
		{
			get
			{
				if (movementTypeCodeEventMap == null)
				{
					movementTypeCodeEventMap = new Dictionary<string, string[]>
					{
						{ ContainerMovementTypes.Codes.Discharge,        new [] { AutoEvents.FreightUnloadedCode } },
						{ ContainerMovementTypes.Codes.WharfGateOut,     new [] { AutoEvents.GateOutCode } },
						{ ContainerMovementTypes.Codes.DepotGateIn,      new [] { AutoEvents.GateInCode } },
						{ ContainerMovementTypes.Codes.DepotGateOut,     new [] { AutoEvents.GateOutCode } },
						{ ContainerMovementTypes.Codes.ReturnToWharf,    new [] { AutoEvents.GateInCode, AutoEvents.DehireCode } },
						{ ContainerMovementTypes.Codes.YardGateIn,       new [] { AutoEvents.GateInCode, AutoEvents.DehireCode } },
						{ ContainerMovementTypes.Codes.YardGateOut,      new [] { AutoEvents.GateOutCode } },
						{ ContainerMovementTypes.Codes.WharfGateIn,      new [] { AutoEvents.GateInCode } },
						{ ContainerMovementTypes.Codes.Load,             new [] { AutoEvents.FreightLoadedCode } },
						{ ContainerMovementTypes.Codes.ReturnedUnshipped, new [] { AutoEvents.DehireCode } },
						{ ContainerMovementTypes.Codes.ReShipRequested,  new [] { AutoEvents.DehireCode } }
					};
				}

				return movementTypeCodeEventMap;
			}
		}

		[ThreadStatic]
		static Dictionary<string, string[]> movementTypeCodeEventMap;

		#endregion
	}
}





