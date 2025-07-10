using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business
{
	static class RateViewExtensions
	{
		public static ZString[] GetUnitsOfMeasure(this RateView rate)
		{
			if (rate == null)
			{
				throw new ArgumentNullException(nameof(rate));
			}
			return rate.UnitsOfMeasure.Select(uom => uom.ZXG_UOM).ToArray();
		}

		public static ZString GetFirstUoM(this RateView rate) => rate.GetUnitsOfMeasure().FirstOrDefault();

		public static CodeDescriptionPair GetFirstUoMCodePair(this RateView rate)
		{
			var uom = rate.GetFirstUoM();
			return uom.IsEmpty ? null : JobComInvoiceLineLookups.FormulaUnitToRateType(uom);
		}
	}
}
