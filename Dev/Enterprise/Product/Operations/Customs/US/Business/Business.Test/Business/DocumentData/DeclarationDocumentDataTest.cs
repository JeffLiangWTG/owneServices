using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	abstract class DeclarationDocumentDataTest<T> : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<T>
		where T : Customs.Business.MultiLineAddInfos.CusAddInfo, IDocumentDeliveredLogSupporter, IParentDocManagerSupport
	{
		public void TestIParentDocManagerSupportMembers()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			T data = (T)GetNewBusinessObject();
			data.B7_ParentID = declaration.PK;
			data.B7_ParentTableCode = declaration.TablePrefix;
			IParentDocManagerSupport supporter = data;
			AssertEquals(Core.Constants.DocManagerCodes.JobDeclaration, supporter.DocManagerInfo.DocManagerCode);
			AssertEquals("ParentGuid", declaration.PK, supporter.ParentGuid);
			AssertEquals("ParentTableName", declaration.TableName, supporter.ParentTableName);
		}

		public void TestIDocumentDeliveredLogSupporterMembers()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			T data = (T)GetNewBusinessObject();
			data.B7_ParentID = declaration.PK;
			data.B7_ParentTableCode = declaration.TablePrefix;
			IDocumentDeliveredLogSupporter supporter = data;
			AssertEquals("BusinessObjectTypeToLogAgainst", typeof(JobDeclaration), supporter.BusinessObjectTypeToLogAgainst);
			AssertEquals("Identifier", declaration.PK, supporter.Identifier);
		}
	}
}
