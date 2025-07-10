using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Business.MultiLineAddInfos.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.Business.Extensions.Testing
{
	sealed class AdditionalChildrenHelperTest : TestCaseWithFactory
	{
		public void TestGetCusAddInfoAndCusCodeDataChildrenIfSupported()
		{
			var sql = @"
IF (OBJECT_ID('Constraint_B7_Type') IS NOT NULL)
BEGIN
    ALTER TABLE dbo.CusAddInfo NOCHECK CONSTRAINT Constraint_B7_Type
END";
			TestConnection.ExecuteNonQuery(sql);

			BaseJobDeclaration declaration = Factory.New<UnknownCusAddInfoTest.JobDeclarationWithCusAddInfoTypeSupporter>();
			var addInfo1 = Factory.New<CusAddInfo<AddInfoWithTypeCode>>();
			addInfo1.B7_ParentID = declaration.PK;
			addInfo1.B7_ParentTableCode = declaration.TablePrefix;
			addInfo1.Data.UZ_String = "Fred1";
			var addInfo2 = Factory.New<CusAddInfo<AddInfoWithTypeCode>>();
			addInfo2.B7_Type = "#@$";
			addInfo2.B7_ParentID = declaration.PK;
			addInfo2.B7_ParentTableCode = declaration.TablePrefix;
			addInfo2.Data.UZ_String = "Fred2";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			BaseJobDeclaration decInDiffFactory = newFactory.Load<UnknownCusAddInfoTest.JobDeclarationWithCusAddInfoTypeSupporter>(declaration.PK);
			var bizObjs = AdditionalChildrenHelper.GetAdditionalChildrenIfSupported(decInDiffFactory);
			AssertEquals(1, bizObjs.Length);
			AssertEquals("Fred1", ((CusAddInfo<AddInfoWithTypeCode>)bizObjs[0]).Data.UZ_String);

			declaration = Factory.New<UnknownCusCodeDataTest.JobDeclarationWithCusCodeDataTypeSupporter>();
			var cusCode1 = Factory.New<DummyCusCodeData>();
			cusCode1.CY_ParentID = declaration.PK;
			cusCode1.CY_ParentTableCode = declaration.TablePrefix;
			cusCode1.CY_Data = "Fred1";
			var cusCode2 = Factory.New<DummyCusCodeData>();
			cusCode2.CY_Type = "#@$";
			cusCode2.CY_ParentID = declaration.PK;
			cusCode2.CY_ParentTableCode = declaration.TablePrefix;
			cusCode2.CY_Data = "Fred2";
			Factory.Save();
			newFactory = new BusinessObjectFactory();
			decInDiffFactory = newFactory.Load<UnknownCusCodeDataTest.JobDeclarationWithCusCodeDataTypeSupporter>(declaration.PK);
			bizObjs = AdditionalChildrenHelper.GetAdditionalChildrenIfSupported(decInDiffFactory);
			AssertEquals(1, bizObjs.Length);
			AssertEquals("Fred1", ((DummyCusCodeData)bizObjs[0]).CY_Data);
		}
	}
}
