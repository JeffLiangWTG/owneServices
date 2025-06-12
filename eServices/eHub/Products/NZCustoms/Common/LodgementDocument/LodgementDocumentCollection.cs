using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Common.Extensions;
using Common.Logging;

namespace CargoWise.eHub.Products.NZCustoms.Common
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Lodgement"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
	public static class LodgementDocumentCollection
	{
		const string ns = "http://cargowise.com/ehub/products/xmlwithattachments";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "LodgementDocumentCollection")]
		public static LodgementDocument[] Create(ILog logger, Stream messageStream)
		{
			var documents = ParseDocuments(messageStream);
			return documents.Length > 1 ? UpdateDocumentsFromDeclaration(logger, documents) : documents;
		}

		static LodgementDocument[] UpdateDocumentsFromDeclaration(ILog logger, LodgementDocument[] documents)
		{
			if (documents.Length <= 1) return documents;

			var declaration = documents.FirstOrDefault(d => d.DocumentType == Constants.DeclarationDocumentType);
			if (declaration == null) return documents;

			try
			{
				var xmlElement = XElement.Parse(new MemoryStream(declaration.Content).ReadToEnd());
				var additionalDocuments = xmlElement.XPathSelectElements("//*[local-name()='Declaration']/*[local-name()='AdditionalDocument']");

				foreach (var additionalDocument in additionalDocuments)
				{
					var imageBinaryObjectElement = additionalDocument.Elements().FirstOrDefault(e => e.Name.LocalName == "ImageBinaryObject");
					if (imageBinaryObjectElement == null) continue;
					var fileNameAttribute = imageBinaryObjectElement.Attributes().FirstOrDefault(a=>String.Equals(a.Name.LocalName, "filename", StringComparison.CurrentCultureIgnoreCase));
					if (fileNameAttribute == null || string.IsNullOrWhiteSpace(fileNameAttribute.Value)) continue;

					var foundDocument = documents.FirstOrDefault(d => d.FileName == fileNameAttribute.Value);
					if (foundDocument == null) continue;

					var categoryCodeElement = additionalDocument.Elements().FirstOrDefault(e => e.Name.LocalName == "CategoryCode");
					if (categoryCodeElement == null || string.IsNullOrWhiteSpace(categoryCodeElement.Value)) continue;

					var mimeCodeAttribute = imageBinaryObjectElement.Attributes().FirstOrDefault(a => String.Equals(a.Name.LocalName, "mimeCode", StringComparison.CurrentCultureIgnoreCase));
					if (mimeCodeAttribute == null || string.IsNullOrWhiteSpace(mimeCodeAttribute.Value)) continue;

					foundDocument.DocumentType = categoryCodeElement.Value;
					foundDocument.DocumentMediaType = mimeCodeAttribute.Value;
				}
			}
			catch (Exception ex)
			{
				logger.Warn("UpdateDocumentsFromDeclaration error", ex);
			}

			return documents;
		}

		static LodgementDocument[] ParseDocuments(Stream messageStream)
		{
			var documents = new List<LodgementDocument>();
			var filenames = new List<string>();

			try
			{
				var reader = XmlReader.Create(messageStream);
				while (reader.ReadToFollowing("Document", ns))
				{
					var document = new LodgementDocument();

					document.DocumentType = reader.GetElementAsString("DocumentType", ns);
					document.FileName = reader.GetElementAsString("FileName", ns);
					document.DocumentMediaType = ParseMediaType(document.FileName);

					if (string.IsNullOrWhiteSpace(document.FileName))
						throw new NZCustomsInvalidOperationException("Attachment document filename can't be empty. Please correct and resend the message.");
					if (filenames.Contains(document.FileName))
						throw new NZCustomsInvalidOperationException("Attachment document filename should be unique. Please correct and resend the message.");

					filenames.Add(document.FileName);
					document.Content = reader.GetElementDecodeAndDecompressAndReturnAsByteArray("Content", ns);
					documents.Add(document);
				}

				if (documents.Count == 0) documents.Add(CreateLegacyDocument(messageStream));
			}
			catch (XmlException)
			{
				documents.Add(CreateLegacyDocument(messageStream));
			}

			if (documents == null)
				throw new InvalidOperationException("LodgementDocumentCollection.Create: Can't recognize input message.");

			return documents.ToArray();
		}

		static LodgementDocument CreateLegacyDocument(Stream messageStream)
		{
			messageStream.Position = 0;
			var document = new LodgementDocument();
			document.DocumentType = Constants.DeclarationDocumentType;
			document.DocumentMediaType = Constants.LegacyDeclarationMediaType;
			document.FileName = Constants.DeclarationContentId;
			document.Content = ((MemoryStream)messageStream).ToArray();
			return document;
		}

		static string ParseMediaType(string fileName)
		{
			if (string.IsNullOrWhiteSpace(fileName)) return "text/xml";
			string fileExtension = Path.GetExtension(fileName).ToUpper();

			switch (fileExtension)
			{
				case ".XML":
					return "application/xml";
				case ".PDF":
					return "application/pdf";
				case ".DOC":
				case ".DOCX":
					return "application/msword";
				case ".CSV":
					return "text/csv";
				case ".XLS":
					return "application/vnd.ms-excel";
				case ".TIF":
					return "image/tiff";
				case ".GIF":
					return "image/gif";
				case ".PNG":
					return "image/png";
				case ".JPEG":
					return "image/jpeg";
				default:
					return "text/xml";
			}
		}
	}
}