using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(DebtorToSelectFromForPrintingCollection))]
	sealed class DebtorToSelectFromForPrintingCollectionBOCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DebtorToSelectFromForPrintingCollection>
	{
		protected override DebtorToSelectFromForPrintingCollection GetCollectionToTest()
		{
			OrgHeaderCollection orgs = new OrgHeaderCollection(Factory);

			return new DebtorToSelectFromForPrintingCollection(orgs);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			OrgHeader header = Factory.New<OrgHeader>();

			return new DebtorToSelectFromForPrinting(header);
		}

		public void TestAllDebtorsWrappedInCollection()
		{
			OrgHeaderCollection orgs = new OrgHeaderCollection(Factory);
			OrgHeader header1 = orgs.AddNew();
			OrgHeader header2 = orgs.AddNew();
			OrgHeader header3 = orgs.AddNew();

			Hashtable hashOrgs = new Hashtable();
			hashOrgs.Add(header1.PK, header1);
			hashOrgs.Add(header2.PK, header2);
			hashOrgs.Add(header3.PK, header3);

			DebtorToSelectFromForPrintingCollection collection = new DebtorToSelectFromForPrintingCollection(orgs);

			AssertEquals("Collection should have a count of 3", 3, collection.Count);
			AssertEquals("Collection contains Header1", true, hashOrgs.ContainsKey(collection[0].Debtor.PK));
			AssertEquals("Collection contains Header2", true, hashOrgs.ContainsKey(collection[1].Debtor.PK));
			AssertEquals("Collection contains Header3", true, hashOrgs.ContainsKey(collection[2].Debtor.PK));
		}
	}
}
