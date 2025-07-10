using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWConsignorOrConsigneeAddressLookups))]
	sealed class TWConsignorOrConsigneeAddressLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestGovRegNumTypes()
		{
			var declaration = Factory.New<JobDeclaration>();
			var consigneeDocumentaryAddress = declaration.ConsigneeDocumentaryAddress;
			var addressLookups = consigneeDocumentaryAddress.Lookups;
			AssertEquals("GovRegNumTypes", "VAT, PID, PAS, FFF", addressLookups.GovRegNumTypes.CodesAsString);
		}
	}
}
