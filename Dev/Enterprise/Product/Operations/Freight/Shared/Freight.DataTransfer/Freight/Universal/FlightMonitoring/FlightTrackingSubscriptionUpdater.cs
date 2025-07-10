using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer.Universal.FlightMonitoring
{
	[Serializable]
	internal abstract class FlightTrackingSubscriptionUpdater : LogSubscriber
	{
		#region LogSubscriber

		public override string[] EventTypes => new[] { AutoEvents.SubscriptionRequested.Code };

		public override string[] TableNames => new[] { BusinessObjectFactory.GetTableNameFromType(GetBusinessObjectType(), false) };

		protected abstract Type GetBusinessObjectType();

		protected abstract IEnumerable<ITransportParent> GetRelatedTransportParents(BusinessObjectFactory factory, IEnumerable<IQueuedLog> queuedLogs);

		protected abstract IQueuedLog GetParentLog(ITransportParent transportParent, IEnumerable<IQueuedLog> queuedLogs);

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			if (IsTrackingEnabled)
			{
				if (IsEHubIDSet)
				{
					var flightTrackingFeedLogs = queuedLogs.Where(IsFlightTrackingFeedRequest).ToList();

					if (flightTrackingFeedLogs.Count > 0)
					{
						var businessObjectFactory = new ReadOnlyBusinessObjectFactory { RefreshEnabled = false, NameForDebugging = "DummyLogForAWBTracking" };
						var factory = flightTrackingFeedLogs[0].Factory;
						var parents = GetRelatedTransportParents(factory, flightTrackingFeedLogs);

						foreach (var parent in parents.Where(p => HasValidMAWBNumber(p) || IsApplicableForConsolSubscription(p)))
						{
							var log = GetParentLog(parent, flightTrackingFeedLogs);
							SendSubscriptionRequest(businessObjectFactory, parent, log);
						}
					}
				}
				else
				{
					DefaultLogger.Log(LogType.Warning, "Subscription requests cannot be sent as eHub ID is not set.");
				}
			}
			else
			{
				DefaultLogger.Log(LogType.Information, "Subscription requests cannot be sent as Flight Monitoring System is disabled.");
			}
		}

		#endregion

		#region Internal

		static bool HasValidMAWBNumber(ITransportParent transportParent)
		{
			var result = false;
			if (transportParent != null)
			{
				var mawbNumber = transportParent.TransportSupporter?.BillOfLading ?? ZString.Empty;
				result = !mawbNumber.IsEmpty && MasterBillValidator.GetMAWBFormatValidMessage(mawbNumber, transportParent.Factory).IsEmpty;
			}

			return result;
		}

		static bool IsApplicableForConsolSubscription(ITransportParent transportParent)
		{
			return transportParent is CommonConsol consol && consol.IsAllDataValidForFlightTrackingSubscription;
		}

		internal static bool IsFlightTrackingFeedRequest(IQueuedLog log)
		{
			var parameters = StmALog.GetParametersFromReference(log.SJ_Reference);

			return parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, out var type)
				&& StringComparer.OrdinalIgnoreCase.Compare(type, Constants.EventReferenceParameterTypes.AWBAutomation) == 0;
		}

		void SendSubscriptionRequest(ReadOnlyBusinessObjectFactory businessObjectFactory, ITransportParent parent, IQueuedLog log)
		{
			var transportParent = parent as BusinessObject;

			DefaultLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Processing {0}", transportParent.HumanReadableName));

			Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter = (outboundSessionTracker) => new FlightTrackingEventDataObjectWriter(outboundSessionTracker, parent);

			var dummyLog = NonPersistentStmALog.GetDummyLog(businessObjectFactory, transportParent, log, Constants.EventReferenceParameterTypes.AWBAutomation);

			var user = businessObjectFactory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, log.SJ_GS_NKUser)
				?? GlbStaff.CurrentUser;

			var branch = businessObjectFactory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, log.SJ_GB_NKBranch)
				?? GlbBranch.CurrentBranch;

			var department = businessObjectFactory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, log.SJ_GE_NKDepartment)
				?? GlbDepartment.CurrentDepartment;

			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				var processor = UniversalXmlWorkflowProcessorBuilder.New(
					new LogSubscriptionActionWrapper(transportParent),
					new UniversalXmlCommunicationModeProvider(() => (CommunicationModes.ToArray(), null)),
					dataWriterGetter,
					dummyLog,
					null,
					null,
					UniversalXmlSchema.Version_2012_11_DO_NOT_USE);

				var replaceThisTokenEventuallyQuestionMarkExclamationMark = CancellationToken.None;
				processor.Process(GetINotificationsWrapperAroundILogger(), replaceThisTokenEventuallyQuestionMarkExclamationMark);
			}
		}

		internal static bool IsTrackingEnabled => FreightDataRegistry.Instance.AWBTracking.Value;

		internal static bool IsEHubIDSet => !string.IsNullOrEmpty(FreightDataRegistry.Instance.AWBTrackingEHubID.Value);

		IEnumerable<NonPersistentEDICommunicationMode> CommunicationModes => communicationModes ?? (communicationModes = new[]
		{
			new NonPersistentEDICommunicationMode
			{
				EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService,
				EK_Destination = FreightDataRegistry.Instance.AWBTrackingEHubID.Value,
			}
		});

		IEnumerable<NonPersistentEDICommunicationMode> communicationModes;

		#endregion
	}
}
