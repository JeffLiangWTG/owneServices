 using System;
using System.Collections;
using System.Runtime.InteropServices;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.Products.AirMessaging.Schemas;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using CargoWise.eHub.Products.AirMessaging.Schemas.Properies;
using System.IO;

namespace CargoWise.eHub.Products.AirMessaging.PipelineComponents
{
	[Serializable]
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_PartyResolver)]
	[Guid("33016F9D-EC04-4C73-BC52-40227EB2C4F4")]

	public class AirMessagePartyResolverComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return string.Empty; }
		}

		public string Name
		{
			get { return "AirMessage PartyResolver"; }
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

		public IBaseMessage Execute(IPipelineContext context, IBaseMessage message)
		{
			ReadMessage(context, message);

			// Check that the sender and recipient party IDs exist
			string sender = string.IsNullOrEmpty(this.SenderID) ? message.Context.ReadPropertyString<SenderPIMA>() : SenderID;
			string recipient = string.IsNullOrEmpty(this.RecipientID) ? message.Context.ReadPropertyString<RecipientPIMA>() : RecipientID;

			if (String.IsNullOrEmpty(sender))
			{
				throw new ApplicationException("Sender was not promoted or specified as a parameter.");
			}

			if (String.IsNullOrEmpty(recipient))
			{
				throw new ApplicationException("Recipient was not promoted or specified as a parameter.");
			}

			if (this.ResolveSender)
			{
				var resolvedSender = ResolvePIMA(sender);
				if (String.IsNullOrEmpty(resolvedSender))
				{
					throw new ApplicationException(String.Format("Unable to resolve a sender '{0}'.", sender));
				}

				sender = resolvedSender;
			}

			if (this.ResolveRecipient)
			{
				var resolvedRecipient = ResolvePIMA(recipient);
				if (String.IsNullOrEmpty(resolvedRecipient))
				{
					throw new ApplicationException(String.Format("Unable to resolve a recipient '{0}'.", recipient));
				}

				recipient = resolvedRecipient;
			}

			// Set the context properties
			message.Context.WriteProperty<BTS.SourceParty>(sender);
			message.Context.WriteProperty<BTS.DestinationParty>(recipient);

			return message;
		}

		static void ReadMessage(IPipelineContext context, IBaseMessage message)
		{
			var streamWrapper = new StreamWrapperComponent();
			var wrappedMessage = streamWrapper.Execute(context, message);
			var reader = new StreamReader(wrappedMessage.BodyPart.Data);
			reader.ReadToEnd();
			wrappedMessage.BodyPart.Data.Position = 0;
		}

		internal virtual string ResolvePIMA(string pima)
		{
			var clientId = ResolveParty(pima);
			if (String.IsNullOrEmpty(clientId))
			{
				clientId = ResolveAirline(pima);
			}

			return clientId;
		}

		internal virtual string ResolveParty(string clientID)
		{
			if (!string.IsNullOrEmpty(clientID))
			{
				return GetPartyAccessor().GetClientIDFromAirPIMA(clientID, ServiceProviderID);
			}

			return null;
		}

		internal virtual string ResolveAirline(string airlineCode)
		{
			if (!string.IsNullOrEmpty(airlineCode))
			{
				return GetPartyAccessor().GetClientIDFromAirlineCode(airlineCode, ServiceProviderID);
			}

			return null;
		}

		internal virtual IPartyAccessor GetPartyAccessor()
		{
			return DataAccessFactories.NewPartyAccessorInstance();
		}

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("33016F9D-EC04-4C73-BC52-40227EB2C4F4");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			object val = null;
			try
			{
				propertyBag.Read("ServiceProviderID", out val, errorLog);
			}
			catch { }
			if (val != null) ServiceProviderID = (string)val;
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
			object val = ServiceProviderID;
			propertyBag.Write("ServiceProviderID", ref val);

			val = SenderID;
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

		public string ServiceProviderID { get; set; }
		public string SenderID { get; set; }
		public string RecipientID { get; set; }
		public bool ResolveSender { get; set; }
		public bool ResolveRecipient { get; set; }

		#endregion
	}
}
