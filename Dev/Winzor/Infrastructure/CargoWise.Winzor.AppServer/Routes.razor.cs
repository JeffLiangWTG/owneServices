using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

namespace CargoWise.Winzor.AppServer
{
	public partial class Routes
	{
		[Parameter]
		public Guid? HostFormGuid { get; set; }
	}
}
