using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using AsycudaPack = Enterprise.Customs.ZA.Business.AsycudaPack;

namespace Enterprise.Customs.ZA.DataTransfer.Universal
{
	public class AsycudaPackDataObjectWriter : DataObjectWriter<AsycudaPack, PackingLine>
	{
		public AsycudaPackDataObjectWriter(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper)
			: base(manager)
		{
			this.helper = Argument.NotNull(helper, "Helper");
		}

		protected override PackingLine PopulateDataObject(AsycudaPack packBO)
		{
			if (packBO != null)
			{
				var packingLineData = new PackingLine(writeManager.WriterStrategy)
				{
					ContainerNumber = packBO.Container?.ACN_ContainerNumber ?? ZString.Empty,
					ManifestedWeight = packBO.APA_Weight,
					ManifestedVolume = packBO.APA_Volume,
					MarksAndNos = packBO.APA_MarksAndNumbers,
					PackQty = new ZLong(packBO.APA_PackQty),
					PackType = ListHelper.GetWithDescription<PackageType>(packBO.APA_PackUQ, packBO.Lookups.PackUQList),
					OutturnQty = packBO.Outturn?.C5_PackagesOutturned,
					GoodsDescription = packBO.APA_GoodsDescription,
					OutturnedWeight = packBO.Outturn?.C5_WeightOutturned,
					OutturnedVolume = packBO.Outturn?.C5_VolumeOutturned
				};

				PopulateAsycudaPackAddInfosData(packBO, packingLineData);

				return packingLineData;
			}
			return null;
		}

		void PopulateAsycudaPackAddInfosData(AsycudaPack packBO, PackingLine packingLineData)
		{
			if (packingLineData != null)
			{
				packingLineData.SetAddInfoCollection(() =>
				{
					var list = new List<AddInfo>();

					if (packBO != null)
					{
						list.AddOrUpdate(AddInfoConstants.AsycudePack.ManifestedWeightUnit, packBO.APA_WeightUQ);
						list.AddOrUpdate(AddInfoConstants.AsycudePack.ManifestedVolumeUnit, packBO.APA_VolumeUQ);

						var outturn = packBO.Outturn;
						if (outturn != null)
						{
							list.AddOrUpdate(AddInfoConstants.AsycudePack.CargoType, outturn.C5_CargoType);
							list.AddOrUpdate(AddInfoConstants.AsycudePack.ContentsFoundToBe, outturn.C5_GoodsDescription);
							list.AddOrUpdate(AddInfoConstants.AsycudePack.CargoTypeDesc, outturn.Lookups.CargoTypeList.GetDescriptionFromCode(outturn.C5_CargoType));
							list.AddOrUpdate(AddInfoConstants.AsycudePack.PackageConditionInd, outturn.C5_PackageCondition);
							list.AddOrUpdate(AddInfoConstants.AsycudePack.PackageConditionIndDesc, outturn.Lookups.PackConditionList.GetDescriptionFromCode(outturn.C5_PackageCondition));
							list.AddOrUpdate(AddInfoConstants.AsycudePack.ExcessShortInd, outturn.ExcessShortInd);
							list.AddOrUpdate(AddInfoConstants.AsycudePack.ExcessShortIndDesc, outturn.Lookups.ExcessShortIndicatorList.GetDescriptionFromCode(outturn.ExcessShortInd));
							list.AddOrUpdate(AddInfoConstants.AsycudePack.PackCondDesc, outturn.PackCondDesc);
							list.AddOrUpdate(AddInfoConstants.AsycudePack.ContShouldBe, outturn.ContShouldBe);
							list.AddOrUpdate(AddInfoConstants.AsycudePack.OutturnedWeightUnit, outturn.C5_WeightOutturnedUQ);
							list.AddOrUpdate(AddInfoConstants.AsycudePack.OutturnedVolumeUnit, outturn.C5_VolumeOutturnedUQ);
							list.AddOrUpdate(AddInfoConstants.AsycudePack.SealIntactIndicator, helper.ConvertBoolToYesNo(outturn.C5_SealIntactIndicator));
						}
					}
					return list;
				});
			}
		}

		readonly AsycudaManifestHeaderDataObjectWriterHelper helper;
	}
}
