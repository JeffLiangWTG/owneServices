using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Yard.Module
{
	public partial class CYDReceiveAdviceFilterControl : ZFilterStripControl
	{
		public CYDReceiveAdviceFilterControl()
			: this(null, null)
		{
		}

		public CYDReceiveAdviceFilterControl(IBusinessObjectCollection gridCollection, CYDReceiveAdviceFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			this.filterBusinessObject = filterBusinessObject;

			InitializeComponent();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in CYDReceiveAdviceFilterControl")]
		readonly CYDReceiveAdviceFilterBusinessObject filterBusinessObject;
	}
}
