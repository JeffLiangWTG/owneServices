using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Protest
{
	partial class ProtestantTypeList
	{
		public static bool RequiresProtestantAddressDetails(ZString protestantType)
		{
			return protestantType == ProtestantTypeList.Codes.ForeignExporterProducer ||
					protestantType == ProtestantTypeList.Codes.Other;
		}
	}
}
