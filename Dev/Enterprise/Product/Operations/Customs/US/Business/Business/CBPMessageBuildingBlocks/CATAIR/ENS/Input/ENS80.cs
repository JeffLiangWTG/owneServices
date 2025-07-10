using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class ENS80 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.ENS80, IBIRDSecondaryLineRecord
	{
		#region IBIRDSecondaryLineRecord Members

		void IBIRDSecondaryLineRecord.Update(JobComInvoiceLine secondaryLine, INotifications notifications)
		{
			IBIRDSecondaryLineRecord record = this;
			record.SetTariffRelatedDetails(secondaryLine, Quantity1, Unit1, Quantity2, Unit2, Quantity3, Unit3);

			secondaryLine.US_SecondarySPI = SpecialProgramsIndicatorSecondary;

			secondaryLine.CusEntryLine.Fees.UpdateOrAddCharge(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, Duty);

			record.UpdateValue(secondaryLine, Value);

			secondaryLine.US_SPI = SpecialProgramsIndicatorPrimaryOrCountry;
		}

		ZString IBIRDTariffRecord.Tariff
		{
			get { return TariffNumber3; }
		}

		#endregion
	}
}
