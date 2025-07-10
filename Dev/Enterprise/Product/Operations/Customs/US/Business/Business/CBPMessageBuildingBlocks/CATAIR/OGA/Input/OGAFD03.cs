using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class OGAFD03 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.OGAFD03, IBIRDOGALineRecord
	{
		#region IBIRDOGALineRecord Members

		void IBIRDOGALineRecord.Update(IOGALine ogaLine, INotifications notifications)
		{
			FDA fdaLine = (FDA)ogaLine;

			fdaLine.US_InvCurrFDAValue = FDAValueByFDALine;

			fdaLine.US_OA_FDAFEI = BIRDOrganisationMatching.GetOrganisationAddress(fdaLine.Factory, OrgMatchedCustomsRegNoType.FEI, FDAConsigneeFDAEstablishmentIndicatorFEI, "FDA Consignee", notifications)?.PK ?? ZGuid.Empty;

			fdaLine.US_TradeBrandName = TradeOrBrandName;

			fdaLine.US_ContainerDim1 = GetContainerDimension(ContainerDimension1);
			fdaLine.US_ContainerDim2 = GetContainerDimension(ContainerDimensions2);
			fdaLine.US_ContainerDim3 = GetContainerDimension(ContainerDimensions3);
		}

		ZDecimal GetContainerDimension(ZString dimension)
		{
			ZDecimal result = ZDecimal.ParseSafe(dimension.Left(2), 0m);

			ZDecimal decimals = ZDecimal.ParseSafe(dimension.Right(2), 0m) / 0.16m;
			decimals = decimals / 100m;

			return result + decimals.Round(2);
		}

		#endregion
	}
}
