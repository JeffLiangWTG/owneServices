using CargoWise.Blazor.SessionBroker.Helpers;
using CargoWiseNext.Infrastructure.Installations;
using Microsoft.AspNetCore.Components;

namespace CargoWise.Blazor.SessionBroker.Pages;

public partial class SiteOffline : ComponentBase
{
	[Inject]
	ISiteOfflineChecker SiteOfflineChecker { get; set; }

	protected override void OnInitialized() => OfflineMessage = SiteOfflineChecker.GetOfflineMessage();

	public string OfflineMessage;
}
