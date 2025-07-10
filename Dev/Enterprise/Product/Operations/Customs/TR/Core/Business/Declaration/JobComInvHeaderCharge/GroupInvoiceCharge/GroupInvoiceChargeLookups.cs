using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class GroupInvoiceChargeLookups : EU.Business.Declaration.GroupInvoiceChargeLookups
	{
		public GroupInvoiceChargeLookups(EU.Business.Declaration.GroupInvoiceCharge groupInvoiceCharge) : base(groupInvoiceCharge)
		{
		}

		public override CodeDescriptionPairList ChargeTypeList
		{
			get
			{
				var isImport = Parent.GroupInvoice.JobDeclaration?.IsImport ?? false;

				return Factory.GetCachedValue<CodeDescriptionPairList>("TRChargeTypeList" + isImport, () =>
				{
					var result = new TRIncotermChargeCodeList();
					if (!isImport)
					{
						result.RemoveCode(TRIncotermChargeCodeList.Codes.DEM);
						result.RemoveCode(TRIncotermChargeCodeList.Codes.ROY);
						result.RemoveCode(TRIncotermChargeCodeList.Codes.OTH);
						result.RemoveCode(TRIncotermChargeCodeList.Codes.COM);
						result.RemoveCode(TRIncotermChargeCodeList.Codes.INT);
					}
					return result;
				});
			}
		}
	}
}
