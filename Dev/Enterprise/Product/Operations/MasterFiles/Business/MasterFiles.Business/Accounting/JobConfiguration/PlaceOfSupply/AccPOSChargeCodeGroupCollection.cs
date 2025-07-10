using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccPOSChargeCodeGroupCollection : DependentBusinessObjectCollection<AccPOSChargeCodeGroup, GlbCompany>
	{
		public AccPOSChargeCodeGroupCollection(GlbCompany master) : base(master)
		{ }
	}
}
