using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public abstract class WhsInventoryDutyAndTaxCalculator : Integration.Customs.IWhsInventoryDutyAndTaxCalculator
	{
		protected WhsInventoryDutyAndTaxCalculator(BusinessObjectFactory factory)
		{
			Factory = Argument.NotNull(factory, nameof(factory));
		}

		public void Calculate(Dictionary<ZString, IZType> data, ZDecimal ratio, ZDateTime arrivalDate, ZDateTime valuationDate, ZString tariff, ZString countryOfOrigin, ZDecimal customsValue, ZDecimal customsQty1, ZString customsUQ1, ZDecimal customsQty2, ZString customsUQ2, ZDecimal customsQty3, ZString customsUQ3)
		{
			CalculateDutyAndTax(data, ratio, arrivalDate, valuationDate, tariff, countryOfOrigin, customsValue, customsQty1, customsUQ1, customsQty2, customsUQ2, customsQty3, customsUQ3);
		}

		protected abstract void CalculateDutyAndTax(Dictionary<ZString, IZType> data, ZDecimal ratio, ZDateTime arrivalDate, ZDateTime valuationDate, ZString tariff, ZString countryOfOrigin, ZDecimal customsValue, ZDecimal customsQty1, ZString customsUQ1, ZDecimal customsQty2, ZString customsUQ2, ZDecimal customsQty3, ZString customsUQ3);

		public BusinessObjectFactory Factory { get; private set; }
	}
}
