using System;
using CargoWise.eHub.Core.Orchestrations.HttpRetry.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Core.Tests.HttpRetry
{
	[TestClass]
	public class HttpResponseTests
	{
		[TestClass]
		public class FindHeaderValueMethod
		{
			[TestMethod]
			public void WhenGettingValidKey_ShouldReturnCorrespondingValue()
			{
				// Arrange.

				var response = new HttpResponse
				{
					Headers = new[]
					{
						HttpHeader.Create("[_MOCK_KEY_01_]", "[_MOCK_VALUE_01_]"),
						HttpHeader.Create("[_MOCK_KEY_02_]", "[_MOCK_VALUE_02_]"),
						HttpHeader.Create("[_MOCK_KEY_03_]", "[_MOCK_VALUE_03_]")
					}
				};

				// Act.

				var value = response.FindHeaderValue("[_MOCK_KEY_02_]");

				// Assert.

				Assert.AreEqual("[_MOCK_VALUE_02_]", value);
			}

			[TestMethod]
			[ExpectedException(typeof(InvalidOperationException))]
			public void WhenGettingNonExistingKey_ShouldThrowInvalidOperationException()
			{
				// Arrange.

				var response = new HttpResponse
				{
					Headers = new[]
					{
						HttpHeader.Create("[_MOCK_KEY_01_]", "[_MOCK_VALUE_01_]"),
						HttpHeader.Create("[_MOCK_KEY_02_]", "[_MOCK_VALUE_02_]"),
						HttpHeader.Create("[_MOCK_KEY_03_]", "[_MOCK_VALUE_03_]")
					}
				};

				// Act.

				var _ = response.FindHeaderValue("[_MOCK_KEY_]");

				// Assert.
			}

			[TestMethod]
			[ExpectedException(typeof(InvalidOperationException))]
			public void WhenGettingInvalidHeaders_ShouldThrowInvalidOperationException()
			{
				// Arrange.

				var response = new HttpResponse
				{
					Headers = null
				};

				// Act.

				var _ = response.FindHeaderValue("[_MOCK_KEY_]");

				// Assert.
			}

			[TestMethod]
			[ExpectedException(typeof(InvalidOperationException))]
			public void WhenGettingAmbiguousKey_ShouldThrowInvalidOperationException()
			{
				// Arrange.

				var response = new HttpResponse
				{
					Headers = new[]
					{
						HttpHeader.Create("[_MOCK_KEY_]", "[_MOCK_VALUE_01_]"),
						HttpHeader.Create("[_MOCK_KEY_]", "[_MOCK_VALUE_02_]"),
						HttpHeader.Create("[_MOCK_KEY_]", "[_MOCK_VALUE_03_]")
					}
				};

				// Act.

				var _ = response.FindHeaderValue("[_MOCK_KEY_]");

				// Assert.
			}
		}
	}
}
