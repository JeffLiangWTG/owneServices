using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.Testing
{
	public class ClientRateValidationTest : RatingHeaderValidationTest
	{
		public void TestClientRateHeaderValidation()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();

			var rate = Helper.NewClientRate(header);

			rate.RunPreSaveValidation();
			AssertHasError(rate.TH_OHInfo, ErrorMessages.InvalidClientRateHeader);

			header.OH_IsConsignee = true;
			Factory.Save();

			rate.RunPreSaveValidation();
			AssertNoError(rate.TH_OHInfo, ErrorMessages.InvalidClientRateHeader);

			var rate2 = Helper.NewClientRate(header);
			rate2.RunPreSaveValidation();

			AssertHasError(rate2.TH_OHInfo, ErrorMessages.RateForThisClientAlreadyExists);
			rate2.TH_GC = ZGuid.Empty;

			rate2.RunPreSaveValidation();
			AssertNoError(rate2.TH_OHInfo, ErrorMessages.RateForThisClientAlreadyExists);
			Factory.Save();

			var globalRate = Helper.NewGlobalClientRate(header);
			globalRate.RunPreSaveValidation();

			AssertHasError(globalRate.TH_OHInfo, ErrorMessages.GlobalRateForThisClientAlreadyExists);

			var header2 = Factory.NewWithValidTestData<OrgHeader>();
			globalRate.TH_OH = header2.PK;

			AssertNoError(globalRate.TH_OHInfo, ErrorMessages.GlobalRateForThisClientAlreadyExists);
		}
	}
}
