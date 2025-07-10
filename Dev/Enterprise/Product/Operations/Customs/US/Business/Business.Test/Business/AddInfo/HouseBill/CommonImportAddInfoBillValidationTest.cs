using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	abstract class CommonImportAddInfoBillValidationTest : TestCaseWithFactory
	{
		protected void AssertUS_VolumeUQ(Bill bill)
		{
			bill.AddInfoValidation.ValidateUS_VolumeUQ();
			AssertNoMessageErrorContaining(bill.US_VolumeUQInfo, "Volume UQ is required when Volume has a value.");
			bill.US_Volume = 10;
			bill.AddInfoValidation.ValidateUS_VolumeUQ();
			AssertHasMessageErrorContaining(bill.US_VolumeUQInfo, "Volume UQ is required when Volume has a value.");
			bill.US_VolumeUQ = Core.Constants.Volume.CubicMetres;
			AssertNoMessageErrorContaining(bill.US_VolumeUQInfo, "Volume UQ is required when Volume has a value.");
		}
	}
}
