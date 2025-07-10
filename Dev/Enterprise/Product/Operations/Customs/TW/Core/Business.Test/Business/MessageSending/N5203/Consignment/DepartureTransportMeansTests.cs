using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class DepartureTransportMeansTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestArrivalDateTime()
		{
			NUnit.Framework.Assert.That(departureTransportMeans.ArrivalDateTime, NUnit.Framework.Is.EqualTo(ZDate.Empty));
		}

		[ExpectNoExceptions]
		public void TestTypeCode()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			NUnit.Framework.Assert.That(departureTransportMeans.TypeCode, NUnit.Framework.Is.EqualTo(TransportCodeList.Codes.SeaPackedSundryGoods).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestItineraryRoutingCountryCodes()
		{
			NUnit.Framework.Assert.That(departureTransportMeans.ItineraryRoutingCountryCodes, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<CargoWise.Types.ZString>)));
		}

		[ExpectNoExceptions]
		public void TestID()
		{
			NUnit.Framework.Assert.That(departureTransportMeans.ID, NUnit.Framework.Is.EqualTo("0982432").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestJourneyID()
		{
			NUnit.Framework.Assert.That(departureTransportMeans.JourneyID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestRegistration()
		{
			NUnit.Framework.Assert.That(departureTransportMeans.Registration, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestCallSignID()
		{
			NUnit.Framework.Assert.That(departureTransportMeans.CallSignID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestName()
		{
			NUnit.Framework.Assert.That(departureTransportMeans.Name, NUnit.Framework.Is.EqualTo("VESSELNAME").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var testHelper = new TestTWCreator(Factory);
			var entryHeader = testHelper.CreateEntryHeaderForN5203();
			testHelper.CreateRefVessel();
			declaration = entryHeader.Declaration;
			declaration.JE_VesselName = "VESSELNAME";
			departureTransportMeans = new Consignment(entryHeader).DepartureTransportMeans;
		}

		JobDeclaration declaration;
		ITransportMeans departureTransportMeans;
	}
}
