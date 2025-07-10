using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DispositionDataCollectionExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestGetReleaseStatus()
		{
			var dispositionCode = Factory.New<DispositionData>();
			var dispositionCodes = new CargoReleaseProcessingResultList();
			foreach (var code in dispositionCodes.GetAllCodes())
			{
				dispositionCode.US_Code = code;
				dispositionCode.US_ReleaseDate = ZDateTime.Today;
				var releaseStatus = dispositionCode.GetReleaseStatus();
				AssertEquals(code, CargoReleaseProcessingResultList.IsDispositionCodeRelatedToRleaseStatus(dispositionCode), !releaseStatus.IsEmpty);
			}
		}
	}
}
