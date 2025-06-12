using System;
using System.IO;

namespace CargoWise.eHub.Products.NZCustoms.SoapWithAttachments.Mime
{
	public class MimePart
	{
		public string ContentType { get; private set; }
		public string TransferEncoding { get; private set; }
		public string ContentId { get; private set; }
		public string CharSet { get; private set; }
		public const string SoapPartContentType = "text/xml";
		public const string DefaultStartPartContentId = "eec9964d17ca4d9c8e97f142910f0e8dnode1";

		public byte[] Content
		{
			get { return content; }
			set { content = value; }
		}
		byte[] content;

		public MimePart()
			: this(SoapPartContentType, "binary", DefaultStartPartContentId, "UTF-8")
		{
		}

		public MimePart(string contentType, string contentId)
			: this(contentType, "binary", contentId, string.Empty)
		{
		}

		public MimePart(string contentType, string transferEncoding, string attachmentId)
			: this(contentType, transferEncoding, attachmentId, string.Empty)
		{
		}


		public MimePart(string contentType, string transferEncoding, string contentId, string charSet)
		{
			ContentType = contentType;
			TransferEncoding = transferEncoding;
			ContentId = contentId;
			CharSet = charSet;
		}

		public string GetContentAsString()
		{
			var stream = GetContentAsStream();

			if (stream != null)
			{
				using (var reader = new StreamReader(GetContentAsStream()))
				{
					return reader.ReadToEnd();
				}
			}
			return string.Empty;
		}

		public Stream GetContentAsStream()
		{
			if (content != null)
			{
				return new MemoryStream(content);
			}

			return null;
		}
	}
}
