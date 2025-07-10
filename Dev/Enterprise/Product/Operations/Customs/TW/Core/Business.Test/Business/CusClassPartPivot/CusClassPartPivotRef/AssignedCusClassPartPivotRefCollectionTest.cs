using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(AssignedCusClassPartPivotRefCollection))]
	sealed class AssignedCusClassPartPivotRefCollectionTest : CusClassPartPivotRefCollectionTest<AssignedCusClassPartPivotRef>
	{
		protected override CusClassPartPivotRefCollection<AssignedCusClassPartPivotRef> GetCusClassPartPivotRefCollection()
		{
			var pivot = Factory.New<OrgSupplierPart>().PivotsForBinding.AddNew();
			return new AssignedCusClassPartPivotRefCollection(pivot);
		}
	}
}
