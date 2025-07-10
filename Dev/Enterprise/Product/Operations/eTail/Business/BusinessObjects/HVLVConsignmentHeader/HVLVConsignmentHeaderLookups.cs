//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoHVLVConsignmentHeaderLookups
//
//    This class should be used for overriding collections in AutoHVLVConsignmentHeaderLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eTail.Business
{
	public class HVLVConsignmentHeaderLookups : AutoHVLVConsignmentHeaderLookups
	{
		public HVLVConsignmentHeaderLookups(AutoHVLVConsignmentHeader parent) : base(parent)
		{
		}

		public ActiveBusinessObjectCollection<HVLVConsignment> DetachedConsignment_List
		{
			get
			{
				if (detachedConsignments == null)
				{
					detachedConsignments = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory);
					detachedConsignments.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Shipments", "Property", ((HVLVConsignmentHeader)Parent).HCH_JS_Shipment, true));
					detachedConsignments.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Active Status", "Property", (ZString)(NoResString)"Inactive", false));
					detachedConsignments.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Consignment Status", "Property", (ZString)(NoResString)HVLVConsignmentStatus.Codes.Detached, false));
				}

				return detachedConsignments;
			}
		}

		ActiveBusinessObjectCollection<HVLVConsignment> detachedConsignments;
	}
}
