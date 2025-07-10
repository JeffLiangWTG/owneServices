using System.Diagnostics.CodeAnalysis;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class ReleaseDetail : AutoReleaseDetail
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public ReleaseDetail(AgencyShipmentContainer container)
			: base(container.Factory)
		{
			this.container = container;

			if (!container.JC_ReleaseNum.IsEmpty)
			{
				ReleaseCount = container.JC_ContainerCount;
				PreviouslyReleased = true;
			}
			else
			{
				PreviouslyReleased = false;
			}
		}

		#region Related Business Objects

		public AgencyShipmentContainer Container
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return container; }
		}

		public OrgHeader ContainerYard
		{
			get { return Factory.Load<OrgHeader>(ContainerYardOrg); }
		}

		#endregion

		#region Properties

		public override ZString ContainerType
		{
			get { return Container.Container == null ? ZString.Empty : container.Container.RC_Code; }
		}

		public override ZShort ContainerCount
		{
			get { return Container.JC_ContainerCount; }
		}

		public override ZString ContainerQuality
		{
			get { return Container.JC_ContainerQuality; }
		}

		public override ZString ContainerStatus
		{
			get { return Container.JC_ContainerStatus; }
		}

		[List("Lookups.ContainerYardOrgs")]
		public override ZGuid ContainerYardOrg
		{
			get { return Container.JC_OA_DepartureContainerYardAddress_ZAddress.OrgPK; }
		}

		[List("Lookups.ContainerYardAddresses")]
		public override ZGuid ContainerYardAddress
		{
			get { return Container.JC_OA_DepartureContainerYardAddress; }
		}

		#endregion

		#region Strategy Objects

		public ReleaseDetailLookups Lookups
		{
			get { return lookups ?? (lookups = new ReleaseDetailLookups(this)); }
		}

		#endregion

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		ReleaseDetailLookups lookups;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly AgencyShipmentContainer container;
	}
}



