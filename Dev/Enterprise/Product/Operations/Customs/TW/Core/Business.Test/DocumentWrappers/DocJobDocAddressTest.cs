using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(DocJobDocAddress))]
	sealed class DocJobDocAddressTest : DocBaseWrapperTest
	{
		public void TestFax()
		{
			jobDocAddress.E2_Fax = "0226778945";
			AssertEquals("0226778945", DocJobDocAddressWrapper.Fax);
		}

		public void TestPhone()
		{
			jobDocAddress.E2_Phone = "0226771234";
			AssertEquals("0226771234", DocJobDocAddressWrapper.Phone);
		}

		public void TestEmail()
		{
			jobDocAddress.E2_Email = "wps234@gmail.com";
			AssertEquals("wps234@gmail.com", DocJobDocAddressWrapper.Email);
		}

		public void TestContact()
		{
			jobDocAddress.E2_Contact = "Tom Chen";
			AssertEquals("Tom Chen", DocJobDocAddressWrapper.Contact);
		}

		protected override void SetUp()
		{
			base.SetUp();
			jobDocAddress = Factory.NewWithValidTestData<TWJobDocAddress>();
		}

		protected override DocBaseWrapper GetNewDocumentWrapper() => DocJobDocAddressWrapper;

		DocJobDocAddress DocJobDocAddressWrapper => DocJobDocAddress.New(jobDocAddress, Factory);

		TWJobDocAddress jobDocAddress;
	}
}
