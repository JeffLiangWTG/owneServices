using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class ReconRefundedChargeLookups : CusCodeDataLookups
	{
		public ReconRefundedChargeLookups(ReconRefundedCharge charge)
			: base(charge)
		{
		}

		public override CodeDescriptionPairList CY_CodeList
		{
			get
			{
				return Factory.GetCachedValue("ReconRefundedCharge CY_CodeList", delegate
				{
					var list = new CodeDescriptionPairList();
					var codes = ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode, ZDateTime.Now);
					list.AddRange(codes);
					foreach (var code in CusFeeCodeConstants.GetTaxCodes())
					{
						list.RemoveCode(code);
					}
					list.Sort();
					return list;
				});
			}
		}
	}
}
