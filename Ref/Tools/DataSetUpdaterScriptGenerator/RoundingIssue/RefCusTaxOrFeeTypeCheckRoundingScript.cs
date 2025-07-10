using CargoWise.RefDbRepo.Client.SqlServer;
using CargoWise.RefDbRepo.Common.Utils;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator;

class RefCusTaxOrFeeTypeCheckRoundingScript : ICheckRoundingScript
{
	public string GetScript()
	{
		var headerTempTableName = SQLBuilder.GetTemporaryTableName<IRefCusTaxOrFeeType>(string.Empty);
		var childTempTableName = SQLBuilder.GetTemporaryTableName<IRefCusTaxOrFee>(string.Empty);
		var (childGroupColumn, childValueColumn) = (nameof(IRefCusTaxOrFee.ZZF_ZZZ_NKDataGrouping), nameof(IRefCusTaxOrFee.ZZF_Value));
		const string parentColumnName = nameof(IRefCusTaxOrFeeType.ZX0_Description);

		return $"""
				IF EXISTS ( SELECT 1 FROM {childTempTableName} WHERE {childGroupColumn} = '{RoundingIssueConstants.Flag}' AND {childValueColumn} <> {RoundingIssueConstants.Value})
				BEGIN
					RAISERROR('CW requires upgrade to the latest version.', 16, 1)
					RETURN
				END

				DELETE FROM {childTempTableName} WHERE {childGroupColumn} = '{RoundingIssueConstants.Flag}'
				DELETE FROM {headerTempTableName} WHERE {parentColumnName} = '{RoundingIssueConstants.DummyDescription}'
				
				""";
	}
}
