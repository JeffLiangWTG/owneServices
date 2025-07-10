using System.Web.UI;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class AWBFindBoxTest : AWBFieldControlTest
	{
		protected override Control GetNewControl()
		{
			return new AWBFindBox();
		}
	}
}
