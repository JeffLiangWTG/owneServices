using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.DataTransfer.Universal
{
	public class AsycudaPackDataObjectReader : DataObjectReader<PackingLine, AsycudaPack>
	{
		public AsycudaPackDataObjectReader(PackingLine packingLineData, IXmlImportLogger logger, UniversalObjectFactory factory, AsycudaBill billBO, UniversalDataObjectReaderHelper helper)
			: base(packingLineData, logger, factory)
		{
			this.packingLineData = Argument.NotNull(packingLineData, "PackingLineDataObject");
			Argument.NotNull(logger, "Logger");
			Argument.NotNull(factory, "Factory");
			this.billBO = Argument.NotNull(billBO, "AsycudaBill");
			this.helper = Argument.NotNull(helper, "Helper");
		}

		protected override AsycudaPack GetExistingBusinessObject()
		{
			return null;
		}

		protected override AsycudaPack GetNewBusinessObject()
		{
			return billBO.Packs.AddNew();
		}

		protected override void PopulateBusinessObject(AsycudaPack asycudaPackBO)
		{
			var packingLineRow = GetColumnIndexer(asycudaPackBO);

			LinkToContainer(packingLineRow);

			SetValue(packingLineRow, AsycudaPackSchema.APA_PackQty, packingLineData.PackQty);
			SetValue(packingLineRow, AsycudaPackSchema.APA_PackUQ, packingLineData.PackType);
			SetValue(packingLineRow, AsycudaPackSchema.APA_Volume, packingLineData.ManifestedVolume);
			SetValue(packingLineRow, AsycudaPackSchema.APA_VolumeUQ, packingLineData.AddInfoCollection?.GetZStringValue(AddInfoConstants.AsycudePack.ManifestedVolumeUnit, logger));
			SetValue(packingLineRow, AsycudaPackSchema.APA_Weight, packingLineData.ManifestedWeight);
			SetValue(packingLineRow, AsycudaPackSchema.APA_WeightUQ, packingLineData.AddInfoCollection?.GetZStringValue(AddInfoConstants.AsycudePack.ManifestedWeightUnit, logger));
			SetValue(packingLineRow, AsycudaPackSchema.APA_GoodsDescription, packingLineData.GetCleanSingleLineGoodsDescription());
			SetValue(packingLineRow, AsycudaPackSchema.APA_MarksAndNumbers, packingLineData.MarksAndNos);

			var cusOutturn = asycudaPackBO?.Outturn;
			if (cusOutturn != null)
			{
				var cusOutturnRow = GetColumnIndexer(cusOutturn);

				SetValue(cusOutturnRow, CusOutturnSchema.C5_PackagesOutturned, packingLineData.OutturnQty);
				SetValue(cusOutturnRow, CusOutturnSchema.C5_WeightOutturned, packingLineData.OutturnedWeight);
				SetValue(cusOutturnRow, CusOutturnSchema.C5_VolumeOutturned, packingLineData.OutturnedVolume);

				SetValue(cusOutturnRow, CusOutturnSchema.C5_GoodsDescription, packingLineData.AddInfoCollection.GetZStringValue(AddInfoConstants.AsycudePack.ContentsFoundToBe, logger));
				SetValue(cusOutturnRow, CusOutturnSchema.C5_CargoType, packingLineData.AddInfoCollection.GetZStringValue(AddInfoConstants.AsycudePack.CargoType, logger));
				SetValue(cusOutturnRow, CusOutturnSchema.C5_PackageCondition, packingLineData.AddInfoCollection.GetZStringValue(AddInfoConstants.AsycudePack.PackageConditionInd, logger));
				SetValue(cusOutturnRow, CusOutturnSchema.C5_WeightOutturnedUQ, packingLineData.AddInfoCollection.GetZStringValue(AddInfoConstants.AsycudePack.OutturnedWeightUnit, logger));
				SetValue(cusOutturnRow, CusOutturnSchema.C5_VolumeOutturnedUQ, packingLineData.AddInfoCollection.GetZStringValue(AddInfoConstants.AsycudePack.OutturnedVolumeUnit, logger));
				SetValue(cusOutturnRow, CusOutturnSchema.C5_SealIntactIndicator, packingLineData.AddInfoCollection.GetZBoolValue(AddInfoConstants.AsycudePack.SealIntactIndicator, logger));

				new GenAddOnColumnCollectionDataObjectReader(logger).ReadIntoBusinessObject(packingLineData.AddInfoCollection, helper.GetCusOutturnGenAddOnColumnList(cusOutturn), cusOutturn);
			}
		}

		void LinkToContainer(IColumnIndexer packingLineRow)
		{
			if (packingLineData.ContainerNumber.HasValue)
			{
				var packingLinePK = packingLineRow.GetValue(AsycudaPackSchema.PK);
				var containerNumber = packingLineData.ContainerNumber;
				if (containerNumber.HasValue)
				{
					var query = new ZQuery(AsycudaContainerSchema.ACN_AMA_Manifest, billBO?.ABL_AMA);
					query.AddToFilter(AsycudaContainerSchema.ACN_ContainerNumber, containerNumber);
					query.FetchOnlyFromLocalCache = !(billBO.Header?.IsInDatabase ?? false);
					query.OrderBy = AsycudaContainer.Schema.PK;
					var container = factory.LoadTop1<AsycudaContainer>(query);

					if (container != null)
					{
						var link = factory.New<AsycudaContainerBillOrPackageLink>();
						var linkLow = GetColumnIndexer(link);
						SetValue(linkLow, AsycudaContainerBillOrPackageLinkSchema.APC_APA_Pack, packingLinePK);
						SetValue(linkLow, AsycudaContainerBillOrPackageLinkSchema.APC_ACN_Container, container.PK);
						SetValue(linkLow, AsycudaContainerBillOrPackageLinkSchema.APC_ClusterKey, container.ACN_ClusterKey);
					}
				}
			}
		}

		readonly PackingLine packingLineData;
		readonly AsycudaBill billBO;
		readonly UniversalDataObjectReaderHelper helper;
	}
}
