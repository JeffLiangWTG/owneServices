using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.RatingTests.Testing.GUI
{
	public class InvoicingTest : TestCaseWithFactory
	{
		public void TestInvoiceTypeDefaultingInBillOfLading()
		{
			var frtQuery = new ZQuery(AccChargeCodeSchema.AC_Code, "FRT");
			frtQuery.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			var frtChargeCode = Factory.LoadTop1<AccChargeCode>(frtQuery);

			var bill = Factory.NewWithValidTestData<BillOfLading>();
			bill.JS_TransportMode = Constants.TransportModes.Sea;
			bill.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;
			bill.JS_INCO = Constants.DomesticPaymentTerms.Collect;
			bill.JS_RL_NKOrigin = "AUBNE";
			bill.JS_RL_NKDestination = "USLAX";

			Job testJob = CreateJob(bill, bill.JS_UniqueConsignRef);
			testJob.PlugInData = bill;
			testJob.LocalChargesPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			testJob.AgentCollectPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			var charge = testJob.Charges.AddNew();
			charge.JR_AC = frtChargeCode.PK;
			AssertEquals("LCO", charge.JR_InvoiceType);
		}

		Job CreateJob(BusinessObject bizO, ZString jobNumber)
		{
			var result = Factory.NewJobForTesting<Job>();
			result.SuspendValidation();
			result.JH_JobNum = jobNumber;
			result.JH_ParentID = bizO.PK;
			result.JH_ParentTableCode = bizO.TablePrefix;
			result.JH_GB = GlbBranch.CurrentBranch.PK;
			result.JH_GE = GlbDepartment.CurrentDepartment.PK;
			return result;
		}
	}
}
