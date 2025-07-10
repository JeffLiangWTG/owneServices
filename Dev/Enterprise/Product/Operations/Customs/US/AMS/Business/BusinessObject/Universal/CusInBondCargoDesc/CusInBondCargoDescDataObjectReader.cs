using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Business.Universal
{
	public class CusInBondCargoDescDataObjectReader : DataTransfer.Universal.CusInBondCargoDescDataObjectReader<CusInBondCargoDesc>
	{
		public CusInBondCargoDescDataObjectReader(PackingLine dataObject, IXmlImportLogger logger, InBondDataObjectReaderHelper helper, ZGuid containerPK)
			: base(dataObject, logger, helper, containerPK)
		{
		}

		protected new InBondDataObjectReaderHelper Helper
		{
			get { return (InBondDataObjectReaderHelper)base.Helper; }
		}

		protected override void FillInBondSpecificData(IColumnIndexer commodityRow, Dictionary<string, ValueSetter> delaySetters, CusInBondCargoDesc commodityBO)
		{
			base.FillInBondSpecificData(commodityRow, delaySetters, commodityBO);
			SetValue(commodityRow, CusInBondCargoDescSchema.BY_RN_NKCountryOfOrigin, dataObject.CountryOfOrigin, delaySetters);
			SetValue(commodityRow, CusInBondCargoDescSchema.BY_CusC4Number, dataObject.ReferenceNumber, delaySetters);
			if (dataObject.DetailedDescription.HasValue)
			{
				commodityBO.DetailedDescription = dataObject.DetailedDescription.Value;
			}
		}

		protected override IEnumerable<ZString> GetSettingOrder(CusInBondCargoDesc commodity)
		{
			yield return ColumnValueSetter.GetKey(commodity.PK, CusInBondCargoDescSchema.BY_HarmonisedTariff);
			yield return ColumnValueSetter.GetKey(commodity.PK, CusInBondCargoDescSchema.BY_MonetaryValue);
			yield return ColumnValueSetter.GetKey(commodity.PK, CusInBondCargoDescSchema.BY_GrossWeight);
			yield return ColumnValueSetter.GetKey(commodity.PK, CusInBondCargoDescSchema.BY_GrossWeightUnit);
			yield return ColumnValueSetter.GetKey(commodity.PK, CusInBondCargoDescSchema.BY_PieceCount);
			yield return ColumnValueSetter.GetKey(commodity.PK, CusInBondCargoDescSchema.BY_ManifestUnitCode);
			yield return ColumnValueSetter.GetKey(commodity.PK, CusInBondCargoDescSchema.BY_Description);
			yield return ColumnValueSetter.GetKey(commodity.PK, CusInBondCargoDescSchema.BY_MarksAndNumbers);
			yield return ColumnValueSetter.GetKey(commodity.PK, CusInBondCargoDescSchema.BY_RN_NKCountryOfOrigin);
			yield return ColumnValueSetter.GetKey(commodity.PK, CusInBondCargoDescSchema.BY_CusC4Number);
		}
	}
}
