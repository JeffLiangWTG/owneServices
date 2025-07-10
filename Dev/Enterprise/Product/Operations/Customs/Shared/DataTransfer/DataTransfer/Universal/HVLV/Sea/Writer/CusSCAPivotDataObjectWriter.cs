using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalXml = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.DataTransfer.Universal.SeaManifest
{
	internal class CusSCAPivotDataObjectWriter : CusSCAPivotDataObjectWriter<BaseCusSCAPivot>
	{
		public CusSCAPivotDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{ }

		protected override ICodeDescriptionPairList GetPackageTypes(BaseCusSCAPivot bizObj) { return null; }
		protected override ICodeDescriptionPairList GetUnitsOfWeight(BaseCusSCAPivot bizObj) { return null; }
	}

	public abstract class CusSCAPivotDataObjectWriter<TCusSCAPivot> : DataObjectWriter<TCusSCAPivot, UniversalXml.PackingLine>
		where TCusSCAPivot : BaseCusSCAPivot
	{
		protected CusSCAPivotDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{ }

		protected BusinessObjectFactory Factory
		{
			get { return writeManager.Action.FactoryForProcessing; }
		}

		protected sealed override UniversalXml.PackingLine PopulateDataObject(TCusSCAPivot sourceBO)
		{
			var data = new UniversalXml.PackingLine(writeManager.WriterStrategy);
			PopulateDataObject(sourceBO, data, false);
			return data;
		}

		void PopulateDataObject(TCusSCAPivot bizObj, UniversalXml.PackingLine data, bool keepExistingData)
		{
			var bizObjRow = (IColumnIndexer)bizObj;
			data.ContainerNumber = PopulateValue(data.ContainerNumber, keepExistingData,
				() =>
				{
					var container = Factory.Load<BaseCusSCAContainer>(bizObjRow.GetValue(CusSCAPivotSchema.CV_CN));
					if (container != null)
					{
						return container.GetValue(CusSCAContainerSchema.CN_ContainerNumber);
					}
					return null;
				});
			data.BillNumber = PopulateValue(data.BillNumber, keepExistingData,
				() =>
				{
					var bill = Factory.Load<BaseCusSCAHouse>(bizObjRow.GetValue(CusSCAPivotSchema.CV_CA));
					if (bill != null)
					{
						return bill.GetValue(CusSCAHouseSchema.CA_HouseBill);
					}
					return null;
				});

			data.PackQty = PopulateValue(data.PackQty, keepExistingData,
				() => new ZLong(bizObjRow.GetValue(CusSCAPivotSchema.CV_PackageCount)));
			data.PackType = PopulateValue(data.PackType, keepExistingData,
				() => ListHelper.GetWithDescription<UniversalXml.PackageType>(bizObjRow.GetValue(CusSCAPivotSchema.CV_PackageType), GetPackageTypes(bizObj)));
			data.Weight = PopulateValue(data.Weight, keepExistingData,
				() => bizObjRow.GetValue(CusSCAPivotSchema.CV_Weight));
			data.WeightUnit = PopulateValue(data.WeightUnit, keepExistingData,
				() => ListHelper.GetWithDescription<UniversalXml.UnitOfWeight>(bizObjRow.GetValue(CusSCAPivotSchema.CV_WeightUQ), GetUnitsOfWeight(bizObj)));
			data.GoodsDescription = PopulateValue(data.GoodsDescription, keepExistingData,
				() => bizObjRow.GetValue(CusSCAPivotSchema.CV_GoodsDescription));
			data.MarksAndNos = PopulateValue(data.MarksAndNos, keepExistingData,
				() => bizObjRow.GetValue(CusSCAPivotSchema.CV_MarksAndNumbers));
			data.Volume = PopulateValue(data.Volume, keepExistingData,
				() => bizObjRow.GetValue(CusSCAPivotSchema.CV_Volume));
			data.LinePrice = PopulateValue(data.LinePrice, keepExistingData,
				() => bizObjRow.GetValue(CusSCAPivotSchema.CV_GoodsValue));
			data.LinePriceCurrency = PopulateValue(data.LinePriceCurrency, keepExistingData,
				() => ListHelper.GetWithDescription<UniversalXml.Currency>(bizObj.CV_RX_NKGoodsCurrency, bizObj.Lookups.GoodsCurrencies));
			data.HarmonisedCode = PopulateValue(data.HarmonisedCode, keepExistingData,
				() => bizObjRow.GetValue(CusSCAPivotSchema.CV_HarmonisedTariffNums));
			WriteUNDGCollection(bizObj, data, keepExistingData);
			PopulateCountrySpecificData(bizObj, data, keepExistingData);
		}

		protected virtual void PopulateCountrySpecificData(TCusSCAPivot bizObj, UniversalXml.PackingLine data, bool keepExistingData) { }

		protected abstract ICodeDescriptionPairList GetPackageTypes(TCusSCAPivot bizObj);
		protected abstract ICodeDescriptionPairList GetUnitsOfWeight(TCusSCAPivot bizObj);

		protected virtual void WriteUNDGCollection(TCusSCAPivot bizObj, UniversalXml.PackingLine data, bool keepExistingData)
		{
		}
	}
}

// Tested in CusSCAOceanBillDataObjectWriter.cs
