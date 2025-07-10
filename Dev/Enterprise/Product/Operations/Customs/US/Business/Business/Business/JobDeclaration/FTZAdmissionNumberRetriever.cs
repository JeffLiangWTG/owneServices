using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public static class FTZAdmissionNumberRetriever
	{
		internal static ZString GetFTZZoneID(ZString admissionNumber)
		{
			return GetAdmissionNumberComponents(admissionNumber)[0];
		}

		static ZString[] GetAdmissionNumberComponents(ZString admissionNumber)
		{
			return admissionNumber.Split('|');
		}

		internal static ZString GetFTZYear(ZString admissionNumber)
		{
			var components = GetAdmissionNumberComponents(admissionNumber);
			return components.Length > 1 ? components[1] : ZString.Empty;
		}

		internal static ZString GetFTZControlNumber(ZString admissionNumber)
		{
			var components = GetAdmissionNumberComponents(admissionNumber);
			return components.Length > 2 ? components[2] : ZString.Empty;
		}

		internal static void SetFTZZoneID(JobDeclaration declaration, ZString zoneID)
		{
			SetFTZAdmissionNumber(declaration, zoneID, declaration.FTZYear, declaration.FTZControlNumber);
		}

		internal static void SetFTZYear(JobDeclaration declaration, ZString year)
		{
			SetFTZAdmissionNumber(declaration, declaration.FTZZoneID, year, declaration.FTZControlNumber);
		}

		internal static void SetFTZControlNumber(JobDeclaration declaration, ZString controlNumber)
		{
			SetFTZAdmissionNumber(declaration, declaration.FTZZoneID, declaration.FTZYear, controlNumber);
		}

		static void SetFTZAdmissionNumber(JobDeclaration declaration, ZString zoneID, ZString year, ZString controlNumber)
		{
			var number = GetFormattedFTZAdmissionNumber(zoneID, year, controlNumber);
			declaration.FTZAdmissionNumber = number == "||" ? "" : number;
		}

		internal static string GetFormattedFTZAdmissionNumber(ZString zoneID, ZString year, ZString controlNumber)
		{
			return zoneID + FTZAdmissionNumberSeparator + year + FTZAdmissionNumberSeparator + controlNumber;
		}

		public static ZString FTZAdmissionNumberFormatted(ZString admissionNumber)
		{
			return admissionNumber.ExcludeChars(FTZAdmissionNumberSeparator);
		}
		public const string FTZAdmissionNumberSeparator = "|";

		internal static ZString GetFTZNumberFromString(ZString number)
		{
			var isNewFormat = number.Length == JobDeclaration.Schema.FTZZoneIDMaxLength + JobDeclaration.Schema.FTZYearMaxLength + JobDeclaration.Schema.FTZControlNumberMaxLength;
			var zoneIDMaxLength = isNewFormat ? JobDeclaration.Schema.FTZZoneIDMaxLength : JobDeclaration.Schema.OldFTZZoneIDMaxLength;
			return number.IsEmpty ? "" : number.SubstringSafe(0, zoneIDMaxLength) + FTZAdmissionNumberSeparator + number.SubstringSafe(zoneIDMaxLength, JobDeclaration.Schema.FTZYearMaxLength) + FTZAdmissionNumberSeparator + number.SubstringSafe(zoneIDMaxLength + JobDeclaration.Schema.FTZYearMaxLength);
		}
	}
}
