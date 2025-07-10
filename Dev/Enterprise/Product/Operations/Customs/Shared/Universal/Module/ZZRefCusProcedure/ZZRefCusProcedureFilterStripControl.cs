using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Universal.Module
{
	partial class ZZRefCusProcedureFilterStripControl : ZFilterStripControl
	{
		public ZZRefCusProcedureFilterStripControl()
		{
			InitializeComponent();
		}

		public ZZRefCusProcedureFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
