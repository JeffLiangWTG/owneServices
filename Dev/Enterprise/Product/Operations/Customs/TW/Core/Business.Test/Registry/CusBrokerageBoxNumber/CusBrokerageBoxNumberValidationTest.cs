using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusBrokerageBoxNumberValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAll()
		{
			currentElement.Validation.ValidateAll();
			AssertHasErrorContaining(currentElement.BoxNumberInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(currentElement.CustomsOfficeAreaInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(currentElement.IsDefaultBoxNumberInfo, ValidationConstants.CusBrokerageBoxNumber.UnselectedDefaultBoxNumber);
		}

		public void TestValidateBoxNumber()
		{
			var targetInfo = currentElement.BoxNumberInfo;
			var errorBoxNumberAgain = ValidationConstants.CusBrokerageBoxNumber.BoxNumberAgain;
			currentElement.BoxNumber = "600";
			AssertNoErrorContaining(targetInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(targetInfo, errorBoxNumberAgain);
			var item2 = collection.AddNew();
			item2.BoxNumber = "600";
			currentElement.Validation.ValidateBoxNumber();
			AssertHasErrorContaining(targetInfo, errorBoxNumberAgain);
			currentElement.CustomsOfficeArea = TaiwanCustomsDistrictList.Codes.A;
			AssertNoErrorContaining(targetInfo, errorBoxNumberAgain);
			item2.CustomsOfficeArea = TaiwanCustomsDistrictList.Codes.A;
			currentElement.Validation.ValidateBoxNumber();
			AssertHasErrorContaining(targetInfo, errorBoxNumberAgain);
			currentElement.BoxNumber = ZString.Empty;
			AssertHasErrorContaining(targetInfo, MandatoryValidation.MustBeEntered);
			currentElement.BoxNumber = "30";
			AssertHasErrorContaining(targetInfo, ValidationConstants.CusBrokerageBoxNumber.InvalidBoxNumber);
			currentElement.BoxNumber = "90Z";
			AssertNoErrorContaining(targetInfo, ValidationConstants.CusBrokerageBoxNumber.InvalidBoxNumber);
			currentElement.BoxNumber = "30@";
			AssertHasErrorContaining(targetInfo, ValidationConstants.CusBrokerageBoxNumber.InvalidBoxNumber);
			currentElement.BoxNumber = "300";
			AssertNoErrorContaining(targetInfo, ValidationConstants.CusBrokerageBoxNumber.InvalidBoxNumber);
			currentElement.BoxNumber = "A23";
			AssertNoErrorContaining(targetInfo, ValidationConstants.CusBrokerageBoxNumber.InvalidBoxNumber);
			currentElement.BoxNumber = "8F";
			AssertHasErrorContaining(targetInfo, ValidationConstants.CusBrokerageBoxNumber.InvalidBoxNumber);
			currentElement.BoxNumber = "F80";
			AssertNoErrorContaining(targetInfo, ValidationConstants.CusBrokerageBoxNumber.InvalidBoxNumber);
			currentElement.BoxNumber = "!A6";
			AssertHasErrorContaining(targetInfo, ValidationConstants.CusBrokerageBoxNumber.InvalidBoxNumber);
			currentElement.BoxNumber = "7B7";
			AssertNoErrorContaining(targetInfo, ValidationConstants.CusBrokerageBoxNumber.InvalidBoxNumber);
			currentElement.BoxNumber = "z10";
			AssertHasErrorContaining(targetInfo, ValidationConstants.CusBrokerageBoxNumber.InvalidBoxNumber);
			currentElement.BoxNumber = "88C";
			AssertNoErrorContaining(targetInfo, ValidationConstants.CusBrokerageBoxNumber.InvalidBoxNumber);
			currentElement.BoxNumber = "ZZZ";
			AssertNoErrorContaining(targetInfo, ValidationConstants.CusBrokerageBoxNumber.InvalidBoxNumber);
			AssertNoErrors(targetInfo);
		}

		public void TestValidateCustomsOfficeArea()
		{
			var targetInfo = currentElement.CustomsOfficeAreaInfo;
			currentElement.CustomsOfficeArea = TaiwanCustomsDistrictList.Codes.A;
			AssertNoErrorContaining(targetInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(targetInfo, ListValidation.InvalidCodeError);
			currentElement.CustomsOfficeArea = "X";
			AssertHasErrorContaining(targetInfo, ListValidation.InvalidCodeError);
			currentElement.CustomsOfficeArea = ZString.Empty;
			AssertHasErrorContaining(targetInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestValidateIsDefaultBoxNumber()
		{
			var targetInfo = currentElement.IsDefaultBoxNumberInfo;
			var errorCustomsOfficeAreaIsDefaultAgain = ValidationConstants.CusBrokerageBoxNumber.CustomsOfficeAreaIsDefaultAgain;
			var errorUnselectedDefaultBoxNumber = ValidationConstants.CusBrokerageBoxNumber.UnselectedDefaultBoxNumber;
			currentElement.CustomsOfficeArea = TaiwanCustomsDistrictList.Codes.A;
			currentElement.IsDefaultBoxNumber = ZBool.False;
			AssertNoErrorContaining(targetInfo, errorCustomsOfficeAreaIsDefaultAgain);
			AssertHasErrorContaining(targetInfo, errorUnselectedDefaultBoxNumber);
			currentElement.IsDefaultBoxNumber = ZBool.True;
			AssertNoErrorContaining(targetInfo, errorCustomsOfficeAreaIsDefaultAgain);
			AssertNoErrorContaining(targetInfo, errorUnselectedDefaultBoxNumber);
			var item2 = collection.AddNew();
			item2.CustomsOfficeArea = TaiwanCustomsDistrictList.Codes.A;
			item2.IsDefaultBoxNumber = ZBool.True;
			currentElement.Validation.ValidateIsDefaultBoxNumber();
			AssertHasErrorContaining(targetInfo, errorCustomsOfficeAreaIsDefaultAgain);
			AssertNoErrorContaining(targetInfo, errorUnselectedDefaultBoxNumber);
			currentElement.CustomsOfficeArea = TaiwanCustomsDistrictList.Codes.B;
			AssertNoErrorContaining(targetInfo, errorCustomsOfficeAreaIsDefaultAgain);
			currentElement.CustomsOfficeArea = TaiwanCustomsDistrictList.Codes.A;
			AssertHasErrorContaining(targetInfo, errorCustomsOfficeAreaIsDefaultAgain);
			currentElement.IsDefaultBoxNumber = ZBool.False;
			AssertNoErrorContaining(targetInfo, errorCustomsOfficeAreaIsDefaultAgain);
			AssertNoErrorContaining(targetInfo, errorUnselectedDefaultBoxNumber);
			item2.IsDefaultBoxNumber = ZBool.False;
			currentElement.Validation.ValidateIsDefaultBoxNumber();
			AssertHasErrorContaining(targetInfo, errorUnselectedDefaultBoxNumber);
			item2.IsDefaultBoxNumber = ZBool.True;
			currentElement.Validation.ValidateIsDefaultBoxNumber();
			AssertNoErrors(targetInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			collection = new CusBrokerageBoxNumberCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			currentElement = collection.AddNew();
		}

		CusBrokerageBoxNumber currentElement;
		CusBrokerageBoxNumberCollection collection;
	}
}
