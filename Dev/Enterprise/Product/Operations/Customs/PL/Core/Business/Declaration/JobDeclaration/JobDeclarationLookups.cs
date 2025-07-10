using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business.Declaration;

public partial class JobDeclarationLookups(JobDeclaration parent) : EU.Business.Declaration.JobDeclarationLookups(parent)
{
	public new JobDeclaration Declaration => Parent;

	protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

	public CodeDescriptionPairList PLRepresentationTypeList => Factory.GetCachedValue<PLRepresentationTypeList>();

	public CodeDescriptionPairList GoodsLocationTypeList
	{
		get
		{
			var result = new CodeDescriptionPairList();
			if (Parent.IsImport)
			{
				result = Factory.GetCachedValue<GoodsLocationTypeList>();
			}
			else
			{
				if (Parent.CustomsEntryInstructions.Any(x => x.ZG_ExportManifest))
				{
					result.AddPair(QualifierOfTheIdentificationList.Codes.Y, QualifierOfTheIdentificationList.Descriptions.Y);
					result.AddPair(QualifierOfTheIdentificationList.Codes.Z, QualifierOfTheIdentificationList.Descriptions.Z);
				}
				else
				{
					result = Factory.GetCachedValue<QualifierOfTheIdentificationList>();
				}
			}
			return result;
		}
	}

	public CodeDescriptionPairList TypeOfLocationList => Factory.GetCachedValue<TypeOfLocationList>();

	public CodeDescriptionPairList PLCustomsChargeTypeList => Factory.GetCachedValue<PLCustomsChargeTypeList>();

	public RefUNLOCOCollection UNLocodeList => new RefUNLOCOCollection(Factory);

	public CustomsOfficeCodeCollection AdditionalCustomsOffices
	{
		get
		{
			var additionalOfficeRequirement = Parent.CustomsOfficeRequirementHelper?.AdditionalOffice;
			var roles = additionalOfficeRequirement?.OfficeRolesForLookup.ToArray() ?? [];
			var isLocalCountryOnly = additionalOfficeRequirement?.IsLocalCountryOnly ?? false;
			var isForeignCountryOnly = additionalOfficeRequirement?.IsForeignCountryOnly ?? false;
			return additionalOfficeRequirement switch
			{
				_ when isLocalCountryOnly => CustomsOfficeCodeCollection.LocalCountryOnlyCustomsOfficesWithRequiredRoles(Factory, Parent.CountryCode, roles),
				_ when isForeignCountryOnly => EUCustomsOfficeCodeCollection.AllEuropeanUnionCustomsOfficesWithRequiredRolesExceptLocal(Factory, Parent.CountryCode, roles),
				_ => EUCustomsOfficeCodeCollection.AllEuropeanUnionCustomsOfficesWithRequiredRoles(Factory, roles)
			};
		}
	}

	public CustomsOfficeCodeCollection GetAllEuropeanUnionCustomsOffices => EUCustomsOfficeCodeCollection.AllEuropeanUnionCustomsOfficesWithRequiredRoles(Factory, string.Empty);

	public CustomsOfficeCodeCollection GetAllEuropeanUnionCustomsOfficesWithSpecificRote(ZString role) => EUCustomsOfficeCodeCollection.AllEuropeanUnionCustomsOfficesWithRequiredRoles(Factory, role);

	public CustomsOfficeCodeCollection GetAllPLCustomsOfficesWithSpecificRote(ZString role) => CustomsOfficeCodeCollection.LocalCountryOnlyCustomsOfficesWithRequiredRoles(Factory, Core.Constants.CountryCodes.Poland, role);

	public ZString[] EUCountryCodes => Factory.GetCachedValue("JobComInvoiceLineLookups|EUCountryCodes", () => new ZString[] { "AT", "BE", "BG", "CY", "CZ", "DE", "DK", "EE", "EL", "ES", "FI", "FR", "GR", "HU", "IE", "IT", "LT", "LU", "LV", "MT", "NT", "PL", "PT", "QR", "QV", "RO", "SE", "SI", "SK", "XI" });

	public override CodeDescriptionPairList EntryStatusList => Factory.GetCachedValue<PLEntryStatusList>();

	public override CodeDescriptionPairList PaymentPartyList => Factory.GetCachedValue<PLMethodOfPaymentList>();

	protected override CodeDescriptionPairList EntryStyleListCore => Parent switch
	{
		_ when Parent.IsExitSummary => Factory.GetCachedValue<EntryStyleListExitSummary>(),
		_ when Parent.IsImport => Factory.GetCachedValue<EntryStyleListImport>(),
		_ => base.EntryStyleListCore
	};

	public RefVessel GetVesselByVesselName(ZString vesselName) => new RefVessel.Loader(Factory).LoadUnique(vesselName, ZString.Empty, ZString.Empty, ZString.Empty);

	public RefVessel GetVesselByLloydsNumber(ZString lloydImo) => new RefVessel.Loader(Factory).LoadUnique(ZString.Empty, lloydImo, ZString.Empty, ZString.Empty);
}
