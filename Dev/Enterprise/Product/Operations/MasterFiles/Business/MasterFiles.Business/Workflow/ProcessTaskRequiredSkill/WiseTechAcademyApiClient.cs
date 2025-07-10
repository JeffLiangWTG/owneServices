using System.Net.Http;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	class WiseTechAcademyApiClient : WTG.WiseTechAcademy.WiseTechAcademyApiClient
	{
		public WiseTechAcademyApiClient()
			: base(GetBaseAddress())
		{
		}

		public WiseTechAcademyApiClient(HttpMessageHandler messageHandler)
			: base(messageHandler, GetBaseAddress())
		{
		}

		static string GetBaseAddress()
		{
			return Globals.IsTest ? "http://nothing/" : RawDataRegistry.Instance.WiseTechAcademyLmsApiBaseAddress.Value;
		}
	}
}
