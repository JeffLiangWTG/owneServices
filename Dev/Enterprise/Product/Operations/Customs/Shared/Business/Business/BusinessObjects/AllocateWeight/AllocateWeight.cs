using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class AllocateWeight : AutoAllocateWeight
	{
		public AllocateWeight(BusinessObjectFactory factory, IEnumerable<BaseJobComInvoiceLine> baseJobComInvoiceLines)
			: base(factory)
		{
			InvoiceLines = Argument.NotNull(baseJobComInvoiceLines, nameof(baseJobComInvoiceLines));
		}

		public IEnumerable<BaseJobComInvoiceLine> InvoiceLines;

		public AllocateWeightLookups Lookups
		{
			get
			{
				if (lookups == null || !IsLookupsCachedInBase)
				{
					lookups = new AllocateWeightLookups(this);
				}
				return lookups;
			}
		}

		AllocateWeightLookups lookups;

		public override ZString SelectedLinesNumbersFormatString
		{
			get
			{
				if (selectedLinesNumbersFormatStringCached == null)
				{
					selectedLinesNumbersFormatStringCached = new CachedValue<ZString>(delegate
					{
						var selectedLinesList = new List<string>();
						foreach (var invoiceLineGroup in InvoiceLines.GroupBy(x => x.JI_Calc_Invoice).OrderBy(x => x.Key))
						{
							selectedLinesList.Add(GetLinesNumbersFormatString(invoiceLineGroup.Key, invoiceLineGroup.ToArray().OrderBy(c => c.JI_LineNo)));
						}
						return string.Join("; ", selectedLinesList);
					});
				}
				return selectedLinesNumbersFormatStringCached.Value;
			}
		}

		ZString GetLinesNumbersFormatString(ZString currentLineName, IOrderedEnumerable<BaseJobComInvoiceLine> invoiceLines)
		{
			var selectedLinesList = new List<string>();
			var from = -1;
			var to = -1;
			foreach (var baseJobComInvoiceLine in invoiceLines)
			{
				var currentLineNo = baseJobComInvoiceLine.JI_LineNo.ToZInt();
				if (from == -1)
				{
					from = currentLineNo;
					to = currentLineNo;
				}
				else
				{
					if (currentLineNo - to == 1)
					{
						to = currentLineNo;
					}
					else
					{
						AddSelectedLinesList(selectedLinesList, from, to);
						from = currentLineNo;
						to = currentLineNo;
					}
				}
			}
			if (from != -1)
			{
				AddSelectedLinesList(selectedLinesList, from, to);
			}
			return ZString.Format("{0}: {1}", currentLineName, string.Join(", ", selectedLinesList));
		}

		CachedValue<ZString> selectedLinesNumbersFormatStringCached;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			NetWeightUnit = Core.Constants.Weight.Kilograms;
			GrossWeightUnit = Core.Constants.Weight.Kilograms;
			AllocateWeightMethod = AllocateWeightMethodList.Codes.Price;
		}

		void AddSelectedLinesList(List<string> selectedLinesList, int from, int to)
		{
			if (to - from >= 2)
			{
				selectedLinesList.Add(from + "-" + to);
			}
			else if (to - from == 0)
			{
				selectedLinesList.Add(from.ToString());
			}
			else if (to - from == 1)
			{
				selectedLinesList.Add(from.ToString());
				selectedLinesList.Add(to.ToString());
			}
		}

		[List(nameof(Lookups) + "." + nameof(AllocateWeightLookups.WeightUQList))]
		public override ZString NetWeightUnit
		{
			get => base.NetWeightUnit;
			set => base.NetWeightUnit = value;
		}

		[List(nameof(Lookups) + "." + nameof(AllocateWeightLookups.WeightUQList))]
		public override ZString GrossWeightUnit
		{
			get => base.GrossWeightUnit;
			set => base.GrossWeightUnit = value;
		}

		[List(nameof(Lookups) + "." + nameof(AllocateWeightLookups.AllocateWeightMethodList))]
		public override ZString AllocateWeightMethod
		{
			get => base.AllocateWeightMethod;
			set => base.AllocateWeightMethod = value;
		}

		public ZBool IsPrice => AllocateWeightMethod == AllocateWeightMethodList.Codes.Price;

		public ZBool IsQuantity => AllocateWeightMethod == AllocateWeightMethodList.Codes.Quantity;
	}
}
