using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	class SupplierConsortiumComparer : ConsortiumComparer
	{
		public override int Compare(FastLine line1, FastLine line2)
		{
			return Compare(line1.ParentRateEntry.Supplier, line2.ParentRateEntry.Supplier);
		}

		protected override string GetName()
		{
			return (NoResString)"Supplier Consortium"; // log message, subject to change, more for support people as of now
		}
	}
}
