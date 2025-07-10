using System.Collections;
using System.Collections.Generic;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class DummyTransports : ITransports
	{
		public List<ITransport> Elements => elements ?? (elements = new List<ITransport>());
		List<ITransport> elements;

		public ITransport PreCarriage { get; set; }
		public ITransport Main { get; set; }
		public ITransport OnForwarding { get; set; }

		public int Count => Elements.Count;

		public IEnumerator<ITransport> GetEnumerator() => Elements.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => Elements.GetEnumerator();
	}
}
