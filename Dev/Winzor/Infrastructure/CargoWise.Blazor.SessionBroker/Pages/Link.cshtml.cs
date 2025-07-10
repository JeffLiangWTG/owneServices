using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;
using CargoWise.Blazor.SessionBroker.Helpers;
using CargoWiseNext.Infrastructure.Installations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CargoWise.Blazor.SessionBroker.Pages;

public class Link : PageModel
{
	internal const string LaunchUrlRegistryName = "WebVersionLaunchUrl";
	public IUrlHandlerProvider urlHandlerProvider;

	public string ClientAppLaunchUri;
	public string MsixAppInstallerUrl;

	[BindProperty(SupportsGet = true)]
	[Required(ErrorMessage = "Command is required")]
	public string Command { get; set; }

	[BindProperty(SupportsGet = true)]
	[Required(ErrorMessage = "Controller ID is required")]
	public string ControllerID { get; set; }

	[BindProperty(SupportsGet = true)]
	[Required(ErrorMessage = "Business Entity PK is required")]
	public string BusinessEntityPK { get; set; }

	readonly IRegistryAccessor registryAccessor;

	public Link(IUrlHandlerProvider urlHandlerProvider, IRegistryAccessor registryAccessor)
	{
		this.urlHandlerProvider = urlHandlerProvider;
		this.registryAccessor = registryAccessor;
	}

	[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer")]
	public void OnGet()
	{
		MsixAppInstallerUrl = $"{Request.Scheme}://{Request.Host}{Arguments.ClientAppInstallerDownloadUrl}";

		if (!ModelState.IsValid)
		{
			ClientAppLaunchUri = $"{urlHandlerProvider.GetUrlHandler()}:{Request.Scheme}://{Request.Host}";
			return;
		}
		
		var parameters = new NameValueCollection { { "Command", Command }, { "ControllerID", ControllerID }, { "BusinessEntityPK", BusinessEntityPK } };
		var launchUrl = registryAccessor.GetStringValue(LaunchUrlRegistryName);
		if (!string.IsNullOrEmpty(launchUrl))
		{
			parameters.Set("WebVersionLaunchURL", launchUrl);
		}

		if (Request.HttpContext.Request.QueryString.HasValue)
		{
			var extraParameters = HttpUtility.ParseQueryString(Request.HttpContext.Request.QueryString.Value!);
			foreach (var key in extraParameters.AllKeys)
			{
				// filtering out some undesired values
				if (!(string.IsNullOrWhiteSpace(key) && string.IsNullOrWhiteSpace(extraParameters[key])))
				{
					parameters.Set(key, extraParameters[key]);
				}
			}
		}

		var queryString = string.Join("&", parameters.AllKeys.Select(a => HttpUtility.UrlEncode(a) + "=" + HttpUtility.UrlEncode(parameters[a])));
		ClientAppLaunchUri = $"edient:{queryString}";
	}
}
