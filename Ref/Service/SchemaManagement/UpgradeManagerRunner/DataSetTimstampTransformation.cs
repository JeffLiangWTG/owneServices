using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataSetTimstampTransformation : IDataTransformationTask
	{
		public int Version => 9;

		[SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sqlText = new StringBuilder();
			sqlText.AppendLine(UpdateLastUpdatedUTC(nameof(RefCarrierCode), nameof(RefCarrierCodeAttribute),
				nameof(RefCarrierCodeAttribute.ZZG_PK), nameof(RefCarrierCodeAttribute.ZZG_ZZ4_CarrierCode), "ZZG"));
			sqlText.AppendLine(UpdateLastUpdatedUTC(nameof(RefCarrierCode), nameof(RefCarrierVesselPivot),
				nameof(RefCarrierVesselPivot.ZZQ_PK), nameof(RefCarrierVesselPivot.ZZQ_ZZ4), "ZZQ"));
			sqlText.AppendLine(UpdateLastUpdatedUTC(nameof(RefCusCodeList), nameof(RefCusCodeListAttribute),
				nameof(RefCusCodeListAttribute.ZZE_PK), nameof(RefCusCodeListAttribute.ZZE_ZZD_CodeList), "ZZE"));
			sqlText.AppendLine(UpdateLastUpdatedUTC(nameof(RefCusNomenclatureGroup), nameof(RefCusNomenclatureGroupNote),
				nameof(RefCusNomenclatureGroupNote.ZZL_PK), nameof(RefCusNomenclatureGroupNote.ZZL_ZZ5_NomenclatureGroup), "ZZL"));
			sqlText.AppendLine(UpdateLastUpdatedUTC(nameof(RefCusTradeGroup), nameof(RefCusTradeGroupCountry),
				nameof(RefCusTradeGroupCountry.ZZB_PK), nameof(RefCusTradeGroupCountry.ZZB_ZZA_TradeGroup), "ZZB"));
			sqlText.AppendLine(UpdateLastUpdatedUTC(nameof(RefCusTariff), nameof(RefCusTariffAttribute),
				nameof(RefCusTariffAttribute.ZZ3_PK), nameof(RefCusTariffAttribute.ZZ3_ZZ1_Tariff), "ZZ3"));
			sqlText.AppendLine(UpdateLastUpdatedUTC(nameof(RefCusTariff), nameof(RefCusTariffUOM),
				nameof(RefCusTariffUOM.ZZ8_PK), nameof(RefCusTariffUOM.ZZ8_ZZ1_Tariff), "ZZ8"));
			sqlText.AppendLine(UpdateLastUpdatedUTC(nameof(RefCusTariff), nameof(RefCusTariffRelationship),
				nameof(RefCusTariffRelationship.ZZH_PK), nameof(RefCusTariffRelationship.ZZH_ZZ1_Tariff), "ZZH"));
			sqlText.AppendLine(UpdateLastUpdatedUTC(nameof(RefCusTariff), nameof(RefCusRate),
				nameof(RefCusRate.ZZ2_PK), nameof(RefCusRate.ZZ2_ZZ1_Tariff), "ZZ2"));

			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Transaction = trans;
				cmd.CommandText = sqlText.ToString();
				cmd.ExecuteNonQuery();
			}
		}

		public static string UpdateLastUpdatedUTC(string mainTable, string depTable, string depTblPK, string depTablFK, string depTablePrefix)
		{
			return $@"
DISABLE TRIGGER ALL ON dbo.{depTable};

UPDATE version SET LastUpdatedUTC = CASE WHEN version.LastUpdatedUTC >= depVersion.LastUpdatedUTC OR depVersion.LastUpdatedUTC IS NULL THEN version.LastUpdatedUTC ELSE depVersion.LastUpdatedUTC END
FROM RefDbVersionControl version
LEFT JOIN {depTable} ON {depTablFK} = version.ParentPK
LEFT JOIN RefDbVersionControl depVersion ON {depTblPK} = depVersion.ParentPK

DELETE t FROM
{depTable} t
JOIN RefDbVersionControl ON {depTblPK} = ParentPK and Deleted = 1;

DELETE RefDbVersionControl WHERE ParentCode = '{depTablePrefix}';

ENABLE TRIGGER ALL ON dbo.{depTable};
";
		}
	}
}
