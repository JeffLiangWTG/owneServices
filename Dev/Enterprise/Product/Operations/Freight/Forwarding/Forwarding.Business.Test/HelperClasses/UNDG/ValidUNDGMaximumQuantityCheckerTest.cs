using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ValidUNDGMaximumQuantityCheckerTest : TestCaseWithFactory
	{
		public void TestDoesDGWeightExceedLimitedQuantityLimit()
		{
			var dangerousGood1 = Factory.New<ForwardingUNDGDataItem>();
			var substance1 = Factory.New<UNDGSubstance>();
			substance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance1.DG_Code = "1001";
			substance1.DG_UNNO = "1001";
			substance1.DG_LQMaxAmt = 50;
			substance1.DG_LQMaxAmtUQ = "t";
			dangerousGood1.DI_DG = substance1.PK;
			dangerousGood1.DI_UnitOfWeight = "KG";
			dangerousGood1.DI_DGWeight = 100000;
			dangerousGood1.DI_PackageCount = 2;
			dangerousGood1.Validation.ValidateDI_OC_DGContact();

			var isLimitedQuantityLimit1 = ValidUNDGMaximumQuantityChecker.DoesDGWeightExceedLimitedQuantityLimit(dangerousGood1);

			AssertEquals(false, isLimitedQuantityLimit1);

			var dangerousGood2 = Factory.New<ForwardingUNDGDataItem>();
			var substance2 = Factory.New<UNDGSubstance>();
			substance2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance2.DG_Code = "1001";
			substance2.DG_UNNO = "1001";
			substance2.DG_LQMaxAmt = 49;
			substance2.DG_LQMaxAmtUQ = "kg";
			dangerousGood2.DI_DG = substance2.PK;
			dangerousGood2.DI_UnitOfWeight = "T";
			dangerousGood2.DI_DGWeight = 1;
			dangerousGood2.DI_PackageCount = 20;
			dangerousGood2.Validation.ValidateDI_OC_DGContact();

			var isLimitedQuantityLimit2 = ValidUNDGMaximumQuantityChecker.DoesDGWeightExceedLimitedQuantityLimit(dangerousGood2);

			AssertEquals(true, isLimitedQuantityLimit2);

			dangerousGood2.DI_PackageCount = 0;

			var isLimitedQuantityLimit3 = ValidUNDGMaximumQuantityChecker.DoesDGWeightExceedLimitedQuantityLimit(dangerousGood2);

			AssertEquals(true, isLimitedQuantityLimit3);

			dangerousGood2.DI_DGWeight = 0;

			var isLimitedQuantityLimit4 = ValidUNDGMaximumQuantityChecker.DoesDGWeightExceedLimitedQuantityLimit(dangerousGood2);

			AssertEquals(false, isLimitedQuantityLimit4);
		}

		public void TestDoesDGWeightExceedPassengerAndCargoLimit()
		{
			var dangerousGood1 = Factory.New<ForwardingUNDGDataItem>();
			var substance1 = Factory.New<UNDGSubstance>();
			substance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance1.DG_Code = "1001";
			substance1.DG_UNNO = "1001";
			substance1.DG_LQ2OrPaxMaxAmt = 50;
			substance1.DG_LQ2OrPaxMaxAmtUQ = "t";
			dangerousGood1.DI_DG = substance1.PK;
			dangerousGood1.DI_UnitOfWeight = "KG";
			dangerousGood1.DI_DGWeight = 100000;
			dangerousGood1.DI_PackageCount = 2;
			dangerousGood1.Validation.ValidateDI_OC_DGContact();

			var isExceedPassengerAndCargoLimit1 = ValidUNDGMaximumQuantityChecker.DoesDGWeightExceedPassengerAndCargoLimit(dangerousGood1);

			AssertEquals(false, isExceedPassengerAndCargoLimit1);

			var dangerousGood2 = Factory.New<ForwardingUNDGDataItem>();
			var substance2 = Factory.New<UNDGSubstance>();
			substance2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance2.DG_Code = "1001";
			substance2.DG_UNNO = "1001";
			substance2.DG_LQ2OrPaxMaxAmt = 49;
			substance2.DG_LQ2OrPaxMaxAmtUQ = "kg";
			dangerousGood2.DI_DG = substance2.PK;
			dangerousGood2.DI_UnitOfWeight = "T";
			dangerousGood2.DI_DGWeight = 1;
			dangerousGood2.DI_PackageCount = 20;
			dangerousGood2.Validation.ValidateDI_OC_DGContact();

			var isExceedPassengerAndCargoLimit2 = ValidUNDGMaximumQuantityChecker.DoesDGWeightExceedPassengerAndCargoLimit(dangerousGood2);

			AssertEquals(true, isExceedPassengerAndCargoLimit2);

			dangerousGood2.DI_PackageCount = 0;

			var isLimitedQuantityLimit3 = ValidUNDGMaximumQuantityChecker.DoesDGWeightExceedPassengerAndCargoLimit(dangerousGood2);

			AssertEquals(true, isLimitedQuantityLimit3);

			dangerousGood2.DI_DGWeight = 0;

			var isLimitedQuantityLimit4 = ValidUNDGMaximumQuantityChecker.DoesDGWeightExceedPassengerAndCargoLimit(dangerousGood2);

			AssertEquals(false, isLimitedQuantityLimit4);
		}

		public void TestDoesDGWeightExceedCargoLimit()
		{
			var dangerousGood1 = Factory.New<ForwardingUNDGDataItem>();
			var substance1 = Factory.New<UNDGSubstance>();
			substance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance1.DG_CargoPackIns = LithiumBatteryConstants.RefPackingInstructions.Forbidden;
			substance1.DG_Code = "1001";
			substance1.DG_UNNO = "1001";
			substance1.DG_CargoMaxAmt = 50;
			substance1.DG_CargoMaxAmtUQ = "t";
			dangerousGood1.DI_DG = substance1.PK;
			dangerousGood1.DI_UnitOfWeight = "KG";
			dangerousGood1.DI_DGWeight = 100000;
			dangerousGood1.DI_PackageCount = 2;
			dangerousGood1.Validation.ValidateDI_OC_DGContact();
			AssertEquals(false, ValidUNDGMaximumQuantityChecker.DoesDGWeightExceedCargoLimit(dangerousGood1));

			var dangerousGood2 = Factory.New<ForwardingUNDGDataItem>();
			var substance2 = Factory.New<UNDGSubstance>();
			substance2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance2.DG_CargoPackIns = "111";
			substance2.DG_Code = "1002";
			substance2.DG_UNNO = "1002";
			substance2.DG_CargoMaxAmt = 50;
			substance2.DG_CargoMaxAmtUQ = "t";
			dangerousGood2.DI_DG = substance2.PK;
			dangerousGood2.DI_UnitOfWeight = "KG";
			dangerousGood2.DI_DGWeight = 100000;
			dangerousGood2.DI_PackageCount = 2;
			dangerousGood2.Validation.ValidateDI_OC_DGContact();
			AssertEquals(false, ValidUNDGMaximumQuantityChecker.DoesDGWeightExceedCargoLimit(dangerousGood2));

			var dangerousGood3 = Factory.New<ForwardingUNDGDataItem>();
			var substance3 = Factory.New<UNDGSubstance>();
			substance3.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance3.DG_CargoPackIns = LithiumBatteryConstants.RefPackingInstructions.PI966;
			substance3.DG_Code = "1003";
			substance3.DG_UNNO = "1003";
			substance3.DG_CargoMaxAmt = 49;
			substance3.DG_CargoMaxAmtUQ = "KG";
			dangerousGood3.DI_DG = substance3.PK;
			dangerousGood3.DI_UnitOfWeight = "T";
			dangerousGood3.DI_DGWeight = 1;
			dangerousGood3.DI_PackageCount = 20;
			dangerousGood3.DI_PackingInstructionSection = "II";
			dangerousGood3.Validation.ValidateDI_OC_DGContact();

			var isExceedCargoLimit2 = ValidUNDGMaximumQuantityChecker.DoesDGWeightExceedCargoLimit(dangerousGood3);
			AssertEquals(true, isExceedCargoLimit2);

			dangerousGood3.DI_PackageCount = 0;
			AssertEquals(true, ValidUNDGMaximumQuantityChecker.DoesDGWeightExceedCargoLimit(dangerousGood3));

			dangerousGood3.DI_DGWeight = 0;
			AssertEquals(false, ValidUNDGMaximumQuantityChecker.DoesDGWeightExceedCargoLimit(dangerousGood3));

			dangerousGood3.DI_PackageCount = 2;
			dangerousGood3.DI_DGWeight = 20;
			dangerousGood3.DI_UnitOfWeight = "KG";
			AssertEquals(true, ValidUNDGMaximumQuantityChecker.DoesDGWeightExceedCargoLimit(dangerousGood3));
		}

		public void TestDoesDGVolumeExceedLimitedQuantityLimit()
		{
			var dangerousGood1 = Factory.New<ForwardingUNDGDataItem>();
			var substance1 = Factory.New<UNDGSubstance>();
			substance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance1.DG_Code = "1001";
			substance1.DG_UNNO = "1001";
			substance1.DG_LQMaxAmt = 50;
			substance1.DG_LQMaxAmtUQ = "GA";
			dangerousGood1.DI_DG = substance1.PK;
			dangerousGood1.DI_UnitOfVolume = "L";
			dangerousGood1.DI_DGVolume = 378;
			dangerousGood1.DI_PackageCount = 2;
			dangerousGood1.Validation.ValidateDI_OC_DGContact();

			var isExceedLimitedQuantityLimit1 = ValidUNDGMaximumQuantityChecker.DoesDGVolumeExceedLimitedQuantityLimit(dangerousGood1);

			AssertEquals(false, isExceedLimitedQuantityLimit1);

			var dangerousGood2 = Factory.New<ForwardingUNDGDataItem>();
			var substance2 = Factory.New<UNDGSubstance>();
			substance2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance2.DG_Code = "1001";
			substance2.DG_UNNO = "1001";
			substance2.DG_LQMaxAmt = 50;
			substance2.DG_LQMaxAmtUQ = "GA";
			dangerousGood2.DI_DG = substance2.PK;
			dangerousGood2.DI_UnitOfVolume = "L";
			dangerousGood2.DI_DGVolume = 379;
			dangerousGood2.DI_PackageCount = 2;
			dangerousGood2.Validation.ValidateDI_OC_DGContact();

			var isExceedLimitedQuantityLimit2 = ValidUNDGMaximumQuantityChecker.DoesDGVolumeExceedLimitedQuantityLimit(dangerousGood2);

			AssertEquals(true, isExceedLimitedQuantityLimit2);

			dangerousGood2.DI_PackageCount = 0;

			var isLimitedQuantityLimit3 = ValidUNDGMaximumQuantityChecker.DoesDGVolumeExceedLimitedQuantityLimit(dangerousGood2);

			AssertEquals(true, isLimitedQuantityLimit3);

			dangerousGood2.DI_DGVolume = 0;

			var isLimitedQuantityLimit4 = ValidUNDGMaximumQuantityChecker.DoesDGVolumeExceedLimitedQuantityLimit(dangerousGood2);

			AssertEquals(false, isLimitedQuantityLimit4);
		}

		public void TestDoesDGVolumeExceedPassengerAndCargoLimit()
		{
			var dangerousGood1 = Factory.New<ForwardingUNDGDataItem>();
			var substance1 = Factory.New<UNDGSubstance>();
			substance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance1.DG_Code = "1001";
			substance1.DG_UNNO = "1001";
			substance1.DG_LQ2OrPaxMaxAmt = 50;
			substance1.DG_LQ2OrPaxMaxAmtUQ = "GA";
			dangerousGood1.DI_DG = substance1.PK;
			dangerousGood1.DI_UnitOfVolume = "L";
			dangerousGood1.DI_DGVolume = 378;
			dangerousGood1.DI_PackageCount = 2;
			dangerousGood1.Validation.ValidateDI_OC_DGContact();

			var isExceedPassengerAndCargoLimit1 = ValidUNDGMaximumQuantityChecker.DoesDGVolumeExceedPassengerAndCargoLimit(dangerousGood1);

			AssertEquals(false, isExceedPassengerAndCargoLimit1);

			var dangerousGood2 = Factory.New<ForwardingUNDGDataItem>();
			var substance2 = Factory.New<UNDGSubstance>();
			substance2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance2.DG_Code = "1001";
			substance2.DG_UNNO = "1001";
			substance2.DG_LQ2OrPaxMaxAmt = 50;
			substance2.DG_LQ2OrPaxMaxAmtUQ = "GA";
			dangerousGood2.DI_DG = substance2.PK;
			dangerousGood2.DI_UnitOfVolume = "L";
			dangerousGood2.DI_DGVolume = 379;
			dangerousGood2.DI_PackageCount = 2;
			dangerousGood2.Validation.ValidateDI_OC_DGContact();

			var isExceedPassengerAndCargoLimit2 = ValidUNDGMaximumQuantityChecker.DoesDGVolumeExceedPassengerAndCargoLimit(dangerousGood2);

			AssertEquals(true, isExceedPassengerAndCargoLimit2);

			dangerousGood2.DI_PackageCount = 0;

			var isLimitedQuantityLimit3 = ValidUNDGMaximumQuantityChecker.DoesDGVolumeExceedPassengerAndCargoLimit(dangerousGood2);

			AssertEquals(true, isLimitedQuantityLimit3);

			dangerousGood2.DI_DGVolume = 0;

			var isLimitedQuantityLimit4 = ValidUNDGMaximumQuantityChecker.DoesDGVolumeExceedPassengerAndCargoLimit(dangerousGood2);

			AssertEquals(false, isLimitedQuantityLimit4);
		}

		public void TestDoesDGVolumeExceedCargoLimit()
		{
			var dangerousGood1 = Factory.New<ForwardingUNDGDataItem>();
			var substance1 = Factory.New<UNDGSubstance>();
			substance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance1.DG_Code = "1001";
			substance1.DG_UNNO = "1001";
			substance1.DG_CargoMaxAmt = 50;
			substance1.DG_CargoMaxAmtUQ = "GA";
			dangerousGood1.DI_DG = substance1.PK;
			dangerousGood1.DI_UnitOfVolume = "L";
			dangerousGood1.DI_DGVolume = 378;
			dangerousGood1.DI_PackageCount = 2;
			dangerousGood1.Validation.ValidateDI_OC_DGContact();

			var isExceedCargoLimit1 = ValidUNDGMaximumQuantityChecker.DoesDGVolumeExceedCargoLimit(dangerousGood1);

			AssertEquals(false, isExceedCargoLimit1);

			var dangerousGood2 = Factory.New<ForwardingUNDGDataItem>();
			var substance2 = Factory.New<UNDGSubstance>();
			substance2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance2.DG_Code = "1001";
			substance2.DG_UNNO = "1001";
			substance2.DG_CargoMaxAmt = 50;
			substance2.DG_CargoMaxAmtUQ = "GA";
			dangerousGood2.DI_DG = substance2.PK;
			dangerousGood2.DI_UnitOfVolume = "L";
			dangerousGood2.DI_DGVolume = 379;
			dangerousGood2.DI_PackageCount = 2;
			dangerousGood2.Validation.ValidateDI_OC_DGContact();

			var isExceedCargoLimit2 = ValidUNDGMaximumQuantityChecker.DoesDGVolumeExceedCargoLimit(dangerousGood2);

			AssertEquals(true, isExceedCargoLimit2);

			dangerousGood2.DI_PackageCount = 0;

			var isLimitedQuantityLimit3 = ValidUNDGMaximumQuantityChecker.DoesDGVolumeExceedCargoLimit(dangerousGood2);

			AssertEquals(true, isLimitedQuantityLimit3);

			dangerousGood2.DI_DGVolume = 0;

			var isLimitedQuantityLimit4 = ValidUNDGMaximumQuantityChecker.DoesDGVolumeExceedCargoLimit(dangerousGood2);

			AssertEquals(false, isLimitedQuantityLimit4);
		}
	}
}
