using System;
using System.Collections;
using System.Runtime.InteropServices;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.Logging;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.DataAccess.Integration;
using Common.Logging;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Core.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_Any)]
    [ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
    [Guid("F1137BEA-74C3-4493-92FA-678EAB5DA4B8")]
	public class IncomingMessageLogger : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return "Writes incoming message into eHubInbox compressed and Base64-encoded."; }
		}

		public string Name
		{
			get { return "Message Logger"; }
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

		public IBaseMessage Execute(IPipelineContext pipelineContext, IBaseMessage message)
		{
			if (Enabled)
			{
				var logger = GetLogger(message);
				var activityId = GetActivityId();
				Guid inboxPK;
				var internalTrackingId = message.Context.ReadPropertyString<InternalTrackingID>();
				if (ShouldAssignNewInternalTrackingID)
				{
					inboxPK = Guid.NewGuid();
					message.Context.WriteProperty<InternalTrackingID>(inboxPK.ToString());
					logger.DebugFormat("Reassigning InternalTrackingId from '{0}' to '{1}'.", internalTrackingId, inboxPK);
				}
				else
				{
					inboxPK = new Guid(internalTrackingId);
				}
				var emailSubject = message.Context.ReadPropertyString<OverrideEmailSubject>();
				var fileName = message.Context.ReadPropertyString<OverrideFilename>();
				var messageStreamPosition = message.BodyPart.GetOriginalDataStream().Position;
				var messageTrackingId = message.Context.ReadPropertyString<MessageTrackingID>();
				var senderId = message.Context.ReadPropertyString<BTS.SourceParty>();
				var recipientId = message.Context.ReadPropertyString<BTS.DestinationParty>();

				logger.DebugFormat("[{0}] Starting InsertToInbox for PK: {1}", activityId, inboxPK);
				try
				{

					GetInboxAccessor().InsertToInbox(
						senderId,
						Guid.Empty,
						inboxPK,
						MessageStatus.Processing,
						new eHubGatewayMessage()
						{
							MessageTrackingID = new Guid(messageTrackingId),
							ClientID = recipientId,
							ApplicationCode = "BIZ",
							SchemaName = string.Empty, // message.Context.ReadPropertyString<BTS.MessageType>(),
							SchemaType = MessageSchemaType.Xml, //GetTransformationAccessor().IsFlatFile(message.Context.ReadPropertyString<BTS.MessageType>()) ? MessageSchemaType.FlatFile : MessageSchemaType.Xml,
							EmailSubject = string.IsNullOrEmpty(emailSubject) ? string.Empty : emailSubject,
							FileName = string.IsNullOrEmpty(fileName) ? string.Empty : fileName,
							MessageStream = message.BodyPart.GetOriginalDataStream().CompressAndEncode()
						});
				}
				catch (Exception ex)
				{
					logger.DebugFormat("[{0}] InsertToInbox inboxPK: [{1}], messageTrackingId: [{2}], senderId: [{3}], recipientId: [{4}], fileName: [{5}], emailSubject: [{6}] caught exception: ", ex, activityId, inboxPK, messageTrackingId, senderId, recipientId, fileName, emailSubject);
					throw;
				}
				logger.DebugFormat("[{0}] Finished InsertToInbox for PK: {1}", activityId, inboxPK);

				message.BodyPart.GetOriginalDataStream().Position = messageStreamPosition;
			}

			return message;
		}

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("F1137BEA-74C3-4493-92FA-678EAB5DA4B8");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			object val = null;
			Func<string, bool> getVal = p => { try { propertyBag.Read(p, out val, errorLog); } catch { } return val != null; };
			if (getVal("Enabled")) Enabled = Convert.ToBoolean(val);
			if (getVal("ShouldAssignNewInternalTrackingID")) ShouldAssignNewInternalTrackingID = Convert.ToBoolean(val);
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val;
			val = Enabled; propertyBag.Write("Enabled", ref val);
			val = ShouldAssignNewInternalTrackingID; propertyBag.Write("ShouldAssignNewInternalTrackingID", ref val);
		}

		#endregion

		#region Properties

		public bool Enabled { get; set; }
		public bool ShouldAssignNewInternalTrackingID {  get; set; }

		#endregion

		#region Implementation

		internal virtual ITransformAccessor GetTransformationAccessor()
		{
			return DataAccessFactories.NewTransformAccessorInstance();
		}

		internal virtual IInboxAccessor GetInboxAccessor()
		{
			return DataAccessFactories.NewInboxAccessorInstance();
		}

		internal virtual ILog GetLogger(IBaseMessage message)
		{
			return LoggerHelpers.GetPipelineLogger(message);
		}

		internal virtual Guid GetActivityId()
		{
			return Guid.NewGuid();
		}

		#endregion
	}
}
