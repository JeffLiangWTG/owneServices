using System.Collections.Generic;

namespace Enterprise.Freight.Business
{
	class TransportWorkflowModifiedFieldChangeTriggerProcessorExtender : FreightWorkflowModifiedFieldChangeTriggerProcessorExtender
	{
		protected override List<string> GetSqls()
		{
			return new List<string>()
			{
				@"
SELECT JW_ParentGUID
FROM dbo.JobConsolTransport
WHERE JW_PK = @ParentID
",
@"SELECT JN_JS
FROM dbo.JobConsolTransport
JOIN dbo.JobConShipLink  ON JW_ParentGUID = JN_JK
WHERE JW_PK = @ParentID
"
			};
		}
	}
}
