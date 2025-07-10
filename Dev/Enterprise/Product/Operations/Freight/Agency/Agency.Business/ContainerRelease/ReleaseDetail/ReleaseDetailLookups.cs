using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class ReleaseDetailLookups : ZLookups
	{
		public ReleaseDetailLookups(ReleaseDetail detail)
			: base(detail) { }

		#region ContainerYardOrgs

		public OrganisationsFindBoxCollection ContainerYardOrgs
		{
			get { return new ContainerYardCollection(Factory); }
		}

		#endregion

		#region ContainerYardAddresses

		public ZAddressList ContainerYardAddresses
		{
			get { return Parent.ContainerYard == null ? new ZAddressList() : Parent.ContainerYard.Address_List; }
		}

		#endregion

		#region Implementation

		public new ReleaseDetail Parent
		{
			get { return (ReleaseDetail)base.Parent; }
		}

		#endregion
	}
}
