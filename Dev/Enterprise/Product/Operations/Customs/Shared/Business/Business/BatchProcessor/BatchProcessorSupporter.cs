namespace Enterprise.Customs.Business.BatchProcessor
{
	using System;
	using System.Text;
	using CargoWise.Types;

	public static class BatchProcessorSupporter
	{
		public static ZString DecodeMIMEText(string mIMEText)
		{
			string mIMETextUpper = mIMEText.ToUpper();
			ZString result;
			ZString mIMEBody = mIMEText.Substring(mIMEText.IndexOf("\r\n\r\n") + 4);
			if (mIMETextUpper.IndexOf("QUOTED-PRINTABLE") != -1)
			{
				result = new CryptoUtilities.SMIME.QuotedPrintableDecoder().Decode(mIMEBody);
			}
			else if ((mIMETextUpper.IndexOf("APPLICATION/TEXT") != -1) ||
				mIMETextUpper.IndexOf("CONTENT-TYPE: TEXT/PLAIN") != -1)
			{
				result = mIMEBody;
			}
			else
			{
				try
				{
					result = Encoding.ASCII.GetString(Convert.FromBase64String(mIMEBody));
				}
				catch (Exception ex) when (ex is ArgumentException || ex is FormatException)
				{
					result = mIMEBody;
				}
			}

			return result;
		}
	}
}
