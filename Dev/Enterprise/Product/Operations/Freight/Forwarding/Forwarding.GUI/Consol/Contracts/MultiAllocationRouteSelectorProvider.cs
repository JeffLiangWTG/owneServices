using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ContractManagement.Business;
using Enterprise.ContractManagement.GUI;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class MultiAllocationRouteSelectorProvider : IMultiAllocationRouteSelectorProvider
	{
		public static void Register(ZForm form)
		{
			if (form != null && form.BusinessEntity != null)
			{
				form.BusinessEntity.Factory.SetValue<IMultiAllocationRouteSelectorProvider, MultiAllocationRouteSelectorProvider>();
			}
		}

		void IMultiAllocationRouteSelectorProvider.PromptUserForSelectingAllocationRoute(IAllocationRouteAssignable routeAssignable, IReadOnlyCollection<IRatingContractAllocationLine> routes)
		{
			var viewManager = new ViewMultiAllocationSelectionManager(routeAssignable.Factory, routes.OfType<RatingContractAllocationLine>().ToArray());
			SelectAllocationRouteForm.ShowDialog(routeAssignable, viewManager);
		}
	}
}
