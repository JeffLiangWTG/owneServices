using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.TrustedMessaging.Business;
using Enterprise.TrustedMessaging.Business.CreditCheck;
using Enterprise.TrustedMessaging.Business.SecuredHttp;
using Enterprise.ZArchitecture.Core;
using WTG.CreditCheck;

namespace Enterprise.MasterFiles.Business
{
	public class CreditCheckServiceWrapper : ISupportCreditCheckService
	{
		public bool IsProductionSystem => Env.Instance.IsProductionSystem || OrganisationsDataRegistry.Instance.EnableRealCreditCheckServiceInTestingSystem.Value;

		public void ReportDeveloperException(string message, Exception ex)
		{
			ExceptionReporter.Instance.ReportDeveloperException(message, ex);
		}

		public IEnumerable<(string Address, bool IsMain)> EndpointAddresses => OrganisationsDataRegistry.Instance.CreditCheckServiceURLs.Value.Cast<CodeDescriptionBool>().Select(x => (x.Description.ToString(), (bool)x.Bool));

		public HttpClientHandler GetHttpClientHandler(string baseUrl)
		{
			return new SecuredHttpMessageHandler(
				new SecuredHttpTrustedClientConfiguration(
					baseUrl,
					new CCSCertificatePairProvider(new CertificateAuthorityClient(), "CW1", ObjectFactory.Get<IProductRegistration>().Key.DatabaseNumber.ToString(CultureInfo.CurrentCulture)),
					new CCSKeyStorage()
				));
		}
	}
}
