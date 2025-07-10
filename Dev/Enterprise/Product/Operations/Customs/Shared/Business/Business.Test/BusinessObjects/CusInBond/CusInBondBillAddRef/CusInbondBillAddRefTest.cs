using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	class CusInbondBillAddRefBaseOnlyTest : TestCase
	{
		public void TestTypeDecider()
		{
			var typeDecider = CusInbondBillAddRef.TypeDecider;
			AssertEquals(typeof(CusInbondBillAddRefTypeDecider), typeDecider.GetType());
		}
	}

	[TestsSubclassesOf(typeof(CusInbondBillAddRef))]
	public abstract class CusInbondBillAddRefTest<T> : EnterpriseBusinessObjectTestCase where T : CusInbondBillAddRef
	{
		public void TestCorrectlyTypeDecided()
		{
			CombineAssertions(() =>
			{
				var factory = new BusinessObjectFactory();
				var bizObj = (T)GetNewBusinessObjectForDeleteTest(factory);
				factory.Save();
				AssertEquals("Something is not right, data was not saved correctly. Please check that the correct factory was used", true, bizObj.IsInDatabase);
				var newFactory = new BusinessObjectFactory();
				var expectedType = typeof(T);
				var bizObjFullName = bizObj.GetType().FullName;
				foreach (var baseType in GetBaseTypes().Union(new[] { expectedType }))
				{
					CusInbondBillAddRef bizObjInDiffFactory = null;
					var baseTypeFullName = baseType.FullName;
					AssertNoExceptionThrown($"Loading {bizObjFullName} using type {baseTypeFullName}", () => bizObjInDiffFactory = (CusInbondBillAddRef)newFactory.Load(baseType, bizObj.PK));
					AssertEquals(baseTypeFullName, expectedType, bizObjInDiffFactory.GetType());
				}
			});
		}

		protected virtual IEnumerable<Type> GetBaseTypes()
		{
			yield return typeof(CusInbondBillAddRef);
		}
	}
}
