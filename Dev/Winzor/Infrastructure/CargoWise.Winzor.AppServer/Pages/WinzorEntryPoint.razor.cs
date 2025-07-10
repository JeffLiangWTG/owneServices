#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

namespace CargoWise.Winzor.AppServer.Pages
{
	public partial class WinzorEntryPoint
	{
		/// <summary>
		/// The ID of the pool window request. Required by the client app to differentiate the URLs but is not used by the server.
		/// </summary>
		[Parameter]
		public string? PoolRequestId { get; set; }
	}
}
