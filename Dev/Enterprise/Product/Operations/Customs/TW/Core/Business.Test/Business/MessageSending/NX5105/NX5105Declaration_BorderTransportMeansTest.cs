using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.TW;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX5105Declaration_BorderTransportMeans))]
	sealed class NX5105Declaration_BorderTransportMeansTest : TestCaseWithFactory
	{
		[TestDate(2019, 01, 01)]
		public void TestDeclaration_BorderTransportMeans()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_DateForDuty = ZDateTime.Today.AddDays(1);
			entryInstruction.CEI_Style = "AA";
			declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(2);
			declaration.JE_DateAtFinalDestination = ZDateTime.Today.AddDays(3);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = ContainerModeList.Codes.BreakBulk;
			var itinerary1 = declaration.Itineraries.AddNew();
			itinerary1.CY_Code = Core.Constants.CountryCodes.Australia;
			var itinerary2 = declaration.Itineraries.AddNew();
			itinerary2.CY_Code = Core.Constants.CountryCodes.UnitedStates;
			ITransportMeans borderTransportMeans = new NX5105Declaration_BorderTransportMeans(entryHeader);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(borderTransportMeans.ArrivalDateTime, NUnit.Framework.Is.EqualTo(new ZDate(2019, 01, 03)), "BorderTransportMeans.ArrivalDateTime should be");
				NUnit.Framework.Assert.That(borderTransportMeans.TypeCode, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison), "BorderTransportMeans.TypeCode should be");
			});
			declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(3);
			declaration.JE_DateAtFinalDestination = ZDateTime.Today.AddDays(4);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ContainerMode = ContainerModeList.Codes.Loose;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(borderTransportMeans.ArrivalDateTime, NUnit.Framework.Is.EqualTo(new ZDate(2019, 01, 04)), "BorderTransportMeans.ArrivalDateTime should be");
				NUnit.Framework.Assert.That(borderTransportMeans.TypeCode, NUnit.Framework.Is.EqualTo("4").Using(CustomComparers.TypeComparison), "BorderTransportMeans.TypeCode should be");
			});

			AssertContainsExactElementsInExactOrder(new[] { Core.Constants.CountryCodes.Australia, Core.Constants.CountryCodes.UnitedStates }, borderTransportMeans.ItineraryRoutingCountryCodes);
		}

		public void TestCheckArgumentsNotNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				ITransportMeans transport = new NX5105Declaration_BorderTransportMeans(null);
			});

			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				var entryHeader = Factory.New<CusEntryHeader>();
				ITransportMeans transport = new NX5105Declaration_BorderTransportMeans(entryHeader);
			});

			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				var entryHeader = Factory.New<CusEntryHeader>();
				ITransportMeans transport = new NX5105Declaration_BorderTransportMeans(entryHeader);
			});

			AssertNoExceptionThrown(() =>
			{
				var jobDeclaration = Factory.New<JobDeclaration>();
				var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
				ITransportMeans transport = new NX5105Declaration_BorderTransportMeans(entryHeader);
			});
		}

		[ExpectNoExceptions]
		public void TestCheckNotApplicableProperties()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			ITransportMeans transport = new NX5105Declaration_BorderTransportMeans(entryHeader);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(transport.ID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "ID - should be [null] or [empty]");
				NUnit.Framework.Assert.That(transport.JourneyID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "JourneyID - should be [null] or [empty]");
				NUnit.Framework.Assert.That(transport.Registration.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Registration - should be [null] or [empty]");
				NUnit.Framework.Assert.That(transport.Name.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Name - should be [null] or [empty]");
				NUnit.Framework.Assert.That(transport.CallSignID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "CallSignID - should be [null] or [empty]");
			});
		}
	}
}
