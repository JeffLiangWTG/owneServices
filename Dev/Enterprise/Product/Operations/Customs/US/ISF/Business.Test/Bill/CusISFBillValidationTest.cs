using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class CusISFBillValidationTest : BusinessObjectValidationTestCase
	{
		[TestDate(2009, 6, 10)]
		public void TestCheckBB_BillType()
		{
			bill.BB_BillType = ZString.Empty;
			AssertHasMessageErrorContaining(bill.BB_BillTypeInfo, MandatoryValidation.YouHaveNotEntered);
			bill.BB_BillType = "ZZ";
			AssertNoMessageErrorContaining(bill.BB_BillTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(bill.BB_BillTypeInfo, ListValidation.InvalidCodeMessageError);
			foreach (ICodeDescription pair in bill.Lookups.BillTypes)
			{
				bill.BB_BillType = pair.Code;
				AssertNoMessageErrorContaining(bill.BB_BillTypeInfo, ListValidation.InvalidCodeMessageError);
			}

			CusISFBill bill2 = header.ReferenceDatas.AddNew();
			bill2.BB_BillType = BillTypeList.Codes.SuretyCode;
			bill.BB_BillType = BillTypeList.Codes.SuretyCode;
			AssertHasMessageError(bill.BB_BillTypeInfo, ValidationConstants.Bill.OnlyOneSuretyCode);
			bill.BB_BillType = BillTypeList.Codes.MasterBillOfLading;
			AssertNoMessageError(bill.BB_BillTypeInfo, ValidationConstants.Bill.OnlyOneSuretyCode);
			bill2.BB_BillType = BillTypeList.Codes.BondReferenceNumber;
			bill.BB_BillType = BillTypeList.Codes.BondReferenceNumber;
			AssertHasMessageError(bill.BB_BillTypeInfo, ValidationConstants.Bill.OnlyOneBondReferenceNumber);
			bill.BB_BillType = BillTypeList.Codes.MasterBillOfLading;
			AssertNoMessageError(bill.BB_BillTypeInfo, ValidationConstants.Bill.OnlyOneBondReferenceNumber);
			bill2.BB_BillType = BillTypeList.Codes.FullNameOfISFImporter;
			bill.BB_BillType = BillTypeList.Codes.FullNameOfISFImporter;
			AssertHasMessageError(bill.BB_BillTypeInfo, ValidationConstants.Bill.OnlyOneFullNameOfISFImporter);
			bill.BB_BillType = BillTypeList.Codes.MasterBillOfLading;
			AssertNoMessageError(bill.BB_BillTypeInfo, ValidationConstants.Bill.OnlyOneFullNameOfISFImporter);
		}

		public void TestCheckBB_BillNum()
		{
			CusISFBill bill2 = header.ReferenceDatas.AddNew();
			bill2.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			bill2.BB_BillNum = "XJF23469874";
			bill.BB_BillNum = ZString.Empty;
			AssertHasMessageErrorContaining(bill.BB_BillNumInfo, MandatoryValidation.YouHaveNotEntered);
			bill.BB_BillNum = "ZZ12322";
			AssertNoMessageErrorContaining(bill.BB_BillNumInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(bill.BB_BillNumInfo, ValidationConstants.Bill.CBPEntryNumberRightFormat);
			bill.BB_BillType = BillTypeList.Codes.USCBPEntryNumber;
			AssertHasMessageError(bill.BB_BillNumInfo, ValidationConstants.Bill.CBPEntryNumberRightFormat);
			bill.BB_BillNum = "XJF23469874";
			AssertNoMessageError(bill.BB_BillNumInfo, ValidationConstants.Bill.CBPEntryNumberRightFormat);
			AssertNoMessageError(bill.BB_BillNumInfo, ValidationConstants.Bill.ReferenceDataAlreadyExists);
			bill.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			AssertHasMessageError(bill.BB_BillNumInfo, ValidationConstants.Bill.ReferenceDataAlreadyExists);
			bill.BB_BillNum = "ODE23469874";
			AssertNoMessageError(bill.BB_BillNumInfo, ValidationConstants.Bill.ReferenceDataAlreadyExists);
			CusISFHeader header2 = Factory.New<CusISFHeader>();
			header2.BF_JobReference = "ISF23424345";
			header2.BF_OceanBill = "OCB2134234";
			header2.BF_MasterBill = "MWB2134234";
			header2.BF_HouseBill = "SCACHB2134234";
			Factory.Save();
			bill2.BB_BillType = BillTypeList.Codes.MasterBillOfLading;
			bill2.BB_BillNum = "MWB2134234";
			string messageError = ValidationConstants.Bill.ReferenceDataAlreadyExistsOnAnotherISF(BillTypeList.Descriptions.HouseBillOfLading, "SCACHB2134234", header2.HumanReadableName);
			bill.BB_BillNum = "SCACHB2134234";
			AssertHasMessageError(bill.BB_BillNumInfo, messageError);
			bill.BB_BillNum = "HB2186446";
			AssertNoMessageError(bill.BB_BillNumInfo, messageError);
			bill2.BB_BillNum = "MWB2567456";
			bill.BB_BillNum = "SCACHB2134234";
			AssertHasMessageError("SCAC plus HB should be unique", bill.BB_BillNumInfo, messageError);
			bill.BB_BillNum = "SCACHB2134233";
			AssertNoMessageError(bill.BB_BillNumInfo, messageError);
			bill.BB_BillType = BillTypeList.Codes.MasterBillOfLading;
			bill.BB_BillNum = "MWB2134234";
			AssertNoWarnings(bill.BB_BillNumInfo);
			bill.BB_BillNum = "MWB2134867";
			AssertNoWarnings(bill.BB_BillNumInfo);
			bill.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			messageError = ValidationConstants.Bill.ReferenceDataAlreadyExistsOnAnotherISF(BillTypeList.Descriptions.OceanBillOfLading, "OCB2134234", header2.HumanReadableName);
			bill.BB_BillNum = "OCB2134234";
			AssertHasMessageError(bill.BB_BillNumInfo, messageError);
			bill.BB_BillNum = "OCB2138687";
			AssertNoMessageError(bill.BB_BillNumInfo, messageError);
		}

		public void TestCheckBB_BillNum_ValidCharacters()
		{
			var list = new BillTypeList();
			list.RemoveCode(BillTypeList.Codes.FullNameOfISFImporter);
			foreach (ICodeDescription pair in list)
			{
				bill.BB_BillType = pair.Code;
				bill.BB_BillNum = "Z-Z";
				AssertHasMessageError(bill.BB_BillNumInfo, ValidationConstants.Character.OnlyAlphaNumericCharactersAreAllowed);
				bill.BB_BillNum = "Z Z";
				AssertHasMessageError(bill.BB_BillNumInfo, ValidationConstants.Character.OnlyAlphaNumericCharactersAreAllowed);
				bill.BB_BillNum = "Z1Z";
				AssertNoMessageError(bill.BB_BillNumInfo, ValidationConstants.Character.OnlyAlphaNumericCharactersAreAllowed);
			}

			bill.BB_BillType = BillTypeList.Codes.FullNameOfISFImporter;
			bill.BB_BillNum = "IMPORTR*NAME";
			var message = @"Reference Data : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'.";
			AssertHasWarningContaining(bill.BB_BillNumInfo, message);
			bill.BB_BillNum = "IMPORTR�NAME";
			AssertHasWarningContaining(bill.BB_BillNumInfo, message);
		}

		public void TestCarnetReferenceRightFormat()
		{
			bill.BB_BillType = BillTypeList.Codes.CarnetIssuingCountryCodeAndCarnetNumber;
			bill.BB_BillNum = ZString.Empty;
			AssertHasMessageErrorContaining(bill.BB_BillNumInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(bill.BB_BillNumInfo, ValidationConstants.Bill.CarnetReferenceRightFormat);
			AssertNoMessageError(bill.BB_BillNumInfo, ValidationConstants.Bill.CarnetReferenceInvalidCountryCode);
			bill.BB_BillNum = "Z1";
			AssertNoMessageErrorContaining(bill.BB_BillNumInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(bill.BB_BillNumInfo, ValidationConstants.Bill.CarnetReferenceRightFormat);
			AssertNoMessageError(bill.BB_BillNumInfo, ValidationConstants.Bill.CarnetReferenceInvalidCountryCode);
			bill.BB_BillNum = "Z1-";
			AssertNoMessageErrorContaining(bill.BB_BillNumInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(bill.BB_BillNumInfo, ValidationConstants.Bill.CarnetReferenceRightFormat);
			AssertNoMessageError(bill.BB_BillNumInfo, ValidationConstants.Bill.CarnetReferenceInvalidCountryCode);
			bill.BB_BillNum = "Z11";
			AssertNoMessageErrorContaining(bill.BB_BillNumInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(bill.BB_BillNumInfo, ValidationConstants.Bill.CarnetReferenceRightFormat);
			AssertHasMessageError(bill.BB_BillNumInfo, ValidationConstants.Bill.CarnetReferenceInvalidCountryCode);
			bill.BB_BillNum = "AU1";
			AssertNoMessageErrorContaining(bill.BB_BillNumInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(bill.BB_BillNumInfo, ValidationConstants.Bill.CarnetReferenceRightFormat);
			AssertNoMessageError(bill.BB_BillNumInfo, ValidationConstants.Bill.CarnetReferenceInvalidCountryCode);
		}

		public void TestSuretyCodeRightFormat()
		{
			bill.BB_BillType = BillTypeList.Codes.SuretyCode;
			bill.BB_BillNum = ZString.Empty;
			AssertNoMessageError(bill.BB_BillNumInfo, ValidationConstants.Bill.SuretyCodeRightFormat);
			bill.BB_BillNum = "7911";
			AssertHasMessageError(bill.BB_BillNumInfo, ValidationConstants.Bill.SuretyCodeRightFormat);
			bill.BB_BillNum = "791";
			AssertNoMessageError(bill.BB_BillNumInfo, ValidationConstants.Bill.SuretyCodeRightFormat);
			bill.BB_BillNum = "7 1";
			AssertHasMessageError(bill.BB_BillNumInfo, ValidationConstants.Bill.SuretyCodeRightFormat);
			bill.BB_BillNum = "7A1";
			AssertHasMessageError(bill.BB_BillNumInfo, ValidationConstants.Bill.SuretyCodeRightFormat);
		}

		public void TestFullNameOfISFImporterFormat()
		{
			bill.BB_BillType = BillTypeList.Codes.FullNameOfISFImporter;
			bill.BB_BillNum = "BOB SMITH";
			AssertNoNotifications(bill.BB_BillNumInfo);
			bill.BB_BillNum = "BOB, SMITH";
			AssertNoNotifications(bill.BB_BillNumInfo);
			AssertNoMessageErrorContaining(bill.BB_BillNumInfo, MandatoryValidation.YouHaveNotEntered);
			bill.BB_BillNum = "";
			AssertHasNotifications(bill.BB_BillNumInfo);
			AssertHasMessageErrorContaining(bill.BB_BillNumInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestHouseBillOfLadingMaxLength()
		{
			AssertMaxLength(BillTypeList.Codes.HouseBillOfLading, 16, ValidationConstants.Bill.BillNumberMaxLength(BillTypeList.Descriptions.HouseBillOfLading, 16));
		}

		public void TestOceanBillOfLadingMaxLength()
		{
			AssertMaxLength(BillTypeList.Codes.OceanBillOfLading, 16, ValidationConstants.Bill.BillNumberMaxLength(BillTypeList.Descriptions.OceanBillOfLading, 16));
		}

		public void TestMasterBillOfLadingMaxLength()
		{
			AssertMaxLength(BillTypeList.Codes.MasterBillOfLading, 16, ValidationConstants.Bill.BillNumberMaxLength(BillTypeList.Descriptions.MasterBillOfLading, 16));
		}

		void AssertMaxLength(ZString billType, int maxLength, ZString messageError)
		{
			bill.BB_BillType = billType;
			bill.BB_BillNum = ZString.Empty;
			AssertNoMessageError(bill.BB_BillNumInfo, messageError);
			bill.BB_BillNum = new ZString('A', maxLength + 1);
			AssertHasMessageError(bill.BB_BillNumInfo, messageError);
			bill.BB_BillNum = new ZString('A', maxLength);
			AssertNoMessageError(bill.BB_BillNumInfo, messageError);
		}

		CusISFBill bill;
		CusISFHeader header;
		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusISFHeader>();
			bill = header.ReferenceDatas.AddNew();
		}
	}
}
