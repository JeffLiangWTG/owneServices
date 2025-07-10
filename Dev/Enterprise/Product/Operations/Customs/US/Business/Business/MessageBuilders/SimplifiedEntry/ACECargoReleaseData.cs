using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public static class ACECargoReleaseData
	{
		public static ZString GetCustomsNumberType(ZString customsNumber)
		{
			var result = ZString.Empty;
			if (!customsNumber.IsEmpty)
			{
				if (EmployerIdentificationNumberValidator.IsValidEIN(customsNumber))
				{
					result = EntityIdentifierQualifierList.Codes.EmployerIdentificationNumber;
				}
				else if (CBPAssignedNumberValidator.IsValidCBPAssignedNumber(customsNumber))
				{
					result = EntityIdentifierQualifierList.Codes.CBPAssignedNumber;
				}
				else if (SocialSecurityNumberValidator.IsValidSSN(customsNumber))
				{
					result = EntityIdentifierQualifierList.Codes.SocialSecurityNumber;
				}
			}
			return result;
		}

		public static ZString GetEntityCustomsNumberType(ZString customsNumber)
		{
			var result = ZString.Empty;
			if (!customsNumber.IsEmpty)
			{
				result = GetCustomsNumberType(customsNumber);
				if (result.IsEmpty)
				{
					if (EncryptedConsigneeNumberValidator.IsValidEncryptedNumber(customsNumber))
					{
						result = EntityIdentifierQualifierList.Codes.CBPEncryptedConsigneeID;
					}
					else if (FIRMSCodeValidator.IsValidFIRMS(customsNumber))
					{
						result = EntityIdentifierQualifierList.Codes.FIRMS;
					}
				}
			}
			return result;
		}

		public static void AddEntity(List<ISimplifiedEntryOrganisationDetails> result, ZPropertyInfo info, ZString entityCode)
		{
			ISimplifiedEntryOrganisationDetails details = OrganisationDetails.New(info);
			if (details != null)
			{
				details.EntityCode = entityCode;
				result.Add(details);
			}
		}
	}
}
