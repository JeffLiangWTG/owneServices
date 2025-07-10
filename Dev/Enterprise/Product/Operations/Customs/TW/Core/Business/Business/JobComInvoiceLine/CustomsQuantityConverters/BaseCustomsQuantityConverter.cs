using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public abstract class BaseCustomsQuantityConverter : Customs.Business.BaseCustomsQuantityConverter
	{
		public BaseCustomsQuantityConverter(JobComInvoiceLine invoiceLine, ZPropertyInfo quantityPropertyInfo, ZPropertyInfo unitPropertyInfo)
		: base(invoiceLine, quantityPropertyInfo, unitPropertyInfo)
		{ }

		protected new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		protected readonly int CustomsQuantityDecimalPlace = 4;

		public override ZDecimal CalculateFromNetWeightToCustomsQtyCore()
		{
			var unit = customsUnitOfQuantityInfo.Value.ToString();
			var mappedUnit = UnitConverterHelper.ConvertToCW1StandardWeightUnit(unit);
			var result = Core.Constants.Weight.Convert(InvoiceLine.JI_NetWeight, InvoiceLine.JI_NetWeightUQ, mappedUnit);
			return result;
		}

		protected override ZDecimal CalculateCustomsQuantityCore()
		{
			return decimal.Round(CustomsConversionFactor * InvoiceLine.JI_InvoiceQuantity, CustomsQuantityDecimalPlace);
		}
	}
}
