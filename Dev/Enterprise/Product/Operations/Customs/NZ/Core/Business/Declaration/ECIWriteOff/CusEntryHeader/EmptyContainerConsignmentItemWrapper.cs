using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;

namespace Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff
{
	public class EmptyContainerConsignmentItemWrapper : ICREConsignmentItem, IICRConsignmentItem
	{
		public EmptyContainerConsignmentItemWrapper(CusContainer container)
		{
			this.container = Argument.NotNull(container, "Container cannot be null");
		}
		readonly CusContainer container;

		#region ICREConsignmentItem Implementation

		ZShort ICREConsignmentItem.SequenceNumber => 0;

		ZBool ICREConsignmentItem.IsEmptyContainer => true;

		ZString ICREConsignmentItem.GoodsDescription => "EMPTY CONTAINER";

		IEnumerable<ICommodity> ICREConsignmentItem.Identifiers => Enumerable.Empty<ICommodity>();

		ZDecimal ICREConsignmentItem.Value => ZDecimal.Zero;

		ZString ICREConsignmentItem.Currency => ZString.Empty;

		IEnumerable<IClassification> ICREConsignmentItem.Classifications => Enumerable.Empty<IClassification>();

		ZDecimal ICREConsignmentItem.GrossWeightInKg => ZDecimal.Zero;

		ZString ICREConsignmentItem.GoodsOriginCountry => ZString.Empty;

		ZInt ICREConsignmentItem.PackageQty => ZInt.Zero;

		ZString ICREConsignmentItem.PackageType => ZString.Empty;

		ZString ICREConsignmentItem.ContainerNumber => container.CO_ContainerNumber;

		ZString ICREConsignmentItem.UNDGHazardousGoodsCode => ZString.Empty;

		#endregion

		#region IICRConsignmentItem Implementation

		ZShort IICRConsignmentItem.SequenceNumber => 0;

		ZBool IICRConsignmentItem.IsEmptyContainer => true;

		ZString IICRConsignmentItem.GoodsDescription => "EMPTY CONTAINER";

		ZString IICRConsignmentItem.IdentityNumber => ZString.Empty;

		ZDecimal IICRConsignmentItem.Value => ZDecimal.Zero;

		ZString IICRConsignmentItem.Currency => ZString.Empty;

		ZString IICRConsignmentItem.IdentityType => ZString.Empty;

		IEnumerable<IClassification> IICRConsignmentItem.Classifications => Enumerable.Empty<IClassification>();

		ZBool IICRConsignmentItem.SendFlashpointTemp => false;

		ZDecimal IICRConsignmentItem.FlashpointTempInCelsius => ZDecimal.Zero;

		ITemperatureRequirements IICRConsignmentItem.Temperatures => null;

		ZDecimal IICRConsignmentItem.GrossWeightInKg => ZDecimal.Zero;

		ZString IICRConsignmentItem.GoodsOriginCountry => ZString.Empty;

		ZInt IICRConsignmentItem.PackageQty => ZInt.Zero;

		ZString IICRConsignmentItem.PackageType => ZString.Empty;

		ZString IICRConsignmentItem.ContainerNumber => container.CO_ContainerNumber;

		ZString IICRConsignmentItem.MPIApprovedSystemNumber => NZCustomsDataRegistry.Instance.EnableSeaICRFields.Value ? container.CO_MPIApprovedSystemNumber : ZString.Empty;

		#endregion
	}
}
