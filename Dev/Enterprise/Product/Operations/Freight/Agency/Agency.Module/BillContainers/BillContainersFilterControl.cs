using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.GUI;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.Module
{
	public partial class BillContainersFilterControl : ZFilterStripControl
	{
		public BillContainersFilterControl(IBusinessObjectCollection collection, BillContainersFilterStrip filterBizo)
			: base(collection, filterBizo)
		{
			InitializeComponent();

			importReleaseOrderStatusStyleInfo.CaptionResourceString = CommonContainer.ImportReleaseOrderStatusStringData;
			importReleaseNumberColumnStyleInfo.CaptionResourceString = CommonContainer.ImportReleaseNumberStringData;
		}

		protected override void OnLoad(System.EventArgs e)
		{
			if (!this.DesignMode)
			{
				ContainerCustomColumnAdder.Set(this.FilteredGrid, ContainerCustomColumnAdder.TargetGridType.Module);
			}

			base.OnLoad(e);
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new CustomModuleFilterControlKludge();
		}
	}
}
