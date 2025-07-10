using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Business
{
	public sealed class CustomsQuantityConverter : BaseCustomsQuantityConverter
	{
		const string NetWeightUnit = "KGM";

		public CustomsQuantityConverter(BaseJobComInvoiceLine invoiceLine, ZPropertyInfo customsQuantityInfo, ZPropertyInfo customsUnitOfQuantityInfo)
			: base(invoiceLine, customsQuantityInfo, customsUnitOfQuantityInfo)
		{
		}

		public override ZDecimal CalculateFromNetWeightToCustomsQtyCore()
		{
			var unitOfQuantity = (ZString)customsUnitOfQuantityInfo.Value;
			var unitCode = unitOfQuantity == NetWeightUnit ? (ZString)Core.Constants.Weight.Kilograms : unitOfQuantity;
			return Core.Constants.Weight.Convert(InvoiceLine.JI_NetWeight, InvoiceLine.JI_NetWeightUQ, unitCode);
		}

		public bool CanConvertUnitOfQuantity(ZString unitOfQuantity)
		{
			return unitOfQuantity == NetWeightUnit || Core.Constants.Weight.ContainsCode(unitOfQuantity);
		}
	}
}
