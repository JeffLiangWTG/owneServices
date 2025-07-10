using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Workflow.Module
{
	public partial class ValidationRuleFilterControl : ZFilterStripControl
	{
		public ValidationRuleFilterControl(IBusinessObjectCollection gridCollection, ValidationRuleFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			this.CaptionRenderingEnabled = true;
		}
	}
}
