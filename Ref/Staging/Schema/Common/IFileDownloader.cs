using System;
using System.Net.Http.Headers;

namespace CargoWise.RefDbRepo.Staging.Common
{
	public interface IFileDownloader
	{
		IResponseStream GetFileStream(AuthenticationHeaderValue authenticationHeaderValue = null);
		DateTime GetCreationTime(AuthenticationHeaderValue authenticationHeaderValue = null);

		string[] SaveAndExtract(string pathToSave, bool copyAlways = false);
	}
}
