using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(GroupHeaderCollection))]
	public class GroupHeaderCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaultValues()
		{
			GroupHeaderCollection headerCollection = (GroupHeaderCollection)GetCollectionToTest();
			BaseJobComInvoiceGroupHeader groupHeader = headerCollection.AddNew();
			AssertEquals("JZ_JE is set", TestDec.PK, groupHeader.JZ_JE);
		}

		public void TestJobDecGuidIsSetOnRemoving()
		{
			GroupHeaderCollection headerCollection = (GroupHeaderCollection)GetCollectionToTest();
			BaseJobComInvoiceGroupHeader groupHeader = headerCollection.AddNew();

			headerCollection.Remove(groupHeader);
			AssertEquals("JZ_JE is cached", TestDec.PK, groupHeader.HiddenOriginalParentGuid);
		}

		public void TestIndexer()
		{
			GroupHeaderCollection headerCollection = (GroupHeaderCollection)GetCollectionToTest();
			BusinessObject testItem1 = Factory.New(typeof(BaseJobComInvoiceGroupHeader));
			BusinessObject testItem2 = Factory.New(typeof(BaseJobComInvoiceGroupHeader));
			headerCollection.Add(testItem2);
			headerCollection.Add(testItem1);
			AssertEquals(testItem2, headerCollection[0]);
			AssertEquals(testItem1, headerCollection[1]);
		}

		public void TestRelationshipFilter()
		{
			BaseJobComInvoiceGroupHeader groupHeaderForTestDec = TestDec.JobComInvoiceGroupHeaders[0];
			AssertEquals("GroupHeaderForTestDec is group", true, groupHeaderForTestDec.JZ_GroupInvoice);

			BaseJobComInvoiceHeader invoice = TestDec.Invoices.AddNew();
			AssertEquals("Invoice is not group", false, invoice.JZ_GroupInvoice);

			GroupHeaderCollection collection = (GroupHeaderCollection)GetCollectionToTest();
			collection.Load();
			AssertEquals("1 item only", 1, collection.Count);

			BaseJobDeclaration testDec2 = BaseJobDeclaration.New(Factory);
			testDec2.Invoices.AddNew();
			BaseJobComInvoiceGroupHeader groupHeader2 = testDec2.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceGroupHeader groupHeader2_1 = groupHeader2.JobComInvoiceGroupHeaders.AddNew();
			AssertEquals("GroupHeader2 is group", true, groupHeader2.JZ_GroupInvoice);
			AssertEquals("GroupHeader2_1 is group", true, groupHeader2_1.JZ_GroupInvoice);

			GroupHeaderCollection collection2 = new GroupHeaderCollection(testDec2);
			collection2.Load();
			AssertEquals("2 items", 2, collection2.Count);
		}

		public void TestSupportAdditionalInvoices()
		{
			var otherFactory = new BusinessObjectFactory();
			var declaration1 = otherFactory.New<BaseJobDeclaration>();
			var declaration2 = otherFactory.New<BaseJobDeclaration>();
			var groupHeader = declaration2.JobComInvoiceGroupHeaders[0];
			var pivot = otherFactory.New<GroupRelatedDeclarationGenPivot>();
			pivot.XX_Relation2ID = declaration1.PK;
			pivot.XX_Relation1ID = groupHeader.PK;
			otherFactory.Save();

			declaration1 = Factory.Load<JobDeclarationSupportAdditionalInvoices>(declaration1.PK);
			groupHeader = (BaseJobComInvoiceGroupHeader)declaration1.AllGroupHeaders.FindByPK(groupHeader.PK);
			AssertNotNull("AllGroupHeaders should contains groupHeader", groupHeader);
			AssertEquals("groupHeader.JobDeclaration should not be changed", declaration2.PK, groupHeader.JobDeclaration.PK);

			Assert("groupHeader is an additional GroupHeader for declaration1", declaration1.AllGroupHeaders.IsAdditionalGroupHeader(groupHeader));
			Assert("groupHeader is not an additional GroupHeader for declaration2", !declaration2.AllGroupHeaders.IsAdditionalGroupHeader(groupHeader));

			declaration1.AllGroupHeaders.Remove(groupHeader);
			AssertEquals("groupHeader.JobDeclaration should not be changed", declaration2.PK, groupHeader.JobDeclaration.PK);
			Assert("groupHeader.HiddenOriginalParentGuid should not be set", groupHeader.HiddenOriginalParentGuid.IsEmpty);
		}

		#region Implementation

		BaseJobDeclaration fTestDec;
		protected BaseJobDeclaration TestDec
		{
			get
			{
				if (fTestDec == null)
				{
					fTestDec = GetNewDeclaration();
				}
				return fTestDec;
			}
		}

		protected virtual BaseJobDeclaration GetNewDeclaration()
		{
			return Factory.New<BaseJobDeclaration>();
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GroupHeaderCollection(TestDec);
		}

		#endregion
	}
}
