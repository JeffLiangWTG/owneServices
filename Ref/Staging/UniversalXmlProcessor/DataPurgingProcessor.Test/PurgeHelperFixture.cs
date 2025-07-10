using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Staging.Schema_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DataPurgingProcessor.Test
{
	[TestFixture]
	class PurgeHelperFixture
	{
		[Test]
		public void CreatePurgingSqlWithEntity()
		{
			var expectedSql = $@"DROP TABLE IF EXISTS #RefCusTariffTemp; CREATE TABLE #RefCusTariffTemp (PK uniqueidentifier PRIMARY KEY)
DROP TABLE IF EXISTS #RefCusConditionTemp; CREATE TABLE #RefCusConditionTemp (PK uniqueidentifier PRIMARY KEY)
DROP TABLE IF EXISTS #RefCusApplicabilityTemp; CREATE TABLE #RefCusApplicabilityTemp (PK uniqueidentifier PRIMARY KEY)
DROP TABLE IF EXISTS #RefCusTariffAdditionalCodeTemp; CREATE TABLE #RefCusTariffAdditionalCodeTemp (PK uniqueidentifier PRIMARY KEY)
INSERT #RefCusTariffTemp (PK)
SELECT TOP 100 ZZ1_PK
FROM  RefCusTariff
LEFT JOIN DataProcessingInformation ON DPI_ParentPK = ZZ1_PK
WHERE DPI_ParentPk IS NULL
INSERT INTO #RefCusConditionTemp (PK)
SELECT ZX1_PK
FROM RefCusCondition
JOIN #RefCusTariffTemp ON ZX1_ZZ1_Tariff = PK
WHERE ZX1_PK NOT IN (SELECT PK FROM #RefCusConditionTemp)
INSERT INTO #RefCusApplicabilityTemp (PK)
SELECT ZZT_PK
FROM RefCusApplicability
JOIN #RefCusConditionTemp ON ZZT_ZX1_Conditions = PK
WHERE ZZT_PK NOT IN (SELECT PK FROM #RefCusApplicabilityTemp)
INSERT INTO #RefCusTariffAdditionalCodeTemp (PK)
SELECT ZY2_PK
FROM RefCusTariffAdditionalCode
JOIN #RefCusTariffTemp ON ZY2_ZZ1_Tariff = PK
WHERE ZY2_PK NOT IN (SELECT PK FROM #RefCusTariffAdditionalCodeTemp)
INSERT INTO #RefCusApplicabilityTemp (PK)
SELECT ZZT_PK
FROM RefCusApplicability
JOIN #RefCusTariffAdditionalCodeTemp ON ZZT_ZY2_AdditionalCode = PK
WHERE ZZT_PK NOT IN (SELECT PK FROM #RefCusApplicabilityTemp)

DELETE entity
FROM RefCusApplicability entity
JOIN #RefCusApplicabilityTemp ON  PK = entity.ZZT_PK


DELETE entity
FROM RefCusCondition entity
JOIN #RefCusConditionTemp ON  PK = entity.ZX1_PK


DELETE entity
FROM RefCusApplicability entity
JOIN #RefCusApplicabilityTemp ON  PK = entity.ZZT_PK


DELETE entity
FROM RefCusTariffAdditionalCode entity
JOIN #RefCusTariffAdditionalCodeTemp ON  PK = entity.ZY2_PK


DELETE entity
FROM RefCusTariff entity
JOIN #RefCusTariffTemp ON  PK = entity.ZZ1_PK

";
			var entityList = new List<Tuple<Type, Type>>
			{
				Tuple.Create((Type)null, typeof(RefCusTariff)),
				Tuple.Create(typeof(RefCusTariff), typeof(RefCusCondition)),
				Tuple.Create(typeof(RefCusCondition), typeof(RefCusApplicability)),
				Tuple.Create(typeof(RefCusTariff), typeof(RefCusTariffAdditionalCode)),
				Tuple.Create(typeof(RefCusTariffAdditionalCode), typeof(RefCusApplicability)),
			};
			string insertSql = @"INSERT #RefCusTariffTemp (PK)
SELECT TOP 100 ZZ1_PK
FROM  RefCusTariff
LEFT JOIN DataProcessingInformation ON DPI_ParentPK = ZZ1_PK
WHERE DPI_ParentPk IS NULL";
			var sql1 = PurgeHelper.CreatePurgingSqlWithEntity(entityList, insertSql);
			Assert.AreEqual(expectedSql, sql1);
		}

		[Test]
		public void TestGetEntityTypeElementFromNonTransformedXml()
		{
			var nonTransformedXml = File.ReadAllText(new string("TestFiles/NonTransformed.xml"));
			var entityTypeElements = PurgeHelper.GetEntityTypeElements(nonTransformedXml);
			Assert.That(entityTypeElements, Has.Length.EqualTo(4));
			Assert.That(entityTypeElements.Select(element => element.Attribute("Name")?.Value).ToArray(), Is.EquivalentTo(new[] { "RefCusTariff", "RefCusRate", "RefCusApplicability", "RefCusRateUOM" }));
		}

		[Test]
		public void TestGetEntityTypeElementsFromTransformedXml()
		{
			var transformedXml = File.ReadAllText(new string("TestFiles/Transformed.xml"));
			var entityTypeElements = PurgeHelper.GetEntityTypeElements(transformedXml);
			Assert.That(entityTypeElements, Has.Length.EqualTo(4));
			Assert.That(entityTypeElements.Select(element => element.Attribute("Name")?.Value).ToArray(), Is.EquivalentTo(new[] { "RefCusTariff", "RefCusRate", "RefCusApplicability", "RefCusRateUOM" }));
		}
	}
}
