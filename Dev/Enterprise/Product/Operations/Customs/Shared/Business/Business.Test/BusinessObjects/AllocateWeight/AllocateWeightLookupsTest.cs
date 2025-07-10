using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.Testing
{
	sealed class AllocateWeightLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestParent()
		{
			AssertEquals(typeof(AllocateWeight), Lookups.Parent.GetType());
		}

		public void TestWeightUQList()
		{
			var weightUnits = Lookups.WeightUQList;
			var list = Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);

			AssertSame(list, weightUnits);
			AssertSame(declaration.Lookups.WeightUnitList, weightUnits);
			Assert("Weight Units", weightUnits.ContainsCode(Core.Constants.Weight.Kilograms));
		}

		public void TestAllocateWeightMethodList()
		{
			var methodList = Lookups.AllocateWeightMethodList;
			CombineAssertions(() =>
			{
				AssertSame("List cached", Factory.GetCachedValue<AllocateWeightMethodList>(), methodList);
				AssertEquals("Codes as string", "PRI, QTY", methodList.CodesAsString);
			});
		}

		AllocateWeightLookups Lookups => allocateWeight.Lookups;

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();

			allocateWeight = new AllocateWeight(declaration.Factory, invoiceHeader.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>());
		}

		BaseJobDeclaration declaration;
		AllocateWeight allocateWeight;
		#endregion
	}
}
