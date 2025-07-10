using CargoWise.RefDbRepo.Client.SqlServer;
using CargoWise.RefDbRepo.Common.Utils;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator;

class RefCusQuotaCheckRoundingScript : ICheckRoundingScript
{
	public string GetScript()
	{
		var tempTableName = SQLBuilder.GetTemporaryTableName<IRefCusQuota>(string.Empty);
		var (groupColumn, valueColumn) = (nameof(IRefCusQuota.ZXQ_ZZZ_NKDataGrouping), nameof(IRefCusQuota.ZXQ_Balance));

		return $"""
				IF EXISTS ( SELECT 1 FROM {tempTableName} WHERE {groupColumn} = '{RoundingIssueConstants.Flag}' AND {valueColumn} <> {RoundingIssueConstants.Value})
				BEGIN
					RAISERROR('CW requires upgrade to the latest version.', 16, 1)
					RETURN
				END

				DELETE FROM {tempTableName} WHERE {groupColumn} = '{RoundingIssueConstants.Flag}'

				""";
	}
}
