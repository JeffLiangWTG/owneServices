using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using System.Web.Http.Routing;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
using Enterprise.Rating.Testing;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests.WebAPI.Controllers
{
	public class RateSearchControllerTest : TestCaseWithFactory
	{
		TestHelper Helper => testHelper ??= new TestHelper(Factory);
		TestHelper testHelper;

		[TestDate(2025, 2, 28, 12, 30, 30)]
		[TestUtcOffset(8, 0, 0)]
		public void TestSearchRates_SingleRate()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "CARR_1";
			carrier.OH_FullName = "Carrier SCAC test";
			carrier.OH_IsShippingProvider = true;
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "CAR1";
			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			var costing1 = Helper.NewCosting(carrier);
			var costEntry1 = costing1.AddRateEntry("FCL", mode: "SEA", origin: "AUMEL", destination: "USLAX", removeLines: true);
			var costLine1 = costEntry1.AddFlatCharge("FRT", 250);

			Factory.Save();

			var rateSearchQuery = new RateQueryDto
			{
				Origin = "AUMEL",
				Destination = "USLAX",
				EffectiveDate = DateTime.UtcNow.AddDays(3),
				TransportMode = "SEA",
				ContainerMode = "FCL",
				RateTypes = new [] { "Forwarding" },
				Context = RateQueryDto.RateSearchContext.RateSearch,
				JobInfo = new JobInfoDto
				{
					Containers = new List<JobContainerDto>
					{
						new ()
						{
							ContainerType = "20GP",
							Count = 4,
							Commodity = "GEN",
							Number = "123456",
							PackLines = new []
							{
								new JobPackLineDto()
								{
									Count = 79,
									PackageType = "PLT",
									Volume = 5.5m,
									VolumeUnit = "M3"
								},
							}
						}
					}
				}
			};

			var response = controller.SearchRates(rateSearchQuery);

			response.AssertResultContains(HttpStatusCode.OK);
			AssertType<JsonResult<RateSearchResponseDto>>(response);

			var jsonResponse = (JsonResult<RateSearchResponseDto>)response;
			var content = jsonResponse.Content;
			AssertNotNull(content.Rates);
			AssertNotNull(content.Log);
			
			AssertEquals($"Should get one rate in the response. Carrier Connect log: {content.Log}", 1, content.Rates.Length);

			var rate = content.Rates[0];
			AssertContainsExactElementsInAnyOrder(rate.SourcePKs, new[] { costEntry1.PK.ToGuid() });
		}

		public void TestSearchRates_ValidQueryWithNoResults()
		{
			var response = controller.SearchRates(GetValidQuery());
			response.AssertResultContains(HttpStatusCode.OK);

			AssertEquals(response.GetType(), typeof(JsonResult<RateSearchResponseDto>));
			var jsonResponse = (JsonResult<RateSearchResponseDto>)response;
			var content = jsonResponse.Content;
			AssertNotNull(content.Rates);
			AssertEquals(0, content.Rates.Length);
		}

		public void TestValidation_NoParameters()
		{
			var response = controller.SearchRates(null);
			response.AssertResultContains(HttpStatusCode.BadRequest);
		}

		public void TestValidation_EmptyParameters()
		{
			var rateSearchQuery = new RateQueryDto();
			var response = controller.SearchRates(rateSearchQuery);
			response.AssertResultContains(HttpStatusCode.BadRequest);
		}

		public void TestValidation_InvalidTransportMode()
		{
			var rateSearchQuery = GetValidQuery();
			rateSearchQuery.TransportMode = "TELEPORTATION";
			var response = controller.SearchRates(rateSearchQuery);
			response.AssertResultContains(HttpStatusCode.BadRequest);
		}

		public void TestValidation_InvalidContainerMode()
		{
			var rateSearchQuery = GetValidQuery();
			rateSearchQuery.ContainerMode = "A BIG BOX!";
			var response = controller.SearchRates(rateSearchQuery);
			response.AssertResultContains(HttpStatusCode.BadRequest);
		}

		public void TestValidation_InvalidOrigin()
		{
			var rateSearchQuery = GetValidQuery();
			rateSearchQuery.Origin = "MELBOURNE AUSTRALIA";
			var response = controller.SearchRates(rateSearchQuery);
			response.AssertResultContains(HttpStatusCode.BadRequest);

			rateSearchQuery.Origin = "*****";
			response = controller.SearchRates(rateSearchQuery);
			response.AssertResultContains(HttpStatusCode.BadRequest);
		}

		public void TestValidation_InvalidDestination()
		{
			var rateSearchQuery = GetValidQuery();
			rateSearchQuery.Destination = "SYDNEY AUSTRALIA";
			var response = controller.SearchRates(rateSearchQuery);
			response.AssertResultContains(HttpStatusCode.BadRequest);

			rateSearchQuery.Destination = "*****";
			response = controller.SearchRates(rateSearchQuery);
			response.AssertResultContains(HttpStatusCode.BadRequest);
		}

		public void TestValidation_InvalidRateType()
		{
			var rateSearchQuery = GetValidQuery();
			rateSearchQuery.RateTypes = new [] { "Potato" };
			var response = controller.SearchRates(rateSearchQuery);
			response.AssertResultContains(HttpStatusCode.BadRequest);
		}

		public void TestValidation_MandatoryJobInfoForContainerized()
		{
			var rateSearchQuery = GetValidQuery();
			rateSearchQuery.TransportMode = "SEA";
			rateSearchQuery.ContainerMode = "FCL";
			rateSearchQuery.JobInfo = new JobInfoDto { Containers = new List<JobContainerDto>() };
			var response = controller.SearchRates(rateSearchQuery);
			response.AssertResultContains(HttpStatusCode.BadRequest);
		}

		[DeveloperOnlyTest]
		public void TestSearchRates_Benchmark()
		{
			var setupStopwatch = new Stopwatch();
			var queryStopwatch = new Stopwatch();
			setupStopwatch.Start();

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "CARR_1";
			carrier.OH_FullName = "Carrier";
			carrier.OH_IsShippingProvider = true;
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "CAR1";
			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			var costing = Helper.NewCosting(carrier);

			for (int i = 0; i < 500; i++)
			{
				var costEntry = costing.AddRateEntry("FCL", mode: "SEA", origin: "AUMEL", destination: "USLAX", removeLines: true);
				costEntry.TI_ContractNumber = $"RNG_{i}";
				costEntry.AddFlatCharge("FRT", i * 10);
			}

			Factory.Save();

			setupStopwatch.Stop();
			queryStopwatch.Start();

			var rateSearchQuery = new RateQueryDto()
			{
				Origin = "AUMEL",
				Destination = "USLAX",
				EffectiveDate = DateTime.UtcNow,
				TransportMode = "SEA",
				ContainerMode = "FCL",
				RateTypes = new[] { "Forwarding" },
				Context = RateQueryDto.RateSearchContext.RateSearch,
				JobInfo = new JobInfoDto
				{
					Containers = new List<JobContainerDto>
					{
						new ()
						{
							ContainerType = "20GP",
							Count = 4,
							Commodity = "GEN",
							Number = "123456",
							PackLines = new JobPackLineDto[]
							{
								new ()
								{
									PackageType = "PLT",
									Count = 79,
									Volume = 5.5m,
									VolumeUnit = "M3"
								},
							}
						}
					}
				}
			};

			var response = controller.SearchRates(rateSearchQuery);

			queryStopwatch.Stop();

			response.AssertResultContains(HttpStatusCode.OK);

			AssertEquals(response.GetType(), typeof(JsonResult<RateSearchResponseDto>));
			var jsonResponse = (JsonResult<RateSearchResponseDto>)response;
			var content = jsonResponse.Content;
			AssertNotNull(content.Rates);
			AssertEquals(500, content.Rates.Length);

			Assert($"Setup took: {setupStopwatch.ElapsedMilliseconds / 1000}s\nQuery took: {queryStopwatch.ElapsedMilliseconds / 1000}s", false);
		}

		RateQueryDto GetValidQuery()
		{
			return new RateQueryDto
			{
				Origin = "AUMEL",
				Destination = "USLAX",
				EffectiveDate = DateTime.UtcNow,
				TransportMode = "SEA",
				ContainerMode = "LCL",
				RateTypes = new [] { "Forwarding" },
				Context = RateQueryDto.RateSearchContext.RateSearch,
			};
		}

		protected override void SetUp()
		{
			base.SetUp();

			controller = new RateSearchController();
			var controllerContext = new HttpControllerContext(new HttpConfiguration(), new HttpRouteData(new HttpRoute()), new HttpRequestMessage());
			controller.ControllerContext = controllerContext;
		}

		protected override void TearDown()
		{
			base.TearDown();

			controller?.Dispose();
		}

		RateSearchController controller;
	}
}
