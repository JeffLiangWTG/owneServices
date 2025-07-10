using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class DrawbackOtherFeeCollection : DependentCusAddInfoCollection<DrawbackOtherFee, JobComInvoiceLine>
	{
		public DrawbackOtherFeeCollection(JobComInvoiceLine master)
			: base(master, CusAddInfoTypeAttribute.Codes.USDrawbackOtherFee)
		{
		}

		protected override bool AllowNewCore
		{
			get
			{
				var result = false;
				var invoiceLine = Master;
				if (invoiceLine != null)
				{
					result = !invoiceLine.IsAutoCalculated && (invoiceLine?.Declaration.IsACEDrawback ?? false);
				}

				return result;
			}
		}

		public DrawbackOtherFee this[string feeType]
		{
			get { return this.OfType<DrawbackOtherFee>().FirstOrDefault(x => x.US_FeeType == feeType); }
		}

		public ZBool HasDuplicateFeeType(ZString feeType)
		{
			var otherFees = this.OfType<DrawbackOtherFee>().Where(x => x.US_FeeType == feeType);
			return otherFees.Count() > 1;
		}

		public DrawbackOtherFee AddNewOrUpdate(ZString feeTypeCode, ZDecimal feeAmount)
		{
			var otherFee = this[feeTypeCode];
			if (otherFee == null)
			{
				otherFee = this.AddNew();
				otherFee.US_FeeType = feeTypeCode;
			}

			otherFee.DeclaredAmount = feeAmount;
			return otherFee;
		}
	}
}
