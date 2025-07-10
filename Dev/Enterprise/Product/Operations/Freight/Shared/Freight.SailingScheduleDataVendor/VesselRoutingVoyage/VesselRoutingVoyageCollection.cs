using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.SailingDataVendor.Business
{
	public class VesselRoutingVoyageCollection : BusinessObjectCollection<VesselRoutingVoyage>
	{
		public VesselRoutingVoyageCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public PortPairTypes PortPairTypeFilter;

		public VesselRoutingVoyage[] GetSelectedVoyages()
		{
			List<VesselRoutingVoyage> result = new List<VesselRoutingVoyage>();
			foreach (VesselRoutingVoyage voyage in this)
			{
				if (voyage.E8_IsSelected)
				{
					result.Add(voyage);
				}
			}

			return result.ToArray();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			VesselRoutingVoyage voyage = (VesselRoutingVoyage)bizOAdded;
			voyage.ParentCollection = this;
		}

		internal readonly IDictionary userEnteredData = new Hashtable();
	}
}
