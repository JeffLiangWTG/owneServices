using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	sealed class NctsHeaderPhase5ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestParent()
		{
			AssertEquals(nctsHeader.Validation.Parent, nctsHeader);
		}

		public void TestCheckStampDutyStatus_Mandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(nctsHeader.StampDutyStatusInfo);
		}

		public void TestCheckStampDutyStatus_List()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(nctsHeader.StampDutyStatusInfo, "A", StampDutyStatusCodeList.Codes.D2);
		}

		public void TestCheckStampDuty_Mandatory()
		{
			nctsHeader.StampDutyStatus = StampDutyStatusCodeList.Codes.D3;
			nctsHeader.StampDuty = ZDecimal.Zero;

			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(nctsHeader.StampDutyInfo, "Please enter Stamp Duty if Stamp Duty Status Code value is 3.");
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		}
		NctsHeader nctsHeader;
	}
}
