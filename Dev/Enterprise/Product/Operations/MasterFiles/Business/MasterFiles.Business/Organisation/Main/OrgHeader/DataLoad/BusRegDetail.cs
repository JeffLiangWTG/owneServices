using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class BusRegDetail
	{
		public BusRegDetail(ZString busRegDetails)
		{
			CountryOfIssue = ZString.Empty;
			CodeType = ZString.Empty;
			RegistrationNumber = ZString.Empty;
			int index = busRegDetails.IndexOf("~");
			if (index > 0)
			{
				CountryOfIssue = busRegDetails.Left(index);
				busRegDetails = busRegDetails.SubstringSafe(index + 1);
				index = busRegDetails.IndexOf("~");

				if (index > 0)
				{
					CodeType = busRegDetails.Left(index);
					RegistrationNumber = busRegDetails.SubstringSafe(index + 1);
				}
			}
		}

		public bool Validate(ZString orgCode, ZString orgName, ZString value, ZString headerColumn, out ZString errorMsg)
		{
			errorMsg = ZString.Empty;
			if (CodeType.IsEmpty || CodeType.IsEmpty)
			{
				errorMsg = Res.GetString("a5a3bdbb-41d4-49de-9ca9-1061b1631715", "Organization [Code: {0}, Name: {1}] has an invalid registration detail ({2}) in column '{3}'; valid registration detail should be in the following format: Country/Region Of Issue~Registration Type~Registration Number e.g. US~MID~MID123456",
					orgCode, orgName, value, headerColumn);
				return false;
			}
			else if (CodeType.Length > OrgCusCodeSchema.OK_CodeType.MaxLength)
			{
				errorMsg = Res.GetString("68c6f5e4-f3f8-403a-a0ba-4183e94af0c2", "Organization [Code: {0}, Name: {1}] has an invalid code type ({2}) in column '{3}': the code type is too long. Please ensure that it's only {4} characters long.",
					orgCode, orgName, CodeType, headerColumn, OrgCusCodeSchema.OK_CodeType.MaxLength);
				return false;
			}
			else if (CountryOfIssue.Length > OrgCusCodeSchema.OK_RN_NKCodeCountry.MaxLength)
			{
				errorMsg = Res.GetString("e903c8dd-0c02-4494-85b7-6e401e1bab9b", "Organization [Code: {0}, Name: {1}] has an invalid country/region code ({2}) in column '{3}': country/region code too long. Please ensure that it's only {4} characters long.",
					orgCode, orgName, CountryOfIssue, headerColumn, OrgCusCodeSchema.OK_RN_NKCodeCountry.MaxLength);
				return false;
			}
			else if (RegistrationNumber.Length > OrgCusCodeSchema.OK_CustomsRegNo.MaxLength)
			{
				errorMsg = Res.GetString("5371b851-cc8a-4fc0-856d-22025d1759d2", "Organization [Code: {0}, Name: {1}] has an invalid customs registration number ({2}) in column '{3}': registration number too long. Please ensure that it's only {4} characters long.",
					orgCode, orgName, RegistrationNumber, headerColumn, OrgCusCodeSchema.OK_CustomsRegNo.MaxLength);
				return false;
			}
			return true;
		}

		public ZString CountryOfIssue;
		public ZString CodeType;
		public ZString RegistrationNumber;
	}
}
