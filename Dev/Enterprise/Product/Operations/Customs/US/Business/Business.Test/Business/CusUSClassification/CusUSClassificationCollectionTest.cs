using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusUSClassificationCollection))]
	sealed class CusUSClassificationCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestParentIDAndParentTableCode()
		{
			var collectin = new CusUSClassificationCollection(Pivot);
			var item = collectin.AddNew();
			AssertEquals(CusClassPartPivotSchema.Constants.Prefix, item.CD_ParentTableCode);
			AssertEquals(Pivot.PK, item.CD_ParentID);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new CusUSClassificationCollection(Pivot);

		CusClassPartPivot pivot;
		CusClassPartPivot Pivot
		{
			get
			{
				if (pivot == null)
				{
					var classification = Factory.New<CusClassification>();
					var product = Factory.New<OrgSupplierPart>();
					product.OP_PartNum = product.PK.ToString().Replace("-", "");
					pivot = Factory.New<CusClassPartPivot>();
					pivot.CI_CC = classification.PK;
					pivot.CI_OP = product.PK;
				}

				return pivot;
			}
		}
	}
}
