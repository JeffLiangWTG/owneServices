using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.GUI.Testing
{
	[TestedType(typeof(DisclaimApplicablePGAsApplicator))]
	public class DisclaimApplicablePGAsApplicatorTest : OperationalActionMethodApplicatorTest
	{
		[TestDate(2020, 4, 16)]
		public void TestDisclaimAllPGAs()
		{
			const string expectedLog = @"WARNING: House Bill house bill 1: For tariff (12345678), E is not a valid disclaim reason code of FWS.
WARNING: House Bill house bill 4: For tariff (12345675), APHIS is flagged as 'Must Be Declared', disclaim reason code is generally not allowed.";

			var views = Clearance.CusUSLVConsignments.Select(x => Factory.Load<USConsignmentCombined>(x.PK)).ToArray();
			ApplyApplicator(views, expectedLog);

			AssertEquals(1, Clearance.CusUSLVConsignments[0].CusUSLVItems[0].CusUSLVItemPGAs.Count);
			AssertEquals(1, Clearance.CusUSLVConsignments[0].CusUSLVItems[0].CusUSLVItemPGAs.OfType<CusUSLVItemPGA>().Count(x => x.AgencyCode == "APHIS" && x.ULP_DisclaimReason == "B"));

			AssertEquals(1, Clearance.CusUSLVConsignments[1].CusUSLVItems[0].CusUSLVItemPGAs.Count);
			AssertEquals(1, Clearance.CusUSLVConsignments[1].CusUSLVItems[0].CusUSLVItemPGAs.OfType<CusUSLVItemPGA>().Count(x => x.AgencyCode == "APHIS" && x.ULP_DisclaimReason == "B"));

			AssertEquals(1, Clearance.CusUSLVConsignments[2].CusUSLVItems[0].CusUSLVItemPGAs.Count);
			AssertEquals(1, Clearance.CusUSLVConsignments[2].CusUSLVItems[0].CusUSLVItemPGAs.OfType<CusUSLVItemPGA>().Count(x => x.AgencyCode == "APHIS" && x.ULP_DisclaimReason == "B"));

			AssertEquals(1, Clearance.CusUSLVConsignments[3].CusUSLVItems[0].CusUSLVItemPGAs.Count);
			AssertEquals(1, Clearance.CusUSLVConsignments[3].CusUSLVItems[0].CusUSLVItemPGAs.OfType<CusUSLVItemPGA>().Count(x => x.AgencyCode == "FDA" && x.ULP_DisclaimReason == "A"));

			AssertEquals(2, Clearance.CusUSLVConsignments[4].CusUSLVItems[0].CusUSLVItemPGAs.Count);
			AssertEquals(1, Clearance.CusUSLVConsignments[4].CusUSLVItems[0].CusUSLVItemPGAs.OfType<CusUSLVItemPGA>().Count(x => x.AgencyCode == "APHIS" && x.ULP_DisclaimReason == "B"));
			AssertEquals(1, Clearance.CusUSLVConsignments[4].CusUSLVItems[0].CusUSLVItemPGAs.OfType<CusUSLVItemPGA>().Count(x => x.AgencyCode == "FDA" && x.ULP_DisclaimReason == "A"));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var applicator = new DisclaimApplicablePGAsApplicator(Factory);

			applicator.Build(Clearance.CusUSLVConsignments.OfType<CusUSLVConsignment>().Select(x => x.PK).ToArray());
			var aphisOption = applicator.DisclaimOptions.OfType<CusUSLVItemPGADisclaimOption>().FirstOrDefault(x => x.AgencyCode == "APHIS");
			aphisOption.DisclaimReason = "B";
			var fdaOption = applicator.DisclaimOptions.OfType<CusUSLVItemPGADisclaimOption>().FirstOrDefault(x => x.AgencyCode == "FDA");
			fdaOption.DisclaimReason = "A";
			var fwsOption = applicator.DisclaimOptions.OfType<CusUSLVItemPGADisclaimOption>().FirstOrDefault(x => x.AgencyCode == "FWS");
			fwsOption.DisclaimReason = "E";
			return applicator;
		}

		CusUSLVClearance Clearance => clearance ?? (clearance = CreateTestData());
		CusUSLVClearance clearance;

		CusUSLVClearance CreateTestData()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();

			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.ULB_HouseBill = "house bill 1";
			var item1 = consignment1.CusUSLVItems.AddNew();
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "12345678";
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.Today;
			tariff1.UE_PGACodes = "AQ1FW3";
			item1.ULI_Tariff = "12345678";

			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment2.ULB_HouseBill = "house bill 2";
			var item2 = consignment2.CusUSLVItems.AddNew();
			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "12345677";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.Today;
			tariff2.UE_PGACodes = "AQ1";
			item2.ULI_Tariff = "12345677";

			var consignment3 = clearance.CusUSLVConsignments.AddNew();
			consignment3.ULB_HouseBill = "house bill 3";
			var item3 = consignment3.CusUSLVItems.AddNew();
			var tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "12345676";
			tariff3.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff3.UE_DateTo = ZDateTime.Today;
			tariff3.UE_PGACodes = "AQ1";
			item3.ULI_Tariff = "12345676";

			var consignment4 = clearance.CusUSLVConsignments.AddNew();
			consignment4.ULB_HouseBill = "house bill 4";
			var item4 = consignment4.CusUSLVItems.AddNew();
			var tariff4 = Factory.New<USCTariff>();
			tariff4.UE_Tariff = "12345675";
			tariff4.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff4.UE_DateTo = ZDateTime.Today;
			tariff4.UE_PGACodes = "AQ2FD1";
			item4.ULI_Tariff = "12345675";

			var consignment5 = clearance.CusUSLVConsignments.AddNew();
			consignment5.ULB_HouseBill = "house bill 5";
			var item5 = consignment5.CusUSLVItems.AddNew();
			var tariff5 = Factory.New<USCTariff>();
			tariff5.UE_Tariff = "12345674";
			tariff5.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff5.UE_DateTo = ZDateTime.Today;
			tariff5.UE_PGACodes = "AQ1FD1";
			item5.ULI_Tariff = "12345674";

			Factory.Save();

			return clearance;
		}

		#endregion
	}
}
