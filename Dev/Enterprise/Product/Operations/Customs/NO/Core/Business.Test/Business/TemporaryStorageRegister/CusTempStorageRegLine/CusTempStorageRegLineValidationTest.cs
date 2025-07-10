using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CusTempStorageRegLineValidation))]
sealed class CusTempStorageRegLineValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckSRL_OwnerReferenceType()
	{
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(line.SRL_OwnerReferenceTypeInfo, "~", OwnerReferenceTypeList.Codes._AWB);
	}

	public void TestCheckSRL_OwnerReference()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(line.SRL_OwnerReferenceInfo);
	}

	public void TestCheckSRL_GoodsDescription()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(line.SRL_GoodsDescriptionInfo);
	}

	public void TestCheckSRL_LineNumber()
	{
		line.SRL_LineNumber = 5;

		var line2 = header.CusTempStorageRegLines.AddNew();
		line2.SRL_LineNumber = 5;
		AssertHasMessageError(line2.SRL_LineNumberInfo, "Line Number should not be duplicated.");
	}

	public void TestCheckSRL_CustodianIdentifier()
	{
		line.Validation.ValidateSRL_CustodianIdentifier();
		AssertIdentifier(line.SRL_CustodianIdentifierInfo);
	}

	public void TestCheckSRL_CustodianIdentifierBranchNo()
	{
		line.Validation.ValidateSRL_CustodianIdentifierBranchNo();
		AssertBranchIdentifier(line.SRL_CustodianIdentifierBranchNoInfo);
	}

	public void TestCheckSRL_GoodsOwnerIdentifier()
	{
		line.Validation.ValidateSRL_GoodsOwnerIdentifier();
		AssertIdentifier(line.SRL_GoodsOwnerIdentifierInfo);
	}

	public void TestCheckSRL_GoodsOwnerIdentifierBranchNo()
	{
		line.Validation.ValidateSRL_GoodsOwnerIdentifierBranchNo();
		AssertBranchIdentifier(line.SRL_GoodsOwnerIdentifierBranchNoInfo);
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<CusTempStorageRegHeader>();
		line = header.CusTempStorageRegLines.AddNew();
	}
	CusTempStorageRegHeader header;
	CusTempStorageRegLine line;

	void AssertIdentifier(ZPropertyInfo propertyInfo)
	{
		var messageError = "The EORI code needs to start with a country/region code as a prefix.";
		CombineAssertions(() =>
		{
			AssertNoMessageError("Empty", propertyInfo, messageError);

			propertyInfo.Value = (ZString)"124444";
			AssertHasMessageError("Invalid", propertyInfo, messageError);

			propertyInfo.Value = (ZString)"DE124444";
			AssertNoMessageError("Valid", propertyInfo, messageError);
		});
	}

	void AssertBranchIdentifier(ZPropertyInfo propertyInfo)
	{
		var messageError = "EORI Branch Suffix should be 4 digits and different to the EORI code, e.g. 0001";
		CombineAssertions(() =>
		{
			AssertNoMessageError("Empty", propertyInfo, messageError);

			propertyInfo.Value = (ZString)"A111";
			AssertHasMessageError("Characters", propertyInfo, messageError);

			propertyInfo.Value = (ZString)"111";
			AssertHasMessageError("Short", propertyInfo, messageError);

			propertyInfo.Value = (ZString)"3444";
			AssertNoMessageError("Valid", propertyInfo, messageError);
		});
	}
}
