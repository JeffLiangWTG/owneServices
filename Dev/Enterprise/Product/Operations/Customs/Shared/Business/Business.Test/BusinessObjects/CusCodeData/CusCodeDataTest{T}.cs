using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business.Testing
{
	[TestsSubclassesOf(typeof(CusCodeData))]
	public abstract class CusCodeDataTest<T> : EnterpriseBusinessObjectTestCase
		where T : CusCodeData
	{
		public void TestCorrectlyTypeDecided()
		{
			CombineAssertions(() =>
			{
				var factory = new BusinessObjectFactory();

				var bizObjs = GetBizObjsForCorrectlyTypeDecideTest(factory).ToArray();
				var supportedTypes = bizObjs[0].parentLoaders.GetSupportedTypes().ToList();
				supportedTypes.Remove(typeof(MultiLineAddInfos.CusAddInfo));
				factory.Save();
				var iCusCodeDataTypeSupportertype = typeof(ICusCodeDataTypeSupporter);
				var parentsMissingImplementation = new ZStringBuilder();
				var supporterFetchStrategyType = typeof(FetchStrategies.CusCodeDataTypeSupporterFetchStrategy);
				var parentsNotSetupCorrectly = new List<string>();
				foreach (var bizObj in bizObjs)
				{
					string parentTypeName = "Unknown";
					var parent = bizObj.Parent;
					if (parent != null)
					{
						var parentType = parent.GetType();
						parentTypeName = parentType.FullName;
						if (!supportedTypes.Contains(parentType))
						{
							supportedTypes.Add(parentType);
						}
						var fetchStrategy = (parent as IAdditionalBusinessObjectFetchStrategyProvider)?.GetFetchStrategies().FirstOrDefault(x => supporterFetchStrategyType.IsAssignableFrom(x.GetType()));
						if (fetchStrategy == null && !parentsNotSetupCorrectly.Contains(parentTypeName))
						{
							parentsNotSetupCorrectly.Add(parentTypeName);
						}
					}

					Assert($"{bizObj.GetType().FullName} (Parent: {parentTypeName}) was deleted", !bizObj.IsDeleted);
					var newFactory = new BusinessObjectFactory();
					LoadParentIfNeeded(newFactory, bizObj);
					CusCodeData bizObjInDiffFactory = null;
					AssertNoExceptionThrown($"Loading {bizObj.GetType().FullName} (Parent: {parentTypeName})", () => bizObjInDiffFactory = newFactory.Load<CusCodeData>(bizObj.PK));
					AssertEquals(typeof(T), bizObjInDiffFactory.GetType());
				}
				foreach (var supportedType in supportedTypes.Where(x => !iCusCodeDataTypeSupportertype.IsAssignableFrom(x)))
				{
					parentsMissingImplementation.Append(supportedType.FullName);
				}
				if (!parentsMissingImplementation.IsEmpty)
				{
					Fail(string.Format("The following class needs to implement {0}:\r\n{1}", iCusCodeDataTypeSupportertype.FullName, parentsMissingImplementation.ToStringWithNewLineBetweenAppends()));
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

		protected virtual void LoadParentIfNeeded(BusinessObjectFactory factory, T bizObj)
		{
		}
	}
}
