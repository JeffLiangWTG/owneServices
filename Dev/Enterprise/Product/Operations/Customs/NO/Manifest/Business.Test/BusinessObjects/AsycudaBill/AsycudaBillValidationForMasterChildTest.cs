using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.Business.Testing;

[TestedType(typeof(AsycudaBillValidationForMasterChild))]
sealed class AsycudaBillValidationForMasterChildTest : AsycudaBillValidationAbstractTest
{
	protected override AsycudaBill GetAsycudaBillForTests() => header.MasterBill;

	public void TestABL_CustomsFinalDestinationPortValidation() => ValidationTestHelper.AssertYouHaveNotEnteredMessageError(bill.ABL_CustomsFinalDestinationPortInfo);

	public void TestCheckABL_RL_NKPortOfLoading() => ValidationTestHelper.AssertFieldIsNotMandatory(bill.ABL_RL_NKPortOfLoadingInfo);

	public void TestCheckABL_RL_NKPortOfDischarge() => ValidationTestHelper.AssertFieldIsNotMandatory(bill.ABL_RL_NKPortOfDischargeInfo);

	public void TestIsABL_E_DEPRequired() => ValidationTestHelper.AssertFieldIsNotMandatory(bill.ABL_E_DEPInfo);

	public void TestCheckMandatoryABL_E_ARV() => ValidationTestHelper.AssertFieldIsNotMandatory(bill.ABL_E_ARVInfo);

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<AsycudaManifestHeader>();
		bill = header.MasterBill;
	}

	AsycudaManifestHeader header;
	AsycudaBill bill;
}
