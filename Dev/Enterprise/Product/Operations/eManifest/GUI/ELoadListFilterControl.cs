
namespace Enterprise.eManifest.GUI
{
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.GUI;

	public partial class ELoadListFilterControl : ZFilterStripControl
	{
		public ELoadListFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
