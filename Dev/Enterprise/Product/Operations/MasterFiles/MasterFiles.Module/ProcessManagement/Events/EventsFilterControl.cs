
namespace Enterprise.MasterFiles.Module
{
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.GUI;

	public partial class EventsFilterControl : ZFilterStripControl
	{
		public EventsFilterControl()
		{
			InitializeComponent();
		}

		public EventsFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}