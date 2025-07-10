using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(WhsTrackingInventorySummaryItemView))]
	sealed class TrackingInventorySummaryItemViewTest : EnterpriseBusinessObjectTestCase
	{
		protected override bool IsDeleteSupported() => false;

		protected override bool CanPersistedObjectBeDeleted => false;
	}
}
