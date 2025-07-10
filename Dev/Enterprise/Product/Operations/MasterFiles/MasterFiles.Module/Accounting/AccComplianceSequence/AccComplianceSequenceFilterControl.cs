using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class AccComplianceSequenceFilterControl : ZFilterStripControl
	{
		protected override ZFilterStrip NewZFilterStrip()
		{
			return new ZFilterStrip();
		}

		public AccComplianceSequenceFilterControl(IBusinessObjectCollection gridCollection, AccComplianceSequenceFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
