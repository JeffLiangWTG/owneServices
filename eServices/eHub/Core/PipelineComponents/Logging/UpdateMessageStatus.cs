using System;
using System.Collections;
using System.Runtime.InteropServices;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.DataAccess.Integration;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using System.Xml;
using Microsoft.BizTalk.XPath;
using CargoWise.eHub.Common.Extensions;

namespace CargoWise.eHub.Core.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[Guid("04B6FDC9-4CE1-4656-9411-7AEA388AD780")]
	public class UpdateMessageStatus : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return "Update inbox and/or outbox message to specified statuses."; }
		}

		public string Name
		{
			get { return "Update Message Status"; }
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
				int? inboxStatus = string.IsNullOrWhiteSpace(InboxStatus) ? null as int? : Convert.ToInt32(InboxStatus);
				int? outboxStatus = string.IsNullOrWhiteSpace(OutboxStatus) ? null as int? : Convert.ToInt32(OutboxStatus);

				string messageTrackingID = message.Context.ReadPropertyString<MessageTrackingID>();
				if (!string.IsNullOrWhiteSpace(messageTrackingID))
				{
					GetOutboxAccessor().UpdateMessageStatus(
						messageTrackingID: messageTrackingID,
						inboxStatus: inboxStatus,
						outboxStatus: outboxStatus
					);
				}
				else
				{
					var originalStream = message.BodyPart.GetOriginalDataStream();
					var xmlReader = new XmlTextReader(originalStream);
					var xpaths = new XPathCollection();
					xpaths.Add("/*[local-name()='OutboxEnvelope']");

					var xPathReader = new XPathReader(xmlReader, xpaths);
					xPathReader.ReadUntilMatch();
					xmlReader.MoveToAttribute("EnvelopeID");

					GetOutboxAccessor().UpdateMessageStatus(
						outboxPK: xmlReader.Value,
						inboxStatus: inboxStatus,
						outboxStatus: outboxStatus
					);

					originalStream.SeekBegin(); 
				}
			}

			return message;
		}

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("04B6FDC9-4CE1-4656-9411-7AEA388AD780");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			object val = null;
			try
			{
				propertyBag.Read("Enabled", out val, errorLog);
			}
			catch { }
			if (val != null) Enabled = Convert.ToBoolean(val);

			val = null;
			try
			{
				propertyBag.Read("InboxStatus", out val, errorLog);
			}
			catch { }
			if (val != null) InboxStatus = Convert.ToString(val);

			val = null;
			try
			{
				propertyBag.Read("OutboxStatus", out val, errorLog);
			}
			catch { }
			if (val != null) OutboxStatus = Convert.ToString(val);
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val = Enabled;
			propertyBag.Write("Enabled", ref val);

			val = InboxStatus;
			propertyBag.Write("InboxStatus", ref val);

			val = OutboxStatus;
			propertyBag.Write("OutboxStatus", ref val);
		}

		#endregion

		#region Properties

		public bool Enabled { get; set; }
		public string InboxStatus { get; set; }
		public string OutboxStatus { get; set; }

		#endregion

		#region Implementation

		internal virtual IOutboxAccessor GetOutboxAccessor()
		{
			return DataAccessFactories.NewOutboxAccessorInstance();
		}

		#endregion
	}
}
