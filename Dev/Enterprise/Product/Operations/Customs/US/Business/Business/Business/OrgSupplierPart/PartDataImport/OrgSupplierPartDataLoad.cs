using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class OrgSupplierPartDataLoad : GlobalOrgSupplierPartDataLoad, Integration.Customs.US.IOrgSupplierPartDataLoad
	{
		#region Parts Data

		protected override IEnumerable<string> GetFieldNames()
		{
			var fieldNames = new List<string>();

			fieldNames.AddRange(base.GetFieldNames());

			fieldNames.Add("ProductClaim");
			fieldNames.Add("SupImportTariff");
			fieldNames.Add("SPI");
			fieldNames.Add("CBTPACertificateNo");
			fieldNames.Add("CottonCertificateNo");
			fieldNames.Add("MiscPermitNo");
			fieldNames.Add("PIRPRulingNo");
			fieldNames.Add("PIRPRulingType");
			fieldNames.Add("WoolLicenceNo");
			fieldNames.Add("IsNAFTANet");
			fieldNames.Add("CAExportCertificate");
			fieldNames.Add("AgricultureLicNo");
			fieldNames.Add("CargoStorageCode");
			fieldNames.Add("ZoneStatus");
			fieldNames.Add("PercentageActiveIngredient");
			fieldNames.Add("ManufacturerID");
			fieldNames.Add("ADDCaseNo");
			fieldNames.Add("CVDCaseNo");
			fieldNames.Add("ProductExclusion");
			fieldNames.Add("ExclusionNumber");
			fieldNames.Add("SupAdditionalTariff1");
			fieldNames.Add("SupAdditionalTariff2");
			fieldNames.Add("SupAdditionalTariff3");
			fieldNames.Add("SupAdditionalTariff4");
			fieldNames.Add("SupAdditionalTariff5");

			return fieldNames;
		}

		readonly USPartsDataToLoad partsDataToLoad = new USPartsDataToLoad();

		protected override PartsDataToLoad GetPartsDataToLoad()
		{
			return partsDataToLoad;
		}

		public class USPartsDataToLoad : GlobalPartsDataToLoad
		{
			public ZString ProductClaim;
			public ZString SupImportTariff;
			public ZString SPI;
			public ZString CBTPACertificateNo;
			public ZString CottonCertificateNo;
			public ZString MiscPermitNo;
			public ZString PIRPRulingNo;
			public ZString PIRPRulingType;
			public ZString WoolLicenceNo;
			public bool IsNAFTANet;
			public ZString CAExportCertificate;
			public ZString AgricultureLicNo;
			public ZString CargoStorageCode;
			public ZString ZoneStatus;
			public decimal PercentageActiveIngredient;
			public ZString ManufacturerID;
			public ZString ADDCaseNo;
			public ZString CVDCaseNo;
			public ZString ProductExclusion;
			public ZString ExclusionNumber;
			public ZString SupAdditionalTariff1;
			public ZString SupAdditionalTariff2;
			public ZString SupAdditionalTariff3;
			public ZString SupAdditionalTariff4;
			public ZString SupAdditionalTariff5;
		}

		#endregion

		#region Create/Update Pivot

		protected override bool UseOldClassificationFields => true;

		protected override bool IsScheduleB(ZString tariff) => true;

		protected override string ScheduleBClassificationType => ClassificationTypeList.Codes.SHB;

		protected override void AddDataToPivotUseOldClassificationFields(BaseCusClassPartPivot pivot, PartsDataToLoad partsData, string importOrExport)
		{
			base.AddDataToPivot(pivot, partsData);
			if (importOrExport == CusClassification.ClassificationType.IMP)
			{
				AddImportDataToPivot((CusClassPartPivot)pivot, partsData);
			}
		}

		void AddImportDataToPivot(CusClassPartPivot pivot, PartsDataToLoad partsData)
		{
			var data = (USPartsDataToLoad)partsData;

			pivot.CD_ProductClaim = data.ProductClaim.Left(pivot.CD_ProductClaimInfo.MaxLength);
			pivot.CI_FormattedSupplementalTariff = data.SupImportTariff.Left(pivot.CI_FormattedSupplementalTariffInfo.MaxLength);
			pivot.CD_SPI = data.SPI.Left(pivot.CD_SPIInfo.MaxLength);
			pivot.CD_CBTPACertificate = data.CBTPACertificateNo.Left(pivot.CD_CBTPACertificateInfo.MaxLength);
			pivot.CD_CottonCertificate = data.CottonCertificateNo.Left(pivot.CD_CottonCertificateInfo.MaxLength);
			pivot.CD_MiscLicenceNo = data.MiscPermitNo.Left(pivot.CD_MiscLicenceNoInfo.MaxLength);
			pivot.CD_RulingNumber = data.PIRPRulingNo.Left(pivot.CD_RulingNumberInfo.MaxLength);
			pivot.CD_RulingType = data.PIRPRulingType.Left(pivot.CD_RulingTypeInfo.MaxLength);
			pivot.CD_WoolLicenceNo = data.WoolLicenceNo.Left(pivot.CD_WoolLicenceNoInfo.MaxLength);
			pivot.CD_NAFTANetCost = data.IsNAFTANet;
			pivot.CD_SugarCertificate = data.CAExportCertificate.Left(pivot.CD_SugarCertificateInfo.MaxLength);
			pivot.CD_AgricultureLicenceNo = data.AgricultureLicNo.Left(pivot.CD_AgricultureLicenceNoInfo.MaxLength);
			pivot.CD_ZoneStatus = data.ZoneStatus.Left(pivot.CD_ZoneStatusInfo.MaxLength);
			pivot.CD_ActiveIngredientPercentage = data.PercentageActiveIngredient;
			pivot.CD_ADDCaseNo = data.ADDCaseNo.Left(pivot.CD_ADDCaseNoInfo.MaxLength);
			pivot.CD_CVDCaseNo = data.CVDCaseNo.Left(pivot.CD_CVDCaseNoInfo.MaxLength);
			pivot.CD_ProductExclusion = data.ProductExclusion.Left(pivot.CD_ProductExclusionInfo.MaxLength);
			pivot.CD_ExclusionNumber = data.ExclusionNumber.Left(pivot.CD_ExclusionNumberInfo.MaxLength);
			pivot.SupFormattedAdditionalTariff1 = data.SupAdditionalTariff1.Left(pivot.SupFormattedAdditionalTariff1Info.MaxLength);
			pivot.SupFormattedAdditionalTariff2 = data.SupAdditionalTariff2.Left(pivot.SupFormattedAdditionalTariff2Info.MaxLength);
			pivot.SupFormattedAdditionalTariff3 = data.SupAdditionalTariff3.Left(pivot.SupFormattedAdditionalTariff3Info.MaxLength);
			pivot.SupFormattedAdditionalTariff4 = data.SupAdditionalTariff4.Left(pivot.SupFormattedAdditionalTariff4Info.MaxLength);
			pivot.SupFormattedAdditionalTariff5 = data.SupAdditionalTariff5.Left(pivot.SupFormattedAdditionalTariff5Info.MaxLength);

			string manufacturerID = data.ManufacturerID;
			if (!string.IsNullOrEmpty(manufacturerID))
			{
				ZQuery query = new ZQuery(OrgCusCodeSchema.OK_CustomsRegNo, manufacturerID);
				query.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.USACodeTypes.ManufacturerID);
				query.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.UnitedStates);
				OrgCusCode cusCode = Factory.LoadTop1<OrgCusCode>(query);
				if (cusCode != null)
				{
					pivot.CD_OA_Manufacturer = cusCode.OK_OA_PremisesAddress;
				}
			}
			pivot.CD_UC_NKCountryOfOrigin = partsData.PartOrigin.Length <= pivot.CD_UC_NKCountryOfOriginInfo.MaxLength
										  ? partsData.PartOrigin
										  : ZString.Empty;
		}

		#endregion

		#region Set UQ

		protected override string GetUQFromTariff(ZString tariffCode, ZString classType)
		{
			var tariffType = classType == Universal.Constants.TariffTypes.Import ? Universal.Constants.TariffTypes.HarmonizedSystem : Universal.Constants.TariffTypes.ScheduleB;
			var tariff = Factory.GetTariff(tariffType, tariffCode, ZDateTime.Today);
			return tariff?.UQ1 ?? ZString.Empty;
		}

		#endregion
	}
}
