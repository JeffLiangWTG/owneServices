using System;
using System.Linq;
using CargoWise.eHub.Core.Orchestrations.HttpRetry.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Core.Tests.HttpRetry
{
	[TestClass]
	public class HttpHelperTests
	{
		[TestClass]
		public class ConvertToHttpAdapterHeaderMethod
		{
			[TestMethod]
			public void WhenGettingValidHeaders_ShouldReturnValueInHttpAdapterFormat()
			{
				// Arrange.

				var headers = new[]
				{
					new HttpHeader { Key = "Accept", Value = "application/xml" },
					new HttpHeader { Key = "Authorization", Value = "Basic FOOBAR42" },
					new HttpHeader { Key = "X-Badge-ID", Value = "KOW" }
				};

				// Act.

				var adapterHeader = HttpHelper.ConvertToHttpAdapterHeader(headers);

				// Assert.

				Assert.IsNotNull(adapterHeader);

				var headerTokens = adapterHeader.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

				Assert.AreEqual(3, headerTokens.Length);

				CollectionAssert.AreEquivalent(
					headerTokens,
					new[] { "Accept: application/xml", "Authorization: Basic FOOBAR42", "X-Badge-ID: KOW" });
			}

			[TestMethod]
			public void WhenGettingEmptyHeader_ShouldReturnEmptyValue()
			{
				// Arrange.

				// Act.

				var adapterHeader = HttpHelper.ConvertToHttpAdapterHeader();

				// Assert.

				Assert.AreEqual(string.Empty, adapterHeader);
			}

			[TestMethod]
			[ExpectedException(typeof(ArgumentException))]
			public void WhenGettingHeadersWithDuplicatingKeys_ShouldThrowArgumentException()
			{
				// Arrange.

				var headers = new[]
				{
					new HttpHeader { Key = "Accept", Value = "application/xml" },
					new HttpHeader { Key = "Authorization", Value = "Basic FOOBAR42" },
					new HttpHeader { Key = "Authorization", Value = "Basic ANOTHERFOOBAR" }
				};

				// Act.

				var _ = HttpHelper.ConvertToHttpAdapterHeader(headers);

				// Assert.
			}
		}

		[TestClass]
		public class ConvertFromHttpAdapterHeader
		{
			[TestMethod]
			public void WhenGettingValidAdapterHeader_ShouldReturnHttpHeaders()
			{
				// Arrange.

				var adapterHeader = 
					"Server: Microsoft-IIS/8.5" + Environment.NewLine +
					"Content-Length: 0" + Environment.NewLine + 
					"X-Fruits: Tomato, Potato, Cucumber";

				// Act.

				var headers = HttpHelper.ConvertFromHttpAdapterHeader(adapterHeader);

				// Assert.

				Assert.IsNotNull(headers);
				Assert.AreEqual(3, headers.Length);

				var headerLookup = headers.ToDictionary(
					header => header.Key,
					header => header.Value);

				CollectionAssert.AreEquivalent(
					new[] { "Server", "Content-Length", "X-Fruits" },
					headerLookup.Keys);

				Assert.AreEqual("Microsoft-IIS/8.5", headerLookup["Server"]);
				Assert.AreEqual("0", headerLookup["Content-Length"]);
				Assert.AreEqual("Tomato, Potato, Cucumber", headerLookup["X-Fruits"]);
			}

			[TestMethod]
			public void WhenGettingEmptyAdapterHeader_ShouldReturnEmptyHeader()
			{
				// Arrange.

				// Act.

				var headers = HttpHelper.ConvertFromHttpAdapterHeader(string.Empty);

				// Assert.

				Assert.IsNotNull(headers);
				Assert.AreEqual(0, headers.Length);
			}

			[TestMethod]
			public void WhenGettingAdapterHeaderWithInvalidEntryFormat_ShouldSkipHeader()
			{
				// Arrange.

				var adapterHeader =
					"Server: Microsoft-IIS/8.5" + Environment.NewLine +
					"Content-Length: 0" + Environment.NewLine +
					"X-Fruits: Tomato : Potato : Cucumber";

				// Act.

				var headers = HttpHelper.ConvertFromHttpAdapterHeader(adapterHeader);

				// Assert.

				Assert.IsNotNull(headers);
				Assert.AreEqual(2, headers.Length);

				var headerLookup = headers.ToDictionary(
					header => header.Key,
					header => header.Value);

				CollectionAssert.AreEquivalent(
					new[] { "Server", "Content-Length" },
					headerLookup.Keys);

				Assert.AreEqual("Microsoft-IIS/8.5", headerLookup["Server"]);
				Assert.AreEqual("0", headerLookup["Content-Length"]);
			}
		}

		[TestClass]
		public class FindOverridingConfigMethod_StatusCode
		{
			[TestMethod]
			public void WhenGettingEmptyOverridingConfig_ShouldReturnNull()
			{
				// Arrange.

				var overridingConfigs = default(OverridingStatusCodeConfig[]);

				var httpError = new HttpError
				{
					StatusCode = 676,
					Message = "[_MOCK_MESSAGE_]"
				};

				// Act.

				var matchedConfig = HttpHelper.FindOverridingConfig(overridingConfigs, httpError);

				// Assert.

				Assert.IsNull(matchedConfig);
			}
			
			[TestMethod]
			public void WhenGettingOverridingConfigsWithoutMatchingStatusCode_ShouldReturnNull()
			{
				// Arrange.

				var overridingConfigs = new[]
				{
					new OverridingStatusCodeConfig
					{
						StatusCode = 474
					},
					new OverridingStatusCodeConfig
					{
						StatusCode = 575
					}
				};

				var httpError = new HttpError
				{
					StatusCode = 676
				};

				// Act.

				var matchedConfig = HttpHelper.FindOverridingConfig(overridingConfigs, httpError);

				// Assert.

				Assert.IsNull(matchedConfig);
			}

			[TestMethod]
			public void WhenGettingOverridingConfigsWithMatchingStatusCode_ShouldReturnIt()
			{
				// Arrange.

				var overridingConfigs = new[]
				{
					new OverridingStatusCodeConfig
					{
						StatusCode = 474,
						Message = string.Empty,
						ShouldRetry = true
					},
					new OverridingStatusCodeConfig
					{
						StatusCode = 575
					}
				};

				var httpError = new HttpError
				{
					StatusCode = 474
				};

				// Act.

				var matchedConfig = HttpHelper.FindOverridingConfig(overridingConfigs, httpError);

				// Assert.

				Assert.IsNotNull(matchedConfig);
				Assert.AreEqual(474, matchedConfig.StatusCode);
				Assert.AreEqual(string.Empty, matchedConfig.Message);
				Assert.IsTrue(matchedConfig.ShouldRetry);
			}

			[TestMethod]
			public void WhenGettingOverridingConfigsWithMatchingStatusCodeButDifferentMessage_ShouldReturnNull()
			{
				// Arrange.

				var overridingConfigs = new[]
				{
					new OverridingStatusCodeConfig
					{
						StatusCode = 474,
						Message = "[_MOCK_OVERRIDING_MESSAGE_]"
					},
					new OverridingStatusCodeConfig
					{
						StatusCode = 575,
						Message = string.Empty
					}
				};

				var httpError = new HttpError
				{
					StatusCode = 474,
					Message = "[_MOCK_ERROR_MESSAGE_]"
				};

				// Act.

				var matchedConfig = HttpHelper.FindOverridingConfig(overridingConfigs, httpError);

				// Assert.

				Assert.IsNull(matchedConfig);
			}

			[TestMethod]
			public void WhenGettingOverridingConfigsWithMultipleMatchingStatusCodesButDifferentMessage_ShouldReturnOneWithoutMessage()
			{
				// Arrange.

				var overridingConfigs = new[]
				{
					new OverridingStatusCodeConfig
					{
						StatusCode = 474,
						Message = string.Empty,
						ShouldRetry = true
					},
					new OverridingStatusCodeConfig
					{
						StatusCode = 474,
						Message = "[_MOCK_OVERRIDING_MESSAGE_]"
					},
					new OverridingStatusCodeConfig
					{
						StatusCode = 575,
						Message = string.Empty
					}
				};

				var httpError = new HttpError
				{
					StatusCode = 474,
					Message = "[_MOCK_ERROR_MESSAGE_]"
				};

				// Act.

				var matchedConfig = HttpHelper.FindOverridingConfig(overridingConfigs, httpError);

				// Assert.


				Assert.IsNotNull(matchedConfig);
				Assert.AreEqual(474, matchedConfig.StatusCode);
				Assert.AreEqual(string.Empty, matchedConfig.Message);
				Assert.IsTrue(matchedConfig.ShouldRetry);
			}

			[TestMethod]
			public void WhenGettingOverridingConfigsWithMultipleMatchingStatusCodesButOneMatchingMessage_ShouldReturnOneWithMessage()
			{
				// Arrange.

				var overridingConfigs = new[]
				{
					new OverridingStatusCodeConfig
					{
						StatusCode = 474,
						Message = string.Empty
					},
					new OverridingStatusCodeConfig
					{
						StatusCode = 474,
						Message = "[_MOCK_OVERRIDING_MESSAGE_]",
						ShouldRetry = true
					},
					new OverridingStatusCodeConfig
					{
						StatusCode = 575,
						Message = string.Empty
					}
				};

				var httpError = new HttpError
				{
					StatusCode = 474,
					Message = "[_MOCK_OVERRIDING_MESSAGE_]"
				};

				// Act.

				var matchedConfig = HttpHelper.FindOverridingConfig(overridingConfigs, httpError);

				// Assert.
				
				Assert.IsNotNull(matchedConfig);
				Assert.AreEqual(474, matchedConfig.StatusCode);
				Assert.AreEqual("[_MOCK_OVERRIDING_MESSAGE_]", matchedConfig.Message);
				Assert.IsTrue(matchedConfig.ShouldRetry);
			}

			[TestMethod]
			public void WhenGettingOverridingConfigsWithMultipleMatchingStatusCodesButOnePartiallyMatchingMessage_ShouldReturnOneWithMessage()
			{
				// Arrange.

				var overridingConfigs = new[]
				{
					new OverridingStatusCodeConfig
					{
						StatusCode = 474,
						Message = string.Empty
					},
					new OverridingStatusCodeConfig
					{
						StatusCode = 474,
						Message = "[_MOCK_OVERRIDING_MESSAGE_]",
						ShouldRetry = true
					},
					new OverridingStatusCodeConfig
					{
						StatusCode = 575,
						Message = string.Empty
					}
				};

				var httpError = new HttpError
				{
					StatusCode = 474,
					Message = "[_MOCK_FULL_[_MOCK_OVERRIDING_MESSAGE_]_MESSAGE_]"
				};

				// Act.

				var matchedConfig = HttpHelper.FindOverridingConfig(overridingConfigs, httpError);

				// Assert.

				Assert.IsNotNull(matchedConfig);
				Assert.AreEqual(474, matchedConfig.StatusCode);
				Assert.AreEqual("[_MOCK_OVERRIDING_MESSAGE_]", matchedConfig.Message);
				Assert.IsTrue(matchedConfig.ShouldRetry);
			}

			[TestMethod]
			public void WhenGettingOverridingConfigsWithMultipleMatchingStatusCodesButNoMatchingMessage_ShouldReturnNull()
			{
				// Arrange.

				var overridingConfigs = new[]
				{
					new OverridingStatusCodeConfig
					{
						StatusCode = 474,
						Message = "[_MOCK_OVERRIDING_MESSAGE_01_]"
					},
					new OverridingStatusCodeConfig
					{
						StatusCode = 474,
						Message = "[_MOCK_OVERRIDING_MESSAGE_02_]"
					},
					new OverridingStatusCodeConfig
					{
						StatusCode = 575,
						Message = string.Empty
					}
				};

				var httpError = new HttpError
				{
					StatusCode = 474,
					Message = "[_MOCK_ERROR_MESSAGE_]"
				};

				// Act.

				var matchedConfig = HttpHelper.FindOverridingConfig(overridingConfigs, httpError);

				// Assert.
				
				Assert.IsNull(matchedConfig);
			}

			[TestMethod]
			[ExpectedException(typeof(ArgumentException))]
			public void WhenGettingOverridingConfigsWithMultipleMatchingStatusCodesWithoutMatchingMessages_ShouldThrowArgumentException()
			{
				// Arrange.

				var overridingConfigs = new[]
				{
					new OverridingStatusCodeConfig
					{
						StatusCode = 474,
						ShouldRetry = true
					},
					new OverridingStatusCodeConfig
					{
						StatusCode = 474
					},
					new OverridingStatusCodeConfig
					{
						StatusCode = 474,
						Message = "[_MOCK_OVERRIDING_MESSAGE_]"
					 
					},
					new OverridingStatusCodeConfig
					{
						StatusCode = 575,
						Message = string.Empty
					}
				};

				var httpError = new HttpError
				{
					StatusCode = 474,
					Message = "[_MOCK_ERROR_MESSAGE_]"
				};

				// Act.

				var _ = HttpHelper.FindOverridingConfig(overridingConfigs, httpError);

				// Assert.
			}

			[TestMethod]
			[ExpectedException(typeof(ArgumentException))]
			public void WhenGettingOverridingConfigsWithMultipleMatchingStatusCodesAndMatchingMessages_ShouldThrowArgumentException()
			{
				// Arrange.

				var overridingConfigs = new[]
				{
					new OverridingStatusCodeConfig
					{
						StatusCode = 474,
						Message = "[_MOCK_OVERRIDING_MESSAGE_]",
						ShouldRetry = true
					},
					new OverridingStatusCodeConfig
					{
						StatusCode = 474,
						Message = "[_MOCK_OVERRIDING_MESSAGE_]"
					},
					new OverridingStatusCodeConfig
					{
						StatusCode = 474,
						Message = string.Empty
					},
					new OverridingStatusCodeConfig
					{
						StatusCode = 575,
						Message = string.Empty
					}
				};

				var httpError = new HttpError
				{
					StatusCode = 474,
					Message = "[_MOCK_OVERRIDING_MESSAGE_]"
				};

				// Act.

				var _ = HttpHelper.FindOverridingConfig(overridingConfigs, httpError);

				// Assert.
			}
		}

		[TestClass]
		public class FindOverridingConfigMethod_Exception
		{
			[TestMethod]
			public void WhenGettingEmptyOverridingConfig_ShouldReturnNull()
			{
				// Arrange.

				var overridingConfigs = default(OverridingExceptionConfig[]);
				var exception = new Exception();

				// Act.

				var matchedConfig = HttpHelper.FindOverridingConfig(overridingConfigs, exception);

				// Assert.

				Assert.IsNull(matchedConfig);
			}

			[TestMethod]
			public void WhenGettingOverridingConfigsWithoutMatchingExceptionType_ShouldReturnNull()
			{
				// Arrange.

				var overridingConfigs = new[]
				{
					new OverridingExceptionConfig
					{
						ExceptionType = "[_MOCK_EXCEPTION_TYPE_01_]"
					},
					new OverridingExceptionConfig
					{
						ExceptionType = "[_MOCK_EXCEPTION_TYPE_02_]"
					}
				};

				var exception = new Exception();

				// Act.

				var matchedConfig = HttpHelper.FindOverridingConfig(overridingConfigs, exception);

				// Assert.

				Assert.IsNull(matchedConfig);
			}

			[TestMethod]
			public void WhenGettingOverridingConfigsWithMatchingExceptionType_ShouldReturnIt()
			{
				// Arrange.

				var overridingConfigs = new[]
				{
					new OverridingExceptionConfig
					{
						ExceptionType = "System.Exception",
						ExceptionMessage = string.Empty,
						ShouldRetry = true
					},
					new OverridingExceptionConfig
					{
						ExceptionType = "[_MOCK_EXCEPTION_TYPE_]"
					}
				};

				var exception = new Exception();

				// Act.

				var matchedConfig = HttpHelper.FindOverridingConfig(overridingConfigs, exception);

				// Assert.

				Assert.IsNotNull(matchedConfig);
				Assert.AreEqual("System.Exception", matchedConfig.ExceptionType);
				Assert.AreEqual(string.Empty, matchedConfig.ExceptionMessage);
				Assert.IsTrue(matchedConfig.ShouldRetry);
			}

			[TestMethod]
			public void WhenGettingOverridingConfigsWithMatchingExceptionTypeButDifferentMessage_ShouldReturnNull()
			{
				// Arrange.

				var overridingConfigs = new[]
				{
					new OverridingExceptionConfig
					{
						ExceptionType = "System.Exception",
						ExceptionMessage = "[_MOCK_OVERRIDING_MESSAGE_]",
						ShouldRetry = true
					},
					new OverridingExceptionConfig
					{
						ExceptionType = "[_MOCK_EXCEPTION_TYPE_]"
					}
				};

				var exception = new Exception("[_MOCK_MESSAGE_]");

				// Act.

				var matchedConfig = HttpHelper.FindOverridingConfig(overridingConfigs, exception);

				// Assert.

				Assert.IsNull(matchedConfig);
			}

			[TestMethod]
			public void WhenGettingOverridingConfigsWithMultipleMatchingExceptionTypesButDifferentMessage_ShouldReturnOneWithoutMessage()
			{
				// Arrange.

				var overridingConfigs = new[]
				{
					new OverridingExceptionConfig
					{
						ExceptionType = "System.Exception",
						ExceptionMessage = string.Empty,
						ShouldRetry = true
					},
					new OverridingExceptionConfig
					{
						ExceptionType = "System.Exception",
						ExceptionMessage = "[_MOCK_OVERRIDING_MESSAGE_]"
					},
					new OverridingExceptionConfig
					{
						ExceptionType = "[_MOCK_EXCEPTION_]",
						ExceptionMessage = string.Empty
					}
				};

				var exception = new Exception("[_MOCK_MESSAGE_]");

				// Act.

				var matchedConfig = HttpHelper.FindOverridingConfig(overridingConfigs, exception);

				// Assert.

				Assert.IsNotNull(matchedConfig);
				Assert.AreEqual("System.Exception", matchedConfig.ExceptionType);
				Assert.AreEqual(string.Empty, matchedConfig.ExceptionMessage);
				Assert.IsTrue(matchedConfig.ShouldRetry);
			}

			[TestMethod]
			public void WhenGettingOverridingConfigsWithMultipleMatchingExceptionTypesButOneMatchingMessage_ShouldReturnOneWithMessage()
			{
				// Arrange.

				var overridingConfigs = new[]
				{
					new OverridingExceptionConfig
					{
						ExceptionType = "System.Exception",
						ExceptionMessage = string.Empty
					},
					new OverridingExceptionConfig
					{
						ExceptionType = "System.Exception",
						ExceptionMessage = "[_MOCK_OVERRIDING_MESSAGE_]",
						ShouldRetry = true
					},
					new OverridingExceptionConfig
					{
						ExceptionType = "[_MOCK_EXCEPTION_TYPE_]",
						ExceptionMessage = string.Empty
					}
				};

				var exception = new Exception("[_MOCK_OVERRIDING_MESSAGE_]");

				// Act.

				var matchedConfig = HttpHelper.FindOverridingConfig(overridingConfigs, exception);

				// Assert.

				Assert.IsNotNull(matchedConfig);
				Assert.AreEqual("System.Exception", matchedConfig.ExceptionType);
				Assert.AreEqual("[_MOCK_OVERRIDING_MESSAGE_]", matchedConfig.ExceptionMessage);
				Assert.IsTrue(matchedConfig.ShouldRetry);
			}

			[TestMethod]
			public void WhenGettingOverridingConfigsWithMultipleMatchingExceptionTypesButOnePartiallyMatchingMessage_ShouldReturnOneWithMessage()
			{
				// Arrange.

				var overridingConfigs = new[]
				{
					new OverridingExceptionConfig
					{
						ExceptionType = "System.Exception",
						ExceptionMessage = string.Empty
					},
					new OverridingExceptionConfig
					{
						ExceptionType = "System.Exception",
						ExceptionMessage = "[_MOCK_OVERRIDING_MESSAGE_]",
						ShouldRetry = true
					},
					new OverridingExceptionConfig
					{
						ExceptionType = "[_MOCK_EXCEPTION_TYPE_]",
						ExceptionMessage = string.Empty
					}
				};

				var exception = new Exception("[_MOCK_FULL_[_MOCK_OVERRIDING_MESSAGE_]_MESSAGE_]");

				// Act.

				var matchedConfig = HttpHelper.FindOverridingConfig(overridingConfigs, exception);

				// Assert.

				Assert.IsNotNull(matchedConfig);
				Assert.AreEqual("System.Exception", matchedConfig.ExceptionType);
				Assert.AreEqual("[_MOCK_OVERRIDING_MESSAGE_]", matchedConfig.ExceptionMessage);
				Assert.IsTrue(matchedConfig.ShouldRetry);
			}


			[TestMethod]
			public void WhenGettingOverridingConfigsWithMultipleMatchingExceptionTypesButNoMatchingMessage_ShouldReturnNull()
			{
				// Arrange.

				var overridingConfigs = new[]
				{
					new OverridingExceptionConfig
					{
						ExceptionType = "System.Exception",
						ExceptionMessage = "[_MOCK_OVERRIDING_MESSAGE_01_]"
					},
					new OverridingExceptionConfig
					{
						ExceptionType = "System.Exception",
						ExceptionMessage = "[_MOCK_OVERRIDING_MESSAGE_02_]"
					},
					new OverridingExceptionConfig
					{
						ExceptionType = "[_MOCK_EXCEPTION_TYPE_]",
						ExceptionMessage = string.Empty
					}
				};

				var exception = new Exception("[_MOCK_MESSAGE_]");

				// Act.

				var matchedConfig = HttpHelper.FindOverridingConfig(overridingConfigs, exception);

				// Assert.

				Assert.IsNull(matchedConfig);
			}

			[TestMethod]
			[ExpectedException(typeof(ArgumentException))]
			public void WhenGettingOverridingConfigsWithMultipleMatchingExceptionTypesWithoutMatchingMessages_ShouldThrowArgumentException()
			{
				// Arrange.

				var overridingConfigs = new[]
				{
					new OverridingExceptionConfig
					{
						ExceptionType = "System.Exception",
						ShouldRetry = true
					},
					new OverridingExceptionConfig
					{
						ExceptionType = "System.Exception"
					},
					new OverridingExceptionConfig
					{
						ExceptionType = "System.Exception",
						ExceptionMessage = "[_MOCK_OVERRIDING_MESSAGE_]"
					 
					},
					new OverridingExceptionConfig
					{
						ExceptionType = "[_MOCK_EXCEPTION_TYPE_]",
						ExceptionMessage = string.Empty
					}
				};

				var exception = new Exception();

				// Act.

				var _ = HttpHelper.FindOverridingConfig(overridingConfigs, exception);

				// Assert.
			}

			[TestMethod]
			[ExpectedException(typeof(ArgumentException))]
			public void WhenGettingOverridingConfigsWithMultipleMatchingExceptionTypesAndMatchingMessages_ShouldThrowArgumentException()
			{
				// Arrange.

				var overridingConfigs = new[]
				{
					new OverridingExceptionConfig
					{
						ExceptionType = "System.Exception",
						ExceptionMessage = "[_MOCK_OVERRIDING_MESSAGE_]",
						ShouldRetry = true
					},
					new OverridingExceptionConfig
					{
						ExceptionType = "System.Exception",
						ExceptionMessage = "[_MOCK_OVERRIDING_MESSAGE_]"
					},
					new OverridingExceptionConfig
					{
						ExceptionType = "System.Exception",
						ExceptionMessage = string.Empty
					},
					new OverridingExceptionConfig
					{
						ExceptionType = "[_MOCK_EXCEPTION_TYPE_]",
						ExceptionMessage = string.Empty
					}
				};

				var exception = new Exception("[_MOCK_OVERRIDING_MESSAGE_]");

				// Act.

				var _ = HttpHelper.FindOverridingConfig(overridingConfigs, exception);

				// Assert.
			}
		}
	}
}
