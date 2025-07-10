using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class FixDuplicateRatesForZA2P3 : DataTransformation, IDataTransformationTask
	{
		public FixDuplicateRatesForZA2P3(int version)
			: base(version)
		{ }

		public void Run(IDbTransaction trans)
		{
			var sql = @"
IF EXISTS (SELECT TOP 1 * FROM RefCusApplicability
JOIN RefCusRate ON ZZ2_PK = ZZT_ZZ2_Rate
JOIN RefCusTariff ON ZZ2_ZZ1_Tariff = ZZ1_PK
WHERE ZZ1_ZZZ_NKDataGrouping = 'ZA' AND ZZT_ZZA_TradeGroup IS NULL AND ZZ1_TariffCode in ('260030106', '260030104'))
BEGIN
	DECLARE @tradeGroupPK uniqueidentifier;
	SELECT @tradeGroupPK = ZZA_PK FROM RefCusTradeGroup
	WHERE ZZA_ZZZ_NKDataGrouping = 'ZA'  and ZZA_TradeGroup = 'STANDARD'

	SELECT DISTINCT ZZ2_PK
	INTO #DeleteRates
	FROM RefCusApplicability
	JOIN RefCusRate r ON ZZ2_PK = ZZT_ZZ2_Rate
	JOIN RefCusTariff ON ZZ2_ZZ1_Tariff = ZZ1_PK
	WHERE ZZ1_ZZZ_NKDataGrouping = 'ZA' AND ZZT_ZZA_TradeGroup = @tradeGroupPK AND ZZ1_TariffCode in ('260030106', '260030104')

	DELETE RefCusApplicability
	WHERE ZZT_ZZ2_Rate IN (SELECT ZZ2_PK FROM #DeleteRates)

	DELETE RefCusRate WHERE ZZ2_PK IN (SELECT ZZ2_PK FROM #DeleteRates)

	UPDATE app SET ZZT_ZZA_TradeGroup = @tradeGroupPK
	FROM RefCusApplicability app
	JOIN RefCusRate ON ZZ2_PK = ZZT_ZZ2_Rate
	JOIN RefCusTariff ON ZZ2_ZZ1_Tariff = ZZ1_PK
	WHERE ZZ1_ZZZ_NKDataGrouping = 'ZA' AND ZZT_ZZA_TradeGroup IS NULL
END
";
			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}
