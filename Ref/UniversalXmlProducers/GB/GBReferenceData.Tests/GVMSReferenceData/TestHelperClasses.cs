using CargoWise.RefDbRepo.GBReferenceData.Services.GVMSReferenceData;
using CargoWise.RefDbRepo.GBReferenceData.Services.GVMSReferenceData.Models;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.GVMSReferenceData
{
	internal class TestHelperClasses
	{
		public class GVMSReferenceDataClientTester : BaseGvmsReferenceDataClient
		{
			public new ReferenceData GetReferenceData(IGvmsWebClient webClientWrapper, string url) => BaseGvmsReferenceDataClient.GetReferenceData(webClientWrapper, url);

			public override IGvmsWebClient GetGvmsWebClient() => TestWebClientWrapper;

			public IGvmsWebClient TestWebClientWrapper { get; set; }
		}
	}
}
