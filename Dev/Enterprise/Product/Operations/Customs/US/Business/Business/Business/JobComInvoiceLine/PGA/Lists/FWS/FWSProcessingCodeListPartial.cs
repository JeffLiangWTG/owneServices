using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public partial class FWSProcessingCodeList
	{
		public static bool IsEDS(ZString code) => code == FWSProcessingCodeList.Codes.EDS;
		public static bool IsLDS(ZString code) => code == FWSProcessingCodeList.Codes.LDS;
	}
}
