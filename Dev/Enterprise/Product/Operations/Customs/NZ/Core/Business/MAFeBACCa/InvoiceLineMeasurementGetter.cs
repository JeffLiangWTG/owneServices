using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
using Enterprise.Customs.NZ.Business.MAFeBACCa.XMLSchemas;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa
{
	class InvoiceLineMeasurementGetter
	{
		internal InvoiceLineMeasurementGetter(MeasurementUQList measurementUnits)
		{
			this.measurementUnits = measurementUnits;
		}
		readonly MeasurementUQList measurementUnits;

		class Result
		{
			public Result(int measurementValue, string measurementUQ)
			{
				MeasurementValue = measurementValue;
				MeasurementUQ = measurementUQ;
			}
			public readonly int MeasurementValue;
			public readonly string MeasurementUQ;
		}

		internal bool TryGetMeasurementWithFallback(JobComInvoiceLine invoiceLine, out int value, out string unit)
		{
			Result result = GetGoodsMeasurementFromEnteredValue(invoiceLine)
				?? GetGoodsMeasurementFromCustomsQty(invoiceLine)
				?? GetGoodsMeasurementFromWeightOrVolume(invoiceLine);

			if (result != null)
			{
				value = result.MeasurementValue;
				unit = result.MeasurementUQ;
				return true;
			}

			value = 0;
			unit = string.Empty;
			return false;
		}

		static Result GetGoodsMeasurementFromWeightOrVolume(JobComInvoiceLine invoiceLine)
		{
			ZInt lineWeight = invoiceLine.EffectiveGrossWeight.InKilogramsSafe.ToZInt();
			if (lineWeight > 0)
			{
				return new Result(lineWeight, MeasurementUQList.Codes.kilograms);
			}

			ZInt lineVolume = invoiceLine.EffectiveVolume.InCubicMetres.ToZInt();
			if (lineVolume > 0)
			{
				return new Result(lineVolume, MeasurementUQList.Codes.cubicMetres);
			}

			return null;
		}

		Result GetGoodsMeasurementFromCustomsQty(JobComInvoiceLine invoiceLine)
		{
			ZString customsUnit = invoiceLine.JI_CustomsUnitQty;
			if (!customsUnit.IsEmpty)
			{
				string mafUnit = MeasurementUQList.TranslateFromCustomsStatisticalUQCode(invoiceLine.JI_CustomsUnitQty);
				if (!string.IsNullOrEmpty(mafUnit))
				{
					ZInt mafQty = invoiceLine.JI_CustomsQuantity.ToZInt();
					if (!mafQty.IsEmpty && measurementUnits.GetEnumValue(mafUnit).HasValue)
					{
						return new Result(mafQty, mafUnit);
					}
				}
			}
			return null;
		}

		Result GetGoodsMeasurementFromEnteredValue(JobComInvoiceLine invoiceLine)
		{
			if (invoiceLine.JI_MAF_MeasurementValue != 0)
			{
				GoodsMeasurementTypeMeasurementUnitQualifer? unit = measurementUnits.GetEnumValue(invoiceLine.JI_MAF_MeasurementUQ);
				if (unit.HasValue)
				{
					return new Result(invoiceLine.JI_MAF_MeasurementValue, invoiceLine.JI_MAF_MeasurementUQ);
				}
			}
			return null;
		}
	}
}
