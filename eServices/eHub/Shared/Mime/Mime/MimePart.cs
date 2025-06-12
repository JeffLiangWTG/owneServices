using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Text;
using CargoWise.eHub.Shared.Crypto;

namespace CargoWise.eHub.Shared.Mime
{
	public abstract class MimePart : IDisposable
	{
		public Dictionary<string, string> Headers { get; private set; }
		public Stream Raw { get; internal set; }
		public string MimeType { get; set; }

		MimePart(string mimeType)
		{
			this.Headers = new Dictionary<string, string>();
			this.MimeType = mimeType;
		}

		MimePart(MimeKit.MimeEntity mimeKitEntity)
		{
			this.Headers = mimeKitEntity.Headers.ToDictionary<MimeKit.Header, string, string>(h => h.Field, h => h.Value);
			this.Raw = new MemoryStream();
			mimeKitEntity.WriteTo(this.Raw);
			this.Raw.Position = 0;
			this.MimeType = mimeKitEntity.ContentType.MimeType;
		}

		public static MimePart Parse(Stream message)
		{
			var mimeKitEntity = new MimeKit.MimeParser(message, MimeKit.MimeFormat.Entity).ParseEntity();

			if (mimeKitEntity is MimeKit.Multipart)
				return new MimePart.Multipart(mimeKitEntity);
			else
				return new MimePart.Content(mimeKitEntity);
		}

		public Stream Format()
		{
			var mimeKitMessage = ConvertToMimeKit();
			var formattedMessage = new MemoryStream();
			mimeKitMessage.WriteTo(formattedMessage);
			formattedMessage.Position = 0;
			return formattedMessage;
		}

		public Stream FormatAsSMime(byte[] signingCert, string password)
		{
			var mimeKitMessage = ConvertToMimeKit();
			var smimeMessage = new MimeKit.Multipart("signed; protocol=\"application/pkcs7-signature\"; micalg=\"sha256\"");
			smimeMessage.Boundary = "_" + GetNewGuid().ToString().ToUpper() + "_";
			smimeMessage.Add(mimeKitMessage);
			var mdnSignaturePart = new MimeKit.MimePart("application", "pkcs7-signature; name=\"smime.p7s\"")
			{
				ContentTransferEncoding = MimeKit.ContentEncoding.Base64,
			};

			var sha256 = Oid.FromFriendlyName("SHA256", OidGroup.HashAlgorithm);
			var signatureData = CmsHelpers.ComputeSignature(Encoding.UTF8.GetBytes(mimeKitMessage.ToString()), signingCert, password, new List<Pkcs9AttributeObject>(), true, sha256);
			var signatureDataStream = new MemoryStream(signatureData);
			mdnSignaturePart.ContentObject = new MimeKit.ContentObject(signatureDataStream);
			smimeMessage.Add(mdnSignaturePart);
			return FormatMimeKit(smimeMessage);
		}

		static Stream FormatMimeKit(MimeKit.MimeEntity message)
		{
			var formattedMessage = new MemoryStream();
			message.WriteTo(formattedMessage);
			formattedMessage.Position = 0;
			return formattedMessage;
		}

		protected abstract MimeKit.MimeEntity ConvertToMimeKit();

		bool disposed = false;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
				return;

			if (disposing)
			{
				if (this.Raw != null)
				{
					this.Raw.Dispose();
					this.Raw = null;
					this.Headers = null;
				}
			}

			disposed = true;
		}

		public class Content : MimePart
		{
			public Stream Contents { get; set; }

			public Content(string mimeType) : base(mimeType)
			{
			}

			internal Content(MimeKit.MimeEntity mimeKitEntity) : base(mimeKitEntity)
			{
				this.Contents = new MemoryStream();
				var mimePart = mimeKitEntity as MimeKit.MimePart;
				if (mimePart != null && mimePart.ContentObject != null)
				{
					mimePart.ContentObject.Stream.CopyTo(this.Contents);
					this.Contents.Position = 0;
				}
			}

			protected override MimeKit.MimeEntity ConvertToMimeKit()
			{
				var mimeKitPart = new MimeKit.MimePart(this.MimeType);

				//MimeKit has default "Content-Type" header which impacts the sequence of the header format
				if (this.Headers.ContainsKey("Content-Type"))
					mimeKitPart.Headers.Clear();

				foreach (var header in this.Headers)
					mimeKitPart.Headers[header.Key] = header.Value;
				if (this.Contents != null)
					mimeKitPart.ContentObject = new MimeKit.ContentObject(this.Contents);
				return mimeKitPart;
			}

			protected override void Dispose(bool disposing)
			{
				if (this.Contents != null)
				{
					this.Contents.Dispose();
					this.Contents = null;
				}

				base.Dispose(disposing);
			}
		}

		public class Multipart : MimePart
		{
			public List<MimePart> Parts { get; private set; }
			public string Boundary { get; private set; }

			public Multipart(string subType, string boundary) : base(subType)
			{
				this.Parts = new List<MimePart>();
				this.Boundary = String.IsNullOrWhiteSpace(boundary) ? ("_" + GetNewGuid().ToString().ToUpper() + "_") : boundary;
			}

			internal Multipart(MimeKit.MimeEntity mimeKitEntity) : base(mimeKitEntity)
			{
				var multipart = mimeKitEntity as MimeKit.Multipart;
				this.Parts = multipart.Select(p =>
				{
					using (var partStream = new MemoryStream())
					{
						p.WriteTo(partStream);
						partStream.Position = 0;
						return MimePart.Parse(partStream);
					}
				}).ToList();
				this.Boundary = multipart.Boundary;
			}

			protected override MimeKit.MimeEntity ConvertToMimeKit()
			{
				var subtype = this.MimeType.Substring(this.MimeType.IndexOf('/') + 1);
				var mimeKitPart = new MimeKit.Multipart(subtype);

				//MimeKit has default "Content-Type" header which impacts the sequence of the header format
				if (this.Headers.ContainsKey("Content-Type"))
					mimeKitPart.Headers.Clear();

				foreach (var header in this.Headers)
					mimeKitPart.Headers[header.Key] = header.Value;
				foreach (var part in this.Parts)
					mimeKitPart.Add(part.ConvertToMimeKit());
				mimeKitPart.Boundary = this.Boundary;
				return mimeKitPart;
			}

			protected override void Dispose(bool disposing)
			{
				if (this.Parts != null)
				{
					this.Parts.ForEach(p => p.Dispose(disposing));
					this.Parts.Clear();
					this.Parts = null;
				}

				base.Dispose(disposing);
			}
		}

		internal static Func<Guid> GetNewGuid = () => Guid.NewGuid();
	}
}
