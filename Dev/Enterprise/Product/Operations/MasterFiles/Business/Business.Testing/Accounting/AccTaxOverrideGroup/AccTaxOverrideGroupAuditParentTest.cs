using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccTaxOverrideGroup))]
	internal class AccTaxOverrideGroupAuditParentTest : AuditParentTest<AccTaxOverrideGroup>
	{
		protected override AccTaxOverrideGroup NewTestAuditParent()
		{
			return Factory.New<AccTaxOverrideGroup>();
		}
	}
}
