using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.AWB.Testing
{
	public class NewAWBFormTest : BaseFreightTest
	{
		#region TestSaveConflict

		public void TestSaveConflict()
		{
			RefAirline airline = Factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, "081"));

			var mawb1 = Factory.New<JobMawb>();
			mawb1.JM_GC_Company = GlbCompany.CurrentCompany.PK;
			mawb1.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb1.JM_Airline3DigitPrefix = airline.RM_EagleAddedAirlinePrefixOrAccountingCode;
			mawb1.JM_MAWB = "11111181";
			Factory.Save();

			var mawb2 = Factory.New<JobMawb>();
			mawb2.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb2.JM_GC_Company = GlbCompany.CurrentCompany.PK;
			mawb2.JM_Airline3DigitPrefix = airline.RM_EagleAddedAirlinePrefixOrAccountingCode;
			mawb2.JM_MAWB = "11111181";

			mawb2.RunPreSaveValidation();
			AssertNoErrors("precondition:", mawb2);

			using (NewAWBForm form = new NewAWBForm(mawb2))
			{
				form.Show();

				form.OkButton.PerformClick();
				AssertEquals("mawb2 should not be saved.", false, mawb2.IsInDatabase);
				AssertEquals("form should not have been closed", true, form.Visible);
				AssertEquals("dialog result should be cancel", DialogResult.None, form.DialogResult);

				mawb2.JM_MAWB = "11111251";
				AssertNoErrors("precondition: ", mawb2);

				form.OkButton.PerformClick();
				AssertEquals("mawb2 should be saved.", true, mawb2.IsInDatabase);
				AssertEquals("form should have been closed", false, form.Visible);
				AssertEquals("dialog result should be ok", DialogResult.OK, form.DialogResult);
			}
		}

		[RequiresSTA]
		public void TestDuplicatesExist()
		{
			var mawb = Factory.New<JobMawb>();

			using (NewAWBForm form = new NewAWBForm(mawb))
			{
				form.Show();

				mawb.JM_GC_Company = GlbCompany.CurrentCompany.PK;
				mawb.JM_GB = GlbBranch.CurrentBranch.PK;
				mawb.JM_Airline3DigitPrefix = "777";
				mawb.JM_MAWB = "00000011";

				mawb.RunPreSaveValidation();
				AssertNoErrors("Pre-condition", mawb);

				form.OkButton.PerformClick();
				AssertEquals("mawb should be saved.", true, mawb.IsInDatabase);
				AssertEquals("form should have been closed", false, form.Visible);
				AssertEquals("dialog result should be ok", DialogResult.OK, form.DialogResult);
			}

			var mawb2 = Factory.New<JobMawb>();

			using (NewAWBForm form = new NewAWBForm(mawb2))
			{
				form.Show();

				mawb2.JM_GC_Company = GlbCompany.CurrentCompany.PK;
				mawb2.JM_GB = GlbBranch.CurrentBranch.PK;
				mawb2.JM_Airline3DigitPrefix = "777";
				mawb2.JM_MAWB = "00000011";

				mawb.RunPreSaveValidation();
				AssertNoErrors("Pre-condition", mawb2);

				form.OkButton.PerformClick();
				AssertEquals("mawb2 should not be saved.", false, mawb2.IsInDatabase);
				AssertEquals("form should not have been closed", true, form.Visible);
				AssertEquals("dialog result should be None", DialogResult.None, form.DialogResult);
			}
		}

		public void TestDuplicatesExistInAnotherConsol()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_UniqueConsignRef = "c000001";
			consol1.JK_TransportMode = Constants.TransportModes.Air;
			consol1.JK_AWBServiceLevel = OrgCarrierServiceLevel.StandardCode;
			consol1.JK_MasterBillNum = "08111111225";
			consol1.JK_RL_NKLoadPort = "AUBNE";
			Factory.Save();

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_UniqueConsignRef = "c000002";
			consol2.JK_TransportMode = Constants.TransportModes.Air;
			consol2.JK_AWBServiceLevel = OrgCarrierServiceLevel.StandardCode;
			consol2.JK_RL_NKLoadPort = "AUBNE";

			var mawb = Factory.New<JobMawb>();

			using (NewAWBForm form = new NewAWBForm(mawb, consol2))
			{
				form.Show();

				mawb.JM_GC_Company = GlbCompany.CurrentCompany.PK;
				mawb.JM_GB = GlbBranch.CurrentBranch.PK;
				mawb.JM_Airline3DigitPrefix = "081";
				mawb.JM_MAWB = "11111225";

				mawb.RunPreSaveValidation();
				AssertNoErrors("Pre-condition", mawb);

				form.OkButton.PerformClick();
				AssertEquals("mawb should not be saved.", false, mawb.IsInDatabase);
				AssertEquals("form should not have been closed", true, form.Visible);
				AssertEquals("dialog result should be none", DialogResult.None, form.DialogResult);
			}
		}

		#endregion

		[RequiresSTA]
		public void TestCreateNewMAWB_AllowUseBranchStock()
		{
			AddMawbStockManagement("081", GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, false, false, true, "N");
			var consol = CreateConsol();
			var mawb = Factory.New<JobMawb>();
			using (var form = new NewAWBForm(mawb, consol))
			{
				form.Show();
				mawb.JM_Airline3DigitPrefix = "081";
				mawb.JM_MAWB = "11111225";

				// Global level stock - should not be saved
				mawb.JM_GC_Company = ZGuid.Empty;
				mawb.JM_GB = ZGuid.Empty;
				form.OkButton.PerformClick();
				AssertSaved(mawb, form, false);

				// Company level stock - should not be saved
				mawb.JM_GC_Company = GlbCompany.CurrentCompany.PK;
				mawb.JM_GB = ZGuid.Empty;
				form.OkButton.PerformClick();
				AssertSaved(mawb, form, false);

				// Branch level stock - should be saved
				mawb.JM_GC_Company = GlbCompany.CurrentCompany.PK;
				mawb.JM_GB = GlbBranch.CurrentBranch.PK;
				form.OkButton.PerformClick();
				AssertSaved(mawb, form, true);
			}
		}

		[RequiresSTA]
		public void TestCreateNewMAWB_AllowUseCompanyStock()
		{
			AddMawbStockManagement("081", GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, false, true, false, "N");
			var consol = CreateConsol();
			var mawb = Factory.New<JobMawb>();
			using (var form = new NewAWBForm(mawb, consol))
			{
				form.Show();
				mawb.JM_Airline3DigitPrefix = "081";
				mawb.JM_MAWB = "11111225";

				// Branch level stock - should not be saved
				mawb.JM_GC_Company = GlbCompany.CurrentCompany.PK;
				mawb.JM_GB = GlbBranch.CurrentBranch.PK;
				form.OkButton.PerformClick();
				AssertSaved(mawb, form, false);

				// Global level stock - should not be saved
				using (mawb.GetValidationSuspender())
				{
					mawb.JM_GC_Company = ZGuid.Empty;
					mawb.JM_GB = ZGuid.Empty;
				}
				form.OkButton.PerformClick();
				AssertSaved(mawb, form, false);

				// Company level stock - should be saved
				mawb.JM_GC_Company = GlbCompany.CurrentCompany.PK;
				mawb.JM_GB = ZGuid.Empty;
				form.OkButton.PerformClick();
				AssertSaved(mawb, form, true);
			}
		}

		[RequiresSTA]
		public void TestCreateNewMAWB_AllowUseGlobalStock()
		{
			AddMawbStockManagement("081", GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, true, false, false, "N");
			var consol = CreateConsol();
			var mawb = Factory.New<JobMawb>();
			using (var form = new NewAWBForm(mawb, consol))
			{
				form.Show();
				mawb.JM_Airline3DigitPrefix = "081";
				mawb.JM_MAWB = "11111225";

				// Company level stock - should not be saved
				mawb.JM_GC_Company = GlbCompany.CurrentCompany.PK;
				mawb.JM_GB = ZGuid.Empty;
				form.OkButton.PerformClick();
				AssertSaved(mawb, form, false);

				// Branch level stock - should not be saved
				mawb.JM_GC_Company = GlbCompany.CurrentCompany.PK;
				mawb.JM_GB = GlbBranch.CurrentBranch.PK;
				form.OkButton.PerformClick();
				AssertSaved(mawb, form, false);

				// Global level stock - should be saved
				// mawb.SuspendValidation();
				mawb.JM_GB = ZGuid.Empty;
				mawb.JM_GC_Company = ZGuid.Empty;
				//mawb.ResumeValidation();
				form.OkButton.PerformClick();
				AssertSaved(mawb, form, true);
			}
		}

		static void AssertSaved(JobMawb mawb, NewAWBForm form, bool shouldSaved)
		{
			AssertEquals($"mawb should be saved - {shouldSaved}", shouldSaved, mawb.IsInDatabase);
			AssertEquals($"form should have been closed - {shouldSaved}", !shouldSaved, form.Visible);
			AssertEquals($"dialog result should be ok - {shouldSaved}", shouldSaved ? DialogResult.OK : DialogResult.None, form.DialogResult);
		}

		ForwardingConsol CreateConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "c000001";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AWBServiceLevel = OrgCarrierServiceLevel.StandardCode;
			consol.JK_MasterBillNum = "08111111225";
			consol.JK_RL_NKLoadPort = "AUBNE";
			Factory.Save();
			return consol;
		}

		void AddMawbStockManagement(string airLinePrefix, ZGuid companyId, ZGuid branchId, bool canUseGlobalStock, bool canUseCompanyStock, bool canUseBranchStock, string useOtherBranchStock = "R")
		{
			var carrier = OrgHeader.FindBy3CharAirlineCode(Factory, airLinePrefix);
			carrier ??= Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsAirLine = true;
			carrier.MiscServ.OM_RM_Airline = RefAirline.LoadFromAirlinePrefix(Factory, airLinePrefix).PK;

			carrier.OrgAirlineMAWBStockManagementCollection.DeleteAll();
			var mawbStockMangement = carrier.OrgAirlineMAWBStockManagementCollection.AddNew();
			mawbStockMangement.OHM_GC_Company = companyId;
			mawbStockMangement.OHM_GB_Branch = branchId;
			mawbStockMangement.OHM_AllowUseGlobalStock = canUseGlobalStock;
			mawbStockMangement.OHM_AllowUseCompanyStock = canUseCompanyStock;
			mawbStockMangement.OHM_AllowUseBranchStock = canUseBranchStock;
			mawbStockMangement.OHM_AllowUseOtherBranchStock = useOtherBranchStock;

			if (canUseGlobalStock)
			{
				mawbStockMangement = carrier.OrgAirlineMAWBStockManagementCollection.AddNew();
				mawbStockMangement.OHM_AllowUseGlobalStock = true;
			}

			if (canUseCompanyStock)
			{
				mawbStockMangement = carrier.OrgAirlineMAWBStockManagementCollection.AddNew();
				mawbStockMangement.OHM_AllowUseCompanyStock = true;
				mawbStockMangement.OHM_GC_Company = companyId;
			}

			Factory.Save();
		}
	}
}
