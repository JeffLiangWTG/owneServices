using System.Collections;
using System.Collections.Generic;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public partial class Transports : DocDataObject, ITransports
	{
		#region PreCarriage

		public Transport PreCarriage
		{
			get => preCarriage;
			set => preCarriage = SetChild(preCarriage, value);
		}

		Transport preCarriage;

		ITransport ITransports.PreCarriage => PreCarriage;

		#endregion

		#region Main

		public Transport Main
		{
			get => main;
			set => main = SetChild(main, value);
		}

		Transport main;

		ITransport ITransports.Main => Main;

		#endregion

		#region OnForwarding

		public Transport OnForwarding
		{
			get => onForwarding;
			set => onForwarding = SetChild(onForwarding, value);
		}

		Transport onForwarding;

		ITransport ITransports.OnForwarding => OnForwarding;

		#endregion

		#region GetEnumerator

		public IEnumerator<ITransport> GetEnumerator() => collection.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		#endregion

		#region Count

		public int Count => collection.Count;

		#endregion

		#region Collection

		public IReadOnlyCollection<Transport> Collection
		{
			get => collection;
			private set => collection = SetChildCollection(collection, value);
		}

		IReadOnlyCollection<Transport> collection;

		#endregion
	}
}
