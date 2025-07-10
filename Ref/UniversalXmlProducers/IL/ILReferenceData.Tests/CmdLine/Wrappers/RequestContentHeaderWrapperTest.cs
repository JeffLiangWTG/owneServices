using System;
using CargoWise.RefDbRepo.ILReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ILReferenceData.Tests.CmdLine
{
	[TestFixture]
	sealed class RequestContentHeaderWrapperTest
	{
		[Test]
		public void TestTransmitionDateTime()
		{
			Assert.AreEqual(new DateTime(2024, 06, 04), requestContentHeaderWrapper.TransmitionDateTime);
		}

		[Test]
		public void TestRecieverID()
		{
			Assert.AreEqual(ApplicationConfig.Instance.RecieverID, requestContentHeaderWrapper.RecieverID);
		}

		[Test]
		public void TestSenderID()
		{
			Assert.AreEqual(ApplicationConfig.Instance.SenderID, requestContentHeaderWrapper.SenderID);
		}

		[SetUp]
		public void SetUp()
		{
			requestContentHeaderWrapper = new RequestContentHeaderWrapper(new DateTime(2024, 06, 04));
		}

		IRequestContentHeader requestContentHeaderWrapper;
	}
}
