using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Warehouse.Transit.Business;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class ContainerByASNDTO
	{
		public ZGuid ASNPK { get; }
		public List<ContainerDTO> Containers;

		public ContainerByASNDTO(ZGuid asnPK, List<ContainerDTO> containerDTOs)
		{
			this.ASNPK = asnPK;
			this.Containers = containerDTOs;
		}
	}

	public class ContainerByDLLDTO
	{
		public ZGuid dllPK { get; }
		public List<ContainerDTO> containerDTOs;

		public ContainerByDLLDTO(ZGuid dllPK, List<ContainerDTO> containerDTOs)
		{
			this.dllPK = dllPK;
			this.containerDTOs = containerDTOs;
		}
	}

	public class ContainerDTO
	{
		public WhsItemReceiveTransportationUnit RTU;
		public WhsItemDispatchTransportationUnit DTU;
		public WhsItemPackageState PackageState;

		public ContainerDTO(WhsItemReceiveTransportationUnit rtu, WhsItemPackageState rtuAsPkgState)
		{
			this.RTU = rtu;
			this.PackageState = rtuAsPkgState;
		}

		public ContainerDTO(WhsItemDispatchTransportationUnit dtu, WhsItemPackageState rtuAsPkgState)
		{
			this.DTU = dtu;
			this.PackageState = rtuAsPkgState;
		}
	}
}