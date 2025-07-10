using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	internal class LCGSTBackRoundingCalculator
	{
		public LCGSTBackRoundingCalculator(BaseJobDeclaration declaration)
		{
			this.declaration = declaration;
			declaration.MergedSuccessfully += new BaseJobDeclaration.MergedSuccessfullyHandler(declaration_MergedSuccessfully);
			declaration.OnApportionmentDirtyChanged += declaration_OnApportionmentDirtyChanged;
			isGSTAmountDirty = true;
			isCustomsValueDirty = true;
		}

		public ZDecimal GetBackRoundedGSTAmount(ZGuid invoiceLinePK)
		{
			if (isGSTAmountDirty)
			{
				isGSTAmountDirty = false;
				PerformBackRounding("GSTAmounts", GSTAmounts,
					delegate(CusEntryLine entryLine)
					{ return entryLine.GSTVATAmount + entryLine.GSTVATDeferred; },
					delegate(BaseJobComInvoiceLine invoiceLine)
					{ return invoiceLine.GSTVATAmountForLandedCost + invoiceLine.JI_Calc_GSTVATDeferred; }
				);
			}

			ZDecimal result = 0m;

			GSTAmounts.TryGetValue(invoiceLinePK, out result);

			return result;
		}

		public ZDecimal GetBackRoundedCustomsValue(ZGuid invoiceLinePK)
		{
			if (isCustomsValueDirty)
			{
				isCustomsValueDirty = false;
				PerformBackRounding("CustomsValues", CustomsValues,
					delegate(CusEntryLine entryLine)
					{ return entryLine.CL_CustomsValue; },
					delegate(BaseJobComInvoiceLine invoiceLine)
					{ return GetApportionedCustomsValue(invoiceLine); }
				);
			}

			ZDecimal result = 0m;

			CustomsValues.TryGetValue(invoiceLinePK, out result);

			return result;
		}

		ZDecimal GetApportionedCustomsValue(BaseJobComInvoiceLine invoiceLine)
		{
			var result = ZDecimal.Zero;
			var entryLine = invoiceLine.CusEntryLine;

			if (invoiceLine.CanPerformApportionmentOfCusEntryLineValues)
			{
				if (entryLine.InvoiceLines.Count == 1)
				{
					result = entryLine.CL_CustomsValue;
				}
				else
				{
					var totalLinePrice = entryLine == null ? 0 : entryLine.InvoiceLines.Cast<BaseJobComInvoiceLine>().Sum(l => l.JI_LinePrice);
					result = totalLinePrice == 0 ? 0 : new ZDecimal(entryLine.CL_CustomsValue * invoiceLine.JI_LinePrice / totalLinePrice).Round(2);
				}
			}

			return result;
		}

		#region Implementation

		delegate ZDecimal GetEntryLineAmountDelegate(CusEntryLine entryLine);
		delegate ZDecimal GetInvoiceLineAmountDelegate(BaseJobComInvoiceLine invoiceLine);

		void PerformBackRounding(ZString dictionaryName, Dictionary<ZGuid, ZDecimal> amounts, GetEntryLineAmountDelegate getEntryLineAmount, GetInvoiceLineAmountDelegate getInvoiceLineAmount)
		{
			amounts.Clear();

			foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
			{
				foreach (CusEntryLine entryLine in entry.MergedLines)
				{
					ZDecimal entryLineAmount = getEntryLineAmount(entryLine);

					if (entryLineAmount > 0m)
					{
						ZDecimal totalOfInvoiceLines = 0m;
						BaseJobComInvoiceLine invoiceLineWithMaxAmount = null;

						foreach (BaseJobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
						{
							ZDecimal invoiceLineAmount = getInvoiceLineAmount(invoiceLine);
							if (invoiceLineWithMaxAmount == null || getInvoiceLineAmount(invoiceLineWithMaxAmount) < invoiceLineAmount)
							{
								invoiceLineWithMaxAmount = invoiceLine;
							}

							totalOfInvoiceLines += invoiceLineAmount;

							if (amounts.ContainsKey(invoiceLine.PK))
							{
								CargoWise.Common.ErrorReporter.ReportOnce("LCBackRoundingCalculator", string.Format(CultureInfo.CurrentCulture, "An invoice line has been added already to a dictionary '{0}'", dictionaryName));
							}

							amounts.Add(invoiceLine.PK, invoiceLineAmount);
						}

						if (entryLineAmount != totalOfInvoiceLines)
						{
							if (invoiceLineWithMaxAmount != null)
							{
								ZDecimal amount;
								amounts.TryGetValue(invoiceLineWithMaxAmount.PK, out amount);
								amounts[invoiceLineWithMaxAmount.PK] = amount + (entryLineAmount - totalOfInvoiceLines);
							}
						}
					}
				}
			}
		}

		readonly BaseJobDeclaration declaration;
		bool isGSTAmountDirty;
		bool isCustomsValueDirty;

		void declaration_MergedSuccessfully()
		{
			isGSTAmountDirty = true;
			isCustomsValueDirty = true;
		}

		void declaration_OnApportionmentDirtyChanged()
		{
			isGSTAmountDirty = true;
			isCustomsValueDirty = true;
		}

		Dictionary<ZGuid, ZDecimal> GSTAmounts
		{
			get { return gstAmounts ?? (gstAmounts = new Dictionary<ZGuid, ZDecimal>(declaration.InvoiceLines.Count)); }
		}
		Dictionary<ZGuid, ZDecimal> gstAmounts;

		Dictionary<ZGuid, ZDecimal> CustomsValues
		{
			get { return customsValues ?? (customsValues = new Dictionary<ZGuid, ZDecimal>(declaration.InvoiceLines.Count)); }
		}
		Dictionary<ZGuid, ZDecimal> customsValues;

		#endregion
	}
}
