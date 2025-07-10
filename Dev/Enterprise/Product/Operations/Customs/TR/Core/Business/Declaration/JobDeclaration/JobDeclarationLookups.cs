using System.Collections;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public partial class JobDeclarationLookups : EU.Business.Declaration.JobDeclarationLookups
	{
		public JobDeclarationLookups(JobDeclaration parent) : base(parent)
		{
		}

		protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

		public override CustomsOfficeCodeCollection CustomsOffices => CustomsOfficeCodeCollection.LocalCountryOnlyCustomsOfficesWithRequiredRoles(Factory, Core.Constants.CountryCodes.Turkey);

		protected override CodeDescriptionPairList EntryStyleListCore => Factory.GetCachedValue<EntryStyleList>();

		public CodeDescriptionPairList EntrySubStyleList => Factory.GetCachedValue<EntrySubStyleList>();

		public CustomsOfficeCodeCollection TRCustomsOfficeList => CustomsOfficeCodeCollection.LocalCountryOnlyCustomsOfficesWithRequiredRoles(Factory, Core.Constants.CountryCodes.Turkey);

		public CodeDescriptionPairList TransportModeInland => Factory.GetCachedValue<TRTransportModeInland>();

		protected override ICollection LocationsCore =>
			RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyWarehouseCodes, Parent.DateOfValuation);

		public override CodeDescriptionPairList PaymentPartyList => Factory.GetCachedValue<DutyPaymentTypeList>();

		public CodeDescriptionPairList TRCustomsPortList => RefCusCodeListTypes.GetCachedList(Factory, Parent.CountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, Parent.DateOfValuation, null, string.Empty, false);

		public ZZRefCusCodeListCombinedCollection BondedWarehouseCodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyBondedWarehouseCodes, Parent.DateOfValuation);

		public override CodeDescriptionPairList CargoIdTypeList
		{
			get
			{
				return Factory.GetCachedValue("TR.JobDeclaration.Lookups.JE_ContainerMode", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised);
					return result;
				});
			}
		}

		public override CodeDescriptionPairList EntryStatusList => Factory.GetCachedValue<EntryStatusTypeList>();

		public CodeDescriptionPairList OrderTypesOfGoodsList => Factory.GetCachedValue<OrderTypesOfGoodsList>();

		public override CodeDescriptionPairList TransportMeansList => Factory.GetCachedValue<TransportMeans>();
	}
}
