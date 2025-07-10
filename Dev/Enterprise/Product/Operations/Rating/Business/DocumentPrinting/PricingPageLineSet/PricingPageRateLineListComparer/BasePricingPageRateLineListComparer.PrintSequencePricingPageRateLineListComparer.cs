using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	public partial class BasePricingPageRateLineListComparer
	{
		sealed class PrintSequencePricingPageRateLineListComparer : BasePricingPageRateLineListComparer
		{
			public PrintSequencePricingPageRateLineListComparer(OrgHeader client, ZString jobType)
			{
				this.client = client;
				this.jobType = jobType;
				this.cache = new Dictionary<ZGuid, short>();
			}

			public override int Compare(PricingPageRateLineList x, PricingPageRateLineList y)
			{
				var code1 = x.Count > 0 ? x[0].ChargeCode : null;
				var code2 = y.Count > 0 ? y[0].ChargeCode : null;

				var result = Compare(code1, code2);

				if (result == 0)
				{
					result = StringComparer.OrdinalIgnoreCase.Compare
					(
						x.Count > 0
							? x[0].GetRateDescOrRateDescLocal()
							: ZString.Empty,
						y.Count > 0
							? y[0].GetRateDescOrRateDescLocal()
							: ZString.Empty
					);

					if (result == 0)
					{
						result = LineSequencePricingPageRateLineListComparer.CompareOrders(x, y);
					}
				}

				return result;
			}

			public override bool Equals(BasePricingPageRateLineListComparer other)
			{
				if (base.Equals(other))
				{
					var comp = (PrintSequencePricingPageRateLineListComparer)other;
					return comp.jobType == jobType && comp.client == client;
				}

				return false;
			}

			int Compare(AccChargeCode code1, AccChargeCode code2)
			{
				if (code1 == null)
				{
					return code2 == null ? 0 : 1;
				}
				else
				{
					return code2 == null ? -1 : GetPrintOrder(code1).CompareTo(GetPrintOrder(code2));
				}
			}

			short GetPrintOrder(AccChargeCode chargeCode)
			{
				short value;

				if (!cache.TryGetValue(chargeCode.PK, out value))
				{
					value = CalculatePrintOrder(chargeCode);
					cache.Add(chargeCode.PK, value);
				}

				return value;
			}

			short CalculatePrintOrder(AccChargeCode chargeCode)
			{
				short? result = null;

				if (client != null)
				{
					foreach (var order in client.InvoiceOrders)
					{
						if (order.AI_AC != chargeCode.PK)
						{
							continue;
						}

						if (order.AI_InvoiceType == jobType)
						{
							return order.AI_PrintOrder;
						}
						else if (order.AI_InvoiceType.IsEmpty || order.AI_InvoiceType == AccClientInvoiceOrderLookups.InvoiceTypes.All.Code)
						{
							result = order.AI_PrintOrder;
						}
					}
				}

				return result.GetValueOrDefault(chargeCode.AC_PrintSequence);
			}

			readonly ZString jobType;
			readonly OrgHeader client;
			readonly Dictionary<ZGuid, short> cache;
		}
	}
}
