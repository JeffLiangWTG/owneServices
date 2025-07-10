using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class SalesClientRelationshipControl : ZUserControl
	{
		public SalesClientRelationshipControl()
		{
			InitializeComponent();
			if (!DesignModeFinder.IsDesigning)
			{
				SetSecurityReadOnly();
			}
		}

		void SetSecurityReadOnly()
		{
			if (!Environment.Env.Security.ClientIntelligenceModifyClientRelationship.IsAllowed)
			{
				this.CMControllingAgentBoundGuidFindBox.ReadOnly = true;
			}
		}
	}
}
