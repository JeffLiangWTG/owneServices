using System;
using System.Collections;
using System.Runtime.InteropServices;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using CargoWise.eHub.DataAccess.Integration;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Core.PipelineComponents
{
    /// <summary>
    /// Either the source and destimation party have been promoted by the time this component is called or static values
    /// can be used to set the sender and recipient as appropriate.  Optional validation of the recipient and/or
    /// sender is available too.  If both sender and recipient are to be resolved then the relationship between them
    /// is also validated.
    /// </summary>
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[Guid("D262069D-F101-4474-AD7F-8DA128014C2F")]
	public class PartyResolutionComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return string.Empty; }
		}

		public string Name
		{
			get { return "eHub Party Resolution"; }
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
			// Only continue processing if this is not an AS2 MDN
			if (PropertyInspector.IsAs2Mdn(message)) return message;

			// Only continue processing if this is not an EDI acknowledgement
			if (PropertyInspector.IsSystemGeneratedEdiAck(message)) return message;

            Tracer.TraceStart(context, message);
            try
            {
                Tracer.TraceInfo("ResolveSender: {0} ({1})", this.ResolveSender, this.SenderID);
                Tracer.TraceInfo("ResolveRecipient: {0} ({1})", this.ResolveRecipient, this.RecipientID);

                // Check that the sender and recipient party IDs exist
                string sender = string.IsNullOrEmpty(this.SenderID) ? message.Context.ReadPropertyString<BTS.SourceParty>() : SenderID;
                string recipient = string.IsNullOrEmpty(this.RecipientID) ? message.Context.ReadPropertyString<BTS.DestinationParty>() : RecipientID;

				if (this.ResolveSender || this.ResolveRecipient)
				{
					var partyAccessor = GetPartyAccessor();

					if (this.ResolveSender)
						if (String.IsNullOrEmpty(sender) || !partyAccessor.ClientExists(sender))
							throw new ApplicationException(String.Format("Sender '{0}' not recognized.", sender));
						else
							Tracer.TraceInfo("Resolved SenderID: {0}", sender);

					if (this.ResolveRecipient)
						if (String.IsNullOrEmpty(recipient) || !partyAccessor.ClientExists(recipient))
							throw new ApplicationException(String.Format("Recipient '{0}' not recognized.", recipient));
						else
							Tracer.TraceInfo("Resolved RecipientID: {0}", recipient);
				}

                // Set the context properties
                message.Context.WriteProperty<BTS.SourceParty>(sender);
                message.Context.WriteProperty<BTS.DestinationParty>(recipient);

                Tracer.TraceEnd();

                return message;
            }
            catch (Exception ex)
            {
                Tracer.TraceError(ex);
                throw;
            }
		}

		internal virtual IPartyAccessor GetPartyAccessor()
		{
			return DataAccessFactories.NewPartyAccessorInstance();
		}

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("D262069D-F101-4474-AD7F-8DA128014C2F");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			object val = null;
			try
			{
				propertyBag.Read("SenderID", out val, errorLog);
			}
			catch { }
			if (val != null) SenderID = (string)val;

			val = null;
			try
			{
				propertyBag.Read("RecipientID", out val, errorLog);
			}
			catch { }
			if (val != null) RecipientID = (string)val;

            val = null;
            try
            {
                propertyBag.Read("ResolveSender", out val, errorLog);
            }
            catch { }
            if (val != null) ResolveSender = (bool)val;

            val = null;
            try
            {
                propertyBag.Read("ResolveRecipient", out val, errorLog);
            }
            catch { }
            if (val != null) ResolveRecipient = (bool)val;
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val = SenderID;
			propertyBag.Write("SenderID", ref val);

			val = RecipientID;
			propertyBag.Write("RecipientID", ref val);

            val = ResolveSender;
            propertyBag.Write("ResolveSender", ref val);

            val = ResolveRecipient;
            propertyBag.Write("ResolveRecipient", ref val);
		}

		#endregion

		#region Properties

		public string SenderID { get; set; }
		public string RecipientID { get; set; }
		public bool ResolveSender { get; set; }
		public bool ResolveRecipient { get; set; }
        
		#endregion
	}
}
