using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business
{
	public class ProhibitedCode : CodeDataPair
	{
		public ProhibitedCode(BusinessObjectFactory factory, ProhibitedCodeCollection parentCollection)
			: base(factory, parentCollection)
		{
		}

		public override bool CodeRequiresData
		{
			get { return false; }
		}

		protected override CodeDescriptionPairList ExportCodes => RefCusCodeListTypes.GetCachedList(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProhibitedGoodsExport, ZDateTime.Today);
		protected override CodeDescriptionPairList ImportCodes => RefCusCodeListTypes.GetCachedList(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProhibitedGoodsImport, ZDateTime.Today);
		protected override CodeDescriptionPairList LegacyCodes => Factory.GetCachedValue<ProhibitedCodeList>();
		protected override string ImportExportCodesCacheKey => "NZ|ProhibitedCode|ImportExportCodes";

		protected override ZString HumanReadableNameCore
		{
			get { return "Prohibited Code"; }
		}

		public override bool ShouldValidateForDuplicateCodes
		{
			get { return true; }
		}
	}
}
