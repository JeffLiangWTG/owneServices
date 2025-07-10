using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class LazyGlbEmailAddressTest : TestCaseWithFactory
	{
		public void TestGI_DeliveryReportTimeUtc()
		{
			AssertNull("Precondition", GlbEmailAddress.Load(Factory, "aaa@a.com"));

			var lazyEmailAddress = new LazyGlbEmailAddress(Factory, () => "aaa@a.com");
			AssertEquals(ZDateTime.Empty, lazyEmailAddress.GI_DeliveryReportTimeUtc);
			AssertNull("Should not create GlbEmailAddress", GlbEmailAddress.Load(Factory, "aaa@a.com"));

			lazyEmailAddress.GI_DeliveryReportTimeUtc = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, lazyEmailAddress.GI_DeliveryReportTimeUtc);
			AssertNull("Should not create GlbEmailAddress", GlbEmailAddress.Load(Factory, "aaa@a.com"));

			lazyEmailAddress.GI_DeliveryReportTimeUtc = new ZDateTime(2002, 2, 2);
			AssertEquals(new ZDateTime(2002, 2, 2), lazyEmailAddress.GI_DeliveryReportTimeUtc);
			var emailAddress = GlbEmailAddress.Load(Factory, "aaa@a.com");
			AssertNotNull("Should have created GlbEmailAddress", emailAddress);
			AssertEquals("aaa@a.com", emailAddress.GI_EmailAddress);
			AssertEquals(new ZDateTime(2002, 2, 2), emailAddress.GI_DeliveryReportTimeUtc);
		}

		[TestDate(2002, 2, 2)]
		public void TestGI_DeliveryStatus()
		{
			AssertNull("Precondition", GlbEmailAddress.Load(Factory, "aaa@a.com"));

			var lazyEmailAddress = new LazyGlbEmailAddress(Factory, () => "aaa@a.com");
			AssertEquals(ZString.Empty, lazyEmailAddress.GI_DeliveryStatus);
			AssertNull("Should not create GlbEmailAddress", GlbEmailAddress.Load(Factory, "aaa@a.com"));

			lazyEmailAddress.GI_DeliveryStatus = ZString.Empty;
			AssertEquals(ZString.Empty, lazyEmailAddress.GI_DeliveryStatus);
			AssertNull("Should not create GlbEmailAddress", GlbEmailAddress.Load(Factory, "aaa@a.com"));

			lazyEmailAddress.GI_DeliveryStatus = "XXX";
			AssertEquals("XXX", lazyEmailAddress.GI_DeliveryStatus);
			var emailAddress = GlbEmailAddress.Load(Factory, "aaa@a.com");
			AssertNotNull("Should have created GlbEmailAddress", emailAddress);
			AssertEquals("aaa@a.com", emailAddress.GI_EmailAddress);
			AssertEquals("XXX", emailAddress.GI_DeliveryStatus);
			AssertEquals(new ZDateTime(2002, 2, 2), emailAddress.GI_DeliveryReportTimeUtc);
		}
	}
}
