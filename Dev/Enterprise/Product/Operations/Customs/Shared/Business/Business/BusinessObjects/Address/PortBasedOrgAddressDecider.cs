using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class PortBasedOrgAddressDecider : OrgAddressDecider
	{
		public delegate ZString GetRelatedPortDelegate();

		public PortBasedOrgAddressDecider(OrgHeader master, CargoAddressType type, GetRelatedPortDelegate getRelatedPort)
			: base(master, type)
		{
			if (getRelatedPort == null)
			{
				throw new ArgumentNullException(nameof(getRelatedPort));
			}

			this.getRelatedPort = getRelatedPort;
		}

		protected override OrgAddress GetAddressInThisOrder(params string[] addressTypesInOrder)
		{
			return fMaster.Addresses.BestAddressForPort(getRelatedPort(), addressTypesInOrder);
		}

		readonly GetRelatedPortDelegate getRelatedPort;
	}
}
