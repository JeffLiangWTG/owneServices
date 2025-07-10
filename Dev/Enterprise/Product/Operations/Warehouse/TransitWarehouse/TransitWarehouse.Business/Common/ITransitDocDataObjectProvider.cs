using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.Warehouse.Transit.Business
{
	public interface ITransitDocDataObjectProvider
	{
		object GetDocDataObject(object parent, string dataCotext, IDocDataObjectParameters parameters);
	}
}
