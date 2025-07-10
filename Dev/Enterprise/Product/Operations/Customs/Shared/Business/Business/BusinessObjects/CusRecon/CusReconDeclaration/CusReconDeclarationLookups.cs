using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusReconDeclarationLookups : CusReconBase.CusReconDeclarationLookups
	{
		public CusReconDeclarationLookups(CusReconDeclaration parent) : base(parent)
		{
		}

		public CodeDescriptionPairList ApplicationCodeList => Factory.GetCachedValue<CusReconDeclarationApplicationCodeList>();

		public virtual CustomsOfficeCodeCollection CustomsOfficeList => CustomsOfficeCodeCollection.GetCachedCollection(Factory, Parent.CountryCode, ZDateTime.Today);

		public virtual CusAuthorisationHeaderCollection AuthorizationList => Factory.GetCachedValue("CusReconDeclarationLookups.AuthorizationList", () => new CusAuthorisationHeaderCollection(Factory));

		public virtual CodeDescriptionPairList DeclarationTypeList => Factory.GetCachedValue<CusReconDeclarationTypeList>();

		public virtual CodeDescriptionPairList DeclarantTypeList => new CodeDescriptionPairList();

		public OrganisationsFindBoxCollection DeclarantList => Factory.GetCachedValue("CusReconDeclarationLookups.DeclarantList", () => new OrganisationsFindBoxCollection(Factory));

		public OrganisationsFindBoxCollection RepresentativeList => Factory.GetCachedValue("CusReconDeclarationLookups.RepresentativeList", () => new BrokerCollection(Factory));

		public OrganisationsFindBoxCollection BuyingAgentList => Factory.GetCachedValue("CusReconDeclarationLookups.BuyingAgentList", () => new OrganisationsFindBoxCollection(Factory));

		protected new CusReconDeclaration Parent => (CusReconDeclaration)base.Parent;
	}
}
