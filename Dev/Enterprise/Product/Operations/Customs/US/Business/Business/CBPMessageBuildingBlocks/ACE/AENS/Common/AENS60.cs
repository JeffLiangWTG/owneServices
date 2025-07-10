using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
{
	public partial class AENS60 : Abstract.AENS60, IACEBIRDLineRecord
	{
		#region IACEBIRDLineRecord Members

		void IACEBIRDLineRecord.Update(JobComInvoiceLine invoiceLine, INotifications notifications)
		{
			invoiceLine.US_TaxCode = AccountingClassCode;
			if (invoiceLine.ImportTariff != null)
			{
				foreach (ZString feeCode in invoiceLine.ImportTariff.GetRelatedFeeCodes())
				{
					if (CusFeeCodeConstants.IsExciseTax(feeCode))
					{
						invoiceLine.CusEntryLine.Fees.UpdateOrAddCharge(feeCode, IRTaxAmount);
						break;
					}
				}
			}
		}

		#endregion
	}
}
