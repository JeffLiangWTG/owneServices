
using CargoWise.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class JobComInvoiceLineTaxLookups : EU.Business.Declaration.JobComInvoiceLineTaxLookups
	{
		public JobComInvoiceLineTaxLookups(JobComInvoiceLineTax parent) : base(parent)
		{ }

		public new JobComInvoiceLineTax Parent => (JobComInvoiceLineTax)base.Parent;

		public override CodeDescriptionPairList RateOverrideList => Factory.GetCachedValue<RateOverrideReasonList>();

		public override CodeDescriptionPairList MOPList => Factory.GetCachedValue<MethodOfPaymentList>();

		public override CodeDescriptionPairList MethodOfCalculationList => Factory.GetCachedValue<MethodOfCalculationList>();

		protected override EU.Business.Declaration.MultiLineAddInfos.TaxLookupsCommon GetNewCommonLookupsHelper() => new TRTaxLookupsCommon(Parent);

		public override ICodeDescriptionPairList TypeList
		{
			get
			{
				var codeDescriptionWithAllPairList = new CodeDescriptionPairList();
				codeDescriptionWithAllPairList.AddPair(DeclarationHelper.NationalVatType.Code, DeclarationHelper.NationalVatType.Description);
				codeDescriptionWithAllPairList.AddRange(base.TypeList);
				codeDescriptionWithAllPairList.RemoveCode(FeeTypeList.Codes.B00);
				codeDescriptionWithAllPairList.Sort();

				return codeDescriptionWithAllPairList;
			}
		}
	}
}
