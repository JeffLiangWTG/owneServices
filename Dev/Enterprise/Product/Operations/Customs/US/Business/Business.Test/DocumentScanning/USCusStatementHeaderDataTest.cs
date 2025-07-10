using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USCusStatementHeaderAssemblyDataTest : TestCase
	{
		public void TestOverrides()
		{
			var assemblyBizObj = new USCusStatementHeaderData();
			AssertEquals(typeof(Customs.Business.BaseCusStatementHeader), assemblyBizObj.BusinessObjectType);
			AssertEquals(typeof(CusStatementHeaderCollection), assemblyBizObj.GetBusinessObjectCollection(new BusinessObjectFactory()).GetType());
			AssertEquals(ModuleIDs.Customs.US.USCustomsStatement, assemblyBizObj.ModuleID);
			AssertEquals(Core.Constants.ReferenceTypes.SupplyChainLogistics, assemblyBizObj.ReferenceType);
			AssertEquals("Statement Header", assemblyBizObj.HumanReadableName.GetUnresolvedString());
			AssertEquals(true, assemblyBizObj.IsAllowedForUnallocatedeDocs);
		}
	}
}
