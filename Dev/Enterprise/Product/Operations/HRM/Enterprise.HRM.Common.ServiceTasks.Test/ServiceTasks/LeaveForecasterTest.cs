using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Definitions.LeaveEngine;
using CargoWise.EntityFramework.Testing;
using Enterprise.HRM.Common;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Moq;
using Newtonsoft.Json;

namespace Enterprise.HRM.ServiceTasks.Testing
{
	class LeaveForecasterTest : TestCaseWithFactory
	{
		public void TestForecastArgumentsPassedCorrectly()
		{
			var tests = new[]
			{
				("api/leave/forecast?staffCode=BOB&accrualDate=2022-06-01&onlyIncludeRequestedSnapshots=false", new object[] { "BOB", new DateTime(2022, 6, 1), null, null, false }),
				("api/leave/forecast?staffCode=BOB&accrualDate=2023-12-15&onlyIncludeRequestedSnapshots=false", new object[] { "BOB", new DateTime(2023, 12, 15), null, null, false }),
				("api/leave/forecast?staffCode=BOB&accrualDate=2022-06-01&onlyIncludeRequestedSnapshots=false&lastAccrualDate=2022-07-01", new object[] { "BOB", new DateTime(2022, 6, 1), new DateTime(2022, 7, 1), null, false }),
				("api/leave/forecast?staffCode=BOB&accrualDate=2022-06-01&onlyIncludeRequestedSnapshots=false&leaveOnAccrualDate=true", new object[] { "BOB", new DateTime(2022, 6, 1), null, true, false }),
				("api/leave/forecast?staffCode=BOB&accrualDate=2022-06-01&onlyIncludeRequestedSnapshots=false&leaveOnAccrualDate=true", new object[] { "BOB", new DateTime(2022, 6, 1), null, true, false }),
				("api/leave/forecast?staffCode=BOB&accrualDate=2022-06-01&onlyIncludeRequestedSnapshots=true&leaveOnAccrualDate=false", new object[] { "BOB", new DateTime(2022, 6, 1), null, false, true }),
			};

			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://serviceaddress");
			var url = string.Empty;

			var mock = new Mock<IGlowServiceClient>();
			mock.Setup(m => m.GetAsync(It.IsAny<string>()))
				.Callback<string>(u => url = u)
				.Throws<Exception>(); // We just need to steal the url used, don't care about what LeaveForecaster does with the results

			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			clientFactoryMock.Setup(f => f.Create(It.IsAny<Uri>())).Returns(mock.Object);

			using (ObjectFactory.Substitute(clientFactoryMock.Object))
			{
				var method = typeof(LeaveForecaster).GetMethod(nameof(LeaveForecaster.Forecast));
				CombineAssertions(() =>
				{
					for (var i = 0; i < tests.Length; i++)
					{
						var (expectedUrl, parameters) = tests[i];
						var forecaster = new LeaveForecaster();
						AssertExceptionThrown<Exception>(() => method.Invoke(forecaster, parameters));
						AssertEquals($"Case #{i}", expectedUrl, url);
					}
				});
			}
		}

		public void TestForecast_Success()
		{
			var dummyData = CreateTestData();

			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://serviceaddress");
			var clientMock = new Mock<IGlowServiceClient>(MockBehavior.Strict);

			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			clientFactoryMock.Setup(f => f.Create(new Uri("https://serviceaddress"))).Returns(clientMock.Object);

			clientMock.Setup(c => c.GetAsync("api/leave/forecast?staffCode=AAB&accrualDate=2023-06-29&onlyIncludeRequestedSnapshots=false&leaveOnAccrualDate=false")).Returns(
				Task.FromResult(
					new HttpResponseMessage()
					{
						Content = new StringContent(JsonConvert.SerializeObject(dummyData))
					}));

			clientMock.Setup(c => c.GetAsync("api/leave/forecast?staffCode=AAB&accrualDate=2023-06-29&onlyIncludeRequestedSnapshots=false")).Returns(
				Task.FromResult(
					new HttpResponseMessage()
					{
						Content = new StringContent(JsonConvert.SerializeObject(dummyData))
					}));

			using (ObjectFactory.Substitute(clientFactoryMock.Object))
			{
				var result = new LeaveForecaster().Forecast("AAB", new DateTime(2023, 06, 29), null, false, false);
				AssertResult(result);

				var resultWithoutLeaveOnAccrualDate = new LeaveForecaster().Forecast("AAB", new DateTime(2023, 06, 29), null, null, false);
				AssertResult(resultWithoutLeaveOnAccrualDate);
			}

			void AssertResult(LeaveForecasterResult result)
			{
				AssertEquals(2, result.BalanceTransactions.Count());
				for (var i = 0; i < 2; i++)
				{
					AssertBalanceTransaction(dummyData.BalanceTransactions.ElementAt(i), result.BalanceTransactions.ElementAt(i));
				}

				AssertEquals(2, result.LeaveProcessed.Count());
				for (var i = 0; i < 2; i++)
				{
					AssertLeaveProcessed(dummyData.LeaveProcessed.ElementAt(i), result.LeaveProcessed.ElementAt(i));
				}
			}
		}

		public void TestForecast_Success_WithLastAccrualDate()
		{
			var dummyData = CreateTestData();

			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://serviceaddress");
			var clientMock = new Mock<IGlowServiceClient>(MockBehavior.Strict);

			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			clientFactoryMock.Setup(f => f.Create(new Uri("https://serviceaddress"))).Returns(clientMock.Object);

			clientMock.Setup(c => c.GetAsync("api/leave/forecast?staffCode=AAB&accrualDate=2023-06-29&onlyIncludeRequestedSnapshots=true&lastAccrualDate=2023-06-28&leaveOnAccrualDate=false")).Returns(
				Task.FromResult(
					new HttpResponseMessage()
					{
						Content = new StringContent(JsonConvert.SerializeObject(dummyData))
					}));

			using (ObjectFactory.Substitute(clientFactoryMock.Object))
			{
				var result = new LeaveForecaster().Forecast("AAB", new DateTime(2023, 06, 29), new DateTime(2023, 06, 28), false, onlyIncludeRequestedSnapshots: true);

				AssertEquals(2, result.BalanceTransactions.Count());
				for (var i = 0; i < 2; i++)
				{
					AssertBalanceTransaction(dummyData.BalanceTransactions.ElementAt(i), result.BalanceTransactions.ElementAt(i));
				}

				AssertEquals(2, result.LeaveProcessed.Count());
				for (var i = 0; i < 2; i++)
				{
					AssertLeaveProcessed(dummyData.LeaveProcessed.ElementAt(i), result.LeaveProcessed.ElementAt(i));
				}
			}
		}

		static void AssertBalanceTransaction(BalanceTransaction expectedTransaction, BalanceTransaction returnTransaction)
		{
			AssertEquals(expectedTransaction.NonForfeitureHours, returnTransaction.NonForfeitureHours);
			AssertEquals(expectedTransaction.EffectiveDate, returnTransaction.EffectiveDate);
			AssertEquals(expectedTransaction.LeaveType, returnTransaction.LeaveType);
			AssertEquals(expectedTransaction.ForfeitureBuckets.Count(), returnTransaction.ForfeitureBuckets.Count());

			AssertEquals(expectedTransaction.ForfeitureBuckets.Count(), returnTransaction.ForfeitureBuckets.Count());
			for (var i = 0; i < expectedTransaction.ForfeitureBuckets.Count(); i++)
			{
				AssertForfeitureBuckets(expectedTransaction.ForfeitureBuckets.ElementAt(i), returnTransaction.ForfeitureBuckets.ElementAt(i));
			}
		}

		static void AssertForfeitureBuckets(PendingForfeiture expectedPendingForfeiture, PendingForfeiture returnPendingForfeiture)
		{
			AssertEquals(expectedPendingForfeiture.Amount, returnPendingForfeiture.Amount);
			AssertEquals(expectedPendingForfeiture.Name, returnPendingForfeiture.Name);
			AssertEquals(expectedPendingForfeiture.Expiry, returnPendingForfeiture.Expiry);
		}

		static void AssertLeaveProcessed(LeaveProcessed expectedProcessed, LeaveProcessed returnProcessed)
		{
			AssertEquals(expectedProcessed.LeaveType, returnProcessed.LeaveType);
			AssertEquals(expectedProcessed.TotalHours, returnProcessed.TotalHours);
			AssertEquals(expectedProcessed.TotalProcessedHours, returnProcessed.TotalProcessedHours);
			AssertEquals(expectedProcessed.HoursProcessedInThisPeriod, returnProcessed.HoursProcessedInThisPeriod);
			AssertEquals(expectedProcessed.ProcessedFrom, returnProcessed.ProcessedFrom);
			AssertEquals(expectedProcessed.ProcessedTo, returnProcessed.ProcessedTo);
			AssertEquals(expectedProcessed.ProcessedDate, returnProcessed.ProcessedDate);
			AssertEquals(expectedProcessed.AccrualDate, returnProcessed.AccrualDate);
			AssertEquals(expectedProcessed.FreeText, returnProcessed.FreeText);
		}

		static LeaveForecasterResult CreateTestData()
		{
			var balanceTransactions = new List<BalanceTransaction>
			{
				new BalanceTransaction
				{
					NonForfeitureHours = 80,
					EffectiveDate = new DateTime(2022, 1, 1),
					LeaveType = "Annual",
					ForfeitureBuckets = new List<PendingForfeiture>
					{
						new PendingForfeiture
						{
							Amount = 10,
							Expiry = new DateTime(2022, 6, 30),
							Name = "Unused hours"
						},
						new PendingForfeiture
						{
							Amount = 5,
							Expiry = new DateTime(2022, 12, 31),
							Name = "Carryover hours"
						}
					}
				},
				new BalanceTransaction
				{
					NonForfeitureHours = 40,
					EffectiveDate = new DateTime(2022, 7, 1),
					LeaveType = "Sick",
					ForfeitureBuckets = new List<PendingForfeiture>
					{
						new PendingForfeiture
						{
							Amount = 8,
							Expiry = new DateTime(2023, 6, 30),
							Name = "Unused hours"
						}
					}
				}
			};

			var leaveProcessed = new List<LeaveProcessed>
			{
				new LeaveProcessed
				{
					LeaveType = "Annual",
					TotalHours = 120,
					TotalProcessedHours = 80,
					HoursProcessedInThisPeriod = 20,
					ProcessedFrom = new DateTime(2022, 1, 1),
					ProcessedTo = new DateTime(2022, 6, 30),
					ProcessedDate = new DateTime(2022, 6, 30),
					AccrualDate = new DateTime(2022, 7, 1),
					FreeText = "Carryover hours",
				},
				new LeaveProcessed
				{
					LeaveType = "Sick",
					TotalHours = 40,
					TotalProcessedHours = 32,
					HoursProcessedInThisPeriod = 8,
					ProcessedFrom = new DateTime(2022, 7, 1),
					ProcessedTo = new DateTime(2022, 12, 31),
					ProcessedDate = new DateTime(2022, 12, 31),
					AccrualDate = new DateTime(2023, 1, 1),
					FreeText = "Carryover hours",
				}
			};

			var leaveForecasterResult = new LeaveForecasterResult
			{
				BalanceTransactions = balanceTransactions,
				LeaveProcessed = leaveProcessed
			};

			return leaveForecasterResult;
		}

		public void TestForecast_HttpError()
		{
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://serviceaddress");
			var clientMock = new Mock<IGlowServiceClient>();

			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			clientFactoryMock.Setup(f => f.Create(new Uri("https://serviceaddress"))).Returns(clientMock.Object);

			clientMock.Setup(c => c.GetAsync(It.IsAny<string>())).Returns(
				Task.FromResult(
					new HttpResponseMessage()
					{
						StatusCode = HttpStatusCode.BadRequest,
						Content = new StringContent("Bad Request")
					}));

			using (ObjectFactory.Substitute(clientFactoryMock.Object))
			{
				AssertExceptionThrown<HttpRequestException>(() => new LeaveForecaster().Forecast("AAB", DateTime.Today, null, false, onlyIncludeRequestedSnapshots: true));
			}
		}
	}
}
