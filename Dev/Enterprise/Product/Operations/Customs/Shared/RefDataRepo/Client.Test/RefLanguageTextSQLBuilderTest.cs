using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;
using NUnit.Framework;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	class RefLanguageTextSQLBuilderTest : TestCase
	{
		public void TestCreateMergeSqlLanguageTextFilter()
		{
			var uniqueConstraintIndexColumnsArray = new[] { new[] {
				new IndexColumn { Column = nameof(IRefLanguageText.RLT_ColumnName), IsNullable = false },
				new IndexColumn { Column = nameof(IRefLanguageText.RLT_ParentTableCode), IsNullable = false },
				new IndexColumn { Column = nameof(IRefLanguageText.RLT_Language), IsNullable = false }
			} };

			var fks = new Dictionary<Type, ForeignKeyRelationship[]>();
			fks.Add(typeof(IUserDummyDependentStorage), new[] { new ForeignKeyRelationship {
				Column = nameof(IRefLanguageText.RLT_ParentId),
				Table = typeof(IRefLanguageText),
				ReferencedColumn = nameof(IDummyDependentDependentStorage.D3_PK),
				ReferencedTable = typeof(IDummyDependentDependentStorage)
			} });

			var mergeSql = RefLanguageTextSQLBuilder.GetMergeSql<IUserDummyDependentStorage, UserDummyDependentStorage>(
				new SqlServerSQLBuilder(), DataSetName, fks, uniqueConstraintIndexColumnsArray, "mergeSourceName", "pkChangesTable", "childPkChangesTable", "tempParentCTE"
			);

			var expectedSql = @"DECLARE childPkChangesTable TABLE
(
	TargetPK uniqueidentifier,
	SourcePK uniqueidentifier INDEX _childPkChangesTable_SourcePK CLUSTERED
)

;WITH tempParentCTE AS
(
	SELECT TargetPK as D3_PK
	FROM pkChangesTable
	WHERE SourcePK IS NOT NULL
),
mergeSourceName AS
(
	SELECT RLT_PK ,RLT_ParentTableCode ,RLT_Language ,RLT_ColumnName ,RLT_Text ,RLT_IsSystem, COALESCE(TargetPK, RLT_ParentId) RLT_ParentId
	FROM #TempDummyDataSetRefLanguageText
	LEFT JOIN pkChangesTable ON RLT_ParentId = SourcePK AND SourcePK IS NOT NULL
)

MERGE RefLanguageText AS t
USING mergeSourceName AS s
ON ((t.RLT_ColumnName = s.RLT_ColumnName) AND (t.RLT_ParentTableCode = s.RLT_ParentTableCode) AND (t.RLT_Language = s.RLT_Language))
WHEN MATCHED AND t.RLT_IsSystem = 1 THEN
	UPDATE SET RLT_ParentId = s.RLT_ParentId, RLT_ParentTableCode = s.RLT_ParentTableCode, RLT_Language = s.RLT_Language, RLT_ColumnName = s.RLT_ColumnName, RLT_Text = s.RLT_Text, RLT_IsSystem = s.RLT_IsSystem
WHEN NOT MATCHED AND RLT_PK IS NOT NULL THEN
	INSERT (RLT_PK, RLT_ParentId, RLT_ParentTableCode, RLT_Language, RLT_ColumnName, RLT_Text, RLT_IsSystem) VALUES (s.RLT_PK, s.RLT_ParentId, s.RLT_ParentTableCode, s.RLT_Language, s.RLT_ColumnName, s.RLT_Text, s.RLT_IsSystem)
WHEN NOT MATCHED BY SOURCE AND RLT_PK IN (SELECT RLT_PK FROM tempParentCTE
JOIN RefLanguageText ON RLT_ParentId = D3_PK) AND t.RLT_IsSystem = 1 THEN
	UPDATE SET @dummy = 1
OUTPUT inserted.RLT_PK, s.RLT_PK INTO childPkChangesTable;


INSERT INTO @RefLanguageText_DELETE
SELECT TargetPK
FROM childPkChangesTable
WHERE SourcePK IS NULL



DELETE FROM RefLanguageText
WHERE RLT_PK IN (SELECT RLT_PK FROM @RefLanguageText_DELETE);";

			AssertContains(expectedSql, mergeSql);
		}

		string DataSetName => "DummyDataSet";
	}
}
