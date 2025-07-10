using System;
using System.Collections;
using System.Text;

namespace Enterprise.CryptoUtilities.SMIME
{
	public class SingleAttachmentMIMEMessage : MIMEMessage
	{
		public SingleAttachmentMIMEMessage(string AttachmentName, byte[] Bytes, string ContentType, string ContentDisposition)
		{
			this.AttachmentName = AttachmentName;
			this.Bytes = Bytes;

			SetTag("Content-Transfer-Encoding", "base64");

			SetTag("Content-Type", ContentType);
			SetTag("Content-Disposition", ContentDisposition);

		}

		#region Implementation
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "internal purpose string")]
		protected override string EncodedContents
		{
			get
			{
				if (fEncodedContents == null)
				{
					String EncodingType = "";
					IDictionaryEnumerator TagEnumeration = MIMETags.GetEnumerator();
					for (int i = 0; i < MIMETags.Count; i++)
					{
						TagEnumeration.MoveNext();
						if (((string)TagEnumeration.Key).ToLower().Equals("content-transfer-encoding"))
						{
							EncodingType = ((string)TagEnumeration.Value).ToLower();
							break;
						}
					}
					if (EncodingType.Length == 0)
					{
						throw new Exception("No Content-Transfer-Encoding Specified");
					}
					switch (EncodingType)
					{
						case "base64":
							string ContentsString = Enterprise.CryptoUtilities.Functions.ToBase64String(Bytes);
							fEncodedContents = ContentsString;
							break;
						case "7bit":
							ASCIIEncoding MyASCIIEncoding = new ASCIIEncoding();
							fEncodedContents = MyASCIIEncoding.GetString(Bytes, 0, Bytes.Length);
							break;
						default:
							throw new Exception("No Valid Content-Transfer-Encoding Specified");
					}

				}
				return fEncodedContents;
			}
		}

		protected byte[] Bytes;
		protected byte[] OutputBytes;
		#endregion
	}
}
