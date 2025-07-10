using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	[TestedType(typeof(ExportAWBAccountingInformation))]
	sealed class ExportAWBAccountingInformationTest : EnterpriseBusinessObjectTestCaseWithListChecking<ExportAWBAccountingInformation>
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<ExportAWBHeader>();
			var result = factory.NewWithValidTestData<ExportAWBAccountingInformation>();
			result.EA_EH = header.PK;

			return result;
		}
	}
}
