using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class ReconOriginalEntryHeaderCollection : NonPersistentBusinessObjectCollection<ReconOriginalEntryHeader>
	{
		public ReconOriginalEntryHeaderCollection(JobDeclaration declaration, ReconDeclaration reconDeclaration)
			: base(reconDeclaration.Factory)
		{
			this.declaration = declaration;
			this.reconDeclaration = reconDeclaration;
			PopulateCollection();
		}

		readonly JobDeclaration declaration;
		readonly ReconDeclaration reconDeclaration;

		public void ResetReconCharges()
		{
			foreach (ReconOriginalEntryHeader entry in this)
			{
				entry.ResetReconChargesIfNecessary();
			}
		}

		public bool Contains(CusEntryHeader entryPassed)
		{
			foreach (ReconOriginalEntryHeader entry in this)
			{
				if (entry.Wraps(entryPassed))
				{
					return true;
				}
			}
			return false;
		}

		public ReconOriginalEntryHeader FindEntryBy(ZString entryFilerAndEntryNumber)
		{
			foreach (ReconOriginalEntryHeader entry in this)
			{
				if (entry.CH_OrigEntryReference == entryFilerAndEntryNumber)
				{
					return entry;
				}
			}
			return null;
		}

		public bool HasImportableEntriesWithoutAnyLinesAttached
		{
			get
			{
				foreach (ReconOriginalEntryHeader entry in this)
				{
					if (!entry.US_R_NoLineDetails
						&& entry.Invoice.JobComInvoiceLines.Count == 0
						&& entry.CH_OrigEntryReference.Length > 3
						&& entry.CH_OrigEntryReference.StartsWith(reconDeclaration.US_EntryFilerCode))
					{
						return true;
					}
				}
				return false;
			}
		}

		public ZDecimal GetOriginalChargeAmount(params string[] feeCode)
		{
			ZDecimal result = 0m;
			foreach (ReconOriginalEntryHeader entry in this)
			{
				result += entry.OriginalCharges.GetTotal(feeCode);
			}
			return result;
		}

		public ZDecimal GetReconChargeAmount(params string[] feeCode)
		{
			ZDecimal result = 0m;
			foreach (ReconOriginalEntryHeader entry in this)
			{
				result += entry.ReconCharges.GetTotalAmount(feeCode);
			}
			return result;
		}

		public bool HasBeenShortPaid
		{
			get
			{
				foreach (ReconOriginalEntryHeader entry in this)
				{
					if (entry.HasBeenShortPaid)
					{
						return true;
					}
				}
				return false;
			}
		}

		void PopulateCollection()
		{
			foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
			{
				if (entry.CH_MessageType == CusEntryHeaderMessageTypeList.Codes.ReconOriginalEntry)
				{
					Add(new ReconOriginalEntryHeader(entry, reconDeclaration));
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var entry = Factory.New<CusEntryHeader>();

			return new ReconOriginalEntryHeader(entry, reconDeclaration);
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);

			ReconOriginalEntryHeader newChild = (ReconOriginalEntryHeader)child;
			newChild.CH_JE = reconDeclaration.JE_PK;
		}

		protected override void OnNonCommittedAdded(BusinessObject bizOAdded)
		{
			var invoice = ((ReconOriginalEntryHeader)bizOAdded).Invoice; // Ensure Invoice is created
			base.OnNonCommittedAdded(bizOAdded);
		}
	}
}
