using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;

namespace Enterprise.Freight.Business
{
	[UniversalCopyClearCollectionOnCopy(ClearAfterAllElementsWereCopied = true)]
	public class ConsolTransportCollection : TransportCollection
	{
		public ConsolTransportCollection(CommonConsol parent)
			: base(parent)
		{
			this.ParentConsol = parent;
		}

		#region Transports

		public Transport ExportTransport
		{
			get { return OrderHelper.ExportLeg; }
		}

		public Transport ImportTransport
		{
			get { return OrderHelper.ImportLeg; }
		}

		public Transport ArrivalTransport
		{
			get { return OrderHelper.LastLeg; }
		}

		public Transport DepartureTransport
		{
			get { return OrderHelper.FirstLeg; }
		}

		public Transport FirstImportTransportByLoadAndDischarge()
		{
			return OrderHelper.FirstImportTransportByLoadAndDischarge();
		}

		public Transport FirstTransportWithTransportMode(ZString transportMode)
		{
			return OrderHelper.FirstLegWithTransportMode(transportMode);
		}

		public Transport FirstTransportWithTransportModeAndExportVessel(ZString transportMode)
		{
			if (ExportTransport == null)
			{
				return OrderHelper.FirstLegMatching(t => t.JW_TransportMode == transportMode);
			}
			else
			{
				return OrderHelper.FirstLegMatching(t => IsLegWithTransportModeAndVessel(t, transportMode, ExportTransport.Vessel, ExportTransport.JW_VoyageFlight));
			}
		}

		public Transport LastTransportWithTransportMode(ZString transportMode)
		{
			return OrderHelper.LastLegWithTransportMode(transportMode, null, ZString.Empty);
		}

		public Transport LastTransportWithTransportModeAndImportVessel(ZString transportMode)
		{
			if (ImportTransport == null)
			{
				return OrderHelper.LastLegMatching(t => t.JW_TransportMode == transportMode);
			}
			else
			{
				return OrderHelper.LastLegMatching(t => IsLegWithTransportModeAndVessel(t, transportMode, ImportTransport.Vessel, ImportTransport.JW_VoyageFlight));
			}
		}

		public Transport MostInterestingTransport
		{
			get
			{
				if (mostInterestingTransportCache == null)
				{
					mostInterestingTransportCache = new CachedProperty<Transport>(Factory, GetMostInterestingTransport);
				}
				return mostInterestingTransportCache.Value;
			}
		}

		CachedProperty<Transport> mostInterestingTransportCache;

		bool IsLegWithTransportModeAndVessel(Transport transport, ZString transportMode, RefVessel vessel, ZString voyageFlight)
		{
			if (transport.JW_TransportMode != transportMode)
			{
				return false;
			}
			else if (transport.IsSea)
			{
				return vessel == null || transport.Vessel == vessel;
			}
			else if (transport.IsAir)
			{
				return voyageFlight.IsEmpty || voyageFlight == transport.JW_VoyageFlight;
			}
			else
			{
				return true;
			}
		}

		Transport GetMostInterestingTransport()
		{
			Transport result;

			if (Count == 1)
			{
				result = this[0];
			}
			else if (Count == 0)
			{
				result = null;
			}
			else if (ParentConsol.IsImport())
			{
				result = OrderHelper.LastLegWithTransportMode(ParentConsol.JK_TransportMode, null, ZString.Empty);

				if (result == null)
				{
					result = OrderHelper.LastLeg;
				}
			}
			else
			{
				result = OrderHelper.FirstLegWithTransportMode(ParentConsol.JK_TransportMode);

				if (result == null)
				{
					result = OrderHelper.FirstLeg;
				}
			}

			return result;
		}

		#endregion

		#region FindByLoadPort / FindByDischargePort

		public Transport FindByLoadPort(ZString loadPort)
		{
			foreach (Transport transport in this)
			{
				if (transport.JW_RL_NKLoadPort == loadPort)
				{
					return transport;
				}
			}
			return null;
		}

		public Transport FindByDischargePort(ZString dischargePort)
		{
			foreach (Transport transport in this)
			{
				if (transport.JW_RL_NKDiscPort == dischargePort)
				{
					return transport;
				}
			}
			return null;
		}

		#endregion

		#region Overrides

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			var transport = (Transport)bizOAdded;
			ParentConsol?.OnTransportAdded(transport);
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			var transport = (Transport)bizO;
			ParentConsol?.OnTransportRemoved(transport);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			Transport transport = (Transport)child;
			base.SetDefaultsForNewChild(transport);

			int nextLeg = 1;
			foreach (Transport existingTransport in this)
			{
				if (existingTransport.JW_LegOrder >= nextLeg)
				{
					nextLeg = existingTransport.JW_LegOrder + 1;
				}
			}

			transport.JW_LegOrder = (ZByte)((short)nextLeg);
		}

		public override void Load()
		{
			base.Load();
			AddInitialTransportIfEmpty();
		}

		public override void Remove(BusinessObject elementToRemove)
		{
			base.Remove(elementToRemove);
			AddInitialTransportIfEmpty();
		}

		#endregion

		#region ParentDeleting

		/// <summary>
		/// Only call from ParentConsol.Delete()
		/// </summary>
		internal void ParentDeleting()
		{
			EnforceNonZeroTransportCount = false;
			RemoveAndDeleteAll();
		}

		internal void CancelParentDeleting()
		{
			EnforceNonZeroTransportCount = true;
			AddInitialTransportIfEmpty();
		}

		bool EnforceNonZeroTransportCount = true;

		#endregion

		#region Implementation

		#region AddInitialTransportIfEmpty

		void AddInitialTransportIfEmpty()
		{
			if (EnforceNonZeroTransportCount && Count == 0 && !IsRefreshingByDataRefreshBus)
			{
				Transport transport = AddNew();

				using (transport.SuspendSettingHasChanges())
				using (transport.GetValidationSuspender())
				{
					if (!ParentConsol.IsCourier)
					{
						transport.JW_IsLinked = true;
					}
				}
			}
		}
		readonly CommonConsol ParentConsol;

		#endregion

		#region OrderHelper

		TransportOrderHelper OrderHelper
		{
			get
			{
				// A removed transport could be still kept in reference due to cached OrderHelper
				// Thus we disable caching of OrderHelper here
				return new TransportOrderHelper(this);
			}
		}

		#endregion

		#endregion
	}
}
