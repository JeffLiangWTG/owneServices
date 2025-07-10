using System.Collections.Generic;

namespace Enterprise.Freight.Business
{
	class VoyageDestinationWorkflowModifiedFieldChangeTriggerProcessorExtender : FreightWorkflowModifiedFieldChangeTriggerProcessorExtender
	{
		protected override List<string> GetSqls()
		{
			return new List<string>()
			{
				@"
SELECT JW_ParentGUID
FROM dbo.JobConsolTransport
JOIN dbo.JobSailing ON JX_PK = JW_JX
WHERE JX_JB = @ParentID",
@"SELECT JN_JS
FROM dbo.JobSailing
JOIN dbo.JobConsolTransport ON JX_PK = JW_JX
JOIN dbo.JobConShipLink		ON JW_ParentGUID = JN_JK
WHERE JX_JB = @ParentID",
@"SELECT JB_JV
FROM dbo.JobVoyDestination
WHERE JB_PK = @ParentID
"
			};
		}
	}
}
