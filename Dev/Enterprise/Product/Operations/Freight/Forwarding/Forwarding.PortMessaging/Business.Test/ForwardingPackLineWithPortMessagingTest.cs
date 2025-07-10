using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business.Testing
{
	[TestedType(typeof(ForwardingPackLineWithPortMessaging))]
	sealed class ForwardingPackLineWithPortMessagingTest : EnterpriseBusinessObjectTestCase
	{
		public void TestNativePacklinePropertiesAreReadonly()
		{
			var packline = Factory.New<ForwardingPackLineWithPortMessaging>();
			AssertEquals(true, packline.JL_ActualVolumeInfo.ReadOnly);
			AssertEquals(true, packline.JL_F3_NKPackTypeInfo.ReadOnly);
		}

		public void TestPortMessaging()
		{
			var packline = Factory.New<ForwardingPackLineWithPortMessaging>();
			AssertNotNull(packline.PortMessaging);
			AssertEquals(false, packline.PortMessaging.JLM_EntryTypeInfo.ReadOnly);
		}

		public void TestPortMessagingWouldBeDeletedTogetherWithPackline()
		{
			var packline = Factory.New<ForwardingPackLineWithPortMessaging>();
			AssertNotNull(packline.PortMessaging);

			packline.Delete();
			Assert(packline.PortMessaging.IsDeleted);
		}
	}
}
