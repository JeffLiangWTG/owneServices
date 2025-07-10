using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class DISCertificateDataWrapperTest : TestCaseWithFactory
	{
		public void TestIDISCertificate()
		{
			var data = new DISCertificateData(Factory);
			data.CertificateNumber = "23897";
			data.CertificateNumber = "A5427890";
			data.CertificateType = "T";
			data.Statement = "S";
			data.IssueDate = ZDateTime.Today.AddDays(-28);
			data.ExpiryDate = ZDateTime.Today;
			data.InspectionLocation = "Location";
			data.GrossTonnage = 2m;
			data.NetTonnage = 1m;
			var iData = (IDISCertificate)new DISCertificateDataWrapper(data, "19-4239784-0");
			AssertEquals("A5427890", iData.Number);
			AssertEquals("19-4239784-0", iData.ImporterOfRecord);
			AssertEquals("T", iData.Type);
			AssertEquals("S", iData.Statement);
			AssertEquals(ZDateTime.Today.AddDays(-28), iData.IssueDate);
			AssertEquals(ZDateTime.Today, iData.ExpiryDate);
			AssertEquals("Location", iData.InspectionLocation);
			AssertEquals(2m, iData.GrossTonnage);
			AssertEquals(1m, iData.NetTonnage);
		}
	}
}
