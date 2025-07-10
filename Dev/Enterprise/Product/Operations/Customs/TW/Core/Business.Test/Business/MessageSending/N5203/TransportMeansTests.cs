using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TransportMeansTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestArrivalDateTime()
		{
			NUnit.Framework.Assert.That(TransportMeans.ArrivalDateTime, NUnit.Framework.Is.EqualTo(ZDate.Empty));
		}

		[ExpectNoExceptions]
		public void TestTypeCode()
		{
			var declaration = entryHeader.Declaration;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			NUnit.Framework.Assert.That(TransportMeans.TypeCode, NUnit.Framework.Is.EqualTo("4").Using(CustomComparers.TypeComparison));
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			NUnit.Framework.Assert.That(TransportMeans.TypeCode, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));
		}

		public void TestItineraryRoutingCountryCodes()
		{
			var declaration = entryHeader.Declaration;
			var itinerary1 = declaration.Itineraries.AddNew();
			itinerary1.CY_Code = Core.Constants.CountryCodes.Australia;
			var itinerary2 = declaration.Itineraries.AddNew();
			itinerary2.CY_Code = Core.Constants.CountryCodes.UnitedStates;
			AssertContainsExactElementsInExactOrder(new[] { Core.Constants.CountryCodes.Australia, Core.Constants.CountryCodes.UnitedStates }, TransportMeans.ItineraryRoutingCountryCodes);
		}

		[ExpectNoExceptions]
		public void TestID()
		{
			NUnit.Framework.Assert.That(TransportMeans.ID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestJourneyID()
		{
			NUnit.Framework.Assert.That(TransportMeans.JourneyID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestRegistration()
		{
			NUnit.Framework.Assert.That(TransportMeans.Registration, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestName()
		{
			NUnit.Framework.Assert.That(TransportMeans.Name, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestCallSignID()
		{
			NUnit.Framework.Assert.That(TransportMeans.CallSignID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var testHelper = new TestTWCreator(Factory);
			entryHeader = testHelper.CreateEntryHeaderForN5203();
		}

		CusEntryHeader entryHeader;
		ITransportMeans TransportMeans => new TransportMeans(entryHeader);
	}
}
