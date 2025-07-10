using System.Collections.Generic;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.ServiceTasks
{
	public class ImportDocsReceivedBeforeOrderLineRequiredDateExceptionGenerator : IProcessor
	{
		const string sql = @"
SELECT
	JO_LineDropDate									RequiredBy,
	Milestone1.P9_EstimatedDefaultTimeDelta			EstimateDefaultTimeDelta,
	JO_PK											OrderLinePK
FROM 				dbo.JobOrderHeader
INNER JOIN			dbo.JobOrderLine				ON		JO_JD = JD_PK
LEFT OUTER JOIN 	dbo.JobShipmentPreplanning		ON		JD_EF_ShipmentPrePlanning = EF_PK
LEFT OUTER JOIN 	dbo.JobShipment					ON		JD_JS = JS_PK
LEFT OUTER JOIN 	dbo.JobDeclaration				ON		JD_JE = JE_PK
INNER JOIN 			dbo.ProcessTasks Milestone1		ON		Milestone1.P9_ParentID = JD_PK AND Milestone1.P9_Type = 'MIL' AND Milestone1.P9_SE_NKMilestoneEvent = 'AID' AND Milestone1.P9_EstimatedDefaultedFrom = 'LNR' AND Milestone1.P9_EstimatedDefaultTimeDelta IS NOT NULL
LEFT OUTER JOIN 	dbo.ProcessTasks Milestone2		ON		Milestone2.P9_ParentID = JS_PK AND Milestone2.P9_Type = 'MIL' AND Milestone2.P9_SE_NKMilestoneEvent = 'AID' AND Milestone2.P9_EstimatedDefaultedFrom = 'LNR' AND Milestone2.P9_EstimatedDefaultTimeDelta IS NOT NULL
LEFT OUTER JOIN 	dbo.ProcessTasks Milestone3		ON		Milestone3.P9_ParentID = JE_PK AND Milestone3.P9_Type = 'MIL' AND Milestone3.P9_SE_NKMilestoneEvent = 'AID' AND Milestone3.P9_EstimatedDefaultedFrom = 'LNR' AND Milestone3.P9_EstimatedDefaultTimeDelta IS NOT NULL
LEFT OUTER JOIN 	dbo.ProcessTasks Milestone4		ON		Milestone4.P9_ParentID = EF_PK AND Milestone4.P9_Type = 'MIL' AND Milestone4.P9_SE_NKMilestoneEvent = 'AID' AND Milestone4.P9_EstimatedDefaultedFrom = 'LNR' AND Milestone4.P9_EstimatedDefaultTimeDelta IS NOT NULL
WHERE (Milestone1.P9_PK IS NOT NULL OR Milestone2.P9_PK IS NOT NULL OR Milestone3.P9_PK IS NOT NULL OR Milestone4.P9_PK IS NOT NULL)
  AND NOT EXISTS (SELECT 1
                  FROM dbo.ProcessTasks
                  WHERE P9_ParentID = JD_PK
                    AND P9_Type = 'EXC'
                    AND P9_SE_NKMilestoneEvent = 'AID'
                    AND P9_Description LIKE '%Line #' + convert(varchar, JO_LineNo))
  AND Milestone1.P9_ActualDate IS NULL 
  AND Milestone2.P9_ActualDate IS NULL 
  AND Milestone3.P9_ActualDate IS NULL
  AND Milestone4.P9_ActualDate IS NULL
  AND JO_LineDropDate IS NOT NULL
  AND JD_SystemCreateTimeUtc > dateadd(day, -32, getdate())
"; // Hard-coded sql statement

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			DbCommand command = Db.Connection.Command(sql); // I need to use sql what of it
			List<ZGuid> overdueOrderLinePKs = new List<ZGuid>();
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					token.ThrowIfCancellationRequested();
					ZDateTime orderLineRequiredBy = new ZDateTime(reader["RequiredBy"]);
					ZDateTime documentsReceivedByDelta = new ZDateTime(reader["EstimateDefaultTimeDelta"]);
					ZDateTime documentsReceivedBy = orderLineRequiredBy + documentsReceivedByDelta.TimeSpan6MonthsFromStartOfYear;
					if (documentsReceivedBy < ZDateTime.Today)
					{
						overdueOrderLinePKs.Add(new ZGuid(reader["OrderLinePK"]));
					}
				}
			}
			CreateExceptionsForOverdueOrderLines(overdueOrderLinePKs.ToArray(), token);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Description for the exception")]
		void CreateExceptionsForOverdueOrderLines(ZGuid[] overdueOrderLinePKs, CancellationToken token)
		{
			int i = 0;
			BusinessObjectFactoryProvider factoryProvider = new BusinessObjectFactoryProvider();
			foreach (ZGuid orderLinePK in overdueOrderLinePKs)
			{
				token.ThrowIfCancellationRequested();
				OrderLine orderLine = factoryProvider.Current.Load<OrderLine>(orderLinePK);
				IWorkflowProvider workflowProvider = orderLine.Order;
				ProcessTask exception = workflowProvider.WorkflowItems.Exceptions.AddNew();
				exception.TriggerConditions.TriggerEventCode = Events.AllImportDocumentsReceived.Code;
				exception.P9_Description = Events.AllImportDocumentsReceived.Description + " for Order Line #" + orderLine.JO_LineNo;
				if (i > 100)
				{
					factoryProvider.SaveCurrentAndCreateNew();
					i = 0;
				}
				i++;
			}
			factoryProvider.SaveCurrentAndCreateNew();
		}
	}
}
