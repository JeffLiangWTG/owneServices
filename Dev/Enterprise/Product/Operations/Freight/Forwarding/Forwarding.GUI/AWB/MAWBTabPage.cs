using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public class MAWBTabPage : AWBTabPage
	{
		public ZTabPage ParentAWBTabPage { get; set; }

		protected override AWBUserControl NewUserControl()
		{
			return new MAWBUserControl();
		}

		protected override void SwitchAWBTabPageVisibility(ZBool value)
		{
			ParentAWBTabPage.TabVisible = value;
			ParentAWBTabPage.TabRelevant = value;
		}
	}
}
