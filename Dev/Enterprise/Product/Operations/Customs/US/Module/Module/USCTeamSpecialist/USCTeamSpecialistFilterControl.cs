using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.Module
{
	public partial class USCTeamSpecialistFilterControl : ZFilterStripControl
	{
		public USCTeamSpecialistFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
		}
	}
}
