using CargoWiseNext.Infrastructure.Installations;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CargoWise.Blazor.SessionBroker.Pages
{
	public class BrowserLaunchPadModel : PageModel
	{
		public string ClientAppLaunchUri;
		public string MsixAppInstallerUrl;
		public IUrlHandlerProvider urlHandlerProvider;
		public BrowserLaunchPadModel(IUrlHandlerProvider urlHandlerProvider)
		{
			this.urlHandlerProvider = urlHandlerProvider;
		}

		public void OnGet()
		{
			ClientAppLaunchUri = $"{urlHandlerProvider.GetUrlHandler()}:{Request.Scheme}://{Request.Host}";
			MsixAppInstallerUrl = $"{Request.Scheme}://{Request.Host}{Arguments.ClientAppInstallerDownloadUrl}";
		}
	}
}
