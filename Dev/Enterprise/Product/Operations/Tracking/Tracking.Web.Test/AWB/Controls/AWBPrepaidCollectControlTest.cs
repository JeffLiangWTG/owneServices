using System.Web.UI;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class AWBPrepaidCollectControlTest : AWBBaseControlTest
	{
		protected override Control GetNewControl()
		{
			return new AWBPrepaidCollectControl();
		}

		protected override bool RequiresBindToCheck
		{
			get { return false; }
		}
	}
}
