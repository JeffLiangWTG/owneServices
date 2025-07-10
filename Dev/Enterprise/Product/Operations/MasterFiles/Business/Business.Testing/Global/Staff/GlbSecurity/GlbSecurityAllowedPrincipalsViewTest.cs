using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbSecurityAllowedOrgsAndWarehousesView))]
	sealed class GlbSecurityAllowedPrincipalsViewTest : GlbSecurityAllowedOrgsAndWarehousesViewBaseTest
	{
		protected override ZString SecurityRight
		{
			get { return GlbSecurity.AllowedPrincipalsSecurityRightName; }
		}
	}
}
