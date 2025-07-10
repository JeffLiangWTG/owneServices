using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(TrackingEDIMessageColumnProvider))]
	sealed class TrackingEDIMessageColumnProviderTest : GridColumnProviderBaseTest
	{
		protected override GridColumnProvider GetNewTestProvider()
		{
			return new TrackingEDIMessageColumnProvider();
		}
	}
}
