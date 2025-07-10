using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	MovementExporterServiceTask.Code,
	MovementExporterServiceTask.Description,
	ServiceTaskConstants.Category,
	typeof(MovementExporterServiceTask),
	MinimumPeriod = "1day",
	DefaultScheduleRunEvery = "1day"
	)]

// Can't apply HostedServiceBusinessObjectBinding
// Business requires to send a batch of universal events once a day.
namespace Enterprise.Freight.Agency.ServiceTasks
{
	[CargoWise.Common.Testing.SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public sealed class MovementExporterServiceTask : ServiceProviderImpl
	{
		public const string Code = "CMX";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "used in assembly attribute")]
		public const string Description = "Periodic Container Movement Exporter";
		public const int MovementBatchSize = 1000;

		public override void RunTask(CancellationToken token)
		{
			var highWaterMark = AgencyRegistry.Instance.CMMeHubHighWaterMark.Value == DateTime.MinValue ? ZDateTime.MinSmallDateTimeValue : AgencyRegistry.Instance.CMMeHubHighWaterMark.Value;
			var newHighWaterMark = ZDateTime.UtcNow.ToDateTime();

			var factoryProvider = new BusinessObjectFactoryProvider();
			var movementPKs = GetMovementPKs(highWaterMark, factoryProvider.Current);
			var notifications = ServiceLogger.GetTaskNotificationSubscriber();

			new MovementExporterBatchProcessor(movementPKs).Process(notifications, token);
			AgencyRegistry.Instance.CMMeHubHighWaterMark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newHighWaterMark);
		}

		static IList<ZGuid> GetMovementPKs(ZDateTime highWaterMark, BusinessObjectFactory factory)
		{
			string sql = @"SELECT E9_PK FROM dbo.JobContainerMove WHERE E9_SystemCreateTimeUtc >= @PostedTime 
ORDER BY E9_SystemCreateTimeUtc";
			ZDateTime postedTime;
			var cutoff = AgencyRegistry.Instance.CMMeHubCutOff.Value;
			if (cutoff == DateTime.MinValue && highWaterMark == DateTime.MinValue)
			{
				postedTime = ZDateTime.MinSmallDateTimeValue;
			}
			else if (cutoff > highWaterMark)
			{
				postedTime = cutoff;
			}
			else
			{
				postedTime = highWaterMark;
			}

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add(ZSqlParameter.New("@PostedTime", postedTime, JobContainerMoveSchema.E9_SystemCreateTimeUtc));

			var queryResult = new DynamicBusinessObjectCollection(factory);
			queryResult.Load(sql, sqlParams);

			return queryResult.Select(obj => (ZGuid)obj[JobContainerMoveSchema.PK]).ToList();
		}

		class MovementExporterBatchProcessor : ManagedBatchProcessor<ContainerMovement>
		{
			public MovementExporterBatchProcessor(IList<ZGuid> movementPks)
			{
				this.movementPks = movementPks;
			}

			int watermark;
			readonly IList<ZGuid> movementPks;

			protected override ZQuery GetQuery()
			{
				if (watermark < movementPks.Count)
				{
					var batch = new List<ZGuid>(BatchSize);
					for (var limit = Math.Min(watermark + BatchSize, movementPks.Count); watermark < limit; watermark++)
					{
						batch.Add(movementPks[watermark]);
					}
					return new ZQuery(JobContainerMoveSchema.PK, batch);
				}
				else
				{
					return ZQuery.NoResultQuery;
				}
			}

			protected override ZQuery GetSingularQuery(ContainerMovement row) => new ZQuery(JobContainerMoveSchema.PK, row.PK);
			protected override void MarkRowAsBadCore(INotifications notifications, ContainerMovement row, BusinessObjectFactory factory)
			{
				// Do nothing; Watermark means this will be ignored.
			}

			protected override void ProcessRowCore(INotifications notifications, CancellationToken token, ContainerMovement movement, BusinessObjectFactory factory, (int, int) position)
			{
				using (var exporter = new ManualDataExport(movement.Factory, movement, UniversalDataType.UniversalEvent))
				{
					if (TryMapExportEvent(movement.E9_MovementType, out var eventCode))
					{
						exporter.EventCode = eventCode;
						exporter.EventReference = ContainerMovementHelper.GetEventReferenceForMovement(movement);
						exporter.RecipientType = MessageRecipientPartyTypeList.Codes.OrgProxy;
						exporter.SendData(notifications);
					}
				}
			}

			[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
			static bool TryMapExportEvent(string movementType, out string eventCode)
			{
				eventCode = string.Empty;
				switch (movementType)
				{
					case ContainerMovementTypes.Codes.ReturnToWharf:
					case ContainerMovementTypes.Codes.YardGateIn:
					case ContainerMovementTypes.Codes.WharfGateIn:
					case ContainerMovementTypes.Codes.ReturnedUnshipped:
					case ContainerMovementTypes.Codes.RePositionIntoYard:
					case ContainerMovementTypes.Codes.OnHire:
					case ContainerMovementTypes.Codes.DepotGateIn:
						eventCode = AutoEvents.GateIn.Code;
						return true;

					case ContainerMovementTypes.Codes.YardGateOut:
					case ContainerMovementTypes.Codes.WharfGateOut:
					case ContainerMovementTypes.Codes.ReShipRequested:
					case ContainerMovementTypes.Codes.RePositionOutOfYard:
					case ContainerMovementTypes.Codes.OffHire:
					case ContainerMovementTypes.Codes.DepotGateOut:
						eventCode = AutoEvents.GateOut.Code;
						return true;

					case ContainerMovementTypes.Codes.Discharge:
						eventCode = AutoEvents.FreightUnloaded.Code;
						return true;

					case ContainerMovementTypes.Codes.Load:
						eventCode = AutoEvents.FreightLoaded.Code;
						return true;
				}

				return false;
			}

			protected override IList<ContainerMovement> LoadBatchCore(BusinessObjectFactory factory, ZQuery query)
			{
				return factory.Load<ContainerMovement>(query);
			}
		}
	}
}


