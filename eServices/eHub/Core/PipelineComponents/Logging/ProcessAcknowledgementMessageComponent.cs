using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.PropertySchemas;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;
using CargoWise.eHub.Core.PipelineComponents.Tools;

namespace CargoWise.eHub.Core.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[Guid("56874E8F-B8ED-42AE-801C-CC57D9A896B2")]
	public class ProcessAcknowledgementMessageComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return string.Empty; }
		}

		public string Name
		{
			get { return GetName(); }
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

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("56874E8F-B8ED-42AE-801C-CC57D9A896B2");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			object var = null;
			try { propertyBag.Read("Enabled", out var, errorLog); }
			catch { }
			if (var != null) Enabled = (bool)var;

			var = null;
			try
			{
				propertyBag.Read("UsingOutboxPK", out var, errorLog);
			}
			catch { }
			if (var != null) UsingOutboxPK = Convert.ToBoolean(var);
		}

		object LoadProperty(IPropertyBag propertyBag, string propertyName, int errorLog)
		{
			object result = null;
			return result;
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val = Enabled;
			propertyBag.Write("Enabled", ref val);

			val = UsingOutboxPK;
			propertyBag.Write("UsingOutboxPK", ref val);
		}

		#endregion

		#region Properties

		public bool Enabled { get; set; }
		public bool UsingOutboxPK{ get; set; }

		#endregion

		#region IComponent Members

		public IBaseMessage Execute(IPipelineContext pipelineContext, IBaseMessage message)
		{
			if (!Enabled) return message;
			
			Tracer.TraceStart(pipelineContext, message);
			try
			{
				var ackType = message.Context.ReadPropertyString<BTS.AckType>();
				var messageTrackingID = message.Context.ReadPropertyString<MessageTrackingID>();
				var outboxPK = message.Context.ReadPropertyString<OutboxPK>();
				XNamespace xmlns = @"http://schemas.microsoft.com/Sql/2008/05/TypedProcedures/dbo";

				var doc = new XDocument(new XElement(xmlns + "ProcessAcknowledgementMessage"));

				if (ackType == "ACK")
				{
					var senderID = message.Context.ReadPropertyString<BTS.SourceParty>();
					var recipientID = message.Context.ReadPropertyString<BTS.DestinationParty>();
					doc.Root.Add(
						new XElement(xmlns + "MessageStatus", 3),
						new XElement(xmlns + "MessageTrackingID", messageTrackingID),
						new XElement(xmlns + "SenderID", senderID),
						new XElement(xmlns + "RecipientID", recipientID)
					);
				}
				else
				{
					if (UsingOutboxPK)
					{
						var description = message.Context.ReadPropertyString<BTS.AckDescription>();
						doc.Root.Add(
							new XElement(xmlns + "MessageStatus", 255),
							new XElement(xmlns + "ErrorSource", "BIZ"),
							new XElement(xmlns + "ErrorType", "Fai"),
							new XElement(xmlns + "ErrorDescription", description),
							new XElement(xmlns + "OutboxPK", outboxPK));
					}
					else 
					{
						var description = message.Context.ReadPropertyString<BTS.AckDescription>();
						doc.Root.Add(
							new XElement(xmlns + "MessageStatus", 255),
							new XElement(xmlns + "MessageTrackingID", messageTrackingID),
							new XElement(xmlns + "ErrorSource", "BIZ"),
							new XElement(xmlns + "ErrorType", "Fai"),
							new XElement(xmlns + "ErrorDescription", description));
					}
					
					
				}
				var resultStream = new VirtualStream();
				doc.Save(resultStream);
				resultStream.SeekBegin();
				message.BodyPart.Data = resultStream;

				return message;
			}
			catch (Exception ex)
			{
				Tracer.TraceError(ex);
				throw;
			}
		}

		#endregion

		protected virtual string GetName()
		{
			return "Process Acknowledgement Message";
		}
	}
}
