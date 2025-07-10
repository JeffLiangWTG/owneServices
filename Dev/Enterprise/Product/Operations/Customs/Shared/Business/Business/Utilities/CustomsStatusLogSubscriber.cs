using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[Serializable]
	public class CustomsStatusLogSubscriber : LogSubscriber
	{
		public override string Name => nameof(CustomsStatusLogSubscriber);

		public override string FriendlyName => (NoResString)"Customs Status Log Subscriber";

		public const string PublishCustomsStatusChangedEventService = "PCS";

		public override string[] TableNames => new[] { CusHAWBSchema.Constants.TableName, CusOutturnSchema.Constants.TableName, CusSCAHouseSchema.Constants.TableName, AsycudaBillSchema.Constants.TableName, CusUSLVClearanceSchema.Constants.TableName };

		public override string[] EventTypes => eventTypes ??= new[] { AutoEvents.CustomsEntryStatusCode, AutoEvents.MessageStatusChangeCode };
		string[] eventTypes;

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			var filteredLogs = FilterQueuedLogs(queuedLogs);

			var factory = filteredLogs.FirstOrDefault()?.Factory;

			if (factory != null)
			{
				var parents = LoadLogParents(factory, filteredLogs).ToArray();

				if (parents.Any())
				{
					var tempLogFactory = new BusinessObjectFactory() { RefreshEnabled = false };
					tempLogFactory.ServiceContainer.AddService<IBOIsSavedByFactoryService>(new NeverSaveBusinessObjectsService());

					var currentBranchCode = GlbBranch.CurrentBranch.GB_Code;

					foreach (var group in filteredLogs.GroupBy(c => c.SJ_GB_NKBranch.IsEmpty ? currentBranchCode : c.SJ_GB_NKBranch))
					{
						using (DisposableEnvironment.ForBranch(group.Key))
						{
							foreach (var log in group.OrderBy(c => c.SJ_PostedTimeUtc))
							{
								var contextType = GetDataContextType(log.SJ_ParentTableCode);

								if (contextType != null)
								{
									var parent = parents.FirstOrDefault(c => c.PK == log.SJ_ParentID);

									if (parent != null)
									{
										var tempLog = BuildTempLog(tempLogFactory, log);
										PublishUniversalEvent(factory, contextType.Value, parent, tempLog);
									}
								}
							}
						}
					}
				}
			}
		}

		void PublishUniversalEvent(BusinessObjectFactory factory, DataContextType contextType, BusinessObject parent, StmALog log)
		{
			var result = UniversalXmlWorkflowProcessor.PublishUniversalEvent(factory, contextType, ZString.Empty, parent, log);

			KeyValuePair<string, string>[] BuildParameters(PublishToUniversalResult publishToUniversalResult)
			{
				var parameters = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, AutoEvents.CustomsEntryStatusCode),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, publishToUniversalResult.ErrorMessage)
				};

				if (log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, out var status))
				{
					parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Status, status));
				}

				return parameters.ToArray();
			}

			result.AddDataExportFailureLogIfNeeded(parent as IStmALogParent, contextType, BuildParameters);
		}

		StmALog BuildTempLog(BusinessObjectFactory factory, IQueuedLog queuedLog)
		{
			var log = factory.New<StmALog>();

			using (((IUpdateFieldsLockReachAround)log).LockForUpdatingKeyFields(false))
			{
				log.SL_EventTimeOffset = queuedLog.EventTimeOffset;
				log.SL_GS_NKUser = queuedLog.SJ_GS_NKUser;
				log.SL_Reference = queuedLog.SJ_Reference;
				log.SL_SE_NKEvent = queuedLog.SJ_SE_NKEvent;

				switch (queuedLog.SJ_ParentTableCode)
				{
					case CusHAWBSchema.Constants.Prefix:
						{
							log.SL_Table = CusHAWBSchema.Constants.TableName;
							break;
						}

					case CusSCAHouseSchema.Constants.Prefix:
						{
							log.SL_Table = CusSCAHouseSchema.Constants.TableName;
							break;
						}

					case CusOutturnSchema.Constants.Prefix:
						{
							log.SL_Table = CusOutturnSchema.Constants.TableName;
							break;
						}

					case AsycudaBillSchema.Constants.Prefix:
						{
							log.SL_Table = AsycudaBillSchema.Constants.TableName;
							break;
						}

					case CusUSLVClearanceSchema.Constants.Prefix:
						{
							log.SL_Table = CusUSLVClearanceSchema.Constants.TableName;
							break;
						}
				}

				log.SL_Parent = queuedLog.SJ_ParentID;
				log.SL_IsEstimate = queuedLog.SJ_IsEstimate;
				log.SL_GB_NKBranch = queuedLog.SJ_GB_NKBranch;
				log.SL_GE_NKDepartment = queuedLog.SJ_GE_NKDepartment;
			}

			return log;
		}

		DataContextType? GetDataContextType(ZString tableCode)
		{
			switch (tableCode)
			{
				case CusHAWBSchema.Constants.Prefix:
				case CusSCAHouseSchema.Constants.Prefix:
				case AsycudaBillSchema.Constants.Prefix:
				case CusUSLVClearanceSchema.Constants.Prefix:
					return DataContextType.HVLVConsignment;

				case CusOutturnSchema.Constants.Prefix:
					return DataContextType.TransitReceive;

				default:
					return null;
			}
		}

		IQueuedLog[] FilterQueuedLogs(IQueuedLog[] queuedLogs)
		{
			var value = string.Concat(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Service, "=", PublishCustomsStatusChangedEventService);
			return queuedLogs?.Where(c => c.SJ_Reference.Contains(value, StringComparison.OrdinalIgnoreCase)).ToArray() ?? Array.Empty<IQueuedLog>();
		}

		IEnumerable<BusinessObject> LoadLogParents(BusinessObjectFactory factory, IQueuedLog[] logs)
		{
			IEnumerable<BusinessObject> LoadLogParentsCore<T>(string tableCode, SchemaPKColumn pkSchema) where T : class
			{
				var filteredLogs = logs.Where(c => c.SJ_ParentTableCode == tableCode);

				return filteredLogs.Any()
					? factory.Load<T>(new ZQuery(pkSchema, filteredLogs.Select(c => c.SJ_ParentID).Distinct())) as BusinessObject[]
					: Array.Empty<BusinessObject>();
			}

			foreach (var parent in LoadLogParentsCore<Integration.Customs.Shared.ICusHAWB>(CusHAWBSchema.Constants.Prefix, CusHAWBSchema.PK))
			{
				yield return parent;
			}

			foreach (var parent in LoadLogParentsCore<Integration.Customs.ICusOutturn>(CusOutturnSchema.Constants.Prefix, CusOutturnSchema.PK))
			{
				yield return parent;
			}

			foreach (var parent in LoadLogParentsCore<Integration.Customs.Shared.IBaseCusSCAHouse>(CusSCAHouseSchema.Constants.Prefix, CusSCAHouseSchema.PK))
			{
				yield return parent;
			}

			foreach (var parent in LoadLogParentsCore<Integration.Customs.ManifestBase.IAsycudaBill>(AsycudaBillSchema.Constants.Prefix, AsycudaBillSchema.PK))
			{
				yield return parent;
			}

			foreach (var parent in LoadLogParentsCore<Integration.Customs.US.LVS.ICusUSLVClearance>(CusUSLVClearanceSchema.Constants.Prefix, CusUSLVClearanceSchema.PK))
			{
				yield return parent;
			}
		}
	}

	sealed class NeverSaveBusinessObjectsService : IBOIsSavedByFactoryService
	{
		public bool IsBOSavedByFactory(BusinessObject businessObjects)
		{
			return false;
		}
	}
}
