using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.ZA.DataTransfer.Universal
{
	public interface IAsycudaContainerDataObjectWriter
	{
		Container GetDataObject(AsycudaContainer sourceBO);
	}

	public class AsycudaContainerDataObjectWriter<TContainer> : DataObjectWriter<TContainer, Container>, IAsycudaContainerDataObjectWriter
		where TContainer : AsycudaContainer
	{
		readonly AsycudaManifestHeaderDataObjectWriterHelper helper;

		public AsycudaContainerDataObjectWriter(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper)
			: base(manager)
		{
			this.helper = helper;
		}

		protected override Container PopulateDataObject(TContainer containerBO)
		{
			if (containerBO != null)
			{
				var uxmlContainerData = new Container(writeManager.WriterStrategy)
				{
					ContainerNumber = containerBO.ACN_ContainerNumber,
					Seal = containerBO.ACN_Seal1,
					ContainerStatus = new CodeDescriptionPair() { Code = containerBO.ACN_EmptyFullIndicator, Description = containerBO.Lookups?.EmptyFullList.GetDescriptionFromCode(containerBO.ACN_EmptyFullIndicator) },
					ContainerType = helper.ConvertGuidToContainerType(containerBO.ACN_RC_ContainerType),
					SealPartyType = new CodeDescriptionPair() { Code = containerBO.ACN_SealingPartyType, Description = containerBO.Lookups?.SealTypeList.GetDescriptionFromCode(containerBO.ACN_SealingPartyType) }
				};

				var containerType = containerBO.ContainerType;
				if (containerType != null)
				{
					uxmlContainerData.ContainerType = UniversalDataBuss.DataObjects.Universal.ContainerType.New(containerType);

					var typeCode = uxmlContainerData.ContainerType?.ISOCode;
					var customsCode = typeCode.HasValue ? ZZRefCusMapCombined.MapCW1CodeToCustomsCode(containerBO.Factory, Core.Constants.CountryCodes.SouthAfrica, RefCusMapTypeList.Codes.CTYPE, typeCode.Value, ZDateTime.Today) : ZString.Empty;
					if (!customsCode.IsEmpty)
					{
						uxmlContainerData.ContainerType.ISOCode = customsCode;
					}

					uxmlContainerData.TotalHeight = containerType.RC_Height;
					uxmlContainerData.TotalWidth = containerType.RC_Width;
					uxmlContainerData.TotalLength = containerType.RC_Length;
				}

				uxmlContainerData.AddInfoCollection = PopulateContainersAddInfosData(containerBO);
				return uxmlContainerData;
			}
			return null;
		}

		List<AddInfo> PopulateContainersAddInfosData(TContainer containerBO)
		{
			var addInfoList = new List<AddInfo>();
			if (containerBO != null)
			{
				addInfoList.AddOrUpdate(AddInfoConstants.AsycudaContainer.ContUnpackTime, helper.ConvertDateTimeToString(containerBO.ContUnpackTime));
				addInfoList.AddOrUpdate(AddInfoConstants.AsycudaContainer.GateInOutDate, helper.ConvertDateTimeToString(containerBO.GateInOutDate));
			}
			return addInfoList;
		}

		Container IAsycudaContainerDataObjectWriter.GetDataObject(AsycudaContainer sourceBO) => GetDataObject(sourceBO as TContainer);
	}
}
