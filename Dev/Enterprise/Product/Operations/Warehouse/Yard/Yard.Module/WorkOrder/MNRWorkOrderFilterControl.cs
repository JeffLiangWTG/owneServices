using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Yard.Module
{
	public partial class MNRWorkOrderFilterControl : ZFilterStripControl
	{
		public MNRWorkOrderFilterControl()
			: this(null, null)
		{
		}

		public MNRWorkOrderFilterControl(IBusinessObjectCollection gridCollection, MNRWorkOrderFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			this.filterBusinessObject = filterBusinessObject;

			InitializeComponent();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in MNRWorkOrderFilterControl")]
		readonly MNRWorkOrderFilterBusinessObject filterBusinessObject;
	}
}
