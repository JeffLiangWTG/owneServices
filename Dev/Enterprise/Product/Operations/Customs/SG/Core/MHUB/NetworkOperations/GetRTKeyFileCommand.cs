using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.MHUB;

namespace Enterprise.Customs.SG.V4.MHUB
{
	/// <summary>
	/// Fires the HTTP request with query string parameter "Command=getRTkey", which will download an RSA key file.  This is later used to log in via SFTP.
	/// </summary>
	public class GetRTKeyFileCommand : MHUBWebCommand
	{
		public GetRTKeyFileCommand(LoginDetails loginDetails, IMHUBSettings settingsProvider, LoggingInformation logger, bool verboseLogging, LoginCommand loginCommandForCookie)
			: base(loginDetails.EdiServlet, settingsProvider, logger, verboseLogging)
		{
			base.sessionCookie = loginCommandForCookie.SessionCookie;
		}

		public ZString KeyFromResponse
		{
			get
			{
				string decodedString = null;
				try
				{
					if (!RawResponseString.IsEmpty)
					{
						var b64Data = ExtractRSATAGContentFromHtmlCumXml(RawResponseString);
						byte[] data = Convert.FromBase64String(b64Data);
						decodedString = Encoding.UTF8.GetString(data);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Logger.LogWarning("Could not download or parse RSA key. Will try to use standard WTG key instead. " + ex.Message);
				}
				return decodedString;
			}
		}

		internal static string ExtractRSATAGContentFromHtmlCumXml(ZString xmlData, string tag = "RSAKEY")
		{
			// The web server sends an entire XML document INSIDE the body tag of an entire HTML doc.  Pffff.
			String startTag = "<" + tag + ">";
			String endTag = "</" + tag + ">";
			int startIndex = xmlData.IndexOf(startTag);
			if (startIndex == -1)
			{
				return null;
			}
			int endIndex = xmlData.IndexOf(endTag);
			if (endIndex == -1)
			{
				return null;
			}
			return xmlData.Substring(startIndex + startTag.Length, endIndex - startIndex - endTag.Length + 1);
		}

		protected override MHUBConstants.CommandType CommandToExecute
		{
			get { return MHUBConstants.CommandType.getRTkey; }
		}

		protected override Dictionary<String, object> InputParameterList
		{
			get { return new Dictionary<String, object>(); }
		}
	}
}
