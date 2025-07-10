using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business
{
	public class JobComInvoiceHeaderLookups : Customs.Business.JobComInvoiceHeaderLookups
	{
		public JobComInvoiceHeaderLookups(JobComInvoiceHeader parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList JZ_IncoTerm_List => Factory.GetCachedValue<NOIncotermCodeList>();

		public override ICodeDescriptionPairList ValuationCodeList
		{
			get
			{
				return RefCusCodeListTypes.GetCachedListMatchAnyAttributes(Factory, Core.Constants.CountryCodes.Norway,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature,
					ZDateTime.Today, null, includeParentDataGrouping: false);
			}
		}

		public CodeDescriptionPairList ValuationMethodList => Factory.GetCachedValue<ValuationMethodList>();
	}
}
