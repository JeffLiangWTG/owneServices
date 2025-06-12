using System;

namespace CargoWise.eHub.Nudge
{
	public class EHubClientSystem
	{
		public EHubClientSystem(string systemCode, string url)
		{
			this.SystemCode = systemCode;
			this.URL = url;
		}

		public string SystemCode { get; set; }

		public string URL { get; set; }
	}
}
