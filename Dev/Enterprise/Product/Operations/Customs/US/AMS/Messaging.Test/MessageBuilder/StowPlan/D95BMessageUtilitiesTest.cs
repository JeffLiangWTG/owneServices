using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.D95B.Elements;
using Enterprise.Edifact.D95B.Segments;

namespace Enterprise.Customs.US.AMS.Messaging.Business.StowPlan.Testing
{
	class D95BMessageUtilitiesTest : TestCaseWithFactory
	{
		public void TestPopulateLOC()
		{
			var loc = new LOCSegment();
			D95BMessageUtilities.PopulateLOC(loc, PlaceLocationQualifierList.BaseportOfDischarge, "XXX", CodeListQualifierList.AirCarrier, CodeListResponsibleAgencyCodedList.UnEceUnitedNationsEconomicCommissionForEurope);
			AssertEquals(PlaceLocationQualifierList.BaseportOfDischarge, loc.PlaceLocationQualifier);
			AssertEquals("XXX", loc.LocationIdentification.PlaceLocationIdentification);
			AssertEquals(CodeListQualifierList.AirCarrier, loc.LocationIdentification.CodeListQualifier);
			AssertEquals(CodeListResponsibleAgencyCodedList.UnEceUnitedNationsEconomicCommissionForEurope, loc.LocationIdentification.CodeListResponsibleAgencyCoded);
		}

		public void TestPopulateDTM()
		{
			var dtm = new DTMSegment();
			D95BMessageUtilities.PopulateDTM(dtm, DateTimePeriodQualifierList.ArrivalDateTimeActual, new ZDateTime(2012, 10, 30));
			AssertEquals(DateTimePeriodQualifierList.ArrivalDateTimeActual, dtm.DateTimePeriod.DateTimePeriodQualifier);
			AssertEquals("121030", dtm.DateTimePeriod.DateTimePeriod);
			AssertEquals(DateTimePeriodFormatQualifierList.Yymmdd, dtm.DateTimePeriod.DateTimePeriodFormatQualifier);
		}
	}
}
