using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobRequiredDocument))]
	sealed class JobRequiredDocumentTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCreationUNLOCOCode()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sinBranchPk, Env.CurrentDepartmentPK))
			{
				var requiredDocument = Factory.NewWithValidTestData<JobRequiredDocument>();
				Factory.Save();
				AssertEquals("SGSIN", requiredDocument.CreationUNLOCOCode);
			}
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sydBranchPk, Env.CurrentDepartmentPK))
			{
				var requiredDocument = Factory.NewWithValidTestData<JobRequiredDocument>();
				Factory.Save();
				AssertEquals("AUSYD", requiredDocument.CreationUNLOCOCode);
			}
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, tesBranchPk, Env.CurrentDepartmentPK))
			{
				var requiredDocument = Factory.NewWithValidTestData<JobRequiredDocument>();
				Factory.Save();
				var addedLog = requiredDocument.GetFirstMatchingLog(new ZQuery(StmALogSchema.SL_SE_NKEvent, EventCodes.AddedARecordToTheSystem));
				addedLog.SL_GB_NKBranch = "";
				AssertEquals("", requiredDocument.CreationUNLOCOCode);
			}
		}

		[TestDate(2023, 1, 3, 15, 0, 0)]
		public void TestDateReceivedLocalAndUtc()
		{
			TestDateAttribute.UseUNLOCO = true;
			JobRequiredDocument requiredDocument;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sydBranchPk, Env.CurrentDepartmentPK))
			{
				requiredDocument = Factory.NewWithValidTestData<JobRequiredDocument>();
				requiredDocument.EQ_DateReceived = ZDateTimeOffset.Now;
				Factory.Save();
				AssertEquals(ZDateTimeOffset.Now, requiredDocument.EQ_DateReceived);
				AssertEquals(ZDateTime.UtcNow, requiredDocument.EQ_DateReceivedUtc);
			}
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sinBranchPk, Env.CurrentDepartmentPK))
			{
				var docInAnotherBranch = NewFactory().Load<JobRequiredDocument>(requiredDocument.PK);
				AssertEquals(ZDateTimeOffset.Now, docInAnotherBranch.EQ_DateReceived);
				AssertEquals(ZDateTime.UtcNow, docInAnotherBranch.EQ_DateReceivedUtc);
			}
		}

		[TestDate(2023, 1, 3, 15, 0, 0)]
		public void TestDateReceivedUtc()
		{
			TestDateAttribute.UseUNLOCO = true;
			JobRequiredDocument requiredDocument;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sydBranchPk, Env.CurrentDepartmentPK))
			{
				requiredDocument = Factory.NewWithValidTestData<JobRequiredDocument>();
				requiredDocument.EQ_DateReceivedUtc = ZDateTime.UtcNow;
				Factory.Save();
				AssertEquals(ZDateTimeOffset.Now, requiredDocument.EQ_DateReceived);
				AssertEquals(ZDateTime.UtcNow, requiredDocument.EQ_DateReceivedUtc);
			}
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sinBranchPk, Env.CurrentDepartmentPK))
			{
				var docInAnotherBranch = NewFactory().Load<JobRequiredDocument>(requiredDocument.PK);
				AssertEquals(ZDateTimeOffset.Now, docInAnotherBranch.EQ_DateReceived);
				AssertEquals(ZDateTime.UtcNow, docInAnotherBranch.EQ_DateReceivedUtc);
			}
		}

		void TestDateReceivedCreatedAtUtc(bool isTransformCompleted)
		{
			TestDateAttribute.UseUNLOCO = true;
			JobRequiredDocument requiredDocument;
			var ukBranchPk = SetUpUKBranch();

			if (!isTransformCompleted)
			{
				SetDateReceivedOffsetTransformIsRunningFlag();
			}
			else
			{
				RemoveDateReceivedOffsetTransformIsRunningFlag();
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, ukBranchPk, Env.CurrentDepartmentPK))
			{
				requiredDocument = Factory.NewWithValidTestData<JobRequiredDocument>();
				requiredDocument.EQ_DateReceivedUtc = ZDateTime.UtcNow;
				Factory.Save();
				AssertEquals(ZDateTimeOffset.Now, requiredDocument.EQ_DateReceived);
				AssertEquals(ZDateTime.UtcNow, requiredDocument.EQ_DateReceivedUtc);
				AssertEquals(0d, requiredDocument.EQ_DateReceived.Offset.TotalSeconds);

				if (isTransformCompleted)
				{
					AssertEquals(3, Factory.GetTableHitCount(StmDataSchema.Constants.TableName));
				}
				else
				{
					AssertEquals(14, Factory.GetTableHitCount(StmDataSchema.Constants.TableName));
				}
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sinBranchPk, Env.CurrentDepartmentPK))
			{
				var factory = NewFactory();
				var docInSinBranch = factory.Load<JobRequiredDocument>(requiredDocument.PK);
				AssertEquals(ZDateTimeOffset.Now, docInSinBranch.EQ_DateReceived);
				AssertEquals(ZDateTime.UtcNow, docInSinBranch.EQ_DateReceivedUtc);

				docInSinBranch.EQ_DateReceivedUtc = ZDateTime.UtcNow.AddDays(1);
				factory.Save();

				if (isTransformCompleted)
				{
					AssertEquals(0, factory.GetTableHitCount(StmDataSchema.Constants.TableName));
				}
				else
				{
					AssertEquals(12, factory.GetTableHitCount(StmDataSchema.Constants.TableName));
				}
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, ukBranchPk, Env.CurrentDepartmentPK))
			{
				var testRequiredDocument = Factory.Load<TestJobRequiredDocument>(requiredDocument.PK);
				AssertEquals(ZDateTimeOffset.Now.AddDays(1), testRequiredDocument.EQ_DateReceived);
				AssertNotEquals(0d, testRequiredDocument.EQ_DateReceived.Offset.TotalSeconds);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, ukBranchPk, Env.CurrentDepartmentPK))
			{
				requiredDocument.EQ_DateReceivedUtc = ZDateTime.UtcNow;
				Factory.Save();
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sydBranchPk, Env.CurrentDepartmentPK))
			{
				var factory = NewFactory();
				var docInSydBranch = factory.Load<TestJobRequiredDocument>(requiredDocument.PK);
				AssertEquals(ZDateTimeOffset.Now, docInSydBranch.EQ_DateReceived);
				AssertEquals(0d, docInSydBranch.EQ_DateReceived.Offset.TotalSeconds);
			}

			RemoveDateReceivedOffsetTransformIsRunningFlag();
		}

		[TestDate(2023, 1, 3, 15, 0, 0)]
		public void TestDateReceivedCreatedAtUtc()
		{
			TestDateReceivedCreatedAtUtc(false);
		}

		[TestDate(2023, 1, 3, 15, 0, 0)]
		public void TestDateReceivedCreatedAtUtcWithOffsetTransformCompleted()
		{
			TestDateReceivedCreatedAtUtc(true);
		}

		void TestDateReceivedCreatedAtSyd(bool isTransformCompleted)
		{
			TestDateAttribute.UseUNLOCO = true;
			JobRequiredDocument requiredDocument;
			var ukBranchPk = SetUpUKBranch();

			if (!isTransformCompleted)
			{
				SetDateReceivedOffsetTransformIsRunningFlag();
			}
			else
			{
				RemoveDateReceivedOffsetTransformIsRunningFlag();
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sydBranchPk, Env.CurrentDepartmentPK))
			{
				requiredDocument = Factory.NewWithValidTestData<JobRequiredDocument>();
				requiredDocument.EQ_DateReceivedUtc = ZDateTime.UtcNow;
				Factory.Save();
				AssertEquals(ZDateTimeOffset.Now, requiredDocument.EQ_DateReceived);
				AssertEquals(ZDateTime.UtcNow, requiredDocument.EQ_DateReceivedUtc);
				AssertNotEquals(0d, requiredDocument.EQ_DateReceived.Offset.TotalSeconds);

				if (isTransformCompleted)
				{
					AssertEquals(3, Factory.GetTableHitCount(StmDataSchema.Constants.TableName));
				}
				else
				{
					AssertEquals(13, Factory.GetTableHitCount(StmDataSchema.Constants.TableName));
				}
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, ukBranchPk, Env.CurrentDepartmentPK))
			{
				var factory = NewFactory();
				var docInUkBranch = factory.Load<JobRequiredDocument>(requiredDocument.PK);
				AssertEquals(ZDateTimeOffset.Now, docInUkBranch.EQ_DateReceived);
				AssertEquals(ZDateTime.UtcNow, docInUkBranch.EQ_DateReceivedUtc);
				AssertEquals(0d, docInUkBranch.EQ_DateReceived.Offset.TotalSeconds);

				docInUkBranch.EQ_DateReceivedUtc = ZDateTime.UtcNow.AddDays(1);
				factory.Save();

				if (isTransformCompleted)
				{
					AssertEquals(0, factory.GetTableHitCount(StmDataSchema.Constants.TableName));
				}
				else
				{
					AssertEquals(12, factory.GetTableHitCount(StmDataSchema.Constants.TableName));
				}
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sydBranchPk, Env.CurrentDepartmentPK))
			{
				var testRequiredDocument = Factory.Load<TestJobRequiredDocument>(requiredDocument.PK);
				AssertEquals(ZDateTimeOffset.Now.AddDays(1), testRequiredDocument.EQ_DateReceived);
				if (isTransformCompleted)
				{
					AssertEquals(0d, testRequiredDocument.EQ_DateReceived.Offset.TotalSeconds);
				}
				else
				{
					AssertNotEquals(0d, testRequiredDocument.EQ_DateReceived.Offset.TotalSeconds);
				}

				requiredDocument = Factory.Load<JobRequiredDocument>(requiredDocument.PK);
				AssertEquals(ZDateTime.UtcNow.AddDays(1), requiredDocument.EQ_DateReceivedUtc);
				AssertEquals(11, requiredDocument.EQ_DateReceived.Offset.Hours);
			}

			RemoveDateReceivedOffsetTransformIsRunningFlag();
		}

		[TestDate(2023, 1, 3, 15, 0, 0)]
		public void TestDateReceivedCreatedAtSyd()
		{
			TestDateReceivedCreatedAtSyd(false);
		}

		[TestDate(2023, 1, 3, 15, 0, 0)]
		public void TestDateReceivedCreatedAtSydWithOffsetTransformCompleted()
		{
			TestDateReceivedCreatedAtSyd(true);
		}

		const string TransformIsRunningName = "AdjustTimezoneOffsetDateReceived.IsRunning";
		void SetDateReceivedOffsetTransformIsRunningFlag()
		{
			var cmd = @$"
IF NOT EXISTS (SELECT 1 FROM dbo.StmData WHERE SD_Name = '{TransformIsRunningName}')
BEGIN
	INSERT dbo.StmData(SD_PK, SD_Name) VALUES(newid(), '{TransformIsRunningName}')
END
";
			Db.Connection.ExecuteNonQuery(cmd);
		}

		void RemoveDateReceivedOffsetTransformIsRunningFlag()
		{
			var cmd = $"DELETE dbo.StmData WHERE SD_Name = '{TransformIsRunningName}'";
			Db.Connection.ExecuteNonQuery(cmd);
		}

		class TestJobRequiredDocument : AutoJobRequiredDocument
		{
			public TestJobRequiredDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		Guid SetUpUKBranch()
		{
			// Arrange
			var ukCompany = Factory.New<GlbCompany>();
			ukCompany.GC_Code = "DAN";
			ukCompany.GC_RN_NKCountryCode = "GB";
			var ukBranch = ukCompany.Branches.AddNew();
			ukBranch.GB_RL_NKHomePort = "GBLON";
			ukBranch.GB_Code = "DAN";
			Factory.Save();
			return ukBranch.PK.ToGuid();
		}

		readonly Guid sinBranchPk = new Guid("EF8CBDB5-9F53-4360-921E-C2929BF77A85");
		readonly Guid sydBranchPk = new Guid("FDD429D2-648C-4895-8F9F-06E90DED2BE5");
		readonly Guid tesBranchPk = new Guid("54226AD9-9E8A-4E29-A7F7-E73A7D18DDF1");

		public void TestSuspendDocTypeUniquenessCheck()
		{
			var docType = Factory.NewWithValidTestData<RefDocType>();
			docType.RT_DocType = RefDocTypes.AgentsInstruction;
			docType.RT_ReferenceType = "ALL";

			var requiredDoc = Factory.New<JobRequiredDocumentForTest>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			requiredDoc.ParentType = orgHeader.GetType();
			requiredDoc.EQ_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			requiredDoc.EQ_ParentID = orgHeader.PK;
			requiredDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Both;
			requiredDoc.EQ_DocType = RefDocTypes.AgentsInstruction;

			var duplicateRequiredDoc = Factory.New<JobRequiredDocumentForTest>();
			duplicateRequiredDoc.ParentType = orgHeader.GetType();
			duplicateRequiredDoc.EQ_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			duplicateRequiredDoc.EQ_ParentID = orgHeader.PK;
			duplicateRequiredDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Both;
			duplicateRequiredDoc.EQ_DocType = RefDocTypes.AgentsInstruction;

			Assert("precondition : IsDocTypeDuplicate is true", duplicateRequiredDoc.IsDocTypeDuplicate);

			using (duplicateRequiredDoc.SuspendDocTypeUniquenessCheck())
			{
				Assert("IsDocTypeDuplicate is false after SuspendDocTypeUniquenessCheck", !duplicateRequiredDoc.IsDocTypeDuplicate);
			}
		}

		public void TestIsDocTypeDuplicateCheckForCertificateOfOrigin()
		{
			var docType = Factory.NewWithValidTestData<RefDocType>();
			docType.RT_DocType = RefDocTypes.AgentsInstruction;
			docType.RT_ReferenceType = "ALL";

			var requiredDoc = Factory.New<JobRequiredDocumentForTest>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			requiredDoc.ParentType = orgHeader.GetType();
			requiredDoc.EQ_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			requiredDoc.EQ_ParentID = orgHeader.PK;
			requiredDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Both;
			requiredDoc.EQ_DocType = RefDocTypes.CertificateOfOrigin;
			var attribute1 = requiredDoc.Attributes.AddNew();
			attribute1.D0_AttribName = JobRequiredDocAttribTypeList.Codes.TradePreferenceCode;
			attribute1.D0_AttribValue = "10";

			var duplicateRequiredDoc = Factory.New<JobRequiredDocumentForTest>();
			duplicateRequiredDoc.ParentType = orgHeader.GetType();
			duplicateRequiredDoc.EQ_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			duplicateRequiredDoc.EQ_ParentID = orgHeader.PK;
			duplicateRequiredDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Both;
			duplicateRequiredDoc.EQ_DocType = RefDocTypes.CertificateOfOrigin;
			var attribute2 = duplicateRequiredDoc.Attributes.AddNew();

			Assert("IsDocTypeDuplicate is true", duplicateRequiredDoc.IsDocTypeDuplicate);

			attribute2.D0_AttribName = JobRequiredDocAttribTypeList.Codes.TradePreferenceCode;
			attribute2.D0_AttribValue = "01";
			Assert("IsDocTypeDuplicate is false", !duplicateRequiredDoc.IsDocTypeDuplicate);

			attribute2.D0_AttribValue = "10";
			Assert("IsDocTypeDuplicate is true", duplicateRequiredDoc.IsDocTypeDuplicate);
		}

		[UseSnapshotProtection]
		public void TestShouldThrowConcurrencyExceptionWhenTwoDocumentsUpdateSameJobRequiredDocument()
		{
			ZGuid orgheaderPK = ZGuid.Empty;
			using (var conn = Db.NewAdminConnection())
			{
				var factory = new BusinessObjectFactory(conn);
				var orgHeader = factory.NewWithValidTestData<OrgHeader>();
				var requiredDoc = factory.New<JobRequiredDocumentForTest>();
				requiredDoc.EQ_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
				requiredDoc.EQ_ParentID = orgHeader.PK;
				requiredDoc.ParentType = orgHeader.GetType();
				requiredDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Both;
				requiredDoc.EQ_DocType = RefDocTypes.AgentsInstruction;
				factory.Save();
				orgheaderPK = orgHeader.PK;
			}

			using (var conn1 = Db.NewAdminConnection())
			{
				var factory = new BusinessObjectFactory(conn1) { RefreshEnabled = false };
				var orgHeader = factory.Load<OrgHeader>(orgheaderPK);
				var requiredDoc = factory.Load<JobRequiredDocumentForTest>(orgHeader.RequiredDocuments[0].PK);
				requiredDoc.ParentType = orgHeader.GetType();

				requiredDoc.AfterOnSaving = () =>
				{
					using (var conn2 = Db.NewAdminConnection())
					{
						var newFactory = new BusinessObjectFactory(conn2) { RefreshEnabled = false };
						var newOrgHeader = newFactory.Load<OrgHeader>(orgheaderPK);
						var newRequiredDoc = newFactory.Load<JobRequiredDocumentForTest>(newOrgHeader.RequiredDocuments[0].PK);
						newRequiredDoc.ParentType = newOrgHeader.GetType();
						newRequiredDoc.EQ_DateReceived = ZDateTimeOffset.Now.AddDays(-1);
						AssertNoExceptionThrown("We don't need to lock the required document.", () => { newFactory.Save(); });
					}
				};

				requiredDoc.EQ_DateReceived = ZDateTimeOffset.Now;
				AssertExceptionThrown<ZSaveConcurrencyException>("As the other has been updated the EQ_DateReceived, this should throw concurrency exception.", () => factory.Save());
			}
		}

		public void TestDontLoadTheParentDuringOnSaving()
		{
			var reqDoc = GetRequiredDocumentForTest();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			reqDoc = newFactory.Load<JobRequiredDocumentForTest>(reqDoc.PK);
			reqDoc.EQ_DocumentNotes = reqDoc.EQ_DocumentNotes + "s";
			AssertNoExceptionThrown(newFactory.Save);
		}

		JobRequiredDocumentForTest GetRequiredDocumentForTest()
		{
			var parent = Factory.NewWithValidTestData<OrgHeader>();
			var reqDoc = Factory.New<JobRequiredDocumentForTest>();
			reqDoc.EQ_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			reqDoc.EQ_ParentID = parent.PK;
			reqDoc.ParentType = parent.GetType();
			reqDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Both;
			reqDoc.EQ_DocType = RefDocTypes.AgentsInstruction;
			return reqDoc;
		}

		public void TestShouldCheckDocTypeDuplication()
		{
			Assert(JobRequiredDocument.ShouldCheckDocTypeDuplication("ACV"));
			Assert(!JobRequiredDocument.ShouldCheckDocTypeDuplication("MSC"));
			Assert(!JobRequiredDocument.ShouldCheckDocTypeDuplication("HXD"));
			Assert(!JobRequiredDocument.ShouldCheckDocTypeDuplication("POA"));
			Assert(!JobRequiredDocument.ShouldCheckDocTypeDuplication("POC"));
			Assert(!JobRequiredDocument.ShouldCheckDocTypeDuplication("POF"));
		}

		public void TestDocumentSupporter()
		{
			var header = Factory.New<OrgHeader>();
			var requiredDocument = header.RequiredDocuments.AddNew();
			var documentSupportable = requiredDocument as IDocumentSupportable;
			AssertNotNull(requiredDocument);
			requiredDocument.EQ_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			requiredDocument.EQ_ParentID = Factory.New<OrgHeader>().PK;
			requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			requiredDocument.EQ_RN_NKRelatedCountry = "AU";
			AssertType<JobRequiredDocumentDocumentSupporter>(documentSupportable.DocumentSupporter);

			requiredDocument.EQ_RN_NKRelatedCountry = "TW";
			Assert(documentSupportable.DocumentSupporter.GetType().IsSubclassOf(typeof(JobRequiredDocumentDocumentSupporter)));
		}

		public void TestIsEXVCreditor()
		{
			JobRequiredDocument requiredDocument = Factory.New<JobRequiredDocument>();
			requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.VATExporterExemption;
			requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Creditor;
			Assert(requiredDocument.IsEXVCreditor);
			requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Debtor;
			Assert(!requiredDocument.IsEXVCreditor);
		}

		public void TestIsEXVDebtor()
		{
			JobRequiredDocument requiredDocument = Factory.New<JobRequiredDocument>();
			requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.VATExporterExemption;
			requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Creditor;
			Assert(!requiredDocument.IsEXVDebtor);
			requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Debtor;
			Assert(requiredDocument.IsEXVDebtor);
		}

		public void TestRelatedCountrySupportDeclarationOfIntent()
		{
			JobRequiredDocument requiredDocument = Factory.New<JobRequiredDocument>();
			requiredDocument.EQ_RN_NKRelatedCountry = "";
			Assert(!requiredDocument.RelatedCountrySupportDeclarationOfIntent);
			requiredDocument.EQ_RN_NKRelatedCountry = "AU";
			Assert(!requiredDocument.RelatedCountrySupportDeclarationOfIntent);
			requiredDocument.EQ_RN_NKRelatedCountry = "IT";
			Assert(requiredDocument.RelatedCountrySupportDeclarationOfIntent);
		}

		public void TestOnSaveGeneratesDeclarationOfIntentData()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				JobRequiredDocument requiredDocument = Factory.New<JobRequiredDocument>();
				requiredDocument.EQ_DocCategory = "CSR";
				requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.VATExporterExemption;
				requiredDocument.EQ_DocPeriod = "PER";
				requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Debtor;
				requiredDocument.EQ_RN_NKRelatedCountry = "IT";
				requiredDocument.EQ_DateReceived = new ZDateTimeOffset(2016, 01, 01);
				requiredDocument.EQ_ValidToDate = new ZDateTime(2016, 12, 31);
				AssertEquals(requiredDocument.Attributes.Count, 0);
				Factory.Save();
				AssertEquals(requiredDocument.Attributes.Count, 2);
				var attrib = requiredDocument.Attributes.FirstOrDefault(x => x.D0_AttribName == JobRequiredDocAttribTypeList.Codes.CompanyCode);
				AssertNotNull(attrib);
				AssertEquals(attrib.D0_AttribValue, GlbCompany.CurrentCompany.PK.ToString());
				attrib = requiredDocument.Attributes.FirstOrDefault(x => x.D0_AttribName == JobRequiredDocAttribTypeList.Codes.SellerControlNumber);
				AssertNotNull(attrib);
				AssertEquals("2016-000001", attrib.D0_AttribValue);

				requiredDocument = Factory.New<JobRequiredDocument>();
				requiredDocument.EQ_DocCategory = "CSR";
				requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.VATExporterExemption;
				requiredDocument.EQ_DocPeriod = "PER";
				requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Creditor;
				requiredDocument.EQ_RN_NKRelatedCountry = "IT";
				requiredDocument.EQ_DateReceived = new ZDateTimeOffset(2016, 01, 01);
				requiredDocument.EQ_ValidToDate = new ZDateTime(2016, 12, 31);
				AssertEquals(requiredDocument.Attributes.Count, 0);
				Factory.Save();
				AssertEquals(requiredDocument.Attributes.Count, 1);
				attrib = requiredDocument.Attributes.FirstOrDefault(x => x.D0_AttribName == JobRequiredDocAttribTypeList.Codes.CompanyCode);
				AssertNotNull(attrib);
				AssertEquals(GlbCompany.CurrentCompany.PK.ToString(), attrib.D0_AttribValue);
				AssertEquals("2016-000001", requiredDocument.EQ_DocNumber);
			}
		}

		public void TestEQ_DocNumber_ReadOnly()
		{
			JobRequiredDocument requiredDocument = Factory.New<JobRequiredDocument>();
			requiredDocument.EQ_DocCategory = "CSR";
			requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.VATExporterExemption;
			requiredDocument.EQ_DocPeriod = "PER";
			requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Debtor;
			requiredDocument.EQ_RN_NKRelatedCountry = "IT";
			Assert(!requiredDocument.EQ_DocNumberInfo.ReadOnly);
			requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Creditor;
			Assert(requiredDocument.EQ_DocNumberInfo.ReadOnly);
		}

		public void TestGetInstructions()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = "US";
			company.GC_Name = "ABC Customs Service Pty";

			var requiredDocument = Factory.New<JobRequiredDocument>();
			AssertEquals(true, requiredDocument.CanDelete);

			var addInfo = requiredDocument.AddInfos.AddNew();
			addInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			addInfo.EX_GC_Company = company.PK;
			AssertEquals(false, requiredDocument.CanDelete);
			AssertContains("go to eDocs and click 'View/Edit DIS Data'. Then delete the related records in the first grid.", requiredDocument.ReasonForNotAbleToDelete);
			AssertContains("ABC Customs Service Pty", requiredDocument.ReasonForNotAbleToDelete);
		}

		public void TestSettingEQ_DocPeriodValidatesEQ_Doc_Type()
		{
			IDocsAndCartageParent declaration = (IDocsAndCartageParent)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			Factory.Save();

			JobRequiredDocument doc1 = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			doc1.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			doc1.EQ_DocType = "QRA";
			doc1.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			doc1.EQ_ValidToDate = ZDateTime.Now.AddDays(2);

			JobRequiredDocument doc2 = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			doc2.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			doc2.EQ_DocType = "QRA";
			doc2.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			doc2.EQ_ValidToDate = ZDateTime.Now.AddDays(5);

			AssertEquals(Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic, doc1.EQ_DocPeriod);
			AssertEquals(Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic, doc2.EQ_DocPeriod);

			Assert(!doc1.EQ_DocTypeInfo.HasErrors());
			Assert(!doc2.EQ_DocTypeInfo.HasErrors());

			doc2.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			Assert(doc2.EQ_DocTypeInfo.HasErrors());
		}

		public void TestSettingEQ_RN_RelatedCountryValidatesDocType()
		{
			var declaration = (IDocsAndCartageParent)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			Factory.Save();

			var doc1 = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			doc1.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			doc1.EQ_DocType = "QRA";
			doc1.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			doc1.EQ_RN_NKRelatedCountry = "CN";

			JobRequiredDocument doc2 = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			doc2.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			doc2.EQ_DocType = "QRA";
			doc2.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;

			AssertNoErrors(doc1.EQ_DocTypeInfo);
			AssertNoErrors(doc2.EQ_DocTypeInfo);

			Factory.Save();

			AssertNoErrors(doc1.EQ_DocTypeInfo);
			AssertNoErrors(doc2.EQ_DocTypeInfo);

			doc2.EQ_RN_NKRelatedCountry = "CN";

			AssertNoErrors(doc1.EQ_DocTypeInfo);
			AssertHasError(doc2.EQ_DocTypeInfo, "The type QRA does not allow multiple periodic documents, but there is already QRA type for this job with the same valid to date. If it's not shown - please reload the form.");
		}

		public void TestIDocManagerSupportProvider()
		{
			var declaration = (IDocsAndCartageParent)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			Factory.Save();

			var requiredDoc = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			AssertEquals("DocumentParent", declaration, requiredDoc.DocumentParent);
			AssertEquals(declaration, ((IDocManagerSupportProvider)requiredDoc).DocManagerSupports.First());
		}

		public void TestGetIDocManagerSupportProviderFromAddInfo()
		{
			var declaration = (IDocsAndCartageParent)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			var requiredDoc = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var addInfo = requiredDoc.AddInfos.AddNew();
			addInfo.EX_ReferenceNumber = "REF00001";
			addInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			var addInfo11 = newFactory.LoadTop1<JobRequiredDocumentAddInfo>(new ZQuery(JobRequiredDocumentAddInfoSchema.EX_ReferenceNumber, "REF00001"));
			var newdec = newFactory.Load<Enterprise.Integration.Customs.IBaseJobDeclaration>(((BusinessObject)declaration).PK);
			var requiredDocuemnt11 = addInfo11.RequiredDocument;
			Assert(!requiredDocuemnt11.IsParentTypeSet);
			AssertEquals("DocumentParent", newdec, requiredDocuemnt11.DocumentParent);
			AssertEquals(newdec, ((IDocManagerSupportProvider)requiredDocuemnt11).DocManagerSupports.First());
		}

		public void TestSupportedLinkedObjectTypes()
		{
			var declaration = (IDocsAndCartageParent)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			Factory.Save();

			var requiredDoc = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			AssertEquals(2, requiredDoc.RegisteredLinkedObjectTypes.Count);
			Assert(requiredDoc.RegisteredLinkedObjectTypes.Contains(ObjectFactory.GetType<Enterprise.Integration.Freight.IJobDocsAndCartage>()));
			Assert(requiredDoc.RegisteredLinkedObjectTypes.Contains(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseCusPermitHeader>()));
		}

		[ExpectNoExceptions]
		public void TestConcurrencyOnDeletedBusinessObject()
		{
			IDocsAndCartageParent parentParent = (IDocsAndCartageParent)Factory.New<Enterprise.Integration.Freight.ICommonShipment>();
			JobRequiredDocument reqDoc = parentParent.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			Factory.Save();

			reqDoc.EQ_DocType = "BOB";

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobRequiredDocument reqDocInNewFactory = newFactory.Load<JobRequiredDocument>(reqDoc.PK);
			reqDocInNewFactory.ParentType = reqDoc.ParentType;
			reqDocInNewFactory.Delete();

			using (GetFactoryIsolater(Factory))
			using (GetFactoryIsolater(newFactory))
			{
				newFactory.Save();
			}

			try
			{
				Factory.Save();
				Fail("should throw a ZSaveConcurrencyException");
			}
			catch (ZSaveConcurrencyException ex)
			{
				// should not throw an exception while handling the ZSaveConcurrencyException.
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		[ExpectNoExceptions]
		public void TestParentTypeSetForConcurrencyResolution()
		{
			IDocsAndCartageParent parentParent = (IDocsAndCartageParent)Factory.New<Enterprise.Integration.Freight.ICommonShipment>();
			JobRequiredDocument reqDoc = parentParent.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			Factory.Save();

			reqDoc.EQ_DocType = "BOB";

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobRequiredDocument reqDocInNewFactory = newFactory.Load<JobRequiredDocument>(reqDoc.PK);
			reqDocInNewFactory.ParentType = reqDoc.ParentType;
			reqDocInNewFactory.EQ_DocType = "BLT";

			using (GetFactoryIsolater(Factory))
			using (GetFactoryIsolater(newFactory))
			{
				newFactory.Save();
			}

			try
			{
				Factory.Save();
				Fail("should throw a ZSaveConcurrencyException");
			}
			catch (ZSaveConcurrencyException ex)
			{
				// should not throw an exception while handling the ZSaveConcurrencyException.
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		[ExpectException(typeof(InvalidOperationException))]
		public void TestThrowExceptionIfAccessingParentAndParentTypeNotYetKnown()
		{
			IDocsAndCartageParent parentParent = (IDocsAndCartageParent)Factory.New<Enterprise.Integration.Freight.ICommonShipment>();

			JobRequiredDocument reqDoc = Factory.New<JobRequiredDocument>();
			reqDoc.EQ_ParentID = parentParent.RequiredDocumentsProvider.PK;
			AssertNotNull("Parent should throw exception (this assert is just to hit the Property)", reqDoc.Parent);
		}

		public void TestDefaultValues()
		{
			IDocsAndCartageParent shipment = (IDocsAndCartageParent)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingShipment)));
			Factory.Save();
			JobRequiredDocument reqDoc1 = shipment.RequiredDocumentsProvider.RequiredDocuments.AddNew();

			IDocsAndCartageParent declaration = (IDocsAndCartageParent)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			Factory.Save();
			JobRequiredDocument reqDoc2 = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();

			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			JobRequiredDocument reqDoc3 = organisation.RequiredDocuments.AddNew();

			OrgSupplierBuyerLink supplierlink = Factory.NewWithValidTestData<OrgSupplierBuyerLink>();
			Factory.Save();
			JobRequiredDocument reqDoc4 = supplierlink.RequiredDocuments.AddNew();

			AssertEquals(Core.Constants.ReferenceTypes.SupplyChainLogistics, reqDoc1.EQ_DocCategory);
			AssertEquals(Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, reqDoc1.EQ_DocPeriod);
			AssertEquals(Core.Constants.ReferenceTypes.SupplyChainLogistics, reqDoc2.EQ_DocCategory);
			AssertEquals(Core.Constants.ReferenceTypes.ClientSupplierRelationship, reqDoc3.EQ_DocCategory);
			AssertEquals(Core.Constants.ReferenceTypes.SupplyChainLogistics, reqDoc4.EQ_DocCategory);

			reqDoc3.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			AssertEquals(Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic, reqDoc3.EQ_DocPeriod);
		}

		public void TestDefaultValuesForPOA()
		{
			IDocsAndCartageParent declaration = (IDocsAndCartageParent)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			Factory.Save();
			JobRequiredDocument reqDoc = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			reqDoc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			AssertNotEquals(Core.Constants.ReferenceTypes.ClientSupplierRelationship, reqDoc.EQ_DocCategory);
			AssertEquals(JobRequiredDocument.DocUsage.Import, reqDoc.EQ_DocUsage);
		}

		public void TestDefaultValuesForEXV()
		{
			JobRequiredDocument reqDoc = Factory.New<JobRequiredDocument>();
			reqDoc.EQ_DocType = Core.Constants.RefDocTypes.VATExporterExemption;
			AssertEquals(Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic, reqDoc.EQ_DocPeriod);
			AssertEquals(ZString.Empty, reqDoc.EQ_DocUsage);
		}

		public void TestDontThrowExceptionIfAccessingParentAndParentTypeKnown()
		{
			IDocsAndCartageParent parentParent = (IDocsAndCartageParent)Factory.New<Enterprise.Integration.Freight.ICommonShipment>();

			JobRequiredDocument reqDoc = Factory.New<JobRequiredDocument>();
			reqDoc.ParentType = parentParent.RequiredDocumentsProvider.GetType();
			reqDoc.EQ_ParentID = parentParent.RequiredDocumentsProvider.PK;
			AssertNotNull("Parent should not throw exception (this assert is just to hit the Property)", reqDoc.Parent);
		}

		public void TestGettingParent()
		{
			JobRequiredDocument orphan = Factory.New<JobRequiredDocument>();
			AssertNull("Expect parent of orphan JobRequiredDocument to be null", orphan.Parent);

			AssertEquals("Expect parent of valid JobRequiredDocument to be DocsAndCartage", Shipment.RequiredDocumentsProvider.PK, Doc.Parent.PK);
		}

		public void TestDocDescriptionField()
		{
			AssertEquals("Precondition - description field is empty", true, Doc.EQ_DocDescription.IsEmpty);
			AssertEquals("Precondition - description field is not editable", true, Doc.EQ_DocDescriptionInfo.ReadOnly);

			Doc.EQ_DocType = Core.Constants.RefDocTypes.MasterBill;
			AssertEquals("Description field should be populated", Core.Constants.RefDocTypeDescriptions.MasterBill, Doc.EQ_DocDescription);
			AssertEquals("Description field shouldn't be editable", true, Doc.EQ_DocDescriptionInfo.ReadOnly);

			Doc.EQ_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			AssertEquals("Description field should be blank", true, Doc.EQ_DocDescription.IsEmpty);
			AssertEquals("Description field should be editable", false, Doc.EQ_DocDescriptionInfo.ReadOnly);
		}

		public void TestEQ_DocPeriodDefaultForPOA()
		{
			Doc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			AssertEquals(Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, Doc.EQ_DocPeriod);

			Doc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyCustoms;
			AssertEquals(Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, Doc.EQ_DocPeriod);

			Doc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyForwarding;
			AssertEquals(Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, Doc.EQ_DocPeriod);

			DeclarationDoc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			AssertEquals(Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, DeclarationDoc.EQ_DocPeriod);

			DeclarationDoc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyCustoms;
			AssertEquals(Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, DeclarationDoc.EQ_DocPeriod);

			DeclarationDoc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyForwarding;
			AssertEquals(Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, DeclarationDoc.EQ_DocPeriod);

			OrganiasationDoc.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			OrganiasationDoc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			AssertEquals(Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic, OrganiasationDoc.EQ_DocPeriod);

			OrganiasationDoc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyCustoms;
			AssertEquals(Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic, OrganiasationDoc.EQ_DocPeriod);

			OrganiasationDoc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyForwarding;
			AssertEquals(Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic, OrganiasationDoc.EQ_DocPeriod);
		}

		public void TestIsPeriodic()
		{
			JobRequiredDocument doc = Factory.New<JobRequiredDocument>();
			doc.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			AssertEquals(false, doc.IsPeriodic);

			doc.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			AssertEquals(true, doc.IsPeriodic);
		}

		public void TestIsPowerOfAttorney()
		{
			Doc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyForwarding;
			AssertEquals(true, Doc.IsPowerOfAttorney);

			Doc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyCustoms;
			AssertEquals(true, Doc.IsPowerOfAttorney);

			Doc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			AssertEquals(true, Doc.IsPowerOfAttorney);

			Doc.EQ_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			AssertEquals(false, Doc.IsPowerOfAttorney);
		}

		public void TestHXDLogsAddedOnSaving()
		{
			string originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);

				AssertEquals("Precondition - doc is new", false, Doc.IsInDatabase);
				AssertNull("Precondition - no logs exist for received from shipper", Doc.Parent.UltimateDocumentParent.GetLogs().MostRecentLogByEventTime(Events.HXDReceivedFromShipper));
				AssertNull("Precondition - no logs exist for to customs broker", Doc.Parent.UltimateDocumentParent.GetLogs().MostRecentLogByEventTime(Events.HXDToCustomsBroker));
				AssertNull("Precondition - no logs exist for from customs broker", Doc.Parent.UltimateDocumentParent.GetLogs().MostRecentLogByEventTime(Events.HXDFromCustomsBroker));
				AssertNull("Precondition - no logs exist for returned to shipper", Doc.Parent.UltimateDocumentParent.GetLogs().MostRecentLogByEventTime(Events.HXDReturnedToShipper));

				Doc.EQ_DocType = Core.Constants.RefDocTypes.HeXiaoDan;
				Doc.EQ_DateReceived = ZDateTimeOffset.Now.AddDays(-10);
				Doc.EQ_SntToCustomsBroker = ZDateTime.Now.AddDays(-8);
				Factory.Save();

				AssertNotNull("A log should have been created for received from shipper", Doc.Parent.UltimateDocumentParent.GetLogs().MostRecentLogByEventTime(Events.HXDReceivedFromShipper));
				AssertNotNull("A log should have been created for to customs broker", Doc.Parent.UltimateDocumentParent.GetLogs().MostRecentLogByEventTime(Events.HXDToCustomsBroker));
				AssertNull("No logs should exist for from customs broker", Doc.Parent.UltimateDocumentParent.GetLogs().MostRecentLogByEventTime(Events.HXDFromCustomsBroker));
				AssertNull("No logs should exist for returned to shipper", Doc.Parent.UltimateDocumentParent.GetLogs().MostRecentLogByEventTime(Events.HXDReturnedToShipper));

				StmALog sentToBrokerLog = Doc.Parent.UltimateDocumentParent.GetLogs().MostRecentLogByEventTime(Events.HXDToCustomsBroker);
				Doc.EQ_SntToCustomsBroker = ZDateTime.Empty;
				Factory.Save();

				AssertNotNull("A log should have been created for received from shipper", Doc.Parent.UltimateDocumentParent.GetLogs().MostRecentLogByEventTime(Events.HXDReceivedFromShipper));
				AssertEquals("To Customs Broker log should be cancelled", true, sentToBrokerLog.SL_IsCancelled);
				AssertNull("No logs should exist for from customs broker", Doc.Parent.UltimateDocumentParent.GetLogs().MostRecentLogByEventTime(Events.HXDFromCustomsBroker));
				AssertNull("No logs should exist for returned to shipper", Doc.Parent.UltimateDocumentParent.GetLogs().MostRecentLogByEventTime(Events.HXDReturnedToShipper));
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountry);
			}
		}

		public void TestSecurityOfFields()
		{
			bool oldSentToBroker = Env.Security.ForwardingDocumentTrackingEditDateSentToBrokerIndividualRecords.IsAllowed;
			bool oldRecivedFromBroker = Env.Security.ForwardingDocumentTrackingEditDateReceivedFromBrokerIndividualRecords.IsAllowed;
			bool oldReturnToShipper = Env.Security.ForwardingDocumentTrackingEditDateReturnToShipperIndividualRecords.IsAllowed;
			bool oldCreditControlStatus = Env.Security.ForwardingDocumentTrackingEditDocumentCreditControlStatus.IsAllowed;

			try
			{
				Doc.EQ_DateReceived = ZDateTimeOffset.Now;

				Env.Security.ForwardingDocumentTrackingEditDateSentToBrokerIndividualRecords.IsAllowed = false;
				Env.Security.ForwardingDocumentTrackingEditDateReceivedFromBrokerIndividualRecords.IsAllowed = false;
				Env.Security.ForwardingDocumentTrackingEditDateReturnToShipperIndividualRecords.IsAllowed = false;
				Env.Security.ForwardingDocumentTrackingEditDocumentCreditControlStatus.IsAllowed = false;
				AssertEquals("User without security permissions shouldn't be able to edit document fields", true, Doc.EQ_CreditControlDocInfo.ReadOnly);
				AssertEquals("User without security permissions shouldn't be able to edit document fields", true, Doc.EQ_SntToCustomsBrokerInfo.ReadOnly);
				Doc.EQ_SntToCustomsBroker = ZDateTime.Now;
				AssertEquals("User without security permissions shouldn't be able to edit document fields", true, Doc.EQ_RcvFromCustomsBrokerInfo.ReadOnly);
				Doc.EQ_RcvFromCustomsBroker = ZDateTime.Now;
				AssertEquals("User without security permissions shouldn't be able to edit document fields", true, Doc.EQ_ReturnToShipperInfo.ReadOnly);

				Env.Security.ForwardingDocumentTrackingEditDateSentToBrokerIndividualRecords.IsAllowed = true;
				Env.Security.ForwardingDocumentTrackingEditDateReceivedFromBrokerIndividualRecords.IsAllowed = true;
				Env.Security.ForwardingDocumentTrackingEditDateReturnToShipperIndividualRecords.IsAllowed = true;
				Env.Security.ForwardingDocumentTrackingEditDocumentCreditControlStatus.IsAllowed = true;
				AssertEquals("User with security permissions should be able to edit document fields", false, Doc.EQ_CreditControlDocInfo.ReadOnly);
				AssertEquals("User with security permissions should be able to edit document fields", false, Doc.EQ_SntToCustomsBrokerInfo.ReadOnly);
				Doc.EQ_SntToCustomsBroker = ZDateTime.Now;
				AssertEquals("User with security permissions should be able to edit document fields", false, Doc.EQ_RcvFromCustomsBrokerInfo.ReadOnly);
				Doc.EQ_RcvFromCustomsBroker = ZDateTime.Now;
				AssertEquals("User with security permissions should be able to edit document fields", false, Doc.EQ_ReturnToShipperInfo.ReadOnly);
			}
			finally
			{
				Env.Security.ForwardingDocumentTrackingEditDateSentToBrokerIndividualRecords.IsAllowed = oldSentToBroker;
				Env.Security.ForwardingDocumentTrackingEditDateReceivedFromBrokerIndividualRecords.IsAllowed = oldRecivedFromBroker;
				Env.Security.ForwardingDocumentTrackingEditDateReturnToShipperIndividualRecords.IsAllowed = oldReturnToShipper;
				Env.Security.ForwardingDocumentTrackingEditDocumentCreditControlStatus.IsAllowed = oldCreditControlStatus;
			}
		}

		public void TestLocalOutstandingInvoices()
		{
			AssertEquals("No invoices should be attached to the document/shipment at first", 0, Doc.LocalOutstandingInvoices.Count);

			JobHeader testJH1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			testJH1.JH_ParentID = ((BusinessObject)Shipment).PK;
			testJH1.JH_ParentTableCode = "JS";
			testJH1.JH_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_RL_NKClosestPort = "CN$$$";
			Factory.Save();

			var outstandingAccHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			outstandingAccHeader.AH_TransactionNum = "33";
			outstandingAccHeader.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			outstandingAccHeader.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			outstandingAccHeader.AH_OH = testOrg.PK;
			outstandingAccHeader.AH_JH = testJH1.PK;
			outstandingAccHeader.AH_OutstandingAmount = 20M;
			outstandingAccHeader.AH_InvoiceAmount = 20M;
			outstandingAccHeader.AH_InvoiceDate = ZDateTime.Now;
			outstandingAccHeader.AH_PostDate = ZDateTime.Now;
			outstandingAccHeader.AH_RX_NKTransactionCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;

			var outstandingAccHeader2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			outstandingAccHeader.AH_TransactionNum = "44";
			outstandingAccHeader2.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			outstandingAccHeader2.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			outstandingAccHeader2.AH_OH = testOrg.PK;
			outstandingAccHeader2.AH_JH = testJH1.PK;
			outstandingAccHeader2.AH_OutstandingAmount = 30M;
			outstandingAccHeader2.AH_InvoiceAmount = 30M;
			outstandingAccHeader2.AH_InvoiceDate = ZDateTime.Now;
			outstandingAccHeader2.AH_PostDate = ZDateTime.Now;
			outstandingAccHeader2.AH_RX_NKTransactionCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;

			Factory.Save();

			AssertEquals("2 invoices should be outstanding", 2, Doc.LocalOutstandingInvoices.Count);
		}

		public void TestEQ_DateReceived()
		{
			AssertEquals("Precondition - doc is new", false, Doc.IsInDatabase);
			AssertNull("Precondition - no all documents received events fired", Doc.Parent.UltimateDocumentParent.GetLogs().MostRecentLogByEventTime(Events.AllExportDocumentsReceived));
			AssertNull("Precondition - no all documents received events fired", Doc.Parent.UltimateDocumentParent.GetLogs().MostRecentLogByEventTime(Events.AllImportDocumentsReceived));

			Doc.EQ_DocType = Core.Constants.RefDocTypes.AgentsInstruction;
			Doc.EQ_DateReceived = ZDateTimeOffset.Now;
			Factory.Save();

			AssertNotNull("all documents received events fired because is was not suspended", Doc.Parent.UltimateDocumentParent.GetLogs().MostRecentLogByEventTime(Events.AllExportDocumentsReceived));
			AssertNotNull("all documents received events fired because is was not suspended", Doc.Parent.UltimateDocumentParent.GetLogs().MostRecentLogByEventTime(Events.AllImportDocumentsReceived));
		}

		public void TestEQ_DateReceivedUtc()
		{
			var brazil = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Brazil));
			var brazilCompany = Factory.NewWithValidTestData<GlbCompany>();
			brazilCompany.GC_RN_NKCountryCode = brazil.Code;
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_GC = brazilCompany.PK;
			Factory.Save();

			Doc.EQ_DocType = Core.Constants.RefDocTypes.AgentsInstruction;
			var utcTime = new TimeFactory().CurrentUtcDateTime;

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, newBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Doc.EQ_DateReceived = ZDateTimeOffset.Now;
				Factory.Save();

				AssertEquals("Attaching Local time", Doc.EQ_DateReceived.ToString("yyyy-MM-dd hh:mm"), new TimeFactory().GetLocalTimeFromUtc(utcTime).ToString("yyyy-MM-dd hh:mm"));
				AssertEquals("Attaching UTC time should depend on a branch", Doc.EQ_DateReceivedUtc.ToString("yyyy-MM-dd hh:mm"), utcTime.ToString("yyyy-MM-dd hh:mm"));
			}
		}

		public void TestCreateAllDocumentReceivedEvent_DeleteNonReceivedRequiredDocument()
		{
			var shipment = (IDocsAndCartageParent)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var jobRequiredDocuments = shipment.RequiredDocumentsProvider.RequiredDocuments;

			var jobRequiredDocument1 = jobRequiredDocuments.AddNew();
			jobRequiredDocument1.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			jobRequiredDocument1.EQ_DocType = Core.Constants.RefDocTypes.AgentsInvoice;
			jobRequiredDocument1.EQ_DocUsage = Enterprise.MasterFiles.Business.JobRequiredDocument.DocUsage.Both;

			var jobRequiredDocument2 = jobRequiredDocuments.AddNew();
			jobRequiredDocument2.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			jobRequiredDocument2.EQ_DocType = Core.Constants.RefDocTypes.HouseBill;
			jobRequiredDocument2.EQ_DocUsage = Enterprise.MasterFiles.Business.JobRequiredDocument.DocUsage.Both;

			jobRequiredDocument1.EQ_DateReceived = ZDateTimeOffset.Now;
			Factory.Save();

			var queryToFindAED = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AllExportDocumentsReceivedCode);
			var queryToFindAID = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AllImportDocumentsReceivedCode);
			AssertEquals("No AED triggered yet", 0, ((BusinessObject)shipment).GetLogs().Find(queryToFindAED).Length);
			AssertEquals("No AID triggered yet", 0, ((BusinessObject)shipment).GetLogs().Find(queryToFindAID).Length);

			jobRequiredDocuments.RemoveCollectionRelationships(jobRequiredDocument2, true);
			jobRequiredDocument2.Delete();
			Factory.Save();

			AssertEquals("AED was triggered", 1, ((BusinessObject)shipment).GetLogs().Find(queryToFindAED).Length);
			AssertEquals("AID was triggered", 1, ((BusinessObject)shipment).GetLogs().Find(queryToFindAID).Length);
		}

		public void TestAddDocumentsNotFiredAIDAEDEvent_WithNoRequiredDocumentOriginIsFromRequirements()
		{
			var shipment = (IDocsAndCartageParent)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var jobRequiredDocuments = shipment.RequiredDocumentsProvider.RequiredDocuments;

			var jobRequiredDocument1 = jobRequiredDocuments.AddNew();
			jobRequiredDocument1.EQ_DocCategory = ReferenceTypes.SupplyChainLogistics;
			jobRequiredDocument1.EQ_DocType = RefDocTypes.AgentsInvoice;
			jobRequiredDocument1.EQ_DocUsage = JobRequiredDocument.DocUsage.Both;
			jobRequiredDocument1.Origin = JobRequiredDocument.JRDOrigin.AddedEDoc;
			jobRequiredDocument1.EQ_DateReceived = ZDateTimeOffset.Now;
			Factory.Save();

			var queryToFindAED = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AllExportDocumentsReceivedCode);
			var queryToFindAID = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AllImportDocumentsReceivedCode);
			AssertEquals("No AED triggered yet", 0, ((BusinessObject)shipment).GetLogs().Find(queryToFindAED).Length);
			AssertEquals("No AID triggered yet", 0, ((BusinessObject)shipment).GetLogs().Find(queryToFindAID).Length);

			//Since Origin is a local property, set requiredDocument.Origin from AddedEDoc to Unknow, to simulate reopening the Form or the DOD ServiceTask handling the deliver eDoc.
			jobRequiredDocument1.Origin = JobRequiredDocument.JRDOrigin.Unknown;

			var jobRequiredDocument2 = jobRequiredDocuments.AddNew();
			jobRequiredDocument2.EQ_DocCategory = ReferenceTypes.SupplyChainLogistics;
			jobRequiredDocument2.EQ_DocType = RefDocTypes.HouseBill;
			jobRequiredDocument2.EQ_DocUsage = JobRequiredDocument.DocUsage.Both;
			jobRequiredDocument2.Origin = JobRequiredDocument.JRDOrigin.AddedEDoc;
			jobRequiredDocument2.EQ_DateReceived = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("No AED triggered yet", 0, ((BusinessObject)shipment).GetLogs().Find(queryToFindAED).Length);
			AssertEquals("No AID triggered yet", 0, ((BusinessObject)shipment).GetLogs().Find(queryToFindAID).Length);
		}

		public void TestDontCancelAndRecreateAllDocumentReceivedEvent()
		{
			IDocsAndCartageParent shipment = (IDocsAndCartageParent)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var jobRequiredDocumentBTH = shipment.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			jobRequiredDocumentBTH.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			jobRequiredDocumentBTH.EQ_DocType = Core.Constants.RefDocTypes.AgentsInvoice;
			jobRequiredDocumentBTH.EQ_DocUsage = Enterprise.MasterFiles.Business.JobRequiredDocument.DocUsage.Both;

			var jobRequiredDocumentIMP = shipment.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			jobRequiredDocumentIMP.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			jobRequiredDocumentIMP.EQ_DocType = Core.Constants.RefDocTypes.ArrivalNotice;
			jobRequiredDocumentIMP.EQ_DocUsage = Enterprise.MasterFiles.Business.JobRequiredDocument.DocUsage.Import;

			Factory.Save();

			var queryToFindAED = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AllExportDocumentsReceivedCode);
			var queryToFindAID = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AllImportDocumentsReceivedCode);
			AssertEquals("No AED triggered yet", 0, ((BusinessObject)shipment).GetLogs().Find(queryToFindAED).Length);
			AssertEquals("No AID triggered yet", 0, ((BusinessObject)shipment).GetLogs().Find(queryToFindAID).Length);

			jobRequiredDocumentBTH.EQ_DateReceived = ZDateTimeOffset.Now;
			Factory.Save();
			AssertEquals("AED was triggered", 1, ((BusinessObject)shipment).GetLogs().Find(queryToFindAED).Length);
			AssertEquals("No AID triggered yet", 0, ((BusinessObject)shipment).GetLogs().Find(queryToFindAID).Length);

			jobRequiredDocumentIMP.EQ_DateReceived = ZDateTimeOffset.Now;
			Factory.Save();
			AssertEquals("Still 1 AED was triggered", 1, ((BusinessObject)shipment).GetLogs().Find(queryToFindAED).Length);
			AssertEquals("AID was triggered", 1, ((BusinessObject)shipment).GetLogs().Find(queryToFindAID).Length);
		}

		public void TestAddDocumentsFiredAIDAEDEvent_WhenRequiredDocumentReceived()
		{
			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var docsAndCartageParent = shipment as IDocsAndCartageParent;
			var jobRequiredDocumentBTH = docsAndCartageParent.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			jobRequiredDocumentBTH.EQ_DocCategory = ReferenceTypes.SupplyChainLogistics;
			jobRequiredDocumentBTH.EQ_DocType = RefDocTypes.AgentsInvoice;
			jobRequiredDocumentBTH.EQ_DocUsage = JobRequiredDocument.DocUsage.Both;

			var jobRequiredDocumentIMP = docsAndCartageParent.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			jobRequiredDocumentIMP.EQ_DocCategory = ReferenceTypes.SupplyChainLogistics;
			jobRequiredDocumentIMP.EQ_DocType = RefDocTypes.ArrivalNotice;
			jobRequiredDocumentIMP.EQ_DocUsage = JobRequiredDocument.DocUsage.Import;

			Factory.Save();

			var queryToFindAED = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AllExportDocumentsReceivedCode);
			var queryToFindAID = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AllImportDocumentsReceivedCode);
			AssertEquals("No AED triggered yet", 0, ((BusinessObject)shipment).GetLogs().Find(queryToFindAED).Length);
			AssertEquals("No AID triggered yet", 0, ((BusinessObject)shipment).GetLogs().Find(queryToFindAID).Length);

			var documentFactory = ((IDocManagerSupport)shipment).DocManagerInfo.MasterFactory;
			AddEDocs(documentFactory, shipment.PK, RefDocTypes.AirFreightManifest, true);

			AssertEquals("No AED triggered yet", 0, ((BusinessObject)shipment).GetLogs().Find(queryToFindAED).Length);
			AssertEquals("No AID triggered yet", 0, ((BusinessObject)shipment).GetLogs().Find(queryToFindAID).Length);

			AddEDocs(documentFactory, shipment.PK, RefDocTypes.AgentsInvoice, false);
			AddEDocs(documentFactory, shipment.PK, RefDocTypes.ArrivalNoticeAndChargeSheet, true);

			AssertEquals("AED was triggered", 1, ((BusinessObject)shipment).GetLogs().Find(queryToFindAED).Length);
			AssertEquals("No AID triggered yet", 0, ((BusinessObject)shipment).GetLogs().Find(queryToFindAID).Length);

			AddEDocs(documentFactory, shipment.PK, RefDocTypes.ArrivalNotice, false);
			AddEDocs(documentFactory, shipment.PK, RefDocTypes.AuthorityToDeal, true);

			AssertEquals("AED was triggered", 1, ((BusinessObject)shipment).GetLogs().Find(queryToFindAED).Length);
			AssertEquals("AID was triggered", 1, ((BusinessObject)shipment).GetLogs().Find(queryToFindAID).Length);
		}

		public void TestAddDocuments_WithNoRequiredDocument_AEDAndAIDEventsRegistryOn()
		{
			SystemDataRegistry.Instance.RestrictAEDAndAIDEvents.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var documentFactory = ((IDocManagerSupport)shipment).DocManagerInfo.MasterFactory;
			Factory.Save();

			var queryToFindAED = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AllExportDocumentsReceivedCode).AddToFilter(StmALogSchema.SL_IsCancelled, false);
			var queryToFindAID = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AllImportDocumentsReceivedCode).AddToFilter(StmALogSchema.SL_IsCancelled, false);

			AddEDocs(documentFactory, shipment.PK, RefDocTypes.ArrivalNoticeAndChargeSheet, true);
			AssertEquals("No AED triggered yet", 0, ((BusinessObject)shipment).GetLogs().Find(queryToFindAED).Length);
			AssertEquals("No AID triggered yet", 0, ((BusinessObject)shipment).GetLogs().Find(queryToFindAID).Length);

			var otherFactory = new BusinessObjectFactory();
			var queryToFindShipment = new ZQuery(JobShipmentSchema.PK, shipment.PK);
			shipment = otherFactory.LoadTop1<Enterprise.Integration.Forwarding.IForwardingShipment>(queryToFindShipment);
			documentFactory = ((IDocManagerSupport)shipment).DocManagerInfo.MasterFactory;

			AddEDocs(documentFactory, shipment.PK, RefDocTypes.CommercialInvoice, true);
			AssertEquals("No AED triggered yet", 0, ((BusinessObject)shipment).GetLogs().Find(queryToFindAED).Length);
			AssertEquals("No AID triggered yet", 0, ((BusinessObject)shipment).GetLogs().Find(queryToFindAID).Length);

			var docsAndCartageParent = shipment as IDocsAndCartageParent;
			var jobRequiredDocumentBTH = docsAndCartageParent.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			jobRequiredDocumentBTH.EQ_DocCategory = ReferenceTypes.SupplyChainLogistics;
			jobRequiredDocumentBTH.EQ_DocType = RefDocTypes.AgentsInvoice;
			jobRequiredDocumentBTH.EQ_DocUsage = JobRequiredDocument.DocUsage.Both;
			otherFactory.Save();

			AssertEquals("No AED triggered yet", 0, ((BusinessObject)shipment).GetLogs().Find(queryToFindAED).Length);
			AssertEquals("No AID triggered yet", 0, ((BusinessObject)shipment).GetLogs().Find(queryToFindAID).Length);

			AddEDocs(documentFactory, shipment.PK, RefDocTypes.AgentsInvoice, true);
			AssertEquals("AED was triggered", 1, ((BusinessObject)shipment).GetLogs().Find(queryToFindAED).Length);
			AssertEquals("AID was triggered", 1, ((BusinessObject)shipment).GetLogs().Find(queryToFindAID).Length);

			var jobRequiredDocumentIMP = docsAndCartageParent.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			jobRequiredDocumentIMP.EQ_DocCategory = ReferenceTypes.SupplyChainLogistics;
			jobRequiredDocumentIMP.EQ_DocType = RefDocTypes.ArrivalNotice;
			jobRequiredDocumentIMP.EQ_DocUsage = JobRequiredDocument.DocUsage.Import;
			otherFactory.Save();

			AssertEquals("AED was triggered", 1, ((BusinessObject)shipment).GetLogs().Find(queryToFindAED).Length);
			AssertEquals("AID was cancelled", 0, ((BusinessObject)shipment).GetLogs().Find(queryToFindAID).Length);
		}

		public void TestAddDocuments_WithRequiredDocuments_AEDAndAIDEventsRegistryOn_SaveSeparated()
		{
			SystemDataRegistry.Instance.RestrictAEDAndAIDEvents.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var documentFactory = ((IDocManagerSupport)shipment).DocManagerInfo.MasterFactory;

			var docsAndCartageParent = shipment as IDocsAndCartageParent;
			var jobRequiredDocumentBTH1 = docsAndCartageParent.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			jobRequiredDocumentBTH1.EQ_DocCategory = ReferenceTypes.SupplyChainLogistics;
			jobRequiredDocumentBTH1.EQ_DocType = RefDocTypes.AgentsInvoice;
			jobRequiredDocumentBTH1.EQ_DocUsage = JobRequiredDocument.DocUsage.Both;

			var jobRequiredDocumentBTH2 = docsAndCartageParent.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			jobRequiredDocumentBTH2.EQ_DocCategory = ReferenceTypes.SupplyChainLogistics;
			jobRequiredDocumentBTH2.EQ_DocType = RefDocTypes.CommercialInvoice;
			jobRequiredDocumentBTH2.EQ_DocUsage = JobRequiredDocument.DocUsage.Both;

			Factory.Save();

			var queryToFindAED = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AllExportDocumentsReceivedCode).AddToFilter(StmALogSchema.SL_IsCancelled, false);
			var queryToFindAID = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AllImportDocumentsReceivedCode).AddToFilter(StmALogSchema.SL_IsCancelled, false);

			AddEDocs(documentFactory, shipment.PK, RefDocTypes.AgentsInvoice, true);
			AssertEquals("No AED triggered yet", 0, ((BusinessObject)shipment).GetLogs().Find(queryToFindAED).Length);
			AssertEquals("No AID triggered yet", 0, ((BusinessObject)shipment).GetLogs().Find(queryToFindAID).Length);

			AddEDocs(documentFactory, shipment.PK, RefDocTypes.AgentsInstruction, true);
			AssertEquals("No AED triggered yet", 0, ((BusinessObject)shipment).GetLogs().Find(queryToFindAED).Length);
			AssertEquals("No AID triggered yet", 0, ((BusinessObject)shipment).GetLogs().Find(queryToFindAID).Length);

			AddEDocs(documentFactory, shipment.PK, RefDocTypes.CommercialInvoice, true);
			AssertEquals("AED was triggered", 1, ((BusinessObject)shipment).GetLogs().Find(queryToFindAED).Length);
			AssertEquals("AID was triggered", 1, ((BusinessObject)shipment).GetLogs().Find(queryToFindAID).Length);
		}

		public void TestAddDocuments_WithRequiredDocuments_AEDAndAIDEventsRegistryOn_SaveTogether()
		{
			SystemDataRegistry.Instance.RestrictAEDAndAIDEvents.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var documentFactory = ((IDocManagerSupport)shipment).DocManagerInfo.MasterFactory;

			var docsAndCartageParent = shipment as IDocsAndCartageParent;
			var jobRequiredDocumentBTH1 = docsAndCartageParent.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			jobRequiredDocumentBTH1.EQ_DocCategory = ReferenceTypes.SupplyChainLogistics;
			jobRequiredDocumentBTH1.EQ_DocType = RefDocTypes.AgentsInvoice;
			jobRequiredDocumentBTH1.EQ_DocUsage = JobRequiredDocument.DocUsage.Both;

			var jobRequiredDocumentBTH2 = docsAndCartageParent.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			jobRequiredDocumentBTH2.EQ_DocCategory = ReferenceTypes.SupplyChainLogistics;
			jobRequiredDocumentBTH2.EQ_DocType = RefDocTypes.CommercialInvoice;
			jobRequiredDocumentBTH2.EQ_DocUsage = JobRequiredDocument.DocUsage.Both;

			Factory.Save();

			var queryToFindAED = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AllExportDocumentsReceivedCode).AddToFilter(StmALogSchema.SL_IsCancelled, false);
			var queryToFindAID = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AllImportDocumentsReceivedCode).AddToFilter(StmALogSchema.SL_IsCancelled, false);

			AddEDocs(documentFactory, shipment.PK, RefDocTypes.AgentsInvoice, false);
			AssertEquals("No AED triggered yet", 0, ((BusinessObject)shipment).GetLogs().Find(queryToFindAED).Length);
			AssertEquals("No AID triggered yet", 0, ((BusinessObject)shipment).GetLogs().Find(queryToFindAID).Length);

			AddEDocs(documentFactory, shipment.PK, RefDocTypes.CommercialInvoice, true);
			AssertEquals("AED was triggered", 1, ((BusinessObject)shipment).GetLogs().Find(queryToFindAED).Length);
			AssertEquals("AID was triggered", 1, ((BusinessObject)shipment).GetLogs().Find(queryToFindAID).Length);
		}

		void AddEDocs(IDocumentFactory documentFactory, ZGuid shipmentPK, string docType, bool shouldSave = false)
		{
			documentFactory.AddFileOrDocument(
				shipmentPK,
				"SHP",
				new byte[3] { 1, 2, 3 },
				"sample.pdf",
				docType,
				"DEF",
				false);
			if (shouldSave)
			{
				documentFactory.Save();
			}
		}

		public void TestUniversalCopy_Shipment_JobRequiredDocument()
		{
			var countryAU = RefCountry.LoadFromCountryCode(Factory, CountryCodes.Australia);
			SetupRequiredDocumentForCountry(countryAU, RefDocTypes.AgentsInvoice, CountryCodes.Australia, CountryCodes.NewZealand, JobRequiredDocument.DocUsage.Both, TransportModes.All, true, true, true, true);

			var entityNode = new EntityCopyTemplateNode();
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobShipmentSchema.Constants.JS_RL_NKOrigin, CopyMethod = CopyMethod.Copy });
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobShipmentSchema.Constants.JS_RL_NKDestination, CopyMethod = CopyMethod.Copy });
			var copyTree = new CopyTemplateTree { InnerNode = entityNode };

			CopyShipment_AssertJobRequiredDocumentCount(copyTree, "NZABY", 1);
			CopyShipment_AssertJobRequiredDocumentCount(copyTree, "USLAX", 0);
		}

		void CopyShipment_AssertJobRequiredDocumentCount(CopyTemplateTree copyTree, ZString destination, int jobRequiredDocumentCount)
		{
			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = destination;
			Factory.Save();
			AssertEquals("Precondition", jobRequiredDocumentCount, ((IDocsAndCartageParent)shipment).RequiredDocumentsProvider.RequiredDocuments.Count);

			var copyShipment = (Enterprise.Integration.Forwarding.IForwardingShipment)new BusinessObjectCopyManager().Copy(shipment, copyTree).Object;
			Factory.Save();
			AssertEquals($"Shipment using Universal Copy should have {jobRequiredDocumentCount} JobRequiredDocument", jobRequiredDocumentCount, ((IDocsAndCartageParent)copyShipment).RequiredDocumentsProvider.RequiredDocuments.Count);
		}

		void SetupRequiredDocumentForCountry(RefCountry country, ZString docType, ZString orig, ZString dest, ZString usage, ZString transport, ZBool isConsol, ZBool isShipment, ZBool isBrokerage, ZBool isOrder)
		{
			var result = country.RequiredDocuments.AddNew();
			result.RD_DocType = docType;
			result.RD_RN_NKOrigin = orig;
			result.RD_RN_NKDestination = dest;
			result.RD_DocUsage = usage;
			result.RD_TransportMode = transport;
			result.RD_OnConsol = isConsol;
			result.RD_OnShipment = isShipment;
			result.RD_OnBrokerage = isBrokerage;
			result.RD_OnOrder = isOrder;
		}

		public void TestMSCDescriptionNotNull()
		{
			var parentParent = (IDocsAndCartageParent)Factory.New<Enterprise.Integration.Freight.ICommonShipment>();
			var reqDoc = parentParent.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			reqDoc.EQ_DocType = "BOB";
			Factory.Save();

			reqDoc.EQ_DocType = "MSC";
			AssertEquals(string.Empty, reqDoc.EQ_DocDescription);
			Assert(reqDoc.EQ_DocDescriptionInfo.HasError("Please enter a description if the document type is Miscellaneous Document."));
		}

		public void TestNoValidToDateForOncePerShipmentDocuments()
		{
			ZDateTime testDate = ZDateTime.Today.AddDays(1);

			var parent = (IDocsAndCartageParent)Factory.New<Enterprise.Integration.Freight.ICommonShipment>();
			var reqDoc = parent.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			reqDoc.EQ_DocType = "BOB";
			reqDoc.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			reqDoc.EQ_ValidToDate = testDate;

			AssertEquals("Precondition", testDate, reqDoc.EQ_ValidToDate);

			reqDoc.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			AssertEquals("Valid To Date is blanked", ZDateTime.Empty, reqDoc.EQ_ValidToDate);
			Assert("Valid To Date is read only", reqDoc.EQ_ValidToDateInfo.ReadOnly);
		}

		public void TestDocDescriptionMultilingual()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				var newDocType = Factory.New(typeof(RefDocType)) as RefDocType;
				newDocType.RT_DocType = "GGG";
				newDocType.RT_ReferenceType = "ALL";
				newDocType.RT_Desc = "Test Description";
				newDocType.RT_IsActive = true;
				newDocType.RT_IsPublished = true;
				newDocType.RT_SaveVersions = true;
				Factory.Save();

				string key = ((ResourceString)newDocType.RT_DescMultilingual).ResourceKey;
				mockRes.Put(key, new ResourceStringData(key, "测试说明"));

				Doc.EQ_DocType = "GGG";

				AssertEquals("Test Description", Doc.EQ_DocDescription);
				AssertEquals("测试说明", Doc.EQ_DocDescriptionMultilingual);

				AssertEquals(true, Doc.EQ_DocDescriptionInfo.ReadOnly);
				AssertEquals(true, Doc.EQ_DocDescriptionMultilingual_ReadOnly);

				Doc.EQ_DocType = "MSC";
				AssertEquals(false, Doc.EQ_DocDescriptionInfo.ReadOnly);
				AssertEquals(false, Doc.EQ_DocDescriptionMultilingual_ReadOnly);

				Doc.EQ_DocDescriptionMultilingual = (NoResString)"Custom Description";
				AssertEquals("Custom Description", Doc.EQ_DocDescription);
			}
		}

		public void TestComplianceReportExclusionForOtherCompany()
		{
			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var report = complianceConfig.AddNew();
			report.ReportCode = "TST";
			report.ReportTitle = "Test Tax Report";
			report.ReportPeriodicity = "RNG";
			report.Country = Env.CurrentCompany.Country.Code;
			report.TaxRegistrationType = "ABN";
			report.ReportBaseTablePrefix = AccTransactionHeaderSchema.Constants.Prefix;
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);

			Assert("Read Only", !Doc.ReadOnly);
			Assert("Can Delete", Doc.CanDelete);
			Assert("EQ_RN_NKRelatedCountry Read Only", !Doc.EQ_RN_NKRelatedCountryInfo.ReadOnly);
			Assert("EQ_OH_DocumentOwner Read Only", !Doc.EQ_OH_DocumentOwnerInfo.ReadOnly);

			Doc.EQ_DocCategory = Core.Constants.ReferenceTypes.ComplianceReport;
			AssertEquals("Related Country is set to Current Company's Country", Env.CurrentCompany.Country.Code, Doc.EQ_RN_NKRelatedCountry);
			AssertEquals("Document Owner is set to Current Company's Org Proxy", Env.CurrentCompany.OrganisationPK, Doc.EQ_OH_DocumentOwner);

			Assert("Read Only", !Doc.ReadOnly);
			Assert("Can Delete", Doc.CanDelete);
			Assert("EQ_RN_NKRelatedCountry Read Only", Doc.EQ_RN_NKRelatedCountryInfo.ReadOnly);
			Assert("EQ_OH_DocumentOwner Read Only", Doc.EQ_OH_DocumentOwnerInfo.ReadOnly);

			Doc.EQ_DocType = "TST";

			var branchQuery = new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, Env.CurrentCompany.PK);
			branchQuery.AddToFilter(GlbBranchSchema.GB_IsActive, true);

			using (Environment.Env.SetTemporaryUserContext(Env.CurrentUser.PK, Factory.LoadTop1<GlbBranch>(branchQuery).PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				Assert("Read Only", Doc.ReadOnly);
				Assert("Can Delete", !Doc.CanDelete);
				AssertEquals("Compliance Report exclusion is related to a different Company: EDI CUSTOMS BROKERS.", Doc.ReasonForNotAbleToDelete);
			}

			Assert("Read Only", !Doc.ReadOnly);
			Assert("Can Delete", Doc.CanDelete);
		}

		public void TestEnableLightValidationIfAvailable()
		{
			var doc = Factory.NewWithValidTestData<JobRequiredDocument>();
			Factory.Save();

			Assert(!doc.HasChanges);
			Assert(Convert.ToBoolean(doc.GetType().GetProperty("EnableLightValidationIfAvailable", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(doc)));

			doc.EQ_DocType = Core.Constants.RefDocTypes.DocumentOfOrigin;

			Assert(doc.HasChanges);
			Assert(!Convert.ToBoolean(doc.GetType().GetProperty("EnableLightValidationIfAvailable", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(doc)));
		}

		public void TestRecordWithDupDocTypeIsNotSaved()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "org111";
			Factory.Save();

			AssertEquals(0, organisation.RequiredDocuments.Count);

			var doc1 = organisation.RequiredDocuments.AddNew();
			doc1.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			doc1.EQ_DocType = Core.Constants.RefDocTypes.AgentsInvoice;

			var factory2 = new BusinessObjectFactory();
			var organisationInAnotherFactory = factory2.Load<OrgHeader>(organisation.PK);

			var doc2 = organisationInAnotherFactory.RequiredDocuments.AddNew();
			doc2.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			doc2.EQ_DocType = Core.Constants.RefDocTypes.AgentsInvoice;
			factory2.Save();

			Factory.Save();
			AssertEquals("doc1 should be deleted", true, doc1.IsDeleted);
		}

		public void TestIsCostaRicaExporterExemptionDocumentForDebtor()
		{
			var doc = Factory.New<JobRequiredDocument>();
			doc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Canada;
			doc.EQ_DocType = Core.Constants.RefDocTypes.VATExporterExemption;
			doc.EQ_DocUsage = JobRequiredDocument.DocUsage.Debtor;

			Assert("Non CR country", !doc.IsCostaRicaExporterExemptionDocumentForDebtor);

			doc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.CostaRica;
			Assert(doc.IsCostaRicaExporterExemptionDocumentForDebtor);

			doc.EQ_DocUsage = JobRequiredDocument.DocUsage.Creditor;
			Assert("Non DBT usage", !doc.IsCostaRicaExporterExemptionDocumentForDebtor);

			doc.EQ_DocUsage = JobRequiredDocument.DocUsage.Debtor;
			Assert(doc.IsCostaRicaExporterExemptionDocumentForDebtor);

			doc.EQ_DocType = Core.Constants.RefDocTypes.DemandLetter;
			Assert("Non EXV type", !doc.IsCostaRicaExporterExemptionDocumentForDebtor);

			doc.EQ_DocType = Core.Constants.RefDocTypes.VATExporterExemption;
			Assert("Precondition", doc.EQ_DocUsage.IsEmpty);
			doc.EQ_DocUsage = JobRequiredDocument.DocUsage.Debtor;

			Assert(doc.IsCostaRicaExporterExemptionDocumentForDebtor);
		}

		public void TestIsTaiwanAttorney()
		{
			var jobRequiredDocument = Factory.New<JobRequiredDocument>();
			jobRequiredDocument.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Canada;
			jobRequiredDocument.EQ_DocType = Core.Constants.RefDocTypes.VATExporterExemption;
			jobRequiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			Assert("Non POA/POC/POF type", !jobRequiredDocument.IsTaiwanAttorney);

			jobRequiredDocument.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				Assert("EQ_RN_NKRelatedCountry: Non TW country", !jobRequiredDocument.IsTaiwanAttorney);

				jobRequiredDocument.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
				Assert(jobRequiredDocument.IsTaiwanAttorney);

				jobRequiredDocument.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyCustoms;
				Assert(jobRequiredDocument.IsTaiwanAttorney);

				jobRequiredDocument.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyForwarding;
				Assert(!jobRequiredDocument.IsTaiwanAttorney);

				jobRequiredDocument.EQ_DocType = Core.Constants.RefDocTypes.QuarantineCertificate;
				Assert("Non POA/POC type", !jobRequiredDocument.IsTaiwanAttorney);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				jobRequiredDocument.EQ_RN_NKRelatedCountry = ZString.Empty;
				jobRequiredDocument.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
				Assert(jobRequiredDocument.IsTaiwanAttorney);

				jobRequiredDocument.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyCustoms;
				Assert(jobRequiredDocument.IsTaiwanAttorney);

				jobRequiredDocument.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyForwarding;
				Assert(!jobRequiredDocument.IsTaiwanAttorney);

				jobRequiredDocument.EQ_DocType = Core.Constants.RefDocTypes.ProFormaInvoice;
				Assert("Non POA/POC type", !jobRequiredDocument.IsTaiwanAttorney);
			}
		}

		public void TestIsTaiwanPowerofAttorneyDocumentForBroker()
		{
			var jobRequiredDocument = Factory.New<JobRequiredDocument>();
			jobRequiredDocument.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
			jobRequiredDocument.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			jobRequiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			Assert(jobRequiredDocument.IsTaiwanAttorneyDocumentForBroker);

			jobRequiredDocument.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Canada;
			Assert("Non TW country", !jobRequiredDocument.IsTaiwanAttorneyDocumentForBroker);

			jobRequiredDocument.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
			jobRequiredDocument.EQ_DocType = Core.Constants.RefDocTypes.QuarantineCertificate;
			Assert("Non POA/POC/POF type", !jobRequiredDocument.IsTaiwanAttorneyDocumentForBroker);

			jobRequiredDocument.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyCustoms;
			Assert(jobRequiredDocument.IsTaiwanAttorneyDocumentForBroker);

			jobRequiredDocument.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyForwarding;
			Assert(!jobRequiredDocument.IsTaiwanAttorneyDocumentForBroker);

			jobRequiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Carrier;
			Assert("Non BRK usage", !jobRequiredDocument.IsTaiwanAttorneyDocumentForBroker);
		}

		public void TestAddCustomsDistrictAttribute()
		{
			var jobRequiredDocument = Factory.New<JobRequiredDocument>();
			jobRequiredDocument.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
			jobRequiredDocument.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			jobRequiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			Assert("RequiredDocument has a CUSTOMS DISTRICT attribute.", jobRequiredDocument.Attributes.HasAttributeType(JobRequiredDocAttribTypeList.Codes.CustomsDistrict));

			CombineAssertions("EQ_RN_NKRelatedCountry", () =>
			{
				jobRequiredDocument.Attributes.DeleteAll();
				jobRequiredDocument.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Canada;
				Assert("RequiredDocument has no a CUSTOMS DISTRICT attribute.", !jobRequiredDocument.Attributes.HasAttributeType(JobRequiredDocAttribTypeList.Codes.CustomsDistrict));
				jobRequiredDocument.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
				Assert("RequiredDocument has a CUSTOMS DISTRICT attribute.", jobRequiredDocument.Attributes.HasAttributeType(JobRequiredDocAttribTypeList.Codes.CustomsDistrict));
			});

			CombineAssertions("EQ_DocType", () =>
			{
				jobRequiredDocument.Attributes.DeleteAll();
				jobRequiredDocument.EQ_DocType = Core.Constants.RefDocTypes.QuarantineCertificate;
				Assert("RequiredDocument has no a CUSTOMS DISTRICT attribute.", !jobRequiredDocument.Attributes.HasAttributeType(JobRequiredDocAttribTypeList.Codes.CustomsDistrict));
				jobRequiredDocument.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyCustoms;
				Assert("RequiredDocument has a CUSTOMS DISTRICT attribute.", jobRequiredDocument.Attributes.HasAttributeType(JobRequiredDocAttribTypeList.Codes.CustomsDistrict));

				jobRequiredDocument.Attributes.DeleteAll();
				jobRequiredDocument.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
				Assert("RequiredDocument has a CUSTOMS DISTRICT attribute.", jobRequiredDocument.Attributes.HasAttributeType(JobRequiredDocAttribTypeList.Codes.CustomsDistrict));
			});

			CombineAssertions("EQ_DocUsage", () =>
			{
				jobRequiredDocument.Attributes.DeleteAll();
				jobRequiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Carrier;
				Assert("RequiredDocument has no a CUSTOMS DISTRICT attribute.", !jobRequiredDocument.Attributes.HasAttributeType(JobRequiredDocAttribTypeList.Codes.CustomsDistrict));
				jobRequiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
				Assert("RequiredDocument has a CUSTOMS DISTRICT attribute.", jobRequiredDocument.Attributes.HasAttributeType(JobRequiredDocAttribTypeList.Codes.CustomsDistrict));
			});
		}

		public void TestAddBoxNumberAttribute()
		{
			var jobRequiredDocument = Factory.New<JobRequiredDocument>();
			jobRequiredDocument.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
			jobRequiredDocument.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			jobRequiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			Assert("RequiredDocument has a BOX NUMBER attribute.", jobRequiredDocument.Attributes.HasAttributeType(JobRequiredDocAttribTypeList.Codes.BoxNumber));
			AssertNotNull(jobRequiredDocument.GetBoxNumberProvider());
			CombineAssertions("EQ_RN_NKRelatedCountry", () =>
			{
				jobRequiredDocument.Attributes.DeleteAll();
				jobRequiredDocument.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Canada;
				Assert("RequiredDocument has no a BOX NUMBER attribute.", !jobRequiredDocument.Attributes.HasAttributeType(JobRequiredDocAttribTypeList.Codes.BoxNumber));
				AssertNull(jobRequiredDocument.GetBoxNumberProvider());
				jobRequiredDocument.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
				Assert("RequiredDocument has a BOX NUMBER attribute.", jobRequiredDocument.Attributes.HasAttributeType(JobRequiredDocAttribTypeList.Codes.BoxNumber));
			});

			var attrib1 = Factory.New<JobRequiredDocAttribForTestBoxNumber>();
			attrib1.D0_AttribName = JobRequiredDocAttribTypeList.Codes.BoxNumber;
			AssertEquals(0, attrib1.Lookups.AttributeValueList.Count);

			var customsDistrictDocAttrib = Factory.New<JobRequiredDocAttribForTestBoxNumber>();
			customsDistrictDocAttrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CustomsDistrict;
			var boxNumberDocAttrib = Factory.New<JobRequiredDocAttribForTestBoxNumber>();
			boxNumberDocAttrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.BoxNumber;

			var jobRequiredDocumentForTestBoxNumber = Factory.New<JobRequiredDocumentForTestBoxNumber>();
			jobRequiredDocumentForTestBoxNumber.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
			jobRequiredDocumentForTestBoxNumber.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			jobRequiredDocumentForTestBoxNumber.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			jobRequiredDocumentForTestBoxNumber.Attributes.DeleteAll();
			customsDistrictDocAttrib.D0_EQ = jobRequiredDocumentForTestBoxNumber.PK;
			boxNumberDocAttrib.D0_EQ = jobRequiredDocumentForTestBoxNumber.PK;

			AssertNotNull(jobRequiredDocumentForTestBoxNumber.BoxNumberDocAttrib);
			customsDistrictDocAttrib.D0_AttribDisplayValue = "A";

			AssertEquals("111", jobRequiredDocumentForTestBoxNumber.BoxNumberDocAttrib.D0_AttribDisplayValue);
			customsDistrictDocAttrib.D0_AttribDisplayValue = "B";
			AssertEquals("333", jobRequiredDocumentForTestBoxNumber.BoxNumberDocAttrib.D0_AttribDisplayValue);
			customsDistrictDocAttrib.D0_AttribDisplayValue = "C";
			AssertEquals("", jobRequiredDocumentForTestBoxNumber.BoxNumberDocAttrib.D0_AttribDisplayValue);

			AssertEquals("111", jobRequiredDocumentForTestBoxNumber.GetDefaultBoxNumber("A"));
			AssertEquals("333", jobRequiredDocumentForTestBoxNumber.GetDefaultBoxNumber("B"));
		}

		public void TestAddBondedIDAttribute()
		{
			var jobRequiredDocument = Factory.New<JobRequiredDocument>();
			jobRequiredDocument.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
			jobRequiredDocument.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			jobRequiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			Assert("RequiredDocument has a BONDED ID attribute.", jobRequiredDocument.Attributes.HasAttributeType(JobRequiredDocAttribTypeList.Codes.BondedID));

			CombineAssertions("EQ_RN_NKRelatedCountry", () =>
			{
				jobRequiredDocument.Attributes.DeleteAll();
				jobRequiredDocument.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Canada;
				Assert("RequiredDocument has no a BONDED ID attribute.", !jobRequiredDocument.Attributes.HasAttributeType(JobRequiredDocAttribTypeList.Codes.BondedID));
				jobRequiredDocument.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
				Assert("RequiredDocument has a BONDED ID attribute.", jobRequiredDocument.Attributes.HasAttributeType(JobRequiredDocAttribTypeList.Codes.BondedID));
			});

			CombineAssertions("EQ_DocType", () =>
			{
				jobRequiredDocument.Attributes.DeleteAll();
				jobRequiredDocument.EQ_DocType = Core.Constants.RefDocTypes.QuarantineCertificate;
				Assert("RequiredDocument has no a BONDED ID attribute.", !jobRequiredDocument.Attributes.HasAttributeType(JobRequiredDocAttribTypeList.Codes.BondedID));
				jobRequiredDocument.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyCustoms;
				Assert("RequiredDocument has a BONDED ID attribute.", jobRequiredDocument.Attributes.HasAttributeType(JobRequiredDocAttribTypeList.Codes.BondedID));

				jobRequiredDocument.Attributes.DeleteAll();
				jobRequiredDocument.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
				Assert("RequiredDocument has a BONDED ID attribute.", jobRequiredDocument.Attributes.HasAttributeType(JobRequiredDocAttribTypeList.Codes.BondedID));
			});

			CombineAssertions("EQ_DocUsage", () =>
			{
				jobRequiredDocument.Attributes.DeleteAll();
				jobRequiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Carrier;
				Assert("RequiredDocument has no a BONDED ID attribute.", !jobRequiredDocument.Attributes.HasAttributeType(JobRequiredDocAttribTypeList.Codes.BondedID));
				jobRequiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
				Assert("RequiredDocument has aBONDED ID attribute.", jobRequiredDocument.Attributes.HasAttributeType(JobRequiredDocAttribTypeList.Codes.BondedID));
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "We really want a 4 character year")]
		public void TestEQ_RN_NKRelatedCountry()
		{
			var jobRequiredDocument = Factory.New<JobRequiredDocument>();
			jobRequiredDocument.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Australia;
			jobRequiredDocument.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			jobRequiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			jobRequiredDocument.EQ_ValidToDate = ZDateTime.Now.AddYears(6);
			const string DATE_FORMAT = "dd-MMM-yyyy";
			AssertNoWarningContaining(jobRequiredDocument.EQ_ValidToDateInfo, DateRangeValidation.WarningForFutureYear(jobRequiredDocument.EQ_ValidToDate.ToString(DATE_FORMAT, CultureInfo.InvariantCulture), 5));

			jobRequiredDocument.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
			AssertHasWarningContaining(jobRequiredDocument.EQ_ValidToDateInfo, DateRangeValidation.WarningForFutureYear(jobRequiredDocument.EQ_ValidToDate.ToString(DATE_FORMAT, CultureInfo.InvariantCulture), 5));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "We really want a 4 character year")]
		public void TestEQ_DocType()
		{
			var jobRequiredDocument = Factory.New<JobRequiredDocument>();
			jobRequiredDocument.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Australia;
			jobRequiredDocument.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			jobRequiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			jobRequiredDocument.EQ_ValidToDate = ZDateTime.Now.AddYears(6);
			const string DATE_FORMAT = "dd-MMM-yyyy";
			AssertNoWarningContaining(jobRequiredDocument.EQ_ValidToDateInfo, DateRangeValidation.WarningForFutureYear(jobRequiredDocument.EQ_ValidToDate.ToString(DATE_FORMAT, CultureInfo.InvariantCulture), 5));

			jobRequiredDocument.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
			AssertHasWarningContaining(jobRequiredDocument.EQ_ValidToDateInfo, DateRangeValidation.WarningForFutureYear(jobRequiredDocument.EQ_ValidToDate.ToString(DATE_FORMAT, CultureInfo.InvariantCulture), 5));
		}

		public void TestEQ_DocType_WithholdingTaxExemption()
		{
			var requiredDocument = Factory.New<JobRequiredDocument>();
			Assert("Pre-condition: EQ_DocType is empty", requiredDocument.EQ_DocType.IsEmpty);
			Assert("Pre-condition: EQ_DocUsage is empty", requiredDocument.EQ_DocUsage.IsEmpty);
			AssertEquals("Default value of EQ_DocPeriod", JobRequiredDocuments.DocumentPeriods.OncePerShipment, requiredDocument.EQ_DocPeriod);

			requiredDocument.EQ_DocType = RefDocTypes.WithholdingTaxExemption;
			AssertEquals("EQ_DocUsage", JobRequiredDocument.DocUsage.Creditor, requiredDocument.EQ_DocUsage);
			AssertEquals("EQ_DocPeriod", JobRequiredDocuments.DocumentPeriods.Periodic, requiredDocument.EQ_DocPeriod);

			requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Warehouse;
			requiredDocument.EQ_DocPeriod = string.Empty;

			requiredDocument.EQ_DocType = RefDocTypes.WithholdingTaxExemption;
			AssertEquals("EQ_DocUsage", JobRequiredDocument.DocUsage.Creditor, requiredDocument.EQ_DocUsage);
			AssertEquals("EQ_DocPeriod", JobRequiredDocuments.DocumentPeriods.Periodic, requiredDocument.EQ_DocPeriod);
		}

		public void TestDocumentTrackingLogForOrg()
		{
			Factory.Save(); // Persist Organisation.

			var record = Factory.New<JobRequiredDocument>();
			record.EQ_DocCategory = Core.Constants.ReferenceTypes.ComplianceReport;
			record.EQ_DocType = AccountingMasterFilesConstants.ComplianceReportCodes.ReportableSmallBusines;
			record.EQ_DocUsage = JobRequiredDocument.DocUsage.Creditor;
			record.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			record.EQ_ValidToDate = new ZDateTime(2021, 6, 30);
			record.EQ_DateReceived = record.EQ_ValidToDate.ToDateTimeOffset(null);
			record.EQ_RN_NKRelatedCountry = CountryCodes.Australia;
			record.EQ_ParentID = Organisation.PK;
			record.EQ_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			record.ParentType = typeof(OrgHeader);

			var expectedAddRecordReference = "Added a new RSB Document Tracking record for reporting period '30-Jun-21 00:00:00'";
			var expectedModifyRecordReference = "Modified RSB Document Tracking record for reporting period '30-Jun-21 00:00:00'";
			var expectedDeleteRecordReference = "Deleted RSB Document Tracking record for reporting period '30-Jun-21 00:00:00'";

			AssertEquals(0, Organisation.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.EditedARecord.Code && x.SL_Reference.Contains(expectedAddRecordReference)));
			AssertEquals(0, Organisation.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.EditedARecord.Code && x.SL_Reference.Contains(expectedModifyRecordReference)));
			AssertEquals(0, Organisation.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.EditedARecord.Code && x.SL_Reference.Contains(expectedDeleteRecordReference)));

			Factory.Save();

			AssertEquals("Added", 1, Organisation.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.EditedARecord.Code && x.SL_Reference.Contains(expectedAddRecordReference)));
			AssertEquals(0, Organisation.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.EditedARecord.Code && x.SL_Reference.Contains(expectedModifyRecordReference)));
			AssertEquals(0, Organisation.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.EditedARecord.Code && x.SL_Reference.Contains(expectedDeleteRecordReference)));

			record.EQ_DocumentNotes += "Test";
			Factory.Save();

			AssertEquals(1, Organisation.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.EditedARecord.Code && x.SL_Reference.Contains(expectedAddRecordReference)));
			AssertEquals("Modified", 1, Organisation.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.EditedARecord.Code && x.SL_Reference.Contains(expectedModifyRecordReference)));
			AssertEquals(0, Organisation.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.EditedARecord.Code && x.SL_Reference.Contains(expectedDeleteRecordReference)));

			record.EQ_DocType = "TST";
			Factory.Save();

			AssertEquals(1, Organisation.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.EditedARecord.Code && x.SL_Reference.Contains(expectedAddRecordReference)));
			AssertEquals(1, Organisation.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.EditedARecord.Code && x.SL_Reference.Contains(expectedModifyRecordReference)));
			AssertEquals("Deleted by changing DocType from RSB to non-RSB", 1, Organisation.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.EditedARecord.Code && x.SL_Reference.Contains(expectedDeleteRecordReference)));

			record.EQ_DocumentNotes += "Test2";
			Factory.Save();

			AssertEquals(1, Organisation.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.EditedARecord.Code && x.SL_Reference.Contains(expectedAddRecordReference)));
			AssertEquals("No log for non-RSB record", 1, Organisation.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.EditedARecord.Code && x.SL_Reference.Contains(expectedModifyRecordReference)));
			AssertEquals(1, Organisation.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.EditedARecord.Code && x.SL_Reference.Contains(expectedDeleteRecordReference)));

			record.EQ_DocType = AccountingMasterFilesConstants.ComplianceReportCodes.ReportableSmallBusines;
			Factory.Save();

			AssertEquals("Added by changing DocType from non-RSB to RSB", 2, Organisation.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.EditedARecord.Code && x.SL_Reference.Contains(expectedAddRecordReference)));
			AssertEquals(1, Organisation.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.EditedARecord.Code && x.SL_Reference.Contains(expectedModifyRecordReference)));
			AssertEquals(1, Organisation.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.EditedARecord.Code && x.SL_Reference.Contains(expectedDeleteRecordReference)));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			IDocsAndCartageParent shipment = (IDocsAndCartageParent)factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			JobRequiredDocument jobRequiredDocument = shipment.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			jobRequiredDocument.Attributes.AddNew();
			return jobRequiredDocument;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		IDocsAndCartageParent Shipment;
		IDocsAndCartageParent Declaration;
		OrgHeader Organisation;
		JobRequiredDocument Doc;
		JobRequiredDocument DeclarationDoc;
		JobRequiredDocument OrganiasationDoc;

		protected override void SetUp()
		{
			base.SetUp();

			Shipment = (IDocsAndCartageParent)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>(); //ShipmentDocumentSupport.New(Factory);
			Doc = Shipment.RequiredDocumentsProvider.RequiredDocuments.AddNew();

			Declaration = (IDocsAndCartageParent)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			DeclarationDoc = Declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();

			Organisation = Factory.NewWithValidTestData<OrgHeader>();
			OrganiasationDoc = Organisation.RequiredDocuments.AddNew();
		}

		#endregion
	}
}
