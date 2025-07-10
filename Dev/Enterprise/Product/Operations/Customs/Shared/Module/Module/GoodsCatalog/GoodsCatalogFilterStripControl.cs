using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Module
{
	public partial class GoodsCatalogFilterStripControl : ZFilterStripControl
	{
		public GoodsCatalogFilterStripControl()
		{
			InitializeComponent();
		}

		public GoodsCatalogFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
