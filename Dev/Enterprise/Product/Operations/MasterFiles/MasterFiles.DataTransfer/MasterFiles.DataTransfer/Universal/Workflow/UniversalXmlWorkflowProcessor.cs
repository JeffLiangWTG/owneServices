using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NotificationType = CargoWise.ComponentModel.NotificationType;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using XmlWriter = Enterprise.UniversalDataBuss.XmlIO.XmlWriting.XmlWriter;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Workflow
{
	public partial class UniversalXmlWorkflowProcessor : UniversalXmlWriter, IUniversalXmlWorkflowProcessor, ISupportUniversalBatchExport, IMessageProcessor, IUniversalXmlContentFilterApplicatorSuspendable
	{
		public UniversalXmlWorkflowProcessor(IUniversalActionInfo actionInfo, IMessageProcessorCommunicationModesResult communicationModesGetter, Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter, BusinessObject exportedBO, IEventInfo eventInfo = null, IXmlWriter xmlWriter = null, IUniversalXmlSchema schema = null)
			: base(actionInfo, dataWriterGetter, exportedBO, eventInfo)
		{
			this.communicationModesGetter = Argument.NotNull(communicationModesGetter, "IEDICommunicationsMode[] communicationModes");
			this.xmlWriter = xmlWriter ?? new XmlWriter();
			this.schema = schema;
		}

		readonly IMessageProcessorCommunicationModesResult communicationModesGetter;
		readonly IXmlWriter xmlWriter;
		readonly IUniversalXmlSchema schema;

		IList<IEDICommunicationsMode> CommunicationModes => communicationModesGetter.Destinations.Cast<IEDICommunicationsMode>().ToList();

		IMessageProcessorCommunicationModesResult IMessageProcessor.GetDestinations() => communicationModesGetter;

		void IUniversalXmlWorkflowProcessor.AddAdditionalTriggerParty(ZString partyCode, ZString partyService) => actionInfo.PopulateRecipientRoleDetails(partyCode, partyService);

		#region ISupportUniversalBatchExport

		void ISupportUniversalBatchExport.AddAnotherExportedBusinessObject(BusinessObject anotherExportedBO)
		{
			var ediMessageSubType = dataWriterGetter(new DataWritingManager(actionInfo, schema: schema)).EDIMessageSubType;

			if (ediMessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalShipment)
			{
				if (anotherExportedBO.GetType() != exportedBO.GetType())
				{
					throw new ArgumentException("All exported objects have to be of the same type");
				}
			}
			else if (ediMessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalTransaction)
			{
				if (!(anotherExportedBO is AccTransactionHeader))
				{
					throw new ArgumentException("All exported objects have to be transaction header.");
				}
			}
			else
			{
				throw new InvalidOperationException(ediMessageSubType + " message is not supported for batch export.");
			}

			if (anotherExportedBO != null && anotherExportedBO != exportedBO)
			{
				if (anotherExportedBO.Factory != exportedBO.Factory)
				{
					throw new ArgumentException("All exported objects have to have the same Factory");
				}

				if (!anotherExportedBOs.Contains(anotherExportedBO))
				{
					anotherExportedBOs.Add(anotherExportedBO);
				}
			}
		}

		readonly List<BusinessObject> anotherExportedBOs = new List<BusinessObject>();

		void ISupportUniversalBatchExport.AddAnotherTopLevelDataObjectWriterAndXmlWriter(ITopLevelDataObjectWriter topLevelDataObjectWriter, IXmlWriter xmlWriter)
		{
			var ediMessageSubType = dataWriterGetter(new DataWritingManager(actionInfo, schema: schema)).EDIMessageSubType;

			if (ediMessageSubType != topLevelDataObjectWriter.EDIMessageSubType)
			{
				throw new ArgumentException("All top level data object writer have to have the same EDI message sub type.");
			}

			anotherTopLevelDataObjectWritersAndXmlWriters.Add((topLevelDataObjectWriter, xmlWriter));
		}

		readonly List<(ITopLevelDataObjectWriter, IXmlWriter)> anotherTopLevelDataObjectWritersAndXmlWriters = new List<(ITopLevelDataObjectWriter, IXmlWriter)>();

		#endregion

#if DEBUG

		internal static readonly Overridable<bool> ShouldSaveResultMessageForTest = new Overridable<bool>(false);
		public static readonly Overridable<bool> PreventTemporarilyResumeValidation_ForTestOnly = new Overridable<bool>(false);

		public IXmlEventValueObject[] Process(INotifications notifications)
		{
			var value = eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.Value;
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value ?? "http://ftp://https://edinet://"))
			{
				return Process(notifications, CancellationToken.None);
			}
		}

#endif

		void IProcessor.Process(INotifications notifications, CancellationToken token) => Process(notifications, token);

		/// <summary>
		/// Called by Workflow, is used to push Universal XML from one module both internally and externally.
		/// </summary>
		/// <param name="notifications">A logger that allows you to show notifications in the Log Walker service task when WorkFlow is being processed.</param>
		public IXmlEventValueObject[] Process(INotifications notifications, CancellationToken token)
		{
			actionInfo.Notifications = notifications;
			return ProcessCore(notifications, null, token, false).Cast<IXmlEventValueObject>().ToArray();
		}

		/// <summary>
		/// Called when module specific interfaces need to push Universal Event from a module directly. Can also push internally and/or externally.
		/// </summary>
		public static PublishUniversalXmlResult PublishUniversalEvent(BusinessObjectFactory factory, RecipientRoleType[] recipientRoleTypes, IWorkflowProviderEvent workflowAndDataProvider, StmALog eventBO)
		{
			return PublishUniversalEvent(factory, recipientRoleTypes, workflowAndDataProvider, workflowAndDataProvider.RecipientOrganisations, eventBO);
		}

		/// <summary>
		/// Called when module specific interfaces need to push Universal Event from a module directly. Can also push internally and/or externally.
		/// </summary>
		public static PublishUniversalXmlResult PublishUniversalEvent(BusinessObjectFactory factory, RecipientRoleType[] recipientRoleTypes, IWorkflowProvider workflowAndDataProvider, OrgHeader[] recipientOrganisations, StmALog eventBO)
		{
			return PublishUniversalEvent(factory, recipientRoleTypes.ToRecipientRoleDetails(), workflowAndDataProvider, recipientOrganisations, eventBO);
		}

		/// <summary>
		/// Called when module specific interfaces need to push Universal Event from a module directly. Can also push internally and/or externally.
		/// </summary>
		public static PublishUniversalXmlResult PublishUniversalEvent(BusinessObjectFactory factory, RecipientRoleDetail[] recipientRoleDetails, IWorkflowProvider workflowAndDataProvider, OrgHeader[] recipientOrganisations, StmALog eventBO)
		{
			var dataProvider = workflowAndDataProvider as BusinessObject ?? throw new InvalidOperationException("The IWorkflowProvider passed in must be a BusinessObject.");

			var manager = dataProvider.GetUniversalDataContextManager() as IEventDataContextManager ?? throw new InvalidOperationException("Cannot call PublishUniversalEvent when the DataContextManager for the IWorkflowProvider (BusinessObject) specified is not capable of generating a Universal Event.");

			var workflowDescriptor = WorkflowDescriptors.Instance.TryGetValueSafe(workflowAndDataProvider.WorkflowType);

			var communicationModesProvider = new UniversalXmlCommunicationModeProvider(() =>
			{
				var communicationModes = new List<IEDICommunicationsMode>();
				foreach (var recipientOrganisation in recipientOrganisations)
				{
					if (recipientOrganisation != null)
					{
						foreach (var recipientRoleDetail in recipientRoleDetails)
						{
							var modeQuery = new EDICommunicationModeQuery(
							parent: dataProvider,
							descriptor: workflowDescriptor,
							fileFormat: WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML,
							purpose: ZString.Empty,
							recipientRole: recipientRoleDetail.Type.ToString(),
							eventCode: eventBO.SL_SE_NKEvent,
							eventReference: eventBO.SL_Reference);

							var modes = workflowDescriptor.GetCommunicationModesForRecipient(recipientOrganisation, modeQuery);
							if (modes.communicationModes != null && modes.communicationModes.Length > 0)
							{
								communicationModes.AddRange(modes.communicationModes);
							}
						}
					}
				}

				if (!communicationModes.Any(o => o.EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss))
				{
					var mode = new IEDICommunicationsMode[] { new NonPersistentEDICommunicationMode() { EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss } };
					communicationModes.AddRange(mode);
				}

				return (communicationModes.ToArray(), null);
			});

			var actionInfo = new ActionInfo(recipientRoleDetails, dataProvider, factory) { ActionType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML };
			Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter = (outboundSessionTracker) => manager.GetEventDataObjectWriter(outboundSessionTracker);
			var processor = new UniversalXmlWorkflowProcessor(actionInfo, communicationModesProvider, dataWriterGetter, eventBO);
			return processor.ProcessCore(new NotificationBuffer(), manager, CancellationToken.None, true);
		}

		public static PublishUniversalXmlResult PublishUniversalXMLInternally(BusinessObjectFactory parentFactory, BusinessObject sendingBO, ITopLevelDataObject topLevelDataObject, string messageSubType, IEDIMessage message = null, IXmlSessionTracker xmlSessionTracker = null, ICodeMappingManager mapper = null, IDataContextManager dataContextManager = null, bool shouldRetry = true, IDelayedTransactionManager transaction = null)
		{
			xmlSessionTracker = EnsureXmlSessionTracker(xmlSessionTracker);
			var sendingEntityID = dataContextManager ?? sendingBO?.GetUniversalDataContextManager();
			IDisposable supressSwitchContextCheck = null;
			IDisposable setUserContext = null;

			try
			{
				HookPreProcessForUnitTests();

				var factory = new BusinessObjectFactory
				{
					NameForDebugging = MasterFilesIntegrationConstants.PublishUniversalXmlInternallyFactoryName,
					RefreshEnabled = parentFactory.RefreshEnabled,
				};
				factory.SuspendValidation();
				factory.AddDisposableService(parentFactory.GetDisposableManager());

				message = message ?? CreateMessageAndLinkToDataExportEventOfSendingBO(parentFactory, sendingBO, topLevelDataObject, messageSubType, null);

				supressSwitchContextCheck = Env.Instance.SuppressSwitchContextCheck(ensureContextIsRestoredAfterSuppression: false);
				setUserContext = SetUserContext(factory, message, topLevelDataObject, xmlSessionTracker, sendingEntityID, out IUserContext userContext);

				if (xmlSessionTracker != null)
				{
					xmlSessionTracker.SessionUserContext = userContext;
				}
				var (resultMessage, events) = SendUniversalXmlInternally(factory, topLevelDataObject, messageSubType, null, sendingBO, sendingEntityID, shouldRetry, message, xmlSessionTracker, mapper, transaction);

				var shouldSave = resultMessage.ShouldSaveResultsFromUniversalXmlProcessing();

#if DEBUG
				shouldSave |= ShouldSaveResultMessageForTest.Value;
#endif
				// Subscribe to save happens here, because SendUniversalXmlInternally may throw an exception.
				if (shouldSave)
				{
					parentFactory.AddInTransactionAction(() =>
					{
						if (factory != null)
						{
							using (SetUserContext(factory, userContext))
							{
								factory.Save();
							}
						}
					});

					parentFactory.SubscribeForDispose(new DisposableAction(() =>
					{
						factory.DeactivateActiveCollectionsAndCaches();
						factory = null;
					}));
				}
				else
				{
					factory.DeactivateActiveCollectionsAndCaches();
				}

				return new PublishUniversalXmlResult(events?.ToArray() ?? Array.Empty<UniversalEvent>());
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var exceptionResult = GetUniversalResultOnException(ex, message, xmlSessionTracker, sendingEntityID, topLevelDataObject);
				exceptionResult.FailureException = ex;
				if (IsRetryException(ex) || ex is ZSaveException)
				{
					exceptionResult.ShouldRetry = true;
				}
				return exceptionResult;
			}
			finally
			{
				setUserContext?.Dispose();
				supressSwitchContextCheck?.Dispose();
			}
		}

		/// <summary>
		/// Called when business objects need to push Universal Event directly.
		/// </summary>
		public static PublishUniversalXmlResult PublishUniversalEvent(BusinessObjectFactory factory, DataContextType contextType, ZString key, BusinessObject dataProvider, StmALog eventBO)
		{
			dataProvider = dataProvider ?? throw new InvalidOperationException("The IWorkflowProvider passed in must be a BusinessObject.");

			var manager = dataProvider.GetUniversalDataContextManager() as IEventDataContextManager ?? throw new InvalidOperationException("Cannot call PublishUniversalEvent when the DataContextManager for the IWorkflowProvider (BusinessObject) specified is not capable of generating a Universal Event.");

			var communicationModesProvider = new UniversalXmlCommunicationModeProvider(() =>
			{
				return (new IEDICommunicationsMode[] { new NonPersistentEDICommunicationMode() { EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss } }, null);
			});

			var recipientRoleDetails = Array.Empty<RecipientRoleDetail>();
			var actionInfo = new ActionInfo(recipientRoleDetails, dataProvider, factory) { ActionType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML };
			Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter = (outboundSessionTracker) => new InternalEventDataObjectWriter(manager.GetEventDataObjectWriter(outboundSessionTracker), contextType, key, outboundSessionTracker);

			var processor = new UniversalXmlWorkflowProcessor(actionInfo, communicationModesProvider, dataWriterGetter, eventBO);
			var result = processor.ProcessCore(new NotificationBuffer(), manager, CancellationToken.None, true);
			return result;
		}

		public class InternalEventDataObjectWriter : EventDataObjectWriter
		{
			public InternalEventDataObjectWriter(ITopLevelDataObjectWriter writer, DataContextType contextType, ZString key, IDataWritingManager writeManager)
				: base(writeManager)
			{
				this.contextType = contextType;
				this.key = key;
				this.writer = writer;
			}

			readonly DataContextType contextType;
			readonly ZString key;
			readonly ITopLevelDataObjectWriter writer;

			protected override void InsertParents(BaseStmALog sourceBO, ref UniversalEvent dataObject)
			{
				base.InsertParents(sourceBO, ref dataObject);

				var parentDataObject = writer.GetDataObject(sourceBO);
				parentDataObject.DataContext.AddDataTarget(contextType, key);
				dataObject = (UniversalEvent)parentDataObject;
			}
		}

		/// <summary>
		/// Called when module specific interfaces need to trigger pushing Universal XML from a module directly. Can also push internally and/or externally.
		/// </summary>
		/// <param name="recipientOrganisation">The Organisation you want to send the Universal Shipment to.</param>
		/// <param name="recipientRoleTypes">Recipient Roles to be sent with the Universal Shipment to tell the recipient what role they are expected to play in the life of the Universal Shipment.</param>
		/// <param name="workflowAndDataProvider">The Top Level BusinessObject that the Universal Shipment will be generated from. Must implement IWorkflowProvider and be a BusinessObject.</param>
		/// <returns></returns>
		public static PublishUniversalXmlResult PublishUniversalShipment(BusinessObjectFactory factory, OrgHeader recipientOrganisation, RecipientRoleType[] recipientRoleTypes, IWorkflowProviderCore workflowAndDataProvider, Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter = null)
		{
			return PublishUniversalShipment(factory, recipientOrganisation, recipientRoleTypes.ToRecipientRoleDetails(), workflowAndDataProvider, dataWriterGetter);
		}

		/// <summary>
		/// Called when module specific interfaces need to trigger pushing Universal XML from a module directly. Can also push internally and/or externally.
		/// </summary>
		/// <param name="recipientOrganisation">The Organisation you want to send the Universal Shipment to.</param>
		/// <param name="recipientRoleDetails">Recipient Roles to be sent with the Universal Shipment to tell the recipient what role they are expected to play in the life of the Universal Shipment.</param>
		/// <param name="workflowAndDataProvider">The Top Level BusinessObject that the Universal Shipment will be generated from. Must implement IWorkflowProvider and be a BusinessObject.</param>
		/// <returns></returns>
		public static PublishUniversalXmlResult PublishUniversalShipment(BusinessObjectFactory factory, OrgHeader recipientOrganisation, RecipientRoleDetail[] recipientRoleDetails, IWorkflowProviderCore workflowAndDataProvider, Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter = null)
		{
			var dataProvider = workflowAndDataProvider as BusinessObject ?? throw new InvalidOperationException("The IWorkflowProvider passed in must be a BusinessObject.");

			var workflowDescriptor = WorkflowDescriptors.Instance.TryGetValueSafe(workflowAndDataProvider.WorkflowType);
			var actionInfo = new ActionInfo(recipientRoleDetails, dataProvider, factory) { ActionType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML };
			var manager = dataProvider.GetUniversalDataContextManager() as IShipmentDataContextManager ?? throw new InvalidOperationException("Cannot call PublishUniversalShipment when the DataContextManager for the IWorkflowProvider (BusinessObject) specified is not capable of generating a Universal Shipment.");

			var communicationModesProvider = new UniversalXmlCommunicationModeProvider(() =>
			{
				if (!recipientRoleDetails.Any())
				{
					return (Array.Empty<IEDICommunicationsMode>(), ResString.GetMultilingualString("0a57732b-49d7-44ff-878d-6a0a287131cc", "No recipient roles provided."));
				}
				else
				{
					var communicationModes = new List<IEDICommunicationsMode>();
					foreach (var recipientRoleDetail in recipientRoleDetails)
					{
						var modeQuery = new EDICommunicationModeQuery(
						parent: dataProvider,
						descriptor: workflowDescriptor,
						fileFormat: WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML,
						purpose: ZString.Empty,
						recipientRole: recipientRoleDetail.Type.ToString(),
						eventCode: ZString.Empty,
						eventReference: ZString.Empty);

						var modes = workflowDescriptor.GetCommunicationModesForRecipient(recipientOrganisation, modeQuery);
						communicationModes.AddRange(modes.communicationModes);
					}

					if (communicationModes.Any())
					{
						return (communicationModes.ToArray(), null);
					}
					else
					{
						var failureMessage = ResString.GetMultilingualString("5ca56600-3049-491d-b510-0bd4f004bfb9", @"No EDI Communications settings were found on the Recipient Organization [{0}]. Please add an entry on the [Details > Config > EDI Communications] tab of this Organization before sending Universal Data.", recipientOrganisation.OH_Code);
						return (Array.Empty<IEDICommunicationsMode>(), failureMessage);
					}
				}
			});

			if (dataWriterGetter == null)
			{
				dataWriterGetter = (outboundSessionTracker) => manager.GetShipmentDataObjectWriter(outboundSessionTracker);
			}

			var processor = new UniversalXmlWorkflowProcessor(actionInfo, communicationModesProvider, dataWriterGetter, dataProvider);

			var result = processor.ProcessCore(new NotificationBuffer(), manager, CancellationToken.None, true);

			return result;
		}

#if DEBUG
		internal static Overridable<Exception> ThrowExceptionOnProcessAsInboundDataObject { get; } = new Overridable<Exception>();
#endif

		PublishUniversalXmlResult ProcessCore(INotifications notifications, IEntityID dataSource, CancellationToken token, bool shouldRetry)
		{
			if (exportedBO == null)
			{
				notifications.Add(NotificationType.Information, ResString.GetMultilingualString("30c3fb31-1057-470f-a2f6-36fb42bb78dd", "Could not find entity to be exported."));
			}
			else if (CommunicationModes.Count > 0)
			{
				var outboundSessionTracker = new DataWritingManager(actionInfo, schema: schema);
				try
				{
					HookPreProcessForUnitTests();

					using (Env.Instance.SuppressSwitchContextCheck(ensureContextIsRestoredAfterSuppression: false))
					{
						var exportedData = GetExportedData(outboundSessionTracker);
						if (CommunicationModes.Any(o => o.EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss))
						{
							using (outboundSessionTracker.SetIsPublishingInternally())
							{
								return SendUniversalXmlInternally(actionInfo.FactoryForProcessing, exportedData, outboundSessionTracker, token, shouldRetry);
							}
						}
						var externalCommunicationModes = CommunicationModes.Where(o => o.EK_CommunicationsTransport != EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss).ToArray();
						if (externalCommunicationModes.Length > 0)
						{
							return SendUniversalXmlExternally(notifications, externalCommunicationModes, exportedData, outboundSessionTracker, dataSource);
						}
					}
				}
				catch (UniversalEventDeliveryFailureException ex)
				{
					notifications.AddError(ex.Message);
					return new PublishUniversalXmlResult(ex.Events.ToList());
				}
				catch (Exception ex) when (ex is DataObjectValidationException or MessageProcessingBusinessFailureException)
				{
					notifications.AddError(ex.Message);
					if (dataSource != null)
					{
						var resultEvent = GetExportFailureResponse(ex.Message, dataSource);
						return new PublishUniversalXmlResult([resultEvent]);
					}
				}
			}
			else
			{
				var error = communicationModesGetter.ConfigurationLogging;
				notifications.AddWarning(error);
				var resultEvent = GetExportFailureResponse(error, dataSource);

				return new PublishUniversalXmlResult(new[] { resultEvent });
			}

			return new PublishUniversalXmlResult(Array.Empty<UniversalEvent>());
		}

		DataObjectWrapper[] GetExportedData(IDataWritingManager outboundSessionTracker)
		{
			var result = new List<DataObjectWrapper>();
			result.Add(new DataObjectWrapper(actionInfo.ParentBO, () => base.GetDataObjectToExport(actionInfo.ParentBO, exportedBO, outboundSessionTracker)));

			foreach (var (anotherTopLevelDataObjectWriter, anotherXmlWriter) in anotherTopLevelDataObjectWritersAndXmlWriters)
			{
				// Need local variable because delegate would use the variable itself, not the current value
				var localAnotherTopLevelDataObjectWriter = anotherTopLevelDataObjectWriter;
				var localAnotherXmlWriter = anotherXmlWriter;

				result.Add(new DataObjectWrapper(actionInfo.ParentBO, () => base.GetDataObjectToExport(actionInfo.ParentBO, exportedBO, outboundSessionTracker, localAnotherTopLevelDataObjectWriter), () => localAnotherXmlWriter));
			}

			foreach (BusinessObject anotherExportedBO in anotherExportedBOs)
			{
				// Need local variable because delegate would use the variable itself, not the current value
				BusinessObject localAnotherExportedBO = anotherExportedBO;
				Func<ITopLevelDataObject> exportedDataObjectGetter = () => base.GetDataObjectToExport(localAnotherExportedBO, localAnotherExportedBO, outboundSessionTracker);

				result.Add(new DataObjectWrapper(anotherExportedBO, exportedDataObjectGetter));
			}

			return result.ToArray();
		}

		class DataObjectWrapper
		{
			public DataObjectWrapper(BusinessObject parentBO, Func<ITopLevelDataObject> dataObjectGetter, Func<IXmlWriter> writerGetter = null)
			{
				ParentBO = parentBO;
				this.dataObjectGetter = dataObjectGetter;
				this.writerGetter = writerGetter;
			}

			public BusinessObject ParentBO { get; private set; }

			public ITopLevelDataObject GetDataObject()
			{
				return dataObjectGetter != null ? dataObjectGetter() : null;
			}
			readonly Func<ITopLevelDataObject> dataObjectGetter;

			public IXmlWriter GetWriter()
			{
				return writerGetter != null ? writerGetter() : null;
			}
			readonly Func<IXmlWriter> writerGetter;
		}

		#region SendUniversalXmlInternally

		static (IEDIMessage message, IEnumerable<UniversalEvent> events) SendUniversalXmlInternally(BusinessObjectFactory factory, ITopLevelDataObject exportedDataObject, ZString messageSubType, IDataWritingManager outboundSessionTracker, BusinessObject sendingBO, IEntityID sendingEntityID, bool shouldRetry, IEDIMessage message = null, IXmlSessionTracker xmlSessionTracker = null, ICodeMappingManager mapper = null, IDelayedTransactionManager transaction = null)
		{
			var deliveryEvents = new List<UniversalEvent>();
			try
			{
				message = message ?? CreateMessageAndLinkToDataExportEventOfSendingBO(factory, sendingBO, exportedDataObject, messageSubType, outboundSessionTracker);
				var result = ProcessAsInboundDataObject(factory, message, exportedDataObject, outboundSessionTracker, shouldRetry, mapper: mapper, xmlSessionTracker, transaction);
				if (result.ImportResults.Any())
				{
					deliveryEvents.AddRange(result.ImportResults.Select(o => UpdateEventProcessingStatusFromEdiMessage(o.ToUniversalEvent(exportedDataObject, GlbCompany.CurrentCompany), message)).ToArray());
				}
				else
				{
					var uniqueResultLogs = result.Logs.Where(z => z.Type.Equals(LogType.Error)).GroupBy(x => x.ToString()).Select(y => y.First());
					var uniqueResultLogsString = string.Join("\r\n", uniqueResultLogs.ToList());
					deliveryEvents.Add(CreateFailureEvent(exportedDataObject, sendingEntityID, message, uniqueResultLogsString));
					throw new UniversalEventDeliveryFailureException(deliveryEvents, "Delivery failed due to validation exception.");
				}
			}
			catch (DataObjectValidationException exception)
			{
				deliveryEvents.Add(CreateFailureEvent(exportedDataObject, sendingEntityID, message, exception.ToString()));
				throw new UniversalEventDeliveryFailureException(deliveryEvents, "Delivery failed due to validation exception.", exception);
			}
			catch (Exception exception) when (!exception.IsCriticalException())
			{
				if (message != null && message.EM_Status == EDIMessageStatusList.Codes.Pending)
				{
					message.EM_Status = EDIMessageStatusList.Codes.Failed;
				}

				throw;
			}

			return (message, deliveryEvents);
		}

		static PublishUniversalXmlResult GetUniversalResultOnException(Exception exception, IEDIMessage message, IXmlSessionTracker xmlSessionTracker, IDataContextManager sendingEntityID, ITopLevelDataObject topLevelDataObject)
		{
			xmlSessionTracker.Log(LogType.Error, exception.Message);
			AddMessageNote(message, xmlSessionTracker);

			if (exception is UniversalEventDeliveryFailureException failuerException)
			{
				return new PublishUniversalXmlResult(failuerException.Events.ToList());
			}
			else if (exception is DataObjectValidationException validationException)
			{
				var resultEvent = GetExportFailureResponse(validationException.Message, sendingEntityID);
				return new PublishUniversalXmlResult(new[] { resultEvent });
			}
			else if (exception is MessageProcessingBusinessFailureException
				|| exception is ZSaveException)
			{
				var resultEvent = GetImportRejectedResponse(exception.Message, sendingEntityID);
				message.EM_Status = EDIMessageStatusList.Codes.Rejected;
				return new PublishUniversalXmlResult(new[] { resultEvent });
			}
			else
			{
				// Some unknown exception occurred. We report this back to the client.
				ErrorReporter.ReportOnce("PublishUniversalXMLInternally Exception", exception);
				var resultEvent = CreateFailureEvent(topLevelDataObject, sendingEntityID, message, exception.ToString());
				return new PublishUniversalXmlResult(new[] { resultEvent });
			}
		}

		static UniversalEvent CreateFailureEvent(ITopLevelDataObject exportedDataObject, IEntityID sendingEntityID, IEDIMessage message, string failureReason)
		{
			return UpdateEventProcessingStatusFromEdiMessage(
				new FailedImportResult(failureReason, sendingEntityID).ToUniversalEvent(exportedDataObject, GlbCompany.CurrentCompany), message);
		}

		static UniversalEvent UpdateEventProcessingStatusFromEdiMessage(UniversalEvent universalEvent, IEDIMessage message)
		{
			var status = message?.EM_Status ?? EDIMessageStatusList.Codes.Failed;
			universalEvent.ContextCollection = universalEvent.ContextCollection ?? new List<Context>();
			universalEvent.ContextCollection.Add(new Context() { Type = nameof(UniversalEvent.ContextTypes.ProcessingStatusCode), Value = status });

			return universalEvent;
		}

		static IDisposable SetUserContext(BusinessObjectFactory factory, IEDIMessage message, ITopLevelDataObject topLevelDataObject, IXmlSessionTracker xmlSessionTracker, IEntityID sendingEntityID, out IUserContext userContext)
		{
			if (message.TryGetUserContext(topLevelDataObject, xmlSessionTracker, out userContext))
			{
				return SetUserContext(factory, userContext);
			}
			else
			{
				throw new UniversalEventDeliveryFailureException(new[] { CreateFailureEvent(topLevelDataObject, sendingEntityID, message, xmlSessionTracker.ToString()) }, "Delivery failed due to validation exception.");
			}
		}

		static IDisposable SetUserContext(BusinessObjectFactory factory, IUserContext userContext)
		{
			var contextSwitchLogger = eAdaptorRegistry.Instance.MessageUserContextTracingEnabled.Value ? new UserContextSwitchLogger() : null;
			var contextSwitchTrace = contextSwitchLogger != null ? Env.StartContextSwitchTrace(contextSwitchLogger) : null;
			var context = UniversalMessageProcessingExtensions.SetUserContext(userContext);
			var workflowContext = factory.ServiceContainer.AddService(new WorkflowUserContextManager()).SetWorkflowUserContext(userContext, contextSwitchLogger);

			return new DisposableAction(() =>
			{
				contextSwitchTrace?.Dispose();
				workflowContext.Dispose();
				context.Dispose();
			});
		}

		class FailedImportResult : IImportResult, IEntityIDWithNullableContext
		{
			public FailedImportResult(string failureReason, IEntityID entityID)
			{
				FailureReason = Argument.NotNull(failureReason, "failureReason");
				EntityID = entityID;
			}

			readonly IEntityID EntityID;
			readonly string FailureReason;

			public override string ToString()
			{
				return FailureReason;
			}

			IEnumerable<IEntityID> IImportResult.LinkedJobs
			{
				get { return Enumerable.Empty<IEntityID>(); }
			}

			bool IImportResult.WasSuccessful
			{
				get { return false; }
			}

			string IEntityID.DataContextKey
			{
				get { return EntityID?.DataContextKey; }
			}

			DataContextType IEntityID.DataContextType
			{
				get { return EntityID.DataContextType; }
			}

			IEnumerable<ISimpleLog> ISimpleLogResult.Logs
			{
				get { return new[] { new SimpleLog(LogType.Information, FailureReason) }; }
			}

			DataContextType? IEntityIDWithNullableContext.NullableDataContextType => EntityID?.DataContextType;
		}

		PublishUniversalXmlResult SendUniversalXmlInternally(BusinessObjectFactory factory, DataObjectWrapper[] exportedData, IDataWritingManager outboundSessionTracker, CancellationToken token, bool shouldRetry)
		{
			var deliveryEvents = new List<UniversalEvent>();

			foreach (DataObjectWrapper dataObjectWrapper in exportedData)
			{
				token.ThrowIfCancellationRequested();
				var sendingBO = dataObjectWrapper.ParentBO;
				var exportedDataObject = dataObjectWrapper.GetDataObject();

				if (sendingBO == null)
				{
					throw new InvalidOperationException("Cannot Send Universal XML internally without being able to get the sending BO from the action.");
				}

				var sendingBODataContextManager = sendingBO.GetUniversalDataContextManager()
					?? throw new InvalidOperationException("Should not be able to send Universal XML from a module that does not have a DataContextManager linked to the Sending BO type. [" + sendingBO.GetType() + "]");

				deliveryEvents.AddRange(SendUniversalXmlInternally(factory, exportedDataObject, dataWriterGetter(outboundSessionTracker).EDIMessageSubType, outboundSessionTracker, sendingBO, sendingBODataContextManager, shouldRetry).events);
			}

			return new PublishUniversalXmlResult(deliveryEvents.ToArray());
		}

		static IXmlSessionTracker ProcessAsInboundDataObject(BusinessObjectFactory factory, IEDIMessage message, ITopLevelDataObject exportedDataObject, IDataWritingManager outboundSessionTracker, bool shouldRetry, ICodeMappingManager mapper = null, IXmlSessionTracker xmlSessionTracker = null, IDelayedTransactionManager transaction = null)
		{
			var concreteSessionTracker = EnsureXmlSessionTracker(xmlSessionTracker) as XmlSessionTracker;
			var universalFactory = new UniversalObjectFactory(factory);
			var manager = xmlSessionTracker != null ? new UniversalMessageProcessingManager(universalFactory, concreteSessionTracker) { OutboundSessionTracker = outboundSessionTracker } : new UniversalMessageProcessingManager(concreteSessionTracker) { OutboundSessionTracker = outboundSessionTracker };
			var currentAttempt = 1;
			const int maxAttempts = 3;
			if (shouldRetry)
			{
				do
				{
					try
					{
						ProcessAsInboundDataObject(manager, message, exportedDataObject, mapper, transaction);
					}
					catch (Exception e) when (IsRetryException(e))
					{
						manager.Logger.Log(LogType.Information, e.Message);

						if (currentAttempt < maxAttempts)
						{
							manager.Logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Failed import, will retry {0} more times...", maxAttempts - currentAttempt));
						}
					}
				} while (manager.Logger.ImportResults.IsNullOrEmpty() && currentAttempt++ < maxAttempts);

				if (manager.Logger.ImportResults.IsNullOrEmpty() && currentAttempt > maxAttempts)
				{
					manager.Logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Failed import after {0} attempts.", maxAttempts));
					message.EM_Status = EDIMessageStatusList.Codes.Error;
				}
			}
			else
			{
				ProcessAsInboundDataObject(manager, message, exportedDataObject, mapper, transaction);
			}

			return manager.Logger;
		}

		public static bool IsRetryException(Exception e)
		{
			if ((e is MessageProcessingBusinessFailureException processingException && processingException.ShouldRetry) || e is ZSaveConcurrencyException)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		static IXmlSessionTracker ProcessAsInboundDataObject(IMessageProcessingManager processingManager, IEDIMessage message, ITopLevelDataObject exportedDataObject, ICodeMappingManager mapper = null, IDelayedTransactionManager transaction = null)
		{
#if DEBUG
			if (ThrowExceptionOnProcessAsInboundDataObject.Value != null)
			{
				throw ThrowExceptionOnProcessAsInboundDataObject.Value;
			}
#endif
			return processingManager.Process(message, exportedDataObject, false, mapper, transaction);
		}

		internal static void TryUseNewFactoryToFailMessage(IEDIMessage message, IXmlSessionTracker xmlSessionTracker, Exception exception)
		{
			TryUseNewFactoryToChangeMessageStatus(message, xmlSessionTracker, EDIMessageStatusList.Codes.Failed, exception);
		}

		internal static void TryUseNewFactoryToChangeMessageStatus(IEDIMessage message, IXmlSessionTracker xmlSessionTracker, ZString status, Exception exception = null)
		{
			var failedToUpdateMessage = FormattableString.Invariant($"Failed to update message [{message.EM_MessageNum}] status to '{status}'");
			try
			{
				var newFactory = new BusinessObjectFactory();
				newFactory.NameForDebugging = "Update Request Message Status";

				using (newFactory.AddDisposableService())
				{
					var messageFromNewFactory = newFactory.Load<IEDIMessage>(message.PK)
						?? throw new UpdateEDIMessageStatusException(failedToUpdateMessage + " because it was deleted.");

					message.EM_Status = status;
					messageFromNewFactory.EM_Status = status;
					messageFromNewFactory.EM_RetryCount++;

					if (exception != null)
					{
						xmlSessionTracker.Log(LogType.Error, exception.Message);
						AddMessageNote(messageFromNewFactory, xmlSessionTracker);
					}

					ZExceptionReporting.ProcessWithSaveExceptionHandling(newFactory.Save, null, false);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException() && ex.Find<UpdateEDIMessageStatusException>() == null)
			{
				if (exception != null)
				{
					failedToUpdateMessage += (NoResString)" or to add data import log with exception message to the failed message.";
				}

				var exceptionToReport = exception == null ? ex : new AggregateException(exception, ex);
				ErrorReporter.ReportOnce("a071bdf7-5a9c-4800-b0b9-2fe46e37527a", failedToUpdateMessage, exceptionToReport);
			}
		}

		static IEDIMessage CreateMessageAndLinkToDataExportEventOfSendingBO(BusinessObjectFactory factory, BusinessObject parentBO, ITopLevelDataObject exportedData, ZString messageSubType, IDataWritingManager dataWritingManager)
		{
			var stream = factory.SubscribeForDispose(new CargoWise.IO.Shim.SubStreamableStream());
			new XmlWriter().WriteXML(exportedData, stream, dataWritingManager?.Schema?.Namespace);

			var message = factory.New<IXmlEDIMessage>();

			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Internal;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = messageSubType;

			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_GE = GlbDepartment.CurrentDepartment.PK;
			message.EM_Status = EDIMessageStatusList.Codes.Pending;
			message.SetEM_MessageTextOrDataSource(stream);

			var parentInfo = EntityInfo.New(parentBO);
			var parentBOWithLogs = factory.Load(parentInfo.Type, parentInfo.InternalPK) as IStmALogParent;
			new MessageDataExportImportLogLinker(Events.DataExport, factory).LinkMessageToParentBOLogs(message, parentBOWithLogs);

			return message;
		}

		#region DummyLogger

		static IXmlSessionTracker EnsureXmlSessionTracker(IXmlSessionTracker xmlSessionTracker)
		{
			return xmlSessionTracker ?? new XmlSessionTracker(new DummyLogger());
		}

		static void AddMessageNote(IEDIMessage message, IXmlSessionTracker tracker)
		{
			if (message != null && tracker is XmlSessionTracker sessionTracker)
			{
				ObjectFactory.New<IEDIMessageDataImportNoteCreator>().AddNew(message, noteStream => sessionTracker.WriteTo(noteStream));
			}
		}

		class DummyLogger : ISimpleLogger
		{
			public IEnumerable<ISimpleLog> Logs => Array.Empty<ISimpleLog>();

			public void Log(LogType type, string message)
			{
			}
		}

		#endregion

		#endregion

		PublishUniversalXmlResult SendUniversalXmlExternally(INotifications notifications, ICollection<IEDICommunicationsMode> externalCommunicationModes, ICollection<DataObjectWrapper> exportedData, IDataWritingManager outboundSessionTracker, IEntityID dataSource)
		{
			var deliveryEvents = new List<UniversalEvent>();
			var context = CreateContext(notifications, outboundSessionTracker);
			var jobNumbers = exportedData.Select(wrapper => JobNumberResolver.GetJobNumber(wrapper.ParentBO));
			var jobNumber = string.Join(", ", jobNumbers.Where(number => !string.IsNullOrEmpty(number)));
			var delivery = new EDIMessageDelivery(jobNumber);

			var outboundAdapterServiceUrlHasValue = !eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.Value.IsNullOrEmpty();
			foreach (var mode in externalCommunicationModes)
			{
				if (mode.EK_CommunicationsTransport != EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface || outboundAdapterServiceUrlHasValue)
				{
					using (outboundSessionTracker.UseNewListForDuplicatePKCheck())
					{
						var streamWrappers = new List<DeliveryStreamWrapperUXML>();
						var exportedDataObjects = new List<ITopLevelDataObject>();

						IDeliveryResult deliveryResult = null;
						try
						{
							foreach (var dataObjectWrapper in exportedData)
							{
								var sendingBO = dataObjectWrapper.ParentBO;
								outboundSessionTracker.ShouldPopulateInternalMilestones = mode.EK_PublishInternalMilestones;
								var exportedDataObject = dataObjectWrapper.GetDataObject();
								var writer = dataObjectWrapper.GetWriter();

								actionInfo.FactoryForProcessing.SubscribeForDispose(exportedDataObject);
								RemoveNonExportedCollections(actionInfo, exportedDataObject);
								exportedDataObjects.Add(exportedDataObject);
								if (writer == null)
								{
									writer = xmlWriter;
								}
								if (writer is ISupportsRemovingEmptyElements supportWriter)
								{
									supportWriter.RemoveEmptyElements = ObjectFactory.Get<IUniversalXmlContentFilterApplicator>().ShouldExcludeEmptyElements(outboundSessionTracker.Action);
								}
								streamWrappers.Add(new DeliveryStreamWrapperUXML(EntityInfo.New(sendingBO), exportedDataObject, writer, outboundSessionTracker.Schema?.Namespace));
							}

							deliveryResult = delivery.DeliverBatch(context, mode, streamWrappers.ToArray());
						}
						catch (Exception ex) when (ex is DataObjectValidationException)
						{
							NotifyExportValidationFailure(context, mode);
							throw;
						}

						if (deliveryResult.Succeeded)
						{
							NotifyExported(context, mode);
						}
						deliveryEvents.AddRange(exportedDataObjects.Select(obj => CreateDeliveryEvent(mode, deliveryResult, obj)));
					}
				}
				else
				{
					notifications.AddError(Res.GetString("UniversalXmlWorkflowProcessor.eHubMessagingRegistry.Instance.OutboundAdapterServiceUrl.Value.IsNullOrEmpty", "Comm.Transport: eAdaptor has not been setup in the registry.\r\nPlease configure eAdaptor or change to a different Transport type."));
					var resultEvent = GetExportFailureResponse(Res.GetString("UniversalXmlWorkflowProcessor.eHubMessagingRegistry.Instance.OutboundAdapterServiceUrl.Value.IsNullOrEmpty", "Comm.Transport: eAdaptor has not been setup in the registry.\r\nPlease configure eAdaptor or change to a different Transport type."), dataSource);
					return new PublishUniversalXmlResult(new[] { resultEvent });
				}
			}

			return new PublishUniversalXmlResult(deliveryEvents);
		}

		void RemoveNonExportedCollections(IUniversalActionInfo actionInfo, ITopLevelDataObject exportedDataObject)
		{
			if (!IsUniversalXmlContentFilterApplicatorSuspended)
			{
				ObjectFactory.Get<IUniversalXmlContentFilterApplicator>().RemoveNonExportedCollections(exportedDataObject, actionInfo);
			}
		}

		protected virtual DeliveryContext CreateContext(INotifications notifications, IDataWritingManager outboundSessionTracker)
		{
			return new DeliveryContext(actionInfo.FactoryForProcessing)
			{
				ParentInfo = EntityInfo.New(actionInfo.ParentBO),
				PurposeCode = actionInfo.PurposeCode,
				ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging,
				MessageTypeCode = EDIMessageTypeList.Codes.XDC,
				MessageSubTypeCode = dataWriterGetter(outboundSessionTracker).EDIMessageSubType,
				Notifications = notifications
			};
		}

		static UniversalEvent CreateDeliveryEvent(IEDICommunicationsMode deliveryMode, IDeliveryResult deliveryResult, ITopLevelDataObject exportedData)
		{
			var deliveryEvent = new UniversalEvent();
			deliveryEvent.ContextCollection = new List<Context>();

			var recipientOrganisation = deliveryMode.Organisation;
			if (recipientOrganisation != null)
			{
				deliveryEvent.ContextCollection.Add(new Context() { Type = "RecipientOrganizationCode", Value = recipientOrganisation.OH_Code });
				deliveryEvent.ContextCollection.Add(new Context() { Type = "RecipientOrganizationName", Value = recipientOrganisation.OH_FullName });
			}
			deliveryEvent.ContextCollection.Add(new Context() { Type = "CommunicationsTransportMode", Value = deliveryMode.EK_CommunicationsTransport });
			deliveryEvent.ContextCollection.Add(new Context() { Type = "CommunicationsDestinationCode", Value = deliveryMode.EK_Destination });
			deliveryEvent.DataContext = DataContextFactory.New();
			deliveryEvent.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			deliveryEvent.AddDataTargetToUniversalEvent(exportedData);

			if (deliveryResult.Succeeded)
			{
				deliveryEvent.EventType = Events.DataExportCode;
			}
			else
			{
				deliveryEvent.EventType = Events.DataExportFailureCode;
				deliveryEvent.ContextCollection.Add(new Context() { Type = nameof(UniversalEvent.ContextTypes.FailureReason), Value = (NoResString)"Error during EDI Message creation" });
			}

			return deliveryEvent;
		}

		static BusinessObject GetBusinessObjectForDeliveryContext(DeliveryContext deliveryContext)
		{
			var businessObject = deliveryContext.Factory.Load(deliveryContext.ParentInfo.Type, deliveryContext.ParentInfo.InternalPK);
			return businessObject is IStmALogParent ? businessObject : null;
		}

		static IUniversalExportHook GetUniversalExportHook(BusinessObject businessObject)
		{
			if (businessObject != null)
			{
				var providers = ObjectFactory.Get<Hashtable>("UniversalDeliveryHooks");
				if (providers[businessObject.TablePrefix] is ObjectHandle handle
					&& handle.GetObject() is IUniversalExportHook universalExportHook)
				{
					return universalExportHook;
				}
			}

			return null;
		}

		static void NotifyExported(DeliveryContext deliveryContext, IEDICommunicationsMode communicationsMode)
		{
			var businessObject = GetBusinessObjectForDeliveryContext(deliveryContext);
			var universalExportHook = GetUniversalExportHook(businessObject);
			if (universalExportHook != null)
			{
				universalExportHook.OnUniversalXmlExport(businessObject, communicationsMode);
			}
		}

		static void NotifyExportValidationFailure(DeliveryContext deliveryContext, IEDICommunicationsMode communicationsMode)
		{
			var businessObject = GetBusinessObjectForDeliveryContext(deliveryContext);
			var universalExportHook = GetUniversalExportHook(businessObject);
			if (universalExportHook != null)
			{
				universalExportHook.OnUniversalXmlExportValidationFailure(businessObject, communicationsMode);
			}
		}

		static UniversalEvent GetExportFailureResponse(string failureMessage, IEntityID dataTarget)
		{
			return GetFailureResponse(failureMessage, dataTarget, Events.DataExportFailureCode);
		}

		static UniversalEvent GetImportRejectedResponse(string failureMessage, IEntityID dataTarget)
		{
			return GetFailureResponse(failureMessage, dataTarget, Events.DataImportFailureCode);
		}

		static UniversalEvent GetFailureResponse(string failureMessage, IEntityID dataTarget, string failureCode)
		{
			var resultEvent = new UniversalEvent();
			resultEvent.EventType = failureCode;
			resultEvent.DataContext = DataContextFactory.New();
			if (dataTarget != null)
			{
				resultEvent.DataContext.AddDataTarget(dataTarget.DataContextType, dataTarget.DataContextKey);
			}
			resultEvent.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			resultEvent.ContextCollection = new List<Context>();
			resultEvent.ContextCollection.Add(new Context()
			{
				Type = nameof(UniversalEvent.ContextTypes.FailureReason),
				Value = failureMessage
			});

			return resultEvent;
		}

		static partial void HookPreProcessForUnitTests();

		public IDisposable SuspendUniversalXmlContentFilterApplicator()
		{
			universalXmlContentFilterApplicatorSuspensionLevel++;
			return new DisposableAction(() => universalXmlContentFilterApplicatorSuspensionLevel--);
		}

		int universalXmlContentFilterApplicatorSuspensionLevel;

		public bool IsUniversalXmlContentFilterApplicatorSuspended =>
			universalXmlContentFilterApplicatorSuspensionLevel > 0;
	}

#if DEBUG
	public partial class UniversalXmlWorkflowProcessor : UniversalXmlWriter, IUniversalXmlWorkflowProcessor, ISupportUniversalBatchExport, IMessageProcessor
	{
		static readonly Overridable<Action> OnPreProcess = new Overridable<Action>(null);

		public static void SetPreProcessActionHook(Action onPreProcess)
		{
			OnPreProcess.Value = onPreProcess;
		}

		static partial void HookPreProcessForUnitTests()
		{
			OnPreProcess.Value?.Invoke();
		}
	}
#endif
}

