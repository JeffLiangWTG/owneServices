using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(VoyageAccount))]
	internal sealed partial class VoyageAccountTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDescription()
		{
			AssertEquals("/ ()", Account1.NA_Calc_Description);

			OrgHeader principal = Factory.New<OrgHeader>();
			principal.OH_Code = "PRINCIPAL";

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "VESSEL";

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "VOYAGE";

			Account1.NA_JV = voyage.PK;
			Account1.NA_OH = principal.PK;

			AssertEquals("VESSEL/VOYAGE (PRINCIPAL)", Account1.NA_Calc_Description);
		}

		public void TestSetDefaultValues()
		{
			VoyageAccount account = Factory.New<VoyageAccount>();
			AssertEquals("NA_GC", GlbCompany.CurrentCompany.PK, account.NA_GC);
		}

		public void TestCalcVesselVoyage()
		{
			var vessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First();

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "0001";
			voyage.JV_RV_NKVessel = vessel.RV_FK;

			Account1.NA_JV = ZGuid.Empty;
			AssertEquals("JV_Calc_Vessel", "", Account1.NA_Calc_Vessel);
			AssertEquals("JV_Calc_Voyage", "", Account1.NA_Calc_Voyage);

			Account1.NA_JV = voyage.PK;
			AssertEquals("JV_Calc_Vessel", "MAJAPAHIT", Account1.NA_Calc_Vessel);
			AssertEquals("JV_Calc_Voyage", "0001", Account1.NA_Calc_Voyage);
		}

		public void TestPrincipal()
		{
			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			VoyageAccount voyageAccount = Factory.NewWithValidTestData<VoyageAccount>();
			principal.OH_FullName = "Principal";

			voyageAccount.NA_OH = principal.PK;

			AssertEquals("Principal", voyageAccount.Principal.OH_FullName);
		}

		public void TestJobNumberSetOnSaving()
		{
			var vessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First();

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "1234";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";

			Account1.NA_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			Account1.NA_JV = voyage.PK;

			long next = Env.NumberFountains.VoyageAccountingNumber(GlbCompany.CurrentCompany.PK.ToGuid()).PeekPreliminary(Factory);
			AssertEquals("NA_JobNumber should not be set until saving", "", Account1.NA_JobNumber);

			Factory.Save();
			AssertEquals("NA_JobNumber should have been set on saving", string.Format("VA{0:00000000}", next), Account1.NA_JobNumber);
		}

		public void TestIsDistinct()
		{
			JobVoyage voyage1 = Factory.New<JobVoyage>();
			JobVoyage voyage2 = Factory.New<JobVoyage>();

			OrgHeader principal1 = Factory.New<OrgHeader>();
			OrgHeader principal2 = Factory.New<OrgHeader>();

			Account1.NA_JV = voyage1.PK;
			Account1.NA_OH = principal1.PK;

			Account2.NA_JV = voyage1.PK;
			Account2.NA_OH = principal2.PK;

			Account3.NA_GC = ZGuid.NewZGuid();
			Account3.NA_JV = voyage1.PK;
			Account3.NA_OH = principal1.PK;

			AssertEquals(true, Account1.IsDistinct);
			AssertEquals(true, Account2.IsDistinct);
			AssertEquals(true, Account3.IsDistinct);

			Account2.NA_OH = principal1.PK;
			AssertEquals(false, Account1.IsDistinct);
			AssertEquals(false, Account2.IsDistinct);
			AssertEquals(true, Account3.IsDistinct);

			Account2.NA_JV = voyage2.PK;
			AssertEquals(true, Account1.IsDistinct);
			AssertEquals(true, Account2.IsDistinct);
			AssertEquals(true, Account3.IsDistinct);
		}

		public void TestJob()
		{
			VoyageAccount voyageAccount = Factory.NewWithValidTestData<VoyageAccount>();

			AssertNull("Should NOT have a Job", voyageAccount.Job);

			JobHeader job = new JobHeader.Loader(voyageAccount).TryLoadOrCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			AssertEquals("Should have a Job", job, voyageAccount.Job);
		}

		public void TestVoyagePrincipalReadOnly()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Test Vessel";

			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			VoyageAccount voyageAccount = Factory.NewWithValidTestData<VoyageAccount>();
			voyageAccount.NA_OH = principal.PK;
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "1234";
			voyage.JV_OH_Line = principal.PK;
			voyageAccount.NA_JV = voyage.PK;

			AssertEquals("Test Vessel", voyageAccount.NA_Calc_Vessel);
			AssertEquals("Voyage should NOT be ReadOnly", false, voyageAccount.NA_JVInfo.ReadOnly);
			AssertEquals("Principal should NOT be ReadOnly", false, voyageAccount.NA_OHInfo.ReadOnly);

			JobHeader job = new JobHeader.Loader(voyageAccount).TryLoadOrCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			JobCharge charge = NewCharge(job);
			AssertEquals("Charge should NOT be CostPosted", false, charge.IsCostPosted);
			AssertEquals("Charge should NOT be RevenuePosted", false, charge.IsRevenuePosted);
			AssertEquals("Voyage should NOT be ReadOnly", false, voyageAccount.NA_JVInfo.ReadOnly);
			AssertEquals("Principal should NOT be ReadOnly", false, voyageAccount.NA_OHInfo.ReadOnly);

			charge.APLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			AssertEquals("Charge should be CostPosted", true, charge.IsCostPosted);
			AssertEquals("Voyage should be ReadOnly", true, voyageAccount.NA_JVInfo.ReadOnly);
			AssertEquals("Principal should be ReadOnly", true, voyageAccount.NA_OHInfo.ReadOnly);

			charge.APLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Accrual;
			AssertEquals("Charge should NOT be CostPosted", false, charge.IsCostPosted);
			AssertEquals("Charge should NOT be RevenuePosted", false, charge.IsRevenuePosted);
			AssertEquals("Voyage should NOT be ReadOnly", false, voyageAccount.NA_JVInfo.ReadOnly);
			AssertEquals("Principal should NOT be ReadOnly", false, voyageAccount.NA_OHInfo.ReadOnly);

			charge = NewCharge(job);
			charge.ARLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			AssertEquals("Charge should be RevenuePosted", true, charge.IsRevenuePosted);
			AssertEquals("Voyage should be ReadOnly", true, voyageAccount.NA_JVInfo.ReadOnly);
			AssertEquals("Principal should be ReadOnly", true, voyageAccount.NA_OHInfo.ReadOnly);
		}

		public void TestVoyageCanDelete()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Test Vessel";

			var principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_Code = "Test Carrier";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "1234";
			voyage.JV_OH_Line = principal.PK;

			var voyageAccount = Factory.NewWithValidTestData<VoyageAccount>();
			voyageAccount.NA_OH = principal.PK;
			voyageAccount.NA_JV = voyage.PK;

			Factory.Save();

			Assert(voyageAccount.CanDelete);
			Assert("Able to delete because no job", voyage.CanDelete);
			AssertEquals("", voyage.ReasonForNotAbleToDelete);

			var job = new JobHeader.Loader(voyageAccount).TryLoadOrCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			Factory.Save();

			Assert(!job.CanDelete);
			Assert(!voyageAccount.CanDelete);
			Assert("Unable to delete because job cannot delete", !voyage.CanDelete);
			AssertEquals("There are jobs referencing Sailing Schedule (Vessel='Test Vessel', Voyage='1234', Carrier='Test Carrier')\r\nVoyage Accounting Job VA00000001", voyage.ReasonForNotAbleToDelete);

			var invoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice.AH_JH = job.PK;
			Factory.Save();

			var reasonOfVoyage = "There are jobs referencing Sailing Schedule (Vessel='Test Vessel', Voyage='1234', Carrier='Test Carrier')\r\nVoyage Accounting Job VA00000001";
			var reasonOfVoyageAccount = "Voyage Accounting Job VA00000001: This record cannot be deleted.\r\nAn Invoicing Job Header (VA00000001) has been created in the company EDI.";
			Assert(!job.CanDelete);
			Assert("Not able to delete because job can't delete", !voyageAccount.CanDelete);
			AssertEquals(reasonOfVoyageAccount, voyageAccount.ReasonForNotAbleToDelete.ToString());
			Assert("Not able to delete because job can't delete", !voyage.CanDelete);
			AssertEquals(reasonOfVoyage, voyage.ReasonForNotAbleToDelete);

			invoice.AH_JH = ZGuid.Empty;
			Factory.Save();

			voyage.Delete();

			Assert(!job.IsDeleted);
			Assert(!voyageAccount.IsDeleted);
			Assert(voyage.IsDeleted);
		}

		public void TestPostedStateChanged()
		{
			VoyageAccount voyageAccount = Factory.NewWithValidTestData<VoyageAccount>();

			int countVoyage = 0;
			voyageAccount.NA_JVInfo.ValueChanged += new EventHandler(delegate
			{ countVoyage++; });
			int countPrincipal = 0;
			voyageAccount.NA_OHInfo.ValueChanged += new EventHandler(delegate
			{ countPrincipal++; });

			((IJobInvoicingPlugIn)voyageAccount).InvoicingSupporter.PostedStateChanged();
			AssertEquals("NA_JVInfo.ValueChanged event should be raised only once", 1, countVoyage);
			AssertEquals("NA_OHInfo.ValueChanged event should be raised only once", 1, countPrincipal);
		}

		public void TestLocalClientFromPrincipal()
		{
			VoyageAccount voyageAccount = Factory.NewWithValidTestData<VoyageAccount>();
			JobHeader job = new JobHeader.Loader(voyageAccount).TryLoadOrCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			AssertEquals("Local Client Should be Empty", true, voyageAccount.Job.JH_OA_LocalChargesAddr.IsEmpty);

			OrgHeader principal_1 = Factory.NewWithValidTestData<OrgHeader>();
			voyageAccount.NA_OH = principal_1.PK;
			AssertEquals("Local Client Should be Principal_1", principal_1.PK, voyageAccount.Job.JH_OA_LocalChargesAddr_ZAddress.OrgPK);

			OrgHeader principal_2 = Factory.NewWithValidTestData<OrgHeader>();
			voyageAccount.NA_OH = principal_2.PK;
			AssertEquals("Local Client Should be Principal_1", principal_1.PK, voyageAccount.Job.JH_OA_LocalChargesAddr_ZAddress.OrgPK);
		}

		#region Implementation

		VoyageAccount Account1
		{
			get { return account1 ?? (account1 = Factory.New<VoyageAccount>()); }
		}
		VoyageAccount account1;

		VoyageAccount Account2
		{
			get { return account2 ?? (account2 = Factory.New<VoyageAccount>()); }
		}
		VoyageAccount account2;

		VoyageAccount Account3
		{
			get { return account3 ?? (account3 = Factory.New<VoyageAccount>()); }
		}
		VoyageAccount account3;

		JobCharge NewCharge(JobHeader job)
		{
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			AccTransactionLines lines = Factory.NewWithValidTestData<AccTransactionLines>();
			charge.JR_AL_ARLine = lines.PK;
			lines = Factory.NewWithValidTestData<AccTransactionLines>();
			charge.JR_AL_APLine = lines.PK;

			return charge;
		}

		#endregion
	}
}
