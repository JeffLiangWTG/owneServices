using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.DataTransfer.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.DataTransfer
{
	public static class DTUColumnIndexerHelper
	{
		public static List<ContainerByDLLDTO> GetContainerByDLLDTOByLoadListPKs(UniversalObjectFactory factory, IEnumerable<ZGuid> dllPKs)
		{
			var containerByDLLDTO = new List<ContainerByDLLDTO>();

			var dllDtuPivotQuery = new ZQuery(WhsItemDispatchLoadListDTUPivotSchema.WLD_WDL_TransitDispatchLoadList, dllPKs);
			var dllDtuPivots = factory.BOFactory.Load<WhsItemDispatchLoadListDTUPivot>(dllDtuPivotQuery);

			if (dllDtuPivots.Length > 0)
			{
				var dllDtuPivots_ByLoadListPK = dllDtuPivots.GroupBy(pivot => pivot.WLD_WDL_TransitDispatchLoadList);
				containerByDLLDTO = ProcessContainerByLoadList(factory, dllDtuPivots_ByLoadListPK);
			}
			var dllPKsWithoutPivot = dllPKs.Except(dllDtuPivots.Select(pivot => pivot.GetValue(WhsItemDispatchLoadListDTUPivotSchema.WLD_WDL_TransitDispatchLoadList)));
			foreach (var dllPK in dllPKsWithoutPivot)
			{
				containerByDLLDTO.Add(new ContainerByDLLDTO(dllPK, new List<ContainerDTO>()));
			}
			return containerByDLLDTO;
		}

		static List<ContainerByDLLDTO> ProcessContainerByLoadList(UniversalObjectFactory factory, IEnumerable<IGrouping<ZGuid, WhsItemDispatchLoadListDTUPivot>> dllDtuPivots_ByLoadListPK)
		{
			var containerByDLLDTO = new List<ContainerByDLLDTO>();
			dllDtuPivots_ByLoadListPK.ForEach(gp =>
			{
				var dtuPKs = gp.Select(g => g.WLD_WDH_TransitDispatchTransportationUnit);
				var dtuQuery = new ZQuery(WhsItemDispatchTransportationUnitSchema.PK, dtuPKs);
				var dtus = factory.BOFactory.Load<WhsItemDispatchTransportationUnit>(dtuQuery);

				var dtuAsPackageStates_ByDtuPK = PackageStateColumnIndexerHelper.GetTransportationUnitAsPackageStates_ByTransportationUnitPK(factory, WhsItemDispatchTransportationUnitSchema.Constants.Prefix, dtuPKs);
				var containerDTOs = new List<ContainerDTO>();
				foreach (var dtuPK in dtuPKs)
				{
					var dtu = dtus.FirstOrDefault(dt => dt.GetValue(WhsItemDispatchTransportationUnitSchema.PK) == dtuPK);
					if (dtuAsPackageStates_ByDtuPK.ContainsKey(dtuPK))
					{
						var dtu_AsPkgState = dtuAsPackageStates_ByDtuPK[dtuPK];
						containerDTOs.Add(new ContainerDTO(dtu, dtu_AsPkgState));
					}
					else
					{
						containerDTOs.Add(new ContainerDTO(dtu, null));
					}
				}
				containerByDLLDTO.Add(new ContainerByDLLDTO(gp.Key, containerDTOs));
			});

			return containerByDLLDTO;
		}
	}
}
