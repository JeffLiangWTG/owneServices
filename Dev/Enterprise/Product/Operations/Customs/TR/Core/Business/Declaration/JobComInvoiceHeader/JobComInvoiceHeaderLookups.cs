using CargoWise.Integration;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class JobComInvoiceHeaderLookups : EU.Business.Declaration.JobComInvoiceHeaderLookups
	{
		public JobComInvoiceHeaderLookups(JobComInvoiceHeader parent) : base(parent)
		{
		}

		protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

		public override CodeDescriptionPairList JZ_IncoTerm_List => Factory.GetCachedValue<TRIncotermCodeList>();

		public CodeDescriptionPairList RelationCodeList => Factory.GetCachedValue<RelationCodeList>();

		public override ICodeDescriptionPairList ValuationCodeList
		{
			get
			{
				var dataGroupingCode = Core.Constants.CountryCodes.Turkey;
				return Factory.GetCachedValue("TRValuationCodeList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddRange(ZZRefCusCodeListCombined.Loader.Load(Factory,
						dataGroupingCode,
						TRValuationCodeType,
						Parent.EffectiveValuationDate));
					result.Sort();
					return result;
				});
			}
		}
		const string TRValuationCodeType = "TRNOB";
	}
}
