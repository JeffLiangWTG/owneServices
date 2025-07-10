using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.MessageBuilders
{
	public class COSTCOContainer : ICOSTCOContainerInformation
	{
		public COSTCOContainer(AsycudaContainer container)
		{
			this.container = Argument.NotNull(container, nameof(container));
		}

		ZString IEQD_EquipmentDetails.EquipmentType => COSTCO.Contants.EquipmentType.Container;

		ZString IEQD_EquipmentDetails.ContainerNumber => container.ACN_ContainerNumber;

		ZString IEQD_EquipmentDetails.ContainerSize => container.ContainerType?.RC_Code ?? ZString.Empty;

		ZString IEQD_EquipmentDetails.ContainerStatusLandedPurpose
		{
			get
			{
				var nature = Header?.AMA_Nature ?? ZString.Empty;
				return GetContainerStatusLandedPurpose(nature);
			}
		}

		public static ZString GetContainerStatusLandedPurpose(ZString nature)
		{
			ZString result;
			switch (nature)
			{
				case NatureList.Codes.Import23:
					result = COSTCO.Contants.ContainerStatusLandedPurpose.Import23;
					break;
				case NatureList.Codes.Export22:
					result = COSTCO.Contants.ContainerStatusLandedPurpose.Export22;
					break;
				case NatureList.Codes.Transhipment28:
					result = COSTCO.Contants.ContainerStatusLandedPurpose.Transhipment28;
					break;
				case NatureList.Codes.Transit24:
					result = COSTCO.Contants.ContainerStatusLandedPurpose.Transit24;
					break;
				default:
					result = ZString.Empty;
					break;
			}

			return result;
		}

		ZString IEQD_EquipmentDetails.ServiceType
		{
			get
			{
				ZString result;
				var emptyFullIndicator = container.ACN_EmptyFullIndicator;
				switch (emptyFullIndicator)
				{
					case EmptyFullList.Codes.EmptyContainer:
						result = COSTCO.Contants.ServiceType.EmptyContainer;
						break;
					case EmptyFullList.Codes.FullContainerLoad:
						result = COSTCO.Contants.ServiceType.FullContainerLoad;
						break;
					case EmptyFullList.Codes.LessThanFullContainerLoad:
						result = COSTCO.Contants.ServiceType.LessThanFullContainerLoad;
						break;
					default:
						result = ZString.Empty;
						break;
				}

				return result;
			}
		}

		ZDateTime IDTM_DateTimeUnpakcedDeconsolidated.DateUnpacked
		{
			get
			{
				var unpackedDate = container.ContUnpackTime;
				return !unpackedDate.IsEmpty ? unpackedDate : FullyLoadedUnloadedDate;
			}
		}

		ZDateTime FullyLoadedUnloadedDate => Header?.FullyLoadedUnloadedDate ?? ZDateTime.Empty;

		ZDateTime IDTM_DateTimeFullyUnloaded.DateTimeFullyUnloaded => FullyLoadedUnloadedDate;

		AsycudaManifestHeader Header => container.Header;

		ZString ISEL_SealNumber.SealNumber
		{
			get
			{
				var seal1 = container.ACN_Seal1;
				return Header?.IsBBB ?? false
					? ZString.Empty
					: !seal1.IsEmpty
						? seal1
						: (ZString)COSTCO.Contants.SealNumber.NoSealNo;
			}
		}

		ZString ISEL_SealNumber.SealingParty
		{
			get
			{
				ZString result;
				var partyType = container.ACN_SealingPartyType;
				switch (partyType)
				{
					case SealTypeList.Codes.AgentForwarder:
						result = COSTCO.Contants.SealingParty.Unknown;
						break;
					case SealTypeList.Codes.Carrier:
						result = COSTCO.Contants.SealingParty.Carrier;
						break;
					case SealTypeList.Codes.Customs:
						result = COSTCO.Contants.SealingParty.Customs;
						break;
					case SealTypeList.Codes.ExporterShipper:
						result = COSTCO.Contants.SealingParty.Shipper;
						break;
					case SealTypeList.Codes.TerminalOperator:
						result = COSTCO.Contants.SealingParty.TerminalOperator;
						break;
					default:
						result = ZString.Empty;
						break;
				}

				return result;
			}
		}

		ZString ISEL_SealNumber.SealStatus
		{
			get
			{
				var sealIntackIndicator = container.Header.Bills.Cast<AsycudaBill>().SelectMany(x => x.Packs.Cast<AsycudaPack>())
					.FirstOrDefault(x => x.ContainerPK == container.PK)?.Outturn.C5_SealIntactIndicator ?? ZBool.False;
				return sealIntackIndicator ? COSTCO.Contants.SealStatus.True : COSTCO.Contants.SealStatus.False;
			}
		}

		readonly AsycudaContainer container;
	}
}
