using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Module
{
	public partial class PackLinesFilterControl : ZFilterStripControl
	{
		public PackLinesFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
		: base(gridCollection, filterBusinessObject)
		{
			if (!DesignModeFinder.IsDesigning)
			{
				InitializeComponent();
			}
		}
	}
}
