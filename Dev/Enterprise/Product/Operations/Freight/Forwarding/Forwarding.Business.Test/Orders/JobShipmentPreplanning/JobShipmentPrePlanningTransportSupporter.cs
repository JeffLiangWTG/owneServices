using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Security;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class JobShipmentPrePlanningTransportSupporter : TransportSupporterTestCase<JobShipmentPreplanningTransportSupporter>
	{
		[RunInExtraTransaction]
		public void TestConsignmentRef()
		{
			ITransportParent parent = Factory.New<JobShipmentPreplanning>();
			TransportSupporter supporter = parent.TransportSupporter;
			AssertEquals("", supporter.ConsignmentRef);

			supporter.SetConsignmentRefIfNotSet();
			Assert("ConsignmentRef should now be populated from the number fountain", Regex.IsMatch(supporter.ConsignmentRef, "PA[0-9]{8}"));
		}

		public void TestGetNewTransportValidator()
		{
			ITransportParent parent = Factory.New<JobShipmentPreplanning>();
			TransportSupporter supporter = parent.TransportSupporter;
			AssertEquals("GetNewTransportValidator", typeof(PreAdviceTransportValidation), supporter.GetNewTransportValidator(parent.Transports.AddNew()).GetType());
		}

		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceOrders; }
		}

		protected override TransportSupporter GetNewTransportSupporter()
		{
			ITransportParent parent = Factory.New<JobShipmentPreplanning>();
			return parent.TransportSupporter;
		}

		protected override ZString TestingCountry
		{
			get { return Core.Constants.CountryCodes.Australia; }
		}
	}
}
