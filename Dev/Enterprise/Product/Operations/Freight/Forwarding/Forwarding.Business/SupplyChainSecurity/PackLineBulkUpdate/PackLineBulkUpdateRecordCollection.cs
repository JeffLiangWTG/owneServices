using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business
{
	public class PackLineBulkUpdateRecordCollection : NonPersistentBusinessObjectCollection<PackLineBulkUpdateRecord>
	{
		#region Constructor

		public PackLineBulkUpdateRecordCollection()
			: base()
		{
		}

		public PackLineBulkUpdateRecordCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#endregion

		#region Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override ZString HumanReadableNameCore => Res.GetString("cca32baf-ed07-482a-ac67-a49bcd7296fd", "collection of packline updates");

		#endregion
	}
}
