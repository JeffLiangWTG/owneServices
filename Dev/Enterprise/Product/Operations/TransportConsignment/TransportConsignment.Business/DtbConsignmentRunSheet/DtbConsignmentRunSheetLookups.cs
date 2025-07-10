//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDtbConsignmentRunSheetLookups
//
//    This class should be used for overriding collections in AutoDtbConsignmentRunSheetLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentRunSheetLookups : AutoDtbConsignmentRunSheetLookups
	{
		public DtbConsignmentRunSheetLookups(AutoDtbConsignmentRunSheet parent) : base(parent)
		{
		}

		public override OrgHeaderCollection TransportCos
		{
			get { return new LocalTransportCollection(Factory); }
		}

		public override GlbStaffCollection TruckDrivers
		{
			get { return new StaffDriverCollection(Factory); }
		}

		#region CarrierServiceLevel_List

		public override OrgCarrierServiceLevelCollection CarrierServiceLevels
		{
			get { return Factory.GetCachedValue("ConsignmentRunSheet|BindToLists", () => new CachedProperty<OrgCarrierServiceLevelCollection>(Factory, GetCarrierServiceLevels)).Value; }
		}

		OrgCarrierServiceLevelCollection GetCarrierServiceLevels()
		{
			var carrier = ((DtbConsignmentRunSheet)Parent).TransportCo;
			var result = carrier != null ? new OrgCarrierServiceLevelCollection(carrier.MiscServ) : new OrgCarrierServiceLevelCollection(Factory);
			result.Load();

			return result;
		}

		#endregion
	}
}
