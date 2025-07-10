using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class NZCommodityProductCollection : DependentCusAddInfoCollection<CommodityProduct, JobComInvoiceLine>
	{
		public NZCommodityProductCollection(JobComInvoiceLine master)
			: base(master, CusAddInfoTypeAttribute.Codes.NZTSWCommodityProductData)
		{
			UpdateMaxCountValidation();
		}

		protected override bool AllowNewCore
		{
			get { return MaxCount < 1 || Count < MaxCount; }
		}

		JobDeclaration Declaration
		{
			get { return fDeclaration ?? (fDeclaration = InvoiceLine != null ? InvoiceLine.Declaration : null); }
		}
		JobDeclaration fDeclaration;

		JobComInvoiceLine InvoiceLine
		{
			get { return fInvoiceLine ?? (fInvoiceLine = Master); }
		}
		JobComInvoiceLine fInvoiceLine;

		internal void UpdateMaxCountValidation()
		{
			if (Declaration != null && Declaration.IsTSWCREWriteOff)
			{
				MaxCountValidationEnable(1, "You are only allowed a maximum of 1 Commodity Product for CRE.");
			}
			else
			{
				MaxCountValidationDisable();
			}
		}
	}
}
