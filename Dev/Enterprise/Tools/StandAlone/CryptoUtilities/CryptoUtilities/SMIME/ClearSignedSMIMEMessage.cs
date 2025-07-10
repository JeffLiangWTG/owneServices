namespace Enterprise.CryptoUtilities.SMIME
{
	public class ClearSignedSMIMEMessage : MIMEMessage
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "internal purpose string")]
		public ClearSignedSMIMEMessage(string EmailBody, Store SigningStore)
		{
			this.EmailBody = EmailBody;
			this.SigningStore = SigningStore;
			SetTag("Content-Type", "multipart/signed; protocol=\"application/pkcs7-signature\"; micalg=sha1;boundary=\"" + BoundaryText);
		}

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "internal purpose string")]
		const string BoundaryText = "#BOUNDARY#1";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "internal purpose string")]
		protected override string EncodedContents
		{
			get
			{
				if (fEncodedContents == null)
				{
					string Boundary = "--" + BoundaryText + "\r\n";
					string Part1Contents = "Content-Type: application/text" + "\r\n\r\n" + EmailBody + "\r\n";
					string Part2Contents = "Content-Type: application/pkcs7-signature; micalg=sha1\r\nContent-Transfer-Encoding: base64"
						+ "\r\n\r\n"
						+ SigningStore.ClearSign(Part1Contents);
					fEncodedContents =
						Boundary
						+ Part1Contents
						+ "\r\n"
						+ Boundary
						+ Part2Contents;
				}
				return fEncodedContents;
			}
		}

		readonly Store SigningStore;
		readonly string EmailBody;

		#endregion
	}
}
