using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Rating.Business.WiseRates;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;
using WiseRates.Api.Model;
using WiseRates.Tools.Enums;

namespace Enterprise.Rating.Business.Testing
{
	public class WiseRatesClientWithCacheTest : TestCase
	{
		#region [SearchAsync] Should return rates from the cache if the query hits the cache

		public void TestSearchAsync_HitCache()
		{
			var request = CreateRequest();
			var response = CreateResponse("1");
			var actualResponse = Search(request, response);
			AssertEquals("Should return result from the service", "1", actualResponse.Rates[0].Id);

			var request2 = GetClone(request);
			var response2 = CreateResponse("2");
			actualResponse = Search(request2, response2);
			AssertEquals("Should return result from cache", "1", actualResponse.Rates[0].Id);
			AssertEquals(true, actualResponse.IsFromCache);
		}

		#endregion

		#region [SearchAsync] Should return rates from the cache if the query hits the cache, even though collection items are in different orde

		public void TestSearchAsync_HitCache_ShuffleCollections()
		{
			var request = CreateRequest();
			var response = CreateResponse("1");
			var actualResponse = Search(request, response);
			AssertEquals("Should return result from the service", "1", actualResponse.Rates[0].Id);

			// Modify the request and send again
			var request2 = GetClone(request);
			ShuffleCollectionsAndAssert(request2, response);
		}

		void ShuffleCollectionsAndAssert(RatesSearchRequest request, RatesSearchResponse cachedResponse)
		{
			var serviceResponse = CreateResponse("2");
			var query = request.RatesQuery;
			var properties = query.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

			foreach (var propertyInfo in properties)
			{
				if (!typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType))
				{
					continue;
				}

				if (propertyInfo.PropertyType == typeof(string))
				{
					continue;
				}

				var currentValue = propertyInfo.GetValue(query);
				var newValue = GetShuffledCollection(query, propertyInfo);
				propertyInfo.SetValue(query, newValue);

				var actualResponse = Search(request, serviceResponse);
				AssertEquals($"{propertyInfo.Name} from {propertyInfo.DeclaringType} has the same although in different order and thus it should take result from the cache", "1", actualResponse.Rates[0].Id);
				AssertEquals(true, actualResponse.IsFromCache);

				propertyInfo.SetValue(query, currentValue);
			}
		}

		IEnumerable GetShuffledCollection(RatesQuery query, PropertyInfo collectionProperty)
		{
			switch (collectionProperty.Name)
			{
				case nameof(RatesQuery.Carrier):
					return query.Carrier.Reverse();

				case nameof(RatesQuery.Commodities):
					return query.Commodities.Reverse();

				case nameof(RatesQuery.Container):
					return query.Container.Reverse();

				case nameof(RatesQuery.Contract):
					return query.Contract.Reverse();

				case nameof(RatesQuery.Destination):
					return query.Destination.Reverse();

				case nameof(RatesQuery.Measures):
					return query.Measures?.Reverse();

				case nameof(RatesQuery.Origin):
					return query.Origin.Reverse();

				case nameof(RatesQuery.AcceptedProviders):
					return query.AcceptedProviders.Reverse();

				case nameof(RatesQuery.ContainerMode):
					return query.ContainerMode.Reverse();

				case nameof(RatesQuery.ContractNumber):
					return query.ContractNumber.Reverse();

				case nameof(RatesQuery.PaymentTerm):
					return query.PaymentTerm.Reverse();

				case nameof(RatesQuery.ServiceLevel):
					return query.ServiceLevel.Reverse();

				case nameof(RatesQuery.TransportMode):
					return query.TransportMode.Reverse();

				case nameof(RatesQuery.NamedAccount):
					return query.NamedAccount.Reverse();

				default:
					throw new InvalidOperationException($"Property {collectionProperty.Name} is not supported. Please add a case for this property.");
			}
		}

		#endregion

		#region [SearchAsync] Should request rates form the service if page is not cached

		public void TestSearchAsync_MissCache_PageId()
		{
			// Send a first request and make sure it is cached
			var request = CreateRequest();
			var response = CreateResponse("1");
			var actualResponse = Search(request, response);
			AssertEquals("Should return result from the service", "1", actualResponse.Rates[0].Id);

			// Modify the request and send again
			var request2 = GetClone(request);
			request2.PageID += 1;

			var response2 = CreateResponse("2");
			actualResponse = Search(request2, response2);
			AssertEquals("Should return result from the service", "2", actualResponse.Rates[0].Id);
			AssertEquals(false, actualResponse.IsFromCache);
		}

		#endregion

		#region [SearchAsync] Should request rates form the service if the query misses the cache

		// TOOD: Fix the test

		// public void TestSearchAsync_MissCache()
		// {
		// 	// Send a first request and make sure it is cached
		// 	var request = CreateRequest();
		// 	var response = CreateResponse("1");
		// 	var actualResponse = await SearchAsync(request, response);
		// 	AssertEquals("Should return result from the service", "1", actualResponse.Rates[0].Id);
		//
		// 	// Modify the request and send again
		// 	var request2 = GetClone(request);
		// 	await ModifyObjectAndAssert(request2.RatesQuery, request2);
		// }
		//
		// void ModifyObjectAndAssert(object obj, RatesSearchRequest request)
		// {
		// 	var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
		//
		// 	foreach (var propertyInfo in properties)
		// 	{
		// 		if (!IsPartOfCacheKey(propertyInfo))
		// 		{
		// 			continue;
		// 		}
		//
		// 		var currentValue = propertyInfo.GetValue(obj);
		// 		if (currentValue is IEnumerable enumerable)
		// 		{
		// 			var enumerator = enumerable.GetEnumerator();
		// 			enumerator.MoveNext();
		//
		// 			var item = enumerator.Current;
		// 			if (!IsSimple(item.GetType()))
		// 			{
		// 				await ModifyObjectAndAssert(item, request);
		// 				continue;
		// 			}
		// 		}
		// 		else if (!IsSimple(currentValue.GetType()))
		// 		{
		// 			await ModifyObjectAndAssert(currentValue, request);
		// 			continue;
		// 		}
		//
		// 		var newValue = GetRandomValue(propertyInfo.PropertyType);
		// 		propertyInfo.SetValue(obj, newValue);
		//
		// 		var serviceResponse = CreateResponse("2");
		// 		var actualResponse = await SearchAsync(request, serviceResponse);
		// 		actualResponse.Rates[0].Id.Should().Be(serviceResponse.Rates[0].Id, $"{propertyInfo.Name} from {propertyInfo.DeclaringType} in the query has changed and a new request to the service should have been sent");
		// 		AssertEquals(false, actualResponse.IsFromCache);
		//
		// 		propertyInfo.SetValue(obj, currentValue);
		// 	}
		// }

		static bool IsPartOfCacheKey(PropertyInfo property)
		{
			var propertiesToIgnore = new[]
			{
				$"{nameof(RatesQueryCarrier)}.{nameof(RatesQueryCarrier.Name)}",
				$"{nameof(RatesQueryCarrier)}.{nameof(RatesQueryCarrier.Source)}",
			};

			var fullName = $"{property.DeclaringType.Name}.{property.Name}";
			return !propertiesToIgnore.Contains(fullName);
		}

		#endregion

		#region [SearchAsync] Should return a copy of the response from the cache

		public void TestSearchAsync_Cache_ReturnCopyOfResponse()
		{
			var request = CreateRequest();
			var response = CreateResponse("1");
			var response1 = Search(request, response);

			var request2 = GetClone(request);
			var newResponse = CreateResponse("2");
			var response2 = Search(request2, newResponse);

			AssertNotSame(response1, response2);
		}

		#endregion

		public void TestSearchAsync_BadResponse_ShouldNotCache()
		{
			// Send the first request which should not be cached as provider 0 failed
			var request = CreateRequest();
			var expectedResponse1 = CreateResponse("1");
			expectedResponse1.Providers[0].ConnectionResult = ConnectionResult.Failure;
			expectedResponse1.Providers[1].ConnectionResult = ConnectionResult.Success;

			var actualResponse = Search(request, expectedResponse1);
			AssertEquals(false, actualResponse.IsFromCache);
			AssertEquals("Should load from the service since the cache is empty", "1", actualResponse.Rates[0].Id);

			// Send the second request which this time should be cached as all providers are fine
			var expectedResponse2 = CreateResponse("2");
			expectedResponse2.Providers[0].ConnectionResult = ConnectionResult.Success;
			expectedResponse2.Providers[1].ConnectionResult = ConnectionResult.Success;

			actualResponse = Search(request, expectedResponse2);
			AssertEquals(false, actualResponse.IsFromCache);
			AssertEquals("Should load from the service since the cache is still empty as the previous request should not have cached", "2", actualResponse.Rates[0].Id);

			// Send the third request which should be retrieved from the cache as the previous one cached
			var expectedResponse3 = CreateResponse("3");

			actualResponse = Search(request, expectedResponse3);
			AssertEquals(true, actualResponse.IsFromCache);
			AssertEquals("Should load from the cache since the previous one should have cached", "2", actualResponse.Rates[0].Id);
		}

		protected RatesSearchRequest CreateRequest()
		{
			var request = new RatesSearchRequest();
			request.RatesQuery = new RatesQuery
			{
				Origin = ["UAIEV"],
				Destination = ["AYSYD"],
				TransportMode = ["SEA"],
				ContainerMode = ["FCL"],
				Commodities = ["Humans", "Cars"],
				Container = [
					new RatesQueryContainer
					{
						Code = "20GP",
						ISOType = "20GP"
					},
					new RatesQueryContainer
					{
						Code = "40GP",
						ISOType = "40GP"
					}
				],
				Contract = [
					new RatesQueryContract
					{
						ContractNumber = "CONTRACT",
					},
					new RatesQueryContract
					{
						ContractNumber = "CONTRACT2",
					}
				],
				ContractNumber = ["CONTRACT", "CONTRACT2"],
				PaymentTerm = ["PPD"],
				ServiceLevel = ["USC", "USC2"],
				AcceptedProviders = ["TESTPROVIDER", "TESTPROVIDER2"],
				Carrier = [
					new RatesQueryCarrier
					{
						Code = "TESTCARRIER",
						Name = "TESTCARRIERNAME",
						SCACCode = "TESTSCAC"
					},
					new RatesQueryCarrier
					{
						Code = "TESTCARRIER2",
						Name = "TESTCARRIERNAME2",
						SCACCode = "TESTSCAC2"
					}
				],
				NamedAccount = [
					new RatesQueryNamedAccount
					{
						Name = "Account1"
					},
					new RatesQueryNamedAccount
					{
						Name = "Account2"
					}
				]
			};

			return request;
		}

		protected RatesSearchResponse CreateResponse(string id)
		{
			var response = new RatesSearchResponse();
			response.Rates =
			[
				new Rate
				{
					Id = id,
					Charges = new List<Charge>
					{
						new Charge
						{
							ChargeCode = "FRT",
							Currency = "AUD",
							FlatRate = 8m,
							ChargeType = ChargeType.NotApplicable
						},
						new Charge
						{
							ChargeCode = "CAF",
							Currency = "AUD",
							FlatRate = 16m,
							ChargeType = ChargeType.Optional | ChargeType.SubjectTo
						},
						new Charge
						{
							ChargeCode = "WAR",
							Currency = "AUD",
							ChargeType = ChargeType.Included
						}
					}
				}
			];

			response.Providers =
			[
				new ProviderResult
				{
					ConnectionResult = ConnectionResult.Success,
					ProviderName = "TestProvider1"
				},
				new ProviderResult
				{
					ConnectionResult = ConnectionResult.Success,
					ProviderName = "TestProvider2"
				},
			];

			return response;
		}

		RatesSearchResponse Search(RatesSearchRequest request, RatesSearchResponse mockedResponse)
		{
			var messageHandler = new Mock<HttpMessageHandler>();
			messageHandler.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent(GetJson(mockedResponse))
				});

			var client = new WiseRatesClientWithCache("http://fakewebsite.com", "1", messageHandler.Object);

			var actualResponse = client.SearchAsync(request).GetAwaiter().GetResult();
			return actualResponse;
		}

		static T GetClone<T>(T obj)
		{
			var json = JsonConvert.SerializeObject(obj);
			var clone = JsonConvert.DeserializeObject<T>(json);
			return clone;
		}

		static string GetJson<T>(T obj)
		{
			return JsonConvert.SerializeObject(obj);
		}

		static bool IsSimple(Type type)
		{
			return type.IsPrimitive
				   || type.IsEnum
				   || type == typeof(string)
				   || type == typeof(decimal)
				   || type == typeof(DateTime);
		}
	}
}
