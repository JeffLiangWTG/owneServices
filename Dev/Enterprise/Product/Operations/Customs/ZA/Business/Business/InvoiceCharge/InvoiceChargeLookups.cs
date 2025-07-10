using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class InvoiceChargeLookups : Customs.Business.JobComInvHeaderChargeLookups
	{
		public InvoiceChargeLookups(InvoiceCharge invoiceCharge)
			: base(invoiceCharge)
		{
		}

		protected new InvoiceCharge Parent
		{
			get { return (InvoiceCharge)base.Parent; }
		}

		public override CodeDescriptionPairList ChargeDistributionBy
		{
			get
			{
				return Factory.GetCachedValue("ZAChargeDistributionBy", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value);
					return result;
				});
			}
		}
	}
}
