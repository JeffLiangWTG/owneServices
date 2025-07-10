using System;
using System.IO;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.DocManagerInfo;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DocManagerInfoTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestStorageMainSaveException()
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var header = factory.New<OrgHeader>();
			header.OH_Code = "TESTORG";
			var dummy = factory.New<DummyBusinessObject>();
			factory.Save();
			var sqlText = $@"UPDATE dbo.DummyBizo SET Z0_Code = 'CHG' WHERE Z0_PK = '{dummy.PK}'";

			dummy.Z0_Code = "ERR";
			((IDbConnected)factory).Connection.ExecuteNonQuery(sqlText);
			var info = new DocManagerInfo(header, "ORG");
			info.UseBusinessEntityFactoryAsInternal = true;
			_ = info.StorageMain;

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var newHeader = newFactory.Load<OrgHeader>(header.PK);
			var newInfo = new DocManagerInfo(newHeader, "ORG");
			newInfo.UseBusinessEntityFactoryAsInternal = true;
			_ = newInfo.StorageMain;
			newInfo.Save();

			info.Save();
		}

		public void TestDocManagerCode()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			DocManagerInfo info = new DocManagerInfo(header, "ORG");

			AssertEquals("DocManagerCode should be what was passed in", "ORG", info.DocManagerCode);
		}

		public void TestBusinessEntity()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			DocManagerInfo info = new DocManagerInfo(header, "ORG");

			AssertEquals("Business entity should be the object that was passed in", header, info.BusinessEntity);
		}

		public void TestRelatedObjects()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			DocManagerInfo info = new DocManagerInfo(header, "ORG");

			AssertNotNull("Related objects should never be null", info.RelatedObjects);
			AssertEquals("Related Objects is empty by default. requires subclassing.", 0, info.RelatedObjects.Length);
		}

		public void TestRelatedObjectsContainsJobHeaderIfBOSuportsIJobInvoicingPlugin()
		{
			BusinessObject shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));

			DocManagerInfo info = new DocManagerInfo(shipment, "IVJ");
			AssertNotNull("Related objects should never be null", info.RelatedObjects);
			AssertEquals("Related Objects should contain 0 object.", 0, info.RelatedObjects.Length);

			JobHeader jobHeader = new JobHeader.Loader((IJobHeaderParent)shipment).TryCreate();
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			info = new DocManagerInfo(shipment, "IVJ");

			AssertNotNull("Related objects should never be null", info.RelatedObjects);
			AssertEquals("Related Objects should contain 1 object.", 1, info.RelatedObjects.Length);
			Assert("The related object should be a JobHeader", info.RelatedObjects[0] is JobHeader);
		}

		public void TestRelatedObjects_ShouldReportErrorIfRelatedObjectsContainsNullElements()
		{
			var header = Factory.New<OrgHeader>();
			var info = new DummyDocManagerInfoWithNullElementsInRelatedObjects(header, "ORG");

			AssertEquals("RelatedObjects should not contain null elements.", 0, info.RelatedObjects.Length);
			AssertContains("Last Error Message contains.", "GetRelatedObjects returns an Array with null Elements in", ErrorReporter.LastMessageReported);
			AssertContains("Last Error Message contains.", "DummyDocManagerInfoWithNullElementsInRelatedObjects", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestMasterFactory()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			DocManagerInfo info = ((IDocManagerSupport)header).DocManagerInfo;

			Assert("DocManagerInfo's MasterFactory property should be a different instance as the one passed in", info.MasterFactory != header.Factory);
		}

		public void TestSave()
		{
			IDocManagerSupport header = Factory.NewWithValidTestData<OrgHeader>();
			var newDocument = (BusinessObject)header.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 1, 1, 1, 1 }, "Test.pdf", "MSC");

			Assert("NewDocument isn't in database yet", !newDocument.IsInDatabase);
			header.DocManagerInfo.Save();
			Assert("New Document should now be in the database", newDocument.IsInDatabase);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAllEDocs()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			CreateEDocsRows(header.PK.ToGuid());
			DocManagerInfo info = ((IDocManagerSupport)header).DocManagerInfo;
			info.AddFileOrDocument(SamplePdfPath, "SOA");
			Factory.Save();
			info.Save();
			AssertEquals("AllEDocs.Count", 3, info.AllEDocs.Count);
		}

		public void TestSetupEDocsFactoryToBeSavedWithMainFactory()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var headerDocInfo = ((IDocManagerSupport)header).DocManagerInfo;

			headerDocInfo.AddFileOrDocument(new byte[] { 1, 2, 3, 4, 5 }, "xxx.yyy", "DUM");
			Factory.Save();

			var reloadedHeader = new BusinessObjectFactory().Load<OrgHeader>(header.PK);
			var reloadedHeaderDocInfo = ((IDocManagerSupport)reloadedHeader).DocManagerInfo;
			AssertEquals("eDoc is not saved as it in different factory.", 0, reloadedHeaderDocInfo.AllEDocs.Count);

			headerDocInfo.SetupEDocsFactoryToBeSavedWithMainFactory(true);
			AssertCollectionContains(headerDocInfo.MasterFactory, header.Factory.ChildFactories);
			Factory.Save();
			AssertEDocFactorySavedWithMainFactory("eDoc should be saved");

			headerDocInfo.AddFileOrDocument(new ZBlob(new byte[] { 5, 6, 7, 8, 9 }), "yyy.zzz", "DUM");
			Factory.Save();
			AssertEDocFactorySavedWithMainFactory("new eDoc should not be saved. Only previous eDoc should be here. SetupEDocsFactoryToBeSavedWithMainFactory is working for one save only.");

			headerDocInfo.SetupEDocsFactoryToBeSavedWithMainFactory(true);
			AssertCollectionContains(headerDocInfo.MasterFactory, header.Factory.ChildFactories);
			Factory.Save();
			reloadedHeader = new BusinessObjectFactory().Load<OrgHeader>(header.PK);
			reloadedHeaderDocInfo = ((IDocManagerSupport)reloadedHeader).DocManagerInfo;
			AssertEquals("Both eDocs should be saved now.", 2, reloadedHeaderDocInfo.AllEDocs.Count);

			void AssertEDocFactorySavedWithMainFactory(string message)
			{
				AssertCollectionNotContains(headerDocInfo.MasterFactory, header.Factory.ChildFactories);

				reloadedHeader = new BusinessObjectFactory().Load<OrgHeader>(header.PK);
				reloadedHeaderDocInfo = ((IDocManagerSupport)reloadedHeader).DocManagerInfo;

				AssertEquals(message, 1, reloadedHeaderDocInfo.AllEDocs.Count);
				var eDoc = reloadedHeaderDocInfo.AllEDocs[0];
				AssertEquals("FileName", "xxx.yyy", eDoc.FileName);
				AssertEquals("ImageData", new byte[] { 1, 2, 3, 4, 5 }, eDoc.ImageData);
				AssertEquals("DocType", "DUM", eDoc.DocType);
			}
		}

		public void TestRemoveEDocsFactoryWhenMainFactorySaveSuccessfully()
		{
			//Arrange
			var header = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var headerDocInfo = ((IDocManagerSupport)header).DocManagerInfo;
			headerDocInfo.AddFileOrDocument(new ZBlob(new byte[] { 1, 2, 3 }), "yyy.zzz", "DUM");

			//Act
			headerDocInfo.SetupEDocsFactoryToBeSavedWithMainFactory(false);

			//Assert
			AssertEquals("Pre-condition: File count of info should be 1.", 1, headerDocInfo.Files.Count);
			AssertNoExceptionThrown("Should not throw exception when saved storageMain record.", () => Factory.Save());
			var eDocFactory = (BusinessObjectFactory)headerDocInfo.MasterFactory;
			AssertEquals("eDocFactory should be removed from Main factory when save succeeded.", false, Factory.ChildFactories.Contains(eDocFactory));
		}

		public void TestSetupEDocsFactoryToBeSavedWithMainFactory_MainFactoryThrowsExceptionOnSaving_ChildFactoryIsNotRemoved()
		{
			//Arrange
			var header = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var headerDocInfo = ((IDocManagerSupport)header).DocManagerInfo;
			headerDocInfo.AddFileOrDocument(new ZBlob(new byte[] { 1, 2, 3 }), "yyy.zzz", "DUM");

			Factory.Saving += new BusinessObjectFactory.SavingEventHandler(Factory_Saving_BlowUp);
			void Factory_Saving_BlowUp(BusinessObjectFactory factory)
			{
				throw new ApplicationException("Test");
			}

			//Act
			headerDocInfo.SetupEDocsFactoryToBeSavedWithMainFactory(false);

			//Assert
			AssertEquals("Pre-condition: File count of info should be 1.", 1, headerDocInfo.Files.Count);
			AssertExceptionThrown<ApplicationException>("Should throw exception.", () => Factory.Save());
			var eDocFactory = (BusinessObjectFactory)headerDocInfo.MasterFactory;
			AssertEquals("eDocFactory should not be removed from Main factory when save failed.", true, Factory.ChildFactories.Contains(eDocFactory));
		}

		public void TestEDocsView()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			CreateEDocsRows(header.PK.ToGuid());
			var info = ((IDocManagerSupport)header).DocManagerInfo;
			info.AddFileOrDocument(
				new byte[] { 1, 1, 1, 1, 1 },
				"SOA",
				"MSC",
				visibleCompanyPK: Guid.NewGuid(),
				visibleBranchPK: Guid.NewGuid(),
				visibleDepartmentPK: Guid.NewGuid());
			Factory.Save();
			AssertEquals("EDocsView.Count", info.AllEDocs.Count - 1, info.EDocsView.Count);
		}

		public void TestEDocsViewShouldBeEmptyIfNoExistingStorageMain()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var info = ((IDocManagerSupport)header).DocManagerInfo;

			AssertType<EmptyStorageDocsBaseCollection>("EDocsView type should be EmptyStorageDocsBaseCollection", info.EDocsView);
			AssertEquals("EDocsView should be empty", 0, info.EDocsView.Count);

			info.AddFileOrDocument(new byte[] { 1, 2, 3, 4, 5 }, "xxx.yyy", "DUM");
			AssertEquals("EDocsView should have 1 file", 1, info.EDocsView.Count);

			((BusinessObject)info.StorageMain).Delete();
			AssertEquals("EDocsView should be empty", 0, info.EDocsView.Count);
			AssertType<EmptyStorageDocsBaseCollection>("EDocsView type should be EmptyStorageDocsBaseCollection", info.EDocsView);
		}

		public void TestConcurrencyHandling_EDocsView()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var info = new DocManagerInfo(dummy, null);
			info.AddFileOrDocument(new byte[] { 1, 2, 3, 4, 5 }, "xxx.yyy", "DUM");

			AssertEquals("EDocsView should have 1 file", 1, info.EDocsView.Count);

			var query = new ZQuery();
			query.AddToFilter(StorageMainSchema.SM_ParentFK, dummy.PK);
			query.AddToFilter(StorageMainSchema.PK, SQLComparisonOperator.NotEqual, ((BusinessObject)info.StorageMain).PK);
			AssertNull(((BusinessObjectFactory)info.MasterFactory).LoadTop1<IStorageMain>(query)); // Store query to query cache

			var anotherFactory = (BusinessObjectFactory)ObjectFactory.Get<IDocumentFactoryProvider>().GetFactory(new BusinessObjectFactory());
			anotherFactory.RefreshEnabled = false;
			var storageMain = (BusinessObject)anotherFactory.New<IStorageMain>();
			storageMain[StorageMainSchema.Constants.SM_ParentFK] = dummy.PK;
			storageMain[StorageMainSchema.Constants.SM_DB] = 1;
			storageMain[StorageMainSchema.Constants.SM_Type] = "DUM";
			((IStorageMain)storageMain).AddFileOrDocument(new byte[] { 1, 2, 3, 4, 5 }, "zzz.yyy", "DUM");

			using (GetFactoryIsolater(anotherFactory))
			using (GetFactoryIsolater((BusinessObjectFactory)info.MasterFactory))
			{
				anotherFactory.Save();
			}
			AssertNull("Should not be loaded without row reload", ((BusinessObjectFactory)info.MasterFactory).LoadTop1<IStorageMain>(query));

			info.Save();

			AssertEquals("Should have 2 files", 2, info.EDocsView.Count);
		}

		public void TestDocuments()
		{
			var header = Factory.New<OrgHeader>();
			header.FillWithValidTestData();
			CreateEDocsRows(header.PK.ToGuid());
			Factory.Save();

			var info = new DocManagerInfo(header, "ORG");
			AssertEquals("Should return two eDocs rows", 2, info.Documents.Count);
			AssertEquals("Should return two eDocs rows", 2, info.ExistingDocuments.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFiles()
		{
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(Db.DatabaseName + "_SD001.dbo." + StorageDocsSchema.Constants.TableName);

			var documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			var documentFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(Factory);
			var header = documentFactory.New<OrgHeader>();
			header.FillWithValidTestData();
			var info = ((IDocManagerSupport)header).DocManagerInfo;

			AssertEquals("precondition: no files", 0, info.Files.Count);

			_ = info.AddFileOrDocument(SamplePdfPath, "SOA");
			info.Save();
			AssertEquals("Files count", 1, info.Files.Count);
			AssertEquals("Files count", 1, info.ExistingFiles.Count);

			_ = info.AddFileOrDocument(SamplePdfPath, "SOA");
			info.Save();
			AssertEquals("Files count", 2, info.Files.Count);
			AssertEquals("Files count", 2, info.ExistingFiles.Count);
		}

		public void TestAccessStogrageMainDoNotCreateOneIfNotExist()
		{
			var header = Factory.New<OrgHeader>();
			header.FillWithValidTestData();
			Factory.Save();

			var info = new DocManagerInfo(header, "ORG");
			AssertDoNotCreateStorageMainIfEmpty(header, info, () => { _ = info.EDocsView; });
			AssertDoNotCreateStorageMainIfEmpty(header, info, () => { _ = info.ExistingDocuments; });
			AssertDoNotCreateStorageMainIfEmpty(header, info, () => { _ = info.ExistingFiles; });
		}

		void AssertDoNotCreateStorageMainIfEmpty(OrgHeader header, DocManagerInfo info, Action action)
		{
			action();
			info.Save();
			AssertNull(Factory.LoadTop1<IStorageMain>(new ZQuery(StorageMainSchema.SM_ParentFK, header.PK)));
		}

		public void TestReadOnly()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.FillWithValidTestData();

			header.ReadOnly = true;
			AssertEquals("DocManagerInfo should be the same readonly status as parent", header.ReadOnly, ((IDocManagerSupport)header).DocManagerInfo.ReadOnly);

			header.ReadOnly = false;
			AssertEquals("DocManagerInfo should be the same readonly status as parent", header.ReadOnly, ((IDocManagerSupport)header).DocManagerInfo.ReadOnly);
		}

		public void TestGetEDocsProviders()
		{
			DocManagerInfo info = new DocManagerInfo(Factory.New<DummyBusinessObject>(), null);
			AssertEquals("GetEDocsProviders().Length", 0, info.GetEDocsProviders().Length);
		}

		public void TestShouldRecordDocument()
		{
			DocManagerInfo info = new DocManagerInfo(Factory.New<DummyBusinessObject>(), null);
			AssertEquals("ShouldRecordDocument()", true, info.ShouldRecordDocument(Factory.New<StmMenuItem>()));
		}

		#region AddFileOrDocument

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddFileOrDocument()
		{
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(Db.DatabaseName + "_SD001.dbo." + StorageDocsSchema.Constants.TableName);
			IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			BusinessObjectFactory documentFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(Factory);
			OrgHeader header = documentFactory.New<OrgHeader>();
			header.FillWithValidTestData();

			DocManagerInfo info = ((IDocManagerSupport)header).DocManagerInfo;

			var addedFile = (BusinessObject)info.AddFileOrDocument(SamplePdfPath, "SOA");
			info.Save();

			ZQuery filter = new ZQuery(StorageMainSchema.SM_ParentFK, header.PK);
			BusinessObject storageMain = (BusinessObject)documentFactory.LoadTop1<IStorageMain>(filter);
			BusinessObjectFactory storageDocsFactory = ((IDocumentFactory)documentFactory).GetFactory((ZInt)storageMain[StorageMainSchema.SM_DB.Name]);
			Assert("Added file should be in the DB - it has to be saved in the method while the factory instance is available.", addedFile.IsInDatabase);
			AssertEquals("Should be a file added", 1, storageDocsFactory.GetDatabaseCount(ObjectFactory.GetType<IStorageDocs>()));

			info.AddFileOrDocument(SamplePdfPath, "SOA");
			info.Save();
			AssertEquals("Should be two file added - default action is to create a new file if the filename is the same", 2, storageDocsFactory.GetDatabaseCount(ObjectFactory.GetType<IStorageDocs>()));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddFileWithOverwriteOverload()
		{
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(Db.DatabaseName + "_SD001.dbo." + StorageDocsSchema.Constants.TableName);
			IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			BusinessObjectFactory documentFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(Factory);
			OrgHeader header = documentFactory.New<OrgHeader>();
			header.FillWithValidTestData();

			DocManagerInfo info = ((IDocManagerSupport)header).DocManagerInfo;

			info.AddFileOrDocument(SamplePdfPath, "SOA", true);
			info.Save();

			ZQuery filter = new ZQuery(StorageMainSchema.SM_ParentFK, header.PK);
			BusinessObject storageMain = (BusinessObject)documentFactory.LoadTop1<IStorageMain>(filter);
			BusinessObjectFactory storageDocsFactory = ((IDocumentFactory)documentFactory).GetFactory((ZInt)storageMain[StorageMainSchema.SM_DB.Name]);
			AssertEquals("Should be a document added to db", 1, storageDocsFactory.GetDatabaseCount(ObjectFactory.GetType<IStorageDocs>()));

			info.AddFileOrDocument(SamplePdfPath, "SOA", true);
			info.Save();
			AssertEquals("Should still be only 1 document - should overwrite the file if it exists", 1, storageDocsFactory.GetDatabaseCount(ObjectFactory.GetType<IStorageDocs>()));

			info.AddFileOrDocument(SamplePdfPath, "SOA", false);
			info.Save();
			AssertEquals("Should now be two documents - didn't overwrite the file if the same name exists", 2, storageDocsFactory.GetDatabaseCount(ObjectFactory.GetType<IStorageDocs>()));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddFileWithNamePathAndOverwriteOverload()
		{
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(Db.DatabaseName + "_SD001.dbo." + StorageDocsSchema.Constants.TableName);
			IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			BusinessObjectFactory documentFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(Factory);
			OrgHeader header = documentFactory.New<OrgHeader>();
			header.FillWithValidTestData();

			DocManagerInfo info = ((IDocManagerSupport)header).DocManagerInfo;

			info.AddFileOrDocument(SamplePdfPath, "SOA", true, filenameOnly: "Test.pdf");
			info.Save();

			ZQuery filter = new ZQuery(StorageMainSchema.SM_ParentFK, header.PK);
			BusinessObject storageMain = (BusinessObject)documentFactory.LoadTop1<IStorageMain>(filter);
			BusinessObjectFactory storageDocsFactory = ((IDocumentFactory)documentFactory).GetFactory((ZInt)storageMain[StorageMainSchema.SM_DB.Name]);
			AssertEquals("Should be a document added to db", 1, storageDocsFactory.GetDatabaseCount(ObjectFactory.GetType<IStorageDocs>()));

			info.AddFileOrDocument(SamplePdfPath, "SOA", true, filenameOnly: "Test.pdf");
			info.Save();
			AssertEquals("Should still be only 1 document - should overwrite the file if it exists", 1, storageDocsFactory.GetDatabaseCount(ObjectFactory.GetType<IStorageDocs>()));

			info.AddFileOrDocument(SamplePdfPath, "SOA", false, filenameOnly: "Test.pdf");
			info.Save();
			AssertEquals("Should now be two documents - didn't overwrite the file if the same name exists", 2, storageDocsFactory.GetDatabaseCount(ObjectFactory.GetType<IStorageDocs>()));

			info.AddFileOrDocument(SamplePdfPath, "SOA", true, filenameOnly: "Test2.pdf");
			info.Save();
			AssertEquals("Should now be two documents - didn't overwrite the file if the same name exists", 3, storageDocsFactory.GetDatabaseCount(ObjectFactory.GetType<IStorageDocs>()));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddFileWithNamePathTypeAndDescription()
		{
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(Db.DatabaseName + "_SD001.dbo." + StorageDocsSchema.Constants.TableName);
			var documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			var documentFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(Factory);
			var header = documentFactory.New<OrgHeader>();
			header.FillWithValidTestData();

			var info = ((IDocManagerSupport)header).DocManagerInfo;

			var testDescription = "Test description";
			info.AddFileOrDocument(SamplePdfPath, "SOA", description: testDescription);
			info.Save();

			var filter = new ZQuery(StorageMainSchema.SM_ParentFK, header.PK);
			var storageMain = (BusinessObject)documentFactory.LoadTop1<IStorageMain>(filter);
			var storageDocsFactory = ((IDocumentFactory)documentFactory).GetFactory((ZInt)storageMain[StorageMainSchema.SM_DB.Name]);
			AssertEquals("Should be a document added to db", 1, storageDocsFactory.GetDatabaseCount(ObjectFactory.GetType<IStorageDocs>()));

			info.AddFileOrDocument(SamplePdfPath, "SOA", description: testDescription);
			info.Save();
			AssertEquals("Should now be two documents - didn't overwrite the file if the same name exists", 2, storageDocsFactory.GetDatabaseCount(ObjectFactory.GetType<IStorageDocs>()));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddFileWithByteArray()
		{
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(Db.DatabaseName + "_SD001.dbo." + StorageDocsSchema.Constants.TableName);
			IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			BusinessObjectFactory documentFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(Factory);
			OrgHeader header = documentFactory.New<OrgHeader>();
			header.FillWithValidTestData();

			DocManagerInfo info = ((IDocManagerSupport)header).DocManagerInfo;

			var testByteArray = File.ReadAllBytes(SamplePdfPath);
			var addedFile = (BusinessObject)info.AddFileOrDocument(testByteArray, "Sample.PDF", "SOA");
			info.Save();

			ZQuery filter = new ZQuery(StorageMainSchema.SM_ParentFK, header.PK);
			BusinessObject storageMain = (BusinessObject)documentFactory.LoadTop1<IStorageMain>(filter);
			BusinessObjectFactory storageDocsFactory = ((IDocumentFactory)documentFactory).GetFactory((ZInt)storageMain[StorageMainSchema.SM_DB.Name]);
			Assert("Added file should be in the DB - it has to be saved in the method while the factory instance is available.", addedFile.IsInDatabase);
			AssertEquals("Should be a document added", 1, storageDocsFactory.GetDatabaseCount(ObjectFactory.GetType<IStorageDocs>()));

			info.AddFileOrDocument(SamplePdfPath, "SOA");
			info.Save();
			AssertEquals("Should be two documents added - default action is to create a new file if the filename is the same", 2, storageDocsFactory.GetDatabaseCount(ObjectFactory.GetType<IStorageDocs>()));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddFileWithByteArrayOverwrite()
		{
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(Db.DatabaseName + "_SD001.dbo." + StorageDocsSchema.Constants.TableName);
			IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			BusinessObjectFactory documentFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(Factory);
			OrgHeader header = documentFactory.New<OrgHeader>();
			header.FillWithValidTestData();

			DocManagerInfo info = ((IDocManagerSupport)header).DocManagerInfo;

			var testByteArray = File.ReadAllBytes(SamplePdfPath);
			var addedFile = (BusinessObject)info.AddFileOrDocument(testByteArray, "Sample.PDF", "SOA");
			info.AddFileOrDocument(testByteArray, "Sample.PDF", "SOA", true);
			info.Save();

			ZQuery filter = new ZQuery(StorageMainSchema.SM_ParentFK, header.PK);
			BusinessObject storageMain = (BusinessObject)documentFactory.LoadTop1<IStorageMain>(filter);
			BusinessObjectFactory storageDocsFactory = ((IDocumentFactory)documentFactory).GetFactory((ZInt)storageMain[StorageMainSchema.SM_DB.Name]);
			AssertEquals("Should be a document added to db", 1, storageDocsFactory.GetDatabaseCount(ObjectFactory.GetType<IStorageDocs>()));

			info.AddFileOrDocument(testByteArray, "Sample.PDF", "SOA", true);
			info.Save();
			AssertEquals("Should still be only 1 document - should overwrite the file if it exists", 1, storageDocsFactory.GetDatabaseCount(ObjectFactory.GetType<IStorageDocs>()));

			info.AddFileOrDocument(testByteArray, "Sample.PDF", "SOA", false);
			info.Save();
			AssertEquals("Should now be two documents - didn't overwrite the file if the same name exists", 2, storageDocsFactory.GetDatabaseCount(ObjectFactory.GetType<IStorageDocs>()));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddCompanyBranchDepartmentSpecificDocument()
		{
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(Db.DatabaseName + "_SD001.dbo." + StorageDocsSchema.Constants.TableName);
			IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			BusinessObjectFactory documentFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(Factory);
			OrgHeader header = documentFactory.New<OrgHeader>();
			header.FillWithValidTestData();

			DocManagerInfo info = ((IDocManagerSupport)header).DocManagerInfo;

			var testByteArray = File.ReadAllBytes(SamplePdfPath);
			var addedFile1 = info.AddFileOrDocument(testByteArray, "Sample.PDF", "SOA");
			AssertEquals(string.Empty, addedFile1.VisibleBranchCode);
			AssertEquals(string.Empty, addedFile1.VisibleCompanyCode);
			AssertEquals(string.Empty, addedFile1.VisibleDepartmentCode);

			var addedFile2 = info.AddFileOrDocument(
				testByteArray,
				"Sample.PDF",
				"SOA",
				visibleCompanyPK: GlbCompany.CurrentCompany.PK.ToGuid(),
				visibleBranchPK: GlbBranch.CurrentBranch.PK.ToGuid(),
				visibleDepartmentPK: GlbDepartment.CurrentDepartment.PK.ToGuid());
			AssertEquals(GlbCompany.CurrentCompany.GC_Code, addedFile2.VisibleCompanyCode);
			AssertEquals(GlbBranch.CurrentBranch.GB_Code, addedFile2.VisibleBranchCode);
			AssertEquals(GlbDepartment.CurrentDepartment.GE_Code, addedFile2.VisibleDepartmentCode);
		}

		#endregion

		#region HasUnreadRelatedDocuments

		public void TestHasUnreadRelatedDocuments()
		{
			DocTypeWithForceUserToRead.Factory.Save();
			TestHasUnreadRelatedDocuments(DocTypeWithForceUserToRead, DocTypeWithForceUserToRead.RT_ReferenceType, typeof(OrgHeader));
		}

		public void TestHasUnreadRelatedDocuments_ForAllReferenceType()
		{
			DocTypeWithForceUserToReadWithReferenceTypeAll.Factory.Save();
			TestHasUnreadRelatedDocuments(DocTypeWithForceUserToReadWithReferenceTypeAll, DocTypeWithForceUserToRead.RT_ReferenceType, typeof(OrgHeader));
		}

		public void TestHasUnreadRelatedDocuments_WithCategoryMapping()
		{
			DocTypeWithForceUserToReadWithCategoryMapping.Factory.Save();
			TestHasUnreadRelatedDocuments(DocTypeWithForceUserToReadWithCategoryMapping, DocTypeWithForceUserToReadWithCategoryMapping.RT_ReferenceType, typeof(OrgHeader));
		}

		public void TestHasUnreadRelatedDocuments_NoDbHitIfNotExistsAnyForceToReadDocTypes()
		{
			DocTypeWithForceUserToRead.Factory.Save(); // invalidate the 'force user to read' reference type cache
			BusinessObject parent = Factory.NewWithValidTestData<OrgHeader>();
			DocManagerInfo docManagerInfo = new DocManagerInfo(parent, "XXX");
			AssertEquals("Should not have any unread documents", false, docManagerInfo.HasUnreadRelatedDocuments);

			int commandCountBefore = Db.Connection.ExecutedCommandCount;
			AssertEquals("Should not have any unread documents", false, docManagerInfo.HasUnreadRelatedDocuments);
			AssertEquals("Should not hit the DB at all now that RefDocType.ForceToReadReferenceTypes is cached", commandCountBefore, Db.Connection.ExecutedCommandCount);
		}

		void TestHasUnreadRelatedDocuments(RefDocType docType, ZString referenceType, Type parentType)
		{
			BusinessObject parent = Factory.NewWithValidTestData(parentType);
			Factory.Save();

			DocManagerInfo docManagerInfo = ((IDocManagerSupport)parent).DocManagerInfo;

			IeDoc document1 = (IeDoc)CreateNewDocumentWithForceUserToReadDocType(docManagerInfo, docType.RT_DocType);
			IeDoc document2 = (IeDoc)CreateNewDocumentWithForceUserToReadDocType(docManagerInfo, docType.RT_DocType);
			IeDoc document3 = (IeDoc)CreateNewDocumentWithForceUserToReadDocType(docManagerInfo, docType.RT_DocType);

			AssertEquals("Unread related documents exist", true, docManagerInfo.HasUnreadRelatedDocuments);

			NotifyDocumentReadByUser(document1);
			NotifyDocumentReadByUser(document3);
			AssertEquals("Some, but not all unread related documents not yet read", true, docManagerInfo.HasUnreadRelatedDocuments);

			NotifyDocumentReadByUser(document2);
			AssertEquals("All unread related documents read", false, docManagerInfo.HasUnreadRelatedDocuments);
		}

		public void TestHasUnreadRelatedDocuments_WhenDocumentCreatedBySameUser()
		{
			IDocManagerSupport docManagerSupport = Factory.NewWithValidTestData<OrgHeader>();
			DocTypeWithForceUserToRead.Factory.Save();
			BusinessObject document = CreateNewDocumentWithForceUserToReadDocType(docManagerSupport.DocManagerInfo, DocTypeWithForceUserToRead.RT_DocType);
			Factory.Save();

			document[StorageDocsSchema.SC_SystemCreateUser] = GlbStaff.CurrentUser.GS_Code;
			docManagerSupport.DocManagerInfo.Save();
			AssertEquals("The document isn't 'unread' if current user == document adding user", false, docManagerSupport.DocManagerInfo.HasUnreadRelatedDocuments);

			document[StorageDocsSchema.SC_SystemCreateUser] = "XXX";
			docManagerSupport.DocManagerInfo.Save();
			AssertEquals("The document is 'unread' if current user != document adding user", true, docManagerSupport.DocManagerInfo.HasUnreadRelatedDocuments);
		}

		public void TestHasUnreadRelatedDocuments_WhenDocumentIsCompanySpecific()
		{
			GlbStaff user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_Code = "AAA";
			user.GS_LoginName = "AAA User";
			user.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(user.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				GlbCompany company1 = Factory.NewWithValidTestData<GlbCompany>();
				AddSecurity(GlbStaff.CurrentUser.PK, "ViewAllCompanySpecificDocuments", Env.CurrentCompany.PK, Env.CurrentBranch.PK, ZGuid.Empty, false);
				Factory.Save();
				Assert(!Env.Security.ViewAllCompanySpecificDocuments.IsAllowed);

				IDocManagerSupport docManagerSupport = Factory.NewWithValidTestData<OrgHeader>();
				DocTypeWithForceUserToRead.Factory.Save();
				var document = CreateNewCompanySpecificDocumentWithForceUserToReadDocType(docManagerSupport.DocManagerInfo,
					DocTypeWithForceUserToRead.RT_DocType, Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				Factory.Save();

				document[StorageDocsSchema.SC_SystemCreateUser] = "XXX";
				docManagerSupport.DocManagerInfo.Save();
				AssertEquals(true, docManagerSupport.DocManagerInfo.HasUnreadRelatedDocuments);

				((IeDoc)document).NotifyReadByUser();
				AssertEquals(false, docManagerSupport.DocManagerInfo.HasUnreadRelatedDocuments);

				var document2 = CreateNewCompanySpecificDocumentWithForceUserToReadDocType(docManagerSupport.DocManagerInfo,
					DocTypeWithForceUserToRead.RT_DocType, company1.PK.ToGuid(), Guid.Empty, Guid.Empty);
				Factory.Save();
				AssertEquals(false, docManagerSupport.DocManagerInfo.HasUnreadRelatedDocuments);
			}
		}

		public void TestHasUnreadRelatedDocuments_WhenDocumentIsBranchSpecific()
		{
			GlbStaff user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_Code = "AAA";
			user.GS_LoginName = "AAA User";
			user.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(user.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
				AddSecurity(GlbStaff.CurrentUser.PK, "ViewAllBranchSpecificDocuments", Env.CurrentCompany.PK, Env.CurrentBranch.PK, ZGuid.Empty, false);
				Factory.Save();
				Assert(!Env.Security.ViewAllBranchSpecificDocuments.IsAllowed);

				IDocManagerSupport docManagerSupport = Factory.NewWithValidTestData<OrgHeader>();
				DocTypeWithForceUserToRead.Factory.Save();
				var document = CreateNewCompanySpecificDocumentWithForceUserToReadDocType(docManagerSupport.DocManagerInfo,
					DocTypeWithForceUserToRead.RT_DocType, Env.CurrentCompany.PK, Env.CurrentBranch.PK, Guid.Empty);
				Factory.Save();

				document[StorageDocsSchema.SC_SystemCreateUser] = "XXX";
				docManagerSupport.DocManagerInfo.Save();
				AssertEquals(true, docManagerSupport.DocManagerInfo.HasUnreadRelatedDocuments);

				((IeDoc)document).NotifyReadByUser();
				AssertEquals(false, docManagerSupport.DocManagerInfo.HasUnreadRelatedDocuments);

				var document2 = CreateNewCompanySpecificDocumentWithForceUserToReadDocType(docManagerSupport.DocManagerInfo,
					DocTypeWithForceUserToRead.RT_DocType, Guid.Empty, branch1.PK.ToGuid(), Guid.Empty);
				Factory.Save();
				AssertEquals(false, docManagerSupport.DocManagerInfo.HasUnreadRelatedDocuments);
			}
		}

		public void TestHasUnreadRelatedDocuments_WhenDocumentIsDepartmentSpecific()
		{
			GlbStaff user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_Code = "AAA";
			user.GS_LoginName = "AAA User";
			user.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(user.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				GlbDepartment department1 = Factory.NewWithValidTestData<GlbDepartment>();
				AddSecurity(GlbStaff.CurrentUser.PK, "ViewAllDepartmentSpecificDocuments", Env.CurrentCompany.PK, Env.CurrentBranch.PK, ZGuid.Empty, false);
				Factory.Save();
				Assert(!Env.Security.ViewAllDepartmentSpecificDocuments.IsAllowed);

				IDocManagerSupport docManagerSupport = Factory.NewWithValidTestData<OrgHeader>();
				DocTypeWithForceUserToRead.Factory.Save();
				var document = CreateNewCompanySpecificDocumentWithForceUserToReadDocType(docManagerSupport.DocManagerInfo,
					DocTypeWithForceUserToRead.RT_DocType, Guid.Empty, Guid.Empty, Env.CurrentDepartment.PK);
				Factory.Save();

				document[StorageDocsSchema.SC_SystemCreateUser] = "XXX";
				docManagerSupport.DocManagerInfo.Save();
				AssertEquals(true, docManagerSupport.DocManagerInfo.HasUnreadRelatedDocuments);

				((IeDoc)document).NotifyReadByUser();
				AssertEquals(false, docManagerSupport.DocManagerInfo.HasUnreadRelatedDocuments);

				var document2 = CreateNewCompanySpecificDocumentWithForceUserToReadDocType(docManagerSupport.DocManagerInfo,
					DocTypeWithForceUserToRead.RT_DocType, Guid.Empty, Guid.Empty, department1.PK.ToGuid());
				Factory.Save();
				AssertEquals(false, docManagerSupport.DocManagerInfo.HasUnreadRelatedDocuments);
			}
		}

		GlbSecurity AddSecurity(ZGuid userPK, string securityRight, ZGuid companyPK, ZGuid branchPK, ZGuid departmentPK, bool granted)
		{
			GlbSecurity security = Factory.New<GlbSecurity>();
			security.GU_GS = userPK;
			security.GU_GC = companyPK;
			security.GU_GB = branchPK;
			security.GU_GE = departmentPK;
			security.GU_SecurityRight = securityRight;
			security.GU_SecurityItemIsAllowed = granted;

			return security;
		}

		#endregion

		public void TestBusinessEntityFactoryIsPassedIntoMasterFactory()
		{
			DocManagerInfo info = new DocManagerInfo(null, null);
			AssertEquals("A null parent was passed into DocManagerInfo", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			BusinessObject obj = (info.MasterFactory as BusinessObjectFactory).LoadTop1<OrgHeader>(new ZQuery());
			AssertEquals(1, info.MasterFactory.ChildParticipants.Length);
			BusinessObjectFactory internalFactory = info.MasterFactory.ChildParticipants[0] as BusinessObjectFactory;
			AssertNotNull(internalFactory);
			AssertEquals(obj.Factory, internalFactory);

			obj = Factory.New<DummyBusinessObject>();
			info = new DocManagerInfo(obj, null);
			obj = (info.MasterFactory as BusinessObjectFactory).LoadTop1<OrgHeader>(new ZQuery());
			AssertEquals(1, info.MasterFactory.ChildParticipants.Length);
			internalFactory = info.MasterFactory.ChildParticipants[0] as BusinessObjectFactory;
			AssertNotEquals(Factory, internalFactory);

			obj = Factory.New<DummyBusinessObject>();
			info = new DocManagerInfo(obj, null);
			info.UseBusinessEntityFactoryAsInternal = true;
			obj = (info.MasterFactory as BusinessObjectFactory).LoadTop1<OrgHeader>(new ZQuery());
			AssertEquals(1, info.MasterFactory.ChildParticipants.Length);
			internalFactory = info.MasterFactory.ChildParticipants[0] as BusinessObjectFactory;
			AssertEquals(Factory, internalFactory);
			AssertEquals(obj.Factory, internalFactory);
			obj = (info.MasterFactory as BusinessObjectFactory).LoadTop1<OrgHeader>(new ZQuery());
			AssertEquals(obj.Factory, internalFactory);
		}

		#region Test Concurrency Handling

		public void TestConcurrencyHandling()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			DocManagerInfo info = new DocManagerInfo(dummy, null);
			info.AddFileOrDocument(new byte[] { 1, 2, 3, 4, 5 }, "xxx.yyy", "DUM");

			ZQuery query = new ZQuery();
			query.AddToFilter(StorageMainSchema.SM_ParentFK, dummy.PK);
			query.AddToFilter(StorageMainSchema.PK, SQLComparisonOperator.NotEqual, ((BusinessObject)info.StorageMain).PK);
			AssertNull(((BusinessObjectFactory)info.MasterFactory).LoadTop1<IStorageMain>(query)); // Store query to query cache

			var anotherFactory = (BusinessObjectFactory)ObjectFactory.Get<IDocumentFactoryProvider>().GetFactory(new BusinessObjectFactory());
			anotherFactory.RefreshEnabled = false;
			BusinessObject storageMain = (BusinessObject)anotherFactory.New<IStorageMain>();
			storageMain[StorageMainSchema.Constants.SM_ParentFK] = dummy.PK;
			storageMain[StorageMainSchema.Constants.SM_DB] = 1;
			storageMain[StorageMainSchema.Constants.SM_Type] = "DUM";

			using (GetFactoryIsolater(anotherFactory))
			using (GetFactoryIsolater((BusinessObjectFactory)info.MasterFactory))
			{
				anotherFactory.Save();
			}
			AssertNull("Should not be loaded without row reload", ((BusinessObjectFactory)info.MasterFactory).LoadTop1<IStorageMain>(query));

			info.Save();

			Assert("There should be a StorageMain and it should be stored in database", ((BusinessObject)info.StorageMain).IsInDatabase);
			AssertEquals(1, info.StorageMain.AllEDocs.Count);
			AssertEquals("xxx.yyy", info.StorageMain.AllEDocs[0].FileName);
		}

		#endregion

		#region Implementation

		void CreateEDocsRows(Guid parentFK)
		{
			// don't have access to the bizos here, so use sql to create
			Guid sM_PK = Guid.NewGuid();
			Guid sC_PK = Guid.NewGuid();
			Guid sC_PK2 = Guid.NewGuid();
			byte[] imageData = new byte[] { 1, 2, 3, 4, 5 };

			string cmdString = "INSERT INTO " + StorageMainSchema.Constants.SqlSchemaName + "." + StorageMainSchema.Constants.TableName
				+ " (" + StorageMainSchema.Constants.PK + ", "
				+ StorageMainSchema.Constants.SM_Type + ", "
				+ StorageMainSchema.Constants.SM_ParentFK + ", "
				+ StorageMainSchema.Constants.SM_DB + " ) "
				+ @" VALUES 
				(@SM_PK, 
				@SM_Type, 
				@SM_ParentFK,
				@SM_DB)";
			var cmd = Db.Connection.Command(cmdString); // don't have access to BizOs here (they're built after us), use SQL to create
			cmd.AddParameterBasedOnDbColumn("@SM_PK", sM_PK, StorageMainSchema.PK);
			cmd.AddParameterBasedOnDbColumn("@SM_Type", "ORG", StorageMainSchema.SM_Type);
			cmd.AddParameterBasedOnDbColumn("@SM_ParentFK", parentFK, StorageMainSchema.SM_ParentFK);
			cmd.AddParameterBasedOnDbColumn("@SM_DB", 1, StorageMainSchema.SM_DB);
			cmd.ExecuteNonQuery();

			cmdString = string.Format("INSERT INTO {0}_SD001..", Db.DatabaseName.Trim()) + StorageDocsSchema.Constants.TableName
				+ " (" + StorageDocsSchema.Constants.PK + ", "
				+ StorageDocsSchema.Constants.SC_Date + ", "
				+ StorageDocsSchema.Constants.SC_SystemCreateTimeUtc + ", "
				+ StorageDocsSchema.Constants.SC_SystemLastEditTimeUtc + ", "
				+ StorageDocsSchema.Constants.SC_DocType + ", "
				+ StorageDocsSchema.Constants.SC_Desc + ", "
				+ StorageDocsSchema.Constants.SC_SM + ", "
				+ StorageDocsSchema.Constants.SC_ImageData + ", "
				+ StorageDocsSchema.Constants.SC_IsSystemGenerated + " ) "
				+ @" VALUES 
				(@SC_PK, 
				getdate(), 
				getdate(), 
				getdate(), 
				@SC_DocType, 
				@SC_Desc,
				@SC_SM, 
				@SC_ImageData,
				@SC_IsSystemGenerated)";
			cmd = Db.Connection.Command(cmdString); // don't have access to BizOs here (they're built after us), use SQL to create
			cmd.AddParameterBasedOnDbColumn("@SC_PK", sC_PK, StorageDocsSchema.PK);
			cmd.AddParameterBasedOnDbColumn("@SC_DocType", "MSC", StorageDocsSchema.SC_DocType);
			cmd.AddParameterBasedOnDbColumn("@SC_Desc", "This is a test", StorageDocsSchema.SC_Desc);
			cmd.AddParameterBasedOnDbColumn("@SC_SM", sM_PK, StorageDocsSchema.SC_SM);
			cmd.AddParameterBasedOnDbColumn("@SC_ImageData", imageData, StorageDocsSchema.SC_ImageData);
			cmd.AddParameterBasedOnDbColumn("@SC_IsSystemGenerated", "Y", StorageDocsSchema.SC_IsSystemGenerated);
			cmd.ExecuteNonQuery();

			cmdString = string.Format("INSERT INTO {0}_SD001..", Db.DatabaseName.Trim()) + StorageDocsSchema.Constants.TableName
				+ " (" + StorageDocsSchema.Constants.PK + ", "
				+ StorageDocsSchema.Constants.SC_Date + ", "
				+ StorageDocsSchema.Constants.SC_SystemCreateTimeUtc + ", "
				+ StorageDocsSchema.Constants.SC_SystemLastEditTimeUtc + ", "
				+ StorageDocsSchema.Constants.SC_DocType + ", "
				+ StorageDocsSchema.Constants.SC_Desc + ", "
				+ StorageDocsSchema.Constants.SC_SM + ", "
				+ StorageDocsSchema.Constants.SC_ImageData + ", "
				+ StorageDocsSchema.Constants.SC_IsSystemGenerated + " ) "
				+ @" VALUES 
				(@SC_PK, 
				getdate(), 
				getdate(), 
				getdate(), 
				@SC_DocType, 
				@SC_Desc,
				@SC_SM, 
				@SC_ImageData,
				@SC_IsSystemGenerated)";
			cmd = Db.Connection.Command(cmdString); // don't have access to BizOs here (they're built after us), use SQL to create
			cmd.AddParameterBasedOnDbColumn("@SC_PK", sC_PK2, StorageDocsSchema.PK);
			cmd.AddParameterBasedOnDbColumn("@SC_DocType", "MSC", StorageDocsSchema.SC_DocType);
			cmd.AddParameterBasedOnDbColumn("@SC_Desc", "This is another test", StorageDocsSchema.SC_Desc);
			cmd.AddParameterBasedOnDbColumn("@SC_SM", sM_PK, StorageDocsSchema.SC_SM);
			cmd.AddParameterBasedOnDbColumn("@SC_ImageData", imageData, StorageDocsSchema.SC_ImageData);
			cmd.AddParameterBasedOnDbColumn("@SC_IsSystemGenerated", "N", StorageDocsSchema.SC_IsSystemGenerated);
			cmd.ExecuteNonQuery();
		}

		BusinessObject CreateNewDocumentWithForceUserToReadDocType(DocManagerInfo docManagerInfo, ZString docType)
		{
			return CreateNewCompanySpecificDocumentWithForceUserToReadDocType(docManagerInfo, docType, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		BusinessObject CreateNewCompanySpecificDocumentWithForceUserToReadDocType(DocManagerInfo docManagerInfo, ZString docType, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			using (CurrentUserInitialsChanger.ChangeCurrentUserInitials("XXX"))
			{
				BusinessObject result = (BusinessObject)docManagerInfo.AddFileOrDocument(new byte[] { 1, 1, 1, 1, 1 }, "Test.pdf", docType, visibleCompanyPK: companyPK, visibleBranchPK: branchPK, visibleDepartmentPK: departmentPK);
				docManagerInfo.Save();
				return result;
			}
		}

		void NotifyDocumentReadByUser(IeDoc document)
		{
			document.NotifyReadByUser();
		}

		RefDocType DocTypeWithForceUserToRead
		{
			get
			{
				if (fDocTypeWithForceUserToRead == null)
				{
					fDocTypeWithForceUserToRead = CreateDocTypeWithForceUserToRead(Core.Constants.ReferenceTypes.ClientSupplierRelationship, "FUR");
				}
				return fDocTypeWithForceUserToRead;
			}
		}
		RefDocType fDocTypeWithForceUserToRead;

		RefDocType DocTypeWithForceUserToReadWithReferenceTypeAll
		{
			get
			{
				if (fDocTypeWithForceUserToReadWithReferenceTypeAll == null)
				{
					fDocTypeWithForceUserToReadWithReferenceTypeAll = CreateDocTypeWithForceUserToRead(Core.Constants.ReferenceTypes.All, "FUR");
				}
				return fDocTypeWithForceUserToReadWithReferenceTypeAll;
			}
		}
		RefDocType fDocTypeWithForceUserToReadWithReferenceTypeAll;

		RefDocType DocTypeWithForceUserToReadWithCategoryMapping
		{
			get
			{
				if (fDocTypeWithForceUserToReadWithCategoryMapping == null)
				{
					fDocTypeWithForceUserToReadWithCategoryMapping = CreateDocTypeWithForceUserToRead(Core.Constants.ReferenceTypes.ClientSupplierRelationship, "FUR");
				}
				return fDocTypeWithForceUserToReadWithCategoryMapping;
			}
		}
		RefDocType fDocTypeWithForceUserToReadWithCategoryMapping;

		RefDocType CreateDocTypeWithForceUserToRead(ZString referenceType, ZString docType)
		{
			RefDocType result = Factory.New<RefDocType>();
			result.RT_ForceUserToRead = true;
			result.RT_ReferenceType = referenceType;
			result.RT_DocType = docType;
			result.RT_Desc = "Test doc type";
			return result;
		}

		string SamplePdfPath
		{
			get { return Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\MasterFiles\Business\MasterFiles.Business\System\DocManager\TestFiles\Sample.PDF"); }
		}

		#endregion
	}
}
