using System;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class FTZ214EntryLineCollection : NonPersistentBusinessObjectCollection<FTZ214EntryLine>, IObsoleteValidation
	{
		public FTZ214EntryLineCollection(JobDeclaration declaration)
			: base(declaration.Factory)
		{
			this.declaration = declaration;
		}

		readonly JobDeclaration declaration;

		public void PopulateElements()
		{
			RemoveAll();
			foreach (var bill in declaration.Bills.Cast<Bill>().Where(bill => bill.IsMasterBill))
			{
				int lineCount = 0;
				foreach (var line in declaration.ActiveEntryHeaders.FindEntryLinesByMasterBill(bill.PK))
				{
					lineCount++;
					Add(new FTZ214EntryLine(Factory, line, bill, lineCount == 1));
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
