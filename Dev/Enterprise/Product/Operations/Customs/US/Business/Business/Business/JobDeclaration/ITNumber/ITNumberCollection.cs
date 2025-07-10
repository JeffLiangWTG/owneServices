using System;

using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class ITNumberCollection : NonPersistentBusinessObjectCollection<ITNumber>
	{
		public ITNumberCollection(JobDeclaration declaration)
		{
			this.declaration = declaration;
			Populate();
		}
		readonly JobDeclaration declaration;

		internal void Populate()
		{
			RemoveAll();

			foreach (Bill bill in declaration.Bills)
			{
				foreach (ITAndSplitDetails itNo in bill.ITAndSplitDetails)
				{
					if (!itNo.US_ITNumber.IsEmpty)
					{
						Add(new ITNumber(itNo));
					}
				}
			}
		}

		#region Override

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new InvalidOperationException();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion
	}
}
