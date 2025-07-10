using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Testing;
using Moq;

namespace Enterprise.Rating.Rateable.Test
{
	public class RatingAdapterForTest : RatingAdapter<DummyBusinessObject>
	{
		public RatingAdapterForTest(DummyBusinessObject parent, IJobInvoicingSupporter invoicingSupporter = null) : base(parent)
		{
			InvoicingSupporter = invoicingSupporter;
		}

		public override IJobInvoicingSupporter InvoicingSupporter { get; }
	}

	public class RatingAdapterTest : RatingTestCase
	{
		public void TestDefaultContractNumberConfiguration()
		{
			var rating = new RatingAdapterForTest(Factory.New<DummyBusinessObject>());

			var configuration = rating.GetContractNumberConfiguration(CostSell.Cost);
			AssertEquals(configuration.ShouldAddContractNumberQueryFilter, true);
			AssertEquals(configuration.ShouldApplySpecificAdapterContractNumberFilter, false);
			AssertEquals(configuration.ShouldIgnoreJobCarrierContractNumbers, false);
			AssertEquals(configuration.ShouldIgnoreJobClientContractNumbers, false);
			AssertEquals(configuration.ShouldMatchJobBlankContractNumber, false);
			AssertEquals(configuration.ShouldUseCarrierContractDateFilter, false);

			configuration = rating.GetContractNumberConfiguration(CostSell.Revenue);
			AssertEquals(configuration.ShouldAddContractNumberQueryFilter, true);
			AssertEquals(configuration.ShouldApplySpecificAdapterContractNumberFilter, false);
			AssertEquals(configuration.ShouldIgnoreJobCarrierContractNumbers, false);
			AssertEquals(configuration.ShouldIgnoreJobClientContractNumbers, false);
			AssertEquals(configuration.ShouldMatchJobBlankContractNumber, false);
			AssertEquals(configuration.ShouldUseCarrierContractDateFilter, false);

			Assert(true);
		}

		public void TestGetExistingCharges()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var dummy2 = Factory.New<DummyBusinessObject>();

			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();

			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_Code = "CCC1";
			chargeCode1.AC_GC = company1.PK;

			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_Code = "CCC2";
			chargeCode2.AC_GC = company1.PK;

			var chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode3.AC_Code = "CCC3";
			chargeCode3.AC_GC = company2.PK;

			var chargeCode4 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode4.AC_Code = "CCC4";
			chargeCode4.AC_GC = GlbCompany.CurrentCompany.PK;

			var chargeCode5 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode5.AC_Code = "CCC5";
			chargeCode5.AC_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			// Company 1
			var job1 = (Job)Factory.NewJobForTesting<JobHeader>();
			job1.JH_GB = GlbBranch.CurrentBranch.PK;
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job1.JH_GC = company1.PK;
			job1.JH_ParentID = dummy.PK;

			var charge1 = job1.Charges.AddNew();
			charge1.JR_GC = company1.PK;
			charge1.JR_AC = chargeCode1.PK;
			var charge2 = job1.Charges.AddNew();
			charge2.JR_GC = company1.PK;
			charge2.JR_AC = chargeCode2.PK;

			// Company 2
			var job2 = (Job)Factory.NewJobForTesting<JobHeader>();
			job2.JH_GB = GlbBranch.CurrentBranch.PK;
			job2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job2.JH_GC = company2.PK;
			job2.JH_ParentID = dummy.PK;

			var charge3 = job2.Charges.AddNew();
			charge3.JR_GC = company2.PK;
			charge3.JR_AC = chargeCode3.PK;

			// Current Company (Dummy)
			var job3 = (Job)Factory.NewJobForTesting<JobHeader>();
			job3.JH_GB = GlbBranch.CurrentBranch.PK;
			job3.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job3.JH_GC = GlbCompany.CurrentCompany.PK;
			job3.JH_ParentID = dummy.PK;

			var charge4 = job3.Charges.AddNew();
			charge4.JR_GC = GlbCompany.CurrentCompany.PK;
			charge4.JR_AC = chargeCode4.PK;

			// Current Company (Dummy 2)
			var job4 = (Job)Factory.NewJobForTesting<JobHeader>();
			job4.JH_GB = GlbBranch.CurrentBranch.PK;
			job4.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job4.JH_GC = GlbCompany.CurrentCompany.PK;
			job4.JH_ParentID = dummy2.PK;

			// A cost for another business object. Just to make sure it doesn't load costs belonging to other business objects.
			var charge5 = job4.Charges.AddNew();
			charge5.JR_GC = GlbCompany.CurrentCompany.PK;
			charge5.JR_AC = chargeCode5.PK;

			var invoicingSupporter = new Mock<IJobInvoicingSupporter>();
			invoicingSupporter.Setup(s => s.Job).Returns(job3);

			var adapter = new RatingAdapterForTest(dummy, invoicingSupporter.Object);

			var charges = adapter.GetExistingCharges();
			var actualCharges = charges.Select(c => (string)c.ChargeCode.AC_Code);
			var expectedCharges = new[] { "CCC4" };
			AssertContainsExactElementsInAnyOrder("Only charges from the current company must be returned if fromAllCompanies flag is false", actualCharges, expectedCharges);

			charges = adapter.GetExistingCharges(fromAllCompanies: true);
			actualCharges = charges.Select(c => (string)c.ChargeCode.AC_Code);
			expectedCharges = new[] { "CCC1", "CCC2", "CCC3", "CCC4" };
			AssertContainsExactElementsInAnyOrder("Charges from the companies must be returned if fromAllCompanies flag is true", actualCharges, expectedCharges);
			Assert(true);
		}
	}
}
