using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.UniversalData
{
	public delegate IUniversalXmlWorkflowProcessor UniversalXmlWorkflowProcessorFactory(IMessageProcessorCommunicationModesResult communicationsModes, BusinessObject[] exportedObjects);

	public sealed class ManualDataExport : NonPersistentBusinessObject, IObsoleteValidation, IDisposable
	{
		public ManualDataExport
			(
				BusinessObjectFactory factory,
				IEnumerable<IWorkflowProvider> parents,
				UniversalDataType dataType,
				IEnumerable<IEDICommunicationsMode> communicationModes = null,
				IUniversalXmlSchema schema = null,
				UniversalXmlWorkflowProcessorFactory universalXmlWorkflowProcessorFactory = null,
				Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter = null
			)
			: base(factory)
		{
			Argument.NotNull(parents, "IWorkflowProvider parents");

			if (!parents.Any())
			{
				throw new ArgumentException("There must be at least one IWorkflowProvider parent");
			}

			this.parents = parents.Cast<BusinessObject>().ToArray();
			this.dataType = dataType;
			this.xmlSchema = schema;
			this.communicationModes = communicationModes;

			var workflowType = parents.First().WorkflowType;

			if (parents.Any(parent => parent.WorkflowType != workflowType))
			{
				throw new ArgumentException("all parents have to have the same workflow type");
			}

			WorkflowDescriptor = Argument.NotNull(WorkflowDescriptors.Instance.TryGetValueSafe(workflowType), "workflowType");
			this.universalXmlWorkflowProcessorFactory = universalXmlWorkflowProcessorFactory ?? GetProcessor;
			this.dataWriterGetter = dataWriterGetter;
		}

		public ManualDataExport
			(
				BusinessObjectFactory factory,
				IWorkflowProvider parent,
				UniversalDataType dataType,
				IEnumerable<IEDICommunicationsMode> communicationModes = null,
				IUniversalXmlSchema schema = null,
				UniversalXmlWorkflowProcessorFactory universalXmlWorkflowProcessorFactory = null,
				Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter = null
			)
			: this
			(
				factory,
				new[] { parent },
				dataType,
				communicationModes,
				schema,
				universalXmlWorkflowProcessorFactory,
				dataWriterGetter
			)
		{
		}

		readonly IEnumerable<BusinessObject> parents;
		readonly IEnumerable<IEDICommunicationsMode> communicationModes;
		internal WorkflowDescriptor WorkflowDescriptor { get; private set; }
		readonly UniversalDataType dataType;
		readonly IUniversalXmlSchema xmlSchema;
		readonly UniversalXmlWorkflowProcessorFactory universalXmlWorkflowProcessorFactory;
		readonly Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter;

		#region Schema

		public static class Schema
		{
			public const string Calc_RecipientType = "Calc_RecipientType";
			public const string RecipientType = "RecipientType";
			public const string RecipientService = "RecipientService";
			public const string RecipientPK = "RecipientPK";
			public const string EventCode = "EventCode";
			public const string EventReference = "EventReference";
			public const string PurposeCode = "PurposeCode";
			public const string TriggerDescription = "TriggerDescription";
		}

		#endregion

		#region RecipientPK

		[List("Lookups.Recipients")]
		[ReadOnlyMember(nameof(RecipientPK_ReadOnly))]
		public ZGuid RecipientPK
		{
			get { return recipientPK; }
			set
			{
				SetNonPersistentPropertyValue(RecipientPKInfo, ref recipientPK, value);
				if (!IsValidationSuspended)
				{
					ValidateRecipientPK();
				}
			}
		}
		ZGuid recipientPK;

		public void ValidateRecipientPK()
		{
			RecipientPKInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(RecipientPKInfo);
			if (IsOtherRecipient)
			{
				MandatoryValidation.CheckEntered(RecipientPKInfo);
			}
		}

		public bool RecipientPK_ReadOnly
		{
			get { return !IsOtherRecipient; }
		}

		public ZPropertyInfo RecipientPKInfo
		{
			get { return GetZPropertyInfo(Schema.RecipientPK); }
		}

		public OrgHeader RecipientOrganization
		{
			get { return Factory.Load<OrgHeader>(RecipientPK); }
		}

		#endregion

		#region Calc_RecipientType

		[List("Lookups.RecipientTypeList")]
		[MaxLength(3)]
		public ZString Calc_RecipientType
		{
			get
			{
				return IsOtherRecipient ? (ZString)MessageRecipientPartyTypeList.SpecialCodes.Other : RecipientType;
			}
			set
			{
				ZString oldValue = Calc_RecipientType;
				value = value.TrimEndSpaceTab();
				value = value.ConvertToWesternEuropeanCharacters();
				CheckMaximumLength(Calc_RecipientTypeInfo, value);
				isOtherRecipient = value == MessageRecipientPartyTypeList.SpecialCodes.Other;
				if (!IsOtherRecipient)
				{
					RecipientType = value;
					ClearRecipientPKIfNeeded();
				}
				Calc_RecipientTypeInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					ValidateCalc_RecipientType();
				}
			}
		}

		public ZPropertyInfo Calc_RecipientTypeInfo
		{
			get { return GetZPropertyInfo(Schema.Calc_RecipientType); }
		}

		void ClearRecipientPKIfNeeded()
		{
			if (RecipientPK.IsValid)
			{
				RecipientPK = ZGuid.Empty;
			}
		}

		public void ValidateCalc_RecipientType()
		{
			Calc_RecipientTypeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(Calc_RecipientTypeInfo);
			MandatoryValidation.CheckEntered(Calc_RecipientTypeInfo);
		}

		public bool IsOtherRecipient
		{
			get
			{
				if (!isOtherRecipient.HasValue)
				{
					isOtherRecipient = !RecipientPK.IsEmpty;
				}
				return isOtherRecipient.Value;
			}
		}
		bool? isOtherRecipient;

		#endregion

		#region RecipientType

		[List("Lookups.AlternateRecipientPartyTypeList")]
		[MaxLength(3)]
		[ReadOnlyMember(nameof(RecipientType_ReadOnly))]
		public ZString RecipientType
		{
			get { return recipientType; }
			set
			{
				SetNonPersistentPropertyValue(RecipientTypeInfo, ref recipientType, value);
				if (!IsValidationSuspended)
				{
					ValidateRecipientType();
				}
			}
		}

		ZString recipientType;
		public ZPropertyInfo RecipientTypeInfo { get { return GetZPropertyInfo(Schema.RecipientType); } }

		public bool RecipientType_ReadOnly
		{
			get { return !IsOtherRecipient; }
		}

		public void ValidateRecipientType()
		{
			RecipientTypeInfo.ClearAllNotifications();
			if (IsOtherRecipient)
			{
				ListValidation.ErrorIfInvalidCode(RecipientTypeInfo);
				MandatoryValidation.CheckEntered(RecipientTypeInfo);
			}
		}

		#endregion

		#region PQ_TriggerPartyService

		[List("Lookups.RecipientServices")]
		[MaxLength(3)]
		[ReadOnlyMember(nameof(RecipientService_ReadOnly))]
		public ZString RecipientService
		{
			get { return recipientService; }
			set
			{
				SetNonPersistentPropertyValue(RecipientServiceInfo, ref recipientService, value);
				if (!IsValidationSuspended)
				{
					ValidateRecipientService();
				}
			}
		}

		ZString recipientService;

		public ZPropertyInfo RecipientServiceInfo
		{
			get { return GetZPropertyInfo(Schema.RecipientService); }
		}

		public bool RecipientService_ReadOnly
		{
			get { return !IsRecipientServiceAvailable; }
		}

		public bool IsRecipientServiceAvailable
		{
			get { return WorkflowDescriptor?.IsRecipientServiceAvailable(ActionType) ?? false; }
		}

		public bool IsRecipientServiceMandatory
		{
			get { return WorkflowDescriptor?.IsRecipientServiceMandatory(ActionType, RecipientType) ?? false; }
		}

		public void ValidateRecipientService()
		{
			RecipientServiceInfo.ClearAllNotifications();

			if (IsRecipientServiceAvailable)
			{
				ListValidation.ErrorIfInvalidCode(RecipientServiceInfo);
				if (IsRecipientServiceMandatory)
				{
					MandatoryValidation.CheckEntered(RecipientServiceInfo);
				}
			}
			else
			{
				MandatoryValidation.CheckNotEntered(RecipientServiceInfo);
			}
		}

		#endregion

		#region EventCode

		[List("Lookups.EventCodeList")]
		[MaxLength(3)]
		public ZString EventCode
		{
			get { return eventCode; }
			set
			{
				SetNonPersistentPropertyValue(EventCodeInfo, ref eventCode, value);
				if (!IsValidationSuspended)
				{
					ValidateEventCode();
				}
			}
		}

		ZString eventCode;
		public ZPropertyInfo EventCodeInfo { get { return GetZPropertyInfo(Schema.EventCode); } }

		public void ValidateEventCode()
		{
			EventCodeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(EventCodeInfo);
			if (dataType == UniversalDataType.UniversalEvent)
			{
				MandatoryValidation.CheckEntered(EventCodeInfo);
			}
		}

		#endregion

		#region EventReference

		[MaxLength(128)]
		public ZString EventReference
		{
			get { return eventReference; }
			set
			{
				SetNonPersistentPropertyValue(EventReferenceInfo, ref eventReference, value);
			}
		}

		ZString eventReference;
		public ZPropertyInfo EventReferenceInfo { get { return GetZPropertyInfo(Schema.EventReference); } }

		#endregion

		#region PurposeCode

		[List("Lookups.PurposeCodeList")]
		[MaxLength(3)]
		public ZString PurposeCode
		{
			get { return purposeCode; }
			set
			{
				SetNonPersistentPropertyValue(PurposeCodeInfo, ref purposeCode, value);
				if (!IsValidationSuspended)
				{
					ValidatePurposeCode();
				}
			}
		}

		ZString purposeCode;
		public ZPropertyInfo PurposeCodeInfo { get { return GetZPropertyInfo(Schema.PurposeCode); } }

		public void ValidatePurposeCode()
		{
			PurposeCodeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(PurposeCodeInfo);
		}

		#endregion

		#region TriggerDescription

		[MaxLength(50)]
		public ZString TriggerDescription
		{
			get { return triggerDescription; }
			set
			{
				SetNonPersistentPropertyValue(TriggerDescriptionInfo, ref triggerDescription, value);
				if (!IsValidationSuspended)
				{
					ValidateTriggerDescription();
				}
			}
		}

		ZString triggerDescription;
		public ZPropertyInfo TriggerDescriptionInfo { get { return GetZPropertyInfo(Schema.TriggerDescription); } }

		public void ValidateTriggerDescription()
		{
			TriggerDescriptionInfo.ClearAllNotifications();
		}

		#endregion

		#region Lookups

		public ManualDataExportLookups Lookups
		{
			get { return new ManualDataExportLookups(this); }
		}

		#endregion

		public IEnumerable<IXmlEventValueObject> SendData(INotifications notifications)
		{
			Argument.NotNull(notifications, "notifications");

			var xmlEvents = new List<IXmlEventValueObject>();
			var recipients = GetRecipients(notifications);

			if (recipients.All(r => r.CommunicationModes.Destinations.Any()))
			{
				foreach (var recipientWrapper in recipients)
				{
					BusinessObject[] exportedObjects = recipientWrapper.ExportedObjects.ToArray();

					string exportedObjectsDescription = string.Join(", ", exportedObjects.Select(bizo => bizo.HumanReadableName));
					notifications.Add(new InfoNotification(Res.GetString("909a8436-e463-48b7-a219-e4b4f6a44e7c", "Processing {0}", exportedObjectsDescription)));

					xmlEvents.AddRange(SendData(recipientWrapper, notifications));

					notifications.Add(new NewlineNotification());
				}
			}
			else
			{
				foreach (var recipient in recipients)
				{
					var failureReason = recipient.CommunicationModes.ConfigurationLogging;
					if (!string.IsNullOrEmpty(failureReason))
					{
						notifications.AddWarning(failureReason);
					}
				}
			}

			return xmlEvents.ToArray();
		}

		public string SendDataDescription
		{
			get { return Res.GetString("048e819c-0dfa-4b4f-9a7a-6eb1ac514433", "Send XML {0}", DataTypeName); }
		}

		public void Dispose()
		{
			WorkflowDescriptor = null;
		}

		#region Implementation

		class Logger : INotifications
		{
			readonly List<string> notifications = new List<string>();

			void INotifications.Add(INotification notification)
			{
				notifications.Add(notification.Message);
				HasErrors = true;
			}

			public override string ToString()
			{
				return string.Join("\r\n", notifications.ToArray());
			}

			internal bool HasErrors { get; private set; }
		}

		public class RecipientWrapper
		{
			public RecipientWrapper(OrgHeader recipient, IMessageProcessorCommunicationModesResult communicationModes, params BusinessObject[] workflowProviders)
				: this(communicationModes, workflowProviders)
			{
				this.Recipient = recipient;
			}

			public RecipientWrapper(IMessageProcessorCommunicationModesResult communicationModes, params BusinessObject[] workflowProviders)
			{
				this.CommunicationModes = communicationModes;

				if (workflowProviders != null && workflowProviders.Any())
				{
					ExportedObjects.AddRange(workflowProviders);
				}
			}

			public readonly OrgHeader Recipient;
			public List<BusinessObject> ExportedObjects = new List<BusinessObject>();
			public IMessageProcessorCommunicationModesResult CommunicationModes;
		}

		public void OverrideRecipient(OrgHeader recipient)
		{
			overriddenRecipient = recipient;
		}

		OrgHeader overriddenRecipient;

		public IEnumerable<RecipientWrapper> GetRecipients(INotifications notifications)
		{
			var allRecipients = new List<RecipientWrapper>();
			eAdaptorErrorAlreadyAdded = false;
			if (communicationModes == null)
			{
				foreach (var parent in parents)
				{
					IEnumerable<OrgHeader> recipientOrganisations;
					if (IsOtherRecipient)
					{
						recipientOrganisations = RecipientPK.IsEmpty ? null : new[] { RecipientOrganization };
					}
					else
					{
						recipientOrganisations = overriddenRecipient != null
							? new[] { overriddenRecipient }
							: WorkflowDescriptor.GetMessageRecipientPartyWithFailureReason(parent, RecipientType).recipients.Select(recipient => recipient.Party);
					}
					if (recipientOrganisations != null && recipientOrganisations.Any())
					{
						foreach (var recipientOrganisation in recipientOrganisations)
						{
							var shouldGroupByRecipients = dataType == UniversalDataType.UniversalShipment;
							if (shouldGroupByRecipients)
							{
								var recipientWrapper = allRecipients.FirstOrDefault(x => x.Recipient == recipientOrganisation);
								if (recipientWrapper == null)
								{
									recipientWrapper = GetRecipient(recipientOrganisation, notifications, parent);
									allRecipients.Add(recipientWrapper);
								}

								if (!recipientWrapper.ExportedObjects.Contains(parent))
								{
									recipientWrapper.ExportedObjects.Add(parent);
								}
							}
							else
							{
								allRecipients.Add(GetRecipient(recipientOrganisation, notifications, parent));
							}
						}
					}
					else
					{
						var msg = Res.GetString(
							"96849f8f-1bb5-4368-b0f5-c9aeb02bd339",
							@"You selected '{0}' for the Recipient Type, but no Organization has been entered for this Recipient Type on {1}.
Please enter an Organization for this Recipient Type, or select another Recipient Type.",
							RecipientType,
							parent.HumanReadableName);

						notifications.AddError(msg);
					}
				}
			}
			else
			{
				allRecipients.Add(new RecipientWrapper(new UniversalXmlCommunicationModeProvider(() => (communicationModes.ToArray(), null)), parents.ToArray()));
			}

			return allRecipients;
		}

		RecipientWrapper GetRecipient(OrgHeader organisation, INotifications notifications, BusinessObject bizo)
		{
			var communicationModesProvider = new UniversalXmlCommunicationModeProvider(() =>
			{
				var modes = GetCommunicationModesForRecipient(organisation, bizo);

				if (!modes.modes.Any())
				{
					return (modes.modes, GetNoCommunicationSettingsError(organisation, modes.failureReason));
				}
				else if (modes.modes.Any(mode => mode.EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface)
					&& eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.Value.IsNullOrEmpty())
				{
					if (!eAdaptorErrorAlreadyAdded)
					{
						var message = ResString.GetMultilingualString(
							"UniversalDataMenuBuilder|SendUniversalData|OutboundAdapterServiceUrl",
							"eAdaptor Outbound Messaging has not been configured in the Registry.\r\nUniversal XML cannot be sent.");
						notifications.AddError(message);
						eAdaptorErrorAlreadyAdded = true;
						return (Array.Empty<IEDICommunicationsMode>(), message);
					}
					else
					{
						return (Array.Empty<IEDICommunicationsMode>(), null);
					}
				}
				else
				{
					return modes;
				}
			});

			return new RecipientWrapper(organisation, communicationModesProvider, bizo);
		}
		bool eAdaptorErrorAlreadyAdded;

		public static MultilingualString GetNoCommunicationSettingsError(OrgHeader recipient, MultilingualString failureReason)
		{
			var res = ResString.GetMultilingualString("5ca56600-3049-491d-b510-0bd4f004bfb8",
						@"No EDI Communications settings were found on the Recipient Organization [{0}].
Please add an entry on the [Details > Config > EDI Communications] tab of this Organization before sending Universal Data.",
						recipient.OH_Code);
			if (string.IsNullOrEmpty(failureReason))
			{
				return res;
			}
			else
			{
				return ResourceString.Join(System.Environment.NewLine, new[] { res, ResString.GetMultilingualString("bb434b76-c693-441e-9f44-f4efc82a823a", "Failure was because:"), failureReason });
			}
		}

		(IEDICommunicationsMode[] modes, MultilingualString failureReason) GetCommunicationModesForRecipient(OrgHeader recipientOrganisation, BusinessObject parent)
		{
			var modeQuery = new EDICommunicationModeQuery(
				parent: parent,
				descriptor: WorkflowDescriptor,
				fileFormat: ActionType,
				purpose: PurposeCode,
				recipientRole: RecipientType,
				eventCode: EventCode,
				eventReference: EventReference);
			return WorkflowDescriptor.GetCommunicationModesForRecipient(recipientOrganisation, modeQuery);
		}

		IEnumerable<IXmlEventValueObject> SendData(RecipientWrapper recipient, INotifications notifications)
		{
			var logger = new Logger();
			IEnumerable<IXmlEventValueObject> xmlEvents;
			try
			{
				var processor = universalXmlWorkflowProcessorFactory(recipient.CommunicationModes, recipient.ExportedObjects.ToArray());
				var replaceThisTokenEventuallyQuestionMarkExclamationMark = CancellationToken.None;
				xmlEvents = processor.Process(logger, replaceThisTokenEventuallyQuestionMarkExclamationMark);
			}
			catch (Exception ex) when (!ex.IsCriticalException() && !ex.ShouldReprocess())
			{
				// We need this error report as the exception stack trace is lost in current exceptions for some reason
				ErrorReporter.ReportOnce("Unhandled UniversalXMLWorkflow Exception", ex);
				throw;
			}
			if (logger.HasErrors)
			{
				var msg = recipient.Recipient == null
					? Res.GetString("e9edf9d8-5297-11e4-ab20-902b34dc814a", "Error sending {0}:-\r\n\r\n{1}", DataTypeName, logger.ToString().Trim())
					: Res.GetString("9bf52888-b613-4978-bdcd-dd618e004e54", "Error sending {0} to Organization [{1}]:-\r\n\r\n{2}",
						DataTypeName,
						recipient.Recipient.OH_Code,
						logger.ToString().Trim());

				notifications.AddError(msg);
			}
			else
			{
				if (recipient.CommunicationModes.Destinations.OfType<IEDICommunicationsMode>().First().EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss)
				{
					var msg = recipient.Recipient == null
						? Res.GetString("2b2a00a2-5298-11e4-baa0-902b34dc814a", "{0} sent internally.", DataTypeName)
						: Res.GetString("c864a631-568a-4cc2-9bcb-171e1cb963ff", "{0} sent internally for Organization [{1}].", DataTypeName, recipient.Recipient.OH_Code);

					notifications.Add(new InfoNotification(msg));
				}
				else
				{
					var msg = recipient.Recipient == null
						? Res.GetString("2fa23e40-5298-11e4-a124-902b34dc814a", "{0} queued for sending.", DataTypeName)
						: Res.GetString("08d66b36-f52f-41ee-9d55-6e9a5077c551", "{0} queued for sending to Organization [{1}].", DataTypeName, recipient.Recipient.OH_Code);

					notifications.Add(new InfoNotification(msg));
				}
			}

			return xmlEvents ?? Array.Empty<IXmlEventValueObject>();
		}

		IUniversalXmlWorkflowProcessor GetProcessor(IMessageProcessorCommunicationModesResult communicationsModes, BusinessObject[] exportedObjects)
		{
			IUniversalXmlWorkflowProcessor processor = null;

			var parent = exportedObjects.First();
			var actionInfo = new ActionWrapper(this, parent);

			var localDataWriterGetter = this.dataWriterGetter;

			if (localDataWriterGetter != null)
			{
				processor = UniversalXmlWorkflowProcessorBuilder.New(actionInfo
								, communicationsModes
								, localDataWriterGetter
								, parent
								, null
								, null
								, xmlSchema);
			}
			else
			{
				switch (dataType)
				{
					case UniversalDataType.UniversalShipment:
						localDataWriterGetter = (outboundSessionTracker) => WorkflowDescriptor.GetUniversalShipmentDataObjectWriter(outboundSessionTracker);
						processor = UniversalXmlWorkflowProcessorBuilder.New(actionInfo
										, communicationsModes
										, localDataWriterGetter
										, parent
										, null
										, null
										, xmlSchema);
						break;

					case UniversalDataType.UniversalEvent:
						localDataWriterGetter = (outboundSessionTracker) => WorkflowDescriptor.GetUniversalEventDataObjectWriter(outboundSessionTracker);
						processor = UniversalXmlWorkflowProcessorBuilder.New(actionInfo
										, communicationsModes
										, localDataWriterGetter
										, GetDummyLog(parent)
										, null
										, null
										, xmlSchema);
						break;

					case UniversalDataType.UniversalTransaction:
						localDataWriterGetter = (outboundSessionTracker) => WorkflowDescriptor.GetUniversalTransactionDataObjectWriter(outboundSessionTracker);
						processor = UniversalXmlWorkflowProcessorBuilder.New(actionInfo
										, communicationsModes
										, localDataWriterGetter
										, parent
										, null
										, null
										, xmlSchema);
						break;

					case UniversalDataType.UniversalSchedule:
						localDataWriterGetter = (outboundSessionTracker) => WorkflowDescriptor.GetUniversalScheduleDataObjectWriter(outboundSessionTracker);
						processor = UniversalXmlWorkflowProcessorBuilder.New(actionInfo
										, communicationsModes
										, localDataWriterGetter
										, parent
										, null
										, null
										, xmlSchema);
						break;

					case UniversalDataType.UniversalActivity:
						localDataWriterGetter = outboundSessionTracker => WorkflowDescriptor.GetUniversalActivityDataObjectWriter(outboundSessionTracker);
						processor = UniversalXmlWorkflowProcessorBuilder.New(actionInfo, communicationsModes, localDataWriterGetter, parent, null, null, xmlSchema);
						break;

					default:
						throw new InvalidOperationException("Unsupported UniversalDataType " + dataType.ToString());
				}
			}

			foreach (IWorkflowProvider exportedObject in exportedObjects.Skip(1))   // First one already used in ctor
			{
				((ISupportUniversalBatchExport)processor).AddAnotherExportedBusinessObject((BusinessObject)exportedObject);
			}

			return processor;
		}

		NonPersistentStmALog GetDummyLog(BusinessObject parent)
		{
			var result = new BusinessObjectFactory { RefreshEnabled = false, NameForDebugging = "DummyLogForManualDataExport" }.New<NonPersistentStmALog>();
			result.SL_Table = parent.TableName;
			result.SL_Parent = parent.PK;
			result.SL_EventTime = ZDateTime.Now;
			result.SL_SE_NKEvent = EventCode;
			result.SL_Reference = EventReference;

			return result;
		}

		public string DataTypeName
		{
			get
			{
				switch (dataType)
				{
					case UniversalDataType.UniversalShipment:
						return Res.GetString("192ecb0c-1a97-48b1-8c32-6f5c548988b7", "Universal Shipment");
					case UniversalDataType.UniversalEvent:
						return Res.GetString("3d0eabd7-ccda-4286-b649-557c753d87d3", "Universal Event");
					case UniversalDataType.UniversalTransaction:
						return Res.GetString("2571027b-8e8b-46c2-aa5f-c0a6b8bf48c7", "Universal Transaction");
					case UniversalDataType.UniversalSchedule:
						return Res.GetString("b1f7830d-17e2-4aed-b88f-ff026eb01a22", "Universal Schedule");
					case UniversalDataType.UniversalActivity:
						return Res.GetString("b5753713-c925-4c00-b94b-e2d562eb27f3", "Universal Activity");
				}

				throw new InvalidOperationException("Unsupported UniversalDataType " + dataType.ToString());
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			this.ValidateRecipientType();
			this.ValidateEventCode();
			this.ValidatePurposeCode();
			this.ValidateTriggerDescription();
		}

		internal string ActionType
		{
			get
			{
				switch (dataType)
				{
					case UniversalDataType.UniversalShipment:
						return WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
					case UniversalDataType.UniversalEvent:
						return WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
					case UniversalDataType.UniversalTransaction:
						return WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML;
					case UniversalDataType.UniversalSchedule:
						return WorkflowTriggerActionTypeConstants.Codes.SendUniversalScheduleXML;
					case UniversalDataType.UniversalActivity:
						return WorkflowTriggerActionTypeConstants.Codes.SendUniversalActivityXML;
				}

				throw new InvalidOperationException("Unsupported UniversalDataType " + dataType.ToString());
			}
		}

		#endregion
	}

	[TestExcludeBusinessObjectsAllHaveTestCases]
	public class NonPersistentStmALog : BaseStmALog
	{
		public NonPersistentStmALog(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void OnFactorySaving() => throw new InvalidOperationException("This log should be in a factory that is not ever saved.");
	}
}
