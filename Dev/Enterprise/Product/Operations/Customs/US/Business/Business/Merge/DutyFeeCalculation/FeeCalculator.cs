using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public delegate void SetFeeResultDelegate(FeeResult feeResult, string feeCode, IFeeCalculationDataProvider line);

	public class FeeCalculator
	{
		public FeeCalculator(BusinessObjectFactory factory, bool isHMFApplicable, SetFeeResultDelegate setFeeResult)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}
			this.factory = factory;
			this.isHMFApplicable = isHMFApplicable;
			this.setCalculatedFeeResult = setFeeResult;
		}

		internal FeeCalculator(BusinessObjectFactory factory, ZBool ignoreTIBExemptionCondition)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}
			this.factory = factory;
			this.ignoreTIBExemptionCondition = ignoreTIBExemptionCondition;
		}

		readonly BusinessObjectFactory factory;
		readonly bool isHMFApplicable;
		readonly SetFeeResultDelegate setCalculatedFeeResult;
		readonly ZBool ignoreTIBExemptionCondition;

		public void Calculate(IFeeCalculationDataProvider line)
		{
			Calculate(line, x => true);
		}

		public void Calculate(IFeeCalculationDataProvider line, Func<string, bool> isCustomsChargeRelevantForDecType)
		{
			if (!line.IsDomesticMerchandise && (!line.IsCombinedLine() || line.IsNormalTariffLine() || CalculateDutyForSetsHelper.IsCombinedXLine(line)))
			{
				var list = CusFeeCodeConstants.GetAccountingClassFeeCodeList(factory);

				foreach (CodeDescriptionPair pair in list)
				{
					if (!line.IsFeeOverriden(pair.Code))
					{
						FeeResult feeAmount = FeeResult.Empty;

						if (isCustomsChargeRelevantForDecType(pair.Code) && line.IsFeeApplicable(pair.Code, line.ImportTariff, line.DateForDutyCalculation))
						{
							feeAmount = CalculateAmount(line, pair.Code);
						}

						setCalculatedFeeResult(feeAmount, pair.Code, line);
					}
				}
			}
		}

		internal ZDecimal CalculateAllNonMPFAndHMFFeesForTIBBondCharge(IFeeCalculationDataProvider line)
		{
			ZDecimal result = ZDecimal.Zero;
			var list = CusFeeCodeConstants.GetAccountingClassFeeCodeList(factory);

			foreach (CodeDescriptionPair pair in list)
			{
				if (pair.Code != Core.Constants.USCustoms.FeeCodes.HMF && pair.Code != Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing)
				{
					result += CalculateAmount(line, pair.Code).Amount;
				}
			}

			return result;
		}

		internal void GetAllNonMPFAndHMFFeeDescForTIBBondCharge(IFeeCalculationDataProvider line, Dictionary<ZString, ZDecimal> feesToPrint)
		{
			var list = CusFeeCodeConstants.GetAccountingClassFeeCodeList(factory);

			foreach (CodeDescriptionPair pair in list)
			{
				if (pair.Code != Core.Constants.USCustoms.FeeCodes.HMF && pair.Code != Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing)
				{
					ZDecimal feeAmount = CalculateAmount(line, pair.Code).Amount;
					if (feeAmount > 0)
					{
						ZString key = pair.CodeAndDescription;
						ZDecimal fee;
						feesToPrint.TryGetValue(key, out fee);
						if (!feesToPrint.ContainsKey(key))
						{
							feesToPrint.Add(pair.CodeAndDescription, feeAmount);
						}
						else
						{
							feesToPrint.Remove(key);
							feesToPrint.Add(pair.CodeAndDescription, feeAmount + fee);
						}
					}
				}
			}
		}

		FeeResult CalculateAmount(IFeeCalculationDataProvider line, string feeCode)
		{
			ILineFeeCalculator feeCalculator = GetFeeCalculator(line, feeCode);

			FeeResult result = FeeResult.Empty;

			if (feeCalculator != null)
			{
				ZString rateType = (feeCode == line.TaxCode) ? line.TaxRateType : line.GetSelectedRateType(feeCode);
				FeeDutyData feeData = new FeeDutyData(line, rateType);

				result = feeCalculator.CalculateFee(feeData);
			}

			return result;
		}

		ILineFeeCalculator GetFeeCalculator(IFeeCalculationDataProvider line, ZString feeCode)
		{
			switch (feeCode.ToString())
			{
				case Core.Constants.USCustoms.FeeCodes.DutiableMail:
					return null;

				case Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing:
					return new MPFCalculator(factory);

				case Core.Constants.USCustoms.FeeCodes.Cotton:
					return new CottonFeeCalculator(ignoreTIBExemptionCondition, false);

				case Core.Constants.USCustoms.FeeCodes.HMF:
					return (isHMFApplicable ? new HarborMaintenanceFeeCalculator(factory) : null);

				case Core.Constants.USCustoms.FeeCodes.DairyFee:
					return new DairyFeeCalculator();

				case Core.Constants.USCustoms.FeeCodes.Raspberry:
					return new RaspberryFeeCalculator(ignoreTIBExemptionCondition);

				case Core.Constants.USCustoms.FeeCodes.Coffee:
					return new CoffeeFeeCalculator(ignoreTIBExemptionCondition);

				default:

					if (line.ImportTariff is USCTariff tariff && tariff.TaxFeeCode == feeCode && tariff.TaxFeeComputationCode == ComputationCodeList.Codes.AdValorem)
					{
						return new AdValoremFeeCalculator(feeCode, ignoreTIBExemptionCondition);
					}
					else
					{
						return new TaxFeeCalculator(feeCode, ignoreTIBExemptionCondition);
					}
			}
		}
	}
}
