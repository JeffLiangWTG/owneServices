using System;
using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefCurrencyPopulateISOSubUnitRatio : DataTransformation, IDataTransformationTask
	{
		public RefCurrencyPopulateISOSubUnitRatio(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sqlText = FormattableString.Invariant($@"
IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[{nameof(RefCurrency)}]') 
		 AND name = '{nameof(RefCurrency.RX_ISOSubUnitRatio)}'
)
BEGIN
	ALTER TABLE {nameof(RefCurrency)} ADD {nameof(RefCurrency.RX_ISOSubUnitRatio)} INT NOT NULL CONSTRAINT DF_RX_ISOSubUnitRatio DEFAULT (0)
END");

			var sqlTextUpdate = FormattableString.Invariant($@"
UPDATE {nameof(RefCurrency)}
SET {nameof(RefCurrency.RX_ISOSubUnitRatio)} = 
	CASE 
		WHEN {nameof(RefCurrency.RX_Code)} = 'TWD' OR {nameof(RefCurrency.RX_Code)} = 'HUF' THEN 100
		ELSE {nameof(RefCurrency.RX_SubUnitRatio)}
	END");

			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Transaction = trans;
				cmd.CommandText = sqlText;
				cmd.ExecuteNonQuery();
			}

			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Transaction = trans;
				cmd.CommandText = sqlTextUpdate;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
