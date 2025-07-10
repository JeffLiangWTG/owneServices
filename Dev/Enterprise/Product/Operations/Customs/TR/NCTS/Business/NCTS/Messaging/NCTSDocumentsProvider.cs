using CargoWise.Common;
using CargoWise.Customs.TR.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Business;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NCTSDocumentsProvider : INCTSSpecialMentionsDocuments
	{
		public NCTSDocumentsProvider(CusSupportingInfo cusSupportingInfo)
		{
			this.cusSupportingInfo = Argument.NotNull(cusSupportingInfo, nameof(CusSupportingInfo));
		}
		protected readonly CusSupportingInfo cusSupportingInfo;

		public string Type => cusSupportingInfo.CSI_Code;
		public string ReferenceNumber => cusSupportingInfo.CSI_ReferenceNumber;
		public string ReferenceNumberLNG => TRMessageConstants.LanguageCode;
		public string ComplementofInformation => cusSupportingInfo.CSI_Description;
		public string CountryCode => cusSupportingInfo.CSI_RN_NKCountryCode;
	}
}
