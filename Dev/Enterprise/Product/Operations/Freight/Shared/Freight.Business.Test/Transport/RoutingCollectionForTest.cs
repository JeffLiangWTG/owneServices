namespace Enterprise.Freight.Business.Testing
{
	sealed class RoutingCollectionForTest : RoutingCollection
	{
		public RoutingCollectionForTest(ITransportParent mainTransportParent)
			: base(mainTransportParent)
		{
		}

		public new void MonitorCollection(TransportCollection collection)
		{
			base.MonitorCollection(collection);
		}

		public new void UnMonitorCollection(TransportCollection collection)
		{
			base.UnMonitorCollection(collection);
		}
	}
}
