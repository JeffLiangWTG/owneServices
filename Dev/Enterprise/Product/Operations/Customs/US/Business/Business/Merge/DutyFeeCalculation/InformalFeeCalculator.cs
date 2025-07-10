using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business
{
	class InformalFeeCalculator
	{
		#region IFeeCalculator Members

		public FeeResult CalculateFee(IDutyDataLineHeader entryHeader)
		{
			ZDecimal result = ZDecimal.Zero;

			if (entryHeader.IsInformalFeeApplicable)
			{
				foreach (IEntryLineOrInvoiceLineDutyData line in entryHeader.DutyDataLines)
				{
					if (DoesAttractFee(line))
					{
						var date = line.FeeDataProviders.FirstOrDefault()?.DateForMPFCalculation;
						if (date.HasValue && date.Value.IsValid)
						{
							result = new FeeCalculationHelper(entryHeader.Factory, date.Value).InformalFeeAmount;
							break;
						}
					}
				}
			}
			return new FeeResult(result);
		}

		bool DoesAttractFee(IDutyData line)
		{
			return line.ShouldHaveMPFOrInformalFee(x => DoesAttractFeeCore(x));
		}

		bool DoesAttractFeeCore(IDutyData line)
		{
			return !line.IsSetVLine && !new MPFAndInformalFeeExemptConditionChecker(false).IsExempt(line);
		}

		#endregion
	}

	static class InformalFeeApplicableCalculator
	{
		public static bool IsApplicable(ZString entryType, ZString transportMode, ZString portOfEntry)
		{
			bool result = false;

			if (entryType == EntryTypeList.Codes.InformalFreeDutiable || entryType == EntryTypeList.Codes.InformalQuotaVisa)
			{
				ZString portOfEntryThirdPosition = portOfEntry.SubstringSafe(2, 1);

				result = transportMode != TransportModeCodes.Codes.Mail
					&& portOfEntryThirdPosition != "7"//express courier facilities 
					&& portOfEntryThirdPosition != "9"//central hubs for the couriers
					;
			}

			return result;
		}
	}

	class DutiableMailFeeCalculator
	{
		public ZDecimal Calculate(IDutyDataLineHeader entry)
		{
			ZDecimal result = 0m;

			if (entry.IsDutiableMailFeeApplicable)
			{
				result = new FeeCalculationHelper(entry.Factory, entry.DateForFeeCalculation).MailFee;
			}

			return result;
		}
	}
}
