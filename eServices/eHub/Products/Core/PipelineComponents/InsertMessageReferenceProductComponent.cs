using System;
using System.Collections;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Transactions;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.DataModel.Accessors;
using CargoWise.eHub.Products.Core.PropertySchemas;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Products.Core.PipelineComponents
{
	[Serializable]
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[Guid("E3F0130B-9596-48B3-BE1F-0260390ACAC9")]
	public class InsertMessageReferenceProductComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return string.Empty; }
		}

		public string Name
		{
			get { return "Product: Insert Message Reference"; }
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

			string senderID = message.Context.ReadPropertyString<SenderID>();
			string reference = this.Reference;
			string applicationCode = this.ApplicationCode;

			if (string.IsNullOrEmpty(reference))
			{
				MessageHelper.ReadMessage(context, message); // Read message to end to ge promoted properties
				reference = message.Context.ReadPropertyString<Reference>();
				applicationCode = message.Context.ReadPropertyString<ApplicationCode>();
			}

			if (!string.IsNullOrEmpty(reference) && !string.IsNullOrEmpty(applicationCode) && !string.IsNullOrEmpty(senderID))
			{
				using (var transactionScope = GetTransactionScope(context))
				{
					try
					{
						GetRegistryAccessor().InsertMessageReference(senderID, applicationCode, reference);
						transactionScope.Complete();
					}
					catch (SqlException ex)
					{
						var hasDuplicateReferenceException = ex.ToString().Contains("Message reference " + reference + " already exists for different client.")
														 &&
														 ex.Number == 50000;
						if (!hasDuplicateReferenceException)
						{
							var exception = ExceptionBuilder.New(ex);
							throw exception;
						}

						var trackingID = MessageHelper.GetMessageTrackingID(message);
						try
						{
							eHubTransactionsAccessor.InsertError(trackingID.ToString(), ex.ToString());
						}
						catch (SqlException e)
						{
							var exception = ExceptionBuilder.New(e);
							throw exception;
						}
						transactionScope.Complete();
						return null;
					}
				}
			}


			return message;
		}

		internal virtual IRegistryAccessor GetRegistryAccessor()
		{
			return DataAccessFactories.NewRegistryAccessorInstance();
		}

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("E3F0130B-9596-48B3-BE1F-0260390ACAC9");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			object val = null;
			Func<string, bool> getVal = p => { try { propertyBag.Read(p, out val, errorLog); } catch { } return val != null; };

			if (getVal("Enabled")) Enabled = Convert.ToBoolean(val);
			if (getVal("Reference")) Reference = Convert.ToString(val);
			if (getVal("ApplicationCode")) ApplicationCode = Convert.ToString(val);
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val;
			val = Enabled; propertyBag.Write("Enabled", ref val);
			val = Reference; propertyBag.Write("Reference", ref val);
			val = ApplicationCode; propertyBag.Write("ApplicationCode", ref val);
		}

		internal virtual TransactionScope GetTransactionScope(IPipelineContext context)
		{
			var transactionNative = (IDtcTransaction)((IPipelineContextEx)context).GetTransaction();
			return new TransactionScope(TransactionInterop.GetTransactionFromDtcTransaction(transactionNative));
		}

		#endregion

		#region Properties

		public bool Enabled { get; set; }
		public string Reference { get; set; }
		public string ApplicationCode { get; set; }

		#endregion

	}
}
