using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.NZ.Business.Data.Universal
{
	public class CusHAWBDataObjectWriter : DataTransfer.Universal.AirManifest.CusHAWBDataObjectWriter<CusHAWB, AirManifestDataObjectWriterHelper>
	{
		public CusHAWBDataObjectWriter(IDataWritingManager manager, AirManifestDataObjectWriterHelper helper)
			: base(manager, helper)
		{
		}

		protected override AirManifestDataObjectWriterHelper GetNewAirManifestLineDataObjectWriterHelper(CusHAWB hawbBO)
		{
			return helper == null ? new AirManifestDataObjectWriterHelper(hawbBO) : new AirManifestDataObjectWriterHelper(hawbBO, helper);
		}

		protected override DataTransfer.Universal.AirManifest.CusHAWBDataObjectWriter<CusHAWB, AirManifestDataObjectWriterHelper> GetNewCusHAWBDataObjectWriter(AirManifestDataObjectWriterHelper helper)
		{
			return new CusHAWBDataObjectWriter(writeManager, helper);
		}

		protected override void PopulateCountrySpecificDetails(Shipment hawbData, CusHAWB hawbBO, AirManifestDataObjectWriterHelper hawbHelper, bool keepExistingData)
		{
			base.PopulateCountrySpecificDetails(hawbData, hawbBO, hawbHelper, keepExistingData);
			hawbData.ConsolidatedCargoStatus = PopulateValue(hawbData.ConsolidatedCargoStatus, keepExistingData, () => ListHelper.GetWithDescription<CodeDescriptionPair>(hawbBO.CS_CustomsStatus, hawbBO.Lookups.CustomsStatus_List)); // TODO: Check with Ben whether this is a Consolidated Status or not
			hawbData.CountryOfSupply = PopulateValue(hawbData.CountryOfSupply, keepExistingData, () => Country.New(hawbBO.GoodsOrigin));
			hawbData.TotalNoOfPacksPackageType = PopulateValue(hawbData.TotalNoOfPacksPackageType, keepExistingData, () => ListHelper.GetWithDescription<PackageType>(hawbBO.CS_PackType, hawbBO.Lookups.PackUnitList));

			var refUNLOCOList = hawbBO.Factory.GetRefUNLOCOList();
			hawbData.PortOfLoading = PopulateValue(hawbData.PortOfLoading, keepExistingData, () => ListHelper.GetWithName(hawbBO.CS_RL_NKLoadPort, refUNLOCOList));
			hawbData.PortOfDischarge = PopulateValue(hawbData.PortOfDischarge, keepExistingData, () => ListHelper.GetWithName(hawbBO.CS_RL_NKDischargePort, refUNLOCOList));

			hawbData.SetOrganizationAddressCollection(() =>
			{
				var goodsLocation = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.GoodsLocation)).GetDataObject(hawbBO.GoodsLocation);
				return hawbData.OrganizationAddressCollection.MergeCollection(new[] { goodsLocation }, keepExistingData, UniversalCommonHelper.IsOrganizationAddressTypeMatched);
			});
			PopulatePackingLines(hawbData, hawbBO);

			PopulateAddInfo(hawbData, hawbBO);
		}

		protected void PopulatePackingLines(Shipment hawbData, CusHAWB hawbBO)
		{
			hawbData.SetPackingLineCollection(() =>
			{
				var packingLineCollection = new DataObjectList<PackingLine>() { Content = CollectionContent.Complete };
				packingLineCollection.Add(CreateFirstPackingLine(hawbBO));
				packingLineCollection.AddRange(CreateSubSequentPackingLines(hawbBO));
				return packingLineCollection;
			});
		}

		PackingLine CreateFirstPackingLine(CusHAWB hawbBO)
		{
			var packingLine = CreateGoodsDetails(hawbBO);
			PopulateUNDGs(packingLine, hawbBO);
			return packingLine;
		}

		IEnumerable<PackingLine> CreateSubSequentPackingLines(CusHAWB hawbBO)
		{
			foreach (var cusHawbItems in hawbBO.CusHAWBItemsCollection.Cast<CusHAWBItems>().OrderBy(x => x.CHI_LineNo))
			{
				yield return CreateGoodsDetails(cusHawbItems);
			}
		}

		PackingLine CreateGoodsDetails(CusHAWB hawbBO)
		{
			var lookups = hawbBO.Lookups;
			return new PackingLine(writeManager.WriterStrategy)
			{
				GoodsDescription = hawbBO.CS_GoodsDescription,
				CountryOfOrigin = ListHelper.GetWithName<Country>(hawbBO.CS_RN_NKGoodsOrigin, lookups.ConsignorCountries),
				LinePrice = hawbBO.CS_GoodsValue,
				LinePriceCurrency = ListHelper.GetWithDescription<Currency>(hawbBO.CS_RX_NKGoodsCurrency, lookups.Currency_List),
				PackQty = new ZLong(hawbBO.CS_PiecesManifested),
				PackType = ListHelper.GetWithDescription<PackageType>(hawbBO.CS_PackType, lookups.PackUnitList),
				Weight = hawbBO.CS_Weight,
				WeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(hawbBO.CS_WeightUQ, lookups.UnitOfWeightList),
				HarmonisedCode = hawbBO.CS_HarmonisedTariffNums
			};
		}

		PackingLine CreateGoodsDetails(CusHAWBItems cusHawbItemsBO)
		{
			var lookups = cusHawbItemsBO.Lookups;
			return new PackingLine(writeManager.WriterStrategy)
			{
				GoodsDescription = cusHawbItemsBO.CHI_GoodsDescription,
				CountryOfOrigin = ListHelper.GetWithName<Country>(cusHawbItemsBO.CHI_RN_NKGoodsOrigin, lookups.GoodsOrigins),
				LinePrice = cusHawbItemsBO.CHI_GoodsValue,
				LinePriceCurrency = ListHelper.GetWithDescription<Currency>(cusHawbItemsBO.CHI_RX_NKGoodsCurrency, lookups.GoodsCurrencies),
				PackQty = new ZLong(cusHawbItemsBO.CHI_PieceCount),
				PackType = ListHelper.GetWithDescription<PackageType>(cusHawbItemsBO.CHI_PackType, lookups.PackUnitList),
				Weight = cusHawbItemsBO.CHI_Weight,
				WeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(cusHawbItemsBO.CHI_WeightUQ, lookups.UnitOfWeightList),
				HarmonisedCode = cusHawbItemsBO.CHI_HarmonisedTariffNums
			};
		}

		void PopulateUNDGs(PackingLine packingLineData, CusHAWB hawbBO)
		{
			packingLineData.SetUNDGCollection(() => hawbBO.IsTSWWriteOff ? ProcessCollection(hawbBO.UNDGs, new UNDGDataObjectWriter(writeManager)) : null);
		}

		protected sealed override IEnumerable<IPropertyValue> GetUserDefinedValues(CusHAWB sourceBO)
		{
			return sourceBO.GetUserDefinedValues();
		}

		void PopulateAddInfo(Shipment hawbData, CusHAWB hawbBO)
		{
			hawbData.SetAddInfoCollection(() => new List<AddInfo>
			{
				new AddInfo
				{
					Key = Constants.AddInfoKeys.IsGSTPrePaid,
					Value = hawbBO.CS_IsGSTPrePaid
				}
			});
		}
	}
}
