using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class EventDataObjectWriter : TopLevelDataObjectWriter<BaseStmALog, UniversalEvent>, IEventDataObjectWriter
	{
		public EventDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
			actionType = writeManager.Action.ActionType;
			recipientRoleTypes = writeManager.Action.RecipientRoleDetails?.Select(r => r.Type) ?? Enumerable.Empty<RecipientRoleType>();
			recipientOrganisation = writeManager.Action.RecipientOrganization;
			parentBO = Argument.NotNull(writeManager.Action.ParentBO, "writeManager.Action.ParentBO");
			var dataContextManager = parentBO.GetUniversalDataContextManager()
				?? throw new InvalidOperationException(string.Format("The type of the parentBO passed in must have the UniversalDataContextAttribute applied. Type: [{0}] PK: [{1}]", parentBO.GetType().FullName, parentBO.PK.ToString()));
			dataContextType = dataContextManager.DataContextType;
		}

		readonly DataContextType dataContextType;
		readonly BusinessObject parentBO;
		readonly string actionType;
		readonly IEnumerable<RecipientRoleType> recipientRoleTypes;
		readonly IOrgHeader recipientOrganisation;

		public bool PopulateAdditionalContexts { get; set; }

		void PopulateAdditionalContextValues(UniversalEvent logData, IParentEventDataContextManager parentEventDataContextManager)
		{
			logData.AdditionalContextCollection = new List<AdditionalContext>();
			var childContextManagers = parentEventDataContextManager.ChildContextManagers;
			if (childContextManagers != null)
			{
				logData.AdditionalContextCollection.AddRange(childContextManagers.Select(childContextManager => new AdditionalContext
				{
					DataContext = DataContextFactory.New(childContextManager, writeManager.Schema.Namespace),
					ContextCollection = GetContexts(childContextManager.EventContextValues)
				}));
			}
		}

		static List<Context> GetContexts(IEnumerable<KeyValuePair<TypeWithDescription, IZType>> contextValues)
		{
			return contextValues != null
				? contextValues.Select(o => new Context { Type = new ContextType { Type = o.Key.Type, Description = o.Key.Description }, Value = SimpleTypeFormatter.FormatForMultiTypedStringElementTargetInDataObject(o.Value) }).ToList()
				: new List<Context>();
		}

		protected override void PopulateDataObject(BaseStmALog logBO, UniversalEvent logData)
		{
			if (logBO.SL_PostedTimeUtc.IsValid)
			{
				logData.CreatedTime = logBO.SL_PostedTimeUtc.ToOffset();
			}

			PopulateEventReference(logBO, logData);

			logData.EventTime = logBO.EventTimeOffset;
			logData.EventType = logBO.SL_SE_NKEvent;
			logData.IsEstimate = logBO.SL_IsEstimate;

			if (parentBO.GetUniversalDataContextManager() is IEventDataContextManager dataSourceManager)
			{
				using (SetUpTriggeringLogOnDataContextManager(dataSourceManager, logBO))
				{
					logData.ContextCollection = GetContexts(dataSourceManager.EventContextValues);
				}

				EDIMessageDeliveryContextWriter.PopulateMessageDeliveryContextValues(writeManager.Action, logData.ContextCollection);

				PopulateDataTargets(dataSourceManager, logData);

				var parentEventDataContextManager = dataSourceManager as IParentEventDataContextManager;
				if (PopulateAdditionalContexts && parentEventDataContextManager != null)
				{
					PopulateAdditionalContextValues(logData, parentEventDataContextManager);
				}

				var additionalFieldsToUpdateValues = dataSourceManager.AdditionalFieldsToUpdateValues;
				if (additionalFieldsToUpdateValues != null && additionalFieldsToUpdateValues.Any())
				{
					logData.AdditionalFieldsToUpdateCollection = additionalFieldsToUpdateValues.Select(o => new AdditionalFieldToUpdate { Type = new ZString(o.Key), Value = SimpleTypeFormatter.FormatForMultiTypedStringElementTargetInDataObject(o.Value) }).ToList();
				}
			}

			if (actionType == WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXMLWithEDoc && parentBO is IDocManagerSupport docManagerSupport)
			{
				var storageDocsPK = StmALog.GetGuid(logBO.ReferenceFreeText);
				if (storageDocsPK.IsValid)
				{
					IeDoc eDoc = FindEDocs(docManagerSupport, storageDocsPK.ToGuid());
					if (eDoc != null)
					{
						var attachedDocuments = new AttachedDocumentDataObjectWriter().GenerateAttachedDocuments(false, eDoc);
						logData.AttachedDocumentCollection = new List<AttachedDocument>(attachedDocuments);
					}
				}
			}
		}

		void PopulateDataTargets(IEventDataContextManager dataSourceManager, UniversalEvent eventDO)
		{
			foreach (var recipientRoleType in recipientRoleTypes)
			{
				var universalLinks = dataSourceManager.GetEventDataTarget(recipientRoleType, recipientOrganisation);
				foreach (var universalLink in universalLinks)
				{
					if (!universalLink.Key.IsEmpty
						&& !universalLink.EnterpriseCode.IsEmpty
						&& !universalLink.ServerCode.IsEmpty
						&& !universalLink.CompanyCode.IsEmpty)
					{
						eventDO.DataContext.AddDataTargetAndSetCompanyAndDataProviderDetails(universalLink);
					}
				}
			}
		}

		IDisposable SetUpTriggeringLogOnDataContextManager(IEventDataContextManager dataSourceManager, BaseStmALog logBO)
		{
			IDisposable result = null;

			var dataContextManagerWithTriggeringLog = dataSourceManager as IEventDataContextManagerWithTriggeringLog;
			if (dataContextManagerWithTriggeringLog != null)
			{
				dataContextManagerWithTriggeringLog.TriggeringLogForUseInPopulatingEventContext = logBO;
				result = new DisposableAction(() => dataContextManagerWithTriggeringLog.TriggeringLogForUseInPopulatingEventContext = null);
			}

			return result;
		}

		void PopulateEventReference(BaseStmALog logBO, UniversalEvent logData)
		{
			if (writeManager.Schema == UniversalXmlSchema.Version_2012_11_DO_NOT_USE)
			{
				var eventParameters = new EventParameters();
				var unsupportedParameters = new Dictionary<string, string>();
				var hasParameters = false;

				foreach (var pair in logBO.Parameters)
				{
					var property = EventParameters.GetPropertyByCode(pair.Key);
					if (property != null)
					{
						var parameterValue = StmALog.ParseParameterValue(pair, Nullable.GetUnderlyingType(property.PropertyType));

						property.SetValue(eventParameters, parameterValue, null);

						hasParameters = true;
					}
					else
					{
						unsupportedParameters[pair.Key] = pair.Value;
					}
				}

				if (hasParameters)
				{
					logData.EventParameters = eventParameters;
				}

				if (!logBO.ReferenceFreeText.IsEmpty || unsupportedParameters.Any())
				{
					logData.EventReference = StmALog.GenerateEventReference(logBO.ReferenceFreeText, unsupportedParameters);
				}
			}
			else
			{
				if (!logBO.SL_Reference.IsEmpty)
				{
					logData.EventReference = logBO.SL_Reference;
				}
			}
		}

		internal static IeDoc FindEDocs(IDocManagerSupport docManagerSupport, Guid docPK)
		{
			IeDoc eDoc = docManagerSupport.DocManagerInfo.AllEDocs.GetFromUniqueKey(docPK);
			return eDoc ?? FindEdocsFromRelatedEvents(docManagerSupport as IStmALogParent, docPK);
		}

		static IeDoc FindEdocsFromRelatedEvents(IStmALogParent logParent, Guid docPK)
		{
			IeDoc eDoc = null;
			if (logParent != null)
			{
				foreach (var relatedBizo in logParent.BusinessObjectsWithRelatedEvents)
				{
					var docManagerSupport = relatedBizo as IDocManagerSupport;
					if (docManagerSupport != null && (eDoc = docManagerSupport.DocManagerInfo.AllEDocs.GetFromUniqueKey(docPK)) != null)
					{
						break;
					}
				}
			}
			return eDoc;
		}

		protected override IDataContextManager GetDataContextManager(BusinessObject sourceBO)
		{
			return base.GetDataContextManager(parentBO);
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalEvent;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return dataContextType;
		}
	}
}
