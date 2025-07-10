using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CusTempStorageRegHeaderValidation))]
sealed class CusTempStorageRegHeaderValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckSRH_PresentationDate() =>	
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(header.SRH_PresentationDateInfo);

	public void TestCheckSRH_Reference() => CombineAssertions(() =>
	{
		header.SRH_Reference = ZString.Empty;
		header.Validation.ValidateSRH_Reference();
		AssertHasErrorContaining(header.SRH_ReferenceInfo, MandatoryValidation.MustBeEntered);

		header.SRH_Reference = "S01";
		AssertNoErrorContaining(header.SRH_ReferenceInfo, MandatoryValidation.MustBeEntered);
	});

	public void TestCheckSRH_CustomsOffice()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var euDataGrouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: euDataGrouping);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "MainCustomsOffice");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE1", "Germany", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		var header = Factory.New<CusTempStorageRegHeader>();
		header.Validation.ValidateSRH_CustomsOffice();
		AssertHasMessageErrorContaining(header.SRH_CustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);

		header.SRH_CustomsOffice = "DE1";
		header.Validation.ValidateSRH_CustomsOffice();
		AssertNoMessageError(header.SRH_CustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckSRH_PreviousReferenceType() =>
		ValidationTestHelper.AssertInvalidCodeMessageError(header.SRH_PreviousReferenceTypeInfo, "XYZ", PreviousReferenceTypes.Codes.Manifest);

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<CusTempStorageRegHeader>();
		header.SRH_Reference = "S01";
	}

	CusTempStorageRegHeader header;
}
