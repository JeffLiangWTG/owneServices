using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class AgencyContainerWorkflowFormForTest : AgencyContainerWorkflowForm
	{
		public AgencyContainerWorkflowFormForTest(AgencyShipmentContainer container) : base(container)
		{
		}

		public ZBool ShowNotesTabForTest
		{
			get
			{
				return ShowNotesTab;
			}
		}

		public TabControl MainTabControlForTest
		{
			get
			{
				return MainTabControl;
			}
		}

		public TabPage MainTabPageForTest
		{
			get
			{
				return MainTabPage;
			}
		}
	}
}
