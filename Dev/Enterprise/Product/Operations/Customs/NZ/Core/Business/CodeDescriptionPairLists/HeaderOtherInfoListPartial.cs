namespace Enterprise.Customs.NZ.Business
{
	public partial class HeaderOtherInfoList
	{
		public static bool IsReferenceDocument(string code)
		{
			return code == HeaderOtherInfoList.Codes.Passport
				|| code == HeaderOtherInfoList.Codes.Certificate
				|| code == HeaderOtherInfoList.Codes.OtherDocument;
		}

		public static bool IsRequiredOtherInfoCode(string code)
		{
			return code != HeaderOtherInfoList.Codes.MAFContainerDeclaration
				&& code != HeaderOtherInfoList.Codes.ApprovedTransitionalFacility
				&& code != HeaderOtherInfoList.Codes.Passport
				&& code != HeaderOtherInfoList.Codes.Certificate
				&& code != HeaderOtherInfoList.Codes.OtherDocument;
		}
	}
}
