using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Workflow.Module
{
	public partial class EDIMessageDeliveryContextFilterControl : ZFilterStripControl
	{
		public EDIMessageDeliveryContextFilterControl(IBusinessObjectCollection gridCollection, EDIMessageDeliveryContextFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			this.CaptionRenderingEnabled = true;
		}
	}
}
