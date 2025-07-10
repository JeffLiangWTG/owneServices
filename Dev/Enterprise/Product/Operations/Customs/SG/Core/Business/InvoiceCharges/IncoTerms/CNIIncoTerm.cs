using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.SG.V4.Business
{
	public class CNIITOTIncoTermCalculator : Common.ITOTIncoTerm.IITOTIncoTermCalculator
	{
		public ZString IncoTerm => UnitPriceTermTypeCodeList.Codes.CNI;

		public ZString Calculate(ICommonInvoice invoice)
		{
			var result = IncoTerm;
			if (invoice.HasChargesExcludedInITOT(CustomsChargeTypeList.Codes.OverseasInsurance))
			{
				result = new Common.ITOTIncoTerm.FOBITOTIncoTermCalculator().Calculate(invoice);
			}
			return result;
		}
	}
}
