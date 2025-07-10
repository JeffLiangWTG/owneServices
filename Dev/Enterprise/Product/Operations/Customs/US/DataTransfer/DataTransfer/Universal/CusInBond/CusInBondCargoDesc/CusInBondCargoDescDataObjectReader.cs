using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class CusInBondCargoDescDataObjectReader<TCommodity> : DataObjectReader<PackingLine, TCommodity>
		where TCommodity : CusInBondCargoDesc
	{
		public CusInBondCargoDescDataObjectReader(PackingLine dataObject, IXmlImportLogger logger, InBondDataObjectReaderHelper helper, ZGuid containerPK)
			: base(dataObject, logger, helper.Factory)
		{
			ContainerPK = Argument.NotNull(containerPK, nameof(containerPK));
			Helper = Argument.NotNull(helper, nameof(helper));
		}

		protected InBondDataObjectReaderHelper Helper { get; }

		protected ZGuid ContainerPK { get; }

		protected override TCommodity GetExistingBusinessObject()
		{
			return null;
		}

		protected override void PopulateBusinessObject(TCommodity commodityBO)
		{
			var commodityRow = GetColumnIndexer(commodityBO);
			var delaySetters = IsDefaultingEnabled ? new Dictionary<string, ValueSetter>() : null;
			SetValue(commodityRow, CusInBondCargoDescSchema.BY_ParentID, ContainerPK);
			SetValue(commodityRow, CusInBondCargoDescSchema.BY_ParentTableCode, CusInBondContainerSchema.Constants.Prefix);
			SetValue(commodityRow, CusInBondCargoDescSchema.BY_HarmonisedTariff, dataObject.HarmonisedCode, delaySetters);
			SetValue(commodityRow, CusInBondCargoDescSchema.BY_MonetaryValue, dataObject.LinePrice, delaySetters);
			SetValue(commodityRow, CusInBondCargoDescSchema.BY_Description, dataObject.GoodsDescription, delaySetters);
			SetValue(commodityRow, CusInBondCargoDescSchema.BY_MarksAndNumbers, dataObject.MarksAndNos, delaySetters);
			SetValue(commodityRow, CusInBondCargoDescSchema.BY_PieceCount, dataObject.PackQty, delaySetters);
			SetValue(commodityRow, CusInBondCargoDescSchema.BY_ManifestUnitCode, dataObject.PackType, delaySetters);
			SetValue(commodityRow, CusInBondCargoDescSchema.BY_GrossWeight, dataObject.Weight, delaySetters);
			SetValue(commodityRow, CusInBondCargoDescSchema.BY_GrossWeightUnit, dataObject.WeightUnit, delaySetters);
			FillInBondSpecificData(commodityRow, delaySetters, commodityBO);
			delaySetters.SetValueInSpecificOrder(GetSettingOrder(commodityBO));
			PopulateChildCommodities(commodityBO);
		}

		protected virtual void FillInBondSpecificData(IColumnIndexer commodityRow, Dictionary<string, ValueSetter> delaySetters, TCommodity commodityBO)
		{
		}

		protected virtual IEnumerable<ZString> GetSettingOrder(TCommodity commodity)
		{
			return System.Array.Empty<ZString>();
		}

		protected virtual void PopulateChildCommodities(TCommodity commodityBO)
		{
		}
	}
}
