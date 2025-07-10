using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using static Enterprise.Customs.TW.Business.MessageConstants;

namespace Enterprise.Customs.TW.Business
{
	public class DutyOtherTaxFeeWrapper : IDutyOtherTaxFee
	{
		public DutyOtherTaxFeeWrapper(ZString chargeType, ZString methodOfCalculation, ZDecimal rate)
		{
			this.chargeType = chargeType;
			this.methodOfCalculation = methodOfCalculation;
			this.rate = rate;
		}

		readonly ZDecimal rate;

		readonly ZString chargeType;

		readonly ZString methodOfCalculation;

		ZString IDutyOtherTaxFee.MethodOfCalculation => methodOfCalculation;

		ZString IDutyOtherTaxFee.MethodCode => methodOfCalculation == UniversalReferenceConstants.MethodOfCalculation.Percentage ? MethodCodes._1 : MethodCodes._2;

		ZDecimal IDutyOtherTaxFee.TaxRateNumeric => rate;

		ZString IDutyOtherTaxFee.TypeCode => chargeType;

		public ZDecimal PercentageNumeric => ZDecimal.Zero;
	}
}
