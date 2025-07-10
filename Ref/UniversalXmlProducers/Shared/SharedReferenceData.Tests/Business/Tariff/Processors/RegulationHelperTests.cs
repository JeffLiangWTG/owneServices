using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using NUnit.Framework;
using static CargoWise.RefDbRepo.SharedReferenceData.Business.Common.Tests.CommonHelpers;
using static CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Helpers.Tests.TestHelperClasses;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors.Tests
{
	[TestFixture]
	class RegulationHelperTests
	{
		[Test]
		public void UniqueKey()
		{
			var data = new TestTariffModel { RegulationId = "ABC123456", RegulationRoleTypeId = "5" };
			var result = RegulationHelper.GetUniqueKey(data);

			Assert.That(result, Is.EqualTo("ABC123456-5"));
		}

		[Test]
		public void LoadFromReferenceData()
		{
			var errorCollector = new StringBuilder();

			var referenceData = new List<ITariffModel>
			{
				new GoodsNomenclature { ItemId = "GN1" },
				new BaseRegulation { RegulationId = "BR1", RegulationRoleTypeId = "1" },
				new ModificationRegulation { RegulationId = "MR1", RegulationRoleTypeId = "1" },
				new Measure { ItemId = "MS1" },

				new GoodsNomenclature { ItemId = "GN2" },
				new BaseRegulation { RegulationId = "BR2", RegulationRoleTypeId = "1" },
				new ModificationRegulation { RegulationId = "MR2", RegulationRoleTypeId = "1" },
				new Measure { ItemId = "MS2" },

				new BaseRegulation { RegulationId = "BR1", RegulationRoleTypeId = "2" },
				new ModificationRegulation { RegulationId = "MR1", RegulationRoleTypeId = "2" },
			};

			var dateTimeProvider = new DateTimeProvider();
			var helper = new RegulationHelperTester(referenceData, dateTimeProvider, errorCollector);

			Assert.That(helper.BaseRegulations, Is.Not.Null);
			Assert.That(helper.ModificationRegulations, Is.Not.Null);

			Assert.That(helper.BaseRegulations.Count, Is.EqualTo(3));
			Assert.That(helper.ModificationRegulations.Count, Is.EqualTo(3));
		}

		[Test]
		public void LoadFromReferenceDataWithDuplicates()
		{
			var errorCollector = new StringBuilder();

			var referenceData = new List<ITariffModel>
			{
				new BaseRegulation { RegulationId = "BR1", RegulationRoleTypeId = "1" },
				new BaseRegulation { RegulationId = "BR1", RegulationRoleTypeId = "1", HJID = "DUPLICATE" },

				new ModificationRegulation { RegulationId = "MR1", RegulationRoleTypeId = "1" },
				new ModificationRegulation { RegulationId = "MR1", RegulationRoleTypeId = "1" },
			};

			var dateTimeProvider = new DateTimeProvider();
			var helper = new RegulationHelperTester(referenceData, dateTimeProvider, errorCollector);

			Assert.That(helper.BaseRegulations, Is.Not.Null);
			Assert.That(helper.BaseRegulations.Count, Is.EqualTo(1));
			Assert.That(helper.BaseRegulations.ContainsKey("BR1-1"), Is.EqualTo(true));
			Assert.That(helper.BaseRegulations["BR1-1"].HJID, Is.EqualTo("DUPLICATE"));

			Assert.That(helper.ModificationRegulations, Is.Not.Null);
			Assert.That(helper.ModificationRegulations.Count, Is.EqualTo(1));
		}

		[Test]
		public void RegulationIsActive()
		{
			var errorCollector = new StringBuilder();
			var dateTimeProvider = new DateTimeProvider();
			dateTimeProvider.TestDateTime = new DateTime(2020, 09, 01);

			var referenceData = new List<ITariffModel>
			{
				new BaseRegulation { RegulationId = "BR1", RegulationRoleTypeId = "1", StartDate = new DateTime(2000,01,01) },
				new BaseRegulation { RegulationId = "BR2", RegulationRoleTypeId = "1", StartDate = new DateTime(2020,12,31) },
				new BaseRegulation { RegulationId = "BR3", RegulationRoleTypeId = "1", StartDate = new DateTime(2000,01,01), EndDate = new DateTime(2020, 12, 31) },
				new BaseRegulation { RegulationId = "BR4", RegulationRoleTypeId = "1", StartDate = new DateTime(2000,01,01), EndDate = new DateTime(2020, 05, 01) },
				new BaseRegulation { RegulationId = "BR5", RegulationRoleTypeId = "1", StartDate = new DateTime(2000,01,01), EffectiveEndDate = new DateTime(2020, 05, 01) },
				new BaseRegulation { RegulationId = "BR6", RegulationRoleTypeId = "1", StartDate = new DateTime(2000,01,01), EndDate = new DateTime(2020, 05, 01), EffectiveEndDate = new DateTime(2020, 12, 31) },
				new BaseRegulation { RegulationId = "BR7", RegulationRoleTypeId = "1", StartDate = new DateTime(2000,01,01), EndDate = new DateTime(2020, 12, 31), EffectiveEndDate = new DateTime(2020, 05, 01) },
				new BaseRegulation { RegulationId = "BR8", RegulationRoleTypeId = "1", StartDate = new DateTime(2020,10,01) },

				new ModificationRegulation { RegulationId = "MR1", RegulationRoleTypeId = "1", StartDate = new DateTime(2000,01,01) },
				new ModificationRegulation { RegulationId = "MR2", RegulationRoleTypeId = "1", StartDate = new DateTime(2020,12,31) },
				new ModificationRegulation { RegulationId = "MR3", RegulationRoleTypeId = "1", StartDate = new DateTime(2000,01,01), EndDate = new DateTime(2020, 12, 31) },
				new ModificationRegulation { RegulationId = "MR4", RegulationRoleTypeId = "1", StartDate = new DateTime(2000,01,01), EndDate = new DateTime(2020, 05, 01) },
				new ModificationRegulation { RegulationId = "MR5", RegulationRoleTypeId = "1", StartDate = new DateTime(2000,01,01), EffectiveEndDate = new DateTime(2020, 05, 01) },
				new ModificationRegulation { RegulationId = "MR6", RegulationRoleTypeId = "1", StartDate = new DateTime(2000,01,01), EndDate = new DateTime(2020, 05, 01), EffectiveEndDate = new DateTime(2020, 12, 31) },
				new ModificationRegulation { RegulationId = "MR7", RegulationRoleTypeId = "1", StartDate = new DateTime(2000,01,01), EndDate = new DateTime(2020, 12, 31), EffectiveEndDate = new DateTime(2020, 05, 01) },
			};

			var helper = new RegulationHelperTester(referenceData, dateTimeProvider, errorCollector);

			Assert.That(helper.IsRegulationActive("XXX-X"), Is.EqualTo(false));

			Assert.That(helper.IsRegulationActive("BR1-1"), Is.EqualTo(true));
			Assert.That(helper.IsRegulationActive("BR2-1"), Is.EqualTo(false));
			Assert.That(helper.IsRegulationActive("BR3-1"), Is.EqualTo(true));
			Assert.That(helper.IsRegulationActive("BR4-1"), Is.EqualTo(false));
			Assert.That(helper.IsRegulationActive("BR5-1"), Is.EqualTo(false));
			Assert.That(helper.IsRegulationActive("BR6-1"), Is.EqualTo(true));
			Assert.That(helper.IsRegulationActive("BR7-1"), Is.EqualTo(false));
			Assert.That(helper.IsRegulationActive("BR8-1"), Is.EqualTo(true));

			Assert.That(helper.IsRegulationActive("MR1-1"), Is.EqualTo(true));
			Assert.That(helper.IsRegulationActive("MR2-1"), Is.EqualTo(false));
			Assert.That(helper.IsRegulationActive("MR3-1"), Is.EqualTo(true));
			Assert.That(helper.IsRegulationActive("MR4-1"), Is.EqualTo(false));
			Assert.That(helper.IsRegulationActive("MR5-1"), Is.EqualTo(false));
			Assert.That(helper.IsRegulationActive("MR6-1"), Is.EqualTo(true));
			Assert.That(helper.IsRegulationActive("MR7-1"), Is.EqualTo(false));

			Assert.That(errorCollector.ToString(), Contains.Substring("Regulation XXX-X not found"));
		}
	}
}
