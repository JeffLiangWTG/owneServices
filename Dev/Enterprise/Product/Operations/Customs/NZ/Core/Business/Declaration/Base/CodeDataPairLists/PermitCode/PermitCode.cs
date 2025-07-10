using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business
{
	public class PermitCode : CodeDataPair
	{
		public PermitCode(BusinessObjectFactory factory, PermitCodeCollection parentCollection)
			: base(factory, parentCollection)
		{
		}

		public override bool CodeRequiresData
		{
			get { return true; }
		}

		protected override bool IsExportDeclaration(JobDeclaration declaration) => declaration.IsTSWExportDeclaration;
		protected override bool IsImportDeclaration(JobDeclaration declaration) => declaration.IsTSWDeclaration && !declaration.IsTSWExportDeclaration;

		protected override CodeDescriptionPairList ExportCodes => RefCusCodeListTypes.GetCachedList(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsExport, ZDateTime.Today);
		protected override CodeDescriptionPairList ImportCodes => RefCusCodeListTypes.GetCachedList(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsImport, ZDateTime.Today);
		protected override CodeDescriptionPairList LegacyCodes => Factory.GetCachedValue<PermitCodeList>();
		protected override string ImportExportCodesCacheKey => "NZ|PermitCode|ImportExportCodes";

		public override bool ShouldValidateForDuplicateCodes
		{
			get { return false; }
		}

		protected override ZString HumanReadableNameCore
		{
			get { return "Permit Code"; }
		}
	}
}
