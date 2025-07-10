using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobDeclarationNumberFountainDuplicateReferenceTestCase : TestCase
	{
		[UseSnapshotProtection]
		public void TestSavingSetJE_OwnerRef_NoDuplicateReferenceException()
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var importer = factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMPABC";
			importer.MiscServ.OM_IMAutoImpJobRefered = true;

			var orgCompanyData = factory.NewWithValidTestData<OrgCompanyData>();
			orgCompanyData.OB_OH = importer.PK;
			orgCompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			factory.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var declaration = factory2.New<BaseJobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_OH_Importer = importer.PK;
			var dbConnection = ((IDbConnected)factory2).Connection;
			var ownerReference = ZString.Empty;
			try
			{
				dbConnection.BeginTransaction();
				declaration.PopulateJE_OwnerRefIfNeeded();
				ownerReference = declaration.JE_OwnerRef;
			}
			finally
			{
				dbConnection.RollbackTransaction();
			}

			var factory3 = new BusinessObjectFactory() { RefreshEnabled = false };
			var declaration2 = factory3.New<BaseJobDeclaration>();
			declaration2.JE_MessageType = "IMP";
			declaration2.JE_OH_Importer = importer.PK;
			factory3.Save();
			AssertEquals(ownerReference, declaration2.JE_OwnerRef);

			AssertEquals(ownerReference, declaration.JE_OwnerRef);
			factory2.Save();
			AssertNotEquals(ownerReference, declaration.JE_OwnerRef);
		}

		[UseSnapshotProtection]
		public void TestSavingSetJE_DeclarationReference_NoDuplicateReferenceException()
		{
			AssertPopulateJE_DeclarationReferenceIfNeededCore(JobMessageTypeList.Codes.WarehousedByExternalAgent);
			AssertPopulateJE_DeclarationReferenceIfNeededCore(JobMessageTypeList.Codes.Import);
		}

		void AssertPopulateJE_DeclarationReferenceIfNeededCore(string messageType)
		{
			var factory1 = new BusinessObjectFactory();
			var declaration = factory1.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_MessageType = messageType;
			var dbConnection = ((IDbConnected)factory1).Connection;
			var declarationReference = AssignNumberFromFountainAndRollbackTransaction(dbConnection, declaration);

			var newFactory = new BusinessObjectFactory();
			var declaration2 = newFactory.NewWithValidTestData<BaseJobDeclaration>();
			declaration2.JE_MessageType = messageType;
			declaration2.JE_DeclarationReference = ZString.Empty;
			newFactory.Save();
			AssertEquals("declaration2 should have the same declarationReference ", declarationReference, declaration2.JE_DeclarationReference);

			AssertEquals("declaration is assigned with the old declarationReference before saving", declarationReference, declaration.JE_DeclarationReference);
			factory1.Save();
			AssertNotEquals("declaration is assigned with a new declarationReference before saving", declarationReference, declaration.JE_DeclarationReference);
		}

		ZString AssignNumberFromFountainAndRollbackTransaction(DbConnection dbConnection, BaseJobDeclaration declaration)
		{
			ZString declarationReference;
			try
			{
				dbConnection.BeginTransaction();
				declaration.PopulateJE_DeclarationReferenceIfNeeded();
				declarationReference = declaration.JE_DeclarationReference;
			}
			finally
			{
				dbConnection.RollbackTransaction();
			}
			return declarationReference;
		}
	}
}
