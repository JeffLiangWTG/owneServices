using System.Collections;
using System.Collections.Generic;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed partial class Transports : DocDataObject, ITransports
	{
		#region PreCarriage

		public Transport PreCarriage
		{
			get => preCarriage;
			set => preCarriage = SetChild(preCarriage, value);
		}

		Transport preCarriage;

		#endregion

		#region Main

		public Transport Main
		{
			get => main;
			set => main = SetChild(main, value);
		}

		Transport main;

		#endregion

		#region OnForwarding

		public Transport OnForwarding
		{
			get => onForwarding;
			set => onForwarding = SetChild(onForwarding, value);
		}

		Transport onForwarding;

		#endregion

		#region ITransports

		public IEnumerator<ITransport> GetEnumerator() => collection.GetEnumerator();

		public int Count => collection.Count;

		public IReadOnlyCollection<Transport> Collection
		{
			get => collection;
			private set => collection = SetChildCollection(collection, value);
		}

		IReadOnlyCollection<Transport> collection;

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		ITransport ITransports.PreCarriage => PreCarriage;
		ITransport ITransports.Main => Main;
		ITransport ITransports.OnForwarding => OnForwarding;

		#endregion
	}
}
