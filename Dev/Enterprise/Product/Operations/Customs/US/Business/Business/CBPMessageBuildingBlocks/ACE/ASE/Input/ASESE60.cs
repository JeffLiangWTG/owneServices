using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	public partial class ASESE60 : Abstract.ASESE60, IACEBIRDSecondaryLineRecord
	{
		#region IACEBIRDSecondaryLineRecord Members

		void IACEBIRDSecondaryLineRecord.Update(JobComInvoiceLine invoiceLine, bool isSupplementaryTariff, INotifications notifications)
		{
			this.UpdateValue(invoiceLine, LineItemValue);

			var parentTariffLine = invoiceLine.ParentTariffLine;
			if (invoiceLine.IsSecondaryTariffLine && parentTariffLine != null)
			{
				invoiceLine.JI_Weight = parentTariffLine.JI_Weight;
				parentTariffLine.JI_Weight = ZDecimal.Zero;

				var totalFTZPackQty = parentTariffLine.US_ManifestQty;
				if (totalFTZPackQty > 0)
				{
					var halfPackQty = totalFTZPackQty / 2;
					invoiceLine.US_ManifestQty = halfPackQty;
					parentTariffLine.US_ManifestQty = totalFTZPackQty - halfPackQty;
				}

				if (parentTariffLine.JI_LinePrice > 0m && new ImportLinePriceValidator().ShouldBeDeclaredAtSecondary(parentTariffLine))
				{
					invoiceLine.JI_LinePrice = parentTariffLine.JI_LinePrice;
					parentTariffLine.JI_LinePrice = 0m;
				}
			}
		}

		ZString IBIRDTariffRecord.Tariff
		{
			get { return HTSNumber; }
		}

		#endregion  

	}
}
