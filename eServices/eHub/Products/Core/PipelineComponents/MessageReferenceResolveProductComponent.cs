using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Transactions;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.Products.Core.PropertySchemas;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Products.Core.PipelineComponents
{
	[Serializable]
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[Guid("DB10F55C-373B-4420-85D7-F5D62C53C6FE")]
	public class MessageReferenceResolveProductComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return string.Empty; }
		}

		public string Name
		{
			get { return "Product: Resolve Message Reference"; }
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

			using (var transactionScope = GetTransactionScope(context))
			{

				if (string.IsNullOrEmpty(this.SenderID))
				{
					throw new ArgumentNullException("Specify SenderID in pipeline settings");
				}

				if (this.ResolveRecipient)
				{
					MessageHelper.ReadMessage(context, message);  // Read message to end to ge promoted properties
					string reference = message.Context.ReadPropertyString<Reference>();
					string applicationCode = message.Context.ReadPropertyString<ApplicationCode>();

					if (string.IsNullOrEmpty(reference))
					{
						throw new ArgumentNullException("CustomerReference field was not promoted.");
					}

					if (string.IsNullOrEmpty(applicationCode))
					{
						throw new ArgumentNullException("ApplicationCode field was not promoted.");
					}

					var clientID = ResolveReference(reference, applicationCode);
					if (string.IsNullOrEmpty(clientID))
					{
						var errorDescription = string.Format("Could not resolve recipient with application code: {0} and reference: {1}.", applicationCode, reference);
						GetExceptionAccessor().SubmitErrorAndUpdateStatus(Guid.NewGuid(), "BIZ", "Fai", errorDescription, Guid.Empty, Guid.Empty, Guid.Parse(message.Context.ReadPropertyString<MessageTrackingID>()), Guid.Empty, null);
						transactionScope.Complete();
						return null;
					}
					recipient = clientID;
				}
				transactionScope.Complete();
			}
			message.Context.WriteProperty<BTS.SourceParty>(SenderID);
			message.Context.WriteProperty<BTS.DestinationParty>(recipient);

			return message;
		}

		internal virtual TransactionScope GetTransactionScope(IPipelineContext context)
		{
			var transactionNative = (IDtcTransaction)((IPipelineContextEx)context).GetTransaction();
			return new TransactionScope(TransactionInterop.GetTransactionFromDtcTransaction(transactionNative));
		}

		internal virtual string ResolveReference(string reference, string applicationCode)
		{
			return GetRegistryAccessor().ResolveMessageReference(reference, applicationCode);
		}

		internal virtual IRegistryAccessor GetRegistryAccessor()
		{
			return DataAccessFactories.NewRegistryAccessorInstance();
		}

		internal virtual IExceptionsAccessor GetExceptionAccessor()
		{
			return DataAccessFactories.NewExceptionsAccessorInstance();
		}

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("DB10F55C-373B-4420-85D7-F5D62C53C6FE");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			object val = null;
			Func<string, bool> getVal = p => { try { propertyBag.Read(p, out val, errorLog); } catch { } return val != null; };

			if (getVal("Enabled")) Enabled = Convert.ToBoolean(val);
			if (getVal("SenderID")) SenderID = Convert.ToString(val);
			if (getVal("RecipientID")) RecipientID = Convert.ToString(val);
			if (getVal("ResolveRecipient")) ResolveRecipient = Convert.ToBoolean(val);
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val;
			val = Enabled; propertyBag.Write("Enabled", ref val);
			val = SenderID; propertyBag.Write("SenderID", ref val);
			val = RecipientID; propertyBag.Write("RecipientID", ref val);
			val = ResolveRecipient; propertyBag.Write("ResolveRecipient", ref val);
		}

		#endregion

		#region Properties

		public bool Enabled { get; set; }
		public string SenderID { get; set; }
		public string RecipientID { get; set; }
		public bool ResolveRecipient { get; set; }

		#endregion

	}
}
