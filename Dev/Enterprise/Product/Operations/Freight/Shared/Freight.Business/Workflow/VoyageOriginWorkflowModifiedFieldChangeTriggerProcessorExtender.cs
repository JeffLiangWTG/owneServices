using System.Collections.Generic;

namespace Enterprise.Freight.Business
{
	class VoyageOriginWorkflowModifiedFieldChangeTriggerProcessorExtender : FreightWorkflowModifiedFieldChangeTriggerProcessorExtender
	{
		protected override List<string> GetSqls()
		{
			return new List<string>()
			{
				@"
SELECT JW_ParentGUID
FROM dbo.JobConsolTransport
JOIN dbo.JobSailing ON JX_PK = JW_JX
WHERE JX_JA = @ParentID",
@"SELECT JN_JS
FROM dbo.JobSailing
JOIN dbo.JobConsolTransport ON JX_PK = JW_JX
JOIN dbo.JobConShipLink		ON JW_ParentGUID = JN_JK
WHERE JX_JA = @ParentID",
@"SELECT JA_JV
FROM dbo.JobVoyOrigin
WHERE JA_PK = @ParentID
"
			};
		}
	}
}
