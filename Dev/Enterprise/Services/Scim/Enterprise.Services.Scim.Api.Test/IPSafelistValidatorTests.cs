using System;
using System.Net;
using Enterprise.Services.Scim.Api.Helpers;
using Enterprise.Services.Scim.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.Scim.Api.Test
{
	class IPSafelistValidatorTests
	{
		Mock<IIPSafelistCacheStore> SafelistCacheMock;
		SafelistValidator Validator;
		readonly string[] AllowedIps =
		[
		   "127.0.0.1",
			"::1",
			"10.0.0.0",
			"172.16.0.0",
			"192.168.0.0"
		];

		[SetUp]
		public void Setup()
		{
			SafelistCacheMock = new Mock<IIPSafelistCacheStore>();
			SafelistCacheMock.Setup(x => x.GetSafelistedIpsFromCache())
			.Returns([
				IPNetwork2.Parse("10.0.0.1"),
				IPNetwork2.Parse("10.0.0.2"),
				IPNetwork2.Parse("10.0.0.3"),
				IPNetwork2.Parse("20.44.3.160/27")
			]);
			Validator = new SafelistValidator(SafelistCacheMock.Object);
		}

		[Test]
		public void TestIPValidationWhenSafeListDisabled()
		{
			var result = Validator.ValidateIp("10.0.0.1", AllowedIps);
			Assert.That(result, Is.True);
		}

		[TestCase("10.0.0.1", true)]
		[TestCase("20.44.3.190", true)]
		[TestCase("192.168.1.1", false)]
		[TestCase("2001:db8::1", false)]
		public void TestIPValidationSafelistedIPS(string ipAddress, bool expectedValue)
		{
			var result = Validator.ValidateIp(ipAddress, AllowedIps);
			Assert.That(result, Is.EqualTo(expectedValue));
		}

		[Test]
		public void TestIPValidation_ThrowsWhenNoIPS()
		{
			SafelistCacheMock.Setup(x => x.GetSafelistedIpsFromCache()).Returns([]);
			Assert.Throws<InvalidOperationException>(() =>
				Validator.ValidateIp("10.0.0.1", AllowedIps)
			);
		}

		[Test]
		public void TestIPValidation_ThrowsWhenRetrievingIPSFail()
		{
			SafelistCacheMock.Setup(x => x.GetSafelistedIpsFromCache())
				.Throws(new Exception());
			Assert.Throws<Exception>(() =>
				Validator.ValidateIp("10.0.0.1", AllowedIps)
			);
		}

		[Test]
		public void TestIPValidation_HandlesEmptyIPAddress()
		{
			Assert.Throws<ArgumentNullException>(() =>
				Validator.ValidateIp("", AllowedIps)
			);
		}

		[Test]
		public void TestIPValidation_HandlesInvalidIPFormat()
		{
			Assert.Throws<FormatException>(() =>
				Validator.ValidateIp("invalid.ip.1", AllowedIps)
			);
		}
	}
}
