using System;
using System.Collections;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.InteropServices;
using System.Transactions;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.Products.Core.PropertySchemas;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Products.Core.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[Guid("2AFDDA97-E998-4435-B08B-C4496FEA4EE8")]
	public class IncomingMessageLoggerProductComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return "Product: Writes incoming message into eHubInbox compressed and Base64-encoded."; }
		}

		public string Name
		{
			get { return "Product: Incoming Message Logger"; }
		}

		public string Version
		{
			get { return "1.0"; }
		}

		#endregion

		#region IComponentUI Members

		public IntPtr Icon
		{
			get { return IntPtr.Zero; }
		}

		public IEnumerator Validate(object projectSystem)
		{
			return null;
		}

		#endregion

		#region IComponent Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public IBaseMessage Execute(IPipelineContext pipelineContext, IBaseMessage message)
		{
			if (Enabled)
			{
				if (pipelineContext == null)
				{
					throw new ArgumentNullException("PipelineContext");
				}

				MessageHelper.WrapMessageInReadOnlySeekableStream(pipelineContext, message);
				Stream compressAndEncodedStream = message.BodyPart.Data.CompressAndEncode();
				pipelineContext.ResourceTracker.AddResource(compressAndEncodedStream);

				string emailSubject = message.Context.ReadPropertyString<EmailSubject>();
				string fileName = message.Context.ReadPropertyString<FileName>();
				string senderID = GetSenderID(message);
				string clientID = GetClientID(message);
				string applicationCode = GetApplicationCode(message);
				var schemaType = GetSchemaType(message);
				string messageType = GetMessageType(message);
				var messageTrackingID = MessageHelper.GetMessageTrackingID(message);

				Guid inboxPK;
				if (UseMessageTrackingIDAsInboxPK)
				{
					inboxPK = messageTrackingID;
				}
				else
				{
					inboxPK = Guid.NewGuid();
					message.Context.WriteProperty<InternalTrackingID>(inboxPK.ToString());
				}

				var messageStatus = (MessageStatus)Enum.Parse(typeof(MessageStatus), MessageStatusName);
				try
				{
					using (var transactionScope = GetTransactionScope(pipelineContext))
					{
						GetInboxAccessor().InsertToInbox(
							senderID,
							Guid.Empty,
							inboxPK,
							messageStatus,
							new eHubGatewayMessage()
							{
								MessageTrackingID = messageTrackingID,
								ClientID = clientID,
								ApplicationCode = applicationCode,
								SchemaName = messageType,
								SchemaType = schemaType,
								EmailSubject = string.IsNullOrEmpty(emailSubject) ? string.Empty : emailSubject,
								FileName = string.IsNullOrEmpty(fileName) ? string.Empty : fileName,
								MessageStream = compressAndEncodedStream
							});

						transactionScope.Complete();
					}
				}
				catch (SqlException ex)
				{
					var exception = ExceptionBuilder.New(ex);
					throw exception;
				}

				message.BodyPart.Data.SeekBegin();
			}

			return message;
		}

		MessageSchemaType GetSchemaType(IBaseMessage message)
		{
			string shcemaTypeString = message.Context.ReadPropertyString<SchemaType>();
			MessageSchemaType schemaType;
			if (!Enum.TryParse<MessageSchemaType>(shcemaTypeString, out schemaType))
			{
				schemaType = MessageSchemaType.Xml;
			}
			return schemaType;
		}

		string GetApplicationCode(IBaseMessage message)
		{
			string applicationCode = ApplicationCode;
			if (string.IsNullOrEmpty(applicationCode))
			{
				applicationCode = message.Context.ReadPropertyString<ApplicationCode>();
			}
			return applicationCode;
		}

		string GetSenderID(IBaseMessage message)
		{
			string senderID = SenderID;
			if (string.IsNullOrEmpty(senderID))
			{
				senderID = message.Context.ReadPropertyString<SenderID>();
			}

			if (string.IsNullOrEmpty(senderID))
			{
				senderID = message.Context.ReadPropertyString<BTS.SourceParty>();
			}
			return senderID;
		}

		string GetMessageType(IBaseMessage message)
		{
			string messageType = MessageType;

			if (string.IsNullOrEmpty(messageType))
			{
				messageType = message.Context.ReadPropertyString<SchemaName>();
			}

			if (string.IsNullOrEmpty(messageType))
			{
				messageType = message.Context.ReadPropertyString<BTS.MessageType>();
			}
			return messageType;
		}

		string GetClientID(IBaseMessage message)
		{
			string clientID = message.Context.ReadPropertyString<ClientID>();

			if (string.IsNullOrEmpty(clientID))
			{
				clientID = message.Context.ReadPropertyString<BTS.DestinationParty>();
			}
			return clientID;
		}

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("2AFDDA97-E998-4435-B08B-C4496FEA4EE8");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			object val = null;
			Func<string, bool> getVal = p => { try { propertyBag.Read(p, out val, errorLog); } catch { } return val != null; };

			if (getVal("Enabled")) Enabled = Convert.ToBoolean(val);
			if (getVal("UseMessageTrackingIDAsInboxPK")) UseMessageTrackingIDAsInboxPK = Convert.ToBoolean(val);
			if (getVal("SenderID")) SenderID = Convert.ToString(val);
			if (getVal("ApplicationCode")) ApplicationCode = Convert.ToString(val);
			if (getVal("MessageType")) MessageType = Convert.ToString(val);
			if (getVal("MessageStatusName")) MessageStatusName = Convert.ToString(val);
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val;
			val = Enabled; propertyBag.Write("Enabled", ref val);
			val = UseMessageTrackingIDAsInboxPK; propertyBag.Write("UseMessageTrackingIDAsInboxPK", ref val);
			val = SenderID; propertyBag.Write("SenderID", ref val);
			val = ApplicationCode; propertyBag.Write("ApplicationCode", ref val);
			val = MessageType; propertyBag.Write("MessageType", ref val);
			val = MessageStatusName; propertyBag.Write("MessageStatusName", ref val);
		}

		internal virtual TransactionScope GetTransactionScope(IPipelineContext context)
		{
			var transactionNative = (IDtcTransaction)((IPipelineContextEx)context).GetTransaction();
			return new TransactionScope(TransactionInterop.GetTransactionFromDtcTransaction(transactionNative));
		}

		#endregion

		#region Properties

		public bool Enabled { get; set; }
		// UseMessageTrackingIDAsInboxPK = false is the new functionality, it is included as a property as some pipelines may need to return to the previous functionality if anything breaks
		public bool UseMessageTrackingIDAsInboxPK { get; set; }
		public string SenderID { get; set; }
		public string ApplicationCode { get; set; }
		public string MessageType { get; set; }
		public string MessageStatusName { get; set; }

		#endregion

		#region Implementation

		internal virtual IInboxAccessor GetInboxAccessor()
		{
			return DataAccessFactories.NewInboxAccessorInstance();
		}

		#endregion
	}
}