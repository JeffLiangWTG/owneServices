using System;

namespace OcmPoc.Tests.Scenario.Configuration
{
	public class ApiConfig
	{
		public string RootUrl { get; set; }
		public Uri RootUri => new Uri(RootUrl ?? "http://foo");
		public Uri MessageFlowsUri => new Uri(RootUri, "message-flows");
	}
}