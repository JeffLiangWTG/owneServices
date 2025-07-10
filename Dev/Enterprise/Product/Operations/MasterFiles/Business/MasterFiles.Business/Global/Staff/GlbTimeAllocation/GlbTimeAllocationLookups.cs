using CargoWise.Common;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public class GlbTimeAllocationLookups : GlbStaffHolidayLookups
	{
		protected GlbTimeAllocationLookups(AutoGlbStaffHoliday parent)
			: base(parent)
		{
		}

		public static GlbTimeAllocationLookups New(AutoGlbStaffHoliday parent)
		{
			GlbTimeAllocationLookups result;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(parent);
			}
			else
			{
				result = new GlbTimeAllocationLookups(parent);
			}
			return result;
		}

		protected delegate GlbTimeAllocationLookups NewDelegate(AutoGlbStaffHoliday parent);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#region Types

		protected override CodeDescriptionBoolCollection GetTypes()
		{
			CodeDescriptionBoolCollection result = new CodeDescriptionBoolCollection();
			result.Add("OTH", ResString.GetMultilingualString("d175188e-83a5-4614-9a8b-85aaecd5539a", "Other - See Comments"), false);

			return result;
		}

		#endregion
	}
}
