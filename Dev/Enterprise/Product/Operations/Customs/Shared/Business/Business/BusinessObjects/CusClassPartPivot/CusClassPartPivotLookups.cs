//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusClassPartPivotLookups
//
//    This class should be used for overriding collections in AutoCusClassPartPivotLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusClassPartPivotLookups : AutoCusClassPartPivotLookups
	{
		public CusClassPartPivotLookups(AutoCusClassPartPivot parent) : base(parent)
		{
		}

		public new BaseCusClassPartPivot Parent => (BaseCusClassPartPivot)base.Parent;

		public GlbStaffCollection Staffs => new GlbStaffCollection(Factory);

		public virtual CodeDescriptionPairList ClassificationTypes => Factory.GetCachedValue<ClassificationTypeList>();

		public virtual IBaseClassificationCollection<BaseCusClassification> ClassificationList => new BaseClassificationCollection<BaseCusClassification>(Factory);

		public virtual IBaseCusGoodsCatalogCollection<BaseCusGoodsCatalog> GoodsCatalogList => new BaseCusGoodsCatalogCollection<BaseCusGoodsCatalog>(Factory);

		public virtual ICodeDescriptionPairList RelatedIndicatorList => Factory.GetCachedValue<RelatedIndicatorList>();

		public virtual BusinessObjectCollection Tariffs => new TariffViewCollection(Factory, Parent.CI_RN_NKCountry);

		public OrgHeaderCollection RelatedOrgs
		{
			get
			{
				var query = new ZQuery(OrgHeaderSchema.OH_IsConsignee, true);
				query.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsConsignor, true);
				return new OrgHeaderCollection(Factory, query);
			}
		}

		public virtual ICodeDescriptionPairList PrimaryPreferenceList => UniversalReferenceDataHelper.GetPreferenceList(Factory,
																											Parent.UseUniversalTariff,
																											Parent.CI_TariffNum,
																											Parent.CI_RN_NKCountryOfOrigin,
																											Parent.UniversalTariff,
																											Parent.AllApplicableRatesSelectionCriteria,
																											Parent?.CI_RN_NKCountry ?? Env.CurrentCompany.Country.Code);
	}
}
