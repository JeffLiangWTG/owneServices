using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.NewSafeDataUpdateService.Controllers;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	[TestFixture]
	class DataSetHeaderControllerFixture
	{
		[Test]
		public void GetIncludeDeletedForUserViews()
		{
			var shippingLine1PK = Guid.NewGuid();
			var shippingLine2PK = Guid.NewGuid();
			var shippingLineUserView1 = new RefShippingLineUserView
			{
				RSL_PK = shippingLine1PK
			};
			var shippingLineUserView2 = new RefShippingLineUserView
			{
				RSL_PK = shippingLine2PK
			};
			var shippingLine1 = new RefShippingLine
			{
				RSL_PK = shippingLine1PK
			};
			var shippingLine2 = new RefShippingLine
			{
				RSL_PK = shippingLine2PK
			};
			var version1 = new RefDbVersionControl
			{
				RVC_LastUpdatedUTC = DateTime.UtcNow,
				RVC_ParentCode = "RSL",
				RVC_IsPublished = true,
				RVC_Deleted = true,
				RVC_ParentPK = shippingLine1PK
			};
			var version2 = new RefDbVersionControl
			{
				RVC_LastUpdatedUTC = DateTime.UtcNow,
				RVC_ParentCode = "RSL",
				RVC_IsPublished = true,
				RVC_Deleted = false,
				RVC_ParentPK = shippingLine2PK
			};

			var repo = new Mock<IReferenceDataRepository>();
			repo.Setup(x => x.Get<RefDbVersionControl>()).Returns(new[] { version1, version2 }.AsQueryable());
			repo.Setup(x => x.Get<RefShippingLineUserView>()).Returns(new[] { shippingLineUserView1, shippingLineUserView2 }.AsQueryable());
			repo.Setup(x => x.Get<RefShippingLine>()).Returns(new[] { shippingLine1, shippingLine2 }.AsQueryable());

			var controllerContext = SafeDataControllerFixture.SetupControllerContext(repo.Object, new DefaultHttpContext().Request);
			var userViewController = new RefShippingLineUserViewUpdateController(new Mock<IAuthorizationHelper>().Object) { ControllerContext = controllerContext };
			var userViewResult = userViewController.Get();
			Assert.That(userViewResult.Count(), Is.EqualTo(2));
			Assert.That(userViewResult.Any(x => x.RSL_PK == shippingLine1PK));
		}

		[Test]
		public void GetLatestDataSetUpdatedTimeUTC()
		{
			var result = controller.GetLatestUpdatedTimeUTC();
			Assert.AreEqual(now.AddDays(-1), result);
		}

		[Test]
		public void GetCreatedTimeUTC()
		{
			var result = controller.GetLastestCreatedTimeUTC();
			Assert.AreEqual(now.AddDays(-1), result);
		}

		[TestCase(null, 0, true)]
		[TestCase(-1, 0, true)]
		[TestCase(-2, -1, false)]
		[TestCase(1, 2, false)]
		public void GetCreatedBetween(int? beforeDays, int afterDays, bool notNull)
		{
			var accTaxRate1 = new RefAccTaxRate
			{
				ZAT_PK = accTaxRate1PK
			};
			var version1 = new RefDbVersionControl
			{
				RVC_ParentCode = "ZAT",
				RVC_ParentPK = accTaxRate1.ZAT_PK,
				RVC_CreatedTimeUTC = now
			};
			var accTaxRate2 = new RefAccTaxRate
			{
				ZAT_PK = accTaxRate2PK
			};
			var version2 = new RefDbVersionControl
			{
				RVC_ParentCode = "ZAT",
				RVC_ParentPK = accTaxRate2.ZAT_PK,
				RVC_CreatedTimeUTC = now,
				RVC_Deleted = true
			};
			var repo = new Mock<IReferenceDataRepository>();
			repo.Setup(x => x.Get<RefDbVersionControl>()).Returns(new[] { version1, version2 }.AsQueryable());
			repo.Setup(x => x.Get<RefAccTaxRate>()).Returns(new[] { accTaxRate1, accTaxRate2 }.AsQueryable());
			var controllerContext = new ControllerContext();
			var httpContext = new Mock<HttpContext>();
			object sharedRepo = Tuple.Create(repo.Object, false);
			httpContext.Setup(x => x.Items.TryGetValue("Batch_DbContext", out sharedRepo)).Returns(true);
			controllerContext.HttpContext = httpContext.Object;

			var controller = new RefAccTaxRateUpdateController(new Mock<IAuthorizationHelper>().Object) { ControllerContext = controllerContext };
			var before = beforeDays.HasValue ? (DateTime?)now.AddDays(beforeDays.Value) : null;
			var result = controller.GetCreatedBetween(before, now.AddDays(afterDays)).FirstOrDefault();
			if (notNull)
			{
				Assert.NotNull(result);
			}
			else
			{
				Assert.Null(result);
			}
		}

		[TestCase(null, 0, true)]
		[TestCase(-1, 0, true)]
		[TestCase(-2, -1, false)]
		[TestCase(1, 2, false)]
		[TestCase(-1, 2, true)]
		public void GetModifiedBetween(int? beforeDays, int afterDays, bool notNull)
		{
			var accTaxRate1 = new RefAccTaxRate
			{
				ZAT_PK = accTaxRate1PK
			};
			var version1 = new RefDbVersionControl
			{
				RVC_ParentPK = accTaxRate1.ZAT_PK,
				RVC_LastUpdatedUTC = now
			};
			var accTaxRate2 = new RefAccTaxRate
			{
				ZAT_PK = accTaxRate2PK
			};
			var version2 = new RefDbVersionControl
			{
				RVC_ParentPK = accTaxRate2.ZAT_PK,
				RVC_LastUpdatedUTC = now.AddDays(afterDays),
				RVC_Deleted = true
			};
			var repo = new Mock<IReferenceDataRepository>();
			repo.Setup(x => x.Get<RefDbVersionControl>()).Returns(new[] { version1, version2 }.AsQueryable());
			repo.Setup(x => x.Get<RefAccTaxRate>()).Returns(new[] { accTaxRate1, accTaxRate2 }.AsQueryable());
			var controllerContext = new ControllerContext();
			var httpContext = new Mock<HttpContext>();
			object sharedRepo = Tuple.Create(repo.Object, false);
			httpContext.Setup(x => x.Items.TryGetValue("Batch_DbContext", out sharedRepo)).Returns(true);
			controllerContext.HttpContext = httpContext.Object;

			var controller = new RefAccTaxRateUpdateController(new Mock<IAuthorizationHelper>().Object) { ControllerContext = controllerContext };
			var before = beforeDays.HasValue ? (DateTime?)now.AddDays(beforeDays.Value) : null;
			var result = controller.GetModifiedBetween(before, now.AddDays(afterDays)).FirstOrDefault();
			if (notNull)
			{
				Assert.NotNull(result);
			}
			else
			{
				Assert.Null(result);
			}
		}

		[TestCase(null, 2, 3)]
		[TestCase(-3, -1, 1)]
		[TestCase(-1, 1, 1)]
		[TestCase(1, 3, 1)]
		[TestCase(-3, 3, 3)]
		public void GetModifiedBetween_UserView(int? beforeDays, int afterDays, int count)
		{
			var now = DateTime.Now;
			var shippingLine1 = new RefShippingLineUserView
			{
				RSL_PK = Guid.NewGuid()
			};
			var version1 = new RefDbVersionControl
			{
				RVC_ParentPK = shippingLine1.RSL_PK,
				RVC_LastUpdatedUTC = now.AddDays(-2),
				RVC_Deleted = true
			};
			var shippingLine2 = new RefShippingLineUserView
			{
				RSL_PK = Guid.NewGuid()
			};
			var version2 = new RefDbVersionControl
			{
				RVC_ParentPK = shippingLine2.RSL_PK,
				RVC_LastUpdatedUTC = now
			};
			var shippingLine3 = new RefShippingLineUserView
			{
				RSL_PK = Guid.NewGuid()
			};
			var version3 = new RefDbVersionControl
			{
				RVC_ParentPK = shippingLine3.RSL_PK,
				RVC_LastUpdatedUTC = now.AddDays(2),
				RVC_Deleted = true
			};

			var repo = new Mock<IReferenceDataRepository>();
			repo.Setup(x => x.Get<RefDbVersionControl>()).Returns(new[] { version1, version2, version3 }.AsQueryable());
			repo.Setup(x => x.Get<RefShippingLineUserView>()).Returns(new[] { shippingLine1, shippingLine2, shippingLine3 }.AsQueryable());
			var controllerContext = SafeDataControllerFixture.SetupControllerContext(repo.Object);
			var controller = new RefShippingLineUserViewUpdateController(new Mock<IAuthorizationHelper>().Object) { ControllerContext = controllerContext };
			var before = beforeDays.HasValue ? (DateTime?)now.AddDays(beforeDays.Value) : null;
			var result = controller.GetModifiedBetween(before, now.AddDays(afterDays));
			Assert.AreEqual(count, result.Count());
		}

		RefAccTaxRate[] GetRefAccTaxRates()
		{
			var accTaxRate1 = new RefAccTaxRate
			{
				ZAT_PK = accTaxRate1PK
			};
			var accTaxRate2 = new RefAccTaxRate
			{
				ZAT_PK = accTaxRate2PK
			};
			var accTaxRate3 = new RefAccTaxRate
			{
				ZAT_PK = accTaxRate3PK
			};

			return new[] { accTaxRate1, accTaxRate2, accTaxRate3 };
		}

		RefDbVersionControl[] GetRefDbVersionControls()
		{
			var version1 = new RefDbVersionControl
			{
				RVC_ParentCode = "ZAT",
				RVC_ParentPK = accTaxRate1PK,
				RVC_IsPublished = true,
				RVC_CreatedTimeUTC = now.AddDays(-1),
				RVC_LastUpdatedUTC = now.AddDays(-1)
			};
			var version2 = new RefDbVersionControl
			{
				RVC_ParentCode = "ZAT",
				RVC_ParentPK = accTaxRate2PK,
				RVC_IsPublished = true,
				RVC_CreatedTimeUTC = now.AddDays(-2),
				RVC_LastUpdatedUTC = now.AddDays(-2)
			};
			var version3 = new RefDbVersionControl
			{
				RVC_ParentCode = "ZAT",
				RVC_ParentPK = accTaxRate3PK,
				RVC_Deleted = true,
				RVC_IsPublished = true,
				RVC_CreatedTimeUTC = now,
				RVC_LastUpdatedUTC = now
			};

			return new[] { version1, version2, version3 };
		}

		[SetUp]
		public void SetUp()
		{
			repo = new Mock<IReferenceDataRepository>();
			repo.Setup(x => x.Get<RefAccTaxRate>()).Returns(GetRefAccTaxRates().AsQueryable());
			repo.Setup(x => x.Get<RefDbVersionControl>()).Returns(GetRefDbVersionControls().AsQueryable());

			var controllerContext = SafeDataControllerFixture.SetupControllerContext(repo.Object);
			controller = new RefAccTaxRateUpdateController(new Mock<IAuthorizationHelper>().Object);
			controller.ControllerContext = controllerContext;
		}

		Mock<IReferenceDataRepository> repo;
		RefAccTaxRateUpdateController controller;
		readonly DateTime now = DateTime.Now;
		readonly Guid accTaxRate1PK = Guid.NewGuid();
		readonly Guid accTaxRate2PK = Guid.NewGuid();
		readonly Guid accTaxRate3PK = Guid.NewGuid();
	}
}
