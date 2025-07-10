using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	class FTZAdmissionNumberRetrieverTest : TestCaseWithFactory
	{
		[TestDate(2015, 1, 1)]
		public void TestFTZAdmissionNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.FTZZoneID = "1530A01";
			AssertEquals("Zone ID", "1530A01", FTZAdmissionNumberRetriever.GetFTZZoneID(declaration.FTZAdmissionNumber));
			FTZAdmissionNumberRetriever.SetFTZZoneID(declaration, "1420102");
			AssertEquals("Admission Number", "1420102|15|", declaration.FTZAdmissionNumber);
			AssertEquals("Zone ID", "1420102", declaration.FTZZoneID);

			declaration.FTZControlNumber = "00000004";
			AssertEquals("Control Number", "00000004", FTZAdmissionNumberRetriever.GetFTZControlNumber(declaration.FTZAdmissionNumber));
			FTZAdmissionNumberRetriever.SetFTZControlNumber(declaration, "00000006");
			AssertEquals("Control Number", "00000006", declaration.FTZControlNumber);
			AssertEquals("Admission Number", "1420102|15|00000006", declaration.FTZAdmissionNumber);
			AssertEquals("Admission Number Formatted", "14201021500000006", FTZAdmissionNumberRetriever.FTZAdmissionNumberFormatted(declaration.FTZAdmissionNumber));

			declaration.FTZControlNumber = ZString.Empty;
			AssertEquals("Control Number", ZString.Empty, FTZAdmissionNumberRetriever.GetFTZControlNumber(declaration.FTZAdmissionNumber));
			AssertEquals("ForeignTradeZone", "1420102|15|", declaration.FTZAdmissionNumber);
			AssertEquals("Zone ID", "1420102", declaration.FTZZoneID);
			AssertEquals("Year", "15", declaration.FTZYear);

			declaration.FTZZoneID = ZString.Empty;
			AssertEquals("Zone ID", ZString.Empty, FTZAdmissionNumberRetriever.GetFTZZoneID(declaration.FTZAdmissionNumber));
			AssertEquals("Year", "15", FTZAdmissionNumberRetriever.GetFTZYear(declaration.FTZAdmissionNumber));

			FTZAdmissionNumberRetriever.SetFTZYear(declaration, "14");
			AssertEquals("Year", "14", declaration.FTZYear);

			AssertEquals("Zone ID", ZString.Empty, FTZAdmissionNumberRetriever.GetFTZZoneID(""));
			AssertEquals("Year", ZString.Empty, FTZAdmissionNumberRetriever.GetFTZYear(""));
			AssertEquals("Control Number", ZString.Empty, FTZAdmissionNumberRetriever.GetFTZControlNumber(""));
			AssertEquals("Admission Number", ZString.Empty, FTZAdmissionNumberRetriever.FTZAdmissionNumberFormatted(""));
			AssertEquals("Admission Number", ZString.Empty, FTZAdmissionNumberRetriever.GetFTZNumberFromString(""));
			AssertEquals("Admission Number", "1450102|15|00000004", FTZAdmissionNumberRetriever.GetFTZNumberFromString("14501021500000004"));
			AssertEquals("Admission Number", "145001002|15|00000004", FTZAdmissionNumberRetriever.GetFTZNumberFromString("1450010021500000004"));
			AssertEquals("Formatted Admission Number", "1450102|15|00000004", FTZAdmissionNumberRetriever.GetFormattedFTZAdmissionNumber("1450102", "15", "00000004"));
			AssertEquals("Formatted Admission Number", "145001002|15|00000004", FTZAdmissionNumberRetriever.GetFormattedFTZAdmissionNumber("145001002", "15", "00000004"));
		}
	}
}
