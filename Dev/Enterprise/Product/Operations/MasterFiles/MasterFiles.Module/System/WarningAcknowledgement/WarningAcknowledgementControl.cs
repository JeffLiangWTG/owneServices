using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class WarningAcknowledgementControl : ZFilterStripControl
	{
		public WarningAcknowledgementControl(IBusinessObjectCollection gridCollection, WarningAcknowledgementFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
