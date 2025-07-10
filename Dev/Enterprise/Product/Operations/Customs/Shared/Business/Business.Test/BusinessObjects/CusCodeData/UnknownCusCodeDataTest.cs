using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(UnknownCusCodeData))]
	sealed class UnknownCusCodeDataTest : CusCodeDataTest<UnknownCusCodeData>
	{
		protected override void LoadParentIfNeeded(BusinessObjectFactory factory, UnknownCusCodeData bizObj)
		{
			factory.Load<JobDeclarationWithCusCodeDataTypeSupporter>(bizObj.CY_ParentID);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = Factory.New<JobDeclarationWithCusCodeDataTypeSupporter>();
			var result = Factory.New<UnknownCusCodeData>();
			result.CY_ParentID = declaration.PK;
			result.CY_ParentTableCode = declaration.TablePrefix;
			result.CY_Type = JobDeclarationWithCusCodeDataTypeSupporter.UnknownCode;
			return result;
		}

		protected override IEnumerable<UnknownCusCodeData> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclarationWithCusCodeDataTypeSupporter>();
			var result = factory.New<UnknownCusCodeData>();
			result.CY_ParentID = declaration.PK;
			result.CY_ParentTableCode = declaration.TablePrefix;
			result.CY_Type = JobDeclarationWithCusCodeDataTypeSupporter.UnknownCode;
			yield return result;
		}

		internal class JobDeclarationWithCusCodeDataTypeSupporter : BaseJobDeclaration, ICusCodeDataTypeSupporter
		{
			public JobDeclarationWithCusCodeDataTypeSupporter(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
			{
				yield return new FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
			}

			IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
			{
				var result = new Dictionary<ZString, Type>();
				result.Add(DummyCusCodeData.DummyType, typeof(DummyCusCodeData));
				result.Add(UnknownCode, typeof(UnknownCusCodeData));
				return result;
			}
			internal const string UnknownCode = "%^&";
		}
	}
}
