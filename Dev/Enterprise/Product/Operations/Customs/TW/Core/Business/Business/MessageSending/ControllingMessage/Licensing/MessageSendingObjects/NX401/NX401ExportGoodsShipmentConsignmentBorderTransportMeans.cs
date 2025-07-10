using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class NX401ExportGoodsShipmentConsignmentBorderTransportMeans : LicensingMessageTransportMeans
	{
		public NX401ExportGoodsShipmentConsignmentBorderTransportMeans(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override ZString GetIDCore() => ZString.Empty;
	}
}
