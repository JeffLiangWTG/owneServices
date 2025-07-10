using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5301;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class BorderTransportMeansTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestArrivalDateTime()
		{
			NUnit.Framework.Assert.That(BorderTransportMeans.ArrivalDateTime, NUnit.Framework.Is.EqualTo(ZDate.BrettsBirthday));
		}

		[ExpectNoExceptions]
		public void TestTypeCode()
		{
			NUnit.Framework.Assert.That(BorderTransportMeans.TypeCode, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestItineraryRoutingCountryCodes()
		{
			NUnit.Framework.Assert.That(BorderTransportMeans.ItineraryRoutingCountryCodes, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<ZString>)));
		}

		[ExpectNoExceptions]
		public void TestID()
		{
			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Code = "VESSEL 2";
			vessel2.RV_RadioCallSign = "8811924";
			vessel2.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Taiwan;

			var vessel3 = Factory.New<RefVessel>();
			vessel3.RV_Code = "VESSEL 3";
			vessel3.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Taiwan;
			NUnit.Framework.Assert.That(BorderTransportMeans.ID, NUnit.Framework.Is.EqualTo("1234567").Using(CustomComparers.TypeComparison));

			header.BH_ImportConveyanceName = "VESSEL 2";
			NUnit.Framework.Assert.That(BorderTransportMeans.ID, NUnit.Framework.Is.EqualTo("8811924").Using(CustomComparers.TypeComparison));

			header.BH_ImportConveyanceName = "VESSEL 3";
			NUnit.Framework.Assert.That(BorderTransportMeans.ID, NUnit.Framework.Is.EqualTo("NIL").Using(CustomComparers.TypeComparison));

			header.BH_ImportConveyanceName = ZString.Empty;
			NUnit.Framework.Assert.That(BorderTransportMeans.ID, NUnit.Framework.Is.EqualTo("X2").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestJourneyID()
		{
			NUnit.Framework.Assert.That(BorderTransportMeans.JourneyID, NUnit.Framework.Is.EqualTo("X2").Using(CustomComparers.TypeComparison));

			header.BH_UniqueVoyageIdentifier = ZString.Empty;
			NUnit.Framework.Assert.That(BorderTransportMeans.JourneyID, NUnit.Framework.Is.EqualTo("NIL").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestRegistration()
		{
			NUnit.Framework.Assert.That(BorderTransportMeans.Registration, NUnit.Framework.Is.EqualTo("X3").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestName()
		{
			NUnit.Framework.Assert.That(BorderTransportMeans.Name, NUnit.Framework.Is.EqualTo("X1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCallSignID()
		{
			NUnit.Framework.Assert.That(BorderTransportMeans.CallSignID.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		protected override void SetUp()
		{
			base.SetUp();
			vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "X1";
			vessel.RV_LloydsNumber = "1234567";
			vessel.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Taiwan;
			header = Factory.NewWithValidTestData<CusInBondHeader>();
			var arrivalBill = header.ArrivalBill;
			var movementBill = header.MovementBill;
			var moveHeader = header.MovementHeader;
			var moveDetail = moveHeader.InBondMoveDetail;
			var moveLine = moveDetail.InBondMoveLineItem;
			header.BH_ETA = ZDateTime.BrettsBirthday;
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.SEA;
			header.BH_ImportConveyanceName = "X1";
			header.BH_UniqueVoyageIdentifier = "X2";
			header.BH_VoyageNumber = "X3";
		}

		ITransportMeans BorderTransportMeans => new BorderTransportMeans(header);
		CusInBondHeader header;
		RefVessel vessel;
	}
}
