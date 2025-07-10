using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ContainerInfo
	{
		public ContainerInfo(ZString containerTypeCode, ZInt containerCount, ZString containerNumber)
		{
			this.containerTypeCode = containerTypeCode;
			this.containerCount = containerCount;
			this.containerNumber = containerNumber;
		}

		readonly ZString containerTypeCode;
		readonly ZInt containerCount;
		readonly ZString containerNumber;

		public RefContainer RefContainer
		{
			get
			{
				if (refContainer == null)
				{
					refContainer = new BusinessObjectFactory().LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, containerTypeCode));
				}
				return refContainer;
			}
		}
		RefContainer refContainer;

		public bool MatchByFreightRateClass(ContainerInfo containerInfo)
		{
			return containerInfo != null
				&& containerInfo.containerCount == containerCount
				&& containerInfo.containerNumber == containerNumber
				&& (containerInfo.RefContainer != null && containerInfo.RefContainer.MatchesClass(RefContainer));
		}

		public override bool Equals(object obj)
		{
			var containerInfo2 = obj as ContainerInfo;

			return containerInfo2 != null
				&& containerInfo2.containerCount == containerCount
				&& containerInfo2.containerNumber == containerNumber
				&& containerInfo2.containerTypeCode == containerTypeCode;
		}

		public override int GetHashCode()
		{
			return containerTypeCode.GetHashCode() ^ containerCount.GetHashCode() ^ containerNumber.GetHashCode();
		}
	}
}
