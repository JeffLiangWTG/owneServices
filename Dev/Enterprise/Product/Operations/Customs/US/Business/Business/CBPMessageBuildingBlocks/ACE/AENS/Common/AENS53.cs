
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
{
	public partial class AENS53 : Abstract.AENS53, IACEBIRDLineRecord
	{
		#region IACEBIRDLineRecord Members

		void IACEBIRDLineRecord.Update(JobComInvoiceLine invoiceLine, INotifications notifications)
		{
			if (CaseNumber.StartsWith("A", System.StringComparison.OrdinalIgnoreCase))
			{
				invoiceLine.US_ADDCaseNo = CaseNumber;
				invoiceLine.US_IsBondedADD = BondCashClaimCode == "B";
				invoiceLine.US_ADDDepositRateIndicator = CaseRateTypeQualifierCode;
				invoiceLine.US_ADDDepositValue = ADCVDValueOfGoodsAmount;
				invoiceLine.US_ADDQty = ADCVDQuantity;
				invoiceLine.CusEntryLine.Fees.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty, ADCVDDutyAmount);
			}
			else
			{
				invoiceLine.US_CVDCaseNo = CaseNumber;
				invoiceLine.US_IsBondedCVD = BondCashClaimCode == "B";
				invoiceLine.US_CVDDepositRateIndicator = CaseRateTypeQualifierCode;
				invoiceLine.US_CVDDepositValue = ADCVDValueOfGoodsAmount;
				invoiceLine.US_CVDQty = ADCVDQuantity;
				invoiceLine.CusEntryLine.Fees.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.CountervailingDuty, ADCVDDutyAmount);
			}
		}

		#endregion
	}
}
