using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.MultiLineAddInfos.Testing
{
	[TestsSubclassesOf(typeof(CusAddInfo))]
	public abstract class CusAddInfoTest<T> : EnterpriseBusinessObjectTestCase
		where T : CusAddInfo
	{
		public void TestCorrectlyTypeDecided()
		{
			CombineAssertions(() =>
			{
				var factory = new BusinessObjectFactory();
				var bizObjs = GetBizObjsForCorrectlyTypeDecideTest(factory).ToArray();
				factory.Save();
				var supporterFetchStrategyType = typeof(FetchStrategies.CusAddInfoTypeSupporterFetchStrategy);
				var parentsNotSetupCorrectly = new List<string>();
				foreach (var bizObj in bizObjs)
				{
					string parentType = "Unknown";
					var parent = bizObj.Parent;
					if (parent != null)
					{
						parentType = parent.GetType().FullName;
						var fetchStrategy = (parent as IAdditionalBusinessObjectFetchStrategyProvider)?.GetFetchStrategies().FirstOrDefault(x => supporterFetchStrategyType.IsAssignableFrom(x.GetType()));
						if (fetchStrategy == null && !parentsNotSetupCorrectly.Contains(parentType))
						{
							parentsNotSetupCorrectly.Add(parentType);
						}
					}
					Assert($"{bizObj.GetType().FullName} (Parent: {parentType}) was deleted", !bizObj.IsDeleted);
					var newFactory = new BusinessObjectFactory();
					LoadParentIfNeeded(newFactory, bizObj);
					CusAddInfo bizObjInDiffFactory = null;
					AssertNoExceptionThrown($"Loading {bizObj.GetType().FullName} (Parent: {parentType})", () => bizObjInDiffFactory = newFactory.Load<CusAddInfo>(bizObj.PK));
					AssertEquals(typeof(T), bizObjInDiffFactory.GetType());
				}
				if (parentsNotSetupCorrectly.Count > 0)
				{
					Fail($"The following BizObjs needs to implement {typeof(IAdditionalBusinessObjectFetchStrategyProvider).FullName} and return either {supporterFetchStrategyType.FullName} or a subclass of it.\r\n{new ZStringBuilder(parentsNotSetupCorrectly).ToStringWithNewLineBetweenAppends()}");
				}
			});
		}

		protected virtual IEnumerable<T> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (T)GetNewBusinessObjectForDeleteTest(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var bizObj = (T)base.GetNewBusinessObjectForDeleteTest(factory);
			bizObj.B7_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			return bizObj;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var bizObj = (T)base.GetNewBusinessObject();
			bizObj.B7_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			return bizObj;
		}

		protected virtual void LoadParentIfNeeded(BusinessObjectFactory factory, T bizObj)
		{
		}
	}
}
