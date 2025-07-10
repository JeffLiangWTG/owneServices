using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ColoadConsolsShipmentsCollection : BusinessObjectCollection<ForwardingShipment>
	{
		public ColoadConsolsShipmentsCollection(ForwardingConsol masterConsol)
			: base(masterConsol.Factory)
		{
			this.masterConsol = masterConsol;
			Rebuild();
			masterConsol.ColoadConsols.CollectionCountChange += ColoadConsols_CollectionCountChange;
			IsLoaded = true;
		}

		readonly ForwardingConsol masterConsol;
		readonly List<ForwardingConsol> consolsListened = new List<ForwardingConsol>();

		void ColoadConsols_CollectionCountChange(object sender, CollectionCountChangedEventArgs e)
		{
			var consol = e.BizObject as ForwardingConsol;
			if (consol != null)
			{
				var isConsolListened = consolsListened.Contains(e.BizObject);

				if (e.ItemAdded && !isConsolListened)
				{
					StartListening(consol);
				}
				else if (e.ItemRemoved && isConsolListened)
				{
					StopListening(consol);
				}
			}
			else
			{
				Rebuild();
			}
		}

		void StartListening(ForwardingConsol consol)
		{
			consol.Shipments.CountChanged += Shipments_CountChanged;
			consolsListened.Add(consol);

			ActInternally(() => AddRange(consol.Shipments), ref addingInternally);
		}

		void StopListening(ForwardingConsol consol)
		{
			consol.Shipments.CountChanged -= Shipments_CountChanged;
			consolsListened.Remove(consol);

			ActInternally(() => RemoveRange(consol.Shipments), ref removingInternally);
		}

		void Shipments_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			var shipment = e.BizObject as ForwardingShipment;
			if (shipment != null)
			{
				var containsShipment = Contains(shipment);

				if (e.ItemAdded && !containsShipment)
				{
					ActInternally(() => Add(shipment), ref addingInternally);
				}
				else if (e.ItemRemoved && containsShipment)
				{
					ActInternally(() => Remove(shipment), ref removingInternally);
				}
			}
			else
			{
				Rebuild();
			}
		}

		void Rebuild()
		{
			if (consolsListened.Any())
			{
				var listClone = new List<ForwardingConsol>(consolsListened);

				foreach (var consol in listClone)
				{
					StopListening(consol);
				}
			}

			foreach (var consol in masterConsol.ColoadConsols)
			{
				StartListening(consol);
			}
		}

		bool addingInternally;
		bool removingInternally;

		void ActInternally(Action action, ref bool hook)
		{
			try
			{
				hook = true;
				action();
			}
			finally
			{
				hook = false;
			}
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			return ZQuery.NoResultQuery;
		}

		public override void Add(BusinessObject businessObject)
		{
			if (addingInternally)
			{
				base.Add(businessObject);
			}
		}

		public override void Remove(BusinessObject elementToRemove)
		{
			if (removingInternally)
			{
				base.Remove(elementToRemove);
			}
		}

		public override void Load()
		{
			throw new NotSupportedException("This collection is managed manually by listening to other collections - it is not supposed to be Loaded ever.");
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}
	}
}
