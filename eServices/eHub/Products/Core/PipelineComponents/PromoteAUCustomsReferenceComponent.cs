using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Transactions;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Products.Core.PropertySchemas;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Products.Core.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[Guid("7B3C2AD9-F1C3-42CC-8F3A-AD292ABF5CA8")]
	public class PromoteAUCustomsReferenceComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Membersва

		public string Description
		{
			get { return "Product: Promote AUCustoms reference component"; }
		}

		public string Name
		{
			get { return "Product: Promote AUCustoms reference component"; }
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
			if (!this.Enabled) return message;
			MessageHelper.WrapMessageInReadOnlySeekableStream(pipelineContext, message);


			string reference = ExtractMessageReference(message);

			using (var transactionScope = GetTransactionScope(pipelineContext))
			{
				if (string.IsNullOrEmpty(reference)) throw new InvalidOperationException("AU Customs Recipient reference can't be extracted.");
				message.Context.WriteProperty<Reference>(reference);
				transactionScope.Complete();
			}

			message.BodyPart.Data.SeekBegin();
			return message;
		}

		static string ExtractMessageReference(IBaseMessage message)
		{
			string text = message.BodyPart.Data.ReadToEnd();
			var decodedText = DecodeEnvelop(text);
			return ParseReference(decodedText);
		}

		static string ParseReference(string encodedText)
		{
			var parts = encodedText.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.None);
			if (parts.Length != 2) throw new InvalidOperationException("AU customs reply message has incorrect format");

			var lines = parts[1].Split(new string[] { "'" }, StringSplitOptions.None);

			foreach (var line in lines)
			{
				if (line.StartsWith("UNB"))
				{
					var segments = line.Split(new string[] { "+" }, StringSplitOptions.None);
					return segments[3].Split(new string[] { ":" }, StringSplitOptions.None)[0];
				}
			}

			return string.Empty;
		}

		static string DecodeEnvelop(string encodedText)
		{
			var parts = encodedText.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.None);
			if (parts.Length != 2) throw new InvalidOperationException("AU customs reply message has incorrect format");
			return DecodeFrom64(parts[1]);
		}

		static string DecodeFrom64(string encodedData)
		{
			var encodedDataAsBytes = System.Convert.FromBase64String(encodedData);
			string returnValue = System.Text.UTF8Encoding.UTF8.GetString(encodedDataAsBytes);
			return returnValue;
		}

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("7B3C2AD9-F1C3-42CC-8F3A-AD292ABF5CA8");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			object val = null;
			Func<string, bool> getVal = p => { try { propertyBag.Read(p, out val, errorLog); } catch { } return val != null; };

			if (getVal("Enabled")) Enabled = Convert.ToBoolean(val);
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val;
			val = Enabled; propertyBag.Write("Enabled", ref val);
		}

		internal virtual TransactionScope GetTransactionScope(IPipelineContext context)
		{
			var transactionNative = (IDtcTransaction)((IPipelineContextEx)context).GetTransaction();
			return new TransactionScope(TransactionInterop.GetTransactionFromDtcTransaction(transactionNative));
		}

		#endregion

		#region Properties

		public bool Enabled { get; set; }

		#endregion
	}
}

