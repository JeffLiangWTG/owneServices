using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Internal.Testing;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class PortCallLookupControlTest : BaseFindBoxTest
	{
		protected override ZFindBoxUserControl NewFindBoxTester => new PortCallLookupControl();
	}
}
