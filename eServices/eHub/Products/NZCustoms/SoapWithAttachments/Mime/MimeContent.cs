using System;
using System.Collections.Generic;
using System.Text;

namespace CargoWise.eHub.Products.NZCustoms.SoapWithAttachments.Mime
{
	public class MimeContent
	{
		public const string MediaType = "multipart/related";
		readonly List<MimePart> parts;
		public MimePart StartPart { get; private set; }
		public const string DefaultBoundary = "f7b0ea2f7b054020b64588e65f0aa23bCARGO";
		public string Boundary { get; private set; }

		MimeContent() : this(DefaultBoundary)
		{

		}

		public MimeContent(string boundary)
		{
			parts = new List<MimePart>();
			Boundary = boundary;
		}

		public void AddPart(MimePart part)
		{
			if (part == null) throw new ArgumentException("AddPart: MimePart is null");

			if (parts.Count == 0)
			{
				StartPart = part;
			}

			parts.Add(part);
		}

		public IEnumerable<MimePart> Parts
		{
			get
			{
				return parts;
			}
		}

		public static string ContentType
		{
			get
			{
				return string.Format("{0}; type=\"{1}\"; start=\"{2}\"; boundary=\"{3}\"", MediaType, MimePart.SoapPartContentType, MimePart.DefaultStartPartContentId, DefaultBoundary);
			}
		}

		public static MimeContent CreateWithSoapPart()
		{
			var soapMimeContent = new MimePart();
			var myContent = new MimeContent();
			myContent.AddPart(soapMimeContent);
			return myContent;
		}

		internal void ClearPartsExceptStartOne()
		{
			parts.Clear();
			parts.Add(StartPart);
		}

		public override string ToString()
		{
			var builder = new StringBuilder();
			builder.AppendFormat("Parts count {0}", parts.Count);
			for(int i = 0; i < parts.Count; i++)
			{
				builder.AppendFormat(" Part {0}, ContentId {1}.", i, parts[i].ContentId);
			}

			return builder.ToString();
		}
	}
}
