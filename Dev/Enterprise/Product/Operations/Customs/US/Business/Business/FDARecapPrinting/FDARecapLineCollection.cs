using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.FDARecapPrinting
{
	public class FDARecapLineCollection : NonPersistentBusinessObjectCollection<FDARecapLine>, IObsoleteValidation
	{
		public FDARecapLineCollection(CusEntryHeader entry)
			: base(entry.Factory)
		{
			PopulateCollection(entry);
		}

		void PopulateCollection(CusEntryHeader entry)
		{
			var previousInvoiceNumber = ZString.Empty;
			var addedFDAs = new System.Collections.Generic.List<FDA>();
			var printCountryOfOriginForLine = entry.UniqueCountryOfOrigin == USConstants.MultipleValueIndicator;
			foreach (CusEntryLine entryLine in entry.MergedLines)
			{
				foreach (FDA fda in entryLine.RandomLine.FDAs)
				{
					if (!addedFDAs.Contains(fda))
					{
						var line = new FDARecapLine(fda, previousInvoiceNumber != fda.InvoiceNumber);
						line.PrintCountryOfOriginForLine = printCountryOfOriginForLine;
						Add(line);
						previousInvoiceNumber = fda.InvoiceNumber;
						addedFDAs.Add(fda);
					}
				}
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject AddNewCore(Type bizOType)
		{
			throw new NotSupportedException();
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}
	}
}
