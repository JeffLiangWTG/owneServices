using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business
{
	public class LineOtherInfo : OtherInfo
	{
		public LineOtherInfo(BusinessObjectFactory factory, LineOtherInfoCollection parentCollection)
			: base(factory, parentCollection)
		{
		}

		JobComInvoiceLine InvoiceLine => ParentCollection?.ParentInvoiceLine;

		protected override bool IsExportDeclaration(JobDeclaration declaration) => declaration.IsTSWExportDeclaration && InvoiceLine != null;
		protected override bool IsImportDeclaration(JobDeclaration declaration) => declaration.IsTSWImportDeclaration && InvoiceLine != null;

		protected override CodeDescriptionPairList ExportCodes => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportLine, ZDateTime.Today);
		protected override CodeDescriptionPairList ImportCodes => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportLine, ZDateTime.Today);
		protected override CodeDescriptionPairList LegacyCodes => Factory.GetCachedValue<LineOtherInfoList>();
		protected override string ImportExportCodesCacheKey => "NZ|LineOtherInfo|ImportExportCodes";

		public override bool ShouldValidateForDuplicateCodes
		{
			get { return false; }
		}

		public bool IsLVX => ZO_Code == Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NZLowValueGoodsExclusion;

		protected override string[] ExportCodesRequiringData
		{
			get
			{
				return Factory.GetCachedValue(".NZ.Business.LineOtherInfo.ExportCodesRequiringData",
					() =>
					{
						var exportCodesListsRequiringData = RefCusCodeListTypes.GetCachedListMatchAllAttributes(
								Factory,
								Core.Constants.CountryCodes.NewZealand,
								Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportLine,
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
				return Factory.GetCachedValue(".NZ.Business.LineOtherInfo.ImportCodesRequiringData",
					() =>
					{
						var importCodesListRequiringData = RefCusCodeListTypes.GetCachedListMatchAllAttributes(
								Factory,
								Core.Constants.CountryCodes.NewZealand,
								Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportLine,
								ZDateTime.Today,
								new[] { new KeyValuePair<ZString, ZString>(UniversalReferenceConstants.RefCusCodeListAttributeTypes.TSWCodeListValueRequired, UniversalReferenceConstants.RefCusCodeListAttributeValues.Yes) });

						return importCodesListRequiringData.GetAllCodes();
					});
			}
		}

		#region Validation
		public override void ValidateZO_Code()
		{
			base.ValidateZO_Code();

			if (IsLVX)
			{
				var invoiceLine = InvoiceLine;
				if (invoiceLine != null && !invoiceLine.JI_Tariff.IsEmpty && !invoiceLine.IsTabaccoOrAlcoholic)
				{
					ZO_CodeInfo.AddMessageError(Res.GetString("49be2379-1909-403f-bdcd-19c2ba1932f8", "LVX code is not applicable to the tariff chapter specified for this item."));
				}
			}
		}
		#endregion
	}
}
