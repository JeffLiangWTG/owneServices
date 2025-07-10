using System.Collections.Generic;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Integration;

namespace Enterprise.Freight.Integration
{
	public interface IRelatedAgentsPivotCollection<out T> : IPivotBusinessObjectCollection
	where T : IPivotBusinessObject, IAllocationRouteAgentPivot
	{
		public IReadOnlyCollection<IOrgHeader> GetAllAgents();
		new T this[int i] { get; }
	}
}
