using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
{
	public partial class AENS62 : Abstract.AENS62, IACEBIRDLineRecord
	{
		#region IACEBIRDLineRecord Members

		void IACEBIRDLineRecord.Update(JobComInvoiceLine invoiceLine, INotifications notifications)
		{
			invoiceLine.FeeCusCodes.UpdateOrAddCharge(AccountingClassCode, UserFeeAmount);

			invoiceLine.CusEntryLine.Fees.UpdateOrAddCharge(AccountingClassCode, UserFeeAmount);

			if (AccountingClassCode == Core.Constants.USCustoms.FeeCodes.HMF)
			{
				invoiceLine.Declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			}
		}

		#endregion
	}
}
