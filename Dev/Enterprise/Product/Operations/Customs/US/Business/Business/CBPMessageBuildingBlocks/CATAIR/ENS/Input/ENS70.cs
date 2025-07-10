using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class ENS70 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.ENS70, IBIRDSecondaryLineRecord
	{
		#region IBIRDSecondaryLineRecord Members

		void IBIRDSecondaryLineRecord.Update(JobComInvoiceLine secondaryLine, INotifications notifications)
		{
			IBIRDSecondaryLineRecord record = this;

			record.SetTariffRelatedDetails(secondaryLine, Quantity1, Unit1, Quantity2, Unit2, Quantity3, Unit3);
			record.UpdateValue(secondaryLine, Value);

			if (secondaryLine.JI_LinePrice == 0m)
			{
				var parentLine = secondaryLine.ParentTariffLine;
				if (parentLine != null && parentLine.ImportTariff != null && parentLine.ImportTariff.UE_DutyComputationCode == ComputationCodeList.Codes.Derived)
				{
					secondaryLine.JI_LinePrice = parentLine.JI_LinePrice;
					parentLine.JI_LinePrice = ZDecimal.Zero;
				}
			}

			secondaryLine.Factory.GetCachedValue<BIRDUpdateHeaderHelperTool>().UpdateOrWarn(secondaryLine, JobComInvoiceLine.Schema.US_SecondarySPI, SpecialProgramsIndicatorSecondary, SpecialProgramsIndicatorSecondary, "Special Programs Indicator Secondary", notifications);
			secondaryLine.CusEntryLine.Fees.UpdateOrAddCharge(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, Duty);

			secondaryLine.Factory.GetCachedValue<BIRDUpdateHeaderHelperTool>().UpdateOrWarn(secondaryLine, JobComInvoiceLine.Schema.US_SPI, SpecialProgramsIndicatorPrimaryOrCountry, SpecialProgramsIndicatorPrimaryOrCountry, "Special Programs Indicator Primary Or Country", notifications);
		}

		ZString IBIRDTariffRecord.Tariff
		{
			get { return TariffNumber2; }
		}

		#endregion
	}
}
