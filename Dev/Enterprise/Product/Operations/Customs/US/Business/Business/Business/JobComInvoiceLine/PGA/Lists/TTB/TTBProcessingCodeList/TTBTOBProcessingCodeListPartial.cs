using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public partial class TTBTOBProcessingCodeList
	{
		public static bool IsQuantityRequired(ZString processingCode)
		{
			return processingCode == TTBTOBProcessingCodeList.Codes.T51 ||
				processingCode == TTBTOBProcessingCodeList.Codes.T52 ||
				processingCode == TTBTOBProcessingCodeList.Codes.T54 ||
				processingCode == TTBTOBProcessingCodeList.Codes.T55;
		}
	}
}
