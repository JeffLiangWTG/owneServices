using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX5105Consignment_BorderTransportMeans))]
	sealed class NX5105Consignment_BorderTransportMeansTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestConsignment_BorderTransportMeans()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "VSCD";
			vessel.RV_LloydsNumber = "123456";
			vessel.RV_RadioCallSign = "654321";
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			ITransportMeans borderTransportMeans = new NX5105Consignment_BorderTransportMeans(declaration);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_VesselName = "VSCD";
			NUnit.Framework.Assert.That(borderTransportMeans.ID, NUnit.Framework.Is.EqualTo("654321").Using(CustomComparers.TypeComparison), "GoodsShipment.Consignment.BorderTransportMeans.ID should be");
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_VoyageFlightNo = "CI0008";
			NUnit.Framework.Assert.That(borderTransportMeans.ID, NUnit.Framework.Is.EqualTo(SharedHelper.GetVoyageFlightNo(declaration, false)), "GoodsShipment.Consignment.BorderTransportMeans.ID should be");
			NUnit.Framework.Assert.That(borderTransportMeans.JourneyID, NUnit.Framework.Is.EqualTo(SharedHelper.GetVoyageFlightNo(declaration)), "GoodsShipment.Consignment.BorderTransportMeans.JourneyID should be");
			declaration.JE_VoyageFlightNo = "";
			NUnit.Framework.Assert.That(borderTransportMeans.JourneyID, NUnit.Framework.Is.EqualTo(SharedHelper.GetVoyageFlightNo(declaration)), "GoodsShipment.Consignment.BorderTransportMeans.JourneyID should be");
			declaration.JE_VesselArrivalReg = ZString.Empty;
			NUnit.Framework.Assert.That(borderTransportMeans.Registration, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "GoodsShipment.Consignment.BorderTransportMeans.Registration should be");
			declaration.JE_VesselArrivalReg = "123";
			NUnit.Framework.Assert.That(borderTransportMeans.Registration, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison), "GoodsShipment.Consignment.BorderTransportMeans.Registration should be");
		}

		public void TestCheckArgumentsNotNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				ITransportMeans transport = new NX5105Consignment_BorderTransportMeans(null);
			}

			);
			AssertNoExceptionThrown(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				ITransportMeans transport = new NX5105Consignment_BorderTransportMeans(declaration);
			}

			);
		}

		[ExpectNoExceptions]
		public void TestCheckNotApplicableProperties()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			ITransportMeans transport = new NX5105Consignment_BorderTransportMeans(jobDeclaration);
			NUnit.Framework.Assert.That(transport.ArrivalDateTime, NUnit.Framework.Is.EqualTo(ZDate.Empty));
			NUnit.Framework.Assert.That(transport.TypeCode, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(transport.ItineraryRoutingCountryCodes, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<ZString>)));
			NUnit.Framework.Assert.That(transport.Name, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(transport.CallSignID, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
		}
	}
}
