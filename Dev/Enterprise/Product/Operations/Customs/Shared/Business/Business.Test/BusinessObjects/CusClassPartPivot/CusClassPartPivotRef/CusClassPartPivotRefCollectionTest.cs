using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusClassPartPivotRefCollection))]
	sealed class CusClassPartPivotRefCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var pivotRefs = Factory.New<CusClassPartPivotRef>();
			pivotRefs.CIR_CI = Pivot.PK;
			return pivotRefs;
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(CusClassPartPivotRefCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusClassPartPivotRefCollection(Pivot);
		}

		OrgSupplierPart Part
		{
			get { return part ?? (part = Factory.New<OrgSupplierPart>()); }
		}
		OrgSupplierPart part;

		BaseCusClassPartPivot Pivot
		{
			get { return pivot ?? (pivot = Part.PivotsForBinding.AddNew()); }
		}
		BaseCusClassPartPivot pivot;
	}
}
