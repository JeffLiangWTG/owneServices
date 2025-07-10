using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USNHTSAPermitAndLicenseAddInfo))]
	public class USNHTSAPermitAndLicenseAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPermitAndLicenses()
		{
			var permitAndLicenses = Factory.New<NHTSAPermitAndLicenses>();
			var addInfo = new USNHTSAPermitAndLicenseAddInfo(permitAndLicenses.B7_AddInfoDataInfo);
			AssertEquals(permitAndLicenses.PK, addInfo.PermitAndLicenses.PK);
		}

		public void TestHumanReadableNames()
		{
			var permitAndLicense = Factory.New<NHTSAPermitAndLicenses>();
			AssertEquals("LPCO Type", permitAndLicense.US_NHTLPCOTypeInfo.HumanReadableName);
			AssertEquals("LPCO Number", permitAndLicense.US_NHTLPCONumberInfo.HumanReadableName);
			AssertEquals("LPCO Date Type", permitAndLicense.US_NHTLPCODateTypeInfo.HumanReadableName);
			AssertEquals("LPCO Date", permitAndLicense.US_NHTLPCODateInfo.HumanReadableName);
			AssertEquals("LPCO Quantity", permitAndLicense.US_NHTLPCOQuantityInfo.HumanReadableName);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var permitAndLicenses = Factory.New<NHTSAPermitAndLicenses>();
			var addInfo = new USNHTSAPermitAndLicenseAddInfo(permitAndLicenses.B7_AddInfoDataInfo);
			return addInfo;
		}

		#endregion
	}
}
