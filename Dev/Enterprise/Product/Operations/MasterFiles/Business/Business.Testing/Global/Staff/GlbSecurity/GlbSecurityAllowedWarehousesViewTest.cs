using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbSecurityAllowedOrgsAndWarehousesView))]
	sealed class GlbSecurityAllowedWarehousesViewTest : GlbSecurityAllowedOrgsAndWarehousesViewBaseTest
	{
		protected override ZString SecurityRight
		{
			get { return GlbSecurity.AllowedWarehousesSecurityRightName; }
		}
	}
}
