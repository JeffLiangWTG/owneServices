using System;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator.Test
{
	[TestFixture]
	internal class MeasureFilterFixture
	{
		[Test]
		public void IsValid()
		{
			var mea1 = new measure();
			var mea2 = new measure { nationalSpecified = true, national = 0L };
			var mea3 = new measure { nationalSpecified = true, national = 0L, goodsNomenclatureCode = "0101", regulationId = "R0715180" };
			var mea4 = new measure { dateEndSpecified = true, dateEnd = DateTime.UtcNow.AddDays(10), nationalSpecified = true, national = 0L, goodsNomenclatureCode = "0101", regulationId = "R0715180" };
			var mea5 = new measure { dateEndSpecified = true, dateEnd = DateTime.UtcNow.AddDays(-10), nationalSpecified = true, national = 0L, goodsNomenclatureCode = "0101", regulationId = "R0715180" };
			var mea6 = new measure { nationalSpecified = true, national = 0L, goodsNomenclatureCode = "0101", regulationId = "R2204750" };
			var filter = new MeasureFilter(emptyFilter, emptyFilter, emptyFilter, new [] { "R0715180" });
			Assert.IsFalse(filter.IsValid(mea1, DateTime.UtcNow));
			Assert.IsFalse(filter.IsValid(mea2, DateTime.UtcNow));
			Assert.IsTrue(filter.IsValid(mea3, DateTime.UtcNow));
			Assert.IsTrue(filter.IsValid(mea4, DateTime.UtcNow));
			Assert.IsFalse(filter.IsValid(mea5, DateTime.UtcNow));
			Assert.IsFalse(filter.IsValid(mea6, DateTime.UtcNow));
		}

		[Test]
		public void IsValid_Filter_MeastureTypes()
		{
			var mea1 = new measure { nationalSpecified = true, national = 0L, measureType = "142", goodsNomenclatureCode = "1010", regulationId = "R0715180" };
			var mea2 = new measure { nationalSpecified = true, national = 0L, measureType = "115", goodsNomenclatureCode = "1010", regulationId = "R0715180" };
			var filter_MeasureTypes = new[] { "115" };
			var filter = new MeasureFilter(emptyFilter, filter_MeasureTypes, emptyFilter, new[] { "R0715180" });
			Assert.IsTrue(filter.IsValid(mea2, DateTime.UtcNow));
			Assert.IsFalse(filter.IsValid(mea1, DateTime.UtcNow));
		}

		[Test]
		public void IsValid_Filter_GeographicalAreaIds()
		{
			var mea1 = new measure { nationalSpecified = true, national = 0L, geographicalAreaId = "SG", goodsNomenclatureCode = "1010", regulationId = "R0715180" };
			var mea2 = new measure { nationalSpecified = true, national = 0L, geographicalAreaId = "CN", goodsNomenclatureCode = "1010", regulationId = "R0715180" };
			var filter_GeographicalAreaIds = new[] { "CN" };
			var filter = new MeasureFilter(emptyFilter, emptyFilter, filter_GeographicalAreaIds, new[] { "R0715180" });
			Assert.IsTrue(filter.IsValid(mea2, DateTime.UtcNow));
			Assert.IsFalse(filter.IsValid(mea1, DateTime.UtcNow));
		}

		readonly string[] emptyFilter = new string[0];

		[Test]
		public void GetValidValue()
		{
			var mea = new measure
			{
				goodsNomenclatureCode = "0111000000",
				measureComponent = new [] {
					new measureComponent { nationalSpecified = true, national = 0L, dutyAmount = 0.1m },
					new measureComponent { nationalSpecified = false, national = 1L, dutyAmount = 0.2m }
				},
				measureCondition = new[] {
					new measureCondition {
						nationalSpecified = true, national = 0L, dutyAmount = 0.1m,
						measureConditionComponent = new [] {
							new measureConditionComponent { nationalSpecified = true, national = 0L, dutyAmount = 0.2m },
							new measureConditionComponent { nationalSpecified = true, national = 1L, dutyAmount = 0.3m },
						}
					},
					new measureCondition {
						nationalSpecified = true, national = 1L, dutyAmount = 0.2m
					}
				},
				measureExcludedGeographicalArea = new [] {
					new measureExcludedGeographicalArea { nationalSpecified = true, national = 0L, geographicalAreaId = "SG" },
					new measureExcludedGeographicalArea { nationalSpecified = true, national = 1L, geographicalAreaId = "CN" },
				}
			};
			var filter = new MeasureFilter(emptyFilter, emptyFilter, emptyFilter, emptyFilter);
			var result = filter.GetValidValue(mea);
			Assert.AreEqual("0111", result.goodsNomenclatureCode);
			Assert.AreEqual(1, result.measureComponent.Length);
			Assert.AreEqual(0.1m, result.measureComponent[0].dutyAmount);
			Assert.AreEqual(1, result.measureCondition.Length);
			Assert.AreEqual(0.1m, result.measureCondition[0].dutyAmount);
			Assert.AreEqual(1, result.measureCondition[0].measureConditionComponent.Length);
			Assert.AreEqual(0.2m, result.measureCondition[0].measureConditionComponent[0].dutyAmount);
			Assert.AreEqual(1, result.measureExcludedGeographicalArea.Length);
			Assert.AreEqual("SG", result.measureExcludedGeographicalArea[0].geographicalAreaId);
		}
	}
}
