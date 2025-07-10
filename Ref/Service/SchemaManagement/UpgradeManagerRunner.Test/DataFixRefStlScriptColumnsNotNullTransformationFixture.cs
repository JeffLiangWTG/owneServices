using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DataFixRefStlScriptColumnsNotNullTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"
select count(1) from RefStlScript
where STL_CompanyCode='' and STL_BranchCode='' and STL_CreatingUserCode='' and STL_BillingReference1='' and STL_BillingReference2='' and STL_BillingReference3='' and STL_BillingReference4=''
	and STL_AdditionalRefs='' and STL_PreparationScript='' and STL_WhereClause='' and STL_MinCW1Version='' and STL_MaxCW1Version='';
";
				Assert.That((int)cmd.ExecuteScalar(), Is.EqualTo(1));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixRefStlScriptColumnsNotNullTransformation(118);
		}

		protected override void PrepareTestData()
		{
			var sql = @"
DROP VIEW [dbo].[RefStlScriptUserView];

ALTER TABLE [dbo].[RefStlScript] DROP CONSTRAINT [DF_RefStlScript_STL_CompanyCode]
ALTER TABLE RefStlScript ALTER COLUMN STL_CompanyCode NVARCHAR(1000) NULL;
ALTER TABLE [dbo].[RefStlScript] DROP CONSTRAINT [DF_RefStlScript_STL_BranchCode]
ALTER TABLE RefStlScript ALTER COLUMN STL_BranchCode NVARCHAR(1000) NULL;
ALTER TABLE [dbo].[RefStlScript] DROP CONSTRAINT [DF_RefStlScript_STL_CreatingUserCode]
ALTER TABLE RefStlScript ALTER COLUMN STL_CreatingUserCode NVARCHAR(1000) NULL;
ALTER TABLE [dbo].[RefStlScript] DROP CONSTRAINT [DF_RefStlScript_STL_BillingReference1]
ALTER TABLE RefStlScript ALTER COLUMN STL_BillingReference1 NVARCHAR(1000) NULL;
ALTER TABLE [dbo].[RefStlScript] DROP CONSTRAINT [DF_RefStlScript_STL_BillingReference2]
ALTER TABLE RefStlScript ALTER COLUMN STL_BillingReference2 NVARCHAR(1000) NULL;
ALTER TABLE [dbo].[RefStlScript] DROP CONSTRAINT [DF_RefStlScript_STL_BillingReference3]
ALTER TABLE RefStlScript ALTER COLUMN STL_BillingReference3 NVARCHAR(1000) NULL;
ALTER TABLE [dbo].[RefStlScript] DROP CONSTRAINT [DF_RefStlScript_STL_BillingReference4]
ALTER TABLE RefStlScript ALTER COLUMN STL_BillingReference4 NVARCHAR(1000) NULL;
ALTER TABLE [dbo].[RefStlScript] DROP CONSTRAINT [DF_RefStlScript_STL_AdditionalRefs]
ALTER TABLE RefStlScript ALTER COLUMN STL_AdditionalRefs NVARCHAR(1000) NULL;
ALTER TABLE [dbo].[RefStlScript] DROP CONSTRAINT [DF_RefStlScript_STL_PreparationScript]
ALTER TABLE RefStlScript ALTER COLUMN STL_PreparationScript NVARCHAR(1000) NULL;
ALTER TABLE [dbo].[RefStlScript] DROP CONSTRAINT [DF_RefStlScript_STL_WhereClause]
ALTER TABLE RefStlScript ALTER COLUMN STL_WhereClause NVARCHAR(1000) NULL;
ALTER TABLE [dbo].[RefStlScript] DROP CONSTRAINT [DF_RefStlScript_STL_MinCW1Version]
ALTER TABLE RefStlScript ALTER COLUMN STL_MinCW1Version NVARCHAR(1000) NULL;
ALTER TABLE [dbo].[RefStlScript] DROP CONSTRAINT [DF_RefStlScript_STL_MaxCW1Version]
ALTER TABLE RefStlScript ALTER COLUMN STL_MaxCW1Version NVARCHAR(1000) NULL;

INSERT INTO [dbo].[RefStlScript]([STL_PK],[STL_FeatureCode],[STL_RoleName],[STL_ModuleName],[STL_FunctionName],[STL_FeatureName],
	[STL_DataGranularity],[STL_CompanyCode],[STL_BranchCode],[STL_TransactionDateUtc],[STL_CreatingUserCode],[STL_GuidReference],
	[STL_BillingReference1],[STL_BillingReference2],[STL_BillingReference3],[STL_BillingReference4],[STL_AdditionalRefs],
	[STL_TransactionCount],[STL_PreparationScript],[STL_FromClause],[STL_WhereClause],[STL_WithOptionRecompile],[STL_UsedInBilling],
	[STL_ActiveOn],[STL_MinCW1Version],[STL_MaxCW1Version],[STL_DateType],[STL_CollectionStartDateUtc])
VALUES (NEWID(),'WIN','LS','WareHouse','Funcs','Contact','TRN',NULL,NULL,'wd.WD_SystemCreateTimeUtc',NULL,'wd.WD_PK',
	NULL,NULL,NULL,NULL,NULL,1,NULL,'WhsDocket wd',NULL,0,1,'ALL',NULL,NULL,'DTE',NULL);
";
			DbHelper.ExecuteNonQuery(Transaction, sql);
		}
	}
}
