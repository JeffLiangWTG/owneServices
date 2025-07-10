using System.Collections.Generic;

namespace Enterprise.Freight.Integration
{
	public interface IMultiAllocationRouteSelectorProvider
	{
		void PromptUserForSelectingAllocationRoute(IAllocationRouteAssignable assignable, IReadOnlyCollection<IRatingContractAllocationLine> routes);
	}
}
