using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class TariffTypeAddWI00222952Transformation : DataTransformation, IDataTransformationTask
	{
		public TariffTypeAddWI00222952Transformation(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = @"
if (Select Count(1) FROM [RefCusTariffType] where ZZI_ZZZ_NKDataGrouping = 'za' and ZZI_TariffType = '6P5') = 0
BEGIN
	insert into [RefCusTariffType] ([ZZI_PK],[ZZI_TariffType],[ZZI_Description],[ZZI_ZZ9_NKNomenclatureGroupType],[ZZI_ZZZ_NKDataGrouping],[ZZI_ZZR_RateType],[ZZI_HasFormulaSpecificQuestions])
	SELECT newid()
	,'6P5'
		  ,'Schedule 6 Part 5'
		  ,[ZZI_ZZ9_NKNomenclatureGroupType]
		  ,[ZZI_ZZZ_NKDataGrouping]
		  ,[ZZI_ZZR_RateType]
		  ,[ZZI_HasFormulaSpecificQuestions]
	  FROM [RefCusTariffType]
	  where ZZI_ZZZ_NKDataGrouping = 'za'
	  and ZZI_TariffType = '6P4'
END";

			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Connection = trans.Connection;
				cmd.Transaction = trans;
				cmd.CommandText = sql;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
