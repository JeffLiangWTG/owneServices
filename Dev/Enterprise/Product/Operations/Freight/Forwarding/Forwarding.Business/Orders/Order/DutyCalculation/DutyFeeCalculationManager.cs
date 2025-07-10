using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public interface ICustomsDetails
	{
		ZString TariffNumber { get; }
		ZDecimal CustomsQuantity { get; }
		ZString CustomsUQ { get; }
	}

	class LCDutyFeeCalculationManager
	{
		public LCDutyFeeCalculationManager(Order order)
		{
			this.parent = order;
			this.dutyResults = new Dictionary<ZGuid, DutyResult>();
			this.feeResults = new Dictionary<ZGuid, Dictionary<ZString, ZDecimal>>();
			this.dutyFeeResults = new Dictionary<ZGuid, DutyTaxEntryFee>();
		}

		readonly Order parent;
		readonly Dictionary<ZGuid, DutyResult> dutyResults;
		readonly Dictionary<ZGuid, Dictionary<ZString, ZDecimal>> feeResults;
		readonly Dictionary<ZGuid, DutyTaxEntryFee> dutyFeeResults;

		public void Calculate()
		{
			dutyResults.Clear();
			feeResults.Clear();
			dutyFeeResults.Clear();

			switch (parent.CountryOfImport)
			{
				case Core.Constants.CountryCodes.Australia:
					auWrapper = null;
					IAUDutyCalculationManager manager = ObjectFactory.Get<IAUDutyCalculationManager>();
					manager.Calculate(new IHeaderFeeData[] { AUWrapper });
					break;
			}
		}

		AUChargeDataProviderWrapper AUWrapper
		{
			get { return auWrapper ?? (auWrapper = new AUChargeDataProviderWrapper(parent, new SetDutyResultDelegate(SetDutyResult), new SetFeeResultDelegate(SetFeeResult))); }
		}
		AUChargeDataProviderWrapper auWrapper;

		public DutyTaxEntryFee GetDutyTaxEntryFeeForLine(OrderLine orderLine)
		{
			DutyTaxEntryFee result;

			if (!dutyFeeResults.TryGetValue(orderLine.PK, out result))
			{
				result = new DutyTaxEntryFee();

				ZDecimal totalLinePrice = orderLine.Order.JD_Calc_TotalPrice;
				ZDecimal ratio = totalLinePrice == 0 ? 0m : orderLine.JO_LinePrice / totalLinePrice;
				DutyTaxEntryFee total = GetTotalDutyTaxEntryFee(orderLine.Order);

				switch (parent.CountryOfImport)
				{
					case Core.Constants.CountryCodes.Australia:

						result[CustomsDisbursementChargeCode.TotalDuty] = GetDutyResult(orderLine.PK).Amount.Amount;
						result[CustomsDisbursementChargeCode.EntryFees] = total["ENT"] * ratio;
						result[CustomsDisbursementChargeCode.QuarantineFees] = total["QUA"] * ratio;
						result[CustomsDisbursementChargeCode.SpecialTax1] = GetFeeResult(orderLine.PK, Enterprise.Registry.Business.Customs.AU.EntryChargeTypeList.Codes.WetAmount);
						result[CustomsDisbursementChargeCode.SpecialTax2] = GetFeeResult(orderLine.PK, Enterprise.Registry.Business.Customs.AU.EntryChargeTypeList.Codes.LCTAmount);
						result[CustomsDisbursementChargeCode.SpecialTax3] = total[CustomsDisbursementChargeCode.SpecialTax3] * ratio;

						break;
				}

				dutyFeeResults.Add(orderLine.PK, result);
			}

			return result;
		}

		public DutyTaxEntryFee GetTotalDutyTaxEntryFee(Order order)
		{
			DutyTaxEntryFee result;

			if (!dutyFeeResults.TryGetValue(order.PK, out result))
			{
				result = new DutyTaxEntryFee();

				switch (order.CountryOfImport)
				{
					case Core.Constants.CountryCodes.Australia:

						result[CustomsDisbursementChargeCode.TotalDuty] = GetTotalDutyAmount();

						result[CustomsDisbursementChargeCode.EntryFees] = GetTotalFeeAmount(Enterprise.Registry.Business.Customs.AU.EntryChargeTypeList.Codes.DeclarationProcessingCharge)
							+ GetTotalFeeAmount(Enterprise.Registry.Business.Customs.AU.EntryChargeTypeList.Codes.TotalPayableAdmin)
							+ GetTotalFeeAmount(Enterprise.Registry.Business.Customs.AU.EntryChargeTypeList.Codes.OtherCharges)
							+ GetTotalFeeAmount(Enterprise.Registry.Business.Customs.AU.EntryChargeTypeList.Codes.AQISProcessingCharge)
							+ GetTotalFeeAmount(Enterprise.Registry.Business.Customs.AU.EntryChargeTypeList.Codes.AQISContainerCharges);

						result[CustomsDisbursementChargeCode.QuarantineFees] = GetTotalFeeAmount(Enterprise.Registry.Business.Customs.AU.EntryChargeTypeList.Codes.AQISServicePaymentAmount);

						result[CustomsDisbursementChargeCode.SpecialTax1] = GetTotalFeeAmount(Enterprise.Registry.Business.Customs.AU.EntryChargeTypeList.Codes.WetAmount);
						result[CustomsDisbursementChargeCode.SpecialTax2] = GetTotalFeeAmount(Enterprise.Registry.Business.Customs.AU.EntryChargeTypeList.Codes.LCTAmount);
						result[CustomsDisbursementChargeCode.SpecialTax3] = GetFeeResult(order.PK, Enterprise.Registry.Business.Customs.AU.EntryChargeTypeList.Codes.Woodlevy);

						break;
				}

				dutyFeeResults.Add(order.PK, result);
			}

			return result;
		}

		ZDecimal GetTotalDutyAmount()
		{
			ZDecimal result = ZDecimal.Zero;

			foreach (DutyResult dutyResult in dutyResults.Values)
			{
				result += dutyResult.Amount.Amount;
			}

			return result;
		}

		ZDecimal GetTotalFeeAmount(ZString feeType)
		{
			ZDecimal result = ZDecimal.Zero;

			foreach (Dictionary<ZString, ZDecimal> fees in feeResults.Values)
			{
				ZDecimal feeAmount;

				fees.TryGetValue(feeType, out feeAmount);

				result += feeAmount;
			}

			return result;
		}

		public ICustomsDetails GetCustomsDetailsFor(OrderLine orderLine)
		{
			switch (parent.CountryOfImport)
			{
				case Core.Constants.CountryCodes.Australia:
					return AUWrapper.GetLineDutyWrapper(orderLine);

				default:
					return null;
			}
		}

		#region Implementation

		void SetFeeResult(ZGuid key, ZString feeType, ZDecimal feeAmount)
		{
			Dictionary<ZString, ZDecimal> fees;

			if (!feeResults.TryGetValue(key, out fees))
			{
				fees = new Dictionary<ZString, ZDecimal>();

				feeResults.Add(key, fees);
			}

			ZDecimal amount = GetFeeResult(key, feeType);
			amount += feeAmount;

			fees[feeType] = amount;
		}

		void SetDutyResult(ZGuid key, DutyResult dutyResult)
		{
			dutyResults[key] = dutyResult;
		}

		public DutyResult GetDutyResult(ZGuid key)
		{
			DutyResult result;

			if (!dutyResults.TryGetValue(key, out result))
			{
				result = new DutyResult();
			}

			return result;
		}

		public ZDecimal GetFeeResult(ZGuid key, ZString feeType)
		{
			ZDecimal result = 0m;

			Dictionary<ZString, ZDecimal> fees;

			if (feeResults.TryGetValue(key, out fees))
			{
				fees.TryGetValue(feeType, out result);
			}

			return result;
		}

		#endregion

	}
}
