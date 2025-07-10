using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.LandedCosting.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalData = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.LandedCosting.DataTransfer.Universal
{
	public class LandedCostDataWriter : UniversalData.ILandedCostDataWriter
	{
		public LandedCostDataWriter(IDataWritingManager manager, Integration.LandedCosting.ILandedCostHeader landedCostHeader)
		{
			this.manager = Argument.NotNull(manager, "manager");
			this.landedCostHeader = Argument.NotNull(landedCostHeader as LandedCostHeader, "landedCostHeader should be LandedCostHeader");
		}
		readonly IDataWritingManager manager;
		readonly LandedCostHeader landedCostHeader;

		List<UniversalData.TransportLogisticsCost> UniversalData.ILandedCostDataWriter.PopulateTransportLogisticsCostCollection(BusinessObject bizObj)
		{
			var result = new List<UniversalData.TransportLogisticsCost>();
			var parentID = bizObj.PK;
			var parentTableCode = bizObj.TablePrefix;
			foreach (var costInput in landedCostHeader.CostInputs.OfType<LandCostInput>().Where(x => x.LI_ParentID == parentID && x.LI_ParentTableCode == parentTableCode))
			{
				result.Add(LandCostInputDataObjectWriter.GetDataObject(costInput));
			}
			return result.Count == 0 ? null : result;
		}

		UniversalData.LandedCostDetail UniversalData.ILandedCostDataWriter.PopulateLandedCostDetail(BusinessObject bizObj)
		{
			UniversalData.LandedCostDetail result = null;
			var parentID = bizObj.PK;
			var parentTableCode = bizObj.TablePrefix;
			var history = landedCostHeader.Histories.OfType<LandedCostHistory>().FirstOrDefault(x => x.LH_ParentID == parentID && x.LH_ParentTableCode == parentTableCode);
			if (history != null)
			{
				result = LandedCostHistoryDataObjectWriter.GetDataObject(history);
			}
			return result;
		}

		LandCostInputDataObjectWriter LandCostInputDataObjectWriter
		{
			get { return landCostInputDataObjectWriter ?? (landCostInputDataObjectWriter = new LandCostInputDataObjectWriter(manager)); }
		}
		LandCostInputDataObjectWriter landCostInputDataObjectWriter;

		LandedCostHistoryDataObjectWriter LandedCostHistoryDataObjectWriter
		{
			get { return landedCostHistoryDataObjectWriter ?? (landedCostHistoryDataObjectWriter = new LandedCostHistoryDataObjectWriter(manager)); }
		}
		LandedCostHistoryDataObjectWriter landedCostHistoryDataObjectWriter;
	}
}
