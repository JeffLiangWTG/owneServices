using System;
using System.IO;
using System.Net;
using System.Text;
using Moq;
using NUnit.Framework;

#if NET6_0_OR_GREATER
using Microsoft.Extensions.Configuration;
#endif

namespace CargoWise.eServices.Authentication.ServiceClient.Tests
{
	[TestFixture]
	public class AuthWebServiceTests
	{
		private Mock<AuthWebServiceApi> client;

		[SetUp]
		public void SetUp()
		{
#if NET6_0_OR_GREATER
			var configSection = new Mock<IConfigurationSection>();
			configSection.SetupGet(_ => _["EndPoint"]).Returns("TestEndpoint");
			client = new Mock<AuthWebServiceApi>(configSection.Object);
#else
			client = new Mock<AuthWebServiceApi>();
#endif
		}

		[Test]
		public void TestCheckSystemExistenceBySystemID()
		{
			var timeMock = new Mock<DateTimeWrapper>();

			client.Setup(_ => _.DateTimeWrapper).Returns(timeMock.Object);
			client.Object.ClearCaches();

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 0, 0)); // called 1 time
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s => s.Equals("\"ABC\"")))).Returns(RequestOK());
			Assert.AreEqual(client.Object.ExistenceCachesCount, 0);
			Assert.IsTrue(client.Object.CheckSystemExistence("ABC"));
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 0, 0)); // called 1 time
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s => s.Equals("\"BCD\"")))).Returns(RequestUnauthorized());
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);
			Assert.IsFalse(client.Object.CheckSystemExistence("BCD"));
			Assert.AreEqual(client.Object.ExistenceCachesCount, 2);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 10, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s => s.Equals("\"ABC\"")))).Returns(RequestUnauthorized());
			Assert.AreEqual(client.Object.ExistenceCachesCount, 2);
			Assert.IsFalse(client.Object.CheckSystemExistence("ABC"));
			Assert.AreEqual(client.Object.ExistenceCachesCount, 2);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 10, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s => s.Equals("\"BCD\"")))).Returns(RequestOK());
			Assert.AreEqual(client.Object.ExistenceCachesCount, 2);
			Assert.IsTrue(client.Object.CheckSystemExistence("BCD"));
			Assert.AreEqual(client.Object.ExistenceCachesCount, 2);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 10, 0)); // called 1 time
			Assert.AreEqual(client.Object.ExistenceCachesCount, 2);
			Assert.IsTrue(client.Object.CheckSystemExistence("BCD"));
			Assert.AreEqual(client.Object.ExistenceCachesCount, 2);

			timeMock.VerifyGet(_ => _.UtcNow, Times.Exactly(11));
		}

		[Test]
		public void TestCheckSystemExistenceByCode()
		{
			var timeMock = new Mock<DateTimeWrapper>();

			client.Setup(_ => _.DateTimeWrapper).Returns(timeMock.Object);
			client.Object.ClearCaches();

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 0, 0)); // called 1 time
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\"]")))).Returns(RequestOK());
			Assert.AreEqual(client.Object.ExistenceCachesCount, 0);
			Assert.IsTrue(client.Object.CheckSystemExistence("ABC", "123"));
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 0, 0)); // called 1 time
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"BCD\",\"234\"]")))).Returns(RequestUnauthorized());
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);
			Assert.IsFalse(client.Object.CheckSystemExistence("BCD", "234"));
			Assert.AreEqual(client.Object.ExistenceCachesCount, 2);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 10, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\"]")))).Returns(RequestUnauthorized());
			Assert.AreEqual(client.Object.ExistenceCachesCount, 2);
			Assert.IsFalse(client.Object.CheckSystemExistence("ABC", "123"));
			Assert.AreEqual(client.Object.ExistenceCachesCount, 2);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 10, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"BCD\",\"234\"]")))).Returns(RequestOK());
			Assert.AreEqual(client.Object.ExistenceCachesCount, 2);
			Assert.IsTrue(client.Object.CheckSystemExistence("BCD", "234"));
			Assert.AreEqual(client.Object.ExistenceCachesCount, 2);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 10, 0)); // called 1 time
			Assert.AreEqual(client.Object.ExistenceCachesCount, 2);
			Assert.IsTrue(client.Object.CheckSystemExistence("BCD", "234"));
			Assert.AreEqual(client.Object.ExistenceCachesCount, 2);

			timeMock.VerifyGet(_ => _.UtcNow, Times.Exactly(11));
		}

		[Test]
		public void TestValidateSystemBySystemID()
		{
			var timeMock = new Mock<DateTimeWrapper>();

			client.Setup(_ => _.DateTimeWrapper).Returns(timeMock.Object);
			client.Object.ClearCaches();

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 0, 0)); // called 1 time
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"password\"]")))).Returns(RequestOK());
			Assert.AreEqual(client.Object.ValidationCachesCount, 0);
			Assert.IsTrue(client.Object.ValidateSystem("ABC", "password"));
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 0, 0)); // called 1 time
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"BCD\",\"password\"]")))).Returns(RequestUnauthorized());
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsFalse(client.Object.ValidateSystem("BCD", "password"));
			Assert.AreEqual(client.Object.ValidationCachesCount, 2);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 10, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"password\"]")))).Returns(RequestUnauthorized());
			Assert.AreEqual(client.Object.ValidationCachesCount, 2);
			Assert.IsFalse(client.Object.ValidateSystem("ABC", "password"));
			Assert.AreEqual(client.Object.ValidationCachesCount, 2);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 10, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"BCD\",\"password\"]")))).Returns(RequestOK());
			Assert.AreEqual(client.Object.ValidationCachesCount, 2);
			Assert.IsTrue(client.Object.ValidateSystem("BCD", "password"));
			Assert.AreEqual(client.Object.ValidationCachesCount, 2);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 10, 0)); // called 3 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"BCD\",\"password2\"]")))).Returns(RequestUnauthorized());
			Assert.AreEqual(client.Object.ValidationCachesCount, 2);
			Assert.IsFalse(client.Object.ValidateSystem("BCD", "password2"));
			Assert.AreEqual(client.Object.ValidationCachesCount, 2);

			timeMock.VerifyGet(_ => _.UtcNow, Times.Exactly(13));
		}

		[Test]
		public void TestValidateSystemByCode()
		{
			var timeMock = new Mock<DateTimeWrapper>();
			
			client.Setup(_ => _.DateTimeWrapper).Returns(timeMock.Object);
			client.Object.ClearCaches();

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 0, 0)); // called 1 time
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\",\"password\"]")))).Returns(RequestOK());
			Assert.AreEqual(client.Object.ValidationCachesCount, 0);
			Assert.IsTrue(client.Object.ValidateSystem("ABC", "123", "password"));
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 0, 0)); // called 1 time
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"BCD\",\"234\",\"password\"]")))).Returns(RequestUnauthorized());
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsFalse(client.Object.ValidateSystem("BCD", "234", "password"));
			Assert.AreEqual(client.Object.ValidationCachesCount, 2);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 10, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\",\"password\"]")))).Returns(RequestUnauthorized());
			Assert.AreEqual(client.Object.ValidationCachesCount, 2);
			Assert.IsFalse(client.Object.ValidateSystem("ABC", "123", "password"));
			Assert.AreEqual(client.Object.ValidationCachesCount, 2);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 10, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"BCD\",\"234\",\"password\"]")))).Returns(RequestOK());
			Assert.AreEqual(client.Object.ValidationCachesCount, 2);
			Assert.IsTrue(client.Object.ValidateSystem("BCD", "234", "password"));
			Assert.AreEqual(client.Object.ValidationCachesCount, 2);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 10, 0)); // called 3 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"BCD\",\"234\",\"password2\"]")))).Returns(RequestUnauthorized());
			Assert.AreEqual(client.Object.ValidationCachesCount, 2);
			Assert.IsFalse(client.Object.ValidateSystem("BCD", "234", "password2"));
			Assert.AreEqual(client.Object.ValidationCachesCount, 2);

			timeMock.VerifyGet(_ => _.UtcNow, Times.Exactly(13));
		}

		#region ValidateSystemByCode with no cache on working web service

		[Test]
		public void TestValidateSystemByCode_Ok()
		{
			var timeMock = new Mock<DateTimeWrapper>();

			client.Setup(_ => _.DateTimeWrapper).Returns(timeMock.Object);
			client.Object.ClearCaches();

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 0, 0)); // called 1 time
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\",\"password\"]")))).Returns(RequestOK());
			Assert.AreEqual(client.Object.ValidationCachesCount, 0);
			Assert.IsTrue(client.Object.ValidateSystem("ABC", "123", "password"), "New record is inserted into cache with true");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.VerifyGet(_ => _.UtcNow, Times.Once);
		}

		[Test]
		public void TestValidateSystemByCode_Found()
		{
			var timeMock = new Mock<DateTimeWrapper>();

			client.Setup(_ => _.DateTimeWrapper).Returns(timeMock.Object);
			client.Object.ClearCaches();

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 0, 0)); // called 1 time
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\",\"password\"]")))).Returns(RequestFound());
			Assert.AreEqual(client.Object.ValidationCachesCount, 0);
			Assert.IsTrue(client.Object.ValidateSystem("ABC", "123", "password"), "New record is inserted into cache with true");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.VerifyGet(_ => _.UtcNow, Times.Once);
		}

		[Test]
		public void TestValidateSystemByCode_Unauthorized()
		{
			var timeMock = new Mock<DateTimeWrapper>();

			client.Setup(_ => _.DateTimeWrapper).Returns(timeMock.Object);
			client.Object.ClearCaches();

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 0, 0)); // called 1 time
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\",\"password\"]")))).Returns(RequestUnauthorized());
			Assert.AreEqual(client.Object.ValidationCachesCount, 0);
			Assert.IsFalse(client.Object.ValidateSystem("ABC", "123", "password"), "New record is inserted into cache with FALSE");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.VerifyGet(_ => _.UtcNow, Times.Once);
		}

		#endregion

		#region ValidateSystemByCode with no cache on bad web service

		[Test]
		public void TestValidateSystemByCodeFails_WebException_Success()
		{
			var timeMock = new Mock<DateTimeWrapper>();

			client.Setup(_ => _.DateTimeWrapper).Returns(timeMock.Object);
			client.Object.ClearCaches();

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 0, 0)); // called 1 time
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\",\"password\"]")))).Returns(WebException_Success());
			Assert.AreEqual(client.Object.ValidationCachesCount, 0);
			Assert.IsFalse(client.Object.ValidateSystem("ABC", "123", "password"), "New record is inserted into cache with false");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.VerifyGet(_ => _.UtcNow, Times.Once);
		}

		[Test]
		public void TestValidateSystemByCodeFails_WebException_Timeout()
		{
			var timeMock = new Mock<DateTimeWrapper>();

			client.Setup(_ => _.DateTimeWrapper).Returns(timeMock.Object);
			client.Object.ClearCaches();

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 0, 0)); // called 1 time
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\",\"password\"]")))).Returns(WebException_Timeout());
			Assert.AreEqual(client.Object.ValidationCachesCount, 0);
			Assert.IsFalse(client.Object.ValidateSystem("ABC", "123", "password"), "New record is inserted into cache with false");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.VerifyGet(_ => _.UtcNow, Times.Once);
		}

		[Test]
		public void TestValidateSystemByCodeFails_WebException_SendFailure_WithResponseFound()
		{
			var timeMock = new Mock<DateTimeWrapper>();

			client.Setup(_ => _.DateTimeWrapper).Returns(timeMock.Object);
			client.Object.ClearCaches();

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 0, 0)); // called 1 time
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\",\"password\"]")))).Returns(WebException_SendFailure_WithResponseFound());
			Assert.AreEqual(client.Object.ValidationCachesCount, 0);
			Assert.IsTrue(client.Object.ValidateSystem("ABC", "123", "password"), "******New record is inserted into cache with TRUE******");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.VerifyGet(_ => _.UtcNow, Times.Once);
		}

		[Test]
		public void TestValidateSystemByCodeFails_InvalidOperationException_Timeout()
		{
			var timeMock = new Mock<DateTimeWrapper>();

			client.Setup(_ => _.DateTimeWrapper).Returns(timeMock.Object);
			client.Object.ClearCaches();

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 0, 0)); // called 1 time
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\",\"password\"]")))).Returns(InvalidOperationException_Timeout());
			Assert.AreEqual(client.Object.ValidationCachesCount, 0);
			Assert.IsFalse(client.Object.ValidateSystem("ABC", "123", "password"), "New record is inserted into cache with false");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.VerifyGet(_ => _.UtcNow, Times.Once);
		}

		#endregion

		#region ValidateSystemByCode with cache < 5 mins

		[Test]
		public void TestValidateSystemByCode_Unauthorized_WebExceptionFound_4mins()
		{
			var timeMock = new Mock<DateTimeWrapper>();

			client.Setup(_ => _.DateTimeWrapper).Returns(timeMock.Object);
			client.Object.ClearCaches();

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 0, 0)); // called 1 time
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\",\"password\"]")))).Returns(RequestUnauthorized());
			Assert.AreEqual(client.Object.ValidationCachesCount, 0);
			Assert.IsFalse(client.Object.ValidateSystem("ABC", "123", "password"), "New record is inserted into cache with FALSE");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 4, 0)); // called 1 time
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsFalse(client.Object.ValidateSystem("ABC", "123", "password"), "Cache is retrieved since it's within expiration and same password");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 4, 0)); // called 3 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\",\"pw\"]")))).Returns(WebException_SendFailure_WithResponseFound());
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsTrue(client.Object.ValidateSystem("ABC", "123", "pw"), "Cache is updated with TRUE");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 8, 0)); // called 1 time
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsTrue(client.Object.ValidateSystem("ABC", "123", "pw"), "Cache is retrieved since it's within expiration and 2nd password");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 9, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\",\"pw\"]")))).Returns(WebException_Timeout());
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsTrue(client.Object.ValidateSystem("ABC", "123", "pw"), "Cache is retrieved due to exception");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 10, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\",\"pp\"]")))).Returns(RequestUnauthorized());
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsFalse(client.Object.ValidateSystem("ABC", "123", "pp"), "Cache is updated within interval but different password");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.VerifyGet(_ => _.UtcNow, Times.Exactly(14));
		}

		[Test]
		public void TestValidateSystemByCodeFails_WebExceptionFound_Unauthorized_4mins()
		{
			var timeMock = new Mock<DateTimeWrapper>();

			client.Setup(_ => _.DateTimeWrapper).Returns(timeMock.Object);
			client.Object.ClearCaches();

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 0, 0)); // called 1 time
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\",\"password\"]")))).Returns(WebException_SendFailure_WithResponseFound());
			Assert.AreEqual(client.Object.ValidationCachesCount, 0);
			Assert.IsTrue(client.Object.ValidateSystem("ABC", "123", "password"), "New record is inserted into cache with TRUE");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 4, 0)); // called 1 time
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsTrue(client.Object.ValidateSystem("ABC", "123", "password"), "Cache is retrieved since it's within expiration and same password");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 4, 0)); // called 3 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\",\"pw\"]")))).Returns(RequestUnauthorized());
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsFalse(client.Object.ValidateSystem("ABC", "123", "pw"), "Cache is updated with FALSE");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 8, 0)); // called 1 time
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsFalse(client.Object.ValidateSystem("ABC", "123", "pw"), "Cache is retrieved since it's within expiration and 2nd password");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 9, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\",\"pw\"]")))).Returns(WebException_Timeout());
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsFalse(client.Object.ValidateSystem("ABC", "123", "pw"), "Cache is retrieved due to exception");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 10, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\",\"pp\"]")))).Returns(RequestOK());
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsTrue(client.Object.ValidateSystem("ABC", "123", "pp"), "Cache is updated within interval but different password");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.VerifyGet(_ => _.UtcNow, Times.Exactly(14));
		}

		#endregion

		#region ValidateSystemByCode with cache 5 mins

		[Test]
		public void TestValidateSystemByCode_Unauthorized_WebExceptionFound_5mins()
		{
			var timeMock = new Mock<DateTimeWrapper>();

			client.Setup(_ => _.DateTimeWrapper).Returns(timeMock.Object);
			client.Object.ClearCaches();

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 0, 0)); // called 1 time
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\",\"password\"]")))).Returns(RequestUnauthorized());
			Assert.AreEqual(client.Object.ValidationCachesCount, 0);
			Assert.IsFalse(client.Object.ValidateSystem("ABC", "123", "password"), "New record is inserted into cache with FALSE");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 5, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\",\"password\"]")))).Returns(WebException_SendFailure_WithResponseFound());
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsTrue(client.Object.ValidateSystem("ABC", "123", "password"), "Cache is updated with TRUE");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 10, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\",\"password\"]")))).Returns(WebException_Success());
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsTrue(client.Object.ValidateSystem("ABC", "123", "password"), "Cache is retrieved due to exception");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.VerifyGet(_ => _.UtcNow, Times.Exactly(9));
		}

		[Test]
		public void TestValidateSystemByCodeFails_WebExceptionFound_Unauthorized_5mins()
		{
			var timeMock = new Mock<DateTimeWrapper>();

			client.Setup(_ => _.DateTimeWrapper).Returns(timeMock.Object);
			client.Object.ClearCaches();

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 0, 0)); // called 1 time
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\",\"password\"]")))).Returns(WebException_SendFailure_WithResponseFound());
			Assert.AreEqual(client.Object.ValidationCachesCount, 0);
			Assert.IsTrue(client.Object.ValidateSystem("ABC", "123", "password"), "New record is inserted into cache with TRUE");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 5, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\",\"password\"]")))).Returns(RequestUnauthorized());
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsFalse(client.Object.ValidateSystem("ABC", "123", "password"), "Cache is updated with FALSE");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 10, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\",\"password\"]")))).Returns(InvalidOperationException_Timeout());
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsFalse(client.Object.ValidateSystem("ABC", "123", "password"), "Cache is retrieved due to exception");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.VerifyGet(_ => _.UtcNow, Times.Exactly(9));
		}

		#endregion

		#region All cache 24 hrs

		[Test]
		public void TestCheckSystemExistenceBySystemID_WebExceptionFound_Exceptions_24hrs()
		{
			var timeMock = new Mock<DateTimeWrapper>();

			client.Setup(_ => _.DateTimeWrapper).Returns(timeMock.Object);
			client.Object.ClearCaches();

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 0, 0)); // called 1 time
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("\"ABC\"")))).Returns(WebException_SendFailure_WithResponseFound());
			Assert.AreEqual(client.Object.ExistenceCachesCount, 0);
			Assert.IsTrue(client.Object.CheckSystemExistence("ABC"), "New record is inserted into cache with TRUE");
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 4, 0)); // called 1 time
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);
			Assert.IsTrue(client.Object.CheckSystemExistence("ABC"), "Cache is retrieved due to expiration");
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 5, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("\"ABC\"")))).Throws(new Exception("Request exception before GetResponse()"));
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);
			Assert.IsTrue(client.Object.CheckSystemExistence("ABC"), "Cache is retrieved due to exception");
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 9, 59)); // called 2 times
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);
			Assert.IsTrue(client.Object.CheckSystemExistence("ABC"), "Cache is retrieved due to check interval");
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 10, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("\"ABC\"")))).Returns(WebException_Success());
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);
			Assert.IsTrue(client.Object.CheckSystemExistence("ABC"), "Cache is retrieved due to exception");
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 23, 55, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("\"ABC\"")))).Returns(WebException_Timeout());
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);
			Assert.IsTrue(client.Object.CheckSystemExistence("ABC"), "Cache is retrieved due to exception");
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 19, 0, 0, 0)); // called 5 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("\"ABC\"")))).Returns(InvalidOperationException_Timeout());
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);
			Assert.IsFalse(client.Object.CheckSystemExistence("ABC"), "Cache is updated to FALSE after 24 hrs");
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 19, 0, 4, 0)); // called 1 time
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);
			Assert.IsFalse(client.Object.CheckSystemExistence("ABC"), "Cache is retrieved");
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 19, 0, 5, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("\"ABC\"")))).Returns(RequestOK());
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);
			Assert.IsTrue(client.Object.CheckSystemExistence("ABC"), "Cache is updated to true after recovery");
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);

			timeMock.VerifyGet(_ => _.UtcNow, Times.Exactly(26));
		}

		[Test]
		public void TestCheckSystemExistenceByCode_WebExceptionFound_Exceptions_24hrs()
		{
			var timeMock = new Mock<DateTimeWrapper>();

			client.Setup(_ => _.DateTimeWrapper).Returns(timeMock.Object);
			client.Object.ClearCaches();

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 0, 0)); // called 1 time
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\"]")))).Returns(WebException_SendFailure_WithResponseFound());
			Assert.AreEqual(client.Object.ExistenceCachesCount, 0);
			Assert.IsTrue(client.Object.CheckSystemExistence("ABC", "123"), "New record is inserted into cache with TRUE");
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 4, 0)); // called 1 time
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);
			Assert.IsTrue(client.Object.CheckSystemExistence("ABC", "123"), "Cache is retrieved due to expiration");
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 5, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\"]")))).Throws(new Exception("Request exception before GetResponse()"));
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);
			Assert.IsTrue(client.Object.CheckSystemExistence("ABC", "123"), "Cache is retrieved due to exception");
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 9, 59)); // called 2 times
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);
			Assert.IsTrue(client.Object.CheckSystemExistence("ABC", "123"), "Cache is retrieved due to check interval");
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 10, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\"]")))).Returns(WebException_Success());
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);
			Assert.IsTrue(client.Object.CheckSystemExistence("ABC", "123"), "Cache is retrieved due to exception");
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 23, 55, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\"]")))).Returns(WebException_Timeout());
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);
			Assert.IsTrue(client.Object.CheckSystemExistence("ABC", "123"), "Cache is retrieved due to exception");
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 19, 0, 0, 0)); // called 5 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\"]")))).Returns(InvalidOperationException_Timeout());
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);
			Assert.IsFalse(client.Object.CheckSystemExistence("ABC", "123"), "Cache is updated to FALSE after 24 hrs");
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 19, 0, 4, 0)); // called 1 time
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);
			Assert.IsFalse(client.Object.CheckSystemExistence("ABC", "123"), "Cache is retrieved");
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 19, 0, 5, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\"]")))).Returns(RequestOK());
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);
			Assert.IsTrue(client.Object.CheckSystemExistence("ABC", "123"), "Cache is updated to true after recovery");
			Assert.AreEqual(client.Object.ExistenceCachesCount, 1);

			timeMock.VerifyGet(_ => _.UtcNow, Times.Exactly(26));
		}

		[Test]
		public void TestValidateSystemBySystemID_WebExceptionFound_Exceptions_24hrs()
		{
			var timeMock = new Mock<DateTimeWrapper>();

			client.Setup(_ => _.DateTimeWrapper).Returns(timeMock.Object);
			client.Object.ClearCaches();

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 0, 0)); // called 1 time
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"password\"]")))).Returns(WebException_SendFailure_WithResponseFound());
			Assert.AreEqual(client.Object.ValidationCachesCount, 0);
			Assert.IsTrue(client.Object.ValidateSystem("ABC", "password"), "New record is inserted into cache with TRUE");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 4, 0)); // called 1 time
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsTrue(client.Object.ValidateSystem("ABC", "password"), "Cache is retrieved due to expiration");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 5, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"password\"]")))).Throws(new Exception("Request exception before GetResponse()"));
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsTrue(client.Object.ValidateSystem("ABC", "password"), "Cache is retrieved due to exception");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 9, 59)); // called 2 times
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsTrue(client.Object.ValidateSystem("ABC", "password"), "Cache is retrieved due to check interval");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 10, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"password\"]")))).Returns(WebException_Success());
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsTrue(client.Object.ValidateSystem("ABC", "password"), "Cache is retrieved due to exception");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 23, 55, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"password\"]")))).Returns(WebException_Timeout());
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsTrue(client.Object.ValidateSystem("ABC", "password"), "Cache is retrieved due to exception");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 19, 0, 0, 0)); // called 5 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"password\"]")))).Returns(InvalidOperationException_Timeout());
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsFalse(client.Object.ValidateSystem("ABC", "password"), "Cache is updated to FALSE after 24 hrs");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 19, 0, 4, 0)); // called 1 time
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsFalse(client.Object.ValidateSystem("ABC", "password"), "Cache is retrieved");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 19, 0, 5, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"password\"]")))).Returns(RequestOK());
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsTrue(client.Object.ValidateSystem("ABC", "password"), "Cache is updated to true after recovery");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.VerifyGet(_ => _.UtcNow, Times.Exactly(26));
		}

		[Test]
		public void TestValidateSystemByCode_WebExceptionFound_Exceptions_24hrs()
		{
			var timeMock = new Mock<DateTimeWrapper>();

			client.Setup(_ => _.DateTimeWrapper).Returns(timeMock.Object);
			client.Object.ClearCaches();

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 0, 0)); // called 1 time
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\",\"password\"]")))).Returns(WebException_SendFailure_WithResponseFound());
			Assert.AreEqual(client.Object.ValidationCachesCount, 0);
			Assert.IsTrue(client.Object.ValidateSystem("ABC", "123", "password"), "New record is inserted into cache with TRUE");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 4, 0)); // called 1 time
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsTrue(client.Object.ValidateSystem("ABC", "123", "password"), "Cache is retrieved due to expiration");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 5, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\",\"password\"]")))).Throws(new Exception("Request exception before GetResponse()"));
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsTrue(client.Object.ValidateSystem("ABC", "123", "password"), "Cache is retrieved due to exception");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 9, 59)); // called 2 times
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsTrue(client.Object.ValidateSystem("ABC", "123", "password"), "Cache is retrieved due to check interval");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 0, 10, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\",\"password\"]")))).Returns(WebException_Success());
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsTrue(client.Object.ValidateSystem("ABC", "123", "password"), "Cache is retrieved due to exception");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 18, 23, 55, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\",\"password\"]")))).Returns(WebException_Timeout());
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsTrue(client.Object.ValidateSystem("ABC", "123", "password"), "Cache is retrieved due to exception");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 19, 0, 0, 0)); // called 5 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\",\"password\"]")))).Returns(InvalidOperationException_Timeout());
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsFalse(client.Object.ValidateSystem("ABC", "123", "password"), "Cache is updated to FALSE after 24 hrs");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 19, 0, 4, 0)); // called 1 time
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsFalse(client.Object.ValidateSystem("ABC", "123", "password"), "Cache is retrieved");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.Setup(_ => _.UtcNow).Returns(new DateTime(2020, 5, 19, 0, 5, 0)); // called 4 times
			client.Setup(_ => _.Request(It.IsAny<string>(), It.Is<string>(s =>  s.Equals("[\"ABC\",\"123\",\"password\"]")))).Returns(RequestOK());
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);
			Assert.IsTrue(client.Object.ValidateSystem("ABC", "123", "password"), "Cache is updated to true after recovery");
			Assert.AreEqual(client.Object.ValidationCachesCount, 1);

			timeMock.VerifyGet(_ => _.UtcNow, Times.Exactly(26));
		}

		#endregion

		#region Mocking

		#region HttpStatusCode.OK

		private IRequest RequestOK()
		{
			var request = new Mock<IRequest>();
			request.Setup(_ => _.GetResponse()).Returns(ResponseOK());
			return request.Object;
		}

		private HttpWebResponse ResponseOK()
		{
			var response = new Mock<HttpWebResponse>();
			response.Setup(_ => _.StatusCode).Returns(HttpStatusCode.OK);
			return response.Object;
		}

		#endregion

		#region HttpStatusCode.Found

		private IRequest RequestFound()
		{
			var request = new Mock<IRequest>();
			request.Setup(_ => _.GetResponse()).Returns(ResponseFound());
			return request.Object;
		}

		private HttpWebResponse ResponseFound()
		{
			var response = new Mock<HttpWebResponse>();
			response.Setup(_ => _.StatusCode).Returns(HttpStatusCode.Found);
			response.Setup(_ => _.GetResponseStream()).Returns(new MemoryStream(Encoding.UTF8.GetBytes("Found")));
			return response.Object;
		}

		#endregion

		#region HttpStatusCode.Unauthorized

		private IRequest RequestUnauthorized()
		{
			var request = new Mock<IRequest>();
			request.Setup(_ => _.GetResponse()).Returns(ResponseUnauthorized());
			return request.Object;
		}

		private HttpWebResponse ResponseUnauthorized()
		{
			var response = new Mock<HttpWebResponse>();
			response.Setup(_ => _.StatusCode).Returns(HttpStatusCode.Unauthorized);
			return response.Object;
		}

		#endregion

		#region GetResponse() Exceptions

		private IRequest WebException_Success()
		{
			var request = new Mock<IRequest>();
			request.Setup(_ => _.GetResponse()).Throws(new WebException("Timeout", WebExceptionStatus.Success));
			return request.Object;
		}

		private IRequest WebException_Timeout()
		{
			var request = new Mock<IRequest>();
			request.Setup(_ => _.GetResponse()).Throws(new WebException("Timeout", WebExceptionStatus.Timeout));
			return request.Object;
		}

		private IRequest WebException_SendFailure_WithResponseFound()
		{
			var request = new Mock<IRequest>();
			request.Setup(_ => _.GetResponse()).Throws(new WebException("Timeout", new Exception("Exception with ok response"), WebExceptionStatus.SendFailure, ResponseFound()));
			return request.Object;
		}

		private IRequest InvalidOperationException_Timeout()
		{
			var request = new Mock<IRequest>();
			request.Setup(_ => _.GetResponse()).Throws(new InvalidOperationException());
			return request.Object;
		}

		#endregion

		#endregion
	}
}
