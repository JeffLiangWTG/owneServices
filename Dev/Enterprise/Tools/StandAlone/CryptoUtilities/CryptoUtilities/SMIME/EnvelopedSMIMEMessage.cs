namespace Enterprise.CryptoUtilities.SMIME
{
	public class EnvelopedSMIMEMessage : MIMEMessage
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "internal purpose string")]
		public EnvelopedSMIMEMessage(MIMEMessage InnerMIMEMessage, Certificate EncryptionCertificate)
		{
			this.InnerMIMEMessage = InnerMIMEMessage;
			this.EncryptionCertificate = EncryptionCertificate;
			SetTag("Content-Transfer-Encoding", "base64");
			SetTag("Content-Type", "application/pkcs7-mime; smime-type=enveloped-data; name = \"smime.p7m\"");
		}

		#region Implemenetation
		protected override string EncodedContents
		{
			get
			{
				if (fEncodedContents == null)
				{
					fEncodedContents = EncryptionCertificate.Encrypt(InnerMIMEMessage.RawMIMEString);
				}
				return fEncodedContents;
			}
		}

		protected Certificate EncryptionCertificate;
		protected MIMEMessage InnerMIMEMessage;
		#endregion
	}
}
