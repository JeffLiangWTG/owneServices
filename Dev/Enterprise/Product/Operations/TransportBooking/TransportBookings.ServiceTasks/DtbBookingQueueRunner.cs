using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.Integration.Freight;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Freight.Integration.Agency;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.TransportBookings.ServiceTasks
{
	[CodeAlive("Configured in EnterpriseApplicationConfiguration.xml to instantiate for IDtbBookingQueueRunner and instantiated via ObjectFactory")]
	class DtbBookingQueueRunner : IDtbBookingQueueRunner
	{
		public DtbBookingQueueRunner()
		{
			Factory = new BusinessObjectFactory();
			deliveryManagerFactory = ObjectFactory.Get<IDtbDeliveryManagerFactory>();
			parentJobTypes = new BindToLists(Factory).ParentJobTypes;
		}

		WrappedNotifications NotifyLandTransportConsignment;
		WrappedNotifications NotifyPortTransport;
		BookingToTransportJobCommonCreator BookingToLandTransportConsignmentCreator;
		BookingToTransportJobCommonCreator BookingToPortTransportCreator;

		public void Run(CancellationToken token, ILogger logger)
		{
			var iteratedQueueRecordsQuery = new ZDBOnlyQuery(typeof(DtbBookingQueue));
			iteratedQueueRecordsQuery.AddToFilter(DtbBookingQueueSchema.KMQ_Iteration, SQLComparisonOperator.GreaterThan, 0);
			var iteratedQueueRecords = Factory.Load<DtbBookingQueue>(iteratedQueueRecordsQuery);

			NotifyLandTransportConsignment = new WrappedNotifications();
			BookingToLandTransportConsignmentCreator = new BookingToTransportJobCommonCreator(NotifyLandTransportConsignment, AutoCreatorTargetModules.Codes.LandTransportConsignment);
			NotifyPortTransport = new WrappedNotifications();
			BookingToPortTransportCreator = new BookingToTransportJobCommonCreator(NotifyPortTransport, AutoCreatorTargetModules.Codes.PortTransport);

			while (!token.IsCancellationRequested)
			{
#if DEBUG
				// for testing only
				if (BeginProcessRecordInMainLoop != null)
				{
					BeginProcessRecordInMainLoop(this, new EventArgs());
				}
#endif

				if (!ProcessNextQueueRecord(null, logger))
				{
					break;
				}

#if DEBUG
				// for testing only
				if (EndProcessRecordInMainLoop != null)
				{
					EndProcessRecordInMainLoop(this, new EventArgs());
				}
#endif
			}

			LogAndThrowIfCancelled(token, logger);

			foreach (var iteratedQueueRecord in iteratedQueueRecords)
			{
				ProcessNextQueueRecord(iteratedQueueRecord, logger);
				LogAndThrowIfCancelled(token, logger);
			}
		}

		void LogAndThrowIfCancelled(CancellationToken token, ILogger logger)
		{
			if (token.IsCancellationRequested)
			{
				logger.Error(CancellationNotificationMessage);
			}
			token.ThrowIfCancellationRequested();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1079:DoNotCompareOnExceptionMessage", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		bool ProcessNextQueueRecord(DtbBookingQueue queueItem, ILogger baseLogger)
		{
			QueueRecord recordToProcess = null;
			var processRecordFailed = false;
			var deleteQueueRecord = false;
			var iterateQueueRecord = false;
			var errorMessageToLog = string.Empty;
			var errorMessageToSendToErrorReporter = string.Empty;
			var identifyingDescription = string.Empty;
			var createdCount = 0;
			var createdType = string.Empty;
			var loggerWithPrefix = new LoggerWithPrefix(baseLogger);
			var errorManager = ObjectFactory.Get<IAutomatedDtbBookingCreationErrorManager>();
			var reasonForJobCreationFailure = string.Empty;

			try
			{
				recordToProcess = queueItem == null ? GetNextQueueRecordToProcess() : GetSpecificQueueRecord(queueItem.PK.ToGuid());

				if (recordToProcess != null)
				{
					using (SetProcessingContext(recordToProcess.BranchCode, loggerWithPrefix))
					{
						createdType = GetCreatedType(recordToProcess);
						identifyingDescription = GetIdentifyingDescription(recordToProcess);
						LogCreationRequest(loggerWithPrefix, "Started", createdType, identifyingDescription);
						if (recordToProcess.TargetModule == AutoCreatorTargetModules.Codes.LandTransportConsignment)
						{
							createdCount = BookingToLandTransportConsignmentCreator.TryCreateTransportJobsForTransportBookingQueueItem(recordToProcess.ParentID);
							processRecordFailed = createdCount == 0;
							if (processRecordFailed)
							{
								reasonForJobCreationFailure = string.Join(System.Environment.NewLine, NotifyLandTransportConsignment.ErrorList.Select(e => e.Message));
							}
						}
						else if (recordToProcess.TargetModule == AutoCreatorTargetModules.Codes.PortTransport)
						{
							createdCount = BookingToPortTransportCreator.TryCreateTransportJobsForTransportBookingQueueItem(recordToProcess.ParentID);
							processRecordFailed = createdCount == 0;
							if (processRecordFailed)
							{
								reasonForJobCreationFailure = string.Join(System.Environment.NewLine, NotifyPortTransport.ErrorList.Select(e => e.Message));
							}
						}
						else
						{
							try
							{
								createdCount = ProcessRecord(recordToProcess, loggerWithPrefix, errorManager)?.Count() ?? 0;
								if (errorManager.ErrorList.Any() || createdCount == 0)
								{
									processRecordFailed = true;
								}
							}
							catch (InvalidOperationException ex) when (ex.Message == "Invalid DtbBooking Parent Table Code" || ex.Message.EndsWith(" is not a valid Transport Booking parent", StringComparison.InvariantCultureIgnoreCase))
							{
								processRecordFailed = true;
								errorManager.AddError(DtbBookingCreationErrorType.InvalidParentError, ex);
							}
						}
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				processRecordFailed = true;
				errorManager.AddError(DtbBookingCreationErrorType.UnknownError, ex);
			}

			if (processRecordFailed || recordToProcess == null)
			{
				if (recordToProcess == null)
				{
					if (processRecordFailed)
					{
						loggerWithPrefix.Error("Exception occurred while getting next queue record to process: " + errorManager.MessageToLog);
					}
					return false;
				}
			}
			else
			{
				LogSuccessfulCreation(loggerWithPrefix, createdCount, createdType, identifyingDescription);
			}

			if (processRecordFailed)
			{
				if (recordToProcess != null)
				{
					var messageToLog = errorManager.MessageToLog;
					if (string.IsNullOrEmpty(messageToLog))
					{
						messageToLog = reasonForJobCreationFailure;
						if (string.IsNullOrEmpty(messageToLog))
						{
							messageToLog = "No " + createdType + "s were created" + identifyingDescription;
						}
					}
					var errorMessageIntro = "Exception occurred during processing" + identifyingDescription;
					if (errorManager.Retry)
					{
						if (recordToProcess.Iteration < GetMaxRetries())
						{
							errorMessageIntro += ", queue record has been iterated: ";
							iterateQueueRecord = true;
						}
						else
						{
							errorMessageIntro += " and has failed the maximum number of times, queue record deleted: ";
							deleteQueueRecord = true;
						}
					}
					else
					{
						errorMessageIntro += ", queue record deleted: ";
						deleteQueueRecord = true;
					}

					errorMessageToLog = errorMessageIntro + messageToLog;
					if (errorManager.SendToErrorReporter)
					{
						errorMessageToSendToErrorReporter = errorMessageIntro + errorManager.MessageToSendToErrorReporter;
					}
				}
			}
			else
			{
				deleteQueueRecord = true;
			}

			if (deleteQueueRecord)
			{
				DeleteSpecificQueueRecord(recordToProcess.PK);
			}
			else if (iterateQueueRecord)
			{
				UpdateIterationOnRecord(recordToProcess.PK, recordToProcess.Iteration);
			}

			if (!string.IsNullOrEmpty(errorMessageToLog))
			{
				loggerWithPrefix.Error(errorMessageToLog);
			}
			if (!string.IsNullOrEmpty(errorMessageToSendToErrorReporter))
			{
				ErrorReporter.ReportOnce(errorMessageToSendToErrorReporter, errorManager.LastException);
			}

			if (recordToProcess != null)
			{
				LogCreationRequest(loggerWithPrefix, "Ended", createdType, identifyingDescription);
			}

			return true;
		}

		void LogSuccessfulCreation(LoggerWithPrefix loggerWithPrefix, int createdCount, string createdType, string identifyingDescription)
		{
			loggerWithPrefix.Information($"Successfully created {createdCount} {createdType}(s){identifyingDescription}.");
		}

		void LogCreationRequest(LoggerWithPrefix loggerWithPrefix, string type, string createdType, string identifyingDescription)
		{
			loggerWithPrefix.Information($"{type} processing {createdType} creation request{identifyingDescription}.");
		}

		DisposableAction SetProcessingContext(string branchCode, LoggerWithPrefix loggerWithPrefix)
		{
			IDisposable disposableEnvironment = null;

			Action createAction = () =>
			{
				var branchFactory = new BusinessObjectFactory();
				var branchQuery = new ZQuery(GlbBranchSchema.GB_Code, branchCode);
				var processingBranch = branchFactory.LoadTop1<GlbBranch>(branchQuery);
				disposableEnvironment = DisposableEnvironment.ForBranch(processingBranch.PK.ToGuid());
				loggerWithPrefix.Prefix = branchCode;
			};

			Action disposeAction = () =>
			{
				disposableEnvironment.Dispose();
			};

			return new DisposableAction(createAction, disposeAction);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		QueueRecord GetNextQueueRecordToProcess()
		{
			QueueRecord record = null;
			var sql = "SELECT TOP(1) * FROM dbo.DtbBookingQueue WHERE KMQ_Iteration = 0 ORDER BY KMQ_SystemCreateTimeUtc";
			var cmd = Db.Connection.Command(sql);
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.Read())
				{
					record = GetRecordData(reader);
				}
			}
			return record;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		QueueRecord GetSpecificQueueRecord(Guid pkToFind)
		{
			QueueRecord record = null;
			var sql = "SELECT * FROM dbo.DtbBookingQueue WHERE KMQ_PK = @PK ";
			var cmd = Db.Connection.Command(sql);
			cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pkToFind);
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.Read())
				{
					record = GetRecordData(reader);
				}
			}
			return record;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		QueueRecord DeleteSpecificQueueRecord(Guid pkToDelete)
		{
			QueueRecord record = null;
			var sql = "DELETE FROM dbo.DtbBookingQueue WHERE KMQ_PK = @PK ";
			var cmd = Db.Connection.Command(sql);
			cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pkToDelete);
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.Read())
				{
					record = GetRecordData(reader);
				}
			}
			return record;
		}

		QueueRecord GetRecordData(IDataReader reader)
		{
			var pk = (Guid)reader[DtbBookingQueueSchema.Constants.PK];
			var parentID = (Guid)reader[DtbBookingQueueSchema.Constants.KMQ_ParentID];
			var parentTableCode = (string)reader[DtbBookingQueueSchema.Constants.KMQ_ParentTableCode];
			var branchCode = (string)reader[DtbBookingQueueSchema.Constants.KMQ_GB_NKBranch];
			var direction = (string)reader[DtbBookingQueueSchema.Constants.KMQ_Direction];
			var combineContainers = (bool)reader[DtbBookingQueueSchema.Constants.KMQ_CombineContainers];
			var iteration = Convert.ToInt16((byte)reader[DtbBookingQueueSchema.Constants.KMQ_Iteration]);
			var targetModule = (string)reader[DtbBookingQueueSchema.Constants.KMQ_TargetModule];

			return new QueueRecord(pk, parentID, parentTableCode, branchCode, direction, combineContainers, iteration, targetModule);
		}

		string GetCreatedType(QueueRecord record)
		{
			if (record == null)
			{
				return string.Empty;
			}

			switch (record.TargetModule)
			{
				case AutoCreatorTargetModules.Codes.LandTransportConsignment:
					return (NoResString)"Land Transport Consignment";
				case AutoCreatorTargetModules.Codes.PortTransport:
					return (NoResString)"Port Transport";
				default:
					return (NoResString)"Transport Booking";
			}
		}

		string GetIdentifyingDescription(QueueRecord record)
		{
			if (record == null)
			{
				return string.Empty;
			}

			return (NoResString)" for Parent " + GetEntityDescription(record.ParentTableCode, record.ParentID) + (NoResString)", job number " + GetJobNumber(record.ParentID, record.ParentTableCode);
		}

		IEnumerable<IDtbBooking> ProcessRecord(QueueRecord record, ILogger logger, IAutomatedDtbBookingCreationErrorManager errorManager)
		{
			var parent = GetBookingParent(record.ParentID, record.ParentTableCode);
			var factory = new BusinessObjectFactory();
			var dtbDeliveryManager = deliveryManagerFactory.CreateManager(factory, parent, (DtbBookingDirection)Enum.Parse(typeof(DtbBookingDirection), record.Direction), record.CombineContainers, logger, errorManager);
			return dtbDeliveryManager.DeliverTransportBooking();
		}

		IDtbBookingParent GetBookingParent(Guid parentID, string parentTableCode)
		{
			switch (parentTableCode)
			{
				case JobShipmentSchema.Constants.Prefix:
					return GetShipmentBookingParentAsIDtbBookingParent(parentID);

				case JobConsolSchema.Constants.Prefix:
					return (IDtbBookingParent)Factory.LoadTop1<IForwardingConsol>(new ZQuery(JobConsolSchema.PK, parentID));

				case JobDeclarationSchema.Constants.Prefix:
					return (IDtbBookingParent)Factory.LoadTop1<IBaseJobDeclaration>(new ZQuery(JobDeclarationSchema.PK, parentID) { IgnoreActiveFilter = true });

				case WhsDocketSchema.Constants.Prefix:
					return (IDtbBookingParent)Factory.LoadTop1<IWhsDocket>(new ZQuery(WhsDocketSchema.PK, parentID));

				default:
					throw new InvalidOperationException("Invalid DtbBooking Parent Table Code");
			}
		}

		BusinessObject GetShipmentBookingParent(ZGuid parentID)
		{
			var shipmentType = GetShipmentType(parentID);

			return Factory.Load(shipmentType, parentID);
		}

		IDtbBookingParent GetShipmentBookingParentAsIDtbBookingParent(ZGuid parentID)
		{
			var shipmentType = GetShipmentType(parentID);

			if (shipmentType.GetInterface(nameof(IDtbBookingParent)) == null)
			{
				throw new InvalidOperationException("Shipment type " + shipmentType.Name + " is not a valid Transport Booking parent");
			}

			return (IDtbBookingParent)Factory.Load(shipmentType, parentID);
		}

		Type GetShipmentType(ZGuid shipmentPK)
		{
			var commonShipment = Factory.LoadTop1<ICommonShipment>(new ZQuery(JobShipmentSchema.PK, shipmentPK) { IgnoreActiveFilter = true });
			return CommonShipment.TypeDecider.GetTypeForLoad(((IBusinessObjectInternals)commonShipment).Row, Factory);
		}

		ViewTransportBookingParents GetViewParentsRecord(ZGuid parentPK)
		{
			var query = new ZQuery(ViewTransportBookingParentsSchema.PK, parentPK);
			return Factory.LoadTop1<ViewTransportBookingParents>(query);
		}

		string GetJobNumber(Guid parentID, string parentTableCode)
		{
			var jobNumber = string.Empty;
			try
			{
				var viewRecord = GetViewParentsRecord(parentID);
				if (viewRecord != null)
				{
					jobNumber = viewRecord.VP_JobNumber;
				}
				else if (parentTableCode == JobShipmentSchema.Constants.Prefix)
				{
					var parent = Factory.Load<CommonShipment>(parentID);
					jobNumber = parent?.JS_UniqueConsignRef;
				}
				else if (parentTableCode == DtbBookingSchema.Constants.Prefix)
				{
					var parent = Factory.Load<DtbBooking>(parentID);
					jobNumber = parent?.KM_JobID;
				}
				else
				{
					var parent = GetBookingParent(parentID, parentTableCode);
					jobNumber = parent?.JobNumber ?? "PK: " + parentID.ToString();
				}
			}
			catch (InvalidOperationException)
			{
				jobNumber = "PK: " + parentID.ToString();
			}
			return jobNumber;
		}

		string GetEntityDescription(string parentTableCode, ZGuid parentPK)
		{
			if (parentTableCode == JobShipmentSchema.Constants.Prefix)
			{
				var shipmentParent = GetShipmentBookingParent(parentPK);
				if (shipmentParent is IBillOfLading)
				{
					return (NoResString)"Bill of Lading";
				}
				else if (shipmentParent is IAgencyShipment)
				{
					return (NoResString)"Agency Shipment";
				}
				else if (shipmentParent is IForwardingShipment)
				{
					return (NoResString)"Forwarding Shipment";
				}
				else
				{
					return shipmentParent.GetType().Name;
				}
			}
			else
			{
				var parentInView = GetViewParentsRecord(parentPK);
				if (parentInView != null)
				{
					return parentJobTypes.GetDescriptionFromCode(parentInView.VP_JobType);
				}

				switch (parentTableCode)
				{
					case JobConsolSchema.Constants.Prefix:
						return (NoResString)"Forwarding Consolidation";

					case JobDeclarationSchema.Constants.Prefix:
						return (NoResString)"Customs Declaration";

					case WhsDocketSchema.Constants.Prefix:
						return (NoResString)"Warehouse Docket";

					case DtbBookingSchema.Constants.Prefix:
						return (NoResString)"Transport Booking";

					default:
						return (NoResString)"Prefix: " + parentTableCode;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void UpdateIterationOnRecord(ZGuid pk, short iteration)
		{
			var updateSql = "UPDATE dbo.DtbBookingQueue SET KMQ_Iteration = " + (iteration + 1).ToString() + " WHERE KMQ_PK = @PK ";
			var cmd = Db.Connection.Command(updateSql);
			cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk.ToGuid());
			cmd.ExecuteNonQuery();
		}

		short GetMaxRetries()
		{
			return DefaultMaxRetries;
		}

		internal BusinessObjectFactory Factory { get; }
		readonly IDtbDeliveryManagerFactory deliveryManagerFactory;
		readonly CodeDescriptionPairList parentJobTypes;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message string")]
		const string CancellationNotificationMessage = "Service task was cancelled by request.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Sql string")]
		const string OutputDeletedClause = "OUTPUT deleted.KMQ_PK, deleted.KMQ_ParentID, deleted.KMQ_ParentTableCode, deleted.KMQ_GB_NKBranch, deleted.KMQ_Direction, deleted.KMQ_CombineContainers, deleted.KMQ_Iteration ";
		const int DefaultMaxRetries = 3;

#if DEBUG
		// for testing only
		public event EventHandler BeginProcessRecordInMainLoop;
		public event EventHandler EndProcessRecordInMainLoop;
#endif

		class QueueRecord
		{
			public QueueRecord(Guid pk, Guid parentID, string parentTableCode, string branchCode, string direction, bool combineContainers, short iteration, string targetModule)
			{
				PK = pk;
				ParentID = parentID;
				ParentTableCode = parentTableCode;
				BranchCode = branchCode;
				Direction = direction;
				CombineContainers = combineContainers;
				Iteration = iteration;
				TargetModule = targetModule;
			}

			public Guid PK { get; }
			public Guid ParentID { get; }
			public string ParentTableCode { get; }
			public string BranchCode { get; }
			public string Direction { get; }
			public bool CombineContainers { get; }
			public short Iteration { get; }
			public string TargetModule { get; }
		}

		class WrappedNotifications : INotifications
		{
			public WrappedNotifications() { }

			void INotifications.Add(INotification notification)
			{
				if (notification.Type == CargoWise.ComponentModel.NotificationType.Error)
				{
					ErrorList.Add(notification);
				}
			}

			public List<INotification> ErrorList { get; set; } = new List<INotification>();
		}
	}
}
