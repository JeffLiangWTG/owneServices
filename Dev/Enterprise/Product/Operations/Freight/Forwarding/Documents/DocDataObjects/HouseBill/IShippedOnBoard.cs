using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public interface IShippedOnBoard : ICodeDescription
	{
		ZDateTime Date { get; }
	}
}
