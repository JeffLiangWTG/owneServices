using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Environment.Business.US
{
	public class WhsRowLookups : Business.WhsRowLookups
	{
		public WhsRowLookups(AutoWhsRow parent)
			: base(parent) { }

		#region Overrides

		public override CodeDescriptionPairList ApprovedKnownStatuses
		{
			get { return new CodeLists.US.TSAStatus(); }
		}

		#endregion
	}
}

