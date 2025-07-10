using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;
using static Enterprise.Customs.Business.MultiLineAddInfos.Testing.UnknownCusAddInfoTest;

namespace Enterprise.Customs.Business.MultiLineAddInfos.Testing
{
	[TestedType(typeof(CusAddInfoWithAutoDelete<AddInfoWithTypeCode>))]
	sealed class CusAddInfoWithAutoDeleteGenericTest : CusAddInfoTest<CusAddInfoWithAutoDelete<AddInfoWithTypeCode>>
	{
		public void TestClone()
		{
			CusAddInfoWithAutoDelete<AddInfoWithTypeCode> info = Factory.New<CusAddInfoWithAutoDelete<AddInfoWithTypeCode>>();
			info.B7_ParentID = ZGuid.NewZGuid();
			info.B7_ParentTableCode = "ZZ";
			info.Data.UZ_String = "Fred";

			CusAddInfo<AddInfoWithTypeCode> clonedInfo = info.Clone();
			AssertNotEquals("Cloned BO PK should be different", info.PK, clonedInfo.PK);
			AssertEquals("clonedInfo.B7_ParentID", ZGuid.Empty, clonedInfo.B7_ParentID);
			AssertEquals("clonedInfo.B7_ParentTableCode", ZString.Empty, clonedInfo.B7_ParentTableCode);
			AssertEquals("clonedInfo.Data.UZ_String", "Fred", clonedInfo.Data.UZ_String);
		}

		public void TestDeleteEmpty()
		{
			var sql = @"
IF (OBJECT_ID('Constraint_B7_Type') IS NOT NULL)
BEGIN
    ALTER TABLE dbo.CusAddInfo NOCHECK CONSTRAINT Constraint_B7_Type
END";
			TestConnection.ExecuteNonQuery(sql);

			CusAddInfoWithAutoDelete<AddInfoWithTypeCode> info = Factory.New<CusAddInfoWithAutoDelete<AddInfoWithTypeCode>>();
			info.B7_ParentID = ZGuid.NewZGuid();
			info.B7_ParentTableCode = "JE";
			info.Data.UZ_String = "Fred";
			Factory.Save();
			AssertEquals(false, info.IsDeleted);
			info.Data.UZ_String = ZString.Empty;
			Factory.Save();
			AssertEquals(true, info.IsDeleted);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var sql = @"
IF (OBJECT_ID('Constraint_B7_Type') IS NOT NULL)
BEGIN
    ALTER TABLE dbo.CusAddInfo NOCHECK CONSTRAINT Constraint_B7_Type
END";
			TestConnection.ExecuteNonQuery(sql);

			CusAddInfoWithAutoDelete<AddInfoWithTypeCode> info = (CusAddInfoWithAutoDelete<AddInfoWithTypeCode>)base.GetNewBusinessObjectForDeleteTest(factory);
			var declaration = factory.New<JobDeclarationWithCusAddInfoTypeSupporter>();
			var result = factory.New<CusAddInfoWithAutoDelete<AddInfoWithTypeCode>>();
			result.B7_ParentID = declaration.PK;
			result.B7_ParentTableCode = declaration.TablePrefix;
			result.B7_Type = JobDeclarationWithCusAddInfoTypeSupporter.AutoDeleteCode;
			result.Data.UZ_String = "FRED";
			return result;
		}

		protected override void LoadParentIfNeeded(BusinessObjectFactory factory, CusAddInfoWithAutoDelete<AddInfoWithTypeCode> bizObj)
		{
			factory.Load<JobDeclarationWithCusAddInfoTypeSupporter>(bizObj.B7_ParentID);
		}
	}
}
