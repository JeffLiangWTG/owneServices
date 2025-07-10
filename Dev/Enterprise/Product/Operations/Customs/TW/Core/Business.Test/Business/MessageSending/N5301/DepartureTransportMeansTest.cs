using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5301;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class DepartureTransportMeansTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestArrivalDateTime()
		{
			NUnit.Framework.Assert.That(DepartureTransportMeans.ArrivalDateTime, NUnit.Framework.Is.EqualTo(ZDateTime.Empty).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTypeCode()
		{
			NUnit.Framework.Assert.That(DepartureTransportMeans.TypeCode.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestItineraryRoutingCountryCodes()
		{
			NUnit.Framework.Assert.That(DepartureTransportMeans.ItineraryRoutingCountryCodes, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<ZString>)));
		}

		[ExpectNoExceptions]
		public void TestID()
		{
			var vessel1 = Factory.New<RefVessel>();
			vessel1.RV_Code = "VESSEL 1";
			vessel1.RV_LloydsNumber = "8811928";
			vessel1.RV_RadioCallSign = "8811927";
			vessel1.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Taiwan;

			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Code = "VESSEL 2";
			vessel2.RV_RadioCallSign = "8811924";
			vessel2.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Taiwan;

			var vessel3 = Factory.New<RefVessel>();
			vessel3.RV_Code = "VESSEL 3";
			vessel3.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Taiwan;

			moveHeader.BM_ConveyanceNumber = "XXX333";
			moveHeader.BM_ExportLadenOn = "VESSEL 1";
			NUnit.Framework.Assert.That(DepartureTransportMeans.ID, NUnit.Framework.Is.EqualTo("8811928").Using(CustomComparers.TypeComparison));

			moveHeader.BM_ExportLadenOn = "VESSEL 2";
			NUnit.Framework.Assert.That(DepartureTransportMeans.ID, NUnit.Framework.Is.EqualTo("8811924").Using(CustomComparers.TypeComparison));

			moveHeader.BM_ExportLadenOn = "VESSEL 3";
			NUnit.Framework.Assert.That(DepartureTransportMeans.ID, NUnit.Framework.Is.EqualTo("NIL").Using(CustomComparers.TypeComparison));

			moveHeader.BM_ExportLadenOn = ZString.Empty;
			NUnit.Framework.Assert.That(DepartureTransportMeans.ID, NUnit.Framework.Is.EqualTo("XXX333").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestJourneyID()
		{
			moveHeader.BM_ConveyanceNumber = "XXX333";
			NUnit.Framework.Assert.That(DepartureTransportMeans.JourneyID, NUnit.Framework.Is.EqualTo("XXX333").Using(CustomComparers.TypeComparison));

			moveHeader.BM_ConveyanceNumber = ZString.Empty;
			NUnit.Framework.Assert.That(DepartureTransportMeans.JourneyID, NUnit.Framework.Is.EqualTo("NIL").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestRegistration()
		{
			moveHeader.BM_TransportAtDeparture = "23";
			NUnit.Framework.Assert.That(DepartureTransportMeans.Registration, NUnit.Framework.Is.EqualTo("23").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestName()
		{
			moveHeader.BM_ExportLadenOn = "VESSEL11";
			NUnit.Framework.Assert.That(DepartureTransportMeans.Name, NUnit.Framework.Is.EqualTo("VESSEL11").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCallSignID()
		{
			NUnit.Framework.Assert.That(DepartureTransportMeans.CallSignID.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			var arrivalBill = header.ArrivalBill;
			var movementBill = header.MovementBill;
			moveHeader = header.MovementHeader;
			var moveDetail = moveHeader.InBondMoveDetail;
			var moveLine = moveDetail.InBondMoveLineItem;
		}

		CusInBondMoveHeader moveHeader;
		ITransportMeans DepartureTransportMeans => new DepartureTransportMeans(moveHeader);
	}
}
