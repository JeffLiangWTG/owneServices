using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Customs.US.Messaging.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class EntryTypeListTest : TestCaseWithFactory
	{
		public void TestIsQuotaProductExclusionType()
		{
			Assert(EntryTypeList.IsQuotaProductExclusionType(EntryTypeList.Codes.ConsumptionQuotaVisa));
			Assert(EntryTypeList.IsQuotaProductExclusionType(EntryTypeList.Codes.ConsumptionFTZ));
			Assert(EntryTypeList.IsQuotaProductExclusionType(EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa));
			Assert(EntryTypeList.IsQuotaProductExclusionType(EntryTypeList.Codes.InformalQuotaVisa));
			Assert(EntryTypeList.IsQuotaProductExclusionType(EntryTypeList.Codes.TemporaryImportationBond));
			Assert(EntryTypeList.IsQuotaProductExclusionType(EntryTypeList.Codes.WarehouseWithdrawalQuota));
			Assert(EntryTypeList.IsQuotaProductExclusionType(EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa));
			Assert(EntryTypeList.IsQuotaProductExclusionType(EntryTypeList.Codes.GovernmentDutiable));
		}

		public void TestIsEntryTypeForAPHISNumberExempt()
		{
			Assert(EntryTypeList.IsEntryTypeForAPHISNumberExempt(EntryTypeList.Codes.InformalFreeDutiable));
			Assert(EntryTypeList.IsEntryTypeForAPHISNumberExempt(EntryTypeList.Codes.InformalQuotaVisa));
			Assert(EntryTypeList.IsEntryTypeForAPHISNumberExempt(EntryTypeList.Codes.LowValue));
			Assert(!EntryTypeList.IsEntryTypeForAPHISNumberExempt(EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa));
			Assert(!EntryTypeList.IsEntryTypeForAPHISNumberExempt(EntryTypeList.Codes.GovernmentDutiable));
		}

		public void TestIsInformalImportWarningOrError()
		{
			Assert(EntryTypeList.IsInformalImportWarningOrError(EntryTypeList.Codes.ConsumptionFreeDutiable));
			Assert(EntryTypeList.IsInformalImportWarningOrError(EntryTypeList.Codes.ConsumptionQuotaVisa));
		}

		public void TestIsWarehouseRelatedExceptFTZ()
		{
			Assert(EntryTypeList.IsWarehouseRelatedExceptFTZ(EntryTypeList.Codes.Warehouse));
			Assert(EntryTypeList.IsWarehouseRelatedExceptFTZ(EntryTypeList.Codes.ReWarehouse));
			Assert(EntryTypeList.IsWarehouseRelatedExceptFTZ(EntryTypeList.Codes.WarehouseWithdrawalConsumption));
			Assert(EntryTypeList.IsWarehouseRelatedExceptFTZ(EntryTypeList.Codes.WarehouseWithdrawalQuota));
			Assert(EntryTypeList.IsWarehouseRelatedExceptFTZ(EntryTypeList.Codes.WarehouseWithdrawalADDCVD));
			Assert(EntryTypeList.IsWarehouseRelatedExceptFTZ(EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa));
		}

		public void TestHasCargoEnteredUSTerritory()
		{
			Assert(EntryTypeList.HasCargoEnteredUSTerritory(EntryTypeList.Codes.ReWarehouse));
			Assert(!EntryTypeList.HasCargoEnteredUSTerritory(EntryTypeList.Codes.ConsumptionADDCVD));
			Assert(EntryTypeList.HasCargoEnteredUSTerritory(EntryTypeList.Codes.WarehouseWithdrawalADDCVD));
			Assert(!EntryTypeList.HasCargoEnteredUSTerritory(EntryTypeList.Codes.Warehouse));
			Assert(!EntryTypeList.HasCargoEnteredUSTerritory(EntryTypeList.Codes.WarehouseFTZ));
		}

		public void TestIsValidForADD_CVD()
		{
			AssertEquals(true, EntryTypeList.IsValidForADD_CVD(EntryTypeList.Codes.ConsumptionADDCVD));
			AssertEquals(false, EntryTypeList.IsValidForADD_CVD(EntryTypeList.Codes.TradeFair));
			AssertEquals(true, EntryTypeList.IsValidForADD_CVD(EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa));
			AssertEquals(false, EntryTypeList.IsValidForADD_CVD(EntryTypeList.Codes.ImmediateTransportation));
			AssertEquals(true, EntryTypeList.IsValidForADD_CVD(EntryTypeList.Codes.WarehouseWithdrawalADDCVD));
			AssertEquals(false, EntryTypeList.IsValidForADD_CVD(EntryTypeList.Codes.ConsumptionFreeDutiable));
			AssertEquals(true, EntryTypeList.IsValidForADD_CVD(EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa));
			AssertEquals(false, EntryTypeList.IsValidForADD_CVD(EntryTypeList.Codes.DCASR));
			AssertEquals(true, EntryTypeList.IsValidForADD_CVD(EntryTypeList.Codes.ConsumptionFTZ));
			AssertEquals("For TIB, ADD/CVD details won't be sent in messages. It is there for calculation of bond amount for TIB.", true, EntryTypeList.IsValidForADD_CVD(EntryTypeList.Codes.TemporaryImportationBond));
		}

		public void TestIsTIB()
		{
			AssertEquals(false, EntryTypeList.IsTIB(EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa));
			AssertEquals(false, EntryTypeList.IsTIB(EntryTypeList.Codes.WarehouseWithdrawalADDCVD));
			AssertEquals(false, EntryTypeList.IsTIB(EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa));
			AssertEquals(false, EntryTypeList.IsTIB(EntryTypeList.Codes.ConsumptionADDCVD));
			AssertEquals(false, EntryTypeList.IsTIB(EntryTypeList.Codes.ConsumptionFTZ));
			AssertEquals(true, EntryTypeList.IsTIB(EntryTypeList.Codes.TemporaryImportationBond));
		}

		public void TestIsCustomsChargePayable()
		{
			AssertEquals(true, EntryTypeList.IsCustomsChargeIncludedInTotalAmountDue(EntryTypeList.Codes.ConsumptionFreeDutiable, Core.Constants.USCustoms.FeeCodes.Duty));
			AssertEquals(false, EntryTypeList.IsCustomsChargeIncludedInTotalAmountDue(EntryTypeList.Codes.Warehouse, Core.Constants.USCustoms.FeeCodes.Duty));

			AssertEquals(false, EntryTypeList.IsCustomsChargeIncludedInTotalAmountDue(EntryTypeList.Codes.ConsumptionFreeDutiable, Core.Constants.USCustoms.FeeCodes.ExciseTaxDeferred));
			AssertEquals(false, EntryTypeList.IsCustomsChargeIncludedInTotalAmountDue(EntryTypeList.Codes.Warehouse, Core.Constants.USCustoms.FeeCodes.ExciseTaxDeferred));

			AssertEquals(true, EntryTypeList.IsCustomsChargeIncludedInTotalAmountDue(EntryTypeList.Codes.ConsumptionFreeDutiable, Core.Constants.USCustoms.FeeCodes.HMF));
			AssertEquals(true, EntryTypeList.IsCustomsChargeIncludedInTotalAmountDue(EntryTypeList.Codes.Warehouse, Core.Constants.USCustoms.FeeCodes.HMF));
		}

		public void TestIsAgriculturalEntryTypes()
		{
			Assert(EntryTypeList.IsAgriculturalLicenseEntryTypes(EntryTypeList.Codes.ConsumptionQuotaVisa));
			Assert(EntryTypeList.IsAgriculturalLicenseEntryTypes(EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa));
			Assert(EntryTypeList.IsAgriculturalLicenseEntryTypes(EntryTypeList.Codes.ConsumptionFTZ));
			Assert(EntryTypeList.IsAgriculturalLicenseEntryTypes(EntryTypeList.Codes.Warehouse));
			Assert(EntryTypeList.IsAgriculturalLicenseEntryTypes(EntryTypeList.Codes.ReWarehouse));
			Assert(EntryTypeList.IsAgriculturalLicenseEntryTypes(EntryTypeList.Codes.WarehouseFTZ));
			Assert(EntryTypeList.IsAgriculturalLicenseEntryTypes(EntryTypeList.Codes.WarehouseWithdrawalConsumption));
			Assert(EntryTypeList.IsAgriculturalLicenseEntryTypes(EntryTypeList.Codes.WarehouseWithdrawalQuota));
			Assert(EntryTypeList.IsAgriculturalLicenseEntryTypes(EntryTypeList.Codes.WarehouseWithdrawalADDCVD));
			Assert(EntryTypeList.IsAgriculturalLicenseEntryTypes(EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa));
		}

		public void TestIsQuotaVisa()
		{
			Assert(EntryTypeList.IsQuotaVisa(EntryTypeList.Codes.ConsumptionQuotaVisa));
			Assert(EntryTypeList.IsQuotaVisa(EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa));
			Assert(EntryTypeList.IsQuotaVisa(EntryTypeList.Codes.InformalQuotaVisa));
			Assert(EntryTypeList.IsQuotaVisa(EntryTypeList.Codes.WarehouseWithdrawalQuota));
			Assert(EntryTypeList.IsQuotaVisa(EntryTypeList.Codes.ConsumptionFTZ));
			Assert(EntryTypeList.IsQuotaVisa(EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa));
		}

		public void TestIsGovernmentSpecific()
		{
			AssertEquals(true, EntryTypeList.IsGovernmentSpecific(EntryTypeList.Codes.DCASR));
			AssertEquals(true, EntryTypeList.IsGovernmentSpecific(EntryTypeList.Codes.GovernmentDutiable));
			AssertEquals(false, EntryTypeList.IsValidForRecon(EntryTypeList.Codes.TradeFair));
		}

		public void TestIsADD_CVDInvolved()
		{
			AssertEquals(true, EntryTypeList.IsADD_CVDInvolved(EntryTypeList.Codes.ConsumptionADDCVD));
			AssertEquals(true, EntryTypeList.IsADD_CVDInvolved(EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa));
			AssertEquals(true, EntryTypeList.IsADD_CVDInvolved(EntryTypeList.Codes.WarehouseWithdrawalADDCVD));
			AssertEquals(true, EntryTypeList.IsADD_CVDInvolved(EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa));
		}

		public void TestIsValidForRecon()
		{
			AssertEquals(true, EntryTypeList.IsValidForRecon(EntryTypeList.Codes.ConsumptionFreeDutiable));
			AssertEquals(false, EntryTypeList.IsValidForRecon(EntryTypeList.Codes.TradeFair));
			AssertEquals(true, EntryTypeList.IsValidForRecon(EntryTypeList.Codes.ConsumptionFTZ));
			AssertEquals(false, EntryTypeList.IsValidForRecon(EntryTypeList.Codes.ImmediateTransportation));
			AssertEquals(true, EntryTypeList.IsValidForRecon(EntryTypeList.Codes.ConsumptionQuotaVisa));
		}

		public void TestIsExWarehouseType()
		{
			var list = new EntryTypeList();
			var entryTypes = new string[]
			{
				EntryTypeList.Codes.WarehouseWithdrawalADDCVD,
				EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa,
				EntryTypeList.Codes.WarehouseWithdrawalConsumption,
				EntryTypeList.Codes.WarehouseWithdrawalQuota
			};

			foreach (string type in entryTypes)
			{
				AssertEquals(type, true, EntryTypeList.IsExWarehouseType(type));
				list.RemoveCode(type);
			}

			foreach (ICodeDescription pair in list)
			{
				AssertEquals(pair.Code, false, EntryTypeList.IsExWarehouseType(pair.Code));
			}

			AssertEquals(false, EntryTypeList.IsExWarehouseOrReWarehouseType(EntryTypeList.Codes.ImmediateExportation));
			AssertEquals(true, EntryTypeList.IsExWarehouseOrReWarehouseType(EntryTypeList.Codes.WarehouseWithdrawalConsumption));
			AssertEquals(true, EntryTypeList.IsExWarehouseOrReWarehouseType(EntryTypeList.Codes.ReWarehouse));
		}

		public void TestIsExWarehouseTypeOrFTZ()
		{
			var list = new EntryTypeList();
			var expectedEntryTypes = new string[]
			{
				EntryTypeList.Codes.ConsumptionFTZ,
				EntryTypeList.Codes.WarehouseWithdrawalConsumption,
				EntryTypeList.Codes.WarehouseWithdrawalQuota,
				EntryTypeList.Codes.WarehouseWithdrawalADDCVD,
				EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa
			};

			foreach (string type in expectedEntryTypes)
			{
				AssertEquals($"Code {type} should be in IsExWarehouseTypeOrFTZ", true, EntryTypeList.IsExWarehouseTypeOrFTZ(type));
				list.RemoveCode(type);
			}

			foreach (ICodeDescription pair in list)
			{
				AssertEquals($"Code {pair.Code} should NOT be in IsExWarehouseTypeOrFTZ", false, EntryTypeList.IsExWarehouseTypeOrFTZ(pair.Code));
			}
		}

		public void TestIsWarehouseOrTIB()
		{
			var list = new EntryTypeList();
			var entryTypes = new string[]
			{
				EntryTypeList.Codes.Warehouse,
				EntryTypeList.Codes.ReWarehouse,
				EntryTypeList.Codes.TemporaryImportationBond
			};

			foreach (string type in entryTypes)
			{
				AssertEquals(type, true, EntryTypeList.IsWarehouseOrTIB(type));
				list.RemoveCode(type);
			}

			foreach (ICodeDescription pair in list)
			{
				AssertEquals(pair.Code, false, EntryTypeList.IsWarehouseOrTIB(pair.Code));
			}
		}

		public void TestIsSoftwoodLumberSection803FarmBillRequired()
		{
			EntryTypeList list = new EntryTypeList();
			string[] entryTypes = new string[]
			{
				EntryTypeList.Codes.ConsumptionFreeDutiable,
				EntryTypeList.Codes.ConsumptionQuotaVisa,
				EntryTypeList.Codes.ConsumptionADDCVD,
				EntryTypeList.Codes.ConsumptionFTZ,
				EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa,
				EntryTypeList.Codes.Warehouse,
				EntryTypeList.Codes.ReWarehouse,
				EntryTypeList.Codes.WarehouseWithdrawalConsumption,
				EntryTypeList.Codes.WarehouseWithdrawalQuota,
				EntryTypeList.Codes.WarehouseWithdrawalADDCVD,
				EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa,
				EntryTypeList.Codes.TemporaryImportationBond
			};

			foreach (string type in entryTypes)
			{
				AssertEquals(type, true, EntryTypeList.IsSoftwoodLumberSection803FarmBillRequired(type));
				list.RemoveCode(type);
			}

			foreach (ICodeDescription pair in list)
			{
				AssertEquals(pair.Code, false, EntryTypeList.IsSoftwoodLumberSection803FarmBillRequired(pair.Code));
			}
		}

		public void TestIsSoftwoodLumberPermitNumberRequired()
		{
			AssertEquals(true, EntryTypeList.IsSoftwoodLumberPermitNumberRequired(EntryTypeList.Codes.ConsumptionFreeDutiable));
			AssertEquals(true, EntryTypeList.IsSoftwoodLumberPermitNumberRequired(EntryTypeList.Codes.ConsumptionQuotaVisa));
			AssertEquals(true, EntryTypeList.IsSoftwoodLumberPermitNumberRequired(EntryTypeList.Codes.ConsumptionADDCVD));
			AssertEquals(true, EntryTypeList.IsSoftwoodLumberPermitNumberRequired(EntryTypeList.Codes.ConsumptionFTZ));
			AssertEquals(true, EntryTypeList.IsSoftwoodLumberPermitNumberRequired(EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa));
			AssertEquals(true, EntryTypeList.IsSoftwoodLumberPermitNumberRequired(EntryTypeList.Codes.Warehouse));
			AssertEquals(true, EntryTypeList.IsSoftwoodLumberPermitNumberRequired(EntryTypeList.Codes.ReWarehouse));
			AssertEquals(true, EntryTypeList.IsSoftwoodLumberPermitNumberRequired(EntryTypeList.Codes.InformalFreeDutiable));
			AssertEquals(true, EntryTypeList.IsSoftwoodLumberPermitNumberRequired(EntryTypeList.Codes.InformalQuotaVisa));
			AssertEquals(false, EntryTypeList.IsSoftwoodLumberPermitNumberRequired(EntryTypeList.Codes.Baggage));
			AssertEquals(false, EntryTypeList.IsSoftwoodLumberPermitNumberRequired(EntryTypeList.Codes.Appraisement));
		}

		public void TestIsInBondEntryType()
		{
			AssertEquals(true, EntryTypeList.IsInBondEntryType(EntryTypeList.Codes.ImmediateExportation));
			AssertEquals(false, EntryTypeList.IsInBondEntryType(EntryTypeList.Codes.TradeFair));
			AssertEquals(true, EntryTypeList.IsInBondEntryType(EntryTypeList.Codes.ImmediateTransportation));
			AssertEquals(false, EntryTypeList.IsInBondEntryType(EntryTypeList.Codes.ConsumptionFreeDutiable));
			AssertEquals(true, EntryTypeList.IsInBondEntryType(EntryTypeList.Codes.TransportationExportation));
		}

		public void TestRemoveInBondEntryTypes()
		{
			EntryTypeList list = new EntryTypeList();
			AssertEquals(true, list.ContainsCode(EntryTypeList.Codes.TransportationExportation));
			AssertEquals(true, list.ContainsCode(EntryTypeList.Codes.ImmediateExportation));
			AssertEquals(true, list.ContainsCode(EntryTypeList.Codes.ImmediateTransportation));

			list.RemoveInBondEntryTypes();
			AssertEquals(false, list.ContainsCode(EntryTypeList.Codes.TransportationExportation));
			AssertEquals(false, list.ContainsCode(EntryTypeList.Codes.ImmediateExportation));
			AssertEquals(false, list.ContainsCode(EntryTypeList.Codes.ImmediateTransportation));

			AssertNoExceptionThrown(() => list.RemoveInBondEntryTypes());
		}

		public void TestIsConsumptionForNAFTARecon()
		{
			AssertEquals(true, EntryTypeList.IsConsumptionForNAFTARecon(EntryTypeList.Codes.ConsumptionFreeDutiable));
			AssertEquals(false, EntryTypeList.IsConsumptionForNAFTARecon(EntryTypeList.Codes.Appraisement));

			AssertEquals(true, EntryTypeList.IsConsumptionForNAFTARecon(EntryTypeList.Codes.ConsumptionQuotaVisa));
			AssertEquals(false, EntryTypeList.IsConsumptionForNAFTARecon(EntryTypeList.Codes.VesselRepair));

			AssertEquals(true, EntryTypeList.IsConsumptionForNAFTARecon(EntryTypeList.Codes.ConsumptionFTZ));
			AssertEquals(false, EntryTypeList.IsConsumptionForNAFTARecon(EntryTypeList.Codes.TradeFair));

			AssertEquals(true, EntryTypeList.IsConsumption(EntryTypeList.Codes.ConsumptionFreeDutiable));
			AssertEquals(true, EntryTypeList.IsConsumption(EntryTypeList.Codes.ConsumptionADDCVD));
			AssertEquals(false, EntryTypeList.IsConsumption(EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa));
		}

		public void TestIsConsumptionMXCementImportLicense()
		{
			AssertEquals(true, EntryTypeList.IsConsumptionMXCementImportLicense(EntryTypeList.Codes.ConsumptionFreeDutiable));
			AssertEquals(true, EntryTypeList.IsConsumptionMXCementImportLicense(EntryTypeList.Codes.ConsumptionQuotaVisa));
			AssertEquals(true, EntryTypeList.IsConsumptionMXCementImportLicense(EntryTypeList.Codes.ConsumptionADDCVD));
			AssertEquals(true, EntryTypeList.IsConsumptionMXCementImportLicense(EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa));

			AssertEquals(false, EntryTypeList.IsConsumptionMXCementImportLicense(EntryTypeList.Codes.ImmediateExportation));
			AssertEquals(false, EntryTypeList.IsConsumptionMXCementImportLicense(EntryTypeList.Codes.ConsumptionFTZ));
			AssertEquals(false, EntryTypeList.IsConsumptionMXCementImportLicense(EntryTypeList.Codes.TradeFair));
			AssertEquals(false, EntryTypeList.IsConsumptionMXCementImportLicense(EntryTypeList.Codes.VesselRepair));
		}

		public void TestIsPaperBased()
		{
			AssertEquals(true, EntryTypeList.IsPaperBased(EntryTypeList.Codes.Appraisement));
			AssertEquals(false, EntryTypeList.IsPaperBased(EntryTypeList.Codes.ConsumptionADDCVD));
			AssertEquals(true, EntryTypeList.IsPaperBased(EntryTypeList.Codes.VesselRepair));
			AssertEquals(false, EntryTypeList.IsPaperBased(EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa));
			AssertEquals(true, EntryTypeList.IsPaperBased(EntryTypeList.Codes.TradeFair));
			AssertEquals(false, EntryTypeList.IsPaperBased(EntryTypeList.Codes.ConsumptionFreeDutiable));
			AssertEquals(true, EntryTypeList.IsPaperBased(EntryTypeList.Codes.PermanentExhibition));
			AssertEquals(false, EntryTypeList.IsPaperBased(EntryTypeList.Codes.ConsumptionFTZ));
			AssertEquals(true, EntryTypeList.IsPaperBased(EntryTypeList.Codes.AircraftVesselSupplyIE));
			AssertEquals(false, EntryTypeList.IsPaperBased(EntryTypeList.Codes.ConsumptionQuotaVisa));
			AssertEquals(true, EntryTypeList.IsPaperBased(EntryTypeList.Codes.Baggage));
			AssertEquals(false, EntryTypeList.IsPaperBased(EntryTypeList.Codes.DCASR));
			AssertEquals(true, EntryTypeList.IsPaperBased(EntryTypeList.Codes.BargeMovement));
			AssertEquals(false, EntryTypeList.IsPaperBased(EntryTypeList.Codes.GovernmentDutiable));
			AssertEquals(false, EntryTypeList.IsPaperBased(EntryTypeList.Codes.ImmediateExportation));
			AssertEquals(false, EntryTypeList.IsPaperBased(EntryTypeList.Codes.ImmediateTransportation));
			AssertEquals(true, EntryTypeList.IsPaperBased(EntryTypeList.Codes.PermitToProceed));
			AssertEquals(false, EntryTypeList.IsPaperBased(EntryTypeList.Codes.InformalFreeDutiable));
			AssertEquals(true, EntryTypeList.IsPaperBased(EntryTypeList.Codes.WarehouseFTZ));
		}

		public void TestIsElectronicFilingAllowed()
		{
			AssertEquals(false, EntryTypeList.IsElectronicFilingAllowed(false, EntryTypeList.Codes.ConsumptionADDCVD));
			AssertEquals(true, EntryTypeList.IsElectronicFilingAllowed(false, EntryTypeList.Codes.ConsumptionFreeDutiable));
			AssertEquals(true, EntryTypeList.IsElectronicFilingAllowed(false, EntryTypeList.Codes.InformalFreeDutiable));

			AssertEquals(true, EntryTypeList.IsElectronicFilingAllowed(true, EntryTypeList.Codes.ConsumptionADDCVD));
			AssertEquals(true, EntryTypeList.IsElectronicFilingAllowed(true, EntryTypeList.Codes.ConsumptionFreeDutiable));
			AssertEquals(true, EntryTypeList.IsElectronicFilingAllowed(true, EntryTypeList.Codes.InformalFreeDutiable));
		}

		public void TestMayRequireWarehousing()
		{
			AssertEquals(true, EntryTypeList.MayRequireWarehousing(EntryTypeList.Codes.Warehouse));
			AssertEquals(false, EntryTypeList.MayRequireWarehousing(EntryTypeList.Codes.WarehouseWithdrawalADDCVD));
			AssertEquals(true, EntryTypeList.MayRequireWarehousing(EntryTypeList.Codes.WarehouseFTZ));
			AssertEquals(true, EntryTypeList.MayRequireWarehousing(EntryTypeList.Codes.TemporaryImportationBond));
			AssertEquals(true, EntryTypeList.MayRequireWarehousing(EntryTypeList.Codes.TradeFair));
			AssertEquals(true, EntryTypeList.MayRequireWarehousing(EntryTypeList.Codes.PermanentExhibition));
		}

		public void TestIsWarehouseType()
		{
			var list = new EntryTypeList();
			var entryTypes = new string[]
			{
				EntryTypeList.Codes.Warehouse,
				EntryTypeList.Codes.WarehouseFTZ,
				EntryTypeList.Codes.ReWarehouse
			};

			foreach (string type in entryTypes)
			{
				AssertEquals(type, true, EntryTypeList.IsWarehouseType(type));
				list.RemoveCode(type);
			}

			foreach (ICodeDescription pair in list)
			{
				AssertEquals(pair.Code, false, EntryTypeList.IsWarehouseType(pair.Code));
			}
		}

		public void TestAddtionalGetACEList()
		{
			var list = EntryTypeList.GetACEList();
			AssertEquals(17, list.Count);
			Assert("ConsumptionFreeDutiable", list.ContainsCode(EntryTypeList.Codes.ConsumptionFreeDutiable));
			Assert("ConsumptionADDCVD", list.ContainsCode(EntryTypeList.Codes.ConsumptionADDCVD));
			Assert("InformalFreeDutiable", list.ContainsCode(EntryTypeList.Codes.InformalFreeDutiable));
			Assert("ConsumptionQuotaVisa", list.ContainsCode(EntryTypeList.Codes.ConsumptionQuotaVisa));
			Assert("ConsumptionFTZ", list.ContainsCode(EntryTypeList.Codes.ConsumptionFTZ));
			Assert("ConsumptionADDCVDQuotaVisa", list.ContainsCode(EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa));
			Assert("InformalQuotaVisa", list.ContainsCode(EntryTypeList.Codes.InformalQuotaVisa));
			Assert("Warehouse", list.ContainsCode(EntryTypeList.Codes.Warehouse));
			Assert("ReWarehouse", list.ContainsCode(EntryTypeList.Codes.ReWarehouse));
			Assert("TemporaryImportationBond", list.ContainsCode(EntryTypeList.Codes.TemporaryImportationBond));
			Assert("WarehouseWithdrawalConsumption", list.ContainsCode(EntryTypeList.Codes.WarehouseWithdrawalConsumption));
			Assert("WarehouseWithdrawalQuota", list.ContainsCode(EntryTypeList.Codes.WarehouseWithdrawalQuota));
			Assert("WarehouseWithdrawalADDCVD", list.ContainsCode(EntryTypeList.Codes.WarehouseWithdrawalADDCVD));
			Assert("WarehouseWithdrawalADDCVDQuotaVisa", list.ContainsCode(EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa));
			Assert("DCASR", list.ContainsCode(EntryTypeList.Codes.DCASR));
			Assert("GovernmentDutiable", list.ContainsCode(EntryTypeList.Codes.GovernmentDutiable));
			Assert("LowValue", list.ContainsCode(EntryTypeList.Codes.LowValue));
		}

		public void TestAddLiquidationEntryTypes()
		{
			EntryTypeList list = new EntryTypeList();
			AssertEquals(true, list.ContainsCode(EntryTypeList.Codes.ReconciliationSummary));
			AssertEquals(true, list.ContainsCode(EntryTypeList.Codes.Drawback));
			list.RemoveLiquidationEntryTypes();
			AssertEquals(false, list.ContainsCode(EntryTypeList.Codes.ReconciliationSummary));
			AssertEquals(false, list.ContainsCode(EntryTypeList.Codes.Drawback));
		}

		public void TestRemoveExWarehouseEntryTypes()
		{
			EntryTypeList list = new EntryTypeList();
			AssertEquals(true, list.ContainsCode(EntryTypeList.Codes.WarehouseWithdrawalADDCVD));

			list.RemoveExWarehouseEntryTypes();
			AssertEquals(false, list.ContainsCode(EntryTypeList.Codes.WarehouseWithdrawalADDCVD));
			AssertEquals(false, list.ContainsCode(EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa));
			AssertEquals(false, list.ContainsCode(EntryTypeList.Codes.WarehouseWithdrawalConsumption));
			AssertEquals(false, list.ContainsCode(EntryTypeList.Codes.WarehouseWithdrawalQuota));

			CodeDescriptionPairList list2 = EntryTypeList.GetExWarehouseEntryTypeList();
			AssertEquals(4, list2.Count);
			AssertEquals(true, list2.ContainsCode(EntryTypeList.Codes.WarehouseWithdrawalADDCVD));
			AssertEquals(true, list2.ContainsCode(EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa));
			AssertEquals(true, list2.ContainsCode(EntryTypeList.Codes.WarehouseWithdrawalConsumption));
			AssertEquals(true, list2.ContainsCode(EntryTypeList.Codes.WarehouseWithdrawalQuota));
		}

		public void TestRemoveDrawbackSummaryEntryTypes()
		{
			EntryTypeList list = new EntryTypeList();
			AssertEquals(true, list.ContainsCode(EntryTypeList.Codes.ReconciliationSummary));
			AssertEquals(true, list.ContainsCode(EntryTypeList.Codes.DirectIdentificationManufacturingDrawback));

			list.RemoveLiquidationEntryTypes();
			AssertEquals(false, list.ContainsCode(EntryTypeList.Codes.ReconciliationSummary));

			list.RemoveDrawbackSummaryEntryTypes();
			AssertEquals(false, list.ContainsCode(EntryTypeList.Codes.DirectIdentificationManufacturingDrawback));
			AssertEquals(false, list.ContainsCode(EntryTypeList.Codes.DirectIdentificationUnusedMerchandiseDrawback));
			AssertEquals(false, list.ContainsCode(EntryTypeList.Codes.RejectedMerchandiseDrawback));
			AssertEquals(false, list.ContainsCode(EntryTypeList.Codes.SubstitutionManufacturerDrawback));
			AssertEquals(false, list.ContainsCode(EntryTypeList.Codes.SubstitutionUnusedMerchandiseDrawback));
			AssertEquals(false, list.ContainsCode(EntryTypeList.Codes.OtherDrawback));

			CodeDescriptionPairList list2 = EntryTypeList.GetDrawbackSummaryEntryTypeList();
			AssertEquals(6, list2.Count);
			AssertEquals(true, list2.ContainsCode(EntryTypeList.Codes.DirectIdentificationManufacturingDrawback));
			AssertEquals(true, list2.ContainsCode(EntryTypeList.Codes.DirectIdentificationUnusedMerchandiseDrawback));
			AssertEquals(true, list2.ContainsCode(EntryTypeList.Codes.RejectedMerchandiseDrawback));
			AssertEquals(true, list2.ContainsCode(EntryTypeList.Codes.SubstitutionManufacturerDrawback));
			AssertEquals(true, list2.ContainsCode(EntryTypeList.Codes.SubstitutionUnusedMerchandiseDrawback));
			AssertEquals(true, list2.ContainsCode(EntryTypeList.Codes.OtherDrawback));
		}

		public void TestIsWarehouseLocationNeededFor()
		{
			AssertEquals(true, EntryTypeList.IsWarehouseLocationNeededFor(EntryTypeList.Codes.AircraftVesselSupplyIE));
			AssertEquals(false, EntryTypeList.IsWarehouseLocationNeededFor(EntryTypeList.Codes.BargeMovement));
			AssertEquals(true, EntryTypeList.IsWarehouseLocationNeededFor(EntryTypeList.Codes.ConsumptionFTZ));
			AssertEquals(false, EntryTypeList.IsWarehouseLocationNeededFor(EntryTypeList.Codes.Appraisement));
			AssertEquals(true, EntryTypeList.IsWarehouseLocationNeededFor(EntryTypeList.Codes.Warehouse));
			AssertEquals(false, EntryTypeList.IsWarehouseLocationNeededFor(EntryTypeList.Codes.Baggage));
			AssertEquals(true, EntryTypeList.IsWarehouseLocationNeededFor(EntryTypeList.Codes.ReWarehouse));
			AssertEquals(false, EntryTypeList.IsWarehouseLocationNeededFor(EntryTypeList.Codes.ConsumptionFreeDutiable));
			AssertEquals(true, EntryTypeList.IsWarehouseLocationNeededFor(EntryTypeList.Codes.TemporaryImportationBond));
			AssertEquals(false, EntryTypeList.IsWarehouseLocationNeededFor(EntryTypeList.Codes.ConsumptionQuotaVisa));
			AssertEquals(true, EntryTypeList.IsWarehouseLocationNeededFor(EntryTypeList.Codes.TradeFair));
			AssertEquals(true, EntryTypeList.IsWarehouseLocationNeededFor(EntryTypeList.Codes.PermanentExhibition));
			AssertEquals(false, EntryTypeList.IsWarehouseLocationNeededFor(EntryTypeList.Codes.DirectIdentificationManufacturingDrawback));
			AssertEquals(true, EntryTypeList.IsWarehouseLocationNeededFor(EntryTypeList.Codes.WarehouseFTZ));
			AssertEquals(false, EntryTypeList.IsWarehouseLocationNeededFor(EntryTypeList.Codes.DirectIdentificationUnusedMerchandiseDrawback));
			AssertEquals(true, EntryTypeList.IsWarehouseLocationNeededFor(EntryTypeList.Codes.WarehouseWithdrawalADDCVD));
			AssertEquals(false, EntryTypeList.IsWarehouseLocationNeededFor(EntryTypeList.Codes.GovernmentDutiable));
			AssertEquals(true, EntryTypeList.IsWarehouseLocationNeededFor(EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa));
			AssertEquals(false, EntryTypeList.IsWarehouseLocationNeededFor(EntryTypeList.Codes.ImmediateExportation));
			AssertEquals(true, EntryTypeList.IsWarehouseLocationNeededFor(EntryTypeList.Codes.WarehouseWithdrawalConsumption));
			AssertEquals(false, EntryTypeList.IsWarehouseLocationNeededFor(EntryTypeList.Codes.VesselRepair));
			AssertEquals(true, EntryTypeList.IsWarehouseLocationNeededFor(EntryTypeList.Codes.WarehouseWithdrawalQuota));
		}

		public void TestIsBondTypeCodeRequired()
		{
			AssertEquals(true, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.AircraftVesselSupplyIE));
			AssertEquals(false, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.Appraisement));
			AssertEquals(true, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.Baggage));
			AssertEquals(true, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.BargeMovement));
			AssertEquals(true, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.ConsumptionADDCVD));
			AssertEquals(true, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa));
			AssertEquals(true, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.ConsumptionFreeDutiable));
			AssertEquals(true, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.ConsumptionFTZ));
			AssertEquals(true, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.ConsumptionQuotaVisa));
			AssertEquals(false, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.DCASR));
			AssertEquals(true, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.DirectIdentificationManufacturingDrawback));
			AssertEquals(true, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.DirectIdentificationUnusedMerchandiseDrawback));
			AssertEquals(false, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.GovernmentDutiable));
			AssertEquals(true, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.ImmediateExportation));
			AssertEquals(true, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.ImmediateTransportation));
			AssertEquals(false, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.InformalFreeDutiable));
			AssertEquals(false, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.InformalQuotaVisa));
			AssertEquals(true, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.NAFTADutyDeferral));
			AssertEquals(true, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.OtherDrawback));
			AssertEquals(true, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.PermanentExhibition));
			AssertEquals(true, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.PermitToProceed));
			AssertEquals(true, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.RejectedMerchandiseDrawback));
			AssertEquals(true, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.ReWarehouse));
			AssertEquals(true, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.SubstitutionManufacturerDrawback));
			AssertEquals(true, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.SubstitutionUnusedMerchandiseDrawback));
			AssertEquals(true, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.TemporaryImportationBond));
			AssertEquals(true, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.TradeFair));
			AssertEquals(true, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.TransportationExportation));
			AssertEquals(true, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.VesselRepair));
			AssertEquals(true, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.WarehouseWithdrawalADDCVD));
			AssertEquals(true, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa));
			AssertEquals(true, EntryTypeList.IsBondTypeCodeRequired(EntryTypeList.Codes.WarehouseWithdrawalConsumption));
		}

		public void TestHMFNotApplicableDueToEntryType()
		{
			AssertEquals("IsHMFNotApplicable", false, EntryTypeList.IsHMFNotApplicable(""));
			AssertEquals("IsHMFNotApplicable", true, EntryTypeList.IsHMFNotApplicable(EntryTypeList.Codes.InformalFreeDutiable));
			AssertEquals("IsHMFNotApplicable", true, EntryTypeList.IsHMFNotApplicable(EntryTypeList.Codes.ConsumptionFTZ));
			AssertEquals("IsHMFNotApplicable", true, EntryTypeList.IsHMFNotApplicable(EntryTypeList.Codes.WarehouseWithdrawalConsumption));
			AssertEquals("IsHMFNotApplicable", true, EntryTypeList.IsHMFNotApplicable(EntryTypeList.Codes.WarehouseWithdrawalQuota));
			AssertEquals("IsHMFNotApplicable", true, EntryTypeList.IsHMFNotApplicable(EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa));
			AssertEquals("IsHMFNotApplicable", true, EntryTypeList.IsHMFNotApplicable(EntryTypeList.Codes.WarehouseWithdrawalADDCVD));
			AssertEquals("IsHMFNotApplicable", true, EntryTypeList.IsHMFNotApplicable(EntryTypeList.Codes.AircraftVesselSupplyIE));
			AssertEquals("IsHMFNotApplicable", false, EntryTypeList.IsHMFNotApplicable(EntryTypeList.Codes.ConsumptionQuotaVisa));
			AssertEquals("IsHMFNotApplicable", true, EntryTypeList.IsHMFNotApplicable(EntryTypeList.Codes.ReWarehouse));
		}

		public void TestIsOnlyHMFPayable()
		{
			AssertEquals(true, EntryTypeList.IsOnlyHMFPayable(EntryTypeList.Codes.Warehouse));
			AssertEquals(false, EntryTypeList.IsOnlyHMFPayable(EntryTypeList.Codes.ReWarehouse));
			AssertEquals(false, EntryTypeList.IsOnlyHMFPayable(EntryTypeList.Codes.WarehouseWithdrawalADDCVD));
			AssertEquals(false, EntryTypeList.IsOnlyHMFPayable(EntryTypeList.Codes.WarehouseFTZ));
			AssertEquals(false, EntryTypeList.IsOnlyHMFPayable(EntryTypeList.Codes.ConsumptionFreeDutiable));
		}

		public void TestIsCargoManifestGroupingNotRequired()
		{
			TestIsExWarehouseType();
			string entrytype = EntryTypeList.Codes.ConsumptionFTZ;
			AssertEquals(false, EntryTypeList.IsCargoManifestGroupingAllowed(entrytype, ""));
			AssertEquals(true, EntryTypeList.IsCargoManifestGroupingAllowed(entrytype, TransportModeCodes.Codes.TruckContainer));

			entrytype = EntryTypeList.Codes.ReWarehouse;
			AssertEquals(true, EntryTypeList.IsCargoManifestGroupingAllowed(entrytype, TransportModeCodes.Codes.TruckContainer));

			entrytype = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			AssertEquals(false, EntryTypeList.IsCargoManifestGroupingAllowed(entrytype, TransportModeCodes.Codes.TruckContainer));

			entrytype = EntryTypeList.Codes.ReWarehouse;
			AssertEquals(false, EntryTypeList.IsCargoManifestGroupingAllowed(entrytype, TransportModeCodes.Codes.AirContainer));
		}

		public void TestIsPlannedPortRequiredEntryTypes()
		{
			AssertEquals(true, EntryTypeList.IsEntryPortRequired(EntryTypeList.Codes.ConsumptionQuotaVisa));
			AssertEquals(true, EntryTypeList.IsEntryPortRequired(EntryTypeList.Codes.ConsumptionFTZ));
			AssertEquals(true, EntryTypeList.IsEntryPortRequired(EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa));
			AssertEquals(true, EntryTypeList.IsEntryPortRequired(EntryTypeList.Codes.InformalQuotaVisa));
			AssertEquals(true, EntryTypeList.IsEntryPortRequired(EntryTypeList.Codes.Warehouse));
			AssertEquals(true, EntryTypeList.IsEntryPortRequired(EntryTypeList.Codes.ReWarehouse));
			AssertEquals(true, EntryTypeList.IsEntryPortRequired(EntryTypeList.Codes.TemporaryImportationBond));
			AssertEquals(true, EntryTypeList.IsEntryPortRequired(EntryTypeList.Codes.LowValue));
			AssertEquals(false, EntryTypeList.IsEntryPortRequired(EntryTypeList.Codes.ConsumptionFreeDutiable));
			AssertEquals(false, EntryTypeList.IsEntryPortRequired(EntryTypeList.Codes.WarehouseFTZ));
		}

		public void TestIsInvalidEntryTypeForWaivedBondTypeWhenADCVDReported()
		{
			AssertEquals(true, EntryTypeList.IsInvalidEntryTypeForWaivedBondTypeWhenADCVDReported(EntryTypeList.Codes.ConsumptionFTZ));
			AssertEquals(true, EntryTypeList.IsInvalidEntryTypeForWaivedBondTypeWhenADCVDReported(EntryTypeList.Codes.Warehouse));
			AssertEquals(true, EntryTypeList.IsInvalidEntryTypeForWaivedBondTypeWhenADCVDReported(EntryTypeList.Codes.ReWarehouse));
			AssertEquals(true, EntryTypeList.IsInvalidEntryTypeForWaivedBondTypeWhenADCVDReported(EntryTypeList.Codes.TemporaryImportationBond));
			AssertEquals(false, EntryTypeList.IsInvalidEntryTypeForWaivedBondTypeWhenADCVDReported(EntryTypeList.Codes.DCASR));
		}

		public void TestIsDrawbackHMF_MPFClaimable()
		{
			Assert(EntryTypeList.IsDrawbackHMF_MPFClaimable(EntryTypeList.Codes.DirectIdentificationUnusedMerchandiseDrawback));
			Assert(EntryTypeList.IsDrawbackHMF_MPFClaimable(EntryTypeList.Codes.SubstitutionUnusedMerchandiseDrawback));
			Assert(!EntryTypeList.IsDrawbackHMF_MPFClaimable(EntryTypeList.Codes.DirectIdentificationManufacturingDrawback));
			Assert(!EntryTypeList.IsDrawbackHMF_MPFClaimable(EntryTypeList.Codes.RejectedMerchandiseDrawback));
			Assert(!EntryTypeList.IsDrawbackHMF_MPFClaimable(EntryTypeList.Codes.SubstitutionManufacturerDrawback));
			Assert(!EntryTypeList.IsDrawbackHMF_MPFClaimable(EntryTypeList.Codes.OtherDrawback));
		}
	}
}
