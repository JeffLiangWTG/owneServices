using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Products.Core.PropertySchemas;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Products.Core.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[Guid("DB9C8DF8-B1C1-40EF-8EB0-3B791680DE23")]
	public class SoapHeadersPromoter : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		const string PropertiesToPromoteKey = "http://schemas.microsoft.com/BizTalk/2006/01/Adapters/WCF-properties/Promote";
		const string PropertiesToWriteKey = "http://schemas.microsoft.com/BizTalk/2006/01/Adapters/WCF-properties/WriteToContext";

		#region IBaseComponent Members

		public string Description
		{
			get { return "Product: Promote all soap headers into message context"; }
		}

		public string Name
		{
			get { return "Product: Soap Headers Promoter"; }
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

			if (String.IsNullOrEmpty(this.PropertySchemaNamespace))
				throw new ApplicationException("PropertySchemaNamespace must be specified.");

			if (String.IsNullOrEmpty(this.HeaderSchemaNamespace))
				throw new ApplicationException("HeaderSchemaNamespace must be specified.");

			PromoteSOAPHeaders(message);

			return message;
		}

		void PromoteSOAPHeaders(IBaseMessage message)
		{
			object inboundHeaders = message.Context.Read("InboundHeaders", "http://schemas.microsoft.com/BizTalk/2006/01/Adapters/WCF-properties");
			if (inboundHeaders == null) throw new ApplicationException("There is not  must InboundHeaders disable this component.");
			string headerXml = inboundHeaders.ToString();

			var element = XElement.Parse(headerXml);
			var headers = element.XPathSelectElements(String.Format("//*[namespace-uri()='{0}']", HeaderSchemaNamespace));

			foreach (var header in headers)
			{
				switch (header.Name.LocalName)
				{
					case "SenderID": message.Context.PromoteProperty<SenderID>(header.Value);
						message.Context.PromoteProperty<BTS.SourceParty>(header.Value);
						break;
					case "MessageTrackingID": message.Context.PromoteProperty<MessageTrackingID>(header.Value);
						message.Context.PromoteProperty<CargoWise.eHub.Core.PropertySchemas.MessageTrackingID>(header.Value);
						break;
					case "ClientID": message.Context.PromoteProperty<ClientID>(header.Value);
						message.Context.PromoteProperty<BTS.DestinationParty>(header.Value);
						break;
					case "SchemaName": message.Context.PromoteProperty<SchemaName>(header.Value); break;
					case "SchemaType": message.Context.PromoteProperty<SchemaType>(header.Value); break;
					case "ApplicationCode": message.Context.PromoteProperty<ApplicationCode>(header.Value); break;
					case "EmailSubject": message.Context.PromoteProperty<EmailSubject>(header.Value); break;
					case "FileName": message.Context.PromoteProperty<FileName>(header.Value); break;
					case "MessageMode": message.Context.PromoteProperty<MessageMode>(header.Value); break;
				}
			}
		}

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("DB9C8DF8-B1C1-40EF-8EB0-3B791680DE23");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			object var = null;
			try { propertyBag.Read("PropertySchemaNamespace", out var, errorLog); }
			catch { }
			if (var != null) PropertySchemaNamespace = (string)var;

			var = null;
			try { propertyBag.Read("HeaderSchemaNamespace", out var, errorLog); }
			catch { }
			if (var != null) HeaderSchemaNamespace = (string)var;

			var = null;
			try { propertyBag.Read("Enabled", out var, errorLog); }
			catch { }
			if (var != null) Enabled = (bool)var;
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val = HeaderSchemaNamespace;
			propertyBag.Write("HeaderSchemaNamespace", ref val);

			val = PropertySchemaNamespace;
			propertyBag.Write("PropertySchemaNamespace", ref val);

			val = Enabled;
			propertyBag.Write("Enabled", ref val);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Is the component active on the pipeline
		/// </summary>
		public bool Enabled { get; set; }

		/// <summary>
		/// PropertySchemaNamespace
		/// </summary>
		public string HeaderSchemaNamespace { get; set; }

		public string PropertySchemaNamespace { get; set; }

		#endregion

	}
}
