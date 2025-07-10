using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class ReportingFeesDutyDataProvider : IFees, IEnumerable<ReportingFeeDutyDataProvider>
	{
		public ReportingFeeDutyDataProvider this[ZString feeType]
		{
			get
			{
				ReportingFeeDutyDataProvider fee = GetFeeFor(feeType);
				if (fee == null)
				{
					fee = AddNew();
					fee.Code = feeType;
				}
				return fee;
			}
		}

		public ReportingFeeDutyDataProvider GetFeeFor(ZString code) => Taxes.FirstOrDefault(x => x.Code == code);
		public ReportingFeeDutyDataProvider AddNew()
		{
			var result = new ReportingFeeDutyDataProvider();
			Taxes.Add(result);
			return result;
		}

		public IEnumerator<ReportingFeeDutyDataProvider> GetEnumerator()
		{
			return Taxes.GetEnumerator();
		}
		List<ReportingFeeDutyDataProvider> Taxes => taxes ?? (taxes = new List<ReportingFeeDutyDataProvider>());
		List<ReportingFeeDutyDataProvider> taxes;

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		IFee IFees.AddNew() => AddNew();
		IFee IFees.GetFeeFor(ZString code) => GetFeeFor(code);
	}
}
