using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class AdditionalMessageInformationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBoardedWeightUQList()
		{
			Assert(AdditionalMessageInformationLookups.BoardedWeightUQList.Count == 2);
			Assert(AdditionalMessageInformationLookups.BoardedWeightUQList.ContainsCode("L"));
			Assert(AdditionalMessageInformationLookups.BoardedWeightUQList.ContainsCode("K"));
		}

		AdditionalMessageInformation AdditionalMessageInformation
		{
			get
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_E_ARV = new ZDateTime(2021, 10, 5, 11, 0, 0);
				header.AMA_RL_NKPortOfLoading = "AUSYD";
				header.AMA_RL_NKPortOfFirstArrival = "USLAX";
				header.AMA_RL_NKPortOfDischarge = "NZAKL";
				return additionalMessageInformation ?? (additionalMessageInformation = new AdditionalMessageInformation(header));
			}
		}

		AdditionalMessageInformation additionalMessageInformation;
		AdditionalMessageInformationLookups AdditionalMessageInformationLookups
		{
			get
			{
				return AdditionalMessageInformation.Lookups;
			}
		}
	}
}
