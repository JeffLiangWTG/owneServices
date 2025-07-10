using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.BuildTools.Testing;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using WTG.Shared.Dash.Common.Services;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefDocType))]
	sealed class RefDocTypeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCompiledLogMacro()
		{
			var docType = Factory.NewWithValidTestData<RefDocType>();
			docType.RT_LogMacro = "Hello <@data.Name>";

			var data = new { Name = "world" };
			var result = docType.EvaluateLogMacro(data);
			AssertEquals("Should point to the right thing", "Hello world", result.Item1);
			Assert("Since the macro is valid we shouldn't have any errors", !result.Item2.Any());

			docType.RT_LogMacro = "Goodbye <@data.Name>";
			result = docType.EvaluateLogMacro(data);
			AssertEquals("When the macro has changed the compiled one should", "Goodbye world", result.Item1);
			Assert("Since the macro is valid we shouldn't have any errors", !result.Item2.Any());

			docType.RT_LogMacro = "\"Goodbye <@data.DoesntExist>\"";
			result = docType.EvaluateLogMacro(data);
			Assert("Since the 'DoesntExist' property doesnt exist, we should recieve an error", result.Item2.Any());
		}

		public void TestChangingIsPublishedOfEDocs()
		{
			var documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProviderForTest>();
			BusinessObjectFactory documentFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(new BusinessObjectFactory());
			var iDocumentFactoryInstance = documentFactory as IDocumentFactoryForTest;
			using (documentFactory as IDisposable)
			{
				BusinessObjectFactory factoryOne = iDocumentFactoryInstance.GetFactory(1);
				TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
				TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
				TestCaseHelper.ClearTable(Db.DatabaseName + "_SD001.dbo." + StorageDocsSchema.Constants.TableName);

				RefDocType docType = documentFactory.NewWithValidTestData<RefDocType>();
				docType.RT_Desc = "Description";
				docType.RT_DocType = "BBB";
				docType.RT_ReferenceType = "SHP";
				docType.RT_IsPublished = true;
				docType.ConfirmUpdate += new ConfirmEventHandler(OnDocType_ConfirmUpdate);
				docType.Factory.Save();

				OrgHeader org1 = documentFactory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
				OrgHeader org2 = documentFactory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));

				BusinessObject storageMainOnDB1 = documentFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(IStorageMain)));
				storageMainOnDB1[StorageMainSchema.Constants.SM_DB] = 1;
				storageMainOnDB1[StorageMainSchema.Constants.SM_Type] = "SHP";
				storageMainOnDB1[StorageMainSchema.Constants.SM_ParentFK] = org1.PK;

				BusinessObject anotherStorageMainOnDB1 = documentFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(IStorageMain)));
				anotherStorageMainOnDB1[StorageMainSchema.Constants.SM_DB] = 1;
				anotherStorageMainOnDB1[StorageMainSchema.Constants.SM_Type] = "BKG";
				anotherStorageMainOnDB1[StorageMainSchema.Constants.SM_ParentFK] = org2.PK;

				BusinessObject storageDocs_IsPublished_OnDB1_BBB = CreateDocument(iDocumentFactoryInstance, storageMainOnDB1.PK);
				storageDocs_IsPublished_OnDB1_BBB[StorageDocsSchema.Constants.SC_DocType] = "BBB";
				storageDocs_IsPublished_OnDB1_BBB[StorageDocsSchema.Constants.SC_IsPublished] = true;

				BusinessObject storageDocs_IsNotPublished_OnDB1_BBB = CreateDocument(iDocumentFactoryInstance, storageMainOnDB1.PK);
				storageDocs_IsNotPublished_OnDB1_BBB[StorageDocsSchema.Constants.SC_DocType] = "BBB";
				storageDocs_IsNotPublished_OnDB1_BBB[StorageDocsSchema.Constants.SC_IsPublished] = false;

				BusinessObject storageDocs_IsPublished_OnDB1_CCC = CreateDocument(iDocumentFactoryInstance, storageMainOnDB1.PK);
				storageDocs_IsPublished_OnDB1_CCC[StorageDocsSchema.Constants.SC_DocType] = "CCC";
				storageDocs_IsPublished_OnDB1_CCC[StorageDocsSchema.Constants.SC_IsPublished] = true;

				BusinessObject storageDocs_IsNotPublished_OnDB1_CCC = CreateDocument(iDocumentFactoryInstance, storageMainOnDB1.PK);
				storageDocs_IsNotPublished_OnDB1_CCC[StorageDocsSchema.Constants.SC_DocType] = "CCC";
				storageDocs_IsNotPublished_OnDB1_CCC[StorageDocsSchema.Constants.SC_IsPublished] = false;

				BusinessObject anotherStorageDocs_IsPublished_OnDB1_BBB = CreateDocument(iDocumentFactoryInstance, anotherStorageMainOnDB1.PK);
				anotherStorageDocs_IsPublished_OnDB1_BBB[StorageDocsSchema.Constants.SC_DocType] = "BBB";
				anotherStorageDocs_IsPublished_OnDB1_BBB[StorageDocsSchema.Constants.SC_IsPublished] = true;

				BusinessObject anotherStorageDocs_IsNotPublished_OnDB1_BBB = CreateDocument(iDocumentFactoryInstance, anotherStorageMainOnDB1.PK);
				anotherStorageDocs_IsNotPublished_OnDB1_BBB[StorageDocsSchema.Constants.SC_DocType] = "BBB";
				anotherStorageDocs_IsNotPublished_OnDB1_BBB[StorageDocsSchema.Constants.SC_IsPublished] = false;

				BusinessObject anotherStorageDocs_IsPublished_OnDB1_CCC = CreateDocument(iDocumentFactoryInstance, anotherStorageMainOnDB1.PK);
				anotherStorageDocs_IsPublished_OnDB1_CCC[StorageDocsSchema.Constants.SC_DocType] = "CCC";
				anotherStorageDocs_IsPublished_OnDB1_CCC[StorageDocsSchema.Constants.SC_IsPublished] = true;

				BusinessObject anotherStorageDocs_IsNotPublished_OnDB1_CCC = CreateDocument(iDocumentFactoryInstance, anotherStorageMainOnDB1.PK);
				anotherStorageDocs_IsNotPublished_OnDB1_CCC[StorageDocsSchema.Constants.SC_DocType] = "CCC";
				anotherStorageDocs_IsNotPublished_OnDB1_CCC[StorageDocsSchema.Constants.SC_IsPublished] = false;

				documentFactory.Save();

				AssertEquals("Two StorageMain must exist on db 1", 2, documentFactory.GetDatabaseCount(ObjectFactory.GetType<IStorageMain>()));

				AssertEquals("Documents count on Main Factory", 0, documentFactory.GetDatabaseCount(ObjectFactory.GetType<IStorageDocs>()));
				AssertEquals("Documents count on db 1", 8, factoryOne.GetDatabaseCount(ObjectFactory.GetType<IStorageDocs>()));

				AssertEquals("four of the StorageDocs on db 1 is IsPublished", 4, factoryOne.GetDatabaseCount(ObjectFactory.GetType<IStorageDocs>(), new ZQuery(StorageDocsSchema.SC_IsPublished, ZBool.True)));

				docType.RT_IsPublished = false;
				docType.Factory.Save();
				AssertEquals("Now six StorageDocs have IsPublished = false on db 1", 6, factoryOne.GetDatabaseCount(ObjectFactory.GetType<IStorageDocs>(), new ZQuery(StorageDocsSchema.SC_IsPublished, ZBool.False)));

				docType.RT_IsPublished = true;
				docType.Factory.Save();
				AssertEquals("Now two StorageDocs has IsPublished = false on db 1", 2, factoryOne.GetDatabaseCount(ObjectFactory.GetType<IStorageDocs>(), new ZQuery(StorageDocsSchema.SC_IsPublished, ZBool.False)));
			}
		}

		public void TestChangingDocTypeAndDescriptionOfEDocs()
		{
			var documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProviderForTest>();
			var documentFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(new BusinessObjectFactory());
			var iDocumentFactoryInstance = documentFactory as IDocumentFactoryForTest;

			using (documentFactory as IDisposable)
			{
				var factoryOne = iDocumentFactoryInstance.GetFactory(1);

				TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
				TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
				TestCaseHelper.ClearTable($"{Db.DatabaseName}_SD001.dbo.{StorageDocsSchema.Constants.TableName}");

				var docTypeForCSR = documentFactory.NewWithValidTestData<RefDocType>();
				docTypeForCSR.RT_Desc = "AAA-CSR";
				docTypeForCSR.RT_DocType = "AAA";
				docTypeForCSR.RT_ReferenceType = ReferenceTypes.ClientSupplierRelationship;
				docTypeForCSR.ConfirmUpdate += new ConfirmEventHandler(OnDocType_ConfirmUpdate);

				var docTypeForALL = documentFactory.NewWithValidTestData<RefDocType>();
				docTypeForALL.RT_Desc = "DDD-ALL";
				docTypeForALL.RT_DocType = "DDD";
				docTypeForALL.RT_ReferenceType = ReferenceTypes.All;
				docTypeForALL.ConfirmUpdate += new ConfirmEventHandler(OnDocType_ConfirmUpdate);
				documentFactory.Save();

				var organizationKey = Guid.NewGuid();
				var companyCampaignKey = Guid.NewGuid();

				var storageMainForOrg = documentFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(IStorageMain)));
				storageMainForOrg[StorageMainSchema.Constants.SM_DB] = 1;
				storageMainForOrg[StorageMainSchema.Constants.SM_Type] = DocManagerCodes.Organisation;
				storageMainForOrg[StorageMainSchema.Constants.SM_ParentFK] = organizationKey;

				var storageMainForCompanyCampaign = documentFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(IStorageMain)));
				storageMainForCompanyCampaign[StorageMainSchema.Constants.SM_DB] = 1;
				storageMainForCompanyCampaign[StorageMainSchema.Constants.SM_Type] = DocManagerCodes.CompanyCampaign;
				storageMainForCompanyCampaign[StorageMainSchema.Constants.SM_ParentFK] = companyCampaignKey;
				documentFactory.Save();

				var doc1 = CreateDocument(iDocumentFactoryInstance, storageMainForOrg.PK);
				doc1[StorageDocsSchema.Constants.SC_DocType] = "DDD";
				doc1[StorageDocsSchema.Constants.SC_Desc] = "DDD-ALL";

				var doc2 = CreateDocument(iDocumentFactoryInstance, storageMainForOrg.PK);
				doc2[StorageDocsSchema.Constants.SC_DocType] = "AAA";
				doc2[StorageDocsSchema.Constants.SC_Desc] = "AAA-CSR";

				var doc3 = CreateDocument(iDocumentFactoryInstance, storageMainForOrg.PK);
				doc3[StorageDocsSchema.Constants.SC_DocType] = "DDD";
				doc3[StorageDocsSchema.Constants.SC_Desc] = "DDD-ALL";

				var doc4 = CreateDocument(iDocumentFactoryInstance, storageMainForCompanyCampaign.PK);
				doc4[StorageDocsSchema.Constants.SC_DocType] = "AAA";
				doc4[StorageDocsSchema.Constants.SC_Desc] = "AAA-CSR";

				documentFactory.Save();

				AssertEquals("Pre-Condition: 2 StorageMain must exist on main db", 2, documentFactory.GetDatabaseCount(ObjectFactory.GetType<IStorageMain>()));
				AssertEquals("Pre-Condition: No StorageDocs exist on main db", 0, documentFactory.GetDatabaseCount(ObjectFactory.GetType<IStorageDocs>()));
				AssertEquals("Pre-Condition: 4 StorageDocs exist on db", 4, factoryOne.GetDatabaseCount(ObjectFactory.GetType<IStorageDocs>()));

				AssertEquals("Pre-Condition: 2 StorageDocs have DocType=AAA and Desc=AAA-CSR on db", 2, factoryOne.GetDatabaseCount(ObjectFactory.GetType<IStorageDocs>(), new ZQuery(StorageDocsSchema.SC_DocType, "AAA").AddToFilter(StorageDocsSchema.SC_Desc, "AAA-CSR")));
				AssertEquals("Pre-Condition: 2 StorageDocs have DocType=DDD and Desc=DDD-ALL on db", 2, factoryOne.GetDatabaseCount(ObjectFactory.GetType<IStorageDocs>(), new ZQuery(StorageDocsSchema.SC_DocType, "DDD").AddToFilter(StorageDocsSchema.SC_Desc, "DDD-ALL")));

				docTypeForCSR.RT_DocType = "AA2";
				docTypeForCSR.Factory.Save();
				AssertEquals("2 StorageDocs have DocType=AA2 and Desc=AAA-CSR on db", 2, factoryOne.GetDatabaseCount(ObjectFactory.GetType<IStorageDocs>(), new ZQuery(StorageDocsSchema.SC_DocType, "AA2").AddToFilter(StorageDocsSchema.SC_Desc, "AAA-CSR")));

				docTypeForCSR.RT_Desc = "AAA-CSR-new";
				docTypeForCSR.Factory.Save();
				AssertEquals("2 StorageDocs have DocType=AA2 and Desc=AAA-CSR-new on db", 2, factoryOne.GetDatabaseCount(ObjectFactory.GetType<IStorageDocs>(), new ZQuery(StorageDocsSchema.SC_DocType, "AA2").AddToFilter(StorageDocsSchema.SC_Desc, "AAA-CSR-new")));

				docTypeForALL.RT_Desc = "DDD-ALL-new";
				docTypeForALL.RT_DocType = "DD2";
				docTypeForALL.Factory.Save();
				AssertEquals("2 StorageDocs have DocType=DD2 and Desc=DDD-ALL-new on db", 2, factoryOne.GetDatabaseCount(ObjectFactory.GetType<IStorageDocs>(), new ZQuery(StorageDocsSchema.SC_DocType, "DD2").AddToFilter(StorageDocsSchema.SC_Desc, "DDD-ALL-new")));
			}
		}

		public void TestChangingTruncatedDescOfEDocs()
		{
			var documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProviderForTest>();
			var documentFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(new BusinessObjectFactory());

			using (documentFactory as IDisposable)
			{
				var orgHeader = documentFactory.NewWithValidTestData<OrgHeader>();
				var confirmHandler = new ConfirmEventHandler(OnDocType_ConfirmUpdate);
				var notConfirmHandler = new ConfirmEventHandler((sender, e) => { return false; });

				var jobRequiredDocument1 = documentFactory.NewWithValidTestData<JobRequiredDocument>();
				jobRequiredDocument1.EQ_ParentID = orgHeader.PK;
				jobRequiredDocument1.EQ_ParentTableCode = orgHeader.TablePrefix;
				jobRequiredDocument1.ParentType = typeof(OrgHeader);
				jobRequiredDocument1.EQ_DocCategory = "CSR";
				jobRequiredDocument1.EQ_DocType = "DT1";
				jobRequiredDocument1.EQ_DocDescription = "DT1-CSR";
				jobRequiredDocument1.EQ_ValidToDate = new ZDateTime(2018, 1, 1);

				var jobRequiredDocument2 = documentFactory.NewWithValidTestData<JobRequiredDocument>();
				jobRequiredDocument2.EQ_ParentID = orgHeader.PK;
				jobRequiredDocument2.EQ_ParentTableCode = orgHeader.TablePrefix;
				jobRequiredDocument2.ParentType = typeof(OrgHeader);
				jobRequiredDocument2.EQ_DocCategory = "ALL";
				jobRequiredDocument2.EQ_DocType = "DT1";
				jobRequiredDocument2.EQ_DocDescription = "DT1-ALL";
				jobRequiredDocument2.EQ_ValidToDate = new ZDateTime(2018, 1, 1);

				var jobRequiredDocument3 = documentFactory.NewWithValidTestData<JobRequiredDocument>();
				jobRequiredDocument3.EQ_ParentID = orgHeader.PK;
				jobRequiredDocument3.EQ_ParentTableCode = orgHeader.TablePrefix;
				jobRequiredDocument3.ParentType = typeof(OrgHeader);
				jobRequiredDocument3.EQ_DocCategory = "SCL";
				jobRequiredDocument3.EQ_DocType = "DT1";
				jobRequiredDocument3.EQ_DocDescription = "DT1-SCL";
				jobRequiredDocument3.EQ_ValidToDate = new ZDateTime(2018, 1, 1);

				var refDocType = documentFactory.NewWithValidTestData<RefDocType>();
				refDocType.RT_ReferenceType = "CSR";
				refDocType.RT_DocType = "DT1";
				refDocType.RT_Desc = "DT1-CSR";
				refDocType.ConfirmUpdate += confirmHandler;

				var refDocType2 = documentFactory.NewWithValidTestData<RefDocType>();
				refDocType2.RT_ReferenceType = "ALL";
				refDocType2.RT_DocType = "DT1";
				refDocType2.RT_Desc = "DT1-ALL";
				refDocType2.ConfirmUpdate += confirmHandler;

				documentFactory.Save();

				refDocType.Reload();
				refDocType.RT_Desc = "123456789012345678901234567890123450";

				refDocType2.Reload();
				refDocType2.RT_Desc = "12345678901234567890123456789012345";

				documentFactory.Save();

				jobRequiredDocument1.Reload();
				AssertEquals("Description should be updated with truncated value.", "12345678901234567890123456789012345", jobRequiredDocument1.EQ_DocDescriptionMultilingual);
				jobRequiredDocument2.Reload();
				AssertEquals("Description should be updated with not truncated value.", "12345678901234567890123456789012345", jobRequiredDocument2.EQ_DocDescriptionMultilingual);
				jobRequiredDocument3.Reload();
				AssertEquals("Description should be updated with not truncated value.", "12345678901234567890123456789012345", jobRequiredDocument3.EQ_DocDescriptionMultilingual);

				refDocType.Reload();
				refDocType.RT_Desc = "DT1-CSR";
				documentFactory.Save();

				refDocType.Reload();
				refDocType.RT_Desc = "123456789012345678901234567890123450";
				refDocType.ConfirmUpdate -= confirmHandler;
				refDocType.ConfirmUpdate += notConfirmHandler;

				documentFactory.Save();
				jobRequiredDocument1.Reload();
				AssertEquals("Description should not be updated.", "DT1-CSR", jobRequiredDocument1.EQ_DocDescriptionMultilingual);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		#region Business Object Overrides

		public void TestClone()
		{
			DocType.RT_Desc = "ABC Description";
			DocType.RT_DocType = "AAA";
			DocType.RT_ReferenceType = "SHP";
			DocType.RT_IsActive = true;
			DocType.RT_IsPublished = true;
			DocType.RT_IsPublishUpdatable = false;
			DocType.RT_SaveVersions = true;

			RefDocType clonedDocType = (RefDocType)DocType.Clone();
			AssertEquals("Cloned object - property should be equal to the original", DocType.RT_Desc, clonedDocType.RT_Desc);
			AssertEquals("Cloned object - property should be equal to the original", DocType.RT_DocType, clonedDocType.RT_DocType);
			AssertEquals("Cloned object - property should be equal to the original", DocType.RT_ReferenceType, clonedDocType.RT_ReferenceType);
			AssertEquals("Cloned object - property should be equal to the original", DocType.RT_IsActive, clonedDocType.RT_IsActive);
			AssertEquals("Cloned object - property should be equal to the original", DocType.RT_IsPublished, clonedDocType.RT_IsPublished);
			AssertEquals("Cloned object - property should be equal to the original", DocType.RT_IsPublishUpdatable, clonedDocType.RT_IsPublishUpdatable);
			AssertEquals("Cloned object - property should be equal to the original", DocType.RT_SaveVersions, clonedDocType.RT_SaveVersions);
		}

		public void TestOnLoaded()
		{
			CombineAssertions(delegate
			{
				AssertEquals("not system doc - RT_IsSystemInfo.ReadOnly", true, DocType.RT_IsSystemInfo.ReadOnly);
				AssertEquals("not system doc - RT_IsActiveInfo.ReadOnly", false, DocType.RT_IsActiveInfo.ReadOnly);
				AssertEquals("not system doc - RT_DescInfo.ReadOnly", false, DocType.RT_DescInfo.ReadOnly);
				AssertEquals("not system doc - RT_ReferenceTypeInfo.ReadOnly", false, DocType.RT_ReferenceTypeInfo.ReadOnly);
				AssertEquals("not system doc - RT_IsPublishedInfo.ReadOnly", false, DocType.RT_IsPublishedInfo.ReadOnly);
				AssertEquals("not system doc - RT_IsPublishUpdatableInfo.ReadOnly", false, DocType.RT_IsPublishUpdatableInfo.ReadOnly);
				AssertEquals("not system doc - RT_SaveVersionsInfo.ReadOnly", false, DocType.RT_SaveVersionsInfo.ReadOnly);
			});

			DocType.RT_IsSystem = true;
			DocType.OnLoaded();
			CombineAssertions(delegate
			{
				AssertEquals("system doc - RT_IsSystemInfo.ReadOnly", true, DocType.RT_IsSystemInfo.ReadOnly);
				AssertEquals("system doc - RT_IsActiveInfo.ReadOnly", false, DocType.RT_IsActiveInfo.ReadOnly);
				AssertEquals("system doc - RT_DescInfo.ReadOnly", true, DocType.RT_DescInfo.ReadOnly);
				AssertEquals("system doc - RT_ReferenceTypeInfo.ReadOnly", true, DocType.RT_ReferenceTypeInfo.ReadOnly);
			});

			DocType.SetIsCheckedOutByMeForTesting(true);
			CombineAssertions(delegate
			{
				AssertEquals("documents checked out - RT_IsSystemInfo.ReadOnly", false, DocType.RT_IsSystemInfo.ReadOnly);
				AssertEquals("documents checked out - RT_IsActiveInfo.ReadOnly", false, DocType.RT_IsActiveInfo.ReadOnly);
				AssertEquals("documents checked out - RT_DescInfo.ReadOnly", false, DocType.RT_DescInfo.ReadOnly);
				AssertEquals("documents checked out - RT_ReferenceTypeInfo.ReadOnly", false, DocType.RT_ReferenceTypeInfo.ReadOnly);
				AssertEquals("documents checked out - RT_IsPublishedInfo.ReadOnly", false, DocType.RT_IsPublishedInfo.ReadOnly);
				AssertEquals("documents checked out - RT_IsPublishUpdatableInfo.ReadOnly", false, DocType.RT_IsPublishUpdatableInfo.ReadOnly);
				AssertEquals("documents checked out - RT_SaveVersionsInfo.ReadOnly", false, DocType.RT_SaveVersionsInfo.ReadOnly);
			});
		}

		#endregion

		#region Properties

		#region IsArchived

		public void TestIsArchived()
		{
			Assert("OnLoaded, IsArchived is false", !DocType.IsArchived);
			DocType.IsArchived = true;
			Assert("DocType.IsArchived should be true", DocType.IsArchived);
		}

		#endregion

		#region RT_OverrideVersions

		public void TestRT_OverrideVersions()
		{
			AssertEquals(DocType.RT_SaveVersions, !DocType.RT_OverrideVersions);
			DocType.RT_OverrideVersions = !DocType.RT_OverrideVersions;
			AssertEquals(DocType.RT_SaveVersions, !DocType.RT_OverrideVersions);
			DocType.RT_SaveVersions = !DocType.RT_SaveVersions;
			AssertEquals(DocType.RT_SaveVersions, !DocType.RT_OverrideVersions);
		}

		#endregion

		#endregion

		#region ReadOnlyPropertiesForEHB

		public void TestReadOnlyPropertiesForSEHB()
		{
			DocType.RT_DocType = RefDocTypeLookups.Codes.ElectronicHouseBill;
			DocType.SetIsCheckedOutByMeForTesting(false);
			AssertReadOnlyPropertiesForSEHB(true);

			DocType.SetIsCheckedOutByMeForTesting(true);
			AssertReadOnlyPropertiesForSEHB(false);

			DocType.RT_DocType = "OTH";
			DocType.SetIsCheckedOutByMeForTesting(false);
			AssertReadOnlyPropertiesForSEHB(false);

			DocType.SetIsCheckedOutByMeForTesting(true);
			AssertReadOnlyPropertiesForSEHB(false);
		}

		void AssertReadOnlyPropertiesForSEHB(bool isReadOnly)
		{
			AssertEquals(!DocType.IsCheckedOutByMe, DocType.RT_IsSystem_ReadOnly);
			AssertEquals(isReadOnly, DocType.RT_IsActive_ReadOnly);
			AssertEquals(isReadOnly, DocType.RT_IsPublished_ReadOnly);
			AssertEquals(isReadOnly, DocType.RT_IsPublishUpdatable_ReadOnly);
			AssertEquals(isReadOnly, DocType.RT_AllowMultiplePeriodicDocs_ReadOnly);
			AssertEquals(isReadOnly, DocType.RT_IsCompanySpecific_ReadOnly);
			AssertEquals(isReadOnly, DocType.RT_IsBranchSpecific_ReadOnly);
			AssertEquals(isReadOnly, DocType.RT_IsDepartmentSpecific_ReadOnly);
		}

		#endregion

		#region ForceToReadReferenceTypes

		public void TestForceToReadReferenceTypes()
		{
			DocType.RT_DocType = "AAA";
			DocType.RT_ReferenceType = "FCE";
			DocType.RT_ForceUserToRead = true;

			Factory.Save();
			AssertEquals("Should find the reference type with 'force user to read' document type", true, ((IList<ZString>)RefDocType.ForceToReadReferenceTypes).Contains("FCE"));

			DocType.RT_ReferenceType = "XXX";
			Factory.Save();
			AssertEquals("Cache should be invalidated after a Factory.Save()", false, ((IList<ZString>)RefDocType.ForceToReadReferenceTypes).Contains("FCE"));
			AssertEquals("Cache should be invalidated after a Factory.Save()", true, ((IList<ZString>)RefDocType.ForceToReadReferenceTypes).Contains("XXX"));

			DocType.Delete();
			AssertEquals("Cache should not be invalidated until the save", true, ((IList<ZString>)RefDocType.ForceToReadReferenceTypes).Contains("XXX"));
			Factory.Save();
			AssertEquals("Should not find the reference type after delete", false, ((IList<ZString>)RefDocType.ForceToReadReferenceTypes).Contains("FCE"));
			AssertEquals("Should not find the reference type after delete", false, ((IList<ZString>)RefDocType.ForceToReadReferenceTypes).Contains("XXX"));
		}

		[TestDate]
		public void TestForceToReadReferenceTypes_CacheInvalidatedAfter30Minutes()
		{
			DocType.RT_DocType = "AAA";
			DocType.RT_ReferenceType = "FCE";
			DocType.RT_ForceUserToRead = true;

			Factory.Save();
			AssertEquals("Should find the reference type with 'force user to read' document type", true, ((IList<ZString>)RefDocType.ForceToReadReferenceTypes).Contains("FCE"));

			Db.Connection.ExecuteNonQuery("UPDATE dbo.RefDocType SET RT_ForceUserToRead = 0 WHERE RT_PK='" + DocType.PK + "'");

			TestDateAttribute.Date = ZDateTime.Now.AddMinutes(29).ToDateTime();
			AssertEquals("Cache should not be invalidated yet", true, ((IList<ZString>)RefDocType.ForceToReadReferenceTypes).Contains("FCE"));

			TestDateAttribute.Date = ZDateTime.Now.AddMinutes(2).ToDateTime();
			AssertEquals("Cache should be invalidated after 30 minutes", false, ((IList<ZString>)RefDocType.ForceToReadReferenceTypes).Contains("FCE"));
		}

		#endregion

		#region Lookups

		public void TestRT_ReferenceType_List()
		{
			Assert("List shouldn't contain UNA code", !DocType.RT_ReferenceType_List.ContainsCode("UNA"));
			Assert("List should contain ALL code", DocType.RT_ReferenceType_List.ContainsCode("ALL"));
		}
		public void TestRT_ParseType_List()
		{
			var parseTypes = ObjectFactory.Get<IDashParametersService>().SupportedParseTypes.ToArray();
			var parseTypeList = DocType.RT_ParseType_List;
			AssertEquals("Parse type list count should be the same to the count of parse types", parseTypes.Length, parseTypeList.Count);

			foreach (var parseType in parseTypes)
			{
				var description = parseTypeList.GetDescriptionFromCode(parseType.TypeCode);
				AssertEquals(parseType.Description, description);
			}
		}

		#endregion

		#region IRefDocType

		public void TestIRefDocType()
		{
			var refDocType = Factory.New<RefDocType>();
			refDocType.RT_Desc = "blah";
			refDocType.RT_DocType = "beh";
			refDocType.RT_ReferenceType = "RRR";

			var iRefDocType = (IRefDocType)refDocType;
			AssertEquals(iRefDocType.PK, refDocType.PK);
			AssertEquals(iRefDocType.RT_Desc, refDocType.RT_Desc);
			AssertEquals(iRefDocType.RT_DocType, refDocType.RT_DocType);
			AssertEquals(iRefDocType.RT_ReferenceType, refDocType.RT_ReferenceType);
		}

		#endregion

		#region ITemplateCopyable

		public void TestTemplateCopy()
		{
			RefDocType copiedDoc = (RefDocType)DocType.TemplateCopy();
			Assert(!copiedDoc.RT_IsSystem);
			Assert(copiedDoc.RT_Desc.IsEmpty);
			Assert(copiedDoc.RT_DocType.IsEmpty);
			AssertEquals(DocType.RT_IsActive, copiedDoc.RT_IsActive);
			AssertEquals(DocType.RT_IsPublished, copiedDoc.RT_IsPublished);
			AssertEquals(DocType.RT_IsPublishUpdatable, copiedDoc.RT_IsPublishUpdatable);
		}

		#endregion

		#region Implementation

		RefDocType DocType;

		BusinessObject CreateDocument(IDocumentFactoryForTest docFactory, ZGuid storageMainPK)
		{
			BusinessObject bizO = docFactory.CreateDocument(storageMainPK);
			bizO[StorageDocsSchema.Constants.SC_Date] = ZDateTime.Now;
			bizO[StorageDocsSchema.Constants.SC_ImageData] = new ZBlob(new byte[] { 1, 2, 3 });
			return bizO;
		}

		bool OnDocType_ConfirmUpdate(RefDocType sender, ConfirmEventArgs e)
		{
			return true;
		}

		protected override void SetUp()
		{
			base.SetUp();
			DocType = Factory.New<RefDocType>();
			DocType.RT_DocType = "AAA";
			DocType.RT_ReferenceType = "ALL";
			MockSourceControl.Setup();
		}

		protected override void TearDown()
		{
			base.TearDown();
			MockSourceControl.TearDown();
		}

		public void TestGetExistingInAnyCategoryOtherThanAll()
		{
			RefDocType doctype1 = Factory.New<RefDocType>();
			doctype1.RT_Desc = "desc1";
			doctype1.RT_DocType = "YYY";
			doctype1.RT_ReferenceType = "SCL";
			Factory.Save();

			RefDocType doctype2 = Factory.New<RefDocType>();
			doctype2.RT_Desc = "desc2";
			doctype2.RT_DocType = "789";
			doctype2.RT_ReferenceType = "BPW";
			doctype2.Validation.ValidateRT_DocType();

			Assert(!doctype2.HasErrors);

			RefDocType doctype3 = Factory.New<RefDocType>();
			doctype3.RT_Desc = "desc3";
			doctype3.RT_DocType = "YYY";
			doctype3.RT_ReferenceType = "ALL";
			RefDocType[] types = doctype3.GetDuplicatedDocTypes();

			AssertEquals("Should be one that can cause duplication", 1, types.Length);
			AssertEquals("Should be the proper one", doctype1.PK, types[0].PK);
		}

		public void TestCheckIfViolatingUniqueIndex()
		{
			StmMenuItem mu1 = Factory.NewWithValidTestData<StmMenuItem>();
			StmMenuItem mu2 = Factory.NewWithValidTestData<StmMenuItem>();
			RefDocType docType1 = Factory.NewWithValidTestData<RefDocType>();
			docType1.RT_DocType = "AA1";
			docType1.RT_ReferenceType = "ALL";

			RefDocType docType2 = Factory.NewWithValidTestData<RefDocType>();
			docType2.RT_DocType = "AA2";
			docType2.RT_ReferenceType = "ALL";

			RefDocType docType3 = Factory.NewWithValidTestData<RefDocType>();
			docType3.RT_DocType = "AA3";
			docType3.RT_ReferenceType = "ALL";

			StmMenuEDocs edoc1 = Factory.NewWithValidTestData<StmMenuEDocs>();
			edoc1.SX_SU = mu1.PK;
			edoc1.SX_RT_DocType = docType1.PK;

			StmMenuEDocs edoc2 = Factory.NewWithValidTestData<StmMenuEDocs>();
			edoc2.SX_SU = mu1.PK;
			edoc2.SX_RT_DocType = docType2.PK;

			StmMenuEDocs edoc3 = Factory.NewWithValidTestData<StmMenuEDocs>();
			edoc3.SX_SU = mu2.PK;
			edoc3.SX_RT_DocType = docType3.PK;

			Factory.Save();

			Assert(docType2.Validation.CheckIfViolatingUniqueIndexStmMenuEDocs(mu1.PK, docType1.PK));
			Assert(!docType3.Validation.CheckIfViolatingUniqueIndexStmMenuEDocs(mu2.PK, docType1.PK));
		}

		#endregion
	}
}
