using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.DataTransfer.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.DataTransfer
{
	public static class RTUColumnIndexerHelper
	{
		public static List<ContainerByASNDTO> GetExistingRTUs_ByAsnPK(UniversalObjectFactory factory, IEnumerable<ZGuid> asnPKs)
		{
			var containerByASNDTOs = new List<ContainerByASNDTO>();

			var pivotQuery = new ZQuery(WhsItemReceiveASNRTUPivotSchema.WAR_WRP_TransitReceiveASN, asnPKs);
			var asnRtuPivots = factory.BOFactory.Load<WhsItemReceiveASNRTUPivot>(pivotQuery);

			if (asnRtuPivots.Length > 0)
			{
				var asnRtuPivots_GroupByAsnPK = asnRtuPivots.GroupBy(pivot => pivot.WAR_WRP_TransitReceiveASN);
				containerByASNDTOs = GetContainerFromAsnRtuPivots(factory, asnRtuPivots_GroupByAsnPK);
			}
			var asnPKsWithoutPivot = asnPKs.Except(asnRtuPivots.Select(pivot => pivot.WAR_WRP_TransitReceiveASN));
			foreach (var asnPK in asnPKsWithoutPivot)
			{
				containerByASNDTOs.Add(new ContainerByASNDTO(asnPK, new List<ContainerDTO>()));
			}

			return containerByASNDTOs;
		}

		public static List<ContainerByASNDTO> GetContainerFromAsnRtuPivots(UniversalObjectFactory factory, IEnumerable<IGrouping<ZGuid, WhsItemReceiveASNRTUPivot>> asnRtuPivots_GroupByAsnPK)
		{
			var containerByASNDTOs = new List<ContainerByASNDTO>();

			asnRtuPivots_GroupByAsnPK.ForEach(gp =>
			{
				var rtuPKs = gp.Select(g => g.WAR_WRH_TransitReceiveTransportationUnit);
				var rtus = factory.BOFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery(WhsItemReceiveTransportationUnitSchema.PK, rtuPKs));

				var rtusAsPackageState_ByRtuPK = PackageStateColumnIndexerHelper.GetTransportationUnitAsPackageStates_ByTransportationUnitPK(factory, WhsItemReceiveTransportationUnitSchema.Constants.Prefix, rtuPKs);
				var containerDTOs = new List<ContainerDTO>();
				foreach (var rtuPK in rtuPKs)
				{
					var rtu = rtus.FirstOrDefault(dt => dt.PK == rtuPK);
					if (rtusAsPackageState_ByRtuPK.ContainsKey(rtuPK))
					{
						var rtuAsPkgState = rtusAsPackageState_ByRtuPK[rtuPK];
						var containerDTO = new ContainerDTO(rtu, rtuAsPkgState);
						containerDTOs.Add(containerDTO);
					}
					else
					{
						var containerDTO = new ContainerDTO(rtu, null);
						containerDTOs.Add(containerDTO);
					}
				}

				containerByASNDTOs.Add(new ContainerByASNDTO(gp.Key, containerDTOs));
			});

			return containerByASNDTOs;
		}
	}
}
