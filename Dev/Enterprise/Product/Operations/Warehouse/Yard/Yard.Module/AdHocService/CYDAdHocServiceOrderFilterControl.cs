using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Yard.Module
{
	public partial class CYDAdHocServiceOrderFilterControl : ZFilterStripControl
	{
		public CYDAdHocServiceOrderFilterControl()
			: this(null, null)
		{
		}

		public CYDAdHocServiceOrderFilterControl(IBusinessObjectCollection gridCollection, CYDAdHocServiceOrderFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			this.filterBusinessObject = filterBusinessObject;

			InitializeComponent();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in CYDAdHocServiceOrderFilterControl")]
		readonly CYDAdHocServiceOrderFilterBusinessObject filterBusinessObject;
	}
}
