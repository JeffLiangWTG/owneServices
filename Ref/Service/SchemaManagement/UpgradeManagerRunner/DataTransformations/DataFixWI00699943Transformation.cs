using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataTransformations
{
	public class DataFixWI00699943Transformation : DataTransformation, IDataTransformationTask
	{
		public DataFixWI00699943Transformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var tryCount = 0;

			while (IsDuplicateDataExists(trans))
			{
				if (tryCount >= 10)
				{
					break;
				}

				RunCore(trans);
				tryCount++;
			}
		}

		protected virtual void RunCore(IDbTransaction trans)
		{
			const string sql = @"DECLARE @MaxColumnLength INT = ( SELECT [max_length]
									FROM sys.columns
									WHERE [name] = 'ZXE_ColumnCaption'
										AND OBJECT_NAME(object_id) = 'RefCusCodeListAttributeName' );

								;
								WITH
									RawData
									AS
									(
										SELECT [ZXE_PK],
											[ZXE_ZZK_NKCodeType],
											[ZXE_ZZZ_NKDataGrouping],
											[ZXE_ColumnCaption],
											COUNT(ZXE_PK)
												   OVER (PARTITION BY [ZXE_ZZK_NKCodeType], [ZXE_ZZZ_NKDataGrouping], [ZXE_ColumnCaption] )                         ROW_CNT,
											ROW_NUMBER() OVER (PARTITION BY [ZXE_ZZK_NKCodeType], [ZXE_ZZZ_NKDataGrouping], [ZXE_ColumnCaption] ORDER BY ZXE_Name) ROW_NUM
										FROM RefCusCodeListAttributeName
										WHERE ZXE_ColumnCaption <> ''
									)
							UPDATE RCCLAN
							SET [ZXE_ColumnCaption] = 	CASE 
															WHEN LEN(CONCAT(RCCLAN.[ZXE_ColumnCaption], '(', RD.ROW_NUM, ')')) < @MaxColumnLength
																THEN CONCAT(RCCLAN.[ZXE_ColumnCaption], '(', RD.ROW_NUM, ')')
															ELSE
																CONCAT(LEFT(RCCLAN.[ZXE_ColumnCaption], @MaxColumnLength - LEN(CONCAT('(', RD.ROW_NUM, ')'))) , '(', RD.ROW_NUM, ')')
														END
							FROM RefCusCodeListAttributeName RCCLAN
									INNER JOIN RawData RD ON RD.ZXE_PK = RCCLAN.ZXE_PK
										AND RD.ROW_CNT > 1;";

			DbHelper.ExecuteNonQuery(trans, sql);
		}

		protected static bool IsDuplicateDataExists(IDbTransaction trans)
		{
			const string sql = @"SELECT COUNT(1) CONFLICT_CNT
							    FROM [dbo].[RefCusCodeListAttributeName]
							    WHERE [ZXE_ColumnCaption] <> ''
							    GROUP BY
							        [ZXE_ZZK_NKCodeType],
							        [ZXE_ZZZ_NKDataGrouping],
							        [ZXE_ColumnCaption]
							    HAVING COUNT([ZXE_PK]) > 1";

			using (var cmd = DbHelper.CreateCommand(trans, sql))
			{
				return cmd.ExecuteScalar() != null;
			}
		}
	}
}
