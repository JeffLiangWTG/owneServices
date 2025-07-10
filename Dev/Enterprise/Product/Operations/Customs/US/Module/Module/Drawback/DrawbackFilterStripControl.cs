using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.Module
{
	public partial class DrawbackFilterStripControl : ZFilterStripControl
	{
		public DrawbackFilterStripControl()
		{
			InitializeComponent();
		}

		public DrawbackFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
		}

		#region Override

		protected override ZBool ShouldSetColorContextKeyFromParentModuleID => true;

		#endregion
	}
}
