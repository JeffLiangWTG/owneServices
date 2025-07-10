using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Common;
using CargoWise.Customs.US.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class EquipmentInfoTypeProvider : IEquipmentInfoType
	{
		readonly AsycudaContainer container;
		readonly RefContainer refContainer;
		readonly IEnumerable<USExportAsycudaPack> packs;

		public EquipmentInfoTypeProvider(AsycudaContainer container, IEnumerable<USExportAsycudaPack> packs)
		{
			this.container = Argument.NotNull(container, "container cannot be null");
			this.packs = packs;
			refContainer = container.Factory.Load<RefContainer>(container.ACN_RC_ContainerType);
		}

		public EquipmentInfoTypeProvider(IEnumerable<USExportAsycudaPack> packs)
		{
			this.packs = packs;
		}

		public IManifestStringType EquipmentTypeCode
		{
			get
			{
				var containerCode = refContainer?.GetCountrySpecificContainerCode(Core.Constants.CountryCodes.UnitedStates) ?? ZString.Empty;
				return new ManifestStringTypeProvider(containerCode);
			}
		}

		public IManifestStringType EquipmentNumber => new ManifestStringTypeProvider(container?.ACN_ContainerNumber ?? ZString.Empty);

		public IManifestStringType EquipmentLength => new ManifestStringTypeProvider(refContainer?.RC_Length.ToZInt().ToString() ?? ZString.Empty);

		public IManifestStringType EquipmentHeight => new ManifestStringTypeProvider(refContainer?.RC_Height.ToZInt().ToString() ?? ZString.Empty);

		public IManifestStringType EquipmentWidth => new ManifestStringTypeProvider(refContainer?.RC_Width.ToZInt().ToString() ?? ZString.Empty);

		public IManifestStringType EquipmentSizeTypeCode => new ManifestStringTypeProvider(refContainer?.RC_ISOType ?? ZString.Empty);

		public IManifestStringType LoadedEmptyStatus
		{
			get
			{
				var result = USExportManifestContainerIndicator.Codes.Loaded;

				if (container == null || container.ACN_EmptyFullIndicator.IsEmpty || container.ACN_EmptyFullIndicator == EmptyFullIndicatorList.Codes.EmptyContainer)
				{
					result = USExportManifestContainerIndicator.Codes.Empty;
				}

				return new ManifestStringTypeProvider(result);
			}
		}

		public IManifestStringType ServiceTypeCode => new ManifestStringTypeProvider(ZString.Empty);

		public Collection<ISealInfoType> SealInfoList
		{
			get
			{
				void AddSealNumberToList(Collection<ISealInfoType> list, ZString sealNumber)
				{
					if (!sealNumber.IsEmpty)
					{
						list.Add(new SealInfoTypeProvider(sealNumber));
					}
				}

				var result = new Collection<ISealInfoType>();
				AddSealNumberToList(result, container?.ACN_Seal1 ?? ZString.Empty);
				AddSealNumberToList(result, container?.ACN_Seal2 ?? ZString.Empty);
				AddSealNumberToList(result, container?.ACN_Seal3 ?? ZString.Empty);

				return result;
			}
		}

		public Collection<ICargoInfoType> CargoInfoList
		{
			get
			{
				var result = new Collection<ICargoInfoType>();

				foreach (USExportAsycudaPack pack in packs)
				{
					result.Add(new CargoInfoTypeProvider(pack));
				}

				return result;
			}
		}

		public Collection<ILicensePlateInfoType> LicensePlateInfoList => new Collection<ILicensePlateInfoType>();

		public Collection<IErrorType> ResponseMessage => new Collection<IErrorType>(System.Array.Empty<ErrorTypeProvider>());
		public Collection<IActionType> Action => new Collection<IActionType>();
	}
}
