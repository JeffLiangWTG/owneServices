using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business
{
	public class HeaderOtherInfo : OtherInfo
	{
		public HeaderOtherInfo(BusinessObjectFactory factory, HeaderOtherInfoCollection parentCollection)
			: base(factory, parentCollection)
		{
		}

		JobDeclaration ParentDeclaration => ParentCollection?.ParentDeclaration;

		protected override bool IsExportDeclaration(JobDeclaration declaration) => declaration.IsTSWExportDeclaration && ParentDeclaration != null;
		protected override bool IsImportDeclaration(JobDeclaration declaration) => declaration.IsTSWImportDeclaration && ParentDeclaration != null;

		protected override CodeDescriptionPairList ExportCodes => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportHeader, ZDateTime.Today);
		protected override CodeDescriptionPairList ImportCodes => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportHeader, ZDateTime.Today);
		protected override CodeDescriptionPairList LegacyCodes => Factory.GetCachedValue<HeaderOtherInfoList>();
		protected override string ImportExportCodesCacheKey => "NZ|HeaderOtherInfo|ImportExportCodes";

		public override bool ShouldValidateForDuplicateCodes
		{
			get { return true; }
		}

		protected override bool CodeDoesntCareIfItHasDataOrNot
		{
			get { return ZO_Code == HeaderOtherInfoList.Codes.MAFContainerDeclaration; }
		}

		protected override void ZO_Code_OnChanged()
		{
			RefreshMCDDetailsIfApplicable();
		}

		protected override void ZO_Data_OnChanged()
		{
			RefreshMCDDetailsIfApplicable();
		}

		public void RefreshMCDDetailsIfApplicable()
		{
			if (ZO_Code == HeaderOtherInfoList.Codes.MAFContainerDeclaration)
			{
				var declaration = Declaration;
				if (declaration != null)
				{
					declaration.JE_SendMCDContainerQuarantineDeclarationInfo.RefreshBinding();
					declaration.JE_HaveMAFContainerDeclarationInfo.RefreshBinding();
					declaration.JE_IsContainerCleanInfo.RefreshBinding();
					declaration.JE_IsPackagingMaterialContaminatedInfo.RefreshBinding();
					declaration.JE_IsWoodPackagingUsedInfo.RefreshBinding();
					declaration.JE_IsWoodPackagingTreatedInfo.RefreshBinding();
					declaration.JE_IsWoodPackagingTreatmentCertificateAvailableInfo.RefreshBinding();
					declaration.CusContainers?.RefreshBinding();
				}
			}
		}

		protected override string[] ExportCodesRequiringData
		{
			get
			{
				return Factory.GetCachedValue(".NZ.Business.HeaderOtherInfo.ExportCodesRequiringData",
					() =>
					{
						var exportCodesListsRequiringData = RefCusCodeListTypes.GetCachedListMatchAllAttributes(
								Factory,
								Core.Constants.CountryCodes.NewZealand,
								Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportHeader,
								ZDateTime.Today,
								new[] { new KeyValuePair<ZString, ZString>(UniversalReferenceConstants.RefCusCodeListAttributeTypes.TSWCodeListValueRequired, UniversalReferenceConstants.RefCusCodeListAttributeValues.Yes) });

						return exportCodesListsRequiringData.GetAllCodes();
					});
			}
		}
		protected override string[] ImportCodesRequiringData
		{
			get
			{
				return Factory.GetCachedValue(".NZ.Business.HeaderOtherInfo.ImportCodesRequiringData",
					() =>
					{
						var importCodesListRequiringData = RefCusCodeListTypes.GetCachedListMatchAllAttributes(
								Factory,
								Core.Constants.CountryCodes.NewZealand,
								Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportHeader,
								ZDateTime.Today,
								new[] { new KeyValuePair<ZString, ZString>(UniversalReferenceConstants.RefCusCodeListAttributeTypes.TSWCodeListValueRequired, UniversalReferenceConstants.RefCusCodeListAttributeValues.Yes) });

						return importCodesListRequiringData.GetAllCodes();
					});
			}
		}

		public override void ValidateZO_Code()
		{
			base.ValidateZO_Code();
			var declaration = Declaration;
			if (declaration != null)
			{
				if (ZO_Code == HeaderOtherInfoList.Codes.MAFContainerDeclaration &&
						declaration.IsFormalEntry && declaration.IsImport && declaration.IsSea &&
						declaration.CusContainers.HasContainerModeOf(ContainerModeList.Codes.FCL) &&
						!declaration.ContainsThisOtherInfoCode(HeaderOtherInfoList.Codes.ApprovedTransitionalFacility))
				{
					ZO_CodeInfo.AddMessageError(ATFCodeIsRequired);
				}
			}
		}

		public const string ATFCodeIsRequired = "An ATF code must also be supplied for FCL declarations.";
	}
}
