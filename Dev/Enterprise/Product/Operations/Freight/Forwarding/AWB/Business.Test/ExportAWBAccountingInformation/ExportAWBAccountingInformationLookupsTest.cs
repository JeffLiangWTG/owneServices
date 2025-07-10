using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	public class ExportAWBAccountingInformationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAccountingCodes()
		{
			ExportAWBAccountingInformation accountingInformation = Factory.New<ExportAWBAccountingInformation>();
			AssertEquals(OLookUpEditType.AWBAccountingCodes, accountingInformation.Lookups.AccountingCodes.LookupEditType);
		}
	}
}
