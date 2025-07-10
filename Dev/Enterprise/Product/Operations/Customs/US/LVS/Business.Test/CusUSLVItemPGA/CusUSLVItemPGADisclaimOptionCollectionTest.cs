using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	[TestedType(typeof(CusUSLVItemPGADisclaimOptionCollection))]
	public class CusUSLVItemPGADisclaimOptionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CusUSLVItemPGADisclaimOptionCollection>
	{
		public void TestAllowNew()
		{
			var collection = GetCollectionToTest();
			Assert(!collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var collection = GetCollectionToTest();
			Assert(!collection.AllowRemove);
		}

		[TestDate(2020, 5, 21)]
		public void TestPopulate()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			var item1 = consignment1.CusUSLVItems.AddNew();
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "12345678";
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.Today;
			tariff1.UE_PGACodes = "AQ1FW3";
			item1.ULI_Tariff = "12345678";

			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			var item2 = consignment2.CusUSLVItems.AddNew();
			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "12345677";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.Today;
			tariff2.UE_PGACodes = "AQ1";
			item2.ULI_Tariff = "12345677";

			var consignment3 = clearance.CusUSLVConsignments.AddNew();
			var item3 = consignment3.CusUSLVItems.AddNew();
			var tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "12345676";
			tariff3.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff3.UE_DateTo = ZDateTime.Today;
			tariff3.UE_PGACodes = "FD2";
			item3.ULI_Tariff = "12345676";

			var collection = GetCollectionToTest();
			collection.Populate(new CusUSLVConsignment[] { consignment1, consignment2, consignment3 });
			AssertEquals("Should contain 3 options", 3, collection.Count);
			Assert(collection.OfType<CusUSLVItemPGADisclaimOption>().Any(x => x.AgencyCode == "APHIS"));
			Assert(collection.OfType<CusUSLVItemPGADisclaimOption>().Any(x => x.AgencyCode == "FDA"));
			Assert(collection.OfType<CusUSLVItemPGADisclaimOption>().Any(x => x.AgencyCode == "FWS"));
		}

		public void TestToDictionary()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			var item1 = consignment1.CusUSLVItems.AddNew();
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "12345678";
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.Today;
			tariff1.UE_PGACodes = "AQ1FW3";
			item1.ULI_Tariff = "12345678";

			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			var item2 = consignment2.CusUSLVItems.AddNew();
			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "12345677";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.Today;
			tariff2.UE_PGACodes = "AQ1";
			item2.ULI_Tariff = "12345677";

			var consignment3 = clearance.CusUSLVConsignments.AddNew();
			var item3 = consignment3.CusUSLVItems.AddNew();
			var tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "12345676";
			tariff3.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff3.UE_DateTo = ZDateTime.Today;
			tariff3.UE_PGACodes = "FD2";
			item3.ULI_Tariff = "12345676";

			var collection = GetCollectionToTest();
			collection.Populate(new CusUSLVConsignment[] { consignment1, consignment2, consignment3 });

			var fdaOption = collection.OfType<CusUSLVItemPGADisclaimOption>().FirstOrDefault(x => x.AgencyCode == "FDA");
			fdaOption.DisclaimReason = "A";

			var aphisOption = collection.OfType<CusUSLVItemPGADisclaimOption>().FirstOrDefault(x => x.AgencyCode == "APHIS");
			aphisOption.DisclaimReason = "B";

			var fwsOption = collection.OfType<CusUSLVItemPGADisclaimOption>().FirstOrDefault(x => x.AgencyCode == "FWS");
			fwsOption.DisclaimReason = "C";

			var options = collection.ToDictionary();

			AssertEquals(3, options.Count);

			Assert(options.ContainsKey("FDA"));
			AssertEquals("A", options["FDA"]);

			Assert(options.ContainsKey("APHIS"));
			AssertEquals("B", options["APHIS"]);

			Assert(options.ContainsKey("FWS"));
			AssertEquals("C", options["FWS"]);
		}

		protected override CusUSLVItemPGADisclaimOptionCollection GetCollectionToTest()
		{
			return new CusUSLVItemPGADisclaimOptionCollection(new BusinessObjectFactory());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CusUSLVItemPGADisclaimOption(new BusinessObjectFactory(), "TST");
		}
	}
}
