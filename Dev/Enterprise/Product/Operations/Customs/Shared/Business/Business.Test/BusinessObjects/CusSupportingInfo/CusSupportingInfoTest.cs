using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestsSubclassesOf(typeof(CusSupportingInfo))]
	public abstract class CusSupportingInfoTest<T> : EnterpriseBusinessObjectTestCase
			where T : CusSupportingInfo
	{
		public void TestCorrectlyTypeDecided()
		{
			CombineAssertions(() =>
			{
				var factory = new BusinessObjectFactory();
				var bizObjs = GetBizObjsForCorrectlyTypeDecideTest(factory).ToArray();
				factory.Save();
				var supporterFetchStrategyType = typeof(FetchStrategies.CusSupportingInfoTypeSupporterFetchStrategy);
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
					CusSupportingInfo bizObjInDiffFactory = null;
					AssertNoExceptionThrown($"Loading {bizObj.GetType().FullName} (Parent: {parentType})", () => bizObjInDiffFactory = newFactory.Load<CusSupportingInfo>(bizObj.PK));
					AssertEquals($"Loading {bizObj.GetType().FullName} (Parent: {parentType})", typeof(T), bizObjInDiffFactory.GetType());
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

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = base.GetNewBusinessObjectForDeleteTest(factory) as CusSupportingInfo;
			result.CSI_Type = Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo;
			result.CSI_ParentTableCode = ZArchitecture.Schema.JobComInvoiceHeaderSchema.Constants.Prefix;
			result.CSI_DataModel = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			return result;
		}
	}
}
