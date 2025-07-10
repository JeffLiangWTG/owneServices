using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Environment.Business.US
{
	public class WhsLocationLookups : WhsLocationViewLookups
	{
		#region Constructors

		public WhsLocationLookups(AutoWhsLocationView parent)
			: base(parent)
		{
		}

		#endregion

		#region Overrides

		protected override CodeDescriptionPairList ApprovedKnownStatusesCore => Factory.GetCachedValue("WhsLocationLookups|ApprovedKnownStatuses", () => new CodeLists.US.TSAStatus());

		#endregion
	}
}
