using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class ENS62 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.ENS62, IBIRDLineRecord
	{
		#region IBIRDLineRecord Members

		void IBIRDLineRecord.Update(JobComInvoiceLine invoiceLine, INotifications notifications)
		{
			invoiceLine.FeeCusCodes.UpdateOrAddCharge(ClassCode, UserFeeAmount);

			invoiceLine.CusEntryLine.Fees.UpdateOrAddCharge(ClassCode, UserFeeAmount);

			if (ClassCode == Core.Constants.USCustoms.FeeCodes.HMF)
			{
				invoiceLine.Declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			}
		}

		ZString IBIRDTariffRecord.Tariff
		{
			get { return ZString.Empty; }
		}

		#endregion
	}
}
