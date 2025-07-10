using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Common.Business.Testing
{
	[TestedType(typeof(ViewJobCartageParents))]
	sealed class ViewJobCartageParentsTest : EnterpriseBusinessObjectTestCase
	{
		protected override bool IsDeleteSupported() => false;

		protected override bool CanPersistedObjectBeDeleted => false;
	}
}
