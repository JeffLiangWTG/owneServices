using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.Freight.Integration
{
	public interface IForwardingDocDataObjectProvider
	{
		object GetDocDataObject(object parent, string dataCotext, IDocDataObjectParameters parameters);
	}
}
