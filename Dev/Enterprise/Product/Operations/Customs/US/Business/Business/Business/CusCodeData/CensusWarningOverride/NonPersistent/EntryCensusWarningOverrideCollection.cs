using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class EntryCensusWarningOverrideCollection : NonPersistentBusinessObjectCollection<EntryCensusWarningOverride>
	{
		public EntryCensusWarningOverrideCollection(CusEntryHeader entry)
			: base(entry.Factory)
		{
			this.entry = entry;
			BuildCollection();
		}

		public readonly CusEntryHeader entry;

		public void CopyToEntryLines()
		{
			var cwosByEntryLine = new Dictionary<CusEntryLine, List<EntryCensusWarningOverride>>();

			foreach (EntryCensusWarningOverride cwo in this)
			{
				var entryLine = cwo.EntryLine;

				if (entryLine != null)
				{
					//Collect all CWO's that would go to the same invoice line
					if (!entryLine.RandomLine.HasEmptySupTariff && !entryLine.US_SupLine)
					{
						entryLine = entryLine.RandomLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);

						if (entryLine == null)
						{
							entryLine = cwo.EntryLine;
						}
					}

					List<EntryCensusWarningOverride> cwos;

					if (!cwosByEntryLine.TryGetValue(entryLine, out cwos))
					{
						cwos = new List<EntryCensusWarningOverride>();
						cwosByEntryLine.Add(entryLine, cwos);
					}

					cwos.Add(cwo);
				}
			}

			foreach (JobComInvoiceLine invoiceLine in entry.InvoiceLines)
			{
				List<EntryCensusWarningOverride> cwos;

				var entryLine = (invoiceLine.HasEmptySupTariff ? invoiceLine.CusEntryLine : invoiceLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true)) ?? invoiceLine.CusEntryLine;

				if (!cwosByEntryLine.TryGetValue(entryLine, out cwos))
				{
					cwos = new List<EntryCensusWarningOverride>();
				}

				invoiceLine.CensusWarningOverrides.CopyFrom(cwos);
			}
		}

		public bool HasErrors()
		{
			foreach (var cwo in this)
			{
				if (cwo.HasErrors)
				{
					return true;
				}
			}
			return false;
		}

		public bool HasMessageErrors()
		{
			foreach (var cwo in this)
			{
				if (cwo.HasMessageErrors)
				{
					return true;
				}
			}
			return false;
		}

		#region Implementation

		void BuildCollection()
		{
			RemoveAll();

			entry.MergedLines.Sort(new EntrySummaryEntryLineComparerForNumbering());

			foreach (CusEntryLine entryLine in entry.MergedLines)
			{
				foreach (CensusWarningOverride cwo in entryLine.RandomLine.CensusWarningOverrides)
				{
					if (!HasThisCW(cwo.CY_Code, entryLine))
					{
						var newElement = AddNew();
						newElement.EntryLinePK = entryLine.PK;
						newElement.ConditionCode = cwo.CY_Code.Left(newElement.ConditionCodeInfo.MaxLength);
						newElement.OverrideCode = cwo.CY_Data.Left(newElement.OverrideCodeInfo.MaxLength);
					}
				}
			}
		}

		public void DefaultCustomsCWOs()
		{
			entry.MergedLines.Sort(new EntrySummaryEntryLineComparerForNumbering());

			foreach (CusEntryLine entryLine in entry.MergedLines)
			{
				foreach (ZString cwo in entryLine.US_CWOs.Split(3))
				{
					if (!HasThisCW(cwo, entryLine))
					{
						var newElement = AddNew();
						newElement.EntryLinePK = entryLine.PK;
						newElement.ConditionCode = cwo;
					}
				}
			}
		}

		bool HasThisCW(ZString code, CusEntryLine entryLine)
		{
			foreach (EntryCensusWarningOverride cwo in this)
			{
				if (cwo.ConditionCode.EqualsIgnoringCase(code) && (cwo.EntryLine == entryLine || cwo.EntryLine.CL_LineNumber == entryLine.CL_LineNumber))
				{
					return true;
				}
			}
			return false;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new EntryCensusWarningOverride(entry, this);
		}

		#endregion
	}
}
