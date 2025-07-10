using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.IO;
using CargoWise.Shared;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	static class MessagingExtensions
	{
		public static string WrapInInterchange(this string universalXml, string recipientId = null)
		{
			if (string.IsNullOrWhiteSpace(universalXml))
			{
				return null;
			}

			var senderId = Env.CurrentCompany.GetLicenceCode();

			var payloadXml = XElement.Parse(universalXml);

			XNamespace ns = UniversalXmlInfo.Namespace_2011_11;

			#region SuppressResourceStringsCheckRegion

			var header = string.IsNullOrWhiteSpace(recipientId)
				? new XElement(ns + "Header",
					new XElement(ns + "SenderID", senderId))
				: new XElement(ns + "Header",
					new XElement(ns + "SenderID", senderId),
					new XElement(ns + "RecipientID", recipientId));

			var interchange = new XElement(ns + "UniversalInterchange",
				header,
				new XElement(ns + "Body", payloadXml));

			#endregion

			return interchange.ToString();
		}

		public static ITopLevelDataObject PopulateDataContext(this ITopLevelDataObject uxmlDataObject, string documentName)
		{
			if (uxmlDataObject?.DataContext is UniversalDataBuss.DataObjects.Universal._2012_11.DataContext dataContext)
			{
				uxmlDataObject.DataContext.SetDocumentaryOverride(documentName, MessagePurposes.Codes.Original, null, true, 1, 1);

				if (dataContext.Workflow == null)
				{
					dataContext.Workflow = new UniversalDataBuss.DataObjects.Universal._2012_11.Workflow();
				}

				dataContext.Workflow.EventUser = new Staff { Code = GlbStaff.CurrentUser.GS_Code, Name = GlbStaff.CurrentUser.GS_FullName };
				dataContext.Workflow.EventBranch = new Branch { Code = GlbBranch.CurrentBranch.GB_Code, Name = GlbBranch.CurrentBranch.GB_BranchName };
				dataContext.Workflow.EventDepartment = new Department { Code = GlbDepartment.CurrentDepartment.GE_Code, Name = GlbDepartment.CurrentDepartment.HumanReadableName };
			}

			return uxmlDataObject;
		}

		public static ITopLevelDataObject ToUniversalXmlDataObject(this DocumentVisualizer.Core.IDocument document, MessageType messageType = MessageType.Unspecified)
		{
			if (document == null
				|| string.IsNullOrWhiteSpace(document.DataContext))
			{
				return null;
			}

			var writer = ObjectFactory.Get<IForwardingDocDataObjectUXmlWriter>();
			return writer.GetDataObject(DefaultDataObjectWriterStrategy.Instance, document, messageType);
		}

		public static string ToUniversalXml(this ITopLevelDataObject uxmlDataObject, string customNamespace = null)
		{
			if (uxmlDataObject == null)
			{
				return string.Empty;
			}

			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				ObjectFactory.Get<IXmlWriter>().WriteXML(uxmlDataObject, stream, UniversalXmlSchema.Version_2012_11_DO_NOT_USE.Namespace);

				using (var reader = new StreamReader(stream))
				{
					return reader.ReadToEnd().ReplaceNamespace(customNamespace);
				}
			}
		}

		public static string ToUniversalXml(this DocumentVisualizer.Core.IDocument document, string customNamespace = null, MessageType messageType = MessageType.Unspecified)
		{
			return document
				?.ToUniversalXmlDataObject(messageType)
				?.ToUniversalXml(customNamespace);
		}

		public static string ReplaceNamespace(this string xml, string customNamespace)
		{
			if (string.IsNullOrWhiteSpace(xml)
				|| string.IsNullOrWhiteSpace(customNamespace))
			{
				return xml;
			}

			var escapedUri = StringUtilities.EscapeUriStringWithQuery(customNamespace);

			string ReplaceNamespace(UriKind uriKind)
			{
				var xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(xml);

				var ns = uriKind == UriKind.Relative
					? string.Concat(xmlDoc.DocumentElement.NamespaceURI, escapedUri)
					: escapedUri;

				xmlDoc.DocumentElement.SetAttribute((NoResString)"xmlns", ns); // programmatic constant

				using (var stringWriter = new StringWriter())
				using (var textWriter = new XmlTextWriter(stringWriter))
				{
					textWriter.Formatting = Formatting.Indented;
					xmlDoc.WriteTo(textWriter);
					return stringWriter.ToString();
				}
			}

			if (Uri.IsWellFormedUriString(customNamespace, UriKind.Relative))
			{
				xml = ReplaceNamespace(UriKind.Relative);
			}
			else if (Uri.IsWellFormedUriString(escapedUri, UriKind.Absolute))
			{
				xml = ReplaceNamespace(UriKind.Absolute);
			}

			return xml;
		}
	}
}
