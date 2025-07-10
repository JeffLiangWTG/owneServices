using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService
{
	public partial class RefStlScriptService
	{
		public IEnumerable<Models.RefStlScript> GetDataUpToV40(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpointPK, int? chunkSize, short dataSetId)
		{
			var mapper = Config.CreateMapper();

			var dataSets = from refStlScript in RefDbRepo.Get<RefStlScript>()
					   join refStlScript2 in RefDbRepo.Get<RefStlScript>() on new { refStlScript.STL_FeatureCode, refStlScript.STL_ActiveOn } equals new { refStlScript2.STL_FeatureCode, refStlScript2.STL_ActiveOn }
					   join version in GetVersionControls(dataSetId).Filter(lowerTimestamp, upperTimestamp, checkpointPK) on refStlScript2.STL_PK equals version.RVC_ParentPK
					   orderby refStlScript.STL_PK ascending, version.RVC_Deleted ascending, version.RVC_LastUpdatedUTC descending
					   select new { refStlScript, version.RVC_Deleted, refStlScript2PK = refStlScript2.STL_PK };

			// GroupBy keeps the order of the elements in source that produced the first key.
			// https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.groupby?view=net-8.0
			var dataSetGroups = dataSets.GroupBy(x => x.refStlScript.STL_PK);

			int idx = 0;
			foreach (var dataSetG in dataSetGroups)
			{
				var dataSet = dataSetG.FirstOrDefault();
				if (dataSet.refStlScript.STL_PK == dataSet.refStlScript2PK)
				{
					var result = mapper.Map<Models.RefStlScript>(dataSet.refStlScript);
					result.Deleted = dataSet.RVC_Deleted;
					result.WriteCheckpoint(dataSet.refStlScript.STL_PK, ref idx, chunkSize);
					yield return result;
				}
			}
		}
	}
}
