using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.LVS.ServiceTasks
{
	[Serializable]
	public class USLVSToDeclarationLogSubscriber : LogSubscriber
	{
		public override string Name => nameof(USLVSToDeclarationLogSubscriber);

		public override string[] EventTypes => new[] { AutoEvents.TransferToCustomsImportsDecCode };

		public override string[] TableNames => new[] { CusUSLVClearanceSchema.Constants.TableName };

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			foreach (var log in queuedLogs)
			{
				var stmALogQuery = new ZQuery(StmALogSchema.SL_Parent, log.SJ_ParentID);
				stmALogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, log.SJ_SE_NKEvent);
				stmALogQuery.AddToFilter(StmALogSchema.PK, log.SJ_ALogReference);
				var stmALog = log.Factory.LoadTop1<StmALog>(stmALogQuery);
				if (stmALog != null && !stmALog.SL_IsCancelled)
				{
					if (log.SJ_ParentTableCode == CusUSLVClearanceSchema.Constants.Prefix)
					{
						var clearance = log.Factory.Load<CusUSLVClearance>(log.SJ_ParentID);
						if (clearance != null)
						{
							StmALog.GetParametersFromReference(log.SJ_Reference).TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, out string reason);
							if (reason == LVSConstants.ConvertToDeclarationReason.CombineConsignments)
							{
								ProcessConsignmentCombined(clearance);
							}
							else
							{
								StmALog.GetParametersFromReference(log.SJ_Reference).TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, out string consignmentPK);
								var consignment = clearance.Factory.Load<CusUSLVConsignment>(new ZGuid(consignmentPK));
								if (consignment != null && !consignment.HasBeenConvertedToStandaloneDeclaration)
								{
									ProcessConsignment(consignment);
								}
							}
						}
					}
				}
			}
		}

		void ProcessConsignment(CusUSLVConsignment consignment)
		{
			StandAloneDeclarationConverter.PublishDeclarationUniversalXMLIndividual(consignment, false);
			var eventProcessedLog = new ZStringBuilder(string.Format(CultureInfo.InvariantCulture,
				"{0} {1} Event processed for consignment. ",
				consignment.Shipment.HumanReadableName, AutoEvents.TransferToCustomsImportsDecCode));

			if (!consignment.CE_EntryLineReference.IsEmpty)
			{
				eventProcessedLog.Append(string.Format("Stand Alone Declaration {0} has been created.", consignment.CE_EntryLineReference));
			}

			DefaultLogger.Log(LogType.Information, eventProcessedLog.ToString());
		}

		void ProcessConsignmentCombined(CusUSLVClearance clearance)
		{
			var combineConsignments = clearance.CombinedConsignmentsToConvert;
			if (combineConsignments != null)
			{
				var consignment = combineConsignments.FirstOrDefault();
				StandAloneDeclarationConverter.PublishDeclarationUniversalXMLCombined(consignment, false);
				var eventProcessedLog = new ZStringBuilder(string.Format(CultureInfo.InvariantCulture,
					"{0} {1} Event processed for combined consignments. ",
					consignment.Shipment.HumanReadableName, AutoEvents.TransferToCustomsImportsDecCode));

				if (!consignment.CE_EntryLineReference.IsEmpty)
				{
					eventProcessedLog.Append(string.Format("Stand Alone Declaration {0} has been created.", consignment.CE_EntryLineReference));
				}

				DefaultLogger.Log(LogType.Information, eventProcessedLog.ToString());
			}
		}
	}
}

