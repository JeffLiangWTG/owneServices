using System;
#if NETCOREAPP
using System.Net.Http;
#endif
#if NETFRAMEWORK
using System.ServiceModel;
using System.ServiceModel.Channels;
#endif
using Enterprise.Customs.DataRegistry.Business;

namespace Enterprise.Customs.Business
{
	public class CusIntegrationService
	{
#if NETFRAMEWORK
		public BasicHttpBinding HttpBinding
		{
			get
			{
				if (httpBinding == null)
				{
					httpBinding = new BasicHttpBinding();
					httpBinding.SendTimeout = new TimeSpan(0, 0, TimeoutInSeconds);
				}
				return httpBinding;
			}
		}
		BasicHttpBinding httpBinding;

		public BasicHttpsBinding HttpsBinding
		{
			get
			{
				if (httpsBinding == null)
				{
					httpsBinding = new BasicHttpsBinding();
					httpsBinding.SendTimeout = new TimeSpan(0, 0, TimeoutInSeconds);
				}
				return httpsBinding;
			}
		}
		BasicHttpsBinding httpsBinding;

		public CustomBinding CustomBinding
		{
			get
			{
				if (customBinding == null)
				{
					customBinding = new CustomBinding();
					customBinding.SendTimeout = new TimeSpan(0, 0, TimeoutInSeconds);
				}
				return customBinding;
			}
		}
		CustomBinding customBinding;
#elif NETCOREAPP
		HttpClient httpClient;

		public HttpClient HttpBinding
		{
			get
			{
				if (httpClient == null)
				{
					httpClient = new HttpClient();
					httpClient.Timeout = new TimeSpan(0, 0, TimeoutInSeconds);
				}
				return httpClient;
			}
		}

		HttpClient httpsClient;

		public HttpClient HttpsBinding
		{
			get
			{
				if (httpsClient == null)
				{
					var handler = new HttpClientHandler
					{
						ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
					};
					httpsClient = new HttpClient(handler);
					httpsClient.Timeout = new TimeSpan(0, 0, TimeoutInSeconds);
				}
				return httpsClient;
			}
		}

		HttpClient customClient;

		public HttpClient CustomBinding
		{
			get
			{
				if (customClient == null)
				{
					var handler = new HttpClientHandler
					{
						ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
						{
							return errors == System.Net.Security.SslPolicyErrors.None;
						},
					};

					customClient = new HttpClient(handler);
					customClient.Timeout = new TimeSpan(0, 0, TimeoutInSeconds);
				}
				return customClient;
			}
		}
#endif

		int TimeoutInSeconds
		{
			get { return CustomsDataRegistry.Instance.WebServiceTimeoutInSeconds.Value; }
		}
	}
}
