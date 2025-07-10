using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusEntryHeaderCollection<CusEntryHeader>))]
	public class CusEntryHeaderCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestHasAnEntryWithEntryNumber()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(false, declaration.CustomsEntryHeaders.HasAnEntryWithEntryNumber);

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(false, declaration.CustomsEntryHeaders.HasAnEntryWithEntryNumber);

			entry.EntryNumber = "1";
			AssertEquals(true, declaration.CustomsEntryHeaders.HasAnEntryWithEntryNumber);
		}

		public void TestLastEntryToClearDate()
		{
			AssertEquals("Empty collection", 0, testCollection.Count);
			AssertEquals(ZDateTime.Empty, testCollection.LastEntryToClearDate);

			var mock = Factory.NewMoq<CusEntryHeader>();
			mock.Setup(m => m.ClearanceDate).Returns(new ZDateTime(2005, 2, 1));
			testCollection.Add(mock.Object);
			AssertEquals(new ZDateTime(2005, 2, 1), testCollection.LastEntryToClearDate);

			mock = Factory.NewMoq<CusEntryHeader>();
			mock.Setup(m => m.ClearanceDate).Returns(new ZDateTime(2005, 1, 1));
			testCollection.Add(mock.Object);
			AssertEquals(new ZDateTime(2005, 2, 1), testCollection.LastEntryToClearDate);

			mock = Factory.NewMoq<CusEntryHeader>();
			mock.Setup(m => m.ClearanceDate).Returns(new ZDateTime(2006, 1, 1));
			testCollection.Add(mock.Object);
			AssertEquals(new ZDateTime(2006, 1, 1), testCollection.LastEntryToClearDate);
		}

		public void TestEntriesExistAndAllHaveEntryNumbers()
		{
			AssertEquals("Empty collection", false, testCollection.EntriesExistAndAllHaveEntryNumbers);
			BaseJobComInvoiceLine line = jobDec.FilteredInvoiceLines.AddNew();
			line.SetDeclarationForTesting(jobDec);

			testCollection.AddNew();
			AssertEquals(false, testCollection.EntriesExistAndAllHaveEntryNumbers);

			testCollection[0].EntryNumber = "Entry1";
			AssertEquals("One entry with entry no and going into bond", true,
				testCollection.EntriesExistAndAllHaveEntryNumbers);

			testCollection.AddNew();
			AssertEquals(false, testCollection.EntriesExistAndAllHaveEntryNumbers);
		}

		[ExpectNoExceptions]
		public void TestIndexer()
		{
			testCollection.AddNew();
			CusEntryHeader entryHeader = testCollection[0];
		}

		public void TestAllowNew()
		{
			AssertEquals(false, testCollection.AllowNew);
		}

		public void TestAllowRemove()
		{
			AssertEquals(false, ((IBindingList)testCollection).AllowNew);
		}

		public void TestAddNewSpecificType()
		{
			BusinessObject bO = testCollection.AddNew();
			Assert("BO is typeof CusEntryHeader", bO is CusEntryHeader);
		}

		public void TestRemoveRelationToInvoices()
		{
			CusEntryHeader newElement = testCollection.AddNew();
			AssertEquals("CH_JE is set", jobDec.PK, newElement.CH_JE);

			testCollection.RemoveRelationshipsToInvoices();
			AssertEquals("CH_JE is cleared", ZGuid.Empty, newElement.CH_JE);
		}

		public void TestAreAllStatusesEqualTo()
		{
			testCollection.AddNew();
			testCollection.AddNew();
			testCollection[0].CH_Status = "XXX";
			testCollection[1].CH_Status = "YYY";
			Assert(!testCollection.AreAllStatusesEqualTo("XXX"));
			testCollection[1].CH_Status = "XXX";
			Assert(testCollection.AreAllStatusesEqualTo("XXX"));
		}

		public void TestAreAnyStatusesEqualTo()
		{
			testCollection.AddNew();
			testCollection.AddNew();
			testCollection[0].CH_Status = "XXX";
			testCollection[1].CH_Status = "YYY";
			Assert(testCollection.AreAnyStatusesEqualTo("XXX"));
			Assert(!testCollection.AreAnyStatusesEqualTo("ZZZ"));
		}

		public void TestAreAnyHeadersWaitingForAResponse()
		{
			testCollection.AddNew();
			testCollection.AddNew();
			Assert(!testCollection.AreAnyHeadersWaitingForAResponse);

			var message = Factory.New<EDIMessage>();
			testCollection[0].Messages.Add(message);
			testCollection[0].Messages[0].EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			Assert(testCollection.AreAnyHeadersWaitingForAResponse);
			testCollection[0].Messages[0].EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Assert(!testCollection.AreAnyHeadersWaitingForAResponse);
		}

		#region Implementation

		protected BaseJobDeclaration jobDec;
		CusEntryHeaderCollection<CusEntryHeader> testCollection;

		protected override void SetUp()
		{
			base.SetUp();
			jobDec = BaseJobDeclaration.New(Factory);
			testCollection = new CusEntryHeaderCollection<CusEntryHeader>(jobDec, Factory);
			AssertNotNull("Collection constructed", testCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusEntryHeaderCollection<CusEntryHeader>(jobDec, Factory);
		}

		#endregion
	}
}
