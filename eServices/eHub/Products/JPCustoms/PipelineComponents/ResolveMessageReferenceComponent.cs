using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Transactions;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.DataAccess.Integration;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Products.JPCustoms.PipelineComponents
{
	[Serializable]
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[Guid("3A2F8B74-4D3F-4C35-B03E-9A6E59961575")]
	public class ResolveMessageReferenceComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return "Resolve Message Reference Component"; }
		}

		public string Name
		{
			get { return "ResolveMessageReferenceComponent"; }
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
			if (!this.Enabled) return message;
			string recipient = RecipientID;

			if (this.ResolveRecipient)
			{
				using (var transactionScope = GetTransactionScope(context))
				{
					MessageHelper.ReadMessage(context, message);  // Read message to end to ge promoted properties
					string destinationParty = message.Context.ReadPropertyString<BTS.DestinationParty>();

					if (string.IsNullOrEmpty(destinationParty))
					{
						throw new ApplicationException("Unable to resolve recipient. BTS.DestinationParty property was empty string.");
					}

					if (string.IsNullOrEmpty(ApplicationCode))
					{
						throw new ApplicationException("Unable to resolve recipient. ApplicationCode property is empty for ResolveMessageReferenceComponent.");
					}

					var clientID = GetJPCustomsDataModelAccessor().GetEHubClientIDFromCredentials(destinationParty);
					if (string.IsNullOrEmpty(clientID)) clientID = ResolveReference(destinationParty, ApplicationCode);

					if (string.IsNullOrEmpty(clientID))
					{
						GetExceptionAccessor().SubmitErrorAndUpdateStatus(Guid.NewGuid(), "BIZ", "Fai", "Could not resolve recipient with given username: " + destinationParty, Guid.Empty, Guid.Empty, Guid.Parse(message.Context.ReadPropertyString<MessageTrackingID>()), Guid.Empty, null);
						transactionScope.Complete();
						return null;
					}

					recipient = clientID;

					transactionScope.Complete();
				}
			}

			message.Context.WriteProperty<BTS.DestinationParty>(recipient);
			return message;
		}

		protected virtual TransactionScope GetTransactionScope(IPipelineContext context)
		{
			var transactionNative = (IDtcTransaction)((IPipelineContextEx)context).GetTransaction();
			return new TransactionScope(TransactionInterop.GetTransactionFromDtcTransaction(transactionNative));
		}

		internal virtual string ResolveReference(string reference, string applicationCode)
		{
			return GetRegistryAccessor().ResolveMessageReference(reference, applicationCode);
		}

		public virtual IRegistryAccessor GetRegistryAccessor()
		{
			return DataAccessFactories.NewRegistryAccessorInstance();
		}

		internal virtual IExceptionsAccessor GetExceptionAccessor()
		{
			return DataAccessFactories.NewExceptionsAccessorInstance();
		}

		internal virtual JPCustomsEhubClientIDDataModelAccessor GetJPCustomsDataModelAccessor()
		{
			return new JPCustomsEhubClientIDDataModelAccessor();
		}

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("3A2F8B74-4D3F-4C35-B03E-9A6E59961575");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			object val = null;
			Func<string, bool> getVal = p => { try { propertyBag.Read(p, out val, errorLog); } catch { } return val != null; };

			if (getVal("Enabled")) Enabled = Convert.ToBoolean(val);
			if (getVal("RecipientID")) RecipientID = Convert.ToString(val);
			if (getVal("ResolveRecipient")) ResolveRecipient = Convert.ToBoolean(val);
			if (getVal("ApplicationCode")) ApplicationCode = Convert.ToString(val);
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val;
			val = Enabled; propertyBag.Write("Enabled", ref val);
			val = RecipientID; propertyBag.Write("RecipientID", ref val);
			val = ResolveRecipient; propertyBag.Write("ResolveRecipient", ref val);
			val = ApplicationCode; propertyBag.Write("ApplicationCode", ref val);
		}

		#endregion

		#region Properties

		public bool Enabled { get; set; }
		public string RecipientID { get; set; }
		public bool ResolveRecipient { get; set; }
		public string ApplicationCode { get; set; }

		#endregion
	}
}
