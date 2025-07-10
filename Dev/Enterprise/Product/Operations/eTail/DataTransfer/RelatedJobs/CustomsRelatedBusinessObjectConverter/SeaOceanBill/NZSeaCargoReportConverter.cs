using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.eTail.DataTransfer
{
	public class NZSeaCargoReportConverter : SeaOceanBillConverter
	{
		public NZSeaCargoReportConverter(BaseHVLVRelatedJobCommand relatedJobCommand)
			: base(relatedJobCommand)
		{
		}

		protected override void PopulateDataTarget(ITopLevelDataObject dataObject, DataContextType dataContextType, ZString dataContextKey)
		{
		}
	}
}
