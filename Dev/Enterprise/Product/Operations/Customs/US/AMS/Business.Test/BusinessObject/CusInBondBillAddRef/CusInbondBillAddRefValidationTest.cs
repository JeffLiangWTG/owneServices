using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	sealed class CusInbondBillAddRefValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBR_Qualifier()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			bill.ValidationModes = ValidationModes.InventoryRecord;
			var reference = bill.ShipmentReferenceDetails.AddNew();
			reference.BR_Qualifier = ZString.Empty;
			AssertHasMessageErrorContaining(reference.BR_QualifierInfo, MandatoryValidation.YouHaveNotEntered);
			reference.BR_Qualifier = "Z!";
			AssertNoMessageErrorContaining(reference.BR_QualifierInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(reference.BR_QualifierInfo, ListValidation.InvalidCodeMessageError);
			foreach (CodeDescriptionPair pair in BillReferenceList.GetCachedValue(Factory))
			{
				reference.BR_Qualifier = pair.Code;
				AssertNoMessageErrorContaining(reference.BR_QualifierInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(reference.BR_QualifierInfo, ListValidation.InvalidCodeMessageError);
			}

			bill.ValidationModes = ValidationModes.None;
			reference.BR_Qualifier = ZString.Empty;
			AssertNoMessageErrorContaining(reference.BR_QualifierInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(reference.BR_QualifierInfo, ListValidation.InvalidCodeMessageError);
			reference.BR_Qualifier = "Z!";
			AssertNoMessageErrorContaining(reference.BR_QualifierInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(reference.BR_QualifierInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBR_ReferenceNum()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			bill.ValidationModes = ValidationModes.InventoryRecord;
			var reference = bill.ShipmentReferenceDetails.AddNew();
			reference.BR_ReferenceNum = ZString.Empty;
			AssertHasMessageErrorContaining(reference.BR_ReferenceNumInfo, MandatoryValidation.YouHaveNotEntered);
			reference.BR_Qualifier = "OB";
			reference.BR_ReferenceNum = "HB2";
			AssertNoMessageErrorContaining(reference.BR_ReferenceNumInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(reference.BR_ReferenceNumInfo, "Bill Of Lading should be 5 to 16 characters; the first 4 characters is the SCAC of the issuer of the Bill Of Lading.");
			reference.BR_ReferenceNum = "HB23423";
			AssertNoMessageError(reference.BR_ReferenceNumInfo, "Bill Of Lading should be 5 to 16 characters; the first 4 characters is the SCAC of the issuer of the Bill Of Lading.");
			AssertHasMessageError(reference.BR_ReferenceNumInfo, "The Standard Carrier Alpha Code (SCAC) 'HB23' is not valid.");
			reference.BR_ReferenceNum = "AABC12345";
			Assert(!reference.BR_ReferenceNumInfo.HasMessageError("The Standard Carrier Alpha Code (SCAC) 'AABC' is not valid."));
			bill.ValidationModes = ValidationModes.None;
			reference.BR_ReferenceNum = ZString.Empty;
			AssertNoMessageErrorContaining(reference.BR_ReferenceNumInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ValidationModes = ValidationModes.InventoryRecord;
			reference.BR_ReferenceNum = "ATT*TT";
			AssertHasMessageErrorContaining(reference.BR_ReferenceNumInfo, "The Standard Carrier Alpha Code (SCAC) 'ATT*' is not valid.");
		}

		public void TestBillOfLadingFormat()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			bill.ValidationModes = ValidationModes.InventoryRecord;
			var reference = bill.ShipmentReferenceDetails.AddNew();
			foreach (var code in new[] { BillReferenceList.Codes.OB, BillReferenceList.Codes.OL })
			{
				reference.BR_Qualifier = code;
				reference.BR_ReferenceNum = ZString.Empty;
				AssertHasMessageErrorContaining(reference.BR_ReferenceNumInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageError(reference.BR_ReferenceNumInfo, ValidationConstants.ShipmentReference.BillOfLadingFormat.ToString());
				reference.BR_ReferenceNum = "1234";
				AssertNoMessageErrorContaining(reference.BR_ReferenceNumInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageError(reference.BR_ReferenceNumInfo, ValidationConstants.ShipmentReference.BillOfLadingFormat.ToString());
				reference.BR_ReferenceNum = "12345";
				AssertNoMessageErrorContaining(reference.BR_ReferenceNumInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageError(reference.BR_ReferenceNumInfo, ValidationConstants.ShipmentReference.BillOfLadingFormat.ToString());
				reference.BR_ReferenceNum = "12345678901234567";
				AssertNoMessageErrorContaining(reference.BR_ReferenceNumInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageError(reference.BR_ReferenceNumInfo, ValidationConstants.ShipmentReference.BillOfLadingFormat.ToString());
				reference.BR_ReferenceNum = "1234567890123456";
				AssertNoMessageErrorContaining(reference.BR_ReferenceNumInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageError(reference.BR_ReferenceNumInfo, ValidationConstants.ShipmentReference.BillOfLadingFormat.ToString());
			}
		}

		public void TestCensusScheduleKFormat()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "12#34", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			bill.ValidationModes = ValidationModes.InventoryRecord;
			var reference = bill.ShipmentReferenceDetails.AddNew();
			reference.BR_Qualifier = BillReferenceList.Codes.CSK;
			reference.BR_ReferenceNum = ZString.Empty;
			AssertHasMessageErrorContaining(reference.BR_ReferenceNumInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(reference.BR_ReferenceNumInfo, ValidationConstants.ShipmentReference.CensusScheduleK);
			reference.BR_ReferenceNum = "@!@#1";
			AssertNoMessageErrorContaining(reference.BR_ReferenceNumInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(reference.BR_ReferenceNumInfo, ValidationConstants.ShipmentReference.CensusScheduleK);
			reference.BR_ReferenceNum = "12#34";
			AssertNoMessageErrorContaining(reference.BR_ReferenceNumInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(reference.BR_ReferenceNumInfo, ValidationConstants.ShipmentReference.CensusScheduleK);
		}

		public void TestUNLOCODEFormat()
		{
			var port = Factory.New<RefUNLOCO>();
			port.RL_Code = "12#34";
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			bill.ValidationModes = ValidationModes.InventoryRecord;
			var reference = bill.ShipmentReferenceDetails.AddNew();
			reference.BR_Qualifier = BillReferenceList.Codes.ULC;
			reference.BR_ReferenceNum = ZString.Empty;
			AssertHasMessageErrorContaining(reference.BR_ReferenceNumInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(reference.BR_ReferenceNumInfo, ValidationConstants.ShipmentReference.UNLOCode);
			reference.BR_ReferenceNum = "@!@#1";
			AssertNoMessageErrorContaining(reference.BR_ReferenceNumInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(reference.BR_ReferenceNumInfo, ValidationConstants.ShipmentReference.UNLOCode);
			reference.BR_ReferenceNum = "12#34";
			AssertNoMessageErrorContaining(reference.BR_ReferenceNumInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(reference.BR_ReferenceNumInfo, ValidationConstants.ShipmentReference.UNLOCode);
		}

		public void TestPedimentoNumberFormat()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			bill.ValidationModes = ValidationModes.InventoryRecord;
			var reference = bill.ShipmentReferenceDetails.AddNew();
			reference.BR_Qualifier = BillReferenceList.Codes.FEN;
			reference.BR_ReferenceNum = ZString.Empty;
			AssertHasMessageErrorContaining(reference.BR_ReferenceNumInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(reference.BR_ReferenceNumInfo, PedimentoNumberValidator.PedimentoNumberRightFormat);
			reference.BR_ReferenceNum = "12859658123";
			AssertNoMessageErrorContaining(reference.BR_ReferenceNumInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(reference.BR_ReferenceNumInfo, PedimentoNumberValidator.PedimentoNumberRightFormat);
			reference.BR_ReferenceNum = "128596581234567";
			AssertNoMessageErrorContaining(reference.BR_ReferenceNumInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(reference.BR_ReferenceNumInfo, PedimentoNumberValidator.PedimentoNumberRightFormat);
		}
	}
}
