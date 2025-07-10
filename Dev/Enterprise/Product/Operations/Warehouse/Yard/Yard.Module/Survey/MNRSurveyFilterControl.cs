using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Yard.Module
{
	public partial class MNRSurveyFilterControl : ZFilterStripControl
	{
		public MNRSurveyFilterControl()
			: this(null, null)
		{
		}

		public MNRSurveyFilterControl(IBusinessObjectCollection gridCollection, MNRSurveyFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			this.filterBusinessObject = filterBusinessObject;

			InitializeComponent();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in MNRSurveyFilterControl")]
		readonly MNRSurveyFilterBusinessObject filterBusinessObject;
	}
}
