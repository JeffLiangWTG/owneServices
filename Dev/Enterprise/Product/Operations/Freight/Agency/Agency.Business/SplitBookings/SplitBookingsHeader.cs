using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public class SplitBookingsHeader : AutoSplitBookingsHeader
	{
		public enum MoveDirection
		{
			ToNew,
			ToOriginal,
		}

		public new abstract class Schema : AutoSplitBookingsHeader.Schema
		{
			public const string OriginalShipment = "OriginalShipment";
			public const string NewShipment = "NewShipment";
		}

		public SplitBookingsHeader(BusinessObjectFactory factory)
			: base(factory) { }

		public static SplitBookingsHeader New(AgencyBooking shipment)
		{
			if (shipment == null)
			{
				throw new ArgumentNullException(nameof(shipment));
			}

			AgencyShipment newShipment = (AgencyShipment)shipment.Clone(new BusinessObjectCloneArgs(new string[]
			{
				JobShipmentSchema.Constants.JS_HouseBill,
				JobShipmentSchema.Constants.JS_UniqueConsignRef,
				JobShipmentSchema.Constants.JS_CFSReference,
			}));

			newShipment.OuterPackLines.RemoveAndDeleteAll();

			shipment.SetCountedReadOnlyIncludingChildren(true);
			newShipment.SetCountedReadOnlyIncludingChildren(true);

			CopyTransportsToNewShipment(shipment, newShipment);

			return new SplitBookingsHeader(shipment.Factory)
			{
				ShowContainers = shipment.IsFCL,
				ShowActualContainers = shipment.IsFCL && shipment.RealContainers.Count > 0,
				ShowVehicles = shipment.IsRollOnRollOff,
				ShowTopLevelPacks = shipment.IsTopLevelPacksMode && !shipment.IsRollOnRollOff,
				ShowPackLines = shipment.IsFCL,
				OriginalShipmentPK = shipment.PK,
				NewShipmentPK = newShipment.PK,
			};
		}

		public AgencyShipment OriginalShipment
		{
			get { return Factory.Load<AgencyBooking>(OriginalShipmentPK); }
		}

		public AgencyBooking NewShipment
		{
			get { return Factory.Load<AgencyBooking>(NewShipmentPK); }
		}

		public void MovePackline(AgencyBookingPackLine packline, MoveDirection direction)
		{
			if (packline == null)
			{
				throw new ArgumentNullException(nameof(packline));
			}

			AgencyBookingContainer container = packline.Container;

			if (container != null)
			{
				MoveBookedContainerAndRelatedPacklinesCore(container, direction);
			}
			else
			{
				MovePacklineCore(packline, direction);
			}
		}

		public void MoveBookedContainer(AgencyBookingContainer container, MoveDirection direction)
		{
			if (container == null)
			{
				throw new ArgumentNullException(nameof(container));
			}

			MoveBookedContainerAndRelatedPacklinesCore(container, direction);
		}

		public void MoveActualContainer(AgencyBookingContainer container, MoveDirection direction)
		{
			if (container == null)
			{
				throw new ArgumentNullException(nameof(container));
			}

			GetSourceShipmentForDirection(direction).RealContainers.Remove(container);
			GetDestinationShipmentForDirection(direction).RealContainers.Add(container);
		}

		public void MoveTopLevelPack(AgencyShipmentContainer topLevelPack, MoveDirection direction)
		{
			if (topLevelPack == null)
			{
				throw new ArgumentNullException(nameof(topLevelPack));
			}

			GetSourceShipmentForDirection(direction).TopLevelPacks.Remove(topLevelPack);
			GetDestinationShipmentForDirection(direction).TopLevelPacks.Add(topLevelPack);
		}

		public void MoveVehicle(AgencyShipmentContainer vehicle, MoveDirection direction)
		{
			if (vehicle == null)
			{
				throw new ArgumentNullException(nameof(vehicle));
			}

			GetSourceShipmentForDirection(direction).Vehicles.Remove(vehicle);
			GetDestinationShipmentForDirection(direction).Vehicles.Add(vehicle);
		}

		AgencyShipment GetSourceShipmentForDirection(MoveDirection direction)
		{
			return direction == MoveDirection.ToNew ? OriginalShipment : NewShipment;
		}

		AgencyShipment GetDestinationShipmentForDirection(MoveDirection direction)
		{
			return direction == MoveDirection.ToNew ? NewShipment : OriginalShipment;
		}

		void MoveBookedContainerAndRelatedPacklinesCore(AgencyBookingContainer container, MoveDirection direction)
		{
			AgencyBookingPackLine[] packlines = container.PackLines.ToArray<AgencyBookingPackLine>();

			GetSourceShipmentForDirection(direction).BookedContainers.Remove(container);
			GetDestinationShipmentForDirection(direction).BookedContainers.Add(container);

			for (int i = 0; i < packlines.Length; i++)
			{
				MovePackline(packlines[i], direction);

				packlines[i].JL_JC = container.PK;
			}
		}

		void MovePacklineCore(AgencyBookingPackLine packline, MoveDirection direction)
		{
			GetSourceShipmentForDirection(direction).OuterPackLines.Remove(packline);
			GetDestinationShipmentForDirection(direction).OuterPackLines.Add(packline);
		}

		static void CopyTransportsToNewShipment(AgencyBooking shipment, AgencyShipment newShipment)
		{
			newShipment.Transports.RemoveAndDeleteAll();

			foreach (var transport in shipment.Transports)
			{
				var duplicateTransport = (Transport)transport.Clone();
				duplicateTransport.JW_ParentGUID = newShipment.PK;
				duplicateTransport.JW_ParentType = Constants.TransportParentTypes.AgencyShipment;

				newShipment.Transports.Add(duplicateTransport);
			}
		}
	}
}


