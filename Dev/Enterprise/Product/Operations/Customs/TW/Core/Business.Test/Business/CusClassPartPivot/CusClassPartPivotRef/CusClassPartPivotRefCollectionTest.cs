using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	abstract class CusClassPartPivotRefCollectionTest<T> : BusinessObjectCollectionTestCase where T : CusClassPartPivotRef
	{
		[ExpectNoExceptions]
		public void TestSetDefaultValuesForNewChild()
		{
			var coll = GetCusClassPartPivotRefCollection();
			var element = coll.AddNew();
			NUnit.Framework.Assert.That(element.CIR_ReferenceType, NUnit.Framework.Is.EqualTo(coll.CIR_ReferenceType));
			NUnit.Framework.Assert.That(element.CusClassPartPivot.PK, NUnit.Framework.Is.EqualTo(coll.Master.PK));
		}

		[ExpectNoExceptions]
		public void TestCreateRelationshipFilter()
		{
			var coll = GetCusClassPartPivotRefCollection();
			coll.RemoveAll();
			var element1 = (CusClassPartPivotRef)GetNewElementToAddToTheCollection();
			var element2 = (CusClassPartPivotRef)GetNewElementToAddToTheCollection();
			coll.Add(element1);
			coll.Add(element2);
			element2.CIR_ReferenceType = "~~~";
			NUnit.Framework.Assert.That(coll.CIR_ReferenceType, NUnit.Framework.Is.Not.EqualTo(element2.CIR_ReferenceType), "PreCondition:JG_ReferenceType passed into the collection is not the same as element2's");
			coll.Load();
			NUnit.Framework.Assert.That(coll.Contains(element1), NUnit.Framework.Is.EqualTo(true), "contains element1");
			NUnit.Framework.Assert.That(coll.Contains(element2), NUnit.Framework.Is.EqualTo(false), "should not contain element2");
		}

		protected abstract CusClassPartPivotRefCollection<T> GetCusClassPartPivotRefCollection();
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return GetCusClassPartPivotRefCollection();
		}
	}
}
