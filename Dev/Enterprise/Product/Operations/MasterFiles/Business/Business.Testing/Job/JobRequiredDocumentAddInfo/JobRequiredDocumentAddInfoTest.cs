using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobRequiredDocumentAddInfo))]
	sealed class JobRequiredDocumentAddInfoTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = (IDocsAndCartageParent)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			var requiredDoc = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var addInfo = requiredDoc.AddInfos.AddNew();
			addInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			return addInfo;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		[UseSnapshotProtection]
		public void TestUniqueIndexFailureHandlers()
		{
			var addInfo = CreateRequiredDocumentAddInfoUnderCACompany();
			var newDocumentAddInfoBO = (IBusinessObjectInternals)addInfo;
			AssertEquals(1, newDocumentAddInfoBO.UniqueIndexFailureHandlers.Count());
			AssertEquals("Enterprise.Customs.CA.Business.DIFReferenceNumberFountainStrategy+CAEntryNumberFountainUniqueIndexFailureHandler", newDocumentAddInfoBO.UniqueIndexFailureHandlers.First().GetType().ToString());

			var documentAddInfo = Factory.New<JobRequiredDocumentAddInfo>();
			var documentAddInfoBO = (IBusinessObjectInternals)documentAddInfo;
			AssertEquals(0, documentAddInfoBO.UniqueIndexFailureHandlers.Count());
		}

		[UseSnapshotProtection]
		public void TestNumberFountainUniqueIndexFailureHandler()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var connection = ((IDbConnected)Factory).Connection;
				using (var transactionManager = connection.BeginTransactionWithManager())
				{
					var fountain = Env.NumberFountains.GetCAEntryNumberGeneratorFountain("CADeclarationTransactionNumber34567DIF");
					fountain.SetNext(Factory, 1);
					transactionManager.CommitTransaction();
				}

				using (var conn = Db.NewAdminConnection())
				{
					var sqlText = @"
IF EXISTS (SELECT NULL FROM dbo.StmData WHERE SD_Name = 'AccountSecurityNo')
	UPDATE dbo.StmData
	SET SD_BinaryValue = CONVERT(VARBINARY(MAX), N'34567')
	WHERE SD_Name = 'AccountSecurityNo'
ELSE
	INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_BinaryValue)
	VALUES(NEWID(), 'AccountSecurityNo', CONVERT(VARBINARY(MAX), N'34567'))";
					conn.ExecuteNonQuery(sqlText);
				}
				var declaration = (IDocsAndCartageParent)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
				var requiredDocument = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
				var addInfo = Factory.New<JobRequiredDocumentAddInfo>();
				addInfo.EX_GC_Company = GlbCompany.CurrentCompany.PK;
				addInfo.EX_ApplicationCode = "CAD";
				addInfo.EX_EQ_RequiredDocument = requiredDocument.PK;
				Factory.Save();
				AssertEquals("EX_ReferenceNumber", "34567000000017", addInfo.EX_ReferenceNumber);

				using (var transactionManager = connection.BeginTransactionWithManager())
				{
					var fountain = Env.NumberFountains.GetCAEntryNumberGeneratorFountain("CADeclarationTransactionNumber34567DIF");
					fountain.SetNext(Factory, 1);
					transactionManager.CommitTransaction();
				}
				var addInfo1 = requiredDocument.AddInfos.AddNew();
				addInfo1.EX_GC_Company = GlbCompany.CurrentCompany.PK;
				addInfo1.EX_ApplicationCode = "CAD";

				var saveFailed = false;
				try
				{
					Factory.Save();
				}
				catch (Exception e)
				{
					saveFailed = true;
					AssertEquals("DocumentAddInfo Reference Number should not be populated.", string.Empty, addInfo1.EX_ReferenceNumber);
					ZExceptionReporting.HandleSaveException(e);
					ErrorReporter.Clear();
				}

				AssertStartsWith("User should be notified about the unique index conflict.",
	@"While you were working, the automatically assigned record number was used by another user.
Saving again should automatically resolve this issue.
Number Fountain: GeneratorFountain-C-F7C3F11D
Index: NR_UX__EX_GC_Company_EX_ReferenceNumber_EX_ApplicationCode
Value: JobRequiredDocumentAddInfo", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Save should have failed", saveFailed);
				UnitTestUserNotification.Instance.ClearMessages();

				Factory.Save();
				AssertEquals("Next DocumentAddInfo Reference Number should be given out.", "34567000000028", addInfo1.EX_ReferenceNumber);
				AssertEquals("There should be no error Message.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestSetDefaultValues()
		{
			var documentAddInfo = Factory.New<JobRequiredDocumentAddInfo>();
			AssertEquals(GlbCompany.CurrentCompany.PK, documentAddInfo.EX_GC_Company);
		}

		public void TestIDocManagerSupportProvider()
		{
			var declaration = (IDocsAndCartageParent)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			Factory.Save();

			var requiredDoc = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var addInfo = requiredDoc.AddInfos.AddNew();

			AssertEquals(declaration, ((IDocManagerSupportProvider)addInfo).DocManagerSupports.First());
		}

		public void TestDISHeader()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var declaration = (IDocsAndCartageParent)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
				Factory.Save();

				var requiredDoc = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
				var addInfo = requiredDoc.AddInfos.AddNew();

				AssertEquals(declaration, addInfo.DisHost);
			}
		}

		[UseSnapshotProtection]
		public void TestOnSaving()
		{
			var addInfo = CreateRequiredDocumentAddInfoUnderCACompany();
			Assert("EX_ReferenceNumber", addInfo.EX_ReferenceNumber.StartsWith("3456700000777"));
		}

		JobRequiredDocumentAddInfo CreateRequiredDocumentAddInfoUnderCACompany()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var connection = ((IDbConnected)Factory).Connection;
				using (var transactionManager = connection.BeginTransactionWithManager())
				{
					var fountain = Env.NumberFountains.GetCAEntryNumberGeneratorFountain("CADeclarationTransactionNumber34567DIF");
					fountain.SetNext(Factory, 777);
					transactionManager.CommitTransaction();
				}

				using (var conn = Db.NewAdminConnection())
				{
					var sqlText = @"
IF EXISTS (SELECT NULL FROM dbo.StmData WHERE SD_Name = 'AccountSecurityNo')
	UPDATE dbo.StmData
	SET SD_BinaryValue = CONVERT(VARBINARY(MAX), N'34567')
	WHERE SD_Name = 'AccountSecurityNo'
ELSE
	INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_BinaryValue)
	VALUES(NEWID(), 'AccountSecurityNo', CONVERT(VARBINARY(MAX), N'34567'))";
					conn.ExecuteNonQuery(sqlText);
				}
				var declaration = (IDocsAndCartageParent)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
				var requiredDocument = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
				var addInfo = Factory.New<JobRequiredDocumentAddInfo>();
				addInfo.EX_GC_Company = GlbCompany.CurrentCompany.PK;
				addInfo.EX_ApplicationCode = "CAD";
				addInfo.EX_EQ_RequiredDocument = requiredDocument.PK;
				Factory.Save();
				return addInfo;
			}
		}
	}
}
