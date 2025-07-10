using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.MessageBuilders
{
	public class MockCOSTCOContainer : ICOSTCOContainerInformation
	{
		public MockCOSTCOContainer(AsycudaManifestHeader header)
		{
			this.header = Argument.NotNull(header, nameof(header));
		}

		ZString IEQD_EquipmentDetails.EquipmentType
		{
			get
			{
				string result;
				var manifestType = ManifestType;
				switch (manifestType)
				{
					case ManifestTypeList.Codes.AirCargoOutturnReport:
					case ManifestTypeList.Codes.AirExcessOutturnReport:
					case ManifestTypeList.Codes.AirLoadDischarge:
						result = COSTCO.Contants.EquipmentType.UnitLoadDevice;
						break;
					default:
						result = COSTCO.Contants.EquipmentType.BreakBulk;
						break;
				}

				return result;
			}
		}

		ZString ManifestType => header.AMA_ManifestType;

		ZString IEQD_EquipmentDetails.ContainerNumber => "1";

		ZString IEQD_EquipmentDetails.ContainerSize => ZString.Empty;

		ZString IEQD_EquipmentDetails.ContainerStatusLandedPurpose
		{
			get
			{
				var nature = header.AMA_Nature;
				return COSTCOContainer.GetContainerStatusLandedPurpose(nature);
			}
		}

		ZString IEQD_EquipmentDetails.ServiceType => ZString.Empty;

		ZDateTime IDTM_DateTimeUnpakcedDeconsolidated.DateUnpacked => FullyLoadedUnloadedDate;

		ZDateTime FullyLoadedUnloadedDate => header.FullyLoadedUnloadedDate;

		ZDateTime IDTM_DateTimeFullyUnloaded.DateTimeFullyUnloaded => FullyLoadedUnloadedDate;

		ZString ISEL_SealNumber.SealNumber => ZString.Empty;

		ZString ISEL_SealNumber.SealingParty => ZString.Empty;

		ZString ISEL_SealNumber.SealStatus => ZString.Empty;

		readonly AsycudaManifestHeader header;
	}
}
