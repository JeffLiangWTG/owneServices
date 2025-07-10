using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[ExcludeFromOverriddenAddNewTest]
	public class InvoiceLinesForEntryLineCollection : BusinessObjectCollection<BaseJobComInvoiceLine>
	{
		public InvoiceLinesForEntryLineCollection(CusEntryLine entryLine)
			: base(entryLine.Factory)
		{
			this.EntryLine = entryLine;
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public override void Load()
		{
			IsLoaded = true;
			try
			{
				RemoveAllButLeaveRelationshipsIntact();

				using (SuspendListChanged())
				{
					BaseJobDeclaration declaration = EntryLine.Declaration;

					bool canBeLinkedByPivot = EntryLine.CanBeLinkedUpByPivot;

					if (declaration != null)
					{
						AddRange(declaration.InvoiceLines.ByEntryLine[EntryLine.PK]);
					}
					else
					{
						ZQuery query = new ZQuery(JobComInvoiceLineSchema.JI_CL, EntryLine.PK);

						if (canBeLinkedByPivot)
						{
							List<ZGuid> invoiceLinePKs = new List<ZGuid>();

							foreach (AdditionalInvoiceLineEntryLineLink pivot in EntryLine.AdditionalInvoiceLineLinks)
							{
								invoiceLinePKs.Add(pivot.BU_JI);
							}

							if (invoiceLinePKs.Count > 0)
							{
								query.AddToFilter(JoinCondition.Or, JobComInvoiceLineSchema.PK, invoiceLinePKs);
							}
						}

						this.AddRange(Factory.Load<BaseJobComInvoiceLine>(query));
					}
				}
			}
			finally
			{
				IsLoaded = false;
			}
			OnLoaded();
		}

		#region Implementation

		protected readonly CusEntryLine EntryLine;

		#endregion
	}
}
