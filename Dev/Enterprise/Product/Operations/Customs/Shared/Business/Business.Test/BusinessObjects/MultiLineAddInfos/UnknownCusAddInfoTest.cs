using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business.MultiLineAddInfos.Testing
{
	[TestedType(typeof(UnknownCusAddInfo))]
	sealed class UnknownCusAddInfoTest : CusAddInfoTest<UnknownCusAddInfo>
	{
		protected override void LoadParentIfNeeded(BusinessObjectFactory factory, UnknownCusAddInfo bizObj)
		{
			factory.Load<JobDeclarationWithCusAddInfoTypeSupporter>(bizObj.B7_ParentID);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var sql = @"
IF (OBJECT_ID('Constraint_B7_Type') IS NOT NULL)
BEGIN
    ALTER TABLE dbo.CusAddInfo NOCHECK CONSTRAINT Constraint_B7_Type
END";
			TestConnection.ExecuteNonQuery(sql);

			var declaration = factory.New<JobDeclarationWithCusAddInfoTypeSupporter>();
			var result = factory.New<UnknownCusAddInfo>();
			result.B7_ParentID = declaration.PK;
			result.B7_ParentTableCode = declaration.TablePrefix;
			result.B7_Type = JobDeclarationWithCusAddInfoTypeSupporter.UnknownCode;
			return result;
		}

		internal class JobDeclarationWithCusAddInfoTypeSupporter : BaseJobDeclaration, ICusAddInfoTypeSupporter
		{
			public JobDeclarationWithCusAddInfoTypeSupporter(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
			{
				yield return new FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
			}

			IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
			{
				var result = new Dictionary<ZString, Type>();
				result.Add(CusAddInfoTypeAttribute.Codes.TypeCodeForTesting, typeof(CusAddInfo<AddInfoWithTypeCode>));
				result.Add(UnknownCode, typeof(UnknownCusAddInfo));
				result.Add(AutoDeleteCode, typeof(CusAddInfoWithAutoDelete<AddInfoWithTypeCode>));
				return result;
			}
			internal const string UnknownCode = "%^&";
			internal const string AutoDeleteCode = "*@#";
		}
	}
}
