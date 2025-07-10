using CargoWise.Common;
using CargoWise.Types;
using Enterprise.LandedCosting.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalData = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.LandedCosting.DataTransfer.Universal
{
	public class LandCostInputDataObjectReader : DataObjectReader<UniversalData.TransportLogisticsCost, LandCostInput>
	{
		public LandCostInputDataObjectReader(UniversalData.TransportLogisticsCost transportLogisticsCostDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, LandCostInput costInput)
			: base(transportLogisticsCostDataObject, logger, factory)
		{
			this.costInput = Argument.NotNull(costInput, "costInput");
		}
		readonly LandCostInput costInput;

		protected override LandCostInput GetExistingBusinessObject()
		{
			return costInput;
		}

		protected override void PopulateBusinessObject(LandCostInput targetBO)
		{
			var landCostInputRow = GetColumnIndexer(targetBO);
			SetValue(landCostInputRow, LandCostInputSchema.LI_ChargeDescription, dataObject.ChargeDescription);
			SetValue(landCostInputRow, LandCostInputSchema.LI_CostAmount, dataObject.CostAmount);
			SetValue(landCostInputRow, LandCostInputSchema.LI_RX_NKCostCurrency, dataObject.CostCurrency);
			SetValue(landCostInputRow, LandCostInputSchema.LI_DistributeCostBy, dataObject.DistributeCostBy);
			if (dataObject.LandedCostGroup != null)
			{
				SetValue(landCostInputRow, LandCostInputSchema.LI_LandedCostGroup, ZByte.ParseSafe(dataObject.LandedCostGroup.GetCodeAsUpperCase(), ZByte.Zero));
			}
			SetValue(landCostInputRow, LandCostInputSchema.LI_ServiceExRate, dataObject.ServiceExRate);
		}
	}
}
