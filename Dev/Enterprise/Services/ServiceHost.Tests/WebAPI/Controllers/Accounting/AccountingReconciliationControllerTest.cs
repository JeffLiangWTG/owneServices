using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CargoWise.Common;
#if NETFRAMEWORK
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
#elif NET
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;
#endif
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.APAutomation.APReconciliation;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Moq;
using Newtonsoft.Json;
using static Enterprise.Services.ServiceHost.AccountingReconciliationController;

namespace Enterprise.Services.ServiceHost.Tests
{
	class AccountingReconciliationControllerTest : TestCaseWithFactory
	{
		public void TestGetChargeAccrualsShouldReturnAccrualsWhenOnlyChargeAccruals()
		{
			var accruals = GetSampleChargeAccruals(sampleGetAccrualsRequest);
			var expectedAccruals = accruals.Select(controller.ConvertChargeToAccrualDetails).ToList();

			mockReconciliationService.Setup(s => s.GetChargeAccruals(It.IsAny<List<JobParentInfo>>(), It.IsAny<ZGuid>())).Returns(accruals);
			mockReconciliationService.Setup(s => s.GetConsolCostAccruals(It.IsAny<List<JobParentInfo>>(), It.IsAny<ZGuid>())).Returns([]);

			var result = controller.GetAccruals(sampleGetAccrualsRequest).JsonResult<IEnumerable<JobDetailsWithAccrualsAndRelatedJobs>>(HttpStatusCode.OK);

			AssertNotNull(result);
			AssertJsonEqual(expectedAccruals, result);
		}

		public void TestGetChargeAccrualsShouldReturnAccrualsWhenOnlyConsolCosts()
		{
			var allConsolCostsIncludingPosted = GetSampleConsolCostsIncludingPosted(sampleGetAccrualsRequest);
			var expectedUnpostedConsolCosts = allConsolCostsIncludingPosted.Select(controller.ConvertConsolCostsToAccrualDetails).ToList();

			mockReconciliationService.Setup(s => s.GetChargeAccruals(It.IsAny<List<JobParentInfo>>(), It.IsAny<ZGuid>())).Returns([]);
			mockReconciliationService.Setup(s => s.GetConsolCostAccruals(It.IsAny<List<JobParentInfo>>(), It.IsAny<ZGuid>())).Returns(allConsolCostsIncludingPosted);

			var filteredJobParentsInfo = sampleGetAccrualsRequest.JobParentsInfo
				.Where(x => x.ParentTableCode == "JK")
				.ToList();

			var filteredRequest = new GetAccrualsRequest
			{
				JobParentsInfo = filteredJobParentsInfo,
				CompanyPK = sampleGetAccrualsRequest.CompanyPK
			};

			var result = controller.GetAccruals(filteredRequest).JsonResult<IEnumerable<JobDetailsWithAccrualsAndRelatedJobs>>(HttpStatusCode.OK);

			AssertNotNull(result);
			AssertJsonEqual(expectedUnpostedConsolCosts, result);
		}

		public void TestGetChargeAccrualsShouldReturnAccrualsWhenBothChargeAccrualsAndConsolCosts()
		{
			var accruals = GetSampleChargeAccruals(sampleGetAccrualsRequest);
			var allConsolCostsIncludingPosted = GetSampleConsolCostsIncludingPosted(sampleGetAccrualsRequest);
			var expectedAccruals = accruals.Select(controller.ConvertChargeToAccrualDetails).ToList();
			var expectedUnpostedConsolCosts = allConsolCostsIncludingPosted.Select(controller.ConvertConsolCostsToAccrualDetails).ToList();
			var expectedResults = expectedAccruals.Concat(expectedUnpostedConsolCosts).ToList();

			mockReconciliationService.Setup(s => s.GetChargeAccruals(It.IsAny<List<JobParentInfo>>(), It.IsAny<ZGuid>())).Returns(accruals);
			mockReconciliationService.Setup(s => s.GetConsolCostAccruals(It.IsAny<List<JobParentInfo>>(), It.IsAny<ZGuid>())).Returns(allConsolCostsIncludingPosted);

			var result = controller.GetAccruals(sampleGetAccrualsRequest).JsonResult<IEnumerable<JobDetailsWithAccrualsAndRelatedJobs>>(HttpStatusCode.OK);

			AssertNotNull(result);
			AssertJsonEqual(expectedResults, result);
		}

		public void TestGetChargeAccrualsShouldReturnAccrualsWithRelatedJobs()
		{
			var accruals = GetSampleChargeAccrualsWithRelatedJobs(sampleGetAccrualsRequest);
			var expectedAccruals = accruals.Select(controller.ConvertChargeToAccrualDetails).ToList();
			var expectedResults = controller.ConvertAccrualsWithRelatedJobs(expectedAccruals, sampleGetAccrualsRequest.JobParentsInfo).ToList();

			mockReconciliationService.Setup(s => s.GetChargeAccruals(It.IsAny<List<JobParentInfo>>(), It.IsAny<ZGuid>())).Returns(accruals);
			mockReconciliationService.Setup(s => s.GetConsolCostAccruals(It.IsAny<List<JobParentInfo>>(), It.IsAny<ZGuid>())).Returns([]);

			var result = controller.GetAccruals(sampleGetAccrualsRequest).JsonResult<IEnumerable<JobDetailsWithAccrualsAndRelatedJobs>>(HttpStatusCode.OK);

			AssertNotNull(result);
			AssertJsonEqual(expectedResults, result);
		}

		public void TestConvertAccrualsWithRelatedJobsShouldReturnParentJobsWithRelatedJobs()
		{
			var parentJobId = Guid.NewGuid();

			var accruals = new List<JobDetailsWithAccrualsAndRelatedJobs>
			{
				new JobDetailsWithAccrualsAndRelatedJobs
				{
					JobParentId = parentJobId,
					JobNumber = "C0001",
					JobParentTableCode = "PTC01",
					AccrualType = AccrualSourceTypes.Consol,
					IsRelatedJob = false,
					ConsolPk = null,
					Accruals = new List<AccrualDetails>()
				},
				new JobDetailsWithAccrualsAndRelatedJobs
				{
					JobParentId = Guid.NewGuid(),
					JobNumber = "J0001",
					JobParentTableCode = "PTC02",
					AccrualType = AccrualSourceTypes.Job,
					IsRelatedJob = true,
					ConsolPk = parentJobId,
					Accruals = new List<AccrualDetails>()
				},
				new JobDetailsWithAccrualsAndRelatedJobs
				{
					JobParentId = Guid.NewGuid(),
					JobNumber = "J0002",
					JobParentTableCode = "PTC03",
					AccrualType = AccrualSourceTypes.Job,
					IsRelatedJob = true,
					ConsolPk = parentJobId,
					Accruals = new List<AccrualDetails>()
				}
			};

			var jobParentInfos = new List<JobParentInfo>
			{
				new JobParentInfo
				{
					ParentId = parentJobId,
					ParentTableCode = "PTC01"
				},
				new JobParentInfo
				{
					ParentId = Guid.NewGuid(),
					ParentTableCode = "PTC03"
				}
			};

			var result = controller.ConvertAccrualsWithRelatedJobs(accruals, jobParentInfos);

			AssertNotNull(result);
			AssertEquals(2, result.Count());
			var parentJob = result.FirstOrDefault(x => x.JobParentTableCode == "PTC01");
			AssertNotNull(parentJob);
			AssertEquals(2, parentJob.RelatedJobs.Count);
			var relatedJob1 = parentJob.RelatedJobs.FirstOrDefault(x => x.JobNumber == "J0001");
			AssertNotNull(relatedJob1);
			AssertEquals("PTC02", relatedJob1.ParentTableCode);
			var relatedJob2 = parentJob.RelatedJobs.FirstOrDefault(x => x.JobNumber == "J0002");
			AssertNotNull(relatedJob2);
			AssertEquals("PTC03", relatedJob2.ParentTableCode);

			var missingParentJob = result.FirstOrDefault(x => x.JobParentTableCode == "PTC03");
			AssertNotNull(missingParentJob);
		}

		public void TestConvertAccrualsWithRelatedJobsShouldReturnParentJobsOnlyWhenNoAccrualsNoRelatedJobs()
		{
			var accruals = new List<JobDetailsWithAccrualsAndRelatedJobs>
			{
				new JobDetailsWithAccrualsAndRelatedJobs
				{
					JobParentId = Guid.NewGuid(),
					JobNumber = "C0001",
					JobParentTableCode = "PTC01",
					AccrualType = AccrualSourceTypes.Consol,
					IsRelatedJob = false,
					ConsolPk = null,
					Accruals = new List<AccrualDetails>()
				},
				new JobDetailsWithAccrualsAndRelatedJobs
				{
					JobParentId = Guid.NewGuid(),
					JobNumber = "J0001",
					JobParentTableCode = "PTC02",
					AccrualType = AccrualSourceTypes.Job,
					IsRelatedJob = false,
					ConsolPk = null,
					Accruals = new List<AccrualDetails>()
				}
			};

			var jobParentInfos = new List<JobParentInfo>
			{
				new JobParentInfo
				{
					ParentId = accruals[0].JobParentId.ToGuid(),
					ParentTableCode = "PTC01"
				},
				new JobParentInfo
				{
					ParentId = accruals[1].JobParentId.ToGuid(),
					ParentTableCode = "PTC02"
				}
			};

			var result = controller.ConvertAccrualsWithRelatedJobs(accruals, jobParentInfos);

			AssertNotNull(result);
			AssertEquals(2, result.Count());
			var parentJob1 = result.FirstOrDefault(x => x.JobParentTableCode == "PTC01");
			AssertNotNull(parentJob1);
			var relatedJob1 = parentJob1.RelatedJobs?.FirstOrDefault();
			AssertNull(relatedJob1);

			var parentJob2 = result.FirstOrDefault(x => x.JobParentTableCode == "PTC02");
			AssertNotNull(parentJob2);
			var relatedJob2 = parentJob2.RelatedJobs?.FirstOrDefault();
			AssertNull(relatedJob2);
		}

		public void TestGetChargeAccrualsShouldReturnErrorWhenRequestBodyIsNull()
		{
			var result = controller.GetAccruals(null);

			AssertHelper.AssertModelState(nameof(GetAccrualsRequest), "Request body cannot be null.", result);
		}

		public void TestGetChargeAccrualsShouldReturnErrorWhenCompanyPKIsEmpty()
		{
			var sampleGetChargeAccrualsRequestWithEmptyCompanyPK = new GetAccrualsRequest
			{
				JobParentsInfo =
				[
					new() { ParentId = sampleJobParentIds[0], ParentTableCode = "JS" },
				],
				CompanyPK = Guid.Empty,
			};
			var result = controller.GetAccruals(sampleGetChargeAccrualsRequestWithEmptyCompanyPK);

			AssertHelper.AssertModelState(nameof(GetAccrualsRequest.CompanyPK), "CompanyPK cannot be an empty GUID.", result);
		}

		public void TestGetChargeAccrualsShouldReturnErrorWhenInvalidJobParentInfo()
		{
			var invalidJobParentInfos = new List<List<JobParentInfo>> { null, new(), };
			foreach (var invalidParentInfo in invalidJobParentInfos)
			{
				var invalidRequest = new GetAccrualsRequest
				{
					JobParentsInfo = invalidParentInfo,
					CompanyPK = sampleCompanyPK
				};

#if NETFRAMEWORK
				var controller = new AccountingReconciliationController(mockReconciliationService.Object);
#elif NET
				var controller = new AccountingReconciliationController(mockReconciliationService.Object, mockHttpContextAccessor.Object);
#endif
				var result = controller.GetAccruals(invalidRequest);

				AssertHelper.AssertModelState(nameof(GetAccrualsRequest.JobParentsInfo), "At least one JobParentInfo is required.", result);
			}
		}

		public void TestGetChargeAccrualsShouldReturnErrorWhenParentTableCodeIsInvalid()
		{
			const string modelStateKey = "JobParentsInfo[0].ParentTableCode";

			var invalidParentTableCodes = new List<string> { null, string.Empty, "A" };
			foreach (var invalidParentTableCode in invalidParentTableCodes)
			{
				var invalidRequest = new GetAccrualsRequest
				{
					JobParentsInfo =
					[
						new JobParentInfo { ParentId = sampleJobParentIds[0], ParentTableCode = invalidParentTableCode }
					],
					CompanyPK = sampleCompanyPK
				};

#if NETFRAMEWORK
				var controllerContext = new HttpControllerContext
				{
					Controller = controller,
					Request = new HttpRequestMessage(),
					Configuration = new HttpConfiguration()
				};
				controller.ControllerContext = controllerContext;
				controller.Validate(invalidRequest);
				var result = controller.GetAccruals(invalidRequest) as InvalidModelStateResult;
				var errors = result.ModelState[modelStateKey].Errors;
#elif NET
				var services = new ServiceCollection();
				services.AddControllers();
				var serviceProvider = services.BuildServiceProvider();

				controller.ControllerContext = new ControllerContext
				{
					ActionDescriptor = new ControllerActionDescriptor(),
					HttpContext = new DefaultHttpContext
					{
						RequestServices = serviceProvider
					}
				};

				controller.ObjectValidator = serviceProvider.GetRequiredService<IObjectModelValidator>();

				controller.TryValidateModel(invalidRequest);

				var result = controller.GetAccruals(invalidRequest) as BadRequestObjectResult;

				var errorMessage = ((result.Value as SerializableError)?[modelStateKey] as string[])?.FirstOrDefault();

				var errors = new ModelErrorCollection();
				if (!string.IsNullOrEmpty(errorMessage))
				{
					errors.Add(errorMessage);
				}

#endif
				if (invalidParentTableCode.IsNullOrEmpty())
				{
					AssertCollectionContains(errors, e => e.ErrorMessage == "ParentTableCode is required.");
				}
				else
				{
					AssertCollectionContains(errors, e => e.ErrorMessage == "ParentTableCode must be at least 2 characters long.");
				}
			}
		}

		public void TestGetChargeAccrualsShouldReturnInternalServerErrorWhenServiceThrowsException()
		{
			mockReconciliationService.Setup(s => s.GetChargeAccruals(It.IsAny<List<JobParentInfo>>(), It.IsAny<ZGuid>()))
				.Throws(new Exception("Unexpected error occurred"));

			var result = controller.GetAccruals(sampleGetAccrualsRequest).GetException();

			AssertNotNull(result);
			AssertEquals("Unexpected error occurred", result.Message);
		}

		void AssertJsonEqual(object expected, object actual)
		{
			AssertEquals(
				JsonConvert.SerializeObject(expected, Formatting.Indented),
				JsonConvert.SerializeObject(actual, Formatting.Indented)
			);
		}

		internal List<AccrualsForAPReconciliationWithConsolInfo<Charge>> GetSampleChargeAccruals(GetAccrualsRequest accrualsAndRelatedJobsRequest)
		{
			return
			[
				new()
				{
					JobParentId = accrualsAndRelatedJobsRequest.JobParentsInfo[0].ParentId,
					JobParentTableCode = accrualsAndRelatedJobsRequest.JobParentsInfo[0].ParentTableCode,
					AccrualType = AccrualSourceTypes.Job,
					Accruals =
					[
						GetSampleCharge("Charge 1", "AUD", 100, 1, 100, 10),
						GetSampleCharge("Charge 2", "AUD", 200, 1, 200, 20),
					],
				},
				new() {
					JobParentId = accrualsAndRelatedJobsRequest.JobParentsInfo[1].ParentId,
					JobParentTableCode = accrualsAndRelatedJobsRequest.JobParentsInfo[1].ParentTableCode,
					AccrualType = AccrualSourceTypes.Job,
					Accruals =
					[
						GetSampleCharge("Charge 1", "AUD", 100, 1, 100, 10),
						GetSampleCharge("Charge 2", "AUD", 200, 1, 200, 20),
					],
				},
				new() {
					JobParentId = accrualsAndRelatedJobsRequest.JobParentsInfo[2].ParentId,
					JobParentTableCode = accrualsAndRelatedJobsRequest.JobParentsInfo[2].ParentTableCode,
					AccrualType = AccrualSourceTypes.Consol,
					Accruals =
					[
						GetSampleCharge("Charge 1", "AUD", 100, 1, 100, 10),
						GetSampleCharge("Charge 2", "AUD", 200, 1, 200, 20),
					],
				},
				new() {
					JobParentId = accrualsAndRelatedJobsRequest.JobParentsInfo[3].ParentId,
					JobParentTableCode = accrualsAndRelatedJobsRequest.JobParentsInfo[3].ParentTableCode,
					AccrualType = AccrualSourceTypes.Consol,
					Accruals =
					[
						GetSampleCharge("Charge 1", "AUD", 100, 1, 100, 10),
						GetSampleCharge("Charge 2", "AUD", 200, 1, 200, 20),
					],
				},
			];
		}

		internal List<AccrualsForAPReconciliationWithConsolInfo<Charge>> GetSampleChargeAccrualsWithRelatedJobs(GetAccrualsRequest accrualsAndRelatedJobsRequest)
		{
			return
			[
				new()
				{
					JobParentId = accrualsAndRelatedJobsRequest.JobParentsInfo[0].ParentId,
					JobParentTableCode = accrualsAndRelatedJobsRequest.JobParentsInfo[0].ParentTableCode,
					AccrualType = AccrualSourceTypes.Job,
					ConsolPk = accrualsAndRelatedJobsRequest.JobParentsInfo[2].ParentId,
					IsRelatedJob = true,
					Accruals =
					[
						GetSampleCharge("Charge 1", "AUD", 100, 1, 100, 10),
						GetSampleCharge("Charge 2", "AUD", 200, 1, 200, 20),
					],
				},
				new() {
					JobParentId = accrualsAndRelatedJobsRequest.JobParentsInfo[1].ParentId,
					JobParentTableCode = accrualsAndRelatedJobsRequest.JobParentsInfo[1].ParentTableCode,
					AccrualType = AccrualSourceTypes.Job,
					ConsolPk = accrualsAndRelatedJobsRequest.JobParentsInfo[3].ParentId,
					IsRelatedJob = true,
					Accruals =
					[
						GetSampleCharge("Charge 1", "AUD", 100, 1, 100, 10),
						GetSampleCharge("Charge 2", "AUD", 200, 1, 200, 20),
					],
				},
				new() {
					JobParentId = accrualsAndRelatedJobsRequest.JobParentsInfo[2].ParentId,
					JobParentTableCode = accrualsAndRelatedJobsRequest.JobParentsInfo[2].ParentTableCode,
					AccrualType = AccrualSourceTypes.Consol,
					Accruals =
					[
						GetSampleCharge("Charge 1", "AUD", 100, 1, 100, 10),
						GetSampleCharge("Charge 2", "AUD", 200, 1, 200, 20),
					],
				},
				new() {
					JobParentId = accrualsAndRelatedJobsRequest.JobParentsInfo[3].ParentId,
					JobParentTableCode = accrualsAndRelatedJobsRequest.JobParentsInfo[3].ParentTableCode,
					AccrualType = AccrualSourceTypes.Consol,
					Accruals =
					[
						GetSampleCharge("Charge 1", "AUD", 100, 1, 100, 10),
						GetSampleCharge("Charge 2", "AUD", 200, 1, 200, 20),
					],
				},
			];
		}

		internal List<AccrualsForAPReconciliationWithConsolInfo<JobConsolCost>> GetSampleConsolCostsIncludingPosted(GetAccrualsRequest accrualsAndRelatedJobsRequest)
		{
			return
			[
				new()
				{
					JobParentId = accrualsAndRelatedJobsRequest.JobParentsInfo[2].ParentId,
					JobParentTableCode = accrualsAndRelatedJobsRequest.JobParentsInfo[2].ParentTableCode,
					AccrualType = AccrualSourceTypes.Consol,
					Accruals =
					[
						GetSampleConsolCost("Consol Cost1", "AUD", 100, 1, 100, 10),
						GetSampleConsolCost("Consol Cost2", "AUD", 200, 1, 200, 20),
						GetSampleConsolCost("Consol Cost3", "AUD", 300, 1, 300, 30, isPosted: true),
					],
				},
				new() {
					JobParentId = accrualsAndRelatedJobsRequest.JobParentsInfo[3].ParentId,
					JobParentTableCode = accrualsAndRelatedJobsRequest.JobParentsInfo[3].ParentTableCode,
					AccrualType = AccrualSourceTypes.Consol,
					Accruals =
					[
						GetSampleConsolCost("Consol Cost1", "AUD", 100, 1, 100, 10),
						GetSampleConsolCost("Consol Cost2", "AUD", 200, 1, 200, 20),
						GetSampleConsolCost("Consol Cost3", "AUD", 300, 1, 300, 30, isPosted: true),
					],
				},
			];
		}

		Charge GetSampleCharge(string desc, string costCurrency, decimal costAmt, decimal costExRate, decimal localCostAmt, decimal taxAmt)
		{
			var charge = Factory.New<Charge>();
			charge.JR_Desc = desc;
			charge.JR_RX_NKCostCurrency = costCurrency;
			charge.JR_OSCostAmt = costAmt;
			charge.JR_OSCostExRate = costExRate;
			charge.JR_LocalCostAmt = localCostAmt;
			charge.JR_OSCostGSTAmt = taxAmt;
			return charge;
		}

		JobConsolCost GetSampleConsolCost(string desc, string osCurrency, decimal costAmt, decimal exRate, decimal localCostAmt, decimal taxAmt, bool isPosted = false)
		{
			var consolCost = Factory.New<JobConsolCost>();
			consolCost.E6_Description = desc;
			consolCost.E6_RX_NKCurrency = osCurrency;
			consolCost.E6_OSCostAmount = costAmt;
			consolCost.E6_ExchangeRate = exRate;
			consolCost.E6_LocalCostAmount = localCostAmt;
			consolCost.E6_OSGSTAmount = taxAmt;
			consolCost.E6_AH_APInvoice = isPosted ? Guid.NewGuid() : Guid.Empty;
			return consolCost;
		}

		protected override void SetUp()
		{
			base.SetUp();
			mockReconciliationService = new Mock<IAPReconciliationService>();
#if NETFRAMEWORK
			controller = new AccountingReconciliationController(mockReconciliationService.Object);
#elif NET
			mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
			controller = new AccountingReconciliationController(mockReconciliationService.Object, mockHttpContextAccessor.Object);
#endif
			sampleJobParentIds = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
			sampleCompanyPK = Guid.NewGuid();
			sampleGetAccrualsRequest = new GetAccrualsRequest
			{
				JobParentsInfo =
				[
					new() { ParentId = sampleJobParentIds[0], ParentTableCode = "JS" },
					new() { ParentId = sampleJobParentIds[1], ParentTableCode = "JS" },
					new() { ParentId = sampleJobParentIds[2], ParentTableCode = "JK" },
					new() { ParentId = sampleJobParentIds[3], ParentTableCode = "JK" },
				],
				CompanyPK = sampleCompanyPK,
			};
		}

		protected override void TearDown()
		{
#if NETFRAMEWORK
			controller?.Dispose();
#endif
			base.TearDown();
		}

		Mock<IAPReconciliationService> mockReconciliationService;
#if NET
		Mock<IHttpContextAccessor> mockHttpContextAccessor;
#endif
		AccountingReconciliationController controller;
		List<Guid> sampleJobParentIds;
		Guid sampleCompanyPK;
		GetAccrualsRequest sampleGetAccrualsRequest;
	}
}
