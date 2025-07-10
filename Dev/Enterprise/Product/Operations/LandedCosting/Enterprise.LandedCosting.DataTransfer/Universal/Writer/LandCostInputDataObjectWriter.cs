using Enterprise.LandedCosting.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalData = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.LandedCosting.DataTransfer.Universal
{
	public class LandCostInputDataObjectWriter : DataObjectWriter<LandCostInput, UniversalData.TransportLogisticsCost>
	{
		public LandCostInputDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override UniversalData.TransportLogisticsCost PopulateDataObject(LandCostInput landCostInputBO)
		{
			var transportLogisticsCostData = new UniversalData.TransportLogisticsCost();
			var chargeCode = landCostInputBO.ChargeCode;
			if (chargeCode != null)
			{
				transportLogisticsCostData.ChargeCode = new UniversalDataBuss.DataObjects.Accounting.ChargeCode() { Code = chargeCode.AC_Code, Description = chargeCode.AC_DescMultilingual.GetUnresolvedString() };
			}
			transportLogisticsCostData.ChargeDescription = landCostInputBO.LI_ChargeDescription;
			transportLogisticsCostData.CostAmount = landCostInputBO.LI_CostAmount;
			transportLogisticsCostData.CostCurrency = UniversalData.Currency.New(landCostInputBO.CostCurrency);
			transportLogisticsCostData.DistributeCostBy = ListHelper.GetWithDescription<UniversalData.CodeDescriptionPair>(landCostInputBO.LI_DistributeCostBy, landCostInputBO.Lookups.DistributeCostBy);
			transportLogisticsCostData.LandedCostGroup = ListHelper.GetWithDescription<UniversalData.CodeDescriptionPair>(landCostInputBO.LCGroupString, landCostInputBO.Lookups.LandCostGroupList);
			transportLogisticsCostData.ServiceExRate = landCostInputBO.LI_ServiceExRate;
			return transportLogisticsCostData;
		}
	}
}
