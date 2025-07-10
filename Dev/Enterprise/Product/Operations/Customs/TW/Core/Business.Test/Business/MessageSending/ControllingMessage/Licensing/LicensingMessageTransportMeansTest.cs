using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Customs.TW.Business.MessageConstants;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(LicensingMessageTransportMeans))]
	sealed class LicensingMessageTransportMeansTest : TestCaseWithFactory
	{
		public void TestData()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "VSCD";
			vessel.RV_LloydsNumber = "123456";
			vessel.RV_RadioCallSign = "654321";
			Factory.Save();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(transportMeans.ArrivalDateTime, NUnit.Framework.Is.EqualTo(new ZDate(2019, 8, 20)), "ArrivalDateTime");
				NUnit.Framework.Assert.That(transportMeans.TypeCode, NUnit.Framework.Is.EqualTo(BorderTransportMeansTypeCodes._4).Using(CustomComparers.TypeComparison), "TypeCode");
				AssertContainsExactElementsInExactOrder("ItineraryRoutingCountryCodes", new[] { "TW", "US" }, transportMeans.ItineraryRoutingCountryCodes);
				NUnit.Framework.Assert.That(transportMeans.ID, NUnit.Framework.Is.EqualTo("CX 100").Using(CustomComparers.TypeComparison), "ID (AIR)");
				NUnit.Framework.Assert.That(transportMeans.JourneyID, NUnit.Framework.Is.EqualTo("CX100").Using(CustomComparers.TypeComparison), "JourneyID (AIR)");
				NUnit.Framework.Assert.That(transportMeans.Registration, NUnit.Framework.Is.EqualTo("212").Using(CustomComparers.TypeComparison), "Registration (AIR)");
				NUnit.Framework.Assert.That(transportMeans.Name.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Name - should be [null] or [empty]");
				NUnit.Framework.Assert.That(transportMeans.CallSignID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "CallSignID - should be [null] or [empty]");

				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				declaration.JE_VesselName = "VSCD";
				declaration.JE_VoyageFlightNo = "CX101";
				declaration.JE_VesselArrivalReg = "213";
				NUnit.Framework.Assert.That(transportMeans.TypeCode, NUnit.Framework.Is.EqualTo(BorderTransportMeansTypeCodes._1).Using(CustomComparers.TypeComparison), "TypeCode for Sea");
				NUnit.Framework.Assert.That(transportMeans.ID, NUnit.Framework.Is.EqualTo("654321").Using(CustomComparers.TypeComparison), "ID (SEA)");
				NUnit.Framework.Assert.That(transportMeans.JourneyID, NUnit.Framework.Is.EqualTo("CX101").Using(CustomComparers.TypeComparison), "JourneyID (SEA)");
				NUnit.Framework.Assert.That(transportMeans.Registration, NUnit.Framework.Is.EqualTo("213").Using(CustomComparers.TypeComparison), "Registration (SEA)");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_DateOfArrival = new ZDateTime(2019, 8, 20);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_VoyageFlightNo = "CX100";
			declaration.JE_VesselArrivalReg = "212";
			var itinerary1 = declaration.Itineraries.AddNew();
			itinerary1.CY_Code = "TW";
			var itinerary2 = declaration.Itineraries.AddNew();
			itinerary2.CY_Code = "US";
			transportMeans = new LicensingMessageTransportMeans(declaration);
		}

		JobDeclaration declaration;
		LicensingMessageTransportMeans transportMeans;
	}
}
