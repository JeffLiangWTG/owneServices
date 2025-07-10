using System;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schema_New.Test
{
	[TestFixture]
	class RefCusTariffNationalCodeFixture
	{
		[Test]
		public void BuildRefCusRateApplicabilities_RefCusTariffNationalCode()
		{
			var nationalCode = new RefCusTariffNationalCode { ZZW_PK = Guid.NewGuid() };
			var rate = new RefCusRate() { ZZ2_PK = Guid.NewGuid(), ZZ2_ZZW_TariffNationalCode = nationalCode.ZZW_PK };
			nationalCode.RefCusRates.Add(rate);
			var app = new RefCusApplicability();
			rate.RefCusApplicabilities.Add(app);
			nationalCode.BuildNonPersistentObjects();

			Assert.That(nationalCode.RefCusRateApplicabilities.Single().S01_ZZW_TariffNationalCode == nationalCode.ZZW_PK);
		}

		[Test]
		public void BuildRefCusRateApplicabilities_RefCusTariffNationalCode_Multi()
		{
			var nationalCode = new RefCusTariffNationalCode { ZZW_PK = Guid.NewGuid() };
			var rate1 = new RefCusRate() { ZZ2_PK = Guid.NewGuid(), ZZ2_ZZW_TariffNationalCode = nationalCode.ZZW_PK };
			var rate2 = new RefCusRate() { ZZ2_PK = Guid.NewGuid(), ZZ2_ZZW_TariffNationalCode = nationalCode.ZZW_PK };
			nationalCode.RefCusRates.Add(rate1);
			nationalCode.RefCusRates.Add(rate2);

			for (int i = 0; i < 3; i++)
			{
				var app = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
				rate1.RefCusApplicabilities.Add(app);
			}
			for (int i = 0; i < 3; i++)
			{
				var app = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
				rate2.RefCusApplicabilities.Add(app);
			}

			nationalCode.BuildNonPersistentObjects();

			Assert.That(nationalCode.RefCusRateApplicabilities.Count == 6);
			Assert.That(nationalCode.RefCusRateApplicabilities.All(x => x.S01_ZZW_TariffNationalCode == nationalCode.ZZW_PK));
		}
	}
}
