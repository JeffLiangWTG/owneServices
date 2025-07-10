using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Freight;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.CarbonEmissions.Business.Testing
{
	[TestedType(typeof(JobCO2eCollection))]
	sealed class JobCO2eCollectionTest : ActiveBusinessObjectCollectionTestCase<JobCO2eCollection>
	{
		protected override JobCO2eCollection GetCollectionToTest()
		{
			var shipment = Factory.New<IForwardingShipment>() as ICommonShipment;
			var jobCO2e = Factory.New<JobCO2e>();
			jobCO2e.JCO_ParentID = shipment.PK;
			jobCO2e.JCO_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobCO2e.JCO_Status = CO2eStatusList.Codes.Current;
			(shipment as BusinessObject).Factory.Save();
			return new JobCO2eCollection(shipment as ICO2eParent);
		}
	}
}
