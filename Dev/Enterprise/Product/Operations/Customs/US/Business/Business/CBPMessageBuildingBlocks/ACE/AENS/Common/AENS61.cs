using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
{
	public partial class AENS61 : Abstract.AENS61, IACEBIRDLineRecord
	{
		#region IACEBIRDLineRecord Members

		void IACEBIRDLineRecord.Update(JobComInvoiceLine invoiceLine, INotifications notifications)
		{
			invoiceLine.FeeCusCodes.UpdateOrAddCharge(AccountingClassCode, UserFeeAmount);
			invoiceLine.CusEntryLine.Fees.UpdateOrAddCharge(AccountingClassCode, UserFeeAmount);
		}

		#endregion
	}
}
