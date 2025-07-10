using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transit.Business;

namespace Enterprise.Warehouse.Transit
{
	public class ContainerViewForReceive : IContainerView
	{
		public ContainerViewForReceive(PkgPackageContainer container)
		{
			receiveTransportationUnit = container.Package.PackageJob.ParentJob as WhsItemReceiveTransportationUnit;
		}
		readonly WhsItemReceiveTransportationUnit receiveTransportationUnit;

		public ZDateTime? PackCompleteDate => null;

		public ZDateTime? UnpackCompleteDate
		{
			get
			{
				if (receiveTransportationUnit != null && receiveTransportationUnit.WRH_UnloadCompleteTime.IsValid)
				{
					return receiveTransportationUnit?.WRH_UnloadCompleteTime.ToZDateTime();
				}

				return null;
			}
		}
	}
}
