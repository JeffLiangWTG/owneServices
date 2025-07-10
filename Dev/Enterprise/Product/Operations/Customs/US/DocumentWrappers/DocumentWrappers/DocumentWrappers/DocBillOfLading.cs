using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.DocumentWrappers
{
	public class DocBillOfLading : Enterprise.DocumentWrappers.Customs.Base.DocBaseBillOfLading
	{
		public DocBillOfLading(Bill billOfLading)
			: base(billOfLading)
		{
		}

		protected override ZString GetBillNumberWithCodeCore(Customs.Business.Bill bill)
		{
			return ((Bill)bill).SCACAndBillNumber;
		}

		protected override ZString GetAdditionalNumber(Customs.Business.Bill bill)
		{
			var result = base.GetAdditionalNumber(bill);
			var itNumbers = ((Bill)bill).AllITNumbers.ToArray();
			if (itNumbers.Length > 0)
			{
				result += "     IT:" + string.Join(",", itNumbers);
			}

			return result;
		}
	}
}
