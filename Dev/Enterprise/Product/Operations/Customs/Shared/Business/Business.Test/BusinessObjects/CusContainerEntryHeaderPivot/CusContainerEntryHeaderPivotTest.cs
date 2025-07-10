using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusContainerEntryHeaderPivot))]
	sealed class CusContainerEntryHeaderPivotTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryheader = declaration.ActiveEntryHeaders.AddNew();
			var container = declaration.CusContainers.AddNew();

			var pivot = Factory.New<CusContainerEntryHeaderPivot>();
			pivot.CCE_CH_EntryHeader = entryheader.PK;
			pivot.CCE_CO_Container = container.PK;

			return pivot;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		public void TestRelatedContainerProperty()
		{
			var container = Factory.New<BaseCusContainer>();

			var pivot = Factory.New<CusContainerEntryHeaderPivot>();
			pivot.CCE_CO_Container = container.PK;
			AssertEquals("Container should be linked to CusContainerEntryHeaderPivot", pivot.CCE_CO_Container, pivot.Container.PK);
		}

		public void TestRelatedEntryHeaderProperty()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "11";

			var pivot = Factory.New<CusContainerEntryHeaderPivot>();
			pivot.CCE_CH_EntryHeader = entryHeader.PK;
			AssertEquals("EntryInstruction should be linked to CusContainerEntryHeaderPivot", pivot.CCE_CH_EntryHeader, pivot.EntryHeader.PK);
		}
	}
}
