using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.SailingScheduleDataVendor
{
	[Serializable]
	internal abstract class VoyageSubscriptionUpdater : LogSubscriber, IMessageProcessorCommunicationModesResult
	{
		#region Constructors

		public VoyageSubscriptionUpdater()
		{
		}

		#endregion

		#region LogSubscriber

		public override string[] EventTypes
		{
			get { return new[] { AutoEvents.SubscriptionRequested.Code }; }
		}

		public override string[] TableNames
		{
			get { return new[] { BusinessObjectFactory.GetTableNameFromType(GetBusinessObjectType(), false) }; }
		}

		/// <summary>
		///		Returns the type of a business object to handle subscription for.
		/// </summary>
		protected abstract Type GetBusinessObjectType();

		protected abstract JobVoyage GetParentVoyage(BusinessObject businessObject);

		protected abstract ITopLevelDataObjectWriter GetDataObjectWriter(IDataWritingManager manager);

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			if (IsTrackingEnabled)
			{
				if (IsEHubIDSet)
				{
					var businessObjectType = GetBusinessObjectType();
					var schema = BusinessObjectFactory.GetTableSchemaFromType(businessObjectType);
					var factory = queuedLogs[0].Factory;
					var scheduleFeedLogs = queuedLogs.Where(IsScheduleFeedRequest).ToList();
					var parentsQuery = new ZQuery(schema.PK, scheduleFeedLogs.Select(l => l.SJ_ParentID));
					var parents = factory.Load(businessObjectType, parentsQuery);

					foreach (var parent in parents)
					{
						var voyage = GetParentVoyage(parent);

						if (ValidateVoyage(voyage))
						{
							var log = scheduleFeedLogs.First(l => l.SJ_ParentID == parent.PK);
							SendSubscriptionRequest(voyage, parent, log);
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
				DefaultLogger.Log(LogType.Information, "Subscription requests cannot be sent as Schedule Feed Tracking is disabled.");
			}
		}

		bool ValidateVoyage(JobVoyage voyage)
		{
			if (voyage == null)
			{
				return false;
			}

			if (!voyage.IsSea)
			{
				DefaultLogger.Log(LogType.Error, "The Transport Mode is wrong: " + voyage.JV_AirSeaRoad);
				return false;
			}

			if (voyage.JV_VoyageFlight.ToString().All(x => x == ' ' || x == '0'))
			{
				DefaultLogger.Log(LogType.Error, "The Voyage Number consists of spaces and/or zeros only: " + voyage.JV_VoyageFlight);
				return false;
			}

			var foundCarrierCode = false;

			var factory = voyage.Factory;
			var orgHeader = factory.Load<OrgHeader>(voyage.JV_OH_Line);

			if (orgHeader != null)
			{
				foreach (OrgCusCode orgCusCode in orgHeader.CustomsCodes)
				{
					if (orgCusCode.OK_RN_NKCodeCountry == "US" && orgCusCode.OK_CodeType == OrgCusCode.CodeTypes.CarrierCode)
					{
						if (orgCusCode.OK_CustomsRegNo.Length == 4)
						{
							foundCarrierCode = true;
						}
						else
						{
							DefaultLogger.Log(LogType.Error, "The carrier code is not correct: " + orgCusCode.OK_CustomsRegNo);
							return false;
						}
					}
				}
			}

			if (!foundCarrierCode)
			{
				DefaultLogger.Log(LogType.Error, "The carrier code not found");
				return false;
			}

			return true;
		}

		#endregion

		#region Internal

		static bool IsScheduleFeedRequest(IQueuedLog log)
		{
			var parameters = StmALog.GetParametersFromReference(log.SJ_Reference);
			string type;

			return parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, out type)
				&& StringComparer.InvariantCultureIgnoreCase.Compare(type, Constants.EventReferenceParameterTypes.ScheduleFeed) == 0;
		}

		void SendSubscriptionRequest(JobVoyage voyage, BusinessObject parent, IQueuedLog log)
		{
			DefaultLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Processing {0}", parent.HumanReadableName));

			var stmLog = GetSourceLog(parent, log);
			var actionInfo = new LogSubscriptionActionWrapper(voyage);
			Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter = this.GetDataObjectWriter;
			var processor = UniversalXmlWorkflowProcessorBuilder.New(actionInfo, this, dataWriterGetter, stmLog, null, null, UniversalXmlSchema.Version_2012_11_DO_NOT_USE);

			var replaceThisTokenEventuallyQuestionMarkExclamationMark = CancellationToken.None;
			processor.Process(GetINotificationsWrapperAroundILogger(), replaceThisTokenEventuallyQuestionMarkExclamationMark);
		}

		StmALog GetSourceLog(BusinessObject parent, IQueuedLog log)
		{
			var query = new ZQuery(StmALogSchema.SL_EventTime, log.SJ_EventTime)
			.AddToFilter(StmALogSchema.SL_Parent, log.SJ_ParentID)
			.AddToFilter(StmALogSchema.SL_Table, ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchemaFromColumnNamePrefix(log.SJ_ParentTableCode).TableName)
			.AddToFilter(StmALogSchema.PK, log.SJ_ALogReference);
			return parent.Factory.LoadTop1<StmALog>(query);
		}

		static bool IsTrackingEnabled
		{
			get
			{
				return FreightDataRegistry.Instance.EnableScheduleFeedService.Value;
			}
		}

		static bool IsEHubIDSet
		{
			get
			{
				return !string.IsNullOrEmpty(FreightDataRegistry.Instance.ScheduleFeedTrackingEHubID.Value);
			}
		}

		IEnumerable<NonPersistentEDICommunicationMode> CommunicationModes
		{
			get
			{
				if (communicationModes == null)
				{
					communicationModes = new[]
					{
						new NonPersistentEDICommunicationMode
						{
							EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService,
							EK_Destination = FreightDataRegistry.Instance.ScheduleFeedTrackingEHubID.Value,
						}
					};
				}

				return communicationModes;
			}
		}

		public IList<IMessageDestinationSource> Destinations => CommunicationModes.ToArray();

		public MultilingualString ConfigurationLogging => null;

		IEnumerable<NonPersistentEDICommunicationMode> communicationModes;

		#endregion
	}
}
