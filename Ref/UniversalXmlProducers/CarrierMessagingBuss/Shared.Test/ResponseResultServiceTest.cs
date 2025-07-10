using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.CarrierMessagingBuss.RefDataProducer;
using CargoWise.RefDbRepo.CarrierMessagingBuss.Shared.Model;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CarrierMessagingBuss.Shared.Test
{
	internal class ResponseResultServiceTest
	{
		[TestCase("eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpZCI6IlBRUiJ9.GeVg7qIEpWTU38DvX50sRuBArYO98lL69KPwCSaMdXc", 2)]
		[TestCase("eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpZCI6IkFCQyJ9.7nteR3I7edYTM36zd8SAPZkC70G3gC2fxlwDP5TXa1M", 3)]
		public async Task GetRefAccessorialListAsync(string token, int totalPages)
		{
			// Arrange
			var totalPagesServed = 0;
			var tokenProvider = MockHelper.GetTokenProvider(token);
			var httpWebHelper = MockHelper.GetHttpWebHelper((uri, token) =>
			{
				Assert.That(uri, Is.EqualTo("api/Accessorial/accessorialInfo?pageNumber=" + (++totalPagesServed)));
				Assert.That(token, Is.EqualTo(token));
			}, getResponseResult);
			var service = new ResponseResultService<AccessorialInfo>(httpWebHelper, tokenProvider);

			// Act
			var result = await service.GetRefAccessorialListAsync(new AppConfiguration()).ConfigureAwait(false);

			// Assert
			Assert.That(result, Is.Not.Null);

			var results = result.ToArray();
			Assert.That(results, Has.Length.EqualTo(totalPages));

			for (int i = 0; i < results.Length; i++)
			{
				Assert.That(results[i].IsSuccess, Is.True);
				Assert.That(results[i].Value.Length, Is.EqualTo(1));
				Assert.That(results[i].Value[0].Code, Is.EqualTo((i + 1).ToString(CultureInfo.InvariantCulture)));
				Assert.That(results[i].Value[0].Description, Is.EqualTo("Test Accessorial " + (i + 1)));
			}

			Task<ResponseResult<AccessorialInfo[]>> getResponseResult()
			{
				if (totalPagesServed < totalPages)
				{
					var code = (totalPagesServed + 1).ToString(CultureInfo.InvariantCulture);
					return Task.FromResult(new ResponseResult<AccessorialInfo[]>
					{
						IsSuccess = true,
						Value =
						[
							new AccessorialInfo
							{
								Code = code,
								Description = "Test Accessorial " + code
							}
						]
					});
				}

				return Task.FromResult(new ResponseResult<AccessorialInfo[]>
				{
					IsSuccess = true
				});
			}
		}

		[Test]
		public async Task GetRefAccessorialListAsyncEmptyResponseReturnsEmptyList()
		{
			// Arrange
			var tokenProvider = MockHelper.GetTokenProvider("valid_token");
			var httpWebHelper = MockHelper.GetHttpWebHelper((uri, token) =>
			{
				Assert.That(token, Is.EqualTo("valid_token"));
			}, () => Task.FromResult(new ResponseResult<AccessorialInfo[]>
			{
				IsSuccess = true,
				Value = null
			}));
			var service = new ResponseResultService<AccessorialInfo>(httpWebHelper, tokenProvider);

			// Act
			var result = await service.GetRefAccessorialListAsync(new AppConfiguration()).ConfigureAwait(false);

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result, Is.Empty);
		}

		[Test]
		public void GetRefAccessorialListAsyncNullApiResponseThrowsInvalidOperationException()
		{
			// Arrange
			var tokenProvider = MockHelper.GetTokenProvider("valid_token");
			var httpWebHelper = MockHelper.GetHttpWebHelper((uri, token) => { }, () => Task.FromResult<ResponseResult<AccessorialInfo[]>>(null));
			var service = new ResponseResultService<AccessorialInfo>(httpWebHelper, tokenProvider);

			// Act & Assert
			Assert.That(Assert.ThrowsAsync<InvalidOperationException>(async () =>
			{
				await service.GetRefAccessorialListAsync(new AppConfiguration()).ConfigureAwait(false);
			}).Message, Is.EqualTo("Api response is null."));
		}

		[TestCase(null)]
		[TestCase("")]
		[TestCase("Error occurred")]
		public void GetRefAccessorialListAsyncApiFailureReturnsEmptyList(string errorMessage)
		{
			// Arrange
			var tokenProvider = MockHelper.GetTokenProvider("valid_token");
			var httpWebHelper = MockHelper.GetHttpWebHelper((uri, token) =>
			{
				Assert.That(token, Is.EqualTo("valid_token"));
			}, () => Task.FromResult(new ResponseResult<AccessorialInfo[]>
			{
				IsSuccess = false,
				Message = errorMessage
			}));
			var service = new ResponseResultService<AccessorialInfo>(httpWebHelper, tokenProvider);

			// Act & Assert
			if (string.IsNullOrEmpty(errorMessage))
			{
				errorMessage = "No error message provided.";
			}
			Assert.That(Assert.ThrowsAsync<InvalidOperationException>(async () =>
			{
				await service.GetRefAccessorialListAsync(new AppConfiguration()).ConfigureAwait(false);
			}).Message, Is.EqualTo($"Api response failed with message: {errorMessage}"));
		}

		[Test]
		public void GetRefAccessorialListAsyncHttpExceptionThrowsException()
		{
			// Arrange
			var tokenProvider = MockHelper.GetTokenProvider("valid_token");
			var httpWebHelper = MockHelper.GetHttpWebHelper<ResponseResult<AccessorialInfo[]>>((uri, token) =>
			{
				Assert.That(token, Is.EqualTo("valid_token"));
			}, () => throw new HttpRequestException("Network error"));
			var service = new ResponseResultService<AccessorialInfo>(httpWebHelper, tokenProvider);

			// Act & Assert
			Assert.ThrowsAsync<HttpRequestException>(async () =>
			{
				await service.GetRefAccessorialListAsync(new AppConfiguration()).ConfigureAwait(false);
			});
		}
	}
}
