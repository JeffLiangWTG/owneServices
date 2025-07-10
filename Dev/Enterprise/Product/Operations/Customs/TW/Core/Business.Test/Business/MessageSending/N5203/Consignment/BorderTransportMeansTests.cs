using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(BorderTransportMeans))]
	sealed class BorderTransportMeansTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestArrivalDateTime()
		{
			NUnit.Framework.Assert.That(borderTransportMeans.ArrivalDateTime, NUnit.Framework.Is.EqualTo(ZDateTime.Empty).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTypeCode()
		{
			NUnit.Framework.Assert.That(borderTransportMeans.TypeCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestItineraryRoutingCountryCodes()
		{
			NUnit.Framework.Assert.That(borderTransportMeans.ItineraryRoutingCountryCodes, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<CargoWise.Types.ZString>)));
		}

		[ExpectNoExceptions]
		public void TestID()
		{
			NUnit.Framework.Assert.That(borderTransportMeans.ID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestJourneyID()
		{
			declaration.JE_VoyageFlightNo = "CX100";
			NUnit.Framework.Assert.That(borderTransportMeans.JourneyID, NUnit.Framework.Is.EqualTo(SharedHelper.GetVoyageFlightNo(declaration)));
		}

		[ExpectNoExceptions]
		public void TestRegistration()
		{
			declaration.JE_VesselArrivalReg = "X123";
			NUnit.Framework.Assert.That(borderTransportMeans.Registration, NUnit.Framework.Is.EqualTo(declaration.JE_VesselArrivalReg));
		}

		[ExpectNoExceptions]
		public void TestCallSignID()
		{
			NUnit.Framework.Assert.That(borderTransportMeans.CallSignID, NUnit.Framework.Is.EqualTo("X232").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestName()
		{
			NUnit.Framework.Assert.That(borderTransportMeans.Name, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var testHelper = new TestTWCreator(Factory);
			var entryHeader = testHelper.CreateEntryHeaderForN5203();
			testHelper.CreateRefVessel();
			declaration = entryHeader.Declaration;
			declaration.JE_VesselName = "VESSELNAME";
			borderTransportMeans = new Consignment(entryHeader).BorderTransportMeans;
		}

		ITransportMeans borderTransportMeans;
		JobDeclaration declaration;
	}
}
