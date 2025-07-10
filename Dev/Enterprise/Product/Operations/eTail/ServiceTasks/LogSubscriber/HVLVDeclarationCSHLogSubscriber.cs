using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.eTail.Business;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.ServiceTasks
{
	[Serializable]
	public class HVLVDeclarationCSHLogSubscriber : LogSubscriber
	{
		public override string Name => nameof(HVLVDeclarationCSHLogSubscriber);

		public override string[] EventTypes => new[] { AutoEvents.ClearanceStatusChangedCode };

		public override string[] TableNames => new[] { JobDeclarationSchema.Constants.TableName };

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			foreach (var log in queuedLogs)
			{
				var parameters = StmALog.GetParametersFromReference(log.SJ_Reference);
				if (parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.SourceModuleId, out var sourceModuleId)
					&& sourceModuleId == Core.Constants.GlobalModuleNamesConstants.HVLV)
				{
					var declaration = log.Factory.Load<BaseJobDeclaration>(log.SJ_ParentID);
					var consignments = log.Factory.Load<HVLVConsignment>(new ZQuery(HVLVConsignmentSchema.HVC_JE_ImportDeclaration, declaration.PK));

					foreach (var consignment in consignments)
					{
						if (declaration.IsExport)
						{
							consignment.HVC_ExportCustomsClearanceStatus = declaration.CustomsClearanceStatus;
						}
						else
						{
							consignment.HVC_ImportCustomsClearanceStatus = declaration.CustomsClearanceStatus;
						}
					}
				}
			}
		}
	}
}
