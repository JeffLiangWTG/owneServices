using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.eTail.DataTransfer
{
	public class NZAirCargoReportConverter : AirManifestConverter
	{
		public NZAirCargoReportConverter(BaseHVLVRelatedJobCommand relatedJobCommand)
			: base(relatedJobCommand)
		{
		}

		protected override void PopulateDataTarget(ITopLevelDataObject dataObject, DataContextType dataContextType, ZString dataContextKey)
		{
		}
	}
}
