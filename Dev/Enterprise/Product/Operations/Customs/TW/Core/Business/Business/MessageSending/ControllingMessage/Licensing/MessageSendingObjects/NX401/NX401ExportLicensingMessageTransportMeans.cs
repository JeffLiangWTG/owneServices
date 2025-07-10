using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class NX401ExportLicensingMessageTransportMeans : LicensingMessageTransportMeans
	{
		public NX401ExportLicensingMessageTransportMeans(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override ZDate GetArrivalDateTimeCore() => default;
	}
}
