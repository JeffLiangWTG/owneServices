using System;
using System.Linq;
using CargoWise.RefDbRepo.NewSafeDataUpdateService.Controllers;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	[TestFixture]
	class RefShippingLineEBLProviderControllerFixture
	{
		[Test]
		public void GetEblProviderDistinctByNames()
		{
			var eblProvider1 = new RefShippingLineEBLProvider
			{
				RSE_IsAvailable = true,
				RSE_IsDefault = true,
				RSE_Name = "Name",
				RSE_PK = Guid.NewGuid(),
				RSE_RSL_ShippingLine = Guid.NewGuid()
			};
			var eblProvider2 = new RefShippingLineEBLProvider
			{
				RSE_IsAvailable = true,
				RSE_IsDefault = true,
				RSE_Name = "Name",
				RSE_PK = Guid.NewGuid(),
				RSE_RSL_ShippingLine = Guid.NewGuid()
			};
			var eblProvider3 = new RefShippingLineEBLProvider
			{
				RSE_IsAvailable = true,
				RSE_IsDefault = true,
				RSE_Name = "Name2",
				RSE_PK = Guid.NewGuid(),
				RSE_RSL_ShippingLine = Guid.NewGuid()
			};
			var eblProvider4 = new RefShippingLineEBLProvider
			{
				RSE_IsAvailable = true,
				RSE_IsDefault = true,
				RSE_Name = "Name2",
				RSE_PK = Guid.NewGuid(),
				RSE_RSL_ShippingLine = Guid.NewGuid()
			};
			var eblProvider5 = new RefShippingLineEBLProvider
			{
				RSE_IsAvailable = true,
				RSE_IsDefault = true,
				RSE_Name = "Name3",
				RSE_PK = Guid.NewGuid(),
				RSE_RSL_ShippingLine = Guid.NewGuid()
			};

			var repo = new Mock<IReferenceDataRepository>();
			repo.Setup(x => x.Get<RefShippingLineEBLProvider>()).Returns(new[] { eblProvider1, eblProvider2, eblProvider3, eblProvider4, eblProvider5 }.AsQueryable());

			var controller = new RefShippingLineEBLProviderController(repo.Object);
			var result = controller.GetDistinctNames();
			Assert.That(result.Count(), Is.EqualTo(3));
			Assert.That(result.Contains(eblProvider1.RSE_Name));
			Assert.That(result.Contains(eblProvider3.RSE_Name));
			Assert.That(result.Contains(eblProvider5.RSE_Name));
		}
	}
}
