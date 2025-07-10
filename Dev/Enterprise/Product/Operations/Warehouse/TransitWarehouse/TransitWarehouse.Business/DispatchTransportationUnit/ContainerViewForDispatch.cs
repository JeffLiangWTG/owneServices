using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transit.Business;

namespace Enterprise.Warehouse.Transit
{
	public class ContainerViewForDispatch : IContainerView
	{
		public ContainerViewForDispatch(PkgPackageContainer container)
		{
			dispatchTransportationUnit = container.Package.PackageJob.ParentJob as WhsItemDispatchTransportationUnit;
		}
		readonly WhsItemDispatchTransportationUnit dispatchTransportationUnit;

		public ZDateTime? PackCompleteDate
		{
			get
			{
				if (dispatchTransportationUnit != null && dispatchTransportationUnit.WDH_LoadCompleteTime.IsValid)
				{
					return dispatchTransportationUnit?.WDH_LoadCompleteTime.ToZDateTime();
				}

				return null;
			}
		}

		public ZDateTime? UnpackCompleteDate => null;
	}
}
