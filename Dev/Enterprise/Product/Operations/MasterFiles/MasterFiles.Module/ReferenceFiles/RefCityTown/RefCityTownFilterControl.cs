using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class RefCityTownFilterControl : ZFilterStripControl
	{
		public RefCityTownFilterControl()
		{
			InitializeComponent();
		}

		public RefCityTownFilterControl(IBusinessObjectCollection gridCollection, RefCityTownFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new RefCityTownFilterStrip();
		}
	}
}
