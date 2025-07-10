using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(GuaranteeBondDetail))]
class GuaranteeBondDetailTest : EnterpriseBusinessObjectTestCase
{
	public void TestIsPasswordReadOnly()
	{
		var bondDetail1 = Factory.New<GuaranteeBondDetail>();
		bondDetail1.PW_BondType = GuaranteeBondTypeList.Codes.Comprehensive;
		bondDetail1.PW_CPH_Guarantee = CreateBaseCusGuaranteeHeader(PLGuaranteeTypeList.Codes.GEN, "OVER9000", "1", CountryCodes.Poland, null).PK;

		var bondDetail2 = Factory.New<GuaranteeBondDetail>();
		bondDetail2.PW_BondType = GuaranteeBondTypeList.Codes.Comprehensive;
		var guaranteeHeader2 = CreateBaseCusGuaranteeHeader(PLGuaranteeTypeList.Codes.GEN, "OVER9001", "1", CountryCodes.Poland, null);
		guaranteeHeader2.MainAccessCode = "1234";
		guaranteeHeader2.AdditionalAccessCodes.AddNew();
		bondDetail2.PW_CPH_Guarantee = guaranteeHeader2.PK;

		var bondDetail3 = Factory.New<GuaranteeBondDetail>();
		bondDetail3.PW_BondType = GuaranteeBondTypeList.Codes.Comprehensive;
		var guaranteeHeader3 = CreateBaseCusGuaranteeHeader(PLGuaranteeTypeList.Codes.GEN, "OVER9002", "1", CountryCodes.Poland, null);
		guaranteeHeader3.MainAccessCode = "1234";
		bondDetail3.PW_CPH_Guarantee = guaranteeHeader3.PK;

		var bondDetail4 = Factory.New<GuaranteeBondDetail>();
		bondDetail4.PW_BondType = GuaranteeBondTypeList.Codes.Individual;
		var guaranteeHeader4 = CreateBaseCusGuaranteeHeader(PLGuaranteeTypeList.Codes.GEN, "OVER9003", "1", CountryCodes.Poland, null);
		bondDetail4.PW_CPH_Guarantee = guaranteeHeader4.PK;

		var bondDetail5 = Factory.New<GuaranteeBondDetail>();
		bondDetail5.PW_BondType = GuaranteeBondTypeList.Codes.Comprehensive;

		CombineAssertions(() =>
		{
			AssertEquals("IsBondTypeAsCON, CorrespondingCusGuaranteeHeader without main access code exists and has no additional Access Codes", false, bondDetail1.PW_PasswordInfo.ReadOnly);
			AssertEquals("IsBondTypeAsCON, CorrespondingCusGuaranteeHeader with main access code exists and has additional Access Codes", false, bondDetail2.PW_PasswordInfo.ReadOnly);
			AssertEquals("IsBondTypeAsCON, CorrespondingCusGuaranteeHeader with main access code exists and has no additional Access Codes", true, bondDetail3.PW_PasswordInfo.ReadOnly);
			AssertEquals("!IsBondTypeAsCON, CorrespondingCusGuaranteeHeader without main access code exists and has no additional Access Codes", true, bondDetail4.PW_PasswordInfo.ReadOnly);
			AssertEquals("IsBondTypeAsCON, CorrespondingCusGuaranteeHeader doesn't exist", true, bondDetail5.PW_PasswordInfo.ReadOnly);
		});
	}

	public void TestInstruction()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		GuaranteeBondDetail bondDetail = entryInstruction.Guarantees.AddNew();
		AssertEquals(entryInstruction, bondDetail.EntryInstruction);
	}

	public void TestIsAmountReadOnly()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		GuaranteeBondDetail bondDetail1 = entryInstruction.Guarantees.AddNew();
		AssertEquals(true, entryInstruction.HasOnlyOneGuarantee());

		GuaranteeBondDetail bondDetail2 = entryInstruction.Guarantees.AddNew();
		AssertEquals(false, entryInstruction.HasOnlyOneGuarantee());
	}

	public void TestValidationAndLookups()
	{
		GuaranteeBondDetail bondDetail = Factory.New<GuaranteeBondDetail>();
		AssertEquals(typeof(GuaranteeBondDetailValidation), bondDetail.Validation.GetType());
		AssertEquals(typeof(GuaranteeBondDetailLookups), bondDetail.Lookups.GetType());
	}

	public void TestPW_BondType_Caption()
	{
		AssertEquals("Type", DataBoundResourceStrings.GetDataForProperty(Detail.PW_BondTypeInfo).Caption);
	}

	public void TestPW_HolderIdentification_Caption()
	{
		AssertEquals("Holder ID", DataBoundResourceStrings.GetDataForProperty(Detail.PW_HolderIdentificationInfo).Caption);
	}

	public void TestPW_BondAmount_Caption()
	{
		AssertEquals("Amount", DataBoundResourceStrings.GetDataForProperty(Detail.PW_BondAmountInfo).Caption);
	}

	public void TestPW_Password_Caption()
	{
		AssertEquals("Access Code", DataBoundResourceStrings.GetDataForProperty(Detail.PW_PasswordInfo).Caption);
	}

	public void TestPW_RX_NKCurrency_Caption()
	{
		AssertEquals("Currency", DataBoundResourceStrings.GetDataForProperty(Detail.PW_RX_NKCurrencyInfo).Caption);
		AssertEquals("Cur.", DataBoundResourceStrings.GetDataForProperty(Detail.PW_RX_NKCurrencyInfo).ShortCaption);
	}

	public void TestBondNumberAlwaysFocesItselfToUpperCase()
	{
		Detail.PW_BondNumber = "abc";
		AssertEquals("GuaranteeBondDetail.PW_BondNumber", "ABC", Detail.PW_BondNumber);
	}
	public void TestHolderIdentificationAlwaysFocesItselfToUpperCase()
	{
		Detail.PW_HolderIdentification = "abc";
		AssertEquals("GuaranteeBondDetail.PW_HolderIdentification", "ABC", Detail.PW_HolderIdentification);
	}

	public void TestPW_BondTypeChanged()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		GuaranteeBondDetail bondDetail = entryInstruction.Guarantees.AddNew();

		bondDetail.PW_BondType = GuaranteeBondTypeList.Codes.Comprehensive;
		AssertEquals(ZString.Empty, bondDetail.PW_HolderIdentification);
		AssertEquals(ZString.Empty, bondDetail.PW_Password);
		AssertEquals(ZString.Empty, bondDetail.PW_BondNumber);
		bondDetail.PW_HolderIdentification = "HOLDERID123";
		bondDetail.PW_Password = "pas1";
		bondDetail.PW_BondNumber = "BOND123";
		AssertEquals("HOLDERID123", bondDetail.PW_HolderIdentification);
		AssertEquals("pas1", bondDetail.PW_Password);
		AssertEquals("BOND123", bondDetail.PW_BondNumber);

		bondDetail.PW_BondType = GuaranteeBondTypeList.Codes.Individual;
		AssertEquals(ZString.Empty, bondDetail.PW_HolderIdentification);
		AssertEquals(ZString.Empty, bondDetail.PW_Password);
		AssertEquals(ZString.Empty, bondDetail.PW_BondNumber);
		bondDetail.PW_HolderIdentification = "HOLDERID234";
		bondDetail.PW_Password = "pas2";
		bondDetail.PW_BondNumber = "BOND234";
		AssertEquals("HOLDERID234", bondDetail.PW_HolderIdentification);
		AssertEquals("pas2", bondDetail.PW_Password);
		AssertEquals("BOND234", bondDetail.PW_BondNumber);

		bondDetail.PW_BondType = GuaranteeBondTypeList.Codes.Comprehensive;
		AssertEquals(ZString.Empty, bondDetail.PW_HolderIdentification);
		AssertEquals(ZString.Empty, bondDetail.PW_Password);
		AssertEquals(ZString.Empty, bondDetail.PW_BondNumber);
		bondDetail.PW_HolderIdentification = "HOLDERID345";
		bondDetail.PW_Password = "pas3";
		bondDetail.PW_BondNumber = "BOND345";
		AssertEquals("HOLDERID345", bondDetail.PW_HolderIdentification);
		AssertEquals("pas3", bondDetail.PW_Password);
		AssertEquals("BOND345", bondDetail.PW_BondNumber);

		bondDetail.PW_BondType = GuaranteeBondTypeList.Codes.Comprehensive;
		AssertEquals("HOLDERID345", bondDetail.PW_HolderIdentification);
		AssertEquals("pas3", bondDetail.PW_Password);
		AssertEquals("BOND345", bondDetail.PW_BondNumber);
	}

	public void TestPW_CPH_Guarantee()
	{
		var importer = Factory.NewWithValidTestData<OrgHeader>();
		var importerNotTINCode = importer.CustomsCodes.AddNew();
		importerNotTINCode.OK_CodeType = OrgCusCode.PolandCodeTypes.NIP;
		importerNotTINCode.OK_RN_NKCodeCountry = CountryCodes.Poland;
		importerNotTINCode.OK_CustomsRegNo = "64533";
		var importerTINCode = importer.CustomsCodes.AddNew();
		importerTINCode.OK_CodeType = OrgCusCode.PolandCodeTypes.TIN;
		importerTINCode.OK_RN_NKCodeCountry = CountryCodes.Poland;
		importerTINCode.OK_CustomsRegNo = "54321";

		var importerGuarantee = CreateBaseCusGuaranteeHeader(PLGuaranteeTypeList.Codes.GEN, "OVER9000", "1", CountryCodes.Poland, importer);
		importerGuarantee.MainAccessCode = "1234";

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		var entry = declaration.CustomsEntryInstructions.AddNew();
		var guarantee = entry.Guarantees.AddNew();

		guarantee.PW_BondType = GuaranteeBondTypeList.Codes.Comprehensive;
		CombineAssertions(() =>
		{
			guarantee.PW_CPH_Guarantee = ZGuid.Empty;
			AssertEquals("ZGuid.Empty - PW_BondNumber is empty", guarantee.PW_BondNumber, ZString.Empty);
			AssertEquals("ZGuid.Empty - PW_HolderIdentification is empty", guarantee.PW_HolderIdentification, ZString.Empty);
			AssertEquals("ZGuid.Empty - PW_Password is empty", guarantee.PW_Password, ZString.Empty);

			guarantee.PW_CPH_Guarantee = ZGuid.Invalid;
			AssertEquals("ZGuid.Invalid - PW_BondNumber is empty", guarantee.PW_BondNumber, ZString.Empty);
			AssertEquals("ZGuid.Invalid - PW_HolderIdentification is empty", guarantee.PW_HolderIdentification, ZString.Empty);
			AssertEquals("ZGuid.Invalid - PW_Password is empty", guarantee.PW_Password, ZString.Empty);

			guarantee.PW_CPH_Guarantee = importerGuarantee.PK;
			AssertEquals("Valid PW_CPH_Guarantee PW_BondNumber", guarantee.PW_BondNumber, "OVER9000");
			AssertEquals("Valid PW_CPH_Guarantee PW_HolderIdentification", guarantee.PW_HolderIdentification, "54321");
			AssertEquals("Valid PW_CPH_Guarantee PW_Password", guarantee.PW_Password, "1234");
		});
	}

	BaseCusGuaranteeHeader CreateBaseCusGuaranteeHeader(ZString type, ZString number, ZString subType, ZString countryCode, OrgHeader orgHeader)
	{
		var result = Factory.NewWithValidTestData<CusGuaranteeHeader>();
		result.CPH_Type = type;
		result.CPH_Number = number;
		result.CPH_OH_PermitHolder = orgHeader?.PK ?? ZGuid.Empty;
		result.CPH_RN_NKCountryCode = countryCode;
		result.CPH_SubType = subType;
		result.CPH_StartDate = ZDate.Today.AddDays(-1);
		result.CPH_EndDate = ZDate.Today.AddDays(1);
		return result;
	}

	public void TestIsCON()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		GuaranteeBondDetail bondDetail = entryInstruction.Guarantees.AddNew();

		bondDetail.PW_BondType = GuaranteeBondTypeList.Codes.Comprehensive;
		AssertEquals(true, bondDetail.IsBondTypeAsCON);

		bondDetail.PW_BondType = GuaranteeBondTypeList.Codes.Individual;
		AssertEquals(false, bondDetail.IsBondTypeAsCON);
	}

	GuaranteeBondDetail Detail => fDetail ??= CreateGuarantee(Factory);
	GuaranteeBondDetail fDetail;

	GuaranteeBondDetail CreateGuarantee(BusinessObjectFactory factory)
	{
		var dec = factory.New<JobDeclaration>();
		var cei = dec.CustomsEntryInstructions.AddNew();
		return cei.Guarantees.AddNew();
	}

	protected override BusinessObject GetNewBusinessObject() => CreateGuarantee(Factory);
	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateGuarantee(factory);
}
