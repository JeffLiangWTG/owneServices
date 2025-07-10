using System;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Blazor.Client.Integration;
using Enterprise.ZArchitecture.Environment;

namespace CargoWise.Winzor.AppServer
{
	public partial class App
	{
		Guid? hostFormId;

		protected override Task OnInitializedAsync()
		{
			if (httpContextAccessor?.HttpContext is not null)
			{
				var hostFormIdHeaderValue = httpContextAccessor.HttpContext.Request.Headers[RequestHeaders.ClientAppHostFormId];

				if (Guid.TryParse(hostFormIdHeaderValue.FirstOrDefault(), out var guid))
				{
					hostFormId = guid;
				}

				Globals.WinzorClientIpAddress = httpContextAccessor.HttpContext.Connection?.RemoteIpAddress;
			}
			return base.OnInitializedAsync();
		}

		string AppendVersion(string href)
			=> $"{href}{fileVersionHash.Get(href)}";
	}
}
