using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.GUI.Testing
{
	[TestedType(typeof(DisclaimApplicablePGAsChildForm))]
	public class DisclaimApplicablePGAsChildFormTest : ZFormBasherTest
	{
		public void TestNoConsignmentApplicable()
		{
			var applicator = new DisclaimApplicablePGAsApplicator(Factory);
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			using (var form = new DisclaimApplicablePGAsChildForm(applicator, new[] { consignment }))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals(0, applicator.DisclaimOptions.Count);

				form.AcceptButton.PerformClick();
				AssertEquals("There is no applicable consignment to update.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestUpdateDisclaimReasonForAllApplicableConsignments()
		{
			var applicator = new DisclaimApplicablePGAsApplicator(Factory);
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.ULB_HouseBill = "HB11111";
			consignment1.CusUSLVItems.AddNew().ULI_Tariff = "2853000059";
			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment2.ULB_HouseBill = "HB22222";
			consignment2.CusUSLVItems.AddNew().ULI_Tariff = "8438909010";

			using (var form = new DisclaimApplicablePGAsChildForm(applicator, new[] { consignment1, consignment2 }))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals(2, applicator.DisclaimOptions.Count);
				var fdaDisclaimOption = applicator.DisclaimOptions.OfType<CusUSLVItemPGADisclaimOption>().First(d => d.AgencyCode == "FDA");
				fdaDisclaimOption.DisclaimReason = "A";
				var odsDisclaimOption = applicator.DisclaimOptions.OfType<CusUSLVItemPGADisclaimOption>().First(d => d.AgencyCode == "ODS");
				odsDisclaimOption.DisclaimReason = "B";

				form.AcceptButton.PerformClick();
				AssertEquals("System has updated PGA disclaim reason for all applicable consignments.", UnitTestUserNotification.Instance.LastMessage.Text);
				CombineAssertions("Consignment HB11111", () =>
				{
					AssertEquals("FDA disclaim reason should be updated to 'A'", "A", consignment1.FirstCusUSLVItem.ACEFDAWrapper.DisclaimReason);
					AssertEquals("ODS disclaim reason shouldn't be updated", ZString.Empty, consignment1.FirstCusUSLVItem.ODSWrapper.DisclaimReason);
				});
				CombineAssertions("Consignment HB22222", () =>
				{
					AssertEquals("FDA disclaim reason shouldn't be updated", ZString.Empty, consignment2.FirstCusUSLVItem.ACEFDAWrapper.DisclaimReason);
					AssertEquals("ODS disclaim reason should be updated to 'B'", "B", consignment2.FirstCusUSLVItem.ODSWrapper.DisclaimReason);
				});
			}
		}

		public void TestUpdateDisclaimReasonForApplicableConsignments()
		{
			var applicator = new DisclaimApplicablePGAsApplicator(Factory);
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.ULB_HouseBill = "HB11111";
			consignment1.CusUSLVItems.AddNew().ULI_Tariff = "2853000059";
			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment2.ULB_HouseBill = "HB22222";
			consignment2.CusUSLVItems.AddNew().ULI_Tariff = "8438909010";

			using (var form = new DisclaimApplicablePGAsChildForm(applicator, new[] { consignment1, consignment2 }))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals(2, applicator.DisclaimOptions.Count);
				var fdaDisclaimOption = applicator.DisclaimOptions.OfType<CusUSLVItemPGADisclaimOption>().First(d => d.AgencyCode == "FDA");
				fdaDisclaimOption.DisclaimReason = "A";
				var odsDisclaimOption = applicator.DisclaimOptions.OfType<CusUSLVItemPGADisclaimOption>().First(d => d.AgencyCode == "ODS");
				odsDisclaimOption.DisclaimReason = "F";

				form.AcceptButton.PerformClick();
				AssertEquals("System is unable to update PGA disclaim reason for following consignments:\r\n\r\nHouse Bill HB22222: For tariff (8438909010), F is not a valid disclaim reason code of ODS.", UnitTestUserNotification.Instance.LastMessage.Text);

				CombineAssertions("Consignment HB11111", () =>
				{
					AssertEquals("FDA disclaim reason should be updated to 'A'", "A", consignment1.FirstCusUSLVItem.ACEFDAWrapper.DisclaimReason);
					AssertEquals("ODS disclaim reason shouldn't be updated", ZString.Empty, consignment1.FirstCusUSLVItem.ODSWrapper.DisclaimReason);
				});
				CombineAssertions("Consignment HB22222", () =>
				{
					AssertEquals("FDA disclaim reason shouldn't be updated", ZString.Empty, consignment2.FirstCusUSLVItem.ACEFDAWrapper.DisclaimReason);
					AssertEquals("ODS disclaim reason shouldn't be updated", ZString.Empty, consignment2.FirstCusUSLVItem.ODSWrapper.DisclaimReason);
				});
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var fd1Tariff = Factory.New<USCTariff>();
			fd1Tariff.UE_Tariff = "2853000059";
			fd1Tariff.UE_PGACodes = "FD1";
			fd1Tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			fd1Tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			var ep1Tariff = Factory.New<USCTariff>();
			ep1Tariff.UE_Tariff = "8438909010";
			ep1Tariff.UE_PGACodes = "EP1";
			ep1Tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			ep1Tariff.UE_DateTo = ZDateTime.Today.AddMonths(1);
		}

		protected override Form GetFormToBashCore() => new DisclaimApplicablePGAsChildForm(new DisclaimApplicablePGAsApplicator(Factory), System.Array.Empty<CusUSLVConsignment>());

		#endregion
	}
}
