using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.FRReferenceData.Services.Exceptions;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class RITADataDownloader
	{
		public virtual string GetFRTradeGroupCountriesWebPage(string tradeGroupCode)
		{
			var requestPayload = string.Format(CultureInfo.InvariantCulture, "expertsReferenceConversation.typeRecherche=PAYS_REGION_GROUPE&expertsReferenceConversation.identifiant=&expertsReferenceConversation.motCle=&expertsReferenceConversation.code=&expertsReferenceConversation.appartenanceCodeAdd=&expertsReferenceConversation.typeCodeAdditionnel=&expertsReferenceConversation.appartenanceDoc=&expertsReferenceConversation.docDtp=&expertsReferenceConversation.typeDoc.code=&expertsReferenceConversation.flux=&expertsReferenceConversation.domaine=&expertsReferenceConversation.cible=&expertsReferenceConversation.typeRenvoi.code=&rechercheCompleteConversation.codeDomaine=&expertsReferenceConversation.typeZone=Z&expertsReferenceConversation.groupePays.code={0}%3AUE+%2B+pays+tiers+%2B+DOM&expertsReferenceConversation.typeCodeTaxe=&expertsReferenceConversation.fluxRegime=I&expertsReferenceConversation.regimeSollicite.code=&expertsReferenceConversation.regimePrecedent.code=&expertsReferenceConversation.regimeComplementaire.code=&expertsReferenceConversation.typeMesure.code=", tradeGroupCode);

			return RetryHelper.RetryWithDelay(
				() => { return GetWebPage(requestPayload, "https://www.douane.gouv.fr/rita-encyclopedie/public/experts/reference/ajaxRechercheComplete", "https://www.douane.gouv.fr/rita-encyclopedie/public/experts/reference/init.action"); },
				TimeSpan.FromMilliseconds(ApplicationConfig.Instance.MinDelayBetweenAttemptsInCaseOfDownloadError),
				ApplicationConfig.Instance.MaxAttemptsCountInCaseOfDownloadError);
		}

		public virtual string GetFRCountriesWebPage()
		{
			var requestPayload = string.Format(CultureInfo.InvariantCulture, "expertsReferenceConversation.typeRecherche=PAYS_REGION_GROUPE&expertsReferenceConversation.identifiant=&expertsReferenceConversation.motCle=&expertsReferenceConversation.code=&expertsReferenceConversation.appartenanceCodeAdd=&expertsReferenceConversation.typeCodeAdditionnel=&expertsReferenceConversation.appartenanceDoc=&expertsReferenceConversation.docDtp=&expertsReferenceConversation.typeDoc.code=&expertsReferenceConversation.flux=&expertsReferenceConversation.domaine=&expertsReferenceConversation.cible=&expertsReferenceConversation.typeRenvoi.code=&rechercheCompleteConversation.codeDomaine=&expertsReferenceConversation.typeZone=Z&expertsReferenceConversation.groupePays.code=&expertsReferenceConversation.typeCodeTaxe=&expertsReferenceConversation.fluxRegime=I&expertsReferenceConversation.regimeSollicite.code=&expertsReferenceConversation.regimePrecedent.code=&expertsReferenceConversation.regimeComplementaire.code=&expertsReferenceConversation.typeMesure.code=");
			
			return RetryHelper.RetryWithDelay(
				() => { return GetWebPage(requestPayload, "https://www.douane.gouv.fr/rita-encyclopedie/public/experts/reference/ajaxRechercheComplete", "https://www.douane.gouv.fr/rita-encyclopedie/public/experts/reference/init.action"); },
				TimeSpan.FromMilliseconds(ApplicationConfig.Instance.MinDelayBetweenAttemptsInCaseOfDownloadError),
				ApplicationConfig.Instance.MaxAttemptsCountInCaseOfDownloadError);
		}

		public virtual string GetFRTradeGroupsWebPage()
		{
			var requestPayload = "expertsReferenceConversation.typeRecherche=PAYS_REGION_GROUPE&expertsReferenceConversation.identifiant=&expertsReferenceConversation.motCle=&expertsReferenceConversation.code=&expertsReferenceConversation.appartenanceCodeAdd=&expertsReferenceConversation.typeCodeAdditionnel=&expertsReferenceConversation.appartenanceDoc=&expertsReferenceConversation.docDtp=&expertsReferenceConversation.typeDoc.code=&expertsReferenceConversation.flux=&expertsReferenceConversation.domaine=&expertsReferenceConversation.cible=&expertsReferenceConversation.typeRenvoi.code=&rechercheCompleteConversation.codeDomaine=&expertsReferenceConversation.typeZone=G&expertsReferenceConversation.paysRegion.code=&expertsReferenceConversation.typeCodeTaxe=&expertsReferenceConversation.fluxRegime=I&expertsReferenceConversation.regimeSollicite.code=&expertsReferenceConversation.regimePrecedent.code=&expertsReferenceConversation.regimeComplementaire.code=&expertsReferenceConversation.typeMesure.code=";
			
			return RetryHelper.RetryWithDelay(
				() => { return GetWebPage(requestPayload, "https://www.douane.gouv.fr/rita-encyclopedie/public/experts/reference/ajaxRechercheComplete", "https://www.douane.gouv.fr/rita-encyclopedie/public/experts/reference/init.action"); },
				TimeSpan.FromMilliseconds(ApplicationConfig.Instance.MinDelayBetweenAttemptsInCaseOfDownloadError),
				ApplicationConfig.Instance.MaxAttemptsCountInCaseOfDownloadError);
		}

		public virtual string GetFRConditionTypesWebPage()
		{
			var requestPayload = "expertsReferenceConversation.typeRecherche=TYPE_MESURE&expertsReferenceConversation.identifiant=&expertsReferenceConversation.motCle=&expertsReferenceConversation.code=&expertsReferenceConversation.appartenanceCodeAdd=&expertsReferenceConversation.typeCodeAdditionnel=&expertsReferenceConversation.appartenanceDoc=&expertsReferenceConversation.docDtp=&expertsReferenceConversation.typeDoc.code=&expertsReferenceConversation.flux=&expertsReferenceConversation.domaine=PROHIBITS%3AProhibitions%2FRestrictions&expertsReferenceConversation.cible=&expertsReferenceConversation.typeRenvoi.code=&rechercheCompleteConversation.codeDomaine=&expertsReferenceConversation.typeZone=&expertsReferenceConversation.groupePays.code=&expertsReferenceConversation.paysRegion.code=&expertsReferenceConversation.typeCodeTaxe=&expertsReferenceConversation.fluxRegime=I&expertsReferenceConversation.regimeSollicite.code=&expertsReferenceConversation.regimePrecedent.code=&expertsReferenceConversation.regimeComplementaire.code=&expertsReferenceConversation.typeMesure.code=";

			return RetryHelper.RetryWithDelay(
				() => { return GetWebPage(requestPayload, "https://www.douane.gouv.fr/rita-encyclopedie/public/experts/reference/ajaxRechercheComplete", "https://www.douane.gouv.fr/rita-encyclopedie/public/experts/reference/init.action"); },
				TimeSpan.FromMilliseconds(ApplicationConfig.Instance.MinDelayBetweenAttemptsInCaseOfDownloadError),
				ApplicationConfig.Instance.MaxAttemptsCountInCaseOfDownloadError);
		}

		public virtual string GetWebPage(string requestPayload, string url, string referer)
		{
			HttpWebRequest wRequest;
			try
			{
#pragma warning disable SYSLIB0014 // Type or member is obsolete
				wRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
#pragma warning restore SYSLIB0014 // Type or member is obsolete
			}
			catch
			{
				throw new RITAWebSiteException($"Queried URL {url} is invalid. The application will now close and retry next run.\r\nIf the problem persists, it is likely that GetWebPage() method is not fed with correct parameters. Please Fix it in FRReferenceData.RitaDataDownloader class");
			}

			wRequest.Accept = "*/*";
			wRequest.ContentLength = requestPayload.Length;
			wRequest.Method = "POST";
			wRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
			wRequest.Host = "www.douane.gouv.fr";
			wRequest.Referer = referer;
			wRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:80.0) Gecko/20100101 Firefox/80.0";
			wRequest.Proxy = WebRequest.GetSystemWebProxy();
			wRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
			wRequest.Timeout = GetTimeOut();
			Stream requestStream;
			try
			{
				requestStream = wRequest.GetRequestStream();
			}
			catch (Exception e)
			{
				throw new RITAWebSiteException($"Queried URL {url} returned an unexpected error. Reported error is:\r\n{e.Message}.\r\nThe application will now close.");
			}

			StreamWriter streamWriter = new StreamWriter(requestStream);
			streamWriter.Write(requestPayload);
			streamWriter.Flush();
			streamWriter.Close();
			requestStream.Close();

			var responseContent = string.Empty;
			attemptCount++;

			if (attemptCount > ApplicationConfig.Instance.MaxAttemptsCountInCaseOfTechnicalError)
			{
				throw new RITAWebSiteException($"Couldn't download data from {url} . The application will now close and retry next run.\r\nIf the problem persists, it is likely that GetWebPage() method is not fed with correct parameters. Please Fix it in FRReferenceData.RitaDataDownloader class");
			}

			try
			{
				using (HttpWebResponse response = GetResponse(wRequest))
				{
					using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
					{
						responseContent = sr.ReadToEnd();
					}
				}
			}
			catch (Exception e)
			{
				if (IsTemporaryWebExceptionError(e))
				{
					Console.WriteLine($"Couldn't download data from {url}. Let's try again...");
					Thread.Sleep(ApplicationConfig.Instance.MinDelayBetweenAttemptsInCaseOfTechnicalError);
					GetWebPage(requestPayload, url, referer);
				}
				else
				{
					throw new RITAWebSiteException($"RITA webSite returned an unexpected error when querying {url} . Reported error is:\r\n{e.Message}.\r\n The application will now close.");
				}
			}

			attemptCount = 0;

			return responseContent;
		}

		public virtual Stream GetRequestStream(HttpWebRequest webRequest) => webRequest.GetRequestStream();

		public virtual int GetTimeOut() => ApplicationConfig.Instance.MinDelayBetweenAttemptsInCaseOfTechnicalError;

		public virtual HttpWebResponse GetResponse(HttpWebRequest wRequest) => (HttpWebResponse)wRequest.GetResponse();

		public static bool IsTemporaryWebExceptionError(Exception e)
		{
			var errorStatusCode = ((e as WebException)?.Response as HttpWebResponse)?.StatusCode ?? HttpStatusCode.OK;
			return errorStatusCode == HttpStatusCode.InternalServerError || errorStatusCode == HttpStatusCode.BadGateway || errorStatusCode == HttpStatusCode.ServiceUnavailable || errorStatusCode == HttpStatusCode.GatewayTimeout;
		}

		public virtual async Task<string> PostRequestAndGetResponse(string xmlRequest)
		{
			try
			{
				var response = await PostXmlRequest(ApplicationConfig.Instance.RITAWebServiceURL, xmlRequest);

				string responseContent;

				responseContent = await TimeoutAfter(response.Content.ReadAsStringAsync(), TimeSpan.FromSeconds(20));

				return responseContent;
			}
			catch (Exception e)
			{
				throw new RITAWebServiceException("An error was reported while querying RITA Web Service. See Inner exception fore more details.", e);
			}
		}

		async static Task<HttpResponseMessage> PostXmlRequest(string baseUrl, string xmlString)
		{
			using (var httpClient = new HttpClient())
			{
				httpClient.Timeout = TimeSpan.FromSeconds(ApplicationConfig.Instance.DownloadTimeoutInSeconds);

				using (var httpContent = new StringContent(xmlString, Encoding.UTF8, "text/xml"))
				{
					return await TimeoutAfter(httpClient.PostAsync(new Uri(baseUrl), httpContent), httpClient.Timeout);
				}
			}
		}

		static async Task<TResult> TimeoutAfter<TResult>(Task<TResult> task, TimeSpan timeout)
		{
			using (var timeoutCancellationTokenSource = new CancellationTokenSource())
			{
				var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
				if (completedTask == task)
				{
					timeoutCancellationTokenSource.Cancel();
					return await task;  // Very important in order to propagate exceptions
				}
				else
				{
					throw new TimeoutException("The operation has timed out.");
				}
			}
		}

		int attemptCount;
	}
}
