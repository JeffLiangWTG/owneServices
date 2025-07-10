using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.ZA.Business.MessageBuilders;

namespace Enterprise.Customs.ZA.Manifest.Business.EDIFACT
{
	public class CusCarContainer : IZACusCarContainer
	{
		public CusCarContainer(AsycudaContainer container)
		{
			this.container = container;
		}

		readonly AsycudaContainer container;

		#region ICusCarContainer

		public ZString ContainerNumber => container.ACN_ContainerNumber;
		public ZString ContainerTypeISO => container.ContainerType?.RC_ISOType ?? ZString.Empty;
		public ZDecimal GrossMassInKilos => container.ACN_GoodsWeightInKilos;
		public ZDecimal VerifiedGrossMassInKilos => GrossMassInKilos + TareWeightInKilos;

		ZDecimal TareWeightInKilos => container.ContainerType?.RC_TareWeight ?? ZDecimal.Zero;

		public ContainerStatus GetContainerStatus(string countryCode)
		{
			var headerNature = container.Header?.AMA_Nature ?? ZString.Empty;
			switch (headerNature)
			{
				case ShipmentTypeList.Codes.Import23:
					return ContainerStatus.Import;
				case ShipmentTypeList.Codes.Export22:
					return ContainerStatus.Export;
				case ShipmentTypeList.Codes.Transhipment28:
					return ContainerStatus.Transhipment;
				case ShipmentTypeList.Codes.Transit24:
					return ContainerStatus.Transit;
				default:
					return ContainerStatus.None;
			}
		}

		public EmptyFullServiceType GetEmptyFullServiceType(string countryCode)
		{
			if (container.Header?.AMA_IsBuyersConsolidation == ZBool.True)
			{
				return EmptyFullServiceType.FullSingleConsignmentFCL;
			}
			else
			{
				switch (container.ACN_EmptyFullIndicator)
				{
					case ZaEmptyFullIndicatorList.Codes.EmptyContainer:
						return EmptyFullServiceType.Empty;
					case ZaEmptyFullIndicatorList.Codes.FullContainerLoad:
						return EmptyFullServiceType.FullSingleConsignmentFCL;
					case ZaEmptyFullIndicatorList.Codes.LessThanFullContainerLoad:
						return EmptyFullServiceType.FullMixedConsignmentLCL;
					case ZaEmptyFullIndicatorList.Codes.FclGroupage:
						return EmptyFullServiceType.FullFCLGroupage;
					default:
						return EmptyFullServiceType.None;
				}
			}
		}

		public IEnumerable<ICusCarSeal> GetSeals(string countryCode)
		{
			var seal1 = container.ACN_Seal1;
			if (!seal1.IsEmpty)
			{
				yield return new CusCarSeal(container.Factory, seal1, container.ACN_SealingPartyType, container.ACN_SealType1);
			}

			var seal2 = container.ACN_Seal2;
			if (!seal2.IsEmpty)
			{
				yield return new CusCarSeal(container.Factory, seal2, container.ACN_SealingPartyType2, container.ACN_SealType2);
			}

			var seal3 = container.ACN_Seal3;
			if (!seal3.IsEmpty)
			{
				yield return new CusCarSeal(container.Factory, seal3, container.ACN_SealingPartyType3, container.ACN_SealType3);
			}
		}

		public ZString GetLandedPurpose() => container.LandedPurpose;

		#endregion
	}
}
