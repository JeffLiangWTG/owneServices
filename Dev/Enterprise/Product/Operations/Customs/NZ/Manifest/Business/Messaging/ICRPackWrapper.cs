using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.NZ.Manifest.Business
{
	public class ICRPackWrapper : IICRConsignmentItem
	{
		public ICRPackWrapper(AsycudaPack pack)
		{
			this.pack = Argument.NotNull(pack, "Packing Line cannot be null");
		}
		readonly AsycudaPack pack;

		ZShort IICRConsignmentItem.SequenceNumber => 1;

		ZBool IICRConsignmentItem.IsEmptyContainer => false;

		ZString IICRConsignmentItem.GoodsDescription => pack.APA_GoodsDescription;

		ZString IICRConsignmentItem.IdentityNumber => ZString.Empty;

		ZDecimal IICRConsignmentItem.Value => pack.LinePrice;

		ZString IICRConsignmentItem.Currency => pack.LinePriceCurrency;

		ZString IICRConsignmentItem.IdentityType => ZString.Empty;

		ZBool IICRConsignmentItem.SendFlashpointTemp => false;

		ZDecimal IICRConsignmentItem.FlashpointTempInCelsius => ZDecimal.Zero;

		ITemperatureRequirements IICRConsignmentItem.Temperatures => null;

		ZDecimal IICRConsignmentItem.GrossWeightInKg => new ZWeight(pack.APA_Weight, pack.APA_WeightUQ).InKilogramsSafe.Round(3);

		ZString IICRConsignmentItem.GoodsOriginCountry => pack.Bill?.Origin?.Country.Code ?? ZString.Empty;

		ZInt IICRConsignmentItem.PackageQty => pack.APA_PackQty;

		ZString IICRConsignmentItem.PackageType => pack.APA_PackUQ;

		ZString IICRConsignmentItem.ContainerNumber => pack.Container?.ACN_ContainerNumber ?? ZString.Empty;

		ZString IICRConsignmentItem.MPIApprovedSystemNumber => ZString.Empty;

		IEnumerable<IClassification> IICRConsignmentItem.Classifications
		{
			get
			{
				var imoClass = pack.UNDGs.FirstOrDefault()?.DI_IMOClass ?? ZString.Empty;
				if (!imoClass.IsEmpty)
				{
					yield return new ICRClassification(imoClass, ClassificationTypeList.Codes.SSO);
				}
			}
		}
	}
}
