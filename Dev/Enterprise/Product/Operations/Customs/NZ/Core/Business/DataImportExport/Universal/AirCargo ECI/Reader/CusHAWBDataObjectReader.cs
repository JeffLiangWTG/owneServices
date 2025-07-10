using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Data.Universal
{
	public class CusHAWBDataObjectReader : DataTransfer.Universal.AirManifest.CusHAWBDataObjectReader<CusMAWB, CusHAWB, DataTransfer.Universal.AirManifest.AirManifestDataObjectReaderHelper>
	{
		public CusHAWBDataObjectReader(Shipment shipmentDataObject, Shipment mawbDataObject, IXmlImportLogger logger, DataTransfer.Universal.AirManifest.AirManifestDataObjectReaderHelper helper, CusMAWB mawb, CusHAWB masterHouse, bool isHVLV, bool singleHAWBCheck = false)
			: base(shipmentDataObject, mawbDataObject, logger, helper, mawb, masterHouse, isHVLV, singleHAWBCheck)
		{
		}

		internal new CusHAWB GetExistingBusinessObject()
		{
			return base.GetExistingBusinessObject();
		}

		protected override DataTransfer.Universal.AirManifest.CusHAWBDataObjectReader<CusMAWB, CusHAWB, DataTransfer.Universal.AirManifest.AirManifestDataObjectReaderHelper> GetNewCusHAWBDataObjectReader(Shipment shipmentDataObject, CusHAWB masterHouse)
		{
			return new CusHAWBDataObjectReader(shipmentDataObject, mawbDataObject, logger, helper, mawb, masterHouse, isHVLV, singleHAWBCheck);
		}

		protected override void FillCountrySpecificDetails(CusHAWB hawb)
		{
			base.FillCountrySpecificDetails(hawb);
			var hawbRow = GetColumnIndexer(hawb);
			if (!ShouldUseFirstPackingLineData)
			{
				SetValue(hawbRow, CusHAWBSchema.CS_RN_NKGoodsOrigin, dataObject.CountryOfSupply);
			}
			SetValue(hawbRow, CusHAWBSchema.CS_RL_NKLoadPort, dataObject.PortOfLoading);
			SetValue(hawbRow, CusHAWBSchema.CS_RL_NKDischargePort, dataObject.PortOfDischarge);
			SetValue(hawbRow, CusHAWBSchema.CS_IsGSTPrePaid, dataObject.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.IsGSTPrePaid, logger));
			PopulateWorkflowCustomFields(hawb, dataObject);

			FillPackingLines(hawb);
		}

		protected override CharacterCase StringValueCharacterCase => CharacterCase.Upper;

		void FillPackingLines(CusHAWB hawb)
		{
			if (dataObject.PackingLineCollection != null)
			{
				FillFirstPackingLine(hawb);
				FillSubSequentPackingLines(hawb);
			}
		}

		protected virtual void FillFirstPackingLine(CusHAWB hawb)
		{
			var packingLine = dataObject.PackingLineCollection.FirstOrDefault();
			if (packingLine != null)
			{
				if (ShouldUseFirstPackingLineData)
				{
					FillGoodsDetails(hawb, packingLine);
				}
				FillUNDGs(hawb, packingLine);
			}
		}

		protected virtual void FillSubSequentPackingLines(CusHAWB hawb)
		{
			var packingLineCollections = dataObject.PackingLineCollection;
			if (packingLineCollections.Any())
			{
				hawb.CusHAWBItemsCollection.DeleteAll();
				foreach (var packingLine in packingLineCollections.Skip(1))
				{
					FillGoodsDetails(hawb.CusHAWBItemsCollection.AddNew(), packingLine);
				}
			}
		}

		bool ShouldUseFirstPackingLineData
		{
			get
			{
				if (!shouldUseFirstPackingLineData.HasValue)
				{
					var firstPackingLine = dataObject.PackingLineCollection?.FirstOrDefault();
					shouldUseFirstPackingLineData = firstPackingLine != null && firstPackingLine.PackQty.HasValue && firstPackingLine.PackType != null;
				}
				return shouldUseFirstPackingLineData.Value;
			}
		}
		bool? shouldUseFirstPackingLineData;

		bool ShouldFillGoodsDetailsFromSubShipment
		{
			get
			{
				if (!shouldFillGoodsDetailsFromSubShipment.HasValue)
				{
					shouldFillGoodsDetailsFromSubShipment = !ShouldUseFirstPackingLineData;
				}
				return shouldFillGoodsDetailsFromSubShipment.Value;
			}
		}
		bool? shouldFillGoodsDetailsFromSubShipment;

		protected override void FillGoodsDescription(IColumnIndexer hawbRow, CusHAWB hawb)
		{
			if (!ShouldUseFirstPackingLineData)
			{
				base.FillGoodsDescription(hawbRow, hawb);
			}
		}

		protected override void FillGoodsValue(IColumnIndexer hawbRow)
		{
			if (ShouldFillGoodsDetailsFromSubShipment)
			{
				base.FillGoodsValue(hawbRow);
			}
		}

		protected override void FillPiecesManifested(IColumnIndexer hawbRow)
		{
			if (ShouldFillGoodsDetailsFromSubShipment)
			{
				base.FillPiecesManifested(hawbRow);
			}
		}

		protected override void FillPackType(IColumnIndexer hawbRow)
		{
			if (ShouldFillGoodsDetailsFromSubShipment)
			{
				base.FillPackType(hawbRow);
			}
		}

		protected override ZString ConvertCustomsPackageType(ZString packType)
		{
			return PackageTypeConverter.GetCustomsPackageType(packType);
		}

		protected override void FillWeight(IColumnIndexer hawbRow)
		{
			if (!ShouldUseFirstPackingLineData)
			{
				base.FillWeight(hawbRow);
			}
		}

		void FillGoodsDetails(CusHAWB hawb, PackingLine packingLine)
		{
			var hawbRow = GetColumnIndexer(hawb);
			SetValue(hawbRow, CusHAWBSchema.CS_GoodsDescription, packingLine.GetCleanSingleLineGoodsDescription());
			SetValue(hawbRow, CusHAWBSchema.CS_RN_NKGoodsOrigin, packingLine.CountryOfOrigin);
			SetValue(hawbRow, CusHAWBSchema.CS_GoodsValue, packingLine.LinePrice);
			SetValue(hawbRow, CusHAWBSchema.CS_RX_NKGoodsCurrency, packingLine.LinePriceCurrency);
			SetValue(hawbRow, CusHAWBSchema.CS_PiecesManifested, packingLine.PackQty);
			SetValue(hawbRow, CusHAWBSchema.CS_PackType, PackageTypeConverter.GetCustomsPackageType(packingLine.PackType?.Code ?? ZString.Empty));
			SetValue(hawbRow, CusHAWBSchema.CS_Weight, packingLine.Weight);
			SetValue(hawbRow, CusHAWBSchema.CS_WeightUQ, packingLine.WeightUnit);
			SetValue(hawbRow, CusHAWBSchema.CS_HarmonisedTariffNums, packingLine.HarmonisedCode);
		}

		void FillUNDGs(CusHAWB hawb, PackingLine packingLine)
		{
			if (packingLine.UNDGCollection != null)
			{
				hawb.UNDGs.DeleteAll();
				var undg = packingLine.UNDGCollection.FirstOrDefault();
				if (undg != null)
				{
					var reader = new UNDGDataObjectReader(undg, logger, factory);
					hawb.UNDGs.Add(reader.ReadIntoBusinessObject());
				}
			}
		}

		void FillGoodsDetails(CusHAWBItems hawbItems, PackingLine packingLine)
		{
			var hawbItemsRow = GetColumnIndexer(hawbItems);
			SetValue(hawbItemsRow, CusHAWBItemsSchema.CHI_GoodsDescription, packingLine.GoodsDescription);
			SetValue(hawbItemsRow, CusHAWBItemsSchema.CHI_RN_NKGoodsOrigin, packingLine.CountryOfOrigin);
			SetValue(hawbItemsRow, CusHAWBItemsSchema.CHI_GoodsValue, packingLine.LinePrice);
			SetValue(hawbItemsRow, CusHAWBItemsSchema.CHI_RX_NKGoodsCurrency, packingLine.LinePriceCurrency);
			SetValue(hawbItemsRow, CusHAWBItemsSchema.CHI_PieceCount, packingLine.PackQty);
			SetValue(hawbItemsRow, CusHAWBItemsSchema.CHI_PackType, PackageTypeConverter.GetCustomsPackageType(packingLine.PackType?.Code ?? ZString.Empty));
			SetValue(hawbItemsRow, CusHAWBItemsSchema.CHI_Weight, packingLine.Weight);
			SetValue(hawbItemsRow, CusHAWBItemsSchema.CHI_WeightUQ, packingLine.WeightUnit);
			SetValue(hawbItemsRow, CusHAWBItemsSchema.CHI_HarmonisedTariffNums, packingLine.HarmonisedCode);
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(CusHAWB hawb)
		{
			var sb = new ZStringBuilder(base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(hawb));
			if (sb.IsEmpty)
			{
				if (ShouldCheckMessageBeforeUpdate && IsMessagingActive(hawb))
				{
					sb.Append(Res.GetString("5BAC3977-E823-4CDF-8A88-633816C1BCBF", "Messaging is active for {0}", hawb.HumanReadableName));
				}
				if (!PackingLinesCurrenciesMatch())
				{
					if (!sb.IsEmpty)
					{
						sb.AppendLine();
					}
					sb.Append(Res.GetString("e116cdaa-8403-44b6-a4f7-ab6e3ddd3a49", "Import failed as line price currency needs to match on all lines"));
				}
			}
			return sb.ToString();
		}

		protected virtual bool ShouldCheckMessageBeforeUpdate => true;

		public static bool IsMessagingActive(CusHAWB hawb)
		{
			return hawb != null && hawb.IsInDatabase && hawb.CS_CustomsStatus != LowValueConsignmentStatusList.Codes.NotSentToCustoms;
		}

		public bool PackingLinesCurrenciesMatch()
		{
			return dataObject.PackingLineCollection == null || dataObject.PackingLineCollection.Select(x => x.LinePriceCurrency?.Code).Distinct().Count() <= 1;
		}
	}
}
