using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator.Test
{
	[TestFixture]
	public class ScriptGeneratorFixture
	{
		[Test]
		public void GetPrerequisites()
		{
			var info1 = new Mock<IDataSetUpdaterInfo>();
			info1.Setup(x => x.GetStorageType()).Returns(typeof(IDummy1));
			info1.Setup(x => x.GetTypeByInsertOrder()).Returns(new[] { Tuple.Create(typeof(IDummy1), typeof(IDummy1)) });
			var info2 = new Mock<IDataSetUpdaterInfo>();
			info2.Setup(x => x.GetStorageType()).Returns(typeof(IDummy2));
			info2.Setup(x => x.GetTypeByInsertOrder()).Returns(new[] { Tuple.Create(typeof(IDummy2), typeof(IDummy2)) });
			var sql = new Mock<ISQLBuilder>();
			var schemaInfo = new Mock<ISchemaInfo>();
			var gen = new ScriptGenerator(sql.Object, schemaInfo.Object, new[] { info1.Object, info2.Object });
			var fks = new ForeignKeyRelationship
			{
				Table = typeof(IDummy1),
				Column = nameof(IDummy1.D1_D2),
				ReferencedTable = typeof(IDummy2),
				ReferencedColumn = nameof(IDummy2.D2_PK)
			};
			schemaInfo.Setup(x => x.GetReferencedForeignKeysFromDb(null)).Returns(new[] { fks });
			CollectionAssert.AreEqual(new[] { nameof(IDummy2) }, gen.GetPrerequisites(info1.Object));
		}

		[Test]
		public void GetPrerequisitesWithNoDependency()
		{
			var info1 = new Mock<IDataSetUpdaterInfo>();
			info1.Setup(x => x.GetStorageType()).Returns(typeof(IDummy1));
			info1.Setup(x => x.GetTypeByInsertOrder()).Returns(new[] { Tuple.Create(typeof(IDummy1), typeof(IDummy1)) });
			var info2 = new Mock<IDataSetUpdaterInfo>();
			info2.Setup(x => x.GetStorageType()).Returns(typeof(IDummy2));
			info2.Setup(x => x.GetTypeByInsertOrder()).Returns(new[] { Tuple.Create(typeof(IDummy2), typeof(IDummy2)) });
			var sql = new Mock<ISQLBuilder>();
			var schemaInfo = new Mock<ISchemaInfo>();
			var gen = new ScriptGenerator(sql.Object, schemaInfo.Object, new[] { info1.Object, info2.Object });
			var fks = new ForeignKeyRelationship
			{
				Table = typeof(IDummy1),
				Column = nameof(IDummy1.D1_D2),
			};
			schemaInfo.Setup(x => x.GetReferencedForeignKeysFromDb(null)).Returns(new[] { fks });
			CollectionAssert.AreEqual(new List<string>(), gen.GetPrerequisites(info1.Object));
		}

		[Test]
		public void GetPrerequisitesWithCyclicDependency()
		{
			var info1 = new Mock<IDataSetUpdaterInfo>();
			info1.Setup(x => x.GetStorageType()).Returns(typeof(IDummy1));
			info1.Setup(x => x.GetTypeByInsertOrder()).Returns(new[] { Tuple.Create(typeof(IDummy1), typeof(IDummy1)) });
			var info2 = new Mock<IDataSetUpdaterInfo>();
			info2.Setup(x => x.GetStorageType()).Returns(typeof(IDummy2));
			info2.Setup(x => x.GetTypeByInsertOrder()).Returns(new[] { Tuple.Create(typeof(IDummy2), typeof(IDummy2)) });
			var info3 = new Mock<IDataSetUpdaterInfo>();
			info3.Setup(x => x.GetStorageType()).Returns(typeof(IDummy3));
			info3.Setup(x => x.GetTypeByInsertOrder()).Returns(new[] { Tuple.Create(typeof(IDummy3), typeof(IDummy3)) });
			var sql = new Mock<ISQLBuilder>();
			var schemaInfo = new Mock<ISchemaInfo>();
			var gen = new ScriptGenerator(sql.Object, schemaInfo.Object, new[] { info1.Object, info2.Object, info3.Object });
			var fks = new ForeignKeyRelationship
			{
				Table = typeof(IDummy1),
				Column = nameof(IDummy1.D1_D2),
				ReferencedTable = typeof(IDummy2),
				ReferencedColumn = nameof(IDummy2.D2_PK)
			};
			var fks1 = new ForeignKeyRelationship
			{
				Table = typeof(IDummy2),
				Column = nameof(IDummy2.D2_PK),
				ReferencedTable = typeof(IDummy3),
				ReferencedColumn = nameof(IDummy3.D3_PK)
			};
			var fks2 = new ForeignKeyRelationship
			{
				Table = typeof(IDummy3),
				Column = nameof(IDummy3.D3_PK),
				ReferencedTable = typeof(IDummy1),
				ReferencedColumn = nameof(IDummy1.D1_D2)
			};

			schemaInfo.Setup(x => x.GetReferencedForeignKeysFromDb(null)).Returns(new[] { fks, fks1, fks2 });
			CollectionAssert.AreEqual(new[] { nameof(IDummy2), nameof(IDummy3), nameof(IDummy1) }, gen.GetPrerequisites(info1.Object));
			CollectionAssert.AreEqual(new[] { nameof(IDummy3), nameof(IDummy1), nameof(IDummy2) }, gen.GetPrerequisites(info2.Object));
			CollectionAssert.AreEqual(new[] { nameof(IDummy1), nameof(IDummy2), nameof(IDummy3) }, gen.GetPrerequisites(info3.Object));
		}

		[Test]
		public void GetExtraPrerequisitesForCertainDatasets()
		{
			var info1 = new Mock<IDataSetUpdaterInfo>();
			info1.Setup(x => x.GetStorageType()).Returns(typeof(IRefCusTariff));
			info1.Setup(x => x.GetTypeByInsertOrder()).Returns(new[] { Tuple.Create(typeof(IRefCusTariff), typeof(IRefCusTariff)), Tuple.Create(typeof(IRefCusCondition), typeof(IRefCusCondition)) });
			var info2 = new Mock<IDataSetUpdaterInfo>();
			info2.Setup(x => x.GetStorageType()).Returns(typeof(IRefCusNomenclatureGroup));
			info2.Setup(x => x.GetTypeByInsertOrder()).Returns(new[] { Tuple.Create(typeof(IRefCusNomenclatureGroup), typeof(IRefCusNomenclatureGroup)), Tuple.Create(typeof(IRefCusCondition), typeof(IRefCusCondition)) });
			var info3 = new Mock<IDataSetUpdaterInfo>();
			info3.Setup(x => x.GetStorageType()).Returns(typeof(IRefCusPreference));
			info3.Setup(x => x.GetTypeByInsertOrder()).Returns(new[] { Tuple.Create(typeof(IRefCusPreference), typeof(IRefCusPreference)), Tuple.Create(typeof(IRefCusCondition), typeof(IRefCusCondition)) });
			var info4 = new Mock<IDataSetUpdaterInfo>();
			info4.Setup(x => x.GetStorageType()).Returns(typeof(IRefCusCodeList));
			info4.Setup(x => x.GetTypeByInsertOrder()).Returns(new[] { Tuple.Create(typeof(IRefCusCodeList), typeof(IRefCusCodeList)), Tuple.Create(typeof(IRefCusCodeListAttribute), typeof(IRefCusCodeListAttribute)) });
			var info5 = new Mock<IDataSetUpdaterInfo>();
			info5.Setup(x => x.GetStorageType()).Returns(typeof(IRefCusCodeListAttributeName));
			info5.Setup(x => x.GetTypeByInsertOrder()).Returns(new[] { Tuple.Create(typeof(IRefCusCodeListAttributeName), typeof(IRefCusCodeListAttributeName)) });
			var info6 = new Mock<IDataSetUpdaterInfo>();
			info6.Setup(x => x.GetStorageType()).Returns(typeof(IRefCusProfile));
			info6.Setup(x => x.GetTypeByInsertOrder()).Returns(new[] { Tuple.Create(typeof(IRefCusTariffType), typeof(IRefCusTariffType)), Tuple.Create(typeof(IRefCusProfileType), typeof(IRefCusProfileType)) });


			var sql = new Mock<ISQLBuilder>();
			var schemaInfo = new Mock<ISchemaInfo>();
			var gen = new ScriptGenerator(sql.Object, schemaInfo.Object, new[] { info1.Object, info2.Object, info3.Object, info4.Object, info5.Object, info6.Object });
			Assert.Contains(nameof(IRefCusPreference), gen.GetPrerequisites(info1.Object).ToArray());
			Assert.Contains(nameof(IRefCusPreference), gen.GetPrerequisites(info2.Object).ToArray());
			Assert.Contains(nameof(IRefCusCodeListAttributeName), gen.GetPrerequisites(info4.Object).ToArray());
		}

		[Test]
		public void GeneratePrepareTemporaryTablesScripts()
		{
			var schemaInfo = new Mock<ISchemaInfo>();
			schemaInfo.Setup(x => x.GetAllUniqueIndexes<IRefAccTaxRate>(null)).Returns(new[] { new[] { new IndexColumn { Column = nameof(IRefAccTaxRate.ZAT_ReferenceRateType) } } });
			var generator = new ScriptGenerator(new Mock<ISQLBuilder>().Object, schemaInfo.Object, new IDataSetUpdaterInfo[0]);
			var info = new Mock<IDataSetUpdaterInfo>();
			info.Setup(x => x.GetTypeByInsertOrder()).Returns(new[] { Tuple.Create(typeof(IRefAccTaxRate), typeof(IRefAccTaxRate)) });
			info.Setup(x => x.GetReferenceTypeByInsertOrder()).Returns(new[] { Tuple.Create(typeof(IRefAccTaxRate), typeof(IRefAccTaxRate)) });
			var result = generator.GeneratePrepareTemporaryTablesScripts(info.Object).ToArray();
			Assert.AreEqual("#TempRefAccTaxRate", result[0].Item1);
			StringAssert.Contains(@"IF OBJECT_ID('tempdb..#TempRefAccTaxRate') IS NOT NULL
BEGIN
	DROP TABLE #TempRefAccTaxRate
END
	SELECT ZAT_PK AS ZAT_PK,ZAT_RN_NKCountry AS ZAT_RN_NKCountry,ZAT_ReferenceRateType AS ZAT_ReferenceRateType,ZAT_StartDate AS ZAT_StartDate,ZAT_EndDate AS ZAT_EndDate,ZAT_RateNumerator AS ZAT_RateNumerator,ZAT_RateDenominator AS ZAT_RateDenominator,CAST (0 AS BIT) AS Deleted INTO #TempRefAccTaxRate FROM RefAccTaxRate WHERE 1 =0
	CREATE CLUSTERED INDEX PK_#TempRefAccTaxRate ON #TempRefAccTaxRate (ZAT_PK)", result[0].Item2);
			Assert.AreEqual("#TempRefAccTaxRate", result[1].Item1);
			StringAssert.Contains(@"IF OBJECT_ID('tempdb..#TempRefAccTaxRate') IS NOT NULL
BEGIN
	DROP TABLE #TempRefAccTaxRate
END
	SELECT ZAT_ReferenceRateType AS ZAT_ReferenceRateType,ZAT_PK AS ZAT_PK INTO #TempRefAccTaxRate FROM RefAccTaxRate WHERE 1 =0
	CREATE CLUSTERED INDEX PK_#TempRefAccTaxRate ON #TempRefAccTaxRate (ZAT_PK)", result[1].Item2);
		}

		[Test]
		public void GenerateOneTableDatasetMergeScript()
		{
			var builder = new Mock<ISQLBuilder>();
			var schemaInfo = new Mock<ISchemaInfo>();
			schemaInfo.Setup(x => x.GetReferencedForeignKeysFromDb(null)).Returns(new ForeignKeyRelationship[0]);
			var generator = new ScriptGenerator(builder.Object, schemaInfo.Object, new IDataSetUpdaterInfo[0]);
			builder.Setup(x => x.CreateDeclarationSqlBeforeMerge()).Returns("Declaration A");
			builder.Setup(x => x.DeleteTemporaryTablesData(string.Empty, It.IsAny<IEnumerable<Type>>())).Returns("Delete A");
			var info = new Mock<IDataSetUpdaterInfo>();
			info.Setup(x => x.GetMergeSqlText(It.IsAny<ISQLBuilder>(), It.IsAny<Dictionary<Type, ForeignKeyRelationship[]>>(), It.IsAny<ISchemaInfo>())).Returns("Merge A");
			info.Setup(x => x.GetStorageType()).Returns(typeof(IRefAccTaxRate));
			var result = generator.GenerateMergeScript(info.Object);
			StringAssert.Contains(@"Declaration A


Merge A
Delete A", result);
		}

		[Test]
		public void GenerateTwoTableDatasetMergeScript()
		{
			var builder = new Mock<ISQLBuilder>();
			var generator = new ScriptGenerator(builder.Object, new Mock<ISchemaInfo>().Object, new IDataSetUpdaterInfo[0]);
			builder.Setup(x => x.CreateDeclarationSqlBeforeMerge()).Returns("Declaration A");
			builder.Setup(x => x.DeleteTemporaryTablesData(string.Empty, It.IsAny<IEnumerable<Type>>())).Returns("Delete A");
			var info = new Mock<IDataSetUpdaterInfo>();
			info.Setup(x => x.GetMergeSqlText(It.IsAny<ISQLBuilder>(), It.IsAny<Dictionary<Type, ForeignKeyRelationship[]>>(), It.IsAny<ISchemaInfo>())).Returns("Merge A");
			info.Setup(x => x.GetStorageType()).Returns(typeof(IRefAccTaxRate));
			var result = generator.GenerateMergeScript(info.Object);
			StringAssert.Contains(@"Declaration A


Merge A
Delete A", result);
		}
	}
}
