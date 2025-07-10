using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ContractManagement.GUI
{
	public partial class ContractAndAllocationsGridPanel : ZUserControl
	{
		public ContractAndAllocationsGridPanel()
		{
			InitializeComponent();
		}

		public ZGrid AllocationRouteGrid => AllocationRoutesGrid;
		public ZFilterGrid ContractFilterGrid => CarrierContractsFilterGrid;
	}
}
