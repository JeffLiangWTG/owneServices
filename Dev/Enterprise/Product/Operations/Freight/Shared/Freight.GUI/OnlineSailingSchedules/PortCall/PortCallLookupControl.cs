using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.OnlineSailingSchedules.PortCall;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Freight.GUI
{
	public class PortCallLookupControl : ZGridFindBox
	{
		protected override IFindBoxPopup GetNewPopupForm()
		{
			PortCallRequestForm result = null;
			var grid = Parent.Parent as ZGrid;

			if (grid?.DataSource != null)
			{
				if (grid.DataSource is IBusiness bo)
				{
					var manager = new PortCallManager(bo.Factory);
					var dataSource = BindingContext[grid.DataSource, grid.DataMember].GetCurrent();

					if (dataSource is VoyageOrigin origin)
					{
						manager.SetupRequest(origin.JA_RL_NKPortOfLoading, origin.JA_E_DEP, origin.Voyage, PortCallRequestType.Load);
					}
					else
					{
						if (dataSource is VoyageDestination destination)
						{
							manager.SetupRequest(destination.JB_RL_NKPortOfDischarge, destination.JB_E_ARV, destination.Voyage, PortCallRequestType.Discharge);
						}
					}

					result = new PortCallRequestForm(manager);
				}
			}

			return result;
		}
	}
}
