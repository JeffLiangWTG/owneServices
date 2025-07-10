//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbDeviceLookups
//
//    This class should be used for overriding collections in AutoGlbDeviceLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Telematics.Business
{
	public class GlbDeviceLookups : AutoGlbDeviceLookups
	{
		public GlbDeviceLookups(AutoGlbDevice parent)
			: base(parent)
		{
		}

		public ReadOnlyCodeDescriptionPairList GlbDeviceStatusList => GetGlbDeviceStatusList();

		ReadOnlyCodeDescriptionPairList GetGlbDeviceStatusList()
		{
			return new GlbDeviceStatusList();
		}

		#region Parent Lookups

		public GlbStaffCollection ParentStaffLookups
		{
			get { return new GlbStaffCollection(Factory); }
		}

		public RefEquipmentCollection ParentEquipmentLookups
		{
			get { return new RefEquipmentCollection(Factory); }
		}

		#endregion
	}
}
