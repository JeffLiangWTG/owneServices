using System.Collections.Generic;

namespace Enterprise.Warehouse.Cartonisation.Integration
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Will be removed when start using it!")]
	public interface ICartonisation
	{
		IEnumerable<ICartonWithItems> CartoniseItems(IEnumerable<ICartonisableItem> cartonisableItems, IEnumerable<ICartonDefinition> cartons);
	}
}
