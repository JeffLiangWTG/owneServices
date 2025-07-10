using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSupplierPartCollectionBOM))]
	public class OrgSupplierPartCollectionBOMTest : OrgSupplierPartCollectionTest
	{
		#region TestAdditionalFilterConstrainsByBOMComponents

		public void TestAdditionalFilterConstrainsByBOMComponents()
		{
			OrgSupplierPart nonBomPart = Factory.NewWithValidTestData<OrgSupplierPart>();
			nonBomPart.OP_PartNum = "nonBomPart";
			OrgSupplierPart bomPart = Factory.NewWithValidTestData<OrgSupplierPart>();
			bomPart.OP_PartNum = "bomPart";
			bomPart.BillOfMaterials.AddNew().FillWithValidTestData();
			Factory.Save();

			Collection.Load();
			AssertCollectionContains(bomPart, Collection);
			AssertCollectionNotContains(nonBomPart, Collection);
		}

		#endregion

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgSupplierPartCollectionBOM(Factory);
		}
	}
}
