using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class UNDGDataItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateSubstancePK()
		{
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Code = "1234a";
			substance.DG_UNNO = "1234";
			substance.DG_Variant = "a";
			substance.DG_LQMaxAmt = 250;
			substance.DG_LQMaxAmtUQ = "L";

			var item = Factory.New<UNDGDataItem>();
			item.SubstancePK = substance.PK;
			item.DI_IMOClass = "1.1";
			AssertNoErrors(item.SubstancePKInfo);

			item.SubstancePK = ZGuid.Empty;
			AssertHasError(item.SubstancePKInfo, "Please enter a DG substance.");
		}

		public void TestValidateLimitedQuantity_VolumeUnits()
		{
			UNDGSubstance substance = Factory.New<UNDGSubstance>();
			substance.DG_Code = "1234a";
			substance.DG_UNNO = "1234";
			substance.DG_Variant = "a";
			substance.DG_LQMaxAmt = 250;
			substance.DG_LQMaxAmtUQ = "L";

			UNDGDataItem item = Factory.New<UNDGDataItem>();
			item.DI_DG = substance.PK;
			item.DI_DGVolume = 200;
			item.DI_UnitOfVolume = "L";
			AssertNoWarnings(item.DI_IsLimitedQuantityInfo);

			item.DI_UnitOfVolume = "XX";
			AssertHasWarning(item.DI_IsLimitedQuantityInfo, "Substance 1234a has a Limited Quantity Volume specified of 250 L, however the volume units entered on this line are valid. Please specify volume units, or manually specify whether this line contains a Limited Quantity.");

			item.DI_UnitOfVolume = "ML";
			AssertNoWarnings(item.DI_IsLimitedQuantityInfo);

			item.DI_UnitOfVolume = "";
			item.DI_DGVolume = 0m;
			AssertNoWarnings(item.DI_IsLimitedQuantityInfo);
		}

		public void TestValidateLimitedQuantity_WeightUnits()
		{
			UNDGSubstance substance = Factory.New<UNDGSubstance>();
			substance.DG_Code = "1234a";
			substance.DG_UNNO = "1234";
			substance.DG_Variant = "a";

			substance.DG_LQMaxAmt = 250;
			substance.DG_LQMaxAmtUQ = "G";

			UNDGDataItem item = Factory.New<UNDGDataItem>();
			item.DI_DG = substance.PK;
			item.DI_DGWeight = 200;
			item.DI_UnitOfWeight = "G";
			AssertNoWarnings(item.DI_IsLimitedQuantityInfo);

			item.DI_UnitOfWeight = "XX";
			AssertHasWarning(item.DI_IsLimitedQuantityInfo, "Substance 1234a has a Limited Quantity Weight specified of 250 G, however the weight units entered on this line are not valid. Please specify weight units, or manually specify whether this line contains a Limited Quantity.");

			item.DI_UnitOfWeight = "KG";
			AssertNoWarnings(item.DI_IsLimitedQuantityInfo);

			item.DI_DGWeight = 0m;
			item.DI_UnitOfWeight = "";
			AssertNoWarnings(item.DI_IsLimitedQuantityInfo);
		}

		public void TestValidateDI_DG_DI_DG_NKSubsOrDI_IMOClassIsEntered()
		{
			UNDGDataItem item = Factory.New<UNDGDataItem>();
			item.DI_DG = ZGuid.Empty;
			item.Validation.ValidateAll();

			string expectedMessage = "At least one of DG Substance, DG Class must be entered.";
			AssertHasError(item.DI_IMOClassInfo, expectedMessage);

			UNDGSubstance substance = Factory.New<UNDGSubstance>();
			substance.DG_Code = "1234a";
			substance.DG_UNNO = "1234";
			substance.DG_Variant = "a";
			item.DI_DG = substance.PK;
			AssertNoErrors(item.DI_IMOClassInfo);
		}

		public void TestValidateLimitedQuantity_GrossWeight()
		{
			UNDGSubstance substance = Factory.New<UNDGSubstance>();
			substance.DG_Code = "1234a";
			substance.DG_LQMaxAmt = 250;
			substance.DG_LQMaxAmtUQ = "G";
			substance.DG_LQMaxAmtType = UNDGSubstanceLookups.LimitedQuantityTypes.GLMCode;

			UNDGDataItem item = Factory.New<UNDGDataItem>();
			item.DI_IsLimitedQuantity = false;
			AssertNoWarnings(item.DI_DGWeightInfo);
			item.DI_DG = substance.PK;
			item.DI_IsLimitedQuantity = true;
			AssertHasWarning(item.DI_DGWeightInfo, "When transported in limited quantities, the gross weight is required for this substance by IATA DGR.");

			item.DI_DGWeight = 200;
			item.DI_IsLimitedQuantity = false;
			AssertNoWarnings(item.DI_DGWeightInfo);

			item.DI_IsLimitedQuantity = true;
			AssertHasWarning(item.DI_DGWeightInfo, "When transported in limited quantities, the gross weight is required for this substance by IATA DGR.");
		}

		public void TestValidateLimitedQuantity_NetWeight()
		{
			UNDGSubstance substance = Factory.New<UNDGSubstance>();
			substance.DG_Code = "1234a";
			substance.DG_LQMaxAmt = 250;
			substance.DG_LQMaxAmtUQ = "G";
			substance.DG_LQMaxAmtType = UNDGSubstanceLookups.LimitedQuantityTypes.NLMCode;

			UNDGDataItem item = Factory.New<UNDGDataItem>();
			item.DI_IsLimitedQuantity = false;
			AssertNoWarnings(item.DI_DGWeightInfo);
			item.DI_DG = substance.PK;
			item.DI_IsLimitedQuantity = true;
			AssertHasWarning(item.DI_DGWeightInfo, "When transported in limited quantities, the weight is required for this substance by IATA DGR.");

			item.DI_DGWeight = 200;
			item.DI_IsLimitedQuantity = false;
			AssertNoWarnings(item.DI_DGWeightInfo);

			item.DI_IsLimitedQuantity = true;
			AssertNoWarnings(item.DI_DGWeightInfo);
		}

		public void TestValidateLimitedQuantity_NetVolume()
		{
			UNDGSubstance substance = Factory.New<UNDGSubstance>();
			substance.DG_Code = "1234a";
			substance.DG_LQMaxAmt = 250;
			substance.DG_LQMaxAmtUQ = "L";
			substance.DG_LQMaxAmtType = UNDGSubstanceLookups.LimitedQuantityTypes.NLMCode;

			UNDGDataItem item = Factory.New<UNDGDataItem>();
			item.DI_IsLimitedQuantity = false;
			AssertNoWarnings(item.DI_DGVolumeInfo);
			item.DI_DG = substance.PK;
			item.DI_IsLimitedQuantity = true;
			AssertHasWarning(item.DI_DGVolumeInfo, "When transported in limited quantities, the volume is required for this substance by IATA DGR.");

			item.DI_DGVolume = 200;
			item.DI_IsLimitedQuantity = false;
			AssertNoWarnings(item.DI_DGVolumeInfo);

			item.DI_IsLimitedQuantity = true;
			AssertNoWarnings(item.DI_DGVolumeInfo);
		}

		public void TestValidateLimitedQuantity_GrossVolume()
		{
			UNDGSubstance substance = Factory.New<UNDGSubstance>();
			substance.DG_Code = "1234a";
			substance.DG_LQMaxAmt = 250;
			substance.DG_LQMaxAmtUQ = "L";
			substance.DG_LQMaxAmtType = UNDGSubstanceLookups.LimitedQuantityTypes.GLMCode;

			UNDGDataItem item = Factory.New<UNDGDataItem>();
			item.DI_IsLimitedQuantity = false;
			AssertNoWarnings(item.DI_DGVolumeInfo);

			item.DI_IsLimitedQuantity = true;
			AssertNoWarnings(item.DI_DGVolumeInfo);

			item.DI_DGVolume = 200;
			item.DI_IsLimitedQuantity = false;
			AssertNoWarnings(item.DI_DGVolumeInfo);

			item.DI_IsLimitedQuantity = true;
			AssertNoWarnings(item.DI_DGVolumeInfo);
		}

		public void TestValidateLimitedQuantity_SpecialProvision()
		{
			UNDGSubstance substance = Factory.NewWithValidTestData<UNDGSubstance>();
			substance.DG_Code = "1234a";
			substance.DG_LQSpecProvIndex = "251";

			UNDGDataItem item = Factory.New<UNDGDataItem>();
			item.DI_DG = substance.PK;
			AssertEquals(true, item.DI_IsLimitedQuantityInfo.Notifications.GetWarnings().GetFirst().Message.StartsWith("Substance 1234a has a Special Provision specified for the handling of Limited Quantities. Please review this provision and manually specify whether this line contains a Limited Quantity. The provision number is 251 and states:\r\n\r\nThe entry CHEMICAL KIT or FIRST AID KIT is intended to apply"));
		}

		public void TestValidateDI_MPMarinePollutant()
		{
			UNDGSubstance substance = Factory.New<UNDGSubstance>();
			substance.DG_Code = "ABC";
			substance.DG_UNNO = "ABC";
			substance.DG_MP = UNDGSubstanceLookups.MarinePollutantTypes.Depends_Code;

			UNDGDataItem item = Factory.New<UNDGDataItem>();

			item.DI_MPMarinePollutant = "";
			AssertNoErrors(item.DI_MPMarinePollutantInfo);

			item.DI_MPMarinePollutant = UNDGSubstanceLookups.MarinePollutantTypes.MarinePollutant_Code;
			AssertNoErrors(item.DI_MPMarinePollutantInfo);

			item.DI_MPMarinePollutant = UNDGSubstanceLookups.MarinePollutantTypes.SevereMarinePollutant_Code;
			AssertNoErrors(item.DI_MPMarinePollutantInfo);

			item.DI_MPMarinePollutant = "%";
			AssertNoErrors(item.DI_MPMarinePollutantInfo);
		}

		public void TestValidateDI_F3_NKPackType()
		{
			var dataItem = Factory.New<UNDGDataItem>();
			dataItem.DI_F3_NKPackType = "CCC";
			AssertHasError(dataItem.DI_F3_NKPackTypeInfo, "Enter a valid Pack Type.");

			var validTypeCode = dataItem.Lookups.PackTypes.FirstOrDefault().F3_Code;

			dataItem.DI_F3_NKPackType = validTypeCode;
			AssertNoErrors(dataItem.DI_F3_NKPackTypeInfo);

			dataItem.DI_PackageCount = 0;
			dataItem.DI_F3_NKPackType = string.Empty;
			AssertNoErrors(dataItem.DI_F3_NKPackTypeInfo);

			dataItem.DI_PackageCount = 3;
			dataItem.DI_F3_NKPackType = string.Empty;
			AssertHasError(dataItem.DI_F3_NKPackTypeInfo, "Please enter a Pack Type.");
		}

		public void TestValidateDI_PackageCount()
		{
			var dataItem = Factory.New<UNDGDataItem>();
			dataItem.DI_PackageCount = -1;
			AssertHasError(dataItem.DI_PackageCountInfo, "Please enter a 'Package Count' greater than or equal to 0.");

			dataItem.DI_PackageCount = 3;
			AssertNoErrors(dataItem.DI_PackageCountInfo);
		}

		public void TestValidateDI_ApprovalCertificateType()
		{
			var dataItem = Factory.New<UNDGDataItem>();
			dataItem.DI_ApprovalCertificateType = ZString.Empty;
			dataItem.DI_ApprovalCertificateIDMark = ZString.Empty;
			AssertNoErrors(dataItem.DI_ApprovalCertificateTypeInfo);

			dataItem.DI_ApprovalCertificateIDMark = "ID";
			AssertHasError(dataItem.DI_ApprovalCertificateTypeInfo, "Please enter an Approval Certificate Type.");

			dataItem.DI_ApprovalCertificateType = ApprovalCertificateTypeList.Codes.TypeCPackageDesignAndShipment;
			AssertNoErrors(dataItem.DI_ApprovalCertificateTypeInfo);

			dataItem.DI_ApprovalCertificateType = "ABC";
			AssertHasError(dataItem.DI_ApprovalCertificateTypeInfo, "Enter a valid Approval Certificate Type.");
		}

		public void TestValidateDI_ApprovalCertificateIDMark()
		{
			var dataItem = Factory.New<UNDGDataItem>();
			dataItem.DI_ApprovalCertificateType = ZString.Empty;
			dataItem.DI_ApprovalCertificateIDMark = ZString.Empty;
			AssertNoErrors(dataItem.DI_ApprovalCertificateIDMarkInfo);

			dataItem.DI_ApprovalCertificateType = ApprovalCertificateTypeList.Codes.TypeCPackageDesignAndShipment;
			AssertHasError(dataItem.DI_ApprovalCertificateIDMarkInfo, "Please enter an Approval Certificate ID Mark.");
		}

		public void TestValidateDI_ClassOneDGWithInvalidNECWeight()
		{
			var subsClassOne = Factory.New<UNDGSubstance>();
			subsClassOne.DG_UNNO = "TTT";
			subsClassOne.DG_Variant = "a";
			subsClassOne.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subsClassOne.DG_Class = "1.1a";
			var classOneItem = Factory.New<UNDGDataItem>();
			classOneItem.DI_DG = subsClassOne.PK;
			classOneItem.DI_DGWeight = new ZDecimal(10);
			classOneItem.DI_NECWeightUQ = Core.Constants.Weight.Kilograms;

			AssertHasWarning(classOneItem.DI_NECWeightInfo, "Net explosive content is required for Class 1 substances");

			classOneItem.DI_NECWeight = new ZDecimal(11);
			AssertHasError(classOneItem.DI_NECWeightInfo, "Net Explosive Content must be less than, or equal to, the net mass of the Class 1 UN substance");
		}

		public void TestValidateDI_ClassOneDGWithNECWeightAndUQ()
		{
			var subsClassOne = Factory.New<UNDGSubstance>();
			subsClassOne.DG_UNNO = "TTT";
			subsClassOne.DG_Variant = "a";
			subsClassOne.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subsClassOne.DG_Class = "1.1a";
			var classOneItem = Factory.New<UNDGDataItem>();
			classOneItem.DI_DG = subsClassOne.PK;
			classOneItem.DI_DGWeight = new ZDecimal(2);
			classOneItem.DI_NECWeight = new ZDecimal(1);
			classOneItem.Validation.ValidateDI_NECWeightUQ();
			AssertHasError(classOneItem.DI_NECWeightUQInfo, "Please enter a value.");
		}
	}
}
