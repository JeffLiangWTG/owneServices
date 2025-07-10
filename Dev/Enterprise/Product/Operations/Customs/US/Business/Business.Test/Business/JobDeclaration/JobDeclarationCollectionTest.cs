using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(JobDeclarationCollection))]
	public class JobDeclarationCollectionTest : Customs.Business.Testing.BaseJobDeclarationBizoCollectionTest
	{
		public void TestAddFetchHintFilterForDIS()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Export;
			var docsAndCartage1 = declaration1.DocsAndCartage;
			var requiredDoc1 = docsAndCartage1.RequiredDocuments.AddNew();
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Export;
			var docsAndCartage2 = declaration2.DocsAndCartage;
			var requiredDoc2 = docsAndCartage2.RequiredDocuments.AddNew();
			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Export;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			newFactory.EnableTableHitQueryCollection(new string[] { JobRequiredDocumentSchema.Constants.TableName });
			newFactory.EnableTableHitQueryCollection(new string[] { JobDocsAndCartageSchema.Constants.TableName });
			var collection = new JobDeclarationCollection(newFactory, GlbCompany.CurrentCompany.PK);
			collection.Load();
			collection.FetchStrategy.FetchForView(collection.ToArray(), new[] { new TableColumn(ZString.Empty, JobDeclaration.Schema.DISStatus) });
			var query = new ZString(newFactory.TableSelects.FirstOrDefault(x => x.TableName == JobRequiredDocumentSchema.Constants.TableName).Queries.FirstOrDefault().Query);
			CombineAssertions(() =>
			{
				AssertEquals("Collect all declaration PKs to search JobRequiredDocument at once.", false, query.Contains("or EQ_ParentID IN", System.StringComparison.InvariantCultureIgnoreCase));
				AssertEquals("Collect all declaration PKs to search JobRequiredDocument at once.", true, query.Contains("EQ_ParentID IN", System.StringComparison.InvariantCultureIgnoreCase));
				AssertEquals(2, newFactory.ActiveFetchHintsForTable(JobRequiredDocumentAddInfoSchema.Constants.TableName));
				AssertEquals("JobRequiredDocument", 1, newFactory.GetTableHitCount(JobRequiredDocumentSchema.Constants.TableName));
			});
		}

		public void TestTypedIndexer()
		{
			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var declaration = collection.AddNew();
			AssertEquals(declaration, collection[0]);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);

		public void TestIFindBoxListProviderDoesNotLoadDecInDifferentCountryAsUSDeclaration()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var auCompany = Factory.New<GlbCompany>();
			auCompany.GC_RN_NKCountryCode = "AU";
			auCompany.GC_Code = "~AU";

			var auBranch = auCompany.Branches.AddNew();
			auBranch.GB_Code = "~AU";

			Factory.Save();

			Integration.Customs.AU.IJobDeclaration auDeclaration = null;
			using (DisposableEnvironment.ForBranch(auBranch.PK.ToGuid()))
			{
				auDeclaration = Factory.New<Integration.Customs.AU.IJobDeclaration>();
				auDeclaration.JE_JS = shipment.PK;
				Factory.Save();
			}

			var usDeclaration = Factory.New<JobDeclaration>();
			usDeclaration.JE_JS = shipment.PK;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var coll = new JobDeclarationCollection(factory2, GlbCompany.CurrentCompany.PK);
			var declaration = ((IFindBoxListProvider)coll).GetBusinessObjectFromCodeWithoutFilter(shipment.JS_UniqueConsignRef);
			AssertEquals("WI00071296: Without filtering on a company, an AU declaration is loaded as a US declaration", "Enterprise.Customs.AU.Declaration.Business.JobDeclaration", factory2.Load<BaseJobDeclaration>(auDeclaration.PK).GetType().FullName);
		}

		public void TestLoadCusStatementHeaderThroughFetchForViewDeclaration()
		{
			var newFac = new BusinessObjectFactory();
			var header1 = newFac.New<Integration.Customs.US.ISF.ICusISFHeader>();
			header1.BF_JobReference = "JOB1";
			header1.BF_SystemCreateTimeUtc = new ZDateTime(2010, 12, 2);

			var bill1 = newFac.New<Integration.Customs.US.ISF.ICusISFBill>();
			bill1.BB_BF = header1.PK;
			bill1.BB_BillType = "BM";
			bill1.BB_BillNum = "AAADHB2";
			bill1.BB_CustomsStatus = "S1";

			for (var i = 0; i < 2200; i++)
			{
				var bill2 = newFac.New<Integration.Customs.US.ISF.ICusISFBill>();
				bill2.BB_BF = header1.PK;
				bill2.BB_BillType = "OB";
				bill2.BB_BillNum = "AAADHB" + i;
				bill2.BB_CustomsStatus = "S1";
			}
			newFac.Save();

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);

			for (var i = 0; i < 2200; i++)
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = "SEA";
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.US_EntryFilerCode = "XJ5";
				declaration.US_EnableENS = true;
				declaration.ImportEntryNumber = i.ToString("D8");

				var bill = declaration.Bills.AddNew();
				bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
				bill.CU_BillNum = "HB1";
				bill.US_UI_NKBillIssuerSCAC = "DDDF";

				var billh = declaration.Bills.AddNew();
				billh.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
				billh.CU_BillNum = "HB" + i;
				billh.US_UI_NKBillIssuerSCAC = "AAAD";

				declarationCollection.Add(declaration);
			}

			AssertNoExceptionThrown(() =>
			{
				var fetchStrategy = declarationCollection.FetchStrategy;
				fetchStrategy.FetchForView(declarationCollection.ToArray(), new[]
				{
					new TableColumn(JobDeclarationSchema.Constants.TableName, JobDeclaration.Schema.ISFBillStatus),
					 new TableColumn(JobDeclarationSchema.Constants.TableName, JobDeclaration.Schema.ISFBillStatusDescription)
				 });
			});
		}
	}
}
