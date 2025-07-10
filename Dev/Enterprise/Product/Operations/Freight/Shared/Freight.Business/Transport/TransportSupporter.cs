using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.Freight.Business
{
	public abstract class TransportSupporter : TransportSupporterCommon
	{
		protected TransportSupporter(ITransportChangeNotifier changeNotifier) : base()
		{
			Argument.NotNull(changeNotifier, "changeNotifier");
			ChangeNotifier = changeNotifier;
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		protected readonly ITransportChangeNotifier ChangeNotifier;

		public abstract ZGuid ShippingLine { get; set; }

		#region Container Mode

		public virtual bool IsDepartureContainerModeFCLorULD
		{
			get
			{
				return Constants.ContainerModes.IsFCLType(ContainerMode);
			}
		}

		public virtual bool IsArrivalContainerModeFCLorULD
		{
			get
			{
				return Constants.ContainerModes.IsFCLType(ContainerMode);
			}
		}

		#endregion

		#region Bill Of Landing

		public virtual bool BillOfLadingHasChanges
		{
			get { return false; }
		}

		#endregion

		public virtual bool DontErrorOnMissingDetails
		{
			get { return false; }
		}

		public virtual bool SupportETD => true;

		public virtual bool SupportVoyageFlight => true;

		public virtual JobConsolTransportValidation GetNewTransportValidator(Transport transport)
		{
			return null;
		}

		public virtual void SetConsignmentRefIfNotSet()
		{
		}

		public void ETDSetFromSailing(Transport transport, ZDateTime oldValue, ZDateTime newValue)
		{
			RunWithExceptionHandling(() => ETDSetFromSailingCore(transport, oldValue, newValue), "ETDSetFromSailing");
		}

		public void ETASetFromSailing(Transport transport, ZDateTime oldValue, ZDateTime newValue)
		{
			RunWithExceptionHandling(() => ETASetFromSailingCore(transport, oldValue, newValue), "ETASetFromSailing");
		}

		public void NotifyTransportTypeChanged(Transport transport, ZString previousValue)
		{
			Action action = () =>
			{
				NotifyTransportTypeChangedCore(transport, previousValue);
				ChangeNotifier.NotifyChanged(TransportChangeNotifyType.TransportType, transport, previousValue);
			};

			RunWithExceptionHandling(action, "NotifyTransportTypeChanged");
		}

		public void NotifySailingChanged(Transport transport, ZGuid previousValue)
		{
			Action action = () =>
			{
				NotifySailingChangedCore(transport, previousValue);
				ChangeNotifier.NotifyChanged(TransportChangeNotifyType.Sailing, transport, previousValue);
			};

			RunWithExceptionHandling(action, "NotifySailingChanged");
		}

		public void NotifyLoadChanged(Transport transport, ZString previousValue)
		{
			Action action = () =>
			{
				NotifyLoadChangedCore(transport, previousValue);
				ChangeNotifier.NotifyChanged(TransportChangeNotifyType.Load, transport, previousValue);
			};

			RunWithExceptionHandling(action, "NotifyLoadChanged");
		}

		public void NotifyDischargeChanged(Transport transport, ZString previousValue)
		{
			Action action = () =>
			{
				NotifyDischargeChangedCore(transport, previousValue);
				ChangeNotifier.NotifyChanged(TransportChangeNotifyType.Discharge, transport, previousValue);
			};

			RunWithExceptionHandling(action, "NotifyDischargeChanged");
		}

		public void NotifyVesselChanged(Transport transport, ZString previousValue)
		{
			Action action = () =>
			{
				NotifyVesselChangedCore(transport, previousValue);
				ChangeNotifier.NotifyChanged(TransportChangeNotifyType.Vessel, transport, previousValue);
			};

			RunWithExceptionHandling(action, "NotifyVesselChanged");
		}

		public void NotifyVoyageFlightChanged(Transport transport, ZString previousValue)
		{
			Action action = () =>
			{
				NotifyVoyageFlightChangedCore(transport, previousValue);
				ChangeNotifier.NotifyChanged(TransportChangeNotifyType.VoyageFlight, transport, previousValue);
			};

			RunWithExceptionHandling(action, "NotifyVoyageFlightChanged");
		}

		public void NotifyETDChanged(Transport transport, ZDateTime previousValue)
		{
			Action action = () =>
			{
				NotifyETDChangedCore(transport, previousValue);
				ChangeNotifier.NotifyChanged(TransportChangeNotifyType.ETD, transport, previousValue);
			};

			RunWithExceptionHandling(action, "NotifyETDChanged");
		}

		public void NotifyETAChanged(Transport transport, ZDateTime previousValue)
		{
			Action action = () =>
			{
				NotifyETAChangedCore(transport, previousValue);
				ChangeNotifier.NotifyChanged(TransportChangeNotifyType.ETA, transport, previousValue);
			};

			RunWithExceptionHandling(action, "NotifyETAChanged");
		}

		public void NotifyATDChanged(Transport transport, ZDateTime previousValue)
		{
			Action action = () =>
			{
				NotifyATDChangedCore(transport, previousValue);
				ChangeNotifier.NotifyChanged(TransportChangeNotifyType.ATD, transport, previousValue);
			};

			RunWithExceptionHandling(action, "NotifyATDChanged");
		}

		public void NotifyATAChanged(Transport transport, ZDateTime previousValue)
		{
			Action action = () =>
			{
				NotifyATAChangedCore(transport, previousValue);
				ChangeNotifier.NotifyChanged(TransportChangeNotifyType.ATA, transport, previousValue);
			};

			RunWithExceptionHandling(action, "NotifyATAChanged");
		}

		public void NotifyTerminalAvailabilityDateChanged(Transport transport, ZDateTime previousValue)
		{
			Action action = () =>
			{
				NotifyTerminalAvailabilityDateChangedCore(transport, previousValue);
				ChangeNotifier.NotifyChanged(TransportChangeNotifyType.TerminalAvailabilityDate, transport, previousValue);
			};

			RunWithExceptionHandling(action, nameof(TransportChangeNotifyType.TerminalAvailabilityDate));
		}

		public void NotifyTerminalStorageDateChanged(Transport transport, ZDateTime previousValue)
		{
			Action action = () =>
			{
				NotifyTerminalStorageDateChangedCore(transport, previousValue);
				ChangeNotifier.NotifyChanged(TransportChangeNotifyType.TerminalStorageDate, transport, previousValue);
			};

			RunWithExceptionHandling(action, nameof(TransportChangeNotifyType.TerminalStorageDate));
		}

		public void NotifyDepotAvailabilityDateChanged(Transport transport, ZDateTime previousValue)
		{
			Action action = () =>
			{
				NotifyDepotAvailabilityDateChangedCore(transport, previousValue);
				ChangeNotifier.NotifyChanged(TransportChangeNotifyType.DepotAvailabilityDate, transport, previousValue);
			};

			RunWithExceptionHandling(action, nameof(TransportChangeNotifyType.DepotAvailabilityDate));
		}

		public void NotifyDepotStorageDateChanged(Transport transport, ZDateTime previousValue)
		{
			Action action = () =>
			{
				NotifyDepotStorageDateChangedCore(transport, previousValue);
				ChangeNotifier.NotifyChanged(TransportChangeNotifyType.DepotStorageDate, transport, previousValue);
			};

			RunWithExceptionHandling(action, nameof(TransportChangeNotifyType.DepotStorageDate));
		}

		public void NotifyCarrierAddressChanged(Transport transport, ZGuid previousValue)
		{
			Action action = () =>
			{
				NotifyCarrierAddressChangedCore(transport, previousValue);
				ChangeNotifier.NotifyChanged(TransportChangeNotifyType.CarrierAddress, transport, previousValue);
			};

			RunWithExceptionHandling(action, "NotifyCarrierAddressChanged");
		}

		public void NotifyCarrierBookingRefChanged(Transport transport, ZString previousValue)
		{
			Action action = () =>
			{
				NotifyCarrierBookingRefChangedCore(transport, previousValue);
				ChangeNotifier.NotifyChanged(TransportChangeNotifyType.CarrierBookingRef, transport, previousValue);
			};

			RunWithExceptionHandling(action, "NotifyCarrierBookingRefChanged");
		}

		public void NotifyLegOrderChanged(Transport transport, ZByte previousValue)
		{
			Action action = () =>
			{
				NotifyLegOrderChangedCore(transport, previousValue);
				ChangeNotifier.NotifyChanged(TransportChangeNotifyType.LegOrder, transport, previousValue);
			};

			RunWithExceptionHandling(action, "NotifyLegOrderChanged");
		}

		public void NotifyIsCharterChanged(Transport transport, ZBool previousValue)
		{
			Action action = () =>
			{
				NotifyIsCharterChangedCore(transport, previousValue);
				ChangeNotifier.NotifyChanged(TransportChangeNotifyType.IsCharter, transport, previousValue);
			};

			RunWithExceptionHandling(action, "NotifyIsCharterChanged");
		}

		public void NotifyAircraftTypeChanged(Transport transport, ZString previousValue)
		{
			Action action = () =>
			{
				NotifyAircraftTypeChangedCore(transport, previousValue);
				ChangeNotifier.NotifyChanged(TransportChangeNotifyType.AircraftType, transport, previousValue);
			};

			RunWithExceptionHandling(action, "NotifyAircraftTypeChanged");
		}

		public void NotifyArrivalLocationChanged(Transport transport, ZGuid previousValue)
		{
			Action action = () =>
			{
				NotifyArrivalLocationChangedCore(transport, previousValue);
				ChangeNotifier.NotifyChanged(TransportChangeNotifyType.ArrivalLocation, transport, previousValue);
			};

			RunWithExceptionHandling(action, "NotifyArrivalLocationChanged");
		}

		public void NotifyDepartureLocationChanged(Transport transport, ZGuid previousValue)
		{
			Action action = () =>
			{
				NotifyDepartureLocationChangedCore(transport, previousValue);
				ChangeNotifier.NotifyChanged(TransportChangeNotifyType.DepartureLocation, transport, previousValue);
			};

			RunWithExceptionHandling(action, "NotifyDepartureLocationChanged");
		}

		public void NotifyTransportModeChanged(Transport transport, ZString previousValue)
		{
			Action action = () =>
			{
				NotifyTransportModeChangedCore(transport, previousValue);
				ChangeNotifier.NotifyChanged(TransportChangeNotifyType.TransportMode, transport, previousValue);
			};

			RunWithExceptionHandling(action, "NotifyTransportModeChanged");
		}

		public void NotifyVoyageUpdated(Transport transport)
		{
			NotifyVoyageUpdatedCore(transport);
		}

		void RunWithExceptionHandling(Action action, string errorMessage)
		{
			try
			{
				action();
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}

				ErrorReporter.ReportOnce(errorMessage, ex);
			}
		}

		protected virtual void ETDSetFromSailingCore(Transport transport, ZDateTime oldValue, ZDateTime newValue)
		{
		}
		protected virtual void ETASetFromSailingCore(Transport transport, ZDateTime oldValue, ZDateTime newValue)
		{
		}
		protected virtual void NotifyTransportTypeChangedCore(Transport transport, ZString previousValue)
		{
		}
		protected virtual void NotifySailingChangedCore(Transport transport, ZGuid previousValue)
		{
		}
		protected virtual void NotifyLoadChangedCore(Transport transport, ZString previousValue)
		{
		}
		protected virtual void NotifyDischargeChangedCore(Transport transport, ZString previousValue)
		{
		}
		protected virtual void NotifyVesselChangedCore(Transport transport, ZString previousValue)
		{
		}
		protected virtual void NotifyVoyageFlightChangedCore(Transport transport, ZString previousValue)
		{
		}
		protected virtual void NotifyETDChangedCore(Transport transport, ZDateTime previousValue)
		{
		}
		protected virtual void NotifyETAChangedCore(Transport transport, ZDateTime previousValue)
		{
		}
		protected virtual void NotifyATDChangedCore(Transport transport, ZDateTime previousValue)
		{
		}
		protected virtual void NotifyATAChangedCore(Transport transport, ZDateTime previousValue)
		{
		}
		protected virtual void NotifyTerminalAvailabilityDateChangedCore(Transport transport, ZDateTime previousValue)
		{
		}
		protected virtual void NotifyTerminalStorageDateChangedCore(Transport transport, ZDateTime previousValue)
		{
		}
		protected virtual void NotifyDepotAvailabilityDateChangedCore(Transport transport, ZDateTime previousValue)
		{
		}
		protected virtual void NotifyDepotStorageDateChangedCore(Transport transport, ZDateTime previousValue)
		{
		}
		protected virtual void NotifyCarrierAddressChangedCore(Transport transport, ZGuid previousValue)
		{
		}
		protected virtual void NotifyCarrierBookingRefChangedCore(Transport transport, ZString previousValue)
		{
		}
		protected virtual void NotifyLegOrderChangedCore(Transport transport, ZByte previousValue)
		{
		}

		protected virtual void NotifyIsCharterChangedCore(Transport transport, ZBool previousValue)
		{
		}

		protected virtual void NotifyAircraftTypeChangedCore(Transport transport, ZString previousValue)
		{
		}

		protected virtual void NotifyArrivalLocationChangedCore(Transport transport, ZGuid previousValue)
		{
		}

		protected virtual void NotifyDepartureLocationChangedCore(Transport transport, ZGuid previousValue)
		{
		}

		protected virtual void NotifyTransportModeChangedCore(Transport transport, ZString previousValue)
		{
		}

		protected virtual void NotifyVoyageUpdatedCore(Transport transport)
		{
		}
	}

	public abstract class TransportSupporter<T> : TransportSupporter
		where T : BusinessObject, ITransportChangeNotifier
	{
		protected TransportSupporter(T parent)
			: base(parent)
		{
		}

		public T Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (T)ChangeNotifier; }
		}
	}
}
