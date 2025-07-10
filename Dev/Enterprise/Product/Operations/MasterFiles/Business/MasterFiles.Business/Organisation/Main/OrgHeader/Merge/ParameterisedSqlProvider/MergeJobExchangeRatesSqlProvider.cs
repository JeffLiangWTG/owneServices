using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class MergeJobExchangeRatesSqlProvider : OrgMergeParameterisedSqlProvider
	{
		protected override string Description => (NoResString)"Merge Job Exchange Rates";

		protected override string SqlText =>
$@"MERGE 
	dbo.JobExRate as target
USING 
	(SELECT 
		JF_OH_Org, 
		JF_JH, 
		JF_OrgType, 
		JF_RX_NKRateCurrency 
	FROM 
		dbo.JobExRate
	WHERE
		JF_OH_Org = {NewPkParameter}) as source
ON
	(target.JF_JH = source.JF_JH AND
	target.JF_OrgType = source.JF_OrgType AND
	target.JF_RX_NKRateCurrency = source.JF_RX_NKRateCurrency AND
	target.JF_OH_Org = {OldPkParameter})
WHEN MATCHED
	THEN DELETE;";
	}
}
