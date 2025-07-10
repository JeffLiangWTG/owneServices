using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class PermitNumbersToSelectFromForPrintingCollection : NonPersistentBusinessObjectCollection<PermitNumberToSelectFromForPrinting>
	{
		protected override bool AllowNewCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}

		public IReadOnlyList<ZString> SelectedPermitNumbers => this.Where(x => x.NeedPrint).Select(x => x.PermitNumber).ToArray();
	}
}
