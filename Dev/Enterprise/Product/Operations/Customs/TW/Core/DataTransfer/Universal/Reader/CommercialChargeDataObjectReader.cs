using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.DataTransfer.Universal;

public class CommercialChargeDataObjectReader : CommercialChargeDataObjectReader<BaseInvoiceCharge>
{
	public CommercialChargeDataObjectReader(CommercialCharge containerDataObject, IXmlImportLogger logger, ICommonNonApportionedChargeProvider<BaseInvoiceCharge> provider, UniversalObjectFactory factory)
		: base(containerDataObject, logger, provider, factory)
	{
	}

	protected override void SetPercentageOfLinePrice(IColumnIndexer chargeRow)
	{
		var amount = dataObject.Amount;
		var percentageOfLinePrice = dataObject.PercentageOfLinePrice;
		var percentageOfLinePriceHasValue = percentageOfLinePrice.HasValue;
		if (amount.GetValueOrDefault() > ZDecimal.Zero && !percentageOfLinePriceHasValue)
		{
			SetValue(chargeRow, JobComInvHeaderChargeSchema.J7_Percentage, ZDecimal.Zero);
		}
		else if (!amount.HasValue || amount.Value != ZDecimal.Zero || !percentageOfLinePriceHasValue || percentageOfLinePrice.Value != ZDecimal.Zero)
		{
			base.SetPercentageOfLinePrice(chargeRow);
		}
	}
}
