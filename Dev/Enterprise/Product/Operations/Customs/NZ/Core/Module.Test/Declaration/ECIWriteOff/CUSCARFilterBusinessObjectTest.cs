using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Module.Declaration.ECIWriteOff.Testing
{
	[TestedType(typeof(CUSCARFilterBusinessObject))]
	sealed class CUSCARFilterBusinessObjectTest : Customs.Module.Testing.JobDeclarationFilterBusinessObjectTest
	{
		public void TestLookups()
		{
			CUSCARFilterBusinessObject filterBizObj = new CUSCARFilterBusinessObject();
			AssertEquals("Lookups of the correct type", typeof(ECIWriteOffFilterLookups), filterBizObj.Lookups.GetType());
		}

		public override void TestEntryStatusForIntegratedCountry()
		{
			var declaration0 = Factory.New<BaseJobDeclaration>();
			declaration0.JE_EntryStatus = "SUB";

			var declaration1 = Factory.New<BaseJobDeclaration>();
			declaration1.JE_EntryStatus = "SUB";
			var entryHeader0 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader0.CH_EntryStatus = "DUT";

			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_EntryStatus = "DUT";
			var entryHeader1 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_EntryStatus = "DUT";
			var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_EntryStatus = "SUB";

			var declaration3 = Factory.New<BaseJobDeclaration>();
			declaration3.JE_EntryStatus = "SUB";
			var entryHeader3 = declaration3.CustomsEntryHeaders.AddNew();
			entryHeader3.CH_EntryStatus = "DUT";
			var entryHeader4 = declaration3.CustomsEntryHeaders.AddNew();
			entryHeader4.CH_EntryStatus = "CSN";

			var declaration4 = Factory.New<BaseJobDeclaration>();
			var entryHeader5 = declaration4.CustomsEntryHeaders.AddNew();
			entryHeader5.CH_EntryStatus = ZString.Empty;
			var entryHeader6 = declaration4.CustomsEntryHeaders.AddNew();
			entryHeader6.CH_EntryStatus = ZString.Empty;
			declaration4.JE_EntryStatus = "SUB";

			Factory.Save();
			AssertEquals("SUB", declaration4.JE_EntryStatus);

			var filter = (EntryStatusFilter)filterBO[Customs.Module.DeclarationFilterConstants.EntryStatusText];
			filter.IsActive = true;
			filter.Property = "SUB";
			var filteredDecs = Factory.Load<BaseJobDeclaration>(filterBO.Filter);

			CombineAssertions("Entry Header Statuses have no effect on the Declaration Entry Status Filter in NZ", () =>
			{
				AssertEquals("declaration0", declaration0.PK, filteredDecs[0].PK);
				AssertEquals("declaration1", declaration1.PK, filteredDecs[1].PK);
				AssertEquals("declaration3", declaration3.PK, filteredDecs[2].PK);
				AssertEquals("declaration4", declaration4.PK, filteredDecs[3].PK);
			});

			filter.Property = "DUT";
			filteredDecs = Factory.Load<BaseJobDeclaration>(filterBO.Filter);
			AssertEquals("declaration2", declaration2.PK, filteredDecs[0].PK);

			filter.Property = "CSN";
			filteredDecs = Factory.Load<BaseJobDeclaration>(filterBO.Filter);
			AssertEquals(0, filteredDecs.Length);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			AssertEquals("ComparisonOperator should have been reset.", filter.ComparisonOperator, ModuleTextFilter.ComparisonConstants.Exact);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			AssertEquals("ComparisonOperator should have been changed.", filter.ComparisonOperator, ModuleTextFilter.ComparisonConstants.NotEqual);
		}
	}
}
