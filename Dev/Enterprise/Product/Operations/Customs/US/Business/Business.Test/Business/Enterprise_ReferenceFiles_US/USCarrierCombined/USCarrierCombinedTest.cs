using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCarrierCombined))]
	sealed class USCarrierCombinedTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "ABD";

			AssertEquals("Carrier ABD", carrier.HumanReadableName);
		}

		public void TestAutoLoggingIsEnabled()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "ABD";
			AssertNull(carrier.Logs.MostRecentLog);
			Factory.Save();
			AssertNotNull(carrier.Logs.MostRecentLogByEventTime(Events.AddedARecordToTheSystem));
		}

		public void TestSetDefault()
		{
			var carrier = Factory.New<USCarrierCombined>();
			AssertEquals("carrier.USC_ModeOfTransportation", TransportModeCodes.Codes.VesselNonContainer, carrier.UI_ModeOfTransportation);
		}

		public void TestClearAirWaybillPrefixIfNotRequired()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_ModeOfTransportation = ZString.Empty;
			carrier.UI_AirwayBillPrefix = "!";
			carrier.UI_ModeOfTransportation = TransportModeCodes.Codes.VesselNonContainer;
			AssertEquals(ZString.Empty, carrier.UI_AirwayBillPrefix);

			carrier.UI_AirwayBillPrefix = "ABC";
			carrier.UI_ModeOfTransportation = TransportModeCodes.Codes.AirNonContainer;
			AssertEquals("ABC", carrier.UI_AirwayBillPrefix);

			carrier.UI_ModeOfTransportation = TransportModeCodes.Codes.AirNonContainer;
			AssertEquals("Should not change", "ABC", carrier.UI_AirwayBillPrefix);
			Assert(!carrier.IsTruck);

			carrier.UI_ModeOfTransportation = TransportModeCodes.Codes.TruckNonContainer;
			Assert(carrier.IsTruck);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var carrier = factory.New<USCarrierCombined>();
			carrier.UI_Code = "#@!@";
			return carrier;
		}
	}
}
