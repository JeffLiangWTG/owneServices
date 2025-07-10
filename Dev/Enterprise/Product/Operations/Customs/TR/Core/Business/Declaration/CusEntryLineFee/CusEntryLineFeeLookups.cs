using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture.Core;
using CusEntryLineFee = Enterprise.Customs.TR.Business.Declaration.CusEntryLineFee;

namespace Enterprise.Customs.TR.Business
{
	public class CusEntryLineFeeLookups : EU.Business.Declaration.CusEntryLineFeeLookups
	{
		public CusEntryLineFeeLookups(CusEntryLineFee parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList MethodOfPaymentList => Factory.GetCachedValue<MethodOfPaymentList>();
		public override CodeDescriptionPairList MethodOfCalculationList => Factory.GetCachedValue<MethodOfCalculationList>();
		protected override TaxLookupsCommon GetNewCommonLookupsHelper() => new TRTaxLookupsCommon(Parent);

		public override CodeDescriptionPairList NationalFeeTypeCodeList
		{
			get
			{
				var codeDescriptionWithAllPairList = new CodeDescriptionPairList();
				codeDescriptionWithAllPairList.AddPair(DeclarationHelper.NationalVatType.Code, DeclarationHelper.NationalVatType.Description);
				codeDescriptionWithAllPairList.AddRange(ChargeTypeList);
				codeDescriptionWithAllPairList.RemoveCode(FeeTypeList.Codes.B00);
				codeDescriptionWithAllPairList.Sort();

				return codeDescriptionWithAllPairList;
			}
		}
	}
}
