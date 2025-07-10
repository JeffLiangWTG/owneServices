using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.MultiLineAddInfos;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USLinkedEntryAddInfo))]
	sealed class USLinkedEntryAddInfoTest : PersistentBusinessObjectTestCase
	{
		public void TestLookupsAndValidation()
		{
			var protest = new Protest.Protest(Factory.New<JobDeclaration>());
			var linkedEntryAddInfo = protest.LinkedEntries.AddNew().Data;
			AssertEquals("USLinkedEntryAddInfo Lookups", typeof(USLinkedEntryAddInfoLookups), linkedEntryAddInfo.Lookups.GetType());
			AssertEquals("USLinkedEntryAddInfo Validation", typeof(USLinkedEntryAddInfoValidation), linkedEntryAddInfo.Validation.GetType());
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBizObj(Factory).Data;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBizObj(factory);

		CusAddInfo<USLinkedEntryAddInfo> GetNewBizObj(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.LinkedEntryNumbers.AddNew();
			return declaration.LinkedEntryNumbers[0];
		}
	}
}
