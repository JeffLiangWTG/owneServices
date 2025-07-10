using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusReference))]
	sealed class CusReferenceBaseOnlyTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetLookups()
		{
			AssertType<CusReferenceLookups>(Factory.New<CusReference>().Lookups);
		}

		public void TestGetValidation()
		{
			AssertType<CusReferenceValidation>(Factory.New<CusReference>().Validation);
		}

		protected override BusinessObject GetNewBusinessObject() => GetBusinessObjectForTest(Factory);
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetBusinessObjectForTest(Factory);
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetBusinessObjectForTest(factory);

		CusReference GetBusinessObjectForTest(BusinessObjectFactory factory)
		{
			var header = factory.New<CusInBondHeaderForTesting>();
			var cusReference = factory.New<CusReference>();
			cusReference.CFR_ParentTableCode = header.TablePrefix;
			cusReference.CFR_ParentID = header.PK;
			cusReference.CFR_Type = "NCT";
			cusReference.CFR_Code = "AUT";
			cusReference.CFR_Reference = "X";
			return cusReference;
		}
	}

	[TestsSubclassesOf(typeof(CusReference))]
	public abstract class CusReferenceAbstractTest<T> : EnterpriseBusinessObjectTestCase where T : CusReference
	{
		public void TestCorrectlyTypeDecided()
		{
			CombineAssertions(() =>
			{
				var factory = new BusinessObjectFactory();
				var bizObjs = GetBizObjsForCorrectlyTypeDecideTest(factory).ToArray();
				factory.Save();
				var parentsNotSetupCorrectly = new List<string>();
				foreach (var bizObj in bizObjs)
				{
					var parent = bizObj.Parent;
					var parentType = parent?.GetType().FullName ?? "Unknown";
					Assert($"{bizObj.GetType().FullName} (Parent: {parentType}) was not saved", bizObj.IsInDatabase);
					Assert($"{bizObj.GetType().FullName} (Parent: {parentType}) was deleted", !bizObj.IsDeleted);

					var newFactory = new BusinessObjectFactory();
					LoadParentIfNeeded(newFactory, bizObj);
					CusReference bizObjInDiffFactory = null;
					AssertNoExceptionThrown($"Loading {bizObj.GetType().FullName} (Parent: {parentType})", () => bizObjInDiffFactory = newFactory.Load<CusReference>(bizObj.PK));
					AssertEquals($"Loading {bizObj.GetType().FullName} (Parent: {parentType})", typeof(T), bizObjInDiffFactory?.GetType());
				}
			});
		}

		protected virtual IEnumerable<T> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (T)GetNewBusinessObjectForDeleteTest(factory);
		}

		protected virtual void LoadParentIfNeeded(BusinessObjectFactory factory, T bizObj)
		{
		}
	}
}
