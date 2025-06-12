using CargoWise.eHub.BizTalkAdapters.Common;
using Common.Logging;
using Microsoft.BizTalk.Message.Interop;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace CargoWise.eHub.BizTalkAdapters.WSHttpEx
{
	public class WSHttpExRequestFactory : IWebRequestFactory
	{
		public WebRequest CreateRequest(IBaseMessage message, WSHttpExProperties config)
		{
			var request = (HttpWebRequest)WebRequest.Create(config.Uri);

			request.AllowAutoRedirect = true;
			request.Method = "POST";
			request.Timeout = config.Timeout;
			request.AllowWriteStreamBuffering = false;
			request.SendChunked = true;
			request.PreAuthenticate = true;
			var contentType = config.ContentType;
			if (contentType.ToLowerInvariant().StartsWith("text/"))
			{
				var charset = message.BodyPart.Charset;
				if (charset != null && charset.Length > 0 && contentType.ToLowerInvariant().IndexOf("charset=") == -1)
					contentType += "; charset=" + charset;
			}
			request.ContentType = contentType;

			if (config.Certificate != null)
			{
				TransferrerHelpers.Log(this, config.Logger, LogLevel.Info, "Importing client certificate for Message ID = '{0}', Endpoint Hash Code = {1}", message.MessageID, GetHashCode());

				var certificates = new X509Certificate2Collection();
				certificates.Import(config.Certificate, config.CertificatePassPhrase, X509KeyStorageFlags.DefaultKeySet);
				request.ClientCertificates = certificates;
			}
			else
			{
				TransferrerHelpers.Log(this, config.Logger, LogLevel.Info, "No client certificate was specified for Message ID = '{0}', Endpoint Hash Code = {1}.  Proceeding with sending the request without a client certificate.", message.MessageID, GetHashCode());
			}

			if (!string.IsNullOrEmpty(config.SoapAction))
			{
				request.Headers.Add("SOAPAction", config.SoapAction);
			}

			if (config.SecurityMode != null)
			{
				if (config.SecurityMode == "Transport")
				{
					request.Credentials = new NetworkCredential(config.UserName, config.Password);
				}
			}

			request.ServerCertificateValidationCallback +=
				(sender, cert, chain, error) =>
				{
					if (error == SslPolicyErrors.None)
					{
						return true;
					}

					TransferrerHelpers.Log(this, config.Logger, LogLevel.Warn, "Certificate policy error {0} was received when sending Message ID = '{1}', Endpoint Hash Code = {2}.", error.ToString(), message.MessageID, GetHashCode());
					return config.IgnoreServerCertErrors;
				};

			return request;
		}

		public WebRequest PreAuthenticateRequest(IBaseMessage message, WSHttpExProperties config)
		{
			var request = (HttpWebRequest)HttpWebRequest.Create(config.Uri);

			request.Credentials = new NetworkCredential(config.UserName, config.Password);
			request.PreAuthenticate = true;
			request.UserAgent = "Post Test";
			request.Method = "HEAD";
			request.Timeout = config.Timeout;
			return request;
		}
	}
}
