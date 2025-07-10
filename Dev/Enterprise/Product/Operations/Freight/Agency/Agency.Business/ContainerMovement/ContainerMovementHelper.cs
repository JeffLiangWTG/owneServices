using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	[CargoWise.Common.Testing.SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public static class ContainerMovementHelper
	{
		public static ContainerMovement FindDuplicate(RefContainerStock stock, ZString type, ZDateTime dateTime)
		{
			if (stock == null)
			{
				throw new ArgumentNullException(nameof(stock));
			}

			if (!dateTime.IsValidSmallDateTime)
			{
				return null;
			}
			else
			{
				ZQuery filter = new ZQuery();
				filter.AddToFilter(JobContainerMoveSchema.E9_R6, stock.PK);
				filter.AddToFilter(JobContainerMoveSchema.E9_MovementType, type);
				filter.AddToFilter(JobContainerMoveSchema.E9_MovementDate, dateTime.ToSmallDateTime());

				return stock.Factory.LoadTop1<ContainerMovement>(filter);
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static string GetEventReferenceForMovement(ContainerMovement movement)
		{
			var parameters = GetEventReferenceForMovementDictionary(movement);
			var reference = string.Empty;

			if (parameters.IsNullOrEmpty())
			{
				reference = movement.DepotPort;
			}

			return StmALog.GenerateEventReference(reference, parameters);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static Dictionary<string, string> GetEventReferenceForMovementDictionary(ContainerMovement movement)
		{
			var parameters = new Dictionary<string, string>();

			switch (movement.E9_MovementType)
			{
				case ContainerMovementTypes.Codes.WharfGateOut:
				case ContainerMovementTypes.Codes.WharfGateIn:
				case ContainerMovementTypes.Codes.ReturnToWharf:
				case ContainerMovementTypes.Codes.Load:
				case ContainerMovementTypes.Codes.Discharge:

					parameters[Constants.EventReferenceParameters.Codes.Facility] = Constants.Facilities.Code.Terminal;
					parameters[Constants.EventReferenceParameters.Codes.Location] = movement.DepotPort;

					if (movement.E9_MovementType == ContainerMovementTypes.Codes.Load || movement.E9_MovementType == ContainerMovementTypes.Codes.Discharge)
					{
						var transportMode = movement.TransportMode;
						parameters[Constants.EventReferenceParameters.Codes.Mode] = transportMode.IsEmpty ? movement.Voyage?.JV_AirSeaRoad : transportMode;
					}
					break;

				case ContainerMovementTypes.Codes.DepotGateIn:
				case ContainerMovementTypes.Codes.DepotGateOut:

					parameters[Constants.EventReferenceParameters.Codes.Facility] = Constants.Facilities.Code.Depot;
					parameters[Constants.EventReferenceParameters.Codes.Location] = movement.DepotPort;
					break;

				case ContainerMovementTypes.Codes.YardGateOut:
				case ContainerMovementTypes.Codes.YardGateIn:
				case ContainerMovementTypes.Codes.ReturnedUnshipped:
				case ContainerMovementTypes.Codes.RePositionIntoYard:
				case ContainerMovementTypes.Codes.RePositionOutOfYard:
				case ContainerMovementTypes.Codes.OffHire:
				case ContainerMovementTypes.Codes.OnHire:

					parameters[Constants.EventReferenceParameters.Codes.Facility] = Constants.Facilities.Code.ContainerYard;
					parameters[Constants.EventReferenceParameters.Codes.Location] = movement.DepotPort;
					break;

				case ContainerMovementTypes.Codes.ReShipRequested:

					parameters[Constants.EventReferenceParameters.Codes.Facility] = Constants.Facilities.Code.Consignee;
					parameters[Constants.EventReferenceParameters.Codes.Location] = movement.DepotPort;
					break;
			}

			return parameters;
		}
	}
}


