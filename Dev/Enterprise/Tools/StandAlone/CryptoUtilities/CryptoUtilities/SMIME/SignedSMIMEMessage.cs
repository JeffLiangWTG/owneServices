namespace Enterprise.CryptoUtilities.SMIME
{
	public class SignedSMIMEMessage : MIMEMessage
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "internal purpose string")]
		public SignedSMIMEMessage(MIMEMessage InnerMIMEMessage, Store SigningCertificateStore)
		{
			this.InnerMIMEMessage = InnerMIMEMessage;
			this.SigningCertificateStore = SigningCertificateStore;
			SetTag("Content-Transfer-Encoding", "base64");
			SetTag("Content-Type", "application/pkcs7-mime; smime-type=signed-data; name = \"smime.p7m\"");
		}

		#region Implemenetation
		protected override string EncodedContents
		{
			get
			{
				if (fEncodedContents == null)
				{
					fEncodedContents = SigningCertificateStore.Sign(InnerMIMEMessage.RawMIMEString);
				}
				return fEncodedContents;
			}
		}

		protected Store SigningCertificateStore;
		protected MIMEMessage InnerMIMEMessage;
		#endregion
	}
}
