using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[Serializable]
	public class WorkflowShipmentCargoReportAcceptedSubscriber : LogSubscriber
	{
		public override string Name
		{
			get { return "ShipmentCargoReporterWorkflow"; }
		}

		public override string FriendlyName
		{
			get { return (NoResString)"Workflow Shipment Cargo Reporter Events"; }
		}

		public override string[] TableNames
		{
			get { return new string[] { JobShipmentSchema.Constants.TableName }; }
		}

		public override string[] EventTypes
		{
			get { return new string[] { Events.AddedARecordToTheSystem.Code, Events.Attached.Code }; }
		}

		protected override ILogBatcher GetLogBatcher() => new LogBatcher();

		class LogBatcher : LogBatcher<ZGuid>
		{
			protected override void AddGroupingFetchHints(IEnumerable<IQueuedLog> enumberable) { }
			protected override ZGuid GetGroupLogKey(IQueuedLog log) => log.SJ_ParentID;

			protected override LogsGroupContext SetContextForLogsGroup(ZGuid groupKey, IEnumerable<IQueuedLog> queuedLogs)
			{
				var factory = queuedLogs.FirstOrDefault(IsMatchTYPParameter)?.Factory;
				if (factory != null)
				{
					var shipment = factory.Load<ForwardingShipment>(groupKey);
					if (shipment != null && shipment.JS_IsForwardRegistered)
					{
						string destinationCountryCode = shipment.JS_RL_NKDestination.Left(2);
						if (IsInterestedDestinationCountry(destinationCountryCode))
						{
							var environment = DisposableEnvironment.ForCompany(GetDestinationCountryCompanyCode(destinationCountryCode, factory), false);
							if (environment != null)
							{
								return new LogsGroupContext(false, environment, ProcessTask.Loader.SuppressTemplateApplication());
							}
						}
					}
				}

				return new LogsGroupContext(true);
			}
		}

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			var queuedLog = queuedLogs.First();
			var shipment = queuedLog.Factory.Load<ForwardingShipment>(queuedLog.SJ_ParentID);
			if (shipment != null)
			{
				DefaultLogger.Log(LogType.Debug, $"Processing cargo report for shipment {shipment.PK} ({shipment.JobNumber}).");
				DefaultLogger.Log(LogType.Debug, $"Shipment isSea: {shipment.IsSea}, Registry Value for Sea: {CustomsDataRegistry.Instance.GroupToSendLateSeaCargoReportAndWarningsTo.Value}");
				DefaultLogger.Log(LogType.Debug, $"Shipment IsAir: {shipment.IsAir}, Registry Value for Air: {CustomsDataRegistry.Instance.GroupToSendLateAirCargoReportAndWarningsTo.Value}");

				if ((shipment.IsSea && CustomsDataRegistry.Instance.GroupToSendLateSeaCargoReportAndWarningsTo.Value != ZGuid.Empty) ||
					(shipment.IsAir && CustomsDataRegistry.Instance.GroupToSendLateAirCargoReportAndWarningsTo.Value != ZGuid.Empty))
				{
					DefaultLogger.Log(LogType.Debug, $"Shipment {shipment.PK} ({shipment.JobNumber}) is going to cargo report processing.");
					try
					{
						GetCargoReportWorkflow(queuedLog.Factory, shipment).ProcessOneImportShipment(shipment);
					}
					catch (Exception ex)
					{
						DefaultLogger.Log(LogType.Debug, $"Error processing cargo report for shipment {shipment.PK} ({shipment.JobNumber}). Exception: {ex.Message}", ex);
					}
				}
			}
			else
			{
				DefaultLogger.Log(LogType.Debug, $"Shipment load by Log:{queuedLog.SJ_ParentID} is null. Unable to process cargo report.");
			}
		}

		static bool IsMatchTYPParameter(IQueuedLog queuedLog)
		{
			var logParameters = StmALog.GetParametersFromReference(queuedLog.SJ_Reference);

			return !logParameters.ContainsKey(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type)
				|| logParameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type] == Core.Constants.EventReferenceParameterTypes.Consol;
		}

		#region Implementation

		static string GetDestinationCountryCompanyCode(string destinationCountryCode, BusinessObjectFactory factory)
		{
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode != destinationCountryCode || GlbCompany.CurrentCompany.IsDemoCompany)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(GlbCompany));
				query.AddToFilter(GlbCompanySchema.GC_Code, SQLComparisonOperator.NotEqual, "DEM");
				ZDBOnlySubQuery branchFilter = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.GB_GC);
				branchFilter.AddToFilter(GlbBranchSchema.GB_IsActive, true);
				branchFilter.AddToFilter(GlbBranchSchema.GB_RL_NKHomePort, SQLComparisonOperator.StartsWith, destinationCountryCode);
				query.AddSubQuery(branchFilter, JoinCondition.And);
				GlbCompany selectedCompany = factory.LoadTop1<GlbCompany>(query);
				if (selectedCompany != null)
				{
					return selectedCompany.GC_Code;
				}
			}

			return GlbCompany.CurrentCompany.GC_Code;
		}

		CargoReportWorkflow GetCargoReportWorkflow(BusinessObjectFactory factory, ForwardingShipment shipment)
		{
			if (shipment.JS_RL_NKDestination.StartsWith(Core.Constants.CountryCodes.Australia))
			{
				DefaultLogger.Log(LogType.Debug, $"Creating CargoReportWorkflow for shipment {shipment.PK}({shipment.JobNumber})");
				return (CargoReportWorkflow)Activator.CreateInstance(ObjectFactory.GetType<Integration.Customs.AU.ICargoReportWorkflow>(), new object[] { factory });
			}
			else
			{
				DefaultLogger.Log(LogType.Debug, $"Creating CargoReportWorkflow for Not AU shipment {shipment.PK}({shipment.JobNumber}) with destination {shipment.JS_RL_NKDestination}");
				return new CargoReportWorkflow(factory);
			}
		}

		static bool IsInterestedDestinationCountry(string code)
		{
			return code.Equals(Core.Constants.CountryCodes.Australia);
		}

		#endregion
	}
}
