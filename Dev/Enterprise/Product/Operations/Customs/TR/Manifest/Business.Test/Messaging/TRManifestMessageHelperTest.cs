using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	public class TRManifestMessageHelperTest : BusinessObjectValidationTestCase
	{
		public void TestCheckFields()
		{
			bool result;
			result = TRManifestMessageHelper.IsForbidden(ManifestBase.AutoAsycudaManifestHeader.Schema.AMA_Trailer1RegNo, TRManifestTypes.Codes.DENITH);
			AssertEquals(false, result);
			result = TRManifestMessageHelper.IsForbidden(ManifestBase.AutoAsycudaManifestHeader.Schema.AMA_Trailer1RegNo, TRManifestTypes.Codes.GRUPAJ);
			AssertEquals(true, result);
			result = TRManifestMessageHelper.IsForbidden(ManifestBase.AutoAsycudaManifestHeader.Schema.AMA_VesselName, TRManifestTypes.Codes.DENITH);
			AssertEquals(false, result);
			result = TRManifestMessageHelper.IsForbidden(ManifestBase.AutoAsycudaBill.Schema.ABL_TransportValue, TRManifestTypes.Codes.TESLIM);
			AssertEquals(false, result);
			result = TRManifestMessageHelper.IsForbidden(ManifestBase.AutoAsycudaBill.Schema.ABL_RX_NKTransportValueCurrency, TRManifestTypes.Codes.TESLIM);
			AssertEquals(false, result);
			result = TRManifestMessageHelper.IsForbidden(ManifestBase.AutoAsycudaBill.Schema.ABL_TransportValue, TRManifestTypes.Codes.GRUPAJ);
			AssertEquals(true, result);
			result = TRManifestMessageHelper.IsForbidden(ManifestBase.AutoAsycudaBill.Schema.ABL_RX_NKTransportValueCurrency, TRManifestTypes.Codes.GRUPAJ);
			AssertEquals(true, result);
		}

		public void TestTRManifestType()
		{
			string result;
			result = TRManifestMessageHelper.TRManifestType(TRManifestTypes.Codes.CIKONC);
			AssertEquals("CIKONC", result);
			result = TRManifestMessageHelper.TRManifestType(TRManifestTypes.Codes.DENITH);
			AssertEquals("DENİTH", result);
			result = TRManifestMessageHelper.TRManifestType(TRManifestTypes.Codes.HAVITH);
			AssertEquals("HAVİTH", result);
		}
	}
}
