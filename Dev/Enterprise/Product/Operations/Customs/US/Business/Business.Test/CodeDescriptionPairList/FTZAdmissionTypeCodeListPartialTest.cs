namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FTZAdmissionTypeCodeListTest : NUnit.Framework.TestCase
	{
		public void TestBooleanMethods()
		{
			AssertEquals(true, FTZAdmissionTypeCodeList.IsODZ_AdmissionType(FTZAdmissionTypeCodeList.Codes.Domestic));
			AssertEquals(true, FTZAdmissionTypeCodeList.IsODZ_AdmissionType(FTZAdmissionTypeCodeList.Codes.OverageAdmission));
			AssertEquals(false, FTZAdmissionTypeCodeList.IsODZ_AdmissionType(FTZAdmissionTypeCodeList.Codes.RegularAdmission));
			AssertEquals(false, FTZAdmissionTypeCodeList.IsODZ_AdmissionType(FTZAdmissionTypeCodeList.Codes.StatusChange));
			AssertEquals(true, FTZAdmissionTypeCodeList.IsODZ_AdmissionType(FTZAdmissionTypeCodeList.Codes.ZoneToZone));

			AssertEquals(false, FTZAdmissionTypeCodeList.IsOC_AdmissionType(FTZAdmissionTypeCodeList.Codes.ZoneToZone));
			AssertEquals(true, FTZAdmissionTypeCodeList.IsOC_AdmissionType(FTZAdmissionTypeCodeList.Codes.OverageAdmission));
			AssertEquals(true, FTZAdmissionTypeCodeList.IsOC_AdmissionType(FTZAdmissionTypeCodeList.Codes.StatusChange));
		}
	}
}
