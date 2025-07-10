using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.ISF.Business
{
	public class ISFLineRowCollection : NonPersistentBusinessObjectCollection<ISFLineRow>
	{
		public ISFLineRowCollection(ISFBillRow billRow)
			: base(billRow.Factory)
		{
			this.BillRow = billRow;
			PopulateLines();
		}

		public readonly ISFBillRow BillRow;

		#region Implementation

		void PopulateLines()
		{
			ISFHeaderRow headerRow = BillRow.HeaderRow;
			foreach (CusISFLine line in headerRow.Header.Lines)
			{
				ISFLineRow row = new ISFLineRow(line, headerRow);
				row.RunPreSaveValidation();
				Add(row);
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new System.NotSupportedException();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		#endregion
	}
}
