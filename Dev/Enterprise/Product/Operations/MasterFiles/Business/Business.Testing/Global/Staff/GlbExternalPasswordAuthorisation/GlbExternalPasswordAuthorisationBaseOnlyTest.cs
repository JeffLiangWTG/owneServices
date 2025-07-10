using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbExternalPasswordAuthorisation))]
	sealed class GlbExternalPasswordAuthorisationBaseOnlyTest : GlbExternalPasswordAuthorisationTest<GlbExternalPasswordAuthorisation, GlbExternalPassword>
	{
	}
}
