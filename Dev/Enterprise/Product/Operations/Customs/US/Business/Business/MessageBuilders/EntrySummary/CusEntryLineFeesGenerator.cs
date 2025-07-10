using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	/// <summary>
	/// Fees sent in 7501 should be aggregated fees of parent and its secondary tariff lines.
	/// </summary>
	class CusEntryLineFeesGenerator
	{
		public CusEntryLineFeesGenerator(CusEntryLine entryLine)
			: this(entryLine, false)
		{
		}

		public CusEntryLineFeesGenerator(CusEntryLine entryLine, bool includeTax)
		{
			this.entryLine = entryLine;
			this.includeTax = includeTax;
		}

		readonly CusEntryLine entryLine;
		readonly bool includeTax;

		public IEnumerable<IFee> Fees
		{
			get
			{
				Dictionary<ZString, ZDecimal> fees = new Dictionary<ZString, ZDecimal>();

				if (!entryLine.IsSecondaryTariffLine)
				{
					AggregateFeeAmount(entryLine, fees);
				}

				foreach (CusEntryLine secondary in entryLine.ChildSecondaryEntryLines)
				{
					AggregateFeeAmount(secondary, fees);
				}

				foreach (ZString feeType in fees.Keys)
				{
					Fee fee = new Fee();
					fee.Code = feeType;
					fee.Amount = fees[feeType];

					yield return fee;
				}
			}
		}

		void AggregateFeeAmount(CusEntryLine entryLine, Dictionary<ZString, ZDecimal> fees)
		{
			List<ZString> mandatoryFeeCodes = new List<ZString>();

			mandatoryFeeCodes.AddRange(entryLine.GetRequiredFees());

			if (entryLine.US_HasMPF)
			{
				mandatoryFeeCodes.Add(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
			}

			foreach (CusEntryLineFee fee in entryLine.Fees)
			{
				if (fee.CF_ChargeAmount > 0)
				{
					if (fee.IsLineFee || includeTax && CusFeeCodeConstants.IsExciseTax(fee.CF_ChargeType))
					{
						ZDecimal existingAmount;

						fees.TryGetValue(fee.CF_ChargeType, out existingAmount);

						fees[fee.CF_ChargeType] = existingAmount + fee.CF_ChargeAmount;

						mandatoryFeeCodes.Remove(fee.CF_ChargeType);
					}
				}
			}

			foreach (ZString mandatoryFeeCode in mandatoryFeeCodes)
			{
				if (!fees.ContainsKey(mandatoryFeeCode))
				{
					fees[mandatoryFeeCode] = 0;
				}
			}
		}
	}
}
