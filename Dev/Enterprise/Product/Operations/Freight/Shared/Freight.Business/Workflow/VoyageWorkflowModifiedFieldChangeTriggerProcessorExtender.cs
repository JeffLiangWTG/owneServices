using System.Collections.Generic;

namespace Enterprise.Freight.Business
{
	class VoyageWorkflowModifiedFieldChangeTriggerProcessorExtender : FreightWorkflowModifiedFieldChangeTriggerProcessorExtender
	{
		protected override List<string> GetSqls()
		{
			return new List<string>()
			{
				@"
SELECT JW_ParentGUID
FROM dbo.JobVoyage
JOIN dbo.JobVoyOrigin		ON JA_JV = JV_PK
JOIN dbo.JobSailing			ON JX_JA = JA_PK
JOIN dbo.JobConsolTransport	ON JW_JX = JX_PK
WHERE JV_PK = @ParentID",
@"SELECT JN_JS
FROM dbo.JobVoyage
JOIN dbo.JobVoyOrigin		ON JA_JV = JV_PK
JOIN dbo.JobSailing			ON JX_JA = JA_PK
JOIN dbo.JobConsolTransport	ON JW_JX = JX_PK
JOIN dbo.JobConShipLink		ON JW_ParentGUID = JN_JK
WHERE JV_PK = @ParentID
"
			};
		}
	}
}
