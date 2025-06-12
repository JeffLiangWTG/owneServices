using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Common.Logging;

namespace CargoWise.eHub.Products.NZCustoms.SoapWithAttachments.Mime
{
	public class MimeParser
	{
		public Encoding ParserEncoding { get; set; }
		public Encoding SoapXmlEncoding { get; set; }
		internal ILog logger = LogManager.GetLogger(typeof(MimeParser));

		public MimeParser()
		{
			ParserEncoding = Encoding.UTF8;
			SoapXmlEncoding = Encoding.UTF8;
		}

		public MimeParser(Encoding encoding)
			: this()
		{
			ParserEncoding = encoding;
		}

		public MimeParser(Encoding encoding, Encoding soapEncoding)
			: this(encoding)
		{
			SoapXmlEncoding = soapEncoding;
		}

		public byte[] SerializeMimeContent(MimeContent content)
		{
			var contentStream = new MemoryStream();
			SerializeMimeContent(content, contentStream);
			return contentStream.ToArray();
		}

		public void SerializeMimeContent(MimeContent content, Stream contentStream)
		{
			byte[] carriageReturnLineFeed = new[] { (byte)'\r', (byte)'\n' };

			//
			// Prepare some bytes written more than once
			//
			byte[] boundaryBytes = ParserEncoding.GetBytes("--" + content.Boundary);

			//
			// Write every part into the stream
			//
			foreach (var item in content.Parts)
			{
				//
				// First of all write the boundary
				//
				contentStream.Write(carriageReturnLineFeed, 0, carriageReturnLineFeed.Length);
				contentStream.Write(boundaryBytes, 0, boundaryBytes.Length);
				contentStream.Write(carriageReturnLineFeed, 0, 2);

				//
				// Write the content-type for the current element
				//
				StringBuilder builder = new StringBuilder();
				builder.Append(string.Format("Content-Type: {0}", item.ContentType));
				if (!string.IsNullOrEmpty(item.CharSet)) builder.Append(string.Format("; charset={0}", item.CharSet));
				builder.Append(new[] { '\r', '\n' });
				builder.Append(string.Format("Content-Transfer-Encoding: {0}", item.TransferEncoding));
				builder.Append(new[] { '\r', '\n' });
				builder.Append(string.Format("Content-Id: {0}", item.ContentId));

				byte[] writeHelper = ParserEncoding.GetBytes(builder.ToString());
				contentStream.Write(writeHelper, 0, writeHelper.Length);
				contentStream.Write(carriageReturnLineFeed, 0, carriageReturnLineFeed.Length);
				contentStream.Write(carriageReturnLineFeed, 0, carriageReturnLineFeed.Length);

				//
				// Write the actual content
				//
				contentStream.Write(item.Content, 0, item.Content.Length);
			}

			//
			// Write one last content boundary
			//
			contentStream.Write(carriageReturnLineFeed, 0, carriageReturnLineFeed.Length);
			contentStream.Write(boundaryBytes, 0, boundaryBytes.Length);
			contentStream.Write(new byte[] { 45, 45 }, 0, 2);
			contentStream.Write(carriageReturnLineFeed, 0, carriageReturnLineFeed.Length);
		}

		public MimeContent DeserializeMimeContent(string httpContentType, byte[] binaryContent)
		{
			string mimeType = null, mimeBoundary = null, mimeStart = null;
			ParseHttpContentTypeHeader(httpContentType, ref mimeType, ref mimeBoundary, ref mimeStart);

			MimeContent content = new MimeContent(mimeBoundary);

			// 
			// Start finding the parts in the mime message
			// Note: in MIME RFC a "--" represents the end of something
			//
			byte[] mimeBoundaryBytes = ParserEncoding.GetBytes("--" + mimeBoundary);
			for (int i = 0; i < binaryContent.Length; i++)
			{
				if (AreArrayPartsForTextEqual(mimeBoundaryBytes, 0, binaryContent, i, mimeBoundaryBytes.Length))
				{
					int endBoundaryHelperIdx = i + mimeBoundaryBytes.Length;
					if ((endBoundaryHelperIdx + 1) < binaryContent.Length)
					{
						// The end of the MIME-message is the boundary followed by "--"
						if (binaryContent[endBoundaryHelperIdx] == '-' && binaryContent[endBoundaryHelperIdx + 1] == '-')
						{
							break;
						}
					}
					else
					{
						throw new ApplicationException("Invalid MIME content parsed, premature End-Of-File detected!");
					}
					// Start reading the mime part after the boundary
					MimePart part = ReadMimePart(binaryContent, ref i, mimeBoundaryBytes);
					if (part != null)
					{
						content.AddPart(part);
					}
				}
			}

			//
			// Finally return the ready-to-use object model
			//
			return content;
		}

		#region Implementation

		static void ParseHttpContentTypeHeader(string httpContentType, ref string mimeType, ref string mimeBoundary, ref string mimeStart)
		{
			string[] contentTypeParsed = httpContentType.Split(new[] { ';' });
			foreach (string t in contentTypeParsed)
			{
				string contentTypePart = t.Trim();
				int equalsIdx = contentTypePart.IndexOf('=');
				if (equalsIdx > 0)
				{
					string key = contentTypePart.Substring(0, equalsIdx);
					string value = contentTypePart.Substring(equalsIdx + 1);
					if (value[0] == '\"') value = value.Remove(0, 1);
					if (value[value.Length - 1] == '\"') value = value.Remove(value.Length - 1);

					switch (key.ToLower())
					{
						case "type":
							mimeType = value;
							break;
						case "start":
							mimeStart = value;
							break;
						case "boundary":
							mimeBoundary = value;
							break;
					}
				}
			}

			if (mimeStart == null) mimeStart = "soap.xml";
			if (mimeType == null) mimeType = MimePart.SoapPartContentType;
			if (mimeBoundary == null)
			{
				throw new ApplicationException("Invalid HTTP content header - please verify if type, start and boundary are available in the multipart/related content type header!");
			}
		}

		MimePart ReadMimePart(byte[] binaryContent, ref int currentIndex, byte[] mimeBoundaryBytes)
		{
			int startPartIndex = currentIndex;

			while (currentIndex < binaryContent.Length)
			{
				if (binaryContent[currentIndex] == 13 && binaryContent[currentIndex + 1] == 10 && binaryContent[currentIndex + 2] == 13 && binaryContent[currentIndex + 3] == 10)
				{
					break;
				}

				currentIndex++;
			}

			// After the last content header, we have \r\n\r\n, always
			currentIndex += 4;

			string headerText = ParserEncoding.GetString(binaryContent, startPartIndex, currentIndex - startPartIndex).Trim();

			var headers = ExtractHeaders(headerText);


			if (!(headers.ContainsKey(contentIdKey) && headers.ContainsKey(contentTypeKey)))
			{
				throw new ApplicationException("Invalid mime content passed into mime parser! Content-Type, ContentId headers for mime part are missing!");
			}

			string contentId = headers[contentIdKey];
			string contentTypeText = headers[contentTypeKey];
			var contentTypeFields = ParseContentType(contentTypeText);
			string contentType = contentTypeFields[contentTypeKey];
			string charSet = string.Empty;

			if (contentTypeFields.ContainsKey(charSetKey))
			{
				charSet = contentTypeFields[charSetKey];
			}

			//
			// Current mime content starts now, therefore find the end
			//
			int startContentIndex = currentIndex;
			int endContentIndex = -1;
			while (currentIndex < binaryContent.Length)
			{
				if (AreArrayPartsForTextEqual(mimeBoundaryBytes, 0, binaryContent, currentIndex, mimeBoundaryBytes.Length))
				{
					endContentIndex = currentIndex - 1;
					break;
				}
				currentIndex++;
			}
			if (endContentIndex == -1) endContentIndex = currentIndex - 1;

			// 
			// Tweak start- and end-indexes, cut all Carriage Return Line Feeds
			//
			while (true)
			{
				if ((binaryContent[startContentIndex] == 13) && (binaryContent[startContentIndex + 1] == 10))
					startContentIndex += 2;
				else
					break;

				if (startContentIndex > binaryContent.Length)
					throw new ApplicationException("Error in content, start index cannot go beyond overall content array!");
			}
			while (true)
			{
				if ((binaryContent[endContentIndex - 1] == 13) && (binaryContent[endContentIndex] == 10))
					endContentIndex -= 2;
				else
					break;

				if (endContentIndex < 0)
					throw new ApplicationException("Error in content, end content index cannot go beyond smallest index of content array!");
			}

			//
			// Now create a byte array for the current mime-part content
			//
			MimePart part = new MimePart(contentType, string.Empty, contentId, charSet);
			var contentLength = Math.Max(0, endContentIndex - startContentIndex + 1);
			part.Content = new byte[contentLength];

			if (contentLength > 0)
			{
				Array.Copy(binaryContent, startContentIndex, part.Content, 0, part.Content.Length);
			} 
			else
			{
				logger.ErrorFormat("Empty MimePart found, contentId: {0}, contentType: {1}, raw message: {2}", contentId, contentType, Encoding.UTF8.GetString(binaryContent));
			}

			// Go to the last sign before the next boundary starts
			currentIndex--;

			return part;
		}

		static Dictionary<string, string> ParseContentType(string contentTypeText)
		{
			var contentTypeFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			var contentTypeParts = contentTypeText.Split(new[] { "; " }, StringSplitOptions.None);

			if (contentTypeParts.Length > 0)
			{
				contentTypeFields.Add(contentTypeKey, contentTypeParts[0].Trim());

				for (int i = 1; i < contentTypeParts.Length; i++)
				{
					var parts = contentTypeParts[i].Split('=');

					if (parts.Length == 2)
					{
						contentTypeFields.Add(parts[0].Trim(), parts[1].Trim());
					}
				}
			}

			return contentTypeFields;
		}

		static Dictionary<string, string> ExtractHeaders(string headerText)
		{
			var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			var lines = headerText.Split(new[] { "\r\n" }, StringSplitOptions.None);

			foreach (var line in lines)
			{
				var parts = line.Split(new[] { ": " }, StringSplitOptions.None);
				if (parts.Length == 2)
				{
					headers.Add(parts[0].Trim(), parts[1].Trim());
				}
			}
			return headers;
		}

		static bool AreArrayPartsForTextEqual(byte[] firstArray, int firstOffset, byte[] secondArray, int secondOffset, int length)
		{
			// Check array boundaries
			if ((firstOffset + length) > firstArray.Length) return false;
			if ((secondOffset + length) > secondArray.Length) return false;

			// Run through the arrays and compare byte-by-byte
			for (int i = 0; i < length; i++)
			{
				char c1 = Char.ToLower((char)firstArray[firstOffset + i]);
				char c2 = char.ToLower((char)secondArray[secondOffset + i]);
				if (c1 != c2)
				{

					return false;
				}
			}

			return true;
		}

		const string contentIdKey = "content-id";
		const string contentTypeKey = "content-type";
		const string charSetKey = "charset";

		#endregion
	}
}
