using CargoWise.Common;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OpportunityManagementValueAnalysisControl : ZUserControl
	{
		protected OpportunityManagementValueAnalysisControl()
		{
			InitializeComponent();
		}

		public static OpportunityManagementValueAnalysisControl New()
		{
			var overridden = OverridableNewDelegate.Value;
			return overridden != null ? overridden() : new OpportunityManagementValueAnalysisControl();
		}

		protected delegate OpportunityManagementValueAnalysisControl NewDelegate();

		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();
	}
}
