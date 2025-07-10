using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Yard.Module
{
	public partial class CYDReleaseAdviceFilterControl : ZFilterStripControl
	{
		public CYDReleaseAdviceFilterControl()
			: this(null, null)
		{
		}

		public CYDReleaseAdviceFilterControl(IBusinessObjectCollection gridCollection, CYDReleaseAdviceFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			this.filterBusinessObject = filterBusinessObject;

			InitializeComponent();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in CYDReleaseAdviceFilterControl")]
		readonly CYDReleaseAdviceFilterBusinessObject filterBusinessObject;
	}
}
