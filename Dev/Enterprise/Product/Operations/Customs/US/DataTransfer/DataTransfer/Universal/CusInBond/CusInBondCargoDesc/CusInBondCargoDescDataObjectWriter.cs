using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class CusInBondCargoDescDataObjectWriter : DataObjectWriter<CusInBondCargoDesc, PackingLine>
	{
		public CusInBondCargoDescDataObjectWriter(IDataWritingManager writeManager, InBondDataObjectWriterHelper helper)
			: base(writeManager)
		{
			this.writerHelper = Argument.NotNull(helper, "helper");
		}
		readonly InBondDataObjectWriterHelper writerHelper;

		protected InBondDataObjectWriterHelper Helper
		{
			get { return writerHelper; }
		}

		protected override PackingLine PopulateDataObject(CusInBondCargoDesc commodityBO)
		{
			var commodityData = new PackingLine(writeManager.WriterStrategy);
			commodityData.HarmonisedCode = commodityBO.BY_HarmonisedTariff;
			commodityData.LinePrice = commodityBO.BY_MonetaryValue;
			commodityData.GoodsDescription = commodityBO.BY_Description;
			commodityData.MarksAndNos = commodityBO.BY_MarksAndNumbers;
			commodityData.SetPackedItemCollection(() => PopulatePackedItemsData(commodityBO, commodityData));
			PopulateInBondSpecificData(commodityBO, commodityData);
			return commodityData;
		}

		protected virtual void PopulateInBondSpecificData(CusInBondCargoDesc commodityBO, PackingLine commodityData)
		{
		}

		protected virtual List<PackedItem> PopulatePackedItemsData(CusInBondCargoDesc commodityBO, PackingLine commodityData)
		{
			return commodityData.PackedItemCollection ?? new List<PackedItem>();
		}
	}
}
