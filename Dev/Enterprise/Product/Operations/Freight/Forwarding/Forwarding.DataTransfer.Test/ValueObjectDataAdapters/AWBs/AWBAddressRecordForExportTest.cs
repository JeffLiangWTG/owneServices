using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class AWBAddressRecordForExportTest : TestCase
	{
		public void TestProperties()
		{
			AWBAddressRecordForExport record = new AWBAddressRecordForExport("accountCode", "companyName", "addrline1", "addline2", "AU", "sydney", "NSW", "1234", "FX", "23223-2");
			AssertEquals("accountCode", record.AccountCode);
			AssertEquals("companyName", record.CompanyName);
			AssertEquals("addrline1", record.AddrLine1);
			AssertEquals("addline2", record.AddrLine2);
			AssertEquals("AU", record.CountryCode);
			AssertEquals("sydney", record.City);
			AssertEquals("NSW", record.State);
			AssertEquals("1234", record.PostCode);
			AssertEquals("FX", record.ContactType);
			AssertEquals("23223-2", record.ContactDetail);
		}
	}
}
