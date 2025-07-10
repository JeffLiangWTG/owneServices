using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class VoyagePortCollection : NonPersistentBusinessObjectCollection<VoyagePort>, IDisposable
	{
		public VoyagePortCollection(JobVoyage voyage)
			: base(voyage.Factory)
		{
			this.voyage = voyage;
			Rebuild();
			voyage.Origins.CountChanged -= OnOriginsCountChanged;
			voyage.Origins.CountChanged += OnOriginsCountChanged;
			voyage.Destinations.CountChanged -= OnOriginsCountChanged;
			voyage.Destinations.CountChanged += OnOriginsCountChanged;
		}
		readonly JobVoyage voyage;

		void OnOriginsCountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			var origin = e.BizObject as VoyageOrigin;
			var destination = e.BizObject as VoyageDestination;
			if (e.ItemRemoved)
			{
				if (origin != null)
				{
					origin.JA_RL_NKPortOfLoadingInfo.ValueChanged -= OnEventsThatNeedRebuilding;
				}
				if (destination != null)
				{
					destination.JB_RL_NKPortOfDischargeInfo.ValueChanged -= OnEventsThatNeedRebuilding;
				}
			}
			else if (e.ItemAdded)
			{
				if (origin != null)
				{
					origin.JA_RL_NKPortOfLoadingInfo.ValueChanged -= OnEventsThatNeedRebuilding;
					origin.JA_RL_NKPortOfLoadingInfo.ValueChanged += OnEventsThatNeedRebuilding;
				}
				if (destination != null)
				{
					destination.JB_RL_NKPortOfDischargeInfo.ValueChanged -= OnEventsThatNeedRebuilding;
					destination.JB_RL_NKPortOfDischargeInfo.ValueChanged += OnEventsThatNeedRebuilding;
				}
			}
			Rebuild();
		}

		void OnEventsThatNeedRebuilding(object sender, EventArgs e)
		{
			Rebuild();
		}

		void Rebuild()
		{
			RemoveAllButLeaveRelationshipsIntact();
			foreach (VoyageOrigin origin in voyage.Origins)
			{
				origin.JA_RL_NKPortOfLoadingInfo.ValueChanged -= OnEventsThatNeedRebuilding;
				origin.JA_RL_NKPortOfLoadingInfo.ValueChanged += OnEventsThatNeedRebuilding;
				AddIfNotExist(new VoyagePort(voyage, origin.JA_RL_NKPortOfLoading));
			}

			foreach (VoyageDestination destination in voyage.Destinations)
			{
				destination.JB_RL_NKPortOfDischargeInfo.ValueChanged -= OnEventsThatNeedRebuilding;
				destination.JB_RL_NKPortOfDischargeInfo.ValueChanged += OnEventsThatNeedRebuilding;
				AddIfNotExist(new VoyagePort(voyage, destination.JB_RL_NKPortOfDischarge));
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new VoyagePort(voyage, ZString.Empty);
		}

		void AddIfNotExist(VoyagePort port)
		{
			if (!this.Cast<VoyagePort>().Any(x => x.Port == port.Port))
			{
				Add(port);
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public void Dispose()
		{
			voyage.Origins.CountChanged -= OnOriginsCountChanged;
			voyage.Destinations.CountChanged -= OnOriginsCountChanged;
			foreach (VoyageOrigin origin in voyage.Origins)
			{
				origin.JA_RL_NKPortOfLoadingInfo.ValueChanged -= OnEventsThatNeedRebuilding;
			}

			foreach (VoyageDestination destination in voyage.Destinations)
			{
				destination.JB_RL_NKPortOfDischargeInfo.ValueChanged -= OnEventsThatNeedRebuilding;
			}
		}
	}
}
