using CargoWise.EntityFramework;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Freight;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.CarbonEmissions.Business.Testing
{
	[TestedType(typeof(JobCO2e))]
	sealed class JobCO2eTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSupportClone()
		{
			var jobCO2e = CreateJobCO2eBO(Factory);
			Assert(jobCO2e.SupportsClone());
		}

		public void TestRefreshParentBindingOnUpdatedByDataRefresh()
		{
			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as ICommonShipment;
			var jobCO2e = Factory.New<JobCO2e>();
			jobCO2e.JCO_ParentID = shipment.PK;
			jobCO2e.JCO_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobCO2e.JCO_TotalCO2e = 10m;
			Factory.Save();

			Assert("Pre-condition", jobCO2e.IsInDatabase);
			AssertEquals("Pre-condition", 10m, ((ICO2eProvider)shipment).GetTotalCO2e());

			var newFactory = new BusinessObjectFactory();
			var jobCO2eInNewFactory = newFactory.Load<JobCO2e>(jobCO2e.PK);
			jobCO2eInNewFactory.JCO_TotalCO2e = 20m;
			newFactory.Save();
			AssertEquals(20m, ((ICO2eProvider)shipment).GetTotalCO2e());
		}

		#region Event Handlers

		public void TestStatusChanged()
		{
			var jobCO2e = CreateJobCO2eBO(Factory);
			var statusChangedCalled = false;
			((IJobCO2e)jobCO2e).StatusChanged += delegate
			{ statusChangedCalled = true; };

			jobCO2e.JCO_Status = CO2eStatusList.Codes.Current;
			Assert(statusChangedCalled);
		}

		public void TestJobCO2eOnSaving()
		{
			var jobCO2e = CreateJobCO2eBO(Factory);
			var onSavingCalled = false;
			((IJobCO2e)jobCO2e).JobCO2eOnSaving += delegate
			{ onSavingCalled = true; };

			jobCO2e.JCO_Status = CO2eStatusList.Codes.Current;
			Assert(!onSavingCalled);
			Factory.Save();
			Assert(onSavingCalled);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateJobCO2eBO(factory);

		protected override BusinessObject GetNewBusinessObject() => CreateJobCO2eBO(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => CreateJobCO2eBO(Factory);

		JobCO2e CreateJobCO2eBO(BusinessObjectFactory factory, string status = CO2eStatusList.Codes.Pending)
		{
			var shipment = factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as ICommonShipment;
			var jobCO2e = factory.New<JobCO2e>();
			jobCO2e.JCO_ParentID = shipment.PK;
			jobCO2e.JCO_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobCO2e.JCO_Status = status;
			return jobCO2e;
		}

		#endregion
	}
}
