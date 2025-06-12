using System;
using System.Collections;
using System.Runtime.InteropServices;
using BTS;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.DataAccess.Integration;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Core.PipelineComponents
{
	[Serializable]
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[Guid("02C1C425-6D89-44CF-A01D-3997B54A53BF")]
	public class UpdateDistributionStatusComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
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

		public IEnumerator Validate(object obj)
		{
			return null;
		}

		#endregion

		#region IComponent Members

		public IBaseMessage Execute(IPipelineContext context, IBaseMessage message)
		{
            if (!Enabled) return message;
		    Tracer.TraceStart(context, message);
            if (UseDeliveryNotification)
		    {
                message.Context.PromoteProperty<BTS.AckRequired>(true);
                message.Context.WriteProperty<CorrelationToken>("UseDeliveryNotificationUpdateStatus");
		    }
            else
            {
                try
                {
                    var msgEventStream = context.GetEventStream();

                    string trackingID = GetTrackingID(message);
                    string senderID = GetSenderID(message);
                    string recipientID = GetRecipientID(message);
                    msgEventStream.StoreCustomEvent(new UpdateStatusEvent(trackingID, senderID, recipientID, GetOutboxAccessor()));
                }
                catch (Exception ex)
                {
                    Tracer.TraceError(ex);
                    throw;
                }
            }
		    return message;
		}

		internal virtual IOutboxAccessor GetOutboxAccessor()
		{
			return DataAccessFactories.NewOutboxAccessorInstance();
		}

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("02C1C425-6D89-44CF-A01D-3997B54A53BF");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
            object var = null;
            object deliveryVal = null;
		    try
		    {
		        propertyBag.Read("Enabled", out var, errorLog);
                propertyBag.Read("UseDeliveryNotification", out deliveryVal, errorLog);
		        
		    }
			catch { }
            if (var != null) Enabled = (bool)var;
            if (deliveryVal != null) UseDeliveryNotification = (bool)deliveryVal;
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
            object val = Enabled;
            object deliveryVal = UseDeliveryNotification;
			propertyBag.Write("Enabled", ref val);
            propertyBag.Write("UseDeliveryNotification", ref deliveryVal);
		}

		#endregion

		#region Properties

        public bool Enabled { get; set; }
        public bool UseDeliveryNotification { get; set; }

		#endregion

		protected virtual string GetName()
		{
			return "Update Distribution Status";
		}

		protected virtual string GetTrackingID(IBaseMessage message)
		{
			return message.Context.ReadPropertyString<MessageTrackingID>();
		}

		protected virtual string GetSenderID(IBaseMessage message)
		{
			return message.Context.ReadPropertyString<BTS.SourceParty>();
		}

		protected virtual string GetRecipientID(IBaseMessage message)
		{
			return message.Context.ReadPropertyString<BTS.DestinationParty>();
		}
	}
}
