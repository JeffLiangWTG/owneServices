using System;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class MostInterestingTransportBindingCollection : BusinessObjectCollection<Transport>
	{
		public MostInterestingTransportBindingCollection(CommonConsol consol)
			: base(consol.Factory)
		{
			this.consol = consol;
			HookConsol(consol);
			UpdateSelectedTransport();
			IsLoaded = true;
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			if (Contains(bizO.PK))
			{
				UpdateSelectedTransport();
			}
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			return ZQuery.NoResultQuery;
		}

		#region HookConsol

		void HookConsol(CommonConsol consol)
		{
			consol.Transports.CountChanged += new CollectionCountChangedEventHandler(Transports_CountChanged);

			consol.JK_TransportModeInfo.ValueChanged += new EventHandler(KeyFieldChanged);
			consol.JK_RL_NKLoadPortInfo.ValueChanged += new EventHandler(KeyFieldChanged);
			consol.JK_RL_NKDischargePortInfo.ValueChanged += new EventHandler(KeyFieldChanged);

			foreach (Transport transport in consol.Transports)
			{
				HookTransport(transport);
			}
		}

		void Transports_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				HookTransport((Transport)e.BizObject);
			}
			else if (e.ItemRemoved)
			{
				UnHookTransport((Transport)e.BizObject);
			}
			UpdateSelectedTransport();
		}

		#endregion

		#region Hook / UnHook Transport

		void HookTransport(Transport transport)
		{
			transport.JW_TransportModeInfo.ValueChanged += new EventHandler(KeyFieldChanged);
			transport.JW_RL_NKLoadPortInfo.ValueChanged += new EventHandler(KeyFieldChanged);
			transport.JW_RL_NKDiscPortInfo.ValueChanged += new EventHandler(KeyFieldChanged);
			transport.JW_LegOrderInfo.ValueChanged += new EventHandler(KeyFieldChanged);
		}

		void UnHookTransport(Transport transport)
		{
			transport.JW_TransportModeInfo.ValueChanged -= new EventHandler(KeyFieldChanged);
			transport.JW_RL_NKLoadPortInfo.ValueChanged -= new EventHandler(KeyFieldChanged);
			transport.JW_RL_NKDiscPortInfo.ValueChanged -= new EventHandler(KeyFieldChanged);
			transport.JW_LegOrderInfo.ValueChanged -= new EventHandler(KeyFieldChanged);
		}

		void KeyFieldChanged(object sender, EventArgs e)
		{
			UpdateSelectedTransport();
		}

		void HookSelectedTransportValueChangesForValidation(Transport transport)
		{
			transport.JW_RL_NKLoadPortInfo.ValueChanged += MainTransportKeyValueChanged;
			transport.JW_RL_NKDiscPortInfo.ValueChanged += MainTransportKeyValueChanged;
			transport.JW_VesselInfo.ValueChanged += MainTransportKeyValueChanged;
			transport.JW_VoyageFlightInfo.ValueChanged += MainTransportKeyValueChanged;
			transport.JW_ETDInfo.ValueChanged += MainTransportKeyValueChanged;
		}

		void UnhookSelectedTransportValueChangesForValidation(Transport transport)
		{
			if (transport != null)
			{
				transport.JW_RL_NKLoadPortInfo.ValueChanged -= MainTransportKeyValueChanged;
				transport.JW_RL_NKDiscPortInfo.ValueChanged -= MainTransportKeyValueChanged;
				transport.JW_VesselInfo.ValueChanged -= MainTransportKeyValueChanged;
				transport.JW_VoyageFlightInfo.ValueChanged -= MainTransportKeyValueChanged;
				transport.JW_ETDInfo.ValueChanged -= MainTransportKeyValueChanged;
			}
		}

		void MainTransportKeyValueChanged(object sender, EventArgs e)
		{
			MarkConsolAndContainersForContractsValidation();
		}

		void MarkConsolAndContainersForContractsValidation()
		{
			if (IsLoaded && !consol.IsMarkingAsNeedingValidationSuspended)
			{
				consol.MarkAsNeedingValidation();
				consol.Containers.MarkAsNeedingValidation();
			}
		}

		#endregion

		#region Implementation

		void UpdateSelectedTransport()
		{
			Transport oldTransport = (Count > 0 ? this[0] : null);
			Transport newTransport = consol.Transports.MostInterestingTransport;

			if (newTransport != oldTransport)
			{
				UnhookSelectedTransportValueChangesForValidation(oldTransport);
				RemoveAll();
				MarkConsolAndContainersForContractsValidation();

				if (newTransport != null)
				{
					HookSelectedTransportValueChangesForValidation(newTransport);
					Add(newTransport);
				}
			}
		}

		#endregion

		readonly CommonConsol consol;
	}
}
