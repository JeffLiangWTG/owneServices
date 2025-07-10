using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class OpeningLadingLinesProvider : IOpeningLadingLines
	{
		public OpeningLadingLinesProvider(CusSupportingInfo manifestToOpen)
		{
			this.manifestToOpen = manifestToOpen;
		}
		readonly CusSupportingInfo manifestToOpen;

		public ZString WarehouseCode => manifestToOpen.CSI_CustomsOffice;
		public ZDecimal AmountInWarehouse => ZDecimal.Zero;
		public ZDecimal AmountTtoOpen => manifestToOpen.CSI_Quantity2;
		public ZString BrandNo => ZString.Empty;
		public ZString ItemType => ZString.Empty;
		public ZString Unit => ZString.Empty;
		public ZInt TotalQuantity => ZInt.Zero;
		public ZInt AmountClosed => ZInt.Zero;
		public ZString MeasurementUnit => ZString.Empty;
		public ZInt OpeningLineNumber => manifestToOpen.CSI_LineNo;
	}
}
