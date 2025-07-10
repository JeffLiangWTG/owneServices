using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class ExportAWBOtherChargesCollection : DependentBusinessObjectCollection<ExportAWBOtherCharges, ExportAWBHeader>, IEnumerable<ExportAWBOtherCharges>
	{
		public ExportAWBOtherChargesCollection(ExportAWBHeader master)
			: base(master)
		{
		}

		public IEnumerator<ExportAWBOtherCharges> GetEnumerator()
		{
			return Elements.Cast<ExportAWBOtherCharges>().GetEnumerator();
		}

		internal ZDecimal SumOf(ChargesDue otherChargesDue)
		{
			ZString entitlementCode = GetEntitlementCode(otherChargesDue);

			ZDecimal result = 0M;
			foreach (var charge in this)
			{
				if (charge.EO_EntitlementCode == entitlementCode)
				{
					result += charge.EO_Amount;
				}
			}

			return result;
		}

		internal ZDecimal SumOf(PaymentTerm paymentType, ChargesDue otherChargesDue)
		{
			ZString prepaidCollectCode = GetPrepaidCollectCode(paymentType);
			ZString entitlementCode = GetEntitlementCode(otherChargesDue);

			ZDecimal result = 0M;
			foreach (var otherCharge in this)
			{
				if (otherCharge.EO_PPDCLT == prepaidCollectCode && otherCharge.EO_EntitlementCode == entitlementCode)
				{
					result += otherCharge.EO_Amount;
				}
			}

			return result;
		}

		static ZString GetEntitlementCode(ChargesDue otherChargesDue)
		{
			switch (otherChargesDue)
			{
				case ChargesDue.Agent:
					return Enterprise.Core.Constants.AWB.EntitlementCode.Agent;

				case ChargesDue.Carrier:
					return Enterprise.Core.Constants.AWB.EntitlementCode.Carrier;
			}

			throw new InvalidOperationException("All values for OtherChargesDue must be mapped in this method.");
		}

		static ZString GetPrepaidCollectCode(PaymentTerm paymentType)
		{
			switch (paymentType)
			{
				case PaymentTerm.Prepaid:
					return ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;

				case PaymentTerm.Collect:
					return ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
			}

			throw new InvalidOperationException("All values for PaymentType must be mapped in this method.");
		}
	}

	public enum ChargesDue
	{
		Carrier,
		Agent
	}

	public enum PaymentTerm
	{
		Prepaid,
		Collect
	}
}
