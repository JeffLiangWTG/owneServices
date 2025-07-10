using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobChargeTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForNew()
		{
			Type type = new JobChargeTypeDecider().GetTypeForNew();
			AssertEquals("ICharge type", ObjectFactory.GetType<ICharge>(), type);
			AssertEquals("Enterprise.Accounting.Business.JobInvoicing.Charge", type.FullName);
		}

		public void TestGetTypeForLoad()
		{
			var typeDecider = new JobChargeTypeDecider();
			var jobcharge = Factory.NewWithValidTestData<JobCharge>();
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			Type type = factory2.Load<JobCharge>(jobcharge.PK).GetType();
			AssertEquals("ICharge type", ObjectFactory.GetType<ICharge>(), type);
			AssertEquals("Enterprise.Accounting.Business.JobInvoicing.Charge", type.FullName);
		}

		public void TestGetTypeForBinding()
		{
			Type type = new JobChargeTypeDecider().GetTypeForBinding();
			AssertEquals("ICharge type", ObjectFactory.GetType<ICharge>(), type);
			AssertEquals("Enterprise.Accounting.Business.JobInvoicing.Charge", type.FullName);
		}
	}
}
