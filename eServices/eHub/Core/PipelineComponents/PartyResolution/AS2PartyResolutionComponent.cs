using System;
using System.Collections;
using System.Runtime.InteropServices;
using CargoWise.eHub.DataAccess.Integration;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Core.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[Guid("11AABAC0-EAE8-494E-B44F-3CB5C0FB79D4")]
	public class AS2PartyResolutionComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return string.Empty; }
		}

		public string Name
		{
			get { return "eHub AS2 Party Resolution"; }
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
			string sender = string.IsNullOrEmpty(this.SenderID) ? message.Context.ReadPropertyString<BTS.SourceParty>() : SenderID;
			string recipient = string.IsNullOrEmpty(this.RecipientID) ? message.Context.ReadPropertyString<BTS.DestinationParty>() : RecipientID;

			var partyAccessor = GetPartyAccessor();

			if (this.ResolveSender)
			{
				var senderCode = message.Context.ReadPropertyString<EdiIntAS.AS2From>();
				sender = partyAccessor.GetClientIDFromAS2Code(senderCode);
			}

			if (this.ResolveRecipient)
			{
				var recipientCode = message.Context.ReadPropertyString<EdiIntAS.AS2To>();
				recipient = partyAccessor.GetClientIDFromAS2Code(recipientCode);
			}

			if (String.IsNullOrEmpty(sender) || !partyAccessor.ClientExists(sender)) throw new ApplicationException(String.Format("Sender '{0}' not recognized.", sender));
			if (String.IsNullOrEmpty(recipient) || !partyAccessor.ClientExists(recipient)) throw new ApplicationException(String.Format("Recipient '{0}' not recognized.", recipient));

			message.Context.WriteProperty<BTS.SourceParty>(sender);
			message.Context.WriteProperty<BTS.DestinationParty>(recipient);

			return message;
		}

		internal virtual IPartyAccessor GetPartyAccessor()
		{
			return DataAccessFactories.NewPartyAccessorInstance();
		}

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("11AABAC0-EAE8-494E-B44F-3CB5C0FB79D4");
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
