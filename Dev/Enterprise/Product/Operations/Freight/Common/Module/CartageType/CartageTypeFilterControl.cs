using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Common.Module
{
	public partial class LocalCartageJobTypeFilterControl : ZFilterStripControl
	{
		public LocalCartageJobTypeFilterControl() : this(null, null)
		{
		}

		public LocalCartageJobTypeFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
