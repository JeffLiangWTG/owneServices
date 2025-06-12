using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CargoWise.eHub.Common;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.DataAccess.Integration;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Core.PipelineComponents
{

	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[Guid("476D06DD-3B50-4217-9E53-AD0E5362B21D")]
	public class NewTrackingIDAssignmentComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{

		#region IBaseComponent Members

		public string Description
		{
			get { return "Assign new tracking IDs for multiple messages"; }
		}

		public string Name
		{
			get { return "Assign new tracking IDs for multiple messages"; }
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


		public IBaseMessage Execute(IPipelineContext pipelineContext, IBaseMessage message)
		{
			Tracer.TraceStart(pipelineContext, message);

			try
			{
				Tracer.TraceInfo("Enabled(AssignNewTrackingID): {0}", Enabled);
				Tracer.TraceInfo("ProcessSubscriptions: {0}", ProcessSubscriptions);
				Tracer.TraceInfo("DiscardUnsubscribeMessage: {0}", DiscardUnsubscribeMessage);
				Tracer.TraceInfo("PropertyOverride: {0}", PropertyOverride);
				Tracer.TraceInfo("DiscardX12_997: {0}", DiscardX12_997);

				if (message.Context.ReadPropertyString<BTS.RouteDirectToTP>() == "True")
				{
					Tracer.TraceInfo("RoutDirectToTP: True");
					Tracer.TraceEnd();
					return message;
				}

				bool shouldDiscardTheMessage = false;

				if (ProcessSubscriptions)
				{
					var msgHelper = GetMessageHelper();
					var subscribers = msgHelper.GetSubscribers(message);

					if (subscribers.Count == 1)
					{
						message.Context.WriteProperty<BTS.DestinationParty>(subscribers[0]);
						Tracer.TraceInfo("Subscribed Party: {0}", message.Context.ReadPropertyString<BTS.DestinationParty>());
					}
					else if (subscribers.Count > 1)
					{
						throw new ApplicationException("Multiple subcribers found for message which is not supported for this pipeline component. Selected subcribers are: " + string.Join(", ", subscribers));
					}
					else
					{
						if (String.IsNullOrEmpty(message.Context.ReadPropertyString<BTS.DestinationParty>()) && DiscardUnsubscribeMessage)
						{
							shouldDiscardTheMessage = true;

							UpdateInboxStatusToDistributed(message, "No Subscription is found and DiscardUnsubscribeMessage is enabled. Message will be discarded.");
						}
					}
				}

				if (DiscardX12_997 && message.Context.ReadPropertyString<BTS.MessageType>() == "http://schemas.microsoft.com/Edi/X12#X12_997_Root")
				{
					shouldDiscardTheMessage = true;

					UpdateInboxStatusToDistributed(message, "DiscardX12_997 is enabled. Message will be discarded.");
				}

				if (!shouldDiscardTheMessage)
				{
					if (Enabled)
					{
						message.Context.WriteProperty<MessageTrackingID>(Guid.NewGuid().ToString().ToUpper());
						Tracer.TraceInfo("NewTrackingID: {0}", message.Context.ReadPropertyString<MessageTrackingID>());
					}

					if (!string.IsNullOrWhiteSpace(PropertyOverride))
					{
						var parts = PropertyOverride.Split(',');
						var target = new Uri(parts[0]);
						var source = new List<object>();
						for (int i = 2; i < parts.Length; i++)
						{
							var prop = new Uri(parts[i]);
							var value = message.Context.Read(
								prop.GetComponents(UriComponents.Fragment, UriFormat.Unescaped),
								prop.GetComponents(UriComponents.HttpRequestUrl, UriFormat.Unescaped)
							);
							source.Add(value);
							Tracer.TraceInfo("Property Override Source: {0} = {1}", parts[i], value);
						}
						var result = string.Format(parts[1], source.ToArray());
						message.Context.Write(target.GetComponents(UriComponents.Fragment, UriFormat.Unescaped), target.GetComponents(UriComponents.HttpRequestUrl, UriFormat.Unescaped), result);
						Tracer.TraceInfo("Property Override Results: {0} = {1}", parts[0], result);
					}
				}

				Tracer.TraceEnd();
				return (shouldDiscardTheMessage) ? null : message;
			}
			catch (Exception ex)
			{
				Tracer.TraceError(ex);
				throw new ApplicationException(ex.ToString());
			}
		}

		void UpdateInboxStatusToDistributed(IBaseMessage message, string traceInfo)
		{
			string trackingID = message.Context.ReadPropertyString<MessageTrackingID>();

			if (!String.IsNullOrEmpty(trackingID))
			{
				int status = Convert.ToInt32(MessageStatus.Distributed);
				GetOutboxAccessor().UpdateMessageStatus(messageTrackingID: trackingID, inboxStatus: status);
			}
			Tracer.TraceInfo(traceInfo);
		}

		internal virtual IMessageHelper GetMessageHelper()
		{
			return new MessageHelper();
		}

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("476D06DD-3B50-4217-9E53-AD0E5362B21D");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			object val = null;
			Func<string, bool> getVal = p => { try { propertyBag.Read(p, out val, errorLog); } catch { } return val != null; };

			if (getVal("Enabled")) Enabled = Convert.ToBoolean(val);
			if (getVal("ProcessSubscriptions")) ProcessSubscriptions = Convert.ToBoolean(val);
			if (getVal("DiscardUnsubscribeMessage")) DiscardUnsubscribeMessage = Convert.ToBoolean(val);
			if (getVal("PropertyOverride")) PropertyOverride = Convert.ToString(val);
			if (getVal("DiscardX12_997")) DiscardX12_997 = Convert.ToBoolean(val);
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val;
			val = Enabled; propertyBag.Write("Enabled", ref val);
			val = ProcessSubscriptions; propertyBag.Write("ProcessSubscriptions", ref val);
			val = DiscardUnsubscribeMessage; propertyBag.Write("DiscardUnsubscribeMessage", ref val);
			val = PropertyOverride; propertyBag.Write("PropertyOverride", ref val);
			val = DiscardX12_997; propertyBag.Write("DiscardX12_997", ref val);
		}

		internal virtual IOutboxAccessor GetOutboxAccessor()
		{
			return DataAccessFactories.NewOutboxAccessorInstance();
		}

		#region Properties

		public bool Enabled { get; set; }
		public bool ProcessSubscriptions { get; set; }
		public bool DiscardUnsubscribeMessage { get; set; }
		public string PropertyOverride { get; set; }
		public bool DiscardX12_997 { get; set; }

		#endregion
	}
}
