using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgAirlineMAWBStockManagementCollection : ActiveBusinessObjectCollection<OrgAirlineMAWBStockManagement>
	{
		public OrgAirlineMAWBStockManagementCollection(BusinessObjectFactory factory, OrgHeader carrier)
			: base(factory, new ZQuery(OrgAirlineMAWBStockManagementSchema.OHM_OH_Carrier, carrier.PK))
		{
			parentCarrier = carrier;
		}

		public OrgAirlineMAWBStockManagementCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		readonly OrgHeader parentCarrier;

		public OrgAirlineMAWBStockManagement GetMAWBStockManagementForCurrentBranch()
			=> this.FirstOrDefault(mawb => mawb.OHM_GB_Branch == GlbBranch.CurrentBranch.PK)
				?? this.FirstOrDefault(mawb => mawb.OHM_GB_Branch == ZGuid.Empty);

		protected override void SetDefaultsForNewElementCore(OrgAirlineMAWBStockManagement newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.OHM_OH_Carrier = parentCarrier.PK;
		}
	}
}
