using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff
{
	public class ContainerConsignmentItemWrapper : ICREConsignmentItem, IICRConsignmentItem
	{
		public ContainerConsignmentItemWrapper(CusContainer container, ZBool notFirstContainer)
		{
			this.container = Argument.NotNull(container, "Container cannot be null");
			this.notFirstContainer = notFirstContainer;
		}

		readonly CusContainer container;
		readonly bool notFirstContainer;

		#region ICREConsignmentItem Implementation

		ZShort ICREConsignmentItem.SequenceNumber => 0;

		ZBool ICREConsignmentItem.IsEmptyContainer => false;

		ZString ICREConsignmentItem.GoodsDescription => GoodsDescription;

		IEnumerable<ICommodity> ICREConsignmentItem.Identifiers => Enumerable.Empty<ICommodity>();

		ZDecimal ICREConsignmentItem.Value => Value;

		ZString ICREConsignmentItem.Currency => Currency;

		IEnumerable<IClassification> ICREConsignmentItem.Classifications => Enumerable.Empty<IClassification>();

		ZDecimal ICREConsignmentItem.GrossWeightInKg => GrossWeightInKg;

		ZString ICREConsignmentItem.GoodsOriginCountry => GoodsOriginCountry;

		ZInt ICREConsignmentItem.PackageQty => TotalNumberOfPackages;

		ZString ICREConsignmentItem.PackageType => APackageType;

		ZString ICREConsignmentItem.ContainerNumber => container.CO_ContainerNumber;

		ZString ICREConsignmentItem.UNDGHazardousGoodsCode => ZString.Empty;

		#endregion

		#region IICRConsignmentItem Implementation

		ZShort IICRConsignmentItem.SequenceNumber => 0;

		ZBool IICRConsignmentItem.IsEmptyContainer => false;

		ZString IICRConsignmentItem.GoodsDescription => GoodsDescription;

		ZString IICRConsignmentItem.IdentityNumber => ZString.Empty;

		ZDecimal IICRConsignmentItem.Value => Value;

		ZString IICRConsignmentItem.Currency => Currency;

		ZString IICRConsignmentItem.IdentityType => ZString.Empty;

		IEnumerable<IClassification> IICRConsignmentItem.Classifications => Enumerable.Empty<IClassification>();

		ZBool IICRConsignmentItem.SendFlashpointTemp => false;

		ZDecimal IICRConsignmentItem.FlashpointTempInCelsius => ZDecimal.Zero;

		ITemperatureRequirements IICRConsignmentItem.Temperatures => null;

		ZDecimal IICRConsignmentItem.GrossWeightInKg => GrossWeightInKg;

		ZString IICRConsignmentItem.GoodsOriginCountry => GoodsOriginCountry;

		ZInt IICRConsignmentItem.PackageQty => TotalNumberOfPackages;

		ZString IICRConsignmentItem.PackageType => APackageType;

		ZString IICRConsignmentItem.ContainerNumber => container.CO_ContainerNumber;

		ZString IICRConsignmentItem.MPIApprovedSystemNumber => NZCustomsDataRegistry.Instance.EnableSeaICRFields.Value ? container.CO_MPIApprovedSystemNumber : ZString.Empty;

		#endregion

		#region Implementation

		ZInt TotalNumberOfPackages
		{
			get
			{
				ZInt result = 0;
				foreach (PackingGroup packGroup in container.PackingGroups)
				{
					result += packGroup.Packages.Cast<Package>().Sum(x => x.CW_PackQty);
				}
				return result;
			}
		}

		ZString APackageType
		{
			get
			{
				var result = ZString.Empty;
				foreach (PackingGroup packGroup in container.PackingGroups)
				{
					var packageWithType = packGroup.Packages.Cast<Package>().FirstOrDefault(x => !x.CW_PackType.IsEmpty);
					if (packageWithType != null)
					{
						result = packageWithType.CW_PackType;
						break;
					}
				}

				return result;
			}
		}

		ZString GoodsDescription => container.Declaration?.JE_GoodsDescription ?? ZString.Empty;

		ZDecimal Value => notFirstContainer ? ZDecimal.Zero : container.Declaration?.JE_ECI_InvoiceAmount ?? ZDecimal.Zero;

		ZString Currency => notFirstContainer ? ZString.Empty : container.Declaration?.ECI_InvoiceCurrency?.RX_Code ?? ZString.Empty;

		ZDecimal GrossWeightInKg => new ZWeight(container.CO_Weight, container.CO_WeightUQ).InKilogramsSafe;

		ZString GoodsOriginCountry => container.Declaration?.JE_RL_NKOrigin.Left(2) ?? ZString.Empty;

		#endregion
	}
}
