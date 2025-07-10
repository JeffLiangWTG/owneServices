using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.eHub.Common.Extensions;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Codes = Enterprise.MasterFiles.Business.EDICommunicationsModeCommsDirectionList.Codes;

namespace Enterprise.MasterFiles.Business.MessageDelivery
{
	public abstract class EServicesDelivery : Delivery, IBatchDelivery
	{
		class PreDeliveryMessageNumberProcessor
		{
			IEDIMessage message;
			readonly DeliveryContext context;
			readonly IDeliveryStreamWrapper stream;

			public PreDeliveryMessageNumberProcessor(DeliveryContext context, IDeliveryStreamWrapper stream)
			{
				this.context = Argument.NotNull(context, nameof(context));
				this.stream = Argument.NotNull(stream, nameof(stream));
			}

			public void SetMessageAndInterchange(IEDIMessage message, IEDIInterchange interchange)
			{
				this.message = message;

				stream.SetMessageNumber(MessageNumberType.TrackingID, (ZString)interchange.EI_SessionGUID.ToString());
				stream.SetMessageNumber(MessageNumberType.InterchangeNumber, interchange.EI_InterchangeNum);
				stream.SetMessageNumber(MessageNumberType.MessageNumber, message.EM_MessageNum);
				if (!message.EM_ExternalReferenceNumber.IsEmpty)
				{
					stream.SetMessageNumber(MessageNumberType.External, message.EM_ExternalReferenceNumber);
				}

				var xmlDataProvider = ObjectFactory.Get<IXMLDataProvider>();
				if (!xmlDataProvider.IgnoreTimestamp)
				{
					stream.SetTimestamp(xmlDataProvider.Timestamp);
				}

				EnsureStreamPopulated();
			}

			void EnsureStreamPopulated()
			{
				stream.PopulateStream(context.Factory);

				if (!context.MessageSubTypeCode.IsEmpty)
				{
					message.EM_MessageSubType = context.MessageSubTypeCode;
				}
				else
				{
					if (message is IXmlEDIMessage xmlEDIMessage)
					{
						xmlEDIMessage.SetMessageTypeFromStream(stream.Content);
					}
				}

				var messageStream = stream.GetMessageStream();
				context.Factory.SubscribeForDispose(messageStream);
				message.SetEM_MessageTextOrDataSource(messageStream);
			}
		}

		public override IDeliveryResult Deliver(DeliveryContext context, IEDICommunicationsMode mode, IDeliveryStreamWrapper stream, Func<IEDIMessage> getmessageFunc = null)
		{
			return MessageDeliveryExceptionHandler.DoWithExceptionHandling(context, ErrorNotifier, () => GetType().Name, mode, new[] { stream }, () => BuildEServicesMessage(context, mode, stream, getmessageFunc));
		}

		public IDeliveryResult DeliverBatch(DeliveryContext context, IEDICommunicationsMode mode, IDeliveryStreamWrapper[] batchStreams, Func<IEDIMessage> getmessageFunc = null)
		{
			return MessageDeliveryExceptionHandler.DoWithExceptionHandling(context, ErrorNotifier, () => GetType().Name, mode, batchStreams, () => BuildEServicesMessage(context, mode, batchStreams, getmessageFunc));
		}

		public IErrorNotifier<IEDICommunicationsMode> ErrorNotifier { get; set; }

		protected abstract string GetRecipientID(IEDICommunicationsMode mode);
		protected abstract string InterchangeQueuedStatus { get; }
		protected virtual string MessageQueuedStatus => EDIMessageStatusList.Codes.Sent;

		protected abstract string TransportType { get; }

		protected virtual string GetInterchangeTo(IEDICommunicationsMode mode)
		{
			return GetRecipientID(mode);
		}

		public IXmlEDIInterchange InterchangeCreated { get; private set; }

		public ZString ExternalReferenceNumber { get; set; }

		internal IXmlEDIInterchange BuildEServicesMessage(DeliveryContext context, IEDICommunicationsMode mode, IDeliveryStreamWrapper stream, Func<IEDIMessage> getmessageFunc = null)
		{
			InterchangeCreated = BuildEServicesMessage(context, mode, new IDeliveryStreamWrapper[] { stream }, getmessageFunc);
			return InterchangeCreated;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Error log text, Logs are English only., Logs are in English Only")]
		IXmlEDIInterchange BuildEServicesMessage(DeliveryContext context, IEDICommunicationsMode mode, IDeliveryStreamWrapper[] batchStreams, Func<IEDIMessage> getmessageFunc)
		{
			Argument.NotNull(context.Notifications, "context.Notifications");
			if (batchStreams.Length == 0)
			{
				context.Notifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Warning, (NoResString)"There is nothing to deliver." + MessageDeliveryExceptionHandler.GetModeDetails(mode)));
			}

			var factory = context.Factory;

			var headerText = CreateHeaderText(mode);
			var messageCommDirection = Codes.Transmit;
			var senderId = Env.CurrentCompany.GetLicenceCode();
			var recipientId = GetRecipientID(mode);

			var interchangeTo = GetInterchangeTo(mode);
			if (string.IsNullOrEmpty(interchangeTo))
			{
				throw new ArgumentException("Interchange cannot be empty", nameof(mode));
			}

			if (interchangeTo.Length > EDIInterchangeSchema.EI_To.MaxLength)
			{
				throw new ArgumentException(FormattableString.Invariant($"Max length of Interchange destination is exceeded [{interchangeTo}]"), nameof(mode));
			}

			var interchange = factory.New<IXmlEDIInterchange>();
			interchange.AssignInterchangeNumber();

			var messages = new List<IEDIMessage>();

			try
			{
				if (!context.ApplicationCode.IsEmpty)
				{
					interchange.EI_ApplicationCode = context.ApplicationCode;
				}
				interchange.EI_InterchangeType = context.MessageTypeCode.IsEmpty ? interchange.GetInterchangeTypeFromFileFormat(mode.EK_FileFormat) : context.MessageTypeCode;
				interchange.EI_ReceiveTransmit = messageCommDirection;
				interchange.EI_From = senderId;
				interchange.EI_To = interchangeTo;

				interchange.EI_HeaderText = headerText;
				interchange.EI_GB = GlbBranch.CurrentBranch.PK;
				interchange.EI_Status = InterchangeQueuedStatus;
				interchange.EI_TransportType = TransportType;
				interchange.EI_ECC_CommunicationPartyConfig = mode.EK_ECC_CommunicationPartyConfig;

				foreach (var messageStreamWrapper in batchStreams)
				{
					var processor = new PreDeliveryMessageNumberProcessor(context, messageStreamWrapper);
					IEDIMessage message;

					if (getmessageFunc != null)
					{
						message = getmessageFunc.Invoke();
						interchange.AddMessage(message);
					}
					else
					{
						message = interchange.AddNeweHubMessage();
						if (!context.ApplicationCode.IsEmpty)
						{
							message.EM_ApplicationCode = context.ApplicationCode;
						}
					}

					message.AssignMessageNumber();
					message.EM_MessageType = context.MessageTypeCode.IsEmpty ? new ZString(EDIMessageTypeList.Codes.XMS) : context.MessageTypeCode;
					message.EM_IsTestMessage = false;
					message.EM_ExternalReferenceNumber = ExternalReferenceNumber;

					processor.SetMessageAndInterchange(message, interchange);

					if (message.EM_MessageSubType == EDIMessageSubTypeList.Codes.Unknown)
					{
						var modeDetails = MessageDeliveryExceptionHandler.GetModeDetails(mode);
						context.Notifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Warning, FormattableString.Invariant($"EDI Messages with unknown Sub Type can't be created. {modeDetails}")));
						message.Delete();
						messageStreamWrapper.GetMessageStream().SeekBegin();
						var content = Encoding.Default.GetString(messageStreamWrapper.GetMessageStream().ToByteArray());
						context.Notifications.AddError(string.Join(System.Environment.NewLine, (NoResString)"Stream Data:", content));
						ErrorReporter.ReportOnce(string.Join(System.Environment.NewLine, "We should not be creating unknown messages.", modeDetails, "ApplicationCode: " + context.ApplicationCode, "Stream Data: " + content));
						continue;
					}
					else
					{
						messages.Add(message);
					}

					message.EM_ReceiveTransmit = messageCommDirection;

					message.EM_GB = Env.CurrentBranch.PK;
					message.EM_GE = Env.CurrentDepartment.PK;
					message.EM_Status = MessageQueuedStatus;
					
					if (messageStreamWrapper.SourceInfo != null)
					{
						var parentInfo = messageStreamWrapper.SourceInfo;
						if (!string.IsNullOrEmpty(parentInfo.TableName))
						{
							message.EM_LinkUniqueID = parentInfo.InternalPK;
						}
						if (parentInfo.InternalPK != Guid.Empty)
						{
							message.EM_LinkTable = parentInfo.TableName;
						}

						var triggerObjectInfo = context.TriggerObjectInfo ?? parentInfo;
						var triggerBO = factory.Load(triggerObjectInfo.Type, triggerObjectInfo.InternalPK);

						if (triggerObjectInfo != null &&
							triggerObjectInfo.Type != null && !triggerObjectInfo.InternalPK.Equals(Guid.Empty) && new ZGuid(triggerObjectInfo.InternalPK).IsValid &&
							triggerBO == null)
						{
							context.Notifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Warning,
								"Cannot find triggering business object. Type: " + triggerObjectInfo.Type.FullName + ", PK:" + triggerObjectInfo.InternalPK.ToString() +
								MessageDeliveryExceptionHandler.GetModeDetails(mode)));
						}

						var triggerBOWithLogs = triggerBO as IStmALogParent;
						if (triggerBOWithLogs != null)
						{
							var interestedLogParents = GetInterestedLogParents(triggerBOWithLogs);
							foreach (var logParent in interestedLogParents)
							{
								GetMessageDataLogLinker(factory, context).LinkMessageToParentBOLogs(message, logParent);
							}
						}
					}

					ExecuteExtraInitializationForMessage(message);
				}
				var interchangeStream = WrapInterchange(context, batchStreams.Select(x => x.GetMessageStream()), senderId, recipientId);
				interchange.SetEI_BodyTextOrDataSource(interchangeStream);
			}
			catch
			{
				messages.ForEach(m => m.Delete());
				interchange.Delete();
				throw;
			}

			if (messages.Count == 0)
			{
				context.Notifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Error, FormattableString.Invariant($"EDI Interchanges that only have Edi Messages with unknown Sub Type can't be created. {MessageDeliveryExceptionHandler.GetModeDetails(mode)}")));
				interchange.Delete();
			}

			return interchange;
		}

		protected virtual void ExecuteExtraInitializationForMessage(IEDIMessage message) { }

		protected virtual IEnumerable<IStmALogParent> GetInterestedLogParents(IStmALogParent triggerBOWithLogs)
		{
			return new List<IStmALogParent> { triggerBOWithLogs };
		}

		protected virtual MessageDataLogLinker GetMessageDataLogLinker(BusinessObjectFactory factory, DeliveryContext context)
		{
			return new MessageDataExportImportLogLinker(Events.DataExport, factory, context.PurposeCode);
		}

		protected virtual Stream WrapInterchange(DeliveryContext context, IEnumerable<Stream> allMessageStreams, string senderId, string recipientId)
		{
			var persistentStream = context.Factory.SubscribeForDispose(new CargoWise.IO.Shim.SubStreamableStream());

			if (context.ApplicationCode == ApplicationCodeList.Codes.UniversalDataMessaging
				|| context.ApplicationCode == ApplicationCodeList.Codes.NativeDataMessaging
				|| context.ApplicationCode == ApplicationCodeList.Codes.AirCargoAdvanceScreening)
			{
				using (var writer = XmlWriter.Create(persistentStream, new XmlWriterSettings { OmitXmlDeclaration = true, Encoding = MessageEncoding.UTF8WithoutBOM, Indent = true }))
				{
					writer.WriteStartElement("UniversalInterchange", UniversalXmlInfo.Namespace_2011_11);
					writer.WriteAttributeString("xmlns", UniversalXmlInfo.Namespace_2011_11);
					writer.WriteAttributeString("version", UniversalXmlInfo.Version_2011_11);
					writer.WriteStartElement("Header");
					writer.WriteElementString("SenderID", senderId);
					writer.WriteElementString("RecipientID", recipientId);
					writer.WriteEndElement();
					writer.WriteStartElement("Body");

					foreach (Stream messageStream in allMessageStreams)
					{
						messageStream.SeekBegin();

						var reader = XmlReader.Create(messageStream);
						reader.MoveToContent();

						writer.WriteNode(reader, true);
					}

					writer.WriteEndElement();
					writer.WriteEndElement();
				}
			}
			else
			{
				foreach (Stream messageStream in allMessageStreams)
				{
					messageStream.SeekBegin();
					messageStream.WriteTo(persistentStream);
					messageStream.SeekBegin();
				}
			}

			persistentStream.SeekBegin();
			return persistentStream;
		}

		protected virtual ZString CreateHeaderText(IEDICommunicationsMode mode)
		{
			return new XElement("EDIDelivery",
				new XElement("FileName", mode.EK_Filename),
				new XElement("EmailSubject", mode.EK_ServerAddressSubject)
				).ToString(SaveOptions.DisableFormatting);
		}
	}
}
