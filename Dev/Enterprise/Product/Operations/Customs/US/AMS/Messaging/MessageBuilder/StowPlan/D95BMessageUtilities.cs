using CargoWise.Types;
using Enterprise.Edifact.D95B.Elements;
using Enterprise.Edifact.D95B.Segments;

namespace Enterprise.Customs.US.AMS.Messaging.Business.StowPlan
{
	static class D95BMessageUtilities
	{
		public static void PopulateLOC(LOCSegment loc, PlaceLocationQualifierList placeLocationQualifier, string placeLocationIdentification, CodeListQualifierList codeListQualifier, CodeListResponsibleAgencyCodedList codeListResponsibleAgencyCodedList)
		{
			loc.PlaceLocationQualifier = placeLocationQualifier;
			loc.LocationIdentification.PlaceLocationIdentification = placeLocationIdentification;
			loc.LocationIdentification.CodeListQualifier = codeListQualifier;
			loc.LocationIdentification.CodeListResponsibleAgencyCoded = codeListResponsibleAgencyCodedList;
		}

		public static void PopulateDTM(DTMSegment dtm, DateTimePeriodQualifierList dateTimePeriodQualifierList, ZDateTime date)
		{
			dtm.DateTimePeriod.DateTimePeriodQualifier = dateTimePeriodQualifierList;
			dtm.DateTimePeriod.DateTimePeriod = date.IsValid ? date.ToString("yyMMdd") : string.Empty;
			dtm.DateTimePeriod.DateTimePeriodFormatQualifier = DateTimePeriodFormatQualifierList.Yymmdd;
		}
	}
}
