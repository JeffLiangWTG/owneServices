using CargoWiseNext.Infrastructure.Installations;
using Microsoft.AspNetCore.Components;

namespace CargoWise.Blazor.SessionBroker.Pages;

public partial class LoginFailed : ComponentBase
{
	[Inject]
	IUrlHandlerProvider UrlHandlerProvider { get; set; }

	public string urlHandler;

	protected override void OnInitialized() => urlHandler = UrlHandlerProvider.GetUrlHandler();
}
