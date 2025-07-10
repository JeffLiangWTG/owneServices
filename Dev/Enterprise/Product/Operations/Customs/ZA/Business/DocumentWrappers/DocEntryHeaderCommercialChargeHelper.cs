using System.Collections.Generic;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	class DocEntryHeaderCommercialChargeHelper
	{
		public DocEntryHeaderCommercialChargeHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public void PrepareDocEntryHeaderCommercialChargeData(IEnumerable<BaseJobComInvoiceLine> invoiceLines)
		{
			foreach (var line in invoiceLines)
			{
				foreach (BaseInvoiceLineApportionedCharge charge in line.ApportionedCharges)
				{
					AddCharge(DocEntryHeaderCommercialCharge.New(charge, factory));
				}
			}
		}

		void AddCharge(DocEntryHeaderCommercialCharge charge)
		{
			var key = GetKeyForCharge(charge);

			if (chargeLookup.ContainsKey(key))
			{
				var existingCharge = chargeLookup[key];
				existingCharge.AddAdditionalAmountInChargeCurrency(charge.ChargeAmount);
			}
			else
			{
				chargeLookup[key] = charge;
			}
		}

		ZString GetKeyForCharge(DocEntryHeaderCommercialCharge charge)
		{
			var builder = new StringBuilder();
			builder.Append(charge.InvoiceNumber);

			if (charge.IsFreight)
			{
				builder.Append("_IsFreight");
			}
			else
			{
				builder.Append("_NotFreight");
			}

			if (charge.IsInsurance)
			{
				builder.Append("_IsInsurance");
			}
			else
			{
				builder.Append("_NotInsurance");
			}

			if (charge.IsDutiable)
			{
				builder.Append("_IsDutiable");
			}
			else
			{
				builder.Append("_NotDutiable");
			}

			if (charge.IsIncludedInLines)
			{
				builder.Append("_IsIncludedInLines");
			}
			else
			{
				builder.Append("_NotIncludedInLines");
			}

			builder.Append("_");
			builder.Append(charge.CurrencyCode);
			return builder.ToString();
		}

		public DocEntryHeaderCommercialChargesCollection GetDocEntryHeaderCommercialChargesCollection()
		{
			var allCharges = new DocEntryHeaderCommercialChargesCollection(factory);

			foreach (var charge in chargeLookup.Values)
			{
				allCharges.Add(charge);
			}
			return allCharges;
		}

		readonly BusinessObjectFactory factory;
		readonly Dictionary<ZString, DocEntryHeaderCommercialCharge> chargeLookup = new Dictionary<ZString, DocEntryHeaderCommercialCharge>();
	}
}
