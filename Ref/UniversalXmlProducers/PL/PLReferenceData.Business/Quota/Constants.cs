using System;
using System.Globalization;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Quota;

public static class Constants
{
	public const string QuotaUniversalReferenceDataXmlFilename = "PLQuotaData.xml";

	public static DateTime ConstantEndDate => Convert.ToDateTime("2079-06-06T23:59:00", CultureInfo.InvariantCulture);
	public const int ExpiredYears = 5;
}
