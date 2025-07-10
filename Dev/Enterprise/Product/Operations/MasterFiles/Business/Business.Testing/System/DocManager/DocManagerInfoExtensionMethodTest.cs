using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DocManagerInfoExtensionMethodTest : TestCaseWithFactory
	{
		public void TestGetRelatedEDocs()
		{
			var storageMain = GetStorageMain((info) => info.GetRelatedEDocs().Count());
			AssertNotNull("StorageMain should not be null", storageMain);
		}

		public void TestGetRelatedEDocsView()
		{
			var storageMain = GetStorageMain((info) => info.GetRelatedEDocsView().Count());
			AssertNull("StorageMain should be null", storageMain);
		}

		IStorageMain GetStorageMain(Func<DocManagerInfo,int> eDocsCount)
		{
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(Db.DatabaseName + "_SD001.dbo." + StorageDocsSchema.Constants.TableName);
			var documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			var documentFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(Factory);
			var header = documentFactory.NewWithValidTestData<OrgHeader>();
			header.Addresses.AddNew();
			var info = ((IDocManagerSupport)header).DocManagerInfo;
			AssertEquals("EDocsView should be empty", 0, eDocsCount(info));
			info.Save();

			var filter = new ZQuery(StorageMainSchema.SM_ParentFK, header.PK);
			return documentFactory.LoadTop1<IStorageMain>(filter);
		}
	}
}
