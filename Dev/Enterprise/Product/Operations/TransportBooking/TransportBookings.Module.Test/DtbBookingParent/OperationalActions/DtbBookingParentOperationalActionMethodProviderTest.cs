using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.TransportBookings.Shared;
using Moq;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(DtbBookingParentOperationalActionMethodProvider))]
	class DtbBookingParentOperationalActionMethodProviderTest : OperationalActionMethodProviderTest
	{
		public void TestNewMethods()
		{
			var supporter = new Mock<OperationalActionSupporter>();
			supporter.Setup(s => s.RootType).Returns(typeof(IDtbBookingParent));
			var methods = Provider.NewMethods(supporter.Object);
			CombineAssertions("Check methods returned by provider", () =>
			{
				AssertEquals("Provider should have returned 2 methods", 2, methods.Length);
				AssertEquals("Provider returned method should be of correct type", typeof(CreateDtbBookingsFromDtbBookingParentsActionMethod), methods[0].GetType());
				AssertEquals("Provider returned method should be of correct type", typeof(CreateMasterAndSubDtbBookingsFromDtbBookingParentsActionMethod), methods[1].GetType());
			});
		}

		protected override ActionMethodProviderID ID => ActionMethodProviderIDs.DtbBookingParent;
	}
}
