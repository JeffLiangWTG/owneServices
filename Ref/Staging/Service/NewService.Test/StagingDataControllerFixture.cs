using System;
using System.Linq;
using System.Net;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.Staging.NewService.Controllers;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.Data.SqlClient;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.NewService.Test
{
	[TestFixture]
	[TransactionedTestCase]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	class StagingDataControllerFixture
	{
		[Test]
		public void Get()
		{
			var pk1 = Guid.Parse("D64AE9C0-02AD-4B43-8BEA-6654C85B403C");
			var pk2 = Guid.Parse("6C5C2A7E-24FD-4C97-B233-2683A85885E4");
			InsertRefAccTaxRateToDatabase(pk1);
			InsertRefAccTaxRateToDatabase(pk2);
			using (var stagingRepo = new StagingRepository(connectionString))
			{
				var controller = GetRefAccTaxRateUpdateController(stagingRepo);
				var result = controller.Get().ToArray();
				Assert.GreaterOrEqual(result.Length, 2);
				var taxRate = result.First(x => x.ZAT_PK == pk1);
				Assert.AreEqual("AU", taxRate.ZAT_RN_NKCountry);
				Assert.AreEqual("AAA", taxRate.ZAT_ReferenceRateType);

				result = controller.Get(pk2).ToArray();
				Assert.AreEqual(1, result.Length);
				Assert.AreEqual(new DateTime(2000, 1, 1), result[0].ZAT_StartDate);
				Assert.AreEqual(new DateTime(2100, 12, 31), result[0].ZAT_EndDate);
			}
		}

		[Test]
		public void Post()
		{
			var pk = Guid.Parse("4FC70C1E-D1E5-46F1-A153-75823C575DC9");
			var taxRate = new RefAccTaxRate
			{
				ZAT_PK = pk,
				ZAT_RN_NKCountry = "AA",
				ZAT_RateNumerator = 1,
				ZAT_RateDenominator = 2,
				ZAT_ReferenceRateType = "AAA",
				ZAT_StartDate = new DateTime(2023, 1, 1),
				ZAT_EndDate = new DateTime(2023, 12, 31)
			};

			using (var stagingRepo = new StagingRepository(connectionString))
			{
				var controller = GetRefAccTaxRateUpdateController(stagingRepo);
				var result = controller.Post(taxRate);
			}
			using (var stagingRepo = new StagingRepository(connectionString))
			{
				var controller = GetRefAccTaxRateUpdateController(stagingRepo);
				var result = controller.Get(pk).ToArray();
				Assert.AreEqual(1, result.Length);
				Assert.AreEqual("AA", result[0].ZAT_RN_NKCountry);
				Assert.AreEqual("AAA", result[0].ZAT_ReferenceRateType);
				Assert.AreEqual(new DateTime(2023, 1, 1), result[0].ZAT_StartDate);
				Assert.AreEqual(new DateTime(2023, 12, 31), result[0].ZAT_EndDate);
			}
		}

		[Test]
		public void Post_Authorization()
		{
			var accTaxRateController = GetRefAccTaxRateUpdateController(stagingRepo.Object);
			authorizationHelper.Setup(x => x.IsAuthorized(It.IsAny<RefAccTaxRate>(), It.IsAny<string>())).Returns(false);
			var result = accTaxRateController.Post(accTaxRate);
			authorizationHelper.Verify(x => x.IsAuthorized(accTaxRate, null));
			Assert.True(result is UnauthorizedResult);
			authorizationHelper.Setup(x => x.IsAuthorized(It.IsAny<RefAccTaxRate>(), It.IsAny<string>())).Returns(true);
			result = accTaxRateController.Post(accTaxRate);
			Assert.True(result is CreatedODataResult<RefAccTaxRate>);
		}

		[Test]
		public void Put()
		{
			var pk = Guid.Parse("19201056-43BD-4308-8887-9CEB283141DD");
			InsertRefAccTaxRateToDatabase(pk);
			var taxRate = new RefAccTaxRate
			{
				ZAT_PK = pk,
				ZAT_RN_NKCountry = "AU",
				ZAT_ReferenceRateType = "BBB",
				ZAT_RateNumerator = 10,
				ZAT_RateDenominator = 1,
				ZAT_StartDate = new DateTime(2023, 1, 1),
				ZAT_EndDate = new DateTime(2100, 12, 31)
			};

			using (var stagingRepo = new StagingRepository(connectionString))
			{
				var controller = GetRefAccTaxRateUpdateController(stagingRepo);
				var result = controller.Put(pk, taxRate);
			}
			using (var stagingRepo = new StagingRepository(connectionString))
			{
				var controller = GetRefAccTaxRateUpdateController(stagingRepo);
				var result = controller.Get(pk).ToArray();
				Assert.AreEqual(1, result.Length);
				Assert.AreEqual(10, result[0].ZAT_RateNumerator);
				Assert.AreEqual("BBB", result[0].ZAT_ReferenceRateType);
				Assert.AreEqual(taxRate.ZAT_StartDate, result[0].ZAT_StartDate);
				Assert.AreEqual(taxRate.ZAT_EndDate, result[0].ZAT_EndDate);
			}
		}

		[Test]
		public void Put_Authorization()
		{
			var accTaxRateController = GetRefAccTaxRateUpdateController(stagingRepo.Object);
			authorizationHelper.Setup(x => x.IsAuthorized(It.IsAny<RefAccTaxRate>(), It.IsAny<string>())).Returns(false);
			var result = accTaxRateController.Put(pk, accTaxRate);
			authorizationHelper.Verify(x => x.IsAuthorized(accTaxRate, null));
			Assert.True(result is UnauthorizedResult);
			authorizationHelper.Setup(x => x.IsAuthorized(It.IsAny<RefAccTaxRate>(), It.IsAny<string>())).Returns(true);
			result = accTaxRateController.Put(pk, accTaxRate);
			Assert.True(result is UpdatedODataResult<RefAccTaxRate>);
		}

		[Test]
		public void Patch()
		{
			var pk = Guid.Parse("1ED34AE1-AC83-450E-AF3B-B12B8861CF0E");
			InsertRefAccTaxRateToDatabase(pk);
			var taxRate = new RefAccTaxRate
			{
				ZAT_PK = pk,
				ZAT_RN_NKCountry = "AU",
				ZAT_RateNumerator = 1,
				ZAT_RateDenominator = 2,
				ZAT_ReferenceRateType = "CCC",
				ZAT_StartDate = new DateTime(2023, 1, 1),
				ZAT_EndDate = new DateTime(2100, 12, 31)
			};

			using (var stagingRepo = new StagingRepository(connectionString))
			{
				var controller = GetRefAccTaxRateUpdateController(stagingRepo);
				var result = controller.Patch(pk, taxRate);
			}
			using (var stagingRepo = new StagingRepository(connectionString))
			{
				var controller = GetRefAccTaxRateUpdateController(stagingRepo);
				var result = controller.Get(pk).ToArray();
				Assert.AreEqual(1, result.Length);
				Assert.AreEqual(2, result[0].ZAT_RateDenominator);
				Assert.AreEqual("CCC", taxRate.ZAT_ReferenceRateType);
				Assert.AreEqual(taxRate.ZAT_StartDate, result[0].ZAT_StartDate);
				Assert.AreEqual(taxRate.ZAT_EndDate, result[0].ZAT_EndDate);
			}
		}

		[Test]
		public void Patch_Authorization()
		{
			var accTaxRateController = GetRefAccTaxRateUpdateController(stagingRepo.Object);
			authorizationHelper.Setup(x => x.IsAuthorized(It.IsAny<RefAccTaxRate>(), It.IsAny<string>())).Returns(false);
			var result = accTaxRateController.Patch(pk, accTaxRate);
			authorizationHelper.Verify(x => x.IsAuthorized(accTaxRate, null));
			Assert.True(result is UnauthorizedResult);
			authorizationHelper.Setup(x => x.IsAuthorized(It.IsAny<RefAccTaxRate>(), It.IsAny<string>())).Returns(true);
			result = accTaxRateController.Patch(pk, accTaxRate);
			Assert.True(result is UpdatedODataResult<RefAccTaxRate>);
		}

		[Test]
		public void Delete()
		{
			var pk = Guid.Parse("9A4B99EE-6262-46D1-AC7D-A1C35A22522D");
			InsertRefAccTaxRateToDatabase(pk);
			using (var stagingRepo = new StagingRepository(connectionString))
			{
				var controller = GetRefAccTaxRateUpdateController(stagingRepo);
				var result = controller.Delete(pk);
			}
			using (var stagingRepo = new StagingRepository(connectionString))
			{
				var controller = GetRefAccTaxRateUpdateController(stagingRepo);
				var result = controller.Get(pk).FirstOrDefault();
				Assert.Null(result);
			}
		}

		[Test]
		public void Delete_Authorization()
		{
			var stagingRepo = new Mock<IStagingRepository>();
			stagingRepo.Setup(x => x.Get<RefAccTaxRate>()).Returns(new[] { accTaxRate }.AsQueryable());
			var accTaxRateController = GetRefAccTaxRateUpdateController(stagingRepo.Object);
			authorizationHelper.Setup(x => x.IsAuthorized(It.IsAny<RefAccTaxRate>(), It.IsAny<string>())).Returns(false);
			var result = accTaxRateController.Delete(pk);
			authorizationHelper.Verify(x => x.IsAuthorized(accTaxRate, null));
			Assert.AreEqual((int)HttpStatusCode.Unauthorized, ((StatusCodeResult)result).StatusCode);
			authorizationHelper.Setup(x => x.IsAuthorized(It.IsAny<RefAccTaxRate>(), It.IsAny<string>())).Returns(true);
			result = accTaxRateController.Delete(pk);
			Assert.AreEqual((int)HttpStatusCode.NoContent, ((StatusCodeResult)result).StatusCode);
		}

		RefAccTaxRateUpdateController GetRefAccTaxRateUpdateController(IStagingRepository stagingRepo)
		{
			var controller = new RefAccTaxRateUpdateController(authorizationHelper.Object);
			controller.ControllerContext = IntegrationTestHelper.SetupControllerContext(stagingRepo);
			return controller;
		}

		void InsertRefAccTaxRateToDatabase(Guid pk)
		{
			var connection = TestConnectionString.GetAdmin(dbName);
			using (var sqlConnection = new SqlConnection(connection))
			{
				sqlConnection.Open();
				var sql = $@"INSERT INTO RefAccTaxRate (ZAT_PK,ZAT_RN_NKCountry,ZAT_ReferenceRateType,ZAT_StartDate,ZAT_EndDate,ZAT_RateNumerator,ZAT_RateDenominator)
VALUES ('{pk}', 'AU', 'AAA', '2000-01-01', '2100-12-31', 0, 1);";

				using (var command = sqlConnection.CreateCommand())
				{
					command.CommandText = sql;
					command.ExecuteNonQuery();
				}
			}
		}

		[SetUp]
		public void SetUp()
		{
			dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
			connectionString = TestConnectionString.GetAdmin(dbName);
			stagingRepo = new Mock<IStagingRepository>();
			authorizationHelper = new Mock<IAuthorizationHelper>();
			authorizationHelper.Setup(x => x.IsAuthorized(It.IsAny<RefAccTaxRate>(), It.IsAny<string>())).Returns(true);
		}

		string dbName;
		string connectionString;
		Mock<IStagingRepository> stagingRepo;
		Mock<IAuthorizationHelper> authorizationHelper;
		readonly static Guid pk = Guid.Parse("08E9C02B-487B-4005-AE51-A4CA94A30EA8");
		RefAccTaxRate accTaxRate = new RefAccTaxRate { ZAT_PK = pk, ZAT_ReferenceRateType = "AAA", ZAT_RN_NKCountry = "AA" };

	}
}
