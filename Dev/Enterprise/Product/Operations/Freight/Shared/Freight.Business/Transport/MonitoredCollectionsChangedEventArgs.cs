using System;

namespace Enterprise.Freight.Business
{
	public class MonitoredCollectionsChangedEventArgs : EventArgs
	{
		public MonitoredCollectionsChangedEventArgs(TransportCollection collection, bool collectionAdded)
		{
			this.collection = collection;
			this.collectionAdded = collectionAdded;
		}

		public TransportCollection Collection
		{
			get { return collection; }
		}

		public bool CollectionAdded
		{
			get { return collectionAdded; }
		}

		public bool CollectionRemoved
		{
			get { return !collectionAdded; }
		}

		readonly TransportCollection collection;
		readonly bool collectionAdded;
	}
}
