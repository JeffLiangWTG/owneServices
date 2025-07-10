using System;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	public static class IntegrationTestHelper
	{
		/// <summary>
		/// IMPORTANT!!!
		/// Please dispose it whenever use it
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "It will be disposed after each test runs")]
		public static WebApplicationFactory<Program> WebAppFactory
		{
			get
			{
				return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
				{
					builder.UseContentRoot(Directory.GetCurrentDirectory());
					builder.ConfigureTestServices(services =>
					{
						services.AddScoped(sp => SetupRepoMock().Object);
					});
				});
			}
		}

		static Mock<IReferenceDataRepository> SetupRepoMock()
		{
			var countryPk = Guid.NewGuid();
			var accTaxRatePk = Guid.Parse("7EDAE0A0-06E1-43C9-A8D9-33635B66F038");
			var cusTaxOrFee = Guid.Parse("ab07a112-8d73-40fb-8e68-0dce92a4ad84");
			var country = new RefCountry
			{
				RN_PK = countryPk,
				RN_Code = "AA",
				RN_Desc = "AA",
				RN_ValidationStatus = "NAV"
			};
			var accTaxRate = new RefAccTaxRate
			{
				ZAT_PK = accTaxRatePk,
				ZAT_RN_NKCountry = "AA",
				ZAT_ReferenceRateType = "STD"
			};

			var refDbVersionControl1 = new RefDbVersionControl
			{
				RVC_ParentPK = countryPk,
				RVC_ParentCode = "RN",
				RVC_IsPublished = true
			};

			var refDbVersionControl2 = new RefDbVersionControl
			{
				RVC_ParentPK = accTaxRatePk,
				RVC_ParentCode = "ZAT",
				RVC_IsPublished = true
			};

			var refCustTaxOrTee = new RefCusTaxOrFee
			{
				ZZF_PK = cusTaxOrFee,
				ZZF_Code = "LNT",
				ZZF_Description = "LCT Normal Vehicle Threshold",
				ZZF_ZZZ_NKDataGrouping = "AU",
				ZZF_ZX0_NKTaxOrFeeType = "VAT"
			};

			var repo = new Mock<IReferenceDataRepository>();
			repo.Setup(x => x.Get<RefCountry>()).Returns(new[] { country }.AsQueryable());
			repo.Setup(x => x.Get<RefAccTaxRate>()).Returns(new[] { accTaxRate }.AsQueryable());
			repo.Setup(x => x.Get<RefDbVersionControl>()).Returns(new[] { refDbVersionControl1, refDbVersionControl2 }.AsQueryable());
			repo.Setup(x => x.Get<RefCusTaxOrFee>()).Returns(new[] { refCustTaxOrTee }.AsQueryable());
			return repo;
		}
	}
}
