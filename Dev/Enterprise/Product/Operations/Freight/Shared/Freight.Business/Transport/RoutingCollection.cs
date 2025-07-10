using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class RoutingCollection : BusinessObjectCollection<Transport>, ITransportCollection
	{
		public RoutingCollection(ITransportParent mainTransportParent)
			: base(mainTransportParent.Factory)
		{
			AddTransportFetchHints(mainTransportParent);
			this.mainTransportParent = mainTransportParent;
			this.mainTransportCollection = mainTransportParent.Transports;
			MonitorCollection(mainTransportCollection);
		}

		Transport ITransportCollection.this[int index]
		{
			get { return base[index]; }
		}

		#region Overrides

		public override void Load()
		{
			throw new NotSupportedException();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return mainTransportCollection.TypeOfElements;
		}

		protected override BusinessObject AddNewCore(Type bizOType)
		{
			throw new NotSupportedException();
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			magicSuspended = true;
			try
			{
				Transport transport = (Transport)child;
				mainTransportCollection.Add(transport);
				mainTransportCollection.SetDefaultsAsThoughForNewChild(transport);
			}
			finally
			{
				magicSuspended = false;
			}
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			Transport transport = (Transport)elementToDelete;

			if (BelongsToMainCollection(transport))
			{
				base.RemoveAndDelete(elementToDelete);
			}
			else
			{
				throw new CannotDeleteException(string.Format("This routing leg comes from '{0}' you will need to delete it from there.", transport.JW_ParentDescription));
			}
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			Transport transport = (Transport)bizOAdded;
			transport.ReadOnly = ShouldBeReadOnly(transport);
		}

		protected override bool AllowNewCore => base.AllowNewCore && allowNewCore;

		public void SetAllowNewCore(bool value)
		{
			allowNewCore = value;
		}

		bool allowNewCore = true;

		#endregion

		#region EnablePluginBehaviour

		public bool PluginBehaviourEnabled
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return pluginBehaviourEnabled; }
			set
			{
				if (value != pluginBehaviourEnabled)
				{
					pluginBehaviourEnabled = value;

					foreach (Transport transport in this)
					{
						transport.ReadOnly = ShouldBeReadOnly(transport);
					}
				}
			}
		}

		#endregion

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

		public Transport FirstLegMatching(Predicate<Transport> predicate)
		{
			return OrderHelper.FirstLegMatching(predicate);
		}

		public Transport LastLegMatching(Predicate<Transport> predicate)
		{
			return OrderHelper.LastLegMatching(predicate);
		}

		public Transport FirstLeg
		{
			get { return OrderHelper.FirstLeg; }
		}

		public Transport LastLeg
		{
			get { return OrderHelper.LastLeg; }
		}

		public List<Transport> GetChain(Transport transport) => OrderHelper.GetChain(transport);

		#endregion

		#region BelongsToMainCollection

		public bool BelongsToMainCollection(Transport transport)
		{
			return transport != null && transport.JW_ParentGUID == mainTransportCollection.Master.PK;
		}

		#endregion

		#region Monitor / UnMonitor Collection

		protected void MonitorCollection(TransportCollection collection)
		{
			if (!monitoredCollections.Contains(collection))
			{
				collection.CountChanged += new CollectionCountChangedEventHandler(TransportCollectionChanged);
				monitoredCollections.Add(collection);
				AddRange(collection);

				OnMonitoredCollectionsChanged(collection, true);
			}
		}

		protected void UnMonitorCollection(TransportCollection collection)
		{
			if (monitoredCollections.Contains(collection))
			{
				collection.CountChanged -= new CollectionCountChangedEventHandler(TransportCollectionChanged);
				monitoredCollections.Remove(collection);

				foreach (Transport transport in collection)
				{
					Remove(transport);
				}

				OnMonitoredCollectionsChanged(collection, false);
			}
		}

		#endregion

		#region AddTransportFetchHints

		protected virtual void AddTransportFetchHints(ITransportParent parent)
		{
			parent.Factory.AddFetchHint(JobConsolTransportSchema.JW_ParentGUID, parent.PK);
		}

		#endregion

		#region Events

		public event EventHandler<MonitoredCollectionsChangedEventArgs> MonitoredCollectionsChanged;

		void OnMonitoredCollectionsChanged(TransportCollection collection, bool collectionAdded)
		{
			if (MonitoredCollectionsChanged != null)
			{
				MonitoredCollectionsChanged(this, new MonitoredCollectionsChangedEventArgs(collection, collectionAdded));
			}
		}

		void TransportCollectionChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (!magicSuspended)
			{
				bool haveTransport = Contains(e.BizObject);

				if (e.ItemAdded && !haveTransport)
				{
					Add(e.BizObject);
				}
				else if (e.ItemRemoved && haveTransport)
				{
					Remove(e.BizObject);
				}
			}
		}

		#endregion

		#region IsAnyDischargeInCountry
		public bool IsAnyDischargeInCountry(string countryCode)
		{
			foreach (Transport transport in this)
			{
				if (transport.JW_RL_NKDiscPort.SubstringSafe(0, 2) == countryCode)
				{
					return true;
				}
			}
			return false;
		}
		#endregion

		#region IsAnyLoadPortInCountry

		public bool IsAnyLoadPortInCountry(string countryCode)
		{
			foreach (Transport transport in this)
			{
				if (transport.JW_RL_NKLoadPort.SubstringSafe(0, 2) == countryCode)
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		#region Route Sets

		RouteSets routeSets;
		public RouteSets RouteSets
		{
			get
			{
				if (routeSets == null)
				{
					routeSets = new RouteSets(Factory, this);
				}
				return routeSets;
			}
		}

		#endregion

		#region MainTransportParent

		public ITransportParent MainTransportParent => mainTransportParent;

		#endregion

		#region Implementation

		bool ShouldBeReadOnly(Transport transport)
		{
			return ReadOnly || !transport.IsPersistent || (PluginBehaviourEnabled && !BelongsToMainCollection(transport));
		}

		TransportOrderHelper OrderHelper
		{
			get
			{
				// A removed transport could be still kept in reference due to cached OrderHelper
				// Thus we disable caching of OrderHelper here
				return new TransportOrderHelper(this);
			}
		}

		public bool HasLoadPortOutsideOfIcs2Zone
		{
			get
			{
				return this.Cast<Transport>().Any(t =>
				{
					if (t.IsAir)
					{
						var loadPortRefUnloco = t.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, t.JW_RL_NKLoadPort);
						return loadPortRefUnloco != null && !loadPortRefUnloco.IsInIcs2Zone;
					}

					return false;
				});
			}
		}

		public bool HasIcs2ZoneAirDischarge
		{
			get
			{
				return this.Cast<Transport>().Any(t =>
				{
					if (t.IsAir)
					{
						var dischargeRefUnloco = t.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, t.JW_RL_NKDiscPort);
						return dischargeRefUnloco != null && dischargeRefUnloco.IsInIcs2Zone;
					}

					return false;
				});
			}
		}

		public bool IsAirImportOrTransitToICS2Zone
		{
			get
			{
				return this.Cast<Transport>().Any(t =>
					{
						if (t.IsAir)
						{
							var loadPortRefUnloco = t.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, t.JW_RL_NKLoadPort);
							var dischargeRefUnloco = t.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, t.JW_RL_NKDiscPort);

							return loadPortRefUnloco != null && !loadPortRefUnloco.IsInIcs2Zone && dischargeRefUnloco != null && dischargeRefUnloco.IsInIcs2Zone;
						}

						return false;
					});
			}
		}

		#endregion

		bool magicSuspended;
		bool pluginBehaviourEnabled;
		readonly TransportCollection mainTransportCollection;
		readonly List<TransportCollection> monitoredCollections = new List<TransportCollection>();
		readonly ITransportParent mainTransportParent;
	}
}
