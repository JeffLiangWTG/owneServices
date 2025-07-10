using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.ITReferenceData.Services;

namespace CargoWise.RefDbRepo.ITReferenceData.Business
{
	public sealed class PublicationTimeLoader : IPublicationTimeLoader
	{
		public PublicationTimeLoader(IDateTimeProvider dateTimeProvider, IHttpClient httpClient)
		{
			this.dateTimeProvider = Argument.NotNull(dateTimeProvider, nameof(dateTimeProvider));
			this.httpClient = Argument.NotNull(httpClient, nameof(httpClient));
		}

		readonly IDateTimeProvider dateTimeProvider;
		readonly IHttpClient httpClient;

		DateTime IPublicationTimeLoader.GetDateTime()
		{
			var responseString = httpClient.Get(ApplicationConfig.TaricServletUrl);
			var dateTimeMatch = Regex.Matches(responseString, Constants.Regex.PublicationDateTime).Cast<Match>().Select(x => x.Groups["DateTime"].Value).FirstOrDefault();
			return dateTimeMatch.ToDateTime() ?? dateTimeProvider.Now;
		}
	}
}
