using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.TransportBookings.Document.Testing
{
	sealed class DummyDocDataObjectParameters : IDocDataObjectParameters
	{
		public string DocumentTitle { get; set; }
		public string DataStoreName { get; set; }
		public object Data { get; set; }
		public IStmALogProvider LogProvider { get; set; }
	}
}
