using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Security;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class CusISFHeaderTransportSupporterTest : TransportSupporterTestCase<CusISFHeaderTransportSupporter>
	{
		public void TestITransportParentMembers()
		{
			var header = Factory.New<CusISFHeader>();
			ITransportParent transportParent = header;
			var supporter = transportParent.TransportSupporter;
			header.BF_OceanBill = "OB2343";
			header.BF_HouseBill = "HB2342";
			AssertEquals("OB2343", supporter.BillOfLading);
			header.BF_OceanBill = ZString.Empty;
			AssertEquals("HB2342", supporter.BillOfLading);
			header.BF_JobReference = "BF23432323";
			AssertEquals("BF23432323", supporter.ConsignmentRef);
			AssertEquals("BF23432323", supporter.Description);
			AssertEquals(Core.Constants.TransportModes.Sea, supporter.TransportMode);
			AssertEquals(ZString.Empty, supporter.ContainerMode);
			AssertEquals("Transports", header.Transports, transportParent.Transports);
			AssertEquals(Core.Constants.TransportParentTypes.ImporterSecurityFiling, transportParent.TypeCode);
		}

		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint => Env.Security.RoadDistanceCalculationServiceCustoms;

		protected override TransportSupporter GetNewTransportSupporter()
		{
			ITransportParent parent = Factory.New<CusISFHeader>();
			return parent.TransportSupporter;
		}

		protected override ZString TestingCountry => Core.Constants.CountryCodes.UnitedStates;
	}
}
