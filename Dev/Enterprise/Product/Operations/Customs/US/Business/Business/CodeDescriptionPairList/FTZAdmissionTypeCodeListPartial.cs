
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public partial class FTZAdmissionTypeCodeList :
		Integration.Customs.US.IFTZAdmissionTypeCodeDescriptionPairProvider,
		DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public static bool IsODZ_AdmissionType(string code)
		{
			return code == FTZAdmissionTypeCodeList.Codes.OverageAdmission
				|| code == FTZAdmissionTypeCodeList.Codes.Domestic
				|| code == FTZAdmissionTypeCodeList.Codes.ZoneToZone;
		}

		public static bool IsOC_AdmissionType(string code)
		{
			return code == FTZAdmissionTypeCodeList.Codes.OverageAdmission ||
				code == FTZAdmissionTypeCodeList.Codes.StatusChange;
		}

		#region ICodeDescriptionPairListProvider Members
		ReadOnlyCodeDescriptionPairList DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider.GetCodeDescriptionPairList()
		{
			return this;
		}
		#endregion
	}
}
