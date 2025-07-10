using Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaTaxPairList;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class AsycudaPackedItemTaxLookups : ASYCUDA.Business.AsycudaTaxLookups
	{
		public AsycudaPackedItemTaxLookups(ASYCUDA.Business.AsycudaTax parent) : base(parent)
		{
		}

		protected new AsycudaPackedItemTax Parent => (AsycudaPackedItemTax)base.Parent;

		public override CodeDescriptionPairList ChargeTypeList => Factory.GetCachedValue<ChargeTypeOtherList>();

		public ChildTariffViewCollection TariffCollection => ChildTariffViewCollection.GetNewCollection(Factory, Core.Constants.CountryCodes.Taiwan, Parent.AET_ChargeType, Parent.EffectiveAssessmentDate, null);

		public override CodeDescriptionPairList MethodOfPaymentList => Factory.GetCachedValue<TaxFeePaymentMethodList>();

		public CodeDescriptionPairList MethodOfCalculationList => TWRefCusCodeListTypes.GetMethodOfCalculationList(Factory);
	}
}
