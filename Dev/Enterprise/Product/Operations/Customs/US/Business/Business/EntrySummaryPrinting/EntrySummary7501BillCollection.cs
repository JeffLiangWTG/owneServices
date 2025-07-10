using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business.EntrySummaryPrinting
{
	public class EntrySummary7501BillCollection : NonPersistentBusinessObjectCollection<EntrySummary7501Bill>, IObsoleteValidation
	{
		public EntrySummary7501BillCollection(BusinessObjectFactory factory)
			: base(factory)
		{
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
